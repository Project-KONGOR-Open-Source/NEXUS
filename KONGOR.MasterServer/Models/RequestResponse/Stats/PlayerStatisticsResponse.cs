namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

/// <summary>
///     Response for show_stats with table="player" (public match statistics with acc_ prefix).
/// </summary>
public class PlayerStatisticsResponse(Account account, AccountStatistics statistics, AggregateStatistics aggregates)
{
    // TODO: Implement Match History Retrieval (matchIds, matchDates, And Related Averages Require Match History Data)
    // TODO: Implement Favourite Heroes Calculation (Requires Aggregating Hero Play Time Across All Matches)

    /// <summary>
    ///     The account's super ID (main account ID).
    /// </summary>
    [PHPProperty("super_id")]
    public string SuperID { get; init; } = account.User.Accounts.Single(account => account.IsMain).ID.ToString();

    /// <summary>
    ///     The account name with the clan tag (if applicable).
    /// </summary>
    [PHPProperty("nickname")]
    public string Nickname { get; init; } = account.NameWithClanTag;

    /// <summary>
    ///     The account standing (moderation status).
    /// </summary>
    [PHPProperty("standing")]
    public string Standing { get; init; } = "3"; // TODO: Implement Account Standing/Moderation Status

    /// <summary>
    ///     The account type.
    /// </summary>
    [PHPProperty("account_type")]
    public string AccountTypeValue { get; init; } = ((int) account.Type).ToString();

    /// <summary>
    ///     The unique account identifier.
    /// </summary>
    [PHPProperty("account_id")]
    public string AccountID { get; init; } = account.ID.ToString();

    /// <summary>
    ///     The account level.
    /// </summary>
    [PHPProperty("level")]
    public string Level { get; init; } = account.User.TotalLevel.ToString();

    /// <summary>
    ///     The current experience points towards the next level.
    /// </summary>
    [PHPProperty("level_exp")]
    public int LevelExperience { get; init; } = account.User.TotalExperience;

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

    #region Public Match Statistics (acc_ Prefix)

    /// <summary>
    ///     Number of public matches won.
    /// </summary>
    [PHPProperty("acc_wins")]
    public string Wins { get; init; } = statistics.MatchesWon.ToString();

    /// <summary>
    ///     Number of public matches lost.
    /// </summary>
    [PHPProperty("acc_losses")]
    public string Losses { get; init; } = statistics.MatchesLost.ToString();

    /// <summary>
    ///     Number of public matches played.
    /// </summary>
    [PHPProperty("acc_games_played")]
    public string GamesPlayed { get; init; } = statistics.MatchesPlayed.ToString();

    /// <summary>
    ///     Number of disconnections in public matches.
    /// </summary>
    [PHPProperty("acc_discos")]
    public string Disconnections { get; init; } = statistics.MatchesDisconnected.ToString();

    /// <summary>
    ///     Number of conceded matches.
    /// </summary>
    [PHPProperty("acc_concedes")]
    public string Concedes { get; init; } = statistics.MatchesConceded.ToString();

    /// <summary>
    ///     Number of times kicked from matches.
    /// </summary>
    [PHPProperty("acc_kicked")]
    public string Kicked { get; init; } = statistics.MatchesKicked.ToString();

    /// <summary>
    ///     Total seconds played.
    /// </summary>
    [PHPProperty("acc_secs")]
    public string SecondsPlayed { get; init; } = "0"; // TODO: Implement Seconds Tracking

    /// <summary>
    ///     Total hero kills.
    /// </summary>
    [PHPProperty("acc_herokills")]
    public string HeroKills { get; init; } = statistics.HeroKills.ToString();

    /// <summary>
    ///     Total hero assists.
    /// </summary>
    [PHPProperty("acc_heroassists")]
    public string HeroAssists { get; init; } = statistics.HeroAssists.ToString();

    /// <summary>
    ///     Total deaths.
    /// </summary>
    [PHPProperty("acc_deaths")]
    public string Deaths { get; init; } = statistics.HeroDeaths.ToString();

