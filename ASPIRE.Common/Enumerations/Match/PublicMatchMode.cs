namespace ASPIRE.Common.Enumerations.Match;

public enum PublicMatchMode
{
    GAME_MODE_NORMAL,
    GAME_MODE_RANDOM_DRAFT,
    GAME_MODE_SINGLE_DRAFT,
    GAME_MODE_DEATHMATCH,
    GAME_MODE_BANNING_DRAFT,
    GAME_MODE_CAPTAINS_DRAFT,
    GAME_MODE_CAPTAINS_MODE,
    GAME_MODE_BANNING_PICK,
    GAME_MODE_ALL_RANDOM,
    GAME_MODE_LOCKPICK,
    GAME_MODE_BLIND_BAN,
    GAME_MODE_BOT_MATCH,
    GAME_MODE_KROS_MODE,
    GAME_MODE_FORCEPICK,
    GAME_MODE_SOCCERPICK,
    GAME_MODE_SOLO_SAME_HERO,
    GAME_MODE_SOLO_DIFF_HERO,
    GAME_MODE_COUNTER_PICK,
    GAME_MODE_MIDWARS_BETA,
    GAME_MODE_HEROBAN,
    GAME_MODE_REBORN,

    NUM_GAME_MODES
};

public static class PublicMatchModeExtensions
{
    public static string? GetPublicMatchModeCode(PublicMatchMode publicMatchMode)
    {
        return publicMatchMode switch
        {
            PublicMatchMode.GAME_MODE_NORMAL         => "nm",
            PublicMatchMode.GAME_MODE_RANDOM_DRAFT   => "rd",
            PublicMatchMode.GAME_MODE_SINGLE_DRAFT   => "sd",
            PublicMatchMode.GAME_MODE_DEATHMATCH     => "dm",
            PublicMatchMode.GAME_MODE_BANNING_DRAFT  => "bd",
            PublicMatchMode.GAME_MODE_CAPTAINS_DRAFT => "cd",
            PublicMatchMode.GAME_MODE_CAPTAINS_MODE  => "cm",
            PublicMatchMode.GAME_MODE_BANNING_PICK   => "bp",
            PublicMatchMode.GAME_MODE_ALL_RANDOM     => "ar",
            PublicMatchMode.GAME_MODE_LOCKPICK       => "lp",
            PublicMatchMode.GAME_MODE_BLIND_BAN      => "bb",
            PublicMatchMode.GAME_MODE_BOT_MATCH      => "bm",
            PublicMatchMode.GAME_MODE_KROS_MODE      => "km",
            PublicMatchMode.GAME_MODE_FORCEPICK      => "fp",
            PublicMatchMode.GAME_MODE_SOCCERPICK     => "sp",
            PublicMatchMode.GAME_MODE_SOLO_SAME_HERO => "ss",
            PublicMatchMode.GAME_MODE_SOLO_DIFF_HERO => "sm",
            PublicMatchMode.GAME_MODE_COUNTER_PICK   => "cp",
            PublicMatchMode.GAME_MODE_MIDWARS_BETA   => "mwb",
            PublicMatchMode.GAME_MODE_HEROBAN        => "hb",
            PublicMatchMode.GAME_MODE_REBORN         => "rb",
            _                                        => null
        };
    }

    public static string? GetPublicMatchModeName(PublicMatchMode publicMatchMode)
    {
        return publicMatchMode switch
        {
            PublicMatchMode.GAME_MODE_NORMAL         => "normal",
            PublicMatchMode.GAME_MODE_RANDOM_DRAFT   => "randomdraft",
            PublicMatchMode.GAME_MODE_SINGLE_DRAFT   => "singledraft",
            PublicMatchMode.GAME_MODE_DEATHMATCH     => "deathmatch",
            PublicMatchMode.GAME_MODE_BANNING_DRAFT  => "banningdraft",
            PublicMatchMode.GAME_MODE_CAPTAINS_DRAFT => "captainsdraft",
            PublicMatchMode.GAME_MODE_CAPTAINS_MODE  => "captainsmode",
            PublicMatchMode.GAME_MODE_BANNING_PICK   => "banningpick",
            PublicMatchMode.GAME_MODE_ALL_RANDOM     => "allrandom",
            PublicMatchMode.GAME_MODE_LOCKPICK       => "lockpick",
            PublicMatchMode.GAME_MODE_BLIND_BAN      => "blindban",
            PublicMatchMode.GAME_MODE_BOT_MATCH      => "botmatch",
            PublicMatchMode.GAME_MODE_KROS_MODE      => "krosmode",
            PublicMatchMode.GAME_MODE_FORCEPICK      => "forcepick",
            PublicMatchMode.GAME_MODE_SOCCERPICK     => "soccerpick",
            PublicMatchMode.GAME_MODE_SOLO_SAME_HERO => "solosame",
            PublicMatchMode.GAME_MODE_SOLO_DIFF_HERO => "solodiff",
            PublicMatchMode.GAME_MODE_COUNTER_PICK   => "counterpick",
            PublicMatchMode.GAME_MODE_MIDWARS_BETA   => "midwars_beta",
            PublicMatchMode.GAME_MODE_HEROBAN        => "heroban",
            PublicMatchMode.GAME_MODE_REBORN         => "reborn",
            _                                        => null
        };
    }

