namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class MatchStatsResponse
{
    /// <summary>
    ///     The amount of gold coins that the account owns.
    /// </summary>
    [PHPProperty("points")]
    public required string GoldCoins { get; init; }

    /// <summary>
    ///     The amount of silver coins that the account owns.
    /// </summary>
    [PHPProperty("mmpoints")]
    public required string SilverCoins { get; init; }

    /// <summary>
    ///     A collection containing the summary of the match.
    ///     The dictionary is keyed by match ID.
    /// </summary>
    [PHPProperty("match_summ")]
    public required Dictionary<int, MatchSummary> MatchSummary { get; init; }

    /// <summary>
    ///     A collection containing player statistics for the match.
    ///     The outer dictionary is keyed by match ID, the inner dictionary is keyed by player account IDs.
    ///     The requesting player's entry will be a <see cref="MatchStatsResponse.MatchPlayerStatisticsWithMatchPerformanceData"/> with additional match performance data.
    /// </summary>
    [PHPProperty("match_player_stats", isDiscriminatedUnion: true)]
    public required Dictionary<int, Dictionary<int, OneOf<MatchPlayerStatisticsWithMatchPerformanceData, MatchPlayerStatistics>>> MatchPlayerStatistics { get; init; }

    /// <summary>
    ///     A collection containing player inventories for the match.
    ///     The outer dictionary is keyed by match ID, the inner dictionary is keyed by player account IDs.
    /// </summary>
    [PHPProperty("inventory")]
    public required Dictionary<int, Dictionary<int, MatchPlayerInventory>> MatchPlayerInventories { get; init; }

    /// <summary>
    ///     Mastery details for the hero played in the match.
    /// </summary>
    [PHPProperty("match_mastery")]
    public required MatchMastery MatchMastery { get; init; }

    /// <summary>
    ///     Tokens for the Kros Dice random ability draft that players can use while dead or in spawn in a Kros Mode match.
    ///     Only works in matches which have the "GAME_OPTION_SHUFFLE_ABILITIES" flag enabled, such as Rift Wars.
    /// </summary>
    [PHPProperty("dice_tokens")]
    public int DiceTokens { get; init; } = 100;

    /// <summary>
    ///     Tokens which grant temporary access to game modes (MidWars, Grimm's Crossing, etc.) for free-to-play players.
    ///     Alternative to permanent "Game Pass" or temporary "Game Access" products (e.g. "m.midwars.pass", "m.midwars.access").
    ///     Legacy accounts have full access to all game modes, and so do accounts which own the "m.allmodes.pass" store item.
    /// </summary>
    [PHPProperty("game_tokens")]
    public int GameTokens { get; init; } = 100;

    /// <summary>
    ///     Controls the visual appearance of tournament/seasonal buildings (towers, barracks, etc.) in matches.
    ///     <code>
    ///         Level 0     -> default appearance
    ///         Level 01-09 -> tier 01 appearance
    ///         Level 10-24 -> tier 02 appearance
    ///         Level 25-49 -> tier 03 appearance
    ///         Level 50-74 -> tier 04 appearance
    ///         Level 75-99 -> tier 05 appearance
    ///         Level 100+  -> tier 06 appearance
    ///     </code>
    /// </summary>
    [PHPProperty("season_level")]
    public int SeasonLevel { get; init; } = 100;

    /// <summary>
    ///     Unused.
    ///     <br/>
    ///     May have been intended as a seasonal progression system similar to "season_level" but for creep cosmetics.
    ///     For the sake of consistency with "season_level", this property is set to "100", although it most likely has no effect.
    /// </summary>
    [PHPProperty("creep_level")]
    public int CreepLevel { get; init; } = 100;

    /// <summary>
    ///     The collection of owned store items.
    ///     <code>
    ///         Chat Name Colour       =>   "cc"
    ///         Chat Symbol            =>   "cs"
    ///         Account Icon           =>   "ai"
    ///         Alternative Avatar     =>   "aa"
    ///         Announcer Voice        =>   "av"
    ///         Taunt                  =>   "t"
    ///         Courier                =>   "c"
    ///         Hero                   =>   "h"
    ///         Early-Access Product   =>   "eap"
    ///         Status                 =>   "s"
    ///         Miscellaneous          =>   "m"
    ///         Ward                   =>   "w"
    ///         Enhancement            =>   "en"
    ///         Coupon                 =>   "cp"
    ///         Mastery                =>   "ma"
    ///         Creep                  =>   "cr"
    ///         Building               =>   "bu"
    ///         Taunt Badge            =>   "tb"
    ///         Teleportation Effect   =>   "te"
    ///         Selection Circle       =>   "sc"
    ///         Bundle                 =>   string.Empty
    ///     </code>
    /// </summary>
    [PHPProperty("my_upgrades")]
    public required List<string> OwnedStoreItems { get; init; }

    /// <summary>
    ///     Detailed information about owned store items including mastery boosts and discount coupons.
    /// </summary>
    [PHPProperty("my_upgrades_info", isDiscriminatedUnion: true)]
    public required Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> OwnedStoreItemsData { get; init; }

    /// <summary>
    ///     The collection of selected store items.
    /// </summary>
    [PHPProperty("selected_upgrades")]
    public required List<string> SelectedStoreItems { get; init; }

    /// <summary>
    ///     The index of the custom icon equipped, or "0" if no custom icon is equipped.
    /// </summary>
    [PHPProperty("slot_id")]
    public required string CustomIconSlotID { get; init; }

    /// <summary>
    ///     The server time (in UTC seconds).
    /// </summary>
    [PHPProperty("timestamp")]
    public long ServerTimestamp { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    /// <summary>
    ///     Used for the quest system, which has been disabled.
    ///     <br/>
    ///     While the quest system is disabled, this dictionary contains a single element with a key of "error".
    ///     The object which is the value of this element has the values of all its properties set to "0".
    /// </summary>
    [PHPProperty("quest_system")]
    public Dictionary<string, QuestSystem> QuestSystem { get; init; } = new () { { "error", new QuestSystem() } };

    /// <summary>
    ///     Unused.
    ///     <br/>
    ///     Statistics related to the "Event Codex" (otherwise known as "Ascension") seasonal system.
    /// </summary>
    [PHPProperty("season_system")]
    public SeasonSystem SeasonSystem { get; init; } = new ();

    /// <summary>
    ///     Statistics related to the Champions Of Newerth seasonal campaign.
    /// </summary>
    [PHPProperty("con_reward")]
    public CampaignReward CampaignReward { get; init; } = new ();

    /// <summary>
    ///     The minimum number of matches a free-to-play (trial) account must complete to become verified.
    ///     A verified account is considered to have full account privileges, and is no longer considered a restricted account.
    /// </summary>
    [PHPProperty("vested_threshold")]
    public int VestedThreshold => 5;

    /// <summary>
    ///     Unknown.
    ///     <br/>
    ///     Seems to be set to "true" on a successful response, or to "false" if an error occurs.
    /// </summary>
    [PHPProperty(0)]
    public bool Zero => true;
}

public class MatchSummary(MatchStatistics matchStatistics, List<PlayerStatistics> playerStatistics, MatchInformation matchInformation)
{
    /// <summary>
    ///     The unique identifier for the match.
    /// </summary>
    [PHPProperty("match_id")]
    public int MatchID { get; init; } = matchStatistics.MatchID;

    /// <summary>
    ///     The server ID where the match was hosted.
    /// </summary>
    [PHPProperty("server_id")]
    public int ServerID { get; init; } = matchStatistics.ServerID;

    /// <summary>
    ///     The server name where the match was hosted.
    /// </summary>
    [PHPProperty("name")]
    public string ServerName { get; init; } = matchInformation.ServerName;

    /// <summary>
    ///     The map on which the match was played (e.g. "caldavar", "midwars", "grimms_crossing").
    /// </summary>
    [PHPProperty("map")]
    public string Map { get; init; } = matchStatistics.Map;

    /// <summary>
    ///     The version of the map used in the match.
    /// </summary>
    [PHPProperty("map_version")]
    public string MapVersion { get; init; } = matchStatistics.MapVersion;

    /// <summary>
    ///     The duration of the match in seconds.
    /// </summary>
    [PHPProperty("time_played")]
    public int TimePlayed { get; init; } = matchStatistics.TimePlayed;

    /// <summary>
    ///     The host where the match replay file is stored.
    ///     This is typically "localhost" in development environments, or "kongor.net" in production environments.
    /// </summary>
    [PHPProperty("file_host")]
    public string FileHost { get; init; } = Environment.GetEnvironmentVariable("INFRASTRUCTURE_GATEWAY") ?? throw new NullReferenceException("Infrastructure Gateway Is NULL");

    /// <summary>
    ///     The size of the match replay file in bytes.
    /// </summary>
    [PHPProperty("file_size")]
    public int FileSize { get; init; } = Math.Max(0, matchStatistics.FileSize);

    /// <summary>
    ///     The filename of the match replay file.
    /// </summary>
    [PHPProperty("file_name")]
    public string FileName { get; init; } = matchStatistics.FileName;

    /// <summary>
    ///     The connection state or match state code.
    /// </summary>
    [PHPProperty("c_state")]
    public int ConnectionState { get; init; } = matchStatistics.ConnectionState;

    /// <summary>
    ///     The game client version used for the match.
    /// </summary>
    [PHPProperty("version")]
    public string Version { get; init; } = matchStatistics.Version;

    /// <summary>
    ///     The average Player Skill Rating (PSR) of all players in the match.
    /// </summary>
    [PHPProperty("avgpsr")]
    public int AveragePSR { get; init; } = matchStatistics.AveragePSR;

    /// <summary>
    ///     The match date, originally formatted as "M/D/YYYY" (e.g. "3/15/2024").
    /// </summary>
    [PHPProperty("date")]
    public string Date { get; init; } = DateTimeOffset.UtcNow.ToString("dd/MM/yyyy");

    /// <summary>
    ///     The match time, originally formatted in 12-hour format with AM/PM (e.g. "2:30:45 PM").
    /// </summary>
    [PHPProperty("time")]
    public string Time { get; init; } = DateTimeOffset.UtcNow.ToString("HH:mm:ss");

    /// <summary>
    ///     The match name.
    /// </summary>
    [PHPProperty("mname")]
    public string MatchName { get; init; } = matchInformation.MatchName;

    /// <summary>
    ///     The arranged match type that categorises how the match was created.
    ///     <code>
    ///         00 -> Public Match
    ///         01 -> Ranked Normal/Casual Matchmaking
    ///         02 -> Scheduled Tournament Match
    ///         03 -> Unscheduled League Match
    ///         04 -> MidWars Matchmaking
    ///         05 -> Bot Co-Op Matchmaking
    ///         06 -> Unranked Normal/Casual Matchmaking
    ///         07 -> RiftWars Matchmaking
    ///         08 -> Public Pre-Lobby
    ///         09 -> Custom Map Matchmaking
    ///         10 -> Ranked Season Normal/Casual Matchmaking
    ///     </code>
    /// </summary>
    [PHPProperty("class")]
    public int Class { get; init; } = (int) matchInformation.MatchType;

    /// <summary>
    ///     Whether the match was private (1) or public (0).
    /// </summary>
    [PHPProperty("private")]
    public int Private { get; init; } = IsPrivateMatch(playerStatistics);

    /// <summary>
    ///     Normal Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("nm")]
    public int NormalMode { get; init; } = matchInformation.MatchMode is PublicMatchMode.GAME_MODE_NORMAL ? 1 : 0;

    /// <summary>
    ///     Single Draft Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("sd")]
    public int SingleDraft { get; init; } = matchInformation.MatchMode is PublicMatchMode.GAME_MODE_SINGLE_DRAFT ? 1 : 0;

    /// <summary>
    ///     Random Draft Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("rd")]
    public int RandomDraft { get; init; } = matchInformation.MatchMode is PublicMatchMode.GAME_MODE_RANDOM_DRAFT ? 1 : 0;

    /// <summary>
    ///     Deathmatch Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("dm")]
    public int Deathmatch { get; init; } = matchInformation.MatchMode is PublicMatchMode.GAME_MODE_DEATHMATCH ? 1 : 0;

    /// <summary>
    ///     Banning Draft Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("bd")]
    public int BanningDraft { get; init; } = matchInformation.MatchMode is PublicMatchMode.GAME_MODE_BANNING_DRAFT ? 1 : 0;

    /// <summary>
    ///     Banning Pick Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("bp")]
    public int BanningPick { get; init; } = matchInformation.MatchMode is PublicMatchMode.GAME_MODE_BANNING_PICK ? 1 : 0;

    /// <summary>
    ///     All Random Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("ar")]
    public int AllRandom { get; init; } = matchInformation.MatchMode is PublicMatchMode.GAME_MODE_ALL_RANDOM ? 1 : 0;

    /// <summary>
    ///     Captains Draft Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("cd")]
    public int CaptainsDraft { get; init; } = matchInformation.MatchMode is PublicMatchMode.GAME_MODE_CAPTAINS_DRAFT ? 1 : 0;

    /// <summary>
    ///     Captains Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("cm")]
    public int CaptainsMode { get; init; } = matchInformation.MatchMode is PublicMatchMode.GAME_MODE_CAPTAINS_MODE ? 1 : 0;

    /// <summary>
    ///     Lock Pick Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("lp")]
    public int LockPick { get; init; } = matchInformation.MatchMode is PublicMatchMode.GAME_MODE_LOCKPICK ? 1 : 0;

    /// <summary>
    ///     Blind Ban Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("bb")]
    public int BlindBan { get; init; } = matchInformation.MatchMode is PublicMatchMode.GAME_MODE_BLIND_BAN ? 1 : 0;

    /// <summary>
    ///     Bot Match Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("bm")]
    public int BotMatch { get; init; } = matchInformation.MatchMode is PublicMatchMode.GAME_MODE_BOT_MATCH ? 1 : 0;

    /// <summary>
    ///     Kros (ability draft) Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("km")]
    public int KrosMode { get; init; } = matchInformation.MatchMode is PublicMatchMode.GAME_MODE_KROS_MODE ? 1 : 0;

    /// <summary>
    ///     Whether the match is part of an organized league system.
    ///     <code>
    ///         0 -> Regular Match (public, matchmaking, tournament, etc.)
    ///         1 -> League Match (organized competitive league play)
    ///     </code>
    ///     <remark>
    ///         League matches belong to a dedicated competitive league structure with player rosters, seasonal standings, and separate win/loss tracking.
    ///     </remark>
    /// </summary>
    [PHPProperty("league")]
    public int League { get; init; } = matchInformation.League;

    /// <summary>
    ///     The maximum number of players allowed in the match (typically 2, 6, or 10).
    /// </summary>
    [PHPProperty("max_players")]
    public int MaximumPlayersCount { get; init; } = matchInformation.MaximumPlayersCount;

    /// <summary>
    ///     Deprecated skill-based server filter that was used for matchmaking.
    ///     <code>
    ///         0 -> Noobs Only
    ///         1 -> Noobs Allowed
    ///         2 -> Professionals
    ///     </code>
    ///     This feature is no longer active and the field has no functional purpose.
    /// </summary>
    [PHPProperty("tier")]
    public int Tier { get; init; } = matchInformation.Tier;

    /// <summary>
    ///     No Repick option flag (1 = repicking disabled, 0 = repicking allowed).
    /// </summary>
    [PHPProperty("no_repick")]
    public int NoHeroRepick { get; init; } = matchInformation.Options.HasFlag(MatchOptions.NoHeroRepick) ? 1 : 0;

    /// <summary>
    ///     No Agility Heroes option flag (1 = agility heroes banned, 0 = allowed).
    /// </summary>
    [PHPProperty("no_agi")]
    public int NoAgilityHeroes { get; init; } = matchInformation.Options.HasFlag(MatchOptions.NoAgilityHeroes) ? 1 : 0;

    /// <summary>
    ///     Drop Items On Death option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("drp_itm")]
    public int DropItems { get; init; } = matchInformation.Options.HasFlag(MatchOptions.DropItems) ? 1 : 0;

    /// <summary>
    ///     No Respawn Timer option flag (1 = picking timer disabled, 0 = timer enabled).
    /// </summary>
    [PHPProperty("no_timer")]
    public int NoRespawnTimer { get; init; } = matchInformation.Options.HasFlag(MatchOptions.NoRespawnTimer) ? 1 : 0;

    /// <summary>
    ///     Reverse Hero Selection option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("rev_hs")]
    public int ReverseHeroSelection { get; init; } = matchInformation.Options.HasFlag(MatchOptions.ReverseHeroSelection) ? 1 : 0;

    /// <summary>
    ///     No Swap option flag (1 = hero swapping disabled, 0 = swapping allowed).
    /// </summary>
    [PHPProperty("no_swap")]
    public int NoHeroSwap { get; init; } = matchInformation.Options.HasFlag(MatchOptions.NoHeroSwap) ? 1 : 0;

    /// <summary>
    ///     No Intelligence Heroes option flag (1 = intelligence heroes banned, 0 = allowed).
    /// </summary>
    [PHPProperty("no_int")]
    public int NoIntelligenceHeroes { get; init; } = matchInformation.Options.HasFlag(MatchOptions.NoIntelligenceHeroes) ? 1 : 0;

    /// <summary>
    ///     Alternate Picking option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("alt_pick")]
    public int AlternateHeroPicking { get; init; } = matchInformation.Options.HasFlag(MatchOptions.AlternateHeroPicking) ? 1 : 0;

    /// <summary>
    ///     Ban Phase option flag (1 = ban phase enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("veto")]
    public int BanPhase { get; init; } = matchInformation.Options.HasFlag(MatchOptions.BanPhase) ? 1 : 0;

    /// <summary>
    ///     Shuffle Teams option flag (1 = shuffle teams enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("shuf")]
    public int ShuffleTeams { get; init; } = matchInformation.Options.HasFlag(MatchOptions.ShuffleTeams) ? 1 : 0;

    /// <summary>
    ///     No Strength Heroes option flag (1 = strength heroes banned, 0 = allowed).
    /// </summary>
    [PHPProperty("no_str")]
    public int NoStrengthHeroes { get; init; } = matchInformation.Options.HasFlag(MatchOptions.NoStrengthHeroes) ? 1 : 0;

    /// <summary>
    ///     No Power-Ups option flag (1 = power-ups/runes disabled, 0 = enabled).
    /// </summary>
    [PHPProperty("no_pups")]
    public int NoPowerUps { get; init; } = matchInformation.Options.HasFlag(MatchOptions.NoPowerUps) ? 1 : 0;

    /// <summary>
    ///     Duplicate Heroes option flag (1 = duplicate heroes allowed, 0 = each hero unique).
    /// </summary>
    [PHPProperty("dup_h")]
    public int DuplicateHeroes { get; init; } = matchInformation.Options.HasFlag(MatchOptions.DuplicateHeroes) ? 1 : 0;

    /// <summary>
    ///     All Pick Mode option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("ap")]
    public int AllPick { get; init; } = matchInformation.Options.HasFlag(MatchOptions.AllPick) ? 1 : 0;

    /// <summary>
    ///     Balanced Random Mode option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("br")]
    public int BalancedRandom { get; init; } = matchInformation.Options.HasFlag(MatchOptions.BalancedRandom) ? 1 : 0;

    /// <summary>
    ///     Easy Mode option flag (1 = easy mode enabled, 0 = normal difficulty).
    /// </summary>
    [PHPProperty("em")]
    public int EasyMode { get; init; } = matchInformation.Options.HasFlag(MatchOptions.EasyMode) ? 1 : 0;

    /// <summary>
    ///     Casual Mode option flag (1 = casual mode enabled, 0 = normal mode).
    /// </summary>
    [PHPProperty("cas")]
    public int CasualMode { get; init; } = matchInformation.Options.HasFlag(MatchOptions.CasualMode) ? 1 : 0;

    /// <summary>
    ///     Reverse Selection option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PHPProperty("rs")]
    public int ReverseSelection { get; init; } = matchInformation.Options.HasFlag(MatchOptions.ReverseSelection) ? 1 : 0;

    /// <summary>
    ///     No Leaver option flag (1 = no leaver penalty applied, 0 = leaver penalties enabled).
    /// </summary>
    [PHPProperty("nl")]
    public int NoLeavers { get; init; } = matchInformation.Options.HasFlag(MatchOptions.NoLeavers) ? 1 : 0;

    /// <summary>
    ///     Official Match flag (1 = official tournament match, 0 = unofficial).
    /// </summary>
    [PHPProperty("officl")]
    public int Official { get; init; } = matchInformation.Options.HasFlag(MatchOptions.Official) ? 1 : 0;

    /// <summary>
    ///     No Statistics option flag (1 = match stats not recorded, 0 = stats recorded).
    /// </summary>
    [PHPProperty("no_stats")]
    public int NoStatistics { get; init; } = matchInformation.Options.HasFlag(MatchOptions.NoStatistics) ? 1 : 0;

    /// <summary>
    ///     Auto Balanced option flag (1 = teams automatically balanced, 0 = manual teams).
    /// </summary>
    [PHPProperty("ab")]
    public int AutoBalanced { get; init; } = matchInformation.Options.HasFlag(MatchOptions.AutoBalanced) ? 1 : 0;

    /// <summary>
    ///     Hardcore Mode option flag (1 = hardcore difficulty enabled, 0 = normal).
    /// </summary>
    [PHPProperty("hardcore")]
    public int Hardcore { get; init; } = matchInformation.Options.HasFlag(MatchOptions.Hardcore) ? 1 : 0;

    /// <summary>
    ///     Development Heroes option flag (1 = development/unreleased heroes allowed, 0 = only released heroes).
    /// </summary>
    [PHPProperty("dev_heroes")]
    public int DevelopmentHeroes { get; init; } = matchInformation.Options.HasFlag(MatchOptions.DevelopmentHeroes) ? 1 : 0;

    /// <summary>
    ///     Verified Only option flag (1 = only verified accounts allowed, 0 = all accounts allowed).
    /// </summary>
    [PHPProperty("verified_only")]
    public int VerifiedOnly { get; init; } = matchInformation.Options.HasFlag(MatchOptions.VerifiedOnly) ? 1 : 0;

    /// <summary>
    ///     Gated option flag (1 = gated/restricted match, 0 = open match).
    /// </summary>
    [PHPProperty("gated")]
    public int Gated { get; init; } = matchInformation.Options.HasFlag(MatchOptions.Gated) ? 1 : 0;

    /// <summary>
    ///     Blitz Mode option flag (1 = rapid fire mode enabled, 0 = normal ability cooldowns).
    /// </summary>
    [PHPProperty("rapidfire")]
    public int BlitzMode { get; init; } = matchInformation.Options.HasFlag(MatchOptions.BlitzMode) ? 1 : 0;

    /// <summary>
    ///     The UNIX timestamp (in seconds) when the match started.
    /// </summary>
    [PHPProperty("timestamp")]
    public int Timestamp { get; init; } = Convert.ToInt32(Math.Min(matchStatistics.TimestampRecorded.ToUnixTimeSeconds(), Convert.ToInt64(Int32.MaxValue)));

    /// <summary>
    ///     The URL for the match replay file.
    /// </summary>
    [PHPProperty("url")]
    public string URL => $"http://{FileHost}/replays/{ServerID}/M{MatchID}.honreplay";

    /// <summary>
    ///     The size of the match replay file (human-readable format or bytes as string).
    /// </summary>
    [PHPProperty("size")]
    public int Size { get; init; } = Math.Max(0, matchStatistics.FileSize);

    /// <summary>
    ///     The directory path where the replay file is stored.
    /// </summary>
    [PHPProperty("dir")]
    public string Directory => $"/replays/{ServerID}";

    /// <summary>
    ///     The S3 download URL for the match replay file.
    /// </summary>
    [PHPProperty("s3_url")]
    public string S3URL => $"http://{FileHost}/replays/{ServerID}/M{MatchID}.honreplay";

    /// <summary>
    ///     The winning team ("1" for Legion, "2" for Hellbourne).
    ///     Determined by analysing player statistics from the match.
    /// </summary>
    [PHPProperty("winning_team")]
    public string WinningTeam { get; init; } = GetWinningTeam(playerStatistics).ToString();

    /// <summary>
    ///     The match mode.
    ///     <code>
    ///         "nm"  -> GAME_MODE_NORMAL
    ///         "rd"  -> GAME_MODE_RANDOM_DRAFT
    ///         "sd"  -> GAME_MODE_SINGLE_DRAFT
    ///         "dm"  -> GAME_MODE_DEATHMATCH
    ///         "bd"  -> GAME_MODE_BANNING_DRAFT
    ///         "cd"  -> GAME_MODE_CAPTAINS_DRAFT
    ///         "cm"  -> GAME_MODE_CAPTAINS_MODE
    ///         "bp"  -> GAME_MODE_BANNING_PICK
    ///         "ar"  -> GAME_MODE_ALL_RANDOM
    ///         "lp"  -> GAME_MODE_LOCKPICK
    ///         "bb"  -> GAME_MODE_BLIND_BAN
    ///         "bm"  -> GAME_MODE_BOT_MATCH
    ///         "km"  -> GAME_MODE_KROS_MODE
    ///         "fp"  -> GAME_MODE_FORCEPICK
    ///         "sp"  -> GAME_MODE_SOCCERPICK
    ///         "ss"  -> GAME_MODE_SOLO_SAME_HERO
    ///         "sm"  -> GAME_MODE_SOLO_DIFF_HERO
    ///         "cp"  -> GAME_MODE_COUNTER_PICK
    ///         "mwb" -> GAME_MODE_MIDWARS_BETA
    ///         "hb"  -> GAME_MODE_HEROBAN
    ///         "rb"  -> GAME_MODE_REBORN
    ///     </code>
    /// </summary>
    [PHPProperty("gamemode")]
    public string GameMode { get; init; } = matchStatistics.GameMode;

    /// <summary>
    ///     The account ID of the Most Valuable Player (MVP) in the match.
    /// </summary>
    [PHPProperty("mvp")]
    public string MVP { get; init; } = (matchStatistics.MVPAccountID ?? -1).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Most Annihilations" award, or "-1" if no player earned this award.
    /// </summary>
    [PHPProperty("awd_mann")]
    public string AwardMostAnnihilations { get; init; } = (matchStatistics.AwardMostAnnihilations ?? -1).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Most Quad Kills" award, or "-1" if no player earned this award.
    /// </summary>
    [PHPProperty("awd_mqk")]
    public string AwardMostQuadKills { get; init; } = (matchStatistics.AwardMostQuadKills ?? -1).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Longest Killing Spree" award, or "-1" if no player earned this award.
    /// </summary>
    [PHPProperty("awd_lgks")]
    public string AwardLongestKillingSpree { get; init; } = GetLongestKillingSpreeAwardRecipientID(playerStatistics).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Most Smackdowns" award, or "-1" if no player earned this award.
    /// </summary>
    [PHPProperty("awd_msd")]
    public string AwardMostSmackdowns { get; init; } = (matchStatistics.AwardMostSmackdowns ?? -1).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Most Kills" award, or "-1" if no player earned this award.
    /// </summary>
    [PHPProperty("awd_mkill")]
    public string AwardMostKills { get; init; } = (matchStatistics.AwardMostKills ?? -1).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Most Assists" award, or "-1" if no player earned this award.
    /// </summary>
    [PHPProperty("awd_masst")]
    public string AwardMostAssists { get; init; } = (matchStatistics.AwardMostAssists ?? -1).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Least Deaths" award, or "-1" if no player earned this award.
    /// </summary>
    [PHPProperty("awd_ledth")]
    public string AwardLeastDeaths { get; init; } = (matchStatistics.AwardLeastDeaths ?? -1).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Most Building Damage" award, or "-1" if no player earned this award.
    /// </summary>
    [PHPProperty("awd_mbdmg")]
    public string AwardMostBuildingDamage { get; init; } = (matchStatistics.AwardMostBuildingDamage ?? -1).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Most Wards" award, or "-1" if no player earned this award.
    /// </summary>
    [PHPProperty("awd_mwk")]
    public string AwardMostWards { get; init; } = GetMostWardsAwardRecipientID(playerStatistics).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Most Hero Damage Dealt" award, or "-1" if no player earned this award.
    /// </summary>
    [PHPProperty("awd_mhdd")]
    public string AwardMostHeroDamageDealt { get; init; } = (matchStatistics.AwardMostHeroDamageDealt ?? -1).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Highest Creep Score" award, or "-1" if no player earned this award.
    /// </summary>
    [PHPProperty("awd_hcs")]
    public string AwardHighestCreepScore { get; init; } = (matchStatistics.AwardHighestCreepScore ?? -1).ToString();

    private static int GetMostWardsAwardRecipientID(List<PlayerStatistics> playerStatistics)
    {
        if (playerStatistics.Where(player => player.WardsPlaced > 0).Any())
            return playerStatistics.OrderByDescending(player => player.WardsPlaced).ThenByDescending(player => player.Experience).First().ID;

        return -1;
    }

    private static int GetLongestKillingSpreeAwardRecipientID(List<PlayerStatistics> playerStatistics)
    {
        if (playerStatistics.Where(player => player.KillStreak15 > 0).Any())
            return playerStatistics.OrderByDescending(player => player.KillStreak15).ThenByDescending(player => player.Experience).First().ID;

        if (playerStatistics.Where(player => player.KillStreak10 > 0).Any())
            return playerStatistics.OrderByDescending(player => player.KillStreak10).ThenByDescending(player => player.Experience).First().ID;

        if (playerStatistics.Where(player => player.KillStreak09 > 0).Any())
            return playerStatistics.OrderByDescending(player => player.KillStreak09).ThenByDescending(player => player.Experience).First().ID;

        if (playerStatistics.Where(player => player.KillStreak08 > 0).Any())
            return playerStatistics.OrderByDescending(player => player.KillStreak08).ThenByDescending(player => player.Experience).First().ID;

        if (playerStatistics.Where(player => player.KillStreak07 > 0).Any())
            return playerStatistics.OrderByDescending(player => player.KillStreak07).ThenByDescending(player => player.Experience).First().ID;

        if (playerStatistics.Where(player => player.KillStreak06 > 0).Any())
            return playerStatistics.OrderByDescending(player => player.KillStreak06).ThenByDescending(player => player.Experience).First().ID;

        if (playerStatistics.Where(player => player.KillStreak05 > 0).Any())
            return playerStatistics.OrderByDescending(player => player.KillStreak05).ThenByDescending(player => player.Experience).First().ID;

        if (playerStatistics.Where(player => player.KillStreak04 > 0).Any())
            return playerStatistics.OrderByDescending(player => player.KillStreak04).ThenByDescending(player => player.Experience).First().ID;

        if (playerStatistics.Where(player => player.KillStreak03 > 0).Any())
            return playerStatistics.OrderByDescending(player => player.KillStreak03).ThenByDescending(player => player.Experience).First().ID;

        return -1;
    }

    private static int GetWinningTeam(List<PlayerStatistics> playerStatistics)
        => playerStatistics.Where(player => player.Loss is 0 && player.Win is 1).DistinctBy(player => player.Team).Single().Team;

    private static int IsPrivateMatch(List<PlayerStatistics> playerStatistics)
        => playerStatistics.DistinctBy(player => player.PublicMatch).Single().PublicMatch is 0 ? 1 : 0;
}

public class MatchMastery(string heroIdentifier, int currentMasteryExperience, int matchMasteryExperience, int bonusExperience)
{
    // TODO: Set Missing Properties Once Database Entities Are Available

    //public class MatchMastery(MasteryRewards rewards)
    //{
    //    MasteryExperienceMaximumLevelHeroesCount = rewards.MasteryMaxLevelHeroesCount;
    //    MasteryExperienceBoostProductCount = rewards.MasteryBoostsOwned;
    //    MasteryExperienceSuperBoostProductCount = rewards.MasterySuperBoostsOwned;
    //}

    /// <summary>
    ///     The identifier of the hero, in the format Hero_{Snake_Case_Name}.
    /// </summary>
    [PHPProperty("cli_name")]
    public string HeroIdentifier { get; init; } = heroIdentifier;

    /// <summary>
    ///     The hero's original mastery experience before the match.
    ///     This is the current mastery level progress persisted to the database.
    /// </summary>
    [PHPProperty("mastery_exp_original")]
    public int CurrentMasteryExperience { get; init; } = currentMasteryExperience;

    /// <summary>
    ///     The base mastery experience earned during the match.
    ///     Calculated from match duration, map, match type, and win/loss status.
    ///     Does not include bonuses or boosts.
    /// </summary>
    [PHPProperty("mastery_exp_match")]
    public int MatchMasteryExperience { get; init; } = matchMasteryExperience;

    /// <summary>
    ///     Additional mastery experience bonus from map-specific multipliers.
    ///     Applied as a percentage multiplier to the base experience.
    /// </summary>
    [PHPProperty("mastery_exp_bonus")]
    public int MasteryExperienceBonus { get; init; } = 0;

    /// <summary>
    ///     The additional mastery experience gained from applying a regular mastery boost consumable.
    ///     Set to zero initially when match results are calculated.
    ///     Only populated with a non-zero value after the player applies a mastery boost product.
    /// </summary>
    [PHPProperty("mastery_exp_boost")]
    public int MasteryExperienceBoost { get; init; } = 0;

    /// <summary>
    ///     The additional mastery experience gained from applying a super mastery boost consumable.
    ///     Set to zero initially when match results are calculated.
    ///     Only populated with a non-zero value after the player applies a super mastery boost product.
    /// </summary>
    [PHPProperty("mastery_exp_super_boost")]
    public int MasteryExperienceSuperBoost { get; init; } = 0;

    /// <summary>
    ///     The number of heroes the account has reached maximum mastery level with.
    ///     Used to calculate the "max_heroes_addon" bonus multiplier.
    /// </summary>
    [PHPProperty("mastery_exp_heroes_count")]
    public required int MasteryExperienceMaximumLevelHeroesCount { get; init; }

    /// <summary>
    ///     Bonus mastery experience awarded based on the number of max-level heroes owned.
    ///     Maps to "mastery_maxlevel_addon" in "match_stats_v2.lua".
    /// </summary>
    [PHPProperty("mastery_exp_heroes_addon")]
    public int MasteryExperienceHeroesBonus { get; init; } = bonusExperience;

    /// <summary>
    ///     The potential experience that can be gained by using a regular mastery boost.
    ///     Displayed when hovering over the mastery boost button in the UI.
    /// </summary>
    [PHPProperty("mastery_exp_to_boost")]
    public int MasteryExperienceToBoost { get; init; } = (matchMasteryExperience + bonusExperience) * 2;

    /// <summary>
    ///     Special event bonus mastery experience granted during promotional periods.
    ///     Typically zero unless an admin-configured mastery experience event is active.
    /// </summary>
    [PHPProperty("mastery_exp_event")]
    public int MasteryExperienceEventBonus { get; init; } = 0;

    /// <summary>
    ///     Setting this value to FALSE disables using or purchasing regular mastery boosts.
    ///     Some use cases for FALSE would be: 1) the hero has reached the maximum mastery level, 2) a mastery experience boost has already been used, 3) the map/mode combination is not eligible for accumulating mastery experience.
    /// </summary>
    [PHPProperty("mastery_canboost")]
    public bool MasteryExperienceCanBoost { get; init; } = true;

    /// <summary>
    ///     Setting this value to FALSE disables using or purchasing super mastery boosts.
    ///     Some use cases for FALSE would be: 1) the hero has reached the maximum mastery level, 2) a mastery experience boost has already been used, 3) the map/mode combination is not eligible for accumulating mastery experience.
    /// </summary>
    [PHPProperty("mastery_super_canboost")]
    public bool MasteryExperienceCanSuperBoost { get; init; } = true;

    /// <summary>
    ///     The product ID for regular mastery boost purchases (typically 3609 for "m.Mastery Boost").
    ///     Used when the player clicks to purchase a mastery boost from the match rewards screen.
    /// </summary>
    [PHPProperty("mastery_boost_product_id")]
    public int MasteryExperienceBoostProductIdentifier { get; init; } = 3609; // m.Mastery Boost

    /// <summary>
    ///     The product ID for super mastery boost purchases (typically 4605 for "m.Super boost").
    ///     Referenced but not directly purchasable from the standard match rewards UI.
    /// </summary>
    [PHPProperty("mastery_super_boost_product_id")]
    public int MasteryExperienceSuperBoostProductIdentifier { get; init; } = 4605; // m.Super boost

    /// <summary>
    ///     The number of regular mastery boost products the player currently owns.
    ///     Retrieved from the account's owned upgrades/products list.
    /// </summary>
    [PHPProperty("mastery_boostnum")]
    public required int MasteryExperienceBoostProductCount { get; init; }

    /// <summary>
    ///     The number of super mastery boost products the player currently owns.
    ///     Retrieved from the account's owned upgrades/products list.
    /// </summary>
    [PHPProperty("mastery_super_boostnum")]
    public required int MasteryExperienceSuperBoostProductCount { get; init; }
}

public class MatchPlayerStatistics(MatchInformation matchInformation, Account account, PlayerStatistics playerStatistics, AccountStatistics currentMatchTypeStatistics, AccountStatistics publicMatchStatistics, AccountStatistics matchmakingStatistics)
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
    ///     The number of friendly creeps denied by the player (last-hitting friendly creeps to prevent opponents from gaining gold/experience).
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
    public SeasonProgress SeasonProgress { get; init; } = new (matchInformation, playerStatistics, matchmakingStatistics);
}

