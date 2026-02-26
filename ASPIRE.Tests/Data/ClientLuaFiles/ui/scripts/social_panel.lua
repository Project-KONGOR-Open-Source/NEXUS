----------------------------------------------------------
--	Name: 		Social Panel Script	            		--
--  Copyright 2015 Frostburn Studios					--
----------------------------------------------------------

local DEFAULT_ONLINE_COLOR = '#39c6ff'
local DEFAULT_INGAME_COLOR = '#f3c113'
local DEFAULT_ONLINE_CUSTOM_COLOR = '#FFFFFFFF'
local DEFAULT_INGAME_CUSTOM_COLOR = '#999999FF'
local DEFAULT_OFFLINE_COLOR = '.6 .6 .6 .9'
local DEFAULT_AUTOCOMPLETE_COLOR = '#39c6ff'
local DEFAULT_AUTOCOMPLETE_COLOR_OFFLINE = '#39c6ffBB'
local MAX_USERENTRY_SLOTS = 20
local MAX_RECENTLY_PLAYED = 20

-- Options to move later
local IGNORE_OFFLINE = false
local LOAD_DEFAULTS = false
SHOW_CUSTOM_COLORS = false
USER_SORT_ONLINE_FIRST = true
MERGE_CLAN_FRIENDS = false
OFFLINE_IN_GROUPS = false

-- Magic numbers to calculate from UI later
local social_playerItem_height = '2.8h'
local social_notificationItem_height = '6.3h'
local social_playerHeader_height = '2.8h'
local social_mainPanel_extraSpace = '2.8h'
local social_mainPanel_minHeight = '20h'
local social_mainPanel_maxHeight = '92.8h'

----------------------------------------------------------

local _G = getfenv(0)
HoN_Social_Panel = {}
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort
local interface, interfaceName = object, object:GetName()
local triggerHelper = GetWidget("social_panel_trigger_helper")
RegisterScript2('Social Panel', '30')
HoN_Social_Panel.selectedPlayerName, HoN_Social_Panel.selectedPlayerIndex, HoN_Social_Panel.lastSelectedName = nil, nil, nil
HoN_Social_Panel.lockBuddyTable, HoN_Social_Panel.lockBuddyList, HoN_Social_Panel.widgetsInitialized = false, false, HoN_Social_Panel.widgetsInitialized or false
HoN_Social_Panel.playerIndex, HoN_Social_Panel.userEntryIndex, HoN_Social_Panel.userOffset, HoN_Social_Panel.visibleUserEntrySlots, HoN_Social_Panel.offlineCount, HoN_Social_Panel.onlineCount = 0, 0, 0, MAX_USERENTRY_SLOTS, 0, 0
HoN_Social_Panel.lastUserOffset = 0
HoN_Social_Panel.autoCompleteString, HoN_Social_Panel.joinGroupString = nil, nil
HoN_Social_Panel.entryTable, HoN_Social_Panel.headerTable, HoN_Social_Panel.autoCompleteTable, HoN_Social_Panel.alertTable, HoN_Social_Panel.userEntryWidgetTable, HoN_Social_Panel.userEntryPlayerTable, HoN_Social_Panel.tempGroupTable, HoN_Social_Panel.inviteTable = {}, {}, {}, {}, {}, {}, {}, {}
HoN_Social_Panel.rightLastSelectedID, HoN_Social_Panel.leftLastSelectedID = nil, nil
HoN_Social_Panel.friendslist = HoN_Social_Panel.friendslist or {}
HoN_Social_Panel.grouplist = HoN_Social_Panel.grouplist or {}
HoN_Social_Panel.inactiveInvitedList = HoN_Social_Panel.inactiveInvitedList or {}
--HoN_Social_Panel.grouplist['add_friend'] = true
HoN_Social_Panel.groupTable = HoN_Social_Panel.groupTable or {}
HoN_Social_Panel.notificationsTable = HoN_Social_Panel.notificationsTable or {}
HoN_Social_Panel.leftMenuClosed = true
HoN_Social_Panel_DB = Database.New('HoN_Social_Panel.ldb')
local db = HoN_Social_Panel_DB

function HoN_Social_Panel:UpdateUserScroller(offset)
	local offset = offset or 0
	local scroller = interface:GetWidget('sp_user_list_scroller')
	local currentValue = scroller:UICmd("GetValue()")
	if ((HoN_Social_Panel.totalEntries + 4) > HoN_Social_Panel.visibleUserEntrySlots ) then
		scroller:SetVisible(true)
		scroller:SetEnabled(1)
		if (offset) and (currentValue ~= (HoN_Social_Panel.visibleUserEntrySlots + offset)) then
			HoN_Social_Panel.lastUserOffset = currentValue - HoN_Social_Panel.visibleUserEntrySlots
			scroller:UICmd("SetMaxValue("..(HoN_Social_Panel.totalEntries + 4).."); SetMinValue("..(HoN_Social_Panel.visibleUserEntrySlots).."); SetValue("..(HoN_Social_Panel.visibleUserEntrySlots + offset)..")")
		end
	else
		scroller:SetEnabled(0)
		if GetCvarBool('ui_socialPanelCanDrag') then
			scroller:SetVisible(false)
		else
			scroller:SetVisible(true)
			if (offset) and (currentValue ~= (HoN_Social_Panel.visibleUserEntrySlots + offset)) then
				HoN_Social_Panel.lastUserOffset = currentValue - HoN_Social_Panel.visibleUserEntrySlots
				scroller:UICmd("SetMaxValue("..(HoN_Social_Panel.totalEntries).."); SetMinValue("..(HoN_Social_Panel.visibleUserEntrySlots).."); SetValue("..(HoN_Social_Panel.visibleUserEntrySlots + offset)..")")
			end
		end
		HoN_Social_Panel.lastUserOffset = 0
	end
	HoN_Social_Panel.lockBuddyList = false
end

local function UpdateOptionsButtons()
	interface:GetWidget('sp_options_btn_1'):SetButtonState(SHOW_CUSTOM_COLORS and 1 or 0)
	interface:GetWidget('sp_options_btn_2'):SetButtonState(USER_SORT_ONLINE_FIRST and 1 or 0)
	interface:GetWidget('sp_options_btn_3'):SetButtonState(MERGE_CLAN_FRIENDS and 1 or 0)
	interface:GetWidget('sp_options_btn_5'):SetButtonState(OFFLINE_IN_GROUPS and 1 or 0)

	HoN_Options:UpdateLUAOptions()
end

local SHOW_CUSTOM_COLORS_CVAR 	= 'social_customColors'
local USER_SORT_ONLINE_FIRST_CVAR = 'social_sortOnlineFirst'
local MERGE_CLAN_FRIENDS_CVAR 	= 'social_mergeClanFriends'
local OFFLINE_IN_GROUPS_CVAR 	= 'social_offlineInGames'

local function SaveOptions()
	db.IGNORE_OFFLINE 			= IGNORE_OFFLINE
	db.LOAD_DEFAULTS 			= LOAD_DEFAULTS
	-- group visibility
	db.grouplist 				= HoN_Social_Panel.grouplist

	db:Flush()

	local cvar
	SetSave(SHOW_CUSTOM_COLORS_CVAR, SHOW_CUSTOM_COLORS, 'bool', true)
	cvar = Cvar.GetCvar(SHOW_CUSTOM_COLORS_CVAR)
	if cvar then
		Cvar.SetCloudSave(cvar, true)
	end

	SetSave(USER_SORT_ONLINE_FIRST_CVAR, USER_SORT_ONLINE_FIRST, 'bool', true)
	cvar = Cvar.GetCvar(USER_SORT_ONLINE_FIRST_CVAR)
	if cvar then
		Cvar.SetCloudSave(cvar, true)
	end

	SetSave(MERGE_CLAN_FRIENDS_CVAR, MERGE_CLAN_FRIENDS, 'bool', true)
	cvar = Cvar.GetCvar(MERGE_CLAN_FRIENDS_CVAR)
	if cvar then
		Cvar.SetCloudSave(cvar, true)
	end

	SetSave(OFFLINE_IN_GROUPS_CVAR, OFFLINE_IN_GROUPS, 'bool', true)
	cvar = Cvar.GetCvar(OFFLINE_IN_GROUPS_CVAR)
	if cvar then
		Cvar.SetCloudSave(cvar, true)
	end

	UpdateOptionsButtons()
end

local function LoadOptions()
	if (db.IGNORE_OFFLINE == nil) then
		SaveOptions()
	else
		IGNORE_OFFLINE 		 	= db.IGNORE_OFFLINE
		LOAD_DEFAULTS		 	= db.LOAD_DEFAULTS
		
		-- group visibility
		HoN_Social_Panel.grouplist = db.grouplist or {}

		local cvar = Cvar.GetCvar(SHOW_CUSTOM_COLORS_CVAR)
		if cvar == nil then
			SHOW_CUSTOM_COLORS 	= db.SHOW_CUSTOM_COLORS or false
		else
			SHOW_CUSTOM_COLORS	= cvar:GetBoolean()
		end

		local cvar = Cvar.GetCvar(USER_SORT_ONLINE_FIRST_CVAR)
		if cvar == nil then
			USER_SORT_ONLINE_FIRST 	= db.USER_SORT_ONLINE_FIRST or true
		else
			USER_SORT_ONLINE_FIRST	= cvar:GetBoolean()
		end

		local cvar = Cvar.GetCvar(MERGE_CLAN_FRIENDS_CVAR)
		if cvar == nil then
			MERGE_CLAN_FRIENDS 	= db.MERGE_CLAN_FRIENDS or false
		else
			MERGE_CLAN_FRIENDS	= cvar:GetBoolean()
		end

		local cvar = Cvar.GetCvar(OFFLINE_IN_GROUPS_CVAR)
		if cvar == nil then
			OFFLINE_IN_GROUPS 	= db.OFFLINE_IN_GROUPS or OFFLINE_IN_GROUPS or false
		else
			OFFLINE_IN_GROUPS	= cvar:GetBoolean()
		end

		UpdateOptionsButtons()
	end
end

function HoN_Social_Panel:OnShowOptions()
	UpdateOptionsButtons()
end

function HoN_Social_Panel:OptionsButtonClicked(widget, buttonIndex)
	--print('widget: '..tostring(widget)..' | name: ' .. widget:GetName() ..' | buttonIndex: ' .. buttonIndex )
	if (buttonIndex == 1) then
		SHOW_CUSTOM_COLORS = not SHOW_CUSTOM_COLORS
	elseif (buttonIndex == 2) then
		USER_SORT_ONLINE_FIRST = not USER_SORT_ONLINE_FIRST
	elseif (buttonIndex == 3) then
		MERGE_CLAN_FRIENDS = not MERGE_CLAN_FRIENDS
	elseif (buttonIndex == 5) then
		OFFLINE_IN_GROUPS = not OFFLINE_IN_GROUPS
	end
	SaveOptions()
	ChatRefresh()
end

function HoN_Social_Panel:UpdateOptions()
	SaveOptions()
	ChatRefresh()
end

local function UpdateGroupArrows(instant)
	for i, v in pairs(HoN_Social_Panel.headerTable) do
		if (HoN_Social_Panel.grouplist[v] == nil or HoN_Social_Panel.grouplist[v]) then
			if (not instant) then
				GetWidget('sp_im_category_header_'..i..'_arrow'):Rotate(0, 150)
			else
				GetWidget('sp_im_category_header_'..i..'_arrow'):SetRotation(0)
			end
		else
			if (not instant) then
				GetWidget('sp_im_category_header_'..i..'_arrow'):Rotate(180, 150)
			else
				GetWidget('sp_im_category_header_'..i..'_arrow'):SetRotation(180)
			end
		end
	end
end

