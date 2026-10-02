namespace KONGOR.MasterServer.Handlers.CDN;

/// <summary>
///     Serves CDN files from a local directory, using the same layout as the public CDN.
/// </summary>
public static class LocalCDNFileHandler
{
    // The Number Of CDN Files Transferred At Once Is Limited, So That A Surge Of Synchronisations Cannot Starve The Rest Of The Master Server
    private const int ConcurrentTransferLimit = 32;

    // Transfers Beyond The Limit Wait In A Bounded Queue Rather Than Being Rejected Straight Away, Since Clients Treat A Rejected Download As A Failed File
    private const int QueuedTransferLimit = 256;

    /// <summary>
    ///     Adds static file serving for the local CDN directory, when both the request path and the local directory are configured.
    ///     The request path is served regardless of whether the local directory exists, so requests for files or directories which are not there are answered with 404 Not Found, and a local directory which is created later is served without a restart.
    ///     The number of concurrent CDN file transfers is limited, and transfers beyond the limit are queued.
    /// </summary>
    /// <param name="application">The web application to add the static file serving middleware to.</param>
    /// <param name="configuration">The CDN configuration settings.</param>
    /// <exception cref="InvalidOperationException">Thrown when the configured request path is not a plain relative URL path.</exception>
    public static void Use(WebApplication application, OperationalConfigurationCDN configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration.ServeFromLocalDirectoryURL) || string.IsNullOrWhiteSpace(configuration.LocalDirectory))
            return;

        string requestPath = configuration.ServeFromLocalDirectoryURL.Trim('/');

        // The Request Path Must Name At Least One Path Segment, And Must Not Be An Absolute URL Or Carry A Query Or A Fragment, Since None Of Those Can Ever Match A Request
        if (requestPath.Length is 0 || requestPath.Any(character => char.IsWhiteSpace(character) || character is ':' or '?' or '#'))
            throw new InvalidOperationException($@"Invalid Local CDN Directory URL ""{configuration.ServeFromLocalDirectoryURL}""");

        string localDirectory = LocalCDNDirectoryPathResolver.Resolve(configuration.LocalDirectory);

        application.Logger.LogInformation(@"Serving Local CDN Directory ""{LocalCDNDirectory}"" At ""/{LocalCDNDirectoryURL}""", localDirectory, requestPath);

        application.Map(new PathString("/" + requestPath), branch =>
        {
            branch.UseRateLimiter(new RateLimiterOptions
            {
                GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context => RateLimitPartition.GetConcurrencyLimiter("CDN", partitionKey => new ConcurrencyLimiterOptions
                {
                    PermitLimit = ConcurrentTransferLimit,
                    QueueLimit = QueuedTransferLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                }))
            });

            branch.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(localDirectory),

                // Distributions Contain Extensionless Executables And Custom Archive Formats (e.g. ".s2z"), Which Have No Registered MIME Type
                ServeUnknownFileTypes = true,
                DefaultContentType = "application/octet-stream"
            });
        });
    }
}
