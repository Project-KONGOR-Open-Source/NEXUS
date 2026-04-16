namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

/// <summary>
///     Response payload returned to the match server manager after a successful match statistics resubmission.
///     Identical to <see cref="StatisticsSubmissionResponse"/> with the addition of the match ID.
/// </summary>
/// <remarks>
///     The match server manager uses the same validation logic as the initial match server submission.
///     The match ID is included so the match server manager can correlate the response with the <c>.stats</c> file for resubmission.
/// </remarks>
public class StatisticsResubmissionResponse(int matchID)
{
    [PHPProperty("match_id")]
    public int MatchID => matchID;

    [PHPProperty("match_info")]
    public string MatchInformation => "OK";

    [PHPProperty("match_summ")]
    public string MatchSummary => "OK";

    [PHPProperty("match_stats")]
    public string MatchStatistics => "OK";

    [PHPProperty("match_history")]
    public string MatchHistory => "OK";
}
