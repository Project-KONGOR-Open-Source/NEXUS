namespace KONGOR.MasterServer.Handlers.CDN;

/// <summary>
///     Resolves local directory paths for CDN file serving, expanding user profile tokens and relative paths.
/// </summary>
public static class LocalCDNDirectoryPathResolver
{
    /// <summary>
    ///    Resolves a local directory path for CDN file serving.
    ///    The <c>{USER}</c> token is replaced with the user's home directory, and the <c>{TEMP}</c> token is replaced with the system's temporary directory.
    ///    Relative paths are resolved against the application's base directory.
    /// </summary>
    /// <returns>
    ///     The resolved local directory path.
    /// </returns>
    public static string Resolve(string path)
    {
        path = path.Trim()
            .Replace("{USER}", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), StringComparison.OrdinalIgnoreCase)
            .Replace("{TEMP}", Path.GetTempPath(), StringComparison.OrdinalIgnoreCase);

        return Path.IsPathFullyQualified(path)
            ? Path.GetFullPath(path)
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, path));
    }
}
