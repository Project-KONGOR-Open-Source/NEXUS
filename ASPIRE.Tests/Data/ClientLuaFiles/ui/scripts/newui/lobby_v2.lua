local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort
local interface, interfaceName = object, object:GetName()

Lobby_V2 = {}

local GAMEMODE_SOUND = {
	rd	= '/shared/sounds/announcer/lobby/random_draft.wav',
	bd	= '/shared/sounds/announcer/lobby/banning_draft.wav',
	sd	= '/shared/sounds/announcer/lobby/single_draft.wav',
	ap	= '/shared/sounds/announcer/lobby/all_pick.wav',
	cm	= '/shared/sounds/announcer/lobby/captains_pick.wav',
	gt	= '/shared/sounds/announcer/lobby/all_pick.wav',
	ar	= '/shared/sounds/announcer/lobby/all_random.wav',
	bb	= '/shared/sounds/announcer/lobby/blind_ban.wav',
	bp	= '/shared/sounds/announcer/lobby/banning_pick.wav',
	lp	= '/shared/sounds/announcer/lobby/lockpick.wav',
	fp	= '/shared/sounds/announcer/lobby/all_pick.wav',
	sp	= '/shared/sounds/announcer/lobby/all_pick.wav',
	sm 	= '/shared/sounds/announcer/lobby/all_pick.wav',
	hb	= '', -- This has been omitted so the audio clip does not get played twice
	mwb	= '/shared/sounds/announcer/lobby/blind_ban.wav',		
	rb	= '/shared/sounds/announcer/lobby/blind_ban.wav',
	cp 	= '/shared/sounds/announcer/lobby/counter_pick.wav'
}

local GAMEMAP_SOUND = {
	prophets		= { sound = '/shared/sounds/announcer/lobby/prophets.wav', length=2000, announceMode = false},
	midwars 		= { sound = '/shared/sounds/announcer/lobby/mid_wars.wav', length=2000, announceMode = true},
	capturetheflag 	= { sound = '/shared/sounds/announcer/lobby/capture_the_flag_merrick.wav', length=2000, announceMode = true},
	thegrimmhunt 	= { sound = '/shared/sounds/announcer/lobby/grimm_hunt.wav', length=2000, announceMode = true},
	devowars 		= { sound = '/shared/sounds/announcer/lobby/devo_wars.wav', length=2000, announceMode = true},
}

local GROUP_NUM = 3
local TEAM_NUM = 2
local ROW_NUM = 3
local PERROW_NUM = 9
local GROUP_HERO_NUM = (PERROW_NUM * ROW_NUM)
local HERORATINGTHRESHOLD = 3.0
local AVATAR_OFFSET = 300
local AVATAR_LARGESIZE = '85%'
local AVATAR_MIDDLESIZE = '65%'
local AVATAR_SMALLSIZE = '50%'

local SPECTATOR_MAX = 7
local FILTER_THROSHOLD = 3.0
local AVATAR_COUPON_MAXCOUNT = 3

local SP_ROW_NUM = 3
local SP_COLUMN_NUM = 10

local SD_ROW_NUM = 5
local SD_COLUMN_NUM = 3

local RD_ROW_NUM = 5
local RD_COLUMN_NUM = 4

local BD_GROUP_NUM = 3
local BD_ROW_NUM = 2
local BD_COLUMN_NUM = 4

local BOT_ROW_NUM = 5
local BOT_COLUMN_NUM = 9

local PLAYER_MAXCOUNT = 10

local EAP_IMMORTAL = 1
local EAP_LEGENDARY = 2
local EAP_HEROONLY = 3
local EAP_ITEMCOUNT = 3

local AVATAR_ANIMATION_TIME = 200
local _largeUI = false
local _uiStr = 'small'
local _heroesInit = false
local _selectedHero = -1
local _myGameHero = ''
local _heroInfos = {}
local _soloCandidates = {}
local _lockedHeroes = {}
local _selectedCandidate = -1
local _selectedLockedHero = -1

local _currentGameMode = ''
local _botPosition = -1
local _selectedBot = {}
local _allBotList = {}
local _validBotList = {}

local _goldCoins = 0
local _silverCoins = 0

local _avatars = {}
local _selectedAvatar = 1
local _avatarChanging = false
local _avatarsNewUltimateInfo = {}
local _avatarHero = ''
local _avatarCode = ''
local _avatarDefaultCode = ''
local _avatarDiscount = ''
local _pickHeroByAvatar = false
local _EAPInfo = {}


local _menuPosition = -1
local _spectators = {}
local _playingMapSound = false
local _showHeroTips = false

local _favoriteHeroes = {}

local function TestEcho(str)
	if not GetCvarBool('releaseStage_stable') then
		Echo(str)
	end
end
---------------------------------- Init ----------------------------------------------
function Lobby_V2:Init()
	_largeUI = Common_V2:UsingLargeUI()
	Lobby_V2:PrepareUIByResolution()

	local widget = GetWidget('lobby_trigger_help')
	widget:RegisterWatch('GamePhase', function(_, ...) Lobby_V2:OnGamePhase(...) end)
	widget:RegisterWatch('HeroSelectInfo', function(_, ...) Lobby_V2:OnHeroSelectInfo(...) end)
	widget:RegisterWatch('StoreAvatarIsNewResult', function(_, ...) Lobby_V2:OnStoreAvatarIsNewResult(...) end)
	widget:RegisterWatch('GoldCoins', function(_, gold) _goldCoins = tonumber(gold) end)
	widget:RegisterWatch('SilverCoins', function(_, silver) _silverCoins = tonumber(silver) end)
	widget:RegisterWatch('LobbyBuyAvatarStatus', function(_, ...) Lobby_V2:OnLobbyPurchaseStatus(...) end)
	widget:RegisterWatch('LobbyBuyAvatarResults', function(_, ...) Lobby_V2:OnLobbyPurchaseResults(...) end)
	widget:RegisterWatch('LobbyBuyHeroStatus', function(_, ...) Lobby_V2:OnLobbyPurchaseStatus(...) end)
	widget:RegisterWatch('LobbyBuyHeroResults', function(_, ...) Lobby_V2:OnLobbyPurchaseResults(...) end)
	widget:RegisterWatch('InfosRefreshed', function(_, ...) Lobby_V2:OnUpgradesRefresh(...) end)
	widget:RegisterWatch('UpgradesRefreshed', function(_, ...) Lobby_V2:OnUpgradesRefresh(...) end)
	widget:RegisterWatch('HeroSelectHeroInfo', function(_, ...) Lobby_V2:OnSelectHeroInfo(...) end)
	widget:RegisterWatch('MicroStoreResults', function(_, ...) Lobby_V2:OnMicroStoreResults(...) end)
	
	for i=0,3 do
		widget:RegisterWatch('HeroSelectHeroAbilityInfo'..i, function(_, ...) Lobby_V2:OnSelectHeroAbilityInfo(i+1, ...) end)
	end
	
	for i=0, 161 do
		widget:RegisterWatch('HeroSelectHeroList'..i, function(_, ...) Lobby_V2:OnHeroSelectHeroList(i, ...) end)
	end

	for i=0, 5 do
		widget:RegisterWatch('SoloHeroCandidate'..i, function(_, ...) Lobby_V2:OnSoloHeroCandidate(i+1, ...) end)
		widget:RegisterWatch('LockHeroSelectHeroList'..i, function(_, ...) Lobby_V2:OnLockHeroSelectHeroList(i+1, ...) end)
	end
end

function Lobby_V2:PrepareUIByResolution()
	if _largeUI then 
		_uiStr = 'large'
		AVATAR_OFFSET = 280
		AVATAR_LARGESIZE = '70%'
		AVATAR_MIDDLESIZE = '50%'
		AVATAR_SMALLSIZE = '35%'
	else
		_uiStr = 'small'
		AVATAR_OFFSET = 220
		AVATAR_LARGESIZE = '70%'
		AVATAR_MIDDLESIZE = '50%'
		AVATAR_SMALLSIZE = '35%'
	end

	GetWidget('lobby_picker_large'):SetVisible(_largeUI)
	GetWidget('lobby_picker_small'):SetVisible(not _largeUI)
	GetWidget('lobby_picker_herotip_large'):SetVisible(_largeUI)
	GetWidget('lobby_picker_herotip_small'):SetVisible(not _largeUI)

	if _largeUI then
		GetWidget('lobby_ready'):SetX('770i')
		GetWidget('lobby_selecthero'):SetX('715i')
		GetWidget('lobby_selecthero'):SetWidth('209i')
		GetWidget('lobby_buyhero'):SetX('715i')
		GetWidget('lobby_buyhero'):SetWidth('209i')
		GetWidget('lobby_picker_herobuttons_coins'):SetX('715i')
		GetWidget('lobby_picker_herobuttons_coins'):SetWidth('209i')
		GetWidget('lobby_repick'):SetX('580i')
		GetWidget('lobby_random'):SetX('580i')
		GetWidget('lobby_picker_avatars_area'):SetWidth('1098i')
		GetWidget('lobby_picker_filter'):SetWidth('1096i')
		GetWidget('lobby_picker_filter'):SetY('50i')
		GetWidget('lobby_picker_avatars_buttons'):SetX('675i')
		GetWidget('lobby_picker_avatars_buttons'):SetWidth('209i')
	else
		GetWidget('lobby_ready'):SetX('590i')
		GetWidget('lobby_selecthero'):SetX('543i')
		GetWidget('lobby_selecthero'):SetWidth('187i')
		GetWidget('lobby_buyhero'):SetX('543i')
		GetWidget('lobby_buyhero'):SetWidth('187i')
		GetWidget('lobby_picker_herobuttons_coins'):SetX('543i')
		GetWidget('lobby_picker_herobuttons_coins'):SetWidth('187i')
		GetWidget('lobby_repick'):SetX('420i')
		GetWidget('lobby_random'):SetX('420i')
		GetWidget('lobby_picker_avatars_area'):SetWidth('775i')
		GetWidget('lobby_picker_filter'):SetWidth('775i')
		GetWidget('lobby_picker_filter'):SetY('100i')
		GetWidget('lobby_picker_avatars_buttons'):SetX('495i')
		GetWidget('lobby_picker_avatars_buttons'):SetWidth('187i')
	end
end
-------------------------------- Function -------------------------------------------
function Lobby_V2:Show()
	GetWidget('lobby'):SetVisible(true)

	Player_Hosted_V2:Hide()
	MapSelection_V2:Hide()

	local botsTable = GetBotDefinitions()
	_allBotList = {}
	for i,v in ipairs(botsTable) do
		if NotEmpty(v.sHeroName) then
			local bot = {}
			bot.sHeroName = v.sHeroName
			bot.sPlayerName = v.sDefaultPlayerName
			bot.def = v.sName
			table.insert(_allBotList, bot)
		end
	end
	 
	-- for i,v in ipairs(_allBotList) do
	-- 	Echo('^g~~~~~~~~~~~~~~~~~~~~~~~~')
	-- 	Echo(tostring(i)..':'..v.sPlayerName)
	-- end
end

function Lobby_V2:Hide()
	GetWidget('lobby'):SetVisible(false)

	GetWidget('lobby_popup_purchase_eap'):SetVisible(false)
	GetWidget('lobby_popup_purchase_avatar'):SetVisible(false)
	GetWidget('lobby_popup_purchase_success'):SetVisible(false)
	GetWidget('lobby_popup_purchase_error'):SetVisible(false)
	GetWidget('lobby_popup_purchase_hero'):SetVisible(false)

	if _heroesInit then
		_heroesInit = false
	end
end

function Lobby_V2:SetUIStage(id)
	GetWidget('lobby_matchinfo'):SetVisible(id == 1)
	GetWidget('lobby_picker'):SetVisible(id == 2)
	GetWidget('lobby_loading'):SetVisible(id == 3)
end

function Lobby_V2:ShowHeroTip(heroName)
	if Empty(heroName) or GetWidget('lobby_picker_avatars'):IsVisible() then 
		_showHeroTips = false
		GetWidget('lobby_picker_herotip'):Sleep(100, function()
			if not _showHeroTips then 
				GetWidget('lobby_picker_herotip'):SetVisible(false)
			end
		end)

		return
	end

	--if compendium is showing, doesn't proc tooltip events if you select heroes behind the frame until its closed
	if not GetWidget('compendium'):IsVisible() then
		_showHeroTips = true
		GetWidget('lobby_picker_herotip'):SetVisible(true)
		interface:UICmd('GetHeroInfo(\''..heroName..'\');')
	end
end

function Lobby_V2:InitHeroes(gameMode, params)
	local function InitHero(mode, team, group, row, id)
		local numPerGroup = GROUP_HERO_NUM * TEAM_NUM
		local index = group * numPerGroup + team * GROUP_HERO_NUM + row * PERROW_NUM + id
		
		if row * PERROW_NUM + id >= GROUP_HERO_NUM then
			return -1, ''
		end

		local rootWidgetName = 'lobby_'.._uiStr..'_'..mode..'_hero_'..team..'_'..group..'_'..row..'_'..id
		GetWidget(rootWidgetName..'_filter'):SetVisible(false)
		GetWidget(rootWidgetName..'_mask'):SetVisible(false)
		return index, rootWidgetName
	end

	local function InitHero_SP(row, id)
		local index = row * SP_COLUMN_NUM + id

		local rootWidgetName = 'lobby_'.._uiStr..'_sp_hero_0_0_'..row..'_'..id
		GetWidget(rootWidgetName..'_filter'):SetVisible(false)
		GetWidget(rootWidgetName..'_mask'):SetVisible(false)
		return index, rootWidgetName
	end

	local function InitHero_SD(row, id, selfTeamIndex)
		local index = row * SD_COLUMN_NUM + id

		local rootWidgetName = 'lobby_'.._uiStr..'_sd_hero_0_0_'..row..'_'..id
		GetWidget(rootWidgetName..'_filter'):SetVisible(false)
		GetWidget(rootWidgetName..'_mask'):SetVisible(false)
		GetWidget(rootWidgetName..'_sd_mask'):SetVisible(row ~= selfTeamIndex)
		return index, rootWidgetName
	end

	local function InitHero_BD(group, row, id)
		local numberPerGroup = BD_ROW_NUM * BD_COLUMN_NUM
		local index = numberPerGroup * group + BD_COLUMN_NUM * row + id

		local rootWidgetName = 'lobby_'.._uiStr..'_bd_hero_0_'..group..'_'..row..'_'..id
		GetWidget(rootWidgetName..'_filter'):SetVisible(false)
		GetWidget(rootWidgetName..'_mask'):SetVisible(false)
		return index, rootWidgetName
	end

	local function InitHero_RD(row, id)
		local index = row * RD_COLUMN_NUM + id

		local rootWidgetName = 'lobby_'.._uiStr..'_rd_hero_0_0_'..row..'_'..id
		GetWidget(rootWidgetName..'_filter'):SetVisible(false)
		GetWidget(rootWidgetName..'_mask'):SetVisible(false)
		return index, rootWidgetName
	end

	GetWidget('lobby_picker_avatars'):SetVisible(false)
	GetWidget('lobby_'.._uiStr..'_heroes'):SetVisible(true)
	GetWidget('lobby_picker_herobuttons'):SetVisible(true)
	GetWidget('lobby_picker_filter'):SetVisible(true)
	GetWidget('lobby_picker_filter_root'):SetVisible(true)

	GetWidget('lobby_'.._uiStr..'_ap_list'):SetVisible(false)
	GetWidget('lobby_'.._uiStr..'_sm_list'):SetVisible(false)
	GetWidget('lobby_'.._uiStr..'_sp_list'):SetVisible(false)
	GetWidget('lobby_'.._uiStr..'_sd_list'):SetVisible(false)
	GetWidget('lobby_'.._uiStr..'_bd_list'):SetVisible(false)
	GetWidget('lobby_'.._uiStr..'_rd_list'):SetVisible(false)
	
	if gameMode == 'sm' then
		GetWidget('lobby_'.._uiStr..'_sm_list'):SetVisible(true)

		for i=0, TEAM_NUM-1 do
			for j=0,GROUP_NUM-1 do
				for k=0,ROW_NUM-1 do
					for m=0,PERROW_NUM-1 do
						local index, rootWidgetName = InitHero('sm', i, j, k, m)
						_heroInfos[index+1] = {}
						_heroInfos[index+1].rootWidgetName = rootWidgetName
					end
				end
			end
		end
	elseif gameMode == 'sp' then
		GetWidget('lobby_'.._uiStr..'_sp_list'):SetVisible(true)
		GetWidget('lobby_picker_filter_root'):SetVisible(false)

		for i=0, SP_ROW_NUM-1 do
			for j=0, SP_COLUMN_NUM-1 do
				local index, rootWidgetName = InitHero_SP(i, j)
				_heroInfos[index+1] = {}
				_heroInfos[index+1].rootWidgetName = rootWidgetName
			end
		end
	elseif gameMode == 'sd' then
		GetWidget('lobby_'.._uiStr..'_sd_list'):SetVisible(true)
		GetWidget('lobby_picker_filter_root'):SetVisible(false)

		for i=0, SD_ROW_NUM-1 do
			for j=0, SD_COLUMN_NUM-1 do
				local selfTeamIndex = tonumber(params[15])
				local index, rootWidgetName = InitHero_SD(i, j, selfTeamIndex)
				_heroInfos[index+1] = {}
				_heroInfos[index+1].rootWidgetName = rootWidgetName
			end
		end
	elseif gameMode == 'bd' then
		GetWidget('lobby_'.._uiStr..'_bd_list'):SetVisible(true)
		GetWidget('lobby_picker_filter_root'):SetVisible(false)

		for i=0, BD_GROUP_NUM-1 do
			for j=0, BD_ROW_NUM-1 do
			 	for k=0, BD_COLUMN_NUM-1 do
			 		local index, rootWidgetName = InitHero_BD(i, j, k)
			 		_heroInfos[index+1] = {}
					_heroInfos[index+1].rootWidgetName = rootWidgetName
			 	end
			end 
		end
	elseif gameMode == 'rd' then
		GetWidget('lobby_'.._uiStr..'_rd_list'):SetVisible(true)
		GetWidget('lobby_picker_filter_root'):SetVisible(false)

		for i=0, RD_ROW_NUM-1 do
			for j=0, RD_COLUMN_NUM-1 do
				local index, rootWidgetName = InitHero_RD(i, j)
				_heroInfos[index+1] = {}
				_heroInfos[index+1].rootWidgetName = rootWidgetName
			end
		end
	else
		GetWidget('lobby_'.._uiStr..'_ap_list'):SetVisible(true)
		GetWidget('lobby_picker_filter_root'):SetVisible(gameMode ~= 'lp')
		GetWidget('lobby_'.._uiStr..'_ap_list_lock'):SetVisible(gameMode == 'lp')

		for i=0, TEAM_NUM-1 do
			for j=0,GROUP_NUM-1 do
				for k=0,ROW_NUM-1 do
					for m=0,PERROW_NUM-1 do
						local index, rootWidgetName = InitHero('ap', i, j, k, m)
						_heroInfos[index+1] = {}
						_heroInfos[index+1].rootWidgetName = rootWidgetName
					end
				end
			end
		end
	end	
