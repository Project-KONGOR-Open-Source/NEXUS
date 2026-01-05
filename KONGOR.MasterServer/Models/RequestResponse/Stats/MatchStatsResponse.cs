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
    public required int SilverCoins { get; set; }

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

public class MatchSummary(MatchStatistics matchStatistics, List<PlayerStatistics> playerStatistics)
{
    /// <summary>
    ///     The unique identifier for the match.
    /// </summary>
    [PhpProperty("match_id")]
    public int MatchID { get; init; } = matchStatistics.ID;

    /// <summary>
    ///     The server ID where the match was hosted.
    /// </summary>
    [PhpProperty("server_id")]
    public int ServerID { get; init; } = matchStatistics.ServerID;

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
    public int TimePlayed { get; init; } = matchStatistics.TimePlayed;

    /// <summary>
    ///     The host where the match replay file is stored.
    /// </summary>
    [PhpProperty("file_host")]
    public string FileHost { get; init; }

    /// <summary>
    ///     The size of the match replay file in bytes.
    /// </summary>
    [PhpProperty("file_size")]
    public int FileSize { get; init; } = matchStatistics.FileSize;

    /// <summary>
    ///     The filename of the match replay file.
    /// </summary>
    [PhpProperty("file_name")]
    public string FileName { get; init; } = matchStatistics.FileName;

    /// <summary>
    ///     The connection state or match state code.
    /// </summary>
    [PhpProperty("c_state")]
    public int ConnectionState { get; init; } = matchStatistics.ConnectionState;

    /// <summary>
    ///     The game client version used for the match.
    /// </summary>
    [PhpProperty("version")]
    public string Version { get; init; } = matchStatistics.Version;

    /// <summary>
    ///     The average Player Skill Rating (PSR) of all players in the match.
    /// </summary>
    [PhpProperty("avgpsr")]
    public int AveragePSR { get; init; } = matchStatistics.AveragePSR;

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
    public string MatchName { get; init; }

    /// <summary>
    ///     The match class/type (e.g. public match, tournament match, custom match).
    ///     <code>
    ///         0 -> Public Match
    ///         1 -> Tournament Match
    ///         2 -> Custom Match
    ///         3 -> Campaign Match
    ///     </code>
    /// </summary>
    [PhpProperty("class")]
    public int Class { get; init; }

    /// <summary>
    ///     Whether the match was private (1) or public (0).
    /// </summary>
    [PhpProperty("private")]
    public int Private { get; init; } = IsPrivateMatch(playerStatistics);

    /// <summary>
    ///     Normal Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("nm")]
    public int NormalMode { get; init; }

    /// <summary>
    ///     Single Draft Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("sd")]
    public int SingleDraft { get; init; }

    /// <summary>
    ///     Random Draft Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("rd")]
    public int RandomDraft { get; init; }

    /// <summary>
    ///     Death Match Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("dm")]
    public int DeathMatch { get; init; }

    /// <summary>
    ///     Banning Draft Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("bd")]
    public int BanningDraft { get; init; }

    /// <summary>
    ///     Banning Pick Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("bp")]
    public int BanningPick { get; init; }

    /// <summary>
    ///     All Random Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("ar")]
    public int AllRandom { get; init; }

    /// <summary>
    ///     Captains Draft Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("cd")]
    public int CaptainsDraft { get; init; }

    /// <summary>
    ///     Captains Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("cm")]
    public int CaptainsMode { get; init; }

    /// <summary>
    ///     Lock Pick Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("lp")]
    public int LockPick { get; init; }

    /// <summary>
    ///     Blind Ban Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("bb")]
    public int BlindBan { get; init; }

    /// <summary>
    ///     Balanced Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("bm")]
    public int BalancedMode { get; init; }

    /// <summary>
    ///     Kros Mode or special game mode indicator.
    ///     <code>
    ///         0 -> Standard match
    ///         1 -> Reserved
    ///         2 -> Solo Diff Mode (1v1)
    ///         3 -> Solo Same Mode (1v1)
    ///         4 -> Hero Ban Mode
    ///         5 -> MidWars Beta Mode
    ///     </code>
    /// </summary>
    [PhpProperty("km")]
    public int KrosMode { get; init; }

