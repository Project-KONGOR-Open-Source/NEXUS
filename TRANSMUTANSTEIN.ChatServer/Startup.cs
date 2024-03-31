﻿using System.Reflection;
using System.Text.Json;

using TRANSMUTANSTEIN.ChatServer.ConnectionHandlers;
using TRANSMUTANSTEIN.ChatServer.Hubs;

namespace TRANSMUTANSTEIN.ChatServer;

public class Startup
{
    private readonly JsonWriterOptions _jsonWriterOptions = new JsonWriterOptions { Indented = true };

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddConnections();

        services.AddSignalR()
        .AddMessagePackProtocol();
        //.AddStackExchangeRedis();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseFileServer();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<DynamicChat>("/dynamic");
            endpoints.MapHub<Chat>("/default");
            endpoints.MapHub<Streaming>("/streaming");
            endpoints.MapHub<UploadHub>("/uploading");
            endpoints.MapHub<HubTChat>("/hubT");
            endpoints.MapHub<ChatHub>(string.Empty);

            endpoints.MapConnectionHandler<MessagesConnectionHandler>("/chat");

            endpoints.MapGet("/deployment", async context =>
            {
                var attributes = Assembly.GetAssembly(typeof(Startup)).GetCustomAttributes<AssemblyMetadataAttribute>();

                context.Response.ContentType = "application/json";
                await using (var writer = new Utf8JsonWriter(context.Response.BodyWriter, _jsonWriterOptions))
                {
                    writer.WriteStartObject();
                    var commitHash = string.Empty;

                    foreach (var attribute in attributes)
                    {
                        writer.WriteString(attribute.Key, attribute.Value);

                        if (string.Equals(attribute.Key, "CommitHash"))
                        {
                            commitHash = attribute.Value;
                        }
                    }

                    if (!string.IsNullOrEmpty(commitHash))
                    {
                        writer.WriteString("GitHubUrl", $"https://github.com/aspnet/SignalR/commit/{commitHash}");
                    }

                    writer.WriteEndObject();
                    await writer.FlushAsync();
                }
            });
        });
    }
}