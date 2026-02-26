
-----------------
-- Communicator Script
-- Copyright 2015 Frostburn Studios
-----------------

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort
local interface, interfaceName = object, object:GetName()
Communicator_V2 = {}
RegisterScript2('Communicator', '32')

local function GetRealPixel(n)
	local height = GetScreenHeight()
	return math.floor(n * height / 1080 + 0.5)
end

local CHAT_TAB_SELECTED_LONG 		= GetRealPixel(10)
local CHAT_TAB_WIDTH_MAX 			= GetRealPixel(130)
local CHAT_TAB_WIDTH_MIN 			= GetRealPixel(70)
local CHAT_TAB_GAP_MAX 				= GetRealPixel(6)
local CHAT_TAB_GAP_MIN 				= GetRealPixel(3)

local CHAT_TAB_MAX 					= 1
local CHAT_TAB_SELECTED_WIDTH 		= GetRealPixel(140)
local CHAT_TAB_NOT_SELECTED_WIDTH	= GetRealPixel(130)
local CHAT_TAB_GAP 					= GetRealPixel(6)
local CHAT_TAB_EDGE_WIDTH			= GetRealPixel(16)
local CHAT_HISTORY_MAX				= 1000
local CHAT_STATUS_TEXT				= 'STATUS'
local CHAT_GAME_TEXT 				= 'Game'
local CHAT_TAB_WIDGET_MAX 			= 24
local USER_NUMBER_MAX				= 6
local USER_WIDGET_MAX 				= 6

local chatEntitiesPool = {}
chatEntitiesPool[1] = {	type = 'channel',
				   		chanID = -1,
				  		chatName = CHAT_STATUS_TEXT,
				  		history = {}}

local chatTabs = {}
table.insert(chatTabs, 1)

local usersPool = {}

local _gameChannelUsers = {}
local _altButtonDown = false
------------------------------------------------------------------
local chatIndex = 2
local selectedChatTab = 1
local firstChatTab = 1
local focusChannel = nil

local firstUserIndex = 1

local _chatType = -1
local _chatSwitched = false
local _matchChannelFirst = false

local _chatDisplayConfig = {}
table.insert(_chatDisplayConfig, {target='watch_system', hide=true})
table.insert(_chatDisplayConfig, {target='queue_loading', hide=true})
table.insert(_chatDisplayConfig, {target='loading_bg_image', hide=true})
table.insert(_chatDisplayConfig, {target='player_ladder', hide=true})
table.insert(_chatDisplayConfig, {target='player_tour_stats', hide=true})
table.insert(_chatDisplayConfig, {target='lobby_loading', hide=true})
table.insert(_chatDisplayConfig, {target='lobby_picker_herotip_large', hide=true})
table.insert(_chatDisplayConfig, {target='match_stats', width=1325, x=0})
table.insert(_chatDisplayConfig, {target='playerstats', width=1325, x=0})
table.insert(_chatDisplayConfig, {target='store_container2', width=977, gap=4})
table.insert(_chatDisplayConfig, {target='lobby_matchinfo', width=1080, x=-115})
table.insert(_chatDisplayConfig, {target='lobby_picker_large', width=1099, x=0, sticky=true, height=253, usermax=5, gap=4})
table.insert(_chatDisplayConfig, {target='lobby_picker_small', width=775, x=0, sticky=true, height=275, usermax=5, gap=4})
table.insert(_chatDisplayConfig, {target='hosted_game', width=994, x=-167})
table.insert(_chatDisplayConfig, {target='map_selection_large', width=977, x=47})
table.insert(_chatDisplayConfig, {target='map_selection_small', width=780, x=50})
_chatDisplayConfig['default'] = {sticky=true, width=866, x=226, height=287}

local function InsertToChatEntitiesPool(chatEntity)
	local emptyPos = -1

	for i,v in ipairs(chatEntitiesPool) do
		if v.type == chatEntity.type and v.chatName == chatEntity.chatName and (v.chanID == chatEntity.chanID or chatEntity.type == 'im') then
			return i
		end

		if v.type == nil and emptyPos == -1 then
			emptyPos = i
		end
	end

	if emptyPos ~= -1 then
		chatEntitiesPool[emptyPos] = chatEntity
		return emptyPos
	else
		table.insert(chatEntitiesPool, chatEntity)
		return #chatEntitiesPool
	end
end

local function IsMatchChannel(channelName)
	return string.lower(string.sub(channelName, 1, 5)) == 'match' and string.len(channelName) > 5
end

function Communicator_V2:Init()
	local widget = GetWidget('communicator_main')

	widget:RegisterWatch("ChatWhisperUpdate", function(_, ...) Communicator_V2:OnChatWhisperUpdate(...) end)
	widget:RegisterWatch("ChatNewGame", function(_, ...) Communicator_V2:OnChatNewGame(...) end)
	widget:RegisterWatch("ChatNewChannel", function(_, ...) Communicator_V2:OnChatNewChannel(...) end)
	widget:RegisterWatch("ChatSetFocusChannel", function(_, ...) Communicator_V2:OnChatSetFocusChannel(...) end)
	widget:RegisterWatch("AllChatMessages", function(_, ...) Communicator_V2:OnAllChatMessages(...) end)
	widget:RegisterWatch("ChatUserNames", function(_, ...) Communicator_V2:OnChatUserNames(...) end)
	widget:RegisterWatch("ChatUserEvent", function(_, ...) Communicator_V2:OnChatUserEvent(...) end)
	widget:RegisterWatch("ChatLeftChannel", function(_, ...) Communicator_V2:OnChatLeftChannel(...) end)
	widget:RegisterWatch("ChatLeftGame", function(_, ...) Communicator_V2:OnChatLeftGame(...) end)
	widget:RegisterWatch("GamePhase", function(_, ...) Communicator_V2:OnGamePhase(...) end)
	for i=0, 31 do
		widget:RegisterWatch("LobbyPlayerList"..i, function(_, ...) Communicator_V2:OnLobbyPlayerList(i, ...) end)
	end
	Communicator_V2:ChatRefreshTab()
end

