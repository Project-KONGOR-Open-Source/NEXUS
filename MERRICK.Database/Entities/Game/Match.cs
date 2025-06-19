using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MERRICK.Database.Enumerations;

namespace MERRICK.Database.Entities.Game;

[Index(nameof(MatchID), IsUnique = true)]
public class Match
{
    [Key]
    public int ID { get; set; }

    public required int MatchID { get; set; }

    public required long ServerID { get; set; }

    [MaxLength(15)]
    public required string HostAccountName { get; set; }

    public required string Map { get; set; }

    [MaxLength(15)]
    public required string MapVersion { get; set; }

    public required string GameMode { get; set; }

    public required ArrangedMatchType MatchType { get; set; }

    public required string Region { get; set; }

    public required bool Ranked { get; set; }

    public required MatchStatus Status { get; set; }

    public required int AveragePSRTeamOne { get; set; }

    public required int AveragePSRTeamTwo { get; set; }

    // TODO: Add MMR fields when implemented
    // public required int AverageMMRTeamOne { get; set; }
    // public required int AverageMMRTeamTwo { get; set; }

    public required DateTime TimestampCreated { get; set; }

    public DateTime? TimestampStarted { get; set; }

    public DateTime? TimestampEnded { get; set; }

    public int? TimePlayed { get; set; }

    public int? ScoreTeam1 { get; set; }

    public int? ScoreTeam2 { get; set; }

    public string? BannedHeroes { get; set; }

    public List<MatchParticipant> Participants { get; set; } = [];
}

public class MatchParticipant
{
    [Key]
    public int ID { get; set; }

    public required int MatchID { get; set; }

    public required int AccountID { get; set; }

    [MaxLength(15)]
    public required string AccountName { get; set; }

    public required int? ClanID { get; set; }

    [MaxLength(4)]
    public required string? ClanTag { get; set; }

    public required int Team { get; set; }

    public required int LobbyPosition { get; set; }

    public required int GroupNumber { get; set; }

    public required bool IsReady { get; set; }

    public required bool HasJoined { get; set; }

    public required bool HasDisconnected { get; set; }

    public Match Match { get; set; } = null!;
}

public enum MatchStatus
{
    Created = 0,
    WaitingForPlayers = 1,
    Loading = 2,
    InProgress = 3,
    Completed = 4,
    Abandoned = 5,
    Failed = 6
}
