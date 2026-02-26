----------------------------------------------------------
--	Name: 		Notifications Script	            	--
-- 	Version: 	3.0.0 r1								--
--  Copyright 2015 Frostburn Studios					--
----------------------------------------------------------
local _G = getfenv(0)
HoN_Notifications = {}
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort
local interface, interfaceName = object, object:GetName()
RegisterScript2('Notifications', '32')
HoN_Notifications.notificationsTable = {}
HoN_Notifications.toastTable = {}
HoN_Notifications.toastWidgetActive = 0
HoN_Notifications.toastWidgetTable = {[1] = {isAvailable = true}, [2] = {isAvailable = true}, [3] = {isAvailable = true}, [4] = {isAvailable = true}, [5] = {isAvailable = true}, [6] = {isAvailable = true}, [7] = {isAvailable = true}, [8] = {isAvailable = true}, [9] = {isAvailable = true}}
HoN_Notifications.toastPauseAnimationDuration = 0
HoN_Notifications.toastPauseAnimationLast = 0
HoN_Notifications.lastUserOffset = 0
HoN_Notifications.currentNotification = 0
HoN_Notifications.currentNotificationOffset = 0
HoN_Notifications.historySize = 0
HoN_Notifications.MAX_POPUP_NOTIFICATIONS_FRIEND = 3
HoN_Notifications.visibleHistorySlots = 6
HoN_Notifications.userUpgradesTable = {}
local HOVER_DELAY = 850
local HOVER_DELAY_SHORT = 250
local MAXIMUM_NOTIFICATIONS = 1000

local rap2Enable = GetCvarBool('cl_Rap2Enable')

--[[notificaiton type from c++
enum ENotifyType
{
	NOTIFY_TYPE_UNKNOWN,
	NOTIFY_TYPE_BUDDY_ADDER,
	NOTIFY_TYPE_BUDDY_ADDED,
	NOTIFY_TYPE_BUDDY_REMOVER,	// DISABLED
	NOTIFY_TYPE_BUDDY_REMOVED,	// DISABLED
	NOTIFY_TYPE_CLAN_RANK,
	NOTIFY_TYPE_CLAN_ADD,
	NOTIFY_TYPE_CLAN_REMOVE,
	NOTIFY_TYPE_BUDDY_ONLINE,
	NOTIFY_TYPE_BUDDY_LEFT_GAME,
	NOTIFY_TYPE_BUDDY_OFFLINE,
	NOTIFY_TYPE_BUDDY_JOIN_GAME,
	NOTIFY_TYPE_CLAN_ONLINE,
	NOTIFY_TYPE_CLAN_LEFT_GAME,
	NOTIFY_TYPE_CLAN_OFFLINE,
	NOTIFY_TYPE_CLAN_JOIN_GAME,
	NOTIFY_TYPE_CLAN_WHISPER,	// UNUSED
	NOTIFY_TYPE_UPDATE,
	NOTIFY_TYPE_GENERIC,
	NOTIFY_TYPE_IM,
	NOTIFY_TYPE_GAME_INVITE,
	NOTIFY_TYPE_SELF_JOIN_GAME,
	NOTIFY_TYPE_BUDDY_REQUESTED_ADDER,
	NOTIFY_TYPE_BUDDY_REQUESTED_ADDED,
	NOTIFY_TYPE_TMM_GROUP_INVITE,
	NOTIFY_TYPE_ACTIVE_STREAM,
	NOTIFY_TYPE_REPLAY_AVAILABLE,
	NOTIFY_TYPE_CLOUDSAVE_UPLOAD_SUCCESS,
	NOTIFY_TYPE_CLOUDSAVE_UPLOAD_FAIL,
	NOTIFY_TYPE_CLOUDSAVE_DOWNLOAD_SUCCESS,
	NOTIFY_TYPE_CLOUDSAVE_DOWNLOAD_FAIL,

	NUM_NOTIFICATIONS
};

--]]

local soundsByType = {
	[1] = 'NotifyAddedAsBuddy',
	[2] = 'NotifyBuddyAdded',
	[3] = 'NotifyBuddyRemoved',
	[4] = 'NotifyBuddyRemoved',
	[5] = 'NotifyClanRank',
	[6] = 'NotifyClanAdd',
	[7] = 'NotifyClanRemove',
	[8] = 'NotifyBuddyOnline',
	[9] = 'NotifyBuddyLeftGame',
	[10] = 'NotifyBuddyOffline',
	[11] = 'NotifyBuddyJoinGame',
	[12] = 'NotifyClanOnline',
	[13] = 'NotifyClanLeftGame',
	[14] = 'NotifyClanOffline',
	[15] = 'NotifyClanJoinGame',
	[16] = 'RecievedClanMessage',
	[17] = '',
	[18] = '',
	[19] = 'RecievedIM',
	[20] = 'NotifyGameInvite',
	[21] = 'NotifySelfJoinGame',
	[22] = '',		-- NotifyBuddyRequester
	[23] = '',		-- NotifyBuddyRequested
	[24] = 'NotifyGameInvite',
	[25] = '',
	[26] = '',
}

local defaultNotificationOptions = {
	-- InGame Outside History
	['ui_not_opt_1'] = {true, true, true},		-- friend add 					-- and 2 3 4
	['ui_not_opt_2'] = {true, true, true},		-- friend added
	['ui_not_opt_3'] = {true, true, true},		-- friend remove
	['ui_not_opt_4'] = {true, true, true},		-- friend removed
	['ui_not_opt_5'] = {false, true, true},		-- clan rank change
	['ui_not_opt_6'] = {false, true, true},		-- clan add
	['ui_not_opt_7'] = {false, true, true},		-- clan removed
	['ui_not_opt_8'] = {false, true, false},	-- friend log in
	['ui_not_opt_9'] = {false, true, false},	-- friend left game
	['ui_not_opt_10'] = {false, false, false},	-- friend log off
	['ui_not_opt_11'] = {false, true, true},	-- friend join game
	['ui_not_opt_12'] = {false, true, false},	-- clan sign on
	['ui_not_opt_13'] = {false, false, false},	-- clan left game
	['ui_not_opt_14'] = {false, false, false},	-- clan sign off
	['ui_not_opt_15'] = {false, true, true},	-- clan join game
	['ui_not_opt_16'] = {false, false, false},	-- clan whisper
	['ui_not_opt_17'] = {false, true, true},	-- patch
	['ui_not_opt_18'] = {false, true, true},	-- info
	['ui_not_opt_19'] = {true, true, false},	-- im recieved
	['ui_not_opt_20'] = {false, true, true},	-- game invite
	['ui_not_opt_21'] = {false, false, true},	-- self join
	['ui_not_opt_22'] = {true, true, true},		-- you send friend request 		-- and 23
	['ui_not_opt_23'] = {true, true, true},		-- you get friend request
	['ui_not_opt_24'] = {false, true, true}, 	-- group invite
	['ui_not_opt_25'] = {false, true, true},	-- stream online
	['ui_not_opt_26'] = {false, true, true},	-- replay ready
	['ui_not_opt_27'] = {false, true, true},	-- cloudsave_upload_success		-- and 28 29 30
	['ui_not_opt_28'] = {false, true, true},	-- cloudsave_upload_fail
	['ui_not_opt_29'] = {false, true, true},	-- cloudsave_download_success
	['ui_not_opt_30'] = {false, true, true},	-- cloudsave_download_fail
}

local function UpdateChatNotificationCounts()

	HoN_Notifications.notificationsCountsTable = {}
	HoN_Notifications.historySize = 0

	for i,v in pairs(HoN_Notifications.notificationsTable) do
		HoN_Notifications.historySize = HoN_Notifications.historySize + 1
		HoN_Notifications.notificationsCountsTable[v.notificationType] = HoN_Notifications.notificationsCountsTable[v.notificationType] or 0
		HoN_Notifications.notificationsCountsTable[v.notificationType] = HoN_Notifications.notificationsCountsTable[v.notificationType] + 1
	end

	GetWidget('sp_no_notifications_cover'):SetVisible(HoN_Notifications.historySize == 0)
	groupfcall('sysbar_notification_count_labels', function(_, widget, _) widget:SetText(math.floor(HoN_Notifications.historySize)) end)
end

local function GetUpgrades(userName)
	local nameColor, accountIcon, nameGlow, nameGlowColor
	userName = StripClanTag(string.lower(userName))

	if (HoN_Social_Panel) and (HoN_Social_Panel.friendslist) then
		for _, groupTable in pairs(HoN_Social_Panel.friendslist) do
			for playerName, playerTable in pairs(groupTable) do
				if userName == StripClanTag(string.lower(playerName)) then

					accountIcon = playerTable.accountIcon or playerTable.chatColorTexture or playerTable.chatSymbolTexture or nil

					if Empty(accountIcon) then
						accountIcon = nil
					end

					if (playerTable.chatColorOnlineString) and (string.len(playerTable.chatColorOnlineString) >= 1) and (playerTable.chatColorOnlineString ~= '#FFFFFF') then
						nameColor = playerTable.chatColorOnlineString
					end

					if (playerTable.nameGlow) then
						nameGlow = playerTable.nameGlow
					end

					break
				end
			end
		end
	end
	if (GetChatClientInfo) and ((not nameColor) or (not accountIcon)) then
		if (HoN_Notifications.userUpgradesTable[userName]) then
			accountIcon 	= HoN_Notifications.userUpgradesTable[userName].accountIcon
			nameColor 		= HoN_Notifications.userUpgradesTable[userName].nameColor
			nameGlow 		= HoN_Notifications.userUpgradesTable[userName].nameGlow
			nameGlowColor	= HoN_Notifications.userUpgradesTable[userName].nameGlowColor
		else
			local clientInfoTable = explode('|', GetChatClientInfo(userName, 'chatnamecolorstring|chatnamecolortexturepath|getaccounticontexturepath|chatnameglow|chatnameglowcolorstring'))

			if NotEmpty(clientInfoTable[3]) then
				accountIcon = clientInfoTable[3]
			elseif NotEmpty(clientInfoTable[2]) then
				accountIcon = clientInfoTable[2]
			end
			if NotEmpty(clientInfoTable[1]) then
				nameColor = clientInfoTable[1]
			end

			nameGlow = NotEmpty(clientInfoTable[4]) and clientInfoTable[4] or 'false'
			nameGlowColor = NotEmpty(clientInfoTable[5]) and clientInfoTable[5] or ''

			if NotEmpty(nameColor) and NotEmpty(accountIcon) then
				HoN_Notifications.userUpgradesTable[userName] = HoN_Notifications.userUpgradesTable[userName] or {}
				HoN_Notifications.userUpgradesTable[userName].accountIcon 	= accountIcon
				HoN_Notifications.userUpgradesTable[userName].nameColor 	= nameColor
				if NotEmpty(nameGlow) then 	-- nested because this is false, not empty, even if we don't have info
					HoN_Notifications.userUpgradesTable[userName].nameGlow 	= nameGlow
				end
				if NotEmpty(nameGlowColor) then
					HoN_Notifications.userUpgradesTable[userName].nameGlowColor	= nameGlowColor
				end
			end
		end
	end
	return nameColor, accountIcon, nameGlow, nameGlowColor
end

local function NotificationsInit()
	SetSave('ui_notifications_toastDuration',  '7', 'int', true)
	SetSave('cc_showBuddyRequestNotification', 'true', 'bool')
	SetSave('cc_showBuddyAddNotification', 'true', 'bool')
	SetSave('cc_showBuddyConnectionNotification', 'true', 'bool')
	SetSave('cc_showBuddyDisconnectionNotification', 'true', 'bool')
	SetSave('cc_showBuddyJoinGameNotification', 'true', 'bool')
	SetSave('cc_showBuddyLeaveGameNotification', 'true', 'bool')
	SetSave('cc_showClanConnectionNotification', 'true', 'bool')
	SetSave('cc_showClanDisconnectionNotification', 'true', 'bool')
	SetSave('cc_showClanJoinGameNotification', 'true', 'bool')
	SetSave('cc_showClanLeaveGameNotification', 'true', 'bool')
	SetSave('cc_showClanRankNotification', 'true', 'bool')
	SetSave('cc_showClanAddNotification', 'true', 'bool')
	SetSave('cc_showClanRemoveNotification', 'true', 'bool')
	SetSave('cc_showGameInvites', 'true', 'bool')
	SetSave('cc_showIMNotification', 'true', 'bool')
	SetSave('cc_showNewPatchNotification', 'true', 'bool')
	SetSave('cc_showActiveStreamNotification', 'true', 'bool')
	SetSave('cc_DisableNotifications', 'false', 'bool')
	SetSave('cc_DisableNotificationsInGame', 'false', 'bool')
	SetSave('ui_not_opt_sound', 'true', 'bool', true)
end

local function PlayNotificationSound(notificationType)
	if GetCvarBool('ui_not_opt_sound') then
		if (NotEmpty(soundsByType[notificationType])) then
			PlayChatSound(soundsByType[notificationType])
		end
	end
end

local function GetNotificationsOptionInfo(option, subOption, notificationType)

	if (notificationType) then
		if (notificationType == 2) or (notificationType == 3) or (notificationType == 4) then
			notificationType = 1
		elseif (notificationType == 22) then
			notificationType = 23
		elseif notificationType > 27 and notificationType <= 30 then	-- cloud save related
			notificationType = 27
		end
	end

	if (notificationType) then
		if (GetCvarBool('ui_not_opt_' .. tostring(notificationType) .. '_' .. tostring(subOption), true) ~= nil) then
			return GetCvarBool('ui_not_opt_' .. tostring(notificationType) .. '_' .. tostring(subOption))
		elseif (defaultNotificationOptions['ui_not_opt_' .. notificationType]) then
			return defaultNotificationOptions['ui_not_opt_' .. notificationType][subOption] and true or false
		else
			return false
		end
	else
		return defaultNotificationOptions[option] and defaultNotificationOptions[option][subOption] and true or false
	end
end

local function UpdateChatNotificationPopupLabels()
	if (HoN_Notifications.notificationsCountsTable[23]) and (HoN_Notifications.notificationsCountsTable[23] > 0) then
		GetWidget('sp_header_friendrequests_button', 'main', false):SetVisible(true)
		GetWidget('sp_header_friendrequests_button_count_bg', 'main', false):SetVisible(true)
		if (HoN_Notifications.notificationsCountsTable[23] <= 9) then
			GetWidget('sp_header_friendrequests_button_count_label', 'main', false):SetText(tostring(HoN_Notifications.notificationsCountsTable[23]))
		else
			GetWidget('sp_header_friendrequests_button_count_label', 'main', false):SetText('+')
		end
		GetWidget('sp_header_popup_friendrequests_showmore', 'main', false):SetVisible(true)
	else
		GetWidget('sp_header_friendrequests_button', 'main', false):SetVisible(false)
		GetWidget('sp_header_friendrequests_button_count_bg', 'main', false):SetVisible(true)
		GetWidget('sp_header_friendrequests_button_count_label', 'main', false):SetText('0')
		GetWidget('sp_header_popup_friendrequests', 'main', false):SetVisible(false)
		GetWidget('sp_header_popup_friendrequests_showmore', 'main', false):SetVisible(false)
	end

	if ((HoN_Notifications.notificationsCountsTable[20]) and (HoN_Notifications.notificationsCountsTable[20] > 0)) or
		((HoN_Notifications.notificationsCountsTable[24]) and (HoN_Notifications.notificationsCountsTable[24] > 0)) then
		HoN_Notifications.notificationsCountsTable[20] = HoN_Notifications.notificationsCountsTable[20] or 0
		HoN_Notifications.notificationsCountsTable[24] = HoN_Notifications.notificationsCountsTable[24] or 0

		GetWidget('sp_header_invites_button', 'main', false):SetVisible(true)
		GetWidget('sp_header_invites_button_count_bg', 'main', false):SetVisible(true)
		if ((HoN_Notifications.notificationsCountsTable[20] + HoN_Notifications.notificationsCountsTable[24]) <= 9) then
			GetWidget('sp_header_invites_button_count_label', 'main', false):SetText(tostring(HoN_Notifications.notificationsCountsTable[20] + HoN_Notifications.notificationsCountsTable[24]))
		else
			GetWidget('sp_header_invites_button_count_label', 'main', false):SetText('+')
		end
		GetWidget('sp_header_popup_invites_showmore', 'main', false):SetVisible(true)
	else
		GetWidget('sp_header_invites_button', 'main', false):SetVisible(false)
		GetWidget('sp_header_invites_button_count_bg', 'main', false):SetVisible(true)
		GetWidget('sp_header_invites_button_count_label', 'main', false):SetText('0')
		GetWidget('sp_header_popup_invites', 'main', false):SetVisible(false)
		GetWidget('sp_header_popup_invites_showmore', 'main', false):SetVisible(false)
	end
