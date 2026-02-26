-- IM scripts
local NUMBER_VISIBLE_SLOTS = 10
local MAX_HISTORY_LINES = 1000
local IM_NAME_HEIGHT = '3.0h'

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.string.lower
local interface, interfaceName = object, object:GetName()
RegisterScript2('Social_IM', '31');
HoN_IM_Panel = _G['HoN_IM_Panel'] or {}
HoN_IM_Panel.selectedPlayerName, HoN_IM_Panel.selectedPlayerIndex = nil, nil
HoN_IM_Panel.lastMessageFrom = nil
HoN_IM_Panel.previouslySelectedIndex = nil
HoN_IM_Panel.playerListOffset = 0
HoN_IM_Panel.numberOfPlayers = 0
HoN_IM_Panel.playerNames = {}
HoN_IM_Panel.chatHistories = {}
HoN_IM_Panel.unreadIMCounts = {}
HoN_IM_Panel.totalUnreadIMs = 0
HoN_IM_Panel.needUserInfo = {}

function HoN_IM_Panel:OnLogin()
	local accountName = GetAccountName()
	if GetCvarBool('ui_im_store_history') and (accountName) and NotEmpty(accountName) then
		HoN_IM_Panel.chatHistories = GetDBEntry('hon_im_history_'..GetAccountName(), nil, false, false, false) or {}
	else
		HoN_IM_Panel.chatHistories = {}
	end
end

function HoN_IM_Panel:OnLogout()
	HoN_IM_Panel.playerNames = {}
	HoN_IM_Panel.chatHistories = {}
	HoN_IM_Panel.unreadIMCounts = {}
	HoN_IM_Panel.needUserInfo = {}
	HoN_IM_Panel.totalUnreadIMs = 0
	HoN_IM_Panel.playerListOffset = 0
	HoN_IM_Panel.numberOfPlayers = 0
	HoN_IM_Panel.selectedPlayerName = nil
	HoN_IM_Panel.selectedPlayerIndex = nil
	HoN_IM_Panel.lastMessageFrom = nil
	HoN_IM_Panel.previouslySelectedIndex = nil
	HoN_IM_Panel.chatWidgetActive = {}
	groupfcall('im_entries', function(_, widget, _) widget:SetVisible(0) end)
	GetWidget('no_im_cover', 'main'):SetVisible(1)
	GetWidget('im_chatbox', 'main'):UICmd("ClearBufferText()")
	GetWidget('whisper_box_panel', 'main'):SetVisible(0)
	HoN_IM_Panel:UpdateSysbarIcon()
end

