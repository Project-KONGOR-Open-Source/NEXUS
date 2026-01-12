using HeroesConstants = KONGOR.MasterServer.Constants.Heroes;

namespace KONGOR.MasterServer.Services;

public interface IHeroDefinitionService
{
    string GetHeroIdentifier(uint heroId);
    uint GetBaseHeroId(uint productId);
    uint GetBaseHeroId(string identifier);
    bool IsHero(uint heroId);
}

public class HeroDefinitionService : IHeroDefinitionService
{
    private readonly IHostEnvironment _env; // To find ContentRootPath
    private readonly Dictionary<uint, string> _heroMappings = new();

    private readonly Dictionary<string, uint> _identifierToBaseId = new();
    private readonly ILogger<HeroDefinitionService> _logger;

    public HeroDefinitionService(ILogger<HeroDefinitionService> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
        LoadBaseHeroes(); // Pre-seed base heroes
        LoadDefinitions(); // Load avatars/overrides from JSON
    }

    public uint GetBaseHeroId(string identifier)
    {
        if (_identifierToBaseId.TryGetValue(identifier, out uint baseId))
        {
            return baseId;
        }

        // Return 0 if not found (Legionnaire/Default)
        return 0;
    }

    public uint GetBaseHeroId(uint productId)
    {
        // 1. Get Identifier for the product ID (e.g. 121 -> "Hero_Jereziah")
        string identifier = GetHeroIdentifier(productId);

        // 2. Look up the authoritative Base ID for this Identifier (e.g. "Hero_Jereziah" -> 12)
        if (_identifierToBaseId.TryGetValue(identifier, out uint baseId))
        {
            return baseId;
        }

        // 3. Fallback: If no authoritative Base ID found, return the product ID itself
        // (This handles cases where the product ID IS the base ID, or we just don't know better)
        return productId;
    }

    public string GetHeroIdentifier(uint heroId)
    {
        if (_heroMappings.TryGetValue(heroId, out string? identifier))
        {
            return identifier;
        }

        // Fallback or Log
        // Returning "Hero_Legionnaire" as a safe fallback visually, but logging so we know.
        // Or keep empty/null to let client handle default? 
        // Based on user feedback, "Legionnaire" IS the annoying default, so maybe we want to be explicit if missing.
        _logger.LogWarning("Missing mapping for HeroID: {HeroID}", heroId);

        // Use a safe default that exists to prevent crashes, but maybe "Hero_Legionnaire" is confusing.
        // However, standard behavior for unmapped ID is often Legionnaire (ID 0/default).
        return "Hero_Legionnaire";
    }

    public bool IsHero(uint heroId)
    {
        return _heroMappings.ContainsKey(heroId);
    }

