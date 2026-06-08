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
            string remoteIPAddress = Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN";

            // If The Cookie Fails Client Session Validation, Check If It Belongs To A Recognised Match Server
            MatchServer? matchServer = string.IsNullOrEmpty(form.Cookie) ? null : await DistributedCache.GetMatchServerBySessionCookie(form.Cookie);

            // If The Cookie Fails Client Session Validation, Check If It Belongs To A Recognised Match Server Manager
            MatchServerManager? matchServerManager = string.IsNullOrEmpty(form.Cookie)
                ? await DistributedCache.GetMatchServerManagerByIPAddress(remoteIPAddress)
                : await DistributedCache.GetMatchServerManagerBySessionCookie(form.Cookie);

            if (matchServer is not null)
            {
                Logger.LogInformation(@"Match Server ""{MatchServerName}"" (ID ""{MatchServerID}"", Version ""{CurrentPatchVersion}"") At IP Address ""{IPAddress}"" Requested Patch Information",
                    matchServer.Name, matchServer.ID, form.CurrentPatchVersion, remoteIPAddress);
            }

            else if (matchServerManager is not null)
            {
                Logger.LogInformation(@"Match Server Manager For Host Account ""{HostAccountName}"" (ID ""{MatchServerManagerID}"", Version ""{CurrentPatchVersion}"") At IP Address ""{IPAddress}"" Requested Patch Information",
                    matchServerManager.HostAccountName, matchServerManager.ID, form.CurrentPatchVersion, remoteIPAddress);
            }

            else // If The Cookie Fails Client Session Validation And Doesn't Belong To A Recognised Match Server Or Match Server Manager, Log A Warning With The Request Context And Return An Unauthorized Response
            {
                string cookie = string.IsNullOrEmpty(form.Cookie) ? "EMPTY" : form.Cookie;

                string requestContext = JsonSerializer.Serialize(new
                {
                    form.Update,
                    form.PatchVersion,
                    form.CurrentPatchVersion,
                    form.OperatingSystem,
                    form.Architecture,
                    RemoteEndpoint = $"{remoteIPAddress}:{Request.HttpContext.Connection.RemotePort}",
                    UserAgent = Request.Headers.UserAgent.ToString()
                });

                Logger.LogWarning(@"IP Address ""{IPAddress}"" Has Requested Patch Information With Forged Cookie ""{Cookie}"" - Request Context: {RequestContext}",
                    remoteIPAddress, cookie, requestContext);

                return Unauthorized($@"Unrecognized Cookie ""{form.Cookie}""");
            }
        }

        (string, string)[] supported =
        [
            // Client Distribution Identifiers And Their Corresponding Supported Architectures
            ("wac", "x86_64"),
            ("lac", "x86-biarch"),
            ("mac", "universal-64"),

            // Server Distribution Identifiers And Their Corresponding Supported Architectures
            ("was-crIac6LASwoafrl8FrOa", "x86_64"),
            ("las-crIac6LASwoafrl8FrOa", "x86-biarch")
        ];

        if (supported.Contains((form.OperatingSystem, form.Architecture)).Equals(false))
            return BadRequest($@"Unsupported Requester: Operating System ""{form.OperatingSystem}"", Architecture ""{form.Architecture}""");

        // The Requester's Current Version Number Needs To Include The Revision Number Even If It Is Zero (e.g. "4.10.1.0" Rather Than Just "4.10.1")
        PatchDetails currentPatch = PatchHandlers.GetPatchDetails(form.OperatingSystem, form.CurrentPatchVersion);

        // Unlike The Requester's Current Version Number, The Revision Number Is Excluded From The Latest Version Number Of The Requester's Distribution If It Is Zero (e.g. "4.10.1.0" Becomes Just "4.10.1")
        PatchDetails latestPatch = PatchHandlers.GetLatestPatchDetails(form.OperatingSystem);

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