end

function Lobby_V2:UpdateSpectatorsList()
	local scrollWidget = GetWidget('lobby_matchinfo_spectators_scroll')
	local startIndex = tonumber(scrollWidget:GetValue())
	local maxValue = tonumber(scrollWidget:UICmd("GetScrollbarMaxValue()"))
	if startIndex < 1 then startIndex = 1 end
	if startIndex > maxValue then startIndex = maxValue end

	for i=1, SPECTATOR_MAX do
		local spectator = _spectators[startIndex+i-1]

		if spectator == nil then
			GetWidget('lobby_matchinfo_spectator_'..i):SetVisible(false)
		else
			GetWidget('lobby_matchinfo_spectator_'..i):SetVisible(true)

			local name = spectator.name
			local accountId = spectator.accountId
			local icon = spectator.icon
			local showBecomeRefereeBtn = spectator.showBecomeRefereeBtn
			local font = NotEmpty(spectator.chatNameColorFont) and spectator.chatNameColorFont..'_11' or 'dyn_12'
			local color = NotEmpty(spectator.chatNameColor) and spectator.chatNameColor or '#a99081'
			local outline = not NotEmpty(spectator.chatNameColor)
			local glow = spectator.chatNameGlow
			local glowColor = spectator.chatNameGlowColor

			if font == '8bit_11' then
				name = string.upper(name)
			end

			local widgetArrary = {'lobby_matchinfo_spectator_'..i..'_name', 'lobby_matchinfo_spectator_'..i..'_tip_value'}
			for _,v in ipairs(widgetArrary) do
				local widget = GetWidget(v)
				widget:SetColor(color)
				widget:SetFont(font)
				widget:SetOutline(outline)
				widget:SetGlow(glow)
				widget:SetBackgroundGlow(spectator.chatNameBackgroundGlow)
				widget:SetGlowColor(glowColor)
			end

			local needTip = Common_V2:SetLongText(GetWidget('lobby_matchinfo_spectator_'..i..'_name'), name)
			GetWidget('lobby_matchinfo_spectator_'..i..'_tip_root'):SetVisible(needTip)
			GetWidget('lobby_matchinfo_spectator_'..i..'_tip_value'):SetText(name)

			if NotEmpty(icon) then
				GetWidget('lobby_matchinfo_spectator_'..i..'_icon'):SetTexture(icon)
			else
				GetWidget('lobby_matchinfo_spectator_'..i..'_icon'):SetAvatar('http://www.heroesofnewerth.com/getAvatar.php?id='..accountId)
			end

			GetWidget('lobby_matchinfo_spectator_'..i..'_becomereferee'):SetVisible(showBecomeRefereeBtn)
			GetWidget('lobby_matchinfo_spectator_'..i..'_removereferee'):SetVisible(false)

			GetWidget('lobby_matchinfo_spectator_'..i..'_becomereferee'):SetCallback('onclick', function()
				interface:UICmd('PromoteRef('..spectator.clientNum..');')
				PlaySound('/shared/sounds/ui/revamp/region_click.wav')
			end)

			GetWidget('lobby_matchinfo_spectator_'..i):SetCallback('onrightclick', function()
				local menutable = {}
				local menu = {}
				if spectator.iAmHost and spectator.canBeKicked then 
					menu = {}
					menu.content = 'game_lobby_kick_tip'
					menu.onclicklua = 'Lobby_V2:KickPlayer('..spectator.clientNum..')'
					table.insert(menutable, menu)
				end
				Common_V2:PopupMenu(menutable)
			end)
		end
	end
end

function Lobby_V2:AssignAsSpectator(clientNum)
	interface:UICmd('RequestAssignSpectator('..clientNum..');')
end

function Lobby_V2:AssignAsReferee(clientNum)
	interface:UICmd('PromoteRef('..clientNum..');')
end

function Lobby_V2:AssignAsHost(clientNum)
	interface:UICmd('RequestAssignHost('..clientNum..');')
end

function Lobby_V2:KickPlayer(clientNum)
	interface:UICmd('Kick('..clientNum..');')
end

function Lobby_V2:UpdateOneHero(index)
	local hero = _heroInfos[index]
	if hero == nil then return end

	local rootWidgetName = hero.rootWidgetName
	local heroName = hero.heroName
	local iconPath = hero.iconPath
	local heroState = hero.heroState
	local isValid = hero.isValid
	local canSelect = hero.canSelect
	local action = hero.action
	local teamStr = hero.teamStr
	local isEarlyAccess = hero.isEarlyAccess
	local isFreeHero = hero.isFreeHero
	local hasPurchaseButton = hero.hasPurchaseButton
	local needToken = hero.needToken
	local earlyAccessCost = hero.earlyAccessCost
	local earlyAccessPremium = hero.earlyAccessPremium
	local earlyAccessPremiumCost = hero.earlyAccessPremiumCost
	local tokenSatisfied = hero.tokenSatisfied
	local isSoloMode = hero.isSoloMode
	local isVoteBlindBan = hero.isVoteBlindBan

	if Empty(heroName) then
		GetWidget(rootWidgetName..'_content'):SetVisible(false)
		return
	end
	GetWidget(rootWidgetName..'_content'):SetVisible(true)

 	if isValid then
		if heroState ~= -2 then
			GetWidget(rootWidgetName..'_icon'):SetTexture(iconPath)
		else
			if teamStr == 'legion' or teamStr == 'dev_legion' then
				GetWidget(rootWidgetName..'_icon'):SetTexture('/ui/fe2/lobby/empty_hero_slot_legion.tga')
			elseif teamStr == 'hellbourne' or teamStr == 'dev_hellbourne' then
				GetWidget(rootWidgetName..'_icon'):SetTexture('/ui/fe2/lobby/empty_hero_slot_hellbourne.tga')
			else
				GetWidget(rootWidgetName..'_icon'):SetTexture('/ui/fe2/lobby/empty_hero_slot_legion.tga')
			end
		end
	else
		if (team == -1) then
			GetWidget(rootWidgetName..'_icon'):SetTexture('/ui/elements/hero_picker_icon.tga')
		elseif (team == 0) then
			GetWidget(rootWidgetName..'_icon'):SetTexture('/ui/fe2/lobby/empty_hero_slot_legion.tga')
		elseif (team == 1) then
			GetWidget(rootWidgetName..'_icon'):SetTexture('/ui/fe2/lobby/empty_hero_slot_hellbourne.tga')
		end
	end

	if ((not isValid) or 
		(heroState == -5) or 
		(canSelect and ((heroState ~= -1 and heroState ~= -3 and heroState ~= 0) or tokenSatisfied or needToken))) and not isVoteBlindBan then
		GetWidget(rootWidgetName..'_icon'):SetRenderMode('normal')
		GetWidget(rootWidgetName..'_border'):SetVisible(true)
	else
		GetWidget(rootWidgetName..'_icon'):SetRenderMode('grayscale')
		GetWidget(rootWidgetName..'_border'):SetVisible(false)
	end

	if isFreeHero then
		GetWidget(rootWidgetName..'_tag'):SetTexture('/ui/fe2/newui/res/lobby/free.png')
	elseif isEarlyAccess then
		GetWidget(rootWidgetName..'_tag'):SetTexture('/ui/fe2/newui/res/lobby/new.png')
	else
		GetWidget(rootWidgetName..'_tag'):SetTexture('$invis')
	end

	GetWidget(rootWidgetName..'_vote'):SetVisible(isVoteBlindBan)
	GetWidget(rootWidgetName..'_ban'):SetVisible(heroState == -1)
	GetWidget(rootWidgetName..'_lock'):SetVisible(heroState == -5)
	GetWidget(rootWidgetName..'_tag'):SetVisible(canSelect)
	GetWidget(rootWidgetName..'_mask'):SetVisible(heroState == -1 or heroState == -5)
	GetWidget(rootWidgetName..'_selected'):SetVisible(index == _selectedHero and heroState ~= -1 and heroState ~= -5) --and canSelect)

	local avatarCode = GetDBEntry('def_av_'..heroName, nil , false) or ''
	if NotEmpty(avatarCode) then
		SetDefaultAvatar(heroName, avatarCode)
	end
end

function Lobby_V2:SelectHero(index)
	-- Update Selected Effect
	if index ~= _selectedHero then
		local lastSelection = _selectedHero
		_selectedHero = index
		Lobby_V2:UpdateOneHero(lastSelection)
		Lobby_V2:UpdateOneHero(_selectedHero)	
	end

	local hero = _heroInfos[_selectedHero]
	local selectBtn = GetWidget('lobby_selecthero')
	local buyBtn = GetWidget('lobby_buyhero')

	local avatarCode = GetDBEntry('def_av_'..hero.heroName, nil , false) or ''
	
	if hero.action == 'SpawnHero' then 
		interface:UICmd('PotentialHero(\'' .. hero.heroName .. '\', \''..avatarCode..'\');')
	end

	if hero.canSelect and hero.heroState > 0 then
		selectBtn:SetEnabled(not hero.isVoteBlindBan)

		selectBtn:SetCallback('onclick', function()
 			interface:UICmd(hero.action .. '(\'' .. hero.heroName .. '\');')
 			PlaySound('/shared/sounds/ui/revamp/region_click.wav')
			if hero.action == 'VoteBanHero' then
				_selectedHero = -1
				_selectedCandidate = -1
				_selectedLockedHero = -1
			end
 		end)
	else
		selectBtn:SetEnabled(false)
	end

	if hero.hasPurchaseButton then 
		buyBtn:SetVisible(true)
		buyBtn:SetCallback('onclick', function()
			if hero.isEarlyAccess then 
				Lobby_V2:PurchaseEAP(hero)
			else
				Lobby_V2:PurchaseHero(hero)
			end
			PlaySound('/shared/sounds/ui/revamp/region_click.wav')
 		end)
 	else
 		buyBtn:SetVisible(false)
 	end

 	if hero.hasPurchaseButton then
	 	local gold = hero.goldPrice or 0
	 	local silver = hero.silverPrice or 0

	 	GetWidget('lobby_picker_herobuttons_coins'):SetVisible(true)
	 	GetWidget('lobby_picker_herobuttons_coins_gold'):SetVisible(gold > 0)
	 	GetWidget('lobby_picker_herobuttons_coins_silver'):SetVisible(silver > 0)

	 	local widget = GetWidget('lobby_picker_herobuttons_coins_gold_text')
	 	local str = tostring(gold) 
	 	widget:SetWidth(widget:GetStringWidth(str))
	 	widget:SetText(str)

	 	widget = GetWidget('lobby_picker_herobuttons_coins_silver_text')
	 	str = tostring(silver) 
	 	widget:SetWidth(widget:GetStringWidth(str))
	 	widget:SetText(str)
	else
		GetWidget('lobby_picker_herobuttons_coins'):SetVisible(false)
	end
end

function Lobby_V2:UpdateSoloCandidate(index)
	local hero = _soloCandidates[index]
	if hero == nil then return end

	local heroName = hero.heroName
	local heroIcon = hero.heroIcon
	local bEnabled = hero.bEnabled
	local banned = hero.banned
	local action = hero.action
	local blindPick = hero.blindPick

	if blindPick then
		GetWidget('lobby_solo_'.._uiStr..'_candidate_'..index..'_heroicon'):SetTexture('/ui/fe2/lobby/mystery.tga')
	elseif NotEmpty(heroName) then
		GetWidget('lobby_solo_'.._uiStr..'_candidate_'..index..'_heroicon'):SetTexture(heroIcon)
		GetWidget('lobby_solo_'.._uiStr..'_candidate_'..index..'_btn'):SetCallback('onmouseover', function() Lobby_V2:ShowHeroTip(heroName) end)
		GetWidget('lobby_solo_'.._uiStr..'_candidate_'..index..'_btn'):SetCallback('onmouseout', function() Lobby_V2:ShowHeroTip() end)
	else
		GetWidget('lobby_solo_'.._uiStr..'_candidate_'..index..'_heroicon'):SetTexture('$invis')
	end

	GetWidget('lobby_solo_'.._uiStr..'_candidate_'..index..'_ban'):SetVisible(banned)
	GetWidget('lobby_solo_'.._uiStr..'_candidate_'..index..'_selected'):SetVisible(_selectedCandidate == index and bEnabled)

	if bEnabled then
		GetWidget('lobby_solo_'.._uiStr..'_candidate_'..index..'_heroicon'):SetRenderMode('normal')
		GetWidget('lobby_solo_'.._uiStr..'_candidate_'..index..'_btn'):SetVisible(true)
	else
		GetWidget('lobby_solo_'.._uiStr..'_candidate_'..index..'_heroicon'):SetRenderMode('grayscale')
		GetWidget('lobby_solo_'.._uiStr..'_candidate_'..index..'_btn'):SetVisible(false)
	end
end

function Lobby_V2:UpdateLockedHero(index)
	local hero = _lockedHeroes[index]
	if hero == nil then return end

	local rootWidgetName = 'lobby_'.._uiStr..'_lp_hero_0_0_0_'..index
	local heroName = hero.heroName
	local iconPath = hero.iconPath
	local heroState = hero.heroState
	local isValid = hero.isValid
	local canSelect = hero.canSelect
	local action = hero.action
	local teamStr = hero.teamStr
	local isEarlyAccess = hero.isEarlyAccess
	local isFreeHero = hero.isFreeHero
	local hasPurchaseButton = hero.hasPurchaseButton
	local needToken = hero.needToken
	local earlyAccessCost = hero.earlyAccessCost
	local earlyAccessPremium = hero.earlyAccessPremium
	local earlyAccessPremiumCost = hero.earlyAccessPremiumCost
	local tokenSatisfied = hero.tokenSatisfied
	local isSoloMode = hero.isSoloMode
	local isVoteBlindBan = hero.isVoteBlindBan

	if Empty(heroName) then
		GetWidget(rootWidgetName..'_content'):SetVisible(false)
		return
	end
	GetWidget(rootWidgetName..'_content'):SetVisible(true)

 	if isValid then
		if heroState ~= -2 then
			GetWidget(rootWidgetName..'_icon'):SetTexture(iconPath)
		else
			if teamStr == 'legion' or teamStr == 'dev_legion' then
				GetWidget(rootWidgetName..'_icon'):SetTexture('/ui/fe2/lobby/empty_hero_slot_legion.tga')
			elseif teamStr == 'hellbourne' or teamStr == 'dev_hellbourne' then
				GetWidget(rootWidgetName..'_icon'):SetTexture('/ui/fe2/lobby/empty_hero_slot_hellbourne.tga')
			else
				GetWidget(rootWidgetName..'_icon'):SetTexture('/ui/fe2/lobby/empty_hero_slot_legion.tga')
			end
		end
	else
		if (team == -1) then
			GetWidget(rootWidgetName..'_icon'):SetTexture('/ui/elements/hero_picker_icon.tga')
		elseif (team == 0) then
			GetWidget(rootWidgetName..'_icon'):SetTexture('/ui/fe2/lobby/empty_hero_slot_legion.tga')
		elseif (team == 1) then
			GetWidget(rootWidgetName..'_icon'):SetTexture('/ui/fe2/lobby/empty_hero_slot_hellbourne.tga')
		end
	end

	if ((not isValid) or 
		(heroState == -5) or 
		(canSelect and ((heroState ~= -1 and heroState ~= -3 and heroState ~= 0) or tokenSatisfied or needToken))) and not isVoteBlindBan then
		GetWidget(rootWidgetName..'_icon'):SetRenderMode('normal')
		GetWidget(rootWidgetName..'_border'):SetVisible(true)
	else
		GetWidget(rootWidgetName..'_icon'):SetRenderMode('grayscale')
		GetWidget(rootWidgetName..'_border'):SetVisible(false)
	end

	if isFreeHero then
		GetWidget(rootWidgetName..'_tag'):SetTexture('/ui/fe2/newui/res/lobby/free.png')
	elseif isEarlyAccess then
		GetWidget(rootWidgetName..'_tag'):SetTexture('/ui/fe2/newui/res/lobby/new.png')
	else
		GetWidget(rootWidgetName..'_tag'):SetTexture('$invis')
	end

	GetWidget(rootWidgetName..'_ban'):SetVisible(false)
	GetWidget(rootWidgetName..'_lock'):SetVisible(false)
	GetWidget(rootWidgetName..'_tag'):SetVisible(canSelect)
	GetWidget(rootWidgetName..'_mask'):SetVisible(not canSelect)
	GetWidget(rootWidgetName..'_selected'):SetVisible(index == _selectedLockedHero and canSelect)

	local avatarCode = GetDBEntry('def_av_'..heroName, nil , false) or ''
	if NotEmpty(avatarCode) then
		SetDefaultAvatar(heroName, avatarCode)
	end
end

