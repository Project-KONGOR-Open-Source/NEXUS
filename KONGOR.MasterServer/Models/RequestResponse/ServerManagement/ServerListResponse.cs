namespace KONGOR.MasterServer.Models.RequestResponse.ServerManagement;

#pragma warning disable CS9113 // Parameter is unread.

public abstract class ServerListResponse
{
    protected ServerListResponse(string cookie)
    {
        string key = Guid.CreateVersion7().ToString();

        AccountKey = key;
        AccountKeyHash = SRPAuthenticationHandlers.ComputeMatchServerChatAuthenticationHash(key, cookie);
    }

    [PHPProperty("acc_key")] public string AccountKey { get; set; }

    [PHPProperty("acc_key_hash")] public string AccountKeyHash { get; set; }

    [PHPProperty("vested_threshold")] public int VestedThreshold => 5;

    [PHPProperty(0)] public bool Zero => true;
}
