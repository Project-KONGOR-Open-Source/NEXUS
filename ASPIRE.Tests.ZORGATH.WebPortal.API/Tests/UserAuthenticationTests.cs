namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Tests;

/// <summary>
///     Tests for user authentication.
/// </summary>
public sealed class UserAuthenticationTests(ZORGATHIntegrationWebApplicationFactory webApplicationFactory)
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().InitialiseAsync();

    [Test]
    [Arguments("login@kongor.com", "LoginPlayer", "SecurePassword123!")]
    [Arguments("auth@kongor.net", "AuthUser", "MyP@ssw0rd!")]
    public async Task LogInUser_WithValidCredentials_ReturnsOKWithValidJWT(string emailAddress, string accountName, string password)
    {
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

        if (validatedToken is not JwtSecurityToken jwt)
            throw new InvalidOperationException($"Expected Token To Be A JWT Security Token, But Was {validatedToken.GetType().Name}");

        using (Assert.Multiple())
        {
            await Assert.That(jwt.Subject).IsEqualTo(accountName);
            await Assert.That(jwt.Claims.GetUserEmailAddress()).IsEqualTo(emailAddress);
            await Assert.That(jwt.Issuer).IsEqualTo(configuration.Value.JWT.Issuer);
            await Assert.That(jwt.Audiences.Single()).IsEqualTo(configuration.Value.JWT.Audience);
        }

        string userIDClaim = jwt.Claims.Single(claim => claim.Type.Equals(Claims.UserID)).Value;

        await Assert.That(userIDClaim).IsEqualTo(authenticationResult.UserID.ToString());
    }

    [Test]
    [Arguments("NonExistentPlayer", "SomePassword123!")]
    [Arguments("InvalidUser", "AnotherPass123!")]
    public async Task LogInUser_WithInvalidAccountName_ReturnsNotFound(string accountName, string password)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        ILogger<UserController> userLogger = scope.ServiceProvider.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = scope.ServiceProvider.GetRequiredService<IOptions<OperationalConfiguration>>();
        IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration, hostEnvironment);

        IActionResult response = await userController.LogInUser(new LogInUserDTO(accountName, password));

        await Assert.That(response).IsTypeOf<NotFoundObjectResult>();
    }

    [Test]
    [Arguments("wrongpass@kongor.com", "WrongPassUser", "CorrectPassword123!", "WrongPassword123!")]
    [Arguments("badauth@kongor.net", "BadAuthUser", "RightP@ss!", "WrongP@ss!")]
    public async Task LogInUser_WithInvalidPassword_ReturnsUnauthorized(string emailAddress, string accountName, string correctPassword, string wrongPassword)
    {
        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        await jwtAuthenticationService.CreateAuthenticatedUser(emailAddress, accountName, correctPassword);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        ILogger<UserController> userLogger = scope.ServiceProvider.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = scope.ServiceProvider.GetRequiredService<IOptions<OperationalConfiguration>>();
        IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration, hostEnvironment);

        IActionResult response = await userController.LogInUser(new LogInUserDTO(accountName, wrongPassword));

        await Assert.That(response).IsTypeOf<UnauthorizedObjectResult>();
    }

    [Test]
    [Arguments("claims@kongor.com", "ClaimsUser", "SecurePassword123!")]
    [Arguments("jwt@kongor.net", "JWTUser", "MyP@ssw0rd!")]
    public async Task LogInUser_JWTContainsAllRequiredClaims(string emailAddress, string accountName, string password)
    {
        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        JWTAuthenticationData authenticationResult = await jwtAuthenticationService.CreateAuthenticatedUser(emailAddress, accountName, password);

        JwtSecurityTokenHandler tokenHandler = new ();
        JwtSecurityToken jwt = tokenHandler.ReadJwtToken(authenticationResult.AuthenticationToken);

        using (Assert.Multiple())
        {
            await Assert.That(jwt.Claims.Any(claim => claim.Type.Equals(JwtRegisteredClaimNames.Sub))).IsTrue();
            await Assert.That(jwt.Claims.Any(claim => claim.Type.Equals(JwtRegisteredClaimNames.Email))).IsTrue();
            await Assert.That(jwt.Claims.Any(claim => claim.Type.Equals(JwtRegisteredClaimNames.Iat))).IsTrue();
            await Assert.That(jwt.Claims.Any(claim => claim.Type.Equals(JwtRegisteredClaimNames.AuthTime))).IsTrue();
            await Assert.That(jwt.Claims.Any(claim => claim.Type.Equals(JwtRegisteredClaimNames.Jti))).IsTrue();
            await Assert.That(jwt.Claims.Any(claim => claim.Type.Equals(JwtRegisteredClaimNames.Nonce))).IsTrue();

            await Assert.That(jwt.Claims.Any(claim => claim.Type.Equals(Claims.UserID))).IsTrue();
            await Assert.That(jwt.Claims.Any(claim => claim.Type.Equals(Claims.AccountID))).IsTrue();
            await Assert.That(jwt.Claims.Any(claim => claim.Type.Equals(Claims.AccountIsMain))).IsTrue();
        }
    }

    [Test]
    [Arguments("fullflow@kongor.com", "FlowUser", "SecurePassword123!")]
    [Arguments("complete@kongor.net", "CompleteUser", "MyP@ssw0rd!")]
    public async Task CompleteAuthenticationFlow_RegisterEmailThenUserThenLogin_Succeeds(string emailAddress, string accountName, string password)
    {
        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        JWTAuthenticationData result = await jwtAuthenticationService.CreateAuthenticatedUser(emailAddress, accountName, password);

        using (Assert.Multiple())
        {
            await Assert.That(result.UserID).IsGreaterThan(0);
            await Assert.That(result.AccountName).IsEqualTo(accountName);
            await Assert.That(result.EmailAddress).IsEqualTo(emailAddress);
            await Assert.That(result.AuthenticationToken).IsNotEmpty();
        }

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        User? user = await databaseContext.Users
            .Include(candidate => candidate.Accounts)
            .Include(candidate => candidate.Role)
            .SingleOrDefaultAsync(candidate => candidate.ID.Equals(result.UserID));

        await Assert.That(user).IsNotNull();

        using (Assert.Multiple())
        {
            await Assert.That(user.EmailAddress).IsEqualTo(emailAddress);
            await Assert.That(user.Accounts).Count().IsGreaterThanOrEqualTo(1);
            await Assert.That(user.Accounts.Any(account => account.Name.Equals(accountName))).IsTrue();
            await Assert.That(user.Role.Name).IsEqualTo(UserRoles.User);
        }
    }

    [Test]
    [Arguments("caselogin@kongor.com", "CaseLoginUser", "CaseLoginUser", "SecurePassword123!")]
    [Arguments("UPPERCASELOGIN@kongor.net", "UPPERCASEUSER", "UPPERCASEUSER", "MyP@ssw0rd!")]
    public async Task LogInUser_WithExactCaseAccountName_ReturnsOK(string emailAddress, string registeredAccountName, string loginAccountName, string password)
    {
        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        await jwtAuthenticationService.CreateAuthenticatedUser(emailAddress, registeredAccountName, password);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        ILogger<UserController> userLogger = scope.ServiceProvider.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = scope.ServiceProvider.GetRequiredService<IOptions<OperationalConfiguration>>();
        IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration, hostEnvironment);

        IActionResult response = await userController.LogInUser(new LogInUserDTO(loginAccountName, password));

        await Assert.That(response).IsTypeOf<OkObjectResult>();
    }

    [Test]
    [Arguments("caselogin@kongor.com", "CaseLoginUser", "caseloginuser", "SecurePassword123!")]
    [Arguments("UPPERCASELOGIN@kongor.net", "UPPERCASEUSER", "uppercaseuser", "MyP@ssw0rd!")]
    public async Task LogInUser_WithDifferentCaseAccountName_ReturnsOK(string emailAddress, string registeredAccountName, string loginAccountName, string password)
    {
        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        await jwtAuthenticationService.CreateAuthenticatedUser(emailAddress, registeredAccountName, password);

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        ILogger<UserController> userLogger = scope.ServiceProvider.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = scope.ServiceProvider.GetRequiredService<IOptions<OperationalConfiguration>>();
        IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration, hostEnvironment);

        IActionResult response = await userController.LogInUser(new LogInUserDTO(loginAccountName, password));

        // SQL Server Uses A Case-Insensitive Collation By Default, So Logging In With "caseloginuser" Succeeds For The Account Registered As "CaseLoginUser"

        await Assert.That(response).IsTypeOf<OkObjectResult>();
    }

    [Test]
    [Arguments("expiry@kongor.com", "ExpiryUser", "SecurePassword123!")]
    [Arguments("tokenlife@kongor.net", "TokenLifeUser", "MyP@ssw0rd!")]
    public async Task LogInUser_JWTHasValidExpirationClaim(string emailAddress, string accountName, string password)
    {
        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        JWTAuthenticationData authenticationResult = await jwtAuthenticationService.CreateAuthenticatedUser(emailAddress, accountName, password);

        JwtSecurityTokenHandler tokenHandler = new ();
        JwtSecurityToken jwt = tokenHandler.ReadJwtToken(authenticationResult.AuthenticationToken);

        using (Assert.Multiple())
        {
            await Assert.That(jwt.Claims.Any(claim => claim.Type.Equals(JwtRegisteredClaimNames.Exp))).IsTrue();
            await Assert.That(jwt.ValidTo).IsGreaterThan(DateTime.UtcNow);
            await Assert.That(jwt.ValidFrom).IsLessThanOrEqualTo(DateTime.UtcNow.AddSeconds(5));
        }
    }
}
