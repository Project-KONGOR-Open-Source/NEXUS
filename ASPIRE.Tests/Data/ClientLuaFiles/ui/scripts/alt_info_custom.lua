----------------------------------------------------------
--	Name: 		Alt Info Script	            			--
-- 	Version: 	4.2										--
--  (C)2017 Garena Shanghai Games						--
----------------------------------------------------------

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort, ToPercent = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, ToPercent
local interface, interfaceName = object, object:GetName()

RegisterScript2('AltInfoCustom', '1')

AltInfoCustom = _G[AltInfoCustom] or {}

--'', 0, false, false, false, false
AltInfoCustom.nameCompare = {}
AltInfoCustom.levelCompare = {}
AltInfoCustom.isHovering = {}
AltInfoCustom.showInventory = {}
AltInfoCustom.hasHealth = {}

AltInfoCustom.HOVER_GLOW = true

AltInfoCustom.background_frame 		= {}
AltInfoCustom.border_frame	 		= {}
AltInfoCustom.name_label 			= {}
AltInfoCustom.main_panel 			= {}
AltInfoCustom.level_panel 			= {}
AltInfoCustom.level_pie 			= {}
AltInfoCustom.exp 					= {}
AltInfoCustom.health_lerp 			= {}
AltInfoCustom.health_bar 			= {}
AltInfoCustom.mana_panel 			= {}
AltInfoCustom.mana_bar 				= {}
AltInfoCustom.health_bar_glow 		= {}
AltInfoCustom.creep_plate_parent	= {}
AltInfoCustom.health_bar_parent		= {}
AltInfoCustom.inventory 			= {}

local function SetupWidgets(unit)
	AltInfoCustom.background_frame[unit] = interface:GetWidget('background_frame'..unit)
	AltInfoCustom.border_frame[unit] = interface:GetWidget('border_frame'..unit)
	AltInfoCustom.name_label[unit] = interface:GetWidget('name_label'..unit)
	AltInfoCustom.main_panel[unit] = interface:GetWidget('main_panel'..unit)
	AltInfoCustom.level_panel[unit] = interface:GetWidget('level_panel'..unit)
	AltInfoCustom.level_pie[unit] = interface:GetWidget('level_pie'..unit)
	AltInfoCustom.exp[unit] = interface:GetWidget('exp'..unit)
	AltInfoCustom.health_lerp[unit] = interface:GetWidget('health_lerp'..unit)
	AltInfoCustom.health_bar[unit] = interface:GetWidget('health_bar'..unit)
	AltInfoCustom.mana_panel[unit] = interface:GetWidget('mana_panel'..unit)
	AltInfoCustom.mana_bar[unit] = interface:GetWidget('mana_bar'..unit)
	AltInfoCustom.health_bar_glow[unit] = interface:GetWidget('health_bar_glow'..unit)
	AltInfoCustom.creep_plate_parent[unit] = interface:GetWidget('creep_plate_parent'..unit)
	AltInfoCustom.health_bar_parent[unit] = interface:GetWidget('health_bar_parent'..unit)
	AltInfoCustom.inventory[unit] = interface:GetWidget('inventory_container'..unit)

	AltInfoCustom.levelCompare[unit] = 0
	AltInfoCustom.isHovering[unit] = false
	AltInfoCustom.showInventory[unit] = false
	AltInfoCustom.hasHealth[unit] = false
end

local function UpdateBackgroundFrame(unit)
	if (AltInfoCustom.nameCompare[unit]) and (AltInfoCustom.isHovering[unit]) then
		AltInfoCustom.background_frame[unit]:SetVisible(true)

		local baseWidth = GetStringWidth('dyn_10', AltInfoCustom.nameCompare[unit])
		if (AltInfoCustom.showInventory[unit]) then
			baseWidth = math.max(baseWidth, AltInfoCustom.inventory[unit]:GetWidth())
		end
		local frameWidth = baseWidth + AltInfoCustom.background_frame[unit]:GetXFromString('1.2h')

		AltInfoCustom.background_frame[unit]:SetX(frameWidth * -0.5)
		AltInfoCustom.background_frame[unit]:SetWidth(frameWidth)

		if (AltInfoCustom.showInventory[unit]) then
			AltInfoCustom.background_frame[unit]:SetY('-6.5h')
			AltInfoCustom.background_frame[unit]:SetHeight("4.6h")
		else
			AltInfoCustom.background_frame[unit]:SetY('-4.7h')
			AltInfoCustom.background_frame[unit]:SetHeight("2.3h")
		end

		AltInfoCustom.border_frame[unit]:SetWidth('100%')
		AltInfoCustom.name_label[unit]:SetWidth('100%')
		AltInfoCustom.name_label[unit]:SetText(AltInfoCustom.nameCompare[unit])
	else
		AltInfoCustom.background_frame[unit]:SetVisible(false)
	end
