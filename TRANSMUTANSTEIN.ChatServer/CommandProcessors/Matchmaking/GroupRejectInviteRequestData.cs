namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

public class GroupRejectInviteRequestData
{
    public GroupRejectInviteRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        InviterName = buffer.ReadString();
    }

    public byte[] CommandBytes { get; init; }

    public string InviterName { get; }
}