    /// <summary>
    ///     Whether the match was a league/ranked match (1) or casual (0).
    /// </summary>
    [PhpProperty("league")]
    public int League { get; init; }

    /// <summary>
    ///     The maximum number of players allowed in the match (typically 2, 4, 6, 8, or 10).
    /// </summary>
    [PhpProperty("max_players")]
    public int MaxPlayers { get; init; }

    /// <summary>
    ///     Deprecated skill-based server filter that was used for matchmaking.
    ///     <code>
    ///         0 -> Noobs Only
    ///         1 -> Noobs Allowed
    ///         2 -> Pro
    ///     </code>
    ///     This feature is no longer active and the field has no functional purpose.
    /// </summary>
    [PhpProperty("tier")]
    public int Tier { get; init; }

    /// <summary>
    ///     No Repick option flag (1 = repicking disabled, 0 = repicking allowed).
    /// </summary>
    [PhpProperty("no_repick")]
    public int NoRepick { get; init; }

    /// <summary>
    ///     No Agility Heroes option flag (1 = agility heroes banned, 0 = allowed).
    /// </summary>
    [PhpProperty("no_agi")]
    public int NoAgility { get; init; }

    /// <summary>
    ///     Drop Items On Death option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("drp_itm")]
    public int DropItems { get; init; }

    /// <summary>
    ///     No Respawn Timer option flag (1 = picking timer disabled, 0 = timer enabled).
    /// </summary>
    [PhpProperty("no_timer")]
    public int NoRespawnTimer { get; init; }

    /// <summary>
    ///     Reverse Hero Selection option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("rev_hs")]
    public int ReverseHeroSelection { get; init; }

    /// <summary>
    ///     No Swap option flag (1 = hero swapping disabled, 0 = swapping allowed).
    /// </summary>
    [PhpProperty("no_swap")]
    public int NoSwap { get; init; }

    /// <summary>
    ///     No Intelligence Heroes option flag (1 = intelligence heroes banned, 0 = allowed).
    /// </summary>
    [PhpProperty("no_int")]
    public int NoIntelligence { get; init; }

    /// <summary>
    ///     Alternate Picking option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("alt_pick")]
    public int AlternatePicking { get; init; }

    /// <summary>
    ///     Ban Phase option flag (1 = ban phase enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("veto")]
    public int BanPhase { get; init; }

    /// <summary>
    ///     Shuffle Abilities option flag (1 = abilities shuffled/randomised, 0 = normal abilities).
    ///     Used in Rift Wars and other Kros Mode variants.
    /// </summary>
    [PhpProperty("shuf")]
    public int ShuffleAbilities { get; init; }

    /// <summary>
    ///     No Strength Heroes option flag (1 = strength heroes banned, 0 = allowed).
    /// </summary>
    [PhpProperty("no_str")]
    public int NoStrength { get; init; }

    /// <summary>
    ///     No Power-Ups option flag (1 = power-ups/runes disabled, 0 = enabled).
    /// </summary>
    [PhpProperty("no_pups")]
    public int NoPowerUps { get; init; }

    /// <summary>
    ///     Duplicate Heroes option flag (1 = duplicate heroes allowed, 0 = each hero unique).
    /// </summary>
    [PhpProperty("dup_h")]
    public int DuplicateHeroes { get; init; }

    /// <summary>
    ///     All Pick Mode option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("ap")]
    public int AllPick { get; init; }

    /// <summary>
    ///     Balanced Random Mode option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("br")]
    public int BalancedRandom { get; init; }

    /// <summary>
    ///     Easy Mode option flag (1 = easy mode enabled, 0 = normal difficulty).
    /// </summary>
    [PhpProperty("em")]
    public int EasyMode { get; init; }

    /// <summary>
    ///     Casual Mode option flag (1 = casual mode enabled, 0 = normal mode).
    /// </summary>
    [PhpProperty("cas")]
    public int CasualMode { get; init; }

    /// <summary>
    ///     Reverse Selection option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("rs")]
    public int ReverseSelection { get; init; }

