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
    public required Dictionary<int, PlayerStatistics> PlayerStatistics { get; set; }

    /// <summary>
    ///     A dictionary of player inventories for the match, keyed by the player's account ID.
    /// </summary>
    [PhpProperty("inventory")]
    public required Dictionary<int, PlayerInventory> PlayerInventories { get; set; }

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
    public Dictionary<string, QuestSystem> QuestSystem { get; set; } = new () { { "error", new QuestSystem() } };

    /// <summary>
    ///     Unused.
    ///     <br/>
    ///     Statistics related to the "Event Codex" (otherwise known as "Ascension") seasonal system.
    /// </summary>
    [PhpProperty("season_system")]
    public SeasonSystem SeasonSystem { get; set; } = new ();

    /// <summary>
    ///     Statistics related to the Champions Of Newerth seasonal campaign.
    /// </summary>
    [PhpProperty("con_reward")]
    public required CampaignReward CampaignReward { get; set; } = new ();

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

public class MatchSummary
{
    /// <summary>
    ///     The unique identifier for the match.
    /// </summary>
    [PhpProperty("match_id")]
    public required int MatchID { get; set; }

    /// <summary>
    ///     The server ID where the match was hosted.
    /// </summary>
    [PhpProperty("server_id")]
    public required int ServerID { get; set; }

    /// <summary>
    ///     The map on which the match was played (e.g. "caldavar", "midwars", "grimms_crossing").
    /// </summary>
    [PhpProperty("map")]
    public required string Map { get; set; }

    /// <summary>
    ///     The version of the map used in the match.
    /// </summary>
    [PhpProperty("map_version")]
    public required string MapVersion { get; set; }

    /// <summary>
    ///     The duration of the match in seconds.
    /// </summary>
    [PhpProperty("time_played")]
    public required int TimePlayed { get; set; }

    /// <summary>
    ///     The host where the match replay file is stored.
    /// </summary>
    [PhpProperty("file_host")]
    public string? FileHost { get; set; }

    /// <summary>
    ///     The size of the match replay file in bytes.
    /// </summary>
    [PhpProperty("file_size")]
    public int? FileSize { get; set; }

    /// <summary>
    ///     The filename of the match replay file.
    /// </summary>
    [PhpProperty("file_name")]
    public string? FileName { get; set; }

    /// <summary>
    ///     The connection state or match state code.
    /// </summary>
    [PhpProperty("c_state")]
    public int? ConnectionState { get; set; }

    /// <summary>
    ///     The game client version used for the match.
    /// </summary>
    [PhpProperty("version")]
    public required string Version { get; set; }

    /// <summary>
    ///     The average Player Skill Rating (PSR) of all players in the match.
    /// </summary>
    [PhpProperty("avgpsr")]
    public required int AveragePSR { get; set; }

    /// <summary>
    ///     The match date, originally formatted as "M/D/YYYY" (e.g. "3/15/2024").
    /// </summary>
    [PhpProperty("date")]
    public required string Date { get; set; }

    /// <summary>
    ///     The match time, originally formatted in 12-hour format with AM/PM (e.g. "2:30:45 PM").
    /// </summary>
    [PhpProperty("time")]
    public required string Time { get; set; }

    /// <summary>
    ///     The match name or custom server name.
    /// </summary>
    [PhpProperty("mname")]
    public string? MatchName { get; set; }

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
    public required int Class { get; set; }

    /// <summary>
    ///     Whether the match was private (1) or public (0).
    /// </summary>
    [PhpProperty("private")]
    public required int Private { get; set; }

    /// <summary>
    ///     Normal Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("nm")]
    public required int NormalMode { get; set; }

    /// <summary>
    ///     Single Draft Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("sd")]
    public required int SingleDraft { get; set; }

    /// <summary>
    ///     Random Draft Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("rd")]
    public required int RandomDraft { get; set; }

    /// <summary>
    ///     Death Match Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("dm")]
    public required int DeathMatch { get; set; }

    /// <summary>
    ///     Banning Draft Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("bd")]
    public required int BanningDraft { get; set; }

    /// <summary>
    ///     Banning Pick Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("bp")]
    public required int BanningPick { get; set; }

    /// <summary>
    ///     All Random Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("ar")]
    public required int AllRandom { get; set; }

    /// <summary>
    ///     Captains Draft Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("cd")]
    public required int CaptainsDraft { get; set; }

