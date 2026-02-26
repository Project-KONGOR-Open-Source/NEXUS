
local _G = getfenv(0)
Player_Stats = {}
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface = object
RegisterScript2('Player_Stats', '30')

--[[
function Player_Stats.OnShow()
	GetWidget('player_stats_mystats_parent'):SetVisible(true)
	-- GetWidget('player_stats_ladder_parent'):SetVisible(false)
	-- GetWidget('player_stats_quests_parent'):SetVisible(false)
end
--]]

function Player_Stats.ClickedMyStats(toggle)
	if (toggle) or ( (not GetWidget('player_stats'):IsVisible()))  then
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'player_stats', nil, false)
	end
	-- GetWidget('player_stats_mystats_parent'):SetVisible(true)
	-- GetWidget('player_stats_ladder_parent'):SetVisible(false)
	-- Set('player_stats_current_panel', '0', 'int')
end

function Player_Stats.ClickedLadder(toggle)
	if (toggle) or ( (not GetWidget('player_ladder'):IsVisible()))  then
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'player_ladder', nil, false)
	end

	-- GetWidget('player_stats_mystats_parent'):SetVisible(false)
	-- Set('player_stats_current_panel', '1', 'int')

	GetWidget('player_stats_ladder_tab'):SetButtonState(1)
	if GetCvarBool('cl_GarenaEnable') then
		GetWidget('player_stats_ladder_tab2'):SetButtonState(0)
	end
	GetWidget('player_stats_quests_tab'):SetButtonState(0)
	GetWidget('player_stats_codex_level_tab'):SetButtonState(0)
	-- GetWidget('player_stats_striker_tab'):SetButtonState(0)
	GetWidget('player_stats_mastery_tab'):SetButtonState(0)
	GetWidget('player_stats_ladder_parent'):SetVisible(true)
	GetWidget('player_stats_quests_parent'):SetVisible(false)
	GetWidget('player_stats_codex_level_parent'):SetVisible(false)
	-- GetWidget('player_stats_striker_parent'):SetVisible(false)
	GetWidget('player_stats_mastery_parent'):SetVisible(false)

	GetWidget('web_browser_ladder_insert'):DoEvent()
end

function Player_Stats.ClickedLadder2(toggle)
	if (toggle) or ( (not GetWidget('player_ladder'):IsVisible()))  then
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'player_ladder', nil, false)
	end

	-- GetWidget('player_stats_mystats_parent'):SetVisible(false)
	-- Set('player_stats_current_panel', '1', 'int')

	GetWidget('player_stats_ladder_tab'):SetButtonState(0)
	if GetCvarBool('cl_GarenaEnable') then
		GetWidget('player_stats_ladder_tab2'):SetButtonState(0)
	end
	GetWidget('player_stats_quests_tab'):SetButtonState(0)
	GetWidget('player_stats_codex_level_tab'):SetButtonState(0)
	-- GetWidget('player_stats_striker_tab'):SetButtonState(0)
	GetWidget('player_stats_mastery_tab'):SetButtonState(0)
	GetWidget('player_stats_ladder_parent'):SetVisible(true)
	GetWidget('player_stats_quests_parent'):SetVisible(false)
	GetWidget('player_stats_codex_level_parent'):SetVisible(false)
	-- GetWidget('player_stats_striker_parent'):SetVisible(false)
	GetWidget('player_stats_mastery_parent'):SetVisible(false)

	GetWidget('web_browser_ladder2_insert'):DoEvent()
end

