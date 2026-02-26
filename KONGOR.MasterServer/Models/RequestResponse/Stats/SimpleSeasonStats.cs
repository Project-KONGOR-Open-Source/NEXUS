namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class SimpleSeasonStats
{
    [PHPProperty("wins")]
    public required int RankedMatchesWon { get; set; }

    [PHPProperty("losses")]
    public required int RankedMatchesLost { get; set; }
    
    [PHPProperty("h_wins")]
    public int HardcoreWins { get; set; }
    
    [PHPProperty("h_losses")]
    public int HardcoreLosses { get; set; }

    [PHPProperty("con_wins")]
    public int CasualWins { get; set; }
    
    [PHPProperty("con_losses")]
    public int CasualLosses { get; set; }

    [PHPProperty("win_streak")]
    public required int WinStreak { get; set; }

    [PHPProperty("rank")]
    public required string CurrentRank { get; set; }

    [PHPProperty("current_level")]
    public string LegacyLevel => CurrentRank;

    /// <summary>
    ///     The current matchmaking rating (MMR).
    ///     References `playerstats_mmr` in Lua (Index 14).
    /// </summary>
    [PHPProperty("smr")]
    public required string RankedRating { get; set; }

    /// <summary>
    ///     The highest rank achieved in the season.
    /// </summary>
    [PHPProperty("highest_level_current")]
    public string? HighestRank { get; set; }

    [PHPProperty("is_placement")]
    public required int InPlacementPhase { get; set; }
    
    [PHPProperty("pub_skill")]
    public string PublicRating { get; set; } = "1500.000";

    [PHPProperty("kam_rating")]
    public string CasualRating { get; set; } = "1500.000";

    // Logan (2025-02-13): Legacy Alias Mappings for Lua
    // Lua expects 'rnk_' prefix for Normal mode and 'cs_' for Casual.
    
    [PHPProperty("rnk_wins")]
    public int RnkWins => RankedMatchesWon;
    
    [PHPProperty("rnk_losses")]
    public int RnkLosses => RankedMatchesLost;
    
    [PHPProperty("rnk_games_played")]
    public int RnkGamesPlayed => RankedMatchesWon + RankedMatchesLost;
    
    [PHPProperty("cs_wins")]
    public int CsWins => CasualWins;
    
    [PHPProperty("cs_losses")]
    public int CsLosses => CasualLosses;
    
    [PHPProperty("cs_games_played")]
    public int CsGamesPlayed => CasualWins + CasualLosses; // Sum of wins and losses
    
    [PHPProperty("acc_wins")] // For Public/Summary tab mapping if used
    public int AccWins => RankedMatchesWon; 
    
    [PHPProperty("acc_games_played")]
    public int AccGamesPlayed => RankedMatchesWon + RankedMatchesLost; // Fallback

    // Helper to hold raw stats for legacy mapping
    // Internal to avoid PHP serialization
    internal PlayerStatisticsAggregatedDTO? GenericStats { get; set; }
}
