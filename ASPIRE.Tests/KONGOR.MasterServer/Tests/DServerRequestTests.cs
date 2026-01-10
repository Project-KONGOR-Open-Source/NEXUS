using System.Net;

using ASPIRE.Tests.Data;

using KONGOR.MasterServer.Extensions.Cache;
using KONGOR.MasterServer.Models.ServerManagement;

namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

using EntityRole = Role;
using ServerManagementStatus = ServerStatus;

public sealed class DServerRequestTests
{
    private async
        Task<(HttpClient Client, MerrickContext DbContext, IDatabase Cache, string Cookie, Account Account,
            WebApplicationFactory<KONGORAssemblyMarker> Factory)> SetupAsync(string? accountName = null,
            ServerManagementStatus initialStatus = ServerManagementStatus.SERVER_STATUS_UNKNOWN)
    {
        WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = factory.CreateClient();
        IServiceScope scope = factory.Services.CreateScope();
        MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
        IDatabase cache = scope.ServiceProvider.GetRequiredService<IDatabase>();

        string cookie = Guid.NewGuid().ToString("N");
        string name = accountName ?? $"ServerTest_{Guid.NewGuid().ToString("N")[..8]}";

        User user = new()
        {
            EmailAddress = $"{name}@kongor.net",
            PBKDF2PasswordHash = "hash",
            SRPPasswordHash = "hash",
            SRPPasswordSalt = "salt",
            Role = new EntityRole { Name = UserRoles.Administrator } // Admin/Staff often needed for server ops
        };

        Account account = new()
        {
            Name = name,
            User = user,
            Type = AccountType.ServerHost, // Distinctive for server ops
            IsMain = true,
            Cookie = cookie
        };

        await dbContext.Users.AddAsync(user);
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();

        // Ensure session cookie map
        await cache.SetAccountNameForSessionCookie(cookie, account.Name);

        // Pre-seed MatchServer just in case needed (often tied to account name)
        MatchServer matchServer = new()
        {
            HostAccountID = user.ID,
            HostAccountName = account.Name,
            ID = 1,
            Name = "Test Server",
            Instance = 1,
            IPAddress = "127.0.0.1",
            Port = 11235,
            Location = "US",
            Description = "Test Server Description",
            Cookie = cookie,
            Status = initialStatus
        };
        await cache.SetMatchServer(matchServer.HostAccountName, matchServer);

        return (client, dbContext, cache, cookie, account, factory);
    }

    #region From ServerRequesterUnverifiedTests.cs

