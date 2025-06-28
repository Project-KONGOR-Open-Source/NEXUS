namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Statistics;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_CAMPAIGN_STATS)]
public class SeasonStats(MerrickContext merrick, ILogger<SeasonStats> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<SeasonStats> Logger { get; set; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        SeasonStatsRequestData requestData = new (buffer);

        Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_CAMPAIGN_STATS);

        // TODO: Send Actual Season Statistics

        Response.WriteFloat32(1850.55f);    // TMM Rating
        Response.WriteInt32(15);            // TMM Rank
        Response.WriteInt32(6661);          // TMM Wins
        Response.WriteInt32(123);           // TMM Losses
        Response.WriteInt32(6662);          // Ranked Win Streak
        Response.WriteInt32(6663);          // Ranked Matches Played
        Response.WriteInt32(5);             // Placement Matches Played
        Response.WriteString("11011");      // Placement Status
        Response.WriteFloat32(1950.55f);    // Casual TMM Rating
        Response.WriteInt32(10);            // Casual TMM Rank
        Response.WriteInt32(4441);          // Casual TMM Wins
        Response.WriteInt32(321);           // Casual TMM Losses
        Response.WriteInt32(4442);          // Casual Ranked Win Streak
        Response.WriteInt32(4443);          // Casual Ranked Matches Played
        Response.WriteInt32(6);             // Casual Placement Matches Played
        Response.WriteString("010101");     // Casual Placement Status
        Response.WriteInt8(1);              // Eligible For TMM
        Response.WriteInt8(1);              // Season End

        Response.PrependBufferSize();

        session.SendAsync(Response.Data);

        Response = new ChatBuffer(); // Also Respond With NET_CHAT_CL_TMM_POPULARITY_UPDATE Since The Client Will Not Explicitly Request It

        await PopularityUpdate.SendMatchmakingPopularity(session, buffer, Response);
    }
}

public class SeasonStatsRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
}