    /// <summary>
    ///     Total wards placed.
    /// </summary>
    [PHPProperty("acc_wards")]
    public string Wards { get; init; } = statistics.WardsPlaced.ToString();

    /// <summary>
    ///     Total smackdowns.
    /// </summary>
    [PHPProperty("acc_smackdown")]
    public string Smackdowns { get; init; } = statistics.Smackdowns.ToString();

    /// <summary>
    ///     Total number of times the player voted to concede.
    /// </summary>
    [PHPProperty("acc_concedevotes")]
    public string ConcedeVotes { get; init; } = "0"; // TODO: Implement Concede Vote Tracking

    /// <summary>
    ///     Total number of buybacks (respawning early by spending gold).
    /// </summary>
    [PHPProperty("acc_buybacks")]
    public string Buybacks { get; init; } = "0"; // TODO: Implement Buyback Tracking

    /// <summary>
    ///     Total damage dealt to enemy heroes.
    /// </summary>
    [PHPProperty("acc_herodmg")]
    public string HeroDamage { get; init; } = "0"; // TODO: Implement Hero Damage Tracking

    /// <summary>
    ///     Total experience gained from hero kills.
    /// </summary>
    [PHPProperty("acc_heroexp")]
    public string HeroExperience { get; init; } = "0"; // TODO: Implement Hero Experience Tracking

    /// <summary>
    ///     Total gold earned from hero kills.
    /// </summary>
    [PHPProperty("acc_herokillsgold")]
    public string HeroKillsGold { get; init; } = "0"; // TODO: Implement Hero Kill Gold Tracking

    /// <summary>
    ///     Total gold lost due to dying.
    /// </summary>
    [PHPProperty("acc_goldlost2death")]
    public string GoldLostToDeath { get; init; } = "0"; // TODO: Implement Gold Lost To Death Tracking

    /// <summary>
    ///     Total seconds spent dead (respawn timer).
    /// </summary>
    [PHPProperty("acc_secs_dead")]
    public string SecondsDead { get; init; } = "0"; // TODO: Implement Seconds Dead Tracking

    /// <summary>
    ///     Total lane creeps killed.
    /// </summary>
    [PHPProperty("acc_teamcreepkills")]
    public string TeamCreepKills { get; init; } = "0"; // TODO: Implement Lane Creep Kill Tracking

    /// <summary>
    ///     Total damage dealt to lane creeps.
    /// </summary>
    [PHPProperty("acc_teamcreepdmg")]
    public string TeamCreepDamage { get; init; } = "0"; // TODO: Implement Lane Creep Damage Tracking

    /// <summary>
    ///     Total experience gained from lane creeps.
    /// </summary>
    [PHPProperty("acc_teamcreepexp")]
    public string TeamCreepExperience { get; init; } = "0"; // TODO: Implement Lane Creep Experience Tracking

    /// <summary>
    ///     Total gold earned from lane creeps.
    /// </summary>
    [PHPProperty("acc_teamcreepgold")]
    public string TeamCreepGold { get; init; } = "0"; // TODO: Implement Lane Creep Gold Tracking

    /// <summary>
    ///     Total neutral creeps killed (jungle camps).
    /// </summary>
    [PHPProperty("acc_neutralcreepkills")]
    public string NeutralCreepKills { get; init; } = "0"; // TODO: Implement Neutral Creep Kill Tracking

    /// <summary>
    ///     Total damage dealt to neutral creeps.
    /// </summary>
    [PHPProperty("acc_neutralcreepdmg")]
    public string NeutralCreepDamage { get; init; } = "0"; // TODO: Implement Neutral Creep Damage Tracking

    /// <summary>
    ///     Total experience gained from neutral creeps.
    /// </summary>
    [PHPProperty("acc_neutralcreepexp")]
    public string NeutralCreepExperience { get; init; } = "0"; // TODO: Implement Neutral Creep Experience Tracking

