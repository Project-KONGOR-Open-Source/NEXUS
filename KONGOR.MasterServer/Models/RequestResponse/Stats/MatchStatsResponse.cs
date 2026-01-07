namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class MatchStatsResponse
{
    /// <summary>
    ///     The amount of gold coins that the account owns.
    /// </summary>
    [PhpProperty("points")]
    public required string GoldCoins { get; set; }

    /// <summary>
    ///     The amount of silver coins that the account owns.
    /// </summary>
    [PhpProperty("mmpoints")]
    public required string SilverCoins { get; set; }

    /// <summary>
    ///     A collection containing the summary of the match.
    ///     This is typically a single-element collection.
    /// </summary>
    [PhpProperty("match_summ")]
    public required List<MatchSummary> MatchSummary { get; set; }

    /// <summary>
    ///     A dictionary of player statistics for the match, keyed by the player's account ID.
    /// </summary>
    [PhpProperty("match_player_stats")]
    public required Dictionary<int, MatchPlayerStatistics> MatchPlayerStatistics { get; set; }

    /// <summary>
    ///     A dictionary of player inventories for the match, keyed by the player's account ID.
    /// </summary>
    [PhpProperty("inventory")]
    public required Dictionary<int, MatchPlayerInventory> MatchPlayerInventories { get; set; }

    /// <summary>
    ///     Mastery details for the hero played in the match.
    /// </summary>
    public required MatchMastery MatchMastery { get; set; }

    /// <summary>
    ///     Tokens for the Kros Dice random ability draft that players can use while dead or in spawn in a Kros Mode match.
    ///     Only works in matches which have the "GAME_OPTION_SHUFFLE_ABILITIES" flag enabled, such as Rift Wars.
    /// </summary>
    [PhpProperty("dice_tokens")]
    public int DiceTokens { get; set; } = 100;

    /// <summary>
    ///     Tokens which grant temporary access to game modes (MidWars, Grimm's Crossing, etc.) for free-to-play players.
    ///     Alternative to permanent "Game Pass" or temporary "Game Access" products (e.g. "m.midwars.pass", "m.midwars.access").
    ///     Legacy accounts have full access to all game modes, and so do accounts which own the "m.allmodes.pass" store item.
    /// </summary>
    [PhpProperty("game_tokens")]
    public int GameTokens { get; set; } = 100;

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
    [PhpProperty("season_level")]
    public int SeasonLevel { get; set; } = 100;

    /// <summary>
    ///     Unused.
    ///     <br/>
    ///     May have been intended as a seasonal progression system similar to "season_level" but for creep cosmetics.
    ///     For the sake of consistency with "season_level", this property is set to "100", although it most likely has no effect.
    /// </summary>
    [PhpProperty("creep_level")]
    public int CreepLevel { get; set; } = 100;

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

    [PhpProperty("my_upgrades")]
    public required List<string> OwnedStoreItems { get; set; }

    /// <summary>
    ///     Detailed information about owned store items including mastery boosts and discount coupons.
    /// </summary>
    [PhpProperty("my_upgrades_info")]
    public required Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> OwnedStoreItemsData { get; set; }

    /// <summary>
    ///     The collection of selected store items.
    /// </summary>
    [PhpProperty("selected_upgrades")]
    public required List<string> SelectedStoreItems { get; set; }

    /// <summary>
    ///     The index of the custom icon equipped, or "0" if no custom icon is equipped.
    /// </summary>
    [PhpProperty("slot_id")]
    public required string CustomIconSlotID { get; set; }

    /// <summary>
    ///     The server time (in UTC seconds).
    /// </summary>
    [PhpProperty("timestamp")]
    public long ServerTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    /// <summary>
    ///     Used for the quest system, which has been disabled.
    ///     <br/>
    ///     While the quest system is disabled, this dictionary contains a single element with a key of "error".
    ///     The object which is the value of this element has the values of all its properties set to "0".
    /// </summary>
    [PhpProperty("quest_system")]
    public Dictionary<string, QuestSystem> QuestSystem { get; set; } = new() { { "error", new QuestSystem() } };

    /// <summary>
    ///     Unused.
    ///     <br/>
    ///     Statistics related to the "Event Codex" (otherwise known as "Ascension") seasonal system.
    /// </summary>
    [PhpProperty("season_system")]
    public SeasonSystem SeasonSystem { get; set; } = new();

    /// <summary>
    ///     Statistics related to the Champions Of Newerth seasonal campaign.
    /// </summary>
    [PhpProperty("con_reward")]
    public required CampaignReward CampaignReward { get; set; } = new();

    /// <summary>
    ///     The minimum number of matches a free-to-play (trial) account must complete to become verified.
    ///     A verified account is considered to have full account privileges, and is no longer considered a restricted account.
    /// </summary>
    [PhpProperty("vested_threshold")]
    public int VestedThreshold => 5;

    /// <summary>
    ///     Unknown.
    ///     <br/>
    ///     Seems to be set to "true" on a successful response, or to "false" if an error occurs.
    /// </summary>
    [PhpProperty(0)]
    public bool Zero => true;
}

public class MatchSummary(MatchStatistics matchStatistics, List<PlayerStatistics> playerStatistics, MatchStartData matchStartData)
{
    /// <summary>
    ///     The unique identifier for the match.
    /// </summary>
    [PhpProperty("match_id")]
    public string MatchID { get; init; } = matchStatistics.ID.ToString();

    /// <summary>
    ///     The server ID where the match was hosted.
    /// </summary>
    [PhpProperty("server_id")]
    public int ServerID { get; init; } = matchStatistics.ServerID;

    /// <summary>
    ///     The server name where the match was hosted.
    /// </summary>
    [PhpProperty("name")]
    public string ServerName { get; init; } = matchStartData.ServerName;

    /// <summary>
    ///     The map on which the match was played (e.g. "caldavar", "midwars", "grimms_crossing").
    /// </summary>
    [PhpProperty("map")]
    public string Map { get; init; } = matchStatistics.Map;

    /// <summary>
    ///     The version of the map used in the match.
    /// </summary>
    [PhpProperty("map_version")]
    public string MapVersion { get; init; } = matchStatistics.MapVersion;

    /// <summary>
    ///     The duration of the match in seconds.
    /// </summary>
    [PhpProperty("time_played")]
    public string TimePlayed { get; init; } = matchStatistics.TimePlayed.ToString();

    /// <summary>
    ///     The host where the match replay file is stored.
    ///     This is typically "localhost" in development environments, or "kongor.net" in production environments.
    /// </summary>
    [PhpProperty("file_host")]
    public string FileHost { get; init; } = Environment.GetEnvironmentVariable("INFRASTRUCTURE_GATEWAY") ?? throw new NullReferenceException("Infrastructure Gateway Is NULL");

    /// <summary>
    ///     The size of the match replay file in bytes.
    /// </summary>
    [PhpProperty("file_size")]
    public int FileSize { get; init; } = Math.Min(0, matchStatistics.FileSize);

    /// <summary>
    ///     The filename of the match replay file.
    /// </summary>
    [PhpProperty("file_name")]
    public string FileName { get; init; } = matchStatistics.FileName;

    /// <summary>
    ///     The connection state or match state code.
    /// </summary>
    [PhpProperty("c_state")]
    public string ConnectionState { get; init; } = matchStatistics.ConnectionState.ToString();

