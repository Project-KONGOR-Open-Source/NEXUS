namespace KONGOR.MasterServer.Handlers.Compatibility;

/// <summary>
///     Reads the version from the user agent the game sends with every request, "S2 Games/Heroes of Newerth/{version}/{distribution}/{architecture}", and words the refusals for another version.
/// </summary>
public static partial class UserAgentHandlers
{
    /// <summary>
    ///     Returns <see langword="true"/>, with the version, for exactly one user agent of the game's shape.
    ///     The game sends a single user agent, so a request which repeats the header does not come from the game.
    /// </summary>
    public static bool TryGetVersion(StringValues userAgent, [NotNullWhen(true)] out string? version)
    {
        Match match = userAgent.Count is 1 ? UserAgentRegex().Match(userAgent.ToString()) : Match.Empty;

        version = match.Success ? match.Groups["version"].Value : null;

        return match.Success;
    }

    /// <summary>
    ///     The refusal for a game client at another version, which names both versions but no service.
    /// </summary>
    public static string GameClientVersionMismatch(string version)
        => $"The Heroes Of Newerth Game Client Version {version} Does Not Match The Expected Version {Distributions.Version}";

    /// <summary>
    ///     The refusal for a match server, or the manager of match servers, at another version, which names both versions but no service.
    /// </summary>
    public static string MatchServerVersionMismatch(string version)
        => $"The Heroes Of Newerth Match Server Version {version} Does Not Match The Expected Version {Distributions.Version}";

    [GeneratedRegex(@"(?>S2 Games)\/(?>Heroes [oO]f Newerth)\/(?<version>\d{1,2}\.\d{1,2}\.\d{1,2}\.\d{1,2})\/(?<platform>[wlm]a[cs])\/(?<architecture>x86_64|x86-biarch|universal-64)")]
    private static partial Regex UserAgentRegex();
}
