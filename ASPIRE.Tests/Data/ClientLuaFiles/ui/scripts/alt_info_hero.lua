----------------------------------------------------------
--	Name: 		Alt Info Hero Script	            	--
-- 	Version: 	4.2										--
--  (C)2017 Garena Shanghai Games						--
----------------------------------------------------------

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()

RegisterScript2('AltInfoHero', '1')

AltInfoHero = _G['AltInfoHero'] or {}

AltInfoHero.HEALTH_PER_PIP 		= GetCvarInt('cg_altInfoPipSpacing', true) or 1000
AltInfoHero.HEALTH_PER_SUB_PIP 	= GetCvarInt('cg_altInfoPipSubSpacing', true) or 1000

AltInfoHero.cur_level_label				= {}
AltInfoHero.cur_main_frame				= {}
AltInfoHero.cur_main_frame_img			= {}
AltInfoHero.cur_health_lerp				= {}
AltInfoHero.cur_health_panel			= {}
AltInfoHero.cur_health_bar				= {}
AltInfoHero.cur_mana_panel				= {}
AltInfoHero.cur_mana_bar				= {}
AltInfoHero.cur_shield_panel			= {}
AltInfoHero.cur_shield_bar				= {}
AltInfoHero.cur_health_mana_parent		= {}
AltInfoHero.cur_healthPipContainer		= {}
AltInfoHero.cur_healthSubPipContainer	= {}
AltInfoHero.cur_behavior_name			= {}

AltInfoHero.nameCompare 	= {}
AltInfoHero.levelCompare 	= {}
AltInfoHero.playerCompare 	= {}
AltInfoHero.heroName 		= {}
AltInfoHero.isHovering 		= {}

AltInfoHero.player_label_above		= {}
AltInfoHero.inventory_panel			= {}
AltInfoHero.player_label			= {}
AltInfoHero.name_label 				= {}
AltInfoHero.main_panel 				= {}

AltInfoHero.level_label 			= {}
AltInfoHero.main_frame 				= {}
AltInfoHero.main_frame_img			= {}
AltInfoHero.health_lerp 			= {}
AltInfoHero.health_panel 			= {}
AltInfoHero.health_bar 				= {}
AltInfoHero.mana_panel 				= {}
AltInfoHero.mana_bar 				= {}
AltInfoHero.shield_panel			= {}
AltInfoHero.shield_bar 				= {}
AltInfoHero.health_mana_parent 		= {}
AltInfoHero.healthPipContainer 		= {}
AltInfoHero.healthSubPipContainer 	= {}
AltInfoHero.behavior_name 			= {}

AltInfoHero.level_label_l 			= {}
AltInfoHero.main_frame_l			= {}
AltInfoHero.main_frame_img_l		= {}
AltInfoHero.health_lerp_l			= {}
AltInfoHero.health_panel_l			= {}
AltInfoHero.health_bar_l			= {}
AltInfoHero.mana_panel_l			= {}
AltInfoHero.mana_bar_l				= {}
AltInfoHero.shield_panel_l			= {}
AltInfoHero.shield_bar_l			= {}
AltInfoHero.health_mana_parent_l	= {}
AltInfoHero.healthPipContainer_l	= {}
AltInfoHero.healthSubPipContainer_l	= {}
AltInfoHero.behavior_name_l			= {}

AltInfoHero.speaker		 			= {}
AltInfoHero.afk		 				= {}
AltInfoHero.state1		 			= {}
AltInfoHero.state1Icon		 		= {}
AltInfoHero.state1LabelBacker		= {}
AltInfoHero.state1BarContainer		= {}
AltInfoHero.state1Label		 		= {}
AltInfoHero.state1Bar		 		= {}

