namespace ZORGATH.WebPortal.API.Models.Configuration;

public class OperationalConfigurationEmail
{
    public required string ApiKey { get; set; }
    public required string FromEmail { get; set; }
    public string? FromName { get; set; }
}
