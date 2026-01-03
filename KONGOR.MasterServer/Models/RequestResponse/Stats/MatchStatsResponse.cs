namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

// ReSharper disable InconsistentNaming

public class MatchStatsResponse
{
    /*
     *  Existing Properties
     */

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

    /*
     *  Migrated Properties
     */

    /// <summary>
    ///     A list containing the primary summary of the match.
    ///     This is typically a single-element list but structured as an array in the original JSON.
    /// </summary>
    [PhpProperty("match_summ")]
    public required List<MatchSummary> MatchSummary { get; set; }

    /// <summary>
    ///     A dictionary of player statistics for the match, keyed by the player's account ID (or a variation thereof).
    /// </summary>
    [PhpProperty("match_player_stats")]
    public required Dictionary<string, MatchPlayerStats> MatchPlayerStats { get; set; }

    /// <summary>
    ///     A dictionary of player inventories for the match, keyed by the player's account ID.
    /// </summary>
    [PhpProperty("inventory")]
    public required Dictionary<string, MatchInventory> Inventory { get; set; }

    /// <summary>
    ///     Rewards or progression updates related to the match.
    /// </summary>
    [PhpProperty("con_reward")]
    public required ConReward ConReward { get; set; }

    /// <summary>
    ///     Mastery (hero progression) updates related to the match.
    /// </summary>
    [PhpProperty("match_mastery")]
    public required MatchMastery MatchMastery { get; set; }

    /// <summary>
    ///     Seasonal progression system data (diamonds/rewards).
    /// </summary>
    [PhpProperty("season_system")]
    public required SeasonSystem SeasonSystem { get; set; }
}

public class MatchSummary
{
    /// <summary>
    ///     The unique identifier for the match.
    /// </summary>
    [PhpProperty("match_id")]
    public required string MatchID { get; set; }

    /// <summary>
    ///     The ID of the server where the match took place.
    /// </summary>
    [PhpProperty("server_id")]
    public required string ServerID { get; set; }

    /// <summary>
    ///     The name of the map played (e.g., "newerthearth").
    /// </summary>
    [PhpProperty("map")]
    public required string Map { get; set; }

    /// <summary>
    ///     The version hash or identifier of the map.
    /// </summary>
    [PhpProperty("map_version")]
    public required string MapVersion { get; set; }

    /// <summary>
    ///     The total duration of the match in time format (H:MM:SS or similar).
    /// </summary>
    [PhpProperty("time_played")]
    public required string TimePlayed { get; set; }

    /// <summary>
    ///     The host address where the replay file is stored.
    /// </summary>
    [PhpProperty("file_host")]
    public required string FileHost { get; set; }

    /// <summary>
    ///     The size of the replay file in bytes.
    /// </summary>
    [PhpProperty("file_size")]
    public required string FileSize { get; set; }

    /// <summary>
    ///     The filename of the replay.
    /// </summary>
    [PhpProperty("file_name")]
    public required string FileName { get; set; }

    /// <summary>
    ///     The completion state of the match.
    /// </summary>
    [PhpProperty("c_state")]
    public required string CState { get; set; }

    /// <summary>
    ///     The game client version used for the match.
    /// </summary>
    [PhpProperty("version")]
    public required string Version { get; set; }

    /// <summary>
    ///     The average Public Skill Rating (PSR) of the match.
    /// </summary>
    [PhpProperty("avgpsr")]
    public required string AvgPsr { get; set; }

    /// <summary>
    ///     The date the match was played (e.g., MM/DD/YYYY).
    /// </summary>
    [PhpProperty("date")]
    public required string Date { get; set; }

    /// <summary>
    ///     The time of day when the match started.
    /// </summary>
    [PhpProperty("time")]
    public required string Time { get; set; }

    /// <summary>
    ///     The name of the match lobby.
    /// </summary>
    [PhpProperty("mname")]
    public required string MatchName { get; set; }

    /// <summary>
    ///     The class identifier for the match type (e.g., "ver" for verified).
    /// </summary>
    [PhpProperty("class")]
    public required string Class { get; set; }

    /// <summary>
    ///     Indicates if the match was private ("1" for yes, "0" for no).
    /// </summary>
    [PhpProperty("private")]
    public required string Private { get; set; }

