namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class QuestSystem
{
    /// <summary>
    ///     Unknown.
    /// </summary>
    [PhpProperty("quest_status")]
    public int QuestStatus { get; set; } = 0;

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PhpProperty("leaderboard_status")]
    public int LeaderboardStatus { get; set; } = 0;
}