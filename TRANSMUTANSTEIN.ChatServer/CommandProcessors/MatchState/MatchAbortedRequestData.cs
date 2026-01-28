namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

public class MatchAbortedRequestData
{
    public MatchAbortedRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        MatchupID = buffer.ReadInt32();
        Reason = buffer.ReadByte();
    }

    public byte[] CommandBytes { get; init; }
    public int MatchupID { get; }
    public byte Reason { get; }
}
