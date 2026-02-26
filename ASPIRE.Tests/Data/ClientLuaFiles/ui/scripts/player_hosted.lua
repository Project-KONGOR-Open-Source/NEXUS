----------------------------------------------------------
--	Name: 		Player Hosted Games Script	            --
--  Copyright 2015 Frostburn Studios					--
----------------------------------------------------------

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()
RegisterScript2('PlayerHosted', '32')

-- To do, transfer from search to create?, fixing things that dont work for the other.
-- map and mode restrictions

PlayerHosted = {}

PlayerHosted.availableMaps = GetMaps()

PlayerHosted.options = GetDBEntry('phGamesOptions9', nil, true, false, true) or {
	['search'] = {
		['gamename'] = '',
		['teamsize'] = '-1',
		['mapname'] = 'caldavar',
		['minpsr'] = '0',
		['maxpsr'] = '0',
		['server'] = 'remote_automatic',
		['spectators'] = '10',
		['referees'] = '2',
		['region'] = '',
		['ping'] = '0',
		['gamemode'] = {
			['allrandom'] = 'allrandom:required',
			['banningdraft'] = 'banningdraft:excluded',
			['normal'] = 'normal:required',
			['randomdraft'] = 'randomdraft:excluded',
			['blindban'] = 'blindban:required',
			['lockpick'] = 'lockpick:excluded',
			['banningpick'] = 'banningpick:required',
			['singledraft'] = 'singledraft:required',
			['botmatch'] = 'botmatch:excluded',
			['krosmode'] = 'krosmode:excluded',
			['captainsmode'] = 'captainsmode:excluded',
			['solosame'] = 'solosame:np',
			['solodiff'] = 'solodiff:np',
			['heroban'] = 'heroban:required',
			['reborn'] = 'reborn:required',
		},
		['gameoption'] = {
			-- filter
			['full'] = 'full:np',
			-- game options
			['noleaver'] = 'noleaver:np',
			['rapidfire'] = 'rapidfire:np',
			['nostats'] = 'nostats:np',
			['verifiedonly'] = 'verifiedonly:np',
			['casual'] = 'casual:np',
			['private'] = 'private:np',
			-- advanced
			['shuffleteams'] = 'shuffleteams:np',
			['devheroes'] = 'devheroes:np',
			['norepick'] = 'norepick:np',
			['noswap'] = 'noswap:np',
			['noagility'] = 'noagility:np',
			['nointelligence'] = 'nointelligence:np',
			['nostrength'] = 'nostrength:np',
			['dropitems'] = 'dropitems:np',
			['nopowerups'] = 'nopowerups:np',
			['norespawntimer'] = 'norespawntimer:np',
			['alternatepicks'] = 'alternatepicks:np',
			['allowduplicate'] = 'allowduplicate:np',
			['reverseselect'] = 'reverseselect:np',
			['hardcore'] = 'hardcore:np',
			['autobalance'] = 'autobalance:np',
			['tournament'] = 'tournament:np',
			['tournamentrules'] = 'tournamentrules:np',
			-- ['balancedrandom'] = 'balancedrandom:np',
			['gated'] = 'gated:np'
		},
	},
	['create'] = {
		['gamename'] = '',
		['teamsize'] = '5',
		['mapname'] = 'caldavar',
		['minpsr'] = '0',
		['maxpsr'] = '0',
		['server'] = 'remote_automatic',
		['spectators'] = '10',
		['referees'] = '2',
		['region'] = '',
		['ping'] = '0',
		['gamemode'] = {
			['allrandom'] = 'allrandom:np',
			['banningdraft'] = 'banningdraft:np',
			['normal'] = 'normal:required',
			['randomdraft'] = 'randomdraft:np',
			['blindban'] = 'blindban:np',
			['lockpick'] = 'lockpick:np',
			['banningpick'] = 'banningpick:np',
			['singledraft'] = 'singledraft:np',
			['botmatch'] = 'botmatch:np',
			['krosmode'] = 'krosmode:np',
			['captainsmode'] = 'captainsmode:np',
			['solosame'] = 'solosame:np',
			['solodiff'] = 'solodiff:np',
			['heroban'] = 'heroban:np',
		},
		['gameoption'] = {
			-- filter
			['full'] = 'full:np',
			-- game options
			['noleaver'] = 'noleaver:np',
			['rapidfire'] = 'rapidfire:np',
			['nostats'] = 'nostats:np',
			['verifiedonly'] = 'verifiedonly:np',
			['casual'] = 'casual:np',
			['private'] = 'private:np',
			-- advanced
			['shuffleteams'] = 'shuffleteams:np',
			['devheroes'] = 'devheroes:np',
			['norepick'] = 'norepick:np',
			['noswap'] = 'noswap:np',
			['noagility'] = 'noagility:np',
			['nointelligence'] = 'nointelligence:np',
			['nostrength'] = 'nostrength:np',
			['dropitems'] = 'dropitems:np',
			['nopowerups'] = 'nopowerups:np',
			['norespawntimer'] = 'norespawntimer:np',
			['alternatepicks'] = 'alternatepicks:np',
			['allowduplicate'] = 'allowduplicate:np',
			['reverseselect'] = 'reverseselect:np',
			['hardcore'] = 'hardcore:np',
			['autobalance'] = 'autobalance:np',
			['tournament'] = 'tournament:np',
			['tournamentrules'] = 'tournamentrules:np',
			-- ['balancedrandom'] = 'balancedrandom:np',
			['gated'] = 'gated:np'
		},
	},
	['activetab'] = 'search',
}

