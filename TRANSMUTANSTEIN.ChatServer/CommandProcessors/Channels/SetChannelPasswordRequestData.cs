namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

public class SetChannelPasswordRequestData
{
    public SetChannelPasswordRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelID = buffer.ReadInt32();
        Password = buffer.ReadString();
    }

    public byte[] CommandBytes { get; init; }

    public int ChannelID { get; }

    public string Password { get; }
}
