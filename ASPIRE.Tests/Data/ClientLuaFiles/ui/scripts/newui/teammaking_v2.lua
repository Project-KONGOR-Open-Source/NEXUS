local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort
local interface, interfaceName = object, object:GetName()

local TEAMMEMBER_MAX_COUNT = 5
local INVITE_AUTOCOMPLETE_MAX_COUNT = 5
local VIP_ITEM_MAX_COUNT = 5


local _alreadyInGroup = false

Teammaking_V2 = {}

Teammaking_V2.TeamMembers = {}
Teammaking_V2.supressDialog = false

local _inviteCandidatesList = {}
local _onlineFriendsList = {}
local _autoCompleteList = {}
local _invitedList = {}

local _userList = {}
local _memberInfoCache = {}

local _detailDelay = nil

local _uiType = -1
local _uiDisplayConfig = {}
table.insert(_uiDisplayConfig, {target='lobby', hide=true})
table.insert(_uiDisplayConfig, {target='player_ladder', hide=true})
table.insert(_uiDisplayConfig, {target='player_tour_stats', hide=true})
table.insert(_uiDisplayConfig, {target='hosted_game', hide=true})
table.insert(_uiDisplayConfig, {target='queue_loading', hide=true})
table.insert(_uiDisplayConfig, {target='watch_system', hide=true})
table.insert(_uiDisplayConfig, {target='loading_bg', hide=true})
table.insert(_uiDisplayConfig, {target='match_stats', hide=true})
table.insert(_uiDisplayConfig, {target='playerstats', hide=true})
table.insert(_uiDisplayConfig, {target='store_container2', hide=true})
table.insert(_uiDisplayConfig, {target='map_selection_large', hidebutton=true, x='-530i'})
table.insert(_uiDisplayConfig, {target='map_selection_small', hidebutton=true, x='-440i'})

function Teammaking_V2:Init()
	local widget = GetWidget('teammaking_trigger_help')
	widget:RegisterWatch("TMMPlayerStatus0", function(_, ...) Teammaking_V2:OnTMMPlayerStatus(0, ...) end)
	widget:RegisterWatch("TMMPlayerStatus1", function(_, ...) Teammaking_V2:OnTMMPlayerStatus(1, ...) end)
	widget:RegisterWatch("TMMPlayerStatus2", function(_, ...) Teammaking_V2:OnTMMPlayerStatus(2, ...) end)
	widget:RegisterWatch("TMMPlayerStatus3", function(_, ...) Teammaking_V2:OnTMMPlayerStatus(3, ...) end)
	widget:RegisterWatch("TMMPlayerStatus4", function(_, ...) Teammaking_V2:OnTMMPlayerStatus(4, ...) end)
	widget:RegisterWatch("PlayerSimpleStats", function(_, ...) Teammaking_V2:OnPlayerSimpleStats(arg) end)
	widget:RegisterWatch('SelectUpgradesStatus', function(_, ...) Teammaking_V2:OnSelectUpgradesStatus(...) end)
	widget:RegisterWatch('MatchInfoSummary', function(_, ...) Teammaking_V2:UpdateAllMembers() end)
	widget:RegisterWatch("TMMReset", function(_, ...) Teammaking_V2:OnTMMReset() end)
end

function Teammaking_V2:Login()

	Teammaking_V2.TeamMembers = {}

	Teammaking_V2:UpdateAllMembers()
	GetWidget('teammaking_invite'):SetVisible(false)
end

function Teammaking_V2:UpdateSingleMember(index)
	local memberInfo = Teammaking_V2.TeamMembers[index]

	if index == 1 and not IsInGroup() then 
		memberInfo = {}
		memberInfo.name = GetAccountName()
		memberInfo.isLeader = false
	end
	
	if memberInfo == nil or Empty(memberInfo.name) then
		GetWidget('teammaking_member_'..index..'_me'):SetVisible(false)
		GetWidget('teammaking_member_'..index..'_other'):SetVisible(false)
		GetWidget('teammaking_member_'..index..'_empty'):SetVisible(true)
		GetWidget('teammaking_member_'..index..'_other_leader'):SetVisible(false)
	elseif memberInfo.name == GetAccountName() then 
		GetWidget('teammaking_member_'..index..'_me'):SetVisible(true)
		GetWidget('teammaking_member_'..index..'_other'):SetVisible(false)
		GetWidget('teammaking_member_'..index..'_empty'):SetVisible(false)

		local iconTable = Explode( "|", GetChatClientInfo(StripClanTag(memberInfo.name), "getaccounticontexturepath"))
		if (iconTable[1]) then 
			GetWidget('teammaking_member_'..index..'_me_icon'):SetTexture(iconTable[1]) 
		end

		GetWidget('teammaking_member_'..index..'_me_leader'):SetVisible(memberInfo.isLeader)
	else
		GetWidget('teammaking_member_'..index..'_me'):SetVisible(false)
		GetWidget('teammaking_member_'..index..'_other'):SetVisible(true)
		GetWidget('teammaking_member_'..index..'_empty'):SetVisible(false)

		local iconTable = Explode( "|", GetChatClientInfo(StripClanTag(memberInfo.name), "getaccounticontexturepath"))
		if (iconTable[1]) then 
			GetWidget('teammaking_member_'..index..'_other_icon'):SetTexture(iconTable[1]) 
		end
		GetWidget('teammaking_member_'..index..'_other_leader'):SetVisible(memberInfo.isLeader)
	end

	local isInQueue = AtoB(interface:UICmd("IsInQueue"))
	GetWidget('teammaking_member_'..index..'_empty'):SetEnabled(not isInQueue)