PlayerHosted.mapsPerMode = PlayerHosted.mapsPerMode or {
	botmatch		= {
		caldavar		= true,
		grimmscrossing	= true,
		midwars			= true,
	}
}

local function SetOptionEnabled(option, enable)
	GetWidget('hosted_filter_gameoption_'..option):SetEnabled(enable)

	if not enable then
		PlayerHosted.options.create.gameoption[option] = option..'np'
	end
end

local function ValidateCreateGameModes(option)

	local hasAGameMode = false
	local option = option

	if (option) then
		hasAGameMode = true
	else
		for i, v in pairs(PlayerHosted.options.create.gamemode) do
			if (v == i .. ':required') then
				hasAGameMode = true
				option = v
			end
		end
	end

	if (hasAGameMode) then
		for i, _ in pairs(PlayerHosted.options.create.gamemode) do
			if (i ~= option) then
				PlayerHosted.options.create.gamemode[i] = i .. ':np'
			end
		end
	else
		PlayerHosted.options.create.gamemode.normal = 'normal:required'
		option = "normal"
	end

	if option == 'solosame' or option == 'solodiff' then
		PlayerHosted.options.create.teamsize = 1
		PlayerHosted.options.create.mapname = 'caldavar'
		GetWidget('creategame_teamsize_combobox'):SetEnabled(false)
		SetOptionEnabled('balancedrandom', false)
		SetOptionEnabled('shuffleteams', false)
		SetOptionEnabled('norepick', false)
		SetOptionEnabled('noswap', false)
		SetOptionEnabled('norespawntimer', false)
		SetOptionEnabled('alternatepicks', false)
		SetOptionEnabled('allowduplicate', false)
		SetOptionEnabled('reverseselect', false)
		SetOptionEnabled('autobalance', false)
	elseif option == 'midwars_beta' then
		PlayerHosted.options.create.mapname = 'midwars'
		SetOptionEnabled('balancedrandom', false)
		SetOptionEnabled('shuffleteams', false)
		SetOptionEnabled('norepick', false)
		SetOptionEnabled('noswap', false)
		SetOptionEnabled('norespawntimer', false)
		SetOptionEnabled('alternatepicks', false)
		SetOptionEnabled('allowduplicate', false)
		SetOptionEnabled('reverseselect', false)
		SetOptionEnabled('autobalance', false)		
	else
		GetWidget('creategame_teamsize_combobox'):SetEnabled(true)
		SetOptionEnabled('balancedrandom', true)
		SetOptionEnabled('shuffleteams', true)
		SetOptionEnabled('norepick', true)
		SetOptionEnabled('noswap', true)
		SetOptionEnabled('norespawntimer', true)
		SetOptionEnabled('alternatepicks', true)
		SetOptionEnabled('allowduplicate', true)
		SetOptionEnabled('reverseselect', true)
		SetOptionEnabled('autobalance', true)
	end
	
	Set('_game_mode', option, 'string')

end

local function ValidateCreateGameOptions(option)
	--"hosted_filter_gameoption_"..option
	-- if two of these are set, disable the 3rd
	if (option == "noagility" or option == "nointelligence" or option == "nostrength") then
		local numTrue, falseTable = 0, {}

		if (PlayerHosted.options.create.gameoption.noagility == 'noagility:required') then numTrue = numTrue + 1 else table.insert(falseTable, 'noagility') end
		if (PlayerHosted.options.create.gameoption.nointelligence == 'nointelligence:required') then numTrue = numTrue + 1 else table.insert(falseTable, 'nointelligence') end
		if (PlayerHosted.options.create.gameoption.nostrength == 'nostrength:required') then numTrue = numTrue + 1 else table.insert(falseTable, 'nostrength') end

		if (numTrue == 2 and falseTable[1]) then 	-- we need to disable the option that's still false
			PlayerHosted.options.create.gameoption[falseTable[1]] = falseTable[1] .. ':excluded'
			GetWidget("hosted_filter_gameoption_"..falseTable[1]):SetButtonState(2)
		else 	-- enable any disabled options
			for _,v in pairs(falseTable) do
				PlayerHosted.options.create.gameoption[v] = v .. ':np'
			end
		end
	end
end

local lastBotMatch = nil

