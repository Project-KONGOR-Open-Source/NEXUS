-- System Notifications

local SYSBAR_VIEW_NONE					= 0
local SYSBAR_VIEW_SYS_NOTIFICATIONS		= 1
local SYSBAR_VIEW_FRIENDS				= 2
local SYSBAR_VIEW_FRIEND_OPTIONS		= 3
local SYSBAR_VIEW_NOTIFICATIONS			= 4

local SYSBAR_SOCIAL_PANEL_FRIENDS		= 0
local SYSBAR_SOCIAL_PANEL_NOTIFICATIONS	= 1

local sysbarLastSection = SYSBAR_VIEW_NONE

local NOTIFICATIONS_TRANSITION_IN_TIME		= 250
local NOTIFICATIONS_TRANSITION_OUT_TIME		= 150
local NOTIFICATIONS_TRANSITION_MOVE_TIME	= 150
local NOTIFICATIONS_DISPLAY_TIME			= 8000
local NOTIFICATIONS_SLIDEOUTS_MAX			= 3

local NOTIFICATIONS_TYPE_BET_SUCCESS               = '1'
local NOTIFICATIONS_TYPE_BET_CANCEL                = '2'
local NOTIFICATIONS_TYPE_SUBSCRIBED_MATCH_BEGIN    = '3'
local NOTIFICATIONS_TYPE_BETTED_MATCH_BEGIN        = '4'
local NOTIFICATIONS_TYPE_CUSTOMIZED                = '5' -- this type of message fits default message
local NOTIFICATIONS_TYPE_POINTS_CHANGED            = '6'
local NOTIFICATIONS_TYPE_RAP                       = '7'

systemNotifications = systemNotifications or {}

systemNotifications.slideoutQueue			= {}
systemNotifications.messageIndex			= 0
systemNotifications.slideoutWidgetsActive	= {}
systemNotifications.deleteNotificationInfo	= nil
systemNotifications.cache = GetDBEntry('systemNotificationsCache', nil, true, false, true) or {}	-- (entry, value, saveToDB, restoreDefault, setDefault)
systemNotifications.activeEntries			= {}	-- only messageIndex.  this is for manipulating entries in the list

function systemNotifications:toggle()
	if self.container:IsVisible() then
		self:hide()
	else
		self:show()
	end
end

function systemNotifications:show()
	self.container:FadeIn(NOTIFICATIONS_TRANSITION_IN_TIME)
	self.container:SlideY('2.7h', NOTIFICATIONS_TRANSITION_IN_TIME)
	self.slideouts:FadeOut(NOTIFICATIONS_TRANSITION_OUT_TIME, function()
		for k,v in ipairs(self.slideouts:GetChildren()) do
			v:SlideX('100%', NOTIFICATIONS_TRANSITION_OUT_TIME)
			v:Sleep(NOTIFICATIONS_TRANSITION_OUT_TIME, function()
				v:Destroy()
			end)
		end
		self.slideoutWidgetsActive = {}
		self.slideoutQueue = {}
	end)
end

function systemNotifications:hide()
	self.container:SlideY('-20h', NOTIFICATIONS_TRANSITION_OUT_TIME)
	self.container:FadeOut(NOTIFICATIONS_TRANSITION_OUT_TIME)
	self.slideouts:SetVisible(true)
end

function systemNotifications:processParam(paramString)
	--[[
	if paramString == '[PLAYERNAME]' then
		paramString = GetAccountName()
	else
		paramString = Translate(paramString)
	end
	--]]
	return paramString
end

function systemNotifications:ProcessInvisibleNotification(notificationInfo)
	-- when get certain messages, client need to get latest rap info to change UI, and these messages can't be seen by player
	if notificationInfo.subject and notificationInfo.subject == 'REPORT_FROM_WARNING_TO_NORMAL_SUBJECT'then
		UpdateRapStatus(false)
		DeleteMessage(notificationInfo.crc)
		return false
	end
	return true
end

function systemNotifications:rearrangeSlideouts()
	local posCur		= 0
	local posOffset		= 0		-- (GetScreenHeight() * 0.005)
	for index, messageIndex in ipairs(self.slideoutWidgetsActive) do
		local slideout = GetWidget('systemNotificationsSlideout'..messageIndex, 'system_notifications')
		if slideout:IsVisible() then
			slideout:SlideY(posCur, NOTIFICATIONS_TRANSITION_MOVE_TIME)
		else
			slideout:SetY(posCur)
		end
		
		posCur = posCur + posOffset + slideout:GetHeight()
	end
end

function systemNotifications:removeSlideout(messageIndex, isInstant, fromPopup)
	isInstant = isInstant or false
	fromPopup = fromPopup or false
	
	local notificationWidget

	for index,activeID in ipairs(self.slideoutWidgetsActive) do
		if activeID == messageIndex then
			notificationWidget = GetWidget('systemNotificationsSlideout'..messageIndex, 'system_notifications')
			table.remove(self.slideoutWidgetsActive, index)
			break
		end
	end

	if notificationWidget then
		if fromPopup then
			notificationWidget:SetNoClick(true)
			notificationWidget:SetPassiveChildren(true)
		end
		if isInstant then
			notificationWidget:Destroy()
			self:rearrangeSlideouts()
		else
			notificationWidget:SlideX('100%', NOTIFICATIONS_TRANSITION_OUT_TIME)
			notificationWidget:Sleep(NOTIFICATIONS_TRANSITION_OUT_TIME, function()
				notificationWidget:Destroy()
				self:evaluateSlideoutQueue()
			end)
		end
	end
end