end

function Teammaking_V2:GetTeamSize()
	local memberCount = 0
	for i,v in ipairs(Teammaking_V2.TeamMembers) do
		if v.accountID ~= nil then 
			memberCount = memberCount + 1
		end
	end

	if memberCount == 0 then memberCount = 1 end

	return memberCount
end

function Teammaking_V2:AllMembersReady()
	if not IsInGroup() then return true end

	local ready = true
	for i,v in ipairs(Teammaking_V2.TeamMembers) do
		if v.accountID ~= nil and not v.isLeader and not v.isReady then 
			ready = false
			break
		end
	end

	return ready
end

function Teammaking_V2:UpdateAllMembers()
	for i=1,TEAMMEMBER_MAX_COUNT do
		Teammaking_V2:UpdateSingleMember(i)
	end	

	if not IsInGroup() then 
		_alreadyInGroup = false
	end

	local newCache = {}
	newCache[string.lower(StripClanTag(GetAccountName()))] = _memberInfoCache[string.lower(StripClanTag(GetAccountName()))]
	for i=1,TEAMMEMBER_MAX_COUNT do
		if Teammaking_V2.TeamMembers[i] ~= nil and NotEmpty(Teammaking_V2.TeamMembers[i].name) then
			newCache[string.lower(StripClanTag(Teammaking_V2.TeamMembers[i].name))] = _memberInfoCache[string.lower(StripClanTag(Teammaking_V2.TeamMembers[i].name))]
		end
	end	
	_memberInfoCache = newCache
end

function Teammaking_V2:OnTMMPlayerStatus(id, ...)
	--[[ params
		0 = uiAccountID
		1 = sName
		2 = ySlot
		3 = nRating
		4 = yLoadingPercent
		5 = yReadyStatus
		6 = isLeader
		7 = isValidIndex
		8 = bVerified
		9 = bFriend
		10 = uiChatNameColorString
		11 = GetChatNameColorTexturePath(uiChatNameColorString)
		12 = bGameModeAccess
		13 = GetChatNameGlow(uiChatNameColor)
		14 = sGameModeAccess
		15 = Ingame
		16 = GetChatNameGlowColorString
		17 = GetChatNameGlowColorIngameString
		18 = Normal RankLevel
		19 = Casual RankLevel
		20 = Normal Ranking
		21 = Casual Ranking
		22 = bEligibleForCampaign
	]]
	local accountID, name, slot, mmr, _, isReady, isLeader, isValidSlot, isVerified, isFriend, nameColor, _, isModeCompat, nameGlow, modeAccess, inGame, nameGlowColorIngame, rankLevel_normal, rankLevel_casual, ranking_normal, ranking_casual, eligible_for_campaign = ...
	--Echo('^yOnTMMPlayerStatus -------------- '.. id .. ': '..accountID..', '..isLeader..','..isReady)

	local member = {}
	member.name = name
	member.accountID = tonumber(accountID)
	member.isLeader = AtoB(isLeader)
	member.slot = tonumber(slot)
	member.isReady = AtoB(isReady)
	member.mmr = tonumber(mmr)

	Teammaking_V2.TeamMembers[id+1] = member
	if UIGetAccountID() == accountID and id > 0 then
		local temp = Teammaking_V2.TeamMembers[1]
		Teammaking_V2.TeamMembers[1] = Teammaking_V2.TeamMembers[id+1]
		Teammaking_V2.TeamMembers[id+1] = temp
	end

	-- if UIGetAccountID() == accountID and not AtoB(isLeader) and not AtoB(isReady) then
	-- 	MapSelection_V2:ReadyTeam()
	-- end

	if UIGetAccountID() == accountID then
		if not _alreadyInGroup and not AtoB(isLeader) and (not GetWidget('map_selection'):IsVisible() or GetWidget('hosted_game'):IsVisible()) then 
			MapSelection_V2:Show()

			if GetWidget('store_container2'):IsVisible() then
				HoN_Store:CloseStoreCommand()
			end
			Player_Stats_V2:Hide()
			Player_Hosted_V2:Hide()
		end
		_alreadyInGroup = true
	end
	

	if Teammaking_V2:GetTeamSize() >= TEAMMEMBER_MAX_COUNT then 
		GetWidget('teammaking_invite'):FadeOut(150)
	end


	Teammaking_V2:UpdateAllMembers()

	if id == 4 and not Teammaking_V2:HasLeader() then
		Teammaking_V2:LeaveGroup()
	end
