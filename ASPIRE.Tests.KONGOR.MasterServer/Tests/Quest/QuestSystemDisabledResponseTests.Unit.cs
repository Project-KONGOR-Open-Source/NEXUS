namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Quest;

/// <summary>
///     Pure-logic tests for the quest-system-disabled response serialisation, verifying the shape the game client receives while the quest system is disabled.
/// </summary>
public sealed class QuestSystemDisabledResponseTests_Unit
{
    [Test]
    public async Task Serialises_The_Disabled_Quest_System_In_The_Shape_The_Client_Expects()
    {
        Dictionary<string, QuestSystem> response = new ()
        {
            { "error", new QuestSystem { QuestStatus = 0, LeaderboardStatus = 0 } }
        };

        string serialised = PhpSerialization.Serialize(response);

        const string expected = @"a:1:{s:5:""error"";a:2:{s:12:""quest_status"";i:0;s:18:""leaderboard_status"";i:0;}}";

        await Assert.That(serialised).IsEqualTo(expected);
    }
}
