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

    // For Debugging Purposes, The "GiveGold" And "GiveExp" Commands (Case-Insensitive) Can Be Used From The Server Console To Complete Matches Quickly And Send Stats
    // e.g. #1: "givegold 0 65535" To Give 65535 Gold To Player Index 0 (The First Player), Or "givegold KONGOR 65535" To Give 65535 Gold To Player With Name "KONGOR"
    // e.g. #2: "giveexp KONGOR 65535" To Give 65535 Experience To Player With Name "KONGOR" (Unlike The "GiveGold" Command, The "GiveExp" Command Does Not Work With A Player Index)
    // NOTE #1: 1v1 Matches Are A Good Way To Test The Stats Submission System, As They Are The Quickest To Complete
    // NOTE #2: Another Quick Way To Test The Stats Submission System Is To Replay A Fiddler/Requestly/etc. Request Or Make A Postman/Insomnia/etc. Request With The Required Form Data

    [HttpPost(Name = "Stats Requester All-In-One")]
    public async Task<IActionResult> StatsRequester([FromForm] StatsForSubmissionRequestForm form)
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
