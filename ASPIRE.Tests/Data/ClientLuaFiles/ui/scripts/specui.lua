-------------------------------------------------------------------------------
-- Spectator Script
-- Copyright 2015 Frostburn Studios
-------------------------------------------------------------------------------

-- Graphs need the totals for a stat tracked, if you are trying to use a new graph and it's throwing errors about the total, make
-- sure it's not commented out (as the only ones not commented out are the ones that are used), if you need a total, uncomment the code for it.

local UPDATE_INTERVAL = 1500			-- How long, in ms, to wait between each update of the spectator interface (push bars, graphs, etc.)

local PUSH_EXAGGERATION = 2 			-- Exaggerates the movements of the push bar, higher number means more exaggeration
local PUSH_DURATION = 1000 				-- how many ms does the changes in the push bars take to animate

-------------------------------------------------------------------------------
local _G = getfenv(0)
HoN_SpecUI = _G[HoN_SpecUI] or {}
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()
RegisterScript2('SpecUI', '35')

HoN_SpecUI.KROSMODE_BASE_HEALTH	= 5000

HoN_SpecUI.uiActive = false

HoN_SpecUI.teamGold = {[0] = 0, [1] = 0}
HoN_SpecUI.teamExperience = {[0] = 0, [1] = 0}
HoN_SpecUI.currentPushGold = 50
HoN_SpecUI.currentPushExp = 50

HoN_SpecUI.playerInfo = {}
HoN_SpecUI.playerInfoTotals = {}
HoN_SpecUI.playerRespawnInfo = {}

HoN_SpecUI.activePlayerGraphs = {}
HoN_SpecUI.ActivePlayerGraph = nil

HoN_SpecUI.registeredTabs = {}

HoN_SpecUI.teamSizes = {[0] = 0, [1] = 0}
HoN_SpecUI.teamMax = 0

HoN_SpecUI.combatNotifications = {}
HoN_SpecUI.itemPops = {}
HoN_SpecUI.lockedTime = 0

-- saved is stored into a table and retrieved
HoN_SpecUI.save = GetDBEntry('HoNSpecUI_Settings4') or
	{
		['teamInfoDisplay'] = 0,
		['activeTab'] = 1,
		['buildingInfoVisible'] = true,
		['dataInfoDisplayVisible'] = true,
		['pushBarVisible'] = true,
		['tabsVisible'] = true,
		['unitFrameMode'] = 2,
		['healthLabelsVisible'] = true,
		['manaLabelsVisible'] = true,
		['abilityCooldownsVisible'] = true,
	}
	-- above are defaults

local function GetWidget(widget, fromInterface, hideErrors)
	--println('GetWidget ' .. tostring(widget) .. ' in interface ' .. tostring(fromInterface))
	if (widget) then
		local returnWidget
		if (fromInterface) then
			returnWidget = UIManager.GetInterface(fromInterface):GetWidget(widget)
		else
			returnWidget = interface:GetWidget(widget)
		end
		if (returnWidget) then
			return returnWidget
		else
			if (not hideErrors) then println('GetWidget failed to find ' .. tostring(widget) .. ' in interface ' .. tostring(fromInterface)) end
			return nil
		end
	else
		println('GetWidget called without a target')
		return nil
	end
end
GetWidget = memoize(GetWidget)

local function ShowWidget(widget, fromInterface)
	if (fromInterface) then
		UIManager.GetInterface(fromInterface):GetWidget(widget):SetVisible(true)
	else
		GetWidget(widget):SetVisible(true)
	end
end

local function HideWidget(widget, fromInterface)
	if (fromInterface) then
		UIManager.GetInterface(fromInterface):GetWidget(widget):SetVisible(false)
	else
		GetWidget(widget):SetVisible(false)
	end
end

HoN_SpecUI.arcadeSetTable = {
	['arcade_text'] = {1800, 1},
	['unicorn'] = {3200, 1},
	['balls'] = {3200, 2},
	['english'] = {3200, 2},
	['pimp'] = {3200, 2},
	['meme'] = {3200, 2},
	['seduction'] = {3200, 2},
	['seductive'] = {3200, 2},
	['thai'] = {3200, 2},
	['thaienglish'] = {3200, 2},
	['pirate'] = {3200, 2},
	['bamf'] = {3200, 3},
	['na_khom'] = {3200, 2},
	['ninja'] = {3200, 2},
	['ursa'] = {3200, 2},
	['merrick'] = {3200, 2},
	['8-bit'] = {3200, 2},
	['sea'] = {3200, 2},
	['surfer'] = {3200, 2},
	['paragon'] = {3200, 2},
	['dark_master'] = {3200, 2},
	['siam'] = {3200, 2},
	['ascension'] = {3200, 2},
	['soccer'] = {3200, 2},
	['soccer_mode'] = {3200, 4},
	['haunted_house'] = {3200, 2},
	['miku'] = {3200, 2},
	['mspudding'] = {3200, 2},
	['graffiti'] = {3200, 2},
	['esan'] = {3200, 2},
	['2018worldcup'] = {3200, 2},
}

local function ArcadeMessage(message, condition, self, value, set)
	if (condition == true) or (condition == tonumber(value)) then
		local modelPanel = GetWidget('spec_arcade_model_'..HoN_SpecUI.arcadeSetTable[set][2], 'game_spectator')
		if (not modelPanel) then
			modelPanel = GetWidget('spec_arcade_model_2', 'game_spectator')
		end

		modelPanel:SetVisible(true)
		modelPanel:UICmd("SetAnim('idle')")
		modelPanel:UICmd("SetModel('" .. '/ui/common/models/'.. set .. '/' .. message .. '.mdf' .. "')")
		modelPanel:UICmd("SetEffect('" .. '/ui/common/models/'.. set .. '/' .. 'bloodlust.effect' ..  "')")
		modelPanel:Sleep(HoN_SpecUI.arcadeSetTable[set][1], function() modelPanel:SetVisible(false) end)
	end
end
object:RegisterWatch('EventTowerDeny', 		function(...) ArcadeMessage('denied', 		true, ...) end)
object:RegisterWatch('EventFirstKill', 		function(...) ArcadeMessage('bloodlust', 	true, ...) end)
object:RegisterWatch('EventMultiKill', 		function(...) ArcadeMessage('hattrick', 	1, ...) 	ArcadeMessage('quadkill', 	2, ...) 	ArcadeMessage('annihilation', 3, ...) end)
object:RegisterWatch('EventKillStreak', 	function(...) ArcadeMessage('bloodbath', 	10, ...) 	ArcadeMessage('immortal', 	15, ...) end)
object:RegisterWatch('EventTeamWipe', 		function(...) ArcadeMessage('genocide', 	true, ...) end)
object:RegisterWatch('EventSmackdown', 		function(...) ArcadeMessage('smackdown', 	true, ...) end)
object:RegisterWatch('EventHumiliation', 	function(...) ArcadeMessage('humiliation', 	true, ...) end)
object:RegisterWatch('EventRival', 			function(...) ArcadeMessage('nemesis', 		true, ...) end)
object:RegisterWatch('EventPayback', 		function(...) ArcadeMessage('payback', 		true, ...) end)
object:RegisterWatch('EventRageQuit', 		function(...) ArcadeMessage('ragequit', 	true, ...) end)
object:RegisterWatch('EventVictory', 		function(...) ArcadeMessage('victory', 		true, ...) end)
object:RegisterWatch('EventDefeat', 		function(...) ArcadeMessage('defeat', 		true, ...) end)

local function SaveSpecUISettings()
	GetDBEntry('HoNSpecUI_Settings4', HoN_SpecUI.save, true)
end

