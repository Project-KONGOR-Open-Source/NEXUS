namespace ZORGATH.WebPortal.API.Contracts;

public record RequestAccountPasswordResetDTO(string EmailAddress);

public record ConfirmAccountPasswordResetDTO(string Token);

public record RequestAccountPasswordUpdateDTO(string CurrentPassword, string Password, string ConfirmPassword);

public record ConfirmAccountPasswordUpdateDTO(string Token);
