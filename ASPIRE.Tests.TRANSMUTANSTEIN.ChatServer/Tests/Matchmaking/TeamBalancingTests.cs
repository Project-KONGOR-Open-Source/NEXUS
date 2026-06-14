namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Locks down <see cref="MatchmakingAlgorithm.BalanceTeams"/>, the first-pass team balancing that swaps same-sized groups between two paired teams to bring the matchup prediction closer to an even 50/50.
/// </summary>
public sealed class TeamBalancingTests
{
    private const double PredictionScale = 80.0;

    [Test]
    public async Task Balancing_A_Lopsided_Pair_Brings_The_Matchup_Closer_To_Even()
    {
        // Legion Holds Four Strong Solos And One Weak Solo, Hellbourne Holds The Mirror; Swapping A Strong Solo For A Weak One Evens The Split

        MatchmakingTeam legionTeam = MatchmakingTeam.FromGroups(
        [
            MatchmakingTestBuilder.BuildSoloGroup(2000.0),
            MatchmakingTestBuilder.BuildSoloGroup(2000.0),
            MatchmakingTestBuilder.BuildSoloGroup(2000.0),
            MatchmakingTestBuilder.BuildSoloGroup(2000.0),
            MatchmakingTestBuilder.BuildSoloGroup(1000.0)
        ], teamSize: 5);

        MatchmakingTeam hellbourneTeam = MatchmakingTeam.FromGroups(
        [
            MatchmakingTestBuilder.BuildSoloGroup(2000.0),
            MatchmakingTestBuilder.BuildSoloGroup(1000.0),
            MatchmakingTestBuilder.BuildSoloGroup(1000.0),
            MatchmakingTestBuilder.BuildSoloGroup(1000.0),
            MatchmakingTestBuilder.BuildSoloGroup(1000.0)
        ], teamSize: 5);

        double distanceBefore = Math.Abs(MatchmakingMatch.CalculateMatchupPrediction(legionTeam.EffectiveTeamRating, hellbourneTeam.EffectiveTeamRating, PredictionScale) - 0.5);

        double[] combinedRatingsBefore = [.. legionTeam.GetAllMembers().Concat(hellbourneTeam.GetAllMembers()).Select(member => member.TMR).OrderBy(tmr => tmr)];

        MatchmakingAlgorithm.BalanceTeams(legionTeam, hellbourneTeam, PredictionScale);

        double distanceAfter = Math.Abs(MatchmakingMatch.CalculateMatchupPrediction(legionTeam.EffectiveTeamRating, hellbourneTeam.EffectiveTeamRating, PredictionScale) - 0.5);

        double[] combinedRatingsAfter = [.. legionTeam.GetAllMembers().Concat(hellbourneTeam.GetAllMembers()).Select(member => member.TMR).OrderBy(tmr => tmr)];

        using (Assert.Multiple())
        {
            // The Matchup Is Strictly More Even After Balancing
            await Assert.That(distanceAfter).IsLessThan(distanceBefore);

            // Balancing Only Repartitions Players; It Never Adds, Drops, Or Duplicates Any
            await Assert.That(legionTeam.PlayerCount).IsEqualTo(5);
            await Assert.That(hellbourneTeam.PlayerCount).IsEqualTo(5);
            await Assert.That(combinedRatingsAfter.SequenceEqual(combinedRatingsBefore)).IsTrue();
        }
    }

    [Test]
    public async Task Balancing_An_Already_Even_Pair_Does_Not_Make_It_Worse()
    {
        // Two Identically-Composed Teams Are Already At A 50/50 Prediction; Balancing Must Not Push Them Away From It

        MatchmakingTeam legionTeam = MatchmakingTeam.FromGroups(
        [
            MatchmakingTestBuilder.BuildGroup([1700.0, 1700.0]),
            MatchmakingTestBuilder.BuildGroup([1300.0, 1300.0]),
            MatchmakingTestBuilder.BuildSoloGroup(1500.0)
        ], teamSize: 5);

        MatchmakingTeam hellbourneTeam = MatchmakingTeam.FromGroups(
        [
            MatchmakingTestBuilder.BuildGroup([1700.0, 1700.0]),
            MatchmakingTestBuilder.BuildGroup([1300.0, 1300.0]),
            MatchmakingTestBuilder.BuildSoloGroup(1500.0)
        ], teamSize: 5);

        double distanceBefore = Math.Abs(MatchmakingMatch.CalculateMatchupPrediction(legionTeam.EffectiveTeamRating, hellbourneTeam.EffectiveTeamRating, PredictionScale) - 0.5);

        MatchmakingAlgorithm.BalanceTeams(legionTeam, hellbourneTeam, PredictionScale);

        double distanceAfter = Math.Abs(MatchmakingMatch.CalculateMatchupPrediction(legionTeam.EffectiveTeamRating, hellbourneTeam.EffectiveTeamRating, PredictionScale) - 0.5);

        using (Assert.Multiple())
        {
            await Assert.That(distanceAfter).IsLessThanOrEqualTo(distanceBefore);
            await Assert.That(legionTeam.PlayerCount).IsEqualTo(5);
            await Assert.That(hellbourneTeam.PlayerCount).IsEqualTo(5);
        }
    }
}