--- Spectator UI functionality
local function UpdateSideDataTexts()
	local legionGoldStr, legionExpStr = "", ""
	local hellbourneGoldStr, hellbourneExpStr = "", ""

	if (HoN_SpecUI.save.teamInfoDisplay == 0) then -- team gold/exp
		legionGoldStr = FtoA(HoN_SpecUI.teamGold[0], 0, 0, ',')
		hellbourneGoldStr = FtoA(HoN_SpecUI.teamGold[1], 0, 0, ',')

		legionExpStr = FtoA(HoN_SpecUI.teamExperience[0], 0, 0, ',')
		hellbourneExpStr = FtoA(HoN_SpecUI.teamExperience[1], 0, 0, ',')
	elseif (HoN_SpecUI.save.teamInfoDisplay == 1) then -- +/- gold/exp in comparison
		local valGold = HoN_SpecUI.teamGold[0] - HoN_SpecUI.teamGold[1]
		local valExp = HoN_SpecUI.teamExperience[0] - HoN_SpecUI.teamExperience[1]

		if (valGold >= 1) then
			legionGoldStr = "+"
			hellbourneGoldStr = "-"
		elseif (valGold <= -1) then
			legionGoldStr = "-"
			hellbourneGoldStr = "+"
		end

		if (valExp >= 1) then
			legionExpStr = "+"
			hellbourneExpStr = "-"
		elseif (valExp <= -1) then
			legionExpStr = "-"
			hellbourneExpStr = "+"
		end

		legionGoldStr = legionGoldStr..FtoA(math.abs(valGold), 0, 0, ',')
		hellbourneGoldStr = hellbourneGoldStr..FtoA(math.abs(valGold), 0, 0, ',')

		legionExpStr = legionExpStr..FtoA(math.abs(valExp), 0, 0, ',')
		hellbourneExpStr = hellbourneExpStr..FtoA(math.abs(valExp), 0, 0, ',')
	elseif (HoN_SpecUI.save.teamInfoDisplay == 2) then -- percent of total gold/exp
		local totalGold = HoN_SpecUI.teamGold[0] + HoN_SpecUI.teamGold[1]
		local totalExp = HoN_SpecUI.teamExperience[0] + HoN_SpecUI.teamExperience[1]

		if (totalGold > 0) then
			legionGoldStr = round((HoN_SpecUI.teamGold[0] / totalGold) * 100.0).."%"
			hellbourneGoldStr = round((HoN_SpecUI.teamGold[1] / totalGold) * 100.0).."%"
		else
			legionGoldStr = "50%"
			hellbourneGoldStr = "50%"
		end

		if (totalExp > 0) then
			legionExpStr = round((HoN_SpecUI.teamExperience[0] / totalExp) * 100.0).."%"
			hellbourneExpStr = round((HoN_SpecUI.teamExperience[1] / totalExp) * 100.0).."%"
		else
			legionExpStr = "50%"
			hellbourneExpStr = "50%"
		end
	else 										  -- percent advantage of gold/exp
		local totalGold = HoN_SpecUI.teamGold[0] + HoN_SpecUI.teamGold[1]
		local totalExp = HoN_SpecUI.teamExperience[0] + HoN_SpecUI.teamExperience[1]

		if (totalGold > 0) then
			local valGold = round(((HoN_SpecUI.teamGold[0] / totalGold) - (HoN_SpecUI.teamGold[1] / totalGold)) * 100.0)

			if (valGold >= 1) then
				legionGoldStr = "+"
				hellbourneGoldStr = "-"
			elseif (valGold <= -1) then
				legionGoldStr = "-"
				hellbourneGoldStr = "+"
			end

			legionGoldStr = legionGoldStr..math.abs(valGold).."%"
			hellbourneGoldStr = hellbourneGoldStr..math.abs(valGold).."%"
		else
			legionGoldStr = "0%"
			hellbourneGoldStr = "0%"
		end

		if (totalExp > 0) then
			local valExp = round(((HoN_SpecUI.teamExperience[0] / totalExp) - (HoN_SpecUI.teamExperience[1] / totalExp)) * 100.0)

			if (valExp >= 1) then
				legionExpStr = "+"
				hellbourneExpStr = "-"
			elseif (valExp <= -1) then
				legionExpStr = "-"
				hellbourneExpStr = "+"
			end

			legionExpStr = legionExpStr..math.abs(valExp).."%"
			hellbourneExpStr = hellbourneExpStr..math.abs(valExp).."%"
		else
			legionExpStr = "0%"
			hellbourneExpStr = "0%"
		end
	end

	-- gold
	GetWidget("legion_data_gold"):SetText(legionGoldStr)
	GetWidget("hellbourne_data_gold"):SetText(hellbourneGoldStr)

	-- exp
	GetWidget("legion_data_exp"):SetText(legionExpStr)
	GetWidget("hellbourne_data_exp"):SetText(hellbourneExpStr)
end

local function PlayerStatToPercent(statVal, statName)
	if (HoN_SpecUI.playerInfoTotals[statName] and HoN_SpecUI.playerInfoTotals[statName] ~= 0) then
		return round((statVal / HoN_SpecUI.playerInfoTotals[statName]) * 100.0).."%"
	else
		return "0%"
	end
end

local function TeamInfo(teamIndex, kills, deaths, expEarned, goldEarned, creepKills, denies, towerDenies, towerCount, rangedCount, meleeCount)
	-- kills, deaths, expEarned, goldEarned = tonumber(kills), tonumber(deaths), tonumber(expEarned), tonumber(goldEarned)
	-- creepKills, denies, towerDenies = tonumber(creepKills), tonumber(denies), tonumber(towerDenies)
	-- towerCount, rangedCount, meleeCount = tonumber(towerCount), tonumber(rangedCount), tonumber(meleeCount)
	expEarned, goldEarned = tonumber(expEarned), tonumber(goldEarned)

	HoN_SpecUI.teamGold[teamIndex] = goldEarned
	HoN_SpecUI.teamExperience[teamIndex] = expEarned
end
interface:RegisterWatch("SpectatorTeamInfo0", function(_, ...) TeamInfo(0, ...) end)
interface:RegisterWatch("SpectatorTeamInfo1", function(_, ...) TeamInfo(1, ...) end)

local function PopulatePlayerGraphBar(graphName, index)
	local graphTable = HoN_SpecUI.activePlayerGraphs[graphName]
	local playerInfo = HoN_SpecUI.playerInfo[index]

	if (graphTable and playerInfo) then
		if (playerInfo.exists) then
			if (graphTable.percent) then
				GetWidget("specui_"..graphName.."_bar"..index.."_label"):SetText(PlayerStatToPercent((graphTable.stats[index] or 0), graphTable.statName))
			else
				GetWidget("specui_"..graphName.."_bar"..index.."_label"):SetText(round(graphTable.stats[index] or 0))
			end

			GetWidget("specui_"..graphName.."_bar"..index.."_heroFrame"):SetColor(playerInfo.color)
			GetWidget("specui_"..graphName.."_bar"..index.."_heroIcon"):SetTexture(playerInfo.heroIcon)
			if (playerInfo.alive) then
				GetWidget("specui_"..graphName.."_bar"..index.."_heroIcon"):SetRenderMode('normal')
			else
				GetWidget("specui_"..graphName.."_bar"..index.."_heroIcon"):SetRenderMode('grayscale')
			end

			GetWidget("specui_"..graphName.."_bar"..index):SetVisible(1)
		else
			GetWidget("specui_"..graphName.."_bar"..index):SetVisible(0)
		end
	else
		GetWidget("specui_"..graphName.."_bar"..index):SetVisible(0)
	end
end

local function PopulatePlayerGraphBars(graphName)
	for i=0,9 do
		PopulatePlayerGraphBar(graphName, i)
	end
end

local function UpdatePlayerGraphBar(index, table)
	for k,v in pairs(HoN_SpecUI.activePlayerGraphs) do
		if (table[v.statName]) then
			-- set the value
			v.stats[index] = table[v.statName]

			if (k == HoN_SpecUI.ActivePlayerGraph) then
				PopulatePlayerGraphBar(k, index)
			end
		end
	end
end

local function UpdatePlayersTab()
	for i=0,9 do
		UpdatePlayersTabSlot(i)
	end
end

local function UpdatePlayersTabSlot(playerIndex)
	local playerInfo = HoN_SpecUI.playerInfo[playerIndex]

	if (playerInfo and playerInfo.exists) then
		if (playerInfo.kills) then-- if we have kills, we have all the other things
			GetWidget("overview_slot_"..playerIndex.."_level"):SetText(playerInfo.level)
			GetWidget("overview_slot_"..playerIndex.."_icon"):SetTexture(playerInfo.heroIcon)
			GetWidget("overview_slot_"..playerIndex.."_name"):SetText(playerInfo.playerName)
			GetWidget("overview_slot_"..playerIndex.."_name"):SetColor(playerInfo.color)
			GetWidget("overview_slot_"..playerIndex.."_mdks"):SetText('^g' .. playerInfo.kills .. '^777/^r' .. playerInfo.deaths .. '^777/^y' .. playerInfo.assists)
			GetWidget("overview_slot_"..playerIndex.."_xpm"):SetText(round(playerInfo.XPpM))
			GetWidget("overview_slot_"..playerIndex.."_gpm"):SetText(round(playerInfo.GpM))
		end

		if (playerInfo.numBuybacks) then -- buybacks run through a seperate trigger, check it seperately
			GetWidget("overview_slot_"..playerIndex.."_buyback1"):SetVisible(playerInfo.numBuybacks >= 1)
			GetWidget("overview_slot_"..playerIndex.."_buyback2"):SetVisible(playerInfo.numBuybacks >= 2)

			if (playerInfo.buybackCost and playerInfo.gold and (playerInfo.gold > playerInfo.buybackCost)) then
				GetWidget("overview_slot_"..playerIndex.."_buyback1"):SetRenderMode("normal")
				GetWidget("overview_slot_"..playerIndex.."_buyback2"):SetRenderMode("normal")
			else
				GetWidget("overview_slot_"..playerIndex.."_buyback1"):SetRenderMode("grayscale")
				GetWidget("overview_slot_"..playerIndex.."_buyback2"):SetRenderMode("grayscale")
			end
		end
	end
end

local function UpdateStatsTab()
	for i=0,9 do
		UpdateStatsTabSlot(i)
	end
end

local function UpdateStatsTabSlot(playerIndex)
	local playerInfo = HoN_SpecUI.playerInfo[playerIndex]

	if (playerInfo and playerInfo.exists and playerInfo.kills) then
		GetWidget("stats_slot_"..playerIndex.."_level"):SetText(playerInfo.level)
		GetWidget("stats_slot_"..playerIndex.."_icon"):SetTexture(playerInfo.heroIcon)
		GetWidget("stats_slot_"..playerIndex.."_frame"):SetColor(playerInfo.color)
		GetWidget("stats_slot_"..playerIndex.."_mdks"):SetText('^g' .. playerInfo.kills .. '^777/^r' .. playerInfo.deaths .. '^777/^y' .. playerInfo.assists)
		GetWidget("stats_slot_"..playerIndex.."_xpm"):SetText(round(playerInfo.XPpM))
		GetWidget("stats_slot_"..playerIndex.."_gpm"):SetText(round(playerInfo.GpM))
		GetWidget("stats_slot_"..playerIndex.."_creepKills"):SetText(round(playerInfo.creepKills))
		GetWidget("stats_slot_"..playerIndex.."_creepDenies"):SetText(round(playerInfo.denies))
		GetWidget("stats_slot_"..playerIndex.."_playerDmg"):SetText(PlayerStatToPercent(playerInfo.heroDamage, "heroDamage"))
		GetWidget("stats_slot_"..playerIndex.."_bldDmg"):SetText(PlayerStatToPercent(playerInfo.bldDamage, "bldDamage"))
	end