    /// <summary>
    ///     No Leaver option flag (1 = no leaver penalty applied, 0 = leaver penalties enabled).
    /// </summary>
    [PhpProperty("nl")]
    public int NoLeaver { get; init; }

    /// <summary>
    ///     Official Match flag (1 = official tournament match, 0 = unofficial).
    /// </summary>
    [PhpProperty("officl")]
    public int Official { get; init; }

    /// <summary>
    ///     No Statistics option flag (1 = match stats not recorded, 0 = stats recorded).
    /// </summary>
    [PhpProperty("no_stats")]
    public int NoStatistics { get; init; }

    /// <summary>
    ///     Auto Balance option flag (1 = teams automatically balanced, 0 = manual teams).
    /// </summary>
    [PhpProperty("ab")]
    public int AutoBalance { get; init; }

    /// <summary>
    ///     Hardcore Mode option flag (1 = hardcore difficulty enabled, 0 = normal).
    /// </summary>
    [PhpProperty("hardcore")]
    public int Hardcore { get; init; }

    /// <summary>
    ///     Development Heroes option flag (1 = development/unreleased heroes allowed, 0 = only released heroes).
    /// </summary>
    [PhpProperty("dev_heroes")]
    public int DevelopmentHeroes { get; init; }

    /// <summary>
    ///     Verified Only option flag (1 = only verified accounts allowed, 0 = all accounts allowed).
    /// </summary>
    [PhpProperty("verified_only")]
    public int VerifiedOnly { get; init; }

    /// <summary>
    ///     Gated option flag (1 = gated/restricted match, 0 = open match).
    /// </summary>
    [PhpProperty("gated")]
    public int Gated { get; init; }

    /// <summary>
    ///     Rapid Fire Mode option flag (1 = rapid fire mode enabled, 0 = normal ability cooldowns).
    /// </summary>
    [PhpProperty("rapidfire")]
    public int RapidFire { get; init; }

    /// <summary>
    ///     The UNIX timestamp (in seconds) when the match started.
    /// </summary>
    [PhpProperty("timestamp")]
    public int Timestamp { get; init; } = Convert.ToInt32(Math.Max(matchStatistics.TimestampRecorded.ToUnixTimeSeconds(), Convert.ToInt64(Int32.MaxValue)));

    /// <summary>
    ///     The URL for the match replay file.
    /// </summary>
    [PhpProperty("url")]
    public string URL { get; init; }

    /// <summary>
    ///     The size of the match replay file (human-readable format or bytes as string).
    /// </summary>
    [PhpProperty("size")]
    public string Size { get; init; }

    /// <summary>
    ///     The name or title of the replay file.
    /// </summary>
    [PhpProperty("name")]
    public string Name { get; init; }

    /// <summary>
    ///     The directory path where the replay file is stored.
    /// </summary>
    [PhpProperty("dir")]
    public string Directory { get; init; }

    /// <summary>
    ///     The S3 download URL for the match replay file.
    /// </summary>
    [PhpProperty("s3_url")]
    public string S3URL { get; init; }

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
    //    MasteryExperienceBoostProductCount = rewards.MasteryBoostsOwned;
    //    MasteryExperienceSuperBoostProductCount = rewards.MasterySuperBoostsOwned;
    //    MasteryExperienceHeroesCount = rewards.MasteryMaxLevelHeroesCount;
    //}

    /// <summary>
    ///     The identifier of the hero, in the format Hero_{Snake_Case_Name}.
    /// </summary>
    [PhpProperty("cli_name")]
    public required string HeroIdentifier { get; init; } = heroIdentifier;

    /// <summary>
    ///     The hero's original mastery experience before the match.
    ///     This is the current mastery level progress persisted to the database.
    /// </summary>
    [PhpProperty("mastery_exp_original")]
    public required int CurrentMasteryExperience { get; init; } = currentMasteryExperience;

    /// <summary>
    ///     The base mastery experience earned during the match.
    ///     Calculated from match duration, map, match type, and win/loss status.
    ///     Does not include bonuses or boosts.
    /// </summary>
    [PhpProperty("mastery_exp_match")]
    public required int MatchMasteryExperience { get; init; } = matchMasteryExperience;

