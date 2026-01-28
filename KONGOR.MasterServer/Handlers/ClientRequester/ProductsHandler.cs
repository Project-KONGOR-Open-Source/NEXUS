using KONGOR.MasterServer.Services.Requester;
using KONGOR.MasterServer.Services.Store; // For StaticCatalog and Product
// For PhpSerialization

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public partial class ProductsHandler(IDatabase distributedCache, ILogger<ProductsHandler> logger) : IClientRequestHandler
{
    private IDatabase DistributedCache { get; } = distributedCache;
    private ILogger Logger { get; } = logger;

    [LoggerMessage(Level = LogLevel.Warning, Message = "[Products] Missing cookie.")]
    private partial void LogMissingCookie();

    [LoggerMessage(Level = LogLevel.Warning, Message = "[Products] Invalid session for cookie '{Cookie}'")]
    private partial void LogInvalidSession(string cookie);

    [LoggerMessage(Level = LogLevel.Information, Message = "[Products] Returning products for '{SessionAccountName}'.")]
    private partial void LogReturningProducts(string sessionAccountName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "[Products] Response Size: {Length}")]
    private partial void LogResponseSize(int length);

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
            LogInvalidSession(cookie);
            return new UnauthorizedObjectResult($@"No Session Found For Cookie ""{cookie}""");
        }

        LogReturningProducts(sessionAccountName);

        // Map StaticCatalog.Products to the legacy GetProductsResponse structure: Category -> ID -> Product
        Dictionary<string, Dictionary<int, Dictionary<string, object>>> productsResponse = new();

        foreach (Product product in StaticCatalog.Products)
        {
            string category = product.Type switch
            {
                "Hero" => "Hero",
                "AccountIcon" => "Icons",
                _ => "Misc"
            };

            if (!productsResponse.TryGetValue(category, out Dictionary<int, Dictionary<string, object>>? value))
            {
                value = new Dictionary<int, Dictionary<string, object>>();
                productsResponse[category] = value;
            }

            int id = Math.Abs(product.ProductCode.GetHashCode());

            value[id] = new Dictionary<string, object>
            {
                ["name"] = product.ProductCode,
                ["cname"] = product.Name,
                ["cost"] = product.CostGold,
                ["purchasable"] = true,
                ["premium"] = false,
                ["premium_mmp_cost"] = product.CostSilver,
                ["dynamic"] = 0,
                ["local_path"] = product.IconPath
            };
        }

        Dictionary<string, object> response = new() { ["products"] = productsResponse };

        string serialized = PhpSerialization.Serialize(response);
        LogResponseSize(serialized.Length);

        return new OkObjectResult(serialized);
    }
}