public class MatchPlayerStatisticsWithMatchPerformanceData(MatchInformation matchInformation, Account account, PlayerStatistics playerStatistics, AccountStatistics currentMatchTypeStatistics, AccountStatistics publicMatchStatistics, AccountStatistics matchmakingStatistics) : MatchPlayerStatistics(matchInformation, account, playerStatistics, currentMatchTypeStatistics, publicMatchStatistics, matchmakingStatistics)
{
    /// <summary>
    ///     The player's team Matchmaking Rating (MMR) before the match.
    /// </summary>
    [PHPProperty("perf_amm_team_rating")]
    public string MatchPerformanceTeamRatingBefore { get; init; } = (matchmakingStatistics.SkillRating - playerStatistics.RankedSkillRatingChange).ToString("F2");

    /// <summary>
    ///     The change in team Matchmaking Rating (MMR) from this match.
    /// </summary>
    [PHPProperty("perf_amm_team_rating_delta")]
    public string MatchPerformanceTeamRatingDelta { get; init; } = playerStatistics.RankedSkillRatingChange.ToString("F2");

    /// <summary>
    ///     Experience points earned based on match outcome (win or loss).
    /// </summary>
    [PHPProperty("perf_victory_exp")]
    public string MatchPerformanceVictoryExperience { get; init; } = (playerStatistics.Disconnected == 0 && playerStatistics.Win == 1 ? 30 * playerStatistics.Benefit : playerStatistics.Disconnected == 0 && playerStatistics.Loss == 1 ? 10 * playerStatistics.Benefit : 0).ToString();

