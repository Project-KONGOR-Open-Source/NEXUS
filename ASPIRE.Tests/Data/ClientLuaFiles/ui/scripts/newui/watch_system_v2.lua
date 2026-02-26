local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()
RegisterScript2('Watch_System', '30')

local dbprint = not GetCvarBool('releaseStage_stable')
local function EchoDebug(msg)
	if dbprint then
		Echo(msg)
	end
end

Watch_System = {}

local MatchInfoResultIdentifier = 1

local tabs = {'live', 'sea_matches', 'naeu_matches', 'bet_history', 'replay'}

local currentTab

local live_itemnum_perpage = 5
local betHistory_itemnum_perpage = 7
local betHistory_data = {}

local MatchStatus_Unknown 		= -1
local MatchStatus_NotStarted 	= 0
local MatchStatus_Binding 		= 1
local MatchStatus_OnGoing 		= 2
local MatchStatus_Finished 		= 3
local MatchStatus_Cancelled 	= 4
local MatchStatus_Hidden 		= 5

local _currentRegion = ''
local _isRegenerateListFromServer = false
local _selectedMatchDate
local _selectedMatchData
local _selectedMatchList = {}
local _regionDateMatches = {}
local _betInfoWinner
local _betInfoFirstTower
local _betInfoFirstBlood
local _betInfoTenKills
local _isConnectionWarnShowing = false
local _defaultMatchInfo = "{'match_id':0,'region':'0','data':{'replay_time':0,'game_phase':0,'match_time':0,'hero_kill1':0,'hero_kill2':0,'team1':[],'team2':[],'spectator':[],'building_status_1':'23.24,21.48 Tower|20.70,21.29 MRax|23.05,18.75 RRax|18.16,12.30 Tower|15.82,14.65 Tower|40.82,38.87 Tower|29.30,30.47 Tower|30.86,7.81 Tower|10.94,28.13 Tower|9.38,25.78 MRax|12.50,25.78 RRax|28.91,9.57 MRax|28.91,6.05 RRax|47.85,8.98 Tower|79.91,8.41 Tower|12.30,42.38 Tower|12.74,58.78 Tower','building_status_2':'73.44,70.31 Tower|76.56,71.09 MRax|74.22,73.44 RRax|80.47,75.59 Tower|78.32,78.13 Tower|54.49,51.76 Tower|65.63,60.55 Tower|50.60,85.76 Tower|19.34,86.13 Tower|87.48,65.23 Tower|89.06,67.38 MRax|85.94,67.97 RRax|67.18,84.39 Tower|69.53,82.62 MRax|69.92,86.13 RRax|88.09,52.73 Tower|86.91,36.91 Tower','finish':false,'chat_log':[],'combat_log':[]}}"

local _buildingInfo = {
	Tower={size="1h", img="/shared/icons/minimap_tower.tga"},
	MRax={size="0.7h", img="/shared/icons/minimap_melee.tga"},
	RRax={size="0.7h", img="/shared/icons/minimap_range.tga"}
}

local _minimapIconAdjustPos = -1.0
local _ExactPlayerColors = {'#0042FF','#1CE6B9','#9000C0','#FFFC01','#FE8A0E','#E55BB0','#959697','#7EBFF1','#106246','#8B4513'}

Watch_System.CurrentMatchJson = {}

local function RefreshTabs(tabName)
	for i=1, #tabs do
		local isCurrentTab = tabs[i] == tabName
		GetWidget('watch_system_tab_' .. tabs[i] .. '_selected', 'main'):SetVisible(isCurrentTab)
		GetWidget('watch_system_' .. tabs[i], 'main'):SetVisible(isCurrentTab)
	end
end

local function InitTabs()
	if (currentTab == nil) then
		currentTab = tabs[1]
		RefreshTabs(currentTab)
	end
end

function Watch_System.SelectTab(tabName)
	if (currentTab and currentTab == tabName) then
		PlaySound('/shared/sounds/ui/button_click_01.wav')
		return
	end

	MatchInfoResultIdentifier = MatchInfoResultIdentifier + 1
	Watch_System.CurrentMatchJson = {}

	PlaySound('/shared/sounds/ui/button_click_05.wav')

	currentTab = tabName

	RefreshTabs(currentTab)
end

function Watch_System.Clicked(toggle)
	if (toggle) or ( (not GetWidget('watch_system'):IsVisible()))  then
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'watch_system', nil, false)
	end
	InitTabs()
end

local function ShowWarning(warnLabel)
	GetWidget('watch_system_warning_label'):SetText(Translate(warnLabel))
	GetWidget('watch_system_warning'):SetVisible(true)

	local function CloseWarning()
		GetWidget('watch_system_warning'):SetVisible(false)
	end

	GetWidget('watch_system_warning_btn_close'):SetCallback('onclick', CloseWarning)
	GetWidget('watch_system_warning_btn_confirm'):SetCallback('onclick', CloseWarning)
end

local function ClearBattleLog()
	local logBuffer = GetWidget('watch_system_battlelog_chatbuffer_' .. _currentRegion)
	if logBuffer ~= nil then
		logBuffer:ClearBufferText()
	end
end

local function GetSelectedMatchStatus()
	if _selectedMatchData ~= nil then
		return tonumber(_selectedMatchData.status)
	end
	return MatchStatus_Unknown
end

local function HideMinimapHeroes()
	GetWidget('watch_system_minimap_heroes_' .. _currentRegion):ChildrenCall(function(child) child:SetVisible(false) end)
end

local function HideMinimapBuildings()
	GetWidget('watch_system_minimap_buildings_' .. _currentRegion):ChildrenCall(function(child) child:SetVisible(false) end)
end

local function ResetMinimap()
	EchoDebug('ResetMinimap')

	Watch_System.UnsubscribeMatchInfo()

	for team=1,2 do
		for slot=1, 5 do
			GetWidget('playerdata_entrypanel_'..team..slot.._currentRegion):SetVisible(false)
		end
	end

	ClearBattleLog()

	GetWidget('watch_system_live_header'):SetVisible(false)
	GetWidget('watch_system_region_header_'.._currentRegion):SetVisible(false)

	GetWidget('watch_system_minimap_win_team_'.._currentRegion):SetVisible(false)

	GetWidget('watch_system_team_kills_label_'.._currentRegion..'1'):SetText('')
	GetWidget('watch_system_team_kills_label_'.._currentRegion..'2'):SetText('')

	GetWidget('watch_system_elapsed_time_'.._currentRegion):SetVisible(false)

	GetWidget('watch_system_button_status_' .. _currentRegion .. '_notstart'):SetVisible(false)
	GetWidget('watch_system_button_status_' .. _currentRegion .. '_preparing'):SetVisible(false)
	GetWidget('watch_system_button_' .. _currentRegion):SetVisible(false)
	GetWidget('watch_system_button_' .. _currentRegion .. '_replay'):SetVisible(false)

	GetWidget('watch_system_minimap_icon_' .. _currentRegion):SetTexture('$invis')

	HideMinimapHeroes()
	HideMinimapBuildings()
end