end

function Teammaking_V2:OnSelectUpgradesStatus(...)
	if tonumber(arg[1]) == 2 then
		interface:Sleep(3000, function() Teammaking_V2:UpdateAllMembers() end)
	end
end

function Teammaking_V2:OnTMMReset()
	GetWidget('mm_popup_joinmatch'):SetVisible(false)
end

------------------------------------------------------------------ Invite --------------------------------------------------
function Teammaking_V2:CanInvite(name)
	if Empty(name) or string.lower(StripClanTag(name)) == string.lower(StripClanTag(GetAccountName())) then 
		return false
	end

	for i,v in ipairs(Teammaking_V2.TeamMembers) do
		if string.lower(StripClanTag(name)) == string.lower(StripClanTag(v.name)) then
			return false
		end
	end

	return true
end

function Teammaking_V2:BuildOnlineFriendList()
	local input = GetWidget('teammaking_invite_search'):GetValue()

	_onlineFriendsList = {}
	for _,v in ipairs(Social_V2.ClanList) do
		local friend = {}
		friend.name = StripClanTag(v.name)
		friend.status = v.status
		friend.icon = v.icon
		friend.isFriend = true

		if (Empty(input) or string.find(string.lower(friend.name), string.lower(input), 1, true)) and Teammaking_V2:CanInvite(friend.name) and friend.status ~= 'offline' then 
			table.insert(_onlineFriendsList, friend)
		end
	end

	--if Social_V2.PlayerList['friends'] == nil then return end

	for group_name, group in pairs(Social_V2.PlayerList) do

		if group_name ~= 'recent' and group_name ~= 'inactive' and group_name ~= 'offline' then
			for k,v in pairs(group) do
				local friend = {}
				friend.name = StripClanTag(v.name)
				friend.status = v.status
				friend.icon = v.accountIcon
				friend.isFriend = true

				if (Empty(input) or string.find(string.lower(friend.name), string.lower(input), 1, true)) and Teammaking_V2:CanInvite(friend.name) and friend.status ~= 'offline' then 
					table.insert(_onlineFriendsList, friend)
				end
			end
		end
	end
end

function Teammaking_V2:MergeCandidatesList()
	local existName = {}
	_inviteCandidatesList = {}

	for i,v in ipairs(_onlineFriendsList) do
		if existName[string.lower(v.name)] == nil then
			table.insert(_inviteCandidatesList, v)
			existName[string.lower(v.name)] = true
		end
	end

	for i,v in ipairs(_autoCompleteList) do
		if existName[string.lower(v.name)] == nil then
			table.insert(_inviteCandidatesList, v)
			existName[string.lower(v.name)] = true
		end
	end

	table.sort(_inviteCandidatesList, function(a, b)
		local valueA = 0
		local valueB = 0

		if a.status == 'online' then 
			valueA = 6
		elseif a.status == 'ingame' then
			valueA = 4
		elseif a.status == 'offline' then 
			valueA = 2
		else
			valueA = 0
		end

		if a.isFriend then 
			valueA = valueA + 1
		end

		if b.status == 'online' then 
			valueB = 6
		elseif b.status == 'ingame' then
			valueB = 4
		elseif b.status == 'offline' then 
			valueB = 2
		else
			valueB = 0
		end

		if b.isFriend then 
			valueB = valueB + 1
		end

		if valueA ~= valueB then 
			return valueA > valueB
		else
			return a.name < b.name
		end
	end)

	-- for i,v in ipairs(_inviteCandidatesList) do
	-- 	Echo('^g~~~~~~~~~~~~~~~~~~~~~~~')
	-- 	Echo(v.name)
	-- 	Echo(v.status)
	-- 	Echo(v.icon)
	-- end
end

