namespace KONGOR.MasterServer.Models.RequestResponse.ServerManagement;

public abstract class ServerForResponse(string id, string ip, string port, string location)
{
    [PHPProperty("server_id")] public string ID { get; set; } = id;

    [PHPProperty("ip")] public string IPAddress { get; set; } = ip;

    [PHPProperty("port")] public string Port { get; set; } = port;

    [PHPProperty("location")] public string Location { get; set; } = location;
}
