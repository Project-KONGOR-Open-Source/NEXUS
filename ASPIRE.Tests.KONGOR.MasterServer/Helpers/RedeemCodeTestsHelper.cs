namespace ASPIRE.Tests.KONGOR.MasterServer.Helpers;

/// <summary>
///     Helpers for building authenticated sessions, seeding redeemable codes, and posting store requests from <see cref="Tests.Store.RedeemCodeTests"/>.
/// </summary>
internal static class RedeemCodeTestsHelper
{
    public static async Task<(string Cookie, int AccountID, int UserID)> SeedAuthenticatedSession(KONGORIntegrationWebApplicationFactory factory, string emailAddress, string accountName, int goldCoins, int silverCoins, int plinkoTickets)
    {
        SRPAuthenticationService service = new (factory);

        (Account account, string _) = await service.CreateAccountWithSRPCredentials(emailAddress, accountName, "DoesNotMatter123!");

        int accountID = account.ID;
        int userID;

        using (IServiceScope scope = factory.Services.CreateScope())
        {
            MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

            User user = await databaseContext.Users.SingleAsync(candidate => candidate.ID == account.User.ID);

            user.GoldCoins = goldCoins;
            user.SilverCoins = silverCoins;
            user.PlinkoTickets = plinkoTickets;

            await databaseContext.SaveChangesAsync();

            userID = user.ID;
        }

        string cookie = Guid.NewGuid().ToString("N");

        IDatabase distributedCache = factory.Services.GetRequiredService<IDatabase>();

        await distributedCache.SetAccountNameForSessionCookie(cookie, accountName);

        return (cookie, accountID, userID);
    }

    public static async Task SeedCode(KONGORIntegrationWebApplicationFactory factory, string code, int goldCoinsReward, int silverCoinsReward, int plinkoTicketsReward, int? productID)
    {
        using IServiceScope scope = factory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        databaseContext.RedeemableCodes.Add(new RedeemableCode
        {
            Code                = code.ToUpperInvariant(),
            GoldCoinsReward     = goldCoinsReward,
            SilverCoinsReward   = silverCoinsReward,
            PlinkoTicketsReward = plinkoTicketsReward,
            ProductID           = productID
        });

        await databaseContext.SaveChangesAsync();
    }

    public static async Task<User> LoadUser(KONGORIntegrationWebApplicationFactory factory, int userID)
    {
        using IServiceScope scope = factory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        return await databaseContext.Users.AsNoTracking().SingleAsync(user => user.ID == userID);
    }

    public static async Task<HttpResponseMessage> PostStore(KONGORIntegrationWebApplicationFactory factory, string cookie, int accountID, string code)
    {
        HttpClient client = factory.CreateClient();

        FormUrlEncodedContent form = new (new Dictionary<string, string>
        {
            ["cookie"]          = cookie,
            ["account_id"]      = accountID.ToString(),
            ["request_code"]    = "10",
            ["code"]            = code,
            ["hostTime"]        = "123456"
        });

        return await client.PostAsync("/store_requester.php", form);
    }

    public static async Task<IDictionary<object, object>> DeserialisePhpResponse(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();

        return PhpSerialization.Deserialize(body) as IDictionary<object, object>
            ?? throw new InvalidOperationException($@"Response Body Was Not A PHP Array: ""{body}""");
    }
}
