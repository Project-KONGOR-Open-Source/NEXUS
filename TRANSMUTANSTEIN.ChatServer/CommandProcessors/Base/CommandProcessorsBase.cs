namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Base;

public abstract class CommandProcessorsBase
{
    protected byte[] ResponseCommand { get; set; } = new byte[2];

    protected ChatBuffer Response { get; set; } = new();

    protected byte[] ResponseSize => BitConverter.GetBytes(Convert.ToInt16(Response.Data.Length));

    protected MerrickContext MerrickContext { get; set; } = TRANSMUTANSTEIN.ServiceProvider.GetRequiredService<MerrickContext>();

    protected ILogger Logger { get; set; } = TRANSMUTANSTEIN.ServiceProvider.GetRequiredService<ILogger<CommandProcessorsBase>>();
}
