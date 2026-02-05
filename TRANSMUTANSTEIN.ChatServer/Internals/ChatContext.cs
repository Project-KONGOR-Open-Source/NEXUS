using TRANSMUTANSTEIN.ChatServer.Services;

namespace TRANSMUTANSTEIN.ChatServer.Internals;

public class ChatContext : IChatContext
{
    public ConcurrentDictionary<string, ChatSession> ClientChatSessions { get; } = [];

    public ConcurrentDictionary<int, ChatSession> MatchServerChatSessions { get; } = [];

    public ConcurrentDictionary<int, ChatSession> MatchServerManagerChatSessions { get; } = [];

    public ConcurrentDictionary<string, ChatChannel> ChatChannels { get; } = [];
}
