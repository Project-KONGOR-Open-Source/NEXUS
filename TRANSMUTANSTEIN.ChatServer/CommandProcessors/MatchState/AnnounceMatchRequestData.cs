namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

public class AnnounceMatchRequestData
{
    public AnnounceMatchRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        MatchupID = buffer.ReadInt32();
        Challenge = buffer.ReadInt32();

        int groupCount = buffer.ReadInt32();
        GroupIDs = [];
        for (int i = 0; i < groupCount; i++)
        {
            GroupIDs.Add(buffer.ReadInt32());
        }

        MatchID = buffer.ReadInt32();
    }

    public byte[] CommandBytes { get; init; }
    public int MatchupID { get; }
    public int Challenge { get; }
    public List<int> GroupIDs { get; }
    public int MatchID { get; }
}
