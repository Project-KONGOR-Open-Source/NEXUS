namespace ASPIRE.Common.Compatibility;

/// <summary>
///     The version of the game client and match server distributions, which is the only version NEXUS accepts.
/// </summary>
public static class Distributions
{
    // INFO: This Version Number Needs To Be Updated In Lock-Step With The Version Number Of The Distributions
    public const string Version = "5.26.10.31";

    /// <summary>
    ///     Turns a version carried as four bytes back into the distributions' text, with every part after the major version written as two digits.
    /// </summary>
    public static string FormatVersion(byte major, byte minor, byte patch, byte revision)
        => $"{major}.{minor:D2}.{patch:D2}.{revision:D2}";
}
