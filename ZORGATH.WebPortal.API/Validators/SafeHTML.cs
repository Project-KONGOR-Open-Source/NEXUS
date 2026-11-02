namespace ZORGATH.WebPortal.API.Validators;

/// <summary>
///     Holds the HTML of an event's or an announcement's body to the small subset the client's pages show: paragraphs, line breaks, bold, italic, and underlined text, lists, inline code and code blocks, and links to HTTP or HTTPS addresses.
/// </summary>
public static class SafeHTML
{
    /// <summary>
    ///     Returns <see langword="true"/> when sanitising the HTML would remove nothing that the administrator can see they wrote, and <see langword="false"/> otherwise, so that a refused body can be explained.
    /// </summary>
    public static bool IsWithinSubset(string html)
    {
        HtmlSanitizer sanitiser = CreateSanitiser();

        bool removed = false;

        sanitiser.RemovingTag += (_, _) => removed = true;
        sanitiser.RemovingAttribute += (_, _) => removed = true;
        sanitiser.RemovingComment += (_, _) => removed = true;
        sanitiser.RemovingStyle += (_, _) => removed = true;
        sanitiser.RemovingAtRule += (_, _) => removed = true;
        sanitiser.RemovingCssClass += (_, _) => removed = true;

        sanitiser.Sanitize(html);

        return removed is false;
    }

    /// <summary>
    ///     Returns the HTML as the sanitiser writes it out, which is what is stored.
    ///     The parser drops what it cannot place, such as a closing tag with nothing to close, without reporting it, so the original text could still close the page's own elements; the sanitiser's output is what the parser made of it.
    /// </summary>
    public static string Sanitise(string html)
        => CreateSanitiser().Sanitize(html);

    private static HtmlSanitizer CreateSanitiser()
        => new (new HtmlSanitizerOptions
        {
            AllowedTags = new HashSet<string>(["p", "br", "strong", "em", "b", "i", "u", "ul", "ol", "li", "a", "code", "pre"], StringComparer.OrdinalIgnoreCase),
            AllowedAttributes = new HashSet<string>(["href"], StringComparer.OrdinalIgnoreCase),
            AllowedSchemes = new HashSet<string>(["http", "https"], StringComparer.OrdinalIgnoreCase),
            UriAttributes = new HashSet<string>(["href"], StringComparer.OrdinalIgnoreCase)
        });
}
