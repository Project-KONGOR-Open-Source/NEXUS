----------------------------------------------------------
--	Name: 		Alt Info Building Script       			--
-- 	Version: 	4.2										--
--  (C)2017 Garena Shanghai Games						--
----------------------------------------------------------

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()

RegisterScript2('AltInfoBuilding', '1')

AltInfoBuilding = _G[AltInfoBuilding] or {}

AltInfoBuilding.nameCompare = {}
AltInfoBuilding.levelCompare = {}
AltInfoBuilding.isHovering = {}
AltInfoBuilding.hasMana = {}

AltInfoBuilding.background_frame 	= {}
AltInfoBuilding.main_frame 		 	= {}
AltInfoBuilding.main_frame_mana  	= {}
AltInfoBuilding.name_label 			= {}
AltInfoBuilding.main_panel 			= {}
AltInfoBuilding.health_panel 		= {}
AltInfoBuilding.health_lerp 		= {}
AltInfoBuilding.health_bar 			= {}
AltInfoBuilding.mana_panel 			= {}
AltInfoBuilding.mana_bar 			= {}

local function SetupWidgets(unit)
	AltInfoBuilding.background_frame[unit] 		= interface:GetWidget('background_frame'..unit)
	AltInfoBuilding.main_frame[unit]	 		= interface:GetWidget('main_frame'..unit)
	AltInfoBuilding.main_frame_mana[unit] 		= interface:GetWidget('main_frame_mana'..unit)
	AltInfoBuilding.name_label[unit] 			= interface:GetWidget('name_label'..unit)
	AltInfoBuilding.main_panel[unit] 			= interface:GetWidget('main_panel'..unit)
	AltInfoBuilding.health_panel[unit] 			= interface:GetWidget('health_panel'..unit)
	AltInfoBuilding.health_lerp[unit] 			= interface:GetWidget('health_lerp'..unit)
	AltInfoBuilding.health_bar[unit] 			= interface:GetWidget('health_bar'..unit)
	AltInfoBuilding.mana_panel[unit] 			= interface:GetWidget('mana_panel'..unit)
	AltInfoBuilding.mana_bar[unit] 				= interface:GetWidget('mana_bar'..unit)

	AltInfoBuilding.nameCompare[unit] = ''
	AltInfoBuilding.levelCompare[unit] = ''
	AltInfoBuilding.isHovering[unit]= false
	AltInfoBuilding.hasMana[unit] = true

	local screenH = GetScreenHeight()
	local frameY, frameManaY = '0.3h', '0.6h'
	local frameH, frameManaH = '2.96h', '2.96h'
	local healthX, healthY = '-0.4h', '0.5h'
	local manaX, manaY = '-0.4h', '1.1h'
	local healthW, healthH, manaH = '11.65h', '0.6h', '0.5h'
	if screenH >= 1080 then
	elseif screenH >= 1064 then
		healthX = '-0.3h'
		manaX = '-0.4h'
		frameY, frameManaY = '0.2h', '0.5h'
	elseif screenH >= 1050 then
		healthX, healthY = '-0.3h', '0.4h'
		healthH = '0.7h'
		frameY, frameManaY = '0.2h', '0.5h'
		frameH, frameManaH = '3.1h', '3.1h'
	elseif screenH >= 1024 then
		frameY, frameManaY = '0.2h', '0.4h'
		healthX, healthY, manaH = '-0.4h', '0.4h', '0.4h'
	elseif screenH >= 960 then
		frameY, frameManaY = '0.2h', '0.5h'
		healthH, healthX, healthY, manaH = '0.5h', '-0.3h', '0.4h', '0.4h'
	elseif screenH >= 900 then
		healthW = '11.75h'
		healthY = '0.5h'
		frameH, frameManaH = '3.1h', '3.1h'
		manaX = '-0.3h'
		frameManaY = '0.7h'
	elseif screenH >= 864 then
		frameH, frameManaH = '3.1h', '3.1h'
		frameY, frameManaY = '0.2h', '0.5h'
		healthH, healthY = '0.6h', '0.3h'
		manaY = '1.0h'
	elseif screenH >= 800 then
		healthY = '0.3h'
		frameH, frameManaH = '3.1h', '3.1h'
		frameY, frameManaY = '0.1h', '0.5h'
	elseif screenH >= 768 then
		frameManaY = '0.5h'
		manaH = '0.5h'
	elseif screenH >= 720 then
		manaY = '1.0h'
		healthH, healthY = '0.6h', '0.4h'
		frameH, frameManaH = '3.5h', '3.3h'
		frameY, frameManaY = '0.3h', '0.6h'
	elseif screenH >= 600 then
		healthX = '-0.5h'
		frameH, frameManaH = '3.75h', '3.5h'
		frameY, frameManaY = '0.3h', '0.5h'
	end

	AltInfoBuilding.main_frame[unit]:SetY(frameY)
	AltInfoBuilding.main_frame_mana[unit]:SetY(frameManaY)

	AltInfoBuilding.main_frame[unit]:SetHeight(frameH)
	AltInfoBuilding.main_frame_mana[unit]:SetHeight(frameManaH)

	AltInfoBuilding.health_panel[unit]:SetX(healthX)
	AltInfoBuilding.health_panel[unit]:SetY(healthY)
	AltInfoBuilding.health_panel[unit]:SetWidth(healthW)
	AltInfoBuilding.health_panel[unit]:SetHeight(healthH)

	AltInfoBuilding.mana_panel[unit]:SetX(manaX)
	AltInfoBuilding.mana_panel[unit]:SetY(manaY)
	AltInfoBuilding.mana_panel[unit]:SetHeight(manaH)