function PlayerHosted.UpdateMapSelection()
	if PlayerHosted.updatingMaps then
		return
	end
	PlayerHosted.updatingMaps = true

	local mapSelectionCreate = PlayerHosted.options['create']['mapname']
	local mapSelectionSearch = PlayerHosted.options['search']['mapname']

	local isBotMatch = (PlayerHosted.options[PlayerHosted.options.activetab]['gamemode']['botmatch'] == 'botmatch:required')

	local validMap = false

	GetWidget('phg_mapname_combobox'):ClearItems()
	GetWidget('phg_mapname_combobox_2'):ClearItems()
	GetWidget('phg_mapname_combobox_2'):AddTemplateListItem('Ncombobox_item', '', 'label', 'main_lobby_filter_any')

	if isBotMatch and PlayerHosted.options.activetab == 'create' then
	-- if isBotMatch and ((not lastBotMatch) or lastBotMatch == nil) then
		for k,v in ipairs(PlayerHosted.availableMaps) do
			if PlayerHosted.mapsPerMode['botmatch'][v] then
				GetWidget('phg_mapname_combobox'):AddTemplateListItem('Ncombobox_item', v, 'label', Translate('map_'..v))
				if v == mapSelectionCreate then
					validMap = true
				end
			end
			GetWidget('phg_mapname_combobox_2'):AddTemplateListItem('Ncombobox_item', v, 'label', Translate('map_'..v))
		end
	else
	-- elseif (not isBotMatch) and (lastBotMatch or lastBotMatch == nil) then
		for k,v in ipairs(PlayerHosted.availableMaps) do
			GetWidget('phg_mapname_combobox'):AddTemplateListItem('Ncombobox_item', v, 'label', Translate('map_'..v))
			GetWidget('phg_mapname_combobox_2'):AddTemplateListItem('Ncombobox_item', v, 'label', Translate('map_'..v))

			if PlayerHosted.options.activetab == 'search' then
				if v == mapSelectionSearch then
					validMap = true
				end
			else
				if v == mapSelectionCreate then
					validMap = true
				end
			end
		end
	end

	if not validMap then
		if PlayerHosted.options.activetab == 'search' then
			mapSelectionSearch = '' --filter Any
		else
			mapSelectionCreate = 'caldavar' --map Forest of Caldavar
		end
	end

	PlayerHosted.options['create']['mapname'] = mapSelectionCreate
	PlayerHosted.options['search']['mapname'] = mapSelectionSearch
	GetWidget('phg_mapname_combobox'):DoEventN(1)
	GetWidget('phg_mapname_combobox_2'):DoEventN(1)

	if PlayerHosted.options.create.gamemode.solosame == 'solosame:required' or
	   PlayerHosted.options.create.gamemode.solodiff == 'solodiff:required'	then
		GetWidget('phg_mapname_combobox'):SetEnabled(false)
		GetWidget('creategame_teamsize_combobox'):SetEnabled(false)
	elseif PlayerHosted.options.create.gamemode.midwars_beta == 'midwars_beta:required' then
		GetWidget('phg_mapname_combobox'):SetEnabled(false)
		GetWidget('creategame_teamsize_combobox'):SetEnabled(true)
	else
		--nothing
	end

	-- Echo('isBotMatch = '..tostring(isBotMatch))
	-- Echo('validMap = '..tostring(validMap))
	-- Echo('mapSelectionCreate = '..tostring(mapSelectionCreate))
	-- Echo('mapSelectionSearch = '..tostring(mapSelectionSearch))
	-- Echo('PlayerHosted.options.create.mapname ='..PlayerHosted.options['create']['mapname'])
	-- Echo('PlayerHosted.options.search.mapname ='..PlayerHosted.options['search']['mapname'])

	lastBotMatch = isBotMatch
	PlayerHosted.updatingMaps = false
end

