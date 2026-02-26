local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort
local interface, interfaceName = object, object:GetName()

local DEFAULT_ONLINE_COLOR = '#f0d3c1'
local DEFAULT_ONLINE_STATUS_COLOR = '#a99081'
local DEFAULT_INGAME_COLOR = '#a99081'
local DEFAULT_INGAME_STATUS_COLOR = '#7e604e'
local DEFAULT_OFFLINE_COLOR = '#707070'

local MAX_ENTRY_SLOTS = 11
local MAX_ENTRY_EMPTY_SLOTS = 3
local MAX_RECENTLY_PLAYED = 20

local IGNORE_OFFLINE = false
local LOAD_DEFAULTS = false
SHOW_CUSTOM_COLORS = true
USER_SORT_ONLINE_FIRST = true
MERGE_CLAN_FRIENDS = false
OFFLINE_IN_GROUPS = false

local SHOW_CUSTOM_COLORS_CVAR 	= 'social_customColors'
local USER_SORT_ONLINE_FIRST_CVAR = 'social_sortOnlineFirst'
local OFFLINE_IN_GROUPS_CVAR 	= 'social_offlineInGames'

local MAX_CLAN_SLOTS = 11
local MAX_CLAN_EMPTY_SLOTS = 3

Social_V2 = {}

Social_V2.ClanList, Social_V2.ClanOnlineList, Social_V2.ClanInGameList, Social_V2.ClanOfflineList = {}, {}, {}, {}
Social_V2.ClanStartIndex, Social_V2.ClanEndIndex = 0, 0

Social_V2.PlayerList = {}

Social_V2.inactiveInvitedList = {}
Social_V2.selectedPlayerName, Social_V2.joinGroupString = nil, nil, nil

Social_V2.grouplist = Social_V2.grouplist or {} --{group, visibility}

Social_V2.userEntryIndex, Social_V2.userOffset, Social_V2.lastUserOffset = 0, 0, 0

Social_V2.lockBuddyTable, Social_V2.lockBuddyList = false, false

Social_V2.totalEntries = 0
Social_V2.entryTable, Social_V2.headerTable, Social_V2.autoCompleteTable, Social_V2.userEntryPlayerTable = {}, {}, {}, {}

Social_V2_DB = Database.New('Social_V2.ldb')
local db = Social_V2_DB

local function SaveOptions()
	db.IGNORE_OFFLINE 			= IGNORE_OFFLINE
	db.LOAD_DEFAULTS 			= LOAD_DEFAULTS
	-- group visibility
	db.grouplist 				= Social_V2.grouplist or {}

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

	SetSave(OFFLINE_IN_GROUPS_CVAR, OFFLINE_IN_GROUPS, 'bool', true)
	cvar = Cvar.GetCvar(OFFLINE_IN_GROUPS_CVAR)
	if cvar then
		Cvar.SetCloudSave(cvar, true)
	end

	HoN_Options:UpdateLUAOptions()
end

local function LoadOptions()
	if (db.IGNORE_OFFLINE == nil) then
		SaveOptions()
	else
		IGNORE_OFFLINE 		 	= db.IGNORE_OFFLINE
		LOAD_DEFAULTS		 	= db.LOAD_DEFAULTS

		-- group visibility
		Social_V2.grouplist = db.grouplist or {}

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

		local cvar = Cvar.GetCvar(OFFLINE_IN_GROUPS_CVAR)
		if cvar == nil then
			OFFLINE_IN_GROUPS 	= db.OFFLINE_IN_GROUPS or OFFLINE_IN_GROUPS or false
		else
			OFFLINE_IN_GROUPS	= cvar:GetBoolean()
		end

		HoN_Options:UpdateLUAOptions()
	end
end

local function IsNumericTable(group)
	return group == 'recent' or group == 'autocomplete'
end

local function IsPredefinedGroup(group)
	return group == 'friends' or group == 'cafe' or group == 'offline' or group == 'autocomplete' or group == 'alert' or group == 'inactive' or group == 'recent'
end

local function IsInSearching()
	return NotEmpty(GetWidget('social_panel_search'):GetInputLine())
end

local function UpdateGroupArrows(group)
	for k, v in pairs(Social_V2.headerTable) do
		if v == group then
			GetWidget('social_player_category_'..k..'_arrow'):Rotate((Social_V2.grouplist[v] == nil or Social_V2.grouplist[v] == true) and 0 or 180, 50)
			break
		end
	end
end

local function ToggleGroupVisibility(group)
	if Social_V2.grouplist[group] ~= nil then
		Social_V2.grouplist[group] = not Social_V2.grouplist[group]
	else
		Social_V2.grouplist[group] = false
	end

	UpdateGroupArrows(group)
	GetWidget("social_friends"):Sleep(50, function() Social_V2:PopulateFriendsList() end)

	SaveOptions()
end

