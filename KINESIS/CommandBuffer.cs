namespace KINESIS;

public class CommandBuffer
{
    // Based on collected data, ~99% of all messages are less than 112 bytes. Array size of 112 also has a nice
    // property that it allocates exactly 128 bytes (since it includes byte[] object header).
    private static readonly int INITIAL_CAPACITY = 112;

    // The rest of the messages are very long (e.g. ChatChannel connects), so when we grow, grow by a lot.
    private static readonly int GROW_SIZE = 896; // will allocate 1024 bytes after first resize.

    public byte[] Buffer = new byte[INITIAL_CAPACITY];
    public int Size;
    private Span<byte> Span => ((Span<byte>)Buffer)[Size..];

    public void WriteInt8(byte value)
    {
        if (Buffer.Length == Size)
        {
            Array.Resize(ref Buffer, Buffer.Length + GROW_SIZE);
        }
        Buffer[Size++] = value;
    }

    public void WriteInt16(short value)
    {
        if (!BitConverter.TryWriteBytes(Span, value))
        {
            Array.Resize(ref Buffer, Buffer.Length + GROW_SIZE);
            BitConverter.TryWriteBytes(Span, value);
        }
        Size += 2;
    }

    public void WriteInt32(int value)
    {
        if (!BitConverter.TryWriteBytes(Span, value))
        {
            Array.Resize(ref Buffer, Buffer.Length + GROW_SIZE);
            BitConverter.TryWriteBytes(Span, value);
        }
        Size += 4;
    }

    public void WriteInt64(long value)
    {
        if (!BitConverter.TryWriteBytes(Span, value))
        {
            Array.Resize(ref Buffer, Buffer.Length + GROW_SIZE);
            BitConverter.TryWriteBytes(Span, value);
        }
        Size += 8;
    }

    public void WriteFloat32(float value)
    {
        if (!BitConverter.TryWriteBytes(Span, value))
        {
            Array.Resize(ref Buffer, Buffer.Length + GROW_SIZE);
            BitConverter.TryWriteBytes(Span, value);
        }
        Size += 4;
    }

    public void WriteString(string value)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(value);
        int finalWriteOffset = Size + data.Length + 1;

        if (Buffer.Length < finalWriteOffset)
        {
            Array.Resize(ref Buffer, Math.Max(Buffer.Length + GROW_SIZE, finalWriteOffset));
        }

        Array.Copy(data, 0, Buffer, Size, data.Length);
        Buffer[finalWriteOffset - 1] = 0;
        Size = finalWriteOffset;
    }
}
