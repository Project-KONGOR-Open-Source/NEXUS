---------------------------------------------------------- 
--	Name: 		Mentor Referral Script	            	--			
--  Copyright 2015 Frostburn Studios					--
----------------------------------------------------------

local _G = getfenv(0)
local HoN_Mentor_Referral = _G[HoN_Mentor_Referral] or {}
local interface, interfaceName = object, object:GetName()
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort, gsub, ceil, floor, sub, find, gfind  = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.string.gsub, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
HoN_Mentor_Referral.MAX_LIST_ITEMS = 4
HoN_Mentor_Referral.refOffset = 0
HoN_Mentor_Referral.lastAccountID = 0
HoN_Mentor_Referral.refPlayerListTable = {}
RegisterScript2('Mentor Referral', '1')

local function GetMentorReferrals()
	local accountID = tonumber(UIGetAccountID())
	if (accountID ~= HoN_Mentor_Referral.lastAccountID) then
		HoN_Mentor_Referral.lastAccountID = accountID
		interface:UICmd("SubmitForm('MentorReferralForm', 'f', 'get_referrals', 'account_id', GetAccountID(), 'cookie', GetCookie())")
	end
end

local promoCounterStringLast
local function PopulateCountdown(self, curTime, endTime, index)
	
	local promo_countdown_controller = GetWidget('promo_countdown_controller_'..index)
	promo_countdown_controller.onevent0 = function(...) 
		self:UICmd("SetLocalServerTimeOffset("..curTime..")")
		promo_countdown_controller:RegisterWatch('CountdownSeconds', promo_countdown_controller.ontrigger)
	end	
	promo_countdown_controller:SetCallback('onevent0', promo_countdown_controller.onevent0)
	promo_countdown_controller.onevent1 = function(...) 
		promo_countdown_controller:RegisterWatch('', promo_countdown_controller.ontrigger)
	end
	promo_countdown_controller:SetCallback('onevent1', promo_countdown_controller.onevent1)	
	promo_countdown_controller.ontrigger = function(...) 
		local promoCounterDays, promoCounterHours,promoCounterMinutes, promoCounterSeconds, promoCounterString = '', '', '','', '00000000'
		local panel_0, label_0, panel_1, label_1
		promoCounterDays = tostring(self:UICmd("GetDayFromTime(GetTimeDifference("..endTime..", GetLocalServerTime()))"))
		promoCounterHours = tostring(self:UICmd("GetHourFromTime(GetTimeDifference("..endTime..", GetLocalServerTime()))"))
		promoCounterMinutes = tostring(self:UICmd("GetMinuteFromTime(GetTimeDifference("..endTime..", GetLocalServerTime()))"))
		promoCounterSeconds = tostring(self:UICmd("GetSecondFromTime(GetTimeDifference("..endTime..", GetLocalServerTime()))"))
		
		if string.len(promoCounterDays) <= 1 then
			promoCounterDays = '0' .. promoCounterDays
		end
		if string.len(promoCounterHours) <= 1 then
			promoCounterHours = '0' .. promoCounterHours
		end
		if string.len(promoCounterMinutes) <= 1 then
			promoCounterMinutes = '0' .. promoCounterMinutes
		end
		if string.len(promoCounterSeconds) <= 1 then
			promoCounterSeconds = '0' .. promoCounterSeconds
		end			
		
		promoCounterString = promoCounterDays..promoCounterHours..promoCounterMinutes..promoCounterSeconds		
		if promoCounterString ~= promoCounterStringLast and (self:UICmd("GetTimeDifference("..endTime..", GetLocalServerTime()) gt 0")) then
			for i = 1,8,1 do
				panel_0 = GetWidget('tourn_countdown_unit_'..i..'_0_'..index)
				label_0 = GetWidget('tourn_countdown_unit_label_'..i..'_0_'..index)
				panel_1 = GetWidget('tourn_countdown_unit_'..i..'_1_'..index)
				label_1 = GetWidget('tourn_countdown_unit_label_'..i..'_1_'..index)	
				local currentValue_0 = label_0:GetText()
				local currentValue_1 = label_1:GetText()
				local newValue = string.sub(promoCounterString, i, i)
				if (panel_0:GetY() >= -10 ) and (panel_0:GetY() <= 10 ) then					
					if (tonumber(currentValue_0) ~= tonumber(newValue)) then
						panel_0:UICmd("SlideY('3.2h', 900)")
						label_1:SetText(round(newValue))
						panel_1:SetY('-3.2h')
						panel_1:UICmd("SlideY('0', 900)")
						panel_1:SetVisible(1)
					end
				else
					if (tonumber(currentValue_1) ~= tonumber(newValue)) then
						panel_1:UICmd("SlideY('3.2h', 900)")
						label_0:SetText(round(newValue))
						panel_0:SetY('-3.2h')
						panel_0:UICmd("SlideY('0', 900)")
					end
				end
			end
		end
		promoCounterStringLast = promoCounterString 
	end	
	promo_countdown_controller:RegisterWatch('CountdownSeconds', promo_countdown_controller.ontrigger)
	promo_countdown_controller:RefreshCallbacks()	
