namespace KONGOR.MasterServer.Configuration.Store;

public class StoreItemConfiguration
{
    public required List<StoreItem> StoreItems { get; set; }
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
}
