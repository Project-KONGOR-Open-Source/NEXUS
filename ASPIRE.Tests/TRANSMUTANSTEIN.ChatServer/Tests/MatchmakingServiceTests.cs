using System.Net.Sockets;
using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;
using KONGOR.MasterServer.Extensions.Cache;
using KONGOR.MasterServer.Models.ServerManagement;
using MERRICK.DatabaseContext.Entities.Statistics; // Fixed Namespace
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using TRANSMUTANSTEIN.ChatServer.Domain.Core;
using TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;
using TRANSMUTANSTEIN.ChatServer.Internals;
using TRANSMUTANSTEIN.ChatServer.Services;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;

public class MatchmakingServiceTests
{
    // Integration Test Provider
    private TRANSMUTANSTEINServiceProvider _app = null!;
    private IMatchmakingService _service = null!;
    private MerrickContext _context = null!;
    
    [Before(Test)]
    public async Task Setup()
    {
        // 1. Start Application (Orchestrated Instance)
        _app = await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(0, playersPerTeam: 1); // 0 = Dynamic Port, 1v1 Matchmaking

        // 2. Get Services
        // ACCESS INTERFACE, NOT CONCRETE TYPE
        _service = _app.Services.GetRequiredService<IMatchmakingService>();
        
        // MatchmakingService is a BackgroundService, usually started by Host.
        
        // For database seeding, we use a scope.
        IServiceScope scope = _app.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<MerrickContext>();
    }
    
    [After(Test)]
    public async Task Cleanup()
    {
        await _app.DisposeAsync();
    }
    
    [Test]
    public async Task Broker_ShouldMatch2SoloPlayers_WhenAllCompatible()
    {
        // Arrange
        List<ChatSession> sessions = new();
        List<TcpClient> clients = new List<TcpClient>(); // Keep alive

        IChatContext chatContext = _app.Services.GetRequiredService<IChatContext>();

        for (int i = 1; i <= 2; i++)
        {
            // Connect Real Client
            TcpClient client = await ChatTestHelpers.ConnectAndAuthenticateAsync(_app, _app.ClientPort, i, $"Player{i}");
            clients.Add(client);
            
            // Find session by Account ID
            // IChatContext uses ConcurrentDictionaries directly
            ChatSession? session = chatContext.ClientChatSessions.Values.FirstOrDefault(s => s.Account?.ID == i);
            await Assert.That(session).IsNotNull();
            sessions.Add(session!);

            // Seed Stats (using same context as app)
            if (await _context.AccountStatistics.FindAsync(i) == null)
            {
                _context.AccountStatistics.Add(new AccountStatistics 
                { 
                    AccountID = i, 
                    SkillRating = 1500 
                });
                await _context.SaveChangesAsync();
            }

            // Create Group
            MatchmakingGroupInformation info = new()
            {
                // TeamSize is read-only computed from GameType
                GameType = ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL,
                GameModes = ["ap"],
                GameRegions = ["USW"],
                MapName = "newerth",
                ClientVersion = "4.10.1",
                GroupType = ChatProtocol.TMMType.TMM_TYPE_PVP, // Correct Enum
                Ranked = true,
                MatchFidelity = 0,
                BotDifficulty = 0,
                RandomizeBots = false
            };

            MatchmakingGroup group = MatchmakingGroup.Create(_service, session!, _context, info);
            
            // Join Queue
            group.SendLoadingStatusUpdate(session!, 100);
            group.SendPlayerReadinessStatusUpdate(session!, ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL);
        }

        // Act & Assert
        // Manual "Eventually" Loop
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
        bool success = false;
        while (!cts.Token.IsCancellationRequested)
        {
            if (_service.Groups.Values.All(g => g.QueueStartTime == null))
            {
                success = true;
                break;
            }
            await Task.Delay(100, cts.Token);
        }
        await Assert.That(success).IsTrue();
        
        foreach (TcpClient client in clients)
        {
             client.Dispose();
        }
    }