local function UpdateServerOptions(mapWasChanged)
	mapWasChanged = mapWasChanged or false
	if (PlayerHosted.options.activetab == 'search') then

		--println('Un Restricted Mode With Pass- Enable Game Mode - Enable Server')

		groupfcall('ph_filter_option_group', function(_, widget, _) widget:DoEventN(3) end)

		GetWidget('select_server_idle'):SetEnabled(1)
		GetWidget('select_server_idle'):SetNoClick(0)

		GetWidget('create_game_multipass'):SetVisible(0)
		GetWidget('create_game_token_required'):SetVisible(0)

		-- removed so that people who have multiple modes selected along with botmatch can still select different maps
		-- if (PlayerHosted.options[PlayerHosted.options.activetab]['gamemode']['botmatch'] == 'botmatch:required') then -- and (GetCvarString('_game_server_type') ~= 'practice')
		-- 	PlayerHosted.options[PlayerHosted.options.activetab]['mapname'] = 'caldavar'
		-- 	GetWidget('phg_mapname_combobox'):SetEnabled(0)
		-- 	GetWidget('phg_mapname_combobox_2'):SetEnabled(0)
		-- else
		GetWidget('phg_mapname_combobox'):SetEnabled(1)
		GetWidget('phg_mapname_combobox_2'):SetEnabled(1)

		if not mapWasChanged then
			PlayerHosted.UpdateMapSelection()
		end

		-- end

		groupfcall('ph_filter_option_group', function(_, widget, _) widget:DoEventN(1) end)

	else

		Set('_game_server_type', '', 'string', true)

		if (not GetCvarBool('ui_canCreateGame')) then

			--println('Unverified - SHUT. DOWN. EVERYTHING!')

			Set('_game_server_type', 'practice', 'string')
			Set('_game_mode', 'normal', 'string')

			if (PlayerHosted.options.create.gamemode.botmatch == 'botmatch:required') then
				PlayerHosted.options.create.gamemode.botmatch = 'botmatch:required'
				PlayerHosted.options.create.gamemode.normal = 'normal:np'
				ValidateCreateGameModes('botmatch')
			else
				PlayerHosted.options.create.gamemode.botmatch = 'botmatch:np'
				PlayerHosted.options.create.gamemode.normal = 'normal:required'
				ValidateCreateGameModes('normal')
			end

			groupfcall('ph_filter_option_group', function(_, widget, _) widget:DoEventN(2) end)

			GetWidget('select_server_idle'):UICmd([[SetSelectedItemByValue('practice', 0)]])
			GetWidget('select_server_idle'):SetEnabled(0)
			GetWidget('select_server_idle'):SetNoClick(1)

			GetWidget('create_game_multipass'):SetVisible(0)
			GetWidget('create_game_token_required'):SetVisible(0)

		elseif (GetCvarString('_game_server_type') == 'practice') and NotEmpty(GetCvarString('_game_server_type')) then

			--println('Practice Game - Disable Game Mode - Enable Change Server')

			if (GetCvarBool('releaseStage_stable')) then
				Set('_game_server_type', 'practice', 'string')
				Set('_game_mode', 'normal', 'string')

				if (PlayerHosted.options.create.gamemode.botmatch == 'botmatch:required') then
					PlayerHosted.options.create.gamemode.botmatch = 'botmatch:required'
					PlayerHosted.options.create.gamemode.normal = 'normal:np'
					ValidateCreateGameModes('botmatch')
				else
					PlayerHosted.options.create.gamemode.botmatch = 'botmatch:np'
					PlayerHosted.options.create.gamemode.normal = 'normal:required'
					ValidateCreateGameModes('normal')
				end
			end
			
			groupfcall('ph_filter_option_group', function(_, widget, _) widget:DoEventN(2) end)

			GetWidget('select_server_idle'):UICmd([[SetSelectedItemByValue('practice', 0)]])
			GetWidget('select_server_idle'):SetEnabled(1)
			GetWidget('select_server_idle'):SetNoClick(0)

			GetWidget('create_game_multipass'):SetVisible(0)
			GetWidget('create_game_token_required'):SetVisible(0)

		elseif  (AccountStanding()	>= GetCvarInt('accountStanding_allModeThreshold')) or
				((GetCvarString('_game_mode') == 'normal') and NotEmpty(GetCvarString('_game_mode'))) or
				((GetCvarString('_game_server_type') == 'local' or GetCvarString('_game_server_type') == 'local_dedicated') and NotEmpty(GetCvarString('_game_server_type'))) then

			--println('Legacy or All Pick - Enable Game Mode - Enable Server')

			groupfcall('ph_filter_option_group', function(_, widget, _) widget:DoEventN(3) end)

			GetWidget('select_server_idle'):SetEnabled(1)
			GetWidget('select_server_idle'):SetNoClick(0)

			GetWidget('create_game_multipass'):SetVisible(0)
			GetWidget('create_game_token_required'):SetVisible(0)

		elseif NeedsToken(GetCvarString('_game_mode')) and (not HasAllHeroes()) then

			--println('Restricted Mode With No Pass - Enable Game Mode - Enable Server - Warn Tokens')

			groupfcall('ph_filter_option_group', function(_, widget, _) widget:DoEventN(3) end)

			GetWidget('select_server_idle'):SetEnabled(1)
			GetWidget('select_server_idle'):SetNoClick(0)

			GetWidget('create_game_multipass'):SetVisible(0)
			GetWidget('create_game_token_required'):FadeIn(150)

		elseif HasGamePass(GetCvarString('_game_mode')) and (not HasAllHeroes()) then

			groupfcall('ph_filter_option_group', function(_, widget, _) widget:DoEventN(3) end)

			GetWidget('select_server_idle'):SetEnabled(1)
			GetWidget('select_server_idle'):SetNoClick(0)

			GetWidget('create_game_multipass'):FadeIn(150)
			GetWidget('create_game_token_required'):SetVisible(0)

		else

			--println('Un Restricted Mode With Pass- Enable Game Mode - Enable Server')

			groupfcall('ph_filter_option_group', function(_, widget, _) widget:DoEventN(3) end)

			GetWidget('select_server_idle'):SetEnabled(1)
			GetWidget('select_server_idle'):SetNoClick(0)

			GetWidget('create_game_multipass'):SetVisible(0)
			GetWidget('create_game_token_required'):SetVisible(0)

		end

		GetWidget('phg_mapname_combobox'):SetEnabled(1)
		GetWidget('phg_mapname_combobox_2'):SetEnabled(1)

		if not mapWasChanged then
		PlayerHosted.UpdateMapSelection()
		end

		groupfcall('ph_filter_option_group', function(_, widget, _) widget:DoEventN(1) end)
	end

end

