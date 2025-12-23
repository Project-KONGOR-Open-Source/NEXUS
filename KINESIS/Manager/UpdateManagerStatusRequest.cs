namespace KINESIS.Manager;

public class UpdateManagerStatusRequest : ProtocolRequest
{
    private readonly ManagerState _managerState;
    public ManagerState ManagerState => _managerState;

    public UpdateManagerStatusRequest(ManagerState managerState)
    {
        _managerState = managerState;
    }

    public static UpdateManagerStatusRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        int managerId = ReadInt(data, offset, out offset);
        string login = ReadString(data, offset, out offset);
        string location = ReadString(data, offset, out offset);
        string name = ReadString(data, offset, out offset);
        string version = ReadString(data, offset, out offset);
        string address = ReadString(data, offset, out offset);
        short port = ReadShort(data, offset, out offset);
        bool shuttingDown = ReadByte(data, offset, out offset) != 0;

        updatedOffset = offset;

        return new UpdateManagerStatusRequest(new ManagerState(managerId, login, location, name, version, address, port, shuttingDown));
    }

}

