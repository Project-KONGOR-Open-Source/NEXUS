namespace KONGOR.MasterServer.Models.RequestResponse.Patch;

public class LatestPatchResponse
{
    /// <summary>
    ///     Details on the latest HoN client patch version.
    /// </summary>
    [PhpProperty(0)]
    public required PatchDetailsForResponse PatchDetails { get; set; }

    /// <summary>
    ///     The HoN client's current version.
    ///     This should match what is sent by the HoN client making the request.
    ///     It is unknown why this duplicates the "current_version" property of the PHP object.
    /// </summary>
    [PhpProperty("version")]
    public required string PatchVersion { get; set; }

    /// <summary>
    ///     The HoN client's current version.
    ///     This should match what is sent by the HoN client making the request.
    /// </summary>
    [PhpProperty("current_version")]
    public required string CurrentPatchVersion { get; set; }

    /// <summary>
    ///     The SHA-1 hash of the zipped manifest file of the client making the request.
    /// </summary>
    [PhpProperty("current_manifest_checksum")]
    public required string CurrentManifestArchiveSHA1Hash { get; set; }

    /// <summary>
    ///     The size in bytes of the zipped manifest file of the client making the request.
    /// </summary>
    [PhpProperty("current_manifest_size")]
    public required string CurrentManifestArchiveSizeInBytes { get; set; }
}

public class PatchDetailsForResponse
{
    /// <summary>
    ///     The latest HoN client version available.
    ///     It is unknown why this duplicates the "latest_version" property of the PHP object.
    /// </summary>
    [PhpProperty("version")]
    public required string PatchVersion { get; set; }

    /// <summary>
    ///     The latest HoN client version available.
    /// </summary>
    [PhpProperty("latest_version")]
    public required string LatestPatchVersion { get; set; }

    /// <summary>
    ///     The HoN client's operating system.
    ///     Generally, (ignoring RCT/SBT/etc.) this will be one of three values: "wac" (Windows client), "lac" (Linux client),
    ///     or "mac" (macOS client).
    /// </summary>
    [PhpProperty("os")]
    public required string OperatingSystem { get; set; }

    /// <summary>
    ///     The HoN client's operating system architecture.
    ///     Generally, this will be one of three values: "x86_64" (Windows client), "x86-biarch" (Linux client), or
    ///     "universal-64" (macOS client).
    ///     The 32-bit versions of Windows ("i686") and macOS ("universal"), alongside other legacy architectures, are not
    ///     supported by Project KONGOR.
    /// </summary>
    [PhpProperty("arch")]
    public required string Architecture { get; set; }

    /// <summary>
    ///     The primary download URL for the patch files.
    ///     This was originally set to "http://cdn.naeu.patch.heroesofnewerth.com/" for the international client.
    /// </summary>
    [PhpProperty("url")]
    public required string PrimaryDownloadURL { get; set; }

    /// <summary>
    ///     The secondary download URL for the patch files.
    ///     If no fallback option exists, this should have the same value as the "url" property of the PHP object.
    ///     For the 32-bit HoN client, this used to be a backup FTP server.
    ///     For the 64-bit HoN client, the same CDN URL was used.
    /// </summary>
    [PhpProperty("url2")]
    public required string SecondaryDownloadURL { get; set; }

    /// <summary>
    ///     The SHA-1 hash of the zipped manifest file of the latest HoN client version.
    /// </summary>
    [PhpProperty("latest_manifest_checksum")]
    public required string LatestManifestArchiveSHA1Hash { get; set; }

    /// <summary>
    ///     The size in bytes of the zipped manifest file of the latest HoN client version.
    /// </summary>
    [PhpProperty("latest_manifest_size")]
    public required string LatestManifestArchiveSizeInBytes { get; set; }
}