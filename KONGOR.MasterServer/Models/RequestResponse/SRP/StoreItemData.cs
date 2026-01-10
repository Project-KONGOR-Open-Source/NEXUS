namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class StoreItemData
{
    /// <summary>
    ///     Unknown.
    ///     Seems to be an empty string in all the network packet dumps.
    /// </summary>
    [PhpProperty("data")]
    public string Data { get; set; } = string.Empty;

    /// <summary>
    ///     The availability start time (in UTC seconds) of a limited-time store item (e.g. early-access hero).
    /// </summary>
    [PhpProperty("start_time")]
    public string AvailableFrom { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

    /// <summary>
    ///     The availability end time (in UTC seconds) of a limited-time store item (e.g. early-access hero).
    /// </summary>
    [PhpProperty("end_time")]
    public string AvailableUntil { get; set; } = DateTimeOffset.UtcNow.AddYears(1000).ToUnixTimeSeconds().ToString();

    /// <summary>
    ///     Unknown.
    ///     If "score" is not set, then this property is not set either.
    ///     But if "score" is set, then this property is set to "0".
    /// </summary>
    [PhpProperty("used")]
    public int Used { get; set; } = 0;

    /// <summary>
    ///     Used for numeric counters and visual effects for specific hero avatars.
    ///     This value represents the number of hero kills achieved with those respective avatars.
    ///     <br />
    ///     We don't currently keep track of these scores, so setting this value to "0" on every login will cause the counter
    ///     to be equal to the number of hero kills each game.
    /// </summary>
    [PhpProperty("score")]
    public string Score { get; set; } = "0";
}