namespace KINESIS.Client;

[Flags]
public enum ChatChannelFlags
{
    Permanent = 1 << 0,      // Doesn't get destroyed when the last user leaves the channel.
    Server = 1 << 1,         // Channel for post-match chat.
    Hidden = 1 << 2,         // Usage unclear.
    Reserved = 1 << 3,       // System created channels (e.g. general, clan, stream, etc.)
    GeneralUse = 1 << 4,     // General purpose chats (e.g. KONGOR 1).
    CannotBeJoined = 1 << 5, // Usage unclear.
    AuthRequired = 1 << 6,   // Usage unclear.
    Clan = 1 << 7,
    StreamUse = 1 << 8,      // Usage unclear.
}
