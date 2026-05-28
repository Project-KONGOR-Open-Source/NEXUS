namespace KONGOR.MasterServer.Controllers.Plinko;

/// <summary>
///     Handles the two Plinko ticket-exchange endpoints: the exchange catalogue listing (<c>/master/ticketexchange/</c>) and the purchase of a specific item (<c>/master/ticketexchange/purchase/</c>).
/// </summary>
[ApiController]
[Consumes("application/x-www-form-urlencoded")]
public class TicketExchangeController(MerrickContext databaseContext, IDatabase distributedCache, ILogger<TicketExchangeController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IDatabase DistributedCache { get; } = distributedCache;
    private ILogger Logger { get; } = logger;

    private const int StatusBadCookie           = 50;
    private const int StatusSuccess             = 51;
    private const int StatusInsufficientTickets = 52;
    private const int StatusAlreadyOwned        = 53;
    private const int StatusInvalidItem         = 54;

    private static TicketExchangeConfiguration ExchangeConfig => JSONConfiguration.TicketExchangeConfiguration;
    private static StoreItemsConfiguration StoreItems => JSONConfiguration.StoreItemsConfiguration;

    /// <summary>
    ///     Returns every entry in the ticket-exchange catalogue, keyed by zero-based index, along with the player's current ticket balance.
    /// </summary>
    [HttpPost("master/ticketexchange/", Name = "Plinko Ticket Exchange List Requester")]
    public async Task<IActionResult> List()
    {
        Account? account = await LoadAccountFromCookie();

        if (account is null)
        {
            return Ok(PhpSerialization.Serialize(new Dictionary<string, object>
            {
                ["status_code"] = StatusBadCookie
            }));
        }

        Dictionary<int, Dictionary<string, object>> items = new ();

        for (int index = 0; index < ExchangeConfig.Items.Count; index++)
        {
            TicketExchangeEntry entry = ExchangeConfig.Items[index];

            items[index] = new Dictionary<string, object>
            {
                ["id"]          = entry.ID,
                ["cost"]        = entry.TicketCost,
                ["product_id"]  = entry.ProductID,
                ["name"]        = entry.Name,
                ["type"]        = entry.Type,
                ["local_path"]  = entry.LocalPath
            };
        }

        Dictionary<string, object> response = new ()
        {
            ["status_code"]     = StatusSuccess,
            ["items"]           = items,
            ["user_tickets"]    = account.User.PlinkoTickets
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Purchases a ticket-exchange item, deducting the item's ticket cost and granting the store product to the player's inventory.
    ///     Returns granular status codes so support staff can distinguish invalid cookie, insufficient tickets, already-owned, and invalid-item failures.
    /// </summary>
    [HttpPost("master/ticketexchange/purchase/", Name = "Plinko Ticket Exchange Purchase Requester")]
    public async Task<IActionResult> Purchase()
    {
        Account? account = await LoadAccountFromCookie();

        if (account is null)
            return PurchaseFailureResponse(StatusBadCookie);

        if (int.TryParse(Request.Form["id"], out int productID).Equals(false))
            return PurchaseFailureResponse(StatusInvalidItem);

        TicketExchangeEntry? entry = ExchangeConfig.GetByProductID(productID);

        if (entry is null)
            return PurchaseFailureResponse(StatusInvalidItem);

        StoreItem? storeItem = StoreItems.GetByID(entry.ProductID);

        if (storeItem is null)
        {
            Logger.LogWarning(@"Ticket Exchange Entry ""{EntryID}"" References Unknown Product ID ""{ProductID}""", entry.ID, entry.ProductID);

            return PurchaseFailureResponse(StatusInvalidItem);
        }

        User user = account.User;

        if (user.PlinkoTickets < entry.TicketCost)
            return PurchaseFailureResponse(StatusInsufficientTickets);

        if (user.OwnedStoreItems.Contains(storeItem.PrefixedCode))
            return PurchaseFailureResponse(StatusAlreadyOwned);

        user.PlinkoTickets -= entry.TicketCost;
        user.OwnedStoreItems.Add(storeItem.PrefixedCode);

        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Ticket Exchange Purchase For Account ""{AccountID}"" Redeemed Product ""{ProductID}"" For ""{TicketCost}"" Tickets",
            account.ID, entry.ProductID, entry.TicketCost);

        Dictionary<string, object> response = BuildPurchaseResponse(StatusSuccess, user.PlinkoTickets);

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Builds a purchase failure response with all eight required client fields, so the HON UI never nil-indexes a missing grab-bag key.
    /// </summary>
    private IActionResult PurchaseFailureResponse(int statusCode)
    {
        // On Failure The Client Still Reads Every Result Param, So Emit The Full Field Set
        Dictionary<string, object> response = BuildPurchaseResponse(statusCode, ticketsRemaining: 0);

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Builds the full eight-field purchase response matching <c>plinko.package</c>'s result-param declaration.
    ///     Grab-bag fields are emitted as placeholders because the Super-Taunt and similar exchange items are not grab-bags.
    /// </summary>
    private static Dictionary<string, object> BuildPurchaseResponse(int statusCode, int ticketsRemaining) => new ()
    {
        ["status_code"]             = statusCode,
        ["tickets_remaining"]       = ticketsRemaining,
        ["grabBag"]                 = "0",
        ["grabBagTheme"]            = string.Empty,
        ["grabBagIDs"]              = string.Empty,
        ["grabBagTypes"]            = string.Empty,
        ["grabBagLocalPaths"]       = string.Empty,
        ["grabBagProductNames"]     = string.Empty
    };

    /// <summary>
    ///     Resolves the current request's session cookie to an <see cref="Account"/> with its <see cref="User"/> eagerly loaded, or returns <see langword="null"/> if the cookie is missing, invalid, or does not correspond to an existing account.
    /// </summary>
    private async Task<Account?> LoadAccountFromCookie()
    {
        string cookie = Request.Form["cookie"].ToString();

        if (string.IsNullOrEmpty(cookie))
            return null;

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid.Equals(false) || accountName is null)
        {
            Logger.LogWarning(@"Ticket Exchange Request With Invalid Cookie ""{Cookie}"" From ""{IPAddress}""",
                cookie, Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN");

            return null;
        }

        return await MerrickContext.Accounts
            .Include(account => account.User)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));
    }
}
