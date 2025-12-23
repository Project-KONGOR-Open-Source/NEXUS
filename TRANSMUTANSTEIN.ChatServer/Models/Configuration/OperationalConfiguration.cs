namespace TRANSMUTANSTEIN.ChatServer.Models.Configuration;

public class OperationalConfiguration
{
    public const string ConfigurationSection = "Operational";
    public required OperationalConfigurationService Service { get; set; }
}

public class OperationalConfigurationService
{
    public required string[] CorsOrigins { get; set; }
}
