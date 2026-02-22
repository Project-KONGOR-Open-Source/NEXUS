namespace KONGOR.MasterServer.Controllers.Store;

[ApiController]
[Route("store_requester.php")]
[Consumes("application/x-www-form-urlencoded")]
public class StoreController(MerrickContext databaseContext, IDatabase distributedCache, ILogger<StoreController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IDatabase DistributedCache { get; } = distributedCache;
    private ILogger Logger { get; } = logger;

    private const int ItemsPerPage = 12;
    private const int TauntStoreItemID = 91;
    private const string TauntPrefixedCode = "t.Standard";
    private const int SuperTauntStoreItemID = 1792;

    private static StoreItemsConfiguration StoreItems => JSONConfiguration.StoreItemsConfiguration;

    [HttpPost(Name = "Store Requester")]
    public async Task<IActionResult> StoreRequester()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid.Equals(false) || accountName is null)
        {
            Logger.LogWarning(@"Store Request With Invalid Cookie ""{Cookie}"" From ""{IPAddress}""",
                cookie, Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN");

            return Ok(PhpSerialization.Serialize(CreateErrorResponse((int) StoreErrorCode.STORE_SESSION_ERROR)));
        }

        string? requestCodeString = Request.Form["request_code"];

        if (requestCodeString is null)
            return BadRequest(@"Missing Value For Form Parameter ""request_code""");

        if (Enum.TryParse(requestCodeString, out StoreRequestCode requestCode).Equals(false))
            return BadRequest(@"Invalid Value For Form Parameter ""request_code""");

        Account account = await MerrickContext.Accounts
            .Include(queriedAccount => queriedAccount.User)
            .SingleAsync(queriedAccount => queriedAccount.Name.Equals(accountName));

        string? accountIDString = Request.Form["account_id"];

        if (accountIDString is not null && int.TryParse(accountIDString, out int accountID) && account.ID != accountID)
        {
            Logger.LogWarning(@"Store Request Account ID Mismatch For ""{AccountName}"" (Expected {RealID}, Got {ClaimedID})",
                accountName, account.ID, accountID);

            return Ok(PhpSerialization.Serialize(CreateErrorResponse((int) StoreErrorCode.STORE_ACCOUNT_INFORMATION_ERROR)));
        }

        switch (requestCode)
        {
            case StoreRequestCode.LIST_STORE_ITEMS_REQUEST:
                return ViewStore(account);

            case StoreRequestCode.LIST_VAULT_ITEMS_REQUEST:
                return ViewVault(account);

            case StoreRequestCode.ATTEMPT_PURCHASE_PRODUCT_REQUEST:
                return await PurchaseProduct(account);

            case StoreRequestCode.LIST_PRODUCT_IDS_SELECTED_REQUEST:
                return ListSelectedUpgrades(account);

            case StoreRequestCode.BUY_PRODUCT_GAME_LOBBY_REQUEST:
                return await PurchaseProductInGame(account);

            case StoreRequestCode.REDEEM_CODE_REQUEST:
                return RedeemCode();

            default:
                return Ok(PhpSerialization.Serialize(new Dictionary<string, object>
                {
                    ["popupCode"] = -1,
                    ["errorCode"] = 0
                }));
        }
    }

    /// <summary>
    ///     Generates a response for a request to view a store category page.
    /// </summary>
    private IActionResult ViewStore(Account account)
    {
        string? categoryIDString = Request.Form["category_id"];

        if (categoryIDString is null)
            return BadRequest(@"Missing Value For Form Parameter ""category_id""");

        if (int.TryParse(categoryIDString, out int categoryID).Equals(false))
            return BadRequest(@"Invalid Value For Form Parameter ""category_id""");

        string? pageString = Request.Form["page"];

        if (pageString is null)
            return BadRequest(@"Missing Value For Form Parameter ""page""");

        if (int.TryParse(pageString, out int currentPage).Equals(false))
            return BadRequest(@"Invalid Value For Form Parameter ""page""");

        string? hostTime = Request.Form["hostTime"];

        if (hostTime is null)
            return BadRequest(@"Missing Value For Form Parameter ""hostTime""");

        Dictionary<string, object> response = new ();

        PopulateItemListing(account.User, account, response, categoryID, currentPage);

        response["popupCode"] = -1;
        response["responseCode"] = (int) StoreResponseCode.BASIC_ITEM_LIST_RESPONSE;
        response["requestHostTime"] = hostTime;

        if (response.ContainsKey("errorCode").Equals(false))
            response["errorCode"] = 0;

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Generates a response for a request to view the vault (owned items).
    /// </summary>
    private IActionResult ViewVault(Account account)
    {
        string? hostTime = Request.Form["hostTime"];

        if (hostTime is null)
            return BadRequest(@"Missing Value For Form Parameter ""hostTime""");

        Dictionary<string, object> response = new ();

        StoreCategory[] vaultCategories =
        [
            StoreCategory.AccountIcons,
            StoreCategory.NameColors,
            StoreCategory.ChatSymbols,
            StoreCategory.Taunt,
            StoreCategory.TauntBadges,
            StoreCategory.Announcers,
            StoreCategory.Couriers,
            StoreCategory.Wards,
            StoreCategory.TeleportationEffects,
            StoreCategory.EarlyAccessHeroes,
            StoreCategory.Creeps,
            StoreCategory.SelectionCircles,
            StoreCategory.Enhancements,
            StoreCategory.Miscellaneous
        ];

        foreach (StoreCategory category in vaultCategories)
        {
            StoreItemType itemType = MapStoreCategoryToItemType((int) category);

            response["vaultCategory" + (int) category] = CreateVaultData(account.User, itemType);
        }

        response["requestHostTime"] = hostTime;
        response["responseCode"] = (int) StoreResponseCode.VAULT_ITEM_LIST_RESPONSE;
        response["totalPoints"] = account.User.GoldCoins;
        response["totalMMPoints"] = account.User.SilverCoins;

        PopulateSelectedUpgrades(account, response);

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Processes a store purchase request.
    /// </summary>
    private async Task<IActionResult> PurchaseProduct(Account account)
    {
        string? productIDString = Request.Form["product_id"];

        if (productIDString is null)
            return BadRequest(@"Missing Value For Form Parameter ""product_id""");

        if (int.TryParse(productIDString, out int productID).Equals(false))
            return BadRequest(@"Invalid Value For Form Parameter ""product_id""");

        string? currency = Request.Form["currency"];

        if (currency is null)
            return BadRequest(@"Missing Value For Form Parameter ""currency""");

        string? categoryIDString = Request.Form["category_id"];

        if (categoryIDString is null)
            return BadRequest(@"Missing Value For Form Parameter ""category_id""");

        if (int.TryParse(categoryIDString, out int categoryID).Equals(false))
            return BadRequest(@"Invalid Value For Form Parameter ""category_id""");

        string? pageString = Request.Form["page"];

        if (pageString is null)
            return BadRequest(@"Missing Value For Form Parameter ""page""");

        if (int.TryParse(pageString, out int currentPage).Equals(false))
            return BadRequest(@"Invalid Value For Form Parameter ""page""");

        string? hostTime = Request.Form["hostTime"];

        if (hostTime is null)
            return BadRequest(@"Missing Value For Form Parameter ""hostTime""");

        StoreItem? storeItem = StoreItems.GetByID(productID);

        (Dictionary<string, object> response, bool success) = ExecutePurchase(account, storeItem, currency);

        if (success)
            await MerrickContext.SaveChangesAsync();

        PopulateItemListing(account.User, account, response, categoryID, currentPage);

        response["responseCode"] = (int) StoreResponseCode.BASIC_ITEM_LIST_RESPONSE;
        response["requestHostTime"] = hostTime;

        if (response.ContainsKey("errorCode").Equals(false))
            response["errorCode"] = 0;

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Processes a purchase request from within the game lobby.
    /// </summary>
    private async Task<IActionResult> PurchaseProductInGame(Account account)
    {
        string? heroName = Request.Form["hero_name"];

        if (heroName is null)
            return BadRequest(@"Missing Value For Form Parameter ""hero_name""");

        string? avatarCode = Request.Form["avatar_code"];

        if (avatarCode is null)
            return BadRequest(@"Missing Value For Form Parameter ""avatar_code""");

        string? currency = Request.Form["currency"];

        if (currency is null)
            return BadRequest(@"Missing Value For Form Parameter ""currency""");

        string itemCode = heroName + "." + avatarCode;

        StoreItem? storeItem = StoreItems.GetByCode(itemCode);

        (Dictionary<string, object> response, bool success) = ExecutePurchase(account, storeItem, currency);

        if (success)
            await MerrickContext.SaveChangesAsync();

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Returns the selected upgrade product IDs for the account.
    /// </summary>
    private IActionResult ListSelectedUpgrades(Account account)
    {
        Dictionary<string, object> response = new ();

        PopulateSelectedUpgrades(account, response);

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Processes a code redemption request. Not yet implemented.
    /// </summary>
    private IActionResult RedeemCode()
    {
        // TODO: Implement Code Redemption

        Dictionary<string, object> response = new ()
        {
            ["popupCode"] = (int) StorePopupCode.POP_UP_ERROR_MESSAGE,
            ["errorCode"] = (int) StoreErrorCode.STORE_PURCHASE_CODE_INVALID_ERROR
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Executes a purchase transaction, deducting the appropriate currency and adding the item to the user's owned items.
    /// </summary>
    private static (Dictionary<string, object> Response, bool Success) ExecutePurchase(Account account, StoreItem? storeItem, string currency)
    {
        Dictionary<string, object> response = new ();
        User user = account.User;

        if (storeItem is null)
        {
            response["popupCode"] = (int) StorePopupCode.POP_UP_ERROR_MESSAGE;
            response["errorCode"] = (int) StoreErrorCode.STORE_PURCHASE_ITEM_MISSING_ERROR;

            return (response, false);
        }

        if (storeItem.Purchasable.Equals(false) || storeItem.IsEnabled.Equals(false))
        {
            response["popupCode"] = (int) StorePopupCode.POP_UP_ERROR_MESSAGE;
            response["errorCode"] = (int) StoreErrorCode.STORE_PURCHASE_ITEM_ERROR;

            return (response, false);
        }

        if (user.OwnedStoreItems.Contains(storeItem.PrefixedCode))
        {
            response["popupCode"] = (int) StorePopupCode.POP_UP_ERROR_MESSAGE;
            response["errorCode"] = (int) StoreErrorCode.STORE_PURCHASE_ALREADY_OWNED;

            return (response, false);
        }

        // Must Own The Base Taunt Before Purchasing Other Taunts Or The Super Taunt
        if (RequiresBaseTaunt(storeItem) && user.OwnedStoreItems.Contains(TauntPrefixedCode).Equals(false))
        {
            response["popupCode"] = (int) StorePopupCode.POP_UP_ERROR_MESSAGE;
            response["errorCode"] = (int) StoreErrorCode.STORE_PURCHASE_ITEM_ERROR;

            return (response, false);
        }

        if (currency is "0")
        {
            if (user.GoldCoins < storeItem.GoldCost)
            {
                response["popupCode"] = (int) StorePopupCode.POP_UP_ERROR_MESSAGE;
                response["errorCode"] = (int) StoreErrorCode.STORE_PURCHASE_POINT_ERROR;

                return (response, false);
            }

            user.GoldCoins -= storeItem.GoldCost;
        }

        else if (currency is "1")
        {
            if (user.SilverCoins < storeItem.SilverCost)
            {
                response["popupCode"] = (int) StorePopupCode.POP_UP_ERROR_MESSAGE;
                response["errorCode"] = (int) StoreErrorCode.STORE_PURCHASE_POINT_ERROR;

                return (response, false);
            }

            user.SilverCoins -= storeItem.SilverCost;
        }

        else
        {
            response["popupCode"] = (int) StorePopupCode.POP_UP_ERROR_MESSAGE;
            response["errorCode"] = (int) StoreErrorCode.STORE_INVALID_REQUEST_ERROR;

            return (response, false);
        }

        user.OwnedStoreItems.Add(storeItem.PrefixedCode);

        // If The Purchased Item Is A Featured Bundle, Also Grant The Individual Products It Contains
        GrantFeaturedBundleContents(user, storeItem);

        // Mark Any Featured Bundles As Owned If All Their Individual Products Are Now Owned
        MarkCompletedFeaturedBundlesAsOwned(user);

        response["popupCode"] = (int) StorePopupCode.POP_UP_PRODUCT_PURCHASE_SUCCESS;
        response["errorCode"] = 0;

        return (response, true);
    }

    /// <summary>
    ///     Grants the individual products contained within a featured bundle to the user.
    ///     If the purchased item is not a featured bundle, this method does nothing.
    /// </summary>
    private static void GrantFeaturedBundleContents(User user, StoreItem purchasedItem)
    {
        FeaturedItemsConfiguration featuredConfiguration = JSONConfiguration.FeaturedItemsConfiguration;

        FeaturedBundle? bundle = featuredConfiguration.Bundles
            .SingleOrDefault(featuredBundle => featuredBundle.StoreItemID == purchasedItem.ID);

        if (bundle is null)
            return;

        foreach (int productIndex in bundle.IncludedProductIndices)
        {
            if (productIndex < 0 || productIndex > featuredConfiguration.FeaturedItemIDs.Count - 1)
                continue;

            int itemID = featuredConfiguration.FeaturedItemIDs[productIndex];
            StoreItem? includedItem = StoreItems.GetByID(itemID);

            if (includedItem is not null && user.OwnedStoreItems.Contains(includedItem.PrefixedCode).Equals(false))
                user.OwnedStoreItems.Add(includedItem.PrefixedCode);
        }
    }

    /// <summary>
    ///     Marks any featured bundles as owned when all of their included products are already owned.
    ///     This prevents the user from purchasing a bundle whose individual products were acquired separately.
    /// </summary>
    private static void MarkCompletedFeaturedBundlesAsOwned(User user)
    {
        FeaturedItemsConfiguration featuredConfiguration = JSONConfiguration.FeaturedItemsConfiguration;

        foreach (FeaturedBundle bundle in featuredConfiguration.Bundles)
        {
            StoreItem? bundleItem = StoreItems.GetByID(bundle.StoreItemID);

            if (bundleItem is null)
                continue;

            if (user.OwnedStoreItems.Contains(bundleItem.PrefixedCode))
                continue;

            bool allIncludedProductsOwned = bundle.IncludedProductIndices.All(productIndex =>
            {
                if (productIndex < 0 || productIndex > featuredConfiguration.FeaturedItemIDs.Count - 1)
                    return false;

                int itemID = featuredConfiguration.FeaturedItemIDs[productIndex];

                StoreItem? includedItem = StoreItems.GetByID(itemID);

                return includedItem is not null && user.OwnedStoreItems.Contains(includedItem.PrefixedCode);
            });

            if (allIncludedProductsOwned)
                user.OwnedStoreItems.Add(bundleItem.PrefixedCode);
        }
    }

    /// <summary>
    ///     Populates the response dictionary with the item listing for a store category page.
    ///     For the featured heroes category (68), a specialised response format with bundle data is returned.
    /// </summary>
    private static void PopulateItemListing(User user, Account account, Dictionary<string, object> response, int categoryID, int currentPage)
    {
        // The Featured Heroes Category Uses A Specialised Response Format With Bundle Data
        if (categoryID == (int) StoreCategory.FeaturedHeroAvatars)
        {
            PopulateFeaturedItemListing(user, account, response);
            return;
        }

        StoreItemType itemType = MapStoreCategoryToItemType(categoryID);

        // Include Taunt Availability Status For Taunt And Miscellaneous Categories
        if (ShouldSendTauntStatus(itemType))
        {
            StoreItem? baseTaunt = StoreItems.GetByID(TauntStoreItemID);

            if (baseTaunt is not null)
            {
                response["tauntUnlocked"] = user.OwnedStoreItems.Contains(baseTaunt.PrefixedCode) ? 1 : 0;
                response["tauntUnlockCost"] = baseTaunt.GoldCost;
                response["tauntUnlockCostMMP"] = baseTaunt.SilverCost;
            }
        }

        // Retrieve All Enabled Items For This Store Item Type
        IEnumerable<StoreItem> itemsForSale = StoreItems.GetEnabledItemsByType(itemType);

        // Chat Symbols Are Split Into Flags And Hero Symbols Based On The Category ID
        if (categoryID == (int) StoreCategory.FlagSymbols)
            itemsForSale = itemsForSale.Where(storeItem => storeItem.Name.Contains("Flag"));

        else if (categoryID == (int) StoreCategory.HeroSymbols)
            itemsForSale = itemsForSale.Where(storeItem => storeItem.Name.Contains("Flag").Equals(false));

        List<StoreItem> allItems = itemsForSale.ToList();

        IEnumerable<StoreItem> displayItems = allItems;

        if (IsPaginated(categoryID))
            displayItems = allItems.Skip((currentPage - 1) * ItemsPerPage).Take(ItemsPerPage);

        List<string> productPrices = new ();
        List<string> productIDs = new ();
        List<string> productAlreadyOwned = new ();
        List<string> purchasable = new ();
        List<string> productPremium = new ();
        List<string> premiumMMPCost = new ();
        List<string> productCodes = new ();
        List<string> productLocalContent = new ();
        List<string> productIsBundle = new ();
        List<string> productsRequired = new ();
        List<string> productEligibility = new ();
        int eligibilityIndex = 0;

        foreach (StoreItem item in displayItems)
        {
            int displayPrice = item.GoldCost;

            // 9001-9010 Is A Reserved Price Range For Non-Purchasable Display Items
            if (displayPrice >= 9001 && displayPrice <= 9010 && displayPrice != 9006)
                displayPrice = 9011;

            productPrices.Add(displayPrice.ToString());
            productIDs.Add(item.ID.ToString());
            productAlreadyOwned.Add(user.OwnedStoreItems.Contains(item.PrefixedCode) ? "1" : "0");
            purchasable.Add(item.Purchasable ? "1" : "0");
            productPremium.Add(item.IsPremium ? "1" : "0");
            premiumMMPCost.Add(item.SilverCost.ToString());
            productCodes.Add(item.PrefixedCode);
            productLocalContent.Add(item.Resource);
            productIsBundle.Add(item.IsBundle ? "1" : "0");

            string requiredString = item.Required.Length > 0
                ? string.Join(";", item.Required)
                : "";

            productsRequired.Add(requiredString);

            if (item.Required.Length > 0)
            {
                bool isEligible = item.Required.All(requiredID =>
                    StoreItems.GetByID(requiredID) is StoreItem requiredItem
                        && user.OwnedStoreItems.Contains(requiredItem.PrefixedCode));

                productEligibility.Add(string.Join("~", item.ID, eligibilityIndex, isEligible ? 1 : 0, item.GoldCost, 0, requiredString));
            }

            eligibilityIndex++;
        }

        response["productPrices"] = string.Join("|", productPrices);
        response["productIDs"] = string.Join("|", productIDs);
        response["productAlreadyOwned"] = string.Join("|", productAlreadyOwned);
        response["purchasable"] = string.Join("|", purchasable);
        response["productPremium"] = string.Join("|", productPremium);
        response["premium_mmp_cost"] = string.Join("|", premiumMMPCost);
        response["productCodes"] = string.Join("|", productCodes);
        response["productLocalContent"] = string.Join("|", productLocalContent);
        response["productIsBundle"] = string.Join("|", productIsBundle);
        response["productsRequired"] = string.Join("|", productsRequired);
        response["productEligibility"] = string.Join("|", productEligibility);

        // Always Compute Total Pages Based On All Items, Even For Non-Paginated Categories (The Client Uses This For Client-Side Pagination)
        int totalPages = allItems.Count > 0
            ? (allItems.Count + ItemsPerPage - 1) / ItemsPerPage
            : 1;

        response["totalPages"] = totalPages;
        response["totalPoints"] = user.GoldCoins;
        response["totalMMPoints"] = user.SilverCoins;
        response["categoryID"] = categoryID;
        response["currentPage"] = currentPage;
        response["customAccountIcon"] = 0;
        response["customAccountIconCost"] = 350;
        response["customAccountIconCostMMP"] = 1000;
        response["accountIconsUnlocked"] = 1;
        response["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        response["vaultHighlight"] = "NULL";

        PopulateSelectedUpgrades(account, response);
    }

    /// <summary>
    ///     Populates the response dictionary with the featured heroes listing.
    ///     This format includes individual product data and associated bundle information, matching the response format expected by the client's featured tab (category 68).
    /// </summary>
    private static void PopulateFeaturedItemListing(User user, Account account, Dictionary<string, object> response)
    {
        FeaturedItemsConfiguration featuredConfiguration = JSONConfiguration.FeaturedItemsConfiguration;

        List<string> productNames = [];
        List<string> productIDs = [];
        List<string> productLocalContent = [];
        List<string> productAlreadyOwned = [];
        List<string> productEligibility = [];

        foreach (int itemID in featuredConfiguration.FeaturedItemIDs)
        {
            StoreItem? item = StoreItems.GetByID(itemID);

            if (item is null)
                continue;

            productNames.Add(item.Code);
            productIDs.Add(item.ID.ToString());
            productLocalContent.Add(item.Resource);
            productAlreadyOwned.Add(user.OwnedStoreItems.Contains(item.PrefixedCode) ? "1" : "0");

            if (item.Required.Length > 0)
            {
                bool isEligible = item.Required.All(requiredID =>
                    StoreItems.GetByID(requiredID) is StoreItem requiredItem
                        && user.OwnedStoreItems.Contains(requiredItem.PrefixedCode));

                string requiredString = string.Join(";", item.Required);

                productEligibility.Add(string.Join("~", item.ID, item.Required.Length, isEligible ? 1 : 0, item.GoldCost, 0, requiredString));
            }
        }

        List<string> bundleIDs = [];
        List<string> bundleAlreadyOwned = [];
        List<string> bundleIncludedProducts = [];
        List<string> bundleNames = [];
        List<string> bundleLocalPaths = [];
        List<string> bundleCosts = [];

        foreach (FeaturedBundle bundle in featuredConfiguration.Bundles)
        {
            StoreItem? bundleItem = StoreItems.GetByID(bundle.StoreItemID);

            if (bundleItem is null)
                continue;

            bundleIDs.Add(bundleItem.ID.ToString());
            bundleAlreadyOwned.Add(user.OwnedStoreItems.Contains(bundleItem.PrefixedCode) ? "1" : "0");

            HashSet<int> includedIndices = [.. bundle.IncludedProductIndices];

            // The Client Interprets Each Tilde-Separated Value As A Boolean Flag Per Product Position
            // So We Convert The Zero-Based Indices Into A Positional Mask (e.g. Indices [0, 1] With 2 Products → "1~1")
            string inclusionMask = string.Join("~", Enumerable.Range(0, featuredConfiguration.FeaturedItemIDs.Count)
                .Select(index => includedIndices.Contains(index) ? "1" : "0"));

            bundleIncludedProducts.Add(inclusionMask);
            bundleNames.Add(bundleItem.Name);
            bundleLocalPaths.Add(bundleItem.Resource);
            bundleCosts.Add(bundleItem.GoldCost.ToString());
        }

        response["productPrices"] = string.Empty;
        response["productNames"] = string.Join("|", productNames);
        response["productIDs"] = string.Join("|", productIDs);
        response["productAlreadyOwned"] = string.Join("|", productAlreadyOwned);
        response["productLocalContent"] = string.Join("|", productLocalContent);
        response["productEligibility"] = string.Join("|", productEligibility);

        int placeholderCount = Math.Max(productNames.Count, 3);
        string placeholderPipe = string.Join("|", Enumerable.Repeat("-1", placeholderCount));

        response["productIsBundle"] = string.Join("|", Enumerable.Repeat("1", placeholderCount));
        response["productQuantity"] = placeholderPipe;
        response["productWebContent"] = placeholderPipe;
        response["productDescription"] = placeholderPipe;

        string specialBundleEntry = string.Join("~", Enumerable.Repeat("-1", 5));
        response["specialBundles"] = string.Join("|", Enumerable.Repeat(specialBundleEntry, placeholderCount));

        string chargesEntry = string.Join("~", Enumerable.Repeat("-1", 6));
        response["productCharges"] = string.Join("|", Enumerable.Repeat(chargesEntry, placeholderCount));

        string durationsEntry = string.Join("~", Enumerable.Repeat("-1", 7));
        response["productDurations"] = string.Join("|", Enumerable.Repeat(durationsEntry, placeholderCount));

        response["productTimes"] = string.Join("|", Enumerable.Repeat("0,0", placeholderCount));
        response["purchasable"] = string.Join("|", Enumerable.Repeat("1", placeholderCount));
        response["productPremium"] = string.Join("|", Enumerable.Repeat("0", placeholderCount));
        response["premium_mmp_cost"] = string.Join("|", Enumerable.Repeat("9002", placeholderCount));

        string statsEntry = "-1~~0~0";
        response["productStats"] = string.Join("|", Enumerable.Repeat(statsEntry, placeholderCount));
        response["productEnhancements"] = string.Join("|", Enumerable.Repeat(string.Empty, placeholderCount));
        response["productEnhancementIDs"] = string.Join("|", Enumerable.Repeat(string.Empty, placeholderCount));

        response["bundleIDs"] = string.Join("|", bundleIDs);
        response["bundleAlreadyOwned"] = string.Join("|", bundleAlreadyOwned);
        response["bundleIncludedProducts"] = string.Join("|", bundleIncludedProducts);
        response["bundleNames"] = string.Join("|", bundleNames);
        response["bundleLocalPaths"] = string.Join("|", bundleLocalPaths);
        response["bundleCosts"] = string.Join("|", bundleCosts);

        response["chargesRemaining"] = string.Empty;
        response["durationsRemaining"] = string.Empty;

        response["totalPages"] = 1;
        response["totalPoints"] = user.GoldCoins;
        response["totalMMPoints"] = user.SilverCoins;
        response["categoryID"] = (int) StoreCategory.FeaturedHeroAvatars;
        response["currentPage"] = 1;
        response["customAccountIcon"] = 0;
        response["customAccountIconCost"] = 350;
        response["customAccountIconCostMMP"] = 1000;
        response["accountIconsUnlocked"] = 1;
        response["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        response["vaultHighlight"] = "NULL";

        PopulateSelectedUpgrades(account, response);
    }

    /// <summary>
    ///     Builds a pipe-delimited vault data string for all owned items of the specified type.
    /// </summary>
    private static string CreateVaultData(User user, StoreItemType itemType)
    {
        List<string> entries = new ();

        foreach (string ownedCode in user.OwnedStoreItems)
        {
            StoreItem? item = StoreItems.GetByPrefixedCode(ownedCode);

            if (item is null)
            {
                // Custom Account Icons Are Not In The Store Item List
                if (ownedCode.StartsWith("ai.custom_icon:") && itemType == StoreItemType.AccountIcon)
                {
                    string iconCode = ownedCode[3..];

                    entries.Add(string.Join("`", iconCode, ownedCode, "web"));
                }

                continue;
            }

            if (item.StoreItemType == itemType)
                entries.Add(string.Join("`", item.ID, item.PrefixedCode, item.Resource));
        }

        return string.Join("|", entries);
    }

    /// <summary>
    ///     Populates the response dictionary with the product IDs of the account's selected upgrades.
    /// </summary>
    private static void PopulateSelectedUpgrades(Account account, Dictionary<string, object> response)
    {
        List<string> upgradeIDs = new ();

        foreach (string selectedCode in account.SelectedStoreItems)
        {
            StoreItem? item = StoreItems.GetByPrefixedCode(selectedCode);

            if (item is not null)
                upgradeIDs.Add(item.ID.ToString());

            else if (selectedCode.Contains("custom_icon:"))
                upgradeIDs.Add(selectedCode[3..]);
        }

        response["selectedUpgrades"] = string.Join("|", upgradeIDs);
    }

    /// <summary>
    ///     Maps a store category ID to the corresponding <see cref="StoreItemType"/>.
    /// </summary>
    private static StoreItemType MapStoreCategoryToItemType(int categoryID)
    {
        return categoryID switch
        {
            (int) StoreCategory.HeroAvatars          => StoreItemType.AlternativeAvatar,
            (int) StoreCategory.FeaturedHeroAvatars  => StoreItemType.AlternativeAvatar,
            (int) StoreCategory.AccountIcons         => StoreItemType.AccountIcon,
            (int) StoreCategory.ChatSymbols          => StoreItemType.ChatSymbol,
            (int) StoreCategory.FlagSymbols          => StoreItemType.ChatSymbol,
            (int) StoreCategory.HeroSymbols          => StoreItemType.ChatSymbol,
            (int) StoreCategory.MiscellaneousSymbols => StoreItemType.ChatSymbol,
            (int) StoreCategory.Announcers           => StoreItemType.AnnouncerVoice,
            (int) StoreCategory.Miscellaneous        => StoreItemType.Miscellaneous,
            (int) StoreCategory.Bundles              => StoreItemType.Bundle,
            (int) StoreCategory.NameColors           => StoreItemType.ChatNameColour,
            (int) StoreCategory.Taunt                => StoreItemType.Taunt,
            (int) StoreCategory.Couriers             => StoreItemType.Courier,
            (int) StoreCategory.EarlyAccessHeroes    => StoreItemType.EarlyAccessProduct,
            (int) StoreCategory.Heroes               => StoreItemType.Hero,
            (int) StoreCategory.Wards                => StoreItemType.Ward,
            (int) StoreCategory.Enhancements         => StoreItemType.Enhancement,
            (int) StoreCategory.Creeps               => StoreItemType.Creep,
            (int) StoreCategory.TauntBadges          => StoreItemType.TauntBadge,
            (int) StoreCategory.TeleportationEffects => StoreItemType.TeleportEffect,
            (int) StoreCategory.SelectionCircles     => StoreItemType.SelectionCircle,
            _                                        => StoreItemType.EarlyAccessProduct
        };
    }

    /// <summary>
    ///     Determines whether the specified store category uses pagination.
    /// </summary>
    private static bool IsPaginated(int categoryID)
    {
        return categoryID switch
        {
            (int) StoreCategory.Announcers           => true,
            (int) StoreCategory.Taunt                => true,
            (int) StoreCategory.NameColors           => true,
            (int) StoreCategory.Enhancements         => true,
            (int) StoreCategory.Couriers             => true,
            (int) StoreCategory.TeleportationEffects => true,
            (int) StoreCategory.Wards                => true,
            (int) StoreCategory.Miscellaneous        => true,
            (int) StoreCategory.Bundles              => true,
            _                                        => false
        };
    }

    /// <summary>
    ///     Determines whether the taunt availability status should be included in the response for the given item type.
    /// </summary>
    private static bool ShouldSendTauntStatus(StoreItemType itemType)
    {
        return itemType is StoreItemType.Taunt or StoreItemType.Miscellaneous;
    }

    /// <summary>
    ///     Determines whether purchasing the given item requires the base taunt to be owned first.
    /// </summary>
    private static bool RequiresBaseTaunt(StoreItem storeItem)
    {
        if (storeItem.StoreItemType == StoreItemType.Taunt)
            return storeItem.ID != TauntStoreItemID;

        return storeItem.ID == SuperTauntStoreItemID;
    }

    /// <summary>
    ///     Creates an error response dictionary with the specified error code.
    /// </summary>
    private static Dictionary<string, object> CreateErrorResponse(int errorCode)
    {
        return new Dictionary<string, object>
        {
            ["popupCode"] = (int) StorePopupCode.POP_UP_ERROR_MESSAGE,
            ["errorCode"] = errorCode
        };
    }
}

/// <summary>
///     A list of store item categories
///
///     <code>
///         ..\game\resources0\stringtables\interface_en.str
///         ..\game\resources0\ui\fe2\store2.package
///         ..\game\resources0\ui\scripts\store2.lua
///     </code>
/// </summary>
file enum StoreCategory
{
    New                  = 01,
    HeroAvatars          = 02,
    AccountIcons         = 03,
    ChatSymbols          = 04,
    Announcers           = 05,
    Miscellaneous        = 06,
    Bundles              = 07,
    NameColors           = 16,
    Taunt                = 27,
    Couriers             = 57,
    EarlyAccessHeroes    = 58,
    FlagSymbols          = 59,
    HeroSymbols          = 60,
    MiscellaneousSymbols = 61,
    FeaturedHeroAvatars  = 68,
    Heroes               = 71,
    Wards                = 74,
    Enhancements         = 75,
    Creeps               = 76,
    TauntBadges          = 77,
    TeleportationEffects = 78,
    SelectionCircles     = 79
}

file enum StorePopupCode
{
    // Error Messages
    POP_UP_ERROR_MESSAGE                = 1,

    // Coin Purchase Successful
    POP_UP_COIN_PURCHASE_SUCCESS        = 2,

    // Item Purchase Successful
    POP_UP_PRODUCT_PURCHASE_SUCCESS     = 3,

    // First Visit
    POP_UP_FIRST_VISIT                  = 4,

    // Coin Purchase For Friend Successful
    POP_UP_COIN_PURCHASE_GIFT_SUCCESS   = 5,

    // Redeem Successful
    POP_UP_COIN_REDEEM_SUCCESS          = 6
}

file enum StoreRequestCode
{
    // Get List Of Store Items
    LIST_STORE_ITEMS_REQUEST            = 01,

    // Get List Of Vault Items
    LIST_VAULT_ITEMS_REQUEST            = 02,

    // Attempt To Purchase Points
    ATTEMPT_PURCHASE_COINS_REQUEST      = 03,

    // Attempt To Purchase Item With Points
    ATTEMPT_PURCHASE_PRODUCT_REQUEST    = 04,

    // Get List Of Point Packages
    LIST_POINT_PACKAGE_REQUEST          = 05,

    // Bundle Contents (Using Bundle ID)
    BUNDLE_CONTENT_REQUEST              = 06,

    // Return List Of Product IDs For Every Selected Upgrade
    LIST_PRODUCT_IDS_SELECTED_REQUEST   = 07,

    // Buy Items From Within Game Lobby Using A Mixture Of Coin Types
    BUY_PRODUCT_GAME_LOBBY_REQUEST      = 08,

    // Get Quantity For Product IDs
    GET_QUANTITY_FOR_PRODUCT_REQUEST    = 09,

    // Redeem Code
    REDEEM_CODE_REQUEST                 = 10
}

file enum StoreResponseCode
{
    // Do Nothing (Primarily Accompanied By A Popup Code)
    NO_RESPONSE                         = 0,

    // Basic Item List
    BASIC_ITEM_LIST_RESPONSE            = 1,

    // Vault Item List
    VAULT_ITEM_LIST_RESPONSE            = 2,

    // Attempt To Purchase Coins
    ATTEMPT_PURCHASE_COINS_RESPONSE     = 3,

    // Attempt To Purchase Item
    ATTEMPT_PURCHASE_PRODUCT_RESPONSE   = 4,

    // Point Package List
    POINT_PACKAGE_RESPONSE              = 5,

    // Vault Avatar List
    VAULT_AVATAR_LIST_RESPONSE          = 6
}

file enum StoreErrorCode
{
    // Invalid Request
    STORE_INVALID_REQUEST_ERROR                     = 01,

    // Invalid Account Information
    STORE_ACCOUNT_INFORMATION_ERROR                 = 02,

    // Unable To Load Account Information
    STORE_ACCOUNT_LOAD_ERROR                        = 03,

    // Unable To Establish A Secure Connection
    STORE_SECURE_CONNECTION_ERROR                   = 04,

    // Unable To Load Store Items
    STORE_ITEMS_ERROR                               = 05,

    // Unable To Load Vault Items
    STORE_LOAD_VAULT_ITEMS_ERROR                    = 06,

    // Unable To Load Point Packages
    STORE_LOAD_POINT_PACKAGE_ERROR                  = 07,

    // Invalid Point Package Selection
    STORE_POINT_PACKAGE_INVALID_ERROR               = 08,

    // Invalid Credit Card Information
    PURCHASE_INVALID_CCARD_ERROR                    = 09,

    // Unable To Process Credit Card Payment
    PURCHASE_CARD_ERROR                             = 10,

    // Unable To Purchase From This Region
    PURCHASE_REGION_ERROR                           = 11,

    // Unable To Use Entered Promo Code
    INVALID_PROMO_CODE_ERROR                        = 12,

    // The Transaction Has Been Declined
    PURCHASE_TRANSACTION_DECLINED_ERROR             = 13,

    // Unable To Log Transaction And Complete Payment
    INVOICE_SYSTEM_ERROR                            = 14,

    // Connection Error While Communicating With Payment Processor
    PURCHASE_SYSTEM_CONNECTION_ERROR                = 15,

    // Credit Card Transaction Failed
    PURCHASE_TRANSACTION_FAILED_ERROR               = 16,

    // The Purchase System Is Unavailable
    PURCHASE_SYSTEM_DOWN_ERROR                      = 17,

    // Inactive Or Disabled Accounts Are Not Permitted To Access The Store
    PURCHASE_DISABLED_ACCOUNT_ERROR                 = 18,

    // Unable To Distribute Points To Your Account
    INVOICE_SYSTEM_DISTRIBUTE_ERROR                 = 19,

    // Unable To Validate Session
    STORE_SESSION_ERROR                             = 20,

    // You Do Not Have Enough Points To Make This Purchase
    STORE_PURCHASE_POINT_ERROR                      = 21,

    // Unable To Purchase This Item
    STORE_PURCHASE_ERROR                            = 22,

    // The Requested Item Is Not Available For Purchase At This Time
    STORE_PURCHASE_ITEM_ERROR                       = 23,

    // You Have Requested An Invalid Or Non-Existent Item
    STORE_PURCHASE_ITEM_MISSING_ERROR               = 24,

    // You Must Specify An Item To Purchase
    STORE_PURCHASE_SPECIFY_ITEM_ERROR               = 25,

    // Invalid Friend Account Name
    STORE_PURCHASE_FRIEND_ERROR                     = 26,

    // Invalid Bundle ID
    STORE_PURCHASE_BUNDLE_ID_ERROR                  = 27,

    // No Items Found For This Bundle
    STORE_PURCHASE_BUNDLE_NO_ITEMS_ERROR            = 28,

    // Unable To Retrieve Bundle Data
    STORE_PURCHASE_BUNDLE_FETCH_ERROR               = 29,

    // Unable To Retrieve List Of Selected Upgrades
    STORE_PURCHASE_SELECT_UPGRADES_LIST_ERROR       = 30,

    // Unable To Retrieve Item Information
    STORE_PURCHASE_RETRIEVE_ITEM_ERROR              = 31,

    // Unable To Load Item For Purchase
    STORE_PURCHASE_LOAD_ITEM_ERROR                  = 32,

    // Selected Item Is Already Owned
    STORE_PURCHASE_ALREADY_OWNED                    = 33,

    // May Not Be Purchased Unless You Unlock Account Icons First
    STORE_PURCHASE_MUST_UNLOCK_ERROR                = 34,

    // Unable To Determine If The Item Is Already Owned
    STORE_PURCHASE_OWNED_AMBIGUITY_ERROR            = 35,

    // Unable To Determine Whether Or Not Account Icons Are Unlocked
    STORE_PURCHASE_ICON_LOCKED_ERROR                = 36,

    // Unable To Load List Of Purchased Items
    STORE_PURCHASE_LOAD_LIST_ERROR                  = 37,

    // The Store Is Closed At This Time
    STORE_PURCHASE_DISABLED_ERROR                   = 38,

    // VIP Exclusive
    STORE_PURCHASE_VIP_ONLY_ERROR                   = 39,

    // Your Santa Already Has This Product
    STORE_PURCHASE_SANTA_OWNED_PRODUCT_ERROR        = 40,

    // Your Santa Has Not Unlocked Taunt
    STORE_PURCHASE_SANTA_TAUNT_LOCK_ERROR           = 41,

    // Your Santa Did Not Receive The Gift
    STORE_PURCHASE_SANTA_GIFT_NOT_RECEIVED_ERROR    = 42,

    // No Santas Left
    STORE_PURCHASE_SANTA_NO_MORE_QTY_ERROR          = 43,

    // Product Not Available To Santa
    STORE_PURCHASE_SANTA_PRODUCT_UNAVAILABLE_ERROR  = 44,

    // This Code Has Already Been Used
    STORE_PURCHASE_CODE_USED_ERROR                  = 45,

    // You Can Only Use This Promotion Once
    STORE_PURCHASE_CODE_USED_ONCE_ERROR             = 46,

    // Invalid Code
    STORE_PURCHASE_CODE_INVALID_ERROR               = 47
}
