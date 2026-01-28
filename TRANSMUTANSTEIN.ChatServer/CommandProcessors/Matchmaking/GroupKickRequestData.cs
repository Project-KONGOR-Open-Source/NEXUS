namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

public class GroupKickRequestData
{
    public GroupKickRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        SlotNumber = buffer.ReadByte();
    }

    public byte[] CommandBytes { get; init; }
    public byte SlotNumber { get; }
}
