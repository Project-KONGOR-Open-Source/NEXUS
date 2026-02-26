-- Lane selection

local interface = object

interface:GetWidget('laneSelect'):RegisterWatch('GameInfo', function(widget, worldName, gameModeName, laneInfoString)
	local laneCount = 0	-- code not there yet

	local laneInfo = Explode(',', laneInfoString)

	local topExists	= false
	local midExists	= false
	local botExists	= false

	for k,v in ipairs(laneInfo) do
		if v == 'top' then
			topExists = true
			laneCount = laneCount + 1
		elseif v == 'middle' then
			midExists = true
			laneCount = laneCount + 1
		elseif v == 'bottom' then
			botExists = true
			laneCount = laneCount + 1
		end
	end

	-- print('================ lane string is '..laneInfoString..'\n')

	interface:GetWidget('laneSelectLane1Cover'):SetVisible(not topExists)
	interface:GetWidget('laneSelectLane2Cover'):SetVisible(not midExists and laneCount >= 1)
	interface:GetWidget('laneSelectLane3Cover'):SetVisible(not botExists)
	interface:GetWidget('laneSelectLane0'):SetVisible(laneCount == 0)

	interface:GetWidget('laneSelectLane1Button'):SetVisible(topExists)
	interface:GetWidget('laneSelectLane2Button'):SetVisible(midExists)
	interface:GetWidget('laneSelectLane3Button'):SetVisible(botExists)
	interface:GetWidget('laneSelectLane0Button'):SetVisible(laneCount == 0)
end)

local function initializeLaneButton(index, button, overlay, glowEffect)
	button:SetCallback('onmouseover', function(widget)
		interface:GetWidget(overlay):SetVisible(true)
		interface:GetWidget(glowEffect):SetVisible(true)
		PlaySound('/ui/common/models/toggle/sounds/ui_toggle_%.wav')
	end)
	button:SetCallback('onmouseout', function(widget)
		interface:GetWidget(overlay):SetVisible(false)
		interface:GetWidget(glowEffect):SetVisible(false)
	end)

	button:SetCallback('onhide', function(widget)
		interface:GetWidget(overlay):SetVisible(false)
		interface:GetWidget(glowEffect):SetVisible(false)
	end)

	button:RegisterWatch('rattletrap_lane_choice', function(widget, param, sourceEntity, targEntity)
		sourceIndex = tonumber(sourceEntity)

		widget:SetCallback('onclick', function(widget)
			SendScriptMessage('rattletrap_lane_selected_'..index, '1', sourceIndex)
		end)
	end)
end

initializeLaneButton(1, interface:GetWidget('laneSelectLane1Button'), 'laneSelectLane1Glow', 'laneSelectLane1GlowEffect')
initializeLaneButton(2, interface:GetWidget('laneSelectLane2Button'), 'laneSelectLane2Glow', 'laneSelectLane2GlowEffect')
initializeLaneButton(3, interface:GetWidget('laneSelectLane3Button'), 'laneSelectLane3Glow', 'laneSelectLane3GlowEffect')
initializeLaneButton(1, interface:GetWidget('laneSelectLane0Button'), 'laneSelectLane0Glow', 'laneSelectLane0GlowEffect')


local lastSourceIndex = -1

interface:GetWidget('laneSelect'):RegisterWatch('rattletrap_lane_choice', function(widget, param, sourceEntity, targEntity)
	widget:SetVisible(true)

	lastSourceIndex = tonumber(sourceEntity)
end)

interface:GetWidget('laneSelect'):RegisterWatch('rattletrap_lane_choice_closed', function(widget)
	widget:SetVisible(false)
end)

interface:GetWidget('laneSelect'):RegisterWatch('GamePhase', function(widget)
	widget:SetVisible(false)
end)

function laneSelectClose()
	SendScriptMessage('rattletrap_lane_close', '1', lastSourceIndex)
end

interface:GetWidget('laneSelectClose'):RegisterWatch('rattletrap_lane_choice', function(widget, param, sourceEntity, targEntity)
	sourceIndex = tonumber(sourceEntity)

	widget:SetCallback('onclick', function(widget)
		SendScriptMessage('rattletrap_lane_close', '1', sourceIndex)
	end)
end)