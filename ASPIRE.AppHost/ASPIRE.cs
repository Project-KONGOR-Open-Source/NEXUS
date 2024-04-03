namespace ASPIRE.AppHost;

public class ASPIRE
{
    public static void Main(string[] args)
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

        builder.AddProject<MERRICK_Database>("database")
            .WithLaunchProfile(builder.Environment.IsProduction() ? "MERRICK.Database Production" : "MERRICK.Database Development");

        // TODO: Set The Connection String Via Environment Variable (Or Use Some Other Way That Solves Duplication)

        builder.AddProject<TRANSMUTANSTEIN_ChatServer>("chat-server")
            .WithLaunchProfile(builder.Environment.IsProduction() ? "TRANSMUTANSTEIN.ChatServer Production" : "TRANSMUTANSTEIN.ChatServer Development");

        builder.AddProject<KONGOR_MasterServer>("master-server")
            .WithLaunchProfile(builder.Environment.IsProduction() ? "KONGOR.MasterServer Production" : "KONGOR.MasterServer Development");

        builder.AddProject<ZORGATH_WebPortal_API>("web-portal-api")
            .WithLaunchProfile(builder.Environment.IsProduction() ? "ZORGATH.WebPortal.API Production" : "ZORGATH.WebPortal.API Development");

        builder.Build().Run();
    }
}
