namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Tests;

/// <summary>
///     Tests for user registration.
/// </summary>
public sealed class UserRegistrationTests(ZORGATHIntegrationWebApplicationFactory webApplicationFactory)
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().InitialiseAsync();

    [Test]
    [Arguments("test@kongor.com", "TestPlayer", "SecurePassword123!")]
    [Arguments("user@kongor.net", "GameUser", "MyP@ssw0rd!")]
    public async Task RegisterUserAndMainAccount_WithValidData_ReturnsCreatedAndCreatesUserAndAccount(string emailAddress, string accountName, string password)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        ILogger<EmailAddressController> emailLogger = scope.ServiceProvider.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        EmailAddressController emailController = new (databaseContext, emailLogger, emailService, hostEnvironment);

        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        Token? registrationToken = await databaseContext.Tokens.SingleOrDefaultAsync(candidate =>
            candidate.EmailAddress.Equals(emailAddress) && candidate.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        await Assert.That(registrationToken).IsNotNull();

        ILogger<UserController> userLogger = scope.ServiceProvider.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = scope.ServiceProvider.GetRequiredService<IOptions<OperationalConfiguration>>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration, hostEnvironment);

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
            await Assert.That(userDTO.Accounts.Single().Name).IsEqualTo(accountName);
        }

        User? user = await databaseContext.Users
            .Include(candidate => candidate.Accounts)
            .SingleOrDefaultAsync(candidate => candidate.EmailAddress.Equals(emailAddress));

        await Assert.That(user).IsNotNull();

        SignupRewards signupRewards = JSONConfiguration.EconomyConfiguration.SignupRewards;

        using (Assert.Multiple())
        {
            await Assert.That(user.Accounts).Count().IsEqualTo(1);
            await Assert.That(user.Accounts.Single().Name).IsEqualTo(accountName);
            await Assert.That(user.Accounts.Single().IsMain).IsTrue();
            await Assert.That(user.GoldCoins).IsEqualTo(signupRewards.GoldCoins);
            await Assert.That(user.SilverCoins).IsEqualTo(signupRewards.SilverCoins);
            await Assert.That(user.PlinkoTickets).IsEqualTo(signupRewards.PlinkoTickets);
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
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        ILogger<EmailAddressController> emailLogger = scope.ServiceProvider.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        EmailAddressController emailController = new (databaseContext, emailLogger, emailService, hostEnvironment);

        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        Token? registrationToken = await databaseContext.Tokens.SingleOrDefaultAsync(candidate =>
            candidate.EmailAddress.Equals(emailAddress) && candidate.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        await Assert.That(registrationToken).IsNotNull();

        ILogger<UserController> userLogger = scope.ServiceProvider.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = scope.ServiceProvider.GetRequiredService<IOptions<OperationalConfiguration>>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration, hostEnvironment);

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

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        ILogger<UserController> userLogger = scope.ServiceProvider.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = scope.ServiceProvider.GetRequiredService<IOptions<OperationalConfiguration>>();
        IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration, hostEnvironment);

        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(invalidToken, accountName, password, password));

        await Assert.That(response).IsTypeOf<NotFoundObjectResult>();
    }

    [Test]
    [Arguments("user1@kongor.com", "user2@kongor.com", "DuplicateName", "SecurePassword123!")]
    [Arguments("first@kongor.net", "second@kongor.net", "SameName", "MyP@ssw0rd!")]
    public async Task RegisterUserAndMainAccount_WithDuplicateAccountName_ReturnsConflict(string emailAddressOne, string emailAddressTwo, string accountName, string password)
    {
        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        await jwtAuthenticationService.CreateAuthenticatedUser(emailAddressOne, accountName, password);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        ILogger<EmailAddressController> emailLogger = scope.ServiceProvider.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        EmailAddressController emailController = new (databaseContext, emailLogger, emailService, hostEnvironment);

        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddressTwo, emailAddressTwo));

        Token? registrationToken = await databaseContext.Tokens.SingleOrDefaultAsync(candidate =>
            candidate.EmailAddress.Equals(emailAddressTwo) && candidate.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        await Assert.That(registrationToken).IsNotNull();

        ILogger<UserController> userLogger = scope.ServiceProvider.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = scope.ServiceProvider.GetRequiredService<IOptions<OperationalConfiguration>>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration, hostEnvironment);

        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(registrationToken.Value.ToString(), accountName, password, password));

        await Assert.That(response).IsTypeOf<ConflictObjectResult>();
    }

    [Test]
    [Arguments("token.reuse@kongor.com", "TokenUser", "SecurePassword123!")]
    [Arguments("token.consumed@kongor.net", "ConsumedUser", "MyP@ssw0rd!")]
    public async Task RegisterUserAndMainAccount_WithConsumedToken_ReturnsConflict(string emailAddress, string accountName, string password)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        ILogger<EmailAddressController> emailLogger = scope.ServiceProvider.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        EmailAddressController emailController = new (databaseContext, emailLogger, emailService, hostEnvironment);

        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        Token? registrationToken = await databaseContext.Tokens.SingleOrDefaultAsync(candidate =>
            candidate.EmailAddress.Equals(emailAddress) && candidate.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        await Assert.That(registrationToken).IsNotNull();

        string tokenValue = registrationToken.Value.ToString();

        ILogger<UserController> userLogger = scope.ServiceProvider.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = scope.ServiceProvider.GetRequiredService<IOptions<OperationalConfiguration>>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration, hostEnvironment);

        IActionResult firstResponse = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(tokenValue, accountName, password, password));

        await Assert.That(firstResponse).IsTypeOf<CreatedAtActionResult>();

        Token? consumedToken = await databaseContext.Tokens.FindAsync(registrationToken.ID);

        await Assert.That(consumedToken).IsNotNull();
        await Assert.That(consumedToken.TimestampConsumed).IsNotNull();

        IActionResult secondResponse = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(tokenValue, $"{accountName}2", password, password));

        await Assert.That(secondResponse).IsTypeOf<ConflictObjectResult>();
    }

    [Test]
    [Arguments("case.one@kongor.com", "case.two@kongor.com", "CaseUser", "CaseUser", "SecurePassword123!")]
    [Arguments("TEST-ONE@KONGOR.NET", "TEST-TWO@KONGOR.NET", "TESTUSER", "TESTUSER", "MyP@ssw0rd!")]
    public async Task RegisterUserAndMainAccount_WithExactCaseAccountName_ReturnsConflict(string firstEmailAddress, string secondEmailAddress, string firstAccountName, string secondAccountName, string password)
    {
        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        await jwtAuthenticationService.CreateAuthenticatedUser(firstEmailAddress, firstAccountName, password);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        ILogger<EmailAddressController> emailLogger = scope.ServiceProvider.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        EmailAddressController emailController = new (databaseContext, emailLogger, emailService, hostEnvironment);

        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(secondEmailAddress, secondEmailAddress));

        Token? registrationToken = await databaseContext.Tokens.SingleOrDefaultAsync(candidate =>
            candidate.EmailAddress.Equals(secondEmailAddress) && candidate.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        await Assert.That(registrationToken).IsNotNull();

        ILogger<UserController> userLogger = scope.ServiceProvider.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = scope.ServiceProvider.GetRequiredService<IOptions<OperationalConfiguration>>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration, hostEnvironment);

        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(registrationToken.Value.ToString(), secondAccountName, password, password));

        await Assert.That(response).IsTypeOf<ConflictObjectResult>();
    }

    [Test]
    [Arguments("case.one@kongor.com", "case.two@kongor.com", "CaseUser", "caseuser", "SecurePassword123!")]
    [Arguments("TEST-ONE@KONGOR.NET", "TEST-TWO@KONGOR.NET", "TESTUSER", "testuser", "MyP@ssw0rd!")]
    public async Task RegisterUserAndMainAccount_WithDifferentCaseAccountName_ReturnsConflict(string firstEmailAddress, string secondEmailAddress, string firstAccountName, string secondAccountName, string password)
    {
        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        await jwtAuthenticationService.CreateAuthenticatedUser(firstEmailAddress, firstAccountName, password);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        ILogger<EmailAddressController> emailLogger = scope.ServiceProvider.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        EmailAddressController emailController = new (databaseContext, emailLogger, emailService, hostEnvironment);

        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(secondEmailAddress, secondEmailAddress));

        Token? registrationToken = await databaseContext.Tokens.SingleOrDefaultAsync(candidate =>
            candidate.EmailAddress.Equals(secondEmailAddress) && candidate.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        await Assert.That(registrationToken).IsNotNull();

        ILogger<UserController> userLogger = scope.ServiceProvider.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = scope.ServiceProvider.GetRequiredService<IOptions<OperationalConfiguration>>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration, hostEnvironment);

        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(registrationToken.Value.ToString(), secondAccountName, password, password));

        // SQL Server Uses A Case-Insensitive Collation By Default, So "CaseUser" And "caseuser" Collide On The Account Name Unique Index

        await Assert.That(response).IsTypeOf<ConflictObjectResult>();
    }
}
