using ASPIRE.Tests.InProcess;


using MERRICK.DatabaseContext;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using StackExchange.Redis;

using TRANSMUTANSTEIN.ChatServer.Services;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

public class TRANSMUTANSTEINServiceProvider : WebApplicationFactory<global::TRANSMUTANSTEIN.ChatServer.TRANSMUTANSTEIN>
{
    public int ClientPort { get; set; } = 50001;

    private const string ChatServerPortMatchServer = "50002";
    private const string ChatServerPortMatchServerManager = "50003";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            Dictionary<string, string?> settings = new Dictionary<string, string?>
            {
                { "CHAT_SERVER_PORT_CLIENT", ClientPort.ToString() },
                { "CHAT_SERVER_PORT_MATCH_SERVER", ChatServerPortMatchServer },
                { "CHAT_SERVER_PORT_MATCH_SERVER_MANAGER", ChatServerPortMatchServerManager },
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
                    descriptor.ServiceType.FullName?.Contains(nameof(MerrickContext)) is true || descriptor.ImplementationType?.FullName?.Contains(nameof(MerrickContext)) is true;

            foreach (ServiceDescriptor? descriptor in services.Where(databaseContextPredicate).ToList())
                services.Remove(descriptor);

            // Add in-memory database
            services.AddDbContext<MerrickContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryMerrickDbForChat")
                       .EnableServiceProviderCaching(false);
            });

            // Remove real Redis
            ServiceDescriptor? redisDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IConnectionMultiplexer));
            if (redisDescriptor != null)
                services.Remove(redisDescriptor);

            ServiceDescriptor? redisCacheDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDatabase));
            if (redisCacheDescriptor != null)
                services.Remove(redisCacheDescriptor);

            // Add In-Process Redis Stub
            services.AddSingleton<IDatabase, InProcessDistributedCacheStore>();
        });
    }

    public static async Task<TRANSMUTANSTEINServiceProvider> CreateOrchestratedInstanceAsync(int clientPort = 50001)
    {
        TRANSMUTANSTEINServiceProvider provider = new TRANSMUTANSTEINServiceProvider
        {
            ClientPort = clientPort
        };

        // This forces the server to start
        HttpClient client = provider.CreateClient();

        // Wait briefly for the TCP listeners to spin up
        await Task.Delay(1000);

        return provider;
    }
}
