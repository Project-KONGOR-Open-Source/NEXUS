namespace KONGOR.MasterServer.Controllers.Message;

[ApiController]
[Consumes("application/x-www-form-urlencoded")]
public class MessageController(MerrickContext databaseContext, IDatabase distributedCache, ILogger<MessageController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;

    private IDatabase DistributedCache { get; } = distributedCache;

    private ILogger Logger { get; } = logger;

    private const string PlaceholderMessageCRC = "welcome";

    private const string PlaceholderMessageImage = "/ui/fe2/NewUI/Res/system_message/msg_type1.png";

    private const string PlaceholderMessageSubject = "Welcome To Project KONGOR !";

    private const string PlaceholderMessageSubtitle = "keeping the real Heroes Of Newerth alive since 2022";

    private const string PlaceholderMessageBodyTitle = "Hello Newerthian" + ",";

    private const string PlaceholderMessageBody = @"<p>Project KONGOR is a community-driven effort to keep the real Heroes Of Newerth alive.</p><p class=""link""><a href=""https://github.com/Project-KONGOR-Open-Source"">Visit The Project On GitHub</a></p>";

    private const string PlaceholderMessageFooter = "[K]ONGOR";

    /// <summary>
    ///     Returns the manifest of the account's messages (subject, icon, and metadata, but not the body, which is fetched per-message by <see cref="MessageGet"/>).
    /// </summary>
    [HttpPost("message/list/{accountID:int}", Name = "Message List")]
    public async Task<IActionResult> MessageList(int accountID)
    {
        if (await ValidateMessageRequest(accountID) is { } error)
            return error;

        List<MessageManifestEntry> messageManifest =
        [
            new MessageManifestEntry
            (
                subject: PlaceholderMessageSubject,
                image: PlaceholderMessageImage,
                read: false,
                expiration: 0,
                sent: CurrentUNIXTimestamp(),
                crc: PlaceholderMessageCRC,
                deletable: true
            )
        ];

        return Ok(PhpSerialization.Serialize(new MessageListResponse { MessageManifest = messageManifest }));
    }

    /// <summary>
    ///     Returns a single message in full (including its body and metadata), identified by its CRC.
    /// </summary>
    [HttpPost("message/get/{accountID:int}/{crc}", Name = "Message Get")]
    public async Task<IActionResult> MessageGet(int accountID, string crc)
    {
        if (await ValidateMessageRequest(accountID) is { } error)
            return error;

        if (crc.Equals(PlaceholderMessageCRC).Equals(false))
        {
            OrderedDictionary notFoundResponse = new ()
            {
                { "success", false },
                { "message", "Not Found" }
            };

            return Ok(PhpSerialization.Serialize(notFoundResponse));
        }

        MessageDetail message = new
        (
            subject: PlaceholderMessageSubject,
            body: PlaceholderMessageBody,
            image: PlaceholderMessageImage,
            read: false,
            expiration: 0,
            sent: CurrentUNIXTimestamp(),
            crc: PlaceholderMessageCRC,
            deletable: true,

            // The In-Game Message Panel Renders The "subtitle" (Under The Title), The "bodyTitle" (A Header Above The Body), The "body", And The "footer" From The Metadata
            // The Body Is Also Provided At The Top Level For Contract Completeness, Even Though The Panel Reads It From The Metadata

            metadata: new Dictionary<string, string>
            {
                ["subtitle"] = PlaceholderMessageSubtitle,
                ["bodyTitle"] = PlaceholderMessageBodyTitle,
                ["body"] = PlaceholderMessageBody,
                ["footer"] = PlaceholderMessageFooter
            }
        );

        return Ok(PhpSerialization.Serialize(new MessageGetResponse { Data = message }));
    }

    /// <summary>
    ///     Deletes a single message, identified by its CRC.
    /// </summary>
    [HttpPost("message/delete/{accountID:int}/{crc}", Name = "Message Delete")]
    public async Task<IActionResult> MessageDelete(int accountID, string crc)
    {
        if (await ValidateMessageRequest(accountID) is { } error)
            return error;

        bool deleted = crc.Equals(PlaceholderMessageCRC);

        return Ok(PhpSerialization.Serialize(new MessageDeleteResponse { Success = deleted }));
    }

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

    private static int CurrentUNIXTimestamp() => Convert.ToInt32(Math.Min(DateTimeOffset.UtcNow.ToUnixTimeSeconds(), int.MaxValue));
}