    /// <summary>
    ///     "No Mods" mode flag.
    /// </summary>
    [PhpProperty("nm")]
    public required string Nm { get; set; }

    /// <summary>
    ///     "Single Draft" mode flag.
    /// </summary>
    [PhpProperty("sd")]
    public required string Sd { get; set; }

    /// <summary>
    ///     "Random Draft" mode flag.
    /// </summary>
    [PhpProperty("rd")]
    public required string Rd { get; set; }

    /// <summary>
    ///     "Death Match" mode flag.
    /// </summary>
    [PhpProperty("dm")]
    public required string Dm { get; set; }

    /// <summary>
    ///     "Banning Draft" mode flag.
    /// </summary>
    [PhpProperty("bd")]
    public required string Bd { get; set; }

    /// <summary>
    ///     "Banning Pick" mode flag.
    /// </summary>
    [PhpProperty("bp")]
    public required string Bp { get; set; }

    /// <summary>
    ///     "All Random" mode flag.
    /// </summary>
    [PhpProperty("ar")]
    public required string Ar { get; set; }

    /// <summary>
    ///     "Captains Draft" mode flag.
    /// </summary>
    [PhpProperty("cd")]
    public required string Cd { get; set; }

    /// <summary>
    ///     "Captains Mode" mode flag.
    /// </summary>
    [PhpProperty("cm")]
    public required string Cm { get; set; }

    /// <summary>
    ///     "League Play" mode flag.
    /// </summary>
    [PhpProperty("lp")]
    public required string Lp { get; set; }

    /// <summary>
    ///     "Blind Ban" mode flag.
    /// </summary>
    [PhpProperty("bb")]
    public required string Bb { get; set; }

    /// <summary>
    ///     "Balanced Mode" mode flag.
    /// </summary>
    [PhpProperty("bm")]
    public required string Bm { get; set; }

    /// <summary>
    ///     "Kros Mode" flag.
    /// </summary>
    [PhpProperty("km")]
    public required string Km { get; set; }

    /// <summary>
    ///     The league identifier associated with the match.
    /// </summary>
    [PhpProperty("league")]
    public required string League { get; set; }

    /// <summary>
    ///     The maximum number of players allowed in the match (e.g., "10").
    /// </summary>
    [PhpProperty("max_players")]
    public required string MaxPlayers { get; set; }

    /// <summary>
    ///     The tier of the match (e.g., "normal", "casual").
    /// </summary>
    [PhpProperty("tier")]
    public required string Tier { get; set; }

    /// <summary>
    ///     "No Repick" option flag.
    /// </summary>
    [PhpProperty("no_repick")]
    public required string NoRepick { get; set; }

    /// <summary>
    ///     "No Agility" option flag.
    /// </summary>
    [PhpProperty("no_agi")]
    public required string NoAgi { get; set; }

    /// <summary>
    ///     "Drop Items" option flag.
    /// </summary>
    [PhpProperty("drp_itm")]
    public required string DropItems { get; set; }

    /// <summary>
    ///     "No Timer" option flag.
    /// </summary>
    [PhpProperty("no_timer")]
    public required string NoTimer { get; set; }

    /// <summary>
    ///     "Reverse Hero Selection" option flag.
    /// </summary>
    [PhpProperty("rev_hs")]
    public required string RevHs { get; set; }

    /// <summary>
    ///     "No Swap" option flag.
    /// </summary>
    [PhpProperty("no_swap")]
    public required string NoSwap { get; set; }

    /// <summary>
    ///     "No Intelligence" option flag.
    /// </summary>
    [PhpProperty("no_int")]
    public required string NoInt { get; set; }

    /// <summary>
    ///     "Alternative Pick" option flag.
    /// </summary>
    [PhpProperty("alt_pick")]
    public required string AltPick { get; set; }

    /// <summary>
    ///     "Veto" option flag.
    /// </summary>
    [PhpProperty("veto")]
    public required string Veto { get; set; }

    /// <summary>
    ///     "Shuffle" option flag.
    /// </summary>
    [PhpProperty("shuf")]
    public required string Shuffle { get; set; }

    /// <summary>
    ///     "No Strength" option flag.
    /// </summary>
    [PhpProperty("no_str")]
    public required string NoStr { get; set; }

    /// <summary>
    ///     "No Power-ups" option flag.
    /// </summary>
    [PhpProperty("no_pups")]
    public required string NoPowerups { get; set; }

