namespace ASPIRE.Tests.ZORGATH.WebPortal.API;

using Infrastructure;

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
        await using ServiceProvider services = new ();

        ILogger<EmailAddressController> logger = services.Factory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = services.Factory.Services.GetRequiredService<IEmailService>();

        EmailAddressController controller = new (services.MerrickContext, logger, emailService);

        IActionResult response = await controller.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        await Assert.That(response).IsTypeOf<OkObjectResult>();

        Token? token = await services.MerrickContext.Tokens.SingleOrDefaultAsync(token =>
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
        await using ServiceProvider services = new ();

        ILogger<EmailAddressController> logger = services.Factory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = services.Factory.Services.GetRequiredService<IEmailService>();

        EmailAddressController controller = new (services.MerrickContext, logger, emailService);

        IActionResult response = await controller.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, confirmEmailAddress));

        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }

    [Test]
    [Arguments("duplicate@kongor.com")]
    [Arguments("existing@kongor.net")]
    public async Task RegisterEmailAddress_WhenAlreadyRegistered_ReturnsBadRequest(string emailAddress)
    {
        await using ServiceProvider services = new ();

        ILogger<EmailAddressController> logger = services.Factory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = services.Factory.Services.GetRequiredService<IEmailService>();

        EmailAddressController controller = new (services.MerrickContext, logger, emailService);

        await controller.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        IActionResult response = await controller.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }
}
