namespace KONGOR.MasterServer.Helpers.MessageOfTheDay;

/// <summary>
///     Reads a picture's media type and size from its header, for the PNG, JPEG, and WebP formats the message of the day accepts.
///     Only the header is read, so no imaging library is needed, and nothing is decoded.
/// </summary>
public static class PictureFormat
{
    private static ReadOnlySpan<byte> PNGSignature => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    /// <summary>
    ///     Returns the picture's media type and size, or <see langword="null"/> when the bytes are not a PNG, JPEG, or WebP picture or end before the header does.
    /// </summary>
    public static PictureInformation? Read(ReadOnlySpan<byte> bytes)
    {
        if (bytes.StartsWith(PNGSignature))
            return ReadPNG(bytes);

        if (bytes.Length >= 2 && bytes[0] is 0xFF && bytes[1] is 0xD8)
            return ReadJPEG(bytes);

        if (bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8) && bytes[8..12].SequenceEqual("WEBP"u8))
            return ReadWebP(bytes);

        return null;
    }

    // The First Chunk Is "IHDR", Whose Data Starts With The Width And Height As Big-Endian 32-Bit Numbers
    private static PictureInformation? ReadPNG(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 24 || bytes[12..16].SequenceEqual("IHDR"u8) is false)
            return null;

        return new PictureInformation("image/png", (int) BinaryPrimitives.ReadUInt32BigEndian(bytes[16..]), (int) BinaryPrimitives.ReadUInt32BigEndian(bytes[20..]));
    }

    // Segments Follow One Another Until A Start Of Frame (Baseline, Extended, Or Progressive), Whose Height And Width Follow Its Precision
    private static PictureInformation? ReadJPEG(ReadOnlySpan<byte> bytes)
    {
        int position = 2;

        while (position + 4 <= bytes.Length)
        {
            if (bytes[position] is not 0xFF)
                return null;

            byte marker = bytes[position + 1];

            // A Marker May Be Padded With Extra 0xFF Bytes
            if (marker is 0xFF)
            {
                position++;

                continue;
            }

            // The Restart Markers And The Temporary Marker Stand Alone, Without A Length
            if (marker is >= 0xD0 and <= 0xD7 or 0x01)
            {
                position += 2;

                continue;
            }

            // The Image Ended, Or Its Scan Began, Before Any Frame Said How Large It Is
            if (marker is 0xD9 or 0xDA)
                return null;

            if (marker is 0xC0 or 0xC1 or 0xC2)
            {
                if (position + 9 > bytes.Length)
                    return null;

                return new PictureInformation("image/jpeg", BinaryPrimitives.ReadUInt16BigEndian(bytes[(position + 7)..]), BinaryPrimitives.ReadUInt16BigEndian(bytes[(position + 5)..]));
            }

            position += 2 + BinaryPrimitives.ReadUInt16BigEndian(bytes[(position + 2)..]);
        }

        return null;
    }

    // The First Chunk After "WEBP" Says Which Variant The Picture Is, And Each Variant Stores Its Size Differently
    private static PictureInformation? ReadWebP(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 20)
            return null;

        ReadOnlySpan<byte> chunkType = bytes[12..16];
        ReadOnlySpan<byte> chunkData = bytes[20..];

        // Lossy: A Three-Byte Frame Tag, The Start Code, Then The Width And Height In Fourteen Bits Each
        if (chunkType.SequenceEqual("VP8 "u8))
        {
            if (chunkData.Length < 10 || chunkData[3] is not 0x9D || chunkData[4] is not 0x01 || chunkData[5] is not 0x2A)
                return null;

            return new PictureInformation("image/webp", BinaryPrimitives.ReadUInt16LittleEndian(chunkData[6..]) & 0x3FFF, BinaryPrimitives.ReadUInt16LittleEndian(chunkData[8..]) & 0x3FFF);
        }

        // Lossless: A Signature, Then The Width And Height, Each Minus One, Packed Into Fourteen Bits Each
        if (chunkType.SequenceEqual("VP8L"u8))
        {
            if (chunkData.Length < 5 || chunkData[0] is not 0x2F)
                return null;

            uint packedSize = BinaryPrimitives.ReadUInt32LittleEndian(chunkData[1..]);

            return new PictureInformation("image/webp", (int) (packedSize & 0x3FFF) + 1, (int) ((packedSize >> 14) & 0x3FFF) + 1);
        }

        // Extended: Flags And Three Reserved Bytes, Then The Canvas Width And Height, Each Minus One, In Three Bytes Each
        if (chunkType.SequenceEqual("VP8X"u8))
        {
            if (chunkData.Length < 10)
                return null;

            return new PictureInformation("image/webp", ReadUInt24LittleEndian(chunkData[4..]) + 1, ReadUInt24LittleEndian(chunkData[7..]) + 1);
        }

        return null;
    }

    private static int ReadUInt24LittleEndian(ReadOnlySpan<byte> bytes)
        => bytes[0] | (bytes[1] << 8) | (bytes[2] << 16);
}
