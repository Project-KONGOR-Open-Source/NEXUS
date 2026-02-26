---------------------------------------------------------- 
--	Name: 		HonTour Script	            			--				
--  Copyright 2015 Frostburn Studios					--
---------------------------------------------------------- 

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface = object
local interfaceName = interface:GetName()
RegisterScript2('HonTour', '5')
HonTour = {}
HonTour.isMatchScheduled = GetCvarBool('ui_hontour_matchAvailable2', true) or false
local ht = {}
ht.loadingVisible = false
ht.tScheduledEventsTable  = {}
ht.tScheduledMatchesTable  = {}
ht.tPlayerInfoTable  = {}
ht.selectedEventID = nil
ht.localIsReady 	= nil
ht.localIsCaptain 	= nil
ht.selectedMatchID = nil
ht.isFindingServer = nil
ht.panelWidget = interface:GetWidget('hontour')

--LUA(ScheduledMatchCommand, "void", "string command, number  scheduledmatchid = 0") Possible commands: "join  scheduledmatchid", "leave", "ready", "unready".	
local function TriggerScheduledMatchCommand(_, command, matchID)
	ScheduledMatchCommand(command, matchID)
end

function HonTour.Error(errorID)
	println('HoN Tour Error: ' .. errorID )
	local errorID = errorID or 0
	GetWidget('hontour_error'):SetVisible(true)
	GetWidget('hontour_error_label'):SetText(Translate('hontour_error_'..errorID))
end

function HonTour:UpdatePlayNowButton()
	local midbar_button_matchmaking = GetWidget('midbar_button_matchmaking', 'main')
	--printdb('UpdatePlayNowButton: isMatchScheduled: ' .. tostring(HonTour.isMatchScheduled) .. ' | IsInScheduledMatch: ' .. tostring(IsInScheduledMatch()) )
	if (HonTour) and ((HonTour.isMatchScheduled) or (IsInScheduledMatch())) and (not (IsInGroup() or IsInQueue()) ) then
		groupfcall('midbar_button_matchmaking_icons', function(index, widget, groupName) widget:SetTexture('/ui/fe2/mainmenu/icons/N_hontour.tga') end, 'main')
		groupfcall('midbar_button_matchmaking_glow', function(index, widget, groupName) widget:SetTexture('/ui/fe2/mainmenu/icons/N_hontour_glow_blue.tga') end, 'main')
		groupfcall('midbar_button_matchmaking_labels', function(index, widget, groupName) widget:SetText(Translate('hontour_playnow')) end, 'main')
	else
		groupfcall('midbar_button_matchmaking_icons', function(index, widget, groupName) widget:SetTexture('/ui/fe2/mainmenu/icons/N_matchmaking.tga') end, 'main')
		groupfcall('midbar_button_matchmaking_glow', function(index, widget, groupName) widget:SetTexture('/ui/fe2/mainmenu/icons/N_matchmaking_glow_blue.tga') end, 'main')
		groupfcall('midbar_button_matchmaking_labels', function(index, widget, groupName) widget:SetText(Translate('main_menu_playnow')) end, 'main')
	end
end

function HonTour.ShowThrobber(isVisible)
	-- RMM GetWidget('hontour_throbber'):SetVisible(isVisible or false)
end

local function EventListing(_, sEventListingString)
	-- uiEventID|sEventName|Startdate|EndDate
	--printdb('^o EventListing param0: ' .. sEventListingString)
	
	ht.tScheduledEventsTable = explode('~', sEventListingString)

	for matchIndex, matchString in ipairs(ht.tScheduledEventsTable) do
		ht.tScheduledEventsTable[matchIndex] = explode('|', matchString)
	end
	--[[
	for i, v in ipairs(ht.tScheduledEventsTable) do
		printdb('^o Event: ' .. i)
		if GetCvarBool('ui_dev') then printTable(v)	end
		printdb('^o ----')
	end
	--]]
	HonTour:UpdatePlayNowButton()
end

