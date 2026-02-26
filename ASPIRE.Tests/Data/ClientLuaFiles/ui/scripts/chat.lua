-------------------------------------------------------------------------------
--	Name: 		Ingame Chat Script					          				--
-- 	Version: 	3.0.0 r1													--					
--  Copyright 2015 Frostburn Studios										--
--[[
	AllChatMessages
		param0 (Type): CLEAR = 0, IRC = 1 (Status), GAME = 2 (Kills etc), TEAM = 3, ALL = 4, ROLL = 5, EMOTE = 6, [Channel = 7], 8 Friend Whisper, 9 Other whisper
		param1 (Channel): NULL for in game, or contains the channel name for IRC
		param2 (Message): The message
		param3 Hero Entity
		
		* Needs: Buddy Whisper-All. Clan Message. Server Message. Global Message.
--]]
-------------------------------------------------------------------------------

local SAVED_HISTORY_SIZE = 1000
local TEMP_HISTORY_SIZE = 1000
local GAME_BOTTOM_LINES = 18	-- Regular chat lines
local GAME_TOP_LINES = 0		-- Top section lines
local FADE_DURATION_SHORT = 750
local FADE_DURATION_LONG = 1500
local FADE_TIME_SHORT = 50
local FADE_TIME_LONG = 10000

local _G = getfenv(0)
local interface = object
local interfaceName = object:GetName()
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
GameChat = GameChat or {}
RegisterScript2('Game Chat', '30')

--local tempDB = tempDB or {}
--GameChat.db = NewDatabase('GameChat.ldb') -- GameChat.db:Flush()

GameChat.gameChat = {}
GameChat.activeTeam = GameChat.activeTeam or nil
GameChat.lastCurrentLine = GameChat.lastCurrentLine or nil
GameChat.fadeType = GameChat.fadeType or nil
GameChat.team = GameChat.team or {}

GameChat.enabledTeam		= GameChat.enabledTeam or true
GameChat.enabledMentor		= GameChat.enabledMentor or true
GameChat.lastType			= GameChat.lastType or 'all'

GameChat.chatEvents	= {		-- This exists exclusively to avoid issues w/non-escaped characters due to to the lack of Lua versions of chat functions.
	all		= 1,
	team	= 2,
	mentor	= 3
}

GameChat.clickedTab = false
GameChat.clickedOptions = false
	
local function groupfcall(groupName, functionArg, fromInterface)
	--println('^o groupfcall looking for: ' .. tostring(groupName) .. ' in interface ' .. tostring(fromInterface)..' \n')
	local groupTable
	if (fromInterface) then
		groupTable = UIManager.GetInterface(fromInterface):GetGroup(groupName)
	else
		groupTable = UIManager.GetActiveInterface():GetGroup(groupName)
	end	
	if (groupTable) then
		for index, widget in ipairs(groupTable) do
			functionArg(index, widget, groupName)
		end 
	else
		--println('^o groupfcall could not find: ' .. tostring(groupName) .. ' in interface ' .. tostring(fromInterface)..' \n')
	end	
end	
	
local function StripClanTag(userName)
	if (userName) and NotEmpty(userName) then
		local userName, userTag = userName, ''
		if (string.find(userName, ']')) then
			userTag = string.sub(userName, 1, string.find(userName, ']'))
			userName = string.sub(userName, string.find(userName, ']') + 1)
		end
		return userName, userTag
	else
		return nil, nil
	end
end

local function GetUsername()		
	local userName, userTag = GetAccountName(), ''
	userName, userTag = StripClanTag(userName)
	return userName, userTag
end		
		
local function GetPlayerTeam(targetPlayerName)
	local returnTeam = nil
	for teamIndex, teamTable in pairs(GameChat.team) do
		for playerIndex, playerName in pairs(teamTable) do
			if (playerName) and NotEmpty(playerName) and (targetPlayerName) and NotEmpty(targetPlayerName) and (StripClanTag(targetPlayerName)) and (StripClanTag(targetPlayerName) == StripClanTag(playerName)) then
				returnTeam = teamIndex
				break
			end
		end
	end
	return returnTeam
end
local function ScoreBoardPlayer(playerTeam, playerIndex, _, playerName, _, _, _, _, _, _, _, _, _, isBot)
	GameChat.team[playerTeam] = GameChat.team[playerTeam]  or {}
	GameChat.team[playerTeam][playerIndex] = playerName
	
	GameChat.bots = GameChat.bots or {}
	if (playerName == "") then
		GameChat.bots[playerIndex] = nil
	else
		GameChat.bots[playerIndex] = GameChat.bots[playerIndex] or {} 
		GameChat.bots[playerIndex].isBot = AtoB(isBot)
		GameChat.bots[playerIndex].Name = playerName
	end
end		
for playerIndex = 0,9,1 do
	if (playerIndex <= 4 ) then
		interface:RegisterWatch('ScoreboardPlayer'..playerIndex, function(...) ScoreBoardPlayer(1, playerIndex, ...) end)
	else
		interface:RegisterWatch('ScoreboardPlayer'..playerIndex, function(...) ScoreBoardPlayer(2, playerIndex, ...) end)
	end
end

local function Team(_, team)		
	GameChat.activeTeam  = tonumber(team)
end
interface:RegisterWatch('Team', Team)

function GameChat:ApplyGameChatFilters(message, premessage, messageType, isBuddy, isMe, messageTeam)	
	if ((HoN_Options.gameChatFilters) and (HoN_Options.gameChatFilters[HoN_Options.gameChatFilters.activeFilter]) and (HoN_Options.gameChatFilters[HoN_Options.gameChatFilters.activeFilter][messageType] ~= false)) and ((HoN_Options.gameChatFilters.muteAll == 0) or (HoN_Options.gameChatFilters.muteAll == 1 and isBuddy) or (messageType == '666')) then
		if (HoN_Options.gameChatFilters.shortFormat) and ((messageType == '5') or (messageType == '6') or (messageType == '7')) and NotEmpty(premessage) then	
			if (GameChat.activeTeam) then
				-- if (messageTeam) and (messageTeam == 1) then
					-- if (messageTeam == GameChat.activeTeam) then
						-- premessage = string.gsub(premessage, '%[ALL%]', '^g[A]^*')
					-- else
						-- premessage = string.gsub(premessage, '%[ALL%]', '^r[A]^*')
					-- end
				-- elseif (messageTeam) and (messageTeam == 2) then
					-- if (messageTeam == GameChat.activeTeam) then
						-- premessage = string.gsub(premessage, '%[ALL%]', '^g[A]^*')
					-- else
						-- premessage = string.gsub(premessage, '%[ALL%]', '^r[A]^*')
					-- end
				-- else
					-- premessage = string.gsub(premessage, '%[ALL%]', '[A]')
				-- end
				premessage = string.gsub(premessage, '%[ALL%]', '[A]')
			else
				-- if (messageTeam) and (messageTeam == 1) then
					-- premessage = string.gsub(premessage, '%[ALL%]', '^g[A]^*')
				-- elseif (messageTeam) and (messageTeam == 2) then
					-- premessage = string.gsub(premessage, '%[ALL%]', '^r[A]^*')
				-- else
					-- premessage = string.gsub(premessage, '%[ALL%]', '[A]')				
				-- end	
				premessage = string.gsub(premessage, '%[ALL%]', '[A]')
			end	

			premessage = string.gsub(premessage, '%[MENTOR%]', '[M]')
			premessage = string.gsub(premessage, '%[TEAM%]', '[T]')	
		end
		
		if (not HoN_Options.gameChatFilters.heroNames) and ((messageType == '5') or (messageType == '6') or (messageType == '7')) and NotEmpty(premessage) then	
			premessage = string.gsub(premessage, '%s%(.+%)', '')
			premessage = string.gsub(premessage, '%s%(%)', '')
		end
		
		-- if (HoN_Options.gameChatFilters.nickHighlight) and (not isMe) and ((messageType == '5') or (messageType == '6') or (messageType == '7')) then
			-- local userName, userTag = GetUsername()
			-- message = string.gsub(message, string.lower(userName), userName)
			-- message = string.gsub(message, userName, '^r^:'..userName..'^;^*')
		-- end
		
		return premessage..message
	else
		return nil
	end
