namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Results;

/// <summary>
///     Handles the result of a match server's attempt to submit match statistics to the master server.
///     The match server reports whether stat submission succeeded, failed, or encountered an error, along with the match ID, how long the request took, and the account IDs of the players involved.
/// </summary>
[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_STAT_SUBMISSION_RESULT)]
public class StatisticsSubmissionResult : ISynchronousCommandProcessor<MatchServerChatSession>
{
    public void Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        StatisticsSubmissionResultRequestData requestData = new (buffer);

        if (requestData.Result is ChatProtocol.StatSubmissionResult.SSR_SUCCESS)
        {
            Log.Information(@"Statistics Submission Succeeded For Match {MatchID} On Server ID {ServerID} In {RequestTime}ms",
                requestData.MatchID, session.Metadata.ServerID, requestData.RequestTimeMilliseconds);

            // TODO: Update Group MMRs For Players In The Match
        }

        else
        {
            Log.Error(@"Statistics Submission Failed For Match {MatchID} On Server ID {ServerID} With Result ""{Result}"" In {RequestTime}ms",
                requestData.MatchID, session.Metadata.ServerID, requestData.Result, requestData.RequestTimeMilliseconds);
        }
    }
}

file class StatisticsSubmissionResultRequestData
{
    public byte[] CommandBytes { get; init; }

    public ChatProtocol.StatSubmissionResult Result { get; init; }

    public int MatchID { get; init; }

    public int RequestTimeMilliseconds { get; init; }

    public byte PlayerCount { get; init; }

    public int[] AccountIDs { get; init; }

    public StatisticsSubmissionResultRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        Result = (ChatProtocol.StatSubmissionResult) buffer.ReadInt8();
        MatchID = buffer.ReadInt32();
        RequestTimeMilliseconds = buffer.ReadInt32();
        PlayerCount = buffer.ReadInt8();
        AccountIDs = new int[PlayerCount];

        for (int i = 0; i < PlayerCount; i++)
        {
            AccountIDs[i] = buffer.ReadInt32();
        }
    }
}
