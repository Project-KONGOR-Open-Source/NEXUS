namespace KONGOR.MasterServer.Models.RequestResponse.ServerManagement;

public class Server
{
    public required Account Host { get; set; }

    public required uint ID { get; set; }

    public required string Name { get; set; }

    public required int Instance { get; set; }

    public required string IPAddress { get; set; }

    public required int Port { get; set; }

    public required string Location { get; set; }

    public required string Description { get; set; }

    public Guid Cookie { get; set; } = Guid.NewGuid();

    public DateTime TimestampCreated { get; set; } = DateTime.UtcNow;
}
