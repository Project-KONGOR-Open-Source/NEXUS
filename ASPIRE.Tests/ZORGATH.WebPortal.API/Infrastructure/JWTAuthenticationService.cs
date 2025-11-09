namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Infrastructure;

/// <summary>
///     Helper Class For Creating Authentication State In Tests
/// </summary>
public sealed class JWTAuthenticationService(MerrickContext merrickContext, WebApplicationFactory<ZORGATHAssemblyMarker> factory)
{
    /// <summary>
    ///     Creates A Complete Authentication Flow: Email Registration, User Registration, And Login
    /// </summary>
    public async Task<AuthenticationResult> CreateAuthenticatedUser(string emailAddress, string accountName, string password)
    {
        Token registrationToken = await RegisterEmailAddress(emailAddress);
        int userID = await RegisterUserAndMainAccount(registrationToken.Value.ToString(), accountName, password);
        string authenticationToken = await LogInUser(accountName, password);

        return new AuthenticationResult(userID, accountName, emailAddress, authenticationToken);
    }

    private async Task<Token> RegisterEmailAddress(string emailAddress)
    {
        ILogger<EmailAddressController> logger = factory.Services.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = factory.Services.GetRequiredService<IEmailService>();

        EmailAddressController controller = new (merrickContext, logger, emailService);

        IActionResult response = await controller.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        if (response is not OkObjectResult)
            throw new InvalidOperationException($"Failed To Register Email Address: {emailAddress}");

        Token? token = await merrickContext.Tokens.SingleOrDefaultAsync(token =>
            token.EmailAddress.Equals(emailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        return token ?? throw new InvalidOperationException($"Registration Token Not Found For Email: {emailAddress}");
    }

    private async Task<int> RegisterUserAndMainAccount(string tokenValue, string accountName, string password)
    {
        ILogger<UserController> logger = factory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = factory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();
        IEmailService emailService = factory.Services.GetRequiredService<IEmailService>();

        UserController controller = new (merrickContext, logger, emailService, configuration);

        IActionResult response = await controller.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(tokenValue, accountName, password, password));

        if (response is not CreatedAtActionResult createdResult)
            throw new InvalidOperationException($"Failed To Register User And Account: {accountName}");

        GetBasicUserDTO? userDTO = createdResult.Value as GetBasicUserDTO;

        return userDTO?.ID ?? throw new InvalidOperationException("User ID Not Found In Registration Response");
    }

    private async Task<string> LogInUser(string accountName, string password)
    {
        ILogger<UserController> logger = factory.Services.GetRequiredService<ILogger<UserController>>();
        IOptions<OperationalConfiguration> configuration = factory.Services.GetRequiredService<IOptions<OperationalConfiguration>>();
        IEmailService emailService = factory.Services.GetRequiredService<IEmailService>();

        UserController controller = new (merrickContext, logger, emailService, configuration);

        IActionResult response = await controller.LogInUser(new LogInUserDTO(accountName, password));

        if (response is not OkObjectResult okResult)
            throw new InvalidOperationException($"Failed To Log In User: {accountName}");

        GetAuthenticationTokenDTO? tokenDTO = okResult.Value as GetAuthenticationTokenDTO;

        return tokenDTO?.Token ?? throw new InvalidOperationException("Authentication Token Not Found In Login Response");
    }
}

/// <summary>
///     Result Of A Complete Authentication Flow
/// </summary>
public sealed record AuthenticationResult(int UserID, string AccountName, string EmailAddress, string AuthenticationToken);
