namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Locks down <see cref="MatchmakingMatch.FromBotGroup"/>.
///     A co-op group always becomes a one-team match with no Hellbourne side. The game type is forced to casual, the map to caldavar, and the mode to <c>botmatch</c>.
/// </summary>
public sealed class BotMatchTests
{
    [Test]
    public async Task Bot_Match_Has_No_Hellbourne_Team()
    {
        MatchmakingGroupInformation coopInformation = MatchmakingTestBuilder.CoopInformation();
        MatchmakingGroup coopGroup = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: coopInformation);

        MatchmakingMatch match = MatchmakingMatch.FromBotGroup(coopGroup);

        using (Assert.Multiple())
        {
            await Assert.That(match.IsBotMatch).IsTrue();
            await Assert.That(match.HellbourneTeam).IsNull();
            await Assert.That(match.LegionTeam).IsNotNull();
        }
    }

    [Test]
    public async Task Bot_Match_Forces_Casual_Game_Type_And_Caldavar_Map()
    {
        // The Co-Op Information Specifies CASUAL Already, But "FromBotGroup" Hardcodes The Output Regardless

        MatchmakingGroupInformation coopInformation = MatchmakingTestBuilder.CoopInformation();
        MatchmakingGroup coopGroup = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR], information: coopInformation);

        MatchmakingMatch match = MatchmakingMatch.FromBotGroup(coopGroup);

        using (Assert.Multiple())
        {
            await Assert.That(match.GameType).IsEqualTo(ChatProtocol.TMMGameType.TMM_GAME_TYPE_CASUAL);
            await Assert.That(match.SelectedMap).IsEqualTo("caldavar");
            await Assert.That(match.SelectedMode).IsEqualTo("botmatch");
            await Assert.That(match.IsRanked).IsFalse();
        }
    }

    [Test]
    public async Task Bot_Match_Carries_Bot_Difficulty_From_Group_Information()
    {
        MatchmakingGroupInformation coopInformation = MatchmakingTestBuilder.Information(groupType: ChatProtocol.TMMType.TMM_TYPE_COOP, botDifficulty: 3);
        MatchmakingGroup coopGroup = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: coopInformation);

        MatchmakingMatch match = MatchmakingMatch.FromBotGroup(coopGroup);

        await Assert.That(match.BotDifficulty).IsEqualTo((byte)3);
    }

    [Test]
    public async Task Bot_Match_Marks_The_Group_As_Matched()
    {
        MatchmakingGroupInformation coopInformation = MatchmakingTestBuilder.CoopInformation();
        MatchmakingGroup coopGroup = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR], information: coopInformation);

        MatchmakingMatch match = MatchmakingMatch.FromBotGroup(coopGroup);

        using (Assert.Multiple())
        {
            await Assert.That(coopGroup.MatchedUp).IsTrue();
            await Assert.That(coopGroup.AssignedMatchGUID).IsEqualTo(match.GUID);
            await Assert.That(coopGroup.AssignedTeamGUID).IsEqualTo(match.LegionTeam.GUID);
        }
    }
}
