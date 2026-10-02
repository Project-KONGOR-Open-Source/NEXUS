namespace KONGOR.MasterServer.Handlers.CDN;

/// <summary>
///     Serves CDN files from a local directory, using the same layout as the public CDN.
/// </summary>
public static class LocalCDNFileHandler
{
    /// <summary>
    ///     Adds static file serving for the local CDN directory, when both the request path and the local directory are configured.
    ///     The local CDN is optional, so if the local directory does not exist, this is logged as information and no CDN files are served.
    /// </summary>
    /// <param name="application">The web application to add the static file serving middleware to.</param>
    /// <param name="configuration">The CDN configuration settings.</param>
    public static void Use(WebApplication application, OperationalConfigurationCDN configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration.ServeFromLocalDirectoryURL) || string.IsNullOrWhiteSpace(configuration.LocalDirectory))
            return;

        string localDirectory = LocalCDNDirectoryPathResolver.Resolve(configuration.LocalDirectory);

        if (Directory.Exists(localDirectory) is false)
        {
            application.Logger.LogInformation(@"Local CDN Directory ""{LocalCDNDirectory}"" Does Not Exist, So No CDN Files Will Be Served", localDirectory);

            return;
        }

        application.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(localDirectory),
            RequestPath = "/" + configuration.ServeFromLocalDirectoryURL.Trim('/'),

            // Distributions Contain Extensionless Executables And Custom Archive Formats (e.g. ".s2z"), Which Have No Registered MIME Type
            ServeUnknownFileTypes = true,
            DefaultContentType = "application/octet-stream"
        });
    }
}
