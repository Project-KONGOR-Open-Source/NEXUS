local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort
local interface, interfaceName = object, object:GetName()

Player_Hosted_V2 = {}

local GAMELIST_MAX_COUNT = 13
local GAMELIST_OPTIONS_MAX_COUNT = 6
local SERVERLIST_MAX_COUNT = 9

local _gameList = {}
local _sortedGameList = {}
local _selectedGame = nil
local _gameListSortKey = nil

local _serverList = {}
local _sortedServerList = {}
local _selectedServer = nil 
local _serverListSortKey = nil

local _corePoolDisabledMode = {
	singledraft = true,
	randomdraft = true,
	banningdraft = true
}

local _localServerAvailable = false

local _FilterSetting = {
	['gamename'] = '',
	['teamsize'] = '0',
	['mapname'] = 'any',
	['minpsr'] = '0',
	['maxpsr'] = '0',
	['region'] = 'any',
	['ping'] = '0',
	['prequery'] = false,
	['hiderestrictedgame'] = false,
	['gamemode'] = {
		['allrandom'] = 'required',
		['banningdraft'] = 'required',
		['normal'] = 'required',
		['randomdraft'] = 'required',
		['blindban'] = 'required',
		['lockpick'] = 'required',
		['banningpick'] = 'required',
		['singledraft'] = 'required',
		['botmatch'] = 'required',
		['krosmode'] = 'required',
		['captainsmode'] = 'required',
		['solosame'] = 'required',
		['solodiff'] = 'required',
		['reborn'] = 'required',
		['midwars_beta'] = 'required',
		['heroban'] = 'required'
	},
	['gameoption'] = {
		['full'] = 'np',
		['noleaver'] = 'np',
		['rapidfire'] = 'np',
		['nostats'] = 'np',
		['verifiedonly'] = 'np',
		['casual'] = 'np',
		['private'] = 'np',
		['shuffleteams'] = 'np',
		-- ['devheroes'] = 'np',
		['norepick'] = 'np',
		['noswap'] = 'np',
		['noagility'] = 'np',
		['nointelligence'] = 'np',
		['nostrength'] = 'np',
		['dropitems'] = 'np',
		['nopowerups'] = 'np',
		['norespawntimer'] = 'np',
		['alternatepicks'] = 'np',
		['allowduplicate'] = 'np',
		['reverseselect'] = 'np',
		['hardcore'] = 'np',
		['autobalance'] = 'np',
		-- ['tournament'] = 'np',
		['tournamentrules'] = 'np',
		-- ['balancedrandom'] = 'np',
		['gated'] = 'np'
	}
}

local _CreateSetting = {
	['simple'] = true,
	['region'] = 'any',
	['gamename'] = '',
	['teamsize'] = '5',
	['mapname'] = 'caldavar',
	['minpsr'] = '0',
	['maxpsr'] = '0',
	['server'] = 'remote_automatic',
	['spectators'] = '10',
	['referees'] = '2',
	['gamemode'] = {
		['allrandom'] = 'np',
		['banningdraft'] = 'np',
		['normal'] = 'required',
		['randomdraft'] = 'np',
		['blindban'] = 'np',
		['lockpick'] = 'np',
		['banningpick'] = 'np',
		['singledraft'] = 'np',
		['botmatch'] = 'np',
		['krosmode'] = 'np',
		['captainsmode'] = 'np',
		['solosame'] = 'np',
		['solodiff'] = 'np',
		['reborn'] = 'np',		
		['midwars_beta'] = 'np',
		['heroban'] = 'np'
	},
	['gameoption'] = {
		['noleaver'] = 'np',
		['rapidfire'] = 'np',
		['nostats'] = 'np',
		['verifiedonly'] = 'np',
		['casual'] = 'np',
		['private'] = 'np',
		['shuffleteams'] = 'np',
		-- ['devheroes'] = 'np',
		['norepick'] = 'np',
		['noswap'] = 'np',
		['noagility'] = 'np',
		['nointelligence'] = 'np',
		['nostrength'] = 'np',
		['dropitems'] = 'np',
		['nopowerups'] = 'np',
		['norespawntimer'] = 'np',
		['alternatepicks'] = 'np',
		['allowduplicate'] = 'np',
		['reverseselect'] = 'np',
		['hardcore'] = 'np',
		['autobalance'] = 'np',
		-- ['tournament'] = 'np',
		['tournamentrules'] = 'np',
		-- ['balancedrandom'] = 'np',
		['gated'] = 'np'
	}
}

function Player_Hosted_V2:Init()
	local widget = GetWidget('hosted_game_helper')

	widget:RegisterWatch('GameListAdd', function(_, ...) Player_Hosted_V2:OnGameListAdd(...) end)
	widget:RegisterWatch('GameListShow', function(_, ...) Player_Hosted_V2:OnGameListShow(...) end)
	widget:RegisterWatch('GameListHide', function(_, ...) Player_Hosted_V2:OnGameListHide(...) end)
	widget:RegisterWatch('GameListUpdate', function(_, ...) Player_Hosted_V2:OnGameListUpdate(...) end)
	widget:RegisterWatch('GameListClear', function(_, ...) Player_Hosted_V2:OnGameListClear(...) end)
	widget:RegisterWatch('ServerListAdd', function(_, ...) Player_Hosted_V2:OnServerListAdd(...) end)
	widget:RegisterWatch('ServerListUpdate', function(_, ...) Player_Hosted_V2:OnServerListUpdate(...) end)
	widget:RegisterWatch('ServerListClear', function(_, ...) Player_Hosted_V2:OnServerListClear(...) end)
	widget:RegisterWatch('UIUpdateRegion', function(_, ...) Player_Hosted_V2:OnUIUpdateRegion(...) end)
	widget:RegisterWatch('ConnectionStatus', function(_, ...) Player_Hosted_V2:OnConnectionStatus(...) end)
	widget:RegisterWatch('LocalServerAvailable', function(_, ...) Player_Hosted_V2:OnLocalServerAvailable(...) end)

	GetWidget('hosted_game_join'):SetEnabled(false)

	local availableMaps = GetMaps()
	local widget = GetWidget('hosted_game_filter_map')
	local widget2 = GetWidget('hosted_game_create_detail_map')
	local widget3 = GetWidget('hosted_game_create_simple_map')
	widget:ClearItems()
	widget2:ClearItems()
	widget3:ClearItems()
	widget:AddTemplateListItem('newui_combobox_item_template', 'any', 'label', 'main_lobby_filter_any', 'color', '#d7b6a2')
	for k,v in ipairs(availableMaps) do
		widget:AddTemplateListItem('newui_combobox_item_template', v, 'label', 'map_'..v, 'color', '#d7b6a2')
		widget2:AddTemplateListItem('newui_combobox_item_template', v, 'label', 'map_'..v, 'color', '#d7b6a2')
		widget3:AddTemplateListItem('newui_combobox_item_template', v, 'label', 'map_'..v, 'color', '#d7b6a2')
	end

	widget = GetWidget('hosted_game_create_detail_server')
	widget:ClearItems()
	widget:AddTemplateListItem('newui_combobox_item_template', 'remote_automatic', 'label', 'game_setup_automatic', 'color', '#d7b6a2')
	widget:AddTemplateListItem('newui_combobox_item_template', 'practice', 'label', 'game_setup_practice', 'color', '#d7b6a2')
	widget:AddTemplateListItem('newui_combobox_item_template', 'browse', 'label', 'game_setup_browse', 'color', '#d7b6a2')

	local filter_setting = GetDBEntry('newui_hosted_filter_setting', nil , false)
	if filter_setting ~= nil then _FilterSetting = filter_setting end

	local create_setting = GetDBEntry('newui_hosted_create_setting', nil , false)
	if create_setting ~= nil then _CreateSetting = create_setting end