local function chooseLargeWidgets(unit, largeWdg)
	AltInfoHero.cur_level_label[unit]					= largeWdg and AltInfoHero.level_label_l[unit] or AltInfoHero.level_label[unit]
	AltInfoHero.cur_main_frame[unit]					= largeWdg and AltInfoHero.main_frame_l[unit] or AltInfoHero.main_frame[unit]
	AltInfoHero.cur_main_frame_img[unit]				= largeWdg and AltInfoHero.main_frame_img_l[unit] or AltInfoHero.main_frame_img[unit]
	AltInfoHero.cur_health_lerp[unit]					= largeWdg and AltInfoHero.health_lerp_l[unit] or AltInfoHero.health_lerp[unit]
	AltInfoHero.cur_health_panel[unit]					= largeWdg and AltInfoHero.health_panel_l[unit] or AltInfoHero.health_panel[unit]
	AltInfoHero.cur_health_bar[unit]					= largeWdg and AltInfoHero.health_bar_l[unit] or AltInfoHero.health_bar[unit]
	AltInfoHero.cur_mana_panel[unit]					= largeWdg and AltInfoHero.mana_panel_l[unit] or AltInfoHero.mana_panel[unit]
	AltInfoHero.cur_mana_bar[unit]						= largeWdg and AltInfoHero.mana_bar_l[unit] or AltInfoHero.mana_bar[unit]
	AltInfoHero.cur_shield_panel[unit]					= largeWdg and AltInfoHero.shield_panel_l[unit] or AltInfoHero.shield_panel[unit]
	AltInfoHero.cur_shield_bar[unit]					= largeWdg and AltInfoHero.shield_bar_l[unit] or AltInfoHero.shield_bar[unit]
	AltInfoHero.cur_health_mana_parent[unit]			= largeWdg and AltInfoHero.health_mana_parent_l[unit] or AltInfoHero.health_mana_parent[unit]
	AltInfoHero.cur_healthPipContainer[unit]			= largeWdg and AltInfoHero.healthPipContainer_l[unit] or AltInfoHero.healthPipContainer[unit]
	AltInfoHero.cur_healthSubPipContainer[unit]			= largeWdg and AltInfoHero.healthSubPipContainer_l[unit] or AltInfoHero.healthSubPipContainer[unit]
	AltInfoHero.cur_behavior_name[unit]					= largeWdg and AltInfoHero.behavior_name_l[unit] or AltInfoHero.behavior_name[unit]

	AltInfoHero.main_frame_l[unit]:SetVisible(largeWdg)
	AltInfoHero.main_frame[unit]:SetVisible(not largeWdg)

	AltInfoHero.inventory_panel[unit]:SetY(largeWdg and '0.5h' or '1.0h')

	AltInfoHero.cur_behavior_name[unit]:SetY(largeWdg and '-2.35h' or '-2h')

	AltInfoHero.player_label_above[unit]:SetY(largeWdg and '-0.25h' or '0h')
end

