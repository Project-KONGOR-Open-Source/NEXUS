namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class LevelingAwardTooltip
{
    /// <summary>
    ///     Awarded for reaching hero levels 2 to 5.
    /// </summary>
    [PhpProperty("2-5")]
    public int TwoToFive { get; set; } = 6;

    /// <summary>
    ///     Awarded for reaching hero levels 6 to 10.
    /// </summary>
    [PhpProperty("6-10")]
    public int SixToTen { get; set; } = 12;

    /// <summary>
    ///     Awarded for reaching hero levels 11 to 15.
    /// </summary>
    [PhpProperty("11-15")]
    public int ElevenToFifteen { get; set; } = 18;
}