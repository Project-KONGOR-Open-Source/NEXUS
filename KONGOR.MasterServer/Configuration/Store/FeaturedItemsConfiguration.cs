namespace KONGOR.MasterServer.Configuration.Store;

/// <summary>
///     Configuration for the featured heroes section of the in-game store (category 68).
///     Specifies which individual items and bundles are showcased in the featured tab.
/// </summary>
public class FeaturedItemsConfiguration
{
    /// <summary>
    ///     The store item IDs of the individual products to display in the featured section.
    /// </summary>
    public required List<int> FeaturedItemIDs { get; set; }

    /// <summary>
    ///     The store item IDs of the bundles offered in the featured section.
    ///     Each bundle references a subset of the featured items via <see cref="FeaturedBundle.IncludedProductIndices"/>.
    /// </summary>
    public required List<FeaturedBundle> Bundles { get; set; }
}

/// <summary>
///     A bundle offered in the featured heroes section.
/// </summary>
public class FeaturedBundle
{
    /// <summary>
    ///     The store item ID of the bundle.
    /// </summary>
    public required int StoreItemID { get; set; }

    /// <summary>
    ///     The zero-based indices into <see cref="FeaturedItemsConfiguration.FeaturedItemIDs"/> that identify which featured products are included in this bundle.
    /// </summary>
    public required List<int> IncludedProductIndices { get; set; }
}