local function TransferFriendsToUI(numericTable, offset, usingSearch)
	if numericTable then
		Social_V2.userEntryIndex = 0

		local localOffSet = offset or 0

		local preItemName = 'social_player_item_'
		local preHeaderName = 'social_player_category_'

		for i=1, MAX_ENTRY_SLOTS do
			local tempGroupTable = numericTable[i + localOffSet]
			if tempGroupTable == nil then break end

			local playerTable = tempGroupTable.playerTable
			local player, group = nil, nil

			-- For searching, if the player is hidden, check the next one
			while usingSearch and tempGroupTable and playerTable and playerTable.hide do
				localOffSet = localOffSet + 1
				tempGroupTable = numericTable[i + localOffSet]
				if tempGroupTable then
					playerTable = tempGroupTable.playerTable
				end
			end

			-- If player data exists (not a header) grab this data
			if type(playerTable) == 'table' then
				player = playerTable.name
				group = playerTable.group
			end

			if tempGroupTable and tempGroupTable.header then
				-- increment widget and check that it's visible
				Social_V2.userEntryIndex = Social_V2.userEntryIndex + 1
				if Social_V2.userEntryIndex > MAX_ENTRY_SLOTS then break end

				-- Add header entry
				group = tempGroupTable.group

				local parent = interface:GetWidget(preItemName..Social_V2.userEntryIndex)
				local header = interface:GetWidget(preHeaderName..Social_V2.userEntryIndex)
				if parent then
					parent:SetVisible(false)
					header:SetVisible(true)

					local text = IsPredefinedGroup(group) and Translate('sp_listlabel_'..group) or group
					interface:GetWidget(preHeaderName..Social_V2.userEntryIndex..'_name'):SetText(text)

					Social_V2.headerTable[Social_V2.userEntryIndex] = group
					UpdateGroupArrows(group)
				end
			elseif player then
				-- increment widget and check that it's visible
				Social_V2.userEntryIndex = Social_V2.userEntryIndex + 1
				if Social_V2.userEntryIndex > MAX_ENTRY_SLOTS then break end

				-- quicker indexed lookup
				if not Social_V2.entryTable[group] then Social_V2.entryTable[group] = {} end
				tinsert(Social_V2.entryTable[group], Social_V2.userEntryIndex)

				-- Add player entry
				local parent = interface:GetWidget(preItemName..Social_V2.userEntryIndex)
				local header = interface:GetWidget(preHeaderName..Social_V2.userEntryIndex)
				if parent then
					parent:SetVisible(true)
					header:SetVisible(false)

					local name = interface:GetWidget(preItemName..Social_V2.userEntryIndex..'_name')
					local icon = interface:GetWidget(preItemName..Social_V2.userEntryIndex..'_icon')

					name:SetText(playerTable.tag..playerTable.name)

					-- Status
					local statusicon, statustext, statuscolor = '', '', '#7e604e'
					local statustext = ''

					if playerTable.status == 'ingame' then
						statusicon, statustext, statuscolor = '/ui/fe2/newui/res/social/ingame.png', 'In Game', DEFAULT_INGAME_STATUS_COLOR
					elseif playerTable.status == 'online' then
						statusicon, statustext, statuscolor = '/ui/fe2/newui/res/social/online.png', 'Online', DEFAULT_ONLINE_STATUS_COLOR
					else
						statusicon, statustext, statuscolor = '/ui/fe2/newui/res/social/offline.png', 'Offline', DEFAULT_OFFLINE_COLOR
					end
					GetWidget(preItemName..Social_V2.userEntryIndex..'_status_icon'):SetTexture(statusicon)
					GetWidget(preItemName..Social_V2.userEntryIndex..'_status_text'):SetText(statustext)
					GetWidget(preItemName..Social_V2.userEntryIndex..'_status_text'):SetColor(statuscolor)

					local btnAddFriend = GetWidget(preItemName..Social_V2.userEntryIndex..'_addfriend')
					if btnAddFriend then
						btnAddFriend:SetVisible(not IsMe(playerTable.name) and not IsBuddy(playerTable.name))
						btnAddFriend:SetCallback('onclick',
							function()
								Common_V2:AddFriend(playerTable.name)
								btnAddFriend:SetEnabled(false)
								interface:Sleep(6000, function() btnAddFriend:SetEnabled(true) end)
							end
						)
					end

					local accountIcon = playerTable.accountIcon or playerTable.chatColorTexture or playerTable.chatSymbolTexture or ''
					local accountIconColor = playerTable.accountIconColor or '1 1 1 1'
					if Empty(accountIcon) or accountIcon == '/ui/fe2/store/icons/account_icons/default.tga' and playerTable.status ~= 'autocomplete' then
						accountIcon = '/ui/fe2/store/icons/account_icons/default.tga'
						icon:SetColor('white')
					else
						icon:SetColor(accountIconColor)
					end
					icon:SetTexture(accountIcon)

					--Ascension level
					local level = math.abs(tonumber(playerTable.ascensionLevel or '0'))
					if level > 9999 then
						level = 9999
					end
					GetWidget(preItemName..Social_V2.userEntryIndex..'_ascension'):SetVisible(level > 0)
					if level > 0 then
						local ascensionImg = 0
						if (level >= 50) then
							ascensionImg = 0
						elseif level >= 30 then
							ascensionImg = 1
						else
							ascensionImg = 2
						end

						for j=0,2 do
							interface:GetWidget(preItemName..Social_V2.userEntryIndex..'_ascension_img_'..j):SetVisible(j == ascensionImg)
						end
						interface:GetWidget(preItemName..Social_V2.userEntryIndex..'_ascension_label'):SetText(tostring(level))
					end

					if playerTable.status == 'online' then
						Social_V2.userEntryPlayerTable[Social_V2.userEntryIndex] = player
						if IsFollowing(playerTable.name) then
							name:SetColor('#ff6d3c')
						else
							if NotEmpty(playerTable.chatColorOnlineString) and playerTable.chatColorOnlineString ~= '#FFFFFF' then
								name:SetColor(playerTable.chatColorOnlineString)
								name:SetGlow(playerTable.nameGlow)
								name:SetGlowColor(playerTable.nameGlowColor)
								name:SetBackgroundGlow(playerTable.nameBackgroundGlow)
							else
								name:SetColor(DEFAULT_ONLINE_COLOR)
								name:SetGlow(false)
							end
						end
					elseif playerTable.status == 'ingame' then
						Social_V2.userEntryPlayerTable[Social_V2.userEntryIndex] = player

						if NotEmpty(playerTable.chatColorIngameString) and playerTable.chatColorIngameString ~= '#777777' then
							name:SetColor(playerTable.chatColorIngameString)
							name:SetGlow(playerTable.nameGlow)
							name:SetBackgroundGlow(playerTable.nameBackgroundGlow)
							if playerTable.nameGlowColorIngame then
								name:SetGlowColor(playerTable.nameGlowColorIngame)
							end
						else
							name:SetColor(DEFAULT_INGAME_COLOR)
							name:SetGlow(false)
						end
					elseif playerTable.status == 'offline' then
						Social_V2.userEntryPlayerTable[Social_V2.userEntryIndex] = player
						if IsFollowing(playerTable.name) then
							name:SetColor('#ff6d3c')
							name:SetGlow(false)
						else
							name:SetColor(DEFAULT_OFFLINE_COLOR)
							name:SetGlow(false)
						end
					end
				end
			end
		end
	end

	if Social_V2.userEntryIndex < MAX_ENTRY_SLOTS then
		for i = Social_V2.userEntryIndex + 1, MAX_ENTRY_SLOTS do
			interface:GetWidget('social_player_item_'..i):SetVisible(false)
			interface:GetWidget('social_player_category_'..i):SetVisible(false)
		end
	end
end

