namespace ASPIRE.AppHost;

public class ASPIRE
{
    public static void Main(string[] args)
    {
        // Create Distributed Application Builder
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

        // Get Chat Server Configuration
        string chatServerHost = builder.Configuration.GetRequiredSection("ChatServer").GetValue<string?>("Host") ?? throw new NullReferenceException("Chat Server Host Is NULL");
        int chatServerPort = builder.Configuration.GetRequiredSection("ChatServer").GetValue<int?>("Port") ?? throw new NullReferenceException("Chat Server Port Is NULL");

        // Add Redis Distributed Cache Resource
        IResourceBuilder<IResourceWithConnectionString> distributedCache = builder.AddRedis("distributed-cache")
            .WithImageTag("latest") // Latest Redis Image: https://github.com/redis/redis/releases/latest
            .WithRedisInsight(container => container.WithLifetime(ContainerLifetime.Persistent).WithEnvironment("RI_ACCEPT_TERMS_AND_CONDITIONS", "true"), // TODO: Confirm This Works From v2.7.2
                containerName: "distributed-cache-insight") // Latest RedisInsight Image: https://github.com/RedisInsight/RedisInsight/releases/latest
            .WithLifetime(ContainerLifetime.Persistent); // Persist Cached Data Between Distributed Application Restarts But Not Between Distributed Cache Container Restarts

        // Add Database Connection String Resource
        IResourceBuilder<IResourceWithConnectionString> databaseConnectionString = builder.AddConnectionString("MERRICK");

        // Add Database Project
        builder.AddProject<MERRICK_Database>("database", builder.Environment.IsProduction() ? "MERRICK.Database Production" : "MERRICK.Database Development")
            .WithReference(databaseConnectionString); // Connect To Database

        // Add Master Server Project
        builder.AddProject<KONGOR_MasterServer>("master-server", builder.Environment.IsProduction() ? "KONGOR.MasterServer Production" : "KONGOR.MasterServer Development")
            .WithReference(databaseConnectionString) // Connect To Database
            .WithReference(distributedCache, connectionName: "DISTRIBUTED-CACHE").WaitFor(distributedCache) // Connect To Redis And Wait For It To Start
            .WithEnvironment("CHAT_SERVER_HOST", chatServerHost).WithEnvironment("CHAT_SERVER_PORT", chatServerPort.ToString()); // Pass Chat Server Configuration

        // Add Chat Server Project
        builder.AddProject<TRANSMUTANSTEIN_ChatServer>("chat-server", builder.Environment.IsProduction() ? "TRANSMUTANSTEIN.ChatServer Production" : "TRANSMUTANSTEIN.ChatServer Development")
            .WithReference(databaseConnectionString) // Connect To Database
            .WithReference(distributedCache, connectionName: "DISTRIBUTED-CACHE").WaitFor(distributedCache) // Connect To Redis And Wait For It To Start
            .WithEnvironment("CHAT_SERVER_HOST", chatServerHost).WithEnvironment("CHAT_SERVER_PORT", chatServerPort.ToString()); // Pass Chat Server Configuration

        // Add Web Portal API Project
        builder.AddProject<ZORGATH_WebPortal_API>("web-portal-api", builder.Environment.IsProduction() ? "ZORGATH.WebPortal.API Production" : "ZORGATH.WebPortal.API Development")
            .WithReference(databaseConnectionString); // Connect To Database

        // Start Orchestrating Distributed Application
        builder.Build().Run();
    }
}