end

local function UpdateBackgroundFrame(unit)
	if (AltInfoBuilding.nameCompare[unit] and AltInfoBuilding.isHovering[unit]) then
		AltInfoBuilding.background_frame[unit]:SetVisible(true)
		AltInfoBuilding.background_frame[unit]:SetX((GetStringWidth('dyn_10', AltInfoBuilding.nameCompare[unit])    + AltInfoBuilding.background_frame[unit]:GetXFromString('1.2h')) * -0.5)
		AltInfoBuilding.background_frame[unit]:SetWidth(GetStringWidth('dyn_10', AltInfoBuilding.nameCompare[unit]) + AltInfoBuilding.background_frame[unit]:GetXFromString('1.2h'))
		AltInfoBuilding.name_label[unit]:SetText(AltInfoBuilding.nameCompare[unit])

		AltInfoBuilding.main_frame[unit]:SetVisible(not AltInfoBuilding.hasMana[unit])
		AltInfoBuilding.main_frame_mana[unit]:SetVisible(AltInfoBuilding.hasMana[unit])
	else
		AltInfoBuilding.background_frame[unit]:SetVisible(false)

		AltInfoBuilding.main_frame[unit]:SetVisible(false)
		AltInfoBuilding.main_frame_mana[unit]:SetVisible(false)
	end
end

local function AltInfo_BuildingName(unit, self, name)
	if (NotEmpty(name)) then
		AltInfoBuilding.nameCompare[unit] = name
		AltInfoBuilding.isHovering[unit] = true
	else
		AltInfoBuilding.nameCompare[unit] = ''
		AltInfoBuilding.isHovering[unit] = false
	end
	UpdateBackgroundFrame(unit)
end

local function AltInfoBuildingColorOld(unit, self, color, nameColor, type, colorScheme)
	local glowColorString = '1 0 0 1'
	local lerpGreen = '0 0.7 0 1'

	local ownerType = AtoN(type)

	if (ownerType == 1) or (ownerType == 2) or (ownerType == 4) or (ownerType == 5) then -- Self or Ally
		glowColorString = '0 1 0 1'
	elseif ownerType == 3 then --enemy
		glowColorString = '1 0 0 1'
	end

	AltInfoBuilding.health_lerp[unit]:SetColor(lerpGreen)
	AltInfoBuilding.main_frame[unit]:SetColor(glowColorString)
	AltInfoBuilding.main_frame_mana[unit]:SetColor(glowColorString)
end

