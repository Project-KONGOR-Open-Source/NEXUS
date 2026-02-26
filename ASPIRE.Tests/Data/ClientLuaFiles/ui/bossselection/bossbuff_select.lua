-- Boss Lane selection

local interface = object

interface:GetWidget('bossbuffSelect'):RegisterWatch('GameInfo', function(widget, worldName, gameModeName, bossbuffInfoString)
	interface:GetWidget('bossbuffSelectChoice0Text'):SetVisible(false)
	interface:GetWidget('bossbuffSelectChoice1Text'):SetVisible(false)
	interface:GetWidget('bossbuffSelectChoice2Text'):SetVisible(false)
	interface:GetWidget('bossbuffSelectChoice3Text'):SetVisible(false)

	interface:GetWidget('bossbuff_Minimized'):SetVisible(true)
	interface:GetWidget('bossbuffSelectChoice0Button'):SetVisible(true)
	interface:GetWidget('bossbuffSelectChoice0GlowEffect'):SetVisible(true)
	interface:GetWidget('bossbuffSelectChoice1Button'):SetVisible(false)
	interface:GetWidget('bossbuffSelectChoice2Button'):SetVisible(false)
	interface:GetWidget('bossbuffSelectChoice3Button'):SetVisible(false)
	interface:GetWidget('bossbuffSelectClose'):SetVisible(false)
	interface:GetWidget('bossbuff_Background'):SetVisible(false)
end)

local function initializeChoiceButton(index, button, overlay, glowEffect, text)
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

	button:RegisterWatch('bossbuff_choice', function(widget, param, sourceEntity, targEntity)
		sourceIndex = tonumber(sourceEntity)

		widget:SetCallback('onclick', function(widget)
			SendScriptMessage('bossbuff_selected', index, sourceIndex)
			PlaySound('/shared/sounds/ui/button_click_06.wav')
		end)
	end)
end

local function initializeChoiceButtonGlowing(index, button, overlay, text)
	button:SetCallback('onmouseover', function(widget)
		interface:GetWidget(overlay):SetVisible(true)
		interface:GetWidget(text):SetVisible(true)
		PlaySound('/shared/sounds/ui/button_over_0%.wav')
	end)
	button:SetCallback('onmouseout', function(widget)
		interface:GetWidget(overlay):SetVisible(false)
		interface:GetWidget(text):SetVisible(false)
	end)

	button:RegisterWatch('bossbuff_choice', function(widget, param, sourceEntity, targEntity)
		sourceIndex = tonumber(sourceEntity)

		widget:SetCallback('onclick', function(widget)
			SendScriptMessage('bossbuff_selected', index, sourceIndex)
			PlaySound('/shared/sounds/ui/button_click_02.wav')
		end)
	end)
end

initializeChoiceButton(1, interface:GetWidget('bossbuffSelectChoice1Button'), 'bossbuffSelectChoice1Glow', 'bossbuffSelectChoice1GlowEffect','bossbuffSelectChoice1Text')
initializeChoiceButton(2, interface:GetWidget('bossbuffSelectChoice2Button'), 'bossbuffSelectChoice2Glow', 'bossbuffSelectChoice2GlowEffect','bossbuffSelectChoice2Text')
initializeChoiceButton(3, interface:GetWidget('bossbuffSelectChoice3Button'), 'bossbuffSelectChoice3Glow', 'bossbuffSelectChoice3GlowEffect','bossbuffSelectChoice3Text')
initializeChoiceButtonGlowing(4, interface:GetWidget('bossbuffSelectChoice0Button'), 'bossbuffSelectChoice0Glow', 'bossbuffSelectChoice0Text')


local lastSourceIndex = -1
local rightSidedMinimap = 0

interface:GetWidget('bossbuffSelect'):RegisterWatch('bossbuff_choice', function(widget, param, sourceEntity, targEntity)
	-- Check if there has been a change to make the minimap right-sided
	if (GetCvarBool('ui_minimap_rightside')) then
		if (rightSidedMinimap == 0) then
			rightSidedMinimap = 1
			widget:SetY("-28h")
		end
	else
		if (rightSidedMinimap == 1) then
			rightSidedMinimap = 0
			widget:SetY("-20h")
		end
	end
	widget:SetVisible(true)
	lastSourceIndex = tonumber(sourceEntity)
end)

interface:GetWidget('bossbuffSelect'):RegisterWatch('bossbuff_choice_open', function(widget)
	interface:GetWidget('bossbuff_Minimized'):SetVisible(false)
	interface:GetWidget('bossbuffSelectChoice0Button'):SetVisible(false)
	interface:GetWidget('bossbuffSelectChoice0GlowEffect'):SetVisible(false)
	interface:GetWidget('bossbuffSelectChoice1Button'):SetVisible(true)
	interface:GetWidget('bossbuffSelectChoice2Button'):SetVisible(true)
	interface:GetWidget('bossbuffSelectChoice3Button'):SetVisible(true)
	interface:GetWidget('bossbuffSelectClose'):SetVisible(true)
	interface:GetWidget('bossbuff_Background'):SetVisible(true)
end)

interface:GetWidget('bossbuffSelect'):RegisterWatch('bossbuff_choice_close', function(widget)
	interface:GetWidget('bossbuff_Minimized'):SetVisible(true)
	interface:GetWidget('bossbuffSelectChoice0Button'):SetVisible(true)
	interface:GetWidget('bossbuffSelectChoice0GlowEffect'):SetVisible(true)
	interface:GetWidget('bossbuffSelectChoice1Button'):SetVisible(false)
	interface:GetWidget('bossbuffSelectChoice2Button'):SetVisible(false)
	interface:GetWidget('bossbuffSelectChoice3Button'):SetVisible(false)
	interface:GetWidget('bossbuffSelectClose'):SetVisible(false)
	interface:GetWidget('bossbuff_Background'):SetVisible(false)
end)

interface:GetWidget('bossbuffSelect'):RegisterWatch('bossbuff_choice_over', function(widget)
	widget:SetVisible(false)
end)

interface:GetWidget('bossbuffSelect'):RegisterWatch('GamePhase', function(widget)
	SendScriptMessage('bossbuff_selected', 6, lastSourceIndex)
	widget:SetVisible(false)
end)

interface:GetWidget('bossbuffSelectClose'):RegisterWatch('bossbuff_choice', function(widget, param, sourceEntity, targEntity)
	sourceIndex = tonumber(sourceEntity)
	widget:SetCallback('onclick', function(widget)
		interface:GetWidget('bossbuff_Minimized'):SetVisible(true)
		interface:GetWidget('bossbuffSelectChoice0Button'):SetVisible(true)
		interface:GetWidget('bossbuffSelectChoice0GlowEffect'):SetVisible(true)
		interface:GetWidget('bossbuffSelectChoice1Button'):SetVisible(false)
		interface:GetWidget('bossbuffSelectChoice2Button'):SetVisible(false)
		interface:GetWidget('bossbuffSelectChoice3Button'):SetVisible(false)
		interface:GetWidget('bossbuffSelectClose'):SetVisible(false)
		interface:GetWidget('bossbuff_Background'):SetVisible(false)
		PlaySound('/shared/sounds/ui/button_click_02.wav')
		SendScriptMessage('bossbuff_selected', 5, sourceIndex)
	end)
end)