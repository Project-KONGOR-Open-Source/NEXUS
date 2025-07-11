﻿namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_CREATE)]
public class GroupCreate(MerrickContext merrick, ILogger<GroupCreate> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<GroupCreate> Logger { get; set; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        GroupCreateRequestData requestData = new (buffer);

        if (Context.MatchmakingGroupChatChannels.ContainsKey(session.ClientInformation.Account.ID) is false)
        {
            MatchmakingGroupMember member = new (session)
            {
                Slot = 1,
                IsLeader = true,
                IsReady = false,
                IsInGame = false,
                IsEligibleForMatchmaking = true,
                LoadingPercent = 0,
                GameModeAccess = string.Join('|', requestData.GameModes.Select(mode => "true"))
            };

            if (MatchmakingService.Groups.ContainsKey(session.ClientInformation.Account.ID) is false)
            {
                if (MatchmakingService.Groups.TryAdd(session.ClientInformation.Account.ID, new MatchmakingGroup(member)) is false)
                {
                    Logger.LogError(@"Failed To Create Matchmaking Group For Account ID ""{Session.ClientInformation.Account.ID}""", session.ClientInformation.Account.ID);

                    return; // TODO: Respond With ChatProtocol.TMMFailedToJoinReason Or Similar (e.g. TMMFailedToCreate, If It Exists)
                }
            }

            else
            {
                if (MatchmakingService.Groups.TryUpdate(session.ClientInformation.Account.ID, new MatchmakingGroup(member), MatchmakingService.Groups[session.ClientInformation.Account.ID]) is false)
                {
                    Logger.LogError(@"Failed To Update Matchmaking Group For Account ID ""{Session.ClientInformation.Account.ID}""", session.ClientInformation.Account.ID);

                    return; // TODO: Respond With ChatProtocol.TMMFailedToJoinReason Or Similar (e.g. TMMFailedToCreate, If It Exists)
                }
            }
        }

        Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE);

        ChatProtocol.TMMUpdateType updateType = ChatProtocol.TMMUpdateType.TMM_CREATE_GROUP;

        Response.WriteInt8(Convert.ToByte(updateType));                                     // Group Update Type
        Response.WriteInt32(session.ClientInformation.Account.ID);                          // Account ID
        Response.WriteInt8(1);                                                              // Group Size
        // TODO: Calculate Average Group Rating
        Response.WriteInt16(1500);                                                          // Average Group Rating
        Response.WriteInt32(session.ClientInformation.Account.ID);                          // Leader Account ID
        // TODO: Dynamically Set Arranged Match Type From The Request Data
        Response.WriteInt8(Convert.ToByte(ChatProtocol.ArrangedMatchType.AM_MATCHMAKING));  // Arranged Match Type
        Response.WriteInt8(Convert.ToByte(requestData.GameType));                           // Game Type
        Response.WriteString(requestData.MapName);                                          // Map Name
        Response.WriteString(string.Join('|', requestData.GameModes));                      // Game Modes
        Response.WriteString(string.Join('|', requestData.GameRegions));                    // Regions
        Response.WriteBool(requestData.Ranked);                                             // Ranked
        Response.WriteBool(requestData.MatchFidelity);                                      // Match Fidelity
        Response.WriteInt8(requestData.BotDifficulty);                                      // Bot Difficulty
        Response.WriteBool(requestData.RandomizeBots);                                      // Randomize Bots
        Response.WriteString(string.Empty);                                                 // Country Restrictions (e.g. "AB->USE|XY->USW" Means Only Country "AB" Can Access Region "USE" And Only Country "XY" Can Access Region "USW")
        Response.WriteString("TODO: Find Out What Player Invitation Responses Do");         // Player Invitation Responses
        // TODO: Dynamically Set Team Size From The Request Data
        Response.WriteInt8(5);                                                              // Team Size (e.g. 5 For Forests Of Caldavar, 3 For Grimm's Crossing)
        Response.WriteInt8(Convert.ToByte(requestData.GroupType));                          // Group Type

        MatchmakingGroup group = MatchmakingService.Groups.Single(group => group.Key == session.ClientInformation.Account.ID).Value;

        foreach (MatchmakingGroupMember member in group.Members)
        {
            if (updateType is not ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE)
            {
                Response.WriteInt32(member.Account.ID);                                     // Account ID
                Response.WriteString(member.Account.Name);                                  // Account Name
                Response.WriteInt8(member.Slot);                                            // Group Slot
                // TODO: Determine Rating From Game Type
                Response.WriteInt16(1500);                                                  // Normal Rank Level
            }

            Response.WriteInt8(member.LoadingPercent);                                      // Loading Percent (0 to 100)
            Response.WriteBool(member.IsReady);                                             // Ready Status
            Response.WriteBool(member.IsInGame);                                            // In-Game Status

            if (updateType is not ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE)
            {
                Response.WriteBool(member.IsEligibleForMatchmaking);                        // Eligible For Matchmaking
                Response.WriteString(member.Account.ChatNameColour);                        // Chat Name Colour
                Response.WriteString(member.Account.Icon);                                  // Account Icon
                Response.WriteString(member.Country);                                       // Country
                Response.WriteBool(member.HasGameModeAccess);                               // Game Mode Access Bool
                Response.WriteString(member.GameModeAccess);                                // Game Mode Access String
            }
        }

        if (updateType is not ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE)
        {
            foreach (MatchmakingGroupMember member in group.Members)
            {
                Response.WriteBool(false);                                                  // Is Friend
            }
        }

        Response.PrependBufferSize();

        // Create A New Matchmaking Group With The Initiator As The Group Leader
        session.SendAsync(Response.Data);
    }
}

public class GroupCreateRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public string ClientVersion = buffer.ReadString();
    public ChatProtocol.TMMType GroupType = (ChatProtocol.TMMType) buffer.ReadInt8();
    public ChatProtocol.TMMGameType GameType = (ChatProtocol.TMMGameType) buffer.ReadInt8();
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
