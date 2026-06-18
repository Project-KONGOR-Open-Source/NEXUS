namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

/// <summary>
///     A dynamic, expandable byte buffer that reads and writes the chat protocol's primitive types over a single backing array.
///     Writes append to the end of the buffer, while reads advance an internal offset from the front, so one instance is used either to compose an outbound packet or to consume an inbound one.
/// </summary>
public class ChatBuffer
{
    /// <summary>
    ///     The backing byte array. Only the first <see cref="Size"/> bytes are meaningful. The remainder is spare capacity.
    /// </summary>
    public byte[] Data { get; private set; }

    /// <summary>
    ///     The number of bytes written to the buffer.
    /// </summary>
    public long Size { get; private set; }

    /// <summary>
    ///     The number of bytes already read from the buffer. This is the offset into <see cref="Data"/> where the next read will occur.
    /// </summary>
    public long Offset { get; private set; }

    /// <summary>
    ///     Initialises a new expandable buffer with zero capacity.
    /// </summary>
    public ChatBuffer()
    {
        Data = [];
        Size = 0;
        Offset = 0;
    }

    /// <summary>
    ///     Initialises a new expandable buffer with the given capacity.
    /// </summary>
    public ChatBuffer(long capacity)
    {
        Data = new byte[capacity];
        Size = 0;
        Offset = 0;
    }

    /// <summary>
    ///     Initialises a new buffer over the given data, ready to be read from the front.
    /// </summary>
    public ChatBuffer(byte[] data)
    {
        Data = data;
        Size = data.Length;
        Offset = 0;
    }

    /// <summary>
    ///     Resizes the buffer to the given size, growing the backing array if required.
    /// </summary>
    public void Resize(long size)
    {
        Reserve(size);

        Size = size;

        if (Offset > Size)
            Offset = Size;
    }

    /// <summary>
    ///     Appends the given 2-byte command identifier to the buffer, and returns the number of bytes appended.
    /// </summary>
    public long WriteCommand(ushort command)
        => WriteCommandBytes(BitConverter.GetBytes(command));

    /// <summary>
    ///     Reads the 2-byte command identifier from the front of the buffer, and returns it as a byte array.
    /// </summary>
    public byte[] ReadCommandBytes()
    {
        if (Offset is not 0)
            throw new InvalidDataException($"Offset Is {Offset}, But 0 (Zero) Was Expected");

        byte[] data = Data[.. 2];

        Shift(2);

        return data;
    }

    /// <summary>
    ///     Appends a single byte to the buffer, and returns the number of bytes appended.
    /// </summary>
    public long WriteInt8(byte value)
        => Append(value);

    /// <summary>
    ///     Reads a single byte from the buffer, and returns it.
    /// </summary>
    public byte ReadInt8()
    {
        if (Size - Offset < 1)
            throw new InvalidDataException($"Unable To Read 1 Byte From Buffer With Size {Size} And Offset {Offset}");

        byte data = Data[Offset];

        Shift(1);

        return data;
    }

    /// <summary>
    ///     Appends a single byte with a value of either 0 or 1 to the buffer, and returns the number of bytes appended.
    /// </summary>
    public long WriteBool(bool value)
        => WriteInt8(BitConverter.GetBytes(value).Single());

    /// <summary>
    ///     Reads a single byte from the buffer, and returns it as a boolean value if it can be parsed to one.
    /// </summary>
    public bool ReadBool()
    {
        byte data = ReadInt8();

        if (data is not 0 and not 1)
            throw new InvalidDataException($"Unable To Read A Boolean Value From Buffer Byte Value {data}");

        return data is 1;
    }

    /// <summary>
    ///     Appends the given 16-bit integer to the buffer, and returns the number of bytes appended.
    /// </summary>
    public long WriteInt16(short value)
        => Append(BitConverter.GetBytes(value));

    /// <summary>
    ///     Reads a 16-bit integer from the buffer, and returns it.
    /// </summary>
    public short ReadInt16()
    {
        if (Size - Offset < 2)
            throw new InvalidDataException($"Unable To Read 2 Bytes From Buffer With Size {Size} And Offset {Offset}");

        short data = BitConverter.ToInt16(Data, (int) Offset);

        Shift(2);

        return data;
    }

    /// <summary>
    ///     Appends the given 32-bit integer to the buffer, and returns the number of bytes appended.
    /// </summary>
    public long WriteInt32(int value)
        => Append(BitConverter.GetBytes(value));

