namespace DAWNBRINGER.WebPortal.UI.Models;

// Email Address Registration

public class RegisterEmailAddressFormModel
{
    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "The email address is invalid")]
    public string EmailAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address confirmation is required")]
    [EmailAddress(ErrorMessage = "The email address is invalid")]
    [Compare(nameof(EmailAddress), ErrorMessage = "The email addresses do not match")]
    public string ConfirmEmailAddress { get; set; } = string.Empty;
}

// User Registration

public class CreateAccountFormModel
{
    [Required(ErrorMessage = "Account name is required")]
    // The Server Allows Up To 16 Characters In Development; Use Swagger UI To Bypass This Limit
    [StringLength(12, ErrorMessage = "The account name must be between 4 and 12 characters long", MinimumLength = 4)]
    [RegularExpression(@"^[a-zA-Z0-9_\-]+$", ErrorMessage = "The account name may only contain letters, numbers, underscores, and hyphens")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(128, ErrorMessage = "The password must be at least 8 characters long", MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

// Log In

public class LogInFormModel
{
    [Required(ErrorMessage = "Account name is required")]
    [StringLength(12, ErrorMessage = "The account name must be between 4 and 12 characters long", MinimumLength = 4)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}

// Password Reset

public class RequestPasswordResetFormModel
{
    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "The email address is invalid")]
    public string EmailAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address confirmation is required")]
    [EmailAddress(ErrorMessage = "The email address is invalid")]
    [Compare(nameof(EmailAddress), ErrorMessage = "The email addresses do not match")]
    public string ConfirmEmailAddress { get; set; } = string.Empty;
}

// Password Update

public class RequestAccountPasswordUpdateFormModel
{
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(128, ErrorMessage = "The password must be at least 8 characters long", MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

// Email Address Update

public class RequestEmailAddressUpdateFormModel
{
    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "The email address is invalid")]
    public string EmailAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address confirmation is required")]
    [EmailAddress(ErrorMessage = "The email address is invalid")]
    [Compare(nameof(EmailAddress), ErrorMessage = "The email addresses do not match")]
    public string ConfirmEmailAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}

// API Request DTOs (Matching ZORGATH.WebPortal.API Contracts)

public record RegisterEmailAddressRequest(string EmailAddress, string ConfirmEmailAddress);

public record RegisterUserAndMainAccountRequest(string Token, string Name, string Password, string ConfirmPassword);

public record LogInUserRequest(string Name, string Password);

public record RequestAccountPasswordResetRequest(string EmailAddress);

public record ConfirmAccountPasswordResetRequest(string Token);

public record RequestAccountPasswordUpdateRequest(string CurrentPassword, string Password, string ConfirmPassword);

public record ConfirmAccountPasswordUpdateRequest(string Token);

public record RequestEmailAddressUpdateRequest(string EmailAddress, string Password);

public record ConfirmEmailAddressUpdateRequest(string Token);

// API Response DTOs

public record AuthenticationTokenResponse(int UserID, string TokenType, string Token);

public record BasicUserResponse(int ID, string EmailAddress, List<BasicAccountResponse> Accounts);

public record BasicAccountResponse(int ID, string Name);

public record UserResponse(int ID, string EmailAddress, string Role, string CurrentAccountName, string CurrentClanName, string CurrentClanTag, DateTimeOffset TimestampCreated, int TotalLevel, int CurrentAccountLevel, List<UserAccountResponse> Accounts);

public record UserAccountResponse(int ID, string Name, bool IsMain, string AccountType, string ClanName, string ClanTag);