local function CreateNumericTable(numericTable, groupTable, group, entryLimit)
	if groupTable then
		local entryCount = 0
		local tempPlayerTable = {}

		-- Hidden group excluded
		if Social_V2.grouplist[group] == nil or Social_V2.grouplist[group] then
			local onlineTable, ingameTable, offlineTable, alertTable = {}, {}, {}, {}

			for _, v in pairs(groupTable) do
				if v.focus == true then
					alertTable[v.name] = v
				elseif v.status == 'online' then
					onlineTable[v.name] = v
				elseif v.status == 'ingame' then
					ingameTable[v.name] = v
				else
					offlineTable[v.name] = v
				end
			end
			for _, v in pairsByKeys(alertTable, function(a, b) return string.lower(a) < string.lower(b) end) do
				if Empty(Social_V2.autoCompleteString)
					or string.find(string.lower(StripClanTag(v.name)), string.lower(StripClanTag(v.autoCompleteString)))
					or NotEmpty(v.tag) and string.find(string.lower(v.tag), string.lower(Social_V2.autoCompleteString)) then
					if group and (not Social_V2.grouplist[group] or Social_V2.grouplist[group]) then
						table.insert(tempPlayerTable, {playerTable = v, header = false})
						Social_V2.totalEntries = Social_V2.totalEntries + 1
					end
					entryCount = entryCount + 1
					if entryLimit and entryCount >= entryLimit then break end
				end
			end
			for _, v in pairsByKeys(onlineTable, function(a,b) return string.lower(a) < string.lower(b) end ) do
				if Empty(Social_V2.autoCompleteString)
					or string.find(string.lower(StripClanTag(v.name)), string.lower(StripClanTag(Social_V2.autoCompleteString)))
					or NotEmpty(v.tag) and string.find(string.lower(v.tag), string.lower(Social_V2.autoCompleteString)) then
					if group and (not Social_V2.grouplist[group] or Social_V2.grouplist[group]) then
						table.insert(tempPlayerTable, {playerTable = v, header = false} )
						Social_V2.totalEntries = Social_V2.totalEntries + 1
					end
					entryCount = entryCount + 1
					if entryLimit and entryCount >= entryLimit then break end
				end
			end
			for _, v in pairsByKeys(ingameTable, function(a, b) return string.lower(a) < string.lower(b) end ) do
				if Empty(Social_V2.autoCompleteString)
					or string.find(string.lower(StripClanTag(v.name)), string.lower(StripClanTag(Social_V2.autoCompleteString)))
					or NotEmpty(v.tag) and string.find(string.lower(v.tag), string.lower(Social_V2.autoCompleteString)) then
					if group and (not Social_V2.grouplist[group] or Social_V2.grouplist[group]) then
						table.insert(tempPlayerTable, {playerTable = v, header = false} )
						Social_V2.totalEntries = Social_V2.totalEntries + 1
					end
					entryCount = entryCount + 1
					if entryLimit and entryCount >= entryLimit then break end
				end
			end
			for _, v in pairsByKeys(offlineTable, function(a, b) return string.lower(a) < string.lower(b) end ) do
				if Empty(Social_V2.autoCompleteString)
					or string.find(string.lower(StripClanTag(v.name)), string.lower(StripClanTag(Social_V2.autoCompleteString)))
					or NotEmpty(v.tag) and string.find(string.lower(v.tag), string.lower(Social_V2.autoCompleteString)) then
					if group and (not Social_V2.grouplist[group] or Social_V2.grouplist[group]) then
						table.insert(tempPlayerTable, {playerTable = v, header = false} )
						Social_V2.totalEntries = Social_V2.totalEntries + 1
					end
					entryCount = entryCount + 1
					if entryLimit and entryCount >= entryLimit then break end
				end
			end
		end

		-- Hidden group header included
		if entryCount > 0 or (Social_V2.grouplist[group] ~= nil and not Social_V2.grouplist[group]) then
			tinsert(numericTable, {group = group, header = true} )
			Social_V2.totalEntries = Social_V2.totalEntries + 1

			for _, v in ipairs(tempPlayerTable) do
				tinsert(numericTable, v)
			end
		end

		tempPlayerTable = nil
	end
	return numericTable
end

function Social_V2:CloseSocialPopups()
	GetWidget('social_friends'):SetVisible(false)
	GetWidget('social_clan'):SetVisible(false)
	GetWidget('system_message_container'):SetVisible(false)
end

function Social_V2:SearchFriendsBegin(widget)
	GetWidget('social_panel_search_idletext'):SetVisible(false)

	widget:RegisterWatch('ChatAutoCompleteAdd', function(_, ...) Social_V2:AutoCompleteAdd(...) end)
	widget:RegisterWatch('ChatAutoCompleteClear', function(_, ...) Social_V2:AutoCompleteClear(...) end)
end

function Social_V2:SearchFriendsEnd(widget)
	if IsInSearching() then
		return
	end

	GetWidget('social_panel_search_idletext'):SetVisible(Empty(widget:GetValue()))

	widget:UnregisterWatch('ChatAutoCompleteAdd')
	widget:UnregisterWatch('ChatAutoCompleteClear')

	Social_V2:PopulateFriendsList(0)
end

function Social_V2:InitFriends()
	local widget = GetWidget('social_friends')

	widget:RegisterWatch('ChatCompanionEvent', function(_, ...) Social_V2:ChatCompanionEvent(...) end)
	widget:RegisterWatch('ChatCompanion', function(_, ...) Social_V2:ChatCompanion(...) end)

	widget:RegisterWatch('ChatBuddyStatusChanged', function(_, name, curStatus, preStatus) Social_V2:ChatBuddyStatusChanged(name, curStatus, preStatus) end)

	for i=0, 9 do
		widget:RegisterWatch("LobbyPlayerInfo"..i, function(_, ...) Social_V2:AddRecentlyPlayed(...) end)
	end

	widget:RegisterWatch('LoginStatus', function(_, accountStatus, statusDescription, isLoggedIn, pwordExpired, isLoggedInChanged, updaterStatus)
	    if AtoB(isLoggedIn) then
			-- inactive players
			Social_V2.inactiveInvitedList = GetDBEntry('hon_inactive_invited_friends_'..GetAccountName(), nil, false, false, false) or {}
	    elseif AtoB(isLoggedInChanged) then
	    	Social_V2.ClanList, Social_V2.ClanOnlineList, Social_V2.ClanInGameList, Social_V2.ClanOfflineList = {}, {}, {}, {}
			Social_V2.ClanStartIndex, Social_V2.ClanEndIndex = 0, 0

	    	Social_V2.PlayerList = {}
	    	Social_V2.grouplist = {}
	    end
	end)
end

