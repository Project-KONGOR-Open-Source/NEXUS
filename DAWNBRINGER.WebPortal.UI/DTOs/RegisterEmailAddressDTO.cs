using System.ComponentModel.DataAnnotations;

namespace DAWNBRINGER.WebPortal.UI.DTOs;

public class RegisterEmailAddressDTO
{
    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(EmailAddress))]
    public string ConfirmEmailAddress { get; set; } = string.Empty;
}
