namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

// {"grab_last_matches":["all_stats","match_summ"]}

public class GrabLastMatchesResponse
{
    [PHPProperty("all_stats")] public required Dictionary<int, List<MatchPlayerStatistics>> AllStats { get; init; }

    [PHPProperty("match_summ")] public required Dictionary<int, MatchSummary> MatchSummary { get; init; }
}
