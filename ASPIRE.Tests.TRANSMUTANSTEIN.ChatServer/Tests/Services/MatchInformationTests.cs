namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Services;

/// <summary>
///     Covers <see cref="MatchmakingService.CreateMatchInformation"/>, in particular that the recorded game client version is taken from the matched groups rather than a hard-coded constant.
/// </summary>
public sealed class MatchInformationTests
{
    [Test]
    public async Task Match_Information_Version_Comes_From_The_Group_Client_Version()
    {
        // A Version Distinct From The Production Default, So The Assertion Fails If The Field Reverts To A Hard-Coded Constant
        const string clientVersion = "4.10.1.666";

        MatchmakingGroupInformation information = new ()
        {
            ClientVersion = clientVersion,
            GroupType     = ChatProtocol.TMMType.TMM_TYPE_PVP,
            GameType      = ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL,
            MapName       = "caldavar",
            GameModes     = ["ap"],
            GameRegions   = ["NEWERTH"],
            Ranked        = true,
            MatchFidelity = 0,
            BotDifficulty = 0,
            RandomizeBots = false
        };

        MatchmakingGroup legionGroup     = MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR, information: information);
        MatchmakingGroup hellbourneGroup = MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR, information: information);

        MatchmakingTeam legionTeam     = new () { Groups = [legionGroup],     TeamSize = 1 };
        MatchmakingTeam hellbourneTeam = new () { Groups = [hellbourneGroup], TeamSize = 1 };

        MatchmakingMatch match = new ()
        {
            LegionTeam     = legionTeam,
            HellbourneTeam = hellbourneTeam,
            GameType       = ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL,
            IsRanked       = true,
            SelectedMap    = "caldavar",
            SelectedMode   = "ap"
        };

        MatchServer server = new ()
        {
            HostAccountID        = 1,
            HostAccountName      = "Host",
            ID                   = 100,
            Name                 = "Test Server",
            MatchServerManagerID = null,
            Instance             = 1,
            IPAddress            = "127.0.0.1",
            Port                 = 11235,
            Location             = "NEWERTH",
            Description          = string.Empty
        };

        MatchInformation matchInformation = MatchmakingService.CreateMatchInformation(match, server, matchID: 12345);

        await Assert.That(matchInformation.Version).IsEqualTo(clientVersion);
    }
}
