namespace ASPIRE.AppHost;

public class ASPIRE
{
    public static void Main(string[] args)
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

        # region Chat Server Configuration

        string chatServerHost = builder.Configuration.GetRequiredSection("ChatServer").GetValue<string?>("Host")
            ?? throw new NullReferenceException("Chat Server Host Is NULL");

        string chatServerAddress = Dns.GetHostEntry(chatServerHost).AddressList.Where(address => address.AddressFamily is not AddressFamily.InterNetworkV6)
            .Select(address => address.MapToIPv4().ToString()).OrderDescending().SingleOrDefault()
                ?? throw new ApplicationException("Unable To Resolve Chat Server Host IP Address");

        int chatServerPort = builder.Configuration.GetRequiredSection("ChatServer").GetValue<int?>("Port")
            ?? throw new NullReferenceException("Chat Server Port Is NULL");

        # endregion

        # region Resource Configuration

        IResourceBuilder<IResourceWithConnectionString> distributedCache = builder.AddRedis("distributed-cache")
            .WithImageRegistry("ghcr.io/microsoft").WithImage("garnet").WithImageTag("latest"); // https://github.com/microsoft/garnet

        IResourceBuilder<IResourceWithConnectionString> databaseConnectionString = builder.AddConnectionString("MERRICK");

        # endregion

        builder.AddProject<MERRICK_Database>("database", builder.Environment.IsProduction() ? "MERRICK.Database Production" : "MERRICK.Database Development")
            .WithReference(databaseConnectionString);

        builder.AddProject<KONGOR_MasterServer>("master-server", builder.Environment.IsProduction() ? "KONGOR.MasterServer Production" : "KONGOR.MasterServer Development")
            .WithReference(databaseConnectionString)
            .WithReference(distributedCache, connectionName: "GARNET")
            .WithEnvironment("CHAT_SERVER_ADDRESS", chatServerAddress).WithEnvironment("CHAT_SERVER_PORT", chatServerPort.ToString());

        builder.AddProject<TRANSMUTANSTEIN_ChatServer>("chat-server", builder.Environment.IsProduction() ? "TRANSMUTANSTEIN.ChatServer Production" : "TRANSMUTANSTEIN.ChatServer Development")
            .WithReference(databaseConnectionString)
            .WithReference(distributedCache, connectionName: "GARNET")
            .WithEnvironment("CHAT_SERVER_ADDRESS", chatServerAddress).WithEnvironment("CHAT_SERVER_PORT", chatServerPort.ToString());

        builder.AddProject<ZORGATH_WebPortal_API>("web-portal-api", builder.Environment.IsProduction() ? "ZORGATH.WebPortal.API Production" : "ZORGATH.WebPortal.API Development")
            .WithReference(databaseConnectionString);

        builder.Build().Run();
    }
}
