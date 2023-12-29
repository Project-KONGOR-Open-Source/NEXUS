namespace ASPIRE.AppHost;

internal class KONGOR
{
    internal static void Main(string[] args)
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

        IResourceBuilder<ProjectResource> database = builder.AddProject<MERRICK_Database_Manager>("MERRICK Database");

        IResourceBuilder<RedisContainerResource> cache = builder.AddRedisContainer("REDIS Cache");

        builder.AddProject<KONGOR_MasterServer>("KONGOR Master Server").WithReference(database);
        builder.AddProject<ZORGATH_WebPortal_API>("ZORGATH Web Portal API").WithReference(database);

        builder.Build().Run();
    }
}
