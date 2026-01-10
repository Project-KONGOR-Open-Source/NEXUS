using System.Collections.Frozen;

namespace KONGOR.MasterServer.Constants;

public static class MatchModeDefinition
{
    // Mapping from Legacy/HON Server MatchMode ID (int) to Client String Code
    private static readonly FrozenDictionary<int, string> LegacyModeIdToCode = new Dictionary<int, string>
    {
        { 1, "ap" }, // All Pick (Normal Mode)
        { 2, "sd" }, // Single Draft
        { 3, "rd" }, // Random Draft
        { 4, "bd" }, // Banning Draft
        { 5, "bp" }, // Banning Pick
        { 6, "cd" }, // Captains Draft
        { 7, "cm" }, // Captains Mode
        { 8, "br" }, // Balanced Random
        { 9, "cp" }, // Campaign Mode
        { 10, "sm" }, // Solo Diff Mode (1v1) - *Hypothetical ID, verify if possible*
        { 11, "ss" }, // Solo Same Mode (1v1)
        { 12, "hb" }, // Hero Ban Mode
        { 13, "mwb" }, // MidWars Beta Mode
        // Hypothetical IDs - IDs must be verified against game server protocol
        { 14, "lp" }, // Lockpick
        { 15, "bb" }, // Blind Ban
        { 16, "bm" }, // Bot Match
        { 17, "sp" }, // Shuffle Pick (Code Guess: sp)
        { 18, "rp" } // Ranked Pick (Code Guess: rp)
        // "nm" -> Normal Mode usually maps to "ap" in HoN context or specific ID 0/1 depending on protocol.
        // Assuming 1 = AP based on standard HoN logic.
    }.ToFrozenDictionary();

    public static string GetCodeFromId(int id)
    {
        return LegacyModeIdToCode.TryGetValue(id, out string? code) ? code : id.ToString();
    }
}