function Teammaking_V2:ToggleInvitePanel()
	local widget = GetWidget('teammaking_invite')

	if widget:IsVisible() then 
		widget:FadeOut(100)
	else
		widget:FadeIn(150)

		_autoCompleteList = {}
		_userList = {}
		_invitedList = {}
		Teammaking_V2:BuildOnlineFriendList()
		Teammaking_V2:UpdateInvitePanel()
	end
end

function Teammaking_V2:InvitePlayer(name)
	local function inviteHelper(...)
		GetWidget('teammaking_invite'):UnregisterWatch("TMMDisplay")
		interface:UICmd("InviteToTMMGroup('"..StripClanTag(name).."');")
		Trigger("ChatAutoCompleteClear")
	end

	if Teammaking_V2:CanInvite(name) then
		if (not IsInGroup()) then
			local mapInfo = MapSelection_V2:CreateGroup(false)
			if mapInfo ~= nil then
				GetWidget('teammaking_invite'):RegisterWatch("TMMDisplay", inviteHelper)
			end
		else
			interface:UICmd("InviteToTMMGroup('"..StripClanTag(name).."');")

			if Teammaking_V2:GetTeamSize() >= (TEAMMEMBER_MAX_COUNT - 1) then
				GetWidget('teammaking_invite'):FadeOut(100)
			else
				Trigger('ChatAutoCompleteClear')
			end
		end

		_invitedList[string.lower(StripClanTag(name))] = true
	end
end

function Teammaking_V2:UpdateInvitePanel()
	Teammaking_V2:BuildOnlineFriendList()
	Teammaking_V2:MergeCandidatesList()

	if #_inviteCandidatesList == 0 then
		GetWidget('teammaking_invite_autocomplete'):SetVisible(false)
		GetWidget('teammaking_invite_noautocomplete'):SetVisible(true)
	else
		GetWidget('teammaking_invite_autocomplete'):SetVisible(true)
		GetWidget('teammaking_invite_noautocomplete'):SetVisible(false)

		local maxValue = 1
		if #_inviteCandidatesList > INVITE_AUTOCOMPLETE_MAX_COUNT then 
			maxValue = #_inviteCandidatesList - INVITE_AUTOCOMPLETE_MAX_COUNT + 1
		end
		
		GetWidget('teammaking_invite_autocompelete_scroll'):SetMaxValue(maxValue)
		GetWidget('teammaking_invite_autocompelete_scroll'):SetValue(1)
		Teammaking_V2:UpdateInviteAutoCompleteList()
	end
end

function Teammaking_V2:UpdateInviteAutoCompleteList()
	local scroll = GetWidget('teammaking_invite_autocompelete_scroll')
	local startIndex = tonumber(scroll:GetValue())
	local maxValue = tonumber(scroll:UICmd("GetScrollbarMaxValue()"))

	if startIndex < 1 then startIndex = 1 end
	if startIndex > maxValue then startIndex = maxValue end

 	for i=1,INVITE_AUTOCOMPLETE_MAX_COUNT do
 		local item = _inviteCandidatesList[startIndex + i - 1]
 		
 		if item ~= nil then
 			GetWidget('teammaking_search_autocomplete_'..i):SetVisible(true)
 			GetWidget('teammaking_search_autocomplete_'..i..'_name'):SetText(item.name)

 			if item.isFriend then
 				GetWidget('teammaking_search_autocomplete_'..i..'_name'):SetColor('#f0d3c1')
 			else
 				GetWidget('teammaking_search_autocomplete_'..i..'_name'):SetColor('#a99081')
 			end

 			GetWidget('teammaking_search_autocomplete_'..i..'_name'):SetY('-8i')
 			if item.status == 'online' then
 				GetWidget('teammaking_search_autocomplete_'..i..'_status_icon'):SetTexture('/ui/fe2/newui/res/social/online.png') 
 				GetWidget('teammaking_search_autocomplete_'..i..'_status_text'):SetText(Translate('sp_listlabel_online'))
 			elseif item.status == 'ingame' then
 				GetWidget('teammaking_search_autocomplete_'..i..'_status_icon'):SetTexture('/ui/fe2/newui/res/social/ingame.png') 
 				GetWidget('teammaking_search_autocomplete_'..i..'_status_text'):SetText(Translate('sp_listlabel_ingame'))
 			elseif item.status == 'offline' then
 				GetWidget('teammaking_search_autocomplete_'..i..'_status_icon'):SetTexture('/ui/fe2/newui/res/social/offline.png') 
 				GetWidget('teammaking_search_autocomplete_'..i..'_status_text'):SetText(Translate('sp_listlabel_offline'))
 			else
 				GetWidget('teammaking_search_autocomplete_'..i..'_name'):SetY('0')
 				GetWidget('teammaking_search_autocomplete_'..i..'_status_icon'):SetTexture('$invis')
 				GetWidget('teammaking_search_autocomplete_'..i..'_status_text'):SetText('')
 			end

 			if NotEmpty(item.icon) then
 				GetWidget('teammaking_search_autocomplete_'..i..'_icon'):SetTexture(item.icon)
 			else
 				GetWidget('teammaking_search_autocomplete_'..i..'_icon'):SetTexture('/ui/fe2/store/icons/account_icons/default.tga')
 			end

 			GetWidget('teammaking_search_autocomplete_'..i..'_invited'):SetVisible(_invitedList[string.lower(item.name)] ~= nil)
 		else
 			GetWidget('teammaking_search_autocomplete_'..i):SetVisible(false)
 		end
 	end
