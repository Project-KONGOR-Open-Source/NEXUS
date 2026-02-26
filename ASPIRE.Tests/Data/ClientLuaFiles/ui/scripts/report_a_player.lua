---------------------------------------------------------- 
--	Name: 		RAP Script	            				--				
--  Copyright 2015 Frostburn Studios					--
----------------------------------------------------------

local interface, interfaceName = object, object:GetName()
local interfaceGameName = 'game'
local interfaceMainName = 'main'
RegisterScript2('Rap_Info', '30')
Rap_Info = {}
Rap_Info.Storage = {}
Rap_Info.Storage.ReportedPlayersThisSession = {}
Rap_Info.Storage.ReportedPlayersNamesInReportOrder = {}
Rap_Info.Storage.ReportedPlayers = {}
Rap_Info.Storage.ReportedButRemovedPlayers = {}
Rap_Info.Storage.PostgameRapVisibilityState = {}
Rap_Info.Storage.RapEntryEditTarget = {}
Rap_Info.Storage.HeroIconsByPlayerIndex = {}
Rap_Info.Storage.ScrollboxContents = {}
Rap_Info.Storage.currentStoredReportedPlayerIndex = 1
Rap_Info.Storage.currentMatchID = -1
Rap_Info.Storage.currentPlayerTeam = 0
Rap_Info.Storage.IsMatchTypeValid = {
	['1'] = false,         -- hosted game
	['6'] = false		   -- group bot game
}
Rap_Info.Storage.IsMapValid = {
	caldavar = true,
	midwars = true
}

Rap_Info.Storage.NUM_DAYS_RAP_TICKETS_VALID = 21
Rap_Info.Storage.NUM_DAYS_RAP_TICKETS_VALID_NEW = 10
Rap_Info.Storage.MaximumScrollboxHeight = '49.5h'

-- Stored in pixels per second
Rap_Info.Storage.entryGrowSpeed = interface:GetHeightFromString('100h')

local rap2Enable = GetCvarBool('cl_Rap2Enable')

-------------------
-- In game stuff --
function Rap_Info.ShowRapMenu(self, playerName)
	if (not GetCvarBool('ui_rap_enabled')) then return end
	
	local convertedPlayerName = StripClanTag(playerName)
	
	local storedRapInfo = Rap_Info.Storage.ReportedPlayers[convertedPlayerName]
	if(storedRapInfo ~= nil) then
		Rap_Info.Storage.SelectedRapType = storedRapInfo.rapType
	else
		Rap_Info.Storage.SelectedRapType = nil
	end
	
	Rap_Info.SetActiveReportType(Rap_Info.Storage.SelectedRapType)
	Rap_Info.Storage.RapReportingPlayer = convertedPlayerName
	Rap_Info.UpdateSelectedRapTypeForPlayerBeingReported()
	
	GetWidget('RAP_pop_player_label', interfaceGameName):SetText(convertedPlayerName)
	GetWidget('RAP_pop_player_label', interfaceGameName):SetColor(Game.PlayerColorsByIndex[Game.PlayerIndexByName[playerName]] or 'white')
	GetWidget('RAP_pop_hero_icon', interfaceGameName):SetTexture(Game.PlayerIconPathsByIndex[Game.PlayerIndexByName[playerName]] or '')
	
	GetWidget('RAP_pop', interfaceGameName):FadeIn(250)
	

end

function Rap_Info.HideRapMenu()
	
	GetWidget('RAP_pop', interfaceGameName):FadeOut(150)
	Rap_Info.Storage.SelectedRapType = nil
end

function Rap_Info.ReportPlayerFromRAPPop(rapType)
	
	Rap_Info.SetRapInfo(Rap_Info.Storage.RapReportingPlayer, rapType)
	Rap_Info.HideRapMenu()
end

function Rap_Info.UpdateSelectedRapTypeForPlayerBeingReported()

	local rapTypeDescriptionToShow = 'rap_info_initial'
	
	-- Hide all the stuff
	GetWidget('rap_info_initial', interfaceGameName):FadeOut(160)
	
	for i=1, 6, 1 do
		GetWidget('rap_info_details'..i, interfaceGameName):FadeOut(160)
		GetWidget('rap_submit_button_panel'.. i, interfaceGameName):FadeOut(160)
	end

	-- Show the stuff that should be visible
	if(Rap_Info.Storage.SelectedRapType ~= nil) then
		rapTypeDescriptionToShow = 'rap_info_details'..Rap_Info.Storage.SelectedRapType
		GetWidget('rap_submit_button_panel'.. Rap_Info.Storage.SelectedRapType, interfaceGameName):FadeIn(160)
	end
	
	GetWidget(rapTypeDescriptionToShow, interfaceGameName):FadeIn(160)
end

function Rap_Info.SetActiveReportType(rapType)

	-- interface:UICmd("GroupCall('rap_pop_offense_type_button_group', 'SetButtonState(0)')")
	groupfcall('rap_pop_offense_type_button_group', function(_, widget, _) widget:SetButtonState(0) end, 'game')

	if(rapType ~= nil) then
		Rap_Info.Storage.SelectedRapType = rapType
		GetWidget('rap_submit_button_panel'.. Rap_Info.Storage.SelectedRapType, interfaceGameName):FadeIn(160)

		GetWidget('rap_pop_offense_type_button'..rapType, interfaceGameName):SetButtonState(1)
	end

end

-- watches for and sets the current MatchID for in-game reporting
function Rap_Info.GameMatchID(widget, matchID)
	Rap_Info.Storage.currentMatchID = tonumber(matchID)
end
interface:RegisterWatch("GameMatchID", function(...) Rap_Info.GameMatchID(...) end)

