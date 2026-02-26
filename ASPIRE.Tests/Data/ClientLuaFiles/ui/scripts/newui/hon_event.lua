local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort
local interface, interfaceName = object, object:GetName()

-- local EventType_Unknown = -1
-- local EventType_Special = 0
-- local EventType_Revival = 1
-- local EventType_Newbie 	= 2

local MAX_REVIVAL_AWARDS = 3
local PANEL_FADEOUT_TIME = 200

HoNEvent = {}
HoNEvent.selectedEvent = nil
HoNEvent.eventID = -1
HoNEvent.eventNames = {}
HoNEvent.eventRawData = nil

HoNEvent.revivalLetterDisplayed = false
HoNEvent.revivalTypes = {'revival', 'login', 'mastery'}

HoNEvent.messages = {}
HoNEvent.messageCache = GetDBEntry('messageCache', nil) or {}
HoNEvent.messagesNeedUpdate = false

local function generateEventID()
    HoNEvent.eventID = HoNEvent.eventID + 1
    return HoNEvent.eventID
end

local function getSpecialMessage(id)
    for _, v in ipairs(HoNEvent.messages) do
        if v.id == id then
            return v
        end
    end
    return nil
end

local function addEventName(eventName)
	local found = false
	for _,v in ipairs(HoNEvent.eventNames) do
		if v == eventName then
			found = true
			break
		end
	end
	if not found then
		table.insert(HoNEvent.eventNames, eventName)
		HoNEvent:ShowSystemButton(true)
	end
end

local function getRevivalTitle(revivalType, param1, param2, param3)
	if revivalType == 'revival' then
		return Translate('honevent_revival_revival_text1'), ''
	elseif revivalType == 'login' then
		return Translate('honevent_revival_login_text1'), Translate('honevent_revival_login_text2', 'total', param1, 'current', param2)
	elseif revivalType == 'mastery' then
		return Translate('honevent_revival_mastery_text1'), Translate('honevent_revival_mastery_text2', 'total', param1, 'current', param2, 'hero', param3)
	end
end

local function initSpecialMessage(rawMessage)
	for _, message in pairs(HoNEvent.messages) do
		if message.md5 == rawMessage.md5 then
			return
		end
	end

	-- local message = {}
	-- message.type = EventType_Special
	rawMessage.new = true
	rawMessage.id = generateEventID()
	-- message.title = rawMessage.title
	-- message.message_id = rawMessage.message_id
	-- message.url = rawMessage.url
	-- message.start_time = rawMessage.start_time
	-- message.end_time = rawMessage.end_time
	-- message.date = rawMessage.date
	-- message.md5 = rawMessage.md5
	-- message.left_secs = rawMessage.left_secs

	local prefixID = 'special_message'..rawMessage.id

	-- Add to event list
	local wdgMessageItem = GetWidget('honevent_listitem_'..prefixID, 'main', true)
	if wdgRevivalItem == nil then
		GetWidget('honevent_list'):Instantiate('honevent_listitem', 'id', prefixID)
		GetWidget('honevent_listitem_'..prefixID..'_title'):SetText(rawMessage.title)
		GetWidget('honevent_listitem_'..prefixID..'_icon'):SetTexture('/ui/fe2/newui/res/system_message/msg_type0.png')
	end

	-- Add special message detail panel
	-- local wdgSpecialMessage = GetWidget('honevent_'..prefixID, 'main', true)
	-- if wdgSpecialMessage == nil then
	-- 	GetWidget('honevent_container'):Instantiate('honevent_special_message', 'id', prefixID, 'url', rawMessage.url)
	-- end

	table.insert(HoNEvent.messages, rawMessage)

	addEventName(prefixID)
end

