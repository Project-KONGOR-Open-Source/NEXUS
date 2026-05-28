namespace DAWNBRINGER.WebPortal.UI.Models;

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
