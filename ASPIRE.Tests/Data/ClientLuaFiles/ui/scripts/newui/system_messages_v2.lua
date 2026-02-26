-- System Messages
local _G = getfenv(0)
local ipairs, pairs, string, table, type, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.string, _G.table, _G.type, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()

systemMessages = systemMessages or {}
-- systemMessages.cache = GetDBEntry('specialMessagesCache', nil) or {}
systemMessages.sys_notification_cache = GetDBEntry('systemNotificationsCache', nil) or {}

systemMessages.messages = {}
systemMessages.startIndex, systemMessages.endIndex = 0, 0

systemMessages.deleteNotificationInfo = nil
systemMessages.deleteNotifications = {}

systemMessages.pendingToasts, systemMessages.showingToasts = {}, {}
systemMessages.wdgVisibleToasts, systemMessages.wdgInvisibleToasts = {}, {}

systemMessages.pendingToastsGame, systemMessages.showingToastsGame = {}, {}
systemMessages.wdgVisibleToastsGame, systemMessages.wdgInvisibleToastsGame = {}, {}

systemMessages.pendingToastsGameSpec, systemMessages.showingToastsGameSpec = {}, {}
systemMessages.wdgVisibleToastsGameSpec, systemMessages.wdgInvisibleToastsGameSpec = {}, {}

local NOTIFICATIONS_TYPE_BET_SUCCESS               = '1'
local NOTIFICATIONS_TYPE_BET_CANCEL                = '2'
local NOTIFICATIONS_TYPE_SUBSCRIBED_MATCH_BEGIN    = '3'
local NOTIFICATIONS_TYPE_BETTED_MATCH_BEGIN        = '4'
local NOTIFICATIONS_TYPE_CUSTOMIZED                = '5' -- this type of message fits default message
local NOTIFICATIONS_TYPE_POINTS_CHANGED            = '6'
local NOTIFICATIONS_TYPE_RAP                       = '7'

local MAX_MESSAGE_SLOTS = 11
local MAX_MESSAGE_EMPTY_SLOTS = 3

local MAX_MESSAGE_TOAST_SLOTS = 6

local _sysmessage_id = 1
local _selected_message_id = 0

local PANEL_CLOSE_ANIMATION_TIME = 200
local INVITE_SLIDE_TIME = 150

local CLAN_INVITE = 99

local MessageType_Unknown = -1
local MessageType_Special = 0
local MessageType_System_Notification = 1
local MessageType_Chat_Notification = 2

local function GenerateMessageID()
    local id = _sysmessage_id
    _sysmessage_id = _sysmessage_id + 1
    return id
end

local function GetDate(time)
    return Empty(time) and '' or sub(time, 1, 10)
end

local function GetMessageObj(id)
    local index = systemMessages.startIndex + id -1
    local message = systemMessages.messages[index]
    if message == nil then
        Echo('GetMessageObj cannot get message object with index: '..index)
    end
    return message
end

local function GetMessageObjByID(id)
    for k, v in ipairs(systemMessages.messages) do
        if v.id == id then
            return v
        end
    end
    return nil
end

local function isInvitation(notification)
    local nType = notification and notification.notificationType or 0
    -- Echo('isInvitation notificationType:'..notificationType..' type:'..type(notification.notificationType))

    --Game invite / clan join / buddy join / group invite / buddy request / patch / self join / stream / replay upload
    return nType == 11 or nType == 15 or nType == 20 or nType == 23 or nType == 24 or nType == 17 or nType == 21 or nType == 25 or nType == 26 or nType == CLAN_INVITE
end

local function GetTargetInterface()
    if GetWidget('notification_toast_main', 'main', true) and GetWidget('notification_toast_main', 'main'):IsVisible() then
        return 'main'
    elseif GetCvarBool('ui_game_specInterfaceLoaded') and GetWidget('notification_toast_game_spec', 'game_spectator', true) and GetWidget('notification_toast_game_spec', 'game_spectator'):IsVisible() then
        return 'game_spectator'
    elseif GetCvarBool('ui_gameInterfaceLoaded') and GetWidget('notification_toast_game', 'game', true) and GetWidget('notification_toast_game', 'game'):IsVisible() then
        return 'game'
    end
    return 'main'
end

local function GetPendingToasts(fromInterface)
    return fromInterface == 'game_spectator' and systemMessages.pendingToastsGameSpec or (fromInterface == 'game' and systemMessages.pendingToastsGame or systemMessages.pendingToasts)
end

local function GetShowingToasts(fromInterface)
    return fromInterface == 'game_spectator' and systemMessages.showingToastsGameSpec or (fromInterface == 'game' and systemMessages.showingToastsGame or systemMessages.showingToasts)
end

local function GetVisibleToastWidgets(fromInterface)
    return fromInterface == 'game_spectator' and systemMessages.wdgVisibleToastsGameSpec or (fromInterface == 'game' and systemMessages.wdgVisibleToastsGame or systemMessages.wdgVisibleToasts)
end

local function GetInvisibleToastWidgets(fromInterface)
    return fromInterface == 'game_spectator' and systemMessages.wdgInvisibleToastsGameSpec or (fromInterface == 'game' and systemMessages.wdgInvisibleToastsGame or systemMessages.wdgInvisibleToasts)
end

