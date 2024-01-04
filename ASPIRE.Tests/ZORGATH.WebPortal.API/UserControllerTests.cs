namespace ASPIRE.Tests.ZORGATH.WebPortal.API;

using ZORGATH = global::ZORGATH.WebPortal.API.ZORGATH;

[TestFixture]
public class UserControllerTests
{
    private MerrickContext EphemeralMerrickContext { get; set; } = null!;
    private WebApplicationFactory<ZORGATH> EphemeralZorgath { get; set; } = null!;
    private HttpClient EphemeralZorgathClient { get; set; } = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        EphemeralZorgath = new WebApplicationFactory<ZORGATH>();
        EphemeralZorgathClient = EphemeralZorgath.CreateClient();
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
        EphemeralZorgath.Dispose();
        EphemeralZorgathClient.Dispose();
    }

    [Test]
    [TestCase("project.kongor@proton.me", "KONGOR", "wh#b739&&2N0*$#9GIHz!p4kdys994r@")]
    public async Task RegisterUserAndMainAccountTest(string emailAddress, string name, string password)
    {
        ILogger<EmailAddressController> emailAddressControllerLogger; ILogger<UserController> userControllerLogger; IConfiguration configuration; IEmailService emailService;

        try
        {
            emailAddressControllerLogger = EphemeralZorgath.Services.GetService(typeof(ILogger<EmailAddressController>)) as ILogger<EmailAddressController>
                ?? throw new NullReferenceException("ILogger<EmailAddressController> Is NULL");

            userControllerLogger = EphemeralZorgath.Services.GetService(typeof(ILogger<UserController>)) as ILogger<UserController>
                ?? throw new NullReferenceException("ILogger<UserController> Is NULL");

            configuration = EphemeralZorgath.Services.GetService(typeof(IConfiguration)) as IConfiguration
                ?? throw new NullReferenceException("IConfiguration Is NULL");

            emailService = EphemeralZorgath.Services.GetService(typeof(IEmailService)) as IEmailService
                ?? throw new NullReferenceException("IEmailService Is NULL");
        }

        catch(NullReferenceException exception)
        {
            Assert.Fail(exception.Message);

            throw new NullReferenceException("Required Service Is NULL", exception);
        }

        EmailAddressController emailAddressController = new(EphemeralMerrickContext, emailAddressControllerLogger, emailService);

        // TODO: Move Email Controller To Separate Tests File

        IActionResult responseRegisterEmailAddress = await emailAddressController.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        Assert.That(responseRegisterEmailAddress is OkObjectResult, Is.True);

        Token registrationToken = await EphemeralMerrickContext.Tokens.SingleAsync(token => token.EmailAddress.Equals(emailAddress));

        UserController userController = new(EphemeralMerrickContext, userControllerLogger, configuration, emailService);

        IActionResult responseRegisterUserAndMainAccount = await userController.RegisterUserAndMainAccount(new RegisterUserAndMainAccountDTO(registrationToken.ID.ToString(), name, password, password));

        Assert.That(responseRegisterUserAndMainAccount is CreatedAtActionResult, Is.True);

        IActionResult responseLogInUser = await userController.LogInUser(new LogInUserDTO(name, password));

        Assert.That(responseLogInUser is OkObjectResult, Is.True);

        string authenticationToken = ((responseLogInUser as OkObjectResult ?? throw new InvalidCastException("Unable To Cast Response"))
            .Value as GetAuthenticationTokenDTO ?? throw new NullReferenceException("Authentication Token DTO Is NULL")).Token;

        string? authenticationTokenSigningKey = configuration["JWT:SigningKey"]; // TODO: Put The Signing Key In A Secrets Vault

        if (authenticationTokenSigningKey is null)
            throw new NullReferenceException("JSON Web Token Signing Key Is NULL");

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

        Assert.Multiple(() =>
        {
            Assert.That((validatedSecurityToken as JwtSecurityToken)?.Subject, Is.EqualTo(name));
            Assert.That((validatedSecurityToken as JwtSecurityToken)?.Claims.GetUserEmailAddress(), Is.EqualTo(emailAddress));
        });
    }
}
