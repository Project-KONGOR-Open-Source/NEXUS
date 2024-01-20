namespace MERRICK.Database.Enumerations;

public enum AccountType
{
    // built-in types
    Disabled            = 0,    // prevented from logging into HoN
    Trial               = 1,    // has limited access to in-game functions
    ServerHost          = 2,    // has permissions to host game servers
    Normal              = 3,    // free-to-play account
    Legacy              = 4,    // purchased HoN during the beta
    Staff               = 5,    // has access to admin functions and can execute admin commands
    GameMaster          = 6,    // can suspend players
    MatchModerator      = 7,    // can spectate and moderate matches
    MatchCaster         = 8,    // can spectate matches

    // custom types
    Streamer            = 101,  // has top priority in the matchmaking queue
    VIP                 = 102   // has top priority in the matchmaking queue
}
