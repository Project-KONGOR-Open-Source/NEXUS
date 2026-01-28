using System.ComponentModel.DataAnnotations;

namespace DAWNBRINGER.WebPortal.UI.Models;

public class RegisterDirectViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public required string Email { get; set; }

    [Required]
    [StringLength(15, MinimumLength = 3, ErrorMessage = "IGN must be between 3 and 15 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_.]+$",
        ErrorMessage = "IGN can only contain letters, numbers, underscores, and dots.")]
    public required string IGN { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    [DataType(DataType.Password)]
    public required string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public required string ConfirmPassword { get; set; }
}