end

local function ClearChat()
	GameChat.gameChat = {}
	local currentLine = GameChat:BuildChatTable(nil) 
    GameChat.TransferChatTable(self, currentLine, 1)
    GameChat:UpdateChatScroller()
end

GameChat.commands = {
	['chan'] = function(arg1, arg2) interface:UICmd([[ChatSendMessage(']]..EscapeString(arg2)..[[', ']]..EscapeString(arg1)..[[')]]) end,
	['clear'] = ClearChat
}

local function ChatCommand(inputText)
	local sendChat = true
	local commandStart, commandEnd, command = string.find(inputText, '/(%a+)')
	if (commandStart) then
		local exFunction = GameChat.commands[command]
		if (exFunction) then
			sendChat = false	
			local args = string.sub(inputText, commandEnd + 2)
			local _, _, arg1, arg2 = string.find(args, '(%a+)%s(.+)')	
			exFunction(arg1, arg2)
		end
	end
	return sendChat
end

function GameChat:ChatCommand(inputText)
	return ChatCommand(inputText)
end

function GameChat:ChatEnterPressed(sourceWidget, inputText, chatCommand)
	if ChatCommand(inputText) then
		sourceWidget:UICmd(chatCommand)
	end
	sourceWidget:SetFocus(false)
	sourceWidget:UICmd([[ClearCurrentChatMessage()]])
end

GameChat.hotkeyLastfilter = HoN_Options.gameChatFilters.activeFilter
function GameChat.ShowTab1Lines(self, param0)
	if AtoB(param0) then
		GameChat.hotkeyLastfilter = HoN_Options.gameChatFilters.activeFilter
		GameChat:ChangeGameChatFilter('2')
		GameChat:ShowAllLines()
	else
		GameChat:ChangeGameChatFilter(GameChat.hotkeyLastfilter or HoN_Options.gameChatFilters.activeFilter)
		GameChat:HideAllLines()	
	end
end
interface:RegisterWatch('ChatShowTab1', GameChat.ShowTab1Lines)

function GameChat:ShowTab2Lines(param0)
	if AtoB(param0) then
		GameChat.hotkeyLastfilter = HoN_Options.gameChatFilters.activeFilter
		GameChat:ChangeGameChatFilter('1')
		GameChat:ShowAllLines()
	else
		GameChat:ChangeGameChatFilter(GameChat.hotkeyLastfilter or HoN_Options.gameChatFilters.activeFilter)
		GameChat:HideAllLines()	
	end
end
interface:RegisterWatch('ChatShowTab2', GameChat.ShowTab2Lines)

function GameChat:ShowTab3Lines(param0)
	if AtoB(param0) then
		GameChat.hotkeyLastfilter = HoN_Options.gameChatFilters.activeFilter
		GameChat:ChangeGameChatFilter('3')
		GameChat:ShowAllLines()
	else
		GameChat:ChangeGameChatFilter(GameChat.hotkeyLastfilter or HoN_Options.gameChatFilters.activeFilter)
		GameChat:HideAllLines()	
	end
end
interface:RegisterWatch('ChatShowTab3', GameChat.ShowTab3Lines)

function GameChat:ShowTab4Lines(param0)
	GameChat:ChangeGameChatFilter('2')
end
interface:RegisterWatch('ChatShowTab4', GameChat.ShowTab4Lines)

function GameChat:ShowTab5Lines(param0)
	GameChat:ChangeGameChatFilter('1')
end
interface:RegisterWatch('ChatShowTab5', GameChat.ShowTab5Lines)

function GameChat:ShowTab6Lines(param0)
	GameChat:ChangeGameChatFilter('3')
end
interface:RegisterWatch('ChatShowTab6', GameChat.ShowTab6Lines)

function GameChat:ShowAllLines()
	
	GameChat:OptionsButtonClicked(self, -1)
	
	HoN_Options.gameChatFilters.walkthroughState = HoN_Options.gameChatFilters.walkthroughState or 0
	
	if (HoN_Options.gameChatFilters.walkthroughState == 0) then
		HoN_Options.gameChatFilters.walkthroughState = 1
		GetDBEntry('ingameChatOptions2', HoN_Options.gameChatFilters, true, false, false)
	elseif (HoN_Options.gameChatFilters.walkthroughState == 1) then
		GameChat.AllChatMessages(self, '666', nil, Translate('chat_walkthrough_info_2'), nil, nil, "false")
		HoN_Options.gameChatFilters.walkthroughState = 2
		GetDBEntry('ingameChatOptions2', HoN_Options.gameChatFilters, true, false, false)		
	elseif (HoN_Options.gameChatFilters.walkthroughState == 2) then
		GameChat.AllChatMessages(self, '666', nil, Translate('chat_walkthrough_info_3'), nil, nil, "false")
		HoN_Options.gameChatFilters.walkthroughState = 3
		GetDBEntry('ingameChatOptions2', HoN_Options.gameChatFilters, true, false, false)			
	end
	GameChat.fadeType = 1
	local currentLine = GameChat:BuildChatTable(nil)
	GameChat.TransferChatTable(self, currentLine, 1)
end

function GameChat:HideAllLines()
	--println('GameChat:HideAllLines()')
	GameChat.fadeType = nil
	GameChat.ClickedGameChatOptionButton(self, -1)
	local currentLine = GameChat:BuildChatTable(nil)
	GameChat.TransferChatTable(self, currentLine, 2)
end

function GameChat:UpdateChatScroller()
	if (GameChat.gameChatFiltered) and (GameChat.gameChatFiltered[HoN_Options.gameChatFilters.activeFilter]) then
		local chatSlots = (GAME_BOTTOM_LINES) -- + GAME_TOP_LINES
		local chatLines = #GameChat.gameChatFiltered[HoN_Options.gameChatFilters.activeFilter]
		local scroller = UIManager.GetActiveInterface():GetWidget(UIManager.GetActiveInterface():GetName() .. '_game_chat_scroller')

		if (scroller) then
			if (chatLines > chatSlots ) then	
				scroller:SetVisible(true)	
				scroller:UICmd("SetMaxValue("..(chatLines).."); SetMinValue("..(chatSlots).."); SetValue("..(chatLines)..")")
			else
				scroller:SetVisible(false)
			end
		end
		if GetWidget('game_chat_filter_tab_cover_' .. HoN_Options.gameChatFilters.activeFilter, UIManager.GetActiveInterface():GetName(), true) then
			groupfcall('game_chat_filter_tab_cover', function(_, widget, _) widget:SetVisible(false) end)
			GetWidget('game_chat_filter_tab_cover_' .. HoN_Options.gameChatFilters.activeFilter, UIManager.GetActiveInterface():GetName()):SetVisible(1)
		end
	end
