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