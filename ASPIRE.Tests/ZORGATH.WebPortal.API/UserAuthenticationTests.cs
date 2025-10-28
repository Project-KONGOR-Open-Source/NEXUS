namespace ASPIRE.Tests.ZORGATH.WebPortal.API;

using Infrastructure;

/// <summary>
///     Tests For User Authentication Functionality
/// </summary>
public sealed class UserAuthenticationTests
{
    [Test]
    public async Task LogInUser_WithValidCredentials_ReturnsOkWithValidJWT()
    {
        const string emailAddress = "new.user@kongor.net";
        const string accountName = "NewUser";
        const string password = "SecurePassword123!";

        await using ServiceProvider services = new();
        
        AuthenticationFactory authenticationFactory = services.CreateAuthenticationFactory();

        AuthenticationResult authenticationResult = await authenticationFactory.CreateAuthenticatedUser(emailAddress, accountName, password);

        IOptions<OperationalConfiguration> configuration = services.Factory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();

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

        JwtSecurityTokenHandler tokenHandler = new();
        
        tokenHandler.ValidateToken(authenticationResult.AuthenticationToken, tokenValidationParameters, out SecurityToken validatedToken);

        JwtSecurityToken jwtToken = (JwtSecurityToken)validatedToken;

        await Assert.That(jwtToken.Subject).IsEqualTo(accountName);
        await Assert.That(jwtToken.Claims.GetUserEmailAddress()).IsEqualTo(emailAddress);
        await Assert.That(jwtToken.Issuer).IsEqualTo(configuration.Value.JWT.Issuer);
        await Assert.That(jwtToken.Audiences.First()).IsEqualTo(configuration.Value.JWT.Audience);

        string userIDClaim = jwtToken.Claims.Single(claim => claim.Type.Equals(MERRICK.DatabaseContext.Constants.Claims.UserID)).Value;

        await Assert.That(userIDClaim).IsEqualTo(authenticationResult.UserID.ToString());
    }

    [Test]
    public async Task LogInUser_WithInvalidAccountName_ReturnsNotFound()
    {
        const string accountName = "NewUser";
        const string password = "SecurePassword123!";

        await using ServiceProvider services = new ();

        ILogger<UserController> userLogger = services.Factory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = services.Factory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();
        IEmailService emailService = services.Factory.Services.GetRequiredService<IEmailService>();

        UserController userController = new (services.MerrickContext, userLogger, emailService, configuration);

        IActionResult response = await userController.LogInUser(new LogInUserDTO(accountName, password));

        await Assert.That(response).IsTypeOf<NotFoundObjectResult>();
    }

    [Test]
    public async Task LogInUser_WithInvalidPassword_ReturnsUnauthorized()
    {
        const string emailAddress = "new.user@kongor.net";
        const string accountName = "NewUser";
        const string correctPassword = "CorrectPassword123!";
        const string wrongPassword = "WrongPassword123!";

        await using ServiceProvider services = new();
        
        AuthenticationFactory authenticationFactory = services.CreateAuthenticationFactory();

        await authenticationFactory.CreateAuthenticatedUser(emailAddress, accountName, correctPassword);

        ILogger<UserController> userLogger = services.Factory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = services.Factory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();
        IEmailService emailService = services.Factory.Services.GetRequiredService<IEmailService>();

        UserController userController = new (services.MerrickContext, userLogger, emailService, configuration);

        IActionResult response = await userController.LogInUser(new LogInUserDTO(accountName, wrongPassword));

        await Assert.That(response).IsTypeOf<UnauthorizedObjectResult>();
    }

    [Test]
    public async Task LogInUser_JWTContainsAllRequiredClaims()
    {
        const string emailAddress = "new.user@kongor.net";
        const string accountName = "NewUser";
        const string password = "SecurePassword123!";

        await using ServiceProvider services = new();
        
        AuthenticationFactory authenticationFactory = services.CreateAuthenticationFactory();

        AuthenticationResult authenticationResult = await authenticationFactory.CreateAuthenticatedUser(emailAddress, accountName, password);

        JwtSecurityTokenHandler tokenHandler = new ();
        JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(authenticationResult.AuthenticationToken);

        await Assert.That(jwtToken.Claims.Any(claim => claim.Type.Equals(JwtRegisteredClaimNames.Sub))).IsTrue();
        await Assert.That(jwtToken.Claims.Any(claim => claim.Type.Equals(JwtRegisteredClaimNames.Email))).IsTrue();
        await Assert.That(jwtToken.Claims.Any(claim => claim.Type.Equals(JwtRegisteredClaimNames.Iat))).IsTrue();
        await Assert.That(jwtToken.Claims.Any(claim => claim.Type.Equals(JwtRegisteredClaimNames.AuthTime))).IsTrue();
        await Assert.That(jwtToken.Claims.Any(claim => claim.Type.Equals(JwtRegisteredClaimNames.Jti))).IsTrue();
        await Assert.That(jwtToken.Claims.Any(claim => claim.Type.Equals(JwtRegisteredClaimNames.Nonce))).IsTrue();

        await Assert.That(jwtToken.Claims.Any(claim => claim.Type.Equals(MERRICK.DatabaseContext.Constants.Claims.UserID))).IsTrue();
        await Assert.That(jwtToken.Claims.Any(claim => claim.Type.Equals(MERRICK.DatabaseContext.Constants.Claims.AccountID))).IsTrue();
        await Assert.That(jwtToken.Claims.Any(claim => claim.Type.Equals(MERRICK.DatabaseContext.Constants.Claims.AccountIsMain))).IsTrue();
    }

    [Test]
    public async Task CompleteAuthenticationFlow_RegisterEmailThenUserThenLogin_Succeeds()
    {
        const string emailAddress = "new.user@kongor.net";
        const string accountName = "NewUser";
        const string password = "SecurePassword123!";

        await using ServiceProvider services = new();
        
        AuthenticationFactory authenticationFactory = services.CreateAuthenticationFactory();

        AuthenticationResult result = await authenticationFactory.CreateAuthenticatedUser(emailAddress, accountName, password);

        await Assert.That(result.UserID).IsGreaterThan(0);
        await Assert.That(result.AccountName).IsEqualTo(accountName);
        await Assert.That(result.EmailAddress).IsEqualTo(emailAddress);
        await Assert.That(result.AuthenticationToken).IsNotEmpty();

        MERRICK.DatabaseContext.Entities.Core.User? user = await services.MerrickContext.Users
            .Include(user => user.Accounts)
            .Include(user => user.Role)
            .SingleOrDefaultAsync(user => user.ID.Equals(result.UserID));

        await Assert.That(user).IsNotNull();
        await Assert.That(user!.EmailAddress).IsEqualTo(emailAddress);
        await Assert.That(user.Accounts).HasCount().GreaterThanOrEqualTo(1);
        await Assert.That(user.Accounts.Any(account => account.Name.Equals(accountName))).IsTrue();
        await Assert.That(user.Role.Name).IsEqualTo(MERRICK.DatabaseContext.Constants.UserRoles.User);
    }
}
