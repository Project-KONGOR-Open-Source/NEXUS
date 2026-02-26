
-----------------
-- Communicator Script
-- Copyright 2015 Frostburn Studios
-----------------

local MAX_CHAT_HISTORY = 1000
local NUM_CHANNEL_PLATES = 9
local NUM_NAME_PLATES = 12

-----------------
local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort
local interface, interfaceName = object, object:GetName()
local HoN_Communicator = {}
RegisterScript2('Communicator', '32')
HoN_Communicator.userInfoPool = {}

HoN_Communicator.channelNames = {}
HoN_Communicator.channelIDs = {}
HoN_Communicator.channelTopics = {}
HoN_Communicator.channelUsers = {}
HoN_Communicator.channelChatHistories = {}
HoN_Communicator.channelUnreadMessages = {}
HoN_Communicator.channelBufferScroll = {}
HoN_Communicator.sortIndexes = {}

HoN_Communicator.lobbyNumUsers = 0
HoN_Communicator.lobbyPlayers = {}
HoN_Communicator.gameChatType = 0

HoN_Communicator.selectedChannel = ""
HoN_Communicator.previousSelectedChannel = "Status"
HoN_Communicator.selectedUser = ""
HoN_Communicator.channelScrollOffset = 0
HoN_Communicator.userScrollOffset = 0
HoN_Communicator.canGameFocus = true
HoN_Communicator.switchedChat = false

-- the status channel is always there!
table.insert(HoN_Communicator.channelNames, "Status")
HoN_Communicator.channelChatHistories["Status"] = {}
HoN_Communicator.channelBufferScroll["Status"] = nil
HoN_Communicator.channelIDs["Status"] = -1