local function ScheduledMatchListing(_, sMatchListingString)
	-- uiEventID|uiScheduledMatchID|sMatchTitle|StartDate|Expirationdate|VerifiedOnly|GameType|sMapName|sGameMode|sRegion|sTeamName1|sTeamName2
	printdb('^c ScheduledMatchListing param0: ' .. sMatchListingString)
	
	if (sMatchListingString) and NotEmpty(sMatchListingString) then
	
		ht.tScheduledMatchesTable = explode('~', sMatchListingString)
		
		if (ht.tScheduledMatchesTable) and type(ht.tScheduledMatchesTable == 'table') and (#ht.tScheduledMatchesTable > 0) then	
		
			for matchIndex, matchString in ipairs(ht.tScheduledMatchesTable) do	
				ht.tScheduledMatchesTable[matchIndex] = explode('|', matchString)
			end
		
			---[[
			for i, v in ipairs(ht.tScheduledMatchesTable) do
				printdb('^c Match Info: ' .. i)
				if GetCvarBool('ui_dev') then printTable(v)	end
				printdb('^c ----')
			end	
			--]]
			HonTour.isMatchScheduled = true
			
		else
		
			HonTour.isMatchScheduled = GetCvarBool('ui_hontour_matchAvailable2', true) or false
			
		end
		
	end
	printdb('^c HonTour.isMatchScheduled : ' .. tostring( HonTour.isMatchScheduled ) )
	HonTour:UpdatePlayNowButton()
	if GetWidget('confirmations_generic_ht_dialog_dropdown_1'):IsVisible() or GetWidget('confirmations_generic_ht_dialog_dropdown_2'):IsVisible() then
		HonTour.PromptToJoinLobby()
	end	
end

local function UpdatePromptEventDropdown()

	--println('UpdatePromptEventDropdown()')

	local confirmations_generic_ht_dialog_dropdown_1 = GetWidget('confirmations_generic_ht_dialog_dropdown_1')
	
	confirmations_generic_ht_dialog_dropdown_1:ClearItems()	
	
	local foundMatch = false
	for i, eventInfoTable in ipairs(ht.tScheduledEventsTable) do
		for i, matchInfoTable in ipairs(ht.tScheduledMatchesTable) do
			--println('eventInfoTable[2] = ' .. tostring(eventInfoTable[2]) )
			--println('matchInfoTable[1] = ' .. tostring(matchInfoTable[1]) )
			--println('eventInfoTable[1] = ' .. tostring(eventInfoTable[1]) )
			if (eventInfoTable[2]) and (matchInfoTable[1] == eventInfoTable[1]) then
				confirmations_generic_ht_dialog_dropdown_1:AddTemplateListItem('Ncombobox_item', eventInfoTable[1], 'label', eventInfoTable[2] )
				printdb('Adding: ' .. eventInfoTable[1] .. ' ' .. eventInfoTable[2] .. ' | matching: ' .. matchInfoTable[1] .. ' in ' .. matchInfoTable[3] )
				foundMatch = true
				break
			end
		end
	end	
	
	if (not foundMatch) then
		HonTour.Error(2)
	end
	
	if (tonumber(ht.selectedEventID)) and (tonumber(ht.selectedEventID) > 0) then
		confirmations_generic_ht_dialog_dropdown_1:SetSelectedItemByIndex(ht.selectedEventID)
	else
		confirmations_generic_ht_dialog_dropdown_1:SetSelectedItemByIndex(0)
	end	
	
end

local function UpdatePromptMatchDropdown()	
	local confirmations_generic_ht_dialog_dropdown_2 = GetWidget('confirmations_generic_ht_dialog_dropdown_2')
	
	confirmations_generic_ht_dialog_dropdown_2:ClearItems()
	for i, matchInfoTable in ipairs(ht.tScheduledMatchesTable) do
		if (matchInfoTable[2]) and (ht.selectedEventID) and (matchInfoTable[1] == ht.selectedEventID) then
			confirmations_generic_ht_dialog_dropdown_2:AddTemplateListItem('Ncombobox_item', matchInfoTable[2], 'label', matchInfoTable[3] )
		end
	end		
	
	if (tonumber(ht.selectedMatchID)) and (tonumber(ht.selectedMatchID) > 0) then
		confirmations_generic_ht_dialog_dropdown_2:SetSelectedItemByIndex(tonumber(ht.selectedMatchID))
	else
		confirmations_generic_ht_dialog_dropdown_2:SetSelectedItemByIndex(0)
	end	

end

function HonTour.PromptToJoinLobby()
	printdb('PromptToJoinLobby')
	
	local confirmations_generic_ht_dialog_dropdown_1 = GetWidget('confirmations_generic_ht_dialog_dropdown_1')
	local confirmations_generic_ht_dialog_dropdown_2 = GetWidget('confirmations_generic_ht_dialog_dropdown_2')
	local generic_ht_dialog_box_left_btn = GetWidget('generic_ht_dialog_box_left_btn')
	local generic_ht_dialog_box_right_btn = GetWidget('generic_ht_dialog_box_right_btn')
	
	Trigger('HTTriggerDialogBox', 'hontour_tournament_available', 'ht_lobby_prompt_left', 'ht_lobby_prompt_right', '', '', 'hontour_hontour_available', 'hontour_hontour_available_desc', tostring(#ht.tScheduledMatchesTable > 1), tostring(#ht.tScheduledEventsTable > 1) )
	
	generic_ht_dialog_box_left_btn:SetCallback('onclick', 
		function(this)
			HonTour.JoinLobby()
		end
	)
	generic_ht_dialog_box_left_btn:RefreshCallbacks()
	
	generic_ht_dialog_box_right_btn:SetCallback('onclick', 
		function(this)
			UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'matchmaking', nil, nil, nil, nil, 721)
		end
	)
	generic_ht_dialog_box_right_btn:RefreshCallbacks()	
	
	if (ht.tScheduledEventsTable) and (ht.tScheduledMatchesTable) then	
		confirmations_generic_ht_dialog_dropdown_1:SetCallback('onselect',
			function(this)
				if (not ht.selectedEventID) or (ht.selectedEventID ~= self:GetValue() ) then
					ht.selectedEventID = self:GetValue() 
					UpdatePromptMatchDropdown()
				else
					ht.selectedEventID = self:GetValue() 
				end
			end
		)	
		confirmations_generic_ht_dialog_dropdown_1:RefreshCallbacks()

		confirmations_generic_ht_dialog_dropdown_2:SetCallback('onselect',
			function(this)
				ht.selectedMatchID = self:GetValue()
			end
		)	
		confirmations_generic_ht_dialog_dropdown_2:RefreshCallbacks()
	end
	
	UpdatePromptEventDropdown()

end

function HonTour.JoinLobby()
	println('HonTour.JoinLobby()')
	println('ht.selectedMatchID = ' .. tostring(ht.selectedMatchID) )
	println('ht.selectedEventID = ' .. tostring(ht.selectedEventID) )
	if (tonumber(ht.selectedMatchID)) and (tonumber(ht.selectedMatchID) > 0)  and (tonumber(ht.selectedEventID)) and (tonumber(ht.selectedEventID) > 0) then
		HonTour.ShowThrobber(true)
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'hontour', nil, nil, nil, nil, 722)
		ScheduledMatchCommand('join', ht.selectedMatchID)
		printdb('Joining scheduled match: ' .. ht.selectedMatchID .. ' in event ' .. ht.selectedEventID )
	else
		HonTour.Error(1)
	end
end

function HonTour.LeaveLobby(sReason)
	if IsInScheduledMatch() then
		printdb('^r Recieved LeaveLobby Command - Attempt Leaving scheduled match: ' .. tostring(ht.selectedMatchID) .. ' in event ' .. tostring(ht.selectedEventID) )	
		ScheduledMatchCommand('leave')	
	else
		printdb('^r Recieved LeaveLobby Command - But you arent in a match. Clearing Lobby. : ' .. tostring(ht.selectedMatchID) .. ' in event ' .. tostring(ht.selectedEventID) )	
		--ScheduledMatchCommand('leave')	
		if GetWidget('hontour'):IsVisible() then
			UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'hontour', nil, nil, true, nil, 723)	
		end		
		HonTour.UnWatchLobbyInfo()	
	end
