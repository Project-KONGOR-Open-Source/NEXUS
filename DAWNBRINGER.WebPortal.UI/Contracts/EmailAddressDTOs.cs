namespace DAWNBRINGER.WebPortal.UI.Contracts;

public record RegisterEmailAddressRequest(string EmailAddress, string ConfirmEmailAddress);

public record RequestEmailAddressUpdateRequest(string EmailAddress, string Password);

public record ConfirmEmailAddressUpdateRequest(string Token);
