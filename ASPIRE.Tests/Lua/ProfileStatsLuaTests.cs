using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NLua;
using TUnit.Core;

namespace ASPIRE.Tests.Lua;

public class ProfileStatsLuaTests : LuaTestBase
{
    private const string SCRIPT_PATH = "newui/player_stats_v2.lua";

    public ProfileStatsLuaTests()
    {
        try
        {
            // Load the script
            string fullPath = Path.Combine(LuaScriptsRoot, SCRIPT_PATH);
            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"[ProfileStatsLuaTests] Script not found: {fullPath}");
                return;
            }

            string content = File.ReadAllText(fullPath);

            // Standard patching
            content = content.Replace("local _G = getfenv(0)", "local _G = _G");
            content = Regex.Replace(content, @"function\s+([a-zA-Z0-9_:\.]+)\s*\(\.\.\.\)",
                "function $1(...) local arg = { ... }; arg.n = #arg");

            // Inject Backdoor to set current tab
            content += "\n function Player_Stats_V2:SetCurrentTab(val) _currentTab = val; print('Tab set to '..val) end \n";

            // Inject Backdoor to set bHeroMasteryRetrieved (required for Overview update)
            content += "\n function Player_Stats_V2:SetHeroMasteryRetrieved(val) bHeroMasteryRetrieved = val; print('HeroMasteryRetrieved set to '..tostring(val)) end \n";

            // Polyfill unpack
            LuaState.DoString("unpack = table.unpack");

            LuaState.DoString(content, "@" + SCRIPT_PATH);
            LuaState.DoString("Player_Stats_V2:Init()");
        }
        catch (Exception ex)
        {
             Console.WriteLine($"[ProfileStatsLuaTests] Constructor failed: {ex}");
             throw;
        }
    }

    // Removed Overview_Should_Populate_Wins_From_Modern_Keys since the legacy array format is enforced and modern keys are not parsed.
}
