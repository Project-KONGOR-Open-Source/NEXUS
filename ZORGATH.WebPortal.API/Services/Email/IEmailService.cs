namespace ZORGATH.WebPortal.API.Services.Email;

public interface IEmailService
{
    Task<bool> SendEmailAddressRegistrationLink(string emailAddress, string token);

    Task<bool> SendEmailAddressRegistrationConfirmation(string emailAddress, string accountName);

    // TODO: Define Email Service (Two Implementations: Real, Console)
}