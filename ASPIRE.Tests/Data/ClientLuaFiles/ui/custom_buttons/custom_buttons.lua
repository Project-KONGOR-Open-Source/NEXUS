-- Custom buttons selection

local interface = object

local function initializeChoiceButton(index, buttonInterface, button, overlay, glowEffect, text, buttonVariable)
	button:SetCallback('onmouseover', function(widget)
		interface:GetWidget(overlay):SetVisible(true)
		interface:GetWidget(glowEffect):SetVisible(true)
		interface:GetWidget(text):SetVisible(true)
		PlaySound('/shared/sounds/ui/button_over_0%.wav')
	end)
	button:SetCallback('onmouseout', function(widget)
		interface:GetWidget(overlay):SetVisible(false)
		interface:GetWidget(glowEffect):SetVisible(false)
		interface:GetWidget(text):SetVisible(false)
	end)

	button:RegisterWatch(buttonInterface, function(widget, param, sourceEntity, targEntity)
		sourceIndex = tonumber(sourceEntity)

		widget:SetCallback('onclick', function(widget)
			SendScriptMessage(buttonVariable, index, sourceIndex)
			PlaySound('/shared/sounds/ui/button_click_06.wav')
		end)
	end)
end

-- If you want a button to Glow by default (for more attention)
local function initializeChoiceButtonGlowing(index, buttonInterface, button, overlay, text, buttonVariable)
	button:SetCallback('onmouseover', function(widget)
		interface:GetWidget(overlay):SetVisible(true)
		interface:GetWidget(text):SetVisible(true)
		PlaySound('/shared/sounds/ui/button_over_0%.wav')
	end)
	button:SetCallback('onmouseout', function(widget)
		interface:GetWidget(overlay):SetVisible(false)
		interface:GetWidget(text):SetVisible(false)
	end)

	button:RegisterWatch(buttonInterface, function(widget, param, sourceEntity, targEntity)
		sourceIndex = tonumber(sourceEntity)

		widget:SetCallback('onclick', function(widget)
			SendScriptMessage(buttonVariable, index, sourceIndex)
			PlaySound('/shared/sounds/ui/button_click_06.wav')
		end)
	end)
end

-- Initiallize all custom buttons either with initializeChoiceButton or initializeChoiceButtonGlowing

local rightSidedMinimap = 0
local buttonPositionTable = {}
local buttonWidgetTable = {}
local buttonVisibilityTable = {}
buttonVisibilityTable[1] = false
buttonVisibilityTable[2] = false
buttonVisibilityTable[3] = false
buttonVisibilityTable[4] = false
buttonVisibilityTable[5] = false
buttonVisibilityTable[6] = false
buttonVisibilityTable[7] = false
buttonVisibilityTable[8] = false

local function assignCustomButtonVisibility(widget, buttonVariable)
	local tempPosition = 0
	local i = 0
	while (i < 8) do
		i = i + 1
		if buttonVisibilityTable[i] == false then
			buttonVisibilityTable[i] = true
			buttonWidgetTable[i] = widget
			tempPosition = rightSidedMinimap * 6
			tempPosition = (tempPosition + 22 + (8 * i)) * (- 1)
			interface:GetWidget(widget):SetY(tempPosition .. "h")
			buttonPositionTable[buttonVariable] = i
			i = 8
		end
	end
end

local function clearCustomButtonVisibility(buttonVariable)
	local tempPosition = 0
	local tempTarget = 0
	local i = 0
	local k = 0
	local m = 0
	while (i < 8) do
		i = i + 1
		if buttonPositionTable[buttonVariable] == i then
			k = i
			buttonVisibilityTable[k] = false
			while (k < 8) do
				buttonVisibilityTable[k] = buttonVisibilityTable[k+1]
				buttonWidgetTable[k] = buttonWidgetTable[k+1]
				if buttonVisibilityTable[k+1] == true then
					while (m < 8) do
						m = m + 1
						if buttonPositionTable[m] == (k + 1) then
							buttonPositionTable[m] = k
							tempPosition = rightSidedMinimap * 6
							tempPosition = (tempPosition + 22 + (8 * k)) * (- 1)
							interface:GetWidget(buttonWidgetTable[k]):SetY(tempPosition .. "h")
							m = 8
						end
					end
					m = 0
				end
				buttonVisibilityTable[k+1] = false
				k = k + 1
			end
			i = 8
		end
	end
	
	buttonPositionTable[buttonVariable] = 0
end

local function updateCustomButtonPositions()
	local tempPosition = 0
	local i = 0
	while (i < 8) do
		i = i + 1
		if buttonVisibilityTable[i] == true then
			tempPosition = rightSidedMinimap * 6
			tempPosition = (tempPosition + 22 + (8 * i)) * (- 1)
			interface:GetWidget(buttonWidgetTable[i]):SetY(tempPosition .. "h")
		end
	end