    /// <summary>
    ///     Gold coins earned based on match outcome (win or loss).
    /// </summary>
    [PHPProperty("perf_victory_gc")]
    public string MatchPerformanceVictoryGoldCoins { get; init; } = (playerStatistics.Disconnected == 0 && playerStatistics.Win == 1 ? 10 * 1 * playerStatistics.Benefit : playerStatistics.Disconnected == 0 && playerStatistics.Loss == 1 ? 5 * 1 * playerStatistics.Benefit : 0).ToString();

    /// <summary>
    ///     Extra experience points earned for playing the first match of the day.
    /// </summary>
    [PHPProperty("perf_first_exp")]
    public string MatchPerformanceFirstMatchExperience { get; init; } = "0";

    /// <summary>
    ///     Extra gold coins earned for playing the first match of the day.
    /// </summary>
    [PHPProperty("perf_first_gc")]
    public string MatchPerformanceFirstMatchGoldCoins { get; init; } = "0";

    /// <summary>
    ///     Extra experience points earned for quick match bonus.
    /// </summary>
    [PHPProperty("perf_quick_exp")]
    public string MatchPerformanceQuickMatchExperience { get; init; } = "0";

    /// <summary>
    ///     Extra gold coins earned for quick match bonus.
    /// </summary>
    [PHPProperty("perf_quick_gc")]
    public string MatchPerformanceQuickMatchGoldCoins { get; init; } = "0";

