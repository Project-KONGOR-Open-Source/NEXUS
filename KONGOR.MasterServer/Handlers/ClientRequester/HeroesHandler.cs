using System.Globalization;

using KONGOR.MasterServer.Services.Requester;
using KONGOR.MasterServer.Services.Store; // For StaticCatalog and Product
// For PhpSerialization

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public partial class HeroesHandler(ILogger<HeroesHandler> logger) : IClientRequestHandler
{
    private ILogger Logger { get; } = logger;

    [LoggerMessage(Level = LogLevel.Information, Message = "[Heroes] Request received for hero list.")]
    private partial void LogRequestReceived();

    [LoggerMessage(Level = LogLevel.Warning, Message = "[Heroes] Catalog is empty. Seeding defaults.")]
    private partial void LogCatalogEmpty();

    [LoggerMessage(Level = LogLevel.Information, Message = "[Heroes] Returning {Count} heroes.")]
    private partial void LogReturningHeroes(int count);

    public Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        LogRequestReceived();

        Dictionary<string, object> heroes = new();

        foreach (Product product in StaticCatalog.Products)
        {
            if (product.Type == "Hero")
            {
                string heroName = product.ProductCode.Replace("h.", "Hero_");
                string pascalCaseName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(heroName.Split('_').Last());
                string legacyIdentifier = $"Hero_{pascalCaseName}";

                int id = Math.Abs(product.ProductCode.GetHashCode());
                heroes[id.ToString(CultureInfo.InvariantCulture)] = legacyIdentifier;
            }
        }

        if (heroes.Count == 0)
        {
            LogCatalogEmpty();
            heroes["1"] = "Hero_Scout";
            heroes["2"] = "Hero_Maliken";
            heroes["3"] = "Hero_Pyromancer";
        }
        else
        {
            LogReturningHeroes(heroes.Count);
        }

        string response = PhpSerialization.Serialize(heroes);
        return Task.FromResult<IActionResult>(new OkObjectResult(response));
    }
}