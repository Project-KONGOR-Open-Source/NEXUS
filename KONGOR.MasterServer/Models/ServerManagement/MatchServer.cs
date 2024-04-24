namespace KONGOR.MasterServer.Models.ServerManagement;

public class MatchServer
{
    public required int HostID { get; set; }

    public required int ID { get; set; }

    public required string Name { get; set; }

    public required int Instance { get; set; }

    public required string IPAddress { get; set; }

    public required int Port { get; set; }

    public required string Location { get; set; }

    public required string Description { get; set; }

    public string Cookie { get; set; } = Guid.NewGuid().ToString();

    public DateTime TimestampRegistered { get; set; } = DateTime.UtcNow;
}
