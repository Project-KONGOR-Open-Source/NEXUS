namespace ASPIRE.AppHost;

public class ASPIRE
{
    public static void Main(string[] args)
    {
        // Create Distributed Application Builder
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

        // Get Chat Server Configuration
        IConfigurationSection chatServerConfiguration = builder.Configuration.GetRequiredSection("ChatServer");
        string chatServerHost = chatServerConfiguration.GetValue<string?>("Host") ?? throw new NullReferenceException("Chat Server Host Is NULL");
        int chatServerPort = chatServerConfiguration.GetValue<int?>("Port") ?? throw new NullReferenceException("Chat Server Port Is NULL");

        // Add Redis Insight Dashboard Resource For Redis Distributed Cache
        Action<IResourceBuilder<RedisInsightResource>> distributedCacheDashboard = builder => builder
            .WithImageTag("latest") // Latest Redis Insight Image: https://github.com/RedisInsight/RedisInsight/releases/latest
            .WithLifetime(ContainerLifetime.Persistent) // Persist Cached Data Between Distributed Application Restarts But Not Between Resource Container Restarts
            .WithEnvironment("RI_ACCEPT_TERMS_AND_CONDITIONS", "true"); // TODO: Confirm This Works From v2.7.2

        // Add Redis Distributed Cache Resource
        IResourceBuilder<IResourceWithConnectionString> distributedCache = builder.AddRedis("distributed-cache")
            .WithImageTag("latest") // Latest Redis Image: https://github.com/redis/redis/releases/latest
            .WithRedisInsight(distributedCacheDashboard, containerName: "distributed-cache-insight") // Add Redis Insight Dashboard
            .WithLifetime(ContainerLifetime.Persistent); // Persist Cached Data Between Distributed Application Restarts But Not Between Resource Container Restarts

        // Get Configuration From Environment Variables And User Secrets
        IConfiguration configuration = new ConfigurationBuilder().AddEnvironmentVariables().AddUserSecrets<ASPIRE>(optional: true).Build();

        // Set Database Password Parameter Name And Environment Variable Name
        const string databasePasswordParameterName = "database-password"; const string databasePasswordEnvironmentVariableName = "DATABASE_PASSWORD";

        // Attempt To Resolve Database Password From Configuration In Order Of Priority: 1) User Secrets, 2) Environment Variables
        string? resolvedDatabasePassword = configuration[$"Parameters:{databasePasswordParameterName}"] ?? configuration[databasePasswordEnvironmentVariableName];

        // Populate Database Password If Available In User Secrets Or Environment Variables
        IResourceBuilder<ParameterResource> databasePassword = resolvedDatabasePassword is not null
            ? builder.AddParameter(databasePasswordParameterName, resolvedDatabasePassword, secret: true)
            : builder.AddParameter(databasePasswordParameterName, secret: true);

        // Configure Database Name Based On Environment
        string databaseName = builder.Environment.IsProduction() ? "production" : "development";

        // Using The Default SQL Server Port Allows The Database To Be Accessible With Just The Host Name/Address And No Port Number (e.g. 127.0.0.1)
        // If The SQL Server Resource Does Not Define A Port, Then It Will Be Randomly Assigned One, And The Connection Address Format Will Be {HOST},{PORT} (e.g. 127.0.0.1,51433)
        // While Aspire's Service Orchestration Is Not Running, The Port To Connect Directly To The Running SQL Server Container Can Be Found In Docker (e.g. "docker container list")
        const int databasePort = 1433;

        // Configure SQL Server Data Directory
        string userHomeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string databaseDirectory = Path.Combine(userHomeDirectory, "SQL", "MERRICK", databaseName);

        // Add SQL Server Container With Persistent Data In The Current User's Directory (Cross-Platform)
        IResourceBuilder<SqlServerServerResource> databaseServer = builder.AddSqlServer("database-server", password: databasePassword, port: databasePort)
            .WithImageTag("latest") // SQL Server Image Tags: https://mcr.microsoft.com/en-gb/artifact/mar/mssql/server/tags
            .WithLifetime(ContainerLifetime.Persistent).WithDataBindMount(source: databaseDirectory) // Persist SQL Server Data Both Between Distributed Application Restarts And Resource Container Restarts
            .WithEnvironment("ACCEPT_EULA", "Y").WithEnvironment("MSSQL_PID", "Developer"); // SQL Server Image Information: https://mcr.microsoft.com/en-gb/artifact/mar/mssql/server/about

        // Add SQL Server Database Resource
        IResourceBuilder<SqlServerDatabaseResource> database = databaseServer.AddDatabase(databaseName);

        // Add Database Project
        builder.AddProject<MERRICK>("database", builder.Environment.IsProduction() ? "MERRICK.Database Production" : "MERRICK.Database Development")
            .WithReference(database, connectionName: "MERRICK").WaitFor(database); // Connect To SQL Server Database And Wait For It To Start

        // Add Master Server Project
        builder.AddProject<KONGOR>("master-server", builder.Environment.IsProduction() ? "KONGOR.MasterServer Production" : "KONGOR.MasterServer Development")
            .WithReference(database, connectionName: "MERRICK").WaitFor(database) // Connect To SQL Server Database And Wait For It To Start
            .WithReference(distributedCache, connectionName: "DISTRIBUTED-CACHE").WaitFor(distributedCache) // Connect To Redis And Wait For It To Start
            .WithEnvironment("CHAT_SERVER_HOST", chatServerHost).WithEnvironment("CHAT_SERVER_PORT", chatServerPort.ToString()); // Pass Chat Server Configuration

        // Add Chat Server Project
        builder.AddProject<TRANSMUTANSTEIN>("chat-server", builder.Environment.IsProduction() ? "TRANSMUTANSTEIN.ChatServer Production" : "TRANSMUTANSTEIN.ChatServer Development")
            .WithReference(database, connectionName: "MERRICK").WaitFor(database) // Connect To SQL Server Database And Wait For It To Start
            .WithReference(distributedCache, connectionName: "DISTRIBUTED-CACHE").WaitFor(distributedCache) // Connect To Redis And Wait For It To Start
            .WithEnvironment("CHAT_SERVER_HOST", chatServerHost).WithEnvironment("CHAT_SERVER_PORT", chatServerPort.ToString()); // Pass Chat Server Configuration

        // Add Web Portal API Project
        builder.AddProject<ZORGATH>("web-portal-api", builder.Environment.IsProduction() ? "ZORGATH.WebPortal.API Production" : "ZORGATH.WebPortal.API Development")
            .WithReference(database, connectionName: "MERRICK").WaitFor(database); // Connect To SQL Server Database And Wait For It To Start

        // Start Orchestrating Distributed Application
        builder.Build().Run();
    }
}
