namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

public class GroupJoinRequestData
{
    public GroupJoinRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ClientVersion = buffer.ReadString();
        InviteIssuerName = buffer.ReadString();
    }

    public byte[] CommandBytes { get; init; }

    public string ClientVersion { get; init; }

    public string InviteIssuerName { get; }
}
