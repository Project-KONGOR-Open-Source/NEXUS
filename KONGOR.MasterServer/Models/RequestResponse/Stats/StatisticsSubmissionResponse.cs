namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

/// <summary>
///     Response payload returned to the match server after a successful match statistics submission.
///     The match server validates the response by deserialising it as PHP and checking that all four keys are set to "OK".
///     If the response is empty, not valid PHP, or missing any of the expected keys, the match server reports <c>SSR_ERROR_INVALID_RESPONSE</c>.
/// </summary>
public class StatisticsSubmissionResponse
{
    [PHPProperty("match_info")]
    public string MatchInformation => "OK";

    [PHPProperty("match_summ")]
    public string MatchSummary => "OK";

    [PHPProperty("match_stats")]
    public string MatchStatistics => "OK";

    [PHPProperty("match_history")]
    public string MatchHistory => "OK";
}
