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
}