local function initRevivalLetter(revivalData)
	if revivalData == nil or revivalData.gifts == nil then
		Echo('^rinitRevivalLetter Invalid revival data!')
		return
	end

	for _, v in pairs(revivalData.gifts) do
		if tonumber(v.received) == 1 then return end
	end

	if HoNEvent.revivalLetterDisplayed then return end

	HoNEvent.revivalLetterDisplayed = true

	local wdgRevivalLetter = GetWidget('honevent_revivalletter', 'main', true)
	if wdgRevivalLetter == nil then
		GetWidget('honevent_panel'):Instantiate('honevent_revivalletter')
		wdgRevivalLetter = GetWidget('honevent_revivalletter', 'main')
	end

	if wdgRevivalLetter ~= nil then
		local time = tonumber(revivalData.away_days)
		local sub = Translate((time ~= -1 and 'honevent_revivalletter_sub1' or 'honevent_revivalletter_sub2'), 'day', time)

		GetWidget('honevent_revivalletter_title'):SetText(Translate('honevent_revivalletter_title', 'name', GetAccountName()))
		GetWidget('honevent_revivalletter_content'):SetText(Translate('honevent_revivalletter_content', 'sub', sub))
		GetWidget('honevent_revivalletter_ending'):SetText(Translate('honevent_revivalletter_ending'))

		wdgRevivalLetter:SetVisible(true)
		GetWidget('honevent'):SetVisible(false)
		GetWidget('honevent_panel'):SetVisible(true)
	else
		Echo('^rinitRevivalLetter failed!')
	end
end

local function initRevival(revivalData)
	if revivalData == nil then Echo('^rinitRevival Invalid revival data!') return end

	local gifts, startTime, endTime = revivalData.gifts, revivalData.start_time or '', revivalData.end_time or ''
	if gifts == nil or gifts.revival == nil or gifts.login == nil or gifts.mastery == nil then return end

	addEventName('revival')

	-- Add to event list
	local wdgRevivalItem = GetWidget('honevent_listitem_revival', 'main', true)
	if wdgRevivalItem == nil then
		GetWidget('honevent_list'):Instantiate('honevent_listitem', 'id', 'revival')
		GetWidget('honevent_listitem_revival_title'):SetText(Translate('honevent_revival'))
		GetWidget('honevent_listitem_revival_icon'):SetTexture('/ui/fe2/newui/res/system_message/msg_type3.png')
	end

	-- Add revival detail panel
	local wdgRevival = GetWidget('honevent_revival', 'main', true)
	if wdgRevival == nil then
		GetWidget('honevent_container'):Instantiate('honevent_revival')
		wdgRevival = GetWidget('honevent_revival')
	end
	if wdgRevival ~= nil then
		--init revival detail
		GetWidget('honevent_revival_title'):SetText(Translate('honevent_revival'))
		GetWidget('honevent_revival_time'):SetText(startTime..endTime)

		local numItem = 0

		for _, revivalType in ipairs(HoNEvent.revivalTypes) do
			if gifts[revivalType]['rewards'] then
				local name = ''

				--Items
				numItem = 0
				for _, v in pairs(gifts[revivalType]['rewards']) do
					numItem = numItem + 1
					GetWidget('honevent_revival_subitem'..revivalType..numItem):SetVisible(true)

					if Empty(name) then
						if  string.find(v.name, 'Hero_') then
							name = GetHeroDisplayNameFromDB(v.name)
						else
							name = Translate('mstore_product'..v.product_id..'_name')
						end
					end

					local texture = v.icon
					if not string.find(v.icon, '/alt') and string.find(v.name, 'Hero_') then
						texture = GetHeroIconFromDB(v.name)
					end
					GetWidget('honevent_revival_subitem'..revivalType..numItem..'_icon'):SetTexture(texture)
					GetWidget('honevent_revival_subitem'..revivalType..numItem..'_label'):SetText(Translate('mstore_product'..v.product_id..'_name')..(tonumber(v.amount)>1 and ('*'..v.amount) or ''))
				end

				--Title
				local label1, label2 = getRevivalTitle(revivalType, gifts[revivalType]['require_data'], gifts[revivalType]['complete_data'], name)
				GetWidget('honevent_revival_item'..revivalType..'_label1'):SetText(label1)
				GetWidget('honevent_revival_item'..revivalType..'_label2'):SetText(label2)

				--Claim Button
				local complete, received = tonumber(gifts[revivalType]['complete']) == 1, tonumber(gifts[revivalType]['received']) == 1
				local label = received and 'honevent_revival_receive_btn2' or 'honevent_revival_receive_btn'
				GetWidget('honevent_revival_item'..revivalType..'_receive'):SetEnabled(complete and not received)
				GetWidget('honevent_revival_item'..revivalType..'_receive'):SetLabel(Translate(label))
			end
		end
	else
		Echo('^rinitRevival failed!')
	end
