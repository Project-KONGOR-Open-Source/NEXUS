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
        ["match_stats[version]"]            = Distributions.Version,
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

    /// <summary>
    ///     Builds the smallest "resubmit_stats" form which the master server accepts, for a match without player statistics, as resubmitted for the match server by its host account with the given password.
    /// </summary>
    public static Dictionary<string, string> BuildResubmission(string hostAccountName, string hostPassword, int serverID, int matchID)
    {
        Dictionary<string, string> resubmission = BuildSubmission(string.Empty, serverID, matchID);

        // A Resubmission Is Authenticated By The Host Account Rather Than By A Match Server Session
        resubmission.Remove("session");

        resubmission["f"] = "resubmit_stats";

        // The Host Account Name Ends With The Colon Which Separates It From The Match Server Instance
        resubmission["login"] = $"{hostAccountName}:";
        resubmission["pass"] = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(hostPassword))).ToLower();
        resubmission["resubmission_key"] = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(matchID + StatisticsRequesterController.MatchStatsSubmissionSalt))).ToLower();
        resubmission["server_id"] = $"{serverID}";

        return resubmission;
    }
}