    /// <summary>
    ///     "Duplicate Heroes" option flag.
    /// </summary>
    [PhpProperty("dup_h")]
    public required string DuplicateHeroes { get; set; }

    /// <summary>
    ///     "All Pick" mode flag.
    /// </summary>
    [PhpProperty("ap")]
    public required string Ap { get; set; }

    /// <summary>
    ///     "Bot Reassignment" or Battle Royale flag.
    /// </summary>
    [PhpProperty("br")]
    public required string Br { get; set; }

    /// <summary>
    ///     "Easy Mode" flag.
    /// </summary>
    [PhpProperty("em")]
    public required string Em { get; set; }

    /// <summary>
    ///     "Casual Mode" flag.
    /// </summary>
    [PhpProperty("cas")]
    public required string Cas { get; set; }

    /// <summary>
    ///     "Force Random" or "Random Start" flag.
    /// </summary>
    [PhpProperty("rs")]
    public required string Rs { get; set; }

    /// <summary>
    ///     "No Leaver" flag, or "New Lobby" flag.
    /// </summary>
    [PhpProperty("nl")]
    public required string Nl { get; set; }

    /// <summary>
    ///     Indicates if the match was official ("1" for yes).
    /// </summary>
    [PhpProperty("officl")]
    public required string Official { get; set; }

    /// <summary>
    ///     "No Stats" match flag.
    /// </summary>
    [PhpProperty("no_stats")]
    public required string NoStats { get; set; }

    /// <summary>
    ///     "Auto Ban" option flag.
    /// </summary>
    [PhpProperty("ab")]
    public required string Ab { get; set; }

    /// <summary>
    ///     "Hardcore" mode flag.
    /// </summary>
    [PhpProperty("hardcore")]
    public required string Hardcore { get; set; }

    /// <summary>
    ///     "Dev Heroes" allowed flag.
    /// </summary>
    [PhpProperty("dev_heroes")]
    public required string DevHeroes { get; set; }

    /// <summary>
    ///     "Verified Only" match flag.
    /// </summary>
    [PhpProperty("verified_only")]
    public required string VerifiedOnly { get; set; }

    /// <summary>
    ///     "Gated" match flag (requires specific criteria).
    /// </summary>
    [PhpProperty("gated")]
    public required string Gated { get; set; }

    /// <summary>
    ///     "Rapid Fire" mode flag.
    /// </summary>
    [PhpProperty("rapidfire")]
    public required string RapidFire { get; set; }

    /// <summary>
    ///     The timestamp when the match was recorded/finished.
    /// </summary>
    [PhpProperty("timestamp")]
    public int Timestamp { get; set; }

    /// <summary>
    ///     The URL to the replay file.
    /// </summary>
    [PhpProperty("url")]
    public required string Url { get; set; }

    /// <summary>
    ///     The size of the replay file in human-readable string format.
    /// </summary>
    [PhpProperty("size")]
    public required string Size { get; set; }

    /// <summary>
    ///     The name of the match.
    /// </summary>
    [PhpProperty("name")]
    public required string Name { get; set; }

    /// <summary>
    ///     The directory path of the replay.
    /// </summary>
    [PhpProperty("dir")]
    public required string Dir { get; set; }

    /// <summary>
    ///     The S3 bucket URL for the replay.
    /// </summary>
    [PhpProperty("s3_url")]
    public required string S3Url { get; set; }

    /// <summary>
    ///     The ID (0 or 1) of the team that won the match.
    /// </summary>
    [PhpProperty("winning_team")]
    public required string WinningTeam { get; set; }

    /// <summary>
    ///     The game mode identifier (e.g., "1" for Normal, "2" for Casual).
    /// </summary>
    [PhpProperty("gamemode")]
    public required string GameMode { get; set; }

    /// <summary>
    ///     The account ID of the Most Valuable Player.
    /// </summary>
    [PhpProperty("mvp")]
    public required string Mvp { get; set; }

    /// <summary>
    ///     The account ID of the player awarded "Annihilation" (5 kills).
    /// </summary>
    [PhpProperty("awd_mann")]
    public required string AwardAnnihilation { get; set; }

    /// <summary>
    ///     The account ID of the player awarded "Quad Kill" (4 kills).
    /// </summary>
    [PhpProperty("awd_mqk")]
    public required string AwardQuadKill { get; set; }

