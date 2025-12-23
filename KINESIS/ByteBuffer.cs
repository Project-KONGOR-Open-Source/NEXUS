namespace KINESIS;

internal class ByteBuffer
{
    public byte[] Buffer { get; private set; } = new byte[1024];
    public int ReadOffset { get; set; } = 0;
    public int WriteOffset { get; set; } = 0;

    public void EnsureCapacity(int capacity)
    {
        int bufferLength = Buffer.Length;
        int availableSpace = bufferLength - WriteOffset;
        if (availableSpace < capacity)
        {
            int unreadDataSize = WriteOffset - ReadOffset;
            // Not enough space in buffer. Shift data to the beginning, or resize the buffer if not possible.
            if (ReadOffset == 0)
            {
                // Cannot shift data, allocate new buffer. Align to the next 1024 bytes boundary.
                int newLength = (unreadDataSize + capacity + 1023) & ~1023;
                byte[] newBuffer = new byte[newLength];
                Array.Copy(Buffer, ReadOffset, newBuffer, 0, unreadDataSize);
                Buffer = newBuffer;
            }
            else
            {
                // Shift all unread data to the beginning of an array, give us more space to read.
                Array.Copy(Buffer, ReadOffset, Buffer, 0, unreadDataSize);
            }
            ReadOffset = 0;
            WriteOffset = unreadDataSize;
        }
    }
}
