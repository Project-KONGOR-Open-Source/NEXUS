namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Statistics;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_CAMPAIGN_STATS)]
public class SeasonStats(MerrickContext merrick, ILogger<SeasonStats> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<SeasonStats> Logger { get; set; } = logger;

    public async Task Process(TCPSession session, ChatBuffer buffer)
    {
        // TODO: Create Response Models

        await SendSeasonStatistics(session, buffer);

        Response = new ChatBuffer(); // Also Respond With NET_CHAT_CL_TMM_POPULARITY_UPDATE Since The Client Will Not Explicitly Request It

        await SendMatchmakingPopularity(session, buffer);
    }

    private async Task SendSeasonStatistics(TCPSession session, ChatBuffer buffer)
    {
        Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_CAMPAIGN_STATS);

        // TODO: Send Actual Season Statistics

        Response.WriteFloat32(1850.55f);    // TMM Rating
        Response.WriteInt32(15);            // TMM Rank
        Response.WriteInt32(6661);          // TMM Wins
        Response.WriteInt32(123);           // TMM Losses
        Response.WriteInt32(6662);          // Ranked Win Streak
        Response.WriteInt32(6663);          // Ranked Matches Played
        Response.WriteInt32(5);             // Placement Matches Played
        Response.WriteString("11011");      // Placement Status
        Response.WriteFloat32(1950.55f);    // Casual TMM Rating
        Response.WriteInt32(10);            // Casual TMM Rank
        Response.WriteInt32(4441);          // Casual TMM Wins
        Response.WriteInt32(321);           // Casual TMM Losses
        Response.WriteInt32(4442);          // Casual Ranked Win Streak
        Response.WriteInt32(4443);          // Casual Ranked Matches Played
        Response.WriteInt32(6);             // Casual Placement Matches Played
        Response.WriteString("010101");     // Casual Placement Status
        Response.WriteInt8(1);              // Eligible For TMM
        Response.WriteInt8(1);              // Season End

        Response.PrependBufferSize();

        session.SendAsync(Response.Data);
    }

    private async Task SendMatchmakingPopularity(TCPSession session, ChatBuffer buffer)
    {
        // TODO: Get All Maps And Compile List
        List<string> maps = ["caldavar", "midwars", "riftwars"];

        List<int> gameTypes =
        [
            Convert.ToInt32(ChatProtocol.TMMGameTypes.TMM_GAME_TYPE_CAMPAIGN_NORMAL),
            Convert.ToInt32(ChatProtocol.TMMGameTypes.TMM_GAME_TYPE_MIDWARS),
            Convert.ToInt32(ChatProtocol.TMMGameTypes.TMM_GAME_TYPE_RIFTWARS)
        ];

        // ALL_PICK: ap, ALL_PICK_GATED: apg, ALL_PICK_DUPLICATE_HEROES: apd, SINGLE_DRAFT: sd, BANNING_DRAFT: bd, BANNING_PICK: bp, ALL_RANDOM: ar, LOCK_PICK: lp, BLIND_BAN: bb, BLIND_BAN_GATED: bbg, BLIND_BAN_RAPID_FIRE: bbr, BOT_MATCH: bm, CAPTAINS_PICK: cm, BALANCED_RANDOM: br, KROS_MODE: km, RANDOM_DRAFT: rd, BANNING_DRAFT_RAPID_FIRE: bdr, COUNTER_PICK: cp, FORCE_PICK: fp, SOCCER_PICK: sp, SOLO_SAME: ss, SOLO_DIFF: sm, HERO_BAN: hb, MIDWARS_BETA: mwb, REBORN: rb
        List<string> gameModes = ["ap", "sd", "ar", "km", "hb", "rb"];

        // USE, USW, EU, SG, MY, PH, TH, ID, VN, RU, KR, AU, LAT, DX, CN, BR, TR
        List<string> regions = ["EU", "USE", "USW", "AU", "BR", "RU"];

        List<string> disabledGameModesByGameType = Enumerable.Empty<string>().ToList();
        List<string> disabledGameModesByRankType = Enumerable.Empty<string>().ToList();

        List<string> disabledGameModesByMap =
        [
            "caldavar->km", "caldavar->hb",
            "midwars->ap", "midwars->sd", "midwars->rb",
            "riftwars->sd", "riftwars->km", "riftwars->hb", "riftwars->rb",
            "team_deathmatch->ap", "team_deathmatch->sd", "team_deathmatch->ar", "team_deathmatch->km", "team_deathmatch->hb", "team_deathmatch->rb"
        ];

        List<string> disabledRegions = Enumerable.Empty<string>().ToList();

        // TODO: Use Geo-Location Over An Internet Connection (List Of Country Codes: https://www.iban.com/country-codes)
        string clientCountryCode = string.Empty;

        // TODO: Create Static Type For Maps/Modes/Regions
        List<string> legendMaps = ["caldavar-0", "midwars-2", "riftwars-3"];
        List<string> legendModes = ["ap-0", "sd-3", "ar-6", "km-14", "hb-22", "rb-24"];
        List<string> legendRegions = ["EU-2", "USE-0", "USW-1", "AU-11", "BR-15", "RU-9"];

        string legend = new StringBuilder()
            .Append("maps:").Append(string.Concat(legendMaps.Select(map => map + '|')))
            .Append("modes:").Append(string.Concat(legendModes.Select(mode => mode + '|')))
            .Append("regions:").Append(string.Concat(legendRegions.Select(region => region + '|')))
            .ToString();

        Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_POPULARITY_UPDATE);

        Response.WriteInt8(1);                                                  // TMM Availability (0 If No Regions Are Enabled, Or 1 Otherwise)
        Response.WriteString(string.Join('|', maps));                           // Available TMM Maps
        Response.WriteString(string.Join('|', gameTypes));                      // Available TMM Game Types (Only Used By The Old UI; Needs To Match The List Of Available TMM Maps)
        Response.WriteString(string.Join('|', gameModes));                      // Available TMM Game Modes
        Response.WriteString(string.Join('|', regions));                        // Available TMM Regions
        Response.WriteString(string.Join('|', disabledGameModesByGameType));    // Disabled Game Modes By Game Type
        Response.WriteString(string.Join('|', disabledGameModesByRankType));    // Disabled Game Modes By Rank Type
        Response.WriteString(string.Join('|', disabledGameModesByMap));         // Disabled Game Modes By Map
        Response.WriteString(string.Join('|', disabledRegions));                // Disabled TMM Regions
        Response.WriteString(clientCountryCode);                                // Client Country Code
        Response.WriteString(legend);                                           // TMM Legend

        List<int> rankTypes =
        [
            Convert.ToInt32(ChatProtocol.TMMRankType.TMM_OPTION_UNRANKED),
            Convert.ToInt32(ChatProtocol.TMMRankType.TMM_OPTION_RANKED)
        ];

        // Popularity By Game Map, Ranges From 0 (Lowest) To 10 (Highest)
        foreach (string _1 in maps)
            foreach (int _2 in gameTypes)
                foreach (int _3 in rankTypes)
                    Response.WriteInt8(10);

        // Popularity By Game Type, Ranges From 0 (Lowest) To 10 (Highest)
        foreach (int _1 in gameTypes)
            foreach (string _2 in maps)
                foreach (int _3 in rankTypes)
                    Response.WriteInt8(10);

        // Popularity By Game Mode, Ranges From 0 (Lowest) To 10 (Highest)
        foreach (string _1 in legendModes)
            foreach (string _2 in maps)
                foreach (int _3 in gameTypes)
                    foreach (int _4 in rankTypes)
                        Response.WriteInt8(10);

        // Popularity By Region, Ranges From 0 (Lowest) To 10 (Highest)
        foreach (string _1 in legendRegions)
            foreach (string _2 in maps)
                foreach (int _3 in gameTypes)
                    foreach (int _4 in rankTypes)
                        Response.WriteInt8(10);

        // Custom Map Rotation End Time (As UNIX Epoch Time); Values In The Past = Disabled
        int customMapRotationTime = Convert.ToInt32(DateTimeOffset.UnixEpoch.ToUnixTimeSeconds());

        Response.WriteInt32(customMapRotationTime);

        Response.PrependBufferSize();

        session.SendAsync(Response.Data);
    }
}
