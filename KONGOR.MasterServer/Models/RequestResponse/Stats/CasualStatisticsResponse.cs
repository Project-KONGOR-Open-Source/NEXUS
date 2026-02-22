namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

/// <summary>
///     Response for show_stats with table="casual" (casual matchmaking statistics with cs_ prefix).
/// </summary>
public class CasualStatisticsResponse(Account account, AccountStatistics statistics, AggregateStatistics aggregates)
{
    #region Account Information

    /// <summary>
    ///     The account's super ID (main account ID for multi-account users).
    /// </summary>
    [PHPProperty("super_id")]
    public string SuperID { get; init; } = account.User.Accounts.Single(account => account.IsMain).ID.ToString();

    /// <summary>
    ///     The account name with the clan tag (if applicable).
    /// </summary>
    [PHPProperty("nickname")]
    public string Nickname { get; init; } = account.NameWithClanTag;

    /// <summary>
    ///     The account standing (moderation status). "3" indicates good standing.
    /// </summary>
    [PHPProperty("standing")]
    public string Standing { get; init; } = "3"; // TODO: Implement Account Standing

    /// <summary>
    ///     The account type (e.g., legacy, free-to-play, etc.).
    /// </summary>
    [PHPProperty("account_type")]
    public string AccountTypeValue { get; init; } = ((int) account.Type).ToString();

    /// <summary>
    ///     The unique account identifier.
    /// </summary>
    [PHPProperty("account_id")]
    public string AccountID { get; init; } = account.ID.ToString();

    #endregion

    #region Casual Match Statistics (cs_ Prefix)

    /// <summary>
    ///     Total casual matches played.
    /// </summary>
    [PHPProperty("cs_games_played")]
    public string GamesPlayed { get; init; } = statistics.MatchesPlayed.ToString();

    /// <summary>
    ///     Total casual matches won.
    /// </summary>
    [PHPProperty("cs_wins")]
    public string Wins { get; init; } = statistics.MatchesWon.ToString();

    /// <summary>
    ///     Total casual matches lost.
    /// </summary>
    [PHPProperty("cs_losses")]
    public string Losses { get; init; } = statistics.MatchesLost.ToString();

    /// <summary>
    ///     Total casual matches conceded.
    /// </summary>
    [PHPProperty("cs_concedes")]
    public string Concedes { get; init; } = statistics.MatchesConceded.ToString();

