namespace ASPIRE.AppHost;

internal class ASPIRE
{
    internal static void Main(string[] args)
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

        IResourceBuilder<ProjectResource> database = builder.AddProject<MERRICK_Database>("MERRICK Database");

        builder.AddProject<KONGOR_MasterServer>("KONGOR Master Server").WithReference(database);
        builder.AddProject<ZORGATH_WebPortal_API>("ZORGATH Web Portal API").WithReference(database);

        builder.Build().Run();
    }
}