    public static string? GetPublicMatchModeString(PublicMatchMode publicMatchMode)
    {
        return publicMatchMode switch
        {
            PublicMatchMode.GAME_MODE_NORMAL         => "Mode_Normal",
            PublicMatchMode.GAME_MODE_RANDOM_DRAFT   => "Mode_RandomDraft",
            PublicMatchMode.GAME_MODE_SINGLE_DRAFT   => "Mode_SingleDraft",
            PublicMatchMode.GAME_MODE_DEATHMATCH     => "Mode_Deathmatch",
            PublicMatchMode.GAME_MODE_BANNING_DRAFT  => "Mode_BanningDraft",
            PublicMatchMode.GAME_MODE_CAPTAINS_DRAFT => "Mode_CaptainsDraft",
            PublicMatchMode.GAME_MODE_CAPTAINS_MODE  => "Mode_Captains",
            PublicMatchMode.GAME_MODE_BANNING_PICK   => "Mode_BanningPick",
            PublicMatchMode.GAME_MODE_LOCKPICK       => "Mode_Lockpick",
            PublicMatchMode.GAME_MODE_ALL_RANDOM     => "Mode_AllRandom",
            PublicMatchMode.GAME_MODE_BLIND_BAN      => "Mode_BlindBan",
            PublicMatchMode.GAME_MODE_BOT_MATCH      => "Mode_BotMatch",
            PublicMatchMode.GAME_MODE_KROS_MODE      => "Mode_KrosMode",
            PublicMatchMode.GAME_MODE_FORCEPICK      => "Mode_ForcePick",
            PublicMatchMode.GAME_MODE_SOCCERPICK     => "Mode_SoccerPick",
            PublicMatchMode.GAME_MODE_SOLO_SAME_HERO => "Mode_SoloSame",
            PublicMatchMode.GAME_MODE_SOLO_DIFF_HERO => "Mode_SoloDiff",
            PublicMatchMode.GAME_MODE_COUNTER_PICK   => "Mode_CounterPick",
            PublicMatchMode.GAME_MODE_MIDWARS_BETA   => "Mode_MidWars_Beta",
            PublicMatchMode.GAME_MODE_HEROBAN        => "Mode_HeroBan",
            PublicMatchMode.GAME_MODE_REBORN         => "Mode_Reborn",
            _                                        => null
        };
    }

    public static PublicMatchMode? GetPublicMatchModeFromCode(string publicMatchModeCode)
    {
        return publicMatchModeCode switch
        {
            "nm"  => PublicMatchMode.GAME_MODE_NORMAL,
            "rd"  => PublicMatchMode.GAME_MODE_RANDOM_DRAFT,
            "sd"  => PublicMatchMode.GAME_MODE_SINGLE_DRAFT,
            "dm"  => PublicMatchMode.GAME_MODE_DEATHMATCH,
            "bd"  => PublicMatchMode.GAME_MODE_BANNING_DRAFT,
            "cd"  => PublicMatchMode.GAME_MODE_CAPTAINS_DRAFT,
            "cm"  => PublicMatchMode.GAME_MODE_CAPTAINS_MODE,
            "bp"  => PublicMatchMode.GAME_MODE_BANNING_PICK,
            "ar"  => PublicMatchMode.GAME_MODE_ALL_RANDOM,
            "lp"  => PublicMatchMode.GAME_MODE_LOCKPICK,
            "bb"  => PublicMatchMode.GAME_MODE_BLIND_BAN,
            "bm"  => PublicMatchMode.GAME_MODE_BOT_MATCH,
            "km"  => PublicMatchMode.GAME_MODE_KROS_MODE,
            "fp"  => PublicMatchMode.GAME_MODE_FORCEPICK,
            "sp"  => PublicMatchMode.GAME_MODE_SOCCERPICK,
            "ss"  => PublicMatchMode.GAME_MODE_SOLO_SAME_HERO,
            "sm"  => PublicMatchMode.GAME_MODE_SOLO_DIFF_HERO,
            "cp"  => PublicMatchMode.GAME_MODE_COUNTER_PICK,
            "mwb" => PublicMatchMode.GAME_MODE_MIDWARS_BETA,
            "hb"  => PublicMatchMode.GAME_MODE_HEROBAN,
            "rb"  => PublicMatchMode.GAME_MODE_REBORN,
            _     => null
        };
    }

