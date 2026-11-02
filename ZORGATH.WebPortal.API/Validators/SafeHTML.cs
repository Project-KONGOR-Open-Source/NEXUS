namespace ZORGATH.WebPortal.API.Validators;

/// <summary>
///     Checks that the HTML of an event's or an announcement's body stays within the small subset the client's pages show: paragraphs, line breaks, bold, italic, and underlined text, lists, and links to HTTP or HTTPS addresses.
/// </summary>
public static class SafeHTML
{
    /// <summary>
    ///     Returns <see langword="true"/> when sanitising the HTML would remove nothing, so that it is stored exactly as the administrator wrote it, and <see langword="false"/> otherwise.
    /// </summary>
    public static bool IsWithinSubset(string html)
    {
        HtmlSanitizer sanitiser = new (new HtmlSanitizerOptions
        {
            AllowedTags = new HashSet<string>(["p", "br", "strong", "em", "b", "i", "u", "ul", "ol", "li", "a"], StringComparer.OrdinalIgnoreCase),
            AllowedAttributes = new HashSet<string>(["href"], StringComparer.OrdinalIgnoreCase),
            AllowedSchemes = new HashSet<string>(["http", "https"], StringComparer.OrdinalIgnoreCase),
            UriAttributes = new HashSet<string>(["href"], StringComparer.OrdinalIgnoreCase)
        });

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
}