    [Test]
    public async Task ReplayAuth_Unverified_Returns401()
    {
        (HttpClient client, _, _, _, Account account, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync();
        await using (factory)
        {
            // Unverified payload with bad password -> 401
            Dictionary<string, string> payload = ServerRequestPayloads.Verified.ReplayAuth(account.Name, "password");
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("server_requester.php", content);

            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        }
    }

    [Test]
    public async Task NewSession_Unverified_Returns401()
    {
        (HttpClient client, _, _, _, Account account, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync();
        await using (factory)
        {
            string login = $"{account.Name}:1";
            Dictionary<string, string> payload = ServerRequestPayloads.Verified.NewSession(
                login,
                "password",
                11235,
                "New Server",
                "Description",
                "US",
                "127.0.0.1"
            );
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("server_requester.php", content);

            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        }
    }

    [Test]
    public async Task Aids2Cookie_Unverified_ReturnsOK()
    {
        (HttpClient client, _, _, _, _, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
            Dictionary<string, string> payload =
                ServerRequestPayloads.Unverified.Aids2Cookie("123", "127.0.0.1", "bad_hash");
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("server_requester.php", content);

            // Stubbed -> OK
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        }
    }

    #endregion

    #region From ServerRequesterVerifiedPayloadTests.cs

    [Test]
    public async Task SetOnline_Verified_UpdatesStatus()
    {
        (HttpClient client, _, IDatabase cache, string cookie, _, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync("ServerHost");
        await using (factory)
        {
            // Simulate 'LOADING' status update
            Dictionary<string, string> payload = ServerRequestPayloads.Verified.SetOnline(
                cookie,
                1,
                num_conn: 5,
                cgt: 123456,
                c_state: "SERVER_STATUS_LOADING",
                prev_c_state: "SERVER_STATUS_UNKNOWN"
            );

            FormUrlEncodedContent content = new(payload);
            HttpResponseMessage response = await client.PostAsync("server_requester.php", content);

            response.EnsureSuccessStatusCode();

            MatchServer? cachedServer = await cache.GetMatchServerBySessionCookie(cookie);
            await Assert.That(cachedServer).IsNotNull();
            await Assert.That(cachedServer!.Status).IsEqualTo(ServerManagementStatus.SERVER_STATUS_LOADING);
        }
    }

    [Test]
    public async Task Heartbeat_Verified_UpdatesLastSeen()
    {
        // Start as LOADING
        (HttpClient client, _, IDatabase cache, string cookie, _, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync("HeartbeatHost", ServerManagementStatus.SERVER_STATUS_LOADING);
        await using (factory)
        {
            // Heartbeat sets status to ACTIVE by default (via set_online logic)
            Dictionary<string, string> payload = ServerRequestPayloads.Verified.Heartbeat(cookie);
            FormUrlEncodedContent content = new(payload);
            HttpResponseMessage response = await client.PostAsync("server_requester.php", content);

            response.EnsureSuccessStatusCode();

            MatchServer? updatedServer = await cache.GetMatchServerBySessionCookie(cookie);
            await Assert.That(updatedServer).IsNotNull();
            await Assert.That(updatedServer.Status).IsEqualTo(ServerManagementStatus.SERVER_STATUS_ACTIVE);
        }
    }

    [Test]
    public async Task GetSpectatorHeader_Verified_ReturnsSuccess()
    {
        (HttpClient client, _, _, _, _, WebApplicationFactory<KONGORAssemblyMarker> factory) = await SetupAsync();
        await using (factory)
        {
            Dictionary<string, string> payload = ServerRequestPayloads.Verified.GetSpectatorHeader();
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("server_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task ClientConnection_Verified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, Account account, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync("ConnUser");
        await using (factory)
        {
            Dictionary<string, string> payload =
                ServerRequestPayloads.Verified.ClientConnection(cookie, "127.0.0.1", account.ID);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("server_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task AcceptKey_Verified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, Account account, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync("AcceptUser");
        await using (factory)
        {
            Dictionary<string, string> payload = ServerRequestPayloads.Verified.AcceptKey(cookie, account.ID);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("server_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task GetQuickStats_Verified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, _, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync();
        await using (factory)
        {
            Dictionary<string, string> payload = ServerRequestPayloads.Verified.GetQuickStats(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("server_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task Shutdown_Verified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, _, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync("ShutdownHost");
        await using (factory)
        {
            Dictionary<string, string> payload = ServerRequestPayloads.Verified.Shutdown(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("server_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task StartGame_Verified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, Account account, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync("StartGameHost", ServerManagementStatus.SERVER_STATUS_IDLE);
        await using (factory)
        {
            Dictionary<string, string> payload = ServerRequestPayloads.Verified.StartGame(cookie, 12345);
            // Payload mstr default needs to match seeded host name
            payload["mstr"] = account.Name + ":";

            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("server_requester.php", content);
            response.EnsureSuccessStatusCode();
        }
    }

    [Test]
    public async Task Aids2Cookie_Verified_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, _, WebApplicationFactory<KONGORAssemblyMarker> factory) =
            await SetupAsync("AidsUser");
        await using (factory)
        {
            // Aids2Cookie usually validates the cookie exists. 
            // Our SetupAsync ensures cookie maps to account name in cache.
            Dictionary<string, string> payload = ServerRequestPayloads.Verified.Aids2Cookie(cookie);
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("server_requester.php", content);

            // Returns empty 200 OK
            response.EnsureSuccessStatusCode();
        }
    }

    #endregion
}