    /// <summary>
    ///     Total gold earned from neutral creeps.
    /// </summary>
    [PHPProperty("acc_neutralcreepgold")]
    public string NeutralCreepGold { get; init; } = "0"; // TODO: Implement Neutral Creep Gold Tracking

    /// <summary>
    ///     Total damage dealt to buildings (towers, barracks, etc.).
    /// </summary>
    [PHPProperty("acc_bdmg")]
    public string BuildingDamage { get; init; } = "0"; // TODO: Implement Building Damage Tracking

    /// <summary>
    ///     Total experience gained from destroying buildings.
    /// </summary>
    [PHPProperty("acc_bdmgexp")]
    public string BuildingExperience { get; init; } = "0"; // TODO: Implement Building Experience Tracking

    /// <summary>
    ///     Total buildings destroyed (towers, barracks, etc.).
    /// </summary>
    [PHPProperty("acc_razed")]
    public string BuildingsRazed { get; init; } = "0"; // TODO: Implement Buildings Razed Tracking

    /// <summary>
    ///     Total gold earned from destroying buildings.
    /// </summary>
    [PHPProperty("acc_bgold")]
    public string BuildingGold { get; init; } = "0"; // TODO: Implement Building Gold Tracking

    /// <summary>
    ///     Total allied creeps denied (last-hitting own creeps to prevent enemy gold/XP).
    /// </summary>
    [PHPProperty("acc_denies")]
    public string Denies { get; init; } = "0"; // TODO: Implement Deny Tracking

    /// <summary>
    ///     Total experience denied to enemies through denies.
    /// </summary>
    [PHPProperty("acc_exp_denied")]
    public string ExperienceDenied { get; init; } = "0"; // TODO: Implement Experience Denied Tracking

    /// <summary>
    ///     Total gold earned.
    /// </summary>
    [PHPProperty("acc_gold")]
    public string Gold { get; init; } = "0"; // TODO: Implement Gold Earned Tracking

    /// <summary>
    ///     Total gold spent on items.
    /// </summary>
    [PHPProperty("acc_gold_spent")]
    public string GoldSpent { get; init; } = "0"; // TODO: Implement Gold Spent Tracking

    /// <summary>
    ///     Total experience earned.
    /// </summary>
    [PHPProperty("acc_exp")]
    public string Experience { get; init; } = "0"; // TODO: Implement Experience Earned Tracking

    /// <summary>
    ///     Total actions performed (clicks, ability uses, etc.).
    /// </summary>
    [PHPProperty("acc_actions")]
    public string Actions { get; init; } = "0"; // TODO: Implement Actions Tracking

    /// <summary>
    ///     Total consumable items used (potions, wards, etc.).
    /// </summary>
    [PHPProperty("acc_consumables")]
    public string Consumables { get; init; } = "0"; // TODO: Implement Consumables Tracking

    /// <summary>
    ///     Total Easy Mode matches played.
    /// </summary>
    [PHPProperty("acc_em_played")]
    public string EasyModeGamesPlayed { get; init; } = "0"; // TODO: Implement Easy Mode Tracking

    /// <summary>
    ///     Total time spent earning experience (seconds).
    /// </summary>
    [PHPProperty("acc_time_earning_exp")]
    public string TimeEarningExperience { get; init; } = "0"; // TODO: Implement Time Earning Experience Tracking

    /// <summary>
    ///     Total first blood kills achieved.
    /// </summary>
    [PHPProperty("acc_bloodlust")]
    public string Bloodlust { get; init; } = "0"; // TODO: Implement First Blood Tracking

    /// <summary>
    ///     Total double kills (two kills within a short time).
    /// </summary>
    [PHPProperty("acc_doublekill")]
    public string DoubleKills { get; init; } = "0"; // TODO: Implement Multi-Kill Tracking

    /// <summary>
    ///     Total triple kills (three kills within a short time).
    /// </summary>
    [PHPProperty("acc_triplekill")]
    public string TripleKills { get; init; } = "0"; // TODO: Implement Multi-Kill Tracking