-----------------------
-- After Match Stuff --
local function UpdatePostgameRapVisibility()

	if rap2Enable then
		-- new rap for sea
		for modInd, visState in pairs(Rap_Info.Storage.PostgameRapVisibilityState) do
			if(visState.widget) then
				local matchIDIsValid = visState.matchID and AtoN(visState.matchID) > 0
				local validName = visState.isValidName ~= nil and visState.isValidName
				local notReportedThisSession = visState.matchID ~= nil and visState.playerName ~= nil and Rap_Info.Storage.ReportedPlayersThisSession[visState.playerName..visState.matchID] ~= true
				local matchNotTooOld = tonumber(visState.gameUnixTime) == nil or (((tonumber(interface:UICmd('GetLocalServerTime()')) - (tonumber(visState.gameUnixTime)))/60/60/24) < Rap_Info.Storage.NUM_DAYS_RAP_TICKETS_VALID_NEW )
				local isValidTeam = visState.playerTeam == Rap_Info.Storage.currentPlayerTeam
				local isValidMap = Rap_Info.Storage.IsMapValid[visState.map] ~= nil and Rap_Info.Storage.IsMapValid[visState.map]
				local isValidMatchType = Rap_Info.Storage.IsMatchTypeValid[visState.matchType] == nil or Rap_Info.Storage.IsMatchTypeValid[visState.matchType]
				local isLatestMatch = visState.matchID == GetCvarString('_stats_latest_match_played')
				local rapEnabled = GetCvarBool('ui_rap_enabled')
				visState.widget:SetVisible(matchIDIsValid and validName and notReportedThisSession and matchNotTooOld and rapEnabled and isValidTeam and isValidMatchType and isValidMap and isLatestMatch)
			end
		end
	else
		-- old rap for naeu
		for modInd, visState in pairs(Rap_Info.Storage.PostgameRapVisibilityState) do
			if(visState.widget) then
				local matchIDIsValid = visState.matchID and AtoN(visState.matchID) > 0
				local validName = visState.isValidName ~= nil and visState.isValidName
				local notReportedThisSession = visState.matchID ~= nil and visState.playerName ~= nil and Rap_Info.Storage.ReportedPlayersThisSession[visState.playerName..visState.matchID] ~= true
				local matchNotTooOld = tonumber(visState.gameUnixTime) == nil or (((tonumber(interface:UICmd('GetLocalServerTime()')) - (tonumber(visState.gameUnixTime)))/60/60/24) < Rap_Info.Storage.NUM_DAYS_RAP_TICKETS_VALID )
				local rapEnabled = GetCvarBool('ui_rap_enabled')
				visState.widget:SetVisible(matchIDIsValid and validName and notReportedThisSession and matchNotTooOld and rapEnabled)
			end
		end
	end
end

function Rap_Info.SetVisibleRapButtonPlayer(self, team, index, playerName)	

	local modifiedIndex = (team-1)*5 + index
	Rap_Info.Storage.PostgameRapVisibilityState[modifiedIndex] = Rap_Info.Storage.PostgameRapVisibilityState[modifiedIndex] or {}
	Rap_Info.Storage.PostgameRapVisibilityState[modifiedIndex].widget = self
	Rap_Info.Storage.PostgameRapVisibilityState[modifiedIndex].playerName = playerName
	Rap_Info.Storage.PostgameRapVisibilityState[modifiedIndex].playerTeam = team
	Rap_Info.Storage.PostgameRapVisibilityState[modifiedIndex].isValidName = playerName ~= '' and StripClanTag(GetAccountName()) ~= StripClanTag(playerName)
	if playerName ~= '' and StripClanTag(GetAccountName()) == StripClanTag(playerName) then
		Rap_Info.Storage.currentPlayerTeam = team
	end
	UpdatePostgameRapVisibility()
end

function Rap_Info.SetVisibleRapButtonSummary(self, team, index, gameUnixTime, matchID, matchType, map)

	local modifiedIndex = (team-1)*5 + index
	Rap_Info.Storage.PostgameRapVisibilityState[modifiedIndex] = Rap_Info.Storage.PostgameRapVisibilityState[modifiedIndex] or {}
	Rap_Info.Storage.PostgameRapVisibilityState[modifiedIndex].widget = self
	Rap_Info.Storage.PostgameRapVisibilityState[modifiedIndex].gameUnixTime = gameUnixTime
	Rap_Info.Storage.PostgameRapVisibilityState[modifiedIndex].matchID = matchID
	Rap_Info.Storage.PostgameRapVisibilityState[modifiedIndex].matchType = matchType
	Rap_Info.Storage.PostgameRapVisibilityState[modifiedIndex].map = map
	UpdatePostgameRapVisibility()
end

function Rap_Info.ResetCurrentPlayerTeam()
	Rap_Info.Storage.currentPlayerTeam = 0
end

----------------------
-- Rap window stuff --
function Rap_Info.HideRapPostgameWindow()
	
	UpdatePostgameRapVisibility()
	GetWidget('rap_postgame_player_list', interfaceMainName):FadeOut(250)
	GetWidget('rap_postgame_player_list', interfaceMainName):UnregisterWatch('HostTime')

end

function Rap_Info.GetRapTextFromType(rapType)
	
	if(rapType > 0) then
		return Translate('report_a_player_abuse_type'.. rapType)
	else
		return 'Select the offense type'
	end
	
end

