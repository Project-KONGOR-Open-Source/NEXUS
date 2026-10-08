namespace MERRICK.DatabaseContext.Entities.Statistics;

/// <summary>
///     Per-account hero mastery progression.
///     The accumulated experience for each played hero is held in <see cref="HeroExperiences"/> (a JSON column). Heroes with no experience are simply absent.
///     The mastery level for a hero is derived from its experience via <see cref="GetLevelFromExperience"/>.
/// </summary>
/// <remarks>
///     <para>
///         The experience thresholds for each of the sixteen mastery levels (level 0 through level 15) are:
///         level 0 = 0, level 1 = 1400, level 2 = 3000, level 3 = 5000, level 4 = 7000, level 5 = 9100, level 6 = 11300, level 7 = 13600, level 8 = 16000, level 9 = 18500, level 10 = 21000, level 11 = 23600, level 12 = 26400, level 13 = 29400, level 14 = 32600, level 15 = 36100.
///     </para>
///     <para>
///         Match experience for ranked normal matchmaking is the hero level multiplied by twenty, while ranked casual matchmaking and MidWars award the hero level multiplied by ten.
///         Each hero at the maximum mastery level adds one point of bonus experience (half, rounded up, for the reduced-experience game modes), but the bonus never exceeds half of the match experience.
///         On average, the maximum mastery level is achieved in roughly one hundred full-experience matches or two hundred reduced-experience matches.
///     </para>
/// </remarks>
[Index(nameof(AccountID), IsUnique = true)]
public class Mastery
{
    [Key]
    public int ID { get; set; }

    public int AccountID { get; set; }

    [ForeignKey(nameof(AccountID))]
    public required Account Account { get; set; }

    /// <summary>
    ///     The accumulated mastery experience for each hero the account has progressed, keyed by hero identifier (for example "Hero_Accursed").
    ///     Persisted as a JSON column. Heroes with no accumulated experience are not stored.
    /// </summary>
    public List<HeroMasteryExperience> HeroExperiences { get; set; } = [];

    /// <summary>
    ///     The current maximum mastery level for a single hero.
    /// </summary>
    public const int MaximumMasteryLevel = 15;

    /// <summary>
    ///     The experience threshold of the maximum mastery level, at which a hero's mastery experience is capped.
    /// </summary>
    public const int MaximumMasteryExperience = 36100;

    /// <summary>
    ///     The mastery experience accumulated for the hero with the given identifier (for example "Hero_Accursed"), or zero if the hero has no accumulated experience.
    /// </summary>
    public int GetHeroExperienceByHeroIdentifier(string identifier)
        => HeroExperiences.SingleOrDefault(entry => entry.HeroIdentifier.Equals(identifier))?.Experience ?? 0;

    /// <summary>
    ///     The mastery level reached for the hero with the given identifier (for example "Hero_Accursed").
    /// </summary>
    public int GetHeroLevelByHeroIdentifier(string identifier)
        => GetLevelFromExperience(GetHeroExperienceByHeroIdentifier(identifier));

    /// <summary>
    ///     Sets the mastery experience accumulated for the hero with the given identifier (for example "Hero_Accursed"), creating the entry if the hero has not been progressed before.
    ///     The experience is capped at <see cref="MaximumMasteryExperience"/>, because a hero cannot progress beyond the maximum mastery level.
    /// </summary>
    public void SetHeroExperienceByHeroIdentifier(string identifier, int experience)
    {
        int cappedExperience = Math.Min(experience, MaximumMasteryExperience);

        HeroMasteryExperience? entry = HeroExperiences.SingleOrDefault(entry => entry.HeroIdentifier.Equals(identifier));

        if (entry is null)
            HeroExperiences.Add(new HeroMasteryExperience { HeroIdentifier = identifier, Experience = cappedExperience });

        else
            entry.Experience = cappedExperience;
    }

    /// <summary>
    ///     The sum of the mastery levels across all heroes.
    /// </summary>
    public int TotalMasteryLevel() => HeroExperiences.Sum(entry => GetLevelFromExperience(entry.Experience));

    /// <summary>
    ///     The sum of the mastery experience across all heroes.
    /// </summary>
    public int TotalMasteryExperience() => HeroExperiences.Sum(entry => entry.Experience);

    /// <summary>
    ///     The number of heroes that have reached the maximum mastery level.
    /// </summary>
    public int HeroesAtMaximumMasteryCount() => HeroExperiences.Count(entry => GetLevelFromExperience(entry.Experience) == MaximumMasteryLevel);

    /// <summary>
    ///     The base mastery experience awarded for a single match of the given type, scaled by the hero level reached during the match.
    /// </summary>
    public static int CalculateMatchExperience(AccountStatisticsType type, int heroLevel)
    {
        const int factor = 20;

        return type switch
        {
            AccountStatisticsType.Matchmaking       => heroLevel * factor,
            AccountStatisticsType.MatchmakingCasual => heroLevel * (factor / 2),
            AccountStatisticsType.MidWars           => heroLevel * (factor / 2),
            _                                       => 0
        };
    }

