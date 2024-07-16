namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_POPULARITY_UPDATE)]
public class PopularityUpdate(MerrickContext merrick, ILogger<PopularityUpdate> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<PopularityUpdate> Logger { get; set; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        PopularityUpdateRequestData requestData = new(buffer);

        await SendMatchmakingPopularity(session, buffer, Response);
    }

    public static async Task SendMatchmakingPopularity(TCPSession session, ChatBuffer buffer, ChatBuffer response)
    {
        // TODO: Get All Maps And Compile List
        List<string> maps = ["caldavar", "midwars", "riftwars"];

        List<int> gameTypes =
        [
            Convert.ToInt32(ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL),
            Convert.ToInt32(ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS),
            Convert.ToInt32(ChatProtocol.TMMGameType.TMM_GAME_TYPE_RIFTWARS)
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

        response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_POPULARITY_UPDATE);

        response.WriteInt8(1);                                                  // TMM Availability (0 If No Regions Are Enabled, Or 1 Otherwise)
        response.WriteString(string.Join('|', maps));                           // Available TMM Maps
        response.WriteString(string.Join('|', gameTypes));                      // Available TMM Game Types (Only Used By The Old UI; Needs To Match The List Of Available TMM Maps)
        response.WriteString(string.Join('|', gameModes));                      // Available TMM Game Modes
        response.WriteString(string.Join('|', regions));                        // Available TMM Regions
        response.WriteString(string.Join('|', disabledGameModesByGameType));    // Disabled Game Modes By Game Type
        response.WriteString(string.Join('|', disabledGameModesByRankType));    // Disabled Game Modes By Rank Type
        response.WriteString(string.Join('|', disabledGameModesByMap));         // Disabled Game Modes By Map
        response.WriteString(string.Join('|', disabledRegions));                // Disabled TMM Regions
        response.WriteString(clientCountryCode);                                // Client Country Code
        response.WriteString(legend);                                           // TMM Legend

        List<int> rankTypes =
        [
            Convert.ToInt32(ChatProtocol.TMMRankType.TMM_OPTION_UNRANKED),
            Convert.ToInt32(ChatProtocol.TMMRankType.TMM_OPTION_RANKED)
        ];

        // Popularity By Game Map, Ranges From 0 (Lowest) To 10 (Highest)
        foreach (string _1 in maps)
            foreach (int _2 in gameTypes)
                foreach (int _3 in rankTypes)
                    response.WriteInt8(10);

        // Popularity By Game Type, Ranges From 0 (Lowest) To 10 (Highest)
        foreach (int _1 in gameTypes)
            foreach (string _2 in maps)
                foreach (int _3 in rankTypes)
                    response.WriteInt8(10);

        // Popularity By Game Mode, Ranges From 0 (Lowest) To 10 (Highest)
        foreach (string _1 in legendModes)
            foreach (string _2 in maps)
                foreach (int _3 in gameTypes)
                    foreach (int _4 in rankTypes)
                        response.WriteInt8(10);

        // Popularity By Region, Ranges From 0 (Lowest) To 10 (Highest)
        foreach (string _1 in legendRegions)
            foreach (string _2 in maps)
                foreach (int _3 in gameTypes)
                    foreach (int _4 in rankTypes)
                        response.WriteInt8(10);

        // Custom Map Rotation End Time (As UNIX Epoch Time); Values In The Past = Disabled
        int customMapRotationTime = Convert.ToInt32(DateTimeOffset.UnixEpoch.ToUnixTimeSeconds());

        response.WriteInt32(customMapRotationTime);

        response.PrependBufferSize();

        session.SendAsync(response.Data);
    }
}

public class PopularityUpdateRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
}
