namespace KONGOR.MasterServer.Models.RequestResponse.Messaging;

/// <summary>
///     A single entry in the message-list manifest, as reported to the game client.
///     The manifest deliberately omits the message body and metadata, which the client fetches per-message when a message is opened.
/// </summary>
public class MessageManifestEntry(string subject, string image, bool read, int expiration, int sent, string crc, bool deletable)
{
    /// <summary>
    ///     The message subject, shown as the title in the in-game inbox. The client passes this through its translation table, falling back to the literal text when no translation key matches.
    /// </summary>
    [PHPProperty("subject")]
    public string Subject { get; } = subject;

    /// <summary>
    ///     The texture path for the message icon shown in the inbox.
    /// </summary>
    [PHPProperty("image")]
    public string Image { get; } = image;

    /// <summary>
    ///     Whether the account has already read the message.
    /// </summary>
    [PHPProperty("read")]
    public bool Read { get; } = read;

    /// <summary>
    ///     The UNIX timestamp (in seconds) at which the message expires, or 0 when the message never expires.
    /// </summary>
    [PHPProperty("expiration")]
    public int Expiration { get; } = expiration;

    /// <summary>
    ///     The UNIX timestamp (in seconds) at which the message was sent.
    /// </summary>
    [PHPProperty("sent")]
    public int Sent { get; } = sent;

    /// <summary>
    ///     The message identifier, used by the client to fetch the message body and to delete the message.
    /// </summary>
    [PHPProperty("crc")]
    public string CRC { get; } = crc;

    /// <summary>
    ///     Whether the account is permitted to delete the message.
    /// </summary>
    [PHPProperty("deletable")]
    public bool Deletable { get; } = deletable;
}
