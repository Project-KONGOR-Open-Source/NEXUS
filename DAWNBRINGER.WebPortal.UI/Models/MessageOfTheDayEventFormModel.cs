namespace DAWNBRINGER.WebPortal.UI.Models;

public class MessageOfTheDayEventFormModel : ScheduledItemFormModel
{
    [Required(ErrorMessage = "Title Is Required")]
    [StringLength(ContentLimits.TitleMaximumLength, ErrorMessage = "The Title Must Be At Most {1} Characters Long")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Subtitle Is Required")]
    [StringLength(ContentLimits.SubtitleMaximumLength, ErrorMessage = "The Subtitle Must Be At Most {1} Characters Long")]
    public string Subtitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Body Is Required")]
    [StringLength(ContentLimits.BodyMaximumLength, ErrorMessage = "The Body Must Be At Most {1} Characters Long")]
    public string Body { get; set; } = string.Empty;

    /// <summary>
    ///     The picture's file, as base64.
    /// </summary>
    public string Picture { get; set; } = string.Empty;

    public static MessageOfTheDayEventFormModel From(MessageOfTheDayEventResponse item)
    {
        MessageOfTheDayEventFormModel model = new () { Title = item.Title, Subtitle = item.Subtitle, Body = item.Body, Picture = item.Picture };

        model.SetTimes(item.Published, item.Expires);

        return model;
    }
}