end

local function UpdateExistingFriendPopupNotifications()
	local targetIndex, targetWidget, targetWidgetLabel1, targetWidgetLabel2, targetWidgetLabel3, targetWidgetBtns, targetWidgetBtn1, targetWidgetBtn2
	local newNotification

	targetIndex = HoN_Notifications.currentNotification
	for index = 1, HoN_Notifications.MAX_POPUP_NOTIFICATIONS_FRIEND, 1 do

		targetWidget = GetWidget('sp_notification_friend_popup_template_' .. index, 'main', true)

		newNotification = nil
		for i = targetIndex, 1, -1 do
			if (HoN_Notifications.notificationsTable[i]) and (HoN_Notifications.notificationsTable[i].notificationType == 23) then
				newNotification = HoN_Notifications.notificationsTable[i]
				targetIndex = i - 1
				break
			end
		end

		if (targetWidget) then
			if (newNotification) then

				targetWidgetIcon 	= GetWidget('sp_notification_friend_popup_template_' .. index .. '_icon' )

				targetWidgetLabel1 	= GetWidget('sp_notification_friend_popup_template_' .. index .. '_label_1' )
				targetWidgetLabel2 	= GetWidget('sp_notification_friend_popup_template_' .. index .. '_label_2' )
				targetWidgetLabel3 	= GetWidget('sp_notification_friend_popup_template_' .. index .. '_label_3' )

				targetWidgetBtn1 	= GetWidget('sp_notification_friend_popup_template_' .. index .. '_button_1' )
				targetWidgetBtn2 	= GetWidget('sp_notification_friend_popup_template_' .. index .. '_button_2' )

				local intNotificationID =  newNotification.intNotificationID
				if (newNotification.inHistory) then
					targetWidgetBtn1:SetCallback('onclick', function()
						if (intNotificationID) then
							HoN_Notifications.PopupNotificationButtonClicked(1, index, 'friend', intNotificationID)
						end
					end)
					targetWidgetBtn1:RefreshCallbacks()
					targetWidgetBtn2:SetCallback('onclick', function()
						if (intNotificationID) then
							HoN_Notifications.PopupNotificationButtonClicked(2, index, 'friend', intNotificationID)
						end
					end)
					targetWidgetBtn2:RefreshCallbacks()
				else
					targetWidgetBtn1:SetCallback('onclick', CreatePopupButtonOnclick(1, newNotification))
					targetWidgetBtn1:RefreshCallbacks()
					targetWidgetBtn2:SetCallback('onclick', CreatePopupButtonOnclick(2, newNotification))
					targetWidgetBtn2:RefreshCallbacks()
				end

				targetWidget:SetVisible(true)

				if (not newNotification.nameColor) and (not newNotification.accountIcon) then
					newNotification.nameColor, newNotification.accountIcon, newNotification.nameGlow, newNotification.nameGlowColor = GetUpgrades(newNotification.sParam1)
				end

				targetWidgetLabel1:SetColor(newNotification.nameColor or '#ffb300')
				if (newNotification.accountIcon) and (newNotification.accountIcon ~= '/ui/fe2/store/icons/account_icons/default.tga') then
					targetWidgetIcon:SetColor('1 1 1 1')
					targetWidgetIcon:SetTexture(newNotification.accountIcon)
				else
					targetWidgetIcon:SetColor('1 1 1 0.6')
					targetWidgetIcon:SetTexture('/ui/fe2/store/icons/account_icons/default.tga')
				end

				targetWidgetLabel1:SetText(newNotification.sParam1)
				targetWidgetLabel2:SetText(Translate(newNotification.notifyTranslateString .. '_info'))
				targetWidgetLabel3:SetText(newNotification.notificationTime)
			else
				targetWidget:SetVisible(false)
			end
		end
	end
end

local function UpdateExistingInvitePopupNotifications()
	local targetIndex, targetWidget, targetWidgetLabel1, targetWidgetLabel2, targetWidgetLabel3, targetWidgetLabel4, targetWidgetLabel5
	local targetIcon1, targetIcon2, targetIcon3, targetIcon4, targetIcon5, targetIcon6, targetIcon7, targetIcon8, targetIcon9, targetIcon10, targetIcon11, targetIcon12, targetIcon13, targetIcon14
	local targetWidgetBtns, targetWidgetBtn1, targetWidgetBtn2
	local newNotification
	local searchLimit = HoN_Notifications.MAX_POPUP_NOTIFICATIONS_FRIEND

	targetIndex = HoN_Notifications.currentNotification
	for index = 1, searchLimit, 1 do
		targetWidget = GetWidget('sp_notification_invite_popup_template_' .. index, 'main', true)

		newNotification = nil
		for i = targetIndex, 1, -1 do
			if (HoN_Notifications.notificationsTable[i]) and ((HoN_Notifications.notificationsTable[i].notificationType == 20) or (HoN_Notifications.notificationsTable[i].notificationType == 24)) then
				newNotification = HoN_Notifications.notificationsTable[i]
				targetIndex = i - 1
				break
			end
		end

		if (targetWidget) then
			if (newNotification) then

				targetWidgetIcon 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_icon' )

				targetWidgetLabel1 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_label_1' )
				targetWidgetLabel2 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_label_2' )
				targetWidgetLabel3 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_label_3' )
				targetWidgetLabel4 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_label_4' )
				targetWidgetLabel5 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_label_5' )

				targetIcon1 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_icon_1' )
				targetIcon2 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_icon_2' )
				targetIcon3 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_icon_3' )
				targetIcon4 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_icon_4' )
				targetIcon5 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_icon_5' )
				targetIcon6 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_icon_6' )
				targetIcon7 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_icon_7' )
				targetIcon8 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_icon_8' )
				targetIcon9 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_icon_9' )
				targetIcon10 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_icon_10' )
				targetIcon11 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_icon_11' )
				targetIcon12 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_icon_12' )
				targetIcon13 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_icon_13' )
				targetIcon14 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_icon_14' )

				targetWidgetBtn1 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_button_1' )
				targetWidgetBtn2 	= GetWidget('sp_notification_invite_popup_template_' .. index .. '_button_2' )

				local intNotificationID =  newNotification.intNotificationID
				if (newNotification.inHistory) then
					targetWidgetBtn1:SetCallback('onclick', function()
						if (intNotificationID) then
							HoN_Notifications.PopupNotificationButtonClicked(1, index, 'invite', intNotificationID)
						end
					end)
					targetWidgetBtn1:RefreshCallbacks()
					targetWidgetBtn2:SetCallback('onclick', function()
						if (intNotificationID) then
							HoN_Notifications.PopupNotificationButtonClicked(2, index, 'invite', intNotificationID)
						end
					end)
					targetWidgetBtn2:RefreshCallbacks()
				else
					targetWidgetBtn1:SetCallback('onclick', CreatePopupButtonOnclick(1, newNotification))
					targetWidgetBtn1:RefreshCallbacks()
					targetWidgetBtn2:SetCallback('onclick', CreatePopupButtonOnclick(2, newNotification))
					targetWidgetBtn2:RefreshCallbacks()
				end

				if rap2Enable then
					targetWidgetBtn2:SetEnabled(not IsAccountSuspended())
				end

				targetWidget:SetVisible(true)

				local label_2 = Translate(newNotification.notifyTranslateString .. '_dependent', 'version', newNotification.sParam2, 'streamname', newNotification.sParam1, 'name', newNotification.sParam2, 'replay', newNotification.sParam1)
				label_2 = NotEmpty(label_2) and (label_2 .. '\n') or ''
				label_2 = label_2 .. Translate(newNotification.notifyTranslateString .. '_info', 'gamename', newNotification.sParam2, 'rank', newNotification.sParam3, 'name', newNotification.sParam2)

				local sRemovedInfo = TranslateOrNil(newNotification.notifyTranslateString .. '_info_removed', 'name', newNotification.sParam2)
				if NotEmpty(sRemovedInfo) then
					label_2 = label_2 .. '\n' .. Translate(newNotification.notifyTranslateString .. '_info_removed', 'name', newNotification.sParam2)
				end

				local notificationTitle = newNotification.sParam1
				if (newNotification.notificationType ~= 24) and (newNotification.gameInfo3) and NotEmpty(newNotification.gameInfo3) then
					notificationTitle = newNotification.gameInfo3
				end

				if (newNotification.notificationType == 20) then
					targetWidget:SetNoClick(false)
					GetWidget('sp_notification_invite_popup_template_' .. index .. '_button_1'):SetVisible(true)
					GetWidget('sp_notification_invite_popup_template_' .. index .. '_button_1'):SetWidth('100%')
					groupfcall('sp_notification_invite_popup_template_' .. index .. '_buttons_1', function(index, widget, groupName) widget:SetText(Translate('general_ignore')) end)

					GetWidget('sp_notification_invite_popup_template_' .. index .. '_button_2'):SetVisible(true)
					GetWidget('sp_notification_invite_popup_template_' .. index .. '_button_2'):SetWidth('100%')
					groupfcall('sp_notification_invite_popup_template_' .. index .. '_buttons_2', function(index, widget, groupName) widget:SetText(Translate('notify_join_game')) end)
				elseif (newNotification.notificationType == 24) then
					targetWidget:SetNoClick(true)
					GetWidget('sp_notification_invite_popup_template_' .. index .. '_button_1'):SetVisible(true)
					GetWidget('sp_notification_invite_popup_template_' .. index .. '_button_1'):SetWidth('100%')
					groupfcall('sp_notification_invite_popup_template_' .. index .. '_buttons_1', function(index, widget, groupName) widget:SetText(Translate('general_ignore')) end)

					GetWidget('sp_notification_invite_popup_template_' .. index .. '_button_2'):SetVisible(true)
					GetWidget('sp_notification_invite_popup_template_' .. index .. '_button_2'):SetWidth('100%')
					groupfcall('sp_notification_invite_popup_template_' .. index .. '_buttons_2', function(index, widget, groupName) widget:SetText(Translate('notify_join_group')) end)
				elseif (newNotification.notificationType == 17) then
					targetWidget:SetNoClick(true)
					GetWidget('sp_notification_invite_popup_template_' .. index .. '_button_1'):SetVisible(false)
					GetWidget('sp_notification_invite_popup_template_' .. index .. '_button_1'):SetWidth('100%')
					groupfcall('sp_notification_invite_popup_template_' .. index .. '_buttons_1', function(index, widget, groupName) widget:SetText(Translate('general_ignore')) end)

					GetWidget('sp_notification_invite_popup_template_' .. index .. '_button_2'):SetVisible(true)
					GetWidget('sp_notification_invite_popup_template_' .. index .. '_button_2'):SetWidth('206%')
					groupfcall('sp_notification_invite_popup_template_' .. index .. '_buttons_2', function(index, widget, groupName) widget:SetText(Translate('notify_update_now')) end)
				else
					targetWidget:SetNoClick(true)
					GetWidget('sp_notification_invite_popup_template_' .. index .. '_button_1'):SetVisible(true)
					GetWidget('sp_notification_invite_popup_template_' .. index .. '_button_1'):SetWidth('100%')
					groupfcall('sp_notification_invite_popup_template_' .. index .. '_buttons_1', function(index, widget, groupName) widget:SetText(Translate('general_ignore')) end)

					GetWidget('sp_notification_invite_popup_template_' .. index .. '_button_2'):SetVisible(true)
					GetWidget('sp_notification_invite_popup_template_' .. index .. '_button_2'):SetWidth('100%')
					groupfcall('sp_notification_invite_popup_template_' .. index .. '_buttons_2', function(index, widget, groupName) widget:SetText('?') end)
				end

				if (not newNotification.nameColor) and (not newNotification.accountIcon) and (not newNotification.nameGlow) then
					newNotification.nameColor, newNotification.accountIcon, newNotification.nameGlow = GetUpgrades(notificationTitle)
				end

				targetWidgetLabel1:SetColor(newNotification.nameColor or '#ffb300')
				targetWidgetLabel1:SetGlow(NotEmpty(newNotification.nameGlow) and AtoB(newNotification.nameGlow) or false)
				targetWidgetLabel1:SetGlowColor(NotEmpty(newNotification.nameGlowColor) and newNotification.nameGlowColor or '')

				if (newNotification.accountIcon) and (newNotification.accountIcon ~= '/ui/fe2/store/icons/account_icons/default.tga') then
					targetWidgetIcon:SetColor('1 1 1 1')
					targetWidgetIcon:SetTexture(newNotification.accountIcon)
				else
					targetWidgetIcon:SetColor('1 1 1 0.6')
					targetWidgetIcon:SetTexture('/ui/fe2/store/icons/account_icons/default.tga')
				end

				targetWidgetLabel1:SetText(notificationTitle or '?')
				targetWidgetLabel2:SetText((FormatStringNewline(label_2 or '?')))
				targetWidgetLabel3:SetText(newNotification.notificationTime)
				targetWidgetLabel4:SetText(newNotification.gameInfo2)
				targetWidgetLabel5:SetText(newNotification.gameInfo4 .. ' - ' .. newNotification.teamSize)

				targetIcon1:SetTexture('/maps/' .. newNotification.mapName .. '/icon.tga')
				targetIcon2:SetTexture('/ui/icons/' .. newNotification.gameMode .. '.tga')
				targetIcon3:SetVisible((newNotification.isVerifiedOnly))
				targetIcon4:SetVisible((newNotification.isCasualMode))
				targetIcon5:SetVisible((newNotification.isHardcore))
				targetIcon6:SetVisible((newNotification.isGated))
				targetIcon7:SetVisible((newNotification.isAdvOptions))
				targetIcon8:SetVisible((newNotification.isAutoBalanced))
				targetIcon9:SetVisible((newNotification.statsLevel == '1'))
				targetIcon10:SetVisible((newNotification.statsLevel == '2'))
				targetIcon11:SetVisible((newNotification.isNoLeaver))
				targetIcon12:SetVisible((newNotification.isPrivate))
				targetIcon13:SetVisible((newNotification.isDevHeroes))
				targetIcon14:SetVisible((newNotification.isBots))

				local function MouseOut()
					Trigger('genericMainFloatingTip', 'false', '', '', '', '', '', '', '', '')
					Set('sp_notification_invite_popup_template_' .. index.. '_mouseover', 'false', 'bool')
					GetWidget('sp_notification_invite_popup_template_' .. index):DoEventN(1)
				end

				local mapName = newNotification.mapName
				targetIcon1:SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/maps/' .. mapName .. '/icon.tga', 'mainlobby_gamelist_'..mapName..'_title', '', '', '', '-18h', '1h')
					Set('sp_notification_invite_popup_template_' .. index.. '_mouseover', 'true', 'bool')
				end)
				targetIcon1:SetCallback('onmouseout', function()
					MouseOut()
				end)
				targetIcon1:RefreshCallbacks()

				local gameMode = newNotification.gameMode
				targetIcon3:SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/' .. gameMode .. '.tga', 'mainlobby_gamelist_'..gameMode..'_title', '', '', '', '-18h', '1h')
					Set('sp_notification_invite_popup_template_' .. index.. '_mouseover', 'true', 'bool')
				end)
				targetIcon3:SetCallback('onmouseout', function()
					MouseOut()
				end)
				targetIcon3:RefreshCallbacks()

				targetIcon2:SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/verified.tga', 'mainlobby_gamelist_verifiedonly_title', 'mainlobby_gamelist_verifiedonly_desc', '', '', '-18h', '1h')
					Set('sp_notification_invite_popup_template_' .. index.. '_mouseover', 'true', 'bool')
				end)
				targetIcon2:SetCallback('onmouseout', function()
					MouseOut()
				end)
				targetIcon2:RefreshCallbacks()

				targetIcon4:SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/casual.tga', 'mainlobby_gamelist_casual_title', 'mainlobby_gamelist_casual_desc', '', '', '-18h', '1h')
					Set('sp_notification_invite_popup_template_' .. index.. '_mouseover', 'true', 'bool')
				end)
				targetIcon4:SetCallback('onmouseout', function()
					MouseOut()
				end)
				targetIcon4:RefreshCallbacks()

				targetIcon5:SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/hardcore.tga', 'mainlobby_gamelist_hardcore_title', 'mainlobby_gamelist_hardcore_desc', '', '', '-18h', '1h')
					Set('sp_notification_invite_popup_template_' .. index.. '_mouseover', 'true', 'bool')
				end)
				targetIcon5:SetCallback('onmouseout', function()
					MouseOut()
				end)
				targetIcon5:RefreshCallbacks()

				targetIcon6:SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/gated.tga', 'mainlobby_gamelist_gated_title', 'mainlobby_gamelist_gated_desc', '', '', '-18h', '1h')
					Set('sp_notification_invite_popup_template_' .. index.. '_mouseover', 'true', 'bool')
				end)
				targetIcon6:SetCallback('onmouseout', function()
					MouseOut()
				end)
				targetIcon6:RefreshCallbacks()

				targetIcon7:SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/more.tga', 'mainlobby_gamelist_more_title', 'mainlobby_gamelist_more_desc', '', '', '-18h', '1h')
					Set('sp_notification_invite_popup_template_' .. index.. '_mouseover', 'true', 'bool')
				end)
				targetIcon7:SetCallback('onmouseout', function()
					MouseOut()
				end)
				targetIcon7:RefreshCallbacks()

				targetIcon8:SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/autobalance.tga', 'mainlobby_gamelist_autobalance_title', 'mainlobby_gamelist_autobalance_desc', '', '', '-18h', '1h')
					Set('sp_notification_invite_popup_template_' .. index.. '_mouseover', 'true', 'bool')
				end)
				targetIcon8:SetCallback('onmouseout', function()
					MouseOut()
				end)
				targetIcon8:RefreshCallbacks()

				targetIcon9:SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/official.tga', 'mainlobby_gamelist_official_title', 'mainlobby_gamelist_official_desc', '', '', '-18h', '1h')
					Set('sp_notification_invite_popup_template_' .. index.. '_mouseover', 'true', 'bool')
				end)
				targetIcon9:SetCallback('onmouseout', function()
					MouseOut()
				end)
				targetIcon9:RefreshCallbacks()

				targetIcon10:SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/official_2.tga', 'mainlobby_gamelist_official_2_title', 'mainlobby_gamelist_official_2_desc', '', '', '-18h', '1h')
					Set('sp_notification_invite_popup_template_' .. index.. '_mouseover', 'true', 'bool')
				end)
				targetIcon10:SetCallback('onmouseout', function()
					MouseOut()
				end)
				targetIcon10:RefreshCallbacks()

				targetIcon11:SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/noleaver.tga', 'mainlobby_gamelist_noleaver_title', 'mainlobby_gamelist_noleaver_desc', '', '', '-18h', '1h')
					Set('sp_notification_invite_popup_template_' .. index.. '_mouseover', 'true', 'bool')
				end)
				targetIcon11:SetCallback('onmouseout', function()
					MouseOut()
				end)
				targetIcon11:RefreshCallbacks()

				targetIcon12:SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/private.tga', 'mainlobby_gamelist_private_title', 'mainlobby_gamelist_private_desc', '', '', '-18h', '1h')
					Set('sp_notification_invite_popup_template_' .. index.. '_mouseover', 'true', 'bool')
				end)
				targetIcon12:SetCallback('onmouseout', function()
					MouseOut()
				end)
				targetIcon12:RefreshCallbacks()

				targetIcon13:SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/devheroes.tga', 'mainlobby_gamelist_devheroes_title', 'mainlobby_gamelist_devheroes_desc', '', '', '-18h', '1h')
					Set('sp_notification_invite_popup_template_' .. index.. '_mouseover', 'true', 'bool')
				end)
				targetIcon13:SetCallback('onmouseout', function()
					MouseOut()
				end)
				targetIcon13:RefreshCallbacks()

			else
				targetWidget:SetVisible(false)
			end

		end
	end
