IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ProjectResource> database = builder.AddProject<Projects.MERRICK_Database>("MERRICK Database");

builder.AddProject<Projects.KONGOR_MasterServer>("KONGOR Master Server")
    .WithReference(database);

builder.Build().Run();