function Rap_Info.ShowRapPostgameWindow(timePlayed, matchID)

	if rap2Enable then
		if (not GetCvarBool('ui_rap_enabled')) then return end

		Rap_Info.Storage.processingText = ''

		local showWindow = false

		local timePlayedInSeconds = timePlayed / 1000
		local maxTimeFormatted = Rap_Info.GetTimestampStringFromSeconds(timePlayedInSeconds)

		for playerName,rapInfo in pairs(Rap_Info.Storage.ReportedPlayers) do

			if(rapInfo.shouldSubmit == true) then
				-- Show or create entries for each player reported that game
				if(not Rap_Info.Storage.ReportedPlayers[playerName].createdPostGameWidget) then
					-- default icon texture
					if (rapInfo.rapIcon == "") then
						rapInfo.rapIcon = "ui/elements/tip_mark_up.tga"
					end

					GetWidget('rap_postgame_report_parent', interfaceMainName):Instantiate('rap_postgame_entry', 'reportedPlayer', playerName, 'abuseIcon', rapInfo.rapIcon, 'playerIcon', rapInfo.playerIcon, 'playerColor', rapInfo.playerColor, 'maxTimeFormatted', maxTimeFormatted, 'maxTime', timePlayedInSeconds, 'editableRapType', tostring(rapInfo.isPostgame))
					Rap_Info.Storage.ReportedPlayers[playerName].createdPostGameWidget = true

					Rap_Info.Storage.ScrollboxContents[playerName] = {}

					Rap_Info.Storage.ScrollboxContents[playerName].entryPanel = 'rap_postgame_entry_'..playerName
					Rap_Info.Storage.ScrollboxContents[playerName].minimumEntryHeight = GetWidget('rap_postgame_entry_'..playerName, interfaceMainName):GetHeight()
					Rap_Info.Storage.ScrollboxContents[playerName].maximumEntryHeight = Rap_Info.Storage.ScrollboxContents[playerName].minimumEntryHeight + interface:GetHeightFromString('12h')
					if(rapInfo.isPostgame) then
						Rap_Info.Storage.ScrollboxContents[playerName].maximumEntryHeight = Rap_Info.Storage.ScrollboxContents[playerName].maximumEntryHeight + interface:GetHeightFromString('6.9h')
					end
					Rap_Info.Storage.ScrollboxContents[playerName].storedEntryHeight = Rap_Info.Storage.ScrollboxContents[playerName].maximumEntryHeight

					Rap_Info.Storage.ScrollboxContents[playerName].detailPanel = 'rap_postgame_entry_details_'..playerName
					Rap_Info.Storage.ScrollboxContents[playerName].minimumDetailHeight = GetWidget('rap_postgame_entry_details_'..playerName, interfaceMainName):GetHeight()
					Rap_Info.Storage.ScrollboxContents[playerName].maximumDetailHeight = Rap_Info.Storage.ScrollboxContents[playerName].minimumDetailHeight + interface:GetHeightFromString('12h')
					if(rapInfo.isPostgame) then
						Rap_Info.Storage.ScrollboxContents[playerName].maximumDetailHeight = Rap_Info.Storage.ScrollboxContents[playerName].maximumDetailHeight + interface:GetHeightFromString('6.9h')
					end
					Rap_Info.Storage.ScrollboxContents[playerName].storedDetailHeight = Rap_Info.Storage.ScrollboxContents[playerName].maximumDetailHeight

					Rap_Info.Storage.ScrollboxContents[playerName].shouldGrow = true

					if(rapInfo.isPostgame) then
						Rap_Info.ToggleEntryDetail(playerName)
						GetWidget('rap_postgame_entry_add_details_button_'..playerName, interfaceMainName):SetVisible(false)
					else
						Rap_Info.Storage.ScrollboxContents[playerName].growingState = -1
						GetWidget('rap_postgame_entry_details_'..playerName, interfaceMainName):SetHeight(3)
					end
				end
				Rap_Info.UpdateAbuseTypeAndTimestamp(playerName, true)
				showWindow = true
			else
				Echo('There was an error creating a new RAP widget for ' .. playerName)
			end

			matchID = rapInfo.matchID
		end

		if(showWindow) then
			GetWidget('rap_postgame_player_list', interfaceMainName):RegisterWatch('HostTime', Rap_Info.UpdateScrollerContents)
			GetWidget('rap_postgame_panel_title', interfaceMainName):SetText(Translate('rap_postagme_window_title_text') .. matchID)
			GetWidget('rap_postgame_player_list', interfaceMainName):FadeIn(250)
		end

		UpdatePostgameRapVisibility()
	else
		if (not GetCvarBool('ui_rap_enabled')) then return end

		Rap_Info.Storage.processingText = ''

		local showWindow = false

		local timePlayedInSeconds = timePlayed / 1000
		local maxTimeFormatted = Rap_Info.GetTimestampStringFromSeconds(timePlayedInSeconds)

		for playerName,rapInfo in pairs(Rap_Info.Storage.ReportedPlayers) do

			if(rapInfo.isPostgame) then
				GetWidget('rap_scroll_blocker'):SetNoClick(true)
			else
				GetWidget('rap_scroll_blocker'):SetNoClick(false)
			end

			if(rapInfo.shouldSubmit == true) then
				-- Show or create entries for each player reported that game
				if(not Rap_Info.Storage.ReportedPlayers[playerName].createdPostGameWidget) then
					-- default icon texture
					if (rapInfo.rapIcon == "") then
						rapInfo.rapIcon = "ui/elements/tip_mark_up.tga"
					end

					GetWidget('rap_scrollbox', interfaceMainName):Instantiate('rap_postgame_entry', 'reportedPlayer', playerName, 'abuseIcon', rapInfo.rapIcon, 'playerIcon', rapInfo.playerIcon, 'playerColor', rapInfo.playerColor, 'maxTimeFormatted', maxTimeFormatted, 'maxTime', timePlayedInSeconds, 'editableRapType', tostring(rapInfo.isPostgame))
					Rap_Info.Storage.ReportedPlayers[playerName].createdPostGameWidget = true

					Rap_Info.Storage.ScrollboxContents[playerName] = {}

					Rap_Info.Storage.ScrollboxContents[playerName].entryPanel = 'rap_postgame_entry_'..playerName
					Rap_Info.Storage.ScrollboxContents[playerName].minimumEntryHeight = GetWidget('rap_postgame_entry_'..playerName, interfaceMainName):GetHeight()
					Rap_Info.Storage.ScrollboxContents[playerName].maximumEntryHeight = Rap_Info.Storage.ScrollboxContents[playerName].minimumEntryHeight + interface:GetHeightFromString('12h')
					if(rapInfo.isPostgame) then
						Rap_Info.Storage.ScrollboxContents[playerName].maximumEntryHeight = Rap_Info.Storage.ScrollboxContents[playerName].maximumEntryHeight + interface:GetHeightFromString('6.9h')
					end
					Rap_Info.Storage.ScrollboxContents[playerName].storedEntryHeight = Rap_Info.Storage.ScrollboxContents[playerName].maximumEntryHeight

					Rap_Info.Storage.ScrollboxContents[playerName].detailPanel = 'rap_postgame_entry_details_'..playerName
					Rap_Info.Storage.ScrollboxContents[playerName].minimumDetailHeight = GetWidget('rap_postgame_entry_details_'..playerName, interfaceMainName):GetHeight()
					Rap_Info.Storage.ScrollboxContents[playerName].maximumDetailHeight = Rap_Info.Storage.ScrollboxContents[playerName].minimumDetailHeight + interface:GetHeightFromString('12h')
					if(rapInfo.isPostgame) then
						Rap_Info.Storage.ScrollboxContents[playerName].maximumDetailHeight = Rap_Info.Storage.ScrollboxContents[playerName].maximumDetailHeight + interface:GetHeightFromString('6.9h')
					end
					Rap_Info.Storage.ScrollboxContents[playerName].storedDetailHeight = Rap_Info.Storage.ScrollboxContents[playerName].maximumDetailHeight

					Rap_Info.Storage.ScrollboxContents[playerName].shouldGrow = true

					if(rapInfo.isPostgame) then
						Rap_Info.ToggleEntryDetail(playerName)
						GetWidget('rap_postgame_entry_add_details_button_'..playerName, interfaceMainName):SetVisible(false)
					else
						Rap_Info.Storage.ScrollboxContents[playerName].growingState = -1
						GetWidget('rap_postgame_entry_details_'..playerName, interfaceMainName):SetHeight(3)
					end
				end
				Rap_Info.UpdateAbuseTypeAndTimestamp(playerName, true)
				showWindow = true
			else
				Echo('There was an error creating a new RAP widget for ' .. playerName)
			end

			matchID = rapInfo.matchID
		end

		if(showWindow) then
			GetWidget('rap_postgame_player_list', interfaceMainName):RegisterWatch('HostTime', Rap_Info.UpdateScrollerContents)
			GetWidget('rap_postgame_panel_title', interfaceMainName):SetText(Translate('rap_postagme_window_title_text') .. matchID)
			GetWidget('rap_postgame_player_list', interfaceMainName):FadeIn(250)
		end

		UpdatePostgameRapVisibility()
		Rap_Info.ResetScrollboxContents()
	end