    /// <summary>
    ///     The account ID of the player awarded "Legendary Kill Streak" (15+ kills).
    /// </summary>
    [PhpProperty("awd_lgks")]
    public required string AwardLegendaryKillStreak { get; set; }

    /// <summary>
    ///     The account ID of the player awarded "Smackdown" (taunt kill).
    /// </summary>
    [PhpProperty("awd_msd")]
    public required string AwardSmackDown { get; set; }

    /// <summary>
    ///     The account ID of the player awarded "Multi Kill".
    /// </summary>
    [PhpProperty("awd_mkill")]
    public required string AwardMultiKill { get; set; }

    /// <summary>
    ///     The account ID of the player awarded "Most Assists".
    /// </summary>
    [PhpProperty("awd_masst")]
    public required string AwardMultiAssist { get; set; }

    /// <summary>
    ///     The account ID of the player awarded "Lowest Health Death" (surviving with low HP?). Or perhaps Most deaths?
    /// </summary>
    [PhpProperty("awd_ledth")]
    public required string AwardLowHealthDeath { get; set; }

    /// <summary>
    ///     The account ID of the player awarded "Most Building Damage".
    /// </summary>
    [PhpProperty("awd_mbdmg")]
    public required string AwardMaxBuildingDamage { get; set; }

    /// <summary>
    ///     The account ID of the player awarded "Most Wards Killed".
    /// </summary>
    [PhpProperty("awd_mwk")]
    public required string AwardMaxWardsKilled { get; set; }

    /// <summary>
    ///     The account ID of the player awarded "Most Hero Damage".
    /// </summary>
    [PhpProperty("awd_mhdd")]
    public required string AwardMaxHeroDamage { get; set; }

    /// <summary>
    ///     The account ID of the player awarded "Highest Creep Score".
    /// </summary>
    [PhpProperty("awd_hcs")]
    public required string AwardHighestCreepScore { get; set; }
}

public class MatchPlayerStats
{
    /// <summary>
    ///     The unique identifier for the match.
    /// </summary>
    [PhpProperty("match_id")]
    public required string MatchID { get; set; }

    /// <summary>
    ///     The player's account ID.
    /// </summary>
    [PhpProperty("account_id")]
    public required string AccountID { get; set; }

    /// <summary>
    ///     The player's clan ID, if applicable.
    /// </summary>
    [PhpProperty("clan_id")]
    public required string ClanID { get; set; }

    /// <summary>
    ///     The ID of the hero played.
    /// </summary>
    [PhpProperty("hero_id")]
    public required string HeroID { get; set; }

    /// <summary>
    ///     The visual position/slot of the player in the lobby/game.
    /// </summary>
    [PhpProperty("position")]
    public required string Position { get; set; }

    /// <summary>
    ///     The team ID (1 for Legion, 2 for Hellbourne).
    /// </summary>
    [PhpProperty("team")]
    public required string Team { get; set; }

    /// <summary>
    ///     The hero level reached at the end of the match.
    /// </summary>
    [PhpProperty("level")]
    public required string Level { get; set; }

    /// <summary>
    ///     "1" if the player won, "0" otherwise.
    /// </summary>
    [PhpProperty("wins")]
    public required string Wins { get; set; }

    /// <summary>
    ///     "1" if the player lost, "0" otherwise.
    /// </summary>
    [PhpProperty("losses")]
    public required string Losses { get; set; }

    /// <summary>
    ///     "1" if the outcome was a concession.
    /// </summary>
    [PhpProperty("concedes")]
    public required string Concedes { get; set; }

    /// <summary>
    ///     The number of times this player voted to concede.
    /// </summary>
    [PhpProperty("concedevotes")]
    public required string ConcedeVotes { get; set; }

    /// <summary>
    ///     The number of times the player bought back into the game.
    /// </summary>
    [PhpProperty("buybacks")]
    public required string Buybacks { get; set; }

    /// <summary>
    ///     "1" if the player disconnected/terminated connection.
    /// </summary>
    [PhpProperty("discos")]
    public required string Discos { get; set; }

    /// <summary>
    ///     "1" if the player was kicked.
    /// </summary>
    [PhpProperty("kicked")]
    public required string Kicked { get; set; }

