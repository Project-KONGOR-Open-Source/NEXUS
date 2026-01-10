namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class MatchSummary(
    MatchStatistics matchStatistics,
    List<PlayerStatistics> playerStatistics,
    MatchStartData matchStartData)
{
    /// <summary>
    ///     The unique identifier for the match.
    /// </summary>
    [PhpProperty("match_id")]
    public int MatchID { get; init; } = matchStatistics.MatchID;

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
    public string FileHost { get; init; } = Environment.GetEnvironmentVariable("INFRASTRUCTURE_GATEWAY") ??
                                            throw new NullReferenceException("Infrastructure Gateway Is NULL");

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
    ///     The match name.
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
    public int Class { get; init; } = matchStartData.MatchType;

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
    ///         League matches belong to a dedicated competitive league structure with player rosters, seasonal standings, and
    ///         separate win/loss tracking.
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
    public string NoAgilityHeroes { get; init; } =
        matchStartData.Options.HasFlag(MatchOptions.NoAgilityHeroes) ? "1" : "0";

    /// <summary>
    ///     Drop Items On Death option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("drp_itm")]
    public string DropItems { get; init; } = matchStartData.Options.HasFlag(MatchOptions.DropItems) ? "1" : "0";

    /// <summary>
    ///     No Respawn Timer option flag (1 = picking timer disabled, 0 = timer enabled).
    /// </summary>
    [PhpProperty("no_timer")]
    public string NoRespawnTimer { get; init; } =
        matchStartData.Options.HasFlag(MatchOptions.NoRespawnTimer) ? "1" : "0";

    /// <summary>
    ///     Reverse Hero Selection option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("rev_hs")]
    public string ReverseHeroSelection { get; init; } =
        matchStartData.Options.HasFlag(MatchOptions.ReverseHeroSelection) ? "1" : "0";

    /// <summary>
    ///     No Swap option flag (1 = hero swapping disabled, 0 = swapping allowed).
    /// </summary>
    [PhpProperty("no_swap")]
    public string NoHeroSwap { get; init; } = matchStartData.Options.HasFlag(MatchOptions.NoHeroSwap) ? "1" : "0";

    /// <summary>
    ///     No Intelligence Heroes option flag (1 = intelligence heroes banned, 0 = allowed).
    /// </summary>
    [PhpProperty("no_int")]
    public string NoIntelligenceHeroes { get; init; } =
        matchStartData.Options.HasFlag(MatchOptions.NoIntelligenceHeroes) ? "1" : "0";

    /// <summary>
    ///     Alternate Picking option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("alt_pick")]
    public string AlternateHeroPicking { get; init; } =
        matchStartData.Options.HasFlag(MatchOptions.AlternateHeroPicking) ? "1" : "0";

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
    public string NoStrengthHeroes { get; init; } =
        matchStartData.Options.HasFlag(MatchOptions.NoStrengthHeroes) ? "1" : "0";

    /// <summary>
    ///     No Power-Ups option flag (1 = power-ups/runes disabled, 0 = enabled).
    /// </summary>
    [PhpProperty("no_pups")]
    public string NoPowerUps { get; init; } = matchStartData.Options.HasFlag(MatchOptions.NoPowerUps) ? "1" : "0";

    /// <summary>
    ///     Duplicate Heroes option flag (1 = duplicate heroes allowed, 0 = each hero unique).
    /// </summary>
    [PhpProperty("dup_h")]
    public string DuplicateHeroes { get; init; } =
        matchStartData.Options.HasFlag(MatchOptions.DuplicateHeroes) ? "1" : "0";

    /// <summary>
    ///     All Pick Mode option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("ap")]
    public string AllPick { get; init; } = matchStartData.Options.HasFlag(MatchOptions.AllPick) ? "1" : "0";

    /// <summary>
    ///     Balanced Random Mode option flag (1 = enabled, 0 = disabled).
    /// </summary>
    [PhpProperty("br")]
    public string BalancedRandom { get; init; } =
        matchStartData.Options.HasFlag(MatchOptions.BalancedRandom) ? "1" : "0";

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
    public string ReverseSelection { get; init; } =
        matchStartData.Options.HasFlag(MatchOptions.ReverseSelection) ? "1" : "0";

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
    public string DevelopmentHeroes { get; init; } =
        matchStartData.Options.HasFlag(MatchOptions.DevelopmentHeroes) ? "1" : "0";

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
    public string Timestamp { get; init; } = Convert
        .ToInt32(Math.Max(matchStatistics.TimestampRecorded.ToUnixTimeSeconds(), Convert.ToInt64(int.MaxValue)))
        .ToString();

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
    public string AwardLongestKillingSpree { get; init; } =
        GetLongestKillingSpreeAwardRecipientID(playerStatistics).ToString();

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
        {
            return playerStatistics.OrderByDescending(player => player.WardsPlaced)
                .ThenByDescending(player => player.Experience).First().ID;
        }

        return -1;
    }

    private static int GetLongestKillingSpreeAwardRecipientID(List<PlayerStatistics> playerStatistics)
    {
        if (playerStatistics.Where(player => player.KillStreak15 > 0).Any())
        {
            return playerStatistics.OrderByDescending(player => player.KillStreak15)
                .ThenByDescending(player => player.Experience).First().ID;
        }

        if (playerStatistics.Where(player => player.KillStreak10 > 0).Any())
        {
            return playerStatistics.OrderByDescending(player => player.KillStreak10)
                .ThenByDescending(player => player.Experience).First().ID;
        }

        if (playerStatistics.Where(player => player.KillStreak09 > 0).Any())
        {
            return playerStatistics.OrderByDescending(player => player.KillStreak05)
                .ThenByDescending(player => player.Experience).First().ID;
        }

        if (playerStatistics.Where(player => player.KillStreak08 > 0).Any())
        {
            return playerStatistics.OrderByDescending(player => player.KillStreak05)
                .ThenByDescending(player => player.Experience).First().ID;
        }

        if (playerStatistics.Where(player => player.KillStreak07 > 0).Any())
        {
            return playerStatistics.OrderByDescending(player => player.KillStreak05)
                .ThenByDescending(player => player.Experience).First().ID;
        }

        if (playerStatistics.Where(player => player.KillStreak06 > 0).Any())
        {
            return playerStatistics.OrderByDescending(player => player.KillStreak05)
                .ThenByDescending(player => player.Experience).First().ID;
        }

        if (playerStatistics.Where(player => player.KillStreak05 > 0).Any())
        {
            return playerStatistics.OrderByDescending(player => player.KillStreak05)
                .ThenByDescending(player => player.Experience).First().ID;
        }

        if (playerStatistics.Where(player => player.KillStreak04 > 0).Any())
        {
            return playerStatistics.OrderByDescending(player => player.KillStreak05)
                .ThenByDescending(player => player.Experience).First().ID;
        }

        if (playerStatistics.Where(player => player.KillStreak03 > 0).Any())
        {
            return playerStatistics.OrderByDescending(player => player.KillStreak05)
                .ThenByDescending(player => player.Experience).First().ID;
        }

        return -1;
    }

    private static int GetWinningTeam(List<PlayerStatistics>? playerStatistics)
    {
        if (playerStatistics is null)
        {
            return 0;
        }

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
        return publicStatuses.Count == 1 ? publicStatuses.Single().PublicMatch is 0 ? 1 : 0 : 0;
    }
}