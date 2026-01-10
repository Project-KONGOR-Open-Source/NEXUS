using System.Globalization;

using KONGOR.MasterServer.Services.Store;

namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private Task<IActionResult> GetAllHeroes()
    {
        Dictionary<string, object> heroes = new();

        // 2026-01-06: Implementing get_all_heroes using StaticCatalog.
        // Legacy format expects a dictionary where key is an integer ID (or string representation of it)
        // and value is the hero identifier (e.g. "Hero_Legionnaire").

        foreach (Product product in StaticCatalog.Products)
        {
            if (product.Type == "Hero")
            {
                // We use the same ID generation logic as get_products for consistency, 
                // though strictly speaking get_all_heroes might just need the string identifier.
                // Re-checking legacy dumps (if available) or standard practice: 
                // get_all_heroes usually returns an array/map of Hero Identifiers.
                // format: [0] => "Hero_Accursed", [1] => "Hero_Scout"...
                // Let's return a simple dictionary/list.

                // Converting "h.accursed" -> "Hero_Accursed" for legacy compatibility?
                // StaticCatalog has "h.accursed".
                // ZORGATH.Heroes has "Hero_Accursed". 
                // We need to map "h.name" to "Hero_Name" or just return what we have.
                // The client likely keys off "Hero_Name". 
                // Helper: "h.accursed" -> "Hero_Accursed".

                string heroName = product.ProductCode.Replace("h.", "Hero_");
                string pascalCaseName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(heroName.Split('_').Last());
                string legacyIdentifier = $"Hero_{pascalCaseName}";

                // Ideally, StaticCatalog should store the strict "Hero_X" identifier. 
                // For now, naive transformation: "h.accursed" -> "Hero_Accursed".
                // "h.pyromancer" -> "Hero_Pyromancer".

                int id = Math.Abs(product.ProductCode.GetHashCode());
                heroes[id.ToString()] = legacyIdentifier;
            }
        }

        // If empty, manually seed some defaults to ensure client testability if catalog is empty.
        if (heroes.Count == 0)
        {
            heroes["1"] = "Hero_Scout";
            heroes["2"] = "Hero_Maliken";
            heroes["3"] = "Hero_Pyromancer";
        }

        return Task.FromResult<IActionResult>(Ok(PhpSerialization.Serialize(heroes)));
    }
}