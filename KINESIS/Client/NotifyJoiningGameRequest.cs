namespace KINESIS.Client;

public class NotifyJoiningGameRequest : ProtocolRequest
{
    private readonly string _serverAddress;
    public string ServerAddress => _serverAddress;

    public NotifyJoiningGameRequest(string serverAddress)
    {
        _serverAddress = serverAddress;
    }

    public static NotifyJoiningGameRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        string serverAddress = ReadString(data, offset, out offset);
        updatedOffset = offset;

        return new NotifyJoiningGameRequest(serverAddress);
    }

}