    [Test]
    public async Task Broker_ShouldNotMatch_DifferentRegions()
    {
        List<TcpClient> clients = new();
        
        try 
        {
            // 5 Players USW
            for (int i = 1; i <= 5; i++)
            {
                 clients.Add(await CreateQueuedGroupReal(i, "USW", ["ap"]));
            }
            
            // 5 Players EU
            for (int i = 6; i <= 10; i++)
            {
                 clients.Add(await CreateQueuedGroupReal(i, "EU", ["ap"]));
            }
    
            await Task.Delay(1000); // Give broker time
    
            // Assert: NO match should occur because buckets are disjoint
            // All groups should still be in queue
            List<MatchmakingGroup> allGroups = _service.Groups.Values.ToList();
            await Assert.That(allGroups.Count).IsEqualTo(10);
            await Assert.That(allGroups.All(g => g.QueueStartTime != null)).IsTrue();
        }
        finally
        {
        foreach(TcpClient client in clients) client.Dispose();
        }
    }
    
    private async Task<TcpClient> CreateQueuedGroupReal(int id, string region, string[] modes)
    {
         TcpClient client = await ChatTestHelpers.ConnectAndAuthenticateAsync(_app, _app.ClientPort, id, $"Player{id}");
         
         IChatContext chatContext = _app.Services.GetRequiredService<IChatContext>();
         // Retry-loop for session availability as ConnectAndAuthenticateAsync might return before Session is fully registered in Context
         ChatSession? session = null;
         for(int k=0; k<10; k++) {
             session = chatContext.ClientChatSessions.Values.FirstOrDefault(s => s.Account?.ID == id);
             if(session != null) break;
             await Task.Delay(50);
         }
         
         await Assert.That(session).IsNotNull();
         
         MerrickContext context = _app.Services.CreateScope().ServiceProvider.GetRequiredService<MerrickContext>();

         if (await context.AccountStatistics.FindAsync(id) == null)
         {
             context.AccountStatistics.Add(new AccountStatistics { AccountID = id, SkillRating = 1500 });
             await context.SaveChangesAsync();
         }

        MatchmakingGroupInformation info = new()
        {
            // TeamSize ReadOnly
            GameType = ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL,
            GameModes = modes,
            GameRegions = [region],
            MapName = "newerth",
            ClientVersion = "4.10.1",
            GroupType = ChatProtocol.TMMType.TMM_TYPE_PVP, // Correct Enum
            Ranked = true,
            MatchFidelity = 0,
            BotDifficulty = 0,
            RandomizeBots = false
        };
        
        MatchmakingGroup group = MatchmakingGroup.Create(_service, session!, context, info);
        group.SendPlayerReadinessStatusUpdate(session!, ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL);
        group.SendLoadingStatusUpdate(session!, 100);
        
        return client;
    }
    [Test]
    public async Task Broker_ShouldAllocateGameServer_WhenMatchFound()
    {
        // 0. Get Services
        IChatContext chatContext = _app.Services.GetRequiredService<IChatContext>();
      
        // 1. Seed Host Account & Match Server
        int hostId = 999;
        string serverCookie = "cookie_host";
        int serverId = 1;
        
        await ChatTestHelpers.SeedGameServerHostAsync(_app, hostId, serverId, serverCookie);

        // 2. Connect Fake Match Server
        // Match Server Port is ClientPort + 100 based on ServiceProvider config
        // Match Server Port is resolved from ServiceProvider
        int matchPort = _app.MatchServerPort;
        
        using TcpClient serverClient = await ChatTestHelpers.ConnectAndAuthenticateGameServerAsync(_app, matchPort, serverId, serverCookie);
        
        // 3. Connect Players and Queue
        List<TcpClient> clients = [];

        try 
        {
            // Create 2 Players
            for (int i = 1; i <= 2; i++)
            {
               clients.Add(await CreateQueuedGroupReal(i, "USW", ["ap"]));
            }

            // 4. Wait for Match Allocation
            // Our "Fake" Match Server should receive NET_CHAT_GS_CREATE_MATCH (0x1502)
            await ChatTestHelpers.ExpectCommandAsync(serverClient.GetStream(), ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_CREATE_MATCH, timeoutSeconds: 30);
            
            // If we reach here, the command was received successfully

        }
        finally
        {
            foreach (TcpClient c in clients) c.Dispose();
        }
    }
}