    /// <summary>
    ///     Total quad kills (four kills within a short time).
    /// </summary>
    [PHPProperty("acc_quadkill")]
    public string QuadKills { get; init; } = "0"; // TODO: Implement Multi-Kill Tracking

    /// <summary>
    ///     Total annihilations (killing all five enemy heroes within a short time).
    /// </summary>
    [PHPProperty("acc_annihilation")]
    public string Annihilations { get; init; } = "0"; // TODO: Implement Multi-Kill Tracking

    /// <summary>
    ///     Total kill streaks of 3 (Serial Killer).
    /// </summary>
    [PHPProperty("acc_ks3")]
    public string KillStreak3 { get; init; } = "0"; // TODO: Implement Kill Streak Tracking

    /// <summary>
    ///     Total kill streaks of 4 (Ultimate Warrior).
    /// </summary>
    [PHPProperty("acc_ks4")]
    public string KillStreak4 { get; init; } = "0"; // TODO: Implement Kill Streak Tracking

    /// <summary>
    ///     Total kill streaks of 5 (Legendary).
    /// </summary>
    [PHPProperty("acc_ks5")]
    public string KillStreak5 { get; init; } = "0"; // TODO: Implement Kill Streak Tracking

    /// <summary>
    ///     Total kill streaks of 6 (Onslaught).
    /// </summary>
    [PHPProperty("acc_ks6")]
    public string KillStreak6 { get; init; } = "0"; // TODO: Implement Kill Streak Tracking

    /// <summary>
    ///     Total kill streaks of 7 (Savage Sick).
    /// </summary>
    [PHPProperty("acc_ks7")]
    public string KillStreak7 { get; init; } = "0"; // TODO: Implement Kill Streak Tracking

    /// <summary>
    ///     Total kill streaks of 8 (Dominating).
    /// </summary>
    [PHPProperty("acc_ks8")]
    public string KillStreak8 { get; init; } = "0"; // TODO: Implement Kill Streak Tracking

    /// <summary>
    ///     Total kill streaks of 9 (Champion).
    /// </summary>
    [PHPProperty("acc_ks9")]
    public string KillStreak9 { get; init; } = "0"; // TODO: Implement Kill Streak Tracking

    /// <summary>
    ///     Total kill streaks of 10 (Bloodbath).
    /// </summary>
    [PHPProperty("acc_ks10")]
    public string KillStreak10 { get; init; } = "0"; // TODO: Implement Kill Streak Tracking

    /// <summary>
    ///     Total kill streaks of 15 (Immortal).
    /// </summary>
    [PHPProperty("acc_ks15")]
    public string KillStreak15 { get; init; } = "0"; // TODO: Implement Kill Streak Tracking

    /// <summary>
    ///     Total times killed while on a killing spree (3+ streak ended by enemy).
    /// </summary>
    [PHPProperty("acc_humiliation")]
    public string Humiliations { get; init; } = "0"; // TODO: Implement Humiliation Tracking

    /// <summary>
    ///     Total times dying to the same enemy consecutively (making them your nemesis).
    /// </summary>
    [PHPProperty("acc_nemesis")]
    public string Nemesis { get; init; } = "0"; // TODO: Implement Nemesis Tracking

    /// <summary>
    ///     Total times killing your nemesis (payback kill).
    /// </summary>
    [PHPProperty("acc_retribution")]
    public string Retributions { get; init; } = "0"; // TODO: Implement Retribution Tracking

    #endregion

    #region Global Account Statistics

    /// <summary>
    ///     Total disconnections across all game modes.
    /// </summary>
    [PHPProperty("discos")]
    public string TotalDisconnections { get; init; } = aggregates.TotalDisconnections.ToString();

    /// <summary>
    ///     Total matches where disconnection was possible (used for leave percentage calculation).
    /// </summary>
    [PHPProperty("possible_discos")]
    public string PossibleDisconnections { get; init; } = "0"; // TODO: Implement Possible Disconnection Tracking

