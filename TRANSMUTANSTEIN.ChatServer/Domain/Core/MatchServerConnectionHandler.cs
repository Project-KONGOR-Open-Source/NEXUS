namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

/// <summary>
///     Accepts match server connections and drives a <see cref="MatchServerChatSession"/> for the lifetime of each one.
/// </summary>
public sealed class MatchServerConnectionHandler(IServiceProvider serviceProvider) : ConnectionHandler
{
    public override Task OnConnectedAsync(ConnectionContext connection)
        => new MatchServerChatSession(connection, serviceProvider).RunAsync();
}