function Lobby_V2:InitBotsPanel()
	_validBotList = {}
	_validBotList[1] = {}
	_validBotList[1].sHeroName = 'random'

	local seletedBot = {}
	for i=0,PLAYER_MAXCOUNT-1 do
		if NotEmpty(_selectedBot[i]) then
			seletedBot[_selectedBot[i]] = true
		end
	end

	for i,v in ipairs(_allBotList) do
		if not seletedBot[v.sPlayerName] then
			table.insert(_validBotList, v)
		end
	end

	local scrollWidget = GetWidget('lobby_matchinfo_addbot_scroll')
	local count = #_validBotList
	local line = 0
	if (count % BOT_COLUMN_NUM) == 0 then
		line = count / BOT_COLUMN_NUM
	else
		line = math.ceil(count/BOT_COLUMN_NUM)
	end

	if line <= BOT_ROW_NUM then
		scrollWidget:SetVisible(false)
		scrollWidget:SetMaxValue(1)
	else
		scrollWidget:SetVisible(true)
		local scrollMaxValue = line - BOT_ROW_NUM + 1
		scrollWidget:SetMaxValue(scrollMaxValue)
	end

	scrollWidget:SetValue(1)
	Lobby_V2:UpdteBotsPanel()
end

function Lobby_V2:UpdteBotsPanel()
	local scrollWidget = GetWidget('lobby_matchinfo_addbot_scroll')
	local startIndex = tonumber(scrollWidget:GetValue())
	local maxValue = tonumber(scrollWidget:UICmd("GetScrollbarMaxValue()"))
	if startIndex < 1 then startIndex = 1 end
	if startIndex > maxValue then startIndex = maxValue end

	for i=0, BOT_ROW_NUM-1 do
		for j=0, BOT_COLUMN_NUM-1 do
			local index = (startIndex -1 + i)*BOT_COLUMN_NUM + j
			local bot = _validBotList[index+1] 
			if bot ~= nil then
				GetWidget('lobby_matchinfo_bot_'..i..'_'..j):SetVisible(true)
				
				if bot.sHeroName == 'random' then 
					GetWidget('lobby_matchinfo_bot_'..i..'_'..j..'_icon'):SetTexture('/ui/elements/question_mark.tga')
				else
					GetWidget('lobby_matchinfo_bot_'..i..'_'..j..'_icon'):SetTexture(GetHeroIconPathFromProduct(bot.sHeroName))
				end
			else
				GetWidget('lobby_matchinfo_bot_'..i..'_'..j):SetVisible(false)
			end
		end
	end
end

function Lobby_V2:SetHeroInfo(heroInfo)
	local function GetHeroCategory(categoryInfo)
		local ratingMap = {
			solorating		= 'solo',
			junglerating	= 'jungle',
			carryrating		= 'carry',
			supportrating	= 'support',
			initiatorrating	= 'initiator',
			gankerrating	= 'ganker',
			pusherrating	= 'pusher'
		}

		local result = ''
		for k, v in pairs(ratingMap) do
			if categoryInfo[k] >= FILTER_THROSHOLD then
				local s = Translate('newui_lobby_filter_'..v)
				if result == '' then
					result = s
				else
					result = result..' '..s
				end
			end
		end
		return result..' '
	end

	GetWidget('lobby_picker_herotip_'.._uiStr..'_heroname'):SetText(heroInfo.name)
	GetWidget('lobby_picker_herotip_'.._uiStr..'_heroicon'):SetTexture(heroInfo.icon)

	local primAttrImage = '/ui/fe2/elements/store2/primary_none.png'
	if heroInfo.primaryAttri == 'strength' then 
		primAttrImage = '/ui/fe2/elements/store2/primary_strength.png'
	elseif heroInfo.primaryAttri == 'agility' then
		primAttrImage = '/ui/fe2/elements/store2/primary_agility.png'
	elseif heroInfo.primaryAttri == 'intelligence' then
		primAttrImage = '/ui/fe2/elements/store2/primary_intelligence.png'
	end
	GetWidget('lobby_picker_herotip_'.._uiStr..'_primattr'):SetTexture(primAttrImage)	

	local attackTypeImage = '/ui/fe2/elements/store2/melee.png'
	if heroInfo.attackType == 'ranged' then
		attackTypeImage = '/ui/fe2/elements/store2/ranged.png'
	end
	GetWidget('lobby_picker_herotip_'.._uiStr..'_attacktype'):SetTexture(attackTypeImage)	
	

	GetWidget('lobby_picker_herotip_'.._uiStr..'_category'):SetText(GetHeroCategory(heroInfo))

	local masteryLevel = GetMasteryLevelByExp(heroInfo.masteryExp)
	local masteryType = GetMasterTypeByLevel(masteryLevel)
	local masteryExpMin, masteryExpMax = GetMasteryExpByLevel(masteryLevel)
	local masteryPercent = math.floor((heroInfo.masteryExp - masteryExpMin) / (masteryExpMax - masteryExpMin) * 100 + 0.5)

	GetWidget('lobby_picker_herotip_'.._uiStr..'_mastery_badge'):SetTexture('/ui/fe2/newui/res/homepage/badge_'..masteryType..'.png')
	GetWidget('lobby_picker_herotip_'.._uiStr..'_mastery_level'):SetText(tostring(masteryLevel))

	if masteryLevel >= GetCvarInt('hero_mastery_maxlevel_new') then
		GetWidget('lobby_picker_herotip_'.._uiStr..'_mastery_exp'):SetWidth('100%')
	else
		GetWidget('lobby_picker_herotip_'.._uiStr..'_mastery_exp'):SetWidth(tostring(masteryPercent)..'%')
	end

	local masteryColor = ''
	if masteryType == 'iron' then
		masteryColor = '#ac7258'
	elseif masteryType == 'silver' then
		masteryColor = '#b7c9c9'
	elseif masteryType == 'gold' then
		masteryColor = '#d69200'
	else
		masteryColor = '#aa1fd2'
	end
	GetWidget('lobby_picker_herotip_'.._uiStr..'_mastery_exp'):SetColor(masteryColor)

	local difficulty = heroInfo.difficulty
	GetWidget('lobby_picker_herotip_'.._uiStr..'_difficulty_parent'):SetVisible(difficulty > 0)
	for i=1,5 do
		local difficultyImage = '/ui/fe2/elements/store2/DI2.png'
		if i <= difficulty then 
			difficultyImage = '/ui/fe2/elements/store2/DI.png'
		elseif i <= (difficulty + 0.5) then
			difficultyImage = '/ui/fe2/elements/store2/DI_h.png'
		end
		GetWidget('lobby_picker_herotip_'.._uiStr..'_difficulty'..i):SetTexture(difficultyImage)
	end

	local strengthStr = '^o'..heroInfo.strength..'^* ( +'..heroInfo.strengthPerLevel..' )'
	local agilityStr = '^o'..heroInfo.agility..'^* ( +'..heroInfo.agilityPerLevel..' )'
	local intelligenceStr = '^o'..heroInfo.intelligence..'^* ( +'..heroInfo.intelligencePerLevel..' )'

	GetWidget('lobby_picker_herotip_'.._uiStr..'_strength'):SetText(strengthStr)
	GetWidget('lobby_picker_herotip_'.._uiStr..'_agility'):SetText(agilityStr)
	GetWidget('lobby_picker_herotip_'.._uiStr..'_intelligence'):SetText(intelligenceStr)

	GetWidget('lobby_picker_herotip_'.._uiStr..'_role'):SetText(heroInfo.role)

	-- Attributes
	-- 1. Attack Range
	local widget = GetWidget('lobby_picker_'.._uiStr..'_herotip_attributes_1')
	local strValue = Translate('store2_hero_select_label_attack_range', 'range', heroInfo.attackRange)
	widget:SetText(strValue)

	-- 2. Damage
	widget = GetWidget('lobby_picker_'.._uiStr..'_herotip_attributes_2')
	strValue = Translate('store2_hero_select_label_damage', 'damage', tostring(heroInfo.damageMin)..' - '..heroInfo.damageMax)
	widget:SetText(strValue)

	-- 3. Attack Speed
	widget = GetWidget('lobby_picker_'.._uiStr..'_herotip_attributes_3')
	strValue = Translate('store2_hero_select_label_attack_speed', 'speed', heroInfo.attackSpeed)
	widget:SetText(strValue)

	-- 4. Armor
	widget = GetWidget('lobby_picker_'.._uiStr..'_herotip_attributes_4')
	strValue = Translate('store2_hero_select_label_armor', 'armor', heroInfo.armor)
	widget:SetText(strValue)

	-- 5. Move Speed
	widget = GetWidget('lobby_picker_'.._uiStr..'_herotip_attributes_5')
	strValue = Translate('store2_hero_select_label_mvspeed', 'mvspeed', heroInfo.moveSpeed)
	widget:SetText(strValue)

	-- 6. Magic Armor
	widget = GetWidget('lobby_picker_'.._uiStr..'_herotip_attributes_6')
	strValue = Translate('store2_hero_select_label_magicarmor', 'armor', heroInfo.magicArmor)
	widget:SetText(strValue)
end

function Lobby_V2:SetHeroAbilityInfo(i, ability)
	local descStr = ability.desc

	GetWidget('lobby_picker_herotip_'.._uiStr..'_ability_'..i..'_icon'):SetTexture(ability.icon)
	GetWidget('lobby_picker_herotip_'.._uiStr..'_ability_'..i..'_name'):SetText(ability.name)

	-- mana cost
	if NotEmpty(ability.manaCost) then
		GetWidget('lobby_picker_herotip_'.._uiStr..'_ability_'..i..'_manacost'):SetText(Translate('heroinfo_mana_cost') .. ability.manaCost)
	else
		GetWidget('lobby_picker_herotip_'.._uiStr..'_ability_'..i..'_manacost'):SetText('')
	end

	-- range
	local rangeTitle = ''
	local rangeValue = ''
	local showRange = true
	if NotEmpty(ability.range) then
		rangeValue = ability.range
		rangeTitle = Translate('heroinfo_range')
	elseif NotEmpty(ability.targetRadius) then
		rangeValue = ability.targetRadius
		rangeTitle = Translate('heroinfo_radius')
	elseif NotEmpty(ability.auraRange) then
		rangeValue = ability.auraRange
		rangeTitle = Translate('heroinfo_aura')..' '..Translate('heroinfo_radius')
	else
		showRange = false
	end
	if showRange then
		descStr = descStr..'\n\n'..rangeTitle..rangeValue
	end

	-- cooldown\passive
	if NotEmpty(ability.cdTime) and not ability.isPassive then 
		if not showRange then 
			descStr = descStr..'\n'
		end

		descStr = descStr..'\n'..Translate('heroinfo_cooldown', 'time', ability.cdTime)
	end


	-- description
	-- GetWidget('lobby_picker_herotip_'.._uiStr..'_ability_'..i..'_desc'):SetText(descStr)
	-- GetWidget('lobby_picker_herotip_'.._uiStr..'_ability_'..i..'_desc'):SetY('0')

	-- local height = GetWidget('lobby_picker_herotip_'.._uiStr..'_ability_'..i..'_desc'):GetHeight()
	-- local maxHeight = GetWidget('lobby_picker_herotip_'.._uiStr..'_ability_'..i..'_descmax'):GetHeight()

	-- if height > maxHeight then
	-- 	local currentTime = GetTime()
	-- 	GetWidget('lobby_picker_herotip_'.._uiStr..'_ability_'..i..'_desc'):SetCallback('onframe', function() 
	-- 		local pastTime = GetTime() - currentTime
	-- 		local move = 20 - ((20 * pastTime/1000) % (height - maxHeight + 40))
	-- 		GetWidget('lobby_picker_herotip_'.._uiStr..'_ability_'..i..'_desc'):SetY(tostring(move))
	-- 	end)
	-- else
	-- 	GetWidget('lobby_picker_herotip_'.._uiStr..'_ability_'..i..'_desc'):ClearCallback('onframe')
	-- end

	local maxHeight = GetWidget('lobby_picker_herotip_'.._uiStr..'_ability_'..i..'_descmax'):GetHeight()
	for j=10, 6, -1 do
		GetWidget('lobby_picker_herotip_'.._uiStr..'_ability_'..i..'_desc'):SetFont('dyn_'..j)
		GetWidget('lobby_picker_herotip_'.._uiStr..'_ability_'..i..'_desc'):SetText(descStr)

		local height = GetWidget('lobby_picker_herotip_'.._uiStr..'_ability_'..i..'_desc'):GetHeight()
		if height <= maxHeight then break end

		GetWidget('lobby_picker_herotip_'.._uiStr..'_ability_'..i..'_desc'):SetText('hahaha')
		if j == 6 then
			GetWidget('lobby_picker_herotip_'.._uiStr..'_ability_'..i..'_desc'):SetText(descStr)
		end
	end
end

function Lobby_V2:TurnOnVoiceChat(playerName, clientNum)
	local stripedName = StripClanTag(playerName) 
	local ignore = AtoB(interface:UICmd([[ChatIsIgnored(']] .. stripedName .. [[')]]))
	if ignore then
		interface:UICmd([[ChatNotifyUnignoreFirst(']] .. stripedName .. [[')]])
	else
		interface:UICmd([[ToggleVoiceMute(']] .. clientNum .. [[')]])
	end
end

function Lobby_V2:TurnOffVoiceChat(clientNum)
	interface:UICmd([[ToggleVoiceMute(']] .. clientNum .. [[')]])
end

function Lobby_V2:AdjustSwapPosPanel()
	local screenW = GetScreenWidth()
	local screenH = GetScreenHeight()
	local widget = GetWidget('lobby_matchinfo_swappos')
	local x = widget:GetAbsoluteX()
	local y = widget:GetAbsoluteY()
	local w = widget:GetWidth()
	local h = widget:GetHeight()

	if x + w > screenW then
		widget:SetAlign('right')
		widget:SetX('-5i')
		widget:GetParent():GetParent():SetX('-10i')
	else
		widget:SetAlign('left')
		widget:SetX('5i')
		widget:GetParent():GetParent():SetX('100%')
	end

	if y + h > screenH then
		widget:SetVAlign('bottom')
	else
		widget:SetVAlign('top')
	end
end

function Lobby_V2:UpdateArrowAnim(widget, index, hflip)
	local hFlip = AtoB(hflip)
	local TIME_ELAPSE = 1000
	local GAP = 10

	local time = GetTime() % TIME_ELAPSE
	local x = time / TIME_ELAPSE * GAP * 3
	x = x + GAP * index
	x = x % (GAP * 3)

	if hFlip then
		widget:SetAlign('right')
		x = 0 - x
	else
		widget:SetAlign('left')
	end

	local alpha = math.abs(math.sin(x/ (GAP * 3) * math.pi))
	widget:SetX(tostring(x)..'i')
	widget:SetColor('1 1 1 '..alpha)
end

function Lobby_V2:UpdateArrowImage(widget, ...)
	local gamePhase = tonumber(arg[41]) 
	
	if gamePhase ~=2 and gamePhase ~= 9 and gamePhase ~= 10 and gamePhase ~= 12 then
		widget:SetTexture('/ui/fe2/newui/res/lobby/a_green.png')
	elseif gamePhase == 2 or gamePhase == 12 then
		widget:SetTexture('/ui/fe2/newui/res/lobby/a_red.png')
	elseif gamePhase == 10 or gamePhase == 9 then
		widget:SetTexture('/ui/fe2/newui/res/lobby/a_yellow.png')
	end
end

function Lobby_V2:UpdateGamePhase(label, gamePhase)
	if gamePhase ~=2 and gamePhase ~= 9 and gamePhase ~= 10 and gamePhase ~= 12 then
		label:SetColor('#30e406')
	elseif gamePhase == 2 or gamePhase == 12 then
		label:SetColor('#fa2504')
	elseif gamePhase == 10 or gamePhase == 9 then
		label:SetColor('#ffc403')
	end

	Lobby_V2:SetSelectHeroButtonText(Translate('newui_lobby_button_pick'))
	if gamePhase == 2 then
		label:SetText(Translate('game_lobby_banning'))
		Lobby_V2:SetSelectHeroButtonText(Translate('newui_lobby_button_ban'))
	elseif gamePhase == 9 then
		label:SetText(Translate('game_lobby_hidden_banning'))
		Lobby_V2:SetSelectHeroButtonText(Translate('newui_lobby_button_ban'))
	elseif gamePhase == 10 then
		label:SetText(Translate('game_lobby_locking'))
		Lobby_V2:SetSelectHeroButtonText(Translate('newui_lobby_button_lock'))
	elseif gamePhase == 12 then
		label:SetText(Translate('game_lobby_shuffle_picking'))
	elseif gamePhase == 13 then
		label:SetText(Translate('game_lobby_vote_banning'))
		Lobby_V2:SetSelectHeroButtonText(Translate('newui_lobby_button_vote'))
	elseif gamePhase == 3 or gamePhase == 11 then
		if _currentGameMode == 'cm' then
			label:SetText(Translate('game_lobby_captains_pick'))
		else
			label:SetText(Translate('game_lobby_picking'))
		end
	else
		label:SetText('')
	end
end

function Lobby_V2:SetSelectHeroButtonText(text, font)
	for i=1,4 do
		GetWidget('lobby_selecthero_text'..i):SetText(text)
		if NotEmpty(font) then
			GetWidget('lobby_selecthero_text'..i):SetFont(font)
		end
	end
end

function Lobby_V2:PlayNotification(widget, type)
	local sound = ''
	if type == 'ban' then
		sound = '/shared/sounds/announcer/lobby/ban_hero.wav'
	elseif type == 'pick' then
		sound = '/shared/sounds/announcer/lobby/pick_hero.wav'
	end

	if NotEmpty(sound) then
		if _playingMapSound then
			widget:Sleep(30, function() Lobby_V2:PlayNotification(widget, type) end)
		else
			widget:Sleep(2200, function() PlaySound(sound) end)
		end
	end
end
-------------------------------- Trigger Handler --------------------------------------
function Lobby_V2:OnGamePhase(gamePhase)
	gamePhase = tonumber(gamePhase)
	if gamePhase == 0 then 
		Lobby_V2:Hide()
	elseif gamePhase == 1 then 
		Lobby_V2:Show()
		Lobby_V2:SetUIStage(1)
	elseif gamePhase == 2 or gamePhase == 3 or gamePhase == 3 or (gamePhase >= 9 and gamePhase <= 13) then 
		Lobby_V2:Show()
		Lobby_V2:SetUIStage(2)
	elseif gamePhase == 4 then 
		Lobby_V2:Show()
		Lobby_V2:SetUIStage(3)
	elseif gamePhase == 5 then 
		Lobby_V2:Hide()
	else
		Lobby_V2:Hide()
	end