function Player_Stats.ClickedQuests(toggle)
	if (toggle) and ( (not GetWidget('player_ladder'):IsVisible()))  then
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'player_ladder', nil, false)
	end

	GetWidget('player_stats_ladder_tab'):SetButtonState(0)
	if GetCvarBool('cl_GarenaEnable') then
		GetWidget('player_stats_ladder_tab2'):SetButtonState(0)
	end
	GetWidget('player_stats_quests_tab'):SetButtonState(1)
	GetWidget('player_stats_codex_level_tab'):SetButtonState(0)
	-- GetWidget('player_stats_striker_tab'):SetButtonState(0)
	GetWidget('player_stats_mastery_tab'):SetButtonState(0)
	GetWidget('player_stats_ladder_parent'):SetVisible(false)
	GetWidget('player_stats_quests_parent'):SetVisible(true)
	GetWidget('player_stats_codex_level_parent'):SetVisible(false)
	-- GetWidget('player_stats_striker_parent'):SetVisible(false)
	GetWidget('player_stats_mastery_parent'):SetVisible(false)

	GetWidget('web_browser_questladder_insert'):DoEvent()
end

function Player_Stats.ClickedCodexLevel(toggle)
	if (toggle) and ( (not GetWidget('player_ladder'):IsVisible()))  then
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'player_ladder', nil, false)
	end

	GetWidget('player_stats_ladder_tab'):SetButtonState(0)
	if GetCvarBool('cl_GarenaEnable') then
		GetWidget('player_stats_ladder_tab2'):SetButtonState(0)
	end
	GetWidget('player_stats_quests_tab'):SetButtonState(0)
	GetWidget('player_stats_codex_level_tab'):SetButtonState(1)
	-- GetWidget('player_stats_striker_tab'):SetButtonState(0)
	GetWidget('player_stats_mastery_tab'):SetButtonState(0)
	GetWidget('player_stats_ladder_parent'):SetVisible(false)
	GetWidget('player_stats_quests_parent'):SetVisible(false)
	GetWidget('player_stats_codex_level_parent'):SetVisible(true)
	-- GetWidget('player_stats_striker_parent'):SetVisible(false)
	GetWidget('player_stats_mastery_parent'):SetVisible(false)

	GetWidget('web_browser_codexlevelladder_insert'):DoEvent()
end

--[[
function Player_Stats.ClickedStriker(toggle)
	if (toggle) and ( (not GetWidget('player_ladder'):IsVisible()))  then
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'player_ladder', nil, false)
	end

	GetWidget('player_stats_ladder_tab'):SetButtonState(0)
	if GetCvarBool('cl_GarenaEnable') then
		GetWidget('player_stats_ladder_tab2'):SetButtonState(0)
	end
	GetWidget('player_stats_quests_tab'):SetButtonState(0)
	GetWidget('player_stats_codex_level_tab'):SetButtonState(0)
	GetWidget('player_stats_striker_tab'):SetButtonState(1)
	GetWidget('player_stats_mastery_tab'):SetButtonState(0)
	GetWidget('player_stats_ladder_parent'):SetVisible(false)
	GetWidget('player_stats_quests_parent'):SetVisible(false)
	GetWidget('player_stats_codex_level_parent'):SetVisible(false)
	GetWidget('player_stats_striker_parent'):SetVisible(true)
	GetWidget('player_stats_mastery_parent'):SetVisible(false)

	GetWidget('web_browser_strikerladder_insert'):DoEvent()
end
]]

function Player_Stats.ClickedMastery(toggle)
	if (toggle) and ( (not GetWidget('player_ladder'):IsVisible()))  then
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'player_ladder', nil, false)
	end

	GetWidget('player_stats_ladder_tab'):SetButtonState(0)
	if GetCvarBool('cl_GarenaEnable') then
		GetWidget('player_stats_ladder_tab2'):SetButtonState(0)
	end
	GetWidget('player_stats_quests_tab'):SetButtonState(0)
	GetWidget('player_stats_codex_level_tab'):SetButtonState(0)
	-- GetWidget('player_stats_striker_tab'):SetButtonState(0)
	GetWidget('player_stats_mastery_tab'):SetButtonState(1)
	GetWidget('player_stats_ladder_parent'):SetVisible(false)
	GetWidget('player_stats_quests_parent'):SetVisible(false)
	GetWidget('player_stats_codex_level_parent'):SetVisible(false)
	-- GetWidget('player_stats_striker_parent'):SetVisible(false)
	GetWidget('player_stats_mastery_parent'):SetVisible(true)

	GetWidget('web_browser_masteryladder_insert'):DoEvent()