end

function Teammaking_V2:OnChatAutoCompleteAdd(name)
	local input = GetWidget('teammaking_invite_search'):GetValue()
	if Empty(input) or string.lower(string.sub(name, 1, string.len(input))) ~= string.lower(input) or not Teammaking_V2:CanInvite(name) then 
		return
	end

	for i,v in ipairs(_autoCompleteList) do
		if string.lower(v.name) == string.lower(name) then 
			return
		end
	end

	if _userList[name] == nil then 
		local userInfo = GetChatClientInfo(name, "getaccounticontexturepath|chatnamecolorstring|chatnamecoloringamestring|matchid|chatnameglow|chatnameglowcolorstring|chatnameglowcoloringamestring")
		if NotEmpty(userInfo) then
			local user = {}
			infoTable = Explode("|", userInfo)

			user.name = name
			if (infoTable[4] == "4294967295") then
				user.status = "online"
			else
				user.status = "ingame"
			end
			user.icon = infoTable[1]

			_userList[name] = user
		end
	end

	local player = _userList[name] or {name=name, status='', icon='', isFriend=false}
	local friendInfo = Social_V2:GetFriendInfo(player.name)

	if friendInfo ~= nil then
		player.status = friendInfo.status
		player.icon = friendInfo.accountIcon
		player.isFriend = true
	elseif Social_V2.ClanList[player.name] ~= nil then
		player.status = Social_V2.ClanList[player.name].status
		player.icon = Social_V2.ClanList[player.name].icon
		player.isFriend = true
	end

	table.insert(_autoCompleteList, player)
	Teammaking_V2:UpdateInvitePanel()
end

function Teammaking_V2:OnChatAutoCompleteClear()
	_autoCompleteList = {}
	Teammaking_V2:UpdateInvitePanel()
end

function Teammaking_V2:OnClickInvite()
	local name = GetWidget("teammaking_invite_search"):GetValue()
	if NotEmpty(name) then 
		Teammaking_V2:InvitePlayer(name)
		GetWidget('teammaking_invite_search'):EraseInputLine()
	end
end

function Teammaking_V2:OnClickAutoComplete(index)
	local name = GetWidget('teammaking_search_autocomplete_'..index..'_name'):GetValue()
	if NotEmpty(name) then 
		GetWidget("teammaking_invite_search"):Sleep(235, function()
			GetWidget("teammaking_invite_search"):UICmd("SetInputLine('"..name.."');")
		end)
	end
end

function Teammaking_V2:OnDoubleClickAutoComplete(index)
	local name = GetWidget('teammaking_search_autocomplete_'..index..'_name'):GetValue()
	if NotEmpty(name) then 
		GetWidget("teammaking_invite_search"):Sleep(1, function()
			Teammaking_V2:InvitePlayer(name)
			GetWidget("teammaking_invite_search"):EraseInputLine()
		end)
	end
end

function Teammaking_V2:OnTextChange()
	local name = GetWidget("teammaking_invite_search"):GetValue()

	if string.len(name) >= 5 then
		ChatAutoCompleteNick(StripClanTag(name))
	else
		Trigger('ChatAutoCompleteClear')
	end

	if string.len(name) > 0 then
		GetWidget('teammaking_invite_enter_name'):SetVisible(false)
		GetWidget('teammaking_invite_doinvite'):SetEnabled(Teammaking_V2:CanInvite(name))
	else
		GetWidget('teammaking_invite_doinvite'):SetEnabled(false)
	end
end

function Teammaking_V2:OnClickPlay()
	local isInQueue = AtoB(interface:UICmd("IsInQueue"))
	if isInQueue then 
		Loading_V2:ExpandQueueLoading()
	else
		MapSelection_V2:Show()
	end
end

function Teammaking_V2:LeaveGroup()
	interface:UICmd("LeaveTMMGroup();")
end