local function SetupWidgets(unit)
	AltInfoHero.player_label_above[unit] 	= interface:GetWidget('player_label_above'..unit)
	AltInfoHero.inventory_panel[unit] 		= interface:GetWidget('inventory_panel'..unit)
	AltInfoHero.player_label[unit] 			= interface:GetWidget('player_label'..unit)
	AltInfoHero.name_label[unit] 			= interface:GetWidget('name_label'..unit)
	AltInfoHero.main_panel[unit] 			= interface:GetWidget('main_panel'..unit)

	AltInfoHero.level_label[unit] 			= interface:GetWidget('level_label'..unit)
	AltInfoHero.main_frame[unit] 			= interface:GetWidget('main_frame'..unit)
	AltInfoHero.main_frame_img[unit]		= interface:GetWidget('main_frame_img'..unit)
	AltInfoHero.health_lerp[unit] 			= interface:GetWidget('health_lerp'..unit)
	AltInfoHero.health_panel[unit] 			= interface:GetWidget('health_panel'..unit)
	AltInfoHero.health_bar[unit] 			= interface:GetWidget('health_bar'..unit)
	AltInfoHero.mana_panel[unit] 			= interface:GetWidget('mana_panel'..unit)
	AltInfoHero.mana_bar[unit] 				= interface:GetWidget('mana_bar'..unit)
	AltInfoHero.shield_panel[unit]			= interface:GetWidget('shield_panel'..unit)
	AltInfoHero.shield_bar[unit] 			= interface:GetWidget('shield_bar'..unit)
	AltInfoHero.health_mana_parent[unit] 	= interface:GetWidget('health_mana_parent'..unit)
	AltInfoHero.healthPipContainer[unit] 	= interface:GetWidget('health_pip_container'..unit)
	AltInfoHero.healthSubPipContainer[unit] = interface:GetWidget('health_subpip_container'..unit)
	AltInfoHero.behavior_name[unit] 		= interface:GetWidget('behavior_name'..unit)

	AltInfoHero.level_label_l[unit]			= interface:GetWidget('level_label_l'..unit)
	AltInfoHero.main_frame_l[unit] 			= interface:GetWidget('main_frame_l'..unit)
	AltInfoHero.main_frame_img_l[unit]		= interface:GetWidget('main_frame_img_l'..unit)
	AltInfoHero.health_lerp_l[unit] 		= interface:GetWidget('health_lerp_l'..unit)
	AltInfoHero.health_panel_l[unit] 		= interface:GetWidget('health_panel_l'..unit)
	AltInfoHero.health_bar_l[unit] 			= interface:GetWidget('health_bar_l'..unit)
	AltInfoHero.mana_panel_l[unit] 			= interface:GetWidget('mana_panel_l'..unit)
	AltInfoHero.mana_bar_l[unit] 			= interface:GetWidget('mana_bar_l'..unit)
	AltInfoHero.shield_panel_l[unit]		= interface:GetWidget('shield_panel_l'..unit)
	AltInfoHero.shield_bar_l[unit] 			= interface:GetWidget('shield_bar_l'..unit)
	AltInfoHero.health_mana_parent_l[unit] 	= interface:GetWidget('health_mana_parent_l'..unit)
	AltInfoHero.healthPipContainer_l[unit] 	= interface:GetWidget('health_pip_container_l'..unit)
	AltInfoHero.healthSubPipContainer_l[unit] = interface:GetWidget('health_subpip_container_l'..unit)
	AltInfoHero.behavior_name_l[unit] 		= interface:GetWidget('behavior_name_l'..unit)

	AltInfoHero.speaker[unit]				= interface:GetWidget('speaker'..unit)
	AltInfoHero.afk[unit]					= interface:GetWidget('afk'..unit)
	AltInfoHero.state1[unit]				= interface:GetWidget('state1'..unit)
	AltInfoHero.state1Icon[unit]			= interface:GetWidget('state1Icon'..unit)
	AltInfoHero.state1LabelBacker[unit]		= interface:GetWidget('state1LabelBacker'..unit)
	AltInfoHero.state1BarContainer[unit]	= interface:GetWidget('state1BarContainer'..unit)
	AltInfoHero.state1Label[unit]			= interface:GetWidget('state1Label'..unit)
	AltInfoHero.state1Bar[unit]				= interface:GetWidget('state1Bar'..unit)

	AltInfoHero.playerCompare[unit] = ''
	AltInfoHero.nameCompare[unit] = ''
	AltInfoHero.levelCompare[unit] = ''
	AltInfoHero.heroName[unit] = ''
	AltInfoHero.isHovering[unit] = false

	chooseLargeWidgets(unit, GetCvarBool('cg_altInfoLarge'))

	local screenH = GetScreenHeight()
	local healthX, healthY = '0.0h', '0.4h'
	local manaX, manaY = '0.0h', '1.1h'
	local healthH, manaH = '0.65h', '0.6h'
	local levelX, levelY, invY = '0.8h', '0.1h', '0.5h'
	local levelLX, levelLY = '1h', '0h'
	local frameLX = '-0.3h'
	if screenH >= 1080 then
		manaH = '0.5h'
		levelLX, levelLY = '1.1h', '0h'
	elseif screenH >= 1050 then
		manaH = '0.4h'
		levelX, levelY, invY = '0.8h', '0.1h', '0.5h'
	elseif screenH >= 1024 then
		manaH = '0.5h'
		levelX = '1h'
		levelLX = '1.1h'
	elseif screenH >= 960 then
		manaH = '0.3h'
		levelX = '0.8h'
		levelLX = '1.1h'
	elseif screenH >= 900 then
		manaH = '0.3h'
		levelX, levelY, invY = '1h', '0h', '0.25h'
		levelLX, levelLY = '1.1h', '-0.1h'
	elseif screenH >= 864 then
		levelX, levelY = '0.9h', '0.1h'
		manaH = '0.3h'
	elseif screenH >= 768 then
		frameLX = '-0.4h'
		levelX, levelY, invY = '0.9h', '-0.1h', '0h'
		healthH, healthY, manaH, manaY = '0.6h', '0.5h', '0.4h', '1.2h'
	elseif screenH >= 720 then
		levelX = '1.0h'
		healthH, healthY, manaH, manaY = '0.5h', '0.4h', '0.4h', '1.1h'
		healthX, manaX = '0.2h', '0.2h'
		levelLX = '1.2h'
	elseif screenH >= 600 then
		levelX, levelY = '1h', '0.0h'
		levelLX, levelLY = '1.3h', '0.1h'
		healthH = '0.8h'
		healthX, healthY = '0.1h', '0.4h'
		manaH = '0.5h'
		manaX, manaY = '0.2h', '1.2h'
	end

	AltInfoHero.main_frame_img_l[unit]:SetX(frameLX)

	AltInfoHero.level_label[unit]:SetX(levelX)
	AltInfoHero.level_label[unit]:SetY(levelY)

	AltInfoHero.level_label_l[unit]:SetX(levelLX)
	AltInfoHero.level_label_l[unit]:SetY(levelLY)

	AltInfoHero.health_panel[unit]:SetX(healthX)
	AltInfoHero.health_panel[unit]:SetY(healthY)
	AltInfoHero.health_panel[unit]:SetHeight(healthH)

	AltInfoHero.mana_panel[unit]:SetX(manaX)
	AltInfoHero.mana_panel[unit]:SetY(manaY)
	AltInfoHero.mana_panel[unit]:SetHeight(manaH)
