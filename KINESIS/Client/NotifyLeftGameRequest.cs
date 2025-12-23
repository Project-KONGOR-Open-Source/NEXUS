namespace KINESIS.Client;

public class NotifyLeftGameRequest : ProtocolRequest
{
    public static NotifyLeftGameRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        updatedOffset = offset;
        return new NotifyLeftGameRequest();
    }

}


