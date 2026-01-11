namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

public class GroupJoinRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public string ClientVersion { get; init; } = buffer.ReadString();

    public string InviteIssuerName { get; } = buffer.ReadString();
}