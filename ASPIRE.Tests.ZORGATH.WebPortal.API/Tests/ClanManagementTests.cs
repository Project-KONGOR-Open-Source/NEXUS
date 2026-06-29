namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Tests;

/// <summary>
///     Tests for the clan management endpoints, covering setting a clan's title and logo (restricted to leaders and officers).
/// </summary>
public sealed class ClanManagementTests(ZORGATHIntegrationWebApplicationFactory webApplicationFactory)
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().InitialiseAsync();

    [Test]
    public async Task Set_Clan_Details_As_A_Leader_Persists_The_Title_And_Logo()
    {
        JWTAuthenticationData authentication = await new JWTAuthenticationService(webApplicationFactory)
            .CreateAuthenticatedUser("clan.leader@kongor.com", "ClanLeader", "SecurePassword123!");

        int accountID = await PlaceAccountInClan(authentication.AccountName, ClanTier.Leader, "Leader Clan", "LDR");

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        ClanController controller = CreateController(scope, authentication.EmailAddress, accountID);

        IActionResult response = await controller.SetClanDetails(new SetClanDetailsDTO("For Glory And Honour", "emblem_42"));

        await Assert.That(response).IsTypeOf<OkObjectResult>();

        Account account = await ReadAccountWithClan(accountID);

        using (Assert.Multiple())
        {
            await Assert.That(account.Clan?.Title).IsEqualTo("For Glory And Honour");
            await Assert.That(account.Clan?.Logo).IsEqualTo("emblem_42");
        }
    }

    [Test]
    public async Task Set_Clan_Details_As_A_Member_Is_Forbidden()
    {
        JWTAuthenticationData authentication = await new JWTAuthenticationService(webApplicationFactory)
            .CreateAuthenticatedUser("clan.member@kongor.com", "ClanMember", "SecurePassword123!");

        int accountID = await PlaceAccountInClan(authentication.AccountName, ClanTier.Member, "Member Clan", "MBR");

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        ClanController controller = CreateController(scope, authentication.EmailAddress, accountID);

        IActionResult response = await controller.SetClanDetails(new SetClanDetailsDTO("Nice Try", "sneaky"));

        await Assert.That(response).IsTypeOf<ForbidResult>();
    }

    private static ClanController CreateController(IServiceScope scope, string emailAddress, int accountID)
    {
        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
        ILogger<ClanController> logger = scope.ServiceProvider.GetRequiredService<ILogger<ClanController>>();

        ClaimsPrincipal principal = new (new ClaimsIdentity(
        [
            new Claim(Claims.Email, emailAddress),
            new Claim(Claims.AccountID, accountID.ToString())
        ], "Test"));

        return new ClanController(databaseContext, logger)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } }
        };
    }

    private async Task<int> PlaceAccountInClan(string accountName, ClanTier tier, string clanName, string clanTag)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account account = await databaseContext.Accounts.SingleAsync(record => record.Name.Equals(accountName));

        account.Clan = new Clan { Name = clanName, Tag = clanTag };
        account.ClanTier = tier;

        await databaseContext.SaveChangesAsync();

        return account.ID;
    }

    private async Task<Account> ReadAccountWithClan(int accountID)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        return await databaseContext.Accounts.Include(record => record.Clan).SingleAsync(record => record.ID.Equals(accountID));
    }
}