function Social_V2:GetFriendInfo(name)
	for k, v in pairs(Social_V2.PlayerList) do
		if not IsNumericTable(k) and v ~= nil and v[name] ~= nil then
			return Social_V2.PlayerList[k][name]
		end
	end
	return nil
end

function Social_V2:AddToGroup(name, group, gameStatus, verified, chatSymbolTexture, chatColorTexture, chatColorOnlineString, chatColorIngameString, accountIcon, clanStatus, nameGlow, buddyGroup, isAccountNew, isAccountInactive, nameGlowColor, nameGlowColorIngame, ascensionLevel, nameBackgroundGlow)
	if Social_V2.PlayerList[group] == nil then
		Social_V2.PlayerList[group] = {}
	end

	if Social_V2.PlayerList[group][name] == nil then
		Social_V2.PlayerList[group][name] = {
			name = name,
			tag = tag or '',
			group = group or '',
			nameGlow = AtoB(nameGlow) or false,
			nameBackgroundGlow = AtoB(nameBackgroundGlow) or false,
			clanStatus = clanStatus or '',
			buddyGroup = buddyGroup or '',
			source = source or '',
			status = gameStatus or 'ingame',
			verified = AtoB(verified or false),
			chatSymbolTexture = chatSymbolTexture or '',
			chatColorTexture = chatColorTexture or '',
			chatColorOnlineString = Empty(chatColorOnlineString) and DEFAULT_ONLINE_COLOR or chatColorOnlineString,
			chatColorIngameString = Empty(chatColorIngameString) and DEFAULT_INGAME_COLOR or chatColorIngameString,
			accountIcon = accountIcon or '',
			hide = false,
			isAccountNew = isAccountNew or nil,
			isAccountInactive = isAccountInactive or nil,
			nameGlowColor = nameGlowColor or '',
			nameGlowColorIngame = nameGlowColorIngame or '',
			ascensionLevel = ascensionLevel or '0'
		}
	else
		Social_V2.PlayerList[group][name].name = name
		Social_V2.PlayerList[group][name].tag = tag
		Social_V2.PlayerList[group][name].group = group
		Social_V2.PlayerList[group][name].nameGlow = AtoB(nameGlow) or false
		Social_V2.PlayerList[group][name].nameBackgroundGlow = AtoB(nameBackgroundGlow) or false
		Social_V2.PlayerList[group][name].clanStatus = clanStatus or ''
		Social_V2.PlayerList[group][name].buddyGroup = buddyGroup or ''
		Social_V2.PlayerList[group][name].source = source or ''
		Social_V2.PlayerList[group][name].status = gameStatus or 'ingame'
		Social_V2.PlayerList[group][name].verified = AtoB(verified or false)
		Social_V2.PlayerList[group][name].chatSymbolTexture = chatSymbolTexture or ''
		Social_V2.PlayerList[group][name].chatColorTexture = chatColorTexture or ''
		Social_V2.PlayerList[group][name].chatColorOnlineString = Empty(chatColorOnlineString) and DEFAULT_ONLINE_COLOR or chatColorOnlineString
		Social_V2.PlayerList[group][name].chatColorIngameString = Empty(chatColorIngameString) and DEFAULT_INGAME_COLOR or chatColorIngameString
		Social_V2.PlayerList[group][name].accountIcon = accountIcon or ''
		Social_V2.PlayerList[group][name].hide = false
		Social_V2.PlayerList[group][name].isAccountNew = isAccountNew or nil
		Social_V2.PlayerList[group][name].isAccountInactive = isAccountInactive or nil
		Social_V2.PlayerList[group][name].nameGlowColor = nameGlowColor or ''
		Social_V2.PlayerList[group][name].nameGlowColorIngame = nameGlowColorIngame or ''
		Social_V2.PlayerList[group][name].ascensionLevel = ascensionLevel or '0'
	end
end

function Social_V2:ChatCompanion(name, isVerified, chatSymbolTexture, chatColorTexture, chatColorOnlineString, chatColorIngameString, accountIcon, nameGlow, buddyGroup, isOnline, isInGame, isBuddy, inClan, isOfficer, isleader, isAccountNew, isAccountInactive, nameGlowColor, nameGlowColorIngame, ascensionLevel, isCafePlayer, nameBackgroundGlow)
	if Empty(name) then Echo('^rSocial_V2:ChatCompanion invalid user name!') return end

	local status, group = AtoB(isInGame) and 'ingame' or (AtoB(isOnline) and 'online' or 'offline'), nil

	if status ~= 'offline' then
		local source = interface:UICmd('GetBuddyGroup(\''..name..'\')') or ''
		if string.len(source) > 1 then
			group = source
		else
			if AtoB(isBuddy) then
				group = 'friends'
			end
			if AtoB(isCafePlayer) then
				group = 'cafe'
			end
		end
	elseif status == 'offline' then
		if AtoB(isAccountInactive) and Social_V2.inactiveInvitedList[name] ~= 1 then
			group = 'inactive'
		elseif AtoB(isBuddy) then
			group = 'offline'
		end
	end

	if NotEmpty(group) then
		Social_V2:AddToGroup(name, group, status, isVerified, chatSymbolTexture, chatColorTexture, chatColorOnlineString, chatColorIngameString, accountIcon, clanStatus, nameGlow, buddyGroup, isAccountNew, isAccountInactive, nameGlowColor, nameGlowColorIngame, ascensionLevel, nameBackgroundGlow)
	end
end

function Social_V2:ChatCompanionEvent(start)
	if not Social_V2.lockBuddyTable then
		if AtoB(start) then
			Social_V2.PlayerList = {}

			if not IsInSearching() then
				Social_V2:AutoCompleteClear()
			end

			-- recent
			Social_V2.PlayerList['recent'] = GetDBEntry('hon_recently_played_'..GetAccountName(), nil, false, false, false) or {}
			for k, v in ipairs(Social_V2.PlayerList['recent']) do
				if IsMe(v.name) or IsBuddy(v.name) then
					table.remove(Social_V2.PlayerList['recent'], k)
				else
					local online, ingame = ChatUserOnline(v.name), ChatUserInGame(v.name)
					v.status = ingame and 'ingame' or (online and 'online' or 'offline')
					v.isOnline = online
					v.isBuddy = false
					v.verified = type(v.verified) == 'string' and AtoB(v.verified) or v.verified
					v.nameGlow = type(v.nameGlow) == 'string' and AtoB(v.nameGlow) or v.nameGlow
					v.nameBackgroundGlow = type(v.nameBackgroundGlow) == 'string' and AtoB(v.nameBackgroundGlow) or v.nameBackgroundGlow
				end
			end
		else
			LoadOptions()
			Social_V2.lockBuddyTable = true
			Social_V2:ScrollFriendList(Social_V2.lastUserOffset or 0)
		end
	end
