namespace KONGOR.MasterServer.Helpers.MessageOfTheDay;

/// <summary>
///     Decides which message of the day items show, and renders the page the client's home page panel loads: a carousel of cards over the game, each opening a full article.
/// </summary>
public static class MessageOfTheDayPage
{
    /// <summary>
    ///     The items that are published and not yet expired at the given moment, newest first.
    /// </summary>
    public static List<MessageOfTheDayItem> Visible(IEnumerable<MessageOfTheDayItem> items, DateTimeOffset now)
        => [.. items.Where(item => item.Published <= now && (item.Expires is null || item.Expires > now)).OrderByDescending(item => item.Published)];

    /// <summary>
    ///     The address of an item's picture, which carries a version of the picture itself, so that a changed picture gets a new address and an address can be cached for good.
    /// </summary>
    public static string PictureAddress(MessageOfTheDayItem item)
        => $"/motd/pictures/{item.ID}?version={Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(item.Picture)))[..12]}";

    /// <summary>
    ///     Renders the page for the given items, in the order given.
    ///     The items travel as JSON inside the page, which the default encoder escapes so that no markup in them can end the script element.
    /// </summary>
    public static string Render(IReadOnlyList<MessageOfTheDayItem> items)
    {
        string data = JsonSerializer.Serialize(items.Select(item => new { title = item.Title, subtitle = item.Subtitle, body = item.Body, picture = PictureAddress(item) }));

        return
        $$$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8" />
                <title>Message Of The Day</title>
                <style>
                    @font-face { font-family: "Ubuntu Sans"; src: url("/fonts/ubuntu-regular.ttf"); font-weight: 400; }
                    @font-face { font-family: "Ubuntu Sans"; src: url("/fonts/ubuntu-medium.ttf"); font-weight: 500; }
                    @font-face { font-family: "Ubuntu Sans"; src: url("/fonts/ubuntu-bold.ttf"); font-weight: 700; }
                    @font-face { font-family: "Ubuntu Sans Mono"; src: url("/fonts/ubuntu-mono-regular.ttf"); font-weight: 400; }
                    @font-face { font-family: "Ubuntu Sans Mono"; src: url("/fonts/ubuntu-mono-bold.ttf"); font-weight: 700; }
                    * { margin: 0; padding: 0; box-sizing: border-box; }
                    html, body { width: 100%; height: 100%; overflow: hidden; background: transparent; font-family: "Ubuntu Sans", sans-serif; color: #e8d4b0; user-select: none; }
                    .frame { position: relative; width: 100%; height: 100%; }
                    .viewport { position: absolute; inset: 6% 7% 12% 7%; }
                    .track { position: absolute; inset: 0; display: flex; }
                    .slot { flex: 0 0 33.33%; }
                    .card { position: relative; flex: 0 0 33.33%; border-radius: 6px; overflow: hidden; background: #241210; border: 1px solid #5a3d24; box-shadow: 0 4px 14px rgba(0, 0, 0, 0.6); cursor: pointer; transform: scale(0.84); opacity: 0.55; transition: transform 0.4s, opacity 0.4s; }
                    .card.centre { transform: scale(1.04); opacity: 1; z-index: 1; border-color: #c8a060; box-shadow: 0 6px 24px rgba(0, 0, 0, 0.8); }
                    .card:hover { opacity: 0.85; }
                    .card.centre:hover { opacity: 1; border-color: #f0c878; }
                    .picture { position: absolute; inset: 0; background-size: cover; background-position: center; }
                    .words { position: absolute; left: 0; right: 0; bottom: 0; padding: 18% 6% 5%; background: linear-gradient(transparent, rgba(10, 4, 3, 0.92) 45%); }
                    .words h2 { margin-bottom: 0.25em; font-size: 1.5vw; font-weight: 700; color: #f4dfb4; text-shadow: 1px 1px 3px #000; }
                    .words p { font-size: 1.05vw; color: #c8ac84; }
                    .arrow { position: absolute; top: 33%; width: 5.5%; height: 22%; color: #f0d090; display: flex; align-items: center; justify-content: center; cursor: pointer; z-index: 2; filter: drop-shadow(0 0 3px rgba(0, 0, 0, 0.95)) drop-shadow(0 2px 6px rgba(0, 0, 0, 0.8)); transition: color 0.2s, transform 0.2s; }
                    .arrow svg { width: 100%; height: 100%; }
                    .arrow:hover { color: #fff0c8; transform: scale(1.12); }
                    .arrow.left { left: 0.5%; }
                    .arrow.right { right: 0.5%; }
                    .dots { position: absolute; bottom: 4%; left: 0; right: 0; display: flex; justify-content: center; gap: 0.6vw; }
                    .dots span { width: 0.6vw; height: 0.6vw; border-radius: 50%; background: rgba(90, 61, 36, 0.9); box-shadow: 0 0 3px rgba(0, 0, 0, 0.9); transition: background 0.3s; }
                    .dots span.on { background: #e0b060; }
                    .hidden { display: none; }
                    .article { position: absolute; inset: 0; display: none; flex-direction: column; z-index: 5; background: #160807; border-radius: 6px; }
                    .article.open { display: flex; }
                    .back { position: absolute; top: 5%; left: 2%; z-index: 2; display: flex; align-items: center; gap: 0.4em; padding: 0.5vw 1.4vw 0.5vw 0.8vw; border-radius: 999px; background: var(--back-background, rgba(20, 8, 6, 0.6)); border: 2px solid var(--back-border, #8a6a40); color: var(--back-text, #f0d090); font-size: 1.3vw; font-weight: 500; cursor: pointer; transition: filter 0.2s; }
                    .back svg { width: 1.3em; height: 1.3em; }
                    .back:hover { filter: brightness(1.35); }
                    .scroll { flex: 1 1 auto; overflow-y: auto; padding: 7% 0 4%; }
                    .column { width: 50%; margin: 0 auto; }
                    .heading { padding-bottom: 1.2em; margin-bottom: 1.2em; border-bottom: 1px solid var(--back-border, #5a3d24); }
                    .heading h1 { margin-bottom: 0.2em; font-size: 2.6vw; line-height: 1.15; font-weight: 700; color: #f4dfb4; text-shadow: 1px 1px 4px rgba(0, 0, 0, 0.6); }
                    .heading p { font-size: 1.4vw; color: #d0b890; }
                    .body { font-size: 1.3vw; line-height: 1.65; color: #ead6b4; user-select: text; }
                    .body p, .body ul, .body ol, .body pre { margin-bottom: 0.8em; }
                    .body ul, .body ol { padding-left: 1.4em; }
                    .body code, .body pre { font-family: "Ubuntu Sans Mono", monospace; font-size: 0.92em; }
                    .body code { padding: 0.1em 0.35em; border-radius: 4px; background: rgba(0, 0, 0, 0.35); }
                    .body pre { padding: 0.8em 1em; border-radius: 6px; background: rgba(0, 0, 0, 0.4); border: 1px solid rgba(138, 106, 64, 0.35); white-space: pre-wrap; overflow-wrap: anywhere; }
                    .body pre code { padding: 0; background: none; font-size: 1em; }
                    .body a { color: #6ad0f5; text-decoration: none; }
                    .body a:hover { text-decoration: underline; }
                </style>
            </head>
            <body>
                <div class="frame">
                    <div class="arrow left"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><polyline points="15 4 7 12 15 20"/></svg></div>
                    <div class="viewport"><div class="track"></div></div>
                    <div class="arrow right"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 4 17 12 9 20"/></svg></div>
                    <div class="dots"></div>
                    <div class="article">
                        <div class="back"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><polyline points="15 4 7 12 15 20"/></svg>Back</div>
                        <div class="scroll"><div class="column">
                            <div class="heading"><h1></h1><p></p></div>
                            <div class="body"></div>
                        </div></div>
                    </div>
                </div>
                <script type="application/json" id="motd-items">{{{data}}}</script>
                <script>
                    (function () {
                        var items = JSON.parse(document.getElementById('motd-items').textContent);
                        var frame = document.querySelector('.frame');
                        var track = frame.querySelector('.track');
                        var dots = frame.querySelector('.dots');
                        var arrows = frame.querySelectorAll('.arrow');
                        var article = frame.querySelector('.article');
                        var centre = 0;
                        var hovering = false;

                        // Nothing At All Shows Without Items, And Arrows And Dots Only Show When There Is Something To Move To
                        if (items.length === 0) {
                            frame.classList.add('hidden');
                            return;
                        }

                        if (items.length === 1) {
                            arrows.forEach(function (arrow) { arrow.classList.add('hidden'); });
                            dots.classList.add('hidden');
                        }

                        // The Average Colours Of The Picture's Upper And Lower Halves, Darkened, Become The Article's Gradient And Its Back Button
                        function colourArticle(address) {
                            var picture = new Image();
                            picture.onload = function () {
                                var canvas = document.createElement('canvas');
                                canvas.width = 8;
                                canvas.height = 8;
                                var context = canvas.getContext('2d');
                                context.drawImage(picture, 0, 0, 8, 8);
                                var pixels = context.getImageData(0, 0, 8, 8).data;
                                function average(fromRow, toRow) {
                                    var red = 0, green = 0, blue = 0, count = 0;
                                    for (var row = fromRow; row < toRow; row++) {
                                        for (var column = 0; column < 8; column++) {
                                            var index = (row * 8 + column) * 4;
                                            red += pixels[index];
                                            green += pixels[index + 1];
                                            blue += pixels[index + 2];
                                            count++;
                                        }
                                    }
                                    return [red / count, green / count, blue / count];
                                }
                                function darken(colour, factor) { return colour.map(function (value) { return Math.round(value * factor); }); }
                                function lighten(colour, amount) { return colour.map(function (value) { return Math.round(value + (255 - value) * amount); }); }
                                function css(colour) { return 'rgb(' + colour.join(',') + ')'; }
                                var upper = average(0, 4);
                                var lower = average(4, 8);
                                article.style.background = 'linear-gradient(160deg, ' + css(darken(upper, 0.42)) + ' 0%, ' + css(darken(lower, 0.30)) + ' 55%, #0e0605 100%)';
                                article.style.setProperty('--back-background', css(darken(upper, 0.28)));
                                article.style.setProperty('--back-border', css(lighten(darken(upper, 0.9), 0.15)));
                                article.style.setProperty('--back-text', css(lighten(upper, 0.7)));
                            };
                            picture.src = address;
                        }

                        function openArticle(item) {
                            article.style.background = '';
                            article.querySelector('h1').textContent = item.title;
                            article.querySelector('.heading p').textContent = item.subtitle;
                            article.querySelector('.body').innerHTML = item.body;
                            article.querySelector('.scroll').scrollTop = 0;
                            colourArticle(item.picture);
                            article.classList.add('open');
                        }

                        function move(step) {
                            centre = (centre + step + items.length) % items.length;
                            render();
                        }

                        // Three Slots: The Previous, The Centre, And The Next Card; With Two Items The Previous Slot Stays Empty, With One Both Do, So No Card Repeats
                        function render() {
                            track.innerHTML = '';
                            dots.innerHTML = '';
                            [-1, 0, 1].forEach(function (offset) {
                                var empty = (items.length < 3 && offset === -1) || (items.length < 2 && offset === 1);
                                if (empty) {
                                    var slot = document.createElement('div');
                                    slot.className = 'slot';
                                    track.appendChild(slot);
                                    return;
                                }
                                var item = items[(centre + offset + items.length) % items.length];
                                var card = document.createElement('div');
                                card.className = 'card' + (offset === 0 ? ' centre' : '');
                                var picture = document.createElement('div');
                                picture.className = 'picture';
                                picture.style.backgroundImage = 'url("' + item.picture + '")';
                                var words = document.createElement('div');
                                words.className = 'words';
                                var title = document.createElement('h2');
                                title.textContent = item.title;
                                var subtitle = document.createElement('p');
                                subtitle.textContent = item.subtitle;
                                words.appendChild(title);
                                words.appendChild(subtitle);
                                card.appendChild(picture);
                                card.appendChild(words);
                                card.addEventListener('click', function () {
                                    if (offset === 0) {
                                        openArticle(item);
                                        return;
                                    }
                                    move(offset);
                                });
                                track.appendChild(card);
                            });
                            items.forEach(function (_, index) {
                                var dot = document.createElement('span');
                                if (index === centre)
                                    dot.className = 'on';
                                dots.appendChild(dot);
                            });
                        }

                        frame.addEventListener('mouseenter', function () { hovering = true; });
                        frame.addEventListener('mouseleave', function () { hovering = false; });

                        // The Next Card Moves Into Focus Every 5 Seconds, Unless The Pointer Is Over The Carousel Or An Article Is Open
                        setInterval(function () {
                            if (hovering || article.classList.contains('open') || items.length < 2)
                                return;
                            move(1);
                        }, 5000);

                        frame.querySelector('.arrow.left').addEventListener('click', function () { move(-1); });
                        frame.querySelector('.arrow.right').addEventListener('click', function () { move(1); });
                        frame.querySelector('.back').addEventListener('click', function () { article.classList.remove('open'); });

                        render();
                    })();
                </script>
            </body>
            </html>
        """;
    }
}