    /// <summary>
    ///     Additional mastery experience bonus from map-specific multipliers.
    ///     Applied as a percentage multiplier to the base experience.
    /// </summary>
    [PhpProperty("mastery_exp_bonus")]
    public required int MasteryExperienceBonus { get; init; } = 0;

    /// <summary>
    ///     The additional mastery experience gained from applying a regular mastery boost consumable.
    ///     Set to zero initially when match results are calculated.
    ///     Only populated with a non-zero value after the player applies a mastery boost product.
    /// </summary>
    [PhpProperty("mastery_exp_boost")]
    public required int MasteryExperienceBoost { get; init; } = 0;

    /// <summary>
    ///     The additional mastery experience gained from applying a super mastery boost consumable.
    ///     Set to zero initially when match results are calculated.
    ///     Only populated with a non-zero value after the player applies a super mastery boost product.
    /// </summary>
    [PhpProperty("mastery_exp_super_boost")]
    public required int MasteryExperienceSuperBoost { get; init; } = 0;

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
    public required int MasteryExperienceHeroesBonus { get; init; } = bonusExperience;

    /// <summary>
    ///     The potential experience that can be gained by using a regular mastery boost.
    ///     Displayed when hovering over the mastery boost button in the UI.
    /// </summary>
    [PhpProperty("mastery_exp_to_boost")]
    public required int MasteryExperienceToBoost { get; init; } = (matchMasteryExperience + bonusExperience) * 2;

    /// <summary>
    ///     Special event bonus mastery experience granted during promotional periods.
    ///     Typically zero unless an admin-configured mastery experience event is active.
    /// </summary>
    [PhpProperty("mastery_exp_event")]
    public required int MasteryExperienceEventBonus { get; init; } = 0;

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

public class MatchPlayerStatistics
{
    /// <summary>
    ///     The unique identifier for the match.
    /// </summary>
    [PhpProperty("match_id")]
    public required int MatchID { get; set; }

    /// <summary>
    ///     The player's account ID.
    /// </summary>
    [PhpProperty("account_id")]
    public required int AccountID { get; set; }

    /// <summary>
    ///     The account name (nickname) of the player.
    /// </summary>
    [PhpProperty("nickname")]
    public required string AccountName { get; set; }

    /// <summary>
    ///     The clan ID of the player's clan, or "0" if the player is not in a clan.
    /// </summary>
    [PhpProperty("clan_id")]
    public required string ClanID { get; set; }

    /// <summary>
    ///     The unique identifier of the hero played in the match.
    /// </summary>
    [PhpProperty("hero_id")]
    public required string HeroID { get; set; }

    /// <summary>
    ///     The lobby position of the player (0-9), indicating their slot in the pre-match lobby.
    /// </summary>
    [PhpProperty("position")]
    public required string Position { get; set; }

    /// <summary>
    ///     The team the player was on ("1" for Legion, "2" for Hellbourne).
    /// </summary>
    [PhpProperty("team")]
    public required string Team { get; set; }

    /// <summary>
    ///     The final hero level reached by the player in the match (1-25).
    /// </summary>
    [PhpProperty("level")]
    public required string Level { get; set; }

    /// <summary>
    ///     The number of wins on the player's account before this match.
    /// </summary>
    [PhpProperty("wins")]
    public required string Wins { get; set; }

    /// <summary>
    ///     The number of losses on the player's account before this match.
    /// </summary>
    [PhpProperty("losses")]
    public required string Losses { get; set; }

    /// <summary>
    ///     The number of conceded matches on the player's account before this match.
    /// </summary>
    [PhpProperty("concedes")]
    public required string Concedes { get; set; }

    /// <summary>
    ///     The number of concede votes the player cast during the match.
    /// </summary>
    [PhpProperty("concedevotes")]
    public required string ConcedeVotes { get; set; }

    /// <summary>
    ///     The number of times the player bought back into the match after dying.
    /// </summary>
    [PhpProperty("buybacks")]
    public required string Buybacks { get; set; }

    /// <summary>
    ///     The number of disconnections on the player's account before this match.
    /// </summary>
    [PhpProperty("discos")]
    public required string Disconnections { get; set; }

