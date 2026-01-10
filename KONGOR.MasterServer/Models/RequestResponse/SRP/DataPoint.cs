namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class DataPoint
{
    /// <summary>
    ///     The ID of the account.
    /// </summary>
    [PhpProperty("account_id")]
    public required string ID { get; set; }

    /// <summary>
    ///     The account type.
    ///     <br />
    ///     0 = None; 1 = Basic Account; 2 = Verified Account; 3 = Legacy Account
    /// </summary>
    [PhpProperty("standing")]
    public string Standing { get; set; } = "3";

    /// <summary>
    ///     The level of the account.
    /// </summary>
    [PhpProperty("level")]
    public required string Level { get; set; }

    /// <summary>
    ///     The experience of the account.
    /// </summary>
    [PhpProperty("level_exp")]
    public required string Experience { get; set; }

    /// <summary>
    ///     The total number of disconnects, including game modes which are not tracked on the statistics page (e.g. custom
    ///     maps).
    /// </summary>
    [PhpProperty("discos")]
    public required string Disconnects { get; set; }

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PhpProperty("possible_discos")]
    public string PossibleDisconnects { get; set; } = "0";

    /// <summary>
    ///     The total number of matches played, including game modes which are not tracked on the statistics page (e.g. custom
    ///     maps).
    /// </summary>
    [PhpProperty("games_played")]
    public required string MatchesPlayed { get; set; }

    /// <summary>
    ///     The number of bot matches won.
    /// </summary>
    [PhpProperty("num_bot_games_won")]
    public required string BotMatchesWon { get; set; }

    /// <summary>
    ///     The PSR (public skill rating) of the account.
    ///     PSR only applies to public matches.
    /// </summary>
    [PhpProperty("acc_pub_skill")]
    public required string PSR { get; set; }

    /// <summary>
    ///     The number of public matches won.
    /// </summary>
    [PhpProperty("acc_wins")]
    public required string PublicMatchesWon { get; set; }

    /// <summary>
    ///     The number of public matches lost.
    /// </summary>
    [PhpProperty("acc_losses")]
    public required string PublicMatchesLost { get; set; }

    /// <summary>
    ///     The number of public matches played.
    /// </summary>
    [PhpProperty("acc_games_played")]
    public required string PublicMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from public matches.
    /// </summary>
    [PhpProperty("acc_discos")]
    public required string PublicMatchDisconnects { get; set; }

    /// <summary>
    ///     The MMR (match making rating) of the account.
    ///     MMR only applies to ranked matches.
    /// </summary>
    [PhpProperty("rnk_amm_team_rating")]
    public required string MMR { get; set; }

    /// <summary>
    ///     The number of ranked matches won.
    /// </summary>
    [PhpProperty("rnk_wins")]
    public required string RankedMatchesWon { get; set; }

    /// <summary>
    ///     The number of ranked matches lost.
    /// </summary>
    [PhpProperty("rnk_losses")]
    public required string RankedMatchesLost { get; set; }

    /// <summary>
    ///     The number of ranked matches played.
    /// </summary>
    [PhpProperty("rnk_games_played")]
    public required string RankedMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from ranked matches.
    /// </summary>
    [PhpProperty("rnk_discos")]
    public required string RankedMatchDisconnects { get; set; }

    /// <summary>
    ///     The casual MMR (match making rating) of the account.
    ///     Casual MMR only applies to casual ranked matches.
    /// </summary>
    [PhpProperty("cs_amm_team_rating")]
    public required string CasualMMR { get; set; }

    /// <summary>
    ///     The number of casual ranked matches won.
    /// </summary>
    [PhpProperty("cs_wins")]
    public required string CasualRankedMatchesWon { get; set; }

    /// <summary>
    ///     The number of casual ranked matches lost.
    /// </summary>
    [PhpProperty("cs_losses")]
    public required string CasualRankedMatchesLost { get; set; }

    /// <summary>
    ///     The number of casual ranked matches played.
    /// </summary>
    [PhpProperty("cs_games_played")]
    public required string CasualRankedMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from casual ranked matches.
    /// </summary>
    [PhpProperty("cs_discos")]
    public required string CasualRankedMatchDisconnects { get; set; }

    /// <summary>
    ///     The MidWars MMR (match making rating) of the account.
    ///     MidWars MMR only applies to ranked MidWars matches.
    /// </summary>
    [PhpProperty("mid_amm_team_rating")]
    public required string MidWarsMMR { get; set; }

    /// <summary>
    ///     The number of ranked MidWars matches played.
    /// </summary>
    [PhpProperty("mid_games_played")]
    public required string RankedMidWarsMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from ranked MidWars matches.
    /// </summary>
    [PhpProperty("mid_discos")]
    public required string RankedMidWarsMatchDisconnects { get; set; }

    /// <summary>
    ///     The RiftWars MMR (match making rating) of the account.
    ///     RiftWars MMR only applies to ranked RiftWars matches.
    /// </summary>
    [PhpProperty("rift_amm_team_rating")]
    public required string RiftWarsMMR { get; set; }

    /// <summary>
    ///     The number of ranked RiftWars matches played.
    /// </summary>
    [PhpProperty("rift_games_played")]
    public required string RankedRiftWarsMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from ranked RiftWars matches.
    /// </summary>
    [PhpProperty("rift_discos")]
    public required string RankedRiftWarsMatchDisconnects { get; set; }

    /// <summary>
    ///     The number of seasonal ranked matches played.
    /// </summary>
    [PhpProperty("cam_games_played")]
    public required int SeasonalRankedMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from seasonal ranked matches.
    /// </summary>
    [PhpProperty("cam_discos")]
    public required int SeasonalRankedMatchDisconnects { get; set; }

    /// <summary>
    ///     The number of casual seasonal ranked matches played.
    /// </summary>
    [PhpProperty("cam_cs_games_played")]
    public required int CasualSeasonalRankedMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from casual seasonal ranked matches.
    /// </summary>
    [PhpProperty("cam_cs_discos")]
    public required int CasualSeasonalRankedMatchDisconnects { get; set; }

    /// <summary>
    ///     Whether the referral system status of the friend is new or not.
    ///     <br />
    ///     0 = False; 1 = True
    /// </summary>
    [PhpProperty("is_new")]
    public int IsNew { get; set; } = 0;
}