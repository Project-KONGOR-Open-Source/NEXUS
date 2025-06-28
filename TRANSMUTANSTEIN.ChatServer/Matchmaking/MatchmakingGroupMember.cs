namespace TRANSMUTANSTEIN.ChatServer.Matchmaking;

/* TODO: Remove This (Temporary Definition From Project KONGOR Legacy)
    public record GroupParticipant(
        int AccountId,
        string Name,
        byte Slot,
        int NormalRankLevel,
        int CasualRankLevel,
        int NormalRanking,
        int CasualRanking,
        byte EligibleForCampaign,
        short Rating,
        byte LoadingPercent,
        byte ReadyStatus,
        byte InGame,
        byte Verified,
        string ChatNameColor,
        string AccountIcon, // Icon:Slot format.
        string Country,
        byte GameModeAccessBool,
        string GameModeAccessString
    );
*/

/// <summary>
///     Defined In "c_chatmanager.h" As "SGroupMemberInfo"
/// </summary>
public class MatchmakingGroupMember
{
    public required int ID { get; set; }

    public required string Name { get; set; }

    public required byte Slot { get; set; }

    public required bool IsLeader { get; set; }

    public required bool IsReady { get; set; }

    public required byte LoadingPercent { get; set; }
}

/*
    struct SGroupMemberInfo
    {
        uint        uiAccountID;
        tstring     sName;
        uint        uiChatNameColor;
        uint        uiAccountIcon;
        int         iAccountIconSlot;
        tstring     sCountry;
        byte        ySlot;
        ushort      unRating;
        byte        yLoadingPercent;
        byte        yReadyStatus;
        bool        bInGame;
        bool        bEligibleForRanked;
        bool        bFriend;
        bool        bGameModeAccess;
        wstring     sGameModeAccess;
        uint        uiCampaignMedalNormal;
        uint        uiCampaignMedalCasual;
        int         iCampaignRankingNormal;
        int         iCampaignrankingCasual;
        bool        bEligibleForCampaign;

        SGroupMemberInfo()
        {
            Clear();
        }

        void Clear()
        {
            uiAccountID = INVALID_INDEX;
            sName.clear();
            uiChatNameColor = INVALID_INDEX;
            uiAccountIcon = INVALID_INDEX;
            iAccountIconSlot = 0;
            sCountry.clear();
            ySlot = 0xff;
            unRating = 0;
            yLoadingPercent = 0;
            yReadyStatus = 0;
            bInGame = false;
            bEligibleForRanked = false;
            bFriend = false;
            bGameModeAccess = false;
            uiCampaignMedalNormal = RANK_LEVEL_UNKNOWN;
            uiCampaignMedalCasual = RANK_LEVEL_UNKNOWN;
            iCampaignRankingNormal = -1;
            iCampaignRankingCasual = -1;
            bEligibleForCampaign = false;
        }
    };
*/
