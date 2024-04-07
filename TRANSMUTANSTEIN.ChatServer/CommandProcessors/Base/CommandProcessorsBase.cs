namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Base;

public abstract class CommandProcessorsBase<T>
{
    protected byte[] ResponseCommand { get; set; } = new byte[2];

    protected ChatBuffer Response { get; set; } = new();

    protected byte[] ResponseSize => BitConverter.GetBytes(Convert.ToInt16(Response.Data.Length));
}
