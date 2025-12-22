using System.ComponentModel.DataAnnotations;

namespace DAWNBRINGER.WebPortal.UI.DTOs;

public class RegisterUserWithDiscordDTO
{
    [Required]
    public string RegistrationToken { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
