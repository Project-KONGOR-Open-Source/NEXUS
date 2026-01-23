namespace KONGOR.MasterServer.Controllers.Store;

[ApiController]
[Route("store_requester.php")]
[Consumes("application/x-www-form-urlencoded")]
public class StoreController(MerrickContext databaseContext, ILogger<StoreController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger Logger { get; } = logger;

    [HttpPost(Name = "Store Requester")]
    public async Task<IActionResult> StoreRequester()
    {
        //IFormCollection form = await Request.ReadFormAsync();
        //IQueryCollection query = Request.Query;

        //string _1 = JsonSerializer.Serialize(form);
        //string _2 = JsonSerializer.Serialize(query);

        return Ok();
    }
}

/// <summary>
///     A list of store item categories, extrapolated from the game clients's resource files.
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
    EearlyAccessHeroes   = 58,
    FlagSymbols          = 59,
    HeroSymbols          = 60,
    FeaturedHeroes       = 68,
    Heroes               = 71,
    Wards                = 74,
    Enhancements         = 75,
    TeleportationEffects = 78,
    SelectionCircles     = 79
}

/*
     public static $categoryIDs = array(
        'Alt Avatar' => 2,
        'Account Icon' => 3,
        'Chat Symbol' => 4,
        'Chat Color' => 16,
        'Alt Announcement' => 5,
        'Misc' => 6,
        'Taunt' => 27,
        'Alt Avatars' => 56,
        'Couriers' => 57,
        'EAP' => 58,
        'Flags' => 59,
        'Hero Symbols' => 60,
        'Misc Symbols' => 61,
        'Heroes' => 71,
        'Bundles' => 7,
        'Featured Hero Alts' => 68,
        'Ward' => 74,
        'Enhancement' => 75,
        'Creep' => 76,
        'Taunt Badge' => 77,
        'TP Effect' => 78,
        'Selection Circle' => 79,
    );
*/

/// <summary>
///     A list of store popup codes, extrapolated from the game clients's resource files.
///     
///     <code>
///         ..\game\resources0\ui\scripts\store.lua
///         ..\game\resources0\ui\scripts\store2.lua
///     </code>
/// </summary>
file enum StorePopupCode
{
    Success                 = -1,
    Error                   =  1,
    CoinPurchaseSuccess     =  2,
    ItemPurchaseSuccess     =  3,
    FirstVisit              =  4,
    CoinGiftSuccess         =  5,
    CouponActivationSuccess =  6
}

/*
    // Popup codes are as follows: (popupCode)
    const POP_UP_ERROR_MESSAGE = 1;
    // 1 - Error messages (display errorCode)
    const POP_UP_COIN_PURCHASE_SUCCESS = 2;
    // 2 - Coin Purchase Successful
    const POP_UP_PRODUCT_PURCHASE_SUCCESS = 3;
    // 3 - Item Purchase Successful
    const POP_UP_FIRST_VISIT = 4;
    // 4 - First visit
    const POP_UP_COIN_PURCHASE_GIFT_SUCCESS = 5;
    // 5 - Coin purchase for friend successful
    const POP_UP_COIN_REDEEM_SUCCESS = 6; // 6 - Redeem Successful
 */

file enum StoreRequestCode
{
    /*
    // Requires one of the following request codes: (request_code). Tells the script what the client wants (not necessarily what it'll get)
    const LIST_STORE_ITEMS_REQUEST = 1;
    // 1 - Get list of store items (returns new if no category)
    const LIST_VAULT_ITEMS_REQUEST = 2;
    // 2 - Get list of vault items (default to (?) category).
    const ATTEMPT_PURCHASE_COINS_REQUEST = 3;
    // 3 - Attempt to purchase points
    const ATTEMPT_PURCHASE_PRODUCT_REQUEST = 4;
    // 4 - Attempt to purchase item with points
    const LIST_POINT_PACKAGE_REQUEST = 5;
    // 5 - Get list of point packages
    const BUNDLE_CONTENT_REQUEST = 6;
    // 6 - Bundle contents (using bundle_id)
    const LIST_PRODUCT_IDS_SELECTED_REQUEST = 7;
    // 7 - Return list of product id's for every selected upgrade
    const BUY_PRODUCT_GAME_LOBBY_REQUEST = 8;
    // 8 - Buy items from within game lobby using a mixture of coin types
    const GET_QUANTITY_FOR_PRODUCT_REQUEST = 9;
    // 9 - Get quantity for product_id(s)
    const REDEEM_CODE_REQUEST = 10; // 10 - Redeem code
     */
}