end

function Player_Hosted_V2:OnGameListAdd(...)
	local ip = arg[1]
	local gameName = arg[2] 
	local ping = arg[3]
	local teamSize = arg[4]
	local gameMode = arg[5]
	local official = arg[7]
	local allInfo = arg[13]
	local noLeaver = AtoB(arg[15])
	local canJoin = AtoB(arg[16])
	local slots = arg[17] .. '/' .. arg[18]
	local mapName = arg[19]
	local minPsr = arg[20]
	local maxPsr = arg[21]
	local verifiedOnly = AtoB(arg[24])

	local game = {}
	game.visible = true
	game.ip = ip
	game.name = gameName
	game.ping = ping
	game.teamSize = teamSize
	game.gameMode = gameMode
	game.emptySlots = tonumber(arg[18]) - tonumber(arg[17])
	game.slots = slots
	game.map = mapName
	game.minPsr = minPsr
	game.maxPsr = maxPsr
	game.allInfo = allInfo
	game.canJoin = canJoin

	game.options = {}
	game.optionsTips = {}
	if verifiedOnly then 
		table.insert(game.options, '/ui/fe2/newui/res/icons/verified.tga')
		table.insert(game.optionsTips, 'verifiedonly')
	else
		table.insert(game.options, '/ui/fe2/newui/res/icons/basic.tga')
		table.insert(game.optionsTips, 'notverifiedonly')
	end

	if official == 'official' then 
		table.insert(game.options, '/ui/fe2/newui/res/icons/official.tga')
		table.insert(game.optionsTips, 'official')
	elseif official == 'official_2' then 
		table.insert(game.options, '/ui/fe2/newui/res/icons/official_2.tga')
		table.insert(game.optionsTips, 'official_2')
	end

	if noLeaver then
		table.insert(game.options, '/ui/fe2/newui/res/icons/noleaver.tga') 
		table.insert(game.optionsTips, 'noleaver')
	end

	for i=1,6 do
		if NotEmpty(arg[7+i-1]) and arg[7+i-1] ~= 'invis' and arg[7+i-1] ~= 'official' and arg[7+i-1] ~= 'official_2' then
			table.insert(game.options, '/ui/fe2/newui/res/icons/'..arg[7+i-1]..'.tga') 
			table.insert(game.optionsTips, arg[7+i-1])
		end
	end
	_gameList[ip] = game
end

function Player_Hosted_V2:OnGameListShow(ip)
	if _gameList[ip] ~= nil then _gameList[ip].visible = true end
end

function Player_Hosted_V2:OnGameListHide(ip)
	if _gameList[ip] ~= nil then _gameList[ip].visible = false end
end

function Player_Hosted_V2:OnGameListUpdate()
	Player_Hosted_V2:BuildSortedGameList()
	Player_Hosted_V2:UpdateGameList()
end

function Player_Hosted_V2:OnGameListClear()
	_gameList = {}
	Player_Hosted_V2:BuildSortedGameList()
	Player_Hosted_V2:UpdateGameList()
end

function Player_Hosted_V2:OnServerListAdd(...)
	local ip = arg[1]
	local serverName= arg[3]
	local ping = arg[4]
	local isOfficial = AtoB(arg[6])
	local region = arg[7]

	local server = {}
	server.ip = ip
	server.name = serverName
	server.ping = ping
	server.isOfficial = isOfficial
	server.region = region

	_serverList[ip] = server
end

function Player_Hosted_V2:OnServerListUpdate()
	Player_Hosted_V2:BuildSortedServerList()
	Player_Hosted_V2:UpdateServerList()
end

function Player_Hosted_V2:OnServerListClear()
	_serverList = {}
	Player_Hosted_V2:BuildSortedServerList()
	Player_Hosted_V2:UpdateServerList()
end

function Player_Hosted_V2:OnUIUpdateRegion()
	local widget = GetWidget('hosted_game_filter_region')
	local widget2 = GetWidget('hosted_game_create_serverlist_region')

	if (HoN_Region.regionTable) and (HoN_Region.regionTable[HoN_Region.activeRegion]) and (widget) then
		widget:ClearItems()
		widget2:ClearItems()
		widget:AddTemplateListItem('newui_combobox_item_template', 'any', 'label', 'main_lobby_filter_any', 'color', '#d7b6a2')
		widget2:AddTemplateListItem('newui_combobox_item_template', 'any', 'label', 'main_lobby_filter_any', 'color', '#d7b6a2')
		for index, serverCode in pairs(HoN_Region.regionTable[HoN_Region.activeRegion].serverCodes) do
			widget:AddTemplateListItem('newui_combobox_item_template', serverCode, 'label', 'mainlobby_label_custom_game_'..serverCode, 'color', '#d7b6a2')
			widget2:AddTemplateListItem('newui_combobox_item_template', serverCode, 'label', 'mainlobby_label_custom_game_'..serverCode, 'color', '#d7b6a2')
		end
	end