    /// <summary>
    ///     Total PvP matches played across all game modes (excluding bot matches).
    /// </summary>
    [PHPProperty("games_played")]
    public string TotalGamesPlayed { get; init; } = aggregates.TotalGamesPlayed.ToString();

    /// <summary>
    ///     Total bot/practice matches won.
    /// </summary>
    [PHPProperty("num_bot_games_won")]
    public string BotGamesWon { get; init; } = aggregates.BotGamesWon.ToString();

    /// <summary>
    ///     Total matches played including bot matches.
    /// </summary>
    [PHPProperty("total_games_played")]
    public int TotalGamesPlayedInt { get; init; } = aggregates.TotalGamesPlayedIncludingBots;

    /// <summary>
    ///     Total disconnections across all game modes (integer format).
    /// </summary>
    [PHPProperty("total_discos")]
    public int TotalDiscosInt { get; init; } = aggregates.TotalDisconnections;

    #endregion

    #region Aggregate Statistics By Game Mode

    /// <summary>
    ///     Total seconds played in ranked matchmaking matches.
    /// </summary>
    [PHPProperty("rnk_secs")]
    public string RankedSecondsPlayed { get; init; } = "0"; // TODO: Implement Seconds Played Tracking

    /// <summary>
    ///     Total ranked matchmaking matches played.
    /// </summary>
    [PHPProperty("rnk_games_played")]
    public string RankedGamesPlayed { get; init; } = aggregates.RankedGamesPlayed.ToString();

    /// <summary>
    ///     Total disconnections in ranked matchmaking matches.
    /// </summary>
    [PHPProperty("rnk_discos")]
    public string RankedDisconnections { get; init; } = aggregates.RankedDisconnections.ToString();

    /// <summary>
    ///     Total seconds played in casual matchmaking matches.
    /// </summary>
    [PHPProperty("cs_secs")]
    public string CasualSecondsPlayed { get; init; } = "0"; // TODO: Implement Seconds Played Tracking

    /// <summary>
    ///     Total casual matchmaking matches played.
    /// </summary>
    [PHPProperty("cs_games_played")]
    public string CasualGamesPlayed { get; init; } = aggregates.CasualGamesPlayed.ToString();

    /// <summary>
    ///     Total disconnections in casual matchmaking matches.
    /// </summary>
    [PHPProperty("cs_discos")]
    public string CasualDisconnections { get; init; } = aggregates.CasualDisconnections.ToString();

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

    #region Favourite Heroes

    /// <summary>
    ///     Most played hero name (lowercase, e.g. "pyromancer").
    /// </summary>
    [PHPProperty("favHero1")]
    public string FavouriteHero1 { get; init; } = string.Empty; // TODO: Implement Favourite Hero Calculation

    /// <summary>
    ///     Percentage of games played with the most played hero.
    /// </summary>
    [PHPProperty("favHero1Time")]
    public double FavouriteHero1Time { get; init; } = 0.0; // TODO: Implement Favourite Hero Calculation

    /// <summary>
    ///     Most played hero identifier (e.g. "Hero_Pyromancer").
    /// </summary>
    [PHPProperty("favHero1_2")]
    public string FavouriteHero1Identifier { get; init; } = string.Empty; // TODO: Implement Favourite Hero Calculation

    /// <summary>
    ///     Second most played hero name (lowercase).
    /// </summary>
    [PHPProperty("favHero2")]
    public string FavouriteHero2 { get; init; } = string.Empty; // TODO: Implement Favourite Hero Calculation

    /// <summary>
    ///     Percentage of games played with the second most played hero.
    /// </summary>
    [PHPProperty("favHero2Time")]
    public double FavouriteHero2Time { get; init; } = 0.0; // TODO: Implement Favourite Hero Calculation

    /// <summary>
    ///     Second most played hero identifier.
    /// </summary>
    [PHPProperty("favHero2_2")]
    public string FavouriteHero2Identifier { get; init; } = string.Empty; // TODO: Implement Favourite Hero Calculation