end

local function UpdatePlayerGraph(graphName, instantUpdate)
	-- set heights, sort, move, shuffle, etc.
	local graphTable = HoN_SpecUI.activePlayerGraphs[graphName]

	local max = 1 -- 1 so if the max is 0 it doesn't break, if the max is zero all the stats will be 0 anyways
	for i=0,9 do
		if (graphTable.stats[i] and (graphTable.stats[i] > max)) then
			max = graphTable.stats[i]
		end
	end

	-- set the heights
	for i=0,9 do
		if (graphTable.stats[i]) then
			GetWidget("specui_"..graphName.."_bar"..i):SetHeight(math.max(3, ((graphTable.stats[i] / max) * 100.0)).."%")
		else
			GetWidget("specui_"..graphName.."_bar"..i):SetHeight(0)
		end
	end

	-- sort
	local team0 = {0, 1, 2, 3, 4}

	local sortFunc0 = function(a, b)
		local cA, cB = graphTable.stats[a], graphTable.stats[b]
		if ((not cA) or (not HoN_SpecUI.playerInfo[a]) or (not HoN_SpecUI.playerInfo[a].exists)) then
			cA = -9001
		end
		if ((not cB) or (not HoN_SpecUI.playerInfo[b]) or (not HoN_SpecUI.playerInfo[b].exists)) then
			cB = -9001
		end

		return cA < cB
 	end
 	table.sort(team0, sortFunc0)

	local team1 = {5, 6, 7, 8, 9}
	local sortFunc1 = function(a, b)
		local cA, cB = graphTable.stats[a], graphTable.stats[b]
		if ((not cA) or (not HoN_SpecUI.playerInfo[a]) or (not HoN_SpecUI.playerInfo[a].exists)) then
			cA = -9001
		end
		if ((not cB) or (not HoN_SpecUI.playerInfo[b]) or (not HoN_SpecUI.playerInfo[b].exists)) then
			cB = -9001
		end

		return cA > cB
 	end
 	table.sort(team1, sortFunc1)

 	-- slide team 0
 	for i=1,5 do
 		local widget = GetWidget("specui_"..graphName.."_bar"..team0[i])
 		if (not instantUpdate) then
	 		widget:SlideX(4 * (i - 1).."h", 250)
	 		-- we need the set X, otherwise the slide starts itself over next time it's called
	 		widget:Sleep(300, function() widget:SetX(widget:GetX()) end)
	 	else
	 		widget:SetX(4 * (i - 1).."h")
	 	end
 	end

 	-- slide team 1
 	for i=1,5 do
 		local widget = GetWidget("specui_"..graphName.."_bar"..team1[i])
 		if (not instantUpdate) then
	 		widget:SlideX(4 * (i - 1).."h", 250)
	 		-- we need the set X, otherwise the slide starts itself over next time it's called
	 		widget:Sleep(300, function() widget:SetX(widget:GetX()) end)
	 	else
	 		widget:SetX(4 * (i - 1).."h")
	 	end
 	end
end

local function UpdatePlayerGraphsBars()
	-- repopulate bars and update sort
	for k,t in pairs(HoN_SpecUI.activePlayerGraphs) do
		PopulatePlayerGraphBars(k)
		UpdatePlayerGraph(k, true)
	end
end

