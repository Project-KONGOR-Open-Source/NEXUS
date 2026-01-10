namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Actions;

public class TrackPlayerActionRequestData
{
    public TrackPlayerActionRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        Action = (ChatProtocol.ActionCampaign) buffer.ReadInt8();
    }

    public byte[] CommandBytes { get; init; }

    public ChatProtocol.ActionCampaign Action { get; }
}
