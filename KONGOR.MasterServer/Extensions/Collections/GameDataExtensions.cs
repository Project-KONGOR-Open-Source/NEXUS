namespace KONGOR.MasterServer.Extensions.Collections;

// TODO: Move To Shared Project To Remove Duplication

public static class GameDataExtensions
{
    public static IList<string> PipeSeparatedStringToList(this string input)
    {
        return input.Split('|').ToList();
    }

    public static string ListToPipeSeparatedString(this IList<string> input)
    {
        return string.Join('|', input);
    }
}