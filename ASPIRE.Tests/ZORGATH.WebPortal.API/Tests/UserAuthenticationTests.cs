namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Tests;

using global::ZORGATH.WebPortal.API.Validators;

/// <summary>
///     Tests For User Authentication Functionality
/// </summary>
public sealed class UserAuthenticationTests
{
    [Test]
    [Arguments("login@kongor.com", "LoginPlayer", "SecurePassword123!")]
    [Arguments("auth@kongor.net", "AuthUser", "MyP@ssw0rd!")]
    public async Task LogInUser_WithValidCredentials_ReturnsOKWithValidJWT(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = ZORGATHServiceProvider.CreateOrchestratedInstance();

        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        JWTAuthenticationData authenticationResult = await jwtAuthenticationService.CreateAuthenticatedUser(emailAddress, accountName, password);

        IOptions<OperationalConfiguration> configuration = webApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();

        TokenValidationParameters tokenValidationParameters = new ()
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.Value.JWT.SigningKey)),
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration.Value.JWT.Issuer,
            ValidateIssuer = true,
            ValidAudience = configuration.Value.JWT.Audience,
            ValidateAudience = true,
            ClockSkew = TimeSpan.Zero,
            ValidateLifetime = true
        };

        JwtSecurityTokenHandler tokenHandler = new ();

        tokenHandler.ValidateToken(authenticationResult.AuthenticationToken, tokenValidationParameters, out SecurityToken validatedToken);

        if (validatedToken is not JwtSecurityToken jwtToken)
            throw new InvalidOperationException($"Expected Token To Be A JWT Security Token, But Was {validatedToken.GetType().Name}");

        using (Assert.Multiple())
        {
            await Assert.That(jwtToken.Subject).IsEqualTo(accountName);
            await Assert.That(jwtToken.Claims.GetUserEmailAddress()).IsEqualTo(emailAddress);
            await Assert.That(jwtToken.Issuer).IsEqualTo(configuration.Value.JWT.Issuer);
            await Assert.That(jwtToken.Audiences.First()).IsEqualTo(configuration.Value.JWT.Audience);
        }

        string userIDClaim = jwtToken.Claims.Single(claim => claim.Type.Equals(Claims.UserID)).Value;

        await Assert.That(userIDClaim).IsEqualTo(authenticationResult.UserID.ToString());
    }

    [Test]
    [Arguments("NonExistentPlayer", "SomePassword123!")]
    [Arguments("InvalidUser", "AnotherPass123!")]
    public async Task LogInUser_WithInvalidAccountName_ReturnsNotFound(string accountName, string password)
    {
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = ZORGATHServiceProvider.CreateOrchestratedInstance();

        ILogger<UserController> userLogger = webApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = webApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = webApplicationFactory.Services.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();
        PasswordValidator passwordValidator = webApplicationFactory.Services.GetRequiredService<PasswordValidator>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration, hostEnvironment, passwordValidator);

        IActionResult response = await userController.LogInUser(new LogInUserDTO(accountName, password));

        await Assert.That(response).IsTypeOf<NotFoundObjectResult>();
    }

    [Test]
    [Arguments("wrongpass@kongor.com", "WrongPassUser", "CorrectPassword123!", "WrongPassword123!")]
    [Arguments("badauth@kongor.net", "BadAuthUser", "RightP@ss!", "WrongP@ss!")]
    public async Task LogInUser_WithInvalidPassword_ReturnsUnauthorized(string emailAddress, string accountName, string correctPassword, string wrongPassword)
    {
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = ZORGATHServiceProvider.CreateOrchestratedInstance();

        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        await jwtAuthenticationService.CreateAuthenticatedUser(emailAddress, accountName, correctPassword);

        ILogger<UserController> userLogger = webApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = webApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = webApplicationFactory.Services.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();
        PasswordValidator passwordValidator = webApplicationFactory.Services.GetRequiredService<PasswordValidator>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration, hostEnvironment, passwordValidator);

        IActionResult response = await userController.LogInUser(new LogInUserDTO(accountName, wrongPassword));

        await Assert.That(response).IsTypeOf<UnauthorizedObjectResult>();
    }

    [Test]
    [Arguments("claims@kongor.com", "ClaimsUser", "SecurePassword123!")]
    [Arguments("jwt@kongor.net", "JWTUser", "MyP@ssw0rd!")]
    public async Task LogInUser_JWTContainsAllRequiredClaims(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = ZORGATHServiceProvider.CreateOrchestratedInstance();

        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        JWTAuthenticationData authenticationResult = await jwtAuthenticationService.CreateAuthenticatedUser(emailAddress, accountName, password);

        JwtSecurityTokenHandler tokenHandler = new ();
        JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(authenticationResult.AuthenticationToken);

        using (Assert.Multiple())
        {
            await Assert.That(jwtToken.Claims.Any(claim => claim.Type.Equals(JwtRegisteredClaimNames.Sub))).IsTrue();
            await Assert.That(jwtToken.Claims.Any(claim => claim.Type.Equals(JwtRegisteredClaimNames.Email))).IsTrue();
            await Assert.That(jwtToken.Claims.Any(claim => claim.Type.Equals(JwtRegisteredClaimNames.Iat))).IsTrue();
            await Assert.That(jwtToken.Claims.Any(claim => claim.Type.Equals(JwtRegisteredClaimNames.AuthTime))).IsTrue();
            await Assert.That(jwtToken.Claims.Any(claim => claim.Type.Equals(JwtRegisteredClaimNames.Jti))).IsTrue();
            await Assert.That(jwtToken.Claims.Any(claim => claim.Type.Equals(JwtRegisteredClaimNames.Nonce))).IsTrue();

            await Assert.That(jwtToken.Claims.Any(claim => claim.Type.Equals(Claims.UserID))).IsTrue();
            await Assert.That(jwtToken.Claims.Any(claim => claim.Type.Equals(Claims.AccountID))).IsTrue();
            await Assert.That(jwtToken.Claims.Any(claim => claim.Type.Equals(Claims.AccountIsMain))).IsTrue();
        }
    }

    [Test]
    [Arguments("fullflow@kongor.com", "FlowUser", "SecurePassword123!")]
    [Arguments("complete@kongor.net", "CompleteUser", "MyP@ssw0rd!")]
    public async Task CompleteAuthenticationFlow_RegisterEmailThenUserThenLogin_Succeeds(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = ZORGATHServiceProvider.CreateOrchestratedInstance();

        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        JWTAuthenticationData result = await jwtAuthenticationService.CreateAuthenticatedUser(emailAddress, accountName, password);

        using (Assert.Multiple())
        {
            await Assert.That(result.UserID).IsGreaterThan(0);
            await Assert.That(result.AccountName).IsEqualTo(accountName);
            await Assert.That(result.EmailAddress).IsEqualTo(emailAddress);
            await Assert.That(result.AuthenticationToken).IsNotEmpty();
        }

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        User? user = await databaseContext.Users
            .Include(user => user.Accounts)
            .Include(user => user.Role)
            .SingleOrDefaultAsync(user => user.ID.Equals(result.UserID));

        await Assert.That(user).IsNotNull();

        using (Assert.Multiple())
        {
            await Assert.That(user.EmailAddress).IsEqualTo(emailAddress);
            await Assert.That(user.Accounts).HasCount().GreaterThanOrEqualTo(1);
            await Assert.That(user.Accounts.Any(account => account.Name.Equals(accountName))).IsTrue();
            await Assert.That(user.Role.Name).IsEqualTo(UserRoles.User);
        }
    }

    [Test]
    [Arguments("caselogin@kongor.com", "CaseLoginUser", "CaseLoginUser", "SecurePassword123!")]
    [Arguments("UPPERCASELOGIN@kongor.net", "UPPERCASEUSER", "UPPERCASEUSER", "MyP@ssw0rd!")]
    public async Task LogInUser_WithExactCaseAccountName_ReturnsOK(string emailAddress, string registeredAccountName, string loginAccountName, string password)
    {
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = ZORGATHServiceProvider.CreateOrchestratedInstance();

        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        await jwtAuthenticationService.CreateAuthenticatedUser(emailAddress, registeredAccountName, password);

        ILogger<UserController> userLogger = webApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = webApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = webApplicationFactory.Services.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();
        PasswordValidator passwordValidator = webApplicationFactory.Services.GetRequiredService<PasswordValidator>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration, hostEnvironment, passwordValidator);

        IActionResult response = await userController.LogInUser(new LogInUserDTO(loginAccountName, password));

        await Assert.That(response).IsTypeOf<OkObjectResult>();
    }

    [Test]
    [Arguments("caselogin@kongor.com", "CaseLoginUser", "caseloginuser", "SecurePassword123!")]
    [Arguments("UPPERCASELOGIN@kongor.net", "UPPERCASEUSER", "uppercaseuser", "MyP@ssw0rd!")]
    public async Task LogInUser_WithDifferentCaseAccountName_ReturnsNotFound(string emailAddress, string registeredAccountName, string loginAccountName, string password)
    {
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = ZORGATHServiceProvider.CreateOrchestratedInstance();

        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        await jwtAuthenticationService.CreateAuthenticatedUser(emailAddress, registeredAccountName, password);

        ILogger<UserController> userLogger = webApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = webApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = webApplicationFactory.Services.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();
        PasswordValidator passwordValidator = webApplicationFactory.Services.GetRequiredService<PasswordValidator>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration, hostEnvironment, passwordValidator);

        IActionResult response = await userController.LogInUser(new LogInUserDTO(loginAccountName, password));

        await Assert.That(response).IsTypeOf<NotFoundObjectResult>();
    }

    [Test]
    [Arguments("expiry@kongor.com", "ExpiryUser", "SecurePassword123!")]
    [Arguments("tokenlife@kongor.net", "TokenLifeUser", "MyP@ssw0rd!")]
    public async Task LogInUser_JWTHasValidExpirationClaim(string emailAddress, string accountName, string password)
    {
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = ZORGATHServiceProvider.CreateOrchestratedInstance();

        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        JWTAuthenticationData authenticationResult = await jwtAuthenticationService.CreateAuthenticatedUser(emailAddress, accountName, password);

        JwtSecurityTokenHandler tokenHandler = new ();
        JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(authenticationResult.AuthenticationToken);

        using (Assert.Multiple())
        {
            // Verify Expiration Claim Exists
            await Assert.That(jwtToken.Claims.Any(claim => claim.Type.Equals(JwtRegisteredClaimNames.Exp))).IsTrue();

            // Verify Token Has A Valid Expiration Time In The Future
            await Assert.That(jwtToken.ValidTo).IsGreaterThan(DateTime.UtcNow);

            // Verify Token Was Issued In The Past Or Now (5 Second Grace For Clock Skew)
            await Assert.That(jwtToken.ValidFrom).IsLessThanOrEqualTo(DateTime.UtcNow.AddSeconds(5));
        }
    }
}
