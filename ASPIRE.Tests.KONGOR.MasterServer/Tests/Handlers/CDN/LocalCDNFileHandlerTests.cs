namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Handlers.CDN;

/// <summary>
///     Request-level tests for <see cref="LocalCDNFileHandler"/>, verifying that a local directory is served with the same layout as the public CDN.
/// </summary>
public sealed class LocalCDNFileHandlerTests
{
    private string TemporaryDirectory { get; } = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    private string LocalCDNDirectory => Path.Combine(TemporaryDirectory, "CDN");

    [Before(HookType.Test)]
    public void Before_Each_Test()
        => Directory.CreateDirectory(LocalCDNDirectory);

    [After(HookType.Test)]
    public void After_Each_Test()
        => Directory.Delete(TemporaryDirectory, recursive: true);

    [Test]
    public async Task Requesting_A_Manifest_Returns_Its_Content_As_JSON()
    {
        await WriteLocalCDNFile("wac/manifest.json", Encoding.UTF8.GetBytes("""{ "version": "4.10.1" }"""));

        await using WebApplication application = await StartApplication(LocalCDNDirectory);

        using HttpResponseMessage response = await application.GetTestClient().GetAsync("/cdn/wac/manifest.json");

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(response.Content.Headers.ContentType?.MediaType).IsEqualTo("application/json");
            await Assert.That(await response.Content.ReadAsStringAsync()).IsEqualTo("""{ "version": "4.10.1" }""");
        }
    }

    [Test]
    [Arguments("las/hon-x86_64-server")]
    [Arguments("wac/base/resources0.s2z")]
    public async Task Requesting_A_File_Without_A_Registered_MIME_Type_Returns_It_As_Binary_Content(string relativePath)
    {
        byte[] content = [0x01, 0x02, 0x03];

        await WriteLocalCDNFile(relativePath, content);

        await using WebApplication application = await StartApplication(LocalCDNDirectory);

        using HttpResponseMessage response = await application.GetTestClient().GetAsync($"/cdn/{relativePath}");

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(response.Content.Headers.ContentType?.MediaType).IsEqualTo("application/octet-stream");
            await Assert.That((await response.Content.ReadAsByteArrayAsync()).SequenceEqual(content)).IsTrue();
        }
    }

    [Test]
    public async Task Probing_A_Manifest_With_A_HEAD_Request_Returns_Its_Headers_Without_A_Body()
    {
        byte[] content = Encoding.UTF8.GetBytes("""{ "version": "4.10.1" }""");

        await WriteLocalCDNFile("was/manifest.json", content);

        await using WebApplication application = await StartApplication(LocalCDNDirectory);

        using HttpRequestMessage request = new (HttpMethod.Head, "/cdn/was/manifest.json");
        using HttpResponseMessage response = await application.GetTestClient().SendAsync(request);

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(response.Content.Headers.ContentLength).IsEqualTo(content.Length);
            await Assert.That((await response.Content.ReadAsByteArrayAsync()).Length).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Requesting_A_File_Outside_The_Local_CDN_Directory_Returns_404_Not_Found()
    {
        // The File Exists Next To The Local CDN Directory, So Only The Containment Check Can Produce The 404
        await File.WriteAllTextAsync(Path.Combine(TemporaryDirectory, "secret.txt"), "Secret");

        await using WebApplication application = await StartApplication(LocalCDNDirectory);

        // The HTTP Client Removes Dot Segments From Request URIs, So The Request Path Is Set Directly On The HTTP Context Instead
        HttpContext context = await application.GetTestServer().SendAsync(httpContext =>
        {
            httpContext.Request.Method = HttpMethods.Get;
            httpContext.Request.Path = "/cdn/../secret.txt";
        });

        await Assert.That(context.Response.StatusCode).IsEqualTo(StatusCodes.Status404NotFound);
    }

    [Test]
    public async Task Requesting_A_File_That_Does_Not_Exist_Returns_404_Not_Found()
    {
        await using WebApplication application = await StartApplication(LocalCDNDirectory);

        using HttpResponseMessage response = await application.GetTestClient().GetAsync("/cdn/wac/manifest.json");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task A_Missing_Local_CDN_Directory_Serves_No_Files_Without_Failing_Start_Up()
    {
        await using WebApplication application = await StartApplication(Path.Combine(TemporaryDirectory, "missing"));

        using HttpResponseMessage response = await application.GetTestClient().GetAsync("/cdn/wac/manifest.json");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task A_Local_CDN_Directory_Created_After_Start_Up_Is_Served_Without_A_Restart()
    {
        string localDirectory = Path.Combine(TemporaryDirectory, "created-later");

        await using WebApplication application = await StartApplication(localDirectory);

        using HttpResponseMessage responseBeforeCreation = await application.GetTestClient().GetAsync("/cdn/wac/manifest.json");

        Directory.CreateDirectory(Path.Combine(localDirectory, "wac"));

        await File.WriteAllTextAsync(Path.Combine(localDirectory, "wac", "manifest.json"), """{ "version": "4.10.1" }""");

        using HttpResponseMessage responseAfterCreation = await application.GetTestClient().GetAsync("/cdn/wac/manifest.json");

        using (Assert.Multiple())
        {
            await Assert.That(responseBeforeCreation.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
            await Assert.That(responseAfterCreation.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(await responseAfterCreation.Content.ReadAsStringAsync()).IsEqualTo("""{ "version": "4.10.1" }""");
        }
    }

    [Test]
    [Arguments("/")]
    [Arguments("//")]
    [Arguments("http://localhost:5555/cdn")]
    [Arguments("/cdn?version=1")]
    [Arguments("/cdn#files")]
    [Arguments("/local cdn")]
    public async Task An_Invalid_Local_CDN_Directory_URL_Throws_At_Start_Up(string localDirectoryURL)
    {
        await using WebApplication application = BuildApplication();

        OperationalConfigurationCDN configuration = CreateConfiguration(LocalCDNDirectory, localDirectoryURL);

        await Assert.That(() => LocalCDNFileHandler.Use(application, configuration))
            .Throws<InvalidOperationException>().WithMessageContaining("Invalid Local CDN Directory URL");
    }

    private async Task WriteLocalCDNFile(string relativePath, byte[] content)
    {
        string filePath = Path.Combine(LocalCDNDirectory, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? LocalCDNDirectory);

        await File.WriteAllBytesAsync(filePath, content);
    }

    private static async Task<WebApplication> StartApplication(string localDirectory)
    {
        WebApplication application = BuildApplication();

        LocalCDNFileHandler.Use(application, CreateConfiguration(localDirectory, "/cdn"));

        await application.StartAsync();

        return application;
    }

    private static WebApplication BuildApplication()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();

        // The Master Server Registers The Rate Limiting Services, Which The Local CDN's Concurrency Limiter Depends On
        builder.Services.AddRateLimiter();

        return builder.Build();
    }

    private static OperationalConfigurationCDN CreateConfiguration(string localDirectory, string localDirectoryURL)
        => new ()
        {
            Host = "http://localhost:5555",
            PrimaryPatchURL = "http://localhost:5555/patch",
            SecondaryPatchURL = "http://localhost:5555/patch",
            ServeFromLocalDirectoryURL = localDirectoryURL,
            LocalDirectory = localDirectory
        };
}