    /// <summary>
    ///     Captains Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("cm")]
    public required int CaptainsMode { get; set; }

    /// <summary>
    ///     Lock Pick Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("lp")]
    public required int LockPick { get; set; }

    /// <summary>
    ///     Blind Ban Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("bb")]
    public required int BlindBan { get; set; }

    /// <summary>
    ///     Balanced Mode flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("bm")]
    public required int BalancedMode { get; set; }

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
    public required int KrosMode { get; set; }

    /// <summary>
    ///     Whether the match was a league/ranked match (1) or casual (0).
    /// </summary>
    [PhpProperty("league")]
    public required int League { get; set; }

    /// <summary>
    ///     The maximum number of players allowed in the match (typically 2, 4, 6, 8, or 10).
    /// </summary>
    [PhpProperty("max_players")]
    public required int MaxPlayers { get; set; }

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
    public required int Tier { get; set; }

    /// <summary>
    ///     No Repick option flag (1 = repicking disabled, 0 = repicking allowed).
    /// </summary>
    [PhpProperty("no_repick")]
    public required int NoRepick { get; set; }

    /// <summary>
    ///     No Agility Heroes option flag (1 = agility heroes banned, 0 = allowed).
    /// </summary>
    [PhpProperty("no_agi")]
    public required int NoAgility { get; set; }

    /// <summary>
    ///     Drop Items On Death option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("drp_itm")]
    public required int DropItems { get; set; }

    /// <summary>
    ///     No Respawn Timer option flag (1 = picking timer disabled, 0 = timer enabled).
    /// </summary>
    [PhpProperty("no_timer")]
    public required int NoRespawnTimer { get; set; }

    /// <summary>
    ///     Reverse Hero Selection option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("rev_hs")]
    public required int ReverseHeroSelection { get; set; }

    /// <summary>
    ///     No Swap option flag (1 = hero swapping disabled, 0 = swapping allowed).
    /// </summary>
    [PhpProperty("no_swap")]
    public required int NoSwap { get; set; }

    /// <summary>
    ///     No Intelligence Heroes option flag (1 = intelligence heroes banned, 0 = allowed).
    /// </summary>
    [PhpProperty("no_int")]
    public required int NoIntelligence { get; set; }

    /// <summary>
    ///     Alternate Picking option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("alt_pick")]
    public required int AlternatePicking { get; set; }

    /// <summary>
    ///     Ban Phase option flag (1 = ban phase enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("veto")]
    public required int BanPhase { get; set; }

    /// <summary>
    ///     Shuffle Abilities option flag (1 = abilities shuffled/randomised, 0 = normal abilities).
    ///     Used in Rift Wars and other Kros Mode variants.
    /// </summary>
    [PhpProperty("shuf")]
    public required int ShuffleAbilities { get; set; }

    /// <summary>
    ///     No Strength Heroes option flag (1 = strength heroes banned, 0 = allowed).
    /// </summary>
    [PhpProperty("no_str")]
    public required int NoStrength { get; set; }

    /// <summary>
    ///     No Power-Ups option flag (1 = power-ups/runes disabled, 0 = enabled).
    /// </summary>
    [PhpProperty("no_pups")]
    public required int NoPowerUps { get; set; }

    /// <summary>
    ///     Duplicate Heroes option flag (1 = duplicate heroes allowed, 0 = each hero unique).
    /// </summary>
    [PhpProperty("dup_h")]
    public required int DuplicateHeroes { get; set; }

    /// <summary>
    ///     All Pick Mode option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("ap")]
    public required int AllPick { get; set; }

    /// <summary>
    ///     Balanced Random Mode option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("br")]
    public required int BalancedRandom { get; set; }

    /// <summary>
    ///     Easy Mode option flag (1 = easy mode enabled, 0 = normal difficulty).
    /// </summary>
    [PhpProperty("em")]
    public required int EasyMode { get; set; }

    /// <summary>
    ///     Casual Mode option flag (1 = casual mode enabled, 0 = normal mode).
    /// </summary>
    [PhpProperty("cas")]
    public required int CasualMode { get; set; }

    /// <summary>
    ///     Reverse Selection option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("rs")]
    public required int ReverseSelection { get; set; }