end

local function UIUpdateRegion() --TODO

	local regionCode, showLadder = HoN_Region:GetLadderRegionInfo()

	if (regionCode) then
		if (showLadder) then
			GetWidget('player_stats_tabs'):SetVisible(1)
			GetWidget('player_stats_mystats_parent'):SetWidth('-4.3h')
		else
			GetWidget('player_stats_tabs'):SetVisible(0)
			GetWidget('player_stats_mystats_parent'):SetWidth('100%')
		end
	end
end
-- interface:RegisterWatch("UIUpdateRegion", UIUpdateRegion)

local rewardsinfo = {}
local statsnickname = ''
local totalLevel = 0
local pageMax = 0
local pageStep = 0
local masteryRewardScrollMax = 0

function Player_Stats.ClickReceiveMasteryTotalLevelReward(index)
	local reward = rewardsinfo[index]
	if reward then
	    Cmd('ReceiveMasteryReward '..reward.level)
    end
end

function Player_Stats.WheelMasteryReward(up)
	local scroll = GetWidget('stats_mastery_reward_scroll')
	local value = (up and (-pageStep)) or pageStep
	if (value == 0) then return end
	scroll:SetValue(scroll:GetValue() + value)
end

function Player_Stats.UpdateMasteryReward()
	local curValue = GetWidget('stats_mastery_reward_scroll'):GetValue()
	local rate = math.min(1, curValue / masteryRewardScrollMax)
	local pos = pageMax * rate
	GetWidget('player_stats_mastery_rewards_root'):SetY('-'..tostring(pos)..'h')
end


function Player_Stats.ResetMasteryInfo()
	for i=1,15 do
		local widget = GetWidget('stats_mastery_hero_proficiency_single_heronumber_'..i)
		if widget then widget:SetText('0') end
	end

	for i=1,5 do
		GetWidget('stats_mastery_levelup_single_'..i):SetVisible(false)
	end

	for i=1,40 do
		GetWidget('stats_mastery_reward_single_'..i):SetVisible(false)
	end
end