    /// <summary>
    ///     The game client version used for the match.
    /// </summary>
    [PhpProperty("version")]
    public string Version { get; init; } = matchStatistics.Version;

    /// <summary>
    ///     The average Player Skill Rating (PSR) of all players in the match.
    /// </summary>
    [PhpProperty("avgpsr")]
    public string AveragePSR { get; init; } = matchStatistics.AveragePSR.ToString();

    /// <summary>
    ///     The match date, originally formatted as "M/D/YYYY" (e.g. "3/15/2024").
    /// </summary>
    [PhpProperty("date")]
    public string Date { get; init; } = DateTimeOffset.UtcNow.ToString("dd/MM/yyyy");

    /// <summary>
    ///     The match time, originally formatted in 12-hour format with AM/PM (e.g. "2:30:45 PM").
    /// </summary>
    [PhpProperty("time")]
    public string Time { get; init; } = DateTimeOffset.UtcNow.ToString("HH:mm:ss");

    /// <summary>
    ///     The match name or custom server name.
    /// </summary>
    [PhpProperty("mname")]
    public string MatchName { get; init; } = matchStartData.MatchName;

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
    [PhpProperty("class")]
    public int Class { get; init; } = (int) matchStartData.MatchType;

    /// <summary>
    ///     Whether the match was private (1) or public (0).
    /// </summary>
    [PhpProperty("private")]
    public string Private { get; init; } = IsPrivateMatch(playerStatistics).ToString();

    /// <summary>
    ///     Normal Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("nm")]
    public string NormalMode { get; init; } = matchStartData.MatchMode is "nm" ? "1" : "0";

    /// <summary>
    ///     Single Draft Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("sd")]
    public string SingleDraft { get; init; } = matchStartData.MatchMode is "sd" ? "1" : "0";

    /// <summary>
    ///     Random Draft Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("rd")]
    public string RandomDraft { get; init; } = matchStartData.MatchMode is "rd" ? "1" : "0";

    /// <summary>
    ///     Deathmatch Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("dm")]
    public string Deathmatch { get; init; } = matchStartData.MatchMode is "dm" ? "1" : "0";

    /// <summary>
    ///     Banning Draft Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("bd")]
    public string BanningDraft { get; init; } = matchStartData.MatchMode is "bd" ? "1" : "0";

    /// <summary>
    ///     Banning Pick Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("bp")]
    public string BanningPick { get; init; } = matchStartData.MatchMode is "bp" ? "1" : "0";

    /// <summary>
    ///     All Random Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("ar")]
    public string AllRandom { get; init; } = matchStartData.MatchMode is "ar" ? "1" : "0";

    /// <summary>
    ///     Captains Draft Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("cd")]
    public string CaptainsDraft { get; init; } = matchStartData.MatchMode is "cd" ? "1" : "0";

    /// <summary>
    ///     Captains Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("cm")]
    public string CaptainsMode { get; init; } = matchStartData.MatchMode is "cm" ? "1" : "0";

    /// <summary>
    ///     Lock Pick Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("lp")]
    public string LockPick { get; init; } = matchStartData.MatchMode is "lp" ? "1" : "0";

    /// <summary>
    ///     Blind Ban Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("bb")]
    public string BlindBan { get; init; } = matchStartData.MatchMode is "bb" ? "1" : "0";

    /// <summary>
    ///     Bot Match Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("bm")]
    public string BotMatch { get; init; } = matchStartData.MatchMode is "bm" ? "1" : "0";

    /// <summary>
    ///     Kros (ability draft) Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("km")]
    public string KrosMode { get; init; } = matchStartData.MatchMode is "km" ? "1" : "0";

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
    [PhpProperty("league")]
    public string League { get; init; } = matchStartData.League.ToString();

    /// <summary>
    ///     The maximum number of players allowed in the match (typically 2, 6, or 10).
    /// </summary>
    [PhpProperty("max_players")]
    public string MaximumPlayersCount { get; init; } = matchStartData.MaximumPlayersCount.ToString();

    /// <summary>
    ///     Deprecated skill-based server filter that was used for matchmaking.
    ///     <code>
    ///         0 -> Noobs Only
    ///         1 -> Noobs Allowed
    ///         2 -> Professionals
    ///     </code>
    ///     This feature is no longer active and the field has no functional purpose.
    /// </summary>
    [PhpProperty("tier")]
    public string Tier { get; init; } = matchStartData.Tier.ToString();

    /// <summary>
    ///     No Repick option flag (1 = repicking disabled, 0 = repicking allowed).
    /// </summary>
    [PhpProperty("no_repick")]
    public string NoHeroRepick { get; init; } = matchStartData.Options.HasFlag(MatchOptions.NoHeroRepick) ? "1" : "0";

    /// <summary>
    ///     No Agility Heroes option flag (1 = agility heroes banned, 0 = allowed).
    /// </summary>
    [PhpProperty("no_agi")]
    public string NoAgilityHeroes { get; init; } = matchStartData.Options.HasFlag(MatchOptions.NoAgilityHeroes) ? "1" : "0";

    /// <summary>
    ///     Drop Items On Death option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("drp_itm")]
    public string DropItems { get; init; } = matchStartData.Options.HasFlag(MatchOptions.DropItems) ? "1" : "0";

    /// <summary>
    ///     No Respawn Timer option flag (1 = picking timer disabled, 0 = timer enabled).
    /// </summary>
    [PhpProperty("no_timer")]
    public string NoRespawnTimer { get; init; } = matchStartData.Options.HasFlag(MatchOptions.NoRespawnTimer) ? "1" : "0";

    /// <summary>
    ///     Reverse Hero Selection option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("rev_hs")]
    public string ReverseHeroSelection { get; init; } = matchStartData.Options.HasFlag(MatchOptions.ReverseHeroSelection) ? "1" : "0";

    /// <summary>
    ///     No Swap option flag (1 = hero swapping disabled, 0 = swapping allowed).
    /// </summary>
    [PhpProperty("no_swap")]
    public string NoHeroSwap { get; init; } = matchStartData.Options.HasFlag(MatchOptions.NoHeroSwap) ? "1" : "0";

    /// <summary>
    ///     No Intelligence Heroes option flag (1 = intelligence heroes banned, 0 = allowed).
    /// </summary>
    [PhpProperty("no_int")]
    public string NoIntelligenceHeroes { get; init; } = matchStartData.Options.HasFlag(MatchOptions.NoIntelligenceHeroes) ? "1" : "0";

    /// <summary>
    ///     Alternate Picking option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("alt_pick")]
    public string AlternateHeroPicking { get; init; } = matchStartData.Options.HasFlag(MatchOptions.AlternateHeroPicking) ? "1" : "0";

    /// <summary>
    ///     Ban Phase option flag (1 = ban phase enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("veto")]
    public string BanPhase { get; init; } = matchStartData.Options.HasFlag(MatchOptions.BanPhase) ? "1" : "0";

    /// <summary>
    ///     Shuffle Teams option flag (1 = shuffle teams enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("shuf")]
    public string ShuffleTeams { get; init; } = matchStartData.Options.HasFlag(MatchOptions.ShuffleTeams) ? "1" : "0";

