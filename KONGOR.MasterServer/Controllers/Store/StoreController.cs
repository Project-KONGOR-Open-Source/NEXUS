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
        IFormCollection form = await Request.ReadFormAsync();
        //IQueryCollection query = Request.Query;

        //string _1 = JsonSerializer.Serialize(form);
        //string _2 = JsonSerializer.Serialize(query);

        if (!Enum.TryParse(form["request_code"], out StoreRequestCode requestCode))
        {
            return Ok();
        }

        switch (requestCode)
        {
            case StoreRequestCode.LIST_STORE_ITEMS_REQUEST:
                return Ok(PhpSerialization.Serialize(new Dictionary<string, object>()));
            default:
                return Ok();
        }

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
    New = 01,
    HeroAvatars = 02,
    AccountIcons = 03,
    ChatSymbols = 04,
    Announcers = 05,
    Miscellaneous = 06,
    Bundles = 07,
    NameColors = 16,
    Taunt = 27,
    Couriers = 57,
    EarlyAccessHeroes = 58,
    FlagSymbols = 59,
    HeroSymbols = 60,
    MiscellaneousSymbols = 61,
    FeaturedHeroAvatars = 68,
    Heroes = 71,
    Wards = 74,
    Enhancements = 75,
    Creeps = 76,
    TauntBadges = 77,
    TeleportationEffects = 78,
    SelectionCircles = 79
}

file enum StorePopupCode
{
    // Error Messages
    POP_UP_ERROR_MESSAGE = 1,

    // Coin Purchase Successful
    POP_UP_COIN_PURCHASE_SUCCESS = 2,

    // Item Purchase Successful
    POP_UP_PRODUCT_PURCHASE_SUCCESS = 3,

    // First Visit
    POP_UP_FIRST_VISIT = 4,

    // Coin Purchase For Friend Successful
    POP_UP_COIN_PURCHASE_GIFT_SUCCESS = 5,

    // Redeem Successful
    POP_UP_COIN_REDEEM_SUCCESS = 6
}

file enum StoreRequestCode
{
    // Get List Of Store Items
    LIST_STORE_ITEMS_REQUEST = 01,

    // Get List Of Vault Items
    LIST_VAULT_ITEMS_REQUEST = 02,

    // Attempt To Purchase Points
    ATTEMPT_PURCHASE_COINS_REQUEST = 03,

    // Attempt To Purchase Item With Points
    ATTEMPT_PURCHASE_PRODUCT_REQUEST = 04,

    // Get List Of Point Packages
    LIST_POINT_PACKAGE_REQUEST = 05,

    // Bundle Contents (Using Bundle ID)
    BUNDLE_CONTENT_REQUEST = 06,

    // Return List Of Product IDs For Every Selected Upgrade
    LIST_PRODUCT_IDS_SELECTED_REQUEST = 07,

    // Buy Items From Within Game Lobby Using A Mixture Of Coin Types
    BUY_PRODUCT_GAME_LOBBY_REQUEST = 08,

    // Get Quantity For Product IDs
    GET_QUANTITY_FOR_PRODUCT_REQUEST = 09,

    // Redeem Code
    REDEEM_CODE_REQUEST = 10
}

file enum StoreResponseCode
{
    // Do Nothing (Primarily Accompanied By A Popup Code)
    NO_RESPONSE = 0,

    // Basic Item List
    BASIC_ITEM_LIST_RESPONSE = 1,

    // Vault Item List
    VAULT_ITEM_LIST_RESPONSE = 2,

    // Attempt To Purchase Coins
    ATTEMPT_PURCHASE_COINS_RESPONSE = 3,

    // Attempt To Purchase Item
    ATTEMPT_PURCHASE_PRODUCT_RESPONSE = 4,

    // Point Package List
    POINT_PACKAGE_RESPONSE = 5,

    // Vault Avatar List
    VAULT_AVATAR_LIST_RESPONSE = 6
}

