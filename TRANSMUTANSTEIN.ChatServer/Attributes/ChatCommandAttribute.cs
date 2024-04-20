namespace TRANSMUTANSTEIN.ChatServer.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ChatCommandAttribute(ushort command) : Attribute
{
    public ushort Command { get; init; } = command;
}
