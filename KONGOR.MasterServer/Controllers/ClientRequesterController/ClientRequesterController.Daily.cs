using Microsoft.EntityFrameworkCore;

namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> HandleGetDailySpecial()
    {
        // 1. Validate Session
        // 1. Validate Session
        string? cookie = Request.Form["cookie"];
        if (string.IsNullOrEmpty(cookie))
            return Unauthorized();

        cookie = cookie.Replace("-", string.Empty);

        string? sessionAccountName = HttpContext.Items["SessionAccountName"] as string;

        if (sessionAccountName is null)
        { 
             // Try validate if not already done (though ClientRequester should have done it)
             (bool accountSessionCookieIsValid, string? cacheAccountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);
             if (accountSessionCookieIsValid) sessionAccountName = cacheAccountName;
        }

        if (sessionAccountName is null) 
            return Unauthorized($@"No Session Found For Cookie ""{cookie}""");

        // 2. Retrieve Account (with User data for ownership check)
        global::MERRICK.DatabaseContext.Entities.Core.Account? account = await MerrickContext.Accounts
            .Include(a => a.User)
            .SingleOrDefaultAsync(a => a.Name == sessionAccountName);

        if (account == null)
            return Unauthorized();

        // 3. Build Response List
        List<Dictionary<string, object>> dailyItems = new();

        foreach (Services.Store.StaticCatalog.DailySpecialDefinition special in Services.Store.StaticCatalog.DailySpecials)
        {
            global::KONGOR.MasterServer.Models.RequestResponse.Store.Product? product = Services.Store.StaticCatalog.Products.FirstOrDefault(p => p.ProductCode == special.ProductCode);
            if (product == null) continue;

            // Calculate discounted prices
            int currentGold = (int)(product.CostGold * (100 - special.DiscountOff) / 100.0);
            int currentSilver = (int)(product.CostSilver * (100 - special.DiscountSilver) / 100.0);

            // Determine if owned
            bool isOwned = account.User.OwnedStoreItems.Contains(product.ProductCode);

            // Handle Alt Avatar hero name extraction (Legacy Logic)
            string heroName = "";
            if (product.Type == "Alt Avatar" && product.ProductCode.Contains("_") && product.ProductCode.Contains("."))
            {
                // format: h.heroname.alt_avatar
                // legacy logic: substring between _ and .
                // But ProductCode in StaticCatalog is like "h.scout". 
                // Let's adapt logic for actual ProductCode format if needed. 
                // For now, leave empty unless Type is Alt Avatar.
            }

            Dictionary<string, object> itemDict = new Dictionary<string, object>
            {
                ["panel_index"] = special.PanelIndex,
                ["product_id"] = product.ProductCode.GetHashCode(), // Stable integer ID
                ["hero_name"] = heroName,
                ["item_name"] = product.ProductCode,
                ["item_cname"] = product.Name,
                ["item_type"] = product.Type, // e.g., "Hero"
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

        // 4. Construct Final Response
        Dictionary<string, object> response = new Dictionary<string, object>
        {
            ["list"] = dailyItems,
            ["0"] = true,
            ["vested_threshold"] = 5
        };

        return Ok(PhpSerialization.Serialize(response));
    }
}
