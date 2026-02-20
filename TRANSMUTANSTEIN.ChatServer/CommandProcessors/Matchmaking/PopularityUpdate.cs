namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_POPULARITY_UPDATE)]
public class PopularityUpdate : ISynchronousCommandProcessor<ClientChatSession>
{
    /// <summary>
    ///     Map name to <see cref="ChatProtocol.TMMGameMap"/> enum value lookup.
    /// </summary>
    private static readonly Dictionary<string, int> MapNameRegistry = new (StringComparer.OrdinalIgnoreCase)
    {
        ["caldavar"]        = (int)ChatProtocol.TMMGameMap.TMM_GAME_MAP_FORESTS_OF_CALDAVAR,
        ["grimms_crossing"] = (int)ChatProtocol.TMMGameMap.TMM_GAME_MAP_GRIMMS_CROSSING,
        ["midwars"]         = (int)ChatProtocol.TMMGameMap.TMM_GAME_MAP_MIDWARS,
        ["riftwars"]        = (int)ChatProtocol.TMMGameMap.TMM_GAME_MAP_RIFTWARS,
        ["team_deathmatch"] = (int)ChatProtocol.TMMGameMap.TMM_GAME_MAP_TEAM_DEATHMATCH
    };

    /// <summary>
    ///     Game mode short code to <see cref="ChatProtocol.TMMGameMode"/> enum value lookup.
    /// </summary>
    private static readonly Dictionary<string, int> GameModeRegistry = new (StringComparer.OrdinalIgnoreCase)
    {
        ["ap"]  = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_ALL_PICK,
        ["apg"] = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_ALL_PICK_GATED,
        ["apd"] = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_ALL_PICK_DUPLICATE_HERO,
        ["sd"]  = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_SINGLE_DRAFT,
        ["bd"]  = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_BANNING_DRAFT,
        ["bp"]  = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_BANNING_PICK,
        ["ar"]  = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_ALL_RANDOM,
        ["lp"]  = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_LOCK_PICK,
        ["bb"]  = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_BLIND_BAN,
        ["bbg"] = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_BLIND_BAN_GATED,
        ["bbr"] = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_BLIND_BAN_RAPID_FIRE,
        ["bm"]  = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_BOT_MATCH,
        ["cm"]  = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_CAPTAINS_PICK,
        ["br"]  = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_BALANCED_RANDOM,
        ["km"]  = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_KROS_MODE,
        ["rd"]  = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_RANDOM_DRAFT,
        ["bdr"] = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_BANNING_DRAFT_RAPID_FIRE,
        ["cp"]  = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_COUNTER_PICK,
        ["fp"]  = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_FORCE_PICK,
        ["sp"]  = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_SOCCER_PICK,
        ["ss"]  = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_SOLO_SAME,
        ["sm"]  = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_SOLO_DIFF,
        ["hb"]  = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_HERO_BAN,
        ["mwb"] = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_MIDWARS_BETA,
        ["rb"]  = (int)ChatProtocol.TMMGameMode.TMM_GAME_MODE_REBORN
    };

    /// <summary>
    ///     Region code to <see cref="ChatProtocol.TMMGameRegion"/> enum value lookup.
    /// </summary>
    private static readonly Dictionary<string, int> RegionRegistry = new (StringComparer.OrdinalIgnoreCase)
    {
        ["USE"] = (int)ChatProtocol.TMMGameRegion.TMM_GAME_REGION_USE,
        ["USW"] = (int)ChatProtocol.TMMGameRegion.TMM_GAME_REGION_USW,
        ["EU"]  = (int)ChatProtocol.TMMGameRegion.TMM_GAME_REGION_EU,
        ["SG"]  = (int)ChatProtocol.TMMGameRegion.TMM_GAME_REGION_SG,
        ["MY"]  = (int)ChatProtocol.TMMGameRegion.TMM_GAME_REGION_MY,
        ["PH"]  = (int)ChatProtocol.TMMGameRegion.TMM_GAME_REGION_PH,
        ["TH"]  = (int)ChatProtocol.TMMGameRegion.TMM_GAME_REGION_TH,
        ["ID"]  = (int)ChatProtocol.TMMGameRegion.TMM_GAME_REGION_ID,
        ["VN"]  = (int)ChatProtocol.TMMGameRegion.TMM_GAME_REGION_VN,
        ["RU"]  = (int)ChatProtocol.TMMGameRegion.TMM_GAME_REGION_RU,
        ["KR"]  = (int)ChatProtocol.TMMGameRegion.TMM_GAME_REGION_KR,
        ["AU"]  = (int)ChatProtocol.TMMGameRegion.TMM_GAME_REGION_AU,
        ["LAT"] = (int)ChatProtocol.TMMGameRegion.TMM_GAME_REGION_LAT,
        ["DX"]  = (int)ChatProtocol.TMMGameRegion.TMM_GAME_REGION_DX,
        ["CN"]  = (int)ChatProtocol.TMMGameRegion.TMM_GAME_REGION_CN,
        ["BR"]  = (int)ChatProtocol.TMMGameRegion.TMM_GAME_REGION_BR,
        ["TR"]  = (int)ChatProtocol.TMMGameRegion.TMM_GAME_REGION_TR
    };

    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        PopularityUpdateRequestData requestData = new (buffer);

