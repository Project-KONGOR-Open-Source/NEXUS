using NLua;

namespace ASPIRE.Tests.Lua;

public abstract class LuaTestBase : IDisposable
{
    protected NLua.Lua LuaState { get; private set; }
    protected string LuaScriptsRoot { get; private set; }

    // Server Integration
    protected WebApplicationFactory<KONGORAssemblyMarker> Factory { get; private set; }
    protected HttpClient HttpClient { get; private set; }

    // Store the last response for verification
    public HttpResponseMessage? LastResponse { get; private set; }
    public string? LastResponseContent { get; private set; }

    public LuaTestBase()
    {
        // Initialize Server
        Factory = KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient = Factory.CreateClient();

        LuaState = new NLua.Lua();
        LuaState.LoadCLRPackage();
        
        // Check Lua version
        try 
        {
             Console.WriteLine($"Lua Version: {LuaState.DoString("return _VERSION")[0]}");
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Failed to get Lua version: {ex.Message}");
        }

        // Define root path for scripts
         string currentDir = AppContext.BaseDirectory;
         // Now that we copy "Data" to output, we can look there directly.
         LuaScriptsRoot = Path.GetFullPath(Path.Combine(currentDir, "Data/ClientLuaFiles/ui/scripts"));
         
         if (!Directory.Exists(LuaScriptsRoot))
         {
             // Fallback to source tree if copy failed or running in different context
             LuaScriptsRoot = Path.GetFullPath(Path.Combine(currentDir, "../../../Data/ClientLuaFiles/ui/scripts"));
         }

         SetupMocks();
    }

