namespace ZORGATH.WebPortal.API.Services.Email;

/// <summary>
///     Production email service implementation that sends emails via AWS SES SMTP with StartTLS and authentication.
/// </summary>
public class AWSSESEmailService(IOptions<OperationalConfiguration> configuration, ILogger<AWSSESEmailService> logger) : IEmailService
{
    private OperationalConfigurationSMTP SMTPConfiguration { get; } = configuration.Value.SMTP;

    private ILogger Logger { get; } = logger;

    public async Task<bool> SendEmailAddressRegistrationLink(string emailAddress, string token)
    {
        string link = "https://portal.ui.kongor.net/register/" + token;

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
                      + Environment.NewLine + Environment.NewLine + "Congratulations on verifying the email address linked to your Heroes Of Newerth account."
                      + " " + "Please remember to be respectful to your fellow Newerthians, and to maintain your account in good standing."
                      + " " + "Suspensions carry over across accounts so, if you receive a suspension, you will not be able to log back into the game by creating a new account."
                      + Environment.NewLine + Environment.NewLine + "Regards,"
                      + Environment.NewLine + "The Project KONGOR Team";

        return await SendEmail(emailAddress, subject, body);
    }

    public async Task<bool> SendAccountPasswordRecoveryLink(string emailAddress, string token, List<string> accountNames)
    {
        string link = "https://portal.ui.kongor.net/recover/" + token;

        const string subject = "Reset Forgotten Password";

        string accountNamesBody = accountNames.Count > 0
            ? "Accounts:" + Environment.NewLine + string.Join(Environment.NewLine, accountNames) + Environment.NewLine
            : "No accounts found." + Environment.NewLine;

        string body = "A password reset has been requested for an account that is registered with this email address."
                      + Environment.NewLine + "If you did not make this request, please ignore this message."
                      + Environment.NewLine + Environment.NewLine + accountNamesBody
                      + Environment.NewLine + "Please follow the link below to continue:"
                      + Environment.NewLine + Environment.NewLine + link
                      + Environment.NewLine + Environment.NewLine + "Regards,"
                      + Environment.NewLine + "The Project KONGOR Team";

        return await SendEmail(emailAddress, subject, body);
    }

    public async Task<bool> SendAccountPasswordRecoveryConfirmation(string emailAddress, List<string> accountNames)
    {
        const string subject = "Account Password Was Reset";

        string body = $@"The password for all accounts linked to email address ""{emailAddress}"" has been changed:" + Environment.NewLine
                      + string.Join(", ", accountNames)
                      + Environment.NewLine + Environment.NewLine + "Regards,"
                      + Environment.NewLine + "The Project KONGOR Team";

        return await SendEmail(emailAddress, subject, body);
    }

    public async Task<bool> SendEmailAddressUpdateLink(string emailAddress, string token)
    {
        string link = "https://portal.ui.kongor.net/update/" + token;

        const string subject = "Update Email Address";

        string body = "An email address update has been requested for an account that is registered with this email address."
                      + Environment.NewLine + "If you did not make this request, please ignore this message."
                      + Environment.NewLine + "Please follow the link below to continue:"
                      + Environment.NewLine + Environment.NewLine + link
                      + Environment.NewLine + Environment.NewLine + "Regards,"
                      + Environment.NewLine + "The Project KONGOR Team";

        return await SendEmail(emailAddress, subject, body);
    }

    public async Task<bool> SendEmailAddressUpdateConfirmation(string oldEmailAddress, string newEmailAddress)
    {
        const string subject = "Email Address Was Updated";

        string body = $@"All accounts linked to previous email address ""{oldEmailAddress}"" are now linked to current email address ""{newEmailAddress}""."
                      + Environment.NewLine + Environment.NewLine + "Regards,"
                      + Environment.NewLine + "The Project KONGOR Team";

        return await SendEmail(newEmailAddress, subject, body);
    }

    private async Task<bool> SendEmail(string emailAddress, string subject, string body)
    {
        MimeMessage message = new();

        message.From.Add(new MailboxAddress(SMTPConfiguration.SenderName, SMTPConfiguration.SenderAddress));
        message.To.Add(InternetAddress.Parse(emailAddress));
        message.Subject = subject;
        message.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = body };

        using SmtpClient client = new();

        if (SMTPConfiguration.Port is null)
        {
            Logger.LogError("Failed To Send Email To {EmailAddress} Using AWS SES: SMTP Port Is Not Configured", emailAddress);

            return false;
        }

        try
        {
            await client.ConnectAsync(SMTPConfiguration.Host, SMTPConfiguration.Port.Value, MailKit.Security.SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(SMTPConfiguration.Username, SMTPConfiguration.Password);

            string response = await client.SendAsync(message);

            // AWS SES Returns A Response Starting With "Ok" On Success (e.g. "Ok 010b018307ef6101-59cfc741-dcbf-44a5-a935-b76452b87bf3-000000")
            if (response.StartsWith("OK", StringComparison.OrdinalIgnoreCase))
                return true;

            Logger.LogError("Email Sending Failure Using AWS SES To {EmailAddress}: {Response}", emailAddress, response);

            return false;
        }

        catch (Exception exception)
        {
            Logger.LogError(exception, "Failed To Send Email To {EmailAddress} Using AWS SES", emailAddress);

            return false;
        }

        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(quit: true);
        }
    }
}
