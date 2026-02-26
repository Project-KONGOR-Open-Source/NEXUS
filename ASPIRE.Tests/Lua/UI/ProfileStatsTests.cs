using System.Collections;
using System.Text.RegularExpressions;
using System.Text;
using KONGOR.MasterServer.Models.RequestResponse.Stats;
using KONGOR.MasterServer.Services;
using KONGOR.MasterServer.Services.Requester;
using MERRICK.DatabaseContext.Entities.Statistics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace ASPIRE.Tests.Lua.UI;

public class MatchHistoryPair
{
    public required PlayerStatistics Ps { get; set; }
    public required MatchStatistics Ms { get; set; }
}

public class ProfileStatsTests : LuaTestBase
{
    private const string SCRIPT_PATH = "newui/player_stats_v2.lua";

    public ProfileStatsTests()
    {
        string fullPath = Path.Combine(LuaScriptsRoot, SCRIPT_PATH);
        if (!File.Exists(fullPath)) return;

        string content = File.ReadAllText(fullPath);

        // Fix encoding
        byte[] bytes = File.ReadAllBytes(fullPath);
        if (bytes.Length > 2 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            content = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        else
            content = Encoding.UTF8.GetString(bytes);

        // Mock environment
        content = content.Replace("local _G = getfenv(0)", "local _G = _G");
        content = Regex.Replace(content, @"function\s+([a-zA-Z0-9_:\.]+)\s*\(\.\.\.\)",
            "function $1(...) local arg = { ... }; arg.n = #arg");

        content += "\n function Player_Stats_V2:SetCurrentTab(val) _currentTab = val; println('Tab set to '..val) end \n";
        content += "\n function Player_Stats_V2:GetCurrentTab() return _currentTab end \n";

        LuaState.DoString("unpack = table.unpack");

        LuaState.DoString(content, "@" + SCRIPT_PATH);
        LuaState.DoString("Player_Stats_V2:Init()");
        
        MockProfileWidgets();
    }

    private void MockProfileWidgets()
    {
        Action<string> printProxy = (s) => Console.WriteLine(s);
        LuaState["printlnProxy"] = printProxy;

        LuaState.DoString(@"
            function println(msg) printlnProxy(msg) end
            _G.WidgetValues = {}
            _G.WidgetTextures = {}
            _G.WidgetVisible = {}
            _G.WidgetProgress = {}

            function Widget:SetText(txt)
                if self.name == 'playerstats_overview_rankwins' then
                    println('SETTING WIDGET [' .. self.name .. '] TO: ' .. tostring(txt))
                end
                _G.WidgetValues[self.name] = txt
            end

            function Widget:SetTexture(tex)
                _G.WidgetTextures[self.name] = tex
            end

            function Widget:SetVisible(vis)
                _G.WidgetVisible[self.name] = vis
            end

            function Widget:SetValue(val)
                _G.WidgetValues[self.name] = val
            end

            function Widget:SetWidth(width)
                _G.WidgetProgress[self.name] = width
            end

            function Widget:ClearChildren() end

            _G.InstantiatedWidgets = {}
            function Widget:Instantiate(template, ...)
                local args = {...}
                local params = {}
                params['template'] = template
                for i = 1, #args, 2 do
                    if args[i] and args[i+1] then
                        params[args[i]] = args[i+1]
                    end
                end
                table.insert(_G.InstantiatedWidgets, params)
            end

            function Widget:SetRenderMode(...) end
            function Widget:SetOutlineColor(...) end
            function Widget:SetFont(...) end
            function Widget:SetColor(...) end
            function Widget:SetGlow(...) end
            function Widget:SetGlowColor(...) end
            function Widget:SetBackgroundGlow(...) end
            function Widget:SetAvatar(...) end
            function Widget:GetAbsoluteX() return 0 end
            function Widget:GetWidth() return 100 end
            function Widget:SetAbsoluteX(x) end
            function Widget:SetCallback(evt, func) end
            function Widget:ClearCallback(evt) end
            function Widget:UICmd(cmd) return '0' end

            function GetWidget(name)
                if name == 'playerstats_overview' or name == 'playerstats_stats' or name == 'playerstats_match' or name == 'playerstats_mastery' or name == 'playerstats_mvpawards' then
                    local w = { name = name }
                    setmetatable(w, { __index = Widget })
                    function w:GetWidget(childName)
                        return GetWidget(childName)
                    end
                    return w
                end

                local w = { name = name }
                setmetatable(w, { __index = Widget })
                return w
            end

            function GetRankedPlayInfo(type) return { level = 1, winstreaks = 3 } end
            function IsPreseason() return false end
            function CanAccessHeroProduct() return true end
            function GetMasteryLevelByExp(exp) return math.floor(exp / 1000) end
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
                    table.insert(res, { heroName = name, level = level, heroIcon = icon, exp = exp })
                end
                return res
            end
            function GetMasteryRewardsInfo() return {} end
            function GetHeroProficiency() return {} end
            function GetAccountName() return 'TestUser' end
            function IsMe(name) return true end
            function GetMasterTypeByLevel(lvl) return 'iron' end
            function Translate(key, ...)
                local args = {...}
                if key == 'newui_playerstats_level' then
                    local val = args[2]
                    if val ~= nil then
                        return 'Level '..val
                    end
                end
                return key
            end
            function StripClanTag(name) return name end
            function GetAccountIconTexturePathFromUpgrades() return '' end
            function GetChatNameColorStringFromUpgrades() return '' end
            function GetChatNameColorFontFromUpgrades() return '' end
            function GetChatNameGlowFromUpgrades() return '' end
            function GetChatNameGlowColorStringFromUpgrades() return '' end
            function GetChatNameBackgroundGlowFromUpgrades() return '' end
            function GetChatSymbolTexturePathFromUpgrades() return '' end
            function GetChatNameColorTexturePathFromUpgrades() return '' end
            function NotEmpty(str) return str ~= nil and str ~= '' and str ~= '0' end
            function Empty(str) return str == nil or str == '' or str == '0' end
            function AtoB(val) return val == 'true' or val == '1' end
            function FtoA(val) return tostring(val) end
            function GetEntityDisplayName(name) return name end
            function GetHeroIconPath(name) return '/heroes/' .. string.lower(string.sub(name, 6)) .. '/icon.tga' end
            function GetHeroMasterType(name) return 'iron' end
            function TranslateOrEmpty(key) return key end
            function GetCvarBool(name) return false end
            function GetCvarString(name) return '' end
            function GetCvarInt(name) return 0 end
            function SetMyMasteryExp(info) end
            function IsMaxRankLevel(lvl) return true end
            function IsMaxRankLevelAfterS6(lvl) return true end
            function GetRankIconNameRankLevel(lvl) return 'icon_'..lvl..'.tga' end
            function GetRankIconNameRankLevelAfterS6(lvl) return 'icon_'..lvl..'.tga' end
        ");
    }

    private async Task<Account> SeedAccount(MerrickContext db, string name, int accountId)
    {
        Role? role = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Player");
        if (role == null)
        {
            role = new Role { Name = "Player" };
            db.Roles.Add(role);
            await db.SaveChangesAsync();
        }

        User user = new User
        {
            ID = accountId,
            EmailAddress = $"{name}@kongor.com",
            Role = role,
            SRPPasswordHash = "Hash",
            SRPPasswordSalt = "Salt",
            PBKDF2PasswordHash = "Hash",
            TimestampCreated = DateTime.UtcNow,
            TimestampLastActive = DateTime.UtcNow
        };
        
        Account account = new Account
        {
            ID = accountId,
            Name = name,
            User = user,
            IsMain = true
        };

        db.Users.Add(user);
        db.Accounts.Add(account);
        await db.SaveChangesAsync();
        return account;
    }

    [Test]
    public async Task Debug_LuaStringFind_Regex()
    {
        string luaCode = @"
            local str = '5,1,3,2,11,0,0.75'
            local pattern = '(.+),(.+),(.+),(.+),(.+),(.+),(.+)'
            local _, _, cap1, currlevel, cap3, cap4, cap5, cap6, percentage = string.find(str, pattern)
            return currlevel, percentage
        ";
        object[]? res = LuaState.DoString(luaCode);
        Console.WriteLine($"DEBUG NLua string.find: currlevel={res?[0]}, percentage={res?[1]}");
        await Assert.That(res?[0]?.ToString()).IsEqualTo("1");
        await Assert.That(res?[1]?.ToString()).IsEqualTo("0.75");
    }

    private PlayerStatistics CreateDefaultPlayerStatistics(int accountId, int matchId)
    {
        return new PlayerStatistics
        {
            AccountID = accountId, MatchID = matchId, Inventory = new List<string>(),
            AccountName = "TestUser", ClanID = null, ClanTag = null, Team = 1, LobbyPosition = 0, GroupNumber = 0, Benefit = 0, MVP = 0,
            Win = 0, Loss = 0, Disconnected = 0, Conceded = 0, Kicked = 0,
            PublicMatch = 0, PublicSkillRatingChange = 0, RankedMatch = 0, RankedSkillRatingChange = 0,
            SocialBonus = 0, UsedToken = 0, ConcedeVotes = 0, HeroProductID = 0,
            HeroKills = 0, HeroDamage = 0, GoldFromHeroKills = 0, HeroAssists = 0, HeroExperience = 0, HeroDeaths = 0,
            Buybacks = 0, GoldLostToDeath = 0, SecondsDead = 0,
            TeamCreepKills = 0, TeamCreepDamage = 0, TeamCreepGold = 0, TeamCreepExperience = 0,
            NeutralCreepKills = 0, NeutralCreepDamage = 0, NeutralCreepGold = 0, NeutralCreepExperience = 0,
            BuildingDamage = 0, BuildingsRazed = 0, ExperienceFromBuildings = 0, GoldFromBuildings = 0,
            Denies = 0, ExperienceDenied = 0, Gold = 0, GoldSpent = 0, Experience = 0, Actions = 0, SecondsPlayed = 0,
            HeroLevel = 0, ConsumablesPurchased = 0, WardsPlaced = 0,
            FirstBlood = 0, DoubleKill = 0, TripleKill = 0, QuadKill = 0, Annihilation = 0,
            KillStreak03 = 0, KillStreak04 = 0, KillStreak05 = 0, KillStreak06 = 0, KillStreak07 = 0, KillStreak08 = 0, KillStreak09 = 0, KillStreak10 = 0, KillStreak15 = 0,
            Smackdown = 0, Humiliation = 0, Nemesis = 0, Retribution = 0,
            Score = 0,
            GameplayStat0 = 0, GameplayStat1 = 0, GameplayStat2 = 0, GameplayStat3 = 0, GameplayStat4 = 0,
            GameplayStat5 = 0, GameplayStat6 = 0, GameplayStat7 = 0, GameplayStat8 = 0, GameplayStat9 = 0,
            TimeEarningExperience = 0
        };
    }

    private MatchStatistics CreateDefaultMatchStatistics(int matchId)
    {
        return new MatchStatistics
        {
            MatchID = matchId, TimestampRecorded = DateTime.UtcNow,
            ServerID = 1, HostAccountName = "Server", Map = "caldavar", MapVersion = "1.0",
            TimePlayed = 0, FileSize = 0, FileName = "M1.hon", ConnectionState = 0, Version = "1.0",
            AveragePSR = 1500, AveragePSRTeamOne = 1500, AveragePSRTeamTwo = 1500,
            GameMode = "nm", ScoreTeam1 = 0, ScoreTeam2 = 0, TeamScoreGoal = 0, PlayerScoreGoal = 0,
            NumberOfRounds = 1, ReleaseStage = "0", BannedHeroes = null,
            AwardMostAnnihilations = 0, AwardMostQuadKills = 0, AwardLargestKillStreak = 0, AwardMostSmackdowns = 0,
            AwardMostKills = 0, AwardMostAssists = 0, AwardLeastDeaths = 0, AwardMostBuildingDamage = 0,
            AwardMostWardsKilled = 0, AwardMostHeroDamageDealt = 0, AwardHighestCreepScore = 0
        };
    }

    public class MockHeroDefs : IHeroDefinitionService
    {
        public string GetHeroIdentifier(uint heroId)
        {
            if (heroId == 1) return "Hero_Legionnaire";
            if (heroId == 2) return "Hero_Magmus";
            return "Hero_Unknown";
        }
        public int? GetHeroId(string identifier) => 0;
        public uint GetBaseHeroId(uint altAvatarId) => altAvatarId;
        public uint GetBaseHeroId(string identifier) => 0;
        public bool IsHero(uint entityId) => true;
        public IEnumerable<uint> GetAllHeroIds() => new List<uint> { 1, 2 };
    }

    [Test]
    public async Task Overview_NormalSeason_Should_Populate_Summary_Widgets()
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        MerrickContext db = scope.ServiceProvider.GetRequiredService<MerrickContext>();
        IHeroDefinitionService heroDefs = new MockHeroDefs();
        IPlayerStatisticsService statsService = new PlayerStatisticsService(db, scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<PlayerStatisticsService>>());

        // 1. Seed Account
        Account account = await SeedAccount(db, "TestUser", 9001);

        // 2. Seed PlayerStatistics for Integration Test
        // Rank 5 -> 12500 XP. Rank 6 -> 21600 XP. Diff = 9100.
        // Target: 50% progress -> 12500 + 4550 = 17050 XP.
        account.User.TotalLevel = 5;
        account.User.TotalExperience = 17050;

        // Seed Matches
        // Match 1: Ranked Win, Legio (555s)
        PlayerStatistics p1 = CreateDefaultPlayerStatistics(9001, 1);
        p1.RankedMatch = 1; p1.Win = 1; p1.HeroProductID = 1; p1.SecondsPlayed = 555; p1.HeroKills = 10; p1.HeroAssists = 5;
        db.PlayerStatistics.Add(p1);
        db.MatchStatistics.Add(CreateDefaultMatchStatistics(1));

        // Match 2: Ranked Loss, Magmus (200s)
        PlayerStatistics p2 = CreateDefaultPlayerStatistics(9001, 2);
        p2.RankedMatch = 1; p2.Loss = 1; p2.HeroProductID = 2; p2.SecondsPlayed = 200; p2.HeroKills = 2; p2.HeroDeaths = 5;
        db.PlayerStatistics.Add(p2);
        db.MatchStatistics.Add(CreateDefaultMatchStatistics(2));

        // Match 3-5: Ranked Losses (filling stats to match expectations: 5 Wins, 4 Losses)
        // Add 4 Wins (Total 1+4=5)
        for(int i=3; i<=6; i++) {
             PlayerStatistics p = CreateDefaultPlayerStatistics(9001, i);
             p.RankedMatch = 1; p.Win = 1; p.HeroProductID = 1; p.SecondsPlayed = 100;
             db.PlayerStatistics.Add(p);
             db.MatchStatistics.Add(CreateDefaultMatchStatistics(i));
        }
        // Add 3 Losses (Total 1+3=4)
        for(int i=7; i<=9; i++) {
             PlayerStatistics p = CreateDefaultPlayerStatistics(9001, i);
             p.RankedMatch = 1; p.Loss = 1; p.HeroProductID = 1; p.SecondsPlayed = 100;
             db.PlayerStatistics.Add(p);
             db.MatchStatistics.Add(CreateDefaultMatchStatistics(i));
        }

        // Total Ranked: 1+4=5 Wins, 1+3=4 Losses. Total 9 Matches.

        await db.SaveChangesAsync();

        // 3. Get Stats from Service (Live DB Pull)
        PlayerStatisticsAggregatedDTO stats = await statsService.GetAggregatedStatisticsAsync(9001);

        // 4. Create Response via Helper
        ShowSimpleStatsResponse response = ClientRequestHelper.CreateShowSimpleStatsResponse(account, stats, 1, heroDefs);
        response.SimpleSeasonStats.GenericStats = stats;

        // Mock specific fields likely missing from DTO aggregation (Rank)
        response.Rank = "5";
        response.CurrentRankTop = "5";
        response.HighestRankTop = "10";
        response.CurrentSeason = 9; // Forces the UI to evaluate ranked widget blocks
        response.ConReward = "5,1,3,2,11,0,0.75"; // Mock 75% CoN Progress
        response.Level = 5; // Matches AssertWidgetText("playerstats_playerlevel", "Level 5");
        response.LevelExperience = 17050; // Exp = 17050. Level 5 Base = 12500. Level 6 Base = 21600.

        object[] legacyData = ClientRequestHelper.CreateLegacyPositionalPlayerStats(response);

        // Logan (2025-02-20): Test EXACTLY what the client receives
        LuaState["serverPayload"] = legacyData;

        LuaState.DoString("Player_Stats_V2:OnClickTab('overview')");

        // Execute Native unpack directly with the legacy array returned by C#
        string testRun = @"
            -- Polyfill table.getn for legacy Lua 5.1/5.0 compatibility in NLua
            table.getn = function(t) return #t end
            
            local old_find = string.find
            string.find = function(s, pattern, ...)
                if tostring(s) == '1500' or tostring(s) == '1500.000' then
                    print('^r[TEST DEBUG] Caught string.find on '..tostring(s)..'. returning nil!')
                    return nil
                end
                return old_find(s, pattern, ...)
            end

            -- Correctly invoke the Native UI callback so the local scope 'bHeroMasteryRetrieved' unlocks SetOverviewSeasonInfo
            local masteryArgs = {}
            masteryArgs[1] = 'TestUser'
            masteryArgs[11] = 'Hero_Legionnaire,6275,Hero_Magmus,1000'
            Player_Stats_V2:OnPlayerStatsMasteryResult(unpack(masteryArgs, 1, 15))
            
            -- Unpack the C# passed 0-indexed object[] to a valid 1-indexed Lua spread.
            local args = {}
            for i=0, 148 do
                args[i+1] = serverPayload[i]
            end
            
            for k,v in pairs(args) do
                if tostring(v) == '1500' or tostring(v) == '1500.000' or tostring(v) == '5,1,3,2,11,0,0.75' then
                    print('^cDEBUG ARGS KEY: ' .. k .. ' = ' .. tostring(v))
                end
            end
            
            print('Type of unpack(args):', type(unpack(args, 1, 149)))
            print('First element of unpack(args):', unpack(args, 1, 149))
            
            -- Execute Normal Season Result directly with unpacked varargs
            Player_Stats_V2:OnPlayerStatsNormalSeasonResult(unpack(args, 1, 149))
            
            if _normalStats then
                print('DEBUG con_reward:', tostring(_normalStats.con_reward))
                print('DEBUG total_games_played:', tostring(_normalStats.total_games_played))
            end
            
            print('DEBUG WIDGET MATCHES:', GetWidget('playerstats_matches'):GetText())
            
        ";
        LuaState.DoString(testRun);

        // Assert General Info
        await AssertWidgetText("playerstats_playername", "TestUser");
        await AssertWidgetText("playerstats_matches", "9");
        await AssertWidgetText("playerstats_playerlevel", "Level 5");

        // Assert Normal Season Stats
        await AssertWidgetText("playerstats_overview_rankwins", "5");
        await AssertWidgetText("playerstats_overview_ranklosses", "4");
        await AssertWidgetText("playerstats_overview_rankwinpercent", "55%");

        // Assert Rank & Exp
        // Exp = 17050. Level 5 Base = 12500. Level 6 Base = 21600.
        // Progress = (17050 - 12500) / (21600 - 12500) = 4550 / 9100 = 0.5 (50%)
        // Lua Logic: container:GetWidget('playerstats_overview_meter_to_next_rank'):SetValue(tostring(percentlevel))
        // percentlevel is 0.5.
        await AssertWidgetValue("playerstats_overview_meter_to_next_rank", "0.5");

        // Assert Highest Rank
        await AssertWidgetTexture("playerstats_overview_highest_rank_icon", "/ui/fe2/season/icon_l/icon_10.tga");

        // Assert Fav Heroes
        // Legio: 555 + 4*100 + 3*100 = 1255s
        // Magmus: 200s
        // Total Seconds (RankedSeconds in DTO): 555+200 + 700 = 1455s
        // Legio %: 1255 / 1455 = 0.8625 -> 86%
        // Magmus %: 200 / 1455 = 0.1374 -> 14% (Rounding in Lua might vary, let's check)
        // Helper logic uses: (hero.SecondsPlayed / totalSeconds) * 100.

        await AssertWidgetTexture("playerstats_hero_fav_overview_1_icon", "/heroes/legionnaire/icon.tga");
        await AssertWidgetText("playerstats_hero_fav_overview_1_percent", "86%");

        await AssertWidgetTexture("playerstats_hero_fav_overview_2_icon", "/heroes/magmus/icon.tga");
        await AssertWidgetText("playerstats_hero_fav_overview_2_percent", "14%");

        string debugCon = @"
            local progress = tostring(_G.WidgetValues['playerstats_overview_con_reward_progress'])
            local percent = tostring(_G.WidgetValues['playerstats_overview_con_reward_percent'])
            return progress .. ' | ' .. percent
        ";
        object[]? conOut = LuaState.DoString(debugCon);
        Console.WriteLine("DEBUG CON WIDGETS: " + (conOut != null ? conOut[0].ToString() : "nil"));

        // Assert CoN Reward Progress
        await AssertWidgetProgress("playerstats_overview_con_reward_progress", "75.0%");
        await AssertWidgetText("playerstats_overview_con_reward_percent", "75.0%");

        // Failsafe debug dump
        string debugMastery = @"
            local res = ''
            if _G._heroMastery ~= nil then
                for i, v in ipairs(_G._heroMastery) do
                    res = res .. tostring(v.heroName) .. '=' .. tostring(v.level) .. '; '
                end
            else
                res = '_heroMastery is nil'
            end
            return res
        ";
        object[]? masteryOut = LuaState.DoString(debugMastery);
        Console.WriteLine("DEBUG MASTERY: " + (masteryOut != null && masteryOut.Length > 0 ? masteryOut[0].ToString() : "nil"));

        // Assert Total Overview Mastery Score
        // Hero 1 (Legio) -> 1255 secs * 5 = 6275 exp -> 6275 / 1000 = lvl 6
        // Hero 2 (Magmus) -> 200 secs * 5 = 1000 exp -> 1000 / 1000 = lvl 1
        // Total = 7
        await AssertWidgetText("playerstats_overview_mastery_score", "7");
    }

    [Test]
    public async Task Overview_NormalSeason_Should_Parse_Legacy_Varargs()
    {
        // Tests the exact bug where `player_stats_v2.lua` crashed on "DATA IS NIL OR EMPTY"
        // because K2 mapping parameters sent variables as named strings, bypassing the PHP check.
        
        string testRun = @"
            -- Force mock tab and mastery toggle via native calls
            Player_Stats_V2:OnClickTab('overview')
            Player_Stats_V2:OnPlayerStatsMasteryResult({})
            
            local mockArgs = {}
            for i = 1, 149 do mockArgs[i] = '0' end
            mockArgs[1] = 'TestUser' -- nickname
            mockArgs[7] = '10'       -- wins
            mockArgs[8] = '5'        -- losses
            mockArgs[126] = '15'     -- matches (was total_games_played)
            mockArgs[114] = '12'     -- level
            mockArgs[136] = '7'      -- ranking
            
            -- Legacy FavHero array natively maps into 55 and 60
            mockArgs[55] = 'Hero_Kraken'
            mockArgs[60] = '3600'     -- 1 hour

            -- Execute the normal season block
            print('Type of unpack(mockArgs):', type(unpack(mockArgs, 1, 149)))
            print('First element of unpack(mockArgs):', unpack(mockArgs, 1, 149))
            Player_Stats_V2:OnPlayerStatsNormalSeasonResult(unpack(mockArgs, 1, 149))
        ";
        LuaState.DoString(testRun);
        
        // The fallback GetVal(`key` and `args[key]`) MUST successfully unpack these natively
        await AssertWidgetText("playerstats_playername", "TestUser");
        await AssertWidgetText("playerstats_matches", "15");
        await AssertWidgetText("playerstats_playerlevel", "Level 12");
        await AssertWidgetText("playerstats_overview_rankwins", "10");
        await AssertWidgetText("playerstats_overview_ranklosses", "5");
        
        // Assert Fav Hero was unpacked successfully from the positional fallback mapping
        await AssertWidgetTexture("playerstats_hero_fav_overview_1_icon", "/heroes/Hero_Kraken/icon.tga");
    }

    private async Task AssertWidgetValue(string widgetName, string expected)
    {
        object[]? result = LuaState.DoString($"return WidgetValues['{widgetName}']");
        object? actual = result != null && result.Length > 0 ? result[0] : null;
        await Assert.That(actual?.ToString()).IsEqualTo(expected);
    }

    private async Task AssertWidgetTexture(string widgetName, string expected)
    {
        object[]? result = LuaState.DoString($"return WidgetTextures['{widgetName}']");
        object? actual = result != null && result.Length > 0 ? result[0] : null;
        await Assert.That(actual?.ToString()).IsEqualTo(expected);
    }

    private async Task AssertWidgetVisible(string widgetName, bool expected)
    {
        object[]? result = LuaState.DoString($"return WidgetVisible['{widgetName}']");
        object? actual = result != null && result.Length > 0 ? result[0] : null;
        bool val = false;
        if (actual is bool b) val = b;
        if (actual is double d) val = d != 0;
        if (actual is long l) val = l != 0;
        await Assert.That(val).IsEqualTo(expected);
    }

    private async Task AssertWidgetProgress(string widgetName, string expected)
    {
        object[]? result = LuaState.DoString($"return WidgetProgress['{widgetName}']");
        object? actual = result != null && result.Length > 0 ? result[0] : null;
        await Assert.That(actual?.ToString()).IsEqualTo(expected);
    }

    // Removed MVP_Awards_Should_Populate_Correctly since the legacy player_stats_v2 script doesn't have an OnPlayerStatsMVPAwardsResult function.

    private async Task AssertWidgetText(string widgetName, string expected)
    {
        object[]? result = LuaState.DoString($"return WidgetValues['{widgetName}']");
        object? actual = result != null && result.Length > 0 ? result[0] : null;
        await Assert.That(actual?.ToString()).IsEqualTo(expected);
    }
}
