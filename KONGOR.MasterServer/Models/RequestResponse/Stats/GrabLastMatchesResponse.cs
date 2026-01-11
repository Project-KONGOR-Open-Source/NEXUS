using System.Text.Json.Serialization;

namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

// {"grab_last_matches":["all_stats","match_summ"]}

public class GrabLastMatchesResponse
{
    [PhpProperty("all_stats")]
    public required Dictionary<int, List<MatchPlayerStatistics>> AllStats { get; init; }

    [PhpProperty("match_summ")]
    public required Dictionary<int, MatchSummary> MatchSummary { get; init; }
}
