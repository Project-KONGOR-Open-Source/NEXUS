namespace KONGOR.MasterServer.Controllers.Quest;

[ApiController]
[Consumes("application/x-www-form-urlencoded")]
public class QuestController(MerrickContext databaseContext, IDatabase distributedCache, ILogger<QuestController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;

    private IDatabase DistributedCache { get; } = distributedCache;

    private ILogger Logger { get; } = logger;

    /// <summary>
    ///     Returns the quests and reward bags currently assigned to the requesting client's account.
    /// </summary>
    [HttpPost("master/quest/getcurrentquests", Name = "Get Current Quests")]
    public async Task<IActionResult> GetCurrentQuests()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid.Equals(false) || accountName is null)
        {
            Logger.LogWarning(@"Get Current Quests Request With Invalid Cookie ""{SessionCookie}"" From ""{IPAddress}""",
                cookie, Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN");

            return Unauthorized(@$"Unrecognised Cookie ""{cookie}""");
        }

        Account account = await MerrickContext.Accounts
            .SingleAsync(queriedAccount => queriedAccount.Name.Equals(accountName));

        // TODO: Build And Return The Quests And Reward Bags Currently Assigned To The "account", Once The Quest System Is Implemented

        return Ok(QuestSystemDisabledResponse());
    }

    /// <summary>
    ///     Resets one of the requesting client's quests and returns the replacement assigned in its place.
    /// </summary>
    [HttpPost("master/quest/resetquest", Name = "Reset Quest")]
    public async Task<IActionResult> ResetQuest()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid.Equals(false) || accountName is null)
        {
            Logger.LogWarning(@"Reset Quest Request With Invalid Cookie ""{SessionCookie}"" From ""{IPAddress}""",
                cookie, Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN");

            return Unauthorized(@$"Unrecognised Cookie ""{cookie}""");
        }

        string? questIDValue = Request.Form["quest_id"];

        if (questIDValue is null || int.TryParse(questIDValue, out int questID).Equals(false))
            return BadRequest(@"Missing Or Invalid Value For Form Parameter ""quest_id""");

        Account account = await MerrickContext.Accounts
            .SingleAsync(queriedAccount => queriedAccount.Name.Equals(accountName));

        // TODO: Reset The Quest With ID "questID" For The "account" And Return Its Replacement, Once The Quest System Is Implemented

        return Ok(QuestSystemDisabledResponse());
    }

    /// <summary>
    ///     Opens a completed reward bag for the requesting client's account and grants its reward.
    /// </summary>
    [HttpPost("master/quest/openbag", Name = "Open Bag")]
    public async Task<IActionResult> OpenBag()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid.Equals(false) || accountName is null)
        {
            Logger.LogWarning(@"Open Bag Request With Invalid Cookie ""{SessionCookie}"" From ""{IPAddress}""",
                cookie, Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN");

            return Unauthorized(@$"Unrecognised Cookie ""{cookie}""");
        }

        string? bagIDValue = Request.Form["bag_id"];

        if (bagIDValue is null || int.TryParse(bagIDValue, out int bagID).Equals(false))
            return BadRequest(@"Missing Or Invalid Value For Form Parameter ""bag_id""");

        Account account = await MerrickContext.Accounts
            .SingleAsync(queriedAccount => queriedAccount.Name.Equals(accountName));

        // TODO: Open The Reward Bag With ID "bagID" For The "account" And Grant Its Reward, Once The Quest System Is Implemented

        return Ok(QuestSystemDisabledResponse());
    }

    /// <summary>
    ///     Returns the quests currently assigned to the accounts named in the request, on behalf of the requesting game server.
    /// </summary>
    [HttpPost("master/questserver/getplayerquests", Name = "Get Player Quests")]
    public async Task<IActionResult> GetPlayerQuests()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        MatchServer? matchServer = await DistributedCache.GetMatchServerBySessionCookie(cookie);

        if (matchServer is null)
        {
            Logger.LogWarning(@"Get Player Quests Request With Invalid Server Session Cookie ""{SessionCookie}"" From ""{IPAddress}""",
                cookie, Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN");

            return Unauthorized(@$"Unrecognised Cookie ""{cookie}""");
        }

        List<int> accountIDs = [];

        foreach (string? accountIDValue in Request.Form["account_ids[]"])
        {
            if (int.TryParse(accountIDValue, out int accountID))
                accountIDs.Add(accountID);
        }

        // TODO: Build And Return The Current Quests For The Accounts In "accountIDs", Once The Quest System Is Implemented

        return Ok(QuestSystemDisabledResponse());
    }

    /// <summary>
    ///     Returns the quest type and quest queue definitions, along with the bag cycle information, on behalf of the requesting game server.
    /// </summary>
    [HttpPost("master/questserver/getsysteminfo", Name = "Get System Information")]
    public async Task<IActionResult> GetSystemInformation()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        MatchServer? matchServer = await DistributedCache.GetMatchServerBySessionCookie(cookie);

        if (matchServer is null)
        {
            Logger.LogWarning(@"Get System Information Request With Invalid Server Session Cookie ""{SessionCookie}"" From ""{IPAddress}""",
                cookie, Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN");

            return Unauthorized(@$"Unrecognised Cookie ""{cookie}""");
        }

        // TODO: Build And Return The Quest Type And Quest Queue Definitions And The Bag Cycle Information, Once The Quest System Is Implemented

        return Ok(QuestSystemDisabledResponse());
    }

    private static string QuestSystemDisabledResponse()
    {
        Dictionary<string, QuestSystem> response = new ()
        {
            { "error", new QuestSystem() }
        };

        return PhpSerialization.Serialize(response);
    }
}
