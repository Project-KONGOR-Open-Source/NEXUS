namespace KONGOR.MasterServer.Models.ServerManagement;

public class MatchServerManager
{
    public required int HostID { get; set; }

    public required uint ID { get; set; }

    public required string IPAddress { get; set; }

    public Guid Cookie { get; set; } = Guid.NewGuid();

    public DateTime TimestampRegistered { get; set; } = DateTime.UtcNow;
}
