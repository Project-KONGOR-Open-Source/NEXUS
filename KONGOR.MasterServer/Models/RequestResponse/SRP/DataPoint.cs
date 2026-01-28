namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class DataPoint
{
    /// <summary>
    ///     The ID of the account.
    /// </summary>
    [PHPProperty("account_id")]
    public required string ID { get; set; }

    /// <summary>
    ///     The account type.
    ///     <br />
    ///     0 = None; 1 = Basic Account; 2 = Verified Account; 3 = Legacy Account
    /// </summary>
    [PHPProperty("standing")]
    public string Standing { get; set; } = "3";

    /// <summary>
    ///     The level of the account.
    /// </summary>
    [PHPProperty("level")]
    public required string Level { get; set; }

    /// <summary>
    ///     The experience of the account.
    /// </summary>
    [PHPProperty("level_exp")]
    public required string Experience { get; set; }

    /// <summary>
    ///     The total number of disconnects, including game modes which are not tracked on the statistics page (e.g. custom
    ///     maps).
    /// </summary>
    [PHPProperty("discos")]
    public required string Disconnects { get; set; }

    /// <summary>
    ///     Unknown.
    /// </summary>
    [PHPProperty("possible_discos")]
    public string PossibleDisconnects { get; set; } = "0";

    /// <summary>
    ///     The total number of matches played, including game modes which are not tracked on the statistics page (e.g. custom
    ///     maps).
    /// </summary>
    [PHPProperty("games_played")]
    public required string MatchesPlayed { get; set; }

    /// <summary>
    ///     The number of bot matches won.
    /// </summary>
    [PHPProperty("num_bot_games_won")]
    public required string BotMatchesWon { get; set; }

    /// <summary>
    ///     The PSR (public skill rating) of the account.
    ///     PSR only applies to public matches.
    /// </summary>
    [PHPProperty("acc_pub_skill")]
    public required string PSR { get; set; }

    /// <summary>
    ///     The number of public matches won.
    /// </summary>
    [PHPProperty("acc_wins")]
    public required string PublicMatchesWon { get; set; }

    /// <summary>
    ///     The number of public matches lost.
    /// </summary>
    [PHPProperty("acc_losses")]
    public required string PublicMatchesLost { get; set; }

    /// <summary>
    ///     The number of public matches played.
    /// </summary>
    [PHPProperty("acc_games_played")]
    public required string PublicMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from public matches.
    /// </summary>
    [PHPProperty("acc_discos")]
    public required string PublicMatchDisconnects { get; set; }

    /// <summary>
    ///     The MMR (match making rating) of the account.
    ///     MMR only applies to ranked matches.
    /// </summary>
    [PHPProperty("rnk_amm_team_rating")]
    public required string MMR { get; set; }

    /// <summary>
    ///     The number of ranked matches won.
    /// </summary>
    [PHPProperty("rnk_wins")]
    public required string RankedMatchesWon { get; set; }

    /// <summary>
    ///     The number of ranked matches lost.
    /// </summary>
    [PHPProperty("rnk_losses")]
    public required string RankedMatchesLost { get; set; }

    /// <summary>
    ///     The number of ranked matches played.
    /// </summary>
    [PHPProperty("rnk_games_played")]
    public required string RankedMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from ranked matches.
    /// </summary>
    [PHPProperty("rnk_discos")]
    public required string RankedMatchDisconnects { get; set; }

    /// <summary>
    ///     The casual MMR (match making rating) of the account.
    ///     Casual MMR only applies to casual ranked matches.
    /// </summary>
    [PHPProperty("cs_amm_team_rating")]
    public required string CasualMMR { get; set; }

    /// <summary>
    ///     The number of casual ranked matches won.
    /// </summary>
    [PHPProperty("cs_wins")]
    public required string CasualRankedMatchesWon { get; set; }

    /// <summary>
    ///     The number of casual ranked matches lost.
    /// </summary>
    [PHPProperty("cs_losses")]
    public required string CasualRankedMatchesLost { get; set; }

    /// <summary>
    ///     The number of casual ranked matches played.
    /// </summary>
    [PHPProperty("cs_games_played")]
    public required string CasualRankedMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from casual ranked matches.
    /// </summary>
    [PHPProperty("cs_discos")]
    public required string CasualRankedMatchDisconnects { get; set; }

    /// <summary>
    ///     The MidWars MMR (match making rating) of the account.
    ///     MidWars MMR only applies to ranked MidWars matches.
    /// </summary>
    [PHPProperty("mid_amm_team_rating")]
    public required string MidWarsMMR { get; set; }

    /// <summary>
    ///     The number of ranked MidWars matches played.
    /// </summary>
    [PHPProperty("mid_games_played")]
    public required string RankedMidWarsMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from ranked MidWars matches.
    /// </summary>
    [PHPProperty("mid_discos")]
    public required string RankedMidWarsMatchDisconnects { get; set; }

    /// <summary>
    ///     The RiftWars MMR (match making rating) of the account.
    ///     RiftWars MMR only applies to ranked RiftWars matches.
    /// </summary>
    [PHPProperty("rift_amm_team_rating")]
    public required string RiftWarsMMR { get; set; }

    /// <summary>
    ///     The number of ranked RiftWars matches played.
    /// </summary>
    [PHPProperty("rift_games_played")]
    public required string RankedRiftWarsMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from ranked RiftWars matches.
    /// </summary>
    [PHPProperty("rift_discos")]
    public required string RankedRiftWarsMatchDisconnects { get; set; }

    /// <summary>
    ///     The number of seasonal ranked matches played.
    /// </summary>
    [PHPProperty("cam_games_played")]
    public required int SeasonalRankedMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from seasonal ranked matches.
    /// </summary>
    [PHPProperty("cam_discos")]
    public required int SeasonalRankedMatchDisconnects { get; set; }

    /// <summary>
    ///     The number of casual seasonal ranked matches played.
    /// </summary>
    [PHPProperty("cam_cs_games_played")]
    public required int CasualSeasonalRankedMatchesPlayed { get; set; }

    /// <summary>
    ///     The number of disconnects from casual seasonal ranked matches.
    /// </summary>
    [PHPProperty("cam_cs_discos")]
    public required int CasualSeasonalRankedMatchDisconnects { get; set; }

    /// <summary>
    ///     Whether the referral system status of the friend is new or not.
    ///     <br />
    ///     0 = False; 1 = True
    /// </summary>
    [PHPProperty("is_new")]
    public int IsNew { get; set; } = 0;
}