local function ApplyGameListFilter(clearFilter, mapWasChanged)
    mapWasChanged = mapWasChanged or false
	-- println('^r^: ApplyGameListFilter = ' .. tostring(clearFilter) )
	if (clearFilter) then

		interface:UICmd([[FilterGameList('', '')]])
	else
		local gameName = PlayerHosted.options[PlayerHosted.options.activetab]['gamename']
		local filter = ''

		if (PlayerHosted.options.activetab == 'create') then
			filter = 'servertype:' .. PlayerHosted.options[PlayerHosted.options.activetab]['server']
		else
			if (PlayerHosted.options[PlayerHosted.options.activetab].gameoption.nostats == 'nostats:required') then
				filter = 'servertype:2'
			elseif (PlayerHosted.options[PlayerHosted.options.activetab].gameoption.nostats == 'nostats:excluded') then
				filter = 'servertype:1'
			else
				filter = 'servertype:-1'
			end
		end
		filter = filter .. ' ' .. 'map:' .. PlayerHosted.options[PlayerHosted.options.activetab]['mapname']
		filter = filter .. ' ' .. 'region:' .. PlayerHosted.options[PlayerHosted.options.activetab]['region']
		filter = filter .. ' ' .. 'teamsize:' .. PlayerHosted.options[PlayerHosted.options.activetab]['teamsize']
		filter = filter .. ' ' .. 'minpsr:' .. PlayerHosted.options[PlayerHosted.options.activetab]['minpsr']
		filter = filter .. ' ' .. 'maxpsr:' .. PlayerHosted.options[PlayerHosted.options.activetab]['maxpsr']
		filter = filter .. ' ' .. 'spectators:' .. PlayerHosted.options[PlayerHosted.options.activetab]['spectators']
		filter = filter .. ' ' .. 'referees:' .. PlayerHosted.options[PlayerHosted.options.activetab]['referees']
		filter = filter .. ' ' .. 'ping:' .. PlayerHosted.options[PlayerHosted.options.activetab]['ping']

		if (PlayerHosted.options.activetab == 'create') then
			local gameMode
			for i, v in pairs(PlayerHosted.options.create.gamemode) do
				if (v == i .. ':required') then
					gameMode = i
				end
			end
			filter = filter .. ' ' .. 'mode:' .. gameMode
		else
			local tempModes = nil
			local secondMode = true
			if (PlayerHosted.options[PlayerHosted.options.activetab].gamemode) then
				for option, value in pairs(PlayerHosted.options[PlayerHosted.options.activetab].gamemode) do
					
					if ((value == (option..':required')) or (value == (option..':np'))) then
						-- println('^g' .. option .. ' | | | ' .. value)
						if (tempModes) then
							if (secondMode) then
								tempModes = tempModes .. ','
								secondMode = false
							end

							tempModes = tempModes .. option .. ','
						else
							tempModes = option
						end

						if (option == 'normal') then
							tempModes = tempModes .. ',soccerpick,forcepick'
						end
					else
						-- println('^r' .. option .. ' | | | ' .. value)
					end
				end
			end
			filter = filter .. ' ' .. 'mode:' .. tempModes
		end

		if (PlayerHosted.options[PlayerHosted.options.activetab].gameoption) then
			for option, value in pairs(PlayerHosted.options[PlayerHosted.options.activetab].gameoption) do
				filter = filter .. ' ' .. value
			end
		end

		interface:UICmd([[FilterGameList(']] .. EscapeString(gameName) .. [[', ']] .. filter .. [[')]])

		printdb('filter = ' .. tostring(filter) )
	end
	UpdateServerOptions(mapWasChanged)
end

local function UpdateModeOptionsIcon(sourceWidget, option, optionType)
	local tab = PlayerHosted.options[PlayerHosted.options.activetab]
	if (tab and tab[optionType]) then
		if tab[optionType][option] then
			if (tab[optionType][option] == option .. ':np') then
				sourceWidget:SetButtonState(0)
			elseif (tab[optionType][option] == option .. ':required') then
				sourceWidget:SetButtonState(1)
			elseif (tab[optionType][option] == option .. ':excluded') then
				if (PlayerHosted.options.activetab == 'create' and (not (option == "noagility" or option == "nostrength" or option == "nointelligence"))) then
					tab[optionType][option] = option .. ':np'
					sourceWidget:SetButtonState(0)
				else
					sourceWidget:SetButtonState(2)
				end
			else
				tab[optionType][option] = option .. ':np'
				sourceWidget:SetButtonState(0)
			end
		else
			sourceWidget:SetButtonState(0)
			e('PlayerHosted option error - option invalid A:', option)
		end
	else
		sourceWidget:SetButtonState(0)
		e('PlayerHosted option error - activetab invalid:', PlayerHosted.options.activetab)
		e('PlayerHosted option error - optionType invalid:', optionType)
	end
end

local function UpdateOptionsIcon(sourceWidget, option, optionType)
	UpdateModeOptionsIcon(sourceWidget, option, optionType)
	--[[
	if (PlayerHosted.options[PlayerHosted.options.activetab] and PlayerHosted.options[PlayerHosted.options.activetab][optionType]) then
		if PlayerHosted.options[PlayerHosted.options.activetab][optionType][option] then
			if (PlayerHosted.options[PlayerHosted.options.activetab][optionType][option] == option .. ':np') then
				sourceWidget:SetButtonState(0)
			elseif (PlayerHosted.options[PlayerHosted.options.activetab][optionType][option] == option .. ':required') then
				sourceWidget:SetButtonState(1)
			elseif (PlayerHosted.options[PlayerHosted.options.activetab][optionType][option] == option .. ':excluded') then
				if (PlayerHosted.options.activetab == 'create' and (not (option == "noagility" or option == "nostrength" or option == "nointelligence"))) then
					PlayerHosted.options[PlayerHosted.options.activetab][optionType][option] = option .. ':np'
					sourceWidget:SetButtonState(0)
				else
					sourceWidget:SetButtonState(2)
				end
			else
				PlayerHosted.options[PlayerHosted.options.activetab][optionType][option] = option .. ':np'
				sourceWidget:SetButtonState(0)
			end
		else
			sourceWidget:SetButtonState(0)
			e('PlayerHosted option error - option invalid A:', option)
		end
	else
		sourceWidget:SetButtonState(0)
		e('PlayerHosted option error - activetab invalid:', PlayerHosted.options.activetab)
		e('PlayerHosted option error - optionType invalid:', optionType)
	end
	--]]
