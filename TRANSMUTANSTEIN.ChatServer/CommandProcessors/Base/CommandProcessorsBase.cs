namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Base;

public abstract class CommandProcessorsBase
{
    protected MerrickContext MerrickContext { get; set; } = TRANSMUTANSTEIN.ServiceProvider.GetRequiredService<MerrickContext>();

    protected ILogger Logger { get; set; } = TRANSMUTANSTEIN.ServiceProvider.GetRequiredService<ILogger<CommandProcessorsBase>>();
}
