namespace KINESIS.Server;

public class DisconnectServerRequest : ProtocolRequest
{
    public static DisconnectServerRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        updatedOffset = offset;
        return new DisconnectServerRequest();
    }

}

