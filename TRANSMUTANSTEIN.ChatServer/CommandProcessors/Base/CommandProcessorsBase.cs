namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Base;

public abstract class CommandProcessorsBase
{
    protected ChatBuffer Response { get; set; } = new ();
}