end

local function ToggleLinkedChannels(activeFilter, messageType, status)

	local linkedChannelsTable = {
		[6] = {7, 8}
	}
	
	if (linkedChannelsTable[messageType]) then
		for i, v in pairs(linkedChannelsTable[messageType]) do
			HoN_Options.gameChatFilters[activeFilter][tostring(v)] = status
		end
	end
	
end

function GameChat.ChatTabMenuCheckboxClick(sourceWidget, messageType)
	if (HoN_Options.gameChatFilters) and (HoN_Options.gameChatFilters.activeFilter) then
		if (HoN_Options.gameChatFilters) and (HoN_Options.gameChatFilters[HoN_Options.gameChatFilters.activeFilter]) and (HoN_Options.gameChatFilters[HoN_Options.gameChatFilters.activeFilter][tostring(messageType)] ~= false) then	
			HoN_Options.gameChatFilters[HoN_Options.gameChatFilters.activeFilter] = HoN_Options.gameChatFilters[HoN_Options.gameChatFilters.activeFilter] or {}
			HoN_Options.gameChatFilters[HoN_Options.gameChatFilters.activeFilter][tostring(messageType)] = false
			ToggleLinkedChannels(HoN_Options.gameChatFilters.activeFilter, messageType, false)
			sourceWidget:SetButtonState(0)
		else
			HoN_Options.gameChatFilters[HoN_Options.gameChatFilters.activeFilter] = HoN_Options.gameChatFilters[HoN_Options.gameChatFilters.activeFilter] or {}
			HoN_Options.gameChatFilters[HoN_Options.gameChatFilters.activeFilter][tostring(messageType)] = true	
			ToggleLinkedChannels(HoN_Options.gameChatFilters.activeFilter, messageType, true)
			sourceWidget:SetButtonState(1)
		end
		GameChat.TransferChatTable(sourceWidget, GameChat.lastCurrentLine, 1)
		groupfcall('game_chat_tab_hover_menu_items', function(_, widget, _) widget:DoEvent() end)
		GetDBEntry('ingameChatOptions2', HoN_Options.gameChatFilters, true, false, false)
	end
end

function GameChat.ChatTabMenuCheckboxShow(sourceWidget, messageType)
	if (HoN_Options.gameChatFilters) and (HoN_Options.gameChatFilters[HoN_Options.gameChatFilters.activeFilter]) and (HoN_Options.gameChatFilters[HoN_Options.gameChatFilters.activeFilter][tostring(messageType)] ~= false) then
		sourceWidget:SetButtonState(1)
	else
		sourceWidget:SetButtonState(0)
	end
end

function GameChat.ClickChatTab(sourceWidget, index, isRightClick)
	if (isRightClick) then
		groupfcall('game_chat_tab_hover_menu_items', function(_, widget, _) widget:DoEvent() end)
		UIManager.GetActiveInterface():GetWidget('game_chat_tab_hover_menu_1'):DoEvent()
	else
		
	end
	local playeroptions = UIManager.GetActiveInterface():GetWidget(UIManager.GetActiveInterface():GetName()..'_game_chat_options')  
	playeroptions:SetVisible(false)	
end

local function IsPlayerAvailable(userName)
	if (userName) and NotEmpty(userName) then
		for teamIndex, teamTable in pairs(GameChat.team) do
			for playerIndex, playerName in pairs(teamTable) do
				if (playerName) and NotEmpty(playerName) and (userName) and NotEmpty(userName) and (StripClanTag(userName)) and (StripClanTag(string.lower(userName)) == StripClanTag(string.lower(playerName))) then
					return true, teamIndex
				end
			end
		end
		return ChatUserOnline(userName), -1
	else
		return false, -1
	end
end

function GameChat.PlayerHoverMenuItemShow(sourceWidget, index)

	Set('game_chat_player_hover_menu_item_'..index, 'false', 'bool')
	if (tonumber(index) == 1) then	-- add friend
		Set('game_chat_player_hover_menu_item_'..index, 'true', 'bool')
	elseif (tonumber(index) == 2) and (GameChat.mouseoverTarget) and (GameChat.mouseoverTargetSenderName) and NotEmpty(GameChat.mouseoverTargetSenderName) then	-- add friend
		if IsMe(GameChat.mouseoverTargetSenderName) then
			Set('game_chat_player_hover_menu_item_'..index, 'false', 'bool')
		elseif IsBuddy(GameChat.mouseoverTargetSenderName) then
			Set('game_chat_player_hover_menu_item_'..index, 'true', 'bool')
			sourceWidget:SetText(Translate('general_remove_friend'))
		elseif ChatUserOnline(GameChat.mouseoverTargetSenderName) then
			Set('game_chat_player_hover_menu_item_'..index, 'true', 'bool')
			sourceWidget:SetText(Translate('general_add_friend'))
		else
			Set('game_chat_player_hover_menu_item_'..index, 'false', 'bool')
		end
	elseif (tonumber(index) == 3) and (GameChat.mouseoverTarget) and (GameChat.mouseoverTargetSenderName) and NotEmpty(GameChat.mouseoverTargetSenderName) then	-- mute
		if IsMe(GameChat.mouseoverTargetSenderName) then
			Set('game_chat_player_hover_menu_item_'..index, 'false', 'bool')
		elseif IsPlayerAvailable(GameChat.mouseoverTargetSenderName) then
			Set('game_chat_player_hover_menu_item_'..index, 'true', 'bool')
			local isMuted = false
			if AtoB(UIManager.GetActiveInterface():UICmd([[ChatIsIgnored(']] .. GameChat.mouseoverTargetSenderName .. [[')]])) then
				isMuted = true
			end
			if (Game.playerNameToClient) and (Game.playerNameToClient[GameChat.mouseoverTargetSenderName]) then
				if AtoB(UIManager.GetActiveInterface():UICmd([[IsVoiceMuted(']] .. Game.playerNameToClient[GameChat.mouseoverTargetSenderName] .. [[')]])) then
					isMuted = true
				end
			end
			if (isMuted) then
				sourceWidget:SetText(Translate('general_unmute', 'name', GameChat.mouseoverTargetSenderName))
			else
				sourceWidget:SetText(Translate('general_mute', 'name', GameChat.mouseoverTargetSenderName))
			end
		else
			Set('game_chat_player_hover_menu_item_'..index, 'false', 'bool')
		end
	elseif (tonumber(index) == 4) and (GameChat.mouseoverTarget) and (GameChat.mouseoverTargetSenderName) and NotEmpty(GameChat.mouseoverTargetSenderName) then -- report
		local function NotABot(name)
			if (GameChat.bots) then
				for _, playerTable in pairs(GameChat.bots) do
					if (string.lower(StripClanTag(playerTable.Name)) == string.lower(StripClanTag(name))) then
						return (not playerTable.isBot)
					end
				end
			end
			return true
		end
	
		local isAvailable, teamIndex = IsPlayerAvailable(GameChat.mouseoverTargetSenderName)
		local clickerIsAvailable, clickerTeamIndex = IsPlayerAvailable(GetAccountName())
		local isNotInReplayOrSpectator = UIManager.GetActiveInterface():GetName() == 'game'
		if IsNotMe(GameChat.mouseoverTargetSenderName) and (isAvailable) and GetCvarBool('ui_rap_enabled') and NotABot(GameChat.mouseoverTargetSenderName) and (clickerTeamIndex ~= -1) and isNotInReplayOrSpectator then -- and (teamIndex == GameChat.activeTeam) 
			Set('game_chat_player_hover_menu_item_'..index, 'true', 'bool')
		else
			Set('game_chat_player_hover_menu_item_'..index, 'false', 'bool')
		end
	elseif (tonumber(index) == 5) and (GameChat.mouseoverTarget) and (GameChat.mouseoverTargetSenderName) and NotEmpty(GameChat.mouseoverTargetSenderName) then -- report
		local function NotABot(name)
			if (GameChat.bots) then
				for _, playerTable in pairs(GameChat.bots) do
					if (string.lower(StripClanTag(playerTable.Name)) == string.lower(StripClanTag(name))) then
						return (not playerTable.isBot)
					end
				end
			end
			return true
		end
	
		local isAvailable, teamIndex = IsPlayerAvailable(GameChat.mouseoverTargetSenderName)
		local clickerIsAvailable, clickerTeamIndex = IsPlayerAvailable(GetAccountName())
		local isNotInReplayOrSpectator = UIManager.GetActiveInterface():GetName() == 'game'
		if IsNotMe(GameChat.mouseoverTargetSenderName) and (isAvailable) and GetCvarBool('ui_rap_enabled') and NotABot(GameChat.mouseoverTargetSenderName) and (clickerTeamIndex ~= -1) and isNotInReplayOrSpectator then -- and (teamIndex == GameChat.activeTeam) 
			Set('game_chat_player_hover_menu_item_'..index, 'true', 'bool')
		else
			Set('game_chat_player_hover_menu_item_'..index, 'false', 'bool')
		end
	end