    /// <summary>
    ///     No Strength Heroes option flag (1 = strength heroes banned, 0 = allowed).
    /// </summary>
    [PhpProperty("no_str")]
    public string NoStrengthHeroes { get; init; } = matchStartData.Options.HasFlag(MatchOptions.NoStrengthHeroes) ? "1" : "0";

    /// <summary>
    ///     No Power-Ups option flag (1 = power-ups/runes disabled, 0 = enabled).
    /// </summary>
    [PhpProperty("no_pups")]
    public string NoPowerUps { get; init; } = matchStartData.Options.HasFlag(MatchOptions.NoPowerUps) ? "1" : "0";

    /// <summary>
    ///     Duplicate Heroes option flag (1 = duplicate heroes allowed, 0 = each hero unique).
    /// </summary>
    [PhpProperty("dup_h")]
    public string DuplicateHeroes { get; init; } = matchStartData.Options.HasFlag(MatchOptions.DuplicateHeroes) ? "1" : "0";

    /// <summary>
    ///     All Pick Mode option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("ap")]
    public string AllPick { get; init; } = matchStartData.Options.HasFlag(MatchOptions.AllPick) ? "1" : "0";

    /// <summary>
    ///     Balanced Random Mode option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("br")]
    public string BalancedRandom { get; init; } = matchStartData.Options.HasFlag(MatchOptions.BalancedRandom) ? "1" : "0";

    /// <summary>
    ///     Easy Mode option flag (1 = easy mode enabled, 0 = normal difficulty).
    /// </summary>
    [PhpProperty("em")]
    public string EasyMode { get; init; } = matchStartData.Options.HasFlag(MatchOptions.EasyMode) ? "1" : "0";

    /// <summary>
    ///     Casual Mode option flag (1 = casual mode enabled, 0 = normal mode).
    /// </summary>
    [PhpProperty("cas")]
    public string CasualMode { get; init; } = matchStartData.Options.HasFlag(MatchOptions.CasualMode) ? "1" : "0";

    /// <summary>
    ///     Reverse Selection option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("rs")]
    public string ReverseSelection { get; init; } = matchStartData.Options.HasFlag(MatchOptions.ReverseSelection) ? "1" : "0";

    /// <summary>
    ///     No Leaver option flag (1 = no leaver penalty applied, 0 = leaver penalties enabled).
    /// </summary>
    [PhpProperty("nl")]
    public string NoLeavers { get; init; } = matchStartData.Options.HasFlag(MatchOptions.NoLeavers) ? "1" : "0";

    /// <summary>
    ///     Official Match flag (1 = official tournament match, 0 = unofficial).
    /// </summary>
    [PhpProperty("officl")]
    public string Official { get; init; } = matchStartData.Options.HasFlag(MatchOptions.Official) ? "1" : "0";

    /// <summary>
    ///     No Statistics option flag (1 = match stats not recorded, 0 = stats recorded).
    /// </summary>
    [PhpProperty("no_stats")]
    public string NoStatistics { get; init; } = matchStartData.Options.HasFlag(MatchOptions.NoStatistics) ? "1" : "0";

    /// <summary>
    ///     Auto Balanced option flag (1 = teams automatically balanced, 0 = manual teams).
    /// </summary>
    [PhpProperty("ab")]
    public string AutoBalanced { get; init; } = matchStartData.Options.HasFlag(MatchOptions.AutoBalanced) ? "1" : "0";

    /// <summary>
    ///     Hardcore Mode option flag (1 = hardcore difficulty enabled, 0 = normal).
    /// </summary>
    [PhpProperty("hardcore")]
    public string Hardcore { get; init; } = matchStartData.Options.HasFlag(MatchOptions.Hardcore) ? "1" : "0";

    /// <summary>
    ///     Development Heroes option flag (1 = development/unreleased heroes allowed, 0 = only released heroes).
    /// </summary>
    [PhpProperty("dev_heroes")]
    public string DevelopmentHeroes { get; init; } = matchStartData.Options.HasFlag(MatchOptions.DevelopmentHeroes) ? "1" : "0";

    /// <summary>
    ///     Verified Only option flag (1 = only verified accounts allowed, 0 = all accounts allowed).
    /// </summary>
    [PhpProperty("verified_only")]
    public string VerifiedOnly { get; init; } = matchStartData.Options.HasFlag(MatchOptions.VerifiedOnly) ? "1" : "0";

    /// <summary>
    ///     Gated option flag (1 = gated/restricted match, 0 = open match).
    /// </summary>
    [PhpProperty("gated")]
    public string Gated { get; init; } = matchStartData.Options.HasFlag(MatchOptions.Gated) ? "1" : "0";

    /// <summary>
    ///     Blitz Mode option flag (1 = rapid fire mode enabled, 0 = normal ability cooldowns).
    /// </summary>
    [PhpProperty("rapidfire")]
    public string BlitzMode { get; init; } = matchStartData.Options.HasFlag(MatchOptions.BlitzMode) ? "1" : "0";

    /// <summary>
    ///     The UNIX timestamp (in seconds) when the match started.
    /// </summary>
    [PhpProperty("timestamp")]
    public string Timestamp { get; init; } = Convert.ToInt32(Math.Max(matchStatistics.TimestampRecorded.ToUnixTimeSeconds(), Convert.ToInt64(Int32.MaxValue))).ToString();

    /// <summary>
    ///     The URL for the match replay file.
    /// </summary>
    [PhpProperty("url")]
    public string URL => $"http://{FileHost}/replays/{ServerID}/M{MatchID}.honreplay";

    /// <summary>
    ///     The size of the match replay file (human-readable format or bytes as string).
    /// </summary>
    [PhpProperty("size")]
    public int Size { get; init; } = Math.Min(0, matchStatistics.FileSize);

    /// <summary>
    ///     The directory path where the replay file is stored.
    /// </summary>
    [PhpProperty("dir")]
    public string Directory => $"/replays/{ServerID}";

    /// <summary>
    ///     The S3 download URL for the match replay file.
    /// </summary>
    [PhpProperty("s3_url")]
    public string S3URL => $"http://{FileHost}/replays/{ServerID}/M{MatchID}.honreplay";

    /// <summary>
    ///     The winning team ("1" for Legion, "2" for Hellbourne).
    ///     Determined by analysing player statistics from the match.
    /// </summary>
    [PhpProperty("winning_team")]
    public string WinningTeam { get; init; } = GetWinningTeam(playerStatistics).ToString();

    /// <summary>
    ///     The game mode code derived from match options.
    ///     <code>
    ///         "ap"  -> All Pick (Normal Mode)
    ///         "sd"  -> Single Draft
    ///         "rd"  -> Random Draft
    ///         "bd"  -> Banning Draft
    ///         "bp"  -> Banning Pick
    ///         "cd"  -> Captains Draft
    ///         "cm"  -> Captains Mode
    ///         "br"  -> Balanced Random
    ///         "cp"  -> Campaign Mode
    ///         "sm"  -> Solo Diff Mode (1v1)
    ///         "ss"  -> Solo Same Mode (1v1)
    ///         "hb"  -> Hero Ban Mode
    ///         "mwb" -> MidWars Beta Mode
    ///     </code>
    /// </summary>
    [PhpProperty("gamemode")]
    public string GameMode { get; init; } = matchStatistics.GameMode;

