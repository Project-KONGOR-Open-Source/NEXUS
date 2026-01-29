using System.Net.Http.Headers;

using KONGOR.MasterServer.Extensions.Cache;

namespace ASPIRE.Tests.KONGOR.MasterServer;

public sealed class StorageControllerTests
{
    private async Task<(HttpClient Client, MerrickContext DbContext, IDatabase Cache, string Cookie, WebApplicationFactory<KONGORAssemblyMarker> Factory, string StoragePath)> SetupAsync(string? accountName = null)
    {
        string testId = Guid.NewGuid().ToString("N");
        string relativeStoragePath = $"App_Data/TestCloud/{testId}";

        WebApplicationFactory<KONGORAssemblyMarker> factory = KONGORServiceProvider.CreateOrchestratedInstance(testId)
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Storage:CloudPath", relativeStoragePath);
            });

        HttpClient client = factory.CreateClient();
        IServiceScope scope = factory.Services.CreateScope();
        MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
        IDatabase cache = scope.ServiceProvider.GetRequiredService<IDatabase>();

        string cookie = Guid.NewGuid().ToString("N");
        string name = accountName ?? $"TestUser_{Guid.NewGuid().ToString("N")[..8]}";

        User user = new()
        {
            EmailAddress = $"{name}@kongor.net",
            PBKDF2PasswordHash = "hash",
            SRPPasswordHash = "hash",
            SRPPasswordSalt = "salt",
            Role = new Role { Name = UserRoles.User }
        };

        Account account = new()
        {
            Name = name,
            User = user,
            Type = AccountType.Staff,
            IsMain = true,
            Cookie = cookie
        };

        await dbContext.Users.AddAsync(user);
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();

        await cache.SetAccountNameForSessionCookie(cookie, account.Name);

        return (client, dbContext, cache, cookie, factory, relativeStoragePath);
    }

    [Test]
    public async Task Store_ReturnsSuccess()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory, string storagePath) = await SetupAsync("StoreUser");
        await using (factory)
        {
            try
            {
                MultipartFormDataContent content = new MultipartFormDataContent();
                content.Add(new StringContent(cookie), "cookie");
                content.Add(new StringContent("hon-game-configs"), "bucket");
                content.Add(new StringContent("2024-01-01 12:00:00"), "file_modify_time");

                ByteArrayContent fileContent = new ByteArrayContent(new byte[] { 0, 1, 2, 3 });
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                content.Add(fileContent, "cloud.zip", "cloud.zip");

                HttpResponseMessage response = await client.PostAsync("/master/storage/store", content);

                response.EnsureSuccessStatusCode();
                string responseString = await response.Content.ReadAsStringAsync();
                    
                // Assert using Regex to avoid deserialization issues with custom attributes
                await Assert.That(responseString).Contains("success");
                await Assert.That(responseString).Contains("b:1"); // Boolean true in PHP
            }
            finally
            {
                // Cleanup
                IWebHostEnvironment env = factory.Services.GetRequiredService<IWebHostEnvironment>();
                string fullPath = Path.Combine(env.ContentRootPath, storagePath);
                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath, true);
                }
            }
        }
    }
    
    [Test]
    public async Task Retrieve_ReturnsFile()
    {
        (HttpClient client, _, _, string cookie, WebApplicationFactory<KONGORAssemblyMarker> factory, string storagePath) = await SetupAsync("RetrieveUser");
        await using (factory)
        {
            try
            {
                // First Store a file
                MultipartFormDataContent storeContent = new MultipartFormDataContent();
                storeContent.Add(new StringContent(cookie), "cookie");
                storeContent.Add(new StringContent("hon-game-configs"), "bucket");
                storeContent.Add(new StringContent("2024-01-01 12:00:00"), "file_modify_time");
                
                byte[] fileBytes = new byte[] { 0, 1, 2, 3, 4, 5 };
                ByteArrayContent fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                storeContent.Add(fileContent, "cloud.zip", "cloud.zip");
                
                HttpResponseMessage storeResponse = await client.PostAsync("/master/storage/store", storeContent);
                storeResponse.EnsureSuccessStatusCode();

                // Act
                MultipartFormDataContent retrieveContent = new MultipartFormDataContent();
                retrieveContent.Add(new StringContent(cookie), "cookie");
                retrieveContent.Add(new StringContent("hon-game-configs"), "bucket");

                HttpResponseMessage response = await client.PostAsync("/master/storage/retrieve", retrieveContent);

                response.EnsureSuccessStatusCode();
                byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                
                await Assert.That(bytes).IsNotNull();
                await Assert.That(bytes.Length).IsEqualTo(fileBytes.Length);
                await Assert.That(bytes).IsEquivalentTo(fileBytes);
            }
            finally
            {
                // Cleanup
                IWebHostEnvironment env = factory.Services.GetRequiredService<IWebHostEnvironment>();
                string fullPath = Path.Combine(env.ContentRootPath, storagePath);
                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath, true);
                }
            }
        }
    }
}