    /// <summary>
    ///     Number of consecutive matches played.
    /// </summary>
    [PHPProperty("perf_consec_played")]
    public string MatchPerformanceConsecutiveMatchesPlayed { get; init; } = "0";

    /// <summary>
    ///     Extra experience points earned for consecutive match bonus.
    /// </summary>
    [PHPProperty("perf_consec_exp")]
    public string MatchPerformanceConsecutiveMatchExperience { get; init; } = "0";

    /// <summary>
    ///     Extra gold coins earned for consecutive match bonus.
    /// </summary>
    [PHPProperty("perf_consec_gc")]
    public string MatchPerformanceConsecutiveMatchGoldCoins { get; init; } = "0";

    /// <summary>
    ///     Experience points earned for annihilation awards.
    /// </summary>
    [PHPProperty("perf_annihilation_exp")]
    public string MatchPerformanceAnnihilationExperience { get; init; } = (playerStatistics.Disconnected == 0 ? 50 * playerStatistics.Benefit * playerStatistics.Annihilation : 0).ToString();

    /// <summary>
    ///     Gold coins earned for annihilation awards.
    /// </summary>
    [PHPProperty("perf_annihilation_gc")]
    public string MatchPerformanceAnnihilationGoldCoins { get; init; } = (playerStatistics.Disconnected == 0 ? 10 * 1 * playerStatistics.Benefit * playerStatistics.Annihilation : 0).ToString();