    protected virtual void SetupMocks()
    {
        // Mock common K2 global functions
        // We do this BEFORE loading any script that might rely on them
        LuaState.DoString(@"
            _G = _G or {}
            
            -- Polyfill getfenv for Lua 5.2+
            if not getfenv then
                function getfenv(lvl) return _G end
            end
            
            -- 1. Helper Functions
            function GetWidget(name) return _G.MockGetWidget(name) end
            function Translate(key, ...) return _G.MockTranslate(key, ...) end
            function TranslateOrEmpty(key) return _G.MockTranslate(key) end
            function GetCvarBool(name) return _G.MockGetCvarBool(name) end
            function GetCvarString(name) return _G.MockGetCvarString(name) end
            function GetCvarInt(name) return _G.MockGetCvarInt(name) end
            function GetCvarNumber(name) return _G.MockGetCvarNumber(name) end
            function NotEmpty(str) return str ~= nil and str ~= '' end
            function Empty(str) return str == nil or str == '' end
            function AtoB(val) return val == 'true' or val == '1' or val == 1 or val == true end
            function AtoN(val) return tonumber(val) or 0 end
            function StripClanTag(name) return name end
            function println(msg) end
            function printdb(msg) end
            function Echo(msg) end
            function SetSave(name, val, type) end 
            function Set(name, val, type) end
            function Cmd(cmd) end
            function GetScreenWidth() return 1920 end
            function GetScreenHeight() return 1080 end
            function SubmitForm(...) return _G.MockSubmitForm(...) end
            function IsMe(name) return true end
            function RegisterScript(name, version) end
            function RegisterScript2(name, version) end
            function GetDBEntry(name, defaultVal, ...) return defaultVal end
            function GetTime() return 0 end
            function Trigger(name, ...) end
            function GetMaps() return 'map1,map2' end
            function SetCancelCallback(cb) end
            function GetMasterTypeByLevel(level) return 'iron' end

            -- Added Mocks for player_stats_v2.lua
            function GetEntityDisplayName(entityName) return _G.MockGetEntityDisplayName(entityName) end
            function FtoA(val, width, precision, str) return string.format('%.'..precision..'f', val) end
            function GetHeroIconPath(hero) return '/ui/icons/'..hero..'.tga' end
            function GetHeroMasterType(hero) return 'gold' end
            function GetExperienceForLevel(level) return 1000 end
            function GetExperienceForNextLevel(level) return 2000 end
            function GetPercentNextLevel(level) return 50 end

            -- 2. Define Widget Class
            Widget = {}
            Widget.__index = Widget
            
            function Widget:IsValid() return true end
            function Widget:Sleep(time, func) if func then func() end end
            function Widget:SetText(text) _G.MockWidgetSetText(self.name, text) end
            function Widget:SetTexture(texture) _G.MockWidgetSetTexture(self.name, texture) end
            function Widget:SetVisible(visible) _G.MockWidgetSetVisible(self.name, visible) end
            function Widget:SetWidth(width) end
            function Widget:SetHeight(height) end
            function Widget:SetColor(color) end
            function Widget:SetFont(font) end
            function Widget:SetOutlineColor(color) end
            function Widget:SetRenderMode(mode) end
            function Widget:SetGlow(glow) end
            function Widget:SetGlowColor(color) end
            function Widget:SetBackgroundGlow(glow) end
            function Widget:SetAvatar(url) end
            function Widget:GetStringWidth(str) return 100 end
            function Widget:GetHeight() return 100 end
            function Widget:ClearChildren() end
            function Widget:Instantiate(...) end
            function Widget:SetValue(val) end
            function Widget:SetMaxValue(val) end
            function Widget:AddTemplateListItem(...) end
            function Widget:ClearItems() end
            function Widget:SetSelectedItemByValue(...) end
            function Widget:RegisterWatch(...) end
            function Widget:GetAbsoluteX() return 0 end
            function Widget:GetWidth() return 100 end
            function Widget:SetCallback(name, func) _G.MockWidgetSetCallback(self.name, name, func) end
            function Widget:ClearCallback(name) end
            function Widget:SetAbsoluteX(x) end
            function Widget:SetY(y) end
            function Widget:FadeIn(time) end
            function Widget:FadeOut(time) end
            function Widget:BringToFront() end
            function Widget:Hide() end
            function Widget:DoEventN(n) end
            function Widget:SetFocus(focus) end
            function Widget:SetEnabled(enabled) end
            function Widget:GetValue() return '0' end
            function Widget:GetText() return '0' end
            function Widget:UICmd(cmd) return '1' end
            function Widget:GetWidget(name) return _G.MockGetWidget(name) end
            
            -- 3. Define Widget Factory (MockGetWidget)
            function _G.MockGetWidget(name)
                local w = { name = name }
                setmetatable(w, Widget)
                return w
            end

            -- 4. Define Object (Interface)
            object = { name = 'MockInterface' }
            setmetatable(object, Widget) -- Make object inherit from Widget for IsValid etc if needed, or just mock methods
            
            function object:GetName() return 'MockInterface' end
            function object:RegisterWatch(name, func) end
            function object:UICmd(cmd) return '' end
            function object:GetWidget(name) return GetWidget(name) end
            function object:GetHeightFromString(str) return 20 end
            function object:GetMaps() return 'map1,map2' end

            -- 5. Define Other Globals
            Cvar = {}
            function Cvar.CreateCvar(name, type, val) end
            function Cvar.GetCvar(name) return nil end
            function Cvar.SetCvar(name, val) end

            Database = {}
            function Database.New(name) 
                return { 
                    current = {}, 
                    default = {}, 
                    Flush = function() end,
                    SetDBEntry = function() end,
                    ReadDBEntry = function() end
                } 
            end

            HoN_Options = {}
            HoN_Options.gameChatFilters = { activeFilter = 'all', muteAll = 0, chatSounds = true, heroIcons = true, shortFormat = false, nickHighlight = true, walkthroughState = 0 }
            
            UI = {}

            UITrigger = {}
            function UITrigger.CreateTrigger(name) end
            function UITrigger.GetTrigger(name) return nil end
            
            UIManager = {}
            UIManager.GetInterface = function(name) return object end
            UIManager.GetActiveInterface = function() return object end
            
            -- Mock objects
            object = {}
            function object:GetName() return 'MockInterface' end
            function object:RegisterWatch(name, func) end
            function object:UICmd(cmd) return '' end
            function object:GetWidget(name) return _G.MockGetWidget(name) end
            
            UIManager = {}
            UIManager.GetInterface = function(name) return object end
            UIManager.GetActiveInterface = function() return object end
            
            Client = {}
            Client.GetCookie = function() return 'mock_cookie' end
            function Client.AccountLessThan24HoursOld() return false end
            function Client.IsSubAccount() return false end
            
            -- Monkey-patch memoizeObject to avoid recursion in tests
            -- The original implementation calls :IsValid() which might be recursive or problematic in mocks
            function memoizeObject(f)
                local mem = {}
                setmetatable(mem, {__mode = 'kv'})
                return function (x, a1, a2)
                    if (x) then
                        local r = mem[x]
                        -- In tests, we skip the IsValid check or make it safe
                        if r == nil then
                            r = f(x, a1, a2)
                            mem[x] = r
                        end
                        return r
                    end
                end
            end
            
            -- 6. Upgrade Helpers
            function GetAccountIconTexturePathFromUpgrades(selected_upgrades, account_id) return '/mock/icon.tga' end
            function GetChatNameColorStringFromUpgrades(selected_upgrades) return '#ffffff' end
            function GetChatNameColorFontFromUpgrades(selected_upgrades) return 'font' end
            function GetChatNameGlowFromUpgrades(selected_upgrades) return 'glow' end
            function GetChatNameGlowColorStringFromUpgrades(selected_upgrades) return '#000000' end
            function GetChatNameBackgroundGlowFromUpgrades(selected_upgrades) return 'bg_glow' end
            function GetChatSymbolTexturePathFromUpgrades(selected_upgrades) return '/mock/symbol.tga' end
            function GetChatNameColorTexturePathFromUpgrades(selected_upgrades) return '/mock/color.tga' end
            function GetRankIconNameRankLevel(level) return 'rank_icon' end
            function GetRankIconNameRankLevelAfterS6(level) return 'rank_icon_s6' end
            function IsMaxRankLevel(level) return false end
            function IsMaxRankLevelAfterS6(level) return false end

            -- Extensive Logging Helpers (Mocked for isolated tests)
            function SerializeTable(val, name, skipnewlines, depth)
                skipnewlines = skipnewlines or false
                depth = depth or 0
                local tmp = string.rep(' ', depth)
                if name then tmp = tmp .. name .. ' = ' end
                if type(val) == 'table' then
                    tmp = tmp .. '{' .. (not skipnewlines and '\n' or '')
                    for k, v in pairs(val) do
                        tmp =  tmp .. SerializeTable(v, k, skipnewlines, depth + 1) .. ',' .. (not skipnewlines and '\n' or '')
                    end
                    tmp = tmp .. string.rep(' ', depth) .. '}'
                elseif type(val) == 'number' then
                    tmp = tmp .. tostring(val)
                elseif type(val) == 'string' then
                    tmp = tmp .. string.format('%q', val)
                elseif type(val) == 'boolean' then
                    tmp = tmp .. (val and 'true' or 'false')
                else
                    tmp = tmp .. '\'[inserializeable datatype:' .. type(val) .. ']\''
                end
                return tmp
            end

            function SerializeArgs(...)
                local arg = {...}
                local str = ''
                for i, v in ipairs(arg) do
                    str = str .. SerializeTable(v) .. ', '
                end
                return str
            end

            function NotEmpty(var)
                return var ~= nil and var ~= '' and var ~= 0
            end
        ");

        // Register C# callbacks
        LuaState.RegisterFunction("MockWidgetSetText", this, typeof(LuaTestBase).GetMethod(nameof(OnWidgetSetText)));
        LuaState.RegisterFunction("MockWidgetSetTexture", this, typeof(LuaTestBase).GetMethod(nameof(OnWidgetSetTexture)));
        LuaState.RegisterFunction("MockWidgetSetVisible", this, typeof(LuaTestBase).GetMethod(nameof(OnWidgetSetVisible)));
        LuaState.RegisterFunction("MockTranslate", this, typeof(LuaTestBase).GetMethod(nameof(OnTranslate)));
        LuaState.RegisterFunction("MockGetCvarBool", this, typeof(LuaTestBase).GetMethod(nameof(OnGetCvarBool)));
        LuaState.RegisterFunction("MockGetCvarInt", this, typeof(LuaTestBase).GetMethod(nameof(OnGetCvarInt)));
        LuaState.RegisterFunction("MockGetCvarNumber", this, typeof(LuaTestBase).GetMethod(nameof(OnGetCvarNumber)));
        LuaState.RegisterFunction("MockGetCvarString", this, typeof(LuaTestBase).GetMethod(nameof(OnGetCvarString)));
        LuaState.RegisterFunction("MockSubmitForm", this, typeof(LuaTestBase).GetMethod(nameof(OnSubmitForm)));
        LuaState.RegisterFunction("MockWidgetSetCallback", this, typeof(LuaTestBase).GetMethod(nameof(OnWidgetSetCallback)));
        LuaState.RegisterFunction("MockGetEntityDisplayName", this, typeof(LuaTestBase).GetMethod(nameof(OnGetEntityDisplayName)));
    }

    public Dictionary<string, string> WidgetTextValues { get; } = new();
    public Dictionary<string, string> WidgetTextureValues { get; } = new();
    public Dictionary<string, bool> WidgetVisibleValues { get; } = new();
    public Dictionary<string, Dictionary<string, NLua.LuaFunction>> WidgetCallbacks { get; } = new();

    public string OnGetEntityDisplayName(string entityName)
    {
        return entityName;
    }

    public void OnWidgetSetCallback(string widgetName, string callbackName, NLua.LuaFunction func)
    {
        if (!WidgetCallbacks.ContainsKey(widgetName))
        {
            WidgetCallbacks[widgetName] = new Dictionary<string, NLua.LuaFunction>();
        }
        WidgetCallbacks[widgetName][callbackName] = func;
    }

    public void ClickButton(string buttonName)
    {
        if (WidgetCallbacks.TryGetValue(buttonName, out Dictionary<string, LuaFunction>? callbacks))
        {
            // Try 'onclick' first, then 'onbutton' (common in HoN)
            if (callbacks.TryGetValue("onclick", out LuaFunction? onClick))
            {
                Console.WriteLine($"[ClickButton] Clicking {buttonName} (onclick)...");
                onClick.Call();
                return;
            }
            if (callbacks.TryGetValue("onbutton", out LuaFunction? onButton))
            {
                Console.WriteLine($"[ClickButton] Clicking {buttonName} (onbutton)...");
                onButton.Call();
                return;
            }
        }
        Console.WriteLine($"[ClickButton] Warning: No click callback found for '{buttonName}'.");
    }

    public void OnWidgetSetText(string widgetName, object text)
    {
        WidgetTextValues[widgetName] = text?.ToString() ?? "";
    }

    public void OnWidgetSetTexture(string widgetName, object texture)
    {
        WidgetTextureValues[widgetName] = texture?.ToString() ?? "";
    }

    public void OnWidgetSetVisible(string widgetName, object visible)
    {
        if (visible is bool b)
        {
            WidgetVisibleValues[widgetName] = b;
        }
        else if (visible is double d)
        {
            WidgetVisibleValues[widgetName] = d != 0;
        }
        else if (visible is long l)
        {
            WidgetVisibleValues[widgetName] = l != 0;
        }
        else
        {
            // Lua might pass "1" or "true" string
            string s = visible?.ToString() ?? "false";
            WidgetVisibleValues[widgetName] = s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
    }

    public string OnTranslate(string key, params object[] args)
    {
        return $"[{key}]";
    }

    public bool OnGetCvarBool(string name)
    {
        return false;
    }

    public int OnGetCvarInt(string name)
    {
        return 0;
    }

    public double OnGetCvarNumber(string name)
    {
        return 0.0;
    }

    public string OnGetCvarString(string name)
    {
        return "";
    }

    public void OnSubmitForm(params object[] args)
    {
        if (args == null || args.Length == 0)
        {
            Console.WriteLine("SubmitForm called with no arguments.");
            return;
        }

        string formName = args[0]?.ToString() ?? "UnknownForm";
        Dictionary<string, string> formData = new Dictionary<string, string>();

        for (int i = 1; i < args.Length; i += 2)
        {
            if (i + 1 < args.Length)
            {
                string key = args[i]?.ToString() ?? "";
                string val = args[i+1]?.ToString() ?? "";
                formData[key] = val;
            }
        }

        Console.WriteLine($"[SubmitForm] Submitting {formName}...");

        try
        {
            using FormUrlEncodedContent content = new FormUrlEncodedContent(formData);
            Task<HttpResponseMessage> requestTask = HttpClient.PostAsync("/client_requester.php", content);
            LastResponse = requestTask.GetAwaiter().GetResult();
            LastResponseContent = LastResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            Console.WriteLine($"[SubmitForm] Response Status: {LastResponse.StatusCode}");
            Console.WriteLine($"[SubmitForm] Response Content: {LastResponseContent}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SubmitForm] Error sending request: {ex.Message}");
            LastResponse = null;
            LastResponseContent = null;
        }
    }

    protected void LoadScript(string relativePath)
    {
        string fullPath = Path.Combine(LuaScriptsRoot, relativePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Lua script not found: {fullPath}");
        
        byte[] bytes = File.ReadAllBytes(fullPath);
        string content;
        
        if (bytes.Length > 2 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            content = System.Text.Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        }
        else
        {
            content = System.Text.Encoding.UTF8.GetString(bytes);
        }
        
        content = content.Replace("local _G = getfenv(0)", "local _G = _G");
        content = System.Text.RegularExpressions.Regex.Replace(
            content, 
            @"function\s+([a-zA-Z0-9_:\.]+)\s*\(\.\.\.\)", 
            "function $1(...) local arg = { ... }; arg.n = #arg"
        );
        
        // Pre-load patch for global_main.lua
        if (relativePath.EndsWith("global_main.lua", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"[LoadScript] Patching {relativePath} with safe memoizeObject...");
            
            // Inject safe implementation and rename original to avoid syntax error
            string safeMemoize = @"
function memoizeObject (f)
    local mem = {}
    setmetatable(mem, {__mode = 'kv'})
    return function (x, a1, a2)
        if (x) then
            local r = mem[x]
            if r == nil then
                -- print('memoizeObject computing: ' .. tostring(x))
                r = f(x, a1, a2)
                mem[x] = r
            end
            return r
        end
    end
end
function memoizeObject_UNUSED (f)";
            
            string newContent = content.Replace("function memoizeObject (f)", safeMemoize);
            
            if (newContent == content)
            {
                 Console.WriteLine("[LoadScript] WARNING: String replace did NOT change content. Pattern 'function memoizeObject (f)' not found?");
            }
            else
            {
                 Console.WriteLine("[LoadScript] Patch applied successfully.");
                 content = newContent;
            }
        }
        
        LuaState.DoString(content, "@" + relativePath);
    }

    protected void LoadAllScripts()
    {
        if (!Directory.Exists(LuaScriptsRoot))
        {
            Console.WriteLine($"[LoadAllScripts] Warning: Script root '{LuaScriptsRoot}' does not exist.");
            return;
        }

        string[] allFiles = Directory.GetFiles(LuaScriptsRoot, "*.lua", SearchOption.AllDirectories);
        List<string> relativePaths = new List<string>();

        foreach (string file in allFiles)
        {
            // Calculate relative path
            string relativePath = Path.GetRelativePath(LuaScriptsRoot, file).Replace("\\", "/");
            relativePaths.Add(relativePath);
        }

        // Sort files to load libraries first
        relativePaths.Sort((a, b) =>
        {
            string nameA = Path.GetFileName(a);
            string nameB = Path.GetFileName(b);

            bool isLibA = nameA.StartsWith("lib_", StringComparison.OrdinalIgnoreCase);
            bool isLibB = nameB.StartsWith("lib_", StringComparison.OrdinalIgnoreCase);

            if (isLibA && !isLibB) return -1;
            if (!isLibA && isLibB) return 1;

            // Load global_main.lua early as it defines core functions like memoizeObject
            if (nameA.Equals("global_main.lua", StringComparison.OrdinalIgnoreCase)) return -1;
            if (nameB.Equals("global_main.lua", StringComparison.OrdinalIgnoreCase)) return 1;

            return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
        });

        foreach (string script in relativePaths)
        {
            try
            {
                Console.WriteLine($"[LoadAllScripts] Loading {script}...");
                LoadScript(script);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadAllScripts] Error loading {script}: {ex.Message}");
                // We continue loading other scripts even if one fails, though likely things will break
            }
        }
    }

    public void Dispose()
    {
        LuaState?.Dispose();
        HttpClient?.Dispose();
        Factory?.Dispose();
    }
}