end

function Lobby_V2:OnHeroSelectInfo(...)
	if not _heroesInit then
		_currentGameMode = arg[3]

		Echo('^g Init Hero ~~~~~~~~~~~~~~~~~~~~~~~~~~~~ '.._currentGameMode)

		_heroInfos = {}
		_soloCandidates = {}
		_lockedHeroes = {}
		_selectedHero = -1
		_selectedCandidate = -1
		_selectedLockedHero = -1
		_myGameHero = ''
		_heroesInit = true
		_favoriteHeroes = GetDBEntry('picker_favorite_heroes', nil, nil) or {}
		_playingMapSound = false

		Lobby_V2:OnClickResetFilter()

		Lobby_V2:InitHeroes(_currentGameMode, arg)
		GetWidget('lobby_selecthero'):SetEnabled(false)

		Trigger('StoreAvatarIsNewRequest')

		SubmitForm('MicroStore',
					'account_id', Client.GetAccountID(),
					'category_id', '58',
					'request_code', '1',
					'page', '1',
					'cookie', Client.GetCookie(),
					'hostTime', GetTime(),
					'displayAll', 'false',
					'notPurchasable', 'false'
				)

		SubmitForm('PlayerStatsMastery', 'f', 'show_stats', 'nickname', StripClanTag(GetAccountName()), 'cookie', Client.GetCookie(), 'table', 'mastery')

		-- Play Sound
		local soundMap = arg[17]
		local soundMode = _currentGameMode

		if soundMode == 'bm' or soundMode == 'km' then
			soundMode = 'ap'
		end

		if soundMode == 'ap' and IsCoNGame() then
			soundMode = 'cp' 
		elseif AtoB(arg[18]) then
			soundMode = 'gt'
		end

		if GAMEMAP_SOUND[soundMap] and NotEmpty(GAMEMAP_SOUND[soundMap].sound) then
			PlaySound(GAMEMAP_SOUND[soundMap].sound)
			_playingMapSound = true
		
			if GAMEMAP_SOUND[soundMap].announceMode and not ViewingStreaming() then
				GetWidget('lobby_trigger_help'):Sleep(GAMEMAP_SOUND[soundMap].length, function() 
					_playingMapSound = false
					if NotEmpty(GAMEMODE_SOUND[soundMode]) then
						PlaySound(GAMEMODE_SOUND[soundMode])
					end
				end)
			end
		else
			if NotEmpty(GAMEMODE_SOUND[soundMode]) and not ViewingStreaming() then
				PlaySound(GAMEMODE_SOUND[soundMode])
			end
		end
	end
end

function Lobby_V2:OnStoreAvatarIsNewResult(productID, isNew, isUltimate)
	_avatarsNewUltimateInfo[productID] = {isNew = AtoB(isNew), isUltimate = AtoB(isUltimate)}
end

function Lobby_V2:OnHeroSelectPlayerInfo(index, sourceWidget, ...)
	local clientNum = arg[3]

	if clientNum == '-1' then 
		GetWidget('lobby_'.._uiStr..'_player_'..index):SetVisible(false)
		return
	end

	local isMe = AtoB(arg[2])
	local playerName = arg[4]
	local heroName = arg[6]
	local heroIcon = arg[7]
	local canRepick = AtoB(arg[8])
	local canSwap = AtoB(arg[9])
	local alreadySwapToMe = AtoB(arg[10])
	local isReady = AtoB(arg[11])
	local isTeamMate = AtoB(arg[12])
	local alreadySwap = AtoB(arg[13])
	local heroDefName = arg[14]
	local hasSelectedHero = AtoB(arg[27])

	-- Base Information
	if Empty(heroIcon) or hasSelectedHero then
		GetWidget('lobby_'.._uiStr..'_player_'..index..'_heroicon'):SetRenderMode('normal')
	else
		GetWidget('lobby_'.._uiStr..'_player_'..index..'_heroicon'):SetRenderMode('grayscale')
	end

	if Empty(heroIcon) then heroIcon = '/ui/fe2/lobby/mystery.tga' end

	GetWidget('lobby_'.._uiStr..'_player_'..index):SetVisible(true)

	GetWidget('lobby_'.._uiStr..'_player_'..index..'_heroname'):SetText(heroName)
	GetWidget('lobby_'.._uiStr..'_player_'..index..'_heroicon'):SetTexture(heroIcon)
	GetWidget('lobby_'.._uiStr..'_player_'..index..'_ready_bg'):SetVisible(hasSelectedHero)
	GetWidget('lobby_'.._uiStr..'_player_'..index..'_notready_bg'):SetVisible(not hasSelectedHero)
	GetWidget('lobby_'.._uiStr..'_player_'..index..'_selfeffect'):SetVisible(isMe)

	-- Swap Button
	if not isMe and canSwap and not alreadySwapToMe and isTeamMate and not alreadySwap then
		GetWidget('lobby_'.._uiStr..'_player_'..index..'_swap'):SetVisible(true)
	else
		GetWidget('lobby_'.._uiStr..'_player_'..index..'_swap'):SetVisible(false)
	end


	-- Buttons States
	GetWidget('lobby_'.._uiStr..'_player_'..index..'_portrait'):SetCallback('onclick', function() end)

	if NotEmpty(heroDefName) then
		GetWidget('lobby_'.._uiStr..'_player_'..index..'_portrait'):SetCallback('onmouseover', function()
			if not GetWidget('lobby_picker_avatars'):IsVisible() then
				Lobby_V2:ShowHeroTip(heroDefName)
				GetWidget('communicator_main_large_scale'):SetVisible(false)
			end
		end)
		GetWidget('lobby_'.._uiStr..'_player_'..index..'_portrait'):SetCallback('onmouseout', function()
			if not GetWidget('lobby_picker_avatars'):IsVisible() then
				Lobby_V2:ShowHeroTip()
				GetWidget('communicator_main_large_scale'):FadeIn(250)
			end
		end)
	else
		GetWidget('lobby_'.._uiStr..'_player_'..index..'_portrait'):SetCallback('onmouseover', function() end)
		GetWidget('lobby_'.._uiStr..'_player_'..index..'_portrait'):SetCallback('onmouseout', function() end)
	end

	if isMe then
		if hasSelectedHero then
			GetWidget('lobby_repick'):SetVisible(canRepick)
			GetWidget('lobby_random'):SetVisible(false)

			GetWidget('lobby_'.._uiStr..'_player_'..index..'_portrait'):SetCallback('onclick', function()
				Lobby_V2:OpenAvatars(heroDefName)
				Lobby_V2:ShowHeroTip()
				GetWidget('communicator_main_large_scale'):FadeIn(250)
			end)
			
			if _myGameHero ~= heroDefName and not _pickHeroByAvatar then
				local avatarCode = GetDBEntry('def_av_'..heroDefName, nil , false) or ''
				if NotEmpty(avatarCode) then
					SelectAvatar(avatarCode)
				end
				_myGameHero = heroDefName
				Lobby_V2:OpenAvatars(heroDefName)
			end
			_pickHeroByAvatar = false

			_myGameHero = heroDefName
		else
			_myGameHero = ''
			GetWidget('lobby_repick'):SetVisible(false)
			GetWidget('lobby_random'):SetVisible(true)	
		end
	end

	-- ReadyButton
	if isMe then
		GetWidget('lobby_ready'):SetEnabled(not isReady and hasSelectedHero)
	end
end

function Lobby_V2:OnHeroSelectMastery(index, type)
	GetWidget('lobby_'.._uiStr..'_player_'..index..'_ready_bg'):SetTexture('/ui/fe2/newui/res/lobby/'.._uiStr..'_player_ready_'..type..'.png')
	GetWidget('lobby_'.._uiStr..'_player_'..index..'_notready_bg'):SetTexture('/ui/fe2/newui/res/lobby/'.._uiStr..'_player_notready_'..type..'.png')
	GetWidget('lobby_'.._uiStr..'_player_'..index..'_border'):SetTexture('/ui/fe2/newui/res/lobby/border_'..type..'.png')
	GetWidget('lobby_'.._uiStr..'_player_'..index..'_swap_image1'):SetTexture('/ui/fe2/newui/res/lobby/swap_'..type..'.png')
	GetWidget('lobby_'.._uiStr..'_player_'..index..'_swap_image2'):SetTexture('/ui/fe2/newui/res/lobby/swap_'..type..'_over.png')
	GetWidget('lobby_'.._uiStr..'_player_'..index..'_swap_image3'):SetTexture('/ui/fe2/newui/res/lobby/swap_'..type..'_over.png')
end

function Lobby_V2:OnLobbyPlayerInfo(index, sourceWidget, ...)
	local clientNum = tonumber(arg[1])
	local playerName = arg[2]
	local isMe = AtoB(arg[9])
	local isAlly = AtoB(arg[19])
	local isStaff = AtoB(arg[20])
	local chatNameColor = arg[82]
	local hasGlow = AtoB(arg[84])
	local isBot = AtoB(arg[85])
	local glowString = arg[86]
	local rank = tonumber(arg[87])
	local chatNameColorFont = arg[88]
	
	-- NAEU (show enemy rank)
	if not GetCvarBool('cl_GarenaEnable') then
		if (ViewingReplay() or (IsCampaignMatch() and rank > 0)) then
			-- Commented for season 7 and later seasons
			--GetWidget('lobby_'.._uiStr..'_player_'..index..'_rank'):SetTexture('/ui/fe2/season/icon_mini/'..GetRankIconNameRankLevel(tonumber(rank)))
			GetWidget('lobby_'.._uiStr..'_player_'..index..'_rank'):SetTexture('/ui/fe2/season/icon_mini/'..GetRankIconNameRankLevelAfterS6(tonumber(rank)))
			GetWidget('lobby_'.._uiStr..'_player_'..index..'_playername_parent'):SetWidth('-30i')
		else
			GetWidget('lobby_'.._uiStr..'_player_'..index..'_rank'):SetTexture('$invis')
			GetWidget('lobby_'.._uiStr..'_player_'..index..'_playername_parent'):SetWidth('100%')
		end
	-- SEA (hide enemy rank)
	else
		if ((isAlly or ViewingReplay()) and IsCampaignMatch() and (rank > 0)) then
			-- Commented for season 7 and later seasons
			--GetWidget('lobby_'.._uiStr..'_player_'..index..'_rank'):SetTexture('/ui/fe2/season/icon_mini/'..GetRankIconNameRankLevel(tonumber(rank)))
			GetWidget('lobby_'.._uiStr..'_player_'..index..'_rank'):SetTexture('/ui/fe2/season/icon_mini/'..GetRankIconNameRankLevelAfterS6(tonumber(rank)))
			GetWidget('lobby_'.._uiStr..'_player_'..index..'_playername_parent'):SetWidth('-30i')
		else
			GetWidget('lobby_'.._uiStr..'_player_'..index..'_rank'):SetTexture('$invis')
			GetWidget('lobby_'.._uiStr..'_player_'..index..'_playername_parent'):SetWidth('100%')
		end
	end

	-- Player Name Font& Color
	local font, color, outline, outlineColor = NotEmpty(chatNameColorFont) and chatNameColorFont..'_10' or 'dyn_10', '#c2b0a4', true, '#230000'
	if NotEmpty(chatNameColor) and chatNameColor ~= '#FFFFFF' then
		color = chatNameColor
		outline = false
	elseif isStaff then
		color = '#FF0000'
		outline = false
	elseif isPremium then
		color = '#DBBF4A'
		outline = false
	end

	local playerNameWidget = GetWidget('lobby_'.._uiStr..'_player_'..index..'_playername')
	local playerNameTip = GetWidget('lobby_'.._uiStr..'_player_'..index..'_playername_tip_value')

	playerNameWidget:SetFont(font)
	playerNameWidget:SetColor(color)
	playerNameWidget:SetGlow(hasGlow)
	playerNameWidget:SetBackgroundGlow(AtoB(arg[89]))
	playerNameWidget:SetGlowColor(glowString)
	playerNameWidget:SetOutline(outline)
	playerNameWidget:SetOutlineColor(outlineColor)

	playerNameTip:SetFont(font)
	playerNameTip:SetColor(color)
	playerNameTip:SetGlow(hasGlow)
	playerNameTip:SetBackgroundGlow(AtoB(arg[89]))
	playerNameTip:SetGlowColor(glowString)
	playerNameTip:SetOutline(outline)
	playerNameTip:SetOutlineColor(outlineColor)

	if font == '8bit_10' then 
		playerName = string.upper(playerName)
	end

	local needTip = Common_V2:SetLongText(playerNameWidget, playerName)
	GetWidget('lobby_'.._uiStr..'_player_'..index..'_playername_tip_root'):SetVisible(needTip)
	if needTip then
		playerNameTip:SetText(playerName)
	end

	-- Right Click Menu
	GetWidget('lobby_'.._uiStr..'_player_'..index..'_portrait'):SetCallback('onrightclick', function()
		local menutable = {}
		local menu = {}

		if not isBot then
			-- View Stats 
			menu = {}
			menu.content = 'ui_items_cc_right_click_view_stats'
			menu.onclicklua = 'Player_Stats_V2:Show(\''..StripClanTag(playerName)..'\')'
			table.insert(menutable, menu)

			-- Add Friend
			if not IsBuddy(StripClanTag(playerName)) and not isMe then
				menu = {}
				menu.content = 'ui_items_cc_right_click_add_buddy'
				menu.onclicklua = 'Common_V2:AddFriend(\''..StripClanTag(playerName)..'\')'
				table.insert(menutable, menu)
			end
		end

		local isMuted = AtoB(interface:UICmd('IsVoiceMuted('..clientNum..')'))

		if isAlly and not isMe then
			if isMuted then
				menu = {}
				menu.content = 'newui_lobby_turnon_voicechat_short'
				menu.onclicklua = 'Lobby_V2:TurnOnVoiceChat(\''..playerName..'\', '..clientNum..')'
				table.insert(menutable, menu)
			else
				menu = {}
				menu.content = 'newui_lobby_turnoff_voicechat_short'
				menu.onclicklua = 'Lobby_V2:TurnOffVoiceChat('..clientNum..')'
				table.insert(menutable, menu)
			end
		end

		Common_V2:PopupMenu(menutable)
	end)
end

function Lobby_V2:OnHeroSelectHeroList(index, ...)
	index = index + 1

	if _currentGameMode == 'sd' and index > 15 then
		index = index - 15
	end

	_heroInfos[index].heroName = arg[1]
	_heroInfos[index].iconPath = arg[2]
	_heroInfos[index].heroState = tonumber(arg[3])
	_heroInfos[index].isValid = AtoB(arg[4])
	_heroInfos[index].canSelect = AtoB(arg[5])
	_heroInfos[index].displayName = arg[6]
	_heroInfos[index].action = arg[30]
	_heroInfos[index].teamStr = arg[31]
	_heroInfos[index].isEarlyAccess = AtoB(arg[35])
	_heroInfos[index].isFreeHero = AtoB(arg[36])
	_heroInfos[index].hasPurchaseButton = AtoB(arg[37])
	_heroInfos[index].needToken = AtoB(arg[38])
	_heroInfos[index].earlyAccessCost = tonumber(arg[39])
	_heroInfos[index].earlyAccessPremium = AtoB(arg[41])
	_heroInfos[index].earlyAccessPremiumCost = tonumber(arg[42])
	_heroInfos[index].tokenSatisfied = AtoB(arg[44])
	_heroInfos[index].isSoloMode = AtoB(arg[56])
	_heroInfos[index].goldPrice = tonumber(arg[39])
	_heroInfos[index].silverPrice = tonumber(arg[42])
	_heroInfos[index].canUseSilverCoin = AtoB(arg[41])
	_heroInfos[index].isVoteBlindBan = AtoB(arg[57] or 'false')

	_heroInfos[index].soloRating		 = tonumber(arg[45])
	_heroInfos[index].jungleRating	 = tonumber(arg[46])
	_heroInfos[index].carryRating	 = tonumber(arg[47])
	_heroInfos[index].supportrating	 = tonumber(arg[48])
	_heroInfos[index].initiatorRating = tonumber(arg[49])
	_heroInfos[index].gankerRating	 = tonumber(arg[50])
	_heroInfos[index].pusherRating	 = tonumber(arg[53])
	_heroInfos[index].rangedRating	 = tonumber(arg[51])
	_heroInfos[index].meleeRating	 = tonumber(arg[52])

	Lobby_V2:UpdateOneHero(index)

	if _selectedHero == index then
		Lobby_V2:SelectHero(index)
	end
end

