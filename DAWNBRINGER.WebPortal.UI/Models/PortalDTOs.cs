namespace DAWNBRINGER.WebPortal.UI.Models;

// Email Address Registration

public class RegisterEmailAddressFormModel
{
    [Required(ErrorMessage = "Email Address Is Required")]
    [EmailAddress(ErrorMessage = "The Email Address Is Invalid")]
    [StringLength(128, ErrorMessage = "The Email Address Must Not Exceed 128 Characters")]
    public string EmailAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email Address Confirmation Is Required")]
    [EmailAddress(ErrorMessage = "The Email Address Confirmation Is Invalid")]
    [StringLength(128, ErrorMessage = "The Email Address Confirmation Must Not Exceed 128 Characters")]
    [Compare(nameof(EmailAddress), ErrorMessage = "The Email Addresses Do Not Match")]
    public string ConfirmEmailAddress { get; set; } = string.Empty;
}

// User Registration

public class CreateAccountFormModel
{
    [Required(ErrorMessage = "Account Name Is Required")]
    [StringLength(15, ErrorMessage = "The Account Name Must Be Between 4 And 15 Characters Long", MinimumLength = 4)]
    [RegularExpression(@"^[a-zA-Z0-9_\-]+$", ErrorMessage = "The Account Name May Only Contain Letters, Numbers, Underscores, And Hyphens")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password Is Required")]
    [StringLength(128, ErrorMessage = "The Password Must Be At Least 8 Characters Long", MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password Confirmation Is Required")]
    [Compare(nameof(Password), ErrorMessage = "The Passwords Do Not Match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

// Log In

public class LogInFormModel
{
    [Required(ErrorMessage = "Account Name Is Required")]
    [StringLength(15, ErrorMessage = "The Account Name Must Be Between 4 And 15 Characters Long", MinimumLength = 4)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password Is Required")]
    public string Password { get; set; } = string.Empty;
}

// Password Reset

public class RequestPasswordResetFormModel
{
    [Required(ErrorMessage = "Email Address Is Required")]
    [EmailAddress(ErrorMessage = "The Email Address Is Invalid")]
    [StringLength(128, ErrorMessage = "The Email Address Must Not Exceed 128 Characters")]
    public string EmailAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email Address Confirmation Is Required")]
    [EmailAddress(ErrorMessage = "The Email Address Confirmation Is Invalid")]
    [StringLength(128, ErrorMessage = "The Email Address Confirmation Must Not Exceed 128 Characters")]
    [Compare(nameof(EmailAddress), ErrorMessage = "The Email Addresses Do Not Match")]
    public string ConfirmEmailAddress { get; set; } = string.Empty;
}

// Password Update

public class RequestAccountPasswordUpdateFormModel
{
    [Required(ErrorMessage = "Current Password Is Required")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New Password Is Required")]
    [StringLength(128, ErrorMessage = "The New Password Must Be At Least 8 Characters Long", MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password Confirmation Is Required")]
    [Compare(nameof(Password), ErrorMessage = "The Passwords Do Not Match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

// Email Address Update

public class RequestEmailAddressUpdateFormModel
{
    [Required(ErrorMessage = "Email Address Is Required")]
    [EmailAddress(ErrorMessage = "The Email Address Is Invalid")]
    [StringLength(128, ErrorMessage = "The Email Address Must Not Exceed 128 Characters")]
    public string EmailAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email Address Confirmation Is Required")]
    [EmailAddress(ErrorMessage = "The Email Address Confirmation Is Invalid")]
    [StringLength(128, ErrorMessage = "The Email Address Confirmation Must Not Exceed 128 Characters")]
    [Compare(nameof(EmailAddress), ErrorMessage = "The Email Addresses Do Not Match")]
    public string ConfirmEmailAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password Is Required")]
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
