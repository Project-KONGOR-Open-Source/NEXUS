using System.Collections.Concurrent;
using TRANSMUTANSTEIN.ChatServer.Domain.Core;
using TRANSMUTANSTEIN.ChatServer.Domain.Social;

namespace TRANSMUTANSTEIN.ChatServer.Internals;

/// <summary>
/// Defines the shared state context for the Chat Server (Sessions and Channels).
/// Scoped as Singleton in Production, but isolated per-host in Tests.
/// </summary>
public interface IChatContext
{
    ConcurrentDictionary<string, ChatSession> ClientChatSessions { get; }

    ConcurrentDictionary<int, ChatSession> MatchServerChatSessions { get; }

    ConcurrentDictionary<int, ChatSession> MatchServerManagerChatSessions { get; }

    ConcurrentDictionary<string, ChatChannel> ChatChannels { get; }

    Services.IMatchmakingService Matchmaking { get; }
}
