namespace KONGOR.MasterServer.Configuration.Store;

public class StoreItemConfiguration
{
    public List<StoreItem> StoreItems { get; }

    private Dictionary<int, StoreItem> StoreItemsByID { get; }

    private Dictionary<string, StoreItem> StoreItemsByCode { get; }

    private Dictionary<string, StoreItem> StoreItemsByPrefixedCode { get; }

    public StoreItemConfiguration(List<StoreItem> storeItems)
    {
        StoreItems = storeItems;
        StoreItemsByID = storeItems.ToDictionary(item => item.ID);
        StoreItemsByCode = new Dictionary<string, StoreItem>(storeItems.Count);
        StoreItemsByPrefixedCode = new Dictionary<string, StoreItem>(storeItems.Count);

        foreach (StoreItem item in storeItems)
        {
            StoreItemsByCode.TryAdd(item.Code, item);
            StoreItemsByPrefixedCode.TryAdd(item.PrefixedCode, item);
        }
    }

    /// <summary>
    ///     Looks up a store item by its numeric ID.
    /// </summary>
    public StoreItem? GetByID(int id)
        => StoreItemsByID.GetValueOrDefault(id);

    /// <summary>
    ///     Looks up a store item by its unprefixed code (e.g. "Hero_Pyromancer.Female").
    /// </summary>
    public StoreItem? GetByCode(string code)
        => StoreItemsByCode.GetValueOrDefault(code);

    /// <summary>
    ///     Looks up a store item by its prefixed code (e.g. "aa.Hero_Pyromancer.Female").
    /// </summary>
    public StoreItem? GetByPrefixedCode(string prefixedCode)
        => StoreItemsByPrefixedCode.GetValueOrDefault(prefixedCode);

    /// <summary>
    ///     Returns all enabled store items whose <see cref="StoreItem.StoreItemType"/> matches the given type.
    /// </summary>
    public IEnumerable<StoreItem> GetEnabledItemsByType(StoreItemType type)
        => StoreItems.Where(item => item.StoreItemType == type && item.IsEnabled);
}

public class StoreItem
{
    public required int ID { get; init; }

    public required string Name { get; init; }

    public required string Code { get; init; }

    public required int Type { get; init; }

    public required string Resource { get; init; }

    public required bool Purchasable { get; init; }

    public required int GoldCost { get; init; }

    public required int SilverCost { get; init; }

    public required bool IsPremium { get; init; }

    public required bool IsBundle { get; init; }

    public required bool IsEnabled { get; init; }

    public required bool IsDynamic { get; init; }

    public required int[] Required { get; init; }

    /// <summary>
    ///     The strongly-typed store item type derived from the numeric <see cref="Type"/> field.
    /// </summary>
    public StoreItemType StoreItemType => (StoreItemType) Type;

    /// <summary>
    ///     The type prefix used to construct prefixed codes (e.g. "aa.", "cc.", "t.").
    /// </summary>
    public string Prefix => GetPrefix(StoreItemType);

    /// <summary>
    ///     The code prefixed with the type identifier (e.g. "aa.Hero_Pyromancer.Female").
    /// </summary>
    public string PrefixedCode => Prefix + Code;

    /// <summary>
    ///     Returns the type prefix for the given store item type.
    /// </summary>
    public static string GetPrefix(StoreItemType type) => type switch
    {
        StoreItemType.ChatNameColour     => "cc.",
        StoreItemType.ChatSymbol         => "cs.",
        StoreItemType.AccountIcon        => "ai.",
        StoreItemType.AlternativeAvatar  => "aa.",
        StoreItemType.AnnouncerVoice     => "av.",
        StoreItemType.Taunt              => "t.",
        StoreItemType.Courier            => "c.",
        StoreItemType.Hero               => "h.",
        StoreItemType.EarlyAccessProduct => "eap.",
        StoreItemType.Status             => "s.",
        StoreItemType.Miscellaneous      => "m.",
        StoreItemType.Ward               => "w.",
        StoreItemType.Enhancement        => "en.",
        StoreItemType.Coupon             => "cp.",
        StoreItemType.Mastery            => "ma.",
        StoreItemType.Creep              => "cr.",
        StoreItemType.Building           => "bu.",
        StoreItemType.TauntBadge         => "tb.",
        StoreItemType.TeleportEffect     => "te.",
        StoreItemType.SelectionCircle    => "sc.",
        StoreItemType.Bundle             => "",
        _                                => ""
    };
}

/// <summary>
///     Numeric types for store items, corresponding to the database "type" column in the products table.
/// </summary>
public enum StoreItemType
{
    ChatNameColour     = 00,
    ChatSymbol         = 01,
    AccountIcon        = 02,
    AlternativeAvatar  = 03,
    AnnouncerVoice     = 04,
    Taunt              = 05,
    Courier            = 06,
    Hero               = 07,
    EarlyAccessProduct = 08,
    Status             = 09,
    Miscellaneous      = 10,
    Ward               = 11,
    Enhancement        = 12,
    Coupon             = 13,
    Mastery            = 14,
    Creep              = 15,
    Building           = 16,
    TauntBadge         = 17,
    TeleportEffect     = 18,
    SelectionCircle    = 19,
    Bundle             = 20
}
