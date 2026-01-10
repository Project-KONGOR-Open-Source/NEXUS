namespace KONGOR.MasterServer.Configuration.Economy;

public class EconomyConfiguration
{
    public required SignupRewards SignupRewards { get; set; }

    public required EventRewards EventRewards { get; set; }

    public required MatchRewards MatchRewards { get; set; }
}

public class SignupRewards
{
    public required int GoldCoins { get; set; }

    public required int SilverCoins { get; set; }

    public required int PlinkoTickets { get; set; }
}

public class EventRewards
{
    public required PostSignupBonus PostSignupBonus { get; set; }
}

public class PostSignupBonus
{
    public required int GoldCoins { get; set; }

    public required int SilverCoins { get; set; }

    public required int PlinkoTickets { get; set; }

    public required int MatchesCount { get; set; }
}

public class MatchRewards
{
    public required Solo Solo { get; set; }

    public required TwoPersonGroup TwoPersonGroup { get; set; }

    public required ThreePersonGroup ThreePersonGroup { get; set; }

    public required FourPersonGroup FourPersonGroup { get; set; }

    public required FivePersonGroup FivePersonGroup { get; set; }
}

public class Solo
{
    public required Win Win { get; set; }

    public required Loss Loss { get; set; }
}

public class TwoPersonGroup
{
    public required Win Win { get; set; }

    public required Loss Loss { get; set; }
}

public class ThreePersonGroup
{
    public required Win Win { get; set; }

    public required Loss Loss { get; set; }
}

public class FourPersonGroup
{
    public required Win Win { get; set; }

    public required Loss Loss { get; set; }
}

public class FivePersonGroup
{
    public required Win Win { get; set; }

    public required Loss Loss { get; set; }
}

public class Win
{
    public required int GoldCoins { get; set; }

    public required int SilverCoins { get; set; }

    public required int PlinkoTickets { get; set; }
}

public class Loss
{
    public required int GoldCoins { get; set; }

    public required int SilverCoins { get; set; }

    public required int PlinkoTickets { get; set; }
}