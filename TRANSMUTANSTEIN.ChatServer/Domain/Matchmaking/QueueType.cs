namespace TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

/// <summary>
///     Defines the distinct partitions used for aggregating matchmaking queue durations.
/// </summary>
public enum QueueType
{
    COOP,
    Caldavar,
    MidWars,
    RiftWars
}
