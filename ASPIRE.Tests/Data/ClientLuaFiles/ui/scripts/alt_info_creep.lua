----------------------------------------------------------
--	Name: 		Alt Info Creep Script	       			--
-- 	Version: 	4.2										--
--  (C)2017 Garena Shanghai Games						--
----------------------------------------------------------

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()

RegisterScript2('AltInfoCreep', '1')

AltInfoCreep = _G['AltInfoCreep'] or {}

AltInfoCreep.levelCompare = {}
AltInfoCreep.isHovering = {}
AltInfoCreep.healthPercent = {}
AltInfoCreep.playerOwned = {}
AltInfoCreep.hasMana = {}

AltInfoCreep.healthHeight = GetCvarNumber('ui_altInfoHealthHeight')
AltInfoCreep.manaHeight = GetCvarNumber('ui_altInfoManaHeight')
AltInfoCreep.gap = GetCvarNumber('ui_altInfoGap')

AltInfoCreep.val = 1
AltInfoCreep.mul = 1.1
AltInfoCreep.add = 0.1

AltInfoCreep.stroke = 0.8
AltInfoCreep.strokeMul = 1
AltInfoCreep.strokeAdd = 0

AltInfoCreep.BGMul = 0.8
AltInfoCreep.BGAdd = 0

AltInfoCreep.background_frame 		= {}
AltInfoCreep.name_label 			= {}
AltInfoCreep.player_label 			= {}
AltInfoCreep.main_panel 			= {}
AltInfoCreep.exp 					= {}
AltInfoCreep.level_panel 			= {}
AltInfoCreep.level_bar 				= {}
AltInfoCreep.health_panel 			= {}
AltInfoCreep.health_lerp 			= {}
AltInfoCreep.health_bar 			= {}
AltInfoCreep.mana_panel 			= {}
AltInfoCreep.mana_bar 				= {}
AltInfoCreep.health_mana_panel 		= {}
AltInfoCreep.health_mana_panel_glow = {}
AltInfoCreep.main_panel_glow		= {}
AltInfoCreep.level_stroke			= {}
AltInfoCreep.health_stroke			= {}
AltInfoCreep.mana_stroke			= {}
AltInfoCreep.behavior_name			= {}

local function SetupWidgets(unit)
	AltInfoCreep.background_frame[unit] = interface:GetWidget('background_frame'..unit)
	AltInfoCreep.name_label[unit] = interface:GetWidget('name_label'..unit)
	AltInfoCreep.player_label[unit] = interface:GetWidget('player_label'..unit)
	AltInfoCreep.main_panel[unit] = interface:GetWidget('main_panel'..unit)
	AltInfoCreep.exp[unit] = interface:GetWidget('exp'..unit)
	AltInfoCreep.level_panel[unit] = interface:GetWidget('level_panel'..unit)
	AltInfoCreep.level_bar[unit] = interface:GetWidget('level_bar'..unit)
	AltInfoCreep.health_panel[unit] = interface:GetWidget('health_panel'..unit)
	AltInfoCreep.health_lerp[unit] = interface:GetWidget('health_lerp'..unit)
	AltInfoCreep.health_bar[unit] = interface:GetWidget('health_bar'..unit)
	AltInfoCreep.mana_panel[unit] = interface:GetWidget('mana_panel'..unit)
	AltInfoCreep.mana_bar[unit] 	= interface:GetWidget('mana_bar'..unit)
	AltInfoCreep.health_mana_panel[unit] 		= interface:GetWidget('health_mana_panel'..unit)
	AltInfoCreep.health_mana_panel_glow[unit] 	= interface:GetWidget('health_mana_panel_glow'..unit)
	AltInfoCreep.main_panel_glow[unit]		 	= interface:GetWidget('main_panel_glow'..unit)
	AltInfoCreep.level_stroke[unit]			 	= interface:GetWidget('level_stroke'..unit)
	AltInfoCreep.health_stroke[unit]			 	= interface:GetWidget('health_stroke'..unit)
	AltInfoCreep.mana_stroke[unit]			 	= interface:GetWidget('mana_stroke'..unit)
	AltInfoCreep.behavior_name[unit]			 	= interface:GetWidget('behavior_name'..unit)

	AltInfoCreep.level_stroke[unit]:SetBorderAlpha(AltInfoCreep.stroke)
	AltInfoCreep.health_stroke[unit]:SetBorderAlpha(AltInfoCreep.stroke)
	AltInfoCreep.mana_stroke[unit]:SetBorderAlpha(AltInfoCreep.stroke)
	AltInfoCreep.healthPercent[unit] = 0
	AltInfoCreep.playerOwned[unit] = false
	AltInfoCreep.isHovering[unit] = false
	AltInfoCreep.levelCompare[unit] = 0
	AltInfoCreep.hasMana[unit] = false
end

