namespace ZORGATH.WebPortal.API.Validators;

/// <summary>
///     Validates a message of the day event against the limits in <see cref="ContentLimits"/>, which the master server's page is built around.
///     The published time is passed in, since an edited event without one keeps the time it already has.
/// </summary>
public class MessageOfTheDayEventValidator : AbstractValidator<SaveMessageOfTheDayEventDTO>
{
    // The Length Of The Base64 Text Of The Largest Picture Accepted, Checked Before Decoding So That An Oversized Picture Is Never Decoded
    private const int PictureMaximumBase64Length = (ContentLimits.PictureMaximumBytes + 2) / 3 * 4;

    public MessageOfTheDayEventValidator(DateTimeOffset published)
    {
        RuleFor(item => item.Title).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Title Must Not Be Empty")
            .MaximumLength(ContentLimits.TitleMaximumLength).WithMessage($"Title Must Be At Most {ContentLimits.TitleMaximumLength} Characters Long");

        RuleFor(item => item.Subtitle).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Subtitle Must Not Be Empty")
            .MaximumLength(ContentLimits.SubtitleMaximumLength).WithMessage($"Subtitle Must Be At Most {ContentLimits.SubtitleMaximumLength} Characters Long");

        RuleFor(item => item.Body).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Body Must Not Be Empty")
            .MaximumLength(ContentLimits.BodyMaximumLength).WithMessage($"Body Must Be At Most {ContentLimits.BodyMaximumLength} Characters Long")
            .Must(SafeHTML.IsWithinSubset).WithMessage("Body May Only Contain Paragraphs, Line Breaks, Bold, Italic, And Underlined Text, Lists, And Links To HTTP Or HTTPS Addresses");

        RuleFor(item => item.Picture).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Picture Must Not Be Empty")
            .MaximumLength(PictureMaximumBase64Length).WithMessage($"Picture Must Be At Most {ContentLimits.PictureMaximumBytes / 1_048_576.0:0.#} MB")
            .Must(IsACardPicture).WithMessage($"Picture Must Be A Base64-Encoded PNG, JPEG, Or WebP Picture Of Exactly {ContentLimits.PictureWidth} By {ContentLimits.PictureHeight} Pixels");

        RuleFor(item => item.Expires)
            .GreaterThan(published).WithMessage("Expiry Must Be After The Published Time");

        RuleFor(item => item.Expires)
            .Must(expires => expires > DateTimeOffset.UtcNow).WithMessage("Expiry Must Be In The Future");
    }

    private static bool IsACardPicture(string picture)
    {
        byte[] buffer = new byte[picture.Length / 4 * 3];

        if (Convert.TryFromBase64String(picture, buffer, out int length) is false)
            return false;

        PictureInformation? information = PictureFormat.Read(buffer.AsSpan(0, length));

        return information is { Width: ContentLimits.PictureWidth, Height: ContentLimits.PictureHeight };
    }
}
