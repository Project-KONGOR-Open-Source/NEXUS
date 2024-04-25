namespace ASPIRE.AppHost;

public class ASPIRE
{
    public static void Main(string[] args)
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

        IResourceBuilder<IResourceWithConnectionString> distributedCache = builder.AddRedis("distributed-cache")
            .WithImageRegistry("ghcr.io/microsoft").WithImage("garnet").WithImageTag("latest"); // https://github.com/microsoft/garnet

        IResourceBuilder<IResourceWithConnectionString> databaseConnectionString = builder.AddConnectionString("MERRICK");

        builder.AddProject<MERRICK_Database>("database", builder.Environment.IsProduction() ? "MERRICK.Database Production" : "MERRICK.Database Development")
            .WithReference(databaseConnectionString);

        builder.AddProject<KONGOR_MasterServer>("master-server", builder.Environment.IsProduction() ? "KONGOR.MasterServer Production" : "KONGOR.MasterServer Development")
            .WithReference(databaseConnectionString)
            .WithReference(distributedCache, connectionName: "GARNET");

        builder.AddProject<TRANSMUTANSTEIN_ChatServer>("chat-server", builder.Environment.IsProduction() ? "TRANSMUTANSTEIN.ChatServer Production" : "TRANSMUTANSTEIN.ChatServer Development")
            .WithReference(databaseConnectionString)
            .WithReference(distributedCache, connectionName: "GARNET");

        builder.AddProject<ZORGATH_WebPortal_API>("web-portal-api", builder.Environment.IsProduction() ? "ZORGATH.WebPortal.API Production" : "ZORGATH.WebPortal.API Development")
            .WithReference(databaseConnectionString);

        builder.Build().Run();
    }
}
