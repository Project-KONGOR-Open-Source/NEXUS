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

    protected static string? ResolveIP(MatchServer server, string? serverManagerIPAddress)
    {
        string ipAddress = server.IPAddress;
        bool isPrivate = server.IsPrivate;

        if (string.IsNullOrEmpty(ipAddress) || IPAddress.TryParse(ipAddress, out IPAddress? parsedAddress) is false)
        {
            // If the server manager has a public IP, it is used instead of the server's reported IP.
            // This is useful for servers behind NAT.
            // CAUTION: This might break local development if not handled correctly.
            // But for parity with remote, we accept the public IP logic.
            return isPrivate ? serverManagerIPAddress : server.IPAddress;
        }

        if (IPAddress.IsLoopback(parsedAddress))
        {
             return serverManagerIPAddress;
        }

        byte[] addressBytes = parsedAddress.GetAddressBytes();
         
        // 10.x.x.x
        // 172.16.x.x - 172.31.x.x
        // 192.168.x.x
        bool isPrivateIp = addressBytes[0] == 10
                      || (addressBytes[0] == 172 && addressBytes[1] >= 16 && addressBytes[1] <= 31)
                      || (addressBytes[0] == 192 && addressBytes[1] == 168);

        return isPrivateIp ? serverManagerIPAddress : server.IPAddress;
    }
}