end

local function DestroyHistoryItem(targetWidget, notificationIndex, intNotificationID)
	intNotificationID = tonumber(intNotificationID)
	notificationIndex = tonumber(notificationIndex)

	if (targetWidget) then
		targetWidget:SetVisible(false)
	else
		-- e('targetWidget invalid', notificationIndex)
	end

	if (HoN_Notifications.notificationsTable[notificationIndex]) and (HoN_Notifications.notificationsTable[notificationIndex].targetWidget) then
		HoN_Notifications.notificationsTable[notificationIndex].targetWidget:SetVisible(false)
	else
		-- e('historyWidget invalid', notificationIndex)
	end

	if (intNotificationID) and (HoN_Notifications.notificationsTable[intNotificationID]) then
		interface:UICmd([[ChatRemoveNotification(']] .. intNotificationID .. [[')']])

		HoN_Notifications.notificationsTable[intNotificationID] = nil
	else
		-- e('intNotificationID invalid', intNotificationID)
		-- e('notificationIndex invalid', notificationIndex)
	end

	UpdateChatNotificationCounts()
	UpdateExistingFriendPopupNotifications()
	UpdateExistingInvitePopupNotifications()
	UpdateChatNotificationPopupLabels()

	HoN_Notifications:UpdateUserScroller(HoN_Notifications.lastUserOffset or 0)
end

local function CreatePopupButtonOnclick(buttonIndex, notification)
	local notificationType = notification.notificationType

	if (buttonIndex == 1) then
		if (notificationType == 20) then
			return function() interface:UICmd([[ClearInviteAddress()]]) end
		elseif (notificationType == 24) then
			return function() interface:UICmd([[RejectTMMInvite(']] .. notification.sParam1 .. [[')]]) end
		end
	elseif (buttonIndex == 2) then
		if (notificationType == 20) or (notificationType == 21) then
			return function() interface:UICmd([[Connect(']] .. notification.gameInfo1 .. [[', ']] .. notification.gameInfo2 .. [[')]]) end
		elseif (notificationType == 23) then
			return function() interface:UICmd([[ChatApproveBuddy(']] .. notification.sParam1 .. [[')']]) end
		elseif (notificationType == 24) then
			return function() HoN_Matchmaking:JoinGroup(notification.sParam1) end
		elseif (notificationType == 17) then
			return function() Trigger('ShowUpdatePanel') end
		end
	end

	return function() end
end

function HoN_Notifications.PopupNotificationButtonClicked(buttonIndex, notificationIndex, templateName, intNotificationID)

	local correctedNotificationIndex = tonumber(intNotificationID)

	if (correctedNotificationIndex) and (HoN_Notifications.notificationsTable) and (HoN_Notifications.notificationsTable[correctedNotificationIndex]) then
		local targetWidget = GetWidget('sp_notification_' .. templateName .. '_popup_template_' .. notificationIndex, 'main', true)
		if (targetWidget) then
			if (buttonIndex == 1) then

				if (HoN_Notifications.notificationsTable[correctedNotificationIndex].notificationType == 20) then	-- game invite
					interface:UICmd([[ClearInviteAddress()]])
				elseif (HoN_Notifications.notificationsTable[correctedNotificationIndex].notificationType == 24) then	-- group invite
					interface:UICmd([[RejectTMMInvite(']] .. HoN_Notifications.notificationsTable[correctedNotificationIndex].sParam1 .. [[')]])
				end

				DestroyHistoryItem(targetWidget, correctedNotificationIndex, intNotificationID)

			elseif (buttonIndex == 2) then

				if (HoN_Notifications.notificationsTable[correctedNotificationIndex].notificationType == 20) or (HoN_Notifications.notificationsTable[correctedNotificationIndex].notificationType == 21) then	-- game invite
					interface:UICmd([[RequireConnect(']] .. HoN_Notifications.notificationsTable[correctedNotificationIndex].gameInfo1 .. [[', ']] .. HoN_Notifications.notificationsTable[correctedNotificationIndex].gameInfo2 .. [[')]])
					interface:UICmd([[ClearInviteAddress()]])
				elseif (HoN_Notifications.notificationsTable[correctedNotificationIndex].notificationType == 23) then	-- buddy request
					interface:UICmd([[ChatApproveBuddy(']] .. HoN_Notifications.notificationsTable[correctedNotificationIndex].sParam1 .. [[')']])
				elseif (HoN_Notifications.notificationsTable[correctedNotificationIndex].notificationType == 24) then 	-- group invite
					HoN_Matchmaking:JoinGroup(HoN_Notifications.notificationsTable[correctedNotificationIndex].sParam1)
				elseif (HoN_Notifications.notificationsTable[correctedNotificationIndex].notificationType == 17) then 	-- patch
					Trigger('ShowUpdatePanel')
				end

				DestroyHistoryItem(targetWidget, correctedNotificationIndex, intNotificationID)
			end
		end
	end
end

local function CreateButtonOnclick(buttonIndex, notification, templateName, notificationIndex)
	local notificationType = notification.notificationType
	local tName, notIndex = templateName, tonumber(notificationIndex)

	local function CloseToast()
		if (tName == 'toast') and (HoN_Notifications.toastWidgetTable[notIndex]) then
			HoN_Notifications.toastWidgetTable[notIndex].isAvailable = true
			HoN_Notifications.toastWidgetTable[notIndex].isClosing = false
			HoN_Notifications.toastWidgetTable[notIndex].despawnTime = HostTime()
		end
	end

	if (buttonIndex == 1) then
		if (notificationType == 20) or (notificationType == 21) or (notificationType == 15) or (notificationType == 11) then
			return function() interface:UICmd([[ClearInviteAddress()]]) CloseToast() end
		elseif (notificationType == 24) then
			return function() interface:UICmd([[RejectTMMInvite(']] .. notification.sParam1 .. [[')]]) CloseToast() end
		end
	elseif (buttonIndex == 2) then
		if (notificationType == 20) or (notificationType == 21) or (notificationType == 15) or (notificationType == 11) then
			return function() interface:UICmd([[RequireConnect(']] .. notification.gameInfo1 .. [[', ']] .. notification.gameInfo2 .. [[')]]) CloseToast() end
		elseif (notificationType == 23) then
			return function() interface:UICmd([[ChatApproveBuddy(']] .. notification.sParam1 .. [[')']]) CloseToast() end
		elseif (notificationType == 24) then
			return function() interface:UICmd("JoinTMMGroup('"..notification.sParam1.."');") CloseToast() end
		elseif (notificationType == 26) then
			return function()
				Set('_stats_last_replay_id', notification.sParam1)
				Set('_stats_last_match_id', notification.sParam1)

				UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'match_stats', nil, true)
				interface:UICmd("ClearEndGameStats(); GetMatchInfo('"..notification.sParam1.."');")
			end
		elseif (notificationType == 17) then
			return function() Trigger('ShowUpdatePanel') CloseToast() end
		end
	elseif (buttonIndex == 3) then
		return CloseToast
	end

	return function() end
end

function HoN_Notifications.NotificationButtonClicked(buttonIndex, notificationIndex, templateName, intNotificationID)
	-- Echo('HoN_Notifications.NotificationButtonClicked buttonIndex:'..buttonIndex..' notificationIndex:'..notificationIndex..' intNotificationID:'..tostring(intNotificationID))

	local correctedNotificationIndex = tonumber(intNotificationID)

	if (correctedNotificationIndex) and (HoN_Notifications.notificationsTable) and (HoN_Notifications.notificationsTable[correctedNotificationIndex]) then
		local targetWidget = GetWidget('sp_notification_' .. templateName .. '_template_' .. notificationIndex, 'main', true)
		if (targetWidget) then
			if (buttonIndex == 1) then

				if (HoN_Notifications.notificationsTable[correctedNotificationIndex].notificationType == 20) or (HoN_Notifications.notificationsTable[correctedNotificationIndex].notificationType == 21) or (HoN_Notifications.notificationsTable[correctedNotificationIndex].notificationType == 15) or (HoN_Notifications.notificationsTable[correctedNotificationIndex].notificationType == 11) then	-- game invite
					interface:UICmd([[ClearInviteAddress()]])
				elseif (HoN_Notifications.notificationsTable[correctedNotificationIndex].notificationType == 24) then	-- group invite
					interface:UICmd([[RejectTMMInvite(']] .. HoN_Notifications.notificationsTable[correctedNotificationIndex].sParam1 .. [[')]])
				end

				DestroyHistoryItem(targetWidget, correctedNotificationIndex, intNotificationID)

				if (templateName == 'toast') and (HoN_Notifications.toastWidgetTable[notificationIndex]) then
					HoN_Notifications.toastWidgetTable[notificationIndex].isAvailable = true
					HoN_Notifications.toastWidgetTable[notificationIndex].isClosing = false
					HoN_Notifications.toastWidgetTable[notificationIndex].despawnTime = HostTime()
				end

			elseif (buttonIndex == 2) then

				if (HoN_Notifications.notificationsTable[correctedNotificationIndex].notificationType == 20) or (HoN_Notifications.notificationsTable[correctedNotificationIndex].notificationType == 21) or (HoN_Notifications.notificationsTable[correctedNotificationIndex].notificationType == 15) or (HoN_Notifications.notificationsTable[correctedNotificationIndex].notificationType == 11) then	-- game invite
					interface:UICmd([[RequireConnect(']] .. HoN_Notifications.notificationsTable[correctedNotificationIndex].gameInfo1 .. [[', ']] .. HoN_Notifications.notificationsTable[correctedNotificationIndex].gameInfo2 .. [[')]])
				elseif (HoN_Notifications.notificationsTable[correctedNotificationIndex].notificationType == 23) then	-- buddy request
						interface:UICmd([[ChatApproveBuddy(']] .. HoN_Notifications.notificationsTable[correctedNotificationIndex].sParam1 .. [[')']])
					DestroyHistoryItem(targetWidget, correctedNotificationIndex, intNotificationID)
				elseif (HoN_Notifications.notificationsTable[correctedNotificationIndex].notificationType == 24) then 	-- group invite
					HoN_Matchmaking:JoinGroup(HoN_Notifications.notificationsTable[correctedNotificationIndex].sParam1)
					DestroyHistoryItem(targetWidget, correctedNotificationIndex, intNotificationID)
				elseif (HoN_Notifications.notificationsTable[correctedNotificationIndex].notificationType == 26) then 	-- replay
					local param = HoN_Notifications.notificationsTable[correctedNotificationIndex].sParam1
					Set('_stats_last_replay_id', param)
					Set('_stats_last_match_id', param)

					UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'match_stats', nil, true)
					interface:UICmd("ClearEndGameStats(); GetMatchInfo('"..param.."');")
				elseif (HoN_Notifications.notificationsTable[correctedNotificationIndex].notificationType == 17) then 	-- patch
					Trigger('ShowUpdatePanel')
				end

				if (templateName == 'toast') and (HoN_Notifications.toastWidgetTable[notificationIndex]) then
					HoN_Notifications.toastWidgetTable[notificationIndex].isAvailable = true
					HoN_Notifications.toastWidgetTable[notificationIndex].isClosing = false
					HoN_Notifications.toastWidgetTable[notificationIndex].despawnTime = HostTime()
				end

			elseif (buttonIndex == 3) then
				Echo('HoN_Notifications.NotificationButtonClicked buttonIndex:'..buttonIndex..' notificationIndex:'..notificationIndex..' intNotificationID:'..tostring(intNotificationID))

				DestroyHistoryItem(targetWidget, correctedNotificationIndex, intNotificationID)

				if (templateName == 'toast') and (HoN_Notifications.toastWidgetTable[notificationIndex]) then
					HoN_Notifications.toastWidgetTable[notificationIndex].isAvailable = true
					HoN_Notifications.toastWidgetTable[notificationIndex].isClosing = false
					HoN_Notifications.toastWidgetTable[notificationIndex].despawnTime = HostTime()
				end

			else
				DestroyHistoryItem(targetWidget, correctedNotificationIndex, intNotificationID)
			end
		else
			DestroyHistoryItem(targetWidget, correctedNotificationIndex, intNotificationID)
		end
	else
		DestroyHistoryItem(targetWidget, correctedNotificationIndex, intNotificationID)
	end
end

-- ChatPushNotification 1 "PersonName" "Test Game Name"
--[[
sParam1, sParam2, sParam3 - vary with notification
gameInfo1	Address:Port // Map Names
gameInfo2	Game Name //  Game Type
gameInfo3	Target User Name // Game Modes
gameInfo4	Server Region //  Regions - USE, USW, EU, pipe (|) delimited
--]]

local function UpdateNotificationHistory(offset)

	local offset = tonumber(offset) or 0
	HoN_Notifications.lastUserOffset = offset
	local searchOffset = 0

	local tempNotificationsTable = {}
	for i = HoN_Notifications.currentNotification, 1, -1 do
		if (HoN_Notifications.notificationsTable[i]) then
			tinsert(tempNotificationsTable , HoN_Notifications.notificationsTable[i])
		end
	end

	for slotIndex = 1, HoN_Notifications.visibleHistorySlots, 1 do

		local targetWidget = GetWidget('sp_notification_history_template_' .. slotIndex, 'main', nil)
		local localOffset = slotIndex + offset

		if (not targetWidget) then
			break
		end

		if (HoN_Notifications.autoCompleteString) and (string.len(HoN_Notifications.autoCompleteString) >= 1 ) then
			while (tempNotificationsTable[localOffset + searchOffset]) do
				if 	string.find(string.lower(tempNotificationsTable[localOffset + searchOffset].sParam1), string.lower(HoN_Notifications.autoCompleteString)) or
					string.find(string.lower(tempNotificationsTable[localOffset + searchOffset].gameInfo3), string.lower(HoN_Notifications.autoCompleteString)) or
					string.find(string.lower(Translate(tempNotificationsTable[localOffset + searchOffset].notifyTranslateString .. '_info', 'gamename', tempNotificationsTable[localOffset + searchOffset].sParam2, 'rank', tempNotificationsTable[localOffset + searchOffset].sParam3, 'name', tempNotificationsTable[localOffset + searchOffset].sParam2)), string.lower(HoN_Notifications.autoCompleteString)) then
					break
				else
					searchOffset = searchOffset + 1
				end
			end
		end

		if (not tempNotificationsTable) or (not tempNotificationsTable[localOffset + searchOffset]) then
			targetWidget:SetVisible(0)
		else
			targetWidget:SetVisible(1)

			local targetNotficiation = tempNotificationsTable[localOffset + searchOffset]
			tempNotificationsTable[localOffset + searchOffset].targetWidget = targetWidget

			if (targetNotficiation.notificationType == 20) or  (targetNotficiation.notificationType == 15)  or  (targetNotficiation.notificationType == 11)  then	-- Game invite / clan join / buddy join

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_1'):SetVisible(true)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_1'):SetWidth('100%')
				groupfcall('sp_notification_history_template_' .. slotIndex .. '_buttons_1', function(index, widget, groupName) widget:SetText(Translate('general_ignore')) end)

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_2'):SetVisible(true)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_2'):SetWidth('100%')
				groupfcall('sp_notification_history_template_' .. slotIndex .. '_buttons_2', function(index, widget, groupName) widget:SetText(Translate('notify_join_game')) end)

				GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(true)

				if (targetNotficiation.notificationType == 20) then
					targetWidget:SetCallback('onclick', function()
						Set('sp_notification_history_template_' .. slotIndex .. '_mouseover', true, 'bool')
						self:ScaleHeight('11.9h', 100)
						GetWidget('sp_notification_history_template_' .. slotIndex .. '_buttons'):FadeIn(100)
						GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(0)
						GetWidget('sp_notification_history_template_' .. slotIndex .. '_game_info'):FadeIn(100)
					end)
					targetWidget:SetCallback('onmouseover', function()
						Set('sp_notification_history_template_' .. slotIndex .. '_mouseover', true, 'bool')
						GetWidget('sp_notification_history_template_' .. slotIndex .. '_border'):FadeIn(100)
						self:Sleep(HOVER_DELAY, function()
							if GetCvarBool('sp_notification_history_template_' .. slotIndex .. '_mouseover') then
								self:ScaleHeight('11.9h', 100)
								GetWidget('sp_notification_history_template_' .. slotIndex .. '_buttons'):FadeIn(100)
								GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(0)
								GetWidget('sp_notification_history_template_' .. slotIndex .. '_game_info'):FadeIn(100)
							end
						end)
					end)
					targetWidget:RefreshCallbacks()
				else
					targetWidget:SetCallback('onclick', function()
						Set('sp_notification_history_template_' .. slotIndex .. '_mouseover', true, 'bool')
						self:ScaleHeight('9.1h', 100)
						GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(0)
						GetWidget('sp_notification_history_template_' .. slotIndex .. '_game_info'):FadeIn(100)
					end)
					targetWidget:SetCallback('onmouseover', function()
						Set('sp_notification_history_template_' .. slotIndex .. '_mouseover', true, 'bool')
						GetWidget('sp_notification_history_template_' .. slotIndex .. '_border'):FadeIn(100)
						self:Sleep(HOVER_DELAY, function()
							if GetCvarBool('sp_notification_history_template_' .. slotIndex .. '_mouseover') then
								self:ScaleHeight('9.1h', 100)
								GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(0)
								GetWidget('sp_notification_history_template_' .. slotIndex .. '_game_info'):FadeIn(100)
							end
						end)
					end)
					targetWidget:RefreshCallbacks()
				end

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_label_4'):SetText(targetNotficiation.gameInfo2 or '?')
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_label_5'):SetText(targetNotficiation.gameInfo4 .. ' - ' .. targetNotficiation.teamSize)

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_1'):SetTexture('/maps/' .. targetNotficiation.mapName .. '/icon.tga')
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_2'):SetTexture('/ui/icons/' .. targetNotficiation.gameMode .. '.tga')
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_3'):SetVisible(targetNotficiation.isVerifiedOnly)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_4'):SetVisible(targetNotficiation.isCasualMode)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_5'):SetVisible(targetNotficiation.isHardcore)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_6'):SetVisible(targetNotficiation.isGated)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_7'):SetVisible(targetNotficiation.isAdvOptions)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_8'):SetVisible(targetNotficiation.isAutoBalanced)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_9'):SetVisible(targetNotficiation.statsLevel == '1')
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_10'):SetVisible(targetNotficiation.statsLevel == '2')
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_11'):SetVisible(targetNotficiation.isNoLeaver)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_12'):SetVisible(targetNotficiation.isPrivate)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_13'):SetVisible(targetNotficiation.isDevHeroes)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_14'):SetVisible(targetNotficiation.isBots)

				local function MouseOut()
					Trigger('genericMainFloatingTip', 'false', '', '', '', '', '', '', '', '')
					Set('sp_notification_history_template_' ..slotIndex .. '_mouseover', 'false', 'bool')
					GetWidget('sp_notification_history_template_' .. slotIndex):DoEventN(1)
				end

				local mapName = targetNotficiation.mapName
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_1'):SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/maps/' .. mapName .. '/icon.tga', 'mainlobby_gamelist_'..mapName..'_title', '', '', '', '-18h', '1h')
					Set('sp_notification_history_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_1'):SetCallback('onmouseout', function()
					MouseOut()
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_1'):RefreshCallbacks()

				local gameMode = targetNotficiation.gameMode
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_2'):SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/' .. gameMode .. '.tga', 'mainlobby_gamelist_'..gameMode..'_title', '', '', '', '-18h', '1h')
					Set('sp_notification_history_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_2'):SetCallback('onmouseout', function()
					MouseOut()
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_2'):RefreshCallbacks()

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_3'):SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/verified.tga', 'mainlobby_gamelist_verifiedonly_title', 'mainlobby_gamelist_verifiedonly_desc', '', '', '-18h', '1h')
					Set('sp_notification_history_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_3'):SetCallback('onmouseout', function()
					MouseOut()
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_3'):RefreshCallbacks()

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_4'):SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/casual.tga', 'mainlobby_gamelist_casual_title', 'mainlobby_gamelist_casual_desc', '', '', '-18h', '1h')
					Set('sp_notification_history_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_4'):SetCallback('onmouseout', function()
					MouseOut()
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_4'):RefreshCallbacks()

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_5'):SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/hardcore.tga', 'mainlobby_gamelist_hardcore_title', 'mainlobby_gamelist_hardcore_desc', '', '', '-18h', '1h')
					Set('sp_notification_history_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_5'):SetCallback('onmouseout', function()
					MouseOut()
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_5'):RefreshCallbacks()

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_6'):SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/gated.tga', 'mainlobby_gamelist_gated_title', 'mainlobby_gamelist_gated_desc', '', '', '-18h', '1h')
					Set('sp_notification_history_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_6'):SetCallback('onmouseout', function()
					MouseOut()
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_6'):RefreshCallbacks()

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_7'):SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/more.tga', 'mainlobby_gamelist_more_title', 'mainlobby_gamelist_more_desc', '', '', '-18h', '1h')
					Set('sp_notification_history_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_7'):SetCallback('onmouseout', function()
					MouseOut()
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_7'):RefreshCallbacks()

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_8'):SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/autobalance.tga', 'mainlobby_gamelist_autobalance_title', 'mainlobby_gamelist_autobalance_desc', '', '', '-18h', '1h')
					Set('sp_notification_history_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_8'):SetCallback('onmouseout', function()
					MouseOut()
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_8'):RefreshCallbacks()

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_9'):SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/official.tga', 'mainlobby_gamelist_official_title', 'mainlobby_gamelist_official_desc', '', '', '-18h', '1h')
					Set('sp_notification_history_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_9'):SetCallback('onmouseout', function()
					MouseOut()
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_9'):RefreshCallbacks()

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_10'):SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/official_2.tga', 'mainlobby_gamelist_official_2_title', 'mainlobby_gamelist_official_2_desc', '', '', '-18h', '1h')
					Set('sp_notification_history_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_10'):SetCallback('onmouseout', function()
					MouseOut()
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_10'):RefreshCallbacks()

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_11'):SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/noleaver.tga', 'mainlobby_gamelist_noleaver_title', 'mainlobby_gamelist_noleaver_desc', '', '', '-18h', '1h')
					Set('sp_notification_history_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_11'):SetCallback('onmouseout', function()
					MouseOut()
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_11'):RefreshCallbacks()

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_12'):SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/private.tga', 'mainlobby_gamelist_private_title', 'mainlobby_gamelist_private_desc', '', '', '-18h', '1h')
					Set('sp_notification_history_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_12'):SetCallback('onmouseout', function()
					MouseOut()
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_12'):RefreshCallbacks()

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_13'):SetCallback('onmouseover', function()
					Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/devheroes.tga', 'mainlobby_gamelist_devheroes_title', 'mainlobby_gamelist_devheroes_desc', '', '', '-18h', '1h')
					Set('sp_notification_history_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_13'):SetCallback('onmouseout', function()
					MouseOut()
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_13'):RefreshCallbacks()

			elseif (targetNotficiation.notificationType == 24)  then --  group invite

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_1'):SetVisible(true)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_1'):SetWidth('100%')
				groupfcall('sp_notification_history_template_' .. slotIndex .. '_buttons_1', function(index, widget, groupName) widget:SetText(Translate('general_ignore')) end)

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_2'):SetVisible(true)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_2'):SetWidth('100%')
				groupfcall('sp_notification_history_template_' .. slotIndex .. '_buttons_2', function(index, widget, groupName) widget:SetText(Translate('notify_join_group')) end)

				GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(true)

				targetWidget:SetCallback('onclick', function()
					Set('sp_notification_history_template_' .. slotIndex .. '_mouseover', true, 'bool')
					self:ScaleHeight('9.1h', 100)
					GetWidget('sp_notification_history_template_' .. slotIndex .. '_buttons'):FadeIn(100)
					GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(0)
				end)
				targetWidget:SetCallback('onmouseover', function()
					Set('sp_notification_history_template_' .. slotIndex .. '_mouseover', true, 'bool')
					GetWidget('sp_notification_history_template_' .. slotIndex .. '_border'):FadeIn(100)
					self:Sleep(HOVER_DELAY, function()
						if GetCvarBool('sp_notification_history_template_' .. slotIndex .. '_mouseover') then
							self:ScaleHeight('9.1h', 100)
							GetWidget('sp_notification_history_template_' .. slotIndex .. '_buttons'):FadeIn(100)
							GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(0)
						end
					end)
				end)
				targetWidget:RefreshCallbacks()

			elseif (targetNotficiation.notificationType == 23) then	-- buddy request

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_1'):SetVisible(true)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_1'):SetWidth('100%')
				groupfcall('sp_notification_history_template_' .. slotIndex .. '_buttons_1', function(index, widget, groupName) widget:SetText(Translate('general_ignore')) end)

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_2'):SetVisible(true)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_2'):SetWidth('100%')
				groupfcall('sp_notification_history_template_' .. slotIndex .. '_buttons_2', function(index, widget, groupName) widget:SetText(Translate('general_approve')) end)

				GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(true)

				targetWidget:SetCallback('onclick', function()
					Set('sp_notification_history_template_' .. slotIndex .. '_mouseover', true, 'bool')
					self:ScaleHeight('9.1h', 100)
					GetWidget('sp_notification_history_template_' .. slotIndex .. '_buttons'):FadeIn(100)
					GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(0)
				end)
				targetWidget:SetCallback('onmouseover', function()
					Set('sp_notification_history_template_' .. slotIndex .. '_mouseover', true, 'bool')
					GetWidget('sp_notification_history_template_' .. slotIndex .. '_border'):FadeIn(100)
					self:Sleep(HOVER_DELAY, function()
						if GetCvarBool('sp_notification_history_template_' .. slotIndex .. '_mouseover') then
							self:ScaleHeight('9.1h', 100)
							GetWidget('sp_notification_history_template_' .. slotIndex .. '_buttons'):FadeIn(100)
							GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(0)
						end
					end)
				end)
				targetWidget:RefreshCallbacks()

			elseif (targetNotficiation.notificationType == 25) then	-- stream

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_1'):SetVisible(false)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_1'):SetWidth('100%')
				groupfcall('sp_notification_history_template_' .. slotIndex .. '_buttons_1', function(index, widget, groupName) widget:SetText(Translate('general_ignore')) end)

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_2'):SetVisible(true)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_2'):SetWidth('206%')
				groupfcall('sp_notification_history_template_' .. slotIndex .. '_buttons_2', function(index, widget, groupName) widget:SetText(Translate('notify_watch')) end)

				GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(true)

				targetWidget:SetCallback('onclick', function()
					if (UIGamePhase() > 0) then return end
					Set('sp_notification_history_template_' .. slotIndex .. '_mouseover', true, 'bool')
					self:ScaleHeight('9.1h', 100)
					GetWidget('sp_notification_history_template_' .. slotIndex .. '_buttons'):FadeIn(100)
					GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(0)
				end)
				targetWidget:SetCallback('onmouseover', function()
					if (UIGamePhase() > 0) then return end
					Set('sp_notification_history_template_' .. slotIndex .. '_mouseover', true, 'bool')
					GetWidget('sp_notification_history_template_' .. slotIndex .. '_border'):FadeIn(100)
					self:Sleep(HOVER_DELAY, function()
						if GetCvarBool('sp_notification_history_template_' .. slotIndex .. '_mouseover') then
							self:ScaleHeight('9.1h', 100)
							GetWidget('sp_notification_history_template_' .. slotIndex .. '_buttons'):FadeIn(100)
							GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(0)
						end
					end)
				end)
				targetWidget:RefreshCallbacks()

			elseif (targetNotficiation.notificationType == 17) then	-- patch

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_1'):SetVisible(false)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_1'):SetWidth('100%')
				groupfcall('sp_notification_history_template_' .. slotIndex .. '_buttons_1', function(index, widget, groupName) widget:SetText(Translate('general_ignore')) end)

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_2'):SetVisible(true)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_2'):SetWidth('206%')
				groupfcall('sp_notification_history_template_' .. slotIndex .. '_buttons_2', function(index, widget, groupName) widget:SetText(Translate('notify_update_now')) end)

				GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(true)

				targetWidget:SetCallback('onclick', function()
					Set('sp_notification_history_template_' .. slotIndex .. '_mouseover', true, 'bool')
					self:ScaleHeight('9.1h', 100)
					GetWidget('sp_notification_history_template_' .. slotIndex .. '_buttons'):FadeIn(100)
					GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(0)
				end)
				targetWidget:SetCallback('onmouseover', function()
					Set('sp_notification_history_template_' .. slotIndex .. '_mouseover', true, 'bool')
					GetWidget('sp_notification_history_template_' .. slotIndex .. '_border'):FadeIn(100)
					self:Sleep(HOVER_DELAY, function()
						if GetCvarBool('sp_notification_history_template_' .. slotIndex .. '_mouseover') then
							self:ScaleHeight('9.1h', 100)
							GetWidget('sp_notification_history_template_' .. slotIndex .. '_buttons'):FadeIn(100)
							GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(0)
						end
					end)
				end)
				targetWidget:RefreshCallbacks()

			elseif (targetNotficiation.notificationType == 21) then	-- self join

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_1'):SetVisible(false)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_1'):SetWidth('100%')
				groupfcall('sp_notification_history_template_' .. slotIndex .. '_buttons_1', function(index, widget, groupName) widget:SetText(Translate('general_ignore')) end)

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_2'):SetVisible(true)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_2'):SetWidth('206%')
				groupfcall('sp_notification_history_template_' .. slotIndex .. '_buttons_2', function(index, widget, groupName) widget:SetText(Translate('notify_join_game')) end)

				GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(true)

				targetWidget:SetCallback('onclick', function()
					Set('sp_notification_history_template_' .. slotIndex .. '_mouseover', true, 'bool')
					self:ScaleHeight('11.9h', 100)
					GetWidget('sp_notification_history_template_' .. slotIndex .. '_buttons'):FadeIn(100)
					GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(0)
					GetWidget('sp_notification_history_template_' .. slotIndex .. '_game_info'):FadeIn(100)
				end)
				targetWidget:SetCallback('onmouseover', function()
					Set('sp_notification_history_template_' .. slotIndex .. '_mouseover', true, 'bool')
					GetWidget('sp_notification_history_template_' .. slotIndex .. '_border'):FadeIn(100)
					self:Sleep(HOVER_DELAY, function()
						if GetCvarBool('sp_notification_history_template_' .. slotIndex .. '_mouseover') then
							self:ScaleHeight('11.9h', 100)
							GetWidget('sp_notification_history_template_' .. slotIndex .. '_buttons'):FadeIn(100)
							GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(0)
							GetWidget('sp_notification_history_template_' .. slotIndex .. '_game_info'):FadeIn(100)
						end
					end)
				end)
				targetWidget:RefreshCallbacks()

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_label_4'):SetText(targetNotficiation.gameInfo2 or '?')
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_label_5'):SetText(targetNotficiation.gameInfo4 .. ' - ' .. targetNotficiation.teamSize)

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_1'):SetTexture('/maps/' .. targetNotficiation.mapName .. '/icon.tga')
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_2'):SetTexture('/ui/icons/' .. targetNotficiation.gameMode .. '.tga')
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_3'):SetVisible(targetNotficiation.isVerifiedOnly)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_4'):SetVisible(targetNotficiation.isCasualMode)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_5'):SetVisible(targetNotficiation.isHardcore)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_6'):SetVisible(targetNotficiation.isGated)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_7'):SetVisible(targetNotficiation.isAdvOptions)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_8'):SetVisible(targetNotficiation.isAutoBalanced)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_9'):SetVisible(targetNotficiation.statsLevel == '1')
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_10'):SetVisible(targetNotficiation.statsLevel == '2')
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_11'):SetVisible(targetNotficiation.isNoLeaver)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_12'):SetVisible(targetNotficiation.isPrivate)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_13'):SetVisible(targetNotficiation.isDevHeroes)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon_14'):SetVisible(targetNotficiation.isBots)
			elseif (targetNotficiation.notificationType == 26) then	-- Replay Upload
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_1'):SetVisible(false)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_1'):SetWidth('100%')
				groupfcall('sp_notification_history_template_' .. slotIndex .. '_buttons_1', function(index, widget, groupName) widget:SetText(Translate('general_ignore')) end)

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_2'):SetVisible(true)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_2'):SetWidth('206%')
				groupfcall('sp_notification_history_template_' .. slotIndex .. '_buttons_2', function(index, widget, groupName) widget:SetText(Translate('match_stats_open')) end)

				GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(true)

				targetWidget:SetCallback('onclick', function()
					if (UIGamePhase() > 0) then return end
					Set('sp_notification_history_template_' .. slotIndex .. '_mouseover', true, 'bool')
					self:ScaleHeight('9.1h', 100)
					GetWidget('sp_notification_history_template_' .. slotIndex .. '_buttons'):FadeIn(100)
					GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(0)
				end)
				targetWidget:SetCallback('onmouseover', function()
					if (UIGamePhase() > 0) then return end
					Set('sp_notification_history_template_' .. slotIndex .. '_mouseover', true, 'bool')
					GetWidget('sp_notification_history_template_' .. slotIndex .. '_border'):FadeIn(100)
					self:Sleep(HOVER_DELAY, function()
						if GetCvarBool('sp_notification_history_template_' .. slotIndex .. '_mouseover') then
							self:ScaleHeight('9.1h', 100)
							GetWidget('sp_notification_history_template_' .. slotIndex .. '_buttons'):FadeIn(100)
							GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(0)
						end
					end)
				end)
				targetWidget:RefreshCallbacks()
			else
				GetWidget('sp_notification_history_expand_btn_' .. slotIndex):SetVisible(false)
				targetWidget:ClearCallback('onclick')
				targetWidget:SetCallback('onmouseover', function()
					GetWidget('sp_notification_history_template_' .. slotIndex .. '_border'):FadeIn(100)
				end)
				targetWidget:RefreshCallbacks()
			end

			local label_2 = Translate(targetNotficiation.notifyTranslateString .. '_dependent', 'version', targetNotficiation.sParam2, 'streamname', targetNotficiation.sParam1, 'name', targetNotficiation.sParam2, 'replay', targetNotficiation.sParam1)
			label_2 = NotEmpty(label_2) and (label_2 .. '\n') or ''
			label_2 = label_2 .. Translate(targetNotficiation.notifyTranslateString .. '_info', 'gamename', targetNotficiation.sParam2, 'rank', targetNotficiation.sParam3, 'name', targetNotficiation.sParam2)

			local sRemovedInfo = TranslateOrNil(targetNotficiation.notifyTranslateString .. '_info_removed', 'name', targetNotficiation.sParam2)
			if sRemovedInfo and NotEmpty(sRemovedInfo) then
				label_2 = label_2 .. '\n' .. Translate(targetNotficiation.notifyTranslateString .. '_info_removed', 'name', targetNotficiation.sParam2)
			end

			local notificationTitle = targetNotficiation.sParam1
			if (targetNotficiation.notificationType ~= 24) and (targetNotficiation.gameInfo3) and NotEmpty(targetNotficiation.gameInfo3) then
				notificationTitle = targetNotficiation.gameInfo3
			end
			if (targetNotficiation.notificationType == 26) then
				notificationTitle = Translate("notify_match_id")..targetNotficiation.sParam1
			end
			if targetNotficiation.notificationType >= 27 and targetNotficiation.notificationType <= 30 then
				notificationTitle = Translate(targetNotficiation.sParam1..'_situation')
			end

			GetWidget('sp_notification_history_template_' .. slotIndex .. '_label_1'):SetText(notificationTitle or '?') -- RMM '^r' .. slotIndex .. '|' .. (localOffset + searchOffset) .. '^* ' ..
			GetWidget('sp_notification_history_template_' .. slotIndex .. '_label_1'):SetColor(targetNotficiation.nameColor or '#ffb300')
			GetWidget('sp_notification_history_template_' .. slotIndex .. '_label_2'):SetText((FormatStringNewline(label_2 or '?')))
			GetWidget('sp_notification_history_template_' .. slotIndex .. '_label_3'):SetText(targetNotficiation.notificationTime or '?')

			if (not targetNotficiation.nameColor) and (not targetNotficiation.accountIcon) and (not targetNotficiation.nameGlow) then
				targetNotficiation.nameColor, targetNotficiation.accountIcon, targetNotficiation.nameGlow = GetUpgrades(notificationTitle)
			end

			if (targetNotficiation.accountIcon) and (targetNotficiation.accountIcon ~= '/ui/fe2/store/icons/account_icons/default.tga') then
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon'):SetColor('1 1 1 1')
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon'):SetTexture(targetNotficiation.accountIcon)
			else
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon'):SetColor('1 1 1 0.6')
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_icon'):SetTexture('/ui/fe2/store/icons/account_icons/default.tga')
			end

			local intNotificationID =  targetNotficiation.intNotificationID
			local notWidgetIndex = slotIndex
			if (targetNotficiation.inHistory) then
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_1'):SetCallback('onclick', function()
					if (intNotificationID) then
						HoN_Notifications.NotificationButtonClicked(1, notWidgetIndex, 'history', intNotificationID)
					end
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_1'):RefreshCallbacks()

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_2'):SetCallback('onclick', function()
					if (intNotificationID) then
						HoN_Notifications.NotificationButtonClicked(2, notWidgetIndex, 'history', intNotificationID)
					end
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_2'):RefreshCallbacks()

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_3'):SetCallback('onclick', function()
					if (intNotificationID) then
						HoN_Notifications.NotificationButtonClicked(3, notWidgetIndex, 'history', intNotificationID)
					end
				end)
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_3'):RefreshCallbacks()
			else
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_1'):SetCallback('onclick', CreateButtonOnclick(1, targetNotficiation, 'history', notWidgetIndex))
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_1'):RefreshCallbacks()

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_2'):SetCallback('onclick', CreateButtonOnclick(2, targetNotficiation, 'history', notWidgetIndex))
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_2'):RefreshCallbacks()

				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_3'):SetCallback('onclick', CreateButtonOnclick(3, targetNotficiation, 'history', notWidgetIndex))
				GetWidget('sp_notification_history_template_' .. slotIndex .. '_button_3'):RefreshCallbacks()
			end
		end
	end

	if GetWidget('sp_notification_history_template_'..HoN_Notifications.visibleHistorySlots, 'main', true) then
		GetWidget('notifications_remove_all_panel'):SetVisible((HoN_Notifications.historySize > 0)) -- (not GetWidget('sp_notification_history_template_'..HoN_Notifications.visibleHistorySlots, 'main', true):IsVisible()) and
	end
