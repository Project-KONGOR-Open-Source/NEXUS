namespace KONGOR.MasterServer.Configuration.Plinko;

/// <summary>
///     The catalogue of items that players can buy directly with Plinko tickets from the ticket exchange panel.
///     Each entry maps a fixed ticket cost to a store product.
/// </summary>
public class TicketExchangeConfiguration
{
    /// <summary>
    ///     The available ticket exchange entries, in display order.
    /// </summary>
    public required List<TicketExchangeEntry> Items { get; init; }

    private Dictionary<int, TicketExchangeEntry> ItemsByProductID { get; set; } = new ();

    /// <summary>
    ///     Builds the <see cref="ProductID"/> lookup. Called by <see cref="JSONConfiguration"/> after deserialisation.
    /// </summary>
    public void Initialise()
    {
        ItemsByProductID = Items.ToDictionary(item => item.ProductID);
    }

    /// <summary>
    ///     Returns the exchange entry for the given store product identifier, or <see langword="null"/> if the product is not available on the exchange.
    /// </summary>
    public TicketExchangeEntry? GetByProductID(int productID)
        => ItemsByProductID.GetValueOrDefault(productID);
}

/// <summary>
///     A single entry in the ticket exchange catalogue.
/// </summary>
public class TicketExchangeEntry
{
    /// <summary>
    ///     The display identifier of the entry shown to the client UI.
    /// </summary>
    public required int ID { get; init; }

    /// <summary>
    ///     The number of tickets the player must spend to claim this product.
    /// </summary>
    public required int TicketCost { get; init; }

    /// <summary>
    ///     The store product identifier granted to the player upon purchase.
    /// </summary>
    public required int ProductID { get; init; }

    /// <summary>
    ///     The display name shown in the ticket exchange panel.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     The product category name shown alongside the icon (e.g. "Misc", "Alt Avatar").
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    ///     The client-side icon path shown in the ticket exchange panel.
    /// </summary>
    public required string LocalPath { get; init; }
}
