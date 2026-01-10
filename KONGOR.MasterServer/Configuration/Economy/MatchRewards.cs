namespace KONGOR.MasterServer.Configuration.Economy;

public class MatchRewards
{
    public required Solo Solo { get; set; }

    public required TwoPersonGroup TwoPersonGroup { get; set; }

    public required ThreePersonGroup ThreePersonGroup { get; set; }

    public required FourPersonGroup FourPersonGroup { get; set; }

    public required FivePersonGroup FivePersonGroup { get; set; }
}