end

local function UpdateRefScroller(self, offset)
	local scroller = GetWidget('referral_scroller')
	local currentValue = scroller:UICmd("GetValue()")	
	--println('currentValue ' .. currentValue)
	if (#HoN_Mentor_Referral.refPlayerListTable > HoN_Mentor_Referral.MAX_LIST_ITEMS ) then	
		scroller:SetVisible(true)	
		scroller:UICmd("SetMaxValue("..(#HoN_Mentor_Referral.refPlayerListTable).."); SetMinValue("..(HoN_Mentor_Referral.MAX_LIST_ITEMS).."); SetValue("..(HoN_Mentor_Referral.MAX_LIST_ITEMS)..")")
	elseif (round(tonumber(currentValue)) == 1) then
		scroller:UICmd("SetMaxValue("..(#HoN_Mentor_Referral.refPlayerListTable).."); SetMinValue("..(HoN_Mentor_Referral.MAX_LIST_ITEMS).."); SetValue("..(HoN_Mentor_Referral.MAX_LIST_ITEMS)..")")
		scroller:SetVisible(false)
	else
		scroller:SetVisible(false)
	end
end

local function MentorReferralFormResult(self, success, referrals, milestones, curTime, vestedThreshold, referralCode)
	--println('MentorReferralFormResult() \n' )
	local curTime, success, endTime = tonumber(curTime), AtoB(success), 0
	if (success) then
		--date_created`referred_id`referred_nickname`milestone_1`milestone_2`milestone_3`milestone_4
		
		if (referrals) and NotEmpty(referrals) then 
			for i,v in pairs(explode('|', referrals)) do
				HoN_Mentor_Referral.refPlayerListTable[i] = explode('`', v)
			end	
		end
		
		--milestone_4`startTime`endTime
		if (milestones) and NotEmpty(milestones) then 
			local milestoneInfoTable = {}
			for i,v in pairs(explode('|', milestones)) do
				milestoneInfoTable[i] = explode('`', v)
			end	
			endTime = milestoneInfoTable[4][3]
			milestoneInfoTable = nil
		end
		
		GetWidget('referral_link_box'):SetCallback('onclick', function() GetWidget('referral_link_box'):UICmd("CopyToClipboard('http://heroesofnewerth.com/ref.php?r="..referralCode.."')")  end)
		GetWidget('referral_link_box'):SetCallback('onmouseover', function() GetWidget('referral_link_bg'):SetBorderColor('0 1 0 1') GetWidget('referral_link_label'):SetText(Translate('referral_URL_click')) end)
		GetWidget('referral_link_box'):SetCallback('onmouseout', function() GetWidget('referral_link_bg'):SetBorderColor('.3 .3 .3 1') GetWidget('referral_link_label'):SetText('http://heroesofnewerth.com/ref.php?r='..referralCode) end)
		GetWidget('referral_link_box'):RefreshCallbacks()
		GetWidget('referral_link_label'):SetText('http://heroesofnewerth.com/ref.php?r='..referralCode)
		if (curTime) and (endTime) then
			PopulateCountdown(self, curTime, endTime, 0)
		end
		if (HoN_Mentor_Referral.refPlayerListTable) and (#HoN_Mentor_Referral.refPlayerListTable >= 1) then
			GetWidget('referral_has_referral'):SetVisible(true)
			GetWidget('referral_has_no_referral'):SetVisible(false)
			UpdateRefScroller()
		else
			GetWidget('referral_has_referral'):SetVisible(false)
			GetWidget('referral_has_no_referral'):SetVisible(true)	
		end
	end
end

local function FormStatusController(trigger, ...)
	--print('^r FormStatusController!: ' .. trigger .. ' | ' .. arg[2] .. ' \n' )
	local referral_throbber = GetWidget('referral_throbber')
	local referral_error = GetWidget('referral_error')
	local referral_throbber_label = GetWidget('referral_throbber_label')
	local referral_error_label = GetWidget('referral_error_label')
	
	if (arg[2] == '1') then															-- arg[2] = status
		referral_throbber:SetVisible(true)
		referral_error:FadeOut(50)
		referral_throbber_label:SetText(Translate('mstore_processing_elipses'))
	elseif (arg[2] == '2') then
		referral_throbber:FadeOut(50)
		referral_error:FadeOut(50)
		HoN_Mentor_Referral.errorCount = 0
	elseif (arg[2] == '3') then
		referral_throbber:FadeOut(50)
		referral_error:SetVisible(true)	
		referral_error_label:SetText(Translate('rap_error_string_1'))					-- timeout
	end
end
	
function HoN_Mentor_Referral:ErrorButtonClicked(buttonID)
	GetMentorReferrals()
end

function HoN_Mentor_Referral:RetryButtonClicked(buttonID)
	GetMentorReferrals()
end	
	
function HoN_Mentor_Referral:PopulateRefList(scrollOffset)		
	HoN_Mentor_Referral.refOffset = (scrollOffset or 0) - HoN_Mentor_Referral.MAX_LIST_ITEMS
	if HoN_Mentor_Referral.refOffset < 0 then HoN_Mentor_Referral.refOffset = 0 end
	
	--println('scrollOffset ' .. scrollOffset)
	--println('HoN_Mentor_Referral.refOffset ' .. HoN_Mentor_Referral.refOffset)
	
	for listIndex = 1, HoN_Mentor_Referral.MAX_LIST_ITEMS, 1 do
		local refIndex 			= listIndex + HoN_Mentor_Referral.refOffset			
		--println('refIndex ' .. refIndex)	
		
		if (HoN_Mentor_Referral.refPlayerListTable[refIndex]) then			
			--println('Showing listIndex ' .. listIndex)
			
			local date_created 		= HoN_Mentor_Referral.refPlayerListTable[refIndex][1]
			local referred_id 		= HoN_Mentor_Referral.refPlayerListTable[refIndex][2]
			local referred_nickname = HoN_Mentor_Referral.refPlayerListTable[refIndex][3]
			local milestone = {}
			milestone[1] 		= HoN_Mentor_Referral.refPlayerListTable[refIndex][4]
			milestone[2] 		= HoN_Mentor_Referral.refPlayerListTable[refIndex][5]
			milestone[3] 		= HoN_Mentor_Referral.refPlayerListTable[refIndex][6]
			milestone[4] 		= HoN_Mentor_Referral.refPlayerListTable[refIndex][7]
					
			GetWidget('referral_user_parent_'..listIndex):SetVisible(true)
			GetWidget('referral_user_name_'..listIndex):SetText(referred_nickname)
			
			for i=1,4,1 do
				if (milestone[i]) and (milestone[i] == '1') then
					GetWidget('referral_user_milestone_'..i..'_'..listIndex):SetColor('white')
					GetWidget('referral_user_milestone_'..i..'_'..listIndex):UICmd("SetRenderMode('normal')")
				else
					GetWidget('referral_user_milestone_'..i..'_'..listIndex):SetColor('.4 .4 .4 .4')
					GetWidget('referral_user_milestone_'..i..'_'..listIndex):UICmd("SetRenderMode('grayscale')")				
				end
			end
		else
			--println('Hiding listIndex ' .. listIndex)
			GetWidget('referral_user_parent_'..listIndex):SetVisible(false)
		end
	end	
end	
	
function HoN_Mentor_Referral:ReferralShown()
	GetMentorReferrals()
end

function interface:HoNMentorReferralF(func, ...)
  print(HoN_Mentor_Referral[func](self, ...))
end		
	
interface:RegisterWatch('MentorReferralFormStatus', function (...) FormStatusController('MentorReferralFormStatus', ...) end  )
interface:RegisterWatch('MentorReferralFormResult', MentorReferralFormResult)		







