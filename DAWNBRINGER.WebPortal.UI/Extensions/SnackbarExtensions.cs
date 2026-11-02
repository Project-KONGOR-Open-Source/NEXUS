namespace DAWNBRINGER.WebPortal.UI.Extensions;

public static class SnackbarExtensions
{
    /// <summary>
    ///     Shows each message of a failed response from the web portal's API, which is either a JSON list of validation messages or a single message, and the fallback when the response holds none.
    /// </summary>
    public static async Task AddResponseErrors(this ISnackbar snackbar, HttpResponseMessage response, string fallback)
    {
        string content = await response.Content.ReadAsStringAsync();

        List<string> messages = [];

        if (content.TrimStart().StartsWith('['))
        {
            try
            {
                messages = JsonSerializer.Deserialize<List<string>>(content) ?? [];
            }

            catch (JsonException)
            {
                messages = [content];
            }
        }

        else if (string.IsNullOrWhiteSpace(content) is false)
        {
            messages = [content];
        }

        if (messages.Count is 0)
            messages = [fallback];

        foreach (string message in messages)
            snackbar.Add(message, Severity.Error);
    }
}
