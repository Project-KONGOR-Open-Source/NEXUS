using ASPIRE.Common.DTOs;

using Moq;

using ZORGATH.WebPortal.API.Services;

namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Tests;

/// <summary>
///     Tests For Identity Policy Configuration (Password/Username Validation)
/// </summary>
public sealed class IdentityPolicyTests
{
    [Test]
    public async Task RegisterUser_InProduction_WithWeakPassword_ReturnsBadRequest()
    {
        string emailAddress = "production.weak@gmail.com";
        string accountName = "ProdWeakUser";
        string password = "weak"; // Fails length (8), complexity, etc.

        // Force Production Environment
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory =
            ZORGATHServiceProvider.CreateOrchestratedInstance()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Production");
                builder.ConfigureServices(services =>
                {
                    // Mock Email Service to prevent "Failed To Send Email" error which deletes the token
                    Mock<IEmailService> mockEmailService = new();
                    mockEmailService
                        .Setup(service => service.SendEmailAddressRegistrationLink(It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync(true);

                    services.AddSingleton(mockEmailService.Object);
                });
            });

        // 1. Register Email
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();
        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        // Seed Roles (Required for Registration)
        if (await databaseContext.Roles.AnyAsync(role => role.Name.Equals(UserRoles.User)) is false)
        {
            await databaseContext.Roles.AddAsync(new Role { Name = UserRoles.User });
            await databaseContext.SaveChangesAsync();
        }
        ILogger<EmailAddressController> emailLogger =
            scope.ServiceProvider.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        EmailAddressController emailController = new(databaseContext, emailLogger, emailService, hostEnvironment);
        IActionResult result = await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        await Assert.That(result).IsTypeOf<OkObjectResult>();

        // 2. Get Token
        Token? registrationToken = await databaseContext.Tokens.SingleOrDefaultAsync(token =>
            token.EmailAddress.Equals(emailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));
        await Assert.That(registrationToken).IsNotNull();

        // 3. Attempt Register User
        ILogger<UserController> userLogger =
            scope.ServiceProvider.GetRequiredService<ILogger<UserController>>();
        IUserService userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        IAuthenticationService authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();

        UserController userController =
            new(databaseContext, userLogger, userService, authService);

        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(registrationToken.Value.ToString(), accountName, password, password));

        // Assert BadRequest (due to validation failure)
        await Assert.That(response).IsTypeOf<BadRequestObjectResult>();
    }

    [Test]
    public async Task RegisterUser_InDevelopment_WithWeakPassword_ReturnsCreated()
    {
        string emailAddress = "development.weak@kongor.com";
        string accountName = "DevWeakUser";
        string password = "weak"; // Should pass in Dev

        // Force Development Environment
        await using WebApplicationFactory<ZORGATHAssemblyMarker> webApplicationFactory =
            ZORGATHServiceProvider.CreateOrchestratedInstance()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureServices(services =>
                {
                    // Mock Email Service
                    Mock<IEmailService> mockEmailService = new();
                    mockEmailService
                        .Setup(service => service.SendEmailAddressRegistrationLink(It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync(true);

                    services.AddSingleton(mockEmailService.Object);
                });
            });

        // 1. Register Email
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();
        MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        // Seed Roles (Required for Registration)
        if (await databaseContext.Roles.AnyAsync(role => role.Name.Equals(UserRoles.User)) is false)
        {
            await databaseContext.Roles.AddAsync(new Role { Name = UserRoles.User });
            await databaseContext.SaveChangesAsync();
        }
        ILogger<EmailAddressController> emailLogger =
            scope.ServiceProvider.GetRequiredService<ILogger<EmailAddressController>>();
        IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        IWebHostEnvironment hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        EmailAddressController emailController = new(databaseContext, emailLogger, emailService, hostEnvironment);
        await emailController.RegisterEmailAddress(new RegisterEmailAddressDTO(emailAddress, emailAddress));

        // 2. Get Token
        Token? registrationToken = await databaseContext.Tokens.SingleOrDefaultAsync(token =>
            token.EmailAddress.Equals(emailAddress) && token.Purpose.Equals(TokenPurpose.EmailAddressVerification));
        await Assert.That(registrationToken).IsNotNull();

        // 3. Attempt Register User
        ILogger<UserController> userLogger =
            scope.ServiceProvider.GetRequiredService<ILogger<UserController>>();
        IUserService userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        IAuthenticationService authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();

        UserController userController =
            new(databaseContext, userLogger, userService, authService);

        IActionResult response = await userController.RegisterUserAndMainAccount(
            new RegisterUserAndMainAccountDTO(registrationToken.Value.ToString(), accountName, password, password));

        // Assert Created (validation passed)
        await Assert.That(response).IsTypeOf<CreatedAtActionResult>();
    }
}
