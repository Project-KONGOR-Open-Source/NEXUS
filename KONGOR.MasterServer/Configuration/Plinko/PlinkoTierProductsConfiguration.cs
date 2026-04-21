namespace KONGOR.MasterServer.Configuration.Plinko;

/// <summary>
///     The per-tier product catalogue for Plinko drops.
///     Each tier (1 to 4) holds a flat list of store product identifiers that can drop when that tier is rolled.
///     Tiers 5 and 6 are ticket-only tiers and have no product list.
/// </summary>
public class PlinkoTierProductsConfiguration
{
    /// <summary>
    ///     The raw per-tier product lists, as loaded from the configuration file.
    /// </summary>
    public required List<PlinkoTierProducts> Tiers { get; init; }

    private Dictionary<int, IReadOnlyList<int>> ProductsByTierID { get; set; } = new ();

    /// <summary>
    ///     Builds the per-tier lookup. Called by <see cref="JSONConfiguration"/> after deserialisation.
    /// </summary>
    public void Initialise()
    {
        ProductsByTierID = Tiers.ToDictionary(tier => tier.TierID, tier => (IReadOnlyList<int>) tier.ProductIDs);
    }

    /// <summary>
    ///     Returns the configured product identifiers for the given tier, or an empty list if the tier is not configured (e.g. ticket tiers 5 and 6).
    /// </summary>
    public IReadOnlyList<int> GetProductIDs(int tierID)
        => ProductsByTierID.TryGetValue(tierID, out IReadOnlyList<int>? products) ? products : [];

    /// <summary>
    ///     Counts the enabled and purchasable products for the given tier, resolving each product identifier through the supplied store catalogue.
    /// </summary>
    public int CountEnabledProducts(int tierID, StoreItemsConfiguration storeItems)
    {
        return GetProductIDs(tierID)
            .Select(productID => storeItems.GetByID(productID))
            .Count(storeItem => storeItem is { IsEnabled: true, Purchasable: true });
    }
}

/// <summary>
///     A single tier's product identifier list.
/// </summary>
public class PlinkoTierProducts
{
    /// <summary>
    ///     The tier identifier (1 = Diamond, 2 = Gold, 3 = Silver, 4 = Bronze).
    /// </summary>
    public required int TierID { get; init; }

    /// <summary>
    ///     The store product identifiers that can drop when this tier is rolled.
    /// </summary>
    public required List<int> ProductIDs { get; init; }
}
