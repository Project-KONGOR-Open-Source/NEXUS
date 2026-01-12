namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Tests;

/// <summary>
///     Tests For User Registration Functionality
/// </summary>
public sealed class UserRegistrationTests
{
    [Test]
    [Arguments("test@kongor.com", "TestPlayer", "SecurePassword123!")]
    [Arguments("user@kongor.net", "GameUser", "MyP@ssw0rd!")]
    public async Task RegisterUserAndMainAccount_WithValidData_ReturnsCreatedAndCreatesUserAndAccount(
        string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory =
            ZORGATHServiceProvider.CreateOrchestratedInstance();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        ILogger<EmailAddressController> emailLogger =
            webApplicationFactory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = webApplicationFactory.Services.GetRequiredService<IWebHostEnvironment>();

        EmailAddressController emailController = new(databaseContext, emailLogger, emailService, hostEnvironment);

        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        Token? registrationToken = await databaseContext.Tokens.SingleOrDefaultAsync(token =>
            token.EmailAddress.Equals(emailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        await Assert.That(registrationToken).IsNotNull();

        ILogger<UserController> userLogger =
            webApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration =
            webApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();
        IWebHostEnvironment userHostEnvironment =
            webApplicationFactory.Services.GetRequiredService<IWebHostEnvironment>();

        UserController userController =
            new(databaseContext, userLogger, emailService, configuration, userHostEnvironment);

        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(registrationToken.Value.ToString(), accountName, password, password));

        await Assert.That(response).IsTypeOf<CreatedAtActionResult>();

        CreatedAtActionResult createdResult = (CreatedAtActionResult) response;
        GetBasicUserDTO? userDTO = createdResult.Value as GetBasicUserDTO;

        await Assert.That(userDTO).IsNotNull();

        using (Assert.Multiple())
        {
            await Assert.That(userDTO.EmailAddress).IsEqualTo(emailAddress);
            await Assert.That(userDTO.Accounts).Count().IsEqualTo(1);
            await Assert.That(userDTO.Accounts.First().Name).IsEqualTo(accountName);
        }

        User? user = await databaseContext.Users
            .Include(user => user.Accounts)
            .SingleOrDefaultAsync(user => user.EmailAddress.Equals(emailAddress));

        await Assert.That(user).IsNotNull();

        using (Assert.Multiple())
        {
            await Assert.That(user.Accounts).Count().IsEqualTo(1);
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
    public async Task RegisterUserAndMainAccount_WithMismatchedPasswords_ReturnsBadRequest(string emailAddress,
        string accountName, string password, string confirmPassword)
    {
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory =
            ZORGATHServiceProvider.CreateOrchestratedInstance();

        ILogger<EmailAddressController> emailLogger =
            webApplicationFactory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();
        IWebHostEnvironment emailHostEnvironment =
            webApplicationFactory.Services.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        EmailAddressController emailController = new(databaseContext, emailLogger, emailService, emailHostEnvironment);

        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        Token? registrationToken = await databaseContext.Tokens.SingleOrDefaultAsync(token =>
            token.EmailAddress.Equals(emailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        await Assert.That(registrationToken).IsNotNull();

        ILogger<UserController> userLogger =
            webApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration =
            webApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();
        IWebHostEnvironment userHostEnvironment =
            webApplicationFactory.Services.GetRequiredService<IWebHostEnvironment>();

        UserController userController =
            new(databaseContext, userLogger, emailService, configuration, userHostEnvironment);

        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(registrationToken.Value.ToString(), accountName, password,
                confirmPassword));

        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }

    [Test]
    [Arguments("InvalidUser", "SecurePassword123!")]
    [Arguments("TestPlayer", "MyP@ssw0rd!")]
    public async Task RegisterUserAndMainAccount_WithInvalidToken_ReturnsNotFound(string accountName, string password)
    {
        string invalidToken = Guid.CreateVersion7().ToString();

        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory =
            ZORGATHServiceProvider.CreateOrchestratedInstance();

        ILogger<UserController> userLogger =
            webApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration =
            webApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();
        IWebHostEnvironment secondHostEnvironment =
            webApplicationFactory.Services.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        UserController userController =
            new(databaseContext, userLogger, emailService, configuration, secondHostEnvironment);

        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(invalidToken, accountName, password, password));

        await Assert.That(response).IsTypeOf<NotFoundObjectResult>();
    }

    [Test]
    [Arguments("user1@kongor.com", "user2@kongor.com", "DuplicateName", "SecurePassword123!")]
    [Arguments("first@kongor.net", "second@kongor.net", "SameName", "MyP@ssw0rd!")]
    public async Task RegisterUserAndMainAccount_WithDuplicateAccountName_ReturnsConflict(string emailAddressOne,
        string emailAddressTwo, string accountName, string password)
    {
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory =
            ZORGATHServiceProvider.CreateOrchestratedInstance();

        JWTAuthenticationService jwtAuthenticationService = new(webApplicationFactory);

        await jwtAuthenticationService.CreateAuthenticatedUser(emailAddressOne, accountName, password);

        ILogger<EmailAddressController> emailLogger =
            webApplicationFactory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();
        IWebHostEnvironment emailHostEnvironment =
            webApplicationFactory.Services.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        EmailAddressController emailController = new(databaseContext, emailLogger, emailService, emailHostEnvironment);

        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddressTwo, emailAddressTwo));

        Token? registrationToken = await databaseContext.Tokens.SingleOrDefaultAsync(token =>
            token.EmailAddress.Equals(emailAddressTwo) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        await Assert.That(registrationToken).IsNotNull();

        ILogger<UserController> userLogger =
            webApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration =
            webApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();
        IWebHostEnvironment userHostEnvironment =
            webApplicationFactory.Services.GetRequiredService<IWebHostEnvironment>();

        UserController userController =
            new(databaseContext, userLogger, emailService, configuration, userHostEnvironment);

        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(registrationToken.Value.ToString(), accountName, password, password));

        await Assert.That(response).IsTypeOf<ConflictObjectResult>();
    }

    [Test]
    [Arguments("token.reuse@kongor.com", "TokenUser", "SecurePassword123!")]
    [Arguments("token.consumed@kongor.net", "ConsumedUser", "MyP@ssw0rd!")]
    public async Task RegisterUserAndMainAccount_WithConsumedToken_ReturnsConflict(string emailAddress,
        string accountName, string password)
    {
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory =
            ZORGATHServiceProvider.CreateOrchestratedInstance();

        ILogger<EmailAddressController> emailLogger =
            webApplicationFactory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();
        IWebHostEnvironment emailHostEnvironment =
            webApplicationFactory.Services.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        EmailAddressController emailController = new(databaseContext, emailLogger, emailService, emailHostEnvironment);

        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        Token? registrationToken = await databaseContext.Tokens.SingleOrDefaultAsync(token =>
            token.EmailAddress.Equals(emailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        await Assert.That(registrationToken).IsNotNull();

        string tokenValue = registrationToken.Value.ToString();

        ILogger<UserController> userLogger =
            webApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration =
            webApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();
        IWebHostEnvironment userHostEnvironment =
            webApplicationFactory.Services.GetRequiredService<IWebHostEnvironment>();

        UserController userController =
            new(databaseContext, userLogger, emailService, configuration, userHostEnvironment);

        // First Registration Should Succeed And Consume The Token
        IActionResult firstResponse = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(tokenValue, accountName, password, password));

        await Assert.That(firstResponse).IsTypeOf<CreatedAtActionResult>();

        // Token Should Be Marked As Consumed
        Token? consumedToken = await databaseContext.Tokens.FindAsync(registrationToken.ID);

        await Assert.That(consumedToken).IsNotNull();

        await Assert.That(consumedToken.TimestampConsumed).IsNotNull();

        // Second Registration Attempt With Consumed Token Should Return Conflict Because Email Already Exists
        IActionResult secondResponse = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(tokenValue, $"{accountName}2", password, password));

        await Assert.That(secondResponse).IsTypeOf<ConflictObjectResult>();
    }

    [Test]
    [Arguments("case.one@kongor.com", "case.two@kongor.com", "CaseUser", "CaseUser", "SecurePassword123!")]
    [Arguments("TEST-ONE@KONGOR.NET", "TEST-TWO@KONGOR.NET", "TESTUSER", "TESTUSER", "MyP@ssw0rd!")]
    public async Task RegisterUserAndMainAccount_WithExactCaseAccountName_ReturnsConflict(string firstEmailAddress,
        string secondEmailAddress, string firstAccountName, string secondAccountName, string password)
    {
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory =
            ZORGATHServiceProvider.CreateOrchestratedInstance();

        JWTAuthenticationService jwtAuthenticationService = new(webApplicationFactory);

        // Register First Account With Original Case
        await jwtAuthenticationService.CreateAuthenticatedUser(firstEmailAddress, firstAccountName, password);

        ILogger<EmailAddressController> emailLogger =
            webApplicationFactory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();
        IWebHostEnvironment emailHostEnvironment =
            webApplicationFactory.Services.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        EmailAddressController emailController = new(databaseContext, emailLogger, emailService, emailHostEnvironment);

        // Try To Register Second Account With Same Case
        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(secondEmailAddress, secondEmailAddress));

        Token? registrationToken = await databaseContext.Tokens.SingleOrDefaultAsync(token =>
            token.EmailAddress.Equals(secondEmailAddress) &&
            token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        await Assert.That(registrationToken).IsNotNull();

        ILogger<UserController> userLogger =
            webApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration =
            webApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();
        IWebHostEnvironment userHostEnvironment =
            webApplicationFactory.Services.GetRequiredService<IWebHostEnvironment>();

        UserController userController =
            new(databaseContext, userLogger, emailService, configuration, userHostEnvironment);

        // Second Registration Attempt With Duplicate Account Name Should Return Conflict
        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(registrationToken.Value.ToString(), secondAccountName, password,
                password));

        await Assert.That(response).IsTypeOf<ConflictObjectResult>();
    }

