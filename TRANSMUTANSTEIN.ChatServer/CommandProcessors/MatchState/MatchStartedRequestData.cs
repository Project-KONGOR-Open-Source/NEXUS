namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

public class MatchStartedRequestData
{
    public MatchStartedRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        MatchupID = buffer.ReadInt32();
    }

    public byte[] CommandBytes { get; init; }
    public int MatchupID { get; }
}