    /// <summary>
    ///     Experience points earned for first blood awards.
    /// </summary>
    [PHPProperty("perf_bloodlust_exp")]
    public string MatchPerformanceBloodlustExperience { get; init; } = (playerStatistics.Disconnected == 0 ? 10 * playerStatistics.Benefit * playerStatistics.FirstBlood : 0).ToString();

    /// <summary>
    ///     Gold coins earned for first blood awards.
    /// </summary>
    [PHPProperty("perf_bloodlust_gc")]
    public string MatchPerformanceBloodlustGoldCoins { get; init; } = (playerStatistics.Disconnected == 0 ? 5 * 1 * playerStatistics.Benefit * playerStatistics.FirstBlood : 0).ToString();

    /// <summary>
    ///     Experience points earned for immortal (15-kill streak) awards.
    /// </summary>
    [PHPProperty("perf_ks15_exp")]
    public string MatchPerformanceKillStreak15Experience { get; init; } = (playerStatistics.Disconnected == 0 ? 35 * playerStatistics.KillStreak15 : 0).ToString();

    /// <summary>
    ///     Gold coins earned for immortal (15-kill streak) awards.
    /// </summary>
    [PHPProperty("perf_ks15_gc")]
    public string MatchPerformanceKillStreak15GoldCoins { get; init; } = (playerStatistics.Disconnected == 0 ? 10 * 1 * playerStatistics.Benefit * playerStatistics.KillStreak15 : 0).ToString();