end

function PlayerHosted.FilterModeButtonClicked(sourceWidget, option, buttonState, template, optionType)
	buttonState = tonumber(sourceWidget:GetButtonState())	-- workaround for race condition

	if (PlayerHosted.options[PlayerHosted.options.activetab] and PlayerHosted.options[PlayerHosted.options.activetab][optionType]) then
		buttonState = tonumber(buttonState)
		if (PlayerHosted.options.activetab == 'create') then
			if (buttonState == 2) then
				buttonState = 0
			end
			if (buttonState == 0) then
				PlayerHosted.options[PlayerHosted.options.activetab][optionType][option] = option .. ':np'
				if  (optionType == 'gamemode') then
					ValidateCreateGameModes()
				elseif (optionType == 'gameoption') then
					ValidateCreateGameOptions(option)
				end
			elseif (buttonState == 1) then
				PlayerHosted.options[PlayerHosted.options.activetab][optionType][option] = option .. ':required'
				if  (optionType == 'gamemode') then
					ValidateCreateGameModes(option)
				elseif (optionType == 'gameoption') then
					ValidateCreateGameOptions(option)
				end
			else
				PlayerHosted.options[PlayerHosted.options.activetab][optionType][option] = option .. ':np'
				e('PlayerHosted option error - state invalid:', buttonState)
				if  (optionType == 'gamemode') then
					ValidateCreateGameModes()
				elseif (optionType == 'gameoption') then
					ValidateCreateGameOptions(option)
				end
			end
		else
			if (buttonState == 0) then
				buttonState = 1
			end
			if (buttonState == 1) then
				PlayerHosted.options[PlayerHosted.options.activetab][optionType][option] = option .. ':required'
			elseif (buttonState == 2) then
				PlayerHosted.options[PlayerHosted.options.activetab][optionType][option] = option .. ':excluded'
			else
				PlayerHosted.options[PlayerHosted.options.activetab][optionType][option] = option .. ':excluded'
				e('PlayerHosted option error - state invalid:', buttonState)
			end
		end
	else
		e('PlayerHosted option error - activetab invalid:', PlayerHosted.options.activetab)
		e('PlayerHosted option error - optionType invalid:', optionType)
	end
	groupfcall('ph_filter_option_group', function(_, widget, _) widget:DoEventN(1) end)
	ApplyGameListFilter(false)
	GetDBEntry('phGamesOptions9', PlayerHosted.options, true, false, false)
end

function PlayerHosted.FilterButtonClicked(sourceWidget, option, buttonState, template, optionType)
	buttonState = tonumber(sourceWidget:GetButtonState())	-- workaround for race condition

	if (PlayerHosted.options[PlayerHosted.options.activetab] and PlayerHosted.options[PlayerHosted.options.activetab][optionType]) then
		buttonState = tonumber(buttonState)
		if (buttonState == 0) then
			PlayerHosted.options[PlayerHosted.options.activetab][optionType][option] = option .. ':np'
			if (PlayerHosted.options.activetab == 'create') then
				if (optionType == 'gamemode') then
					ValidateCreateGameModes()
				elseif (optionType == 'gameoption') then
					ValidateCreateGameOptions(option)
				end
			end
		elseif (buttonState == 1) then
			PlayerHosted.options[PlayerHosted.options.activetab][optionType][option] = option .. ':required'
			if (PlayerHosted.options.activetab == 'create') then
				if  (optionType == 'gamemode') then
					ValidateCreateGameModes(option)
				elseif (optionType == 'gameoption') then
					ValidateCreateGameOptions(option)
				end
			end
		elseif (buttonState == 2) then
			PlayerHosted.options[PlayerHosted.options.activetab][optionType][option] = option .. ':excluded'
			if (PlayerHosted.options.activetab == 'create') then
				if (optionType == 'gamemode') then
					ValidateCreateGameModes()
				elseif (optionType == 'gameoption') then
					ValidateCreateGameOptions(option)
				end
			end
		else
			PlayerHosted.options[PlayerHosted.options.activetab][optionType][option] = option .. ':np'
			e('PlayerHosted option error - state invalid:', buttonState)
			if (PlayerHosted.options.activetab == 'create') then
				if (optionType == 'gamemode') then
					ValidateCreateGameModes()
				elseif (optionType == 'gameoption') then
					ValidateCreateGameOptions(option)
				end
			end
		end
	else
		e('PlayerHosted option error - activetab invalid:', PlayerHosted.options.activetab)
		e('PlayerHosted option error - optionType invalid:', optionType)
	end
	groupfcall('ph_filter_option_group', function(_, widget, _) widget:DoEventN(1) end)
	ApplyGameListFilter(false)
	GetDBEntry('phGamesOptions9', PlayerHosted.options, true, false, false)
end

function PlayerHosted.FilterModeButtonOnShow(sourceWidget, option, buttonState, template, optionType)
	 UpdateModeOptionsIcon(sourceWidget, option, optionType)
end

function PlayerHosted.FilterButtonOnShow(sourceWidget, option, buttonState, template, optionType)
	 UpdateOptionsIcon(sourceWidget, option, optionType)
end

function PlayerHosted.FilterModeButtonUpdate(sourceWidget, option, buttonState, template, optionType)
	 UpdateModeOptionsIcon(sourceWidget, option, optionType)
end