    private void LoadBaseHeroes()
    {
        // Load Base Heroes from KONGOR.MasterServer.Constants.Heroes
        // This is the authoritative source for code-defined hero mappings.
        _logger.LogInformation("Loading Base Heroes from Constants.Heroes...");

        try
        {
            Type heroesType = typeof(HeroesConstants);
            int count = 0;

            foreach (Type nestedType in heroesType.GetNestedTypes())
            {
                FieldInfo? idField = nestedType.GetField("HeroId");
                FieldInfo? identifierField = nestedType.GetField("Identifier");

                if (idField != null && identifierField != null)
                {
                    uint id = (uint) idField.GetValue(null)!;
                    string identifier = (string) identifierField.GetValue(null)!;

                    if (id > 0 && !string.IsNullOrEmpty(identifier))
                    {
                        _heroMappings[id] = identifier;
                        _identifierToBaseId[identifier] = id; // Cache reverse mapping
                        count++;
                    }
                }
            }

            _logger.LogInformation("Loaded {Count} base heroes from Constants (Total Mapped: {Total})", count,
                _heroMappings.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load base heroes from Constants.Heroes");
        }

        // Manual fallbacks for IDs not yet in Constants.Heroes
        // Restored from previous version to fix "Missing mapping" warnings
        Dictionary<uint, string> manualBaseHeroes = new()
        {
            { 0, "Hero_Legionnaire" },
            // Legion
            { 3, "Hero_Vindicator" },
            { 4, "Hero_Scout" },
            { 6, "Hero_Engineer" },
            { 7, "Hero_Rocky" },
            { 8, "Hero_Chipper" },
            { 9, "Hero_Bubbles" },

            { 11, "Hero_Tremble" },
            { 12, "Hero_Jereziah" },

            { 14, "Hero_Yogi" },
            { 16, "Hero_Scar" },
            { 17, "Hero_Javaras" },
            { 18, "Hero_Chronos" },
            { 19, "Hero_Ravenor" },
            { 20, "Hero_Rampage" },
            { 21, "Hero_Chronos" },

            { 24, "Hero_Tundra" },
            { 25, "Hero_Fade" },
            { 26, "Hero_Pyromancer" },
            { 27, "Hero_Voodoo" },
            { 32, "Hero_Scout" },
            { 34, "Hero_Valkyrie" },
            { 36, "Hero_Fairy" },
            { 40, "Hero_Kunas" },
            { 42, "Hero_DwarfMagi" },
            { 44, "Hero_WitchSlayer" }, // "AlphaMale" -> Witch Slayer (per Upgrades.JSON)
            { 10, "Hero_Hammerstorm" },
            { 13, "Hero_Krixi" },
            { 22, "Hero_Hiro" },
            { 96, "Hero_DoctorRepulsor" },
            { 122, "Hero_Pandamonium" },
            { 173, "Hero_Succubis" },
            { 250, "Hero_FlameDragon" },
            // Hellbourne
            { 91, "Hero_Deadwood" },
            { 92, "Hero_Defiler" },
            { 93, "Hero_Devourer" },
            { 94, "Hero_Electrician" },
            { 95, "Hero_Kenisis" }, // "Maestro" -> Kinesis (Folder is /heroes/kenisis/)

            { 103, "Hero_Glacius" },
            { 104, "Hero_Ophelia" },
            { 105, "Hero_Gravekeeper" },
            { 106, "Hero_Hellbringer" },
            { 109, "Hero_Hunter" },
            { 110, "Hero_Hydromancer" },
            { 111, "Hero_WitchSlayer" },
            { 112, "Hero_Ra" },
            { 115, "Hero_Kraken" },
            { 116, "Hero_Cthulhuphant" },
            { 119, "Hero_Vanya" },
            { 120, "Hero_PlagueRider" },

            { 123, "Hero_MasterOfArms" },
            { 125, "Hero_Gladiator" },
            { 126, "Hero_Chronos" },
            // Common IDs
            { 153, "Hero_Accursed" },
            { 155, "Hero_Maliken" },
            { 156, "Hero_Magmar" },
            { 152, "Hero_PuppetMaster" },
            { 162, "Hero_Shaman" },
            { 168, "Hero_Ebulus" },
            { 172, "Hero_Soulstealer" },

            { 178, "Hero_Xalynx" },
            { 185, "Hero_WitchSlayer" },
            // Other
            { 671, "Hero_MonkeyKing" },
            { 227, "Hero_Solstice" },
            { 228, "Hero_EmeraldWarden" },
            { 232, "Hero_Midas" },
            { 236, "Hero_Nomad" },
            { 240, "Hero_Artesia" },
            { 245, "Hero_Silhouette" },
            { 246, "Hero_Gemini" },
            { 249, "Hero_Berzerker" },

            { 62, "Hero_Empath" },
            { 5819, "Hero_Empath" },
            { 121, "Hero_Jereziah" },
            { 205, "Hero_Gauntlet" },
            { 293, "Hero_Calamity" }
        };

        foreach (KeyValuePair<uint, string> kvp in manualBaseHeroes)
        {
            if (!_heroMappings.ContainsKey(kvp.Key))
            {
                _heroMappings[kvp.Key] = kvp.Value;

                // Also attempt to populate Base ID reverse lookup if possible
                // For manual overrides, we assume the Key is the Base ID unless it's a known product ID alias
                // But for safety, we only trust Heroes.cs for authoritative Base IDs.
                // However, we can use these Keys as Base IDs if they are < 255 and not already mapped?
                // Let's rely on Heroes.cs for the "True" Base ID where possible.
            }
        }
    }


    private void LoadDefinitions()
    {
        // Try multiple paths to find Upgrades.JSON
        string[] possiblePaths =
        {
            Path.Combine(_env.ContentRootPath, "Data", "Seed", "Upgrades.JSON"),
            Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "Upgrades.JSON"),
            Path.Combine(Directory.GetCurrentDirectory(), "Data", "Seed", "Upgrades.JSON")
        };

        string? path = possiblePaths.FirstOrDefault(File.Exists);

        if (path == null)
        {
            _logger.LogCritical("Upgrades.JSON NOT FOUND! Checked: {Paths}", string.Join(", ", possiblePaths));
            return;
        }

        _logger.LogInformation("Loading Hero Definitions from: {Path}", path);

        try
        {
            string json = File.ReadAllText(path);
            JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
            List<UpgradeDefinition>? upgrades = JsonSerializer.Deserialize<List<UpgradeDefinition>>(json, options);

            if (upgrades == null)
            {
                _logger.LogError("Failed to deserialize Upgrades.JSON (Result was null).");
                return;
            }

            int count = 0;
            foreach (UpgradeDefinition upgrade in upgrades)
            {
                if (uint.TryParse(upgrade.UpgradeId, out uint id))
                {
                    // Type 1 = Hero (Base)
                    if (upgrade.UpgradeType == 1 && !string.IsNullOrEmpty(upgrade.Code))
                    {
                        // Protect our hardcoded Base Heroes from bad JSON data (e.g. "Wild Soul" vs "Hero_Wildsoul")
                        if (!_heroMappings.ContainsKey(id))
                        {
                            // Fix: Some JSON entries use "Armadon" instead of "Hero_Armadon". Enforce prefix.
                            string code = upgrade.Code;
                            if (!code.StartsWith("Hero_", StringComparison.OrdinalIgnoreCase) &&
                                !code.StartsWith("Avatar_", StringComparison.OrdinalIgnoreCase))
                            {
                                // Only prepend if it really looks like a bare name
                                code = "Hero_" + code;
                            }

                            _heroMappings[id] = code;
                            count++;
                        }
                    }
                    // Type 3 = Avatar (Alt) - We map them to their Code (e.g., "Avatar_Legionnaire_Alt1")
                    // The client usually renders these fine if the Code is valid.
                    else if (upgrade.UpgradeType == 3 && !string.IsNullOrEmpty(upgrade.Code))
                    {
                        // Type 3 might be an Avatar overriding a base ID? Usually typically UpgradeId is unique.
                        // But if we have a hardcoded base hero, we generally trust our list more for the BASE identity.
                        if (!_heroMappings.ContainsKey(id))
                        {
                            _heroMappings[id] = upgrade.Code;
                        }
                    }
                }
            }

            _logger.LogInformation("Loaded {Count} hero mappings from JSON (Total Mapped: {Total})", count,
                _heroMappings.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Upgrades.JSON.");
        }
    }

    // Minimal class for JSON deserialization
    private class UpgradeDefinition
    {
        public string UpgradeId { get; } = string.Empty;
        public int UpgradeType { get; set; }
        public string Code { get; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}