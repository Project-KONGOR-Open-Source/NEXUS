namespace ZORGATH.WebPortal.API.Controllers;

/// <summary>
///     Lets administrators add, edit, list, and delete the message of the day's events, which the master server shows beside its permanent items until each event expires.
/// </summary>
[ApiController]
[Route("Administration/MessageOfTheDay/Events")]
[Consumes("application/json")]
[EnableRateLimiting(RateLimiterPolicies.Relaxed)]
[Authorize(Policy = UserRoles.RolesWithElevatedPrivileges)]
public class MessageOfTheDayAdministrationController(IDatabase distributedCache, ILogger<MessageOfTheDayAdministrationController> logger) : ControllerBase
{
    private IDatabase DistributedCache { get; } = distributedCache;
    private ILogger Logger { get; } = logger;

    /// <summary>
    ///     Lists every event which has not expired, scheduled ones included, newest first.
    /// </summary>
    [HttpGet("", Name = "Get Message Of The Day Events")]
    [ProducesResponseType(typeof(List<MessageOfTheDayEventDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvents()
    {
        List<MessageOfTheDayEventDTO> events = [.. (await DistributedCache.GetMessageOfTheDayEvents())
            .OrderByDescending(item => item.Published)
            .Select(ToDTO)];

        return Ok(events);
    }

    [HttpPost("", Name = "Create Message Of The Day Event")]
    [ProducesResponseType(typeof(MessageOfTheDayEventDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEvent([FromBody] SaveMessageOfTheDayEventDTO payload)
    {
        DateTimeOffset published = payload.Published ?? DateTimeOffset.UtcNow;

        ValidationResult validationResult = await new MessageOfTheDayEventValidator(published).ValidateAsync(payload);

        if (validationResult.IsValid is false)
            return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage));

        MessageOfTheDayItem item = new (Guid.CreateVersion7(), payload.Title, payload.Subtitle, SafeHTML.Sanitise(payload.Body), payload.Picture, published, payload.Expires);

        await DistributedCache.SetMessageOfTheDayEvent(item);

        Logger.LogInformation(@"Created Message Of The Day Event ""{EventID}""", item.ID);

        return CreatedAtAction(nameof(GetEvents), ToDTO(item));
    }

    /// <summary>
    ///     Edits an event in place, keeping its identity and, unless a new one is given, its published time, so that it keeps its place in the carousel.
    /// </summary>
    [HttpPut("{id:guid}", Name = "Update Message Of The Day Event")]
    [ProducesResponseType(typeof(MessageOfTheDayEventDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] SaveMessageOfTheDayEventDTO payload)
    {
        MessageOfTheDayItem? existing = await DistributedCache.GetMessageOfTheDayEvent(id);

        if (existing is null)
            return NotFound();

        DateTimeOffset published = payload.Published ?? existing.Published;

        ValidationResult validationResult = await new MessageOfTheDayEventValidator(published).ValidateAsync(payload);

        if (validationResult.IsValid is false)
            return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage));

        MessageOfTheDayItem item = new (id, payload.Title, payload.Subtitle, SafeHTML.Sanitise(payload.Body), payload.Picture, published, payload.Expires);

        await DistributedCache.SetMessageOfTheDayEvent(item);

        Logger.LogInformation(@"Updated Message Of The Day Event ""{EventID}""", item.ID);

        return Ok(ToDTO(item));
    }

    [HttpDelete("{id:guid}", Name = "Delete Message Of The Day Event")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(Guid id)
    {
        if (await DistributedCache.RemoveMessageOfTheDayEvent(id) is false)
            return NotFound();

        Logger.LogInformation(@"Deleted Message Of The Day Event ""{EventID}""", id);

        return NoContent();
    }

    // Events Are Always Stored With An Expiry, Since The Cache Refuses Them Otherwise
    private static MessageOfTheDayEventDTO ToDTO(MessageOfTheDayItem item)
        => new (item.ID, item.Title, item.Subtitle, item.Body, item.Picture, item.Published, item.Expires.GetValueOrDefault());
}