end

function Rap_Info.UpdateAbuseTypeAndTimestamp(reportedPlayer, firstUpdate)

	local timestampString = Rap_Info.GetTimestampStringFromSeconds(Rap_Info.Storage.ReportedPlayers[reportedPlayer].timeStamp)
	local abuseTypeAndTimestamp = Rap_Info.GetRapTextFromType(Rap_Info.Storage.ReportedPlayers[reportedPlayer].rapType) .. ' @ ' .. timestampString

	GetWidget('rap_postgame_abuse_type_and_timestamp_'.. reportedPlayer):SetText(abuseTypeAndTimestamp)
	GetWidget('rap_timestamp_slider_value_'.. reportedPlayer):SetText(timestampString)
	
	if(firstUpdate) then
		GetWidget('rap_timestamp_slider_'.. reportedPlayer):UICmd('SetValue('..Rap_Info.Storage.ReportedPlayers[reportedPlayer].timeStamp..');')
	end
	
end

function Rap_Info.GetTimestampStringFromSeconds(timestamp)
	
	local hours, minutes, seconds = math.floor(timestamp / 60 / 60),  math.floor(timestamp / 60 % 60), math.floor(timestamp % 60)
		
	return string.format("%02d", hours) .. ':' .. string.format("%02d", minutes) .. ':' .. string.format("%02d", seconds)
end

----------------------------
-- Individual entry stuff --
function Rap_Info.ShowEntryDetail(playerName)
	
	Rap_Info.Storage.ScrollboxContents[playerName].growingState = 1
	Rap_Info.Storage.ScrollboxContents[playerName].shouldGrow = false
end

function Rap_Info.HideEntryDetail(playerName)
	
	Rap_Info.Storage.ScrollboxContents[playerName].growingState = -1
	Rap_Info.Storage.ScrollboxContents[playerName].shouldGrow = true
	
	GetWidget(Rap_Info.Storage.ScrollboxContents[playerName].detailPanel, interfaceMainName):FadeOut(100)
end

function Rap_Info.ToggleEntryDetail(playerName)
	
	if(Rap_Info.Storage.ScrollboxContents[playerName].shouldGrow) then
		Rap_Info.ShowEntryDetail(playerName)
	else
		Rap_Info.HideEntryDetail(playerName)
	end

	if not rap2Enable then
		-- Sleep before call so that the onshows can resolve to a proper size
		GetWidget('rap_sleeper'):Sleep(1, function()
			interface:UICmd("Trigger('rap_postgame_scrolled');")
		end)
	end
end

function Rap_Info.RemoveRapEntry(playerName)
	
	-- Store this term because I can't find a way to pass this information through otherwise
	Rap_Info.Storage.RemoveDialogPlayer = playerName;
	interface:UICmd("Trigger('rap_confirm_remove_entry', 'rap_confirm_cancel_header', 'options_button_ok', 'general_cancel', '', '".. Translate('rap_confirm_cancel_body') .. ' ' ..playerName.."?');")
	
end

function Rap_Info.RemoveRapEntryConfirmed()
	
	local playerName = Rap_Info.Storage.RemoveDialogPlayer
	Rap_Info.Storage.RemoveDialogPlayer = ''
	if(GetWidget('rap_postgame_entry_'.. playerName, interfaceMainName) ~= nil) then
		GetWidget('rap_postgame_entry_'.. playerName, interfaceMainNAme):SetVisible(false)
		Rap_Info.Storage.ScrollboxContents[playerName] = nil
	end
	
	Rap_Info.Storage.ReportedPlayers[playerName].shouldSubmit = false
	
	local allAreSubmittedOrDeleted = true
	for playerName,rapInfo in pairs(Rap_Info.Storage.ReportedPlayers) do
		if(rapInfo.shouldSubmit) then
			allAreSubmittedOrDeleted = false
			break
		end
	end
	
	if(allAreSubmittedOrDeleted) then
		Rap_Info.CancelRapReport(false)
	end

	if not rap2Enable then
		Rap_Info.ResetScrollboxContents()
	end
end

function Rap_Info.RemoveRapEntryCanceled()
	
	Rap_Info.Storage.RemoveDialogPlayer = ''

end

function Rap_Info.UpdateEditReportTimestamp(self, reportedPlayer, this)

	local timeStamp = this
	Rap_Info.Storage.ReportedPlayers[reportedPlayer].timeStamp = timeStamp
	Rap_Info.UpdateAbuseTypeAndTimestamp(reportedPlayer, false)