    [Test]
    [Arguments("case.one@kongor.com", "case.two@kongor.com", "CaseUser", "caseuser", "SecurePassword123!")]
    [Arguments("TEST-ONE@KONGOR.NET", "TEST-TWO@KONGOR.NET", "TESTUSER", "testuser", "MyP@ssw0rd!")]
    public async Task RegisterUserAndMainAccount_WithDifferentCaseAccountName_ReturnsCreated(string firstEmailAddress,
        string secondEmailAddress, string firstAccountName, string secondAccountName, string password)
    {
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory =
            ZORGATHServiceProvider.CreateOrchestratedInstance();

        JWTAuthenticationService jwtAuthenticationService = new(webApplicationFactory);

        // Register First Account With Original Case
        await jwtAuthenticationService.CreateAuthenticatedUser(firstEmailAddress, firstAccountName, password);

        ILogger<EmailAddressController> emailLogger =
            webApplicationFactory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();
        IWebHostEnvironment emailHostEnvironment =
            webApplicationFactory.Services.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        EmailAddressController emailController = new(databaseContext, emailLogger, emailService, emailHostEnvironment);

        // Try To Register Second Account With Different Case (Account Names Are Case-Sensitive)
        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(secondEmailAddress, secondEmailAddress));

        Token? registrationToken = await databaseContext.Tokens.SingleOrDefaultAsync(token =>
            token.EmailAddress.Equals(secondEmailAddress) &&
            token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        await Assert.That(registrationToken).IsNotNull();

        ILogger<UserController> userLogger =
            webApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration =
            webApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();
        IWebHostEnvironment userHostEnvironment =
            webApplicationFactory.Services.GetRequiredService<IWebHostEnvironment>();

        UserController userController =
            new(databaseContext, userLogger, emailService, configuration, userHostEnvironment);

        // Second Registration Attempt With Duplicate Account Name Should Return Conflict
        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(registrationToken.Value.ToString(), secondAccountName, password,
                password));

        await Assert.That(response).IsTypeOf<CreatedAtActionResult>();
    }
}