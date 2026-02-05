namespace KONGOR.MasterServer.Models.Configuration;

public class OperationalConfiguration
{
    public const string ConfigurationSection = "Operational";

    public required OperationalConfigurationCDN CDN { get; set; }

    public string CurrentSeason { get; set; } = "12";
}