    /// <summary>
    ///     Reads a 32-bit integer from the buffer, and returns it.
    /// </summary>
    public int ReadInt32()
    {
        if (Size - Offset < 4)
            throw new InvalidDataException($"Unable To Read 4 Bytes From Buffer With Size {Size} And Offset {Offset}");

        int data = BitConverter.ToInt32(Data, (int) Offset);

        Shift(4);

        return data;
    }

    /// <summary>
    ///     Appends the given 64-bit integer to the buffer, and returns the number of bytes appended.
    /// </summary>
    public long WriteInt64(long value)
        => Append(BitConverter.GetBytes(value));

    /// <summary>
    ///     Reads a 64-bit integer from the buffer, and returns it.
    /// </summary>
    public long ReadInt64()
    {
        if (Size - Offset < 8)
            throw new InvalidDataException($"Unable To Read 8 Bytes From Buffer With Size {Size} And Offset {Offset}");

        long data = BitConverter.ToInt64(Data, (int) Offset);

        Shift(8);

        return data;
    }

    /// <summary>
    ///     Appends the given single-precision floating-point value to the buffer, and returns the number of bytes appended.
    /// </summary>
    public long WriteFloat32(float value)
        => Append(BitConverter.GetBytes(value));

    /// <summary>
    ///     Reads a single-precision floating-point value from the buffer, and returns it.
    /// </summary>
    public float ReadFloat32()
    {
        if (Size - Offset < 4)
            throw new InvalidDataException($"Unable To Read 4 Bytes From Buffer With Size {Size} And Offset {Offset}");

        float data = BitConverter.ToSingle(Data, (int) Offset);

        Shift(4);

        return data;
    }

    /// <summary>
    ///     Appends the given string to the buffer as a <see langword="null"/>-terminated UTF-8 string, and returns the number of bytes appended.
    ///     The trailing "\0" is the <see langword="null"/> terminator that marks the end of a C-style string; UTF-8 is compatible with <see langword="null"/>-terminated strings because no encoded character contains a zero byte.
    /// </summary>
    public long WriteString(string value)
        => Append(Encoding.UTF8.GetBytes(value).Append<byte>(0).ToArray());

    /// <summary>
    ///     Reads a <see langword="null"/>-terminated UTF-8 string from the buffer, and returns it.
    ///     The trailing "\0" is the <see langword="null"/> terminator that marks the end of a C-style string; UTF-8 is compatible with <see langword="null"/>-terminated strings because no encoded character contains a zero byte.
    /// </summary>
    public string ReadString()
    {
        long marker = Offset;

        while (marker <= Size && Data[marker] is not 0)
            marker++;

        if (marker > Size)
            throw new InvalidDataException($"Unable To Read A String Value From Buffer With Size {Size} And Offset {Offset}");

        string data = Encoding.UTF8.GetString(Data, (int) Offset, (int) (marker - Offset));

        marker++; // Move The Marker Past The NULL Terminator

        Offset = marker;

        return data;
    }

    /// <summary>
    ///     Validates that the value is 2 bytes, appends it to the buffer, and returns the number of bytes appended.
    /// </summary>
    private long WriteCommandBytes(byte[] value)
    {
        if (value.Length is not 2)
            throw new InvalidDataException($"Chat Command Is Expected To Be 2 Bytes In Length, But It Is {value.Length} Bytes");

        return Append(value);
    }

    /// <summary>
    ///     Grows the backing array if the requested capacity exceeds it, preserving the bytes already written.
    /// </summary>
    private void Reserve(long capacity)
    {
        if (capacity < 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Buffer Capacity Cannot Be Negative");

        if (capacity > Data.Length)
        {
            byte[] data = new byte[capacity];

            Array.Copy(Data, 0, data, 0, Size);

            Data = data;
        }
    }

    /// <summary>
    ///     Advances the read offset by the given number of bytes.
    /// </summary>
    private void Shift(long offset)
        => Offset += offset;

    /// <summary>
    ///     Appends a single byte to the buffer, and returns the number of bytes appended.
    /// </summary>
    private long Append(byte value)
    {
        Reserve(Size + 1);

        Data[Size] = value;
        Size += 1;

        return 1;
    }

    /// <summary>
    ///     Appends the given byte array to the buffer, and returns the number of bytes appended.
    /// </summary>
    private long Append(byte[] buffer)
    {
        Reserve(Size + buffer.Length);

        Array.Copy(buffer, 0, Data, Size, buffer.Length);
        Size += buffer.Length;

        return buffer.Length;
    }
}