end

function Player_Hosted_V2:OnConnectionStatus(isConnected, serverName)
	local widget = GetWidget('hosted_game_create_detail_server')
	widget:ClearItems()

	if AtoB(isConnected) then
		Player_Hosted_V2:CloseServerList()
		widget:AddTemplateListItem('newui_combobox_item_template', 'selected', 'label', serverName, 'color', '#d7b6a2')
		_CreateSetting['server'] = 'selected'
	elseif _CreateSetting['server'] == 'selected' then
		_CreateSetting['server'] = 'remote_automatic'
	end

	widget:AddTemplateListItem('newui_combobox_item_template', 'remote_automatic', 'label', 'game_setup_automatic', 'color', '#d7b6a2')
	if _localServerAvailable then
		widget:AddTemplateListItem('newui_combobox_item_template', 'local', 'label', 'game_setup_local', 'color', '#d7b6a2')
		widget:AddTemplateListItem('newui_combobox_item_template', 'local_dedicated', 'label', 'game_setup_local_dedicated', 'color', '#d7b6a2')
	end
	widget:AddTemplateListItem('newui_combobox_item_template', 'practice', 'label', 'game_setup_practice', 'color', '#d7b6a2')
	widget:AddTemplateListItem('newui_combobox_item_template', 'browse', 'label', 'game_setup_browse', 'color', '#d7b6a2')

	widget:SetSelectedItemByValue(_CreateSetting['server'], false)
end

function Player_Hosted_V2:OnLocalServerAvailable(localServer)
	_localServerAvailable = AtoB(localServer)

	local widget = GetWidget('hosted_game_create_detail_server')
	widget:ClearItems()
	widget:AddTemplateListItem('newui_combobox_item_template', 'remote_automatic', 'label', 'game_setup_automatic', 'color', '#d7b6a2')
	if _localServerAvailable then
		widget:AddTemplateListItem('newui_combobox_item_template', 'local', 'label', 'game_setup_local', 'color', '#d7b6a2')
		widget:AddTemplateListItem('newui_combobox_item_template', 'local_dedicated', 'label', 'game_setup_local_dedicated', 'color', '#d7b6a2')
	end
	widget:AddTemplateListItem('newui_combobox_item_template', 'practice', 'label', 'game_setup_practice', 'color', '#d7b6a2')
	widget:AddTemplateListItem('newui_combobox_item_template', 'browse', 'label', 'game_setup_browse', 'color', '#d7b6a2')
end
----------------------------------------------------------------------------
function Player_Hosted_V2:Show()
	GetWidget('hosted_game'):FadeIn(150)
	GetWidget('hosted_game_filter'):SetVisible(false)
	GetWidget('hosted_game_create'):SetVisible(false)
	Player_Hosted_V2:UpdateFilter()
	Player_Hosted_V2:OnClickRefreshGameList()
end

function Player_Hosted_V2:Hide()
	GetWidget('hosted_game'):FadeOut(150)
end

------------------------------------------------------------------------
function Player_Hosted_V2:OnClickRefreshGameList()
	if _FilterSetting['prequery'] and NotEmpty(_FilterSetting['region']) and _FilterSetting['region'] ~= 'any' then
		interface:UICmd('GetGameList(\''.. _FilterSetting['region'] ..'\');')
	else
		interface:UICmd('GetGameList();')
	end
end

function Player_Hosted_V2:OnClickGame(index)
	local scrollWidget = GetWidget('hosted_game_gamelist_scroll')
	local startIndex = tonumber(scrollWidget:GetValue())
	local maxValue = tonumber(scrollWidget:UICmd("GetScrollbarMaxValue()"))
	if startIndex < 1 then startIndex = 1 end
	if startIndex > maxValue then startIndex = maxValue end

	local ip = _sortedGameList[startIndex+index-1]
	if ip ~= nil then
		_selectedGame = ip
		Player_Hosted_V2:UpdateGameList()
		GetWidget('hosted_game_join'):SetEnabled(true)
	end
end

function Player_Hosted_V2:OnClickJoin()
	if _selectedGame ~= nil then 
		GetWidget("hosted_game_join"):Sleep(550, function()
			GetWidget("hosted_game_join"):UICmd('Connect(\''.. _selectedGame ..'\');')
			Loading_V2:SetHostedLoadingInfo(_gameList[_selectedGame].map)
		end)
	end
end

function Player_Hosted_V2:OnClickApplyFilter()
	GetWidget('hosted_game_filter'):SetVisible(false)
	Player_Hosted_V2:CollectFilterSetting()
	Player_Hosted_V2:UpdateFilter()
end

function Player_Hosted_V2:OnClickCreateGameMode(gameMode)
	for k,v in pairs(_CreateSetting['gamemode']) do
		local widget = GetWidget('hosted_game_create_setting_'..k)

		if k == gameMode then
			_CreateSetting['gamemode'][k] = 'required'
			widget:SetButtonState(1)
		else
			_CreateSetting['gamemode'][k] = 'np'
			widget:SetButtonState(0)
		end
	end

	GetWidget('hosted_game_create_detail_map'):SetEnabled(true)
	GetWidget('hosted_game_create_simple_map'):SetEnabled(true)
	GetWidget('hosted_game_create_detail_teamsize'):SetEnabled(true)

	if _CreateSetting['gamemode']['midwars_beta'] == 'required' then
		GetWidget('hosted_game_create_detail_map'):SetSelectedItemByValue('midwars')
		GetWidget('hosted_game_create_detail_map'):SetEnabled(false)
		GetWidget('hosted_game_create_simple_map'):SetSelectedItemByValue('midwars')
		GetWidget('hosted_game_create_simple_map'):SetEnabled(false)
	elseif _CreateSetting['gamemode']['solosame'] == 'required' or _CreateSetting['gamemode']['solodiff'] == 'required' then
		GetWidget('hosted_game_create_detail_teamsize'):SetSelectedItemByValue('1')
		GetWidget('hosted_game_create_detail_teamsize'):SetEnabled(false)
		GetWidget('hosted_game_create_detail_map'):SetSelectedItemByValue('caldavar')
		GetWidget('hosted_game_create_detail_map'):SetEnabled(false)
		GetWidget('hosted_game_create_simple_map'):SetSelectedItemByValue('caldavar')
		GetWidget('hosted_game_create_simple_map'):SetEnabled(false)
	end

	local corePoolValid = Player_Hosted_V2:OptionValidForCorePool()
	GetWidget('hosted_game_create_setting_gated'):SetEnabled(corePoolValid)
