namespace ZORGATH.WebPortal.API.Services.Email;

/// <summary>
///     Development email service implementation that sends emails to a local MailPit SMTP server without authentication.
///     MailPit provides a web interface for inspecting sent emails, making it ideal for development and testing.
/// </summary>
public class MailPitEmailService(IOptions<OperationalConfiguration> configuration, ILogger<MailPitEmailService> logger) : IEmailService
{
    private OperationalConfigurationSMTP SMTPConfiguration { get; } = configuration.Value.SMTP;

    private ILogger Logger { get; } = logger;

    public async Task<bool> SendEmailAddressRegistrationLink(string emailAddress, string token)
    {
        string link = "https://localhost:5557/account/register/" + token;

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

    public async Task<bool> SendAccountPasswordResetLink(string emailAddress, string token, string generatedPassword, List<string> accountNames)
    {
        string link = "https://localhost:5557/password/recover/" + token;

        const string subject = "Reset Forgotten Password";

        string accountNamesBody = accountNames.Count > 0
            ? "Account(s): " + string.Join(", ", accountNames)
            : "No accounts found.";

        string body = "A password reset has been requested for an account that is registered with this email address."
                      + Environment.NewLine + "If you did not make this request, please ignore this message."
                      + Environment.NewLine + Environment.NewLine + accountNamesBody
                      + Environment.NewLine + $@"Your new password will be: ""{generatedPassword}"""
                      + Environment.NewLine + Environment.NewLine + "Please follow the link below to confirm and activate this new password:"
                      + Environment.NewLine + Environment.NewLine + link
                      + Environment.NewLine + Environment.NewLine + "Regards,"
                      + Environment.NewLine + "The Project KONGOR Team";

        return await SendEmail(emailAddress, subject, body);
    }

    public async Task<bool> SendAccountPasswordResetConfirmation(string emailAddress, List<string> accountNames)
    {
        const string subject = "Account Password Was Reset";

        string body = $@"The password for all accounts linked to email address ""{emailAddress}"" has been reset:" + Environment.NewLine
                      + string.Join(", ", accountNames)
                      + Environment.NewLine + Environment.NewLine + "Regards,"
                      + Environment.NewLine + "The Project KONGOR Team";

        return await SendEmail(emailAddress, subject, body);
    }

    public async Task<bool> SendAccountPasswordUpdateLink(string emailAddress, string token, List<string> accountNames)
    {
        string link = "https://localhost:5557/password/update/" + token;

        const string subject = "Confirm Password Update";

        string accountNamesBody = accountNames.Count > 0
            ? "Account(s): " + string.Join(", ", accountNames)
            : "No accounts found.";

        string body = "A password update has been requested for an account that is registered with this email address."
                      + Environment.NewLine + "If you did not make this request, please ignore this message."
                      + Environment.NewLine + Environment.NewLine + accountNamesBody
                      + Environment.NewLine + "Please follow the link below to confirm and activate your new password:"
                      + Environment.NewLine + Environment.NewLine + link
                      + Environment.NewLine + Environment.NewLine + "Regards,"
                      + Environment.NewLine + "The Project KONGOR Team";

        return await SendEmail(emailAddress, subject, body);
    }

    public async Task<bool> SendAccountPasswordUpdateConfirmation(string emailAddress, List<string> accountNames)
    {
        const string subject = "Account Password Was Updated";

        string body = $@"The password for all accounts linked to email address ""{emailAddress}"" has been updated:" + Environment.NewLine
                      + string.Join(", ", accountNames)
                      + Environment.NewLine + Environment.NewLine + "Regards,"
                      + Environment.NewLine + "The Project KONGOR Team";

        return await SendEmail(emailAddress, subject, body);
    }

    public async Task<bool> SendEmailAddressUpdateLink(string emailAddress, string token)
    {
        string link = "https://localhost:5557/email/update/" + token;

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
        MimeMessage message = new ();

        message.From.Add(new MailboxAddress(SMTPConfiguration.SenderName, SMTPConfiguration.SenderAddress));
        message.To.Add(InternetAddress.Parse(emailAddress));
        message.Subject = subject;
        message.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = body };

        using SmtpClient client = new ();

        if (string.IsNullOrWhiteSpace(SMTPConfiguration.Host))
        {
            Logger.LogError("Failed To Send Email To {EmailAddress} Using MailPit: SMTP Host Is Not Configured", emailAddress);

            return false;
        }

        if (SMTPConfiguration.Port is null)
        {
            Logger.LogError("Failed To Send Email To {EmailAddress} Using MailPit: SMTP Port Is Not Configured", emailAddress);

            return false;
        }

        try
        {
            // MailPit Accepts Unencrypted Connections On Its SMTP Port
            await client.ConnectAsync(SMTPConfiguration.Host, SMTPConfiguration.Port.Value, MailKit.Security.SecureSocketOptions.None);

            await client.SendAsync(message);

            Logger.LogDebug("Email Sent To {EmailAddress} Using MailPit: {Subject}", emailAddress, subject);

            return true;
        }

        catch (Exception exception)
        {
            Logger.LogError(exception, "Failed To Send Email To {EmailAddress} Using MailPit", emailAddress);

            return false;
        }

        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(quit: true);
        }
    }
}
