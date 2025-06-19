namespace TRANSMUTANSTEIN.ChatServer.Matchmaking.Responses;

public record MatchmakingGroupUpdateResponse(
    byte UpdateType,
    int AccountId,
    byte GroupSize,
    short AverageTMR,
    int LeaderAccountId,
    byte Unknown1,
    ChatProtocol.TMMGameType GameType,
    string MapName,
    string GameModes,
    string Regions,
    bool Ranked,
    bool MatchFidelity,
    byte BotDifficulty,
    bool RandomizeBots,
    string Unknown2,
    string PlayerInvitationResponses,
    byte TeamSize,
    ChatProtocol.TMMType GroupType,
    List<MatchmakingGroupUpdateResponse.GroupParticipant> GroupParticipants,
    byte[] FriendshipStatus,
    bool IsLoadingResources
) : IMatchmakingResponse
{
    public record GroupParticipant(
        int AccountId,
        string Name,
        byte Slot,
        float NormalRankLevel,
        float CasualRankLevel,
        int NormalRanking,
        int CasualRanking,
        byte EligibleForCampaign,
        float Rating,
        byte LoadingPercent,
        byte ReadyStatus,
        byte InGame,
        byte Verified,
        string ChatNameColor,
        string AccountIcon,
        string Country,
        byte GameModeAccessBool,
        string GameModeAccessString
    );

    public byte[] Serialize()
    {
        ChatBuffer buffer = new();
        buffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE);
        
        // Write group participant data (up to 5 slots)
        for (int i = 0; i < 5; i++)
        {
            if (i < GroupParticipants.Count)
            {
                var participant = GroupParticipants[i];
                buffer.WriteInt32(participant.AccountId);
                buffer.WriteString(participant.Name);
                buffer.WriteInt8(participant.Slot);
                buffer.WriteInt32((int)participant.Rating);
                buffer.WriteInt8(participant.LoadingPercent);
                buffer.WriteInt8(participant.ReadyStatus);
                buffer.WriteInt8(participant.InGame);
                buffer.WriteInt8(participant.Verified);
                buffer.WriteString(participant.ChatNameColor);
                buffer.WriteString(participant.AccountIcon);
                buffer.WriteString(participant.Country);
                buffer.WriteInt8(participant.GameModeAccessBool);
                buffer.WriteString(participant.GameModeAccessString);
                buffer.WriteInt32((int)participant.NormalRankLevel);
                buffer.WriteInt32((int)participant.CasualRankLevel);
                buffer.WriteInt32(participant.NormalRanking);
                buffer.WriteInt32(participant.CasualRanking);
                buffer.WriteInt8(participant.EligibleForCampaign);
            }
            else
            {
                // Empty slot
                buffer.WriteInt32(0); // AccountId
                buffer.WriteString(""); // Name
                buffer.WriteInt8(0); // Slot
                buffer.WriteInt32(0); // Rating
                buffer.WriteInt8(0); // LoadingPercent
                buffer.WriteInt8(0); // ReadyStatus
                buffer.WriteInt8(0); // InGame
                buffer.WriteInt8(0); // Verified
                buffer.WriteString(""); // ChatNameColor
                buffer.WriteString(""); // AccountIcon
                buffer.WriteString(""); // Country
                buffer.WriteInt8(0); // GameModeAccessBool
                buffer.WriteString(""); // GameModeAccessString
                buffer.WriteInt32(0); // NormalRankLevel
                buffer.WriteInt32(0); // CasualRankLevel
                buffer.WriteInt32(0); // NormalRanking
                buffer.WriteInt32(0); // CasualRanking
                buffer.WriteInt8(0); // EligibleForCampaign
            }
        }
        
        // Write group metadata
        buffer.WriteInt8(UpdateType);
        buffer.WriteInt8(GroupSize);
        buffer.WriteInt16(AverageTMR);
        buffer.WriteInt32(LeaderAccountId);
        buffer.WriteInt8((byte)GameType);
        buffer.WriteString(MapName);
        buffer.WriteString(GameModes);
        buffer.WriteString(Regions);
        buffer.WriteString(""); // TSNULL
        buffer.WriteString(PlayerInvitationResponses);
        buffer.WriteInt8(TeamSize);
        buffer.WriteString(""); // TSNULL
        buffer.WriteInt8(1); // Verified
        buffer.WriteInt8(1); // VerifiedOnly
        buffer.WriteInt8(BotDifficulty);
        buffer.WriteBool(RandomizeBots);
        buffer.WriteInt8((byte)GroupType);
        
        // Write friendship status
        foreach (byte status in FriendshipStatus)
        {
            buffer.WriteInt8(status);
        }
        
        buffer.PrependBufferSize();
        return buffer.Data;
    }
}