function systemNotifications:clearDeletedMessage(notificationInfo)	-- General cleanup on a deleted message (remove from cache, get rid of slideout, and remove list entry)
	if notificationInfo and type(notificationInfo) == 'table' then
		local accountNotificationCache = self:checkCreateAccountCache()

		if not notificationInfo.isTest then
			if accountNotificationCache[notificationInfo.crc] then
				tableMerge(accountNotificationCache[notificationInfo.crc], notificationInfo)
				accountNotificationCache[notificationInfo.crc] = nil
			end
		end

		systemNotifications.activeEntries[notificationInfo.crc] = nil

		GetWidget('systemNotificationsBody', 'system_notifications'):EraseListItemByValue(notificationInfo.messageIndex)

		self:removeSlideout(notificationInfo, nil, true)
		self:updateMessageCount()
		GetDBEntry('systemNotificationsCache', self.cache, true, false, false)
	end
end

function systemNotifications:checkCreateAccountCache()
	self.cache = self.cache or {}
	local accountID = Client.GetAccountID() 

	self.cache[accountID] = self.cache[accountID] or {}
	return self.cache[accountID]
end

-- local systemBarStatus = TriggerManager:GetTrigger('systemBarStatus')
function systemNotifications:SetDefaultMessage( notificationInfo )
	local title				= GetWidget('systemNotificationsPopupTitle', 'main')
	local subtitle			= GetWidget('systemNotificationsPopupSubtitle', 'main')
	local icon				= GetWidget('systemNotificationsPopupIcon', 'main')
	local image				= GetWidget('systemNotificationsPopupImage', 'main')
	local imageContainer	= GetWidget('systemNotificationsPopupImageContainer', 'main')
	local bodyTitle			= GetWidget('systemNotificationsPopupBodyTitle', 'main')
	local body				= GetWidget('systemNotificationsPopupBody', 'main')
	local footer			= GetWidget('systemNotificationsFooter', 'main')
	local footerLabel		= GetWidget('systemNotificationsFooterLabel', 'main')

	local iconPath = '/ui/elements/empty_pack.tga'

	if not Empty(notificationInfo.icon) then
		iconPath = notificationInfo.icon
	end

	setTextureCheckForWeb(icon, iconPath)

	local paramTable

	if notificationInfo.meta and type(notificationInfo.meta) == 'table' then
		paramTable = self:buildParamTable(notificationInfo)

		if not Empty(notificationInfo.meta.subtitle) then
			subtitle:SetText(self:processStringParams(notificationInfo.meta.subtitle, paramTable))
			subtitle:SetVisible(true)
		else
			subtitle:SetVisible(false)
		end

		if not Empty(notificationInfo.meta.bodyTitle) then
			bodyTitle:SetText(self:processStringParams(notificationInfo.meta.bodyTitle, paramTable))
			bodyTitle:SetVisible(true)
		else
			bodyTitle:SetVisible(false)
		end

		if not Empty(notificationInfo.meta.image) then
			imageContainer:SetVisible(true)

			if notificationInfo.meta.imageHeight then
				image:SetHeight(notificationInfo.meta.imageHeight)
			else
				image:SetHeight('30h')
			end

			if notificationInfo.meta.imageWidth then
				image:SetWidth(notificationInfo.meta.imageWidth)
			else
				image:SetWidth('30h')
			end

			setTextureCheckForWeb(image, notificationInfo.meta.image)
		else
			imageContainer:SetVisible(false)
		end

		if not Empty(notificationInfo.meta.body) and notificationInfo.meta.body ~= 'general_product_type_' and notificationInfo.meta.body ~= 'general_product_type_false' then
			if paramTable then
				body:SetText(self:processStringParams(notificationInfo.meta.body, paramTable))
			else
				body:SetText(Translate(notificationInfo.meta.body))
			end
			body:SetVisible(true)
		else
			body:SetVisible(false)
		end


		if not Empty(notificationInfo.meta.footer) then
			footer:SetVisible(true)
			footerLabel:SetText(self:processStringParams(notificationInfo.meta.footer, paramTable))
		else
			footer:SetVisible(false)
		end
	else
		body:SetVisible(false)
		subtitle:SetVisible(false)
		bodyTitle:SetVisible(false)
		imageContainer:SetVisible(false)
	end

	if Empty(notificationInfo.icon) then
		icon:SetTexture('/ui/elements/empty_pack.tga')
	else
		icon:SetTexture(notificationInfo.icon)
	end

	if not Empty(notificationInfo.subject) then
		title:SetText(Translate(notificationInfo.subject))
	else
		title:SetText('---')
	end
	title:SetVisible(true)
end


function systemNotifications:SetBetMessage(notificationInfo)
	local title				= GetWidget('systemNotificationsPopupTitle', 'main')
	local subtitle			= GetWidget('systemNotificationsPopupSubtitle', 'main')
	local icon				= GetWidget('systemNotificationsPopupIcon', 'main')
	local image				= GetWidget('systemNotificationsPopupImage', 'main')
	local imageContainer	= GetWidget('systemNotificationsPopupImageContainer', 'main')
	local bodyTitle			= GetWidget('systemNotificationsPopupBodyTitle', 'main')
	local body				= GetWidget('systemNotificationsPopupBody', 'main')
	local footer			= GetWidget('systemNotificationsFooter', 'main')
	local footerLabel		= GetWidget('systemNotificationsFooterLabel', 'main')

	subtitle:SetVisible(false)
	imageContainer:SetVisible(false)
	bodyTitle:SetVisible(false)
	footer:SetVisible(false)

	icon:SetTexture('/ui/icons/message_bet.tga')
	title:SetText(Translate(notificationInfo.subject))

	Echo(notificationInfo.meta.betting_mode)
	if notificationInfo.meta.betting_mode and notificationInfo.meta.type == NOTIFICATIONS_TYPE_BET_SUCCESS then
		body:SetVisible(true)
		body:SetText(Translate('system_message_bet_successful', 'bettype', Translate('system_message_bettype_'..notificationInfo.meta.betting_mode), 'matchname', notificationInfo.meta.match_name, 'coinnum', notificationInfo.meta.reward))
	elseif notificationInfo.meta.betting_mode and notificationInfo.meta.type == NOTIFICATIONS_TYPE_BET_CANCEL then
		body:SetText(Translate('system_message_bet_cancel', 'bettype', Translate('system_message_bettype_'..notificationInfo.meta.betting_mode), 'matchname', notificationInfo.meta.match_name, 'coinnum', notificationInfo.meta.coin))
		body:SetVisible(true)
	end
