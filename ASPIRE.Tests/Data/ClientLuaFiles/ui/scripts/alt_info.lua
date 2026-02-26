----------------------------------------------------------
--	Name: 		Alt Info Script	            			--
-- 	Version: 	4.2										--
--  (C)2017 Garena Shanghai Games						--
----------------------------------------------------------

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()

AltInfo = _G['AltInfo'] or {}

AltInfo.level_panel = {}
AltInfo.level_label = {}
AltInfo.nameCompare = {}
AltInfo.isHovering = {}
AltInfo.hasMana = {}

AltInfo.healthHeight = GetCvarNumber('ui_altInfoHealthHeight')
AltInfo.manaHeight = GetCvarNumber('ui_altInfoManaHeight')
AltInfo.gap = GetCvarNumber('ui_altInfoGap')

AltInfo.background_frame 		= {}
AltInfo.name_label 				= {}
AltInfo.main_panel 				= {}
AltInfo.exp 					= {}
AltInfo.health_lerp 			= {}
AltInfo.health_bar 				= {}
AltInfo.mana_panel 				= {}
AltInfo.mana_bar 				= {}
AltInfo.health_mana_panel_glow 	= {}
AltInfo.health_mana_panel 		= {}
AltInfo.health_panel 	        = {}
AltInfo.inventory_panel 		= {}
AltInfo.behavior_name			= {}

local function SetupWidgets(unit)
	AltInfo.level_panel[unit] 				= interface:GetWidget('level_panel'..unit)
	AltInfo.level_label[unit] 				= interface:GetWidget('level_label'..unit)
	AltInfo.background_frame[unit] 			= interface:GetWidget('background_frame'..unit)
	AltInfo.name_label[unit] 				= interface:GetWidget('name_label'..unit)
	AltInfo.main_panel[unit] 				= interface:GetWidget('main_panel'..unit)
	AltInfo.exp[unit] 						= interface:GetWidget('exp'..unit)
	AltInfo.health_lerp[unit] 				= interface:GetWidget('health_lerp'..unit)
	AltInfo.health_bar[unit] 				= interface:GetWidget('health_bar'..unit)
	AltInfo.mana_panel[unit] 				= interface:GetWidget('mana_panel'..unit)
	AltInfo.mana_bar[unit] 					= interface:GetWidget('mana_bar'..unit)
	AltInfo.health_mana_panel_glow[unit] 	= interface:GetWidget('health_mana_panel_glow'..unit)
	AltInfo.health_mana_panel[unit] 		= interface:GetWidget('health_mana_panel'..unit)
	AltInfo.health_panel[unit] 	        	= interface:GetWidget('health_panel'..unit)
	AltInfo.inventory_panel[unit] 			= interface:GetWidget('inventory_panel'..unit)
	AltInfo.behavior_name[unit]			 	= interface:GetWidget('behavior_name'..unit)

	AltInfo.nameCompare[unit] = ''
	AltInfo.isHovering[unit] = false
	AltInfo.hasMana[unit] = false
end

local function UpdateBackgroundFrame(unit)
	if (AltInfo.nameCompare[unit]) and (AltInfo.isHovering[unit]) then
		AltInfo.background_frame[unit]:SetVisible(true)
		AltInfo.background_frame[unit]:SetX((GetStringWidth('dyn_11', AltInfo.nameCompare[unit])    + AltInfo.background_frame[unit]:GetXFromString('1.2h')) * -0.5)
		AltInfo.background_frame[unit]:SetWidth(GetStringWidth('dyn_11', AltInfo.nameCompare[unit]) + AltInfo.background_frame[unit]:GetXFromString('1.2h'))
		AltInfo.name_label[unit]:SetWidth('100%')
		AltInfo.name_label[unit]:SetText(AltInfo.nameCompare[unit])
	else
		AltInfo.background_frame[unit]:SetVisible(false)
	end

	AltInfo.health_mana_panel_glow[unit]:SetVisible(AltInfo.isHovering[unit])

	AltInfo.health_panel[unit]:SetHeight(tostring(AltInfo.healthHeight) .. 'h')

	if (AltInfo.hasMana[unit]) then
		AltInfo.mana_panel[unit]:SetY(tostring(AltInfo.healthHeight + AltInfo.gap) .. 'h')
		AltInfo.mana_panel[unit]:SetHeight(tostring(AltInfo.manaHeight) .. 'h')
		AltInfo.health_mana_panel[unit]:SetHeight(tostring(AltInfo.healthHeight + AltInfo.gap + AltInfo.manaHeight) .. 'h')
		AltInfo.health_mana_panel_glow[unit]:SetY('0')
		AltInfo.health_mana_panel_glow[unit]:SetHeight('+4')
	else
		AltInfo.health_mana_panel[unit]:SetHeight(tostring(AltInfo.healthHeight) .. 'h')
		AltInfo.health_mana_panel_glow[unit]:SetY('0')
		AltInfo.health_mana_panel_glow[unit]:SetHeight('+4')
	end