function Lobby_V2:OnUpdateMatchInfoPlayer(index, sourceWidget, ...)
	local clientNum = tonumber(arg[1])
	local playerName = arg[2]
	local isHost = AtoB(arg[4])
	local canBeKicked = AtoB(arg[5])
	local isMe = AtoB(arg[9])
	local accountID = tonumber(arg[10])
	local psr = tonumber(arg[11])
	local pointsChange = AtoB(arg[12])
	local winValue = arg[13]
	local lossValue = arg[14]
	local isLock = AtoB(arg[16])
	local iAmHost = AtoB(arg[18])
	local isAlly = AtoB(arg[19])
	local isStaff = AtoB(arg[20])
	local isPremium = AtoB(arg[21])
	local isAutoBalance = AtoB(arg[22])
	local chatNameColor = arg[82]
	local playerIcon = arg[83]
	local hasGlow = AtoB(arg[84])
	local isBot = AtoB(arg[85])
	local glowString = arg[86]
	local chatNameColorFont = arg[88]

	GetWidget('lobby_matchinfo_player_'..index):SetVisible(clientNum ~= -2)
	GetWidget('lobby_matchinfo_player_'..index..'_join'):SetVisible(clientNum == -1 and not isLock)
	GetWidget('lobby_matchinfo_player_'..index..'_lock'):SetVisible(clientNum == -1 and isLock)
	GetWidget('lobby_matchinfo_player_'..index..'_lockpos'):SetVisible(clientNum == -1 and not isAutoBalance and iAmHost and not isLock)
	GetWidget('lobby_matchinfo_player_'..index..'_unlockpos'):SetVisible(clientNum == -1 and not isAutoBalance and iAmHost and isLock)
	GetWidget('lobby_matchinfo_player_'..index..'_quit'):SetVisible(clientNum >= 0 and isMe)

	GetWidget('lobby_matchinfo_player_'..index..'_mute'):SetEnabled(clientNum >= 0 and not isMe and isAlly)
	GetWidget('lobby_matchinfo_player_'..index..'_unmute'):SetEnabled(clientNum >= 0 and not isMe and isAlly)

	local isBotMatch = string.lower(GetCurrentGameModeName()) == 'botmatch'
	GetWidget('lobby_matchinfo_player_'..index..'_addbot'):SetVisible(isBotMatch and iAmHost and Empty(playerName) and not isLock)

	if isBot then
		_selectedBot[index] = playerName
	else
		_selectedBot[index] = nil
	end

	local playerInfoWidget = GetWidget('lobby_matchinfo_player_'..index..'_info')
	if clientNum >= 0 then 
		if not playerInfoWidget:IsVisibleSelf() then 
			playerInfoWidget:FadeIn(250)
			playerInfoWidget:SetWidth('0%')
			playerInfoWidget:ScaleWidth('100%', 300)
			PlaySound('/shared/sounds/ui/revamp/teamslot_click.wav')
		end
	else
		playerInfoWidget:FadeOut(150)
	end

	if iAmHost and not isMe and clientNum >= 0 and canBeKicked then 
		GetWidget('lobby_matchinfo_player_'..index..'_kick'):SetVisible(true)
		GetWidget('lobby_matchinfo_player_'..index..'_kick'):SetCallback('onclick', function()
			Lobby_V2:KickPlayer(clientNum)
			PlaySound('/shared/sounds/ui/revamp/teamslot_leave.wav')
		end)
	else
		GetWidget('lobby_matchinfo_player_'..index..'_kick'):SetVisible(false)
	end

	if clientNum >= 0 then
		-- Player Icon
		if isBot then 
			GetWidget('lobby_matchinfo_player_'..index..'_info_playericon'):SetTexture('/ui/fe2/newui/res/lobby/boticon.png')
		elseif NotEmpty(playerIcon) then
			GetWidget('lobby_matchinfo_player_'..index..'_info_playericon'):SetTexture(playerIcon)
		else
			GetWidget('lobby_matchinfo_player_'..index..'_info_playericon'):SetAvatar('http://www.heroesofnewerth.com/getAvatar.php?id='..accountID)
		end

		-- Player Icon Border
		local borderColor = 'invisible'
		if isMe then
			if index < 5 then 
				GetWidget('lobby_matchinfo_player_'..index..'_info_border'):SetTexture('/ui/fe2/newui/res/lobby/border_legion_me.png')
			else
				GetWidget('lobby_matchinfo_player_'..index..'_info_border'):SetTexture('/ui/fe2/newui/res/lobby/border_hellbourne_me.png')
			end
		else
			if index < 5 then 
				GetWidget('lobby_matchinfo_player_'..index..'_info_border'):SetTexture('/ui/fe2/newui/res/lobby/border_legion.png')
			else
				GetWidget('lobby_matchinfo_player_'..index..'_info_border'):SetTexture('/ui/fe2/newui/res/lobby/border_hellbourne.png')
			end
		end

		-- Is Host
		GetWidget('lobby_matchinfo_player_'..index..'_info_host'):SetVisible(isHost)

		-- Player Name
		local font, color, outline, outlineColor = NotEmpty(chatNameColorFont) and chatNameColorFont..'_14' or 'dyn_14', '#c2b0a4', true, '#230000'
		if NotEmpty(chatNameColor) and chatNameColor ~= '#FFFFFF' then
			color = chatNameColor
			outline = false
		elseif isStaff then
			color = '#FF0000'
			outline = false
		elseif isPremium then
			color = '#DBBF4A'
			outline = false
		elseif isMe then
			if index < 5 then 
				color = '#ffdcc5'
				outlineColor = '#1e3645'
			else
				color = '#ffdcc5'
				outlineColor = '#623e27'
			end
		end

		local playerNameWidget = GetWidget('lobby_matchinfo_player_'..index..'_info_playername')
		playerNameWidget:SetFont(font)
		playerNameWidget:SetColor(color)
		playerNameWidget:SetGlow(hasGlow)
		playerNameWidget:SetBackgroundGlow(AtoB(arg[89]))
		playerNameWidget:SetGlowColor(glowString)
		playerNameWidget:SetOutline(outline)
		playerNameWidget:SetOutlineColor(outlineColor)

		if font == '8bit_14' then
			playerNameWidget:SetText(string.upper(playerName))
		else
			playerNameWidget:SetText(playerName)
		end

		-- Self Background
		GetWidget('lobby_matchinfo_player_'..index..'_info_isme'):SetVisible(isMe)


		-- Psr Value	
		color, outlineColor = '#7b695f', '#230000'		
		if isMe then
			if index < 5 then 
				color = '#a48a7b'
				outlineColor = '#1e3645'
			else
				color = '#a48a7b'
				outlineColor = '#623e27'
			end
		end

		local psrText = ''
		local psrWidget = GetWidget('lobby_matchinfo_player_'..index..'_info_playerpsr')
		if psr >= 0 and not ShouldHideStats() and not IsCampaignMatch() then
			psrText	= tostring(psr)
		end	
		psrWidget:SetColor(color)
		psrWidget:SetOutlineColor(outlineColor)
		psrWidget:SetText(psrText)

		-- Psr Change Value
		color, outlineColor = '#ac9383', '#230000'		
		if isMe then
			if index < 5 then 
				color = '#dbb6a0'
				outlineColor = '#1e3645'
			else
				color = '#dbb6a0'
				outlineColor = '#623e27'
			end
		end
		local psrChangeText = ''
		local psrChangeWidget = GetWidget('lobby_matchinfo_player_'..index..'_info_playerpsrchange')
		if not isAutoBalance and not ShouldHideStats() and not IsCampaignMatch() and pointsChange then
			psrChangeText = '+'..winValue..' / '..lossValue
		end
		psrChangeWidget:SetColor(color)
		psrChangeWidget:SetOutlineColor(outlineColor)
		psrChangeWidget:SetText(psrChangeText)

		-- Right Click Menu
		GetWidget('lobby_matchinfo_player_'..index..'_info_click'):SetCallback('onrightclick', function() 
			local menutable = {}
			local menu = {}

			if not isBot then 
				-- View Stats 
				menu = {}
				menu.content = 'ui_items_cc_right_click_view_stats'
				menu.onclicklua = 'Player_Stats_V2:Show(\''..StripClanTag(playerName)..'\')'
				table.insert(menutable, menu)

				-- Add Friend
				if not IsBuddy(StripClanTag(playerName)) and not isMe then
					menu = {}
					menu.content = 'ui_items_cc_right_click_add_buddy'
					menu.onclicklua = 'Common_V2:AddFriend(\''..StripClanTag(playerName)..'\')'
					table.insert(menutable, menu)
				end
			end

			if iAmHost then 
				if not isBot then
					menu = {}
					menu.content = 'game_lobby_assign_spectator_tip'
					menu.onclicklua = 'Lobby_V2:AssignAsSpectator('..clientNum..')'
					table.insert(menutable, menu)

					menu = {}
					menu.content = 'game_lobby_assign_referee_tip'
					menu.onclicklua = 'Lobby_V2:AssignAsReferee('..clientNum..')'
					table.insert(menutable, menu)

					menu = {}
					menu.content = 'game_lobby_assign_host_tip'
					menu.onclicklua = 'Lobby_V2:AssignAsHost('..clientNum..')'
					table.insert(menutable, menu)
				end
					
				if canBeKicked then
					menu = {}
					menu.content = 'game_lobby_kick_tip'
					menu.onclicklua = 'Lobby_V2:KickPlayer('..clientNum..')'
					table.insert(menutable, menu)
				end

				menu = {}
				menu.type = 2
				menu.content = 'newui_lobby_menu_swappos'
				menu.menu = 'lobby_matchinfo_swappos_template'
				table.insert(menutable, menu)

				_menuPosition = index
			end

			Common_V2:PopupMenu(menutable)
		end)
		
		-- Mute Button 
		GetWidget('lobby_matchinfo_player_'..index..'_mute'):SetCallback('onclick', function()
			Lobby_V2:TurnOffVoiceChat(clientNum)
			PlaySound('/shared/sounds/ui/revamp/region_click.wav')
		end)

		-- Unmute Button
		GetWidget('lobby_matchinfo_player_'..index..'_unmute'):SetCallback('onclick', function()
			Lobby_V2:TurnOnVoiceChat(playerName, clientNum)
			PlaySound('/shared/sounds/ui/revamp/region_click.wav')
		end)
	end
end

function Lobby_V2:OnUpdateLoadingInfo(index, sourceWidget, ...)
	local clientNum = arg[3]

	if clientNum == '-1' then 
		GetWidget('lobby_loading_player_'..index):SetVisible(false)
		return
	end

	local playerName = arg[4]
	local heroName = arg[6]
	local heroIcon = arg[7]
	local progress = tonumber(arg[19]) * 100

	GetWidget('lobby_loading_player_'..index):SetVisible(true)
	GetWidget('lobby_loading_player_'..index..'_playername'):SetText(playerName)
	GetWidget('lobby_loading_player_'..index..'_heroname'):SetText(heroName)
	GetWidget('lobby_loading_player_'..index..'_heroicon'):SetTexture(heroIcon)
	GetWidget('lobby_loading_player_'..index..'_progress'):SetWidth(tostring(progress)..'%')
end

function Lobby_V2:OnUpdateMatchInfo(sourceWidget, ...)
	local function SetInfo(id, value)
		local needTip = Common_V2:SetLongText(GetWidget('lobby_matchinfo_info_'..id..'_value'), value)

		GetWidget('lobby_matchinfo_info_'..id..'_tip_root'):SetVisible(needTip)
		if needTip then
			GetWidget('lobby_matchinfo_info_'..id..'_tip_value'):SetText(value)
		end
	end

	local gameMode = arg[1]
	local gameOption = arg[2]
	local serverName = arg[3]
	local mapName = arg[4]
	local playerNum = arg[5]
	local playerMaxNum = arg[6]
	local ping = arg[7]
	local gameName = arg[8]
	local matchid = arg[9]
	local hostName = arg[10]
	local version = arg[12]
	local gameModeName = arg[13]
	local botDifficulty = arg[15]
	local mapFileName = arg[16]

	if matchid == '-1' then 
		matchid = '<none>' 
	end

	local isBotMatch = string.lower(GetCurrentGameModeName()) == 'botmatch'
	if isBotMatch then 
		gameOption = gameOption..','..Translate('glb_options_bot_difficulty_'..botDifficulty)
	end

	SetInfo('matchid', matchid)
	SetInfo('hostname', hostName)
	SetInfo('servername', serverName)
	SetInfo('mapname', mapName)
	SetInfo('gamemode', gameMode)
	SetInfo('gameoption', gameOption)
	SetInfo('ping', ping..' ms')
	SetInfo('players', playerNum..'/'..playerMaxNum)
	SetInfo('version', version)

	GetWidget('lobby_matchinfo_title_gamename'):SetText(gameName)
	GetWidget('lobby_matchinfo_title_mapname'):SetText(mapName)

	if mapFileName == 'midwars_reborn' then
		mapFileName = 'midwars'
	end

	if mapFileName == 'caldavar' or mapFileName == 'midwars' or mapFileName == 'capturetheflag' or mapFileName == 'devowars' or 
		mapFileName == 'grimmscrossing' or mapFileName == 'soccer' or mapFileName == 'team_deathmatch' or mapFileName == 'caldavar_reborn' then 

		GetWidget('lobby_matchinfo_mapimage'):SetTexture('/ui/fe2/newui/res/lobby/map_'..mapFileName..'.png')
		GetWidget('lobby_loading_bg'):SetTexture('/ui/fe2/newui/res/loading/'..mapFileName..'.png')
	else
		GetWidget('lobby_matchinfo_mapimage'):SetTexture('/ui/fe2/newui/res/lobby/map_other.png')
		GetWidget('lobby_loading_bg'):SetTexture('/ui/fe2/newui/res/loading/other.png')
	end

	if IsCampaignMatch() then
		GetWidget('lobby_loading_bg_logo'):SetTexture('/ui/fe2/newui/res/loading/bglogo_con.png')
	else
		GetWidget('lobby_loading_bg_logo'):SetTexture('/ui/fe2/newui/res/loading/bglogo.png')
	end
end

function Lobby_V2:OnUpdateMatchInfoSpectator(sourceWidget, ...)
	local clientNum = tonumber(arg[1])
	local isReferee = AtoB(arg[6])

	if clientNum == -1 then
		_spectators = {}
		Lobby_V2:UpdateSpectatorsList()

	elseif not isReferee then
		local spectator = {}
		spectator.clientNum = clientNum
		spectator.name = arg[2]
		spectator.accoundId = arg[10]
		spectator.icon = arg[14]
		spectator.showBecomeRefereeBtn = AtoB(arg[7])
		spectator.iAmHost = AtoB(arg[7])
		spectator.canBeKicked = AtoB(arg[5])
		spectator.chatNameColor = arg[13]
		spectator.chatNameColorFont = arg[17]
		spectator.chatNameGlow = AtoB(arg[15])
		spectator.chatNameGlowColor = arg[16]
		spectator.chatNameBackgroundGlow = AtoB(arg[18])

		table.insert(_spectators, spectator)

		local scrollWidget = GetWidget('lobby_matchinfo_spectators_scroll')
		if #_spectators <= SPECTATOR_MAX then
			scrollWidget:SetMaxValue(1)
		else
			local scrollMaxValue = #_spectators - SPECTATOR_MAX + 1
			scrollWidget:SetMaxValue(scrollMaxValue)
		end

		scrollWidget:SetValue(1)
		Lobby_V2:UpdateSpectatorsList()
	end
end

function Lobby_V2:OnUpdateMatchInfoReferee(index, sourceWidget, ...)
	local clientNum = tonumber(arg[1])

	if clientNum == -1 then
		GetWidget('lobby_matchinfo_referee_'..index):SetVisible(false)
	else
		GetWidget('lobby_matchinfo_referee_'..index):SetVisible(true)

		local clientNum = arg[1]
		local name = arg[2]
		local canBeKicked = AtoB(arg[3])
		local accountId = arg[6]
		local icon = arg[10]
		local iAmHost = AtoB(arg[4])
		local font = NotEmpty(arg[13]) and arg[13]..'_11' or 'dyn_12'
		local color = NotEmpty(arg[9]) and arg[9] or '#a99081'
		local outline = not NotEmpty(arg[9])
		local glow = AtoB(arg[11])
		local glowColor = arg[12]

		if font == '8bit_11' then
			name = string.upper(name)
		end

		local widgetArrary = {'lobby_matchinfo_referee_'..index..'_name', 'lobby_matchinfo_referee_'..index..'_tip_value'}
		for _,v in ipairs(widgetArrary) do
			local widget = GetWidget(v)
			widget:SetColor(color)
			widget:SetFont(font)
			widget:SetOutline(outline)
			widget:SetGlow(glow)
			widget:SetBackgroundGlow(AtoB(arg[14]))
			widget:SetGlowColor(glowColor)
		end

		local needTip = Common_V2:SetLongText(GetWidget('lobby_matchinfo_referee_'..index..'_name'), name)
		GetWidget('lobby_matchinfo_referee_'..index..'_tip_root'):SetVisible(needTip)
		GetWidget('lobby_matchinfo_referee_'..index..'_tip_value'):SetText(name)

		if NotEmpty(icon) then
			GetWidget('lobby_matchinfo_referee_'..index..'_icon'):SetTexture(icon)
		else
			GetWidget('lobby_matchinfo_referee_'..index..'_icon'):SetAvatar('http://www.heroesofnewerth.com/getAvatar.php?id='..accountId)
		end

		GetWidget('lobby_matchinfo_referee_'..index..'_becomereferee'):SetVisible(false)
		GetWidget('lobby_matchinfo_referee_'..index..'_removereferee'):SetVisible(iAmHost)

		GetWidget('lobby_matchinfo_referee_'..index..'_removereferee'):SetCallback('onclick', function()
			interface:UICmd('DemoteRef('..clientNum..');')
			PlaySound('/shared/sounds/ui/revamp/region_click.wav')
		end)

		GetWidget('lobby_matchinfo_referee_'..index):SetCallback('onrightclick', function()
			local menutable = {}
			local menu = {}
			if iAmHost and canBeKicked then 
				menu = {}
				menu.content = 'game_lobby_kick_tip'
				menu.onclicklua = 'Lobby_V2:KickPlayer('..clientNum..')'
				table.insert(menutable, menu)
			end
			Common_V2:PopupMenu(menutable)
		end)
	end
end

function Lobby_V2:OnSoloHeroCandidate(index, ...)
	_soloCandidates[index] = _soloCandidates[index] or {}
	_soloCandidates[index].heroName = arg[1]
	_soloCandidates[index].heroIcon = arg[2]
	_soloCandidates[index].bEnabled = AtoB(arg[3])
	_soloCandidates[index].banned = AtoB(arg[4])
	_soloCandidates[index].action = arg[5]
	_soloCandidates[index].blindPick = AtoB(arg[6])

	Lobby_V2:UpdateSoloCandidate(index)

	if index == _selectedCandidate then
		Lobby_V2:OnClickSoloCandidate(index)
	end
end

