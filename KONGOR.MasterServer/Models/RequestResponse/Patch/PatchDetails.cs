namespace KONGOR.MasterServer.Models.RequestResponse.Patch;

public class PatchDetails
{
    public required string DistributionIdentifier { get; set; }
    public required string Version { get; set; }
    public required string FullVersion { get; set; }
    public required string ManifestArchiveSHA1Hash { get; set; }
    public required string ManifestArchiveSizeInBytes { get; set; }
    public required bool Latest { get; set; }
}