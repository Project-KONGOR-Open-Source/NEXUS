namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

using ASPIRE.Tests.Data;
using ASPIRE.Tests.KONGOR.MasterServer.Infrastructure;
using KONGOR.MasterServer;
using MERRICK.DatabaseContext;
using MERRICK.DatabaseContext.Entities;
using MERRICK.DatabaseContext.Enumerations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
// using NUnit.Framework; // Removed
using EntityRole = MERRICK.DatabaseContext.Entities.Utility.Role;

public sealed class ClientRequesterUnverifiedTests
{
    private async Task<(HttpClient Client, MerrickContext DbContext, string Cookie, WebApplicationFactory<KONGORAssemblyMarker> Factory)> SetupAsync()
    {
        WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = factory.CreateClient();
        IServiceScope scope = factory.Services.CreateScope();
        MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        // Seed Account
        string cookie = Guid.NewGuid().ToString("N");
        User user = new()
        {
            EmailAddress = $"test_{Guid.NewGuid()}@kongor.net",
            PBKDF2PasswordHash = "hash",
            SRPPasswordHash = "hash",
            SRPPasswordSalt = "salt",
            Role = new EntityRole { Name = UserRoles.User }
        };

        Account account = new()
        {
            Name = $"TestUser_{Guid.NewGuid().ToString("N")[..8]}",
            User = user,
            Type = AccountType.Staff,
            IsMain = true,
            Cookie = cookie
        };

        await dbContext.Users.AddAsync(user);
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();

        return (client, dbContext, cookie, factory);
    }

    [Test]
    public async Task Auth_ReturnsSuccess()
    {
        (HttpClient client, _, _, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        using (factory)
        {
            // Note: Auth typically requires real credentials, this payload is unverified/placeholder
            Dictionary<string, string> payload = ClientRequesterUnverifiedPayloads.Auth("login", "password");
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php?f=auth", content);
            // Auth is disabled by design (returns 400 with specific error), so we assert BadRequest to confirm reachability and correct "disabled" behavior.
            if (response.StatusCode != System.Net.HttpStatusCode.BadRequest)
            {
                await Assert.That(response.StatusCode).IsEqualTo(System.Net.HttpStatusCode.BadRequest);
            }
        }
    }

    [Test]
    public async Task GetServerList_ReturnsSuccess()
    {
        (HttpClient client, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        using (factory)
        {
            Dictionary<string, string> payload = ClientRequesterUnverifiedPayloads.GetServerList(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task GetAllHeroes_ReturnsSuccess()
    {
        (HttpClient client, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        using (factory)
        {
            Dictionary<string, string> payload = ClientRequesterUnverifiedPayloads.GetAllHeroes(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task CreateGame_ReturnsSuccess()
    {
        (HttpClient client, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        using (factory)
        {
            Dictionary<string, string> payload = ClientRequesterUnverifiedPayloads.CreateGame(cookie, "TestGame");
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task ShowSimpleStats_ReturnsSuccess()
    {
        (HttpClient client, MerrickContext dbContext, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        using (factory)
        {
            // We can query our own seeded account
            // We need to fetch the account name associated with the cookie from the DB setup
            // SetupAsync creates a random name, so we must retrieve it.
            Account account = await dbContext.Accounts.FirstAsync(a => a.Cookie == cookie);

            Dictionary<string, string> payload = ClientRequesterUnverifiedPayloads.ShowSimpleStats(cookie, account.Name);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task ClientEventsInfo_ReturnsSuccess()
    {
        (HttpClient client, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        using (factory)
        {
            Dictionary<string, string> payload = ClientRequesterUnverifiedPayloads.ClientEventsInfo(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }
    
    [Test]
    public async Task GetSpecialMessages_ReturnsSuccess()
    {
        (HttpClient client, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        using (factory)
        {
            Dictionary<string, string> payload = ClientRequesterUnverifiedPayloads.GetSpecialMessages(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }
}
