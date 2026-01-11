using System.Net;
using System.Net.Sockets;

using ASPIRE.Tests.InProcess;

using Microsoft.AspNetCore.TestHost;
using Moq;

using Microsoft.Extensions.Configuration;
using TRANSMUTANSTEIN.ChatServer.Services;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

public class TRANSMUTANSTEINServiceProvider : WebApplicationFactory<global::TRANSMUTANSTEIN.ChatServer.TRANSMUTANSTEIN>
{
    public int ClientPort { get; set; } = 50001;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            Dictionary<string, string?> settings = new()
            {
                { "CHAT_SERVER_PORT_CLIENT", ClientPort.ToString() },
                { "CHAT_SERVER_PORT_MATCH_SERVER", (ClientPort + 1).ToString() },
                { "CHAT_SERVER_PORT_MATCH_SERVER_MANAGER", (ClientPort + 2).ToString() },
                // Use localhost for infrastructure to avoid DNS issues in tests
                { "INFRASTRUCTURE_GATEWAY", "localhost" }
            };

            config.AddInMemoryCollection(settings);
        });

        builder.ConfigureTestServices(services =>
        {
            // Remove real database context
            // Remove MerrickContext Registration (Including Pooled & Options)
            Func<ServiceDescriptor, bool> databaseContextPredicate = descriptor =>
                descriptor.ServiceType.FullName?.Contains(nameof(MerrickContext)) is true ||
                descriptor.ImplementationType?.FullName?.Contains(nameof(MerrickContext)) is true;

            Console.WriteLine("[DEBUG] Removing MerrickContext Services...");
            foreach (ServiceDescriptor descriptor in services.Where(databaseContextPredicate).ToList())
            {
                services.Remove(descriptor);
            }

            Console.WriteLine("[DEBUG] MerrickContext Services Removed.");

            // Replace FloodPreventionService with NullFloodPreventionService to prevent test bans
            Console.WriteLine("[DEBUG] Replacing FloodPreventionService...");
            ServiceDescriptor? floodService =
                services.SingleOrDefault(d => d.ServiceType == typeof(FloodPreventionService));
            if (floodService != null)
            {
                Console.WriteLine("[DEBUG] Check: Found original FloodPreventionService. Removing.");
                services.Remove(floodService);
            }
            else
            {
                Console.WriteLine("[DEBUG] Check: Original FloodPreventionService NOT Found.");
            }

            services.AddSingleton<FloodPreventionService, NullFloodPreventionService>();
            Console.WriteLine("[DEBUG] NullFloodPreventionService Registered.");

            // Add in-memory database
            services.AddDbContext<MerrickContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryMerrickDbForChat");
            });

            // Remove real Redis
            ServiceDescriptor? redisDescriptor =
                services.SingleOrDefault(d => d.ServiceType == typeof(IConnectionMultiplexer));
            if (redisDescriptor != null)
            {
                services.Remove(redisDescriptor);
            }

            ServiceDescriptor? redisCacheDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDatabase));
            if (redisCacheDescriptor != null)
            {
                services.Remove(redisCacheDescriptor);
            }

            // Register Mock Redis (using Moq to avoid massive interface stubbing)
            Mock<IConnectionMultiplexer> mockMuxer = new Mock<IConnectionMultiplexer>();
            Mock<ISubscriber> mockSubscriber = new Mock<ISubscriber>();
            InProcessDistributedCacheStore inProcessDb = new InProcessDistributedCacheStore();

            mockMuxer.Setup(m => m.IsConnected).Returns(true);
            mockMuxer.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(inProcessDb);
            mockMuxer.Setup(m => m.GetSubscriber(It.IsAny<object>())).Returns(mockSubscriber.Object);
            mockMuxer.Setup(m => m.ClientName).Returns("MockRedis");
            mockMuxer.Setup(m => m.ToString()).Returns("MockConnectionMultiplexer");
            
            // Ensure Subscriber returns connected state for checks
            mockSubscriber.Setup(s => s.IsConnected(It.IsAny<RedisChannel>())).Returns(true);
            mockSubscriber.Setup(s => s.Multiplexer).Returns(mockMuxer.Object);

            services.AddSingleton(mockMuxer.Object);
            services.AddSingleton<IDatabase>(inProcessDb);
        });
    }

    public static async Task<TRANSMUTANSTEINServiceProvider> CreateOrchestratedInstanceAsync(int clientPort = 50001)
    {
        // If clientPort is 0, we use it as-is to signal dynamic port allocation.
        // Otherwise, we use the provided port.
        TRANSMUTANSTEINServiceProvider provider = new() { ClientPort = clientPort };

        // This forces the server to start
        provider.CreateClient();

        // If we requested a dynamic port (0), we need to find out which port was actually assigned.
        if (clientPort == 0)
        {
            // Resolve ChatService to access the ChatServer and its ClientServer endpoint
            ChatService chatService = provider.Services.GetRequiredService<ChatService>();
            
            // We need to wait until the server is started and bound
            int attempts = 0;
            while ((chatService.ChatServer == null || !chatService.ChatServer.IsStarted) && attempts < 50)
            {
                await Task.Delay(100);
                attempts++;
            }

            if (chatService.ChatServer == null || !chatService.ChatServer.IsStarted)
            {
                 throw new Exception("Chat Server failed to start or ChatService.ChatServer is null.");
            }

            // Retrieve the bound port
            if (chatService.ChatServer.ClientServer.Endpoint is IPEndPoint ipEndpoint)
            {
                provider.ClientPort = ipEndpoint.Port;
                clientPort = provider.ClientPort; // Update local variable for the probe check
            }
            else
            {
                throw new Exception("Could not determine bound port from ClientServer Endpoint.");
            }
        }

        // Wait for the TCP listeners to spin up using exponential backoff probing
        int attempt = 0;
        int maxAttempts = 10;
        int delayMs = 100;

        while (attempt < maxAttempts)
        {
            try
            {
                using TcpClient probe = new();
                await probe.ConnectAsync("localhost", clientPort);
                if (probe.Connected)
                {
                    break;
                }
            }
            catch
            {
                attempt++;
                if (attempt == maxAttempts)
                {
                    throw new Exception(
                        $"Failed to connect to Chat Server on port {clientPort} after {maxAttempts} attempts.");
                }

                await Task.Delay(delayMs);
                delayMs = Math.Min(delayMs * 2, 2000); // Cap delay at 2s
            }
        }

        return provider;
    }
}