    /// <summary>
    ///     No Leaver option flag (1 = no leaver penalty applied, 0 = leaver penalties enabled).
    /// </summary>
    [PhpProperty("nl")]
    public required int NoLeaver { get; set; }

    /// <summary>
    ///     Official Match flag (1 = official tournament match, 0 = unofficial).
    /// </summary>
    [PhpProperty("officl")]
    public required int Official { get; set; }

    /// <summary>
    ///     No Statistics option flag (1 = match stats not recorded, 0 = stats recorded).
    /// </summary>
    [PhpProperty("no_stats")]
    public required int NoStatistics { get; set; }

    /// <summary>
    ///     Auto Balance option flag (1 = teams automatically balanced, 0 = manual teams).
    /// </summary>
    [PhpProperty("ab")]
    public required int AutoBalance { get; set; }

    /// <summary>
    ///     Hardcore Mode option flag (1 = hardcore difficulty enabled, 0 = normal).
    /// </summary>
    [PhpProperty("hardcore")]
    public required int Hardcore { get; set; }

    /// <summary>
    ///     Development Heroes option flag (1 = development/unreleased heroes allowed, 0 = only released heroes).
    /// </summary>
    [PhpProperty("dev_heroes")]
    public required int DevelopmentHeroes { get; set; }

    /// <summary>
    ///     Verified Only option flag (1 = only verified accounts allowed, 0 = all accounts allowed).
    /// </summary>
    [PhpProperty("verified_only")]
    public required int VerifiedOnly { get; set; }

    /// <summary>
    ///     Gated option flag (1 = gated/restricted match, 0 = open match).
    /// </summary>
    [PhpProperty("gated")]
    public required int Gated { get; set; }

    /// <summary>
    ///     Rapid Fire Mode option flag (1 = rapid fire mode enabled, 0 = normal ability cooldowns).
    /// </summary>
    [PhpProperty("rapidfire")]
    public int RapidFire { get; set; }

    /// <summary>
    ///     The UNIX timestamp (in seconds) when the match started.
    /// </summary>
    [PhpProperty("timestamp")]
    public required int Timestamp { get; set; }

    /// <summary>
    ///     The URL for the match replay file.
    /// </summary>
    [PhpProperty("url")]
    public required string URL { get; set; }

    /// <summary>
    ///     The size of the match replay file (human-readable format or bytes as string).
    /// </summary>
    [PhpProperty("size")]
    public required string Size { get; set; }

    /// <summary>
    ///     The name or title of the replay file.
    /// </summary>
    [PhpProperty("name")]
    public required string Name { get; set; }

    /// <summary>
    ///     The directory path where the replay file is stored.
    /// </summary>
    [PhpProperty("dir")]
    public required string Directory { get; set; }

    /// <summary>
    ///     The S3 download URL for the match replay file.
    /// </summary>
    [PhpProperty("s3_url")]
    public required string S3URL { get; set; }

    /// <summary>
    ///     The winning team ("1" for Legion, "2" for Hellbourne).
    ///     Determined by analysing player statistics from the match.
    /// </summary>
    [PhpProperty("winning_team")]
    public required string WinningTeam { get; set; }

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
    public required string GameMode { get; set; }

    /// <summary>
    ///     The account ID of the Most Valuable Player (MVP) in the match.
    /// </summary>
    [PhpProperty("mvp")]
    public required string MVP { get; set; }

    /// <summary>
    ///     The account ID of the player who earned the "Most Annihilations" award.
    /// </summary>
    [PhpProperty("awd_mann")]
    public required string AwardMostAnnihilations { get; set; }

    /// <summary>
    ///     The account ID of the player who earned the "Most Quad Kills" award.
    /// </summary>
    [PhpProperty("awd_mqk")]
    public required string AwardMostQuadKills { get; set; }

    /// <summary>
    ///     The account ID of the player who earned the "Longest Killing Spree" award.
    /// </summary>
    [PhpProperty("awd_lgks")]
    public required string AwardLongestKillingSpree { get; set; }

    /// <summary>
    ///     The account ID of the player who earned the "Most Smackdowns" award.
    /// </summary>
    [PhpProperty("awd_msd")]
    public required string AwardMostSmackdowns { get; set; }

    /// <summary>
    ///     The account ID of the player who earned the "Most Kills" award.
    /// </summary>
    [PhpProperty("awd_mkill")]
    public required string AwardMostKills { get; set; }