end

function HoN_Notifications:NotificationHistoryOnScroll(offset)

	local currentUserLine = offset or 0
	HoN_Notifications.userOffset = currentUserLine - HoN_Notifications.visibleHistorySlots
	if HoN_Notifications.userOffset < 0 then HoN_Notifications.userOffset = 0 end

	local listbox = interface:GetWidget('social_notifications_listbox')
	if (listbox) then
		listbox:UICmd("SetClipAreaToChild();")

		UpdateChatNotificationCounts()

		groupfcall('sp_notification_history_expand_btns', function(index, widget, groupName) widget:SetVisible(0) end)

		UpdateNotificationHistory(HoN_Notifications.userOffset or 0)
	end

	groupfcall('sp_notification_history_templates', function(index, widget, groupName) widget:DoEventN(2) end)
end

function HoN_Notifications:UpdateUserScroller(offset)
	local offset = offset or 0
	local scroller = interface:GetWidget('social_notifications_scroller')
	local currentValue = scroller:UICmd("GetValue()")
	if (HoN_Notifications.historySize > HoN_Notifications.visibleHistorySlots ) then
		scroller:SetVisible(true)
		scroller:SetEnabled(1)
		if (offset) and (currentValue ~= (HoN_Notifications.visibleHistorySlots + offset)) then
			scroller:UICmd("SetMaxValue("..(HoN_Notifications.historySize + 1).."); SetMinValue("..(HoN_Notifications.visibleHistorySlots).."); SetValue("..(HoN_Notifications.visibleHistorySlots + offset)..")")
		end
	elseif (HoN_Notifications.historySize >= HoN_Notifications.visibleHistorySlots ) then
		scroller:SetVisible(true)
		scroller:SetEnabled(1)
		if (offset) and (currentValue ~= (HoN_Notifications.visibleHistorySlots + offset)) then
			scroller:UICmd("SetMaxValue("..(HoN_Notifications.historySize + 1).."); SetMinValue("..(HoN_Notifications.visibleHistorySlots).."); SetValue("..(HoN_Notifications.visibleHistorySlots + offset)..")")
		end
	elseif (HoN_Notifications.historySize > 0 ) then
		scroller:SetVisible(true)
		scroller:SetEnabled(0)
		if (offset) and (currentValue ~= (HoN_Notifications.visibleHistorySlots + offset)) then
			scroller:UICmd("SetMaxValue("..(HoN_Notifications.historySize + 1).."); SetMinValue("..(HoN_Notifications.visibleHistorySlots).."); SetValue("..(HoN_Notifications.visibleHistorySlots + offset)..")")
		end
		HoN_Notifications.lastUserOffset = 0
	else
		scroller:SetVisible(true)
		scroller:SetEnabled(0)
		if (offset) and (currentValue ~= (HoN_Notifications.visibleHistorySlots + offset)) then
			scroller:UICmd("SetMaxValue(1); SetMinValue(1); SetValue(1)")
		end
		HoN_Notifications.lastUserOffset = 0
	end
