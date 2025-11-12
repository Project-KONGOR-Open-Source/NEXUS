namespace ASPIRE.Tests.Zorgath.WebPortal.API.Tests;

/// <summary>
///     Tests For Email Address Registration Functionality
/// </summary>
public sealed class EmailAddressRegistrationTests
{
    [Test]
    [Arguments("test@kongor.com")]
    [Arguments("user@kongor.net")]
    public async Task RegisterEmailAddress_WithValidEmailAddress_ReturnsOkAndCreatesToken(string emailAddress)
    {
        WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = await ZORGATHServiceProvider.CreateOrchestratedInstance();

        ILogger<EmailAddressController> logger = webApplicationFactory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        EmailAddressController controller = new (databaseContext, logger, emailService);

        IActionResult response = await controller.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        await Assert.That(response).IsTypeOf<OkObjectResult>();

        Token? token = await databaseContext.Tokens.SingleOrDefaultAsync(token =>
            token.EmailAddress.Equals(emailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        await Assert.That(token).IsNotNull();

        if (token is null)
            throw new NullReferenceException("Token Is NULL");

        await Assert.That(token.EmailAddress).IsEqualTo(emailAddress);
        await Assert.That(token.Purpose).IsEqualTo(TokenPurpose.EmailAddressVerification);
        await Assert.That(token.TimestampConsumed).IsNull();
    }

    [Test]
    [Arguments("test@kongor.com", "different@kongor.com")]
    [Arguments("user@kongor.net", "typo@kongor.net")]
    public async Task RegisterEmailAddress_WithMismatchedConfirmation_ReturnsBadRequest(string emailAddress, string confirmEmailAddress)
    {
        WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = await ZORGATHServiceProvider.CreateOrchestratedInstance();

        ILogger<EmailAddressController> logger = webApplicationFactory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        EmailAddressController controller = new (databaseContext, logger, emailService);

        IActionResult response = await controller.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, confirmEmailAddress));

        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }

    [Test]
    [Arguments("duplicate@kongor.com")]
    [Arguments("existing@kongor.net")]
    public async Task RegisterEmailAddress_WhenAlreadyRegistered_ReturnsBadRequest(string emailAddress)
    {
        WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = await ZORGATHServiceProvider.CreateOrchestratedInstance();

        ILogger<EmailAddressController> logger = webApplicationFactory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        EmailAddressController controller = new (databaseContext, logger, emailService);

        await controller.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        IActionResult response = await controller.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }
}