    /// <summary>
    ///     The number of times the player was kicked from matches on their account before this match.
    /// </summary>
    [PhpProperty("kicked")]
    public required string Kicked { get; set; }

    /// <summary>
    ///     The player's Public Skill Rating (PSR) before this match.
    /// </summary>
    [PhpProperty("pub_skill")]
    public required string PublicSkill { get; set; }

    /// <summary>
    ///     The number of public matches played on the player's account before this match.
    /// </summary>
    [PhpProperty("pub_count")]
    public required string PublicCount { get; set; }

    /// <summary>
    ///     The player's Automatic Matchmaking (AMM) solo rating before this match.
    /// </summary>
    [PhpProperty("amm_solo_rating")]
    public required string AMMSoloRating { get; set; }

    /// <summary>
    ///     The number of AMM solo matches played on the player's account before this match.
    /// </summary>
    [PhpProperty("amm_solo_count")]
    public required string AMMSoloCount { get; set; }

    /// <summary>
    ///     The player's Automatic Matchmaking (AMM) team rating before this match.
    /// </summary>
    [PhpProperty("amm_team_rating")]
    public required string AMMTeamRating { get; set; }

    /// <summary>
    ///     The number of AMM team matches played on the player's account before this match.
    /// </summary>
    [PhpProperty("amm_team_count")]
    public required string AMMTeamCount { get; set; }

    /// <summary>
    ///     The player's average score across all matches before this match.
    /// </summary>
    [PhpProperty("avg_score")]
    public required string AverageScore { get; set; }

    /// <summary>
    ///     The number of enemy hero kills achieved by the player in the match.
    /// </summary>
    [PhpProperty("herokills")]
    public required string HeroKills { get; set; }

    /// <summary>
    ///     The total damage dealt to enemy heroes by the player in the match.
    /// </summary>
    [PhpProperty("herodmg")]
    public required string HeroDamage { get; set; }

    /// <summary>
    ///     The total experience gained from killing or assisting in killing enemy heroes.
    /// </summary>
    [PhpProperty("heroexp")]
    public required string HeroExperience { get; set; }

    /// <summary>
    ///     The total gold earned from killing or assisting in killing enemy heroes.
    /// </summary>
    [PhpProperty("herokillsgold")]
    public required string HeroKillsGold { get; set; }

    /// <summary>
    ///     The number of assists (participating in hero kills without landing the final blow) achieved by the player.
    /// </summary>
    [PhpProperty("heroassists")]
    public required string HeroAssists { get; set; }

    /// <summary>
    ///     The number of times the player died in the match.
    /// </summary>
    [PhpProperty("deaths")]
    public required string Deaths { get; set; }

    /// <summary>
    ///     The total gold lost by the player due to deaths in the match.
    /// </summary>
    [PhpProperty("goldlost2death")]
    public required string GoldLostToDeath { get; set; }

    /// <summary>
    ///     The total time in seconds the player spent dead (waiting to respawn) during the match.
    /// </summary>
    [PhpProperty("secs_dead")]
    public required string SecondsDead { get; set; }

    /// <summary>
    ///     The number of friendly team creeps killed by the player (last-hitting own creeps for gold/experience).
    /// </summary>
    [PhpProperty("teamcreepkills")]
    public required string TeamCreepKills { get; set; }

    /// <summary>
    ///     The total damage dealt to friendly team creeps by the player.
    /// </summary>
    [PhpProperty("teamcreepdmg")]
    public required string TeamCreepDamage { get; set; }

    /// <summary>
    ///     The total experience gained from killing friendly team creeps.
    /// </summary>
    [PhpProperty("teamcreepexp")]
    public required string TeamCreepExperience { get; set; }

    /// <summary>
    ///     The total gold earned from killing friendly team creeps.
    /// </summary>
    [PhpProperty("teamcreepgold")]
    public required string TeamCreepGold { get; set; }

    /// <summary>
    ///     The number of neutral creeps killed by the player (jungle creeps).
    /// </summary>
    [PhpProperty("neutralcreepkills")]
    public required string NeutralCreepKills { get; set; }

