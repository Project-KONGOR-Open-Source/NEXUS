namespace ASPIRE.Tests.ZORGATH.WebPortal.API;

using Infrastructure;

/// <summary>
///     Tests For Email Address Registration Functionality
/// </summary>
public sealed class EmailAddressRegistrationTests
{
    [Test]
    public async Task RegisterEmailAddress_WithValidEmailAddress_ReturnsOkAndCreatesToken()
    {
        const string emailAddress = "new.user@kongor.net";

        await using ServiceProvider services = new ();

        ILogger<EmailAddressController> logger = services.Factory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = services.Factory.Services.GetRequiredService<IEmailService>();

        EmailAddressController controller = new (services.MerrickContext, logger, emailService);

        IActionResult response = await controller.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        await Assert.That(response).IsTypeOf<OkObjectResult>();

        Token? token = await services.MerrickContext.Tokens.SingleOrDefaultAsync(token =>
            token.EmailAddress.Equals(emailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        await Assert.That(token).IsNotNull();
        await Assert.That(token!.EmailAddress).IsEqualTo(emailAddress);
        await Assert.That(token.Purpose).IsEqualTo(TokenPurpose.EmailAddressVerification);
        await Assert.That(token.TimestampConsumed).IsNull();
    }

    [Test]
    public async Task RegisterEmailAddress_WithMismatchedConfirmation_ReturnsBadRequest()
    {
        const string emailAddress = "new.user@kongor.net";
        const string confirmEmailAddress = "different@kongor.com";

        await using ServiceProvider services = new ();

        ILogger<EmailAddressController> logger = services.Factory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = services.Factory.Services.GetRequiredService<IEmailService>();

        EmailAddressController controller = new (services.MerrickContext, logger, emailService);

        IActionResult response = await controller.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, confirmEmailAddress));

        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }

    [Test]
    public async Task RegisterEmailAddress_WhenAlreadyRegistered_ReturnsBadRequest()
    {
        const string emailAddress = "new.user@kongor.net";

        await using ServiceProvider services = new ();

        ILogger<EmailAddressController> logger = services.Factory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = services.Factory.Services.GetRequiredService<IEmailService>();

        EmailAddressController controller = new (services.MerrickContext, logger, emailService);

        await controller.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        IActionResult response = await controller.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }
}