end

function Rap_Info.HideTextboxIfOutsideScroller(self, associatedPlayer)

	local scrollValue =  tonumber(GetWidget('rap_scrollbox_vscroll'):UICmd([[GetScrollbarValue()]])) or 0
	local miny = GetWidget('rap_scrollbox'):GetAbsoluteY()
	local maxy = miny + GetWidget('rap_scrollbox'):GetHeight() - 13  -- minus constant because the size is *just* larger than what looks good to clip at
	local myScrolledPosition = self:GetAbsoluteY() - scrollValue
	if(myScrolledPosition < miny or myScrolledPosition > maxy) then
		self:SetVisible(false)
	elseif (Rap_Info.Storage.ScrollboxContents[associatedPlayer].lastGrowingState ~= 1 ) then
		self:SetVisible(true)
	end
end

--------------------
-- Scroller stuff --
local function DoRapPostgameScroll()

	if (Rap_Info.Storage.scrollState == 1) then
		GetWidget('rap_scrollbox_vscroll'):UICmd([[SetScrollbarValue(GetScrollbarValue()+1)]])
		GetWidget('rap_scrollbox', interfaceMainName):Sleep(1, function() DoRapPostgameScroll() end)
	elseif (Rap_Info.Storage.scrollState == -1) then
		GetWidget('rap_scrollbox_vscroll'):UICmd([[SetScrollbarValue(GetScrollbarValue()-1)]])
		GetWidget('rap_scrollbox', interfaceMainName):Sleep(1, function() DoRapPostgameScroll() end)
	end

	interface:UICmd("Trigger('rap_postgame_scrolled');")

end

function Rap_Info.DoRapPostgameScrollDown()
	GetWidget('rap_scrollbox_vscroll'):UICmd([[SetVScrollbarStep('1')]])
	Rap_Info.Storage.scrollState = 1
	DoRapPostgameScroll()
end

function Rap_Info.DoRapPostgameScrollUp()
	GetWidget('rap_scrollbox_vscroll'):UICmd([[SetVScrollbarStep('1')]])
	Rap_Info.Storage.scrollState = -1
	DoRapPostgameScroll()
end

function Rap_Info.StopRapPostgameScroll()
	Rap_Info.Storage.scrollState = 0
end

function Rap_Info.InitRapScroller(sourceWidget)
	sourceWidget:SetCallback('onslide', function()
		local scrollValue = tonumber(GetWidget('rap_scrollbox_vscroll'):UICmd([[GetScrollbarValue()]])) or 0
		local scrollMax = tonumber(GetWidget('rap_scrollbox_vscroll'):UICmd([[GetScrollbarMaxValue()]])) or 500
		local scrollMin = tonumber(GetWidget('rap_scrollbox_vscroll'):UICmd([[GetScrollbarMinValue()]])) or 1
		if (scrollValue > scrollMin) then
			Set('rap_canScrollUp', 'true', 'bool')
			GetWidget('rap_fade_top', interfaceMainName):FadeIn(250)
		else
			Set('rap_canScrollUp', 'false', 'bool')
			GetWidget('rap_fade_top', interfaceMainName):FadeOut(250)
		end
		if (scrollValue < scrollMax) then
			Set('rap_canScrollDown', 'true', 'bool')
			GetWidget('rap_fade_bottom', interfaceMainName):FadeIn(250)
		else
			Set('rap_canScrollDown', 'false', 'bool')
			GetWidget('rap_fade_bottom', interfaceMainName):FadeOut(250)
		end
	end)

	Rap_Info.ResetScrollboxContents()

	sourceWidget:RefreshCallbacks()
end

function Rap_Info.ResetScrollboxContents()

	GetWidget('rap_sleeper', interfaceMainName):Sleep(1, function()

		GetWidget('rap_scrollbox', interfaceMainName):UICmd([[SetClipAreaToChild()]])

	end)
end

function Rap_Info.UpdateScrollerContents(self, hostTime)
	
	local deltaTime = hostTime - (Rap_Info.Storage.lastTime or hostTime)
	Rap_Info.Storage.lastTime = hostTime
	-- Convert to seconds
	deltaTime = deltaTime / 1000.0
	
	-- Update the sizes and contents of each individual entry
	Rap_Info.UpdateIndividualEntries(deltaTime)
	
	-- Find the height of everything and resize the container
	Rap_Info.ResizeScrollboxAndContainer()
	
end

function Rap_Info.ResizeScrollboxAndContainer()
	
	local totalHeight = 0	
	for key,value in pairs(Rap_Info.Storage.ScrollboxContents) do
		local panel = value.entryPanel
		if(GetWidget(panel, interfaceMainName):IsVisible()) then
			totalHeight = totalHeight + GetWidget(panel, interfaceMainName):GetHeight()
		end
	end

	if rap2Enable then
		if(totalHeight < interface:GetHeightFromString(Rap_Info.Storage.MaximumScrollboxHeight)) then
			GetWidget('rap_postgame_panel', interfaceMain):SetHeight(totalHeight + interface:GetHeightFromString('9.5h'))
		else
			GetWidget('rap_postgame_panel', interfaceMain):SetHeight(interface:GetHeightFromString(Rap_Info.Storage.MaximumScrollboxHeight) + interface:GetHeightFromString('9.5h'))
		end
	else
		if(totalHeight < interface:GetHeightFromString(Rap_Info.Storage.MaximumScrollboxHeight)) then
			GetWidget('rap_scrollbox', interfaceMain):SetHeight(totalHeight)
			GetWidget('rap_postgame_panel', interfaceMain):SetHeight(totalHeight + interface:GetHeightFromString('9.5h'))
		else
			GetWidget('rap_scrollbox', interfaceMain):SetHeight(Rap_Info.Storage.MaximumScrollboxHeight)
			GetWidget('rap_postgame_panel', interfaceMain):SetHeight(interface:GetHeightFromString(Rap_Info.Storage.MaximumScrollboxHeight) + interface:GetHeightFromString('9.5h'))
		end

		-- This will update with data from last frame, but since we do this every frame, there's no need to wrap it in a sleep
		GetWidget('rap_scrollbox', interfaceMainName):UICmd([[SetClipAreaToChild()]])
	end
