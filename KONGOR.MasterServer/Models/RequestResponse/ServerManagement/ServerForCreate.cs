namespace KONGOR.MasterServer.Models.RequestResponse.ServerManagement;

public class ServerForCreate(string id, string ip, string port, string location)
    : ServerForResponse(id, ip, port, location)
{
    [PhpProperty("c_state")] public string Category { get; set; } = "1";
}