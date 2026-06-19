namespace TRANSMUTANSTEIN.ChatServer.Extensions.Collections;

public static class ByteArrayExtensions
{
    /// <summary>
    ///     Renders the byte array as an uppercase space-separated hexadecimal string (for example, "0A 1B 2C").
    /// </summary>
    public static string ToHexString(this byte[] bytes)
        => BitConverter.ToString(bytes).Replace('-', ' ');
}
