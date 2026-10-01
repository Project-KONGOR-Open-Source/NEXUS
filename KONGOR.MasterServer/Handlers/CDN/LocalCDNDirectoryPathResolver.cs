namespace KONGOR.MasterServer.Handlers.CDN;

/// <summary>
///     Resolves local directory paths for CDN file serving, expanding user profile tokens and relative paths.
/// </summary>
public static class LocalCDNDirectoryPathResolver
{
    /// <summary>
    ///     Resolves the configured local directory path against user profile tokens or the content root path.
    /// </summary>
    /// <param name="configuredPath">The directory path configured in application settings.</param>
    /// <param name="contentRootPath">The application content root path used to resolve relative paths.</param>
    /// <returns>The fully qualified directory path, or <see langword="null"/> if the configured path is null or whitespace.</returns>
    public static string? Resolve(string? configuredPath, string contentRootPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
            return null;

        string path = configuredPath.Trim();

        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        path = path.Replace("Directory.User", userProfile, StringComparison.OrdinalIgnoreCase);
        path = path.Replace("{UserProfile}", userProfile, StringComparison.OrdinalIgnoreCase);

        if (Path.IsPathFullyQualified(path))
            return Path.GetFullPath(path);

        return Path.GetFullPath(Path.Combine(contentRootPath, path));
    }
}
