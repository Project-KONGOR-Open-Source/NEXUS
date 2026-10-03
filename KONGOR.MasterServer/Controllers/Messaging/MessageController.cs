namespace KONGOR.MasterServer.Controllers.Messaging;

[ApiController]
[Consumes("application/x-www-form-urlencoded")]
public class MessageController(MerrickContext databaseContext, IDatabase distributedCache, ILogger<MessageController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;

    private IDatabase DistributedCache { get; } = distributedCache;

    private ILogger Logger { get; } = logger;

    /// <summary>
    ///     Returns the manifest of the account's messages (subject, icon, and metadata, but not the body, which is fetched per-message by <see cref="MessageGet"/>).
    /// </summary>
    [HttpPost("message/list/{accountID:int}", Name = "Message List")]
    public async Task<IActionResult> MessageList(int accountID)
    {
        if (await ValidateMessageRequest(accountID) is { } error)
            return error;

        DateTimeOffset now = DateTimeOffset.UtcNow;

        List<Message> messages = await MerrickContext.Messages
            .Where(message => message.AccountID == accountID && (message.TimestampExpires == null || message.TimestampExpires > now))
            .OrderByDescending(message => message.TimestampSent)
            .ToListAsync();

        List<MessageManifestEntry> messageManifest = messages
            .Select(message => new MessageManifestEntry
            (
                subject: message.Subject,
                image: message.Image,
                read: message.Read,
                expiration: ToUNIXTimestamp(message.TimestampExpires),
                sent: ToUNIXTimestamp(message.TimestampSent),
                crc: message.ID.ToString(),
                deletable: message.Deletable
            ))
            .ToList();

        return Ok(PhpSerialization.Serialize(new MessageListResponse { MessageManifest = messageManifest }));
    }

    /// <summary>
    ///     Returns a single message in full (including its body and metadata), identified by its CRC, and marks it as read.
    /// </summary>
    [HttpPost("message/get/{accountID:int}/{crc}", Name = "Message Get")]
    public async Task<IActionResult> MessageGet(int accountID, string crc)
    {
        if (await ValidateMessageRequest(accountID) is { } error)
            return error;

        Message? message = await ResolveMessage(accountID, crc);

        if (message is null)
        {
            OrderedDictionary notFoundResponse = new ()
            {
                { "success", false },
                { "message", "Not Found" }
            };

            return Ok(PhpSerialization.Serialize(notFoundResponse));
        }

        if (message.Read.Equals(false))
        {
            message.Read = true;

            await MerrickContext.SaveChangesAsync();
        }

        MessageDetail messageDetail = new
        (
            subject: message.Subject,
            body: message.Body,
            image: message.Image,
            read: message.Read,
            expiration: ToUNIXTimestamp(message.TimestampExpires),
            sent: ToUNIXTimestamp(message.TimestampSent),
            crc: message.ID.ToString(),
            deletable: message.Deletable,

            // The In-Game Message Panel Renders The "subtitle" (Under The Title), The "bodyTitle" (A Header Above The Body), The "body", And The "footer" From The Metadata
            metadata: new Dictionary<string, string>
            {
                ["subtitle"] = message.Subtitle,
                ["bodyTitle"] = message.BodyTitle,
                ["body"] = message.Body,
                ["footer"] = message.Footer
            }
        );

        return Ok(PhpSerialization.Serialize(new MessageGetResponse { Data = messageDetail }));
    }

    /// <summary>
    ///     Deletes a single message, identified by its CRC, when the account is permitted to delete it.
    /// </summary>
    [HttpPost("message/delete/{accountID:int}/{crc}", Name = "Message Delete")]
    public async Task<IActionResult> MessageDelete(int accountID, string crc)
    {
        if (await ValidateMessageRequest(accountID) is { } error)
            return error;

        Message? message = await ResolveMessage(accountID, crc);

        bool deleted = false;

        if (message is not null && message.Deletable)
        {
            MerrickContext.Messages.Remove(message);

            await MerrickContext.SaveChangesAsync();

            deleted = true;
        }

        return Ok(PhpSerialization.Serialize(new MessageDeleteResponse { Success = deleted }));
    }

    /// <summary>
    ///     Resolves the message identified by the client-supplied CRC (the message's identifier) for the given account.
    /// </summary>
    private async Task<Message?> ResolveMessage(int accountID, string crc)
        => int.TryParse(crc, out int messageID)
            ? await MerrickContext.Messages.SingleOrDefaultAsync(message => message.ID == messageID && message.AccountID == accountID)
            : null;

    /// <summary>
    ///     Authenticates the session cookie and confirms it was issued to the account whose messages are being requested.
    ///     Returns the failure result to send to the client, or <see langword="null"/> when the request is authorised.
    /// </summary>
    private async Task<IActionResult?> ValidateMessageRequest(int accountID)
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid.Equals(false) || accountName is null)
        {
            Logger.LogWarning(@"Message Request With Invalid Cookie ""{SessionCookie}"" From ""{IPAddress}""",
                cookie, Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN");

            return Unauthorized(@$"Unrecognised Cookie ""{cookie}""");
        }

        Account account = await MerrickContext.Accounts
            .SingleAsync(queriedAccount => queriedAccount.Name.Equals(accountName));

        if (account.ID != accountID)
        {
            Logger.LogWarning(@"Account ID ""{AccountID}"" Attempted To Access Messages For Account ID ""{RequestedAccountID}""", account.ID, accountID);

            return Unauthorized(@$"Session Cookie ""{cookie}"" Does Not Correspond To Account ID ""{accountID}""");
        }

        return null;
    }

    private static int ToUNIXTimestamp(DateTimeOffset? timestamp)
        => timestamp is null ? 0 : Convert.ToInt32(Math.Min(timestamp.Value.ToUnixTimeSeconds(), int.MaxValue));
}
