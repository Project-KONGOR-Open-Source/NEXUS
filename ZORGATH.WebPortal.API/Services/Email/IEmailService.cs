namespace ZORGATH.WebPortal.API.Services.Email;

public interface IEmailService
{
    public Task<bool> SendEmailAddressRegistrationLink(string emailAddress, string token);

    public Task<bool> SendEmailAddressRegistrationConfirmation(string emailAddress, string accountName);
}