end

local function AltInfoCustomColorOld(unit, self, color, nameColor, type, colorScheme)
	local glowColorString = '0 1 0 1'
	local lerpGreen = 	'0.7 0 0 1'
	local capGreen = 	'0 0.78 0 1'

	local ownerType = AtoN(type)
	if (ownerType == 1) or (ownerType == 2) or (ownerType == 4) or (ownerType == 5) then -- Self or Ally
		glowColorString = '0 1 0 1'
	elseif ownerType == 3 then --enemy
		glowColorString = '1 0 0 1'
	end

	AltInfoCustom.health_lerp[unit]:SetColor(lerpGreen)
	AltInfoCustom.border_frame[unit]:SetBorderColor(color)
	AltInfoCustom.health_bar_glow[unit]:SetBorderColor(glowColorString)
end

local function AltInfo_CustomColor(unit, self, color, nameColor, type, colorScheme)
	local ownerType = AtoN(type)
	local colorScheme = AtoN(colorScheme)
	if not (colorScheme == 2 or colorScheme == 3) then
		AltInfoCustomColorOld(unit, self, color, nameColor, type, colorScheme)
		return
	end
	local glowGreen, glowRed, barGreen, barRed, lerpGreen, lerpRed, bgGreen, bgRed

	if (colorScheme == 3) then
		glowGreen, glowRed = 	'1 1 0 1', 			'0 0.3 1 1'
		barGreen, barRed = 		'.88 .88 0 1', 		'0 0.25 1 1'
		lerpGreen, lerpRed = 	'0 0.3 1 0.9', 		'1 1 0 0.9'
		bgGreen, bgRed = glowGreen, glowRed
	else
		glowGreen, glowRed = 	'0 1 0 1', 			'1 0 0 1'
		barGreen, barRed = 		'0 .78 0 1', 		'0.8 0.15 0.15 1'
		lerpGreen, lerpRed = 	'0.7 0 0 1', 		'1.0 1.0 0 0.7'
		bgGreen, bgRed = glowGreen, glowRed
	end

	if (ownerType == 1) or (ownerType == 2) or (ownerType == 4) or (ownerType == 5) then -- Self or Ally
		AltInfoCustom.health_bar_glow[unit]:SetBorderColor(glowGreen)
		AltInfoCustom.health_lerp[unit]:SetColor(lerpGreen)
		AltInfoCustom.health_bar[unit]:SetColor(barGreen)
		AltInfoCustom.border_frame[unit]:SetBorderColor(bgGreen)
	elseif ownerType == 3 then --enemy
		AltInfoCustom.health_bar_glow[unit]:SetBorderColor(glowRed)
		AltInfoCustom.health_lerp[unit]:SetColor(lerpRed)
		AltInfoCustom.health_bar[unit]:SetColor(barRed)
		AltInfoCustom.border_frame[unit]:SetBorderColor(bgRed)
	else
		AltInfoCustom.health_bar[unit]:SetColor(color)
		AltInfoCustom.health_lerp[unit]:SetColor(color)
		AltInfoCustom.border_frame[unit]:SetBorderColor(color)
		AltInfoCustom.health_bar_glow[unit]:SetBorderColor(color)
	end
end

local function AltInfo4HealthPercentOld(self, healthPercent, colorScheme)
	AltInfoCustom.health_bar[unit]:SetWidth(ToPercent(healthPercent))
	AltInfoCustom.health_bar[unit]:SetColor(Saturate(1 - (healthPercent - 0.50) / 0.50), (healthPercent + (((healthPercent - 0.05) / 1.0) * 0.2)) / 0.45, 0, 1)
end

local function AltInfo_CustomHealthPercent(unit, self, healthPercent, colorScheme)
	local colorScheme = AtoN(colorScheme)
	if not (colorScheme == 2 or colorScheme == 3) then
		AltInfo4HealthPercentOld(unit, self, healthPercent, colorScheme)
		return
	end
	AltInfoCustom.health_bar[unit]:SetWidth(ToPercent(healthPercent))
end