end

function Player_Hosted_V2:OnClickCreateGameOption()
	local t = {'nostrength', 'nointelligence', 'noagility'}
	
	local state1Num = 0
	for i,v in ipairs(t) do
		local state = GetWidget('hosted_game_create_setting_'..v):GetButtonState()
		if state == 1 then 
			state1Num = state1Num + 1
		end
	end
	
	if state1Num >= 2 then 
		for i,v in ipairs(t) do
			local state = GetWidget('hosted_game_create_setting_'..v):GetButtonState()
			if state == 0 then 
				GetWidget('hosted_game_create_setting_'..v):SetEnabled(false)
			end
		end
	else
		for i,v in ipairs(t) do
			local state = GetWidget('hosted_game_create_setting_'..v):SetEnabled(true)
		end
	end
end

function Player_Hosted_V2:OnSelectMap()
	
	local value = GetWidget('hosted_game_create_detail_server'):GetValue()

	if not GetCvarBool('releaseStage_test') then
		if value ~= 'practice' then
			local mapSelectWidget = GetWidget('hosted_game_create_detail_map')
			if tostring(mapSelectWidget:GetValue()) == 'caldavar_reborn' or tostring(mapSelectWidget:GetValue()) == 'midwars_reborn' then
				mapSelectWidget:SetSelectedItemByValue('caldavar')
			end
		end
	end
end

function Player_Hosted_V2:OnSelectServerType()
	local value = GetWidget('hosted_game_create_detail_server'):GetValue()
	if value == 'browse' then
		GetWidget('hosted_game_create_advance_gamemode'):SetVisible(false)
		GetWidget('hosted_game_create_advance_options'):SetVisible(false)
		Player_Hosted_V2:OpenServerList()
	else
		if value == 'local' or value == 'local_dedicated' then
			interface:UICmd('Disconnect();')
		end

		if NotEmpty(value) then
			_CreateSetting['server'] = value
		end
	end

	if value == 'practice' and (not GetCvarBool('ui_canCreateGame') or GetCvarBool('releaseStage_stable')) then 
		for k,v in pairs(_CreateSetting['gamemode']) do
			if k ~= 'normal' and k ~= 'botmatch' then
				GetWidget('hosted_game_create_setting_'..k):SetEnabled(false)
			end
		end

		if _CreateSetting['gamemode']['normal'] ~= 'required' and _CreateSetting['gamemode']['botmatch'] ~= 'required' then
			Player_Hosted_V2:OnClickCreateGameMode('normal')
		end
	else
		for k,v in pairs(_CreateSetting['gamemode']) do
			GetWidget('hosted_game_create_setting_'..k):SetEnabled(true)
		end
	end
	
	--Reborn Settings
	if Player_Hosted_V2:OptionValidForCorePool() then
		GetWidget('hosted_game_create_setting_gated'):SetEnabled(true)
	else
		local mapWidget = GetWidget('hosted_game_create_detail_map')
		GetWidget('hosted_game_create_setting_gated'):SetEnabled(false)

		if mapWidget:GetValue() == 'caldavar_reborn' or mapWidget:GetValue() == 'midwars_reborn' then
			mapWidget:SetSelectedItemByValue('caldavar')
		end
	end
end

function Player_Hosted_V2:OnClickRefreshServerList()
	if NotEmpty(_CreateSetting['region']) and _CreateSetting['region'] ~= 'any' then
		interface:UICmd('GetServerList(\''.. _CreateSetting['region'] ..'\');')
	else
		interface:UICmd('GetServerList();')
	end
end

function Player_Hosted_V2:OnSelectServerRegion()
	_CreateSetting['region'] = GetWidget('hosted_game_create_serverlist_region'):GetValue()
	Player_Hosted_V2:OnClickRefreshServerList()
end

function Player_Hosted_V2:OnClickServer(index)
	local scrollWidget = GetWidget('hosted_game_create_serverlist_scroll')
	local startIndex = tonumber(scrollWidget:GetValue())
	local maxValue = tonumber(scrollWidget:UICmd("GetScrollbarMaxValue()"))
	if startIndex < 1 then startIndex = 1 end
	if startIndex > maxValue then startIndex = maxValue end

	local ip = _sortedServerList[startIndex+index-1]
	if ip ~= nil then
		_selectedServer = ip
		Player_Hosted_V2:UpdateServerList()
		GetWidget('hosted_game_create_serverlist_select_btn'):SetEnabled(true)
	end
end

function Player_Hosted_V2:OnClickSelectServer()
	if NotEmpty(_selectedServer) then
		interface:UICmd('Connect(\''.._selectedServer..'\', \'\', true); CancelServerList();')
		GetWidget('hosted_game_create_serverlist_mask'):SetVisible(true)
	end
end

function Player_Hosted_V2:OnClickCreateGame()
	Player_Hosted_V2:CloseCreate()
	Player_Hosted_V2:CreateGame()
end

function Player_Hosted_V2:OnClickSortGameList(type)
	if _gameListSortKey ~= nil and _gameListSortKey.type == type then 
		_gameListSortKey.value = 0 - _gameListSortKey.value
	else
		if _gameListSortKey == nil then 
			_gameListSortKey = {}
		end

		_gameListSortKey.type = type
		_gameListSortKey.value = 1
	end

	Player_Hosted_V2:BuildSortedGameList()
	Player_Hosted_V2:UpdateGameList()
end

function Player_Hosted_V2:OnClickSortServerList(type)
	if _serverListSortKey ~= nil and _serverListSortKey.type == type then 
		_serverListSortKey.value = 0 - _serverListSortKey.value
	else
		if _serverListSortKey == nil then 
			_serverListSortKey = {}
		end

		_serverListSortKey.type = type
		_serverListSortKey.value = 1
	end

	Player_Hosted_V2:BuildSortedServerList()
	Player_Hosted_V2:UpdateServerList()