local function CalculatePanelAlpha(unit, mul, add)
	local alpha = (1-tonumber(AltInfoCreep.healthPercent[unit]))*mul + add
	if (alpha > 1) then
		alpha = 1
	elseif (alpha < 0) then
		alpha = 0
	end
	return alpha
end

local function SetStrokeAlpha(unit, alpha)
	AltInfoCreep.level_stroke[unit]:SetBorderAlpha(alpha)
	AltInfoCreep.health_stroke[unit]:SetBorderAlpha(alpha)
	AltInfoCreep.mana_stroke[unit]:SetBorderAlpha(alpha)
end

local function SetPanelAlpha(unit, barAlpha, bgAlpha)
	AltInfoCreep.health_lerp[unit]:SetAlpha(barAlpha)
	AltInfoCreep.health_bar[unit]:SetAlpha(barAlpha)
	AltInfoCreep.mana_bar[unit]:SetAlpha(barAlpha)

	AltInfoCreep.level_panel[unit]:SetAlpha(bgAlpha)
	AltInfoCreep.health_panel[unit]:SetAlpha(bgAlpha)
	AltInfoCreep.mana_panel[unit]:SetAlpha(bgAlpha)
end

local function UpdateBackgroundFrame(unit)
	local altInfoAlpha = GetCvarBool('cg_altInfoAlpha')

	if (AltInfoCreep.playerOwned[unit]) then
		SetPanelAlpha(unit, 1, AltInfoCreep.val)
	else
		AltInfoCreep.background_frame[unit]:SetVisible(false)

		if (altInfoAlpha) then
			SetStrokeAlpha(unit, CalculatePanelAlpha(unit, AltInfoCreep.strokeMul, AltInfoCreep.strokeAdd))
			if (AltInfoCreep.isHovering[unit]) then
				SetPanelAlpha(unit, 1, CalculatePanelAlpha(unit, AltInfoCreep.BGMul, AltInfoCreep.BGAdd))
			else
				SetPanelAlpha(unit, CalculatePanelAlpha(unit, AltInfoCreep.mul, AltInfoCreep.add), CalculatePanelAlpha(unit, AltInfoCreep.BGMul, AltInfoCreep.BGAdd))
			end
		else
			SetPanelAlpha(unit, 1, AltInfoCreep.val)
			SetStrokeAlpha(unit, AltInfoCreep.stroke)
		end
	end

	if (AltInfoCreep.levelCompare[unit] > 0) then

		AltInfoCreep.main_panel_glow[unit]:SetVisible(AltInfoCreep.isHovering[unit])
		AltInfoCreep.health_mana_panel_glow[unit]:SetVisible(false)

		AltInfoCreep.level_panel[unit]:SetHeight(tostring(AltInfoCreep.manaHeight) .. 'h')

		AltInfoCreep.health_mana_panel[unit]:SetY(tostring(AltInfoCreep.manaHeight + AltInfoCreep.gap) .. 'h')

		AltInfoCreep.health_panel[unit]:SetHeight(tostring(AltInfoCreep.healthHeight) .. 'h')

		if (AltInfoCreep.hasMana[unit]) then
			AltInfoCreep.mana_panel[unit]:SetY(tostring(AltInfoCreep.healthHeight + AltInfoCreep.gap) .. 'h')
			AltInfoCreep.mana_panel[unit]:SetHeight(tostring(AltInfoCreep.manaHeight) .. 'h')
			AltInfoCreep.main_panel[unit]:SetHeight(tostring(AltInfoCreep.manaHeight * 2 + AltInfoCreep.healthHeight + AltInfoCreep.gap * 2) .. 'h')
			AltInfoCreep.health_mana_panel[unit]:SetHeight(tostring(AltInfoCreep.healthHeight + AltInfoCreep.gap + AltInfoCreep.manaHeight) .. 'h')
		else
			AltInfoCreep.health_mana_panel[unit]:SetHeight(tostring(AltInfoCreep.healthHeight) .. 'h')
			AltInfoCreep.main_panel[unit]:SetHeight(tostring(AltInfoCreep.manaHeight + AltInfoCreep.healthHeight + AltInfoCreep.gap) .. 'h')
		end
	else
		AltInfoCreep.main_panel_glow[unit]:SetVisible(false)
		AltInfoCreep.health_mana_panel_glow[unit]:SetVisible(AltInfoCreep.isHovering[unit])

		AltInfoCreep.health_panel[unit]:SetHeight(tostring(AltInfoCreep.healthHeight) .. 'h')

		if (AltInfoCreep.hasMana[unit]) then
			AltInfoCreep.mana_panel[unit]:SetY(tostring(AltInfoCreep.healthHeight + AltInfoCreep.gap) .. 'h')
			AltInfoCreep.mana_panel[unit]:SetHeight(tostring(AltInfoCreep.manaHeight) .. 'h')
			AltInfoCreep.health_mana_panel[unit]:SetHeight(tostring(AltInfoCreep.healthHeight + AltInfoCreep.gap + AltInfoCreep.manaHeight) .. 'h')

			AltInfoCreep.health_mana_panel_glow[unit]:SetY('0')
			AltInfoCreep.health_mana_panel_glow[unit]:SetHeight('+4')
		else
			AltInfoCreep.health_mana_panel[unit]:SetHeight(tostring(AltInfoCreep.healthHeight) .. 'h')

			AltInfoCreep.health_mana_panel_glow[unit]:SetY('0')
			AltInfoCreep.health_mana_panel_glow[unit]:SetHeight('+4')
		end
	end
