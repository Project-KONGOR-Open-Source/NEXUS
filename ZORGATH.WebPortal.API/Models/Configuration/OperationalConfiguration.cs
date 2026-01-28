namespace ZORGATH.WebPortal.API.Models.Configuration;

public class OperationalConfiguration
{
    public const string ConfigurationSection = "Operational";
    public required OperationalConfigurationJWT JWT { get; set; }
    public required OperationalConfigurationEmail Email { get; set; }
    public string UIBaseUrl { get; set; } = string.Empty;
}