    /// <summary>
    ///     The account ID of the Most Valuable Player (MVP) in the match.
    /// </summary>
    [PhpProperty("mvp")]
    public string MVP { get; init; } = (matchStatistics.MVPAccountID ?? -1).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Most Annihilations" award, or "-1" if no player earned this award.
    /// </summary>
    [PhpProperty("awd_mann")]
    public string AwardMostAnnihilations { get; init; } = (matchStatistics.AwardMostAnnihilations ?? -1).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Most Quad Kills" award, or "-1" if no player earned this award.
    /// </summary>
    [PhpProperty("awd_mqk")]
    public string AwardMostQuadKills { get; init; } = (matchStatistics.AwardMostQuadKills ?? -1).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Longest Killing Spree" award, or "-1" if no player earned this award.
    /// </summary>
    [PhpProperty("awd_lgks")]
    public string AwardLongestKillingSpree { get; init; } = GetLongestKillingSpreeAwardRecipientID(playerStatistics).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Most Smackdowns" award, or "-1" if no player earned this award.
    /// </summary>
    [PhpProperty("awd_msd")]
    public string AwardMostSmackdowns { get; init; } = (matchStatistics.AwardMostSmackdowns ?? -1).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Most Kills" award, or "-1" if no player earned this award.
    /// </summary>
    [PhpProperty("awd_mkill")]
    public string AwardMostKills { get; init; } = (matchStatistics.AwardMostKills ?? -1).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Most Assists" award, or "-1" if no player earned this award.
    /// </summary>
    [PhpProperty("awd_masst")]
    public string AwardMostAssists { get; init; } = (matchStatistics.AwardMostAssists ?? -1).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Least Deaths" award, or "-1" if no player earned this award.
    /// </summary>
    [PhpProperty("awd_ledth")]
    public string AwardLeastDeaths { get; init; } = (matchStatistics.AwardLeastDeaths ?? -1).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Most Building Damage" award, or "-1" if no player earned this award.
    /// </summary>
    [PhpProperty("awd_mbdmg")]
    public string AwardMostBuildingDamage { get; init; } = (matchStatistics.AwardMostBuildingDamage ?? -1).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Most Wards" award, or "-1" if no player earned this award.
    /// </summary>
    [PhpProperty("awd_mwk")]
    public string AwardMostWards { get; init; } = GetMostWardsAwardRecipientID(playerStatistics).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Most Hero Damage Dealt" award, or "-1" if no player earned this award.
    /// </summary>
    [PhpProperty("awd_mhdd")]
    public string AwardMostHeroDamageDealt { get; init; } = (matchStatistics.AwardMostHeroDamageDealt ?? -1).ToString();

    /// <summary>
    ///     The account ID of the player who earned the "Highest Creep Score" award, or "-1" if no player earned this award.
    /// </summary>
    [PhpProperty("awd_hcs")]
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
            return playerStatistics.OrderByDescending(player => player.KillStreak05).ThenByDescending(player => player.Experience).First().ID;

        if (playerStatistics.Where(player => player.KillStreak08 > 0).Any())
            return playerStatistics.OrderByDescending(player => player.KillStreak05).ThenByDescending(player => player.Experience).First().ID;

        if (playerStatistics.Where(player => player.KillStreak07 > 0).Any())
            return playerStatistics.OrderByDescending(player => player.KillStreak05).ThenByDescending(player => player.Experience).First().ID;

        if (playerStatistics.Where(player => player.KillStreak06 > 0).Any())
            return playerStatistics.OrderByDescending(player => player.KillStreak05).ThenByDescending(player => player.Experience).First().ID;

        if (playerStatistics.Where(player => player.KillStreak05 > 0).Any())
            return playerStatistics.OrderByDescending(player => player.KillStreak05).ThenByDescending(player => player.Experience).First().ID;

        if (playerStatistics.Where(player => player.KillStreak04 > 0).Any())
            return playerStatistics.OrderByDescending(player => player.KillStreak05).ThenByDescending(player => player.Experience).First().ID;

        if (playerStatistics.Where(player => player.KillStreak03 > 0).Any())
            return playerStatistics.OrderByDescending(player => player.KillStreak05).ThenByDescending(player => player.Experience).First().ID;

        return -1;
    }

    private static int GetWinningTeam(List<PlayerStatistics>? playerStatistics)
    {
        if (playerStatistics is null)
            return 0;

        List<int> winningTeams = playerStatistics
            .Where(player => player is { Loss: 0, Win: 1 })
            .Select(player => player.Team)
            .Distinct()
            .ToList();

        return winningTeams.Count == 1 ? winningTeams[0] : 0;
    }

    private static int IsPrivateMatch(List<PlayerStatistics> playerStatistics)
    {
        List<PlayerStatistics> publicStatuses = playerStatistics.DistinctBy(player => player.PublicMatch).ToList();

        // If all players have the same public match status, return it. Otherwise default to Public (0)
        return publicStatuses.Count == 1 ? (publicStatuses.Single().PublicMatch is 0 ? 1 : 0) : 0;
    }
}

public class MatchMastery
{
    public MatchMastery() { }

    public MatchMastery(string heroIdentifier, int currentMasteryExperience, int matchMasteryExperience, int bonusExperience)
    {
        HeroIdentifier = heroIdentifier;
        CurrentMasteryExperience = currentMasteryExperience;
        MatchMasteryExperience = matchMasteryExperience;
        MasteryExperienceBonus = bonusExperience;
        MasteryExperienceBoost = matchMasteryExperience + bonusExperience;
        MasteryExperienceHeroesBonus = bonusExperience;
        MasteryExperienceToBoost = (matchMasteryExperience + bonusExperience) * 2;
        MasteryExperienceCanBoost = true;
        MasteryExperienceCanSuperBoost = true;
        MasteryExperienceBoostProductIdentifier = 3609;
        MasteryExperienceSuperBoostProductIdentifier = 4605;
        MasteryExperienceSuperBoost = 0;
        MasteryExperienceMaximumLevelHeroesCount = 0;
        MasteryExperienceEventBonus = 0;
        MasteryExperienceBoostProductCount = 0;
        MasteryExperienceSuperBoostProductCount = 0;
    }
    // TODO: Set Missing Properties Once Database Entities Are Available

    //public class MatchMastery(MasteryRewards rewards)
    //{
    //    MasteryExperienceBoostProductCount = rewards.MasteryBoostsOwned;
    //    MasteryExperienceSuperBoostProductCount = rewards.MasterySuperBoostsOwned;
    //    MasteryExperienceHeroesCount = rewards.MasteryMaxLevelHeroesCount;
    //}

    /// <summary>
    ///     The identifier of the hero, in the format Hero_{Snake_Case_Name}.
    /// </summary>
    [PhpProperty("cli_name")]
    public required string HeroIdentifier { get; init; }

    /// <summary>
    ///     The hero's original mastery experience before the match.
    ///     This is the current mastery level progress persisted to the database.
    /// </summary>
    [PhpProperty("mastery_exp_original")]
    public required int CurrentMasteryExperience { get; init; }

    /// <summary>
    ///     The base mastery experience earned during the match.
    ///     Calculated from match duration, map, match type, and win/loss status.
    ///     Does not include bonuses or boosts.
    /// </summary>
    [PhpProperty("mastery_exp_match")]
    public required int MatchMasteryExperience { get; init; }