    /// <summary>
    ///     The player's Public Skill Rating (PSR) before the match?
    /// </summary>
    [PhpProperty("pub_skill")]
    public required string PubSkill { get; set; }

    /// <summary>
    ///     The number of public matches played.
    /// </summary>
    [PhpProperty("pub_count")]
    public required string PubCount { get; set; }

    /// <summary>
    ///     The player's Matchmaking Rating (MMR) for solo queue.
    /// </summary>
    [PhpProperty("amm_solo_rating")]
    public required string AmmSoloRating { get; set; }

    /// <summary>
    ///     The number of solo matchmaking games played.
    /// </summary>
    [PhpProperty("amm_solo_count")]
    public required string AmmSoloCount { get; set; }

    /// <summary>
    ///     The player's Matchmaking Rating (MMR) for team queue.
    /// </summary>
    [PhpProperty("amm_team_rating")]
    public required string AmmTeamRating { get; set; }

    /// <summary>
    ///     The number of team matchmaking games played.
    /// </summary>
    [PhpProperty("amm_team_count")]
    public required string AmmTeamCount { get; set; }

    /// <summary>
    ///     The average score (GPM/XPM composite?) or an internal score metric.
    /// </summary>
    [PhpProperty("avg_score")]
    public required string AvgScore { get; set; }

    /// <summary>
    ///     Total number of enemy heroes killed.
    /// </summary>
    [PhpProperty("herokills")]
    public required string HeroKills { get; set; }

    /// <summary>
    ///     Total damage dealt to enemy heroes.
    /// </summary>
    [PhpProperty("herodmg")]
    public required string HeroDamage { get; set; }

    /// <summary>
    ///     Experience earned from killing heroes.
    /// </summary>
    [PhpProperty("heroexp")]
    public required string HeroExp { get; set; }

    /// <summary>
    ///     Gold earned from killing heroes.
    /// </summary>
    [PhpProperty("herokillsgold")]
    public required string HeroKillsGold { get; set; }

    /// <summary>
    ///     Number of assists.
    /// </summary>
    [PhpProperty("heroassists")]
    public required string HeroAssists { get; set; }

    /// <summary>
    ///     Total number of times the player died.
    /// </summary>
    [PhpProperty("deaths")]
    public required string Deaths { get; set; }

    /// <summary>
    ///     Total gold lost due to dying.
    /// </summary>
    [PhpProperty("goldlost2death")]
    public required string GoldLostToDeath { get; set; }

    /// <summary>
    ///     Total seconds spent dead.
    /// </summary>
    [PhpProperty("secs_dead")]
    public required string SecondsDead { get; set; }

    /// <summary>
    ///     Total team creep kills (assists on creeps?).
    /// </summary>
    [PhpProperty("teamcreepkills")]
    public required string TeamCreepKills { get; set; }

    /// <summary>
    ///     Damage dealt to team creeps (denies?).
    /// </summary>
    [PhpProperty("teamcreepdmg")]
    public required string TeamCreepDamage { get; set; }

    /// <summary>
    ///     Experience from team creeps.
    /// </summary>
    [PhpProperty("teamcreepexp")]
    public required string TeamCreepExp { get; set; }

    /// <summary>
    ///     Gold from team creeps.
    /// </summary>
    [PhpProperty("teamcreepgold")]
    public required string TeamCreepGold { get; set; }

    /// <summary>
    ///     Number of neutral creeps killed.
    /// </summary>
    [PhpProperty("neutralcreepkills")]
    public required string NeutralCreepKills { get; set; }

    /// <summary>
    ///     Damage dealt to neutral creeps.
    /// </summary>
    [PhpProperty("neutralcreepdmg")]
    public required string NeutralCreepDamage { get; set; }

    /// <summary>
    ///     Experience earned from neutral creeps.
    /// </summary>
    [PhpProperty("neutralcreepexp")]
    public required string NeutralCreepExp { get; set; }

    /// <summary>
    ///     Gold earned from neutral creeps.
    /// </summary>
    [PhpProperty("neutralcreepgold")]
    public required string NeutralCreepGold { get; set; }

    /// <summary>
    ///     Total damage dealt to structures (towers, racks, shrine).
    /// </summary>
    [PhpProperty("bdmg")]
    public required string BuildingDamage { get; set; }

