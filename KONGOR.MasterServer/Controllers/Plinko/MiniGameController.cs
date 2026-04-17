namespace KONGOR.MasterServer.Controllers.Plinko;

/// <summary>
///     Handles the three Plinko mini-game endpoints: the panel index (<c>/master/casino/</c>), a single drop (<c>/master/casino/drop/</c>), and the paginated chest-browsing view (<c>/master/casino/viewchest/</c>).
/// </summary>
[ApiController]
[Consumes("application/x-www-form-urlencoded")]
public class MiniGameController(MerrickContext databaseContext, IDatabase distributedCache, Random random, ILogger<MiniGameController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IDatabase DistributedCache { get; } = distributedCache;
    private Random Random { get; } = random;
    private ILogger Logger { get; } = logger;

    private const int ViewChestPageSize = 56;

    private static PlinkoConfiguration PlinkoConfig => JSONConfiguration.PlinkoConfiguration;
    private static PlinkoTierProductsConfiguration TierProducts => JSONConfiguration.PlinkoTierProductsConfiguration;
    private static StoreItemsConfiguration StoreItems => JSONConfiguration.StoreItemsConfiguration;

    /// <summary>
    ///     Returns the Plinko panel's initial state: costs, the player's balances, the tier bucket layout, and the per-bucket product counts used by the UI.
    /// </summary>
    [HttpPost("master/casino/", Name = "Plinko Index Requester")]
    public async Task<IActionResult> Index()
    {
        Account? account = await LoadAccountFromCookie();

        if (account is null)
        {
            return Ok(PhpSerialization.Serialize(new Dictionary<string, object>
            {
                ["status_code"] = 0
            }));
        }

        User user = account.User;

        // Per-Bucket Enabled Product Counts, In The Visual Order Declared By The Bucket Layout
        string amountOfProducts = string.Join(",", PlinkoConfig.TierBucketOrder
            .Select(tierID => TierProducts.CountEnabledProducts(tierID, StoreItems).ToString()));

        Dictionary<string, object> response = new ()
        {
            ["status_code"]         = 1,
            ["tiers"]               = PlinkoConfig.TierBucketOrder,
            ["ticket_cost"]         = PlinkoConfig.TicketCost,
            ["gold_cost"]           = PlinkoConfig.GoldCost,
            ["user_gold"]           = user.GoldCoins,
            ["silver"]              = user.SilverCoins,
            ["user_tickets"]        = user.PlinkoTickets,
            ["amount_of_products"]  = amountOfProducts,
            ["last_update_time"]    = PlinkoConfig.LastUpdateTimes
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Executes a single Plinko drop, consuming either gold or tickets, rolling a weighted tier, and granting either a cosmetic product from that tier's pool or a consolation ticket payout.
    /// </summary>
    [HttpPost("master/casino/drop/", Name = "Plinko Drop Requester")]
    public async Task<IActionResult> Drop()
    {
        Account? account = await LoadAccountFromCookie();

        if (account is null)
        {
            return Ok(PhpSerialization.Serialize(new Dictionary<string, object>
            {
                ["status_code"] = 1
            }));
        }

        string currency = Request.Form["currency"].ToString();

        if (currency is not "gold" and not "tickets")
        {
            return Ok(PhpSerialization.Serialize(new Dictionary<string, object>
            {
                ["status_code"] = 1
            }));
        }

        User user = account.User;

        bool payingWithGold = currency.Equals("gold");

        if (payingWithGold && user.GoldCoins < PlinkoConfig.GoldCost)
        {
            return Ok(PhpSerialization.Serialize(new Dictionary<string, object>
            {
                ["status_code"] = 1
            }));
        }

        if (payingWithGold.Equals(false) && user.PlinkoTickets < PlinkoConfig.TicketCost)
        {
            return Ok(PhpSerialization.Serialize(new Dictionary<string, object>
            {
                ["status_code"] = 1
            }));
        }

        // Deduct The Cost Up-Front So Post-Drop Balances Are Consistent With A Failed Reward Path
        if (payingWithGold)
            user.GoldCoins -= PlinkoConfig.GoldCost;
        else
            user.PlinkoTickets -= PlinkoConfig.TicketCost;

        int tierID = GetRandomPlinkoTier();

        PlinkoDropOutcome outcome = tierID switch
        {
            1 or 2 or 3 or 4    => ResolveChestDrop(user, tierID),
            5 or 6              => ResolveTicketTierDrop(tierID),
            _                   => throw new InvalidOperationException($@"Rolled Tier ID ""{tierID}"" Is Outside The Supported Range 1-6")
        };

        user.PlinkoTickets += outcome.TicketReward;

        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Plinko Drop For Account ""{AccountID}"" Rolled Tier ""{TierID}"" (Currency ""{Currency}"", Product ""{ProductID}"", Ticket Delta ""{TicketDelta}"")",
            account.ID, tierID, currency, outcome.ProductID, outcome.TicketReward);

        Dictionary<string, object> response = new ()
        {
            ["status_code"]         = 1,
            ["random_tier"]         = tierID,
            ["product_id"]          = outcome.ProductID,
            ["product_name"]        = outcome.ProductName,
            ["product_path"]        = outcome.ProductPath,
            ["product_type"]        = outcome.ProductType,
            ["ticket_amount"]       = outcome.TicketReward,
            ["user_tickets"]        = user.PlinkoTickets,
            ["user_gold"]           = user.GoldCoins,
            ["products_exhausted"]  = outcome.ProductsExhausted ? 1 : 0
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Returns a paginated slice of the products available in a chest tier.
    ///     Used by the client's "browse chest contents" UI. Tiers 5 and 6 are ticket-only and are rejected.
    /// </summary>
    [HttpPost("master/casino/viewchest/", Name = "Plinko View Chest Requester")]
    public async Task<IActionResult> ViewChest()
    {
        Account? account = await LoadAccountFromCookie();

        if (account is null)
            return Ok(PhpSerialization.Serialize(new Dictionary<string, object> { ["status_code"] = 0 }));

        if (int.TryParse(Request.Form["tier_id"], out int tierID).Equals(false) || tierID is < 1 or > 4)
            return Ok(PhpSerialization.Serialize(new Dictionary<string, object> { ["status_code"] = 0 }));

        int targetIndex = 1;

        if (int.TryParse(Request.Form["target_index"], out int parsedTargetIndex))
            targetIndex = Math.Max(parsedTargetIndex, 1);

        IReadOnlyList<int> productIDs = TierProducts.GetProductIDs(tierID);

        List<StoreItem> page = productIDs
            .Skip(targetIndex - 1)
            .Take(ViewChestPageSize)
            .Select(productID => StoreItems.GetByID(productID))
            .Where(storeItem => storeItem is { Purchasable: true, IsEnabled: true })
            .Select(storeItem => storeItem!)
            .ToList();

        Dictionary<string, object> response = new ()
        {
            ["tier_id"]             = tierID,
            ["target_index"]        = targetIndex,
            ["first_item_index"]    = targetIndex,
            ["items_amount"]        = page.Count,
            ["product_names"]       = string.Join(",", page.Select(storeItem => storeItem.Code)),
            ["product_types"]       = string.Join(",", page.Select(storeItem => TrimTrailingDot(storeItem.Prefix))),
            ["product_paths"]       = string.Join(",", page.Select(storeItem => storeItem.Resource)),
            ["product_ids"]         = string.Join(",", page.Select(storeItem => storeItem.ID.ToString()))
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Rolls a weighted tier using the injected random source, matching the legacy probability distribution.
    /// </summary>
    private int GetRandomPlinkoTier()
    {
        double roll = Random.NextDouble() * 100.0;

        double cumulative = PlinkoConfig.DropProbabilities.Tier1;
        if (roll < cumulative)
            return 1;

        cumulative += PlinkoConfig.DropProbabilities.Tier2;
        if (roll < cumulative)
            return 2;

        cumulative += PlinkoConfig.DropProbabilities.Tier3;
        if (roll < cumulative)
            return 3;

        cumulative += PlinkoConfig.DropProbabilities.Tier4;
        if (roll < cumulative)
            return 4;

        cumulative += PlinkoConfig.DropProbabilities.Tier5;
        if (roll < cumulative)
            return 5;

        return 6;
    }

    /// <summary>
    ///     Resolves a chest-tier drop: filters the tier's products against what the user already owns and what is disabled, then either grants a uniformly random eligible product or, if none remain, falls back to the configured exhaustion ticket reward.
    /// </summary>
    private PlinkoDropOutcome ResolveChestDrop(User user, int tierID)
    {
        HashSet<string> ownedPrefixedCodes = user.OwnedStoreItems.ToHashSet();

        List<StoreItem> eligible = new ();

        foreach (int productID in TierProducts.GetProductIDs(tierID))
        {
            StoreItem? storeItem = StoreItems.GetByID(productID);

            if (storeItem is null)
            {
                Logger.LogWarning(@"Plinko Tier ""{TierID}"" References Unknown Product ID ""{ProductID}""", tierID, productID);
                continue;
            }

            if (storeItem.IsEnabled.Equals(false) || storeItem.Purchasable.Equals(false))
                continue;

            if (ownedPrefixedCodes.Contains(storeItem.PrefixedCode))
                continue;

            eligible.Add(storeItem);
        }

        if (eligible.Count.Equals(0))
        {
            return new PlinkoDropOutcome
            {
                ProductID           = -1,
                ProductName         = "Ticket",
                ProductPath         = "Ticket",
                ProductType         = "Ticket",
                TicketReward        = PlinkoConfig.GetExhaustionTicketReward(tierID),
                ProductsExhausted   = true
            };
        }

        StoreItem winner = eligible[Random.Next(eligible.Count)];

        user.OwnedStoreItems.Add(winner.PrefixedCode);

        return new PlinkoDropOutcome
        {
            ProductID           = winner.ID,
            ProductName         = winner.Code,
            ProductPath         = winner.Resource,
            ProductType         = StoreItem.GetClientCategoryName(winner.StoreItemType),
            TicketReward        = 0,
            ProductsExhausted   = false
        };
    }

    /// <summary>
    ///     Resolves a ticket-tier drop: always grants the configured consolation tickets and never mutates the user's inventory.
    /// </summary>
    private static PlinkoDropOutcome ResolveTicketTierDrop(int tierID) => new ()
    {
        ProductID           = -1,
        ProductName         = "Ticket",
        ProductPath         = "Ticket",
        ProductType         = "Ticket",
        TicketReward        = PlinkoConfig.GetConsolationTicketReward(tierID),
        ProductsExhausted   = true
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
            Logger.LogWarning(@"Plinko Request With Invalid Cookie ""{Cookie}"" From ""{IPAddress}""",
                cookie, Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN");

            return null;
        }

        return await MerrickContext.Accounts
            .Include(account => account.User)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));
    }

    /// <summary>
    ///     Strips the trailing dot from a store item prefix to produce the bare type code the client expects (e.g. <c>"aa."</c> becomes <c>"aa"</c>).
    /// </summary>
    private static string TrimTrailingDot(string prefix) => prefix.EndsWith('.') ? prefix[..^1] : prefix;
}

/// <summary>
///     The internal outcome of resolving a Plinko drop, used to assemble the response dictionary.
/// </summary>
internal sealed record PlinkoDropOutcome
{
    public required int ProductID { get; init; }
    public required string ProductName { get; init; }
    public required string ProductPath { get; init; }
    public required string ProductType { get; init; }
    public required int TicketReward { get; init; }
    public required bool ProductsExhausted { get; init; }
}