end

local function initNewbie()
	addEventName('newbie')
	-- Add to event list
	-- Add newbie panel
end

function HoNEvent:ShowSystemButton(show)
	Trigger('HoNEventSystemButton', tostring(show))
end

function HoNEvent:ClickRevivalLetter()
	GetWidget('honevent_revivalletter'):SetVisible(false)
	HoNEvent:SelectEvent('revival')
	HoNEvent:OpenEventPanel()
end

function HoNEvent:CheckSpecialMessageCache()
    local key = Client.GetAccountID()..'_sm'
    HoNEvent.messageCache[key] = HoNEvent.messageCache[key] or {}

    return HoNEvent.messageCache[key]
end

function HoNEvent:ToggleEventPanel()
	local visible = GetWidget('honevent_panel'):IsVisible()
	if not visible then
		GetWidget('honevent'):SetVisible(true)
		if #HoNEvent.eventNames > 0 then
			HoNEvent:SelectEvent(HoNEvent.selectedEvent == nil and HoNEvent.eventNames[1] or HoNEvent.selectedEvent)
		end

		if IsLoggedIn() then
			RequestHoNEvent()
		end

		HoNEvent:OpenEventPanel()
	else
		HoNEvent:CloseEventPanel()
	end
end

function HoNEvent:OpenEventPanel()
	local popup = GetWidget('honevent')
    popup:SetVisible(true)
    popup:SetWidth('1260i')
    popup:SetHeight('785i')
    popup:SetX((popup:GetParent():GetWidth() - popup:GetWidth())/2)
	popup:SetY((popup:GetParent():GetHeight() - popup:GetHeight())/2 - 5.75 * GetScreenHeight() / 100)

    GetWidget('honevent_line'):SetVisible(true)
    GetWidget('honevent_list'):SetVisible(true)
    GetWidget('honevent_container'):SetVisible(true)
    GetWidget('honevent_panel'):SetVisible(true)
end

function HoNEvent:CloseEventPanel()
    local btnContainer = GetWidget('sysbar_honevent')
    local x, y = btnContainer:GetAbsoluteX()+btnContainer:GetWidth()/2, btnContainer:GetAbsoluteY()+btnContainer:GetHeight()/2

    GetWidget('honevent_line'):SetVisible(false)
    GetWidget('honevent_list'):SetVisible(false)
    GetWidget('honevent_container'):SetVisible(false)

    local popup = GetWidget('honevent')
    popup:SlideX(x - 15, PANEL_FADEOUT_TIME)
    popup:SlideY(y, PANEL_FADEOUT_TIME)
    popup:Scale('150i', '100i', PANEL_FADEOUT_TIME)
    popup:Sleep(PANEL_FADEOUT_TIME, function(widget)
        widget:GetParent():SetVisible(false)
    end)
end

function HoNEvent:GetSelectedEvent()
	return HoNEvent.selectedEvent
end