function Teammaking_V2:KickTeamMember(index)
	local memberInfo = Teammaking_V2.TeamMembers[index]
	if memberInfo == nil or Empty(memberInfo.name) then return end

	interface:UICmd("KickFromTMMGroup("..memberInfo.slot..");")
end

function Teammaking_V2:OnClickTeamMember(index)
	local name = GetAccountName()
	if IsInGroup() then
		name = Teammaking_V2.TeamMembers[index].name
	end

	if Empty(name) then return end

	Player_Stats_V2:Show(name)
end

function Teammaking_V2:OnRightClickTeamMember(index)
	local memberInfo = {name=GetAccountName(), isLeader=false}

	if IsInGroup() then
		memberInfo = Teammaking_V2.TeamMembers[index]
	end

	if memberInfo == nil or Empty(memberInfo.name) then return end

	local menutable = {}
	local isMe = IsMe(memberInfo.name)
	local iamLeader = IsInGroup() and Teammaking_V2.TeamMembers[1].isLeader

	-- Send Message
	if not isMe then
		menu = {}
		menu.content = 'newui_communicator_sendmessage'
		menu.onclicklua = 'Communicator_V2:StartChatWithPlayer(\''..StripClanTag(memberInfo.name)..'\', true)'
		table.insert(menutable, menu)
	end

	-- View Stats
	menu = {}
	menu.content = 'ui_items_cc_right_click_view_stats'
	menu.onclicklua = 'Player_Stats_V2:Show(\''..StripClanTag(memberInfo.name)..'\')'
	table.insert(menutable, menu)

	-- Add Friend
	if not IsBuddy(StripClanTag(memberInfo.name)) and not isMe then
		menu = {}
		menu.content = 'ui_items_cc_right_click_add_buddy'
		menu.onclicklua = 'Common_V2:AddFriend(\''..StripClanTag(memberInfo.name)..'\')'
		table.insert(menutable, menu)
	-- elseif not isMe then
	-- 	menu = {}
	-- 	menu.content = 'newui_general_removefriend'
	-- 	menu.onclicklua = 'Common_V2:RemoveFriend(\''..StripClanTag(memberInfo.name)..'\')'
	-- 	table.insert(menutable, menu)
	end

	if IsInGroup() and isMe then
		menu = {}
		if memberInfo.isLeader then
			menu.content = 'newui_teammaking_dismissteam'
		else
			menu.content = 'newui_teammaking_quitteam'
		end
		menu.onclicklua = 'Teammaking_V2:LeaveGroup()'
		table.insert(menutable, menu)
	end

	if not isMe and iamLeader then
		menu = {}
		menu.content = 'Kick'
		menu.onclicklua = 'Teammaking_V2:KickTeamMember('..index..')'
		table.insert(menutable, menu)
	end

	Common_V2:PopupMenu(menutable)
end

function Teammaking_V2:OnClickLeaveQueue()
	if IsInQueue() then
		local iamLeader = IsInGroup() and Teammaking_V2.TeamMembers[1].isLeader
		if iamLeader then
			interface:UICmd('LeaveTMMQueue();')
		else
			Teammaking_V2:LeaveGroup()
		end
	end
end

function Teammaking_V2:OnFrame()
	if GetCurrentGamePhase() > 0 then 
		GetWidget('teammaking'):SetVisible(false)
		_uiType = -3
		return
	end

	local matched = false
	for i,v in ipairs(_uiDisplayConfig) do
		if GetWidget(v.target):IsVisible() then
			if _uiType ~= i then 
				if v.hide then 
					GetWidget('teammaking'):SetVisible(false)
				else
					GetWidget('teammaking'):SetVisible(true)
					GetWidget('teammaking_logo'):SetVisible(not v.hidebutton)
					-- GetWidget('main_playerinfo_play'):SetVisible(not v.hidebutton)
					GetWidget('teammaking'):SetX(v.x)
				end
			end

			_uiType = i
			matched = true
			break
		end
	end
	if not matched then
		if _uiType ~= -1 then
			GetWidget('teammaking'):SetX('-440i')
			GetWidget('teammaking'):SetVisible(true)
			GetWidget('teammaking_logo'):SetVisible(true)
			-- GetWidget('main_playerinfo_play'):SetVisible(true)
		end
		_uiType = -1
	end

	if _detailDelay ~= nil then
		if _memberInfoCache[string.lower(_detailDelay.name)] ~= nil then 
			Teammaking_V2:OnPlayerSimpleStats(_memberInfoCache[string.lower(_detailDelay.name)])
			_detailDelay = nil
		elseif not _detailDelay.alreadySend then
			RequestPlayerSimpleStats(_detailDelay.name or '')
			_detailDelay.alreadySend = true 
		end
	end
end

