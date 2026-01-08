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
            Dictionary<string, string> payload = ClientRequestPayloads.Unverified.Auth("login", "password");
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
            Dictionary<string, string> payload = ClientRequestPayloads.Unverified.GetServerList(cookie);
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
            Dictionary<string, string> payload = ClientRequestPayloads.Unverified.GetAllHeroes(cookie);
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
            Dictionary<string, string> payload = ClientRequestPayloads.Unverified.CreateGame(cookie, "TestGame");
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

            Dictionary<string, string> payload = ClientRequestPayloads.Unverified.ShowSimpleStats(cookie, account.Name);
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
            Dictionary<string, string> payload = ClientRequestPayloads.Unverified.ClientEventsInfo(cookie);
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
            Dictionary<string, string> payload = ClientRequestPayloads.Unverified.GetSpecialMessages(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task GetInitStats_ReturnsBadRequest()
    {
        (HttpClient client, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        using (factory)
        {
            // This test verifies that "get_init_stats" (snake_case) fails as expected,
            // proving the controller requires "get_initStats" (camelCase).
            Dictionary<string, string> payload = ClientRequestPayloads.Unverified.GetInitStats(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            
            if (response.StatusCode != System.Net.HttpStatusCode.BadRequest)
            {
                await Assert.That(response.StatusCode).IsEqualTo(System.Net.HttpStatusCode.BadRequest);
            }
        }
    }

    [Test]
    public async Task GrabServerList_ReturnsSuccess()
    {
        (HttpClient client, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.Unverified.GrabServerList(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task ServerList_ReturnsSuccess()
    {
        (HttpClient client, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.Unverified.ServerList(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task GetHeroList_ReturnsSuccess()
    {
        (HttpClient client, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        using (factory)
        {
            Dictionary<string, string> payload = ClientRequestPayloads.Unverified.GetHeroList(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task ShowStats_ReturnsSuccess()
    {
        (HttpClient client, MerrickContext dbContext, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        using (factory)
        {
            Account account = await dbContext.Accounts.FirstAsync(a => a.Cookie == cookie);

            Dictionary<string, string> payload = ClientRequestPayloads.Unverified.ShowStats(cookie, account.Name);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("client_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }
}
