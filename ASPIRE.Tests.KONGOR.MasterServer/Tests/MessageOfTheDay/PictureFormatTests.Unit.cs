namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.MessageOfTheDay;

/// <summary>
///     Pure-logic tests for reading a picture's type and size from its header, which is how the message of the day refuses pictures of the wrong size or format.
///     Each test builds the smallest header the format defines, so the tests need no picture files.
/// </summary>
public sealed class PictureFormat_Unit
{
    [Test]
    public async Task A_PNG_Header_Gives_Its_Size_And_Type()
    {
        PictureInformation? information = PictureFormat.Read(PNGHeader(800, 900));

        await Assert.That(information).IsEqualTo(new PictureInformation("image/png", 800, 900));
    }

    [Test]
    public async Task A_Baseline_And_A_Progressive_JPEG_Give_Their_Size()
    {
        using (Assert.Multiple())
        {
            await Assert.That(PictureFormat.Read(JPEGHeader(0xC0, 800, 900))).IsEqualTo(new PictureInformation("image/jpeg", 800, 900));
            await Assert.That(PictureFormat.Read(JPEGHeader(0xC2, 640, 480))).IsEqualTo(new PictureInformation("image/jpeg", 640, 480));
        }
    }

    [Test]
    public async Task Each_WebP_Variant_Gives_Its_Size()
    {
        using (Assert.Multiple())
        {
            await Assert.That(PictureFormat.Read(WebPLossyHeader(800, 900))).IsEqualTo(new PictureInformation("image/webp", 800, 900));
            await Assert.That(PictureFormat.Read(WebPLosslessHeader(800, 900))).IsEqualTo(new PictureInformation("image/webp", 800, 900));
            await Assert.That(PictureFormat.Read(WebPExtendedHeader(800, 900))).IsEqualTo(new PictureInformation("image/webp", 800, 900));
        }
    }

    [Test]
    public async Task A_GIF_Is_Not_Read()
    {
        byte[] gif = [.. "GIF89a"u8, 0x20, 0x03, 0x84, 0x03, 0x00, 0x00, 0x00];

        await Assert.That(PictureFormat.Read(gif)).IsNull();
    }

    [Test]
    public async Task A_Truncated_Header_Is_Not_Read()
    {
        using (Assert.Multiple())
        {
            await Assert.That(PictureFormat.Read(PNGHeader(800, 900).AsSpan(0, 20))).IsNull();
            await Assert.That(PictureFormat.Read(JPEGHeader(0xC0, 800, 900).AsSpan(0, 8))).IsNull();
            await Assert.That(PictureFormat.Read(WebPLossyHeader(800, 900).AsSpan(0, 24))).IsNull();
            await Assert.That(PictureFormat.Read([])).IsNull();
        }
    }

    // The Signature, Then The IHDR Chunk: Its Length, Its Type, The Width And Height (Big-Endian), And Five More Bytes
    private static byte[] PNGHeader(int width, int height)
        => [0x89, .. "PNG"u8, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, .. "IHDR"u8, .. BigEndian(width, 4), .. BigEndian(height, 4), 0x08, 0x06, 0x00, 0x00, 0x00];

    // Start Of Image, An APP0 Segment To Skip, Then A Start Of Frame Segment Whose Height And Width Follow Its Precision
    private static byte[] JPEGHeader(byte startOfFrame, int width, int height)
        => [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, .. "JFIF"u8, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00,
            0xFF, startOfFrame, 0x00, 0x11, 0x08, .. BigEndian(height, 2), .. BigEndian(width, 2), 0x03, 0x01, 0x22, 0x00, 0x02, 0x11, 0x01, 0x03, 0x11, 0x01];

    // A "VP8 " Chunk: Three Bytes Of Frame Tag, The Start Code, Then The Width And Height In Fourteen Bits Each (Little-Endian)
    private static byte[] WebPLossyHeader(int width, int height)
        => RIFF([.. "VP8 "u8, .. LittleEndian(10, 4), 0x9D, 0x01, 0x2A, 0x9D, 0x01, 0x2A, .. LittleEndian(width, 2), .. LittleEndian(height, 2)]);

    // A "VP8L" Chunk: Its Signature, Then The Width And Height, Each Minus One, Packed Into Fourteen Bits Each (Little-Endian)
    private static byte[] WebPLosslessHeader(int width, int height)
        => RIFF([.. "VP8L"u8, .. LittleEndian(5, 4), 0x2F, .. LittleEndian((width - 1) | ((height - 1) << 14), 4)]);

    // A "VP8X" Chunk: Flags, Three Reserved Bytes, Then The Canvas Width And Height, Each Minus One, In Three Bytes Each (Little-Endian)
    private static byte[] WebPExtendedHeader(int width, int height)
        => RIFF([.. "VP8X"u8, .. LittleEndian(10, 4), 0x00, 0x00, 0x00, 0x00, .. LittleEndian(width - 1, 3), .. LittleEndian(height - 1, 3)]);

    private static byte[] RIFF(byte[] chunk)
        => [.. "RIFF"u8, .. LittleEndian(chunk.Length + 4, 4), .. "WEBP"u8, .. chunk];

    private static byte[] BigEndian(int value, int length)
        => [.. Enumerable.Range(0, length).Select(index => (byte) (value >> (8 * (length - 1 - index))))];

    private static byte[] LittleEndian(int value, int length)
        => [.. Enumerable.Range(0, length).Select(index => (byte) (value >> (8 * index)))];
}