    /// <summary>
    ///     Experience earned from destroying buildings.
    /// </summary>
    [PhpProperty("bdmgexp")]
    public required string BuildingDamageExp { get; set; }

    /// <summary>
    ///     Number of buildings razed/destroyed.
    /// </summary>
    [PhpProperty("razed")]
    public required string Razed { get; set; }

    /// <summary>
    ///     Gold earned from destroying buildings.
    /// </summary>
    [PhpProperty("bgold")]
    public required string BuildingGold { get; set; }

    /// <summary>
    ///     Number of creeps denied.
    /// </summary>
    [PhpProperty("denies")]
    public required string Denies { get; set; }

    /// <summary>
    ///     Amount of experience denied to the enemy.
    /// </summary>
    [PhpProperty("exp_denied")]
    public required string ExpDenied { get; set; }

    /// <summary>
    ///     Total gold earned (GPM related).
    /// </summary>
    [PhpProperty("gold")]
    public required string Gold { get; set; }

    /// <summary>
    ///     Total gold spent on items and buybacks.
    /// </summary>
    [PhpProperty("gold_spent")]
    public required string GoldSpent { get; set; }

    /// <summary>
    ///     Total experience earned (XPM related).
    /// </summary>
    [PhpProperty("exp")]
    public required string Exp { get; set; }

    /// <summary>
    ///     Number of actions performed (APM proxy).
    /// </summary>
    [PhpProperty("actions")]
    public required string Actions { get; set; }

    /// <summary>
    ///     Duration of the match for this player in seconds.
    /// </summary>
    [PhpProperty("secs")]
    public required string Seconds { get; set; }

    /// <summary>
    ///     Number of consumables used.
    /// </summary>
    [PhpProperty("consumables")]
    public required string Consumables { get; set; }

    /// <summary>
    ///     Number of wards placed.
    /// </summary>
    [PhpProperty("wards")]
    public required string Wards { get; set; }

    /// <summary>
    ///     Time spent earning experience (combat time).
    /// </summary>
    [PhpProperty("time_earning_exp")]
    public required string TimeEarningExp { get; set; }

    /// <summary>
    ///     "1" if the player achieved "Bloodlust" (First Blood).
    /// </summary>
    [PhpProperty("bloodlust")]
    public required string Bloodlust { get; set; }

    /// <summary>
    ///     Number of "Double Kill" streaks.
    /// </summary>
    [PhpProperty("doublekill")]
    public required string DoubleKill { get; set; }

    /// <summary>
    ///     Number of "Triple Kill" streaks.
    /// </summary>
    [PhpProperty("triplekill")]
    public required string TripleKill { get; set; }

    /// <summary>
    ///     Number of "Quad Kill" streaks.
    /// </summary>
    [PhpProperty("quadkill")]
    public required string QuadKill { get; set; }

    /// <summary>
    ///     Number of "Annihilation" (Penta Kill) streaks.
    /// </summary>
    [PhpProperty("annihilation")]
    public required string Annihilation { get; set; }

    /// <summary>
    ///     Number of 3-kill streaks (Serial Killer).
    /// </summary>
    [PhpProperty("ks3")]
    public required string KillStreak3 { get; set; }

    /// <summary>
    ///     Number of 4-kill streaks (Ultimate Warrior).
    /// </summary>
    [PhpProperty("ks4")]
    public required string KillStreak4 { get; set; }

    /// <summary>
    ///     Number of 5-kill streaks (Legendary).
    /// </summary>
    [PhpProperty("ks5")]
    public required string KillStreak5 { get; set; }

    /// <summary>
    ///     Number of 6-kill streaks (Onslaught).
    /// </summary>
    [PhpProperty("ks6")]
    public required string KillStreak6 { get; set; }

    /// <summary>
    ///     Number of 7-kill streaks (Savage Sick).
    /// </summary>
    [PhpProperty("ks7")]
    public required string KillStreak7 { get; set; }

    /// <summary>
    ///     Number of 8-kill streaks (Dominating).
    /// </summary>
    [PhpProperty("ks8")]
    public required string KillStreak8 { get; set; }

    /// <summary>
    ///     Number of 9-kill streaks (Champion of Newerth).
    /// </summary>
    [PhpProperty("ks9")]
    public required string KillStreak9 { get; set; }

