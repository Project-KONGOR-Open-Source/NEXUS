namespace TRANSMUTANSTEIN.ChatServer.Communication;

public class ChatChannel
{
    public int ID => Name.GetDeterministicInt32Hash();

    public required string Name { get; set; }

    public required ChatProtocol.ChatChannelType Flags { get; set; }

    public ConcurrentDictionary<string, ChatChannelMember> Administrators { get; set; } = [];

    public ConcurrentDictionary<string, ChatChannelMember> Members { get; set; } = [];
}