function Teammaking_V2:UpdateLeaverBlock()
	if (IsLeaver()) then
		if (not GetWidget("matchmaking_blocker_leaver"):IsVisibleSelf()) and (not HoN_Region.regionTable[HoN_Region.activeRegion].tmmAllowLeavers) then
			PlaySound('/shared/sounds/announcer/denied.wav')
			GetWidget("matchmaking_blocker_leaver"):SetVisible(true)
			Trigger("TMMReset")
		end
	else
		GetWidget("matchmaking_blocker_leaver"):SetVisible(false)
	end
end

function Teammaking_V2:HasLeader()
	local hasLeader = false
	for i,v in ipairs(Teammaking_V2.TeamMembers) do
		if v.isLeader then 
			hasLeader = true
			break
		end
	end
	return hasLeader
end
--------------------------------------- Member Info Detail ----------------------------------------------
function Teammaking_V2:OnMouseOverTeamMember(index)
	local memberInfo = {name=GetAccountName(), isLeader=false}
	if IsInGroup() then
		memberInfo = Teammaking_V2.TeamMembers[index]
	end
	if memberInfo == nil or Empty(memberInfo.name) or index == 1 then return end

	_detailDelay = {}
	_detailDelay.alreadySend = false
	_detailDelay.name = StripClanTag(memberInfo.name)
end

function Teammaking_V2:OnMouseOutTeamMember(index)
	GetWidget('teammaking_info_floater'):SetVisible(false)
	_detailDelay = nil 
end