function Communicator_V2:OnChatWhisperUpdate(name, text, info)
	if GetChatModeType() == 2 then
		return 
	end
	
	if (GameChat) then
		GameChat.ChatWhisperUpdate(name, text, info)
	end

	local exist = false
	for i,v in ipairs(chatTabs) do
		local chatEntity = chatEntitiesPool[v]
		if chatEntity.type == 'im' and chatEntity.chatName == name then
			exist = true
			break
		end
	end

	if not exist and info ~= '0' then Communicator_V2:StartChatWithPlayer(name) end

	for i,v in pairs(chatEntitiesPool) do
		if v.type == 'im' then
			if v.chatName == name then
				table.insert(v.history, text)
				if (#v.history > CHAT_HISTORY_MAX) then
					table.remove(v.history, 1)
				end

				if i == chatTabs[selectedChatTab] then
					GetWidget('communicator_chatbuffer'):AddBufferText(text)
					if not GetWidget('communicator_main_large'):IsVisible() then
						v.new = true
					else
						PlaySound('/shared/sounds/ui/ccpanel/cc_recieved_im.wav')
					end
				else
					v.new = true
				end
			end
		end
	end

	Communicator_V2:ChatRefreshTab()
end

function Communicator_V2:OnChatNewGame()
	if chatEntitiesPool[chatTabs[1]].type ~= 'channel' or
		chatEntitiesPool[chatTabs[1]].chatName ~= CHAT_GAME_TEXT then

		if (UIGamePhase() >= 2) then
			_chatSwitched = true
			GetWidget('communicator_main_input_chattype'):SetButtonState(1)
		else
			_chatSwitched = false
			GetWidget('communicator_main_input_chattype'):SetButtonState(0)
		end

		local chatEntity = { type = 'channel',
				   			 chanID = -2,
				  			 chatName = CHAT_GAME_TEXT,
				  			 users = _gameChannelUsers,
				  			 history = {}}
		local index = InsertToChatEntitiesPool(chatEntity)
		table.insert(chatTabs, 1, index)
		Communicator_V2:UpdateChatTabVariables()
		Communicator_V2:ChatJumpToTargetChat(1)
	end
end

function Communicator_V2:OnChatLeftGame()
	for i,v in ipairs(chatTabs) do
		local chatEntity = chatEntitiesPool[v]
		if chatEntity.type == 'channel' and chatEntity.chanID == -2 then
			table.remove(chatTabs, i)
			Communicator_V2:UpdateChatTabVariables()
			chatEntitiesPool[v] = {}
			_chatSwitched = false

			if i == selectedChatTab then
				local newSelection = i
				if i > #chatTabs then newSelection = i -1 end

				Communicator_V2:ChatJumpToTargetChat(newSelection)

			elseif i < selectedChatTab then
				selectedChatTab = selectedChatTab - 1
				Communicator_V2:ChatJumpToTargetChat(selectedChatTab)

			else
				Communicator_V2:ChatJumpToTargetChat(selectedChatTab)
			end
		end
	end

	_gameChannelUsers = {}
end

function Communicator_V2:OnChatNewChannel(chanID, chanName)
	-- Echo('^g~~~~~~~~~~~~~~~~~~~~~~ On Chat new Channel ~~~~~~~~~~~~~~~~~~~~~~')
	-- Echo('^gChannel ID:'..chanID)
	-- Echo('^gChannel Name:'..chanName..'|')

	if Empty(chanName) then return end

	chanID = tonumber(chanID)

	local chatEntity = { type = 'channel',
				   		 chanID = chanID,
				  		 chatName = chanName,
				  		 history = {}}
	local index = InsertToChatEntitiesPool(chatEntity)

	for i,v in ipairs(chatTabs) do
		if v == index then
			Communicator_V2:ChatJumpToTargetChat(i)
			return
		end
	end

	for i,v in ipairs(chatTabs) do
		if v == 1 then
			table.insert(chatTabs, i+1, index)
			Communicator_V2:UpdateChatTabVariables()
			if IsMatchChannel(chatEntity.chatName) then
				Communicator_V2:ChatRefreshTab()
			else
				Communicator_V2:ChatJumpToTargetChat(i+1)
			end

			if _matchChannelFirst then
				Communicator_V2:FocusToPostMatchChannel()
			end
			return
		end
	end

	table.insert(chatTabs, index)
	Communicator_V2:UpdateChatTabVariables()

	if IsMatchChannel(chatEntity.chatName) then
		Communicator_V2:ChatRefreshTab()
	else
		Communicator_V2:ChatJumpToTargetChat(#chatTabs)
	end

	if _matchChannelFirst then
		Communicator_V2:FocusToPostMatchChannel()
	end
end

function Communicator_V2:OnChatSetFocusChannel(chanID, chanName)
	chanID = tonumber(chanID)

	for i,v in ipairs(chatTabs) do
		if chatEntitiesPool[v] ~= nil and chatEntitiesPool[v].type == 'channel' and chatEntitiesPool[v].chanID == chanID then
			selectedChatTab = i
			focusChannel = chanID
			Communicator_V2:ChatRefreshTab()
			Communicator_V2:ChatRefreshContent()
			Communicator_V2:UserRefresh(true)
			break
		end
	end
end

function Communicator_V2:OnAllChatMessages(msgType, channelName, text, entity, noFormatting, isSelf)
	if (GameChat and UIGamePhase() > 0 and UIGamePhase() <= 4) then
		GameChat:AllChatMessages(msgType, channelName, text, entity, noFormatting, isSelf)
	end

	local messageType = tonumber(msgType)
	local chanName = channelName

	if messageType == 0 then return end

	for i,v in pairs(chatEntitiesPool) do
		if v.type == 'channel' then
			if ((messageType > 0 and messageType <= 2) or (messageType > 9)) and chanName == '' then -- the message need to notify every channel
				table.insert(v.history, text)
				if (#v.history > CHAT_HISTORY_MAX) then
					table.remove(v.history, 1)
				end

				if i == chatTabs[selectedChatTab] then
					GetWidget('communicator_chatbuffer'):AddBufferText(text)
				end
			else
				if messageType >= 3 and messageType <= 9 and chanName == '' then
					chanName = chatEntitiesPool[chatTabs[1]].chatName
				end

				if v.chatName == chanName then
					table.insert(v.history, text)
					if (#v.history > CHAT_HISTORY_MAX) then
						table.remove(v.history, 1)
					end

					if i == chatTabs[selectedChatTab] then
						GetWidget('communicator_chatbuffer'):AddBufferText(text)
						if not GetWidget('communicator_main_large'):IsVisible() then
							v.new = true
						end
					else
						v.new = true
					end
				end
			end
		end
	end

	Communicator_V2:ChatRefreshTab()
end

function Communicator_V2:OnChatUserNames(...)
	usersPool[arg[2]] = usersPool[arg[2]] or {}

	usersPool[arg[2]].isStaff = (arg[3] == "4")
	usersPool[arg[2]].ingame = (arg[4] == "1")
	usersPool[arg[2]].isPremium = (arg[5] == "1")
	usersPool[arg[2]].accountID = tonumber(arg[6])
	usersPool[arg[2]].chatSymbolTexture = arg[7]
	usersPool[arg[2]].chatNameColorTexture = arg[8]
	usersPool[arg[2]].chatNameColorString = arg[9]
	usersPool[arg[2]].chatNameColorIngame = arg[10]
	usersPool[arg[2]].accountIconTexture = arg[11]
	usersPool[arg[2]].chatNameGlow = (arg[13] == "true")
	usersPool[arg[2]].chatNameGlowColor = arg[14]
	usersPool[arg[2]].chatNameGlowColorIngame = arg[15]
	usersPool[arg[2]].ascensionLevel = arg[16]
	usersPool[arg[2]].chatNameColorFont = arg[17]
	usersPool[arg[2]].chatNameBackgroundGlow = AtoB(arg[18])
	usersPool[arg[2]].sortIndex = usersPool[arg[2]].sortIndex or {}

	local sortIndexStr = string.format('%03d', tonumber(arg[12]))
	usersPool[arg[2]].sortIndex[arg[1]] = tonumber(arg[4]..tostring((4 - tonumber(arg[3])))..sortIndexStr)

	for i,v in pairs(chatEntitiesPool) do
		if v.type == 'channel' and v.chatName == arg[1] then
			v.users = v.users or {}
			local exist = false

			for _, u in ipairs(v.users) do
				if u == arg[2] then
					exist = true
					break
				end
			end

			if not exist then
				table.insert(v.users, arg[2])
				break
			end
		end
	end
end

function Communicator_V2:OnChatUserEvent(chanName, command)
	for k,v in ipairs(chatEntitiesPool) do
		if v.type == 'channel' and v.chatName == chanName then
			if string.find(command, 'ClearItems') then
				v.users = {}
			elseif string.find(command, 'SortListboxSortIndex') and k == chatTabs[selectedChatTab] then
				table.sort(v.users, function(a,b)
					if (usersPool[a].sortIndex[chanName] == usersPool[b].sortIndex[chanName]) then
						return string.lower(StripClanTag(a)) < string.lower(StripClanTag(b))
					else
						return usersPool[a].sortIndex[chanName] < usersPool[b].sortIndex[chanName]
					end
				end)
				Communicator_V2:UserRefresh(false)
			elseif string.find(command, 'EraseListItemByValue') then
				local username = string.sub(command, 23, string.len(command)-3)

				for i,u in ipairs(v.users) do
					if u == username then
						table.remove(v.users, i)
						break
					end
				end

				if k == chatTabs[selectedChatTab] then
					Communicator_V2:UserRefresh(false)
				end
			end

			break
		end
	end
end

function Communicator_V2:OnChatLeftChannel(chanID)
	chanID = tonumber(chanID)

	if chanID == -1 then
		Communicator_V2:OnChatLeftGame()

		for i,v in ipairs(chatTabs) do
			local chatEntity = chatEntitiesPool[v]

			if chatEntity.type == 'channel' and chatEntity.chanID ~= -1 then
				chatEntitiesPool[v] = {}
			end
		end

		chatTabs = {}
		table.insert(chatTabs, 1)
		Communicator_V2:UpdateChatTabVariables()
		selectedChatTab = 1
		firstChatTab = 1
		focusChannel = nil
		Communicator_V2:ChatRefreshTab()
	else
		for i,v in ipairs(chatTabs) do
			local chatEntity = chatEntitiesPool[v]
			if chatEntity.type == 'channel' and chatEntity.chanID == chanID then
				table.remove(chatTabs, i)
				Communicator_V2:UpdateChatTabVariables()
				chatEntitiesPool[v] = {}

				if i == selectedChatTab then
					local newSelection = i
					if i > #chatTabs then newSelection = i -1 end

					Communicator_V2:ChatJumpToTargetChat(newSelection)

				elseif i < selectedChatTab then
					selectedChatTab = selectedChatTab - 1
					Communicator_V2:ChatJumpToTargetChat(selectedChatTab)

				else
					Communicator_V2:ChatJumpToTargetChat(selectedChatTab)
				end
				break
			end
		end
	end
end

function Communicator_V2:OnLogout()
	local widget = GetWidget('communicator_chatbuffer')
	widget:SetValue(0)
	widget:ClearBufferText()

	chatEntitiesPool = {}
	chatEntitiesPool[1] = {	type = 'channel',
				   			chanID = -1,
				  			chatName = CHAT_STATUS_TEXT,
				  			history = {}}

	chatTabs = {}
	table.insert(chatTabs, 1)
	Communicator_V2:UpdateChatTabVariables()

	chatIndex = 2
	selectedChatTab = 1
	firstChatTab = 1
	focusChannel = nil
	Communicator_V2:ChatRefreshTab()
end

function Communicator_V2:OnGamePhase(gamePhase)
	if (tonumber(gamePhase) == 0) then
		Communicator_V2:OnChatLeftGame()
		Communicator_V2:FocusToPostMatchChannel()
		_matchChannelFirst = true
		GetWidget('communicator_main'):Sleep(4000, function() _matchChannelFirst = false end)
	elseif (tonumber(gamePhase) == 1) then
		Communicator_V2:ChatJumpToTargetChat(1)
	elseif (tonumber(gamePhase) >= 2 and not _chatSwitched) then -- switch to team chat for default
		GetWidget('communicator_main_input_chattype'):SetButtonState(1)
		_chatSwitched = true
	end
end

function Communicator_V2:OnLobbyPlayerList(index, ...)

	local users = _gameChannelUsers

	if arg[1] == '-1' then
		users[index+1] = nil
	else
		usersPool[arg[2]] = usersPool[arg[2]] or {}

		usersPool[arg[2]].clientNum = tonumber(arg[1])
		usersPool[arg[2]].isLoading = AtoB(arg[5])
		usersPool[arg[2]].loadingProgress = tonumber(arg[8])
		usersPool[arg[2]].isStaff = AtoB(arg[9])
		usersPool[arg[2]].ingame = true
		usersPool[arg[2]].isPremium = AtoB(arg[10])
		usersPool[arg[2]].showKick = AtoB(arg[11])
		usersPool[arg[2]].accountID = tonumber(arg[12])
		usersPool[arg[2]].chatSymbolTexture = arg[13]
		usersPool[arg[2]].chatNameColorTexture = arg[14]
		usersPool[arg[2]].chatNameColorString = arg[15]
		usersPool[arg[2]].chatNameColorIngame = arg[16]
		usersPool[arg[2]].accountIconTexture = arg[17]
		usersPool[arg[2]].chatNameGlow = AtoB(arg[21])
		usersPool[arg[2]].chatNameGlowColor = arg[23]
		usersPool[arg[2]].chatNameGlowColorIngame = arg[24]
		usersPool[arg[2]].ascensionLevel = '0'
		usersPool[arg[2]].chatNameColorFont = arg[25]
		usersPool[arg[2]].chatNameBackgroundGlow = AtoB(arg[26])
		usersPool[arg[2]].sortIndex = usersPool[arg[2]].sortIndex or {}
		usersPool[arg[2]].sortIndex[CHAT_GAME_TEXT] = tonumber(arg[12])

		users[index+1] = arg[2]
	end
	for i,v in ipairs(chatEntitiesPool) do
		if v.type == 'channel' and v.chatName == CHAT_GAME_TEXT and i == chatTabs[selectedChatTab] then
			Communicator_V2:UserRefresh(false)
		end
	end
end
----------------------------------------------------------------------
function Communicator_V2:UpdateChatTabVariables()
	local rootWidth = GetWidget('communicator_main_large_tabroot'):GetWidth()
	local tabNum = #chatTabs

	local total = CHAT_TAB_WIDTH_MAX + CHAT_TAB_SELECTED_LONG + CHAT_TAB_WIDTH_MAX * (tabNum-1) + CHAT_TAB_GAP_MAX * tabNum
	if total <= rootWidth then
		CHAT_TAB_MAX 				= tabNum
		CHAT_TAB_SELECTED_WIDTH 	= CHAT_TAB_WIDTH_MAX + CHAT_TAB_SELECTED_LONG
		CHAT_TAB_NOT_SELECTED_WIDTH	= CHAT_TAB_WIDTH_MAX
		CHAT_TAB_GAP 				= CHAT_TAB_GAP_MAX

		if firstChatTab + CHAT_TAB_MAX > #chatTabs then
			firstChatTab = #chatTabs - CHAT_TAB_MAX + 1
			if firstChatTab < 1 then
				firstChatTab = 1
			end
		end
		return
	end

	total = CHAT_TAB_WIDTH_MIN + CHAT_TAB_SELECTED_LONG + CHAT_TAB_WIDTH_MIN * (tabNum-1) + CHAT_TAB_GAP_MIN * tabNum
	if total > rootWidth then
		CHAT_TAB_MAX 				= math.floor((rootWidth - CHAT_TAB_SELECTED_LONG) / (CHAT_TAB_WIDTH_MIN + CHAT_TAB_GAP_MIN))

		local width = math.floor((rootWidth - CHAT_TAB_MAX * CHAT_TAB_GAP_MIN - CHAT_TAB_SELECTED_LONG) / CHAT_TAB_MAX)

		CHAT_TAB_SELECTED_WIDTH 	= width + CHAT_TAB_SELECTED_LONG
		CHAT_TAB_NOT_SELECTED_WIDTH	= width
		CHAT_TAB_GAP 				= CHAT_TAB_GAP_MIN

		if firstChatTab + CHAT_TAB_MAX > #chatTabs then
			firstChatTab = #chatTabs - CHAT_TAB_MAX + 1
			if firstChatTab < 1 then
				firstChatTab = 1
			end
		end
		return
	end

	local width = math.floor((rootWidth - tabNum * CHAT_TAB_GAP_MIN - CHAT_TAB_SELECTED_LONG) / tabNum)
	CHAT_TAB_MAX 				= tabNum
	CHAT_TAB_SELECTED_WIDTH 	= width + CHAT_TAB_SELECTED_LONG
	CHAT_TAB_NOT_SELECTED_WIDTH	= width
	CHAT_TAB_GAP 				= CHAT_TAB_GAP_MIN

	if firstChatTab + CHAT_TAB_MAX > #chatTabs then
			firstChatTab = #chatTabs - CHAT_TAB_MAX + 1
			if firstChatTab < 1 then
				firstChatTab = 1
			end
		end
end

function Communicator_V2:ChatRefreshTab()
	local function SetChatTab(i, x, width, selected, chatEntity)
		if chatEntity == nil then GetWidget('communicator_chat_'..i):SetVisible(false) return end

		GetWidget('communicator_chat_'..i):SetVisible(true)
		GetWidget('communicator_chat_'..i):SetX(tostring(x))
		GetWidget('communicator_chat_'..i):SetWidth(tostring(width))
		GetWidget('communicator_chat_'..i..'_button_selected'):SetVisible(selected)
		GetWidget('communicator_chat_'..i..'_button_notselected'):SetVisible(not selected)
		GetWidget('communicator_chat_'..i..'_notification'):SetVisible(chatEntity.new ~= nil and chatEntity.new and not selected)
		if chatEntity.new ~= nil and chatEntity.new then
			GetWidget('communicator_expand_notification'):SetVisible(true)
		end

		local selectedIcon = ''
		local notSelectedIcon = ''
		if chatEntity.type == 'im' then
			selectedIcon = '/ui/fe2/newui/res/communicator/chat1.png'
			notSelectedIcon = '/ui/fe2/newui/res/communicator/chat2.png'
		elseif chatEntity.chatName == CHAT_GAME_TEXT or chatEntity.chatName == CHAT_STATUS_TEXT then
			selectedIcon = '/ui/fe2/newui/res/communicator/status1.png'
			notSelectedIcon = '/ui/fe2/newui/res/communicator/status2.png'
		else
			selectedIcon = '/ui/fe2/newui/res/communicator/channel1.png'
			notSelectedIcon = '/ui/fe2/newui/res/communicator/channel2.png'
		end

		local needTip = false
		for j=1,3 do
			if selected then
				GetWidget('communicator_chat_'..i..'_button_selected_c'..j):SetWidth(width-2*CHAT_TAB_EDGE_WIDTH)
				GetWidget('communicator_chat_'..i..'_button_selected_icon'..j):SetTexture(selectedIcon)
				needTip = Common_V2:SetLongText(GetWidget('communicator_chat_'..i..'_button_selected_name'..j), chatEntity.chatName)
			else
				GetWidget('communicator_chat_'..i..'_button_notselected_c'..j):SetWidth(width-2*CHAT_TAB_EDGE_WIDTH)
				GetWidget('communicator_chat_'..i..'_button_notselected_icon'..j):SetTexture(notSelectedIcon)
				needTip = Common_V2:SetLongText(GetWidget('communicator_chat_'..i..'_button_notselected_name'..j), chatEntity.chatName)
			end
		end

		local btn = nil
		if selected then
			btn = GetWidget('communicator_chat_'..i..'_button_selected')
		else
			btn = GetWidget('communicator_chat_'..i..'_button_notselected')
		end

		GetWidget('communicator_chat_'..i..'_tip_value'):SetColor(selected and '#eaccba' or '#7a5f51')
		GetWidget('communicator_chat_'..i..'_tip_value'):SetText(chatEntity.chatName)
		GetWidget('communicator_chat_'..i..'_tip_root'):SetVisible(needTip)
	end

	local line1 = 0
	local line2 = 0
	local pos = 0

	if firstChatTab ~= selectedChatTab then pos = CHAT_TAB_GAP end

	for i=1,CHAT_TAB_WIDGET_MAX do
		GetWidget('communicator_chat_'..i):SetVisible(false)
	end
	GetWidget('communicator_expand_notification'):SetVisible(false)

	for i=1, CHAT_TAB_MAX do
		local chatEntityIndex = chatTabs[firstChatTab + i - 1]
		if chatEntityIndex ~= nil and chatEntitiesPool[chatEntityIndex] ~= nil then

			local chatEntity = chatEntitiesPool[chatEntityIndex]
			if 	(firstChatTab + i - 1) == selectedChatTab then
				SetChatTab(i, pos, CHAT_TAB_SELECTED_WIDTH, true, chatEntity)

				line1 = pos
				pos = pos + CHAT_TAB_SELECTED_WIDTH
				line2 = pos
			else
				SetChatTab(i, pos, CHAT_TAB_NOT_SELECTED_WIDTH, false, chatEntity)

				pos = pos + CHAT_TAB_NOT_SELECTED_WIDTH
			end

			pos = pos + CHAT_TAB_GAP
		else
			SetChatTab(i, pos, CHAT_TAB_NOT_SELECTED_WIDTH, false, nil)
		end
	end

	if line1 > 0 then
		GetWidget('communicator_chat_tab_line1'):SetVisible(true)
		GetWidget('communicator_chat_tab_line1'):SetWidth(tostring(line1))
	else
		GetWidget('communicator_chat_tab_line1'):SetVisible(false)
	end

	if line2 > 0 then
		GetWidget('communicator_chat_tab_line2'):SetWidth('-'..tostring(line2))
	else
		GetWidget('communicator_chat_tab_line2'):SetWidth('100%')
	end

	GetWidget('communicator_main_large_chat_lastpage'):SetVisible(CHAT_TAB_MAX < #chatTabs)
	GetWidget('communicator_main_large_chat_nextpage'):SetVisible(CHAT_TAB_MAX < #chatTabs)
	GetWidget('communicator_main_large_chat_lastpage'):SetEnabled(firstChatTab > 1)
	GetWidget('communicator_main_large_chat_nextpage'):SetEnabled(firstChatTab + CHAT_TAB_MAX <= #chatTabs)
end

function Communicator_V2:ChatRefreshContent()
	local chatEntity = chatEntitiesPool[chatTabs[selectedChatTab]]

	if chatEntity.type == 'channel' and chatEntity.chanID == -2 then
		GetWidget('communicator_main_input_chattype'):SetVisible(true)
		GetWidget('communicator_send_box'):SetWidth('-155i')
	else
		GetWidget('communicator_main_input_chattype'):SetVisible(false)
		GetWidget('communicator_send_box'):SetWidth('-105i')
	end

	chatEntity.new = false

	local widget = GetWidget('communicator_chatbuffer')
	widget:SetValue(0)
	widget:ClearBufferText()

	for _, text in ipairs(chatEntity.history) do
		widget:AddBufferText(text)
	end
end

function Communicator_V2:ChatJumpToTargetChat(index)
	if index < firstChatTab then
		firstChatTab = firstChatTab - CHAT_TAB_MAX
		if firstChatTab < 1 then
			firstChatTab = 1
		end
	elseif index >= firstChatTab + CHAT_TAB_MAX then
		firstChatTab = firstChatTab + CHAT_TAB_MAX
		if firstChatTab + CHAT_TAB_MAX > #chatTabs then
			firstChatTab = #chatTabs - CHAT_TAB_MAX + 1
		end
	end

	Communicator_V2:ChatOnClickTab(index - firstChatTab + 1)

	if GetWidget('communicator_main_large'):IsVisible() then
		GetWidget('communicator_send_box'):SetFocus(true)
	end
end

function Communicator_V2:UserRefresh(resetScroll)
	local chatEntity = chatEntitiesPool[chatTabs[selectedChatTab]]
	chatEntity.users = chatEntity.users or {}
	if chatEntity.type == 'channel' and chatEntity.chatName ~= CHAT_STATUS_TEXT then
		GetWidget('communicator_main_large_users'):SetVisible(true)

		if resetScroll and chatEntity.chatName ~= CHAT_GAME_TEXT then
			local chanName = chatEntity.chatName
			table.sort(chatEntity.users, function(a,b)
				if (usersPool[a].sortIndex[chanName] == usersPool[b].sortIndex[chanName]) then
					return string.lower(StripClanTag(a)) < string.lower(StripClanTag(b))
				else
					return usersPool[a].sortIndex[chanName] < usersPool[b].sortIndex[chanName]
				end
			end)
		end

		local channelName = chatEntity.chatName..'('..tostring(#chatEntity.users)..')'
		Common_V2:SetLongTextLabel('communicator_main_large_channelname', chatEntity.chatName, '('..tostring(#chatEntity.users)..')')

		local scrollMaxValue = #chatEntity.users - USER_NUMBER_MAX + 1
		if scrollMaxValue < 1 then scrollMaxValue = 1 end

		local scrollWidget = GetWidget('communicator_user_scrollbar')
		scrollWidget:SetMaxValue(scrollMaxValue)
		if resetScroll then
			scrollWidget:SetValue(1)
		else
			scrollWidget:SetValue(scrollWidget:GetValue())
		end
		scrollWidget:SetVisible(#chatEntity.users > USER_NUMBER_MAX)
		Communicator_V2:UserPopulate()
	else
		GetWidget('communicator_main_large_users'):SetVisible(false)
	end
end

function Communicator_V2:UserPopulate()
	local startIndex = tonumber(GetWidget('communicator_user_scrollbar'):GetValue())
	local maxValue = tonumber(GetWidget('communicator_user_scrollbar'):UICmd("GetScrollbarMaxValue()"))
	if startIndex < 1 then startIndex = 1 end
	if startIndex > maxValue then startIndex = maxValue end

	local chatEntity = chatEntitiesPool[chatTabs[selectedChatTab]]
	local users = chatEntity.users
	local isGameChannel = chatEntity.type == 'channel' and chatEntity.chatName == CHAT_GAME_TEXT

	for i=1, USER_WIDGET_MAX do
		GetWidget('communicator_user_'..i):SetVisible(false)
	end

	for i=1,USER_NUMBER_MAX do
		if users[startIndex+i-1] ~= nil then
			GetWidget('communicator_user_'..i):SetVisible(true)

			local user = usersPool[users[startIndex+i-1]]
			local labelWidget = GetWidget('communicator_user_'..i..'_label')
			local avatarWidget = GetWidget('communicator_user_'..i..'_avatar')
			local symbolWidget = GetWidget('communicator_user_'..i..'_symbol')
			local avatarMask = GetWidget('communicator_user_'..i..'_avatarmask')
			local symbolMask = GetWidget('communicator_user_'..i..'_symbolmask')
			local kickWidget = GetWidget('communicator_user_'..i..'_kick')
			local progressWidget = GetWidget('communicator_user_'..i..'_progress')

			local text = users[startIndex+i-1]
			local font = NotEmpty(user.chatNameColorFont) and user.chatNameColorFont..'_10' or 'dyn_10'
			local color = user.chatNameColorString

			if Empty(color) or color == '#FFFFFF' then 
				if (not user.ingame) then
					if (user.isStaff) then 
						color = '#FF0000'
					elseif (user.isPremium) then 
						color = '#DBBF4A'
					else 
						color = '#FFFFFF'
					end
				else
					if (user.isStaff) then 
						color = '#770000'
					elseif (user.isPremium) then 
						color = '#6E6025'
					else 
						color = '#777777'
					end
				end
			end

			if isGameChannel and user.isLoading then
				local value = math.floor(user.loadingProgress * 100)
				text = text..' ['..value..'%]'
			end

			if font == '8bit_10' then
				text = string.upper(text)
			end

			labelWidget:SetText(text)
			labelWidget:SetFont(font)
			labelWidget:SetColor(color)
			labelWidget:SetGlow(user.chatNameGlow)
			labelWidget:SetBackgroundGlow(user.chatNameBackgroundGlow)
			labelWidget:SetGlowColor(user.chatNameGlowColor)

			if (user.accountIconTexture and user.accountIconTexture ~= "") then
				avatarWidget:SetVisible(1)
				avatarWidget:SetTexture(user.accountIconTexture)
				avatarMask:SetVisible(user.accountIconTexture == '/ui/fe2/store/icons/account_icons/default.tga')
			elseif (user.accountID > 0) then
				avatarWidget:SetVisible(1)
				avatarWidget:UICmd("SetAvatar('http://www.heroesofnewerth.com/getAvatar.php?id="..user.accountID.."')")
				avatarMask:SetVisible(false)
			else
				avatarWidget:SetVisible(0)
			end

			if (user.chatSymbolTexture and user.chatSymbolTexture ~= "") then
				symbolWidget:SetTexture(user.chatSymbolTexture)
			elseif (user.chatNameColorTexture and user.chatNameColorTexture ~= "") then
				symbolWidget:SetTexture(user.chatNameColorTexture)
			else
				if (user.isStaff) then
					symbolWidget:SetTexture(GetCvarString('ui_staffIconPath'))
				elseif (user.isPremium) then
					symbolWidget:SetTexture("/ui/icons/premium.tga")
				else
					symbolWidget:SetTexture("/ui/icons/ingame_2.tga")
				end
			end
			symbolMask:SetVisible(symbolWidget:GetTexture() == '/ui/icons/ingame_2.tga')

			if (user.ingame and not isGameChannel) then
				symbolWidget:SetRenderMode("grayscale")
				symbolWidget:SetColor("1 1 1 .3")
			else
				symbolWidget:SetRenderMode("normal")
				symbolWidget:SetColor("1 1 1 1")
			end

			if isGameChannel and user.isLoading then
				progressWidget:SetVisible(true)
				progressWidget:SetWidth(tostring(user.loadingProgress * 100)..'%')
			else
				progressWidget:SetVisible(false)
			end

			-- ascension
			local level = math.abs(tonumber(user.ascensionLevel))
			if level > 9999 then
				level = 9999;
			end
			GetWidget("communicator_user_"..i.."_ascension"):SetVisible(level > 0)

			if (level > 0) then
				local ascensionImg = 0
				if (level >= 50) then
					ascensionImg = 0
				elseif level >= 30 then
					ascensionImg = 1
				else
					ascensionImg = 2
				end

				for j=0,2 do
					GetWidget("communicator_user_"..i.."_ascension_img_"..j):SetVisible(j == ascensionImg)
				end

				GetWidget("communicator_user_"..i.."_ascension_label"):SetText(tostring(level))
			end


			kickWidget:SetVisible(isGameChannel and user.showKick)
			kickWidget:SetCallback('onclick', function() 
				Lobby_V2:KickPlayer(user.clientNum)
			end)
		else
			GetWidget('communicator_user_'..i):SetVisible(false)
		end
	end
end

function Communicator_V2:CloseChat(index)
	local chatEntity = chatEntitiesPool[chatTabs[index]]

	if chatEntity.type == 'channel' then
		if string.sub(chatEntity.chatName, 1, 5) == 'Group' and IsInGroup() then
			interface:UICmd("LeaveTMMGroup();")
		end

		interface:UICmd("ChatLeaveChannel('"..chatEntity.chatName.."');")
	else
		table.remove(chatTabs, index)
		Communicator_V2:UpdateChatTabVariables()
		if index == selectedChatTab then
			local newSelection = index
			if index > #chatTabs then newSelection = index -1 end
			Communicator_V2:ChatJumpToTargetChat(newSelection)
		elseif index < selectedChatTab then
			selectedChatTab = selectedChatTab - 1
			Communicator_V2:ChatJumpToTargetChat(selectedChatTab)
		else
			Communicator_V2:ChatJumpToTargetChat(selectedChatTab)
		end
	end
end

function Communicator_V2:StartChatWithPlayer(name, focus)
	local nickname = StripClanTag(name)

	local chatEntity = { type = 'im',
						 chanID = -99,
				  		 chatName = nickname,
				  		 history = {}}

	local index = InsertToChatEntitiesPool(chatEntity)

	local target = -1
	for i,v in ipairs(chatTabs) do
		if v == index then
			target = i
			break
		end
	end

	if target == -1 then
		local inserted = false
		for i,v in ipairs(chatTabs) do
			if v == 1 and focus then
				table.insert(chatTabs, i+1, index)
				Communicator_V2:UpdateChatTabVariables()
				target = i+1
				inserted = true
			end
		end

		if not inserted then
			table.insert(chatTabs, index)
			Communicator_V2:UpdateChatTabVariables()
			target = #chatTabs
		end
	end

	if focus then
		Communicator_V2:ChatJumpToTargetChat(target)
	else
		Communicator_V2:ChatRefreshTab()
	end
end

function Communicator_V2:FocusToPostMatchChannel()
	for i,v in ipairs(chatTabs) do
		local chatEntity = chatEntitiesPool[v]

		if IsMatchChannel(chatEntity.chatName) then
			Communicator_V2:ChatJumpToTargetChat(i)
			break
		end
	end
end
----------------------------------------------------------------------
function Communicator_V2:ChatOnFrame()
	local function SetCommunicator(config)
		if config.hide then
			GetWidget('communicator_main'):SetVisible(false)
			return
		end

		GetWidget('communicator_main'):SetVisible(true)
		GetWidget('communicator_main'):SetWidth(tostring(config.width)..'i')
		GetWidget('communicator_main'):SetX(tostring(config.x)..'i')

		GetWidget('communicator_main_large'):SetWidth(tostring(config.width)..'i')
		if config.height ~= nil then
			GetWidget('communicator_main_large'):SetHeight(tostring(config.height)..'i')
		else
			GetWidget('communicator_main_large'):SetHeight('287i')
		end

		local gap = config.gap or 15
		GetWidget('communicator_main_large'):SetY('-'..tostring(gap+47)..'i')

		if not config.sticky then
			GetWidget('communicator_main_large'):SetVisible(false)
			GetWidget('communicator_expand'):SetVisible(true)
			GetWidget('communicator_collapse'):SetVisible(false)
			GetWidget('communicator_send_box'):SetX('45i')
			GetWidget('communicator_send_box'):SetWidth('-105i')
			GetWidget('communicator_main_large_bg'):SetVisible(true)
			GetWidget('communicator_main_large_scale'):SetWidth('0%')
			GetWidget('communicator_main_large_scale'):SetHeight('0%')
		else
			GetWidget('communicator_main_large'):SetVisible(true)
			GetWidget('communicator_expand'):SetVisible(false)
			GetWidget('communicator_collapse'):SetVisible(false)
			GetWidget('communicator_send_box'):SetX('0')
			GetWidget('communicator_send_box'):SetWidth('-60i')
			GetWidget('communicator_main_large_bg'):SetVisible(false)
			GetWidget('communicator_main_large_scale'):SetWidth('100%')
			GetWidget('communicator_main_large_scale'):SetHeight('100%')
		end

		GetWidget('communicator_main_popup_joinchannel'):SetVisible(false)

		USER_NUMBER_MAX = config.usermax or 6

		Communicator_V2:UpdateChatTabVariables()
		Communicator_V2:ChatRefreshTab()
	 	Communicator_V2:ChatRefreshContent()
	 	Communicator_V2:UserRefresh(true)
	end

  	local matched = false
	for i,v in ipairs(_chatDisplayConfig) do
		if GetWidget(v.target):IsVisible() then
			if _chatType ~= i then
				SetCommunicator(v)
				_chatType = i
			end

			matched = true
			break
		end
	end
	if not matched and _chatType ~= 0 then
		SetCommunicator(_chatDisplayConfig['default'])
		_chatType = 0
	end
end

function Communicator_V2:ChatOnClickTab(i)
	local targetChatTab = firstChatTab + i - 1

	local chatEntity = chatEntitiesPool[chatTabs[targetChatTab]]

	if chatEntity.type == 'channel' then
		if chatEntity.chanID ~= focusChannel then
			interface:UICmd("SetFocusedChannel('"..chatEntity.chanID.."');")
		else
			Communicator_V2:OnChatSetFocusChannel(focusChannel)
		end
	else
		selectedChatTab = targetChatTab
		Communicator_V2:ChatRefreshTab()
		Communicator_V2:ChatRefreshContent()
		Communicator_V2:UserRefresh(true)
	end
end

function Communicator_V2:ChatOnRightClickTab(i)
	local targetChatTab = firstChatTab + i - 1

	local chatEntity = chatEntitiesPool[chatTabs[targetChatTab]]
	if chatEntity.type == 'channel' and (chatEntity.chanID == -1 or chatEntity.chanID == -2) then return end -- Status

	if chatEntity.type == 'channel' then
		local menutable = {}

		menu = {}
		menu.content = 'ui_items_chat_leavechan'
		menu.onclicklua = 'Communicator_V2:CloseChat('..tostring(targetChatTab)..')'
		table.insert(menutable, menu)

		if chatEntity.chanID ~= -1 and chatEntity.chanID ~= -2 then
			local isChannelAutoConnect = interface:UICmd("ChatIsSavedChannel('"..chatEntity.chatName.."')")
			if AtoB(isChannelAutoConnect) then 
				menu = {}
				menu.content = 'newui_general_remove_autoconnect'
				menu.onclicklua = 'Common_V2:SetAutoConnect(\''..chatEntity.chatName..'\',true)'
			else
				menu = {}
				menu.content = 'newui_general_set_autoconnect'
				menu.onclicklua = 'Common_V2:SetAutoConnect(\''..chatEntity.chatName..'\',false)'
			end

			table.insert(menutable, menu)
		end

		Common_V2:PopupMenu(menutable)
	else
		local menutable = {}

		menu = {}
		menu.content = 'newui_communicator_closechat'
		menu.onclicklua = 'Communicator_V2:CloseChat('..tostring(targetChatTab)..')'
		table.insert(menutable, menu)

		Common_V2:PopupMenu(menutable)
	end
end

function Communicator_V2:ChatOnClickChangePage(nextpage)
	if nextpage then
		firstChatTab = firstChatTab + CHAT_TAB_MAX
		if firstChatTab + CHAT_TAB_MAX > #chatTabs then
			firstChatTab = #chatTabs - CHAT_TAB_MAX + 1
		end
		Communicator_V2:ChatRefreshTab()
	else
		firstChatTab = firstChatTab - CHAT_TAB_MAX
		if firstChatTab < 1 then
			firstChatTab = 1
		end
		Communicator_V2:ChatRefreshTab()
	end
end

function Communicator_V2:ChatOnClickSendText()
	local msg = GetWidget("communicator_send_box"):GetValue()

	local chatEntity = chatEntitiesPool[chatTabs[selectedChatTab]]

	if (string.sub(msg, 1, 6) == "/clear") then
		chatEntity.history = {}
		GetWidget('communicator_chatbuffer'):ClearBufferText()
	elseif chatEntity.type == 'channel' then
		if chatEntity.chanID == -2 then
			local chatType = GetWidget('communicator_main_input_chattype'):GetButtonState()
			if chatType == 0 then
				interface:UICmd("AllChat('"..string.gsub(string.gsub(msg, "\\", "\\\\"), "'", "\\'").."')")
			else
				interface:UICmd("TeamChat('"..string.gsub(string.gsub(msg, "\\", "\\\\"), "'", "\\'").."')")
			end

			PlaySound('/shared/sounds/ui/revamp/chat_send.wav')
		else
			interface:UICmd("ChatSendMessage('"..string.gsub(string.gsub(msg, "\\", "\\\\"), "'", "\\'").."', '"..chatEntity.chatName.."');")

			PlaySound('/shared/sounds/ui/revamp/chat_send.wav')
		end
	else
		local muted = false
		local nickname = chatEntity.chatName
		if not IsChatMuted() then
			interface:UICmd("ChatSendIM('"..nickname.."', '"..string.gsub(string.gsub(msg, "\\", "\\\\"), "'", "\\'").."');")
		end
	end
end

function Communicator_V2:ChatOnClickJoinChannel()
	local channel = GetWidget('communicator_main_popup_joinchannel_name'):GetValue()
	if Empty(channel) then return end

	local isMatchChannel = false
	if IsMatchChannel(channel) then 
		isMatchChannel = true
	end

	if channel ~= CHAT_GAME_TEXT and string.lower(channel) ~= string.lower(CHAT_STATUS_TEXT) and not isMatchChannel then
		interface:UICmd("ChatLeaveChannel('"..channel.."'); ChatJoinChannel('"..channel.."');")
	end
	GetWidget('communicator_main_popup_joinchannel'):FadeOut(150)
end

function Communicator_V2:ChatOnClickShowJoinChannelPanel()
	local widget = GetWidget('communicator_main_popup_joinchannel')
	if widget:IsVisible() then
		widget:FadeOut(150)
	else
		widget:FadeIn(150)
		GetWidget('communicator_main_popup_joinchannel_name'):SetFocus(true)
		GetWidget('communicator_main_popup_joinchannel_name'):SetInputLine('')
	end
end

function Communicator_V2:GenerateRightClickMenu(nickname, channelID, from)
	if Empty(nickname) then return end

	if Empty(channelID) then
		channelID = -1
	end

	from = from or ''

	Echo('Communicator_V2:GenerateRightClickMenu channelID: '..channelID..' user: '..nickname)
	local isMe = IsMe(nickname)
	local myAdminLevel = ChatGetAdminLevel(GetAccountName(), tonumber(channelID))
	local userAdminLevel = ChatGetAdminLevel(StripClanTag(nickname), tonumber(channelID))

	local menutable = {}
	local menu = nil

	-- Send Message
	if not isMe then
		menu = {}
		menu.content = 'newui_communicator_sendmessage'
		menu.onclicklua = 'Communicator_V2:StartChatWithPlayer(\''..nickname..'\', true)'
		table.insert(menutable, menu)
	end
	-- View Stats
	menu = {}
	menu.content = 'ui_items_cc_right_click_view_stats'
	menu.onclicklua = 'Player_Stats_V2:Show(\''..nickname..'\')'
	table.insert(menutable, menu)
	-- Follow & UnFollow
	local following = AtoB(interface:UICmd('IsFollowing(\''..StripClanTag(nickname)..'\')'))
	if not isMe then
		menu = {}
		if following then
			menu.content = 'ui_items_cc_right_click_unfollow'
			menu.onclicklua = 'Common_V2:UnFollowPlayer(\''..nickname..'\')'
		else
			menu.content = 'ui_items_cc_right_click_follow'
			menu.onclicklua = 'Common_V2:FollowPlayer(\''..nickname..'\')'
		end
		table.insert(menutable, menu)
	end
	-- Join Game
	local chatInGame = AtoB(interface:UICmd('ChatInGame()'))
	local chatUserInGame = AtoB(interface:UICmd('ChatUserInGame(\''..StripClanTag(nickname)..'\')'))
	local chatHasServerInfo = AtoB(interface:UICmd('ChatHasServerInfo(\''..StripClanTag(nickname)..'\')'))

	if not isMe and not chatInGame and chatUserInGame and chatHasServerInfo then
		menu = {}
		menu.content = 'ui_items_cc_right_click_joing_game'
		menu.onclicklua = 'Common_V2:ChatJoinGame(\''..nickname..'\')'
		table.insert(menutable, menu)
	end
	-- Add Friend
	if not IsBuddy(nickname) and not isMe then
		menu = {}
		menu.content = 'ui_items_cc_right_click_add_buddy'
		menu.onclicklua = 'Common_V2:AddFriend(\''..nickname..'\')'
		table.insert(menutable, menu)
	elseif not isMe then
		menu = {}
		menu.content = 'ui_items_cc_right_click_remove_buddy'
		menu.onclicklua = 'Common_V2:RemoveFriend(\''..nickname..'\')'
		table.insert(menutable, menu)
	end
	-- Invite to game
	local isPrivateGame = AtoB(interface:UICmd('IsPrivateGame()'))
	local isHost = IsHost()
	local chatUserOnline = AtoB(interface:UICmd('ChatUserOnline(\''..StripClanTag(nickname)..'\')'))
	if not isMe and (not isPrivateGame or (isPrivateGame and isHost)) and chatInGame and chatUserOnline and not chatUserInGame then
		menu = {}
		menu.content = 'ui_items_cc_right_click_invite_to_game'
		menu.onclicklua = 'Common_V2:GameInvite(\''..nickname..'\')'
		table.insert(menutable, menu)
	end
	-- Get User Info
	if not isMe then
		menu = {}
		menu.content = 'ui_items_cc_right_click_user_info'
		menu.onclicklua = 'Common_V2:GetUserInfo(\''..nickname..'\')'
		table.insert(menutable, menu)
	end
	-- Kick from Channel
	-- Ban in Channel
	-- Silence in Channel
	if not isMe and myAdminLevel > userAdminLevel then
		menu = {}
		menu.content = 'ui_items_cc_right_click_channel_kick'
		menu.onclicklua = 'ChatChannelKick(\''..nickname..'\', \''..channelID..'\')'
		table.insert(menutable, menu)

		menu = {}
		menu.content = 'ui_items_cc_right_click_channel_ban'
		menu.onclicklua = 'ChatChannelBan(\''..nickname..'\', \''..channelID..'\')'
		table.insert(menutable, menu)

		menu = {}
		menu.content = 'ui_items_cc_right_click_channel_silence'
		menu.onclicklua = 'ChatChannelSilence(\''..nickname..'\', \''..channelID..'\', \'5\')'
		table.insert(menutable, menu)
	end
	-- Ignore & Unignore
	local isIgnored = AtoB(interface:UICmd('ChatIsIgnored(\''..StripClanTag(nickname)..'\')'))
	if not isMe and isIgnored then
		menu = {}
		menu.content = 'ui_items_cc_right_click_unignore'
		menu.onclicklua = 'Common_V2:ChatUnignore(\''..nickname..'\')'
		table.insert(menutable, menu)
	elseif not isMe then
		menu = {}
		menu.content = 'ui_items_cc_right_click_ignore'
		menu.onclicklua = 'Common_V2:ChatIgnore(\''..nickname..'\')'
		table.insert(menutable, menu)
	end
	-- Ban & Remove Ban
	local isBanned = AtoB(interface:UICmd('ChatIsBanned(\''..StripClanTag(nickname)..'\')'))
	if not isMe and isBanned then
		menu = {}
		menu.content = 'ui_items_cc_right_click_remban'
		menu.onclicklua = 'Common_V2:ChatRemoveBanlist(\''..nickname..'\')'
		table.insert(menutable, menu)
	elseif not isMe then
		menu = {}
		menu.content = 'ui_items_cc_right_click_addban'
		menu.onclicklua = 'Common_V2:ChatAddBanlist(\''..nickname..'\')'
		table.insert(menutable, menu)
	end

	-- Channel Promote & Demote
	local isUserAdmin = interface:UICmd('ChatIsAdmin(\''..StripClanTag(nickname)..'\', \''..channelID..'\')')
	if not isMe and myAdminLevel > (userAdminLevel+1) then
		menu = {}
		menu.content = 'ui_items_cc_right_click_channel_promote'
		menu.onclicklua = 'Common_V2:ChatChannelPromote(\''..nickname..'\', \''..channelID..'\')'
		table.insert(menutable, menu)
	elseif not isMe and myAdminLevel > (userAdminLevel+1) and isUserAdmin then
		menu = {}
		menu.content = 'ui_items_cc_right_click_channel_demote'
		menu.onclicklua = 'Common_V2:ChatChannelDemote(\''..nickname..'\', \''..channelID..'\')'
		table.insert(menutable, menu)
	end

	-- Clan Promote & Demote
	local isClanMember = AtoB(interface:UICmd('IsClanMember(\''..StripClanTag(nickname)..'\')'))
	local isMeLeder = AtoB(interface:UICmd('ChatIsClanLeader(\''..GetAccountName()..'\')'))
	local isMeOfficer = AtoB(interface:UICmd('ChatIsClanOfficer(\''..GetAccountName()..'\')'))
	local isUserLeader = AtoB(interface:UICmd('ChatIsClanLeader(\''..StripClanTag(nickname)..'\')'))
	local isUserOfficer = AtoB(interface:UICmd('ChatIsClanOfficer(\''..StripClanTag(nickname)..'\')'))
	if not isMe and isClanMember and isMeLeder and not isUserLeader and not isUserOfficer then
		menu = {}
		menu.content = 'ui_items_cc_right_click_clan_promote'
		menu.onclicklua = 'Common_V2:ChatPromoteClanMember(\''..nickname..'\')'
		table.insert(menutable, menu)
	elseif not isMe and isClanMember and isMeLeder and not isUserLeader and isUserOfficer then
		menu = {}
		menu.content = 'ui_items_cc_right_click_clan_demote'
		menu.onclicklua = 'Common_V2:ChatDemoteClanMember(\''..nickname..'\')'
		table.insert(menutable, menu)
	end
	-- Clan Remove & Invite
	if not isMe and isClanMember and (isMeLeder or isMeOfficer) and not isUserLeader and not isUserOfficer then
		menu = {}
		menu.content = 'ui_items_cc_right_click_clan_remove'
		menu.onclicklua = 'Common_V2:ChatRemoveClanMember(\''..nickname..'\')'
		table.insert(menutable, menu)
	elseif not isMe and not isClanMember and (isMeLeder or isMeOfficer) then
		menu = {}
		menu.content = 'ui_items_cc_right_click_clan_invite'
		menu.onclicklua = 'Common_V2:ChatInviteUserToClan(\''..nickname..'\')'
		table.insert(menutable, menu)
	end
	-- Invite to group
	local isInQueue = AtoB(interface:UICmd('IsInQueue()'))
	if not isMe and not chatInGame and chatUserOnline and not chatUserInGame and not isInQueue then
		menu = {}
		menu.content = 'ui_items_cc_right_click_invite_to_tmm'
		menu.onclicklua = 'Teammaking_V2:InvitePlayer(\''..nickname..'\')'
		table.insert(menutable, menu)
	end
	-- Set Friend Group
	if not isMe and IsBuddy(nickname) and from == 'friend' then
		menu = {}
		menu.content = 'sp_group_rclick'
		menu.onclicklua = 'Social_V2:SetFriendGroup(\''..nickname..'\')'
		table.insert(menutable, menu)
	end
	-- Monitor Game
	if not isMe and ChatUserInGame(nickname) and (IsBuddy(nickname) or IsClanMember(nickname) or IsStaff()) then
		menu = {}
		menu.content = 'ui_items_cc_right_click_watch_game'
		menu.onclicklua = 'Common_V2:MonitorGame(\''..nickname..'\')'
		table.insert(menutable, menu)
	end
	-- Spectate Player
	-- Mentor Player
	if not isMe and not ChatInGame() and ChatUserInGame(nickname) and ChatHasServerInfo(nickname) then
		menu = {}
		menu.content = 'ui_items_cc_right_click_spectate_player'
		menu.onclicklua = 'Common_V2:SpectatePlayer(\''..nickname..'\', \'false\')'
		table.insert(menutable, menu)

		menu = {}
		menu.content = 'ui_items_cc_right_click_mentor_player'
		menu.onclicklua = 'Common_V2:SpectatePlayer(\''..nickname..'\', \'true\')'
		table.insert(menutable, menu)
	end

	-- Staff Spectate Game
	if not isMe and not ChatInGame() and (Client.IsStaff() or Client.IsCaster()) and ChatUserInGame(nickname) and ChatHasServerInfo(nickname) then
		menu = {}
		menu.content = 'ui_items_cc_right_click_staff_spectate'
		menu.onclicklua = 'Common_V2:JoinGameAsStaff(\''..nickname..'\')'
		table.insert(menutable, menu)
	end

	Common_V2:PopupMenu(menutable)
end

function Communicator_V2:UserOnRightClick(id)
	local startIndex = tonumber(GetWidget('communicator_user_scrollbar'):GetValue())
	local maxValue = tonumber(GetWidget('communicator_user_scrollbar'):UICmd("GetScrollbarMaxValue()"))
	if startIndex < 1 then startIndex = 1 end
	if startIndex > maxValue then startIndex = maxValue end

	local chatEntity = chatEntitiesPool[chatTabs[selectedChatTab]]
	Communicator_V2:GenerateRightClickMenu(chatEntity.users[startIndex+id-1], chatEntity.chanID)
end

function Communicator_V2:UserOnDoubleClick(id)
	local startIndex = tonumber(GetWidget('communicator_user_scrollbar'):GetValue())
	local maxValue = tonumber(GetWidget('communicator_user_scrollbar'):UICmd("GetScrollbarMaxValue()"))
	if startIndex < 1 then startIndex = 1 end
	if startIndex > maxValue then startIndex = maxValue end

	local chatEntity = chatEntitiesPool[chatTabs[selectedChatTab]]
	local userName = chatEntity.users[startIndex+id-1]
	if not IsMe(userName) then
		Communicator_V2:StartChatWithPlayer(userName, true)
	end
end

function Communicator_V2:ChatOnSwitchBigAndSmallText()
	local large = GetWidget('communicator_main_large')
	local largeScale = GetWidget('communicator_main_large_scale')
	local expand = GetWidget('communicator_expand')
	local collapse = GetWidget('communicator_collapse')

	if large:IsVisible() then
		Communicator_V2:ChatRefreshTab()
		large:FadeOut(250)
		largeScale:ScaleWidth('0%', 250)
		largeScale:ScaleHeight('0%', 250)

		expand:SetVisible(true)
		collapse:SetVisible(false)
	else
		Communicator_V2:ChatRefreshTab()
		Communicator_V2:ChatRefreshContent()
		large:FadeIn(250)
		largeScale:ScaleWidth('100%', 250)
		largeScale:ScaleHeight('100%', 250)
		expand:SetVisible(false)
		collapse:SetVisible(true)
	end
end

function Communicator_V2:ChatOnInputGetFocus()
	if not GetWidget('communicator_main_large'):IsVisible() then
		Communicator_V2:ChatOnSwitchBigAndSmallText()
	end
end

function Communicator_V2:ChatOnAltToggle(altDown)
	local widget = GetWidget('communicator_main_input_chattype')
	if not widget:IsVisible() then return end

	if altDown and not _altButtonDown then
		local state = widget:GetButtonState()
		if state == 0 then 
			widget:SetButtonState(1)
		else
			widget:SetButtonState(0)
		end
		_altButtonDown = true
	elseif not altDown and _altButtonDown then
		local state = widget:GetButtonState()
		if state == 0 then 
			widget:SetButtonState(1)
		else
			widget:SetButtonState(0)
		end
		_altButtonDown = false
	end
end