end
----------------------------------------------------------------------
function Player_Hosted_V2:OptionValidForCorePool()

	local gameMode = ''

	for k,v in pairs(_CreateSetting['gamemode']) do
		if v == 'required' then
			gameMode = k
		end
	end

	for k,v in pairs(_corePoolDisabledMode) do
		if gameMode == k then
			return false
		end
	end
	
	local value = GetWidget('hosted_game_create_detail_server'):GetValue()

	if not GetCvarBool('releaseStage_test') and value ~= 'practice' then
		return false
	end

	return true
end

function Player_Hosted_V2:BuildSortedGameList()
	local function sort(a, b)
		local x = nil
		local y = nil
		if _gameListSortKey['type'] == 'ping' or _gameListSortKey['type'] == 'minPsr' or _gameListSortKey['type'] == 'maxPsr' then
			x = tonumber(_gameList[a][_gameListSortKey['type']])
			y = tonumber(_gameList[b][_gameListSortKey['type']])
		else
			x = _gameList[a][_gameListSortKey['type']]
			y = _gameList[b][_gameListSortKey['type']]
		end

		if _gameListSortKey['value'] > 0 then
			return x < y
		else
			return x > y
		end
	end

	_sortedGameList = {}

	for k,v in pairs(_gameList) do
		if v.visible and (not _FilterSetting['hiderestrictedgame'] or v.canJoin) then
			table.insert(_sortedGameList, k)
		end
	end

	if _gameListSortKey ~= nil then
		table.sort(_sortedGameList, sort)
	end

	if _selectedGame ~= nil then 
		local stillExist = false
		for i,v in ipairs(_sortedGameList) do
			if v == _selectedGame then 
				stillExist = true
				break
			end
		end

		GetWidget('hosted_game_join'):SetEnabled(stillExist)
	end
	
	local scrollWidget = GetWidget('hosted_game_gamelist_scroll')
	if #_sortedGameList <= GAMELIST_MAX_COUNT then
		scrollWidget:SetVisible(false)
		scrollWidget:SetMaxValue(1)
	else
		scrollWidget:SetVisible(true)
		local scrollMaxValue = #_sortedGameList - GAMELIST_MAX_COUNT + 1
		scrollWidget:SetMaxValue(scrollMaxValue)
	end

	scrollWidget:SetValue(1)
	Player_Hosted_V2:UpdateGameList()
end

function Player_Hosted_V2:UpdateGameList()
	local function SetGameInfo(index, info)
		if info == nil then
			GetWidget('hosted_game_gamelist_item_'..index):SetVisible(false)
		else
			GetWidget('hosted_game_gamelist_item_'..index):SetVisible(true)

			Common_V2:SetLongTextLabel('hosted_game_gamelist_item_'..index..'_gamename', info.name)
			GetWidget('hosted_game_gamelist_item_'..index..'_teamsize'):SetText(info.teamSize)
			GetWidget('hosted_game_gamelist_item_'..index..'_ping'):SetText(info.ping)
			GetWidget('hosted_game_gamelist_item_'..index..'_slots'):SetText(info.slots)
			GetWidget('hosted_game_gamelist_item_'..index..'_minpsr'):SetText(info.minPsr)
			GetWidget('hosted_game_gamelist_item_'..index..'_maxpsr'):SetText(info.maxPsr)
			GetWidget('hosted_game_gamelist_item_'..index..'_map_text'):SetText(Translate('mainlobby_gamelist_'..info.map..'_title'))
			GetWidget('hosted_game_gamelist_item_'..index..'_map_icon'):SetTexture('/maps/'..info.map..'/icon.tga')
			GetWidget('hosted_game_gamelist_item_'..index..'_gamemode_text'):SetText(Translate('mainlobby_gamelist_'..info.gameMode..'_title'))
			GetWidget('hosted_game_gamelist_item_'..index..'_gamemode_icon'):SetTexture('/ui/fe2/newui/res/icons/'..info.gameMode..'.tga')
			GetWidget('hosted_game_gamelist_item_'..index..'_selected'):SetVisible(info.ip == _selectedGame)
			GetWidget('hosted_game_gamelist_item_'..index..'_invalid'):SetVisible(not info.canJoin)

			GetWidget('hosted_game_gamelist_item_'..index..'_map'):SetCallback('onmouseover', function() 
				Common_V2:ShowGenericTip(true, 'mainlobby_gamelist_'..info.map..'_title', 'mainlobby_gamelist_'..info.map..'_desc')
			end)

			GetWidget('hosted_game_gamelist_item_'..index..'_gamemode'):SetCallback('onmouseover', function() 
				Common_V2:ShowGenericTip(true, 'mainlobby_gamelist_'..info.gameMode..'_title', 'mainlobby_gamelist_'..info.gameMode..'_desc')
			end)

			for i=1,GAMELIST_OPTIONS_MAX_COUNT do
				if NotEmpty(info.options[i]) then 
					GetWidget('hosted_game_gamelist_item_'..index..'_options'..i):SetVisible(true)
					GetWidget('hosted_game_gamelist_item_'..index..'_options'..i):SetTexture(info.options[i])

					GetWidget('hosted_game_gamelist_item_'..index..'_options'..i):SetCallback('onmouseover', function() 
						Common_V2:ShowGenericTip(true, 'mainlobby_gamelist_'..info.optionsTips[i]..'_title', 'mainlobby_gamelist_'..info.optionsTips[i]..'_desc')
					end)

					GetWidget('hosted_game_gamelist_item_'..index..'_options'..i):SetCallback('onmouseout', function() 
						Common_V2:ShowGenericTip(false)
					end)
				else
					GetWidget('hosted_game_gamelist_item_'..index..'_options'..i):SetVisible(false)
				end
			end
		end
	end

	local scrollWidget = GetWidget('hosted_game_gamelist_scroll')
	local startIndex = tonumber(scrollWidget:GetValue())
	local maxValue = tonumber(scrollWidget:UICmd("GetScrollbarMaxValue()"))
	if startIndex < 1 then startIndex = 1 end
	if startIndex > maxValue then startIndex = maxValue end

	for i=1,GAMELIST_MAX_COUNT do
		local ip = _sortedGameList[startIndex+i-1]
		if ip == nil then
			SetGameInfo(i, nil)
		else
			SetGameInfo(i, _gameList[ip])
		end
	end

	Player_Hosted_V2:UpdateGameTitle()
end

