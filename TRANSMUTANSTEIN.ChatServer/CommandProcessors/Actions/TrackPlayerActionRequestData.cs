namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Actions;

public class TrackPlayerActionRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public ChatProtocol.ActionCampaign Action { get; } = (ChatProtocol.ActionCampaign) buffer.ReadInt8();
}