end

function Social_V2:ChatBuddyStatusChanged(name, curStatus, preStatus)
	curStatus, preStatus = tonumber(curStatus), tonumber(preStatus)
	if (curStatus == preStatus) then return end

	-- Echo('^ySocial_V2:ChatBuddyStatusChanged name:'..name..' curStatus:'..curStatus..' preStatus:'..preStatus)

	local online, ingame, wasOnline = curStatus >= 3, curStatus >= 4, preStatus >= 3

	if wasOnline and online then
		for g, group in pairs(Social_V2.PlayerList) do
			if not IsNumericTable(g) and group ~= nil and group[name] ~= nil then
				group[name].status = ingame and 'ingame' or 'online'
				Social_V2:PopulateFriendsList()
				break
			end
		end
	else
		if wasOnline and not online then
			for g, group in pairs(Social_V2.PlayerList) do
				if not IsNumericTable(g) and group ~= nil and group[name] ~= nil then
					group[name].status = 'offline'
					Social_V2.PlayerList['offline'][name] = group[name]
					Social_V2.PlayerList[g][name] = nil
					Social_V2:PopulateFriendsList()
					break
				end
			end
		elseif not wasOnline and online then
			local newgroup, source = 'friends', interface:UICmd('GetBuddyGroup(\''..name..'\')') or ''
			if string.len(source) > 1 then
				newgroup = source
			end

			local chatInfo = Explode('|', GetChatClientInfo(name, 'chatnamecolorstring|chatnamecoloringamestring|getaccounticontexturepath|chatnameglow|chatnameglowcolorstring|chatnameglowcoloringamestring|chatnamebackgroundglow'))

			for g, group in pairs(Social_V2.PlayerList) do
				if not IsNumericTable(g) and group ~= nil and group[name] ~= nil then
					group[name].status = 'online'
					group[name].chatColorOnlineString = Empty(chatInfo[1]) and DEFAULT_ONLINE_COLOR or chatInfo[1]
					group[name].chatColorIngameString = Empty(chatInfo[2]) and DEFAULT_INGAME_COLOR or chatInfo[2]
					group[name].accountIcon = chatInfo[3]
					group[name].nameGlow = AtoB(chatInfo[4]) or false
					group[name].nameGlowColor = chatInfo[5] or ''
					group[name].nameGlowColorIngame = chatInfo[6] or ''
					group[name].nameBackgroundGlow = AtoB(chatInfo[7]) or false

					Social_V2.PlayerList[newgroup] = Social_V2.PlayerList[newgroup] or {}
					Social_V2.PlayerList[newgroup][name] = group[name]
					Social_V2.PlayerList[g][name] = nil
					Social_V2:PopulateFriendsList()
					break
				end
			end
		end
	end
end

function Social_V2:AddRecentlyPlayed(...)
	if IsInSearching() then return end

	local name = arg[2]

	if AtoB(arg[85]) or		-- is bot
		Empty(name) or 		-- is empty
		name == "?????" or 	-- is someone loading or something
		IsMe(name) or		-- is self
		IsBuddy(name) 		-- is buddy
	then
		return
	end

	-- prune the table
	local numRecent = #Social_V2.PlayerList['recent']
	while numRecent >= MAX_RECENTLY_PLAYED do
		table.remove(Social_V2.PlayerList['recent'], numRecent)
		numRecent = numRecent - 1
	end

	-- search for dupes and remove them (we want to reinsert them at the top even if they are a dupe)
	for k, v in ipairs(Social_V2.PlayerList['recent']) do
		if v.name == arg[2] then
			table.remove(Social_V2.PlayerList['recent'], k)
		end
	end

	-- get the info for the player
	local chatInfoTable = Explode("|", GetChatClientInfo(arg[2], "chatsymbol|chatnamecolortexturepath|chatnamecolorstring|chatnamecoloringamestring|getaccounticontexturepath|chatnameglow|chatnameglowcolorstring|chatnameglowcoloringamestring|chatnamebackgroundglow"))

	local online, ingame = ChatUserOnline(arg[2]), ChatUserInGame(arg[2])

	-- add to the table
	local player = {
		name = arg[2],
		tag = '',
		group = 'recent',
		clanStatus = '',
		buddyGroup = '',
		source = 'recent',
		status = ingame and 'ingame' or (online and 'online' or 'offline'),
		verified = false,
		chatSymbolTexture = chatInfoTable[1],
		chatColorTexture = chatInfoTable[2],
		chatColorOnlineString = Empty(chatInfoTable[3]) and DEFAULT_ONLINE_COLOR or chatInfoTable[3],
		chatColorIngameString = Empty(chatInfoTable[4]) and DEFAULT_INGAME_COLOR or chatInfoTable[4],
		accountIcon = chatInfoTable[5],
		isOnline = online,
		isBuddy = false,
		focus = false,
		hide = false,
		nameGlow = AtoB(chatInfoTable[6]) or false,
		nameGlowColor = chatInfoTable[7] or '',
		nameGlowColorIngame = chatInfoTable[8] or '',
		nameBackgroundGlow = AtoB(chatInfoTable[9]) or false,
		ascensionLevel = '0',
	}

	table.insert(Social_V2.PlayerList['recent'], 1, player)

	-- save the table
	GetDBEntry('hon_recently_played_'..GetAccountName(), Social_V2.PlayerList['recent'], true, false, false)

	Social_V2:PopulateFriendsList()
end

function Social_V2:AutoCompleteAdd(name)
	if not IsInSearching() then
		Social_V2:AutoCompleteClear()
		return
	end

	local online, ingame = ChatUserOnline(name), ChatUserInGame(name)
	local player = {
		name = name,
		tag = '',
		group = 'autocomplete',
		nameGlow = false,
		nameBackgroundGlow = false,
		clanStatus = '',
		buddyGroup = '',
		source = 'autocomplete',
		status = ingame and 'ingame' or (online and 'online' or 'offline'),
		verified = false,
		chatSymbolTexture = '',
		chatColorTexture = '',
		chatColorOnlineString = '',
		chatColorIngameString = '',
		accountIcon = '',
		isOnline = online,
		isBuddy = IsBuddy(name),
		focus = false,
		hide = false
	}

	table.insert(Social_V2.autoCompleteTable, player)
	Social_V2:PopulateFriendsList()
