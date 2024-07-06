namespace ZORGATH.WebPortal.API.Services.Email;

// TODO: Implement Secret Management Component
// TODO: Implement Real Email Service

public class EmailService(IOptions<OperationalConfiguration> configuration, ILogger<EmailService> logger) : IEmailService
{
    private OperationalConfiguration Configuration { get; } = configuration.Value;
    private ILogger Logger { get; } = logger;

    private string BaseURL { get; } = ZORGATH.RunsInDevelopmentMode is true ? "https://localhost:55510" : "https://portal.api.kongor.net";

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

        // TODO: Add "try/catch" Block And Return "false" On Failure

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

        // TODO: Add "try/catch" Block And Return "false" On Failure

        await Task.Delay(250); return true;
    }
}
