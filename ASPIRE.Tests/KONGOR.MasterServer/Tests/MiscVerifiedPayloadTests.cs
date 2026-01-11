using KONGOR.MasterServer.Extensions.Cache;

namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

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

            string responseBody = await response.Content.ReadAsStringAsync();
            await Assert.That(responseBody).Contains("openapi");
        }
    }


    [Test]
    public async Task MessageList_ReturnsSuccess()
    {
        (HttpClient client, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
            HttpResponseMessage response = await client.PostAsync("message/list/59", null);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            await Assert.That(responseBody).Contains("a:0:{}");
        }
    }

    [Test]
    public async Task StorageStatus_ReturnsSuccess()
    {
        (HttpClient client, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();

            string cookie = "storage_cookie";
            await distributedCache.SetAccountNameForSessionCookie(cookie, "StorageUser");

            Dictionary<string, string> payload = new() { { "cookie", cookie } };
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("master/storage/status", content);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            await Assert.That(responseBody).Contains("cloud_storage_info");
            await Assert.That(responseBody).Contains("success");
        }
    }
}