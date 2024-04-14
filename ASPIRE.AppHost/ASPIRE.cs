namespace ASPIRE.AppHost;

public class ASPIRE
{
    public static void Main(string[] args)
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

        builder.AddProject<MERRICK_Database>("database", builder.Environment.IsProduction() ? "MERRICK.Database Production" : "MERRICK.Database Development");

        // TODO: Set The Connection String Via Environment Variable (Or Use Some Other Way That Solves Duplication)

        IResourceBuilder<RedisResource> cache = builder.AddRedis("cache", 55513).WithImage("ghcr.io/microsoft/garnet").WithImageTag("latest"); // https://github.com/microsoft/garnet

        // TODO: Put The Cache Resource Port In The Configuration

        builder.AddProject<KONGOR_MasterServer>("master-server", builder.Environment.IsProduction() ? "KONGOR.MasterServer Production" : "KONGOR.MasterServer Development")
            .WithReference(cache);

        builder.AddProject<TRANSMUTANSTEIN_ChatServer>("chat-server", builder.Environment.IsProduction() ? "TRANSMUTANSTEIN.ChatServer Production" : "TRANSMUTANSTEIN.ChatServer Development")
            .WithReference(cache);

        builder.AddProject<ZORGATH_WebPortal_API>("web-portal-api", builder.Environment.IsProduction() ? "ZORGATH.WebPortal.API Production" : "ZORGATH.WebPortal.API Development");

        builder.Build().Run();
    }
}
