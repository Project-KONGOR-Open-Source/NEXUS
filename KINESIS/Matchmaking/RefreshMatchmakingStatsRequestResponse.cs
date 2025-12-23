namespace KINESIS.Matchmaking;

public class RefreshMatchmakingStatsRequestResponse : ProtocolResponse
{
    // Normal Matchmaking MMR
    private readonly float Rating;

    // Normal Matchmaking Rank (aka Medal)
    private readonly int Rank;

    // Normal Matchmaking Wins
    private readonly int NumberOfWins;

    // Normal Matchmaking Losses
    private readonly int NumberOfLosses;

    // Normal Matchmaking Win Streak
    private readonly int WinStreak;

    // Normal Matchmaking Matches Played
    private readonly int NumberOfMatchesPlayed;

    // Normal Matchmaking Placement Matches Played
    private readonly int NumberOfPlacementMatchesPlayed;

    // Results of Matchmaking Placement Matches
    private readonly string PlacementMatchesDetails;

    // Casual Matchmaking MMR
    private readonly float CasualRating;

    // Casual Matchmaking Rank (aka Medal)
    private readonly int CasualRank;

    // Casual Matchmaking Wins
    private readonly int CasualNumberOfWins;

    // Casual Matchmaking Losses
    private readonly int CasualNumberOfLosses;

    // Casual Matchmaking Win Streak
    private readonly int CasualWinStreak;

    // Casual Matchmaking Matches Played
    private readonly int CasualNumberOfMatchesPlayed;

    // Casual Matchmaking Placement Matches Played
    private readonly int CasualNumberOfPlacementMatchesPlayed;

    // Result of Casual Matchmaking Placement Matches
    private readonly string CasualPlacementMatchesDetails;

    // Matchmaking Eligible (Boolean)
    private readonly byte EligibleForMatchmaking;

    // Season End (Boolean)
    private readonly byte SeasonEnd;

    public RefreshMatchmakingStatsRequestResponse(float rating, int rank, int numberOfWins, int numberOfLosses, int winStreak, int numberOfMatchesPlayed, int numberOfPlacementMatchesPlayed, string placementMatchesDetails, float casualRating, int casualRank, int casualNumberOfWins, int casualNumberOfLosses, int casualWinStreak, int casualNumberOfMatchesPlayed, int casualNumberOfPlacementMatchesPlayed, string casualPlacementMatchesDetails, byte eligibleForMatchmaking, byte seasonEnd)
    {
        Rating = rating;
        Rank = rank;
        NumberOfWins = numberOfWins;
        NumberOfLosses = numberOfLosses;
        WinStreak = winStreak;
        NumberOfMatchesPlayed = numberOfMatchesPlayed;
        NumberOfPlacementMatchesPlayed = numberOfPlacementMatchesPlayed;
        PlacementMatchesDetails = placementMatchesDetails;
        CasualRating = casualRating;
        CasualRank = casualRank;
        CasualNumberOfWins = casualNumberOfWins;
        CasualNumberOfLosses = casualNumberOfLosses;
        CasualWinStreak = casualWinStreak;
        CasualNumberOfMatchesPlayed = casualNumberOfMatchesPlayed;
        CasualNumberOfPlacementMatchesPlayed = casualNumberOfPlacementMatchesPlayed;
        CasualPlacementMatchesDetails = casualPlacementMatchesDetails;
        EligibleForMatchmaking = eligibleForMatchmaking;
        SeasonEnd = seasonEnd;
    }

    public override CommandBuffer Encode()
    {
        CommandBuffer buffer = new();
        buffer.WriteInt16(ChatServerResponse.RefreshMatchmakingStatsResponse);
        buffer.WriteFloat32(Rating);
        buffer.WriteInt32(Rank);
        buffer.WriteInt32(NumberOfWins);
        buffer.WriteInt32(NumberOfLosses);
        buffer.WriteInt32(WinStreak);
        buffer.WriteInt32(NumberOfMatchesPlayed);
        buffer.WriteInt32(NumberOfPlacementMatchesPlayed);
        buffer.WriteString(PlacementMatchesDetails);
        buffer.WriteFloat32(CasualRating);
        buffer.WriteInt32(CasualRank);
        buffer.WriteInt32(CasualNumberOfWins);
        buffer.WriteInt32(CasualNumberOfLosses);
        buffer.WriteInt32(CasualWinStreak);
        buffer.WriteInt32(CasualNumberOfMatchesPlayed);
        buffer.WriteInt32(CasualNumberOfPlacementMatchesPlayed);
        buffer.WriteString(CasualPlacementMatchesDetails);
        buffer.WriteInt8(EligibleForMatchmaking);
        buffer.WriteInt8(SeasonEnd);

        return buffer;
    }
}