    /// <summary>
    ///     The account ID of the player who earned the "Most Assists" award.
    /// </summary>
    [PhpProperty("awd_masst")]
    public required string AwardMostAssists { get; set; }

    /// <summary>
    ///     The account ID of the player who earned the "Least Deaths" award.
    /// </summary>
    [PhpProperty("awd_ledth")]
    public required string AwardLeastDeaths { get; set; }

    /// <summary>
    ///     The account ID of the player who earned the "Most Building Damage" award.
    /// </summary>
    [PhpProperty("awd_mbdmg")]
    public required string AwardMostBuildingDamage { get; set; }

    /// <summary>
    ///     The account ID of the player who earned the "Most Wards" award.
    /// </summary>
    [PhpProperty("awd_mwk")]
    public required string AwardMostWards { get; set; }

    /// <summary>
    ///     The account ID of the player who earned the "Most Hero Damage Dealt" award.
    /// </summary>
    [PhpProperty("awd_mhdd")]
    public required string AwardMostHeroDamageDealt { get; set; }

    /// <summary>
    ///     The account ID of the player who earned the "Highest Creep Score" award.
    /// </summary>
    [PhpProperty("awd_hcs")]
    public required string AwardHighestCreepScore { get; set; }
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

public class PlayerStatistics
{
    public string match_id { get; set; }
    public string account_id { get; set; }
    public string clan_id { get; set; }
    public string hero_id { get; set; }
    public string position { get; set; }
    public string team { get; set; }
    public string level { get; set; }
    public string wins { get; set; }
    public string losses { get; set; }
    public string concedes { get; set; }
    public string concedevotes { get; set; }
    public string buybacks { get; set; }
    public string discos { get; set; }
    public string kicked { get; set; }
    public string pub_skill { get; set; }
    public string pub_count { get; set; }
    public string amm_solo_rating { get; set; }
    public string amm_solo_count { get; set; }
    public string amm_team_rating { get; set; }
    public string amm_team_count { get; set; }
    public string avg_score { get; set; }
    public string herokills { get; set; }
    public string herodmg { get; set; }
    public string heroexp { get; set; }
    public string herokillsgold { get; set; }
    public string heroassists { get; set; }
    public string deaths { get; set; }
    public string goldlost2death { get; set; }
    public string secs_dead { get; set; }
    public string teamcreepkills { get; set; }
    public string teamcreepdmg { get; set; }
    public string teamcreepexp { get; set; }
    public string teamcreepgold { get; set; }
    public string neutralcreepkills { get; set; }
    public string neutralcreepdmg { get; set; }
    public string neutralcreepexp { get; set; }
    public string neutralcreepgold { get; set; }
    public string bdmg { get; set; }
    public string bdmgexp { get; set; }
    public string razed { get; set; }
    public string bgold { get; set; }
    public string denies { get; set; }
    public string exp_denied { get; set; }
    public string gold { get; set; }
    public string gold_spent { get; set; }
    public string exp { get; set; }
    public string actions { get; set; }
    public string secs { get; set; }
    public string consumables { get; set; }
    public string wards { get; set; }
    public string time_earning_exp { get; set; }
    public string bloodlust { get; set; }
    public string doublekill { get; set; }
    public string triplekill { get; set; }
    public string quadkill { get; set; }
    public string annihilation { get; set; }
    public string ks3 { get; set; }
    public string ks4 { get; set; }
    public string ks5 { get; set; }
    public string ks6 { get; set; }
    public string ks7 { get; set; }
    public string ks8 { get; set; }
    public string ks9 { get; set; }
    public string ks10 { get; set; }
    public string ks15 { get; set; }
    public string smackdown { get; set; }
    public string humiliation { get; set; }
    public string nemesis { get; set; }
    public string retribution { get; set; }
    public string used_token { get; set; }
    public string cli_name { get; set; }
    public string tag { get; set; }
    public string nickname { get; set; }
    public string alt_avatar_name { get; set; }

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

    public string is_casual { get; set; }
    public string mmr_before { get; set; }
    public string mmr_after { get; set; }
    public string medal_before { get; set; }
    public string medal_after { get; set; }
    public string season { get; set; }
    public int placement_matches { get; set; }
    public string placement_wins { get; set; }
}

public class PlayerInventory
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