        SendMatchmakingPopularity(session);
    }

    public static void SendMatchmakingPopularity(ClientChatSession session)
    {
        MatchmakingConfiguration configuration = JSONConfiguration.MatchmakingConfiguration;

        List<string> maps = [.. configuration.Maps.Select(mapConfiguration => mapConfiguration.Map)];
        List<int> gameTypes = [.. configuration.Maps.SelectMany(mapConfiguration => mapConfiguration.GameTypes)];
        List<string> gameModes = [.. configuration.GameModes];
        List<string> regions = [.. configuration.Regions];

        // Compute Disabled Game Modes By Map: For Each Map, Any Global Mode Not In The Map's Mode List Is Disabled
        HashSet<string> globalModeSet = [.. gameModes];

        List<string> disabledGameModesByMap = [.. configuration.Maps
            .SelectMany(mapConfiguration => globalModeSet
                .Except(mapConfiguration.Modes, StringComparer.OrdinalIgnoreCase)
                .Select(disabledMode => $"{mapConfiguration.Map}->{disabledMode}"))];

        List<string> disabledGameModesByGameType = [];
        List<string> disabledGameModesByRankType = [];
        List<string> disabledRegions = [];

        // TODO: Use Geo-Location Over An Internet Connection (List Of Country Codes: https://www.iban.com/country-codes)
        string clientCountryCode = string.Empty;

        // Build The Legend From Enum Lookups (Maps, Modes, Regions)
        StringBuilder legend = new ();

        legend.Append("maps:");

        foreach (string map in maps)
            if (MapNameRegistry.TryGetValue(map, out int value))
                legend.Append(map).Append('-').Append(value).Append('|');

        legend.Append("modes:");

        foreach (string mode in gameModes)
            if (GameModeRegistry.TryGetValue(mode, out int value))
                legend.Append(mode).Append('-').Append(value).Append('|');

        legend.Append("regions:");

        foreach (string region in regions)
            if (RegionRegistry.TryGetValue(region, out int value))
                legend.Append(region).Append('-').Append(value).Append('|');

        ChatBuffer response = new ();

        response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_POPULARITY_UPDATE);

        response.WriteInt8(1);                                               // TMM Availability (0 If No Regions Are Enabled, Or 1 Otherwise)
        response.WriteString(string.Join('|', maps));                        // Available TMM Maps
        response.WriteString(string.Join('|', gameTypes));                   // Available TMM Game Types (Only Used By The Old UI; Needs To Match The List Of Available TMM Maps)
        response.WriteString(string.Join('|', gameModes));                   // Available TMM Game Modes
        response.WriteString(string.Join('|', regions));                     // Available TMM Regions
        response.WriteString(string.Join('|', disabledGameModesByGameType)); // Disabled Game Modes By Game Type
        response.WriteString(string.Join('|', disabledGameModesByRankType)); // Disabled Game Modes By Rank Type
        response.WriteString(string.Join('|', disabledGameModesByMap));      // Disabled Game Modes By Map
        response.WriteString(string.Join('|', disabledRegions));             // Disabled TMM Regions
        response.WriteString(clientCountryCode);                             // Client Country Code
        response.WriteString(legend.ToString());                             // TMM Legend

        List<int> rankTypes =
        [
            Convert.ToInt32(ChatProtocol.TMMRankType.TMM_OPTION_UNRANKED),
            Convert.ToInt32(ChatProtocol.TMMRankType.TMM_OPTION_RANKED)
        ];

        // Popularity By Game Map, Ranges From 0 (Lowest) To 10 (Highest)
        foreach (string map in maps)
            foreach (int gameType in gameTypes)
                foreach (int rankType in rankTypes)
                    response.WriteInt8(10);

        // Popularity By Game Type, Ranges From 0 (Lowest) To 10 (Highest)
        foreach (int gameType in gameTypes)
            foreach (string map in maps)
                foreach (int rankType in rankTypes)
                    response.WriteInt8(10);

        // Popularity By Game Mode, Ranges From 0 (Lowest) To 10 (Highest)
        foreach (string mode in gameModes)
            foreach (string map in maps)
                foreach (int gameType in gameTypes)
                    foreach (int rankType in rankTypes)
                        response.WriteInt8(10);

        // Popularity By Region, Ranges From 0 (Lowest) To 10 (Highest)
        foreach (string region in regions)
            foreach (string map in maps)
                foreach (int gameType in gameTypes)
                    foreach (int rankType in rankTypes)
                        response.WriteInt8(10);

        // Custom Map Rotation End Time (As UNIX Epoch Time); Values In The Past = Disabled
        int customMapRotationTime = Convert.ToInt32(DateTimeOffset.UnixEpoch.ToUnixTimeSeconds());

        response.WriteInt32(customMapRotationTime);

        session.Send(response);
    }
}

file class PopularityUpdateRequestData
{
    public byte[] CommandBytes { get; init; }

    public PopularityUpdateRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }
}