function HoNEvent:SelectEvent(eventName, playsound)
	if Empty(eventName) then
		Echo('^rHoNEvent:SelectEvent invalid event name!')
		return
	end

	-- if HoNEvent.selectedEvent == eventName then
	-- 	return
	-- end

	HoNEvent.selectedEvent = eventName

	playsound = playsound or true
	if playsound then
		PlaySound('/shared/sounds/ui/button_click_03.wav')
	end

	--Highlight event list item
	local children = GetWidget('honevent_list'):GetChildren()
	for _, child in ipairs(children) do
		local childName = child:GetName()
		GetWidget(childName..'_highlight'):SetVisible(childName == 'honevent_listitem_'..eventName)
	end

	local bSpecialMessage = string.find(eventName, 'special_message') ~= nil
	if bSpecialMessage then
		local message = getSpecialMessage(tonumber(string.sub(eventName, -1)))
		if message then
			message.new = false
			UIManager.GetInterface('webpanel'):HoNWebPanelF('ShowSpecialMessagesPage', GetWidget('honevent_special_message'), message.url)
		end
	end

	--Show detail panel
	children = GetWidget('honevent_container'):GetChildren()
	for _, child in ipairs(children) do
		if bSpecialMessage then
			child:SetVisible(child:GetName() == 'honevent_special_message')
		else
			child:SetVisible(child:GetName() == 'honevent_'..eventName)
		end
	end

	GetWidget('honevent'):SetVisible(true)
end

function HoNEvent:ReceiveRevival(type)
	GetWidget('honevent_revival_item'..type..'_throb'):SetVisible(true)
	SubmitForm('FormReceiveRevival', 'cookie', Client.GetCookie(), 'type', type)
end

GetWidget('honevent_panel'):RegisterWatch('HoNEvent', function(_, resultStr)
	if HoNEvent.messagesNeedUpdate then
		HoNEvent.messagesNeedUpdate = false
		UpdateSpecialMessage(true)
	end

	-- Echo('HoNEvent resultStr:'..resultStr)
	-- resultStr = '{"success":true,"data":{"revival_event":{"start_time":"","end_time":"","away_days":"36","gifts":{"revival":{"id":"6","away_days":"36","reward_id":"1","reward_hero_id":"13","reward_hero_pid":"582","complete":1,"received":0,"rewards":[{"product_id":582,"icon":"\/heroes\/hammerstorm\/icons\/hero.tga","name":"Hero_Hammerstorm","amount":1}]},"login":{"id":"5","reward_avatar":"0","complete":"0","require_data":7,"complete_data":5,"received":0,"rewards":[{"product_id":3609,"icon":"\/ui\/fe2\/store\/icons\/mastery_boost.tga","name":"Mastery Boost","amount":30}]},"mastery":{"id":"5","hero_id":"212","before_level":"1","require_level":"3","reward_avatar":"2646","complete":"0","require_data":"3","complete_data":1,"received":0,"rewards":[{"product_id":2646,"icon":"\/heroes\/hammerstorm\/alt7\/icon.tga","name":"Hero_Hammerstorm","amount":1}]}}}},"vested_threshold":5,"0":true}'
	-- resultStr = '{"success":false,"errors":"login error for reivival","vested_threshold":5,"0":true}'
	if Empty(resultStr) == nil then Echo("^rHoNEvent invalid event data!") return end

	if HoNEvent.eventRawData == resultStr then return end

	HoNEvent.eventRawData = resultStr

	local resultJson = lib_json.decode(resultStr)
	if resultJson == nil or not resultJson.success then
		Echo("^rHoNEvent failed to decode with string: "..resultStr)
		if not resultJson.success then
			Trigger('TriggerDialogBox', 'honevent_revival', '', 'general_ok', '', '', '', resultJson.errors)
		end
		return
	end

	for k, v in pairs(resultJson.data) do
		if k == 'revival_event' then
			initRevivalLetter(v)
			initRevival(v)
		elseif k == 'newbie_event' then
			initNewbie(v)
		else
			Echo('^yHoNEvent unknown event with: '..k)
		end
	end
end)