end

local function UpdateBackgroundFrame(unit)
	local nameAboveHP = GetCvarInt('cg_altInfoNameAboveHP')
	local colorScheme = GetCvarInt('cg_altInfoColorScheme')
	-- Echo('UpdateBackgroundFrame unit:'..unit)

	if (AltInfoHero.nameCompare[unit]) and (AltInfoHero.isHovering[unit]) then
		AltInfoHero.inventory_panel[unit]:SetVisible(true)
		AltInfoHero.player_label[unit]:SetText(AltInfoHero.playerCompare[unit])
		AltInfoHero.name_label[unit]:SetText(AltInfoHero.nameCompare[unit])
		AltInfoHero.player_label_above[unit]:SetVisible(false)
		AltInfoHero.cur_main_frame_img[unit]:SetVisible(true)
		AltInfoHero.cur_behavior_name[unit]:SetVisible(false)
	else
		AltInfoHero.cur_main_frame_img[unit]:SetVisible(colorScheme == 1)
		AltInfoHero.inventory_panel[unit]:SetVisible(false)

		local showNameAboveHP = nameAboveHP > 0
		AltInfoHero.player_label_above[unit]:SetVisible(showNameAboveHP)

		if NotEmpty(AltInfoHero.behavior_name[unit]:GetText()) then
			AltInfoHero.cur_behavior_name[unit]:SetVisible(true)
			AltInfoHero.cur_behavior_name[unit]:SetY(showNameAboveHP and '-4h' or '-2h')
		end
	end

	if nameAboveHP == 1 then
		AltInfoHero.player_label_above[unit]:SetText(AltInfoHero.playerCompare[unit])
	elseif nameAboveHP == 2 then
		AltInfoHero.player_label_above[unit]:SetText(AltInfoHero.heroName[unit])
	else
		AltInfoHero.player_label_above[unit]:SetText('')
	end
	AltInfoHero.player_label_above[unit]:RecalculateSize()
end

