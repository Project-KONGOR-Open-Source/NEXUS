namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

/// <summary>
///     Accepts match server manager connections and drives a <see cref="MatchServerManagerChatSession"/> for the lifetime of each one.
/// </summary>
public sealed class MatchServerManagerConnectionHandler(IServiceProvider serviceProvider) : ConnectionHandler
{
    public override Task OnConnectedAsync(ConnectionContext connection)
        => new MatchServerManagerChatSession(connection, serviceProvider).RunAsync();
}
