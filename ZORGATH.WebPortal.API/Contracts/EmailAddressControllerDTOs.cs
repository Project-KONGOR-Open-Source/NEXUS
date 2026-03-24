namespace ZORGATH.WebPortal.API.Contracts;

public record RegisterEmailAddressDTO(string EmailAddress, string ConfirmEmailAddress);

public record RequestAccountPasswordRecoveryDTO(string EmailAddress);

public record ResetAccountPasswordDTO(string Token, string Password, string ConfirmPassword);

public record RequestEmailAddressUpdateDTO(string EmailAddress, string Password);

public record ConfirmEmailAddressUpdateDTO(string Token);
