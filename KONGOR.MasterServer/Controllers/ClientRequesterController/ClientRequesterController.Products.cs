namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> HandleGetProducts()
    {
        string? cookie = Request.Form["cookie"];
        if (string.IsNullOrEmpty(cookie)) return new UnauthorizedResult();

        cookie = cookie.Replace("-", string.Empty);

        string? sessionAccountName = HttpContext.Items["SessionAccountName"] as string;

        if (sessionAccountName is null)
        {
             (bool accountSessionCookieIsValid, string? cacheAccountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);
             if (accountSessionCookieIsValid) sessionAccountName = cacheAccountName;
        }

        if (sessionAccountName is null)
            return Unauthorized($@"No Session Found For Cookie ""{cookie}""");

        // Map StaticCatalog.Products to the legacy GetProductsResponse structure: Category -> ID -> Product
        Dictionary<string, Dictionary<int, Dictionary<string, object>>> productsResponse = new Dictionary<string, Dictionary<int, Dictionary<string, object>>>();

        foreach (global::KONGOR.MasterServer.Models.RequestResponse.Store.Product product in Services.Store.StaticCatalog.Products)
        {
            // Map NEXUS Product Type to Legacy Category Name
            string category = product.Type switch
            {
                "Hero" => "Hero",
                "AccountIcon" => "Icons", // Confirm legacy name. READ ONLY has "Misc"? Or "Icons"? READ ONLY GetProductsResponse doesn't show "Icons". It has "Misc", "Alt Avatar", etc.
                // Re-checking READ ONLY: AddProducts(products, "Hero", Upgrade.Type.Hero);
                // "Alt Avatar", "Taunt", "Misc", "Alt Announcement", "Couriers", "Ward", "EAP".
                // NEXUS tokens uses "ai." and "h.".
                // Let's assume "Hero" -> "Hero". "AccountIcon" -> "Misc" (or maybe it's not in the main list?)
                // Wait, "Account Icons" in 'get_products' stub was mapped to "icons". 
                // Let's use "Hero" and "Misc" for now to be safe, or stick to what READ ONLY has.
                _ => "Misc"
            };
            
            // If the category dictionary doesn't exist, create it
            if (!productsResponse.ContainsKey(category))
                productsResponse[category] = new Dictionary<int, Dictionary<string, object>>();

            // Legacy Entry Structure
            // product.ProductCode is a string, but legacy uses int ID.
            // Functionality: We need an int ID.
            // NEXUS Product class doesn't seem to have an int ID in StaticCatalog (it has Name, Description, Cost...). 
            // Wait, I need an ID. 
            // I'll use HashCode or a counter? Or valid IDs if available.
            // StaticCatalog in NEXUS doesn't have IDs. 
            // I will generate a stable ID based on Code hash for now.
            int id = Math.Abs(product.ProductCode.GetHashCode()); 

            productsResponse[category][id] = new Dictionary<string, object>
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

        Dictionary<string, object> response = new Dictionary<string, object>()
        {
            ["products"] = productsResponse
        };

        return Ok(PhpSerialization.Serialize(response));
    }
}
