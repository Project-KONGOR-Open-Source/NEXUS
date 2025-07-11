﻿namespace ASPIRE.Tests.ZORGATH.WebPortal.API;

// Run This Fixture Before Any Other One, In Order To Retrieve An Authentication Token
// Other Methods Will Wait For An Authentication Token To Be Available

[TestFixture, Order(1)]
public sealed class AuthenticationFlowTests : WebPortalAPITestSetup
{
    [SetUp]
    public override async Task SetUp()
    {
        // This Method Overrides An Asynchronous Task, So It Needs To Do Some Asynchronous Work To Keep The Compiler Happy
        await Task.Delay(TimeSpan.Zero);

        TransientZorgath = new WebApplicationFactory<IZorgathAssemblyMarker>();
        TransientZorgathClient = TransientZorgath.CreateClient();

        // Override The Default "TransientMerrickContext", And Create A Named Database Context For Sharing Between The Methods Part Of The Authentication Flow
        TransientMerrickContext = InMemoryHelpers.GetInMemoryMerrickContext("Registration And Authentication");
    }

    // TODO: Use HttpClient

    [Test, Order(1)]
    [TestCase("project@kongor.com")]
    public async Task RegisterEmailAddress(string emailAddress)
    {
        ILogger<EmailAddressController> emailAddressControllerLogger; IEmailService emailService;

        try
        {
            emailAddressControllerLogger = TransientZorgath.Services.GetService(typeof(ILogger<EmailAddressController>)) as ILogger<EmailAddressController>
                ?? throw new NullReferenceException("ILogger<EmailAddressController> Is NULL");

            emailService = TransientZorgath.Services.GetService(typeof(IEmailService)) as IEmailService
                ?? throw new NullReferenceException("IEmailService Is NULL");
        }

        catch (NullReferenceException exception)
        {
            Assert.Fail(exception.Message);

            throw new NullReferenceException("Required Service Is NULL", exception);
        }

        EmailAddressController emailAddressController = new (TransientMerrickContext, emailAddressControllerLogger, emailService);

        IActionResult responseRegisterEmailAddress = await emailAddressController.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        Assert.That(responseRegisterEmailAddress is OkObjectResult, Is.True);

        Token? token = await TransientMerrickContext.Tokens.SingleOrDefaultAsync(token => token.EmailAddress.Equals(emailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        Assert.That(token, Is.Not.Null);
    }

    [Test, Order(2)]
    [TestCase("project@kongor.com", "K-O-N-G-O-R", "https://github.com/K-O-N-G-O-R")]
    public async Task RegisterUserAndMainAccount(string emailAddress, string name, string password)
    {
        ILogger<UserController> userControllerLogger; IOptions<OperationalConfiguration> configuration; IEmailService emailService;

        try
        {
            userControllerLogger = TransientZorgath.Services.GetService(typeof(ILogger<UserController>)) as ILogger<UserController>
                ?? throw new NullReferenceException("ILogger<UserController> Is NULL");

            configuration = TransientZorgath.Services.GetService(typeof(IOptions<OperationalConfiguration>)) as IOptions<OperationalConfiguration>
                ?? throw new NullReferenceException("IOptions<OperationalConfiguration> Is NULL");

            emailService = TransientZorgath.Services.GetService(typeof(IEmailService)) as IEmailService
                ?? throw new NullReferenceException("IEmailService Is NULL");
        }

        catch (NullReferenceException exception)
        {
            Assert.Fail(exception.Message);

            throw new NullReferenceException("Required Service Is NULL", exception);
        }

        Token? registrationToken = await TransientMerrickContext.Tokens.SingleOrDefaultAsync(token => token.EmailAddress.Equals(emailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        if (registrationToken is null)
        {
            Assert.Fail("Registration Token Is NULL");

            throw new NullReferenceException("Registration Token Is NULL");
        }

        UserController userController = new (TransientMerrickContext, userControllerLogger, emailService, configuration);

        IActionResult responseRegisterUserAndMainAccount = await userController.RegisterUserAndMainAccount(new RegisterUserAndMainAccountDTO(registrationToken.Value.ToString(), name, password, password));

        Assert.That(responseRegisterUserAndMainAccount is CreatedAtActionResult, Is.True);
    }

    [Test, Order(3)]
    [TestCase("project@kongor.com", "K-O-N-G-O-R", "https://github.com/K-O-N-G-O-R")]
    public async Task LogInUser(string emailAddress, string name, string password)
    {
        ILogger<UserController> userControllerLogger; IOptions<OperationalConfiguration> configuration; IEmailService emailService;

        try
        {
            userControllerLogger = TransientZorgath.Services.GetService(typeof(ILogger<UserController>)) as ILogger<UserController>
                ?? throw new NullReferenceException("ILogger<UserController> Is NULL");

            configuration = TransientZorgath.Services.GetService(typeof(IOptions<OperationalConfiguration>)) as IOptions<OperationalConfiguration>
                ?? throw new NullReferenceException("IOptions<OperationalConfiguration> Is NULL");

            emailService = TransientZorgath.Services.GetService(typeof(IEmailService)) as IEmailService
                ?? throw new NullReferenceException("IEmailService Is NULL");
        }

        catch (NullReferenceException exception)
        {
            Assert.Fail(exception.Message);

            throw new NullReferenceException("Required Service Is NULL", exception);
        }

        UserController userController = new (TransientMerrickContext, userControllerLogger, emailService, configuration);

        IActionResult responseLogInUser = await userController.LogInUser(new LogInUserDTO(name, password));

        Assert.That(responseLogInUser is OkObjectResult, Is.True);

        string authenticationToken = ((responseLogInUser as OkObjectResult ?? throw new InvalidCastException("Unable To Cast Response"))
            .Value as GetAuthenticationTokenDTO ?? throw new NullReferenceException("Authentication Token DTO Is NULL")).Token;

        TokenValidationParameters tokenValidationParameters = new ()
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.Value.JWT.SigningKey)), // TODO: Put The Signing Key In A Secrets Vault
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration.Value.JWT.Issuer,
            ValidateIssuer = true,
            ValidAudience = configuration.Value.JWT.Audience,
            ValidateAudience = true,
            ClockSkew = TimeSpan.Zero,
            ValidateLifetime = true
        };

        new JwtSecurityTokenHandler().ValidateToken(authenticationToken, tokenValidationParameters, out SecurityToken validatedSecurityToken);

        // Set The Authentication Token For The Web Portal API Test Run; Other Tests Will Use This Token To Make Authenticated Requests
        WebPortalAPITestContext.TransientAuthenticationToken = authenticationToken;

        // Signal Other Tests That An Authentication Token Is Available For Making Authenticated Requests
        WebPortalAPITestContext.AuthenticationFlowHasExecuted = true;

        Assert.Multiple(() =>
        {
            Assert.That((validatedSecurityToken as JwtSecurityToken)?.Subject, Is.EqualTo(name));
            Assert.That((validatedSecurityToken as JwtSecurityToken)?.Claims.GetUserEmailAddress(), Is.EqualTo(emailAddress));
        });
    }
}