    public static PublicMatchMode? GetPublicMatchModeFromName(string publicMatchModeName)
    {
        return publicMatchModeName switch
        {
            "normal"        => PublicMatchMode.GAME_MODE_NORMAL,
            "randomdraft"   => PublicMatchMode.GAME_MODE_RANDOM_DRAFT,
            "singledraft"   => PublicMatchMode.GAME_MODE_SINGLE_DRAFT,
            "deathmatch"    => PublicMatchMode.GAME_MODE_DEATHMATCH,
            "banningdraft"  => PublicMatchMode.GAME_MODE_BANNING_DRAFT,
            "captainsdraft" => PublicMatchMode.GAME_MODE_CAPTAINS_DRAFT,
            "captainsmode"  => PublicMatchMode.GAME_MODE_CAPTAINS_MODE,
            "banningpick"   => PublicMatchMode.GAME_MODE_BANNING_PICK,
            "allrandom"     => PublicMatchMode.GAME_MODE_ALL_RANDOM,
            "lockpick"      => PublicMatchMode.GAME_MODE_LOCKPICK,
            "blindban"      => PublicMatchMode.GAME_MODE_BLIND_BAN,
            "botmatch"      => PublicMatchMode.GAME_MODE_BOT_MATCH,
            "krosmode"      => PublicMatchMode.GAME_MODE_KROS_MODE,
            "forcepick"     => PublicMatchMode.GAME_MODE_FORCEPICK,
            "soccerpick"    => PublicMatchMode.GAME_MODE_SOCCERPICK,
            "solosame"      => PublicMatchMode.GAME_MODE_SOLO_SAME_HERO,
            "solodiff"      => PublicMatchMode.GAME_MODE_SOLO_DIFF_HERO,
            "counterpick"   => PublicMatchMode.GAME_MODE_COUNTER_PICK,
            "midwars_beta"  => PublicMatchMode.GAME_MODE_MIDWARS_BETA,
            "heroban"       => PublicMatchMode.GAME_MODE_HEROBAN,
            "reborn"        => PublicMatchMode.GAME_MODE_REBORN,
            _               => null
        };
    }

    public static PublicMatchMode? GetPublicMatchModeFromString(string publicMatchModeString)
    {
        return publicMatchModeString switch
        {
            "Mode_Normal"        => PublicMatchMode.GAME_MODE_NORMAL,
            "Mode_RandomDraft"   => PublicMatchMode.GAME_MODE_RANDOM_DRAFT,
            "Mode_SingleDraft"   => PublicMatchMode.GAME_MODE_SINGLE_DRAFT,
            "Mode_Deathmatch"    => PublicMatchMode.GAME_MODE_DEATHMATCH,
            "Mode_BanningDraft"  => PublicMatchMode.GAME_MODE_BANNING_DRAFT,
            "Mode_CaptainsDraft" => PublicMatchMode.GAME_MODE_CAPTAINS_DRAFT,
            "Mode_Captains"      => PublicMatchMode.GAME_MODE_CAPTAINS_MODE,
            "Mode_BanningPick"   => PublicMatchMode.GAME_MODE_BANNING_PICK,
            "Mode_Lockpick"      => PublicMatchMode.GAME_MODE_LOCKPICK,
            "Mode_AllRandom"     => PublicMatchMode.GAME_MODE_ALL_RANDOM,
            "Mode_BlindBan"      => PublicMatchMode.GAME_MODE_BLIND_BAN,
            "Mode_BotMatch"      => PublicMatchMode.GAME_MODE_BOT_MATCH,
            "Mode_KrosMode"      => PublicMatchMode.GAME_MODE_KROS_MODE,
            "Mode_ForcePick"     => PublicMatchMode.GAME_MODE_FORCEPICK,
            "Mode_SoccerPick"    => PublicMatchMode.GAME_MODE_SOCCERPICK,
            "Mode_SoloSame"      => PublicMatchMode.GAME_MODE_SOLO_SAME_HERO,
            "Mode_SoloDiff"      => PublicMatchMode.GAME_MODE_SOLO_DIFF_HERO,
            "Mode_CounterPick"   => PublicMatchMode.GAME_MODE_COUNTER_PICK,
            "Mode_MidWars_Beta"  => PublicMatchMode.GAME_MODE_MIDWARS_BETA,
            "Mode_HeroBan"       => PublicMatchMode.GAME_MODE_HEROBAN,
            "Mode_Reborn"        => PublicMatchMode.GAME_MODE_REBORN,
            _                    => null
        };
    }
}
