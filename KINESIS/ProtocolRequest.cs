namespace KINESIS;

public abstract class ProtocolRequest
{
    // public abstract void HandleRequest(IDbContextFactory<BountyContext> dbContextFactory, T subject);

    public static byte ReadByte(byte[] data, int offset, out int updatedOffset)
    {
        byte result = data[offset];
        updatedOffset = offset + 1;
        return result;
    }

    public static short ReadShort(byte[] data, int offset, out int updatedOffset)
    {
        short result = BitConverter.ToInt16(data, offset);
        updatedOffset = offset + 2;
        return result;
    }

    public static int ReadInt(byte[] data, int offset, out int updatedOffset)
    {
        int result = BitConverter.ToInt32(data, offset);
        updatedOffset = offset + 4;
        return result;
    }
    public static long ReadLong(byte[] data, int offset, out int updatedOffset)
    {
        long result = BitConverter.ToInt64(data, offset);
        updatedOffset = offset + 8;
        return result;
    }
    public static float ReadFloat(byte[] data, int offset, out int updatedOffset)
    {
        return BitConverter.Int32BitsToSingle(ReadInt(data, offset, out updatedOffset));
    }

    public static string ReadString(byte[] data, int offset, out int updatedOffset)
    {
        int end = offset;
        while (true)
        {
            byte b = data[end];
            if (b == 0)
            {
                break;
            }
            ++end;
        }
        string result = System.Text.Encoding.UTF8.GetString(data, offset, end - offset);

        updatedOffset = end + 1; // skip \0
        return result;
    }
}