end

function GameChat.PlayerHoverMenuItemSelected(sourceWidget, index)
	if (GameChat.mouseoverTarget) then
		if (tonumber(index) == 1) then	-- Copy Text
			local targetText = UIManager.GetActiveInterface():GetWidget('game_chat_item_' .. GameChat.mouseoverTarget .. '_label'):GetText()
			if (targetText) and NotEmpty(targetText) then
				UIManager.GetActiveInterface():UICmd([[CopyToClipboard(']] .. EscapeString(targetText) .. [[')]])
			end
		elseif (tonumber(index) == 2) and (GameChat.mouseoverTargetSenderName) then	-- Toggle Friend
			if IsBuddy(GameChat.mouseoverTargetSenderName) then
				UIManager.GetActiveInterface():UICmd([[ChatRemoveBuddy(']] .. GameChat.mouseoverTargetSenderName .. [[')]])
			else
				UIManager.GetActiveInterface():UICmd([[ChatAddBuddy(']] .. GameChat.mouseoverTargetSenderName .. [[')]])
			end
		elseif (tonumber(index) == 3) and (GameChat.mouseoverTargetSenderName) then	-- Toggle Mute
			local shouldMute = false
			if AtoB(UIManager.GetActiveInterface():UICmd([[ChatIsIgnored(']] .. GameChat.mouseoverTargetSenderName .. [[')]])) then
				shouldMute = false
				UIManager.GetActiveInterface():UICmd([[ChatUnignore(']] .. GameChat.mouseoverTargetSenderName .. [[')]])
			else
				shouldMute = true
				UIManager.GetActiveInterface():UICmd([[ChatIgnore(']] .. GameChat.mouseoverTargetSenderName .. [[')]])
			end
			if (Game.playerNameToClient) and (Game.playerNameToClient[GameChat.mouseoverTargetSenderName]) then
				if (not AtoB(UIManager.GetActiveInterface():UICmd([[IsVoiceMuted(']] .. Game.playerNameToClient[GameChat.mouseoverTargetSenderName] .. [[')]]))) or (shouldMute) then
					UIManager.GetActiveInterface():UICmd([[VoiceMute(']] .. Game.playerNameToClient[GameChat.mouseoverTargetSenderName] .. [[')]])
				else
					UIManager.GetActiveInterface():UICmd([[VoiceUnmute(']] .. Game.playerNameToClient[GameChat.mouseoverTargetSenderName] .. [[')]])
				end
			end
		elseif (tonumber(index) == 4) and (GameChat.mouseoverTargetSenderName) then	-- Report
			Rap_Info.ShowRapMenu(sourceWidget, GameChat.mouseoverTargetSenderName)
		end
	end
end

function GameChat:MouseoverChatLine(sourceWidget, index, isOver)
	if (isOver) then
		GameChat.mouseoverTarget = index
		--local currentLine = GameChat:BuildChatTable(GameChat.lastCurrentLine)
		GameChat.TransferChatTable(self, GameChat.lastCurrentLine, 0)
	else
		GameChat.mouseoverTarget = nil
		GameChat.mouseoverTargetSenderName = nil
		GameChat.mouseoverTargetSenderTag = nil
		--local currentLine = GameChat:BuildChatTable(GameChat.lastCurrentLine)
		GameChat.TransferChatTable(self, GameChat.lastCurrentLine, 2)
	end
end

function GameChat.ClickChatLine(sourceWidget, index, isRightClick)
	if (isRightClick) then
		GameChat:MouseoverChatLine(sourceWidget, index, true)
		groupfcall('game_chat_player_hover_menu_items', function(_, widget, _) widget:DoEvent() end)
		UIManager.GetActiveInterface():GetWidget('game_chat_player_hover_menu_1'):DoEvent()
		UIManager.GetActiveInterface():GetWidget('game_chat_player_hover_menu_1'):SetCallback('onlosefocus', function() GameChat:MouseoverChatLine(sourceWidget, index, false) end)
		UIManager.GetActiveInterface():GetWidget('game_chat_player_hover_menu_1'):RefreshCallbacks()
		sourceWidget:Sleep(0, function() GameChat:MouseoverChatLine(sourceWidget, index, true) end)
	-- elseif (GameChat.mouseoverTarget) and (GameChat.mouseoverTargetSenderName) and NotEmpty(GameChat.mouseoverTargetSenderName) then
		-- UIManager.GetActiveInterface():GetWidget(UIManager.GetActiveInterface():GetName() .. '_chat_box_popup'):SetVisible(false)
		-- UIManager.GetActiveInterface():UICmd([[Action('ChatTeam')]])
		-- UIManager.GetActiveInterface():GetWidget(UIManager.GetActiveInterface():GetName() .. '_chat_team_input'):UICmd([[SetInputLine('/w ]] .. GameChat.mouseoverTargetSenderName .. [[ ')]])
		--UIManager.GetActiveInterface():GetWidget(UIManager.GetActiveInterface():GetName() .. '_chat_team_input'):SetFocus(false)
	end
end