    /// <summary>
    ///     Number of 10-kill streaks (Bloodbath).
    /// </summary>
    [PhpProperty("ks10")]
    public required string KillStreak10 { get; set; }

    /// <summary>
    ///     Number of 15-kill streaks (Immortal).
    /// </summary>
    [PhpProperty("ks15")]
    public required string KillStreak15 { get; set; }

    /// <summary>
    ///     Number of Smackdowns performed (killing taunted enemy).
    /// </summary>
    [PhpProperty("smackdown")]
    public required string SmackDown { get; set; }

    /// <summary>
    ///     Number of Humiliations suffered (being killed while taunting?).
    /// </summary>
    [PhpProperty("humiliation")]
    public required string Humiliation { get; set; }

    /// <summary>
    ///     Count of specific nemesis kills.
    /// </summary>
    [PhpProperty("nemesis")]
    public required string Nemesis { get; set; }

    /// <summary>
    ///     Count of retribution kills.
    /// </summary>
    [PhpProperty("retribution")]
    public required string Retribution { get; set; }

    /// <summary>
    ///     "1" if a token was used for this match?
    /// </summary>
    [PhpProperty("used_token")]
    public required string UsedToken { get; set; }

    /// <summary>
    ///     The client version or name used.
    /// </summary>
    [PhpProperty("cli_name")]
    public required string ClientName { get; set; }

    /// <summary>
    ///     Clan tag of the player.
    /// </summary>
    [PhpProperty("tag")]
    public required string Tag { get; set; }

    /// <summary>
    ///     Current nickname of the player.
    /// </summary>
    [PhpProperty("nickname")]
    public required string Nickname { get; set; }

    /// <summary>
    ///     The name of the alternative avatar used for the hero.
    /// </summary>
    [PhpProperty("alt_avatar_name")]
    public required string AltAvatarName { get; set; }

    /// <summary>
    ///     Campaign or seasonal progression info for this player.
    /// </summary>
    [PhpProperty("campaign_info")]
    public required MatchPlayerCampaignInfo CampaignInfo { get; set; }
}

public class MatchPlayerCampaignInfo
{
    /// <summary>
    ///     The unique identifier for the match.
    /// </summary>
    [PhpProperty("match_id")]
    public required string MatchID { get; set; }

    /// <summary>
    ///     The player's account ID.
    /// </summary>
    [PhpProperty("account_id")]
    public required string AccountID { get; set; }

    /// <summary>
    ///     "1" if the match was casual mode.
    /// </summary>
    [PhpProperty("is_casual")]
    public required string IsCasual { get; set; }

    /// <summary>
    ///     MMR before the match.
    /// </summary>
    [PhpProperty("mmr_before")]
    public required string MmrBefore { get; set; }

    /// <summary>
    ///     MMR after the match.
    /// </summary>
    [PhpProperty("mmr_after")]
    public required string MmrAfter { get; set; }

    /// <summary>
    ///     Rank medal before the match.
    /// </summary>
    [PhpProperty("medal_before")]
    public required string MedalBefore { get; set; }

    /// <summary>
    ///     Rank medal after the match.
    /// </summary>
    [PhpProperty("medal_after")]
    public required string MedalAfter { get; set; }

    /// <summary>
    ///     The current season identifier.
    /// </summary>
    [PhpProperty("season")]
    public required string Season { get; set; }

    /// <summary>
    ///     Number of placement matches played.
    /// </summary>
    [PhpProperty("placement_matches")]
    public int PlacementMatches { get; set; }

    /// <summary>
    ///     Number of wins during placement.
    /// </summary>
    [PhpProperty("placement_wins")]
    public required string PlacementWins { get; set; }
}

public class MatchInventory
{
    /// <summary>
    ///     The player's account ID.
    /// </summary>
    [PhpProperty("account_id")]
    public required string AccountID { get; set; }

    /// <summary>
    ///     The unique identifier for the match.
    /// </summary>
    [PhpProperty("match_id")]
    public required string MatchID { get; set; }

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
    public required object Slot3 { get; set; }

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

public class ConReward
{
    /// <summary>
    ///     Level before the match/reward.
    /// </summary>
    [PhpProperty("old_lvl")]
    public int OldLevel { get; set; }

    /// <summary>
    ///     Current level after the match.
    /// </summary>
    [PhpProperty("curr_lvl")]
    public int CurrentLevel { get; set; }

