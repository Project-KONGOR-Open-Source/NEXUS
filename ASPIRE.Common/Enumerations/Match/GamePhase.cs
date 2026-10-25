namespace ASPIRE.Common.Enumerations.Match;

/// <summary>
///     The phases of a match, as the match server reports them, with the names and values of the engine's "EGamePhase" in "HON.Shared/game_shared_types.h".
/// </summary>
public enum GamePhase
{
    GAME_PHASE_INVALID = -1,

    GAME_PHASE_IDLE = 0,
    GAME_PHASE_WAITING_FOR_PLAYERS,
    GAME_PHASE_HERO_BAN,
    GAME_PHASE_HERO_SELECT,
    GAME_PHASE_HERO_LOADING,
    GAME_PHASE_PRE_MATCH,
    GAME_PHASE_ACTIVE,
    GAME_PHASE_POST_MATCH,
    GAME_PHASE_ENDED,
    GAME_PHASE_HERO_BLIND_BAN,
    GAME_PHASE_HERO_LOCK,
    GAME_PHASE_HERO_LOCK_PICK,
    GAME_PHASE_SHUFFLE_PICK,
    GAME_PHASE_HERO_VOTE_BLIND_BAN,
    GAME_PHASE_CUSTOM,
    GAME_PHASE_WAIT
}
