namespace KONGOR.MasterServer.Models.Configuration;

public class RuntimeOptions
{
    public const string ConfigurationSection = "RuntimeOptions";
    public required FunctionalParametersChatServer ChatServer { get; set; }
}

public class FunctionalParametersChatServer
{
    public required FunctionalParametersChatServerHTTP HTTP { get; set; }
    public required FunctionalParametersChatServerHTTPS HTTPS { get; set; }
}

public class FunctionalParametersChatServerHTTP
{
    public required string Protocol { get; set; }
    public required string Host { get; set; }
    public required int Port { get; set; }
}

public class FunctionalParametersChatServerHTTPS
{
    public required string Protocol { get; set; }
    public required string Host { get; set; }
    public required int Port { get; set; }
}