function Player_Hosted_V2:UpdateGameTitle()
	for i=1,9 do
		GetWidget('hosted_game_title_'..i..'_uparrow'):SetVisible(false)
		GetWidget('hosted_game_title_'..i..'_downarrow'):SetVisible(false)
	end

	if _gameListSortKey == nil then return end

	local id = 0 

	if _gameListSortKey['type'] == 'name' then
		id = 1 
	elseif _gameListSortKey['type'] == 'map' then
		id = 2
	elseif _gameListSortKey['type'] == 'gameMode' then
		id = 3
	elseif _gameListSortKey['type'] == 'teamSize' then
		id = 4
	elseif _gameListSortKey['type'] == 'ping' then
		id = 5
	elseif _gameListSortKey['type'] == 'emptySlots' then
		id = 6
	elseif _gameListSortKey['type'] == 'minPsr' then
		id = 7
	elseif _gameListSortKey['type'] == 'maxPsr' then
		id = 8
	end

	if id > 0 then 
		GetWidget('hosted_game_title_'..id..'_uparrow'):SetVisible(_gameListSortKey.value < 0)
		GetWidget('hosted_game_title_'..id..'_downarrow'):SetVisible(_gameListSortKey.value > 0)
	end
end

function Player_Hosted_V2:OpenFilter()
	GetWidget('hosted_game_filter'):FadeIn(150)
	GetWidget('hosted_game_filter_advance_gamemode'):SetVisible(false)
	GetWidget('hosted_game_filter_advance_options'):SetVisible(false)
	
	GetWidget('hosted_game_filter_gamename'):SetInputLine(_FilterSetting['gamename'])
	GetWidget('hosted_game_filter_teamsize'):SetSelectedItemByValue(_FilterSetting['teamsize'])
	GetWidget('hosted_game_filter_map'):SetSelectedItemByValue(_FilterSetting['mapname'])
	GetWidget('hosted_game_filter_minpsr'):SetSelectedItemByValue(_FilterSetting['minpsr'])
	GetWidget('hosted_game_filter_maxpsr'):SetSelectedItemByValue(_FilterSetting['maxpsr'])
	GetWidget('hosted_game_filter_region'):SetSelectedItemByValue(_FilterSetting['region'])
	GetWidget('hosted_game_filter_ping'):SetSelectedItemByValue(_FilterSetting['ping'])

	if  _FilterSetting['prequery'] then 
		GetWidget('hosted_game_filter_prequery'):SetButtonState(1)
	else
		GetWidget('hosted_game_filter_prequery'):SetButtonState(0)
	end

	if  _FilterSetting['hiderestrictedgame'] then 
		GetWidget('hosted_game_filter_hiderestrictedgame'):SetButtonState(1)
	else
		GetWidget('hosted_game_filter_hiderestrictedgame'):SetButtonState(0)
	end

	for k,v in pairs(_FilterSetting['gamemode']) do
		local widget = GetWidget('hosted_game_filter_gamemode_'..k)
		if v == 'required' then
			widget:SetButtonState(0) 
		else
			widget:SetButtonState(1) 
		end
	end

	for k,v in pairs(_FilterSetting['gameoption']) do
		local widget = GetWidget('hosted_game_filter_option_'..k)
		if v == 'required' then
			widget:SetButtonState(1) 
		elseif v == 'excluded' then
			widget:SetButtonState(2) 
		else
			widget:SetButtonState(0)
		end
	end
end

function Player_Hosted_V2:CollectFilterSetting()
	_FilterSetting['gamename'] = GetWidget('hosted_game_filter_gamename'):GetValue()

	_FilterSetting['teamsize'] = GetWidget('hosted_game_filter_teamsize'):GetValue()
	_FilterSetting['mapname'] = GetWidget('hosted_game_filter_map'):GetValue()
	_FilterSetting['minpsr'] = GetWidget('hosted_game_filter_minpsr'):GetValue()
	_FilterSetting['maxpsr'] = GetWidget('hosted_game_filter_maxpsr'):GetValue()
	_FilterSetting['region'] = GetWidget('hosted_game_filter_region'):GetValue()
	_FilterSetting['ping'] = GetWidget('hosted_game_filter_ping'):GetValue()

	local state = GetWidget('hosted_game_filter_prequery'):GetButtonState()
	_FilterSetting['prequery'] = state == 1
	state = GetWidget('hosted_game_filter_hiderestrictedgame'):GetButtonState()
	_FilterSetting['hiderestrictedgame'] = state == 1

	for k,v in pairs(_FilterSetting['gamemode']) do
		local widget = GetWidget('hosted_game_filter_gamemode_'..k)
		local state = widget:GetButtonState()
		if state == 0 then 
			_FilterSetting['gamemode'][k] = 'required'
		else
			_FilterSetting['gamemode'][k] = 'excluded'
		end
	end

	for k,v in pairs(_FilterSetting['gameoption']) do
		local widget = GetWidget('hosted_game_filter_option_'..k)
		local state = widget:GetButtonState()

		if state == 0 then
			_FilterSetting['gameoption'][k] = 'np'
		elseif state == 1 then
			_FilterSetting['gameoption'][k] = 'required'
		else
			_FilterSetting['gameoption'][k] = 'excluded'
		end
	end

	GetDBEntry('newui_hosted_filter_setting', _FilterSetting , true)
end

