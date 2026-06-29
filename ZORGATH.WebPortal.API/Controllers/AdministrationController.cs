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
        const int MessageSubjectMaximumLength = 100;
        const int MessageSubtitleMaximumLength = 80;
        const int MessageBodyTitleMaximumLength = 100;
        const int MessageBodyMaximumLength = 2000;
        const int MessageFooterMaximumLength = 80;

        // Messages Are Inserted In Batches So That A Broadcast To Many Accounts Does Not Build One Enormous Transaction Or Change-Tracker Graph.
        const int MessageBroadcastBatchSize = 1000;

        if (string.IsNullOrWhiteSpace(payload.Subject) || payload.Subject.Length > MessageSubjectMaximumLength)
            return BadRequest($@"The Subject Is Required And Must Not Exceed {MessageSubjectMaximumLength} Characters");

        if (string.IsNullOrWhiteSpace(payload.Body) || payload.Body.Length > MessageBodyMaximumLength)
            return BadRequest($@"The Body Is Required And Must Not Exceed {MessageBodyMaximumLength} Characters");

        if (payload.Subtitle?.Length > MessageSubtitleMaximumLength)
            return BadRequest($@"The Subtitle Must Not Exceed {MessageSubtitleMaximumLength} Characters");

        if (payload.BodyTitle?.Length > MessageBodyTitleMaximumLength)
            return BadRequest($@"The Body Title Must Not Exceed {MessageBodyTitleMaximumLength} Characters");

        if (payload.Footer?.Length > MessageFooterMaximumLength)
            return BadRequest($@"The Footer Must Not Exceed {MessageFooterMaximumLength} Characters");

        List<int> accountIDs = await MerrickContext.Accounts.Select(account => account.ID).ToListAsync();

        for (int index = 0; index < accountIDs.Count; index += MessageBroadcastBatchSize)
        {
            IEnumerable<Message> batch = accountIDs
                .Skip(index)
                .Take(MessageBroadcastBatchSize)
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