function SetMasteryReward()
	Trigger('HasValidMasteryReward', 'false')
	for i,v in ipairs(rewardsinfo) do
		local widget = GetWidget('stats_mastery_reward_single_'..i)
		if not widget then break end

		local reward = rewardsinfo[i]
		local lastReward = rewardsinfo[i-1]

		if (reward) then
		 	GetWidget('stats_mastery_reward_single_'..i..'_text'):SetText(reward.text)
			local iconWidget = GetWidget('stats_mastery_reward_single_'..i..'_icon')
		 	iconWidget:SetTexture(reward.icon)
		 	GetWidget('stats_mastery_reward_single_'..i..'_btntext'):SetText('LV '..tostring(reward.level))
		 	GetWidget('stats_mastery_reward_single_'..i..'_hidebtn_text'):SetText('LV '..tostring(reward.level))
		 	GetWidget('stats_mastery_reward_single_'..i..'_effect'):SetVisible(false)

		 	local isSuperBoost = string.match(reward.icon, 'mastery_super_boost')
		 	if isSuperBoost then
		 		iconWidget:SetCallback('onmouseover', function()
		 			GetWidget('stats_mastery_superboost_tip'):SetVisible(true)
		 			end)
		 		iconWidget:SetCallback('onmouseout', function()
		 			GetWidget('stats_mastery_superboost_tip'):SetVisible(false)
		 			end)
		 		iconWidget:SetNoClick(false)
		 	else
		 		iconWidget:ClearCallback('onmouseover')
		 		iconWidget:ClearCallback('onmouseout')
		 		iconWidget:SetNoClick(true)
		 	end

			if (reward.taken) then
				GetWidget('stats_mastery_reward_single_'..i..'_bg'):SetTexture('/ui/fe2/mastery/reward_bg_3.tga')
				GetWidget('stats_mastery_reward_single_'..i..'_showbtn'):SetVisible(true)
				GetWidget('stats_mastery_reward_single_'..i..'_hidebtn'):SetVisible(false)
				GetWidget('stats_mastery_reward_single_'..i..'_btn'):SetEnabled(false)
				GetWidget('stats_mastery_reward_single_'..i..'_text'):SetColor('#424242')
				GetWidget('stats_mastery_reward_single_'..i..'_btntext'):SetColor('#424242')
				GetWidget('stats_mastery_reward_single_'..i..'_icon'):SetRenderMode('grayscale')
			elseif (totalLevel >= reward.level) and IsMe(statsnickname) then
				GetWidget('stats_mastery_reward_single_'..i..'_bg'):SetTexture('/ui/fe2/mastery/reward_bg_2.tga')
				GetWidget('stats_mastery_reward_single_'..i..'_showbtn'):SetVisible(true)
				GetWidget('stats_mastery_reward_single_'..i..'_hidebtn'):SetVisible(false)
				GetWidget('stats_mastery_reward_single_'..i..'_btn'):SetEnabled(true)
				GetWidget('stats_mastery_reward_single_'..i..'_text'):SetColor('white')
				GetWidget('stats_mastery_reward_single_'..i..'_btntext'):SetColor('#db9c3f')

				GetWidget('stats_mastery_reward_single_'..i..'_icon'):SetRenderMode('normal')

				Trigger('HasValidMasteryReward', 'true')
			else
				GetWidget('stats_mastery_reward_single_'..i..'_bg'):SetTexture('/ui/fe2/mastery/reward_bg_1.tga')
				GetWidget('stats_mastery_reward_single_'..i..'_showbtn'):SetVisible(false)
				GetWidget('stats_mastery_reward_single_'..i..'_hidebtn'):SetVisible(true)
				GetWidget('stats_mastery_reward_single_'..i..'_text'):SetColor('white')
				GetWidget('stats_mastery_reward_single_'..i..'_icon'):SetRenderMode('normal')

				if ((lastReward == nil) or (lastReward.level <= totalLevel)) and IsMe(statsnickname) then
					GetWidget('stats_mastery_reward_single_'..i..'_effect'):SetVisible(true)
				end
			end

			GetWidget('stats_mastery_reward_single_'..i):SetVisible(true)
		end
	end
end