file enum StoreResponseCode
{
    /*
         // Response codes are as follows: (responseCode). Response codes tell the client what to do with the processed data (if anything)
    const NO_RESPONSE = 0;
    // 0 - Do nothing (aka error/completion/etc) - primarily accompanied by a popupCode.
    const BASIC_ITEM_LIST_RESPONSE = 1;
    // 1 - Basic item list
    const VAULT_ITEM_LIST_RESPONSE = 2;
    // 2 - Vault item list
    const ATTEMPT_PURCHASE_COINS_RESPONSE = 3;
    // 3 - Attempt to purchase coins
    const ATTEMPT_PURCHASE_PRODUCT_RESPONSE = 4;
    // 4 - Attempt to purchase item
    const POINT_PACKAGE_RESPONSE = 5;
    // 5 - Point package list
    const VAULT_AVATAR_LIST_RESPONSE = 6; // 6 - Vault avatar list
     */
}

file enum StoreErrorCode
{
    /*
         const STORE_INVALID_REQUEST_ERROR = 1;
    // 1 - Invalid Request
    const STORE_ACCOUNT_INFORMATION_ERROR = 2;
    // 2 - Invalid Account Information
    const STORE_ACCOUNT_LOAD_ERROR = 3;
    // 3 - Unable to load account information
    const STORE_SECURE_CONNECTION_ERROR = 4;
    // 4 - Unable to establish a secure connection
    const STORE_ITEMS_ERROR = 5;
    // - Unable to load store items
    const STORE_LOAD_VAULT_ITEMS_ERROR = 6;
    // - Unable to load vault items
    const STORE_LOAD_POINT_PACKAGE_ERROR = 7;
    // 7 - Unable to load point packages
    const STORE_POINT_PACKAGE_INVALID_ERROR = 8;
    // 8 - Invalid point package selection

    // CORE PURCHASE SYSTEM
    const PURCHASE_INVALID_CCARD_ERROR = 9;
    // 9 - Invalid credit card information
    const PURCHASE_CARD_ERROR = 10;
    // 10 - Unable to process credit card payment
    const PURCHASE_REGION_ERROR = 11;
    // 11 - Purchasing for the following countries will be handled through a third party affiliate: Vietnam, Taiwan, Malaysia, Phillipines, and Russia. Look for more information in the near future.
    const PURCHASE_TRANSACTION_DECLINED_ERROR = 13;
    // 13 - Your transaction has been declined. Please contact your credit card provider.
    const PURCHASE_SYSTEM_CONNECTION_ERROR = 15;
    // 15 - Connection error - please try again (also for authnet issues).\
    const PURCHASE_TRANSACTION_FAILED_ERROR = 16;
    // 16 - Credit card transaction failed. Please contact support for assistance. (unknown error from authnet - using response[3] from authnet).
    const PURCHASE_SYSTEM_DOWN_ERROR = 17;
    // 17 - Store is currently down for maintenance (or just generic unavailable. I don't care).
    const PURCHASE_DISABLED_ACCOUNT_ERROR = 18;
    // 18 - Inactive or Disabled accounts are not permitted to access the store.
    const INVALID_PROMO_CODE_ERROR = 12;
    // 12 - Unable to use entered promo code.
    const INVOICE_SYSTEM_ERROR = 14;
    // 14 - Unable to log transaction and complete payment. Please contact support (invoice creation failed).
    const INVOICE_SYSTEM_DISTRIBUTE_ERROR = 19;
    // 19 - Unable to distribute points to your account.
    const STORE_SESSION_ERROR = 20;
    // 20 - Unable to validate session. Please log out and back in. If the problem persists, please contact support for assistance.
    const STORE_PURCHASE_POINT_ERROR = 21;
    // 21 - You do not have enough points to make this purchase. (generally should never happen).
    const STORE_PURCHASE_ERROR = 22;
    // 22 - Unable to purchase this item. Please contact support support for assistance (extremely rare).
    const STORE_PURCHASE_ITEM_ERROR = 23;
    // 23 - The requested item is not available for purchase at this time.
    const STORE_PURCHASE_ITEM_MISSING_ERROR = 24;
    // 24 - You have requested an invalid or nonexistent item.
    const STORE_PURCHASE_SPECIFY_ITEM_ERROR = 25;
    // 25 - You must specify an item to purchase.
    const STORE_PURCHASE_FRIEND_ERROR = 26;
    // 26 - Invalid friend nickname (should be very rare/never happen)
    const STORE_PURCHASE_BUNDLE_ID_ERROR = 27;
    // 27 - Invaild bundle ID.
    const STORE_PURCHASE_BUNDLE_NO_ITEMS_ERROR = 28;
    // 28 - No items found for this bundle.
    const STORE_PURCHASE_BUNDLE_FETCH_ERROR = 29;
    // 29 - Unable to retrieve bundle data.
    const STORE_PURCHASE_SELECT_UPGRADES_LIST_ERROR = 30;
    // 30 - Unable to retrieve list of selected upgrades
    const STORE_PURCHASE_RETRIEVE_ITEM_ERROR = 31;
    // 31 - Unable to retrieve item information.
    const STORE_PURCHASE_LOAD_ITEM_ERROR = 32;
    // 32 - Unable to load item for purchase.
    const STORE_PURCHASE_ALREADY_OWNED = 33;
    // 33 - Selected item is already owned
    const STORE_PURCHASE_MUST_UNLOCK_ERROR = 34;
    // 34 - May not be purchased unless you unlock account icons first
    const STORE_PURCHASE_OWNED_AMBIGUITY_ERROR = 35;
    // 35 - Unable to determine if the item is already owned
    const STORE_PURCHASE_ICON_LOCKED_ERROR = 36;
    // 36 - Unable to determine whether or not account icons are unlocked
    const STORE_PURCHASE_LOAD_LIST_ERROR = 37;
    // 37 - Unable to load list of purchased items.
    const STORE_PURCHASE_DISABLED_ERROR = 38;
    // 38 - The store is closed at this time (live = 0).
    const STORE_PURCHASE_VIP_ONLY_ERROR = 39;
    // 39 - VIP Exclusive to purchase a VIP Account, please visit http://shop.garena.com/hon/.
    const STORE_PURCHASE_SANTA_OWNED_PRODUCT_ERROR = 40;
    // 40 - Your santa already has this product.
    const STORE_PURCHASE_SANTA_TAUNT_LOCK_ERROR = 41;
    // 41 - Your santa has not unlocked taunt.
    const STORE_PURCHASE_SANTA_GIFT_NOT_RECEIVED_ERROR = 42;
    // 42 - Your sant didnt' get his stuff =(
    const STORE_PURCHASE_SANTA_NO_MORE_QTY_ERROR = 43;
    // 43 - NO SANTAS LEFT
    const STORE_PURCHASE_SANTA_PRODUCT_UNAVAILABLE_ERROR = 44;
    // 44 - Product not available to santa
    const STORE_PURCHASE_CODE_USED_ERROR = 45;
    // 45 - This code has already been used.
    const STORE_PURCHASE_CODE_USED_ONCE_ERROR = 46;
    // 46 - You can only use this promotion once.
    const STORE_PURCHASE_CODE_INVALID_ERROR = 47; // 47 - Invalid code.
     */
}