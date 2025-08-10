namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN)]
public class GroupJoin(ILogger<GroupJoin> logger) : CommandProcessorsBase, ICommandProcessor
{
    private ILogger<GroupJoin> Logger { get; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        GroupJoinRequestData requestData = new(buffer);

        // Minimal placeholder implementation: accept join without validation and send a basic group update.
        // TODO: Replace hard-coded placeholder data with real group lookup, invite validation, ban checks, etc.

        Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE);

        ChatProtocol.TMMUpdateType updateType = ChatProtocol.TMMUpdateType.TMM_PLAYER_JOINED_GROUP; // Placeholder update type
        Response.WriteInt8(Convert.ToByte(updateType));                  // Group Update Type
        Response.WriteInt32(session.ClientInformation.Account.ID);       // Account ID (placeholder: joining player)
        Response.WriteInt8(1);                                           // Group Size (placeholder: solo group after join)
        Response.WriteInt16(1500);                                       // Average Group Rating (placeholder)
        Response.WriteInt32(session.ClientInformation.Account.ID);       // Leader Account ID (placeholder: self as leader)
        Response.WriteInt8(Convert.ToByte(ChatProtocol.ArrangedMatchType.AM_MATCHMAKING)); // Arranged Match Type (placeholder)
        Response.WriteInt8(Convert.ToByte(ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL)); // Game Type (placeholder)
        Response.WriteString("caldavar");                                // Map Name (placeholder)
        Response.WriteString("ap|ar|sd");                                // Game Modes (placeholder)
        Response.WriteString("EU|USE|USW");                              // Regions (placeholder)
        Response.WriteBool(true);                                        // Ranked (placeholder)
        Response.WriteBool(true);                                        // Match Fidelity (placeholder)
        Response.WriteInt8(1);                                           // Bot Difficulty (placeholder)
        Response.WriteBool(false);                                       // Randomize Bots (placeholder)
        Response.WriteString(string.Empty);                              // Country Restrictions (placeholder)
        Response.WriteString(string.Empty);                              // Player Invitation Responses (placeholder)
        Response.WriteInt8(5);                                           // Team Size (placeholder)
        Response.WriteInt8(Convert.ToByte(ChatProtocol.TMMType.TMM_TYPE_CAMPAIGN)); // Group Type (placeholder)

        // Placeholder single member data (joining player)
        Response.WriteInt32(session.ClientInformation.Account.ID);       // Member Account ID
        Response.WriteString(session.ClientInformation.Account.Name);    // Member Account Name
        Response.WriteInt8(1);                                           // Member Slot (placeholder)
        Response.WriteInt16(1500);                                       // Member Rating (placeholder)
        Response.WriteInt8(0);                                           // Loading Percent (placeholder)
        Response.WriteBool(false);                                       // Ready Status (placeholder)
        Response.WriteBool(false);                                       // In-Game Status (placeholder)
        Response.WriteBool(true);                                        // Eligible For Matchmaking (placeholder)
        Response.WriteString(session.ClientInformation.Account.ChatNameColour); // Chat Name Colour
        Response.WriteString(session.ClientInformation.Account.Icon);    // Account Icon
        Response.WriteString("NEWERTH");                                 // Country (placeholder)
        Response.WriteBool(true);                                        // Has Game Mode Access (placeholder)
        Response.WriteString("true|true|true");                          // Game Mode Access (placeholder)

        // Friend flags for each member (only one member)
        Response.WriteBool(false);                                       // Is Friend (placeholder)

        Response.PrependBufferSize();
        session.SendAsync(Response.Data);
    }
}

public class GroupJoinRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public string ClientVersion = buffer.ReadString();           // Placeholder: Version string read but unused
    public string InitiatorAccountName = buffer.ReadString();    // Placeholder: Intended group leader name read but unused
}
