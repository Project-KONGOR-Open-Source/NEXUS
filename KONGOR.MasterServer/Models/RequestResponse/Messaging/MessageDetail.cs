namespace KONGOR.MasterServer.Models.RequestResponse.Messaging;

/// <summary>
///     A single message in full, as returned to the game client when a message is opened.
///     This carries the message body and metadata that the manifest (<see cref="MessageManifestEntry"/>) omits.
/// </summary>
public class MessageDetail(string subject, string body, string image, bool read, int expiration, int sent, string crc, bool deletable, Dictionary<string, string> metadata)
{
    [PHPProperty("subject")]
    public string Subject { get; } = subject;

    [PHPProperty("body")]
    public string Body { get; } = body;

    [PHPProperty("image")]
    public string Image { get; } = image;

    [PHPProperty("read")]
    public bool Read { get; } = read;

    [PHPProperty("expiration")]
    public int Expiration { get; } = expiration;

    [PHPProperty("sent")]
    public int Sent { get; } = sent;

    [PHPProperty("crc")]
    public string CRC { get; } = crc;

    [PHPProperty("deletable")]
    public bool Deletable { get; } = deletable;

    /// <summary>
    ///     Structured metadata keyed by field name (for example "bodyTitle", "subtitle", "footer", "body").
    ///     The in-game message panel renders its body text from the "body" entry, passing it through the client's translation table with a literal-text fallback.
    /// </summary>
    [PHPProperty("meta")]
    public Dictionary<string, string> Metadata { get; } = metadata;
}
