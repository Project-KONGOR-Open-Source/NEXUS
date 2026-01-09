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
        if (!string.IsNullOrEmpty(form.Cookie) && (await DistributedCache.ValidateAccountSessionCookie(form.Cookie)).IsValid.Equals(false))
        {
            Logger.LogWarning($@"IP Address ""{Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN"}"" Has Requested Patch Information With Forged Cookie ""{form.Cookie}""");

            return Unauthorized($@"Unrecognized Cookie ""{form.Cookie}""");
        }

        (string, string)[] supported = [ 
            ("wac", "x86_64"), ("lac", "x86-biarch"), ("mac", "universal-64"),
            ("was-crIac6LASwoafrl8FrOa", "x86_64"), ("las-crIac6LASwoafrl8FrOa", "x86_64"), ("mas-crIac6LASwoafrl8FrOa", "x86_64")
        ];

        // Default to Windows/x64 if not specified (for 'latest' checks)
        string os = form.OperatingSystem ?? "wac";
        string arch = form.Architecture ?? "x86_64";

        if (supported.Contains((os, arch)).Equals(false))
            return BadRequest($@"Unsupported Client: Operating System ""{os}"", Architecture ""{arch}""");

        // The Current Client's Version Number Needs To Include The Revision Number Even If It Is Zero (e.g. "4.10.1.0" rather than just "4.10.1")
        PatchDetails? currentPatch = null;
        if (!string.IsNullOrEmpty(form.CurrentPatchVersion))
        {
            currentPatch = PatchHandlers.GetClientPatchDetails(os, form.CurrentPatchVersion);
            
            if (currentPatch is null)
                return NotFound($@"Current Patch Details Not Found For Version ""{form.CurrentPatchVersion}""");
        }

        // Unlike The Current Client's Version, The Revision Number Is Excluded From The Version Number Of The Latest Client If It Is Zero (e.g. "4.10.1.0" becomes just "4.10.1")
        PatchDetails? latestPatch = PatchHandlers.GetLatestClientPatchDetails(os);
        
        if (latestPatch is null)
        {
             Logger.LogError(@"Latest Patch Details Not Found For Distribution ""{OperatingSystem}""", os);
             return StatusCode(500, "Internal Server Error: Latest Patch Details Not Found");
        }

        // Handle case where we don't have current patch details (e.g. fresh install check or 'latest' probe)
        // If currentPatch is null, we assume they are on 0.0.0.0 or just want latest info.
        // But LatestPatchResponse expects non-null strings? Let's check model.
        // Assuming we can return "0.0.0.0" as current if unknown.

        LatestPatchResponse response = new ()
        {
            PatchVersion = currentPatch?.FullVersion ?? "0.0.0.0",
            CurrentPatchVersion = currentPatch?.FullVersion ?? "0.0.0.0",
            CurrentManifestArchiveSHA1Hash = currentPatch?.ManifestArchiveSHA1Hash ?? string.Empty,
            CurrentManifestArchiveSizeInBytes = currentPatch?.ManifestArchiveSizeInBytes ?? "0",
            PatchDetails = new PatchDetailsForResponse
            {
                OperatingSystem = os,
                Architecture = arch,
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
