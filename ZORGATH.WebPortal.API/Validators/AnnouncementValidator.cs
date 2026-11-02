namespace ZORGATH.WebPortal.API.Validators;

/// <summary>
///     Validates an announcement against the limits in <see cref="ContentLimits"/>, which the client's board is built around.
///     The published time is passed in, since an edited announcement without one keeps the time it already has.
/// </summary>
public class AnnouncementValidator : AbstractValidator<SaveAnnouncementDTO>
{
    public AnnouncementValidator(DateTimeOffset published)
    {
        RuleFor(announcement => announcement.Title).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Title Must Not Be Empty")
            .MaximumLength(ContentLimits.TitleMaximumLength).WithMessage($"Title Must Be At Most {ContentLimits.TitleMaximumLength} Characters Long");

        RuleFor(announcement => announcement.Body).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Body Must Not Be Empty")
            .MaximumLength(ContentLimits.BodyMaximumLength).WithMessage($"Body Must Be At Most {ContentLimits.BodyMaximumLength} Characters Long")
            .Must(SafeHTML.IsWithinSubset).WithMessage("Body May Only Contain Paragraphs, Line Breaks, Bold, Italic, And Underlined Text, Lists, And Links To HTTP Or HTTPS Addresses");

        RuleFor(announcement => announcement.Expires)
            .GreaterThan(published).WithMessage("Expiry Must Be After The Published Time");

        RuleFor(announcement => announcement.Expires)
            .Must(expires => expires > DateTimeOffset.UtcNow).WithMessage("Expiry Must Be In The Future");
    }
}