    /// <summary>
    ///     Extra gold coins earned for social group bonus.
    /// </summary>
    [PHPProperty("perf_social_bonus_gc")]
    public string MatchPerformanceSocialBonusGoldCoins { get; init; } = "0";

    /// <summary>
    ///     Total wins on the player's account after this match.
    /// </summary>
    [PHPProperty("perf_wins")]
    public string MatchPerformanceTotalWins { get; init; } = currentMatchTypeStatistics.MatchesWon.ToString();

    /// <summary>
    ///     Change in wins from this match.
    /// </summary>
    [PHPProperty("perf_wins_delta")]
    public string MatchPerformanceWinsDelta { get; init; } = playerStatistics.Win.ToString();

    /// <summary>
    ///     Extra gold coins earned for win milestone.
    /// </summary>
    [PHPProperty("perf_wins_gc")]
    public string MatchPerformanceWinsGoldCoins { get; init; } = "0";

    /// <summary>
    ///     Total hero kills on the player's account after this match.
    /// </summary>
    [PHPProperty("perf_herokills")]
    public string MatchPerformanceTotalHeroKills { get; init; } = currentMatchTypeStatistics.HeroKills.ToString();

    /// <summary>
    ///     Change in hero kills from this match.
    /// </summary>
    [PHPProperty("perf_herokills_delta")]
    public string MatchPerformanceHeroKillsDelta { get; init; } = playerStatistics.HeroKills.ToString();

