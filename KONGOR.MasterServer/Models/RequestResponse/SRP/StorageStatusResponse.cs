using KONGOR.MasterServer.Attributes.Serialisation;

namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class StorageStatusResponse
{
    [PHPProperty("success")]
    public bool Success { get; set; } = true;

    [PHPProperty("data")]
    public string? Data { get; set; } = null;

    [PHPProperty("cloud_storage_info")]
    public required CloudStorageInformation CloudStorageInformation { get; set; }

    [PHPProperty("messages")]
    public string Messages { get; set; } = string.Empty;
}