function Teammaking_V2:OnPlayerSimpleStats(info)
	-- Logan (2026-02-12): Fix for Modern Server Response (Table vs Positional)
	if type(info[1]) == 'table' then
		local params = {}
		for k,v in pairs(info[1]) do
			if type(k) == 'number' then
				params[k+1] = v
			end
		end
		-- Ensure strict nil checks don't break logic
		setmetatable(params, {__index = function(t,k) return nil end})
		info = params
	end

	_memberInfoCache[string.lower(StripClanTag(info[1]))] = info

	if _detailDelay == nil or GetWidget('newui_popup_menu'):IsVisible() then return end
	GetWidget('teammaking_info_floater'):SetVisible(true)

	local nickName = info[1]
	local level = tonumber(info[2])
	local level_exp = tonumber(info[3])
	local heroes_num = info[4]
	local avatars_num = info[5]
	local matches_num = info[6]
	local mvp_num = tonumber(info[7])
	local selected_upgrades = info[8]
	local account_id = tonumber(info[9])
	local season_id = info[10]
	local mvp_startindex = 19
	local mvp_awards_name = {info[mvp_startindex], info[mvp_startindex+1], info[mvp_startindex+2], info[mvp_startindex+3]}
	local mvp_awards_number = {info[mvp_startindex+4], info[mvp_startindex+5], info[mvp_startindex+6], info[mvp_startindex+7]}
	local normal_wins = info[11]
	local normal_losses = info[12]
	local normal_winstreaks = info[13]
	local normal_level = info[14]
	local casual_wins = info[15]
	local casual_losses = info[16]
	local casual_winstreaks = info[17]
	local casual_level = info[18]



	---------------------------------------------------------------------------------------
	-----------------------------  General Information ------------------------------------
	---------------------------------------------------------------------------------------
	local widget = GetWidget('teammaking_info_floater_playericon')
	widget:SetTexture('/ui/common/ability_coverup.tga')

	local playerIcon = GetAccountIconTexturePathFromUpgrades(selected_upgrades, account_id)
	if NotEmpty(playerIcon) then
		widget:SetTexture(playerIcon)
	else
		widget:SetAvatar('http://www.heroesofnewerth.com/getAvatar.php?id='..account_id)
	end
	------------------------------------------------------------------------------------------

	widget = GetWidget('teammaking_info_floater_playername')
	local nameColor = GetChatNameColorStringFromUpgrades(selected_upgrades)
	local nameColorFont = GetChatNameColorFontFromUpgrades(selected_upgrades)
	local font = NotEmpty(nameColorFont) and nameColorFont..'_16' or 'dyn_16'

	widget:SetFont(font)
	widget:SetColor(NotEmpty(nameColor) and nameColor or '#efd2c0')
	widget:SetGlow(GetChatNameGlowFromUpgrades(selected_upgrades))
	widget:SetGlowColor(GetChatNameGlowColorStringFromUpgrades(selected_upgrades))
	widget:SetBackgroundGlow(GetChatNameBackgroundGlowFromUpgrades(selected_upgrades))
	widget:SetText(font == '8bit_16' and string.upper(nickName) or nickName)
	--------------------------------------------------------------------------------------------------

	widget = GetWidget('teammaking_info_floater_chatsymbol')
	local chatSymbol = GetChatSymbolTexturePathFromUpgrades(selected_upgrades)
	local nameColor = GetChatNameColorTexturePathFromUpgrades(selected_upgrades)

	if NotEmpty(chatSymbol) then
		widget:SetTexture(chatSymbol)
	elseif NotEmpty(nameColor) then
		widget:SetTexture(nameColor)
	else
		widget:SetTexture('$invis')
	end
	--------------------------------------------------------------------------------------------------
	local levelExp = tonumber(interface:UICmd('GetAccountExperienceForLevel('..level..')'))
	local nextLevelExp = tonumber(interface:UICmd('GetAccountExperienceForNextLevel(\''..level_exp..'\')'))
	local nextLevelPercent = tonumber(interface:UICmd('GetAccountPercentNextLevel(\''..level_exp..'\')'))
	GetWidget('teammaking_info_floater_playerlevel'):SetText(Translate('newui_playerstats_level', 'level', tostring(level)))
	GetWidget('teammaking_info_floater_levelpercent'):SetWidth(tostring(nextLevelPercent*100)..'%')
	GetWidget('teammaking_info_floater_levelexp'):SetText(tostring(level_exp - levelExp)..'/'..(nextLevelExp) - levelExp)

	---------------------------------------------------------------------------------------
	-----------------------------  VIP Information ----------------------------------------
	---------------------------------------------------------------------------------------
	GetWidget('teammaking_info_floater_mvp_1_icon'):SetTexture('/ui/fe2/newui/res/playerstats/mvpawards/awd_mvp_big.png')
	GetWidget('teammaking_info_floater_mvp_1_text'):SetText('X '.. mvp_num)
	for i=2, VIP_ITEM_MAX_COUNT do
		GetWidget('teammaking_info_floater_mvp_'..i..'_icon'):SetTexture('/ui/fe2/newui/res/playerstats/mvpawards/'..mvp_awards_name[i-1]..'_big.png')
		GetWidget('teammaking_info_floater_mvp_'..i..'_text'):SetText('X '.. mvp_awards_number[i-1])
	end

	---------------------------------------------------------------------------------------
	-----------------------------  Other Information ----------------------------------------
	---------------------------------------------------------------------------------------
	GetWidget('teammaking_info_floater_other_matches_value'):SetText(matches_num)
	GetWidget('teammaking_info_floater_other_avatars_value'):SetText(avatars_num)
	if GetCvarBool('cl_GarenaEnable') then
		GetWidget('teammaking_info_floater_other_heroes_value'):SetText(heroes_num)
	end

	---------------------------------------------------------------------------------------
	-----------------------------  CoN Information ----------------------------------------
	---------------------------------------------------------------------------------------
 	if tonumber(normal_level) > 0 then
 		GetWidget('teammaking_info_floater_con_normal'):SetVisible(true)

 		GetWidget('teammaking_info_floater_con_normal_icon'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevelAfterS6(tonumber(normal_level)))
 		GetWidget('teammaking_info_floater_con_normal_wins'):SetText(normal_wins)
 		GetWidget('teammaking_info_floater_con_normal_losses'):SetText(normal_losses)
 		GetWidget('teammaking_info_floater_con_normal_winstreaks'):SetText(normal_winstreaks)
 		GetWidget('teammaking_info_floater_con_normal_title'):SetText(Translate('newui_teammaking_normalseason', 'id', season_id))
 		GetWidget('teammaking_info_floater_con_normal_level'):SetText(Translate('player_compaign_level_name_S7_'..normal_level))

 	else
 		GetWidget('teammaking_info_floater_con_normal'):SetVisible(false)
 	end

 	if tonumber(casual_level) > 0 then
 		GetWidget('teammaking_info_floater_con_casual'):SetVisible(true)

 		GetWidget('teammaking_info_floater_con_casual_icon'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevelAfterS6(tonumber(casual_level)))
 		GetWidget('teammaking_info_floater_con_casual_wins'):SetText(casual_wins)
 		GetWidget('teammaking_info_floater_con_casual_losses'):SetText(casual_losses)
 		GetWidget('teammaking_info_floater_con_casual_winstreaks'):SetText(casual_winstreaks)
 		GetWidget('teammaking_info_floater_con_casual_title'):SetText(Translate('newui_teammaking_casualseason', 'id', season_id))
 		GetWidget('teammaking_info_floater_con_casual_level'):SetText(Translate('player_compaign_level_name_S7_'..casual_level))

 	else
 		GetWidget('teammaking_info_floater_con_casual'):SetVisible(false)
 	end
end