local function UpdateIndexedPlayerExists(playerIndex, exists)
	-- show/hide seperators if they exist (top slots don't have them)
	if (WidgetExists("specui_seperator_overview_"..playerIndex, "game_spectator")) then
		GetWidget("specui_seperator_overview_"..playerIndex):SetVisible(exists)
		GetWidget("specui_seperator_stats_"..playerIndex):SetVisible(exists)
	end

	-- update their stats/player slot
	UpdatePlayersTabSlot(playerIndex)
	UpdateStatsTabSlot(playerIndex)

	-- overview and stats slots
	GetWidget("stats_slot_"..playerIndex):SetVisible(exists)
	GetWidget("overview_slot_"..playerIndex):SetVisible(exists)

	-- unit frame
	GetWidget("unit_frame_"..playerIndex):SetVisible(exists)

	-- update graph bars
	UpdatePlayerGraphsBars()

	-- update bar for graphs
	if (HoN_SpecUI.playerInfo[playerIndex]) then
		UpdatePlayerGraphBar(playerIndex, HoN_SpecUI.playerInfo[playerIndex])
	end
end

local function UpdateAllPlayerExistsVisibility()
	for i=0,9 do
		if (HoN_SpecUI.playerInfo[i]) then
			UpdateIndexedPlayerExists(i, HoN_SpecUI.playerInfo[i].exists)
		else
			UpdateIndexedPlayerExists(i, false)
		end
	end
end

local function BuybackCosts(playerIndex, buybackCost)
	if (not HoN_SpecUI.playerInfo[playerIndex]) then
		HoN_SpecUI.playerInfo[playerIndex] = {}
	end

	HoN_SpecUI.playerInfo[playerIndex].buybackCost = tonumber(buybackCost)

	-- update the players tab (only thing that uses the buybacks)
	UpdatePlayersTabSlot(playerIndex)
end
for i=0,9 do
	interface:RegisterWatch("SpectatorHeroBuybackCost"..i, function(_, ...) BuybackCosts(i, ...) end)
end

local function NumberBuybacks(playerIndex, numBuybacks)
	if (not HoN_SpecUI.playerInfo[playerIndex]) then
		HoN_SpecUI.playerInfo[playerIndex] = {}
	end

	HoN_SpecUI.playerInfo[playerIndex].numBuybacks = tonumber(numBuybacks)

	-- update the players tab (only thing that uses the buybacks)
	UpdatePlayersTabSlot(playerIndex)
end
for i=0,9 do
	interface:RegisterWatch("SpectatorHeroNumBuybacksRemaining"..i, function(_, ...) NumberBuybacks(i, ...) end)
end

local function PlayerExists(playerIndex, exists)
	exists = AtoB(exists)
	if (not HoN_SpecUI.playerInfo[playerIndex]) then
		HoN_SpecUI.playerInfo[playerIndex] = {}
	end

	-- we are switching from not existing to existing, if we have stats, we want to add them now
	-- (since they will be subtracted next time PlayerInfo triggers)
	if ((not HoN_SpecUI.playerInfo[playerIndex].exists) and exists and HoN_SpecUI.playerInfo[playerIndex].kills) then -- if kills exists, they all do
		--HoN_SpecUI.playerInfoTotals.kills = (HoN_SpecUI.playerInfoTotals.kills or 0) + HoN_SpecUI.playerInfo[playerIndex].kills
		--HoN_SpecUI.playerInfoTotals.deaths = (HoN_SpecUI.playerInfoTotals.deaths or 0) + HoN_SpecUI.playerInfo[playerIndex].deaths
		--HoN_SpecUI.playerInfoTotals.assists = (HoN_SpecUI.playerInfoTotals.assists or 0) + HoN_SpecUI.playerInfo[playerIndex].assists
		HoN_SpecUI.playerInfoTotals.XPpM = (HoN_SpecUI.playerInfoTotals.XPpM or 0) + HoN_SpecUI.playerInfo[playerIndex].XPpM
		HoN_SpecUI.playerInfoTotals.GpM = (HoN_SpecUI.playerInfoTotals.GpM or 0) + HoN_SpecUI.playerInfo[playerIndex].GpM
		--HoN_SpecUI.playerInfoTotals.gold = (HoN_SpecUI.playerInfoTotals.gold or 0) + HoN_SpecUI.playerInfo[playerIndex].gold
		--HoN_SpecUI.playerInfoTotals.goldSpent = (HoN_SpecUI.playerInfoTotals.goldSpent or 0) + HoN_SpecUI.playerInfo[playerIndex].goldSpent
		HoN_SpecUI.playerInfoTotals.heroDamage = (HoN_SpecUI.playerInfoTotals.heroDamage or 0) + HoN_SpecUI.playerInfo[playerIndex].heroDamage
		HoN_SpecUI.playerInfoTotals.bldDamage = (HoN_SpecUI.playerInfoTotals.bldDamage or 0) + HoN_SpecUI.playerInfo[playerIndex].bldDamage
		--HoN_SpecUI.playerInfoTotals.creepKills = (HoN_SpecUI.playerInfoTotals.creepKills or 0) + HoN_SpecUI.playerInfo[playerIndex].creepKills
		--HoN_SpecUI.playerInfoTotals.denies = (HoN_SpecUI.playerInfoTotals.denies or 0) + HoN_SpecUI.playerInfo[playerIndex].denies
	elseif (HoN_SpecUI.playerInfo[playerIndex].exists and (not exists) and HoN_SpecUI.playerInfo[playerIndex].kills) then -- we are ceasing to exist, we need to remove all our values from the totals
		--HoN_SpecUI.playerInfoTotals.kills = HoN_SpecUI.playerInfoTotals.kills - HoN_SpecUI.playerInfo[playerIndex].kills
		--HoN_SpecUI.playerInfoTotals.deaths = HoN_SpecUI.playerInfoTotals.deaths - HoN_SpecUI.playerInfo[playerIndex].deaths
		--HoN_SpecUI.playerInfoTotals.assists = HoN_SpecUI.playerInfoTotals.assists - HoN_SpecUI.playerInfo[playerIndex].assists
		HoN_SpecUI.playerInfoTotals.XPpM = HoN_SpecUI.playerInfoTotals.XPpM - HoN_SpecUI.playerInfo[playerIndex].XPpM
		HoN_SpecUI.playerInfoTotals.GpM = HoN_SpecUI.playerInfoTotals.GpM - HoN_SpecUI.playerInfo[playerIndex].GpM
		--HoN_SpecUI.playerInfoTotals.gold = HoN_SpecUI.playerInfoTotals.gold - HoN_SpecUI.playerInfo[playerIndex].gold
		--HoN_SpecUI.playerInfoTotals.goldSpent = HoN_SpecUI.playerInfoTotals.goldSpent - HoN_SpecUI.playerInfo[playerIndex].goldSpent
		HoN_SpecUI.playerInfoTotals.heroDamage = HoN_SpecUI.playerInfoTotals.heroDamage - HoN_SpecUI.playerInfo[playerIndex].heroDamage
		HoN_SpecUI.playerInfoTotals.bldDamage = HoN_SpecUI.playerInfoTotals.bldDamage - HoN_SpecUI.playerInfo[playerIndex].bldDamage
		--HoN_SpecUI.playerInfoTotals.creepKills = HoN_SpecUI.playerInfoTotals.creepKills - HoN_SpecUI.playerInfo[playerIndex].creepKills
		--HoN_SpecUI.playerInfoTotals.denies= HoN_SpecUI.playerInfoTotals.denies - HoN_SpecUI.playerInfo[playerIndex].denies
	end

	HoN_SpecUI.playerInfo[playerIndex].exists = exists

	-- Update slot visibility and such
	UpdateIndexedPlayerExists(playerIndex, exists)
end
for i=0,9 do
	interface:RegisterWatch("SpectatorHeroExists"..i, function(_, ...) PlayerExists(i, ...) end)
end

local function PlayerInfo(playerIndex, playerName, heroName, heroIcon, color, level, kills, deaths, assists, alive, XPpM, GpM, gold, goldSpent, heroDamage, bldDamage, creepKills, denies)
	if (not HoN_SpecUI.playerInfo[playerIndex]) then
		HoN_SpecUI.playerInfo[playerIndex] = {}
	elseif (HoN_SpecUI.playerInfo[playerIndex].exists and HoN_SpecUI.playerInfo[playerIndex].kills) then -- if kills exists they all do
		-- subtract the current value from the totals
		--HoN_SpecUI.playerInfoTotals.kills = HoN_SpecUI.playerInfoTotals.kills - HoN_SpecUI.playerInfo[playerIndex].kills
		--HoN_SpecUI.playerInfoTotals.deaths = HoN_SpecUI.playerInfoTotals.deaths - HoN_SpecUI.playerInfo[playerIndex].deaths
		--HoN_SpecUI.playerInfoTotals.assists = HoN_SpecUI.playerInfoTotals.assists - HoN_SpecUI.playerInfo[playerIndex].assists
		HoN_SpecUI.playerInfoTotals.XPpM = HoN_SpecUI.playerInfoTotals.XPpM - HoN_SpecUI.playerInfo[playerIndex].XPpM
		HoN_SpecUI.playerInfoTotals.GpM = HoN_SpecUI.playerInfoTotals.GpM - HoN_SpecUI.playerInfo[playerIndex].GpM
		--HoN_SpecUI.playerInfoTotals.gold = HoN_SpecUI.playerInfoTotals.gold - HoN_SpecUI.playerInfo[playerIndex].gold
		--HoN_SpecUI.playerInfoTotals.goldSpent = HoN_SpecUI.playerInfoTotals.goldSpent - HoN_SpecUI.playerInfo[playerIndex].goldSpent
		HoN_SpecUI.playerInfoTotals.heroDamage = HoN_SpecUI.playerInfoTotals.heroDamage - HoN_SpecUI.playerInfo[playerIndex].heroDamage
		HoN_SpecUI.playerInfoTotals.bldDamage = HoN_SpecUI.playerInfoTotals.bldDamage - HoN_SpecUI.playerInfo[playerIndex].bldDamage
		--HoN_SpecUI.playerInfoTotals.creepKills = HoN_SpecUI.playerInfoTotals.creepKills - HoN_SpecUI.playerInfo[playerIndex].creepKills
		--HoN_SpecUI.playerInfoTotals.denies= HoN_SpecUI.playerInfoTotals.denies - HoN_SpecUI.playerInfo[playerIndex].denies
	end

	HoN_SpecUI.playerInfo[playerIndex].playerName = playerName
	HoN_SpecUI.playerInfo[playerIndex].heroName = heroName
	HoN_SpecUI.playerInfo[playerIndex].heroIcon = heroIcon
	HoN_SpecUI.playerInfo[playerIndex].color = color
	HoN_SpecUI.playerInfo[playerIndex].level = tonumber(level)
	HoN_SpecUI.playerInfo[playerIndex].kills = tonumber(kills)
	HoN_SpecUI.playerInfo[playerIndex].deaths = tonumber(deaths)
	HoN_SpecUI.playerInfo[playerIndex].assists = tonumber(assists)
	HoN_SpecUI.playerInfo[playerIndex].alive = AtoB(alive)
	HoN_SpecUI.playerInfo[playerIndex].XPpM = tonumber(XPpM)
	HoN_SpecUI.playerInfo[playerIndex].GpM = tonumber(GpM)
	HoN_SpecUI.playerInfo[playerIndex].gold = tonumber(gold)
	HoN_SpecUI.playerInfo[playerIndex].goldSpent = tonumber(goldSpent)
	HoN_SpecUI.playerInfo[playerIndex].heroDamage = tonumber(heroDamage)
	HoN_SpecUI.playerInfo[playerIndex].bldDamage = tonumber(bldDamage)
	HoN_SpecUI.playerInfo[playerIndex].creepKills = tonumber(creepKills)
	HoN_SpecUI.playerInfo[playerIndex].denies = tonumber(denies)

	-- add the new value to the totals
	if (HoN_SpecUI.playerInfo[playerIndex].exists and HoN_SpecUI.playerInfoTotals and HoN_SpecUI.playerInfo[playerIndex].kills) then
		--HoN_SpecUI.playerInfoTotals.kills = (HoN_SpecUI.playerInfoTotals.kills or 0) + HoN_SpecUI.playerInfo[playerIndex].kills
		--HoN_SpecUI.playerInfoTotals.deaths = (HoN_SpecUI.playerInfoTotals.deaths or 0) + HoN_SpecUI.playerInfo[playerIndex].deaths
		--HoN_SpecUI.playerInfoTotals.assists = (HoN_SpecUI.playerInfoTotals.assists or 0) + HoN_SpecUI.playerInfo[playerIndex].assists
		HoN_SpecUI.playerInfoTotals.XPpM = (HoN_SpecUI.playerInfoTotals.XPpM or 0) + HoN_SpecUI.playerInfo[playerIndex].XPpM
		HoN_SpecUI.playerInfoTotals.GpM = (HoN_SpecUI.playerInfoTotals.GpM or 0) + HoN_SpecUI.playerInfo[playerIndex].GpM
		--HoN_SpecUI.playerInfoTotals.gold = (HoN_SpecUI.playerInfoTotals.gold or 0) + HoN_SpecUI.playerInfo[playerIndex].gold
		--HoN_SpecUI.playerInfoTotals.goldSpent = (HoN_SpecUI.playerInfoTotals.goldSpent or 0) + HoN_SpecUI.playerInfo[playerIndex].goldSpent
		HoN_SpecUI.playerInfoTotals.heroDamage = (HoN_SpecUI.playerInfoTotals.heroDamage or 0) + HoN_SpecUI.playerInfo[playerIndex].heroDamage
		HoN_SpecUI.playerInfoTotals.bldDamage = (HoN_SpecUI.playerInfoTotals.bldDamage or 0) + HoN_SpecUI.playerInfo[playerIndex].bldDamage
		--HoN_SpecUI.playerInfoTotals.creepKills = (HoN_SpecUI.playerInfoTotals.creepKills or 0) + HoN_SpecUI.playerInfo[playerIndex].creepKills
		--HoN_SpecUI.playerInfoTotals.denies = (HoN_SpecUI.playerInfoTotals.denies or 0) + HoN_SpecUI.playerInfo[playerIndex].denies

		-- update any graph bars and store the stat for graphs that use this
		UpdatePlayerGraphBar(playerIndex, HoN_SpecUI.playerInfo[playerIndex])

		-- update the stat tabs
		UpdatePlayersTabSlot(playerIndex)
		UpdateStatsTabSlot(playerIndex)
	end
end
for i=0,9 do
	interface:RegisterWatch("SpectatorPlayer"..i, function(_, ...) PlayerInfo(i, ...) end)
end

local function UpdatePushBar(pushBarType, pushTo, pushDuration, teamPushing)
	if (WidgetExists("specui_hellbourne_push"..pushBarType, "game_spectator")) then --normal mode
		GetWidget("specui_hellbourne_push"..pushBarType.."_parent"):ScaleWidth('-'..(math.min(98, math.max(2, (((pushTo-50)*PUSH_EXAGGERATION)+50))))..'%', pushDuration, 0)
		local pushHighlight = GetWidget("specui_hellbourne_push"..pushBarType)
		if (teamPushing == 1) then
			pushHighlight:FadeIn(pushDuration/3)
			pushHighlight:Sleep(pushDuration/3, function() pushHighlight:FadeOut(100) end)
		else
			pushHighlight:SetVisible(0)
		end

		GetWidget("specui_legion_push"..pushBarType.."_parent"):ScaleWidth((math.min(98, math.max(2, (((pushTo-50)*PUSH_EXAGGERATION)+50))))..'%', pushDuration, 0)
		local pushHighlight = GetWidget("specui_legion_push"..pushBarType)
		if (teamPushing == -1) then
			pushHighlight:FadeIn(pushDuration/3)
			pushHighlight:Sleep(pushDuration/3, function() pushHighlight:FadeOut(100) end)
		else
			pushHighlight:SetVisible(0)
		end
	elseif ((WidgetExists("specui_mini_hellbourne_push"..pushBarType, "game_spectator"))) then -- mini mode
		GetWidget("specui_mini_hellbourne_push"..pushBarType):ScaleWidth('-'..(math.min(98, math.max(2, (((pushTo-50)*PUSH_EXAGGERATION)+48) )))..'%', pushDuration, 0)
		GetWidget("specui_mini_legion_push"..pushBarType):ScaleWidth((math.min(98, math.max(2, (((pushTo-50)*PUSH_EXAGGERATION)+50)  )))..'%', pushDuration, 0)
	else
		printdb("SpecUI- Trying to update an unknown PushBar \'"..pushBarType.."\'")
	end
end

local function UpdatePlayerGraphs()
	for k,v in pairs(HoN_SpecUI.activePlayerGraphs) do
		UpdatePlayerGraph(k)
	end
end

local function UpdateBottomSize(tabID)
	-- determine the team sizes, then determine how large we need to make the cover to fit to them
	if (tabID and HoN_SpecUI.registeredTabs[tabID]) then
		HoN_SpecUI.teamSizes = {[0] = 0, [1] = 0}
		HoN_SpecUI.teamMax = 0

		for i=0,4 do
			if (HoN_SpecUI.playerInfo[i] and HoN_SpecUI.playerInfo[i].exists) then
				HoN_SpecUI.teamSizes[0] = HoN_SpecUI.teamSizes[0] + 1
			end
		end
		for i=5,9 do
			if (HoN_SpecUI.playerInfo[i] and HoN_SpecUI.playerInfo[i].exists) then
				HoN_SpecUI.teamSizes[1] = HoN_SpecUI.teamSizes[1] + 1
			end
		end
		HoN_SpecUI.teamMax = math.max(HoN_SpecUI.teamSizes[0], HoN_SpecUI.teamSizes[1])

		local bottomWidget = GetWidget("spec_bottom_parent")
		bottomWidget:SetWidth(HoN_SpecUI.registeredTabs[tabID].parentWidth)
		if (HoN_SpecUI.registeredTabs[tabID].fitToPlayers) then
			bottomWidget:SetHeight(((HoN_SpecUI.teamMax) * bottomWidget:GetHeightFromString('3.2h')) + bottomWidget:GetHeightFromString('5.0h'))
		else
			bottomWidget:SetHeight(HoN_SpecUI.registeredTabs[tabID].parentHeight)
		end
	end
end

local function UpdateBuildingInfoDisplay(visible, instant)
	local legionBld = GetWidget("specui_legion_bldInfo")
	local hellbourneBld = GetWidget("specui_hellbourne_bldInfo")

	if (visible) then
		if (instant) then
			legionBld:SetY('50%')
			hellbourneBld:SetY('50%')
		else
			legionBld:SlideY('50%', 150)
			hellbourneBld:SlideY('50%', 150)
		end

		legionBld:Sleep(1, function() legionBld:SetVisible(1) end)
		hellbourneBld:Sleep(1, function() hellbourneBld:SetVisible(1) end)
	else
		if (instant) then
			legionBld:SetY('20%')
			legionBld:SetVisible(0)
			hellbourneBld:SetY('20%')
			hellbourneBld:SetVisible(0)
		else
			legionBld:SlideY('20%', 150)
			legionBld:Sleep(150, function() legionBld:SetVisible(0) end)
			hellbourneBld:SlideY('20%', 150)
			hellbourneBld:Sleep(150, function() hellbourneBld:SetVisible(0) end)
		end
	end
end

local function UpdateDataInfoDisplay(visible, instant)
	local legionData = GetWidget("specui_legion_info_display")
	local hellbourneData = GetWidget("specui_hellbourne_info_display")

	if (visible) then
		if (instant) then
			legionData:SetX('-10%')
			hellbourneData:SetX('10%')
		else
			legionData:SlideX('-10%', 150)
			hellbourneData:SlideX('10%', 150)
		end

		-- sleep so we interrupt any sleeps waiting to hide it
		legionData:Sleep(1, function() legionData:SetVisible(1) end)
		hellbourneData:Sleep(1, function() hellbourneData:SetVisible(1) end)
	else
		if (instant) then
			legionData:SetX('6%')
			legionData:SetVisible(0)
			hellbourneData:SetX('-6%')
			hellbourneData:SetVisible(0)
		else
			legionData:SlideX('6%', 150)
			legionData:Sleep(150, function() legionData:SetVisible(0) end)
			hellbourneData:SlideX('-6%', 150)
			hellbourneData:Sleep(150, function() hellbourneData:SetVisible(0) end)
		end
	end
end

local function UpdatePushBarVisible(visible, instant)
	local pushBar
	if GetMap() == 'soccer' then
		pushBar = GetWidget("game_top_soccer_spec")
	else
		pushBar = GetWidget("spec_top_parent")
	end

	if (visible) then
		if (instant) then
			pushBar:SetY('0')
			pushBar:SetVisible(1)
		else
			pushBar:SlideY('0', 150)
			pushBar:FadeIn(250)
		end
	else
		if (instant) then
			pushBar:SetY('-15h')
			pushBar:SetVisible(0)
		else
			pushBar:SlideY('-15h', 150)
			pushBar:FadeOut(250)
		end
	end

	-- set the minimap button state
	if (visible) then
		GetWidget("btn_specui_push_visible"):SetButtonState(0)
	else
		GetWidget("btn_specui_push_visible"):SetButtonState(1)
	end
end

local function UpdateTabsVisible(visible, instant)
	local tabs = GetWidget("spec_bottom_parent")

	if (visible) then
		if (instant) then
			tabs:SetVisible(1)
		else
			tabs:FadeIn(150)
		end
	else
		if (instant) then
			tabs:SetVisible(0)
		else
			tabs:FadeOut(150)
		end
	end

	-- set the minimap button state
	if (visible) then
		GetWidget("btn_specui_bottom_tab_visible"):SetButtonState(0)
	else
		GetWidget("btn_specui_bottom_tab_visible"):SetButtonState(1)
	end
end

local function UpdateUnitFramePositions(position, instant)
	local destLeft, destRight = 0, 0
	if (position == 0) then
		destLeft = "-22.4h"
		destRight = "22.4h"
	elseif (position == 1) then
		destLeft = "-7.4h"
		destRight = "7.4h"
	end

	if (instant) then
		GetWidget("UnitFramesLeft"):SetX(destLeft)
	else
		GetWidget("UnitFramesLeft"):SlideX(destLeft, 250)
	end

	if (instant) then
		GetWidget("UnitFramesRight"):SetX(destRight)
	else
		GetWidget("UnitFramesRight"):SlideX(destRight, 250)
	end


	-- set the minimap button state
	if (position == 2) then
		GetWidget("btn_specui_unitframe_state"):SetButtonState(2)
	elseif (position == 1) then
		GetWidget("btn_specui_unitframe_state"):SetButtonState(0)
	else
		GetWidget("btn_specui_unitframe_state"):SetButtonState(1)
	end
end

local function UpdateHealthLabelsVisible(visible)
	for i=0,9 do
		GetWidget("specui_unit"..i.."_health"):SetVisible(visible)
	end
end

local function UpdateManaLabelsVisible(visible)
	for i=0,9 do
		GetWidget("specui_unit"..i.."_mana"):SetVisible(visible)
	end
end

local function UpdateAbilityCooldownsVisible(visible)
	for i=0,9 do
		for j=0,3 do
			GetWidget("specui_unit"..i.."_ability"..j.."Cooldown"):SetVisible(visible)
		end
	end
end

local function UpdateInterface()
	if (HoN_SpecUI.save.pushBarVisible) then
		-- experience push bar
		local totalExp = HoN_SpecUI.teamExperience[0] + HoN_SpecUI.teamExperience[1]
		local team0percent, team1percent = 50, 50
		if (totalExp ~= 0) then
			team0percent = (HoN_SpecUI.teamExperience[0] / totalExp) * 100.0
			team1percent = (HoN_SpecUI.teamExperience[1] / totalExp) * 100.0
		end

		if (team0percent ~= HoN_SpecUI.currentPushExp) then
			local change = HoN_SpecUI.currentPushExp - team0percent
			local teamPushing = 0

			if (change >= 1) then teamPushing = 1 elseif (change <= -1) then teamPushing = -1 end

			HoN_SpecUI.currentPushExp = team0percent

			UpdatePushBar("Exp", team0percent, PUSH_DURATION, teamPushing)
		end

		-- gold push bar
		local totalGold = HoN_SpecUI.teamGold[0] + HoN_SpecUI.teamGold[1]
		team0percent, team1percent = 50, 50
		if (totalGold ~= 0) then
			team0percent = (HoN_SpecUI.teamGold[0] / totalGold) * 100.0
			team1percent = (HoN_SpecUI.teamGold[1] / totalGold) * 100.0
		end

		if (team0percent ~= HoN_SpecUI.currentPushGold) then
			local change = HoN_SpecUI.currentPushGold - team0percent
			local teamPushing = 0

			if (change >= 1) then teamPushing = 1 elseif (change <= -1) then teamPushing = -1 end

			HoN_SpecUI.currentPushGold = team0percent

			UpdatePushBar("Gold", team0percent, PUSH_DURATION, teamPushing)
		end

		-- side texts
		if (HoN_SpecUI.save.dataInfoDisplayVisible) then
			UpdateSideDataTexts()
		end
	end

	-- update graphs
	if (HoN_SpecUI.save.tabsVisible) then
		if (HoN_SpecUI.ActivePlayerGraph) then
			UpdatePlayerGraph(HoN_SpecUI.ActivePlayerGraph)
		end

		-- update cover size
		if (HoN_SpecUI.save.activeTab) then
			UpdateBottomSize(HoN_SpecUI.save.activeTab)
		end
	end

	-- replacement for the throttle, especially given it couldn't be turned off
	if (HoN_SpecUI.uiActive) then
		GetWidget("spectator_update_sleeper"):Sleep(UPDATE_INTERVAL, UpdateInterface)
	end
end

local function RestorePanels()
	UpdateBuildingInfoDisplay(HoN_SpecUI.save.buildingInfoVisible)
	UpdateDataInfoDisplay(HoN_SpecUI.save.dataInfoDisplayVisible)
	UpdatePushBarVisible(HoN_SpecUI.save.pushBarVisible)
	UpdateTabsVisible(HoN_SpecUI.save.tabsVisible)
	UpdateUnitFramePositions(HoN_SpecUI.save.unitFrameMode)
end

local function ShowAll(_, pressed)
	if (AtoB(pressed)) then
		UpdateBuildingInfoDisplay(true)
		UpdateDataInfoDisplay(true)
		UpdatePushBarVisible(true)
		UpdateTabsVisible(true)
		UpdateUnitFramePositions(2)
		UpdateManaLabelsVisible(true)
		UpdateHealthLabelsVisible(true)
		UpdateAbilityCooldownsVisible(true)
	else
		RestorePanels()
	end
end
interface:RegisterWatch("SpecUIShowAll", ShowAll)

local function HideAll(_, pressed)
	if (AtoB(pressed)) then
		UpdatePushBarVisible(false)
		UpdateTabsVisible(false)
		UpdateUnitFramePositions(0)
	else
		RestorePanels()
	end
end
interface:RegisterWatch("SpecUIHideAll", HideAll)

local function ToggleAll(_, pressed)
	if (AtoB(pressed)) then
		HoN_SpecUI:TogglePushBarVisible()
		HoN_SpecUI:ToggleTabsVisible()
		-- frames
		if (HoN_SpecUI.save.unitFrameMode >= 1) then
			HoN_SpecUI.save.unitFrameMode = 0
		else
			HoN_SpecUI.save.unitFrameMode = 1
		end
		UpdateUnitFramePositions(HoN_SpecUI.save.unitFrameMode)
		SaveSpecUISettings()
	end
end
interface:RegisterWatch("ReplayToggle1", ToggleAll)

local function CombatNotification(_, entity, index)
	if (GetCvarBool('specui_show_popup_gank')) then
		index = tonumber(index)

		local exists = false
		local freeSlot = nil
		for i=1,3 do
			if (HoN_SpecUI.combatNotifications[i]) then
				if (HoN_SpecUI.combatNotifications[i] == index) then
					exists = true
					break
				end
			elseif (not freeSlot) then
				freeSlot = i
			end
		end

		if ((not exists) and freeSlot) then
			HoN_SpecUI.combatNotifications[freeSlot] = index
			-- populate the notification
			GetWidget("specui_notification_"..freeSlot.."_icon"):SetTexture(GetEntityIconPath(entity))
			GetWidget("specui_notification_"..freeSlot.."_hotkey"):SetText(interface:UICmd("GetKeybindButton('spectator', 'TriggerToggle', 'SpecUICombat"..freeSlot.."')"))
			GetWidget("specui_notification_"..freeSlot.."_hotkey"):SetVisible(GetCvarBool("specui_show_hotkeys"))

			local parentWidget = GetWidget("specui_notification_"..freeSlot)
			parentWidget:FadeIn(100)
			parentWidget:SlideY('8h', 100)
			parentWidget:Sleep((1000 * GetCvarNumber('specui_combatpop_display')), function()
				parentWidget:SlideY('-3h', 100)
				parentWidget:Sleep(120, function()
					parentWidget:SetVisible(0)
					parentWidget:Sleep((1000 * GetCvarNumber('specui_combatpop_cooldown')), function()
						HoN_SpecUI.combatNotifications[freeSlot] = nil
					end)
				end)
			end)
		end
	end
end
interface:RegisterWatch("SpectatorGankAlert", CombatNotification)

local function CombatJump(index, _, select)
	if (HoN_SpecUI.combatNotifications[index]) then
		if AtoB(select) then
			-- twice so it jumps to the player
			interface:UICmd("SelectUnit("..HoN_SpecUI.combatNotifications[index]..")")
			interface:UICmd("SelectUnit("..HoN_SpecUI.combatNotifications[index]..")")
			-- slide the icon out
			local parentWidget = GetWidget("specui_notification_"..index)
			parentWidget:SlideY('-3h', 100)
			parentWidget:Sleep(120, function()
				parentWidget:SetVisible(0)
				parentWidget:Sleep((1000 * GetCvarNumber('specui_combatpop_cooldown')), function()
					HoN_SpecUI.combatNotifications[index] = nil
				end)
			end)
		end
	end
end
for i=1,3 do
	interface:RegisterWatch("SpecUICombat"..i, function(...) CombatJump(i, ...) end)
end

local function ItemPurchased(_, itemName, itemCost, isScroll, isRecipe, playerIndex)
	itemCost, isScroll, isRecipe, playerIndex = tonumber(itemCost), AtoB(isScroll), AtoB(isRecipe), tonumber(playerIndex)

	-- lol if condition
	if (NotEmpty(itemName) and HoN_SpecUI.playerInfo[playerIndex] and HoN_SpecUI.playerInfo[playerIndex].exists and ((itemCost > GetCvarInt('specui_itempop_gold_threshold')) or isRecipe) and (not (isScroll and (not isRecipe))) and GetCvarBool('specui_show_popup_item')) then
		if ((not HoN_SpecUI.itemPops[playerIndex]) or isRecipe) then
			HoN_SpecUI.itemPops[playerIndex] = true

			local parent = GetWidget("specui_ItemPopout"..playerIndex)
			local image = GetWidget("spec_animated_item"..playerIndex)
			local gold = GetWidget("spec_animated_gold"..playerIndex)

			local function HostTimeFunc(widget, _, time)
				widget:SetRotation(math.sin(((tonumber(time) % 300) / 300) * (3.14159265 * 2)) * 2)
			end

			-- weeeeeee, animation is fun
			parent:Sleep(100, function()
				HoN_SpecUI.itemPops[playerIndex] = nil
				image:SetTexture(GetEntityIconPath(itemName))
				parent:FadeIn(100)
				parent:UnregisterWatch("HostTime")
				parent:SetRotation(0)
				image:UnregisterWatch("HostTime")
				image:SetRotation(0)
				parent:SetHeight('7h')
				parent:SetWidth('7h')
				parent:DoEventN(0) 	-- sets the x based on the {infoalign} value
				parent:SetY('0')
				parent:Sleep(100, function()
					parent:FadeIn(100)
					PlaySound('/ui/fe2/store/sounds/coin_jingle_4.wav', 0.4, 1)
					gold:StartAnim()
					parent:Sleep(0, function()
						parent:RegisterWatch("HostTime", function(...) HostTimeFunc(parent, ...) end)
						image:RegisterWatch("HostTime", function(...) HostTimeFunc(image, ...) end)
						parent:Scale('14h', '14h', 500, false)
						parent:Sleep(250, function()
							parent:Scale('7h', '7h', 500, false)
							parent:Sleep(500, function()
								parent:DoEventN(1) -- slide based on {infoalign}
								parent:UnregisterWatch("HostTime")
								parent:SetRotation(0)
								image:UnregisterWatch("HostTime")
								image:SetRotation(0)
								self:FadeOut(800)
							end)
						end)
					end)
				end)
			end)
		end
	end
end
interface:RegisterWatch("SpectatorItemPurchased", ItemPurchased)

local function ReplayLockToggle(_, toggle)
	toggle = AtoB(toggle)

	if (toggle) then
		HoN_SpecUI.wasLocked = GetCvarBool('cg_lockcamera')
		Set('cg_lockcamera', not HoN_SpecUI.wasLocked)
	elseif (HostTime() >= HoN_SpecUI.lockedTime) then
		Set('cg_lockcamera', HoN_SpecUI.wasLocked)
	end

	HoN_SpecUI.lockedTime = HostTime() + 250
end
interface:RegisterWatch("ReplayLockToggle", ReplayLockToggle)

function HoN_SpecUI:StartInterface()
	HoN_SpecUI.uiActive = true
	-- refresh positions and the such
	HoN_SpecUI:ResetInterface()

	-- start the update loop
	UpdateInterface()
end

function HoN_SpecUI:StopInterface()
	HoN_SpecUI.uiActive = false
	-- interrupt the sleep that's looping to cause updates
	GetWidget("spectator_update_sleeper"):Sleep(1, function() end)
end

function HoN_SpecUI:ResetInterface()
	-- move everything back to starting positions, setup displays, etc.
	HoN_SpecUI.teamGold = {[0] = 0, [1] = 0}
	HoN_SpecUI.teamExperience = {[0] = 0, [1] = 0}
	HoN_SpecUI.currentPushGold = 50
	HoN_SpecUI.currentPushExp = 50

	HoN_SpecUI.lastHealth1 = nil
	HoN_SpecUI.lastHealth0 = nil

	HoN_SpecUI.teamSizes = {[0] = 0, [1] = 0}
	HoN_SpecUI.teamMax = 0

	HoN_SpecUI.playerInfo = {}
	HoN_SpecUI.playerInfoTotals = {}

	-- clear out stat graphs and update
	for k,t in pairs(HoN_SpecUI.activePlayerGraphs) do
		t.stats = {}
	end

	UpdatePushBar("Gold", 50, 1, 0)
	UpdatePushBar("Exp", 50, 1, 0)

	UpdateSideDataTexts()
	UpdatePlayerGraphs()

	UpdateBuildingInfoDisplay(HoN_SpecUI.save.buildingInfoVisible, true)
	UpdateDataInfoDisplay(HoN_SpecUI.save.dataInfoDisplayVisible, true)
	UpdatePushBarVisible(HoN_SpecUI.save.pushBarVisible, true)
	UpdateTabsVisible(HoN_SpecUI.save.tabsVisible, true)
	UpdateUnitFramePositions(HoN_SpecUI.save.unitFrameMode, true)
	UpdateHealthLabelsVisible(HoN_SpecUI.save.healthLabelsVisible)
	UpdateManaLabelsVisible(HoN_SpecUI.save.manaLabelsVisible)
	UpdateAbilityCooldownsVisible(HoN_SpecUI.save.abilityCooldownsVisible)

	-- when this runs either nothing or that tab will be visible
	-- thus we make the UI act like there was no previous tab open
	if (HoN_SpecUI.save.activeTab) then
		local tab = HoN_SpecUI.save.activeTab
		HoN_SpecUI.save.activeTab = nil
		HoN_SpecUI:SetActiveTab(tab, true)
	end
end

function HoN_SpecUI:ChangeInfoDisplay()
	HoN_SpecUI.save.teamInfoDisplay = HoN_SpecUI.save.teamInfoDisplay + 1

	if (HoN_SpecUI.save.teamInfoDisplay >= 4) then
		HoN_SpecUI.save.teamInfoDisplay = 0
	end

	SaveSpecUISettings()

	UpdateSideDataTexts()
end

function HoN_SpecUI:RegisterPlayerStatGraph(graphName, statName, percent)
	HoN_SpecUI.activePlayerGraphs[graphName] = {}
	HoN_SpecUI.activePlayerGraphs[graphName].statName = statName
	HoN_SpecUI.activePlayerGraphs[graphName].percent = percent

	HoN_SpecUI.activePlayerGraphs[graphName].stats = {}
end

function HoN_SpecUI:SetActivePlayerGraph(graphName)
	HoN_SpecUI.ActivePlayerGraph = graphName
	PopulatePlayerGraphBars(graphName)
	UpdatePlayerGraph(graphName, true)
end

function HoN_SpecUI:RemoveActivePlayerGraph()
	HoN_SpecUI.ActivePlayerGraph = nil
end

function HoN_SpecUI:HoverGraphBar(graphName, index, label)
	local graphTable = HoN_SpecUI.activePlayerGraphs[graphName]
	local playerInfo = HoN_SpecUI.playerInfo[index]

	if (graphTable.percent) then
		Trigger('SpecUIGraphTooltipUpdate', playerInfo.playerName, playerInfo.color, (graphTable.stats[index] or 0), Translate(label), HoN_SpecUI.playerInfoTotals[graphTable.statName])
	else
		Trigger('SpecUIGraphTooltipUpdate', playerInfo.playerName, playerInfo.color, round(graphTable.stats[index] or 0), Translate(label), 0)
	end
	GetWidget('specui_graph_tooltips'):SetVisible(1)
end

function HoN_SpecUI:RegisterTab(tabID, fitToPlayers, parentHeight, parentWidth)
	HoN_SpecUI.registeredTabs[tabID] = {}
	HoN_SpecUI.registeredTabs[tabID].fitToPlayers = fitToPlayers
	HoN_SpecUI.registeredTabs[tabID].parentHeight = parentHeight
	HoN_SpecUI.registeredTabs[tabID].parentWidth = parentWidth
end

function HoN_SpecUI:SetActiveTab(tabID, instant)
	if (not GetWidget('spec_bottom_parent'):IsVisible()) then return end
	if (tabID and HoN_SpecUI.registeredTabs[tabID]) then
		-- hide the old tab or toggle the current if it's open
		if (HoN_SpecUI.save.activeTab) then
			if (HoN_SpecUI.save.activeTab == tabID) then
				HoN_SpecUI:CloseActiveTab()
				return
			else
				GetWidget("specui_bottom_tab_"..HoN_SpecUI.save.activeTab):SetVisible(0)
				if (WidgetExists("specui_bottom_header_tab_"..HoN_SpecUI.save.activeTab.."_cover", "game_spectator")) then
					GetWidget("specui_bottom_header_tab_"..HoN_SpecUI.save.activeTab.."_cover"):SetVisible(0)
				end
			end
		end

		HoN_SpecUI.save.activeTab = tabID
		SaveSpecUISettings()

		-- show/update for the new tab
		-- always do the slide because buggy code side stuff with the set hieghts in the update bottom size
		if (instant) then
			GetWidget("spec_bottom_tabs"):SetY(0)
			GetWidget("spec_bottom_tabs"):SetVisible(1)
		else
			GetWidget("spec_bottom_tabs"):SlideY(0, 150)
			GetWidget("spec_bottom_tabs"):FadeIn(150)
		end
		-- if a tab is suppose to be hiding, we will interrupt it's sleep to hid it now, or if it's suppose to
		-- hide what we want to open, we will just prevent it
		if (HoN_SpecUI.oldTab) then
			if (HoN_SpecUI.oldTab == tabID) then
				GetWidget("spec_bottom_tabs"):Sleep(1, function() HoN_SpecUI.oldTab = nil end)
			else
				GetWidget("spec_bottom_tabs"):Sleep(1, function() GetWidget("specui_bottom_tab_"..HoN_SpecUI.oldTab):SetVisible(0) HoN_SpecUI.oldTab = nil end)
			end
		end

		-- do this before actually setting it visible so that sizes are right when stuff like graphs update
		UpdateBottomSize(tabID)

		GetWidget("specui_bottom_tab_"..tabID):SetVisible(1)
		if (WidgetExists("specui_bottom_header_tab_"..HoN_SpecUI.save.activeTab.."_cover", "game_spectator")) then
			GetWidget("specui_bottom_header_tab_"..HoN_SpecUI.save.activeTab.."_cover"):SetVisible(1)
		end
	else-- we are saying to open something, but there is nothing to open (or at least nothing registered)
		-- just close whatever is open
		HoN_SpecUI:CloseActiveTab()
	end
end

function HoN_SpecUI:CloseActiveTab()
	if (HoN_SpecUI.save.activeTab) then
		HoN_SpecUI.oldTab = HoN_SpecUI.save.activeTab
		GetWidget("spec_bottom_tabs"):SlideY('21.5h', 150)
		GetWidget("spec_bottom_tabs"):FadeOut(150)
		GetWidget("spec_bottom_tabs"):Sleep(150, function() GetWidget("specui_bottom_tab_"..HoN_SpecUI.oldTab):SetVisible(0) HoN_SpecUI.oldTab = nil end)
		if (WidgetExists("specui_bottom_header_tab_"..HoN_SpecUI.save.activeTab.."_cover", "game_spectator")) then
			GetWidget("specui_bottom_header_tab_"..HoN_SpecUI.save.activeTab.."_cover"):SetVisible(0)
		end
	end

	HoN_SpecUI.save.activeTab = nil
	SaveSpecUISettings()
end

function HoN_SpecUI:ToggleBuildingInfoVisible()
	HoN_SpecUI.save.buildingInfoVisible = not HoN_SpecUI.save.buildingInfoVisible
	UpdateBuildingInfoDisplay(HoN_SpecUI.save.buildingInfoVisible)
	SaveSpecUISettings()
end

function HoN_SpecUI:ToggleDataInfoDisplayVisible()
	HoN_SpecUI.save.dataInfoDisplayVisible = not HoN_SpecUI.save.dataInfoDisplayVisible
	UpdateDataInfoDisplay(HoN_SpecUI.save.dataInfoDisplayVisible)
	SaveSpecUISettings()
end

function HoN_SpecUI:TogglePushBarVisible()
	HoN_SpecUI.save.pushBarVisible = not HoN_SpecUI.save.pushBarVisible
	UpdatePushBarVisible(HoN_SpecUI.save.pushBarVisible)
	SaveSpecUISettings()
end

function HoN_SpecUI:ToggleTabsVisible()
	HoN_SpecUI.save.tabsVisible = not HoN_SpecUI.save.tabsVisible
	UpdateTabsVisible(HoN_SpecUI.save.tabsVisible)
	SaveSpecUISettings()
end

function HoN_SpecUI:ToggleUnitFrames()
	HoN_SpecUI.save.unitFrameMode = HoN_SpecUI.save.unitFrameMode - 1
	if (HoN_SpecUI.save.unitFrameMode < 0) then
		HoN_SpecUI.save.unitFrameMode = 2
	end

	UpdateUnitFramePositions(HoN_SpecUI.save.unitFrameMode)
	SaveSpecUISettings()
end

function HoN_SpecUI:ToggleExpandedFrame()
	if (HoN_SpecUI.save.unitFrameMode == 2) then
		HoN_SpecUI.save.unitFrameMode = 1
	else
		HoN_SpecUI.save.unitFrameMode = 2
	end

	UpdateUnitFramePositions(HoN_SpecUI.save.unitFrameMode)
	SaveSpecUISettings()
end

function HoN_SpecUI:ToggledClosedFrame()
	if (HoN_SpecUI.save.unitFrameMode == 0) then
		HoN_SpecUI.save.unitFrameMode = 1
	else
		HoN_SpecUI.save.unitFrameMode = 0
	end

	UpdateUnitFramePositions(HoN_SpecUI.save.unitFrameMode)
	SaveSpecUISettings()
end

function HoN_SpecUI:ToggleHealthLabels()
	HoN_SpecUI.save.healthLabelsVisible = not HoN_SpecUI.save.healthLabelsVisible
	UpdateHealthLabelsVisible(HoN_SpecUI.save.healthLabelsVisible)
	SaveSpecUISettings()
end

function HoN_SpecUI:ToggleManaLabels()
	HoN_SpecUI.save.manaLabelsVisible = not HoN_SpecUI.save.manaLabelsVisible
	UpdateManaLabelsVisible(HoN_SpecUI.save.manaLabelsVisible)
	SaveSpecUISettings()
end

function HoN_SpecUI:ToggleAbilityCooldowns()
	HoN_SpecUI.save.abilityCooldownsVisible = not HoN_SpecUI.save.abilityCooldownsVisible
	UpdateAbilityCooldownsVisible(HoN_SpecUI.save.abilityCooldownsVisible)
	SaveSpecUISettings()
end

function HoN_SpecUI:GetPlayerInfo(playerIndex, infoVar)
	playerIndex = tonumber(playerIndex)

	if (HoN_SpecUI.playerInfo[playerIndex] and (HoN_SpecUI.playerInfo[playerIndex][infoVar] ~= nil)) then
		return HoN_SpecUI.playerInfo[playerIndex][infoVar]
	else
		return nil
	end
end

function HoN_SpecUI:ClickCombatNotification(index)
	index = tonumber(index)
	if (HoN_SpecUI.combatNotifications[index]) then
		-- twice so it jumps to the player
		interface:UICmd("SelectUnit("..HoN_SpecUI.combatNotifications[index]..")")
		interface:UICmd("SelectUnit("..HoN_SpecUI.combatNotifications[index]..")")
		-- slide the icon out
		local parentWidget = GetWidget("specui_notification_"..index)
		parentWidget:SlideY('-3h', 100)
		parentWidget:Sleep(120, function()
			parentWidget:SetVisible(0)
			parentWidget:Sleep((1000 * GetCvarNumber('specui_combatpop_cooldown')), function()
				HoN_SpecUI.combatNotifications[index] = nil
			end)
		end)
	end
end

function HoN_SpecUI:HoverPlayer(index)
	local pos = ''
	if (index >= 5) then
		pos = '-27h'
	end

	local body = ""
	if (AtoB(HoN_SpecUI.playerRespawnInfo[index].permaDead) and (tonumber(HoN_SpecUI.playerRespawnInfo[index].respawnTime) > 0)) then
		body = Translate("team_tip_permadead").."\n\n"
	end
	body = body .. Translate("specui_player_tip")

	Trigger('SpecUITooltipUpdate', HoN_SpecUI.playerInfo[index].playerName, body, '', '', pos, '')
end

function HoN_SpecUI:HeroRespawn(index, self, respawnTime, respawnDuration, respawnPercent, permaDead)
	HoN_SpecUI.playerRespawnInfo[index] = {
		['respawnTime'] = respawnTime,
		['permaDead'] = permaDead
	}
end
for i=0,9 do
	interface:RegisterWatch("SpectatorHeroRespawn"..i, function(...) HoN_SpecUI:HeroRespawn(i, ...) end)
end

------------------------------

local function ChatWhisperUpdate(...)
	if (GameChat) then
		GameChat.ChatWhisperUpdate(...)
	end
end
interface:RegisterWatch('ChatWhisperUpdate', ChatWhisperUpdate)

local function AllChatMessages(...)
	if (GameChat) then
		GameChat.AllChatMessages(...)
	end
end
interface:RegisterWatch('AllChatMessages', AllChatMessages)

function interface:HoNSpecUIF(func, ...)
  print(HoN_SpecUI[func](self, ...))
end

-- ===========================================================================

local lastBaseHealthVis = {}
lastBaseHealthVis[1]	= false
lastBaseHealthVis[2]	= false

local lastTeamScoreMax	= 0

local lastTeamScore	= {}
lastTeamScore[1]		= 0
lastTeamScore[2]		= 0

local function updateBaseHealthBarVis(baseID)	-- team index, more or less
	GetWidget('game_base_health_header_'..baseID..'_spec'):SetVisible(lastBaseHealthVis[baseID] and lastTeamScoreMax <= 0)
end

local function updateScoreLabel(baseID)
	local totalScore	= lastTeamScoreMax
	local score			= lastTeamScore[baseID]

	if baseID == 1 then
		GetWidget('legion_data_score'):SetText(score..'/'..totalScore)
	elseif baseID == 2 then
		GetWidget('hellbourne_data_score'):SetText(score..'/'..totalScore)
	end

	if totalScore > 0 then
		GetWidget('gameScoreHeaderSpec'..baseID):SetVisible(true)
		if totalScore < 99999 then
			GetWidget('gameScoreHeaderSpec'..baseID..'Label'):SetText(score..'/'..totalScore)
			GetWidget('gameScoreHeaderSpec'..baseID..'Bar'):SetVisible(true)
			GetWidget('gameScoreHeaderSpec'..baseID..'Bar'):SetWidth(ToPercent(math.min(1, score / totalScore)))
		else
			GetWidget('gameScoreHeaderSpec'..baseID..'Bar'):SetVisible(false)
			GetWidget('gameScoreHeaderSpec'..baseID..'Label'):SetText(score)
		end
	else
		GetWidget('gameScoreHeaderSpec'..baseID):SetVisible(false)
	end
end

for i=1,2,1 do
	GetWidget('gameScoreHeaderSpec'..i):RegisterWatch('ScoreboardInfo', function(widget, teamScoreGoal, playerScoreGoal)
		teamScoreGoal = AtoN(teamScoreGoal)
		lastTeamScoreMax	= teamScoreGoal

		updateBaseHealthBarVis(i)
		updateScoreLabel(i)
	end)

	GetWidget('gameScoreHeaderSpec'..i):RegisterWatch('ScoreboardTeam'..i, function(widget, p0, p1, p2, p3, p4, p5, teamScore)
		teamScore = AtoN(teamScore)
		lastTeamScore[i]	= teamScore

		updateScoreLabel(i)
	end)
end

local FLAG_STATE_IDLE		= 0
local FLAG_STATE_OUT		= 1
local FLAG_STATE_HELD		= 2

local function scoresFlagRegister(index)
	local lastState = FLAG_STATE_IDLE


	local heldEffect	= '/ui/elements/ctf/flag/has_flag_blue.effect'
	local flagColor		= '#46c4ff'
	if index == 1 then
		heldEffect	= '/ui/elements/ctf/flag/has_flag_yellow.effect'
		flagColor	= '#edb829'
	end

	interface:GetWidget('gameScoreHeaderSpec'..index..'Flag'):RegisterWatch('scoresFlagState'..index, function(widget, state)
		state = AtoN(state or FLAG_STATE_IDLE)

		if lastState <= FLAG_STATE_IDLE and state >= FLAG_STATE_OUT then
			widget:SetAnim('furlDn')
		elseif lastState >= FLAG_STATE_OUT and state <= FLAG_STATE_IDLE then
			widget:SetAnim('furlUp')
		end

		if state == FLAG_STATE_HELD and lastState ~= FLAG_STATE_HELD then
			widget:SetEffect(heldEffect)
			widget:SetTeamColor(flagColor)
		elseif lastState == FLAG_STATE_HELD then
			widget:SetEffect('')
			widget:SetTeamColor("#FFFFFF")
		end

		lastState = state
	end)
end

scoresFlagRegister(0)
scoresFlagRegister(1)
scoresFlagRegister = nil

function HoN_SpecUI:ToggleReplayControlUI()
	local widget = nil
	if (ViewingStreaming()) then
		widget = GetWidgetNoMem('replay_control_container2', 'game_replay_control')
	else
		widget = GetWidgetNoMem('replay_control_container', 'game_replay_control')
	end

	if widget then widget:SetVisible(not widget:IsVisible()) end
end