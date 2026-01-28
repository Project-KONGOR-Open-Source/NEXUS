using SendGrid;
using SendGrid.Helpers.Mail;

namespace ZORGATH.WebPortal.API.Services.Email;

public class EmailService(
    IOptions<OperationalConfiguration> configuration,
    ILogger<EmailService> logger,
    IWebHostEnvironment hostEnvironment,
    ISendGridClient sendGridClient) : IEmailService
{
    private OperationalConfiguration Configuration { get; } = configuration.Value;
    private ILogger Logger { get; } = logger;
    private IWebHostEnvironment HostEnvironment { get; } = hostEnvironment;
    private ISendGridClient SendGridClient { get; } = sendGridClient;

    private string BaseURL =>
        string.IsNullOrWhiteSpace(Configuration.UIBaseUrl)
            ? (HostEnvironment.IsDevelopment() ? "https://localhost:5557" : "https://portal.ui.kongor.net")
            : Configuration.UIBaseUrl;

    public async Task<bool> SendEmailAddressRegistrationLink(string emailAddress, string token)
    {
        string link = BaseURL + "/register/" + token;

        const string subject = "Verify Email Address";

        string body = "You need to verify your email address before you can create your Heroes Of Newerth account."
                      + Environment.NewLine + "Please follow the link below to continue:"
                      + Environment.NewLine + Environment.NewLine + link
                      + Environment.NewLine + Environment.NewLine + "Regards,"
                      + Environment.NewLine + "The Project KONGOR Team";

        return await SendEmail(emailAddress, subject, body);
    }

    public async Task<bool> SendEmailAddressRegistrationConfirmation(string emailAddress, string accountName)
    {
        const string subject = "Email Address Verified";

        string body = $"Hi {accountName},"
                      + Environment.NewLine + Environment.NewLine +
                      "Congratulations on verifying the email address linked to your Heroes Of Newerth account."
                      + " " +
                      "Please remember to be respectful to your fellow Newerthians, and to maintain your account in good standing."
                      + " " +
                      "Suspensions carry over across accounts so, if you receive a suspension, you will not be able to log back into the game by creating a new account."
                      + Environment.NewLine + Environment.NewLine + "Regards,"
                      + Environment.NewLine + "The Project KONGOR Team";

        return await SendEmail(emailAddress, subject, body);
    }

    private async Task<bool> SendEmail(string toEmail, string subject, string plainTextContent)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Configuration.Email.ApiKey) || Configuration.Email.ApiKey == "SENDGRID_API_KEY_PLACEHOLDER")
            {
                Logger.LogWarning("SendGrid API Key Not Configured. Falling Back To Console logging for development.");
                Console.WriteLine($"Subject: {subject}{Environment.NewLine}{plainTextContent}");
                await Task.Delay(250);
                return true;
            }



            EmailAddress from = new(Configuration.Email.FromEmail, Configuration.Email.FromName);
            EmailAddress to = new(toEmail);
            SendGridMessage msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, null);

            Response response = await SendGridClient.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                Logger.LogInformation("Email Sent Successfully to {ToEmail}", toEmail);
                return true;
            }

            string body = await response.Body.ReadAsStringAsync();
            Logger.LogError("Failed to send email to {ToEmail}. Status Code: {StatusCode}. Body: {Body}", toEmail, response.StatusCode, body);
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception Occurred While Sending Email to {ToEmail}", toEmail);
            return false;
        }
    }
}