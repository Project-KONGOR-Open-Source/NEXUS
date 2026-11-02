namespace DAWNBRINGER.WebPortal.UI.Models;

/// <summary>
///     The published and expiry times shared by the event and announcement forms, each picked as a date and a time in UTC.
/// </summary>
public abstract class ScheduledItemFormModel
{
    public DateTime? PublishedDate { get; set; }

    public TimeSpan? PublishedTime { get; set; }

    [Required(ErrorMessage = "Expiry Date Is Required")]
    public DateTime? ExpiresDate { get; set; }

    public TimeSpan? ExpiresTime { get; set; }

    private DateTimeOffset? OriginalPublished { get; set; }

    /// <summary>
    ///     Returns the published time, or <see langword="null"/> when no date is picked, so that a new item is published at once and an edited item keeps its time.
    ///     An edited item whose published time is unchanged to the minute also returns <see langword="null"/>, so that a picker which drops the seconds cannot move the time, which would make an announcement pop up again for every player.
    /// </summary>
    public DateTimeOffset? PublishedAt()
    {
        DateTimeOffset? published = Combine(PublishedDate, PublishedTime);

        if (published is DateTimeOffset chosen && OriginalPublished is DateTimeOffset original && chosen.UtcTicks / TimeSpan.TicksPerMinute == original.UtcTicks / TimeSpan.TicksPerMinute)
            return null;

        return published;
    }

    public DateTimeOffset? ExpiresAt() => Combine(ExpiresDate, ExpiresTime);

    protected void SetTimes(DateTimeOffset published, DateTimeOffset expires)
    {
        OriginalPublished = published;
        PublishedDate = published.UtcDateTime.Date;
        PublishedTime = published.UtcDateTime.TimeOfDay;
        ExpiresDate = expires.UtcDateTime.Date;
        ExpiresTime = expires.UtcDateTime.TimeOfDay;
    }

    // A Missing Time Means Midnight
    private static DateTimeOffset? Combine(DateTime? date, TimeSpan? time)
        => date is null ? null : new DateTimeOffset(DateTime.SpecifyKind(date.Value.Date + time.GetValueOrDefault(), DateTimeKind.Unspecified), TimeSpan.Zero);
}
