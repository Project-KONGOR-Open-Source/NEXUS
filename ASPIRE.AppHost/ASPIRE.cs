namespace ASPIRE.AppHost;

public class ASPIRE
{
    public static void Main(string[] args)
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

        builder.AddProject<MERRICK_Database>("MERRICK Database")
            .WithLaunchProfile(builder.Environment.IsProduction() ? "MERRICK.Database Production" : "MERRICK.Database Development");

        builder.AddProject<KONGOR_MasterServer>("KONGOR Master Server")
            .WithLaunchProfile(builder.Environment.IsProduction() ? "KONGOR.MasterServer Production" : "KONGOR.MasterServer Development");

        builder.AddProject<ZORGATH_WebPortal_API>("ZORGATH Web Portal API")
            .WithLaunchProfile(builder.Environment.IsProduction() ? "ZORGATH.WebPortal.API Production" : "ZORGATH.WebPortal.API Development");

        builder.Build().Run();
    }
}
