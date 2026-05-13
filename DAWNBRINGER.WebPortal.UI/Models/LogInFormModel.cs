namespace DAWNBRINGER.WebPortal.UI.Models;

public class LogInFormModel
{
    [Required(ErrorMessage = "Account Name Is Required")]
    [StringLength(15, ErrorMessage = "The Account Name Must Be Between 4 And 15 Characters Long", MinimumLength = 4)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password Is Required")]
    public string Password { get; set; } = string.Empty;
}
