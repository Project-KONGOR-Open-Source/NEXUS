namespace ASPIRE.Common.Enumerations;

[Flags]
public enum MatchOptions
{
    None = 0 << 00,

    AllPick = 1 << 00, // option[ap]
    AllRandom = 1 << 01, // option[ar]
    AlternateHeroPicking = 1 << 02, // option[alt_pick]
    AutoBalanced = 1 << 03, // option[ab]
    BalancedRandom = 1 << 04, // option[br]
    BanPhase = 1 << 05, // option[veto]
    BlitzMode = 1 << 06, // option[rapidfire]
    CasualMode = 1 << 07, // option[cas]
    DevelopmentHeroes = 1 << 08, // option[dev_heroes]
    DropItems = 1 << 09, // option[drp_itm]
    DuplicateHeroes = 1 << 10, // option[dup_h]
    EasyMode = 1 << 11, // option[em]
    Gated = 1 << 12, // option[gated]
    Hardcore = 1 << 13, // option[hardcore]
    NoAgilityHeroes = 1 << 14, // option[no_agi]
    NoHeroRepick = 1 << 15, // option[no_repick]
    NoHeroSwap = 1 << 16, // option[no_swap]
    NoIntelligenceHeroes = 1 << 17, // option[no_int]
    NoLeavers = 1 << 18, // option[nl]
    NoPowerUps = 1 << 19, // option[no_pups]
    NoRespawnTimer = 1 << 20, // option[no_timer]
    NoStatistics = 1 << 21, // option[no_stats]
    NoStrengthHeroes = 1 << 22, // option[no_str]
    Official = 1 << 23, // option[officl]
    ReverseHeroSelection = 1 << 24, // option[rev_hs]
    ReverseSelection = 1 << 25, // option[rs]
    ShuffleAbilities = 1 << 26, // option[shuffleabilities]
    ShuffleTeams = 1 << 27, // option[shuf]
    TournamentRules = 1 << 28, // option[tr]
    VerifiedOnly = 1 << 29 // option[verified_only]
}