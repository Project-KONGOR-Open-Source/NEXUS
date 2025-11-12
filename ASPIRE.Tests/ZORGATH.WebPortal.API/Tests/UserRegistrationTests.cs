namespace ASPIRE.Tests.Zorgath.WebPortal.API.Tests;

/// <summary>
///     Tests For User Registration Functionality
/// </summary>
public sealed class UserRegistrationTests
{
    [Test]
    [Arguments("test@kongor.com", "TestPlayer", "SecurePassword123!")]
    [Arguments("user@kongor.net", "GameUser", "MyP@ssw0rd!")]
    public async Task RegisterUserAndMainAccount_WithValidData_ReturnsCreatedAndCreatesUserAndAccount(string emailAddress, string accountName, string password)
    {
        ZORGATHServiceProvider serviceProvider = new ();

        JWTAuthenticationService jwtAuthenticationService = new (serviceProvider.WebApplicationFactory, serviceProvider.DatabaseContext);

        ILogger<EmailAddressController> emailLogger = serviceProvider.WebApplicationFactory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = serviceProvider.WebApplicationFactory.Services.GetRequiredService<IEmailService>();

        EmailAddressController emailController = new (serviceProvider.DatabaseContext, emailLogger, emailService);

        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        Token? registrationToken = await serviceProvider.DatabaseContext.Tokens.SingleOrDefaultAsync(token =>
            token.EmailAddress.Equals(emailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        if (registrationToken is null)
            throw new NullReferenceException("Registration Token Is NULL");

        ILogger<UserController> userLogger = serviceProvider.WebApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = serviceProvider.WebApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();

        UserController userController = new (serviceProvider.DatabaseContext, userLogger, emailService, configuration);

        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(registrationToken.Value.ToString(), accountName, password, password));

        await Assert.That(response).IsTypeOf<CreatedAtActionResult>();

        CreatedAtActionResult createdResult = (CreatedAtActionResult)response;
        GetBasicUserDTO? userDTO = createdResult.Value as GetBasicUserDTO;

        await Assert.That(userDTO).IsNotNull();

        if (userDTO is null)
            throw new NullReferenceException("User DTO Is NULL");

        await Assert.That(userDTO.EmailAddress).IsEqualTo(emailAddress);
        await Assert.That(userDTO.Accounts).HasCount().EqualTo(1);
        await Assert.That(userDTO.Accounts.First().Name).IsEqualTo(accountName);

        User? user = await serviceProvider.DatabaseContext.Users
            .Include(user => user.Accounts)
            .SingleOrDefaultAsync(user => user.EmailAddress.Equals(emailAddress));

        await Assert.That(user).IsNotNull();

        if (user is null)
            throw new NullReferenceException("User Is NULL");

        await Assert.That(user.Accounts).HasCount().EqualTo(1);
        await Assert.That(user.Accounts.First().Name).IsEqualTo(accountName);
        await Assert.That(user.Accounts.First().IsMain).IsTrue();

        Token? consumedToken = await serviceProvider.DatabaseContext.Tokens.FindAsync(registrationToken.ID);

        if (consumedToken is null)
            throw new NullReferenceException("Consumed Token Is NULL");

        await Assert.That(consumedToken.TimestampConsumed).IsNotNull();
    }

    [Test]
    [Arguments("mismatch@kongor.com", "MismatchUser", "Password123!", "DifferentPass123!")]
    [Arguments("test@kongor.net", "TestUser", "MyP@ss!", "WrongP@ss!")]
    public async Task RegisterUserAndMainAccount_WithMismatchedPasswords_ReturnsBadRequest(string emailAddress, string accountName, string password, string confirmPassword)
    {
        ZORGATHServiceProvider serviceProvider = new ();

        ILogger<EmailAddressController> emailLogger = serviceProvider.WebApplicationFactory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = serviceProvider.WebApplicationFactory.Services.GetRequiredService<IEmailService>();

        EmailAddressController emailController = new (serviceProvider.DatabaseContext, emailLogger, emailService);

        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        Token? registrationToken = await serviceProvider.DatabaseContext.Tokens.SingleOrDefaultAsync(token =>
            token.EmailAddress.Equals(emailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        if (registrationToken is null)
            throw new NullReferenceException("Registration Token Is NULL");

        ILogger<UserController> userLogger = serviceProvider.WebApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = serviceProvider.WebApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();

        UserController userController = new (serviceProvider.DatabaseContext, userLogger, emailService, configuration);

        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(registrationToken.Value.ToString(), accountName, password, confirmPassword));

        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }

    [Test]
    [Arguments("InvalidUser", "SecurePassword123!")]
    [Arguments("TestPlayer", "MyP@ssw0rd!")]
    public async Task RegisterUserAndMainAccount_WithInvalidToken_ReturnsNotFound(string accountName, string password)
    {
        string invalidToken = Guid.NewGuid().ToString();

        ZORGATHServiceProvider serviceProvider = new ();

        ILogger<UserController> userLogger = serviceProvider.WebApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = serviceProvider.WebApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();
        IEmailService emailService = serviceProvider.WebApplicationFactory.Services.GetRequiredService<IEmailService>();

        UserController userController = new (serviceProvider.DatabaseContext, userLogger, emailService, configuration);

        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(invalidToken, accountName, password, password));

        await Assert.That(response).IsTypeOf<NotFoundObjectResult>();
    }

    [Test]
    [Arguments("user1@kongor.com", "user2@kongor.com", "DuplicateName", "SecurePassword123!")]
    [Arguments("first@kongor.net", "second@kongor.net", "SameName", "MyP@ssw0rd!")]
    public async Task RegisterUserAndMainAccount_WithDuplicateAccountName_ReturnsConflict(string emailAddressOne, string emailAddressTwo, string accountName, string password)
    {
        ZORGATHServiceProvider serviceProvider = new ();

        JWTAuthenticationService jwtAuthenticationService = new(serviceProvider.WebApplicationFactory, serviceProvider.DatabaseContext);

        await jwtAuthenticationService.CreateAuthenticatedUser(emailAddressOne, accountName, password);

        ILogger<EmailAddressController> emailLogger = serviceProvider.WebApplicationFactory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = serviceProvider.WebApplicationFactory.Services.GetRequiredService<IEmailService>();

        EmailAddressController emailController = new (serviceProvider.DatabaseContext, emailLogger, emailService);

        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddressTwo, emailAddressTwo));

        Token? registrationToken = await serviceProvider.DatabaseContext.Tokens.SingleOrDefaultAsync(token =>
            token.EmailAddress.Equals(emailAddressTwo) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        if (registrationToken is null)
            throw new NullReferenceException("Registration Token Is NULL");

        ILogger<UserController> userLogger = serviceProvider.WebApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = serviceProvider.WebApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();

        UserController userController = new (serviceProvider.DatabaseContext, userLogger, emailService, configuration);

        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(registrationToken.Value.ToString(), accountName, password, password));

        await Assert.That(response).IsTypeOf<ConflictObjectResult>();
    }
}
