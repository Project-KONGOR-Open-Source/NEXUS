namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

/// <summary>
///     Response model for the "get_seasons" request.
/// </summary>
public class GetSeasonsResponse
{
    /// <summary>
    ///     A pipe-delimited string of all available seasons and their modes in the format "season_number,mode_type|season_number,mode_type".
    /// </summary>
    /// <remarks>
    ///     Mode Type: 0 = Normal, 1 = Casual
    /// </remarks>
    [PHPProperty("all_seasons")]
    public required string AllSeasons { get; set; }

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