end

function HonTour.PromptLeaveLobby()
	local generic_ht_dialog_box_left_btn = GetWidget('generic_ht_dialog_box_left_btn')
	local generic_ht_dialog_box_right_btn = GetWidget('generic_ht_dialog_box_right_btn')
	generic_ht_dialog_box_left_btn:SetCallback('onclick', 
		function(this)		
			HonTour.LeaveLobby('leave_team')
		end
	)
	generic_ht_dialog_box_left_btn:RefreshCallbacks()
	generic_ht_dialog_box_right_btn:SetCallback('onclick', 
		function(this)
		end
	)
	generic_ht_dialog_box_right_btn:RefreshCallbacks()			
	Trigger('HTTriggerDialogBox', 'hontour_leave_team', 'hontour_leave_team', 'general_cancel', [[]], [[]], 'hontour_leave_team_conf', 'hontour_leave_team_conf_body', 'false', 'false')
end

local function UpdateLobbyPlayerInfo(localTeam, localIsCaptain, localIsReady)
	if (tonumber(HonTour.tScheduledMatchInfoTable[1])) and (tonumber(HonTour.tScheduledMatchInfoTable[1]) > 0) and (HonTour.tScheduledMatchInfoTable[2]) then
		for team = 0,1,1 do
			
			GetWidget('hontour_teamlist_'..team..'_teamname_label'):SetText(HonTour.tScheduledMatchInfoTable[11+team] or '?')
			GetWidget('hontour_teamlist_'..team..'_readyup_btn'):SetVisible(team == localTeam)
			GetWidget('hontour_teamlist_'..team..'_leave_btn'):SetVisible(team == localTeam)
			
			if AreAllPlayersReady(tonumber(HonTour.tScheduledMatchInfoTable[2]), team, true) then -- MatchID, team, includeCaptain
				--printdb('^g AreAllPlayersReady true ' .. HonTour.tScheduledMatchInfoTable[2] .. ' | ' .. team)
				GetWidget('hontour_bg_glow_'..team):SetVisible(true)
				GetWidget('hontour_teamlist_'..team..'_ready_label'):SetColor('#008f00')	
				GetWidget('hontour_teamlist_'..team..'_ready_label'):SetText(Translate('ht_team_ready'))
				-- Anyone can unready the team
				Trigger('HoNTourReadyUp'..team, 'ht_readyBtn_team_unready')	
				GetWidget('hontour_teamlist_'..team..'_readyup_btn'):SetEnabled(true)
				GetWidget('hontour_teamlist_'..team..'_readyup_btn'):SetCallback('onclick', function() ScheduledMatchCommand('unready') end )
				GetWidget('hontour_teamlist_'..team..'_readyup_btn'):RefreshCallbacks()		
			elseif AreAllPlayersReady(tonumber(HonTour.tScheduledMatchInfoTable[2]), team, false) then	
				--printdb('^y AreTeammatesReady true ' .. HonTour.tScheduledMatchInfoTable[2] .. ' | ' .. team)
				GetWidget('hontour_bg_glow_'..team):SetVisible(false)
				GetWidget('hontour_teamlist_'..team..'_ready_label'):SetColor('#008f00')	
				GetWidget('hontour_teamlist_'..team..'_ready_label'):SetText(Translate('ht_team_ready'))
				if (team == localTeam) then
					if (localIsCaptain) then
						if (localIsReady) then
							ScheduledMatchCommand('unready')
						end
						Trigger('HoNTourReadyUp'..team, 'ht_readyBtn_team_ready')
						GetWidget('hontour_teamlist_'..team..'_readyup_btn'):SetEnabled(true)
						GetWidget('hontour_teamlist_'..team..'_readyup_btn'):SetCallback('onclick', function() ScheduledMatchCommand('ready') end )
						GetWidget('hontour_teamlist_'..team..'_readyup_btn'):RefreshCallbacks()					
					else
						Trigger('HoNTourReadyUp'..team, 'ht_readyBtn_unready')	
						GetWidget('hontour_teamlist_'..team..'_readyup_btn'):SetEnabled(true)
						GetWidget('hontour_teamlist_'..team..'_readyup_btn'):SetCallback('onclick', function() ScheduledMatchCommand('unready') end )
						GetWidget('hontour_teamlist_'..team..'_readyup_btn'):RefreshCallbacks()					
					end	
				end
			else
				--printdb('^r AreAllPlayersReady false ' .. HonTour.tScheduledMatchInfoTable[2] .. ' | ' .. team)
				GetWidget('hontour_bg_glow_'..team):SetVisible(false)
				GetWidget('hontour_teamlist_'..team..'_ready_label'):SetColor('0.5 0.5 0.5 1')
				GetWidget('hontour_teamlist_'..team..'_ready_label'):SetText(Translate('ht_team_not_ready'))
				if (team == localTeam) then
					if (localIsCaptain) then
						Trigger('HoNTourReadyUp'..team, 'ht_readyBtn_waiting')
						GetWidget('hontour_teamlist_'..team..'_readyup_btn'):SetEnabled(false)
					elseif (localIsReady) then
						Trigger('HoNTourReadyUp'..team, 'ht_readyBtn_unready')
						GetWidget('hontour_teamlist_'..team..'_readyup_btn'):SetEnabled(true)
						GetWidget('hontour_teamlist_'..team..'_readyup_btn'):SetCallback('onclick', function() ScheduledMatchCommand('unready') end )
						GetWidget('hontour_teamlist_'..team..'_readyup_btn'):RefreshCallbacks()			
					else
						Trigger('HoNTourReadyUp'..team, 'ht_readyBtn_ready')
						GetWidget('hontour_teamlist_'..team..'_readyup_btn'):SetEnabled(true)
						GetWidget('hontour_teamlist_'..team..'_readyup_btn'):SetCallback('onclick', function() ScheduledMatchCommand('ready') end )
						GetWidget('hontour_teamlist_'..team..'_readyup_btn'):RefreshCallbacks()	
					end	
				end
			end
			
		end
	end