end

local function AltInfo_GenericName(unit, self, name)
	if (NotEmpty(name)) then
		AltInfo.nameCompare[unit] = name
		AltInfo.isHovering[unit] = true
	else
		AltInfo.nameCompare[unit] = ''
		AltInfo.isHovering[unit] = false
	end
	AltInfo.health_mana_panel_glow[unit]:SetVisible(AltInfo.isHovering[unit])

	UpdateBackgroundFrame(unit)
end

local function AltInfo_GenericHasMana(unit, self, hasMana)
	AltInfo.hasMana[unit] = AtoB(hasMana)
	AltInfo.mana_panel[unit]:SetVisible(AltInfo.hasMana[unit])

	UpdateBackgroundFrame(unit)
end

local function AltInfoGenericColorOld(unit, self, color, nameColor, type, colorScheme)
	-- Echo('AltInfoGenericColorOld unit:'..unit..' ownerType:'..type..' color:'..color..' colorScheme:'..colorScheme)
	local ownerType = AtoN(type)

	local glowColorString = '0 1 0 1'
	local lerpGreen = '.7 0 0 1'

	if (ownerType == 1) or (ownerType == 2) or (ownerType == 4) or (ownerType == 5) then -- Self or Ally
		glowColorString = 'lime'
	elseif (ownerType == 3) then --enemy
		glowColorString = 'red'
	end

	AltInfo.health_mana_panel_glow[unit]:SetBorderColor(glowColorString)
	AltInfo.health_lerp[unit]:SetColor(lerpGreen)
	AltInfo.health_bar[unit]:SetColor(color)
end

local function AltInfo_GenericColor(unit, self, color, nameColor, type, colorScheme)
	local colorScheme = AtoN(colorScheme)

	if not (colorScheme == 2 or colorScheme == 3) then
		AltInfoGenericColorOld(unit, self, color, nameColor, type, colorScheme)
		return
	end

	local glowGreen, glowRed, barGreen, barRed, lerpGreen, lerpRed

	if (colorScheme == 3) then
		glowGreen, glowRed = '1 1 0 1', '0 .3 1 1'
		barGreen, barRed = '.88 .88 0 1', '0 .25 1 1'
		lerpGreen, lerpRed = '0 .3 1 .9', '1 1 0 .9'
	else
		glowGreen, glowRed =  '0 1 0 1', '1 0 0 1'
		barGreen, barRed = '#3aa400', '#e70000'
		lerpGreen, lerpRed = '.7 0 0 1', '1 1 0 .7'
	end

	local ownerType = AtoN(type)

	if (ownerType == 1) or (ownerType == 2) or (ownerType == 4) or (ownerType == 5) then -- Self or Ally
		AltInfo.health_mana_panel_glow[unit]:SetBorderColor(glowGreen)
		AltInfo.health_lerp[unit]:SetColor(lerpGreen)
		AltInfo.health_bar[unit]:SetColor(barGreen)
	elseif ownerType == 3 then
		AltInfo.health_mana_panel_glow[unit]:SetBorderColor(glowRed)
		AltInfo.health_lerp[unit]:SetColor(lerpRed)
		AltInfo.health_bar[unit]:SetColor(barRed)
	else
		AltInfo.health_mana_panel_glow[unit]:SetBorderColor(color)
		AltInfo.health_lerp[unit]:SetColor(color)
		AltInfo.health_bar[unit]:SetColor(color)
	end
end