function PlayerHosted.FilterButtonUpdate(sourceWidget, option, buttonState, template, optionType)
	 UpdateOptionsIcon(sourceWidget, option, optionType)
end

function PlayerHosted.TabButtonClicked(sourceWidget, tabType)
	-- println('sourceWidget ' .. tostring(sourceWidget))
	-- println('tabType ' .. tostring(tabType))
	-- println('PlayerHosted.options[tabType] ' .. tostring(PlayerHosted.options[tabType]))
	if (PlayerHosted.options[tabType]) then
		PlayerHosted.options.activetab = tabType
		ApplyGameListFilter(false)
		groupfcall('ph_filter_option_group', function(_, widget, _) widget:DoEventN(1) end)
		Trigger('GameListStatus', 0, 0, 0, 0, 0, 0)
		GetDBEntry('phGamesOptions9', PlayerHosted.options, true, false, false)
		groupfcall('py_bottom_status_info', function(_, widget, _) widget:SetVisible(0) end)
	end
end

function PlayerHosted.OnShow()
	Trigger('GameListStatus', 0, 0, 0, 0, 0, 0)
	groupfcall('py_bottom_status_info', function(_, widget, _) widget:SetVisible(0) end)
end

function PlayerHosted.OptionUpdate(sourceWidget, option)
	-- println('PlayerHosted.options.activetab ' .. tostring(PlayerHosted.options.activetab) )
	-- println('option ' .. tostring(option) )
	-- println('PlayerHosted.options[PlayerHosted.options.activetab] ' .. tostring(PlayerHosted.options[PlayerHosted.options.activetab]) )
	-- println('PlayerHosted.options[PlayerHosted.options.activetab][option] ' .. tostring(PlayerHosted.options[PlayerHosted.options.activetab][option]) )

	if (PlayerHosted.options[PlayerHosted.options.activetab][option]) then
		return PlayerHosted.options[PlayerHosted.options.activetab][option]
	else
		return [['no data']]
	end

end

function PlayerHosted.OptionChanged(sourceWidget, option, value)
	local optionChanged = false
	-- println('OptionChanged = ' .. tostring(option) .. ' | ' .. tostring(value) )
	if (PlayerHosted.options[PlayerHosted.options.activetab][option]) then
		if PlayerHosted.options[PlayerHosted.options.activetab][option] ~= value then
			PlayerHosted.options[PlayerHosted.options.activetab][option] = value
			optionChanged = true
		end
	end

	if optionChanged then
		ApplyGameListFilter(false, option == 'mapname')
		PlayerHosted.UpdateMapOptions(option == 'mapname')
	end
	GetDBEntry('phGamesOptions9', PlayerHosted.options, true, false, false)

	--groupfcall('ph_filter_option_group', function(_, widget, _) widget:DoEventN(1) end)
end

function PlayerHosted.OptionsInit()
	ApplyGameListFilter(false)
	groupfcall('ph_filter_option_group', function(_, widget, _) widget:DoEventN(1) end)
	if GetWidget('optiontab_' .. PlayerHosted.options.activetab, nil, true) then
		GetWidget('optiontab_' .. PlayerHosted.options.activetab):DoEvent()
	end
end

function PlayerHosted.OptionsOnLoad()
	-- clear onload? ApplyGameListFilter(true)
end

function PlayerHosted.UpdateMapOptions(mapWasChanged)
	--println(' ^g PlayerHosted.UpdateMapOptions() ')
	if GetWidget('game_list') then
		UpdateServerOptions(mapWasChanged)
		-- UpdateMapOptions()
	end
end

local function GetGameOptions()

	local gameName = PlayerHosted.options[PlayerHosted.options.activetab]['gamename']
	local filter = ''

	if (PlayerHosted.options[PlayerHosted.options.activetab].gameoption.tournament == 'tournament:required') then
		PlayerHosted.options[PlayerHosted.options.activetab].gameoption.tournamentrules = 'tournamentrules:required'
	else
		PlayerHosted.options[PlayerHosted.options.activetab].gameoption.tournamentrules = 'tournamentrules:np'
	end

	printTable(PlayerHosted.options[PlayerHosted.options.activetab].gameoption)

	-- filter = 'servertype:' .. PlayerHosted.options[PlayerHosted.options.activetab]['server']
	filter = 'map:' .. PlayerHosted.options[PlayerHosted.options.activetab]['mapname']
	filter = filter .. ' ' .. 'region:' .. PlayerHosted.options[PlayerHosted.options.activetab]['region']
	filter = filter .. ' ' .. 'teamsize:' .. PlayerHosted.options[PlayerHosted.options.activetab]['teamsize']
	filter = filter .. ' ' .. 'minpsr:' .. PlayerHosted.options[PlayerHosted.options.activetab]['minpsr']
	filter = filter .. ' ' .. 'maxpsr:' .. PlayerHosted.options[PlayerHosted.options.activetab]['maxpsr']
	filter = filter .. ' ' .. 'spectators:' .. PlayerHosted.options[PlayerHosted.options.activetab]['spectators']
	filter = filter .. ' ' .. 'referees:' .. PlayerHosted.options[PlayerHosted.options.activetab]['referees']
	filter = filter .. ' ' .. 'ping:' .. PlayerHosted.options[PlayerHosted.options.activetab]['ping']

	local gameMode
	if (PlayerHosted.options.activetab == 'create') then
		for i, v in pairs(PlayerHosted.options.create.gamemode) do
			if (v == i .. ':required') then
				gameMode = i
			end
		end
		filter = filter .. ' ' .. 'mode:' .. gameMode
	else
		if (PlayerHosted.options[PlayerHosted.options.activetab].gamemode) then
			for option, value in pairs(PlayerHosted.options[PlayerHosted.options.activetab].gamemode) do
				if (gameMode) then
					gameMode = gameMode .. ', ' ..option
				else
					gameMode = option
				end
			end
		end
		filter = filter .. ' ' .. 'mode:' .. gameMode
	end

	if (not gameMode) then
		gameMode = 'normal'
	end

	if (PlayerHosted.options[PlayerHosted.options.activetab].gameoption) then
		for option, value in pairs(PlayerHosted.options[PlayerHosted.options.activetab].gameoption) do
			if (value == option .. ':required') then
				filter = filter .. ' ' .. option .. ':true'
			else
				filter = filter .. ' ' .. option .. ':false'
			end
		end
	end

	return gameName, gameMode, filter

