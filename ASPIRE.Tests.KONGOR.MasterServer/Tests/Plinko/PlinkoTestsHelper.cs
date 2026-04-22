namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Plinko;

/// <summary>
///     Shared helpers between the Plinko test classes for seeding authenticated sessions and issuing HTTP requests.
/// </summary>
internal static class PlinkoTestsHelper
{
    public static async Task<(string Cookie, int UserID)> SeedAuthenticatedSession(KONGORIntegrationWebApplicationFactory factory, string emailAddress, string accountName, int goldCoins, int plinkoTickets)
    {
        SRPAuthenticationService service = new (factory);

        (Account account, string _) = await service.CreateAccountWithSRPCredentials(emailAddress, accountName, "DoesNotMatter123!");

        int userID;

        using (IServiceScope scope = factory.Services.CreateScope())
        {
            MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

            User user = await databaseContext.Users.SingleAsync(candidate => candidate.ID == account.User.ID);

            user.GoldCoins = goldCoins;
            user.PlinkoTickets = plinkoTickets;

            await databaseContext.SaveChangesAsync();

            userID = user.ID;
        }

        string cookie = Guid.NewGuid().ToString("N");

        IDatabase distributedCache = factory.Services.GetRequiredService<IDatabase>();

        await distributedCache.SetAccountNameForSessionCookie(cookie, accountName);

        return (cookie, userID);
    }

    public static async Task<User> LoadUser(KONGORIntegrationWebApplicationFactory factory, int userID)
    {
        using IServiceScope scope = factory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        return await databaseContext.Users.AsNoTracking().SingleAsync(user => user.ID == userID);
    }

    public static async Task MutateUser(KONGORIntegrationWebApplicationFactory factory, int userID, Action<User> mutation)
    {
        using IServiceScope scope = factory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        User user = await databaseContext.Users.SingleAsync(candidate => candidate.ID == userID);

        mutation(user);

        await databaseContext.SaveChangesAsync();
    }

    public static async Task<HttpResponseMessage> PostForm(KONGORIntegrationWebApplicationFactory factory, string route, Dictionary<string, string> fields)
    {
        HttpClient client = factory.CreateClient();

        FormUrlEncodedContent form = new (fields);

        return await client.PostAsync(route, form);
    }

    public static async Task<IDictionary<object, object>> DeserialisePhpResponse(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();

        return PhpSerialization.Deserialize(body) as IDictionary<object, object>
            ?? throw new InvalidOperationException($@"Response Body Was Not A PHP Array: ""{body}""");
    }
}