    /// <summary>
    ///     The total damage dealt to neutral creeps by the player.
    /// </summary>
    [PhpProperty("neutralcreepdmg")]
    public required string NeutralCreepDamage { get; set; }

    /// <summary>
    ///     The total experience gained from killing neutral creeps.
    /// </summary>
    [PhpProperty("neutralcreepexp")]
    public required string NeutralCreepExperience { get; set; }

    /// <summary>
    ///     The total gold earned from killing neutral creeps.
    /// </summary>
    [PhpProperty("neutralcreepgold")]
    public required string NeutralCreepGold { get; set; }

    /// <summary>
    ///     The total damage dealt to enemy buildings (towers, barracks, base structures) by the player.
    /// </summary>
    [PhpProperty("bdmg")]
    public required string BuildingDamage { get; set; }

    /// <summary>
    ///     The total experience gained from damaging or destroying enemy buildings.
    /// </summary>
    [PhpProperty("bdmgexp")]
    public required string BuildingExperience { get; set; }

    /// <summary>
    ///     The number of enemy buildings (towers, barracks) destroyed by the player.
    /// </summary>
    [PhpProperty("razed")]
    public required string BuildingsRazed { get; set; }

    /// <summary>
    ///     The total gold earned from damaging or destroying enemy buildings.
    /// </summary>
    [PhpProperty("bgold")]
    public required string BuildingGold { get; set; }

    /// <summary>
    ///     The number of enemy creeps denied by the player (last-hitting enemy creeps to prevent opponents from gaining gold/experience).
    /// </summary>
    [PhpProperty("denies")]
    public required string Denies { get; set; }

    /// <summary>
    ///     The total experience denied to opponents through denying enemy creeps.
    /// </summary>
    [PhpProperty("exp_denied")]
    public required string ExperienceDenied { get; set; }

    /// <summary>
    ///     The total gold accumulated by the player at the end of the match.
    /// </summary>
    [PhpProperty("gold")]
    public required string Gold { get; set; }

    /// <summary>
    ///     The total gold spent by the player on items during the match.
    /// </summary>
    [PhpProperty("gold_spent")]
    public required string GoldSpent { get; set; }

    /// <summary>
    ///     The total experience gained by the player during the match.
    /// </summary>
    [PhpProperty("exp")]
    public required string Experience { get; set; }

    /// <summary>
    ///     The total number of actions performed by the player during the match (clicks, commands, ability usage, etc.).
    /// </summary>
    [PhpProperty("actions")]
    public required string Actions { get; set; }

    /// <summary>
    ///     The total time in seconds the player was actively playing in the match.
    /// </summary>
    [PhpProperty("secs")]
    public required string Seconds { get; set; }

    /// <summary>
    ///     The number of consumable items (potions, wards, teleport scrolls, etc.) purchased by the player.
    /// </summary>
    [PhpProperty("consumables")]
    public required string Consumables { get; set; }

    /// <summary>
    ///     The number of observer or sentry wards placed by the player during the match.
    /// </summary>
    [PhpProperty("wards")]
    public required string Wards { get; set; }

    /// <summary>
    ///     The total time in seconds the player spent within experience range of dying enemy units.
    /// </summary>
    [PhpProperty("time_earning_exp")]
    public required string TimeEarningExperience { get; set; }

    /// <summary>
    ///     The number of First Blood awards earned by the player (1 or 0).
    /// </summary>
    [PhpProperty("bloodlust")]
    public required string FirstBlood { get; set; }

    /// <summary>
    ///     The number of Double Kill awards earned by the player (killing 2 heroes in quick succession).
    /// </summary>
    [PhpProperty("doublekill")]
    public required string DoubleKill { get; set; }

    /// <summary>
    ///     The number of Triple Kill awards earned by the player (killing 3 heroes in quick succession).
    /// </summary>
    [PhpProperty("triplekill")]
    public required string TripleKill { get; set; }

    /// <summary>
    ///     The number of Quad Kill awards earned by the player (killing 4 heroes in quick succession).
    /// </summary>
    [PhpProperty("quadkill")]
    public required string QuadKill { get; set; }

