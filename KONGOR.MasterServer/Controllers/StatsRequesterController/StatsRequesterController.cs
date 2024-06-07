namespace KONGOR.MasterServer.Controllers.StatsRequesterController;

[ApiController]
[Route("stats_requester.php")]
[Consumes("application/x-www-form-urlencoded")]
public partial class StatsRequesterController(MerrickContext databaseContext, IConnectionMultiplexer multiplexer, IMemoryCache cache, ILogger<StatsRequesterController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IDatabase DistributedCache { get; } = multiplexer.GetDatabase();
    private IMemoryCache Cache { get; } = cache;
    private ILogger Logger { get; } = logger;

    private string MatchStatsSubmissionSalt => "s8c7xaduxAbRanaspUf3kadRachecrac9efeyupr8suwrewecrUphayeweqUmana";

    [HttpPost(Name = "Stats Requester All-In-One")]
    public async Task<IActionResult> StatsRequester()
    {
        // TODO: Implement Stats Requester Controller Cookie Validation

        //if (Cache.ValidateAccountSessionCookie(form.Cookie, out string? _).Equals(false))
        //{
        //    Logger.LogWarning($@"IP Address ""{Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN"}"" Has Made A Stats Controller Request With Forged Cookie ""{form.Cookie}""");

        //    return Unauthorized($@"Unrecognized Cookie ""{form.Cookie}""");
        //}

        return Request.Query["f"].SingleOrDefault() switch
        {
            "submit_stats"      => await HandleStatsSubmission(),
            "resubmit_stats"    => await HandleStatsResubmission(),

            _                   => throw new NotImplementedException($"Unsupported Stats Requester Controller Query String Parameter: f={Request.Query["f"].Single()}")
        };
    }
}
