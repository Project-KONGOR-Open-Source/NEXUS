namespace KONGOR.MasterServer.Handlers.Patch;

public static class PatchHandlers
{
    public static PatchDetails? GetLatestClientPatchDetails(string distribution)
        => DistributionVersions.FirstOrDefault(details => details.DistributionIdentifier.Equals(distribution) && details.Latest.Equals(true));

    public static PatchDetails? GetClientPatchDetails(string distribution, string version)
        => DistributionVersions.FirstOrDefault(details => details.DistributionIdentifier.Equals(distribution) && details.Version.Equals(version));

    private static List<PatchDetails> DistributionVersions { get; set; } =
    [
        new PatchDetails()
        {
            DistributionIdentifier = "wac",
            Version = "4.10.1",
            FullVersion = "4.10.1.0",
            ManifestArchiveSHA1Hash = "33b5151fca1704aff892cf76e41f3986634d38bb",
            ManifestArchiveSizeInBytes = "3628533",
            Latest = true
        },
        new PatchDetails()
        {
            DistributionIdentifier = "lac",
            Version = "4.10.1",
            FullVersion = "4.10.1.0",
            ManifestArchiveSHA1Hash = "3977f63f62954e06038c34572a00656a8cb7e311",
            ManifestArchiveSizeInBytes = "6122296",
            Latest = true
        },
        new PatchDetails()
        {
            DistributionIdentifier = "mac",
            Version = "4.10.1",
            FullVersion = "4.10.1.0",
            ManifestArchiveSHA1Hash = "3933009",
            ManifestArchiveSizeInBytes = "ce18408b94a14a968736bb39213daac9a09e4026",
            Latest = true
        },
        new PatchDetails()
        {
            DistributionIdentifier = "was-crIac6LASwoafrl8FrOa",
            Version = "4.10.1",
            FullVersion = "4.10.1.0",
            ManifestArchiveSHA1Hash = "37fc03fc781925e00ae633f8855f3bbd2996c5e7",
            ManifestArchiveSizeInBytes = "1507915",
            Latest = true
        },
        new PatchDetails()
        {
            DistributionIdentifier = "las-crIac6LASwoafrl8FrOa",
            Version = "4.10.1",
            FullVersion = "4.10.1.0",
            ManifestArchiveSHA1Hash = "8ba887c3de95b0b0d33b461bdb4721f29ace952e",
            ManifestArchiveSizeInBytes = "2705984",
            Latest = true
        },
    ];
}