-- chat sounds
local function ChatSound(soundMessageType, isMe)
	if (GetWidget("communicator_main"):IsVisible()) then 	-- only play if the communicator is open
		soundMessageType = tonumber(soundMessageType)
		if ((soundMessageType) and GetCvarBool('ui_game_chat_sounds')) then

			local soundsByType = {
				--[0]  = {''}, -- clear
				[1]  = {'RecievedChannelMessage', 'SentChannelMessage'}, -- irc (standard)
				--[2]  = {''}, -- system
				--[3]  = {''}, -- game
				--[4]  = {''}, -- game important
				[5]  = {'RecievedChannelMessage', ''}, -- team
				[6]  = {'RecievedChannelMessage', ''}, -- all
				[7]  = {'RecievedChannelMessage', 'SentChannelMessage'}, -- roll
				[8]  = {'RecievedChannelMessage', 'SentChannelMessage'}, -- emote
				[9]  = {'RecievedWhisper', 'SentWhisper'},  -- group
				[10] = {'RecievedWhisper', 'SentWhisper'},  -- whisper
				[11] = {'RecievedWhisper', 'SentWhisper'},  -- whisper buddies
				[12] = {'RecievedClanMessage', 'SentClanMessage'}, -- clan
				--[13] = {''}, -- global
				--[14] = {''}, -- IM
				--[15] = {''}, -- Server
				--[16] = {''}, -- Local
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
end

-- recieves chat
function HoN_Communicator:AllChatMessages(widget, msgType, channelName, text, entity, noFormatting, isSelf)
	if (GameChat and UIGamePhase() > 0 and UIGamePhase() <= 4) then
		GameChat:AllChatMessages(msgType, channelName, text, entity, noFormatting, isSelf)
	end

	local chanName = channelName
	isSelf = AtoB(isSelf)

	if (tonumber(msgType) == 0) then
		-- clear. out. EVERYTHING! (Note: This is a bug! We aren't getting a target channel)
		-- if (chanName and chanName ~= "") then
		-- 	if (HoN_Communicator.channelChatHistories[chanName]) then
		-- 		HoN_Communicator.channelChatHistories[chanName] = {}
		-- 		if (chanName == HoN_Communicator.selectedChannel) then
		-- 			GetWidget("communicator_chatbuffer"):UICmd("ClearBufferText()")
		--			HoN_Communicator.channelBufferScroll[chanName] = nil
		-- 		end
		-- 	end
		-- else
		-- 	for i, c in ipairs(HoN_Communicator.channelNames) do
		-- 		HoN_Communicator.channelChatHistories[c] = {}
		--		HoN_Communicator.channelBufferScroll[c] = nil
		-- 	end
		-- 	GetWidget("communicator_chatbuffer"):UICmd("ClearBufferText()")
		-- end
		return 0
	elseif (((tonumber(msgType) > 0 and tonumber(msgType) <= 2) or (tonumber(msgType) > 9)) and channelName == "") then
		-- this goes to all channels (game channel too)
		for i, v in ipairs(HoN_Communicator.channelNames) do
			table.insert(HoN_Communicator.channelChatHistories[v], text)

			-- prune messages if there are too many
			if (#HoN_Communicator.channelChatHistories[v] > MAX_CHAT_HISTORY) then
				table.remove(HoN_Communicator.channelChatHistories[v], 1)
				if (HoN_Communicator.channelBufferScroll[v]) then
					HoN_Communicator.channelBufferScroll[v] = HoN_Communicator.channelBufferScroll[v] - 1
					if (HoN_Communicator.channelBufferScroll[v] < 0) then HoN_Communicator.channelBufferScroll[v] = 0 end
				end
			end

			if (v == HoN_Communicator.selectedChannel) then	-- update the text buffer
				GetWidget("communicator_chatbuffer"):AddBufferText(text)
				ChatSound(msgType, isSelf)	-- isME = false for now
			end
		end
		return 1
	elseif (tonumber(msgType) >= 3 and tonumber(msgType) <= 9 and channelName == "") then
		-- game message
		if (HoN_Communicator.channelNames[1] == "Game") then
			chanName = "Game"
		else
			chanName = "Status"
		end
	-- elseif (tonumber(msgType) ~= 1 and channelName == "") then
	-- 	Echo("^rThe communicator recieved a message it didn't know how to handle!-")
	-- 	Echo("\t^gAllChatMessages:^r "..tostring(msgType).." "..tostring(channelName).." ^*"..tostring(text))
	-- 	return -1
	end

	-- normal targeted chat message
	if (HoN_Communicator.channelChatHistories[chanName]) then
		table.insert(HoN_Communicator.channelChatHistories[chanName], text)

		-- prune messages if there are too many
		if (#HoN_Communicator.channelChatHistories[chanName] > MAX_CHAT_HISTORY) then
			table.remove(HoN_Communicator.channelChatHistories[chanName], 1)
			if (HoN_Communicator.channelBufferScroll[v]) then
				HoN_Communicator.channelBufferScroll[v] = HoN_Communicator.channelBufferScroll[v] - 1
				if (HoN_Communicator.channelBufferScroll[v] < 0) then HoN_Communicator.channelBufferScroll[v] = 0 end
			end
		end

		if (chanName == HoN_Communicator.selectedChannel) then	-- update the text buffer
			GetWidget("communicator_chatbuffer"):AddBufferText(text)
			ChatSound(msgType, isSelf)	-- isME = false for now
		elseif (tonumber(msgType) == 1 or (tonumber(msgType) >= 5 and tonumber(msgType) <= 10)) then
			if (not HoN_Communicator.channelUnreadMessages[chanName]) then
				HoN_Communicator.channelUnreadMessages[chanName] = true
				local plateNum = HoN_Communicator:GetChannelPlateNumber(chanName)
				if (plateNum) then
					GetWidget("communicator_channelplate_"..plateNum.."_icon"):SetTexture("/ui/fe2/elements/comm_newmsg.tga")
				end
			end
		end
	end

	--[[
	local unreadChannelCount = 0
	for k,v in pairs(HoN_Communicator.channelUnreadMessages) do
		if v == true then
			unreadChannelCount = unreadChannelCount + 1
		end
	end
	questsUpdateTabUnreadMessages(unreadChannelCount)
	--]]
end

-- opens new chats
function HoN_Communicator:ChatNewChannel(widget, chanID, chanName)
	if (chanName == "") then
		return
	end

	if (not HoN_Communicator.channelChatHistories[chanName]) then
		table.insert(HoN_Communicator.channelNames, chanName)
		HoN_Communicator.channelChatHistories[chanName] = {}
		HoN_Communicator.channelUsers[chanName] = {}
		HoN_Communicator.channelUnreadMessages[chanName] = false
		HoN_Communicator.channelBufferScroll[chanName] = nil
		HoN_Communicator.channelTopics[chanName] = ""
		HoN_Communicator.channelIDs[chanName] = tonumber(chanID)
		-- if (string.sub(chanName, 1, 6) ~= "Match ") then -- this will prevent the Match chan from being selected after the Game chan opens
		-- 	HoN_Communicator:SelectChannel(#HoN_Communicator.channelNames)
		-- 	-- note: we haven't sorted yet, so it will be at the end
		-- end
	else 	-- (this will probably never happen anymore)
		-- see if the channel is already open
		local found = false
		for i, v in ipairs(HoN_Communicator.channelNames) do
			if (v == chanName) then found = true break end
		end

		-- if not found, then the channel is open but not in the list
		if (not found) then
			table.insert(HoN_Communicator.channelNames, chanName)
		end
	end

	HoN_Communicator:SortChannelList()
	HoN_Communicator:PopulateChannels(HoN_Communicator.channelScrollOffset)
end

-- opens new chats
function HoN_Communicator:ChatNewGame(widget)
	if (HoN_Communicator.channelNames[1] ~= "Game") then
		table.insert(HoN_Communicator.channelNames, 1, "Game")
		HoN_Communicator.canGameFocus = true
		if (UIGamePhase() >= 2) then
			HoN_Communicator.switchedChat = true
			HoN_Communicator.gameChatType = 1
		else
			HoN_Communicator.switchedChat = false
			HoN_Communicator.gameChatType = 0
		end
		HoN_Communicator.channelIDs["Game"] = -2
		HoN_Communicator.channelChatHistories["Game"] = {}
		HoN_Communicator.channelUsers["Game"] = {}
		HoN_Communicator.channelUnreadMessages["Game"] = false
		HoN_Communicator.channelBufferScroll["Game"] = nil
		HoN_Communicator.channelTopics["Game"] = ""
		-- HoN_Communicator:SelectChannel(1, true)
		HoN_Communicator:PopulateChannels(HoN_Communicator.channelScrollOffset)
	end
end

function HoN_Communicator:SelectChannel(index, allowReselect)
	-- if not reselecting the same channel
	if (HoN_Communicator.selectedChannel ~= HoN_Communicator.channelNames[index] or allowReselect) then
		if (index > #HoN_Communicator.channelNames) then index = #HoN_Communicator.channelNames end

		-- save the scroll amount
		local bufferScroller = GetWidget("communicator_chatbuffer_scroll")
		local maxScroll = tonumber(bufferScroller:UICmd("GetScrollbarMaxValue()"))
		if ((maxScroll > 0) and (tonumber(bufferScroller:GetValue()) < maxScroll)) then -- only save if there is scroll and they aren't at the bottom
			HoN_Communicator.channelBufferScroll[HoN_Communicator.selectedChannel] = bufferScroller:GetValue()
		else
			HoN_Communicator.channelBufferScroll[HoN_Communicator.selectedChannel] = nil
		end

		local chanName = HoN_Communicator.channelNames[index]
		HoN_Communicator.previousSelectedChannel = HoN_Communicator.selectedChannel
		HoN_Communicator.selectedChannel = chanName

		-- fade in/out the new/old channels
		local tempPlateNum = HoN_Communicator:GetChannelPlateNumber(HoN_Communicator.previousSelectedChannel)
		if (tempPlateNum) then
			GetWidget("communicator_channelplate_"..tempPlateNum.."_selected"):FadeOut("250")
			if (HoN_Communicator.previousSelectedChannel ~= "Game") then
				GetWidget("communicator_channelplate_"..tempPlateNum.."_icon"):SetTexture("/ui/elements/chat_icon.tga")
			end
		end
		tempPlateNum = HoN_Communicator:GetChannelPlateNumber(chanName) 	-- (the index may not be on screen)
		if (tempPlateNum) then 	-- set the icon to current and the selected
			GetWidget("communicator_channelplate_"..tempPlateNum.."_selected"):FadeIn("250")
			GetWidget("communicator_channelplate_"..tempPlateNum.."_icon"):SetTexture("/ui/fe2/elements/comm_current.tga")
		end

		if (HoN_Communicator.channelChatHistories[chanName]) then
			HoN_Communicator:SortUserList(chanName)

			if (chanName == "Status" or chanName == "Game") then 	-- hide the auto connect button on the status/game channel
				GetWidget("communicator_autoconnect"):SetVisible(0)
				GetWidget("communicator_autoconnect_tip"):SetVisible(0)
				GetWidget("channel_header_closebutton"):SetVisible(0)
			else
				GetWidget("communicator_autoconnect"):SetVisible(1)
				GetWidget("communicator_autoconnect_tip"):SetVisible(1)
				GetWidget("channel_header_closebutton"):SetVisible(1)
			end

			-- set the input bar width and show/hide the team/all button (and phase label)
			if (chanName == "Game") then
				if (HoN_Communicator.gameChatType == 0) then
					GetWidget("game_lobby_chattype_all"):SetVisible(1)
				else
					GetWidget("game_lobby_chattype_team"):SetVisible(1)
				end
				GetWidget("communicator_textbox_panel"):SetWidth("48.25h")
				GetWidget("communicator_game_header_phase"):SetVisible(1)
			else
				GetWidget("game_lobby_chattype_team"):SetVisible(0)
				GetWidget("game_lobby_chattype_all"):SetVisible(0)
				GetWidget("communicator_textbox_panel"):SetWidth("57.0h")
				GetWidget("communicator_game_header_phase"):SetVisible(0)
			end

			-- mark as all read
			HoN_Communicator.channelUnreadMessages[chanName] = false

			-- dump all the texts into textbuffer
			local buffer = GetWidget("communicator_chatbuffer")
			bufferScroller:SetValue(0)
			buffer:ClearBufferText()

			for _, text in ipairs(HoN_Communicator.channelChatHistories[chanName]) do
				buffer:AddBufferText(text)
			end

			if (HoN_Communicator.channelBufferScroll[chanName]) then
				bufferScroller:SetValue(HoN_Communicator.channelBufferScroll[chanName])
			end

			GetWidget("communicator_chat_name"):SetText(chanName)
			if (HoN_Communicator.channelTopics[chanName] and HoN_Communicator.channelTopics[chanName] ~= "") then
				local topic = HoN_Communicator.channelTopics[chanName]
				local topicText = topic
				if (string.len(topic) >= 96) then	-- 96 is from the original communicator, setup the hover
					topicText = string.sub(topic, 1, 93).."..."
				else
					GetWidget("communicator_chat_topic_hoverer_panel"):SetVisible(0)
				end

				GetWidget("communicator_chat_topic"):SetText(topicText)
				GetWidget("communicator_chat_topic_hoverer"):SetText(topic)
			else 	-- setting it to a space fixes issues with the label not being the proper width when we acutally
				GetWidget("communicator_chat_topic"):SetText(" ")		-- set it later
				GetWidget("communicator_chat_topic_hoverer"):SetText(" ")
			end

			-- set the auto-connect box
			local autoCon = GetWidget("communicator_autoconnect")
			if (interface:UICmd("ChatIsSavedChannel('"..chanName.."')") == "true") then
				autoCon:UICmd("SetButtonState(1);")
			else
				autoCon:UICmd("SetButtonState(0);")
			end

			-- clear out the selected used (for the sake of looking nicer, we aren't gonna restore it, it's not saved)
			HoN_Communicator.selectedUser = ""
			HoN_Communicator.userScrollOffset = 0 		-- just dump the scroll offset
			HoN_Communicator:PopulateUsers(0)

			-- done like the old communicator, this makes SetNextFocusedChannel work correctly when leaving a practice game in some cases
			interface:UICmd("SetFocusedChannel("..HoN_Communicator.channelIDs[chanName]..");")
		end
	end
end

function HoN_Communicator:GetChannelPlateNumber(chanName)
	local numChans = #HoN_Communicator.channelNames
	for i=1, NUM_CHANNEL_PLATES do
		if (i > numChans) then	-- there won't be more active panels (can't use IsVisible if ingame)
			return nil
		elseif (GetWidget("communicator_channelplate_"..i.."_roomname"):GetValue() == chanName) then
			return i
		end
	end
	-- nothing found
	return nil
end

function HoN_Communicator:UpdateSingleChannel(chanName)
	-- search the channel plates for the chan, if it's there, update it
	for i=1, NUM_CHANNEL_PLATES do
		local panel = GetWidget("communicator_channelplate_"..i)
		local panelName = GetWidget("communicator_channelplate_"..i.."_roomname")
		if (not panel:IsVisible()) then 	-- no more active channels in the list
			break
		elseif (panelName:GetValue() == chanName) then
			local panelNumplate = GetWidget("communicator_channelplate_"..i.."_usercount")
			local panelNumUsers = GetWidget("communicator_channelplate_"..i.."_numusers")

			-- setup the back and icon graphics
			if (chanName == HoN_Communicator.selectedChannel) then
				GetWidget("communicator_channelplate_"..i.."_selected"):SetVisible(1)
				GetWidget("communicator_channelplate_"..i.."_icon"):SetTexture("/ui/fe2/elements/comm_current.tga")
			else
				GetWidget("communicator_channelplate_"..i.."_selected"):SetVisible(0)
				if (chanName == "Game") then
					GetWidget("communicator_channelplate_"..i.."_icon"):SetTexture("/ui/fe2/elements/comm_current.tga")
				elseif (HoN_Communicator.channelUnreadMessages[chanName]) then
					GetWidget("communicator_channelplate_"..i.."_icon"):SetTexture("/ui/fe2/elements/comm_newmsg.tga")
				else
					GetWidget("communicator_channelplate_"..i.."_icon"):SetTexture("/ui/elements/chat_icon.tga")
				end
			end

			if (chanName ~= "Status") then
				if (chanName ~= "Game") then
					panelNumUsers:SetText(tostring(#HoN_Communicator.channelUsers[chanName]))
				else
					panelNumUsers:SetText(tostring(HoN_Communicator.lobbyNumUsers))
				end
				panelNumplate:SetVisible(1)
			else
				panelNumplate:SetVisible(0)
			end
			break
		end
	end
end

function HoN_Communicator:ToggleChatType(ignoreVisibility)
	if (HoN_Communicator.gameChatType == 0 and (GetWidget("game_lobby_chattype_all"):IsVisible() or ignoreVisibility)) then   -- all chat
		HoN_Communicator.gameChatType = 1
		GetWidget("game_lobby_chattype_all"):SetVisible(0)
		GetWidget("game_lobby_chattype_team"):SetVisible(1)
	elseif (HoN_Communicator.gameChatType == 1 and (GetWidget("game_lobby_chattype_team"):IsVisible() or ignoreVisibility)) then
		HoN_Communicator.gameChatType = 0
		GetWidget("game_lobby_chattype_all"):SetVisible(1)
		GetWidget("game_lobby_chattype_team"):SetVisible(0)
	end
end

function HoN_Communicator:HoverTopic(widget, inOut)
	if (HoN_Communicator.channelTopics[HoN_Communicator.selectedChannel]) then
		if (tonumber(inOut) == 1) then	-- mouse over
			if (string.len(HoN_Communicator.channelTopics[HoN_Communicator.selectedChannel]) >= 96) then
				GetWidget("communicator_chat_topic_hoverer_panel"):SetVisible(1)
			else
				GetWidget("communicator_chat_topic_hoverer_panel"):SetVisible(0)
			end
		else 							-- mouse out
			GetWidget("communicator_chat_topic_hoverer_panel"):SetVisible(0)
		end
	end
end

function HoN_Communicator:ClickAutoConnect(widget, type)
	if (tonumber(type) == 0) then -- remove single (left click)
		if (interface:UICmd("ChatIsSavedChannel('"..HoN_Communicator.selectedChannel.."')") == "true") then
			interface:UICmd("ChatRemoveChannel('"..HoN_Communicator.selectedChannel.."');")
		else
			interface:UICmd("ChatSaveChannel('"..HoN_Communicator.selectedChannel.."');")
		end
	else 						-- remove all (right click)
		interface:UICmd("ChatRemoveChannels();")
	end
end

function HoN_Communicator:ChannelClick(widget, index)
	--HoN_Communicator:SelectChannel(tonumber(index)+HoN_Communicator.channelScrollOffset, false)
	if (HoN_Communicator.channelNames[tonumber(index)+HoN_Communicator.channelScrollOffset] == "Game") then
		HoN_Communicator.canGameFocus = true
	end

	interface:UICmd("SetFocusedChannel('"..HoN_Communicator.channelIDs[HoN_Communicator.channelNames[tonumber(index)+HoN_Communicator.channelScrollOffset]].."');")
end

function HoN_Communicator:PopulateChannels(scrollOffset)
	-- setup the range for the sliders
	local cScrollbar = GetWidget("communicator_channel_scrollbar")
	local scrollOffset = scrollOffset or 0
	if (#HoN_Communicator.channelNames > NUM_CHANNEL_PLATES) then
		local numChans = #HoN_Communicator.channelNames
		cScrollbar:SetMaxValue(numChans - NUM_CHANNEL_PLATES)

		if (tonumber(cScrollbar:GetValue()) > (numChans - NUM_CHANNEL_PLATES)) then
			cScrollbar:SetValue(numChans - NUM_CHANNEL_PLATES)
			scrollOffset = numChans - NUM_CHANNEL_PLATES
		end
	else
		cScrollbar:SetMaxValue(0)
		cScrollbar:SetValue(0)
		scrollOffset = 0
	end

	for i = 1, NUM_CHANNEL_PLATES do		-- 8 channel plates
		local panel = GetWidget("communicator_channelplate_"..i)
		local panelName = GetWidget("communicator_channelplate_"..i.."_roomname")
		local panelNumplate = GetWidget("communicator_channelplate_"..i.."_usercount")
		local panelNumUsers = GetWidget("communicator_channelplate_"..i.."_numusers")

		if (HoN_Communicator.channelNames[scrollOffset+i]) then
			panelName:SetText(HoN_Communicator.channelNames[scrollOffset+i])

			-- setup the back and icon graphics
			if (HoN_Communicator.channelNames[scrollOffset+i] == HoN_Communicator.selectedChannel) then
				GetWidget("communicator_channelplate_"..i.."_selected"):SetVisible(1)
				GetWidget("communicator_channelplate_"..i.."_icon"):SetTexture("/ui/fe2/elements/comm_current.tga")
			else
				GetWidget("communicator_channelplate_"..i.."_selected"):SetVisible(0)
				if (HoN_Communicator.channelNames[scrollOffset+i] == "Game") then
					GetWidget("communicator_channelplate_"..i.."_icon"):SetTexture("/ui/fe2/elements/comm_current.tga")
				elseif (HoN_Communicator.channelUnreadMessages[HoN_Communicator.channelNames[scrollOffset+i]]) then
					GetWidget("communicator_channelplate_"..i.."_icon"):SetTexture("/ui/fe2/elements/comm_newmsg.tga")
				else
					GetWidget("communicator_channelplate_"..i.."_icon"):SetTexture("/ui/elements/chat_icon.tga")
				end
			end

			if (HoN_Communicator.channelNames[scrollOffset+i] ~= "Status") then
				if (HoN_Communicator.channelNames[scrollOffset+i] ~= "Game") then
					panelNumUsers:SetText(tostring(#HoN_Communicator.channelUsers[HoN_Communicator.channelNames[scrollOffset+i]]))
				else
					panelNumUsers:SetText(tostring(HoN_Communicator.lobbyNumUsers))
				end
				panelNumplate:SetVisible(1)
			else
				panelNumplate:SetVisible(0)
			end

			panel:SetVisible(1)
		else
			panel:SetVisible(0)
		end
	end
end

function HoN_Communicator:PopulateUsers(scrollOffset)
	-- set the scroller up
	local uScrollbar = GetWidget("communicator_user_scrollbar")

	local resetValue = false
	if (HoN_Communicator.selectedChannel ~= "Status") then
		local numUsers = #HoN_Communicator.channelUsers[HoN_Communicator.selectedChannel]
		if (HoN_Communicator.selectedChannel == "Game") then numUsers = HoN_Communicator.lobbyNumUsers end

		local scrollOffset = scrollOffset or 0
		if (numUsers > NUM_NAME_PLATES) then
			uScrollbar:SetMaxValue(numUsers - NUM_NAME_PLATES)
			local scrollValue = tonumber(uScrollbar:GetValue())
			if (scrollValue > (numUsers - NUM_NAME_PLATES)) then
				uScrollbar:SetValue(numUsers - NUM_NAME_PLATES)
				resetValue = (scrollOffset ~= numUsers - NUM_NAME_PLATES)
				scrollOffset = numUsers - NUM_NAME_PLATES
			elseif scrollValue < 0 then
				uScrollbar:SetMaxValue(0)
				uScrollbar:SetValue(0)
				resetValue = true
			end
		else
			uScrollbar:SetMaxValue(0)
			uScrollbar:SetValue(0)
			resetValue = (scrollOffset ~= 0)
			scrollOffset = 0
		end
	end

	if resetValue then return end

	if (HoN_Communicator.selectedChannel == "Game") then -- game lobby
		GetWidget("communicator_normal_plateset"):SetVisible(0)
		GetWidget("communicator_lobby_plateset"):SetVisible(1)

		for i = 1, NUM_NAME_PLATES do
			local playerPlate = GetWidget("communicator_lobby_nameplate_"..i)
			local playerNameLabel = GetWidget("communicator_lobby_nameplate_"..i.."_label")
			local playerAvatar = GetWidget("communicator_lobby_nameplate_"..i.."_avatar")
			local playerSymbol = GetWidget("communicator_lobby_nameplate_"..i.."_symbol")

			local userInfo = HoN_Communicator.lobbyPlayers[i+scrollOffset]

			if userInfo and NotEmpty(userInfo.name) then
				playerPlate:SetVisible(1)

				-- user highlight
				GetWidget("communicator_lobby_nameplate_"..i.."_selected"):SetVisible(userInfo.name == HoN_Communicator.selectedUser)

				-- loading
				local loadingPanel = GetWidget("communicator_lobby_nameplate_"..i.."_loading")
				if (userInfo.isLoading) then
					loadingPanel:SetWidth(tostring(userInfo.loadingProgress * 100.0).."%")
					loadingPanel:SetVisible(1)
				else
					loadingPanel:SetVisible(0)
				end

				-- text stuff
				if tostring(userInfo.chatNameColorFont) == '8bit' then
					if (userInfo.isLoading) then
						playerNameLabel:SetText(string.upper(userInfo.name)..string.format(" [%i", userInfo.loadingProgress * 100.0).."%]")
						playerNameLabel:SetY('.1h')
					else
						playerNameLabel:SetText(string.upper(userInfo.name))
						playerNameLabel:SetY('0h')
					end
				else
					if (userInfo.isLoading) then
						playerNameLabel:SetText(userInfo.name..string.format(" [%i", userInfo.loadingProgress * 100.0).."%]")
						playerNameLabel:SetY('0h')
					else
						playerNameLabel:SetText(userInfo.name)
						playerNameLabel:SetY('0h')
					end
				end

				if (userInfo.chatNameColorString and userInfo.chatNameColorString ~= "") then
					if (not userInfo.ingame) then
						playerNameLabel:SetColor(userInfo.chatNameColorString)
					else
						playerNameLabel:SetColor(userInfo.chatNameColorIngame)
					end
				else
					if (not userInfo.ingame) then
						if (userInfo.isStaff) then playerNameLabel:SetColor("#FF0000")
						elseif (userInfo.isPremium) then playerNameLabel:SetColor("#DBBF4A")
						else playerNameLabel:SetColor("#FFFFFF") end
					else 						-- don't ask me why '4' :I
						if (userInfo.isStaff) then playerNameLabel:SetColor("#770000")
						elseif (userInfo.isPremium) then playerNameLabel:SetColor("#6E6025")
						else playerNameLabel:SetColor("#777777") end
					end
				end

				playerNameLabel:SetGlow(userInfo.chatNameGlow)

				if userInfo.ingame then
					if NotEmpty(userInfo.chatNameGlowColorIngame) then
						playerNameLabel:SetGlowColor(userInfo.chatNameGlowColorIngame)
					end
				else
					if NotEmpty(userInfo.chatNameGlowColor) then
						playerNameLabel:SetGlowColor(userInfo.chatNameGlowColor)
					end
				end

				-- Echo('userInfo.chatNameColorFont 1: '..tostring(userInfo.chatNameColorFont))
				playerNameLabel:SetFont(NotEmpty(userInfo.chatNameColorFont) and userInfo.chatNameColorFont..'_10' or 'dyn_10')

				-- the color orbythingy texture
				local colorOrb = GetWidget("communicator_lobby_nameplate_"..i.."_orbythingy")
				if (userInfo.isLoading) then
					colorOrb:SetTexture("/ui/elements/lobby/icon_loading.tga")
				elseif (userInfo.isHost) then
					colorOrb:SetTexture("/ui/elements/lobby/icon_host.tga")
				elseif (userInfo.isReferee) then
					colorOrb:SetTexture("/ui/elements/lobby/icon_referee.tga")
				elseif (userInfo.team == 0) then
					colorOrb:SetTexture("/ui/elements/lobby/icon_spectator.tga")
				else
					colorOrb:SetTexture("/ui/elements/lobby/slot_color.tga")
				end
				-- color orbythingy color
				if (userInfo.isLoading or userInfo.team <= 0) then --loading or no team
					colorOrb:SetColor("#FFFFFF")
				else
					colorOrb:SetColor(userInfo.color)
				end

				-- kick button
				GetWidget("communicator_lobby_nameplate_"..i.."_kick"):SetVisible(userInfo.canBeKicked)

				-- set icons and stuff here too
				if (userInfo.accountIconTexture and userInfo.accountIconTexture ~= "") then
					playerAvatar:SetVisible(1)
					playerAvatar:SetTexture(userInfo.accountIconTexture)
				elseif (userInfo.accountID > 0) then
					playerAvatar:SetVisible(1)
					playerAvatar:UICmd("SetAvatar('http://www.heroesofnewerth.com/getAvatar.php?id="..userInfo.accountID.."')")
				else
					playerAvatar:SetVisible(0)
				end

				-- set player symbol
				if (userInfo.chatSymbolTexture and userInfo.chatSymbolTexture ~= "") then
					playerSymbol:SetTexture(userInfo.chatSymbolTexture)
				elseif (userInfo.chatNameColorTexture and userInfo.chatNameColorTexture ~= "") then
					playerSymbol:SetTexture(userInfo.chatNameColorTexture)
				else
					if (userInfo.isStaff) then
						playerSymbol:SetTexture(GetCvarString('ui_staffIconPath'))
					-- elseif (userInfo.isgm) then
					-- 	playerSymbol:SetTexture("/ui/icons/gm.tga")
					-- elseif (userInfo.isadmin) then
					-- 	playerSymbol:SetTexture("/ui/icons/admin.tga")
					-- elseif (userInfo.isop) then
					-- 	playerSymbol:SetTexture("/ui/icons/op.tga")
					elseif (userInfo.isPremium) then
						playerSymbol:SetTexture("/ui/icons/premium.tga")
					else
						playerSymbol:SetTexture("/ui/icons/ingame_2.tga")
					end
				end

				if (userInfo.ingame) then
					playerSymbol:SetRenderMode("grayscale")
					playerSymbol:SetColor("1 1 1 .3")
				else
					playerSymbol:SetRenderMode("normal")
					playerSymbol:SetColor("1 1 1 1")
				end

			else
				playerPlate:SetVisible(0)
			end
		end
	elseif (HoN_Communicator.selectedChannel ~= "Status") then
		GetWidget("communicator_normal_plateset"):SetVisible(1)
		GetWidget("communicator_lobby_plateset"):SetVisible(0)

		for i = 1, NUM_NAME_PLATES do
			local playerPlate = GetWidget("communicator_nameplate_"..i)
			local playerNameLabel = GetWidget("communicator_nameplate_"..i.."_label")
			local playerAvatar = GetWidget("communicator_nameplate_"..i.."_avatar")
			local playerSymbol = GetWidget("communicator_nameplate_"..i.."_symbol")
			local playerAscensionLabel = GetWidget("communicator_nameplate_"..i.."_ascension_label")

			local userInfo = HoN_Communicator.userInfoPool[HoN_Communicator.channelUsers[HoN_Communicator.selectedChannel][i+scrollOffset]]
			if (userInfo and userInfo.name and userInfo.name ~= "") then
				playerPlate:SetVisible(1)

				-- user highlight
				GetWidget("communicator_nameplate_"..i.."_selected"):SetVisible(userInfo.name == HoN_Communicator.selectedUser)

				-- text stuff
				if tostring(userInfo.chatNameColorFont) == '8bit' then
					playerNameLabel:SetText(string.upper(userInfo.name))
					playerNameLabel:SetY('.1h')
				else
					playerNameLabel:SetText(userInfo.name)
					playerNameLabel:SetY('0h')
				end
				if (userInfo.chatNameColorString and userInfo.chatNameColorString ~= "") then
					if (not userInfo.ingame) then
						playerNameLabel:SetColor(userInfo.chatNameColorString)
					else
						playerNameLabel:SetColor(userInfo.chatNameColorIngame)
					end
				else
					if (not userInfo.ingame) then
						if (userInfo.isStaff) then playerNameLabel:SetColor("#FF0000")
						elseif (userInfo.isPremium) then playerNameLabel:SetColor("#DBBF4A")
						else playerNameLabel:SetColor("#FFFFFF") end
					else 						-- don't ask me why '4' :I
						if (userInfo.isStaff) then playerNameLabel:SetColor("#770000")
						elseif (userInfo.isPremium) then playerNameLabel:SetColor("#6E6025")
						else playerNameLabel:SetColor("#777777") end
					end
				end

				playerNameLabel:SetGlow(userInfo.chatNameGlow)

				if userInfo.ingame then
					if NotEmpty(userInfo.chatNameGlowColorIngame) then
						playerNameLabel:SetGlowColor(userInfo.chatNameGlowColorIngame)
					end
				else
					if NotEmpty(userInfo.chatNameGlowColor) then
						playerNameLabel:SetGlowColor(userInfo.chatNameGlowColor)
					end
				end

				-- Echo('userInfo.chatNameColorFont 2: '..tostring(userInfo.chatNameColorFont))
				playerNameLabel:SetFont(NotEmpty(userInfo.chatNameColorFont) and userInfo.chatNameColorFont..'_10' or 'dyn_10')

				--Ascension level
				local level = math.abs(tonumber(userInfo.ascensionLevel))
				if level > 9999 then
					level = 9999;
				end
				GetWidget("communicator_nameplate_"..i.."_ascension"):SetVisible(level > 0)

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
						GetWidget("communicator_nameplate_"..i.."_ascension_img_"..j):SetVisible(j == ascensionImg)
					end

					playerAscensionLabel:SetText(tostring(level))
				end

				-- set icons and stuff here too
				if (userInfo.accountIconTexture and userInfo.accountIconTexture ~= "") then
					playerAvatar:SetVisible(1)
					playerAvatar:SetTexture(userInfo.accountIconTexture)
				elseif (userInfo.accountID > 0) then
					playerAvatar:SetVisible(1)
					playerAvatar:UICmd("SetAvatar('http://www.heroesofnewerth.com/getAvatar.php?id="..userInfo.accountID.."')")
				else
					playerAvatar:SetVisible(0)
				end

				-- set player symbol
				if (userInfo.chatSymbolTexture and userInfo.chatSymbolTexture ~= "") then
					playerSymbol:SetTexture(userInfo.chatSymbolTexture)
				elseif (userInfo.chatNameColorTexture and userInfo.chatNameColorTexture ~= "") then
					playerSymbol:SetTexture(userInfo.chatNameColorTexture)
				else
					if (userInfo.isStaff) then
						playerSymbol:SetTexture(GetCvarString('ui_staffIconPath'))
					-- elseif (userInfo.isgm) then
					-- 	playerSymbol:SetTexture("/ui/icons/gm.tga")
					-- elseif (userInfo.isadmin) then
					-- 	playerSymbol:SetTexture("/ui/icons/admin.tga")
					-- elseif (userInfo.isop) then
					-- 	playerSymbol:SetTexture("/ui/icons/op.tga")
					elseif (userInfo.isPremium) then
						playerSymbol:SetTexture("/ui/icons/premium.tga")
					else
						playerSymbol:SetTexture("/ui/icons/ingame_2.tga")
					end
				end

				if (userInfo.ingame) then
					playerSymbol:SetRenderMode("grayscale")
					playerSymbol:SetColor("1 1 1 .3")
				else
					playerSymbol:SetRenderMode("normal")
					playerSymbol:SetColor("1 1 1 1")
				end

			else
				playerPlate:SetVisible(0)
			end
		end
	else 	-- status channel, just hide everything
		GetWidget("communicator_normal_plateset"):SetVisible(0)
		GetWidget("communicator_lobby_plateset"):SetVisible(0)
		uScrollbar:SetMaxValue(0)
		uScrollbar:SetValue(0)
	end
end

function HoN_Communicator:ChatUserNames(widget, ...)
	-- Echo('HoN_Communicator:ChatUserNames chatNameColorFont:'..arg[17])

	-- add/update the users info in the pool
	if (not HoN_Communicator.userInfoPool[arg[2]]) then	-- initalize if it doesn't exist yet
		HoN_Communicator.userInfoPool[arg[2]] = {}
	end

	HoN_Communicator.userInfoPool[arg[2]].name = arg[2]
	HoN_Communicator.userInfoPool[arg[2]].isStaff = (arg[3] == "4")
	HoN_Communicator.userInfoPool[arg[2]].ingame = (arg[4] == "1")
	HoN_Communicator.userInfoPool[arg[2]].isPremium = (arg[5] == "1")
	-- HoN_Communicator.userInfoPool[arg[2]].isgm = false
	-- HoN_Communicator.userInfoPool[arg[2]].isadmin = false	-- these were on the old communicator code, but were always false
	-- HoN_Communicator.userInfoPool[arg[2]].isop = false
	HoN_Communicator.userInfoPool[arg[2]].accountID = tonumber(arg[6])
	HoN_Communicator.userInfoPool[arg[2]].chatSymbolTexture = arg[7]
	HoN_Communicator.userInfoPool[arg[2]].chatNameColorTexture = arg[8]
	HoN_Communicator.userInfoPool[arg[2]].chatNameColorString = arg[9]
	HoN_Communicator.userInfoPool[arg[2]].chatNameColorIngame = arg[10]
	HoN_Communicator.userInfoPool[arg[2]].accountIconTexture = arg[11]
	--HoN_Communicator.userInfoPool[arg[2]].sortIndex = tonumber(arg[4]..tostring((4 - tonumber(arg[3])))..arg[12])
	HoN_Communicator.userInfoPool[arg[2]].chatNameGlow = (arg[13] == "true")
	HoN_Communicator.userInfoPool[arg[2]].chatNameGlowColor = arg[14]
	HoN_Communicator.userInfoPool[arg[2]].chatNameGlowColorIngame = arg[15]
	HoN_Communicator.userInfoPool[arg[2]].ascensionLevel = arg[16]
	HoN_Communicator.userInfoPool[arg[2]].chatNameColorFont = arg[17]

	-- new sort index
	if (not HoN_Communicator.sortIndexes[arg[1]]) then
		HoN_Communicator.sortIndexes[arg[1]] = {}
	end
	HoN_Communicator.sortIndexes[arg[1]][arg[2]] = tonumber(arg[4]..tostring((4 - tonumber(arg[3])))..arg[12])

	-- add the user (by name) to the channels list
	if (HoN_Communicator.channelUsers[arg[1]]) then	-- the server is trying to give us info for a channel we left
		alreadyExists = false
		for _, v in ipairs(HoN_Communicator.channelUsers[arg[1]]) do
			if (v == arg[2]) then alreadyExists = true break end
		end

		if (not alreadyExists) then table.insert(HoN_Communicator.channelUsers[arg[1]], arg[2]) end

		-- update the user count on the channel
		local tempPlateNum = HoN_Communicator:GetChannelPlateNumber(arg[1])
		if (tempPlateNum) then
			GetWidget("communicator_channelplate_"..tempPlateNum.."_numusers"):SetText(tostring(#HoN_Communicator.channelUsers[arg[1]]))
		end
	end
end

function HoN_Communicator:SortUserList(chanName)
	if (chanName and chanName ~= "" and HoN_Communicator.channelUsers[chanName]) then
		local userSortFunc = function(a, b)
			if (HoN_Communicator.sortIndexes[chanName][a] == HoN_Communicator.sortIndexes[chanName][b]) then
				return string.lower(StripClanTag(a)) < string.lower(StripClanTag(b))
			else
				return HoN_Communicator.sortIndexes[chanName][a] < HoN_Communicator.sortIndexes[chanName][b]
			end
		end

		table.sort(HoN_Communicator.channelUsers[chanName], userSortFunc)
	end
end

function HoN_Communicator:SortChannelList()
	if (HoN_Communicator.channelNames) then
		local chanSortFunc = function(a, b)
			if (HoN_Communicator.channelIDs[a] == HoN_Communicator.channelIDs[b]) then  -- this will probably never happen
				return string.lower(a) < string.lower(b)
			else
				return HoN_Communicator.channelIDs[a] < HoN_Communicator.channelIDs[b]
			end
		end

		table.sort(HoN_Communicator.channelNames, chanSortFunc)
	end
end

-- clear, sort, etc type stuff
function HoN_Communicator:ChatUserEvent(widget, chanName, command)
	if (HoN_Communicator.channelUsers[chanName]) then --stops commands from channels we aren't in
		if (string.find(command, "ClearItems")) then
			HoN_Communicator.channelUsers[chanName] = {}
		elseif (string.find(command, "SortListboxSortIndex") and chanName == HoN_Communicator.selectedChannel) then
			HoN_Communicator:SortUserList(HoN_Communicator.selectedChannel)
			HoN_Communicator:PopulateUsers(HoN_Communicator.userScrollOffset)
		elseif (string.find(command, "EraseListItemByValue")) then
			local playerIndex = HoN_Communicator:RemoveNameFromChannel(chanName, string.sub(command, 23, string.len(command)-3))
			if (playerIndex and chanName == HoN_Communicator.selectedChannel) then
				-- need to shift the scroll
				if (playerIndex <= HoN_Communicator.userScrollOffset) then
					HoN_Communicator.userScrollOffset = HoN_Communicator.userScrollOffset - 1
				end
				HoN_Communicator:PopulateUsers(HoN_Communicator.userScrollOffset)
			end
		end
	end
end

function HoN_Communicator:RemoveNameFromChannel(chanName, userName)
	for i, v in ipairs(HoN_Communicator.channelUsers[chanName]) do
		if (v == userName) then
			table.remove(HoN_Communicator.channelUsers[chanName], i)
			-- update the user count on the channel
			local tempPlateNum = HoN_Communicator:GetChannelPlateNumber(chanName)
			if (tempPlateNum) then
				GetWidget("communicator_channelplate_"..tempPlateNum.."_numusers"):SetText(tostring(#HoN_Communicator.channelUsers[chanName]))
			end
			return i
		end
	end

	return nil
end

function HoN_Communicator:ChatChanTopic(widget, chanName, topic)
	local chan = chanName
	if (chan == "irc_status_chan") then
		chan = "Status" end

	HoN_Communicator.channelTopics[chan] = topic
	if (chan == HoN_Communicator.selectedChannel) then
		local topicText = topic
		if (string.len(topic) >= 96) then	-- 96 is from the original communicator, setup the hover
			topicText = string.sub(topic, 1, 93).."..."
		else
			GetWidget("communicator_chat_topic_hoverer_panel"):SetVisible(0)
		end

		GetWidget("communicator_chat_topic"):SetText(topicText)
		GetWidget("communicator_chat_topic_hoverer"):SetText(topic)
	end
end

function HoN_Communicator:ChatLeftChannel(widget, chanID)
	if (tonumber(chanID) == -1) then	-- LEAVE ALL THE CHANNELS (that aren't status)!!!
		HoN_Communicator:ChatLeftGame(nil)

		while (HoN_Communicator.channelNames[2]) do
			local chanName = HoN_Communicator.channelNames[2]
			HoN_Communicator.channelChatHistories[chanName] = nil
			HoN_Communicator.channelUsers[chanName] = nil
			HoN_Communicator.channelTopics[chanName] = nil
			HoN_Communicator.channelIDs[chanName] = nil
			HoN_Communicator.channelUnreadMessages[chanName] = nil
			HoN_Communicator.channelBufferScroll[chanName] = nil
			table.remove(HoN_Communicator.channelNames, 2)
		end

		-- HoN_Communicator:SelectChannel(1, true)
		HoN_Communicator.channelScrollOffset = 0
		HoN_Communicator:PopulateChannels(0)
	else
		-- look for a matching ID in the table
		local chanName = nil
		for chan, id in pairs(HoN_Communicator.channelIDs) do
			if (tonumber(id) == tonumber(chanID)) then chanName = chan break end
		end

		if (chanName) then
			local removeIndex = nil --, selectIndex, prevIndex = nil, nil, nil

			for i, v in ipairs(HoN_Communicator.channelNames) do
				if (chanName == v) then removeIndex = i break end
				--if (not removeIndex) then prevIndex = i end
				--if (HoN_Communicator.previousSelectedChannel == v) then if (not removeIndex) then selectIndex = i else selectIndex = i-1 end end

				--if (removeIndex and selectIndex) then break end
				--if (removeIndex) then break end
			end

			if (removeIndex) then
				table.remove(HoN_Communicator.channelNames, removeIndex)
				HoN_Communicator.channelChatHistories[chanName] = nil
				HoN_Communicator.channelUsers[chanName] = nil
				HoN_Communicator.channelTopics[chanName] = nil
				HoN_Communicator.channelIDs[chanName] = nil
				HoN_Communicator.channelUnreadMessages[chanName] = nil
				HoN_Communicator.channelBufferScroll[chanName] = nil

				-- if (selectIndex) then
				-- 	HoN_Communicator:SelectChannel(selectIndex, false)
				-- else
				-- 	HoN_Communicator:SelectChannel(prevIndex, false)
				-- end
				if (chanName == HoN_Communicator.selectedChannel) then
					interface:UICmd("SetNextFocusedChannel();")
				end
				HoN_Communicator:PopulateChannels(HoN_Communicator.channelScrollOffset)
			end
		end
	end
end

function HoN_Communicator:SendChat(self)
	local msg = GetWidget("communicator_send_box"):GetValue()
	if (string.sub(msg, 1, 6) == "/clear") then 	-- fix to make /clear work
		if (HoN_Communicator.selectedChannel and HoN_Communicator.selectedChannel ~= "" and HoN_Communicator.channelChatHistories[HoN_Communicator.selectedChannel]) then
			HoN_Communicator.channelChatHistories[HoN_Communicator.selectedChannel] = {}
			GetWidget("communicator_chatbuffer"):ClearBufferText()
			HoN_Communicator.channelBufferScroll[HoN_Communicator.selectedChannel] = nil
		end
	else
		if (HoN_Communicator.selectedChannel ~= "Game") then
			interface:UICmd("ChatSendMessage('"..string.gsub(string.gsub(msg, "\\", "\\\\"), "'", "\\'").."', '"..HoN_Communicator.selectedChannel.."');")
		else
			if (HoN_Communicator.gameChatType == 0) then
				interface:UICmd("AllChat('"..string.gsub(string.gsub(msg, "\\", "\\\\"), "'", "\\'").."')")
			else
				interface:UICmd("TeamChat('"..string.gsub(string.gsub(msg, "\\", "\\\\"), "'", "\\'").."')")
			end
		end
	end
end

function HoN_Communicator:ChannelListSlide(widget, offset)
	HoN_Communicator.channelScrollOffset = tonumber(offset)
	HoN_Communicator:PopulateChannels(tonumber(offset))
end

function HoN_Communicator:UserListSlide(widget, offset)
	HoN_Communicator.userScrollOffset = tonumber(offset)
	HoN_Communicator:PopulateUsers(tonumber(offset))
end

function HoN_Communicator:RightClickUser(widget, panelType, index)
	HoN_Communicator:SelectUser(widget, panelType, index)
	Set("ui_currentChannel", HoN_Communicator.selectedChannel)

	if (panelType == "lobby") then
		Set("ui_lastSelectedUser", StripClanTag(HoN_Communicator.channelUsers[HoN_Communicator.selectedChannel][index+HoN_Communicator.userScrollOffset]))
		GetWidget(panelType.."_userlist_rtclick"):DoEvent()
	elseif (not HoN_Communicator.lobbyPlayers[index+HoN_Communicator.userScrollOffset].isBot) then
		Set("ui_lastSelectedUser", StripClanTag(HoN_Communicator.lobbyPlayers[index+HoN_Communicator.userScrollOffset].name))
		Set("ui_lastSelectedClientNumber", HoN_Communicator.lobbyPlayers[index+HoN_Communicator.userScrollOffset].clientNumber)
		GetWidget(panelType.."_userlist_rtclick"):DoEvent()
	end
end

function HoN_Communicator:SelectUser(widget, panelType, index)
	local addName = ""
	if (panelType == "game") then addName = "lobby_" end

	-- go through and fade out the selected plates as needed if the current selected user is in the list
	for i=1, NUM_NAME_PLATES do
		local selectWidget = GetWidget("communicator_"..addName.."nameplate_"..i.."_selected")
		if (selectWidget:IsVisible()) then selectWidget:FadeOut("250") end
	end

	-- select the new user
	if (panelType == "lobby") then
		HoN_Communicator.selectedUser = HoN_Communicator.channelUsers[HoN_Communicator.selectedChannel][index+HoN_Communicator.userScrollOffset]
	else
		HoN_Communicator.selectedUser = HoN_Communicator.lobbyPlayers[index+HoN_Communicator.userScrollOffset].name
	end
	GetWidget("communicator_"..addName.."nameplate_"..index.."_selected"):FadeIn("250")
end

function HoN_Communicator:DoubleClickUser(widget, panelType, index)
	-- get the name
	local whisperName = nil
	if (panelType == "lobby") then
		whisperName = HoN_Communicator.channelUsers[HoN_Communicator.selectedChannel][index+HoN_Communicator.userScrollOffset]
	else
		whisperName = HoN_Communicator.lobbyPlayers[index+HoN_Communicator.userScrollOffset].name
	end

	-- clear the input, set the text to the whisper, and focus it
	if (whisperName) then
		local sendBox = GetWidget("communicator_send_box")
		sendBox:UICmd("EraseInputLine(); SetInputLine('/w "..whisperName.." '); SetFocus(true);")
	end
end

function HoN_Communicator:RightClickChannel(widget, index)
	Set("ui_currentChannel", HoN_Communicator.selectedChannel)
	GetWidget("chat_chan_rtclick"):DoEvent()
end

function HoN_Communicator:CreateNewChannel(widget)
	-- the gsub will prevent errors, the JoinChannel function will strip that stuff out itself
	local chanName = string.gsub(string.gsub(widget:GetValue(), "\\", "\\\\"), "'", "\\'")
	if (chanName ~= "Status" and chanName ~= "Game") then
		interface:UICmd("ChatLeaveChannel('"..chanName.."'); ChatJoinChannel('"..chanName.."');")
	end
end

function HoN_Communicator:CloseChat(widget)
	if (HoN_Communicator.selectedChannel ~= "Status" and HoN_Communicator.selectedChannel ~= "Game") then
		-- if it's a group channel, leave the group
		if ((string.sub(HoN_Communicator.selectedChannel, 1, 5) == "Group") and IsInGroup()) then
			interface:UICmd("LeaveTMMGroup();")
		end

		interface:UICmd("ChatLeaveChannel('"..HoN_Communicator.selectedChannel.."');")
	end
end

function HoN_Communicator:ChatLeftGame(widget)
	if (HoN_Communicator.channelNames[1] == "Game") then
		HoN_Communicator.channelIDs["Game"] = nil
		table.remove(HoN_Communicator.channelNames, 1)
		HoN_Communicator.channelChatHistories["Game"] = nil
		HoN_Communicator.channelUsers["Game"] = nil
		HoN_Communicator.channelTopics["Game"] = nil
		HoN_Communicator.channelUnreadMessages["Game"] = nil
		HoN_Communicator.channelBufferScroll["Game"] = nil
		HoN_Communicator.canGameFocus = true
		HoN_Communicator.switchedChat = false

		-- select the match channel (or status if we can't find it)
		-- local chanIndex = 1
		-- for i, v in ipairs(HoN_Communicator.channelNames) do
		-- 	if ((string.len(v) > 6) and (string.sub(v, 1, 6) == "Match ")) then chanIndex = i break end
		-- end

		-- HoN_Communicator:SelectChannel(chanIndex, true)
		if (GetCvarString("cc_curGameChannel") ~= "") then
			interface:UICmd("SetFocusedChannel("..GetCvarInt("cc_curGameChannelID")..");")
		else
			interface:UICmd("SetNextFocusedChannel();")
		end
		HoN_Communicator:PopulateChannels(HoN_Communicator.channelScrollOffset)
	end
end

function HoN_Communicator:LobbyPlayerListIndex(index, widget, ...)
	-- Echo('LobbyPlayerListIndex chatNameColorFont:'..arg[25])
	if (tonumber(arg[1]) == -1) then
		if (HoN_Communicator.lobbyPlayers[index+1]) then 	-- deleted existing lobby player
			HoN_Communicator.lobbyPlayers[index+1] = nil
			HoN_Communicator:PopulateUsers(HoN_Communicator.userScrollOffset)
		end
	else
		if (not HoN_Communicator.lobbyPlayers[index+1]) then
			HoN_Communicator.lobbyPlayers[index+1] = {}
		end
		HoN_Communicator.lobbyPlayers[index+1].clientNumber			= tonumber(arg[1])
		HoN_Communicator.lobbyPlayers[index+1].name 				= arg[2]
		HoN_Communicator.lobbyPlayers[index+1].color				= arg[3]
		HoN_Communicator.lobbyPlayers[index+1].isHost				= AtoB(arg[4])
		HoN_Communicator.lobbyPlayers[index+1].isLoading			= AtoB(arg[5])
		HoN_Communicator.lobbyPlayers[index+1].team					= tonumber(arg[6])
		HoN_Communicator.lobbyPlayers[index+1].isReferee			= AtoB(arg[7])
		HoN_Communicator.lobbyPlayers[index+1].loadingProgress		= tonumber(arg[8])
		HoN_Communicator.lobbyPlayers[index+1].isStaff				= AtoB(arg[9])
		HoN_Communicator.lobbyPlayers[index+1].isPremium			= AtoB(arg[10])
		HoN_Communicator.lobbyPlayers[index+1].canBeKicked			= AtoB(arg[11])
		HoN_Communicator.lobbyPlayers[index+1].accountID			= tonumber(arg[12])
		HoN_Communicator.lobbyPlayers[index+1].chatSymbolTexture	= arg[13]
		HoN_Communicator.lobbyPlayers[index+1].chatNameColorTexture	= arg[14]
		HoN_Communicator.lobbyPlayers[index+1].chatNameColorString	= arg[15]
		HoN_Communicator.lobbyPlayers[index+1].chatNameColorIngame	= arg[16]
		HoN_Communicator.lobbyPlayers[index+1].accountIconTexture	= arg[17]
		HoN_Communicator.lobbyPlayers[index+1].isMe					= AtoB(arg[18])
		HoN_Communicator.lobbyPlayers[index+1].isActiveSlot			= AtoB(arg[19])
		HoN_Communicator.lobbyPlayers[index+1].connectionStatus		= tonumber(arg[20])
		HoN_Communicator.lobbyPlayers[index+1].chatNameGlow			= AtoB(arg[21])
		HoN_Communicator.lobbyPlayers[index+1].isBot				= AtoB(arg[22] or 'false')
		HoN_Communicator.lobbyPlayers[index+1].chatNameGlowColor	= arg[23]
		HoN_Communicator.lobbyPlayers[index+1].chatNameGlowColorIngame	= arg[24]
		HoN_Communicator.lobbyPlayers[index+1].chatNameColorFont	= arg[25]
		HoN_Communicator:PopulateUsers(HoN_Communicator.userScrollOffset)
	end
end

function HoN_Communicator:LobbyGameInfo(widget, ...)
	if (HoN_Communicator.channelNames[1] == "Game") then
		HoN_Communicator.channelTopics["Game"] = arg[8]
		if (HoN_Communicator.selectedChannel == "Game") then
			GetWidget("communicator_chat_topic"):SetText(arg[8])
			GetWidget("communicator_chat_topic_hoverer"):SetText(arg[8])
		end
	end
end

function HoN_Communicator:LobbyPlayerListSize(widget, size)
	GetWidget("communicator_channelplate_1_numusers"):SetText(tostring(size))

	if(tonumber(size) == 0 and tonumber(size) ~= HoN_Communicator.lobbyNumUsers) then 	-- the game was closed most likey, unregister all the watches
		for i=1, HoN_Communicator.lobbyNumUsers do
			HoN_Communicator.lobbyPlayers[i] = nil end
	end

	HoN_Communicator.lobbyNumUsers = tonumber(size)
end

function HoN_Communicator:KickPlayer(widget, slotNumber)
	interface:UICmd("Kick("..tostring(HoN_Communicator.lobbyPlayers[slotNumber+HoN_Communicator.userScrollOffset].clientNumber)..")")
	Trigger("LobbyKicked", HoN_Communicator.lobbyPlayers[slotNumber+HoN_Communicator.userScrollOffset].name)
end

function HoN_Communicator:GamePhaseExtended(widget, phase)
	phase = tonumber(phase)
	local phaseTextTable = {
		[1] = "game_lobby_phase_waiting",
		[2] = "game_lobby_phase_banning",
		[3] = "game_lobby_phase_picking",
		[4] = "game_lobby_phase_loading",
		[5] = "game_lobby_phase_prematch",
		[6] = "game_lobby_phase_match",
		[7] = "game_lobby_phase_postmatch",
		[8] = "game_lobby_phase_ended",
		[9] = "game_lobby_phase_hiddenban",
		[10] = "game_lobby_phase_locking",
		[11] = "game_lobby_phase_lock_pick",
		[12] = "game_lobby_phase_shuffle_pick",
	}

	if (phaseTextTable and phaseTextTable[phase] and phaseTextTable[phase] ~= "") then
		GetWidget("communicator_game_header_phase"):SetText(Translate(phaseTextTable[phase]))
	else
		GetWidget("communicator_game_header_phase"):SetText(" ")
	end
end

function HoN_Communicator:GamePhase(widget, phase)
	if (tonumber(phase) == 0) then
		HoN_Communicator:ChatLeftGame()
	end
	if (tonumber(phase) >= 2 and not HoN_Communicator.switchedChat) then -- switch to team chat for default
		if (HoN_Communicator.gameChatType == 0) then
			HoN_Communicator:ToggleChatType(true)
		end
		HoN_Communicator.switchedChat = true
	end
end

function HoN_Communicator:SelectChannelByID(id)
	local selectIndex = 1

	for i,n in ipairs(HoN_Communicator.channelNames) do
		if (HoN_Communicator.channelIDs[n] == id) then
			selectIndex = i
			break
		end
	end

	HoN_Communicator:SelectChannel(selectIndex, true)
end

function HoN_Communicator:ChatSetFocusChannel(widget, channelID, channelName)
	if (tonumber(channelID) == -2) then
		if (not HoN_Communicator.canGameFocus) then
			return
		else
			HoN_Communicator.canGameFocus = false
		end
	 end

	HoN_Communicator:SelectChannelByID(tonumber(channelID))
end

function CommunicatorInitalize(commWidget)
	-- init here so we don't mess up any interface registered watches
	--		(since some of these are commonly used triggers)
	commWidget:RegisterWatch("AllChatMessages", function(...) HoN_Communicator:AllChatMessages(...) end)
	commWidget:RegisterWatch("ChatNewChannel", function(...) HoN_Communicator:ChatNewChannel(...) end)
	commWidget:RegisterWatch("ChatNewGame", function(...) HoN_Communicator:ChatNewGame(...) end)
	commWidget:RegisterWatch("ChatUserNames", function(...) HoN_Communicator:ChatUserNames(...) end)
	commWidget:RegisterWatch("ChatUserEvent", function(...) HoN_Communicator:ChatUserEvent(...) end)
	commWidget:RegisterWatch("ChatChanTopic", function(...) HoN_Communicator:ChatChanTopic(...) end)
	commWidget:RegisterWatch("ChatLeftChannel", function(...) HoN_Communicator:ChatLeftChannel(...) end)
	commWidget:RegisterWatch("ChatLeftGame", function(...) HoN_Communicator:ChatLeftGame(...) end)
	commWidget:RegisterWatch("LobbyGameInfo", function(...) HoN_Communicator:LobbyGameInfo(...) end)
	commWidget:RegisterWatch("LobbyPlayerListSize", function(...) HoN_Communicator:LobbyPlayerListSize(...) end)
	commWidget:RegisterWatch("GamePhaseExtended", function(...) HoN_Communicator:GamePhaseExtended(...) end)
	commWidget:RegisterWatch("GamePhase", function(...) HoN_Communicator:GamePhase(...) end)
	commWidget:RegisterWatch("ChatSetFocusChannel", function(...) HoN_Communicator:ChatSetFocusChannel(...) end)

	for i=0, 31 do
		commWidget:RegisterWatch("LobbyPlayerList"..i, function(...) HoN_Communicator:LobbyPlayerListIndex(i, ...) end)
	end
end

function interface:HoNCommunicatorF(func, ...)
	print(HoN_Communicator[func](self, ...))
end
