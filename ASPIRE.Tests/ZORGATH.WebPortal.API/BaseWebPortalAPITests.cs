﻿namespace ASPIRE.Tests.ZORGATH.WebPortal.API;

using ZORGATH = global::ZORGATH.WebPortal.API.ZORGATH;

public abstract class BaseWebPortalAPITests
{
    protected MerrickContext EphemeralMerrickContext { get; set; } = null!;
    protected WebApplicationFactory<ZORGATH> EphemeralZorgath { get; set; } = null!;
    protected HttpClient EphemeralZorgathClient { get; set; } = null!;

    protected static string EphemeralAuthenticationToken { get; set; } = null!;

    private static bool AuthenticationFlowExecuted { get; set; } = false;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        EphemeralZorgath = new WebApplicationFactory<ZORGATH>();
        EphemeralZorgathClient = EphemeralZorgath.CreateClient();

        if (AuthenticationFlowExecuted.Equals(false))
        {
            const string emailAddress = "project.kongor@proton.me";
            const string name = "KONGOR";
            string password = Guid.NewGuid().ToString();

            await RegisterEmailAddress(emailAddress);
            await RegisterUserAndMainAccount(emailAddress, name, password);

            // This Method Sets The Ephemeral Authentication Token
            await LogInUser(emailAddress, name, password);

            // Only Execute The Authenticate Flow Once, At The Start Of The Test Run
            AuthenticationFlowExecuted = true;
        }

        EphemeralZorgathClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, EphemeralAuthenticationToken);
    }

    [SetUp]
    public void SetUp()
    {
        EphemeralMerrickContext = InMemoryHelpers.GetInMemoryMerrickContext();
    }

    [TearDown]
    public void TearDown()
    {
        EphemeralMerrickContext.Dispose();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        EphemeralMerrickContext.Dispose();
        EphemeralZorgath.Dispose();
        EphemeralZorgathClient.Dispose();
    }

    private async Task RegisterEmailAddress(string emailAddress)
    {
        // Override The Default "EphemeralMerrickContext", And Create A Named Database Context For Sharing With "RegisterUserAndMainAccountTest()"
        EphemeralMerrickContext = InMemoryHelpers.GetInMemoryMerrickContext("Registration And Authentication");

        ILogger<EmailAddressController> emailAddressControllerLogger; IEmailService emailService;

        try
        {
            emailAddressControllerLogger = EphemeralZorgath.Services.GetService(typeof(ILogger<EmailAddressController>)) as ILogger<EmailAddressController>
                ?? throw new NullReferenceException("ILogger<EmailAddressController> Is NULL");

            emailService = EphemeralZorgath.Services.GetService(typeof(IEmailService)) as IEmailService
                ?? throw new NullReferenceException("IEmailService Is NULL");
        }

        catch (NullReferenceException exception)
        {
            Assert.Fail(exception.Message);

            throw new NullReferenceException("Required Service Is NULL", exception);
        }

        EmailAddressController emailAddressController = new(EphemeralMerrickContext, emailAddressControllerLogger, emailService);

        IActionResult responseRegisterEmailAddress = await emailAddressController.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        Assert.That(responseRegisterEmailAddress is OkObjectResult, Is.True);

        Token? token = await EphemeralMerrickContext.Tokens.SingleOrDefaultAsync(token => token.EmailAddress.Equals(emailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        Assert.That(token, Is.Not.Null);
    }

    private async Task RegisterUserAndMainAccount(string emailAddress, string name, string password)
    {
        // Override The Default "EphemeralMerrickContext", And Create A Named Database Context For Sharing With "RegisterEmailAddressTest()"
        EphemeralMerrickContext = InMemoryHelpers.GetInMemoryMerrickContext("Registration And Authentication");

        ILogger<UserController> userControllerLogger; IConfiguration configuration; IEmailService emailService;

        try
        {
            userControllerLogger = EphemeralZorgath.Services.GetService(typeof(ILogger<UserController>)) as ILogger<UserController>
                ?? throw new NullReferenceException("ILogger<UserController> Is NULL");

            configuration = EphemeralZorgath.Services.GetService(typeof(IConfiguration)) as IConfiguration
                ?? throw new NullReferenceException("IConfiguration Is NULL");

            emailService = EphemeralZorgath.Services.GetService(typeof(IEmailService)) as IEmailService
                ?? throw new NullReferenceException("IEmailService Is NULL");
        }

        catch (NullReferenceException exception)
        {
            Assert.Fail(exception.Message);

            throw new NullReferenceException("Required Service Is NULL", exception);
        }

        Token? registrationToken = await EphemeralMerrickContext.Tokens.SingleOrDefaultAsync(token => token.EmailAddress.Equals(emailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        if (registrationToken is null)
        {
            Assert.Fail("Registration Token Is NULL");

            throw new NullReferenceException("Registration Token Is NULL");
        }

        UserController userController = new(EphemeralMerrickContext, userControllerLogger, configuration, emailService);

        IActionResult responseRegisterUserAndMainAccount = await userController.RegisterUserAndMainAccount(new RegisterUserAndMainAccountDTO(registrationToken.ID.ToString(), name, password, password));

        Assert.That(responseRegisterUserAndMainAccount is CreatedAtActionResult, Is.True);
    }

    private async Task LogInUser(string emailAddress, string name, string password)
    {
        // Override The Default "EphemeralMerrickContext", And Create A Named Database Context For Sharing With "RegisterEmailAddressTest()"
        EphemeralMerrickContext = InMemoryHelpers.GetInMemoryMerrickContext("Registration And Authentication");

        ILogger<UserController> userControllerLogger; IConfiguration configuration; IEmailService emailService;

        try
        {
            userControllerLogger = EphemeralZorgath.Services.GetService(typeof(ILogger<UserController>)) as ILogger<UserController>
                ?? throw new NullReferenceException("ILogger<UserController> Is NULL");

            configuration = EphemeralZorgath.Services.GetService(typeof(IConfiguration)) as IConfiguration
                ?? throw new NullReferenceException("IConfiguration Is NULL");

            emailService = EphemeralZorgath.Services.GetService(typeof(IEmailService)) as IEmailService
                ?? throw new NullReferenceException("IEmailService Is NULL");
        }

        catch (NullReferenceException exception)
        {
            Assert.Fail(exception.Message);

            throw new NullReferenceException("Required Service Is NULL", exception);
        }

        UserController userController = new(EphemeralMerrickContext, userControllerLogger, configuration, emailService);

        IActionResult responseLogInUser = await userController.LogInUser(new LogInUserDTO(name, password));

        Assert.That(responseLogInUser is OkObjectResult, Is.True);

        string authenticationToken = ((responseLogInUser as OkObjectResult ?? throw new InvalidCastException("Unable To Cast Response"))
            .Value as GetAuthenticationTokenDTO ?? throw new NullReferenceException("Authentication Token DTO Is NULL")).Token;

        string? authenticationTokenSigningKey = configuration["JWT:SigningKey"]; // TODO: Put The Signing Key In A Secrets Vault

        if (authenticationTokenSigningKey is null)
        {
            Assert.Fail("JSON Web Token Signing Key Is NULL");

            throw new NullReferenceException("JSON Web Token Signing Key Is NULL");
        }

        TokenValidationParameters tokenValidationParameters = new()
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationTokenSigningKey)),
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["JWT:Issuer"],
            ValidateIssuer = true,
            ValidAudience = configuration["JWT:Audience"],
            ValidateAudience = true,
            ClockSkew = TimeSpan.Zero,
            ValidateLifetime = true
        };

        new JwtSecurityTokenHandler().ValidateToken(authenticationToken, tokenValidationParameters, out SecurityToken validatedSecurityToken);

        EphemeralAuthenticationToken = authenticationToken;

        Assert.Multiple(() =>
        {
            Assert.That((validatedSecurityToken as JwtSecurityToken)?.Subject, Is.EqualTo(name));
            Assert.That((validatedSecurityToken as JwtSecurityToken)?.Claims.GetUserEmailAddress(), Is.EqualTo(emailAddress));
        });
    }
}