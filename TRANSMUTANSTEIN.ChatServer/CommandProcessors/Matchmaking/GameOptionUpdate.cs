namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

/// <summary>
///     Handles match-option changes made by the group leader (map, game modes, regions, ranked status, match fidelity, and bot-match options).
///     The updated options are applied to the group and broadcast to all members, so that every member's matchmaking panel reflects the leader's selection.
/// </summary>
[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GAME_OPTION_UPDATE)]
public class GameOptionUpdate : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        GameOptionUpdateRequestData requestData = new (buffer);

        MatchmakingGroup? group = MatchmakingService.GetMatchmakingGroup(session.Account.ID);

        if (group is null)
        {
            Log.Error("[BUG] No Matchmaking Group Found For Account ID {AccountID}", session.Account.ID);

            return;
        }

        // Only The Group Leader May Change The Group's Match Options
        if (group.Leader.Account.ID != session.Account.ID)
        {
            Log.Error("[BUG] Account ID {AccountID} Attempted To Change Match Options For Group {GroupGUID} But Is Not The Leader", session.Account.ID, group.GUID);

            return;
        }

        group.UpdateGameOptions(requestData.GameType, requestData.MapName, requestData.GameModes, requestData.GameRegions, requestData.Ranked, requestData.MatchFidelity, requestData.BotDifficulty, requestData.RandomizeBots);
    }
}

file class GameOptionUpdateRequestData
{
    public byte[] CommandBytes { get; init; }

    public ChatProtocol.TMMGameType GameType { get; init; }

    public string MapName { get; init; }

    public string[] GameModes { get; init; }

    public string[] GameRegions { get; init; }

    public bool Ranked { get; init; }

    public byte MatchFidelity { get; init; }

    public byte BotDifficulty { get; init; }

    public bool RandomizeBots { get; init; }

    public GameOptionUpdateRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        GameType = (ChatProtocol.TMMGameType) buffer.ReadInt8();
        MapName = buffer.ReadString();
        GameModes = buffer.ReadString().Split('|', StringSplitOptions.RemoveEmptyEntries);
        GameRegions = buffer.ReadString().Split('|', StringSplitOptions.RemoveEmptyEntries);
        Ranked = buffer.ReadBool();
        MatchFidelity = buffer.ReadInt8();
        BotDifficulty = buffer.ReadInt8();
        RandomizeBots = buffer.ReadBool();
    }
}
