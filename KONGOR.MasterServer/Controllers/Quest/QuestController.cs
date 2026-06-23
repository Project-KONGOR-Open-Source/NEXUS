namespace KONGOR.MasterServer.Controllers.Quest;

[ApiController]
[Consumes("application/x-www-form-urlencoded")]
public class QuestController(IOptions<OperationalConfiguration> configuration) : ControllerBase
{
    private OperationalConfiguration Configuration { get; } = configuration.Value;

    // Mirrors The Game Client's "EQuestsAvailabilityType": A Status Of "1" Marks The System As Live, While Any Other Value Disables It
    private const int QuestSystemLiveStatus = 1;

    private const string QuestSystemNotImplementedMessage = "The Quest System Is Marked As Live In The Operational Configuration, But Has Not Yet Been Implemented";

    private bool QuestSystemIsLive => Configuration.Quest.SystemStatus == QuestSystemLiveStatus;

    /// <summary>
    ///     Returns the quests and reward bags currently assigned to the requesting client's account.
    ///     While the quest system is disabled, this returns the quest-system-disabled response.
    /// </summary>
    [HttpPost("master/quest/getcurrentquests", Name = "Get Current Quests")]
    public IActionResult GetCurrentQuests()
    {
        if (QuestSystemIsLive)
            throw new NotImplementedException(QuestSystemNotImplementedMessage);

        return Ok(QuestSystemDisabledResponse());
    }

    /// <summary>
    ///     Resets one of the requesting client's quests and assigns a replacement in its place.
    ///     While the quest system is disabled, this returns the quest-system-disabled response.
    /// </summary>
    [HttpPost("master/quest/resetquest", Name = "Reset Quest")]
    public IActionResult ResetQuest()
    {
        if (QuestSystemIsLive)
            throw new NotImplementedException(QuestSystemNotImplementedMessage);

        return Ok(QuestSystemDisabledResponse());
    }

    /// <summary>
    ///     Opens a completed reward bag for the requesting client's account and grants its reward.
    ///     While the quest system is disabled, this returns the quest-system-disabled response.
    /// </summary>
    [HttpPost("master/quest/openbag", Name = "Open Bag")]
    public IActionResult OpenBag()
    {
        if (QuestSystemIsLive)
            throw new NotImplementedException(QuestSystemNotImplementedMessage);

        return Ok(QuestSystemDisabledResponse());
    }

    /// <summary>
    ///     Returns the quests currently assigned to the accounts named in the request, on behalf of the requesting game server.
    ///     While the quest system is disabled, this returns the quest-system-disabled response.
    /// </summary>
    [HttpPost("master/questserver/getplayerquests", Name = "Get Player Quests")]
    public IActionResult GetPlayerQuests()
    {
        if (QuestSystemIsLive)
            throw new NotImplementedException(QuestSystemNotImplementedMessage);

        return Ok(QuestSystemDisabledResponse());
    }

    /// <summary>
    ///     Returns the quest type and quest queue definitions, along with the bag cycle information, on behalf of the requesting game server.
    ///     While the quest system is disabled, this returns the quest-system-disabled response.
    /// </summary>
    [HttpPost("master/questserver/getsysteminfo", Name = "Get System Information")]
    public IActionResult GetSystemInformation()
    {
        if (QuestSystemIsLive)
            throw new NotImplementedException(QuestSystemNotImplementedMessage);

        return Ok(QuestSystemDisabledResponse());
    }

    private string QuestSystemDisabledResponse()
    {
        Dictionary<string, QuestSystem> response = new ()
        {
            { "error", new QuestSystem { QuestStatus = Configuration.Quest.SystemStatus, LeaderboardStatus = Configuration.Quest.LeaderboardStatus } }
        };

        return PhpSerialization.Serialize(response);
    }
}
