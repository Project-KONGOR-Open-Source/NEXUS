namespace KONGOR.MasterServer.Handlers.CDN;

/// <summary>
///     Resolves local directory paths for CDN file serving, expanding path tokens and relative paths.
/// </summary>
public static class LocalCDNDirectoryPathResolver
{
    private static string SolutionRootDirectory { get; } = FindSolutionRootDirectory();

    /// <summary>
    ///     Resolves a local directory path for CDN file serving.
    ///     The <c>{ROOT}</c> token is replaced with the solution root directory, the <c>{USER}</c> token is replaced with the user's home directory, and the <c>{TEMP}</c> token is replaced with the system's temporary directory.
    ///     The solution root directory is the nearest ancestor of the application's base directory that contains a "license" file, which holds whether the application runs from source or from compiled binaries inside the solution.
    ///     If no ancestor contains a "license" file (for example, when compiled binaries are deployed outside of the solution), the application's base directory is used as the solution root directory instead.
    ///     Relative paths are resolved against the solution root directory.
    /// </summary>
    /// <param name="path">The configured local directory path.</param>
    /// <returns>
    ///     The resolved local directory path.
    /// </returns>
    public static string Resolve(string path)
    {
        // Token Values Are Trimmed Of Trailing Directory Separators, So That The Separator Which Follows A Token In The Configured Path Is Never Doubled
        path = path.Trim()
            .Replace("{ROOT}", SolutionRootDirectory, StringComparison.OrdinalIgnoreCase)
            .Replace("{USER}", Path.TrimEndingDirectorySeparator(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)), StringComparison.OrdinalIgnoreCase)
            .Replace("{TEMP}", Path.TrimEndingDirectorySeparator(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase);

        return Path.IsPathFullyQualified(path)
            ? Path.GetFullPath(path)
            : Path.GetFullPath(Path.Combine(SolutionRootDirectory, path));
    }

    private static string FindSolutionRootDirectory()
    {
        for (DirectoryInfo? directory = new (AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "license")))
                return Path.TrimEndingDirectorySeparator(directory.FullName);
        }

        return Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory);
    }
}
