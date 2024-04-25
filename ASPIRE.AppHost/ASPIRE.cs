namespace ASPIRE.AppHost;

public class ASPIRE
{
    public static void Main(string[] args)
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

        IResourceBuilder<RedisResource> distributedCache = builder.AddRedis("cache", 55513).WithImage("ghcr.io/microsoft/garnet").WithImageTag("latest"); // https://github.com/microsoft/garnet

        // TODO: Put The Cache Resource Port In The Configuration

        IResourceBuilder<IResourceWithConnectionString> databaseConnectionString = builder.AddConnectionString("MERRICK");

        builder.AddProject<MERRICK_Database>("database", builder.Environment.IsProduction() ? "MERRICK.Database Production" : "MERRICK.Database Development")
            .WithReference(databaseConnectionString);

        builder.AddProject<KONGOR_MasterServer>("master-server", builder.Environment.IsProduction() ? "KONGOR.MasterServer Production" : "KONGOR.MasterServer Development")
            .WithReference(databaseConnectionString)
            .WithReference(distributedCache);

        builder.AddProject<TRANSMUTANSTEIN_ChatServer>("chat-server", builder.Environment.IsProduction() ? "TRANSMUTANSTEIN.ChatServer Production" : "TRANSMUTANSTEIN.ChatServer Development")
            .WithReference(databaseConnectionString)
            .WithReference(distributedCache);

        builder.AddProject<ZORGATH_WebPortal_API>("web-portal-api", builder.Environment.IsProduction() ? "ZORGATH.WebPortal.API Production" : "ZORGATH.WebPortal.API Development")
            .WithReference(databaseConnectionString);

        builder.Build().Run();
    }
}
