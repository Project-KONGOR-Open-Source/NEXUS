namespace ASPIRE.AppHost;

public class ASPIRE
{
    public static void Main(string[] args)
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

        string chatServerHost = builder.Configuration.GetRequiredSection("ChatServer").GetValue<string?>("Host") ?? throw new NullReferenceException("Chat Server Host Is NULL");
        int chatServerPort = builder.Configuration.GetRequiredSection("ChatServer").GetValue<int?>("Port") ?? throw new NullReferenceException("Chat Server Port Is NULL");

        IResourceBuilder<IResourceWithConnectionString> distributedCache = builder.AddRedis("distributed-cache", port: 6379 /* For Redis Insight (https://redis.io/insight/) */ )
            .WithImageRegistry("ghcr.io/microsoft").WithImage("garnet").WithImageTag("latest"); // https://github.com/microsoft/garnet

        IResourceBuilder<IResourceWithConnectionString> databaseConnectionString = builder.AddConnectionString("MERRICK");

        builder.AddProject<MERRICK_Database>("database", builder.Environment.IsProduction() ? "MERRICK.Database Production" : "MERRICK.Database Development")
            .WithReference(databaseConnectionString);

        builder.AddProject<KONGOR_MasterServer>("master-server", builder.Environment.IsProduction() ? "KONGOR.MasterServer Production" : "KONGOR.MasterServer Development")
            .WithReference(databaseConnectionString)
            .WithReference(distributedCache, connectionName: "GARNET")
            .WithEnvironment("CHAT_SERVER_HOST", chatServerHost).WithEnvironment("CHAT_SERVER_PORT", chatServerPort.ToString());

        builder.AddProject<TRANSMUTANSTEIN_ChatServer>("chat-server", builder.Environment.IsProduction() ? "TRANSMUTANSTEIN.ChatServer Production" : "TRANSMUTANSTEIN.ChatServer Development")
            .WithReference(databaseConnectionString)
            .WithReference(distributedCache, connectionName: "GARNET")
            .WithEnvironment("CHAT_SERVER_HOST", chatServerHost).WithEnvironment("CHAT_SERVER_PORT", chatServerPort.ToString());

        builder.AddProject<ZORGATH_WebPortal_API>("web-portal-api", builder.Environment.IsProduction() ? "ZORGATH.WebPortal.API Production" : "ZORGATH.WebPortal.API Development")
            .WithReference(databaseConnectionString);

        builder.Build().Run();
    }
}