local function ToggleGroupVisibility(group)
	if (HoN_Social_Panel.grouplist[group] ~= nil) then
		HoN_Social_Panel.grouplist[group] = not HoN_Social_Panel.grouplist[group]
	else
		HoN_Social_Panel.grouplist[group] = false
	end

	UpdateGroupArrows()
	GetWidget("social_panel_trigger_helper"):Sleep(150, function() HoN_Social_Panel:PopulateFriendsList(HoN_Social_Panel.userOffset or 0) end)

	-- save the toggles
	SaveOptions()

	--[[
	if (HoN_Social_Panel.entryTable[group]) then
		for i,HoN_Social_Panel.userEntryIndex in ipairs(HoN_Social_Panel.entryTable[group]) do
			local parent = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex)
			if parent:IsVisibleSelf() or interface:GetWidget('sp_im_category_header_'..HoN_Social_Panel.userEntryIndex):IsVisibleSelf() then
				parent:FadeOut(250)
				--print('^g Fading Out: ' .. HoN_Social_Panel.userEntryIndex .. '\n' )
			else
				parent:FadeIn(250)
				--print('^r Fading In: ' .. HoN_Social_Panel.userEntryIndex .. '\n' )
			end
		end
	end
	--]]
end

-- local function RightClickMenuVisibilty(show)
-- 	if (HoN_Social_Panel.rightLastSelectedID) then
-- 		if (show) then
-- 			interface:GetWidget('sp_player_item_'..HoN_Social_Panel.rightLastSelectedID..'_hover'):SetVisible(true)
-- 		else
-- 			interface:GetWidget('sp_player_item_'..HoN_Social_Panel.rightLastSelectedID..'_hover'):SetVisible(false)
-- 			HoN_Social_Panel.rightLastSelectedID = nil
-- 		end
-- 	end
-- end

function HoN_Social_Panel:ClearCurrentlySelected()
	for i=1, HoN_Social_Panel.visibleUserEntrySlots do
		local name = GetWidget("sp_player_item_"..i.."_name")
		local hover = GetWidget("sp_player_item_"..i.."_hover")

		if (StripClanTag(name:GetValue()) == HoN_Social_Panel.selectedPlayerName and hover:IsVisible()) then
			hover:SetVisible(0)
		end
	end
end

function HoN_Social_Panel:UserEntryMouseover(id, isOver, widgetName, isMiniButton)
	if (isOver) then
		interface:GetWidget('sp_player_item_'..id..'_hover'):SetVisible(true)
	else
		if (HoN_Social_Panel.selectedPlayerName == nil) or (StripClanTag(GetWidget("sp_player_item_"..id.."_name"):GetValue()) ~= HoN_Social_Panel.selectedPlayerName) then
			interface:GetWidget('sp_player_item_'..id..'_hover'):SetVisible(false)
		end
	end
end

function HoN_Social_Panel:UserEntryMiniButtonClicked(id, isRightClick, buttonID)
	if (HoN_Social_Panel.userEntryPlayerTable[id]) then

	end
end

function HoN_Social_Panel:UserEntryButtonFriendIconClicked(id, isRightClick, isDoubleClick)
	HoN_Social_Panel.rightLastSelectedID = id
	if (HoN_Social_Panel.userEntryPlayerTable[id]) then
		Cvar.CreateCvar('ui_lastSelectedUser', 'string', HoN_Social_Panel.userEntryPlayerTable[id])
		ChatAddBuddy(HoN_Social_Panel.userEntryPlayerTable[id])
	end
end

function HoN_Social_Panel:UserEntryButtonIconClicked(id, isRightClick, isDoubleClick)
	if (not isDoubleClick) then			-- right or left click
		HoN_Social_Panel.rightLastSelectedID = id
		if (HoN_Social_Panel.userEntryPlayerTable[id]) then
			Cvar.CreateCvar('ui_lastSelectedUser', 'string', HoN_Social_Panel.userEntryPlayerTable[id])
			Cvar.CreateCvar('ui_currentChannel', 'string', '')
			HoN_Social_Panel:ClearCurrentlySelected()
			if (HoN_Social_Panel:OpenLeftUserMenu(id)) then
				HoN_Social_Panel.selectedPlayerName =  HoN_Social_Panel.userEntryPlayerTable[id]
			end
		end
	else
		if ( HoN_Social_Panel.userEntryWidgetTable[id] == 'autocomplete' ) or ( (HoN_Social_Panel.autoCompleteString ~= nil) and (string.len(HoN_Social_Panel.autoCompleteString) >= 1) ) then
			local fullName = interface:GetWidget('sp_player_item_'..id..'_name'):GetValue()
			ChatAddBuddy(fullName)
		else
			interface:GetWidget('sp_textbox_friendlist_input'):SetFocus(false)
			if (isDoubleClick) then	-- or (HoN_Social_Panel.leftLastSelectedID == id)
				if (HoN_Social_Panel.userEntryPlayerTable[id]) then
					if (StripClanTag(GetAccountName()) ~= HoN_Social_Panel.userEntryPlayerTable[id]) and AtoB(interface:UICmd("ChatUserOnline('"..HoN_Social_Panel.userEntryPlayerTable[id].."')") ) then
						interface:UICmd("ChatOpenMessage('"..HoN_Social_Panel.userEntryPlayerTable[id].."')")
					end
				end
			end
		end
		HoN_Social_Panel.leftLastSelectedID = id
	end
end

function HoN_Social_Panel:ClosedLeftMenu()
	HoN_Social_Panel.leftMenuClosed = true
	HoN_Social_Panel:ClearCurrentlySelected()
	HoN_Social_Panel.lastSelectedName = HoN_Social_Panel.selectedPlayerName
	HoN_Social_Panel.selectedPlayerName = nil
	HoN_Social_Panel.selectedPlayerIndex = nil
	HoN_Social_Panel.leftLastSelectedID = nil
	HoN_Social_Panel.rightLastSelectedID = nil
end

local function CalculateLeftMenuY(id, menuWidget)
	local yPos = GetWidget("sp_player_item_"..id):GetY() + interface:GetHeightFromString("4.0h")
	local spWidget = GetWidget("social_panel")
	if ((yPos + menuWidget:GetHeight()) > spWidget:GetHeight()) then
		yPos = spWidget:GetHeight() - menuWidget:GetHeight()
	end

	return yPos
end

function HoN_Social_Panel:OpenLeftUserMenu(id, sliding)
	local menu = GetWidget("sp_left_user_menu")
	GetWidget("sp_player_item_"..id.."_hover"):SetVisible(1) 	-- select whoever we 'clicked'

	if (HoN_Social_Panel.selectedPlayerName ~= HoN_Social_Panel.userEntryPlayerTable[id]) then
		local menu = GetWidget("sp_left_user_menu")
		if (not HoN_Social_Panel.leftMenuClosed) then
			local y = menu:GetY() 	-- fix sliding being funny on resizing
			menu:DoEvent()

			if (not sliding) then menu:SetY(y) menu:SlideY(CalculateLeftMenuY(id, menu), 200) else menu:SetY(CalculateLeftMenuY(id, menu)) end

			menu:SetX('-100%')
		else
			menu:DoEvent()
			menu:SetY(CalculateLeftMenuY(id, menu))
			menu:DoEventN(2)
			HoN_Social_Panel.leftMenuClosed = false
		end
	else
		if (not HoN_Social_Panel.leftMenuClosed) then
			if (not sliding) then
				menu:DoEventN(1)
				return false
			end
		else
			menu:DoEvent()
			menu:SetY(CalculateLeftMenuY(id, menu))
			menu:DoEventN(2) 	-- turn the hover back on after turning it off from the above
			HoN_Social_Panel.leftMenuClosed = false
		end
	end

	return true
end

function HoN_Social_Panel:UserEntryButtonClicked(id, isRightClick, isDoubleClick)
	if (not isDoubleClick) then		-- right or left click
		HoN_Social_Panel.rightLastSelectedID = id
		if (HoN_Social_Panel.userEntryPlayerTable[id]) then
			Cvar.CreateCvar('ui_lastSelectedUser', 'string', HoN_Social_Panel.userEntryPlayerTable[id])
			Cvar.CreateCvar('ui_currentChannel', 'string', '')
			HoN_Social_Panel:ClearCurrentlySelected()
			if (HoN_Social_Panel:OpenLeftUserMenu(id)) then
				HoN_Social_Panel.selectedPlayerName = HoN_Social_Panel.userEntryPlayerTable[id]
			end
		end
	else
		if ( HoN_Social_Panel.userEntryWidgetTable[id] == 'autocomplete' ) or ( (HoN_Social_Panel.autoCompleteString ~= nil) and (string.len(HoN_Social_Panel.autoCompleteString) >= 1) ) then
			local fullName = interface:GetWidget('sp_player_item_'..id..'_name'):GetValue()
			interface:GetWidget('sp_textbox_friendlist_input'):UICmd("SetInputLine('"..fullName.."')")
		else
			interface:GetWidget('sp_textbox_friendlist_input'):SetFocus(false)
			if (isDoubleClick) then -- or (HoN_Social_Panel.leftLastSelectedID == id)
				if (HoN_Social_Panel.userEntryPlayerTable[id]) then
					if (StripClanTag(GetAccountName()) ~= HoN_Social_Panel.userEntryPlayerTable[id]) and AtoB(interface:UICmd("ChatUserOnline('"..HoN_Social_Panel.userEntryPlayerTable[id].."')") ) then
						interface:UICmd("ChatOpenMessage('"..HoN_Social_Panel.userEntryPlayerTable[id].."')")
					end
				end
			end
		end
		HoN_Social_Panel.leftLastSelectedID = id
		GetWidget('sp_left_user_menu'):DoEventN(1)
	end
end

function HoN_Social_Panel:GroupButtonClicked(id)
	ToggleGroupVisibility(HoN_Social_Panel.headerTable[id])
end

local function UpdateSystemBarPlayerCount()
	local friends_online = interface:GetWidget('sysbar_friendsonline')
	local labelGroup = interface:GetGroup('sysbar_friendsonline_label')

	if not isNewUI() then
		friends_online:SetWidth( friends_online:UICmd("GetStringWidth('dyn_10', '"..HoN_Social_Panel.onlineCount.."')") + friends_online:GetWidthFromString('5.5h') )
	end

	if labelGroup then
		for index, label in ipairs(labelGroup) do
			label:SetText(HoN_Social_Panel.onlineCount) -- HoN_Social_Panel.onlineCount..'/'..HoN_Social_Panel.offlineCount
		end
	end
	local sp_header_friends_label = interface:GetWidget('sp_header_friends_label')
	if (sp_header_friends_label) then
		sp_header_friends_label:SetText(HoN_Social_Panel.onlineCount..'/'..HoN_Social_Panel.offlineCount);
	end

	Cvar.CreateCvar('friends_offline', 'int', HoN_Social_Panel.offlineCount)
end

local function UpdateGroupListbox()
	local sp_group_listbox = interface:GetWidget('sp_group_listbox')
	sp_group_listbox:ClearItems()
	local currentGroup = UIGetBuddyGroup(HoN_Social_Panel.selectedPlayerName)
	if (string.len(currentGroup) >= 1)  and (currentGroup ~= 'offline') and (currentGroup ~= 'autocomplete') and (currentGroup ~= 'alert') and (currentGroup ~= 'add_friend') and (currentGroup ~= 'recent') and (currentGroup ~= 'inactive') then
		sp_group_listbox:AddTemplateListItem('autocomplete_player', 'remove group', 'username', Translate("sp_remove_group"), 'color', 'red')
	end
	for index,group in pairs(HoN_Social_Panel.groupTable) do
		if (group) and (group.name ~= 'offline') and (group.name ~= 'autocomplete') and (group.name ~= 'alert') and (group.name ~= 'add_friend') and (group.name ~= 'recent')  and (group.name ~= 'inactive') then
			sp_group_listbox:AddTemplateListItem('autocomplete_player', group.name, 'username', group.name)
		end
	end