function GameChat:BuildChatTable(currentLine)
	if (HoN_Options.gameChatFilters) and (HoN_Options.gameChatFilters[HoN_Options.gameChatFilters.activeFilter]) then
		
		GameChat.gameChatFiltered = GameChat.gameChatFiltered or {}
		GameChat.gameChatFiltered[HoN_Options.gameChatFilters.activeFilter] = {}
		
		local currentChatLine = currentLine or #GameChat.gameChat
		local highestLine = 1
		
		for chatLineIndex = 1, currentChatLine, 1 do
			if (GameChat.gameChat[chatLineIndex]) then
				if (GameChat.gameChat[chatLineIndex].message) then
					local message = GameChat.ApplyGameChatFilters(self, GameChat.gameChat[chatLineIndex].message, GameChat.gameChat[chatLineIndex].premessage, GameChat.gameChat[chatLineIndex].messageType, GameChat.gameChat[chatLineIndex].isBuddy, GameChat.gameChat[chatLineIndex].isMe, GameChat.gameChat[chatLineIndex].team)
					if (message) then
						tinsert(GameChat.gameChatFiltered[HoN_Options.gameChatFilters.activeFilter], GameChat.gameChat[chatLineIndex])
						highestLine = Max(chatLineIndex, highestLine)
					else
						GameChat.gameChat[chatLineIndex].soundMessageType = nil
					end
				end
			end
		end
		
		return highestLine
		
	end
end					

local function ChatSound(soundMessageType, isMe)
	soundMessageType = tonumber(soundMessageType)
	if (soundMessageType) and (HoN_Options.gameChatFilters.chatSounds) then
	
		local soundsByType = {
			--[0]  = nil,
			[1]  = {'RecievedChannelMessage', ''},
			--[2]  = {'RecievedChannelMessage', ''},
			--[3]  = {'RecievedChannelMessage', ''},
			--[4]  = {'RecievedChannelMessage', ''},
			[5]  = {'RecievedChannelMessage', ''},
			[6]  = {'RecievedChannelMessage', ''},
			[7]  = {'RecievedChannelMessage', ''},
			[8]  = {'RecievedChannelMessage', ''},
			[9]  = {'RecievedChannelMessage', ''},
			[10] = {'RecievedWhisper', 'SentWhisper'}, 
			[11] = {'RecievedWhisper', 'SentWhisper'},
			[12] = {'RecievedWhisper', 'SentWhisper'}, 
			[13] = {'RecievedClanMessage', 'SentClanMessage'},
			--[14] = nil,
			[15] = {'RecievedIM', 'SentIM'},
			--[16] = nil,
			--[17] = nil,

			[31] = {'RecievedChannelMessage', 'SentChannelMessage'},
			[32] = {'RecievedWhisper', 'SentWhisper'}, 
			[33] = {'RecievedWhisper', 'SentWhisper'}, 
		
		}
		
		if (isMe) then
			if (soundsByType[soundMessageType]) and (soundsByType[soundMessageType][2]) and NotEmpty(soundsByType[soundMessageType][2]) then
				PlayChatSound(soundsByType[soundMessageType][2])		
			end		
		else
			if (soundsByType[soundMessageType]) and (soundsByType[soundMessageType][1]) and NotEmpty(soundsByType[soundMessageType][1]) then
				PlayChatSound(soundsByType[soundMessageType][1])
			end
		end
	end
end				
				
function GameChat:TransferChatTable(currentLine, fadeType)
	if (GameChat.gameChat) and (GameChat.gameChatFiltered) and (GameChat.gameChatFiltered[HoN_Options.gameChatFilters.activeFilter]) then
		
		local fadeType = fadeType or 0
		if (GameChat.fadeType) then
			fadeType = GameChat.fadeType or 0
		end		
		
		local chatTopIndex = GAME_BOTTOM_LINES + 1
		local chatBottomIndex = 1
		local hosttime = tonumber(UIManager.GetActiveInterface():UICmd("HostTime"))
		
		local maxChatLines = #GameChat.gameChat
		local currentChatLine = currentLine or maxChatLines
		
		local chatTable = GameChat.gameChatFiltered[HoN_Options.gameChatFilters.activeFilter]
		
		local framewidget, frameborderwidget, labelwidget, imagewidget, bgwidget, parentwidget
		
		for widgetIndex = 1, GAME_BOTTOM_LINES, 1 do

			framewidget 		= UIManager.GetActiveInterface():GetWidget('game_chat_item_'..widgetIndex..'_frame')
			frameborderwidget 	= UIManager.GetActiveInterface():GetWidget('game_chat_item_'..widgetIndex..'_frame_border')
			labelwidget 		= UIManager.GetActiveInterface():GetWidget('game_chat_item_'..widgetIndex..'_label')
			imagewidget			= UIManager.GetActiveInterface():GetWidget('game_chat_item_'..widgetIndex..'_image')
			bgwidget 			= UIManager.GetActiveInterface():GetWidget('game_chat_item_'..widgetIndex..'_bg')
			parentwidget 		= UIManager.GetActiveInterface():GetWidget('game_chat_item_'..widgetIndex)			
			
			if (parentwidget) then
				parentwidget:SetVisible(false)
				parentwidget:SetNoClick(1)	
				parentwidget:ClearCallback('onevent')
				parentwidget:RefreshCallbacks()			
				parentwidget:UnregisterWatch('HostTime')

				for chatLineIndex = currentChatLine, 1, -1 do
				
					if (chatTable[chatLineIndex]) then
						if (chatTable[chatLineIndex].message) then
							
							local message = GameChat.ApplyGameChatFilters(self, chatTable[chatLineIndex].message, chatTable[chatLineIndex].premessage, chatTable[chatLineIndex].messageType, chatTable[chatLineIndex].isBuddy, chatTable[chatLineIndex].isMe, chatTable[chatLineIndex].team)

							if (message) then

								-- images if entity exists
								local entity = chatTable[chatLineIndex].entity
								
								if (HoN_Options.gameChatFilters.heroIcons) and (entity) and (string.len(entity) > 4) then
									framewidget:SetVisible(1)
									framewidget:SetHeight('2.2h')
									imagewidget:UICmd("SetTexture(GetEntityIconPath('"..entity.."'))")
									labelwidget:SetWidth('-4.4h')
								else
									framewidget:SetVisible(0)
									framewidget:SetHeight('1.8h')
									labelwidget:SetWidth('100%')
								end
								
								if (GameChat.mouseoverTarget) and (GameChat.mouseoverTarget == widgetIndex) then
									--message = string.gsub(message, '%^%d%d%d', '^999')
									--labelwidget:SetOutline(true)
									--labelwidget:SetShadow(false)
									labelwidget:SetShadowColor('0.7 0.7 0.7 1')
									frameborderwidget:SetBorderColor('1 1 1 0.7')
									frameborderwidget:SetColor('1 1 1 0.2')
									--bgwidget:SetVisible(1)
									--bgwidget:SetColor('0.7 0.7 0.7 0.1')
									GameChat.mouseoverTargetSenderName = chatTable[chatLineIndex].senderName
									GameChat.mouseoverTargetSenderTag = chatTable[chatLineIndex].userTag
								else
									--labelwidget:SetOutline(false)
									--labelwidget:SetShadow(true)
									labelwidget:SetShadowColor('0 0 0 1')
									frameborderwidget:SetBorderColor('0 0 0 1')
									frameborderwidget:SetColor('1 1 1 0')
								end
								
								if NotEmpty(chatTable[chatLineIndex].channel) then
									labelwidget:SetText('^770['.. chatTable[chatLineIndex].channel .. ']^* ' .. message)
								else
									labelwidget:SetText(message)
								end								
																								
								-- background if team exists
								--[[					
								if (chatTable[chatLineIndex].team) then
									--println('team: ' .. chatTable[chatLineIndex].team .. ' | ' .. GameChat.activeTeam)
									bgwidget:SetVisible(1)
									if (chatTable[chatLineIndex].team == 1) then
										bgwidget:SetColor('0 0.7 0 0.2')
									else
										bgwidget:SetColor('0.7 0.1 0 0.2')
									end
								else
									bgwidget:SetVisible(0)
								end				
								--]]	

								if (fadeType == 0) or (fadeType == 2) then -- use timestamp to recalculate sleeps and fade out
									parentwidget:RegisterWatch('HostTime', 
										function(sourceWidget, curHostTime) 
											if (tonumber(curHostTime) > ( (chatTable[chatLineIndex].hosttime + FADE_TIME_LONG) )) then
												sourceWidget:UnregisterWatch('HostTime')
												sourceWidget:FadeOut(FADE_DURATION_LONG)
											else
												if (chatTable[chatLineIndex].soundMessageType) then
													ChatSound(chatTable[chatLineIndex].soundMessageType, chatTable[chatLineIndex].isMe)
													chatTable[chatLineIndex].soundMessageType = nil
												end
												sourceWidget:SetVisible(1)
											end
										end
									 )
									parentwidget:SetNoClick(1)	
								elseif (fadeType == 1)  then -- Show all, no fade timer
									if (string.len(labelwidget:GetText()) >= 1) then
										parentwidget:SetVisible(1)			
									end
									parentwidget:SetNoClick(0)	
								end						
						
								currentChatLine = chatLineIndex - 1
								break
							else
								--println('^r message filtered ' .. chatLineIndex .. ' | ' .. widgetIndex)
								currentChatLine = chatLineIndex - 1
							end
						else
							--println('^r no message ' .. chatLineIndex .. ' | ' .. widgetIndex)
						end
					else
						--println('^r no index ' .. chatLineIndex .. ' | ' .. widgetIndex)
					end
				end
				
				if ( UIManager.GetActiveInterface():GetWidget(UIManager.GetActiveInterface():GetName() .. '_chat_box_popup'):GetAbsoluteY() > parentwidget:GetAbsoluteY() ) and (fadeType ~= 1) then
					-- println('widgetIndex = ' .. widgetIndex)
					-- println('^y 1: ' .. tostring(UIManager.GetActiveInterface():GetWidget(UIManager.GetActiveInterface():GetName() .. '_chat_box_popup'):GetAbsoluteY()) )
					-- println('^o 2: ' .. tostring(parentwidget:GetAbsoluteY()) )		
					parentwidget:SetVisible(false)
				end
			else
				--println('^r no widget ' .. widgetIndex)			
			end
		end
		GameChat.lastCurrentLine = currentLine
	end
