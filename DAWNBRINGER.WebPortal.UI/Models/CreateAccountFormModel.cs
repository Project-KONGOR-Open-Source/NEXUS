namespace DAWNBRINGER.WebPortal.UI.Models;

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
