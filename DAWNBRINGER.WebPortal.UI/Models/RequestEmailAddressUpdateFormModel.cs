namespace DAWNBRINGER.WebPortal.UI.Models;

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
