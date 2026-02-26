
local function ReplaceFrameTexture(frame, state)
	if not frame then return end
	local texture = frame:GetTexture()
	if texture and texture ~= '' then
		texture = string.match(texture,'(.*_)[^_]+%.tga')
		texture = texture..state..'.tga'
	else
		texture = '/ui/elements/button_bevel_disabled.tga'
	end
	frame:SetTexture(texture)
end

lib_button.addVisualStateHandler(
	'simpleAbility',						-- Handler name
	{ 'Body', 'Frame' },					-- Required widgets
	{					-- Table of states by name
		up			= function(buttonInfo, oldState)
			buttonInfo.widgets.Body:SetX(0)
			buttonInfo.widgets.Body:SetY(0)
			ReplaceFrameTexture(buttonInfo.widgets.Frame, 'up')
		end,
		over		= function(buttonInfo, oldState)
			buttonInfo.widgets.Body:SetX(0)
			buttonInfo.widgets.Body:SetY(0)
			ReplaceFrameTexture(buttonInfo.widgets.Frame, 'over')
		end,
		down		= function(buttonInfo, oldState)
			buttonInfo.widgets.Body:SetX(1)
			buttonInfo.widgets.Body:SetY(1)
			ReplaceFrameTexture(buttonInfo.widgets.Frame, 'down')
		end,
		disabled	= function(buttonInfo, oldState)
			buttonInfo.widgets.Body:SetX(0)
			buttonInfo.widgets.Body:SetY(0)
			ReplaceFrameTexture(buttonInfo.widgets.Frame, 'disabled')
		end
	},
	function(buttonInfo)	-- validator
		return true
	end,
	nil	-- Extra stuff, required in table format (buttonInfo.visualStateHandler.whatever)
)