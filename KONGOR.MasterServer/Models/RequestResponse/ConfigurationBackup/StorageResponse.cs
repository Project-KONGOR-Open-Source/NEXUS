namespace KONGOR.MasterServer.Models.RequestResponse.ConfigurationBackup;

/// <summary>
///     The response payload for the storage store and storage retrieve endpoints.
/// </summary>
public class StorageResponse
{
    [PHPProperty("success")]
    public bool Success { get; init; }

    [PHPProperty("messages")]
    public string Messages { get; init; } = string.Empty;
}
