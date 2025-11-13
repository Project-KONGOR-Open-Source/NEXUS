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
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = ZORGATHServiceProvider.CreateOrchestratedInstance();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        JWTAuthenticationService jwtAuthenticationService = new(webApplicationFactory);

        ILogger<EmailAddressController> emailLogger = webApplicationFactory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();

        EmailAddressController emailController = new(databaseContext, emailLogger, emailService);

        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        Token? registrationToken = await databaseContext.Tokens.SingleOrDefaultAsync(token =>
            token.EmailAddress.Equals(emailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        await Assert.That(registrationToken).IsNotNull();

        ILogger<UserController> userLogger = webApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = webApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();

        UserController userController = new(databaseContext, userLogger, emailService, configuration);

        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(registrationToken.Value.ToString(), accountName, password, password));

        await Assert.That(response).IsTypeOf<CreatedAtActionResult>();

        CreatedAtActionResult createdResult = (CreatedAtActionResult) response;
        GetBasicUserDTO? userDTO = createdResult.Value as GetBasicUserDTO;

        await Assert.That(userDTO).IsNotNull();

        using (Assert.Multiple())
        {
            await Assert.That(userDTO.EmailAddress).IsEqualTo(emailAddress);
            await Assert.That(userDTO.Accounts).HasCount().EqualTo(1);
            await Assert.That(userDTO.Accounts.First().Name).IsEqualTo(accountName);
        }

        User? user = await databaseContext.Users
            .Include(user => user.Accounts)
            .SingleOrDefaultAsync(user => user.EmailAddress.Equals(emailAddress));

        await Assert.That(user).IsNotNull();

        using (Assert.Multiple())
        {
            await Assert.That(user.Accounts).HasCount().EqualTo(1);
            await Assert.That(user.Accounts.First().Name).IsEqualTo(accountName);
            await Assert.That(user.Accounts.First().IsMain).IsTrue();
        }

        Token? consumedToken = await databaseContext.Tokens.FindAsync(registrationToken.ID);

        using (Assert.Multiple())
        {
            await Assert.That(consumedToken).IsNotNull();
            await Assert.That(consumedToken.TimestampConsumed).IsNotNull();
        }
    }

    [Test]
    [Arguments("mismatch@kongor.com", "MismatchUser", "Password123!", "DifferentPass123!")]
    [Arguments("test@kongor.net", "TestUser", "MyP@ss!", "WrongP@ss!")]
    public async Task RegisterUserAndMainAccount_WithMismatchedPasswords_ReturnsBadRequest(string emailAddress, string accountName, string password, string confirmPassword)
    {
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = ZORGATHServiceProvider.CreateOrchestratedInstance();

        ILogger<EmailAddressController> emailLogger = webApplicationFactory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        EmailAddressController emailController = new (databaseContext, emailLogger, emailService);

        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        Token? registrationToken = await databaseContext.Tokens.SingleOrDefaultAsync(token =>
            token.EmailAddress.Equals(emailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        await Assert.That(registrationToken).IsNotNull();

        ILogger<UserController> userLogger = webApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = webApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration);

        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(registrationToken.Value.ToString(), accountName, password, confirmPassword));

        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }

    [Test]
    [Arguments("InvalidUser", "SecurePassword123!")]
    [Arguments("TestPlayer", "MyP@ssw0rd!")]
    public async Task RegisterUserAndMainAccount_WithInvalidToken_ReturnsNotFound(string accountName, string password)
    {
        string invalidToken = Guid.CreateVersion7().ToString();

        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = ZORGATHServiceProvider.CreateOrchestratedInstance();

        ILogger<UserController> userLogger = webApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = webApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration);

        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(invalidToken, accountName, password, password));

        await Assert.That(response).IsTypeOf<NotFoundObjectResult>();
    }

    [Test]
    [Arguments("user1@kongor.com", "user2@kongor.com", "DuplicateName", "SecurePassword123!")]
    [Arguments("first@kongor.net", "second@kongor.net", "SameName", "MyP@ssw0rd!")]
    public async Task RegisterUserAndMainAccount_WithDuplicateAccountName_ReturnsConflict(string emailAddressOne, string emailAddressTwo, string accountName, string password)
    {
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = ZORGATHServiceProvider.CreateOrchestratedInstance();

        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        await jwtAuthenticationService.CreateAuthenticatedUser(emailAddressOne, accountName, password);

        ILogger<EmailAddressController> emailLogger = webApplicationFactory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        EmailAddressController emailController = new (databaseContext, emailLogger, emailService);

        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddressTwo, emailAddressTwo));

        Token? registrationToken = await databaseContext.Tokens.SingleOrDefaultAsync(token =>
            token.EmailAddress.Equals(emailAddressTwo) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        await Assert.That(registrationToken).IsNotNull();

        ILogger<UserController> userLogger = webApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = webApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration);

        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(registrationToken.Value.ToString(), accountName, password, password));

        await Assert.That(response).IsTypeOf<ConflictObjectResult>();
    }

    [Test]
    [Arguments("tokenreuse@kongor.com", "TokenUser", "SecurePassword123!")]
    [Arguments("consumed@kongor.net", "ConsumedUser", "MyP@ssw0rd!")]
    public async Task RegisterUserAndMainAccount_WithConsumedToken_ReturnsUnauthorizedOrConflict(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = ZORGATHServiceProvider.CreateOrchestratedInstance();

        ILogger<EmailAddressController> emailLogger = webApplicationFactory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        EmailAddressController emailController = new (databaseContext, emailLogger, emailService);

        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        Token? registrationToken = await databaseContext.Tokens.SingleOrDefaultAsync(token =>
            token.EmailAddress.Equals(emailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        await Assert.That(registrationToken).IsNotNull();

        string tokenValue = registrationToken.Value.ToString();

        ILogger<UserController> userLogger = webApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = webApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration);

        // First Registration Should Succeed And Consume The Token
        IActionResult firstResponse = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(tokenValue, accountName, password, password));

        await Assert.That(firstResponse).IsTypeOf<CreatedAtActionResult>();

        // Token Should Be Marked As Consumed
        Token? consumedToken = await databaseContext.Tokens.FindAsync(registrationToken.ID);

        await Assert.That(consumedToken).IsNotNull();

        await Assert.That(consumedToken.TimestampConsumed).IsNotNull();

        // May Return UnauthorizedObjectResult (Invalid Token) Or ConflictObjectResult (If Email Already Registered)
        IActionResult secondResponse = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(tokenValue, $"{accountName}2", password, password));

        bool isUnauthorizedOrConflict = secondResponse is UnauthorizedObjectResult or ConflictObjectResult;

        await Assert.That(isUnauthorizedOrConflict).IsTrue();
    }

    [Test]
    [Arguments("Case@Kongor.Com", "CaseUser", "SecurePassword123!")]
    [Arguments("TEST@KONGOR.NET", "TESTUSER", "MyP@ssw0rd!")]
    public async Task RegisterUserAndMainAccount_WithMixedCaseAccountName_ShouldAllowDifferentCases(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = ZORGATHServiceProvider.CreateOrchestratedInstance();

        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        // Register First Account With Original Case
        await jwtAuthenticationService.CreateAuthenticatedUser(emailAddress, accountName, password);

        ILogger<EmailAddressController> emailLogger = webApplicationFactory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        string secondEmail = $"second{emailAddress}";

        EmailAddressController emailController = new (databaseContext, emailLogger, emailService);

        // Try To Register Second Account With Different Case (Should Succeed If Case-Insensitive Or Fail If Case-Sensitive)
        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(secondEmail, secondEmail));

        Token? registrationToken = await databaseContext.Tokens.SingleOrDefaultAsync(token =>
            token.EmailAddress.Equals(secondEmail) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        await Assert.That(registrationToken).IsNotNull();

        ILogger<UserController> userLogger = webApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = webApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration);

        string differentCaseAccountName = accountName.ToLowerInvariant();

        // May Return ConflictObjectResult (If Case-Sensitive) Or CreatedAtActionResult (If Case-Insensitive)
        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(registrationToken.Value.ToString(), differentCaseAccountName, password, password));

        bool isConflictOrCreated = response is ConflictObjectResult or CreatedAtActionResult;

        await Assert.That(isConflictOrCreated).IsTrue();
    }
}
