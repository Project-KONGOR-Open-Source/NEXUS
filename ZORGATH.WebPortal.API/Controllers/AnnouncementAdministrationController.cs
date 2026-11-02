namespace ZORGATH.WebPortal.API.Controllers;

/// <summary>
///     Lets administrators add, edit, list, and delete the announcements on the Project KONGOR Announcement Board, which the master server lists to every client at login until each announcement expires.
/// </summary>
[ApiController]
[Route("Administration/Announcements")]
[Consumes("application/json")]
[EnableRateLimiting(RateLimiterPolicies.Relaxed)]
[Authorize(Policy = UserRoles.RolesWithElevatedPrivileges)]
public class AnnouncementAdministrationController(IDatabase distributedCache, ILogger<AnnouncementAdministrationController> logger) : ControllerBase
{
    private IDatabase DistributedCache { get; } = distributedCache;
    private ILogger Logger { get; } = logger;

    /// <summary>
    ///     Lists every announcement which has not expired, scheduled ones included, newest first.
    /// </summary>
    [HttpGet("", Name = "Get Announcements")]
    [ProducesResponseType(typeof(List<AnnouncementDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnnouncements()
    {
        List<AnnouncementDTO> announcements = [.. (await DistributedCache.GetAnnouncements())
            .OrderByDescending(announcement => announcement.Published)
            .Select(ToDTO)];

        return Ok(announcements);
    }

    /// <summary>
    ///     Adds an announcement, unless the board already holds as many live or scheduled announcements as the client has room for.
    /// </summary>
    [HttpPost("", Name = "Create Announcement")]
    [ProducesResponseType(typeof(AnnouncementDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAnnouncement([FromBody] SaveAnnouncementDTO payload)
    {
        DateTimeOffset published = payload.Published ?? DateTimeOffset.UtcNow;

        ValidationResult validationResult = await new AnnouncementValidator(published).ValidateAsync(payload);

        if (validationResult.IsValid is false)
            return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage));

        if ((await DistributedCache.GetAnnouncements()).Count >= ContentLimits.UnexpiredAnnouncementsMaximum)
            return Conflict($"No More Than {ContentLimits.UnexpiredAnnouncementsMaximum} Announcements May Be Live Or Scheduled At Once");

        Announcement announcement = new (Guid.CreateVersion7(), payload.Title, SafeHTML.Sanitise(payload.Body), published, payload.Expires);

        await DistributedCache.SetAnnouncement(announcement);

        Logger.LogInformation(@"Created Announcement ""{AnnouncementID}""", announcement.ID);

        return CreatedAtAction(nameof(GetAnnouncements), ToDTO(announcement));
    }

    /// <summary>
    ///     Edits an announcement in place, keeping its identity and, unless a new one is given, its published time.
    ///     A new title or published time makes the announcement pop up again for every account.
    /// </summary>
    [HttpPut("{id:guid}", Name = "Update Announcement")]
    [ProducesResponseType(typeof(AnnouncementDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAnnouncement(Guid id, [FromBody] SaveAnnouncementDTO payload)
    {
        Announcement? existing = await DistributedCache.GetAnnouncement(id);

        if (existing is null)
            return NotFound();

        DateTimeOffset published = payload.Published ?? existing.Published;

        ValidationResult validationResult = await new AnnouncementValidator(published).ValidateAsync(payload);

        if (validationResult.IsValid is false)
            return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage));

        Announcement announcement = new (id, payload.Title, SafeHTML.Sanitise(payload.Body), published, payload.Expires);

        await DistributedCache.SetAnnouncement(announcement);

        Logger.LogInformation(@"Updated Announcement ""{AnnouncementID}""", announcement.ID);

        return Ok(ToDTO(announcement));
    }

    [HttpDelete("{id:guid}", Name = "Delete Announcement")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAnnouncement(Guid id)
    {
        if (await DistributedCache.RemoveAnnouncement(id) is false)
            return NotFound();

        Logger.LogInformation(@"Deleted Announcement ""{AnnouncementID}""", id);

        return NoContent();
    }

    private static AnnouncementDTO ToDTO(Announcement announcement)
        => new (announcement.ID, announcement.Title, announcement.Body, announcement.Published, announcement.Expires);
}
