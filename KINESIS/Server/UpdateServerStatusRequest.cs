namespace KINESIS.Server;

public class UpdateServerStatusRequest : ProtocolRequest
{
    private readonly ServerState _serverState;
    public ServerState ServerState => _serverState;

    public UpdateServerStatusRequest(ServerState serverState)
    {
        _serverState = serverState;
    }

    public static UpdateServerStatusRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        int serverId = ReadInt(data, offset, out offset);
        string address = ReadString(data, offset, out offset);
        short port = ReadShort(data, offset, out offset);
        string location = ReadString(data, offset, out offset);
        string name = ReadString(data, offset, out offset);
        int slaveId = ReadInt(data, offset, out offset);
        int matchId = ReadInt(data, offset, out offset);
        ServerStatus status = (ServerStatus)ReadByte(data, offset, out offset);
        byte a2 = ReadByte(data, offset, out offset);
        short a3 = ReadShort(data, offset, out offset);
        string mapName = ReadString(data, offset, out offset);
        string gameName = ReadString(data, offset, out offset);
        string gameMode = ReadString(data, offset, out offset);
        byte a7 = ReadByte(data, offset, out offset);
        short a8 = ReadShort(data, offset, out offset);
        short a9 = ReadShort(data, offset, out offset);
        int matchDuration = ReadInt(data, offset, out offset);
        int gamePhase = ReadInt(data, offset, out offset);
        string[] teamAndPlayerInfo = new string[12];
        for (int i = 0; i < 12; ++i)
        {
            teamAndPlayerInfo[i] = ReadString(data, offset, out offset);
        }

        int a24 = ReadInt(data, offset, out offset);
        int a25 = ReadInt(data, offset, out offset);
        long a26 = ReadLong(data, offset, out offset);
        long a27 = ReadLong(data, offset, out offset);
        long a28 = ReadLong(data, offset, out offset);
        long a29 = ReadLong(data, offset, out offset);
        string? a30 = ReadString(data, offset, out offset);

        int numClients = ReadInt(data, offset, out offset);
        float a32 = ReadFloat(data, offset, out offset);
        string? a33 = ReadString(data, offset, out offset);

        for (int i = 0; i < numClients; ++i)
        {
            int b1 = ReadInt(data, offset, out offset);
            string? b2 = ReadString(data, offset, out offset);
            short b3 = ReadShort(data, offset, out offset);
            short b4 = ReadShort(data, offset, out offset);
            short b5 = ReadShort(data, offset, out offset);
            long b6 = ReadLong(data, offset, out offset);
            long b7 = ReadLong(data, offset, out offset);
            long b8 = ReadLong(data, offset, out offset);
            long b9 = ReadLong(data, offset, out offset);
            long b10 = ReadLong(data, offset, out offset);
            long b11 = ReadLong(data, offset, out offset);
            long b12 = ReadLong(data, offset, out offset);
            long b13 = ReadLong(data, offset, out offset);
        }

        updatedOffset = offset;

        return new UpdateServerStatusRequest(
            new ServerState(
                serverId: serverId,
                address: address,
                port: port,
                location: location,
                name: name,
                slaveId: slaveId,
                matchId: matchId,
                status: status,
                mapName: mapName,
                gameName: gameName,
                gameMode: gameMode,
                matchDuration: matchDuration,
                gamePhase: gamePhase,
                teamAndPlayerInfo: teamAndPlayerInfo));
    }

}

