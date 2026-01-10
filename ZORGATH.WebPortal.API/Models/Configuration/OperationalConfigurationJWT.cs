namespace ZORGATH.WebPortal.API.Models.Configuration;

public class OperationalConfigurationJWT
{
    public required string SigningKey { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required int DurationInHours { get; set; }
}