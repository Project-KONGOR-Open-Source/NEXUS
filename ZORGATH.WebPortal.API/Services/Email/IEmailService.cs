namespace ZORGATH.WebPortal.API.Services.Email;

/// <summary>
///     Defines the contract for sending transactional emails related to user account management.
/// </summary>
public interface IEmailService
{
    /// <summary>
    ///     Sends an email containing a link for the user to verify their email address during registration.
    /// </summary>
    Task<bool> SendEmailAddressRegistrationLink(string emailAddress, string token);

    /// <summary>
    ///     Sends a confirmation email after the user has successfully verified their email address and created an account.
    /// </summary>
    Task<bool> SendEmailAddressRegistrationConfirmation(string emailAddress, string accountName);

    /// <summary>
    ///     Sends an email containing a generated random password and a confirmation link for the user to activate the reset.
    /// </summary>
    Task<bool> SendAccountPasswordResetLink(string emailAddress, string token, string generatedPassword, List<string> accountNames);

    /// <summary>
    ///     Sends a confirmation email after the user has successfully confirmed their account password reset.
    /// </summary>
    Task<bool> SendAccountPasswordResetConfirmation(string emailAddress, List<string> accountNames);

    /// <summary>
    ///     Sends an email containing a confirmation link for the user to activate their chosen new password.
    /// </summary>
    Task<bool> SendAccountPasswordUpdateLink(string emailAddress, string token, List<string> accountNames);

    /// <summary>
    ///     Sends a confirmation email after the user has successfully confirmed their account password update.
    /// </summary>
    Task<bool> SendAccountPasswordUpdateConfirmation(string emailAddress, List<string> accountNames);

    /// <summary>
    ///     Sends an email containing a link for the user to confirm an email address update.
    /// </summary>
    Task<bool> SendEmailAddressUpdateLink(string emailAddress, string token);

    /// <summary>
    ///     Sends a confirmation email after the user has successfully updated their email address.
    /// </summary>
    Task<bool> SendEmailAddressUpdateConfirmation(string oldEmailAddress, string newEmailAddress);
}
