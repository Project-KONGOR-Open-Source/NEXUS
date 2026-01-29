using KONGOR.MasterServer.Extensions.Cache;
using MERRICK.DatabaseContext.Constants;
using MERRICK.DatabaseContext.Entities.Core;
using MERRICK.DatabaseContext.Entities.Utility;
using MERRICK.DatabaseContext.Enumerations;
using MERRICK.DatabaseContext.Persistence;

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
            MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

            string cookie = "storage_cookie";
            string accountName = "StorageUser";

            // Seed DB
            User user = new()
            {
                EmailAddress = "storage@kongor.net",
                PBKDF2PasswordHash = "hash",
                SRPPasswordHash = "hash",
                SRPPasswordSalt = "salt",
                Role = new Role { Name = UserRoles.User }
            };

            Account account = new()
            {
                Name = accountName,
                User = user,
                Type = AccountType.Staff,
                IsMain = true,
                Cookie = cookie,
                UseCloud = true,
                AutomaticCloudUpload = true,
                BackupLastUpdatedTime = DateTimeOffset.Parse("2023-01-01 12:00:00")
            };

            await dbContext.Users.AddAsync(user);
            await dbContext.Accounts.AddAsync(account);
            await dbContext.SaveChangesAsync();

            await distributedCache.SetAccountNameForSessionCookie(cookie, accountName);

            Dictionary<string, string> payload = new() { { "cookie", cookie } };
            FormUrlEncodedContent content = new(payload);

            HttpResponseMessage response = await client.PostAsync("master/storage/status", content);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            await Assert.That(responseBody).Contains("cloud_storage_info");
            await Assert.That(responseBody).Contains("success");

            // Verify dynamic values
            await Assert.That(responseBody).Contains("s:9:\"use_cloud\";s:1:\"1\";");
            await Assert.That(responseBody).Contains("s:16:\"cloud_autoupload\";s:1:\"1\";");
            // Check formatted date
            await Assert.That(responseBody).Contains("2023-01-01 12:00:00");
        }
    }
}