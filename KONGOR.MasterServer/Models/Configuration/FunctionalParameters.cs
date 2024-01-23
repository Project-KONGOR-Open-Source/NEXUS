namespace KONGOR.MasterServer.Models.Configuration;

public class FunctionalParameters
{
    public required FunctionalParametersChatServerAddress ChatServerAddress { get; set; }
}

public class FunctionalParametersChatServerAddress
{
    public required FunctionalParametersChatServerAddressHTTP HTTP { get; set; }
    public required FunctionalParametersChatServerAddressHTTPS HTTPS { get; set; }
}

public class FunctionalParametersChatServerAddressHTTP
{
    public required string Protocol { get; set; }
    public required string Host { get; set; }
    public required int Port { get; set; }
}

public class FunctionalParametersChatServerAddressHTTPS
{
    public required string Protocol { get; set; }
    public required string Host { get; set; }
    public required int Port { get; set; }
}
