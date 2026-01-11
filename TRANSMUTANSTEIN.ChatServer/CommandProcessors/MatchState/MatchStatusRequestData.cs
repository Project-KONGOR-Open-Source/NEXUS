namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

public class MatchStatusRequestData
{
    public MatchStatusRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ServerId = buffer.ReadInt32();
        MatchId = buffer.ReadInt32();
        Phase = buffer.ReadInt32();
        CurrentGameTime = buffer.ReadInt32();
    }

    public byte[] CommandBytes { get; init; }

    public int ServerId { get; init; }

    public int MatchId { get; init; }

    public int Phase { get; init; }

    public int CurrentGameTime { get; init; }
}