-- Match Stats post XML
Set('matchstats_use_simple_view', 'false', 'bool')

local function selectMatchID(matchID, listFinished)
	listFinished = listFinished or false
	if matchID and type(matchID) == 'string' then
		Set('ui_match_replays_blockSimpleStats', 'true');
		Set('_stats_last_match_id', matchID)

		local _stats_last_match_id = GetCvarString('_stats_last_match_id')

		if GetWidget('main_stats_listbox'):GetValue() == Translate('match_replays_no_match_stats') and listFinished then
			GetWidget('nomatchhistory'):SetVisible(true)
		else
			if string.len(_stats_last_match_id) > 0 and GetWidget('match_stats'):IsVisible() and tonumber(_stats_last_match_id) > 0 then
				GetWidget('main_stats_listbox'):UICmd("GetMatchInfo(".._stats_last_match_id..")")
			end
		end
	end
end

local function matchListProcessFinished()
	if GetWidget('main_stats_listbox'):GetNumListItems() > 0 and GetWidget('main_stats_listbox'):GetValue() ~= Translate('match_replays_no_match_stats') then
		local _stats_last_match_id = GetCvarString('_stats_last_match_id')

		if string.len(_stats_last_match_id) > 0 and tonumber(_stats_last_match_id) > 0 then
			local list = GetWidget('main_stats_listbox')
			if list then
				list:SetSelectedItemByValue(_stats_last_match_id)
				list:ScrollToListItem(_stats_last_match_id)
				return
			end
		end
		
		if string.len(_stats_last_match_id) == 0 or _stats_last_match_id == '4294967295' or (string.len(_stats_last_match_id) > 0 and tonumber(_stats_last_match_id) <= 0) then
			GetWidget('main_stats_listbox'):SetSelectedItemByIndex(0)
			Set('_stats_latest_match_played', GetWidget('main_stats_listbox'):GetValue(), 'string')
			selectMatchID(GetWidget('main_stats_listbox'):GetValue(), true)			
		end
	else
		GetWidget('nomatchhistory'):SetVisible(true)
	end
end

GetWidget('main_stats_listbox'):RegisterWatch('MatchEntriesFinished', matchListProcessFinished)

local function matchListProcessEntry(matchID)
	if NotEmpty(matchID) then
		GetWidget('main_stats_listbox'):AddTemplateListItem('match_list_entry', matchID, 'matchid', matchID)
	else
		--local stripAccountName	= GetWidget('main_stats_listbox'):UICmd("StripClanTag('"..GetAccountName().."')")
		--local stripSearchName	= GetWidget('main_stats_listbox'):UICmd("StripClanTag('"..tostring(GetCvarBool('_player_stats_searchname')).."')")
		--if stripAccountName == stripSearchName or string.len(stripSearchName) == 0 then
		GetWidget('main_stats_listbox'):ClearItems()
		--end
	end
end

GetWidget('main_stats_listbox'):RegisterWatch('MatchEntry', function(widget, matchID)
	--print('process match from code '..matchID..'\n')
	matchListProcessEntry(matchID)
end)

GetWidget('main_stats_listbox'):RegisterWatch('PlayerRecentGamesListResult', function(widget, ...)
	Set('_matchstats_recentgames_lastSuccessfulLookup', GetCvarString('_matchstats_recentgames_lastLookup'))
	widget:ClearItems()
	local matchList = Explode(',', arg[1])
	for k,v in ipairs(matchList) do
		print('process match from form '..v..'\n')
		matchListProcessEntry(v)
	end
	matchListProcessFinished()
end)

GetWidget('main_stats_listbox'):RegisterWatch('MatchInfoSummary', function(widget, ...)
	local matchID = arg[2]
	if matchID and type(matchID) == 'string' and string.len(matchID) > 0 then
		widget:SetSelectedItemByValue(matchID, false)
		Cvar.GetCvar('_stats_last_match_id'):Set(matchID)
	end
end)


GetWidget('match_stats_retry_btn'):SetCallback('onclick', function(widget)
	Cvar.GetCvar('ui_match_replays_blockSimpleStats'):Set('false')

	if string.len(GetCvarString('_stats_last_match_id')) > 0 and GetWidget('match_stats'):IsVisible() then
		local matchId = GetCvarString('_stats_last_match_id')
		GetWidget('match_stats_retry_btn'):UICmd("GetMatchInfo('"..matchId.."')")
	end
end)

GetWidget('main_stats_listbox'):SetCallback('onselect', function(widget)
	widget:UICmd("ClearEndGameStats()")
	PlaySound('/shared/sounds/ui/button_click_01.wav')
	selectMatchID(widget:GetValue())
end)