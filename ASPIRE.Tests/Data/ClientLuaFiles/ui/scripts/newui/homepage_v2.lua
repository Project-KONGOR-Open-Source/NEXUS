local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort
local interface, interfaceName = object, object:GetName()

local VIP_ITEM_MAX_COUNT = 5
local FILTER_THROSHOLD = 3.0

Homepage_V2 = {}

for i=0,3 do
	interface:RegisterWatch('HeroSelectHeroAbilityInfo'..i, function(_, ...) Homepage_V2:OnSelectHeroAbilityInfo(i+1, ...) end)
end
interface:RegisterWatch('HeroSelectHeroInfo', function(_, ...) Homepage_V2:OnSelectHeroInfo(...) end)

function Homepage_V2:GetHeroIconPath(name)
	local tempName = name
	if tempName == 'forsakenarcher' then
		tempName = 'forsaken_archer'
	elseif tempName == 'corrupteddisciple' then
		tempName = 'corrupted_disciple'
	elseif tempName == 'sandwraith' then
		tempName = 'sand_wraith'
	elseif tempName == 'witchslayer' then
		tempName = 'witch_slayer'
	elseif tempName == 'dwarfmagi' then
		tempName = 'dwarf_magi'
	elseif tempName == 'flintbeastwood' then
		tempName = 'flint_beastwood'
	elseif tempName == 'doctorrepulsor' then
		tempName = 'doctor_repulsor'
	elseif tempName == 'bombardier' then
		tempName = 'bomb'
	elseif tempName == 'emeraldwarden' then
		tempName = 'emerald_warden'
	elseif tempName == 'monkeyking' then
		tempName = 'monkey_king'
	elseif tempName == 'masterofarms' then
		tempName = 'master_of_arms'
	elseif tempName == 'sirbenzington' then
		tempName = 'sir_benzington'
	elseif tempName == 'kingklout' then
		tempName = 'king_klout'
	end

	local path = '/ui/common/ability_coverup.tga'
	if NotEmpty(tempName) then
		path = '/heroes/'..tempName..'/icon.tga'
	end

	if tempName == 'pollywogpriest' then
		path = '/heroes/pollywogpriest/icons/hero.tga'
	elseif tempName == 'electrician' then
		path = '/heroes/electrician/icons/hero.tga'
	elseif tempName == 'hammerstorm' then
		path = '/heroes/hammerstorm/icons/hero.tga'
	elseif tempName == 'yogi' then
		path = '/heroes/yogi/icons/hero.tga'
	elseif tempName == 'cthulhuphant' then
		path = '/heroes/cthulhuphant/alt/icon.tga'
	elseif tempName == 'artillery' then
		path = '/heroes/artillery/alt/icon.tga'
	end

	return path
end

function Homepage_V2:AdjustMidButtonText(btnName, content, btnType)
	local text = Translate(content)
	
	for i=1,4 do
		local labelWidget = GetWidget(btnName..'_btntext_'..i)
		local stringWidth = labelWidget:GetStringWidth(text)
		labelWidget:SetWidth(stringWidth)

		if btnType == 'left1' or btnType == 'right3' then
			if i == 1 then 
				labelWidget:SetColor('#cae4e5')
				labelWidget:SetOutlineColor('#072627')
			elseif i == 2 or i == 3 then
				labelWidget:SetColor('#d9feff')
				labelWidget:SetOutlineColor('#072627')
			else
				labelWidget:SetColor('#a1948f')
				labelWidget:SetOutlineColor('#230000')
			end
		else
			if i == 1 then 
				labelWidget:SetColor('#e1c0ab')
				labelWidget:SetOutlineColor('#230000')
			elseif i == 2 or i == 3 then
				labelWidget:SetColor('#fedcc7')
				labelWidget:SetOutlineColor('#230000')
			else
				labelWidget:SetColor('#a1948f')
				labelWidget:SetOutlineColor('#230000')
			end
		end
	end
end

function Homepage_V2:UpdateMySimpleStats()
	RequestPlayerSimpleStats(StripClanTag(GetAccountName()))
	GetWidget('homepage_selfInfo_mask'):SetVisible(true)
end

