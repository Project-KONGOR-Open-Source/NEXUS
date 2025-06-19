using TRANSMUTANSTEIN.ChatServer.Models;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using TRANSMUTANSTEIN.ChatServer.Core;

namespace TRANSMUTANSTEIN.ChatServer.Services;

public class GameServerAllocationService(ILogger<GameServerAllocationService> logger)
{
    private readonly ILogger<GameServerAllocationService> _logger = logger;

    // For demonstration purposes, using a simple in-memory list
    // In production, this would query a database or service registry
    private readonly List<GameServer> _availableServers = new()
    {
        new GameServer { Id = 1, Name = "US-East-01", IPAddress = "192.168.1.10", Port = 11124, Location = "US-East", Description = "US East Server 1" },
        new GameServer { Id = 2, Name = "US-East-02", IPAddress = "192.168.1.11", Port = 11124, Location = "US-East", Description = "US East Server 2" },
        new GameServer { Id = 3, Name = "US-West-01", IPAddress = "192.168.1.20", Port = 11124, Location = "US-West", Description = "US West Server 1" },
        new GameServer { Id = 4, Name = "EU-01", IPAddress = "192.168.1.30", Port = 11124, Location = "EU", Description = "EU Server 1" }
    };    public async Task<GameServer?> AllocateServerAsync(string region = "US")
    {
        try
        {
            // Filter servers by status and region
            var availableServers = _availableServers
                .Where(s => s.Status == ServerStatus.Idle)
                .Where(s => string.IsNullOrEmpty(region) || s.Location.Contains(region, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!availableServers.Any())
            {
                _logger.LogWarning("No available servers found for region: {Region}", region);
                return null;
            }

            // Select the first available server (could implement more sophisticated selection)
            var selectedServer = availableServers.First();
            
            // Reserve the server
            selectedServer.Status = ServerStatus.Loading;
            
            _logger.LogInformation("Allocated server {ServerID} ({Location}) for match", selectedServer.Id, selectedServer.Location);
            
            return selectedServer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error allocating game server for region: {Region}", region);
            return null;
        }
    }    public async Task<bool> UpdateServerStatusAsync(int serverId, ServerStatus status)
    {
        try
        {
            var server = _availableServers.FirstOrDefault(s => s.Id == serverId);
            if (server == null)
            {
                _logger.LogWarning("Server {ServerID} not found", serverId);
                return false;
            }

            server.Status = status;
            _logger.LogInformation("Updated server {ServerID} status to {Status}", serverId, status);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating server {ServerID} status to {Status}", serverId, status);
            return false;
        }
    }    public async Task<bool> ReleaseServerAsync(int serverId)
    {
        return await UpdateServerStatusAsync(serverId, ServerStatus.Idle);
    }

    public async Task<bool> ReserveServerAsync(int serverId)
    {
        return await UpdateServerStatusAsync(serverId, ServerStatus.Loading);
    }

    public async Task<IEnumerable<GameServer>> GetAvailableServersAsync(string? region = null)
    {
        return _availableServers
            .Where(s => s.Status == ServerStatus.Idle)
            .Where(s => string.IsNullOrEmpty(region) || s.Location.Contains(region, StringComparison.OrdinalIgnoreCase));
    }
}
