namespace ASPIRE.Tests.Zorgath.WebPortal.API.Tests;

/// <summary>
///     Tests For User Authentication Functionality
/// </summary>
public sealed class UserAuthenticationTests
{
    [Test]
    [Arguments("login@kongor.com", "LoginPlayer", "SecurePassword123!")]
    [Arguments("auth@kongor.net", "AuthUser", "MyP@ssw0rd!")]
    public async Task LogInUser_WithValidCredentials_ReturnsOkWithValidJWT(string emailAddress, string accountName, string password)
    {
        WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = await ZORGATHServiceProvider.CreateOrchestratedInstance();

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

        JwtSecurityToken jwtToken = (JwtSecurityToken)validatedToken;

        await Assert.That(jwtToken.Subject).IsEqualTo(accountName);
        await Assert.That(jwtToken.Claims.GetUserEmailAddress()).IsEqualTo(emailAddress);
        await Assert.That(jwtToken.Issuer).IsEqualTo(configuration.Value.JWT.Issuer);
        await Assert.That(jwtToken.Audiences.First()).IsEqualTo(configuration.Value.JWT.Audience);

        string userIDClaim = jwtToken.Claims.Single(claim => claim.Type.Equals(Claims.UserID)).Value;

        await Assert.That(userIDClaim).IsEqualTo(authenticationResult.UserID.ToString());
    }

    [Test]
    [Arguments("NonExistentPlayer", "SomePassword123!")]
    [Arguments("InvalidUser", "AnotherPass123!")]
    public async Task LogInUser_WithInvalidAccountName_ReturnsNotFound(string accountName, string password)
    {
        WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = await ZORGATHServiceProvider.CreateOrchestratedInstance();

        ILogger<UserController> userLogger = webApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = webApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration);

        IActionResult response = await userController.LogInUser(new LogInUserDTO(accountName, password));

        await Assert.That(response).IsTypeOf<NotFoundObjectResult>();
    }

    [Test]
    [Arguments("wrongpass@kongor.com", "WrongPassUser", "CorrectPassword123!", "WrongPassword123!")]
    [Arguments("badauth@kongor.net", "BadAuthUser", "RightP@ss!", "WrongP@ss!")]
    public async Task LogInUser_WithInvalidPassword_ReturnsUnauthorized(string emailAddress, string accountName, string correctPassword, string wrongPassword)
    {
        WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = await ZORGATHServiceProvider.CreateOrchestratedInstance();

        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        await jwtAuthenticationService.CreateAuthenticatedUser(emailAddress, accountName, correctPassword);

        ILogger<UserController> userLogger = webApplicationFactory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = webApplicationFactory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();
        IEmailService emailService = webApplicationFactory.Services.GetRequiredService<IEmailService>();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        UserController userController = new (databaseContext, userLogger, emailService, configuration);

        IActionResult response = await userController.LogInUser(new LogInUserDTO(accountName, wrongPassword));

        await Assert.That(response).IsTypeOf<UnauthorizedObjectResult>();
    }

    [Test]
    [Arguments("claims@kongor.com", "ClaimsUser", "SecurePassword123!")]
    [Arguments("jwt@kongor.net", "JWTUser", "MyP@ssw0rd!")]
    public async Task LogInUser_JWTContainsAllRequiredClaims(string emailAddress, string accountName, string password)
    {
        WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = await ZORGATHServiceProvider.CreateOrchestratedInstance();

        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        JWTAuthenticationData authenticationResult = await jwtAuthenticationService.CreateAuthenticatedUser(emailAddress, accountName, password);

        JwtSecurityTokenHandler tokenHandler = new ();
        JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(authenticationResult.AuthenticationToken);

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

    [Test]
    [Arguments("fullflow@kongor.com", "FlowUser", "SecurePassword123!")]
    [Arguments("complete@kongor.net", "CompleteUser", "MyP@ssw0rd!")]
    public async Task CompleteAuthenticationFlow_RegisterEmailThenUserThenLogin_Succeeds(string emailAddress, string accountName, string password)
    {
        WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory = await ZORGATHServiceProvider.CreateOrchestratedInstance();

        JWTAuthenticationService jwtAuthenticationService = new (webApplicationFactory);

        JWTAuthenticationData result = await jwtAuthenticationService.CreateAuthenticatedUser(emailAddress, accountName, password);

        await Assert.That(result.UserID).IsGreaterThan(0);
        await Assert.That(result.AccountName).IsEqualTo(accountName);
        await Assert.That(result.EmailAddress).IsEqualTo(emailAddress);
        await Assert.That(result.AuthenticationToken).IsNotEmpty();

        MerrickContext databaseContext = webApplicationFactory.Services.GetRequiredService<MerrickContext>();

        User? user = await databaseContext.Users
            .Include(user => user.Accounts)
            .Include(user => user.Role)
            .SingleOrDefaultAsync(user => user.ID.Equals(result.UserID));

        await Assert.That(user).IsNotNull();

        if (user is null)
            throw new NullReferenceException("User Is NULL");

        await Assert.That(user.EmailAddress).IsEqualTo(emailAddress);
        await Assert.That(user.Accounts).HasCount().GreaterThanOrEqualTo(1);
        await Assert.That(user.Accounts.Any(account => account.Name.Equals(accountName))).IsTrue();
        await Assert.That(user.Role.Name).IsEqualTo(UserRoles.User);
    }
}