    /// <summary>
    ///     Extra gold coins earned for hero kill milestone.
    /// </summary>
    [PHPProperty("perf_herokills_gc")]
    public string MatchPerformanceHeroKillsGoldCoins { get; init; } = "0";

    /// <summary>
    ///     Total hero assists on the player's account after this match.
    /// </summary>
    [PHPProperty("perf_heroassists")]
    public string MatchPerformanceTotalHeroAssists { get; init; } = currentMatchTypeStatistics.HeroAssists.ToString();

    /// <summary>
    ///     Change in hero assists from this match.
    /// </summary>
    [PHPProperty("perf_heroassists_delta")]
    public string MatchPerformanceHeroAssistsDelta { get; init; } = playerStatistics.HeroAssists.ToString();

    /// <summary>
    ///     Extra gold coins earned for hero assist milestone.
    /// </summary>
    [PHPProperty("perf_heroassists_gc")]
    public string MatchPerformanceHeroAssistsGoldCoins { get; init; } = "0";

    /// <summary>
    ///     Total wards placed on the player's account after this match.
    /// </summary>
    [PHPProperty("perf_wards")]
    public string MatchPerformanceTotalWards { get; init; } = currentMatchTypeStatistics.WardsPlaced.ToString();

    /// <summary>
    ///     Change in wards placed from this match.
    /// </summary>
    [PHPProperty("perf_wards_delta")]
    public string MatchPerformanceWardsDelta { get; init; } = playerStatistics.WardsPlaced.ToString();

    /// <summary>
    ///     Extra gold coins earned for ward placement milestone.
    /// </summary>
    [PHPProperty("perf_wards_gc")]
    public string MatchPerformanceWardsGoldCoins { get; init; } = "0";

    /// <summary>
    ///     Total smackdowns on the player's account after this match.
    /// </summary>
    [PHPProperty("perf_smackdown")]
    public string MatchPerformanceTotalSmackdown { get; init; } = currentMatchTypeStatistics.Smackdowns.ToString();

    /// <summary>
    ///     Change in smackdowns from this match.
    /// </summary>
    [PHPProperty("perf_smackdown_delta")]
    public string MatchPerformanceSmackdownDelta { get; init; } = playerStatistics.Smackdown.ToString();

    /// <summary>
    ///     Extra gold coins earned for smackdown milestone.
    /// </summary>
    [PHPProperty("perf_smackdown_gc")]
    public string MatchPerformanceSmackdownGoldCoins { get; init; } = "0";

    /// <summary>
    ///     Player level after this match.
    /// </summary>
    [PHPProperty("perf_level")]
    public string MatchPerformanceLevel { get; init; } = account.User.TotalLevel.ToString();

    /// <summary>
    ///     Experience points on the player's level progress bar.
    /// </summary>
    [PHPProperty("perf_level_exp")]
    public string MatchPerformanceLevelExperience { get; init; } = account.User.TotalExperience.ToString();

    /// <summary>
    ///     Change in level experience from this match.
    /// </summary>
    [PHPProperty("perf_level_delta")]
    public string MatchPerformanceLevelExperienceDelta { get; init; } = playerStatistics.Experience.ToString();

    /// <summary>
    ///     Extra gold coins earned for level milestone.
    /// </summary>
    [PHPProperty("perf_level_gc")]
    public string MatchPerformanceLevelGoldCoins { get; init; } = "0";

    /// <summary>
    ///     Extra gold coins earned for being a legacy account owner.
    /// </summary>
    [PHPProperty("perf_legacy_gc")]
    public string MatchPerformanceLegacyGoldCoins { get; init; } = "0";

    /// <summary>
    ///     Silver coins multiplier for this match.
    /// </summary>
    [PHPProperty("perf_multiplier_mmpoints")]
    public string MatchPerformanceMultiplierSilverCoins { get; init; } = "0";

    /// <summary>
    ///     Experience multiplier for this match.
    /// </summary>
    [PHPProperty("perf_multiplier_exp")]
    public string MatchPerformanceMultiplierExperience { get; init; } = "0";
}

public class SeasonProgress(MatchInformation matchInformation, PlayerStatistics playerStatistics, AccountStatistics matchmakingStatistics)
{
    /// <summary>
    ///     The player's account ID.
    /// </summary>
    [PHPProperty("account_id")]
    public int AccountID { get; init; } = playerStatistics.AccountID;

    /// <summary>
    ///     The unique identifier for the match.
    /// </summary>
    [PHPProperty("match_id")]
    public int MatchID { get; init; } = playerStatistics.MatchID;

    /// <summary>
    ///     Whether the match was a casual ranked match ("1") or competitive ranked match ("0").
    /// </summary>
    [PHPProperty("is_casual")]
    public string IsCasual { get; init; } = matchInformation.IsCasual ? "1" : "0";

    /// <summary>
    ///     The player's Matchmaking Rating (MMR) before the match.
    /// </summary>
    [PHPProperty("mmr_before")]
    public string MMRBefore { get; init; } = (matchmakingStatistics.SkillRating - playerStatistics.RankedSkillRatingChange).ToString();