end	

function GameChat:ChangeGameChatFilter(newFilter)
	HoN_Options.gameChatFilters.activeFilter = newFilter
	local currentLine = GameChat:BuildChatTable(nil)
	GameChat.TransferChatTable(self, currentLine, 1)
	GameChat.ClickedGameChatOptionButton(self, -1)
	GameChat:UpdateChatScroller()
	GetDBEntry('ingameChatOptions2', HoN_Options.gameChatFilters, true, false, false)
	
	if GetWidget('game_chat_filter_tab_cover_' .. HoN_Options.gameChatFilters.activeFilter, UIManager.GetActiveInterface():GetName(), true) then
		groupfcall('game_chat_filter_tab_cover', function(_, widget, _) widget:SetVisible(false) end)
		GetWidget('game_chat_filter_tab_cover_' .. HoN_Options.gameChatFilters.activeFilter, UIManager.GetActiveInterface():GetName()):SetVisible(1)
	end
end

local function UpdateOptionsButton(baseName, value)
	local frame = GetWidget(baseName.."_frame", UIManager.GetActiveInterface():GetName())
	local check = GetWidget(baseName.."_check", UIManager.GetActiveInterface():GetName())

	check:SetVisible(value)

	if (value) then
		frame:SetRenderMode("normal")
	else
		frame:SetRenderMode("grayscale")
	end
end

function GameChat:TipVisibility()
	local visible = GetDBEntry('ingameTipVisible2', 'nil', false, false, false)
	if visible == 'nil' then visible = true else visible = AtoB(visible) end

	return visible
end

function GameChat:UpdateTip(clickType)
	clickType = tonumber(clickType)

	if (clickType == 1) then --options
		GameChat.clickedOptions = true
	elseif (clickType == 2) then --right tab
		GameChat.clickedTab = true
	end

	if ((GameChat.clickedOptions and GameChat.clickedTab) or clickType == 3) then
		-- hide
		GetDBEntry('ingameTipVisible2', 'false', true, false, false)
		UIManager.GetActiveInterface():GetWidget(UIManager.GetActiveInterface():GetName()..'_game_chat_tips'):FadeOut(100)
	end
end

function GameChat:OptionsButtonClicked(widget, buttonIndex)
	--print('widget: '..tostring(widget)..' | name: ' .. widget:GetName() ..' | buttonIndex: ' .. buttonIndex )
	
	if (buttonIndex == 1) then
		HoN_Options.gameChatFilters.shortFormat = not HoN_Options.gameChatFilters.shortFormat
	elseif (buttonIndex == 2) then
		HoN_Options.gameChatFilters.heroIcons = not HoN_Options.gameChatFilters.heroIcons
	elseif (buttonIndex == 3) then
		HoN_Options.gameChatFilters.heroNames = not HoN_Options.gameChatFilters.heroNames		
	elseif (buttonIndex == 4) then
		HoN_Options.gameChatFilters.nickHighlight = not HoN_Options.gameChatFilters.nickHighlight
	elseif (buttonIndex == 5) then
		HoN_Options.gameChatFilters.chatSounds = not HoN_Options.gameChatFilters.chatSounds		
	end

	-- refresh
	--local chat_tab_options_btn_4 = GetWidget('chat_tab_options_btn_4', UIManager.GetActiveInterface():GetName())
	UpdateOptionsButton("chat_tab_options_btn_1", HoN_Options.gameChatFilters.shortFormat)
	UpdateOptionsButton("chat_tab_options_btn_2", HoN_Options.gameChatFilters.heroIcons)
	UpdateOptionsButton("chat_tab_options_btn_3", HoN_Options.gameChatFilters.heroNames)	
	--UpdateOptionsButton(chat_tab_options_btn_4, HoN_Options.gameChatFilters.nickHighlight)	
	UpdateOptionsButton("chat_tab_options_btn_5", HoN_Options.gameChatFilters.chatSounds)

	HoN_Options:SaveChatOptions()
	HoN_Options:UpdateLUAOptions()
end