function Homepage_V2:OnPlayerSimpleStats(widget, ...)
	GetWidget('homepage_selfInfo_mask'):SetVisible(false)
	
	-- Logan (2026-02-12): Fix for Modern Server Response (Table vs Positional)
	local params = arg
	Echo('^y OnPlayerSimpleStats: Type of arg[1]: ' .. type(arg[1]))
	if type(arg[1]) == 'table' then
		Echo('^y OnPlayerSimpleStats: Detected Table! Converting...')
		params = {}
		for k,v in pairs(arg[1]) do
			-- Echo('Key: ' .. tostring(k) .. ' Value: ' .. tostring(v))
			if type(k) == 'number' then
				params[k+1] = v
			end
		end
		-- Ensure strict nil checks don't break logic
		setmetatable(params, {__index = function(t,k) return nil end})
	else
		Echo('^r OnPlayerSimpleStats: NOT A TABLE. Using raw args.')
	end

	local nickName = params[1]
	local level = tonumber(params[2])
	local level_exp = tonumber(params[3])
	local heroes_num = params[4]
	local avatars_num = params[5]
	local matches_num = params[6]
	local mvp_num = tonumber(params[7])
	local selected_upgrades = params[8]
	local account_id = tonumber(params[9])
	local mvp_startindex = 19
	local mvp_awards_name = {params[mvp_startindex], params[mvp_startindex+1], params[mvp_startindex+2], params[mvp_startindex+3]}
	local mvp_awards_number = {params[mvp_startindex+4], params[mvp_startindex+5], params[mvp_startindex+6], params[mvp_startindex+7]}


	if string.lower(StripClanTag(nickName))  ~= string.lower(StripClanTag(GetAccountName())) then return end

	local widget = GetWidget('homepage_selfInfo_playericon')
	widget:SetTexture('/ui/common/ability_coverup.tga')

	local playerIcon = GetAccountIconTexturePathFromUpgrades(selected_upgrades, account_id)
	if NotEmpty(playerIcon) then
		widget:SetTexture(playerIcon)
	else
		widget:SetAvatar('http://www.heroesofnewerth.com/getAvatar.php?id='..account_id)
	end

	--------------------------------------------------------------------------------------------------

	widget = GetWidget('homepage_selfInfo_playername')
	local nameColor = GetChatNameColorStringFromUpgrades(selected_upgrades)
	local nameColorFont = GetChatNameColorFontFromUpgrades(selected_upgrades)
	local font = NotEmpty(nameColorFont) and nameColorFont..'_16' or 'dyn_16'

	widget:SetFont(font)
	widget:SetColor(NotEmpty(nameColor) and nameColor or '#efd2c0')
	widget:SetGlow(GetChatNameGlowFromUpgrades(selected_upgrades))
	widget:SetGlowColor(GetChatNameGlowColorStringFromUpgrades(selected_upgrades))
	widget:SetBackgroundGlow(GetChatNameBackgroundGlowFromUpgrades(selected_upgrades))

	if (font == '8bit_16') then
		nickName = string.upper(nickName)
	end
	widget:SetText(nickName)

	GetWidget('homepage_selfInfo_playername_btn'):SetWidth(widget:GetStringWidth(nickName))
	--------------------------------------------------------------------------------------------------
	widget = GetWidget('homepage_selfInfo_chatsymbol')
	local chatSymbol = GetChatSymbolTexturePathFromUpgrades(selected_upgrades)
	local nameColor = GetChatNameColorTexturePathFromUpgrades(selected_upgrades)

	if NotEmpty(chatSymbol) then
		widget:SetTexture(chatSymbol)
	elseif NotEmpty(nameColor) then
		widget:SetTexture(nameColor)
	else
		widget:SetTexture('$invis')
	end
	--------------------------------------------------------------------------------------------------
	local levelExp = tonumber(interface:UICmd('GetAccountExperienceForLevel('..level..')'))
	local nextLevelExp = tonumber(interface:UICmd('GetAccountExperienceForNextLevel(\''..level_exp..'\')'))
	local nextLevelPercent = tonumber(interface:UICmd('GetAccountPercentNextLevel(\''..level_exp..'\')'))
	GetWidget('homepage_selfInfo_playerlevel'):SetText(Translate('newui_playerstats_level', 'level', tostring(level)))
	GetWidget('homepage_selfInfo_levelpercent'):SetWidth(tostring(nextLevelPercent*100)..'%')

	GetWidget('homepage_selfInfo_levelexp'):SetText(tostring(level_exp - levelExp)..'/'..(nextLevelExp - levelExp))

	----------------------------------------------------------------------------------------------
	if GetCvarBool('cl_GarenaEnable') then 
		GetWidget('homepage_seleInfo_bottom_heroes_value'):SetText(heroes_num)
	end
	GetWidget('homepage_seleInfo_bottom_matches_value'):SetText(matches_num)
	GetWidget('homepage_seleInfo_bottom_avatars_value'):SetText(avatars_num)
	----------------------------------------------------------------------------------------------

	GetWidget('homepage_seleInfo_mvp_1_icon'):SetTexture('/ui/fe2/newui/res/playerstats/mvpawards/awd_mvp_big.png')
	GetWidget('homepage_seleInfo_mvp_1_text'):SetText('X '.. mvp_num)
	GetWidget('homepage_seleInfo_mvp_1'):SetCallback('onmouseover', function()
		Common_V2:ShowSimpleTip(true, 'award_mvp', '80i')
	end)

	for i=2, VIP_ITEM_MAX_COUNT do
		GetWidget('homepage_seleInfo_mvp_'..i..'_icon'):SetTexture('/ui/fe2/newui/res/playerstats/mvpawards/'..mvp_awards_name[i-1]..'_big.png')
		GetWidget('homepage_seleInfo_mvp_'..i..'_text'):SetText('X '.. mvp_awards_number[i-1])

		local value = explode('_', mvp_awards_name[i-1])
		GetWidget('homepage_seleInfo_mvp_'..i):SetCallback('onmouseover', function()
			Common_V2:ShowSimpleTip(true, 'award_'..value[2], '180i')
		end)
	end	
