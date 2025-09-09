namespace TRANSMUTANSTEIN.ChatServer.Matchmaking;

public class MatchmakingGroup(MatchmakingGroupMember leader)
{
    public MatchmakingGroupMember Leader => Members.Single(member => member.IsLeader);

    public List<MatchmakingGroupMember> Members { get; set; } = [ leader ];

    public required MatchmakingGroupInformation Information { get; set; }

    // public required ChatChannel ChatChannel { get; set; }

    public float AverageRating => 1500; // TODO: Members.Average(member => member.Rating);

    public float RatingDisparity => 100; // TODO: Members.Max(member => member.Rating) - Members.Min(member => member.Rating);

    public int FullTeamDifference => Information.TeamSize - Members.Count;

    public DateTimeOffset? QueueStartTime { get; set; } = null;

    public TimeSpan QueueDuration => QueueStartTime is not null ? DateTimeOffset.UtcNow - QueueStartTime.Value : TimeSpan.Zero;

    public void MulticastUpdate(int emitterAccountID, ChatProtocol.TMMUpdateType updateType)
    {
        ChatBuffer update = new ();

        update.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE);

        update.WriteInt8(Convert.ToByte(updateType));                                    // Group Update Type
        update.WriteInt32(emitterAccountID);                                             // Account ID
        update.WriteInt8(Convert.ToByte(Members.Count));                                 // Group Size
        // TODO: Calculate Average Group Rating
        update.WriteInt16(1500);                                                         // Average Group Rating
        update.WriteInt32(Leader.Account.ID);                                            // Leader Account ID
        // TODO: Dynamically Set Arranged Match Type From The Request Data
        update.WriteInt8(Convert.ToByte(ChatProtocol.ArrangedMatchType.AM_MATCHMAKING)); // Arranged Match Type
        update.WriteInt8(Convert.ToByte(Information.GameType));                          // Game Type
        update.WriteString(Information.MapName);                                         // Map Name
        update.WriteString(string.Join('|', Information.GameModes));                     // Game Modes
        update.WriteString(string.Join('|', Information.GameRegions));                   // Game Regions
        update.WriteBool(Information.Ranked);                                            // Ranked
        update.WriteInt8(Information.MatchFidelity);                                     // Match Fidelity
        update.WriteInt8(Information.BotDifficulty);                                     // Bot Difficulty
        update.WriteBool(Information.RandomizeBots);                                     // Randomize Bots
        update.WriteString(string.Empty);                                                // Country Restrictions (e.g. "AB->USE|XY->USW" Means Only Country "AB" Can Access Region "USE" And Only Country "XY" Can Access Region "USW")
        // TODO: Find Out What Player Invitation Responses Do
        update.WriteString("What Is This ??? (Player Invitation Responses)");            // Player Invitation Responses
        update.WriteInt8(Information.TeamSize);                                          // Team Size (e.g. 5 For Forests Of Caldavar, 3 For Grimm's Crossing)
        update.WriteInt8(Convert.ToByte(Information.GroupType));                         // Group Type

        bool fullGroupUpdate = updateType switch
        {
            ChatProtocol.TMMUpdateType.TMM_CREATE_GROUP             => true,
            ChatProtocol.TMMUpdateType.TMM_FULL_GROUP_UPDATE        => true,
            ChatProtocol.TMMUpdateType.TMM_PLAYER_JOINED_GROUP      => true,
            ChatProtocol.TMMUpdateType.TMM_PLAYER_LEFT_GROUP        => true,
            ChatProtocol.TMMUpdateType.TMM_PLAYER_KICKED_FROM_GROUP => true,
            _                                                       => false
        };

        foreach (MatchmakingGroupMember member in Members)
        {
            if (fullGroupUpdate)
            {
                update.WriteInt32(member.Account.ID);                                    // Account ID
                update.WriteString(member.Account.Name);                                 // Account Name
                update.WriteInt8(member.Slot);                                           // Group Slot
                // TODO: Get Real Rank Level And Rating
                /* TODO: Establish Rank (Medal) Level From Rating And Add To The Database
                    enum ECampaignLevel
                    {
                        CAMPAIGN_LEVEL_NONE = 0,

                        CAMPAIGN_LEVEL_BRONZE_5,
                        CAMPAIGN_LEVEL_BRONZE_4,
                        CAMPAIGN_LEVEL_BRONZE_3,
                        CAMPAIGN_LEVEL_BRONZE_2,
                        CAMPAIGN_LEVEL_BRONZE_1,

                        CAMPAIGN_LEVEL_SILVER_5,
                        CAMPAIGN_LEVEL_SILVER_4,
                        CAMPAIGN_LEVEL_SILVER_3,
                        CAMPAIGN_LEVEL_SILVER_2,
                        CAMPAIGN_LEVEL_SILVER_1,

                        CAMPAIGN_LEVEL_GOLD_4,
                        CAMPAIGN_LEVEL_GOLD_3,
                        CAMPAIGN_LEVEL_GOLD_2,
                        CAMPAIGN_LEVEL_GOLD_1,

                        CAMPAIGN_LEVEL_DIAMOND_3,
                        CAMPAIGN_LEVEL_DIAMOND_2,
                        CAMPAIGN_LEVEL_DIAMOND_1,

                        CAMPAIGN_LEVEL_LEGENDARY2,
                        CAMPAIGN_LEVEL_LEGENDARY1,

                        CAMPAIGN_LEVEL_IMMORTAL
                    };
                */
                update.WriteInt32(20);                                                   // Normal Rank Level (Also Known As Normal Campaign Level Or Medal)
                update.WriteInt32(15);                                                   // Casual Rank Level (Also Known As Casual Campaign Level Or Medal)
                // TODO: Figure Out What These Ranks Are (Potentially Actual Global Ranking Index In Order Of Rating Descending, e.g. Highest Rating Is Rank 1)
                update.WriteInt32(20);                                                   // Normal Rank
                update.WriteInt32(15);                                                   // Casual Rank
                update.WriteBool(true);                                                  // Eligible For Campaign
                // TODO: Set Actual Rating, Dynamically From The Database
                // TODO: Can Be Set To -1 To Hide The Rating From Other Players For Unranked Game Modes
                update.WriteInt16(1850);                                                 // Rating
            }

            update.WriteInt8(member.LoadingPercent);                                     // Loading Percent (0 to 100)
            update.WriteBool(member.IsReady);                                            // Ready Status
            update.WriteBool(member.IsInGame);                                           // In-Game Status

            if (fullGroupUpdate)
            {
                update.WriteBool(member.IsEligibleForMatchmaking);                       // Eligible For Matchmaking
                update.WriteString(member.Account.ChatNameColour);                       // Chat Name Colour
                update.WriteString(member.Account.Icon);                                 // Account Icon
                update.WriteString(member.Country);                                      // Country
                update.WriteBool(member.HasGameModeAccess);                              // Game Mode Access Bool
                update.WriteString(member.GameModeAccess);                               // Game Mode Access String
            }
        }

        if (fullGroupUpdate)
        {
            foreach (MatchmakingGroupMember member in Members)
            {
                // TODO: Determine Friendship Status
                update.WriteBool(false);                                                 // Is Friend
            }
        }

        update.PrependBufferSize();

        Parallel.ForEach(Members, member => member.Session.SendAsync(update.Data));
    }
}