    /// <summary>
    ///     Next level goal.
    /// </summary>
    [PhpProperty("next_lvl")]
    public int NextLevel { get; set; }

    /// <summary>
    ///     Rank required for something?
    /// </summary>
    [PhpProperty("require_rank")]
    public int RequireRank { get; set; }

    /// <summary>
    ///     Matches needed to play to unlock something.
    /// </summary>
    [PhpProperty("need_more_play")]
    public int NeedMorePlay { get; set; }

    /// <summary>
    ///     Experience percentage towards next level before match.
    /// </summary>
    [PhpProperty("percentage_before")]
    public required string PercentageBefore { get; set; }

    /// <summary>
    ///     Experience percentage towards next level after match.
    /// </summary>
    [PhpProperty("percentage")]
    public required string Percentage { get; set; }
}

public class MatchMastery
{
    /// <summary>
    ///     Client name/version.
    /// </summary>
    [PhpProperty("cli_name")]
    public required string ClientName { get; set; }

    /// <summary>
    ///     Base mastery experience.
    /// </summary>
    [PhpProperty("mastery_exp_original")]
    public int MasteryExpOriginal { get; set; }

    /// <summary>
    ///     Mastery experience earned from the match.
    /// </summary>
    [PhpProperty("mastery_exp_match")]
    public int MasteryExpMatch { get; set; }

    /// <summary>
    ///     Bonus mastery experience earned.
    /// </summary>
    [PhpProperty("mastery_exp_bonus")]
    public int MasteryExpBonus { get; set; }

    /// <summary>
    ///     Experience from boosts.
    /// </summary>
    [PhpProperty("mastery_exp_boost")]
    public int MasteryExpBoost { get; set; }

    /// <summary>
    ///     Experience from super boosts.
    /// </summary>
    [PhpProperty("mastery_exp_super_boost")]
    public int MasteryExpSuperBoost { get; set; }

    /// <summary>
    ///     Experience related to hero count.
    /// </summary>
    [PhpProperty("mastery_exp_heroes_count")]
    public int MasteryExpHeroesCount { get; set; }

    /// <summary>
    ///     Add-on experience for heroes.
    /// </summary>
    [PhpProperty("mastery_exp_heroes_addon")]
    public int MasteryExpHeroesAddon { get; set; }

    /// <summary>
    ///     Experience applied to boost?
    /// </summary>
    [PhpProperty("mastery_exp_to_boost")]
    public int MasteryExpToBoost { get; set; }

    /// <summary>
    ///     Event-specific mastery experience.
    /// </summary>
    [PhpProperty("mastery_exp_event")]
    public int MasteryExpEvent { get; set; }

    /// <summary>
    ///     Can mastery be boosted?
    /// </summary>
    [PhpProperty("mastery_canboost")]
    public bool MasteryCanBoost { get; set; }

    /// <summary>
    ///     Can mastery be super boosted?
    /// </summary>
    [PhpProperty("mastery_super_canboost")]
    public bool MasterySuperCanBoost { get; set; }

    /// <summary>
    ///     Product ID for the mastery boost.
    /// </summary>
    [PhpProperty("mastery_boost_product_id")]
    public int MasteryBoostProductID { get; set; }

    /// <summary>
    ///     Product ID for the super mastery boost.
    /// </summary>
    [PhpProperty("mastery_super_boost_product_id")]
    public int MasterySuperBoostProductID { get; set; }

    /// <summary>
    ///     Number of mastery boosts applied.
    /// </summary>
    [PhpProperty("mastery_boostnum")]
    public int MasteryBoostNum { get; set; }

    /// <summary>
    ///     Number of super mastery boosts applied.
    /// </summary>
    [PhpProperty("mastery_super_boostnum")]
    public int MasterySuperBoostNum { get; set; }
}

public class SeasonSystem
{
    /// <summary>
    ///     Diamonds dropped/earned?
    /// </summary>
    [PhpProperty("drop_diamonds")]
    public int DropDiamonds { get; set; }

    /// <summary>
    ///     Current diamond balance.
    /// </summary>
    [PhpProperty("cur_diamonds")]
    public int CurDiamonds { get; set; }

    /// <summary>
    ///     Pricing for boxes/chests?
    /// </summary>
    [PhpProperty("box_price")]
    public required object[] BoxPrice { get; set; }
}
