namespace TRANSMUTANSTEIN.ChatServer.Core;

public class ChatBuffer : TCPBuffer
{
    /// <summary>
    ///     Initialize a new expandable chat buffer with zero capacity.
    /// </summary>
    public ChatBuffer() { }

    /// <summary>
    ///     Initialize a new expandable chat buffer with the given capacity.
    /// </summary>
    public ChatBuffer(long capacity) : base(capacity) { }

    /// <summary>
    ///     Initialize a new expandable chat buffer with the given data.
    /// </summary>
    public ChatBuffer(byte[] data) : base(data) { }

    /// <summary>
    ///     Append 2 bytes to the buffer, and return the number of bytes appended.
    /// </summary>
    public long WriteCommandBytes(byte[] value)
    {
        if (value.Length is not 2)
            throw new InvalidDataException($"Chat Command Is Expected To Be 2 Bytes In Length, But It Is {value.Length} Bytes");

        return Append(value);
    }

    /// <summary>
    ///     Append 2 bytes to the buffer, and return the number of bytes appended.
    /// </summary>
    public long WriteCommand(ushort command)
        => WriteCommandBytes(BitConverter.GetBytes(command));

    /// <summary>
    ///     Read 2 bytes from the buffer, and return the result as a byte array.
    /// </summary>
    public byte[] ReadCommandBytes()
    {
        if (_offset is not 0)
            throw new InvalidDataException($"Offset Is {_offset}, But 0 (Zero) Was Expected");

        byte[] data = _data[..2];

        Shift(2);

        return data;
    }

    /// <summary>
    ///     Append 1 byte to the buffer, and return the number of bytes appended.
    /// </summary>
    public long WriteInt8(byte value)
        => Append(value);

    /// <summary>
    ///     Read 1 byte from the buffer, and return the result.
    /// </summary>
    public byte ReadInt8()
    {
        if (_size - _offset < 1)
            throw new InvalidDataException($"Unable To Read 1 Byte From Buffer With Size {_size} And Offset {_offset}");

        byte data = _data[_offset];

        Shift(1);

        return data;
    }

    /// <summary>
    ///     Append 2 bytes to the buffer, and return the number of bytes appended.
    /// </summary>
    public long WriteInt16(short value)
        => Append(BitConverter.GetBytes(value));

    /// <summary>
    ///     Read 2 bytes from the buffer, and return the result as a short value.
    /// </summary>
    public short ReadInt16()
    {
        if (_size - _offset < 2)
            throw new InvalidDataException($"Unable To Read 2 Bytes From Buffer With Size {_size} And Offset {_offset}");

        short data = BitConverter.ToInt16(_data, (int)_offset);

        Shift(2);

        return data;
    }

    /// <summary>
    ///     Append 4 bytes to the buffer, and return the number of bytes appended.
    /// </summary>
    public long WriteInt32(int value)
        => Append(BitConverter.GetBytes(value));

    /// <summary>
    ///     Read 4 bytes from the buffer, and return the result as an int value.
    /// </summary>
    public int ReadInt32()
    {
        if (_size - _offset < 4)
            throw new InvalidDataException($"Unable To Read 4 Bytes From Buffer With Size {_size} And Offset {_offset}");

        int data = BitConverter.ToInt32(_data, (int)_offset);

        Shift(4);

        return data;
    }

    /// <summary>
    ///     Append 8 bytes to the buffer, and return the number of bytes appended.
    /// </summary>
    public long WriteInt64(long value)
        => Append(BitConverter.GetBytes(value));

    /// <summary>
    ///     Read 8 bytes from the buffer, and return the result as a long value.
    /// </summary>
    public long ReadInt64()
    {
        if (_size - _offset < 8)
            throw new InvalidDataException($"Unable To Read 8 Bytes From Buffer With Size {_size} And Offset {_offset}");

        long data = BitConverter.ToInt64(_data, (int)_offset);

        Shift(8);

        return data;
    }

    /// <summary>
    ///     Append 4 bytes to the buffer, and return the number of bytes appended.
    /// </summary>
    public long WriteFloat32(float value)
        => Append(BitConverter.GetBytes(value));

    /// <summary>
    ///     Read 4 bytes from the buffer, and return the result as a float value.
    /// </summary>
    public float ReadFloat32()
    {
        if (_size - _offset < 4)
            throw new InvalidDataException($"Unable To Read 4 Bytes From Buffer With Size {_size} And Offset {_offset}");

        float data = BitConverter.ToSingle(_data, (int)_offset);

        Shift(4);

        return data;
    }

    /// <summary>
    ///     Append an arbitrary number of bytes to the buffer, and return the number of bytes appended.
    ///     For C-style strings, "\0" is the NULL character (also known as the NULL Terminator), which has the value 0 in the ASCII table and is used to determine the end of C-style strings.
    ///     UTF-8 is also compatible with NULL-terminated strings, meaning that no character will have a zero byte in it after being encoded.
    /// </summary>
    public long WriteString(string value)
        => Append(Encoding.UTF8.GetBytes(value).Append<byte>(0).ToArray());

    /// <summary>
    ///     Reads an arbitrary number of bytes from the buffer, and return the result as a string value.
    ///     For C-style strings, "\0" is the NULL character (also known as the NULL Terminator), which has the value 0 in the ASCII table and is used to determine the end of C-style strings.
    ///     UTF-8 is also compatible with NULL-terminated strings, meaning that no character will have a zero byte in it after being encoded.
    /// </summary>
    public string ReadString()
    {
        long marker = _offset;

        while (marker <= _size && _data[marker] is not 0)
            marker++;

        if (marker > _size)
            throw new InvalidDataException($"Unable To Read A String Value From Buffer With Size {_size} And Offset {_offset}");

        string data = Encoding.UTF8.GetString(_data, (int)_offset, (int)(marker - _offset));

        marker++; // Move Marker To NULL Terminator Position

        _offset = marker;

        return data;
    }

    /// <summary>
    ///     Prepend 2 bytes to the buffer, and return the number of bytes prepended.
    /// </summary>
    public long PrependBufferSize()
    {
        const long bytes = 2;

        Reserve(_size + bytes);

        _data = BitConverter.GetBytes(Convert.ToInt16(_size)).Concat(_data).ToArray();
        _size = _size + bytes;

        return bytes;
    }
}