end

local function CreateGame(gameName, gameMode, filter, isLocal)

	println('_game_server_type = ' .. tostring(GetCvarString('_game_server_type')) )
	println('gameName = ' .. tostring(gameName) )
	println('gameMode = ' .. tostring(gameMode) )
	println('filter = ' .. tostring(filter) )
	println('isLocal = ' .. tostring(isLocal) )

	if ((GetCvarString('_game_server_type') == "practice") and (tostring(gameMode) == "botmatch")) then
		Set("ui_local_bot_game", true, "bool")
	else
		Set("ui_local_bot_game", false, "bool")
	end

	if (not gameName) or Empty(gameName) then
		gameName = GetAccountName() or 'Nameless'
	end

	if (gameMode ~= 'normal') then
		if (not (AccountStanding() >= GetCvarInt('accountStanding_allModeThreshold'))) and NeedsToken(gameMode) then
			if (GameTokens() >= GetCvarInt('tokenCostPerGame')) then
				-- reset to normal as first param
				if (isLocal) then
					Trigger('WarnTokensTrigger', '', [[Cmd('set _loading_tip_shown false'); Call('player_host_sleep_helper_2', 'SleepWidget(400, \'CancelServerList; StartGame(_game_server_type, \\\']] .. EscapeString(gameName) .. [[\\\', \\\']] .. filter .. [[\\\');\');')]])
				else
					Trigger('WarnTokensTrigger', '', [[Cmd('set _loading_tip_shown false'); Call('player_host_sleep_helper_2', 'SleepWidget(400, \'CancelServerList; SendCreateGameRequest(\\\']] .. EscapeString(gameName) .. [[\\\', \\\']] .. filter .. [[\\\');\');')]])
				end
			else
				Trigger('NeedTokensTrigger', '', 'CallEvent(\'mm_popup_buytoken\', 0);')
			end
		else
			interface:UICmd([[Cmd('Set _loading_tip_shown false')]])
			-- UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'game_list', nil, nil, true, nil, 3)
			if (isLocal) then
				GetWidget('player_host_sleep_helper_2'):Sleep(400, function() interface:UICmd([[CancelServerList; StartGame(_game_server_type, ']] .. EscapeString(gameName) .. [[', ']] .. filter .. [[')]]) end)
			else
				GetWidget('player_host_sleep_helper_2'):Sleep(400, function() interface:UICmd([[CancelServerList; SendCreateGameRequest(']] .. EscapeString(gameName) .. [[', ']] .. filter .. [[')]]) end)
			end
		end
	else
		interface:UICmd([[Cmd('Set _loading_tip_shown false')]])
		-- UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'game_list', nil, nil, true, nil, 3)
		if (isLocal) then
			GetWidget('player_host_sleep_helper_2'):Sleep(400, function() interface:UICmd([[CancelServerList; StartGame(_game_server_type, ']] .. EscapeString(gameName) .. [[', ']] .. filter .. [[')]]) end)
		else
			GetWidget('player_host_sleep_helper_2'):Sleep(400, function() interface:UICmd([[CancelServerList; SendCreateGameRequest(']] .. EscapeString(gameName) .. [[', ']] .. filter .. [[')]]) end)
		end
	end

end

function PlayerHosted.CreateGame()

	local gameName, gameMode, filter = GetGameOptions()

	if (GetCvarString('_game_server_type') == 'local') or (GetCvarString('_game_server_type') == 'local_dedicated') or (GetCvarString('_game_server_type') == 'practice')  or (GetCvarString('_game_server_type') == 'remote_automatic') then
		CreateGame(gameName, gameMode, filter, true)
	else
		CreateGame(gameName, gameMode, filter, false)
	end
end

local function RemoteLoading(sourceWidget, isLoading)
	if AtoB(isLoading) then
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'game_list', nil, nil, true, nil, 3)
	end
end
interface:RegisterWatch('RemoteLoading', RemoteLoading)

-- local function UIIsVerified(_, isVerified)
	-- UpdateMapOptions()
-- end
-- GetWidget('game_list'):RegisterWatch('UIIsVerified', UIIsVerified)

-- local function AccountInfo(_, _, _, _, _, _, _, isLeaver)
	-- UpdateMapOptions()
-- end
-- GetWidget('game_list'):RegisterWatch('AccountInfo', AccountInfo)


