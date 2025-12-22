namespace ZORGATH.WebPortal.API.Models.Configuration;

public class OperationalConfiguration
{
    public const string ConfigurationSection = "Operational";
    public required OperationalConfigurationJWT JWT { get; set; }
    public required OperationalConfigurationDiscord Discord { get; set; }
}

public class OperationalConfigurationJWT
{
    public required string SigningKey { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required int DurationInHours { get; set; }
}

public class OperationalConfigurationDiscord
{
    public required string ClientID { get; set; }
    public required string ClientSecret { get; set; }
}