function GameChat:ClickedGameChatOptionButton(optionButton)

	local playeroptions = UIManager.GetActiveInterface():GetWidget(UIManager.GetActiveInterface():GetName()..'_game_chat_options')  
	local playeroptions_cover = UIManager.GetActiveInterface():GetWidget('game_chat_filter_tab_cover_0') 

	playeroptions:SetVisible(false)
	playeroptions_cover:SetVisible(false)
	
	if (optionButton == 0) then -- Player Options	
		playeroptions:SetVisible(true)
		playeroptions_cover:SetVisible(true)
		UIManager.GetActiveInterface():GetWidget(UIManager.GetActiveInterface():GetName() .. '_game_chat_scroller'):SetVisible(0)
		UIManager.GetActiveInterface():GetWidget(UIManager.GetActiveInterface():GetName() .. '_chat_box'):SetVisible(0)
	else
		UIManager.GetActiveInterface():GetWidget(UIManager.GetActiveInterface():GetName() .. '_chat_box'):SetVisible(1)
	end
	GetDBEntry('ingameChatOptions2', HoN_Options.gameChatFilters, true, false, false)
end

-- local function FindMIA(message)

	-- local messageToSound = {}
	
		-- {'mia', 'bot', '/ui/mia/mia_bot.mp3'},
		-- {'mia', 'top', '/ui/mia/mia_top.mp3'},
		-- {'mia', 'mid', '/ui/mia/mia_mid.mp3'},
		-- {'mia', nil, '/ui/mia/mia_hero.mp3'},
	
	-- }
	
	-- for i, v in pairs(messageToSound) do

		-- if string.find(message, v[1]) ((not v[2]) or string.find(message, v[2])) then
			-- PlaySound(i)
			-- break
		-- end
	-- end
			
-- end

