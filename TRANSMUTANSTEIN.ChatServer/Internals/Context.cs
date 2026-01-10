namespace TRANSMUTANSTEIN.ChatServer.Internals;

public static class Context
{
    public static ConcurrentDictionary<string, ChatSession> ClientChatSessions { get; set; } = [];

    public static ConcurrentDictionary<int, ChatSession> MatchServerChatSessions { get; set; } = [];

    public static ConcurrentDictionary<int, ChatSession> MatchServerManagerChatSessions { get; set; } = [];

    public static ConcurrentDictionary<string, ChatChannel> ChatChannels { get; set; } = [];
}

