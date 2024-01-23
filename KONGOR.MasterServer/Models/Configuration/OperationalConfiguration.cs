namespace KONGOR.MasterServer.Models.Configuration;

public class OperationalConfiguration
{
    public const string ConfigurationSection = "Operational";
    public required OperationalConfigurationChatServer ChatServer { get; set; }
}

public class OperationalConfigurationChatServer
{
    public required OperationalConfigurationChatServerHTTP HTTP { get; set; }
    public required OperationalConfigurationChatServerHTTPS HTTPS { get; set; }
}

public class OperationalConfigurationChatServerHTTP
{
    public required string Protocol { get; set; }
    public required string Host { get; set; }
    public required int Port { get; set; }
}

public class OperationalConfigurationChatServerHTTPS
{
    public required string Protocol { get; set; }
    public required string Host { get; set; }
    public required int Port { get; set; }
}
