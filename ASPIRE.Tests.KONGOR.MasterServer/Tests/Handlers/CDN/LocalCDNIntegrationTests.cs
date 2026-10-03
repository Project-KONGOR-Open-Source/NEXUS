namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Handlers.CDN;

/// <summary>
///     Integration tests for the local CDN, verifying that the master server serves CDN files at the request path bound from its application settings.
///     Each test points the local CDN directory at a temporary directory, so the tests do not depend on any repository being cloned alongside the solution.
/// </summary>
public sealed class LocalCDNIntegrationTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private string LocalCDNDirectory { get; } = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

    [After(HookType.Test)]
    public void After_Each_Test()
    {
        if (Directory.Exists(LocalCDNDirectory))
            Directory.Delete(LocalCDNDirectory, recursive: true);
    }

    [Test]
    public async Task Requesting_A_File_From_The_Local_CDN_Returns_It_Through_The_Master_Server()
    {
        Directory.CreateDirectory(Path.Combine(LocalCDNDirectory, "wac"));

        await File.WriteAllTextAsync(Path.Combine(LocalCDNDirectory, "wac", "manifest.json"), """{ "version": "4.10.1" }""");

        await using WebApplicationFactory<KONGORAssemblyMarker> factory = CreateFactoryWithLocalCDNDirectory(LocalCDNDirectory);

        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.GetAsync("/cdn/wac/manifest.json");

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(await response.Content.ReadAsStringAsync()).IsEqualTo("""{ "version": "4.10.1" }""");
        }
    }

    [Test]
    public async Task Requesting_A_File_When_The_Local_CDN_Directory_Does_Not_Exist_Returns_404_Not_Found()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> factory = CreateFactoryWithLocalCDNDirectory(LocalCDNDirectory);

        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.GetAsync("/cdn/wac/manifest.json");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    private WebApplicationFactory<KONGORAssemblyMarker> CreateFactoryWithLocalCDNDirectory(string localDirectory)
        => webApplicationFactory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.PostConfigure<OperationalConfiguration>(configuration => configuration.CDN.LocalDirectory = localDirectory)));
}