local function AltInfo_BuildingColor(unit, self, color, nameColor, type, colorScheme)
	local colorScheme = AtoN(colorScheme)
	if not (colorScheme == 2 or colorScheme == 3) then
		AltInfoBuildingColorOld(unit, self, color, nameColor, type, colorScheme)
		return
	end

	local ownerType = AtoN(type)
	local glowGreen, glowRed, barGreen, barRed, lerpGreen, lerpRed

	if (colorScheme == 3) then
		glowGreen, glowRed = '1 1 0 1', '0 .3 1 1'
		barGreen, barRed = '.88 .88 0 1', '0 .25 1 1'
		lerpGreen, lerpRed = '0 .3 1 .9', '1 1 0 .9'
	else
		glowGreen, glowRed = '0 1 0 1', '1 0 0 1'
		barGreen, barRed = '#3aa400', '#e70000'
		lerpGreen, lerpRed = '.7 0 0 1', '1 1 0 .7'
	end

	if (ownerType == 1) or (ownerType == 2) or (ownerType == 4) or (ownerType == 5) then -- Self or Ally
		AltInfoBuilding.health_lerp[unit]:SetColor(lerpGreen)
		AltInfoBuilding.health_bar[unit]:SetColor(barGreen)
		AltInfoBuilding.main_frame[unit]:SetColor(glowGreen)
		AltInfoBuilding.main_frame_mana[unit]:SetColor(glowGreen)
	elseif ownerType == 3 then --enemy
		AltInfoBuilding.health_lerp[unit]:SetColor(lerpRed)
		AltInfoBuilding.health_bar[unit]:SetColor(barRed)
		AltInfoBuilding.main_frame[unit]:SetColor(glowRed)
		AltInfoBuilding.main_frame_mana[unit]:SetColor(glowRed)
	else --other
		AltInfoBuilding.health_lerp[unit]:SetColor(color)
		AltInfoBuilding.health_bar[unit]:SetColor(color)
		AltInfoBuilding.main_frame[unit]:SetColor(glowRed)
		AltInfoBuilding.main_frame_mana[unit]:SetColor(glowRed)
	end
end

local function AltInfo_BuildingHasHealth(unit, self, hasHealth)
	AltInfoBuilding.main_panel[unit]:SetVisible(AtoB(hasHealth))
end

local function AltInfoBuildingHealthPercent(unit, self, healthPercent, colorScheme)
	AltInfoBuilding.health_bar[unit]:SetWidth(ToPercent(healthPercent))
	AltInfoBuilding.health_bar[unit]:SetColor(Saturate(1 - (healthPercent - 0.5) / 0.5), (healthPercent + ((healthPercent - 0.05) * 0.2)) / 0.45, 0, 1)
end

local function AltInfo_BuildingHealthPercent(unit, self, healthPercent, colorScheme)
	local colorScheme = AtoN(colorScheme)
	if not (colorScheme == 2 or colorScheme == 3) then
		AltInfoBuildingHealthPercent(unit, self, healthPercent, colorScheme)
		return
	end
	AltInfoBuilding.health_bar[unit]:SetWidth(ToPercent(healthPercent))
end

local function AltInfo_BuildingHasMana(unit, self, hasMana)
	AltInfoBuilding.hasMana[unit] = AtoB(hasMana)
	if (AltInfoBuilding.hasMana[unit]) then
		AltInfoBuilding.mana_panel[unit]:SetVisible(true)
	else
		AltInfoBuilding.mana_panel[unit]:SetVisible(false)
	end
end

local function AltInfo_BuildingManaPercent(unit, self, manaPercent)
	AltInfoBuilding.mana_bar[unit]:SetWidth(ToPercent(manaPercent))
end

local function AltInfo_BuildingHealthLerp(unit, self, healthLerp)
	AltInfoBuilding.health_lerp[unit]:SetWidth(ToPercent(healthLerp))
end

function AltInfoBuilding.Initialize(unit)

	SetupWidgets(unit)

	interface:RegisterWatch('AltInfo_BuildingColor'..unit, function(...) AltInfo_BuildingColor(unit, ...) end)
	interface:RegisterWatch('AltInfo_BuildingHealthPercent'..unit, function(...) AltInfo_BuildingHealthPercent(unit, ...) end)
	interface:RegisterWatch('AltInfo_BuildingName'..unit, function(...) AltInfo_BuildingName(unit, ...) end)
	interface:RegisterWatch('AltInfo_BuildingHasMana'..unit, function(...) AltInfo_BuildingHasMana(unit, ...) end)
	interface:RegisterWatch('AltInfo_BuildingHasHealth'..unit, function(...) AltInfo_BuildingHasHealth(unit, ...) end)
	interface:RegisterWatch('AltInfo_BuildingManaPercent'..unit, function(...) AltInfo_BuildingManaPercent(unit, ...) end)
	interface:RegisterWatch('AltInfo_BuildingHealthLerp'..unit, function(...) AltInfo_BuildingHealthLerp(unit, ...) end)
end