end
----------------------------------------
-- Above code doesn't need to be changed
----------------------------------------




-- Rally button
-- Rally button is assigned the position "1" in the table
buttonPositionTable[1] = 0

interface:GetWidget('custom_button_Rally_Panel'):RegisterWatch('GameInfo', function(widget, worldName, gameModeName, bossbuffInfoString)
	interface:GetWidget('custom_button_Rally_Background'):SetVisible(true)
	interface:GetWidget('custom_button_Rally_Button1'):SetVisible(true)
	interface:GetWidget('custom_button_Rally_SelectChoice1GlowEffect'):SetVisible(false) -- Set this to true if glowing
	interface:GetWidget('custom_button_Rally_SelectChoice1Text'):SetVisible(false)
end)

initializeChoiceButton(1,'custom_button_Rally', interface:GetWidget('custom_button_Rally_Button1'), 'custom_button_Rally_SelectChoice1Glow', 'custom_button_Rally_SelectChoice1GlowEffect','custom_button_Rally_SelectChoice1Text','custom_button_Rally_selected')
-- initializeChoiceButtonGlowing(1,'custom_button_Rally', interface:GetWidget('custom_button_Rally_Button1'), 'custom_button_Rally_SelectChoice1Glow','custom_button_Rally_SelectChoice1Text','custom_button_Rally_selected')
-- Above is just an example of how to initialize a glowing button

interface:GetWidget('custom_button_Rally_Panel'):RegisterWatch('custom_button_Rally', function(widget)
	-- 1 here is the position of the rally button in the table
	if buttonPositionTable[1] == 0 then
		assignCustomButtonVisibility('custom_button_Rally_Panel', 1)
	end
	
	-- Check if there has been a change to make the minimap right-sided
	if (GetCvarBool('ui_minimap_rightside')) then
		if (rightSidedMinimap == 0) then
			rightSidedMinimap = 1
			updateCustomButtonPositions()
		end
	else
		if (rightSidedMinimap == 1) then
			rightSidedMinimap = 0
			updateCustomButtonPositions()
		end
	end
	
	widget:SetVisible(true)
end)

interface:GetWidget('custom_button_Rally_Panel'):RegisterWatch('custom_button_Rally_hide', function(widget)
	widget:SetVisible(false)
	-- 1 here is the position of the rally button in the table
	if not(buttonPositionTable[1] == 0) then
		clearCustomButtonVisibility(1)
	end
end)

interface:GetWidget('custom_button_Rally_Panel'):RegisterWatch('GamePhase', function(widget)
	widget:SetVisible(false)
	-- 1 here is the position of the rally button in the table
	if not(buttonPositionTable[1] == 0) then
		clearCustomButtonVisibility(1)
	end
end)
-- End of Rally button

-- Shellshock button
buttonPositionTable[2] = 0

interface:GetWidget('custom_button_Shellshock_Panel'):RegisterWatch('GameInfo', function(widget, worldName, gameModeName, bossbuffInfoString)
	interface:GetWidget('custom_button_Shellshock_Background'):SetVisible(true)
	interface:GetWidget('custom_button_Shellshock_Button1'):SetVisible(true)
	interface:GetWidget('custom_button_Shellshock_SelectChoice1GlowEffect'):SetVisible(false)
	interface:GetWidget('custom_button_Shellshock_SelectChoice1Text'):SetVisible(false)
end)

initializeChoiceButton(1,'custom_button_Shellshock', interface:GetWidget('custom_button_Shellshock_Button1'), 'custom_button_Shellshock_SelectChoice1Glow', 'custom_button_Shellshock_SelectChoice1GlowEffect','custom_button_Shellshock_SelectChoice1Text','custom_button_Shellshock_selected')

interface:GetWidget('custom_button_Shellshock_Panel'):RegisterWatch('custom_button_Shellshock', function(widget)
	if buttonPositionTable[2] == 0 then
		assignCustomButtonVisibility('custom_button_Shellshock_Panel', 2)
	end
	
	-- Check if there has been a change to make the minimap right-sided
	if (GetCvarBool('ui_minimap_rightside')) then
		if (rightSidedMinimap == 0) then
			rightSidedMinimap = 1
			updateCustomButtonPositions()
		end
	else
		if (rightSidedMinimap == 1) then
			rightSidedMinimap = 0
			updateCustomButtonPositions()
		end
	end
	
	widget:SetVisible(true)
end)

interface:GetWidget('custom_button_Shellshock_Panel'):RegisterWatch('custom_button_Shellshock_hide', function(widget)
	widget:SetVisible(false)
	if not(buttonPositionTable[2] == 0) then
		clearCustomButtonVisibility(2)
	end
end)