end

local function UpdateLobbyTeamInfo()

	local team, index, localTeam, localIsReady, playerTable
	local localIsCaptain = false
	for playerIndex = 1,10,1 do
		playerTable = ht.tPlayerInfoTable[playerIndex]
		if (playerIndex <= 5) then
			team = 0
			index = playerIndex - 1
		else
			team = 1
			index = playerIndex - 6
		end
		
		GetWidget('hontour_player_load_bg_'..team..'_'..index):SetVisible(false)
		
		if (playerTable) and (playerTable[1]) and (tonumber(playerTable[1])) and (tonumber(playerTable[1]) > 0) and (playerTable[2]) then			
			GetWidget('hontour_player_dot_'..team..'_'..index):SetVisible(true)	
			if (playerTable[5]) and AtoB(playerTable[5]) then -- Ready
				GetWidget('hontour_player_dot_'..team..'_'..index):SetTexture('/ui/fe2/lobby/matchmaking_server_dot_green.tga')
				GetWidget('hontour_player_name_'..team..'_'..index):SetColor('#008f00')
			else
				GetWidget('hontour_player_dot_'..team..'_'..index):SetTexture('/ui/fe2/lobby/matchmaking_server_dot_gray.tga')
				GetWidget('hontour_player_name_'..team..'_'..index):SetColor('0.5 0.5 0.5 1')	
			end
			if  (playerTable[1]) and (UIGetAccountID() == playerTable[1]) then
				localTeam = team
				localIsReady = AtoB(playerTable[5])
				ht.localIsReady	= AtoB(playerTable[5])
				if (index == 0) then
					localIsCaptain = true
					ht.localIsCaptain	= true
					GetWidget('hontour_player_name_'..team..'_'..index):SetText(playerTable[2]..'  ^y(Cpt)^*')	
				else
					ht.localIsCaptain	= false
					GetWidget('hontour_player_name_'..team..'_'..index):SetText(playerTable[2])	
				end	
			else
				if (index == 0) then
					GetWidget('hontour_player_name_'..team..'_'..index):SetText(playerTable[2]..'  ^y(Cpt)^*')	
				else
					GetWidget('hontour_player_name_'..team..'_'..index):SetText(playerTable[2])	
				end				
			end			
		else
			GetWidget('hontour_player_name_'..team..'_'..index):SetText('')
			GetWidget('hontour_player_dot_'..team..'_'..index):SetVisible(false)	
		end
	end		
	if (team) then
		UpdateLobbyPlayerInfo(localTeam, localIsCaptain, localIsReady)
	end
