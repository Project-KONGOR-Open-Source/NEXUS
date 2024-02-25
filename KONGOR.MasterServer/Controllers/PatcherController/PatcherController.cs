namespace KONGOR.MasterServer.Controllers.PatcherController;

[ApiController]
[Route("patcher/patcher.php")]
[Consumes("application/x-www-form-urlencoded")]
public class PatcherController(ILogger<PatcherController> logger, IOptions<OperationalConfiguration> configuration) : ControllerBase
{
    private ILogger Logger { get; } = logger;
    private OperationalConfiguration Configuration { get; } = configuration.Value;

    [HttpPost(Name = "Patcher")]
    public IActionResult LatestPatch([FromForm] LatestPatchRequestForm form)
    {
        string currentClientVersion = form.CurrentPatchVersion.Split('.').Length is 3 ? form.CurrentPatchVersion + ".0" : form.CurrentPatchVersion;
        string latestClientVersion = "4.10.1"; // Unlike The Current Client's Version, The Revision Number Is Excluded From The Latest Client Version If It's Zero

        LatestPatchResponse response = new()
        {
            PatchVersion = currentClientVersion,
            CurrentPatchVersion = currentClientVersion,
            CurrentManifestZipSHA1Hash = "33b5151fca1704aff892cf76e41f3986634d38bb",
            CurrentManifestZipSizeInBytes = "3628533",
            PatchDetails = new PatchDetails
            {
                OperatingSystem = form.OperatingSystem,
                Architecture = form.Architecture,
                PatchVersion = latestClientVersion,
                LatestPatchVersion = latestClientVersion,
                LatestManifestZipSHA1Hash = "33b5151fca1704aff892cf76e41f3986634d38bb",
                LatestManifestZipSizeInBytes = "3628533",
                PrimaryDownloadURL = Configuration.CDN.PrimaryPatchURL,
                SecondaryDownloadURL = Configuration.CDN.SecondaryPatchURL
            }
        };

        return Ok(PhpSerialization.Serialize(response));
    }
}