interface:GetWidget('custom_button_Shellshock_Panel'):RegisterWatch('GamePhase', function(widget)
	widget:SetVisible(false)
	if not(buttonPositionTable[2] == 0) then
		clearCustomButtonVisibility(2)
	end
end)
-- End of Shellshock button

-- Armadon button
buttonPositionTable[3] = 0

interface:GetWidget('custom_button_Armadon_Panel'):RegisterWatch('GameInfo', function(widget, worldName, gameModeName, bossbuffInfoString)
	interface:GetWidget('custom_button_Armadon_Background'):SetVisible(true)
	interface:GetWidget('custom_button_Armadon_Button1'):SetVisible(true)
	interface:GetWidget('custom_button_Armadon_SelectChoice1GlowEffect'):SetVisible(false)
	interface:GetWidget('custom_button_Armadon_SelectChoice1Text'):SetVisible(false)
end)

initializeChoiceButton(1,'custom_button_Armadon', interface:GetWidget('custom_button_Armadon_Button1'), 'custom_button_Armadon_SelectChoice1Glow', 'custom_button_Armadon_SelectChoice1GlowEffect','custom_button_Armadon_SelectChoice1Text','custom_button_Armadon_selected')

interface:GetWidget('custom_button_Armadon_Panel'):RegisterWatch('custom_button_Armadon', function(widget)
	if buttonPositionTable[3] == 0 then
		assignCustomButtonVisibility('custom_button_Armadon_Panel', 3)
	end
	
	-- Check if there has been a change to make the minimap right-sided
	if (GetCvarBool('ui_minimap_rightside')) then
		if (rightSidedMinimap == 0) then
			rightSidedMinimap = 1
			updateCustomButtonPositions()
		end
	else
		if (rightSidedMinimap == 1) then
			rightSidedMinimap = 0
			updateCustomButtonPositions()
		end
	end
	
	widget:SetVisible(true)
end)

interface:GetWidget('custom_button_Armadon_Panel'):RegisterWatch('custom_button_Armadon_hide', function(widget)
	widget:SetVisible(false)
	if not(buttonPositionTable[3] == 0) then
		clearCustomButtonVisibility(3)
	end
end)

interface:GetWidget('custom_button_Armadon_Panel'):RegisterWatch('GamePhase', function(widget)
	widget:SetVisible(false)
	if not(buttonPositionTable[3] == 0) then
		clearCustomButtonVisibility(3)
	end
end)
-- End of Armadon button

-- Ophelia's Pact Quest reward button 1
buttonPositionTable[4] = 0

interface:GetWidget('custom_button_OpheliaQuest1_Panel'):RegisterWatch('GameInfo', function(widget, worldName, gameModeName, bossbuffInfoString)
	interface:GetWidget('custom_button_OpheliaQuest1_Background'):SetVisible(true)
	interface:GetWidget('custom_button_OpheliaQuest1_Button1'):SetVisible(true)
	interface:GetWidget('custom_button_OpheliaQuest1_SelectChoice1GlowEffect'):SetVisible(false)
	interface:GetWidget('custom_button_OpheliaQuest1_SelectChoice1Text'):SetVisible(false)
end)

initializeChoiceButton(1,'custom_button_OpheliaQuest1', interface:GetWidget('custom_button_OpheliaQuest1_Button1'), 'custom_button_OpheliaQuest1_SelectChoice1Glow', 'custom_button_OpheliaQuest1_SelectChoice1GlowEffect','custom_button_OpheliaQuest1_SelectChoice1Text','custom_button_OpheliaQuest1_selected')

interface:GetWidget('custom_button_OpheliaQuest1_Panel'):RegisterWatch('custom_button_OpheliaQuest1', function(widget)
	if buttonPositionTable[4] == 0 then
		assignCustomButtonVisibility('custom_button_OpheliaQuest1_Panel', 4)
	end
	
	-- Check if there has been a change to make the minimap right-sided
	if (GetCvarBool('ui_minimap_rightside')) then
		if (rightSidedMinimap == 0) then
			rightSidedMinimap = 1
			updateCustomButtonPositions()
		end
	else
		if (rightSidedMinimap == 1) then
			rightSidedMinimap = 0
			updateCustomButtonPositions()
		end
	end
	
	widget:SetVisible(true)
end)

interface:GetWidget('custom_button_OpheliaQuest1_Panel'):RegisterWatch('custom_button_OpheliaQuest1_hide', function(widget)
	widget:SetVisible(false)
	if not(buttonPositionTable[4] == 0) then
		clearCustomButtonVisibility(4)
	end
end)

interface:GetWidget('custom_button_OpheliaQuest1_Panel'):RegisterWatch('GamePhase', function(widget)
	widget:SetVisible(false)
	if not(buttonPositionTable[4] == 0) then
		clearCustomButtonVisibility(4)
	end
end)
-- End of OpheliaQuest1 button

