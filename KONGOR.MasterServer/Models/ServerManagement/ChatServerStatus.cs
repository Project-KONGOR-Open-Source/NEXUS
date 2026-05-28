namespace KONGOR.MasterServer.Models.ServerManagement;

/// <summary>
///     The result of a chat server health probe.
/// </summary>
/// <param name="IsHealthy">Whether the chat server reported itself as healthy.</param>
/// <param name="RawStatus">The <c>status</c> field returned by the chat server's health endpoint, or a synthetic error description if the probe failed.</param>
/// <param name="CheckedAt">The UTC timestamp at which the probe was issued.</param>
public sealed record ChatServerStatus(bool IsHealthy, string RawStatus, DateTimeOffset CheckedAt);