end

function systemNotifications:openMessage(openedNotification)
	-- systemBarStatus.systemNotificationsBusy		= false


	if openedNotification and type(openedNotification) == 'table' then
		local notificationInfo

		if openedNotification.isTest or openedNotification.wasOpened then
			notificationInfo = openedNotification
		else
			local accountNotificationCache = self:checkCreateAccountCache()
			notificationInfo = accountNotificationCache[openedNotification.crc]
			tableMerge(openedNotification, notificationInfo)
		end

		notificationInfo.wasOpened = true

		self:removeSlideout(notificationInfo.messageIndex, nil, true)

		if (notificationInfo.meta.type) and (notificationInfo.meta.type == NOTIFICATIONS_TYPE_BET_SUCCESS or notificationInfo.meta.type == NOTIFICATIONS_TYPE_BET_CANCEL) then
			self:SetBetMessage(notificationInfo)
		elseif notificationInfo.meta.type and notificationInfo.meta.type == NOTIFICATIONS_TYPE_SUBSCRIBED_MATCH_BEGIN then
			self:SetSubscribedMatchBeginMessage(notificationInfo)
		elseif notificationInfo.meta.type and notificationInfo.meta.type == NOTIFICATIONS_TYPE_BETTED_MATCH_BEGIN then
			self:SetBettedMatchBeginMessage(notificationInfo)
		elseif notificationInfo.meta.type and notificationInfo.meta.type == NOTIFICATIONS_TYPE_POINTS_CHANGED then
			self:SetPointsChangedMessage(notificationInfo)
		elseif notificationInfo.meta.type and notificationInfo.meta.type == NOTIFICATIONS_TYPE_RAP then
			self:SetRAPMessage(notificationInfo)
		else
			self:SetDefaultMessage(notificationInfo)
		end

		GetWidget('systemNotificationsPopupAction', 'main'):SetCallback('onclick', function(widget) self:popupClose(notificationInfo) end)

		GetWidget('systemNotificationsPopupClosex'):SetCallback('onclick', function(widget)
			self:popupClose(notificationInfo)
		end)

		GetWidget('systemNotificationsPopup', 'main'):FadeIn(250)
	else
		print('Empty/invalid notification details. --  ('..tostring(notificationInfo)..')\n')
	end
end

function systemNotifications:buildParamTable(notificationInfo)
	local paramTable = {}
	local tempParam
	for k,v in ipairs({'amount', 'productId', 'productType', 'gifterNick', 'accountId'}) do
		tempParam = notificationInfo.meta[v]
		if not Empty(tempParam) then
			-- tempParam = self:processParam(tempParam)

			table.insert(paramTable, v)
			table.insert(paramTable, tempParam)

			if (
				notificationInfo.subject == 'sysmessage_product_gift' or
				notificationInfo.subject == 'sysmessage_product_removed' or
				notificationInfo.subject == 'sysmessage_product_system'
			) then
				if v == 'productId' then
					if Empty(notificationInfo.meta.bodyTitle) then
						notificationInfo.meta.bodyTitle = Translate('mstore_product'..tempParam..'_name')
					end
					table.insert(paramTable, 'productName')
					table.insert(paramTable, 'mstore_product'..tempParam..'_name')
				elseif v == 'productType' then
					if Empty(notificationInfo.meta.body) then
						notificationInfo.meta.body = Translate('general_product_type_'..tempParam)
					end
					table.insert(paramTable, 'productType')
					table.insert(paramTable, 'general_product_type_'..tempParam)
				end

			end
		end
	end

	return paramTable
end

function systemNotifications:processStringParams(stringKey, paramTable)
	stringKey = string.gsub(stringKey, '%[PLAYERNAME%]', GetAccountName())
	return Translate(stringKey, unpack(paramTable))
end

function systemNotifications:setupOpenButton(buttonWidget, notificationInfo)
	buttonWidget:SetCallback('onclick', function(widget)
		-- local systemBarStatus = TriggerManager:GetTrigger('systemBarStatus')
		if notificationInfo.isTest or notificationInfo.wasOpened then
			self:openMessage(notificationInfo)
		else
			-- systemBarStatus.systemNotificationsBusy		= true
			buttonWidget:SetEnabled(false)
			GetMessageDetails(notificationInfo.crc, function(...)
				buttonWidget:SetEnabled(true)
				self:openMessage(...)
			end)
		end
	end)



	--[[
	local buttonWatch = TriggerManager:CreateWatch('systemBarStatus', function(triggerData)
		buttonWidget:SetEnabled(not triggerData.systemNotificationsBusy)
	end, 'systemNotificationsBusy')

	buttonWidget:SetCallback('onkill', function(widget)
		buttonWatch:SetCallback(nil)
	end)
	--]]
end