    /// <summary>
    ///     Third most played hero name (lowercase).
    /// </summary>
    [PHPProperty("favHero3")]
    public string FavouriteHero3 { get; init; } = string.Empty; // TODO: Implement Favourite Hero Calculation

    /// <summary>
    ///     Percentage of games played with the third most played hero.
    /// </summary>
    [PHPProperty("favHero3Time")]
    public double FavouriteHero3Time { get; init; } = 0.0; // TODO: Implement Favourite Hero Calculation

    /// <summary>
    ///     Third most played hero identifier.
    /// </summary>
    [PHPProperty("favHero3_2")]
    public string FavouriteHero3Identifier { get; init; } = string.Empty; // TODO: Implement Favourite Hero Calculation

    /// <summary>
    ///     Fourth most played hero name (lowercase).
    /// </summary>
    [PHPProperty("favHero4")]
    public string FavouriteHero4 { get; init; } = string.Empty; // TODO: Implement Favourite Hero Calculation

    /// <summary>
    ///     Percentage of games played with the fourth most played hero.
    /// </summary>
    [PHPProperty("favHero4Time")]
    public double FavouriteHero4Time { get; init; } = 0.0; // TODO: Implement Favourite Hero Calculation

    /// <summary>
    ///     Fourth most played hero identifier.
    /// </summary>
    [PHPProperty("favHero4_2")]
    public string FavouriteHero4Identifier { get; init; } = string.Empty; // TODO: Implement Favourite Hero Calculation

    /// <summary>
    ///     Fifth most played hero name (lowercase).
    /// </summary>
    [PHPProperty("favHero5")]
    public string FavouriteHero5 { get; init; } = string.Empty; // TODO: Implement Favourite Hero Calculation

    /// <summary>
    ///     Percentage of games played with the fifth most played hero.
    /// </summary>
    [PHPProperty("favHero5Time")]
    public double FavouriteHero5Time { get; init; } = 0.0; // TODO: Implement Favourite Hero Calculation

    /// <summary>
    ///     Fifth most played hero identifier.
    /// </summary>
    [PHPProperty("favHero5_2")]
    public string FavouriteHero5Identifier { get; init; } = string.Empty; // TODO: Implement Favourite Hero Calculation

    #endregion

    #region Upgrades

    /// <summary>
    ///     The index of the custom icon equipped, or "0" if no custom icon is equipped.
    /// </summary>
    [PHPProperty("slot_id")]
    public string CustomIconSlotID { get; init; } = StatisticsResponseHelper.GetCustomIconSlotID(account);

    /// <summary>
    ///     The collection of owned store items.
    /// </summary>
    [PHPProperty("my_upgrades")]
    public List<string> OwnedStoreItems { get; init; } = account.User.OwnedStoreItems;

    /// <summary>
    ///     The collection of selected/equipped store items.
    /// </summary>
    [PHPProperty("selected_upgrades")]
    public List<string> SelectedStoreItems { get; init; } = account.SelectedStoreItems;

    /// <summary>
    ///     Detailed information about owned store items including mastery boosts and discount coupons.
    /// </summary>
    [PHPProperty("my_upgrades_info", isDiscriminatedUnion: true)]
    public Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> OwnedStoreItemsData { get; init; } = StatisticsResponseHelper.GetOwnedStoreItemsData(account);

    #endregion

    #region Match History

    /// <summary>
    ///     Space-separated list of the last 20 match IDs.
    /// </summary>
    [PHPProperty("matchIds")]
    public string MatchIDs { get; init; } = string.Empty; // TODO: Implement Match History

    /// <summary>
    ///     Space-separated list of match dates corresponding to matchIds (format: MM/dd/yyyy).
    /// </summary>
    [PHPProperty("matchDates")]
    public string MatchDates { get; init; } = string.Empty; // TODO: Implement Match History

    #endregion

    #region Averages

    /// <summary>
    ///     Average kills/deaths/assists per game (format: "K/D/A").
    /// </summary>
    [PHPProperty("k_d_a")]
    public string KDA { get; init; } = StatisticsResponseHelper.CalculateKDA(statistics);

