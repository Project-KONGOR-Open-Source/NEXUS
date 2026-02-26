local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort
local interface, interfaceName = object, object:GetName()

MapSelection_V2 = {}

MapSelection_V2.popupTimer = 0
MapSelection_V2.popupShow = true

local MAP_MAXCOUNT = 6
local REGION_MAXCOUNT = 10
local UPDATEGROUPOTPIONDELAYTIME = 500 
local HOVER_SCALE = '106%'
local RANK_PLACEMENT_MAXCOUNT = 6
local GAMEMODE_HEIGHT = 32
local INVALID_CON_TEAMMEMBER_COUNT = 0
local TEAMMEMBER_MAX_COUNT = 5

local MAP_INFO = {
	coop = {
		mapinfo = true
	},
	caldavar = {
		allowGameType = {1, 2},
		mapinfo = true
	},

	midwars = {
		allowGameType = {3},
		mapinfo = true
	},

	con = {
		allowGameType = {6, 7},
		mapinfo = true
	},

	grimmscrossing = {
		allowGameType = {2},
		mapinfo = true
	},

	riftwars = {
		allowGameType = {4},
		mapinfo = true
	},

	team_deathmatch = {
		allowGameType = {5},
		mapinfo = true
	},

	thegrimmhunt = {
		allowGameType = {2}
	},

	capturetheflag = {
		allowGameType = {2},
		mapinfo = true
	},

	devowars = {
		allowGameType = {2},
		mapinfo = true
	},

	soccer = {
		allowGameType = {2},
		mapinfo = true
	},

	solomap = {
		allowGameType = {5},
		mapinfo = true
	},

	midwarsbeta = {
		allowGameType = {5},
		mapinfo = true
	},
	caldavar_reborn = {
		allowGameType = {9},
		mapinfo = true
	},
	midwars_reborn = {
		allowGameType = {10},
		mapinfo = false
	},
}

if not GetCvarBool('cl_GarenaEnable') then
	MAP_INFO.con.allowGameType = {6}
	MAP_INFO.caldavar_reborn.allowGameType = {8}
end