end

local function ScheduledMatchTime(_, gameStatus, countdown)
	--printdb('ScheduledMatchTime: ' .. tostring(gameStatus) .. ' ' .. countdown)
	if (not ht.isFindingServer) and (not ht.loadingVisible) then
		local hontourVisible = GetWidget('hontour'):IsVisible()
		GetWidget('hontour_shield_throbber'):FadeOut(50)
		GetWidget('hontour_countdown_label'):FadeIn(150)
		if (hontourVisible) then
			GetWidget('hontour_latch_shield_label_1'):FadeOut(50)
			GetWidget('hontour_latch_shield_label_2'):FadeOut(50)	
		else
			GetWidget('hontour_latch_shield_label_1'):FadeIn(150)
			GetWidget('hontour_latch_shield_label_2'):FadeIn(150)		
		end

		GetWidget('hontour_min_label'):FadeIn(150)
		GetWidget('hontour_sec_label'):FadeIn(150)
		if (gameStatus == '2') then		-- match expired
			GetWidget('mm_hontour_status'):FadeOut(50)
		elseif (gameStatus == '1') then	-- available to start
			GetWidget('mm_hontour_status'):FadeIn(150)
			GetWidget('hontour_startsin_label'):SetText(Translate('ht_expires'))
			GetWidget('hontour_countdown_label'):SetText(countdown)
			GetWidget('hontour_latch_shield_label_1'):SetText(countdown)
			GetWidget('hontour_latch_shield_label_2'):SetText(Translate('ht_s_expires'))
		elseif (gameStatus == '0') then	-- waiting to start
			GetWidget('mm_hontour_status'):FadeIn(150)
			GetWidget('hontour_startsin_label'):SetText(Translate('ht_startsin'))
			GetWidget('hontour_countdown_label'):SetText(countdown)
			GetWidget('hontour_latch_shield_label_1'):SetText(countdown)
			if (ht.localIsReady) then
				GetWidget('hontour_latch_shield_label_2'):SetText(Translate('ht_s_ready'))
			else
				GetWidget('hontour_latch_shield_label_2'):SetText(Translate('ht_s_startsin'))	
			end				
		end
	else
		GetWidget('mm_hontour_status'):FadeOut(50)
	end
end

local function UpdateLobbyLoadingInfo()
	local team, index, localTeam, localIsReady, playerTable
	for playerIndex = 1,10,1 do
		playerTable = ht.tPlayerInfoTable[playerIndex]
		if (playerIndex <= 5) then
			team = 0
			index = playerIndex - 1
		else
			team = 1
			index = playerIndex - 6
		end
		
		if (playerTable[1]) and (tonumber(playerTable[1])) and (tonumber(playerTable[1]) > 0) and (tonumber(playerTable[4]) > 0) and (tonumber(playerTable[4]) < 100) then	
			GetWidget('hontour_player_load_bg_'..team..'_'..index):SetVisible(true)
			GetWidget('hontour_player_load_'..team..'_'..index):SetWidth((playerTable[4])..'%')
		else
			GetWidget('hontour_player_load_bg_'..team..'_'..index):SetVisible(false)	
		end
	end	
end

