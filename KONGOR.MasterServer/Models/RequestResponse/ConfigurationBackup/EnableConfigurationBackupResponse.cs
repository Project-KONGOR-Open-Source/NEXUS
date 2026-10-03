namespace KONGOR.MasterServer.Models.RequestResponse.ConfigurationBackup;

/// <summary>
///     The response payload for the cloud set-user-enrollment endpoint, reporting the account's updated configuration backup settings.
/// </summary>
public class EnableConfigurationBackupResponse
{
    [PHPProperty("success")]
    public bool Success { get; init; } = true;

    [PHPProperty("data")]
    public required ConfigurationBackupInformation Data { get; init; }
}
