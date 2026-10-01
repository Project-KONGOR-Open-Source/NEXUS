namespace KONGOR.MasterServer.Handlers.CDN;

/// <summary>
///     Handles S3-compatible HTTP requests for CDN files served from a local directory.
/// </summary>
public static class S3CDNFileHandler
{
    private const string DefaultContentType = "application/octet-stream";

    private const int BufferSize = 64 * 1024;

    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = CreateContentTypeProvider();

    private static readonly S3MD5HashCache DefaultHashCache = new();

    /// <summary>
    ///     Maps the S3-compatible CDN file serving endpoint when local directory serving is enabled.
    /// </summary>
    /// <param name="application">The web application builder used to configure endpoints.</param>
    /// <param name="configuration">The CDN configuration settings.</param>
    /// <param name="contentRootPath">The application content root path used to resolve relative paths.</param>
    public static void Map(WebApplication application, OperationalConfigurationCDN configuration, string contentRootPath)
    {
        if (string.IsNullOrWhiteSpace(configuration.ServeFromLocalDirectoryURL) || string.IsNullOrWhiteSpace(configuration.LocalDirectory))
        {
            return;
        }

        string resolvedRootDirectory = LocalCDNDirectoryPathResolver.Resolve(configuration.LocalDirectory);

        string routePrefix = "/" + configuration.ServeFromLocalDirectoryURL.Trim('/');

        application.MapMethods($"{routePrefix}/{{*subpath}}", ["GET", "HEAD", "OPTIONS"], (HttpContext httpContext, string? subpath, S3MD5HashCache hashCache) =>
            HandleRequest(httpContext, resolvedRootDirectory, subpath ?? string.Empty, hashCache));
    }

