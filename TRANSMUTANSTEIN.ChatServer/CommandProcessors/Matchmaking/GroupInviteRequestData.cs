namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

public class GroupInviteRequestData
{
    public GroupInviteRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        InviteReceiverName = buffer.ReadString();
    }

    public byte[] CommandBytes { get; init; }

    public string InviteReceiverName { get; }
}