function systemNotifications:slideoutShow(notificationInfo, skipRearrange)
	skipRearrange = skipRearrange or false
	local messageIndex = notificationInfo.messageIndex
	self.slideouts:Instantiate(
		'systemNotificationsEntry',
		'id', notificationInfo.messageIndex,
		'showclose', 'true',
		'actionvalign', 'bottom',
		'actiony', '-1h',
		'prefix', 'systemNotificationsSlideout',
		'visible', 'false',
		'title', Translate(notificationInfo.subject or '')
	)

	self:setupOpenButton(
		GetWidget('systemNotificationsSlideout'..messageIndex..'Action', 'system_notifications'),
		notificationInfo
	)

	local iconPath = notificationInfo.icon
	
	if notificationInfo.subject and (notificationInfo.subject == 'BET_WIN_SUBJECT' or notificationInfo.subject == 'BET_CANCELLED_SUBJECT') then
		iconPath = '/ui/icons/message_bet.tga'
	elseif notificationInfo.subject and notificationInfo.subject == 'SUBSCRIBED_MATCH_BEGIN_SUBJECT' then
		iconPath = '/ui/icons/message_subscribe.tga'
	elseif notificationInfo.subject and notificationInfo.subject == 'BETTED_MATCH_BEGIN_SUBJECT' then
		iconPath = '/ui/icons/message_match.tga'
	elseif Empty(iconPath) then
		iconPath = '/ui/elements/empty_pack.tga'
	end

	setTextureCheckForWeb(GetWidget('systemNotificationsSlideout'..notificationInfo.messageIndex..'Icon', 'system_notifications'), iconPath)

	local notificationWidget	= GetWidget('systemNotificationsSlideout'..notificationInfo.messageIndex, 'system_notifications')
	table.insert(self.slideoutWidgetsActive, notificationInfo.messageIndex)

	if not skipRearrange then
		self:rearrangeSlideouts()
	end

	GetWidget('systemNotificationsSlideout'..notificationInfo.messageIndex..'Close', 'system_notifications'):SetCallback('onclick', function(widget)
		self:removeSlideout(messageIndex)
	end)
	
	notificationWidget:SetX('100%')
	notificationWidget:SlideX(0, NOTIFICATIONS_TRANSITION_IN_TIME)
	notificationWidget:FadeIn(NOTIFICATIONS_TRANSITION_IN_TIME)
	notificationWidget:Sleep(NOTIFICATIONS_DISPLAY_TIME, function()
		self:removeSlideout(messageIndex)
	end)
end

function systemNotifications:updateMessageCount()
	-- local systemBarStatus			= TriggerManager:GetTrigger('systemBarStatus')
	local newMessageCount			= 0

	for k,v in pairs(systemNotifications.activeEntries) do
		newMessageCount = newMessageCount + 1
	end
	-- systemBarStatus.systemNotificationsCount = newMessageCount
	Trigger('systemNotificationsCount', newMessageCount)

end