end

local function AltInfo_CreepName(unit, self, name)
	if (NotEmpty(name)) then
		AltInfoCreep.isHovering[unit] = true
		AltInfoCreep.name_label[unit]:SetText(name)
	else
		AltInfoCreep.isHovering[unit] = false
	end
	AltInfoCreep.background_frame[unit]:SetVisible(AltInfoCreep.isHovering[unit])

	UpdateBackgroundFrame(unit)
end

local function AltInfo_CreepHasMana(unit, self, hasMana)
	AltInfoCreep.hasMana[unit] = AtoB(hasMana)
	AltInfoCreep.mana_panel[unit]:SetVisible(AltInfoCreep.hasMana[unit])

	UpdateBackgroundFrame(unit)
end

local function AltInfoCreepColorOld(unit, self, color, nameColor, type, colorScheme)
	local glowColorString = '0 1 0 1'
	local lerpGreen = '0.7 0 0 1'

	local ownerType = AtoN(type)

	if (ownerType == 1) or (ownerType == 2) or (ownerType == 4) or (ownerType == 5) then -- Self or Ally
		glowColorString = '0 1 0 1'
	elseif ownerType == 3 then --enemy
		glowColorString = '1 0 0 1'
	end

	AltInfoCreep.health_lerp[unit]:SetColor(lerpGreen)
	AltInfoCreep.player_label[unit]:SetColor(nameColor)
	AltInfoCreep.health_mana_panel_glow[unit]:SetBorderColor(color)
	AltInfoCreep.main_panel_glow[unit]:SetBorderColor(color)
end

local function AltInfo_CreepColor(unit, self, color, nameColor, type, colorScheme)
	local colorScheme = AtoN(colorScheme)
	if not (colorScheme == 2 or colorScheme == 3) then
		AltInfoCreepColorOld(unit, self, color, nameColor, type, colorScheme)
		return
	end

	local glowGreen, glowRed, barGreen, barRed, lerpGreen, lerpRed

	if (colorScheme == 3) then
		glowGreen, glowRed = '1 1 0 1', '0 0.3 1 1'
		barGreen, barRed = '.88 .88 0 1', '0 0.25 1 1'
		lerpGreen, lerpRed = '0 0.3 1 0.9', '1 1 0 0.9'
	else
		glowGreen, glowRed = '0 1 0 1', '1 0 0 1'
		barGreen, barRed = '0 .78 0 1', '0.8 0.15 0.15 1'
		lerpGreen, lerpRed = '0.7 0 0 1', '1.0 1.0 0 0.7'
	end

	local ownerType = AtoN(type)

	if (ownerType == 1) or (ownerType == 2) or (ownerType == 4) or (ownerType == 5) then -- Self or Ally
		AltInfoCreep.health_lerp[unit]:SetColor(lerpGreen)
		AltInfoCreep.health_bar[unit]:SetColor(barGreen)
		AltInfoCreep.player_label[unit]:SetColor(barGreen)
		AltInfoCreep.health_mana_panel_glow[unit]:SetBorderColor(glowGreen)
		AltInfoCreep.main_panel_glow[unit]:SetBorderColor(glowGreen)
	elseif ownerType == 3 then
		AltInfoCreep.health_lerp[unit]:SetColor(lerpRed)
		AltInfoCreep.health_bar[unit]:SetColor(barRed)
		AltInfoCreep.player_label[unit]:SetColor(barRed)
		AltInfoCreep.health_mana_panel_glow[unit]:SetBorderColor(glowRed)
		AltInfoCreep.main_panel_glow[unit]:SetBorderColor(glowRed)
	else
		AltInfoCreep.health_lerp[unit]:SetColor(color)
		AltInfoCreep.health_bar[unit]:SetColor(color)
		AltInfoCreep.player_label[unit]:SetColor(color)
		AltInfoCreep.health_mana_panel_glow[unit]:SetBorderColor(color)
		AltInfoCreep.main_panel_glow[unit]:SetBorderColor(color)
	end

	UpdateBackgroundFrame(unit)
end