-- Ophelia's Pact Quest reward button 2
buttonPositionTable[5] = 0

interface:GetWidget('custom_button_OpheliaQuest2_Panel'):RegisterWatch('GameInfo', function(widget, worldName, gameModeName, bossbuffInfoString)
	interface:GetWidget('custom_button_OpheliaQuest2_Background'):SetVisible(true)
	interface:GetWidget('custom_button_OpheliaQuest2_Button1'):SetVisible(true)
	interface:GetWidget('custom_button_OpheliaQuest2_SelectChoice1GlowEffect'):SetVisible(false)
	interface:GetWidget('custom_button_OpheliaQuest2_SelectChoice1Text'):SetVisible(false)
end)

initializeChoiceButton(1,'custom_button_OpheliaQuest2', interface:GetWidget('custom_button_OpheliaQuest2_Button1'), 'custom_button_OpheliaQuest2_SelectChoice1Glow', 'custom_button_OpheliaQuest2_SelectChoice1GlowEffect','custom_button_OpheliaQuest2_SelectChoice1Text','custom_button_OpheliaQuest2_selected')

interface:GetWidget('custom_button_OpheliaQuest2_Panel'):RegisterWatch('custom_button_OpheliaQuest2', function(widget)
	if buttonPositionTable[5] == 0 then
		assignCustomButtonVisibility('custom_button_OpheliaQuest2_Panel', 5)
	end
	
	-- Check if there has been a change to make the minimap right-sided
	if (GetCvarBool('ui_minimap_rightside')) then
		if (rightSidedMinimap == 0) then
			rightSidedMinimap = 1
			updateCustomButtonPositions()
		end
	else
		if (rightSidedMinimap == 1) then
			rightSidedMinimap = 0
			updateCustomButtonPositions()
		end
	end
	
	widget:SetVisible(true)
end)

interface:GetWidget('custom_button_OpheliaQuest2_Panel'):RegisterWatch('custom_button_OpheliaQuest2_hide', function(widget)
	widget:SetVisible(false)
	if not(buttonPositionTable[5] == 0) then
		clearCustomButtonVisibility(5)
	end
end)

interface:GetWidget('custom_button_OpheliaQuest2_Panel'):RegisterWatch('GamePhase', function(widget)
	widget:SetVisible(false)
	if not(buttonPositionTable[5] == 0) then
		clearCustomButtonVisibility(5)
	end
end)
-- End of OpheliaQuest2 button

-- Ophelia's Pact Quest reward button 3
buttonPositionTable[6] = 0

interface:GetWidget('custom_button_OpheliaQuest3_Panel'):RegisterWatch('GameInfo', function(widget, worldName, gameModeName, bossbuffInfoString)
	interface:GetWidget('custom_button_OpheliaQuest3_Background'):SetVisible(true)
	interface:GetWidget('custom_button_OpheliaQuest3_Button1'):SetVisible(true)
	interface:GetWidget('custom_button_OpheliaQuest3_SelectChoice1GlowEffect'):SetVisible(false)
	interface:GetWidget('custom_button_OpheliaQuest3_SelectChoice1Text'):SetVisible(false)
end)

initializeChoiceButton(1,'custom_button_OpheliaQuest3', interface:GetWidget('custom_button_OpheliaQuest3_Button1'), 'custom_button_OpheliaQuest3_SelectChoice1Glow', 'custom_button_OpheliaQuest3_SelectChoice1GlowEffect','custom_button_OpheliaQuest3_SelectChoice1Text','custom_button_OpheliaQuest3_selected')

interface:GetWidget('custom_button_OpheliaQuest3_Panel'):RegisterWatch('custom_button_OpheliaQuest3', function(widget)
	if buttonPositionTable[6] == 0 then
		assignCustomButtonVisibility('custom_button_OpheliaQuest3_Panel', 6)
	end
	
	-- Check if there has been a change to make the minimap right-sided
	if (GetCvarBool('ui_minimap_rightside')) then
		if (rightSidedMinimap == 0) then
			rightSidedMinimap = 1
			updateCustomButtonPositions()
		end
	else
		if (rightSidedMinimap == 1) then
			rightSidedMinimap = 0
			updateCustomButtonPositions()
		end
	end
	
	widget:SetVisible(true)
end)

interface:GetWidget('custom_button_OpheliaQuest3_Panel'):RegisterWatch('custom_button_OpheliaQuest3_hide', function(widget)
	widget:SetVisible(false)
	if not(buttonPositionTable[6] == 0) then
		clearCustomButtonVisibility(6)
	end
end)

interface:GetWidget('custom_button_OpheliaQuest3_Panel'):RegisterWatch('GamePhase', function(widget)
	widget:SetVisible(false)
	if not(buttonPositionTable[6] == 0) then
		clearCustomButtonVisibility(6)
	end
end)
-- End of OpheliaQuest3 button
