namespace KONGOR.MasterServer.Configuration;

public static class JSONConfiguration
{
    private static readonly string BasePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Configuration");

    private static readonly string EconomyConfigurationJSON = File.ReadAllText(Path.Combine(BasePath, "Economy", "EconomyConfiguration.json"));

    private static readonly string MatchmakingConfigurationJSON = File.ReadAllText(Path.Combine(BasePath, "Matchmaking", "MatchmakingConfiguration.json"));

    private static readonly string MasteryRewardsConfigurationJSON = File.ReadAllText(Path.Combine(BasePath, "Mastery", "MasteryRewardsConfiguration.json"));

    private static readonly string StoreItemsConfigurationJSON = File.ReadAllText(Path.Combine(BasePath, "Store", "StoreItemsConfiguration.json"));

    private static readonly string DailySpecialsConfigurationJSON = File.ReadAllText(Path.Combine(BasePath, "Store", "DailySpecialsConfiguration.json"));

    private static readonly string FeaturedItemsConfigurationJSON = File.ReadAllText(Path.Combine(BasePath, "Store", "FeaturedItemsConfiguration.json"));

    private static readonly string AnnouncementsConfigurationJSON = File.ReadAllText(Path.Combine(BasePath, "Announcements", "AnnouncementsConfiguration.json"));

    private static readonly string PlinkoConfigurationJSON = File.ReadAllText(Path.Combine(BasePath, "Plinko", "PlinkoConfiguration.json"));

    private static readonly string PlinkoTierProductsConfigurationJSON = File.ReadAllText(Path.Combine(BasePath, "Plinko", "PlinkoTierProducts.json"));

    private static readonly string TicketExchangeConfigurationJSON = File.ReadAllText(Path.Combine(BasePath, "Plinko", "TicketExchangeConfiguration.json"));

    public static readonly EconomyConfiguration EconomyConfiguration = JsonSerializer.Deserialize<EconomyConfiguration>(EconomyConfigurationJSON)
        ?? throw new NullReferenceException("Economy Configuration Is NULL");

    public static readonly MatchmakingConfiguration MatchmakingConfiguration = JsonSerializer.Deserialize<MatchmakingConfiguration>(MatchmakingConfigurationJSON)
        ?? throw new NullReferenceException("Matchmaking Configuration Is NULL");

    public static readonly MasteryRewardsConfiguration MasteryRewardsConfiguration = JsonSerializer.Deserialize<MasteryRewardsConfiguration>(MasteryRewardsConfigurationJSON)
        ?? throw new NullReferenceException("Mastery Rewards Configuration Is NULL");

    public static readonly StoreItemsConfiguration StoreItemsConfiguration = new (JsonSerializer.Deserialize<List<StoreItem>>(StoreItemsConfigurationJSON)
        ?? throw new NullReferenceException("Store Item Configuration Is NULL"));

    public static readonly DailySpecialsConfiguration DailySpecialsConfiguration = JsonSerializer.Deserialize<DailySpecialsConfiguration>(DailySpecialsConfigurationJSON)
        ?? throw new NullReferenceException("Daily Specials Configuration Is NULL");

    public static readonly FeaturedItemsConfiguration FeaturedItemsConfiguration = JsonSerializer.Deserialize<FeaturedItemsConfiguration>(FeaturedItemsConfigurationJSON)
        ?? throw new NullReferenceException("Featured Items Configuration Is NULL");

    public static readonly AnnouncementsConfiguration AnnouncementsConfiguration = JsonSerializer.Deserialize<AnnouncementsConfiguration>(AnnouncementsConfigurationJSON)
        ?? throw new NullReferenceException("Announcements Configuration Is NULL");

    public static readonly PlinkoConfiguration PlinkoConfiguration = LoadPlinkoConfiguration();

    public static readonly PlinkoTierProductsConfiguration PlinkoTierProductsConfiguration = LoadPlinkoTierProductsConfiguration();

    public static readonly TicketExchangeConfiguration TicketExchangeConfiguration = LoadTicketExchangeConfiguration();

    private static PlinkoConfiguration LoadPlinkoConfiguration()
    {
        PlinkoConfiguration configuration = JsonSerializer.Deserialize<PlinkoConfiguration>(PlinkoConfigurationJSON)
            ?? throw new NullReferenceException("Plinko Configuration Is NULL");

        configuration.Validate();

        return configuration;
    }

    private static PlinkoTierProductsConfiguration LoadPlinkoTierProductsConfiguration()
    {
        List<PlinkoTierProducts> tiers = JsonSerializer.Deserialize<List<PlinkoTierProducts>>(PlinkoTierProductsConfigurationJSON)
            ?? throw new NullReferenceException("Plinko Tier Products Configuration Is NULL");

        PlinkoTierProductsConfiguration configuration = new () { Tiers = tiers };

        configuration.Initialise();

        return configuration;
    }

    private static TicketExchangeConfiguration LoadTicketExchangeConfiguration()
    {
        TicketExchangeConfiguration configuration = JsonSerializer.Deserialize<TicketExchangeConfiguration>(TicketExchangeConfigurationJSON)
            ?? throw new NullReferenceException("Ticket Exchange Configuration Is NULL");

        configuration.Initialise();

        return configuration;
    }
}
