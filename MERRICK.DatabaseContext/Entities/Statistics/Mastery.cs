namespace MERRICK.DatabaseContext.Entities.Statistics;

/// <summary>
///     Per-account hero mastery progression.
///     Each hero has an experience column, and the corresponding mastery level is derived from that experience via <see cref="GetLevelFromExperience"/>.
/// </summary>
/// <remarks>
///     <para>
///         The experience thresholds for each of the sixteen mastery levels (level 0 through level 15) are:
///         level 0 = 0, level 1 = 1400, level 2 = 3000, level 3 = 5000, level 4 = 7000, level 5 = 9100, level 6 = 11300, level 7 = 13600, level 8 = 16000, level 9 = 18500, level 10 = 21000, level 11 = 23600, level 12 = 26400, level 13 = 29400, level 14 = 32600, level 15 = 36100.
///     </para>
///     <para>
///         Match experience for ranked normal matchmaking is the hero level multiplied by twenty, while ranked casual matchmaking and MidWars award the hero level multiplied by ten.
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
    ///     The current maximum mastery level for a single hero.
    /// </summary>
    public const int MaximumMasteryLevel = 15;

    /// <summary>
    ///     The sum of the mastery levels across all heroes.
    /// </summary>
    public int TotalMasteryLevel() => GetType().GetProperties().Where(property => property.Name.EndsWith("Level", StringComparison.InvariantCultureIgnoreCase))
        .Select(property => Convert.ToInt32(property.GetValue(this))).Sum();

    /// <summary>
    ///     The sum of the mastery experience across all heroes.
    /// </summary>
    public int TotalMasteryExperience() => GetType().GetProperties().Where(property => property.Name.EndsWith("XP", StringComparison.InvariantCultureIgnoreCase))
        .Select(property => Convert.ToInt32(property.GetValue(this))).Sum();

    /// <summary>
    ///     The number of heroes that have reached the maximum mastery level.
    /// </summary>
    public int HeroesAtMaximumMasteryCount() => GetType().GetProperties().Where(property => property.Name.EndsWith("Level", StringComparison.InvariantCultureIgnoreCase))
        .Count(property => Convert.ToInt32(property.GetValue(this)) == MaximumMasteryLevel);

    /// <summary>
    ///     The percentage of heroes that have reached the maximum mastery level.
    /// </summary>
    public float HeroesAtMaximumMasteryPercentage()
        => Convert.ToSingle(HeroesAtMaximumMasteryCount()) / Convert.ToSingle(GetType().GetProperties().Count(property => property.Name.EndsWith("Level", StringComparison.InvariantCultureIgnoreCase))) * 100f;

    /// <summary>
    ///     The base mastery experience awarded for a single match of the given type, scaled by the hero level reached during the match.
    /// </summary>
    public int CalculateMatchExperience(AccountStatisticsType type, int heroLevel) => type switch
    {
        AccountStatisticsType.Matchmaking        =>  heroLevel * 20,
        AccountStatisticsType.MatchmakingCasual  =>  heroLevel * 10,
        AccountStatisticsType.MidWars            =>  heroLevel * 10,
        _                                        =>  0
    };

    /// <summary>
    ///     The bonus mastery experience awarded for a single match, scaled by the percentage of heroes already at the maximum mastery level.
    ///     Ranked casual matchmaking and MidWars award half of the bonus that ranked normal matchmaking awards.
    /// </summary>
    public int CalculateBonusExperience(AccountStatisticsType type)
    {
        int experience = HeroesAtMaximumMasteryPercentage() switch
        {
            float.NaN         =>  000,
             < 05             =>  000,
            >= 05  and  < 10  =>  010,
            >= 10  and  < 20  =>  020,
            >= 20  and  < 30  =>  030,
            >= 30  and  < 40  =>  040,
            >= 40  and  < 50  =>  050,
            >= 50  and  < 60  =>  060,
            >= 60  and  < 70  =>  070,
            >= 70  and  < 80  =>  080,
            >= 80  and  < 90  =>  090,
            >= 90             =>  100
        };

        return type switch
        {
            AccountStatisticsType.Matchmaking        =>  experience,
            AccountStatisticsType.MatchmakingCasual  =>  experience / 2,
            AccountStatisticsType.MidWars            =>  experience / 2,
            _                                        =>  0
        };
    }

    /// <summary>
    ///     The combined base and bonus mastery experience awarded for a single match.
    /// </summary>
    public int CalculateMatchAndBonusExperience(AccountStatisticsType type, int heroLevel) => CalculateMatchExperience(type, heroLevel) + CalculateBonusExperience(type);

    /// <summary>
    ///     The mastery experience awarded by a regular mastery boost, which is double the combined base and bonus match experience.
    /// </summary>
    public int CalculateRegularMasteryBoostExperience(AccountStatisticsType type, int heroLevel) => CalculateMatchAndBonusExperience(type, heroLevel) * 2;

    /// <summary>
    ///     The mastery experience accumulated for the hero with the given identifier (for example "Hero_Accursed").
    /// </summary>
    public int GetHeroExperienceByHeroIdentifier(string identifier) => (GetType().GetProperties().SingleOrDefault(property => property.Name == $"{identifier}_XP")?.GetValue(this) as int?).GetValueOrDefault();

    /// <summary>
    ///     The mastery level reached for the hero with the given identifier (for example "Hero_Accursed").
    /// </summary>
    public int GetHeroLevelByHeroIdentifier(string identifier) => (GetType().GetProperties().SingleOrDefault(property => property.Name == $"{identifier}_Level")?.GetValue(this) as int?).GetValueOrDefault();

    /// <summary>
    ///     Sets the mastery experience accumulated for the hero with the given identifier (for example "Hero_Accursed").
    /// </summary>
    /// <returns>
    ///     <see langword="true"/> if the hero identifier was recognised and the experience was set, otherwise <see langword="false"/>.
    /// </returns>
    public bool SetHeroExperienceByHeroIdentifier(string identifier, int experience)
    {
        PropertyInfo? property = GetType().GetProperties().SingleOrDefault(property => property.Name == $"{identifier}_XP");

        if (property is null)
            return false;

        property.SetValue(this, experience);

        return true;
    }

    /// <summary>
    ///     The mastery experience for every hero, keyed by hero identifier (for example "Hero_Accursed").
    /// </summary>
    public Dictionary<string, int> GetAllMasteriesInfo()
        => GetType().GetProperties().Where(property => property.Name.EndsWith("XP", StringComparison.InvariantCultureIgnoreCase))
            .ToDictionary(property => property.Name.Replace("_XP", string.Empty), property => Convert.ToInt32(property.GetValue(this)));

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

    public int Hero_Accursed_XP { get; set; }

    [NotMapped]
    public int Hero_Accursed_Level => GetLevelFromExperience(Hero_Accursed_XP);

    public int Hero_Adrenaline_XP { get; set; }

    [NotMapped]
    public int Hero_Adrenaline_Level => GetLevelFromExperience(Hero_Adrenaline_XP);

    public int Hero_Aluna_XP { get; set; }

    [NotMapped]
    public int Hero_Aluna_Level => GetLevelFromExperience(Hero_Aluna_XP);

    public int Hero_Andromeda_XP { get; set; }

    [NotMapped]
    public int Hero_Andromeda_Level => GetLevelFromExperience(Hero_Andromeda_XP);

    public int Hero_Apex_XP { get; set; }

    [NotMapped]
    public int Hero_Apex_Level => GetLevelFromExperience(Hero_Apex_XP);

    public int Hero_Arachna_XP { get; set; }

    [NotMapped]
    public int Hero_Arachna_Level => GetLevelFromExperience(Hero_Arachna_XP);

    public int Hero_Armadon_XP { get; set; }

    [NotMapped]
    public int Hero_Armadon_Level => GetLevelFromExperience(Hero_Armadon_XP);

    public int Hero_Artesia_XP { get; set; }

    [NotMapped]
    public int Hero_Artesia_Level => GetLevelFromExperience(Hero_Artesia_XP);

    public int Hero_Artillery_XP { get; set; }

    [NotMapped]
    public int Hero_Artillery_Level => GetLevelFromExperience(Hero_Artillery_XP);

    public int Hero_BabaYaga_XP { get; set; }

    [NotMapped]
    public int Hero_BabaYaga_Level => GetLevelFromExperience(Hero_BabaYaga_XP);

    public int Hero_Behemoth_XP { get; set; }

    [NotMapped]
    public int Hero_Behemoth_Level => GetLevelFromExperience(Hero_Behemoth_XP);

    public int Hero_Bephelgor_XP { get; set; }

    [NotMapped]
    public int Hero_Bephelgor_Level => GetLevelFromExperience(Hero_Bephelgor_XP);

    public int Hero_Berzerker_XP { get; set; }

    [NotMapped]
    public int Hero_Berzerker_Level => GetLevelFromExperience(Hero_Berzerker_XP);

    public int Hero_Blitz_XP { get; set; }

    [NotMapped]
    public int Hero_Blitz_Level => GetLevelFromExperience(Hero_Blitz_XP);

    public int Hero_Bombardier_XP { get; set; }

    [NotMapped]
    public int Hero_Bombardier_Level => GetLevelFromExperience(Hero_Bombardier_XP);

    public int Hero_Bubbles_XP { get; set; }

    [NotMapped]
    public int Hero_Bubbles_Level => GetLevelFromExperience(Hero_Bubbles_XP);

    public int Hero_Bushwack_XP { get; set; }

    [NotMapped]
    public int Hero_Bushwack_Level => GetLevelFromExperience(Hero_Bushwack_XP);

    public int Hero_Calamity_XP { get; set; }

    [NotMapped]
    public int Hero_Calamity_Level => GetLevelFromExperience(Hero_Calamity_XP);

    public int Hero_Chipper_XP { get; set; }

    [NotMapped]
    public int Hero_Chipper_Level => GetLevelFromExperience(Hero_Chipper_XP);

    public int Hero_Chi_XP { get; set; }

    [NotMapped]
    public int Hero_Chi_Level => GetLevelFromExperience(Hero_Chi_XP);

    public int Hero_Chronos_XP { get; set; }

    [NotMapped]
    public int Hero_Chronos_Level => GetLevelFromExperience(Hero_Chronos_XP);

    public int Hero_Circe_XP { get; set; }

    [NotMapped]
    public int Hero_Circe_Level => GetLevelFromExperience(Hero_Circe_XP);

    public int Hero_CorruptedDisciple_XP { get; set; }

    [NotMapped]
    public int Hero_CorruptedDisciple_Level => GetLevelFromExperience(Hero_CorruptedDisciple_XP);

    public int Hero_Cthulhuphant_XP { get; set; }

    [NotMapped]
    public int Hero_Cthulhuphant_Level => GetLevelFromExperience(Hero_Cthulhuphant_XP);

    public int Hero_Dampeer_XP { get; set; }

    [NotMapped]
    public int Hero_Dampeer_Level => GetLevelFromExperience(Hero_Dampeer_XP);

    public int Hero_Deadlift_XP { get; set; }

    [NotMapped]
    public int Hero_Deadlift_Level => GetLevelFromExperience(Hero_Deadlift_XP);

    public int Hero_Deadwood_XP { get; set; }

    [NotMapped]
    public int Hero_Deadwood_Level => GetLevelFromExperience(Hero_Deadwood_XP);

    public int Hero_Defiler_XP { get; set; }

    [NotMapped]
    public int Hero_Defiler_Level => GetLevelFromExperience(Hero_Defiler_XP);

    public int Hero_Devourer_XP { get; set; }

    [NotMapped]
    public int Hero_Devourer_Level => GetLevelFromExperience(Hero_Devourer_XP);

    public int Hero_DiseasedRider_XP { get; set; }

    [NotMapped]
    public int Hero_DiseasedRider_Level => GetLevelFromExperience(Hero_DiseasedRider_XP);

    public int Hero_DoctorRepulsor_XP { get; set; }

    [NotMapped]
    public int Hero_DoctorRepulsor_Level => GetLevelFromExperience(Hero_DoctorRepulsor_XP);

    public int Hero_Dreadknight_XP { get; set; }

    [NotMapped]
    public int Hero_Dreadknight_Level => GetLevelFromExperience(Hero_Dreadknight_XP);

    public int Hero_DrunkenMaster_XP { get; set; }

    [NotMapped]
    public int Hero_DrunkenMaster_Level => GetLevelFromExperience(Hero_DrunkenMaster_XP);

    public int Hero_DwarfMagi_XP { get; set; }

    [NotMapped]
    public int Hero_DwarfMagi_Level => GetLevelFromExperience(Hero_DwarfMagi_XP);

    public int Hero_Ebulus_XP { get; set; }

    [NotMapped]
    public int Hero_Ebulus_Level => GetLevelFromExperience(Hero_Ebulus_XP);

    public int Hero_Electrician_XP { get; set; }

    [NotMapped]
    public int Hero_Electrician_Level => GetLevelFromExperience(Hero_Electrician_XP);

    public int Hero_Ellonia_XP { get; set; }

    [NotMapped]
    public int Hero_Ellonia_Level => GetLevelFromExperience(Hero_Ellonia_XP);

    public int Hero_EmeraldWarden_XP { get; set; }

    [NotMapped]
    public int Hero_EmeraldWarden_Level => GetLevelFromExperience(Hero_EmeraldWarden_XP);

    public int Hero_Empath_XP { get; set; }

    [NotMapped]
    public int Hero_Empath_Level => GetLevelFromExperience(Hero_Empath_XP);

    public int Hero_Engineer_XP { get; set; }

    [NotMapped]
    public int Hero_Engineer_Level => GetLevelFromExperience(Hero_Engineer_XP);

    public int Hero_Fade_XP { get; set; }

    [NotMapped]
    public int Hero_Fade_Level => GetLevelFromExperience(Hero_Fade_XP);

    public int Hero_Fairy_XP { get; set; }

    [NotMapped]
    public int Hero_Fairy_Level => GetLevelFromExperience(Hero_Fairy_XP);

    public int Hero_FlameDragon_XP { get; set; }

    [NotMapped]
    public int Hero_FlameDragon_Level => GetLevelFromExperience(Hero_FlameDragon_XP);

    public int Hero_FlintBeastwood_XP { get; set; }

    [NotMapped]
    public int Hero_FlintBeastwood_Level => GetLevelFromExperience(Hero_FlintBeastwood_XP);

    public int Hero_Flux_XP { get; set; }

    [NotMapped]
    public int Hero_Flux_Level => GetLevelFromExperience(Hero_Flux_XP);

    public int Hero_ForsakenArcher_XP { get; set; }

    [NotMapped]
    public int Hero_ForsakenArcher_Level => GetLevelFromExperience(Hero_ForsakenArcher_XP);

    public int Hero_Frosty_XP { get; set; }

    [NotMapped]
    public int Hero_Frosty_Level => GetLevelFromExperience(Hero_Frosty_XP);

    public int Hero_Gauntlet_XP { get; set; }

    [NotMapped]
    public int Hero_Gauntlet_Level => GetLevelFromExperience(Hero_Gauntlet_XP);

    public int Hero_Gemini_XP { get; set; }

    [NotMapped]
    public int Hero_Gemini_Level => GetLevelFromExperience(Hero_Gemini_XP);

    public int Hero_Geomancer_XP { get; set; }

    [NotMapped]
    public int Hero_Geomancer_Level => GetLevelFromExperience(Hero_Geomancer_XP);

    public int Hero_Gladiator_XP { get; set; }

    [NotMapped]
    public int Hero_Gladiator_Level => GetLevelFromExperience(Hero_Gladiator_XP);

    public int Hero_Goldenveil_XP { get; set; }

    [NotMapped]
    public int Hero_Goldenveil_Level => GetLevelFromExperience(Hero_Goldenveil_XP);

    public int Hero_Grinex_XP { get; set; }

    [NotMapped]
    public int Hero_Grinex_Level => GetLevelFromExperience(Hero_Grinex_XP);

    public int Hero_Gunblade_XP { get; set; }

    [NotMapped]
    public int Hero_Gunblade_Level => GetLevelFromExperience(Hero_Gunblade_XP);

    public int Hero_Hammerstorm_XP { get; set; }

    [NotMapped]
    public int Hero_Hammerstorm_Level => GetLevelFromExperience(Hero_Hammerstorm_XP);

    public int Hero_Hantumon_XP { get; set; }

    [NotMapped]
    public int Hero_Hantumon_Level => GetLevelFromExperience(Hero_Hantumon_XP);

    public int Hero_Hellbringer_XP { get; set; }

    [NotMapped]
    public int Hero_Hellbringer_Level => GetLevelFromExperience(Hero_Hellbringer_XP);

    public int Hero_HellDemon_XP { get; set; }

    [NotMapped]
    public int Hero_HellDemon_Level => GetLevelFromExperience(Hero_HellDemon_XP);

    public int Hero_Hiro_XP { get; set; }

    [NotMapped]
    public int Hero_Hiro_Level => GetLevelFromExperience(Hero_Hiro_XP);

    public int Hero_Hunter_XP { get; set; }

    [NotMapped]
    public int Hero_Hunter_Level => GetLevelFromExperience(Hero_Hunter_XP);

    public int Hero_Hydromancer_XP { get; set; }

    [NotMapped]
    public int Hero_Hydromancer_Level => GetLevelFromExperience(Hero_Hydromancer_XP);

    public int Hero_Ichor_XP { get; set; }

    [NotMapped]
    public int Hero_Ichor_Level => GetLevelFromExperience(Hero_Ichor_XP);

    public int Hero_Javaras_XP { get; set; }

    [NotMapped]
    public int Hero_Javaras_Level => GetLevelFromExperience(Hero_Javaras_XP);

    public int Hero_Jereziah_XP { get; set; }

    [NotMapped]
    public int Hero_Jereziah_Level => GetLevelFromExperience(Hero_Jereziah_XP);

    public int Hero_Kane_XP { get; set; }

    [NotMapped]
    public int Hero_Kane_Level => GetLevelFromExperience(Hero_Kane_XP);

    public int Hero_Kenisis_XP { get; set; }

    [NotMapped]
    public int Hero_Kenisis_Level => GetLevelFromExperience(Hero_Kenisis_XP);

    public int Hero_KingKlout_XP { get; set; }

    [NotMapped]
    public int Hero_KingKlout_Level => GetLevelFromExperience(Hero_KingKlout_XP);

    public int Hero_Klanx_XP { get; set; }

    [NotMapped]
    public int Hero_Klanx_Level => GetLevelFromExperience(Hero_Klanx_XP);

    public int Hero_Kraken_XP { get; set; }

    [NotMapped]
    public int Hero_Kraken_Level => GetLevelFromExperience(Hero_Kraken_XP);

    public int Hero_Krixi_XP { get; set; }

    [NotMapped]
    public int Hero_Krixi_Level => GetLevelFromExperience(Hero_Krixi_XP);

    public int Hero_Kunas_XP { get; set; }

    [NotMapped]
    public int Hero_Kunas_Level => GetLevelFromExperience(Hero_Kunas_XP);

    public int Hero_Legionnaire_XP { get; set; }

    [NotMapped]
    public int Hero_Legionnaire_Level => GetLevelFromExperience(Hero_Legionnaire_XP);

    public int Hero_Lodestone_XP { get; set; }

    [NotMapped]
    public int Hero_Lodestone_Level => GetLevelFromExperience(Hero_Lodestone_XP);

    public int Hero_Magmar_XP { get; set; }

    [NotMapped]
    public int Hero_Magmar_Level => GetLevelFromExperience(Hero_Magmar_XP);

    public int Hero_Maliken_XP { get; set; }

    [NotMapped]
    public int Hero_Maliken_Level => GetLevelFromExperience(Hero_Maliken_XP);

    public int Hero_Martyr_XP { get; set; }

    [NotMapped]
    public int Hero_Martyr_Level => GetLevelFromExperience(Hero_Martyr_XP);

    public int Hero_MasterOfArms_XP { get; set; }

    [NotMapped]
    public int Hero_MasterOfArms_Level => GetLevelFromExperience(Hero_MasterOfArms_XP);

    public int Hero_Midas_XP { get; set; }

    [NotMapped]
    public int Hero_Midas_Level => GetLevelFromExperience(Hero_Midas_XP);

    public int Hero_Mimix_XP { get; set; }

    [NotMapped]
    public int Hero_Mimix_Level => GetLevelFromExperience(Hero_Mimix_XP);

    public int Hero_Moira_XP { get; set; }

    [NotMapped]
    public int Hero_Moira_Level => GetLevelFromExperience(Hero_Moira_XP);

    public int Hero_Monarch_XP { get; set; }

    [NotMapped]
    public int Hero_Monarch_Level => GetLevelFromExperience(Hero_Monarch_XP);

    public int Hero_MonkeyKing_XP { get; set; }

    [NotMapped]
    public int Hero_MonkeyKing_Level => GetLevelFromExperience(Hero_MonkeyKing_XP);

    public int Hero_Moraxus_XP { get; set; }

    [NotMapped]
    public int Hero_Moraxus_Level => GetLevelFromExperience(Hero_Moraxus_XP);

    public int Hero_Mumra_XP { get; set; }

    [NotMapped]
    public int Hero_Mumra_Level => GetLevelFromExperience(Hero_Mumra_XP);

    public int Hero_Nitro_XP { get; set; }

    [NotMapped]
    public int Hero_Nitro_Level => GetLevelFromExperience(Hero_Nitro_XP);

    public int Hero_Nomad_XP { get; set; }

    [NotMapped]
    public int Hero_Nomad_Level => GetLevelFromExperience(Hero_Nomad_XP);

    public int Hero_Oogie_XP { get; set; }

    [NotMapped]
    public int Hero_Oogie_Level => GetLevelFromExperience(Hero_Oogie_XP);

    public int Hero_Ophelia_XP { get; set; }

    [NotMapped]
    public int Hero_Ophelia_Level => GetLevelFromExperience(Hero_Ophelia_XP);

    public int Hero_Panda_XP { get; set; }

    [NotMapped]
    public int Hero_Panda_Level => GetLevelFromExperience(Hero_Panda_XP);

    public int Hero_Parallax_XP { get; set; }

    [NotMapped]
    public int Hero_Parallax_Level => GetLevelFromExperience(Hero_Parallax_XP);

    public int Hero_Parasite_XP { get; set; }

    [NotMapped]
    public int Hero_Parasite_Level => GetLevelFromExperience(Hero_Parasite_XP);

    public int Hero_Pearl_XP { get; set; }

    [NotMapped]
    public int Hero_Pearl_Level => GetLevelFromExperience(Hero_Pearl_XP);

    public int Hero_Pestilence_XP { get; set; }

    [NotMapped]
    public int Hero_Pestilence_Level => GetLevelFromExperience(Hero_Pestilence_XP);

    public int Hero_Plant_XP { get; set; }

    [NotMapped]
    public int Hero_Plant_Level => GetLevelFromExperience(Hero_Plant_XP);

    public int Hero_PollywogPriest_XP { get; set; }

    [NotMapped]
    public int Hero_PollywogPriest_Level => GetLevelFromExperience(Hero_PollywogPriest_XP);

    public int Hero_Predator_XP { get; set; }

    [NotMapped]
    public int Hero_Predator_Level => GetLevelFromExperience(Hero_Predator_XP);

    public int Hero_Prisoner_XP { get; set; }

    [NotMapped]
    public int Hero_Prisoner_Level => GetLevelFromExperience(Hero_Prisoner_XP);

    public int Hero_Prophet_XP { get; set; }

    [NotMapped]
    public int Hero_Prophet_Level => GetLevelFromExperience(Hero_Prophet_XP);

    public int Hero_PuppetMaster_XP { get; set; }

    [NotMapped]
    public int Hero_PuppetMaster_Level => GetLevelFromExperience(Hero_PuppetMaster_XP);

    public int Hero_Pyromancer_XP { get; set; }

    [NotMapped]
    public int Hero_Pyromancer_Level => GetLevelFromExperience(Hero_Pyromancer_XP);

    public int Hero_Rally_XP { get; set; }

    [NotMapped]
    public int Hero_Rally_Level => GetLevelFromExperience(Hero_Rally_XP);

    public int Hero_Rampage_XP { get; set; }

    [NotMapped]
    public int Hero_Rampage_Level => GetLevelFromExperience(Hero_Rampage_XP);

    public int Hero_Ravenor_XP { get; set; }

    [NotMapped]
    public int Hero_Ravenor_Level => GetLevelFromExperience(Hero_Ravenor_XP);

    public int Hero_Ra_XP { get; set; }

    [NotMapped]
    public int Hero_Ra_Level => GetLevelFromExperience(Hero_Ra_XP);

    public int Hero_Revenant_XP { get; set; }

    [NotMapped]
    public int Hero_Revenant_Level => GetLevelFromExperience(Hero_Revenant_XP);

    public int Hero_Rhapsody_XP { get; set; }

    [NotMapped]
    public int Hero_Rhapsody_Level => GetLevelFromExperience(Hero_Rhapsody_XP);

    public int Hero_Riftmage_XP { get; set; }

    [NotMapped]
    public int Hero_Riftmage_Level => GetLevelFromExperience(Hero_Riftmage_XP);

    public int Hero_Riptide_XP { get; set; }

    [NotMapped]
    public int Hero_Riptide_Level => GetLevelFromExperience(Hero_Riptide_XP);

    public int Hero_Rocky_XP { get; set; }

    [NotMapped]
    public int Hero_Rocky_Level => GetLevelFromExperience(Hero_Rocky_XP);

    public int Hero_Salomon_XP { get; set; }

    [NotMapped]
    public int Hero_Salomon_Level => GetLevelFromExperience(Hero_Salomon_XP);

    public int Hero_SandWraith_XP { get; set; }

    [NotMapped]
    public int Hero_SandWraith_Level => GetLevelFromExperience(Hero_SandWraith_XP);

    public int Hero_Sapphire_XP { get; set; }

    [NotMapped]
    public int Hero_Sapphire_Level => GetLevelFromExperience(Hero_Sapphire_XP);

    public int Hero_Scar_XP { get; set; }

    [NotMapped]
    public int Hero_Scar_Level => GetLevelFromExperience(Hero_Scar_XP);

    public int Hero_Scout_XP { get; set; }

    [NotMapped]
    public int Hero_Scout_Level => GetLevelFromExperience(Hero_Scout_XP);

    public int Hero_ShadowBlade_XP { get; set; }

    [NotMapped]
    public int Hero_ShadowBlade_Level => GetLevelFromExperience(Hero_ShadowBlade_XP);

    public int Hero_Shaman_XP { get; set; }

    [NotMapped]
    public int Hero_Shaman_Level => GetLevelFromExperience(Hero_Shaman_XP);

    public int Hero_Shellshock_XP { get; set; }

    [NotMapped]
    public int Hero_Shellshock_Level => GetLevelFromExperience(Hero_Shellshock_XP);

    public int Hero_Silhouette_XP { get; set; }

    [NotMapped]
    public int Hero_Silhouette_Level => GetLevelFromExperience(Hero_Silhouette_XP);

    public int Hero_SirBenzington_XP { get; set; }

    [NotMapped]
    public int Hero_SirBenzington_Level => GetLevelFromExperience(Hero_SirBenzington_XP);

    public int Hero_Skrap_XP { get; set; }

    [NotMapped]
    public int Hero_Skrap_Level => GetLevelFromExperience(Hero_Skrap_XP);

    public int Hero_Solstice_XP { get; set; }

    [NotMapped]
    public int Hero_Solstice_Level => GetLevelFromExperience(Hero_Solstice_XP);

    public int Hero_Soulstealer_XP { get; set; }

    [NotMapped]
    public int Hero_Soulstealer_Level => GetLevelFromExperience(Hero_Soulstealer_XP);

    public int Hero_Succubis_XP { get; set; }

    [NotMapped]
    public int Hero_Succubis_Level => GetLevelFromExperience(Hero_Succubis_XP);

    public int Hero_Taint_XP { get; set; }

    [NotMapped]
    public int Hero_Taint_Level => GetLevelFromExperience(Hero_Taint_XP);

    public int Hero_Tarot_XP { get; set; }

    [NotMapped]
    public int Hero_Tarot_Level => GetLevelFromExperience(Hero_Tarot_XP);

    public int Hero_Tempest_XP { get; set; }

    [NotMapped]
    public int Hero_Tempest_Level => GetLevelFromExperience(Hero_Tempest_XP);

    public int Hero_Treant_XP { get; set; }

    [NotMapped]
    public int Hero_Treant_Level => GetLevelFromExperience(Hero_Treant_XP);

    public int Hero_Tremble_XP { get; set; }

    [NotMapped]
    public int Hero_Tremble_Level => GetLevelFromExperience(Hero_Tremble_XP);

    public int Hero_Tundra_XP { get; set; }

    [NotMapped]
    public int Hero_Tundra_Level => GetLevelFromExperience(Hero_Tundra_XP);

    public int Hero_Valkyrie_XP { get; set; }

    [NotMapped]
    public int Hero_Valkyrie_Level => GetLevelFromExperience(Hero_Valkyrie_XP);

    public int Hero_Vanya_XP { get; set; }

    [NotMapped]
    public int Hero_Vanya_Level => GetLevelFromExperience(Hero_Vanya_XP);

    public int Hero_Vindicator_XP { get; set; }

    [NotMapped]
    public int Hero_Vindicator_Level => GetLevelFromExperience(Hero_Vindicator_XP);

    public int Hero_Voodoo_XP { get; set; }

    [NotMapped]
    public int Hero_Voodoo_Level => GetLevelFromExperience(Hero_Voodoo_XP);

    public int Hero_Warchief_XP { get; set; }

    [NotMapped]
    public int Hero_Warchief_Level => GetLevelFromExperience(Hero_Warchief_XP);

    public int Hero_WitchSlayer_XP { get; set; }

    [NotMapped]
    public int Hero_WitchSlayer_Level => GetLevelFromExperience(Hero_WitchSlayer_XP);

    public int Hero_WolfMan_XP { get; set; }

    [NotMapped]
    public int Hero_WolfMan_Level => GetLevelFromExperience(Hero_WolfMan_XP);

    public int Hero_Xalynx_XP { get; set; }

    [NotMapped]
    public int Hero_Xalynx_Level => GetLevelFromExperience(Hero_Xalynx_XP);

    public int Hero_Yogi_XP { get; set; }

    [NotMapped]
    public int Hero_Yogi_Level => GetLevelFromExperience(Hero_Yogi_XP);

    public int Hero_Zephyr_XP { get; set; }

    [NotMapped]
    public int Hero_Zephyr_Level => GetLevelFromExperience(Hero_Zephyr_XP);
}