-- Recieve a chat message
function HoN_IM_Panel:AddChatEntry(name, text, info)
	--Echo("AddChatEntry "..name.." "..text.." "..info)
	local cleanText = string.gsub(string.gsub(text, "\\", "\\\\"), "'", "\\'")
	if(HoN_IM_Panel:CreateChat(name, info) >= 0) then
		local lName = string.lower(name)

		-- get the name casing if we need it
		if (HoN_IM_Panel.needUserInfo[lName] and GetChatClientInfo) then
			local nameString = GetChatClientInfo(lName, "name")
			local nameIndex = HoN_IM_Panel:GetIndexFromNameInsensitive(lName)
			if (nameString and (string.sub(nameString, 0, -2) ~= "") and nameIndex) then  -- replace the current entry in the playerNames table
				table.remove(HoN_IM_Panel.playerNames, nameIndex)
				table.insert(HoN_IM_Panel.playerNames, nameIndex, StripClanTag(string.sub(nameString, 0, -2)))
				HoN_IM_Panel.needUserInfo[lName] = nil
				HoN_IM_Panel:UpdateVisibleSlots(HoN_IM_Panel.playerListOffset) --update the name in the list
			end
		end

		-- already have the chat setup, append the new history entry
		table.insert(HoN_IM_Panel.chatHistories[lName], cleanText)
		-- remove the topmost entry if we have too many
		if (#HoN_IM_Panel.chatHistories[lName] > MAX_HISTORY_LINES) then
			table.remove(HoN_IM_Panel.chatHistories[lName], 1)
		end

		if GetCvarBool('ui_im_store_history') then
			GetDBEntry('hon_im_history_'..GetAccountName(), HoN_IM_Panel.chatHistories, true, false, false)
		end

		if (HoN_IM_Panel:IsChatVisible() and string.lower(HoN_IM_Panel.selectedPlayerName) == lName) then
			-- we are looking at that text already, add it to the current box and toggle it read
			local IMWindow = GetWidget("im_chatbox");
			if (IMWindow) then
				IMWindow:UICmd("AddBufferText('"..cleanText.."')")
				PlayChatSound('SentIM')
			end

			HoN_IM_Panel.lastMessageFrom = name
			interface:UICmd("ChatRemoveUnreadIMs('"..name.."');")
		else	-- we aren't looking at it, if the message is to an active slot set the texture to new message
			if (info > 0) then
				HoN_IM_Panel.lastMessageFrom = name
				HoN_IM_Panel.unreadIMCounts[lName] = HoN_IM_Panel.unreadIMCounts[lName] + 1
				HoN_IM_Panel.totalUnreadIMs = HoN_IM_Panel.totalUnreadIMs + 1
			end

			local targetSlotNumber = (HoN_IM_Panel:GetIndexFromNameInsensitive(name) - HoN_IM_Panel.playerListOffset)
			if (targetSlotNumber > 0 and targetSlotNumber <= NUMBER_VISIBLE_SLOTS) then
				GetWidget("im_name_"..targetSlotNumber.."_icon"):SetTexture("/ui/fe2/elements/comm_newmsg.tga")
			end
		end

		HoN_IM_Panel:UpdateSysbarIcon()
	end
end

-- Set the chat text when selecting a user
function HoN_IM_Panel:SelectChat(playerOffset)
	if (playerOffset <= HoN_IM_Panel.numberOfPlayers) then
		HoN_IM_Panel.previouslySelectedIndex = HoN_IM_Panel.selectedPlayerIndex
		HoN_IM_Panel.selectedPlayerIndex = playerOffset
		HoN_IM_Panel.selectedPlayerName = HoN_IM_Panel.playerNames[playerOffset]

		-- set the focused im
		if (HoN_IM_Panel.selectedPlayerName) then
			interface:UICmd("ChatFocusedIM('"..HoN_IM_Panel.selectedPlayerName.."');")
			interface:UICmd("ChatRemoveUnreadIMs('"..HoN_IM_Panel.selectedPlayerName.."');")

			lName = string.lower(HoN_IM_Panel.selectedPlayerName)
			HoN_IM_Panel.totalUnreadIMs = HoN_IM_Panel.totalUnreadIMs - HoN_IM_Panel.unreadIMCounts[lName]
			HoN_IM_Panel.unreadIMCounts[lName] = 0
		end
		HoN_IM_Panel:UpdateSysbarIcon()

		-- set the chat icons
		if (HoN_IM_Panel.previouslySelectedIndex) then	-- prev will be normal
			local prevSlotNumber = (HoN_IM_Panel.previouslySelectedIndex - HoN_IM_Panel.playerListOffset)
			if (prevSlotNumber > 0 and prevSlotNumber <= NUMBER_VISIBLE_SLOTS) then
				GetWidget("im_name_"..prevSlotNumber.."_icon"):SetTexture("/ui/elements/chat_icon.tga")
				GetWidget("im_nameplate_"..prevSlotNumber.."_selected"):FadeOut("250")
			end
		end
		local slotNumber = (playerOffset - HoN_IM_Panel.playerListOffset)	-- current
		if (slotNumber > 0 and slotNumber <= NUMBER_VISIBLE_SLOTS) then
			GetWidget("im_name_"..slotNumber.."_icon"):SetTexture("/ui/fe2/elements/comm_current.tga")
			GetWidget("im_nameplate_"..slotNumber.."_selected"):FadeIn("250")
		end

		-- clear and then set the text in the chatbox
		local IMWindow = GetWidget("im_chatbox");
		if (IMWindow) then
			IMWindow:UICmd("ClearBufferText()")
			for _,text in ipairs(HoN_IM_Panel.chatHistories[string.lower(HoN_IM_Panel.selectedPlayerName)]) do
				IMWindow:UICmd("AddBufferText('"..text.."')")
			end

		end
	end
end

-- Close a chat tab
function HoN_IM_Panel:EndChat(playerOffset)
	if (playerOffset <= HoN_IM_Panel.numberOfPlayers) then

		if ((HoN_IM_Panel.selectedPlayerIndex == playerOffset) and HoN_IM_Panel.previouslySelectedIndex) then
			HoN_IM_Panel:SelectChat(HoN_IM_Panel.previouslySelectedIndex)	-- we are closing the current chat and have a previous
		end

		interface:UICmd("ChatCloseIM('"..HoN_IM_Panel.playerNames[playerOffset].."');")
		interface:UICmd("ChatRemoveUnreadIMs('"..HoN_IM_Panel.playerNames[playerOffset].."');")	--remove unread, even though we arent reading them
		HoN_IM_Panel:UpdateSysbarIcon()

		lName = string.lower(HoN_IM_Panel.playerNames[playerOffset])
		--HoN_IM_Panel.chatHistories[lName] = nil	-- clear the text, remove the name
		HoN_IM_Panel.chatWidgetActive[lName] = false
		HoN_IM_Panel.totalUnreadIMs = HoN_IM_Panel.totalUnreadIMs - HoN_IM_Panel.unreadIMCounts[lName]
		HoN_IM_Panel.unreadIMCounts[lName] = nil

		table.remove(HoN_IM_Panel.playerNames, playerOffset)

		HoN_IM_Panel.numberOfPlayers = HoN_IM_Panel.numberOfPlayers - 1

		local numberScroll = HoN_IM_Panel.numberOfPlayers - NUMBER_VISIBLE_SLOTS
		local scrollbar = GetWidget("im_list_scrollbar")
		if (numberScroll > 0) then
			scrollbar:SetMaxValue(numberScroll)
		else
			scrollbar:SetEnabled(0)
			scrollbar:SetMaxValue(0)
		end

		if (HoN_IM_Panel.numberOfPlayers == 0) then
			GetWidget("no_im_cover"):SetVisible(1)	-- no chats, reshow the cover
			GetWidget("whisper_box_panel"):SetVisible(0)	-- hide the chat window
			HoN_IM_Panel.selectedPlayerName, HoN_IM_Panel.selectedPlayerIndex = nil, nil
			HoN_IM_Panel.lastMessageFrom = nil
		elseif(HoN_IM_Panel.selectedPlayerIndex ~= playerOffset) then	-- keep the currently select chat
			if (HoN_IM_Panel.selectedPlayerIndex > playerOffset) then
				HoN_IM_Panel:SelectChat(HoN_IM_Panel.selectedPlayerIndex - 1)
			end
		else															-- select the top chat, they are all new
			HoN_IM_Panel:SelectChat(1)
		end

		-- update the visible list, unless you close a chat below the visible boxes, it will change
		HoN_IM_Panel:UpdateVisibleSlots(HoN_IM_Panel.playerListOffset)
	end
end

-- Update the names in the IM Name slots
function HoN_IM_Panel:UpdateVisibleSlots(listOffset)
	HoN_IM_Panel.playerListOffset = listOffset

	for i = 1, NUMBER_VISIBLE_SLOTS do
		local nameWidget = GetWidget("im_name_"..i)
		local nameLabel = nameWidget:GetWidget("im_nameplate_"..i)

		if (nameWidget and nameLabel) then
			if (i+listOffset > HoN_IM_Panel.numberOfPlayers) then
				nameWidget:SetVisible(0);	-- we are out of players, hide the list element
				break;	-- no need to check for more, only need to set this one invisible as it will be the only one not already invis
			else
				nameWidget:SetVisible(1);	-- show and set the name for the list element
				nameLabel:SetText(HoN_IM_Panel.playerNames[i+listOffset]);

				-- set icons and highlight, etc
				if (HoN_IM_Panel.unreadIMCounts[string.lower(HoN_IM_Panel.playerNames[i+listOffset])] > 0) then
					GetWidget("im_name_"..i.."_icon"):SetTexture("/ui/fe2/elements/comm_newmsg.tga")
				else
					GetWidget("im_name_"..i.."_icon"):SetTexture("/ui/elements/chat_icon.tga")
				end

				if ((i+listOffset) == HoN_IM_Panel.selectedPlayerIndex) then
					GetWidget("im_name_"..i.."_icon"):SetTexture("/ui/fe2/elements/comm_current.tga")
					GetWidget("im_nameplate_"..i.."_selected"):FadeIn("250")
				else
					GetWidget("im_nameplate_"..i.."_selected"):FadeOut("0")
				end
			end
		end
	end
end

-- send a chat message to the targeted player
function HoN_IM_Panel:SendChatMessage(chatbox, target)
	local muted = false
	if (IsChatMuted and IsChatMuted()) then muted = true end

	if ((not muted) or (IsBuddy(target) or IsClanMember(taget))) then
		interface:UICmd("ChatSendIM('"..target.."', '"..string.gsub(string.gsub(chatbox:GetValue(), "\\", "\\\\"), "'", "\\'").."');")
	else
		-- can't talk to anyone except friends and clan members
		HoN_IM_Panel:AddChatEntry(target, Translate("im_muted", 'days', ceil(GetChatMuteExpiration()/86400), 0))
	end

	-- clear the input line
	chatbox:Clear()
	chatbox:SetFocus(1)
end

function HoN_IM_Panel:CreateChat(name, info)
	local lName = string.lower(name)
	HoN_IM_Panel.chatWidgetActive = HoN_IM_Panel.chatWidgetActive or {}
	if (not HoN_IM_Panel.chatWidgetActive[lName]) then		--history doesn't exist, chat is not opened
		if (info == 0) then
			return 0
		end

		HoN_IM_Panel.chatWidgetActive[lName] = true

		-- set the casing if we have the info, else set it up in AddChatEntry
		if (GetChatClientInfo) then
			local nameString = GetChatClientInfo(lName, "name")
			if (nameString and (string.sub(nameString, 0, -2) ~= "")) then name = StripClanTag(string.sub(nameString, 0, -2)) else HoN_IM_Panel.needUserInfo[lName] = true end
		end

		-- add the player to the list, create the history table
		table.insert(HoN_IM_Panel.playerNames, name)
		HoN_IM_Panel.chatHistories[lName] = HoN_IM_Panel.chatHistories[lName] or {}
		HoN_IM_Panel.unreadIMCounts[lName] = 0

		HoN_IM_Panel.numberOfPlayers = HoN_IM_Panel.numberOfPlayers + 1

		-- bump the max value for the scroller
		local numberScroll = HoN_IM_Panel.numberOfPlayers - NUMBER_VISIBLE_SLOTS
		local scrollbar = GetWidget("im_list_scrollbar")
		if (numberScroll > 0 and scrollbar) then

			scrollbar:SetEnabled(1)
			scrollbar:SetMaxValue(numberScroll)
		elseif (scrollbar) then
			scrollbar:SetEnabled(0)
			scrollbar:SetMaxValue(0)
		end

		if (HoN_IM_Panel.numberOfPlayers >= 1) then		-- first/only chat
			GetWidget("no_im_cover"):SetVisible(0)
			GetWidget("whisper_box_panel"):SetVisible(1)
			if (HoN_IM_Panel:IsChatVisible() and HoN_IM_Panel.numberOfPlayers == 1) then
				HoN_IM_Panel:SelectChat(1)
			end
		end

		if ((HoN_IM_Panel.numberOfPlayers - HoN_IM_Panel.playerListOffset) <= NUMBER_VISIBLE_SLOTS) then
			HoN_IM_Panel:UpdateVisibleSlots(HoN_IM_Panel.playerListOffset)	-- the new IM will be visible so update the list to show it
		end

		return 1
	end

	return 2
end

function HoN_IM_Panel:GetIndexFromName(name)
	for i,v_name in ipairs(HoN_IM_Panel.playerNames) do
		if (v_name == name) then
			return i
		end
	end

	return nil
end

function HoN_IM_Panel:GetIndexFromNameInsensitive(name)
	local lName = string.lower(name)
	for i,v_name in ipairs(HoN_IM_Panel.playerNames) do
		local v_lName = string.lower(v_name)

		if (v_lName == lName) then
			return i
		end
	end

	return nil
end

function HoN_IM_Panel:UpdateSysbarIcon()
	GetWidget("im_up_msgs"):SetVisible(HoN_IM_Panel.totalUnreadIMs > 0)
	GetWidget("im_over_msgs"):SetVisible(HoN_IM_Panel.totalUnreadIMs > 0)
	GetWidget("im_down_msgs"):SetVisible(HoN_IM_Panel.totalUnreadIMs > 0)
	GetWidget("im_up_label"):SetText(tostring(HoN_IM_Panel.totalUnreadIMs))
	GetWidget("im_over_label"):SetText(tostring(HoN_IM_Panel.totalUnreadIMs))
	GetWidget("im_down_label"):SetText(tostring(HoN_IM_Panel.totalUnreadIMs))
end

function HoN_IM_Panel:ChatSend(self)
	local chatbox = GetWidget("im_chat_box");

	if (HoN_IM_Panel.selectedPlayerName and chatbox) then
		HoN_IM_Panel:SendChatMessage(chatbox, HoN_IM_Panel.selectedPlayerName)
	end
end

function HoN_IM_Panel:ChatFocusChange(self, slot)
	HoN_IM_Panel:SelectChat(slot + HoN_IM_Panel.playerListOffset)
end

function HoN_IM_Panel:ChatClosed(self, slot)
	HoN_IM_Panel:EndChat(slot + HoN_IM_Panel.playerListOffset)
end

function HoN_IM_Panel:ChatWindowToggled(self, isVisible)
	if (tonumber(isVisible) == 0) then
		HoN_IM_Panel.lastMessageFrom = HoN_IM_Panel.selectedPlayerName
	else
		if (HoN_IM_Panel.lastMessageFrom) then
			HoN_IM_Panel:SelectChat(HoN_IM_Panel:GetIndexFromNameInsensitive(HoN_IM_Panel.lastMessageFrom))
			HoN_IM_Panel:UpdateVisibleSlots(HoN_IM_Panel.playerListOffset) --updates status icons
			GetWidget('im_chat_box'):SetFocus(1)
		end
	end
end

function HoN_IM_Panel:ChatWindowSlide(self, value)
	HoN_IM_Panel:UpdateVisibleSlots(value)
end

function HoN_IM_Panel:IsChatVisible()
	return GetWidget("ccwidget_whisper"):IsVisible()
end

-- allow ui to use lua functions
function interface:HoNIMPanelF(func, ...)
	print(HoN_IM_Panel[func](self, ...))
end

-- get messages from the game when the main ui is hidden

-- watch functions
local function ChatWhisperUpdate(self, name, text, info)
	if (NotEmpty(name)) then
		HoN_IM_Panel:AddChatEntry(name, text, tonumber(info))
	end
end

local function ChatUserStatus(self, name, status)
	if (name ~= StripClanTag(GetAccountName())) then	-- can't open a chat with yourself
		if (tonumber(status) >= 2) then	--user online
			if (HoN_IM_Panel:CreateChat(name, 1) == 1) then
				--set that chat as active (newest one)
				HoN_IM_Panel:SelectChat(HoN_IM_Panel.numberOfPlayers)
			else
				local index = HoN_IM_Panel:GetIndexFromNameInsensitive(name)
				if (index) then
					HoN_IM_Panel:SelectChat(index)
				end
			end
		else 	-- display the error
			GetWidget("im_name_error"):SetText(Translate("im_user_not_found"))
			GetWidget("im_user_error_panel"):SetVisible(1)
		end
	else -- reselect the new IM box
		GetWidget("cc_new_im_textbox"):SetFocus(true)
		GetWidget("im_name_error"):SetText(Translate("im_error_self"))
		GetWidget("im_user_error_panel"):SetVisible(1)
	end
end

local function ChatOpenMessage(self, name)
	local index = HoN_IM_Panel:GetIndexFromNameInsensitive(name)
	if (index) then
		HoN_IM_Panel:SelectChat(index)		-- select the chat
		GetWidget('im_chat_box'):SetFocus(1)
	else
		HoN_IM_Panel:CreateChat(name, 1)	-- create the chat and select it (the last index)
		HoN_IM_Panel:SelectChat(HoN_IM_Panel.numberOfPlayers)
		GetWidget('im_chat_box'):SetFocus(1)
	end

	-- make the window visible
	HoN_IM_Panel.lastMessageFrom = nil	-- prevent the visible toggle from screwing us up
	GetWidget("ccwidget_whisper"):SetVisible(1)
end

interface:RegisterWatch("ChatWhisperUpdate", ChatWhisperUpdate)
interface:RegisterWatch("ChatUserStatus", ChatUserStatus)
interface:RegisterWatch("ChatMessageTrigger", ChatOpenMessage)