GetWidget('honevent_panel'):RegisterWatch('ReceiveRevivalResult', function(_, resultStr)
	-- Echo('ReceiveRevivalResult resultStr:'..resultStr)
	if Empty(resultStr) == nil then
		Echo("^rReceiveRevivalResult result is nil!")
		return
	end

	local resultJson = lib_json.decode(resultStr)
	if resultJson == nil then
		Echo("^rReceiveRevivalResult failed to decode with string: "..resultStr)
		return
	end

	GetWidget('honevent_revival_item'..resultJson.data.type..'_throb'):SetVisible(false)

	local message

	if resultJson.success then
		interface:UICmd('ChatRefreshUpgrades();ClientRefreshUpgrades();')

		GetWidget('honevent_revival_item'..resultJson.data.type..'_receive'):SetEnabled(false)
		GetWidget('honevent_revival_item'..resultJson.data.type..'_receive'):SetLabel(Translate('honevent_revival_receive_btn2'))

		if tonumber(resultJson.data.owned_id) ~= 0 then
			local reward = resultJson.data.rewards[1]
			local owned = Translate('mstore_product'..resultJson.data.owned_id..'_name')
			local item = Translate('mstore_product'..reward.product_id..'_name')..(tonumber(reward.amount) > 1 and ('*'..reward.amount) or '')
			message = Translate('honevent_revival_receive_success2', 'owned', owned, 'item', item)
		else
			message = Translate('honevent_revival_receive_success')
		end
	else
		message = Translate('honevent_revival_receive_unsuccess')
	end

	if NotEmpty(message) then
		Trigger('TriggerDialogBox', 'honevent_revival', '', 'general_ok', '', '', '', message)
	end
end)

GetWidget('honevent_panel'):RegisterWatch('SpecialMessagesUpdated', function(_, forcePopup)
    local messageCache = HoNEvent:CheckSpecialMessageCache()
    local newMessageID = -1
    local newMessages = GetSpecialMessages()

    for _, v in ipairs(newMessages) do
    	initSpecialMessage(v)

        if not messageCache[v.md5] then
            messageCache[v.md5] = v
            if newMessageID == -1 then
            	newMessageID = v.id
            end
        end
    end

    if forcePopup and newMessageID ~= -1 and not HoNEvent.revivalLetterDisplayed then
        for _, v in ipairs(HoNEvent.messages) do
        	if v.id == newMessageID then
        		HoNEvent:SelectEvent('special_message'..v.id, false)
        		HoNEvent:OpenEventPanel()
        		break
        	end
        end
    end

    GetDBEntry('messageCache', HoNEvent.messageCache, true, false, false)
end)

GetWidget('honevent_panel'):RegisterWatch('LoginStatus', function(_, accountStatus, statusDescription, isLoggedIn, pwordExpired, isLoggedInChanged, updaterStatus)
	if AtoB(isLoggedIn) then
		local isLoggedInChanged, updaterStatus = AtoB(isLoggedInChanged), tonumber(updaterStatus)

		-- Fetch HoN events
		if isLoggedInChanged then
			RequestHoNEvent()
		end

		-- Fetch special messages
		if isLoggedInChanged  and (updaterStatus == 5 or updaterStatus == 6 or updaterStatus == 8) then
			HoNEvent.messagesNeedUpdate = true
        end

		-- GetWidget('honevent_panel'):Sleep(3000, function() Trigger('ReceiveRevivalResult', '{"success":true,"data":{"type":"revival","rewards":[{"product_id":3609,"icon":"\/ui\/fe2\/store\/icons\/mastery_boost.tga","name":"Mastery Boost","amount":20}],"owned_id":582},"vested_threshold":5,"0":true}') end)
	elseif AtoB(isLoggedInChanged) then
		HoNEvent:ShowSystemButton(false)

		HoNEvent.selectedEvent = nil
		HoNEvent.eventRawData = nil
		HoNEvent.revivalLetterDisplayed = false
		HoNEvent.eventID = -1
		HoNEvent.eventNames = {}
		HoNEvent.messages = {}
		HoNEvent.messagesNeedUpdate = false

		GetWidget('honevent_list'):ClearChildren()
	end
end)