namespace ZORGATH.WebPortal.API.Services.Email;

// TODO: Implement Real Email Service

internal class EmailService(IConfiguration configuration, ILogger logger) : IEmailService
{
    private ILogger Logger { get; init; } = logger;

    private string BaseURL { get; init; } = configuration.GetSection("environmentVariables").GetSection("ASPNETCORE_ENVIRONMENT").Value is "Development"
        ? "https://localhost:55508"
        : configuration.GetSection("environmentVariables").GetSection("ASPNETCORE_ENVIRONMENT").Value is "Production"
            ? "https://portal.api.kongor.online"
            : throw new ArgumentOutOfRangeException(@"Unknown ""ASPNETCORE_ENVIRONMENT"" Value");

    public async Task<bool> SendEmailAddressRegistrationLink(string emailAddress, string token)
    {
        string link = BaseURL + "/register/" + token;

        const string subject = "Verify Email Address";

        string body = "You need to verify your email address before you can create your Heroes Of Newerth account."
                      + Environment.NewLine + "Please follow the link below to continue:"
                      + Environment.NewLine + Environment.NewLine + link
                      + Environment.NewLine + Environment.NewLine + "Regards,"
                      + Environment.NewLine + "The Project KONGOR Team";

        Console.WriteLine(body);

        await Task.Delay(250); return true;
    }

    public async Task<bool> SendEmailAddressRegistrationConfirmation(string emailAddress, string accountName)
    {
        const string subject = "Email Address Verified";

        string body = $"Hi {accountName},"
                      + Environment.NewLine + Environment.NewLine + "Congratulations on verifying the email address linked to your Heroes Of Newerth account."
                      + " " + "Please remember to be respectful to your fellow Newerthians, and to maintain your account in good standing."
                      + " " + "Suspensions carry over across accounts so, if you receive a suspension, you will not be able to log back into the game by creating a new account."
                      + Environment.NewLine + Environment.NewLine + "Regards,"
                      + Environment.NewLine + "The Project KONGOR Team";

        Console.WriteLine(body);

        await Task.Delay(250); return true;
    }
}
