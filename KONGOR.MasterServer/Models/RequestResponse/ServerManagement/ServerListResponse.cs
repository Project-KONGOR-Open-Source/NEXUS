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

    [PhpProperty("acc_key")] public string AccountKey { get; set; }

    [PhpProperty("acc_key_hash")] public string AccountKeyHash { get; set; }

    [PhpProperty("vested_threshold")] public int VestedThreshold => 5;

    [PhpProperty(0)] public bool Zero => true;
}