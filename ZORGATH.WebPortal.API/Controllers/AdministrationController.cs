namespace ZORGATH.WebPortal.API.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[EnableRateLimiting(RateLimiterPolicies.Strict)]
public class AdministrationController(MerrickContext databaseContext, ILogger<AdministrationController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger Logger { get; } = logger;

    private static readonly TimeSpan HostAccountAuthorisationTokenValidity = TimeSpan.FromHours(24);

    [HttpPost("Authorise/Host", Name = "Issue Host Account Authorisation Token")]
    [Authorize(Policy = UserRoles.RolesWithElevatedPrivileges)]
    [ProducesResponseType(typeof(HostAccountAuthorisationTokenDTO), StatusCodes.Status201Created)]
    public async Task<IActionResult> IssueHostAccountAuthorisationToken()
    {
        Token token = new ()
        {
            Purpose = TokenPurpose.HostAccountAuthorisation,
            EmailAddress = string.Empty,
            Value = Guid.CreateVersion7(),
            Data = AccountType.ServerHost.ToString(),
            Validity = HostAccountAuthorisationTokenValidity
        };

        await MerrickContext.Tokens.AddAsync(token);
        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Issued Host Account Authorisation Token ""{Token}""", token.Value);

        HostAccountAuthorisationTokenDTO response = new (token.Value, token.TimestampCreated + token.Validity);

        return CreatedAtAction(nameof(IssueHostAccountAuthorisationToken), response);
    }

    /// <summary>
    ///     Broadcasts a system message to every account's in-game inbox.
    ///     This is the retroactive counterpart to the system messages seeded at account creation: it reaches accounts that already exist.
    /// </summary>
    [HttpPost("Broadcast", Name = "Broadcast System Message To All Accounts")]
    [Authorize(Policy = UserRoles.RolesWithElevatedPrivileges)]
    [ProducesResponseType(typeof(BroadcastSystemMessageResultDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BroadcastSystemMessage([FromBody] BroadcastSystemMessageDTO payload)
    {
        // These Limits Mirror The Maximum Lengths Of The Corresponding Columns On The "Message" Entity.
        const int messageSubjectMaximumLength = 100;
        const int messageSubtitleMaximumLength = 80;
        const int messageBodyTitleMaximumLength = 100;
        const int messageBodyMaximumLength = 2000;
        const int messageFooterMaximumLength = 80;

        // Messages Are Inserted In Batches So That A Broadcast To Many Accounts Does Not Build One Enormous Transaction Or Change-Tracker Graph.
        const int messageBroadcastBatchSize = 1000;

        if (string.IsNullOrWhiteSpace(payload.Subject) || payload.Subject.Length > messageSubjectMaximumLength)
            return BadRequest($@"The Subject Is Required And Must Not Exceed {messageSubjectMaximumLength} Characters");

        if (string.IsNullOrWhiteSpace(payload.Body) || payload.Body.Length > messageBodyMaximumLength)
            return BadRequest($@"The Body Is Required And Must Not Exceed {messageBodyMaximumLength} Characters");

        if (payload.Subtitle?.Length > messageSubtitleMaximumLength)
            return BadRequest($@"The Subtitle Must Not Exceed {messageSubtitleMaximumLength} Characters");

        if (payload.BodyTitle?.Length > messageBodyTitleMaximumLength)
            return BadRequest($@"The Body Title Must Not Exceed {messageBodyTitleMaximumLength} Characters");

        if (payload.Footer?.Length > messageFooterMaximumLength)
            return BadRequest($@"The Footer Must Not Exceed {messageFooterMaximumLength} Characters");

        List<int> accountIDs = await MerrickContext.Accounts.Select(account => account.ID).ToListAsync();

        for (int index = 0; index < accountIDs.Count; index += messageBroadcastBatchSize)
        {
            IEnumerable<Message> batch = accountIDs
                .Skip(index)
                .Take(messageBroadcastBatchSize)
                .Select(accountID => new Message
                {
                    AccountID = accountID,
                    Subject = payload.Subject,
                    Subtitle = payload.Subtitle ?? string.Empty,
                    BodyTitle = payload.BodyTitle ?? string.Empty,
                    Body = payload.Body,
                    Footer = payload.Footer ?? string.Empty
                });

            await MerrickContext.Messages.AddRangeAsync(batch);
            await MerrickContext.SaveChangesAsync();

            // Detach The Just-Saved Messages So The Change Tracker Does Not Accumulate Across Batches
            MerrickContext.ChangeTracker.Clear();
        }

        Logger.LogInformation(@"Broadcast A System Message (""{Subject}"") To {AccountCount} Account(s)", payload.Subject, accountIDs.Count);

        return Ok(new BroadcastSystemMessageResultDTO(accountIDs.Count));
    }
}
