namespace KONGOR.MasterServer.Models.RequestResponse.ConfigurationBackup;

/// <summary>
///     The response payload for the storage status endpoint, reporting the account's current configuration backup settings.
/// </summary>
public class ConfigurationBackupStatusResponse
{
    [PHPProperty("success")]
    public bool Success { get; init; } = true;

    /// <summary>
    ///     Unused by the client. Always serialised as <see langword="null"/>.
    /// </summary>
    [PHPProperty("data")]
    public string? Data => null;

    [PHPProperty("cloud_storage_info")]
    public required ConfigurationBackupInformation ConfigurationBackupInformation { get; init; }

    [PHPProperty("messages")]
    public string Messages { get; init; } = string.Empty;
}
