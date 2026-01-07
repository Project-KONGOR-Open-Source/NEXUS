namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

using global::KONGOR.MasterServer.Extensions.Cache;
using global::MERRICK.DatabaseContext;
using global::MERRICK.DatabaseContext.Entities;
using global::MERRICK.DatabaseContext.Entities.Utility;
using global::MERRICK.DatabaseContext.Enumerations;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

public sealed partial class CookieTests
{
    [Test]
    public async Task Aids2Cookie_WithValidSession_ReturnsAccountId()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient httpClient = webApplicationFactory.CreateClient();

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();
        MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
        IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();

        // 1. Seed Account
        Account account = new()
        {
            Name = "CookieMonster",
            User = new User
            {
                EmailAddress = "cookie@monster.com",
                PBKDF2PasswordHash = "hash",
                SRPPasswordHash = "hash",
                SRPPasswordSalt = "salt",
                Role = new global::MERRICK.DatabaseContext.Entities.Utility.Role { Name = UserRoles.User }
            },
            Type = AccountType.Normal,
            IsMain = true
        };

        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();

        // 2. Set Cookie
        string cookie = "yummy_cookie";
        await distributedCache.SetAccountNameForSessionCookie(cookie, account.Name);

        // 3. Request
        Dictionary<string, string> formData = new()
        {
            ["f"] = "aids2cookie",
            ["cookie"] = cookie
        };
        FormUrlEncodedContent content = new(formData);

        // 4. Act
        HttpResponseMessage response = await httpClient.PostAsync("client_requester.php", content);

        // 5. Assert
        string responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"[TEST] Response: {responseBody}");
        
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        await Assert.That(responseBody).Contains($"i:{account.ID};");
    }

    [Test]
    public async Task Logout_RemovesSession_ReturnsTrue()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient httpClient = webApplicationFactory.CreateClient();

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();
        IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();

        // 1. Set Cookie
        string cookie = "logout_cookie";
        await distributedCache.SetAccountNameForSessionCookie(cookie, "LogoutUser");

        // 2. Request
        Dictionary<string, string> formData = new()
        {
            ["f"] = "logout",
            ["cookie"] = cookie
        };
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
    public async Task Aids2Cookie_OnServerRequester_ReturnsAccountId()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient httpClient = webApplicationFactory.CreateClient();

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();
        MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
        IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();

        // 1. Seed Account
        Account account = new()
        {
            Name = "ServerCookieMonster",
            User = new User
            {
                EmailAddress = "server_cookie@monster.com",
                PBKDF2PasswordHash = "hash",
                SRPPasswordHash = "hash",
                SRPPasswordSalt = "salt",
                Role = new global::MERRICK.DatabaseContext.Entities.Utility.Role { Name = UserRoles.User }
            },
            Type = AccountType.Normal,
            IsMain = true
        };

        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();

        // 2. Set Cookie
        string cookie = "server_yummy_cookie";
        await distributedCache.SetAccountNameForSessionCookie(cookie, account.Name);

        // 3. Request (Using Query for 'f' and Form for 'cookie' to match log scenario)
        Dictionary<string, string> formData = new()
        {
            ["cookie"] = cookie
        };
        FormUrlEncodedContent content = new(formData);

        // 4. Act
        HttpResponseMessage response = await httpClient.PostAsync("server_requester.php?f=aids2cookie", content);

        // 5. Assert
        string responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"[TEST] Server Response: {responseBody}");
        
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        await Assert.That(responseBody).Contains($"i:{account.ID};");
    }
}