--[[chat notifications]]
--[[notification type from c++
enum ENotifyType
{
NOTIFY_TYPE_UNKNOWN,
NOTIFY_TYPE_BUDDY_ADDER,
NOTIFY_TYPE_BUDDY_ADDED,
NOTIFY_TYPE_BUDDY_REMOVER,// DISABLED
NOTIFY_TYPE_BUDDY_REMOVED,// DISABLED
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
NOTIFY_TYPE_CLAN_WHISPER,// UNUSED
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

local _soundsByType = {
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
    [22] = '', -- NotifyBuddyRequester
    [23] = '', -- NotifyBuddyRequested
    [24] = 'NotifyGameInvite',
    [25] = '',
    [26] = '',
}

local _defaultNotificationOptions = {
    ['ui_not_opt_1'] = {true, true, true}, -- friend add -- and 2 3 4
    ['ui_not_opt_2'] = {true, true, true}, -- friend added
    ['ui_not_opt_3'] = {true, true, true}, -- friend remove
    ['ui_not_opt_4'] = {true, true, true}, -- friend removed
    ['ui_not_opt_5'] = {false, true, true}, -- clan rank change
    ['ui_not_opt_6'] = {false, true, true}, -- clan add
    ['ui_not_opt_7'] = {false, true, true}, -- clan removed
    ['ui_not_opt_8'] = {false, true, false}, -- friend log in
    ['ui_not_opt_9'] = {false, true, false}, -- friend left game
    ['ui_not_opt_10'] = {false, false, false}, -- friend log off
    ['ui_not_opt_11'] = {false, true, true}, -- friend join game
    ['ui_not_opt_12'] = {false, true, false}, -- clan sign on
    ['ui_not_opt_13'] = {false, false, false}, -- clan left game
    ['ui_not_opt_14'] = {false, false, false}, -- clan sign off
    ['ui_not_opt_15'] = {false, true, true}, -- clan join game
    ['ui_not_opt_16'] = {false, false, false}, -- clan whisper
    ['ui_not_opt_17'] = {false, true, true}, -- patch
    ['ui_not_opt_18'] = {false, true, true}, -- info
    ['ui_not_opt_19'] = {true, true, false}, -- im recieved
    ['ui_not_opt_20'] = {false, true, true}, -- game invite
    ['ui_not_opt_21'] = {false, false, true}, -- self join
    ['ui_not_opt_22'] = {true, true, true}, -- you send friend request -- and 23
    ['ui_not_opt_23'] = {true, true, true}, -- you get friend request
    ['ui_not_opt_24'] = {false, true, true}, -- group invite
    ['ui_not_opt_25'] = {false, true, true}, -- stream online
    ['ui_not_opt_26'] = {false, true, true}, -- replay ready
    ['ui_not_opt_27'] = {false, true, true}, -- cloudsave_upload_success-- and 28 29 30
    ['ui_not_opt_28'] = {false, true, true}, -- cloudsave_upload_fail
    ['ui_not_opt_29'] = {false, true, true}, -- cloudsave_download_success
    ['ui_not_opt_30'] = {false, true, true}, -- cloudsave_download_fail
}

local function getUpgrades(userName)
    userName = StripClanTag(string.lower(userName))

    local nameColor, accountIcon, nameGlow, nameGlowColor
    local clientInfoTable = explode('|', GetChatClientInfo(userName, 'chatnamecolorstring|chatnamecolortexturepath|getaccounticontexturepath|chatnameglow|chatnameglowcolorstring'))

    if NotEmpty(clientInfoTable[3]) then
        accountIcon = clientInfoTable[3]
    elseif NotEmpty(clientInfoTable[2]) then
        accountIcon = clientInfoTable[2]
    else
        accountIcon = '/ui/fe2/store/icons/account_icons/default.tga'
    end
    if NotEmpty(clientInfoTable[1]) then
        nameColor = clientInfoTable[1]
    end

    nameGlow = NotEmpty(clientInfoTable[4]) and clientInfoTable[4] or 'false'
    nameGlowColor = NotEmpty(clientInfoTable[5]) and clientInfoTable[5] or ''

    return nameColor, accountIcon, nameGlow, nameGlowColor
end

local function playNotificationSound(notificationType)
    if GetCvarBool('ui_not_opt_sound') and NotEmpty(_soundsByType[notificationType]) then
        PlayChatSound(_soundsByType[notificationType])
    end
end

local function GetNotificationsOptionInfo(option, subOption, notificationType)
    if notificationType then
        if (notificationType == 2) or (notificationType == 3) or (notificationType == 4) then
            notificationType = 1
        elseif (notificationType == 22) then
            notificationType = 23
        elseif notificationType > 27 and notificationType <= 30 then    -- cloud save related
            notificationType = 27
        end

        if GetCvarBool('ui_not_opt_' .. tostring(notificationType) .. '_' .. tostring(subOption), true) ~= nil then
            return GetCvarBool('ui_not_opt_' .. tostring(notificationType) .. '_' .. tostring(subOption))
        elseif _defaultNotificationOptions['ui_not_opt_' .. notificationType] then
            return _defaultNotificationOptions['ui_not_opt_' .. notificationType][subOption] and true or false
        else
            return false
        end
    else
        return _defaultNotificationOptions[option] and _defaultNotificationOptions[option][subOption] and true or false
    end
end

local function createButtonOnclick(buttonIndex, notification, sTemp)
    local notificationType = notification.notificationType

    local function closePopup(removeNotification)
        removeNotification = removeNotification or true

        if removeNotification then
            ChatRemoveNotification(notification.intNotificationID)
        end

        if NotEmpty(sTemp) then
            local wdgMessage = GetWidget(sTemp, fromInterface)
            if wdgMessage ~= nil then
                systemMessages:checkRemainingChatMessages(wdgMessage, notification, notification.notificationType ~= 17)
            end
        end
    end

    if (buttonIndex == 1) then
        if (notificationType == 20) or (notificationType == 21) or (notificationType == 15) or (notificationType == 11) then
            return function() interface:UICmd([[ClearInviteAddress()]]) closePopup(true) end
        elseif (notificationType == 24) then
            return function() interface:UICmd([[RejectTMMInvite(']] .. notification.sParam1 .. [[')]]) closePopup(true) end
        elseif (notificationType == CLAN_INVITE) then
            return function() interface:UICmd([[ChatRejectClanInvite()]]) closePopup(false) end
        end
    elseif (buttonIndex == 2) then
        if (notificationType == 20) or (notificationType == 21) or (notificationType == 15) or (notificationType == 11) then
            return function() Loading_V2:SetHostedLoadingInfo('unknown') interface:UICmd([[Connect(']] .. notification.gameInfo1 .. [[', ']] .. notification.gameInfo2 .. [[')]]) closePopup(true) end
        elseif (notificationType == 23) then
            return function() interface:UICmd([[ChatApproveBuddy(']] .. notification.sParam1 .. [[')']]) closePopup(true) end
        elseif (notificationType == 24) then
            return function()
                if not IsInGroup() then
                    interface:UICmd("JoinTMMGroup('"..notification.sParam1.."');")
                else
                    Teammaking_V2:LeaveGroup()
                    GetWidget('event_com', 'main'):Sleep(1000, function() interface:UICmd("JoinTMMGroup('"..notification.sParam1.."');")  end)
                end
                closePopup(true)
            end
        elseif (notificationType == 26) then
            return function()
                Set('_stats_last_replay_id', notification.sParam1)
                Set('_stats_last_match_id', notification.sParam1)

                UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'match_stats', nil, true)
                interface:UICmd("ClearEndGameStats(); GetMatchInfo('"..notification.sParam1.."');")

                closePopup(true)
            end
        elseif (notificationType == 17) then
            return function() --[[Cmd('RestartAndUpdate')]] Trigger('ShowUpdatePanel') closePopup(true) end
        elseif (notificationType == CLAN_INVITE) then
            return function() interface:UICmd([[ChatAcceptClanInvite()]]) closePopup(false) end
        end
    end

    return function() closePopup(true) end
end

local function receiveChatNotification(_, sParam1, sParam2, notificationType, notifyTranslateString, genericType, actionTemplate, notificationTime, extNotficationID, intNotificationID, sParam3, gameInfo1, gameInfo2, gameInfo3, gameInfo4, gameMode, teamSize, mapName, skillTier, statsLevel, isNoLeaver, isPrivate, isAllHeroes, isCasualMode, isForceRandom, isAutoBalanced, isAdvOptions, minPSR, maxPSR, isDevHeroes, isHardcore, silentNotification, isVerifiedOnly, isGated, isBots)
    -- Echo('receiveChatNotification notificationType:'..notificationType..' silentNotification:'..tostring(silentNotification))

    notificationType = tonumber(notificationType)
    intNotificationID = tonumber(intNotificationID)

    --friend: online, offline, left game, join game, clan join game, self join, clan rank, buddy request, clan online, clan offline, clan join, clan left
    if (notificationType >= 6 and notificationType <= 11) or notificationType == 1 or notificationType == 2 or notificationType == 15 or notificationType == 21 or notificationType == 5 or notificationType == 22 or notificationType == 12 or notificationType == 14 then
        return
    end

    playNotificationSound(notificationType)

    local nameColor, accountIcon, nameGlow = getUpgrades(NotEmpty(gameInfo3) and gameInfo3 or sParam1)
    local newNotification = {
        id = GenerateMessageID(),
        type = MessageType_Chat_Notification,
        sParam1 = sParam1 or '',
        sParam2 = sParam2 or '',
        sParam3 = sParam3 or '',
        notificationType = notificationType or 0,
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
        isBots = isBots and AtoB(isBots) or false,
        nameColor = nameColor or '#f0d3c1',
        icon = accountIcon,
        nameGlow = nameGlow or false,
        forcePopup = false,
    }

    if notificationType == 17 or (notificationType >= 27 and notificationType <= 30) then
        newNotification.icon = '/ui/fe2/NewUI/Res/system_message/msg_type1.png'
    end

    systemMessages:openMessageToast(newNotification, GetTargetInterface())

    if GetNotificationsOptionInfo(nil, 3, newNotification.notificationType) then
        systemMessages:addMessage(newNotification, true, true)
    end
end

local function chatClanInvite(_, sPlayerName, sClanName)
    local nameColor, accountIcon, nameGlow = getUpgrades(sPlayerName)
    local newNotification = {
        id = GenerateMessageID(),
        type = MessageType_Chat_Notification,
        sParam1 = sPlayerName,
        sParam2 = sClanName,
        sParam3 = sParam3 or '',
        notificationType = CLAN_INVITE,
        notifyTranslateString = '',
        genericType = '',
        actionTemplate = '',
        notificationTime = '',
        extNotficationID = 0,
        intNotificationID = 0,
        gameInfo1 = '',
        gameInfo2 = '',
        gameInfo3 = '',
        gameInfo4 = '',
        gameMode = '',
        teamSize = '',
        mapName = '',
        skillTier = '0',
        statsLevel = '0',
        isNoLeaver = false,
        isPrivate = false,
        isAllHeroes = false,
        isCasualMode = false,
        isForceRandom = false,
        isAutoBalanced = false,
        isAdvOptions = false,
        minPSR = '0',
        maxPSR = '0',
        isDevHeroes = false,
        isHardcore = false,
        silentNotification = false,
        isVerifiedOnly = false,
        isGated = false,
        isBots = false,
        nameColor = nameColor or '#f0d3c1',
        icon = accountIcon,
        nameGlow = nameGlow or false,
        forcePopup = true,
    }

    systemMessages:openMessageToast(newNotification, GetTargetInterface())
    systemMessages:addMessage(newNotification, true, true)
end

local function chatNotificationHistoryPerformCMD(_, uiCommand)
    if (uiCommand == 'ClearItems();') then
        systemMessages:clearMessages()
    end
end

function systemMessages:init(widget)
    --[[
    widget:RegisterWatch('SpecialMessagesUpdated', function(_, forcePopup)
        local specialMessagesCache = systemMessages:checkCreateAccountSMCache()
        local newMessageID, numNewMessage = 0, 0
        local newMessages = GetSpecialMessages()

        for k, v in ipairs(newMessages) do
            local bFound = false
            for _, message in pairs(systemMessages.messages) do
                if message.type == MessageType_Special and message.md5 == v.md5 then
                    bFound = true
                    break
                end
            end
            if not bFound then
                local message = {}
                message.type = MessageType_Special
                message.new = true
                message.id = GenerateMessageID()
                message.subject = v.title
                message.message_id = v.message_id
                message.url = v.url
                message.start_time = v.start_time
                message.end_time = v.end_time
                message.date = v.date
                message.md5 = v.md5
                message.left_secs = v.left_secs

                newMessageID = message.id

                numNewMessage = numNewMessage + 1

                systemMessages:addMessage(message, false, true)
            end

            if not specialMessagesCache[v.md5] then
                specialMessagesCache[v.md5] = v
            else
                newMessageID = 0
            end
        end

        if numNewMessage > 0 then
            systemMessages:generateMessageList()
        end

        if forcePopup and newMessageID ~= 0 then
            for k, v in ipairs(systemMessages.messages) do
                if v.id == newMessageID then
                    systemMessages:openMessage(k - systemMessages.startIndex + 1)
                    break
                end
            end
        end

        GetDBEntry('specialMessagesCache', systemMessages.cache, true, false, false)
    end)
    ]]

    widget:RegisterWatch('MessageDeleted', function(_, crc, deleteSuccess)
        if AtoB(deleteSuccess) then
            systemMessages:clearDeletedMessage(systemMessages.deleteNotificationInfo)
        else
            Echo('^rMessageDeleted Failed to delete message!')
        end

        systemMessages.deleteNotificationInfo = nil

        if #systemMessages.deleteNotifications > 0 then
            systemMessages.deleteNotificationInfo = table.remove(systemMessages.deleteNotifications)
            DeleteMessage(systemMessages.deleteNotificationInfo.crc)
        end
    end)

    widget:RegisterWatch('MessagesUpdated', function(_, listUpdated)
        if AtoB(listUpdated) then
            local newNotifications, newNotificationsByCRC = GetMessages(), {}
            local notificationCache = systemMessages:checkCreateAccountCache()

            for _, v in ipairs(newNotifications) do-- mostly to build a list by CRC
                newNotificationsByCRC[v.crc] = v

                if not notificationCache[v.crc] then
                    notificationCache[v.crc] = v
                end
            end

            local numNewNotification = 0
            for k, v in pairs(notificationCache) do
                if newNotificationsByCRC[v.crc] then
                    if not v.displayed then
                        systemMessages:newNotification(v)
                        numNewNotification = numNewNotification + 1
                    end
                else
                    notificationCache[k] = nil
                end
            end
            if numNewNotification > 0 then
                systemMessages:generateMessageList()
            end
            GetDBEntry('systemNotificationsCache', systemMessages.sys_notification_cache, true, false, false)
        end
    end)

    widget:RegisterWatch('LoginStatus', function(widget, accountStatus, statusDescription, isLoggedIn, pwordExpired, isLoggedInChanged, updaterStatus)
        if AtoB(isLoggedIn) then
            --Fetch special messages
            -- local isLoggedInChanged, updaterStatus = AtoB(isLoggedInChanged), tonumber(updaterStatus)
            -- if isLoggedInChanged  and (updaterStatus == 5 or updaterStatus == 6 or updaterStatus == 8) then
            --     UpdateSpecialMessage(true)
            -- end

            --System notifications cache
            local notificationCache, numNewNotification = systemMessages:checkCreateAccountCache(), 0
            for _, v in pairs(notificationCache) do
                systemMessages:newNotification(v)
                numNewNotification = numNewNotification + 1
            end
            if numNewNotification > 0 then
                systemMessages:generateMessageList()
            end
            GetDBEntry('systemNotificationsCache', systemMessages.sys_notification_cache, true, false, false)
        elseif AtoB(isLoggedInChanged) then
            systemMessages:initChatMessages()
            systemMessages:initGameChatMessages()
            systemMessages:initGameSpecChatMessages()

            systemMessages:reset()

            systemMessages.deleteNotifications = {}

            widget:SetVisible(false)
        end
    end)

    widget:RegisterWatch('GamePhase', function(widget, newPhase)
        if tonumber(newPhase) == 0 then
            Set('ui_gameInterfaceLoaded', false)
            Set('ui_game_specInterfaceLoaded', false)
        end
    end)

    widget:RegisterWatch('ChatNotificationBuddy', receiveChatNotification)
    widget:RegisterWatch('ChatNotificationClan', receiveChatNotification)
    widget:RegisterWatch('ChatNotificationMessage', receiveChatNotification)
    widget:RegisterWatch('ChatNotificationInvite', receiveChatNotification)
    widget:RegisterWatch('ChatNotificationGroupInvite', receiveChatNotification)
    widget:RegisterWatch('ChatClanInvite', chatClanInvite)
    widget:RegisterWatch('ChatNotificationHistoryPerformCMD', chatNotificationHistoryPerformCMD)
end

function systemMessages:showNewMessageMark(show)
    interface:GetWidget('sysbar_system_messages_new'):SetVisible(show)
end

function systemMessages:initChatMessages()
    systemMessages.pendingToasts, systemMessages.showingToasts = {}, {}
    systemMessages.wdgVisibleToasts, systemMessages.wdgInvisibleToasts = {}, {}

    for i=1, MAX_MESSAGE_TOAST_SLOTS do
        local wdgMessage = interface:GetWidget('sys_chat_message'..i..'main')
        wdgMessage:SetVisible(false)
        table.insert(systemMessages.wdgInvisibleToasts, wdgMessage)
    end
end

function systemMessages:initGameChatMessages()
    systemMessages.pendingToastsGame, systemMessages.showingToastsGame = {}, {}
    systemMessages.wdgVisibleToastsGame, systemMessages.wdgInvisibleToastsGame = {}, {}

    if GetCvarBool('ui_gameInterfaceLoaded') then
        for i=1, MAX_MESSAGE_TOAST_SLOTS do
            local wdgMessage = GetWidget('sys_chat_message'..i..'game', 'game', true)
            if wdgMessage then
                wdgMessage:SetVisible(false)
                table.insert(systemMessages.wdgInvisibleToastsGame, wdgMessage)
            end
        end
    end
end

function systemMessages:initGameSpecChatMessages()
    systemMessages.pendingToastsGameSpec, systemMessages.showingToastsGameSpec = {}, {}
    systemMessages.wdgVisibleToastsGameSpec, systemMessages.wdgInvisibleToastsGameSpec = {}, {}

    if GetCvarBool('ui_game_specInterfaceLoaded') then
        for i=1, MAX_MESSAGE_TOAST_SLOTS do
            local wdgMessage = GetWidget('sys_chat_message'..i..'game_spectator', 'game_spectator', true)
            if wdgMessage then
                wdgMessage:SetVisible(false)
                table.insert(systemMessages.wdgInvisibleToastsGameSpec, wdgMessage)
            end
        end
    end
end

function systemMessages:InitOption(option)
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

function systemMessages:InteractNotificationOption(sourceWidget, isClick, option, subOption, currentState)
    local cvarName = tostring(option) .. '_' .. tostring(subOption)
    if GetCvarBool(tostring(option) .. '_' .. tostring(subOption), true) == nil then
        SetSave(cvarName,  GetNotificationsOptionInfo(tostring(option), (subOption), nil), 'bool')
        local cvar = Cvar.GetCvar(cvarName)
        if cvar == nil then
            Echo('^rInteractNotificationOption() nil cvar named '..tostring(cvarName))
        else
            Cvar.SetCloudSave(cvar, true)
        end
    end

    if isClick then
        SetSave(cvarName,  (not GetCvarBool(cvarName)), 'bool')
    end

    sourceWidget:SetButtonState(GetCvarBool(cvarName) and 1 or 0)
end

function systemMessages:setChatMessageDetail(sTemp, notification, fromInterface)
    if notification == nil then
        Echo('^rsystemMessages:setChatMessageDetail invalid notification!!!')
        return
    end

    local notificationType = notification.notificationType
    Echo('systemMessages:setChatMessageDetail sTemp:'..sTemp..' fromInterface: '..fromInterface..' notificationType: '..notificationType)

    GetWidget(sTemp..'_value', fromInterface):SetText(notification.id)

    GetWidget(sTemp..'_gameinfo', fromInterface):SetVisible(false)
    GetWidget(sTemp .. '_line', fromInterface):SetVisible(false)
    GetWidget(sTemp .. '_buttons', fromInterface):SetVisible(false)
    GetWidget(sTemp .. '_button_1', fromInterface):SetVisible(false)
    GetWidget(sTemp .. '_button_2', fromInterface):SetVisible(false)

    if notificationType == 20 or notificationType == 15 or notificationType == 11 then --Game invite / clan join / buddy join
        GetWidget(sTemp..'_gameinfo', fromInterface):SetVisible(true)

        GetWidget(sTemp .. '_button_1', fromInterface):SetVisible(true)
        GetWidget(sTemp .. '_button_1', fromInterface):SetLabel(Translate('general_ignore'))

        GetWidget(sTemp .. '_button_2', fromInterface):SetVisible(true)
        GetWidget(sTemp .. '_button_2', fromInterface):SetLabel(Translate('notify_join_game'))

        GetWidget(sTemp .. '_label_4', fromInterface):SetText(notification.gameInfo2 or '?')
        GetWidget(sTemp .. '_label_5', fromInterface):SetText(notification.gameInfo4 .. ' - ' .. notification.teamSize)

        local function MouseOut() Common_V2:ShowSimpleTip(false) end
        local wdgIcon = nil

        wdgIcon = GetWidget(sTemp .. '_icon_1', fromInterface)
        wdgIcon:SetTexture('/maps/' .. notification.mapName .. '/icon.tga')
        wdgIcon:SetCallback('onmouseover', function() Common_V2:ShowSimpleTip(true, 'mainlobby_gamelist_'..notification.mapName..'_title', '18h') end)
        wdgIcon:SetCallback('onmouseout', function() MouseOut() end)
        wdgIcon:RefreshCallbacks()

        wdgIcon = GetWidget(sTemp .. '_icon_2', fromInterface)
        wdgIcon:SetTexture('/ui/icons/' .. notification.gameMode .. '.tga')
        wdgIcon:SetCallback('onmouseover', function() Common_V2:ShowSimpleTip(true, 'mainlobby_gamelist_'..notification.gameMode..'_title', '18h') end)
        wdgIcon:SetCallback('onmouseout', function() MouseOut() end)
        wdgIcon:RefreshCallbacks()

        wdgIcon = GetWidget(sTemp .. '_icon_3', fromInterface)
        wdgIcon:SetVisible(notification.isVerifiedOnly)
        wdgIcon:SetCallback('onmouseover', function() Common_V2:ShowSimpleTip(true, 'mainlobby_gamelist_verifiedonly_title', '18h') end)
        wdgIcon:SetCallback('onmouseout', function() MouseOut() end)
        wdgIcon:RefreshCallbacks()

        wdgIcon = GetWidget(sTemp .. '_icon_4', fromInterface)
        wdgIcon:SetVisible(notification.isCasualMode)
        wdgIcon:SetCallback('onmouseover', function() Common_V2:ShowSimpleTip(true, 'mainlobby_gamelist_casual_title', '18h') end)
        wdgIcon:SetCallback('onmouseout', function() MouseOut() end)
        wdgIcon:RefreshCallbacks()

        wdgIcon = GetWidget(sTemp .. '_icon_5', fromInterface)
        wdgIcon:SetVisible(notification.isHardcore)
        wdgIcon:SetCallback('onmouseover', function() Common_V2:ShowSimpleTip(true, 'mainlobby_gamelist_hardcore_title', '18h') end)
        wdgIcon:SetCallback('onmouseout', function() MouseOut() end)
        wdgIcon:RefreshCallbacks()

        wdgIcon = GetWidget(sTemp .. '_icon_6', fromInterface)
        wdgIcon:SetVisible(notification.isGated)
        wdgIcon:SetCallback('onmouseover', function() Common_V2:ShowSimpleTip(true, 'mainlobby_gamelist_gated_title', '18h') end)
        wdgIcon:SetCallback('onmouseout', function() MouseOut() end)
        wdgIcon:RefreshCallbacks()

        wdgIcon = GetWidget(sTemp .. '_icon_7', fromInterface)
        wdgIcon:SetVisible(notification.isAdvOptions)
        wdgIcon:SetCallback('onmouseover', function() Common_V2:ShowSimpleTip(true, 'mainlobby_gamelist_more_title', '18h') end)
        wdgIcon:SetCallback('onmouseout', function() MouseOut() end)
        wdgIcon:RefreshCallbacks()

        wdgIcon = GetWidget(sTemp .. '_icon_8', fromInterface)
        wdgIcon:SetVisible(notification.isAutoBalanced)
        wdgIcon:SetCallback('onmouseover', function() Common_V2:ShowSimpleTip(true, 'mainlobby_gamelist_autobalance_title', '18h') end)
        wdgIcon:SetCallback('onmouseout', function() MouseOut() end)
        wdgIcon:RefreshCallbacks()

        wdgIcon = GetWidget(sTemp .. '_icon_9', fromInterface)
        wdgIcon:SetVisible(notification.statsLevel == '1')
        wdgIcon:SetCallback('onmouseover', function() Common_V2:ShowSimpleTip(true, 'mainlobby_gamelist_official_title', '18h') end)
        wdgIcon:SetCallback('onmouseout', function() MouseOut() end)
        wdgIcon:RefreshCallbacks()

        wdgIcon = GetWidget(sTemp .. '_icon_10', fromInterface)
        wdgIcon:SetVisible(notification.statsLevel == '2')
        wdgIcon:SetCallback('onmouseover', function() Common_V2:ShowSimpleTip(true, 'mainlobby_gamelist_official_2_title', '18h') end)
        wdgIcon:SetCallback('onmouseout', function() MouseOut() end)
        wdgIcon:RefreshCallbacks()

        wdgIcon = GetWidget(sTemp .. '_icon_11', fromInterface)
        wdgIcon:SetVisible(notification.isNoLeaver)
        wdgIcon:SetCallback('onmouseover', function() Common_V2:ShowSimpleTip(true, 'mainlobby_gamelist_noleaver_title', '18h') end)
        wdgIcon:SetCallback('onmouseout', function() MouseOut() end)
        wdgIcon:RefreshCallbacks()

        wdgIcon = GetWidget(sTemp .. '_icon_12', fromInterface)
        wdgIcon:SetVisible(notification.isPrivate)
        wdgIcon:SetCallback('onmouseover', function() Common_V2:ShowSimpleTip(true, 'mainlobby_gamelist_private_title', '18h') end)
        wdgIcon:SetCallback('onmouseout', function() MouseOut() end)
        wdgIcon:RefreshCallbacks()

        wdgIcon = GetWidget(sTemp .. '_icon_13', fromInterface)
        wdgIcon:SetVisible(notification.isDevHeroes)
        wdgIcon:SetCallback('onmouseover', function() Common_V2:ShowSimpleTip(true, 'mainlobby_gamelist_devheroes_title', '18h') end)
        wdgIcon:SetCallback('onmouseout', function() MouseOut() end)
        wdgIcon:RefreshCallbacks()

        wdgIcon = GetWidget(sTemp .. '_icon_14', fromInterface)
        wdgIcon:SetVisible(notification.isBots)
        wdgIcon:SetCallback('onmouseover', function() Common_V2:ShowSimpleTip(true, 'mainlobby_gamelist_botmatch_title', '18h') end)
        wdgIcon:SetCallback('onmouseout', function() MouseOut() end)
        wdgIcon:RefreshCallbacks()
    elseif notificationType == 24 then --group invite
        GetWidget(sTemp .. '_button_1', fromInterface):SetVisible(true)
        GetWidget(sTemp .. '_button_1', fromInterface):SetLabel(Translate('general_ignore'))

        GetWidget(sTemp .. '_button_2', fromInterface):SetVisible(true)
        GetWidget(sTemp .. '_button_2', fromInterface):SetLabel(Translate('notify_join_group'))
    elseif notificationType == 23 then --buddy request
        GetWidget(sTemp .. '_button_1', fromInterface):SetVisible(true)
        GetWidget(sTemp .. '_button_1', fromInterface):SetLabel(Translate('general_ignore'))

        GetWidget(sTemp .. '_button_2', fromInterface):SetVisible(true)
        GetWidget(sTemp .. '_button_2', fromInterface):SetLabel(Translate('general_approve'))
    elseif notificationType == 25 then --stream
        GetWidget(sTemp .. '_button_1', fromInterface):SetVisible(false)

        GetWidget(sTemp .. '_button_2', fromInterface):SetVisible(true)
        GetWidget(sTemp .. '_button_2', fromInterface):SetLabel(Translate('notify_watch'))
    elseif notificationType == 17 then --patch
        GetWidget(sTemp .. '_button_1', fromInterface):SetVisible(false)

        GetWidget(sTemp .. '_button_2', fromInterface):SetVisible(true)
        GetWidget(sTemp .. '_button_2', fromInterface):SetLabel(Translate('notify_update_now'))
    elseif notificationType == 21 then --self join
        GetWidget(sTemp .. '_button_1', fromInterface):SetVisible(false)

        GetWidget(sTemp .. '_button_2', fromInterface):SetVisible(true)
        GetWidget(sTemp .. '_button_2', fromInterface):SetLabel(Translate('notify_join_game'))

        GetWidget(sTemp .. '_label_4', fromInterface):SetText(notification.gameInfo2 or '?')
        GetWidget(sTemp .. '_label_5', fromInterface):SetText(notification.gameInfo4 .. ' - ' .. notification.teamSize)

        GetWidget(sTemp .. '_icon_1', fromInterface):SetTexture('/maps/' .. notification.mapName .. '/icon.tga')
        GetWidget(sTemp .. '_icon_2', fromInterface):SetTexture('/ui/icons/' .. notification.gameMode .. '.tga')
        GetWidget(sTemp .. '_icon_3', fromInterface):SetVisible(notification.isVerifiedOnly)
        GetWidget(sTemp .. '_icon_4', fromInterface):SetVisible(notification.isCasualMode)
        GetWidget(sTemp .. '_icon_5', fromInterface):SetVisible(notification.isHardcore)
        GetWidget(sTemp .. '_icon_6', fromInterface):SetVisible(notification.isGated)
        GetWidget(sTemp .. '_icon_7', fromInterface):SetVisible(notification.isAdvOptions)
        GetWidget(sTemp .. '_icon_8', fromInterface):SetVisible(notification.isAutoBalanced)
        GetWidget(sTemp .. '_icon_9', fromInterface):SetVisible(notification.statsLevel == '1')
        GetWidget(sTemp .. '_icon_10', fromInterface):SetVisible(notification.statsLevel == '2')
        GetWidget(sTemp .. '_icon_11', fromInterface):SetVisible(notification.isNoLeaver)
        GetWidget(sTemp .. '_icon_12', fromInterface):SetVisible(notification.isPrivate)
        GetWidget(sTemp .. '_icon_13', fromInterface):SetVisible(notification.isDevHeroes)
        GetWidget(sTemp .. '_icon_14', fromInterface):SetVisible(notification.isBots)
    elseif notificationType == 26 then --replay upload
        GetWidget(sTemp .. '_button_1', fromInterface):SetVisible(false)
        GetWidget(sTemp .. '_button_1', fromInterface):SetLabel(Translate('general_ignore'))

        GetWidget(sTemp .. '_button_2', fromInterface):SetVisible(true)
        GetWidget(sTemp .. '_button_2', fromInterface):SetLabel(Translate('match_stats_open'))
    elseif notificationType == CLAN_INVITE then --clan invite
        GetWidget(sTemp .. '_button_1', fromInterface):SetVisible(true)
        GetWidget(sTemp .. '_button_1', fromInterface):SetLabel(Translate('general_ignore'))

        GetWidget(sTemp .. '_button_2', fromInterface):SetVisible(true)
        GetWidget(sTemp .. '_button_2', fromInterface):SetLabel(Translate('general_accept'))
    end

    local label_2 = Translate(notification.notifyTranslateString .. '_dependent', 'version', notification.sParam1, 'streamname', notification.sParam1, 'name', notification.sParam2, 'replay', notification.sParam1)
    label_2 = NotEmpty(label_2) and (label_2 .. ' ') or ''
    label_2 = label_2 .. Translate(notification.notifyTranslateString .. '_info', 'gamename', notification.sParam2, 'rank', notification.sParam3, 'name', notification.sParam2)

    if NotEmpty(notification.sParam2) and notificationType == 7 then
        label_2 = label_2 .. '\n' .. Translate(notification.notifyTranslateString .. '_info_removed', 'name', notification.sParam2)
    end

    if notificationType == CLAN_INVITE then
        label_2 = Translate('notify_clan_invite_info', 'clan', notification.sParam2)
    end

    local notificationTitle = notification.sParam1
    if notificationType ~= 24 and NotEmpty(notification.gameInfo3) then
        notificationTitle = notification.gameInfo3
    end
    if notificationType == 26 then
        notificationTitle = Translate("notify_match_id")..notification.sParam1
    end
    if notificationType >= 27 and notificationType <= 30 then
        notificationTitle = Translate(notification.sParam1..'_situation')
    end

    GetWidget(sTemp .. '_label_1', fromInterface):SetText(notificationTitle or '?')
    GetWidget(sTemp .. '_label_1', fromInterface):SetColor(notification.nameColor or '#ffb300')
    GetWidget(sTemp .. '_label_2', fromInterface):SetText((FormatStringNewline(label_2 or '?')))
    GetWidget(sTemp .. '_label_3', fromInterface):SetText(notification.notificationTime or '?')

    GetWidget(sTemp .. '_icon', fromInterface):SetColor('1 1 1 1')
    GetWidget(sTemp .. '_icon', fromInterface):SetTexture(notification.icon)

    GetWidget(sTemp .. '_button_1', fromInterface):SetCallback('onclick', createButtonOnclick(1, notification, sTemp, fromInterface))
    GetWidget(sTemp .. '_button_1', fromInterface):RefreshCallbacks()

    GetWidget(sTemp .. '_button_2', fromInterface):SetCallback('onclick', createButtonOnclick(2, notification, sTemp, fromInterface))
    GetWidget(sTemp .. '_button_2', fromInterface):RefreshCallbacks()

    local interactive = GetWidget(sTemp .. '_button_1', fromInterface):IsVisibleSelf() or GetWidget(sTemp .. '_button_2', fromInterface):IsVisibleSelf()
    Echo('systemMessages:setChatMessageDetail notificationTitle:'..notificationTitle..' interactive: '..tostring(interactive))
    GetWidget(sTemp .. '_line', fromInterface):SetVisible(interactive)
    GetWidget(sTemp .. '_buttons', fromInterface):SetVisible(interactive)
end

function systemMessages:checkRemainingChatMessages(wdgMessage, notification, delete, fromInterface)
    if wdgMessage == nil then return end

    delete = delete or false

    wdgMessage:SlideX(wdgMessage:GetWidth(), INVITE_SLIDE_TIME, true)
    wdgMessage:Sleep(INVITE_SLIDE_TIME,
        function()
            Echo('systemMessages:checkRemainingChatMessages name:'..wdgMessage:GetName()..' notID: '..(notification and notification.id or -1))
            wdgMessage:SetVisible(false)

            local wdgInvisibleToasts = GetInvisibleToastWidgets(fromInterface)
            table.insert(wdgInvisibleToasts, wdgMessage)

            if notification ~= nil then
                local showingToasts = GetShowingToasts(fromInterface)
                for k, v in ipairs(showingToasts) do
                    if notification == v then
                        table.remove(showingToasts, k)
                        break
                    end
                end

                if delete then
                    for k, v in ipairs(systemMessages.messages) do
                        if v == notification then
                            systemMessages:removeMessage(k)
                            break
                        end
                    end
                end
            end

            local wdgVisibleToasts = GetVisibleToastWidgets(fromInterface)
            for k, v in ipairs(wdgVisibleToasts) do
                if v == wdgMessage then
                    table.remove(wdgVisibleToasts, k)
                    break
                end
            end
            local y = 0
            for i=1, #wdgVisibleToasts do
                wdgVisibleToasts[i]:SetY(y)
                y = y + wdgVisibleToasts[i]:GetHeight()
            end

            local pendingToasts = GetPendingToasts(fromInterface)
            if #pendingToasts > 0 then
                systemMessages:openMessageToast(table.remove(pendingToasts, 1), fromInterface)
            end
        end
    )
end

function systemMessages:closeMessageToast(wdgMessage, messageID, fromInterface)
    if wdgMessage == nil then return end

    local message = GetMessageObjByID(tonumber(messageID))
    systemMessages:checkRemainingChatMessages(wdgMessage, message, false, fromInterface)
end

function systemMessages:openMessageToast(notification, fromInterface)
    notification.forcePopup = notification.forcePopup or false

    local index = fromInterface == 'main' and 2 or 1
    Echo('^ysystemMessages:openMessageToast fromInterface:'..fromInterface..' notificationType: '..notification.notificationType..' index:'..index..' forcePopup:'..tostring(notification.forcePopup))
    if not GetNotificationsOptionInfo(nil, index, notification.notificationType) and not notification.forcePopup then
        Echo('^ysystemMessages:openMessageToast not show toast with type: '..notification.notificationType)
        return
    end

    local showingToasts = GetShowingToasts(fromInterface)
    for _, v in ipairs(showingToasts) do
        if notification == v then
            return
        end
    end

    local wdgVisibleToasts = GetVisibleToastWidgets(fromInterface)
    if #wdgVisibleToasts >= MAX_MESSAGE_TOAST_SLOTS then
        local pendingToasts = GetPendingToasts(fromInterface)
        table.insert(pendingToasts, notification)
        return
    end

    --Find one valid widget and refresh
    local wdgInvisibleToasts = GetInvisibleToastWidgets(fromInterface)
    local wdgMessage = table.remove(wdgInvisibleToasts, 1)
    if wdgMessage == nil then
        Echo('systemMessages:openMessageToast no valid widget found in interface:'..fromInterface)
        return
    end

    local y = 0
    for k, v in ipairs(wdgVisibleToasts) do
        y = y + v:GetHeight()
    end

    systemMessages:setChatMessageDetail(wdgMessage:GetName(), notification, fromInterface)

    wdgMessage:SetVisible(true)
    wdgMessage:SetX(wdgMessage:GetWidth())
    wdgMessage:SetY(y)
    wdgMessage:SlideX(0, INVITE_SLIDE_TIME, true)

    wdgMessage:Sleep(GetCvarInt('ui_notifications_toastDuration') * 1000,
        function()
            systemMessages:checkRemainingChatMessages(wdgMessage, notification, false, fromInterface)
        end
    )

    table.insert(showingToasts, notification)
    table.insert(wdgVisibleToasts, wdgMessage)
end

function systemMessages:openMessage(id)
    Echo('systemMessages:openMessage id: '..id)
    local message = GetMessageObj(id)
    if message == nil then
        Echo('^rsystemMessages:openMessage cannot find message object with id: '..id)
        return
    end

    if _selected_message_id == id and not isInvitation(message) then
        return
    end

    _selected_message_id = id

    message.new = false
    GetWidget('system_message_item'..id..'_new'):SetVisible(false)

    if message.type == MessageType_Special then
        Set('special_messages_web_url', message.url, 'string')
        GetWidget('sys_msg_special_message_main_title'):SetText(message.subject)

        systemMessages:openSpecialMessage()
    elseif message.type == MessageType_System_Notification then
        if message.isTest or message.wasOpened then
            systemMessages:openNotification(message)
        else
            GetMessageDetails(message.crc, function(...)
                systemMessages:openNotification(...)
            end)
        end
    elseif message.type == MessageType_Chat_Notification then
        message.forcePopup = true
        systemMessages:openMessageToast(message, 'main')
    end
end

function systemMessages:reset()
    local widget = GetWidget('sys_msg_special_message')
    if widget and widget:IsVisible() then
        widget:DoEventN(1)
    end

    local widget = GetWidget('sys_msg_system_notification')
    if widget and widget:IsVisible() then
        widget:DoEventN(1)
    end

    for i = 1, MAX_MESSAGE_SLOTS do
        interface:GetWidget('system_message_item'..i):SetVisible(false)
    end

    systemMessages.messages = {}
    systemMessages.startIndex, systemMessages.endIndex = 0, 0
    systemMessages.deleteNotificationInfo = nil

    _sysmessage_id = 1
    _selected_message_id = 0
end

function systemMessages:refreshMessageList()
    local id = 1
    for i = systemMessages.startIndex, systemMessages.endIndex do
        local message = systemMessages.messages[i]
        if message and id <= MAX_MESSAGE_SLOTS then
            local prefix, title, subtitle, icon, new, close = 'system_message_item'..id, '', '', '$invis', message.new, false

            if message.type == MessageType_Special then
                title = message.subject
                subtitle = GetDate(message.start_time)..' - '..GetDate(message.end_time)
                icon = '/ui/fe2/NewUI/Res/system_message/msg_type0.png'
            elseif message.type == MessageType_System_Notification then
                title = Translate(message.subject or '')
                icon = message.icon or '$invis'
            elseif message.type == MessageType_Chat_Notification then
                local translateStr = message.notifyTranslateString

                if (message.notificationType == 26) then
                    title = Translate("notify_match_id")..message.sParam1
                elseif message.notificationType >= 27 and message.notificationType <= 30 then
                    title = Translate(message.sParam1..'_situation')
                elseif message.notificationType ~= 24 and NotEmpty(message.gameInfo3) then
                    title = message.gameInfo3
                else
                    title = message.sParam1
                end

                local label_2 = Translate(translateStr .. '_dependent', 'version', message.sParam1, 'streamname', message.sParam1, 'name', message.sParam2, 'replay', message.sParam1)
                if Empty(label_2) then
                    label_2 = NotEmpty(label_2) and (label_2 .. ' ') or ''
                    label_2 = label_2 .. Translate(translateStr .. '_info', 'gamename', message.sParam2, 'rank', message.sParam3, 'name', message.sParam2)

                    if NotEmpty(message.sParam2) and notificationType == 7 then
                        label_2 = label_2 .. ' ' .. Translate(translateStr .. '_info_removed', 'name', message.sParam2)
                    end
                end

                if message.notificationType == CLAN_INVITE then
                    label_2 = Translate('notify_clan_invite_info', 'clan', message.sParam2)
                end

                if message.notificationType == 17 then
                    title = label_2
                else
                    title = title .. ' ' .. label_2
                end

                if Empty(message.nameColor) then
                    message.nameColor = '#ffb300'
                end

                subtitle = message.notificationTime
                icon = message.icon or '$invis'
                close = not isInvitation(message)
            else
                Echo('^rsystemMessages:refreshMessageList failed with unknown message type: '..message.type)
            end

            interface:GetWidget(prefix):SetVisible(true)
            interface:GetWidget(prefix..'_title'):SetText(title)
            interface:GetWidget(prefix..'_subhead'):SetText(subtitle)
            interface:GetWidget(prefix..'_new'):SetVisible(new)
            interface:GetWidget(prefix..'_close'):SetVisible(close)

            local wdgIcon = interface:GetWidget(prefix..'_icon')
            setTextureCheckForWeb(wdgIcon, icon)

            id = id + 1
        end
    end
    if id <= MAX_MESSAGE_SLOTS then
        for i = id, MAX_MESSAGE_SLOTS do
            interface:GetWidget('system_message_item'..i):SetVisible(false)
        end
    end
end

function systemMessages:scrollMessageList(offset)
    if offset == 1 and systemMessages.startIndex == 1 then
        return
    end

    local numMessages = #systemMessages.messages

    local preStartIndex, preEndIndex = systemMessages.startIndex, systemMessages.endIndex

    systemMessages.startIndex = offset
    systemMessages.endIndex = systemMessages.startIndex + MAX_MESSAGE_SLOTS

    if systemMessages.startIndex > (numMessages + MAX_MESSAGE_EMPTY_SLOTS - MAX_MESSAGE_SLOTS) then
        systemMessages.startIndex = numMessages + MAX_MESSAGE_EMPTY_SLOTS - MAX_MESSAGE_SLOTS
    elseif systemMessages.startIndex < 1 then
        systemMessages.startIndex = 1
    end

    if systemMessages.endIndex > (numMessages + MAX_MESSAGE_EMPTY_SLOTS) then
        systemMessages.endIndex = numMessages + MAX_MESSAGE_EMPTY_SLOTS
    end

    if preStartIndex ~= systemMessages.startIndex or preEndIndex ~= systemMessages.endIndex then
        systemMessages:refreshMessageList()
    end
end

function systemMessages:generateMessageList()
    local numMessages = #systemMessages.messages
    if numMessages > 0 then
        local showScroll = numMessages > MAX_MESSAGE_SLOTS

        local scrollBar = interface:GetWidget('system_message_list_scrollbar')
        local scrollPanel = interface:GetWidget('system_message_list_scrollpanel')

        scrollBar:SetVisible(showScroll)
        scrollPanel:SetVisible(showScroll)

        if showScroll then
            systemMessages.startIndex = systemMessages.startIndex > 0 and systemMessages.startIndex or 1

            scrollBar:SetMaxValue(numMessages + MAX_MESSAGE_EMPTY_SLOTS - MAX_MESSAGE_SLOTS)
            scrollBar:SetMinValue(1)
        else
            systemMessages.startIndex = 1
        end
        scrollBar:SetValue(systemMessages.startIndex)

        if (systemMessages.startIndex + MAX_MESSAGE_SLOTS - 1) <= numMessages then
            systemMessages.endIndex = systemMessages.startIndex + MAX_MESSAGE_SLOTS - 1
        else
            systemMessages.endIndex = numMessages
        end
    else
        systemMessages.startIndex = 0
        systemMessages.endIndex = 0
    end

    systemMessages:refreshMessageList()
end

function systemMessages:addMessage(message, bNewList, bMark)
    Echo('systemMessages:addMessage message: '..message.id)
    bNewList = bNewList or false
    bMark = bMark or true

    message.new = true

    if message.type == MessageType_Special then
        table.insert(systemMessages.messages, 1, message)
    else
        table.insert(systemMessages.messages, message)
    end

    if bMark then
        systemMessages:showNewMessageMark(true)
    end
    if bNewList then
        systemMessages:generateMessageList()
    end
end

function systemMessages:removeMessage(index)
    table.remove(systemMessages.messages, index)

    local hasNew = false
    for _, v in ipairs(systemMessages.messages) do
        if v.new then
            hasNew = true
            break
        end
    end
    if not hasNew then
        systemMessages:showNewMessageMark(false)
    end

    if index >= systemMessages.startIndex and index <= systemMessages.endIndex then
        systemMessages:generateMessageList()
    end
end

function systemMessages:clearMessages()
    for i = #systemMessages.messages, 1, -1 do
        if systemMessages.messages[i].type == MessageType_Chat_Notification then
            table.remove(systemMessages.messages, i)
        elseif systemMessages.messages[i].type == MessageType_System_Notification then
            if systemMessages.messages[i].deletable or systemMessages.messages[i].isTest then
                if systemMessages.deleteNotificationInfo == nil then
                    systemMessages.deleteNotificationInfo = systemMessages.messages[i]
                    DeleteMessage(systemMessages.messages[i].crc)
                else
                    table.insert(systemMessages.deleteNotifications, systemMessages.messages[i])
                end
                table.remove(systemMessages.messages, i)
            end
        end
    end

    systemMessages:showNewMessageMark(false)
    systemMessages:generateMessageList()
end

function systemMessages:resetMessage()
    if _selected_message_id < 1 or _selected_message_id > MAX_MESSAGE_SLOTS then
        return
    end

    local message = GetMessageObj(_selected_message_id)
    if message ~= nil then
        if message.type == MessageType_System_Notification then
            systemMessages.deleteNotificationInfo = message
            Echo('systemMessages:resetMessage message.deletable: '..tostring(message.deletable))

            if message.isTest then
                systemMessages:clearDeletedMessage(message)
            elseif message.deletable then
                DeleteMessage(message.crc)
            end

            systemMessages:removeMessage(systemMessages.startIndex + _selected_message_id - 1)
        end
    end

    _selected_message_id = 0
end

function systemMessages:toggleSpecialMessage()
    local outerPanel = GetWidget('systemSpecialMessagePopup')
    if outerPanel:IsVisible() then
        self:closeSpecialMessage()
    else
        self:openSpecialMessage()
    end
end

function systemMessages:openSpecialMessage()
    local messagePopup = GetWidget('sys_msg_special_message_main')
    messagePopup:SetVisible(true)
    messagePopup:SetWidth('1260i')
    messagePopup:SetHeight('820i')

    GetWidget('sys_msg_special_message'):SetVisible(true)
    GetWidget('sys_msg_special_message_main_frame'):SetVisible(true)

    GetWidget('sys_msg_special_message_main_header'):SetVisible(true)
    GetWidget('sys_msg_special_message_main_close'):SetVisible(true)
    GetWidget('web_browser_special_messages_insert'):SetVisible(true)
end

function systemMessages:closeSpecialMessage()
    _selected_message_id = 0

    local msgButtonContainer = GetWidget('sysbar_notifications')
    local x, y = msgButtonContainer:GetAbsoluteX()+msgButtonContainer:GetWidth()/2, msgButtonContainer:GetAbsoluteY()+msgButtonContainer:GetHeight()/2

    GetWidget('web_browser_special_messages_insert'):SetVisible(false)
    GetWidget('sys_msg_special_message_main_header'):SetVisible(false)

    local messagePopup = GetWidget('sys_msg_special_message_main')
    messagePopup:Scale('150i', '100i', PANEL_CLOSE_ANIMATION_TIME)
    messagePopup:SlideX(x - 15, PANEL_CLOSE_ANIMATION_TIME)
    messagePopup:SlideY(y, PANEL_CLOSE_ANIMATION_TIME)
    messagePopup:Sleep(PANEL_CLOSE_ANIMATION_TIME, function(widget)
        GetWidget('sys_msg_special_message_main_frame'):SetVisible(false)
        GetWidget('sys_msg_special_message_main_close'):SetVisible(false)
        widget:GetParent():DoEventN(1)
    end)
end

-- function systemMessages:checkCreateAccountSMCache()
--     local accountID = Client.GetAccountID()
--     local key = accountID..'_sm'
--     self.cache[key] = self.cache[key] or {}

--     return self.cache[key]
-- end

function systemMessages:checkCreateAccountCache()
    local accountID = Client.GetAccountID()
    self.sys_notification_cache[accountID] = self.sys_notification_cache[accountID] or {}

    return self.sys_notification_cache[accountID]
end

-- General cleanup on a deleted message (remove from cache, get rid of slideout, and remove list entry)
function systemMessages:clearDeletedMessage(notification)
    if notification and type(notification) == 'table' then
        local notificationCache = self:checkCreateAccountCache()

        if not notification.isTest then
            if notificationCache[notification.crc] then
                tableMerge(notificationCache[notification.crc], notification)
                notificationCache[notification.crc] = nil
            end
        end

        GetDBEntry('systemNotificationsCache', self.cache, true, false, false)
    end
end

function systemMessages:processStringParams(stringKey, paramTable)
    stringKey = string.gsub(stringKey, '%[PLAYERNAME%]', GetAccountName())
    return Translate(stringKey, unpack(paramTable))
end

function systemMessages:buildParamTable(notification)
    local paramTable = {}
    local tempParam
    for k, v in ipairs({'amount', 'productId', 'productType', 'gifterNick', 'accountId'}) do
        tempParam = notification.meta[v]
        if not Empty(tempParam) then
            table.insert(paramTable, v)
            table.insert(paramTable, tempParam)

            if (notification.subject == 'sysmessage_product_gift' or notification.subject == 'sysmessage_product_removed' or notification.subject == 'sysmessage_product_system') then
                if v == 'productId' then
                    if Empty(notification.meta.bodyTitle) then
                        notification.meta.bodyTitle = Translate('mstore_product'..tempParam..'_name')
                    end
                    table.insert(paramTable, 'productName')
                    table.insert(paramTable, 'mstore_product'..tempParam..'_name')
                elseif v == 'productType' then
                    if Empty(notification.meta.body) then
                        notification.meta.body = Translate('general_product_type_'..tempParam)
                    end
                    table.insert(paramTable, 'productType')
                    table.insert(paramTable, 'general_product_type_'..tempParam)
                end
            end
        end
    end

    return paramTable
end

function systemMessages:openNotification(openedNotification)
    if openedNotification and type(openedNotification) == 'table' then
        -- Echo('systemMessages:openNotification openedNotification:')
        -- printTableDeep(openedNotification)

        local notification

        if openedNotification.isTest or openedNotification.wasOpened then
            notification = openedNotification
        else
            local notificationCache = self:checkCreateAccountCache()
            notification = notificationCache[openedNotification.crc]

            --don't merge icon
            local icon = notification.icon

            tableMerge(openedNotification, notification)

            notification.icon = icon
        end

        notification.wasOpened = true

        if notification.meta.type and (notification.meta.type == NOTIFICATIONS_TYPE_BET_SUCCESS or notification.meta.type == NOTIFICATIONS_TYPE_BET_CANCEL) then
            self:SetBetMessage(notification)
        elseif notification.meta.type and notification.meta.type == NOTIFICATIONS_TYPE_SUBSCRIBED_MATCH_BEGIN then
            self:SetSubscribedMatchBeginMessage(notification)
        elseif notification.meta.type and notification.meta.type == NOTIFICATIONS_TYPE_BETTED_MATCH_BEGIN then
            self:SetBettedMatchBeginMessage(notification)
        elseif notification.meta.type and notification.meta.type == NOTIFICATIONS_TYPE_POINTS_CHANGED then
            self:SetPointsChangedMessage(notification)
        elseif notification.meta.type and notification.meta.type == NOTIFICATIONS_TYPE_RAP then
            self:SetRAPMessage(notification)
        else
            self:SetDefaultMessage(notification)
        end

        GetWidget('systemNotificationsPopupAction'):SetCallback('onclick', function(widget) GetWidget('sys_msg_system_notification'):DoEventN(1) end)
        GetWidget('sys_msg_system_notification'):DoEventN(0)
    else
        print('Empty/invalid notification details. --  ('..tostring(notification) .. ')\n')
    end
end

function systemMessages:processInvisibleNotification(notification)
    -- when get certain messages, client need to get latest rap info to change UI, and these messages can't be seen by player
    if notification.subject and notification.subject == 'REPORT_FROM_WARNING_TO_NORMAL_SUBJECT'then
        UpdateRapStatus(false)
        DeleteMessage(notification.crc)
        return false
    end
    return true
end

--[[ notification table
k: messageIndex | v: 23
k: sent | v: 1468563055
k: crc | v: bf611f98
k: deletable | v: true
k: read | v: false
k: expiration | v: 0
k: displayed | v: true
k: icon | v: /heroes/shadowblade/alt2/icon.tga
k: subject | v: Test Login Event
--]]
function systemMessages:newNotification(notification, generateList, showMark)
    generateList = generateList or true
    showMark = showMark or true

    if systemMessages:processInvisibleNotification(notification) then

        notification.displayed = true
        notification.id = GenerateMessageID()
        notification.type = MessageType_System_Notification

        local iconPath, subject = notification.icon, notification.subject

        if subject then
            if subject == 'BET_WIN_SUBJECT' or subject == 'BET_CANCELLED_SUBJECT' then
                iconPath = '/ui/icons/message_bet.tga'
            elseif subject == 'SUBSCRIBED_MATCH_BEGIN_SUBJECT' then
                iconPath = '/ui/icons/message_subscribe.tga'
            elseif subject == 'BETTED_MATCH_BEGIN_SUBJECT' then
                iconPath = '/ui/icons/message_match.tga'
            end

            if subject == 'POINTS_CHANGED_SUBJECT' then
                SubmitForm('PointsChanged', 'f', 'get_points', 'cookie', Client.GetCookie());
            end

            -- when get certain messages, client need to get latest rap info to change UI
            if subject == 'REPORT_SUSPEND_SUBJECT' or subject == 'REPORT_WARNING_SUBJECT' then
                local updated = notification.updated or false
                if not updated then
                    UpdateRapStatus(true)
                    notification.updated = true
                end
            end
        end

        if Empty(iconPath) then
            iconPath = '/ui/elements/empty_pack.tga'
        end

        notification.icon = iconPath

        systemMessages:addMessage(notification, generateList, showMark)
    end
end

-- General cleanup on a deleted message (remove from cache, get rid of slideout, and remove list entry)
function systemMessages:clearDeletedNotification(notification)
    if notification and type(notification) == 'table' then
        local notificationCache = self:checkCreateAccountCache()

        if not notification.isTest then
            if notificationCache[notification.crc] then
                tableMerge(notificationCache[notification.crc], notification)
                notificationCache[notification.crc] = nil
            end
        end

        GetDBEntry('systemNotificationsCache', self.cache, true, false, false)
    end
end

function systemMessages:SetDefaultMessage(notification)
    local title = GetWidget('systemNotificationsPopupTitle')
    local subtitle = GetWidget('systemNotificationsPopupSubtitle')
    local icon = GetWidget('systemNotificationsPopupIcon')
    local image = GetWidget('systemNotificationsPopupImage')
    local imageContainer = GetWidget('systemNotificationsPopupImageContainer')
    local bodyTitle = GetWidget('systemNotificationsPopupBodyTitle')
    local body = GetWidget('systemNotificationsPopupBody')
    local footer = GetWidget('systemNotificationsFooter')
    local footerLabel = GetWidget('systemNotificationsFooterLabel')

    setTextureCheckForWeb(icon, notification.icon)

    local paramTable

    if notification.meta and type(notification.meta) == 'table' then
        paramTable = self:buildParamTable(notification)

        if not Empty(notification.meta.subtitle) then
            subtitle:SetText(self:processStringParams(notification.meta.subtitle, paramTable))
            subtitle:SetVisible(true)
        else
            subtitle:SetVisible(false)
        end

        if not Empty(notification.meta.bodyTitle) then
            bodyTitle:SetText(self:processStringParams(notification.meta.bodyTitle, paramTable))
            bodyTitle:SetVisible(true)
        else
            bodyTitle:SetVisible(false)
        end

        -- Echo('systemMessages:SetDefaultMessage notification.meta.image:'..tostring(notification.meta.image))
        if NotEmpty(notification.meta.image) then
            imageContainer:SetVisible(true)

            if notification.meta.imageHeight then
                image:SetHeight(notification.meta.imageHeight)
            else
                image:SetHeight('30h')
            end

            if notification.meta.imageWidth then
                image:SetWidth(notification.meta.imageWidth)
            else
                image:SetWidth('30h')
            end

            setTextureCheckForWeb(image, notification.meta.image)
        else
            imageContainer:SetVisible(false)
        end

        if not Empty(notification.meta.body) and notification.meta.body ~= 'general_product_type_' and notification.meta.body ~= 'general_product_type_false' then
            if paramTable then
                body:SetText(self:processStringParams(notification.meta.body, paramTable))
            else
                body:SetText(Translate(notification.meta.body))
            end
            body:SetVisible(true)
        else
            body:SetVisible(false)
        end

        if not Empty(notification.meta.footer) then
            footer:SetVisible(true)
            footerLabel:SetText(self:processStringParams(notification.meta.footer, paramTable))
        else
            footer:SetVisible(false)
        end
    else
        body:SetVisible(false)
        subtitle:SetVisible(false)
        bodyTitle:SetVisible(false)
        imageContainer:SetVisible(false)
    end

    title:SetText(not Empty(notification.subject) and Translate(notification.subject) or '---')
    title:SetVisible(true)
end

function systemMessages:SetBetMessage(notification)
    local title = GetWidget('systemNotificationsPopupTitle')
    local subtitle = GetWidget('systemNotificationsPopupSubtitle')
    local icon = GetWidget('systemNotificationsPopupIcon')
    local image = GetWidget('systemNotificationsPopupImage')
    local imageContainer = GetWidget('systemNotificationsPopupImageContainer')
    local bodyTitle = GetWidget('systemNotificationsPopupBodyTitle')
    local body = GetWidget('systemNotificationsPopupBody')
    local footer = GetWidget('systemNotificationsFooter')
    local footerLabel = GetWidget('systemNotificationsFooterLabel')

    subtitle:SetVisible(false)
    imageContainer:SetVisible(false)
    bodyTitle:SetVisible(false)
    footer:SetVisible(false)

    -- Echo('systemMessages:SetBetMessage notification.icon: '..notification.icon)

    setTextureCheckForWeb(icon, notification.icon)

    title:SetText(Translate(notification.subject))

    if notification.meta.betting_mode and notification.meta.type == NOTIFICATIONS_TYPE_BET_SUCCESS then
        body:SetVisible(true)
        body:SetText(Translate('system_message_bet_successful', 'bettype', Translate('system_message_bettype_'..notification.meta.betting_mode), 'matchname', notification.meta.match_name, 'coinnum', notification.meta.reward))
    elseif notification.meta.betting_mode and notification.meta.type == NOTIFICATIONS_TYPE_BET_CANCEL then
        body:SetText(Translate('system_message_bet_cancel', 'bettype', Translate('system_message_bettype_'..notification.meta.betting_mode), 'matchname', notification.meta.match_name, 'coinnum', notification.meta.coin))
        body:SetVisible(true)
    end
end

function systemMessages:SetSubscribedMatchBeginMessage(notification)
    local title = GetWidget('systemNotificationsPopupTitle')
    local subtitle = GetWidget('systemNotificationsPopupSubtitle')
    local icon = GetWidget('systemNotificationsPopupIcon')
    local image = GetWidget('systemNotificationsPopupImage')
    local imageContainer = GetWidget('systemNotificationsPopupImageContainer')
    local bodyTitle = GetWidget('systemNotificationsPopupBodyTitle')
    local body = GetWidget('systemNotificationsPopupBody')
    local footer = GetWidget('systemNotificationsFooter')
    local footerLabel = GetWidget('systemNotificationsFooterLabel')

    setTextureCheckForWeb(icon, notification.icon)

    local paramTable

    if notification.meta and type(notification.meta) == 'table' then
        paramTable = self:buildParamTable(notification)

        if not Empty(notification.meta.title) then
            title:SetText(self:processStringParams(notification.meta.title, paramTable))
            title:SetVisible(true)
        else
            title:SetVisible(false)
        end

        if not Empty(notification.meta.subtitle) then
            subtitle:SetText(self:processStringParams(notification.meta.subtitle, paramTable))
            subtitle:SetVisible(true)
        else
            subtitle:SetVisible(false)
        end

        if not Empty(notification.meta.bodyTitle) then
            bodyTitle:SetText(self:processStringParams(notification.meta.bodyTitle, paramTable))
            bodyTitle:SetVisible(true)
        else
            bodyTitle:SetVisible(false)
        end

        if not Empty(notification.meta.image) then
            imageContainer:SetVisible(true)

            if notification.meta.imageHeight then
                image:SetHeight(notification.meta.imageHeight)
            else
                image:SetHeight('30h')
            end

            if notification.meta.imageWidth then
                image:SetWidth(notification.meta.imageWidth)
            else
                image:SetWidth('30h')
            end

            setTextureCheckForWeb(image, notification.meta.image)
        else
            imageContainer:SetVisible(false)
        end

        body:SetText(Translate('subscribed_match_begin', 'clan', notification.meta.clan_name, 'match', notification.meta.match_name))
        body:SetVisible(true)

        if not Empty(notification.meta.footer) then
            footer:SetVisible(true)
            footerLabel:SetText(self:processStringParams(notification.meta.footer, paramTable))
        else
            footer:SetVisible(false)
        end
    else
        body:SetVisible(false)
        subtitle:SetVisible(false)
        bodyTitle:SetVisible(false)
        imageContainer:SetVisible(false)
    end

    title:SetText(not Empty(notification.subject) and Translate(notification.subject) or '---')
    title:SetVisible(true)
end

function systemMessages:SetBettedMatchBeginMessage(notification)
    local title = GetWidget('systemNotificationsPopupTitle')
    local subtitle = GetWidget('systemNotificationsPopupSubtitle')
    local icon = GetWidget('systemNotificationsPopupIcon')
    local image = GetWidget('systemNotificationsPopupImage')
    local imageContainer = GetWidget('systemNotificationsPopupImageContainer')
    local bodyTitle = GetWidget('systemNotificationsPopupBodyTitle')
    local body = GetWidget('systemNotificationsPopupBody')
    local footer = GetWidget('systemNotificationsFooter')
    local footerLabel = GetWidget('systemNotificationsFooterLabel')

    setTextureCheckForWeb(icon, notification.icon)

    local paramTable

    if notification.meta and type(notification.meta) == 'table' then
        paramTable = self:buildParamTable(notification)

        if not Empty(notification.meta.title) then
            title:SetText(self:processStringParams(notification.meta.title, paramTable))
            title:SetVisible(true)
        else
            title:SetVisible(false)
        end

        if not Empty(notification.meta.subtitle) then
            subtitle:SetText(self:processStringParams(notification.meta.subtitle, paramTable))
            subtitle:SetVisible(true)
        else
            subtitle:SetVisible(false)
        end

        if not Empty(notification.meta.bodyTitle) then
            bodyTitle:SetText(self:processStringParams(notification.meta.bodyTitle, paramTable))
            bodyTitle:SetVisible(true)
        else
            bodyTitle:SetVisible(false)
        end

        if not Empty(notification.meta.image) then
            imageContainer:SetVisible(true)

            if notification.meta.imageHeight then
                image:SetHeight(notification.meta.imageHeight)
            else
                image:SetHeight('30h')
            end

            if notification.meta.imageWidth then
                image:SetWidth(notification.meta.imageWidth)
            else
                image:SetWidth('30h')
            end

            setTextureCheckForWeb(image, notification.meta.image)
        else
            imageContainer:SetVisible(false)
        end

        body:SetText(Translate('betted_match_begin', 'match', notification.meta.match_name))
        body:SetVisible(true)

        if not Empty(notification.meta.footer) then
            footer:SetVisible(true)
            footerLabel:SetText(self:processStringParams(notification.meta.footer, paramTable))
        else
            footer:SetVisible(false)
        end
    else
        body:SetVisible(false)
        subtitle:SetVisible(false)
        bodyTitle:SetVisible(false)
        imageContainer:SetVisible(false)
    end

    title:SetText(not Empty(notification.subject) and Translate(notification.subject) or '---')
    title:SetVisible(true)
end

function systemMessages:SetPointsChangedMessage(notification)
    local title = GetWidget('systemNotificationsPopupTitle')
    local subtitle = GetWidget('systemNotificationsPopupSubtitle')
    local icon = GetWidget('systemNotificationsPopupIcon')
    local image = GetWidget('systemNotificationsPopupImage')
    local imageContainer = GetWidget('systemNotificationsPopupImageContainer')
    local bodyTitle = GetWidget('systemNotificationsPopupBodyTitle')
    local body = GetWidget('systemNotificationsPopupBody')
    local footer = GetWidget('systemNotificationsFooter')
    local footerLabel = GetWidget('systemNotificationsFooterLabel')

    setTextureCheckForWeb(icon, notification.icon)

    local paramTable

    if notification.meta and type(notification.meta) == 'table' then
        paramTable = self:buildParamTable(notification)

        if not Empty(notification.meta.title) then
            title:SetText(self:processStringParams(notification.meta.title, paramTable))
            title:SetVisible(true)
        else
            title:SetVisible(false)
        end

        if not Empty(notification.meta.subtitle) then
            subtitle:SetText(self:processStringParams(notification.meta.subtitle, paramTable))
            subtitle:SetVisible(true)
        else
            subtitle:SetVisible(false)
        end

        if not Empty(notification.meta.bodyTitle) then
            bodyTitle:SetText(self:processStringParams(notification.meta.bodyTitle, paramTable))
            bodyTitle:SetVisible(true)
        else
            bodyTitle:SetVisible(false)
        end

        if not Empty(notification.meta.image) then
            imageContainer:SetVisible(true)

            if notification.meta.imageHeight then
                image:SetHeight(notification.meta.imageHeight)
            else
                image:SetHeight('30h')
            end

            if notification.meta.imageWidth then
                image:SetWidth(notification.meta.imageWidth)
            else
                image:SetWidth('30h')
            end

            setTextureCheckForWeb(image, notification.meta.image)
        else
            imageContainer:SetVisible(false)
        end

        body:SetText(Translate('points_changed', 'points', notification.meta.points, 'channel', Translate(notification.meta.channel)))
        body:SetVisible(true)


        if not Empty(notification.meta.footer) then
            footer:SetVisible(true)
            footerLabel:SetText(self:processStringParams(notification.meta.footer, paramTable))
        else
            footer:SetVisible(false)
        end
    else
        body:SetVisible(false)
        subtitle:SetVisible(false)
        bodyTitle:SetVisible(false)
        imageContainer:SetVisible(false)
    end

    title:SetText(not Empty(notification.subject) and Translate(notification.subject) or '---')
    title:SetVisible(true)
end

function systemMessages:SetRAPMessage(notification)
    local title = GetWidget('systemNotificationsPopupTitle')
    local subtitle = GetWidget('systemNotificationsPopupSubtitle')
    local icon = GetWidget('systemNotificationsPopupIcon')
    local image = GetWidget('systemNotificationsPopupImage')
    local imageContainer = GetWidget('systemNotificationsPopupImageContainer')
    local bodyTitle = GetWidget('systemNotificationsPopupBodyTitle')
    local body = GetWidget('systemNotificationsPopupBody')
    local footer = GetWidget('systemNotificationsFooter')
    local footerLabel = GetWidget('systemNotificationsFooterLabel')

    setTextureCheckForWeb(icon, notification.icon)

    local paramTable
    local subject = notification.subject

    if notification.meta and type(notification.meta) == 'table' then
        paramTable = self:buildParamTable(notification)

        if not Empty(notification.meta.title) then
            title:SetText(self:processStringParams(notification.meta.title, paramTable))
            title:SetVisible(true)
        else
            title:SetVisible(false)
        end

        if not Empty(notification.meta.subtitle) then
            subtitle:SetText(self:processStringParams(notification.meta.subtitle, paramTable))
            subtitle:SetVisible(true)
        else
            subtitle:SetVisible(false)
        end

        if not Empty(notification.meta.bodyTitle) then
            bodyTitle:SetText(self:processStringParams(notification.meta.bodyTitle, paramTable))
            bodyTitle:SetVisible(true)
        else
            bodyTitle:SetVisible(false)
        end

        if not Empty(notification.meta.image) then
            imageContainer:SetVisible(true)

            if notification.meta.imageHeight then
                image:SetHeight(notification.meta.imageHeight)
            else
                image:SetHeight('30h')
            end

            if notification.meta.imageWidth then
                image:SetWidth(notification.meta.imageWidth)
            else
                image:SetWidth('30h')
            end

            setTextureCheckForWeb(image, notification.meta.image)
        else
            imageContainer:SetVisible(false)
        end

        if subject == 'REPORT_REFUND_SUBJECT' then
            body:SetText(Translate('rap_report_refund_body', 'nickname', notification.meta.reportee_nickname, 'time', notification.meta.report_time))
        elseif subject == 'REPORT_SUSPEND_SUBJECT' then
            body:SetText(Translate('rap_report_suspend_body', 'time', notification.meta.time))
        elseif subject == 'REPORT_WARNING_SUBJECT' then
            body:SetText(Translate('rap_report_warning_body', 'time', notification.meta.time))
        end

        body:SetVisible(true)

        if not Empty(notification.meta.footer) then
            footer:SetVisible(true)
            footerLabel:SetText(self:processStringParams(notification.meta.footer, paramTable))
        else
            footer:SetVisible(false)
        end
    else
        body:SetVisible(false)
        subtitle:SetVisible(false)
        bodyTitle:SetVisible(false)
        imageContainer:SetVisible(false)
    end

    title:SetText(not Empty(notification.subject) and Translate(notification.subject) or '---')
    title:SetVisible(true)
end

---[[
local function systemNotificationTestInvite()
    -- for i=3, 22 do
        local i = 20
        local name = i < 10 and 'snq00'..i or 'snq0'..i
        receiveChatNotification(_, name, '', 23, 'notify_buddy_requested_added', 'notfication_generic_action', '', '03/19 03:47 AM', '951245', 1, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', 'true', '', '', '')
    -- end
end

local function systemNotificationTestGift()
    systemMessages:newNotification({
        subject     = 'sysmessage_product_gift',
        icon        = '/ui/icons/gift_red.tga',
        expiration  = 1446768333,
        crc         = 'aaaaaaaaaaa',
        meta        = {
            productType     = 'aa',
            gifterNick      = 'FBMerc',
            subtitle    = 'sysmessage_product_gift_body',
            productId   = '1282'
        },
        isTest      = true
    }, nil, true)
end

local function systemNotificationTestProductRemoved()
    systemMessages:newNotification({
        subject     = 'sysmessage_product_removed',
        icon        = '/ui/icons/alert_green.tga',
        expiration  = 1446768331,
        crc         = 'aaaaaaaaaab',
        meta        = {
            productType     = 'aa',
            productId   = '1282'

        },
        isTest      = true
    }, nil, true)
end

local function systemNotificationTestProductAdded()
    systemMessages:newNotification({
        subject     = 'sysmessage_product_system',
        icon        = '/ui/icons/gift_blue.tga',
        expiration  = 1446768331,
        crc         = 'aaaaaaaaaac',
        meta        = {
            productType     = 'aa',
            subtitle    = 'sysmessage_product_system_body',
            productId   = '1282'
        },
        isTest      = true
    }, nil, true)
end

local function systemNotificationTestGiftSilver()
    systemMessages:newNotification({
        subject     = 'sysmessage_silver_friend',
        icon        = '/ui/icons/silver_coin_stack.tga',
        expiration  = 1446768333,
        crc         = 'aaaaaaaaaad',
        meta        = {
            body    = 'sysmessage_silver_friend_body',
            gifterNick  = 'merctest56',
            amount  = tostring(math.random(25, 250))
        },
        isTest      = true
    }, nil, true)
end

local function systemNotificationTestSilver()
    systemMessages:newNotification({
        subject     = 'sysmessage_silver_system_add',
        icon        = '/ui/icons/silver_coin_stack.tga',
        expiration  = 1446768333,
        crc         = 'aaaaaaaaaae',
        meta        = {
            body    = 'sysmessage_silver_system_add_body',
            amount  = tostring(math.random(25, 250))
        },
        isTest      = true
    }, nil, true)
end

local function systemNotificationTestSilverRemove()
    systemMessages:newNotification({
        subject     = 'sysmessage_silver_system_remove',
        icon        = '/ui/icons/alert_green.tga',
        expiration  = 1446768333,
        crc         = 'aaaaaaaaaaf',
        meta        = {
            body    = 'sysmessage_silver_system_remove_body',
            amount  = tostring(math.random(25, 250))
        },
        isTest      = true
    }, nil, true)
end

local function systemNotificationTestGiftGold()
    systemMessages:newNotification({
        subject     = 'sysmessage_gold_friend',
        icon        = '/ui/icons/gold_coin_stack.tga',
        expiration  = 1446768333,
        crc         = 'aaaaaaaaaag',
        meta        = {
            body    = 'sysmessage_gold_friend_body',
            amount  = tostring(math.random(25, 250)),
            gifterNick  = 'merctest101'
        },
        isTest      = true
    }, nil, true)
end

local function systemNotificationTestGold()
    systemMessages:newNotification({
        subject     = 'sysmessage_gold_system_add',
        icon        = '/ui/icons/gold_coin_stack.tga',
        expiration  = 1446768333,
        crc         = 'aaaaaaaaaah',
        meta        = {
            body    = 'sysmessage_gold_system_add_body',
            amount  = tostring(math.random(25, 250))
        },
        isTest      = true
    }, nil, true)
end

local function systemNotificationTestGoldRemove()
    systemMessages:newNotification({
        subject     = 'sysmessage_gold_system_remove',
        icon        = '/ui/icons/alert_green.tga',
        expiration  = 1446768333,
        crc         = 'aaaaaaaaaai',
        meta        = {
            body    = 'sysmessage_gold_system_remove_body',
            amount  = tostring(math.random(25, 250))
        },
        isTest      = true
    }, nil, true)
end

local function systemNotificationTestGiftTickets()
    systemMessages:newNotification({
        subject     = 'sysmessage_tickets_friend',
        icon        = '/ui/icons/tickets_stack.tga',
        expiration  = 1446768333,
        crc         = 'aaaaaaaaaaj',
        meta        = {
            body    = 'sysmessage_tickets_friend_body',
            gifterNick  = 'merctest102',
            amount  = tostring(math.random(25, 250))
        },
        isTest      = true
    }, nil, true)
end

local function systemNotificationTestTickets()
    systemMessages:newNotification({
        subject     = 'sysmessage_tickets_system_add',
        icon        = '/ui/icons/tickets_stack.tga',
        expiration  = 1446768333,
        crc         = 'aaaaaaaaaak',
        meta        = {
            body    = 'sysmessage_tickets_system_add_body',
            amount  = tostring(math.random(25, 250))
        },
        isTest      = true
    }, nil, true)
end

local function systemNotificationTestTicketsRemove()
    systemMessages:newNotification({
        subject     = 'sysmessage_tickets_system_remove',
        icon        = '/ui/icons/alert_green.tga',
        expiration  = 1446768333,
        crc         = 'aaaaaaaaaal',
        meta        = {
            body    = 'sysmessage_tickets_system_remove_body',
            amount  = tostring(math.random(25, 250))
        },
        isTest      = true
    }, nil, true)
end

local function systemNotificationTestEvent()
    systemMessages:newNotification({
        subject     = 'Mercenary Legionnaire Event',
        icon        = '/heroes/legionnaire/icon.tga',
        expiration  = 1446768333,
        crc         = 'aaaaaaaaaam',
        meta        = {
            body        = "Hello [PLAYERNAME], we've announced the Mercenary Legionnaire release event.  Check it out November 20th.  Good luck if you don't speak English cause this won't be translated.",
            image       = 'http://1.bp.blogspot.com/-pCYJKyFLBtg/Vk83JW6W8bI/AAAAAAAABDU/3MnY6OoTS_0/s1600/legion-MOTD-2.jpg',
            imageWidth  = '43.5h',
            imageHeight = '30h',
            footer      = "sysmessage_footer_website"
        },
        isTest      = true
    }, nil, true)
end

local function systemNotificationTestDowntime()
    systemMessages:newNotification({
        subject     = 'sysmessage_downtime',
        expiration  = 1446768000,
        -- ['local']    = true,     -- we'll always be translating these
        crc         = 'aaaaaaaaaan',
        meta        = {
            body    = "sysmessage_downtime_body",
            footer  = "sysmessage_footer_website"
        },
        icon        = '/ui/icons/alert_yellow.tga',
        isTest      = true
    }, nil, true)
end

local spamThread = nil

function systemMessages:systemNotificationsTestSpam(doWait, waitMaxAdd)
    if doWait == nil then doWait = true end
    local waitMaxAdd = waitMaxAdd or 0

    if spamThread ~= nil then
        spamThread:Kill()
        spamThread = nil
    end

    spamThread = newthread(function()
        spamList = {
            systemNotificationTestInvite,
            systemNotificationTestGift,
            systemNotificationTestGiftSilver,
            systemNotificationTestSilver,
            systemNotificationTestSilverRemove,
            systemNotificationTestGiftGold,
            systemNotificationTestGold,
            systemNotificationTestGoldRemove,
            systemNotificationTestGiftTickets,
            systemNotificationTestTickets,
            systemNotificationTestTicketsRemove,
            systemNotificationTestEvent,
            systemNotificationTestProductRemoved,
            systemNotificationTestProductAdded,
            systemNotificationTestDowntime
        }

        for k,v in ipairs(spamList) do
            v()
            if doWait then
                wait(math.random(500, 1000 + waitMaxAdd))
            end
        end
        spamThread = nil
    end)
end

function systemMessages:systemNotificationsClearCache()
    GetDBEntry('systemNotificationsCache', {}, true, false, false)
end
--]]