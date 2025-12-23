namespace KINESIS.Matchmaking;

public enum MatchmakingFailedToJoinReason
{
    Leaver, //	Group has a Leaver
    Disabled, //	Matchmaking is disabLed
    Busy, //	Matchmaking is fuLL
    OptionUnavailable, //	An option seLected is currentLy unavaiLabLe
    InvalidVersion, //	CLient's version is out of date
    GroupFull, //	The group you're trying to join is fuLL
    BadStats, //	UnabLe to retrieve pLayer's stats
    AlreadyQueued, //	The group you're trying to join is in gueue
    Trial, //	TriaL accounts aren't aLLowed to pLay matchmaking (deprecated)
    Banned, //	You're currentLy banned from matchmaking
    LobbyFull,
    WrongPassword,
    CampaignNotEligible
}
