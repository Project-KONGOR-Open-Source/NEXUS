namespace KONGOR.MasterServer.Services.Store;

public static class StaticCatalog
{
    public static readonly List<Product> Products =
    [
        // Heroes (Sample - would eventually include all)
        new()
        {
            Name = "Accursed",
            Description = "Legion Strength Hero",
            CostGold = 250,
            CostSilver = 0,
            ProductCode = "h.accursed",
            Type = "Hero",
            IconPath = "heroes/accursed/icon.tga"
        },
        new()
        {
            Name = "Scout",
            Description = "Legion Agility Hero",
            CostGold = 250,
            CostSilver = 0,
            ProductCode = "h.scout",
            Type = "Hero",
            IconPath = "heroes/scout/icon.tga"
        },
        new()
        {
            Name = "Pyromancer",
            Description = "Legion Intelligence Hero",
            CostGold = 250,
            CostSilver = 0,
            ProductCode = "h.pyromancer",
            Type = "Hero",
            IconPath = "heroes/pyromancer/icon.tga"
        },

        // Account Icons
        new()
        {
            Name = "Default Icon",
            Description = "Default Account Icon",
            CostGold = 0,
            CostSilver = 0,
            ProductCode = "ai.default",
            Type = "AccountIcon",
            IconPath = "ui/common/account_icons/default.tga"
        },
        new()
        {
            Name = "Kongor Icon",
            Description = "The Mighty Kongor",
            CostGold = 100,
            CostSilver = 500,
            ProductCode = "ai.kongor",
            Type = "AccountIcon",
            IconPath = "ui/common/account_icons/kongor.tga"
        }
    ];

    public static readonly List<DailySpecialDefinition> DailySpecials =
    [
        new()
        {
            PanelIndex = 1,
            ProductCode = "h.scout",
            DiscountOff = 50,
            DiscountSilver = 50,
            TagTitle = "Daily Deal",
            TagDateLimit = "2026-01-01"
        },
        new()
        {
            PanelIndex = 2,
            ProductCode = "h.pyromancer",
            DiscountOff = 25,
            DiscountSilver = 25,
            TagTitle = "Hot Pick",
            TagDateLimit = "2026-01-01"
        },
        new()
        {
            PanelIndex = 3,
            ProductCode = "ai.kongor",
            DiscountOff = 10,
            DiscountSilver = 10,
            TagTitle = "Exclusive",
            TagDateLimit = "2026-01-01"
        }
    ];

    public static Dictionary<string, object> GetCatalogResponse()
    {
        // HoN Legacy Catalog Format Logic
        // This is a simplification. The real format is complex, but the client often accepts a map of products.
        // We return a dictionary where keys are product codes.

        // This structure mimics what the PHP serialization expects for an associative array
        Dictionary<string, object> catalog = new();

        foreach (Product product in Products)
        {
            // Each product is itself an associative array of properties
            catalog[product.ProductCode] = new Dictionary<string, string>
            {
                ["name"] = product.Name,
                ["description"] = product.Description,
                ["icon"] = product.IconPath,
                ["cost_gold"] = product.CostGold.ToString(),
                ["cost_silver"] = product.CostSilver.ToString(),
                ["type"] = product.Type,
                ["can_buy"] = "1"
            };
        }

        return catalog;
    }

    public struct DailySpecialDefinition
    {
        public int PanelIndex { get; set; }
        public string ProductCode { get; set; } // Map to Product Code
        public int DiscountOff { get; set; }
        public int DiscountSilver { get; set; }
        public string TagTitle { get; set; }
        public string TagDateLimit { get; set; }
    }
}