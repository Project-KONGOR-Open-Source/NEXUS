namespace ZORGATH.WebPortal.API.Services.Email;

internal interface IEmailService
{
    Task<bool> SendEmailAddressRegistrationLink(string emailAddress, string token);

    Task<bool> SendEmailAddressRegistrationConfirmation(string emailAddress, string accountName);
}
