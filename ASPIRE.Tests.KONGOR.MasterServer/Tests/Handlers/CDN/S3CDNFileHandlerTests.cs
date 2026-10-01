namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Handlers.CDN;

/// <summary>
///     Unit tests for S3-compatible CDN file handling and MD5 hash caching.
/// </summary>
public sealed class S3CDNFileHandlerTests
{
    [Test]
    public async Task Computing_MD5_Returns_Quoted_32_Hex_String()
    {
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            string filePath = Path.Combine(temporaryDirectory, "test.txt");
            await File.WriteAllTextAsync(filePath, "Hello, World!");

            FileInfo fileInfo = new(filePath);
            S3MD5HashCache cache = new();

            string etag = cache.GetOrCreateETag(filePath, fileInfo);

            // MD5 Of "Hello, World!" In Lowercase Hexadecimal
            await Assert.That(etag).IsEqualTo(@"""65a8e27d8879283831b664bd8b7f0ad4""");
        }

        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Test]
    public async Task Cache_Returns_Existing_ETag_When_Path_And_Modified_Time_Match()
    {
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            string filePath = Path.Combine(temporaryDirectory, "test.txt");
            await File.WriteAllTextAsync(filePath, "Original Content");

            FileInfo fileInfo = new(filePath);
            S3MD5HashCache cache = new();

            string firstETag = cache.GetOrCreateETag(filePath, fileInfo);

            // Modify File Without Changing The FileInfo Timestamp Or Size Passed To The Cache Method
            await File.WriteAllTextAsync(filePath, "Different Content");

            string secondETag = cache.GetOrCreateETag(filePath, fileInfo);

            await Assert.That(secondETag).IsEqualTo(firstETag);
        }

        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Test]
    public async Task Cache_Recomputes_ETag_When_Modified_Time_Changes()
    {
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            string filePath = Path.Combine(temporaryDirectory, "test.txt");
            await File.WriteAllTextAsync(filePath, "Initial Content");

            FileInfo initialFileInfo = new(filePath);
            S3MD5HashCache cache = new();

            string initialETag = cache.GetOrCreateETag(filePath, initialFileInfo);

            await File.WriteAllTextAsync(filePath, "Updated Content");
            File.SetLastWriteTimeUtc(filePath, DateTime.UtcNow.AddMinutes(5));
            FileInfo updatedFileInfo = new(filePath);

            string updatedETag = cache.GetOrCreateETag(filePath, updatedFileInfo);

            await Assert.That(updatedETag).IsNotEqualTo(initialETag);
        }

        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Test]
    public async Task Handling_Range_Request_Returns_Partial_Content_With_Content_Range()
    {
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            string filePath = Path.Combine(temporaryDirectory, "range-test.txt");
            await File.WriteAllTextAsync(filePath, "0123456789");

            DefaultHttpContext httpContext = new();
            httpContext.Request.Method = "GET";
            httpContext.Request.Headers["Range"] = "bytes=2-6";
            httpContext.Response.Body = new MemoryStream();

            S3MD5HashCache cache = new();
            await S3CDNFileHandler.HandleRequest(httpContext, temporaryDirectory, "range-test.txt", cache);

            await Assert.That(httpContext.Response.StatusCode).IsEqualTo(StatusCodes.Status206PartialContent);
            await Assert.That(httpContext.Response.Headers["Content-Range"].ToString()).IsEqualTo("bytes 2-6/10");
            await Assert.That(httpContext.Response.Headers["Content-Length"].ToString()).IsEqualTo("5");
            await Assert.That(httpContext.Response.Headers["Accept-Ranges"].ToString()).IsEqualTo("bytes");

            httpContext.Response.Body.Position = 0;
            using StreamReader reader = new(httpContext.Response.Body);
            string responseBody = await reader.ReadToEndAsync();
            await Assert.That(responseBody).IsEqualTo("23456");
        }

        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Test]
    public async Task Handling_Range_Request_With_Open_Ended_Range_Returns_Partial_Content()
    {
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            string filePath = Path.Combine(temporaryDirectory, "range-test.txt");
            await File.WriteAllTextAsync(filePath, "0123456789");

            DefaultHttpContext httpContext = new();
            httpContext.Request.Method = "GET";
            httpContext.Request.Headers["Range"] = "bytes=5-";
            httpContext.Response.Body = new MemoryStream();

            S3MD5HashCache cache = new();
            await S3CDNFileHandler.HandleRequest(httpContext, temporaryDirectory, "range-test.txt", cache);

            await Assert.That(httpContext.Response.StatusCode).IsEqualTo(StatusCodes.Status206PartialContent);
            await Assert.That(httpContext.Response.Headers["Content-Range"].ToString()).IsEqualTo("bytes 5-9/10");
            await Assert.That(httpContext.Response.Headers["Content-Length"].ToString()).IsEqualTo("5");

            httpContext.Response.Body.Position = 0;
            using StreamReader reader = new(httpContext.Response.Body);
            string responseBody = await reader.ReadToEndAsync();
            await Assert.That(responseBody).IsEqualTo("56789");
        }

        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Test]
    public async Task Handling_Range_Request_With_Invalid_Range_Returns_416_Range_Not_Satisfiable()
    {
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            string filePath = Path.Combine(temporaryDirectory, "range-test.txt");
            await File.WriteAllTextAsync(filePath, "0123456789");

            DefaultHttpContext httpContext = new();
            httpContext.Request.Method = "GET";
            httpContext.Request.Headers["Range"] = "bytes=20-30";
            httpContext.Response.Body = new MemoryStream();

            S3MD5HashCache cache = new();
            await S3CDNFileHandler.HandleRequest(httpContext, temporaryDirectory, "range-test.txt", cache);

            await Assert.That(httpContext.Response.StatusCode).IsEqualTo(StatusCodes.Status416RangeNotSatisfiable);
            await Assert.That(httpContext.Response.Headers["Content-Range"].ToString()).IsEqualTo("bytes */10");
        }

        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Test]
    public async Task Handling_HEAD_Request_Returns_Accurate_Headers_Without_Response_Body()
    {
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            string filePath = Path.Combine(temporaryDirectory, "head-test.txt");
            await File.WriteAllTextAsync(filePath, "Hello, World!");

            DefaultHttpContext httpContext = new();
            httpContext.Request.Method = "HEAD";
            httpContext.Response.Body = new MemoryStream();

            S3MD5HashCache cache = new();
            await S3CDNFileHandler.HandleRequest(httpContext, temporaryDirectory, "head-test.txt", cache);

            await Assert.That(httpContext.Response.StatusCode).IsEqualTo(StatusCodes.Status200OK);
            await Assert.That(httpContext.Response.Headers["ETag"].ToString()).IsEqualTo(@"""65a8e27d8879283831b664bd8b7f0ad4""");
            await Assert.That(httpContext.Response.Headers["Content-Length"].ToString()).IsEqualTo("13");
            await Assert.That(httpContext.Response.Headers["Content-Type"].ToString()).StartsWith("text/plain");
            await Assert.That(httpContext.Response.Headers["Accept-Ranges"].ToString()).IsEqualTo("bytes");
            await Assert.That(httpContext.Response.Body.Length).IsEqualTo(0L);
        }

        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Test]
    public async Task Handling_GET_Request_Returns_Full_Content_With_200_OK()
    {
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            string filePath = Path.Combine(temporaryDirectory, "get-test.txt");
            await File.WriteAllTextAsync(filePath, "Full CDN Content");

            DefaultHttpContext httpContext = new();
            httpContext.Request.Method = "GET";
            httpContext.Response.Body = new MemoryStream();

            S3MD5HashCache cache = new();
            await S3CDNFileHandler.HandleRequest(httpContext, temporaryDirectory, "get-test.txt", cache);

            await Assert.That(httpContext.Response.StatusCode).IsEqualTo(StatusCodes.Status200OK);
            await Assert.That(httpContext.Response.Headers["ETag"].ToString()).IsNotEmpty();
            await Assert.That(httpContext.Response.Headers["Content-Length"].ToString()).IsEqualTo("16");

            httpContext.Response.Body.Position = 0;
            using StreamReader reader = new(httpContext.Response.Body);
            string responseBody = await reader.ReadToEndAsync();
            await Assert.That(responseBody).IsEqualTo("Full CDN Content");
        }

        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Test]
    public async Task Handling_OPTIONS_Request_Returns_CORS_Headers()
    {
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            string filePath = Path.Combine(temporaryDirectory, "options-test.txt");
            await File.WriteAllTextAsync(filePath, "Content");

            DefaultHttpContext httpContext = new();
            httpContext.Request.Method = "OPTIONS";
            httpContext.Response.Body = new MemoryStream();

            S3MD5HashCache cache = new();
            await S3CDNFileHandler.HandleRequest(httpContext, temporaryDirectory, "options-test.txt", cache);

            await Assert.That(httpContext.Response.StatusCode).IsEqualTo(StatusCodes.Status200OK);
            await Assert.That(httpContext.Response.Headers["Access-Control-Allow-Origin"].ToString()).IsEqualTo("*");
            await Assert.That(httpContext.Response.Headers["Access-Control-Allow-Methods"].ToString()).IsEqualTo("GET, HEAD, OPTIONS");
            await Assert.That(httpContext.Response.Headers["Access-Control-Allow-Headers"].ToString()).IsEqualTo("*");
            await Assert.That(httpContext.Response.Headers["Access-Control-Expose-Headers"].ToString()).IsEqualTo("ETag, Content-Length, Content-Range, Accept-Ranges");
        }

        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Test]
    public async Task Handling_Path_Traversal_Returns_404_Not_Found()
    {
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            string[] suspiciousPaths =
            [
                "../secret.txt",
                @"..\secret.txt",
                "/../secret.txt",
                "folder/../../secret.txt",
                "C:/Windows/win.ini",
                "%2e%2e/secret.txt"
            ];

            S3MD5HashCache cache = new();

            foreach (string suspiciousPath in suspiciousPaths)
            {
                DefaultHttpContext httpContext = new();
                httpContext.Request.Method = "GET";
                httpContext.Response.Body = new MemoryStream();

                await S3CDNFileHandler.HandleRequest(httpContext, temporaryDirectory, suspiciousPath, cache);

                await Assert.That(httpContext.Response.StatusCode).IsEqualTo(StatusCodes.Status404NotFound);
            }
        }

        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Test]
    public async Task Handling_Non_Existent_File_Returns_404_Not_Found()
    {
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            DefaultHttpContext httpContext = new();
            httpContext.Request.Method = "GET";
            httpContext.Response.Body = new MemoryStream();

            S3MD5HashCache cache = new();
            await S3CDNFileHandler.HandleRequest(httpContext, temporaryDirectory, "nonexistent.s2z", cache);

            await Assert.That(httpContext.Response.StatusCode).IsEqualTo(StatusCodes.Status404NotFound);
        }

        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Test]
    public async Task Resolving_S2Z_File_Extension_Returns_Application_Octet_Stream_MIME_Type()
    {
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            string filePath = Path.Combine(temporaryDirectory, "game_data.s2z");
            await File.WriteAllBytesAsync(filePath, [0x01, 0x02, 0x03]);

            DefaultHttpContext httpContext = new();
            httpContext.Request.Method = "HEAD";
            httpContext.Response.Body = new MemoryStream();

            S3MD5HashCache cache = new();
            await S3CDNFileHandler.HandleRequest(httpContext, temporaryDirectory, "game_data.s2z", cache);

            await Assert.That(httpContext.Response.StatusCode).IsEqualTo(StatusCodes.Status200OK);
            await Assert.That(httpContext.Response.Headers["Content-Type"].ToString()).IsEqualTo("application/octet-stream");
        }

        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Test]
    public async Task Mapping_Endpoint_When_Configured_Registers_Route()
    {
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            builder.Services.AddSingleton<S3MD5HashCache>();
            WebApplication application = builder.Build();

            OperationalConfigurationCDN configuration = new()
            {
                Host = "http://localhost:5555",
                PrimaryPatchURL = "http://localhost:5555/patch",
                SecondaryPatchURL = "http://localhost:5555/patch",
                ServeFromLocalDirectoryURL = "/cdn",
                LocalDirectory = temporaryDirectory
            };

            S3CDNFileHandler.Map(application, configuration, AppContext.BaseDirectory);

            IEndpointRouteBuilder routeBuilder = application;
            IReadOnlyList<Endpoint> endpoints = [.. routeBuilder.DataSources.SelectMany(dataSource => dataSource.Endpoints)];

            await Assert.That(endpoints.Count).IsGreaterThan(0);
        }

        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Test]
    public async Task Mapping_Endpoint_With_Null_Or_Empty_Configuration_Does_Not_Register_Route()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        WebApplication application = builder.Build();

        OperationalConfigurationCDN configuration = new()
        {
            Host = "http://localhost:5555",
            PrimaryPatchURL = "http://localhost:5555/patch",
            SecondaryPatchURL = "http://localhost:5555/patch",
            ServeFromLocalDirectoryURL = null,
            LocalDirectory = null
        };

        S3CDNFileHandler.Map(application, configuration, AppContext.BaseDirectory);

        IEndpointRouteBuilder routeBuilder = application;
        IReadOnlyList<Endpoint> endpoints = [.. routeBuilder.DataSources.SelectMany(dataSource => dataSource.Endpoints)];

        await Assert.That(endpoints.Count).IsEqualTo(0);
    }
}

