namespace DAWNBRINGER.WebPortal.UI.Models;

public class AddHostAccountFormModel
{
    [Required(ErrorMessage = "Account Name Is Required")]
    [StringLength(15, ErrorMessage = "The Account Name Must Be Between 4 And 15 Characters Long", MinimumLength = 4)]
    [RegularExpression(@"^[a-zA-Z0-9_\-]+$", ErrorMessage = "The Account Name May Only Contain Letters, Numbers, Underscores, And Hyphens")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Authorisation Token Is Required")]
    [GUID(ErrorMessage = "The Authorisation Token Must Be A Valid GUID")]
    public string Token { get; set; } = string.Empty;
}
