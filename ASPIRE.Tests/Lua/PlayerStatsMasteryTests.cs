using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NLua;
using TUnit.Core;
using System.Reflection;

namespace ASPIRE.Tests.Lua;

public class PlayerStatsMasteryTests : LuaTestBase
{
    private const string SCRIPT_PATH = "newui/player_stats_v2.lua";

    public PlayerStatsMasteryTests()
    {
        // Load the script manually to ensure we can patch it if needed
        string fullPath = Path.Combine(LuaScriptsRoot, SCRIPT_PATH);
        
        // Simple read
        string content = File.ReadAllText(fullPath);
        
         // Quick BOM strip
        byte[] bytes = File.ReadAllBytes(fullPath);
        if (bytes.Length > 2 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            content = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        else
            content = Encoding.UTF8.GetString(bytes);

        // Patch global environment access
        content = content.Replace("local _G = getfenv(0)", "local _G = _G");
        
        // Patch variadic functions to support NLua 
        content = Regex.Replace(content, @"function\s+([a-zA-Z0-9_:\.]+)\s*\(\.\.\.\)",
            "function $1(...) local arg = { ... }; arg.n = #arg");

        // Load the script
        Console.WriteLine($"[Test] Loading script from: {fullPath}");
        if (!File.Exists(fullPath))
        {
            Console.WriteLine($"[Test] ERROR: Script not found at {fullPath}");
            Console.WriteLine($"[Test] LuaScriptsRoot: {LuaScriptsRoot}");
            Console.WriteLine($"[Test] Current Directory: {Directory.GetCurrentDirectory()}");
        }

        LuaState.DoString("unpack = table.unpack"); // Polyfill unpack
        LuaState.DoString(content, "@" + SCRIPT_PATH);
        LuaState.DoString("Player_Stats_V2:Init()");
        SetupWidgetMocks();
    }



    private void SetupWidgetMocks()
    {
        // Monkey-patch Widget:SetTexture        // Mock Global Functions
        LuaState.DoString(@"
            function GetWidget(name) 
                return _G.MockGetWidget(name) 
            end
            
            function Translate(key, ...) 
                return key 
            end

            -- Mock GetHeroMasteryUpgradeInfo (Called by OnPlayerStatsMasteryResult)
            function GetHeroMasteryUpgradeInfo(info)
                local res = {}
                if type(info) ~= 'string' then return res end
                local parts = {}
                for s in string.gmatch(info, '([^,]+)') do
                    table.insert(parts, s)
                end
                for i=1, #parts, 2 do
                    local name = parts[i]
                    local exp = tonumber(parts[i+1]) or 0
                    local level = math.floor(exp / 1000)
                    local icon = '/heroes/' .. string.lower(string.sub(name, 6)) .. '/icon.tga'
                    table.insert(res, { heroName = name, level = tostring(level), heroIcon = icon, exp = tostring(exp) })
                end
                return res
            end

            -- Mock GetMasteryRewardsInfo (Called by OnPlayerStatsMasteryResult)
            function GetMasteryRewardsInfo(info)
                return {} -- Ignored for now
            end

            -- Mock GetHeroProficiency
            function GetHeroProficiency(info)
                return {}
            end

            -- Mock GetMasterTypeByLevel
            function GetMasterTypeByLevel(level)
                return 'gold'
            end

            function Widget:SetTexture(texture) 
                _G.MockWidgetSetTexture(self.name, texture) 
            end
            
            -- Mock dependent methods on Player_Stats_V2 that might be missing or local
            Player_Stats_V2.SetMasteryRewardsInfo = function(self, ...) end
            -- Mock dependent methods on Player_Stats_V2 that might be missing or local
            Player_Stats_V2.SetMasteryRewardsInfo = function(self, ...) end
            -- Player_Stats_V2.SetMasteryAllHeroes = function(self, ...) end -- REMOVED to allow real logic testing
            Player_Stats_V2.ShowWaitingMask = function(self, ...) end
            Player_Stats_V2.HideWaitingMask = function(self, ...) end
            
            -- Mock global or local functions called
            function SetOverviewSeasonInfo(...) end
            function SetMyMasteryExp(...) end
        ");

        // Register the callback
        LuaState.RegisterFunction("MockWidgetSetTexture", this,
            typeof(PlayerStatsMasteryTests).GetMethod(nameof(OnTestWidgetSetTexture),  BindingFlags.Instance | BindingFlags.NonPublic));
    }

    private void OnTestWidgetSetTexture(string widgetName, object? texture)
    {
        if (texture == null)
        {
             // Fail the test explicitly to mimic K2 engine behavior (strict string check)
             // This ensures we catch data flow issues where nil is passed to UI
             throw new ArgumentNullException(nameof(texture), $"[Client Simulation] SetTexture called on '{widgetName}' with nil value. The K2 engine requires a string path.");
        }
        WidgetTextureValues[widgetName] = texture.ToString() ?? "";
    }

    [Test]
    public async Task OnPlayerStatsMasteryResult_WithCSVString_ParsesCorrectly()
    {
        // 1. Arrange: Setup Mocks (already done in Base, but we need specific hooks if any)
        
        // 2. Act: Pass a CSV string which mimics the C# fix and native K2 engine engine format
        // The format is: "HeroName,Experience,HeroName,Experience"
        // 15000 / 1000 = lvl 15
        // 5000 / 1000 = lvl 5
        string masteryCsv = "Hero_PuppetMaster,15000,Hero_Devourer,5000";
        
        LuaState["netMasteryCsv"] = masteryCsv;

        const string script = @"
            local args = {}
            args[1] = 'Nickname'
            args[11] = netMasteryCsv
            args[12] = '' 
            
            -- Call the function
            Player_Stats_V2:OnPlayerStatsMasteryResult(unpack(args, 1, 12)) 
        ";
        
        LuaState.DoString(script);

        // 3. Assert
        // Sum of levels: 15 + 5 = 20
        await Assert.That(WidgetTextValues).ContainsKey("playerstats_overview_mastery_score");
        await Assert.That(WidgetTextValues["playerstats_overview_mastery_score"]).IsEqualTo("20");
        
        // Sorted by level desc: PuppetMaster (15) should be first (Row 0, Col 0)
        await Assert.That(WidgetTextValues["playerstats_mastery_all_item_0_0_name"]).IsEqualTo("Hero_PuppetMaster");
        // Devourer (5) should be second (Row 0, Col 1)
        await Assert.That(WidgetTextValues["playerstats_mastery_all_item_0_1_name"]).IsEqualTo("Hero_Devourer");
    }

    [Test]
    public async Task OnPlayerStatsMasteryResult_WithNilIcon_ThrowsException()
    {
         // This test verifies that we catch missing data (nil texture)
         // which corresponds to the 'bad argument #1 to SetTexture' error in client.

        string masteryCsv = "Hero_Broken,1000";

        LuaState["netMasteryCsv"] = masteryCsv;

        const string script = @"
            local args = {}
            args[1] = 'Nickname'
            args[11] = netMasteryCsv
            args[12] = ''

            -- Quick inline mock override for Hero_Broken
            local oldGetHero = GetHeroMasteryUpgradeInfo
            function GetHeroMasteryUpgradeInfo(info)
                local res = oldGetHero(info)
                if res and res[1] and res[1].heroName == 'Hero_Broken' then
                    res[1].heroIcon = nil -- Deliberately break the icon to test crash safety
                end
                return res
            end

            Player_Stats_V2:OnPlayerStatsMasteryResult(unpack(args, 1, 12))
        ";

        try
        {
            LuaState.DoString(script);
            Assert.Fail("Test should have thrown exception due to nil SetTexture argument");
        }
        catch (NLua.Exceptions.LuaScriptException ex)
        {
            // NLua wraps .NET exceptions. Check InnerException or Message.
            // The message might be "A .NET exception occurred in user-code"
            if (ex.InnerException != null)
            {
                await Assert.That(ex.InnerException.Message).Contains("SetTexture called on");
            }
            else
            {
                // Fallback check if message is propagated
                await Assert.That(ex.Message).Contains("SetTexture called on");
            }
        }
    }
}
