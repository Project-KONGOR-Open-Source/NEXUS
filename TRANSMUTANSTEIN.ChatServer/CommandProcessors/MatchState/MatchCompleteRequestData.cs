namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

public class MatchCompleteRequestData
{
    public MatchCompleteRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();

        MatchID = buffer.ReadInt32();
        Reason = (ChatProtocol.MatchEndedReason) buffer.ReadInt8();
        WinningTeam = buffer.ReadInt8();
        PlayerCount = buffer.ReadInt8();
        AccountIDs = new int[PlayerCount];

        for (int i = 0; i < PlayerCount; i++)
        {
            AccountIDs[i] = buffer.ReadInt32();
        }
    }

    public byte[] CommandBytes { get; init; }

    public int MatchID { get; init; }

    public ChatProtocol.MatchEndedReason Reason { get; init; }

    public byte WinningTeam { get; init; }

    public byte PlayerCount { get; init; }

    public int[] AccountIDs { get; init; }
}