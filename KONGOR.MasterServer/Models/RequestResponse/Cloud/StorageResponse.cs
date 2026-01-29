using KONGOR.MasterServer.Attributes.Serialisation;

namespace KONGOR.MasterServer.Models.RequestResponse.Cloud;

[Serializable]
public class StorageResponse
{
    [PHPProperty("success")]
    public bool Success { get; set; }

    [PHPProperty("messages")]
    public string Messages { get; set; } = "";
}
