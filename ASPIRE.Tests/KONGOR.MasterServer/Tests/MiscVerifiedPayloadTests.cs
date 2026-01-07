namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

using ASPIRE.Tests.KONGOR.MasterServer.Infrastructure;
using global::KONGOR.MasterServer.Extensions.Cache;
using Microsoft.AspNetCore.Mvc.Testing;

public sealed class MiscVerifiedPayloadTests
{
    private Task<(HttpClient Client, WebApplicationFactory<KONGORAssemblyMarker> Factory)> SetupAsync()
    {
        WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = factory.CreateClient();
        
        // Note: Full DB/Auth setup omitted as it is not currently needed for Swagger/Simple endpoint tests.
        // If future tests require it, copy SetupAsync from MiscUnverifiedTests.cs.
        
        return Task.FromResult((client, factory));
    }

    [Test]
    public async Task GetSwaggerJson_ReturnsSuccess()
    {
        (HttpClient client, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
            HttpResponseMessage response = await client.GetAsync("swagger/v1/swagger.json");
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task LatestPatch_ReturnsSuccess()
    {
        (HttpClient client, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();

            // Must seed a valid session logic because PatcherController calls ValidateAccountSessionCookie
            string cookie = "test_cookie_patcher";
            await distributedCache.SetAccountNameForSessionCookie(cookie, "PatchTester");

            Dictionary<string, string> payload = new Dictionary<string, string>
            {
                { "update", "1" },
                { "version", "0.0.0.0" },
                { "current_version", "4.10.1" },
                { "os", "wac" },
                { "arch", "x86_64" },
                { "cookie", cookie }
            };
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("patcher/patcher.php", content);
            response.EnsureSuccessStatusCode();
        }
    }
}
