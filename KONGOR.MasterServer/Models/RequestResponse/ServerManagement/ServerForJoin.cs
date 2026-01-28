namespace KONGOR.MasterServer.Models.RequestResponse.ServerManagement;

public class ServerForJoin(string id, string ip, string port, string location)
    : ServerForResponse(id, ip, port, location)
{
    [PHPProperty("class")] public string Category { get; set; } = "1";
}