end

function HoN_Notifications:InputBoxUpdate(input)
	if (input and input ~= "") then
		input = string.gsub(string.gsub(string.gsub(string.gsub(string.gsub(string.gsub(input, "%[", ""), "%%", ""), "%(", ""), "%]", ""), "%)", ""), "'", "")
	end

	if (input) then
		local length = string.len(input)
		if (length == 0 ) then
			HoN_Notifications.autoCompleteString = nil
			UpdateNotificationHistory(HoN_Notifications.userOffset or 0)
		else
			HoN_Notifications.autoCompleteString = input
			UpdateNotificationHistory(HoN_Notifications.userOffset or 0)
		end
	end
end

local function UpdateToastPositions(startPoint, endPoint, targetInterface)
	local prevHeight = 4
	for i = startPoint, endPoint, 1 do
		local toastWidgetTable = HoN_Notifications.toastWidgetTable[i]
		if (not toastWidgetTable.isClosing) and (not toastWidgetTable.isOpening) then
			GetWidget('sp_notification_toast_template_' .. i, targetInterface):SetY(prevHeight)
		end
		prevHeight = prevHeight + GetWidget('sp_notification_toast_template_' .. i, targetInterface):GetHeight() + GetWidget('sp_notification_toast_template_' .. i, targetInterface):GetHeightFromString('0.1h')
	end
end

local function ShowToast(toastIndex, fromInterface)
	if GetWidget('sp_notification_toast_template_' .. toastIndex, fromInterface, true) then
		local toastWidgetTable = HoN_Notifications.toastWidgetTable[toastIndex]
		if (not GetWidget('sp_notification_toast_template_' .. toastIndex, fromInterface):IsVisible()) and (toastWidgetTable) then
			toastWidgetTable.isOpening = true
			toastWidgetTable.openFinish = HostTime() + 850
			GetWidget('sp_notification_toast_template_' .. toastIndex, fromInterface):SetX('30h')
			GetWidget('sp_notification_toast_template_' .. toastIndex, fromInterface):SetVisible(1)
			GetWidget('sp_notification_toast_template_' .. toastIndex, fromInterface):SlideX('0', 500, true)
			GetWidget('sp_notification_toast_template_' .. toastIndex, fromInterface):Sleep(500, function()
				GetWidget('sp_notification_toast_template_' .. toastIndex, fromInterface):SetX('0')
				GetWidget('sp_notification_toast_template_' .. toastIndex, fromInterface):Scale('27.5h', '6.5h', 150)
				GetWidget('sp_notification_toast_template_' .. toastIndex, fromInterface):Sleep(150, function()
					GetWidget('sp_notification_toast_template_' .. toastIndex, fromInterface):Scale('25.0h', '6.5h', 150)
					GetWidget('sp_notification_toast_template_' .. toastIndex, fromInterface):Sleep(150, function()
						toastWidgetTable.isOpening = false
					end)
				end)
			end)
		end
	end
end

local function HideToast(toastIndex, fromInterface, toastWidgetTable)
	if GetWidget('sp_notification_toast_template_' .. toastIndex, fromInterface, true) then
		if GetWidget('sp_notification_toast_template_' .. toastIndex, fromInterface):IsVisible() then
			GetWidget('sp_notification_toast_template_' .. toastIndex, fromInterface):SlideX('30h', 500, true)
			GetWidget('sp_notification_toast_template_' .. toastIndex, fromInterface):Sleep(500, function()
				toastWidgetTable.isAvailable = true
				toastWidgetTable.isClosing = false
				HoN_Notifications.toastWidgetActive = HoN_Notifications.toastWidgetActive - 1
				GetWidget('sp_notification_toast_template_' .. toastIndex, fromInterface):SetVisible(0)
			end)
		else
			toastWidgetTable.isAvailable = true
			toastWidgetTable.isClosing = false
		end
	else
		toastWidgetTable.isAvailable = true
		toastWidgetTable.isClosing = false
	end
end