local function AltInfo_GenericHasHealth(unit, self, hasHealth)
	AltInfo.main_panel[unit]:SetVisible(AtoB(hasHealth))
end

local function AltInfoGenericHealthPercentOld(unit, self, healthPercent, colorScheme)
	AltInfo.health_bar[unit]:SetWidth(ToPercent(healthPercent))

	local r, g = Saturate(1 - (healthPercent - 0.5) / 0.5), Saturate((healthPercent + ((healthPercent - 0.05) * 0.2)) / 0.45)
	-- Echo('AltInfoGenericHealthPercentOld unit:'..unit..' healthPercent:'..healthPercent..' r:'..r..' g:'..g)

	AltInfo.health_bar[unit]:SetColor(r, g, 0, 1)
end

local function AltInfo_GenericHealthPercent(unit, self, healthPercent, colorScheme)
	local colorScheme = AtoN(colorScheme)
	if not (colorScheme == 2 or colorScheme == 3) then
		AltInfoGenericHealthPercentOld(unit, self, healthPercent, colorScheme)
		return
	end
	AltInfo.health_bar[unit]:SetWidth(ToPercent(healthPercent))
end

local function AltInfo_GenericManaPercent(unit, self, manaPercent)
	AltInfo.mana_bar[unit]:SetWidth(ToPercent(manaPercent))
end

local function AltInfo_GenericHealthLerp(unit, self, healthLerp)
	AltInfo.health_lerp[unit]:SetWidth(ToPercent(healthLerp))
end

local function AltInfo_GenericIsInExpRange(unit, self, isInExpRange)
	AltInfo.exp[unit]:SetVisible(AtoB(isInExpRange))
end

local function AltInfo_GenericShowInventory(unit, self, show)
	AltInfo.inventory_panel[unit]:SetVisible(AtoB(show))
end

local function AltInfo_GenericLevel(unit, self, level)
	if (tonumber(level) and tonumber(level) > 0) then
		AltInfo.level_panel[unit]:SetVisible(true)
		AltInfo.level_label[unit]:SetText(level)
	else
		AltInfo.level_panel[unit]:SetVisible(false)
	end
	UpdateBackgroundFrame(unit)
end

local function AltInfo_GenericDisplayBehavior(unit, self, behaviorName)
	if (NotEmpty(behaviorName)) then
		AltInfo.behavior_name[unit]:SetVisible(true)
		AltInfo.behavior_name[unit]:SetText(behaviorName)
	else
		AltInfo.behavior_name[unit]:SetVisible(false)
	end
end

function AltInfo.Initialize(unit)
	SetupWidgets(unit)

	interface:RegisterWatch('AltInfo_GenericColor'..unit, function(...) AltInfo_GenericColor(unit, ...) end)
	interface:RegisterWatch('AltInfo_GenericHealthPercent'..unit, function(...) AltInfo_GenericHealthPercent(unit, ...) end)
	interface:RegisterWatch('AltInfo_GenericName'..unit, function(...) AltInfo_GenericName(unit, ...) end)
	interface:RegisterWatch('AltInfo_GenericHasMana'..unit, function(...) AltInfo_GenericHasMana(unit, ...) end)
	interface:RegisterWatch('AltInfo_GenericHasHealth'..unit, function(...) AltInfo_GenericHasHealth(unit, ...) end)
	interface:RegisterWatch('AltInfo_GenericManaPercent'..unit, function(...) AltInfo_GenericManaPercent(unit, ...) end)
	interface:RegisterWatch('AltInfo_GenericHealthLerp'..unit, function(...) AltInfo_GenericHealthLerp(unit, ...) end)
	interface:RegisterWatch('AltInfo_GenericIsInExpRange'..unit, function(...) AltInfo_GenericIsInExpRange(unit, ...) end)
	interface:RegisterWatch('AltInfo_GenericShowInventory'..unit, function(...) AltInfo_GenericShowInventory(unit, ...) end)
	interface:RegisterWatch('AltInfo_GenericLevel'..unit, function(...) AltInfo_GenericLevel(unit, ...) end)
	interface:RegisterWatch('AltInfo_GenericDisplayBehavior'..unit, function(...) AltInfo_GenericDisplayBehavior(unit, ...) end)
end