local function ScheduledMatchInfo(_, updateType, matchInfo, ...) 	-- param2-11 = arg1-10 = player info
	-- printdb('^g ScheduledMatchInfo param0: ' .. updateType)			-- UpdateType SM_UPDATE = 0 SM_PLAYER_JOIN,  SM_PLAYER_LEAVE, SM_PLAYER_READY,  SM_PLAYER_UNREADY,  SM_LOADING_UPDATE  SM_FOUND_MATCH, SM_FOUND_SERVER, SM_REMOVED,
	-- printdb('^g ScheduledMatchInfo param1: ' .. matchInfo)			-- uiEventID|uiScheduledMatchID|sMatchTitle|StartDate|ExpirationDate|bVerifiedOnly|yGameType|yTeamSize|sMapName|sGameMode|sRegion|
	-- Players: uiAccountID|sName|nRank|yLoadingPercent|yReadyStatus|bVerified|uiChatNameColor
	--[[
	printdb('^g ScheduledMatchInfo param2: ' .. arg[1])	
	printdb('^g ScheduledMatchInfo param3: ' .. arg[2])	
	printdb('^g ScheduledMatchInfo param4: ' .. arg[3])	
	printdb('^g ScheduledMatchInfo param5: ' .. arg[4])	
	printdb('^g ScheduledMatchInfo param6: ' .. arg[5])	
	printdb('^g ScheduledMatchInfo param7: ' .. arg[6])	
	printdb('^g ScheduledMatchInfo param8: ' .. arg[7])	
	printdb('^g ScheduledMatchInfo param9: ' .. arg[8])	
	printdb('^g ScheduledMatchInfo param10: ' .. arg[9])	
	printdb('^g ScheduledMatchInfo param11: ' .. arg[10])	
	printdb('^g ScheduledMatchInfo param12: ' .. tostring(arg[11]))
	printdb('^g ScheduledMatchInfo param13: ' .. arg[12])
	--]]

	if GetCvarBool('ui_dev') then printTable(HonTour.tScheduledMatchInfoTable) end	
	
	ht.tPlayerInfoTable = {}
	
	for playerIndex = 1,10,1 do
		ht.tPlayerInfoTable[playerIndex] = explode('|', arg[playerIndex])
	end
	
	--[[
	for i, v in ipairs(ht.tPlayerInfoTable) do
		printdb('^y Player Info: ' .. i)
		if GetCvarBool('ui_dev') then printTable(v)	end
		printdb('^y ----')
	end	
	--]]
	
	--[[
	ht.tSpectatorInfoTable = {}
	if (arg[12]) and NotEmpty(arg[12]) then
		ht.tSpectatorInfoTable = explode('|', arg[12])	

		if (ht.tSpectatorInfoTable) and (#ht.tSpectatorInfoTable > 0) then
			local spectatorTooltip = ''
			for i, spectatorName in pairs(ht.tSpectatorInfoTable) do
				if Empty(spectatorTooltip) then
					spectatorTooltip = spectatorTooltip .. spectatorName
				else
					spectatorTooltip = spectatorTooltip .. '\\n' .. spectatorName
				end
			end
		
			GetWidget('hontour_spec_eye_box'):SetVisible(1)
			GetWidget('hontour_spec_eye'):SetCallback('onmouseover', function()
				Trigger('genericMainFloatingTip', 'true', '25h', '', 'gamelobby_spectators_header', spectatorTooltip, '', '', '3h', '-2h')
			end)
			GetWidget('hontour_spec_eye'):RefreshCallbacks()
		else
			GetWidget('hontour_spec_eye_box'):SetVisible(0)
		end
	else
		GetWidget('hontour_spec_eye_box'):SetVisible(0)
	end
	--]]
	
	if (updateType == '1') and (arg[11] == UIGetAccountID()) then
		printdb('^r ScheduledMatchInfo: I Joined a match')
		HonTour.ShowThrobber(false)	
		HonTour.WatchLobbyInfo()
	elseif (updateType == '2') and (arg[11] == UIGetAccountID()) then	
		printdb('^r ScheduledMatchInfo: I Left a match')
		if GetWidget('hontour'):IsVisible() then
			UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'hontour', nil, nil, true, nil, 723)	
		end		
		HonTour.UnWatchLobbyInfo()
		printdb('Recieved Leave Update - Leaving scheduled match: ' .. tostring(ht.selectedMatchID) .. ' in event ' .. tostring(ht.selectedEventID) )		
	end
	
	if (updateType == '5') or ht.loadingVisible then		
		UpdateLobbyLoadingInfo()	
	elseif (not ht.loadingVisible) then
		if (matchInfo) and NotEmpty(matchInfo) then
			HonTour.tScheduledMatchInfoTable = explode('|', matchInfo)
		end
		UpdateLobbyTeamInfo()
	end
end

local function ScheduledMatchServerInfo(_, updateEnum, extraInfo)
	-- printdb('^o^: ScheduledMatchServerInfo param0: ' .. tostring(updateEnum))	
	-- printdb('^o^: ScheduledMatchServerInfo param1: ' .. tostring(extraInfo))	
	ht.isFindingServer = true
	
	if (updateEnum == '6') then			-- SM_SERVER_LOADING 
		GetWidget('hontour_startsin_label'):SetText(Translate('hontour_finding_server'))
	elseif (updateEnum == '7') then		-- SM_SERVER_READY
		PlaySound('/shared/sounds/ui/menu/match_found.wav')
		GetWidget('hontour_startsin_label'):SetText(Translate('hontour_loading_server'))
		if GetWidget('hontour'):IsVisible() then
			UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'hontour', nil, nil, true, nil, 723)	
		end		
		HonTour.isMatchScheduled = false
	elseif (updateEnum == '8') then
		GetWidget('hontour_startsin_label'):SetText(Translate('hontour_ready_server'))		
	else
		GetWidget('hontour_startsin_label'):SetText(Translate('hontour_unknown_server'))
	end
	
	GetWidget('hontour_shield_throbber'):FadeIn(150)
	GetWidget('hontour_countdown_label'):FadeOut(50)
	GetWidget('hontour_latch_shield_label_1'):FadeOut(50)
	GetWidget('hontour_latch_shield_label_2'):FadeOut(50)
	GetWidget('hontour_min_label'):FadeOut(50)
	GetWidget('hontour_sec_label'):FadeOut(50)
