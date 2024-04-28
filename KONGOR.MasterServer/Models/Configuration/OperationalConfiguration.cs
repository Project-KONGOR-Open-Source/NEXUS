namespace KONGOR.MasterServer.Models.Configuration;

public class OperationalConfiguration
{
    public const string ConfigurationSection = "Operational";
    public required OperationalConfigurationCDN CDN { get; set; }
}

public class OperationalConfigurationCDN
{
    public required string PrimaryPatchURL { get; set; }
    public required string SecondaryPatchURL { get; set; }
}