    /// <summary>
    ///     Additional mastery experience bonus from map-specific multipliers.
    ///     Applied as a percentage multiplier to the base experience.
    /// </summary>
    [PhpProperty("mastery_exp_bonus")]
    public required int MasteryExperienceBonus { get; init; }

    /// <summary>
    ///     The additional mastery experience gained from applying a regular mastery boost consumable.
    ///     Set to zero initially when match results are calculated.
    ///     Only populated with a non-zero value after the player applies a mastery boost product.
    /// </summary>
    [PhpProperty("mastery_exp_boost")]
    public required int MasteryExperienceBoost { get; init; }

    /// <summary>
    ///     The additional mastery experience gained from applying a super mastery boost consumable.
    ///     Set to zero initially when match results are calculated.
    ///     Only populated with a non-zero value after the player applies a super mastery boost product.
    /// </summary>
    [PhpProperty("mastery_exp_super_boost")]
    public required int MasteryExperienceSuperBoost { get; init; }

    /// <summary>
    ///     The number of heroes the account has reached maximum mastery level with.
    ///     Used to calculate the "max_heroes_addon" bonus multiplier.
    /// </summary>
    [PhpProperty("mastery_exp_heroes_count")]
    public required int MasteryExperienceMaximumLevelHeroesCount { get; init; }

    /// <summary>
    ///     Bonus mastery experience awarded based on the number of max-level heroes owned.
    ///     Maps to "mastery_maxlevel_addon" in "match_stats_v2.lua".
    /// </summary>
    [PhpProperty("mastery_exp_heroes_addon")]
    public required int MasteryExperienceHeroesBonus { get; init; }

    /// <summary>
    ///     The potential experience that can be gained by using a regular mastery boost.
    ///     Displayed when hovering over the mastery boost button in the UI.
    /// </summary>
    [PhpProperty("mastery_exp_to_boost")]
    public required int MasteryExperienceToBoost { get; init; }

    /// <summary>
    ///     Special event bonus mastery experience granted during promotional periods.
    ///     Typically zero unless an admin-configured mastery experience event is active.
    /// </summary>
    [PhpProperty("mastery_exp_event")]
    public required int MasteryExperienceEventBonus { get; init; }

    /// <summary>
    ///     Setting this value to FALSE disables using or purchasing regular mastery boosts.
    ///     Some use cases for FALSE would be: 1) the hero has reached the maximum mastery level, 2) a mastery experience boost has already been used, 3) the map/mode combination is not eligible for accumulating mastery experience.
    /// </summary>
    [PhpProperty("mastery_canboost")]
    public required bool MasteryExperienceCanBoost { get; set; } = true;

    /// <summary>
    ///     Setting this value to FALSE disables using or purchasing super mastery boosts.
    ///     Some use cases for FALSE would be: 1) the hero has reached the maximum mastery level, 2) a mastery experience boost has already been used, 3) the map/mode combination is not eligible for accumulating mastery experience.
    /// </summary>
    [PhpProperty("mastery_super_canboost")]
    public required bool MasteryExperienceCanSuperBoost { get; set; } = true;

    /// <summary>
    ///     The product ID for regular mastery boost purchases (typically 3609 for "m.Mastery Boost").
    ///     Used when the player clicks to purchase a mastery boost from the match rewards screen.
    /// </summary>
    [PhpProperty("mastery_boost_product_id")]
    public required int MasteryExperienceBoostProductIdentifier { get; init; } = 3609; // m.Mastery Boost

    /// <summary>
    ///     The product ID for super mastery boost purchases (typically 4605 for "m.Super boost").
    ///     Referenced but not directly purchasable from the standard match rewards UI.
    /// </summary>
    [PhpProperty("mastery_super_boost_product_id")]
    public required int MasteryExperienceSuperBoostProductIdentifier { get; init; } = 4605; // m.Super boost

    /// <summary>
    ///     The number of regular mastery boost products the player currently owns.
    ///     Retrieved from the account's owned upgrades/products list.
    /// </summary>
    [PhpProperty("mastery_boostnum")]
    public required int MasteryExperienceBoostProductCount { get; init; }

    /// <summary>
    ///     The number of super mastery boost products the player currently owns.
    ///     Retrieved from the account's owned upgrades/products list.
    /// </summary>
    [PhpProperty("mastery_super_boostnum")]
    public required int MasteryExperienceSuperBoostProductCount { get; init; }
}

public class MatchPlayerStatistics(Account account, PlayerStatistics playerStatistics)
{
    /// <summary>
    ///     The unique identifier for the match.
    /// </summary>
    [PhpProperty("match_id")]
    public int MatchID { get; init; } = playerStatistics.MatchID;

    /// <summary>
    ///     The player's account ID.
    /// </summary>
    [PhpProperty("account_id")]
    public int AccountID { get; init; } = playerStatistics.AccountID;

    /// <summary>
    ///     The account name (nickname) of the player.
    /// </summary>
    [PhpProperty("nickname")]
    public string AccountName { get; init; } = playerStatistics.AccountName;

    /// <summary>
    ///     The clan ID of the player's clan, or "0" if the player is not in a clan.
    /// </summary>
    [PhpProperty("clan_id")]
    public string ClanID { get; init; } = (playerStatistics.ClanID ?? 0).ToString();

    /// <summary>
    ///     The unique identifier of the hero played in the match.
    /// </summary>
    [PhpProperty("hero_id")]
    public string HeroID { get; set; } = (playerStatistics.HeroProductID ?? 0).ToString();

    /// <summary>
    ///     The lobby position of the player (0-9), indicating their slot in the pre-match lobby.
    /// </summary>
    [PhpProperty("position")]
    public string Position { get; set; } = playerStatistics.LobbyPosition.ToString();

    /// <summary>
    ///     The team the player was on ("1" for Legion, "2" for Hellbourne).
    /// </summary>
    [PhpProperty("team")]
    public string Team { get; init; } = playerStatistics.Team.ToString();

    /// <summary>
    ///     The final hero level reached by the player in the match (1-25).
    /// </summary>
    [PhpProperty("level")]
    public string Level { get; set; } = playerStatistics.HeroLevel.ToString();

    /// <summary>
    ///     The number of wins on the player's account before this match.
    /// </summary>
    [PhpProperty("wins")]
    public string Wins { get; set; } = "0"; // TODO: Implement Cumulative Stats

    /// <summary>
    ///     The number of losses on the player's account before this match.
    /// </summary>
    [PhpProperty("losses")]
    public string Losses { get; set; } = "0"; // TODO: Implement Cumulative Stats

    /// <summary>
    ///     The number of conceded matches on the player's account before this match.
    /// </summary>
    [PhpProperty("concedes")]
    public string Concedes { get; set; } = playerStatistics.Conceded.ToString();

    /// <summary>
    ///     The number of concede votes the player cast during the match.
    /// </summary>
    [PhpProperty("concedevotes")]
    public string ConcedeVotes { get; set; } = playerStatistics.ConcedeVotes.ToString();

    /// <summary>
    ///     The number of times the player bought back into the match after dying.
    /// </summary>
    [PhpProperty("buybacks")]
    public string Buybacks { get; set; } = playerStatistics.Buybacks.ToString();

