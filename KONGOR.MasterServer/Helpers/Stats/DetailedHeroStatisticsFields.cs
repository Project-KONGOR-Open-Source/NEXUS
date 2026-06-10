namespace KONGOR.MasterServer.Helpers.Stats;

/// <summary>
///     Writes the detailed per-hero statistics field set shared by the "get_hero_stats" and "get_campaign_hero_stats" responses.
///     The original API emits sixty-three fields per game mode, each prefixed by a mode-specific token (for example "rnk_" for ranked or "cam_cs_" for campaign casual).
///     These are populated from the accumulated per-hero summary in <see cref="HeroStats"/>.
///     The few account-level skill rating and match-count fields that are not tracked per hero are emitted with a value of "0", mirroring the original API which defaults absent database columns to zero.
/// </summary>
public static class DetailedHeroStatisticsFields
{
    /// <summary>
    ///     The sixty-three detailed per-hero statistics field names, in the order the original API emits them.
    ///     Each field is prefixed with a mode-specific token when written to a response (for example "ph_used" becomes "rnk_ph_used" for ranked statistics).
    /// </summary>
    private static readonly string[] FieldNames =
    [
        "ph_used",            "ph_wins",            "ph_losses",           "ph_concedes",        "ph_concedevotes",   "ph_buybacks",        "ph_discos",
        "ph_kicked",          "ph_pub_skill",       "ph_pub_count",        "ph_amm_solo_rating", "ph_amm_solo_count", "ph_amm_team_rating", "ph_amm_team_count",
        "ph_avg_score",       "ph_herokills",       "ph_herodmg",          "ph_heroexp",         "ph_herokillsgold",  "ph_heroassists",     "ph_deaths",
        "ph_goldlost2death",  "ph_secs_dead",       "ph_teamcreepkills",   "ph_teamcreepdmg",    "ph_teamcreepexp",   "ph_teamcreepgold",   "ph_neutralcreepkills",
        "ph_neutralcreepdmg", "ph_neutralcreepexp", "ph_neutralcreepgold", "ph_bdmg",            "ph_bdmgexp",        "ph_razed",           "ph_bgold",
        "ph_denies",          "ph_exp_denied",      "ph_gold",             "ph_gold_spent",      "ph_exp",            "ph_actions",         "ph_secs",
        "ph_consumables",     "ph_wards",           "ph_time_earning_exp", "ph_bloodlust",       "ph_doublekill",     "ph_triplekill",      "ph_quadkill",
        "ph_annihilation",    "ph_ks3",             "ph_ks4",              "ph_ks5",             "ph_ks6",            "ph_ks7",             "ph_ks8",
        "ph_ks9",             "ph_ks10",            "ph_ks15",             "ph_smackdown",       "ph_humiliation",    "ph_nemesis",         "ph_retribution"
    ];

    /// <summary>
    ///     Writes all sixty-three detailed per-hero statistics fields into the given dictionary, prefixing each field name with the given mode prefix.
    ///     Tracked fields are populated from the supplied statistics; untracked fields, and all fields when <paramref name="statistics"/> is <see langword="null"/>, are written as "0".
    /// </summary>
    /// <param name="target">The response dictionary to write the fields into.</param>
    /// <param name="prefix">The mode-specific field prefix (for example "rnk_", "cs_", "cam_", "cam_cs_", or the empty string for non-prefixed player statistics).</param>
    /// <param name="statistics">The per-hero statistics to populate the tracked fields from, or <see langword="null"/> when no statistics are available.</param>
    public static void Write(OrderedDictionary target, string prefix, HeroStats? statistics)
    {
        Dictionary<string, string> trackedValues = GetTrackedValues(statistics);

        foreach (string fieldName in FieldNames)
            target.Add(prefix + fieldName, trackedValues.GetValueOrDefault(fieldName, "0"));
    }