function Player_Hosted_V2:UpdateFilter()
	local gameName = _FilterSetting['gamename']

	local filterStr = ''
	if _FilterSetting['gameoption']['nostats'] == 'required' then
		filterStr = 'servertype:2'
	elseif _FilterSetting['gameoption']['nostats'] == 'excluded' then
		filterStr = 'servertype:1'
	else
		filterStr = 'servertype:-1'
	end

	if NotEmpty(_FilterSetting['mapname']) and _FilterSetting['mapname'] ~= 'any' then
		filterStr = filterStr .. ' ' .. 'map:' .. _FilterSetting['mapname']
	end

	if NotEmpty(_FilterSetting['region']) and _FilterSetting['region'] ~= 'any' then
		filterStr = filterStr .. ' ' .. 'region:' .. _FilterSetting['region']
	end

	if NotEmpty(_FilterSetting['teamsize']) and _FilterSetting['teamsize'] ~= '0' then
		filterStr = filterStr .. ' ' .. 'teamsize:' .. _FilterSetting['teamsize']
	end
	
	if NotEmpty(_FilterSetting['minpsr']) and _FilterSetting['minpsr'] ~= '0' then
		filterStr = filterStr .. ' ' .. 'minpsr:' .. _FilterSetting['minpsr']
	end

	if NotEmpty(_FilterSetting['maxpsr']) and _FilterSetting['maxpsr'] ~= '0' then
		filterStr = filterStr .. ' ' .. 'maxpsr:' .. _FilterSetting['maxpsr']
	end

	if NotEmpty(_FilterSetting['ping']) and _FilterSetting['ping'] ~= '0' then
		filterStr = filterStr .. ' ' .. 'ping:' .. _FilterSetting['ping']
	end

	local gameModeStr = ''
	for k,v in pairs(_FilterSetting['gamemode']) do
		if v == 'required' then 
			gameModeStr = gameModeStr .. k ..','

			if k == 'normal' then 
				gameModeStr = gameModeStr..'soccerpick,forcepick,'
			end
		end
	end

	if Empty(gameModeStr) then gameModeStr = 'none' end
	filterStr = filterStr .. ' ' .. 'mode:' .. gameModeStr

	for k,v in pairs(_FilterSetting['gameoption']) do
		filterStr = filterStr .. ' ' .. k .. ':' .. v
	end
	
	interface:UICmd([[FilterGameList(']] .. EscapeString(gameName) .. [[', ']] .. filterStr .. [[')]])		
end

function Player_Hosted_V2:SwitchCreatePanel(simple)
	Player_Hosted_V2:CollectCreateSetting()
	_CreateSetting['simple'] = simple
	Player_Hosted_V2:OpenCreate()
end

function Player_Hosted_V2:OpenCreate()
	GetWidget('hosted_game_create'):SetVisible(true)
	GetWidget('hosted_game_create_simple'):SetVisible(false)
	GetWidget('hosted_game_create_detail'):SetVisible(false)

	_CreateSetting['server'] = GetCvarBool('ui_canCreateGame') and 'remote_automatic' or 'practice'

	if _CreateSetting['simple'] then
		GetWidget('hosted_game_create_simple'):FadeIn(150)
		GetWidget('hosted_game_create_detail'):FadeOut(150)

		GetWidget('hosted_game_create_simple_gamename'):SetInputLine(_CreateSetting['gamename'])
		GetWidget('hosted_game_create_simple_map'):SetSelectedItemByValue(_CreateSetting['mapname'])
	else
		GetWidget('hosted_game_create_simple'):FadeOut(150)
		GetWidget('hosted_game_create_detail'):FadeIn(150)

		GetWidget('hosted_game_create_advance_options'):SetVisible(false)
		GetWidget('hosted_game_create_advance_gamemode'):SetVisible(false)

		GetWidget('hosted_game_create_detail_gamename'):SetInputLine(_CreateSetting['gamename'])
		GetWidget('hosted_game_create_detail_teamsize'):SetSelectedItemByValue(_CreateSetting['teamsize'])
		GetWidget('hosted_game_create_detail_map'):SetSelectedItemByValue(_CreateSetting['mapname'])
		GetWidget('hosted_game_create_detail_minpsr'):SetSelectedItemByValue(_CreateSetting['minpsr'])
		GetWidget('hosted_game_create_detail_maxpsr'):SetSelectedItemByValue(_CreateSetting['maxpsr'])
		GetWidget('hosted_game_create_detail_server'):SetSelectedItemByValue(_CreateSetting['server'])
		GetWidget('hosted_game_create_detail_server'):SetEnabled(GetCvarBool('ui_canCreateGame'))
		GetWidget('hosted_game_create_detail_spectators'):SetSelectedItemByValue(_CreateSetting['spectators'])
		GetWidget('hosted_game_create_detail_referees'):SetSelectedItemByValue(_CreateSetting['referees'])

		for k,v in pairs(_CreateSetting['gamemode']) do
			local widget = GetWidget('hosted_game_create_setting_'..k)
			if v == 'np' then
				widget:SetButtonState(0)
			else
				widget:SetButtonState(1)
			end
		end

		for k,v in pairs(_CreateSetting['gameoption']) do
			local widget = GetWidget('hosted_game_create_setting_'..k)
			
			if widget ~= nil then
				if v == 'np' then
					widget:SetButtonState(0)
				else
					widget:SetButtonState(1)
				end
			end
		end
		Player_Hosted_V2:OnClickCreateGameOption()
	end
end

function Player_Hosted_V2:CloseCreate(disconnect)
	GetWidget('hosted_game_create'):FadeOut(150)
	Player_Hosted_V2:CollectCreateSetting()

	if disconnect then 
		interface:UICmd('Disconnect();')
	end
end

function Player_Hosted_V2:CollectCreateSetting()

	if _CreateSetting['simple'] then
		_CreateSetting['gamename'] = GetWidget('hosted_game_create_simple_gamename'):GetValue()
		_CreateSetting['mapname'] = GetWidget('hosted_game_create_simple_map'):GetValue()
	else
		_CreateSetting['gamename'] = GetWidget('hosted_game_create_detail_gamename'):GetValue()
		_CreateSetting['teamsize'] = GetWidget('hosted_game_create_detail_teamsize'):GetValue()
		_CreateSetting['mapname'] = GetWidget('hosted_game_create_detail_map'):GetValue()
		_CreateSetting['minpsr'] = GetWidget('hosted_game_create_detail_minpsr'):GetValue()
		_CreateSetting['maxpsr'] = GetWidget('hosted_game_create_detail_maxpsr'):GetValue()
		_CreateSetting['spectators'] = GetWidget('hosted_game_create_detail_spectators'):GetValue()
		_CreateSetting['referees'] = GetWidget('hosted_game_create_detail_referees'):GetValue()

		for k,v in pairs(_CreateSetting['gameoption']) do
			local widget = GetWidget('hosted_game_create_setting_'..k)
			
			if widget ~= nil then
				local state = widget:GetButtonState()
				local enabled = widget:IsEnabled()

				if state == 0 or not enabled then
					_CreateSetting['gameoption'][k] = 'np'
				else
					_CreateSetting['gameoption'][k] = 'required'
				end
			end
		end
	end

	GetDBEntry('newui_hosted_create_setting', _CreateSetting , true)