local function InstantiateToastNotification(spawnWidget, targetInterface, slotIndex, targetNotficiation)
	targetWidget = GetWidget('sp_notification_toast_template_' .. slotIndex, targetInterface, true)

	if targetNotficiation and targetWidget then
		targetNotficiation.targetWidget = targetWidget

		ShowToast(slotIndex, targetInterface)

		PlayNotificationSound(targetNotficiation.notificationType)

		if (targetNotficiation.notificationType == 20) or (targetNotficiation.notificationType == 15) or (targetNotficiation.notificationType == 11) then	-- Game invite / clan join / buddy join

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_1', targetInterface):SetVisible(true)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_1', targetInterface):SetWidth('100%')
			groupfcall('sp_notification_toast_template_' .. slotIndex .. '_buttons_1', function(index, widget, groupName) widget:SetText(Translate('general_ignore')) end, targetInterface)

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_2', targetInterface):SetVisible(true)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_2', targetInterface):SetWidth('100%')
			groupfcall('sp_notification_toast_template_' .. slotIndex .. '_buttons_2', function(index, widget, groupName) widget:SetText(Translate('notify_join_game')) end, targetInterface)

			GetWidget('sp_notification_history_expand_btn_toast_' .. slotIndex, targetInterface):SetVisible(true)

			if (targetNotficiation.notificationType == 20) then
				targetWidget:SetCallback('onevent2', function()
					if (HoN_Notifications.toastWidgetTable[slotIndex].isClosing) then return end
					Set('sp_notification_toast_template_' .. slotIndex .. '_mouseover', true, 'bool')
					GetWidget("sp_notification_sleeper_"..slotIndex, targetInterface):Sleep(1, function() end)
					HoN_Notifications.toastWidgetTable[slotIndex].isOpen = true
					self:ScaleHeight('11.9h', 100)
					GetWidget('sp_notification_toast_template_' .. slotIndex .. '_buttons', targetInterface):FadeIn(100)
					GetWidget('sp_notification_toast_template_' .. slotIndex .. '_game_info', targetInterface):FadeIn(100)
				end)
				targetWidget:SetCallback('onevent3', function()
					if (HoN_Notifications.toastWidgetTable[slotIndex].isClosing) then return end
					HoN_Notifications.MouseoverToastNotification(true)
					Set('sp_notification_toast_template_' .. slotIndex .. '_mouseover', true, 'bool')
					self:Sleep(HOVER_DELAY_SHORT, function()
						if GetCvarBool('sp_notification_toast_template_' .. slotIndex .. '_mouseover') then
							GetWidget("sp_notification_sleeper_"..slotIndex, targetInterface):Sleep(1, function() end)
							HoN_Notifications.toastWidgetTable[slotIndex].isOpen = true
							self:ScaleHeight('11.9h', 100)
							GetWidget('sp_notification_toast_template_' .. slotIndex .. '_buttons', targetInterface):FadeIn(100)
							GetWidget('sp_notification_toast_template_' .. slotIndex .. '_game_info', targetInterface):FadeIn(100)
						end
					end)
				end)
			else
				targetWidget:SetCallback('onevent2', function()
					if (HoN_Notifications.toastWidgetTable[slotIndex].isClosing) then return end
					Set('sp_notification_toast_template_' .. slotIndex .. '_mouseover', true, 'bool')
					GetWidget("sp_notification_sleeper_"..slotIndex, targetInterface):Sleep(1, function() end)
					HoN_Notifications.toastWidgetTable[slotIndex].isOpen = true
					self:ScaleHeight('9.1h', 100)
					GetWidget('sp_notification_toast_template_' .. slotIndex .. '_game_info', targetInterface):FadeIn(100)
				end)
				targetWidget:SetCallback('onevent3', function()
					if (HoN_Notifications.toastWidgetTable[slotIndex].isClosing) then return end
					HoN_Notifications.MouseoverToastNotification(true)
					Set('sp_notification_toast_template_' .. slotIndex .. '_mouseover', true, 'bool')
					self:Sleep(HOVER_DELAY_SHORT, function()
						if GetCvarBool('sp_notification_toast_template_' .. slotIndex .. '_mouseover') then
							GetWidget("sp_notification_sleeper_"..slotIndex, targetInterface):Sleep(1, function() end)
							HoN_Notifications.toastWidgetTable[slotIndex].isOpen = true
							self:ScaleHeight('9.1h', 100)
							GetWidget('sp_notification_toast_template_' .. slotIndex .. '_game_info', targetInterface):FadeIn(100)
						end
					end)
				end)
				targetWidget:RefreshCallbacks()
			end

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_label_4', targetInterface):SetText(targetNotficiation.gameInfo2 or '?')
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_label_5', targetInterface):SetText(targetNotficiation.gameInfo4 .. ' - ' .. targetNotficiation.teamSize)

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_1', targetInterface):SetTexture('/maps/' .. targetNotficiation.mapName .. '/icon.tga')
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_2', targetInterface):SetTexture('/ui/icons/' .. targetNotficiation.gameMode .. '.tga')
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_3', targetInterface):SetVisible(targetNotficiation.isVerifiedOnly)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_4', targetInterface):SetVisible(targetNotficiation.isCasualMode)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_5', targetInterface):SetVisible(targetNotficiation.isHardcore)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_6', targetInterface):SetVisible(targetNotficiation.isGated)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_7', targetInterface):SetVisible(targetNotficiation.isAdvOptions)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_8', targetInterface):SetVisible(targetNotficiation.isAutoBalanced)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_9', targetInterface):SetVisible(targetNotficiation.statsLevel == '1')
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_10', targetInterface):SetVisible(targetNotficiation.statsLevel == '2')
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_11', targetInterface):SetVisible(targetNotficiation.isNoLeaver)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_12', targetInterface):SetVisible(targetNotficiation.isPrivate)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_13', targetInterface):SetVisible(targetNotficiation.isDevHeroes)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_14', targetInterface):SetVisible(targetNotficiation.isBots)

			local function MouseOut()
				Trigger('genericMainFloatingTip', 'false', '', '', '', '', '', '', '', '')
				Set('sp_notification_toast_template_' ..slotIndex .. '_mouseover', 'false', 'bool')
				GetWidget('sp_notification_toast_template_' .. slotIndex, targetInterface):DoEventN(1)
			end

			local mapName = targetNotficiation.mapName
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_1', targetInterface):SetCallback('onmouseover', function()
				HoN_Notifications.MouseoverToastNotification(true)
				Trigger('genericMainFloatingTip', 'true', '18h', '/maps/' .. mapName .. '/icon.tga', 'mainlobby_gamelist_'..mapName..'_title', '', '', '', '-18h', '1h')
				Set('sp_notification_toast_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_1', targetInterface):SetCallback('onmouseout', function()
				MouseOut()
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_1', targetInterface):RefreshCallbacks()

			local gameMode = targetNotficiation.gameMode
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_2', targetInterface):SetCallback('onmouseover', function()
				HoN_Notifications.MouseoverToastNotification(true)
				Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/' .. gameMode .. '.tga', 'mainlobby_gamelist_'..gameMode..'_title', '', '', '', '-18h', '1h')
				Set('sp_notification_toast_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_2', targetInterface):SetCallback('onmouseout', function()
				MouseOut()
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_2', targetInterface):RefreshCallbacks()

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_3', targetInterface):SetCallback('onmouseover', function()
				HoN_Notifications.MouseoverToastNotification(true)
				Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/verified.tga', 'mainlobby_gamelist_verifiedonly_title', 'mainlobby_gamelist_verifiedonly_desc', '', '', '-18h', '1h')
				Set('sp_notification_toast_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_3', targetInterface):SetCallback('onmouseout', function()
				MouseOut()
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_3', targetInterface):RefreshCallbacks()

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_4', targetInterface):SetCallback('onmouseover', function()
				HoN_Notifications.MouseoverToastNotification(true)
				Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/casual.tga', 'mainlobby_gamelist_casual_title', 'mainlobby_gamelist_casual_desc', '', '', '-18h', '1h')
				Set('sp_notification_toast_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_4', targetInterface):SetCallback('onmouseout', function()
				MouseOut()
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_4', targetInterface):RefreshCallbacks()

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_5', targetInterface):SetCallback('onmouseover', function()
				HoN_Notifications.MouseoverToastNotification(true)
				Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/hardcore.tga', 'mainlobby_gamelist_hardcore_title', 'mainlobby_gamelist_hardcore_desc', '', '', '-18h', '1h')
				Set('sp_notification_toast_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_5', targetInterface):SetCallback('onmouseout', function()
				MouseOut()
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_5', targetInterface):RefreshCallbacks()

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_6', targetInterface):SetCallback('onmouseover', function()
				HoN_Notifications.MouseoverToastNotification(true)
				Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/gated.tga', 'mainlobby_gamelist_gated_title', 'mainlobby_gamelist_gated_desc', '', '', '-18h', '1h')
				Set('sp_notification_toast_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_6', targetInterface):SetCallback('onmouseout', function()
				MouseOut()
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_6', targetInterface):RefreshCallbacks()

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_7', targetInterface):SetCallback('onmouseover', function()
				HoN_Notifications.MouseoverToastNotification(true)
				Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/more.tga', 'mainlobby_gamelist_more_title', 'mainlobby_gamelist_more_desc', '', '', '-18h', '1h')
				Set('sp_notification_toast_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_7', targetInterface):SetCallback('onmouseout', function()
				MouseOut()
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_7', targetInterface):RefreshCallbacks()

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_8', targetInterface):SetCallback('onmouseover', function()
				HoN_Notifications.MouseoverToastNotification(true)
				Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/autobalance.tga', 'mainlobby_gamelist_autobalance_title', 'mainlobby_gamelist_autobalance_desc', '', '', '-18h', '1h')
				Set('sp_notification_toast_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_8', targetInterface):SetCallback('onmouseout', function()
				MouseOut()
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_8', targetInterface):RefreshCallbacks()

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_9', targetInterface):SetCallback('onmouseover', function()
				HoN_Notifications.MouseoverToastNotification(true)
				Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/official.tga', 'mainlobby_gamelist_official_title', 'mainlobby_gamelist_official_desc', '', '', '-18h', '1h')
				Set('sp_notification_toast_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_9', targetInterface):SetCallback('onmouseout', function()
				MouseOut()
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_9', targetInterface):RefreshCallbacks()

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_10', targetInterface):SetCallback('onmouseover', function()
				HoN_Notifications.MouseoverToastNotification(true)
				Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/official_2.tga', 'mainlobby_gamelist_official_2_title', 'mainlobby_gamelist_official_2_desc', '', '', '-18h', '1h')
				Set('sp_notification_toast_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_10', targetInterface):SetCallback('onmouseout', function()
				MouseOut()
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_10', targetInterface):RefreshCallbacks()

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_11', targetInterface):SetCallback('onmouseover', function()
				HoN_Notifications.MouseoverToastNotification(true)
				Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/noleaver.tga', 'mainlobby_gamelist_noleaver_title', 'mainlobby_gamelist_noleaver_desc', '', '', '-18h', '1h')
				Set('sp_notification_toast_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_11', targetInterface):SetCallback('onmouseout', function()
				MouseOut()
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_11', targetInterface):RefreshCallbacks()

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_12', targetInterface):SetCallback('onmouseover', function()
				HoN_Notifications.MouseoverToastNotification(true)
				Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/private.tga', 'mainlobby_gamelist_private_title', 'mainlobby_gamelist_private_desc', '', '', '-18h', '1h')
				Set('sp_notification_toast_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_12', targetInterface):SetCallback('onmouseout', function()
				MouseOut()
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_12', targetInterface):RefreshCallbacks()

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_13', targetInterface):SetCallback('onmouseover', function()
				HoN_Notifications.MouseoverToastNotification(true)
				Trigger('genericMainFloatingTip', 'true', '18h', '/ui/icons/devheroes.tga', 'mainlobby_gamelist_devheroes_title', 'mainlobby_gamelist_devheroes_desc', '', '', '-18h', '1h')
				Set('sp_notification_toast_template_' ..slotIndex .. '_mouseover', 'true', 'bool')
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_13', targetInterface):SetCallback('onmouseout', function()
				MouseOut()
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_13', targetInterface):RefreshCallbacks()

		elseif (targetNotficiation.notificationType == 24)  then --  group invite

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_1', targetInterface):SetVisible(true)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_1', targetInterface):SetWidth('100%')
			groupfcall('sp_notification_toast_template_' .. slotIndex .. '_buttons_1', function(index, widget, groupName) widget:SetText(Translate('general_ignore')) end, targetInterface)

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_2', targetInterface):SetVisible(true)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_2', targetInterface):SetWidth('100%')
			groupfcall('sp_notification_toast_template_' .. slotIndex .. '_buttons_2', function(index, widget, groupName) widget:SetText(Translate('notify_join_group')) end, targetInterface)

			GetWidget('sp_notification_history_expand_btn_toast_' .. slotIndex, targetInterface):SetVisible(true)

			targetWidget:SetCallback('onevent2', function()
				if (HoN_Notifications.toastWidgetTable[slotIndex].isClosing) then return end
				Set('sp_notification_toast_template_' .. slotIndex .. '_mouseover', true, 'bool')
				GetWidget("sp_notification_sleeper_"..slotIndex, targetInterface):Sleep(1, function() end)
				HoN_Notifications.toastWidgetTable[slotIndex].isOpen = true
				self:ScaleHeight('9.1h', 100)
				GetWidget('sp_notification_toast_template_' .. slotIndex .. '_buttons', targetInterface):FadeIn(100)
			end)
			targetWidget:SetCallback('onevent3', function()
				if (HoN_Notifications.toastWidgetTable[slotIndex].isClosing) then return end
				HoN_Notifications.MouseoverToastNotification(true)
				Set('sp_notification_toast_template_' .. slotIndex .. '_mouseover', true, 'bool')
				self:Sleep(HOVER_DELAY_SHORT, function()
					if GetCvarBool('sp_notification_toast_template_' .. slotIndex .. '_mouseover') then
						GetWidget("sp_notification_sleeper_"..slotIndex, targetInterface):Sleep(1, function() end)
						HoN_Notifications.toastWidgetTable[slotIndex].isOpen = true
						self:ScaleHeight('9.1h', 100)
						GetWidget('sp_notification_toast_template_' .. slotIndex .. '_buttons', targetInterface):FadeIn(100)
					end
				end)
			end)
			targetWidget:RefreshCallbacks()

		elseif (targetNotficiation.notificationType == 23) then	-- buddy request

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_1', targetInterface):SetVisible(true)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_1', targetInterface):SetWidth('100%')
			groupfcall('sp_notification_toast_template_' .. slotIndex .. '_buttons_1', function(index, widget, groupName) widget:SetText(Translate('general_ignore')) end, targetInterface)

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_2', targetInterface):SetVisible(true)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_2', targetInterface):SetWidth('100%')
			groupfcall('sp_notification_toast_template_' .. slotIndex .. '_buttons_2', function(index, widget, groupName) widget:SetText(Translate('general_approve')) end, targetInterface)

			GetWidget('sp_notification_history_expand_btn_toast_' .. slotIndex, targetInterface):SetVisible(true)

			targetWidget:SetCallback('onevent2', function()
				if (HoN_Notifications.toastWidgetTable[slotIndex].isClosing) then return end
				Set('sp_notification_toast_template_' .. slotIndex .. '_mouseover', true, 'bool')
				GetWidget("sp_notification_sleeper_"..slotIndex, targetInterface):Sleep(1, function() end)
				HoN_Notifications.toastWidgetTable[slotIndex].isOpen = true
				self:ScaleHeight('9.1h', 100)
				GetWidget('sp_notification_toast_template_' .. slotIndex .. '_buttons', targetInterface):FadeIn(100)
			end)
			targetWidget:SetCallback('onevent3', function()
				if (HoN_Notifications.toastWidgetTable[slotIndex].isClosing) then return end
				HoN_Notifications.MouseoverToastNotification(true)
				Set('sp_notification_toast_template_' .. slotIndex .. '_mouseover', true, 'bool')
				self:Sleep(HOVER_DELAY_SHORT, function()
					if GetCvarBool('sp_notification_toast_template_' .. slotIndex .. '_mouseover') then
						GetWidget("sp_notification_sleeper_"..slotIndex, targetInterface):Sleep(1, function() end)
						HoN_Notifications.toastWidgetTable[slotIndex].isOpen = true
						self:ScaleHeight('9.1h', 100)
						GetWidget('sp_notification_toast_template_' .. slotIndex .. '_buttons', targetInterface):FadeIn(100)
					end
				end)
			end)
			targetWidget:RefreshCallbacks()

		elseif (targetNotficiation.notificationType == 25) then	-- stream

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_1', targetInterface):SetVisible(false)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_1', targetInterface):SetWidth('100%')
			groupfcall('sp_notification_toast_template_' .. slotIndex .. '_buttons_1', function(index, widget, groupName) widget:SetText(Translate('general_ignore')) end, targetInterface)

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_2', targetInterface):SetVisible(true)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_2', targetInterface):SetWidth('206%')
			groupfcall('sp_notification_toast_template_' .. slotIndex .. '_buttons_2', function(index, widget, groupName) widget:SetText(Translate('notify_watch')) end, targetInterface)

			GetWidget('sp_notification_history_expand_btn_toast_' .. slotIndex, targetInterface):SetVisible(true)

			targetWidget:SetCallback('onevent2', function()
				if (UIGamePhase() > 0) then return end
				if (HoN_Notifications.toastWidgetTable[slotIndex].isClosing) then return end
				Set('sp_notification_toast_template_' .. slotIndex .. '_mouseover', true, 'bool')
				GetWidget("sp_notification_sleeper_"..slotIndex, targetInterface):Sleep(1, function() end)
				HoN_Notifications.toastWidgetTable[slotIndex].isOpen = true
				self:ScaleHeight('9.1h', 100)
				GetWidget('sp_notification_toast_template_' .. slotIndex .. '_buttons', targetInterface):FadeIn(100)
			end)
			targetWidget:SetCallback('onevent3', function()
				if (UIGamePhase() > 0) then return end
				if (HoN_Notifications.toastWidgetTable[slotIndex].isClosing) then return end
				HoN_Notifications.MouseoverToastNotification(true)
				Set('sp_notification_toast_template_' .. slotIndex .. '_mouseover', true, 'bool')
				self:Sleep(HOVER_DELAY_SHORT, function()
					if GetCvarBool('sp_notification_toast_template_' .. slotIndex .. '_mouseover') then
						GetWidget("sp_notification_sleeper_"..slotIndex, targetInterface):Sleep(1, function() end)
						HoN_Notifications.toastWidgetTable[slotIndex].isOpen = true
						self:ScaleHeight('9.1h', 100)
						GetWidget('sp_notification_toast_template_' .. slotIndex .. '_buttons', targetInterface):FadeIn(100)
					end
				end)
			end)
			targetWidget:RefreshCallbacks()

		elseif (targetNotficiation.notificationType == 17) then	-- patch

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_1', targetInterface):SetVisible(false)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_1', targetInterface):SetWidth('100%')
			groupfcall('sp_notification_toast_template_' .. slotIndex .. '_buttons_1', function(index, widget, groupName) widget:SetText(Translate('general_ignore')) end, targetInterface)

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_2', targetInterface):SetVisible(true)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_2', targetInterface):SetWidth('206%')
			groupfcall('sp_notification_toast_template_' .. slotIndex .. '_buttons_2', function(index, widget, groupName) widget:SetText(Translate('notify_update_now')) end, targetInterface)

			GetWidget('sp_notification_history_expand_btn_toast_' .. slotIndex, targetInterface):SetVisible(true)

			targetWidget:SetCallback('onevent2', function()
				if (HoN_Notifications.toastWidgetTable[slotIndex].isClosing) then return end
				Set('sp_notification_toast_template_' .. slotIndex .. '_mouseover', true, 'bool')
				GetWidget("sp_notification_sleeper_"..slotIndex, targetInterface):Sleep(1, function() end)
				HoN_Notifications.toastWidgetTable[slotIndex].isOpen = true
				self:ScaleHeight('9.1h', 100)
				GetWidget('sp_notification_toast_template_' .. slotIndex .. '_buttons', targetInterface):FadeIn(100)
			end)
			targetWidget:SetCallback('onevent3', function()
				if (HoN_Notifications.toastWidgetTable[slotIndex].isClosing) then return end
				HoN_Notifications.MouseoverToastNotification(true)
				Set('sp_notification_toast_template_' .. slotIndex .. '_mouseover', true, 'bool')
				self:Sleep(HOVER_DELAY_SHORT, function()
					if GetCvarBool('sp_notification_toast_template_' .. slotIndex .. '_mouseover') then
						GetWidget("sp_notification_sleeper_"..slotIndex, targetInterface):Sleep(1, function() end)
						HoN_Notifications.toastWidgetTable[slotIndex].isOpen = true
						self:ScaleHeight('9.1h', 100)
						GetWidget('sp_notification_toast_template_' .. slotIndex .. '_buttons', targetInterface):FadeIn(100)
					end
				end)
			end)
			targetWidget:RefreshCallbacks()

		elseif (targetNotficiation.notificationType == 21) then	-- self join

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_1', targetInterface):SetVisible(false)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_1', targetInterface):SetWidth('100%')
			groupfcall('sp_notification_toast_template_' .. slotIndex .. '_buttons_1', function(index, widget, groupName) widget:SetText(Translate('general_ignore')) end, targetInterface)

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_2', targetInterface):SetVisible(true)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_2', targetInterface):SetWidth('206%')
			groupfcall('sp_notification_toast_template_' .. slotIndex .. '_buttons_2', function(index, widget, groupName) widget:SetText(Translate('notify_join_game')) end, targetInterface)

			GetWidget('sp_notification_history_expand_btn_toast_' .. slotIndex, targetInterface):SetVisible(true)

			targetWidget:SetCallback('onevent2', function()
				if (HoN_Notifications.toastWidgetTable[slotIndex].isClosing) then return end
				Set('sp_notification_toast_template_' .. slotIndex .. '_mouseover', true, 'bool')
				GetWidget("sp_notification_sleeper_"..slotIndex, targetInterface):Sleep(1, function() end)
				HoN_Notifications.toastWidgetTable[slotIndex].isOpen = true
				self:ScaleHeight('11.9h', 100)
				GetWidget('sp_notification_toast_template_' .. slotIndex .. '_buttons', targetInterface):FadeIn(100)
				GetWidget('sp_notification_toast_template_' .. slotIndex .. '_game_info', targetInterface):FadeIn(100)
			end)
			targetWidget:SetCallback('onevent3', function()
				if (HoN_Notifications.toastWidgetTable[slotIndex].isClosing) then return end
				HoN_Notifications.MouseoverToastNotification(true)
				Set('sp_notification_toast_template_' .. slotIndex .. '_mouseover', true, 'bool')
				self:Sleep(HOVER_DELAY_SHORT, function()
					if GetCvarBool('sp_notification_toast_template_' .. slotIndex .. '_mouseover') then
						GetWidget("sp_notification_sleeper_"..slotIndex, targetInterface):Sleep(1, function() end)
						HoN_Notifications.toastWidgetTable[slotIndex].isOpen = true
						self:ScaleHeight('11.9h', 100)
						GetWidget('sp_notification_toast_template_' .. slotIndex .. '_buttons', targetInterface):FadeIn(100)
						GetWidget('sp_notification_toast_template_' .. slotIndex .. '_game_info', targetInterface):FadeIn(100)
					end
				end)
			end)
			targetWidget:RefreshCallbacks()

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_label_4', targetInterface):SetText(targetNotficiation.gameInfo2 or '?')
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_label_5', targetInterface):SetText(targetNotficiation.gameInfo4 .. ' - ' .. targetNotficiation.teamSize)

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_1', targetInterface):SetTexture('/maps/' .. targetNotficiation.mapName .. '/icon.tga')
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_2', targetInterface):SetTexture('/ui/icons/' .. targetNotficiation.gameMode .. '.tga')
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_3', targetInterface):SetVisible(targetNotficiation.isVerifiedOnly)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_4', targetInterface):SetVisible(targetNotficiation.isCasualMode)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_5', targetInterface):SetVisible(targetNotficiation.isHardcore)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_6', targetInterface):SetVisible(targetNotficiation.isGated)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_7', targetInterface):SetVisible(targetNotficiation.isAdvOptions)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_8', targetInterface):SetVisible(targetNotficiation.isAutoBalanced)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_9', targetInterface):SetVisible(targetNotficiation.statsLevel == '1')
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_10', targetInterface):SetVisible(targetNotficiation.statsLevel == '2')
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_11', targetInterface):SetVisible(targetNotficiation.isNoLeaver)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_12', targetInterface):SetVisible(targetNotficiation.isPrivate)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_13', targetInterface):SetVisible(targetNotficiation.isDevHeroes)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon_14', targetInterface):SetVisible(targetNotficiation.isBots)

		elseif (targetNotficiation.notificationType == 26) then	-- Replay Upload
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_1', targetInterface):SetVisible(false)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_1', targetInterface):SetWidth('100%')
			groupfcall('sp_notification_toast_template_' .. slotIndex .. '_buttons_1', function(index, widget, groupName) widget:SetText(Translate('general_ignore')) end, targetInterface)

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_2', targetInterface):SetVisible(true)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_2', targetInterface):SetWidth('206%')
			groupfcall('sp_notification_toast_template_' .. slotIndex .. '_buttons_2', function(index, widget, groupName) widget:SetText(Translate('match_stats_open')) end, targetInterface)

			GetWidget('sp_notification_history_expand_btn_toast_' .. slotIndex, targetInterface):SetVisible(true)

			targetWidget:SetCallback('onevent2', function()
				if (UIGamePhase() > 0) then return end
				if (HoN_Notifications.toastWidgetTable[slotIndex].isClosing) then return end
				Set('sp_notification_toast_template_' .. slotIndex .. '_mouseover', true, 'bool')
				GetWidget("sp_notification_sleeper_"..slotIndex, targetInterface):Sleep(1, function() end)
				HoN_Notifications.toastWidgetTable[slotIndex].isOpen = true
				self:ScaleHeight('9.1h', 100)
				GetWidget('sp_notification_toast_template_' .. slotIndex .. '_buttons', targetInterface):FadeIn(100)
			end)
			targetWidget:SetCallback('onevent3', function()
				if (UIGamePhase() > 0) then return end
				if (HoN_Notifications.toastWidgetTable[slotIndex].isClosing) then return end
				HoN_Notifications.MouseoverToastNotification(true)
				Set('sp_notification_toast_template_' .. slotIndex .. '_mouseover', true, 'bool')
				self:Sleep(HOVER_DELAY_SHORT, function()
					if GetCvarBool('sp_notification_toast_template_' .. slotIndex .. '_mouseover') then
						GetWidget("sp_notification_sleeper_"..slotIndex, targetInterface):Sleep(1, function() end)
						HoN_Notifications.toastWidgetTable[slotIndex].isOpen = true
						self:ScaleHeight('9.1h', 100)
						GetWidget('sp_notification_toast_template_' .. slotIndex .. '_buttons', targetInterface):FadeIn(100)
					end
				end)
			end)
			targetWidget:RefreshCallbacks()
		else
			GetWidget('sp_notification_history_expand_btn_toast_' .. slotIndex, targetInterface):SetVisible(false)
			targetWidget:ClearCallback('onevent2')
			targetWidget:SetCallback('onevent3', function()
				if (HoN_Notifications.toastWidgetTable[slotIndex].isClosing) then return end
				HoN_Notifications.MouseoverToastNotification(true)
			end)
			targetWidget:RefreshCallbacks()
		end

		targetWidget:SetCallback('onmouseover', function()
			if (HoN_Notifications.toastWidgetTable[slotIndex].isOpening) then
				GetWidget("sp_notification_sleeper_"..slotIndex, targetInterface):Sleep(
					HoN_Notifications.toastWidgetTable[slotIndex].openFinish - HostTime(),
					function() GetWidget('sp_notification_toast_template_' .. slotIndex, targetInterface, true):DoEventN(2) end
				)
			else
				self:DoEventN(2)
			end
		end)

		targetWidget:SetCallback('onclick', function()
			if (HoN_Notifications.toastWidgetTable[slotIndex].isOpening) then
				GetWidget("sp_notification_sleeper_"..slotIndex, targetInterface):Sleep(
					HoN_Notifications.toastWidgetTable[slotIndex].openFinish - HostTime(),
					function() GetWidget('sp_notification_toast_template_' .. slotIndex, targetInterface, true):DoEventN(3) end
				)
			else
				self:DoEventN(3)
			end
		end)

		targetWidget:RefreshCallbacks()
		local label_2 = Translate(targetNotficiation.notifyTranslateString .. '_dependent', 'version', targetNotficiation.sParam2, 'streamname', targetNotficiation.sParam1, 'name', targetNotficiation.sParam2)

		if (label_2) and NotEmpty(label_2) then
			label_2 = label_2 .. '\n'
		else
			label_2 = ''
		end

		label_2 = label_2 .. Translate(targetNotficiation.notifyTranslateString .. '_info', 'gamename', targetNotficiation.sParam2, 'rank', targetNotficiation.sParam3, 'name', targetNotficiation.sParam2)

		local sRemovedInfo = TranslateOrNil(targetNotficiation.notifyTranslateString .. '_info_removed', 'name', targetNotficiation.sParam2)
		if sRemovedInfo and NotEmpty(sRemovedInfo) then
			label_2 = label_2 .. '\n' .. Translate(targetNotficiation.notifyTranslateString .. '_info_removed', 'name', targetNotficiation.sParam2)
		end

		local notificationTitle = targetNotficiation.sParam1
		if (targetNotficiation.notificationType ~= 24) and (targetNotficiation.gameInfo3) and NotEmpty(targetNotficiation.gameInfo3) then
			notificationTitle = targetNotficiation.gameInfo3
		end
		if (targetNotficiation.notificationType == 26) then
			notificationTitle = Translate("notify_match_id")..targetNotficiation.sParam1
		end
		if targetNotficiation.notificationType >= 27 and targetNotficiation.notificationType <= 30 then
			notificationTitle = Translate(targetNotficiation.sParam1..'_situation')
		end

		GetWidget('sp_notification_toast_template_' .. slotIndex .. '_label_1', targetInterface):SetText(notificationTitle or '?')
		GetWidget('sp_notification_toast_template_' .. slotIndex .. '_label_1', targetInterface):SetColor(targetNotficiation.nameColor or '#ffb300')

		if (targetNotficiation.nameGlow and NotEmpty(targetNotficiation.nameGlow)) then
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_label_1', targetInterface):SetGlow(AtoB(targetNotficiation.nameGlow))
		else
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_label_1', targetInterface):SetGlow(false)
		end

		if (targetNotficiation.nameGlowColor and NotEmpty(targetNotficiation.nameGlowColor)) then
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_label_1', targetInterface):SetGlowColor(targetNotficiation.nameGlowColor)
		else
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_label_1', targetInterface):SetGlowColor('')
		end

		GetWidget('sp_notification_toast_template_' .. slotIndex .. '_label_2', targetInterface):SetText((FormatStringNewline(label_2 or '?')))
		GetWidget('sp_notification_toast_template_' .. slotIndex .. '_label_3', targetInterface):SetText(targetNotficiation.notificationTime or '?')

		if (not targetNotficiation.nameColor) and (not targetNotficiation.accountIcon) and (not targetNotficiation.nameGlow) then
			targetNotficiation.nameColor, targetNotficiation.accountIcon, targetNotficiation.nameGlow = GetUpgrades(notificationTitle)
		end

		if (targetNotficiation.accountIcon) and (targetNotficiation.accountIcon ~= '/ui/fe2/store/icons/account_icons/default.tga') then
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon', targetInterface):SetColor('1 1 1 1')
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon', targetInterface):SetTexture(targetNotficiation.accountIcon)
		else
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon', targetInterface):SetColor('1 1 1 0.6')
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_icon', targetInterface):SetTexture('/ui/fe2/store/icons/account_icons/default.tga')
		end

		local intNotificationID =  targetNotficiation.intNotificationID
		local notWidgetIndex = slotIndex
		if (targetNotficiation.inHistory) then
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_1', targetInterface):SetCallback('onclick', function()
				if (intNotificationID) then
					HoN_Notifications.NotificationButtonClicked(1, notWidgetIndex, 'toast', intNotificationID)
				end
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_1', targetInterface):RefreshCallbacks()

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_2', targetInterface):SetCallback('onclick', function()
				if (intNotificationID) then
					HoN_Notifications.NotificationButtonClicked(2, notWidgetIndex, 'toast', intNotificationID)
				end
			end)
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_2', targetInterface):RefreshCallbacks()

			GetWidget('sp_notification_history_template_toast_' .. slotIndex .. '_button_3', targetInterface):SetCallback('onclick', function()
				if (intNotificationID) then
					HoN_Notifications.NotificationButtonClicked(3, notWidgetIndex, 'toast', intNotificationID)
				end
			end)
			GetWidget('sp_notification_history_template_toast_' .. slotIndex .. '_button_3', targetInterface):RefreshCallbacks()

		else
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_1', targetInterface):SetCallback('onclick', CreateButtonOnclick(1, targetNotficiation, 'toast', notWidgetIndex))
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_1', targetInterface):RefreshCallbacks()

			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_2', targetInterface):SetCallback('onclick', CreateButtonOnclick(2, targetNotficiation, 'toast', notWidgetIndex))
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_2', targetInterface):RefreshCallbacks()

			GetWidget('sp_notification_history_template_toast_' .. slotIndex .. '_button_3', targetInterface):SetCallback('onclick', CreateButtonOnclick(3, targetNotficiation, 'toast', notWidgetIndex))
			GetWidget('sp_notification_history_template_toast_' .. slotIndex .. '_button_3', targetInterface):RefreshCallbacks()

			GetWidget('sp_notification_toast_template_' .. slotIndex, targetInterface):SetCallback('onrightclick', CreateButtonOnclick(3, targetNotficiation, 'toast', notWidgetIndex))
		end

		if rap2Enable then
			GetWidget('sp_notification_toast_template_' .. slotIndex .. '_button_2', targetInterface):SetEnabled(not IsAccountSuspended())
		end
	end
