namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Locks down <see cref="MatchmakingMatch.CalculateMatchupPrediction"/>.
///     A logistic curve <c>1 / (1 + e^(-(legion - hellbourne) / scale))</c> with a default scale of 80.
/// </summary>
public sealed class MatchupPredictionTests
{
    /// <summary>
    ///     The production default for <see cref="MatchmakingSettings.LogisticPredictionScale"/>.
    /// </summary>
    private const double DefaultLogisticScale = 80.0;

    /// <summary>
    ///     Tolerance for the logistic comparison since prediction values are computed from <see cref="Math.Exp"/>.
    /// </summary>
    private const double LogisticTolerance = 0.001;

    /// <summary>
    ///     Logistic of +1 standard score: <c>1 / (1 + e^-1)</c>.
    /// </summary>
    private const double LogisticAtPlusOne = 0.7311;

    /// <summary>
    ///     Logistic of +2 standard scores: <c>1 / (1 + e^-2)</c>.
    /// </summary>
    private const double LogisticAtPlusTwo = 0.8808;

    /// <summary>
    ///     Logistic of −1 standard score: <c>1 / (1 + e^1)</c>.
    /// </summary>
    private const double LogisticAtMinusOne = 0.2689;

    [Test]
    public async Task Equal_Teams_Predicts_50_Percent()
    {
        double prediction = MatchmakingMatch.CalculateMatchupPrediction(legionTMR: MatchmakingTestBuilder.BaselineTMR, hellbourneTMR: MatchmakingTestBuilder.BaselineTMR);

        await Assert.That(Math.Abs(prediction - 0.5) < LogisticTolerance).IsTrue();
    }

    /// <summary>
    ///     Each row pairs a TMR offset (in units of <see cref="DefaultLogisticScale"/>) with the expected logistic output.
    ///     The offset is multiplied by the scale and added to Hellbourne's baseline rating.
    /// </summary>
    [Test]
    [Arguments(+1.0, LogisticAtPlusOne,  "Legion +1 Scale")]
    [Arguments(+2.0, LogisticAtPlusTwo,  "Legion +2 Scales")]
    [Arguments(-1.0, LogisticAtMinusOne, "Legion -1 Scale")]
    public async Task Legion_At_Multiple_Of_Default_Scale_Produces_Expected_Logistic(double offsetInScales, double expectedPrediction, string description)
    {
        double legionTMR = MatchmakingTestBuilder.BaselineTMR + offsetInScales * DefaultLogisticScale;

        double prediction = MatchmakingMatch.CalculateMatchupPrediction(legionTMR, hellbourneTMR: MatchmakingTestBuilder.BaselineTMR, scale: DefaultLogisticScale);

        await Assert.That(Math.Abs(prediction - expectedPrediction) < LogisticTolerance).IsTrue();
    }

    [Test]
    public async Task Tighter_Scale_Amplifies_The_Curve()
    {
        // Same TMR Difference Of +80, Halved Scale → Effectively Two Scales Apart, Prediction Climbs From 0.7311 To 0.8808
        const double TightScale = DefaultLogisticScale / 2.0;
        double legionTMR = MatchmakingTestBuilder.BaselineTMR + DefaultLogisticScale;

        double tight   = MatchmakingMatch.CalculateMatchupPrediction(legionTMR, hellbourneTMR: MatchmakingTestBuilder.BaselineTMR, scale: TightScale);
        double @default = MatchmakingMatch.CalculateMatchupPrediction(legionTMR, hellbourneTMR: MatchmakingTestBuilder.BaselineTMR, scale: DefaultLogisticScale);

        using (Assert.Multiple())
        {
            await Assert.That(tight).IsGreaterThan(@default);
            await Assert.That(Math.Abs(tight - LogisticAtPlusTwo) < LogisticTolerance).IsTrue();
        }
    }
}
