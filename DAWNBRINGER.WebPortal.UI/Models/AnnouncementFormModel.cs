namespace DAWNBRINGER.WebPortal.UI.Models;

public class AnnouncementFormModel : ScheduledItemFormModel
{
    [Required(ErrorMessage = "Title Is Required")]
    [StringLength(ContentLimits.TitleMaximumLength, ErrorMessage = "The Title Must Be At Most {1} Characters Long")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Body Is Required")]
    [StringLength(ContentLimits.BodyMaximumLength, ErrorMessage = "The Body Must Be At Most {1} Characters Long")]
    public string Body { get; set; } = string.Empty;

    public static AnnouncementFormModel From(AnnouncementResponse announcement)
    {
        AnnouncementFormModel model = new () { Title = announcement.Title, Body = announcement.Body };

        model.SetTimes(announcement.Published, announcement.Expires);

        return model;
    }
}