end

function Rap_Info.UpdateIndividualEntries(deltaTime)
	
	local dontAllowSubmit = false
	
	for playerName,entry in pairs(Rap_Info.Storage.ScrollboxContents) do
	
		if(entry.growingState == 1 and GetWidget(entry.detailPanel, interfaceMainName):GetHeight() < entry.maximumDetailHeight) then
			
			Rap_Info.Storage.ScrollboxContents[playerName].lastGrowingState = 1
			local deltaPixels = Rap_Info.Storage.entryGrowSpeed * deltaTime
			
			-- Don't let the outer panel overshoot the target
			-- if(Rap_Info.Storage.ScrollboxContents[playerName].storedEntryHeight + deltaPixels > entry.maximumEntryHeight) then
				-- deltaPixels = entry.maximumEntryHeight - Rap_Info.Storage.ScrollboxContents[playerName].storedEntryHeight
			-- end
			
			-- grow outer panel
			-- Rap_Info.Storage.ScrollboxContents[playerName].storedEntryHeight = Rap_Info.Storage.ScrollboxContents[playerName].storedEntryHeight + deltaPixels
			-- GetWidget(entry.entryPanel, interfaceMainName):SetHeight(Rap_Info.Storage.ScrollboxContents[playerName].storedEntryHeight)
			
			-- Don't let the inner panel overshoot the target
			if(Rap_Info.Storage.ScrollboxContents[playerName].storedDetailHeight + deltaPixels > entry.maximumDetailHeight) then
				deltaPixels = entry.maximumDetailHeight - Rap_Info.Storage.ScrollboxContents[playerName].storedDetailHeight
			end
			
			-- grow inner panel
			Rap_Info.Storage.ScrollboxContents[playerName].storedDetailHeight = Rap_Info.Storage.ScrollboxContents[playerName].storedDetailHeight + deltaPixels
			GetWidget(entry.detailPanel, interfaceMainName):SetHeight(Rap_Info.Storage.ScrollboxContents[playerName].storedDetailHeight)
			
			GetWidget(entry.entryPanel, interfaceMainName):SetHeight(GetWidget(entry.detailPanel, interfaceMainName):GetHeight() + interface:GetHeightFromString('6.9h'))

			
			-- Hide the description text since clip doesn't clip it properly
			if(GetWidget('rap_textbox_panel_'..playerName, interfaceMainName):GetY() <= GetWidget(entry.detailPanel, interfaceMainName):GetHeight()) then
				GetWidget('rap_postgame_entry_description_'..playerName, interfaceMainName):SetVisible(true)
			end
			
		elseif(entry.growingState == -1 and GetWidget(entry.detailPanel, interfaceMainName):GetHeight() > entry.minimumDetailHeight) then
			
			Rap_Info.Storage.ScrollboxContents[playerName].lastGrowingState = -1
			local deltaPixels = Rap_Info.Storage.entryGrowSpeed * deltaTime
			
			-- Don't let it overshoot the target
			-- if(Rap_Info.Storage.ScrollboxContents[playerName].storedDetailHeight - deltaPixels < entry.minimumDetailHeight) then
				-- deltaPixels = Rap_Info.Storage.ScrollboxContents[playerName].storedDetailHeight - entry.minimumDetailHeight
			-- end
			
			-- shrink outer panel
			-- Rap_Info.Storage.ScrollboxContents[playerName].storedEntryHeight = Rap_Info.Storage.ScrollboxContents[playerName].storedEntryHeight - deltaPixels
			-- GetWidget(entry.entryPanel, interfaceMainName):SetHeight(Rap_Info.Storage.ScrollboxContents[playerName].storedEntryHeight)
			
			-- Don't let the inner panel overshoot the target
			if(Rap_Info.Storage.ScrollboxContents[playerName].storedDetailHeight - deltaPixels < entry.minimumDetailHeight) then
				deltaPixels = Rap_Info.Storage.ScrollboxContents[playerName].storedDetailHeight - entry.minimumDetailHeight
			end
			
			-- shrink inner panel
			Rap_Info.Storage.ScrollboxContents[playerName].storedDetailHeight = Rap_Info.Storage.ScrollboxContents[playerName].storedDetailHeight - deltaPixels
			GetWidget(entry.detailPanel, interfaceMainName):SetHeight(Rap_Info.Storage.ScrollboxContents[playerName].storedDetailHeight)
			
			GetWidget(entry.entryPanel, interfaceMainName):SetHeight(GetWidget(entry.detailPanel, interfaceMainName):GetHeight() + interface:GetHeightFromString('6.9h'))

			
			-- Hide the description text since clip doesn't clip it properly
			if(GetWidget('rap_textbox_panel_'..playerName, interfaceMainName):GetY() >= GetWidget(entry.detailPanel, interfaceMainName):GetHeight()) then
				GetWidget('rap_postgame_entry_description_'..playerName, interfaceMainName):SetVisible(false)
			end
			
		elseif(entry.growingState ~= 0) then
			
			if(entry.growingState == 1) then
				GetWidget(entry.detailPanel, interfaceMainName):FadeIn(250)
			end
			
			Rap_Info.Storage.ScrollboxContents[playerName].growingState = 0
		end
		
		
		if(Rap_Info.Storage.ReportedPlayers[playerName].rapType == 0) then
			dontAllowSubmit = true
		end
		
	end
	
	
	-- Don't let submit work if someone is missing a rap type since postgame reported people start without one
	if(dontAllowSubmit) then
		interface:UICmd("Trigger('rap_submission_processing', '" .. Translate('rap_cant_submit').."');")
		GetWidget('rap_postgame_submit_buton', interfaceMainName):SetEnabled(false)
	else
		interface:UICmd("Trigger('rap_submission_processing', '" .. Translate('general_submit').."');")
		GetWidget('rap_postgame_submit_buton', interfaceMainName):SetEnabled(true)
	end
	
end

