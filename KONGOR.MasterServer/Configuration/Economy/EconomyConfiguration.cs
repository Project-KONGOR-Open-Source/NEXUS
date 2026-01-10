namespace KONGOR.MasterServer.Configuration.Economy;

public class EconomyConfiguration
{
    public required SignupRewards SignupRewards { get; set; }

    public required EventRewards EventRewards { get; set; }

    public required MatchRewards MatchRewards { get; set; }
}