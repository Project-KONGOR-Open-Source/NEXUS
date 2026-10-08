namespace MERRICK.DatabaseContext.Entities.Statistics;

[Index(nameof(MatchID), nameof(AccountID), IsUnique = true)]
public class MatchParticipantStatistics
{
    [Key]
    public int ID { get; set; }

    public required int MatchID { get; set; }

    public required int AccountID { get; set; }

    [MaxLength(15)]
    public required string AccountName { get; set; }

    public required int? ClanID { get; set; }

    [MaxLength(4)]
    public required string? ClanTag { get; set; }

    public required int Team { get; set; }

    public required int LobbyPosition { get; set; }

    public required int GroupNumber { get; set; }

    public required int Benefit { get; set; }

    public required uint? HeroProductID { get => field is 0 ? null : field; set => field = value is 0 ? null : value; }

    public required string HeroIdentifier { get; set; }

    public string? AlternativeAvatarName { get; set; }

    public uint? AlternativeAvatarProductID { get; set; }

    public string? WardProductName { get; set; }

    public uint? WardProductID { get; set; }

    public string? TauntProductName { get; set; }

    public uint? TauntProductID { get; set; }

    public string? AnnouncerProductName { get; set; }

    public uint? AnnouncerProductID { get; set; }

    public string? CourierProductName { get; set; }

    public uint? CourierProductID { get; set; }

    public string? AccountIconProductName { get; set; }

    public uint? AccountIconProductID { get; set; }

    public string? ChatColourProductName { get; set; }

    public uint? ChatColourProductID { get; set; }

    public required List<string> Inventory { get; set; }

    public required int Win { get; set; }

    public required int Loss { get; set; }

    public required int Disconnected { get; set; }

    public required int Conceded { get; set; }

    public required int Kicked { get; set; }

    public required int PublicMatch { get; set; }

    public required double PublicSkillRatingChange { get; set; }

    public required int RankedMatch { get; set; }

    public required double RankedSkillRatingChange { get; set; }

    public required int SocialBonus { get; set; }

    public required int UsedToken { get; set; }

    public required int ConcedeVotes { get; set; }

    public required int HeroKills { get; set; }

    public required int HeroDamage { get; set; }

    public required int GoldFromHeroKills { get; set; }

    public required int HeroAssists { get; set; }

    public required int HeroExperience { get; set; }

    public required int HeroDeaths { get; set; }

    public required int Buybacks { get; set; }

    public required int GoldLostToDeath { get; set; }

    public required int SecondsDead { get; set; }

    public required int TeamCreepKills { get; set; }

    public required int TeamCreepDamage { get; set; }

    public required int TeamCreepGold { get; set; }

    public required int TeamCreepExperience { get; set; }

    public required int NeutralCreepKills { get; set; }

    public required int NeutralCreepDamage { get; set; }

    public required int NeutralCreepGold { get; set; }

    public required int NeutralCreepExperience { get; set; }

    public required int BuildingDamage { get; set; }

    public required int BuildingsRazed { get; set; }

    public required int ExperienceFromBuildings { get; set; }

    public required int GoldFromBuildings { get; set; }

    public required int Denies { get; set; }

    public required int ExperienceDenied { get; set; }

    public required int Gold { get; set; }

    public required int GoldSpent { get; set; }

    public required int Experience { get; set; }

    public required int Actions { get; set; }

    public required int SecondsPlayed { get; set; }

    public required int HeroLevel { get; set; }

    public required int ConsumablesPurchased { get; set; }

    public required int WardsPlaced { get; set; }

    public required int FirstBlood { get; set; }

    public required int DoubleKill { get; set; }

    public required int TripleKill { get; set; }

    public required int QuadKill { get; set; }

    public required int Annihilation { get; set; }

    public required int KillStreak03 { get; set; }

    public required int KillStreak04 { get; set; }

    public required int KillStreak05 { get; set; }

    public required int KillStreak06 { get; set; }

    public required int KillStreak07 { get; set; }

    public required int KillStreak08 { get; set; }

    public required int KillStreak09 { get; set; }

    public required int KillStreak10 { get; set; }

    public required int KillStreak15 { get; set; }

    public required int Smackdown { get; set; }

    public required int Humiliation { get; set; }

    public required int Nemesis { get; set; }

    public required int Retribution { get; set; }

    public required int Score { get; set; }

    public required double GameplayStat0 { get; set; }

    public required double GameplayStat1 { get; set; }

    public required double GameplayStat2 { get; set; }

    public required double GameplayStat3 { get; set; }

    public required double GameplayStat4 { get; set; }

    public required double GameplayStat5 { get; set; }

    public required double GameplayStat6 { get; set; }

    public required double GameplayStat7 { get; set; }

    public required double GameplayStat8 { get; set; }

    public required double GameplayStat9 { get; set; }

    public required int TimeEarningExperience { get; set; }

    public List<ItemEvent>? ItemHistory { get; set; }

    public List<AbilityEvent>? AbilityHistory { get; set; }

    /// <summary>
    ///     The mastery progression of the participant's hero in this match, persisted as a JSON column.
    ///     This is <see langword="null"/> for matches that awarded no mastery experience.
    /// </summary>
    public MasteryProgression? MasteryProgression { get; set; }
}

public class ItemEvent
{
    public required string ItemName { get; set; }

    public required int GameTimeSeconds { get; set; }

    public required byte EventType { get; set; }
}

public class AbilityEvent
{
    public required string HeroName { get; set; }

    public required string AbilityName { get; set; }

    public required int GameTimeSeconds { get; set; }

    public required byte SlotIndex { get; set; }
}

/// <summary>
///     The mastery progression of a hero in a single match, stored as part of the <see cref="MatchParticipantStatistics.MasteryProgression"/> JSON column.
///     Every component of the awarded experience is recorded, so that the experience after the match can be derived and the awarded experience can be audited if the formula changes.
/// </summary>
public class MasteryProgression
{
    /// <summary>
    ///     The hero's accumulated mastery experience before this match.
    /// </summary>
    public int ExperienceBeforeMatch { get; set; }

    /// <summary>
    ///     The base mastery experience awarded for this match.
    /// </summary>
    public int MatchExperience { get; set; }

    /// <summary>
    ///     The number of heroes at the maximum mastery level when this match was recorded, from which the bonus experience is calculated.
    /// </summary>
    public int HeroesAtMaximumMasteryCount { get; set; }

    /// <summary>
    ///     The bonus mastery experience awarded for this match.
    /// </summary>
    public int BonusExperience { get; set; }

    /// <summary>
    ///     The mastery experience added by a regular mastery boost applied to this match, or zero if none has been applied.
    /// </summary>
    public int BoostExperience { get; set; }

    /// <summary>
    ///     The mastery experience added by a super mastery boost applied to this match, or zero if none has been applied.
    /// </summary>
    public int SuperBoostExperience { get; set; }

    /// <summary>
    ///     The hero's mastery experience after this match, including any boost applied to it, capped at <see cref="Mastery.MaximumMasteryExperience"/>.
    /// </summary>
    public int ExperienceAfterMatch()
        => Math.Min(ExperienceBeforeMatch + MatchExperience + BonusExperience + BoostExperience + SuperBoostExperience, Mastery.MaximumMasteryExperience);
}