----------------------
-- Submission stuff --
function Rap_Info.SubmitRapReport()

	if rap2Enable then
		-- new rap for sea
		for playerName, rapInfo in pairs(Rap_Info.Storage.ReportedPlayers) do
			rapInfo.description = GetWidget('rap_postgame_entry_description_' .. playerName):GetValue()
			if(rapInfo.description == Translate('rap_final_error_1')) then
				rapInfo.description = ''
			end
			-- Don't submit if the user canceled the report
			if(rapInfo.shouldSubmit == true) then
				-- submit info to new rap system
				SubmitForm('RapReport', 'reporter_id', Client.GetAccountID(), 'cookie', Client.GetCookie(), 'reportee_name', StripClanTag(rapInfo.reportedPlayer), 'reason_type', rapInfo.rapType, 'report_description', string.gsub(rapInfo.description, "'", "\\\'") .. "'")
				return
			end
		end
	else
		-- old rap for naeu
		local submissionEntryNames = ''
		local submissionEntryMatchIDs = ''
		local submissionEntryReportTypes = ''
		local submissionEntryDescriptions = ''
		local submissionEntryTimeStamps = ''
		for playerName, rapInfo in pairs(Rap_Info.Storage.ReportedPlayers) do
			rapInfo.description = GetWidget('rap_postgame_entry_description_' .. playerName):GetValue()
			if(rapInfo.description == Translate('rap_final_error_1')) then
				rapInfo.description = ''
			end
			-- Don't submit if the user canceled the report
			if(rapInfo.shouldSubmit == true) then
				submissionEntryNames = submissionEntryNames .. ", 'offenderName[]', '" .. rapInfo.reportedPlayer .. "'"
				submissionEntryMatchIDs = submissionEntryMatchIDs .. ", 'match_id[]', '" .. rapInfo.matchID .. "'"
				-- the old and new rapType are exchanged between 5 and 6
				-- for new 5 is afk, 6 is badnickname, for old 5 is badnickname, 6 is afk
				local rapType = rapInfo.rapType
				if rapType == 5 then
					rapType = 6
				elseif rapType == 6 then
					rapType = 5
				end
				submissionEntryReportTypes = submissionEntryReportTypes .. ", 'type[]', '" .. rapType .. "'"
				submissionEntryDescriptions = submissionEntryDescriptions .. ", 'body[]', '" .. string.gsub(rapInfo.description, "'", "\\\'") .. "'"
				submissionEntryTimeStamps = submissionEntryTimeStamps .. ", 'time[]', '" .. math.floor(rapInfo.timeStamp/60) .. "'"
			end
		end
		if(submissionEntryNames ~= '') then
			-- web submission logic here
			local formString = [[SubmitForm('ReportAbuse2', 'account_id', GetAccountID(), 'cookie', GetCookie()]] .. submissionEntryNames .. submissionEntryMatchIDs .. submissionEntryReportTypes .. submissionEntryDescriptions .. submissionEntryTimeStamps ..[[);]]
			UIManager.GetInterface(interfaceMainName):UICmd(formString)
		end
	end
end

function Rap_Info.SubmissionStatus(...)
	-- new rap for sea
	Rap_Info.Storage.processingText = Rap_Info.Storage.processingText .. '.'

	if(Rap_Info.Storage.processingText:len() > 4) then
		Rap_Info.Storage.processingText = '.'
	end

	if(arg[2] == '1') then
		interface:UICmd("Trigger('rap_submission_processing', '".. Rap_Info.Storage.processingText .."');")
	elseif(arg[2] == '2') then
		interface:UICmd("Trigger('rap_submission_processing', '"..Translate('general_submit') .."');")
		Rap_Info.CancelRapReport(true)
	elseif(arg[2] == '3') then
		interface:UICmd("Trigger('rap_submission_processing', '"..Translate('general_submit') .."');")
		Rap_Info.CancelRapReport(true)
	else
		Echo('Unhandled rap submission id: ' .. arg[2])
		interface:UICmd("Trigger('rap_submission_processing', '"..Translate('general_submit') .."');")
	end
end
interface:RegisterWatch('RapReportStatus', Rap_Info.SubmissionStatus)

function Rap_Info.SubmissionStatus(...)
	-- old rap for naeu
	Rap_Info.Storage.processingText = Rap_Info.Storage.processingText .. '.'

	if(Rap_Info.Storage.processingText:len() > 4) then
		Rap_Info.Storage.processingText = '.'
	end

	if(arg[2] == '1') then
		interface:UICmd("Trigger('rap_submission_processing', '".. Rap_Info.Storage.processingText .."');")
	elseif(arg[2] == '2') then
		interface:UICmd("Trigger('rap_submission_processing', '"..Translate('general_submit') .."');")

		-- Now that they're sent, we'll 'cancel' them so they are cleaned up
		Rap_Info.CancelRapReport(true)
		GetWidget('rap_postgame_submission_status', interfaceMainName):DoEventN(1)
	elseif(arg[2] == '3') then
		interface:UICmd("Trigger('rap_submission_processing', '"..Translate('general_submit') .."');")

		-- Now that they're sent, we'll 'cancel' them so they are cleaned up
		Rap_Info.CancelRapReport(true)
		GetWidget('rap_postgame_submission_status', interfaceMainName):DoEventN(1)
	else
		Echo('Unhandled rap submission id: ' .. arg[2])

		interface:UICmd("Trigger('rap_submission_processing', '"..Translate('general_submit') .."');")
		GetWidget('rap_postgame_submission_status', interfaceMainName):DoEventN(2)
	end

end
interface:RegisterWatch('ReportAbuseStatus2', Rap_Info.SubmissionStatus)

function Rap_Info.SubmissionResult(...)
	if rap2Enable then
		-- new rap for sea
		if arg[2] == 'false' then
			GetWidget('rap_postgame_submission_status', interfaceMainName):DoEventN(2)
		else
			if arg[3] == '1' then
				GetWidget('rap_postgame_submission_status', interfaceMainName):DoEventN(1)
			elseif arg[3] == '2' then
				GetWidget('rap_postgame_submission_status', interfaceMainName):DoEventN(2)
			elseif arg[3] == '3' then
				GetWidget('rap_postgame_submission_status', interfaceMainName):DoEventN(3)
			end
		end
	end
end
interface:RegisterWatch('RapReportResult', Rap_Info.SubmissionResult)


