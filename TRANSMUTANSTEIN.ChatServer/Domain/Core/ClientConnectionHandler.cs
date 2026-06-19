namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

/// <summary>
///     Accepts game client connections and drives a <see cref="ClientChatSession"/> for the lifetime of each one.
/// </summary>
public sealed class ClientConnectionHandler(IServiceProvider serviceProvider) : ConnectionHandler
{
    public override Task OnConnectedAsync(ConnectionContext connection)
        => new ClientChatSession(connection, serviceProvider).RunAsync();
}
