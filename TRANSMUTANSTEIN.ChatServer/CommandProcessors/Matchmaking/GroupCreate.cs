namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_CREATE)]
public class GroupCreate(MerrickContext merrick, ILogger<GroupCreate> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<GroupCreate> Logger { get; set; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        GroupCreateRequestData requestData = new(buffer);

        // Check if player is already in a group
        if (Context.MatchmakingGroupChatChannels.ContainsKey(session.ClientInformation.Account.ID))
        {
            Logger.LogWarning($"Account {session.ClientInformation.Account.ID} ({session.ClientInformation.Account.Name}) attempted to create a group while already in one");
              // Send failure response
            Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_FAILED_TO_JOIN);
            Response.WriteInt8((byte)ChatProtocol.TMMFailedToJoinReason.TMMFTJR_GROUP_FULL);
            Response.PrependBufferSize();
            
            session.SendAsync(Response.Data);
            return;
        }

        // TODO: Add additional validation (e.g., banned players, maintenance mode, etc.)

        try
        {
            // Create the matchmaking group
            MatchmakingGroup group = new(
                requestData.GroupType,
                requestData.GameType,
                string.Join("|", requestData.GameModes),
                string.Join("|", requestData.GameRegions),
                requestData.Ranked,
                requestData.MatchFidelity,
                requestData.BotDifficulty,
                requestData.RandomizeBots
            );

            // Add the group to the appropriate collection based on size
            var groupDict = GetGroupDictionary(requestData.GroupType);
            groupDict.TryAdd(session.ClientInformation.Account.ID, group);

            // Add the player to the group
            if (!group.AddParticipant(session, out var failureReason))
            {
                // Remove the group if we couldn't add the participant
                groupDict.TryRemove(session.ClientInformation.Account.ID, out _);
                
                Logger.LogWarning($"Failed to add account {session.ClientInformation.Account.ID} to their own group: {failureReason}");
                  // Send failure response
                Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_FAILED_TO_JOIN);
                Response.WriteInt8((byte)failureReason);
                Response.PrependBufferSize();
                
                session.SendAsync(Response.Data);
                return;
            }

            Logger.LogInformation($"Created matchmaking group for account {session.ClientInformation.Account.ID} ({session.ClientInformation.Account.Name})");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Failed to create matchmaking group for account {session.ClientInformation.Account.ID}");
              // Send failure response
            Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_FAILED_TO_JOIN);
            Response.WriteInt8((byte)ChatProtocol.TMMFailedToJoinReason.TMMFTJR_DISABLED);
            Response.PrependBufferSize();
            
            session.SendAsync(Response.Data);
        }
    }    private static ConcurrentDictionary<int, MatchmakingGroup> GetGroupDictionary(ChatProtocol.TMMType groupType)
    {
        return groupType switch
        {
            ChatProtocol.TMMType.TMM_TYPE_SOLO => MatchmakingService.SoloPlayerGroups,
            ChatProtocol.TMMType.TMM_TYPE_PVP => MatchmakingService.TwoPlayerGroups,
            ChatProtocol.TMMType.TMM_TYPE_COOP => MatchmakingService.ThreePlayerGroups,
            ChatProtocol.TMMType.TMM_TYPE_CAMPAIGN => MatchmakingService.FourPlayerGroups,
            _ => MatchmakingService.SoloPlayerGroups
        };
    }
}

public class GroupCreateRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public string ClientVersion = buffer.ReadString();
    public ChatProtocol.TMMType GroupType = (ChatProtocol.TMMType)buffer.ReadInt8();
    public ChatProtocol.TMMGameType GameType = (ChatProtocol.TMMGameType)buffer.ReadInt8();
    public string MapName = buffer.ReadString();
    public string[] GameModes = buffer.ReadString().Split('|', StringSplitOptions.RemoveEmptyEntries);
    public string[] GameRegions = buffer.ReadString().Split('|', StringSplitOptions.RemoveEmptyEntries);
    public bool Ranked = buffer.ReadBool();

    // If TRUE, Skill Disparity Will Be Lower But The Matchmaking Queue Time Will Be Longer
    public bool MatchFidelity = buffer.ReadBool();

    // 1: Easy, 2: Medium, 3: Hard
    // Only Used For Bot Matches, But Sent With Every Request To Create A Group
    public byte BotDifficulty = buffer.ReadInt8();

    // Only Used For Bot Matches, But Sent With Every Request To Create A Group
    public bool RandomizeBots = buffer.ReadBool();
}