function Lobby_V2:OnLockHeroSelectHeroList(index, ...)
	_lockedHeroes[index] = _lockedHeroes[index] or {}
	_lockedHeroes[index].heroName = arg[1]
	_lockedHeroes[index].iconPath = arg[2]
	_lockedHeroes[index].heroState = tonumber(arg[3])
	_lockedHeroes[index].isValid = AtoB(arg[4])
	_lockedHeroes[index].canSelect = AtoB(arg[5])
	_lockedHeroes[index].displayName = arg[6]
	_lockedHeroes[index].action = arg[30]
	_lockedHeroes[index].teamStr = arg[31]
	_lockedHeroes[index].isEarlyAccess = AtoB(arg[35])
	_lockedHeroes[index].isFreeHero = AtoB(arg[36])
	_lockedHeroes[index].hasPurchaseButton = AtoB(arg[37])
	_lockedHeroes[index].needToken = AtoB(arg[38])
	_lockedHeroes[index].earlyAccessCost = tonumber(arg[39])
	_lockedHeroes[index].earlyAccessPremium = AtoB(arg[41])
	_lockedHeroes[index].earlyAccessPremiumCost = tonumber(arg[42])
	_lockedHeroes[index].tokenSatisfied = AtoB(arg[44])
	_lockedHeroes[index].isSoloMode = AtoB(arg[56] or 'false')
	_lockedHeroes[index].goldPrice = tonumber(arg[39])
	_lockedHeroes[index].silverPrice = tonumber(arg[42])
	_lockedHeroes[index].canUseSilverCoin = AtoB(arg[41])
	_lockedHeroes[index].isVoteBlindBan = AtoB(arg[57] or 'false')

	Lobby_V2:UpdateLockedHero(index)

	if _selectedLockedHero == index then
		Lobby_V2:OnClickLockedHero(index)
	end
end

function Lobby_V2:OnLobbyPurchaseStatus(responseCode)
	if responseCode == 3 then
		GetWidget('lobby_popup_purchase_error'):SetVisible(true)
		GetWidget('lobby_popup_purchase_error_desc'):SetText(Translate('newui_lobby_popup_error_desc'))
	end
end

function Lobby_V2:OnLobbyPurchaseResults(responseCode, popupCode, errorCode)
	errorCode = tonumber(errorCode)
	popupCode = tonumber(popupCode)

	if errorCode > 0 then 
		GetWidget('lobby_popup_purchase_error'):SetVisible(true)

		local errorText = Translate('newui_lobby_popup_error_desc')..' (Error Code:'.. errorCode..')'
		if errorCode == 23 then
			errorText = Translate('mstore_buyavatar_needhero_desc')
		end

		GetWidget('lobby_popup_purchase_error_desc'):SetText(errorText)
		return
	end

	if popupCode == 3 then
		interface:UICmd('ChatRefreshUpgrades();')
		interface:UICmd('ClientRefreshUpgrades();')
		interface:UICmd('ServerRefreshUpgrades();')

		GetWidget('lobby_popup_purchase_success'):SetVisible(true)
	end
end

function Lobby_V2:OnUpgradesRefresh()
	if GetWidget('lobby_picker_avatars'):IsVisible() then
		Lobby_V2:InitAvatars()
	end
end

function Lobby_V2:OnSelectHeroInfo(...)
	local heroInfo = {}

	heroInfo.name = arg[3]
	heroInfo.icon = arg[2]
	heroInfo.primaryAttri = arg[5]
	heroInfo.attackType = arg[24]
	heroInfo.strength = math.floor(tonumber(arg[6]))
	heroInfo.strengthPerLevel = tonumber(string.format('%.1f', tonumber(arg[7]) or 0))
	heroInfo.agility = math.floor(tonumber(arg[8]))
	heroInfo.agilityPerLevel = tonumber(string.format('%.1f', tonumber(arg[9]) or 0))
	heroInfo.intelligence = math.floor(tonumber(arg[10]))
	heroInfo.intelligencePerLevel = tonumber(string.format('%.1f', tonumber(arg[11]) or 0))
	heroInfo.attackRange = string.format('%d', tonumber(arg[14]) or 0)
	heroInfo.attackSpeed = string.format('%.2f', 1000 / tonumber(arg[15]))
	heroInfo.damageMin = string.format('%d', tonumber(arg[16]) or 0)
	heroInfo.damageMax = string.format('%d', tonumber(arg[17]) or 0)
	heroInfo.armor = string.format('%.2f', tonumber(arg[18]) or 0)
	heroInfo.magicArmor = string.format('%.2f', tonumber(arg[19]) or 0)
	heroInfo.moveSpeed = string.format('%d', tonumber(arg[12]) or 0)
	heroInfo.role = arg[25]
	heroInfo.solorating		 = tonumber(arg[26])
	heroInfo.junglerating	 = tonumber(arg[27])
	heroInfo.carryrating	 = tonumber(arg[28])
	heroInfo.supportrating	 = tonumber(arg[29])
	heroInfo.initiatorrating = tonumber(arg[30])
	heroInfo.gankerrating	 = tonumber(arg[31])
	heroInfo.pusherrating	 = tonumber(arg[34])
	heroInfo.rangedrating	 = tonumber(arg[32])
	heroInfo.meleerating	 = tonumber(arg[33])
	heroInfo.masteryExp = tonumber(GetMyMasteryExp(arg[35]))
	heroInfo.difficulty = 0

	Lobby_V2:SetHeroInfo(heroInfo)
end

function Lobby_V2:OnSelectHeroAbilityInfo(i, ...)
	local ability = {}
	ability.icon = arg[2]
	ability.name = arg[3]
	ability.manaCost = arg[6]
	ability.desc = arg[14]
	ability.cdTime = arg[7]
	ability.isPassive = not AtoB(arg[11])
	ability.range = arg[8]
	ability.targetRadius = arg[12]
	ability.auraRange = arg[13]

	Lobby_V2:SetHeroAbilityInfo(i, ability)
end

function Lobby_V2:OnMicroStoreResults(...)
	if Empty(arg[1]) then return end

	local popupCode = tonumber(arg[2])
	local errorCode = tonumber(arg[16])
	local category = arg[14]
	local productIds = explode('|', arg[3])
	local productPrices = explode('|', arg[5])
	local productCodes = explode('|', arg[21])

	if category ~= '58' and not GetWidget('lobby'):IsVisible() then return end

	if errorCode > 0 then
		GetWidget('lobby_popup_purchase_error'):SetVisible(true)

		local errorText = Translate('newui_lobby_popup_error_desc')..' (Error Code:'.. errorCode..')'
		GetWidget('lobby_popup_purchase_error_desc'):SetText(errorText)
		return 
	end

	if popupCode == 3 then
		interface:UICmd('ChatRefreshUpgrades();')
		interface:UICmd('ClientRefreshUpgrades();')
		interface:UICmd('ServerRefreshUpgrades();')

		GetWidget('lobby_popup_purchase_success'):SetVisible(true)
		return
	end

	if #productIds < 3 or #productPrices < 3 then return end
	
	_EAPInfo[1] = {price=productPrices[3], productId=productIds[3], productCode=productCodes[1], name=Translate('mstore_product'..productIds[1]..'_name')}
	_EAPInfo[2] = {price=productPrices[2], productId=productIds[2], productCode=productCodes[3], name=Translate('mstore_product'..productIds[3]..'_name')}
	_EAPInfo[3] = {price=productPrices[1], productId=productIds[1], productCode=productCodes[2], name=Translate('mstore_product'..productIds[2]..'_name')}
end
-------------------------- UI Handler ------------------------------------------
function Lobby_V2:OnClickPrivateCheckbox()
	local state = GetWidget('lobby_matchinfo_private'):GetButtonState()
	if state == 0 then
		interface:UICmd('SetServerPrivateValue(0);')
	else
		interface:UICmd('SetServerPrivateValue(1);')
	end
end

function Lobby_V2:OnMouseOverHero(team, group, row, id, gameMode)
	local index = -1
	if gameMode == 'sp' then
		index = row * SP_COLUMN_NUM + id
	elseif gameMode == 'sd' then
		index = row * SD_COLUMN_NUM + id
	elseif gameMode == 'bd' then
		local numPerGroup = BD_ROW_NUM * BD_COLUMN_NUM
		index = numPerGroup * group + BD_COLUMN_NUM * row + id
	elseif gameMode == 'rd' then
		index = row * RD_COLUMN_NUM + id
	elseif gameMode == 'lp' then
		local hero = _lockedHeroes[id]
		if hero ~= nil then
			Lobby_V2:ShowHeroTip(hero.heroName)
			GetWidget('communicator_main_large_scale'):SetVisible(false)
		end
			
		return
	else
		local numPerGroup = GROUP_HERO_NUM * TEAM_NUM
		index = group * numPerGroup + team * GROUP_HERO_NUM + row * PERROW_NUM + id
	end	

	if index == -1 then return end

	Lobby_V2:ShowHeroTip(_heroInfos[index+1].heroName)
	GetWidget('communicator_main_large_scale'):SetVisible(false)
end

function Lobby_V2:OnMouseOutHero()
	Lobby_V2:ShowHeroTip()
	GetWidget('communicator_main_large_scale'):FadeIn(250)
end

function Lobby_V2:OnClickHero(team, group, row, id, gameMode)
	if ViewingStreaming() then return end

	local index = -1
	if gameMode == 'sp' then
		index = row * SP_COLUMN_NUM + id
	elseif gameMode == 'sd' then
		index = row * SD_COLUMN_NUM + id
	elseif gameMode == 'bd' then
		local numPerGroup = BD_ROW_NUM * BD_COLUMN_NUM
		index = numPerGroup * group + BD_COLUMN_NUM * row + id
	elseif gameMode == 'rd' then
		index = row * RD_COLUMN_NUM + id
	elseif gameMode == 'lp' then
		Lobby_V2:OnClickLockedHero(id)
		return
	else
		local numPerGroup = GROUP_HERO_NUM * TEAM_NUM
		index = group * numPerGroup + team * GROUP_HERO_NUM + row * PERROW_NUM + id
	end	

	if index == -1 then return end

	Lobby_V2:SelectHero(index + 1)
end

function Lobby_V2:OnRightClickHero(team, group, row, id, gameMode)
	if ViewingStreaming() then return end

	local index = -1
	if gameMode == 'sp' then
		index = row * SP_COLUMN_NUM + id
	elseif gameMode == 'sd' then
		index = row * SD_COLUMN_NUM + id
	elseif gameMode == 'bd' then
		local numPerGroup = BD_ROW_NUM * BD_COLUMN_NUM
		index = numPerGroup * group + BD_COLUMN_NUM * row + id
	elseif gameMode == 'rd' then
		index = row * RD_COLUMN_NUM + id
	elseif gameMode == 'lp' then
		local hero = _lockedHeroes[id]
		if hero ~= nil then
			Lobby_V2:OpenAvatars(hero.heroName)
		end
			
		return
	else
		local numPerGroup = GROUP_HERO_NUM * TEAM_NUM
		index = group * numPerGroup + team * GROUP_HERO_NUM + row * PERROW_NUM + id
	end	

	if index == -1 then return end

	Lobby_V2:OpenAvatars(_heroInfos[index+1].heroName)
	--Lobby_V2:PurchaseHero(_heroInfos[index+1])
	--Lobby_V2:PurchaseEAP(_heroInfos[index+1])
end

function Lobby_V2:OnClickSoloCandidate(index)
	if index ~= _selectedCandidate then
		local lastSelection = _selectedCandidate
		_selectedCandidate = index
		Lobby_V2:UpdateSoloCandidate(lastSelection)
		Lobby_V2:UpdateSoloCandidate(_selectedCandidate)	
	end

	local hero = _soloCandidates[index]
	local selectBtn = GetWidget('lobby_selecthero')

	local heroName = hero.heroName
	local heroIcon = hero.heroIcon
	local bEnabled = hero.bEnabled
	local banned = hero.banned
	local action = hero.action
	local blindPick = hero.blindPick

	if bEnabled then
		selectBtn:SetEnabled(true)
		selectBtn:SetCallback('onclick', function()
			interface:UICmd(action .. '(\'' .. heroName .. '\');') 
		end)
	else
		selectBtn:SetEnabled(false)
	end
end

function Lobby_V2:OnClickLockedHero(index)
	if index ~= _selectedLockedHero then
		local lastSelection = _selectedLockedHero
		_selectedLockedHero = index
		Lobby_V2:UpdateOneHero(lastSelection)
		Lobby_V2:UpdateOneHero(_selectedLockedHero)	
	end

	local hero = _heroInfos[_selectedLockedHero]
	local selectBtn = GetWidget('lobby_selecthero')
	local buyBtn = GetWidget('lobby_buyhero')

	if hero.canSelect and hero.heroState > 0 then
		selectBtn:SetEnabled(true)

		if hero.action == 'SpawnHero' then 
			interface:UICmd('PotentialHero(\'' .. hero.heroName .. '\');')
		end

		selectBtn:SetCallback('onclick', function()
 			interface:UICmd(hero.action .. '(\'' .. hero.heroName .. '\');')
 		end)
	else
		selectBtn:SetEnabled(false)
	end

	if hero.hasPurchaseButton then 
		buyBtn:SetVisible(true)
		buyBtn:SetCallback('onclick', function()
			if hero.isEarlyAccess then 
				Lobby_V2:PurchaseEAP(hero)
			else
				Lobby_V2:PurchaseHero(hero)
			end
 		end)
 	else
 		buyBtn:SetVisible(false)
 	end
 	
 	if hero.hasPurchaseButton then
	 	local gold = hero.goldPrice or 0
	 	local silver = hero.silverPrice or 0

	 	GetWidget('lobby_picker_herobuttons_coins'):SetVisible(true)
	 	GetWidget('lobby_picker_herobuttons_coins_gold'):SetVisible(gold > 0)
	 	GetWidget('lobby_picker_herobuttons_coins_silver'):SetVisible(silver > 0)

	 	local widget = GetWidget('lobby_picker_herobuttons_coins_gold_text')
	 	local str = tostring(gold) 
	 	widget:SetWidth(widget:GetStringWidth(str))
	 	widget:SetText(str)

	 	widget = GetWidget('lobby_picker_herobuttons_coins_silver_text')
	 	str = tostring(silver) 
	 	widget:SetWidth(widget:GetStringWidth(str))
	 	widget:SetText(str)
	else
		GetWidget('lobby_picker_herobuttons_coins'):SetVisible(false)
	end
end

function Lobby_V2:OnClickAddBot(index)
	_botPosition = index

	GetWidget('lobby_matchinfo_addbot'):SetVisible(true)
	GetWidget('lobby_matchinfo_addbot_difficulty'):SetSelectedItemByValue(GetCvarInt('g_botDifficulty'))
	Lobby_V2:InitBotsPanel()
end

