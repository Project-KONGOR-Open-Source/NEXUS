namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

public class JoinedMatchRequestData
{
    public JoinedMatchRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        MatchName = buffer.ReadString();
        MatchID = buffer.ReadInt32();

        // Don't Add The Player To The Match Chat Channel If They Have Joined As A Spectator Or A Mentor
        JoinMatchChannel = buffer.ReadInt8() is not 0;
    }

    public byte[] CommandBytes { get; init; }

    public string MatchName { get; init; }

    public int MatchID { get; }

    public bool JoinMatchChannel { get; init; }
}
