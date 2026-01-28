namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class QuestSystem
{
    /// <summary>
    ///     Unknown.
    /// </summary>
    [PHPProperty("quest_status")]
    public int QuestStatus { get; set; } = 0;

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PHPProperty("leaderboard_status")]
    public int LeaderboardStatus { get; set; } = 0;
}