    /// <summary>
    ///     The player's Matchmaking Rating (MMR) after the match.
    /// </summary>
    [PHPProperty("mmr_after")]
    public string MMRAfter { get; init; } = matchmakingStatistics.SkillRating.ToString();

    /// <summary>
    ///     The player's medal rank before the match.
    ///     <code>
    ///         00      -> Unranked
    ///         01-05   -> Bronze   (V, IV, III, II, I)
    ///         06-10   -> Silver   (V, IV, III, II, I)
    ///         11-15   -> Gold     (V, IV, III, II, I)
    ///         16-20   -> Diamond  (V, IV, III, II, I)
    ///         21      -> Immortal
    ///     </code>
    /// </summary>
    [PHPProperty("medal_before")]
    public string MedalBefore { get; init; } = ((int) RankExtensions.GetRank(matchmakingStatistics.SkillRating - playerStatistics.RankedSkillRatingChange)).ToString();

    /// <summary>
    ///     The player's medal rank after the match.
    ///     Uses the same medal ranking system as "medal_before".
    /// </summary>
    [PHPProperty("medal_after")]
    public string MedalAfter { get; init; } = ((int) RankExtensions.GetRank(matchmakingStatistics.SkillRating)).ToString();

    /// <summary>
    ///     The seasonal campaign identifier.
    /// </summary>
    /// <remarks>
    ///     The last official season was Season 12.
    ///     Consumer-side changes are required to use a higher season number.
    /// </remarks>
    [PHPProperty("season")]
    public string Season { get; init; } = 12.ToString();

    /// <summary>
    ///     The number of placement matches the player has completed in the current season.
    ///     Players must complete placement matches before receiving their seasonal medal rank.
    /// </summary>
    /// <remarks>
    ///     The total expected number of placement matches is 6.
    /// </remarks>
    [PHPProperty("placement_matches")]
    public int PlacementMatches { get; init; } = 6;

    /// <summary>
    ///     The number of placement matches won by the player in the current season.
    /// </summary>
    [PHPProperty("placement_wins")]
    public string PlacementWins { get; init; } = matchmakingStatistics?.PlacementMatchesData ?? string.Empty;

    /// <summary>
    ///     The player's current ranking position on the Immortal leaderboard.
    ///     Only populated for Immortal rank players (medal index 20) with a ranking between 1 and 100.
    ///     Not present in the response for players below Immortal rank or outside the top 100.
    /// </summary>
    [PHPProperty("ranking")]
    public string? Ranking => RankExtensions.GetRank(matchmakingStatistics.SkillRating) is Rank.IMMORTAL ? 1.ToString() : null; // TODO: Implement Actual Leaderboard Ranking Retrieval
}

public class MatchPlayerInventory
{
    /// <summary>
    ///     The player's account ID.
    /// </summary>
    [PHPProperty("account_id")]
    public required int AccountID { get; init; }

    /// <summary>
    ///     The unique identifier for the match.
    /// </summary>
    [PHPProperty("match_id")]
    public required int MatchID { get; init; }

    /// <summary>
    ///     Item in slot 1 (Top Left), or NULL if the slot is empty.
    /// </summary>
    [PHPProperty("slot_1")]
    public required string? Slot1 { get; init; }

    /// <summary>
    ///     Item in slot 2 (Top Center), or NULL if the slot is empty.
    /// </summary>
    [PHPProperty("slot_2")]
    public required string? Slot2 { get; init; }

    /// <summary>
    ///     Item in slot 3 (Top Right), or NULL if the slot is empty.
    /// </summary>
    [PHPProperty("slot_3")]
    public required string? Slot3 { get; init; }

    /// <summary>
    ///     Item in slot 4 (Bottom Left), or NULL if the slot is empty.
    /// </summary>
    [PHPProperty("slot_4")]
    public required string? Slot4 { get; init; }

    /// <summary>
    ///     Item in slot 5 (Bottom Center), or NULL if the slot is empty.
    /// </summary>
    [PHPProperty("slot_5")]
    public required string? Slot5 { get; init; }

    /// <summary>
    ///     Item in slot 6 (Bottom Right), or NULL if the slot is empty.
    /// </summary>
    [PHPProperty("slot_6")]
    public required string? Slot6 { get; init; }
}

public class SeasonSystem
{
    /// <summary>
    ///     Number of diamonds earned/dropped from the match.
    ///     Calculated based on drop probability.
    /// </summary>
    [PHPProperty("drop_diamonds")]
    public int DropDiamonds { get; init; } = 0;

    /// <summary>
    ///     Current total diamonds the account has accumulated this season.
    /// </summary>
    [PHPProperty("cur_diamonds")]
    public int TotalDiamonds { get; init; } = 0;

    /// <summary>
    ///     Seasonal shop loot box prices and information.
    /// </summary>
    [PHPProperty("box_price")]
    public Dictionary<int, int> BoxPrice { get; init; } = [];
}

public class CampaignReward
{
    /// <summary>
    ///     Champions Of Newerth reward level before the match.
    ///     Set to "-2" if no previous match data exists.
    /// </summary>
    [PHPProperty("old_lvl")]
    public int PreviousCampaignLevel { get; init; } = 5;

    /// <summary>
    ///     Current Champions Of Newerth reward level after the match.
    ///     Maximum level is "6".
    /// </summary>
    [PHPProperty("curr_lvl")]
    public int CurrentCampaignLevel { get; init; } = 6;

    /// <summary>
    ///     Next Champions Of Newerth reward level to unlock.
    ///     Set to "0" when maximum level ("6") has been reached.
    /// </summary>
    [PHPProperty("next_lvl")]
    public int NextLevel { get; init; } = 0;

    /// <summary>
    ///     Minimum medal rank required to unlock the next Champions Of Newerth reward level.
    ///     <code>
    ///         Level 1 -> Medal 01 (Bronze V)
    ///         Level 2 -> Medal 06 (Silver V)
    ///         Level 3 -> Medal 11 (Gold V)
    ///         Level 4 -> Medal 15 (Diamond V)
    ///         Level 5 -> Medal 18 (Diamond II)
    ///         Level 6 -> Medal 20 (Immortal)
    ///     </code>
    ///     Set to "0" if the rank requirement is already met or if maximum level has been reached.
    /// </summary>
    [PHPProperty("require_rank")]
    public int RequireRank { get; init; } = 0;

    /// <summary>
    ///     Number of additional matches needed to accumulate enough reward points to reach the next Champions Of Newerth level.
    ///     Each level requires "12" reward points, earned from winning matches.
    ///     Set to "0" when maximum level has been reached.
    /// </summary>
    [PHPProperty("need_more_play")]
    public int NeedMorePlay { get; init; } = 0;

    /// <summary>
    ///     Progress percentage towards the next Champions Of Newerth reward level before the match.
    ///     Calculated as "reward_points" divided by 12, formatted as a decimal string (e.g. "0.75" for 75%).
    /// </summary>
    [PHPProperty("percentage_before")]
    public string PercentageBefore { get; init; } = "0.92";

    /// <summary>
    ///     Progress percentage towards the next Champions Of Newerth reward level after the match.
    ///     Calculated as "reward_points" divided by 12, formatted as a decimal string (e.g. "1.00" for 100%).
    /// </summary>
    [PHPProperty("percentage")]
    public string Percentage { get; init; } = "1.00";
}
