namespace KINESIS.Matchmaking;

public class MatchmakingSettingsResponse : ProtocolResponse
{
    // Matchmaking Availability Boolean (0: Unavailable, 1: Available)
    private readonly byte MatchmakingAvailability;

    // Pipe Separated List Of Available Maps
    // grimmscrossing, midwars, riftwars, prophets, thegrimmhunt, capturetheflag, devowars, soccer, solomap, team_deathmatch, caldavar_reborn, midwars_reborn
    private readonly string AvailableMaps;

    // Pipe Separated List Of Available Game Types
    // Type of `enum TmmGameTypes`
    private readonly string GameTypes;

    // Pipe Separated List Of Available Game Modes
    // ALL_PICK: ap, ALL_PICK_GATED: apg, ALL_PICK_DUPLICATE_HEROES: apd, SINGLE_DRAFT: sd, BANNING_DRAFT: bd, BANNING_PICK: bp, ALL_RANDOM: ar, LOCK_PICK: lp, BLIND_BAN: bb, BLIND_BAN_GATED: bbg, BLIND_BAN_RAPID_FIRE: bbr, BOT_MATCH: bm, CAPTAINS_PICK: cm, BALANCED_RANDOM: br, KROS_MODE: km, RANDOM_DRAFT: rd, BANNING_DRAFT_RAPID_FIRE: bdr, COUNTER_PICK: cp, FORCE_PICK: fp, SOCCER_PICK: sp, SOLO_SAME: ss, SOLO_DIFF: sm, HERO_BAN: hb, MIDWARS_BETA: mwb, REBORN: rb
    private readonly string GameModes;

    // Pipe Separated List Of Available Regions
    // USE, USW, EU, SG, MY, PH, TH, ID, VN, RU, KR, AU, LAT, DX, CN, BR, TR
    private readonly string AvailableRegions;

    // Pipe Separated List Of Disabled Game Modes By Game Type
    private readonly string DisabledGameModesByGameType;

    // Pipe Separated List Of Disabled Game Modes By Rank Type
    private readonly string DisabledGameModesByRankType;

    // Pipe Separated List Of Disabled Game Modes By Map
    private readonly string DisabledGameModesByMap;

    // Pipe Separated List Of Restricted Regions
    private readonly string RestrictedRegions;

    // Client Country Code
    private readonly string ClientCountryCode;

    // Pipe Separated Legend
    private readonly string Legend;

    // Popularity By Game Map, Ranges From 0 (Lowest) To 10 (Highest)
    private readonly byte[] PopularityByGameMap;

    // Popularity By Game Type, Ranges From 0 (Lowest) To 10 (Highest)
    private readonly byte[] PopularityByGameType;

    // Popularity By Game Mode, Ranges From 0 (Lowest) To 10 (Highest)
    private readonly byte[] PopularityByGameMode;

    // Popularity By Region, Ranges From 0 (Lowest) To 10 (Highest)
    private readonly byte[] PopularityByRegion;

    // Custom Map Rotation Time
    private readonly int CustomMapRotationTime;

    public MatchmakingSettingsResponse(byte matchmakingAvailability, string availableMaps, string gameTypes, string gameModes, string availableRegions, string disabledGameModesByGameType, string disabledGameModesByRankType, string disabledGameModesByMap, string restrictedRegions, string clientCountryCode, string legend, byte[] popularityByGameMap, byte[] popularityByGameType, byte[] popularityByGameMode, byte[] popularityByRegion, int customMapRotationTime)
    {
        MatchmakingAvailability = matchmakingAvailability;
        AvailableMaps = availableMaps;
        GameTypes = gameTypes;
        GameModes = gameModes;
        AvailableRegions = availableRegions;
        DisabledGameModesByGameType = disabledGameModesByGameType;
        DisabledGameModesByRankType = disabledGameModesByRankType;
        DisabledGameModesByMap = disabledGameModesByMap;
        RestrictedRegions = restrictedRegions;
        ClientCountryCode = clientCountryCode;
        Legend = legend;
        PopularityByGameMap = popularityByGameMap;
        PopularityByGameType = popularityByGameType;
        PopularityByGameMode = popularityByGameMode;
        PopularityByRegion = popularityByRegion;
        CustomMapRotationTime = customMapRotationTime;
    }

    public override CommandBuffer Encode()
    {
        CommandBuffer buffer = new();
        buffer.WriteInt16(ChatServerResponse.MatchmakingSettingsResponse);
        buffer.WriteInt8(MatchmakingAvailability);
        buffer.WriteString(AvailableMaps);
        buffer.WriteString(GameTypes);
        buffer.WriteString(GameModes);
        buffer.WriteString(AvailableRegions);
        buffer.WriteString(DisabledGameModesByGameType);
        buffer.WriteString(DisabledGameModesByRankType);
        buffer.WriteString(DisabledGameModesByMap);
        buffer.WriteString(RestrictedRegions);
        buffer.WriteString(ClientCountryCode);
        buffer.WriteString(Legend);

        foreach (byte b in PopularityByGameMap)
        {
            buffer.WriteInt8(b);
        }

        foreach (byte b in PopularityByGameType)
        {
            buffer.WriteInt8(b);
        }

        foreach (byte b in PopularityByGameMode)
        {
            buffer.WriteInt8(b);
        }

        foreach (byte b in PopularityByRegion)
        {
            buffer.WriteInt8(b);
        }

        buffer.WriteInt32(CustomMapRotationTime);

        return buffer;
    }
}