function Rap_Info.CancelRapReport(success)
	
	-- Delete all widgets for the reports that have been canceled
	for playerName, rapInfo in pairs(Rap_Info.Storage.ReportedPlayers) do
		GetWidget('rap_postgame_entry_'.. playerName, interfaceMainName):Destroy()
		Rap_Info.Storage.ReportedPlayers[playerName] = nil
		Rap_Info.Storage.ScrollboxContents[playerName] = nil
		Rap_Info.StoreReportedPlayerThisSession(playerName, rapInfo.matchID, success)
	end
	
	Rap_Info.HideRapPostgameWindow()
end


--------------------
-- Misc rap stuff --
function Rap_Info.SetRapInfo(reportedPlayer, rapType)
	if (Rap_Info.Storage.currentMatchID > 0) then
		Rap_Info.Storage.ReportedPlayers[reportedPlayer] = {}
		Rap_Info.Storage.ReportedPlayers[reportedPlayer].reportedPlayer = reportedPlayer
		Rap_Info.Storage.ReportedPlayers[reportedPlayer].timeStamp = math.floor(Game.lastMatchTime)
		Rap_Info.Storage.ReportedPlayers[reportedPlayer].rapType = rapType
		Rap_Info.Storage.ReportedPlayers[reportedPlayer].shouldSubmit = true
		Rap_Info.Storage.ReportedPlayers[reportedPlayer].description = ''
		Rap_Info.Storage.ReportedPlayers[reportedPlayer].matchID = Rap_Info.Storage.currentMatchID
		Rap_Info.Storage.ReportedPlayers[reportedPlayer].playerIcon = Game.PlayerIconPathsByIndex[Game.PlayerIndexByName[reportedPlayer]] or ''
		Rap_Info.Storage.ReportedPlayers[reportedPlayer].playerColor = Game.PlayerColorsByIndex[Game.PlayerIndexByName[reportedPlayer]] or 'white'
		Rap_Info.Storage.ReportedPlayers[reportedPlayer].rapIcon = GetWidget('rap_pop_offense_type_button_icon'.. rapType, interfaceGameName):GetTexture()
		Rap_Info.Storage.ReportedPlayers[reportedPlayer].isPostgame = false

		Rap_Info.StoreReportedPlayerThisSession(reportedPlayer, Rap_Info.Storage.currentMatchID, true)

	end
end

---------------------------
-- Match stats reporting --
function Rap_Info.MatchStatsReport(team, index, reportedPlayer)

	local modifiedIndex = (team-1)*5 + index
	local color = interface:UICmd("GetColorFromPosition('" .. modifiedIndex .."')")

	Rap_Info.Storage.ReportedPlayers[reportedPlayer] = {}
	Rap_Info.Storage.ReportedPlayers[reportedPlayer].reportedPlayer = reportedPlayer
	Rap_Info.Storage.ReportedPlayers[reportedPlayer].timeStamp = 0
	Rap_Info.Storage.ReportedPlayers[reportedPlayer].rapType = 0
	Rap_Info.Storage.ReportedPlayers[reportedPlayer].shouldSubmit = true
	Rap_Info.Storage.ReportedPlayers[reportedPlayer].description = ''
	Rap_Info.Storage.ReportedPlayers[reportedPlayer].matchID = GetCvarInt("_stats_last_match_id")
	Rap_Info.Storage.ReportedPlayers[reportedPlayer].playerIcon = Rap_Info.Storage.HeroIconsByPlayerIndex[modifiedIndex] or ''
	Rap_Info.Storage.ReportedPlayers[reportedPlayer].playerColor = color or ''
	Rap_Info.Storage.ReportedPlayers[reportedPlayer].rapIcon = ''
	Rap_Info.Storage.ReportedPlayers[reportedPlayer].isPostgame = true
	
	Rap_Info.StoreReportedPlayerThisSession(reportedPlayer, Rap_Info.Storage.ReportedPlayers[reportedPlayer].matchID, true)
	
	Rap_Info.ShowRapPostgameWindow(5400000, '')
	
end

function Rap_Info.StoreReportedPlayerThisSession(playerName, matchID, value)

	if(playerName == nil or matchID == nil) then
		Echo('^rInvalid player name or matchID passed to StoreReportedPlayerThisSession -- ' .. tostring(playerName) .. ' ' .. tostring(matchID))
	end	
	
	local playerNameMatchIDComboToRemove = Rap_Info.Storage.ReportedPlayersNamesInReportOrder[Rap_Info.Storage.currentStoredReportedPlayerIndex]
	if(playerNameMatchIDComboToRemove ~= nil) then
		table.remove(Rap_Info.Storage.ReportedPlayersThisSession, playerNameMatchIDComboToRemove)
	end
	
	Rap_Info.Storage.ReportedPlayersThisSession[playerName..matchID] = value
	Rap_Info.Storage.ReportedPlayersNamesInReportOrder[Rap_Info.Storage.currentStoredReportedPlayerIndex] = playerName..matchID
		
	Rap_Info.Storage.currentStoredReportedPlayerIndex = Rap_Info.Storage.currentStoredReportedPlayerIndex +1
	if(Rap_Info.Storage.currentStoredReportedPlayerIndex > 100) then
		Rap_Info.Storage.currentStoredReportedPlayerIndex = 1
	end

end

function Rap_Info.StorePlayerIcon(playerIndex, iconPath)
	
	Rap_Info.Storage.HeroIconsByPlayerIndex[playerIndex] = iconPath

end

function Rap_Info.SetRapTypeForPlayer(reportedPlayer, rapType)
	
	if(Rap_Info.Storage.ReportedPlayers[reportedPlayer] ~= nil) then
		Rap_Info.Storage.ReportedPlayers[reportedPlayer].rapType = rapType
	end

	GetWidget('rap_postgame_entry_rap_icon_'..reportedPlayer, interfaceMainName):SetTexture(GetWidget('rap_postgame_entry_rap_type_icon_'..rapType..'_'..reportedPlayer, interfaceMainName):GetTexture())
	Rap_Info.UpdateAbuseTypeAndTimestamp(reportedPlayer)
end

interface:RegisterWatch('show_rap_panel', Rap_Info.ShowRapMenu)
interface:RegisterWatch('rap_timestamp_slider_updated', Rap_Info.UpdateEditReportTimestamp)
