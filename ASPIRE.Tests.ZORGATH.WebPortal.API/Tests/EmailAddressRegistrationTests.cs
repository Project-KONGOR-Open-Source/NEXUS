namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Tests;

/// <summary>
///     Tests for email address registration.
/// </summary>
public sealed class EmailAddressRegistrationTests(ZORGATHIntegrationWebApplicationFactory webApplicationFactory)
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().InitialiseAsync();

    [Test]
    [Arguments("test@kongor.com")]
    [Arguments("user@kongor.net")]
    public async Task RegisterEmailAddress_WithValidEmailAddress_ReturnsOKAndCreatesTokenAndDeliversEmail(string emailAddress)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        ILogger<EmailAddressController> logger = scope.ServiceProvider.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        EmailAddressController controller = new (databaseContext, logger, emailService, hostEnvironment);

        IActionResult response = await controller.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        await Assert.That(response).IsTypeOf<OkObjectResult>();

        Token? token = await databaseContext.Tokens.SingleOrDefaultAsync(candidate =>
            candidate.EmailAddress.Equals(emailAddress) && candidate.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        await Assert.That(token).IsNotNull();

        using (Assert.Multiple())
        {
            await Assert.That(token.EmailAddress).IsEqualTo(emailAddress);
            await Assert.That(token.Purpose).IsEqualTo(TokenPurpose.EmailAddressVerification);
            await Assert.That(token.TimestampConsumed).IsNull();
        }

        // Verify The Controller Actually Dispatched A Registration-Link Email, With The Token Available To The Recipient So They Can Complete Verification
        IReadOnlyList<RecordedEmail> delivered = webApplicationFactory.GetInMemoryEmailService().GetRecordedFor(emailAddress);

        await Assert.That(delivered.Count).IsEqualTo(1);

        RecordedEmail recorded = delivered.Single();

        using (Assert.Multiple())
        {
            await Assert.That(recorded.Kind).IsEqualTo(EmailKind.EmailAddressRegistrationLink);
            await Assert.That(recorded.Recipient).IsEqualTo(emailAddress);
            await Assert.That(recorded.Parameters["Token"]).IsEqualTo(token.Value.ToString());
        }
    }

    [Test]
    [Arguments("test@kongor.com", "different@kongor.com")]
    [Arguments("user@kongor.net", "typo@kongor.net")]
    public async Task RegisterEmailAddress_WithMismatchedConfirmation_ReturnsBadRequest(string emailAddress, string confirmEmailAddress)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        ILogger<EmailAddressController> logger = scope.ServiceProvider.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        EmailAddressController controller = new (databaseContext, logger, emailService, hostEnvironment);

        IActionResult response = await controller.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, confirmEmailAddress));

        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }

    [Test]
    [Arguments("duplicate@kongor.com")]
    [Arguments("existing@kongor.net")]
    public async Task RegisterEmailAddress_WhenAlreadyRegistered_ReturnsBadRequest(string emailAddress)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        ILogger<EmailAddressController> logger = scope.ServiceProvider.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        EmailAddressController controller = new (databaseContext, logger, emailService, hostEnvironment);

        await controller.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        IActionResult response = await controller.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }
}