    /// <summary>
    ///     The number of disconnections on the player's account before this match.
    /// </summary>
    [PhpProperty("discos")]
    public string Disconnections { get; set; } = playerStatistics.Disconnected.ToString();

    /// <summary>
    ///     The number of times the player was kicked from matches on their account before this match.
    /// </summary>
    [PhpProperty("kicked")]
    public string Kicked { get; set; } = playerStatistics.Kicked.ToString();

    /// <summary>
    ///     The player's Public Skill Rating (PSR) before this match.
    /// </summary>
    [PhpProperty("pub_skill")]
    public string PublicSkill { get; set; } = "1500"; // TODO: Implement Cumulative Stats

    /// <summary>
    ///     The number of public matches played on the player's account before this match.
    /// </summary>
    [PhpProperty("pub_count")]
    public string PublicCount { get; set; } = "0"; // TODO: Implement Cumulative Stats

    /// <summary>
    ///     The player's Automatic Matchmaking (AMM) solo rating before this match.
    /// </summary>
    [PhpProperty("amm_solo_rating")]
    public string AMMSoloRating { get; set; } = "0"; // TODO: Implement Cumulative Stats

    /// <summary>
    ///     The number of AMM solo matches played on the player's account before this match.
    /// </summary>
    [PhpProperty("amm_solo_count")]
    public string AMMSoloCount { get; set; } = "0"; // TODO: Implement Cumulative Stats

    /// <summary>
    ///     The player's Automatic Matchmaking (AMM) team rating before this match.
    /// </summary>
    [PhpProperty("amm_team_rating")]
    public string AMMTeamRating { get; set; } = "0"; // TODO: Implement Cumulative Stats

    /// <summary>
    ///     The number of AMM team matches played on the player's account before this match.
    /// </summary>
    [PhpProperty("amm_team_count")]
    public string AMMTeamCount { get; set; } = "0"; // TODO: Implement Cumulative Stats

    /// <summary>
    ///     The player's average score across all matches before this match.
    /// </summary>
    [PhpProperty("avg_score")]
    public string AverageScore { get; set; } = playerStatistics.Score.ToString();

    /// <summary>
    ///     The number of enemy hero kills achieved by the player in the match.
    /// </summary>
    [PhpProperty("herokills")]
    public string HeroKills { get; set; } = playerStatistics.HeroKills.ToString();

    /// <summary>
    ///     The total damage dealt to enemy heroes by the player in the match.
    /// </summary>
    [PhpProperty("herodmg")]
    public string HeroDamage { get; set; } = playerStatistics.HeroDamage.ToString();

    /// <summary>
    ///     The total experience gained from killing or assisting in killing enemy heroes.
    /// </summary>
    [PhpProperty("heroexp")]
    public string HeroExperience { get; set; } = playerStatistics.HeroExperience.ToString();

    /// <summary>
    ///     The total gold earned from killing or assisting in killing enemy heroes.
    /// </summary>
    [PhpProperty("herokillsgold")]
    public string HeroKillsGold { get; set; } = playerStatistics.GoldFromHeroKills.ToString();

    /// <summary>
    ///     The number of assists (participating in hero kills without landing the final blow) achieved by the player.
    /// </summary>
    [PhpProperty("heroassists")]
    public string HeroAssists { get; set; } = playerStatistics.HeroAssists.ToString();

    /// <summary>
    ///     The number of times the player died in the match.
    /// </summary>
    [PhpProperty("deaths")]
    public string Deaths { get; set; } = playerStatistics.HeroDeaths.ToString();

    /// <summary>
    ///     The total gold lost by the player due to deaths in the match.
    /// </summary>
    [PhpProperty("goldlost2death")]
    public string GoldLostToDeath { get; set; } = playerStatistics.GoldLostToDeath.ToString();

    /// <summary>
    ///     The total time in seconds the player spent dead (waiting to respawn) during the match.
    /// </summary>
    [PhpProperty("secs_dead")]
    public string SecondsDead { get; set; } = playerStatistics.SecondsDead.ToString();

    /// <summary>
    ///     The number of friendly team creeps killed by the player (last-hitting own creeps for gold/experience).
    /// </summary>
    [PhpProperty("teamcreepkills")]
    public string TeamCreepKills { get; set; } = playerStatistics.TeamCreepKills.ToString();

    /// <summary>
    ///     The total damage dealt to friendly team creeps by the player.
    /// </summary>
    [PhpProperty("teamcreepdmg")]
    public string TeamCreepDamage { get; set; } = playerStatistics.TeamCreepDamage.ToString();

    /// <summary>
    ///     The total experience gained from killing friendly team creeps.
    /// </summary>
    [PhpProperty("teamcreepexp")]
    public string TeamCreepExperience { get; set; } = playerStatistics.TeamCreepExperience.ToString();

    /// <summary>
    ///     The total gold earned from killing friendly team creeps.
    /// </summary>
    [PhpProperty("teamcreepgold")]
    public string TeamCreepGold { get; set; } = playerStatistics.TeamCreepGold.ToString();

    /// <summary>
    ///     The number of neutral creeps killed by the player (jungle creeps).
    /// </summary>
    [PhpProperty("neutralcreepkills")]
    public string NeutralCreepKills { get; set; } = playerStatistics.NeutralCreepKills.ToString();

    /// <summary>
    ///     The total damage dealt to neutral creeps by the player.
    /// </summary>
    [PhpProperty("neutralcreepdmg")]
    public string NeutralCreepDamage { get; set; } = playerStatistics.NeutralCreepDamage.ToString();

    /// <summary>
    ///     The total experience gained from killing neutral creeps.
    /// </summary>
    [PhpProperty("neutralcreepexp")]
    public string NeutralCreepExperience { get; set; } = playerStatistics.NeutralCreepExperience.ToString();

    /// <summary>
    ///     The total gold earned from killing neutral creeps.
    /// </summary>
    [PhpProperty("neutralcreepgold")]
    public string NeutralCreepGold { get; set; } = playerStatistics.NeutralCreepGold.ToString();

    /// <summary>
    ///     The total damage dealt to enemy buildings (towers, barracks, base structures) by the player.
    /// </summary>
    [PhpProperty("bdmg")]
    public string BuildingDamage { get; set; } = playerStatistics.BuildingDamage.ToString();

    /// <summary>
    ///     The total experience gained from damaging or destroying enemy buildings.
    /// </summary>
    [PhpProperty("bdmgexp")]
    public string BuildingExperience { get; set; } = playerStatistics.ExperienceFromBuildings.ToString();

    /// <summary>
    ///     The number of enemy buildings (towers, barracks) destroyed by the player.
    /// </summary>
    [PhpProperty("razed")]
    public string BuildingsRazed { get; set; } = playerStatistics.BuildingsRazed.ToString();

    /// <summary>
    ///     The total gold earned from damaging or destroying enemy buildings.
    /// </summary>
    [PhpProperty("bgold")]
    public string BuildingGold { get; set; } = playerStatistics.GoldFromBuildings.ToString();

    /// <summary>
    ///     The number of enemy creeps denied by the player (last-hitting enemy creeps to prevent opponents from gaining gold/experience).
    /// </summary>
    [PhpProperty("denies")]
    public string Denies { get; set; } = playerStatistics.Denies.ToString();

