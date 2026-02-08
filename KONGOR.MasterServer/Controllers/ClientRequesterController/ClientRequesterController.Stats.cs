namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> GetSeasons()
    {
        int[] seasons = [ 666 ];

        GetSeasonsResponse response = new ()
        {
            AllSeasons = string.Join("|", seasons.Select(season => $"{season},0|{season},1"))
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> GetSimpleStatistics()
    {
        string? accountName = Request.Form["nickname"];

        if (accountName is null)
            return BadRequest(@"Missing Value For Form Parameter ""nickname""");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound($@"Account With Name ""{accountName}"" Was Not Found");

        ShowSimpleStatsResponse response = new ()
        {
            NameWithClanTag = account.NameWithClanTag,
            ID = account.ID.ToString(),
            Level = account.User.TotalLevel,
            LevelExperience = account.User.TotalExperience,
            NumberOfAvatarsOwned = account.User.OwnedStoreItems.Count(item => item.StartsWith("aa.")),
            TotalMatchesPlayed = 5555, // TODO: Implement Matches Played
            CurrentSeason = 666,
            SimpleSeasonStats = new SimpleSeasonStats() // TODO: Implement Stats
            {
                RankedMatchesWon = 1001 /* ranked */ + 1001 /* ranked casual */,
                RankedMatchesLost = 1002 /* ranked */ + 1002 /* ranked casual */,
                WinStreak = Math.Max(1003 /* ranked */, 1003 /* ranked casual */),
                InPlacementPhase = 0, // TODO: Implement Placement Matches
                LevelsGainedThisSeason = account.User.TotalLevel
            },
            SimpleCasualSeasonStats = new SimpleSeasonStats() // TODO: Implement Stats
            {
                RankedMatchesWon = 1001 /* ranked */ + 1001 /* ranked casual */,
                RankedMatchesLost = 1002 /* ranked */ + 1002 /* ranked casual */,
                WinStreak = Math.Max(1003 /* ranked */, 1003 /* ranked casual */),
                InPlacementPhase = 0, // TODO: Implement Placement Matches
                LevelsGainedThisSeason = account.User.TotalLevel
            },
            MVPAwardsCount = 1004,
            Top4AwardNames = [ "awd_masst", "awd_mhdd", "awd_mbdmg", "awd_lgks" ], // TODO: Implement Awards
            Top4AwardCounts = [ 1005, 1006, 1007, 1008 ], // TODO: Implement Awards
            CustomIconSlotID = SetCustomIconSlotID(account),
            OwnedStoreItems = account.User.OwnedStoreItems,
            SelectedStoreItems = account.SelectedStoreItems,
            OwnedStoreItemsData = SetOwnedStoreItemsData(account)
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> GetStatistics()
    {
        string? accountName = Request.Form["nickname"];

        if (accountName is null)
            return BadRequest(@"Missing Value For Form Parameter ""nickname""");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound($@"Account With Name ""{accountName}"" Was Not Found");

        string? table = Request.Form["table"];

        if (table is null)
            return BadRequest(@"Missing Value For Form Parameter ""table""");

        // Fetch All Statistics For The Account To Build Aggregates
        List<AccountStatistics> allStatistics = await MerrickContext.AccountStatistics
            .Where(statistics => statistics.AccountID == account.ID)
            .ToListAsync();

        Dictionary<AccountStatisticsType, AccountStatistics> statisticsByType = allStatistics.ToDictionary(statistics => statistics.Type);

        AggregateStatistics aggregates = AggregateStatistics.FromStatistics(statisticsByType);

        if (table is "player")
        {
            AccountStatistics statistics = statisticsByType[AccountStatisticsType.Public];

            PlayerStatisticsResponse response = new(account, statistics, aggregates);

            string realResponse = PhpSerialization.Serialize(response);
            string mockResponse = @"a:144:{s:8:""super_id"";s:6:""195592"";s:8:""nickname"";s:7:""[K]GOPO"";s:8:""standing"";s:1:""3"";s:12:""account_type"";s:1:""4"";s:10:""account_id"";s:6:""195592"";s:16:""acc_games_played"";s:3:""552"";s:8:""acc_wins"";s:3:""263"";s:10:""acc_losses"";s:3:""289"";s:12:""acc_concedes"";s:3:""159"";s:16:""acc_concedevotes"";s:2:""34"";s:12:""acc_buybacks"";s:2:""15"";s:10:""acc_discos"";s:1:""1"";s:10:""acc_kicked"";s:1:""0"";s:13:""acc_pub_skill"";s:8:""1422.016"";s:13:""acc_pub_count"";s:3:""547"";s:12:""acc_pub_pset"";s:1:""1"";s:13:""acc_avg_score"";s:4:""0.00"";s:13:""acc_herokills"";s:4:""2637"";s:11:""acc_herodmg"";s:7:""6163930"";s:11:""acc_heroexp"";s:7:""3491310"";s:17:""acc_herokillsgold"";s:6:""991114"";s:15:""acc_heroassists"";s:4:""4394"";s:10:""acc_deaths"";s:4:""3200"";s:18:""acc_goldlost2death"";s:6:""937751"";s:13:""acc_secs_dead"";s:6:""214784"";s:18:""acc_teamcreepkills"";s:5:""42422"";s:16:""acc_teamcreepdmg"";s:8:""19041020"";s:16:""acc_teamcreepexp"";s:7:""3614761"";s:17:""acc_teamcreepgold"";s:7:""1581296"";s:21:""acc_neutralcreepkills"";s:5:""10276"";s:19:""acc_neutralcreepdmg"";s:7:""7513193"";s:19:""acc_neutralcreepexp"";s:6:""756098"";s:20:""acc_neutralcreepgold"";s:6:""452299"";s:8:""acc_bdmg"";s:6:""577730"";s:11:""acc_bdmgexp"";s:5:""13300"";s:9:""acc_razed"";s:3:""550"";s:9:""acc_bgold"";s:6:""764373"";s:10:""acc_denies"";s:4:""5430"";s:14:""acc_exp_denied"";s:6:""233195"";s:8:""acc_gold"";s:7:""3946281"";s:14:""acc_gold_spent"";s:7:""4624947"";s:7:""acc_exp"";s:7:""7901395"";s:11:""acc_actions"";s:7:""1699513"";s:8:""acc_secs"";s:7:""1269820"";s:15:""acc_consumables"";s:4:""1471"";s:9:""acc_wards"";s:3:""171"";s:13:""acc_em_played"";s:1:""1"";s:20:""acc_time_earning_exp"";s:7:""1265988"";s:19:""acc_no_stats_played"";s:1:""0"";s:22:""acc_trial_games_played"";s:1:""0"";s:13:""acc_bloodlust"";s:2:""40"";s:14:""acc_doublekill"";s:3:""268"";s:14:""acc_triplekill"";s:2:""28"";s:12:""acc_quadkill"";s:1:""4"";s:16:""acc_annihilation"";s:1:""0"";s:7:""acc_ks3"";s:3:""272"";s:7:""acc_ks4"";s:3:""152"";s:7:""acc_ks5"";s:2:""79"";s:7:""acc_ks6"";s:2:""47"";s:7:""acc_ks7"";s:2:""29"";s:7:""acc_ks8"";s:2:""24"";s:7:""acc_ks9"";s:2:""18"";s:8:""acc_ks10"";s:1:""7"";s:8:""acc_ks15"";s:1:""2"";s:13:""acc_smackdown"";s:2:""25"";s:15:""acc_humiliation"";s:1:""0"";s:11:""acc_nemesis"";s:4:""1133"";s:15:""acc_retribution"";s:2:""18"";s:5:""level"";s:2:""58"";s:9:""level_exp"";i:173525;s:6:""discos"";s:2:""72"";s:15:""possible_discos"";s:1:""0"";s:12:""games_played"";s:4:""7353"";s:17:""num_bot_games_won"";s:2:""10"";s:18:""total_games_played"";i:8321;s:12:""total_discos"";i:49;s:8:""rnk_secs"";s:7:""7526246"";s:16:""rnk_games_played"";s:4:""3515"";s:10:""rnk_discos"";s:2:""12"";s:7:""cs_secs"";s:6:""112945"";s:15:""cs_games_played"";s:2:""61"";s:9:""cs_discos"";s:1:""2"";s:16:""mid_games_played"";s:4:""3646"";s:10:""mid_discos"";s:2:""30"";s:17:""rift_games_played"";s:1:""0"";s:11:""rift_discos"";s:1:""0"";s:13:""last_activity"";s:10:""05/26/2022"";s:11:""create_date"";s:10:""08/11/2009"";s:4:""name"";s:6:""KONGOR"";s:4:""rank"";s:6:""Leader"";s:8:""favHero1"";s:7:""armadon"";s:12:""favHero1Time"";d:6.1600000000000001;s:10:""favHero1_2"";s:12:""Hero_Armadon"";s:8:""favHero2"";s:10:""pestilence"";s:12:""favHero2Time"";d:4.1699999999999999;s:10:""favHero2_2"";s:15:""Hero_Pestilence"";s:8:""favHero3"";s:13:""diseasedrider"";s:12:""favHero3Time"";d:3.7999999999999998;s:10:""favHero3_2"";s:18:""Hero_DiseasedRider"";s:8:""favHero4"";s:6:""treant"";s:12:""favHero4Time"";d:3.6200000000000001;s:10:""favHero4_2"";s:11:""Hero_Treant"";s:8:""favHero5"";s:6:""voodoo"";s:12:""favHero5Time"";d:2.54;s:10:""favHero5_2"";s:11:""Hero_Voodoo"";s:11:""dice_tokens"";s:1:""1"";s:12:""season_level"";i:0;s:7:""slot_id"";s:1:""5"";s:11:""my_upgrades"";a:3:{i:0;s:15:""m.allmodes.pass"";i:1;s:16:""h.AllHeroes.Hero"";i:2;s:13:""m.Super-Taunt"";}s:17:""selected_upgrades"";a:3:{i:0;s:9:""cs.legacy"";i:1;s:11:""cc.limesoda"";i:2;s:16:""ai.custom_icon:5"";}s:11:""game_tokens"";i:0;s:16:""my_upgrades_info"";a:0:{}s:11:""creep_level"";i:0;s:9:""timestamp"";i:1654534450;s:8:""matchIds"";s:200:""161011843 161011719 160997707 160997672 160997617 160997588 160997546 160997461 160924791 160810175 160381140 160380941 160380861 160360234 160306077 160240812 160240771 160240673 160240551 160189687 "";s:10:""matchDates"";s:200:""02/27/202102/27/202102/24/202102/24/202102/24/202102/24/202102/24/202102/24/202102/14/202101/30/202112/01/202012/01/202012/01/202011/28/202011/21/202011/12/202011/12/202011/12/202011/12/202011/04/2020"";s:5:""k_d_a"";s:9:""4.8/5.8/8"";s:13:""avgGameLength"";d:2300.4000000000001;s:9:""avgXP_min"";d:374.48000000000002;s:9:""avgDenies"";d:9.8399999999999999;s:13:""avgCreepKills"";d:76.849999999999994;s:15:""avgNeutralKills"";d:18.620000000000001;s:14:""avgActions_min"";d:80.299999999999997;s:12:""avgWardsUsed"";d:0.31;s:11:""quest_stats"";a:1:{s:5:""error"";a:2:{s:12:""quest_status"";i:0;s:18:""leaderboard_status"";i:0;}}s:29:""prev_seasons_cam_games_played"";i:207;s:23:""prev_seasons_cam_discos"";i:2;s:30:""latest_season_cam_games_played"";s:3:""340"";s:24:""latest_season_cam_discos"";s:1:""2"";s:28:""curr_season_cam_games_played"";s:3:""340"";s:22:""curr_season_cam_discos"";s:1:""2"";s:8:""cam_wins"";s:3:""180"";s:10:""cam_losses"";s:3:""160"";s:32:""prev_seasons_cam_cs_games_played"";i:0;s:26:""prev_seasons_cam_cs_discos"";i:0;s:33:""latest_season_cam_cs_games_played"";s:1:""0"";s:27:""latest_season_cam_cs_discos"";s:1:""0"";s:31:""curr_season_cam_cs_games_played"";s:1:""0"";s:25:""curr_season_cam_cs_discos"";s:1:""0"";s:11:""cam_cs_wins"";s:1:""0"";s:13:""cam_cs_losses"";s:1:""0"";s:10:""con_reward"";a:7:{s:7:""old_lvl"";i:0;s:8:""curr_lvl"";s:1:""3"";s:8:""next_lvl"";i:4;s:12:""require_rank"";i:15;s:14:""need_more_play"";i:4;s:17:""percentage_before"";s:4:""0.00"";s:10:""percentage"";s:4:""0.00"";}s:16:""vested_threshold"";i:5;i:0;b:1;}";

            // TODO: Use Real Response Data After Confirming The Correctness Of The Response Structure And Data

            return Ok(mockResponse);
        }

        if (table is "ranked")
        {
            AccountStatistics statistics = statisticsByType[AccountStatisticsType.Matchmaking];

            RankedStatisticsResponse response = new(account, statistics, aggregates);

            string realResponse = PhpSerialization.Serialize(response);
            string mockResponse = @"a:152:{s:8:""super_id"";s:6:""195592"";s:8:""nickname"";s:7:""[K]GOPO"";s:8:""standing"";s:1:""3"";s:12:""account_type"";s:1:""4"";s:10:""account_id"";s:6:""195592"";s:16:""rnk_games_played"";s:4:""3515"";s:8:""rnk_wins"";s:4:""1729"";s:10:""rnk_losses"";s:4:""1786"";s:12:""rnk_concedes"";s:4:""1427"";s:16:""rnk_concedevotes"";s:3:""476"";s:12:""rnk_buybacks"";s:3:""113"";s:10:""rnk_discos"";s:2:""12"";s:10:""rnk_kicked"";s:1:""4"";s:19:""rnk_amm_solo_rating"";s:8:""1500.000"";s:18:""rnk_amm_solo_count"";s:1:""0"";s:17:""rnk_amm_solo_conf"";s:4:""0.00"";s:17:""rnk_amm_solo_prov"";s:1:""0"";s:17:""rnk_amm_solo_pset"";s:1:""0"";s:19:""rnk_amm_team_rating"";s:8:""1611.374"";s:18:""rnk_amm_team_count"";s:4:""3515"";s:17:""rnk_amm_team_conf"";s:4:""0.00"";s:17:""rnk_amm_team_prov"";s:4:""3515"";s:17:""rnk_amm_team_pset"";s:3:""127"";s:13:""rnk_herokills"";s:5:""15888"";s:11:""rnk_herodmg"";s:8:""39981961"";s:11:""rnk_heroexp"";s:8:""21331137"";s:17:""rnk_herokillsgold"";s:7:""9804561"";s:15:""rnk_heroassists"";s:5:""30985"";s:10:""rnk_deaths"";s:5:""21211"";s:18:""rnk_goldlost2death"";s:7:""5456511"";s:13:""rnk_secs_dead"";s:7:""3716604"";s:18:""rnk_teamcreepkills"";s:6:""235510"";s:16:""rnk_teamcreepdmg"";s:9:""106249209"";s:16:""rnk_teamcreepexp"";s:8:""20899031"";s:17:""rnk_teamcreepgold"";s:7:""9136851"";s:21:""rnk_neutralcreepkills"";s:5:""53286"";s:19:""rnk_neutralcreepdmg"";s:8:""37046592"";s:19:""rnk_neutralcreepexp"";s:7:""4081042"";s:20:""rnk_neutralcreepgold"";s:7:""2298907"";s:8:""rnk_bdmg"";s:7:""3053909"";s:11:""rnk_bdmgexp"";s:1:""0"";s:9:""rnk_razed"";s:4:""2713"";s:9:""rnk_bgold"";s:7:""4681848"";s:10:""rnk_denies"";s:5:""23752"";s:14:""rnk_exp_denied"";s:7:""1034671"";s:8:""rnk_gold"";s:8:""29223924"";s:14:""rnk_gold_spent"";s:8:""27540188"";s:7:""rnk_exp"";s:8:""46383688"";s:11:""rnk_actions"";s:8:""10479212"";s:8:""rnk_secs"";s:7:""7526246"";s:15:""rnk_consumables"";s:5:""16498"";s:9:""rnk_wards"";s:4:""3639"";s:13:""rnk_em_played"";s:1:""0"";s:9:""rnk_level"";s:2:""48"";s:13:""rnk_level_exp"";s:6:""122185"";s:11:""rnk_min_exp"";s:6:""117500"";s:11:""rnk_max_exp"";s:6:""122399"";s:20:""rnk_time_earning_exp"";s:7:""7480931"";s:13:""rnk_bloodlust"";s:3:""306"";s:14:""rnk_doublekill"";s:4:""1544"";s:14:""rnk_triplekill"";s:3:""158"";s:12:""rnk_quadkill"";s:2:""15"";s:16:""rnk_annihilation"";s:1:""0"";s:7:""rnk_ks3"";s:4:""1242"";s:7:""rnk_ks4"";s:3:""698"";s:7:""rnk_ks5"";s:3:""393"";s:7:""rnk_ks6"";s:3:""218"";s:7:""rnk_ks7"";s:3:""147"";s:7:""rnk_ks8"";s:2:""82"";s:7:""rnk_ks9"";s:2:""51"";s:8:""rnk_ks10"";s:2:""31"";s:8:""rnk_ks15"";s:1:""6"";s:13:""rnk_smackdown"";s:4:""1041"";s:15:""rnk_humiliation"";s:2:""17"";s:11:""rnk_nemesis"";s:5:""14741"";s:15:""rnk_retribution"";s:3:""362"";s:5:""level"";s:2:""58"";s:9:""level_exp"";i:173525;s:6:""discos"";s:2:""72"";s:15:""possible_discos"";s:1:""0"";s:12:""games_played"";s:4:""7353"";s:17:""num_bot_games_won"";s:2:""10"";s:18:""total_games_played"";i:8321;s:12:""total_discos"";i:49;s:8:""acc_secs"";s:7:""1269820"";s:16:""acc_games_played"";s:3:""552"";s:10:""acc_discos"";s:1:""1"";s:7:""cs_secs"";s:6:""112945"";s:15:""cs_games_played"";s:2:""61"";s:9:""cs_discos"";s:1:""2"";s:16:""mid_games_played"";s:4:""3646"";s:10:""mid_discos"";s:2:""30"";s:17:""rift_games_played"";s:1:""0"";s:11:""rift_discos"";s:1:""0"";s:13:""last_activity"";s:10:""06/06/2022"";s:11:""create_date"";s:10:""08/11/2009"";s:4:""name"";s:6:""KONGOR"";s:4:""rank"";s:6:""Leader"";s:8:""favHero1"";s:6:""shaman"";s:12:""favHero1Time"";d:4.2400000000000002;s:10:""favHero1_2"";s:11:""Hero_Shaman"";s:8:""favHero2"";s:6:""kraken"";s:12:""favHero2Time"";d:3.21;s:10:""favHero2_2"";s:11:""Hero_Kraken"";s:8:""favHero3"";s:9:""lodestone"";s:12:""favHero3Time"";d:2.8999999999999999;s:10:""favHero3_2"";s:14:""Hero_Lodestone"";s:8:""favHero4"";s:2:""ra"";s:12:""favHero4Time"";d:2.8999999999999999;s:10:""favHero4_2"";s:7:""Hero_Ra"";s:8:""favHero5"";s:8:""gauntlet"";s:12:""favHero5Time"";d:2.4500000000000002;s:10:""favHero5_2"";s:13:""Hero_Gauntlet"";s:11:""dice_tokens"";s:1:""1"";s:12:""season_level"";i:0;s:7:""slot_id"";s:1:""5"";s:11:""my_upgrades"";a:3:{i:0;s:15:""m.allmodes.pass"";i:1;s:16:""h.AllHeroes.Hero"";i:2;s:13:""m.Super-Taunt"";}s:17:""selected_upgrades"";a:3:{i:0;s:9:""cs.legacy"";i:1;s:11:""cc.limesoda"";i:2;s:16:""ai.custom_icon:5"";}s:11:""game_tokens"";i:0;s:16:""my_upgrades_info"";a:0:{}s:11:""creep_level"";i:0;s:9:""timestamp"";i:1654534446;s:8:""matchIds"";s:200:""151993153 150928667 149642192 145852212 145851630 145851176 145818131 145817494 145525801 145525176 145516585 145516324 145516068 145515673 145515375 145515012 145514923 145507860 145507552 145507115 "";s:10:""matchDates"";s:200:""11/25/201708/19/201705/07/201708/18/201608/18/201608/18/201608/15/201608/15/201607/20/201607/20/201607/19/201607/19/201607/19/201607/19/201607/19/201607/19/201607/19/201607/18/201607/18/201607/18/2016"";s:5:""k_d_a"";s:9:""4.5/6/8.8"";s:13:""avgGameLength"";d:2141.1799999999998;s:9:""avgXP_min"";d:372.01999999999998;s:9:""avgDenies"";d:6.7599999999999998;s:13:""avgCreepKills"";d:67;s:15:""avgNeutralKills"";d:15.16;s:14:""avgActions_min"";d:83.540000000000006;s:12:""avgWardsUsed"";d:1.04;s:11:""quest_stats"";a:1:{s:5:""error"";a:2:{s:12:""quest_status"";i:0;s:18:""leaderboard_status"";i:0;}}s:29:""prev_seasons_cam_games_played"";i:207;s:23:""prev_seasons_cam_discos"";i:2;s:30:""latest_season_cam_games_played"";s:3:""340"";s:24:""latest_season_cam_discos"";s:1:""2"";s:28:""curr_season_cam_games_played"";s:3:""340"";s:22:""curr_season_cam_discos"";s:1:""2"";s:8:""cam_wins"";s:3:""180"";s:10:""cam_losses"";s:3:""160"";s:32:""prev_seasons_cam_cs_games_played"";i:0;s:26:""prev_seasons_cam_cs_discos"";i:0;s:33:""latest_season_cam_cs_games_played"";s:1:""0"";s:27:""latest_season_cam_cs_discos"";s:1:""0"";s:31:""curr_season_cam_cs_games_played"";s:1:""0"";s:25:""curr_season_cam_cs_discos"";s:1:""0"";s:11:""cam_cs_wins"";s:1:""0"";s:13:""cam_cs_losses"";s:1:""0"";s:10:""con_reward"";a:7:{s:7:""old_lvl"";i:0;s:8:""curr_lvl"";s:1:""3"";s:8:""next_lvl"";i:4;s:12:""require_rank"";i:15;s:14:""need_more_play"";i:4;s:17:""percentage_before"";s:4:""0.00"";s:10:""percentage"";s:4:""0.00"";}s:16:""vested_threshold"";i:5;i:0;b:1;}";

            // TODO: Use Real Response Data After Confirming The Correctness Of The Response Structure And Data

            return Ok(mockResponse);
        }

        if (table is "casual")
        {
            AccountStatistics statistics = statisticsByType[AccountStatisticsType.MatchmakingCasual];

            CasualStatisticsResponse response = new(account, statistics, aggregates);

            string realResponse = PhpSerialization.Serialize(response);
            string mockResponse = @"a:147:{s:8:""super_id"";s:6:""195592"";s:8:""nickname"";s:7:""[K]GOPO"";s:8:""standing"";s:1:""3"";s:12:""account_type"";s:1:""4"";s:10:""account_id"";s:6:""195592"";s:15:""cs_games_played"";s:2:""61"";s:7:""cs_wins"";s:2:""35"";s:9:""cs_losses"";s:2:""26"";s:11:""cs_concedes"";s:2:""19"";s:15:""cs_concedevotes"";s:2:""13"";s:11:""cs_buybacks"";s:1:""8"";s:9:""cs_discos"";s:1:""2"";s:9:""cs_kicked"";s:1:""0"";s:18:""cs_amm_team_rating"";s:8:""1551.914"";s:17:""cs_amm_team_count"";s:2:""61"";s:16:""cs_amm_team_conf"";s:4:""0.00"";s:16:""cs_amm_team_prov"";s:2:""61"";s:16:""cs_amm_team_pset"";s:2:""61"";s:12:""cs_herokills"";s:3:""459"";s:10:""cs_herodmg"";s:7:""1020740"";s:10:""cs_heroexp"";s:6:""653035"";s:16:""cs_herokillsgold"";s:6:""424146"";s:14:""cs_heroassists"";s:3:""696"";s:9:""cs_deaths"";s:3:""451"";s:17:""cs_goldlost2death"";s:1:""0"";s:12:""cs_secs_dead"";s:5:""22092"";s:17:""cs_teamcreepkills"";s:4:""4730"";s:15:""cs_teamcreepdmg"";s:7:""2295818"";s:15:""cs_teamcreepexp"";s:6:""366016"";s:16:""cs_teamcreepgold"";s:6:""189968"";s:20:""cs_neutralcreepkills"";s:3:""711"";s:18:""cs_neutralcreepdmg"";s:6:""566764"";s:18:""cs_neutralcreepexp"";s:5:""51748"";s:19:""cs_neutralcreepgold"";s:5:""31817"";s:7:""cs_bdmg"";s:6:""157482"";s:10:""cs_bdmgexp"";s:5:""72349"";s:8:""cs_razed"";s:3:""114"";s:8:""cs_bgold"";s:6:""157681"";s:9:""cs_denies"";s:3:""334"";s:13:""cs_exp_denied"";s:4:""8754"";s:7:""cs_gold"";s:6:""971939"";s:13:""cs_gold_spent"";s:6:""938928"";s:6:""cs_exp"";s:7:""1144024"";s:10:""cs_actions"";s:6:""152600"";s:7:""cs_secs"";s:6:""112945"";s:14:""cs_consumables"";s:3:""426"";s:8:""cs_wards"";s:2:""82"";s:12:""cs_em_played"";s:1:""0"";s:8:""cs_level"";s:1:""6"";s:12:""cs_level_exp"";s:4:""2235"";s:10:""cs_min_exp"";s:4:""2000"";s:10:""cs_max_exp"";s:4:""2699"";s:19:""cs_time_earning_exp"";s:6:""109597"";s:12:""cs_bloodlust"";s:1:""9"";s:13:""cs_doublekill"";s:2:""81"";s:13:""cs_triplekill"";s:2:""13"";s:11:""cs_quadkill"";s:1:""1"";s:15:""cs_annihilation"";s:1:""0"";s:6:""cs_ks3"";s:2:""32"";s:6:""cs_ks4"";s:2:""27"";s:6:""cs_ks5"";s:2:""17"";s:6:""cs_ks6"";s:2:""10"";s:6:""cs_ks7"";s:1:""6"";s:6:""cs_ks8"";s:1:""4"";s:6:""cs_ks9"";s:1:""3"";s:7:""cs_ks10"";s:1:""3"";s:7:""cs_ks15"";s:1:""0"";s:12:""cs_smackdown"";s:2:""39"";s:14:""cs_humiliation"";s:1:""0"";s:10:""cs_nemesis"";s:3:""258"";s:14:""cs_retribution"";s:2:""13"";s:5:""level"";s:2:""58"";s:9:""level_exp"";i:173525;s:6:""discos"";s:2:""72"";s:15:""possible_discos"";s:1:""0"";s:12:""games_played"";s:4:""7353"";s:17:""num_bot_games_won"";s:2:""10"";s:18:""total_games_played"";i:8321;s:12:""total_discos"";i:49;s:8:""acc_secs"";s:7:""1269820"";s:16:""acc_games_played"";s:3:""552"";s:10:""acc_discos"";s:1:""1"";s:8:""rnk_secs"";s:7:""7526246"";s:16:""rnk_games_played"";s:4:""3515"";s:10:""rnk_discos"";s:2:""12"";s:16:""mid_games_played"";s:4:""3646"";s:10:""mid_discos"";s:2:""30"";s:17:""rift_games_played"";s:1:""0"";s:11:""rift_discos"";s:1:""0"";s:13:""last_activity"";s:10:""06/06/2022"";s:11:""create_date"";s:10:""08/11/2009"";s:4:""name"";s:6:""KONGOR"";s:4:""rank"";s:6:""Leader"";s:8:""favHero1"";s:14:""doctorrepulsor"";s:12:""favHero1Time"";d:21.309999999999999;s:10:""favHero1_2"";s:19:""Hero_DoctorRepulsor"";s:8:""favHero2"";s:11:""hellbringer"";s:12:""favHero2Time"";d:9.8399999999999999;s:10:""favHero2_2"";s:16:""Hero_Hellbringer"";s:8:""favHero3"";s:7:""defiler"";s:12:""favHero3Time"";d:6.5599999999999996;s:10:""favHero3_2"";s:12:""Hero_Defiler"";s:8:""favHero4"";s:8:""hantumon"";s:12:""favHero4Time"";d:6.5599999999999996;s:10:""favHero4_2"";s:13:""Hero_Hantumon"";s:8:""favHero5"";s:8:""predator"";s:12:""favHero5Time"";d:4.9199999999999999;s:10:""favHero5_2"";s:13:""Hero_Predator"";s:11:""dice_tokens"";s:1:""1"";s:12:""season_level"";i:0;s:7:""slot_id"";s:1:""5"";s:11:""my_upgrades"";a:3:{i:0;s:15:""m.allmodes.pass"";i:1;s:16:""h.AllHeroes.Hero"";i:2;s:13:""m.Super-Taunt"";}s:17:""selected_upgrades"";a:3:{i:0;s:9:""cs.legacy"";i:1;s:11:""cc.limesoda"";i:2;s:16:""ai.custom_icon:5"";}s:11:""game_tokens"";i:0;s:16:""my_upgrades_info"";a:0:{}s:11:""creep_level"";i:0;s:9:""timestamp"";i:1654534448;s:8:""matchIds"";s:200:""141748802 135965003 135502327 135501613 135501183 135500687 135500243 135281408 135280386 135110841 134994190 134910769 134910063 134909629 134909073 132360596 132359291 131843252 131842832 131815884 "";s:10:""matchDates"";s:200:""11/01/201501/30/201501/12/201501/12/201501/12/201501/12/201501/12/201501/03/201501/03/201512/27/201412/23/201412/19/201412/19/201412/19/201412/19/201409/07/201409/07/201408/19/201408/19/201408/18/2014"";s:5:""k_d_a"";s:12:""7.5/7.4/11.4"";s:13:""avgGameLength"";d:1851.5599999999999;s:9:""avgXP_min"";d:626.30999999999995;s:9:""avgDenies"";d:5.4800000000000004;s:13:""avgCreepKills"";d:77.540000000000006;s:15:""avgNeutralKills"";d:11.66;s:14:""avgActions_min"";d:81.069999999999993;s:12:""avgWardsUsed"";d:1.3400000000000001;s:11:""quest_stats"";a:1:{s:5:""error"";a:2:{s:12:""quest_status"";i:0;s:18:""leaderboard_status"";i:0;}}s:29:""prev_seasons_cam_games_played"";i:207;s:23:""prev_seasons_cam_discos"";i:2;s:30:""latest_season_cam_games_played"";s:3:""340"";s:24:""latest_season_cam_discos"";s:1:""2"";s:28:""curr_season_cam_games_played"";s:3:""340"";s:22:""curr_season_cam_discos"";s:1:""2"";s:8:""cam_wins"";s:3:""180"";s:10:""cam_losses"";s:3:""160"";s:32:""prev_seasons_cam_cs_games_played"";i:0;s:26:""prev_seasons_cam_cs_discos"";i:0;s:33:""latest_season_cam_cs_games_played"";s:1:""0"";s:27:""latest_season_cam_cs_discos"";s:1:""0"";s:31:""curr_season_cam_cs_games_played"";s:1:""0"";s:25:""curr_season_cam_cs_discos"";s:1:""0"";s:11:""cam_cs_wins"";s:1:""0"";s:13:""cam_cs_losses"";s:1:""0"";s:10:""con_reward"";a:7:{s:7:""old_lvl"";i:0;s:8:""curr_lvl"";s:1:""3"";s:8:""next_lvl"";i:4;s:12:""require_rank"";i:15;s:14:""need_more_play"";i:4;s:17:""percentage_before"";s:4:""0.00"";s:10:""percentage"";s:4:""0.00"";}s:16:""vested_threshold"";i:5;i:0;b:1;}";

            // TODO: Use Real Response Data After Confirming The Correctness Of The Response Structure And Data

            return Ok(mockResponse);
        }

        if (table is "campaign")
        {
            AccountStatistics statistics = statisticsByType[AccountStatisticsType.Matchmaking];

            CampaignStatisticsResponse response = new(account, statistics, aggregates);

            string realResponse = PhpSerialization.Serialize(response);
            string mockResponse = @"a:158:{s:8:""super_id"";s:6:""195592"";s:8:""nickname"";s:7:""[K]GOPO"";s:8:""standing"";s:1:""3"";s:12:""account_type"";s:1:""4"";s:10:""account_id"";s:6:""195592"";s:5:""level"";s:2:""58"";s:9:""level_exp"";i:173525;s:6:""discos"";s:2:""72"";s:15:""possible_discos"";s:1:""0"";s:12:""games_played"";s:4:""7353"";s:17:""num_bot_games_won"";s:2:""10"";s:18:""total_games_played"";i:8321;s:12:""total_discos"";i:49;s:8:""acc_secs"";s:7:""1269820"";s:16:""acc_games_played"";s:3:""552"";s:10:""acc_discos"";s:1:""1"";s:8:""rnk_secs"";s:7:""7526246"";s:16:""rnk_games_played"";s:4:""3515"";s:10:""rnk_discos"";s:2:""12"";s:7:""cs_secs"";s:6:""112945"";s:15:""cs_games_played"";s:2:""61"";s:9:""cs_discos"";s:1:""2"";s:16:""mid_games_played"";s:4:""3646"";s:10:""mid_discos"";s:2:""30"";s:17:""rift_games_played"";s:1:""0"";s:11:""rift_discos"";s:1:""0"";s:13:""last_activity"";s:10:""05/26/2022"";s:11:""create_date"";s:10:""08/11/2009"";s:6:""season"";s:2:""12"";s:16:""cam_games_played"";s:3:""341"";s:8:""cam_wins"";s:3:""180"";s:10:""cam_losses"";s:3:""160"";s:12:""cam_concedes"";s:3:""125"";s:16:""cam_concedevotes"";s:2:""64"";s:12:""cam_buybacks"";s:2:""10"";s:10:""cam_discos"";s:1:""2"";s:10:""cam_kicked"";s:1:""0"";s:19:""cam_amm_solo_rating"";s:7:""1500.00"";s:18:""cam_amm_solo_count"";s:1:""0"";s:17:""cam_amm_solo_conf"";s:4:""0.00"";s:17:""cam_amm_solo_prov"";s:1:""0"";s:17:""cam_amm_solo_pset"";s:1:""0"";s:19:""cam_amm_team_rating"";s:7:""1363.45"";s:18:""cam_amm_team_count"";s:3:""341"";s:17:""cam_amm_team_conf"";s:4:""0.00"";s:17:""cam_amm_team_prov"";s:3:""341"";s:17:""cam_amm_team_pset"";s:3:""127"";s:13:""cam_herokills"";s:4:""1377"";s:11:""cam_herodmg"";s:7:""3576677"";s:11:""cam_heroexp"";s:7:""1669570"";s:17:""cam_herokillsgold"";s:7:""1016538"";s:15:""cam_heroassists"";s:4:""3474"";s:10:""cam_deaths"";s:4:""2377"";s:18:""cam_goldlost2death"";s:6:""478153"";s:13:""cam_secs_dead"";s:6:""125859"";s:18:""cam_teamcreepkills"";s:5:""19339"";s:16:""cam_teamcreepdmg"";s:7:""9469895"";s:16:""cam_teamcreepexp"";s:7:""2091921"";s:17:""cam_teamcreepgold"";s:6:""822507"";s:21:""cam_neutralcreepkills"";s:4:""3684"";s:19:""cam_neutralcreepdmg"";s:7:""2742477"";s:19:""cam_neutralcreepexp"";s:6:""401376"";s:20:""cam_neutralcreepgold"";s:6:""162608"";s:8:""cam_bdmg"";s:6:""295090"";s:11:""cam_bdmgexp"";s:1:""0"";s:9:""cam_razed"";s:3:""242"";s:9:""cam_bgold"";s:6:""477650"";s:10:""cam_denies"";s:4:""1381"";s:14:""cam_exp_denied"";s:5:""60743"";s:8:""cam_gold"";s:7:""2864581"";s:14:""cam_gold_spent"";s:7:""2875928"";s:7:""cam_exp"";s:7:""4172367"";s:11:""cam_actions"";s:6:""849121"";s:8:""cam_secs"";s:6:""706695"";s:15:""cam_consumables"";s:4:""3195"";s:9:""cam_wards"";s:4:""1181"";s:13:""cam_em_played"";s:1:""0"";s:9:""cam_level"";s:1:""1"";s:13:""cam_level_exp"";s:5:""14755"";s:11:""cam_min_exp"";s:1:""0"";s:11:""cam_max_exp"";s:3:""199"";s:20:""cam_time_earning_exp"";s:6:""704760"";s:13:""cam_bloodlust"";s:2:""30"";s:14:""cam_doublekill"";s:2:""96"";s:14:""cam_triplekill"";s:2:""11"";s:12:""cam_quadkill"";s:1:""0"";s:16:""cam_annihilation"";s:1:""0"";s:7:""cam_ks3"";s:2:""92"";s:7:""cam_ks4"";s:2:""45"";s:7:""cam_ks5"";s:2:""16"";s:7:""cam_ks6"";s:2:""10"";s:7:""cam_ks7"";s:1:""6"";s:7:""cam_ks8"";s:1:""3"";s:7:""cam_ks9"";s:1:""2"";s:8:""cam_ks10"";s:1:""2"";s:8:""cam_ks15"";s:1:""0"";s:13:""cam_smackdown"";s:2:""32"";s:15:""cam_humiliation"";s:1:""2"";s:11:""cam_nemesis"";s:3:""656"";s:15:""cam_retribution"";s:2:""55"";s:4:""name"";s:6:""KONGOR"";s:4:""rank"";s:6:""Leader"";s:8:""favHero1"";s:6:""shaman"";s:12:""favHero1Time"";d:9.3800000000000008;s:10:""favHero1_2"";s:11:""Hero_Shaman"";s:8:""favHero2"";s:11:""hellbringer"";s:12:""favHero2Time"";d:9.0899999999999999;s:10:""favHero2_2"";s:16:""Hero_Hellbringer"";s:8:""favHero3"";s:6:""ebulus"";s:12:""favHero3Time"";d:7.3300000000000001;s:10:""favHero3_2"";s:11:""Hero_Ebulus"";s:8:""favHero4"";s:5:""fairy"";s:12:""favHero4Time"";d:4.1100000000000003;s:10:""favHero4_2"";s:10:""Hero_Fairy"";s:8:""favHero5"";s:8:""revenant"";s:12:""favHero5Time"";d:3.8100000000000001;s:10:""favHero5_2"";s:13:""Hero_Revenant"";s:11:""dice_tokens"";s:1:""1"";s:12:""season_level"";i:0;s:7:""slot_id"";s:1:""5"";s:11:""my_upgrades"";a:3:{i:0;s:15:""m.allmodes.pass"";i:1;s:16:""h.AllHeroes.Hero"";i:2;s:13:""m.Super-Taunt"";}s:17:""selected_upgrades"";a:3:{i:0;s:9:""cs.legacy"";i:1;s:11:""cc.limesoda"";i:2;s:16:""ai.custom_icon:5"";}s:11:""game_tokens"";i:0;s:16:""my_upgrades_info"";a:0:{}s:11:""creep_level"";i:0;s:9:""timestamp"";i:1654534383;s:8:""matchIds"";s:200:""162435683 162192026 162191733 161914657 161881792 161881580 161881339 161879583 161879474 161879353 161845026 161844682 161844506 161843638 161843480 161837884 161837740 161837528 161744253 161743911 "";s:10:""matchDates"";s:200:""10/26/202109/11/202109/11/202107/24/202107/18/202107/18/202107/18/202107/17/202107/17/202107/17/202107/11/202107/11/202107/11/202107/11/202107/11/202107/10/202107/10/202107/10/202106/23/202106/23/2021"";s:5:""k_d_a"";s:8:""4/7/10.2"";s:13:""avgGameLength"";d:2072.4200000000001;s:9:""avgXP_min"";d:355.22000000000003;s:9:""avgDenies"";d:4.0499999999999998;s:13:""avgCreepKills"";d:56.710000000000001;s:15:""avgNeutralKills"";d:10.800000000000001;s:14:""avgActions_min"";d:72.090000000000003;s:12:""avgWardsUsed"";d:3.46;s:11:""quest_stats"";a:1:{s:5:""error"";a:2:{s:12:""quest_status"";i:0;s:18:""leaderboard_status"";i:0;}}s:21:""highest_level_current"";s:2:""12"";s:13:""current_level"";s:2:""12"";s:13:""level_percent"";d:36;s:9:""season_id"";s:2:""12"";s:29:""prev_seasons_cam_games_played"";i:207;s:23:""prev_seasons_cam_discos"";i:2;s:30:""latest_season_cam_games_played"";s:3:""340"";s:24:""latest_season_cam_discos"";s:1:""2"";s:28:""curr_season_cam_games_played"";s:3:""340"";s:22:""curr_season_cam_discos"";s:1:""2"";s:32:""prev_seasons_cam_cs_games_played"";i:0;s:26:""prev_seasons_cam_cs_discos"";i:0;s:33:""latest_season_cam_cs_games_played"";s:1:""0"";s:27:""latest_season_cam_cs_discos"";s:1:""0"";s:31:""curr_season_cam_cs_games_played"";s:1:""0"";s:25:""curr_season_cam_cs_discos"";s:1:""0"";s:11:""cam_cs_wins"";s:1:""0"";s:13:""cam_cs_losses"";s:1:""0"";s:10:""con_reward"";a:7:{s:7:""old_lvl"";i:0;s:8:""curr_lvl"";s:1:""3"";s:8:""next_lvl"";i:4;s:12:""require_rank"";i:15;s:14:""need_more_play"";i:4;s:17:""percentage_before"";s:4:""0.00"";s:10:""percentage"";s:4:""0.00"";}s:16:""vested_threshold"";i:5;i:0;b:1;}";

            // TODO: Use Real Response Data After Confirming The Correctness Of The Response Structure And Data

            return Ok(mockResponse);
        }

        if (table is "campaign_casual")
        {
            AccountStatistics statistics = statisticsByType[AccountStatisticsType.MatchmakingCasual];

            CampaignCasualStatisticsResponse response = new(account, statistics, aggregates);

            string realResponse = PhpSerialization.Serialize(response);
            string mockResponse = @"???";

            // TODO: Use Real Response Data After Confirming The Correctness Of The Response Structure And Data

            return Ok(mockResponse);
        }

        if (table is "mastery")
        {
            ShowMasteryStatisticsResponse response = new(account);

            // TODO: Populate MasteryInfo From Mastery System Once Re-Implemented
            // TODO: Populate MasteryRewards From Mastery System Once Re-Implemented (Only For Own Account)

            string realResponse = PhpSerialization.Serialize(response);
            string mockResponse = @"a:14:{s:10:""account_id"";s:6:""195592"";s:8:""nickname"";s:7:""[K]GOPO"";s:4:""name"";s:6:""KONGOR"";s:4:""rank"";s:6:""Leader"";s:8:""standing"";s:1:""3"";s:11:""create_date"";s:10:""08/11/2009"";s:13:""last_activity"";s:10:""06/07/2022"";s:17:""selected_upgrades"";a:9:{i:0;s:13:""av.Flamboyant"";i:1;s:13:""c.cat_courier"";i:2;s:16:""t.Dumpster_Taunt"";i:3;s:9:""cs.legacy"";i:4;s:11:""w.8bit_ward"";i:5;s:22:""sc.punk_circle_upgrade"";i:6;s:10:""te.Punk TP"";i:7;s:11:""cc.limesoda"";i:8;s:16:""ai.custom_icon:5"";}s:5:""level"";s:2:""58"";s:9:""level_exp"";s:6:""173525"";s:12:""mastery_info"";a:139:{i:0;a:2:{s:8:""heroname"";s:12:""Hero_Armadon"";s:3:""exp"";s:5:""16951"";}i:1;a:2:{s:8:""heroname"";s:13:""Hero_Behemoth"";s:3:""exp"";s:5:""22769"";}i:2;a:2:{s:8:""heroname"";s:12:""Hero_Chronos"";s:3:""exp"";s:5:""16850"";}i:3;a:2:{s:8:""heroname"";s:12:""Hero_Defiler"";s:3:""exp"";s:5:""25132"";}i:4;a:2:{s:8:""heroname"";s:13:""Hero_Devourer"";s:3:""exp"";s:5:""28163"";}i:5;a:2:{s:8:""heroname"";s:14:""Hero_DwarfMagi"";s:3:""exp"";s:4:""7317"";}i:6;a:2:{s:8:""heroname"";s:11:""Hero_Ebulus"";s:3:""exp"";s:5:""36100"";}i:7;a:2:{s:8:""heroname"";s:16:""Hero_Electrician"";s:3:""exp"";s:4:""4705"";}i:8;a:2:{s:8:""heroname"";s:10:""Hero_Fairy"";s:3:""exp"";s:5:""22098"";}i:9;a:2:{s:8:""heroname"";s:11:""Hero_Frosty"";s:3:""exp"";s:4:""5298"";}i:10;a:2:{s:8:""heroname"";s:16:""Hero_Hammerstorm"";s:3:""exp"";s:4:""6662"";}i:11;a:2:{s:8:""heroname"";s:13:""Hero_Hantumon"";s:3:""exp"";s:4:""2512"";}i:12;a:2:{s:8:""heroname"";s:9:""Hero_Hiro"";s:3:""exp"";s:4:""1939"";}i:13;a:2:{s:8:""heroname"";s:11:""Hero_Hunter"";s:3:""exp"";s:4:""3454"";}i:14;a:2:{s:8:""heroname"";s:11:""Hero_Kraken"";s:3:""exp"";s:4:""3071"";}i:15;a:2:{s:8:""heroname"";s:10:""Hero_Kunas"";s:3:""exp"";s:5:""36100"";}i:16;a:2:{s:8:""heroname"";s:10:""Hero_Krixi"";s:3:""exp"";s:4:""6841"";}i:17;a:2:{s:8:""heroname"";s:19:""Hero_PollywogPriest"";s:3:""exp"";s:4:""3165"";}i:18;a:2:{s:8:""heroname"";s:10:""Hero_Rocky"";s:3:""exp"";s:4:""5799"";}i:19;a:2:{s:8:""heroname"";s:16:""Hero_Soulstealer"";s:3:""exp"";s:4:""3909"";}i:20;a:2:{s:8:""heroname"";s:11:""Hero_Treant"";s:3:""exp"";s:4:""5132"";}i:21;a:2:{s:8:""heroname"";s:10:""Hero_Vanya"";s:3:""exp"";s:4:""1067"";}i:22;a:2:{s:8:""heroname"";s:11:""Hero_Voodoo"";s:3:""exp"";s:5:""16224"";}i:23;a:2:{s:8:""heroname"";s:12:""Hero_WolfMan"";s:3:""exp"";s:4:""3850"";}i:24;a:2:{s:8:""heroname"";s:9:""Hero_Yogi"";s:3:""exp"";s:5:""15469"";}i:25;a:2:{s:8:""heroname"";s:11:""Hero_Zephyr"";s:3:""exp"";s:4:""1088"";}i:26;a:2:{s:8:""heroname"";s:10:""Hero_Mumra"";s:3:""exp"";s:5:""36100"";}i:27;a:2:{s:8:""heroname"";s:12:""Hero_Tempest"";s:3:""exp"";s:5:""10878"";}i:28;a:2:{s:8:""heroname"";s:12:""Hero_Ophelia"";s:3:""exp"";s:4:""5437"";}i:29;a:2:{s:8:""heroname"";s:12:""Hero_Javaras"";s:3:""exp"";s:4:""5306"";}i:30;a:2:{s:8:""heroname"";s:16:""Hero_Legionnaire"";s:3:""exp"";s:4:""4398"";}i:31;a:2:{s:8:""heroname"";s:13:""Hero_Predator"";s:3:""exp"";s:5:""12736"";}i:32;a:2:{s:8:""heroname"";s:13:""Hero_Accursed"";s:3:""exp"";s:4:""8636"";}i:33;a:2:{s:8:""heroname"";s:10:""Hero_Nomad"";s:3:""exp"";s:4:""2214"";}i:34;a:2:{s:8:""heroname"";s:9:""Hero_Scar"";s:3:""exp"";s:4:""4725"";}i:35;a:2:{s:8:""heroname"";s:11:""Hero_Shaman"";s:3:""exp"";s:5:""36100"";}i:36;a:2:{s:8:""heroname"";s:10:""Hero_Scout"";s:3:""exp"";s:4:""1156"";}i:37;a:2:{s:8:""heroname"";s:13:""Hero_Jereziah"";s:3:""exp"";s:4:""4726"";}i:38;a:2:{s:8:""heroname"";s:11:""Hero_Xalynx"";s:3:""exp"";s:4:""4591"";}i:39;a:2:{s:8:""heroname"";s:17:""Hero_PuppetMaster"";s:3:""exp"";s:4:""2666"";}i:40;a:2:{s:8:""heroname"";s:12:""Hero_Arachna"";s:3:""exp"";s:4:""8162"";}i:41;a:2:{s:8:""heroname"";s:16:""Hero_Hellbringer"";s:3:""exp"";s:5:""36100"";}i:42;a:2:{s:8:""heroname"";s:15:""Hero_Pyromancer"";s:3:""exp"";s:4:""1856"";}i:43;a:2:{s:8:""heroname"";s:15:""Hero_Pestilence"";s:3:""exp"";s:4:""7040"";}i:44;a:2:{s:8:""heroname"";s:12:""Hero_Maliken"";s:3:""exp"";s:5:""22040"";}i:45;a:2:{s:8:""heroname"";s:14:""Hero_Andromeda"";s:3:""exp"";s:4:""2805"";}i:46;a:2:{s:8:""heroname"";s:13:""Hero_Valkyrie"";s:3:""exp"";s:4:""5619"";}i:47;a:2:{s:8:""heroname"";s:13:""Hero_BabaYaga"";s:3:""exp"";s:4:""2616"";}i:48;a:2:{s:8:""heroname"";s:13:""Hero_Succubis"";s:3:""exp"";s:4:""5252"";}i:49;a:2:{s:8:""heroname"";s:11:""Hero_Magmar"";s:3:""exp"";s:4:""3260"";}i:50;a:2:{s:8:""heroname"";s:18:""Hero_DiseasedRider"";s:3:""exp"";s:4:""8766"";}i:51;a:2:{s:8:""heroname"";s:14:""Hero_HellDemon"";s:3:""exp"";s:4:""8033"";}i:52;a:2:{s:8:""heroname"";s:10:""Hero_Panda"";s:3:""exp"";s:4:""2187"";}i:53;a:2:{s:8:""heroname"";s:22:""Hero_CorruptedDisciple"";s:3:""exp"";s:5:""10363"";}i:54;a:2:{s:8:""heroname"";s:15:""Hero_Vindicator"";s:3:""exp"";s:5:""12851"";}i:55;a:2:{s:8:""heroname"";s:15:""Hero_SandWraith"";s:3:""exp"";s:4:""9114"";}i:56;a:2:{s:8:""heroname"";s:12:""Hero_Rampage"";s:3:""exp"";s:4:""3798"";}i:57;a:2:{s:8:""heroname"";s:16:""Hero_WitchSlayer"";s:3:""exp"";s:4:""2150"";}i:58;a:2:{s:8:""heroname"";s:19:""Hero_ForsakenArcher"";s:3:""exp"";s:5:""16824"";}i:59;a:2:{s:8:""heroname"";s:13:""Hero_Engineer"";s:3:""exp"";s:4:""7321"";}i:60;a:2:{s:8:""heroname"";s:13:""Hero_Deadwood"";s:3:""exp"";s:5:""29396"";}i:61;a:2:{s:8:""heroname"";s:12:""Hero_Chipper"";s:3:""exp"";s:4:""2830"";}i:62;a:2:{s:8:""heroname"";s:12:""Hero_Bubbles"";s:3:""exp"";s:4:""1824"";}i:63;a:2:{s:8:""heroname"";s:9:""Hero_Fade"";s:3:""exp"";s:4:""3495"";}i:64;a:2:{s:8:""heroname"";s:14:""Hero_Bephelgor"";s:3:""exp"";s:5:""12035"";}i:65;a:2:{s:8:""heroname"";s:13:""Hero_Gauntlet"";s:3:""exp"";s:5:""33712"";}i:66;a:2:{s:8:""heroname"";s:11:""Hero_Tundra"";s:3:""exp"";s:4:""9966"";}i:67;a:2:{s:8:""heroname"";s:14:""Hero_Gladiator"";s:3:""exp"";s:5:""11565"";}i:68;a:2:{s:8:""heroname"";s:19:""Hero_DoctorRepulsor"";s:3:""exp"";s:5:""16028"";}i:69;a:2:{s:8:""heroname"";s:19:""Hero_FlintBeastwood"";s:3:""exp"";s:5:""32400"";}i:70;a:2:{s:8:""heroname"";s:15:""Hero_Bombardier"";s:3:""exp"";s:4:""1117"";}i:71;a:2:{s:8:""heroname"";s:12:""Hero_Moraxus"";s:3:""exp"";s:4:""2579"";}i:72;a:2:{s:8:""heroname"";s:16:""Hero_Hydromancer"";s:3:""exp"";s:5:""10949"";}i:73;a:2:{s:8:""heroname"";s:12:""Hero_Dampeer"";s:3:""exp"";s:4:""1860"";}i:74;a:2:{s:8:""heroname"";s:11:""Hero_Empath"";s:3:""exp"";s:5:""36100"";}i:75;a:2:{s:8:""heroname"";s:10:""Hero_Aluna"";s:3:""exp"";s:4:""5752"";}i:76;a:2:{s:8:""heroname"";s:12:""Hero_Tremble"";s:3:""exp"";s:3:""520"";}i:77;a:2:{s:8:""heroname"";s:15:""Hero_Silhouette"";s:3:""exp"";s:4:""2510"";}i:78;a:2:{s:8:""heroname"";s:9:""Hero_Flux"";s:3:""exp"";s:4:""7648"";}i:79;a:2:{s:8:""heroname"";s:11:""Hero_Martyr"";s:3:""exp"";s:5:""36100"";}i:80;a:2:{s:8:""heroname"";s:7:""Hero_Ra"";s:3:""exp"";s:4:""4363"";}i:81;a:2:{s:8:""heroname"";s:13:""Hero_Parasite"";s:3:""exp"";s:3:""478"";}i:82;a:2:{s:8:""heroname"";s:18:""Hero_EmeraldWarden"";s:3:""exp"";s:4:""7664"";}i:83;a:2:{s:8:""heroname"";s:13:""Hero_Revenant"";s:3:""exp"";s:5:""16110"";}i:84;a:2:{s:8:""heroname"";s:15:""Hero_MonkeyKing"";s:3:""exp"";s:4:""2620"";}i:85;a:2:{s:8:""heroname"";s:18:""Hero_DrunkenMaster"";s:3:""exp"";s:4:""2891"";}i:86;a:2:{s:8:""heroname"";s:17:""Hero_MasterOfArms"";s:3:""exp"";s:5:""36100"";}i:87;a:2:{s:8:""heroname"";s:13:""Hero_Rhapsody"";s:3:""exp"";s:4:""3385"";}i:88;a:2:{s:8:""heroname"";s:14:""Hero_Geomancer"";s:3:""exp"";s:4:""8816"";}i:89;a:2:{s:8:""heroname"";s:10:""Hero_Midas"";s:3:""exp"";s:4:""4572"";}i:90;a:2:{s:8:""heroname"";s:17:""Hero_Cthulhuphant"";s:3:""exp"";s:4:""6165"";}i:91;a:2:{s:8:""heroname"";s:12:""Hero_Monarch"";s:3:""exp"";s:5:""14425"";}i:92;a:2:{s:8:""heroname"";s:11:""Hero_Gemini"";s:3:""exp"";s:4:""2688"";}i:93;a:2:{s:8:""heroname"";s:16:""Hero_Dreadknight"";s:3:""exp"";s:5:""11887"";}i:94;a:2:{s:8:""heroname"";s:16:""Hero_ShadowBlade"";s:3:""exp"";s:5:""22131"";}i:95;a:2:{s:8:""heroname"";s:12:""Hero_Artesia"";s:3:""exp"";s:4:""3920"";}i:96;a:2:{s:8:""heroname"";s:10:""Hero_Taint"";s:3:""exp"";s:4:""2231"";}i:97;a:2:{s:8:""heroname"";s:14:""Hero_Berzerker"";s:3:""exp"";s:4:""3435"";}i:98;a:2:{s:8:""heroname"";s:16:""Hero_FlameDragon"";s:3:""exp"";s:4:""8461"";}i:99;a:2:{s:8:""heroname"";s:12:""Hero_Kenisis"";s:3:""exp"";s:5:""12699"";}i:100;a:2:{s:8:""heroname"";s:13:""Hero_Gunblade"";s:3:""exp"";s:4:""2693"";}i:101;a:2:{s:8:""heroname"";s:10:""Hero_Blitz"";s:3:""exp"";s:3:""614"";}i:102;a:2:{s:8:""heroname"";s:14:""Hero_Artillery"";s:3:""exp"";s:5:""20138"";}i:103;a:2:{s:8:""heroname"";s:12:""Hero_Ellonia"";s:3:""exp"";s:5:""17709"";}i:104;a:2:{s:8:""heroname"";s:13:""Hero_Riftmage"";s:3:""exp"";s:4:""1142"";}i:105;a:2:{s:8:""heroname"";s:10:""Hero_Plant"";s:3:""exp"";s:4:""3751"";}i:106;a:2:{s:8:""heroname"";s:12:""Hero_Ravenor"";s:3:""exp"";s:4:""7027"";}i:107;a:2:{s:8:""heroname"";s:12:""Hero_Prophet"";s:3:""exp"";s:4:""3149"";}i:108;a:2:{s:8:""heroname"";s:10:""Hero_Rally"";s:3:""exp"";s:4:""4237"";}i:109;a:2:{s:8:""heroname"";s:10:""Hero_Oogie"";s:3:""exp"";s:4:""6337"";}i:110;a:2:{s:8:""heroname"";s:13:""Hero_Solstice"";s:3:""exp"";s:4:""4240"";}i:111;a:2:{s:8:""heroname"";s:10:""Hero_Pearl"";s:3:""exp"";s:5:""12417"";}i:112;a:2:{s:8:""heroname"";s:11:""Hero_Grinex"";s:3:""exp"";s:4:""3258"";}i:113;a:2:{s:8:""heroname"";s:14:""Hero_Lodestone"";s:3:""exp"";s:5:""12577"";}i:114;a:2:{s:8:""heroname"";s:13:""Hero_Bushwack"";s:3:""exp"";s:5:""10208"";}i:115;a:2:{s:8:""heroname"";s:12:""Hero_Salomon"";s:3:""exp"";s:4:""3881"";}i:116;a:2:{s:8:""heroname"";s:13:""Hero_Prisoner"";s:3:""exp"";s:5:""22240"";}i:117;a:2:{s:8:""heroname"";s:18:""Hero_SirBenzington"";s:3:""exp"";s:3:""977"";}i:118;a:2:{s:8:""heroname"";s:10:""Hero_Circe"";s:3:""exp"";s:4:""5173"";}i:119;a:2:{s:8:""heroname"";s:10:""Hero_Klanx"";s:3:""exp"";s:4:""7816"";}i:120;a:2:{s:8:""heroname"";s:12:""Hero_Riptide"";s:3:""exp"";s:4:""4693"";}i:121;a:2:{s:8:""heroname"";s:10:""Hero_Moira"";s:3:""exp"";s:4:""2159"";}i:122;a:2:{s:8:""heroname"";s:10:""Hero_Tarot"";s:3:""exp"";s:5:""36100"";}i:123;a:2:{s:8:""heroname"";s:9:""Hero_Kane"";s:3:""exp"";s:4:""6147"";}i:124;a:2:{s:8:""heroname"";s:13:""Hero_Calamity"";s:3:""exp"";s:4:""1702"";}i:125;a:2:{s:8:""heroname"";s:13:""Hero_Deadlift"";s:3:""exp"";s:5:""24814"";}i:126;a:2:{s:8:""heroname"";s:13:""Hero_Parallax"";s:3:""exp"";s:4:""8903"";}i:127;a:2:{s:8:""heroname"";s:10:""Hero_Skrap"";s:3:""exp"";s:4:""7470"";}i:128;a:2:{s:8:""heroname"";s:10:""Hero_Nitro"";s:3:""exp"";s:5:""11670"";}i:129;a:2:{s:8:""heroname"";s:14:""Hero_KingKlout"";s:3:""exp"";s:4:""1102"";}i:130;a:2:{s:8:""heroname"";s:15:""Hero_Shellshock"";s:3:""exp"";s:5:""31210"";}i:131;a:2:{s:8:""heroname"";s:10:""Hero_Ichor"";s:3:""exp"";s:4:""5133"";}i:132;a:2:{s:8:""heroname"";s:15:""Hero_Adrenaline"";s:3:""exp"";s:5:""10733"";}i:133;a:2:{s:8:""heroname"";s:9:""Hero_Apex"";s:3:""exp"";s:4:""1872"";}i:134;a:2:{s:8:""heroname"";s:13:""Hero_Warchief"";s:3:""exp"";s:4:""6756"";}i:135;a:2:{s:8:""heroname"";s:13:""Hero_Sapphire"";s:3:""exp"";s:5:""17892"";}i:136;a:2:{s:8:""heroname"";s:15:""Hero_Goldenveil"";s:3:""exp"";s:4:""1636"";}i:137;a:2:{s:8:""heroname"";s:8:""Hero_Chi"";s:3:""exp"";s:5:""36100"";}i:138;a:2:{s:8:""heroname"";s:10:""Hero_Mimix"";s:3:""exp"";s:4:""7682"";}}s:15:""mastery_rewards"";a:24:{i:0;a:3:{s:5:""level"";i:1;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:3609;s:12:""product_name"";s:13:""Mastery Boost"";s:21:""product_local_content"";s:37:""/ui/fe2/store/icons/mastery_boost.tga"";s:8:""quantity"";i:2;s:6:""points"";i:0;s:8:""mmpoints"";i:0;s:7:""tickets"";i:0;}}i:1;a:3:{s:5:""level"";i:2;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:3860;s:12:""product_name"";s:18:""Iron Mastery Badge"";s:21:""product_local_content"";s:42:""/ui/fe2/store/icons/mastery_iron_badge.tga"";s:8:""quantity"";i:1;s:6:""points"";i:0;s:8:""mmpoints"";i:0;s:7:""tickets"";i:0;}}i:2;a:3:{s:5:""level"";i:3;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:0;s:12:""product_name"";s:0:"""";s:21:""product_local_content"";s:0:"""";s:8:""quantity"";i:0;s:6:""points"";i:0;s:8:""mmpoints"";i:75;s:7:""tickets"";i:0;}}i:3;a:3:{s:5:""level"";i:5;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:3609;s:12:""product_name"";s:13:""Mastery Boost"";s:21:""product_local_content"";s:37:""/ui/fe2/store/icons/mastery_boost.tga"";s:8:""quantity"";i:5;s:6:""points"";i:0;s:8:""mmpoints"";i:0;s:7:""tickets"";i:0;}}i:4;a:3:{s:5:""level"";i:7;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:0;s:12:""product_name"";s:0:"""";s:21:""product_local_content"";s:0:"""";s:8:""quantity"";i:0;s:6:""points"";i:0;s:8:""mmpoints"";i:100;s:7:""tickets"";i:0;}}i:5;a:3:{s:5:""level"";i:10;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:3861;s:12:""product_name"";s:20:""Bronze Mastery Badge"";s:21:""product_local_content"";s:44:""/ui/fe2/store/icons/mastery_bronze_badge.tga"";s:8:""quantity"";i:1;s:6:""points"";i:0;s:8:""mmpoints"";i:0;s:7:""tickets"";i:0;}}i:6;a:3:{s:5:""level"";i:15;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:0;s:12:""product_name"";s:0:"""";s:21:""product_local_content"";s:0:"""";s:8:""quantity"";i:0;s:6:""points"";i:0;s:8:""mmpoints"";i:150;s:7:""tickets"";i:0;}}i:7;a:3:{s:5:""level"";i:20;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:3609;s:12:""product_name"";s:13:""Mastery Boost"";s:21:""product_local_content"";s:37:""/ui/fe2/store/icons/mastery_boost.tga"";s:8:""quantity"";i:10;s:6:""points"";i:0;s:8:""mmpoints"";i:0;s:7:""tickets"";i:0;}}i:8;a:3:{s:5:""level"";i:25;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:0;s:12:""product_name"";s:0:"""";s:21:""product_local_content"";s:0:"""";s:8:""quantity"";i:0;s:6:""points"";i:0;s:8:""mmpoints"";i:175;s:7:""tickets"";i:0;}}i:9;a:3:{s:5:""level"";i:30;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:0;s:12:""product_name"";s:0:"""";s:21:""product_local_content"";s:0:"""";s:8:""quantity"";i:0;s:6:""points"";i:0;s:8:""mmpoints"";i:0;s:7:""tickets"";i:50;}}i:10;a:3:{s:5:""level"";i:35;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:4225;s:12:""product_name"";s:20:""Silver Mastery Badge"";s:21:""product_local_content"";s:44:""/ui/fe2/store/icons/mastery_silver_badge.tga"";s:8:""quantity"";i:1;s:6:""points"";i:0;s:8:""mmpoints"";i:0;s:7:""tickets"";i:0;}}i:11;a:3:{s:5:""level"";i:40;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:3609;s:12:""product_name"";s:13:""Mastery Boost"";s:21:""product_local_content"";s:37:""/ui/fe2/store/icons/mastery_boost.tga"";s:8:""quantity"";i:10;s:6:""points"";i:0;s:8:""mmpoints"";i:0;s:7:""tickets"";i:0;}}i:12;a:3:{s:5:""level"";i:45;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:0;s:12:""product_name"";s:0:"""";s:21:""product_local_content"";s:0:"""";s:8:""quantity"";i:0;s:6:""points"";i:0;s:8:""mmpoints"";i:0;s:7:""tickets"";i:100;}}i:13;a:3:{s:5:""level"";i:50;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:0;s:12:""product_name"";s:0:"""";s:21:""product_local_content"";s:0:"""";s:8:""quantity"";i:0;s:6:""points"";i:100;s:8:""mmpoints"";i:0;s:7:""tickets"";i:0;}}i:14;a:3:{s:5:""level"";i:55;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:4605;s:12:""product_name"";s:11:""Super boost"";s:21:""product_local_content"";s:43:""/ui/fe2/store/icons/mastery_super_boost.tga"";s:8:""quantity"";i:1;s:6:""points"";i:0;s:8:""mmpoints"";i:0;s:7:""tickets"";i:0;}}i:15;a:3:{s:5:""level"";i:60;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:4634;s:12:""product_name"";s:18:""Gold Mastery Badge"";s:21:""product_local_content"";s:42:""/ui/fe2/store/icons/mastery_gold_badge.tga"";s:8:""quantity"";i:1;s:6:""points"";i:0;s:8:""mmpoints"";i:0;s:7:""tickets"";i:0;}}i:16;a:3:{s:5:""level"";i:65;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:3609;s:12:""product_name"";s:13:""Mastery Boost"";s:21:""product_local_content"";s:37:""/ui/fe2/store/icons/mastery_boost.tga"";s:8:""quantity"";i:10;s:6:""points"";i:0;s:8:""mmpoints"";i:0;s:7:""tickets"";i:0;}}i:17;a:3:{s:5:""level"";i:70;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:0;s:12:""product_name"";s:0:"""";s:21:""product_local_content"";s:0:"""";s:8:""quantity"";i:0;s:6:""points"";i:0;s:8:""mmpoints"";i:0;s:7:""tickets"";i:100;}}i:18;a:3:{s:5:""level"";i:75;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:4605;s:12:""product_name"";s:11:""Super boost"";s:21:""product_local_content"";s:43:""/ui/fe2/store/icons/mastery_super_boost.tga"";s:8:""quantity"";i:1;s:6:""points"";i:0;s:8:""mmpoints"";i:0;s:7:""tickets"";i:0;}}i:19;a:3:{s:5:""level"";i:80;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:4635;s:12:""product_name"";s:18:""Epic Mastery Badge"";s:21:""product_local_content"";s:42:""/ui/fe2/store/icons/mastery_epic_badge.tga"";s:8:""quantity"";i:1;s:6:""points"";i:0;s:8:""mmpoints"";i:0;s:7:""tickets"";i:0;}}i:20;a:3:{s:5:""level"";i:85;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:3609;s:12:""product_name"";s:13:""Mastery Boost"";s:21:""product_local_content"";s:37:""/ui/fe2/store/icons/mastery_boost.tga"";s:8:""quantity"";i:10;s:6:""points"";i:0;s:8:""mmpoints"";i:0;s:7:""tickets"";i:0;}}i:21;a:3:{s:5:""level"";i:90;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:0;s:12:""product_name"";s:0:"""";s:21:""product_local_content"";s:0:"""";s:8:""quantity"";i:0;s:6:""points"";i:0;s:8:""mmpoints"";i:200;s:7:""tickets"";i:0;}}i:22;a:3:{s:5:""level"";i:95;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:4605;s:12:""product_name"";s:11:""Super boost"";s:21:""product_local_content"";s:43:""/ui/fe2/store/icons/mastery_super_boost.tga"";s:8:""quantity"";i:1;s:6:""points"";i:0;s:8:""mmpoints"";i:0;s:7:""tickets"";i:0;}}i:23;a:3:{s:5:""level"";i:100;s:10:""alreadygot"";b:1;s:6:""reward"";a:7:{s:10:""product_id"";i:0;s:12:""product_name"";s:0:"""";s:21:""product_local_content"";s:0:"""";s:8:""quantity"";i:0;s:6:""points"";i:100;s:8:""mmpoints"";i:0;s:7:""tickets"";i:0;}}}s:16:""vested_threshold"";i:5;i:0;b:1;}";

            // TODO: Use Real Response Data After Confirming The Correctness Of The Response Structure And Data

            return Ok(mockResponse);
        }

        throw new ArgumentOutOfRangeException(nameof(table), table, $@"Unsupported Value For Form Parameter ""table"": ""{table}""");
    }

    private async Task<IActionResult> GetMatchStatistics()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        string? matchID = Request.Form["match_id"];

        if (matchID is null)
            return BadRequest(@"Missing Value For Form Parameter ""match_id""");

        MatchStatistics? matchStatistics = await MerrickContext.MatchStatistics.SingleOrDefaultAsync(matchStatistics => matchStatistics.MatchID == int.Parse(matchID));

        if (matchStatistics is null)
            return new NotFoundObjectResult("Match Stats Not Found");

        List<MatchParticipantStatistics> allPlayerStatistics = await MerrickContext.MatchParticipantStatistics.Where(playerStatistics => playerStatistics.MatchID == matchStatistics.MatchID).ToListAsync();

        string? accountName = await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
            return new NotFoundObjectResult("Session Not Found");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return new NotFoundObjectResult("Account Not Found");

        MatchInformation? matchInformation = await DistributedCache.GetMatchInformation(matchStatistics.MatchID);

        if (matchInformation is null)
            return new NotFoundObjectResult("Match Information Not Found");

        MatchSummary matchSummary = new (matchStatistics, allPlayerStatistics, matchInformation);

        List<int> otherPlayerAccountIDs = [.. allPlayerStatistics.Select(statistics => statistics.AccountID).Where(id => id != account.ID)];

        List<Account> otherPlayerAccounts = await MerrickContext.Accounts
            .Include(playerAccount => playerAccount.User)
            .Include(playerAccount => playerAccount.Clan)
            .Where(playerAccount => otherPlayerAccountIDs.Contains(playerAccount.ID))
            .ToListAsync();

        List<Account> allPlayerAccounts = [account, .. otherPlayerAccounts];

        Dictionary<int, OneOf<MatchPlayerStatisticsWithMatchPerformanceData, MatchPlayerStatistics>> matchPlayerStatistics = [];
        Dictionary<int, MatchPlayerInventory> matchPlayerInventories = [];

        foreach (MatchParticipantStatistics playerStatistics in allPlayerStatistics)
        {
            Account playerAccount = allPlayerAccounts.Single(playerAccount => playerAccount.ID == playerStatistics.AccountID);

            List<AccountStatistics> accountStatistics = await MerrickContext.AccountStatistics.Where(statistics => statistics.AccountID == playerStatistics.AccountID).ToListAsync();

            // TODO: Figure Out How To Select Which Statistics To Use (Public Match, Matchmaking, etc.)
            // INFO: Currently, This Code Logic Assumes A Public Match
            // INFO: Potential Logic + Switch/Case On Map Name: bool isPublic = form.player_stats.First().Value.First().Value.pub_count == 1;

            AccountStatistics currentMatchTypeStatistics = accountStatistics.Single(statistics => statistics.Type == AccountStatisticsType.Public);

            // TODO: Increment Current Match Type Statistics With Current Match Data

            AccountStatistics publicMatchStatistics = accountStatistics.Single(statistics => statistics.Type == AccountStatisticsType.Public);

            // TODO: Increment Public Match Statistics With Current Match Data

            AccountStatistics matchmakingStatistics = accountStatistics.Single(statistics => statistics.Type == AccountStatisticsType.Matchmaking);

            // TODO: Increment Matchmaking Statistics With Current Match Data

            // Use PrimaryMatchPlayerStatistics With Additional Information For The Primary (Requesting) Player And MatchPlayerStatistics With The Standard Amount Of Information For Secondary Players
            matchPlayerStatistics[playerStatistics.AccountID] = playerStatistics.AccountID == account.ID
                ? new MatchPlayerStatisticsWithMatchPerformanceData(matchInformation, playerAccount, playerStatistics, currentMatchTypeStatistics, publicMatchStatistics, matchmakingStatistics)
                    { HeroIdentifier = playerStatistics.HeroIdentifier }
                : new MatchPlayerStatistics(matchInformation, playerAccount, playerStatistics, currentMatchTypeStatistics, publicMatchStatistics, matchmakingStatistics)
                    { HeroIdentifier = playerStatistics.HeroIdentifier };

            List<string> inventory = playerStatistics.Inventory ?? [];

            matchPlayerInventories[playerStatistics.AccountID] = new MatchPlayerInventory
            {
                AccountID = playerStatistics.AccountID,
                MatchID = playerStatistics.MatchID,

                Slot1 = inventory.ElementAtOrDefault(0),
                Slot2 = inventory.ElementAtOrDefault(1),
                Slot3 = inventory.ElementAtOrDefault(2),
                Slot4 = inventory.ElementAtOrDefault(3),
                Slot5 = inventory.ElementAtOrDefault(4),
                Slot6 = inventory.ElementAtOrDefault(5)
            };
        }

        MatchParticipantStatistics requestingPlayerStatistics = allPlayerStatistics.Single(statistics => statistics.AccountID == account.ID);

        MatchMastery matchMastery = new
        (
            heroIdentifier: requestingPlayerStatistics.HeroIdentifier,
            currentMasteryExperience: 0, // TODO: Retrieve From Mastery System Once Re-Implemented
            matchMasteryExperience: 100, // TODO: Calculate Based On Match Duration And Result (Use Calculation That I Implemented In Legacy PK)
            bonusExperience: 10 // TODO: Calculate Based On Max-Level Heroes Owned
        )
        {
            MasteryExperienceMaximumLevelHeroesCount = 0, // TODO: Count Heroes At Max Mastery Level (+ Enable MatchMastery Constructor Once Masteries Are Re-Implemented)
            MasteryExperienceBoostProductCount = 0, // TODO: Count "ma.Mastery Boost" Items (+ Enable MatchMastery Constructor Once Masteries Are Re-Implemented)
            MasteryExperienceSuperBoostProductCount = 0 // TODO: Count "ma.Super Mastery Boost" Items (+ Enable MatchMastery Constructor Once Masteries Are Re-Implemented)
        };

        MatchStatsResponse response = new ()
        {
            GoldCoins = account.User.GoldCoins.ToString(),
            SilverCoins = account.User.SilverCoins.ToString(),
            MatchSummary = new Dictionary<int, MatchSummary> { { matchStatistics.MatchID, matchSummary } },
            MatchPlayerStatistics = new Dictionary<int, Dictionary<int, OneOf<MatchPlayerStatisticsWithMatchPerformanceData, MatchPlayerStatistics>>> { { matchStatistics.MatchID, matchPlayerStatistics } },
            MatchPlayerInventories = new Dictionary<int, Dictionary<int, MatchPlayerInventory>> { { matchStatistics.MatchID, matchPlayerInventories } },
            MatchMastery = matchMastery,
            OwnedStoreItems = account.User.OwnedStoreItems,
            OwnedStoreItemsData = SetOwnedStoreItemsData(account),
            SelectedStoreItems = account.SelectedStoreItems,
            CustomIconSlotID = SetCustomIconSlotID(account)
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    private static string SetCustomIconSlotID(Account account)
        => account.SelectedStoreItems.Any(item => item.StartsWith("ai.custom_icon"))
            ? account.SelectedStoreItems.Single(item => item.StartsWith("ai.custom_icon")).Replace("ai.custom_icon:", string.Empty) : "0";

    private static Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> SetOwnedStoreItemsData(Account account)
    {
        Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> items = account.User.OwnedStoreItems
            .Where(item => item.StartsWith("ma.").Equals(false) && item.StartsWith("cp.").Equals(false))
            .ToDictionary<string, string, OneOf<StoreItemData, StoreItemDiscountCoupon>>(upgrade => upgrade, upgrade => new StoreItemData());

        // TODO: Add Mastery Boosts And Coupons

        /*
            Dictionary<string, object> myUpgradesInfo = accountDetails.UnlockedUpgradeCodes
                .Where(upgrade => upgrade.StartsWith("ma.").Equals(false) && upgrade.StartsWith("cp.").Equals(false))
                .ToDictionary<string, string, object>(upgrade => upgrade, upgrade => new MyUpgradesInfoEntry());

            foreach (string boost in GameConsumables.GetOwnedMasteryBoostProducts(accountDetails.UnlockedUpgradeCodes))
                myUpgradesInfo.Add(boost, new MyUpgradesInfoEntry());

            foreach (KeyValuePair<string, Coupon> coupon in GameConsumables.GetOwnedCoupons(accountDetails.UnlockedUpgradeCodes))
                myUpgradesInfo.Add(coupon.Key, coupon.Value);

            return myUpgradesInfo;
         */

        return items;
    }
}
