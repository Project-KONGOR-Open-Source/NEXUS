namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Statistics;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_CAMPAIGN_STATS)]
public class SeasonStatistics : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        SeasonStatisticsRequestData requestData = new(buffer);

        ChatBuffer response = new();

        response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_CAMPAIGN_STATS);
        response.WriteFloat32(1850.55f); // TMM Rating
        response.WriteInt32(15); // TMM Rank
        response.WriteInt32(6661); // TMM Wins
        response.WriteInt32(123); // TMM Losses
        response.WriteInt32(6662); // Ranked Win Streak
        response.WriteInt32(6663); // Ranked Matches Played
        response.WriteInt32(5); // Placement Matches Played
        response.WriteString("11011"); // Placement Status
        response.WriteFloat32(1950.55f); // Casual TMM Rating
        response.WriteInt32(10); // Casual TMM Rank
        response.WriteInt32(4441); // Casual TMM Wins
        response.WriteInt32(321); // Casual TMM Losses
        response.WriteInt32(4442); // Casual Ranked Win Streak
        response.WriteInt32(4443); // Casual Ranked Matches Played
        response.WriteInt32(6); // Casual Placement Matches Played
        response.WriteString("010101"); // Casual Placement Status
        response.WriteInt8(1); // Eligible For TMM
        response.WriteInt8(1); // Season End

        // TODO: Send Actual Season Statistics

        session.Send(response);

        // Also Respond With NET_CHAT_CL_TMM_POPULARITY_UPDATE Since The Client Will Not Explicitly Request It
        PopularityUpdate.SendMatchmakingPopularity(session);
    }
}

file class SeasonStatisticsRequestData
{
    public SeasonStatisticsRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }

    public byte[] CommandBytes { get; init; }
}