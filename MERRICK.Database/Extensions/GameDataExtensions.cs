namespace MERRICK.Database.Extensions;

public static class GameDataExtensions
{
    public static IList<string> PipeSeparatedStringToList(this string input)
        => input.Split('|').ToList();

    public static string ListToPipeSeparatedString(this IList<string> input)
        => string.Join('|', input);
}
