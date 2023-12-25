namespace ASPIRE.AppHost;

internal class KONGOR
{
    internal static void Main(string[] args)
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

        IResourceBuilder<ProjectResource> database = builder.AddProject<MERRICK_Database_Manager>("MERRICK Database");

        builder.AddProject<KONGOR_MasterServer>("KONGOR Master Server")
            .WithReference(database);

        builder.Build().Run();
    }
}
