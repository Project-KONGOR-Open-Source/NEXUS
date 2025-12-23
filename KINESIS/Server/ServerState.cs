namespace KINESIS.Server;

public class ServerState
{
    public readonly int ServerId;
    public readonly string Address;
    public readonly short Port;
    public readonly string Location;
    public readonly string Name;
    public readonly int SlaveId;
    public readonly int MatchId;
    public readonly ServerStatus Status;
    public readonly string MapName;
    public readonly string GameName;
    public readonly string GameMode;
    public readonly int MatchDuration;
    public readonly int GamePhase;
    public readonly string[] TeamAndPlayerInfo;

    public ServerState(int serverId, string address, short port, string location, string name, int slaveId, int matchId, ServerStatus status, string mapName, string gameName, string gameMode, int matchDuration, int gamePhase, string[] teamAndPlayerInfo)
    {
        ServerId = serverId;
        Address = address;
        Port = port;
        Location = location;
        Name = name;
        SlaveId = slaveId;
        MatchId = matchId;
        Status = status;
        MapName = mapName;
        GameName = gameName;
        GameMode = gameMode;
        MatchDuration = matchDuration;
        GamePhase = gamePhase;
        TeamAndPlayerInfo = teamAndPlayerInfo;
    }
}