function Lobby_V2:OnClickBot(row, id)
	local scrollWidget = GetWidget('lobby_matchinfo_addbot_scroll')
	local startIndex = tonumber(scrollWidget:GetValue())
	local maxValue = tonumber(scrollWidget:UICmd("GetScrollbarMaxValue()"))
	if startIndex < 1 then startIndex = 1 end
	if startIndex > maxValue then startIndex = maxValue end

	local index = (startIndex -1 + row)*BOT_COLUMN_NUM + id
	local bot = _validBotList[index+1] 

	if bot ~= nil then
		local team = _botPosition < 5 and 1 or 2
		if bot.sHeroName ~= 'random' then
			AddBot(team, bot.def)
		else
			AddBot(team, _validBotList[math.random(2, #_validBotList)].def)
		end

		GetWidget('lobby_matchinfo_addbot'):SetVisible(false)
	end
end

function Lobby_V2:OnClickSetFirstBan(team)
	if team == 1 then
		if GetWidget('lobby_matchinfo_firstban_legion'):GetButtonState() == 0 then
			GetWidget('lobby_matchinfo_firstban_legion'):SetButtonState(1)
		else
			interface:UICmd('AssignFirstBanTeam(1);')
		end
	elseif team == 2 then
		if GetWidget('lobby_matchinfo_firstban_hellbourne'):GetButtonState() == 0 then
			GetWidget('lobby_matchinfo_firstban_hellbourne'):SetButtonState(1)
		else
			interface:UICmd('AssignFirstBanTeam(2);')
		end
	end
end

function Lobby_V2:OnClickSwapPosition(target)
	if _menuPosition == target then return end

	local team1 = (_menuPosition > 4) and 2 or 1
	local slot1 = _menuPosition % 5
	local team2 = (target > 4) and 2 or 1
	local slot2 = target % 5

	interface:UICmd('RequestSwapPlayerSlots('..team1..','..slot1..','..team2..','..slot2..');')
end
-------------------------------- Avatar -----------------------------------------------
function Lobby_V2:GetAvatarType(avatarInfo)
	local avatarType = 'unavailable'
	if not avatarInfo.Available and not CanAccessAltAvatar(_avatarHero..'.'..avatarInfo.Name) then
		local gold = avatarInfo.Cost or 0
		local silver = avatarInfo.PremiumCost or 0

		if gold >= 9001 and gold ~= 9006 then	-- xx only cases
		elseif silver > 0 and silver < 9001 and _silverCoins >= silver then
			avatarType = 'purchase'
		elseif gold > 0 and gold < 9001 and _goldCoins >= gold then
			avatarType = 'purchase'
		elseif (gold > 0 and gold < 9001) or (silver > 0 and silver < 9001) then
			avatarType = 'nocoin'
		end
	elseif avatarInfo.Available and ((CanAccessHeroProduct(_avatarHero)) or (IsEarlyAccessHero(_avatarHero) and CanAccessEarlyAccessProduct(_avatarHero))) then
		avatarType = 'available'
	end

	return avatarType
end

function Lobby_V2:InitAvatars()
	_selectedAvatar = 1
	_avatars = {}

	local avatars = GetAltAvatars(_avatarHero)

	table.insert(_avatars, avatars[1])
	for i= #avatars, 2, -1 do
		if Lobby_V2:GetAvatarType(avatars[i]) ~= 'unavailable' then
			table.insert(_avatars, avatars[i])
		end
	end

	GetWidget('lobby_picker_avatars_parent'):ClearChildren()

	for i,v in ipairs(_avatars) do
		GetWidget('lobby_picker_avatars_parent'):Instantiate('lobby_avatar_card_template',
			'id', tostring(i),
			'x', tostring(AVATAR_OFFSET * (i-1)..'i')
		)

		local avatarName = _avatarHero..'.'..v.Name
		local widget = GetWidget('lobby_avatar_card_'..i..'_model')
		widget:SetModel(GetHeroPreviewModelPathFromProduct(avatarName))
		widget:SetModelPos(GetHeroStorePosFromProduct(avatarName))
		widget:SetModelAngles(GetHeroStoreAnglesFromProduct(avatarName))
		widget:SetModelScale(GetHeroStoreScaleFromProduct(avatarName))
		widget:SetMultiEffect(GetHeroStorePassiveEffectPathFromProduct(avatarName), 0)

		local productID = tostring(v.ProductID)
		local isNew = false
		local isUltimate = false
		if _avatarsNewUltimateInfo[productID] ~= nil then
			isNew = _avatarsNewUltimateInfo[productID].isNew
			isUltimate = _avatarsNewUltimateInfo[productID].isUltimate
		end

		if isNew and isUltimate then
			widget:SetEffect('/ui/fe2/newui/res/effects/glow_gold_high.effect')
		elseif isNew then
			widget:SetEffect('/ui/fe2/newui/res/effects/glow_blue.effect')
		else
			widget:SetEffect('')
		end

		if _avatarCode ~= nil then
			if _avatarCode == v.Name then
				_selectedAvatar = i
			end
		elseif v.Name == _avatarDefaultCode then 
			_selectedAvatar = i
		end
	end

	Lobby_V2:UpdateAvatars()
end

function Lobby_V2:OpenAvatars(heroName)
	GetWidget('lobby_picker_avatars'):SetVisible(true)
	GetWidget('lobby_'.._uiStr..'_heroes'):SetVisible(false)
	GetWidget('lobby_picker_herobuttons'):SetVisible(false)
	GetWidget('lobby_picker_filter'):SetVisible(false)

	_avatarHero = heroName
	_avatarCode = nil
	_avatarDefaultCode = GetDBEntry('def_av_'.._avatarHero, nil , false) or 'Base'
	_avatarChanging = false
	
	Lobby_V2:InitAvatars()
end

function Lobby_V2:CloseAvatars()
	GetWidget('lobby_picker_avatars'):SetVisible(false)
	GetWidget('lobby_'.._uiStr..'_heroes'):SetVisible(true)
	GetWidget('lobby_picker_herobuttons'):SetVisible(true)
	GetWidget('lobby_picker_filter'):SetVisible(true)

	GetDBEntry('def_av_'.._avatarHero, _avatarDefaultCode , true)
end

function Lobby_V2:UpdateAvatars()
	GetWidget('lobby_avatar_card_'.._selectedAvatar):BringToFront()
	for i,v in ipairs(_avatars) do
		GetWidget('lobby_avatar_card_'..i):SetVisible(i >= (_selectedAvatar-2) and i <= (_selectedAvatar+2))

		local modelWidget = GetWidget('lobby_avatar_card_'..i..'_model')
		local scaleWidget = GetWidget('lobby_avatar_card_'..i..'_scale')
		if i == _selectedAvatar then 
			scaleWidget:SetWidth(AVATAR_LARGESIZE)
			scaleWidget:SetHeight(AVATAR_LARGESIZE)

			modelWidget:SetSunColor(1, 1, 1)
			modelWidget:SetAmbientColor(0.65, 0.65, 0.65)
		elseif i == _selectedAvatar + 1 or i == _selectedAvatar - 1 then
			GetWidget('lobby_avatar_card_'..i..'_scale'):SetWidth(AVATAR_MIDDLESIZE)
			GetWidget('lobby_avatar_card_'..i..'_scale'):SetHeight(AVATAR_MIDDLESIZE)

			modelWidget:SetSunColor(0.5, 0.5, 0.5)
			modelWidget:SetAmbientColor(0.35, 0.35, 0.35)
		else
			GetWidget('lobby_avatar_card_'..i..'_scale'):SetWidth(AVATAR_SMALLSIZE)
			GetWidget('lobby_avatar_card_'..i..'_scale'):SetHeight(AVATAR_SMALLSIZE)

			modelWidget:SetSunColor(0.5, 0.5, 0.5)
			modelWidget:SetAmbientColor(0.35, 0.35, 0.35)
		end
	end

	GetWidget('lobby_picker_avatars_parent'):SetX('-'..tostring(AVATAR_OFFSET * (_selectedAvatar-1)).. 'i')

	local avatarInfo = _avatars[_selectedAvatar]
	if avatarInfo == nil then return end

	_avatarCode = avatarInfo.Name

	GetWidget('lobby_picker_avatars_icon'):SetTexture(avatarInfo.Icon)
	GetWidget('lobby_picker_avatars_name'):SetText(avatarInfo.AvatarDisplayName or avatarInfo.DisplayName or '')

	if _avatarDefaultCode == avatarInfo.Name then
		GetWidget('lobby_picker_avatars_default'):SetButtonState(1)
	else
		GetWidget('lobby_picker_avatars_default'):SetButtonState(0)
	end
	GetWidget('lobby_picker_avatars_default'):SetVisible(avatarInfo.Available)

	-- Settting Buttons
	local avatarType = Lobby_V2:GetAvatarType(avatarInfo)
	GetWidget('lobby_select_avatar'):SetVisible(avatarType == 'available')
	GetWidget('lobby_select_avatar'):SetEnabled(Empty(_myGameHero) or (_myGameHero == _avatarHero))
	GetWidget('lobby_purchase_avatar'):SetVisible(avatarType == 'purchase')
	GetWidget('lobby_nocointobuy_avatar'):SetVisible(avatarType == 'nocoin')
	GetWidget('lobby_unavailable_avatar'):SetVisible(avatarType == 'unavailable')
	GetWidget('lobby_picker_avatars_buttons_coins'):SetVisible(avatarType == 'purchase' or avatarType == 'nocoin')

	if avatarInfo.Cost and avatarInfo.Cost > 0 and avatarInfo.Cost < 9001 then
		local value = tostring(avatarInfo.Cost)
		local textWidth = GetWidget('lobby_picker_avatars_buttons_coins_gold_text'):GetStringWidth(value)

		GetWidget('lobby_picker_avatars_buttons_coins_gold_text'):SetText(value)
		GetWidget('lobby_picker_avatars_buttons_coins_gold_text'):SetWidth(textWidth)
		GetWidget('lobby_picker_avatars_buttons_coins_gold_text'):SetVisible(true)
		GetWidget('lobby_picker_avatars_buttons_coins_gold_icon'):SetVisible(true)
	else
		GetWidget('lobby_picker_avatars_buttons_coins_gold_text'):SetVisible(false)
		GetWidget('lobby_picker_avatars_buttons_coins_gold_icon'):SetVisible(false)
	end

	if avatarInfo.PremiumCost and avatarInfo.PremiumCost > 0 and avatarInfo.PremiumCost < 9001 then
		local value = tostring(avatarInfo.PremiumCost)
		local textWidth = GetWidget('lobby_picker_avatars_buttons_coins_silver_text'):GetStringWidth(value)

		GetWidget('lobby_picker_avatars_buttons_coins_silver_text'):SetText(value)
		GetWidget('lobby_picker_avatars_buttons_coins_silver_text'):SetWidth(textWidth)
		GetWidget('lobby_picker_avatars_buttons_coins_silver_text'):SetVisible(true)
		GetWidget('lobby_picker_avatars_buttons_coins_silver_icon'):SetVisible(true)
	else
		GetWidget('lobby_picker_avatars_buttons_coins_silver_text'):SetVisible(false)
		GetWidget('lobby_picker_avatars_buttons_coins_silver_icon'):SetVisible(false)
	end

	-- Setting Trial & Coupon & GCA
	local trialInfo = GetTrialInfo(_avatarHero, _avatarCode)
	local couponTable = GetCardsInfo(_avatarHero, _avatarCode)
	local bIsGCABenifit = IsGCABenifitAltAvatar(_avatarHero, _avatarCode)
	if (NotEmpty(trialInfo)) then
		local cardLabel = Translate('compendium_trial_info')..' '..Translate('compendium_trial_infohead')..trialInfo..Translate('compendium_trial_infotail')
		GetWidget('lobby_picker_avatars_trial_text'):SetText(cardLabel)
		GetWidget('lobby_picker_avatars_trial'):SetVisible(true)
		GetWidget('lobby_picker_avatars_coupon'):SetVisible(false)
		GetWidget('lobby_picker_avatars_gca'):SetVisible(false)
	elseif bIsGCABenifit then
		GetWidget('lobby_picker_avatars_trial'):SetVisible(false)
		GetWidget('lobby_picker_avatars_coupon'):SetVisible(false)
		GetWidget('lobby_picker_avatars_gca'):SetVisible(true)
	elseif couponTable and (#couponTable > 0) and avatarType == 'purchase' then
		GetWidget('lobby_picker_avatars_trial'):SetVisible(false)
		GetWidget('lobby_picker_avatars_coupon'):SetVisible(true)
		GetWidget('lobby_picker_avatars_gca'):SetVisible(false)
	else
		GetWidget('lobby_picker_avatars_trial'):SetVisible(false)
		GetWidget('lobby_picker_avatars_coupon'):SetVisible(false)
		GetWidget('lobby_picker_avatars_gca'):SetVisible(false)
	end
end

function Lobby_V2:SelectAvatar()
	local avatarInfo = _avatars[_selectedAvatar]
	if avatarInfo == nil then return end

	if Empty(_myGameHero) then
		if CanCaptainSpawnHero() then
			SpawnHero(_avatarHero)
			SelectAvatar(avatarInfo.Name)
			_pickHeroByAvatar = true
		end
	elseif _avatarHero == _myGameHero then
		SelectAvatar(avatarInfo.Name)
	end

	Lobby_V2:CloseAvatars()
end

function Lobby_V2:UpdateAvatarCoupon(index)
	local couponTable = GetCardsInfo(_avatarHero, _avatarCode)

	if couponTable[index] == nil then 
		GetWidget('lobby_popup_purchase_avatar_coupon_'..index):SetVisible(false)
	else
		GetWidget('lobby_popup_purchase_avatar_coupon_'..index):SetVisible(true)
		GetWidget('lobby_popup_purchase_avatar_coupon_'..index..'_name'):SetText(Translate('mstore_product'..couponTable[index].Product_id..'_name'))
		GetWidget('lobby_popup_purchase_avatar_coupon_'..index..'_discount'):SetText(tostring(couponTable[index].Discount)..'% off')

		local state = GetWidget('lobby_popup_purchase_avatar_coupon_'..index..'_btn'):GetButtonState()
		if state == 0 then
			GetWidget('lobby_popup_purchase_avatar_coupon_'..index..'_name'):SetColor('#ad9281')
			GetWidget('lobby_popup_purchase_avatar_coupon_'..index..'_discount'):SetColor('#ad9281')
		else
			GetWidget('lobby_popup_purchase_avatar_coupon_'..index..'_name'):SetColor('#efd4c1')
			GetWidget('lobby_popup_purchase_avatar_coupon_'..index..'_discount'):SetColor('#efd4c1')

			GetWidget('lobby_popup_purchase_avatar_coupon_info'):SetColor('#ad9281')
			GetWidget('lobby_popup_purchase_avatar_coupon_info'):SetText(Translate("mstore_cards_desc")..' '..couponTable[index].EndTime)
			_avatarDiscount = couponTable[index].Coupon_id
		end
	end
end

function Lobby_V2:PurchaseAvatar()
	local avatarInfo = _avatars[_selectedAvatar]
	if avatarInfo == nil then return end

	GetWidget('lobby_popup_purchase_avatar'):FadeIn(150)

	GetWidget('lobby_popup_purchase_avatar_icon'):SetTexture(avatarInfo.Icon)
	GetWidget('lobby_popup_purchase_avatar_name'):SetText(avatarInfo.AvatarDisplayName or avatarInfo.DisplayName or '')

	local canUseGold = false
	local canUseSilver = false
	if avatarInfo.Cost > 0 and avatarInfo.Cost < 9001 then
		canUseGold = true
		local value = tostring(avatarInfo.Cost)
		local textWidth = GetWidget('lobby_popup_purchase_avatar_gold_text'):GetStringWidth(value)
		GetWidget('lobby_popup_purchase_avatar_gold_text'):SetText(value)
		GetWidget('lobby_popup_purchase_avatar_gold_text'):SetWidth(textWidth)
		GetWidget('lobby_popup_purchase_avatar_gold'):SetVisible(true)

		GetWidget('lobby_popup_purchase_avatar_gold_buy'):SetCallback('onclick', function()
			SubmitForm('LobbyBuyAvatar', 
						'account_id', tostring(Client.GetAccountID()), 
						'request_code', '8', 
						'cookie', Client.GetCookie(), 
						'hero_name', _avatarHero, 
						'avatar_code', avatarInfo.Name, 
						'currency', '0', 
						'type', 'Alt Avatar', 
						'discount', _avatarDiscount or '')

			GetWidget('lobby_popup_purchase_avatar'):SetVisible(false)

			PlaySound('/shared/sounds/ui/revamp/region_click.wav')
		end)
		GetWidget('lobby_popup_purchase_avatar_gold_buy'):SetVisible(_goldCoins >= avatarInfo.Cost)
	else
		GetWidget('lobby_popup_purchase_avatar_gold'):SetVisible(false)
	end

	if avatarInfo.PremiumCost > 0 and avatarInfo.PremiumCost < 9001 then
		canUseSilver = true
		local value = tostring(avatarInfo.PremiumCost)
		local textWidth = GetWidget('lobby_popup_purchase_avatar_silver_text'):GetStringWidth(value)
		GetWidget('lobby_popup_purchase_avatar_silver_text'):SetText(value)
		GetWidget('lobby_popup_purchase_avatar_silver_text'):SetWidth(textWidth)
		GetWidget('lobby_popup_purchase_avatar_silver'):SetVisible(true)

		GetWidget('lobby_popup_purchase_avatar_silver_buy'):SetCallback('onclick', function()
			SubmitForm('LobbyBuyAvatar', 
						'account_id', tostring(Client.GetAccountID()), 
						'request_code', '8', 
						'cookie', Client.GetCookie(), 
						'hero_name', _avatarHero, 
						'avatar_code', avatarInfo.Name, 
						'currency', '1', 
						'type', 'Alt Avatar', 
						'discount', _avatarDiscount or '')
			GetWidget('lobby_popup_purchase_avatar'):SetVisible(false)

			PlaySound('/shared/sounds/ui/revamp/region_click.wav')
		end)
		GetWidget('lobby_popup_purchase_avatar_silver_buy'):SetVisible(_silverCoins >= avatarInfo.PremiumCost)
	else
		GetWidget('lobby_popup_purchase_avatar_silver'):SetVisible(false)
	end

	if canUseGold and canUseSilver then
		GetWidget('lobby_popup_purchase_avatar_desc'):SetText(Translate('mstore_purchase_choosecurrency_desc'))
	elseif canUseGold then
		GetWidget('lobby_popup_purchase_avatar_desc'):SetText(Translate('mstore_purchase_goldonly'))
	elseif canUseSilver then
		GetWidget('lobby_popup_purchase_avatar_desc'):SetText(Translate('mstore_purchase_silveronly'))
	else
		GetWidget('lobby_popup_purchase_avatar_desc'):SetText('')
	end

	-- Coupon
	local couponTable = GetCardsInfo(_avatarHero, _avatarCode)

	if #couponTable > 0 then
		GetWidget('lobby_popup_purchase_avatar_coupon'):SetVisible(true)
		GetWidget('lobby_popup_purchase_avatar_coupon_info'):SetColor('red')
		GetWidget('lobby_popup_purchase_avatar_coupon_info'):SetText(Translate('mstore_nocards_desc'))

		_avatarDiscount = ''
		for i=1, AVATAR_COUPON_MAXCOUNT do
			GetWidget('lobby_popup_purchase_avatar_coupon_'..i..'_btn'):SetButtonState(0)
			Lobby_V2:UpdateAvatarCoupon(i)
		end
	else
		_avatarDiscount = ''
		GetWidget('lobby_popup_purchase_avatar_coupon'):SetVisible(false)
	end
end

function Lobby_V2:ChangeSelectedAvatar(index)
	if _avatarChanging then return end

	local newSelection = index
	if newSelection < 1 then newSelection = 1 end
	if newSelection > #_avatars then newSelection = #_avatars end

	if newSelection == _selectedAvatar then return end

	_avatarChanging = true
	_selectedAvatar = newSelection

	for i,v in ipairs(_avatars) do
		if i == _selectedAvatar then 
			GetWidget('lobby_avatar_card_'..i..'_scale'):ScaleWidth(AVATAR_LARGESIZE, AVATAR_ANIMATION_TIME)
			GetWidget('lobby_avatar_card_'..i..'_scale'):ScaleHeight(AVATAR_LARGESIZE, AVATAR_ANIMATION_TIME)
		elseif i == _selectedAvatar + 1 or i == _selectedAvatar - 1 then
			GetWidget('lobby_avatar_card_'..i..'_scale'):ScaleWidth(AVATAR_MIDDLESIZE, AVATAR_ANIMATION_TIME)
			GetWidget('lobby_avatar_card_'..i..'_scale'):ScaleHeight(AVATAR_MIDDLESIZE, AVATAR_ANIMATION_TIME)
		else
			GetWidget('lobby_avatar_card_'..i..'_scale'):ScaleWidth(AVATAR_SMALLSIZE, AVATAR_ANIMATION_TIME)
			GetWidget('lobby_avatar_card_'..i..'_scale'):ScaleHeight(AVATAR_SMALLSIZE, AVATAR_ANIMATION_TIME)
		end

		if next then
			if i == _selectedAvatar + 2 then
				GetWidget('lobby_avatar_card_'..i):FadeIn(AVATAR_ANIMATION_TIME)
			elseif i == _selectedAvatar - 3 then
				GetWidget('lobby_avatar_card_'..i):FadeOut(AVATAR_ANIMATION_TIME)
			end
		else
			if i == _selectedAvatar - 2 then
				GetWidget('lobby_avatar_card_'..i):FadeIn(AVATAR_ANIMATION_TIME)
			elseif i == _selectedAvatar + 3 then
				GetWidget('lobby_avatar_card_'..i):FadeOut(AVATAR_ANIMATION_TIME)
			end
		end
	end
 
	GetWidget('lobby_picker_avatars_parent'):SlideX('-'..tostring(AVATAR_OFFSET * (_selectedAvatar-1)).. 'i', AVATAR_ANIMATION_TIME)

	GetWidget('lobby_picker_avatars_area'):Sleep(AVATAR_ANIMATION_TIME, function() 
		_avatarChanging = false
		Lobby_V2:UpdateAvatars()
	end)
end

function Lobby_V2:OnSlideAvatar(forward)
	if forward then
		Lobby_V2:ChangeSelectedAvatar(_selectedAvatar+1)
	else
		Lobby_V2:ChangeSelectedAvatar(_selectedAvatar-1)
	end
end

function Lobby_V2:OnDoubleClickAvatar(index)
	local avatarInfo = _avatars[index]
	if avatarInfo == nil then return end

	local avatarType = Lobby_V2:GetAvatarType(avatarInfo)

	if avatarType == 'available' then
		_selectedAvatar = index
		Lobby_V2:SelectAvatar()
	elseif avatarType == 'purchase' then
		Lobby_V2:PurchaseAvatar()
	end
end

function Lobby_V2:OnMouseOverAvatar(index)
	local widget = GetWidget('lobby_avatar_card_'..index..'_model')
	widget:SetEffect('/ui/fe2/newui/res/effects/glow.effect')
end

function Lobby_V2:OnMouseOutAvatar(index)
	local avatar = _avatars[index]
	local productID = tostring(avatar.ProductID)
	local isNew = false
	local isUltimate = false

	if _avatarsNewUltimateInfo[productID] ~= nil then
		isNew = _avatarsNewUltimateInfo[productID].isNew
		isUltimate = _avatarsNewUltimateInfo[productID].isUltimate
	end

	local widget = GetWidget('lobby_avatar_card_'..index..'_model')
	if isNew and isUltimate then
		widget:SetEffect('/ui/fe2/newui/res/effects/glow_gold_high.effect')
	elseif isNew then
		widget:SetEffect('/ui/fe2/newui/res/effects/glow_blue.effect')
	else
		widget:SetEffect('')
	end
end

function Lobby_V2:OnClickSetDefaultAvatar()
	local state = GetWidget('lobby_picker_avatars_default'):GetButtonState()
	if state == 0 then
		GetWidget('lobby_picker_avatars_default'):SetButtonState(1)
		return
	end

	local avatarInfo = _avatars[_selectedAvatar]
	if avatarInfo == nil then return end
	_avatarDefaultCode = avatarInfo.Name
	GetDBEntry('def_av_'.._avatarHero, _avatarDefaultCode , true)
end

function Lobby_V2:OnClickCoupon(index)
	local couponTable = GetCardsInfo(_avatarHero, _avatarCode)

	local discount = 100
	local state = GetWidget('lobby_popup_purchase_avatar_coupon_'..index..'_btn'):GetButtonState()
	if state == 0 then
		_avatarDiscount = ''
		GetWidget('lobby_popup_purchase_avatar_coupon_info'):SetColor('red')
		GetWidget('lobby_popup_purchase_avatar_coupon_info'):SetText(Translate('mstore_nocards_desc'))
		Lobby_V2:UpdateAvatarCoupon(index)
	else
		discount = 100-couponTable[index].Discount
		for i=1, AVATAR_COUPON_MAXCOUNT do
			if i ~= index then
				GetWidget('lobby_popup_purchase_avatar_coupon_'..i..'_btn'):SetButtonState(0)
			end
			Lobby_V2:UpdateAvatarCoupon(i)
		end
	end

	local avatarInfo = _avatars[_selectedAvatar]
	if avatarInfo == nil then return end

	if avatarInfo.Cost > 0 and avatarInfo.Cost < 9001 then
		local cost = math.floor(avatarInfo.Cost * discount / 100 + 0.5)
		local value = tostring(cost)
		local textWidth = GetWidget('lobby_popup_purchase_avatar_gold_text'):GetStringWidth(value)
		GetWidget('lobby_popup_purchase_avatar_gold_text'):SetText(value)
		GetWidget('lobby_popup_purchase_avatar_gold_text'):SetWidth(textWidth)
		GetWidget('lobby_popup_purchase_avatar_gold_buy'):SetVisible(_goldCoins >= cost)	
	end

	if avatarInfo.PremiumCost > 0 and avatarInfo.PremiumCost < 9001 then
		local cost = math.floor(avatarInfo.PremiumCost * discount / 100 + 0.5)
		local value = tostring(cost)
		local textWidth = GetWidget('lobby_popup_purchase_avatar_silver_text'):GetStringWidth(value)
		GetWidget('lobby_popup_purchase_avatar_silver_text'):SetText(value)
		GetWidget('lobby_popup_purchase_avatar_silver_text'):SetWidth(textWidth)
		GetWidget('lobby_popup_purchase_avatar_silver_buy'):SetVisible(_silverCoins >= cost)
	end
end

function Lobby_V2:PurchaseHero(hero)
	GetWidget('lobby_popup_purchase_hero'):FadeIn(150)

	-- Set Basic Infor
	GetWidget('lobby_popup_purchase_hero_icon'):SetTexture(GetEntityIconPath(hero.heroName))

	GetWidget('lobby_popup_purchase_hero_name'):SetText(hero.displayName)

	-- Set Model
	local modelWidget = GetWidget('lobby_popup_purchase_hero_model')
	modelWidget:SetModel(GetHeroPreviewModelPathFromProduct(hero.heroName))
	modelWidget:SetModelPos(GetHeroStorePosFromProduct(hero.heroName))
	modelWidget:SetModelAngles(GetHeroStoreAnglesFromProduct(hero.heroName))
	modelWidget:SetModelScale(GetHeroStoreScaleFromProduct(hero.heroName))
	modelWidget:SetMultiEffect(GetHeroStorePassiveEffectPathFromProduct(hero.heroName), 0)

	-- Set Price 
	if hero.goldPrice > 0 then
		local value = tostring(hero.goldPrice)
		local textWidth = GetWidget('lobby_popup_purchase_hero_gold_text'):GetStringWidth(value)
		GetWidget('lobby_popup_purchase_hero_gold_text'):SetText(value)
		GetWidget('lobby_popup_purchase_hero_gold_text'):SetWidth(textWidth)
		GetWidget('lobby_popup_purchase_hero_gold'):SetVisible(true)

		if _goldCoins >= hero.goldPrice then
			GetWidget('lobby_popup_purchase_hero_gold_buy'):SetVisible(true)
			GetWidget('lobby_popup_purchase_hero_gold_buy'):SetCallback('onclick', function()
				SubmitForm('LobbyBuyHero', 
							'account_id', tostring(Client.GetAccountID()), 
							'request_code', '8', 
							'cookie', Client.GetCookie(), 
							'hero_name', hero.heroName, 
							'avatar_code', 'Hero', 
							'currency', '0', 
							'type', 'Hero'
							)

				GetWidget('lobby_popup_purchase_hero'):SetVisible(false)

				PlaySound('/shared/sounds/ui/revamp/region_click.wav')
			end)
		else
			GetWidget('lobby_popup_purchase_hero_gold_buy'):SetVisible(false)
		end
	else
		GetWidget('lobby_popup_purchase_hero_gold'):SetVisible(false)
	end

	if hero.silverPrice > 0 then 
		local value = tostring(hero.silverPrice)
		local textWidth = GetWidget('lobby_popup_purchase_hero_silver_text'):GetStringWidth(value)
		GetWidget('lobby_popup_purchase_hero_silver_text'):SetText(value)
		GetWidget('lobby_popup_purchase_hero_silver_text'):SetWidth(textWidth)
		GetWidget('lobby_popup_purchase_hero_silver'):SetVisible(true)

		if _silverCoins >= hero.silverPrice then
			GetWidget('lobby_popup_purchase_hero_silver_buy'):SetVisible(true)
			GetWidget('lobby_popup_purchase_hero_silver_buy'):SetCallback('onclick', function()
				SubmitForm('LobbyBuyHero', 
							'account_id', tostring(Client.GetAccountID()), 
							'request_code', '8', 
							'cookie', Client.GetCookie(), 
							'hero_name', hero.heroName, 
							'avatar_code', 'Hero', 
							'currency', '1', 
							'type', 'Hero'
							)

				GetWidget('lobby_popup_purchase_hero'):SetVisible(false)

				PlaySound('/shared/sounds/ui/revamp/region_click.wav')
			end)
		else
			GetWidget('lobby_popup_purchase_hero_silver_buy'):SetVisible(false)
		end
	else
		GetWidget('lobby_popup_purchase_hero_silver'):SetVisible(false)
	end
end

function Lobby_V2:PurchaseEAP(hero)
	GetWidget('lobby_popup_purchase_eap'):FadeIn(150)
	for i=1,EAP_ITEMCOUNT do
		local avatarName = _EAPInfo[i].productCode
		local widget = GetWidget('lobby_popup_purchase_eap_model_'..i..'_model')
		widget:SetModel(GetHeroPreviewModelPathFromProduct(avatarName))
		widget:SetModelPos(GetHeroStorePosFromProduct(avatarName))
		widget:SetModelAngles(GetHeroStoreAnglesFromProduct(avatarName))
		widget:SetModelScale(GetHeroStoreScaleFromProduct(avatarName))
		widget:SetMultiEffect(GetHeroStorePassiveEffectPathFromProduct(avatarName), 0)

		GetWidget('lobby_popup_purchase_eap_model_'..i..'_nametext'):SetText(_EAPInfo[i].name)
	end

	Lobby_V2:SetEAPItems(EAP_IMMORTAL)
end

function Lobby_V2:SetEAPItems(type)
	local function SetAvatar(i, selected)
		local modelWidget = GetWidget('lobby_popup_purchase_eap_model_'..i..'_model')
		local pededtalWidget = GetWidget('lobby_popup_purchase_eap_model_'..i..'_pedestal')
		local namebgWidget = GetWidget('lobby_popup_purchase_eap_model_'..i..'_namebg')
		local typebgWidget = GetWidget('lobby_popup_purchase_eap_model_'..i..'_typebg')
		local nametextWidget= GetWidget('lobby_popup_purchase_eap_model_'..i..'_nametext')
		local typetextWidget= GetWidget('lobby_popup_purchase_eap_model_'..i..'_typetext')

		if selected then
			modelWidget:SetSunColor(1, 1, 1)
			modelWidget:SetAmbientColor(0.65, 0.65, 0.65)
			pededtalWidget:SetRenderMode('normal')
			namebgWidget:SetRenderMode('normal')
			typebgWidget:SetRenderMode('normal')
			nametextWidget:SetColor('#d7b6a2')
			nametextWidget:SetOutlineColor('#280b0b')
			typetextWidget:SetColor('#d7b6a2')
			typetextWidget:SetOutlineColor('#280b0b')
		else
			modelWidget:SetSunColor(0.2, 0.2, 0.2)
			modelWidget:SetAmbientColor(0.1, 0.1, 0.1)
			pededtalWidget:SetRenderMode('grayscale')
			namebgWidget:SetRenderMode('grayscale')
			typebgWidget:SetRenderMode('grayscale')
			nametextWidget:SetColor('#ffffff')
			nametextWidget:SetOutlineColor('#525252')
			typetextWidget:SetColor('#ffffff')
			typetextWidget:SetOutlineColor('#525252')
		end
	end

	local itemsState = {}
	itemsState[1] = true
	itemsState[2] = type == EAP_LEGENDARY or type == EAP_IMMORTAL
	itemsState[3] = type == EAP_IMMORTAL

	for i=1, EAP_ITEMCOUNT do
		GetWidget('lobby_popup_purchase_eap_type_'..i):SetEnabled(i ~= type)

		SetAvatar(i, itemsState[i])
	end

	GetWidget('lobby_popup_purchase_eap_gold_text'):SetText(tostring(_EAPInfo[type].price))
	GetWidget('lobby_popup_purchase_eap_legendary_selected'):SetVisible(itemsState[2])
	GetWidget('lobby_popup_purchase_eap_immortal_selected'):SetVisible(itemsState[3])

	GetWidget('lobby_popup_purchase_eap_buy'):SetCallback('onclick', function()
		GetWidget('lobby_popup_purchase_eap'):FadeOut(150)
		PlaySound('/shared/sounds/ui/revamp/region_click.wav')

		SubmitForm( 'MicroStore',
				    'account_id', Client.GetAccountID(),
				    'request_code', '4',
					'cookie', Client.GetCookie(),
					'product_id', _EAPInfo[type].productId,
					'category_id', '58',
					'hostTime', GetTime(),
					'currency', 0
				);
	end)
end
----------------------------- Filter --------------------------------------------------
function Lobby_V2:UpdateFilters()
	local text = string.lower(GetWidget('lobby_picker_filter_text'):GetValue())

	local value = GetWidget('lobby_picker_filter_type'):GetValue()
	local key = ''
	if value == '1' then key = 'soloRating' 
	elseif value == '2' then key = 'jungleRating'
	elseif value == '3' then key = 'carryRating'
	elseif value == '4' then key = 'supportrating'
	elseif value == '5' then key = 'initiatorRating'
	elseif value == '6' then key = 'gankerRating'
	elseif value == '7' then key = 'pusherRating'
	end

	for i,v in ipairs(_heroInfos) do
		local include = true
		if NotEmpty(v.heroName) then
			local heroDisplayName = string.lower(Translate("mstore_"..v.heroName.."_name"))

			if NotEmpty(heroDisplayName) and NotEmpty(text) then
				include = string.find(heroDisplayName, text, 1, true)
			end

			local meleeState = GetWidget('lobby_picker_filter_melee'):GetButtonState()
			local rangedState = GetWidget('lobby_picker_filter_ranged'):GetButtonState()

			if meleeState > 0 and rangedState > 0 then
			elseif meleeState > 0 then
				if v.meleeRating >= FILTER_THROSHOLD then 
					include = include
				else
					include = false
				end
			else
				if v.rangedRating >= FILTER_THROSHOLD then 
					include = include
				else
					include = false
				end
			end

			if NotEmpty(key) then
				if v[key] >= FILTER_THROSHOLD then 
					include = include
				else
					include = false
				end
			end

			if GetWidget('lobby_picker_filter_favorite'):GetButtonState() == 1 then
				include = include and _favoriteHeroes[v.heroName] or false
			end
		else
			include = false
		end

		GetWidget(v.rootWidgetName..'_filter'):SetVisible(not include)
	end

	Lobby_V2:SetRandomFilter()
	Lobby_V2:SetRandomFavorite()
end

function Lobby_V2:OnClickFilterButton(type)
	if type == 'melee' then 
		if GetWidget('lobby_picker_filter_ranged'):GetButtonState() == 0 then
			GetWidget('lobby_picker_filter_melee'):SetButtonState(1) 
		end
	else
		if GetWidget('lobby_picker_filter_melee'):GetButtonState() == 0 then
			GetWidget('lobby_picker_filter_ranged'):SetButtonState(1) 
		end
	end

	Lobby_V2:UpdateFilters()
end

function Lobby_V2:OnClickResetFilter()
	GetWidget('lobby_picker_filter_melee'):SetButtonState(1)
	GetWidget('lobby_picker_filter_ranged'):SetButtonState(1)
	GetWidget('lobby_picker_filter_text'):EraseInputLine()
	GetWidget('lobby_picker_filter_type'):SetSelectedItemByValue(0)
	GetWidget('lobby_picker_filter_favorite'):SetButtonState(0)
	Lobby_V2:UpdateFilters()
	SetHeroSelectThreshold(FILTER_THROSHOLD)
end

function Lobby_V2:SetRandomFilter()
	local value = GetWidget('lobby_picker_filter_type'):GetValue()
	local filterName = ''
	if value == '1' then filterName = 'filter_solo' 
	elseif value == '2' then filterName = 'filter_jungle'
	elseif value == '3' then filterName = 'filter_carry'
	elseif value == '4' then filterName = 'filter_support'
	elseif value == '5' then filterName = 'filter_initiator'
	elseif value == '6' then filterName = 'filter_ganker'
	elseif value == '7' then filterName = 'filter_pusher'
	end

	local meleeState = GetWidget('lobby_picker_filter_melee'):GetButtonState()
	local rangedState = GetWidget('lobby_picker_filter_ranged'):GetButtonState()

	if meleeState == 0 then
		if NotEmpty(filterName) then filterName = filterName .. '|' end
		filterName = filterName..'filter_ranged'
	elseif rangedState == 0 then
		if NotEmpty(filterName) then filterName = filterName .. '|' end
		filterName = filterName..'filter_melee'
	end

	TestEcho('^gLobby_V2:SetRandomFilter()~~~~ \''..filterName..'\'')
	SetHeroSelectFilters(filterName)
end

function Lobby_V2:SetRandomFavorite()
	local text = string.lower(GetWidget('lobby_picker_filter_text'):GetValue())
	local isFavorite = GetWidget('lobby_picker_filter_favorite'):GetButtonState() == 1
	local heroes = ''

	for i,v in ipairs(_heroInfos) do
		local heroDisplayName = string.lower(Translate("mstore_"..v.heroName.."_name"))

		if isFavorite then
			if (Empty(text) or string.find(heroDisplayName, text, 1, true)) and _favoriteHeroes[v.heroName] then
				if NotEmpty(heroes) then heroes = heroes..'|' end
				heroes = heroes ..v.heroName
			end
		else
			if NotEmpty(text) and string.find(heroDisplayName, text, 1, true) then
				if NotEmpty(heroes) then heroes = heroes..'|' end
				heroes = heroes ..v.heroName
			end
		end
	end

	TestEcho('^gLobby_V2:SetRandomFavorite()~~~~ \''..heroes..'\'')
	SetHeroSelectFavoriteHeroes(heroes, isFavorite)
end