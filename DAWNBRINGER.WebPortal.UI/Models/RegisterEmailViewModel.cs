using System.ComponentModel.DataAnnotations;

namespace DAWNBRINGER.WebPortal.UI.Models;

public class RegisterEmailViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public required string Email { get; set; }
}
