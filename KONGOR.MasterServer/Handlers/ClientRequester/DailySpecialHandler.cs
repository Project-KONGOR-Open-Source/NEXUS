using KONGOR.MasterServer.Services.Requester;
using KONGOR.MasterServer.Services.Store; // For StaticCatalog, DailySpecialDefinition
// For PhpSerialization

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public partial class DailySpecialHandler(
    MerrickContext databaseContext,
    IDatabase distributedCache,
    ILogger<DailySpecialHandler> logger) : IClientRequestHandler
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IDatabase DistributedCache { get; } = distributedCache;
    private ILogger Logger { get; } = logger;

    [LoggerMessage(Level = LogLevel.Warning, Message = "[DailySpecial] Missing cookie.")]
    private partial void LogMissingCookie();

    [LoggerMessage(Level = LogLevel.Information, Message = "[DailySpecial] Fetching specials for '{SessionAccountName}'...")]
    private partial void LogFetchingSpecials(string sessionAccountName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[DailySpecial] Account '{SessionAccountName}' not found.")]
    private partial void LogAccountNotFound(string sessionAccountName);

    public async Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        HttpRequest Request = context.Request;
        string? cookie = ClientRequestHelper.GetCookie(Request);
        if (string.IsNullOrEmpty(cookie))
        {
            LogMissingCookie();
            return new UnauthorizedResult();
        }

        cookie = cookie.Replace("-", string.Empty);

        string? sessionAccountName = context.Items["SessionAccountName"] as string;

        if (sessionAccountName is null)
        {
            (bool accountSessionCookieIsValid, string? cacheAccountName) =
                await DistributedCache.ValidateAccountSessionCookie(cookie);
            if (accountSessionCookieIsValid)
            {
                sessionAccountName = cacheAccountName;
            }
        }

        if (sessionAccountName is null)
        {
            return new UnauthorizedObjectResult($@"No Session Found For Cookie ""{cookie}""");
        }

        LogFetchingSpecials(sessionAccountName);

        Account? account = await MerrickContext.Accounts
            .Include(a => a.User)
            .SingleOrDefaultAsync(a => a.Name == sessionAccountName);

        if (account == null)
        {
            LogAccountNotFound(sessionAccountName);
            return new UnauthorizedResult();
        }

        List<Dictionary<string, object>> dailyItems = [];

        foreach (StaticCatalog.DailySpecialDefinition special in StaticCatalog.DailySpecials)
        {
            Product? product = StaticCatalog.Products.FirstOrDefault(p => p.ProductCode == special.ProductCode);
            if (product == null)
            {
                continue;
            }

            int currentGold = (int) (product.CostGold * (100 - special.DiscountOff) / 100.0);
            int currentSilver = (int) (product.CostSilver * (100 - special.DiscountSilver) / 100.0);

            bool isOwned = account.User.OwnedStoreItems.Contains(product.ProductCode);

            string heroName = "";
            if (product.Type == "Alt Avatar" && product.ProductCode.Contains('_') && product.ProductCode.Contains('.'))
            {
                // Logic for extracting hero name if needed
            }

            Dictionary<string, object> itemDict = new()
            {
                ["panel_index"] = special.PanelIndex,
                ["product_id"] = product.ProductCode.GetHashCode(),
                ["hero_name"] = heroName,
                ["item_name"] = product.ProductCode,
                ["item_cname"] = product.Name,
                ["item_type"] = product.Type,
                ["purchasable"] = true,
                ["item_code"] = product.ProductCode,
                ["local_content"] = "",
                ["plinko_only"] = 0,
                ["gold_coins"] = product.CostGold,
                ["silver_coins"] = product.CostSilver,
                ["discount_off"] = special.DiscountOff,
                ["discount_silver"] = special.DiscountSilver,
                ["current_gold_coins"] = currentGold,
                ["current_silver_coins"] = currentSilver,
                ["tag_title"] = special.TagTitle,
                ["tag_date_limit"] = special.TagDateLimit,
                ["item_page"] = 0,
                ["is_owned"] = isOwned
            };

            dailyItems.Add(itemDict);
        }

        Dictionary<string, object> response = new() { ["list"] = dailyItems, ["0"] = true, ["vested_threshold"] = 5 };

        return new OkObjectResult(PhpSerialization.Serialize(response));
    }
}