local function AltInfo_HeroHealthCurrent(unit, self, currentHealth, maxHealth, pips)
	if not AtoB(pips) then
		AltInfoHero.cur_healthPipContainer[unit]:SetVisible(false)
		AltInfoHero.cur_healthSubPipContainer[unit]:SetVisible(false)
		return
	end

	local healthMax = AtoN(maxHealth)
	local showSubPips, showPips = true, true
	if healthMax >= 12000 then
		showSubPips = false
		showPips = false
	elseif healthMax >= 8000 then
		showSubPips = false
		AltInfoHero.cur_healthPipContainer[unit]:SetTexture('/ui/elements/bar_segment_big_16.tga')
	elseif healthMax >= 5000 then
		showSubPips = false
		AltInfoHero.cur_healthPipContainer[unit]:SetTexture('/ui/elements/bar_segment_big_32.tga')
	elseif healthMax >= 3000 then
		AltInfoHero.cur_healthPipContainer[unit]:SetTexture('/ui/elements/bar_segment_big_64.tga')
		AltInfoHero.cur_healthSubPipContainer[unit]:SetTexture('/ui/elements/bar_segment_8.tga')
	elseif healthMax >= 2000 then
		AltInfoHero.cur_healthPipContainer[unit]:SetTexture('/ui/elements/bar_segment_big_64.tga')
		AltInfoHero.cur_healthSubPipContainer[unit]:SetTexture('/ui/elements/bar_segment_16.tga')
	elseif healthMax >= 1000 then
		AltInfoHero.cur_healthPipContainer[unit]:SetTexture('/ui/elements/bar_segment_big_128.tga')
		AltInfoHero.cur_healthSubPipContainer[unit]:SetTexture('/ui/elements/bar_segment_32.tga')
	elseif healthMax >= 800 then
		showPips = false
		AltInfoHero.cur_healthSubPipContainer[unit]:SetTexture('/ui/elements/bar_segment_32.tga')
	else
		showPips = false
		AltInfoHero.cur_healthSubPipContainer[unit]:SetTexture('/ui/elements/bar_segment_64.tga')
	end
	AltInfoHero.cur_healthPipContainer[unit]:SetVisible(showPips)
	AltInfoHero.cur_healthPipContainer[unit]:SetUScale(AltInfoHero.cur_healthPipContainer[unit]:GetWidth() / (healthMax / AltInfoHero.HEALTH_PER_PIP)..'p')
	AltInfoHero.cur_healthSubPipContainer[unit]:SetVisible(showSubPips)
	if showSubPips then
		AltInfoHero.cur_healthSubPipContainer[unit]:SetUScale(AltInfoHero.cur_healthPipContainer[unit]:GetWidth() / (healthMax / AltInfoHero.HEALTH_PER_SUB_PIP)..'p')
	end
end

local function AltInfo_HeroLevel(unit, self, level)
	AltInfoHero.levelCompare[unit] = level
	AltInfoHero.cur_level_label[unit]:SetText(level)
	UpdateBackgroundFrame(unit)
end

local function AltInfo_HeroPlayer(unit, self, player, hero)
	if (NotEmpty(player)) then
		AltInfoHero.playerCompare[unit] = player
	else
		AltInfoHero.playerCompare[unit] = ''
	end

	AltInfoHero.heroName[unit] = hero or ''

	UpdateBackgroundFrame(unit)
end

local function AltInfo_HeroName(unit, self, name)
	if NotEmpty(name) then
		AltInfoHero.nameCompare[unit] = name
		AltInfoHero.isHovering[unit] = true
	else
		AltInfoHero.nameCompare[unit] = ''
		AltInfoHero.isHovering[unit] = false
	end
	UpdateBackgroundFrame(unit)
end

local function AltInfo_HeroHasMana(unit, self, hasMana)
	AltInfoHero.cur_mana_panel[unit]:SetVisible(AtoB(hasMana))
end

local function AltInfoHeroColorOld(unit, self, color, nameColor, type, colorScheme, playerIndex, selfDiffColor)
	local diff = AtoB(selfDiffColor)
	local lerpGreen = '0.7 0 0 1'
	local glowColorString = '0 1 0 1'

	local ownerType = AtoN(type)

	if (ownerType == 1) or (ownerType == 2) or (ownerType == 5) then -- self or ally
		glowColorString = (ownerType == 1 and diff) and 'yellow' or '0 1 0 1'
	elseif (ownerType == 3) then
		glowColorString = '1 0 0 1'
	else
		glowColorString = 'gray'
	end

	AltInfoHero.cur_health_lerp[unit]:SetColor(lerpGreen)
	AltInfoHero.player_label[unit]:SetColor(nameColor)
	AltInfoHero.cur_main_frame_img[unit]:SetColor(glowColorString)
end