local function UpdateMinimapBuildings()
	HideMinimapBuildings()

	local containerWdg = GetWidget('watch_system_minimap_buildings_'.._currentRegion)

	for team=1,2 do
		local buildingInfo = Watch_System.CurrentMatchJson['building_status_'..team]
		if buildingInfo ~= nil and #buildingInfo > 0 then

			local buildings = split(buildingInfo, "|")
			local buildingSlots = {Tower=0, MRax=0, RRax=0}

			for _, building in pairs(buildings) do
				local buildingArray = split(building, " ")
				local buildingType = (NotEmpty(buildingArray[2]) and #buildingArray[2] > 1) and buildingArray[2] or 'Tower'
				local widgetName = 'watch_system_minimap_building_'..team..buildingSlots[buildingType].._currentRegion..buildingType

				--Check if there is a building widget
				if not containerWdg:HasChildWidget(widgetName) then
					containerWdg:Instantiate('watch_system_minimap_building',
						'team', team,
						'slot', buildingSlots[buildingType],
						'category', _currentRegion,
						'type', buildingType,
						'x', 0,
						'y', 0,
						'size', _buildingInfo[buildingType].size,
						'texture', _buildingInfo[buildingType].img,
						'color', 'green')
				end

				local wdgBuilding = containerWdg:GetChildWidget(widgetName)
				if wdgBuilding ~= nil then
					local posArray = split(buildingArray[1], ",")
					local posX = tostring(tonumber(posArray[1]) + _minimapIconAdjustPos) .. '%'
					local posY = tostring(100 - tonumber(posArray[2]) + _minimapIconAdjustPos) .. '%'

					wdgBuilding:SetX(posX)
					wdgBuilding:SetY(posY)
					wdgBuilding:SetVisible(true)

					containerWdg:GetChildWidget(widgetName..'_img'):SetColor(team == 1 and 'green' or 'red')
				end

				buildingSlots[buildingType] = buildingSlots[buildingType] + 1
			end
		end
	end
end

local function UpdateMinimapHeroes()
	local matchStatus = GetSelectedMatchStatus()
	if matchStatus ~= MatchStatus_Binding and matchStatus ~= MatchStatus_OnGoing then
		return
	end

	local containerWdg = GetWidget('watch_system_minimap_heroes_'.._currentRegion)

	for team=1,2 do
		local teamHeroes = Watch_System.CurrentMatchJson['team'..team]
		local heroes = {}
		if teamHeroes ~= nil then
			for _, teamHero in pairs(teamHeroes) do
				table.insert(heroes, teamHero)
			end
		else
			Echo('^yUpdateMinimapHeroes no team heroes for team: '..team)
		end

		for slot=1, 5 do
			local bShow = slot <= #heroes

			local widgetName = 'watch_system_minimap_hero_'..team..slot.._currentRegion

			--Check if there is a hero widget
			if bShow and not containerWdg:HasChildWidget(widgetName) then
				containerWdg:Instantiate('watch_system_minimap_hero',
					'team', team,
					'slot', slot,
					'category', _currentRegion,
					'x', 0,
					'y', 0,
					'texture', '$invis',
					'color', 'white')
			end

			local wdgHero = containerWdg:GetChildWidget(widgetName)
			if bShow and wdgHero ~= nil then
				local posString = heroes[slot].position or '0:0'
				if posString == '0:0' then posString = '-9999:-9999' end

				local posArray = split(posString, ':')
				local posX = tostring(tonumber(posArray[1]) + _minimapIconAdjustPos) .. '%'
				local posY = tostring(100 - tonumber(posArray[2]) + _minimapIconAdjustPos) .. '%'

				wdgHero:SetX(posX)
				wdgHero:SetY(posY)

				local teamIndex = heroes[slot].team_index ~= nil and tonumber(heroes[slot].team_index)+1 or slot
				wdgHero:GetChildWidget(widgetName..'_0'):SetColor(_ExactPlayerColors[teamIndex+(team-1)*5])
				wdgHero:GetChildWidget(widgetName..'_1'):SetColor(team == 1 and 'green' or 'red')
			end
			if wdgHero ~= nil then
				wdgHero:SetVisible(bShow)
			end
		end
	end
end

local function UpdateTeamHeroes()
	for team=1,2 do
		local teamHeroes = Watch_System.CurrentMatchJson['team'..team]
		local heroes = {}
		if teamHeroes ~= nil then
			for _, teamHero in pairs(teamHeroes) do
				table.insert(heroes, teamHero)
			end
		else
			Echo('^yUpdateTeamHeroes no team heroes for team: '..team)
		end

		for slot=1, 5 do
			local bShow = slot <= #heroes
			GetWidget('playerdata_entrypanel_'..team..slot.._currentRegion):SetVisible(bShow)

			if bShow then
				local heroName = heroes[slot].hero_used
				local teamIndex = heroes[slot].team_index ~= nil and tonumber(heroes[slot].team_index)+1 or slot
				GetWidget('playerdata_entryname_'..team..slot.._currentRegion):SetColor(_ExactPlayerColors[teamIndex+(team-1)*5])

				local nameWdg = GetWidget('playerdata_entryname_'..team..slot.._currentRegion)
				-- heroes[slot].nickname = '[WWWWW]WWWWWWWWWWWWW'
				nameWdg:SetFont(GetFontSizeForWidth(heroes[slot].nickname, nameWdg:GetWidth(), 10, 6))
				nameWdg:SetText(heroes[slot].nickname)

				GetWidget('playerdata_entryhero_'..team..slot.._currentRegion):SetText(#heroName > 0 and GetHeroDisplayNameFromDB(heroName) or '')
				GetWidget('playerdata_entryimage_'..team..slot.._currentRegion):SetTexture(#heroName > 0 and GetHeroIconFromDB(heroName) or '/ui/common/primary_none.tga')
				GetWidget('playerdata_entrykda_'..team..slot.._currentRegion):SetText('^g'..heroes[slot].kill..'^* / ^r'..heroes[slot].death..'^* / ^y'..heroes[slot].assist..'^*')
			end
		end
	end
end

local function UpdateBattleLog()
	local logBuffer = GetWidget('watch_system_battlelog_chatbuffer_' .. _currentRegion)
	if logBuffer ~= nil then
		if Watch_System.CurrentMatchJson.chat_log ~= nil then
			for _, message in pairs(Watch_System.CurrentMatchJson.chat_log) do
				logBuffer:AddBufferText(message)
			end
		else
			Echo('UpdateBattleLog invalid chat log!')
		end
		if Watch_System.CurrentMatchJson.combat_log ~= nil then
			for _, message in pairs(Watch_System.CurrentMatchJson.combat_log) do
				logBuffer:AddBufferText(message)
			end
		else
			Echo('UpdateBattleLog invalid combat log!')
		end
	end
end

local function UpdateMatchInfoPanel(matchJSON)
	if matchJSON == nil then
		Echo('UpdateMatchInfoPanel invalid match data!!!')
		return
	end

	if Watch_System.CurrentMatchJson ~= nil and Watch_System.CurrentMatchJson.match_time == matchJSON.match_time and tonumber(matchJSON.match_time) > 0 then
		Echo('UpdateMatchInfoPanel same match time: '..matchJSON.match_time)
		return
	end

	Watch_System.CurrentMatchJson = matchJSON

	if _selectedMatchData.status == 2 or _selectedMatchData.status == 3 then
		GetWidget('watch_system_team_kills_label_'.._currentRegion..'1'):SetText(matchJSON.hero_kill1 or '0')
		GetWidget('watch_system_team_kills_label_'.._currentRegion..'2'):SetText(matchJSON.hero_kill2 or '0')
	end

	if _selectedMatchData.status == 2 then
		local label = ''
		local phase = tonumber(matchJSON.game_phase)
		if phase == 3 then
			label = Translate('game_lobby_phase_picking')
		elseif phase == 4 then
			label = Translate('game_lobby_phase_loading')
		elseif phase == 5 then
			label = Translate('game_lobby_phase_prematch')
		elseif phase == 6 then
			label = Translate('watch_system_minimap_elapsed_time', 'time', FormatTime(tonumber(matchJSON.match_time)*1000))
		end
		GetWidget('watch_system_elapsed_time_'.._currentRegion):SetVisible(NotEmpty(label))
		GetWidget('watch_system_elapsed_time_label_'.._currentRegion):SetText(label)
	else
		GetWidget('watch_system_elapsed_time_'.._currentRegion):SetVisible(false)
	end

	local showWinTeam = false
	local winTeamIcon
	if not _selectedMatchData.is_recommended and matchJSON.finish then
		showWinTeam = true
		if matchJSON.win_team ~= nil and tonumber(matchJSON.win_team) > 0 then
			winTeamIcon = _selectedMatchData['team'..matchJSON.win_team..'Icon']
		else
			Echo('^rUpdateMatchInfoPanel invalid winning team with: '..tostring(matchJSON.win_team))
		end
	end

	GetWidget('watch_system_minimap_win_team_'.._currentRegion):SetVisible(showWinTeam)
	if showWinTeam and winTeamIcon ~= nil then
		setTextureCheckForWeb(GetWidget('watch_system_minimap_win_team_icon_'.._currentRegion), winTeamIcon)
	end

	UpdateMinimapBuildings()
	UpdateMinimapHeroes()
	UpdateTeamHeroes()
	UpdateBattleLog()
end

local function MatchUpdate(identifer, _, matchinfo)
	-- EchoDebug('MatchUpdate matchinfo: ' .. tostring(matchinfo))

	if identifer ~= nil and identifer ~= MatchInfoResultIdentifier then
		return
	end

	if matchinfo == nil or #matchinfo == 0 then
		if _selectedMatchData.status == MatchStatus_OnGoing then
			matchinfo = _defaultMatchInfo
		else
			Echo('MatchUpdate invalid match info: ' .. tostring(matchinfo) .. ' with match status: '.._selectedMatchData.status)
			return
		end
	end

	local matchJSON = lib_json.decode(matchinfo)
	if matchJSON.error == 'finished' then
		return
	end
	EchoDebug('MatchUpdate matchJSON.data is nil: ' .. tostring(matchJSON.data == nil))
	UpdateMatchInfoPanel(matchJSON.data)
end

local function GetTeamInfo(matchData, team)
	local clanTeam = (matchData.f_match_clan_0_team == nil or matchData.f_match_clan_0_team == 'legion') and 1 or 2
	local clan = clanTeam == team and matchData.clan0 or matchData.clan1
	if clan ~= nil then
		if clan.f_clan_avatar == nil or clan.f_clan_name == nil or clan.f_clan_short_name == nil then
			Echo('^rGetTeamInfo invalid clan info with id: '..tostring(clan.f_id)..' avatar: '..tostring(clan.f_clan_avatar)..' name: '..tostring(clan.f_clan_short_name))
		end
		return {icon=clan.f_clan_avatar ~= nil and clan.f_clan_avatar or '$black',
				name=clan.f_clan_short_name ~= nil and clan.f_clan_short_name or '',
				fullname=clan.f_clan_name ~= nil and clan.f_clan_name or ''}
	else
		return {icon='$black', name='', fullname=''}
	end
end

local function GetStatusInfo(matchStatus)
	local icon, color, status = '', 'black', tonumber(matchStatus)

	if status == MatchStatus_NotStarted or status == MatchStatus_Binding then
		icon = 'coming'
		color = '#ac840a'
	elseif status == MatchStatus_OnGoing then
		icon = 'ongoing'
		color = '#187232'
	elseif status == MatchStatus_Finished then
		icon = 'finished'
		color = '#ab5510'
	elseif status == MatchStatus_Cancelled then
		icon = 'cancelled'
		color = '#505050'
	end

	local lang = GetCvarString('host_language')
	if lang ~= 'en' and lang ~= 'th' then
		lang = 'en'
	end

	return {icon=icon..'_'..lang, color=color, label=Translate('watch_system_match_status_'..icon)}
end

local function GetStarPlayers(matchData)
	local players = {}
	if matchData ~= nil then
		table.sort(matchData.account_list, function(a, b) return a.mmr > b.mmr end)

		for _, account in ipairs(matchData.account_list) do
			if account.is_star then
				table.insert(players, account.nickname)
			end
		end
	end
	return players
end

local function GetRankLevelByMMR(mmr)
	local config = GetCvarString('cc_mmrConfigAfterS6')
	local mmrArray = explode(',', config)
	local i = 1
	while mmrArray[i] do
		if mmr < mmrArray[i] then return i-1 end
		i = i + 1
	end

	return 20
end

local function GetMatchNames(matchData)
	local starPlayers = GetStarPlayers(matchData)
	local names = {name='', team_name=''}
	if matchData ~= nil then
		local ruleType = tonumber(matchData.rule_type)
		if ruleType == 1 then
			if GetCvarBool('cl_GarenaEnable') then
				names.name = Translate('watch_system_mmr_match_name', 'mmr', matchData.avg_mmr)
				names.team_name = Translate('watch_system_match_caldavar_sea_normal')
			else
				names.name = Translate('watch_system_nommr_match_name')
				names.team_name = Translate('watch_system_match_caldavar_normal')
			end
		elseif ruleType == 2 then
			if GetCvarBool('cl_GarenaEnable') then
				names.name = Translate('watch_system_mmr_match_name', 'mmr', matchData.avg_mmr)
				names.team_name = Translate('watch_system_match_caldavar_sea_casual')
			else
				names.name = Translate('watch_system_nommr_match_name', 'mmr', matchData.avg_mmr)
				names.team_name = Translate('watch_system_match_caldavar_casual')
			end
		elseif ruleType == 3 then
			names.name = Translate('watch_system_midwars')
			names.team_name = Translate('watch_system_match_midwars')
		elseif ruleType == 4 then
			names.team_name = Translate('watch_system_match_starplayer', 'player', #starPlayers > 0 and starPlayers[1] or '')
			names.name = Translate('watch_system_starplayer')
		elseif ruleType == 5 then
			local level = GetRankLevelByMMR(matchData.avg_mmr)
			names.name = Translate('watch_system_campaign_match_name', 'rank', Translate('player_compaign_level_name_S7_'..level))
			names.team_name = Translate('watch_system_match_campaign_normal')
		elseif ruleType == 6 then
			local level = GetRankLevelByMMR(matchData.avg_mmr)
			names.name = Translate('watch_system_campaign_match_name', 'rank', Translate('player_compaign_level_name_S7_'..level))
			names.team_name = Translate('watch_system_match_campaign_casual')
		end
	end
	return names
end

local function GenerateRegionMatchlist(matchesTable, bRegenerate)
	local listBox = GetWidget('watch_system_' .. _currentRegion .. '_listbox')
	listBox:ClearItems()

	_selectedMatchList = {}

	if matchesTable ~= nil then
		local unstartedlist, ongoinglist, finishedlist = {}, {}, {}
		for _, match in ipairs(matchesTable) do
			if match.status == MatchStatus_Binding or match.status == MatchStatus_NotStarted then
				table.insert(unstartedlist, match)
			elseif match.status == MatchStatus_OnGoing then
				table.insert(ongoinglist, match)
			else
				table.insert(finishedlist, match)
			end
		end

		for _, match in ipairs(ongoinglist) do
			table.insert(_selectedMatchList, match)
		end
		for _, match in ipairs(unstartedlist) do
			table.insert(_selectedMatchList, match)
		end
		for _, match in ipairs(finishedlist) do
			table.insert(_selectedMatchList, match)
		end

		local idx = 0
		for _, match in ipairs(_selectedMatchList) do
			match.index = idx

			local statusInfo = GetStatusInfo(match.status)
			listBox:AddTemplateListItem(
				'match_item', match.matchID,
				'id', match.index,
				'category', _currentRegion,
				'itemName', match.matchName,
				'team1Name', match.team1Name,
				'team2Name', match.team2Name,
				'team1Icon', match.team1Icon,
				'team2Icon', match.team2Icon,
				'tour_icon', match.tournamentIcon,
				'tour_link', match.tournamentLink,
				'enabled', 0,
				'matchTime', match.matchTime,
				'status_id', match.id,
				'matchID', match.matchID,
				'status_img', statusInfo.icon
			)
			idx = idx + 1
		end

		if #_selectedMatchList > 0 then
			local listIndex = 0
			if (bRegenerate or _isRegenerateListFromServer) and _selectedMatchData ~= nil then
				_isRegenerateListFromServer = false
				for _, selectedMatch in ipairs(_selectedMatchList) do
					if tonumber(_selectedMatchData.id) == tonumber(selectedMatch.id) then
						listIndex = selectedMatch.index
						break
					end
				end
			end
			EchoDebug('GenerateRegionMatchlist listIndex: ' .. tostring(listIndex))
			listBox:SetSelectedItemByIndex(listIndex)
			if listIndex > 3 then
				listBox:ScrollToListItem(_selectedMatchData.matchID)
			end
		else
			ResetMinimap()
		end
	end
end

local function RefreshSelectedMatchHeader(matchData)
	if matchData == nil then
		Echo('RefreshSelectedMatchHeader no match data!!!')
		return
	end

	if _currentRegion == 'live' then
		GetWidget('watch_system_live_header'):SetVisible(matchData.is_recommended)
		GetWidget('watch_system_region_header_live'):SetVisible(not matchData.is_recommended)

		if matchData.is_recommended then
			GetWidget('watch_system_live_header_title'):SetText(matchData.name)
			GetWidget('watch_system_live_header_mode_icon'):SetTexture('/ui/icons/' .. GetGameModeName(matchData.match_mode) .. '.tga')
			GetWidget('watch_system_live_header_mode_label'):SetText(GetGameModeString(matchData.match_mode))
			GetWidget('watch_system_live_header_casual_label'):SetText(Translate(matchData.is_casual and 'mm_gen_casual' or 'mm_gen_normal'))
			GetWidget('watch_system_live_header_match_id'):SetText(Translate('watch_system_match_id', 'id', matchData.matchID))
		else
			GetWidget('watch_system_region_header_live_team1name'):SetText(matchData.team1FullName)
			GetWidget('watch_system_region_header_live_team2name'):SetText(matchData.team2FullName)
			setTextureCheckForWeb(GetWidget('watch_system_region_header_live_team1icon'), matchData.team1Icon)
			setTextureCheckForWeb(GetWidget('watch_system_region_header_live_team2icon'), matchData.team2Icon)
		end
	else
		GetWidget('watch_system_region_header_'.._currentRegion):SetVisible(true)
		GetWidget('watch_system_region_header_'.._currentRegion..'_team1name'):SetText(matchData.team1FullName)
		GetWidget('watch_system_region_header_'.._currentRegion..'_team2name'):SetText(matchData.team2FullName)
		setTextureCheckForWeb(GetWidget('watch_system_region_header_'.._currentRegion..'_team1icon'), matchData.team1Icon)
		setTextureCheckForWeb(GetWidget('watch_system_region_header_'.._currentRegion..'_team2icon'), matchData.team2Icon)
	end
end

local function RefreshWatchButtonState(matchData)
	local status = tonumber(matchData.status)
	-- EchoDebug('RefreshWatchButtonState status: '..status..' region: '.._currentRegion)
	if status == MatchStatus_NotStarted then
		GetWidget('watch_system_button_status_' .. _currentRegion .. '_notstart'):SetVisible(true)
		GetWidget('watch_system_button_status_' .. _currentRegion .. '_preparing'):SetVisible(false)
		GetWidget('watch_system_button_' .. _currentRegion):SetVisible(false)
		GetWidget('watch_system_button_' .. _currentRegion .. '_replay'):SetVisible(false)
	elseif status == MatchStatus_Binding then
		GetWidget('watch_system_button_status_' .. _currentRegion .. '_notstart'):SetVisible(false)
		GetWidget('watch_system_button_status_' .. _currentRegion .. '_preparing'):SetVisible(true)
		GetWidget('watch_system_button_' .. _currentRegion):SetVisible(false)
		GetWidget('watch_system_button_' .. _currentRegion .. '_replay'):SetVisible(false)
	elseif status == MatchStatus_OnGoing then
		GetWidget('watch_system_button_status_' .. _currentRegion .. '_notstart'):SetVisible(false)
		GetWidget('watch_system_button_status_' .. _currentRegion .. '_preparing'):SetVisible(false)
		GetWidget('watch_system_button_' .. _currentRegion):SetVisible(true)
		GetWidget('watch_system_button_' .. _currentRegion .. '_replay'):SetVisible(false)
	elseif status == MatchStatus_Finished then
		GetWidget('watch_system_button_status_' .. _currentRegion .. '_notstart'):SetVisible(false)
		GetWidget('watch_system_button_status_' .. _currentRegion .. '_preparing'):SetVisible(false)
		GetWidget('watch_system_button_' .. _currentRegion):SetVisible(false)
		GetWidget('watch_system_button_' .. _currentRegion .. '_replay'):SetVisible(true)
	else
		GetWidget('watch_system_button_status_' .. _currentRegion .. '_notstart'):SetVisible(false)
		GetWidget('watch_system_button_status_' .. _currentRegion .. '_preparing'):SetVisible(false)
		GetWidget('watch_system_button_' .. _currentRegion):SetVisible(false)
		GetWidget('watch_system_button_' .. _currentRegion .. '_replay'):SetVisible(false)
	end
end

local function RefreshLiveMatchList(listBox, region, data)
	local result = lib_json.decode(data)
	if (result == nil or not result.success) then
		Echo('Failed to retrieve recommended matches with data: ' .. data)
		return
	end

	_currentRegion = region
	_selectedMatchList = {}

	local tempMatches = {}
	local playerRegion = GetAscensionRegion()
	local isTest = GetCvarBool('releaseStage_test')
	if playerRegion == 'sea' then
		if result.sea_match_list ~= nil then
			for _, datedMatches in pairs(result.sea_match_list) do
				for _, match in ipairs(datedMatches) do
					table.insert(tempMatches, match)
				end
			end
		end
		if isTest then
			if result.naeu_match_list ~= nil then
				for _, datedMatches in pairs(result.naeu_match_list) do
					for _, match in ipairs(datedMatches) do
						table.insert(tempMatches, match)
					end
				end
			end
		end
	elseif playerRegion == 'naeu' then
		if result.naeu_match_list ~= nil then
			for _, datedMatches in pairs(result.naeu_match_list) do
				for _, match in ipairs(datedMatches) do
					table.insert(tempMatches, match)
				end
			end
		end
		if isTest then
			if result.sea_match_list ~= nil then
				for _, datedMatches in pairs(result.sea_match_list) do
					for _, match in ipairs(datedMatches) do
						table.insert(tempMatches, match)
					end
				end
			end
		end
	end

	local matchIdx = 0
	for _, match in ipairs(tempMatches) do
		local team1 = GetTeamInfo(match, 1)
		local team2 = GetTeamInfo(match, 2)
		local tempMatch = {}
		tempMatch.index = matchIdx
		tempMatch.id = match.f_id
		tempMatch.matchID = match.f_match_id
		tempMatch.region = match.f_region
		tempMatch.elapsed_time = Translate('watch_system_list_elapsed_time', 'time', FormatTime(tonumber(match.f_match_elapsed_time)*1000))
		tempMatch.view_num = match.f_view_num
		tempMatch.name = match.f_match_name
		tempMatch.team_name = Translate('watch_system_match_team_name', 'team1', team1.name or '', 'team2', team2.name)
		tempMatch.map = '/maps/caldavar/minimap.tga'
		tempMatch.map_name = Translate('map_caldavar')
		tempMatch.is_tournament = true
		tempMatch.is_starplayer = false
		tempMatch.is_newmap = false
		tempMatch.status = tonumber(match.f_match_status)
		tempMatch.is_recommended = false
		tempMatch.team1Name = team1.name
		tempMatch.team1FullName = team1.fullname
		tempMatch.team1Icon = team1.icon
		tempMatch.team2Name = team2.name
		tempMatch.team2FullName = team2.fullname
		tempMatch.team2Icon = team2.icon
		tempMatch.is_casual = false
		tempMatch.match_mode = 0

		for team=1,2 do
			local heroIndex = 0
			if match['team' .. team] ~= nil then
				for _, hero in pairs(match['team' .. team]) do
					tempMatch['hero' .. team .. heroIndex] = #hero.hero_used > 0 and GetHeroIconFromDB(hero.hero_used) or '/ui/common/primary_none.tga'
					heroIndex = heroIndex + 1
				end
			end
			for i=heroIndex,4 do
				tempMatch['hero' .. team .. i] = '/ui/common/primary_none.tga'
			end
		end

		table.insert(_selectedMatchList, tempMatch)

		matchIdx = matchIdx + 1
	end

	if #result.recommended_pool > 0 then
		for idxMatch=1, #result.recommended_pool do
			local match = result.recommended_pool[idxMatch]
			local matchNames = GetMatchNames(match)
			local tempMatch = {}
			tempMatch.index = matchIdx
			tempMatch.id = 0
			tempMatch.matchID = match.match_id
			tempMatch.region = playerRegion == 'sea' and 1 or 2
			tempMatch.elapsed_time = Translate('watch_system_list_elapsed_time', 'time', FormatTime(tonumber(match.elapsed_secs)*1000))
			tempMatch.view_num = match.f_view_num or '0'
			tempMatch.name = matchNames.name
			tempMatch.team_name = matchNames.team_name
			tempMatch.map = '/maps/' .. match.map .. '/minimap.tga'
			tempMatch.map_name = Translate('map_' .. match.map)
			tempMatch.is_tournament = false
			tempMatch.is_starplayer = tonumber(match.rule_type) == 4
			tempMatch.is_newmap = false
			tempMatch.status = 2
			tempMatch.is_recommended = true
			tempMatch.team1Name = ''
			tempMatch.team1FullName = ''
			tempMatch.team1Icon = '$invis'
			tempMatch.team2Name = ''
			tempMatch.team2FullName = ''
			tempMatch.team2Icon = '$invis'
			tempMatch.is_casual = tonumber(match.casual) == 1
			tempMatch.match_mode = tonumber(match.match_mode)

			for team=1,2 do
				for index=0,4 do
					tempMatch['hero' .. team .. index] = '/ui/common/primary_none.tga'
				end
			end
			local idx1, idx2 = 0, 0
			for _, account in ipairs(match.account_list) do
				local index
				local team = tonumber(account.team)
				if team == 1 then
					index = idx1
					idx1 = idx1 + 1
				elseif team == 2 then
					index = idx2
					idx2 = idx2 + 1
				end
				--EchoDebug('RefreshLiveMatchList index: '..index..' team: '..team..' hero: '..account.hero_name)
				if index ~= nil then
					tempMatch['hero' .. team .. index] = #account.hero_name > 0 and GetHeroIconFromDB(account.hero_name) or '/ui/common/primary_none.tga'
				end
			end

			table.insert(_selectedMatchList, tempMatch)

			matchIdx = matchIdx + 1
		end
	end

	--Generate live match list
	if #_selectedMatchList > 0 then
		Watch_System.InitPageBar('live', math.ceil(table.getn(_selectedMatchList) / live_itemnum_perpage))
	end
	Watch_System.UpdateLiveList()
end

function Watch_System.UpdateLiveList()
	local listBox = GetWidget('watch_system_live_matches_listbox')
	listBox:ClearItems()
	local page = Watch_System.GetPageBarSelected('live')
	local index = page * live_itemnum_perpage

	local lang = GetCvarString('host_language')
	if lang ~= 'en' and lang ~= 'th' then
		lang = 'en'
	end

	for i=1,live_itemnum_perpage do
		local match = _selectedMatchList[index+i]
		if match ~= nil then
			listBox:AddTemplateListItem(
				'live_match_item',
				match.matchID,
				'id', tostring(i-1),
				'elapsed_time', match.elapsed_time,
				'view_num', match.view_num,
				'match_name', match.name,
				'team_name', match.team_name,
				'map', match.map,
				'mapname', match.map_name,
				'is_tournament', tostring(match.is_tournament),
				'tournament', '/ui/elements/watch_system/tournament_'..lang..'.tga',
				'is_starplayer', tostring(match.is_starplayer),
				'starplayer', '/ui/elements/watch_system/starplayer_'..lang..'.tga',
				'is_newmap', tostring(match.is_newmap),
				'newmap', '/ui/elements/watch_system/newmap_'..lang..'.tga',
				'hero10', match.hero10,
				'hero11', match.hero11,
				'hero12', match.hero12,
				'hero13', match.hero13,
				'hero14', match.hero14,
				'hero20', match.hero20,
				'hero21', match.hero21,
				'hero22', match.hero22,
				'hero23', match.hero23,
				'hero24', match.hero24
		)
		end
	end

	if listBox:GetNumListItems() > 0 then
		listBox:SetSelectedItemByIndex(0)
	else
		ResetMinimap()
	end
end

local function RefreshRegionMatchList(listBox, region, data)
	local jsonData = lib_json.decode(data)
	local matches = jsonData.match_list

	listBox:ClearItems()

	_currentRegion = region
	_regionDateMatches = {}

	local dates = {}
	for date, datedMatches in pairs(matches) do
		local sDate = string.sub(date, 6)
		local matches = {}
		for _, match in ipairs(datedMatches) do
			local team1 = GetTeamInfo(match, 1)
			local team2 = GetTeamInfo(match, 2)
			local tempMatch = {}

			tempMatch.index = 0
			tempMatch.id = match.f_id
			tempMatch.matchID = match.f_match_id
			tempMatch.region = match.f_region
			tempMatch.status = tonumber(match.f_match_status)
			tempMatch.stats = match.match_stats
			tempMatch.map = '/maps/caldavar/minimap.tga'
			tempMatch.team1Name = team1.name
			tempMatch.team1FullName = team1.fullname
			tempMatch.team1Icon = team1.icon
			tempMatch.team2Name = team2.name
			tempMatch.team2FullName = team2.fullname
			tempMatch.team2Icon = team2.icon
			tempMatch.is_recommended = false
			tempMatch.elapsed_time = Translate('watch_system_list_elapsed_time', 'time', FormatTime(tonumber(match.f_match_elapsed_time)*1000))
			tempMatch.matchName = match.f_match_name
			tempMatch.tournamentIcon = match.f_tournament_icon or '$black'
			tempMatch.tournamentLink = match.f_tournament_link or ''
			tempMatch.matchTime = match.f_match_presetting_start_time

			table.insert(matches, tempMatch)
		end
		_regionDateMatches[sDate] = matches
		table.insert(dates, sDate)
	end
	table.sort(dates)

	local wdgDates = GetWidget('watch_system_' .. region .. '_matches_dates')
	wdgDates:ClearChildren()
	for _, date in ipairs(dates) do
		wdgDates:Instantiate('watch_system_date_tab', 'category', region, 'date', date)
	end

	Watch_System.SelectRegionMatchDate(string.sub(jsonData.default_date, 6))
end

local function GetMatchInfo(panel, identifier)
	local region = _currentRegion == 'live' and GetAscensionRegion() or _currentRegion
	local matchID = _selectedMatchData.matchID
	local regionID = region == 'sea' and 1 or 2
	local targetURL = '/?r=match/matchstats&hongameclientcookie='.. Client.GetCookie() .. '|' .. GetAscensionRegion() .. '|' .. GetCvarString('host_language') .. '&match_id=' .. matchID .. '&region=' .. regionID
	EchoDebug('Watch_System.GetMatchInfo targetURL: ' .. targetURL)

	SetFormResultTrigger('WatchSystemMatchDataForm', 'MatchInfoResult')
	SetFormTarget('WatchSystemMatchDataForm', targetURL)

	panel:RegisterWatch('MatchInfoResult',
		function(...) MatchUpdate(identifier, ...) end
	)

	SubmitForm('WatchSystemMatchDataForm')
end

local function GetTodayMatchStatus(panel)
	local targetURL = '/?r=season/refreshtodaymatch&hongameclientcookie='.. Client.GetCookie() .. '|' .. GetAscensionRegion() .. '|' .. GetCvarString('host_language')
	-- EchoDebug('GetTodayMatchStatus targetURL: ' .. targetURL)

	SetFormResultTrigger('WatchSystemRefreshTodayMatchForm', 'TodayMatchResult')
	SetFormTarget('WatchSystemRefreshTodayMatchForm', targetURL)

	local function RefreshTodayMatchStatus(_, data)
		-- EchoDebug('RefreshTodayMatchStatus data: '..data)
		local info = lib_json.decode(data)
		if info.error_code ~= 100 or info.data == nil then
			if info.error_code == 801 then
				Watch_System.UnsubscribeTodayMatch()
			end
			return
		end

		--refresh match match list if there is any match remaked
		local bRefreshMatchList = false
		for statusMatchID, statusMatch in pairs(info.data) do
			for _, selectedMatch in ipairs(_selectedMatchList) do
				if tonumber(statusMatchID) == tonumber(selectedMatch.id) then
					if tonumber(statusMatch.match_id) > 0 and tonumber(selectedMatch.matchID) > 0 and tonumber(selectedMatch.matchID) ~= tonumber(statusMatch.match_id) then
						EchoDebug('RefreshTodayMatchStatus selected list id: '..statusMatchID .. ' statusMatch.match_id: '..statusMatch.match_id..' selectedMatch.matchID: '..selectedMatch.matchID)
						bRefreshMatchList = true
						break
					end
				end
			end
			if bRefreshMatchList then break end
		end
		if not bRefreshMatchList and (_currentRegion == 'sea' or _currentRegion == 'naeu') then
			for statusMatchID, statusMatch in pairs(info.data) do
				for _, datedMatches in pairs(_regionDateMatches) do
					for _, datedMatch in pairs(datedMatches) do
						if tonumber(statusMatchID) == tonumber(datedMatch.id) then
							if tonumber(datedMatch.matchID) > 0 and tonumber(statusMatch.match_id) > 0 and tonumber(datedMatch.matchID) ~= tonumber(statusMatch.match_id) then
								EchoDebug('RefreshTodayMatchStatus region matches id: '..statusMatchID .. ' statusMatch.match_id: '..statusMatch.match_id..' datedMatch.matchID: '..datedMatch.matchID)
								bRefreshMatchList = true
								break
							end
						end
					end
					if bRefreshMatchList then break end
				end
				if bRefreshMatchList then break end
			end
		end
		if bRefreshMatchList then
			EchoDebug('GetTodayMatchStatus going to refresh match list bRefreshMatchList: ' .. tostring(bRefreshMatchList))
			if _currentRegion == 'live' then
				Watch_System.SubscribeLiveMatches(GetWidget('watch_system_live_matches_listbox'), _currentRegion)
			else
				Watch_System.SubscribeRegionMatches(GetWidget('watch_system_' .. _currentRegion .. '_listbox'), _currentRegion, false)
			end
			return
		end

		--check status
		local bRegenerateList = false
		for statusMatchID, statusMatch in pairs(info.data) do
			--update selectedMatchList
			for _, selectedMatch in ipairs(_selectedMatchList) do
				if tonumber(statusMatchID) == tonumber(selectedMatch.id) then
					--refresh status flags of match list
					if _currentRegion == 'sea' or _currentRegion == 'naeu' then
						if tonumber(statusMatch.status) >= MatchStatus_OnGoing and tonumber(statusMatch.status) ~= tonumber(selectedMatch.status) then
							if tonumber(statusMatch.status) == MatchStatus_Finished then
								_isRegenerateListFromServer = true
								Watch_System.SubscribeRegionMatches(GetWidget('watch_system_' .. _currentRegion .. '_listbox'), _currentRegion, false)
								return
							else
								bRegenerateList = true
							end
						end
					end

					if tonumber(selectedMatch.matchID) ~= tonumber(statusMatch.match_id) then
						-- EchoDebug('^tRefreshTodayMatchStatus selectedMatches matchID: '..selectedMatch.matchID..' new matchID: '..statusMatch.match_id..' status: '..selectedMatch.status..' new status:'..statusMatch.status.. ' id: '..statusMatchID)
						selectedMatch.matchID = tonumber(statusMatch.match_id)
					end

					local selectedMatchStatus = tonumber(_selectedMatchData.status)
					selectedMatch.status = tonumber(statusMatch.status)

					-- EchoDebug('Selected ID:'.._selectedMatchData.id..', current id:'..selectedMatch.id)
					if tonumber(_selectedMatchData.id) == tonumber(selectedMatch.id) then
						-- EchoDebug('^tRefreshTodayMatchStatus found _selectedMatchData.matchID: '.._selectedMatchData.matchID..' selectedMatch.matchID: '..selectedMatch.matchID..' _selectedMatchData.status: '..selectedMatchStatus..' selectedMatch.status: '..selectedMatch.status.. ' id: '..statusMatchID)
						RefreshWatchButtonState(_selectedMatchData)

						if selectedMatchStatus == MatchStatus_Binding and tonumber(selectedMatch.status) == MatchStatus_OnGoing then
							Watch_System.GetMatchBetInfo(_selectedMatchData, false)
							Watch_System.SubscribeMatchInfo()
						end
					end
					break
				end
			end

			--update datedMatches
			for _, datedMatches in pairs(_regionDateMatches) do
				local found = false
				for _, datedMatch in pairs(datedMatches) do
					if tonumber(statusMatchID) == tonumber(datedMatch.id) then
						if tonumber(datedMatch.matchID) ~= tonumber(statusMatch.match_id) then
							-- EchoDebug('^tRefreshTodayMatchStatus datedMatches datedMatch.matchID: '..datedMatch.matchID..' new matchID: '..statusMatch.match_id..' status: '..datedMatch.status..' new status:'..statusMatch.status)
							datedMatch.matchID = tonumber(statusMatch.match_id)
						end
						datedMatch.status = tonumber(statusMatch.status)
						found = true
						break
					end
				end
				if found then
					break
				end
			end
		end

		if bRegenerateList then
			-- EchoDebug('RefreshTodayMatchStatus _selectedMatchDate: '.._selectedMatchDate)
			GenerateRegionMatchlist(_regionDateMatches[_selectedMatchDate], true)
		end
	end
	panel:RegisterWatch('TodayMatchResult', RefreshTodayMatchStatus)

	SubmitForm('WatchSystemRefreshTodayMatchForm')
end
--bet mode: 1:胜负 2:首塔 3:首杀 4:十杀
--bet status: 0:无效 1:有效 2:完结 3:取消 4:锁定 5:发奖?
local function RefreshMatchBetInfo(_, data)
	-- EchoDebug('RefreshMatchBetInfo data: '..tostring(data))
	local widgets = {GetWidget('pick_winner_'.._currentRegion),GetWidget('tower_crush_'.._currentRegion),GetWidget('first_blood_'.._currentRegion),GetWidget('ten_kills_'.._currentRegion)}
	local info = data ~= nil and lib_json.decode(data) or nil
	if info == nil or info.error_code ~= 100 or #info.data == 0 then
		Echo('RefreshMatchBetInfo invalid bet data: '..tostring(data))
		for _, w in ipairs(widgets) do
			w:SetButtonState(0)
			w:SetEnabled(false)
		end
		return
	end

	for _, v in pairs(info.data) do
		local mode = tonumber(v.info.f_betting_mode)
		if mode == 1 then
			_betInfoWinner = v
		elseif mode == 2 then
			_betInfoFirstTower = v
		elseif mode == 3 then
			_betInfoFirstBlood = v
		elseif mode == 4 then
			_betInfoTenKills = v
		end
	end

	local bets = {_betInfoWinner, _betInfoFirstTower, _betInfoFirstBlood, _betInfoTenKills}
	local betTipTitles = {'watch_system_bet_tip_title1', 'watch_system_bet_tip_title2', 'watch_system_bet_tip_title3', 'watch_system_bet_tip_title4'}

	local function ShowBetPopup(betInfo)
		if betInfo == nil then return end

		GetWidget('watch_system_bet_btn'):SetEnabled(false)
		GetWidget('watch_system_bet_dialog'):SetVisible(true)
		GetWidget('watch_system_bet_dialog_title'):SetText(betInfo.info.f_betting_title)

		local coins = split(betInfo.info.f_betting_coin_list, ',')
		for team=1,2 do
			local wdgContainer = GetWidget('watch_system_bet_team_container_'..team)
			wdgContainer:ClearChildren()
			for i=1,#coins do
				local coin = tonumber(coins[i])
				wdgContainer:Instantiate('watch_system_radiobutton',
					'radiobutton_name', 'watch_system_bet_50_'..team..i,
					'label', coin,
					'cvar', 'watch_system_bet_coin',
					'value', coin,
					'cvar2', 'watch_system_bet_item_id',
					'value2', tonumber(betInfo.item[team].f_item_id),
					'cvar3', 'watch_system_bet_id',
					'value3', tonumber(betInfo.info.f_betting_id),
					'ratio', betInfo.item[team].f_item_rate,
					'group', 'watch_system_bet')
			end
			GetWidget('watch_system_bet_team_odds_'..team):SetText('1:'..string.format('%.3f', tonumber(betInfo.item[team].f_item_rate)/1000))
			setTextureCheckForWeb(GetWidget('watch_system_bet_team_icon_'..team), betInfo.item[team].clan.f_clan_avatar)
		end
	end

	local function GetBetTip(status, betInfo, myBet, myTeam)
		local tip
		if status == 1 then -- valid
			if myBet ~= nil then
				tip = 'watch_system_bet_tip_already_bet'
			end
		elseif status == 2 then -- finished
			tip = myBet == nil and 'watch_system_bet_tip_bet_result_nobet' or (tonumber(myBet.f_reward_coin_num) > 0 and 'watch_system_bet_tip_bet_result_v' or 'watch_system_bet_tip_bet_result_f')
		elseif status == 3 then -- cancelled
			tip = 'watch_system_bet_tip_bet_canceled'
		elseif status == 4 then -- locked
			tip = myBet == nil and 'watch_system_bet_tip_bet_lock_nobet' or 'watch_system_bet_tip_already_bet'
		elseif status == 5 then -- rewarding
			tip = myBet == nil and 'watch_system_bet_tip_bet_rewarding_nobet' or 'watch_system_bet_tip_bet_rewarding'
		end

		if tip == nil then
			return ''
		else
			if myBet == nil then
				return Translate(tip)
			else
				return Translate(tip,
					'coin', myBet.f_coin_num,
					'team', betInfo.item[myTeam].clan.f_clan_short_name,
					'ratio', '1:'..string.format('%.3f', tonumber(betInfo.item[myTeam].f_item_rate)/1000),
					'reward', myBet.f_reward_coin_num
				)
			end
		end
	end

	local function ShowBetTip(status, betInfo, myBet, myTeam, title)
		GetWidget('watch_system_bet_floater'):SetVisible(true)
		GetWidget('watch_system_bet_floater_title'):SetText(Translate(title))
		GetWidget('watch_system_bet_floater_label'):SetText(GetBetTip(status, betInfo, myBet, myTeam))
	end

	local function HideBetTip()
		GetWidget('watch_system_bet_floater'):SetVisible(false)
	end

	for i=1, #bets do
		if bets[i] == nil or bets[i].info.f_is_locked == 1 then
			widgets[i]:SetEnabled(false)
			widgets[i]:SetButtonState(0)
		else
			widgets[i]:SetEnabled(true)
			widgets[i]:SetButtonState(0)
			widgets[i]:ClearCallback('onmouseover')
			widgets[i]:ClearCallback('onmouseout')

			local myBet, myTeam
			for team=1,2 do
				myBet = bets[i].item[team].my
				if myBet ~= nil then
					myTeam = team
					break
				end
			end
			local status = tonumber(bets[i].info.f_status)
			if status == 0 then --invalid or cancelled
				widgets[i]:SetEnabled(false)
			elseif status == 1 and myBet == nil then --valid
				widgets[i]:SetButtonState(0)
				widgets[i]:SetCallback('onclick', function() self:SetButtonState(0) ShowBetPopup(bets[i]) end)
			elseif (status == 1 and myBet ~= nil) or status == 2 or status == 3 or status == 4 or status == 5 then --finished or cancelled or locked or rewarding
				widgets[i]:SetButtonState(1)
				widgets[i]:SetCallback('onclick', function() self:SetButtonState(1) end)
				widgets[i]:SetCallback('onmouseover', function() ShowBetTip(status, bets[i], myBet, myTeam, betTipTitles[i]) end)
				widgets[i]:SetCallback('onmouseout', function() HideBetTip() end)
			else
				Echo('Unknown bet status with: '..status)
			end
		end
	end
end

local function RefreshMatchBetResult(_, data)
	EchoDebug('RefreshMatchBetResult data: '..data)
	local info = lib_json.decode(data)
	if info ~= nil then
		ShowWarning(info.error_msg)
	end
	if info.error_code == 100 then
		local function DoUpdate()
			Watch_System.GetMatchBetInfo(_selectedMatchData)
		end
		GetWidget('watch_system_refresh_match_bet_info_helper'):Sleep(100, DoUpdate)
		interface:UICmd("ChatRefreshUpgrades(); ClientRefreshUpgrades();")
	end
end

function Watch_System.Reset()
	_regionDateMatches = {}
	_selectedMatchList = {}
	Watch_System.UnsubscribeTodayMatch()
	Watch_System.UnsubscribeMatchInfo()
	Watch_System.CurrentMatchJson = {}
end

function Watch_System.TourClicked(link)
	EchoDebug('Watch_System.TourClicked link: '..tostring(link))
end

function Watch_System.GetSelectedMatch()
	return _selectedMatchData
end

function Watch_System.RefreshBetDialog(coin, ratio)
	local str = Translate('watch_system_bet_dialog_desc_2', 'coin', math.floor(tonumber(coin)*tonumber(ratio)/1000))
	GetWidget('watch_system_bet_desc2'):SetText(str)
	GetWidget('watch_system_bet_btn'):SetEnabled(true)
end

function Watch_System.GetMatchBetInfo(matchData, showStatus)
	showStatus = showStatus == nil and true or showStatus

	local targetURL = '/?r=betting/index&hongameclientcookie='.. Client.GetCookie() .. '|' .. GetAscensionRegion() .. '|' .. GetCvarString('host_language') .. '&id=' .. matchData.id
	EchoDebug('Watch_System.GetMatchBetInfo targetURL: ' .. targetURL .. ' showStatus: '..tostring(showStatus))

	SetFormResultTrigger('WatchSystemMatchBetForm', 'MatchBetInfoResult')
	SetFormStatusTrigger('WatchSystemMatchBetForm', showStatus and 'MatchBetInfoStatus' or '')
	SetFormTarget('WatchSystemMatchBetForm', targetURL)

	SubmitForm('WatchSystemMatchBetForm')
end

function Watch_System.DoBet()
	local targetURL = '/?r=betting/add&hongameclientcookie='.. Client.GetCookie() .. '|' .. GetAscensionRegion() .. '|' .. GetCvarString('host_language') .. '&betting_id=' .. GetCvarInt('watch_system_bet_id') .. '&item_id=' .. GetCvarInt('watch_system_bet_item_id') .. '&coin_num=' .. GetCvarInt('watch_system_bet_coin')
	EchoDebug('Watch_System.DoBet targetURL: ' .. targetURL)

	SetFormResultTrigger('WatchSystemMatchBetForm', 'MatchBetResult')
	SetFormStatusTrigger('WatchSystemMatchBetForm', 'WatchSystemRequestStatus')
	SetFormTarget('WatchSystemMatchBetForm', targetURL)

	SubmitForm('WatchSystemMatchBetForm')
end

function Watch_System.SelectRegionMatchDate(date)
	_selectedMatchDate = date

	for k, _ in pairs(_regionDateMatches) do
		GetWidget('watch_system_date_tab_'.. _currentRegion .. k .. '_selected'):SetVisible(k == date)
	end

	GenerateRegionMatchlist(_regionDateMatches[date], false)
end

function Watch_System.GetBetHistory()
	local targetURL = '/?r=betting/getuserbettinglist&hongameclientcookie=' .. Client.GetCookie() .. "|" .. GetAscensionRegion() .. '|' .. GetCvarString('host_language')

	EchoDebug('Watch_System.GetBetHistory() url: '..targetURL)
	SetFormStatusTrigger('WatchSystemForm', 'WatchSystemRequestStatus')
	SetFormResultTrigger('WatchSystemForm', 'BetHistoryResult')
	SetFormTarget('WatchSystemForm', targetURL)

	SubmitForm('WatchSystemForm')
end

function Watch_System.UpdateBetHistory()
	for i=1, betHistory_itemnum_perpage do
		GetWidget('watch_system_bethistory_item_'..(i-1)):SetVisible(false)

		local page = Watch_System.GetPageBarSelected('bethistory')
		local index = page * betHistory_itemnum_perpage + i

		local itemdata = betHistory_data[index]
		if itemdata ~= nil then
			GetWidget('watch_system_bethistory_item_'..(i-1)):SetVisible(true)

			-- Match Date
			if NotEmpty(itemdata['match_time']) then
				GetWidget('watch_system_bethistory_item_'..(i-1)..'_date'):SetText(itemdata['match_time'])
			else
				GetWidget('watch_system_bethistory_item_'..(i-1)..'_date'):SetText('Unknown')
			end

			-- Match Name
			if NotEmpty(itemdata['match_name']) then
				GetWidget('watch_system_bethistory_item_'..(i-1)..'_name'):SetText(itemdata['match_name'])
			else
				GetWidget('watch_system_bethistory_item_'..(i-1)..'_name'):SetText('Unknown')
			end

			-- Tournament icon
			if NotEmpty(itemdata['tournament_icon']) then
				setTextureCheckForWeb(GetWidget('watch_system_bethistory_item_'..(i-1)..'_tournament_icon'), itemdata['tournament_icon'])
			else
				GetWidget('watch_system_bethistory_item_'..(i-1)..'_tournament_icon'):SetTexture('$invis')
			end

			-- Clan1 Name
			if itemdata['clan1'] ~= nil and NotEmpty(itemdata['clan1']['f_clan_short_name']) then
				GetWidget('watch_system_bethistory_item_'..(i-1)..'_clan1_name'):SetText(itemdata['clan1']['f_clan_short_name'])
			else
				GetWidget('watch_system_bethistory_item_'..(i-1)..'_clan1_name'):SetText('Unkonwn')
			end

			-- Clan1 Texture
			if  itemdata['clan1'] ~= nil and NotEmpty(itemdata['clan1']['f_clan_avatar']) then
				setTextureCheckForWeb(GetWidget('watch_system_bethistory_item_'..(i-1)..'_clan1_icon'), itemdata['clan1']['f_clan_avatar'])
			else
				GetWidget('watch_system_bethistory_item_'..(i-1)..'_clan1_icon'):SetTexture('$invis')
			end

			-- Clan2 Name
			if itemdata['clan2'] ~= nil and NotEmpty(itemdata['clan2']['f_clan_short_name']) then
				GetWidget('watch_system_bethistory_item_'..(i-1)..'_clan2_name'):SetText(itemdata['clan2']['f_clan_short_name'])
			else
				GetWidget('watch_system_bethistory_item_'..(i-1)..'_clan2_name'):SetText('Unkonwn')
			end

			-- Clan2 Texture
			if itemdata['clan1'] ~= nil and NotEmpty(itemdata['clan2']['f_clan_avatar']) then
				setTextureCheckForWeb(GetWidget('watch_system_bethistory_item_'..(i-1)..'_clan2_icon'), itemdata['clan2']['f_clan_avatar'])
			else
				GetWidget('watch_system_bethistory_item_'..(i-1)..'_clan2_icon'):SetTexture('$invis')
			end

			for j=1,4 do
				GetWidget('watch_system_bethistory_betret_'..(i-1)..'_'..j..'_success'):SetVisible(false)
				GetWidget('watch_system_bethistory_betret_'..(i-1)..'_'..j..'_fail'):SetVisible(false)
				GetWidget('watch_system_bethistory_betret_'..(i-1)..'_'..j..'_nobet'):SetVisible(false)
				if itemdata['mode'..j] == nil then
					GetWidget('watch_system_bethistory_betret_'..(i-1)..'_'..j..'_nobet'):SetVisible(true)
				elseif itemdata['mode'..j] == 0 then
					GetWidget('watch_system_bethistory_betret_'..(i-1)..'_'..j..'_fail'):SetVisible(true)
				else
					GetWidget('watch_system_bethistory_betret_'..(i-1)..'_'..j..'_success'):SetVisible(true)
					GetWidget('watch_system_bethistory_betret_'..(i-1)..'_'..j..'_success_wincoin'):SetText(tostring(itemdata['mode'..j]))
				end
			end
		end
	end
end

function Watch_System.SubscribeTodayMatch()
	local wdgHelper = GetWidget('watch_system_refresh_today_match_helper')
	local function DoUpdate()
		if wdgHelper:IsVisible() then
			GetTodayMatchStatus(wdgHelper)
			wdgHelper:Sleep(30000, DoUpdate)
		end
	end
	wdgHelper:Sleep(5000, DoUpdate)
end

function Watch_System.UnsubscribeTodayMatch()
	GetWidget('watch_system_refresh_today_match_helper'):Wakeup()
end

function Watch_System.SubscribeMatchInfo()
	local wdgHelper = GetWidget('watch_system_match_info_helper')
	local function DoUpdate()
		GetMatchInfo(wdgHelper, MatchInfoResultIdentifier)
		wdgHelper:Sleep(10000, DoUpdate)
	end
	DoUpdate()
end

function Watch_System.UnsubscribeMatchInfo()
	GetWidget('watch_system_match_info_helper'):UnregisterWatch('MatchInfoResult')
	GetWidget('watch_system_match_info_helper'):Wakeup()
end

function Watch_System.SelectMatch(listBox)
	local prefix = _currentRegion == 'live' and 'live_match_item_' or 'match_item_' .. _currentRegion

	local selectedIndex = listBox:GetSelectedItemIndex()
	local dataIndex = selectedIndex
	if _currentRegion == 'live' then
		local page = Watch_System.GetPageBarSelected('live')
		dataIndex = dataIndex + page * live_itemnum_perpage
	end
	EchoDebug('Watch_System.SelectMatch selectedIndex: ' .. selectedIndex)

	for i=0,listBox:GetNumListItems()-1 do
		if i ~= selectedIndex then
			GetWidget(prefix .. i .. '_glow'):SetVisible(false)
		end
	end
	GetWidget(prefix .. selectedIndex .. '_glow'):SetVisible(true)

	ResetMinimap()

	Watch_System.CurrentMatchJson = {}

	_selectedMatchData = _selectedMatchList[dataIndex+1]

	MatchInfoResultIdentifier = MatchInfoResultIdentifier + 1

	if _selectedMatchData ~= nil then
		EchoDebug('Watch_System.SelectMatch matchID: ' .. _selectedMatchData.matchID .. ' status: ' .. _selectedMatchData.status..' type: '..type(_selectedMatchData.status))

		local minimapIcon = GetWidget('watch_system_minimap_icon_' .. _currentRegion)
		if minimapIcon ~= nil then
			minimapIcon:SetTexture(_selectedMatchData.map)
		end

		RefreshSelectedMatchHeader(_selectedMatchData)
		RefreshWatchButtonState(_selectedMatchData)

		if _currentRegion ~= 'live' then
			local widgets = {GetWidget('pick_winner_'.._currentRegion),GetWidget('tower_crush_'.._currentRegion),GetWidget('first_blood_'.._currentRegion),GetWidget('ten_kills_'.._currentRegion)}
			for _, w in ipairs(widgets) do
				w:SetButtonState(0)
				w:SetEnabled(false)
			end
			Watch_System.GetMatchBetInfo(_selectedMatchData)
		end

		local status = tonumber(_selectedMatchData.status)
		if status == MatchStatus_OnGoing then
			Watch_System.SubscribeMatchInfo()
		elseif status == MatchStatus_Finished then
			if _selectedMatchData.stats ~= nil then
				_selectedMatchData.stats.finish = true
			end
			EchoDebug('Watch_System.SelectMatch _selectedMatchData.stats is nil: ' .. tostring(_selectedMatchData.stats == nil))
			UpdateMatchInfoPanel(_selectedMatchData.stats)
		else
			MatchUpdate(nil, _, _defaultMatchInfo)
		end
	end
end

function Watch_System.StartWatch()
	if _selectedMatchData == nil then
		Echo('Watch_System.StartWatch no match data!!!')
		return
	end

	local targetURL = '?r=api/MasterServer/SpectateMatch&hongameclientcookie='.. Client.GetCookie() .. '|' .. GetAscensionRegion() .. '|' .. GetCvarString('host_language') .. '&match_id=' .. _selectedMatchData.matchID .. '&region=' .. _selectedMatchData.region .. '&is_recommended=' .. (_selectedMatchData.is_recommended and 1 or 0)
	EchoDebug('Watch_System.StartWatch targetURL: ' .. targetURL)

	SetFormStatusTrigger('WatchSystemSpectateMatchForm', 'WatchSystemRequestStatus')
	SetFormResultTrigger('WatchSystemSpectateMatchForm', 'StartWatchResult')
	SetFormTarget('WatchSystemSpectateMatchForm', targetURL)

	--0:success 1:notstart 2:finished 3:invalidmatch 4:indelay 5:unwatchable
	local function StartWatching(_, data)
		EchoDebug('StartWatching data: '..data)
		local info = lib_json.decode(data)
		if info.result == 0 or info.result == 2 then
			StartWatchingMatch(info.spectate_dir, info.spectate_time, info.spectate_dir_second, info.match_version)
		elseif info.result == 1 then
			ShowWarning('watch_system_match_notstart')
		elseif info.result == 3 then
			ShowWarning('watch_system_match_invalid')
		elseif info.result == 4 then
			ShowWarning('watch_system_match_indelay')
		elseif info.result == 5 then
			ShowWarning('watch_system_match_unwatchable')
		end
	end
	interface:RegisterWatch('StartWatchResult', StartWatching)

	SubmitForm('WatchSystemSpectateMatchForm')
end

function Watch_System.SubscribeLiveMatches(listBox, region)
	local targetURL = '?r=api/match/getrecommendedmatches&hongameclientcookie=' .. Client.GetCookie() .. "|" .. GetAscensionRegion() .. '|' .. "en"
	EchoDebug('Watch_System.SubscribeLiveMatches targetURL: ' .. targetURL)

	SetFormStatusTrigger('WatchSystemForm', 'WatchSystemRequestStatus')
	SetFormResultTrigger('WatchSystemForm', 'LiveMatchesResult')
	SetFormTarget('WatchSystemForm', targetURL)

	if listBox ~= nil then
		local function RefreshLiveMatches(_, data)
			EchoDebug('Watch_System.SubscribeLiveMatches data: ' .. data)
			RefreshLiveMatchList(listBox, region, data)
		end
		listBox:RegisterWatch('LiveMatchesResult', RefreshLiveMatches)
	end

	SubmitForm('WatchSystemForm')
end

function Watch_System.UnsubscribeLiveMatches(listBox)
	if listBox ~= nil then
		listBox:UnregisterWatch('LiveMatchesResult')
	end
end

function Watch_System.SubscribeRegionMatches(listBox, region, showStatus)
	showStatus = showStatus == nil and true or showStatus

	local tempRegion = region == 'sea' and 1 or 2
	local targetURL = '/?r=season/matchlist&hongameclientcookie='.. Client.GetCookie() .. '|' .. GetAscensionRegion() .. '|' .. GetCvarString('host_language') .. '&ajax=1&region=' .. tempRegion

	EchoDebug('Watch_System.SubscribeRegionMatches region: '.. region .. ' targetURL: ' .. targetURL .. ' showStatus: ' .. tostring(showStatus))

	if not showStatus then
		local cover = GetWidget('web_browser_cover', 'webpanel')
		if cover:IsVisible() then
			EchoDebug('Watch_System.SubscribeRegionMatches hon cover is displaying, fade out now')
			cover:FadeOut(500)
		end
	end

	SetFormStatusTrigger('WatchSystemForm', 'WatchSystemRequestStatus')
	SetFormStatusTrigger('WatchSystemForm', showStatus and 'WatchSystemRequestStatus' or '')
	SetFormResultTrigger('WatchSystemForm', 'RegionMatchesResult')
	SetFormTarget('WatchSystemForm', targetURL)

	if listBox ~= nil then
		local function RefreshRegionMatches(_, data)
			RefreshRegionMatchList(listBox, region, data)
		end
		listBox:RegisterWatch('RegionMatchesResult', RefreshRegionMatches)
	end

	SubmitForm('WatchSystemForm')
end

function Watch_System.UnsubscribeRegionMatches(listBox)
	if listBox ~= nil then
		listBox:UnregisterWatch('RegionMatchesResult')
	end
end

function Watch_System.ChatStatus(_, isConnected)
	isConnected = AtoB(isConnected)
	if _isConnectionWarnShowing then
		if isConnected then
			GetWidget('watch_system_warning'):SetVisible(false)
			_isConnectionWarnShowing = false
			return
		end
	elseif isConnected then
		return
	end
	GetWidget('watch_system_warning'):SetVisible(not isConnected)
	if GetWidget('watch_system_warning'):IsVisible() then
		_isConnectionWarnShowing = true

		local function CloseWatchSystem()
			GetWidget('watch_system_warning'):SetVisible(false)

			Set('_mainmenu_currentpanel', 'news')
			GetWidget('MainMenuPanelSwitcher'):DoEvent()
		end

		GetWidget('watch_system_warning_label'):SetText(Translate('mstore_error_connect'))
		GetWidget('watch_system_warning_btn_close'):SetCallback('onclick', CloseWatchSystem)
		GetWidget('watch_system_warning_btn_confirm'):SetCallback('onclick', CloseWatchSystem)
	end
end

function Watch_System.InitPageBar(type, total)
	Set('_pagebar_'..type..'_start', 0, 'int')
	Set('_pagebar_'..type..'_total', total, 'int')
	Set('_pagebar_'..type..'_selected', 0, 'int')
	Watch_System.UpdatePageBar(type)
end

function Watch_System.AdjustPageBar(type)
	local start = GetCvarInt('_pagebar_'..type..'_start')
	local total = GetCvarInt('_pagebar_'..type..'_total')
	local selected = GetCvarInt('_pagebar_'..type..'_selected')
	local permax = GetCvarInt('_pagebar_'..type..'_permax')

	local bestselected = math.floor(permax / 2)
	if selected > bestselected then
		local delta = selected - bestselected
		for i=delta,1,-1 do
			if start + permax + i <= total then
				Set('_pagebar_'..type..'_start', start + i, 'int')
				Set('_pagebar_'..type..'_selected', selected - i, 'int')
				break
			end
		end
	elseif selected < bestselected then
		local delta = bestselected - selected
		for i=delta,1,-1 do
			if start - i >= 0 then
				Set('_pagebar_'..type..'_start', start - i, 'int')
				Set('_pagebar_'..type..'_selected', selected + i, 'int')
				break
			end
		end
	end
end

function Watch_System.UpdatePageBar(type)
	Watch_System.AdjustPageBar(type)

	local start = GetCvarInt('_pagebar_'..type..'_start')
	local total = GetCvarInt('_pagebar_'..type..'_total')
	local selected = GetCvarInt('_pagebar_'..type..'_selected')
	local permax = GetCvarInt('_pagebar_'..type..'_permax')

	-- Echo('start:'..tostring(start)..', total:'..tostring(total)..', selected:'..tostring(selected)..', premax:'..tostring(permax))

	for i=0,4 do
		local bNeedShow = (start+i+1) <= total  and (i < permax)
		GetWidget('watch_system_'..type..'_pagebtn_'..i):SetVisible(bNeedShow)
		GetWidget('watch_system_'..type..'_pagebtn_'..i..'_text'):SetText(tostring(start+i+1))

		if i == selected then
			GetWidget('watch_system_'..type..'_pagebtn_'..i..'_btn'):SetEnabled(false)
			GetWidget('watch_system_'..type..'_pagebtn_'..i..'_text'):SetColor('white')
		else
			GetWidget('watch_system_'..type..'_pagebtn_'..i..'_btn'):SetEnabled(true)
			GetWidget('watch_system_'..type..'_pagebtn_'..i..'_text'):SetColor('gray')
		end
	end
end

function Watch_System.GetPageBarSelected(type)
	local start = GetCvarInt('_pagebar_'..type..'_start')
	local selected = GetCvarInt('_pagebar_'..type..'_selected')
	return start + selected
end

local function RefreshRequestStatus(_, statusCode)
	local cover = GetWidget('web_browser_cover', 'webpanel')
	if (cover == nil) then return end

	statusCode = tonumber(statusCode)
	-- 0: idle 1: sending 2: success 3: error
	GetWidget('watch_system_request_status'):SetNoClick(true)
	if (statusCode == 1) then
		cover:SetParent(GetWidget('watch_system_request_status'))
		GetWidget('watch_system_request_status'):SetNoClick(false)
		cover:SetVisible(true)
	elseif (statusCode == 2 or statusCode == 3) then
		cover:FadeOut(500)
		if statusCode == 3 then
			ShowWarning(Translate('mstore_error_connect'))
		end
	end
end

local function RefreshMatchBetInfoStatus(_, statusCode)
	local cover = GetWidget('watch_system_loading_'.._currentRegion)
	if (cover == nil) then return end

	statusCode = tonumber(statusCode)
	-- 0: idle 1: sending 2: success 3: error
	if (statusCode == 1) then
		cover:SetVisible(true)
	elseif (statusCode == 2 or statusCode == 3) then
		cover:FadeOut(300)
		if statusCode == 3 then
			ShowWarning(Translate('mstore_error_connect'))
		end
	end
end

local function RefreshWatchSystemBetHistory(widget, infor)
	EchoDebug('RefreshWatchSystemBetHistory :'..infor)
	local response = lib_json.decode(infor)

	if (response['error_code'] == 100) then
		local tempdata = {}
		for _, value in pairs(response['data']) do
			local match_id = value['base']['f_match_info_id']
			tempdata[match_id] = tempdata[match_id] or {}
			tempdata[match_id]['match_name'] = value['base']['f_match_name']
			tempdata[match_id]['match_time'] = value['base']['f_match_start_time']
			tempdata[match_id]['match_id'] = match_id
			tempdata[match_id]['tournament_icon'] = value['base']['f_tournament_icon']
			tempdata[match_id]['clan1'] = value['clan'][1]
			tempdata[match_id]['clan2'] = value['clan'][2]

			local betting_mode = value['base']['f_betting_mode']
			tempdata[match_id]['mode'..betting_mode] = tonumber(value['base']['f_reward_coin_num'])
		end

		betHistory_data = {}
		for _, value in pairs(tempdata) do
			table.insert(betHistory_data, value)
		end

		table.sort(betHistory_data, function(a, b)
				return tonumber(a['match_id']) > tonumber(b['match_id'])
			end)

		if #betHistory_data > 0 then
			Watch_System.InitPageBar('bethistory', math.ceil(table.getn(betHistory_data) / betHistory_itemnum_perpage))
		end
		Watch_System.UpdateBetHistory()
	end
end

local function OnStreamStarted()
	local panelWidget = GetWidget('watch_system')
	if panelWidget and panelWidget:IsVisible() then
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'watch_system', nil, false)
	end
end

interface:RegisterWatch('StreamSucessfullyStarted', OnStreamStarted)
interface:RegisterWatch('WatchSystemRequestStatus', RefreshRequestStatus)
interface:RegisterWatch('BetHistoryResult', RefreshWatchSystemBetHistory)
interface:RegisterWatch('MatchBetInfoStatus', RefreshMatchBetInfoStatus)
interface:RegisterWatch('MatchBetInfoResult', RefreshMatchBetInfo)
interface:RegisterWatch('MatchBetResult', RefreshMatchBetResult)

---------------------------------------- Replay -------------------------------------------------------------------
interface:RegisterWatch('ReplayList', function(_, ...) Watch_System:OnReplayList(...) end)
interface:RegisterWatch('ReplayInfoGame', function(_, ...) Watch_System:OnReplayInfoGame(...) end)

interface:RegisterWatch('CompatDownloading', function(widget, isDownloading)
							if not AtoB(isDownloading) then
								UpdateReplayInfo()
							end
						end)

interface:RegisterWatch('CompatCalculating', function(widget, isCalculating)
							if not AtoB(isCalculating) then
								UpdateReplayInfo()
							end
						end)

for i=0,9 do
	interface:RegisterWatch('ReplayInfoPlayer'..i, function(_, ...) Watch_System:OnReplayInfoPlayer(i, ...) end)
end

local REPLAY_MAX_COUNT = 12
local _replayList = {}
local _selectedReplay = nil
local _selectedInfo = nil

function Watch_System:UpdateReplayList()
	local scrollWidget = GetWidget('watch_system_replay_scroll')
	local startIndex = tonumber(scrollWidget:GetValue())
	local maxValue = tonumber(scrollWidget:UICmd("GetScrollbarMaxValue()"))
	if startIndex < 1 then startIndex = 1 end
	if startIndex > maxValue then startIndex = maxValue end

	for i=1,REPLAY_MAX_COUNT do
		if NotEmpty(_replayList[i+startIndex-1]) then
			GetWidget('watch_system_replay_item_'..i):SetVisible(true)
			GetWidget('watch_system_replay_item_'..i..'_selected'):SetVisible((i+startIndex-1) == _selectedReplay)

			local file = interface:UICmd('FilenameGetName(\''.._replayList[i+startIndex-1]..'\')')
			GetWidget('watch_system_replay_item_'..i..'_text'):SetText(file)
		else
			GetWidget('watch_system_replay_item_'..i):SetVisible(false)
		end
	end
end

function Watch_System:OnReplayList(file)

	table.insert(_replayList, file)

	local scrollWidget = GetWidget('watch_system_replay_scroll')
	if #_replayList <= REPLAY_MAX_COUNT then
		scrollWidget:SetMaxValue(1)
	else
		local scrollMaxValue = #_replayList - REPLAY_MAX_COUNT + 1
		scrollWidget:SetMaxValue(scrollMaxValue)
	end

	scrollWidget:SetValue(1)
	Watch_System:UpdateReplayList()

	Watch_System:OnClickReplay(1)
end

function Watch_System:OnShowReplay()
	GetWidget('watch_system_replay_info_root'):SetVisible(false)
	GetWidget('watch_system_replay_compatize'):SetVisible(false)
	GetWidget('watch_system_replay_viewstats'):SetEnabled(false)
	GetWidget('watch_system_replay_viewreplay'):SetEnabled(false)
	GetWidget('watch_system_replay_result'):SetVisible(false)
	GetWidget('watch_system_replay_map_icon'):SetTexture('$invis')
	GetWidget('watch_system_replay_map_icon_mask'):SetVisible(false)
	for i=0,9 do
		GetWidget('watch_system_replay_player_'..i):SetVisible(false)
	end

	_replayList = {}
	_selectedReplay = nil
	_selectedInfo = nil
	interface:UICmd('RefreshReplayList();')
end

function Watch_System:OnClickReplay(index)
	local scrollWidget = GetWidget('watch_system_replay_scroll')
	local startIndex = tonumber(scrollWidget:GetValue())
	local maxValue = tonumber(scrollWidget:UICmd("GetScrollbarMaxValue()"))
	if startIndex < 1 then startIndex = 1 end
	if startIndex > maxValue then startIndex = maxValue end

	if _selectedReplay == startIndex + index - 1 then
		return
	end

	_selectedReplay = startIndex + index - 1
	Watch_System:UpdateReplayList()

	SetReplayInfo('')
	UpdateReplayInfo()

	SetReplayInfo(_replayList[_selectedReplay])
	UpdateReplayInfo()
end

function Watch_System:OnReplayInfoGame(...)
	_selectedInfo = arg

	local matchid = arg[1]
	local version = arg[2]
	local date = arg[3]
	local length = string.format('%02d', arg[4])..':'..string.format('%02d', arg[5])..':'..string.format('%02d', arg[6])
	local mapfancyname = arg[7]
	local gamemode = arg[8]
	local gameoption = arg[9]
	local winner = arg[10]
	local compatVersionSupported = AtoB(arg[11])
	local mapname = arg[12]

	if mapname == 'midwars_reborn' then
		mapname = 'midwars'
	end
	
	if mapname ~= 'caldavar' and mapname ~= 'midwars' and mapname ~= 'capturetheflag' and mapname ~= 'devowars' and mapname ~= 'grimmscrossing' and mapname ~= 'soccer' and mapname ~= 'team_deathmatch' and mapname ~= 'caldavar_reborn' then
		mapname = 'other'
	end

	GetWidget('watch_system_replay_info_root'):SetVisible(true)
	GetWidget('watch_system_replay_info_matchid'):SetText(matchid)
	GetWidget('watch_system_replay_info_date'):SetText(date)
	GetWidget('watch_system_replay_info_version'):SetText(version)
	GetWidget('watch_system_replay_info_length'):SetText(length)
	GetWidget('watch_system_replay_info_map'):SetText(mapfancyname)
	GetWidget('watch_system_replay_info_gamemode'):SetText(gamemode)
	GetWidget('watch_system_replay_map_icon'):SetTexture('/ui/fe2/newui/res/replay/'..mapname..'.tga')
	GetWidget('watch_system_replay_map_icon_mask'):SetVisible(true)
	GetWidget('watch_system_replay_map_icon_mask_map'):SetText(mapfancyname)
	Common_V2:SetLongTextLabel('watch_system_replay_info_gameoption', gameoption)

	if string.lower(winner) == 'hellbourne' then
		GetWidget('watch_system_replay_winner'):SetText(Translate('newui_replay_hellbourne'))
		GetWidget('watch_system_replay_winner'):SetColor('#f01010')
	else
		GetWidget('watch_system_replay_winner'):SetText(Translate('newui_replay_legion'))
		GetWidget('watch_system_replay_winner'):SetColor('#1ec1f8')
	end

	GetWidget('watch_system_replay_compatize'):SetVisible(NotEmpty(matchid) and not compatVersionSupported)
	GetWidget('watch_system_replay_viewreplay'):SetVisible(Empty(matchid) or compatVersionSupported)
	GetWidget('watch_system_replay_viewreplay'):SetEnabled(NotEmpty(matchid))
	GetWidget('watch_system_replay_viewstats'):SetEnabled(true)
	GetWidget('watch_system_replay_result'):SetVisible(true)

	GetWidget('watch_system_replay_viewstats'):SetCallback('onclick', function()
		RewardStat_GetMatchInfo(tonumber(matchid))
		
		if GetWidget('watch_system'):IsVisible() then
			Set('_match_stats_parent_panel', 'watch_system')
			Set('_mainmenu_currentpanel', 'match_stats') 
			GetWidget('MainMenuPanelSwitcher'):DoEvent()
		end
		
	end)
end

function Watch_System:OnReplayInfoPlayer(i, ...)
	local name = arg[1]
	local heroname = arg[4]
	local heroicon = arg[5]

	if NotEmpty(name) then
		GetWidget('watch_system_replay_player_'..i):SetVisible(true)
		GetWidget('watch_system_replay_player_'..i..'_heroicon'):SetTexture(heroicon)

		Common_V2:SetLongTextLabel('watch_system_replay_player_'..i..'_heroname', GetHeroDisplayNameFromDB(heroname))
		Common_V2:SetLongTextLabel('watch_system_replay_player_'..i..'_playername', name)

	else
		GetWidget('watch_system_replay_player_'..i):SetVisible(false)
	end
end

function Watch_System:OnClickReplayViewReplay()
	local mapname = _selectedInfo[12]
	if mapname ~= 'caldavar' and mapname ~= 'midwars' and mapname ~= 'capturetheflag' and mapname ~= 'devowars' and mapname ~= 'grimmscrossing' and mapname ~= 'soccer' and mapname ~= 'team_deathmatch' and mapname ~= 'caldavar_reborn' then
		mapname = 'other'
	end
	Loading_V2:SetHostedLoadingInfo(mapname)
	Trigger('ViewReplayDelayed', _replayList[_selectedReplay])
end

function Watch_System:OnClickReplayCompatize()
	if _selectedInfo == nil then return end

	interface:UICmd('DownloadCompat(\''.._selectedInfo[2]..'\');')
end

function Watch_System:OnMatchInfoSummary(_, ...)
--	if NotEmpty(arg[1]) then
--		if arg[1] ~= 'main_stats_retrieving_match' then
--			local widget = GetWidget('watch_system_replay_fetchmatch_fail')
--			widget:FadeIn(250)
--			widget:Sleep(2000, function() widget:FadeOut(250) end)
--		end
--		return
--	end

--	if arg[2] and NotEmpty(arg[2]) and tonumber(arg[2]) > 0 then
--		if GetWidget('watch_system'):IsVisible() then
--			Set('_match_stats_parent_panel', 'watch_system')
--			Set('_mainmenu_currentpanel', 'match_stats') 
--			GetWidget('MainMenuPanelSwitcher'):DoEvent()
--		end
--	end
end