namespace TRANSMUTANSTEIN.ChatServer.Internals;

public static class Context
{
    public static ConcurrentDictionary<string, ClientChatSession> ClientChatSessions { get; set; } = [];

    public static ConcurrentDictionary<int, MatchServerChatSession> MatchServerChatSessions { get; set; } = [];

    public static ConcurrentDictionary<int, MatchServerManagerChatSession> MatchServerManagerChatSessions { get; set; } = [];

    public static ConcurrentDictionary<string, ChatChannel> ChatChannels { get; set; } = [];
}
