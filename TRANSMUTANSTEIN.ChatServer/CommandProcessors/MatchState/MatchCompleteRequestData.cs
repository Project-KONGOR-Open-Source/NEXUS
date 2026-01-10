namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

public class MatchCompleteRequestData
{
    public MatchCompleteRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();

        // Safely partial read
        if (buffer.HasRemainingData())
        {
            ServerID = buffer.ReadInt32();
        }

        if (buffer.HasRemainingData())
        {
            MatchID = buffer.ReadInt32();
        }

        if (buffer.HasRemainingData())
        {
            WinningTeam = buffer.ReadInt32();
        }

        if (buffer.HasRemainingData())
        {
            MatchDuration = buffer.ReadInt32();
        }
    }

    public byte[] CommandBytes { get; init; }

    public int ServerID { get; init; }

    public int MatchID { get; init; }

    public int WinningTeam { get; init; }

    public int MatchDuration { get; init; }
}
