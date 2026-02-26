local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort
local interface, interfaceName = object, object:GetName()

Loading_V2 = {}

local TEAM_MEMBER_MAX_COUNT = 5

local _queueAllReady = false
local _queueValidStartTime = false
local _queueLoadingProgress = {}

function Loading_V2:Init()
	local widget = GetWidget('loading_v2_helper')
	widget:RegisterWatch('TMMReadyStatus', function(_, ...) Loading_V2:OnTMMReadyStatus(...) end)
	widget:RegisterWatch("TMMPlayerStatus0", function(_, ...) Loading_V2:OnTMMPlayerStatus(0, ...) end)
	widget:RegisterWatch("TMMPlayerStatus1", function(_, ...) Loading_V2:OnTMMPlayerStatus(1, ...) end)
	widget:RegisterWatch("TMMPlayerStatus2", function(_, ...) Loading_V2:OnTMMPlayerStatus(2, ...) end)
	widget:RegisterWatch("TMMPlayerStatus3", function(_, ...) Loading_V2:OnTMMPlayerStatus(3, ...) end)
	widget:RegisterWatch("TMMPlayerStatus4", function(_, ...) Loading_V2:OnTMMPlayerStatus(4, ...) end)
end

function Loading_V2:OnTMMPlayerStatus(id, ...)
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
	local player = {}
	player.name = arg[2]
	player.accountID = arg[1]
	player.progress = tonumber(arg[5]) -- 0- 100
	_queueLoadingProgress[id+1] = player
	if UIGetAccountID() == player.accountID and id > 0 then
		local temp = _queueLoadingProgress[1]
		_queueLoadingProgress[1] = _queueLoadingProgress[id+1]
		_queueLoadingProgress[id+1] = temp
	end
	Loading_V2:UpdateQueueLoadingProgress()
end

function Loading_V2:OnTMMReadyStatus(isLeader, otherPlayerReady, allReady, selfReady, validStartTime)
	-- Echo('^gOnTMMReadyStatus ----------------------------------')
	-- Echo('^gisLeader  '..tostring(isLeader))
	-- Echo('^gotherPlayerReady   '..tostring(otherPlayerReady))
	-- Echo('^gallReady   '..tostring(allReady))
	-- Echo('^gselfReady   '..tostring(selfReady))
	-- Echo('^gvalidStartTime   '..tostring(validStartTime))

	_queueAllReady = AtoB(allReady)
	_queueValidStartTime = AtoB(validStartTime)
	Loading_V2:UpdateQueueLoadingPanel()
end

function Loading_V2:UpdateQueueLoadingPanel()
	local isInQueue = AtoB(interface:UICmd("IsInQueue"))
	if isInQueue then 

		if not GetWidget('queue_loading_small'):IsVisible() then
			Loading_V2:ShowQueueLoading()

			Loading_V2:CollopseQueueLoading()
			--GetWidget('queue_loading_phase1'):SetVisible(false)
			--GetWidget('queue_loading_phase2'):SetVisible(true)
		end
	else
		if _queueAllReady and not _queueValidStartTime then 
			Loading_V2:ShowQueueLoading()
			MapSelection_V2:Hide()
		else
			GetWidget('queue_loading'):SetVisible(false)
		end

		--GetWidget('queue_loading_phase1'):SetVisible(true)
		--GetWidget('queue_loading_phase2'):SetVisible(false)
		GetWidget('queue_loading_small'):SetVisible(false)
	end
end

function Loading_V2:UpdateQueueLoadingProgress()
	local memberCount = 0
	for i=1, TEAM_MEMBER_MAX_COUNT do
		local rootwidget = GetWidget('queue_loading_progress_'..i)
		local progresswidget = GetWidget('queue_loading_progress_'..i..'_progress')

		if _queueLoadingProgress[i] and NotEmpty(_queueLoadingProgress[i].name) then
			rootwidget:SetVisible(true) 
			progresswidget:SetWidth(tostring(_queueLoadingProgress[i].progress)..'%')

			if i > 1 then 
				local iconTable = Explode( "|", GetChatClientInfo(StripClanTag(_queueLoadingProgress[i].name), "getaccounticontexturepath"))
				if (iconTable[1]) then 
					GetWidget('queue_loading_progress_'..i..'_icon'):SetTexture(iconTable[1]) 
				else
					GetWidget('queue_loading_progress_'..i..'_icon'):SetTexture('/ui/fe2/store/icons/account_icons/default.tga') 
				end

				GetWidget('queue_loading_progress_'..i..'_playername'):SetText(_queueLoadingProgress[i].name)
			end

			memberCount = memberCount + 1
		else
			rootwidget:SetVisible(false)
		end
	end

	GetWidget('queue_loading_group'):SetVisible(memberCount > 1)
