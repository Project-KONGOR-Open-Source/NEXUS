namespace KONGOR.MasterServer.Configuration;

public static class JSONConfiguration
{
    private static readonly string BasePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Configuration");

    private static readonly string EconomyConfigurationJSON = File.ReadAllText(Path.Combine(BasePath, "Economy", "EconomyConfiguration.json"));

    private static readonly string MatchmakingConfigurationJSON = File.ReadAllText(Path.Combine(BasePath, "Matchmaking", "MatchmakingConfiguration.json"));

    private static readonly string MasteryRewardsConfigurationJSON = File.ReadAllText(Path.Combine(BasePath, "Mastery", "MasteryRewardsConfiguration.json"));

    private static readonly string StoreItemConfigurationJSON = File.ReadAllText(Path.Combine(BasePath, "Store", "StoreItemConfiguration.json"));

    public static readonly EconomyConfiguration EconomyConfiguration = JsonSerializer.Deserialize<EconomyConfiguration>(EconomyConfigurationJSON)
        ?? throw new NullReferenceException("Economy Configuration Is NULL");

    public static readonly MatchmakingConfiguration MatchmakingConfiguration = JsonSerializer.Deserialize<MatchmakingConfiguration>(MatchmakingConfigurationJSON)
        ?? throw new NullReferenceException("Matchmaking Configuration Is NULL");

    public static readonly MasteryRewardsConfiguration MasteryRewardsConfiguration = JsonSerializer.Deserialize<MasteryRewardsConfiguration>(MasteryRewardsConfigurationJSON)
        ?? throw new NullReferenceException("Mastery Rewards Configuration Is NULL");

    public static readonly StoreItemConfiguration StoreItemConfiguration = JsonSerializer.Deserialize<StoreItemConfiguration>(StoreItemConfigurationJSON)
        ?? throw new NullReferenceException("Store Item Configuration Is NULL");
}
