namespace ASPIRE.Common.Enumerations.Match;

public enum MatchType
{
    AM_PUBLIC               = 00, // Public Match
    AM_MATCHMAKING          = 01, // Ranked Normal/Casual Matchmaking
    AM_SCHEDULED_MATCH      = 02, // Scheduled Tournament Match
    AM_UNSCHEDULED_MATCH    = 03, // Unscheduled League Match
    AM_MATCHMAKING_MIDWARS  = 04, // MidWars Matchmaking
    AM_MATCHMAKING_BOTMATCH = 05, // Bot Co-Op Matchmaking
    AM_UNRANKED_MATCHMAKING = 06, // Unranked Normal/Casual Matchmaking
    AM_MATCHMAKING_RIFTWARS = 07, // RiftWars Matchmaking
    AM_PUBLIC_PRELOBBY      = 08, // Public Pre-Lobby
    AM_MATCHMAKING_CUSTOM   = 09, // Custom Map Matchmaking
    AM_MATCHMAKING_CAMPAIGN = 10, // Ranked Season Normal/Casual Matchmaking

    NUM_ARRANGED_MATCH_TYPES
};

public static class MatchTypeExtensions
{
    public static bool IsMatchmakingType(MatchType matchType)
    {
        return matchType switch
        {
            MatchType.AM_PUBLIC               => false,
            MatchType.AM_SCHEDULED_MATCH      => false,
            MatchType.AM_UNSCHEDULED_MATCH    => false,

            MatchType.AM_MATCHMAKING          => true,
            MatchType.AM_MATCHMAKING_MIDWARS  => true,
            MatchType.AM_MATCHMAKING_RIFTWARS => true,
            MatchType.AM_MATCHMAKING_BOTMATCH => true,
            MatchType.AM_UNRANKED_MATCHMAKING => true,
            MatchType.AM_MATCHMAKING_CUSTOM   => true,
            MatchType.AM_MATCHMAKING_CAMPAIGN => true,

            _                                 => false
        };
    }
}
