using ASPIRE.Common.DTOs;

using ZORGATH.WebPortal.API.Services;

namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Services;

/// <summary>
///     Helper Class For Creating Authentication State In Tests
/// </summary>
public sealed class JWTAuthenticationService(WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory)
{
    /// <summary>
    ///     Creates A Complete Authentication Flow: Email Registration, User Registration, And Login
    /// </summary>
    public async Task<JWTAuthenticationData> CreateAuthenticatedUser(string emailAddress, string accountName,
        string password)
    {
        Token registrationToken = await RegisterEmailAddress(emailAddress);

        int userID = await RegisterUserAndMainAccount(registrationToken.Value.ToString(), accountName, password);
        string authenticationToken = await LogInUser(accountName, password);

        return new JWTAuthenticationData(userID, accountName, emailAddress, authenticationToken);
    }

    private async Task<Token> RegisterEmailAddress(string emailAddress)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        ILogger<EmailAddressController> logger =
            scope.ServiceProvider.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        MerrickContext merrickContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        EmailAddressController controller = new(merrickContext, logger, emailService, hostEnvironment);

        IActionResult response =
            await controller.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        if (response is not OkObjectResult)
        {
            throw new InvalidOperationException($"Failed To Register Email Address: {emailAddress}");
        }

        Token? token = await merrickContext.Tokens.SingleOrDefaultAsync(token =>
            token.EmailAddress.Equals(emailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));

        return token ?? throw new InvalidOperationException($"Registration Token Not Found For Email: {emailAddress}");
    }

    private async Task<int> RegisterUserAndMainAccount(string tokenValue, string accountName, string password)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        ILogger<UserController> logger = scope.ServiceProvider.GetRequiredService<ILogger<UserController>>();
        MerrickContext merrickContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
        IUserService userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        IAuthenticationService authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();

        UserController controller = new(merrickContext, logger, userService, authService);

        IActionResult response = await controller.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(tokenValue, accountName, password, password));

        if (response is not CreatedAtActionResult createdResult)
        {
            throw new InvalidOperationException($"Failed To Register User And Account: {accountName}");
        }

        GetBasicUserDTO? userDTO = createdResult.Value as GetBasicUserDTO;

        return userDTO?.ID ?? throw new InvalidOperationException("User ID Not Found In Registration Response");
    }

    private async Task<string> LogInUser(string accountName, string password)
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        ILogger<UserController> logger = scope.ServiceProvider.GetRequiredService<ILogger<UserController>>();
        MerrickContext merrickContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();
        IUserService userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        IAuthenticationService authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();

        UserController controller = new(merrickContext, logger, userService, authService);

        IActionResult response = await controller.LogInUser(new LogInUserDTO(accountName, password));

        if (response is not OkObjectResult okResult)
        {
            throw new InvalidOperationException($"Failed To Log In User: {accountName}");
        }

        GetAuthenticationTokenDTO? tokenDTO = okResult.Value as GetAuthenticationTokenDTO;

        return tokenDTO?.Token ??
               throw new InvalidOperationException("Authentication Token Not Found In Login Response");
    }
}