    /// <summary>
    ///     Average game duration in seconds.
    /// </summary>
    [PHPProperty("avgGameLength")]
    public double AverageGameLength { get; init; } = 0.0; // TODO: Implement Average Calculation

    /// <summary>
    ///     Average experience earned per minute.
    /// </summary>
    [PHPProperty("avgXP_min")]
    public double AverageExperiencePerMinute { get; init; } = 0.0; // TODO: Implement Average Calculation

    /// <summary>
    ///     Average creep denies per game.
    /// </summary>
    [PHPProperty("avgDenies")]
    public double AverageDenies { get; init; } = 0.0; // TODO: Implement Average Calculation

    /// <summary>
    ///     Average lane creep kills per game.
    /// </summary>
    [PHPProperty("avgCreepKills")]
    public double AverageCreepKills { get; init; } = 0.0; // TODO: Implement Average Calculation

    /// <summary>
    ///     Average neutral creep kills per game.
    /// </summary>
    [PHPProperty("avgNeutralKills")]
    public double AverageNeutralKills { get; init; } = 0.0; // TODO: Implement Average Calculation

    /// <summary>
    ///     Average actions per minute (APM).
    /// </summary>
    [PHPProperty("avgActions_min")]
    public double AverageActionsPerMinute { get; init; } = 0.0; // TODO: Implement Average Calculation

    /// <summary>
    ///     Average wards placed per game.
    /// </summary>
    [PHPProperty("avgWardsUsed")]
    public double AverageWardsUsed { get; init; } = 0.0; // TODO: Implement Average Calculation

    #endregion

    #region Miscellaneous

    /// <summary>
    ///     Tokens for the Kros Dice random ability draft that players can use while dead or in spawn in a Kros Mode match.
    ///     Only works in matches which have the "GAME_OPTION_SHUFFLE_ABILITIES" flag enabled, such as Rift Wars.
    /// </summary>
    [PHPProperty("dice_tokens")]
    public string DiceTokens { get; init; } = "100"; // TODO: Implement Dice Token Tracking

    /// <summary>
    ///     Tokens which grant temporary access to game modes (MidWars, Grimm's Crossing, etc.) for free-to-play players.
    ///     Alternative to permanent "Game Pass" or temporary "Game Access" products (e.g. "m.midwars.pass", "m.midwars.access").
    ///     Legacy accounts have full access to all game modes, and so do accounts which own the "m.allmodes.pass" store item.
    /// </summary>
    [PHPProperty("game_tokens")]
    public int GameTokens { get; init; } = 100; // TODO: Implement Game Token Tracking

    /// <summary>
    ///     Controls the visual appearance of tournament/seasonal buildings (towers, barracks, etc.) in matches.
    /// </summary>
    [PHPProperty("season_level")]
    public int SeasonLevel { get; init; } = 100; // TODO: Implement Season Level Tracking

    /// <summary>
    ///     Unused. May have been intended for creep cosmetics similar to season_level.
    /// </summary>
    [PHPProperty("creep_level")]
    public int CreepLevel { get; init; } = 100; // TODO: Implement Data Tracking

    /// <summary>
    ///     The server timestamp in UTC seconds.
    /// </summary>
    [PHPProperty("timestamp")]
    public int ServerTimestamp { get; init; } = Convert.ToInt32(Math.Min(DateTimeOffset.UtcNow.ToUnixTimeSeconds(), Convert.ToInt64(int.MaxValue)));

    /// <summary>
    ///     The minimum number of matches a free-to-play (trial) account must complete to become verified.
    ///     A verified account is considered to have full account privileges, and is no longer considered a restricted account.
    /// </summary>
    [PHPProperty("vested_threshold")]
    public int VestedThreshold => 5;

    /// <summary>
    ///     Success indicator. Set to <see langword="true"/> on successful response, <see langword="false"/> if an error occurs.
    /// </summary>
    [PHPProperty(0)]
    public bool Zero => true;

    #endregion
}
