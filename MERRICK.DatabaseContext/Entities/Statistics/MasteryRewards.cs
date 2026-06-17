namespace MERRICK.DatabaseContext.Entities.Statistics;

/// <summary>
///     Tracks which total mastery level reward tiers an account has already claimed, held as a JSON column of claimed levels.
///     The reward content for each tier is defined in the mastery rewards configuration. This entity records only which tiers have been claimed.
/// </summary>
[Index(nameof(AccountID), IsUnique = true)]
public class MasteryRewards
{
    [Key]
    public int ID { get; set; }

    public int AccountID { get; set; }

    [ForeignKey(nameof(AccountID))]
    public required Account Account { get; set; }

    /// <summary>
    ///     The total mastery levels whose reward tier has been claimed.
    /// </summary>
    public List<int> ClaimedLevels { get; set; } = [];

    /// <summary>
    ///     Whether the reward tier for the given total mastery level has already been claimed.
    /// </summary>
    public bool HasObtained(int level) => ClaimedLevels.Contains(level);

    /// <summary>
    ///     Marks the reward tier for the given total mastery level as claimed.
    /// </summary>
    /// <returns>
    ///     <see langword="true"/> if the tier was newly marked as claimed, or <see langword="false"/> if it had already been claimed.
    /// </returns>
    public bool MarkObtained(int level)
    {
        if (ClaimedLevels.Contains(level))
            return false;

        ClaimedLevels.Add(level);

        return true;
    }
}
