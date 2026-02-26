-- Player Stats Post XML


local statPanels = {
	stats_matches_list_event	= {
		trigger		= 'PlayerStatsResultEvent'
	},
	stats_matches_list_ranked	= {
		trigger		= 'PlayerStatsResultRanked'
	},
	stats_matches_list_casual	= {
		trigger		= 'PlayerStatsResultCasual'
	},
	stats_matches_list_public	= {
		trigger		= 'PlayerStatsResult'
	},
	stats_matches_list_season_normal = {
		trigger		= 'PlayerStatsResultSeasonNormal'
	},
	stats_matches_list_season_casual = {
		trigger		= 'PlayerStatsResultSeasonCasual'
	}
}


for statPanel, statPanelInfo in pairs(statPanels) do
	if GetWidget(statPanel) then
		GetWidget(statPanel):RegisterWatch(statPanelInfo.trigger, function(widget, ...)

			-- local selectedPlayerDropdown = GetWidget('matchreplays_selectedplayer_dropdown')
			-- selectedPlayerDropdown:ClearItems()

			-- local searchName = GetCvarString('_player_stats_searchname')
			-- if string.len(searchName) > 0 and (not selectedPlayerDropdown:HasListItem(searchName)) and GetAccountName() ~= searchName then
			-- 	selectedPlayerDropdown:AddTemplateListItem('Ncombobox_item', searchName, 'label', searchName)
			-- 	selectedPlayerDropdown:SetSelectedItemByValue(searchName)
			-- end
			-- if (not selectedPlayerDropdown:HasListItem(Client.GetAccountID())) then
			-- 	selectedPlayerDropdown:AddTemplateListItem('Ncombobox_item', GetAccountName(), 'label', GetAccountName())
			-- end

			-- selectedPlayerDropdown:SetVisible(selectedPlayerDropdown:GetNumListItems() > 1)

			widget:ClearItems()
			local matchIDs		= arg[53] or ''
			local matchDates	= arg[54] or ''

			local matchList	= {}

			while string.len(matchIDs) > 0 do
				table.insert(
					matchList, 
					{
						id		= widget:UICmd('Trim('..string.sub(matchIDs, 1, 10)..')'),
						date	= string.sub(matchDates, 1, 10)
					}
				)
				matchIDs	= string.sub(matchIDs, 11, -1)
				matchDates	= string.sub(matchDates, 11, -1)
			end

			for k,v in ipairs(matchList) do
				if v.id and v.date then
					widget:AddTemplateListItem('stats_lastmatches_match', v.id, 'match_id', v.id, 'match_date', v.date, 'matchnum', (k - 1), 'match_listbox', statPanel)
				end
			end

			GetWidget(statPanel..'_no_history'):SetVisible(#matchList <= 0)
		end)
	end
end

local function ShouldHideSeasonHistory()
	if HoN_Region.activeRegion == 'international' then	-- hide other's season history for naeu
		local accountName = GetAccountName()
		if (string.find(accountName, ']')) then
			accountName = string.sub(accountName, string.find(accountName, ']') + 1)
		end
		local targetName = GetCvarString('_player_stats_searchname')
		if (string.find(targetName, ']')) then
			targetName = string.sub(targetName, string.find(targetName, ']') + 1)
		end
		if accountName ~= targetName then
			return true
		end
	end
	return false
end

local function UpdateFormCallback(formName, hideSeasonHistiry)

	local interface = UIManager.GetInterface('main')
	local form = interface:GetForm(formName)

	if form == nil then
		Echo('^rUpdateFormCallback: nil form named '..tostring(formName))
		return
	elseif hideSeasonHistiry then
		form:SetResponseCallback(function(updatedForm)
			local responseTable = updatedForm:GetLastResponse()

			local hideTable = {}
			for k, v in pairs(responseTable) do
				local t = type(v)
				if t == 'number' then
					hideTable[k] = 0
				elseif t == 'string' and tonumber(v) then
					hideTable[k] = '0'
				end
			end

			local nonHideFields = {
				'nickname',  'account_id', 'selected_upgrades',
				'name', 'rank', 'standing', 'level', 'level_exp',
				'last_activity', 'create_date',
			}
			for i, s in ipairs(nonHideFields) do
				hideTable[s] = responseTable[s]
			end

			local emptyStrFields = {
				'curr_season_cam_cs_games_played', 'curr_season_cam_games_played',
				'cam_amm_team_rating'
			}
			for i, s in ipairs(emptyStrFields) do
				hideTable[s] = ''
			end

			form:SetResponseData(hideTable)
		end)
	else
		form:SetResponseCallback(function() end)
	end
end

local function InitSeasonHistoryView(widget, seasons, seasonType, hideSeasonHistiry)
	local alreadyRequest = false

	for i,v in ipairs(seasons) do
		local singleSeason = explode(',', v)
		local season_id = singleSeason[1] 
		local is_casual = singleSeason[2]

		if season_id and is_casual then
			local season_name = ''
			local isCasual = AtoB(is_casual)
			if tonumber(season_id) >= 1000 then
				if isCasual then
					season_name = Translate('player_stats_preseason_name_casual')
				else
					season_name = Translate('player_stats_preseason_name_normal')
				end
			else
				if isCasual then
					season_name = Translate('player_stats_season_name_casual', 'season_id', season_id)
				else
					season_name = Translate('player_stats_season_name_normal', 'season_id', season_id)
				end
			end
			if isCasual then
				widget:AddTemplateListItem('stats_season_history_casual', tostring(i), 'season_name', season_name, 'season_id', season_id, 'panel', seasonType)
			else
				widget:AddTemplateListItem('stats_season_history_normal', tostring(i), 'season_name', season_name, 'season_id', season_id, 'panel', seasonType)
			end

			if not alreadyRequest and GetWidget('stats_panel_history_'..seasonType):IsVisible() then
				if isCasual then
					GetWidget('unnamedButton209_'..season_id..'_'..seasonType):DoEvent()
				else
					GetWidget('unnamedButton208_'..season_id..'_'..seasonType):DoEvent()
				end
				alreadyRequest = true
				if hideSeasonHistiry then
					break
				end
			end
		end
	end

	GetWidget('stats_matches_list_history_'..seasonType..'_no_season'):SetVisible(#seasons <= 0 or hideSeasonHistiry)
	widget:SetVisible(not hideSeasonHistiry)
end

if not isNewUI() then
GetWidget('stats_matches_list_history_normal_season'):RegisterWatch('GetSeasonsResult', function(widget, ...)
		widget:ClearItems()

		local seasons = explode('|', arg[1])
		if not NotEmpty(arg[1]) then 
			seasons = {}
		end

		local hideSeasonHistiry = ShouldHideSeasonHistory()
		UpdateFormCallback('PlayerStatsHistoryNormal', hideSeasonHistiry)

		InitSeasonHistoryView(widget, seasons, 'normal', hideSeasonHistiry)
	end)

GetWidget('stats_matches_list_history_casual_season'):RegisterWatch('GetSeasonsResult', function(widget, ...)
		widget:ClearItems()

		local seasons = explode('|', arg[1])
		if not NotEmpty(arg[1]) then 
			seasons = {}
		end

		local hideSeasonHistiry = ShouldHideSeasonHistory()
		UpdateFormCallback('PlayerStatsHistoryCasual', hideSeasonHistiry)

		InitSeasonHistoryView(widget, seasons, 'casual', hideSeasonHistiry)
	end)
end