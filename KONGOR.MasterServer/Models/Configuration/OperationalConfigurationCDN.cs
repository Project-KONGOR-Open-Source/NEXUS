namespace KONGOR.MasterServer.Models.Configuration;

public class OperationalConfigurationCDN
{
    public required string Host { get; set; }

    public required string PrimaryPatchURL { get; set; }

    public required string SecondaryPatchURL { get; set; }
}