end

local function ScheduledMatchLeave(leaveMessage, text1, text2, text3)
	local generic_ht_dialog_box_left_btn = GetWidget('generic_ht_dialog_box_left_btn')
	local generic_ht_dialog_box_right_btn = GetWidget('generic_ht_dialog_box_right_btn')
	generic_ht_dialog_box_left_btn:SetCallback('onclick', 
		function(this)		
			HonTour.LeaveLobby('ScheduledMatchLeave '..leaveMessage)
		end
	)
	generic_ht_dialog_box_left_btn:RefreshCallbacks()
	generic_ht_dialog_box_right_btn:SetCallback('onclick', 
		function(this)
		end
	)
	generic_ht_dialog_box_right_btn:RefreshCallbacks()			
	Trigger('HTTriggerDialogBox', text1, [[general_ok]], [[]], [[]], [[]], text2, text3, 'false', 'false')
	GetWidget('hontour_startsin_label'):SetText(Translate('ht_expired'))
	GetWidget('hontour_countdown_label'):SetText('00:00')	
end

local function ScheduledMatchLeaveTrigger(_, leaveMessage)
	printdb('^r^: ScheduledMatchLeave:^*^; ' .. tostring(leaveMessage) )
	if (leaveMessage == 'matchexpired') then
		ScheduledMatchLeave(leaveMessage, 'hontour_match_expired_header', 'hontour_match_expired_title', 'hontour_match_expired_desc')
		HonTour.isMatchScheduled = false
	elseif (leaveMessage == 'disconnected') then
		ScheduledMatchLeave(leaveMessage, 'hontour_match_dc_header', 'hontour_match_dc_title', 'hontour_match_dc_desc')
	elseif (leaveMessage == 'left') then
		ScheduledMatchLeave(leaveMessage, 'hontour_match_left_header', 'hontour_match_left_title', 'hontour_match_left_desc')
	elseif (leaveMessage == 'teamfull') then
		ScheduledMatchLeave(leaveMessage, 'hontour_match_teamfull_header', 'hontour_match_teamfull_title', 'hontour_match_teamfull_desc')
	elseif (leaveMessage == 'removed') then
		if (not IsInGame()) and (HonTour.isMatchScheduled or IsInScheduledMatch()) then
			ScheduledMatchLeave(leaveMessage, 'hontour_match_removed_header', 'hontour_match_removed_title', 'hontour_match_removed_desc')
		end
		HonTour.isMatchScheduled = false
	elseif (leaveMessage == 'forfeit_won') then
		if (not IsInGame()) then
			ScheduledMatchLeave(leaveMessage, 'hontour_match_forfeit_won_header', 'hontour_match_forfeit_won_title', 'hontour_match_forfeit_won_desc')
		end
		HonTour.isMatchScheduled = false
	elseif (leaveMessage == 'forfeit_loss') then
		if (not IsInGame()) then
			ScheduledMatchLeave(leaveMessage, 'hontour_match_forfeit_loss_header', 'hontour_match_forfeit_loss_title', 'hontour_match_forfeit_loss_desc')
		end
		HonTour.isMatchScheduled = false
	elseif (leaveMessage == 'disconnect_mm_connect_timeout') then
		Trigger('TriggerDialogBox', 'hontour_match_dc_header', [[general_ok]], [[]], [[]], [[]], 'hontour_mm_dc_error', 'hontour_mm_connect_timeout', 'false', 'false')
	elseif (leaveMessage == 'disconnect_mm_start_timeout') then
		Trigger('TriggerDialogBox', 'hontour_match_dc_header', [[general_ok]], [[]], [[]], [[]], 'hontour_mm_dc_error', 'hontour_mm_start_timeout', 'false', 'false')
	elseif (leaveMessage == 'disconnect_mm_player_left') then
		Trigger('TriggerDialogBox', 'hontour_match_dc_header', [[general_ok]], [[]], [[]], [[]], 'hontour_mm_dc_error', 'hontour_mm_player_left', 'false', 'false')
	else
	
	end
	HonTour:UpdatePlayNowButton()
end

local function LoadingVisible(_,isLoading)
	--printdb('LoadingVisible: ' .. tostring(isLoading) )
	ht.loadingVisible = AtoB(isLoading)
end

