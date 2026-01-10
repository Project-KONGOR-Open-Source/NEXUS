using KONGOR.MasterServer.Extensions.Cache;

namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

public sealed class CookieTests
{
    [Test]
    public async Task Logout_RemovesSession_ReturnsTrue()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory =
            KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient httpClient = webApplicationFactory.CreateClient();

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();
        IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();

        // 1. Set Cookie
        string cookie = "logout_cookie";
        await distributedCache.SetAccountNameForSessionCookie(cookie, "LogoutUser");

        // 2. Request
        Dictionary<string, string> formData = new() { ["f"] = "logout", ["cookie"] = cookie };
        FormUrlEncodedContent content = new(formData);

        // 3. Act
        HttpResponseMessage response = await httpClient.PostAsync("client_requester.php", content);

        // 4. Assert
        string responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"[TEST] Logout Response: {responseBody}");

        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        // PHP serialize(true) is "b:1;"
        await Assert.That(responseBody).Contains("b:1;");

        // Verify cookie is gone
        string? accountName = await distributedCache.GetAccountNameForSessionCookie(cookie);
        await Assert.That(accountName).IsNull();
    }

    [Test]
    public async Task Aids2Cookie_OnServerRequester_ReturnsOk()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory =
            KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient httpClient = webApplicationFactory.CreateClient();

        // 1. Request
        Dictionary<string, string> formData = new() { ["cookie"] = "any_cookie_value" };
        FormUrlEncodedContent content = new(formData);

        // 2. Act
        HttpResponseMessage response = await httpClient.PostAsync("server_requester.php?f=aids2cookie", content);

        // 3. Assert
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
    }
}