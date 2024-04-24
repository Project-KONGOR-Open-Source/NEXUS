namespace KONGOR.MasterServer.Models.ServerManagement;

public class MatchServerManager
{
    public required int HostID { get; set; }

    public required int ID { get; set; }

    public required string IPAddress { get; set; }

    public string Cookie { get; set; } = Guid.NewGuid().ToString();

    public DateTime TimestampRegistered { get; set; } = DateTime.UtcNow;
}
