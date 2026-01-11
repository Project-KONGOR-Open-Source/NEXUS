using Projects;

namespace ASPIRE.ApplicationHost;

public class ASPIRE
{
    public static void Main(string[] args)
    {
        // Create Distributed Application Builder
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

        // Get Configuration From Environment Variables And User Secrets
        IConfiguration configuration =
            new ConfigurationBuilder().AddEnvironmentVariables().AddUserSecrets<ASPIRE>(true).Build();

        // Get Chat Server Configuration
        IConfigurationSection chatServerConfiguration = builder.Configuration.GetRequiredSection("ChatServer");
        string chatServerHost = chatServerConfiguration.GetValue<string?>("Host") ??
                                throw new NullReferenceException("Chat Server Host Is NULL");
        int chatServerClientConnectionsPort = chatServerConfiguration.GetValue<int?>("ClientPort") ??
                                              throw new NullReferenceException(
                                                  "Chat Server Client Connections Port Is NULL");
        int chatServerMatchServerConnectionsPort = chatServerConfiguration.GetValue<int?>("MatchServerPort") ??
                                                   throw new NullReferenceException(
                                                       "Chat Server Match Server Connections Port Is NULL");
        int chatServerMatchServerManagerConnectionsPort =
            chatServerConfiguration.GetValue<int?>("MatchServerManagerPort") ??
            throw new NullReferenceException("Chat Server Match Server Manager Connections Port Is NULL");

        // Get Infrastructure Configuration
        IConfigurationSection infrastructureConfiguration = builder.Configuration.GetRequiredSection("Infrastructure");
        string gateway = infrastructureConfiguration.GetValue<string?>("Gateway") ??
                         throw new NullReferenceException("Infrastructure Gateway Is NULL");

        // Set Distributed Cache Password Parameter Name And Environment Variable Name
        const string distributedCachePasswordParameterName = "distributed-cache-password";
        const string distributedCachePasswordEnvironmentVariableName = "DISTRIBUTED_CACHE_PASSWORD";

        // Attempt To Resolve Distributed Cache Password From Configuration In Order Of Priority: 1) User Secrets, 2) Environment Variables
        string? resolvedDistributedCachePassword =
            configuration[$"Parameters:{distributedCachePasswordParameterName}"] ??
            configuration[distributedCachePasswordEnvironmentVariableName];

        // Populate Distributed Cache Password If Available In User Secrets Or Environment Variables
        IResourceBuilder<ParameterResource> distributedCachePassword = resolvedDistributedCachePassword is not null
            ? builder.AddParameter(distributedCachePasswordParameterName, resolvedDistributedCachePassword,
                secret: true)
            : builder.AddParameter(distributedCachePasswordParameterName, true);

        // Add Distributed Cache Resource
        IResourceBuilder<RedisResource> distributedCache = builder
            .AddRedis("distributed-cache", password: distributedCachePassword)
            .WithImageTag("latest") // Latest Redis Image: https://github.com/redis/redis/releases/latest
            .WithLifetime(ContainerLifetime.Persistent)
            .WithDataVolume(
                "distributed-cache-data"); // Persist Cached Data Between Distributed Application Restarts But Not Between Resource Container Restarts

        // Create Resource Relationship After Parent Resource Is Defined
        distributedCachePassword
            .WithDescription("Distributed Cache Password") // Add Description To Parameter Resource
            .WithParentRelationship(distributedCache); // Set Distributed Cache As Parent Resource

        // Create Distributed Cache Dashboard Resource
        Action<IResourceBuilder<RedisInsightResource>> distributedCacheDashboard = dashboard => dashboard
            .WithImageTag(
                "latest") // Latest Redis Insight Image: https://github.com/RedisInsight/RedisInsight/releases/latest
            .WithLifetime(ContainerLifetime.Persistent)
            .WithDataVolume(
                "distributed-cache-dashboard-data") // Persist Cached Data Between Distributed Application Restarts But Not Between Resource Container Restarts
            .WithEnvironment("RI_ACCEPT_TERMS_AND_CONDITIONS",
                "true") // Automatically Accept Terms And Conditions: https://redis.io/docs/latest/operate/redisinsight/configuration/
            .WithParentRelationship(distributedCache); // Set Distributed Cache As Parent Resource

        // Add Distributed Cache Dashboard Resource
        distributedCache.WithRedisInsight(distributedCacheDashboard, "distributed-cache-dashboard");

        // Set Database Password Parameter Name And Environment Variable Name
        const string databasePasswordParameterName = "database-password";
        const string databasePasswordEnvironmentVariableName = "DATABASE_PASSWORD";

        // Attempt To Resolve Database Password From Configuration In Order Of Priority: 1) User Secrets, 2) Environment Variables
        string? resolvedDatabasePassword = configuration[$"Parameters:{databasePasswordParameterName}"] ??
                                           configuration[databasePasswordEnvironmentVariableName];

        // Populate Database Password If Available In User Secrets Or Environment Variables
        IResourceBuilder<ParameterResource> databasePassword = resolvedDatabasePassword is not null
            ? builder.AddParameter(databasePasswordParameterName, resolvedDatabasePassword, secret: true)
            : builder.AddParameter(databasePasswordParameterName, true);

        // Configure Database Name Based On Environment
        string databaseName = builder.Environment.IsProduction() ? "production" : "development";

        // Using The Default SQL Server Port Allows The Database To Be Accessible With Just The Host Name/Address And No Port Number (e.g. 127.0.0.1)
        // If The SQL Server Resource Does Not Define A Port, Then It Will Be Randomly Assigned One, And The Connection Address Format Will Be {HOST},{PORT} (e.g. 127.0.0.1,51433)
        // While Aspire's Service Orchestration Is Not Running, The Port To Connect Directly To The Running SQL Server Container Can Be Found In Docker (e.g. "docker container list")
        const int databasePort = 1433;

        // Add SQL Server Container With Persistent Data In A Docker Named Volume (Avoids Windows Host I/O Issues)
        IResourceBuilder<SqlServerServerResource> databaseServer = builder
            .AddSqlServer("database-server", databasePassword, databasePort)
            .WithImageTag(
                "2022-latest") // SQL Server Image Tags: https://mcr.microsoft.com/en-gb/artifact/mar/mssql/server/tags
            .WithLifetime(ContainerLifetime.Persistent)
            .WithDataVolume("merrick-data-" +
                            databaseName) // Persist SQL Server Data In A Docker Volume Managed By The Daemon
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("MSSQL_PID",
                "Developer"); // SQL Server Image Information: https://mcr.microsoft.com/en-gb/artifact/mar/mssql/server/about

        // Create Resource Relationship After Parent Resource Is Defined
        databasePassword
            .WithDescription("Database Password") // Add Description To Parameter Resource
            .WithParentRelationship(databaseServer); // Set Database Server As Parent Resource

        // Add SQL Server Database Resource
        IResourceBuilder<SqlServerDatabaseResource> database = databaseServer.AddDatabase("database", databaseName)
            .WithParentRelationship(databaseServer); // Set Database Server As Parent Resource

        // Add Database Project
        builder.AddProject<MERRICK_DatabaseContext>("database-context",
                builder.Environment.IsProduction()
                    ? "MERRICK.DatabaseContext Production"
                    : "MERRICK.DatabaseContext Development")
            .WithReference(database, "MERRICK")
            .WaitFor(database) // Connect To SQL Server Database And Wait For It To Start
            .WithParentRelationship(databaseServer) // Set Database Server As Parent Resource
            .WithEnvironment("INFRASTRUCTURE_GATEWAY", gateway);

        // Add Master Server Project
        builder.AddProject<KONGOR_MasterServer>("master-server",
                builder.Environment.IsProduction()
                    ? "KONGOR.MasterServer Production"
                    : "KONGOR.MasterServer Development")
            .WithReference(database, "MERRICK")
            .WaitFor(database) // Connect To SQL Server Database And Wait For It To Start
            .WithReference(distributedCache, "DISTRIBUTED-CACHE")
            .WaitFor(distributedCache) // Connect To Distributed Cache And Wait For It To Start
            .WithEnvironment("CHAT_SERVER_HOST", chatServerHost)
            .WithEnvironment("CHAT_SERVER_PORT_CLIENT", chatServerClientConnectionsPort.ToString())
            .WithEnvironment("CHAT_SERVER_PORT_MATCH_SERVER", chatServerMatchServerConnectionsPort.ToString())
            .WithEnvironment("CHAT_SERVER_PORT_MATCH_SERVER_MANAGER",
                chatServerMatchServerManagerConnectionsPort.ToString())
            .WithEnvironment("INFRASTRUCTURE_GATEWAY", gateway);

        // Determine Proxy Setting Based On Environment
        bool isChatServerProxied = builder.Environment.IsProduction();

        // Add Chat Server Project
        builder.AddProject<TRANSMUTANSTEIN_ChatServer>("chat-server",
                builder.Environment.IsProduction()
                    ? "TRANSMUTANSTEIN.ChatServer Production"
                    : "TRANSMUTANSTEIN.ChatServer Development")
            .WithReference(database, "MERRICK")
            .WaitFor(database) // Connect To SQL Server Database And Wait For It To Start
            .WithReference(distributedCache, "DISTRIBUTED-CACHE")
            .WaitFor(distributedCache) // Connect To Distributed Cache And Wait For It To Start
            .WithEnvironment("CHAT_SERVER_HOST", chatServerHost)
            .WithEnvironment("CHAT_SERVER_PORT_CLIENT", chatServerClientConnectionsPort.ToString())
            .WithEnvironment("CHAT_SERVER_PORT_MATCH_SERVER", chatServerMatchServerConnectionsPort.ToString())
            .WithEnvironment("CHAT_SERVER_PORT_MATCH_SERVER_MANAGER",
                chatServerMatchServerManagerConnectionsPort.ToString())
            .WithEnvironment("INFRASTRUCTURE_GATEWAY", gateway)
            .WithEndpoint(targetPort: chatServerClientConnectionsPort, port: chatServerClientConnectionsPort,
                name: "client-port", scheme: "tcp", isProxied: isChatServerProxied)
            .WithEndpoint(targetPort: chatServerMatchServerConnectionsPort, port: chatServerMatchServerConnectionsPort,
                name: "match-server-port", scheme: "tcp", isProxied: isChatServerProxied)
            .WithEndpoint(targetPort: chatServerMatchServerManagerConnectionsPort,
                port: chatServerMatchServerManagerConnectionsPort, name: "manager-port", scheme: "tcp",
                isProxied: isChatServerProxied);

        // Add Web Portal API Project
        IResourceBuilder<ProjectResource> webPortalApi = builder.AddProject<ZORGATH_WebPortal_API>("web-portal-api",
                builder.Environment.IsProduction()
                    ? "ZORGATH.WebPortal.API Production"
                    : "ZORGATH.WebPortal.API Development")
            .WithReference(database, "MERRICK")
            .WaitFor(database) // Connect To SQL Server Database And Wait For It To Start
            .WithEnvironment("INFRASTRUCTURE_GATEWAY", gateway);

        // Add Web Portal UI Project
        builder.AddProject<DAWNBRINGER_WebPortal_UI>("web-portal-ui",
                builder.Environment.IsProduction()
                    ? "DAWNBRINGER.WebPortal.UI Production"
                    : "DAWNBRINGER.WebPortal.UI Development")
            .WithReference(database, "MERRICK")
            .WaitFor(database) // Connect To SQL Server Database And Wait For It To Start
            .WithReference(webPortalApi.GetEndpoint("http")).WaitFor(webPortalApi) // Connect To Web Portal API
            .WithEnvironment("INFRASTRUCTURE_GATEWAY", gateway);

        // Start Orchestrating Distributed Application
        builder.Build().Run();
    }
}