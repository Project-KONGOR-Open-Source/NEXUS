namespace TRANSMUTANSTEIN.ChatServer.Models;

public class GameServer
{
    public required int Id { get; set; }
    public int ID => Id; // Alias for backward compatibility
    public required string Name { get; set; }
    public required string IPAddress { get; set; }
    public required int Port { get; set; }
    public required string Location { get; set; }
    public required string Description { get; set; }
    public ServerStatus Status { get; set; } = ServerStatus.Idle;
    public string Cookie { get; set; } = Guid.NewGuid().ToString();
    public DateTime TimestampRegistered { get; set; } = DateTime.UtcNow;
    public int MaxPlayers { get; set; } = 10;
    public int CurrentPlayers { get; set; } = 0;
    public string HostAccountName { get; set; } = "GameServer";
}

public enum ServerStatus
{
    Sleeping,
    Idle,
    Loading,
    Active,
    Crashed,
    Killed,
    Unknown
}