    /// <summary>
    ///     The bonus mastery experience awarded for a single match, which is one point for each hero at the maximum mastery level.
    ///     Ranked casual matchmaking and MidWars award half of the bonus, rounded up, in line with their halved match experience.
    ///     The bonus never exceeds half of the match experience, so that a short match cannot earn more from the bonus than from playing.
    /// </summary>
    public static int CalculateBonusExperience(AccountStatisticsType type, int matchExperience, int heroesAtMaximumMasteryCount)
    {
        int experience = type switch
        {
            AccountStatisticsType.Matchmaking       => heroesAtMaximumMasteryCount,
            AccountStatisticsType.MatchmakingCasual => (heroesAtMaximumMasteryCount + 1) / 2,
            AccountStatisticsType.MidWars           => (heroesAtMaximumMasteryCount + 1) / 2,
            _                                       => 0
        };

        return Math.Min(experience, matchExperience / 2);
    }

    /// <summary>
    ///     The mastery experience awarded by a regular mastery boost, which is double the combined match and bonus experience of the boosted match.
    /// </summary>
    public static int CalculateRegularMasteryBoostExperience(int matchExperience, int bonusExperience) => (matchExperience + bonusExperience) * 2;

    /// <summary>
    ///     The mastery level for the given mastery experience.
    /// </summary>
    public static int GetLevelFromExperience(int experience) => experience switch
    {
         < 01400                =>  00,
        >= 01400  and  < 03000  =>  01,
        >= 03000  and  < 05000  =>  02,
        >= 05000  and  < 07000  =>  03,
        >= 07000  and  < 09100  =>  04,
        >= 09100  and  < 11300  =>  05,
        >= 11300  and  < 13600  =>  06,
        >= 13600  and  < 16000  =>  07,
        >= 16000  and  < 18500  =>  08,
        >= 18500  and  < 21000  =>  09,
        >= 21000  and  < 23600  =>  10,
        >= 23600  and  < 26400  =>  11,
        >= 26400  and  < 29400  =>  12,
        >= 29400  and  < 32600  =>  13,
        >= 32600  and  < 36100  =>  14,
        >= 36100                =>  15
    };

    /// <summary>
    ///     The experience threshold at the start of the mastery level for the given mastery experience.
    /// </summary>
    public static int GetLowerLevelBoundaryFromExperience(int experience) => experience switch
    {
         < 01400                =>  00000,
        >= 01400  and  < 03000  =>  01400,
        >= 03000  and  < 05000  =>  03000,
        >= 05000  and  < 07000  =>  05000,
        >= 07000  and  < 09100  =>  07000,
        >= 09100  and  < 11300  =>  09100,
        >= 11300  and  < 13600  =>  11300,
        >= 13600  and  < 16000  =>  13600,
        >= 16000  and  < 18500  =>  16000,
        >= 18500  and  < 21000  =>  18500,
        >= 21000  and  < 23600  =>  21000,
        >= 23600  and  < 26400  =>  23600,
        >= 26400  and  < 29400  =>  26400,
        >= 29400  and  < 32600  =>  29400,
        >= 32600  and  < 36100  =>  32600,
        >= 36100                =>  36100
    };

    /// <summary>
    ///     The experience threshold at the start of the next mastery level for the given mastery experience.
    /// </summary>
    public static int GetUpperLevelBoundaryFromExperience(int experience) => experience switch
    {
         < 01400                =>  01400,
        >= 01400  and  < 03000  =>  03000,
        >= 03000  and  < 05000  =>  05000,
        >= 05000  and  < 07000  =>  07000,
        >= 07000  and  < 09100  =>  09100,
        >= 09100  and  < 11300  =>  11300,
        >= 11300  and  < 13600  =>  13600,
        >= 13600  and  < 16000  =>  16000,
        >= 16000  and  < 18500  =>  18500,
        >= 18500  and  < 21000  =>  21000,
        >= 21000  and  < 23600  =>  23600,
        >= 23600  and  < 26400  =>  26400,
        >= 26400  and  < 29400  =>  29400,
        >= 29400  and  < 32600  =>  32600,
        >= 32600  and  < 36100  =>  36100,
        >= 36100                =>  36100
    };
}

/// <summary>
///     The accumulated mastery experience for a single hero, stored as part of the <see cref="Mastery.HeroExperiences"/> JSON column.
/// </summary>
public class HeroMasteryExperience
{
    /// <summary>
    ///     The hero identifier, in the format "Hero_{Name}" (for example "Hero_Accursed").
    /// </summary>
    public required string HeroIdentifier { get; set; }

    public int Experience { get; set; }
}
