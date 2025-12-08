namespace KONGOR.MasterServer.Controllers.PatcherController;

[ApiController]
[Route("patcher/patcher.php")]
[Consumes("application/x-www-form-urlencoded")]
public class PatcherController(ILogger<PatcherController> logger, IDatabase distributedCache, IOptions<OperationalConfiguration> configuration) : ControllerBase
{
    private ILogger Logger { get; } = logger;
    private IDatabase DistributedCache { get; } = distributedCache;
    private OperationalConfiguration Configuration { get; } = configuration.Value;

    [HttpPost(Name = "Patcher")]
    public async Task<IActionResult> LatestPatch([FromForm] LatestPatchRequestForm form)
    {
        if ((await DistributedCache.ValidateAccountSessionCookie(form.Cookie)).IsValid.Equals(false))
        {
            Logger.LogWarning($@"IP Address ""{Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN"}"" Has Made A Patch Request With Forged Cookie ""{form.Cookie}""");

            return Unauthorized($@"Unrecognized Cookie ""{form.Cookie}""");
        }

        (string, string)[] supported = [ ("wac", "x86_64"), ("lac", "x86-biarch"), ("mac", "universal-64") ];

        if (supported.Contains((form.OperatingSystem, form.Architecture)).Equals(false))
            return BadRequest($@"Unsupported Client: Operating System ""{form.OperatingSystem}"", Architecture ""{form.Architecture}""");

        // The Current Client's Version Number Needs To Include The Revision Number Even If It Is Zero (e.g. "4.10.1.0" rather than just "4.10.1")
        PatchDetails currentPatch = PatchHandlers.GetClientPatchDetails(form.OperatingSystem, form.CurrentPatchVersion);

        // Unlike The Current Client's Version, The Revision Number Is Excluded From The Version Number Of The Latest Client If It Is Zero (e.g. "4.10.1.0" becomes just "4.10.1")
        PatchDetails latestPatch = PatchHandlers.GetLatestClientPatchDetails(form.OperatingSystem);

        LatestPatchResponse response = new ()
        {
            PatchVersion = currentPatch.FullVersion,
            CurrentPatchVersion = currentPatch.FullVersion,
            CurrentManifestArchiveSHA1Hash = currentPatch.ManifestArchiveSHA1Hash,
            CurrentManifestArchiveSizeInBytes = currentPatch.ManifestArchiveSizeInBytes,
            PatchDetails = new PatchDetailsForResponse
            {
                OperatingSystem = form.OperatingSystem,
                Architecture = form.Architecture,
                PatchVersion = latestPatch.Version,
                LatestPatchVersion = latestPatch.Version,
                LatestManifestArchiveSHA1Hash = latestPatch.ManifestArchiveSHA1Hash,
                LatestManifestArchiveSizeInBytes = latestPatch.ManifestArchiveSizeInBytes,
                PrimaryDownloadURL = Configuration.CDN.PrimaryPatchURL,
                SecondaryDownloadURL = Configuration.CDN.SecondaryPatchURL
            }
        };

        return Ok(PhpSerialization.Serialize(response));
    }
}
