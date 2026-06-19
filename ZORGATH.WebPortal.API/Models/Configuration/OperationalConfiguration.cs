namespace ZORGATH.WebPortal.API.Models.Configuration;

public class OperationalConfiguration
{
    public const string ConfigurationSection = "Operational";

    public required OperationalConfigurationJWT JWT { get; set; }

    public required OperationalConfigurationSMTP SMTP { get; set; }
}

public class OperationalConfigurationJWT
{
    public required string SigningKey { get; set; }

    public required string Issuer { get; set; }

    public required string Audience { get; set; }

    public required int DurationInHours { get; set; }
}

public class OperationalConfigurationSMTP
{
    public string? Host { get; set; } = Environment.GetEnvironmentVariable("SMTP_HOST");

    public int? Port { get; set; } = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out int port) ? port : null;

    public required string SenderName { get; set; }

    public required string SenderAddress { get; set; }

    public required bool UseTLS { get; set; }

    public string? Username { get; set; } = Environment.GetEnvironmentVariable("SMTP_USERNAME");

    public string? Password { get; set; } = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
}
