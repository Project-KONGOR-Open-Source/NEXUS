namespace KINESIS.Matchmaking;

public class CreateMatchmakingGroupRequest : ProtocolRequest
{
    private readonly string Version;
    private readonly byte GroupType; // 2 - midwars/riftwars, 3 - botmatch, 4 - ranked.
    private readonly GameFinder.TMMGameType GameType;
    private readonly string MapNames;
    private readonly string GameModes;
    private readonly string Regions;
    private readonly byte Ranked;
    private readonly byte MatchFidelity;
    private readonly byte BotDifficulty;
    private readonly byte RandomizeBots;

    public CreateMatchmakingGroupRequest(string version, byte groupType, GameFinder.TMMGameType gameType, string mapNames, string gameModes, string regions, byte ranked, byte matchFidelity, byte botDifficulty, byte randomizeBots)
    {
        Version = version;
        GroupType = groupType;
        GameType = gameType;
        MapNames = mapNames;
        GameModes = gameModes;
        Regions = regions;
        Ranked = ranked;
        MatchFidelity = matchFidelity;
        BotDifficulty = botDifficulty;
        RandomizeBots = randomizeBots;
    }

    public static CreateMatchmakingGroupRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        CreateMatchmakingGroupRequest createMatchmakingGroupRequest = new CreateMatchmakingGroupRequest(
            version: ReadString(data, offset, out offset),
            groupType: ReadByte(data, offset, out offset),
            gameType: (GameFinder.TMMGameType)ReadByte(data, offset, out offset),
            mapNames: ReadString(data, offset, out offset),
            gameModes: ReadString(data, offset, out offset),
            regions: ReadString(data, offset, out offset),
            ranked: ReadByte(data, offset, out offset),
            matchFidelity: ReadByte(data, offset, out offset),
            botDifficulty: ReadByte(data, offset, out offset),
            randomizeBots: ReadByte(data, offset, out offset)
        );
        updatedOffset = offset;
        return createMatchmakingGroupRequest;
    }

}

