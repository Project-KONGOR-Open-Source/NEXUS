local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort
local interface, interfaceName = object, object:GetName()

local triggerHelper = GetWidget('mm_popup_helper')

local function AccountInfo(sourceWidget, ...)
--[[
 1 - 39 Games		3 Leaves
40 - 79 Games		4 Leaves
79 - 99 Games		5 Leaves
100+ Games			5% Leaves
]]
	if (GetAccountName() == "UnnamedNewbie") then return end

	local gamesPlayed = tonumber((select(5, ...))) or 0
	local disconnects = tonumber((select(6, ...))) or 0
	local leaverPercentage = (gamesPlayed > 0) and (disconnects / gamesPlayed) or 0
	local gamesToClearLeaver = 0
	if (disconnects <= 4) then -- leaver with <= 4 dcs indicates < 40 games
		gamesToClearLeaver = 40
	elseif (disconnects == 5) then -- leaver with 5 dcs indicates < 79 games
		gamesToClearLeaver = 79
	else -- disconnects > 5
		local leaverThreshold = tonumber(interface:UICmd("GetLeaverThreshold("..(gamesPlayed >= 100 and gamesPlayed or 100)..")")) + 0.001
		gamesToClearLeaver = disconnects / leaverThreshold
	end
	local gamesToClearLeaverLeft = math.ceil(gamesToClearLeaver - gamesPlayed)
	if (gamesToClearLeaverLeft < 0) then
		gamesToClearLeaverLeft = 0
	end
	GetWidget("matchmaking_leaver_label"):SetText(Translate("mm_isleaver_onopen_body", "leaverpercent", string.format("%.2f%%", leaverPercentage * 100), 'matchestoclear', gamesToClearLeaverLeft, 'gamesplayed', gamesPlayed) )
end
triggerHelper:RegisterWatch("AccountInfo", AccountInfo)

local function HostErrorMessage()
	if (not IsInGroup()) and (not IsInQueue()) then
		Trigger('TMMReset')
	end
end
triggerHelper:RegisterWatch('HostErrorMessage', HostErrorMessage)

local function TMMGamePhase(_, phase)
	if tonumber(phase) > 0 then
		HideWidget('mm_popup_joinmatch', 'main')
		HideWidget('mm_popup_serverfound', 'main')
	end
end
triggerHelper:RegisterWatch('TMMGamePhase', TMMGamePhase)

local function TMMReset()
	HideWidget('mm_popup_init')
	HideWidget('mm_loading_1')
	HideWidget('mm_popup_joiningqueue')
	HideWidget('mm_popup_serverfound')
	HideWidget('mm_popup_foundmatch')
	HideWidget('mm_popup_joinmatch')
end
triggerHelper:RegisterWatch('TMMReset', TMMReset)

local function TMMDisplay(sourceWidget, ...)
--[[ params
		-- for player slots 1 - 5
		0 = Slot Account ID
		1 = Slot Username
		2 = Slot number
		3 = Slot TMR
		4 = Player Loading TMM Status | Player Ready Status
		--
		25 = Update type
		26 = group size
		27 = average TMR
		28 = leader account id
		29 = game type
		30 = MapNames
		31 = GameModes
		32 = Regions
		33 = TSNULL
		34 = PlayerInvitationResponses
		35 = TeamSize
		36 = TSNULL
		37 = Verified
		38 = VerifiedOnly
		39 = BotDifficulty
		40 = RandomizeBots
		41 = GroupType
]]
	HideWidget("mm_popup_init")
end
triggerHelper:RegisterWatch("TMMDisplay", TMMDisplay)

local function TMMFoundServer()
	HideWidget('mm_popup_foundmatch')
	GetWidget('mm_popup_serverfound'):DoEventN(0)
end
triggerHelper:RegisterWatch('TMMFoundServer', TMMFoundServer)

local function TMMJoinMatch()
	GetWidget('mm_popup_serverfound'):SetVisible(false)
	GetWidget('mm_popup_joinmatch'):DoEventN(0)
end
triggerHelper:RegisterWatch('TMMJoinMatch', TMMJoinMatch)

local function TMMNoMatchesFound()
	GetWidget('mm_popup_nomatchesfound'):DoEventN(0)
end
triggerHelper:RegisterWatch('TMMNoMatchesFound', TMMNoMatchesFound)

