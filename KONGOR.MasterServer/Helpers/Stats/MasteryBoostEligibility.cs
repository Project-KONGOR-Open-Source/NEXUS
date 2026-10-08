namespace KONGOR.MasterServer.Helpers.Stats;

/// <summary>
///     Determines whether a mastery boost can be applied to a match, so that the match statistics response and the boost endpoint enforce the same rules.
/// </summary>
public static class MasteryBoostEligibility
{
    /// <summary>
    ///     Gets the reason why a mastery boost cannot be applied to the given participant's match, or <see langword="null"/> if a boost can be applied.
    ///     A boost can only be applied once, only to a match that awarded mastery experience, only to the account's most recent such match, and never to a hero at the maximum mastery level.
    /// </summary>
    public static async Task<MasteryBoostRejection?> GetRejection(MerrickContext databaseContext, Mastery mastery, MatchParticipantStatistics participant)
    {
        MasteryProgression? progression = participant.MasteryProgression;

        // Error Code 6 Matches The Original API's "Match Mastery Data Does Not Exist" Code, Because The Original API Recorded No Mastery Data For Matches That Award No Mastery Experience
        if (progression is null)
            return new MasteryBoostRejection(6, $"No Mastery Data Exists For Match ID {participant.MatchID}");

        // Error Code 4 Matches The Original API's "Match Already Boosted" Code
        if (progression.BoostExperience > 0 || progression.SuperBoostExperience > 0)
            return new MasteryBoostRejection(4, "A Hero Mastery Boost Has Already Been Applied To This Match");

        // Error Code 1 Matches The Original API's Generic Error Code, Which It Returned For A Hero At The Maximum Mastery Level
        if (mastery.GetHeroLevelByHeroIdentifier(participant.HeroIdentifier) >= Mastery.MaximumMasteryLevel)
            return new MasteryBoostRejection(1, "Hero Mastery Boosts Cannot Be Applied To A Hero At The Maximum Mastery Level");

        // Match IDs Are Not Chronological, So The Most Recent Mastery Match Is Resolved By The Recorded Timestamp, With The Participant Record As A Deterministic Tie-Breaker
        int mostRecentMasteryMatchID = await databaseContext.MatchParticipantStatistics
            .Where(statistics => statistics.AccountID == participant.AccountID && statistics.MasteryProgression != null)
            .Join(databaseContext.MatchStatistics, statistics => statistics.MatchID, match => match.MatchID, (statistics, match) => new { statistics.ID, statistics.MatchID, match.TimestampRecorded })
            .OrderByDescending(record => record.TimestampRecorded)
            .ThenByDescending(record => record.ID)
            .Select(record => record.MatchID)
            .FirstOrDefaultAsync();

        // Error Code 5 Matches The Original API's "Match Outdated" Code
        if (participant.MatchID != mostRecentMasteryMatchID)
            return new MasteryBoostRejection(5, "Hero Mastery Boosts May Only Be Applied To Your Most Recent Mastery Match");

        return null;
    }
}

/// <summary>
///     The reason why a mastery boost cannot be applied, expressed as an error code and message of the original client API.
/// </summary>
public record MasteryBoostRejection(int ErrorCode, string ErrorMessage);
