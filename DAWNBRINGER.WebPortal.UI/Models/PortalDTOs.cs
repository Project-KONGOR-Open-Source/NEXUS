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
    [StringLength(12, ErrorMessage = "The account name must be between 4 and 12 characters long", MinimumLength = 4)]
    [RegularExpression(@"^[a-zA-Z0-9_\-`]+$", ErrorMessage = "The account name may only contain letters, numbers, underscores, hyphens, and backticks")]
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

// Password Recovery

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

public class ResetForgottenPasswordFormModel
{
    [Required(ErrorMessage = "Password is required")]
    [StringLength(128, ErrorMessage = "The password must be at least 8 characters long", MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

// API Request DTOs (Matching ZORGATH.WebPortal.API Contracts)

public record RegisterEmailAddressRequest(string EmailAddress, string ConfirmEmailAddress);

public record RegisterUserAndMainAccountRequest(string Token, string Name, string Password, string ConfirmPassword);

public record LogInUserRequest(string Name, string Password);

// API Response DTOs

public record AuthenticationTokenResponse(int UserID, string TokenType, string Token);

public record BasicUserResponse(int ID, string EmailAddress, List<BasicAccountResponse> Accounts);

public record BasicAccountResponse(int ID, string Name);