end

function Social_V2:AutoCompleteClear()
	Social_V2.autoCompleteTable = {}
end

function Social_V2:UserEntryClicked(id)
	if Social_V2.userEntryPlayerTable[id] then
		Communicator_V2:GenerateRightClickMenu(Social_V2.userEntryPlayerTable[id], '-1', 'friend')
	end
end

function Social_V2:UserEntryDoubleClicked(id)
	if Social_V2.userEntryPlayerTable[id] then
		local player = Social_V2.userEntryPlayerTable[id]
		if not IsMe(player) then
			Communicator_V2:StartChatWithPlayer(player, true)
		end
	end
end

function Social_V2:GroupButtonClicked(id)
	ToggleGroupVisibility(Social_V2.headerTable[id])
end

function Social_V2:ScrollFriendList(offset)
	if not Social_V2.lockBuddyList then
		Social_V2.lockBuddyList = true
		Social_V2:PopulateFriendsList(offset or 0)
	end
end

function Social_V2:PopulateFriendsList(offset)
	Social_V2.totalEntries = 0
	Social_V2.entryTable, Social_V2.headerTable = {}, {}

	local usingSearch = false
	local listbox = interface:GetWidget('social_panel_friends_container')
	local offset = offset or Social_V2.lastUserOffset or 1

	local numericTable = {}

	for group, groupTable in pairs(Social_V2.PlayerList) do
		if group ~= 'friends' and group ~= 'cafe' and group ~= 'offline' and group ~= 'inactive' and group ~= 'recent' then
			numericTable = CreateNumericTable(numericTable, groupTable, group)
		end
	end

	numericTable = CreateNumericTable(numericTable, Social_V2.PlayerList['friends'], 'friends')
	numericTable = CreateNumericTable(numericTable, Social_V2.PlayerList['cafe'], 'cafe')
	numericTable = CreateNumericTable(numericTable, Social_V2.PlayerList['recent'], 'recent')
	numericTable = CreateNumericTable(numericTable, Social_V2.PlayerList['inactive'], 'inactive')
	numericTable = CreateNumericTable(numericTable, Social_V2.PlayerList['offline'], 'offline')

	---[[ Filter all user groups if search string present
	usingSearch = NotEmpty(Social_V2.autoCompleteString) and #numericTable >= 1
	--]]

	---[[ Add autocomplete 'group' as lowest priority
	if #Social_V2.autoCompleteTable >= 1 then
		numericTable = CreateNumericTable(numericTable, Social_V2.autoCompleteTable, 'autocomplete')
	end
	--]]

	-- Update actual UI userlist widgets
	if #numericTable >= 1 then
		local showScroll = #numericTable > MAX_ENTRY_SLOTS
		local scrollBar = interface:GetWidget('social_panel_friends_scrollbar')
		local scrollPanel = interface:GetWidget('social_panel_friends_scrollpanel')
		scrollBar:SetVisible(showScroll)
		scrollPanel:SetVisible(showScroll)
		if showScroll then
			scrollBar:SetMaxValue(#numericTable - MAX_ENTRY_SLOTS + MAX_ENTRY_EMPTY_SLOTS)
		else
			offset = 0
			scrollBar:SetValue(0)
		end

		TransferFriendsToUI(numericTable, offset, usingSearch)
	end

	Social_V2.lastUserOffset = offset or 1
	Social_V2.lockBuddyList = false
	Social_V2.lockBuddyTable = false
end

function Social_V2:UpdateAutoCompleteInput(searchText)
	Social_V2.autoCompleteString = searchText

	if NotEmpty(searchText) then
		searchText = string.gsub(string.gsub(string.gsub(string.gsub(string.gsub(string.gsub(searchText, "%[", ""), "%%", ""), "%(", ""), "%]", ""), "%)", ""), "'", "")
	end

	if searchText then
		Social_V2:AutoCompleteClear()

		local length = string.len(searchText)
		if length >= 5 then
			ChatAutoCompleteNick(searchText)
		else
			if length > 0 then
				Trigger('ChatAutoCompleteAdd', searchText)
			else
				Social_V2:PopulateFriendsList()
			end
		end
	end
end

function Social_V2:OnToggleFriendPanel()
	local widget = GetWidget('social_friends')
	local visible = widget:IsVisible()
	if visible then
		if IsInSearching() then
			GetWidget('social_panel_search'):EraseInputLine()
			GetWidget('social_panel_search_idletext'):SetVisible(true)
			Social_V2:PopulateFriendsList(0)
		end
	end
	widget:SetVisible(not visible)
end

function Social_V2:OnToggleClanPanel()
	local widget = GetWidget('social_clan')
	widget:SetVisible(not widget:IsVisible())
end

function Social_V2:UpdateOptions()
	SaveOptions()
	ChatRefresh()
end

----------------------------------------------------------------------------------------
--group
local function ChatChangeBuddyGroup(nickname, buddyGroup)
	interface:UICmd("ChatChangeBuddyGroup('"..nickname.."', '"..string.gsub(string.gsub(buddyGroup, "\\", "\\\\"), "'", "\\'").."')")
end

local function AddUserToGroup()
	interface:GetWidget('social_create_group'):FadeOut(50)
	if Social_V2.selectedPlayerName and Social_V2.joinGroupString then
		ChatChangeBuddyGroup(Social_V2.selectedPlayerName, Social_V2.joinGroupString)
	end
	delayFunction(3000, 'ChatRefresh', _G)
end

local function RemoveUserGroup()
	interface:GetWidget('social_create_group'):FadeOut(50)
	if Social_V2.selectedPlayerName and Social_V2.joinGroupString then
		ChatChangeBuddyGroup(Social_V2.selectedPlayerName, '')
	end
	delayFunction(3000, 'ChatRefresh', _G)
end

local function CreateGroupButtonClicked(input, button)
	if (input == 1) then
		AreYouSureDialog(button, function() button:SetVisible(true) AddUserToGroup() end, function() button:SetVisible(true) end)
		button:SetVisible(false)
	elseif (input == 2) then
		AreYouSureDialog(button, function() button:SetVisible(true) AddUserToGroup() end, function() button:SetVisible(true) end)
		button:SetVisible(false)
	elseif (input == 3) then
		AreYouSureDialog(button, function() button:SetVisible(true) RemoveUserGroup() end, function() button:SetVisible(true) end)
		button:SetVisible(false)
	end
end

local function UpdateGroupListbox()
	local wdgGroupList = interface:GetWidget('sp_group_listbox')
	wdgGroupList:ClearItems()
	local currentGroup = UIGetBuddyGroup(Social_V2.selectedPlayerName)
	if NotEmpty(currentGroup) and not IsPredefinedGroup(currentGroup) then
		wdgGroupList:AddTemplateListItem('social_create_clan_autocomplete_player', 'remove group', 'username', Translate("sp_remove_group"), 'color', 'red')
	end
	for group, _ in pairs(Social_V2.PlayerList) do
		if not IsPredefinedGroup(group) then
			wdgGroupList:AddTemplateListItem('social_create_clan_autocomplete_player', group, 'username', group)
		end
	end
end

function Social_V2:GroupListSelectInput(input)
	if NotEmpty(input) then
		GetWidget('sp_group_input_cover'):SetVisible(false)
		GetWidget('sp_textbox_group_input'):SetInputLine(input)
	end
end

function Social_V2:GroupInputBoxUpdate(input)
	Social_V2.joinGroupString = NotEmpty(input) and input or nil

	local label = interface:GetWidget('sp_group_dyn_label')
	local button = interface:GetWidget('sp_group_dyn_button')

	if Social_V2.joinGroupString then
		if Social_V2.joinGroupString == 'remove group' then
			label:SetText(Translate('social_remove_from_group'))
			button:SetCallback('onclick', function() CreateGroupButtonClicked(3, button) end)
		elseif Social_V2.PlayerList[Social_V2.joinGroupString] then
			label:SetText(Translate('social_add_to_group'))
			button:SetCallback('onclick', function() CreateGroupButtonClicked(2, button) end)
		elseif #Social_V2.joinGroupString > 1 then
			label:SetText(Translate('social_create_new_group'))
			button:SetCallback('onclick', function() CreateGroupButtonClicked(1, button) end)
		end
	else
		if interface:GetWidget('sp_group_input_cover'):IsVisible() then
			label:SetText(Translate('social_select_a_group'))
		else
			label:SetText(Translate('social_enter_new_group'))
		end
		button:SetCallback('onclick', function() CreateGroupButtonClicked(0, button) end)
	end
end

function Social_V2:SetFriendGroup(nickname)
	Social_V2.selectedPlayerName = StripClanTag(nickname)
	local wdgPanel = GetWidget('social_create_group')
	wdgPanel:SetX(GetWidget('social_friends'):GetX()-wdgPanel:GetWidth())
	wdgPanel:SetY(Input.GetCursorPosY()-wdgPanel:GetHeight())
	wdgPanel:FadeIn(150)
	UpdateGroupListbox()
end

local function AddPlayerToInvitedInactiveList(_, player)
	Social_V2.inactiveInvitedList[player] = 1
	Social_V2.inactiveInvitedList = GetDBEntry('hon_inactive_invited_friends_'..GetAccountName(), Social_V2.inactiveInvitedList, true, false, false) or {}
end
interface:RegisterWatch('social_panel_inactive_invited', AddPlayerToInvitedInactiveList)

-----------------------------------Clan list-----------------------------------------------------------
local function RefreshClanList()
	local id = 1
	for i = Social_V2.ClanStartIndex, Social_V2.ClanEndIndex do
		local player = Social_V2.ClanList[i]
		if player and id <= MAX_CLAN_SLOTS then
			local prefix = 'social_clan_list_item_'..id

			interface:GetWidget(prefix):SetVisible(true)

			interface:GetWidget(prefix..'_icon'):SetTexture(player.icon)
			interface:GetWidget(prefix..'_icon'):SetRenderMode(player.rendermode)

			interface:GetWidget(prefix..'_officer'):SetVisible(player.officer)
			interface:GetWidget(prefix..'_leader'):SetVisible(player.leader)

			local wdgName = interface:GetWidget(prefix..'_name')
			wdgName:SetText(player.fullname)
			wdgName:SetColor(IsFollowing(player.fullname) and '#ff6d3c' or player.color)
			wdgName:SetGlow(player.nameGlow)
			wdgName:SetGlowColor(player.nameGlowColor)
			wdgName:SetBackgroundGlow(player.nameBackgroundGlow)
			wdgName:RegisterWatch('ChatFollowStatus', function(widget) widget:SetColor(IsFollowing(player.fullname) and '#ff6d3c' or player.color) end)

			interface:GetWidget(prefix..'_status_icon'):SetTexture('/ui/fe2/newui/res/social/'..player.status..'.png')

			interface:GetWidget(prefix..'_status_text'):SetText(player.status_label)
			interface:GetWidget(prefix..'_status_text'):SetColor(player.statuscolor)

			id = id + 1
		end
	end
	if id <= MAX_CLAN_SLOTS then
		for i = id, MAX_CLAN_SLOTS do
			interface:GetWidget('social_clan_list_item_'..i):SetVisible(false)
		end
	end
end

function Social_V2:ChatTotalClanMembers(...)
	local hasMember = tonumber(arg[1]) > 0
	GetWidget('social_clan_list_container'):SetVisible(hasMember)
	GetWidget('social_clan_settings_btn'):SetVisible(hasMember)
	GetWidget('social_create_new_clan_btn'):SetVisible(not hasMember)
end

function Social_V2:ChatClanOnline(...)
	local player = {
		name = StripClanTag(arg[1]),
		fullname = arg[1],
		icon = NotEmpty(arg[7]) and arg[7] or '/ui/fe2/store/icons/account_icons/default.tga',
		rendermode = 'normal',
		officer = AtoB(arg[11]),
		leader = AtoB(arg[12]),
		color = (Empty(arg[5]) or arg[5] == '#FFFFFF') and DEFAULT_ONLINE_COLOR or arg[5],
		nameGlow = AtoB(arg[8]) or false,
		nameGlowColor = arg[9],
		nameBackgroundGlow = AtoB(arg[13]) or false,
		statuscolor = DEFAULT_ONLINE_STATUS_COLOR,
		status = 'online',
		status_label = Translate('sp_listlabel_online')
	}

	table.insert(Social_V2.ClanOnlineList, player)
end

function Social_V2:ChatClanGame(...)
	local player = {
		name = StripClanTag(arg[1]),
		fullname = arg[1],
		icon = NotEmpty(arg[7]) and arg[7] or '/ui/fe2/store/icons/account_icons/default.tga',
		rendermode = 'normal',
		officer = AtoB(arg[11]),
		leader = AtoB(arg[12]),
		color = (Empty(arg[6]) or arg[6] == '#777777') and DEFAULT_INGAME_COLOR or arg[6],
		nameGlow = AtoB(arg[8]) or false,
		nameGlowColor = arg[9],
		nameBackgroundGlow = AtoB(arg[13]) or false,
		statuscolor = DEFAULT_INGAME_STATUS_COLOR,
		status = 'ingame',
		status_label = Translate('sp_listlabel_ingame')
	}

	table.insert(Social_V2.ClanInGameList, player)
end

function Social_V2:ChatClanOffline(...)
	local player = {
		name = StripClanTag(arg[1]),
		fullname = arg[1],
		icon = NotEmpty(arg[7]) and arg[7] or '/ui/fe2/store/icons/account_icons/default.tga',
		rendermode = 'grayscale',
		officer = AtoB(arg[11]),
		leader = AtoB(arg[12]),
		color = DEFAULT_OFFLINE_COLOR,
		nameGlow = AtoB(arg[8]) or false,
		nameGlowColor = arg[9],
		nameBackgroundGlow = AtoB(arg[13]) or false,
		statuscolor = DEFAULT_OFFLINE_COLOR,
		status = 'offline',
		status_label = Translate('sp_listlabel_offline')
	}

	table.insert(Social_V2.ClanOfflineList, player)
end

function Social_V2:ChatClanEvent(...)
	local isGaming = GetCurrentGamePhase() >= 5
	if arg[1] == 'ClearItems' then
		Social_V2.ClanList, Social_V2.ClanOnlineList, Social_V2.ClanInGameList, Social_V2.ClanOfflineList = {}, {}, {}, {}
		Social_V2.ClanStartIndex, Social_V2.ClanEndIndex = 0, 0
	elseif arg[1] == 'SortListboxSortIndex' then
		Social_V2:GenerateClanList()
	end
end

function Social_V2:InitClan()
	local widget = GetWidget('social_clan')

	widget:RegisterWatch('ChatTotalClanMembers', function(_, ...) Social_V2:ChatTotalClanMembers(...) end)

	widget:RegisterWatch('ChatClanOnline', function(_, ...) Social_V2:ChatClanOnline(...) end)
	widget:RegisterWatch('ChatClanGame', function(_, ...) Social_V2:ChatClanGame(...) end)
	widget:RegisterWatch('ChatClanOffline', function(_, ...) Social_V2:ChatClanOffline(...) end)
	widget:RegisterWatch('ChatClanEvent', function(_, ...) Social_V2:ChatClanEvent(...) end)
end

function Social_V2:ClanEntryClicked(id)
	if Social_V2.ClanList[Social_V2.ClanStartIndex+id-1] then
		Communicator_V2:GenerateRightClickMenu(Social_V2.ClanList[Social_V2.ClanStartIndex+id-1].fullname, '-1')
	else
		Echo('Social_V2:ClanEntryClicked invalid entry data with id:'..id)
	end
end

function Social_V2:ClanEntryDoubleClicked(id)
	if Social_V2.ClanList[Social_V2.ClanStartIndex+id-1] then
		local player = Social_V2.ClanList[Social_V2.ClanStartIndex+id-1].fullname
		if not IsMe(player) then
			Communicator_V2:StartChatWithPlayer(player, true)
		end
	else
		Echo('Social_V2:ClanEntryDoubleClicked invalid entry data with id:'..id)
	end
end

function Social_V2:ScrollClanList(offset)
	if offset == 1 and Social_V2.ClanStartIndex == 1 then
		return
	end

	local numClanMember = #Social_V2.ClanList

	local preClanStartIndex, preClanEndIndex = Social_V2.ClanStartIndex, Social_V2.ClanEndIndex

	Social_V2.ClanStartIndex = offset
	Social_V2.ClanEndIndex = Social_V2.ClanStartIndex + MAX_CLAN_SLOTS

	if Social_V2.ClanStartIndex > (numClanMember + MAX_CLAN_EMPTY_SLOTS - MAX_CLAN_SLOTS) then
		Social_V2.ClanStartIndex = numClanMember + MAX_CLAN_EMPTY_SLOTS - MAX_CLAN_SLOTS
	elseif Social_V2.ClanStartIndex < 1 then
		Social_V2.ClanStartIndex = 1
	end

	if Social_V2.ClanEndIndex > (numClanMember + MAX_CLAN_EMPTY_SLOTS) then
		Social_V2.ClanEndIndex = numClanMember + MAX_CLAN_EMPTY_SLOTS
	end

	if preClanStartIndex ~= Social_V2.ClanStartIndex or preClanEndIndex ~= Social_V2.ClanEndIndex then
		RefreshClanList()
	end
end

function Social_V2:GenerateClanList()
	local sortFunc = function(a, b) return string.lower(a.fullname) < string.lower(b.fullname) end
	table.sort(Social_V2.ClanOnlineList, sortFunc)
	table.sort(Social_V2.ClanInGameList, sortFunc)
	table.sort(Social_V2.ClanOfflineList, sortFunc)

	for _, v in ipairs(Social_V2.ClanOnlineList) do
		table.insert(Social_V2.ClanList, v)
	end
	for _, v in ipairs(Social_V2.ClanInGameList) do
		table.insert(Social_V2.ClanList, v)
	end
	for _, v in ipairs(Social_V2.ClanOfflineList) do
		table.insert(Social_V2.ClanList, v)
	end

	local numClanMember = #Social_V2.ClanList
	if numClanMember > 0 then
		local showScroll = numClanMember > MAX_CLAN_SLOTS

		local scrollBar = interface:GetWidget('social_clan_list_scrollbar')
		local scrollPanel = interface:GetWidget('social_clan_list_scrollpanel')

		scrollBar:SetVisible(showScroll)
		scrollPanel:SetVisible(showScroll)

		if showScroll then
			scrollBar:SetMaxValue(numClanMember + MAX_CLAN_EMPTY_SLOTS)
			scrollBar:SetMinValue(1)
			scrollBar:SetValue(1)
		end

		Social_V2.ClanStartIndex = 1
		Social_V2.ClanEndIndex = numClanMember > MAX_CLAN_SLOTS and MAX_CLAN_SLOTS or numClanMember

		RefreshClanList()
	end
end