end

function Loading_V2:SetQueueLoadingInfo(name, map)
	local bg = ''
	local map_string_list = {}

	if name == 'coop' then 
		bg = 'coop'
	elseif name == 'con' or name == 'caldavar' or name == 'midwars' or
		name == 'capturetheflag' or name == 'devowars' or name == 'grimmscrossing' or name == 'soccer' or name == 'team_deathmatch' or name == 'caldavar_reborn' or
		name == 'midwars_reborn' then
		bg = map
	else
		bg = 'other'
	end

	if bg == 'midwars_reborn' then
		bg = 'midwars'
	end

	-- randomize loading screen if map is Forests of Caldavar
	if bg == 'caldavar' then
		map_string_list = { 
			'caldavar', 
			'caldavar_reborn', 
			'caldavar_alt1', 
		}
		local rand = math.random(1, #map_string_list)

		bg = map_string_list[rand]
	end

	-- randomize loading screen if map is Mid Wars
	if bg == 'midwars' then
		local map_string_list = { 
			'midwars', 
			'midwars_alt1', 
			'midwars_alt2'
		}
		local rand = math.random(1, #map_string_list)

		bg = map_string_list[rand]
	end

	GetWidget('queue_loading_bg'):SetTexture('/ui/fe2/newui/res/loading/'..bg..'.tga')

	local text = ''
	if name == 'coop' then
		text = 'newui_loading_coop'
	elseif name == 'con' then
		text = 'newui_loading_con'
	else
		text = 'map_'..map
	end
	GetWidget('queue_loading_small_name'):SetText(Translate(text))

	if name == 'con' then
		GetWidget('queue_loading_bg_logo'):SetTexture('/ui/fe2/newui/res/loading/bglogo_con.png')
	else
		GetWidget('queue_loading_bg_logo'):SetTexture('/ui/fe2/newui/res/loading/bglogo.png')
	end
end

function Loading_V2:ShowQueueLoading()
	GetWidget('queue_loading'):SetWidth('100%')
	GetWidget('queue_loading'):SetHeight('100%')
	GetWidget('queue_loading'):SetVisible(true)

	Social_V2:CloseSocialPopups()
end

function Loading_V2:ExpandQueueLoading()
	GetWidget('queue_loading'):ScaleWidth('100%', 300)
	GetWidget('queue_loading'):ScaleHeight('100%', 300)
	GetWidget('queue_loading'):FadeIn(300)
	GetWidget('queue_loading_small'):FadeOut(150)
end

function Loading_V2:CollopseQueueLoading()
	GetWidget('queue_loading'):ScaleWidth('0%', 300)
	GetWidget('queue_loading'):ScaleHeight('0%', 300)
	GetWidget('queue_loading'):FadeOut(300)
	GetWidget('queue_loading_small'):FadeIn(150)
end

----------------------------------------------------------------------------------------------------------------------
function Loading_V2:SetHostedLoadingInfo(map)
	if map == 'midwars_reborn' then
		map = 'midwars'
	end

	if map == 'unknown' or map == 'caldavar' or map == 'midwars' or 
		map == 'capturetheflag' or map == 'devowars' or map == 'grimmscrossing' or map == 'soccer' or map == 'team_deathmatch' or map == 'caldavar_reborn' then
		GetWidget('loading_bg_image'):SetTexture('/ui/fe2/newui/res/loading/'..map..'.tga')
	else
		GetWidget('loading_bg_image'):SetTexture('/ui/fe2/newui/res/loading/other.tga')
	end
	GetWidget('queue_loading_progress_hosted_progress', 'loading'):SetWidth('0%')
	GetWidget('loading_bg_image_logo'):SetTexture('/ui/fe2/newui/res/loading/bglogo.png')
end

function Loading_V2:SetMMLoadingInfo(name, map)
	local bg = ''
	if name == 'coop' then 
		bg = 'coop'
	elseif name == 'con' or name == 'caldavar' or name == 'midwars' or
			name == 'capturetheflag' or name == 'devowars' or name == 'grimmscrossing' or name == 'soccer' or name == 'team_deathmatch' or name == 'caldavar_reborn' or
			name == 'midwars_reborn' then
		bg = map
	else
		bg = 'other'
	end

	if bg == 'midwars_reborn' then
		bg = 'midwars'
	end
	
	GetWidget('loading_bg_image'):SetTexture('/ui/fe2/newui/res/loading/'..bg..'.tga')
	if name == 'con' then
		GetWidget('loading_bg_image_logo'):SetTexture('/ui/fe2/newui/res/loading/bglogo_con.png')
	else
		GetWidget('loading_bg_image_logo'):SetTexture('/ui/fe2/newui/res/loading/bglogo.png')
	end

	local iconTable = Explode( "|", GetChatClientInfo(GetAccountName(), "getaccounticontexturepath"))
	if (iconTable[1]) then 
		GetWidget('mm_loading_player_10_icon', 'loading_matchmaking_connecting'):SetTexture(iconTable[1]) 
		GetWidget('mm_loading_player_11_icon'):SetTexture(iconTable[1]) 
	end
end

function Loading_V2:OnMMLobbyPlayerList(index, _, ...)
	if index < 0 or index > 9 then return end

	local isMe = AtoB(arg[18])
	local icon = arg[17]
	local isActiveSlot = AtoB(arg[19])
	local connectStatus = tonumber(arg[20])
	local isBot = AtoB(arg[22])

	GetWidget('mm_loading_player_'..index):SetVisible(isActiveSlot and not isMe)
	if isActiveSlot and not isMe then
		if connectStatus == 0 then 
			GetWidget('mm_loading_player_'..index..'_icon'):SetTexture('/ui/fe2/lobby/mystery.tga')
			GetWidget('mm_loading_player_'..index..'_icon'):SetRenderMode('grayscale')
			GetWidget('mm_loading_player_'..index..'_light'):SetTexture('/ui/fe2/newui/res/loading/light_gray.png')
		else
			if isBot then
				GetWidget('mm_loading_player_'..index..'_icon'):SetTexture('/ui/fe2/newui/res/lobby/boticon.png')
			else
				GetWidget('mm_loading_player_'..index..'_icon'):SetTexture(icon)
			end
			GetWidget('mm_loading_player_'..index..'_icon'):SetRenderMode('normal')

			if connectStatus == 1 then 
				GetWidget('mm_loading_player_'..index..'_light'):SetTexture('/ui/fe2/newui/res/loading/light_yellow.png')
			else
				GetWidget('mm_loading_player_'..index..'_light'):SetTexture('/ui/fe2/newui/res/loading/light_green.png')
			end
		end
		
	end
end

function Loading_V2:OnMMLoadingProgress(progress)
	progress = tonumber(progress)
	GetWidget('mm_loading_me_progress', 'loading_matchmaking_connecting'):SetVisible(progress < 0.99)
	GetWidget('mm_loading_me_progress', 'loading_matchmaking_connecting'):SetWidth(tostring(progress*100)..'%')
	GetWidget('mm_popup_serverfound'):SetVisible(false)
	GetWidget('mm_popup_joinmatch'):SetVisible(false)
end

function Loading_V2:OnFrame()
	local hasLoading = GetWidget('hosted_loading', 'loading'):IsVisible() or
						GetWidget('mm_loading_phase1', 'loading_matchmaking_connecting'):IsVisible() or
						GetWidget('mm_loading_phase2'):IsVisible()

	GetWidget('loading_bg'):SetVisible(hasLoading)
end

function Loading_V2:OnClickLeaveQueue()
	Trigger('TriggerDialogBox', 'newui_loading_leavequeue_title', 'general_cancel', 'general_yes', '', '', 
										'newui_loading_leavequeue_desc', '', '', 'Teammaking_V2:OnClickLeaveQueue() playBgMusic()')
end