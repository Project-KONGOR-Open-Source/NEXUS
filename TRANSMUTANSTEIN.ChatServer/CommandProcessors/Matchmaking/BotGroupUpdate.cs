namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

/// <summary>
///     Handles a bot roster change made by the group leader in a bot-match group, assigning a bot to an ally or enemy team slot.
///     The change is recorded on the group and broadcast to all members, so that every member's bot roster reflects the leader's selection.
/// </summary>
[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_BOT_GROUP_UPDATE)]
public class BotGroupUpdate : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        BotGroupUpdateRequestData requestData = new (buffer);

        MatchmakingGroup? group = MatchmakingService.GetMatchmakingGroup(session.Account.ID);

        if (group is null)
        {
            Log.Error("[BUG] No Matchmaking Group Found For Account ID {AccountID}", session.Account.ID);

            return;
        }

        // Only The Group Leader May Change The Group's Bot Roster
        if (group.Leader.Account.ID != session.Account.ID)
        {
            Log.Error("[BUG] Account ID {AccountID} Attempted To Change The Bot Roster For Group {GroupGUID} But Is Not The Leader", session.Account.ID, group.GUID);

            return;
        }

        group.UpdateBot(requestData.Team, requestData.Slot, requestData.BotName);
    }
}

file class BotGroupUpdateRequestData
{
    public byte[] CommandBytes { get; init; }

    /// <summary>
    ///     The team the bot slot belongs to: 1 for the ally team, 2 for the enemy team.
    /// </summary>
    public byte Team { get; init; }

    /// <summary>
    ///     The team slot index (0 to 4).
    /// </summary>
    public byte Slot { get; init; }

    /// <summary>
    ///     The bot definition name (for example "ChronosBot").
    /// </summary>
    public string BotName { get; init; }

    public BotGroupUpdateRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        Team = buffer.ReadInt8();
        Slot = buffer.ReadInt8();
        BotName = buffer.ReadString();
    }
}
