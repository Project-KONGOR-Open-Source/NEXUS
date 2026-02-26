lib_button.addVisualStateHandler(
	'simpleDepress',			-- Handler name
	{ 'Body' },					-- Required widgets
	{					-- Table of states by name
		up			= function(buttonInfo, oldState)
			buttonInfo.widgets.Body:SetX(0)
			buttonInfo.widgets.Body:SetY(0)
		end,
		over		= function(buttonInfo, oldState)
			buttonInfo.widgets.Body:SetX(0)
			buttonInfo.widgets.Body:SetY(0)
		end,
		down		= function(buttonInfo, oldState)
			buttonInfo.widgets.Body:SetX(1)
			buttonInfo.widgets.Body:SetY(1)
		end,
		disabled	= function(buttonInfo, oldState)
			buttonInfo.widgets.Body:SetX(0)
			buttonInfo.widgets.Body:SetY(0)
		end
	},
	function(buttonInfo)	-- validator
		return true
	end,
	nil	-- Extra stuff, required in table format (buttonInfo.visualStateHandler.whatever)
)