end

function Player_Hosted_V2:OpenServerList()
	_selectedServer = nil
	GetWidget('hosted_game_create_serverlist'):FadeIn(150)
	GetWidget('hosted_game_create_serverlist_select_btn'):SetEnabled(false)
	GetWidget('hosted_game_create_serverlist_region'):SetSelectedItemByValue(_CreateSetting['region'])
	GetWidget('hosted_game_create_serverlist_mask'):SetVisible(false)
	for i=1,SERVERLIST_MAX_COUNT do
		GetWidget('hosted_game_serverlist_item_'..i):SetVisible(false)
	end

	Player_Hosted_V2:OnClickRefreshServerList()
end

function Player_Hosted_V2:CloseServerList()
	Player_Hosted_V2:OnClickRefreshGameList()
	GetWidget('hosted_game_create_serverlist'):FadeOut(150)
	GetWidget('hosted_game_create_detail_server'):SetSelectedItemByValue(_CreateSetting['server'], false)
end

function Player_Hosted_V2:BuildSortedServerList()
	local function sort(a, b)
		local x = nil
		local y = nil
		if _serverListSortKey['type'] == 'ping' then
			x = tonumber(_serverList[a][_serverListSortKey['type']])
			y = tonumber(_serverList[b][_serverListSortKey['type']])
		else
			x = _serverList[a][_serverListSortKey['type']]
			y = _serverList[b][_serverListSortKey['type']]
		end

		if _serverListSortKey['value'] > 0 then
			return x < y
		else
			return x > y
		end
	end

	_sortedServerList = {}

	for k,v in pairs(_serverList) do
		table.insert(_sortedServerList, k)
	end

	if _serverListSortKey ~= nil then
		table.sort(_sortedServerList, sort)
	end

	if _selectedServer ~= nil then 
		local stillExist = false
		for i,v in ipairs(_sortedServerList) do
			if v == _selectedServer then 
				stillExist = true
				break
			end
		end

		GetWidget('hosted_game_create_serverlist_select_btn'):SetEnabled(stillExist)
	end
	
	local scrollWidget = GetWidget('hosted_game_create_serverlist_scroll')
	if #_sortedServerList <= SERVERLIST_MAX_COUNT then
		scrollWidget:SetVisible(false)
		scrollWidget:SetMaxValue(1)
	else
		scrollWidget:SetVisible(true)
		local scrollMaxValue = #_sortedServerList - SERVERLIST_MAX_COUNT + 1
		scrollWidget:SetMaxValue(scrollMaxValue)
	end

	scrollWidget:SetValue(1)
	Player_Hosted_V2:UpdateServerList()
end

function Player_Hosted_V2:UpdateServerList()
	local function SetServerInfo(index, info)
		if info == nil then
			GetWidget('hosted_game_serverlist_item_'..index):SetVisible(false)
		else
			GetWidget('hosted_game_serverlist_item_'..index):SetVisible(true)

			GetWidget('hosted_game_serverlist_item_'..index..'_official'):SetVisible(info.isOfficial)
			GetWidget('hosted_game_serverlist_item_'..index..'_region'):SetText(info.region)
			GetWidget('hosted_game_serverlist_item_'..index..'_ping'):SetText(info.ping)
			GetWidget('hosted_game_serverlist_item_'..index..'_name'):SetText(info.name)
			GetWidget('hosted_game_serverlist_item_'..index..'_selected'):SetVisible(info.ip == _selectedServer)
		end
	end

	local scrollWidget = GetWidget('hosted_game_create_serverlist_scroll')
	local startIndex = tonumber(scrollWidget:GetValue())
	local maxValue = tonumber(scrollWidget:UICmd("GetScrollbarMaxValue()"))
	if startIndex < 1 then startIndex = 1 end
	if startIndex > maxValue then startIndex = maxValue end

	for i=1,SERVERLIST_MAX_COUNT do
		local ip = _sortedServerList[startIndex+i-1]

		if ip == nil then
			SetServerInfo(i, nil)
		else
			SetServerInfo(i, _serverList[ip])
		end
	end
end

function Player_Hosted_V2:CreateGame()
	local isLocalGame = _CreateSetting['server'] ~= 'selected'
	local gameMode = ''
	local gameName = _CreateSetting['gamename']
	local serverType = _CreateSetting['server']

	for k,v in pairs(_CreateSetting['gamemode']) do
		if v == 'required' then
			gameMode = k
		end
	end

	if Empty(gameMode) then gameMode = 'normal' end
	if Empty(gameName) then gameName = GetAccountName() or 'Nameless' end

	local filterStr = ''
	filterStr = filterStr .. ' ' .. 'map:' .. _CreateSetting['mapname']
	filterStr = filterStr .. ' ' .. 'teamsize:' .. _CreateSetting['teamsize']
	filterStr = filterStr .. ' ' .. 'minpsr:' .. _CreateSetting['minpsr']
	filterStr = filterStr .. ' ' .. 'maxpsr:' .. _CreateSetting['maxpsr']
	filterStr = filterStr .. ' ' .. 'spectators:' .. _CreateSetting['spectators']
	filterStr = filterStr .. ' ' .. 'referees:' .. _CreateSetting['referees']

	for k,v in pairs(_CreateSetting['gamemode']) do
		if v == 'required' then 
			filterStr = filterStr .. ' ' .. 'mode:' .. k
			break
		end
	end

	for k,v in pairs(_CreateSetting['gameoption']) do
		if v == 'required' then 
			filterStr = filterStr .. ' ' .. k .. ':true'
		else
			filterStr = filterStr .. ' ' .. k .. ':false'
		end
	end
	Echo('^yStart Game !!!!!!!!!!!!!!!')
	Echo(filterStr)	

	Loading_V2:SetHostedLoadingInfo(_CreateSetting['mapname'])

	if (isLocalGame) then
		GetWidget('hosted_game_helper'):Sleep(400, function() interface:UICmd([[CancelServerList; StartGame(']]..serverType..[[', ']] .. EscapeString(gameName) .. [[', ']] .. filterStr .. [[')]]) end)
	else
		GetWidget('hosted_game_helper'):Sleep(400, function() interface:UICmd([[CancelServerList; SendCreateGameRequest(']] .. EscapeString(gameName) .. [[', ']] .. filterStr .. [[')]]) end)
	end
end