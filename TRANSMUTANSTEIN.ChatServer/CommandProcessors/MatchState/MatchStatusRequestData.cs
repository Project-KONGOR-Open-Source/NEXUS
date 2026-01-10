namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

public class MatchStatusRequestData
{
    public MatchStatusRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ServerID = buffer.ReadInt32();
        MatchID = buffer.ReadInt32();
        Phase = buffer.ReadInt32();
        CurrentGameTime = buffer.ReadInt32();
    }

    public byte[] CommandBytes { get; init; }

    public int ServerID { get; init; }

    public int MatchID { get; init; }

    public int Phase { get; init; }

    public int CurrentGameTime { get; init; }
}
