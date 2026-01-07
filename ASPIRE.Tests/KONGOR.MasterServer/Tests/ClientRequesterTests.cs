namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

using ASPIRE.Tests.Data;
using MERRICK.DatabaseContext;
using MERRICK.DatabaseContext.Entities;
using MERRICK.DatabaseContext.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using EntityRole = MERRICK.DatabaseContext.Entities.Utility.Role;

public sealed class ClientRequesterTests
{
    [Test]
    public async Task GetInitStats_WithValidCookie_ReturnsStats()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = webApplicationFactory.CreateClient();
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();
        MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        // 1. Seed Account
        string cookie = Guid.NewGuid().ToString("N");
        User user = new()
        {
            EmailAddress = "client_test@kongor.net",
            PBKDF2PasswordHash = "hash",
            SRPPasswordHash = "hash",
            SRPPasswordSalt = "salt",
            Role = new EntityRole { Name = UserRoles.User }
        };

        Account account = new()
        {
            Name = "ClientTester",
            User = user,
            Type = AccountType.Staff,
            IsMain = true,
            Cookie = cookie
        };

        await dbContext.Users.AddAsync(user);
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();

        // 2. Prepare Payload
        Dictionary<string, string> payload = ClientRequesterVerifiedPayloads.GetInitStats(cookie);
        FormUrlEncodedContent content = new(payload);

        // 3. Execute
        HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
        
        // 4. Verify
        response.EnsureSuccessStatusCode();
        string body = await response.Content.ReadAsStringAsync();
        
        // TUnit Assertions
        await Assert.That(body).Contains("nickname");
        await Assert.That(body).Contains("ClientTester");
        await Assert.That(body).Contains("slot_id");
    }

    [Test]
    public async Task GetProducts_WithValidCookie_ReturnsProductList()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = webApplicationFactory.CreateClient();
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();
        MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        // 1. Seed Account
        string cookie = Guid.NewGuid().ToString("N");
        User user = new()
        {
            EmailAddress = "products_test@kongor.net",
            PBKDF2PasswordHash = "hash",
            SRPPasswordHash = "hash",
            SRPPasswordSalt = "salt",
            Role = new EntityRole { Name = UserRoles.User }
        };

        Account account = new()
        {
            Name = "ProductTester",
            User = user,
            Type = AccountType.Staff,
            IsMain = true,
            Cookie = cookie
        };

        await dbContext.Users.AddAsync(user);
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();

        // 2. Prepare Payload
        Dictionary<string, string> payload = ClientRequesterVerifiedPayloads.GetProducts(cookie);
        FormUrlEncodedContent content = new(payload);

        // 3. Execute
        HttpResponseMessage response = await client.PostAsync("client_requester.php", content);

        // 4. Verify
        string body = await response.Content.ReadAsStringAsync();
        await Assert.That(body).IsNotEmpty();
    }
}
