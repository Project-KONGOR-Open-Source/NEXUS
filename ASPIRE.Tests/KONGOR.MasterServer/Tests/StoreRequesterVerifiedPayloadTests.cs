namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

using EntityRole = Role;

public sealed class StoreRequesterVerifiedPayloadTests
{
    private static Dictionary<string, string> StoreRequestPayload(string cookie)
    {
        // Based on user logs:
        // category_id=58&request_code=1&page=1&cookie=...&hostTime=...&displayAll=false&notPurchasable=false&bb=
        return new Dictionary<string, string>
        {
            { "cookie", cookie },
            { "category_id", "58" },
            { "request_code", "1" },
            { "page", "1" },
            { "hostTime", "956218" },
            { "displayAll", "false" },
            { "notPurchasable", "false" },
            { "bb", "" }
        };
    }

    [Test]
    public async Task StoreRequest_ReturnsSuccess()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory =
            KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = webApplicationFactory.CreateClient();
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();
        MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
        IDatabase distributedCache = scope.ServiceProvider.GetRequiredService<IDatabase>();

        string cookie = Guid.NewGuid().ToString("N");
        User user = new()
        {
            EmailAddress = "store@kongor.net",
            PBKDF2PasswordHash = "h",
            SRPPasswordHash = "h",
            SRPPasswordSalt = "s",
            Role = new EntityRole { Name = UserRoles.User }
        };
        Account account = new()
        {
            Name = "StoreUser",
            User = user,
            Type = AccountType.Normal,
            IsMain = true,
            Cookie = cookie
        };
        await dbContext.Users.AddAsync(user);
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();

        // Seed session in Redis if needed, but StoreRequester often just checks DB by cookie
        // Some methods use Redis, let's play safe? 
        // Logic usually: StoreRequesterController.Handle -> validates cookie from DB or Cache.

        Dictionary<string, string> payload = StoreRequestPayload(cookie);
        FormUrlEncodedContent content = new(payload);

        HttpResponseMessage response = await client.PostAsync("store_requester.php", content);
        response.EnsureSuccessStatusCode();
    }
}