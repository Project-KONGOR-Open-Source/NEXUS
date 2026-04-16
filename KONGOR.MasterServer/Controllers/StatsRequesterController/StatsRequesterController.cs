namespace KONGOR.MasterServer.Controllers.StatsRequesterController;

[ApiController]
[Route("stats_requester.php")]
[Consumes("application/x-www-form-urlencoded")]
public partial class StatsRequesterController(MerrickContext databaseContext, IDatabase distributedCache, ILogger<StatsRequesterController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IDatabase DistributedCache { get; } = distributedCache;
    private ILogger Logger { get; } = logger;

    /// <summary>
    ///     <para>
    ///         The stats resubmission key is the SHA1 hash of the match ID prepended to this salt.
    ///     </para>
    ///     <code>
    ///         StatsResubmissionKey = SHA1.HashData(Encoding.UTF8.GetBytes(matchID + MatchStatsSubmissionSalt));
    ///     </code>
    ///     <para>
    ///         This key can be found at offset 0x00F03A10 in k2_x64.dll of the WAS distribution.
    ///     </para>
    /// </summary>
    private static string MatchStatsSubmissionSalt => "s8c7xaduxAbRanaspUf3kadRachecrac9efeyupr8suwrewecrUphayeweqUmana";

    /// <summary>
    ///     The ASP.NET default form value count limit is 1,024, which is not enough for a full 5v5 match stat submission.
    ///     A baseline 5v5 submission uses ~1,040 form values (match stats, team stats, ~95 per-player fields × 10 players, inventory).
    ///     With all optional server CVARs enabled ("svr_submitMatchStatItems", "svr_submitMatchStatAbilities", "svr_submitMatchStatFrags"), a long match can reach ~4,500+ form values due to the per-event item, ability, and frag history entries.
    ///     A value of 8,192 provides sufficient headroom for the worst-case scenario.
    /// </summary>
    private const int StatsSubmissionFormValueCountLimit = 8192;

    // For Debugging Purposes, The "GiveGold" And "GiveExp" Commands (Case-Insensitive) Can Be Used From The Server Console To Complete Matches Quickly And Send Stats
    // e.g. #1: "givegold 0 65535" To Give 65535 Gold To Player Index 0 (The First Player), Or "givegold KONGOR 65535" To Give 65535 Gold To Player With Name "KONGOR"
    // e.g. #2: "giveexp KONGOR 65535" To Give 65535 Experience To Player With Name "KONGOR" (Unlike The "GiveGold" Command, The "GiveExp" Command Does Not Work With A Player Index)
    // NOTE #1: 1v1 Matches Are A Good Way To Test The Stats Submission System, As They Are The Quickest To Complete
    // NOTE #2: Another Quick Way To Test The Stats Submission System Is To Replay A Fiddler/Requestly/etc. Request Or Make A Postman/Insomnia/etc. Request With The Required Form Data

    [HttpPost(Name = "Stats Requester All-In-One")]
    [RequestFormLimits(ValueCountLimit = StatsSubmissionFormValueCountLimit)]
    public async Task<IActionResult> StatsRequester([FromForm] StatsForSubmissionRequestForm form)
    {
        return Request.Form["f"].SingleOrDefault() switch
        {
            "submit_stats"      => await HandleStatsSubmission(form),
            "resubmit_stats"    => await HandleStatsResubmission(form),

            _                   => throw new NotImplementedException($"Unsupported Stats Requester Controller Form Parameter: f={Request.Form["f"].Single()}")
        };
    }
}