file enum StoreErrorCode
{
    // Invalid Request
    STORE_INVALID_REQUEST_ERROR = 01,

    // Invalid Account Information
    STORE_ACCOUNT_INFORMATION_ERROR = 02,

    // Unable To Load Account Information
    STORE_ACCOUNT_LOAD_ERROR = 03,

    // Unable To Establish A Secure Connection
    STORE_SECURE_CONNECTION_ERROR = 04,

    // Unable To Load Store Items
    STORE_ITEMS_ERROR = 05,

    // Unable To Load Vault Items
    STORE_LOAD_VAULT_ITEMS_ERROR = 06,

    // Unable To Load Point Packages
    STORE_LOAD_POINT_PACKAGE_ERROR = 07,

    // Invalid Point Package Selection
    STORE_POINT_PACKAGE_INVALID_ERROR = 08,

    // Invalid Credit Card Information
    PURCHASE_INVALID_CCARD_ERROR = 09,

    // Unable To Process Credit Card Payment
    PURCHASE_CARD_ERROR = 10,

    // Unable To Purchase From This Region
    PURCHASE_REGION_ERROR = 11,

    // Unable To Use Entered Promo Code
    INVALID_PROMO_CODE_ERROR = 12,

    // The Transaction Has Been Declined
    PURCHASE_TRANSACTION_DECLINED_ERROR = 13,

    // Unable To Log Transaction And Complete Payment
    INVOICE_SYSTEM_ERROR = 14,

    // Connection Error While Communicating With Payment Processor
    PURCHASE_SYSTEM_CONNECTION_ERROR = 15,

    // Credit Card Transaction Failed
    PURCHASE_TRANSACTION_FAILED_ERROR = 16,

    // The Purchase System Is Unavailable
    PURCHASE_SYSTEM_DOWN_ERROR = 17,

    // Inactive Or Disabled Accounts Are Not Permitted To Access The Store
    PURCHASE_DISABLED_ACCOUNT_ERROR = 18,

    // Unable To Distribute Points To Your Account
    INVOICE_SYSTEM_DISTRIBUTE_ERROR = 19,

    // Unable To Validate Session
    STORE_SESSION_ERROR = 20,

    // You Do Not Have Enough Points To Make This Purchase
    STORE_PURCHASE_POINT_ERROR = 21,

    // Unable To Purchase This Item
    STORE_PURCHASE_ERROR = 22,

    // The Requested Item Is Not Available For Purchase At This Time
    STORE_PURCHASE_ITEM_ERROR = 23,

    // You Have Requested An Invalid Or Non-Existent Item
    STORE_PURCHASE_ITEM_MISSING_ERROR = 24,

    // You Must Specify An Item To Purchase
    STORE_PURCHASE_SPECIFY_ITEM_ERROR = 25,

    // Invalid Friend Account Name
    STORE_PURCHASE_FRIEND_ERROR = 26,

    // Invalid Bundle ID
    STORE_PURCHASE_BUNDLE_ID_ERROR = 27,

    // No Items Found For This Bundle
    STORE_PURCHASE_BUNDLE_NO_ITEMS_ERROR = 28,

    // Unable To Retrieve Bundle Data
    STORE_PURCHASE_BUNDLE_FETCH_ERROR = 29,

    // Unable To Retrieve List Of Selected Upgrades
    STORE_PURCHASE_SELECT_UPGRADES_LIST_ERROR = 30,

    // Unable To Retrieve Item Information
    STORE_PURCHASE_RETRIEVE_ITEM_ERROR = 31,

    // Unable To Load Item For Purchase
    STORE_PURCHASE_LOAD_ITEM_ERROR = 32,

    // Selected Item Is Already Owned
    STORE_PURCHASE_ALREADY_OWNED = 33,

    // May Not Be Purchased Unless You Unlock Account Icons First
    STORE_PURCHASE_MUST_UNLOCK_ERROR = 34,

    // Unable To Determine If The Item Is Already Owned
    STORE_PURCHASE_OWNED_AMBIGUITY_ERROR = 35,

    // Unable To Determine Whether Or Not Account Icons Are Unlocked
    STORE_PURCHASE_ICON_LOCKED_ERROR = 36,

    // Unable To Load List Of Purchased Items
    STORE_PURCHASE_LOAD_LIST_ERROR = 37,

    // The Store Is Closed At This Time
    STORE_PURCHASE_DISABLED_ERROR = 38,

    // VIP Exclusive
    STORE_PURCHASE_VIP_ONLY_ERROR = 39,

    // Your Santa Already Has This Product
    STORE_PURCHASE_SANTA_OWNED_PRODUCT_ERROR = 40,

    // Your Santa Has Not Unlocked Taunt
    STORE_PURCHASE_SANTA_TAUNT_LOCK_ERROR = 41,

    // Your Santa Did Not Receive The Gift
    STORE_PURCHASE_SANTA_GIFT_NOT_RECEIVED_ERROR = 42,

    // No Santas Left
    STORE_PURCHASE_SANTA_NO_MORE_QTY_ERROR = 43,

    // Product Not Available To Santa
    STORE_PURCHASE_SANTA_PRODUCT_UNAVAILABLE_ERROR = 44,

    // This Code Has Already Been Used
    STORE_PURCHASE_CODE_USED_ERROR = 45,

    // You Can Only Use This Promotion Once
    STORE_PURCHASE_CODE_USED_ONCE_ERROR = 46,

    // Invalid Code
    STORE_PURCHASE_CODE_INVALID_ERROR = 47
}