    /// <summary>
    ///     Total times voted to concede in casual matches.
    /// </summary>
    [PHPProperty("cs_concedevotes")]
    public string ConcedeVotes { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total buybacks in casual matches.
    /// </summary>
    [PHPProperty("cs_buybacks")]
    public string Buybacks { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total disconnections in casual matches.
    /// </summary>
    [PHPProperty("cs_discos")]
    public string Disconnections { get; init; } = statistics.MatchesDisconnected.ToString();

    /// <summary>
    ///     Total times kicked from casual matches.
    /// </summary>
    [PHPProperty("cs_kicked")]
    public string Kicked { get; init; } = statistics.MatchesKicked.ToString();

    /// <summary>
    ///     Solo queue matchmaking rating (MMR) for casual mode. Base rating is 1500.
    /// </summary>
    [PHPProperty("cs_amm_solo_rating")]
    public string SoloRating { get; init; } = "1500.000"; // TODO: Implement Solo Queue Rating

    /// <summary>
    ///     Total solo queue casual matches counted towards rating.
    /// </summary>
    [PHPProperty("cs_amm_solo_count")]
    public string SoloCount { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Solo queue casual rating confidence (0.00 to 1.00).
    /// </summary>
    [PHPProperty("cs_amm_solo_conf")]
    public string SoloConfidence { get; init; } = "0.00"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Number of provisional (placement) matches played in solo queue casual.
    /// </summary>
    [PHPProperty("cs_amm_solo_prov")]
    public string SoloProvisional { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Solo queue casual placement set identifier.
    /// </summary>
    [PHPProperty("cs_amm_solo_pset")]
    public string SoloPlacementSet { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Team/group queue matchmaking rating (MMR) for casual mode. Base rating is 1500.
    /// </summary>
    [PHPProperty("cs_amm_team_rating")]
    public string TeamRating { get; init; } = statistics.SkillRating.ToString("F3");

    /// <summary>
    ///     Total team queue casual matches counted towards rating.
    /// </summary>
    [PHPProperty("cs_amm_team_count")]
    public string TeamCount { get; init; } = statistics.MatchesPlayed.ToString();

    /// <summary>
    ///     Team queue casual rating confidence (0.00 to 1.00).
    /// </summary>
    [PHPProperty("cs_amm_team_conf")]
    public string TeamConfidence { get; init; } = "0.00"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Number of provisional (placement) matches played in team queue casual.
    /// </summary>
    [PHPProperty("cs_amm_team_prov")]
    public string TeamProvisional { get; init; } = statistics.MatchesPlayed.ToString();

    /// <summary>
    ///     Team queue casual placement set identifier.
    /// </summary>
    [PHPProperty("cs_amm_team_pset")]
    public string TeamPlacementSet { get; init; } = "127"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total hero kills in casual matches.
    /// </summary>
    [PHPProperty("cs_herokills")]
    public string HeroKills { get; init; } = statistics.HeroKills.ToString();

    /// <summary>
    ///     Total damage dealt to enemy heroes in casual matches.
    /// </summary>
    [PHPProperty("cs_herodmg")]
    public string HeroDamage { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total experience gained from hero kills in casual matches.
    /// </summary>
    [PHPProperty("cs_heroexp")]
    public string HeroExperience { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total gold earned from hero kills in casual matches.
    /// </summary>
    [PHPProperty("cs_herokillsgold")]
    public string HeroKillsGold { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total hero assists in casual matches.
    /// </summary>
    [PHPProperty("cs_heroassists")]
    public string HeroAssists { get; init; } = statistics.HeroAssists.ToString();

    /// <summary>
    ///     Total deaths in casual matches.
    /// </summary>
    [PHPProperty("cs_deaths")]
    public string Deaths { get; init; } = statistics.HeroDeaths.ToString();

    /// <summary>
    ///     Total gold lost to deaths in casual matches.
    /// </summary>
    [PHPProperty("cs_goldlost2death")]
    public string GoldLostToDeath { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total seconds spent dead in casual matches.
    /// </summary>
    [PHPProperty("cs_secs_dead")]
    public string SecondsDead { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total enemy lane creeps killed in casual matches.
    /// </summary>
    [PHPProperty("cs_teamcreepkills")]
    public string TeamCreepKills { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total damage dealt to enemy lane creeps in casual matches.
    /// </summary>
    [PHPProperty("cs_teamcreepdmg")]
    public string TeamCreepDamage { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total experience gained from enemy lane creeps in casual matches.
    /// </summary>
    [PHPProperty("cs_teamcreepexp")]
    public string TeamCreepExperience { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total gold earned from enemy lane creeps in casual matches.
    /// </summary>
    [PHPProperty("cs_teamcreepgold")]
    public string TeamCreepGold { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total neutral creeps killed in casual matches.
    /// </summary>
    [PHPProperty("cs_neutralcreepkills")]
    public string NeutralCreepKills { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total damage dealt to neutral creeps in casual matches.
    /// </summary>
    [PHPProperty("cs_neutralcreepdmg")]
    public string NeutralCreepDamage { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total experience gained from neutral creeps in casual matches.
    /// </summary>
    [PHPProperty("cs_neutralcreepexp")]
    public string NeutralCreepExperience { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total gold earned from neutral creeps in casual matches.
    /// </summary>
    [PHPProperty("cs_neutralcreepgold")]
    public string NeutralCreepGold { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total damage dealt to buildings in casual matches.
    /// </summary>
    [PHPProperty("cs_bdmg")]
    public string BuildingDamage { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total experience gained from destroying buildings in casual matches.
    /// </summary>
    [PHPProperty("cs_bdmgexp")]
    public string BuildingExperience { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total buildings destroyed in casual matches.
    /// </summary>
    [PHPProperty("cs_razed")]
    public string BuildingsRazed { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total gold earned from destroying buildings in casual matches.
    /// </summary>
    [PHPProperty("cs_bgold")]
    public string BuildingGold { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total allied creeps denied in casual matches.
    /// </summary>
    [PHPProperty("cs_denies")]
    public string Denies { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total experience denied to enemies through denies in casual matches.
    /// </summary>
    [PHPProperty("cs_exp_denied")]
    public string ExperienceDenied { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total gold earned in casual matches.
    /// </summary>
    [PHPProperty("cs_gold")]
    public string Gold { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total gold spent on items in casual matches.
    /// </summary>
    [PHPProperty("cs_gold_spent")]
    public string GoldSpent { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total experience earned in casual matches.
    /// </summary>
    [PHPProperty("cs_exp")]
    public string Experience { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total actions (commands issued) in casual matches.
    /// </summary>
    [PHPProperty("cs_actions")]
    public string Actions { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total seconds played in casual matches.
    /// </summary>
    [PHPProperty("cs_secs")]
    public string SecondsPlayed { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total consumable items used in casual matches.
    /// </summary>
    [PHPProperty("cs_consumables")]
    public string Consumables { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total wards placed in casual matches.
    /// </summary>
    [PHPProperty("cs_wards")]
    public string Wards { get; init; } = statistics.WardsPlaced.ToString();

    /// <summary>
    ///     Total easy mode casual matches played.
    /// </summary>
    [PHPProperty("cs_em_played")]
    public string EasyModeGamesPlayed { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     The account level for casual mode.
    /// </summary>
    [PHPProperty("cs_level")]
    public string CasualLevel { get; init; } = account.User.TotalLevel.ToString();

    /// <summary>
    ///     The current experience points towards the next casual level.
    /// </summary>
    [PHPProperty("cs_level_exp")]
    public string CasualLevelExperience { get; init; } = account.User.TotalExperience.ToString();

    /// <summary>
    ///     The minimum experience earned in a single casual match.
    /// </summary>
    [PHPProperty("cs_min_exp")]
    public string CasualMinimumExperience { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     The maximum experience earned in a single casual match.
    /// </summary>
    [PHPProperty("cs_max_exp")]
    public string CasualMaximumExperience { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total time (in seconds) spent earning experience in casual matches.
    /// </summary>
    [PHPProperty("cs_time_earning_exp")]
    public string TimeEarningExperience { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total bloodlust kills (first blood) in casual matches.
    /// </summary>
    [PHPProperty("cs_bloodlust")]
    public string Bloodlust { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total double kills (2 kills in quick succession) in casual matches.
    /// </summary>
    [PHPProperty("cs_doublekill")]
    public string DoubleKills { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total triple kills (3 kills in quick succession) in casual matches.
    /// </summary>
    [PHPProperty("cs_triplekill")]
    public string TripleKills { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total quad kills (4 kills in quick succession) in casual matches.
    /// </summary>
    [PHPProperty("cs_quadkill")]
    public string QuadKills { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total annihilations (5 kills, entire enemy team) in casual matches.
    /// </summary>
    [PHPProperty("cs_annihilation")]
    public string Annihilations { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total 3-kill streaks in casual matches.
    /// </summary>
    [PHPProperty("cs_ks3")]
    public string KillStreak3 { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total 4-kill streaks in casual matches.
    /// </summary>
    [PHPProperty("cs_ks4")]
    public string KillStreak4 { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total 5-kill streaks in casual matches.
    /// </summary>
    [PHPProperty("cs_ks5")]
    public string KillStreak5 { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total 6-kill streaks in casual matches.
    /// </summary>
    [PHPProperty("cs_ks6")]
    public string KillStreak6 { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total 7-kill streaks in casual matches.
    /// </summary>
    [PHPProperty("cs_ks7")]
    public string KillStreak7 { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total 8-kill streaks in casual matches.
    /// </summary>
    [PHPProperty("cs_ks8")]
    public string KillStreak8 { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total 9-kill streaks in casual matches.
    /// </summary>
    [PHPProperty("cs_ks9")]
    public string KillStreak9 { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total 10-kill streaks in casual matches.
    /// </summary>
    [PHPProperty("cs_ks10")]
    public string KillStreak10 { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total 15-kill streaks (immortal) in casual matches.
    /// </summary>
    [PHPProperty("cs_ks15")]
    public string KillStreak15 { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total smackdowns (killing a player on a 3+ kill streak) in casual matches.
    /// </summary>
    [PHPProperty("cs_smackdown")]
    public string Smackdowns { get; init; } = statistics.Smackdowns.ToString();

    /// <summary>
    ///     Total humiliations (killing a player with 0 kills) in casual matches.
    /// </summary>
    [PHPProperty("cs_humiliation")]
    public string Humiliations { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total nemesis kills (killing the same enemy 3+ times) in casual matches.
    /// </summary>
    [PHPProperty("cs_nemesis")]
    public string Nemesis { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total retribution kills (killing your nemesis) in casual matches.
    /// </summary>
    [PHPProperty("cs_retribution")]
    public string Retributions { get; init; } = "0"; // TODO: Implement Data Tracking

    #endregion

    #region Global Account Statistics

    /// <summary>
    ///     The account level (combined across all game modes).
    /// </summary>
    [PHPProperty("level")]
    public string Level { get; init; } = account.User.TotalLevel.ToString();

    /// <summary>
    ///     The current experience points towards the next level.
    /// </summary>
    [PHPProperty("level_exp")]
    public int LevelExperience { get; init; } = account.User.TotalExperience;

    /// <summary>
    ///     Total disconnections across all game modes.
    /// </summary>
    [PHPProperty("discos")]
    public string TotalDisconnections { get; init; } = aggregates.TotalDisconnections.ToString();

    /// <summary>
    ///     Number of possible disconnections (games where the player could have disconnected but did not).
    /// </summary>
    [PHPProperty("possible_discos")]
    public string PossibleDisconnections { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total games played across all PvP game modes.
    /// </summary>
    [PHPProperty("games_played")]
    public string TotalGamesPlayed { get; init; } = aggregates.TotalGamesPlayed.ToString();

    /// <summary>
    ///     Total bot/practice games won.
    /// </summary>
    [PHPProperty("num_bot_games_won")]
    public string BotGamesWon { get; init; } = aggregates.BotGamesWon.ToString();

    /// <summary>
    ///     Total games played including bot/practice games.
    /// </summary>
    [PHPProperty("total_games_played")]
    public int TotalGamesPlayedInt { get; init; } = aggregates.TotalGamesPlayedIncludingBots;

    /// <summary>
    ///     Total disconnections as an integer value.
    /// </summary>
    [PHPProperty("total_discos")]
    public int TotalDiscosInt { get; init; } = aggregates.TotalDisconnections;

    #endregion

    #region Aggregate Statistics By Game Mode

    /// <summary>
    ///     Total seconds played in public (unranked) matches.
    /// </summary>
    [PHPProperty("acc_secs")]
    public string PublicSecondsPlayed { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total public (unranked) matches played.
    /// </summary>
    [PHPProperty("acc_games_played")]
    public string PublicGamesPlayed { get; init; } = aggregates.PublicGamesPlayed.ToString();

    /// <summary>
    ///     Total disconnections in public (unranked) matches.
    /// </summary>
    [PHPProperty("acc_discos")]
    public string PublicDisconnections { get; init; } = aggregates.PublicDisconnections.ToString();

    /// <summary>
    ///     Total seconds played in ranked matches.
    /// </summary>
    [PHPProperty("rnk_secs")]
    public string RankedSecondsPlayed { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total ranked matches played.
    /// </summary>
    [PHPProperty("rnk_games_played")]
    public string RankedGamesPlayed { get; init; } = aggregates.RankedGamesPlayed.ToString();

    /// <summary>
    ///     Total disconnections in ranked matches.
    /// </summary>
    [PHPProperty("rnk_discos")]
    public string RankedDisconnections { get; init; } = aggregates.RankedDisconnections.ToString();

    /// <summary>
    ///     The total number of MidWars matches played.
    /// </summary>
    [PHPProperty("mid_games_played")]
    public string MidWarsGamesPlayed { get; init; } = aggregates.MidWarsGamesPlayed.ToString();

    /// <summary>
    ///     The total number of disconnections in MidWars matches.
    /// </summary>
    [PHPProperty("mid_discos")]
    public string MidWarsDisconnections { get; init; } = aggregates.MidWarsDisconnections.ToString();

    /// <summary>
    ///     The total number of RiftWars matches played.
    /// </summary>
    [PHPProperty("rift_games_played")]
    public string RiftWarsGamesPlayed { get; init; } = aggregates.RiftWarsGamesPlayed.ToString();

    /// <summary>
    ///     The total number of disconnections in RiftWars matches.
    /// </summary>
    [PHPProperty("rift_discos")]
    public string RiftWarsDisconnections { get; init; } = aggregates.RiftWarsDisconnections.ToString();

    #endregion

    #region Account Metadata

    /// <summary>
    ///     The date of last activity on the account.
    /// </summary>
    [PHPProperty("last_activity")]
    public string LastActivity { get; init; } = account.TimestampLastActive.ToString("MM/dd/yyyy");

    /// <summary>
    ///     The date the account was created.
    /// </summary>
    [PHPProperty("create_date")]
    public string CreateDate { get; init; } = account.TimestampCreated.ToString("MM/dd/yyyy");

    /// <summary>
    ///     The clan name, or an empty string if the account is not in a clan.
    /// </summary>
    [PHPProperty("name")]
    public string ClanName { get; init; } = account.Clan?.Name ?? string.Empty;

    /// <summary>
    ///     The clan rank (tier) name.
    /// </summary>
    [PHPProperty("rank")]
    public string ClanRank { get; init; } = account.ClanTierName;

    #endregion

    #region Favourite Heroes

    /// <summary>
    ///     The display name of the player's most-played hero.
    /// </summary>
    [PHPProperty("favHero1")]
    public string FavouriteHero1 { get; init; } = string.Empty; // TODO: Implement Data Tracking

    /// <summary>
    ///     The total time played (in seconds) with the player's most-played hero.
    /// </summary>
    [PHPProperty("favHero1Time")]
    public double FavouriteHero1Time { get; init; } = 0.0; // TODO: Implement Data Tracking

    /// <summary>
    ///     The internal identifier of the player's most-played hero (e.g., "Hero_Pyromancer").
    /// </summary>
    [PHPProperty("favHero1_2")]
    public string FavouriteHero1Identifier { get; init; } = string.Empty; // TODO: Implement Data Tracking

    /// <summary>
    ///     The display name of the player's second most-played hero.
    /// </summary>
    [PHPProperty("favHero2")]
    public string FavouriteHero2 { get; init; } = string.Empty; // TODO: Implement Data Tracking

    /// <summary>
    ///     The total time played (in seconds) with the player's second most-played hero.
    /// </summary>
    [PHPProperty("favHero2Time")]
    public double FavouriteHero2Time { get; init; } = 0.0; // TODO: Implement Data Tracking

    /// <summary>
    ///     The internal identifier of the player's second most-played hero.
    /// </summary>
    [PHPProperty("favHero2_2")]
    public string FavouriteHero2Identifier { get; init; } = string.Empty; // TODO: Implement Data Tracking

    /// <summary>
    ///     The display name of the player's third most-played hero.
    /// </summary>
    [PHPProperty("favHero3")]
    public string FavouriteHero3 { get; init; } = string.Empty; // TODO: Implement Data Tracking

    /// <summary>
    ///     The total time played (in seconds) with the player's third most-played hero.
    /// </summary>
    [PHPProperty("favHero3Time")]
    public double FavouriteHero3Time { get; init; } = 0.0; // TODO: Implement Data Tracking

    /// <summary>
    ///     The internal identifier of the player's third most-played hero.
    /// </summary>
    [PHPProperty("favHero3_2")]
    public string FavouriteHero3Identifier { get; init; } = string.Empty; // TODO: Implement Data Tracking

    /// <summary>
    ///     The display name of the player's fourth most-played hero.
    /// </summary>
    [PHPProperty("favHero4")]
    public string FavouriteHero4 { get; init; } = string.Empty; // TODO: Implement Data Tracking

    /// <summary>
    ///     The total time played (in seconds) with the player's fourth most-played hero.
    /// </summary>
    [PHPProperty("favHero4Time")]
    public double FavouriteHero4Time { get; init; } = 0.0; // TODO: Implement Data Tracking

    /// <summary>
    ///     The internal identifier of the player's fourth most-played hero.
    /// </summary>
    [PHPProperty("favHero4_2")]
    public string FavouriteHero4Identifier { get; init; } = string.Empty; // TODO: Implement Data Tracking

    /// <summary>
    ///     The display name of the player's fifth most-played hero.
    /// </summary>
    [PHPProperty("favHero5")]
    public string FavouriteHero5 { get; init; } = string.Empty; // TODO: Implement Data Tracking

    /// <summary>
    ///     The total time played (in seconds) with the player's fifth most-played hero.
    /// </summary>
    [PHPProperty("favHero5Time")]
    public double FavouriteHero5Time { get; init; } = 0.0; // TODO: Implement Data Tracking

    /// <summary>
    ///     The internal identifier of the player's fifth most-played hero.
    /// </summary>
    [PHPProperty("favHero5_2")]
    public string FavouriteHero5Identifier { get; init; } = string.Empty; // TODO: Implement Data Tracking

    #endregion

    #region Upgrades

    /// <summary>
    ///     The number of dice tokens (used for dice rolling features).
    /// </summary>
    [PHPProperty("dice_tokens")]
    public string DiceTokens { get; init; } = "100"; // TODO: Implement Data Tracking

    /// <summary>
    ///     The season level for cosmetic progression (determines seasonal rewards).
    /// </summary>
    [PHPProperty("season_level")]
    public int SeasonLevel { get; init; } = 100; // TODO: Implement Data Tracking

    /// <summary>
    ///     The custom icon slot ID for the account icon display.
    /// </summary>
    [PHPProperty("slot_id")]
    public string CustomIconSlotID { get; init; } = StatisticsResponseHelper.GetCustomIconSlotID(account);

    /// <summary>
    ///     The list of owned store item codes.
    /// </summary>
    [PHPProperty("my_upgrades")]
    public List<string> OwnedStoreItems { get; init; } = account.User.OwnedStoreItems;

    /// <summary>
    ///     The list of currently selected/equipped store item codes.
    /// </summary>
    [PHPProperty("selected_upgrades")]
    public List<string> SelectedStoreItems { get; init; } = account.SelectedStoreItems;

    /// <summary>
    ///     The number of game tokens (earnable in-game currency).
    /// </summary>
    [PHPProperty("game_tokens")]
    public int GameTokens { get; init; } = 100; // TODO: Implement Data Tracking

    /// <summary>
    ///     Detailed information about owned store items, including expiration and discount coupons.
    /// </summary>
    [PHPProperty("my_upgrades_info", isDiscriminatedUnion: true)]
    public Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> OwnedStoreItemsData { get; init; } = StatisticsResponseHelper.GetOwnedStoreItemsData(account);

    /// <summary>
    ///     The creep level for creep cosmetics progression.
    /// </summary>
    [PHPProperty("creep_level")]
    public int CreepLevel { get; init; } = 100; // TODO: Implement Data Tracking

    /// <summary>
    ///     The server timestamp in UTC seconds.
    /// </summary>
    [PHPProperty("timestamp")]
    public int ServerTimestamp { get; init; } = Convert.ToInt32(Math.Min(DateTimeOffset.UtcNow.ToUnixTimeSeconds(), Convert.ToInt64(int.MaxValue)));

    #endregion

    #region Match History

    /// <summary>
    ///     Comma-separated list of recent match IDs.
    /// </summary>
    [PHPProperty("matchIds")]
    public string MatchIDs { get; init; } = string.Empty; // TODO: Implement Data Tracking

    /// <summary>
    ///     Comma-separated list of recent match dates.
    /// </summary>
    [PHPProperty("matchDates")]
    public string MatchDates { get; init; } = string.Empty; // TODO: Implement Data Tracking

    #endregion

    #region Averages

    /// <summary>
    ///     The kill/death/assist ratio string (e.g., "2.5/1.0/3.0").
    /// </summary>
    [PHPProperty("k_d_a")]
    public string KDA { get; init; } = StatisticsResponseHelper.CalculateKDA(statistics);

    /// <summary>
    ///     The average game length in seconds.
    /// </summary>
    [PHPProperty("avgGameLength")]
    public double AverageGameLength { get; init; } = 0.0; // TODO: Implement Data Tracking

    /// <summary>
    ///     The average experience earned per minute.
    /// </summary>
    [PHPProperty("avgXP_min")]
    public double AverageExperiencePerMinute { get; init; } = 0.0; // TODO: Implement Data Tracking

    /// <summary>
    ///     The average denies per game.
    /// </summary>
    [PHPProperty("avgDenies")]
    public double AverageDenies { get; init; } = 0.0; // TODO: Implement Data Tracking

    /// <summary>
    ///     The average creep kills per game.
    /// </summary>
    [PHPProperty("avgCreepKills")]
    public double AverageCreepKills { get; init; } = 0.0; // TODO: Implement Data Tracking

    /// <summary>
    ///     The average neutral creep kills per game.
    /// </summary>
    [PHPProperty("avgNeutralKills")]
    public double AverageNeutralKills { get; init; } = 0.0; // TODO: Implement Data Tracking

    /// <summary>
    ///     The average actions (commands issued) per minute.
    /// </summary>
    [PHPProperty("avgActions_min")]
    public double AverageActionsPerMinute { get; init; } = 0.0; // TODO: Implement Data Tracking

    /// <summary>
    ///     The average wards placed per game.
    /// </summary>
    [PHPProperty("avgWardsUsed")]
    public double AverageWardsUsed { get; init; } = 0.0; // TODO: Implement Data Tracking

    #endregion

    #region Quest And Season

    /// <summary>
    ///     Quest statistics data (disabled feature).
    /// </summary>
    [PHPProperty("quest_stats")]
    public Dictionary<string, QuestStatistics> QuestStatistics { get; init; } = new () { { "error", new QuestStatistics() } };

    /// <summary>
    ///     Total campaign games played in previous seasons.
    /// </summary>
    [PHPProperty("prev_seasons_cam_games_played")]
    public int PreviousSeasonsCampaignGamesPlayed { get; init; } = 0; // TODO: Implement Season Tracking

    /// <summary>
    ///     Total disconnections in campaign games from previous seasons.
    /// </summary>
    [PHPProperty("prev_seasons_cam_discos")]
    public int PreviousSeasonsCampaignDisconnections { get; init; } = 0; // TODO: Implement Season Tracking

    /// <summary>
    ///     Total campaign games played in the latest (most recent completed) season.
    /// </summary>
    [PHPProperty("latest_season_cam_games_played")]
    public string LatestSeasonCampaignGamesPlayed { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total disconnections in campaign games from the latest season.
    /// </summary>
    [PHPProperty("latest_season_cam_discos")]
    public string LatestSeasonCampaignDisconnections { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total campaign games played in the current season.
    /// </summary>
    [PHPProperty("curr_season_cam_games_played")]
    public string CurrentSeasonCampaignGamesPlayed { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total disconnections in campaign games from the current season.
    /// </summary>
    [PHPProperty("curr_season_cam_discos")]
    public string CurrentSeasonCampaignDisconnections { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total campaign mode wins.
    /// </summary>
    [PHPProperty("cam_wins")]
    public string CampaignWins { get; init; } = aggregates.CampaignWins.ToString();

    /// <summary>
    ///     Total campaign mode losses.
    /// </summary>
    [PHPProperty("cam_losses")]
    public string CampaignLosses { get; init; } = aggregates.CampaignLosses.ToString();

    /// <summary>
    ///     Total casual campaign games played in previous seasons.
    /// </summary>
    [PHPProperty("prev_seasons_cam_cs_games_played")]
    public int PreviousSeasonsCasualGamesPlayed { get; init; } = 0; // TODO: Implement Season Tracking

    /// <summary>
    ///     Total disconnections in casual campaign games from previous seasons.
    /// </summary>
    [PHPProperty("prev_seasons_cam_cs_discos")]
    public int PreviousSeasonsCasualDisconnections { get; init; } = 0; // TODO: Implement Season Tracking

    /// <summary>
    ///     Total casual campaign games played in the latest season.
    /// </summary>
    [PHPProperty("latest_season_cam_cs_games_played")]
    public string LatestSeasonCasualGamesPlayed { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total disconnections in casual campaign games from the latest season.
    /// </summary>
    [PHPProperty("latest_season_cam_cs_discos")]
    public string LatestSeasonCasualDisconnections { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total casual campaign games played in the current season.
    /// </summary>
    [PHPProperty("curr_season_cam_cs_games_played")]
    public string CurrentSeasonCasualGamesPlayed { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total disconnections in casual campaign games from the current season.
    /// </summary>
    [PHPProperty("curr_season_cam_cs_discos")]
    public string CurrentSeasonCasualDisconnections { get; init; } = "0"; // TODO: Implement Data Tracking

    /// <summary>
    ///     Total casual campaign mode wins.
    /// </summary>
    [PHPProperty("cam_cs_wins")]
    public string CasualSeasonWins { get; init; } = aggregates.CampaignCasualWins.ToString();

    /// <summary>
    ///     Total casual campaign mode losses.
    /// </summary>
    [PHPProperty("cam_cs_losses")]
    public string CasualSeasonLosses { get; init; } = aggregates.CampaignCasualLosses.ToString();

    /// <summary>
    ///     Campaign reward data including medal levels and achievement status.
    /// </summary>
    [PHPProperty("con_reward")]
    public CampaignReward CampaignReward { get; init; } = new ();

    #endregion

    #region Miscellaneous

    /// <summary>
    ///     The minimum number of matches a free-to-play account must complete to become verified.
    /// </summary>
    [PHPProperty("vested_threshold")]
    public int VestedThreshold => 5;

    /// <summary>
    ///     Success indicator. Set to <see langword="true"/> on successful response.
    /// </summary>
    [PHPProperty(0)]
    public bool Zero => true;

    #endregion
}
