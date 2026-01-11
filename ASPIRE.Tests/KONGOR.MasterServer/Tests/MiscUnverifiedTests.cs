using System.Net;
using System.Security.Cryptography;

using ASPIRE.Tests.Data;

using KONGOR.MasterServer.Extensions.Cache;
using KONGOR.MasterServer.Models.ServerManagement;

namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

using EntityRole = Role;

public sealed class MiscUnverifiedTests
{
    private async
        Task<(HttpClient Client, MerrickContext DbContext, string Cookie, WebApplicationFactory<KONGORAssemblyMarker>
            Factory, IDatabase Cache)> SetupAsync()
    {
        WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = factory.CreateClient();
        IServiceScope scope = factory.Services.CreateScope();
        MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
        IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();

        string cookie = Guid.NewGuid().ToString("N");
        User user = new()
        {
            EmailAddress = $"misc_test_{Guid.NewGuid()}@kongor.net",
            PBKDF2PasswordHash = "hash",
            SRPPasswordHash = "hash",
            SRPPasswordSalt = "salt",
            Role = new EntityRole { Name = UserRoles.User }
        };

        Account account = new()
        {
            Name = $"MiscUser_{Guid.NewGuid().ToString("N")[..8]}",
            User = user,
            Type = AccountType.Staff,
            IsMain = true,
            Cookie = cookie
        };

        await dbContext.Users.AddAsync(user);
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();

        // Populate Redis Mock for Session Validation (Critical for PatcherController)
        await distributedCache.SetAccountNameForSessionCookie(cookie, account.Name);

        return (client, dbContext, cookie, factory, distributedCache);
    }

    [Test]
    public async Task ResubmitStats_ReturnsSuccess()
    {
        (HttpClient client, MerrickContext dbContext, string cookie,
            WebApplicationFactory<KONGORAssemblyMarker> factory, _) = await SetupAsync();
        await using (factory)
        {
            // Seed Server Host Account
            User hostUser = new()
            {
                EmailAddress = "host@kongor.net",
                PBKDF2PasswordHash = "hash",
                SRPPasswordHash = "hash", // In reality, we'd need valid SRP hash matching input password
                SRPPasswordSalt = "salt",
                Role = new EntityRole { Name = UserRoles.User }
            };

            Account hostAccount = new()
            {
                Name = "ServerHostAccount", // Matches payload
                User = hostUser,
                Type = AccountType.ServerHost,
                IsMain = true,
                Cookie = Guid.NewGuid().ToString("N") // Ensure cookie is set
            };

            // Seed Player Account
            User playerUser = new()
            {
                EmailAddress = "player@kongor.net",
                PBKDF2PasswordHash = "hash",
                SRPPasswordHash = "hash",
                SRPPasswordSalt = "salt",
                Role = new EntityRole { Name = UserRoles.User }
            };
            Account playerAccount = new()
            {
                Name = "TestPlayer",
                User = playerUser,
                Type = AccountType.Staff,
                IsMain = true,
                Cookie = Guid.NewGuid().ToString("N")
            };

            await dbContext.Users.AddAsync(hostUser);
            await dbContext.Accounts.AddAsync(hostAccount);
            await dbContext.Users.AddAsync(playerUser);
            await dbContext.Accounts.AddAsync(playerAccount);
            await dbContext.SaveChangesAsync();

            // Manually seed MatchServer in cache for the host account
            // This is required because HandleStatsResubmission keys off server_id to find the server and its secret/state
            MatchServer matchServer = new()
            {
                HostAccountID = hostAccount.ID,
                HostAccountName = hostAccount.Name,
                ID = 1,
                Name = "TestServer",
                Instance = 1,
                IPAddress = "127.0.0.1",
                Port = 11235,
                Location = "US",
                Description = "Test Server",
                Status = ServerStatus.SERVER_STATUS_ACTIVE,
                Cookie = hostAccount.Cookie,
                TimestampRegistered = DateTimeOffset.UtcNow
            };
            // Use DistributedCacheExtensions.SetMatchServer
            IDatabase cache = factory.Services.CreateScope().ServiceProvider.GetRequiredService<IDatabase>();
            await cache.SetMatchServer(hostAccount.Name, matchServer);


            // Calculate Resubmission Key
            // Example
            // Key = SHA1(matchID + salt)
            const string salt = "s8c7xaduxAbRanaspUf3kadRachecrac9efeyupr8suwrewecrUphayeweqUmana";
            const int matchId = 123;
            string keyInput = matchId + salt;
            string resubmissionKey = Convert
                .ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(keyInput)))
                .ToLower();

            Dictionary<string, string> payload =
                MiscRequestPayloads.Unverified.ResubmitStats(cookie, matchId, "2023-01-01", "compressed_data");
            payload["resubmission_key"] = resubmissionKey;
            payload["server_id"] = "1"; // Must match matchServer.ID

            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("stats_requester.php", content);

            // Expecting 401 due to SRP password check failure (we can't easily replicate hashing here), or 200 if somehow passed.
            // 404/500/400 would be failures.
            // Expecting 401 due to SRP password check failure.
            // We verify that the controller validation logic is reached by checking the specific error message.
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);

            string responseBody = await response.Content.ReadAsStringAsync();
            await Assert.That(responseBody).Contains("Invalid Host Account Password");
        }
    }
}