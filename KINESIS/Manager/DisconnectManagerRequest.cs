namespace KINESIS.Manager;

public class DisconnectManagerRequest : ProtocolRequest
{
    public static DisconnectManagerRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        updatedOffset = offset;
        return new();
    }

}