local function AltInfo_CreepHasHealth(unit, self, hasHealth)
	AltInfoCreep.main_panel[unit]:SetVisible(AtoB(hasHealth))
end

local function AltInfoCreepHealthPercentOld(unit, self, healthPercent, colorScheme)
	AltInfoCreep.health_bar[unit]:SetWidth(ToPercent(healthPercent))
	AltInfoCreep.health_bar[unit]:SetColor(Saturate(1 - (healthPercent - 0.5) / 0.5), (healthPercent + ((healthPercent - 0.05) * 0.2)) / 0.45, 0, 1)
end

local function AltInfo_CreepHealthPercent(unit, self, healthPercent, colorScheme)
	local colorScheme = AtoN(colorScheme)
	if not (colorScheme == 2 or colorScheme == 3) then
		AltInfoCreepHealthPercentOld(unit, self, healthPercent, colorScheme)
		return
	end

	AltInfoCreep.healthPercent[unit] = tonumber(healthPercent)
	AltInfoCreep.health_bar[unit]:SetWidth(ToPercent(healthPercent))

	UpdateBackgroundFrame(unit)
end

local function AltInfo_CreepManaPercent(unit, self, manaPercent)
	AltInfoCreep.mana_bar[unit]:SetWidth(ToPercent(manaPercent))

	UpdateBackgroundFrame(unit)
end

local function AltInfo_CreepHealthLerp(unit, self, healthLerp)
	AltInfoCreep.health_lerp[unit]:SetWidth(ToPercent(healthLerp))

	UpdateBackgroundFrame(unit)
end

local function AltInfo_CreepIsInExpRange(unit, self, isInExpRange)
	AltInfoCreep.exp[unit]:SetVisible(AtoB(isInExpRange))
end

local function AltInfo_CreepLevel(unit, self, level)
	AltInfoCreep.levelCompare[unit] = tonumber(level) or 0

	if (AltInfoCreep.levelCompare[unit] > 0) then
		AltInfoCreep.level_panel[unit]:SetVisible(true)
		AltInfoCreep.level_bar[unit]:SetWidth(ToPercent(AltInfoCreep.levelCompare[unit])..'%')
	else
		AltInfoCreep.level_panel[unit]:SetVisible(false)
	end

	UpdateBackgroundFrame(unit)
end

local function AltInfo_CreepPlayer(unit, self, player)
	if (NotEmpty(player)) then
		AltInfoCreep.player_label[unit]:SetText(player)
	end
end

local function AltInfo_CreepPlayerOwned(unit, self, owned)
	AltInfoCreep.playerOwned[unit] = AtoB(owned)

	UpdateBackgroundFrame(unit)
end

local function AltInfo_CreepDisplayBehavior(unit, self, behaviorName)
	if (NotEmpty(behaviorName)) then
		AltInfoCreep.behavior_name[unit]:SetVisible(true)
		AltInfoCreep.behavior_name[unit]:SetText(behaviorName)
	else
		AltInfoCreep.behavior_name[unit]:SetVisible(false)
	end
end

function AltInfoCreep.Initialize(unit)
	SetupWidgets(unit)

	interface:RegisterWatch('AltInfo_CreepColor'..unit, function(...) AltInfo_CreepColor(unit, ...) end)
	interface:RegisterWatch('AltInfo_CreepHealthPercent'..unit, function(...) AltInfo_CreepHealthPercent(unit, ...) end)
	interface:RegisterWatch('AltInfo_CreepPlayer'..unit, function(...) AltInfo_CreepPlayer(unit, ...) end)
	interface:RegisterWatch('AltInfo_CreepLevel'..unit, function(...) AltInfo_CreepLevel(unit, ...) end)
	interface:RegisterWatch('AltInfo_CreepName'..unit, function(...) AltInfo_CreepName(unit, ...) end)
	interface:RegisterWatch('AltInfo_CreepHasMana'..unit, function(...) AltInfo_CreepHasMana(unit, ...) end)
	interface:RegisterWatch('AltInfo_CreepHasHealth'..unit, function(...) AltInfo_CreepHasHealth(unit, ...) end)
	interface:RegisterWatch('AltInfo_CreepManaPercent'..unit, function(...) AltInfo_CreepManaPercent(unit, ...) end)
	interface:RegisterWatch('AltInfo_CreepHealthLerp'..unit, function(...) AltInfo_CreepHealthLerp(unit, ...) end)
	interface:RegisterWatch('AltInfo_CreepIsInExpRange'..unit, function(...) AltInfo_CreepIsInExpRange(unit, ...) end)
	interface:RegisterWatch('AltInfo_CreepPlayerOwned'..unit, function(...) AltInfo_CreepPlayerOwned(unit, ...) end)
	interface:RegisterWatch('AltInfo_CreepDisplayBehavior'..unit, function(...) AltInfo_CreepDisplayBehavior(unit, ...) end)
end
