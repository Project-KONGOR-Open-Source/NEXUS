lib_button.addVisualStateHandler(
	'button2',			-- Handler name
	{ 'Body', 'Background', 'Label', 'BackgroundCover' },					-- Required widgets
	{					-- Table of states by name
		up			= function(buttonInfo, oldState)
			buttonInfo.widgets.Label:SetColor(
				buttonInfo.labelR,
				buttonInfo.labelG,
				buttonInfo.labelB,
				buttonInfo.labelA
			)
			buttonInfo.widgets.Label:SetShadowColor(0,0,0)
			buttonInfo.widgets.Background:SetTexture('/ui/info/button.tga')
			buttonInfo.widgets.Background:SetColor(1,1,1)
			buttonInfo.widgets.Background:SetBorderColor(1,1,1)
			buttonInfo.widgets.BackgroundCover:SetVisible(false)
			buttonInfo.widgets.Body:SetX(0)
			buttonInfo.widgets.Body:SetY(0)
		end,
		over		= function(buttonInfo, oldState)
			buttonInfo.widgets.Label:SetColor(
				Saturate(buttonInfo.labelR * 1.1),
				Saturate(buttonInfo.labelG * 1.1),
				Saturate(buttonInfo.labelB * 1.1),
				buttonInfo.labelA
			)
			buttonInfo.widgets.Label:SetShadowColor('#7F6E00')
			buttonInfo.widgets.Background:SetTexture('/ui/info/button_over.tga')
			buttonInfo.widgets.Background:SetColor(1,1,1)
			buttonInfo.widgets.Background:SetBorderColor(1,1,1)
			buttonInfo.widgets.BackgroundCover:SetVisible(false)
			buttonInfo.widgets.Body:SetX(0)
			buttonInfo.widgets.Body:SetY(0)
		end,
		down		= function(buttonInfo, oldState)
			buttonInfo.widgets.Label:SetColor(
				Saturate(buttonInfo.labelR * 0.85),
				Saturate(buttonInfo.labelG * 0.85),
				Saturate(buttonInfo.labelB * 0.85),
				buttonInfo.labelA
			)
			buttonInfo.widgets.Label:SetShadowColor('#7F6E00')
			buttonInfo.widgets.Background:SetTexture('/ui/info/button_down.tga')
			buttonInfo.widgets.Background:SetColor(1,1,1)
			buttonInfo.widgets.Background:SetBorderColor(1,1,1)
			buttonInfo.widgets.BackgroundCover:SetVisible(false)
			buttonInfo.widgets.Body:SetX(1)
			buttonInfo.widgets.Body:SetY(1)
		end,
		disabled	= function(buttonInfo, oldState)
			buttonInfo.widgets.Label:SetColor(1, 1, 1, 1)
			buttonInfo.widgets.Label:SetShadowColor(0,0,0)
			buttonInfo.widgets.Background:SetTexture('/ui/frames/rounded_button_bg_white.tga')
			buttonInfo.widgets.Background:SetColor(0.3, 0.3, 0.3, 0.7)
			buttonInfo.widgets.Background:SetBorderColor(0.3, 0.3, 0.3, 0.7)
			buttonInfo.widgets.BackgroundCover:SetVisible(true)
			buttonInfo.widgets.Body:SetX(0)
			buttonInfo.widgets.Body:SetY(0)
		end
	},
	function(buttonInfo)	-- validator
		buttonInfo.labelR, buttonInfo.labelG, buttonInfo.labelB, buttonInfo.labelA	= buttonInfo.widgets.Label:GetColor()
		return true
	end,
	nil	-- Extra stuff, required in table format (buttonInfo.visualStateHandler.whatever)
)