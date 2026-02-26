---------------------------------------------------------- 
--	Name: 		Enhanced Buffs		            		--				
--  Copyright 2015 Frostburn Studios					--
----------------------------------------------------------

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface = object
local interfaceName = interface:GetName()

-- sorry about var names, the original var names were confusing too
HoN_Ebuffs = {}

HoN_Ebuffs.graphCoeff = -20000
HoN_Ebuffs.graphOffset = -1250
HoN_Ebuffs.threash = 0.5

HoN_Ebuffs.endTime = 0.4
HoN_Ebuffs.startScale = 1.5
HoN_Ebuffs.maxScale = 3
HoN_Ebuffs.peakTime = 0.18564064605--(HoN_Ebuffs.endTime / (HoN_Ebuffs.startScale - 1.0)) * (HoN_Ebuffs.startScale - HoN_Ebuffs.maxScale + ((HoN_Ebuffs.maxScale - HoN_Ebuffs.startScale)*(HoN_Ebuffs.maxScale - 1)) ^ 0.5     --(0.4 / (1.5 - 1.0)) * (1.5 - 3 + ((3 - 1.5)*(3 - 1)) ^ 0.5
HoN_Ebuffs.coeff = 43.525635097--(HoN_Ebuffs.maxScale - HoN_Ebuffs.startScale) / (HoN_Ebuffs.peakTime * HoN_Ebuffs.peakTime)   --(3 - 1.5) / (0.18564064605 * 0.18564064605)

HoN_Ebuffs.borderThickness = 0

HoN_Ebuffs.buffGroups = {}
HoN_Ebuffs.popped = {}
HoN_Ebuffs.nextFrame = {}

function GetSlotFromNameIcon(group, name, icon)
	if (group and name and icon) then
		for i,v in pairs(HoN_Ebuffs.buffGroups[group].buffs) do
			if ((v.name == name) and (v.icon == icon)) then
				return i
			end
		end
	end

	return nil
end

function HoN_Ebuffs.GetFrameSlot(group)
	local ret = HoN_Ebuffs.nextFrame[group] + 13
	if (HoN_Ebuffs.nextFrame[group] == 10) then
		HoN_Ebuffs.nextFrame[group] = 0
	else
		HoN_Ebuffs.nextFrame[group] = HoN_Ebuffs.nextFrame[group] + 1
	end

	return ret
end

function HoN_Ebuffs:RegisterBuffGroup(groupName)
	HoN_Ebuffs.buffGroups[groupName] = {}
	HoN_Ebuffs.buffGroups[groupName].bestC = 0
	HoN_Ebuffs.buffGroups[groupName].bestR = 0
	HoN_Ebuffs.buffGroups[groupName].bestCoverage = 0

	HoN_Ebuffs.buffGroups[groupName].buffs = {}
	HoN_Ebuffs.popped[groupName] = {}
	HoN_Ebuffs.nextFrame[groupName] = 0
end

local function BuffExists(group, name, icon)
	if (group and name and icon) then
		for k,v in pairs(HoN_Ebuffs.buffGroups[group].buffs) do
			if ((v.name == name) and (v.icon == icon)) then
				return v.exists
			end
		end
	end

	return false
end

local function AlreadyPopped(group, name, icon)
	if (group and name and icon) then
		for k,v in pairs(HoN_Ebuffs.popped[group]) do
			if (v[1] == name and v[2] == icon) then
				return true
			end
		end
	end

	return false
end

local function DeletePopped(group, name, icon)
	if (group and name and icon) then
		for k,v in pairs(HoN_Ebuffs.popped[group]) do
			if (v[1] == name and v[2] == icon) then
				table.remove(HoN_Ebuffs.popped[group], k)
				return
			end
		end
	end
end	

local function CreateFrameWatch(group, slot, startTime)
	if (NotEmpty(HoN_Ebuffs.buffGroups[group].buffs[slot].name) and NotEmpty(HoN_Ebuffs.buffGroups[group].buffs[slot].icon)) then
		if ((GetCvarBool('ebuff_cfgActive_popenabled') and ((not GetCvarBool('ebuff_cfgActive_popdispassive')) or HoN_Ebuffs.buffGroups[group].buffs[slot].duration > 0))) then
			local regWidget = GetWidget("ebuff_"..group..HoN_Ebuffs.GetFrameSlot(group).."_sleeper", "game")
			local name, icon = HoN_Ebuffs.buffGroups[group].buffs[slot].name, HoN_Ebuffs.buffGroups[group].buffs[slot].icon

			local onFrameFunc = function()
				local fixedSlot = GetSlotFromNameIcon(group, name, icon)
				if fixedSlot then
					local borderWidget = GetWidget("ebuff_iconframe"..group..fixedSlot.."_border1", "game")

					local dimension = math.max(1, HoN_Ebuffs.maxScale - HoN_Ebuffs.coeff * math.pow(((GetTime() - startTime) / 1000 - HoN_Ebuffs.peakTime), 2))
					local alpha = math.min(1, ((GetTime() - startTime) / HoN_Ebuffs.peakTime / 1000))

					borderWidget:Fade(alpha, alpha, 1)
					borderWidget:SetWidth(FtoP(dimension))
					borderWidget:SetHeight(FtoP(dimension))

					GetWidget('ebuff_stateicon'..group..fixedSlot, 'game', true):BringToFront()
					HoN_Ebuffs:UpdateBuffIcon(group, fixedSlot)
				end

				if ((GetTime() - startTime) > (HoN_Ebuffs.endTime * 1000)) then
					regWidget:ClearCallback('onframe')
					regWidget:RefreshCallbacks()
				end
			end

			regWidget:SetCallback('onframe', onFrameFunc)
			regWidget:RefreshCallbacks()
		end
		table.insert(HoN_Ebuffs.popped[group], {HoN_Ebuffs.buffGroups[group].buffs[slot].name, HoN_Ebuffs.buffGroups[group].buffs[slot].icon})
	end
end

local function SyncPops(group)
	for i,v in pairs(HoN_Ebuffs.popped[group]) do
		if (not BuffExists(group, v[1], v[2])) then
			DeletePopped(group, v[1], v[2])
		end
	end
end

local function CheckForAndUpdatePop(group, slot, name, icon)
	SyncPops(group)

	if (not AlreadyPopped(group, name, icon)) then
		CreateFrameWatch(group, slot, GetTime())
	end
end

function HoN_Ebuffs:RegisterBuff(slotWidget, group, slot)
	slot = tonumber(slot)
	if (not HoN_Ebuffs.buffGroups[group].buffs[slot]) then
		HoN_Ebuffs.buffGroups[group].buffs[slot] = {}
	end

	local inventoryExists = function(group, slot, param)
		param = AtoB(param)

		HoN_Ebuffs.buffGroups[group].buffs[slot].exists = param
		GetWidget("ebuff_iconframe"..group..slot, "game"):SetVisible(param)

		if (param) then
			if (not AlreadyPopped(group, HoN_Ebuffs.buffGroups[group].buffs[slot].name, HoN_Ebuffs.buffGroups[group].buffs[slot].icon)) then
				CreateFrameWatch(group, slot, GetTime())
			end
		else
			GetWidget("ebuff_iconframe"..group..slot, "game"):Sleep(5, function() SyncPops(group) end)
		end
	end

	local inventoryDescription = function(group, slot, param)
		if (string.gsub(param, "'", "") ~= HoN_Ebuffs.buffGroups[group].buffs[slot].name) then
			GetWidget("ebuff_"..group..slot.."_sleeper", "game"):Sleep(3, function()
				CheckForAndUpdatePop(group, slot, param, HoN_Ebuffs.buffGroups[group].buffs[slot].icon)
			end)
		end

		if (NotEmpty(param)) then
			HoN_Ebuffs.buffGroups[group].buffs[slot].name = string.gsub(param, "'", "")
		else
			HoN_Ebuffs.buffGroups[group].buffs[slot].name = ""
		end

		local borderWidg = GetWidget("ebuff_iconframe"..group..slot.."_border1", "game")
		borderWidg:SetHeight("100%")
		borderWidg:SetWidth("100%")
		borderWidg:Fade(1, 1, 1)
		HoN_Ebuffs:UpdateBuffIcon(group, slot)
	end

	local inventoryIcon = function(group, slot, param)
		if (string.gsub(param, "'", "") ~= HoN_Ebuffs.buffGroups[group].buffs[slot].icon) then
			GetWidget("ebuff_"..group..slot.."_sleeper", "game"):Sleep(1, function()
				CheckForAndUpdatePop(group, slot, HoN_Ebuffs.buffGroups[group].buffs[slot].name, icon)
			end)
		end

		HoN_Ebuffs.buffGroups[group].buffs[slot].icon = param

		local borderWidg = GetWidget("ebuff_iconframe"..group..slot.."_border1", "game")
		borderWidg:SetHeight("100%")
		borderWidg:SetWidth("100%")
		borderWidg:Fade(1, 1, 1)
		HoN_Ebuffs:UpdateBuffIcon(group, slot)

		GetWidget("ebuff_texture"..group..slot, "game"):SetTexture(param)
	end

	local inventoryDuration = function(group, slot, param)
		param = tonumber(param)

		HoN_Ebuffs.buffGroups[group].buffs[slot].duration = param

		if (slot and param and NotEmpty(group) and HoN_Ebuffs.buffGroups[group].buffs[slot].durationPercent) then
			local maxDuration = 100.0 * round((param / HoN_Ebuffs.buffGroups[group].buffs[slot].durationPercent) / 100.0)
			local threashTime = math.min(HoN_Ebuffs.threash * maxDuration, 4000)

			local graphVal = HoN_Ebuffs.graphCoeff / (param - HoN_Ebuffs.graphOffset) + HoN_Ebuffs.graphCoeff / HoN_Ebuffs.graphOffset
			local prevBlink = (HoN_Ebuffs.graphOffset * HoN_Ebuffs.graphCoeff / (math.ceil(graphVal) * HoN_Ebuffs.graphOffset - HoN_Ebuffs.graphCoeff) + HoN_Ebuffs.graphOffset)

			if (GetCvarBool('ebuff_cfgActive_blinkenabled') and (graphVal - math.floor(graphVal)) > 0.5 and (prevBlink < threashTime) and (param < threashTime)) then
				GetWidget("ebuff_iconframe"..group..slot.."_fadeframe", "game"):Fade(0.3, 0.3, 1)
			else
				GetWidget("ebuff_iconframe"..group..slot.."_fadeframe", "game"):Fade(1, 1, 1)
			end
		end

		GetWidget("ebuff_iconframe"..group..slot.."_time", "game"):SetVisible((tonumber(param) > 0) and GetCvarBool('ebuff_cfgActive_showlbl'))
		if (GetCvarBool('ebuff_cfgActive_dectimer')) then
			if (param > 10000) then
				GetWidget("ebuff_iconframe"..group..slot.."_time", "game"):SetText(math.floor(param / 1000)..'s')
			else
				GetWidget("ebuff_iconframe"..group..slot.."_time", "game"):SetText(math.floor(param / 1000)..'.'..math.floor((param%1000) / 100)..'s')
			end
		else
			GetWidget("ebuff_iconframe"..group..slot.."_time", "game"):UICmd('SetText(FtoT('..param..', 0, 0, \'-\') # \'s\')')
		end


	end

	local inventoryState = function(group, slot, param1, param2)
		param1, param2 = AtoB(param1), AtoB(param2)
		HoN_Ebuffs.buffGroups[group].buffs[slot].state = {param1, param2}

		local pie1 = GetWidget("ebuff_iconframe"..group..slot.."_pie1", "game")
		local pie2 = GetWidget("ebuff_iconframe"..group..slot.."_pie2", "game")
		local timebar = GetWidget("ebuff_iconframe"..group..slot.."_timebar", "game")

		if (GetCvarBool('ebuff_cfgActive_timerbar')) then
			if (param1) then
				pie1:SetColor('.8 0 0')
			elseif (param2) then
				pie1:SetColor('0 .8 0')
			else
				pie1:SetColor('0 .4 .8')
			end
		else
			if (param1) then
				pie1:SetColor('.2 0 0')
			elseif (param2) then
				pie1:SetColor('0 .2 0')
			else
				pie1:SetColor('0 .1 .2')
			end
		end

		if (param1) then
			pie2:SetColor('1 0 0')
		elseif (param2) then
			pie2:SetColor('0 1 0')
		else
			pie2:SetColor('0 .5 1')
		end

		if (param1) then
			timebar:SetColor('1 0 0')
		elseif (param2) then
			timebar:SetColor('0 1 0')
		else
			timebar:SetColor('0 .5 1')
		end
	end

	local inventoryDurationPercent = function(group, slot, param)
		param = tonumber(param)

		HoN_Ebuffs.buffGroups[group].buffs[slot].durationPercent = param

		if (param > 0) then
			GetWidget("ebuff_iconframe"..group..slot.."_pie2", "game"):SetValue(param)
		else
			GetWidget("ebuff_iconframe"..group..slot.."_pie2", "game"):SetValue(1)
		end

		GetWidget("ebuff_iconframe"..group..slot.."_timebar", "game"):SetWidth(FtoP(math.min(1, param)))
	end

	local inventoryCharges = function(group, slot, param)
		param = tonumber(param)
		HoN_Ebuffs.buffGroups[group].buffs[slot].charges = param

		GetWidget('ebuff_iconcharges'..group..slot, "game"):SetVisible(param > 0)
		GetWidget("ebuff_iconframe"..group..slot.."_charges", "game"):SetText(param)
	end

	slotWidget:RegisterWatch(group..'InventoryExists'..slot, function(_, param) inventoryExists(group, slot, param) end)
	slotWidget:RegisterWatch(group..'InventoryDescription'..slot, function(_, param) inventoryDescription(group, slot, param) end)
	slotWidget:RegisterWatch(group..'InventoryIcon'..slot, function(_, param) inventoryIcon(group, slot, param) end)
	slotWidget:RegisterWatch(group..'InventoryDuration'..slot, function(_, param) inventoryDuration(group, slot, param) end)
	slotWidget:RegisterWatch(group..'InventoryState'..slot, function(_, _, param1, param2) inventoryState(group, slot, param1, param2) end)
	slotWidget:RegisterWatch(group..'InventoryDurationPercent'..slot, function(_, param) inventoryDurationPercent(group, slot, param) end)
	slotWidget:RegisterWatch(group..'InventoryCharges'..slot, function(_, param) inventoryCharges(group, slot, param) end)
end

function HoN_Ebuffs:UpdateBuffGroup(group)
	-- wrap stuff
	local tempWidget = GetWidget("ebuff_"..group.."_wrap", "game")

	local divideValue = 1.0
	if (GetCvarBool('ebuff_cfgActive_overheadlbl') and GetCvarBool('ebuff_cfgActive_showlbl')) then
		divideValue = divideValue + 0.35
	end
	if (GetCvarBool('ebuff_cfgActive_timerbar')) then
		divideValue = divideValue + 0.05
	end

	local slotRatio = 1.0 / divideValue
	local updateRatio = GetWidget('ebuff_'..group, 'game'):GetWidth() / GetWidget('ebuff_'..group, 'game'):GetHeight()

	local compareValues = {11, 6, 4, 3, 2, 1}
	local numVals = #compareValues

	-- reset values
	HoN_Ebuffs.buffGroups[group].bestCoverage = 0
	HoN_Ebuffs.buffGroups[group].bestC = 11
	HoN_Ebuffs.buffGroups[group].bestR = 1

	for i=1,numVals do
		local tempC, tempR = compareValues[i], compareValues[(numVals+1)-i]
		local tempCoverage = math.min((updateRatio / (slotRatio * tempC / tempR)),  ((slotRatio * tempC / tempR) / updateRatio))
		if (tempCoverage > HoN_Ebuffs.buffGroups[group].bestCoverage) then
			HoN_Ebuffs.buffGroups[group].bestCoverage = tempCoverage
			HoN_Ebuffs.buffGroups[group].bestC = tempC
			HoN_Ebuffs.buffGroups[group].bestR = tempR
		end
	end

	local ratio = slotRatio * HoN_Ebuffs.buffGroups[group].bestC / HoN_Ebuffs.buffGroups[group].bestR
	local widthPercent, heightPercent
	if (ratio > updateRatio) then
		widthPercent = 1.0
		heightPercent = updateRatio / ratio
	else
		widthPercent = ratio / updateRatio
		heightPercent = 1.0
	end

	tempWidget:SetWidth(FtoP(widthPercent))
	tempWidget:SetHeight(FtoP(heightPercent))

	-- icons
	if (HoN_Ebuffs.buffGroups[group].buffs) then
		for k,v in pairs(HoN_Ebuffs.buffGroups[group].buffs) do
			HoN_Ebuffs:UpdateBuffIcon(group, k)
		end
	end
end

function HoN_Ebuffs:OptionsUpdated()
	HoN_Ebuffs:UpdateAllBuffs()
end

function HoN_Ebuffs:UpdateAllBuffs()
	for k,v in pairs(HoN_Ebuffs.buffGroups) do
		HoN_Ebuffs:UpdateBuffGroup(k)
	end
end

local function UpdateWidgetBorderThickness(widget, positiveHeight)
	widget:SetWidth(-(2.0 * HoN_Ebuffs.borderThickness))
	if (positiveHeight) then
		widget:SetHeight(2.0 * HoN_Ebuffs.borderThickness)
	else
		widget:SetHeight(-(2.0 * HoN_Ebuffs.borderThickness))
	end
end

function HoN_Ebuffs:UpdateBuffIcon(group, slot)
	if (HoN_Ebuffs.buffGroups[group] and HoN_Ebuffs.buffGroups[group].buffs[slot] and (HoN_Ebuffs.buffGroups[group].bestC ~= 0) and (HoN_Ebuffs.buffGroups[group].bestR ~= 0)) then 
		--- stateicon
		local tempWidget = GetWidget("ebuff_stateicon"..group..slot, "game")
		tempWidget:SetWidth(FtoP(1.0 / HoN_Ebuffs.buffGroups[group].bestC))
		tempWidget:SetHeight(FtoP(1.0 / HoN_Ebuffs.buffGroups[group].bestR))
		tempWidget:SetX(FtoP(((slot - 13) % HoN_Ebuffs.buffGroups[group].bestC) / HoN_Ebuffs.buffGroups[group].bestC))
		tempWidget:SetY(FtoP(math.floor((slot - 13) / HoN_Ebuffs.buffGroups[group].bestC) / HoN_Ebuffs.buffGroups[group].bestR))

		if group == 'Selected' then
			local bufAlignment = GetCvarBool('ui_minimap_rightside') and 'right' or 'left'
			tempWidget:SetAlign(bufAlignment)
		end

		-- label position thing
		tempWidget = GetWidget("ebuff_"..group..slot.."_labelpos", "game")
		if (GetCvarBool('ebuff_cfgActive_overheadlbl') and GetCvarBool('ebuff_cfgActive_showlbl')) then
			tempWidget:SetY('40@')
		else
			tempWidget:SetY(0)
		end

		if (GetCvarBool('ebuff_cfgActive_timerbar')) then
			tempWidget:SetHeight('105@')
		else
			tempWidget:SetHeight('100@')
		end

		-- border thickness for a couple things
		tempWidget = GetWidget("ebuff_iconframe"..group..slot.."_border1", "game")
		if (not GetCvarBool('ebuff_cfgActive_timerbar')) then
			HoN_Ebuffs.borderThickness = math.ceil(tempWidget:GetWidth() / 32)
		else
			HoN_Ebuffs.borderThickness = math.ceil(tempWidget:GetWidth() / 64)
		end
		UpdateWidgetBorderThickness(GetWidget('ebuff_'..group..slot..'_borderblack', 'game'))
		UpdateWidgetBorderThickness(GetWidget('ebuff_'..group..slot..'_bordercolor', 'game'))
		tempWidget = GetWidget('ebuff_iconframe'..group..slot..'_timebarparent', 'game')
		UpdateWidgetBorderThickness(tempWidget, true)
		tempWidget:SetY(-HoN_Ebuffs.borderThickness)

		-- pie graph stuff
		GetWidget("ebuff_iconframe"..group..slot.."_pie2", "game"):SetVisible(not GetCvarBool('ebuff_cfgActive_timerbar'))

		-- square icon
		tempWidget = GetWidget("ebuff_"..group..slot.."_square", "game")
		tempWidget:SetHeight(tempWidget:GetWidth())

		-- timebar stuff
		tempWidget = GetWidget("ebuff_iconframe"..group..slot.."_timebarmaster", "game")
		tempWidget:SetVisible(GetCvarBool('ebuff_cfgActive_timerbar'))
		tempWidget:SetHeight(tonumber(tempWidget:UICmd('GetHeightFromString(\'100%\')')) - tempWidget:GetWidth())

		-- time label position
		tempWidget = GetWidget("ebuff_iconframe"..group..slot.."_time", "game")
		if (GetCvarBool('ebuff_cfgActive_overheadlbl')) then
			tempWidget:SetY(tempWidget:UICmd('GetYFromString(\'-36%\')') - HoN_Ebuffs.borderThickness)
		else
			tempWidget:SetY('15%')
		end

		-- these guys need to be refreshed since they resize in a special way
		GetWidget("ebuff_iconframe"..group..slot.."_time", "game"):UICmd("RefreshWidget();")
		GetWidget("ebuff_iconframe"..group..slot.."_charges", "game"):UICmd("RefreshWidget();")
	end
end

function interface:HoNEBuffsF(func, ...)
  print(HoN_Ebuffs[func](self, ...))
end	