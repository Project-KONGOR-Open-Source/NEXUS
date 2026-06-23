namespace KONGOR.MasterServer.Models.Configuration;

public class OperationalConfiguration
{
    public const string ConfigurationSection = "Operational";

    public required OperationalConfigurationCDN CDN { get; set; }

    public required OperationalConfigurationQuest Quest { get; set; }
}

public class OperationalConfigurationCDN
{
    public required string Host { get; set; }

    public required string PrimaryPatchURL { get; set; }

    public required string SecondaryPatchURL { get; set; }
}

public class OperationalConfigurationQuest
{
    /// <summary>
    ///     The availability of the quest system, mirroring the game client's "EQuestsAvailabilityType" enumeration.
    ///     A value of "0" disables the quest system; a value of "1" marks it as live.
    /// </summary>
    public required int SystemStatus { get; set; }

    /// <summary>
    ///     The availability of the quest leaderboard, mirroring the game client's "EQuestsAvailabilityType" enumeration.
    ///     A value of "0" disables the quest leaderboard; a value of "1" marks it as live.
    /// </summary>
    public required int LeaderboardStatus { get; set; }
}