end

function Homepage_V2:OnSelectUpgradesStatus(widget, ...)
	if tonumber(arg[1]) == 2 then
		Homepage_V2:UpdateMySimpleStats()
	end
end

function Homepage_V2:OnInfoChanged()
	Homepage_V2:UpdateMySimpleStats()
end

function Homepage_V2:GetMotdURL()
	local motdURL = HoN_Region.regionTable[HoN_Region.activeRegion].motdURL.retail

	if GetCvarBool('releaseStage_rc') then
		motdURL = HoN_Region.regionTable[HoN_Region.activeRegion].motdURL.rct
	elseif GetCvarBool('releaseStage_test') then
		motdURL = HoN_Region.regionTable[HoN_Region.activeRegion].motdURL.sbt
	elseif GetCvarBool('releaseStage_pbt') then
		motdURL = HoN_Region.regionTable[HoN_Region.activeRegion].motdURL.pbt
	end

	return motdURL.."?lang="..GetCvarString('host_language')
end

function Homepage_V2:InitMotd()
	local url = Homepage_V2:GetMotdURL()

	UIManager.GetInterface('webpanel'):HoNWebPanelF('LoadURLWithThrob', url, GetWidget('motd'))
end

function Homepage_V2:SetHeroInfo(heroInfo)
	local function GetHeroCategory(categoryInfo)
		local ratingMap = {
			solorating		= 'solo',
			junglerating	= 'jungle',
			carryrating		= 'carry',
			supportrating	= 'support',
			initiatorrating	= 'initiator',
			gankerrating	= 'ganker',
			pusherrating	= 'pusher',
			rangedrating	= 'ranged',
			meleerating		= 'melee',
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

	GetWidget('homepage_hero_info_heroname'):SetText(heroInfo.name)
	GetWidget('homepage_hero_info_heroicon'):SetTexture(heroInfo.icon)
	
	local masteryLevel = GetMasteryLevelByExp(heroInfo.masteryExp)
	local masteryType = GetMasterTypeByLevel(masteryLevel)
	local masteryExpMin, masteryExpMax = GetMasteryExpByLevel(masteryLevel)
	local masteryPercent = math.floor((heroInfo.masteryExp - masteryExpMin) / (masteryExpMax - masteryExpMin) * 100 + 0.5)

	GetWidget('homepage_hero_info_mastery_badge'):SetTexture('/ui/fe2/newui/res/homepage/badge_'..masteryType..'.png')
	GetWidget('homepage_hero_info_mastery_level'):SetText(tostring(masteryLevel))

	if masteryLevel >= GetCvarInt('hero_mastery_maxlevel_new') then
		GetWidget('homepage_hero_info_mastery_exp'):SetWidth('100%')
	else
		GetWidget('homepage_hero_info_mastery_exp'):SetWidth(tostring(masteryPercent)..'%')
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
	GetWidget('homepage_hero_info_mastery_exp'):SetColor(masteryColor)

	local primAttrImage = '/ui/fe2/elements/store2/primary_none.png'
	if heroInfo.primaryAttri == 'strength' then 
		primAttrImage = '/ui/fe2/elements/store2/primary_strength.png'
	elseif heroInfo.primaryAttri == 'agility' then
		primAttrImage = '/ui/fe2/elements/store2/primary_agility.png'
	elseif heroInfo.primaryAttri == 'intelligence' then
		primAttrImage = '/ui/fe2/elements/store2/primary_intelligence.png'
	end
	GetWidget('homepage_hero_info_primattr'):SetTexture(primAttrImage)	

	local attackTypeImage = '/ui/fe2/elements/store2/melee.png'
	if heroInfo.attackType == 'ranged' then
		attackTypeImage = '/ui/fe2/elements/store2/ranged.png'
	end
	GetWidget('homepage_hero_info_attacktype'):SetTexture(attackTypeImage)	
	

	GetWidget('homepage_hero_info_category'):SetText(GetHeroCategory(heroInfo))

	local strengthStr = '^o'..heroInfo.strength..'^* ( +'..heroInfo.strengthPerLevel..' )'
	local agilityStr = '^o'..heroInfo.agility..'^* ( +'..heroInfo.agilityPerLevel..' )'
	local intelligenceStr = '^o'..heroInfo.intelligence..'^* ( +'..heroInfo.intelligencePerLevel..' )'

	GetWidget('homepage_hero_info_strength'):SetText(strengthStr)
	GetWidget('homepage_hero_info_agility'):SetText(agilityStr)
	GetWidget('homepage_hero_info_intelligence'):SetText(intelligenceStr)

	GetWidget('homepage_hero_info_role'):SetText(heroInfo.role)

	-- Attributes
	-- 1. Attack Range
	local widget = GetWidget('homepage_hero_info_attributes_1')
	local strValue = Translate('store2_hero_select_label_attack_range', 'range', heroInfo.attackRange)
	widget:SetText(strValue)

	-- 2. Damage
	widget = GetWidget('homepage_hero_info_attributes_2')
	strValue = Translate('store2_hero_select_label_damage', 'damage', tostring(heroInfo.damageMin)..' - '..heroInfo.damageMax)
	widget:SetText(strValue)

	-- 3. Attack Speed
	widget = GetWidget('homepage_hero_info_attributes_3')
	strValue = Translate('store2_hero_select_label_attack_speed', 'speed', heroInfo.attackSpeed)
	widget:SetText(strValue)

	-- 4. Armor
	widget = GetWidget('homepage_hero_info_attributes_4')
	strValue = Translate('store2_hero_select_label_armor', 'armor', heroInfo.armor)
	widget:SetText(strValue)

	-- 5. Move Speed
	widget = GetWidget('homepage_hero_info_attributes_5')
	strValue = Translate('store2_hero_select_label_mvspeed', 'mvspeed', heroInfo.moveSpeed)
	widget:SetText(strValue)

	-- 6. Magic Armor
	widget = GetWidget('homepage_hero_info_attributes_6')
	strValue = Translate('store2_hero_select_label_magicarmor', 'armor', heroInfo.magicArmor)
	widget:SetText(strValue)
end

function Homepage_V2:SetHeroAbilityInfo(i, ability)
	local descStr = ability.desc

	GetWidget('homepage_hero_info_ability_'..i..'_icon'):SetTexture(ability.icon)
	GetWidget('homepage_hero_info_ability_'..i..'_name'):SetText(ability.name)

	-- mana cost
	if NotEmpty(ability.manaCost) then
		GetWidget('homepage_hero_info_ability_'..i..'_manacost'):SetText(Translate('heroinfo_mana_cost') .. ability.manaCost)
	else
		GetWidget('homepage_hero_info_ability_'..i..'_manacost'):SetText('')
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

	local maxHeight = GetWidget('homepage_hero_info_ability_'..i..'_descmax'):GetHeight()
	for j=10, 6, -1 do
		GetWidget('homepage_hero_info_ability_'..i..'_desc'):SetFont('dyn_'..j)
		GetWidget('homepage_hero_info_ability_'..i..'_desc'):SetText(descStr)

		local height = GetWidget('homepage_hero_info_ability_'..i..'_desc'):GetHeight()
		if height <= maxHeight then break end

		GetWidget('homepage_hero_info_ability_'..i..'_desc'):SetText('hahaha')
	end
end

function Homepage_V2:OnSelectHeroInfo(...)
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

	Homepage_V2:SetHeroInfo(heroInfo)
end

function Homepage_V2:OnSelectHeroAbilityInfo(i, ...)
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

	Homepage_V2:SetHeroAbilityInfo(i, ability)
end

function Homepage_V2:ClickedLadder(toggle)
	if toggle  then
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

function Homepage_V2:ClickedLadder2(toggle)
	if toggle then
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

function Homepage_V2:ClickedQuests(toggle)
	if toggle then
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

function Homepage_V2:ClickedCodexLevel(toggle)
	if toggle then
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

function Homepage_V2:ClickedMastery(toggle)
	if toggle then
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