    /// <summary>
    ///     Processes an incoming S3-compatible CDN file request, handling CORS, ETags, byte ranges, and file streaming.
    /// </summary>
    /// <param name="httpContext">The HTTP context for the current request.</param>
    /// <param name="rootDirectory">The root directory on disk from which CDN files are served.</param>
    /// <param name="relativePath">The relative path to the requested file within the root directory.</param>
    /// <param name="hashCache">The optional MD5 hash cache used to generate ETags.</param>
    /// <returns>A task that completes when the request handling has finished.</returns>
    public static async Task HandleRequest(HttpContext httpContext, string rootDirectory, string relativePath, S3MD5HashCache? hashCache = null)
    {
        // Set CORS Response Headers Unconditionally
        IHeaderDictionary responseHeaders = httpContext.Response.Headers;
        responseHeaders["Access-Control-Allow-Origin"] = "*";
        responseHeaders["Access-Control-Allow-Methods"] = "GET, HEAD, OPTIONS";
        responseHeaders["Access-Control-Allow-Headers"] = "*";
        responseHeaders["Access-Control-Expose-Headers"] = "ETag, Content-Length, Content-Range, Accept-Ranges";

        // Handle Preflight OPTIONS Requests
        if (HttpMethods.IsOptions(httpContext.Request.Method))
        {
            httpContext.Response.StatusCode = StatusCodes.Status200OK;
            return;
        }

        // Verify And Normalise The Root Directory Path
        string normalisedRoot = Path.GetFullPath(rootDirectory);
        if (!normalisedRoot.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
        {
            normalisedRoot += Path.DirectorySeparatorChar;
        }

        // Resolve And Normalise Relative Path, Disallowing Path Traversal
        string fullPath;
        try
        {
            string unescapedRelativePath = Uri.UnescapeDataString(relativePath);
            string cleanRelativePath = unescapedRelativePath.TrimStart('/', '\\');
            fullPath = Path.GetFullPath(Path.Combine(normalisedRoot, cleanRelativePath));
        }
        catch (Exception)
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        if (!fullPath.StartsWith(normalisedRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        FileInfo fileInfo = new(fullPath);

        // Resolve MIME Content-Type
        string contentType = ContentTypeProvider.TryGetContentType(fullPath, out string? resolvedType)
            ? resolvedType
            : DefaultContentType;

        // Obtain Quoted MD5 ETag
        S3MD5HashCache cache = hashCache ?? httpContext.RequestServices.GetService<S3MD5HashCache>() ?? DefaultHashCache;
        string etag = cache.GetOrCreateETag(fullPath, fileInfo);

        string lastModified = fileInfo.LastWriteTimeUtc.ToString("R", CultureInfo.InvariantCulture);

        responseHeaders["ETag"] = etag;
        responseHeaders["Accept-Ranges"] = "bytes";
        responseHeaders["Last-Modified"] = lastModified;

        // Check For HTTP Range Header
        string? rangeHeader = httpContext.Request.Headers["Range"].ToString();
        if (!string.IsNullOrWhiteSpace(rangeHeader))
        {
            if (!TryParseByteRange(rangeHeader, fileInfo.Length, out long start, out long end))
            {
                httpContext.Response.StatusCode = StatusCodes.Status416RangeNotSatisfiable;
                responseHeaders["Content-Range"] = $"bytes */{fileInfo.Length}";
                return;
            }

            long contentLength = end - start + 1;
            httpContext.Response.StatusCode = StatusCodes.Status206PartialContent;
            responseHeaders["Content-Range"] = $"bytes {start}-{end}/{fileInfo.Length}";
            responseHeaders["Content-Length"] = contentLength.ToString(CultureInfo.InvariantCulture);
            responseHeaders["Content-Type"] = contentType;

            if (HttpMethods.IsHead(httpContext.Request.Method))
            {
                return;
            }

            await using FileStream rangeStream = new(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: BufferSize, useAsync: true);
            rangeStream.Seek(start, SeekOrigin.Begin);

            byte[] buffer = new byte[BufferSize];
            long bytesRemaining = contentLength;

            while (bytesRemaining > 0)
            {
                int bytesToRead = (int)Math.Min(buffer.Length, bytesRemaining);
                int bytesRead = await rangeStream.ReadAsync(buffer.AsMemory(0, bytesToRead), httpContext.RequestAborted);

                if (bytesRead == 0)
                {
                    break;
                }

                await httpContext.Response.Body.WriteAsync(buffer.AsMemory(0, bytesRead), httpContext.RequestAborted);
                bytesRemaining -= bytesRead;
            }

            return;
        }

        // Full File Response
        httpContext.Response.StatusCode = StatusCodes.Status200OK;
        responseHeaders["Content-Length"] = fileInfo.Length.ToString(CultureInfo.InvariantCulture);
        responseHeaders["Content-Type"] = contentType;

        if (HttpMethods.IsHead(httpContext.Request.Method))
        {
            return;
        }

        await using FileStream fileStream = new(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: BufferSize, useAsync: true);
        await fileStream.CopyToAsync(httpContext.Response.Body, httpContext.RequestAborted);
    }

    private static FileExtensionContentTypeProvider CreateContentTypeProvider()
    {
        FileExtensionContentTypeProvider provider = new();
        provider.Mappings[".s2z"] = "application/octet-stream";
        return provider;
    }

    private static bool TryParseByteRange(string rangeHeader, long totalLength, out long start, out long end)
    {
        start = 0;
        end = 0;

        if (!rangeHeader.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string rangeSpecification = rangeHeader["bytes=".Length..].Trim();
        if (rangeSpecification.Contains(','))
        {
            rangeSpecification = rangeSpecification.Split(',')[0].Trim();
        }

        int dashIndex = rangeSpecification.IndexOf('-');
        if (dashIndex == -1)
        {
            return false;
        }

        if (dashIndex == 0)
        {
            // Suffix Byte Range: "-suffix"
            string suffixText = rangeSpecification[1..].Trim();
            if (!long.TryParse(suffixText, CultureInfo.InvariantCulture, out long suffix) || suffix <= 0 || totalLength == 0)
            {
                return false;
            }

            start = Math.Max(0, totalLength - suffix);
            end = totalLength - 1;
            return true;
        }

        string startText = rangeSpecification[..dashIndex].Trim();
        if (!long.TryParse(startText, CultureInfo.InvariantCulture, out start) || start < 0 || start >= totalLength)
        {
            return false;
        }

        if (dashIndex == rangeSpecification.Length - 1)
        {
            // Open-Ended Byte Range: "start-"
            end = totalLength - 1;
            return true;
        }

        // Closed Byte Range: "start-end"
        string endText = rangeSpecification[(dashIndex + 1)..].Trim();
        if (!long.TryParse(endText, CultureInfo.InvariantCulture, out end) || end < start)
        {
            return false;
        }

        if (end >= totalLength)
        {
            end = totalLength - 1;
        }

        return true;
    }
}
