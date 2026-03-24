namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Services;

/// <summary>
///     Stub email service for test environments that records sent emails without requiring an SMTP connection.
/// </summary>
public sealed class StubEmailService : IEmailService
{
    public List<(string EmailAddress, string Token)> RegistrationLinks { get; } = [];
    public List<(string EmailAddress, string AccountName)> RegistrationConfirmations { get; } = [];
    public List<(string EmailAddress, string Token, List<string> AccountNames)> PasswordRecoveryLinks { get; } = [];
    public List<(string EmailAddress, List<string> AccountNames)> PasswordRecoveryConfirmations { get; } = [];
    public List<(string EmailAddress, string Token)> EmailAddressUpdateLinks { get; } = [];
    public List<(string OldEmailAddress, string NewEmailAddress)> EmailAddressUpdateConfirmations { get; } = [];

    public Task<bool> SendEmailAddressRegistrationLink(string emailAddress, string token)
    {
        RegistrationLinks.Add((emailAddress, token));
        return Task.FromResult(true);
    }

    public Task<bool> SendEmailAddressRegistrationConfirmation(string emailAddress, string accountName)
    {
        RegistrationConfirmations.Add((emailAddress, accountName));
        return Task.FromResult(true);
    }

    public Task<bool> SendPasswordRecoveryLink(string emailAddress, string token, List<string> accountNames)
    {
        PasswordRecoveryLinks.Add((emailAddress, token, accountNames));
        return Task.FromResult(true);
    }

    public Task<bool> SendPasswordRecoveryConfirmation(string emailAddress, List<string> accountNames)
    {
        PasswordRecoveryConfirmations.Add((emailAddress, accountNames));
        return Task.FromResult(true);
    }

    public Task<bool> SendEmailAddressUpdateLink(string emailAddress, string token)
    {
        EmailAddressUpdateLinks.Add((emailAddress, token));
        return Task.FromResult(true);
    }

    public Task<bool> SendEmailAddressUpdateConfirmation(string oldEmailAddress, string newEmailAddress)
    {
        EmailAddressUpdateConfirmations.Add((oldEmailAddress, newEmailAddress));
        return Task.FromResult(true);
    }
}
