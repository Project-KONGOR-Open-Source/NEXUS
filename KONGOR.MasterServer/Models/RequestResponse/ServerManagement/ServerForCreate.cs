namespace KONGOR.MasterServer.Models.RequestResponse.ServerManagement;

public class ServerForCreate(string id, string ip, string port, string location)
    : ServerForResponse(id, ip, port, location)
{
    [PHPProperty("c_state")] public string Category { get; set; } = "1";

    [PHPProperty("class")] public string Class { get; set; } = "1";
}