    /// <summary>
    ///     The total experience denied to opponents through denying enemy creeps.
    /// </summary>
    [PhpProperty("exp_denied")]
    public string ExperienceDenied { get; set; } = playerStatistics.ExperienceDenied.ToString();

    /// <summary>
    ///     The total gold accumulated by the player at the end of the match.
    /// </summary>
    [PhpProperty("gold")]
    public string Gold { get; set; } = playerStatistics.Gold.ToString();

    /// <summary>
    ///     The total gold spent by the player on items during the match.
    /// </summary>
    [PhpProperty("gold_spent")]
    public string GoldSpent { get; set; } = playerStatistics.GoldSpent.ToString();

    /// <summary>
    ///     The total experience gained by the player during the match.
    /// </summary>
    [PhpProperty("exp")]
    public string Experience { get; set; } = playerStatistics.Experience.ToString();

    /// <summary>
    ///     The total number of actions performed by the player during the match (clicks, commands, ability usage, etc.).
    /// </summary>
    [PhpProperty("actions")]
    public string Actions { get; set; } = playerStatistics.Actions.ToString();

    /// <summary>
    ///     The total time in seconds the player was actively playing in the match.
    /// </summary>
    [PhpProperty("secs")]
    public string Seconds { get; set; } = playerStatistics.SecondsPlayed.ToString();

    /// <summary>
    ///     The number of consumable items (potions, wards, teleport scrolls, etc.) purchased by the player.
    /// </summary>
    [PhpProperty("consumables")]
    public string Consumables { get; set; } = playerStatistics.ConsumablesPurchased.ToString();

    /// <summary>
    ///     The number of observer or sentry wards placed by the player during the match.
    /// </summary>
    [PhpProperty("wards")]
    public string Wards { get; set; } = playerStatistics.WardsPlaced.ToString();

    /// <summary>
    ///     The total time in seconds the player spent within experience range of dying enemy units.
    /// </summary>
    [PhpProperty("time_earning_exp")]
    public string TimeEarningExperience { get; set; } = playerStatistics.TimeEarningExperience.ToString();

    /// <summary>
    ///     The number of First Blood awards earned by the player (1 or 0).
    /// </summary>
    [PhpProperty("bloodlust")]
    public string FirstBlood { get; set; } = playerStatistics.FirstBlood.ToString();

    /// <summary>
    ///     The number of Double Kill awards earned by the player (killing 2 heroes in quick succession).
    /// </summary>
    [PhpProperty("doublekill")]
    public string DoubleKill { get; set; } = playerStatistics.DoubleKill.ToString();

    /// <summary>
    ///     The number of Triple Kill awards earned by the player (killing 3 heroes in quick succession).
    /// </summary>
    [PhpProperty("triplekill")]
    public string TripleKill { get; set; } = playerStatistics.TripleKill.ToString();

    /// <summary>
    ///     The number of Quad Kill awards earned by the player (killing 4 heroes in quick succession).
    /// </summary>
    [PhpProperty("quadkill")]
    public string QuadKill { get; set; } = playerStatistics.QuadKill.ToString();

    /// <summary>
    ///     The number of Annihilation awards earned by the player (killing all 5 enemy heroes in quick succession).
    /// </summary>
    [PhpProperty("annihilation")]
    public string Annihilation { get; set; } = playerStatistics.Annihilation.ToString();

    /// <summary>
    ///     The number of 3-kill streaks achieved by the player (killing 3 heroes without dying).
    /// </summary>
    [PhpProperty("ks3")]
    public string KillStreak3 { get; set; } = playerStatistics.KillStreak03.ToString();

    /// <summary>
    ///     The number of 4-kill streaks achieved by the player (killing 4 heroes without dying).
    /// </summary>
    [PhpProperty("ks4")]
    public string KillStreak4 { get; set; } = playerStatistics.KillStreak04.ToString();

    /// <summary>
    ///     The number of 5-kill streaks achieved by the player (killing 5 heroes without dying).
    /// </summary>
    [PhpProperty("ks5")]
    public string KillStreak5 { get; set; } = playerStatistics.KillStreak05.ToString();

    /// <summary>
    ///     The number of 6-kill streaks achieved by the player (killing 6 heroes without dying).
    /// </summary>
    [PhpProperty("ks6")]
    public string KillStreak6 { get; set; } = playerStatistics.KillStreak06.ToString();

    /// <summary>
    ///     The number of 7-kill streaks achieved by the player (killing 7 heroes without dying).
    /// </summary>
    [PhpProperty("ks7")]
    public string KillStreak7 { get; set; } = playerStatistics.KillStreak07.ToString();

    /// <summary>
    ///     The number of 8-kill streaks achieved by the player (killing 8 heroes without dying).
    /// </summary>
    [PhpProperty("ks8")]
    public string KillStreak8 { get; set; } = playerStatistics.KillStreak08.ToString();

    /// <summary>
    ///     The number of 9-kill streaks achieved by the player (killing 9 heroes without dying).
    /// </summary>
    [PhpProperty("ks9")]
    public string KillStreak9 { get; set; } = playerStatistics.KillStreak09.ToString();

    /// <summary>
    ///     The number of 10-kill streaks achieved by the player (killing 10 heroes without dying).
    /// </summary>
    [PhpProperty("ks10")]
    public string KillStreak10 { get; set; } = playerStatistics.KillStreak10.ToString();

    /// <summary>
    ///     The number of 15-kill streaks achieved by the player (killing 15 heroes without dying).
    /// </summary>
    [PhpProperty("ks15")]
    public string KillStreak15 { get; set; } = playerStatistics.KillStreak15.ToString();

    /// <summary>
    ///     The number of Smackdown awards earned by the player (ending an enemy's kill streak).
    /// </summary>
    [PhpProperty("smackdown")]
    public string Smackdown { get; set; } = playerStatistics.Smackdown.ToString();

    /// <summary>
    ///     The number of Humiliation awards earned by the player (killing an enemy hero who is significantly higher level).
    /// </summary>
    [PhpProperty("humiliation")]
    public string Humiliation { get; set; } = playerStatistics.Humiliation.ToString();

    /// <summary>
    ///     The number of Nemesis awards earned by the player (being killed repeatedly by the same enemy hero).
    /// </summary>
    [PhpProperty("nemesis")]
    public string Nemesis { get; set; } = playerStatistics.Nemesis.ToString();

    /// <summary>
    ///     The number of Retribution awards earned by the player (killing an enemy hero who has killed you repeatedly).
    /// </summary>
    [PhpProperty("retribution")]
    public string Retribution { get; set; } = playerStatistics.Retribution.ToString();

    /// <summary>
    ///     Whether the player used a token (game access token or dice token) during the match ("1" if used, "0" otherwise).
    /// </summary>
    [PhpProperty("used_token")]
    public string UsedToken { get; set; } = playerStatistics.UsedToken.ToString();

    /// <summary>
    ///     The hero identifier in the format Hero_{Snake_Case_Name} (e.g. "Hero_Andromeda", "Hero_Legionnaire").
    /// </summary>
    [PhpProperty("cli_name")]
    public string HeroIdentifier { get; set; } = "Hero_Legionnaire"; // TODO: Map Hero ID to Hero Name using Game Data Service

