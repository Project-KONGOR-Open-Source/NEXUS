namespace KONGOR.MasterServer.Models.RequestResponse.Store;

/// <summary>
///     Response model for the "get_products" endpoint.
///     Returns all enabled store products grouped by their category name, keyed by product ID.
///     The client uses this data to populate the in-game store and to determine purchasable items.
/// </summary>
public class GetProductsResponse
{
    /// <summary>
    ///     Store products grouped by category name (e.g. "Alt Avatar", "Taunt", "Hero").
    ///     Each category maps product IDs to their details.
    /// </summary>
    [PHPProperty("products")]
    public Dictionary<string, Dictionary<int, GetProductsResponseEntry>> Products { get; init; }

    /// <summary>
    ///     CRC32 checksum of the products data, used by the client for cache validation.
    /// </summary>
    [PHPProperty("crc")]
    public int CRC { get; init; }

    /// <summary>
    ///     Creates a <see cref="GetProductsResponse"/> from all enabled store items in the store configuration.
    /// </summary>
    public GetProductsResponse(StoreItemsConfiguration storeItemsConfiguration)
    {
        Dictionary<string, Dictionary<int, GetProductsResponseEntry>> products = new();

        AddProducts(products, storeItemsConfiguration, "Alt Avatar", StoreItemType.AlternativeAvatar);
        AddProducts(products, storeItemsConfiguration, "Taunt", StoreItemType.Taunt);
        AddProducts(products, storeItemsConfiguration, "Misc", StoreItemType.Miscellaneous);
        AddProducts(products, storeItemsConfiguration, "Alt Announcement", StoreItemType.AnnouncerVoice);
        AddProducts(products, storeItemsConfiguration, "Couriers", StoreItemType.Courier);
        AddProducts(products, storeItemsConfiguration, "Hero", StoreItemType.Hero);
        AddProducts(products, storeItemsConfiguration, "Ward", StoreItemType.Ward);
        AddProducts(products, storeItemsConfiguration, "EAP", StoreItemType.EarlyAccessProduct);
        AddProducts(products, storeItemsConfiguration, "Mastery", StoreItemType.Mastery);

        Products = products;

        string serialised = JsonSerializer.Serialize(products);
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(serialised));

        // Compute A Stable CRC32 Of The Serialised Products For Client-Side Cache Validation
        CRC = BitConverter.ToInt32(hash, 0);
    }

    private static void AddProducts(Dictionary<string, Dictionary<int, GetProductsResponseEntry>> products, StoreItemsConfiguration storeItemsConfiguration, string categoryName, StoreItemType type)
    {
        Dictionary<int, GetProductsResponseEntry> entries = new();

        foreach (StoreItem item in storeItemsConfiguration.GetEnabledItemsByType(type))
        {
            entries[item.ID] = new GetProductsResponseEntry(item);
        }

        products[categoryName] = entries;
    }
}

/// <summary>
///     A single product entry within the "get_products" response.
/// </summary>
public class GetProductsResponseEntry
{
    public GetProductsResponseEntry(StoreItem storeItem)
    {
        ProductCode = storeItem.Code;
        ProductName = storeItem.Name;
        GoldCost = storeItem.GoldCost;
        Purchasable = storeItem.Purchasable;
        IsPremium = storeItem.IsPremium;
        SilverCost = storeItem.SilverCost;
        IsDynamic = storeItem.IsDynamic ? 1 : 0;

        if (storeItem.IsDynamic)
            LocalPath = storeItem.Resource;
    }

    /// <summary>
    ///     The unprefixed product code (e.g. "Hero_Pyromancer.Female").
    /// </summary>
    [PHPProperty("name")]
    public string ProductCode { get; init; }

    /// <summary>
    ///     The human-readable product name (e.g. "Female Pyromancer").
    /// </summary>
    [PHPProperty("cname")]
    public string ProductName { get; init; }

    /// <summary>
    ///     The gold coin cost.
    /// </summary>
    [PHPProperty("cost")]
    public int GoldCost { get; init; }

    /// <summary>
    ///     Whether the product can be purchased.
    /// </summary>
    [PHPProperty("purchasable")]
    public bool Purchasable { get; init; }

    /// <summary>
    ///     Whether the product is premium (silver-coin-only).
    /// </summary>
    [PHPProperty("premium")]
    public bool IsPremium { get; init; }

    /// <summary>
    ///     The silver coin cost.
    /// </summary>
    [PHPProperty("premium_mmp_cost")]
    public int SilverCost { get; init; }

    /// <summary>
    ///     Whether the product has dynamic (downloadable) content.
    /// </summary>
    [PHPProperty("dynamic")]
    public int IsDynamic { get; init; }

    /// <summary>
    ///     The local resource path for dynamic content. Only set when <see cref="IsDynamic"/> is 1.
    /// </summary>
    [PHPProperty("local_path")]
    public string? LocalPath { get; init; }
}