end

function HoN_Notifications.MouseoverToastNotification(isOver)
	Echo('HoN_Notifications.MouseoverToastNotification isOver:'..tostring(isOver))
	if (isOver) then
		HoN_Notifications.toastHoverActive = true
		HoN_Notifications.toastPauseAnimationLast = HostTime() + 250
	else
		HoN_Notifications.toastHoverActive = false
		HoN_Notifications.toastPauseAnimationLast = 0
	end
end

local function UpdateToastNotifications()
	if (#HoN_Notifications.toastTable > 0) or (HoN_Notifications.toastWidgetActive > 0) then

		tsort(HoN_Notifications.toastTable, function(a,b) return tonumber(a.spawnTime) < tonumber(b.spawnTime) end )

		if (HoN_Notifications.toastHoverActive) then
			if (HoN_Notifications.toastPauseAnimationLast == 0) then
				HoN_Notifications.toastPauseAnimationLast = HostTime()
			else
				HoN_Notifications.toastPauseAnimationDuration = HoN_Notifications.toastPauseAnimationDuration + (HostTime() - HoN_Notifications.toastPauseAnimationLast)
				HoN_Notifications.toastPauseAnimationLast = HostTime()
			end
		else
			HoN_Notifications.toastPauseAnimationLast = 0
		end

		if GetWidget('notification_toast_main', 'main', true) and GetWidget('notification_toast_main', 'main', true):IsVisible() then
			for toastIndex = 1, 3, 1 do
				local toastWidgetTable = HoN_Notifications.toastWidgetTable[toastIndex]
				if toastWidgetTable.isAvailable and not toastWidgetTable.isClosing and HoN_Notifications.toastTable[1] then
					if GetNotificationsOptionInfo(nil, 2, HoN_Notifications.toastTable[1].notificationType) then
						toastWidgetTable.isAvailable = false
						toastWidgetTable.isClosing = false
						toastWidgetTable.despawnTime = (HostTime() + (GetCvarInt('ui_notifications_toastDuration') * 1000)) - HoN_Notifications.toastPauseAnimationDuration

						local tempToastTable = {}
						for i, v in pairs(HoN_Notifications.toastTable[1]) do
							tempToastTable[i] = v
						end

						GetWidget('notification_toast_main', 'main', true):RegisterWatch('HostTime', UpdateToastNotifications)
						toastWidgetTable.targetInterface = 'main'
						HoN_Notifications.currentInterface = 'main'
						HoN_Notifications.toastWidgetActive = HoN_Notifications.toastWidgetActive + 1
						InstantiateToastNotification(GetWidget('notification_toast_main', 'main', true), 'main', toastIndex, tempToastTable)
						tremove(HoN_Notifications.toastTable, 1)
						break
					else
						tremove(HoN_Notifications.toastTable, 1)
					end
				elseif (not toastWidgetTable.isClosing) and (not toastWidgetTable.isOpen) and (toastWidgetTable.targetInterface) and ((not toastWidgetTable.despawnTime) or ((toastWidgetTable.despawnTime + HoN_Notifications.toastPauseAnimationDuration) <= HostTime())) then
					toastWidgetTable.isClosing = true
					HideToast(toastIndex, toastWidgetTable.targetInterface, toastWidgetTable)
				end
			end
			UpdateToastPositions(1, 3, 'main')
		elseif GetCvarBool('ui_gameInterfaceLoaded') and GetWidget('notification_toast_game', 'game', true) and GetWidget('notification_toast_game', 'game', true):IsVisible() then
			for toastIndex = 1, 3, 1 do
				local toastWidgetTable = HoN_Notifications.toastWidgetTable[toastIndex]

				if toastWidgetTable.isAvailable and (not toastWidgetTable.isClosing) and (HoN_Notifications.toastTable[1]) then
					if ( GetNotificationsOptionInfo(nil, 1, HoN_Notifications.toastTable[1].notificationType) ) then
						toastWidgetTable.isAvailable = false
						toastWidgetTable.isClosing = false
						toastWidgetTable.despawnTime = (HostTime() + (GetCvarInt('ui_notifications_toastDuration') * 1000)) - HoN_Notifications.toastPauseAnimationDuration

						local tempToastTable = {}
						for i, v in pairs(HoN_Notifications.toastTable[1]) do
							tempToastTable[i] = v
						end

						GetWidget('notification_toast_game', 'game', true):RegisterWatch('HostTime', UpdateToastNotifications)
						toastWidgetTable.targetInterface = 'game'
						HoN_Notifications.currentInterface = 'game'
						HoN_Notifications.toastWidgetActive = HoN_Notifications.toastWidgetActive + 1
						InstantiateToastNotification(GetWidget('notification_toast_game', 'game'), 'game', toastIndex + 3, tempToastTable)
						tremove(HoN_Notifications.toastTable, 1)
						break
					else
						tremove(HoN_Notifications.toastTable, 1)
					end
				elseif (not toastWidgetTable.isClosing) and (not toastWidgetTable.isOpen) and (toastWidgetTable.targetInterface) and ((not toastWidgetTable.despawnTime) or ((toastWidgetTable.despawnTime + HoN_Notifications.toastPauseAnimationDuration) <= HostTime())) then
					toastWidgetTable.isClosing = true
					HideToast(toastIndex + 3, toastWidgetTable.targetInterface, toastWidgetTable)
				end
			end
			UpdateToastPositions(4, 6, 'game')
		elseif GetCvarBool('ui_game_specInterfaceLoaded') and GetWidget('notification_toast_game_spec', 'game_spectator', true) and GetWidget('notification_toast_game_spec', 'game_spectator', true):IsVisible() then
			for toastIndex = 1, 3, 1 do
				local toastWidgetTable = HoN_Notifications.toastWidgetTable[toastIndex]

				if toastWidgetTable.isAvailable and (not toastWidgetTable.isClosing) and (HoN_Notifications.toastTable[1]) then
					if ( GetNotificationsOptionInfo(nil, 1, HoN_Notifications.toastTable[1].notificationType) ) then
						toastWidgetTable.isAvailable = false
						toastWidgetTable.isClosing = false
						toastWidgetTable.despawnTime = (HostTime() + (GetCvarInt('ui_notifications_toastDuration') * 1000)) - HoN_Notifications.toastPauseAnimationDuration

						local tempToastTable = {}
						for i, v in pairs(HoN_Notifications.toastTable[1]) do
							tempToastTable[i] = v
						end

						GetWidget('notification_toast_game_spec', 'game_spectator', true):RegisterWatch('HostTime', UpdateToastNotifications)
						toastWidgetTable.targetInterface = 'game_spectator'
						HoN_Notifications.currentInterface = 'game_spectator'
						HoN_Notifications.toastWidgetActive = HoN_Notifications.toastWidgetActive + 1
						InstantiateToastNotification(GetWidget('notification_toast_game_spec', 'game_spectator'), 'game_spectator', toastIndex + 6, tempToastTable)
						tremove(HoN_Notifications.toastTable, 1)
						break
					else
						tremove(HoN_Notifications.toastTable, 1)
					end
				elseif (not toastWidgetTable.isClosing) and (not toastWidgetTable.isOpen) and (toastWidgetTable.targetInterface) and ((not toastWidgetTable.despawnTime) or ((toastWidgetTable.despawnTime + HoN_Notifications.toastPauseAnimationDuration) <= HostTime())) then
					toastWidgetTable.isClosing = true
					HideToast(toastIndex + 6, toastWidgetTable.targetInterface, toastWidgetTable)
				end
			end
			UpdateToastPositions(7, 9, 'game_spectator')
		else
			e('UpdateToastNotifications found no spawn point')
			tremove(HoN_Notifications.toastTable, 1)
		end
	else
		GetWidget('notification_toast_main', 'main', true):UnregisterWatch('HostTime')
		if GetCvarBool('ui_gameInterfaceLoaded') then
			GetWidget('notification_toast_game', 'game', true):UnregisterWatch('HostTime')
		end
		if GetCvarBool('ui_game_specInterfaceLoaded') then
			GetWidget('notification_toast_game_spec', 'game_spectator', true):UnregisterWatch('HostTime')
		end
		HoN_Notifications.toastPauseAnimationDuration = 0
		HoN_Notifications.toastPauseAnimationLast = 0
	end
end

local function AddToastNotification(newNotification)
	-- prioritise notifications.
	if (newNotification) and (newNotification.notificationType) then

		newNotification.spawnTime = HostTime()

		tinsert(HoN_Notifications.toastTable, newNotification)

		UpdateToastNotifications()
	end
end

function HoN_Notifications:ChatNotification(skipVisiblity, self, sParam1, sParam2, notificationType, notifyTranslateString, genericType, actionTemplate, notificationTime, extNotficationID, intNotificationID, sParam3,
				gameInfo1, gameInfo2, gameInfo3, gameInfo4, gameMode, teamSize, mapName, skillTier, statsLevel, isNoLeaver, isPrivate, isAllHeroes, isCasualMode,
				isForceRandom, isAutoBalanced, isAdvOptions, minPSR, maxPSR, isDevHeroes, isHardcore, silentNotification, isVerifiedOnly, isGated, isBots)
	local isBots = isBots or 'false'

	-- Echo('HoN_Notifications:ChatNotification notificationType:'..notificationType..' intNotificationID:'..intNotificationID)

	if (skipVisiblity) or (UIManager.GetInterface('main'):IsVisible()) then

		intNotificationID = tonumber(intNotificationID)

		HoN_Notifications.currentNotification = Max(HoN_Notifications.currentNotification, intNotificationID)

		local nameColor, accountIcon, nameGlow = GetUpgrades(NotEmpty(gameInfo3) and gameInfo3 or sParam1)

		local newNotification = {
			sParam1 = sParam1 or '',
			sParam2 = sParam2 or '',
			sParam3 = sParam3 or '',
			notificationType = tonumber(notificationType) or 0,
			notifyTranslateString = notifyTranslateString or '',
			genericType = genericType or '',
			actionTemplate = actionTemplate or '',
			notificationTime = notificationTime or '',
			extNotficationID = tonumber(extNotficationID) or 0,
			intNotificationID = intNotificationID or 0,
			gameInfo1 = gameInfo1 or '',
			gameInfo2 = gameInfo2 or '',
			gameInfo3 = gameInfo3 or '',
			gameInfo4 = gameInfo4 or '',
			gameMode = gameMode or '',
			teamSize = teamSize or '',
			mapName = mapName or '',
			skillTier = skillTier or '0',
			statsLevel = statsLevel or '0',
			isNoLeaver = AtoB(isNoLeaver) or false,
			isPrivate = AtoB(isPrivate) or false,
			isAllHeroes = AtoB(isAllHeroes) or false,
			isCasualMode = AtoB(isCasualMode) or false,
			isForceRandom = AtoB(isForceRandom) or false,
			isAutoBalanced = AtoB(isAutoBalanced) or false,
			isAdvOptions = AtoB(isAdvOptions) or false,
			minPSR = minPSR or '0',
			maxPSR = maxPSR or '0',
			isDevHeroes = AtoB(isDevHeroes) or false,
			isHardcore = AtoB(isHardcore) or false,
			silentNotification = AtoB(silentNotification) or false,
			isVerifiedOnly = AtoB(isVerifiedOnly) or false,
			isGated = AtoB(isGated) or false,
			isBots = AtoB(isBots),
			nameColor = nameColor,
			accountIcon = accountIcon,
			nameGlow = nameGlow,
		}

		local showToast = HoN_Notifications.notificationsTable[intNotificationID] and false or true

		if (HoN_Notifications.historySize <= MAXIMUM_NOTIFICATIONS) and GetNotificationsOptionInfo(nil, 3, newNotification.notificationType) then
			newNotification.inHistory = true
			HoN_Notifications.notificationsTable[intNotificationID] = newNotification
		end

		UpdateChatNotificationCounts()

		HoN_Notifications:UpdateUserScroller(HoN_Notifications.lastUserOffset or 0)

		UpdateExistingFriendPopupNotifications()
		UpdateExistingInvitePopupNotifications()
		UpdateChatNotificationPopupLabels()

		if (not (AtoB(silentNotification) or false)) and (showToast) then
			AddToastNotification(newNotification)
		end

		if GetCvarBool('ui_dev_notifications') then
			println('^g intNotificationID = ' .. intNotificationID)
			println('^w showToast = ' .. tostring(showToast))
			println('^w silentNotification = ' .. tostring(silentNotification))
			printTable(newNotification)
		end
	else
		if GetCvarBool('ui_dev') then
			e('skipVisiblity', skipVisiblity)
			println('^r intNotificationID = ' .. intNotificationID)
		end
	end
end

interface:RegisterWatch('ChatNotificationBuddy', 		function(...) 	HoN_Notifications:ChatNotification(0, ...) end)
interface:RegisterWatch('ChatNotificationClan', 		function(...) 	HoN_Notifications:ChatNotification(1, ...) end)
interface:RegisterWatch('ChatNotificationMessage', 		function(...) 	HoN_Notifications:ChatNotification(2, ...) end)
interface:RegisterWatch('ChatNotificationInvite', 		function(...) 	HoN_Notifications:ChatNotification(3, ...) end)
interface:RegisterWatch('ChatNotificationGroupInvite', 	function(...) 	HoN_Notifications:ChatNotification(4, ...) end)

local function ChatNotificationHistoryPerformCMD (self, uiCommand)
	if (uiCommand == 'ClearItems();') then
		groupfcall('sp_notification_history_templates', function(index, widget, groupName) widget:SetVisible(0) end)
		HoN_Notifications.notificationsTable = {}

		UpdateChatNotificationCounts()
		UpdateExistingFriendPopupNotifications()
		UpdateExistingInvitePopupNotifications()
		UpdateChatNotificationPopupLabels()
	end

	local function LoginStatus(self, accountStatus, statusDescription, isLoggedIn, pwordExpired, isLoggedInChanged, updaterStatus)
		local isLoggedIn, pwordExpired, isLoggedInChanged, updaterStatus = AtoB(isLoggedIn), AtoB(pwordExpired), AtoB(isLoggedInChanged), tonumber(updaterStatus)
		if (isLoggedIn) then
			UpdateChatNotificationCounts()
			NotificationsInit()
		elseif (isLoggedInChanged) then
			GetWidget('social_panel'):FadeOut(150)
			GetWidget('notification_toast_main', 'main'):SetX('-0.5h')
			UpdateChatNotificationCounts()
		end
	end
	GetWidget('social_panel'):RegisterWatch('LoginStatus', LoginStatus)
end
interface:RegisterWatch('ChatNotificationHistoryPerformCMD', ChatNotificationHistoryPerformCMD)

function HoN_Notifications.InitOption(option)
	local subOptions = {1, 2, 3}
	for i, v in ipairs(subOptions) do
		local cvarName = tostring(option) .. '_' .. v
		SetSave(cvarName,  GetNotificationsOptionInfo(tostring(option), (v), nil), 'bool', true)
		local cvar = Cvar.GetCvar(cvarName)
		if cvar == nil then
			Echo('^InitOption() nil cvar named '..tostring(cvarName))
		else
			Cvar.SetCloudSave(cvar, true)
		end
	end
end

function HoN_Notifications.InteractNotificationOption(sourceWidget, isClick, option, subOption, currentState)

	local cvarName = tostring(option) .. '_' .. tostring(subOption)
	if (GetCvarBool(tostring(option) .. '_' .. tostring(subOption), true) == nil) then
		SetSave(cvarName,  GetNotificationsOptionInfo(tostring(option), (subOption), nil), 'bool')
		local cvar = Cvar.GetCvar(cvarName)
		if cvar == nil then
			Echo('^rInteractNotificationOption() nil cvar named '..tostring(cvarName))
		else
			Cvar.SetCloudSave(cvar, true)
		end
	end

	if (isClick) then
		SetSave(cvarName,  (not GetCvarBool(cvarName)), 'bool')
	end

	self:SetButtonState(GetCvarBool(cvarName) and 1 or 0)
end

function HoN_Notifications.GamePhase(sourceWidget, gamePhase)

	UpdateChatNotificationCounts()

	-- Only non-replay mode requires clear of game invite notifications after entering game.

	local isNotReplaying = not GetCvarBool('replay_isPlaying')

	if ( isNotReplaying and
	     ( (HoN_Notifications.notificationsCountsTable[20] and HoN_Notifications.notificationsCountsTable[20] > 0) or
	       (HoN_Notifications.notificationsCountsTable[24] and HoN_Notifications.notificationsCountsTable[24] > 0)) ) and
	     (tonumber(gamePhase) >= 1) then
		for i, v in pairs(HoN_Notifications.notificationsTable) do
			if (v) and ((v.notificationType == 20) or (v.notificationType == 24)) then
				HoN_Notifications.notificationsTable[i] = nil
			end
		end
	end

	UpdateExistingFriendPopupNotifications()
	UpdateExistingInvitePopupNotifications()
	UpdateChatNotificationPopupLabels()
end

function interface:HoNNotificationF(func, ...)
  print(HoN_Notifications[func](self, ...))
end

