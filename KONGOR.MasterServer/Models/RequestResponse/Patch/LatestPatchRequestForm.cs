namespace KONGOR.MasterServer.Models.RequestResponse.Patch;

public class LatestPatchRequestForm
{
    /// <summary>
    ///     The HoN client's whole version, in the distributions' format (see <see cref="Distributions"/>), sent by the HoN client making the request.
    /// </summary>
    [FromForm(Name = "current_version")]
    public required string CurrentPatchVersion { get; set; }

    /// <summary>
    ///     The HoN client's operating system, sent by the HoN client making the request.
    ///     Generally, (ignoring RCT/SBT/etc.) this will be one of two values: "wac" (Windows client) or "lac" (Linux client).
    /// </summary>
    [FromForm(Name = "os")]
    public required string OperatingSystem { get; set; }

    /// <summary>
    ///     The HoN client's cookie, used to authorize the session.
    /// </summary>
    [FromForm(Name = "cookie")]
    public required string Cookie { get; set; }
}