function Player_Stats.SetMasteryInfo(mastery, rewards, nickname)
	if (GetCvarBool('cg_masteryuidebug')) then
		mastery = GetCvarString('cg_masteryHeroString')
		rewards = GetCvarString('cg_masteryRewardsString')
	end
	--mastery = 'Hero_Artillery,32599,Hero_Behemoth,32599,Hero_Chipper,32599,Hero_Artesia, 32599,Hero_Frosty,32600'
	--rewards = '1,false,239,malaysia,/ui/icons/flags/malaysia.tga,1,0,0,5,false,0,0,0,0,0,100,10,false,120001,Mastery Boost,/ui/fe2/store/icons/mastery_boost.tga,10,0,0,15,false,0,0,0,0,50,0,1,false,239,malaysia,/ui/icons/flags/malaysia.tga,1,0,0,5,false,0,0,0,0,0,100,10,false,120001,Mastery Boost,/ui/fe2/store/icons/mastery_boost.tga,10,0,0,15,false,0,0,0,0,50,0,1,false,239,malaysia,/ui/icons/flags/malaysia.tga,1,0,0,5,false,0,0,0,0,0,100,10,false,120001,Mastery Boost,/ui/fe2/store/icons/mastery_boost.tga,10,0,0,15,false,0,0,0,0,50,0'
	Echo('Player_Stats.SetMasteryInfo(mastery): '..mastery)
	Echo('Player_Stats.SetMasteryInfo(rewards): '..rewards)

	Player_Stats.ResetMasteryInfo()
	statsnickname = nickname
	local heroProficiency = GetHeroProficiency(mastery)
	for i,v in ipairs(heroProficiency) do
		local widget = GetWidget('stats_mastery_hero_proficiency_single_heronumber_'..i)
		if (widget) then
			widget:SetText(tostring(v))
		end
	end

	local heroUpgradeInfo = GetHeroMasteryUpgradeInfo(mastery)
    totalLevel = 0
    local i = 1
	for _,v in ipairs(heroUpgradeInfo) do
		if v.heroName then
			if i <= 5 then
				if (tonumber(v.level) < GetCvarInt('hero_mastery_maxlevel_new')) then
					local widget = GetWidget('stats_mastery_levelup_single_'..i)
					if (widget) then
						GetWidget('stats_mastery_levelup_single_'..i..'_heroicon'):SetTexture(v.heroIcon)
						GetWidget('stats_mastery_levelup_single_'..i..'_heroname'):SetText(v.heroName)
						GetWidget('stats_mastery_levelup_single_'..i..'_percenttext'):SetText(Translate('mastery_stats_leftpercent', 'value', v.percent))
						GetWidget('stats_mastery_levelup_single_'..i..'_level'):SetText('LV.'..(v.level))
						GetWidget('stats_mastery_levelup_single_'..i..'_percent'):SetWidth(v.percent..'%')
						widget:SetVisible(1)
						i = i + 1
					end
				end
			end
			totalLevel = totalLevel + tonumber(v.level or 0)
		end
	end

    GetWidget('player_stats_mastery_totallevel'):SetText(tostring(totalLevel))

	rewardsinfo = GetMasteryRewardsInfo(rewards)
	local scrollbar = GetWidget('stats_mastery_reward_scroll')

	if table.getn(rewardsinfo) > 3 then
		local itemCount = table.getn(rewardsinfo)
		pageStep = 50
		masteryRewardScrollMax = (itemCount - 3) * pageStep
		scrollbar:SetMaxValue(masteryRewardScrollMax)
		pageMax = (itemCount - 3) * 10.8 + 0.8
	else
		masteryRewardScrollMax = 1
		scrollbar:SetMaxValue(1)
		pageStep = 0
		pageMax = 0
	end

	scrollbar:SetValue(1)

	SetMasteryReward()
end


local function EntityDefinitionsLoaded2()
	GetWidget('player_stats_mastery'):Sleep(1, function() GetWidget('main_stats_blocker'):FadeOut(50) end)
end
function Player_Stats.OnShowPlayerStats()
	Echo('Player_Stats.OnShowPlayerStats   '..Main.walkthroughState)
	GetWidget('RegisterEntityDefinitionsHelper'):DoEvent()
end

Player_Stats.autoSlip = 'stop'
function Player_Stats.OnMasteryFrame()
	local scroll = GetWidget('stats_mastery_reward_scroll')
	local curPos = tonumber(scroll:GetValue())
	GetWidget('stats_mastery_autoslip_up'):SetVisible(curPos > 1)
	GetWidget('stats_mastery_autoslip_down'):SetVisible(curPos < masteryRewardScrollMax)

    local widget = GetWidget('player_stats_mastery_rewards_root')
	if Player_Stats.autoSlip == 'up' then
		scroll:SetValue(scroll:GetValue() - 1)
	elseif Player_Stats.autoSlip == 'down' then
		scroll:SetValue(scroll:GetValue() + 1)
	end
end

function Player_Stats.OpenTourStats()
	local searchName = GetCvarString('_player_stats_searchname')
	local url = HoN_Region:GetPlayerTourStats() .. '&nickname=' .. searchName
	Echo('Player_Stats.OpenTourStats url: '..url)

	UIManager.GetInterface('webpanel'):HoNWebPanelF('ShowPlayerTourStats', GetWidget('web_browser_player_tour_stats_insert'), url)
	UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'player_tour_stats', nil, false)
end