    /// <summary>
    ///     The clan tag of the player's clan, or empty string if the player is not in a clan.
    /// </summary>
    [PhpProperty("tag")]
    public string ClanTag { get; set; } = account.Clan?.Tag ?? string.Empty;

    /// <summary>
    ///     The alternative avatar name used by the player during the match, or empty string if using the default hero skin.
    /// </summary>
    [PhpProperty("alt_avatar_name")]
    public string AlternativeAvatarName { get; set; } = playerStatistics.AlternativeAvatarName ?? string.Empty;

    /// <summary>
    ///     Seasonal campaign progression information for the player in the match.
    /// </summary>
    [PhpProperty("campaign_info")]
    public SeasonProgress SeasonProgress { get; set; } = new()
    {
        AccountID = account.ID,
        MatchID = playerStatistics.MatchID,
        IsCasual = "0", // TODO: Determine from Match Mode
        MMRBefore = "1500", // Placeholder
        MMRAfter = "1500", // Placeholder
        MedalBefore = "06", // Placeholder
        MedalAfter = "06", // Placeholder
        Season = "12",
        PlacementMatches = 0,
        PlacementWins = "0"
    };
}

public class SeasonProgress
{
    /// <summary>
    ///     The player's account ID.
    /// </summary>
    [PhpProperty("account_id")]
    public required int AccountID { get; set; }

    /// <summary>
    ///     The unique identifier for the match.
    /// </summary>
    [PhpProperty("match_id")]
    public required int MatchID { get; set; }

    /// <summary>
    ///     Whether the match was a casual ranked match ("1") or competitive ranked match ("0").
    /// </summary>
    [PhpProperty("is_casual")]
    public required string IsCasual { get; set; }

    /// <summary>
    ///     The player's Matchmaking Rating (MMR) before the match.
    /// </summary>
    [PhpProperty("mmr_before")]
    public required string MMRBefore { get; set; }

    /// <summary>
    ///     The player's Matchmaking Rating (MMR) after the match.
    /// </summary>
    [PhpProperty("mmr_after")]
    public required string MMRAfter { get; set; }

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
    [PhpProperty("medal_before")]
    public required string MedalBefore { get; set; }

    /// <summary>
    ///     The player's medal rank after the match.
    ///     Uses the same medal ranking system as "medal_before".
    /// </summary>
    [PhpProperty("medal_after")]
    public required string MedalAfter { get; set; }

    /// <summary>
    ///     The seasonal campaign identifier.
    /// </summary>
    [PhpProperty("season")]
    public required string Season { get; set; }

    /// <summary>
    ///     The number of placement matches the player has completed in the current season.
    ///     Players must complete placement matches before receiving their seasonal medal rank.
    /// </summary>
    [PhpProperty("placement_matches")]
    public required int PlacementMatches { get; set; }

    /// <summary>
    ///     The number of placement matches won by the player in the current season.
    /// </summary>
    [PhpProperty("placement_wins")]
    public required string PlacementWins { get; set; }

    /// <summary>
    ///     The player's current ranking position on the Immortal leaderboard.
    ///     Only populated for Immortal rank players (medal 21) with a ranking between 1 and 100.
    ///     Not present in the response for players below Immortal rank or outside the top 100.
    /// </summary>
    [PhpProperty("ranking")]
    public string? Ranking { get; set; }
}

public class MatchPlayerInventory
{
    /// <summary>
    ///     The player's account ID.
    /// </summary>
    [PhpProperty("account_id")]
    public required int AccountID { get; set; }

    /// <summary>
    ///     The unique identifier for the match.
    /// </summary>
    [PhpProperty("match_id")]
    public required int MatchID { get; set; }

    /// <summary>
    ///     Item in slot 1 (Top Left).
    /// </summary>
    [PhpProperty("slot_1")]
    public required string Slot1 { get; set; }

    /// <summary>
    ///     Item in slot 2 (Top Center).
    /// </summary>
    [PhpProperty("slot_2")]
    public required string Slot2 { get; set; }

    /// <summary>
    ///     Item in slot 3 (Top Right).
    /// </summary>
    [PhpProperty("slot_3")]
    public required string Slot3 { get; set; }

    /// <summary>
    ///     Item in slot 4 (Bottom Left).
    /// </summary>
    [PhpProperty("slot_4")]
    public required string Slot4 { get; set; }

    /// <summary>
    ///     Item in slot 5 (Bottom Center).
    /// </summary>
    [PhpProperty("slot_5")]
    public required string Slot5 { get; set; }

    /// <summary>
    ///     Item in slot 6 (Bottom Right).
    /// </summary>
    [PhpProperty("slot_6")]
    public required string Slot6 { get; set; }
}

public class SeasonSystem
{
    /// <summary>
    ///     Number of diamonds earned/dropped from the match.
    ///     Calculated based on drop probability.
    /// </summary>
    [PhpProperty("drop_diamonds")]
    public int DropDiamonds { get; set; } = 0;

    /// <summary>
    ///     Current total diamonds the account has accumulated this season.
    /// </summary>
    [PhpProperty("cur_diamonds")]
    public int TotalDiamonds { get; set; } = 0;

    /// <summary>
    ///     Seasonal shop loot box prices and information.
    /// </summary>
    [PhpProperty("box_price")]
    public Dictionary<int, int> BoxPrice { get; set; } = [];
}

public class CampaignReward
{
    /// <summary>
    ///     Champions Of Newerth reward level before the match.
    ///     Set to "-2" if no previous match data exists.
    /// </summary>
    [PhpProperty("old_lvl")]
    public int PreviousCampaignLevel { get; set; } = 5;

    /// <summary>
    ///     Current Champions Of Newerth reward level after the match.
    ///     Maximum level is "6".
    /// </summary>
    [PhpProperty("curr_lvl")]
    public int CurrentCampaignLevel { get; set; } = 6;

    /// <summary>
    ///     Next Champions Of Newerth reward level to unlock.
    ///     Set to "0" when maximum level ("6") has been reached.
    /// </summary>
    [PhpProperty("next_lvl")]
    public int NextLevel { get; set; } = 0;

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
    [PhpProperty("require_rank")]
    public int RequireRank { get; set; } = 0;

    /// <summary>
    ///     Number of additional matches needed to accumulate enough reward points to reach the next Champions Of Newerth level.
    ///     Each level requires "12" reward points, earned from winning matches.
    ///     Set to "0" when maximum level has been reached.
    /// </summary>
    [PhpProperty("need_more_play")]
    public int NeedMorePlay { get; set; } = 0;

    /// <summary>
    ///     Progress percentage towards the next Champions Of Newerth reward level before the match.
    ///     Calculated as "reward_points" divided by 12, formatted as a decimal string (e.g. "0.75" for 75%).
    /// </summary>
    [PhpProperty("percentage_before")]
    public string PercentageBefore { get; set; } = "0.92";

    /// <summary>
    ///     Progress percentage towards the next Champions Of Newerth reward level after the match.
    ///     Calculated as "reward_points" divided by 12, formatted as a decimal string (e.g. "1.00" for 100%).
    /// </summary>
    [PhpProperty("percentage")]
    public string Percentage { get; set; } = "1.00";
}