local function AltInfo_CustomName(unit, self, name)
	if (name) then
		if (string.len(name) > 0) then
			AltInfoCustom.nameCompare[unit] = name
			AltInfoCustom.isHovering[unit] = true
			AltInfoCustom.health_bar_glow[unit]:SetVisible(AltInfoCustom.HOVER_GLOW)
		else
			AltInfoCustom.nameCompare[unit] = ''
			AltInfoCustom.isHovering[unit] = false
			AltInfoCustom.health_bar_glow[unit]:SetVisible(false)
		end
	else
		AltInfoCustom.health_bar_glow[unit]:SetVisible(false)
		AltInfoCustom.nameCompare[unit] = ''
		AltInfoCustom.isHovering[unit] = false
	end
	UpdateBackgroundFrame(unit)
end

local function AltInfo_CustomHasMana(unit, self, hasMana)
	if (AtoB(hasMana)) then
		AltInfoCustom.mana_panel[unit]:SetVisible(true)
		AltInfoCustom.creep_plate_parent[unit]:SetHeight('0.85h')
		AltInfoCustom.health_bar_parent[unit]:SetHeight(AltInfoCustom.creep_plate_parent[unit]:GetHeight() - AltInfoCustom.mana_panel[unit]:GetHeight() - 3)
	else
		AltInfoCustom.mana_panel[unit]:SetVisible(false)
		AltInfoCustom.creep_plate_parent[unit]:SetHeight('0.56h')
		AltInfoCustom.health_bar_parent[unit]:SetHeight( AltInfoCustom.creep_plate_parent[unit]:GetHeight() - 2)
	end
end

local function AltInfo_CustomHasHealth(unit, self, hasHealth)
	AltInfoCustom.hasHealth[unit] = AtoB(hasHealth)

	AltInfoCustom.main_panel[unit]:SetVisible(AltInfoCustom.hasHealth[unit] or AltInfoCustom.levelCompare[unit] > 0)
end

local function AltInfo_CustomManaPercent(unit, self, manaPercent)
	AltInfoCustom.mana_bar[unit]:SetWidth(ToPercent(manaPercent))
end

local function AltInfo_CustomHealthLerp(unit, self, healthLerp)
	AltInfoCustom.health_lerp[unit]:SetWidth(ToPercent(healthLerp))
end

local function AltInfo_CustomIsInExpRange(unit, self, isInExpRange)
	AltInfoCustom.exp[unit]:SetVisible(AtoB(isInExpRange))
end

local function AltInfo_CustomShowInventory(unit, self, showInventory)
	AltInfoCustom.showInventory[unit] = AtoB(showInventory)
	AltInfoCustom.inventory[unit]:SetVisible(AltInfoCustom.showInventory[unit])
	UpdateBackgroundFrame(unit)
end

local function AltInfo_CustomLevel(unit, self, level)
	local level = tonumber(level) or 0
	AltInfoCustom.levelCompare[unit] = level

	AltInfoCustom.main_panel[unit]:SetVisible(AltInfoCustom.hasHealth[unit] or AltInfoCustom.levelCompare[unit] > 0)

	if (level > 0) then
		AltInfoCustom.level_panel[unit]:SetVisible(true)
		AltInfoCustom.level_pie[unit]:SetValue(level)
	else
		AltInfoCustom.level_panel[unit]:SetVisible(false)
	end
	UpdateBackgroundFrame(unit)
end

function AltInfoCustom.Initialize(unit)
	SetupWidgets(unit)

	interface:RegisterWatch('AltInfo_CustomColor'..unit, function(...) AltInfo_CustomColor(unit, ...) end)
	interface:RegisterWatch('AltInfo_CustomHealthPercent'..unit, function(...) AltInfo_CustomHealthPercent(unit, ...) end)
	interface:RegisterWatch('AltInfo_CustomName'..unit, function(...) AltInfo_CustomName(unit, ...) end)
	interface:RegisterWatch('AltInfo_CustomHasMana'..unit, function(...) AltInfo_CustomHasMana(unit, ...) end)
	interface:RegisterWatch('AltInfo_CustomHasHealth'..unit, function(...) AltInfo_CustomHasHealth(unit, ...) end)
	interface:RegisterWatch('AltInfo_CustomManaPercent'..unit, function(...) AltInfo_CustomManaPercent(unit, ...) end)
	interface:RegisterWatch('AltInfo_CustomHealthLerp'..unit, function(...) AltInfo_CustomHealthLerp(unit, ...) end)
	interface:RegisterWatch('AltInfo_CustomIsInExpRange'..unit, function(...) AltInfo_CustomIsInExpRange(unit, ...) end)
	interface:RegisterWatch('AltInfo_CustomShowInventory'..unit, function(...) AltInfo_CustomShowInventory(unit, ...) end)
	interface:RegisterWatch('AltInfo_CustomLevel'..unit, function(...) AltInfo_CustomLevel(unit, ...) end)
end