namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class MatchMastery
{
    public MatchMastery() { }

    public MatchMastery(string heroIdentifier, int currentMasteryExperience, int matchMasteryExperience,
        int bonusExperience)
    {
        HeroIdentifier = heroIdentifier;
        CurrentMasteryExperience = currentMasteryExperience;
        MatchMasteryExperience = matchMasteryExperience;
        MasteryExperienceBonus = bonusExperience;
        MasteryExperienceBoost = matchMasteryExperience + bonusExperience;
        MasteryExperienceHeroesBonus = bonusExperience;
        MasteryExperienceToBoost = (matchMasteryExperience + bonusExperience) * 2;
        MasteryExperienceCanBoost = true;
        MasteryExperienceCanSuperBoost = true;
        MasteryExperienceBoostProductIdentifier = 3609;
        MasteryExperienceSuperBoostProductIdentifier = 4605;
        MasteryExperienceSuperBoost = 0;
        MasteryExperienceMaximumLevelHeroesCount = 0;
        MasteryExperienceEventBonus = 0;
        MasteryExperienceBoostProductCount = 0;
        MasteryExperienceSuperBoostProductCount = 0;
    }
    // TODO: Set Missing Properties Once Database Entities Are Available

    //public class MatchMastery(MasteryRewards rewards)
    //{
    //    MasteryExperienceMaximumLevelHeroesCount = rewards.MasteryMaxLevelHeroesCount;
    //    MasteryExperienceBoostProductCount = rewards.MasteryBoostsOwned;
    //    MasteryExperienceSuperBoostProductCount = rewards.MasterySuperBoostsOwned;
    //}

    /// <summary>
    ///     The identifier of the hero, in the format Hero_{Snake_Case_Name}.
    /// </summary>
    [PhpProperty("cli_name")]
    public required string HeroIdentifier { get; init; }

    /// <summary>
    ///     The hero's original mastery experience before the match.
    ///     This is the current mastery level progress persisted to the database.
    /// </summary>
    [PhpProperty("mastery_exp_original")]
    public required int CurrentMasteryExperience { get; init; }

    /// <summary>
    ///     The base mastery experience earned during the match.
    ///     Calculated from match duration, map, match type, and win/loss status.
    ///     Does not include bonuses or boosts.
    /// </summary>
    [PhpProperty("mastery_exp_match")]
    public required int MatchMasteryExperience { get; init; }

    /// <summary>
    ///     Additional mastery experience bonus from map-specific multipliers.
    ///     Applied as a percentage multiplier to the base experience.
    /// </summary>
    [PhpProperty("mastery_exp_bonus")]
    public int MasteryExperienceBonus { get; init; }

    /// <summary>
    ///     The additional mastery experience gained from applying a regular mastery boost consumable.
    ///     Set to zero initially when match results are calculated.
    ///     Only populated with a non-zero value after the player applies a mastery boost product.
    /// </summary>
    [PhpProperty("mastery_exp_boost")]
    public int MasteryExperienceBoost { get; init; }

    /// <summary>
    ///     The additional mastery experience gained from applying a super mastery boost consumable.
    ///     Set to zero initially when match results are calculated.
    ///     Only populated with a non-zero value after the player applies a super mastery boost product.
    /// </summary>
    [PhpProperty("mastery_exp_super_boost")]
    public int MasteryExperienceSuperBoost { get; init; }

    /// <summary>
    ///     The number of heroes the account has reached maximum mastery level with.
    ///     Used to calculate the "max_heroes_addon" bonus multiplier.
    /// </summary>
    [PhpProperty("mastery_exp_heroes_count")]
    public required int MasteryExperienceMaximumLevelHeroesCount { get; init; }

    /// <summary>
    ///     Bonus mastery experience awarded based on the number of max-level heroes owned.
    ///     Maps to "mastery_maxlevel_addon" in "match_stats_v2.lua".
    /// </summary>
    [PhpProperty("mastery_exp_heroes_addon")]
    public required int MasteryExperienceHeroesBonus { get; init; }

    /// <summary>
    ///     The potential experience that can be gained by using a regular mastery boost.
    ///     Displayed when hovering over the mastery boost button in the UI.
    /// </summary>
    [PhpProperty("mastery_exp_to_boost")]
    public required int MasteryExperienceToBoost { get; init; }

    /// <summary>
    ///     Special event bonus mastery experience granted during promotional periods.
    ///     Typically zero unless an admin-configured mastery experience event is active.
    /// </summary>
    [PhpProperty("mastery_exp_event")]
    public int MasteryExperienceEventBonus { get; init; }

    /// <summary>
    ///     Setting this value to FALSE disables using or purchasing regular mastery boosts.
    ///     Some use cases for FALSE would be: 1) the hero has reached the maximum mastery level, 2) a mastery experience boost
    ///     has already been used, 3) the map/mode combination is not eligible for accumulating mastery experience.
    /// </summary>
    [PhpProperty("mastery_canboost")]
    public bool MasteryExperienceCanBoost { get; init; } = true;

    /// <summary>
    ///     Setting this value to FALSE disables using or purchasing super mastery boosts.
    ///     Some use cases for FALSE would be: 1) the hero has reached the maximum mastery level, 2) a mastery experience boost
    ///     has already been used, 3) the map/mode combination is not eligible for accumulating mastery experience.
    /// </summary>
    [PhpProperty("mastery_super_canboost")]
    public bool MasteryExperienceCanSuperBoost { get; init; } = true;

    /// <summary>
    ///     The product ID for regular mastery boost purchases (typically 3609 for "m.Mastery Boost").
    ///     Used when the player clicks to purchase a mastery boost from the match rewards screen.
    /// </summary>
    [PhpProperty("mastery_boost_product_id")]
    public int MasteryExperienceBoostProductIdentifier { get; init; } = 3609; // m.Mastery Boost

    /// <summary>
    ///     The product ID for super mastery boost purchases (typically 4605 for "m.Super boost").
    ///     Referenced but not directly purchasable from the standard match rewards UI.
    /// </summary>
    [PhpProperty("mastery_super_boost_product_id")]
    public int MasteryExperienceSuperBoostProductIdentifier { get; init; } = 4605; // m.Super boost

    /// <summary>
    ///     The number of regular mastery boost products the player currently owns.
    ///     Retrieved from the account's owned upgrades/products list.
    /// </summary>
    [PhpProperty("mastery_boostnum")]
    public required int MasteryExperienceBoostProductCount { get; init; }

    /// <summary>
    ///     The number of super mastery boost products the player currently owns.
    ///     Retrieved from the account's owned upgrades/products list.
    /// </summary>
    [PhpProperty("mastery_super_boostnum")]
    public required int MasteryExperienceSuperBoostProductCount { get; init; }
}