local function TMMNoServersFound()
	GetWidget('mm_popup_noserversfound'):DoEventN(0)
end
triggerHelper:RegisterWatch('TMMNoServersFound', TMMNoServersFound)

local function TMMLeaveGroup(_, reason, param1)
	Echo('TMMLeaveGroup reason:'..reason..' param1:'..param1)
	if (reason == 'isleaver') then
		Teammaking_V2:UpdateLeaverBlock()
	elseif (reason == 'disabled') then
		GetWidget('mm_popup_tmmdisabled'):DoEventN(0)
	elseif (reason == 'invalidversion') then
		GetWidget('mm_popup_invalid_version'):DoEventN(0)
	elseif (reason == 'groupfull') then
		GetWidget('mm_popup_group_full'):DoEventN(0)
	elseif (reason == 'busy') then
		GetWidget('mm_popup_tmmbusy'):DoEventN(0)
	elseif (reason == 'optionunavailable') then
		GetWidget('mm_popup_optionsunavailable'):DoEventN(0)
	elseif (reason == 'disbanded') then
		if not Teammaking_V2.supressDialog then
			GetWidget('mm_popup_disbanded'):DoEventN(0)
		end
		Teammaking_V2.supressDialog  = false
	elseif (reason == 'kicked') then
		GetWidget('mm_popup_kicked'):DoEventN(0)
	elseif (reason == 'disconnected') then
		if not IsLoggedIn() then return end

		local wasInGroup = AtoB(param1)
		local hasMaxRetries = GetConnectToChatServerRetries() >= GetCvarInt('chat_maxReconnectAttempts')
		if not wasInGroup and hasMaxRetries then
			Set('_TMM_Disconnected_CS', true)
		end

		if wasInGroup or hasMaxRetries then
			GetWidget('mm_popup_disconnected_label'):SetText(Translate(wasInGroup and 'mm_disconnected_body' or 'mm_disconnected_body2'))
			GetWidget('mm_popup_disconnected'):DoEventN(0)
		end
	elseif (reason == 'groupqueued') then
		GetWidget('mm_popup_groupqueued'):DoEventN(0)
	elseif (reason == 'banned') then
		GetWidget('mm_popup_banned'):DoEventN(0)
	elseif (reason == 'unknown') then
		GetWidget('mm_popup_unknown'):DoEventN(0)
	elseif (reason == 'foundmatch') then
		HideWidget('mm_popup_serverfound')
	elseif (reason == 'servernotidle') then
		GetWidget('mm_popup_servernotidle'):DoEventN(0)
	elseif (reason == 'noteligible') then
		GetWidget('mm_popup_noteligible'):DoEventN(0)
	end

	if (reason == 'optionunavailable') then
		interface:UICmd('RequestTMMPopularityUpdate()')
	else
		Trigger('TMMReset')
	end
end
triggerHelper:RegisterWatch('TMMLeaveGroup', TMMLeaveGroup)

local function FailedAcceptGroupMM(names)
	local pNames = explode(',', names)
	local bSelf = false
	local others = false

	if (pNames) then
		for i,n in ipairs(pNames) do
			if (n == StripClanTag(GetAccountName())) then
				bSelf = true
			else
				others = true
			end

			if (bSelf and others) then
				break
			end
		end
	end

	local string = ''
	if (bSelf and others) then
		string = Translate('mm_group_timeout_self_others')..' '
		local first = true
		for i,n in ipairs(pNames) do
			if (n ~= StripClanTag(GetAccountName())) then
				if (first) then
					string = string .. n
					first = false
				else
					string = string .. ', ' .. n
				end
			end
		end
		string = string .. '.'
	elseif (others) then
		string = Translate('mm_group_timeout_others')..' '
		local first = true
		for i,n in ipairs(pNames) do
			if (n ~= StripClanTag(GetAccountName())) then
				if (first) then
					string = string .. n
					first = false
				else
					string = string .. ', ' .. n
				end
			end
		end
		string = string .. '.'
	elseif (bSelf) then
		string = 'mm_group_timeout_self'
	else
		string = 'mm_group_timeout_unknown'
	end

	Trigger('TriggerDialogBox', 'mm_group_timeout', '', 'general_ok', '', '', 'mm_group_timeout', string)
end

triggerHelper:RegisterWatch('TMMMatchFailedToAccept', function(_, names) FailedAcceptGroupMM(names) end)