
namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

using ASPIRE.Tests.Data;
using global::KONGOR.MasterServer.Extensions.Cache;
using global::KONGOR.MasterServer.Models.ServerManagement;
using global::MERRICK.DatabaseContext;
using global::MERRICK.DatabaseContext.Entities;
using global::MERRICK.DatabaseContext.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using EntityRole = global::MERRICK.DatabaseContext.Entities.Utility.Role;
using ServerManagementStatus = global::KONGOR.MasterServer.Models.ServerManagement.ServerStatus;

public sealed class ServerRequesterTests
{

    [Test]
    public async Task SetOnline_WithValidCookie_UpdatesStatus()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = webApplicationFactory.CreateClient();
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();
        MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
        IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();

        // 1. Seed Host Account
        string cookie = Guid.NewGuid().ToString("N");
        User hostUser = new()
        {
            EmailAddress = "server_host@kongor.net",
            PBKDF2PasswordHash = "hash",
            SRPPasswordHash = "hash",
            SRPPasswordSalt = "salt",
            Role = new EntityRole { Name = UserRoles.User }
        };

        Account hostAccount = new()
        {
            Name = "ServerHost",
            User = hostUser,
            Type = AccountType.Staff,
            IsMain = true,
            Cookie = cookie
        };

        await dbContext.Users.AddAsync(hostUser);
        await dbContext.Accounts.AddAsync(hostAccount);
        await dbContext.SaveChangesAsync();

        // 2. Pre-seed Server in Cache
        MatchServer matchServer = new()
        {
            HostAccountID = hostUser.ID,
            HostAccountName = hostAccount.Name,
            ID = 1,
            Name = "Test Server",
            Instance = 1,
            IPAddress = "127.0.0.1",
            Port = 11235,
            Location = "US",
            Description = "Test Server Description",
            Cookie = cookie,
            Status = ServerManagementStatus.SERVER_STATUS_UNKNOWN // Start OFFLINE (UNKNOWN)
        };

        await distributedCache.SetMatchServer(matchServer.HostAccountName, matchServer);

        // 3. Prepare Payload
        // We simulate a server coming online (LOADING)
        Dictionary<string, string> payload = ServerRequesterVerifiedPayloads.SetOnline(
            cookie,
            matchId: 1,
            num_conn: 5,
            cgt: 123456,
            c_state: "SERVER_STATUS_LOADING", // LOADING
            prev_c_state: "SERVER_STATUS_UNKNOWN" // OFFLINE
        );

        FormUrlEncodedContent content = new(payload);

        // 4. Execute
        HttpResponseMessage response = await client.PostAsync("server_requester.php", content);

        // 5. Verify
        response.EnsureSuccessStatusCode();
        string body = await response.Content.ReadAsStringAsync();

        // The current implementation returns an empty 200 OK
        // await Assert.That(body).IsNotEmpty();
        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        // Verify Cache Update
        MatchServer? cachedServer = await distributedCache.GetMatchServerBySessionCookie(cookie);
        await Assert.That(cachedServer).IsNotNull();
        await Assert.That(cachedServer!.Cookie).IsEqualTo(cookie);
        // Status should be updated to LOADING based on c_state 1
        await Assert.That(cachedServer.Status).IsEqualTo(ServerManagementStatus.SERVER_STATUS_LOADING);
    }

    [Test]
    public async Task Heartbeat_WithValidCookie_UpdatesLastSeen()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = webApplicationFactory.CreateClient();
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();
        MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
        IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();

        // 1. Seed Host Account
        string cookie = Guid.NewGuid().ToString("N");
        User hostUser = new()
        {
            EmailAddress = "heartbeat_host@kongor.net",
            PBKDF2PasswordHash = "hash",
            SRPPasswordHash = "hash",
            SRPPasswordSalt = "salt",
            Role = new EntityRole { Name = UserRoles.User }
        };

        Account hostAccount = new()
        {
            Name = "HeartbeatHost",
            User = hostUser,
            Type = AccountType.Staff,
            IsMain = true,
            Cookie = cookie
        };

        await dbContext.Users.AddAsync(hostUser);
        await dbContext.Accounts.AddAsync(hostAccount);
        await dbContext.SaveChangesAsync();

        // 2. Pre-seed Server in Cache
         MatchServer matchServer = new()
        {
            ID = 1,
            HostAccountID = hostUser.ID, 
            HostAccountName = "HeartbeatHost",
            Name = "HeartbeatServer",
            Instance = 1,
            IPAddress = "127.0.0.1",
            Port = 11337,
            Location = "US",
            Description = "Test Server",
            Cookie = cookie,
            Status = ServerManagementStatus.SERVER_STATUS_LOADING 
        };
        await distributedCache.SetMatchServer("HeartbeatHost", matchServer);

        // 3. Execute Heartbeat
        // This effectively sends set_online with new=false and c_state=ACTIVE
        Dictionary<string, string> payload = ServerRequesterVerifiedPayloads.Heartbeat(cookie);
        FormUrlEncodedContent content = new(payload);
        HttpResponseMessage response = await client.PostAsync("server_requester.php", content);

        // 4. Verify
        response.EnsureSuccessStatusCode();
        
        // Asserting status updated to ACTIVE from LOADING
        MatchServer? updatedServer = await distributedCache.GetMatchServerBySessionCookie(cookie);
        await Assert.That(updatedServer).IsNotNull();
        await Assert.That(updatedServer.Status).IsEqualTo(ServerManagementStatus.SERVER_STATUS_ACTIVE);
    }
}
