namespace TRANSMUTANSTEIN.ChatServer.Internals;

public static class Context
{
    public static ConcurrentDictionary<string, ChatSession> ChatSessions { get; set; } = [];

    public static ConcurrentDictionary<string, ChatChannel> ChatChannels { get; set; } = [];
}
