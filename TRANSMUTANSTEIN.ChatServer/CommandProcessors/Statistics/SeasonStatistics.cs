namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Statistics;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_CAMPAIGN_STATS)]
public class SeasonStatistics(MerrickContext merrick) : IAsynchronousCommandProcessor<ClientChatSession>
{
    public async Task Process(ClientChatSession session, ChatBuffer buffer)
    {
        SeasonStatisticsRequestData requestData = new (buffer);

        List<AccountStatistics> statistics = await merrick.AccountStatistics
            .Where(accountStatistics => accountStatistics.AccountID == session.Account.ID).ToListAsync();

        AccountStatistics? rankedNormalStatistics = statistics.SingleOrDefault(accountStatistics => accountStatistics.Type == AccountStatisticsType.Matchmaking);
        AccountStatistics? rankedCasualStatistics = statistics.SingleOrDefault(accountStatistics => accountStatistics.Type == AccountStatisticsType.MatchmakingCasual);

        ChatBuffer response = new ();

        response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_CAMPAIGN_STATS);

        WriteQueueStatistics(response, rankedNormalStatistics);
        WriteQueueStatistics(response, rankedCasualStatistics);

        response.WriteInt8(1); // Eligible For TMM
        response.WriteInt8(1); // Season End

        session.Send(response);

        // Also Respond With NET_CHAT_CL_TMM_POPULARITY_UPDATE Since The Client Will Not Explicitly Request It
        PopularityUpdate.SendMatchmakingPopularity(session);
    }

    /// <summary>
    ///     Writes a single matchmaking queue's season statistics to the response.
    ///     While the queue's placement phase is incomplete the rank level is written as zero, which the client uses to display the placement match results instead of a rank medal.
    /// </summary>
    private static void WriteQueueStatistics(ChatBuffer response, AccountStatistics? statistics)
    {
        double rating = statistics?.SkillRating ?? 1500.0;
        string placementMatchesData = statistics?.PlacementMatchesData ?? string.Empty;
        bool isInPlacementPhase = statistics?.IsInPlacementPhase ?? false;

        int rankLevel = isInPlacementPhase ? 0 : RankExtensions.CalculateCampaignLevel(rating);

        response.WriteFloat32((float) rating);                // TMM Rating
        response.WriteInt32(rankLevel);                       // TMM Rank
        response.WriteInt32(statistics?.MatchesWon ?? 0);     // TMM Wins
        response.WriteInt32(statistics?.MatchesLost ?? 0);    // TMM Losses
        response.WriteInt32(0);                               // Ranked Win Streak // TODO: Implement Win Streak Tracking
        response.WriteInt32(statistics?.MatchesPlayed ?? 0);  // Ranked Matches Played
        response.WriteInt32(placementMatchesData.Length);     // Placement Matches Played
        response.WriteString(placementMatchesData);           // Placement Status
    }
}

file class SeasonStatisticsRequestData
{
    public byte[] CommandBytes { get; init; }

    public SeasonStatisticsRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }
}
