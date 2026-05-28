namespace ZORGATH.WebPortal.API.Contracts;

public record RegisterEmailAddressDTO(string EmailAddress, string ConfirmEmailAddress);

public record RequestEmailAddressUpdateDTO(string EmailAddress, string Password);

public record ConfirmEmailAddressUpdateDTO(string Token);
