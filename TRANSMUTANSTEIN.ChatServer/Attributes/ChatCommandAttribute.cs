namespace TRANSMUTANSTEIN.ChatServer.Attributes;

public class ChatCommandAttribute: Attribute
{
    public ChatCommandAttribute(byte[] command)
    {
        if (command.Length is not 2)
            throw new ArgumentException("Chat Command Is Expected To Be 2 Bytes In Length");

        Command = command;
    }

    public byte[] Command { get; init; }
}
