namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

/// <summary>
///     Response model for the "get_initStats" endpoint.
///     Returns initial account statistics used to refresh the client's account information after a match ends without performing a full upgrade refresh.
/// </summary>
public class GetInitialStatisticsResponse
{
    /// <summary>
    ///     Account information keyed by account ID.
    ///     Contains level, experience, skill ratings, games played, disconnections, and other per-mode aggregate data.
    /// </summary>
    [PHPProperty("infos")]
    public required Dictionary<int, FieldStatisticsEntry> Information { get; init; }
}
