namespace KONGOR.MasterServer.Models.RequestResponse.Patch;

public class LatestPatchRequestForm
{
    /// <summary>
    ///     Unknown.
    ///     This value appears to always be "1", perhaps indicating that a patch request was issued.
    /// </summary>
    [FromForm(Name = "update")]
    public required string Update { get; set; }

    /// <summary>
    ///     Unknown.
    ///     This value appears to always be "0.0.0.0".
    /// </summary>
    [FromForm(Name = "version")]
    public required string PatchVersion { get; set; }

    /// <summary>
    ///     The HoN client's version, in the format "1.2.3(.4)", sent by the HoN client making the request.
    /// </summary>
    [FromForm(Name = "current_version")]
    public required string CurrentPatchVersion { get; set; }

    /// <summary>
    ///     The HoN client's operating system, sent by the HoN client making the request.
    ///     Generally, (ignoring RCT/SBT/etc.) this will be one of three values: "wac" (Windows client), "lac" (Linux client), or "mac" (macOS client).
    /// </summary>
    [FromForm(Name = "os")]
    public required string OperatingSystem { get; set; }

    /// <summary>
    ///     The HoN client's operating system architecture, sent by the HoN client making the request.
    ///     Generally, this will be one of three values: "x86_64" (Windows client), "x86-biarch" (Linux client), or "universal-64" (macOS client).
    ///     The 32-bit versions of Windows ("i686") and macOS ("universal"), alongside other legacy architectures, are not supported by Project KONGOR.
    /// </summary>
    [FromForm(Name = "arch")]
    public required string Architecture { get; set; }

    /// <summary>
    ///     The HoN client's cookie, used to authorize the session.
    /// </summary>
    [FromForm(Name = "cookie")]
    public required string Cookie { get; set; }
}
