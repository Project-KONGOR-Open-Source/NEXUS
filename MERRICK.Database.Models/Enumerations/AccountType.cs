namespace MERRICK.Database.Models.Enumerations;

public enum AccountType
{
    Disabled = 0,   // prevented from logging into HON
    Normal = 3,     // free-to-play account
    Legacy = 4,     // purchased HON during the beta
    Staff = 5,      // has access to admin functions and can execute admin commands

    Streamer = 101, // has top priority in the matchmaking queue
    VIP = 102,      // has top priority in the matchmaking queue
    Caster = 100    // has the "Staff Spectate" permission
}
