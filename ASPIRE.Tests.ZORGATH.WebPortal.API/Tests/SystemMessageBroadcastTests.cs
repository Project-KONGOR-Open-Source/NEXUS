namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Tests;

/// <summary>
///     Tests for the administration endpoint that broadcasts a system message to every account's inbox.
/// </summary>
public sealed class SystemMessageBroadcastTests(ZORGATHIntegrationWebApplicationFactory webApplicationFactory)
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().InitialiseAsync();

    [Test]
    public async Task Broadcasting_A_System_Message_Adds_It_To_Every_Account()
    {
        JWTAuthenticationService authentication = new (webApplicationFactory);

        await authentication.CreateAuthenticatedUser("broadcast.one@kongor.com", "BroadcastOne", "SecurePassword123!");
        await authentication.CreateAuthenticatedUser("broadcast.two@kongor.com", "BroadcastTwo", "SecurePassword123!");

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
        ILogger<AdministrationController> logger = scope.ServiceProvider.GetRequiredService<ILogger<AdministrationController>>();

        AdministrationController controller = new (databaseContext, logger);

        IActionResult response = await controller.BroadcastSystemMessage(new BroadcastSystemMessageDTO("Server Maintenance", "<p>The servers will be down at midnight.</p>", null, null, null));

        await Assert.That(response).IsTypeOf<OkObjectResult>();

        using (Assert.Multiple())
        {
            await Assert.That((await GetMessages("BroadcastOne")).Any(message => message.Subject.Equals("Server Maintenance"))).IsTrue();
            await Assert.That((await GetMessages("BroadcastTwo")).Any(message => message.Subject.Equals("Server Maintenance"))).IsTrue();
        }
    }

    private async Task<List<Message>> GetMessages(string accountName)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        Account account = await databaseContext.Accounts.SingleAsync(record => record.Name.Equals(accountName));

        return await databaseContext.Messages.AsNoTracking().Where(message => message.AccountID == account.ID).ToListAsync();
    }
}
