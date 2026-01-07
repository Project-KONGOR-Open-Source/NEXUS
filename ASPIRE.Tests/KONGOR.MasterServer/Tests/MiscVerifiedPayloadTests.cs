namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

using ASPIRE.Tests.KONGOR.MasterServer.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

public sealed class MiscVerifiedPayloadTests
{
    private async Task<(HttpClient Client, WebApplicationFactory<KONGORAssemblyMarker> Factory)> SetupAsync()
    {
        WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = factory.CreateClient();
        
        // Note: Full DB/Auth setup omitted as it is not currently needed for Swagger/Simple endpoint tests.
        // If future tests require it, copy SetupAsync from MiscUnverifiedTests.cs.
        
        return (client, factory);
    }

    [Test]
    public async Task GetSwaggerJson_ReturnsSuccess()
    {
        (HttpClient client, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        using (factory)
        {
            HttpResponseMessage response = await client.GetAsync("swagger/v1/swagger.json");
            response.EnsureSuccessStatusCode();
        }
    }
}