end

local function TransferFriendsToUI(numericPlayerTable, offset, usingSearch)
	if (numericPlayerTable) then
		HoN_Social_Panel.userEntryIndex = 0
		UpdateSystemBarPlayerCount()

		local localOffSet = offset --(HoN_Social_Panel.visibleUserEntrySlots + offset) - HoN_Social_Panel.playerIndex
		local selectedVisible = false
		for i = 1,HoN_Social_Panel.visibleUserEntrySlots do

			local tempGroupTable = numericPlayerTable[i + localOffSet]

			if (tempGroupTable == nil) then
				HoN_Social_Panel.userEntryIndex = HoN_Social_Panel.userEntryIndex + 1
				local parent = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex)
				local header = interface:GetWidget('sp_im_category_header_'..HoN_Social_Panel.userEntryIndex)
				local addplayer = interface:GetWidget('sp_add_friend_item_'..HoN_Social_Panel.userEntryIndex)
				if (parent) then
					parent:SetVisible(false)
					header:SetVisible(false)
					addplayer:SetVisible(false)
				end
				break
			end

			local playerTable = tempGroupTable.playerTable
			local player, group = nil, nil

			-- For searching, if the player is hidden, check the next one
			while (tempGroupTable) and (playerTable) and ((playerTable.hide) and usingSearch) do
				localOffSet = localOffSet + 1
				tempGroupTable = numericPlayerTable[i + localOffSet]
				if (tempGroupTable) then
					playerTable = tempGroupTable.playerTable
				end
			end

			-- If player data exists (not a header) grab this data
			if type(playerTable) == 'table' then
				player = playerTable.name
				group = playerTable.group
			end

			-- Group hidden check
			while (group) and (HoN_Social_Panel.grouplist[group] == false) do
				--print('^r grouplist is hidden 444! ' .. localOffSet .. ' | ' .. tostring(HoN_Social_Panel.grouplist) .. ' | ' .. group .. ' \n')
				localOffSet = localOffSet + 1
				tempGroupTable = numericPlayerTable[i + localOffSet]
				if (tempGroupTable) then
					playerTable = tempGroupTable.playerTable
					if type(playerTable) == 'table' then
						group = playerTable.group
						player = playerTable.name
					else
						player, group = nil, nil
					end
				else
					if (localOffSet > HoN_Social_Panel.totalPlayers ) then
						break
					end
				end
			end

			if (tempGroupTable) and (tempGroupTable.header) then
				-- increment widget and check that it's visible
				HoN_Social_Panel.userEntryIndex = HoN_Social_Panel.userEntryIndex + 1
				if (HoN_Social_Panel.userEntryIndex > HoN_Social_Panel.visibleUserEntrySlots) then break end

				-- Add header entry
				group = tempGroupTable.group
				-- println('^y group = ' .. tostring(group) )
				local parent = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex)
				local header = interface:GetWidget('sp_im_category_header_'..HoN_Social_Panel.userEntryIndex)
				local addplayer = interface:GetWidget('sp_add_friend_item_'..HoN_Social_Panel.userEntryIndex)
				local expandbtn = interface:GetWidget('sp_im_category_header_'..HoN_Social_Panel.userEntryIndex..'_button')
				if (parent) then
					addplayer:SetVisible(false)
					parent:SetVisible(false)
					if (header) then
						header:SetVisible(true)
					end

					if (group == 'add_friend') then
						expandbtn:SetVisible(false)
						interface:GetWidget('sp_im_category_header_'..HoN_Social_Panel.userEntryIndex..'_name'):UICmd("SetText(Translate('sp_listlabel_"..group.."'))")
					elseif (group == 'friends') or (group == 'clan') or (group == 'cafe') or (group == 'offline') or (group == 'autocomplete') or (group == 'alert') or (group == 'inactive') or (group == 'recent')  then
						if (interface:GetWidget('sp_im_category_header_'..HoN_Social_Panel.userEntryIndex..'_name')) then
							interface:GetWidget('sp_im_category_header_'..HoN_Social_Panel.userEntryIndex..'_name'):UICmd("SetText(Translate('sp_listlabel_"..group.."'))")
							expandbtn:SetVisible(true)
						end
					else
						interface:GetWidget('sp_im_category_header_'..HoN_Social_Panel.userEntryIndex..'_name'):SetText(group)
						expandbtn:SetVisible(true)
					end

					if not (HoN_Social_Panel.headerTable) then HoN_Social_Panel.headerTable = {} end
					HoN_Social_Panel.headerTable[HoN_Social_Panel.userEntryIndex] = group
					HoN_Social_Panel.userEntryWidgetTable[HoN_Social_Panel.userEntryIndex] = 'header'
					UpdateGroupArrows(true)
				end
			elseif (group) and (group == 'add_friend') then
				-- increment widget and check that it's visible
				HoN_Social_Panel.userEntryIndex = HoN_Social_Panel.userEntryIndex + 1

				-- println('HoN_Social_Panel.userEntryIndex = ^g add friend ' .. tostring(HoN_Social_Panel.userEntryIndex) )

				-- if (tempGroupTable) then
					-- printTable(tempGroupTable)
				-- end
				-- if (playerTable) then
					-- printTable(playerTable)
				-- end

				if (HoN_Social_Panel.userEntryIndex > HoN_Social_Panel.visibleUserEntrySlots) then break end

				local parent = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex)
				local header = interface:GetWidget('sp_im_category_header_'..HoN_Social_Panel.userEntryIndex)
				local addplayer = interface:GetWidget('sp_add_friend_item_'..HoN_Social_Panel.userEntryIndex)
				if (parent) then
					addplayer:SetVisible(true)
					parent:SetVisible(false)
					header:SetVisible(false)
				end
			elseif (player) then
				-- increment widget and check that it's visible
				HoN_Social_Panel.userEntryIndex = HoN_Social_Panel.userEntryIndex + 1
				if (HoN_Social_Panel.userEntryIndex > HoN_Social_Panel.visibleUserEntrySlots) then break end

				-- quicker indexed lookup
				if (HoN_Social_Panel.userEntryIndex > HoN_Social_Panel.visibleUserEntrySlots) then break end
				if not (HoN_Social_Panel.entryTable[group]) then HoN_Social_Panel.entryTable[group] = {} end
				tinsert(HoN_Social_Panel.entryTable[group], HoN_Social_Panel.userEntryIndex)

				-- Add player entry
				local parent = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex)
				local header = interface:GetWidget('sp_im_category_header_'..HoN_Social_Panel.userEntryIndex)
				local addplayer = interface:GetWidget('sp_add_friend_item_'..HoN_Social_Panel.userEntryIndex)
				if (parent) then
					addplayer:SetVisible(false)
					parent:SetVisible(true)
					header:SetVisible(false)

					local bg_green = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex..'_bg_green')
					local bg_blue = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex..'_bg_blue')
					local border = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex..'_border')
					local hover = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex..'_hover')
					local hover_bg_green = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex..'_hover_bg_green')
					local hover_bg_blue = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex..'_hover_bg_blue')
					local hover_border = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex..'_hover_border')
					local name = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex..'_name')
					local icon = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex..'_icon')
					local full = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex..'_full')
					local userinfo = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex..'_userinfo')
					local iconframe = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex..'_iconframe')
					local alerticonframe = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex..'_alert_iconframe')
					local alerticon_new = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex..'_alert_icon_new')
					local alerticon_inactive = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex..'_alert_icon_inactive')

					local ascensionLabel = interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex..'_ascension_label')

					---[[ Set player list item
					local tag = playerTable.tag or ''
					--print('tag: ' .. playerTable.tag .. ' | name: ' .. playerTable.name .. '\n')

					name:SetText(tag..playerTable.name)
					if (playerTable.name == HoN_Social_Panel.selectedPlayerName) then
						hover:SetVisible(1)
						if (parent:GetY() < (GetWidget("social_panel"):GetHeight() - interface:GetHeightFromString("6.0h"))) then
							selectedVisible = true
							HoN_Social_Panel:OpenLeftUserMenu(HoN_Social_Panel.userEntryIndex, true)
						end
					else
						hover:SetVisible(0)
					end

					local accountIcon = playerTable.accountIcon or playerTable.chatColorTexture or playerTable.chatSymbolTexture or ''
					local accountIconColour = playerTable.accountIconColour or '1 1 1 1'
					if ((string.len(accountIcon) <= 1) or (accountIcon == '/ui/fe2/store/icons/account_icons/default.tga')) and (not (playerTable.status == 'autocomplete')) then -- catch empty icons from offline players
						accountIcon = '/ui/fe2/store/icons/account_icons/default.tga'
						icon:SetColor('1 1 1 0.4')
					else
						icon:SetColor(accountIconColour)
					end
					icon:SetTexture(accountIcon)

					--Ascension level
					local level = math.abs(tonumber(playerTable.ascensionLevel or '0'))
					if level > 9999 then
						level = 9999;
					end
					GetWidget("sp_player_item_"..HoN_Social_Panel.userEntryIndex.."_ascension"):SetVisible(level > 0)
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
							interface:GetWidget('sp_player_item_'..HoN_Social_Panel.userEntryIndex..'_ascension_img_'..j):SetVisible(j == ascensionImg)
						end

						ascensionLabel:SetText(tostring(level))
					end

					if (false) then	-- playerTable.isAccountInactive
						if false then -- AtoB(playerTable.isAccountInactive)
							alerticonframe:SetVisible(true)
							alerticon_new:SetVisible(false)
							alerticon_inactive:SetVisible(true)
						elseif AtoB(playerTable.isAccountNew) then
							alerticonframe:SetVisible(true)
							alerticon_new:SetVisible(true)
							alerticon_inactive:SetVisible(false)
						else
							alerticonframe:SetVisible(false)
						end
					else
						alerticonframe:SetVisible(false)
					end

					if (playerTable.status == 'online') then
						HoN_Social_Panel.userEntryWidgetTable[HoN_Social_Panel.userEntryIndex] = 'online'
						HoN_Social_Panel.userEntryPlayerTable[HoN_Social_Panel.userEntryIndex] = player
						full:SetVisible(true)
						icon:SetVisible(true)
						if IsFollowing(playerTable.name) then
							name:SetColor('#ff6d3c')
						elseif (SHOW_CUSTOM_COLORS) then
							if (playerTable.chatColorOnlineString) and (string.len(playerTable.chatColorOnlineString) >= 1) and (playerTable.chatColorOnlineString ~= '#FFFFFF') then
								name:SetColor(playerTable.chatColorOnlineString)
								name:SetGlow(AtoB(playerTable.nameGlow))
								name:SetGlowColor(playerTable.nameGlowColor)
							else
								name:SetColor(DEFAULT_ONLINE_CUSTOM_COLOR)
								name:SetGlow(false)
							end
						else
							name:SetColor(DEFAULT_ONLINE_COLOR)
							name:SetGlow(false)
						end
						border:SetBorderColor('.2 .2 .2 1')
						bg_blue:SetVisible(true)
						bg_blue:UICmd("SetRenderMode('normal')")

						hover_bg_blue:SetVisible(true)
						hover_bg_green:SetVisible(false)
						hover_border:SetBorderColor('#00ed43')
						parent:SetHeight(social_playerItem_height)
					elseif (playerTable.status == 'ingame') then
						HoN_Social_Panel.userEntryWidgetTable[HoN_Social_Panel.userEntryIndex] = 'ingame'
						HoN_Social_Panel.userEntryPlayerTable[HoN_Social_Panel.userEntryIndex] = player
						full:SetVisible(true)
						icon:SetVisible(true)

						if (SHOW_CUSTOM_COLORS) then
							if (playerTable.chatColorIngameString) and (string.len(playerTable.chatColorIngameString) >= 1) then
								name:SetColor(playerTable.chatColorIngameString)
								name:SetGlow(AtoB(playerTable.nameGlow))
								if playerTable.nameGlowColorIngame then
									name:SetGlowColor(playerTable.nameGlowColorIngame)
								end
							else
								name:SetColor(DEFAULT_INGAME_CUSTOM_COLOR)
								name:SetGlow(false)
							end
						else
							name:SetColor(DEFAULT_INGAME_COLOR)
							name:SetGlow(false)
						end
						border:SetBorderColor('.2 .2 .2 1')
						bg_blue:SetVisible(true)
						bg_blue:UICmd("SetRenderMode('normal')")

						hover_bg_blue:SetVisible(true)
						hover_bg_green:SetVisible(false)
						hover_border:SetBorderColor('#0081ed')
						parent:SetHeight(social_playerItem_height)
					elseif (playerTable.status == 'offline') then
						HoN_Social_Panel.userEntryWidgetTable[HoN_Social_Panel.userEntryIndex] = 'offline'
						HoN_Social_Panel.userEntryPlayerTable[HoN_Social_Panel.userEntryIndex] = player
						full:SetVisible(true)
						icon:SetVisible(true)
						if IsFollowing(playerTable.name) then
							name:SetColor('#ff6d3c')
							name:SetGlow(false)
						else
							name:SetColor(DEFAULT_OFFLINE_COLOR)
							name:SetGlow(false)
						end
						border:SetBorderColor('0 0 0 1')
						bg_blue:SetVisible(true)
						bg_blue:UICmd("SetRenderMode('grayscale')")

						hover_bg_blue:SetVisible(true)
						hover_bg_green:SetVisible(false)
						hover_border:SetBorderColor('.2 .2 .2 1')
						parent:SetHeight(social_playerItem_height)
					elseif (playerTable.status == 'autocomplete') then
						HoN_Social_Panel.userEntryWidgetTable[HoN_Social_Panel.userEntryIndex] = 'autocomplete'
						HoN_Social_Panel.userEntryPlayerTable[HoN_Social_Panel.userEntryIndex] = player
						full:SetVisible(false)

						if (playerTable.isOnline) then
							name:SetColor(DEFAULT_AUTOCOMPLETE_COLOR)
							name:SetGlow(false)
						else
							name:SetColor(DEFAULT_AUTOCOMPLETE_COLOR_OFFLINE)
							name:SetGlow(false)
						end
						if (playerTable.isBuddy == nil) or (playerTable.isBuddy == true) then
							icon:SetVisible(false)
						else
							icon:SetVisible(true)
							icon:SetTexture('/ui/fe2/elements/social_addfriend.tga')
						end
						border:SetBorderColor('0 0 0 1')
						bg_blue:SetVisible(true)
						bg_blue:UICmd("SetRenderMode('grayscale')")
						hover_bg_blue:SetVisible(true)
						hover_bg_green:SetVisible(false)
						hover_border:SetBorderColor('.2 .2 .2 1')
						parent:SetHeight(social_playerHeader_height)
					elseif (playerTable.status == 'recent') then
						HoN_Social_Panel.userEntryWidgetTable[HoN_Social_Panel.userEntryIndex] = 'recent'
						HoN_Social_Panel.userEntryPlayerTable[HoN_Social_Panel.userEntryIndex] = player
						full:SetVisible(true)
						icon:SetVisible(true)
						if IsFollowing(playerTable.name) then
							name:SetColor('#ff6d3c')
							name:SetGlow(false)
						elseif (SHOW_CUSTOM_COLORS) then
							if (ChatUserOnline(playerTable.name)) then
								if (playerTable.chatColorOnlineString) and (string.len(playerTable.chatColorOnlineString) >= 1) and (playerTable.chatColorOnlineString ~= '#FFFFFF') then
									name:SetColor(playerTable.chatColorOnlineString)
									if (playerTable.nameGlow) then
										name:SetGlow(AtoB(playerTable.nameGlow))
									else
										name:SetGlow(false)
									end

									if (playerTable.nameGlowColor) then
										name:SetGlowColor(playerTable.nameGlowColor)
									else
										name:SetGlowColor("")
									end
								else
									name:SetColor(DEFAULT_ONLINE_CUSTOM_COLOR)
									name:SetGlow(false)
								end
							else
								name:SetColor(DEFAULT_OFFLINE_COLOR)
								name:SetGlow(false)
							end
						else
							if (ChatUserOnline(playerTable.name)) then
								name:SetColor(DEFAULT_ONLINE_COLOR)
							else
								name:SetColor(DEFAULT_OFFLINE_COLOR)
							end
							name:SetGlow(false)
						end
						border:SetBorderColor('.2 .2 .2 1')
						bg_blue:SetVisible(true)
						bg_blue:UICmd("SetRenderMode('grayscale')")
						hover_bg_blue:SetVisible(true)
						hover_bg_green:SetVisible(false)
						hover_border:SetBorderColor('.2 .2 .2 1')
						parent:SetHeight(social_playerItem_height)
					end
				end
			end
		end
		local leftMenu = GetWidget("sp_left_user_menu")
		if (not selectedVisible and not HoN_Social_Panel.leftMenuClosed) then
			leftMenu:DoEventN(3)
			HoN_Social_Panel.leftMenuClosed = true
			HoN_Social_Panel.selectedPlayerName = nil
		end
	end

	for i = HoN_Social_Panel.userEntryIndex + 1, MAX_USERENTRY_SLOTS do
		HoN_Social_Panel.userEntryWidgetTable[i] = 'none'
		local parent = interface:GetWidget('sp_player_item_'..i)
		local header = interface:GetWidget('sp_im_category_header_'..i)
		local addplayer = interface:GetWidget('sp_add_friend_item_'..i)
		if (parent) then
			parent:SetVisible(false)
			header:SetVisible(false)
			addplayer:SetVisible(false)
		else
			break
		end
	end
end

local function AddToGroupList(group)
	--println('^y group = ' .. tostring(group) )
	HoN_Social_Panel.groupTable[group] = HoN_Social_Panel.groupTable[group] or {
			name = group,
			tag = '',
			group = 'group',
			nameGlow = 'false',
			clanStatus = '',
			buddyGroup = '',
			source = 'group',
			status = 'group',
			verified = 'false',
			chatSymbolTexture = '',
			chatColorTexture = '',
			chatColorOnlineString = '',
			chatColorIngameString = '',
			accountIcon = '',
			focus = false,
			hide = false
			}
end

local function CreateNumericTable(numericPlayerTable, groupTable, group, entryLimit)
	if (groupTable) then
		local entryCount = 0
		local tempPlayerTable = {}

		if (USER_SORT_ONLINE_FIRST) then
			local onlineTable, ingameTable, offlineTable, alertTable = {}, {}, {}, {}
			for player,playerTable in pairs(groupTable) do
				if (playerTable.focus == true) then
					alertTable[playerTable.name] = playerTable
				elseif (playerTable.status == 'online') then
					onlineTable[playerTable.name] = playerTable
				elseif (playerTable.status == 'ingame') then
					ingameTable[playerTable.name] = playerTable
				else
					offlineTable[playerTable.name] = playerTable
				end
			end
			for player,playerTable in pairsByKeys(alertTable, function(a,b) return string.lower(a)<string.lower(b) end ) do
				if not ((HoN_Social_Panel.autoCompleteString) and (string.len(HoN_Social_Panel.autoCompleteString) >= 1 ))
				or (string.find(string.lower(StripClanTag(playerTable.name)), string.lower(StripClanTag(HoN_Social_Panel.autoCompleteString))))
				or ((playerTable.tag) and (playerTable.tag ~= "") and (string.find(string.lower(playerTable.tag), string.lower(HoN_Social_Panel.autoCompleteString)))) then
					if (group) and (HoN_Social_Panel.grouplist[group] == false) then
					else
						table.insert(tempPlayerTable, {playerTable = playerTable, header = false} )
						HoN_Social_Panel.totalPlayers = HoN_Social_Panel.totalPlayers + 1
						HoN_Social_Panel.totalEntries = HoN_Social_Panel.totalEntries + 1
					end
					entryCount = entryCount + 1
					if (entryLimit) and (entryCount >= entryLimit) then
						break
					end
				end
			end
			for player,playerTable in pairsByKeys(onlineTable, function(a,b) return string.lower(a)<string.lower(b) end ) do
				if not ((HoN_Social_Panel.autoCompleteString) and (string.len(HoN_Social_Panel.autoCompleteString) >= 1 ))
				or (string.find(string.lower(StripClanTag(playerTable.name)), string.lower(StripClanTag(HoN_Social_Panel.autoCompleteString))))
				or ((playerTable.tag) and (playerTable.tag ~= "") and (string.find(string.lower(playerTable.tag), string.lower(HoN_Social_Panel.autoCompleteString)))) then
					if (group) and (HoN_Social_Panel.grouplist[group] == false) then
					else
						table.insert(tempPlayerTable, {playerTable = playerTable, header = false} )
						HoN_Social_Panel.totalPlayers = HoN_Social_Panel.totalPlayers + 1
						HoN_Social_Panel.totalEntries = HoN_Social_Panel.totalEntries + 1
					end
					entryCount = entryCount + 1
					if (entryLimit) and (entryCount >= entryLimit) then
						break
					end
				end
			end
			for player,playerTable in pairsByKeys(ingameTable, function(a,b) return string.lower(a)<string.lower(b) end ) do
				if not ((HoN_Social_Panel.autoCompleteString) and (string.len(HoN_Social_Panel.autoCompleteString) >= 1 ))
				or (string.find(string.lower(StripClanTag(playerTable.name)), string.lower(StripClanTag(HoN_Social_Panel.autoCompleteString))))
				or ((playerTable.tag) and (playerTable.tag ~= "") and (string.find(string.lower(playerTable.tag), string.lower(HoN_Social_Panel.autoCompleteString)))) then
					if (group) and (HoN_Social_Panel.grouplist[group] == false) then
					else
						table.insert(tempPlayerTable, {playerTable = playerTable, header = false} )
						HoN_Social_Panel.totalPlayers = HoN_Social_Panel.totalPlayers + 1
						HoN_Social_Panel.totalEntries = HoN_Social_Panel.totalEntries + 1
					end
					entryCount = entryCount + 1
					if (entryLimit) and (entryCount >= entryLimit) then
						break
					end
				end
			end
			for player,playerTable in pairsByKeys(offlineTable, function(a,b) return string.lower(a)<string.lower(b) end ) do
				if not ((HoN_Social_Panel.autoCompleteString) and (string.len(HoN_Social_Panel.autoCompleteString) >= 1 ))
				or (string.find(string.lower(StripClanTag(playerTable.name)), string.lower(StripClanTag(HoN_Social_Panel.autoCompleteString))))
				or ((playerTable.tag) and (playerTable.tag ~= "") and (string.find(string.lower(playerTable.tag), string.lower(HoN_Social_Panel.autoCompleteString)))) then
					if (group) and (HoN_Social_Panel.grouplist[group] == false) then
					else
						table.insert(tempPlayerTable, {playerTable = playerTable, header = false} )
						HoN_Social_Panel.totalPlayers = HoN_Social_Panel.totalPlayers + 1
						HoN_Social_Panel.totalEntries = HoN_Social_Panel.totalEntries + 1
					end
					entryCount = entryCount + 1
					if (entryLimit) and (entryCount >= entryLimit) then
						break
					end
				end
			end
		else
			for player,playerTable in pairsByKeys(groupTable, function(a,b) return string.lower(a)<string.lower(b) end ) do
				if not ((HoN_Social_Panel.autoCompleteString) and (string.len(HoN_Social_Panel.autoCompleteString) >= 1 ))
				or (string.find(string.lower(StripClanTag(playerTable.name)), string.lower(StripClanTag(HoN_Social_Panel.autoCompleteString))))
				or ((playerTable.tag) and (playerTable.tag ~= "") and (string.find(string.lower(playerTable.tag), string.lower(HoN_Social_Panel.autoCompleteString)))) then
					if (group) and (HoN_Social_Panel.grouplist[group] == false) then
					else
						table.insert(tempPlayerTable, {playerTable = playerTable, header = false} )
						HoN_Social_Panel.totalPlayers = HoN_Social_Panel.totalPlayers + 1
						HoN_Social_Panel.totalEntries = HoN_Social_Panel.totalEntries + 1
					end
					entryCount = entryCount + 1
					if (entryLimit) and (entryCount >= entryLimit) then
						break
					end
				end
			end
		end
		if (entryCount > 0) then
			HoN_Social_Panel.totalEntries = HoN_Social_Panel.totalEntries + 1

			-- Sort the inactive group based on if they've been invited back yet (so invited players are at the bottom)
			-- if(group == 'inactive') then
				-- table.sort(tempPlayerTable,	function(a,b) return (HoN_Social_Panel.inactiveInvitedList[a.playerTable.name] or 0) < (HoN_Social_Panel.inactiveInvitedList[b.playerTable.name] or 0) end )
			-- end

			AddToGroupList(group)

			-- Populate the player table with the header and the players
			tinsert(numericPlayerTable, {group = group, header = true} )
			for _, v in ipairs(tempPlayerTable) do
				tinsert(numericPlayerTable, v)
			end
			tempPlayerTable = nil
		end
	end
	return numericPlayerTable
end

local function FilterTableByName(filterTable, searchString, offset)
	for i,v in pairs(filterTable) do
		if (v.playerTable) then
			if (string.find(string.lower(StripClanTag(v.playerTable.name)), string.lower(StripClanTag(searchString)))) then
				v.playerTable.hide = false
			else
				v.playerTable.hide = true
				HoN_Social_Panel.totalPlayers = HoN_Social_Panel.totalPlayers - 1
			end
		end
	end
	return filterTable, offset
end

local function TempResizeSocialPanel()
	local social_panel = interface:GetWidget('social_panel')
	local dragwidget = interface:GetWidget('social_panel_drag_widget')
	dragwidget:SetY(social_panel:GetHeight() - dragwidget:GetHeight())
	if GetCvarBool('ui_socialPanelCanDrag') then
		social_panel:UICmd("SetAbsoluteX("..db.x..")")
		social_panel:UICmd("SetAbsoluteY("..db.y..")")
	end
end

local function InteractionFooter(showFooter)
	local dragwidget = interface:GetWidget('social_panel_drag_widget')
	local footer_top = interface:GetWidget('sp_footer_top')
	local social_friends_userlist = interface:GetWidget('social_friends_userlist')
	local social_friends_panel = interface:GetWidget('social_friends_panel')

	if (showFooter) then
		--dragwidget:SetHeight('6.5h')
		footer_top:SetVisible(true)
		social_friends_userlist:SetHeight('-8.0h')
		social_friends_panel:SetY('3.0h')
		TempResizeSocialPanel()
	else
		--dragwidget:SetHeight('2.5h')
		footer_top:SetVisible(false)
		social_friends_userlist:SetHeight('-5.5h')
		social_friends_panel:SetY('0')
		TempResizeSocialPanel()
	end
end

local function ChatChangeBuddyGroup(nickname, buddyGroup)
	interface:UICmd("ChatChangeBuddyGroup('"..nickname.."', '"..string.gsub(string.gsub(buddyGroup, "\\", "\\\\"), "'", "\\'").."')")
end

local function AddUserToGroupYes()
	local create_group = interface:GetWidget('social_panel_create_group')
	create_group:FadeOut(50)
	if (HoN_Social_Panel.lastSelectedName) and (HoN_Social_Panel.joinGroupString) then
		ChatChangeBuddyGroup(HoN_Social_Panel.lastSelectedName, HoN_Social_Panel.joinGroupString)
	end
	delayFunction(3000, 'ChatRefresh', _G)
end

local function AddUserToGroupNo()
end

local function RemoveUserGroup()
	local create_group = interface:GetWidget('social_panel_create_group')
	create_group:FadeOut(50)
	if (HoN_Social_Panel.lastSelectedName) and (HoN_Social_Panel.joinGroupString) then
		ChatChangeBuddyGroup(HoN_Social_Panel.lastSelectedName, '')
	end
	delayFunction(3000, 'ChatRefresh', _G)
end

local function ResetBuddyListInput()
	--interface:GetWidget('sp_footer_input_cover'):SetVisible(false)
	--interface:GetWidget('sp_textbox_friendlist_input'):UICmd("EraseInputLine(); SetFocus(false);")
end

local function BuddyListBlueButtonClicked(input, button, linkedBtn, bigButtonOne)
	if (input == 1) then
		AreYouSureDialog(button, function() button:SetVisible(true) if (linkedBtn) then linkedBtn:SetVisible(true) end ChatRemoveBuddy(HoN_Social_Panel.autoCompleteString) ResetBuddyListInput() end, function() button:SetVisible(true) if (linkedBtn) then linkedBtn:SetVisible(true) end end, linkedBtn, bigButtonOne)
		button:SetVisible(false)
		if (linkedBtn) then linkedBtn:SetVisible(false) end
	elseif (input == 2) then
		AreYouSureDialog(button, function() button:SetVisible(true) if (linkedBtn) then linkedBtn:SetVisible(true) end ChatAddBuddy(HoN_Social_Panel.autoCompleteString) ResetBuddyListInput() end, function() button:SetVisible(true) if (linkedBtn) then linkedBtn:SetVisible(true) end end, linkedBtn, bigButtonOne)
		button:SetVisible(false)
		if (linkedBtn) then linkedBtn:SetVisible(false) end
	elseif (input == 3) then
		AreYouSureDialog(button, function() button:SetVisible(true) if (linkedBtn) then linkedBtn:SetVisible(true) end ChatRemoveClanMember(HoN_Social_Panel.autoCompleteString) ResetBuddyListInput() end, function() button:SetVisible(true) if (linkedBtn) then linkedBtn:SetVisible(true) end end, linkedBtn, true)
		button:SetVisible(false)
		if (linkedBtn) then linkedBtn:SetVisible(false) end
	elseif (input == 4) then
		AreYouSureDialog(button, function() button:SetVisible(true) if (linkedBtn) then linkedBtn:SetVisible(true) end ChatInviteUserToClan(HoN_Social_Panel.autoCompleteString) ResetBuddyListInput() end, function() button:SetVisible(true) if (linkedBtn) then linkedBtn:SetVisible(true) end end, linkedBtn, true)
		button:SetVisible(false)
		if (linkedBtn) then linkedBtn:SetVisible(false) end
	end
end

local function UpdateBuddyListBlueButton(input)
	local label_1 = interface:GetWidget('sp_main_dyn_btn_1_label')
	local button_1 = interface:GetWidget('sp_main_dyn_btn_1')
	local label_2 = interface:GetWidget('sp_main_dyn_btn_2_label')
	local button_2 = interface:GetWidget('sp_main_dyn_btn_2')

	if (HoN_Social_Panel.autoCompleteString) then
		local bigButtonOne = false

		if ChatUserOnline(HoN_Social_Panel.autoCompleteString) and IsClanMember(HoN_Social_Panel.autoCompleteString) and (ChatIsClanLeader(GetAccountName()) or ChatIsClanOfficer(GetAccountName())) and IsNotMe(HoN_Social_Panel.autoCompleteString) and (not ChatIsClanLeader(HoN_Social_Panel.autoCompleteString)) and (not ChatIsClanOfficer(HoN_Social_Panel.autoCompleteString)) then
			label_2:SetText(Translate('sp_main_dyn_btn_2b'))
			button_2:SetCallback('onclick', function() BuddyListBlueButtonClicked(3, button_2, button_1) end)
			button_2:SetVisible(true)
			button_1:SetWidth('46%')
		elseif ChatUserOnline(HoN_Social_Panel.autoCompleteString) and (ChatIsClanLeader(GetAccountName()) or ChatIsClanOfficer(GetAccountName())) and (not ChatUserIsInClan(HoN_Social_Panel.autoCompleteString)) and IsNotMe(HoN_Social_Panel.autoCompleteString) then
			label_2:SetText(Translate('sp_main_dyn_btn_2'))
			button_2:SetCallback('onclick', function() BuddyListBlueButtonClicked(4, button_2, button_1) end)
			button_2:SetVisible(true)
			button_1:SetWidth('46%')
		else
			label_2:SetText(Translate('sp_main_dyn_btn_3'))
			button_2:ClearCallback('onclick')
			button_2:SetVisible(false)
			button_1:SetWidth('92%')
			bigButtonOne = true
		end

		if IsBuddy(HoN_Social_Panel.autoCompleteString) and IsNotMe(HoN_Social_Panel.autoCompleteString) then
			label_1:SetText(Translate('sp_main_dyn_btn_1b'))
			button_1:SetCallback('onclick', function() BuddyListBlueButtonClicked(1, button_1, button_2, bigButtonOne) end)
		elseif (not IsBuddy(HoN_Social_Panel.autoCompleteString)) and IsNotMe(HoN_Social_Panel.autoCompleteString) then
			label_1:SetText(Translate('sp_main_dyn_btn_1'))
			button_1:SetCallback('onclick', function() BuddyListBlueButtonClicked(2, button_1, button_2, bigButtonOne) end)
		else
			label_1:SetText(Translate('sp_main_dyn_btn_3'))
			button_1:ClearCallback('onclick')
		end

	else
		label_1:SetText(Translate('sp_main_dyn_btn_3'))
		label_2:SetText(Translate('sp_main_dyn_btn_3'))
		button_1:SetCallback('onclick', function() BuddyListBlueButtonClicked(0, button_1) end)
		button_2:SetCallback('onclick', function() BuddyListBlueButtonClicked(0, button_2) end)
		button_1:SetWidth('46%')
	end

	button_2:SetVisible(false)

end

local function CreateGroupButtonClicked(input, button)
	if (input == 1) then
		AreYouSureDialog(button, function() button:SetVisible(true) AddUserToGroupYes() end, function() button:SetVisible(true) AddUserToGroupNo() end)
		button:SetVisible(false)
	elseif (input == 2) then
		AreYouSureDialog(button, function() button:SetVisible(true) AddUserToGroupYes() end, function() button:SetVisible(true) AddUserToGroupNo() end)
		button:SetVisible(false)
	elseif (input == 3) then
		AreYouSureDialog(button, function() button:SetVisible(true) RemoveUserGroup() end, function() button:SetVisible(true) AddUserToGroupNo() end)
		button:SetVisible(false)
	else
	end
end

local function UpdateCreateGroupButton(input)
	local label = interface:GetWidget('sp_group_dyn_label')
	local button = interface:GetWidget('sp_group_dyn_button')

	if (HoN_Social_Panel.joinGroupString) then
		if (HoN_Social_Panel.joinGroupString == 'remove group' ) then
			label:SetText(Translate('social_remove_from_group'))
			button:SetCallback('onclick', function() CreateGroupButtonClicked(3, button) end)
		elseif (HoN_Social_Panel.groupTable[HoN_Social_Panel.joinGroupString]) then
			label:SetText(Translate('social_add_to_group'))
			button:SetCallback('onclick', function() CreateGroupButtonClicked(2, button) end)
		else
			label:SetText(Translate('social_create_new_group'))
			button:SetCallback('onclick', function() CreateGroupButtonClicked(1, button) end)
		end
	else
		if (interface:GetWidget('sp_group_input_cover'):IsVisible()) then
			label:SetText(Translate('social_select_a_group'))
		else
			label:SetText(Translate('social_enter_new_group'))
		end
		button:SetCallback('onclick', function() CreateGroupButtonClicked(0, button) end)
	end
end

local function ShowGroupCreationPanel()
	interface:GetWidget('social_panel_create_group'):FadeIn(150)
	UpdateGroupListbox()
end

function HoN_Social_Panel:GroupListSelectInput(input)
	if (input) and ( string.len(input) > 0 ) then
		interface:GetWidget('sp_group_input_cover'):SetVisible(false)
		interface:GetWidget('sp_textbox_group_input'):SetInputLine(input)
	end
end

function HoN_Social_Panel:GroupInputBoxUpdate(input)
	if (input) then
		if ( string.len(input) == 0 ) then
			HoN_Social_Panel.joinGroupString = nil
		else
			HoN_Social_Panel.joinGroupString = input
		end
	end
	UpdateCreateGroupButton(input)
end

function HoN_Social_Panel:InputBoxUpdate(input)
	if (input and input ~= "") then
		input = string.gsub(string.gsub(string.gsub(string.gsub(string.gsub(string.gsub(input, "%[", ""), "%%", ""), "%(", ""), "%]", ""), "%)", ""), "'", "")
	end

	if (input) then
		local length = string.len(input)
		if (length == 0 ) then
			HoN_Social_Panel.autoCompleteString = nil
			HoN_Social_Panel.autoCompleteTable = {}
			Trigger('ChatAutoCompleteClear')
			delayFunction(150, 'PopulateFriendsList', HoN_Social_Panel)
			InteractionFooter(false)
		else
			HoN_Social_Panel.autoCompleteString = input
			if (length >= 5 ) then
				HoN_Social_Panel.autoCompleteTable = {}
				Trigger('ChatAutoCompleteClear')
				Trigger('ChatAutoCompleteAdd', input)
				if GetWidget('sp_textbox_friendlist_input'):IsVisible() then
					GetWidget('sp_textbox_friendlist_input'):Sleep(650, function()
						interface:UICmd("ChatAutoCompleteNick('"..input.."')")
					end)
				end
			elseif (length >= 1 ) then
				HoN_Social_Panel.autoCompleteTable = {}
				Trigger('ChatAutoCompleteClear')
				Trigger('ChatAutoCompleteAdd', input)
				delayFunction(150, 'PopulateFriendsList', HoN_Social_Panel)
			else
				HoN_Social_Panel.autoCompleteTable = {}
				Trigger('ChatAutoCompleteClear')
				delayFunction(150, 'PopulateFriendsList', HoN_Social_Panel)
			end
			InteractionFooter(false)
		end
	end
	--UpdateBuddyListBlueButton(input) -- RMM removed because incomplete/buggy
end

function HoN_Social_Panel:PopulateFriendsList(offset)
	if (interface:GetWidget('social_panel') == nil) then return end

	HoN_Social_Panel.totalPlayers = 0
	HoN_Social_Panel.totalEntries = 0
	HoN_Social_Panel.entryTable, HoN_Social_Panel.headerTable = {}, {}
	local usingSearch = false
	local numericPlayerTable = {}
	local listbox = interface:GetWidget('social_friends_listbox')
	local offset = offset or HoN_Social_Panel.lastUserOffset or 0

	---[[ Load the invited inactive players for this account
	HoN_Social_Panel.inactiveInvitedList = GetDBEntry('hon_inactive_invited_friends_'..GetAccountName(), nil, false, false, false) or {}
	--]]

	--[[ Add alerts as first priority
	if (#HoN_Social_Panel.alertTable) >= 1 then
		numericPlayerTable = CreateNumericTable(numericPlayerTable, HoN_Social_Panel.alertTable, 'alert')
	end
	--]]

	--[[ Add temporary 'fake' groups to allow the creation of real ones
	for group,groupTable in pairs(HoN_Social_Panel.tempGroupTable) do
		if (not HoN_Social_Panel.friendslist[group]) then
			AddToGroupList(group)
			table.insert(numericPlayerTable, {group = group, header = true} )
		end
	end
	--]]

	---[[ Add custom groups
	for group,groupTable in pairs(HoN_Social_Panel.friendslist) do
		if (group ~= 'friends') and (group ~= 'clan') and (group ~= 'cafe') and (group ~= 'offline') and (group ~= 'inactive') and (group ~= 'add_friend') and (group ~= 'recent') then
			numericPlayerTable = CreateNumericTable(numericPlayerTable, HoN_Social_Panel.friendslist[group], group)
		end
	end
	--]]

	---[[ Add default groups
	if (MERGE_CLAN_FRIENDS) then
		numericPlayerTable = CreateNumericTable(numericPlayerTable, HoN_Social_Panel.friendslist['friends'], 'friends')
	else
		numericPlayerTable = CreateNumericTable(numericPlayerTable, HoN_Social_Panel.friendslist['friends'], 'friends')
		numericPlayerTable = CreateNumericTable(numericPlayerTable, HoN_Social_Panel.friendslist['clan'], 'clan')
	end

	numericPlayerTable = CreateNumericTable(numericPlayerTable, HoN_Social_Panel.friendslist['cafe'], 'cafe')

	numericPlayerTable = CreateNumericTable(numericPlayerTable, HoN_Social_Panel.friendslist['recent'], 'recent')

	numericPlayerTable = CreateNumericTable(numericPlayerTable, HoN_Social_Panel.friendslist['add_friend'], 'add_friend')

	numericPlayerTable = CreateNumericTable(numericPlayerTable, HoN_Social_Panel.friendslist['inactive'], 'inactive')

	numericPlayerTable = CreateNumericTable(numericPlayerTable, HoN_Social_Panel.friendslist['offline'], 'offline')
	--]]

	---[[ Filter all user groups if search string present
	if (HoN_Social_Panel.autoCompleteString) and (string.len(HoN_Social_Panel.autoCompleteString) >= 1 ) and ((#numericPlayerTable) >= 1) then
		usingSearch = true
		--numericPlayerTable, offset = FilterTableByName(numericPlayerTable, HoN_Social_Panel.autoCompleteString, offset)
	else
		usingSearch = false
	end
	--]]

	--[[ Add group 'group' if requested
	if (showGroupList) then
		--printTable(HoN_Social_Panel.groupTable)
		numericPlayerTable = CreateNumericTable(numericPlayerTable, HoN_Social_Panel.groupTable, 'group')
	end
	--]]

	---[[ Add autocomplete 'group' as lowest priority
	if (#HoN_Social_Panel.autoCompleteTable) >= 1 then
		--printTable(HoN_Social_Panel.autoCompleteTable)
		numericPlayerTable = CreateNumericTable(numericPlayerTable, HoN_Social_Panel.autoCompleteTable, 'autocomplete', nil)
	end
	--]]

	-- Update actual UI userlist widgets
	if (#numericPlayerTable) >= 1 then
		-- println('^y -------------- ')
		-- for i, v in ipairs(numericPlayerTable) do
			-- println('^y  i = ' .. tostring(i) )
			-- printTable(v)
		-- end

		TransferFriendsToUI(numericPlayerTable, offset, usingSearch)
	end

	-- Update UI scroll area and scrollbar
	interface:GetWidget('social_friends_listbox'):UICmd("SetClipAreaToChild();")
	HoN_Social_Panel:UpdateUserScroller(offset, usingSearch)
	HoN_Social_Panel.lockBuddyTable = false
end

function HoN_Social_Panel:PopulateUserlist(scrollOffset)
	local currentUserLine = scrollOffset or 0
	HoN_Social_Panel.userOffset = currentUserLine - HoN_Social_Panel.visibleUserEntrySlots
	if HoN_Social_Panel.userOffset < 0 then HoN_Social_Panel.userOffset = 0 end
	HoN_Social_Panel:UpdateVisibleSlots(HoN_Social_Panel.userOffset)
end

local function AddPlayerToInvitedInactiveList(sourceWidget, player)
	HoN_Social_Panel.inactiveInvitedList[player] = 1
	HoN_Social_Panel.inactiveInvitedList =  GetDBEntry('hon_inactive_invited_friends_'..GetAccountName(), HoN_Social_Panel.inactiveInvitedList, true, false, false) or {}
end
interface:RegisterWatch('social_panel_inactive_invited', AddPlayerToInvitedInactiveList)

function HoN_Social_Panel:IsPlayerNew(name)
	-- println('is new: ' .. name)
	-- printTable(newPlayerTable)
	name = string.lower(StripClanTag(name))
	if (HoN_Social_Panel.newPlayerTable) and (HoN_Social_Panel.newPlayerTable[name]) then
		return true
	else
		return false
	end
end

function HoN_Social_Panel:UpdateVisibleSlots(tempUserOffset)
	local listbox = interface:GetWidget('social_friends_listbox')
	if (listbox) then
		---[[ Using Fixed Height for 3.0
		local visibleHeaders, visibleMini = 0, 0
		for index, slotType in ipairs(HoN_Social_Panel.userEntryWidgetTable) do
			if slotType == 'header' then
				visibleHeaders = visibleHeaders + 1
			elseif slotType == 'autocomplete' then
				visibleMini = visibleMini + 1
			end
		end
		HoN_Social_Panel.visibleUserEntrySlots = math.ceil( (  (listbox:GetHeight() + (listbox:GetHeightFromString('2.2h') * visibleHeaders) + (listbox:GetHeightFromString('2.0h') * visibleMini) ) / (listbox:GetHeightFromString(social_playerItem_height) + listbox:GetHeightFromString('0.2h') )   ) + 1)
		--]]
		HoN_Social_Panel.visibleUserEntrySlots = MAX_USERENTRY_SLOTS

		if not (HoN_Social_Panel.lockBuddyList) then
			HoN_Social_Panel.lockBuddyList = true
			HoN_Social_Panel:PopulateFriendsList(tempUserOffset or 0)
		end
	end
end

local RMMCount = 0
local function AddToFriendsList(name, source, gameStatus, verified, chatSymbolTexture, chatColorTexture, chatColorOnlineString, chatColorIngameString, accountIcon, clanStatus, nameGlow, buddyGroup, isAccountNew, isAccountInactive, nameGlowColor, nameGlowColorIngame, ascensionLevel)
	isAccountInactive = "false"

	local tag, group  = '', 'offline'

	if (string.find(name, ']')) then
		tag = string.sub(name, 1, string.find(name, ']'))
		name = string.sub(name, string.find(name, ']') + 1)
	end

	if (name == HoN_Social_Panel.selectedPlayerName and gameStatus == "offline") then
		-- the selected player logged off, close the menu for them
		HoN_Social_Panel:ClosedLeftMenu()
	end

	if ((gameStatus ~= 'offline') or (OFFLINE_IN_GROUPS)) or IGNORE_OFFLINE then
		group  = interface:UICmd("GetBuddyGroup('"..name.."')") or ''	 -- group  = buddyGroup or ''
		if (string.len(group) <= 1) then
			group = source
		end
	elseif (HoN_Social_Panel.alertCount <= 2) and (isAccountInactive) and AtoB(isAccountInactive) and HoN_Social_Panel.inactiveInvitedList[name] ~= 1 then
		HoN_Social_Panel.alertCount = HoN_Social_Panel.alertCount + 1
		group = 'inactive'
	else
		group = 'offline'
	end

	if (isAccountNew) and AtoB(isAccountNew) then
		HoN_Social_Panel.newPlayerTable = HoN_Social_Panel.newPlayerTable or {}
		HoN_Social_Panel.newPlayerTable[string.lower(StripClanTag(name))] = true
	end

	if (string.len(group) >= 1) then
		if (MERGE_CLAN_FRIENDS) and (group == 'clan') then group = 'friends' end
		--HoN_Social_Panel.grouplist[group] = true
		HoN_Social_Panel.friendslist[group] = HoN_Social_Panel.friendslist[group] or {}

		if (HoN_Social_Panel.inviteTable[name]) then
			HoN_Social_Panel.friendslist[group][name] = {
				name = name or '',
				tag = tag or '',
				group = group or '',
				nameGlow = nameGlow or 'false',
				clanStatus = clanStatus or '',
				buddyGroup = buddyGroup or '',
				source = source or '',
				status = gameStatus or 'ingame',
				verified = verified or 'false',
				chatSymbolTexture = chatSymbolTexture or '',
				chatColorTexture = chatColorTexture or '',
				chatColorOnlineString = chatColorOnlineString or '',
				chatColorIngameString = chatColorIngameString or '',
				accountIcon = accountIcon or '',
				--accountIcon = '/ui/fe2/systembar/icons/ims.tga',
				--accountIconColour = '#dc0000',
				focus = true,
				focusIcon = '/ui/fe2/systembar/icons/motd.tga',
				hide = false,
				isAccountNew = isAccountNew or nil,
				isAccountInactive = isAccountInactive or nil,
				nameGlowColor = nameGlowColor or '',
				nameGlowColorIngame = nameGlowColorIngame or '',
				ascensionLevel = ascensionLevel or '0'
			}
		elseif (HoN_Social_Panel.alertTable[name]) then
			HoN_Social_Panel.friendslist[group][name] = {
				name = name or '',
				tag = tag or '',
				group = group or '',
				nameGlow = nameGlow or 'false',
				clanStatus = clanStatus or '',
				buddyGroup = buddyGroup or '',
				source = source or '',
				status = gameStatus or 'ingame',
				verified = verified or 'false',
				chatSymbolTexture = chatSymbolTexture or '',
				chatColorTexture = chatColorTexture or '',
				chatColorOnlineString = chatColorOnlineString or '',
				chatColorIngameString = chatColorIngameString or '',
				accountIcon = accountIcon or '',
				--accountIcon = '/ui/fe2/systembar/icons/ims.tga',
				--accountIconColour = '#dc0000',
				focus = true,
				focusIcon = '/ui/fe2/systembar/icons/ims.tga',
				hide = false,
				isAccountNew = isAccountNew or nil,
				isAccountInactive = isAccountInactive or nil,
				nameGlowColor = nameGlowColor or '',
				nameGlowColorIngame = nameGlowColorIngame or '',
				ascensionLevel = ascensionLevel or '0'
			}
		else
			HoN_Social_Panel.friendslist[group][name] = {
				name = name or '',
				tag = tag or '',
				group = group or '',
				nameGlow = nameGlow or 'false',
				clanStatus = clanStatus or '',
				buddyGroup = buddyGroup or '',
				source = source or '',
				status = gameStatus or 'ingame',
				verified = verified or 'false',
				chatSymbolTexture = chatSymbolTexture or '',
				chatColorTexture = chatColorTexture or '',
				chatColorOnlineString = chatColorOnlineString or '',
				chatColorIngameString = chatColorIngameString or '',
				accountIcon = accountIcon or '',
				focus = focus or false,
				hide = false,
				isAccountNew = isAccountNew or nil,
				isAccountInactive = isAccountInactive or nil,
				nameGlowColor = nameGlowColor or '',
				nameGlowColorIngame = nameGlowColorIngame or '',
				ascensionLevel = ascensionLevel or '0'
			}
		end
	end
end

function HoN_Social_Panel:SocialPanelOnShow(self)
	interface:GetWidget('social_friends_listbox'):DoEvent()
end

function HoN_Social_Panel:SocialPanelOnHide(self)
	interface:GetWidget('social_panel_create_group'):SetVisible(false)
end

function HoN_Social_Panel:SocialPanelOnLoad(self)
	delayFunction(3000, 'ChatRefresh', _G)
end

function interface:HoNSocialPanelF(func, ...)
  print(HoN_Social_Panel[func](self, ...))
end

local function SetAddonPosition()
	-- local screenWidth = tonumber(interface:UICmd("GetScreenWidth()"))
	-- local screenHeight = tonumber(interface:UICmd("GetScreenHeight()"))
	local social_panel_addons_1 = interface:GetWidget('social_panel_addons_1')
	local social_panel_addons_2 = interface:GetWidget('social_panel_addons_2')
	-- if (db.x) > (screenWidth / 2) then
		social_panel_addons_1:SetX('-38.2h')
		social_panel_addons_2:SetX('-23.6h')
	-- else
		-- social_panel_addons_1:SetX('26.2h')
		-- social_panel_addons_2:SetX('26.2h')
	-- end
end

local function LoadLocationFromDB()
	local social_panel = interface:GetWidget('social_panel')
	social_panel:UICmd("SetAbsoluteX("..db.x..")")
	social_panel:UICmd("SetAbsoluteY("..db.y..")")
end

local function SaveLocation()
	local social_panel = interface:GetWidget('social_panel')
	if (social_panel) then
		db.x = social_panel:GetX()
		db.y = social_panel:GetY()

		local screenWidth = tonumber(interface:UICmd("GetScreenWidth()"))
		local screenHeight = tonumber(interface:UICmd("GetScreenHeight()"))

		if (db.x < 0 + social_panel:GetWidthFromString('1') ) then
			db.x = 0 + social_panel:GetWidthFromString('1')
			LoadLocationFromDB()
		elseif (db.x > (screenWidth - social_panel:GetWidth()  - social_panel:GetWidthFromString('1')) ) then
			db.x = (screenWidth - social_panel:GetWidth() - social_panel:GetWidthFromString('1') )
			LoadLocationFromDB()
		end

		if (db.y < 0 + social_panel:GetHeightFromString('2.6h') ) then
			db.y = 0 + social_panel:GetHeightFromString('2.6h')
			LoadLocationFromDB()
		elseif (db.y > (screenHeight - social_panel:GetHeight() - social_panel:GetHeightFromString('0.8h')) ) then
			db.y = (screenHeight - social_panel:GetHeight() - social_panel:GetHeightFromString('0.8h'))
			LoadLocationFromDB()
		end

		db.height = social_panel:GetHeight()
		db.width = social_panel:GetWidth()
		db:Flush()
		SetAddonPosition()
		--print('^g db.x: '.. db.x .. '\n')
		--print('^g db.y: '.. db.y .. '\n')
		--print('^g db.height: '.. db.height .. '\n')
		--print('^g db.width: '.. db.width .. '\n')
	else
		--print('^r NOT Saving location \n')
	end
end

local function LoadLocation()
	local social_panel = interface:GetWidget('social_panel')
	local dragwidget = interface:GetWidget('social_panel_drag_widget')
	if (social_panel) then
		if (db.x == nil) then
			--print('^r Load fail cuz no DB \n')
			SaveLocation()
		else
			--print('^g Loading location from DB \n')
			--print('^g db.x: '.. db.x .. '\n')
			--print('^g db.y: '.. db.y .. '\n')
			--print('^g db.height: '.. db.height .. '\n')
			--print('^g db.width: '.. db.width .. '\n')

			social_panel:SetHeight(db.height)
			social_panel:SetWidth(db.width)
			social_panel:UICmd("SetAbsoluteX("..db.x..")")
			social_panel:UICmd("SetAbsoluteY("..db.y..")")
			dragwidget:SetY(social_panel:GetHeight() - dragwidget:GetYFromString('4h'))
			SetAddonPosition()
		end
	end
end

-- Need to load position from DB on drag handle release and swap to using lua onframe
local function SocialMPDragged (_, dragState, parentState)
	if (parentState == '1' ) then
		SaveLocation()
	elseif (dragState == '1' ) then
		local social_panel = interface:GetWidget('social_panel')
		interface:GetWidget('social_friends_listbox'):SetNoClick(true)
		interface:GetWidget('social_panel'):SetNoClick(true)
		local groupTable = interface:GetGroup('sp_player_items')
		for index, widget in ipairs(groupTable) do
			widget:SetNoClick(true)
		end
		if (db.x) then
			social_panel:UICmd("SetAbsoluteX("..db.x..")")
			social_panel:UICmd("SetAbsoluteY("..db.y..")")
		end
	elseif (dragState == '0' ) then
		interface:GetWidget('social_friends_listbox'):SetNoClick(false)
		interface:GetWidget('social_panel'):SetNoClick(false)
		local groupTable = interface:GetGroup('sp_player_items')
		for index, widget in ipairs(groupTable) do
			widget:SetNoClick(false)
		end
		local social_panel = interface:GetWidget('social_panel')
		local dragwidget = interface:GetWidget('social_panel_drag_widget')

		-- if GetCvarBool('ui_socialPanelCanDrag') then -- Height Snapping
			-- social_panel:SetHeight(
				-- social_panel:GetHeightFromString(social_mainPanel_extraSpace) +
				-- (
					-- social_panel:GetHeight() -
					-- ( social_panel:GetHeight() % social_panel:GetHeightFromString(social_playerItem_height) )
				-- )
			-- );
		-- end

		dragwidget:SetY(social_panel:GetHeight() - dragwidget:GetHeight())

		if (db.x) then
			social_panel:UICmd("SetAbsoluteX("..db.x..")")
			social_panel:UICmd("SetAbsoluteY("..db.y..")")
		end

		SaveLocation()
	end
end

local function Initialize()
	if not (HoN_Social_Panel.widgetsInitialized) then
		HoN_Social_Panel.widgetsInitialized = true
		if (LOAD_DEFAULTS) then
			if GetCvarBool('ui_socialPanelCanDrag') then
				SaveLocation()
			end
			SaveOptions()
		else
			if GetCvarBool('ui_socialPanelCanDrag') then
				LoadLocation()
			end
			LoadOptions()
		end

		local dragWidget = interface:GetWidget('social_panel_drag_widget')
		local social_panel = interface:GetWidget('social_panel')

		if GetCvarBool('ui_socialPanelCanDrag') then
			dragWidget.onevent = function()
				if (dragWidget:GetY() < dragWidget:GetYFromString(social_mainPanel_minHeight) ) then
					dragWidget:SetY(social_mainPanel_minHeight)
				elseif (dragWidget:GetY() > dragWidget:GetYFromString(social_mainPanel_maxHeight) ) then
					dragWidget:SetY(social_mainPanel_maxHeight)
				else
					dragWidget:SetY(dragWidget:GetY())
				end
				social_panel:SetHeight( dragWidget:GetY() + dragWidget:GetHeight() )
				social_panel:UICmd("SetAbsoluteX("..db.x..")")
				social_panel:UICmd("SetAbsoluteY("..db.y..")")
			end

			dragWidget:RefreshCallbacks()
		end
	end
end

-- recently played stuff
local function AddRecentlyPlayed(_, ...)
	if (AtoB(arg[85]) or 		-- is bot
		(arg[2] == "") or 		-- is empty
		(arg[2] == "?????") or 	-- is someone loading or something
		IsMe(arg[2]) or			-- is self
		IsClanMember(arg[2]) or -- is clan
		IsBuddy(arg[2])) 		-- is buddy
	then
		return
	end

	if (not HoN_Social_Panel.friendslist['recent']) then
		HoN_Social_Panel.friendslist['recent'] = GetDBEntry('hon_recently_played_'..GetAccountName(), nil, false, false, false) or {}
	end

	-- get the info for the player
	chatInfoTable = Explode("|", GetChatClientInfo(arg[2], "chatsymbol|chatnamecolortexturepath|chatnamecolorstring|chatnamecoloringamestring|getaccounticontexturepath|chatnameglow|chatnameglowcolorstring|chatnameglowcoloringamestring"))

	local nameGlow = "false"
	if (chatInfoTable[6] and NotEmpty(chatInfoTable[6])) then
		nameGlow = chatInfoTable[6]
	end

	local nameGlowColor = ""
	if (chatInfoTable[7] and NotEmpty(chatInfoTable[7])) then
		nameGlowColor = chatInfoTable[7]
	end

	local nameGlowColorIngame = ""
	if (chatInfoTable[8] and NotEmpty(chatInfoTable[8])) then
		nameGlowColorIngame = chatInfoTable[8]
	end

	-- add to the table
	local player = {
		name = arg[2],
		tag = '',
		group = 'recent',
		nameGlow = nameGlow,
		clanStatus = '',
		buddyGroup = '',
		source = 'recent',
		status = 'recent',
		verified = 'false',
		chatSymbolTexture = chatInfoTable[1],
		chatColorTexture = chatInfoTable[2],
		chatColorOnlineString = chatInfoTable[3],
		chatColorIngameString = chatInfoTable[4],
		accountIcon = chatInfoTable[5],
		isOnline = ChatUserOnline(name),
		isBuddy = IsBuddy(name),
		focus = false,
		hide = false,
		nameGlowColor = nameGlowColor,
		nameGlowColorIngame = nameGlowColorIngame,
		ascensionLevel = '0',
	}

	table.insert(HoN_Social_Panel.friendslist['recent'], 1, player)

	-- search for dupes and remove them (we want to reinsert them at the top even if they are a dupe)
	local search = 2
	while (HoN_Social_Panel.friendslist['recent'][search]) do
		if HoN_Social_Panel.friendslist['recent'][search].name and HoN_Social_Panel.friendslist['recent'][search].name == arg[2] then
			table.remove(HoN_Social_Panel.friendslist['recent'], search)
		else
			search = search + 1
		end
	end

	-- prune the table
	local numRecent = #HoN_Social_Panel.friendslist['recent']
	while (numRecent > MAX_RECENTLY_PLAYED) do
		table.remove(HoN_Social_Panel.friendslist['recent'], numRecent)
		numRecent = numRecent - 1
	end

	-- save the table
	GetDBEntry('hon_recently_played_'..GetAccountName(), HoN_Social_Panel.friendslist['recent'], true, false, false)

	-- update the list
	HoN_Social_Panel:UpdateVisibleSlots(HoN_Social_Panel.lastUserOffset or 0)
end
for i=0, 9 do triggerHelper:RegisterWatch("LobbyPlayerInfo"..i, AddRecentlyPlayed) end

local function ChatCompanion (_, name, isVerified, chatSymbolTexture, chatColorTexture, chatColorOnlineString, chatColorIngameString, accountIcon, nameGlow, buddyGroup, isOnline, isInGame, isBuddy, inClan, isOfficer, isleader, isAccountNew, isAccountInactive, nameGlowColor, nameGlowColorIngame, ascensionLevel, isCafePlayer)
	local source, status = 'friends', 'offline'

	if (isInGame) and (isInGame == 'true')  then
		status = 'ingame'
		HoN_Social_Panel.onlineCount = HoN_Social_Panel.onlineCount + 1
	elseif (isOnline) and (isOnline == 'true') then
		status = 'online'
		HoN_Social_Panel.onlineCount = HoN_Social_Panel.onlineCount + 1
	else
		status = 'offline'
		HoN_Social_Panel.offlineCount = HoN_Social_Panel.offlineCount + 1
	end

	if (string.len(name) >= 1) then

		local isFriends = isBuddy and isBuddy == 'true'
		local isClan = (inClan == 'true')

		if isFriends or isClan or isCafePlayer then
			if isFriends then
				AddToFriendsList(name, 'friends', status, isVerified, chatSymbolTexture, chatColorTexture, chatColorOnlineString, chatColorIngameString, accountIcon, clanStatus, nameGlow, buddyGroup, isAccountNew, isAccountInactive, nameGlowColor, nameGlowColorIngame, ascensionLevel)
			end
			if isClan then
				AddToFriendsList(name, 'clan', status, isVerified, chatSymbolTexture, chatColorTexture, chatColorOnlineString, chatColorIngameString, accountIcon, clanStatus, nameGlow, buddyGroup, isAccountNew, isAccountInactive, nameGlowColor, nameGlowColorIngame, ascensionLevel)
			end
			if AtoB(isCafePlayer) then
				AddToFriendsList(name, 'cafe', status, isVerified, chatSymbolTexture, chatColorTexture, chatColorOnlineString, chatColorIngameString, accountIcon, clanStatus, nameGlow, buddyGroup, isAccountNew, isAccountInactive, nameGlowColor, nameGlowColorIngame, ascensionLevel)
			end
		else
			AddToFriendsList(name, source, status, isVerified, chatSymbolTexture, chatColorTexture, chatColorOnlineString, chatColorIngameString, accountIcon, clanStatus, nameGlow, buddyGroup, isAccountNew, isAccountInactive, nameGlowColor, nameGlowColorIngame, ascensionLevel)
		end
	end
end

local function ChatCompanionEvent (_, isStart)
	if not (HoN_Social_Panel.lockBuddyTable) then
		if (isStart == 'true') then
			-- Clear table
			HoN_Social_Panel.friendslist = {}
			HoN_Social_Panel.newPlayerTable = {}
			HoN_Social_Panel.alertTable = {}
			HoN_Social_Panel.inviteTable = {}
			HoN_Social_Panel.offlineCount, HoN_Social_Panel.onlineCount, HoN_Social_Panel.alertCount = 0, 0, 0
			HoN_Social_Panel.friendslist['add_friend'] = {}
			HoN_Social_Panel.friendslist['add_friend']['add_friend'] = {
				name = 'add_friend',
				tag = 'add_friend',
				group = 'add_friend',
				nameGlow = 'false',
				nameGlowColor = '',
				nameGlowColorIngame = '',
				clanStatus = 'add_friend',
				buddyGroup = 'add_friend',
				source = 'add_friend',
				status = 'add_friend',
				verified = 'false',
				chatSymbolTexture = '',
				chatColorTexture = '',
				chatColorOnlineString = '',
				chatColorIngameString = '',
				accountIcon = accountIcon or '',
				focus = false,
				focusIcon = '',
				hide = false,
				isAccountNew = isAccountNew or nil,
				isAccountInactive = false
			}
			-- load the recently played entries
			HoN_Social_Panel.friendslist['recent'] = GetDBEntry('hon_recently_played_'..GetAccountName(), nil, false, false, false) or {}
		else
			HoN_Social_Panel.lockBuddyTable = true
			Initialize()
			HoN_Social_Panel:UpdateVisibleSlots(HoN_Social_Panel.lastUserOffset or 0)
			--delayFunction(50, 'UpdateVisibleSlots', HoN_Social_Panel, HoN_Social_Panel.lastUserOffset or 0)
		end
	end
end

local function ChatAutoCompleteClear(self)
	HoN_Social_Panel.autoCompleteTable = {}
end

local function ChatAutoCompleteAdd(self, name)

	tinsert(HoN_Social_Panel.autoCompleteTable, {
			name = name,
			tag = '',
			group = 'autocomplete',
			nameGlow = 'false',
			clanStatus = '',
			buddyGroup = '',
			source = 'autocomplete',
			status = 'autocomplete',
			verified = 'false',
			chatSymbolTexture = '',
			chatColorTexture = '',
			chatColorOnlineString = '',
			chatColorIngameString = '',
			accountIcon = '',
			isOnline = ChatUserOnline(name),
			isBuddy = IsBuddy(name),
			focus = false,
			hide = false
			}
			)
	delayFunction(50, 'PopulateFriendsList', HoN_Social_Panel)
end

local function ChatNotificationsClear(self)
	HoN_Social_Panel.notificationsTable = {}
end

local function ChatNoficationAdd(self, name)
	tinsert(HoN_Social_Panel.notificationsTable, {
			name = name,
			tag = '',
			group = 'notification',
			nameGlow = 'false',
			clanStatus = '',
			buddyGroup = '',
			source = 'autocomplete',
			status = 'autocomplete',
			verified = 'false',
			chatSymbolTexture = '',
			chatColorTexture = '',
			chatColorOnlineString = '',
			chatColorIngameString = '',
			accountIcon = '',
			focus = false,
			hide = false,
			notification = true
			}
			)
	delayFunction(50, 'PopulateFriendsList', HoN_Social_Panel)
end

local function ChatBuddyStatusChanged(_, name, newStatus, oldStatus)
	oldStatus = tonumber(oldStatus)
	newStatus = tonumber(newStatus)

	local Disconnected = 0
	local online = 1
	local ingame = 4

	if (string.find(name, ']')) then
		local tag = string.sub(name, 1, string.find(name, ']'))
		name = string.sub(name, string.find(name, ']') + 1)
	end

	local oldGroup = ""
	if oldStatus == Disconnected and not OFFLINE_IN_GROUPS then
		oldGroup = "offline"
	else
		local group = interface:UICmd("GetBuddyGroup('"..name.."')") or ''
		if string.len(group) > 1 and HoN_Social_Panel.friendslist[group] ~= nil then
			oldGroup = group
		else
			oldGroup = "friends"
		end
		if MERGE_CLAN_FRIENDS and oldGroup == "clan" then
			oldGroup = "friends"
		end
	end
	local newGroup = ""
	if newStatus == Disconnected and not OFFLINE_IN_GROUPS then
		newGroup = "offline"
	else
		group  = interface:UICmd("GetBuddyGroup('"..name.."')") or ''
		if string.len(group) > 1 and HoN_Social_Panel.friendslist[group] ~= nil then
			newGroup = group
		else
			newGroup = "friends"
		end
		if MERGE_CLAN_FRIENDS and newGroup == "clan" then
			newGroup = "friends"
		end
	end

	local friend = HoN_Social_Panel.friendslist[oldGroup][name]
	friend.group = newGroup
	if newStatus == Disconnected then
		friend.status = "offline"
	else
		if newStatus >= online then
			friend.status = "online"
		end
		if newStatus >= ingame then
			friend.status = "ingame"
		end
	end

	local chatInfo = Explode('|', GetChatClientInfo(name, 'chatnamecolorstring|chatnamecoloringamestring|getaccounticontexturepath|chatnameglow|chatnameglowcolorstring|chatnameglowcoloringamestring|chatnamebackgroundglow'))

	friend.chatColorOnlineString	= Empty(chatInfo[1]) and DEFAULT_ONLINE_COLOR or chatInfo[1]
	friend.chatColorIngameString	= Empty(chatInfo[2]) and DEFAULT_INGAME_COLOR or chatInfo[2]
	friend.accountIcon				= chatInfo[3]
	friend.nameGlow					= chatInfo[4] or "false"
	friend.nameGlowColor			= chatInfo[5] or ''
	friend.nameGlowColorIngame		= chatInfo[6] or ''
	friend.nameBackgroundGlow		= chatInfo[7] or "false"
	
	if HoN_Social_Panel.friendslist[newGroup] == nil then
		HoN_Social_Panel.friendslist[newGroup] = {}
	end
	
	HoN_Social_Panel.friendslist[newGroup][name] = friend
	if newGroup ~= oldGroup then
		HoN_Social_Panel.friendslist[oldGroup][name] = nil
	end

	local onlinefriends = -1 --Dont ask
	for group, list in pairs(HoN_Social_Panel.friendslist) do
		if group ~= "offline" and group ~= "recent" then
			for username, friendtable in pairs(list) do
				onlinefriends = onlinefriends + 1
			end
		end
	end
	HoN_Social_Panel.onlineCount = onlinefriends

	HoN_Social_Panel.PopulateFriendsList()
end

triggerHelper:RegisterWatch('ChatAutoCompleteClear', ChatAutoCompleteClear)
triggerHelper:RegisterWatch('ChatAutoCompleteAdd', ChatAutoCompleteAdd)
triggerHelper:RegisterWatch('ChatCompanion', ChatCompanion)
triggerHelper:RegisterWatch('ChatCompanionEvent', ChatCompanionEvent)
triggerHelper:RegisterWatch('SocialMPDragged', SocialMPDragged)
triggerHelper:RegisterWatch('ShowCreateGroupPanel', ShowGroupCreationPanel)
triggerHelper:RegisterWatch('ChatBuddyStatusChanged', ChatBuddyStatusChanged)