    /// <summary>
    ///     The number of Annihilation awards earned by the player (killing all 5 enemy heroes in quick succession).
    /// </summary>
    [PhpProperty("annihilation")]
    public required string Annihilation { get; set; }

    /// <summary>
    ///     The number of 3-kill streaks achieved by the player (killing 3 heroes without dying).
    /// </summary>
    [PhpProperty("ks3")]
    public required string KillStreak3 { get; set; }

    /// <summary>
    ///     The number of 4-kill streaks achieved by the player (killing 4 heroes without dying).
    /// </summary>
    [PhpProperty("ks4")]
    public required string KillStreak4 { get; set; }

    /// <summary>
    ///     The number of 5-kill streaks achieved by the player (killing 5 heroes without dying).
    /// </summary>
    [PhpProperty("ks5")]
    public required string KillStreak5 { get; set; }

    /// <summary>
    ///     The number of 6-kill streaks achieved by the player (killing 6 heroes without dying).
    /// </summary>
    [PhpProperty("ks6")]
    public required string KillStreak6 { get; set; }

    /// <summary>
    ///     The number of 7-kill streaks achieved by the player (killing 7 heroes without dying).
    /// </summary>
    [PhpProperty("ks7")]
    public required string KillStreak7 { get; set; }

    /// <summary>
    ///     The number of 8-kill streaks achieved by the player (killing 8 heroes without dying).
    /// </summary>
    [PhpProperty("ks8")]
    public required string KillStreak8 { get; set; }

    /// <summary>
    ///     The number of 9-kill streaks achieved by the player (killing 9 heroes without dying).
    /// </summary>
    [PhpProperty("ks9")]
    public required string KillStreak9 { get; set; }

    /// <summary>
    ///     The number of 10-kill streaks achieved by the player (killing 10 heroes without dying).
    /// </summary>
    [PhpProperty("ks10")]
    public required string KillStreak10 { get; set; }

    /// <summary>
    ///     The number of 15-kill streaks achieved by the player (killing 15 heroes without dying).
    /// </summary>
    [PhpProperty("ks15")]
    public required string KillStreak15 { get; set; }

    /// <summary>
    ///     The number of Smackdown awards earned by the player (ending an enemy's kill streak).
    /// </summary>
    [PhpProperty("smackdown")]
    public required string Smackdown { get; set; }

    /// <summary>
    ///     The number of Humiliation awards earned by the player (killing an enemy hero who is significantly higher level).
    /// </summary>
    [PhpProperty("humiliation")]
    public required string Humiliation { get; set; }

    /// <summary>
    ///     The number of Nemesis awards earned by the player (being killed repeatedly by the same enemy hero).
    /// </summary>
    [PhpProperty("nemesis")]
    public required string Nemesis { get; set; }

    /// <summary>
    ///     The number of Retribution awards earned by the player (killing an enemy hero who has killed you repeatedly).
    /// </summary>
    [PhpProperty("retribution")]
    public required string Retribution { get; set; }

    /// <summary>
    ///     Whether the player used a token (game access token or dice token) during the match ("1" if used, "0" otherwise).
    /// </summary>
    [PhpProperty("used_token")]
    public required string UsedToken { get; set; }

    /// <summary>
    ///     The hero identifier in the format Hero_{Snake_Case_Name} (e.g. "Hero_Andromeda", "Hero_Legionnaire").
    /// </summary>
    [PhpProperty("cli_name")]
    public required string HeroIdentifier { get; set; }

    /// <summary>
    ///     The clan tag of the player's clan, or empty string if the player is not in a clan.
    /// </summary>
    [PhpProperty("tag")]
    public required string ClanTag { get; set; }

    /// <summary>
    ///     The alternative avatar name used by the player during the match, or empty string if using the default hero skin.
    /// </summary>
    [PhpProperty("alt_avatar_name")]
    public required string AlternativeAvatarName { get; set; }

    /// <summary>
    ///     Seasonal campaign progression information for the player in the match.
    /// </summary>
    [PhpProperty("campaign_info")]
    public required SeasonProgress SeasonProgress { get; set; }
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
