------------------------
--	HoN Codex Script
-- Copyright 2017 Frostburn Studios
------------------------

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()

RegisterScript2('Codex', '1')

Set('codex_postgame_poptime', '20000', 'number')
Set('codex_gamelength_check', '45000', 'number')

HoN_Codex = _G["HoN_Codex"] or {}	

--------------------------------------------------------------------------------------------------
HoN_Codex.autopopup = false
local MultiLevel_Graffiti_Config = {}
MultiLevel_Graffiti_Config['Hero_Pyromancer.Alt8'] = {'1_1', '1_2', '1_3', '1_4', '1_5'}
MultiLevel_Graffiti_Config['Hero_Valkyrie.Alt12'] = {'2_1', '2_2', '2_3', '2_4', '2_5'}
MultiLevel_Graffiti_Config['Hero_DwarfMagi.Alt6'] = {'3_1', '3_2', '3_3', '3_4', '3_5'}
MultiLevel_Graffiti_Config['Hero_Hiro.Alt12'] = {'4_1', '4_2', '4_3', '4_4', '4_5'}
MultiLevel_Graffiti_Config['Hero_Nomad.Alt12'] = {'5_1', '5_2', '5_3', '5_4', '5_5'}
MultiLevel_Graffiti_Config['Hero_PollywogPriest.Alt7'] = {'6_1', '6_2', '6_3', '6_4', '6_5'}
MultiLevel_Graffiti_Config['Hero_DoctorRepulsor.Alt10'] = {'7_1', '7_2', '7_3', '7_4', '7_5'}

local OneLevel_Graffiti_Config = {}
OneLevel_Graffiti_Config['Hero_Ravenor.Alt8'] = '1'
OneLevel_Graffiti_Config['Hero_Hammerstorm.Alt11'] = '2'
OneLevel_Graffiti_Config['Hero_FlintBeastwood.Alt13'] = '3'
OneLevel_Graffiti_Config['Hero_Chronos.Alt9'] = '4'
OneLevel_Graffiti_Config['Hero_Rampage.Alt14'] = '5'
OneLevel_Graffiti_Config['Purchase'] = 'purchase'
OneLevel_Graffiti_Config['Share'] = 'share'

local MAX_COUNT = 7
local PAGE_INDEX = 9
local MAX_LOTTERY_NUM = 68
local LIST_NUM = 12
local ROULETTE_NUM = 68

local Current_Page = 0
local Total_Page = 0
local RotationStart_CursorPos = 0
local RotationStart_Angle = 0
local Share_Signature_Id = 1

local WallList = {}
local GraffitiItems = {}
local EditedGraffitiItems = {}
local GeneralInfo = {}
local Roulette = {}
local RouletteHistory = {}
local RankInfo = {}
local Community = {}
local NewGraffiti = {}
local NewGraffitiModels = {}

local SpecialAvatars = {}

local DebugAvatars = {}

--------------------------------------------------------------------------------------------------
local function ParseHeroAvatar(m)
	if not m or m =='' then return nil, nil end
	local arr = split(m, '%.')

	local hero = arr[1]
	local avatar = arr[2]

	if avatar == 'Hero' then
		avatar = ''
	elseif not avatar then
		return nil, nil
	else
		avatar = hero..'.'..avatar
	end

	return hero, avatar
end

local function SetHighLightIconOnTheWall(w, texture, frontimg, frontAlpha, backimg, backAlpha, title, titleX, titleY)
	local x = w:GetAbsoluteX() - GetScreenWidth() / 2
	local y = w:GetAbsoluteY() - GetScreenHeight() / 2

	x = math.floor(1080 / GetScreenHeight() * x + 0.5)
	y = math.floor(1080 / GetScreenHeight() * y + 0.5)

	x = x + 1920 / 2
	y = y + 1080 / 2

	if NotEmpty(title) then
		w:SetDynamicTexture(0, texture, 9, tostring(x), tostring(y), frontimg, frontAlpha, backimg, backAlpha, title, tostring(titleX), tostring(titleY))
	else
		w:SetDynamicTexture(0, texture, 6, tostring(x), tostring(y), frontimg, frontAlpha, backimg, backAlpha)
	end
end

local function SetHighLightIconOnTheWall2(w, texture, frontimg, frontAlpha, backimg, backAlpha, color)
	local x = w:GetAbsoluteX()
	local y = w:GetAbsoluteY()
	x = math.floor(1080 / GetScreenHeight() * x + 0.5)
	y = math.floor(1080 / GetScreenHeight() * y + 0.5)

	w:SetDynamicTexture(0, texture, 7, tostring(x), tostring(y), frontimg, frontAlpha, backimg, backAlpha, tostring(tonumber(color)))
end

local function SetDateOnTheWall(w, frontAlpha, backimg, backAlpha)
	local x = w:GetAbsoluteX()
	local y = w:GetAbsoluteY()
	x = math.floor(1080 / GetScreenHeight() * x + 0.5)
	y = math.floor(1080 / GetScreenHeight() * y + 0.5)

	local year, month, day = GetDateStrings()
	local startX = 0

	if string.len(month) > 1 and string.len(day) > 1 then
		startX = 62
	elseif string.len(month) <= 1 and string.len(day) <= 1 then
		startX = 75
	else
		startX = 70
	end

	local sGap = 12
	local bGap = 20
	local strPos = ''

	for i=1, string.len(year) do
		strPos = strPos..string.sub(year,i,i)..','..startX..'/'
		startX = startX + sGap
	end
	startX = startX + bGap - sGap

	for i=1, string.len(month) do
		strPos = strPos..string.sub(month,i,i)..','..startX..'/'
		startX = startX + sGap
	end

	startX = startX + bGap - sGap
	for i=1, string.len(day) do
		strPos = strPos..string.sub(day,i,i)..','..startX..'/'
		startX = startX + sGap
	end
	strPos = string.sub(strPos, 1, -2)

	w:SetDynamicTexture(3, 'graffiti_date', 6, tostring(x), tostring(y), frontAlpha, backimg, backAlpha, strPos)
end

local function ShowMask()
	GetWidget('codex_mask'):SetVisible(true)
end

local function HideMask()
	GetWidget('codex_mask'):SetVisible(false)
end