    /// <summary>
    ///     Maps the detailed field names that are tracked in <see cref="HeroStats"/> to their values.
    ///     Returns an empty map when no statistics are available, in which case every field defaults to "0".
    ///     The account-level skill rating and match-count fields ("ph_pub_skill", "ph_pub_count", and the "ph_amm_*" ratings and counts) are not tracked per hero, so they are not mapped here and consequently default to "0".
    /// </summary>
    private static Dictionary<string, string> GetTrackedValues(HeroStats? statistics)
    {
        if (statistics is null)
            return [];

        int averageScore = statistics.GamesPlayed > 0 ? statistics.ScoreTotal / statistics.GamesPlayed : 0;

        return new Dictionary<string, string>
        {
            ["ph_used"]               = statistics.GamesPlayed.ToString(),
            ["ph_wins"]               = statistics.Wins.ToString(),
            ["ph_losses"]             = statistics.Losses.ToString(),
            ["ph_concedes"]           = statistics.Concedes.ToString(),
            ["ph_concedevotes"]       = statistics.ConcedeVotes.ToString(),
            ["ph_buybacks"]           = statistics.Buybacks.ToString(),
            ["ph_discos"]             = statistics.Disconnections.ToString(),
            ["ph_kicked"]             = statistics.Kicks.ToString(),
            ["ph_avg_score"]          = averageScore.ToString(),
            ["ph_herokills"]          = statistics.HeroKills.ToString(),
            ["ph_herodmg"]            = statistics.HeroDamage.ToString(),
            ["ph_heroexp"]            = statistics.HeroExperience.ToString(),
            ["ph_herokillsgold"]      = statistics.GoldFromHeroKills.ToString(),
            ["ph_heroassists"]        = statistics.HeroAssists.ToString(),
            ["ph_deaths"]             = statistics.HeroDeaths.ToString(),
            ["ph_goldlost2death"]     = statistics.GoldLostToDeath.ToString(),
            ["ph_secs_dead"]          = statistics.SecondsDead.ToString(),
            ["ph_teamcreepkills"]     = statistics.TeamCreepKills.ToString(),
            ["ph_teamcreepdmg"]       = statistics.TeamCreepDamage.ToString(),
            ["ph_teamcreepexp"]       = statistics.TeamCreepExperience.ToString(),
            ["ph_teamcreepgold"]      = statistics.TeamCreepGold.ToString(),
            ["ph_neutralcreepkills"]  = statistics.NeutralCreepKills.ToString(),
            ["ph_neutralcreepdmg"]    = statistics.NeutralCreepDamage.ToString(),
            ["ph_neutralcreepexp"]    = statistics.NeutralCreepExperience.ToString(),
            ["ph_neutralcreepgold"]   = statistics.NeutralCreepGold.ToString(),
            ["ph_bdmg"]               = statistics.BuildingDamage.ToString(),
            ["ph_bdmgexp"]            = statistics.ExperienceFromBuildings.ToString(),
            ["ph_razed"]              = statistics.BuildingsRazed.ToString(),
            ["ph_bgold"]              = statistics.GoldFromBuildings.ToString(),
            ["ph_denies"]             = statistics.Denies.ToString(),
            ["ph_exp_denied"]         = statistics.ExperienceDenied.ToString(),
            ["ph_gold"]               = statistics.Gold.ToString(),
            ["ph_gold_spent"]         = statistics.GoldSpent.ToString(),
            ["ph_exp"]                = statistics.Experience.ToString(),
            ["ph_actions"]            = statistics.Actions.ToString(),
            ["ph_secs"]               = statistics.SecondsPlayed.ToString(),
            ["ph_consumables"]        = statistics.ConsumablesPurchased.ToString(),
            ["ph_wards"]              = statistics.WardsPlaced.ToString(),
            ["ph_time_earning_exp"]   = statistics.TimeEarningExperience.ToString(),
            ["ph_bloodlust"]          = statistics.FirstBloods.ToString(),
            ["ph_doublekill"]         = statistics.DoubleKills.ToString(),
            ["ph_triplekill"]         = statistics.TripleKills.ToString(),
            ["ph_quadkill"]           = statistics.QuadKills.ToString(),
            ["ph_annihilation"]       = statistics.Annihilations.ToString(),
            ["ph_ks3"]                = statistics.KillStreak03.ToString(),
            ["ph_ks4"]                = statistics.KillStreak04.ToString(),
            ["ph_ks5"]                = statistics.KillStreak05.ToString(),
            ["ph_ks6"]                = statistics.KillStreak06.ToString(),
            ["ph_ks7"]                = statistics.KillStreak07.ToString(),
            ["ph_ks8"]                = statistics.KillStreak08.ToString(),
            ["ph_ks9"]                = statistics.KillStreak09.ToString(),
            ["ph_ks10"]               = statistics.KillStreak10.ToString(),
            ["ph_ks15"]               = statistics.KillStreak15.ToString(),
            ["ph_smackdown"]          = statistics.Smackdowns.ToString(),
            ["ph_humiliation"]        = statistics.Humiliations.ToString(),
            ["ph_nemesis"]            = statistics.Nemeses.ToString(),
            ["ph_retribution"]        = statistics.Retributions.ToString()
        };
    }
}