local GAMEMODE_INFO = {
	ap = {
		MODE = "normal",
		NAME = "mainlobby_label_normal",
		DESC = "general_mode_all_pick",
		ICON = "normal",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	bb = {
		MODE = "blindban",
		NAME = "mainlobby_label_blind_ban",
		DESC = "general_mode_blindbanningpick",
		ICON = "blindban",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = true,
		HIDE_DISABLED = true,
	},
	sd = {
		MODE = "singledraft",
		NAME = "mainlobby_label_single_draft",
		DESC = "general_mode_singledraft",
		ICON = "singledraft",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = true,
		HIDE_DISABLED = true,
	},
	bd = {
		MODE = "banningdraft",
		NAME = "mainlobby_label_banning_draft",
		DESC = "general_mode_banningdraft",
		ICON = "banningdraft",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = true,
		HIDE_DISABLED = true,
	},
	bp = {
		MODE = "banningpick",
		NAME = "mainlobby_label_banning_pick",
		DESC = "general_mode_banningpick",
		ICON = "banningpick",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	lp = {
		MODE = "lockpick",
		NAME = "mainlobby_label_lockpick",
		DESC = "general_mode_lockpick",
		ICON = "lockpick",
		BASIC = false,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
		MIN_TEAM_SIZE = 5,
		DEFAULT_OFF = true,
	},
	cm = {
		MODE = "captainsmode",
		NAME = "mainlobby_label_captainsmode",
		DESC = "general_mode_captainsmode",
		ICON = "captainsmode",
		BASIC = false,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
		MIN_TEAM_SIZE = 5,
		DEFAULT_OFF = true,
	},
	km = {
		MODE = "krosmode",
		NAME = "mainlobby_label_krosmode",
		DESC = "general_mode_krosmode",
		ICON = "krosmode",
		BASIC = false,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
		DEFAULT_OFF = false,
	},
	ar = {
		MODE = "allrandom",
		NAME = "mainlobby_label_all_random",
		DESC = "general_mode_allrandom",
		ICON = "forcerandom",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = true,
		HIDE_DISABLED = true,
	},
	br = {
		MODE = "allrandom",
		NAME = "mainlobby_label_balanced_random",
		DESC = "general_mode_balancedrandom",
		ICON = "forcerandom",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = true,
		HIDE_DISABLED = true,
	},
	rd = {
		MODE = "randomdraft",
		NAME = "mainlobby_label_random_draft",
		DESC = "general_mode_random_draft",
		ICON = "randomdraft",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = true,
		HIDE_DISABLED = true,
	},
	gt = {
		MODE = "normal",
		NAME = "mainlobby_label_gated",
		DESC = "general_mode_gated",
		ICON = "gated",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	apg = {
		MODE = "normal",
		NAME = "mainlobby_label_gated",
		DESC = "general_mode_gated",
		ICON = "gated",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	bbg = {
		MODE = "normal",
		NAME = "mainlobby_label_gated",
		DESC = "general_mode_gated",
		ICON = "gated",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	apd = {
		MODE = "normal",
		NAME = "mainlobby_label_dupe_hero",
		DESC = "general_mode_dupe_hero",
		ICON = "dupehero",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	bbr = {
		MODE = "blindban",
		NAME = "mainlobby_label_blind_ban_blitz",
		DESC = "general_mode_blindbanningpickblitz",
		ICON = "blitz",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = true,
		HIDE_DISABLED = true,
	},
	bdr = {
		MODE = "banningdraft",
		NAME = "mainlobby_label_banning_draft_blitz",
		DESC = "general_mode_banningdraftblitz",
		ICON = "blitz",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = true,
		HIDE_DISABLED = true,
	},
	cp = {
		MODE = "counterpick",
		NAME = "mainlobby_label_counter_pick",
		DESC = "newui_mapselection_counter_ban_pick_title",
		ICON = "counterpick",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	fp = {
		MODE = "forcepick",
		NAME = "mainlobby_label_force_pick",
		DESC = "general_mode_force_pick",
		ICON = "forcepick",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	sp = {
		MODE = "soccerpick",
		NAME = "mainlobby_label_soccer_pick",
		DESC = "general_mode_soccer_pick",
		ICON = "soccerpick",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	ss = {
		MODE = "solosame",
		NAME = "mainlobby_label_solo_same",
		DESC = "general_mode_solo_same",
		ICON = "solosame",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	sm = {
		MODE = "solodiff",
		NAME = "mainlobby_label_solo_diff",
		DESC = "general_mode_solo_diff",
		ICON = "solodiff",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	hb = {
		MODE = "heroban",
		NAME = "mainlobby_label_heroban",
		DESC = "general_mode_herobanpick",
		ICON = "heroban",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = true,
		HIDE_DISABLED = true,
	},
	mwb = {
		MODE = "midwars_beta",
		NAME = "mainlobby_label_midwars_beta",
		DESC = "general_mode_midwars_beta",
		ICON = "midwars_beta",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	rb = {
		MODE = "reborn",
		NAME = "mainlobby_label_reborn_playerhosted",
		DESC = "general_mode_reborn",
		ICON = "banningdraft",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
}

local _SETTING = {
	lastmap = 'caldavar',
	botdifficulty = 0,
	fillbots = false
}

local _mapInfos = {}
if not GetCvarBool('cl_GarenaEnable') then
	_mapInfos[1] = {name='hg'}
end

local _regionInfos = {}

local _largeUI = false
local _uiStr = 'small'
local _selected_map = nil
local _hovered_map = nil

local _availableGameType = {}
local _availableMap = {}
local _availableGameMode = {}
local _availableRegion = {}

local _lastGameTypeString = ''
local _lastMapString = ''
local _lastGameModeString = ''
local _lastRegionString = ''

local _groupGameType = 1
local _groupMap = ''
local _groupGameModes = ''
local _groupTmmType = 1
local _groupLeader = 0
local _groupBotSetting = 1
local _groupRegions = ''

local _forbidUpdateGroupOption = false

local _updateGroupOptionTimer = 0
local _updateGroupOptionBuffer = {}

local function IsGameModeAvailable(gameType, map, gameMode)
	local inAvailableGameMode = false
	for i,v in ipairs(_availableGameMode) do
		if v == gameMode then 
			inAvailableGameMode = true 
			break
		end
	end

	local inMapGameMode = false
	if NotEmpty(gameType) then
		inMapGameMode = IsEnabledGameMode(gameType, map, gameMode, '')
	end

	local trick = true
	if map ~= 'con' and gameMode == 'cp' then trick = false end
	return inAvailableGameMode and inMapGameMode and trick
end

local function SoloQueue()
 	local mapInfo = MapSelection_V2:CreateGroup(true)
 	if mapInfo ~= nil then 
 		GetWidget('map_selection_startgame'):RegisterWatch("TMMDisplay", function() 
 			GetWidget('map_selection_startgame'):UnregisterWatch("TMMDisplay")
			interface:UICmd("SendTMMPlayerReadyStatus(1,"..mapInfo.gameType..");")
 		end)
 	end
end

local function EnterQueue()
	local map = _mapInfos[_selected_map]

	if (map.gameModes['lp'] ~= nil and map.gameModes['lp'].selected) or 
		(map.gameModes['cm'] ~= nil and map.gameModes['cm'].selected) then
		if Teammaking_V2:GetTeamSize() < 5 then 
			return
		end
	end
	interface:UICmd('SendTMMPlayerReadyStatus(1,'..map.gameType..');')
end

local function UpdateGroupOptions(selectedMapInfo)
	if _forbidUpdateGroupOption or not IsInGroup() or _groupLeader ~= UIGetAccountID() or selectedMapInfo.name == 'hg' then return end
	
	local tmmType = 0
	local gameType = 0
	local mapName = ''
	local gameModes = ''
	local regions = ''
	local ranked = true
	local botDifficulty = 1
	local randomizeBots = 1

	-- tmmType 
	if selectedMapInfo.name == 'coop' then 
		tmmType = 3
	elseif selectedMapInfo.name == 'con' then 
		tmmType = 4
	else
		tmmType = 2
	end
	-- Ranked
	if selectedMapInfo.name == 'coop' then 
		ranked = false
	end

	-- Game Type
	gameType = selectedMapInfo.gameType

	-- Map name
	mapName = selectedMapInfo.map

	-- Game Mode
	for k,v in pairs(selectedMapInfo.gameModes) do
		if v.selected then
			if NotEmpty(gameModes) then gameModes = gameModes..'|' end
			gameModes = gameModes..k
		end
	end

	-- Regions
	for i,v in ipairs(_regionInfos) do
		if v.status == 'selected' then 
			regions = regions..v.id..'|'
		end
	end

	-- botDifficulty
	if selectedMapInfo.botsetting ~= nil then 
		botDifficulty = selectedMapInfo.botsetting + 1
	end

	_updateGroupOptionBuffer.tmmType = tmmType
	_updateGroupOptionBuffer.gameType = gameType
	_updateGroupOptionBuffer.mapName = mapName
	_updateGroupOptionBuffer.gameModes = gameModes
	_updateGroupOptionBuffer.regions = regions
	_updateGroupOptionBuffer.ranked = ranked
	_updateGroupOptionBuffer.botDifficulty = botDifficulty
	_updateGroupOptionBuffer.randomizeBots = randomizeBots

	_updateGroupOptionTimer = GetTime()

	GetWidget('map_selection_startgame'):SetEnabled(false)
end

local function CheckGameModeSetting(name, gameMode)
	if _SETTING[name] == nil or _SETTING[name].gameMode == nil then 
		return true
	else
		for k,v in pairs(_SETTING[name].gameMode) do
			if gameMode == k then
				return v
			end
		end
		return true
	end
end
-------------------------------------  Init ---------------------------------------------------------
function MapSelection_V2:Init()
	_largeUI = Common_V2:UsingLargeUI()
	
	if _largeUI then 
		_uiStr = 'large'
	else
		_uiStr = 'small'
	end
	
	local setting = GetDBEntry('newui_hosted_mapselection_setting', nil , false)
	if setting ~= nil then _SETTING = setting end

	MapSelection_V2:PrepareUIByResolution()

	GetWidget('map_selection_startgame'):SetEnabled(false)
	GetWidget('map_selection_openregion'):SetEnabled(false)

	MapSelection_V2:ResetRegionByConfig()

	local widget = GetWidget('map_selection_trigger_help')
	widget:RegisterWatch('TMMOptionsAvailable', function(_, ...) MapSelection_V2:OnTMMOptionsAvailable(...) end)
	widget:RegisterWatch('RankInfoUpdated', function(_, ...) MapSelection_V2:OnRankInfoUpdated(...) end)
	widget:RegisterWatch('TMMDisplay', function(_, ...) MapSelection_V2:OnTMMDisplay(...) end)
	widget:RegisterWatch('TMMLeaveGroup', function(_, ...) MapSelection_V2:OnTMMLeaveGroup(...) end)
	widget:RegisterWatch('ReceiveCampaignSeasonRewards', function(_, ...) MapSelection_V2:OnReceiveCampaignSeasonRewards(...) end)
	widget:RegisterWatch('ChatServerMapSettingChanged', function() MapSelection_V2:OnChatServerMapSettingChanged() end)
end

function MapSelection_V2:PrepareUIByResolution()
	GetWidget('map_selection_large'):SetVisible(_largeUI)
	GetWidget('map_selection_small'):SetVisible(not _largeUI)

	if _largeUI then
		GetWidget('map_selection_title'):SetWidth('1507i')
	else
		GetWidget('map_selection_title'):SetWidth('1343i')
	end
end

-------------------------------------  Function ---------------------------------------------------------
function MapSelection_V2:Show()
	MapSelection_V2:ResetUI()
	GetWidget('map_selection'):FadeIn(150)

	local isTMMEnabled = IsTMMEnabled()
	if not isTMMEnabled then
		_mapInfos = {}
		if not GetCvarBool('cl_GarenaEnable') then
			_mapInfos[1] = {name='hg'}
		end

		_lastGameTypeString = ''
		_lastMapString = ''
		_lastGameModeString = ''
		_lastRegionString = ''
	end

	MapSelection_V2:RefresAll()

	if isTMMEnabled then
		interface:UICmd("RequestTMMPopularityUpdate()")
		RequestRankedPlayInfo()
	end
	Teammaking_V2:UpdateLeaverBlock()
	Cmd('GetCampaignSeasonRewards')
end

function MapSelection_V2:Hide()
	GetWidget('map_selection'):FadeOut(150)
	GetDBEntry('newui_hosted_mapselection_setting', _SETTING , true)

	Homepage_V2:InitMotd()
	GetWidget('map_selection_popup_con_info'):SetVisible(false)
	GetWidget('map_selection_popup_con_rewards'):SetVisible(false)
	GetWidget('map_selection_popup_map_info'):SetVisible(false)
end

function MapSelection_V2:RefresAll()
	_forbidUpdateGroupOption = true

	MapSelection_V2:SetMaps()
	MapSelection_V2:UpdateSeasonInfo()
	MapSelection_V2:SetOptions()

	if IsInGroup() then
		if _groupLeader ~= UIGetAccountID() then
			MapSelection_V2:UpdateGroupOptions()
		end
	else
		MapSelection_V2:SetDefault()
		MapSelection_V2:UpdateRegion()
	end
	_forbidUpdateGroupOption = false
end

function MapSelection_V2:UpdateMapInfo()
	_mapInfos = {}

	local map = {
					name = 'coop',
					map = 'caldavar',
					gameType = 2,
					botsetting = _SETTING.botdifficulty,
					fillbots = _SETTING.fillbots
				}
	table.insert(_mapInfos, map)
	
	map = {
			name = 'con',
			map = 'caldavar'
		   }
	table.insert(_mapInfos, map)

	if GetCvarBool('cl_GarenaEnable') then
		if NotEmpty(_availableMap[1]) then
			map = {
					name = _availableMap[1],
					map = _availableMap[1]
			  	}
			table.insert(_mapInfos, 2, map)
		end
	else
		map = {
				name = 'hg'
			  }
		table.insert(_mapInfos, 2, map)
	end

	if NotEmpty(_availableMap[2]) then
		map = {
				name = _availableMap[2],
				map = _availableMap[2],
			  }
		table.insert(_mapInfos, map)
	end

	if NotEmpty(_availableMap[3]) then
		map = {
				name = _availableMap[3],
				map = _availableMap[3]
			  }
		table.insert(_mapInfos, map)
	end

	for i,v in ipairs(_mapInfos) do
		local name = v.name

		if MAP_INFO[name] ~= nil then
			local allowGameType = MAP_INFO[name].allowGameType
			if allowGameType ~= nil and #allowGameType > 0 then
				if _SETTING[name] ~= nil and _SETTING[name].gameType ~= nil then
					v.gameType = _SETTING[name].gameType 
				else
					v.gameType = allowGameType[1]

					if GetCvarBool('cl_GarenaEnable') and (name == 'con' or name == 'caldavar') then
						v.gameType = allowGameType[2]
					end
				end
			end
		end

		v.gameModes = v.gameModes or {}
		if name ~= 'coop' and name ~= 'con' and name ~= 'hg' then 
			for k,u in pairs(GAMEMODE_INFO) do
				if IsGameModeAvailable(v.gameType, name, k) then
					local settingValue = CheckGameModeSetting(name, k)
					v.gameModes[k] = {selected = settingValue}
				end
			end
		elseif name == 'coop' then 
			v.gameModes['ap'] = {selected = true}
		elseif name == 'con' then 
			v.gameModes['rb'] = {selected = true}
		end
	end
end

function  MapSelection_V2:ResetUI()
	_hovered_map = nil

	for i=1, MAP_MAXCOUNT do
		GetWidget('map_selection_'.._uiStr..'_item_'..i):SetVisible(false)

		for k,v in pairs(GAMEMODE_INFO) do
			local widget = GetWidget('map_selection_'.._uiStr..'_item_'..i..'_gamemodes_'..k)
			if widget then 
				widget:SetVisible(false) 
				widget:SetEnabled(true)
			end
		end
	end

	if IsInGroup() and _groupLeader ~= UIGetAccountID() then 
		for i=1,MAP_MAXCOUNT do
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_group_mask'):SetVisible(true)
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_fillbots_groupfilter'):SetVisible(false)
		end
		--GetWidget('map_selection_region_mask'):SetVisible(true)
		GetWidget('map_selection_startgame'):SetEnabled(false)
	else
		for i=1,MAP_MAXCOUNT do
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_group_mask'):SetVisible(false)
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_fillbots_groupfilter'):SetVisible(true)
		end
		--GetWidget('map_selection_region_mask'):SetVisible(false)
		GetWidget('map_selection_startgame'):SetEnabled(Teammaking_V2:AllMembersReady())
	end

	--GetWidget('map_selection_region'):SetVisible(false)
	--GetWidget('map_selection_region_bg'):SetVisible(false)
	GetWidget('map_selection_popup_con_info'):SetVisible(false)
	GetWidget('map_selection_popup_con_rewards'):SetVisible(false)
	GetWidget('map_selection_popup_map_info'):SetVisible(false)
end

function MapSelection_V2:SetMaps()
	local function GetMapBgTextureName(name)
		if name == 'coop' or name == 'caldavar' or name == 'midwars' or name == 'hg' or 
			name == 'capturetheflag' or name == 'devowars' or name == 'grimmscrossing' or name == 'soccer' or name == 'team_deathmatch' or name == 'caldavar_reborn' or
			name == 'midwars_reborn' then 
			return '/ui/fe2/newui/res/mapselection/mapimage_'.._uiStr..'_'..name..'.png'
		elseif name == 'con' then
			return '/ui/fe2/newui/res/mapselection/mapimage_'.._uiStr..'_con/1.png'
		else
			return '/ui/fe2/newui/res/mapselection/mapimage_'.._uiStr..'_other.png'
		end
	end

	local function GetMapNameTextureName(name)
		if name == 'coop' or name == 'caldavar' or name == 'midwars' or name == 'hg' or name == 'con' or name == 'caldavar_reborn' or name == 'midwars_reborn' then 
			return '/ui/fe2/newui/res/mapselection/mapname_'.._uiStr..'_'..name..'.png'
		else
			return '/ui/fe2/newui/res/mapselection/mapname_'.._uiStr..'_other.png'
		end
	end

	local rootWidth = 0
	local count = #_mapInfos

	local large_gap = 5
	local small_gap = 5

	if count == 5 then 
		large_gap = 18
		small_gap = 14
	elseif count == 4 then
		large_gap = 31
		small_gap = 30
	elseif count <= 3 then
		large_gap = 50
		small_gap = 70
	end

	rootWidth_large = 287 * count + large_gap * (count - 1)
	rootWidth_small = 250 * count + small_gap * (count - 1)

	GetWidget('map_selection_large'):SetWidth(tostring(rootWidth_large)..'i')
	GetWidget('map_selection_small'):SetWidth(tostring(rootWidth_small)..'i')

	for i=1, MAP_MAXCOUNT do
		if _mapInfos[i] == nil then
			GetWidget('map_selection_'.._uiStr..'_item_'..i):SetVisible(false)
		else
			GetWidget('map_selection_'.._uiStr..'_item_'..i):SetVisible(true)
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_mapimg'):SetTexture(GetMapBgTextureName(_mapInfos[i].name))
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_mapname'):SetTexture(GetMapNameTextureName(_mapInfos[i].name))

			local name = _mapInfos[i].name
			if name == 'coop' or name == 'caldavar' or name == 'midwars' or name == 'hg' or name == 'con' then 
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_mapname_bg'):SetVisible(false)
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_mapname_bg2'):SetVisible(false)
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_mapname_text'):SetText('')
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_mapname_text2'):SetVisible(false)
			else
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_mapname_bg'):SetVisible(true)
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_mapname_bg2'):SetVisible(true)
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_mapname_text'):SetText(Translate('map_'..name))
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_mapname_text2'):SetVisible(true)
			end

			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_scale_con'):SetVisible(name == 'con')
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_noscale_con'):SetVisible(name == 'con')
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_conreward'):SetVisible(name == 'con')
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_mapinfo'):SetVisible(MAP_INFO[name] ~= nil and MAP_INFO[name].mapinfo ~= nil and MAP_INFO[name].mapinfo)
		end
	end
end

function MapSelection_V2:SetOptions()	
	for i,v in ipairs(_mapInfos) do
		local offset = 0

		-- Bot Setting
		if v.botsetting ~= nil then
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_botsetting'):SetY(tostring(offset)..'i')
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_botsetting'):SetVisible(true)
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_botsetting_combobox'):SetSelectedItemByValue(v.botsetting)
			offset = offset + 36
		else
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_botsetting'):SetVisible(false)
		end

		-- -- Game Type
		local showGameTypeChoose = false
		GetWidget('map_selection_'.._uiStr..'_item_'..i..'_gametype'):SetVisible(false)
		GetWidget('map_selection_'.._uiStr..'_item_'..i..'_gametype_combobox'):ClearItems()
		if MAP_INFO[v.name] ~= nil and MAP_INFO[v.name].allowGameType ~= nil then
			for _, u in ipairs(MAP_INFO[v.name].allowGameType) do
			 	if u == 1 or u == 2 or u == 6 or u == 7 then
			 		local text = ''
			 	    if u == 1 or u == 6 then 
			 	    	text = 'Normal'
			 	    else
			 	    	text = 'Casual'
			 	    end
			 		GetWidget('map_selection_'.._uiStr..'_item_'..i..'_gametype_combobox'):AddTemplateListItem('newui_combobox_item_template', tostring(u), 'label', text, 'color', '#7e6459', 'font', 'dyn_11')
			 		showGameTypeChoose = true
			 	end
			end 
		end
		if showGameTypeChoose then
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_gametype_combobox'):SetSelectedItemByValue(v.gameType)
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_gametype'):SetVisible(true)
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_gametype'):SetY(tostring(offset)..'i')
			offset = offset + 36
		end

		-- Fill Bots
		GetWidget('map_selection_'.._uiStr..'_item_'..i..'_fillbots'):SetVisible(v.name == 'coop')
		GetWidget('map_selection_'.._uiStr..'_item_'..i..'_fillbots'):SetY(tostring(offset)..'i')
		if v.fillbots then
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_fillbots_select'):SetButtonState(1)
		else
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_fillbots_select'):SetButtonState(0)
		end

		

		-- Game Modes
		GetWidget('map_selection_'.._uiStr..'_item_'..i..'_gamemodes'):SetVisible(v.name ~= 'hg' and v.name ~= 'coop')

		local isCpShowed = false
		local gameModesCount = 0
		GetWidget('map_selection_'.._uiStr..'_item_'..i..'_gamemodes'):SetY(tostring(offset)..'i')

		v.gameModes = v.gameModes or {}
		for k,u in pairs(v.gameModes) do
			local widget = GetWidget('map_selection_'.._uiStr..'_item_'..i..'_gamemodes_'..k)
			if widget then
				if k == 'cp' then
					isCpShowed = true
				end
				widget:SetVisible(true)
				gameModesCount = gameModesCount + 1
			end
		end

		for k,u in pairs(v.gameModes) do
			local widget = GetWidget('map_selection_'.._uiStr..'_item_'..i..'_gamemodes_'..k)
			if widget then
				-- widget:SetEnabled(gameModesCount > 1)
				if gameModesCount > 2 then 
					widget:SetCallback('onmouseover', function() 
						GetWidget('map_selection_'.._uiStr..'_item_'..i..'_gamemodes_clip'):ScaleHeight(tostring(GAMEMODE_HEIGHT*gameModesCount)..'i', 150) 
						GetWidget('map_selection_'.._uiStr..'_item_'..i..'_gamemodes_bg'):FadeIn(150)
						GetWidget('map_selection_'.._uiStr..'_item_'..i..'_gamemodes'):BringToFront()
						PlaySound('/shared/sounds/ui/revamp/region_hover.wav')

						local title = GAMEMODE_INFO[k].DESC
						Common_V2:ShowGenericTip(true, title, title..'_desc')
					end)
					widget:SetCallback('onmouseout', function() 
						GetWidget('map_selection_'.._uiStr..'_item_'..i..'_gamemodes_clip'):ScaleHeight(tostring(GAMEMODE_HEIGHT*2)..'i', 150) 
						GetWidget('map_selection_'.._uiStr..'_item_'..i..'_gamemodes_bg'):FadeOut(150)
						Common_V2:ShowGenericTip(false)
					end)
				else
					widget:SetCallback('onmouseover', function() 
						local title = GAMEMODE_INFO[k].DESC
						Common_V2:ShowGenericTip(true, title, title..'_desc') 

						PlaySound('/shared/sounds/ui/revamp/region_hover.wav')
					end)
					widget:SetCallback('onmouseout', function() Common_V2:ShowGenericTip(false) end)
				end
			end
		end

		local widget = GetWidget('map_selection_'.._uiStr..'_item_'..i..'_gamemodes_clip')
		widget:SetVAlign((gameModesCount<=2) and 'top' or 'bottom')
		widget:SetHeight((gameModesCount==1 and not isCpShowed) and (tostring(GAMEMODE_HEIGHT)..'i') or (tostring(GAMEMODE_HEIGHT*2)..'i'))

		widget = GetWidget('map_selection_'.._uiStr..'_item_'..i..'_gamemodes_arrow'):SetVisible(gameModesCount > 2)

		MapSelection_V2:UpdateGameModesSelection(i)
	end
end

function MapSelection_V2:UpdateGameModesSelection(index)
	if _mapInfos[index] ~= nil and _mapInfos[index].gameModes ~= nil then
		_SETTING[_mapInfos[index].name] = _SETTING[_mapInfos[index].name] or {}
		_SETTING[_mapInfos[index].name].gameMode = _SETTING[_mapInfos[index].name].gameMode or {}

		for k,v in pairs(_mapInfos[index].gameModes) do
			local widget = GetWidget('map_selection_'.._uiStr..'_item_'..index..'_gamemodes_'..k)
			if v.selected then 
				widget:SetButtonState(1)
				
				_SETTING[_mapInfos[index].name].gameMode[k] = true
			else
				widget:SetButtonState(0)
				
				_SETTING[_mapInfos[index].name].gameMode[k] = false
			end
		end

		UpdateGroupOptions(_mapInfos[index])
	end
end

function MapSelection_V2:SetDefault()
	if #_mapInfos == 0 then return end
	local defIndex = -1

	for i,v in ipairs(_mapInfos) do
		if v.name == _SETTING.lastmap then 
			defIndex = i
			break
		end
	end
	
	if defIndex == -1 then 
		defIndex = 1 
	end
	MapSelection_V2:SelectMap(defIndex)
	GetWidget('map_selection_startgame'):SetEnabled(Teammaking_V2:AllMembersReady())
end

function MapSelection_V2:UpdateGroupOptions()
	local groupGameModes = _groupGameModes
	local selectedName = ''
	if _groupGameType == 6 or _groupGameType == 7 then 
		selectedName = 'con'
	elseif _groupTmmType == 3 then 
		selectedName = 'coop'
		groupGameModes = 'ap'
	else
		selectedName = _groupMap
	end

	local regions = explode('|', _groupRegions)
	for _,v in ipairs(_regionInfos) do
		if v.status == 'selected' or v.status == 'unselected' then
			local enabled = false
			for _,k in ipairs(regions) do
				if v.id == k then
					enabled = true
					break
				end
			end

			if enabled then
				v.status = 'selected'
			else
				v.status = 'unselected'
			end
		end
	end
	MapSelection_V2:UpdateRegion()

	for i,v in ipairs(_mapInfos) do
		if v.name == selectedName then 
			MapSelection_V2:SelectMap(i)

			if v.botsetting ~= nil then 
				v.botsetting = _groupBotSetting - 1
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_botsetting_combobox'):SetSelectedItemByValue(v.botsetting)
			end

			v.gameType = _groupGameType
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_gametype_combobox'):SetSelectedItemByValue(v.gameType)

			local temp = explode('|', groupGameModes)
			for k,u in pairs(v.gameModes) do
				u.selected = false
			end
			for _, mode in ipairs(temp) do
				if v.gameModes[mode] then 
					v.gameModes[mode].selected = true
				end
			end
			MapSelection_V2:UpdateGameModesSelection(i)
		end
	end
end

function MapSelection_V2:UpdateSeasonInfo()
	for i,v in ipairs(_mapInfos) do
		if v.name == 'con' then 
			local rankInfo = nil 
			if v.gameType == 6 then 
				rankInfo = GetRankedPlayInfo(0)
			else
				rankInfo = GetRankedPlayInfo(1)
			end

			if rankInfo.level > 0 then 
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_scale_con_norank'):SetVisible(false)
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_scale_con_rankinfobg'):SetVisible(true)
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_noscale_con'):SetVisible(true)
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_noscale_con_wins'):SetText(tostring(rankInfo.wins))
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_noscale_con_losses'):SetText(tostring(rankInfo.losses))
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_noscale_con_winstreaks'):SetText(tostring(rankInfo.winstreaks))
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_noscale_con_rating'):SetText('MMR: ' .. string.format('%.0f', tostring(rankInfo.mmr)))
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_mapimg'):SetTexture('/ui/fe2/newui/res/mapselection/mapimage_'.._uiStr..'_con/'..rankInfo.level..'.png')
			else
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_scale_con_norank'):SetVisible(true)
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_scale_con_rankinfobg'):SetVisible(false)
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_noscale_con'):SetVisible(false)
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_mapimg'):SetTexture('/ui/fe2/newui/res/mapselection/mapimage_'.._uiStr..'_con/0.png')

				for j=1, RANK_PLACEMENT_MAXCOUNT do
					local gameWidget = GetWidget('map_selection_'.._uiStr..'_item_'..i..'_scale_con_norank_game'..j)
					local resultWidget = GetWidget('map_selection_'.._uiStr..'_item_'..i..'_scale_con_norank_result'..j)
					if gameWidget ~= nil then
						gameWidget:SetVisible(rankInfo.placementMatches >= j)
					end

					if string.len(rankInfo.placementDetail) < j then 
						resultWidget:SetTexture('/ui/fe2/newui/res/mapselection/mapimage_'.._uiStr..'_con/no_'..j..'.png')
					elseif string.sub(rankInfo.placementDetail, j, j) == '0' then 
						resultWidget:SetTexture('/ui/fe2/newui/res/mapselection/mapimage_'.._uiStr..'_con/loss_'..j..'.png')
					else
						resultWidget:SetTexture('/ui/fe2/newui/res/mapselection/mapimage_'.._uiStr..'_con/win_'..j..'.png')
					end
				end
			end

			break
		end
	end
end

function MapSelection_V2:SelectMap(index)
	if _selected_map == index then return end

	-- handle solomap 
	if _mapInfos[index].name == 'solomap' and IsInGroup() and Teammaking_V2:GetTeamSize() > 1 then
		Trigger('TriggerDialogBox', 'newui_mapselection_playsolomap_title', 'general_cancel', 'general_ok', '', '', 
										'newui_mapselection_playsolomap_desc', '', '', 'Teammaking_V2:LeaveGroup()  MapSelection_V2:SelectMap('..index..')')
		return
	end

	if _selected_map ~= nil then 
		local widget =  GetWidget('map_selection_'.._uiStr..'_item_'.._selected_map..'_scale')
		if widget then 
			widget:SetWidth('100%', 100)
			widget:SetHeight('100%', 100)
			GetWidget('map_selection_'.._uiStr..'_item_'.._selected_map..'_selected'):SetVisible(false)
			GetWidget('map_selection_'.._uiStr..'_item_'.._selected_map..'_mask1'):SetVisible(true)
			GetWidget('map_selection_'.._uiStr..'_item_'.._selected_map..'_mask'):SetVisible(true)
			_selected_map = nil 
		end
	end

	local widget = GetWidget('map_selection_'.._uiStr..'_item_'..index..'_scale')
	if widget then
		widget:SetWidth(HOVER_SCALE)
		widget:SetHeight(HOVER_SCALE)
		GetWidget('map_selection_'.._uiStr..'_item_'..index):BringToFront()
		GetWidget('map_selection_'.._uiStr..'_item_'..index..'_selected'):SetVisible(true)
		GetWidget('map_selection_'.._uiStr..'_item_'..index..'_mask1'):SetVisible(false)
		GetWidget('map_selection_'.._uiStr..'_item_'..index..'_mask'):SetVisible(false)
		_selected_map = index
		
		-- Set Loading Info
		if _mapInfos[_selected_map].name ~= 'hg' then 
			Loading_V2:SetQueueLoadingInfo(_mapInfos[_selected_map].name, _mapInfos[_selected_map].map)
			Loading_V2:SetMMLoadingInfo(_mapInfos[_selected_map].name, _mapInfos[_selected_map].map)	
		else
			GetWidget('map_selection_startgame'):SetEnabled(Teammaking_V2:AllMembersReady())
			_updateGroupOptionTimer = 0
		end

		_SETTING.lastmap = _mapInfos[_selected_map].name
		UpdateGroupOptions(_mapInfos[_selected_map])

		if IsInGroup() and _groupLeader == UIGetAccountID() and _mapInfos[_selected_map].name == 'con' and Teammaking_V2:GetTeamSize() == INVALID_CON_TEAMMEMBER_COUNT then
			MapSelection_V2:SetDisableDesc(Translate('season_invalid_temamember_number'))
		else
			MapSelection_V2:SetDisableDesc('')
		end
	end
end

function MapSelection_V2:ResetRegionByConfig()
	_regionInfos = {}
	local regionMap, regionsList, regionInfo = HoN_Region:GetMatchmakingRegionInfo()
	for i,v in ipairs(regionsList) do
		local value = nil
		if _SETTING.regions ~= nil then 
			value = _SETTING.regions[v]
			if value == 'disabled' then
				value = 'selected'
			end
		end
		_regionInfos[i] = {name=v, status=(value or 'unselected'), flag = regionInfo[v].FLAG, id=regionInfo[v].ID}
	end
end

function MapSelection_V2:UpdateRegion()
	--_SETTING.regions = _SETTING.regions or {}

	for _,v in ipairs(_regionInfos) do
		if v.status ~= 'disabled' or GetCvarBool('cl_GarenaEnable') then
			local available = CanPlayerAccessRegion(v.id) and (not IsInGroup() or CanGroupAccessRegion(v.id))
			if not available then
				Echo('Can not Access Region:'..v.id)
				Echo(tostring(CanPlayerAccessRegion(v.id)))
				Echo(tostring(IsInGroup()))
				Echo(tostring(CanGroupAccessRegion(v.id)))

				v.status = 'disabled'
			elseif v.status ~= 'unselected' and v.status ~= 'selected' then
				v.status = 'unselected'
			end
		end
	end

	local hasRegion = false
	for i=1,REGION_MAXCOUNT do
		local region = _regionInfos[i]

		if region ~= nil then
			GetWidget('map_selection_region_item_'..i):SetVisible(true)

			GetWidget('map_selection_region_item_'..i..'_flag'):SetTexture(region.flag)
			GetWidget('map_selection_region_item_'..i..'_flag'):SetRenderMode('normal')

			if region.status == 'selected' then 
				GetWidget('map_selection_region_item_'..i..'_selection'):SetButtonState(1)
				GetWidget('map_selection_region_item_'..i..'_selection'):SetEnabled(true)
			elseif region.status == 'unselected' then
				GetWidget('map_selection_region_item_'..i..'_selection'):SetButtonState(0)
				GetWidget('map_selection_region_item_'..i..'_selection'):SetEnabled(true)
			else
				GetWidget('map_selection_region_item_'..i..'_selection'):SetButtonState(0)
				GetWidget('map_selection_region_item_'..i..'_selection'):SetEnabled(false)
				GetWidget('map_selection_region_item_'..i..'_flag'):SetRenderMode('grayscale')
			end

			Common_V2:SetCheckboxText('map_selection_region_item_'..i..'_selection', string.upper(region.name))

			hasRegion = true
		else
			GetWidget('map_selection_region_item_'..i):SetVisible(false)
		end
	end

	UpdateGroupOptions(_mapInfos[_selected_map])
	GetWidget('map_selection_openregion'):SetEnabled(hasRegion)
end

function MapSelection_V2:CreateGroup(isSolo)
	if IsInGroup() then return end

	local selectedMapInfo = nil
	if _selected_map ~= nil and _mapInfos[_selected_map].name ~= 'hg' then
		selectedMapInfo = _mapInfos[_selected_map]
	elseif _mapInfos[1].name ~= 'hg' then
		selectedMapInfo = _mapInfos[1]
	end

	if selectedMapInfo ~= nil then

		local tmmType = 0
		local gameType = 0
		local mapName = ''
		local gameModes = ''
		local regions = ''
		local ranked = true
		local botDifficulty = 1
		local randomizeBots = 1

		-- TMM Type & Ranked
		if selectedMapInfo.name == 'coop' then 
			tmmType = 3
			ranked = false
		elseif selectedMapInfo.name == 'con' then
			tmmType = 4
		elseif isSolo then
			tmmType = 1
		else
			tmmType = 2
		end

		-- Game Type
		gameType = selectedMapInfo.gameType

		-- Map name
		mapName = selectedMapInfo.map

		-- Game Mode
		for k,v in pairs(selectedMapInfo.gameModes) do
			if v.selected then
				if NotEmpty(gameModes) then gameModes = gameModes..'|' end
				gameModes = gameModes..k
			end
		end

		-- Regions
		for i,v in ipairs(_regionInfos) do
			if v.status == 'selected' then 
				regions = regions..v.id..'|'
			end
		end
 
		interface:UICmd("CreateTMMGroup("..tmmType..", "..gameType..", '"..mapName.."', '"..gameModes.."', '"..regions.."', "..tostring(ranked)..", cc_TMMMatchFidelity, "..botDifficulty..", "..tostring(randomizeBots)..");")
		return selectedMapInfo
	end

	return nil
end

function MapSelection_V2:SetDisableDesc(text)
	if Empty(text) then
		GetWidget('map_selection_disable_desc'):SetVisible(false)
	else
		GetWidget('map_selection_disable_desc'):SetVisible(true)
		GetWidget('map_selection_disable_desc_text'):SetText(text)
	end
end
----------------------------------- Trigger Handlers -----------------------------------------------
function MapSelection_V2:OnTMMOptionsAvailable(gameType, map, gameMode, region)
	_availableGameType = explode('|', gameType)
	_availableMap = explode('|', map)
	_availableGameMode = explode('|', gameMode)
	_availableRegion = explode('|', region)

	if map ~= _lastMapString or gameMode ~= _lastGameModeString then
		MapSelection_V2:UpdateMapInfo()
		MapSelection_V2:RefresAll()
	end

	if region ~= _lastRegionString then
		for _,v in ipairs(_regionInfos) do
			local available = false
			for _,k in ipairs(_availableRegion) do
				if v.id == k then
					available = true
					break
				end
			end

			if not available then 
				v.status = 'disabled'
			elseif v.status == 'disabled' then
				v.status = 'unselected'
			end
		end
		MapSelection_V2:UpdateRegion()
	end

	_lastGameTypeString = gameType
	_lastMapString = map
	_lastGameModeString = gameMode
	_lastRegionString = region
end

function MapSelection_V2:OnRankInfoUpdated()
	MapSelection_V2:UpdateSeasonInfo()
end

function MapSelection_V2:OnTMMDisplay(...)
	--[[ params
		-- for player slots 1 - 5
		0 = Slot Account ID
		1 = Slot Username
		2 = Slot number
		3 = Slot TMR
		4 = Player Loading TMM Status | Player Ready Status
		--
		25 = Update type
		26 = group size
		27 = average TMR
		28 = leader account id
		29 = game type
		30 = MapNames
		31 = GameModes
		32 = Regions
		33 = TSNULL
		34 = PlayerInvitationResponses
		35 = TeamSize
		36 = TSNULL
		37 = Verified
		38 = VerifiedOnly
		39 = BotDifficulty
		40 = RandomizeBots
		41 = GroupType
	]]
	-- Echo('^gOnTMMDisplay -------------------- ')
	-- Echo('game type: '..arg[30])
	-- Echo('map name: '..arg[31])
	-- Echo('game modes: '..arg[32])
	-- Echo('group size: '..arg[27])
	-- Echo('group leader: '..arg[29])
	-- Echo('bot difficulty: '..arg[40])

	_groupGameType = tonumber(arg[30])
	_groupMap = arg[31]
	_groupGameModes = arg[32]
	_groupTmmType = tonumber(arg[42])
	_groupLeader = arg[29]
	_groupBotSetting = tonumber(arg[40])
	_groupRegions = arg[33]
	
	if _groupLeader ~= UIGetAccountID() then 
		MapSelection_V2:UpdateGroupOptions() 
		for i=1,MAP_MAXCOUNT do
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_group_mask'):SetVisible(true)
			GetWidget('map_selection_'.._uiStr..'_item_'..i..'_fillbots_groupfilter'):SetVisible(false)
		end
		--GetWidget('map_selection_region_mask'):SetVisible(true)
		GetWidget('map_selection_startgame'):SetEnabled(false)
	end 

	if _groupLeader == UIGetAccountID() then 
		local map = _mapInfos[_selected_map]
		if map.name ~= 'hg' then
			if map.name == 'con' and tonumber(arg[27]) == INVALID_CON_TEAMMEMBER_COUNT then
				GetWidget('map_selection_startgame'):SetEnabled(false)
				MapSelection_V2:SetDisableDesc(Translate('season_invalid_temamember_number'))
			else
				GetWidget('map_selection_startgame'):SetEnabled(map.map == _groupMap and map.gameType == _groupGameType and Teammaking_V2:AllMembersReady())
				MapSelection_V2:SetDisableDesc('')
			end
		end

		_forbidUpdateGroupOption = true
		MapSelection_V2:UpdateRegion()
		_forbidUpdateGroupOption = false
	end
end

function MapSelection_V2:OnTMMLeaveGroup()
	for i=1,MAP_MAXCOUNT do
		GetWidget('map_selection_'.._uiStr..'_item_'..i..'_group_mask'):SetVisible(false)
		GetWidget('map_selection_'.._uiStr..'_item_'..i..'_fillbots_groupfilter'):SetVisible(true)
	end
	--GetWidget('map_selection_region_mask'):SetVisible(false)
	GetWidget('map_selection_startgame'):SetEnabled(true)
	MapSelection_V2:SetDisableDesc('')

	MapSelection_V2:ResetRegionByConfig()
	MapSelection_V2:UpdateRegion()
	_lastRegionString = ''
	if IsTMMEnabled() then
		interface:UICmd("RequestTMMPopularityUpdate()")
	end
end

function MapSelection_V2:OnReceiveCampaignSeasonRewards(highestleve, ...)
	local function GetPrefixFromProductType(type)
		if type == 'Chat Color' then 
			return 'cc' 
		elseif type == 'Chat Symbol' then
			return 'cs'
		elseif type == 'Account Icon' then 
			return 'ai'
		elseif type == 'Alt Avatar' then
			return 'aa' 
		end

		return ''
	end
	
	GetWidget('map_selection_popup_con_rewards_icon'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevelAfterS6(tonumber(highestleve)))

	local levelStr = Translate('player_compaign_level_name_S7_'..highestleve)
	GetWidget('map_selection_popup_con_rewards_tips'):SetText(Translate('season_rewards_tips', 'level', levelStr))
	
	local medal = tonumber(highestleve)
	local widget = GetWidget('map_selection_popup_con_rewards_effect')
	if medal > 0 and medal <= 5 then 
		widget:StartEffect('/ui/fe2/season/effects/popup_back_1.effect', 0.5, 0.5, '1 1 1', 1)
	elseif medal > 5 and medal <= 10 then 
		widget:StartEffect('/ui/fe2/season/effects/popup_back_2.effect', 0.5, 0.5, '1 1 1', 1)
	elseif medal > 10 and medal <= 13 then
		widget:StartEffect('/ui/fe2/season/effects/popup_back_3.effect', 0.5, 0.5, '1 1 1', 1)
	elseif medal > 13 and medal <= 15 then 
		widget:StartEffect('/ui/fe2/season/effects/popup_back_4.effect', 0.5, 0.5, '1 1 1', 1)
	elseif medal == 16 then
		widget:StartEffect('/ui/fe2/season/effects/popup_back_5.effect', 0.5, 0.5, '1 1 1', 1)
	elseif medal >= 17 then
		widget:StartEffect('/ui/fe2/season/effects/popup_back_6.effect', 0.5, 0.5, '1 1 1', 1)
	end

	for i=1, 6 do
		local type = arg[(i-1)*5+1]
		local id = arg[(i-1)*5+2]
		local path = arg[(i-1)*5+3]
		local count = arg[(i-1)*5+4]
		local productType = arg[(i-1)*5+5]

		GetWidget('map_selection_popup_con_rewards_item'..i..'_text'):SetFont('dyn_12')
		if type ~= nil then
			if tonumber(type) == 1 then
				GetWidget('map_selection_popup_con_rewards_item'..i..'_icon'):SetTexture(path)

				local typeprefix = GetPrefixFromProductType(productType)
				local typeText = ''
				if NotEmpty(typeprefix) then
					GetWidget('map_selection_popup_con_rewards_item'..i..'_text'):SetFont('dyn_10')
					typeText = '^885['..Translate('general_product_type_'..typeprefix)..']^* \n'
				end

				local text = typeText..Translate('mstore_product'..id..'_name')
				if tonumber(count) > 1 then
					text = text..' ^279x '..count
				end
				GetWidget('map_selection_popup_con_rewards_item'..i..'_text'):SetText(text)
			elseif tonumber(type) == 2 then
				GetWidget('map_selection_popup_con_rewards_item'..i..'_icon'):SetTexture('/ui/fe2/season/gold.tga')
				local text = Translate('season_rewards_goldcoin')..' ^279x '..count
				GetWidget('map_selection_popup_con_rewards_item'..i..'_text'):SetText(text)
			elseif tonumber(type) == 3 then
				GetWidget('map_selection_popup_con_rewards_item'..i..'_icon'):SetTexture('/ui/fe2/season/plinko.tga')
				local text = Translate('season_rewards_plinkoticket')..' ^279x '..count
				GetWidget('map_selection_popup_con_rewards_item'..i..'_text'):SetText(text)
			end
		end
		GetWidget('map_selection_popup_con_rewards_item'..i):SetVisible(type~=nil)
	end

	MapSelection_V2.popupTimer = GetTime()
    MapSelection_V2.popupShow = true
    GetWidget('map_selection_popup_con_rewards'):SetVisible(true)
end

function MapSelection_V2:OnChatServerMapSettingChanged()
	-- if player is queuing 'caldavar' or 'midwars' , don't popup window (hard code)
	if IsInQueue() and _groupMap ~= 'caldavar' and _groupMap ~= 'midwars' then
		GetWidget('map_selection_mapsetting_changed'):FadeIn(150)
	end
end
------------------------------------- UI Handler ----------------------
function MapSelection_V2:OnFrame()
	if _updateGroupOptionTimer > 0 and (GetTime() - _updateGroupOptionTimer) >= UPDATEGROUPOTPIONDELAYTIME then 
		local tmmType = _updateGroupOptionBuffer.tmmType
		local gameType = _updateGroupOptionBuffer.gameType
		local mapName = _updateGroupOptionBuffer.mapName
		local gameModes = _updateGroupOptionBuffer.gameModes
		local regions = _updateGroupOptionBuffer.regions
		local ranked = _updateGroupOptionBuffer.ranked 
		local botDifficulty = _updateGroupOptionBuffer.botDifficulty 
		local randomizeBots = _updateGroupOptionBuffer.randomizeBots

		GroupChangeType(tmmType)

		interface:UICmd("SendTMMGroupOptionsUpdate("..gameType..", '"..mapName.."', '"..gameModes.."', '"..regions.."', "..tostring(ranked)..", cc_TMMMatchFidelity, "..botDifficulty..", "..tostring(randomizeBots)..");")
		_updateGroupOptionTimer = 0
	end	
end

function MapSelection_V2:OnClickMap(index)
	MapSelection_V2:SelectMap(index)
end

function MapSelection_V2:OnHoverMap(index, hover)
	if hover then 
		for i=1, MAP_MAXCOUNT do
			if i ~= _selected_map then 
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_scale'):ScaleWidth('100%', 100)
				GetWidget('map_selection_'.._uiStr..'_item_'..i..'_scale'):ScaleHeight('100%', 100)
			end
		end
		GetWidget('map_selection_'.._uiStr..'_item_'..index..'_scale'):ScaleWidth(HOVER_SCALE, 150)
		GetWidget('map_selection_'.._uiStr..'_item_'..index..'_scale'):ScaleHeight(HOVER_SCALE, 150)
		GetWidget('map_selection_'.._uiStr..'_item_'..index..'_mask1'):FadeOut(150)

		GetWidget('map_selection_'.._uiStr..'_item_'..index):BringToFront()

		if _selected_map ~= nil then 
			GetWidget('map_selection_'.._uiStr..'_item_'.._selected_map):BringToFront()
		end
	else
		if index ~= _selected_map then 
			GetWidget('map_selection_'.._uiStr..'_item_'..index..'_scale'):ScaleWidth('100%', 100)
			GetWidget('map_selection_'.._uiStr..'_item_'..index..'_scale'):ScaleHeight('100%', 100)
			GetWidget('map_selection_'.._uiStr..'_item_'..index..'_mask1'):FadeIn(100)
		end
	end
end

function MapSelection_V2:OnSelectBotSetting(index, select)
	local value = tonumber(select)
	if _mapInfos[index] ~= nil and value ~= nil and _mapInfos[index].botsetting ~= value then
		_mapInfos[index].botsetting = value

		_SETTING.botdifficulty = value
		UpdateGroupOptions(_mapInfos[index])
	end
end

function MapSelection_V2:OnSelectGameType(index, select)
	local value = tonumber(select)
	if _mapInfos[index] ~= nil and value ~= nil and _mapInfos[index].gameType ~= value then
		_mapInfos[index].gameType = value

		if _mapInfos[index].name == 'con' then
			MapSelection_V2:UpdateSeasonInfo()
		end

		_SETTING[_mapInfos[index].name] = _SETTING[_mapInfos[index].name] or {}
		_SETTING[_mapInfos[index].name].gameType = value

		UpdateGroupOptions(_mapInfos[index])
	end
end

function MapSelection_V2:OnSelectGameMode(index, mode, selected)
	if _mapInfos[index] ~= nil and _mapInfos[index].gameModes ~= nil and _mapInfos[index].gameModes[mode] ~= nil then
		if _mapInfos[index].gameModes[mode].selected == selected then return end

		if not selected then 
			local selectedCount = 0
			for k,v in pairs(_mapInfos[index].gameModes) do
				if v.selected then 
					selectedCount = selectedCount + 1
				end
			end

			if selectedCount <= 1 then 
				_forbidUpdateGroupOption = true
				MapSelection_V2:UpdateGameModesSelection(index)
				_forbidUpdateGroupOption = false
				return
			end
		end

		_mapInfos[index].gameModes[mode].selected = selected
		MapSelection_V2:UpdateGameModesSelection(index)
	end
end

function MapSelection_V2:OnStartGame()

	local map = _mapInfos[_selected_map]
	if map == nil then return end

	if(map.name ~= 'coop' and map.name ~= 'hg') then
		--checks to see if at least one region is selected
		local atLeastOneRegion = false
		for i,v in ipairs(_regionInfos) do
			if v.status == 'selected' then 
				atLeastOneRegion = true
				break
			end
		end
		
		--show popup saying no region selected and abort start game function
		if (atLeastOneRegion == false) then
			GetWidget('mm_popup_noregionselected'):DoEventN(0)
			return
		end
	end

	if not GetWidget('map_selection_startgame'):IsEnabled() then return end
	
	if IsInGroup() and _groupLeader ~= UIGetAccountID() then return end 

	if _mapInfos[_selected_map].name ~= 'hg' then 
		Loading_V2:SetQueueLoadingInfo(_mapInfos[_selected_map].name, _mapInfos[_selected_map].map)
		Loading_V2:SetMMLoadingInfo(_mapInfos[_selected_map].name, _mapInfos[_selected_map].map)	
	end

	if map.name == 'coop' and map.fillbots then
		if not IsInGroup() or Teammaking_V2:GetTeamSize() < 2 then
			if IsInGroup() then 
				Teammaking_V2.supressDialog  = true
				Teammaking_V2:LeaveGroup()
			end

			local sGameStr = 'StartGame practice LocalBotsGame mode:botmatch map:caldavar casual:true'
			if (math.random(2) == 1) then
				sGameStr = sGameStr..' randombots:4|5'
			else
				sGameStr = sGameStr..' randombots:5|4'
			end
			Cmd(sGameStr)
		elseif _groupLeader == UIGetAccountID() then
			local botSlotNum = TEAMMEMBER_MAX_COUNT - Teammaking_V2:GetTeamSize()
			local botsTable = GetBotDefinitions()
			
			if #botsTable < botSlotNum then 
				Echo('^rThere is no enough bots')
				return 
			end

			local gameBots = {}
			while #gameBots < botSlotNum do
				local index = math.random(#botsTable)
				table.insert(gameBots, botsTable[index].sName)
				table.remove(botsTable, index)
			end

			for i=1, botSlotNum do
				SetTeamBot(Teammaking_V2:GetTeamSize()+i-1, gameBots[i])
			end

			GetWidget('map_selection_trigger_help'):Sleep(500, function() EnterQueue() end)
			GetWidget('map_selection_mask'):SetVisible(true)
			GetWidget('map_selection_mask'):Sleep(10000, function() GetWidget('map_selection_mask'):SetVisible(false) end)
		end
	elseif map.name ~= 'hg' then
		PlaySound('/shared/sounds/ui/revamp/startgame.wav')
		if IsLeaver() then
			Teammaking_V2:UpdateLeaverBlock()
		elseif IsInGroup() then 
			if _groupLeader == UIGetAccountID() then
				EnterQueue()

				GetWidget('map_selection_mask'):SetVisible(true)
				GetWidget('map_selection_mask'):Sleep(10000, function() GetWidget('map_selection_mask'):SetVisible(false) end)
			end
		else
			SoloQueue()
			GetWidget('map_selection_mask'):SetVisible(true)
			GetWidget('map_selection_mask'):Sleep(10000, function() GetWidget('map_selection_mask'):SetVisible(false) end)
		end 
	else
		MapSelection_V2:OnClickHostedGame()
		PlaySound('/shared/sounds/ui/revamp/region_click.wav')
	end
end

function MapSelection_V2:OnClickRegion()
	--local widget = GetWidget('map_selection_region')
	--local widget2 = GetWidget('map_selection_region_bg')
	--if widget:IsVisible() then
	--	widget:FadeOut(150)
	--	widget2:SetVisible(false)
	--else
	--	widget:FadeIn(150)
	--	widget2:SetVisible(true)
	--end
end

function MapSelection_V2:OnClickSelectRegion(index)
	local state = GetWidget('map_selection_region_item_'..index..'_selection'):GetButtonState()
	if state == 0 then 
		_regionInfos[index].status = 'unselected'
	else
		_regionInfos[index].status = 'selected'
	end

	local hasSelected = false
	for i,v in ipairs(_regionInfos) do
		if v.status == 'selected' then 
			hasSelected = true
			break
		end
	end

	if not hasSelected then 
		_regionInfos[index].status = 'selected'
	end

	_SETTING.regions = _SETTING.regions or {}
	for i,v in ipairs(_regionInfos) do
		_SETTING.regions[v.name] = v.status
	end

	MapSelection_V2:UpdateRegion()
end

function MapSelection_V2:OnClickHostedGame()
	if IsInGroup() then
		Trigger('TriggerDialogBox', 'newui_mapselection_playhg_title', 'general_cancel', 'general_ok', '', '', 
										'newui_mapselection_playhg_desc', '', '', 'Teammaking_V2:LeaveGroup() Player_Hosted_V2:Show()')
	else
		Player_Hosted_V2:Show()
	end	
end

function MapSelection_V2:OnClickConRewards()
	local url = ''
	if GetCvarBool('cl_GarenaEnable') then
		url = 'http://www.hon.in.th/events/20161215_seasoninfo/?hongameclientcookie='..Client.GetCookie()..'|sea|'..GetCvarString('host_language')
	else
		url = 'http://fun.naeu.heroesofnewerth.com/events/20161215_seasoninfo/?hongameclientcookie='..Client.GetCookie()..'|naeu|'..GetCvarString('host_language')
	end

	GetWidget('map_selection_popup_con_info'):FadeIn(150)
	UIManager.GetInterface('webpanel'):HoNWebPanelF('LoadURLWithThrob', url, GetWidget('map_selection_popup_con_info_parent'))
end

function MapSelection_V2:OnClickMapInfo()
	local map = _mapInfos[_selected_map]
	if map == nil then return end

	local mapName = map.map

	for k,v in pairs(MAP_INFO) do
		local widget = GetWidget('map_selection_popup_map_info1_'..k)
		if widget then
			widget:SetVisible(k == mapName)
		end

		widget = GetWidget('map_selection_popup_map_info2_'..k)
		if widget then
			widget:SetVisible(k == mapName)
		end
	end

	GetWidget('map_selection_popup_map_info3'):SetTexture('/ui/fe2/newui/res/mapselection/mapinfo/'..mapName..'.png')
	GetWidget('map_selection_popup_map_info_mapname'):SetText(Translate('map_'..mapName))

	GetWidget('map_selection_popup_map_info'):FadeIn(150)
end

function MapSelection_V2:OnConRewardPopupFrame()
	if MapSelection_V2.popupShow then
	    local value = (GetTime() - MapSelection_V2.popupTimer) / 200
	    if (value > 1) then
	    	value = 1
	    end

	    GetWidget('map_selection_popup_con_rewards_levelup_anim'):SetWidth(tostring(96*value)..'h')
	    GetWidget('map_selection_popup_con_rewards_levelup_anim'):SetHeight(tostring(96*value)..'h')
	else
		local value = (GetTime() - MapSelection_V2.popupTimer) / 200 + 1.0
		GetWidget('map_selection_popup_con_rewards_levelup_anim'):SetWidth(tostring(96*value)..'h')
		GetWidget('map_selection_popup_con_rewards_levelup_anim'):SetHeight(tostring(96*value)..'h')

		if (value > 1.8) then
			GetWidget('map_selection_popup_con_rewards'):SetVisible(false)
			GetWidget('map_selection_popup_con_rewards_levelup_anim'):SetWidth('96h')
			GetWidget('map_selection_popup_con_rewards_levelup_anim'):SetHeight('96h')
		end
	end 
end

function MapSelection_V2:OnClickSetFillBots(index)
	local state = GetWidget('map_selection_'.._uiStr..'_item_'..index..'_fillbots_select'):GetButtonState()
	if state == 0 then
		_mapInfos[index].fillbots = false
		_SETTING.fillbots = false
	else
		_mapInfos[index].fillbots = true
		_SETTING.fillbots = true
	end
end
-- function MapSelection_V2:ReadyTeam()
-- 	local map = _mapInfos[_selected_map]
-- 	if _groupLeader == UIGetAccountID() then return end

-- 	interface:UICmd('SendTMMPlayerReadyStatus(1,'..map.gameType..');')
-- end