function HoN_Codex:OnGraffitiWallsResult(responseString)
	Echo('OnGraffitiWallsResult:'..responseString)

	HideMask()

	if not lib_json.RoughCheckJsonString(responseString) then
		Echo('^rOnGraffitiWallsResult failed: json error !!!')
		return
	end

	local json = lib_json.decode(responseString)
	if not json.success then
		Echo('^rOnGraffitiWallsResult failed: '..tostring(json.error))
		return
	end

	local reqType = json.req_type or 'open'
	local popup = json.popup or 1
	local newGraffiti = false
	
	NewGraffiti = {}
	NewGraffitiModels = {}

	if reqType == 'postgame' then
		if popup ~= 1 then return end
		GetWidget('codex'):SetVisible(true)
		newGraffiti = true
	end

	if reqType ~= 'open' and not GetWidget('codex'):IsVisible() then return end

	if GeneralInfo.accountId ~= json.account.account_id then
		WallList = {}
		GraffitiItems = {}
	end

	GeneralInfo.accountId = json.account.account_id
	GeneralInfo.accountName = json.account.nickname
	GeneralInfo.accountIcon = GetAccountIconTexturePathFromUpgrades(json.selected_upgrades or '', GeneralInfo.accountId) 
	GeneralInfo.countOfToday = json.graffiti_count_today or 0
	GeneralInfo.limitationOfOneDay = json.graffiti_daily_limit or 0
	GeneralInfo.rank = json.rank_level or 0
	GeneralInfo.shareCount = json.share_count_today or 0
	GeneralInfo.shareLeftReward = json.share_reward_left_num or 0
	GeneralInfo.hasBoostReward = json.can_boost or false
	Roulette.boostPrice = json.boost_price or 0
 
 	Total_Page = json.page_count or 0
 	Current_Page = json.page or Total_Page

 	if reqType == 'buy' or reqType == 'buyanddraw' then
 		if  Total_Page > 1 then
 			WallList[Total_Page-1] = nil
 		end

 		if reqType == 'buyanddraw' then
			HoN_Codex:OpenRoulette('autostart')
		end
	end

	for i,v in ipairs(json.data) do
		local wall = {}
		wall.wall_id = v.wall_id
		wall.level = v.show_level
		wall.graffitis = {}

		for j,u in ipairs(v.infos) do
			local item = {}
			item.modelName = u.layout_info.avatar.name
			item.modelAngles = tonumber(u.layout_info.avatar.rot)
			item.modelPos = tonumber(u.layout_info.avatar.frame)
			item.level = tonumber(u.layout_info.avatar.level)
			item.friendname = u.buddy_nickname 
			item.selectedAnim = u.layout_info.avatar.ani or 1
			item.selectedAnim = tonumber(item.selectedAnim)

			local killreward = u.layout_info.kill or '' 
			if u.layout_info.kill == 'Annihilation' then
				item.title = '/ui/fe2/codex/killrewards/annihilation.png'
			elseif u.layout_info.kill == 'Immortal' then
				item.title = '/ui/fe2/codex/killrewards/immortal.png'
			elseif u.layout_info.kill == 'Quad Kill' then
				item.title = '/ui/fe2/codex/killrewards/quadkill.png'
			elseif u.layout_info.kill == 'Bloodbath' then
				item.title = '/ui/fe2/codex/killrewards/bloodbath.png'
			elseif u.layout_info.kill == 'Hat-Trick' then
				item.title = '/ui/fe2/codex/killrewards/hattrick.png'
			else
				item.title = ''
			end

			GraffitiItems[u.graffiti_id] = item
			table.insert(wall.graffitis, u.graffiti_id)
		end
		
		if Current_Page > 0 then
			WallList[Current_Page] = wall
		end
	end

	if newGraffiti then
		local graffitis = WallList[Current_Page].graffitis
		if graffitis ~= nil then
			NewGraffiti[#graffitis] = true

			local last = graffitis[#graffitis]
			local item = GraffitiItems[last]
			if NotEmpty(item.friendname) and (#graffitis - 1) >= 1 then
				NewGraffiti[#graffitis - 1] = true
			end 
		end
	end

	HoN_Codex:SetGeneralInfo()
	if Current_Page > 0 then
		HoN_Codex:UpdateWall()
		HoN_Codex:RefreshPages()
	end
end

function HoN_Codex:OnGetLotteryList(responseString)
	Echo('OnGetLotteryList:'..responseString)

	HideMask()

	if not lib_json.RoughCheckJsonString(responseString) then
		Echo('^rOnGetLotteryList failed: json error !!!')
		return
	end

	local json = lib_json.decode(responseString)
	if not json.success then
		Echo('^rOnGetLotteryList failed: '..tostring(json.error))
		return
	end

	Roulette.owned = {}

	for i,v in ipairs(json.data.lottery_list) do
		local id = tonumber(v.gift_id)
		local owned = tonumber(v.owned)

		if owned ~= nil and owned > 0 then
			Roulette.owned[id] = true
		end
	end

	local reqType = json.data.req_type or 'normal'
	local autostart = reqType == 'autostart'
	Roulette.boostPrice = json.data.boost_price or 0
	Roulette.freeTimes = json.data.left_num or 0

	HoN_Codex:RefreshRoulette(autostart)
	if autostart then
		SubmitForm('FormLottery',  'account_id', Client.GetAccountID(), 'cookie', Client.GetCookie(), 'boost', '1')
		ShowMask()
	end
end

function HoN_Codex:OnGetLotteryResult(responseString)
	Echo('OnGetLotteryResult:'..responseString)

	HideMask()

	if not lib_json.RoughCheckJsonString(responseString) then
		Echo('^rOnGetLotteryResult failed: json error !!!')
		return
	end

	local json = lib_json.decode(responseString)
	if not json.success then
		Echo('^rOnGetLotteryResult failed: '..tostring(json.error))
		return
	end

	local lucknum = tonumber(json.data.gift_id)
	local alreadyHas = json.data.super_owned and tonumber(json.data.super_owned) > 0

	if lucknum < 1 or lucknum > MAX_LOTTERY_NUM then return end

	HoN_Codex:StartRoulette(lucknum, alreadyHas)
end

function HoN_Codex:OnGetLotteryHistoryResult(responseString)
	Echo('OnGetLotteryHistoryResult:'..responseString)
	if not lib_json.RoughCheckJsonString(responseString) then
		Echo('^rOnGetLotteryHistoryResult failed: json error !!!')
		return
	end

	local json = lib_json.decode(responseString)
	if not json.success then
		Echo('^rOnGetLotteryHistoryResult failed: '..tostring(json.error))
		return
	end

	RouletteHistory = {}

	for i,v in ipairs(json.data) do
		local item = {}
		item.date = FormatDateTime(tonumber(v.create_time), '%Y/%m/%d', true)
		item.time = FormatDateTime(tonumber(v.create_time), '%H:%M:%S', true)
		item.icon = HoN_Codex:GetRouletteRewardIcon(tonumber(v.gift_id or '0'))
		item.name = HoN_Codex:GetRouletteRewardString(tonumber(v.gift_id or '0'))

		if NotEmpty(item.icon) then
			table.insert(RouletteHistory, item)
		end
	end

	if #RouletteHistory > LIST_NUM then
		GetWidget('codex_roulette_history_scrollbar'):SetMaxValue(#RouletteHistory-LIST_NUM+1)
	else
		GetWidget('codex_roulette_history_scrollbar'):SetMaxValue(1)
	end
	GetWidget('codex_roulette_history_scrollbar'):SetValue(1)
	HoN_Codex:RefreshRouletteHistory()
end

function HoN_Codex:OnGetLadderResult(responseString)
	Echo('OnGetLadderResult:'..responseString)
	if not lib_json.RoughCheckJsonString(responseString) then
		Echo('^rOnGetLadderResult failed: json error !!!')
		return
	end

	local json = lib_json.decode(responseString)
	if not json.success then
		Echo('^rOnGetLadderResult failed: '..tostring(json.error))
		return
	end

	RankInfo = {}

	for i,v in ipairs(json.data) do
		local item = {}
		item.rank = i
		item.name = v.nickname
		item.completed = v.level_max

		table.insert(RankInfo, item)
	end

	local titleItem = {}
	titleItem.rank = 0
	table.insert(RankInfo, 1, titleItem)

	if #RankInfo > LIST_NUM then
		GetWidget('codex_rank_scrollbar'):SetMaxValue(#RankInfo-LIST_NUM+1)
	else
		GetWidget('codex_rank_scrollbar'):SetMaxValue(1)
	end
	GetWidget('codex_rank_scrollbar'):SetValue(1)
	HoN_Codex:RefreshRankInfo()
end

function HoN_Codex:OnBuyWallResult(responseString)
	Echo('OnBuyWallResult:'..responseString)
	if not lib_json.RoughCheckJsonString(responseString) then
		Echo('^rOnBuyWallResult failed: json error !!!')
		return
	end

	local json = lib_json.decode(responseString)
	if not json.success then
		Echo('^rOnBuyWallResult failed: '..tostring(json.error))
		return
	end

	Cmd('ClientRefreshUpgrades')

	if GeneralInfo.hasBoostReward then 
		SubmitForm('FormGraffitiWalls',  'account_id', Client.GetAccountID(), 'cookie', Client.GetCookie(), 'req_type', 'buyanddraw')
	else
		SubmitForm('FormGraffitiWalls',  'account_id', Client.GetAccountID(), 'cookie', Client.GetCookie(), 'req_type', 'buy')
	end
	ShowMask()
end

function HoN_Codex:OnGetCommunityGoal(responseString)
	Echo('OnGetCommunityGoal:'..responseString)
	if not lib_json.RoughCheckJsonString(responseString) then
		Echo('^rOnGetCommunityGoal failed: json error !!!')
		return
	end

	local json = lib_json.decode(responseString)
	if not json.success then
		Echo('^rOnGetCommunityGoal failed: '..tostring(json.error))
		return
	end

	Community.progress = math.floor(json.data.goal_process * 100 + 0.5)
	Community.type = json.data.current_week
	if Community.type < 1 then Community.type = 1 end

	HoN_Codex:SetCommunityGoal()
end

function HoN_Codex:OnConfirmFacebookShare(responseString)
	Echo('OnConfirmFacebookShare:'..responseString)
	if not lib_json.RoughCheckJsonString(responseString) then
		Echo('^rOnConfirmFacebookShare failed: json error !!!')
		return
	end

	local json = lib_json.decode(responseString)
	if not json.success then
		Echo('^rOnConfirmFacebookShare failed: '..tostring(json.error))
		return
	end

	if tonumber(json.data) > 0 then 
		SubmitForm('FormGraffitiWalls',  'account_id', Client.GetAccountID(), 'cookie', Client.GetCookie(), 'req_type', 'share')
	end
end

function HoN_Codex:OnFacebookShare(start, type, successful)
	Echo('^gHoN_Codex:OnFacebookShare:'..start..','..type..','..successful)
	if not AtoB(start) and (tonumber(type) == 3 or tonumber(type) == 4) and AtoB(successful) then
		local wall_id = 0
		if tonumber(type) == 3 then
			wall_id = WallList[Current_Page].wall_id
		end
		SubmitForm('FormShareGraffiti',  'account_id', Client.GetAccountID(), 'cookie', Client.GetCookie(), 'wall_id', wall_id)
	end
end
--------------------------------------------------------------------------------------------------
function HoN_Codex:OnFrame()
	local removeItems = {}
	for k,v in pairs(NewGraffitiModels) do
		local startTime = v.startTime
		local currentTime = GetTime()
		local delta = currentTime - startTime

		if delta >= v.totalTime then
			k:SetAnim(v.anim)
			k:SetAnimSpeed(0)
			k:SetAnimOffsetTime(v.pos)
			NewGraffiti[v.id] = nil
			GetWidget('codex_graffiti_item_back_'..v.id..'_graffiti'):FadeIn(2000)
			PlaySound('/ui/fe2/codex/sounds/graffiti.ogg')
			table.insert(removeItems, k)
		end
	end

	for i,v in ipairs(removeItems) do
		NewGraffitiModels[v] = nil
	end
end

function HoN_Codex:PostGamePopup(match_id)
	if HoN_Codex.autopopup then 
		Echo('HoN_Codex:PostGamePopup: '..tostring(match_id))
		SubmitForm('FormGraffitiWalls',  'account_id', Client.GetAccountID(), 'cookie', Client.GetCookie(), 'req_type', 'postgame', 'match_id', tostring(match_id))
		HoN_Codex.autopopup = false
	end
end

function HoN_Codex:Init()
	HoN_Codex:InitRoulette()

	local showLadder = GetCvarBool('cl_GarenaEnable') 
	GetWidget('codex_ladder_enterance'):SetVisible(showLadder)
	GetWidget('codex_ladder_enterance_text'):SetVisible(showLadder)
	GetWidget('codex_community_enterance'):SetVisible(false)
	GetWidget('codex_community_enterance_text'):SetVisible(false)

	local widget = GetWidget('codex')

	widget:RegisterWatch("GraffitiWallsResult", function(_, ...) HoN_Codex:OnGraffitiWallsResult(...) end)
	widget:RegisterWatch("GetLotteryListResult", function(_, ...) HoN_Codex:OnGetLotteryList(...) end)
	widget:RegisterWatch("LotteryResult", function(_, ...) HoN_Codex:OnGetLotteryResult(...) end)
	widget:RegisterWatch("GetLotteryHistoryResult", function(_, ...) HoN_Codex:OnGetLotteryHistoryResult(...) end)
	widget:RegisterWatch("GetLadderResult", function(_, ...) HoN_Codex:OnGetLadderResult(...) end)
	widget:RegisterWatch("BuyWallResult", function(_, ...) HoN_Codex:OnBuyWallResult(...) end)
	widget:RegisterWatch("GetCommunityGoalResult", function(_, ...) HoN_Codex:OnGetCommunityGoal(...) end)
	widget:RegisterWatch("ShareGraffitiResult", function(_, ...) HoN_Codex:OnConfirmFacebookShare(...) end)
	widget:RegisterWatch("FacebookShareStatus", function(_, ...) HoN_Codex:OnFacebookShare(...) end)
	widget:RegisterWatch("FileDropNotifyTrigger", function(_, ...) HoN_Codex:Editor_ChangeModelOrEffect(...) end)
end

function HoN_Codex:Clear()
	for i=1, MAX_COUNT do
		GetWidget('codex_graffiti_item_front_'..i):SetVisible(false)
		GetWidget('codex_graffiti_item_middle_'..i):SetVisible(false)
		GetWidget('codex_graffiti_item_back_'..i):SetVisible(false)
	end
	GetWidget('codex_bg'):SetTexture('/ui/fe2/codex/bg/1.png')
	GetWidget('codex_painted'):SetText('')
	GetWidget('codex_player_name'):SetText('')
	GetWidget('codex_rank_number'):SetVisible(false)
	GetWidget('codex_wall_index1'):SetTexture('$invis')
	GetWidget('codex_wall_index2'):SetTexture('$invis')
	GetWidget('codex_wall_index3'):SetTexture('$invis')
	GetWidget('codex_wall_index4'):SetTexture('$invis')
	GetWidget('codex_lastpage'):SetVisible(false)
	GetWidget('codex_nextpage'):SetVisible(false)
	GetWidget('codex_popup'):SetVisible(false)
	GetWidget('codex_mask'):SetVisible(false)
	GetWidget('codex_roulette_tip'):SetVisible(false)
	GetWidget('codex_roulette'):SetVisible(false)

	for i=1,PAGE_INDEX do
		GetWidget('codex_page_index_'..i):SetVisible(false)
	end
end

function HoN_Codex:Open()
	if GetCvarBool('cg_debugGraffiti') then
		GetWidget('codex'):SetVisible(true)

		WallList = {}
		GraffitiItems = {}

		GeneralInfo.rank = 1

		Current_Page = 1
        Total_Page = math.ceil(#DebugAvatars / MAX_COUNT)
        Echo('Total Page:'..Total_Page)

        local graffiti_id = 1

        for i=1,Total_Page do
        	local wall = {}
			wall.level = 1
			wall.graffitis = {}

			for j=1,MAX_COUNT do
				if (i-1) * MAX_COUNT + j  > #DebugAvatars then break end

				local item = {}
				item.modelName = DebugAvatars[(i-1) * MAX_COUNT + j]
				item.modelAngles = 0
				item.modelPos = 0.5
				item.level = 1
				item.selectedAnim = 1
				item.friendname = ''

				GraffitiItems[graffiti_id] = item
				table.insert(wall.graffitis, graffiti_id)

				graffiti_id = graffiti_id + 1
			end
			WallList[i] = wall
        end

        HoN_Codex:UpdateWall()
		HoN_Codex:RefreshPages()

		return
	end

	SubmitForm('FormGraffitiWalls',  'account_id', Client.GetAccountID(), 'cookie', Client.GetCookie(), 'req_type', 'normal')
	HoN_Codex:Clear()
	GetWidget('codex'):SetVisible(true)
	ShowMask()
end

function HoN_Codex:Close()
	GetWidget('codex'):SetVisible(false)
	GetWidget('codex_roulette'):SetVisible(false)
	GetWidget('codex_roulette_tip'):SetVisible(false)
	GetWidget('codex_popup'):SetVisible(false)
	GetWidget('codex_mask'):SetVisible(false)
	GetWidget('codex_editor'):SetVisible(false)
	HoN_Codex:SaveEditedItems()
end

function HoN_Codex:SaveEditedItems()
	local data = {}
	for k,v in pairs(EditedGraffitiItems) do
		local item = {}
		item.graffiti_id = k
		item.rot = GraffitiItems[k].modelAngles
		item.frame = GraffitiItems[k].modelPos
		item.ani = GraffitiItems[k].selectedAnim
		
		table.insert(data, item)
	end
	EditedGraffitiItems = {}

	Echo('Edit Number:'..#data)
	if #data == 0 then return end

	local json = lib_json.encode(data)
	SubmitForm( 'FormGraffitiEdit', 
				'account_id', Client.GetAccountID(),
				'cookie', Client.GetCookie(),
				'data', json)
end

function HoN_Codex:SelectPage(i)
	local text = GetWidget('codex_page_index_'..i..'_label1'):GetText()
	local wallnum = tonumber(text)

	if wallnum ~= nil then
		HoN_Codex:SelectWall(wallnum)
	elseif i == 2 then
		text = GetWidget('codex_page_index_3_label1'):GetText()
		wallnum = tonumber(text)
		HoN_Codex:SelectWall(wallnum - 1)
	elseif i == PAGE_INDEX-1 then
		text = GetWidget('codex_page_index_7_label1'):GetText()
		wallnum = tonumber(text)
		HoN_Codex:SelectWall(wallnum + 1)
	end
end

function HoN_Codex:TurningPage(nextpage)
	if nextpage then
		if Current_Page < Total_Page then
			HoN_Codex:SelectWall(Current_Page + 1)
		end
	else
		if Current_Page > 1 then
			HoN_Codex:SelectWall(Current_Page - 1)
		end
	end
end

function HoN_Codex:RefreshPages()
	local function SetSelected(i, selected)
		if selected then
			GetWidget('codex_page_index_'..i..'_label1'):SetColor('#2fbff5')
			GetWidget('codex_page_index_'..i..'_label2'):SetColor('#5fefff')
			GetWidget('codex_page_index_'..i..'_label3'):SetColor('#5fefff')
		else
			GetWidget('codex_page_index_'..i..'_label1'):SetColor('#827474')
			GetWidget('codex_page_index_'..i..'_label2'):SetColor('#c2b4b4')
			GetWidget('codex_page_index_'..i..'_label3'):SetColor('#c2b4b4')
		end
	end

	local function SetText(i, text)
		GetWidget('codex_page_index_'..i..'_label1'):SetText(text)
		GetWidget('codex_page_index_'..i..'_label2'):SetText(text)
		GetWidget('codex_page_index_'..i..'_label3'):SetText(text)

		local number = tonumber(text)
		local w = GetWidget('codex_page_index_'..i)

		if number == nil then
			w:SetWidth('30i')
		elseif number >= 100 then
			w:SetWidth('36i')
		elseif number >= 10 then
			w:SetWidth('30i')
		elseif number > 0 then
			w:SetWidth('24i')
		end
	end


	local total = Total_Page
	local selected = Current_Page

	if total <= PAGE_INDEX then
		for i=1,PAGE_INDEX do
			GetWidget('codex_page_index_'..i):SetVisible(i <= total)
			SetSelected(i, i == selected)
			SetText(i, tostring(i))
		end
		GetWidget('codex_page_index_1'):SetVisible(total > 1)
		GetWidget('codex_lastpage'):SetVisible(total > 1)
		GetWidget('codex_nextpage'):SetVisible(total > 1)
	else
		local leftpoints = selected >= 5
		local rightpoints = selected < (total - 3)
		local wallnum = {}
		if leftpoints and rightpoints then
			for i= 3, PAGE_INDEX-2 do
				wallnum[i] = selected - 5 + i
			end
		elseif leftpoints then
			for i= 3, PAGE_INDEX-2 do
				wallnum[i] = total - 9 + i
			end
		elseif rightpoints then
			for i= 3, PAGE_INDEX-2 do
				wallnum[i] = i
			end
		else
			return
		end

		for i=1,PAGE_INDEX do
			GetWidget('codex_page_index_'..i):SetVisible(true)

			if i == 1 then
				SetSelected(i, i == selected)
				SetText(i, tostring(i))
			elseif i == PAGE_INDEX then
				SetSelected(i, total == selected)
				SetText(i, tostring(total))
			elseif i == 2 then
				if leftpoints then
					SetSelected(i, false)
					SetText(i, '...')
				else
					SetSelected(i, i == selected)
					SetText(i, tostring(i))
				end
			elseif i == PAGE_INDEX-1 then
				if rightpoints then
					SetSelected(i, false)
					SetText(i, '...')
				else
					SetSelected(i, (total-1) == selected)
					SetText(i, tostring(total-1))
				end
			else
				SetSelected(i, wallnum[i] == selected)
				SetText(i, tostring(wallnum[i]))
			end
		end	
		GetWidget('codex_lastpage'):SetVisible(true)
		GetWidget('codex_nextpage'):SetVisible(true)
	end

	GetWidget('codex_lastpage'):SetEnabled(selected > 1)
	GetWidget('codex_nextpage'):SetEnabled(selected < total)
end

function HoN_Codex:SetGeneralInfo()
	GetWidget('codex_player_name'):SetText(GeneralInfo.accountName)
	GetWidget('codex_painted'):SetText(Translate('graffiti_painted_text', 'current', tostring(GeneralInfo.countOfToday), 'total', tostring(GeneralInfo.limitationOfOneDay)))

	if Empty(GeneralInfo.accountIcon) then 
		GetWidget('codex_player_icon'):SetAvatar('http://www.heroesofnewerth.com/getAvatar.php?id='..GeneralInfo.accountId)
	else
		GetWidget('codex_player_icon'):SetTexture(GeneralInfo.accountIcon)
	end
end

function HoN_Codex:SelectWall(wallIndex)
	if wallIndex == Current_Page then return end

	if WallList[wallIndex] == nil then
		SubmitForm('FormGraffitiWalls',  'account_id', Client.GetAccountID(), 'cookie', Client.GetCookie(), 'req_type', 'pageturning', 'page', tostring(wallIndex))
		ShowMask()
	else
		ShowMask()
		GetWidget('codex'):Sleep(1, function()
				Current_Page = wallIndex
				HoN_Codex:UpdateWall()
				HoN_Codex:RefreshPages()
				HideMask()
			end)
	end
end

function HoN_Codex:UpdateOneItem(i)
	local wall = WallList[Current_Page]
	if wall == nil then return end

	local graffitiId = wall.graffitis[i]
	local item = GraffitiItems[graffitiId]

	GetWidget('codex_graffiti_item_front_'..i):SetVisible(item ~= nil)
	GetWidget('codex_graffiti_item_middle_'..i):SetVisible(item ~= nil)
	GetWidget('codex_graffiti_item_back_'..i):SetVisible(item ~= nil)
		

	if item ~= nil then
		local editedItem = GraffitiItems[graffitiId]
		if editedItem ~= nil then
			item = editedItem
		end

		Echo('model'..i..':'..item.modelName)
		HoN_Codex:SetGraffitiInfo(i, item)
		HoN_Codex:SetModel(i, item.modelName)
		HoN_Codex:SetModelAngles(i, item.modelAngles)
		if item.modelName ~= 'Purchase' and item.modelName ~= 'Share' then
			local anim = HoN_Codex:SetModelAnim(i)
			HoN_Codex:SetModelPos(i, anim, item.modelPos, true)
			HoN_Codex:SetModelPos(i, anim, item.modelPos, false)
		end	
	end
end

function HoN_Codex:UpdateWall()
	local wall = WallList[Current_Page]
	if wall == nil then return end

	for i=1, MAX_COUNT do
		HoN_Codex:UpdateOneItem(i)
	end

	GetWidget('codex_bg'):SetTexture('/ui/fe2/codex/bg/'..wall.level..'.png')

	local wallIndex = Current_Page
	if wallIndex >= 10000 then wallIndex = 9999 end

	if wallIndex < 100 then
		local index1 = math.floor(wallIndex/10)
		local index2 = wallIndex % 10
	
		SetHighLightIconOnTheWall(GetWidget('codex_wall_index1'), 
							  'Graffiti_WallIndex1', 
							  '/ui/fe2/codex/number/'..index1..'.png', 
							  '1.0',
							  '/ui/fe2/codex/bg/'..wall.level..'.png',
							  '0.6')

		SetHighLightIconOnTheWall(GetWidget('codex_wall_index2'), 
							  'Graffiti_WallIndex2', 
							  '/ui/fe2/codex/number/'..index2..'.png', 
							  '1.0',
							  '/ui/fe2/codex/bg/'..wall.level..'.png',
							  '0.6')
		GetWidget('codex_wall_index3'):SetTexture('$invis')
		GetWidget('codex_wall_index4'):SetTexture('$invis')
	elseif wallIndex < 1000 then
		local index1 = math.floor(wallIndex/100)
		local index2 = math.floor((wallIndex % 100) / 10)
		local index3 = wallIndex % 10
	
		SetHighLightIconOnTheWall(GetWidget('codex_wall_index3'), 
							  'Graffiti_WallIndex3', 
							  '/ui/fe2/codex/number/'..index1..'.png', 
							  '1.0',
							  '/ui/fe2/codex/bg/'..wall.level..'.png',
							  '0.6')

		SetHighLightIconOnTheWall(GetWidget('codex_wall_index1'), 
							  'Graffiti_WallIndex1', 
							  '/ui/fe2/codex/number/'..index2..'.png', 
							  '1.0',
							  '/ui/fe2/codex/bg/'..wall.level..'.png',
							  '0.6')
		SetHighLightIconOnTheWall(GetWidget('codex_wall_index2'), 
							  'Graffiti_WallIndex2', 
							  '/ui/fe2/codex/number/'..index3..'.png', 
							  '1.0',
							  '/ui/fe2/codex/bg/'..wall.level..'.png',
							  '0.6')
		GetWidget('codex_wall_index4'):SetTexture('$invis')
	else
		local index1 = math.floor(wallIndex/1000)
		local index2 = math.floor((wallIndex % 1000) / 100)
		local index3 = math.floor((wallIndex % 100) / 10)
		local index4 = wallIndex % 10

		SetHighLightIconOnTheWall(GetWidget('codex_wall_index4'), 
							  'Graffiti_WallIndex4', 
							  '/ui/fe2/codex/number/'..index1..'.png', 
							  '1.0',
							  '/ui/fe2/codex/bg/'..wall.level..'.png',
							  '0.6')

		SetHighLightIconOnTheWall(GetWidget('codex_wall_index3'), 
							  'Graffiti_WallIndex3', 
							  '/ui/fe2/codex/number/'..index2..'.png', 
							  '1.0',
							  '/ui/fe2/codex/bg/'..wall.level..'.png',
							  '0.6')
		SetHighLightIconOnTheWall(GetWidget('codex_wall_index1'), 
							  'Graffiti_WallIndex1', 
							  '/ui/fe2/codex/number/'..index3..'.png', 
							  '1.0',
							  '/ui/fe2/codex/bg/'..wall.level..'.png',
							  '0.6')
		SetHighLightIconOnTheWall(GetWidget('codex_wall_index2'), 
							  'Graffiti_WallIndex2', 
							  '/ui/fe2/codex/number/'..index4..'.png', 
							  '1.0',
							  '/ui/fe2/codex/bg/'..wall.level..'.png',
							  '0.6')
	end


	if GeneralInfo.rank <= 0 then 
		GetWidget('codex_rank_number'):SetVisible(false)
	else
		GetWidget('codex_rank_number'):SetVisible(true)

		if GeneralInfo.rank == 1 then
			SetHighLightIconOnTheWall2(GetWidget('codex_rank_number_rank'), 
							 	 	  'Graffiti_Rank', 
							  	 	  '/ui/fe2/codex/rank.png', 
							 	 	  '1.0',
							 	 	  '/ui/fe2/codex/bg/'..wall.level..'.png',
							 	  	  '0.6',
							 	  	  '0xffbb2a')
		elseif GeneralInfo.rank > 1 and GeneralInfo.rank <= 10 then
			SetHighLightIconOnTheWall2(GetWidget('codex_rank_number_rank'), 
							 	 	  'Graffiti_Rank', 
							  	 	  '/ui/fe2/codex/rank.png', 
							 	 	  '1.0',
							 	 	  '/ui/fe2/codex/bg/'..wall.level..'.png',
							 	  	  '0.6',
							 	  	  '0x69e7f3')
		elseif GeneralInfo.rank > 10 then
			SetHighLightIconOnTheWall(GetWidget('codex_rank_number_rank'), 
							 	 	  'Graffiti_Rank', 
							  	 	  '/ui/fe2/codex/rank.png', 
							 	 	  '1.0',
							 	 	  '/ui/fe2/codex/bg/'..wall.level..'.png',
							 	  	  '0.6')
		end

		local texture = {}

		if GeneralInfo.rank < 10 then
			texture[1] = '/ui/fe2/codex/number/0s.png'
			texture[2] = '/ui/fe2/codex/number/'..GeneralInfo.rank..'s.png'
		elseif GeneralInfo.rank < 100 then
			local index1 = math.floor(GeneralInfo.rank / 10)
			local index2 = GeneralInfo.rank % 10

			texture[1] = '/ui/fe2/codex/number/'..index1..'s.png'
			texture[2] = '/ui/fe2/codex/number/'..index2..'s.png'

		elseif GeneralInfo.rank < 1000 then
			local index1 = math.floor(GeneralInfo.rank / 100)
			local index2 = math.floor((GeneralInfo.rank % 100) / 10)
			local index3 = GeneralInfo.rank % 10

			texture[1] = '/ui/fe2/codex/number/'..index1..'s.png'
			texture[2] = '/ui/fe2/codex/number/'..index2..'s.png'
			texture[3] = '/ui/fe2/codex/number/'..index3..'s.png'
		else
			texture[1] = '/ui/fe2/codex/number/9s.png'
			texture[2] = '/ui/fe2/codex/number/9s.png'
			texture[3] = '/ui/fe2/codex/number/9s.png'
		end

		for i=1,3 do
			if NotEmpty(texture[i]) then
				if GeneralInfo.rank == 1 then
					SetHighLightIconOnTheWall2(GetWidget('codex_rank_number_'..i), 
							 	 		  	  'Graffiti_RankIndex'..i, 
							  			  	   texture[i], 
							 			 	  '1.0',
							 	  		  	  '/ui/fe2/codex/bg/'..wall.level..'.png',
							 	  		 	  '0.6',
							 	  		 	  '0xffbb2a')
				elseif GeneralInfo.rank > 1 and GeneralInfo.rank <= 10 then
					SetHighLightIconOnTheWall2(GetWidget('codex_rank_number_'..i), 
							 	 		  	  'Graffiti_RankIndex'..i, 
							  			  	   texture[i], 
							 			 	  '1.0',
							 	  		  	  '/ui/fe2/codex/bg/'..wall.level..'.png',
							 	  		 	  '0.6',
							 	  		 	  '0x69e7f3')
				elseif GeneralInfo.rank > 10 then
					SetHighLightIconOnTheWall(GetWidget('codex_rank_number_'..i), 
							 	 		  	  'Graffiti_RankIndex'..i, 
							  			  	   texture[i], 
							 			 	  '1.0',
							 	  		  	  '/ui/fe2/codex/bg/'..wall.level..'.png',
							 	  		 	  '0.6')
				end
			else
				GetWidget('codex_rank_number_'..i):SetTexture('$invis')
			end
		end
	end
end

function HoN_Codex:RecordAnglesChange(id, angles)
	local graffitiId = WallList[Current_Page].graffitis[id]
	if GraffitiItems[graffitiId].modelAngles ~= angles then
		GraffitiItems[graffitiId].modelAngles = angles
		EditedGraffitiItems[graffitiId] = true
	end
end

function HoN_Codex:RecordPosChange(id, pos)
	local graffitiId = WallList[Current_Page].graffitis[id]
	if GraffitiItems[graffitiId].modelPos ~= pos then
		GraffitiItems[graffitiId].modelPos = pos
		EditedGraffitiItems[graffitiId] = true
	end
end

function HoN_Codex:RecordAnimChange(id, anim)
	local graffitiId = WallList[Current_Page].graffitis[id]
	if GraffitiItems[graffitiId].selectedAnim ~= anim then
		GraffitiItems[graffitiId].selectedAnim = anim
		EditedGraffitiItems[graffitiId] = true
	end
end

function HoN_Codex:SetSliderVisible(id, visible)
	local wall = WallList[Current_Page]
	if wall == nil then return end

	local graffitiId = wall.graffitis[id]
	local item = GraffitiItems[graffitiId]

	if visible and NewGraffiti[id] == nil and item.modelName ~= 'Purchase' and item.modelName ~= 'Share' then
		for i=1, MAX_COUNT do
			GetWidget('codex_graffiti_item_front_'..i..'_avatar_editor'):SetVisible(false)
			GetWidget('codex_graffiti_item_front_'..i..'_avatar_editor2'):SetVisible(false)
		end
		GetWidget('codex_graffiti_item_front_'..id..'_avatar_editor'):SetVisible(true)
		GetWidget('codex_graffiti_item_front_'..id..'_avatar_editor2'):SetVisible(true)
	else
		GetWidget('codex_graffiti_item_front_'..id..'_avatar_editor'):SetVisible(false)
		GetWidget('codex_graffiti_item_front_'..id..'_avatar_editor2'):SetVisible(false)
	end
end

-- Set Graffiti Information
function HoN_Codex:SetGraffitiInfo(id, item)
	local heroAvatar = item.modelName
	local level = item.level
	local friendname = item.friendname
	local wall = WallList[Current_Page]

	local w = GetWidget('codex_graffiti_item_back_'..id..'_graffiti')
	local icon = 'default'

	if MultiLevel_Graffiti_Config[heroAvatar] ~= nil then
		if MultiLevel_Graffiti_Config[heroAvatar][level] ~= nil then
			icon = MultiLevel_Graffiti_Config[heroAvatar][level]
		end
	elseif OneLevel_Graffiti_Config[heroAvatar] ~= nil then
		icon = OneLevel_Graffiti_Config[heroAvatar]
	end

	-- Set HightLight Effect Texture
	w:SetVisible(true)

	local titleY = 200
	if icon == 'default' then titleY = 155 end

	SetHighLightIconOnTheWall(w, 
							  'Graffiti_'..tostring(id), 
							  '/ui/fe2/codex/icons/'..icon..'.png', 
							  '1.0',
							  '/ui/fe2/codex/bg/'..wall.level..'.png',
							  '0.6',
							  item.title,
							  125,
							  titleY)
	
	local p = GetWidget('codex_graffiti_item_front_'..id..'_friend')
	local s = GetWidget('codex_graffiti_item_front_'..id..'_friend_name')
	if Empty(friendname) then
		p:SetVisible(false)
	else
		p:SetVisible(true)
		s:SetText(friendname)
	end

	if NewGraffiti[id] ~= nil then
		w:SetVisible(false)
	end
end

-- Set Model Information
function HoN_Codex:SetModel(id, heroAvatar)
	local w = GetWidget('codex_graffiti_item_middle_'..id..'_avatar')
	w:SetModel('')
	w:SetEffect('')

	if Empty(heroAvatar) then return end

	local model = ''
	local effect = ''
	local enableEffect = false
	local scale = 0.5
	local ambient = {x=1.0, y=1.0, z=1.0}
	local sunHeight = 0
	local sunAngle = 0
	local sunColor = {x=0, y=0, z=0}

	local hero, avatar = ParseHeroAvatar(heroAvatar)
	if hero == nil then
		hero = heroAvatar
		avatar = ''
	end

	local data = GetHeroPreviewDataFromDB(hero, avatar, 'large')
	if data ~= nil then
		model = data.modelPath
		effect = data.effectPath
		scale = data.modelScale
		ambient = data.ambient
		sunHeight = data.sunHeight
		sunAngle = data.sunAngle
		sunColor = data.sunColor
	end

	if SpecialAvatars[heroAvatar] ~= nil then
		model = SpecialAvatars[heroAvatar].model or model
		effect = SpecialAvatars[heroAvatar].effect or effect
		enableEffect = SpecialAvatars[heroAvatar].enableEffect or enableEffect
		scale = SpecialAvatars[heroAvatar].scale or scale
	end

	w:SetModel(model)
	w:SetEffect(effect)
	w:SetModelScale(scale * 0.4)
	w:SetModelPos(0, 0, 0)
	w:SetAmbientColor(ambient.x, ambient.y, ambient.z)
	w:SetSunPosition(sunHeight, sunAngle)
	w:SetSunColor(sunColor.x, sunColor.y, sunColor.z)
	w:SetPauseEffect(not enableEffect)
end

function HoN_Codex:SetModelAnim(id)
	local wall = WallList[Current_Page]
	if wall == nil then return end
	local graffitiId = wall.graffitis[id]
	local item = GraffitiItems[graffitiId]

	local w = GetWidget('codex_graffiti_item_middle_'..id..'_avatar')
	local totalAnim = {'showoff1', 'showoff2', 'showoff3', 'showoff4', 'showoff5', 'taunt_1', 'attack_1', 'walk_1', 'bored_1', 'idle'}
	local validAnim = {}

	for i,v in ipairs(totalAnim) do
		if w:HasAnim(v) then
			table.insert(validAnim, v)
		end
	end

	local selected = item.selectedAnim
	if selected > #validAnim or selected < 1 then selected = 1 end

	for i=1,5 do
		GetWidget('codex_graffiti_editor'..id..'_btn'..i):SetVisible(i <= #validAnim and #validAnim > 1)
		GetWidget('codex_graffiti_editor'..id..'_btn'..i):SetEnabled(i ~= selected)
		GetWidget('codex_graffiti_editor'..id..'_disablebtn'..i):SetVisible(i == selected)
	end

	if (#validAnim < 1) then return end

	w:SetAnim(validAnim[selected])
	return validAnim[selected]
end

function HoN_Codex:SetModelPos(id, anim, value, updateSlider)
	if value < 0 then value = 0 end
	if value > 1 then value = 1 end

	if updateSlider then
		local w = GetWidget('codex_graffiti_item_front_'..id..'_avatar_editor')
		w:SetValue(value)
	else
		HoN_Codex:RecordPosChange(id, value)

		value = value * 0.95 + 0.01
		local w = GetWidget('codex_graffiti_item_middle_'..id..'_avatar')

		if NewGraffiti[id] == nil or anim == nil then
			w:SetAnimSpeed(0)
			w:SetAnimOffsetTime(value)
		else
			w:SetAnim(anim)
			PlaySound('/ui/fe2/codex/sounds/appearing.ogg')
			NewGraffitiModels[w] = {anim = anim, pos = value, startTime = GetTime(), totalTime = w:GetCurrentAnimTotalTime(), id = id}
		end	
	end
end

function HoN_Codex:SetModelAngles(id, angles)
	local b = GetWidget('codex_graffiti_item_middle_'..id..'_avatar')
	b:SetModelAngles('0 0 '..tostring(angles))

	HoN_Codex:RecordAnglesChange(id, angles)
end

function HoN_Codex:StartRotation(id)
	if NewGraffiti[id] ~= nil then return end

	local f = GetWidget('codex_graffiti_item_front_'..id..'_avatar')
	local b = GetWidget('codex_graffiti_item_middle_'..id..'_avatar')
	f:SetWatch('Frametime')
	RotationStart_CursorPos = Input.GetCursorPosX()
	_, _, RotationStart_Angle = b:GetModelAngles()
end

function HoN_Codex:EndRotation(id)
	local f = GetWidget('codex_graffiti_item_front_'..id..'_avatar')
	f:SetWatch('')
end

function HoN_Codex:UpdateRotation(id)
	local f = GetWidget('codex_graffiti_item_front_'..id..'_avatar')
	local b = GetWidget('codex_graffiti_item_middle_'..id..'_avatar')
	
	local currentX = Input.GetCursorPosX()
	local delta = currentX - RotationStart_CursorPos
	local deltaRate = delta / math.max(1, f:GetWidth())
	local deltaDegree = deltaRate * 360
		
	local angles = deltaDegree + RotationStart_Angle

	while angles < 0 do angles = angles + 360 end
	while angles > 360 do angles = angles - 360 end

	HoN_Codex:SetModelAngles(id, angles)
end
----------------------------- RANK ---------------------------------------------------------------------------
function HoN_Codex:RefreshRankInfo()
	local startindex = tonumber(GetWidget('codex_rank_scrollbar'):GetValue())
	local maxvalue = tonumber(GetWidget('codex_rank_scrollbar'):UICmd("GetScrollbarMaxValue()"))
	if startindex < 1 then startindex = 1 end
	if startindex > maxvalue then startindex = maxvalue end 

	for i=1,LIST_NUM do
		local index = i+startindex-1

		GetWidget('codex_rank_item_'..i):SetVisible(index <= #RankInfo)

		local item = RankInfo[index]

		if item ~= nil then
			if item.rank <= 0 then
				GetWidget('codex_rank_item_'..i..'_rank_text'):SetText(Translate('graffiti_rank_title_rank'))
				GetWidget('codex_rank_item_'..i..'_rank_image'):SetTexture('$invis')
				GetWidget('codex_rank_item_'..i..'_name'):SetText(Translate('graffiti_rank_title_name'))
				GetWidget('codex_rank_item_'..i..'_completed'):SetText(Translate('graffiti_rank_title_completed'))
				GetWidget('codex_rank_item_'..i..'_reward_text'):SetText(Translate('graffiti_rank_title_reward'))
				GetWidget('codex_rank_item_'..i..'_reward_image'):SetTexture('$invis')
			else
				if item.rank <= 3 then
					GetWidget('codex_rank_item_'..i..'_rank_text'):SetText('')
					GetWidget('codex_rank_item_'..i..'_rank_image'):SetTexture('/ui/fe2/codex/rank'..item.rank..'.png')
				else
					GetWidget('codex_rank_item_'..i..'_rank_text'):SetText(tostring(item.rank))
					GetWidget('codex_rank_item_'..i..'_rank_image'):SetTexture('$invis')
				end
				GetWidget('codex_rank_item_'..i..'_name'):SetText(item.name)
				GetWidget('codex_rank_item_'..i..'_completed'):SetText(tostring(item.completed))
				GetWidget('codex_rank_item_'..i..'_reward_text'):SetText('')

				local rewardIcon = '$invis'
				if item.rank == 1 then 
					rewardIcon = '/ui/fe2/codex/Graffiti_Master2018.tga'
				elseif item.rank <= 10 then 
					rewardIcon = '/ui/fe2/codex/Graffiti_Star2018.tga'
				elseif item.rank <= 100 then
					rewardIcon = '/ui/fe2/codex/Graffiti_Hero2018.tga'
				end

				if rewardIcon ~= nil then 
					GetWidget('codex_rank_item_'..i..'_reward_image'):SetTexture(rewardIcon)
				else
					GetWidget('codex_rank_item_'..i..'_reward_image'):SetTexture('$invis')
				end
			end
		end
	end
end

----------------------------- POPUP WINDOW -------------------------------------------------------------------
function HoN_Codex:OnClickBoost()
	local lottery = GeneralInfo.hasBoostReward

	GetWidget('codex_popup_text'):SetFont('square_22')
	if lottery then
		local text = Translate('graffiti_roulette_desc1', 'gold', tostring(Roulette.boostPrice))
		if not GetCvarBool('cl_GarenaEnable')  then
			text = text..'\n'..Translate('graffiti_roulette_subaccountwarning')
			GetWidget('codex_popup_text'):SetFont('square_20')
		end
		GetWidget('codex_popup_text'):SetText(text)
	else
		GetWidget('codex_popup_text'):SetText(Translate('graffiti_roulette_desc2', 'gold', tostring(Roulette.boostPrice)))
	end
	GetWidget('codex_popup_confirm_btn'):ClearCallback('onclick')
	GetWidget('codex_popup_confirm_btn'):SetCallback('onclick', function()
		local goldcoin = tonumber(UIGoldCoins())
		Echo('goldcoin='..tostring(goldcoin))

		if goldcoin < Roulette.boostPrice then
			GetWidget('codex'):Sleep(500, function()
				HoN_Codex:PopupRecharge()
			end)
			
		else
			SubmitForm('FormBuyWall',  'account_id', Client.GetAccountID(), 'cookie', Client.GetCookie())
			ShowMask()
		end
		GetWidget('codex_popup'):FadeOut(250)
	end)

	GetWidget('codex_popup_price_root'):SetVisible(true)
	GetWidget('codex_popup_price'):SetText(tostring(Roulette.boostPrice))
	GetWidget('codex_popup'):FadeIn(250)
end

function HoN_Codex:PopupRecharge()
	GetWidget('codex_popup_price_root'):SetVisible(true)
	GetWidget('codex_popup_price'):SetText(tostring(Roulette.boostPrice))
	GetWidget('codex_popup_text'):SetFont('square_22')
	GetWidget('codex_popup_text'):SetText(Translate('graffiti_roulette_recharge'))
	GetWidget('codex_popup'):FadeIn(250)

	GetWidget('codex_popup_confirm_btn'):ClearCallback('onclick')
	GetWidget('codex_popup_confirm_btn'):SetCallback('onclick', function()

		-- Go to charege
		GetWidget('codex_popup'):FadeOut(250)
		HoN_Codex:Close()
		OpenStoreToBuyCoins(true)
		interface:Sleep(1000, function() 
			OpenStoreToBuyCoins(true)
		end)
	end)
end

function HoN_Codex:PopupTimeOut()
	GetWidget('codex_popup_price_root'):SetVisible(false)
	GetWidget('codex_popup_text'):SetFont('square_22')
	GetWidget('codex_popup_text'):SetText(Translate('mstore_error_connect'))
	GetWidget('codex_popup'):FadeIn(250)

	GetWidget('codex_popup_confirm_btn'):ClearCallback('onclick')
	GetWidget('codex_popup_confirm_btn'):SetCallback('onclick', function()
		GetWidget('codex_popup'):FadeOut(250)
	end)
end
----------------------------- SHARE ---------------------------------------------------------------------------
function HoN_Codex:OpenShareSetting()
	HoN_Codex:OnClickChooseSignature(1)
	GetWidget('codex_share'):FadeIn(250)
	
	if GeneralInfo.shareLeftReward > 0 then
		GetWidget('codex_share_text'):SetText(Translate('graffiti_share_tip1'))
	else
		GetWidget('codex_share_text'):SetText(Translate('graffiti_share_tip2'))
	end
end

function HoN_Codex:OnClickChooseSignature(id)
	Share_Signature_Id = id

	for i=1,5 do
		local w = GetWidget('codex_share_item_'..i)
		if i == Share_Signature_Id then
			w:SetButtonState(2)
		else
			w:SetButtonState(0)
		end
	end
end

function HoN_Codex:OnClickShareMainUI()
	local wall = WallList[Current_Page]
	local w = GetWidget('codex_signature')
	w:SetVisible(true)

	if Share_Signature_Id == 1 then
		SetDateOnTheWall(w, '1.0', '/ui/fe2/codex/bg/'..wall.level..'.png', '0.6')
	else
		SetHighLightIconOnTheWall(w, 
							  	  'codex_signature', 
							  	  '/ui/fe2/codex/signatures/s'..Share_Signature_Id..'.png', 
							 	  '1.0',
							  	  '/ui/fe2/codex/bg/'..wall.level..'.png',
							  	  '0.6')
	end

	GetWidget('codex'):Sleep(200, function() 
			SetFbShareType(3)
			WidgetPicture('codex', 'main', 'share/graffiti.jpg')
			OpenFbBrowser()
			w:SetVisible(false)
	end)	
end
----------------------------- Community Goal ---------------------------------------------------------------
function HoN_Codex:SetCommunityGoal()
	GetWidget('codex_community_back_img'):SetTexture('/ui/fe2/codex/community_goal.png')
	GetWidget('codex_community_goal'):SetText(Translate('graffiti_community_goal'))

	if Community.progress < 100 then 
		GetWidget('codex_community_text'):SetText(Translate('graffiti_community_text', 'value', tostring(Community.progress)..'%'))
		GetWidget('codex_community_front_img'):SetDynamicTexture(1, 'Graffiti_Community_Goal', 2, '/ui/fe2/codex/community_goal.png', tostring(Community.progress))
	else
		GetWidget('codex_community_text'):SetText(Translate('graffiti_community_text2'))
		GetWidget('codex_community_front_img'):SetTexture('/ui/fe2/codex/community_goal100.png')
	end
end
----------------------------- LOTTERY -------------------------------------------------------------------------
Roulette.positionTable = {}
table.insert(Roulette.positionTable, {x = 478, y = 372})
table.insert(Roulette.positionTable, {x = 480, y = 484})
table.insert(Roulette.positionTable, {x = 478, y = 595})
table.insert(Roulette.positionTable, {x = 478, y = 705})

table.insert(Roulette.positionTable, {x = 574, y = 319})
table.insert(Roulette.positionTable, {x = 573, y = 429})
table.insert(Roulette.positionTable, {x = 575, y = 538})
table.insert(Roulette.positionTable, {x = 573, y = 650})
table.insert(Roulette.positionTable, {x = 573, y = 761})

table.insert(Roulette.positionTable, {x = 671, y = 264})
table.insert(Roulette.positionTable, {x = 671, y = 376})
table.insert(Roulette.positionTable, {x = 672, y = 482})
table.insert(Roulette.positionTable, {x = 671, y = 595})
table.insert(Roulette.positionTable, {x = 671, y = 707})
table.insert(Roulette.positionTable, {x = 671, y = 815})

table.insert(Roulette.positionTable, {x = 769, y = 319})
table.insert(Roulette.positionTable, {x = 771, y = 428})
table.insert(Roulette.positionTable, {x = 769, y = 542})
table.insert(Roulette.positionTable, {x = 769, y = 651})
table.insert(Roulette.positionTable, {x = 769, y = 763})

table.insert(Roulette.positionTable, {x = 868, y = 263})
table.insert(Roulette.positionTable, {x = 867, y = 376})
table.insert(Roulette.positionTable, {x = 867, y = 706})
table.insert(Roulette.positionTable, {x = 867, y = 816})

table.insert(Roulette.positionTable, {x = 960, y = 318})
table.insert(Roulette.positionTable, {x = 960, y = 761})

table.insert(Roulette.positionTable, {x = 1053, y = 263})
table.insert(Roulette.positionTable, {x = 1053, y = 376})
table.insert(Roulette.positionTable, {x = 1054, y = 707})
table.insert(Roulette.positionTable, {x = 1053, y = 816})

table.insert(Roulette.positionTable, {x = 1151, y = 318})
table.insert(Roulette.positionTable, {x = 1151, y = 428})
table.insert(Roulette.positionTable, {x = 1151, y = 542})
table.insert(Roulette.positionTable, {x = 1151, y = 652})
table.insert(Roulette.positionTable, {x = 1151, y = 763})

table.insert(Roulette.positionTable, {x = 1248, y = 264})
table.insert(Roulette.positionTable, {x = 1248, y = 376})
table.insert(Roulette.positionTable, {x = 1248, y = 482})
table.insert(Roulette.positionTable, {x = 1248, y = 594})
table.insert(Roulette.positionTable, {x = 1249, y = 708})
table.insert(Roulette.positionTable, {x = 1248, y = 816})

table.insert(Roulette.positionTable, {x = 1347, y = 319})
table.insert(Roulette.positionTable, {x = 1346, y = 431})
table.insert(Roulette.positionTable, {x = 1346, y = 538})
table.insert(Roulette.positionTable, {x = 1347, y = 651})
table.insert(Roulette.positionTable, {x = 1346, y = 761})

table.insert(Roulette.positionTable, {x = 1440, y = 374})
table.insert(Roulette.positionTable, {x = 1439, y = 482})
table.insert(Roulette.positionTable, {x = 1441, y = 596})
table.insert(Roulette.positionTable, {x = 1441, y = 706})

-- 2018 New
table.insert(Roulette.positionTable, {x = 382, y = 429})
table.insert(Roulette.positionTable, {x = 385, y = 541})
table.insert(Roulette.positionTable, {x = 382, y = 653})
table.insert(Roulette.positionTable, {x = 383, y = 761})
table.insert(Roulette.positionTable, {x = 478, y = 814})

table.insert(Roulette.positionTable, {x = 573, y = 870})
table.insert(Roulette.positionTable, {x = 769, y = 870})
table.insert(Roulette.positionTable, {x = 960, y = 870})
table.insert(Roulette.positionTable, {x = 1151, y = 870})
table.insert(Roulette.positionTable, {x = 1347, y = 870})

table.insert(Roulette.positionTable, {x = 1441, y = 814})
table.insert(Roulette.positionTable, {x = 1534, y = 761})
table.insert(Roulette.positionTable, {x = 1534, y = 651})
table.insert(Roulette.positionTable, {x = 1534, y = 537})
table.insert(Roulette.positionTable, {x = 1534, y = 429})

table.insert(Roulette.positionTable, {x = 1151, y = 206})
table.insert(Roulette.positionTable, {x = 960, y = 206})
table.insert(Roulette.positionTable, {x = 768, y = 206})
--------------------------------------------------------------

Roulette.config_path = {}
Roulette.config_path[1] = {1, 5, 10, 68, 21, 67, 27, 66, 36, 42, 
						   47, 65, 64, 63, 62, 61, 60, 41, 59, 30,
						   58, 24, 57, 15, 56, 55, 9, 14, 20, 23, 
						   26, 29, 34, 35, 40, 46, 50, 49, 45, 39,
						   44, 48, 43, 38, 33, 32, 37, 31, 28, 25,
						   22, 16, 17, 18, 19, 13, 12, 11, 6, 7,
						   8, 4, 54, 53, 3, 52, 2, 51}

Roulette.config_path[2] = {51, 52, 53, 54, 4, 55, 56, 15, 57, 24,
						   58, 30, 59, 35, 40, 41, 46, 60, 61, 62, 
						   50, 45, 49, 63, 64, 65, 47, 48, 44, 43, 
						   42, 36, 66, 27, 67, 21, 25, 28, 31, 37, 
						   38, 32, 33, 39, 34, 29, 26, 23, 20, 14, 
						   9, 8, 13, 19, 18, 17, 22, 16, 68, 10, 
						   11, 12, 7, 3, 2, 6, 5, 1}


Roulette.config_path[3] = {22, 21, 68, 10, 5, 1, 51, 52, 2, 6,
						   11, 16, 17, 12, 7, 3, 53, 54, 55, 4, 
						   8, 13, 18, 19, 14, 9, 56, 15, 20, 57,
						   24, 23, 26, 58, 30, 59, 41, 60, 61, 62,
						   63, 50, 46, 40, 35, 29, 34, 39, 45, 49,
						   64, 65, 48, 44, 38, 33, 32, 37, 43, 47,
						   42, 36, 66, 31, 28, 27, 67, 25}


-- Echo('^g~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~')
-- for i,v in ipairs(Roulette.config_path) do
-- 	local checkTable = {}
-- 	for j, u in ipairs(v) do
-- 		checkTable[u] = true
-- 	end

-- 	local checkPass = true
-- 	for j=1,68 do
-- 		if not checkTable[j] then
-- 			checkPass = false
-- 			Echo('Roulette.config_path['..i..'] miss '..j)
-- 		end
-- 	end
-- 	if checkPass then
-- 		Echo('Roulette.config_path['..i..'] is OK !')
-- 	end
-- end

Roulette.config_rewards = {}
Roulette.config_rewards[1] = {4800, 'upgrade'}
Roulette.config_rewards[2] = {4838, '/ui/fe2/codex/rewards/hiro_alt12.png', 'avatar'}
Roulette.config_rewards[3] = {4852, '/ui/fe2/codex/rewards/selected.png', 'selected'}
Roulette.config_rewards[4] = {4813, 'upgrade'}
Roulette.config_rewards[5] = {3609, 'masteryboost5'}
Roulette.config_rewards[6] = {4840, '/ui/fe2/codex/rewards/ai_punk_tough.png', 'accounticon'}
Roulette.config_rewards[7] = {4809, 'upgrade'}
Roulette.config_rewards[8] = {4605, 'masterysuperboost'}
Roulette.config_rewards[9] = {4841, '/ui/fe2/codex/rewards/ai_too_good.png', 'accounticon'}
Roulette.config_rewards[10] = {4797, 'upgrade'}
Roulette.config_rewards[11] = {4801, 'upgrade'}
Roulette.config_rewards[12] = {4849, '/ui/fe2/codex/rewards/announcer_graffiti.png', 'announcer'}
Roulette.config_rewards[13] = {4847, '/ui/fe2/codex/rewards/courier.png', 'courier'}
Roulette.config_rewards[14] = {4814, 'upgrade'}
Roulette.config_rewards[15] = {4815, 'upgrade'}
Roulette.config_rewards[16] = {4829, '/ui/fe2/codex/rewards/valkyrie_alt12.png', 'avatar'}
Roulette.config_rewards[17] = {4606, 'masteryboost10'}
Roulette.config_rewards[18] = {4804, 'upgrade'}
Roulette.config_rewards[19] = {-1, 'plinko80'}
Roulette.config_rewards[20] = {4842, '/ui/fe2/codex/rewards/ai_too_sassy.png', 'accounticon'}
Roulette.config_rewards[21] = {4805, 'upgrade'}
Roulette.config_rewards[22] = {4843, '/ui/fe2/codex/rewards/ai_punk_cool.png', 'accounticon'}
Roulette.config_rewards[23] = {4810, 'upgrade'}
Roulette.config_rewards[24] = {-2, 'plinko120'}
Roulette.config_rewards[25] = {4808, 'upgrade'}
Roulette.config_rewards[26] = {4827, '/ui/fe2/codex/rewards/nomad_alt12.png', 'avatar'}
Roulette.config_rewards[27] = {4825, '/ui/fe2/codex/rewards/dwarf_magi_alt6.png', 'avatar'}
Roulette.config_rewards[28] = {4851, '/ui/fe2/codex/rewards/chatcolor.png', 'chatcolor'}
Roulette.config_rewards[29] = {4606, 'masteryboost10'}
Roulette.config_rewards[30] = {4807, 'upgrade'}
Roulette.config_rewards[31] = {4799, 'upgrade'}
Roulette.config_rewards[32] = {4605, 'masterysuperboost'}
Roulette.config_rewards[33] = {-1, 'plinko80'}
Roulette.config_rewards[34] = {4844, '/ui/fe2/codex/rewards/ai_too_sexy.png', 'accounticon'}
Roulette.config_rewards[35] = {4806, 'upgrade'}
Roulette.config_rewards[36] = {4845, '/ui/fe2/codex/rewards/ai_too_explosive.png', 'accounticon'}
Roulette.config_rewards[37] = {4848, '/ui/fe2/codex/rewards/ward.png', 'ward'}
Roulette.config_rewards[38] = {4850, '/ui/fe2/codex/rewards/taunt.png', 'taunt'}
Roulette.config_rewards[39] = {4816, 'upgrade'}
Roulette.config_rewards[40] = {4823, '/ui/fe2/codex/rewards/pyromancer_alt8.png', 'avatar'}
Roulette.config_rewards[41] = {4812, 'upgrade'}
Roulette.config_rewards[42] = {4798, 'upgrade'}
Roulette.config_rewards[43] = {4802, 'upgrade'}
Roulette.config_rewards[44] = {4846, '/ui/fe2/codex/rewards/ai_too_deadly.png', 'accounticon'}
Roulette.config_rewards[45] = {3609, 'masteryboost5'}
Roulette.config_rewards[46] = {4803, 'upgrade'}
Roulette.config_rewards[47] = {4605, 'masterysuperboost'}
Roulette.config_rewards[48] = {-2, 'plinko120'}
Roulette.config_rewards[49] = {4811, 'upgrade'}
Roulette.config_rewards[50] = {4853, '/ui/fe2/codex/rewards/creep.png', 'creep'}

Roulette.config_rewards[51] = {3609, 'masteryboost5'}
Roulette.config_rewards[52] = {5482, 'upgrade'}
Roulette.config_rewards[53] = {-2, 'plinko120'}
Roulette.config_rewards[54] = {5486, 'upgrade'}
Roulette.config_rewards[55] = {5478, '/ui/fe2/codex/rewards/doctor_repulsor_alt10.png', 'avatar'}
Roulette.config_rewards[56] = {5485, 'upgrade'}
Roulette.config_rewards[57] = {3609, 'masteryboost5'}
Roulette.config_rewards[58] = {5488, 'upgrade'}
Roulette.config_rewards[59] = {5496, '/ui/fe2/codex/rewards/ai_punk_repulsor.png', 'accounticon'}
Roulette.config_rewards[60] = {-1, 'plinko80'}
Roulette.config_rewards[61] = {5483, 'upgrade'}
Roulette.config_rewards[62] = {5480, '/ui/fe2/codex/rewards/pollywogpriest_alt7.png', 'avatar'}
Roulette.config_rewards[63] = {5489, 'upgrade'}
Roulette.config_rewards[64] = {5497, '/ui/fe2/codex/rewards/ai_punk_pollywog.png', 'accounticon'}
Roulette.config_rewards[65] = {5484, 'upgrade'}
Roulette.config_rewards[66] = {5490, '/ui/fe2/codex/rewards/te_punkplay.png', 'tpeffect'}
Roulette.config_rewards[67] = {5487, 'upgrade'}
Roulette.config_rewards[68] = {-1, 'plinko80'}


Roulette.config_topVelocity = 16
Roulette.config_bottomVelocity = 1
Roulette.config_startAcceleration = 4
Roulette.config_endAcceleration = 4
Roulette.lastPos = -1

function HoN_Codex:GetRouletteRewardIcon(i)
	if Roulette.config_rewards[i] == nil then return '' end

	local icon = Roulette.config_rewards[i][2]
	if icon == nil then return '' end

	if icon == 'upgrade' then
		return '/ui/fe2/codex/rewards/roulette_upgrade.png'
	elseif icon == 'masteryboost5' then
		return '/ui/fe2/codex/rewards/roulette_boost5.png'
	elseif icon == 'masteryboost10' then
		return '/ui/fe2/codex/rewards/roulette_boost10.png'
	elseif icon == 'masterysuperboost' then
		return '/ui/fe2/codex/rewards/roulette_supperboost.png'
	elseif icon == 'plinko80' then
		return '/ui/fe2/codex/rewards/roulette_plinko80.png'
	elseif icon == 'plinko120' then
		return '/ui/fe2/codex/rewards/roulette_plinko120.png'
	else
		return icon
	end
end

function HoN_Codex:GetRouletteRewardString(i, isTip)
	if Roulette.config_rewards[i] == nil then return '' end
	local productId = Roulette.config_rewards[i][1]
	local icon = Roulette.config_rewards[i][2]
	if productId == nil or icon == nil then return '' end

	local type = ''
	if Roulette.config_rewards[i][3] ~= nil then
		type = Translate('graffiti_rewardtype_'..Roulette.config_rewards[i][3])
	end

	local content = ''
	if icon == 'upgrade' and isTip then
		content = Translate('graffiti_roulette_upgrade')
	elseif productId == -1 then
		content = Translate('graffiti_roulette_plinko80')
	elseif productId == -2 then
		content = Translate('graffiti_roulette_plinko120')
	else
		content = Translate('mstore_product'..productId..'_name')
	end

	if NotEmpty(type) then
		return type..' - '..content
	else
		return content
	end
end

function HoN_Codex:InitRoulette()
	math.randomseed(GetTime())
	for i,v in ipairs(Roulette.positionTable) do
		local x = v.x - 960
		local y = v.y - 540

		if not WidgetExists('codex_roulette_item_'..i) then
			GetWidget('codex_roulette_item_root'):Instantiate('codex_roulette_item_template', 'id', tostring(i), 'x', tostring(x)..'i', 'y', tostring(y)..'i')
		end

		GetWidget('codex_roulette_item_'..i..'_icon'):SetTexture(HoN_Codex:GetRouletteRewardIcon(i))
		GetWidget('codex_roulette_item_'..i..'_highlight'):SetVisible(false)
		GetWidget('codex_roulette_item_'..i..'_highlight'):SetDynamicTexture(2, 'Graffiti_Roulette_HighLight'..i, 1, HoN_Codex:GetRouletteRewardIcon(i))
	end
end

function HoN_Codex:OpenRoulette(type)
	SubmitForm('FormLotteryList',  'account_id', Client.GetAccountID(), 'cookie', Client.GetCookie(), 'req_type', type)
	GetWidget('codex_roulette'):FadeIn(250)
	ShowMask()
end

function HoN_Codex:RefreshRoulette(autostart)
	local hasMoreReward = false
	for i=1, ROULETTE_NUM do
		GetWidget('codex_roulette_item_'..i..'_owned'):SetVisible(Roulette.owned[i] ~= nil)

		hasMoreReward = hasMoreReward or Roulette.owned[i] == nil
	end

	if not hasMoreReward or Roulette.freeTimes <= 0 then
		GetWidget('codex_roulette_noroulette'):SetVisible(true)
		GetWidget('codex_roulette_start'):SetVisible(false)
		GetWidget('codex_roulette_boost'):SetVisible(false)
	else
		GetWidget('codex_roulette_noroulette'):SetVisible(false)
		GetWidget('codex_roulette_start'):SetEnabled(true)
		GetWidget('codex_roulette_start'):SetVisible(true)
		GetWidget('codex_roulette_boost'):SetVisible(false)

		for i=1,3 do
			GetWidget('codex_roulette_freetimes'..i):SetText(Translate('graffiti_roulette_freetimes', 'num', tostring(Roulette.freeTimes)))
		end
	end
end

function HoN_Codex:CloseRoulette()
	if not Roulette.start then
		GetWidget('codex_roulette'):FadeOut(250)
		GetWidget('codex_roulette_result'):FadeOut(250)
	end
end

function HoN_Codex:StartRoulette(targetid, alreadyHas)
	Roulette.start = true
	Roulette.roulettePath = Roulette.config_path [math.random(1, #Roulette.config_path)]
	Roulette.startPos = math.random(1, #Roulette.roulettePath)
	Roulette.startTime = GetTime()
	Roulette.step = 1
	Roulette.lastPos = -1

	for i,v in ipairs(Roulette.roulettePath) do
		if v == targetid then
			Roulette.targetPos = i
			break
		end
	end

	local time = (Roulette.config_topVelocity - Roulette.config_bottomVelocity) / Roulette.config_endAcceleration
	Roulette.endDistance = math.floor(Roulette.config_topVelocity * time - 0.5 * Roulette.config_endAcceleration * time * time + 0.5) + 2
	if Roulette.endDistance > #Roulette.roulettePath then
		Roulette.endDistance = #Roulette.roulettePath
	end

	local w = GetWidget('codex_roulette_item_100_icon')
	w:SetTexture(HoN_Codex:GetRouletteRewardIcon(targetid))

	local x = Roulette.positionTable[targetid].x -960
	local y = Roulette.positionTable[targetid].y -540
	GetWidget('codex_roulette_result'):SetX(tostring(x)..'i')
	GetWidget('codex_roulette_result'):SetY(tostring(y)..'i')
	GetWidget('codex_roulette_result'):FadeOut(250)
	GetWidget('codex_roulette_mask'):SetVisible(true)
	if alreadyHas then
		GetWidget('codex_roulette_result_duplicated'):SetVisible(true)
		GetWidget('codex_roulette_result_rewardname2'):SetText(Translate('graffiti_roulette_alreadyhas', 'item', HoN_Codex:GetRouletteRewardString(targetid)))
		GetWidget('codex_roulette_result_rewardname'):SetText('')
	else
		GetWidget('codex_roulette_result_duplicated'):SetVisible(false)
		GetWidget('codex_roulette_result_rewardname'):SetText(HoN_Codex:GetRouletteRewardString(targetid))
		GetWidget('codex_roulette_result_rewardname2'):SetText('')
	end
	GetWidget('codex_roulette_tip'):SetVisible(false)

	GetWidget('codex_roulette_noroulette'):SetVisible(true)
	GetWidget('codex_roulette_start'):SetVisible(false)
	GetWidget('codex_roulette_boost'):SetVisible(false)
end

function HoN_Codex:EndRoulette()
	for i=1, ROULETTE_NUM do
		GetWidget('codex_roulette_item_'..i..'_highlight'):SetVisible(false)
	end
	GetWidget('codex_roulette_result'):FadeIn(250)
	PlaySound('/ui/fe2/codex/sounds/congrats.ogg')
end

function HoN_Codex:UpdateRoulette()
	if not Roulette.start then return end

	local pos = -1
	local distance = 0
	local time = (GetTime() - Roulette.startTime) / 1000 

	if Roulette.step == 1 then
		distance = math.floor(0.5 * Roulette.config_startAcceleration * time * time + 0.5)
		pos = ((Roulette.startPos - 1 + distance) %  #Roulette.roulettePath) + 1

		local velocity = Roulette.config_startAcceleration * time
		if velocity >= Roulette.config_topVelocity then
			Roulette.startPos = pos
			Roulette.startTime = GetTime()
			Roulette.step = 2
		end
	elseif Roulette.step == 2 then
		distance = math.floor(Roulette.config_topVelocity * time + 0.5)
		pos = ((Roulette.startPos - 1 + distance) %  #Roulette.roulettePath) + 1

		local endDistance = 0
		if pos > Roulette.targetPos then
			endDistance = #Roulette.roulettePath - (pos - Roulette.targetPos)
		elseif pos < Roulette.targetPos then
			endDistance = Roulette.targetPos - pos
		else
			endDistance = #Roulette.roulettePath
		end

		if endDistance == Roulette.endDistance and distance > math.floor((#Roulette.roulettePath)/2) then
			Roulette.startPos = pos
			Roulette.startTime = GetTime()
			Roulette.step = 3
		end
	elseif Roulette.step == 3 then
		distance =  math.floor(Roulette.config_topVelocity * time - 0.5 * Roulette.config_endAcceleration * time * time + 0.5)
		pos = ((Roulette.startPos - 1 + distance) %  #Roulette.roulettePath) + 1

		local velocity = Roulette.config_topVelocity - Roulette.config_endAcceleration * time
		if velocity <= Roulette.config_bottomVelocity then
			Roulette.startPos = pos
			Roulette.startTime = GetTime()
			Roulette.step = 4
		end
	elseif Roulette.step == 4 then
		distance = math.floor(Roulette.config_bottomVelocity * time + 0.5)
		pos = ((Roulette.startPos - 1 + distance) %  #Roulette.roulettePath) + 1

		if pos == Roulette.targetPos then
			Roulette.step = 5
			GetWidget('codex'):Sleep(1000, function()
				Roulette.start  = false
				HoN_Codex:EndRoulette()
			end)
		end
	else
		pos = Roulette.targetPos
	end

	if pos ~= Roulette.lastPos then
		PlaySound('/ui/fe2/codex/sounds/rolling.ogg')
		Roulette.lastPos = pos
	end

	local value = (GetTime() % 2000) / 2000
	local alpha = math.abs(math.sin(math.pi * 2 * value)) * 0.5 + 0.5
	
	for i,v in ipairs(Roulette.positionTable) do
		local w = GetWidget('codex_roulette_item_'..i..'_highlight')

		if i == Roulette.roulettePath[pos] then
			if Roulette.start then
				w:SetVisible(true)
				w:SetColor('1 1 1 '..tostring(alpha))
			else
				w:SetTexture('$invis')
				w:SetVisible(false)
			end
		else
			w:SetVisible(false)
		end
	end
end

function HoN_Codex:OnClickRouletteResultOK()
	SubmitForm('FormLotteryList',  'account_id', Client.GetAccountID(), 'cookie', Client.GetCookie(), 'req_type', 'normal')

	GetWidget('codex_roulette_result'):FadeOut(250)
	GetWidget('codex_roulette_mask'):SetVisible(false)
	ShowMask()
end

function HoN_Codex:OnClickRouletteResultShare()
	SubmitForm('FormLotteryList',  'account_id', Client.GetAccountID(), 'cookie', Client.GetCookie(), 'req_type', 'normal')
	SetFbShareType(4)
	WidgetPicture('codex_roulette_item_capture', 'main', 'share/roulette.jpg')
	OpenFbBrowser()
	GetWidget('codex_roulette_result'):FadeOut(250)
	GetWidget('codex_roulette_mask'):SetVisible(false)
	ShowMask()
end

function HoN_Codex:OnClickFreeRoulette()
	SubmitForm('FormLottery',  'account_id', Client.GetAccountID(), 'cookie', Client.GetCookie())
	ShowMask()
end

function HoN_Codex:OnTipMouseMove()
	local x = Input.GetCursorPosX()
	local y = Input.GetCursorPosY() -  math.floor(GetScreenHeight() / 1080 * 60 + 0.5)
	GetWidget('codex_roulette_tip'):SetX(tostring(x))
	GetWidget('codex_roulette_tip'):SetY(tostring(y))
end

function HoN_Codex:OnMouseOverRoulette(id)
	if id < 1 or id > ROULETTE_NUM then return end

	local x = Input.GetCursorPosX()
	local y = Input.GetCursorPosY() - math.floor(GetScreenHeight() / 1080 * 60 + 0.5)
	GetWidget('codex_roulette_tip_text'):SetText(HoN_Codex:GetRouletteRewardString(id, true))
	GetWidget('codex_roulette_tip'):SetX(tostring(x))
	GetWidget('codex_roulette_tip'):SetY(tostring(y))
	GetWidget('codex_roulette_tip'):FadeIn(250)
end
------------------------ LOTTERY HISTORY ------------------------------
function HoN_Codex:RefreshRouletteHistory()
	local startindex = tonumber(GetWidget('codex_roulette_history_scrollbar'):GetValue())
	local maxvalue = tonumber(GetWidget('codex_roulette_history_scrollbar'):UICmd("GetScrollbarMaxValue()"))
	if startindex < 1 then startindex = 1 end
	if startindex > maxvalue then startindex = maxvalue end 

	for i=1,LIST_NUM do
		local index = i+startindex-1

		GetWidget('codex_roulette_history_item_'..i):SetVisible(index <= #RouletteHistory)

		local item = RouletteHistory[index]

		if item ~= nil then
			GetWidget('codex_roulette_history_item_'..i..'_date'):SetText(item.date)
			GetWidget('codex_roulette_history_item_'..i..'_time'):SetText(item.time)
			GetWidget('codex_roulette_history_item_'..i..'_icon'):SetTexture(item.icon)
			GetWidget('codex_roulette_history_item_'..i..'_name'):SetText(item.name)
		end
	end
end
------------------------ UpdateEntrance ---------------------------
function HoN_Codex:UpdateEntrance()
	
end
------------------------ MASK -----------------------------------
local MaskShowTime = 0
function HoN_Codex:OnMaskShow()
	MaskShowTime = GetTime()
end

function HoN_Codex:OnTimeOut()
	HideMask()
	HoN_Codex:PopupTimeOut()
end

function HoN_Codex:OnMaskFrame()
	if GetTime() - MaskShowTime > 60000 and GetWidget('codex_mask'):IsVisible() then
		HoN_Codex:OnTimeOut()
	end
end
------------------------ EDITOR -------------------------------------
local Editor_Current = 0
local Editor_Avatar = ''

function HoN_Codex:OpenEditor(id)
	if not GetCvarBool('cg_debugGraffiti') then return end

	local wall = WallList[Current_Page]
	if wall == nil then return end

	Editor_Current = id

	local graffitiId = wall.graffitis[id]
	local item = GraffitiItems[graffitiId]

	Editor_Avatar = item.modelName
	GetWidget('codex_editor_avatarName'):SetText('^gAvatar '..id..' : ^w'..item.modelName)

	local model = ''
	local effect = ''
	local enableEffect = false

	local hero, avatar = ParseHeroAvatar(item.modelName)
	if hero == nil then
		hero = item.modelName
		avatar = ''
	end

	local data = GetHeroPreviewDataFromDB(hero, avatar, 'large')
	if data ~= nil then
		model = data.modelPath
		effect = data.effectPath
	end

	if SpecialAvatars[item.modelName] ~= nil then
		model = SpecialAvatars[item.modelName].model or model
		effect = SpecialAvatars[item.modelName].effect or effect
		enableEffect = SpecialAvatars[item.modelName].enableEffect or enableEffect
	end

	GetWidget('codex_editor_model'):SetText('^gModelPath: ^w'..model)
	GetWidget('codex_editor_effect'):SetText('^gEffectPath: ^w'..effect)
	if enableEffect then 
		GetWidget('codex_editor_effectEnabled'):SetButtonState(2)
	else
		GetWidget('codex_editor_effectEnabled'):SetButtonState(0)
	end

	GetWidget('codex_editor'):SetVisible(true)
end

function HoN_Codex:Editor_ChangeModelOrEffect(ext, path)
	if not GetWidget('codex_editor'):IsVisible() then return end

	Echo('^gEditor_ChangeModelOrEffect() path='..tostring(path)..', ext='..tostring(ext))

	SpecialAvatars[Editor_Avatar] = SpecialAvatars[Editor_Avatar] or {}
	if ext == 'mdf' then
		SpecialAvatars[Editor_Avatar].model = path
	elseif ext == 'effect' then
		SpecialAvatars[Editor_Avatar].effect = path
	end

	HoN_Codex:Editor_UpdateSpecialAvatar(Editor_Avatar)
	HoN_Codex:UpdateOneItem(Editor_Current)
	HoN_Codex:OpenEditor(Editor_Current)
end

function HoN_Codex:Editor_ChangeEffectEnabled()
	local state = GetWidget('codex_editor_effectEnabled'):GetButtonState()

	SpecialAvatars[Editor_Avatar] = SpecialAvatars[Editor_Avatar] or {}

	if state == 0 then
		SpecialAvatars[Editor_Avatar].enableEffect = false
	else
		SpecialAvatars[Editor_Avatar].enableEffect = true
	end
	HoN_Codex:Editor_UpdateSpecialAvatar(Editor_Avatar)
	HoN_Codex:UpdateOneItem(Editor_Current)
end

function HoN_Codex:Editor_UpdateSpecialAvatar(heroAvatar)
	if SpecialAvatars[heroAvatar] == nil then return end

	local originalModel = ''
	local originalEffect = ''
	local hero, avatar = ParseHeroAvatar(heroAvatar)
	if hero == nil then
		hero = heroAvatar
		avatar = ''
	end

	local newModel = SpecialAvatars[heroAvatar].model
	local newEffect = SpecialAvatars[heroAvatar].effect
	local data = GetHeroPreviewDataFromDB(hero, avatar, 'large')
	if data ~= nil then
		originalModel = data.modelPath
		newModel = newModel or data.modelPath
		originalEffect = data.effectPath
		newEffect = newEffect or data.effectPath
	end

	if originalModel == newModel and 
		originalEffect == newEffect and 
		not SpecialAvatars[heroAvatar].enableEffect then

		SpecialAvatars[heroAvatar] = nil
	end
end

function HoN_Codex:Editor_Save()
	local savepath = GetWidget('codex_editor_savepath'):GetText()
	local file = io.open(savepath, 'w')
	local data = ''
	for k,v in pairs(SpecialAvatars) do
		local item = 'SpecialAvatars[\''..k..'\'] = {'

		if NotEmpty(v.model) then
			item = item..'model=\''..v.model..'\','
		end

		if NotEmpty(v.effect) then
			item = item..'effect=\''..v.effect..'\','
		end

		if v.enableEffect then
			item = item..'enableEffect=true,'
		end

		if v.scale then
			item = item..'scale='..v.scale
		end


		item = item..'} \n'
		data = data..item
	end
	file:write(data)
	file:close()
end

SpecialAvatars['Hero_Solstice.Alt8'] = {model='/heroes/solstice/alt8/preview.mdf',} 
SpecialAvatars['Hero_MasterOfArms.Alt7'] = {enableEffect=true,} 
SpecialAvatars['Hero_Javaras.Classic'] = {enableEffect=true,} 
SpecialAvatars['Hero_Solstice.Alt3'] = {model='/heroes/solstice/alt3/preview.mdf',} 
SpecialAvatars['Hero_Grinex.Alt6'] = {enableEffect=true,} 
SpecialAvatars['Hero_Salomon.Alt7'] = {model='/heroes/salomon/alt7/preview.mdf',} 
SpecialAvatars['Hero_Lodestone.Alt'] = {enableEffect=true,} 
SpecialAvatars['Hero_Legionnaire.Alt10'] = {model='/heroes/legionnaire/alt10/legion/preview.mdf',} 
SpecialAvatars['Hero_Tarot.Alt6'] = {enableEffect=true,} 
SpecialAvatars['Hero_Yogi.Alt2'] = {model='/heroes/yogi/alt2/preview.mdf',} 
SpecialAvatars['Hero_Zephyr.Alt3'] = {enableEffect=true,} 
SpecialAvatars['Hero_Solstice.Alt10'] = {model='/heroes/solstice/alt10/preview.mdf',} 
SpecialAvatars['Hero_Predator.Alt5'] = {enableEffect=true,} 
SpecialAvatars['Hero_Riptide'] = {model='/heroes/riptide/preview.mdf',} 
SpecialAvatars['Hero_ShadowBlade.Alt5'] = {model='/heroes/shadowblade/alt5/preview.mdf',} 
SpecialAvatars['Hero_Midas.Alt'] = {enableEffect=true,} 
SpecialAvatars['Purchase'] = {model='/ui/fe2/codex/models/gold/model.mdf',scale=0.5} 
SpecialAvatars['Hero_Kane.Alt9'] = {enableEffect=true,} 
SpecialAvatars['Hero_Behemoth.Alt7'] = {enableEffect=true,} 
SpecialAvatars['Hero_Cthulhuphant.Alt8'] = {scale=0.5} 
SpecialAvatars['Share'] = {model='/ui/fe2/codex/models/silver/model.mdf',scale=0.5} 
SpecialAvatars['Hero_Revenant.Alt3'] = {enableEffect=true,} 
SpecialAvatars['Hero_Kunas.Alt3'] = {enableEffect=true,} 
SpecialAvatars['Hero_Parallax.Alt'] = {enableEffect=true,} 
SpecialAvatars['Hero_Rhapsody.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_DoctorRepulsor.Alt4'] = {enableEffect=true,} 
SpecialAvatars['Hero_Nitro'] = {enableEffect=true,} 
SpecialAvatars['Hero_Ravenor.Alt7'] = {enableEffect=true,} 
SpecialAvatars['Hero_Tarot.Alt'] = {enableEffect=true,} 
SpecialAvatars['Hero_Hellbringer.Alt4'] = {model='/heroes/hellbringer/alt4/preview.mdf',} 
SpecialAvatars['Hero_Berzerker.Alt12'] = {model='/heroes/berzerker/alt12/preview.mdf',} 
SpecialAvatars['Hero_PollywogPriest.Alt6'] = {model='/heroes/pollywogpriest/alt6/preview.mdf',} 
SpecialAvatars['Hero_Bubbles.trophy_skin'] = {model='/heroes/bubbles/preview.mdf',effect='/heroes/bubbles/trophy_skin/effects/body.effect',enableEffect=true,} 
SpecialAvatars['Hero_Tempest.main_reskin'] = {enableEffect=true,} 
SpecialAvatars['Hero_Salomon.Alt4'] = {model='/heroes/salomon/alt4/preview.mdf',} 
SpecialAvatars['Hero_Geomancer.Alt6'] = {model='/heroes/geomancer/alt6/preview.mdf',scale=0.35} 
SpecialAvatars['Hero_Riftmage.Alt4'] = {enableEffect=true,} 
SpecialAvatars['Hero_Warchief.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Yogi.Soccer_skin'] = {model='/heroes/yogi/soccer_skin/preview.mdf',} 
SpecialAvatars['Hero_Xalynx.Classic'] = {enableEffect=true,} 
SpecialAvatars['Hero_DrunkenMaster.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Hellbringer.Alt6'] = {model='/heroes/hellbringer/alt6/preview.mdf',} 
SpecialAvatars['Hero_Tarot.Alt8'] = {enableEffect=true,} 
SpecialAvatars['Hero_MasterOfArms.trophy_skin'] = {effect='/heroes/master_of_arms/trophy_skin/effects/body_preview.effect',enableEffect=true,} 
SpecialAvatars['Hero_Berzerker.Alt6'] = {enableEffect=true,} 
SpecialAvatars['Hero_Pearl.Alt8'] = {enableEffect=true,} 
SpecialAvatars['Hero_Parallax.set_ascension'] = {model='/heroes/parallax/set_ascension/preview.mdf',effect='/heroes/parallax/set_ascension/effects/helix_passive.effect',enableEffect=true,} 
SpecialAvatars['Hero_Bubbles.trophy_skin02'] = {model='/heroes/bubbles/preview.mdf',effect='/heroes/bubbles/trophy_skin/effects/body.effect',enableEffect=true,} 
SpecialAvatars['Hero_Moraxus.Alt3'] = {enableEffect=true,} 
SpecialAvatars['Hero_Salomon'] = {model='/heroes/salomon/preview.mdf',} 
SpecialAvatars['Hero_Hammerstorm.Alt5'] = {enableEffect=true,} 
SpecialAvatars['Hero_ShadowBlade.Alt3'] = {model='/heroes/shadowblade/alt3/preview.mdf',} 
SpecialAvatars['Hero_BabaYaga.Alt8'] = {enableEffect=true,} 
SpecialAvatars['Hero_Solstice.Alt9'] = {model='/heroes/solstice/alt9/preview.mdf',} 
SpecialAvatars['Hero_Tarot.trophy_skin'] = {enableEffect=true,} 
SpecialAvatars['Hero_Dampeer.Alt7'] = {model='/heroes/dampeer/alt7/preview.mdf',} 
SpecialAvatars['Hero_Artesia.Alt'] = {enableEffect=true,} 
SpecialAvatars['Hero_Lodestone.Alt6'] = {model='/heroes/lodestone/alt6/preview.mdf',} 
SpecialAvatars['Hero_ShadowBlade.Alt2'] = {model='/heroes/shadowblade/alt2/preview.mdf',} 
SpecialAvatars['Hero_FlameDragon.Alt9'] = {enableEffect=true,} 
SpecialAvatars['Hero_Devourer.Alt13'] = {scale=0.55} 
SpecialAvatars['Hero_Shellshock.Alt2'] = {model='/heroes/shellshock/alt2/preview.mdf',} 
SpecialAvatars['Hero_Gemini.Alt6'] = {enableEffect=true,} 
SpecialAvatars['Hero_ForsakenArcher.main_reskin'] = {enableEffect=true,} 
SpecialAvatars['Hero_Hellbringer.Alt3'] = {model='/heroes/hellbringer/alt3/model.mdf',} 
SpecialAvatars['Hero_Pyromancer.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Skrap.Alt2'] = {model='/heroes/skrap/alt2/preview.mdf',} 
SpecialAvatars['Hero_Yogi.Alt5'] = {model='/heroes/yogi/alt5/preview.mdf',} 
SpecialAvatars['Hero_Hellbringer'] = {model='/heroes/hellbringer/preview.mdf',} 
SpecialAvatars['Hero_Hunter.Alt5'] = {enableEffect=true,} 
SpecialAvatars['Hero_Riptide.Alt3'] = {model='/heroes/riptide/alt3/preview.mdf',effect='/heroes/riptide/alt3/effects/body.effect',enableEffect=true,} 
SpecialAvatars['Hero_Soulstealer.Classic'] = {enableEffect=true,} 
SpecialAvatars['Hero_Kane.Alt'] = {enableEffect=true,} 
SpecialAvatars['Hero_FlameDragon.Alt10'] = {enableEffect=true,} 
SpecialAvatars['Hero_DwarfMagi.Alt4'] = {enableEffect=true,} 
SpecialAvatars['Hero_BabaYaga.Alt4'] = {enableEffect=true,} 
SpecialAvatars['Hero_Ebulus.pog_skin'] = {enableEffect=true,} 
SpecialAvatars['Hero_Pyromancer.Alt3'] = {enableEffect=true,} 
SpecialAvatars['Hero_Hellbringer.Alt'] = {model='/heroes/hellbringer/alt/preview.mdf',effect='/heroes/hellbringer/alt/effects/body.effect',enableEffect=true,} 
SpecialAvatars['Hero_Behemoth.Alt13'] = {scale=0.55} 
SpecialAvatars['Hero_Rampage.Alt8'] = {enableEffect=true,} 
SpecialAvatars['Hero_Riptide.Alt6'] = {model='/heroes/riptide/alt6/model_store_m.mdf',} 
SpecialAvatars['Hero_Nitro.Alt3'] = {enableEffect=true,} 
SpecialAvatars['Hero_Lodestone.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Hydromancer.Alt9'] = {model='/heroes/hydromancer/alt9/preview.mdf',} 
SpecialAvatars['Hero_Riptide.Alt4'] = {model='/heroes/riptide/alt4/graffiti.mdf',effect='/heroes/riptide/alt4/effects/body.effect',} 
SpecialAvatars['Hero_Midas.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_PuppetMaster.Alt9'] = {enableEffect=true,} 
SpecialAvatars['Hero_FlintBeastwood.Alt9'] = {enableEffect=true,scale=0.6} 
SpecialAvatars['Hero_Deadlift.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_ShadowBlade.Alt4'] = {model='/heroes/shadowblade/alt4/preview.mdf',} 
SpecialAvatars['Hero_Hellbringer.Alt5'] = {model='/heroes/hellbringer/alt5/preview.mdf',} 
SpecialAvatars['Hero_Klanx.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Maliken.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Empath.Alt4'] = {model='/heroes/empath/alt4/preview.mdf',effect='/heroes/empath/alt4/grass/body.effect',enableEffect=true,} 
SpecialAvatars['Hero_Oogie.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Skrap'] = {model='/heroes/skrap/preview.mdf',scale=0.6} 
SpecialAvatars['Hero_Flux.Alt5'] = {enableEffect=true,} 
SpecialAvatars['Hero_DoctorRepulsor.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Taint.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Tempest.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Hantumon.Alt4'] = {enableEffect=true,} 
SpecialAvatars['Hero_Dampeer.Alt10'] = {enableEffect=true,} 
SpecialAvatars['Hero_Salomon.Alt'] = {model='/heroes/salomon/alt/preview.mdf',} 
SpecialAvatars['Hero_Pyromancer.Alt4'] = {enableEffect=true,} 
SpecialAvatars['Hero_Prophet.Alt9'] = {model='/heroes/prophet/alt9/preview.mdf',} 
SpecialAvatars['Hero_Gauntlet.pog_skin'] = {enableEffect=true,} 
SpecialAvatars['Hero_Shellshock'] = {model='/heroes/shellshock/preview.mdf',} 
SpecialAvatars['Hero_Nitro.Alt'] = {enableEffect=true,} 
SpecialAvatars['Hero_Andromeda.Alt'] = {enableEffect=true,} 
SpecialAvatars['Hero_Bushwack.Alt'] = {enableEffect=true,} 
SpecialAvatars['Hero_MasterOfArms.Alt5'] = {enableEffect=true,} 
SpecialAvatars['Hero_Parasite.Alt5'] = {effect='/heroes/parasite/alt5/effects/body.effect',enableEffect=true,} 
SpecialAvatars['Hero_Midas.Alt6'] = {model='/heroes/midas/alt6/lvl_3/preview.mdf',effect='/heroes/midas/alt6/effects/body_lvl_4.effect',enableEffect=true,} 
SpecialAvatars['Hero_Salomon.Alt2'] = {model='/heroes/salomon/alt2/preview.mdf',} 
SpecialAvatars['Hero_Gauntlet.Alt6'] = {enableEffect=true,} 
SpecialAvatars['Hero_DiseasedRider.Alt'] = {enableEffect=true,} 
SpecialAvatars['Hero_PuppetMaster.Alt12'] = {enableEffect=true,} 
SpecialAvatars['Hero_Tarot.Alt4'] = {enableEffect=true,} 
SpecialAvatars['Hero_Solstice'] = {model='/heroes/solstice/preview.mdf',} 
SpecialAvatars['Hero_ForsakenArcher.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Gauntlet.Alt'] = {enableEffect=true,} 
SpecialAvatars['Hero_Midas.Alt4'] = {enableEffect=true,} 
SpecialAvatars['Hero_Fade.Alt'] = {enableEffect=true,} 
SpecialAvatars['Hero_MasterOfArms.Alt4'] = {enableEffect=true,} 
SpecialAvatars['Hero_Ophelia.Alt3'] = {enableEffect=true,} 
SpecialAvatars['Hero_Prisoner.Alt5'] = {enableEffect=true,} 
SpecialAvatars['Hero_Ra.Alt3'] = {enableEffect=true,} 
SpecialAvatars['Hero_ShadowBlade.Alt'] = {model='/heroes/shadowblade/alt/preview.mdf',} 
SpecialAvatars['Hero_Hydromancer.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Fade.Alt7'] = {enableEffect=true,} 
SpecialAvatars['Hero_Pestilence.Alt3'] = {enableEffect=true,} 
SpecialAvatars['Hero_Klanx.Alt7'] = {model='/heroes/klanx/alt7/preview.mdf',} 
SpecialAvatars['Hero_Hellbringer.Alt2'] = {model='/heroes/hellbringer/alt2/preview.mdf',} 
SpecialAvatars['Hero_Jereziah.Alt9'] = {model='/heroes/jereziah/alt9/preview.mdf',} 
SpecialAvatars['Hero_Shellshock.Alt'] = {scale=0.55} 
SpecialAvatars['Hero_Valkyrie.Alt9'] = {enableEffect=true,} 
SpecialAvatars['Hero_Tempest.Classic'] = {enableEffect=true,} 
SpecialAvatars['Hero_ShadowBlade.Alt7'] = {model='/heroes/shadowblade/alt7/model_store.mdf',} 
SpecialAvatars['Hero_Nitro.Alt2'] = {model='/heroes/nitro/alt2/base/preview.mdf',enableEffect=true,} 
SpecialAvatars['Hero_Pearl.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Andromeda.Classic'] = {enableEffect=true,} 
SpecialAvatars['Hero_Berzerker.Alt4'] = {enableEffect=true,} 
SpecialAvatars['Hero_Salomon.Alt3'] = {model='/heroes/salomon/alt3/preview.mdf',} 
SpecialAvatars['Hero_Riptide.Alt7'] = {model='/heroes/riptide/alt7/preview.mdf',} 
SpecialAvatars['Hero_BabaYaga.Alt3'] = {enableEffect=true,} 
SpecialAvatars['Hero_Yogi'] = {model='/heroes/yogi/preview.mdf',} 
SpecialAvatars['Hero_Blitz.Alt6'] = {enableEffect=true,} 
SpecialAvatars['Hero_Rhapsody.Alt7'] = {enableEffect=true,} 
SpecialAvatars['Hero_Parallax.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Hiro.Alt8'] = {enableEffect=true,} 
SpecialAvatars['Hero_DwarfMagi.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Bubbles.trophy_skin03'] = {model='/heroes/bubbles/preview.mdf',effect='/heroes/bubbles/trophy_skin/effects/body.effect',enableEffect=true,} 
SpecialAvatars['Hero_Calamity.Alt3'] = {enableEffect=true,} 
SpecialAvatars['Hero_Skrap.Alt3'] = {model='/heroes/skrap/alt3/preview.mdf',effect='/heroes/skrap/alt3/effects/body_boomerang.effect',enableEffect=true,} 
SpecialAvatars['Hero_Tarot.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Flux.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Solstice.trophy_skin'] = {model='/heroes/solstice/trophy_skin/preview.mdf',} 
SpecialAvatars['Hero_Parallax.Alt4'] = {enableEffect=true,} 
SpecialAvatars['Hero_EmeraldWarden.Alt5'] = {enableEffect=true,} 
SpecialAvatars['Hero_Lodestone.Alt3'] = {enableEffect=true,} 
SpecialAvatars['Hero_Devourer.Alt14'] = {scale=0.55} 
SpecialAvatars['Hero_Gauntlet.Alt2'] = {model='/heroes/gauntlet/alt2/model_store_m.mdf',} 
SpecialAvatars['Hero_Blitz.Alt3'] = {enableEffect=true,} 
SpecialAvatars['Hero_Salomon.Alt5'] = {model='/heroes/salomon/model.mdf',} 
SpecialAvatars['Hero_Rampage.Alt9'] = {enableEffect=true,} 
SpecialAvatars['Hero_Chronos.Alt6'] = {enableEffect=true,} 
SpecialAvatars['Hero_Kenisis.Alt5'] = {enableEffect=true,} 
SpecialAvatars['Hero_Gemini.Alt5'] = {model='/heroes/gemini/alt5/preview.mdf',} 
SpecialAvatars['Hero_SandWraith.Alt4'] = {enableEffect=true,} 
SpecialAvatars['Hero_Andromeda.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Accursed.Alt8'] = {model='/heroes/accursed/alt8/preview.mdf',enableEffect=true,} 
SpecialAvatars['Hero_Riptide.Alt2'] = {model='/heroes/riptide/alt2/preview.mdf',effect='/heroes/riptide/alt2/effects/body.effect',} 
SpecialAvatars['Hero_Salomon.Alt6'] = {model='/heroes/salomon/alt6/preview.mdf',} 
SpecialAvatars['Hero_Dampeer.Alt8'] = {model='/heroes/dampeer/alt8/preview.mdf',} 
SpecialAvatars['Hero_Dampeer.Alt2'] = {model='/heroes/dampeer/alt2/graffiti.mdf',scale=0.4} 
SpecialAvatars['Hero_Scar.Alt8'] = {enableEffect=true,} 
SpecialAvatars['Hero_SirBenzington.Alt'] = {model='/heroes/sir_benzington/alt/model_store_m.mdf',} 
SpecialAvatars['Hero_Yogi.Classic'] = {model='/heroes/yogi/preview.mdf',} 
SpecialAvatars['Hero_Riptide.Alt5'] = {model='/heroes/riptide/alt4/graffiti.mdf',} 
SpecialAvatars['Hero_Salomon.set_ascension'] = {model='/heroes/salomon/set_ascension/preview.mdf',} 
SpecialAvatars['Hero_CorruptedDisciple.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Fairy.main_reskin'] = {effect='/heroes/fairy/reskin/effects/body_store_m.effect',enableEffect=true,} 
SpecialAvatars['Hero_Grinex.Alt7'] = {enableEffect=true,} 
SpecialAvatars['Hero_Magmar.Alt6'] = {enableEffect=true,} 
SpecialAvatars['Hero_Magmar.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Aluna.Alt'] = {enableEffect=true,} 
SpecialAvatars['Hero_Yogi.Alt4'] = {model='/heroes/yogi/alt4/preview.mdf',} 
SpecialAvatars['Hero_Skrap.Alt'] = {model='/heroes/skrap/alt/preview.mdf',} 
SpecialAvatars['Hero_PuppetMaster.Alt11'] = {model='/heroes/puppetmaster/alt11/preview.mdf',} 
SpecialAvatars['Hero_Klanx.Alt'] = {enableEffect=true,} 
SpecialAvatars['Hero_Solstice.Alt4'] = {model='/heroes/solstice/alt4/preview.mdf',} 
SpecialAvatars['Hero_Hellbringer.Alt7'] = {model='/heroes/hellbringer/alt7/preview.mdf',} 
SpecialAvatars['Hero_Scar.Alt5'] = {enableEffect=true,} 
SpecialAvatars['Hero_Revenant.Alt4'] = {enableEffect=true,} 
SpecialAvatars['Hero_Prophet.Alt4'] = {enableEffect=true,scale=0.6} 
SpecialAvatars['Hero_Mumra.Alt3'] = {enableEffect=true,} 
SpecialAvatars['Hero_Bushwack.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Tarot.Alt3'] = {enableEffect=true,} 
SpecialAvatars['Hero_Deadwood.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Devourer.Alt6'] = {enableEffect=true,scale=0.65} 
SpecialAvatars['Hero_Bushwack.Alt3'] = {enableEffect=true,} 
SpecialAvatars['Hero_Riptide.Alt'] = {model='/heroes/riptide/alt/preview.mdf',effect='/heroes/riptide/alt/effects/body.effect',enableEffect=true,} 
SpecialAvatars['Hero_Yogi.Alt3'] = {model='/heroes/yogi/alt3/preview.mdf',effect='/heroes/yogi/alt3/effects/body_graffiti.effect',enableEffect=true,} 
SpecialAvatars['Hero_Legionnaire.Alt5'] = {enableEffect=true,} 
SpecialAvatars['Hero_Devourer.Alt9'] = {enableEffect=true,} 
SpecialAvatars['Hero_Xalynx.Alt3'] = {enableEffect=true,} 
SpecialAvatars['Hero_FlintBeastwood.Alt4'] = {enableEffect=true,} 
SpecialAvatars['Hero_Bubbles.Alt5'] = {enableEffect=true,} 
SpecialAvatars['Hero_Ellonia.Alt'] = {enableEffect=true,} 
SpecialAvatars['Hero_Arachna.Alt7'] = {model='/heroes/arachna/alt7/preview.mdf',enableEffect=true,} 
SpecialAvatars['Hero_Deadlift.Alt'] = {enableEffect=true,} 
SpecialAvatars['Hero_Prophet.Alt8'] = {model='/heroes/prophet/alt8/preview.mdf',} 
SpecialAvatars['Hero_Yogi.Alt'] = {model='/heroes/yogi/alt/preview.mdf',} 
SpecialAvatars['Hero_Frosty.Alt2'] = {enableEffect=true,} 
SpecialAvatars['Hero_Yogi.Alt6'] = {model='/heroes/yogi/alt6/preview.mdf',} 
SpecialAvatars['Hero_ShadowBlade.Alt6'] = {model='/heroes/shadowblade/preview.mdf',} 
SpecialAvatars['Hero_ShadowBlade'] = {model='/heroes/shadowblade/preview.mdf',} 
SpecialAvatars['Hero_Hammerstorm.Alt10'] = {enableEffect=true,} 
SpecialAvatars['Hero_Bubbles.Alt'] = {enableEffect=true,} 
SpecialAvatars['Hero_SandWraith.Alt10'] = {model='/heroes/sand_wraith/alt10/1/preview.mdf',} 
SpecialAvatars['Hero_Geomancer.Alt3'] = {enableEffect=true,} 
SpecialAvatars['Hero_Frosty.Alt9'] = {model='/heroes/frosty/alt9/preview.mdf',} 
SpecialAvatars['Hero_Yogi.pog_skin'] = {enableEffect=true,} 
SpecialAvatars['Hero_Hammerstorm.Alt12'] = {model='/heroes/hammerstorm/alt12/preview.mdf',} 
SpecialAvatars['Hero_Vindicator.Alt11'] = {model='/heroes/vindicator/alt11/preview.mdf',} 
SpecialAvatars['Hero_Tarot.Alt9'] = {enableEffect=true,} 
SpecialAvatars['Hero_Legionnaire.trophy_skin'] = {enableEffect=true,} 
SpecialAvatars['Hero_Silhouette.Alt12'] = {model='/heroes/silhouette/alt12/preview.mdf',} 
SpecialAvatars['Hero_Cthulhuphant.trophy_skin'] = {scale=0.45} 
SpecialAvatars['Hero_Cthulhuphant.trophy_skin02'] = {scale=0.45} 
SpecialAvatars['Hero_Cthulhuphant.trophy_skin03'] = {scale=0.45} 
SpecialAvatars['Hero_Cthulhuphant.Alt9'] = {scale=0.4} 
SpecialAvatars['Hero_Andromeda.Alt6'] = {enableEffect=true,}
SpecialAvatars['Hero_Chipper.trophy_skin01'] = {scale=0.45} 
SpecialAvatars['Hero_Chipper.trophy_skin02'] = {scale=0.45} 
SpecialAvatars['Hero_Chipper.trophy_skin03'] = {scale=0.45} 


table.insert (DebugAvatars,'Hero_Cthulhuphant.trophy_skin')				--模型过大，适当调小
table.insert (DebugAvatars,'Hero_Cthulhuphant.trophy_skin02')			--模型过大，适当调小
table.insert (DebugAvatars,'Hero_Cthulhuphant.trophy_skin03')			--模型过大，适当调小
table.insert (DebugAvatars,'Hero_Cthulhuphant.Alt9')					--模型过大，适当调小
table.insert (DebugAvatars,'Hero_Andromeda.Alt6')						--要加上特效
table.insert (DebugAvatars,'Hero_Bubbles.Alt15')						--特效还是去掉
table.insert (DebugAvatars,'Hero_Chipper.trophy_skin01')				--模型过大，适当调小
table.insert (DebugAvatars,'Hero_Chipper.trophy_skin02')				--模型过大，适当调小
table.insert (DebugAvatars,'Hero_Chipper.trophy_skin03')				--模型过大，适当调小
table.insert (DebugAvatars,'Hero_Accursed.Alt8')						--看特效是否可以拆分，只留头部特效，去掉入场特效