function systemNotifications:evaluateSlideoutQueue()
	local queuedNotification
	local slideCount = math.min(NOTIFICATIONS_SLIDEOUTS_MAX - #self.slideoutWidgetsActive, #self.slideoutQueue)
	if slideCount > 0 then
		for i=1,slideCount,1 do
			queuedNotification = table.remove(self.slideoutQueue, 1)
			self:slideoutShow(queuedNotification, true)
		end
	end
	self:rearrangeSlideouts()
end

function systemNotifications:queueSlideout(notificationInfo)
	table.insert(self.slideoutQueue, notificationInfo)
end

function systemNotifications:slideoutEntry(notificationInfo)
	if #self.slideoutWidgetsActive >= NOTIFICATIONS_SLIDEOUTS_MAX then
		self:queueSlideout(notificationInfo)
	else
		self:slideoutShow(notificationInfo)
	end
end

function systemNotifications:popupClose(notificationInfo)
	GetWidget('systemNotificationsPopup', 'main'):FadeOut(250)

	if notificationInfo and type(notificationInfo) == 'table' then
		
		if notificationInfo.isTest then
			self.deleteNotificationInfo = notificationInfo
			self:clearDeletedMessage(notificationInfo)
		elseif notificationInfo.deletable then
			self.deleteNotificationInfo = notificationInfo
			DeleteMessage(notificationInfo.crc)
		end
	end
end

function systemNotifications:insertEntry(notificationInfo)
	GetWidget('systemNotificationsBody', 'system_notifications'):AddTemplateListItem(
		'systemNotificationsEntry', notificationInfo.messageIndex,
		'id', notificationInfo.messageIndex,
		'title', Translate(notificationInfo.subject or '')
	)

	self:setupOpenButton(
		GetWidget('systemNotificationsEntry'..notificationInfo.messageIndex..'Action', 'system_notifications'),
		notificationInfo
	)

	local iconPath = notificationInfo.icon

	if notificationInfo.subject and (notificationInfo.subject == 'BET_WIN_SUBJECT' or notificationInfo.subject == 'BET_CANCELLED_SUBJECT') then
		iconPath = '/ui/icons/message_bet.tga'
	elseif notificationInfo.subject and notificationInfo.subject == 'SUBSCRIBED_MATCH_BEGIN_SUBJECT' then
		iconPath = '/ui/icons/message_subscribe.tga'
	elseif notificationInfo.subject and notificationInfo.subject == 'BETTED_MATCH_BEGIN_SUBJECT' then
		iconPath = '/ui/icons/message_match.tga'
	elseif Empty(iconPath) then
		iconPath = '/ui/elements/empty_pack.tga'
	end	

	if notificationInfo.subject and notificationInfo.subject == 'POINTS_CHANGED_SUBJECT' then
		SubmitForm('PointsChanged', 'f', 'get_points', 'cookie', Client.GetCookie());
	end

	-- when get certain messages, client need to get latest rap info to change UI
	if notificationInfo.subject and (notificationInfo.subject == 'REPORT_SUSPEND_SUBJECT' or notificationInfo.subject == 'REPORT_WARNING_SUBJECT')then
		local updated = notificationInfo.updated or false
		if not updated then
			UpdateRapStatus(true)
			notificationInfo.updated = true
		end
	end

	setTextureCheckForWeb(GetWidget('systemNotificationsEntry'..notificationInfo.messageIndex..'Icon', 'system_notifications'), iconPath)

	self.activeEntries[notificationInfo.crc] = notificationInfo.messageIndex
end

function systemNotifications:newMessage(notificationInfo, skipSlide, updateList)
	if self:ProcessInvisibleNotification(notificationInfo) then
		skipSlide	= skipSlide or false
		updateList	= updateList or false
		self.messageIndex = self.messageIndex + 1
		notificationInfo.messageIndex = self.messageIndex
		self:insertEntry(notificationInfo)

		if (not skipSlide) and (not self.container:IsVisible()) then
			self:slideoutEntry(notificationInfo)
		end
	
		if updateList then
			self:updateMessageCount()
		end
	end
end

function systemNotifications:initialize()
	self.container			= GetWidget('systemNotifications', 'system_notifications')
	self.slideouts			= GetWidget('systemNotificationsSlideouts', 'system_notifications')

	self.container:RegisterWatch('MessageDeleted', function(widget, crc, deleteSuccess)
		if AtoB(deleteSuccess) then
			self:clearDeletedMessage(self.deleteNotificationInfo)
			self.deleteNotificationInfo = nil
		else
			print('Failed to delete message.\n')
		end
	end)

	self.container:RegisterWatch('MessagesUpdated', function(widget, listUpdated)
		if AtoB(listUpdated) then
			local newList	= GetMessages()
			local accountNotificationCache = self:checkCreateAccountCache()

			local newMessagesByCRC = {}


			for k,newMessage in ipairs(newList) do	-- mostly to build a list by CRC
				newMessagesByCRC[newMessage.crc] = newMessage

				if (not accountNotificationCache[newMessage.crc]) then
					accountNotificationCache[newMessage.crc] = newMessage
				end
			end
			for k,v in pairs(accountNotificationCache) do	-- Have to resolve messages that were removed elsewhere/message list being flushed.
				if newMessagesByCRC[v.crc] then
					if not v.displayed then
						self:newMessage(v)
						v.displayed = true
					end
				else
					accountNotificationCache[k] = nil
				end
			end
			self:updateMessageCount()
			GetDBEntry('systemNotificationsCache', self.cache, true, false, false)
		end
	end)

	self.container:RegisterWatch('LoginStatus', function(widget, _, _, isLoggedIn)
		if AtoB(isLoggedIn) then
			GetWidget('systemNotificationsBody', 'system_notifications'):ClearItems()
			local accountNotificationCache = self:checkCreateAccountCache()

			for k,v in pairs(accountNotificationCache) do
				v.displayed = true
				self:newMessage(v, true)
			end
			GetDBEntry('systemNotificationsCache', self.cache, true, false, false)
		else
			Trigger('systemBarView', SYSBAR_VIEW_NONE)
			sysbarLastSection = SYSBAR_VIEW_NONE
		end
	end)

	--[[
	TriggerManager:CreateWatch('systemBarStatus', function(triggerData)
		local messageCount = triggerData.systemNotificationsCount
		for k,v in ipairs(GetWidget('sysbar_system_notifications'):GetGroup('sysbar_system_notifications_button_state_label')) do
			v:SetText(messageCount)
		end

		GetWidget('systemNotificationsEmpty'):SetVisible(messageCount <= 0)
	end, 'systemNotificationsCount')
	--]]

	GetWidget('sysbar_system_notifications'):RegisterWatch('systemNotificationsCount', function(widget, notificationCount)
		local messageCount = AtoN(notificationCount)
		for k,v in ipairs(GetWidget('sysbar_system_notifications'):GetGroup('sysbar_system_notifications_button_state_label')) do
			v:SetText(messageCount)
		end

		GetWidget('systemNotificationsEmpty'):SetVisible(messageCount <= 0)
	end)


	--[[
	GetWidget('sysbar_system_notifications_button'):SetCallback('onmouseover', function(widget)
		Trigger('genericMainFloatingTip', 'true', '24.0h', '', 'sysmessage_tip', 'sysmessage_tip_body', '', '', '22', '-22')
	end)

	GetWidget('sysbar_system_notifications_button'):SetCallback('onmouseout', function(widget)
		Trigger('genericMainFloatingTip', 'false', '', '', '', '', '', '', '', '')
	end)
	--]]
end

if HoN_Region and HoN_Region.regionTable[HoN_Region.activeRegion].globalMessaging then
	systemNotifications:initialize()
	GetWidget('sysbar_system_notifications_container'):SetVisible(true)
end

-- ================================================================================
-- ================ System bar / social panel visibility stuff ====================
-- ================================================================================

local function sysbarClickToggleSection(sectionID)
	PlaySound('/shared/sounds/ui/button_click_01.wav')
	PlaySound('/shared/sounds/ui/menu/systembar_slide.wav')
	local newView

	if sectionID == sysbarLastSection then
		if sectionID == SYSBAR_VIEW_FRIEND_OPTIONS then
			newView = SYSBAR_VIEW_FRIENDS
			sectionID = SYSBAR_VIEW_FRIENDS
		else
			newView = SYSBAR_VIEW_NONE
			sectionID = SYSBAR_VIEW_NONE
		end
	else
		newView = sectionID
	end

	-- systemBarStatus.view = newView
	Trigger('systemBarView', newView)

	sysbarLastSection = sectionID
end

GetWidget('sysbar_system_notifications_button'):SetCallback('onclick', function(widget)
	sysbarClickToggleSection(SYSBAR_VIEW_SYS_NOTIFICATIONS)
end)

GetWidget('sysbar_friendsonline_button'):SetCallback('onclick', function(widget)
	sysbarClickToggleSection(SYSBAR_VIEW_FRIENDS)
end)

GetWidget('sysbar_notifications_btn'):SetCallback('onclick', function(widget)
	sysbarClickToggleSection(SYSBAR_VIEW_NOTIFICATIONS)
end)

GetWidget('social_friends_options_btn'):SetCallback('onclick', function(widget)
	sysbarClickToggleSection(SYSBAR_VIEW_FRIEND_OPTIONS)
end)


-- SocialGroupVisiblePanel trigger stuff is legacy / only for widgetstate trickery

local function systemBarView(currentView)
	if currentView == SYSBAR_VIEW_SYS_NOTIFICATIONS then
		if HoN_Region and HoN_Region.regionTable[HoN_Region.activeRegion].globalMessaging then
			systemNotifications:show()
		end
		Trigger('SocialGroupVisiblePanel', 'system_notifications')
	else
		if HoN_Region and HoN_Region.regionTable[HoN_Region.activeRegion].globalMessaging then
			systemNotifications:hide()
		end
	end

	if currentView == SYSBAR_VIEW_NONE then
		Trigger('SocialGroupVisiblePanel', '')
	end

	if currentView >= SYSBAR_VIEW_FRIENDS then		-- social_panel

		GetWidget('social_panel'):SlideY('2.6h', 150)
		GetWidget('social_panel'):Sleep(150, function()
			GetWidget('social_panel'):SetY('2.6h')
		end)
		GetWidget('social_panel'):SetVisible(true)

		GetWidget('social_friends_panel'):SetVisible(currentView == SYSBAR_VIEW_FRIENDS)
		GetWidget('social_notifications_panel'):SetVisible(currentView == SYSBAR_VIEW_NOTIFICATIONS)
		GetWidget('social_notifications_scroller'):SetVisible(currentView == SYSBAR_VIEW_NOTIFICATIONS)
		GetWidget('sp_user_list_scroller'):SetVisible(currentView == SYSBAR_VIEW_FRIENDS)
		GetWidget('social_friends_options'):SetVisible(currentView == SYSBAR_VIEW_FRIEND_OPTIONS)

		if currentView == SYSBAR_VIEW_NOTIFICATIONS then
			GetWidget('sp_friends_header_label'):SetText(Translate('options_notifications_title'))

			GetWidget('social_panel'):ScaleWidth('26h', 150)
			GetWidget('notification_toast_main'):SlideX('-26.5h', 150)

			Trigger('SocialGroupVisiblePanel', 'notification_history')
		elseif currentView == SYSBAR_VIEW_FRIENDS or currentView == SYSBAR_VIEW_FRIEND_OPTIONS then
			GetWidget('sp_friends_header_label'):SetText(Translate('mainlobby_label_cc_friends'))

			GetWidget('social_panel'):ScaleWidth('23h', 150)
			GetWidget('notification_toast_main'):SlideX('-23.2h', 150)

			Trigger('SocialGroupVisiblePanel', 'social_panel')
		end
	else
		GetWidget('social_panel'):SlideY('-55h', 300)
		GetWidget('social_panel'):Sleep(300, function()
			GetWidget('social_panel'):SetY('-55h')
			GetWidget('social_panel'):SetVisible(0)
		end)

		GetWidget('notification_toast_main'):SlideX('-0.5h', 150)
		GetWidget('sp_footer_bot'):SetVisible(false)

		GetWidget('sp_left_user_menu'):DoEventN(1)
	end
end

GetWidget('social_panel'):RegisterWatch('systemBarView', function(widget, newView)
	systemBarView(AtoN(newView))
end)

--[[
TriggerManager:CreateWatch('systemBarStatus', function(triggerData)
	systemBarView(triggerData.view)
end, 'view')
--]]

function systemNotifications:SetSubscribedMatchBeginMessage( notificationInfo )
	local title				= GetWidget('systemNotificationsPopupTitle', 'main')
	local subtitle			= GetWidget('systemNotificationsPopupSubtitle', 'main')
	local icon				= GetWidget('systemNotificationsPopupIcon', 'main')
	local image				= GetWidget('systemNotificationsPopupImage', 'main')
	local imageContainer	= GetWidget('systemNotificationsPopupImageContainer', 'main')
	local bodyTitle			= GetWidget('systemNotificationsPopupBodyTitle', 'main')
	local body				= GetWidget('systemNotificationsPopupBody', 'main')
	local footer			= GetWidget('systemNotificationsFooter', 'main')
	local footerLabel		= GetWidget('systemNotificationsFooterLabel', 'main')
	local iconPath = '/ui/icons/message_subscribe.tga'

	if not Empty(notificationInfo.icon) then
		iconPath = notificationInfo.icon
	end

	setTextureCheckForWeb(icon, iconPath)

	local paramTable

	if notificationInfo.meta and type(notificationInfo.meta) == 'table' then
		paramTable = self:buildParamTable(notificationInfo)

		if not Empty(notificationInfo.meta.title) then
			title:SetText(self:processStringParams(notificationInfo.meta.title, paramTable))
			title:SetVisible(true)
		else
			title:SetVisible(false)
		end

		if not Empty(notificationInfo.meta.subtitle) then
			subtitle:SetText(self:processStringParams(notificationInfo.meta.subtitle, paramTable))
			subtitle:SetVisible(true)
		else
			subtitle:SetVisible(false)
		end

		if not Empty(notificationInfo.meta.bodyTitle) then
			bodyTitle:SetText(self:processStringParams(notificationInfo.meta.bodyTitle, paramTable))
			bodyTitle:SetVisible(true)
		else
			bodyTitle:SetVisible(false)
		end

		if not Empty(notificationInfo.meta.image) then
			imageContainer:SetVisible(true)

			if notificationInfo.meta.imageHeight then
				image:SetHeight(notificationInfo.meta.imageHeight)
			else
				image:SetHeight('30h')
			end

			if notificationInfo.meta.imageWidth then
				image:SetWidth(notificationInfo.meta.imageWidth)
			else
				image:SetWidth('30h')
			end

			setTextureCheckForWeb(image, notificationInfo.meta.image)
		else
			imageContainer:SetVisible(false)
		end

		--[[
		if not Empty(notificationInfo.meta.body) then
			if paramTable then
				body:SetText(self:processStringParams(notificationInfo.meta.body, paramTable))
			else
				body:SetText(Translate('subscribed_match_begin', 'team', 'notificationInfo.meta.team_name'))
			end
		
			body:SetVisible(true)
		else
			body:SetVisible(false)
		end
		--]]
		body:SetText(Translate('subscribed_match_begin', 'clan', notificationInfo.meta.clan_name, 'match', notificationInfo.meta.match_name))
		body:SetVisible(true)

		if not Empty(notificationInfo.meta.footer) then
			footer:SetVisible(true)
			footerLabel:SetText(self:processStringParams(notificationInfo.meta.footer, paramTable))
		else
			footer:SetVisible(false)
		end
	else
		body:SetVisible(false)
		subtitle:SetVisible(false)
		bodyTitle:SetVisible(false)
		imageContainer:SetVisible(false)
	end

	--[[
	if Empty(notificationInfo.icon) then
		icon:SetTexture('/ui/elements/empty_pack.tga')
	else
		icon:SetTexture(notificationInfo.icon)
	end
	--]]
	icon:SetTexture("/ui/icons/message_subscribe.tga")

	if not Empty(notificationInfo.subject) then
		title:SetText(Translate(notificationInfo.subject))
	else
		title:SetText('---')
	end
		title:SetVisible(true);

end

function systemNotifications:SetBettedMatchBeginMessage( notificationInfo )
	local title				= GetWidget('systemNotificationsPopupTitle', 'main')
	local subtitle			= GetWidget('systemNotificationsPopupSubtitle', 'main')
	local icon				= GetWidget('systemNotificationsPopupIcon', 'main')
	local image				= GetWidget('systemNotificationsPopupImage', 'main')
	local imageContainer	= GetWidget('systemNotificationsPopupImageContainer', 'main')
	local bodyTitle			= GetWidget('systemNotificationsPopupBodyTitle', 'main')
	local body				= GetWidget('systemNotificationsPopupBody', 'main')
	local footer			= GetWidget('systemNotificationsFooter', 'main')
	local footerLabel		= GetWidget('systemNotificationsFooterLabel', 'main')
	local iconPath = '/ui/icons/message_match.tga'

	if not Empty(notificationInfo.icon) then
		iconPath = notificationInfo.icon
	end

	setTextureCheckForWeb(icon, iconPath)

	local paramTable

	if notificationInfo.meta and type(notificationInfo.meta) == 'table' then
		paramTable = self:buildParamTable(notificationInfo)

		if not Empty(notificationInfo.meta.title) then
			title:SetText(self:processStringParams(notificationInfo.meta.title, paramTable))
			title:SetVisible(true)
		else
			title:SetVisible(false)
		end

		if not Empty(notificationInfo.meta.subtitle) then
			subtitle:SetText(self:processStringParams(notificationInfo.meta.subtitle, paramTable))
			subtitle:SetVisible(true)
		else
			subtitle:SetVisible(false)
		end

		if not Empty(notificationInfo.meta.bodyTitle) then
			bodyTitle:SetText(self:processStringParams(notificationInfo.meta.bodyTitle, paramTable))
			bodyTitle:SetVisible(true)
		else
			bodyTitle:SetVisible(false)
		end

		if not Empty(notificationInfo.meta.image) then
			imageContainer:SetVisible(true)

			if notificationInfo.meta.imageHeight then
				image:SetHeight(notificationInfo.meta.imageHeight)
			else
				image:SetHeight('30h')
			end

			if notificationInfo.meta.imageWidth then
				image:SetWidth(notificationInfo.meta.imageWidth)
			else
				image:SetWidth('30h')
			end

			setTextureCheckForWeb(image, notificationInfo.meta.image)
		else
			imageContainer:SetVisible(false)
		end

		--[[
		if not Empty(notificationInfo.meta.body) then
			if paramTable then
				body:SetText(self:processStringParams(notificationInfo.meta.body, paramTable))
			else
				body:SetText(Translate('betted_match_begin', 'match', notificationInfo.meta.match_name))
			end
		
			body:SetVisible(true)
		else
			body:SetVisible(false)
		end
		--]]
		body:SetText(Translate('betted_match_begin', 'match', notificationInfo.meta.match_name))
		body:SetVisible(true)


		if not Empty(notificationInfo.meta.footer) then
			footer:SetVisible(true)
			footerLabel:SetText(self:processStringParams(notificationInfo.meta.footer, paramTable))
		else
			footer:SetVisible(false)
		end
	else
		body:SetVisible(false)
		subtitle:SetVisible(false)
		bodyTitle:SetVisible(false)
		imageContainer:SetVisible(false)
	end

	--[[
	if Empty(notificationInfo.icon) then
		icon:SetTexture('/ui/elements/empty_pack.tga')
	else
		icon:SetTexture(notificationInfo.icon)
	end
	--]]

	icon:SetTexture("/ui/icons/message_match.tga")

	if not Empty(notificationInfo.subject) then
		title:SetText(Translate(notificationInfo.subject))
	else
		title:SetText('---')
	end
		title:SetVisible(true);

end

function systemNotifications:SetPointsChangedMessage( notificationInfo )
	local title				= GetWidget('systemNotificationsPopupTitle', 'main')
	local subtitle			= GetWidget('systemNotificationsPopupSubtitle', 'main')
	local icon				= GetWidget('systemNotificationsPopupIcon', 'main')
	local image				= GetWidget('systemNotificationsPopupImage', 'main')
	local imageContainer	= GetWidget('systemNotificationsPopupImageContainer', 'main')
	local bodyTitle			= GetWidget('systemNotificationsPopupBodyTitle', 'main')
	local body				= GetWidget('systemNotificationsPopupBody', 'main')
	local footer			= GetWidget('systemNotificationsFooter', 'main')
	local footerLabel		= GetWidget('systemNotificationsFooterLabel', 'main')
	local iconPath = '/ui/elements/empty_pack.tga'

	if not Empty(notificationInfo.icon) then
		iconPath = notificationInfo.icon
	end

	setTextureCheckForWeb(icon, iconPath)

	local paramTable

	if notificationInfo.meta and type(notificationInfo.meta) == 'table' then
		paramTable = self:buildParamTable(notificationInfo)

		if not Empty(notificationInfo.meta.title) then
			title:SetText(self:processStringParams(notificationInfo.meta.title, paramTable))
			title:SetVisible(true)
		else
			title:SetVisible(false)
		end

		if not Empty(notificationInfo.meta.subtitle) then
			subtitle:SetText(self:processStringParams(notificationInfo.meta.subtitle, paramTable))
			subtitle:SetVisible(true)
		else
			subtitle:SetVisible(false)
		end

		if not Empty(notificationInfo.meta.bodyTitle) then
			bodyTitle:SetText(self:processStringParams(notificationInfo.meta.bodyTitle, paramTable))
			bodyTitle:SetVisible(true)
		else
			bodyTitle:SetVisible(false)
		end

		if not Empty(notificationInfo.meta.image) then
			imageContainer:SetVisible(true)

			if notificationInfo.meta.imageHeight then
				image:SetHeight(notificationInfo.meta.imageHeight)
			else
				image:SetHeight('30h')
			end

			if notificationInfo.meta.imageWidth then
				image:SetWidth(notificationInfo.meta.imageWidth)
			else
				image:SetWidth('30h')
			end

			setTextureCheckForWeb(image, notificationInfo.meta.image)
		else
			imageContainer:SetVisible(false)
		end

		body:SetText(Translate('points_changed', 'points', notificationInfo.meta.points, 'channel', Translate(notificationInfo.meta.channel )))
		body:SetVisible(true)


		if not Empty(notificationInfo.meta.footer) then
			footer:SetVisible(true)
			footerLabel:SetText(self:processStringParams(notificationInfo.meta.footer, paramTable))
		else
			footer:SetVisible(false)
		end
	else
		body:SetVisible(false)
		subtitle:SetVisible(false)
		bodyTitle:SetVisible(false)
		imageContainer:SetVisible(false)
	end

	icon:SetTexture("/ui/elements/empty_pack.tga")

	if not Empty(notificationInfo.subject) then
		title:SetText(Translate(notificationInfo.subject))
	else
		title:SetText('---')
	end
	title:SetVisible(true);

end

function systemNotifications:SetRAPMessage(notificationInfo)
	local title				= GetWidget('systemNotificationsPopupTitle', 'main')
	local subtitle			= GetWidget('systemNotificationsPopupSubtitle', 'main')
	local icon				= GetWidget('systemNotificationsPopupIcon', 'main')
	local image				= GetWidget('systemNotificationsPopupImage', 'main')
	local imageContainer	= GetWidget('systemNotificationsPopupImageContainer', 'main')
	local bodyTitle			= GetWidget('systemNotificationsPopupBodyTitle', 'main')
	local body				= GetWidget('systemNotificationsPopupBody', 'main')
	local footer			= GetWidget('systemNotificationsFooter', 'main')
	local footerLabel		= GetWidget('systemNotificationsFooterLabel', 'main')
	local iconPath = '/ui/elements/empty_pack.tga'

	if not Empty(notificationInfo.icon) then
		iconPath = notificationInfo.icon
	end

	setTextureCheckForWeb(icon, iconPath)

	local paramTable
	local subject = notificationInfo.subject

	if notificationInfo.meta and type(notificationInfo.meta) == 'table' then
		paramTable = self:buildParamTable(notificationInfo)

		if not Empty(notificationInfo.meta.title) then
			title:SetText(self:processStringParams(notificationInfo.meta.title, paramTable))
			title:SetVisible(true)
		else
			title:SetVisible(false)
		end

		if not Empty(notificationInfo.meta.subtitle) then
			subtitle:SetText(self:processStringParams(notificationInfo.meta.subtitle, paramTable))
			subtitle:SetVisible(true)
		else
			subtitle:SetVisible(false)
		end

		if not Empty(notificationInfo.meta.bodyTitle) then
			bodyTitle:SetText(self:processStringParams(notificationInfo.meta.bodyTitle, paramTable))
			bodyTitle:SetVisible(true)
		else
			bodyTitle:SetVisible(false)
		end

		if not Empty(notificationInfo.meta.image) then
			imageContainer:SetVisible(true)

			if notificationInfo.meta.imageHeight then
				image:SetHeight(notificationInfo.meta.imageHeight)
			else
				image:SetHeight('30h')
			end

			if notificationInfo.meta.imageWidth then
				image:SetWidth(notificationInfo.meta.imageWidth)
			else
				image:SetWidth('30h')
			end

			setTextureCheckForWeb(image, notificationInfo.meta.image)
		else
			imageContainer:SetVisible(false)
		end

		if subject == 'REPORT_REFUND_SUBJECT' then
			body:SetText(Translate('rap_report_refund_body', 'nickname', notificationInfo.meta.reportee_nickname, 'time', notificationInfo.meta.report_time))
		elseif subject == 'REPORT_SUSPEND_SUBJECT' then
			body:SetText(Translate('rap_report_suspend_body', 'time', notificationInfo.meta.time))
		elseif subject == 'REPORT_WARNING_SUBJECT' then
			body:SetText(Translate('rap_report_warning_body', 'time', notificationInfo.meta.time))
		--[[ elseif subject == 'REPORT_FROM_WARNING_TO_NORMAL_SUBJECT' then
			body:SetText(Translate('rap_report_warning_to_normal_body')) ]]--
		end

		body:SetVisible(true)

		if not Empty(notificationInfo.meta.footer) then
			footer:SetVisible(true)
			footerLabel:SetText(self:processStringParams(notificationInfo.meta.footer, paramTable))
		else
			footer:SetVisible(false)
		end
	else
		body:SetVisible(false)
		subtitle:SetVisible(false)
		bodyTitle:SetVisible(false)
		imageContainer:SetVisible(false)
	end

	icon:SetTexture("/ui/elements/empty_pack.tga")

	if not Empty(notificationInfo.subject) then
		title:SetText(Translate(notificationInfo.subject))
	else
		title:SetText('---')
	end
	title:SetVisible(true);
end
