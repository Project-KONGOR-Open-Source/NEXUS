IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ProjectResource> database = builder.AddProject<Projects.MERRICK_Database>("MERRICK Database");

// TODO: look into splitting MERRICK into an entities project and a database manager project

builder.Build().Run();
