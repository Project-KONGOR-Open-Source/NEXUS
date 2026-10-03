namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Health;

/// <summary>
///     Integration tests for the master server's health endpoint, which WILLOWMAKER probes with an HTTP HEAD request to report whether a master server is online.
/// </summary>
public sealed class HealthEndpointTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

    [Test]
    public async Task Probing_The_Health_Endpoint_With_A_HEAD_Request_Returns_200_OK()
    {
        using HttpClient client = webApplicationFactory.CreateClient();

        using HttpRequestMessage request = new (HttpMethod.Head, "/health");
        using HttpResponseMessage response = await client.SendAsync(request);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }
}
