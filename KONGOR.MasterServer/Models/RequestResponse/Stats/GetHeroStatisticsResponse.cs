namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

/// <summary>
///     Response model for the "get_account_all_hero_stats" request.
/// </summary>
public class GetHeroStatisticsResponse
{
    /// <summary>
    ///     All hero statistics organised by game mode.
    /// </summary>
    [PHPProperty("all_hero_stats")]
    public required AllHeroStatistics AllHeroStatistics { get; set; }

    /// <summary>
    ///     The minimum number of matches a free-to-play (trial) account must complete to become verified.
    ///     A verified account is considered to have full account privileges, and is no longer considered a restricted account.
    /// </summary>
    [PHPProperty("vested_threshold")]
    public int VestedThreshold => 5;

    /// <summary>
    ///     Unknown.
    ///     <br/>
    ///     Seems to be set to "true" on a successful response, or to "false" if an error occurs.
    /// </summary>
    [PHPProperty(0)]
    public bool Zero => true;
}

/// <summary>
///     Container for hero statistics across all game modes.
/// </summary>
public class AllHeroStatistics
{
    /// <summary>
    ///     Hero statistics for ranked matches.
    /// </summary>
    [PHPProperty("ranked")]
    public required List<RankedHeroStatistics> Ranked { get; set; }

    /// <summary>
    ///     Hero statistics for casual matches.
    /// </summary>
    [PHPProperty("casual")]
    public required List<CasualHeroStatistics> Casual { get; set; }

    /// <summary>
    ///     Hero statistics for campaign (normal seasonal) matches.
    /// </summary>
    [PHPProperty("campaign")]
    public required List<CampaignHeroStatistics> Campaign { get; set; }

    /// <summary>
    ///     Hero statistics for campaign casual (casual seasonal) matches.
    /// </summary>
    [PHPProperty("campaign_casual")]
    public required List<CampaignCasualHeroStatistics> CampaignCasual { get; set; }
}

/// <summary>
///     Hero statistics for ranked matches.
/// </summary>
public class RankedHeroStatistics
{
    [PHPProperty("cli_name")]
    public required string HeroIdentifier { get; set; }

    [PHPProperty("rnk_ph_used")]
    public required string TimesUsed { get; set; }

    [PHPProperty("rnk_ph_wins")]
    public required string Wins { get; set; }

    [PHPProperty("rnk_ph_losses")]
    public required string Losses { get; set; }

    [PHPProperty("rnk_ph_herokills")]
    public required string HeroKills { get; set; }

    [PHPProperty("rnk_ph_deaths")]
    public required string Deaths { get; set; }

    [PHPProperty("rnk_ph_heroassists")]
    public required string HeroAssists { get; set; }

    [PHPProperty("rnk_ph_teamcreepkills")]
    public required string TeamCreepKills { get; set; }

    [PHPProperty("rnk_ph_denies")]
    public required string Denies { get; set; }

    [PHPProperty("rnk_ph_exp")]
    public required string Experience { get; set; }

    [PHPProperty("rnk_ph_gold")]
    public required string Gold { get; set; }

    [PHPProperty("rnk_ph_actions")]
    public required string Actions { get; set; }

    [PHPProperty("rnk_ph_time_earning_exp")]
    public required string TimeEarningExperience { get; set; }
}

/// <summary>
///     Hero statistics for casual matches.
/// </summary>
public class CasualHeroStatistics
{
    [PHPProperty("cli_name")]
    public required string HeroIdentifier { get; set; }

    [PHPProperty("cs_ph_used")]
    public required string TimesUsed { get; set; }

    [PHPProperty("cs_ph_wins")]
    public required string Wins { get; set; }

    [PHPProperty("cs_ph_losses")]
    public required string Losses { get; set; }

    [PHPProperty("cs_ph_herokills")]
    public required string HeroKills { get; set; }

    [PHPProperty("cs_ph_deaths")]
    public required string Deaths { get; set; }

    [PHPProperty("cs_ph_heroassists")]
    public required string HeroAssists { get; set; }

    [PHPProperty("cs_ph_teamcreepkills")]
    public required string TeamCreepKills { get; set; }

    [PHPProperty("cs_ph_denies")]
    public required string Denies { get; set; }

    [PHPProperty("cs_ph_exp")]
    public required string Experience { get; set; }

    [PHPProperty("cs_ph_gold")]
    public required string Gold { get; set; }

    [PHPProperty("cs_ph_actions")]
    public required string Actions { get; set; }

    [PHPProperty("cs_ph_time_earning_exp")]
    public required string TimeEarningExperience { get; set; }
}

/// <summary>
///     Hero statistics for campaign (normal seasonal) matches.
/// </summary>
public class CampaignHeroStatistics
{
    [PHPProperty("cli_name")]
    public required string HeroIdentifier { get; set; }

    [PHPProperty("cam_ph_used")]
    public required string TimesUsed { get; set; }

    [PHPProperty("cam_ph_wins")]
    public required string Wins { get; set; }

    [PHPProperty("cam_ph_losses")]
    public required string Losses { get; set; }

    [PHPProperty("cam_ph_herokills")]
    public required string HeroKills { get; set; }

    [PHPProperty("cam_ph_deaths")]
    public required string Deaths { get; set; }

    [PHPProperty("cam_ph_heroassists")]
    public required string HeroAssists { get; set; }

    [PHPProperty("cam_ph_teamcreepkills")]
    public required string TeamCreepKills { get; set; }

    [PHPProperty("cam_ph_denies")]
    public required string Denies { get; set; }

    [PHPProperty("cam_ph_exp")]
    public required string Experience { get; set; }

    [PHPProperty("cam_ph_gold")]
    public required string Gold { get; set; }

    [PHPProperty("cam_ph_actions")]
    public required string Actions { get; set; }

    [PHPProperty("cam_ph_time_earning_exp")]
    public required string TimeEarningExperience { get; set; }
}

/// <summary>
///     Hero statistics for campaign casual (casual seasonal) matches.
/// </summary>
public class CampaignCasualHeroStatistics
{
    [PHPProperty("cli_name")]
    public required string HeroIdentifier { get; set; }

    [PHPProperty("csc_ph_used")]
    public required string TimesUsed { get; set; }

    [PHPProperty("csc_ph_wins")]
    public required string Wins { get; set; }

    [PHPProperty("csc_ph_losses")]
    public required string Losses { get; set; }

    [PHPProperty("csc_ph_herokills")]
    public required string HeroKills { get; set; }

    [PHPProperty("csc_ph_deaths")]
    public required string Deaths { get; set; }

    [PHPProperty("csc_ph_heroassists")]
    public required string HeroAssists { get; set; }

    [PHPProperty("csc_ph_teamcreepkills")]
    public required string TeamCreepKills { get; set; }

    [PHPProperty("csc_ph_denies")]
    public required string Denies { get; set; }

    [PHPProperty("csc_ph_exp")]
    public required string Experience { get; set; }

    [PHPProperty("csc_ph_gold")]
    public required string Gold { get; set; }

    [PHPProperty("csc_ph_actions")]
    public required string Actions { get; set; }

    [PHPProperty("csc_ph_time_earning_exp")]
    public required string TimeEarningExperience { get; set; }
}
