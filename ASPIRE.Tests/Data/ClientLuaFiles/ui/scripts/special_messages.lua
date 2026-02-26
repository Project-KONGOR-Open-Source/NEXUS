-- Special Messages
local interface, interfaceName = object, object:GetName()

local PANEL_CLOSE_ANIMATION_TIME = 200

specialMessages = specialMessages or {}
specialMessages.cache = GetDBEntry('specialMessagesCache', nil, true, false, true) or {}

interface:RegisterWatch('SpecialMessagesUpdated', function(widget, forcePopup)
	local accountSpecialMessagesCache = specialMessages:checkCreateAccountSMCache()
	local newList = GetSpecialMessages()
	local count = 0;

	for k,newSpecialMessage in ipairs(newList) do
		Echo('msg title: '..newSpecialMessage.title..'       msg url: '..newSpecialMessage.url)
		Set('special_messages_web_url', newSpecialMessage.url, 'string')
		GetWidget('system_special_message_title'):SetText(newSpecialMessage.title)
		count = count + 1
		if not accountSpecialMessagesCache[newSpecialMessage.md5] then
			accountSpecialMessagesCache[newSpecialMessage.md5] = newSpecialMessage
			if forcePopup then
				specialMessages:OpenSpecialMessages()
				break
			end
		end
	end

	GetWidget('sysbar_special_message_button'):SetVisible(count ~= 0)

	GetDBEntry('specialMessagesCache', specialMessages.cache, true, false, false)
end)

function specialMessages:checkCreateAccountSMCache()
	self.cache = self.cache or {}
	local accountID = Client.GetAccountID()
	local key = accountID..'_sm'
	self.cache[key] = self.cache[key] or {}
	return self.cache[key]
end

function specialMessages:ToggleSpecialMessages()

	local outerPanel = GetWidget('systemSpecialMessagePopup')
	if outerPanel:IsVisible() then
		self:CloseSpecialMessages()
	else
		self:OpenSpecialMessages()
	end
end

function specialMessages:OpenSpecialMessages()
	local outerPanel = GetWidget('systemSpecialMessagePopup')
	local closeBtn = GetWidget('system_special_message_button')
	local label = GetWidget('system_special_message_title')
	local webPanel = GetWidget('web_browser_special_messages_container')
	outerPanel:Scale('1222i', '990i', 0)
	closeBtn:Scale('4.3h', '4.3h', 0)
	webPanel:SetVisible(true)
	label:SetVisible(true)
	outerPanel:SetVisible(true)
end

function specialMessages:CloseSpecialMessages()

	local outerPanel = GetWidget('systemSpecialMessagePopup')
	local closeBtn = GetWidget('system_special_message_button')
	local label = GetWidget('system_special_message_title')
	local webPanel = GetWidget('web_browser_special_messages_container')

	webPanel:SetVisible(false)
	label:SetVisible(false)
	closeBtn:Scale('0h', '0h', PANEL_CLOSE_ANIMATION_TIME)
	outerPanel:Scale('0h', '0h', PANEL_CLOSE_ANIMATION_TIME)
	outerPanel:SlideX('27h', PANEL_CLOSE_ANIMATION_TIME)
	outerPanel:SlideY('-0.2h', PANEL_CLOSE_ANIMATION_TIME)
	outerPanel:Sleep(PANEL_CLOSE_ANIMATION_TIME, function(widget)
		widget:SetVisible(false)
	end)
end
