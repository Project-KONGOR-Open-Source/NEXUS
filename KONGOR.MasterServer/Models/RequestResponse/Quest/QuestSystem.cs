namespace KONGOR.MasterServer.Models.RequestResponse.Quest;

/// <summary>
///     The availability information for the quest system and its leaderboard.
///     While the quest system is disabled, this is returned wrapped in a dictionary keyed by "error", with all of its properties set to "0".
/// </summary>
public class QuestSystem
{
    /// <summary>
    ///     The availability of the quest system, mirroring the game client's "EQuestsAvailabilityType" enumeration.
    ///     A value of "0" indicates that the quest system is disabled, while a value of "1" indicates that it is live.
    /// </summary>
    [PHPProperty("quest_status")]
    public int QuestStatus { get; set; } = 0;

    /// <summary>
    ///     The availability of the quest leaderboard, mirroring the game client's "EQuestsAvailabilityType" enumeration.
    ///     A value of "0" indicates that the quest leaderboard is disabled, while a value of "1" indicates that it is live.
    /// </summary>
    [PHPProperty("leaderboard_status")]
    public int LeaderboardStatus { get; set; } = 0;
}
