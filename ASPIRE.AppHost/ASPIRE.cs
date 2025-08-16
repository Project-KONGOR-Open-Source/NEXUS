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

        // Add Redis Insight Dashboard Resource For Redis Distributed Cache
        Action<IResourceBuilder<RedisInsightResource>> distributedCacheDashboard = builder => builder.WithContainerName("distributed-cache-insight")
            .WithImageTag("latest") // Latest Redis Insight Image: https://github.com/RedisInsight/RedisInsight/releases/latest
            .WithLifetime(ContainerLifetime.Persistent) // Persist Cached Data Between Distributed Application Restarts But Not Between Resource Container Restarts
            .WithEnvironment("RI_ACCEPT_TERMS_AND_CONDITIONS", "true"); // TODO: Confirm This Works From v2.7.2

        // Add Redis Distributed Cache Resource
        IResourceBuilder<IResourceWithConnectionString> distributedCache = builder.AddRedis("distributed-cache")
            .WithImageTag("latest") // Latest Redis Image: https://github.com/redis/redis/releases/latest
            .WithRedisInsight(distributedCacheDashboard) // Add Redis Insight Dashboard
            .WithLifetime(ContainerLifetime.Persistent); // Persist Cached Data Between Distributed Application Restarts But Not Between Resource Container Restarts

        // TODO: Populate Database Password

        // Add Database Password Parameter (Secret)
        IResourceBuilder<ParameterResource> databasePassword = builder.AddParameter("database-password", secret: true);

        // Configure Database Name Based On Environment
        string databaseName = builder.Environment.IsProduction() ? "production" : "development";

        // Configure SQL Server Data Directory
        string userHomeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string databaseDirectory = Path.Combine(userHomeDirectory, "SQL", "MERRICK", databaseName);

        // Add SQL Server Container With Persistent Data In The Current User's Directory (Cross-Platform)
        IResourceBuilder<SqlServerServerResource> databaseServer = builder.AddSqlServer("database-server", password: databasePassword)
            .WithImageTag("latest") // SQL Server Image Tags: https://mcr.microsoft.com/en-gb/artifact/mar/mssql/server/tags
            .WithLifetime(ContainerLifetime.Persistent).WithDataBindMount(source: databaseDirectory) // Persist SQL Server Data Both Between Distributed Application Restarts And Resource Container Restarts
            .WithEnvironment("ACCEPT_EULA", "Y").WithEnvironment("MSSQL_PID", "Developer"); // SQL Server Image Information: https://mcr.microsoft.com/en-gb/artifact/mar/mssql/server/about

        // Add SQL Server Database Resource
        IResourceBuilder<SqlServerDatabaseResource> database = databaseServer.AddDatabase(databaseName);

        // Add Database Project
        builder.AddProject<MERRICK_Database>("database", builder.Environment.IsProduction() ? "MERRICK.Database Production" : "MERRICK.Database Development")
            .WithReference(database, connectionName: "MERRICK").WaitFor(database); // Connect To SQL Server Database And Wait For It To Start

        // Add Master Server Project
        builder.AddProject<KONGOR_MasterServer>("master-server", builder.Environment.IsProduction() ? "KONGOR.MasterServer Production" : "KONGOR.MasterServer Development")
            .WithReference(database, connectionName: "MERRICK").WaitFor(database) // Connect To SQL Server Database And Wait For It To Start
            .WithReference(distributedCache, connectionName: "DISTRIBUTED-CACHE").WaitFor(distributedCache) // Connect To Redis And Wait For It To Start
            .WithEnvironment("CHAT_SERVER_HOST", chatServerHost).WithEnvironment("CHAT_SERVER_PORT", chatServerPort.ToString()); // Pass Chat Server Configuration

        // Add Chat Server Project
        builder.AddProject<TRANSMUTANSTEIN_ChatServer>("chat-server", builder.Environment.IsProduction() ? "TRANSMUTANSTEIN.ChatServer Production" : "TRANSMUTANSTEIN.ChatServer Development")
            .WithReference(database, connectionName: "MERRICK").WaitFor(database) // Connect To SQL Server Database And Wait For It To Start
            .WithReference(distributedCache, connectionName: "DISTRIBUTED-CACHE").WaitFor(distributedCache) // Connect To Redis And Wait For It To Start
            .WithEnvironment("CHAT_SERVER_HOST", chatServerHost).WithEnvironment("CHAT_SERVER_PORT", chatServerPort.ToString()); // Pass Chat Server Configuration

        // Add Web Portal API Project
        builder.AddProject<ZORGATH_WebPortal_API>("web-portal-api", builder.Environment.IsProduction() ? "ZORGATH.WebPortal.API Production" : "ZORGATH.WebPortal.API Development")
            .WithReference(database, connectionName: "MERRICK").WaitFor(database); // Connect To SQL Server Database And Wait For It To Start

        // Start Orchestrating Distributed Application
        builder.Build().Run();
    }
}
