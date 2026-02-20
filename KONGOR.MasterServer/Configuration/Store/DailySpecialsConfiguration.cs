namespace KONGOR.MasterServer.Configuration.Store;

/// <summary>
///     Configuration for the daily specials panel in the in-game store.
///     Each entry references a store item by ID and specifies discount percentages.
/// </summary>
public class DailySpecialsConfiguration
{
    public required List<DailySpecialEntry> Specials { get; set; }
}

/// <summary>
///     A single daily special entry shown in the store's specials tab.
/// </summary>
public class DailySpecialEntry
{
    /// <summary>
    ///     The store item ID of the featured product.
    /// </summary>
    public required int StoreItemID { get; set; }

    /// <summary>
    ///     The gold coin discount percentage (0–100). The discounted gold price is calculated as <c>GoldCost * (100 - DiscountPercentageGold) / 100</c>.
    /// </summary>
    public required int DiscountPercentageGold { get; set; }

    /// <summary>
    ///     The silver coin discount percentage (0–100). The discounted silver price is calculated as <c>SilverCost * (100 - DiscountPercentageSilver) / 100</c>.
    /// </summary>
    public required int DiscountPercentageSilver { get; set; }

    /// <summary>
    ///     An optional promotional tag displayed on the daily special (e.g. "The One And Only!").
    /// </summary>
    public required string TagTitle { get; set; }

    /// <summary>
    ///     An optional date or sale label displayed on the daily special (e.g. "20% Sale").
    /// </summary>
    public required string TagDateLimit { get; set; }
}