--[[
	AllChatMessages
		param0 (Type): CLEAR = 0, IRC = 1 (Status), 2 = Blue system text, GAME = 3 (Kills etc), TEAM = 4, ALL = 5, ROLL = 6, EMOTE = 7, [Channel = 8], 9 Friend Whisper, 10 Other whisper
		param1 (Channel): NULL for in game, or contains the channel name for IRC
		param2 (Message): The message
		param3 Hero Entity
		
		* Needs: Buddy Whisper-All. Clan Message. Server Message. Global Message.
		
		0 CHAT_MESSAGE_CLEAR = 0,
		1 CHAT_MESSAGE_IRC, 
		2 CHAT_MESSAGE_SYSTEM,
		3 CHAT_MESSAGE_GAME,
		4 CHAT_MESSAGE_GAME_IMPORTANT,
		5 CHAT_MESSAGE_TEAM,
		6 CHAT_MESSAGE_ALL,
		7 CHAT_MESSAGE_ROLL,
		8 CHAT_MESSAGE_EMOTE,
		9 CHAT_MESSAGE_GROUP,
		10 CHAT_MESSAGE_WHISPER,
		11 CHAT_MESSAGE_WHISPER_BUDDIES,
		12 CHAT_MESSAGE_CLAN,
		13 CHAT_MESSAGE_GLOBAL,
		14 CHAT_MESSAGE_IM,
		15 CHAT_MESSAGE_SERVER,
		16 CHAT_MESSAGE_LOCAL,

		31 - Channel
		32 - Friend whisper
		33 - Other whisper
		
--]]
function GameChat:AllChatMessages(messageType, channel, message, entity, noFormatting, isMe)
	messageType, channel, message, entity, premessage = messageType or '', channel or '', message or '', entity or '', ''
	
	--println('^y' .. messageType.. ' | ' .. channel .. ' | ' .. message .. ' | ' .. entity .. ' | ' .. tostring(noFormatting) .. ' | ' .. tostring(isMe))
	
	if NotEmpty(channel) then -- channel message
		messageType = '31'
	end

	local userTag, isBuddy = '', false
	local team	
	local nameStart, nameEnd, senderName = string.find(message, '%s(.+)%s%(.-%):')
	
	if (nameStart) and (nameEnd) and (senderName)  then
		--println('^y' .. nameStart.. ' | ' .. nameEnd .. ' | ' .. senderName)
		premessage  = string.sub(message, 1, nameEnd)
		message 	= string.sub(message, nameEnd + 1)
		--println('^r' .. premessage.. ' | ' .. message)
		if (string.find(senderName, ']')) then
			userTag = string.sub(senderName, 1, string.find(senderName, ']'))
			senderName = string.sub(senderName, string.find(senderName, ']') + 1)
		elseif (string.find(senderName, '%^')) then
			userTag = ''
			senderName = string.sub(senderName, string.find(senderName, '%^') + 4)
		end
		isBuddy = IsBuddy(senderName)
		team 	= GetPlayerTeam(senderName)
		if (isBuddy) and (messageType == '12') then 
			messageType = '32'
		elseif ChatUserOnline(senderName) and (messageType == '11') then 
			messageType = '33'
		end		
		
		-- if (messageType == '5') then
			-- FindMIA(message)
		-- end
	end	
	
	local hosttime = tonumber(UIManager.GetActiveInterface():UICmd("HostTime"))
	table.insert(GameChat.gameChat,  { noFormatting = noFormatting, messageType = messageType, soundMessageType = messageType, channel = channel, message = message, premessage = premessage, entity = entity, hosttime = hosttime, team = team, senderName = senderName, userTag = userTag, isBuddy = isBuddy, isMe = AtoB(isMe) or false} )
	
	-- enforce maximum table size
	if (#GameChat.gameChat > TEMP_HISTORY_SIZE) then
		table.remove(GameChat.gameChat, 1)
	end
	
	-- if we try to use all and our current filter doesn't allow it, change to the all tab if it still contains all messages, this should be smoother for experienced hon players
	if (isMe) and AtoB(isMe) and  (messageType == '6') and (not HoN_Options.gameChatFilters[HoN_Options.gameChatFilters.activeFilter][messageType]) and (HoN_Options.gameChatFilters['1'][messageType])  then
		if (UIManager.GetActiveInterface():GetName() ~= "main") then
			GameChat:ChangeGameChatFilter('1')
			GameChat.AllChatMessages(self, '666', nil, Translate('chat_walkthrough_info_4', 'key', interface:UICmd("GetKeybindButton('game', 'ShowChat', '', 0)")), nil, nil, "false")
		end
	else -- update chat normally
		local currentLine = GameChat:BuildChatTable(nil)
		GameChat.TransferChatTable(self, currentLine, 0)
		GameChat:UpdateChatScroller()	
	end	
end

function GameChat:ChatWhisperUpdate(senderName, message, info)	-- registered in game_new.lua
	-- don't display if DND
	-- 0 = CHAT_MODE_AVAILABLE
	-- 1 = CHAT_MODE_AFK
	-- 2 = CHAT_MODE_DND
	-- 3 = CHAT_MODE_INVISIBLE
	if (GetChatModeType and GetChatModeType() == 2) then
		return
	end

	-- check for friend/clan only
	if (GetFriendlyChat and GetFriendlyChat()) then
		if ((not IsBuddy(StripClanTag(senderName))) and (not IsClanMember(StripClanTag(senderName)))) then
			return
		end
	end
	
	--println('^y' .. senderName.. ' | ' .. message)
	
	local userTag, isBuddy, isMe = '', false, (tonumber(info) == 0)
	
	local nameStart, nameEnd, _ = string.find(message, '%^(.+)%:%s')
	
	if (nameStart) and (nameEnd) and (senderName) and NotEmpty(senderName) and (not isMe) then 	-- remove the isMe to show messages sent to others
																								-- we aren't showing them now since you can't reply in-game
		--premessage  = string.sub(message, 1, nameEnd)
		message 	= string.sub(message, nameEnd + 1)
		premessage  = ""
	
		if (not isMe) then
			premessage = Translate("game_chat_im_recieve", "target", senderName)
		else
			premessage = Translate("game_chat_im_send", "target", senderName)
		end

		message = '^999 ' .. message
	
		if (string.find(senderName, ']')) then
			userTag = string.sub(senderName, 1, string.find(senderName, ']'))
			senderName = string.sub(senderName, string.find(senderName, ']') + 1)
		elseif (string.find(senderName, '%^')) then
			userTag = ''
			senderName = string.sub(senderName, string.find(senderName, '%^') + 4)
		end

		isBuddy = self:UICmd("IsBuddy('"..senderName.."')")
		
		if (isBuddy) then 
			messageType = '32'
		else
			messageType = '33'
		end		

		local hosttime = tonumber(UIManager.GetActiveInterface():UICmd("HostTime"))

		table.insert(GameChat.gameChat,  { messageType = messageType, soundMessageType = messageType, channel = '', message = message, premessage = premessage, entity = '', hosttime = hosttime, playerIndex = '', team = 1, senderName = senderName, userTag = userTag, isBuddy = isBuddy, isMe = isMe} )
		
		-- enforce maximum table size
		if (#GameChat.gameChat > TEMP_HISTORY_SIZE) then
			table.remove(GameChat.gameChat, 1)
		end
		
		-- update chat
		local currentLine = GameChat:BuildChatTable(nil)
		GameChat.TransferChatTable(self, currentLine, 0)
		GameChat:UpdateChatScroller()	
	
	end

end

function GameChat.GamePhase(sourceWidget, gamePhase)
	gamePhase = tonumber(gamePhase)
	if (gamePhase <= 0) then
		GameChat.gameChat = {}
	elseif (gamePhase == 5) then
		if (HoN_Options.gameChatFilters.walkthroughState == 0) and (GameChat.walkthroughtState ~= 1) then
			GameChat.walkthroughtState = 1
			GameChat.AllChatMessages(self, '666', nil, Translate('chat_walkthrough_info_1', 'key', interface:UICmd("GetKeybindButton('game', 'ShowChat', '', 0)")), nil, nil, "false")
		end
	end
end

local function MapChatMessage(_, text)
	GameChat.AllChatMessages(self, '3', nil, Translate(text), nil, nil, "false")
end
interface:RegisterWatch("MapChatMessage", MapChatMessage)

function interface:HoNChatF(func, ...)
  print(GameChat[func](self, ...))
end	

local function nonAllChatCondition(map)	-- , gameMode
	return map ~= 'prophets'
end

local function chatInputRegister(object)

	local chatTypes	= {	-- Duplicating behavior from uiscript
		all		= 'all',
		team	= 'team',
		mentor	= 'team',
		comment = 'comment',
	}

	--[[
			all				AllChat		SetCurrentChatType('all')
			team			TeamChat	SetCurrentChatType('team')
			mentor			MentorChat	SetCurrentChatType('team')
	--]]
	
	local inputBox		= object:GetWidget(interfaceName..'_chatInput')
	local inputLabel	= object:GetWidget(interfaceName..'_chatInputLabel')
	local inputBoxFrame	= object:GetWidget(interfaceName..'_textbox_frame')
	local inputSeperator= object:GetWidget(interfaceName..'_chat_seperator')

	inputBox:SetCallback('onkeydown', function(widget, keyPressed)
		local key = tonumber(keyPressed)
			
		if key == 281 then
			ChatPrevHistory()
			widget:SetInputLine(self:UICmd('ChatGetCurHistory()'))
		elseif key == 283 then
			ChatNextHistory()
			widget:SetInputLine(self:UICmd('ChatGetCurHistory()'))
		end
	end)
	
	inputBox:SetCallback('ontab', function(widget)
		local widgetValue	= widget:GetValue()
		local tabResult = TabChat(widgetValue)
		if tabResult ~= widgetValue then
			widget:SetInputLine(tabResult)
		end
	end)
	
	local function chatInputFocus(hasFocus)
		inputBox:SetVisible(hasFocus)
		inputLabel:SetVisible(hasFocus)
		inputBoxFrame:SetVisible(hasFocus)
		
		if hasFocus then
			inputLabel:SetText(Translate('gamechat_indicate_'..GameChat.lastType))
		end

		-- need more space for string 'comment'
		if GameChat.lastType == 'comment' then
			inputLabel:SetWidth('22%')
			inputBox:SetX('23.5%')
			inputBox:SetWidth('70%')
			inputSeperator:SetX('22%')
		else
			inputLabel:SetWidth('12%')
			inputBox:SetX('13.5%')
			inputBox:SetWidth('80%')
			inputSeperator:SetX('12%')
		end
		Set('_game_chat_inputbox_focused', hasFocus, 'bool')
	end
	
	inputBox:SetCallback('onlosefocus', function(widget)
		chatInputFocus(false)
	end)
	
	inputBox:SetCallback('onfocus', function(widget)
		chatInputFocus(true)
		widget:SetInputLine(self:UICmd('GetCurrentChatMessage()'))
		widget:UICmd("SetCurrentChatType('"..chatTypes[GameChat.lastType].."')")
	end)
	
	inputBox:SetCallback('onchange', function(widget)
		local widgetValue	= widget:GetValue()
		local tabResult = SetCurrentChatMessage(widgetValue)
		if (tabResult ~= widgetValue) then
			widget:SetInputLine(tabResult)
		end
	end)
	
	local function initializeInput(chatType)
		if chatType ~= 'all' then
			if chatType == 'mentor' and not GameChat.enabledMentor then
				-- return false
				chatType = 'all'
			end
			
			if chatType == 'team' and not GameChat.enabledTeam then
				-- return false
				chatType = 'all'
			end

			if chatType == 'comment' and not GetCvarBool('watch_bullet_sending') then
				chatType = 'all'
			end
		end
		
		GameChat.lastType = chatType
		inputBox:SetFocus(true)
	end	
	
	local function chatInputRegisterField(object, index)
		local tempInput		= object:GetWidget(interfaceName..'_chat_'..index..'_input')
	

		tempInput:SetCallback('onfocus', function(widget)
			initializeInput(index)
			tempInput:SetFocus(false)
		end)
		
		tempInput:SetCallback('onlosefocus', function(widget)
			tempInput:SetVisible(false)
			
		end)
		
		tempInput:SetCallback('onshow', function(widget)
			
		end)
	end

	chatInputRegisterField(object, 'mentor')
	chatInputRegisterField(object, 'all')
	chatInputRegisterField(object, 'team')
	chatInputRegisterField(object, 'comment')
	
	local function nonAllChatEnable()
		GameChat.enabledTeam	= true
		GameChat.enabledMentor	= true
		
	end

	local function nonAllChatDisable()
		GameChat.enabledTeam	= false
		GameChat.enabledMentor	= false
		
		if inputBox:HasFocus() and GameChat.lastType ~= 'all'  then
			
			GameChat.lastType = 'all'
			inputBox:UICmd("SetCurrentChatType('all')")
		end
	end

	GameUIManager.RegisterFeature('non_all_chat', nonAllChatCondition, nonAllChatEnable, nonAllChatDisable, true)
	
end

chatInputRegister(object)