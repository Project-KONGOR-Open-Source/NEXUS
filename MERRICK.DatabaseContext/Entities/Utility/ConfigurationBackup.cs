namespace MERRICK.DatabaseContext.Entities.Utility;

/// <summary>
///     A per-account backup of the game client's configuration archive, together with the account's cloud enrolment preferences.
///     The game client compresses its local configuration into an archive, uploads it here, and can download it again on another machine.
/// </summary>
public class ConfigurationBackup
{
    [Key]
    public int ID { get; set; }

    public int AccountID { get; set; }

    [ForeignKey(nameof(AccountID))]
    public required Account Account { get; set; }

    /// <summary>
    ///     Whether the account has opted to automatically download its backed-up configuration when logging in.
    /// </summary>
    public bool UseCloud { get; set; } = false;

    /// <summary>
    ///     Whether the account has opted to automatically upload its configuration changes to the cloud.
    /// </summary>
    public bool AutomaticUpload { get; set; } = false;

    /// <summary>
    ///     The modification timestamp of the stored configuration archive, as reported by the game client when it was last uploaded.
    /// </summary>
    public string? FileModificationTime { get; set; } = null;

    /// <summary>
    ///     The compressed archive containing the account's backed-up client configuration.
    /// </summary>
    public byte[]? ConfigurationArchive { get; set; } = null;
}
