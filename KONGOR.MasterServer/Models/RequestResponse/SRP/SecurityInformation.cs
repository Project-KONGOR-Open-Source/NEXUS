namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class SecurityInformation
{
    /// <summary>
    ///     Used for the Tencent anti-DDoS protection component, which does network packet watermarking that gets verified by
    ///     the game server proxy.
    /// </summary>
    [PhpProperty("initial_vector")]
    public string InitialVector { get; set; } = "73088db5e71cfb6d";

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PhpProperty("hash_code")]
    public string HashCode { get; set; } = "73088db5e71cfb6d43ae0bb4abf095dd43862200";

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PhpProperty("key_version")]
    public string KeyVersion { get; set; } = "3e2d";
}