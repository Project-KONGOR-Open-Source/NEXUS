namespace KONGOR.MasterServer.Models.RequestResponse.ServerManagement;

public abstract class ServerForResponse(string id, string ip, string port, string location)
{
    [PhpProperty("server_id")] public string ID { get; set; } = id;

    [PhpProperty("ip")] public string IPAddress { get; set; } = ip;

    [PhpProperty("port")] public string Port { get; set; } = port;

    [PhpProperty("location")] public string Location { get; set; } = location;
}