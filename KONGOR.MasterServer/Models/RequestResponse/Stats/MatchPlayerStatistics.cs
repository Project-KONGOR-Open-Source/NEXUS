namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class MatchPlayerStatistics(
    MatchStartData matchStartData,
    Account account,
    PlayerStatistics playerStatistics,
    AccountStatistics currentMatchTypeStatistics,
    AccountStatistics publicMatchStatistics,
    AccountStatistics matchmakingStatistics)
{
    /// <summary>
    ///     The unique identifier for the match.
    /// </summary>
    [PHPProperty("match_id")]
    public int MatchID { get; init; } = playerStatistics.MatchID;

    /// <summary>
    ///     The player's account ID.
    /// </summary>
    [PHPProperty("account_id")]
    public int AccountID { get; init; } = playerStatistics.AccountID;

    /// <summary>
    ///     The account name (nickname) of the player.
    /// </summary>
    [PHPProperty("nickname")]
    public string AccountName { get; init; } = playerStatistics.AccountName;

    /// <summary>
    ///     The clan ID of the player's clan, or "0" if the player is not in a clan.
    /// </summary>
    [PHPProperty("clan_id")]
    public string ClanID { get; init; } = (playerStatistics.ClanID ?? 0).ToString();

    /// <summary>
    ///     The unique identifier of the hero played in the match.
    /// </summary>
    [PHPProperty("hero_id")]
    public string HeroProductID { get; init; } = (playerStatistics.HeroProductID ?? 0).ToString();

    /// <summary>
    ///     The lobby position of the player (0-9), indicating their slot in the pre-match lobby.
    /// </summary>
    [PHPProperty("position")]
    public string Position { get; init; } = playerStatistics.LobbyPosition.ToString();

    /// <summary>
    ///     The team the player was on ("1" for Legion, "2" for Hellbourne).
    /// </summary>
    [PHPProperty("team")]
    public string Team { get; init; } = playerStatistics.Team.ToString();

    /// <summary>
    ///     The final hero level reached by the player in the match (1-25).
    /// </summary>
    [PHPProperty("level")]
    public string Level { get; init; } = playerStatistics.HeroLevel.ToString();

    /// <summary>
    ///     The number of wins on the player's account.
    /// </summary>
    [PHPProperty("wins")]
    public string TotalWonMatches { get; init; } = currentMatchTypeStatistics.MatchesWon.ToString();

    /// <summary>
    ///     The number of losses on the player's account.
    /// </summary>
    [PHPProperty("losses")]
    public string TotalLostMatches { get; init; } = currentMatchTypeStatistics.MatchesLost.ToString();

    /// <summary>
    ///     The number of conceded matches on the player's account.
    /// </summary>
    [PHPProperty("concedes")]
    public string TotalConcededMatches { get; init; } = currentMatchTypeStatistics.MatchesConceded.ToString();

    /// <summary>
    ///     The number of concede votes the player cast during the match.
    /// </summary>
    [PHPProperty("concedevotes")]
    public string ConcedeVotes { get; init; } = playerStatistics.ConcedeVotes.ToString();

    /// <summary>
    ///     The number of times the player bought back into the match after dying.
    /// </summary>
    [PHPProperty("buybacks")]
    public string Buybacks { get; init; } = playerStatistics.Buybacks.ToString();

    /// <summary>
    ///     The number of disconnections on the player's account.
    /// </summary>
    [PHPProperty("discos")]
    public string TotalDisconnections { get; init; } = currentMatchTypeStatistics.MatchesDisconnected.ToString();

    /// <summary>
    ///     The number of times the player was kicked from matches on their account.
    /// </summary>
    [PHPProperty("kicked")]
    public string TotalKicks { get; init; } = currentMatchTypeStatistics.MatchesKicked.ToString();

    /// <summary>
    ///     The player's Public Skill Rating (PSR).
    /// </summary>
    [PHPProperty("pub_skill")]
    public string PublicMatchRating { get; init; } = publicMatchStatistics.SkillRating.ToString();

    /// <summary>
    ///     The number of public matches played on the player's account.
    /// </summary>
    [PHPProperty("pub_count")]
    public string PublicMatchCount { get; init; } = publicMatchStatistics.MatchesPlayed.ToString();

    /// <summary>
    ///     The player's solo Matchmaking Rating (MMR).
    /// </summary>
    [PHPProperty("amm_solo_rating")]
    public string SoloRankedMatchRating { get; init; } = matchmakingStatistics.SkillRating.ToString();

    /// <summary>
    ///     The number of solo ranked matches played on the player's account.
    /// </summary>
    [PHPProperty("amm_solo_count")]
    public string SoloRankedMatchCount { get; init; } = matchmakingStatistics.MatchesPlayed.ToString();

    /// <summary>
    ///     The player's team Matchmaking Rating (MMR).
    /// </summary>
    [PHPProperty("amm_team_rating")]
    public string TeamRankedMatchRating { get; init; } = matchmakingStatistics.SkillRating.ToString();

    /// <summary>
    ///     The number of team ranked matches played on the player's account.
    /// </summary>
    [PHPProperty("amm_team_count")]
    public string TeamRankedMatchCount { get; init; } = matchmakingStatistics.MatchesPlayed.ToString();

    /// <summary>
    ///     The player's performance score across all matches, calculated as (Kills + Assists) / Max(1, Deaths).
    /// </summary>
    [PHPProperty("avg_score")]
    public string PerformanceScore { get; init; } = currentMatchTypeStatistics.PerformanceScore.ToString("F2");

    /// <summary>
    ///     The number of enemy hero kills achieved by the player in the match.
    /// </summary>
    [PHPProperty("herokills")]
    public string HeroKills { get; init; } = playerStatistics.HeroKills.ToString();

    /// <summary>
    ///     The total damage dealt to enemy heroes by the player in the match.
    /// </summary>
    [PHPProperty("herodmg")]
    public string HeroDamage { get; init; } = playerStatistics.HeroDamage.ToString();

    /// <summary>
    ///     The total experience gained by the player's hero in the match.
    /// </summary>
    [PHPProperty("heroexp")]
    public string HeroExperience { get; init; } = playerStatistics.HeroExperience.ToString();

    /// <summary>
    ///     The total gold earned by the player's hero in the match.
    /// </summary>
    [PHPProperty("herokillsgold")]
    public string HeroGold { get; init; } = playerStatistics.GoldFromHeroKills.ToString();

    /// <summary>
    ///     The number of assists (participating in hero kills without landing the final blow) achieved by the player.
    /// </summary>
    [PHPProperty("heroassists")]
    public string HeroAssists { get; init; } = playerStatistics.HeroAssists.ToString();

    /// <summary>
    ///     The number of times the player died in the match.
    /// </summary>
    [PHPProperty("deaths")]
    public string Deaths { get; init; } = playerStatistics.HeroDeaths.ToString();

    /// <summary>
    ///     The total gold lost by the player due to deaths in the match.
    /// </summary>
    [PHPProperty("goldlost2death")]
    public string GoldLostToDeath { get; init; } = playerStatistics.GoldLostToDeath.ToString();

    /// <summary>
    ///     The total time in seconds the player spent dead (waiting to respawn) during the match.
    /// </summary>
    [PHPProperty("secs_dead")]
    public string SecondsDead { get; init; } = playerStatistics.SecondsDead.ToString();

    /// <summary>
    ///     The number of friendly team creeps killed by the player (last-hitting own creeps for gold/experience).
    /// </summary>
    [PHPProperty("teamcreepkills")]
    public string TeamCreepKills { get; init; } = playerStatistics.TeamCreepKills.ToString();

    /// <summary>
    ///     The total damage dealt to friendly team creeps by the player.
    /// </summary>
    [PHPProperty("teamcreepdmg")]
    public string TeamCreepDamage { get; init; } = playerStatistics.TeamCreepDamage.ToString();

    /// <summary>
    ///     The total experience gained from killing friendly team creeps.
    /// </summary>
    [PHPProperty("teamcreepexp")]
    public string TeamCreepExperience { get; init; } = playerStatistics.TeamCreepExperience.ToString();

    /// <summary>
    ///     The total gold earned from killing friendly team creeps.
    /// </summary>
    [PHPProperty("teamcreepgold")]
    public string TeamCreepGold { get; init; } = playerStatistics.TeamCreepGold.ToString();

    /// <summary>
    ///     The number of neutral creeps killed by the player (jungle creeps).
    /// </summary>
    [PHPProperty("neutralcreepkills")]
    public string NeutralCreepKills { get; init; } = playerStatistics.NeutralCreepKills.ToString();

    /// <summary>
    ///     The total damage dealt to neutral creeps by the player.
    /// </summary>
    [PHPProperty("neutralcreepdmg")]
    public string NeutralCreepDamage { get; init; } = playerStatistics.NeutralCreepDamage.ToString();

    /// <summary>
    ///     The total experience gained from killing neutral creeps.
    /// </summary>
    [PHPProperty("neutralcreepexp")]
    public string NeutralCreepExperience { get; init; } = playerStatistics.NeutralCreepExperience.ToString();

    /// <summary>
    ///     The total gold earned from killing neutral creeps.
    /// </summary>
    [PHPProperty("neutralcreepgold")]
    public string NeutralCreepGold { get; init; } = playerStatistics.NeutralCreepGold.ToString();

    /// <summary>
    ///     The total damage dealt to enemy buildings (towers, barracks, base structures) by the player.
    /// </summary>
    [PHPProperty("bdmg")]
    public string BuildingDamage { get; init; } = playerStatistics.BuildingDamage.ToString();

    /// <summary>
    ///     The total experience gained from damaging or destroying enemy buildings.
    /// </summary>
    [PHPProperty("bdmgexp")]
    public string BuildingExperience { get; init; } = playerStatistics.ExperienceFromBuildings.ToString();

    /// <summary>
    ///     The number of enemy buildings (towers, barracks) destroyed by the player.
    /// </summary>
    [PHPProperty("razed")]
    public string BuildingsRazed { get; init; } = playerStatistics.BuildingsRazed.ToString();

    /// <summary>
    ///     The total gold earned from damaging or destroying enemy buildings.
    /// </summary>
    [PHPProperty("bgold")]
    public string BuildingGold { get; init; } = playerStatistics.GoldFromBuildings.ToString();

    /// <summary>
    ///     The number of friendly creeps denied by the player (last-hitting friendly creeps to prevent opponents from gaining
    ///     gold/experience).
    /// </summary>
    [PHPProperty("denies")]
    public string Denies { get; init; } = playerStatistics.Denies.ToString();

    /// <summary>
    ///     The total experience denied to opponents through denying friendly creeps.
    /// </summary>
    [PHPProperty("exp_denied")]
    public string ExperienceDenied { get; init; } = playerStatistics.ExperienceDenied.ToString();

    /// <summary>
    ///     The total gold accumulated by the player at the end of the match.
    /// </summary>
    [PHPProperty("gold")]
    public string Gold { get; init; } = playerStatistics.Gold.ToString();

    /// <summary>
    ///     The total gold spent by the player on items during the match.
    /// </summary>
    [PHPProperty("gold_spent")]
    public string GoldSpent { get; init; } = playerStatistics.GoldSpent.ToString();

    /// <summary>
    ///     The total experience gained by the player during the match.
    /// </summary>
    [PHPProperty("exp")]
    public string Experience { get; init; } = playerStatistics.Experience.ToString();

    /// <summary>
    ///     The total number of actions performed by the player during the match (clicks, commands, ability usage, etc.).
    /// </summary>
    [PHPProperty("actions")]
    public string Actions { get; init; } = playerStatistics.Actions.ToString();

    /// <summary>
    ///     The total time in seconds the player was actively playing in the match.
    /// </summary>
    [PHPProperty("secs")]
    public string Seconds { get; init; } = playerStatistics.SecondsPlayed.ToString();

    /// <summary>
    ///     The number of consumable items (potions, wards, teleport scrolls, etc.) purchased by the player.
    /// </summary>
    [PHPProperty("consumables")]
    public string Consumables { get; init; } = playerStatistics.ConsumablesPurchased.ToString();

    /// <summary>
    ///     The number of observer or sentry wards placed by the player during the match.
    /// </summary>
    [PHPProperty("wards")]
    public string Wards { get; init; } = playerStatistics.WardsPlaced.ToString();

    /// <summary>
    ///     The total time in seconds the player spent within experience range of dying enemy units.
    /// </summary>
    [PHPProperty("time_earning_exp")]
    public string TimeEarningExperience { get; init; } = playerStatistics.TimeEarningExperience.ToString();

    /// <summary>
    ///     The number of First Blood awards earned by the player (1 or 0).
    /// </summary>
    [PHPProperty("bloodlust")]
    public string FirstBlood { get; init; } = playerStatistics.FirstBlood.ToString();

    /// <summary>
    ///     The number of Double Kill awards earned by the player (killing 2 heroes in quick succession).
    /// </summary>
    [PHPProperty("doublekill")]
    public string DoubleKill { get; init; } = playerStatistics.DoubleKill.ToString();

    /// <summary>
    ///     The number of Triple Kill awards earned by the player (killing 3 heroes in quick succession).
    /// </summary>
    [PHPProperty("triplekill")]
    public string TripleKill { get; init; } = playerStatistics.TripleKill.ToString();

    /// <summary>
    ///     The number of Quad Kill awards earned by the player (killing 4 heroes in quick succession).
    /// </summary>
    [PHPProperty("quadkill")]
    public string QuadKill { get; init; } = playerStatistics.QuadKill.ToString();

    /// <summary>
    ///     The number of Annihilation awards earned by the player (killing all 5 enemy heroes in quick succession).
    /// </summary>
    [PHPProperty("annihilation")]
    public string Annihilation { get; init; } = playerStatistics.Annihilation.ToString();

    /// <summary>
    ///     The number of 3-kill streaks achieved by the player (killing 3 heroes without dying).
    /// </summary>
    [PHPProperty("ks3")]
    public string KillStreak3 { get; init; } = playerStatistics.KillStreak03.ToString();

    /// <summary>
    ///     The number of 4-kill streaks achieved by the player (killing 4 heroes without dying).
    /// </summary>
    [PHPProperty("ks4")]
    public string KillStreak4 { get; init; } = playerStatistics.KillStreak04.ToString();

    /// <summary>
    ///     The number of 5-kill streaks achieved by the player (killing 5 heroes without dying).
    /// </summary>
    [PHPProperty("ks5")]
    public string KillStreak5 { get; init; } = playerStatistics.KillStreak05.ToString();

    /// <summary>
    ///     The number of 6-kill streaks achieved by the player (killing 6 heroes without dying).
    /// </summary>
    [PHPProperty("ks6")]
    public string KillStreak6 { get; init; } = playerStatistics.KillStreak06.ToString();

    /// <summary>
    ///     The number of 7-kill streaks achieved by the player (killing 7 heroes without dying).
    /// </summary>
    [PHPProperty("ks7")]
    public string KillStreak7 { get; init; } = playerStatistics.KillStreak07.ToString();

    /// <summary>
    ///     The number of 8-kill streaks achieved by the player (killing 8 heroes without dying).
    /// </summary>
    [PHPProperty("ks8")]
    public string KillStreak8 { get; init; } = playerStatistics.KillStreak08.ToString();

    /// <summary>
    ///     The number of 9-kill streaks achieved by the player (killing 9 heroes without dying).
    /// </summary>
    [PHPProperty("ks9")]
    public string KillStreak9 { get; init; } = playerStatistics.KillStreak09.ToString();

    /// <summary>
    ///     The number of 10-kill streaks achieved by the player (killing 10 heroes without dying).
    /// </summary>
    [PHPProperty("ks10")]
    public string KillStreak10 { get; init; } = playerStatistics.KillStreak10.ToString();

    /// <summary>
    ///     The number of 15-kill streaks achieved by the player (killing 15 heroes without dying).
    /// </summary>
    [PHPProperty("ks15")]
    public string KillStreak15 { get; init; } = playerStatistics.KillStreak15.ToString();

    /// <summary>
    ///     The number of Smackdown awards earned by the player (killing a player after taunting them).
    /// </summary>
    [PHPProperty("smackdown")]
    public string Smackdown { get; init; } = playerStatistics.Smackdown.ToString();

    /// <summary>
    ///     The number of Humiliation awards earned by the player (getting killed by a player after taunting them).
    /// </summary>
    [PHPProperty("humiliation")]
    public string Humiliation { get; init; } = playerStatistics.Humiliation.ToString();

    /// <summary>
    ///     The number of Nemesis awards earned by the player (repeatedly killing the same enemy hero).
    /// </summary>
    [PHPProperty("nemesis")]
    public string Nemesis { get; init; } = playerStatistics.Nemesis.ToString();

    /// <summary>
    ///     The number of Retribution awards earned by the player (killing an enemy hero who has killed you repeatedly).
    /// </summary>
    [PHPProperty("retribution")]
    public string Retribution { get; init; } = playerStatistics.Retribution.ToString();

    /// <summary>
    ///     Whether the player used a token (game access token or dice token) during the match ("1" if used, "0" otherwise).
    /// </summary>
    [PHPProperty("used_token")]
    public string UsedToken { get; init; } = playerStatistics.UsedToken.ToString();

    /// <summary>
    ///     The hero identifier in the format Hero_{Snake_Case_Name} (e.g. "Hero_Andromeda", "Hero_Legionnaire").
    /// </summary>
    [PHPProperty("cli_name")]
    public required string HeroIdentifier { get; init; }

    /// <summary>
    ///     The clan tag of the player's clan, or empty string if the player is not in a clan.
    /// </summary>
    [PHPProperty("tag")]
    public string ClanTag { get; init; } = account.Clan?.Tag ?? string.Empty;

    /// <summary>
    ///     The alternative avatar name used by the player during the match, or empty string if using the default hero skin.
    /// </summary>
    [PHPProperty("alt_avatar_name")]
    public string AlternativeAvatarName { get; init; } = playerStatistics.AlternativeAvatarName ?? string.Empty;

    /// <summary>
    ///     Seasonal campaign progression information for the player in the match.
    /// </summary>
    [PHPProperty("campaign_info")]
    public SeasonProgress SeasonProgress { get; init; } = new(matchStartData, playerStatistics, matchmakingStatistics);
}