local function AltInfo_HeroColor(unit, self, color, nameColor, type, colorScheme, playerIndex, selfDiffColor)
	local colorScheme = AtoN(colorScheme)

	AltInfoHero.cur_main_frame_img[unit]:SetVisible(colorScheme == 1)

	if not (colorScheme == 2 or colorScheme == 3) then
		AltInfoHeroColorOld(unit, self, color, nameColor, type, colorScheme, playerIndex, selfDiffColor)
		return
	end

	local glowGreen, glowRed, barGreen, barRed, barYellow, lerpGreen, lerpRed
	local ownerType = AtoN(type)

	if (colorScheme == 3) then
		glowGreen, glowRed = '1 1 0 1', '0 .3 1 1'
		barGreen, barRed, barYellow = '.88 .88 0 1', '0 .25 1 1', '.88 .88 0 1'
		lerpGreen, lerpRed = '0 .3 1 .9', '1 1 0 .9'
	else
		glowGreen, glowRed = '0 1 0 1', '1 0 0 1'
		barGreen, barRed, barYellow = '.08 .89 .08 1', '.9 .14 .14 1', '#ffb400'
		lerpGreen, lerpRed = '.7 0 0 1', '1 1 0 .7'
	end

	AltInfoHero.player_label_above[unit]:SetColor(nameColor)
	AltInfoHero.player_label[unit]:SetColor(nameColor)

	if (ownerType == 2) or (ownerType == 4) or (ownerType == 5) or ownerType == 6 then -- Ally or Ally you can control (cyan)
		AltInfoHero.cur_health_lerp[unit]:SetColor(lerpGreen)
		AltInfoHero.cur_health_bar[unit]:SetColor(barGreen)

		local borderColor = glowGreen
		if (ownerType == 5) then
			borderColor = nameColor
		end

		AltInfoHero.cur_main_frame_img[unit]:SetColor(borderColor)
	elseif (ownerType == 1) then -- Self
		if colorScheme == 3 and AtoB(selfDiffColor) then
			barYellow = '0 1 0 1'
		end
		AltInfoHero.cur_health_lerp[unit]:SetColor(lerpGreen)
		AltInfoHero.cur_health_bar[unit]:SetColor(barYellow)
		AltInfoHero.cur_main_frame_img[unit]:SetColor(barYellow)
	elseif (ownerType == 3) then --enemy
		AltInfoHero.cur_health_lerp[unit]:SetColor(lerpRed)
		AltInfoHero.cur_health_bar[unit]:SetColor(barRed)
		AltInfoHero.cur_main_frame_img[unit]:SetColor(glowRed)
	else
		AltInfoHero.cur_health_lerp[unit]:SetColor(color)
		AltInfoHero.cur_health_bar[unit]:SetColor(color)
		AltInfoHero.cur_main_frame_img[unit]:SetColor(glowRed)
	end
end

local function AltInfo_HeroDisplayBehavior(unit, self, behaviorName)
	-- behaviorName = "Follow"
	if (NotEmpty(behaviorName)) then
		AltInfoHero.cur_behavior_name[unit]:SetVisible(true)
		AltInfoHero.cur_behavior_name[unit]:SetText(behaviorName)
	else
		AltInfoHero.cur_behavior_name[unit]:SetVisible(false)
	end
end

local function AltInfo_HeroHasHealth(unit, self, hasHealth)
	if NotEmpty(hasHealth) then
		AltInfoHero.cur_main_frame[unit]:SetVisible(AtoB(hasHealth))
	end
end

local function AltInfoHeroHealthPercentOld(unit, self, healthPercent, colorScheme)
	AltInfoHero.cur_health_bar[unit]:SetWidth(ToPercent(healthPercent))
	AltInfoHero.cur_health_bar[unit]:SetColor(Saturate(1 - (healthPercent - 0.5) / 0.5), (healthPercent + ((healthPercent - 0.05) * 0.2)) / 0.45, 0, 1)
end

local function AltInfo_HeroHealthPercent(unit, self, healthPercent, colorScheme)
	local colorScheme = AtoN(colorScheme)
	if not (colorScheme == 2 or colorScheme == 3) then
		AltInfoHeroHealthPercentOld(unit, self, healthPercent, colorScheme)
		return
	end
	AltInfoHero.cur_health_bar[unit]:SetWidth(ToPercent(healthPercent))
end

local function AltInfo_HeroManaPercent(unit, self, manaPercent)
	AltInfoHero.cur_mana_bar[unit]:SetWidth(ToPercent(manaPercent))
end

local function AltInfo_HeroHealthLerp(unit, self, healthLerp)
	AltInfoHero.cur_health_lerp[unit]:SetWidth(ToPercent(healthLerp))
end

local function AltInfo_HeroHasShield(unit, self, hasShield)
	--hasShield = 'true'
	AltInfoHero.cur_shield_panel[unit]:SetVisible(AtoB(hasShield))