local function ChatStatus(_,isConnected)
	--printdb('ChatStatus: ' .. tostring(isConnected) )
	if  (AtoB(isConnected) == false) and (ht.chatStatus) and (IsInScheduledMatch()) then
		ScheduledMatchLeave('chat_dc', 'hontour_match_dc_header', 'hontour_match_dc_title', 'hontour_match_dc_desc')
	end
	ht.chatStatus = AtoB(isConnected)
end

local function TMMReadyStatus(_,isAvailable)
	HonTour:UpdatePlayNowButton()
end

local function HostErrorMessage(_, param0, param1, param2, param3)
	printdb('^r^:HostErrorMessage: ' .. tostring(param0) .. '  | ' .. tostring(param1) .. '  | ' .. tostring(param2) .. '  | ' .. tostring(param3) )
	if (IsInScheduledMatch()) then
		if (true) or (param3 == 'disconnect_mm_connect_timeout') or (param3 == 'disconnect_mm_connect_timeout') or (param3 == 'disconnect_mm_connect_timeout') or (param3 == 'disconnect_timeout') then
			ht.isFindingServer = false
			if (true) or (ht.localIsCaptain) then
				ScheduledMatchCommand('unready')
			end
		end
	end
end

local function ScheduledMatchSpectatorInfo(_, isSpectating)
	if (isSpectating) and AtoB(isSpectating) then
		HonTour.WatchLobbyInfo()
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'hontour', nil, nil, nil, nil, 722)
		printdb('Joining scheduled match as staff spectator')
	end
end

function HonTour.ClickCloseWindowBtn()
	UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'hontour', nil, nil, true, nil, 724)
	if (IsStaffSpecatingScheduledMatch) and (IsStaffSpecatingScheduledMatch()) then
		ScheduledMatchCommand('leave')
	end
end

function HonTour.WatchLobbyInfo()
	printdb('^g WatchLobbyInfo Is Enabled')
	ht.isFindingServer = false
	GetWidget('mm_hontour_status'):FadeIn(100)
	ht.panelWidget:RegisterWatch('HostErrorMessage', HostErrorMessage)
	ht.panelWidget:RegisterWatch('ChatStatus', ChatStatus)
	ht.panelWidget:RegisterWatch('LoadingVisible', LoadingVisible)
	ht.panelWidget:RegisterWatch('ScheduledMatchTime', ScheduledMatchTime)
end

function HonTour.UnWatchLobbyInfo()
	printdb('^g WatchLobbyInfo Is Disabled')
	ht.isFindingServer = false
	HonTour.isMatchScheduled = false
	GetWidget('mm_hontour_status'):FadeOut(50)
	ht.panelWidget:UnregisterWatch('HostErrorMessage')	
	ht.panelWidget:UnregisterWatch('ChatStatus')
	ht.panelWidget:UnregisterWatch('LoadingVisible')
	ht.panelWidget:UnregisterWatch('ScheduledMatchTime')
	ht.tPlayerInfoTable = {}
end

--  Enable HoN Tour
function HonTour.Initialize(isEnabled)
	if (isEnabled) then
		printdb('^g HoN Tour Is Enabled')
		ht.panelWidget:RegisterWatch('ScheduledMatchCommand', TriggerScheduledMatchCommand)
		ht.panelWidget:RegisterWatch('EventListing', EventListing)
		ht.panelWidget:RegisterWatch('TMMReadyStatus', TMMReadyStatus)
		ht.panelWidget:RegisterWatch('ScheduledMatchListing', ScheduledMatchListing)	
		ht.panelWidget:RegisterWatch('ScheduledMatchInfo', ScheduledMatchInfo)	
		ht.panelWidget:RegisterWatch('ScheduledMatchServerInfo', ScheduledMatchServerInfo)	
		ht.panelWidget:RegisterWatch('ScheduledMatchLeave', ScheduledMatchLeaveTrigger)	
		ht.panelWidget:RegisterWatch('ScheduledMatchSpectatorInfo', ScheduledMatchSpectatorInfo)
	else
		HonTour.isMatchScheduled = false
		printdb('^r HoN Tour Is Disabled')
		ht.panelWidget:UnregisterWatch('ScheduledMatchCommand')
		ht.panelWidget:UnregisterWatch('EventListing')
		ht.panelWidget:UnregisterWatch('TMMReadyStatus')
		ht.panelWidget:UnregisterWatch('ScheduledMatchListing')	
		ht.panelWidget:UnregisterWatch('ScheduledMatchInfo')	
		ht.panelWidget:UnregisterWatch('ScheduledMatchTime')
		ht.panelWidget:UnregisterWatch('ScheduledMatchLeave')
		ht.panelWidget:UnregisterWatch('LoadingVisible')
		ht.panelWidget:UnregisterWatch('HostErrorMessage')
		ht.panelWidget:UnregisterWatch('ChatStatus')
		ht.panelWidget:UnregisterWatch('ScheduledMatchServerInfo')
		ht.panelWidget:UnregisterWatch('ScheduledMatchSpectatorInfo')
	end
	HonTour:UpdatePlayNowButton()
end



