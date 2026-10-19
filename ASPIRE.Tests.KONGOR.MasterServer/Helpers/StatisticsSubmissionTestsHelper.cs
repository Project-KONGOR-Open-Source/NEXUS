namespace ASPIRE.Tests.KONGOR.MasterServer.Helpers;

/// <summary>
///     Builds the statistics submission requests which a match server sends to the master server, in the fields of <see cref="StatsForSubmissionRequestForm"/>.
/// </summary>
internal static class StatisticsSubmissionTestsHelper
{
    /// <summary>
    ///     Builds the smallest "submit_stats" form which the master server accepts, for a match without player statistics, as submitted by the match server with the given session cookie.
    /// </summary>
    public static Dictionary<string, string> BuildSubmission(string session, int serverID, int matchID) => new ()
    {
        ["f"]                               = "submit_stats",
        ["session"]                         = session,
        ["match_stats[server_id]"]          = $"{serverID}",
        ["match_stats[match_id]"]           = $"{matchID}",
        ["match_stats[map]"]                = "caldavar",
        ["match_stats[map_version]"]        = "0.0.0",
        ["match_stats[time_played]"]        = "0",
        ["match_stats[file_size]"]          = "0",
        ["match_stats[file_name]"]          = $"M{matchID}.honreplay",
        ["match_stats[c_state]"]            = "0",
        ["match_stats[version]"]            = "4.10.1.0",
        ["match_stats[avgpsr]"]             = "0",
        ["match_stats[avgpsr_team1]"]       = "0",
        ["match_stats[avgpsr_team2]"]       = "0",
        ["match_stats[gamemode]"]           = "normal",
        ["match_stats[teamscoregoal]"]      = "0",
        ["match_stats[playerscoregoal]"]    = "0",
        ["match_stats[numrounds]"]          = "0",
        ["match_stats[release_stage]"]      = "stable",
        ["match_stats[awd_mann]"]           = "0",
        ["match_stats[awd_mqk]"]            = "0",
        ["match_stats[awd_lgks]"]           = "0",
        ["match_stats[awd_msd]"]            = "0",
        ["match_stats[awd_mkill]"]          = "0",
        ["match_stats[awd_masst]"]          = "0",
        ["match_stats[awd_ledth]"]          = "0",
        ["match_stats[awd_mbdmg]"]          = "0",
        ["match_stats[awd_mwk]"]            = "0",
        ["match_stats[awd_mhdd]"]           = "0",
        ["match_stats[awd_hcs]"]            = "0",
        ["match_stats[submission_debug]"]   = "test",
        ["team_stats[1][score]"]            = "0",
        ["team_stats[2][score]"]            = "0",

        // The Model Binder Requires The Player Statistics, Which An Empty Field Satisfies With An Empty Dictionary
        ["player_stats"]                    = string.Empty
    };
}
