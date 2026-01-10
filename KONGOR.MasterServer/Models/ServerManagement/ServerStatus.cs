namespace KONGOR.MasterServer.Models.ServerManagement;

/// <summary>
///     This enumeration is part of the Chat Server Protocol, and needs to match its counterpart in order for servers in
///     the distributed cache to be handled correctly.
/// </summary>
public enum ServerStatus
{
    SERVER_STATUS_SLEEPING,
    SERVER_STATUS_IDLE,
    SERVER_STATUS_LOADING,
    SERVER_STATUS_ACTIVE,
    SERVER_STATUS_CRASHED,
    SERVER_STATUS_KILLED,

    SERVER_STATUS_UNKNOWN
}