end

local function AltInfo_HeroShieldPercent(unit, self, shieldPercent)
	-- shieldPercent = '1'
	AltInfoHero.cur_shield_bar[unit]:SetWidth(ToPercent(shieldPercent))
end

local function AltInfo_HeroSpeaker(unit, self, isSpeaking)
	-- isSpeaking = 'true'
	AltInfoHero.speaker[unit]:SetVisible(AtoB(isSpeaking))
end

local function AltInfo_HeroAFK(unit, self, isAFK)
	-- isAFK = 'false'
	AltInfoHero.afk[unit]:SetVisible(AtoB(isAFK))
end

local function AltInfo_HeroNegativeEffects(unit, self, exists, iconPath, remainingTime, remainingPercent)
	AltInfoHero.state1[unit]:SetVisible(AtoB(exists))
	AltInfoHero.state1Icon[unit]:SetTexture(iconPath)

	local timeRemaining = tonumber(remainingTime)
	local percentRemaining = tonumber(remainingPercent)
	if percentRemaining > 0 then
		AltInfoHero.state1LabelBacker[unit]:SetVisible(true)
		AltInfoHero.state1BarContainer[unit]:SetVisible(true)
		AltInfoHero.state1Label[unit]:SetText(math.ceil(timeRemaining / 1000)..'s')
		AltInfoHero.state1Bar[unit]:SetWidth(ToPercent(percentRemaining))
	else
		AltInfoHero.state1LabelBacker[unit]:SetVisible(false)
		AltInfoHero.state1BarContainer[unit]:SetVisible(false)
	end
end

local function AltInfo_HeroLargeBar(unit, self, largeBar)
	chooseLargeWidgets(unit, AtoB(largeBar))
end

function AltInfoHero.Initialize(unit)
	SetupWidgets(unit)

	interface:RegisterWatch('AltInfo_HeroHealthCurrent'..unit, function(...) AltInfo_HeroHealthCurrent(unit, ...) end)
	interface:RegisterWatch('AltInfo_HeroColor'..unit, function(...) AltInfo_HeroColor(unit, ...) end)
	interface:RegisterWatch('AltInfo_HeroHealthPercent'..unit, function(...) AltInfo_HeroHealthPercent(unit, ...) end)
	interface:RegisterWatch('AltInfo_HeroName'..unit, function(...) AltInfo_HeroName(unit, ...) end)
	interface:RegisterWatch('AltInfo_HeroHasMana'..unit, function(...) AltInfo_HeroHasMana(unit, ...) end)
	interface:RegisterWatch('AltInfo_HeroHasHealth'..unit, function(...) AltInfo_HeroHasHealth(unit, ...) end)
	interface:RegisterWatch('AltInfo_HeroHasShield'..unit, function(...) AltInfo_HeroHasShield(unit, ...) end)
	interface:RegisterWatch('AltInfo_HeroManaPercent'..unit, function(...) AltInfo_HeroManaPercent(unit, ...) end)
	interface:RegisterWatch('AltInfo_HeroHealthLerp'..unit, function(...) AltInfo_HeroHealthLerp(unit, ...) end)
	interface:RegisterWatch('AltInfo_HeroPlayer'..unit, function(...) AltInfo_HeroPlayer(unit, ...) end)
	interface:RegisterWatch('AltInfo_HeroLevel'..unit, function(...) AltInfo_HeroLevel(unit, ...) end)
	interface:RegisterWatch('AltInfo_HeroShieldPercent'..unit, function(...) AltInfo_HeroShieldPercent(unit, ...) end)
	interface:RegisterWatch('AltInfo_HeroDisplayBehavior'..unit, function(...) AltInfo_HeroDisplayBehavior(unit, ...) end)
	interface:RegisterWatch('AltInfo_HeroSpeaker'..unit, function(...) AltInfo_HeroSpeaker(unit, ...) end)
	interface:RegisterWatch('AltInfo_HeroAFK'..unit, function(...) AltInfo_HeroAFK(unit, ...) end)
	interface:RegisterWatch('AltInfo_HeroNegativeEffects'..unit, function(...) AltInfo_HeroNegativeEffects(unit, ...) end)
	interface:RegisterWatch('AltInfo_HeroLargeBar'..unit, function(...) AltInfo_HeroLargeBar(unit, ...) end)
end
