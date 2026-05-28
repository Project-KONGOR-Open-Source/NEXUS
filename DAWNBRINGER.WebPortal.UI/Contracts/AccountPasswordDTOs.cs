namespace DAWNBRINGER.WebPortal.UI.Contracts;

public record RequestAccountPasswordResetRequest(string EmailAddress);

public record ConfirmAccountPasswordResetRequest(string Token);

public record RequestAccountPasswordUpdateRequest(string CurrentPassword, string Password, string ConfirmPassword);

public record ConfirmAccountPasswordUpdateRequest(string Token);
