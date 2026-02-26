local interface, interfaceName = object, object:GetName()

Main_Walkthrough = {}
Main_Walkthrough.Storage = {}
RegisterScript2('Main_Walkthrough', '30')

function Main_Walkthrough.ShowWalkthroughTextPanel(show, x, y, skippable, label, description, iconTexture, sound)
		
	if(show) then
		-- Set the parent to invisible so the onshow triggers when visible is set to true
		interface:GetWidget('walkthrough_tip'):SetVisible(false)
		
		interface:GetWidget('walkthrough_tip'):SetX(x)
		interface:GetWidget('walkthrough_tip'):SetY(y)
		
		
		interface:GetWidget('walkthrough_tip'):SetNoClick(skippable)
		
		interface:GetWidget('lobby_walkthrough_tip_icon_label'):SetText(Translate(label))
		interface:GetWidget('lobby_walkthrough_tip_icon_desc'):SetText(Translate(description))
		
		if(iconTexture ~= '') then
			interface:GetWidget('lobby_walkthrough_tip_icon'):SetTexture(iconTexture)
		else
			interface:GetWidget('lobby_walkthrough_tip_icon'):SetTexture(Translate('walkthrough_tip_default_icon'))
		end
		interface:GetWidget('lobby_walkthrough_tip_icon'):SetVisible(true)
		interface:GetWidget('walkthrough_tip'):SetVisible(true)
		interface:GetWidget('walkthrough_tip'):BringToFront()
		interface:GetWidget('lobby_walkthrough_tip_icon_label'):SetVisible(true)
		interface:GetWidget('lobby_walkthrough_tip_icon_desc'):SetVisible(true)	
		
		Main_Walkthrough.PlayTranslatedSound(sound)
		
	else
		interface:GetWidget('walkthrough_tip'):FadeOut(250)
	end
end

function Main_Walkthrough.PlayTranslatedSound(path)

	local language = GetCvarString('host_language')
	local localizedPath = string.gsub(path, 'LANGUAGE', language)
	
	if(localizedPath ~= '') then
		interface:UICmd("PlaySoundDampen('"..localizedPath.."', '.5')")
	end
end

local function AnimatePointerIn(pointerWidget, x, y)
	--println('AnimatePointerIn')
	pointerWidget:SlideX(pointerWidget:GetX() - (x * (pointerWidget:GetWidth())), 800)
	pointerWidget:SlideY(pointerWidget:GetY() - (y * (pointerWidget:GetHeight())), 800)
	pointerWidget:Sleep(840, function() pointerWidget:DoEvent() end)
end

local function AnimatePointerOut(pointerWidget, x, y)
	--println('AnimatePointerOut')
	pointerWidget:SlideX(pointerWidget:GetX() + (x * (pointerWidget:GetWidth())), 800)
	pointerWidget:SlideY(pointerWidget:GetY() + (y * (pointerWidget:GetHeight())), 800)
	pointerWidget.onevent = function() AnimatePointerOut(pointerWidget, x, y) end
	pointerWidget:RefreshCallbacks()	
	pointerWidget:Sleep(840, function() AnimatePointerIn(pointerWidget, x, y) end)
end

function Main_Walkthrough.PointAtWidgetSleep(targetWidget, doPointer, pointerLabel, forcePointerSideValue, sleepTime)
	
	local pointer		= GetWidget('main_widget_highlighter_pointer', interfaceName)
	pointer:SetVisible(false)
	pointer:Sleep(sleepTime, function() Main_Walkthrough.PointAtWidget(targetWidget, doPointer, pointerLabel, forcePointerSideValue) end)
end


function Main_Walkthrough.PointAtWidget(targetWidget, doPointer, pointerLabel, forcePointerSideValue)
	local pointer		= GetWidget('main_widget_highlighter_pointer', interfaceName)
	local label_frame 	= GetWidget('main_widget_highlighter_label_frame', interfaceName)
	local label 		= GetWidget('main_widget_highlighter_label', interfaceName)
	
	if (doPointer) and (targetWidget) then
		pointer:FadeIn(1000)
		if (pointerLabel) and NotEmpty(pointerLabel) then
			label_frame:FadeIn(1000)
			label_frame:BringToFront()
			label:SetText(Translate(pointerLabel))
		else
			label_frame:SetVisible(false)	
		end
		pointer:BringToFront()
		local screenWidth = tonumber(targetWidget:UICmd("GetScreenWidth()"))
		local screenHeight = tonumber(targetWidget:UICmd("GetScreenHeight()"))
		local x = targetWidget:GetAbsoluteX()
		local y = targetWidget:GetAbsoluteY()
		local targetWidth = targetWidget:GetWidth()
		local targetHeight = targetWidget:GetHeight()		
		local labelWidth = label_frame:GetWidth()
		local labelHeight = label_frame:GetHeight()
		local pointerWidth = pointer:GetWidth()
		local pointerHeight = pointer:GetHeight()
		
		--Echo('PointAtWidget - x: '..x.. ' y: '..y..' screenW: '..screenWidth..' screenH: '..screenHeight.. ' WidgetName: '..targetWidget:GetName())
		
		if((x > screenWidth / 2 or forcePointerSideValue == '1' or forcePointerSideValue == '2') and  forcePointerSideValue ~= '3' and forcePointerSideValue ~= '4') then	
			pointer:SetX(x + targetWidth/2 - pointerWidth)	
			label_frame:SetX(x + (-1 * (labelWidth + (pointerWidth * 2.1))) )		
			
			if (y > screenHeight / 2 or forcePointerSideValue == '1' and forcePointerSideValue ~= '2') then
				pointer:SetRotation('135')				
				pointer:SetY(y + targetHeight/2 - pointerHeight)	
				label_frame:SetY(y + (-1 * (labelHeight + (pointerHeight * 2.1))) )				
				AnimatePointerOut(pointer, -0.5, -0.5)
			else
				pointer:SetRotation('45')
				pointer:SetY(y + targetHeight/2)
				label_frame:SetY(y + (1 * (targetHeight + (pointerHeight * 2.1))) )
				AnimatePointerOut(pointer, -0.5, 0.5)
			end
		else	
			pointer:SetX(x + targetWidth/2)
			label_frame:SetX(x + (1 * (targetWidth + (pointerWidth * 2.1))) )
			
			if (y > screenHeight / 2  or forcePointerSideValue == '3' and forcePointerSideValue ~= '4') then
				pointer:SetRotation('-135')				
				pointer:SetY(y + targetHeight/2 - pointerHeight)
				label_frame:SetY(y + (-1 * (targetHeight + (pointerHeight * 2.1))) )		
				AnimatePointerOut(pointer, 0.5, -0.5)
			else
				pointer:SetRotation('-45')
				pointer:SetY(y + targetHeight/2)
				label_frame:SetY(y + (1 * (targetHeight + (pointerHeight * 2.1))) )		
				AnimatePointerOut(pointer, 0.5, 0.5)
			end
		end
		
		--Echo('Pointer xy: (' .. pointer:GetAbsoluteX() .. ', ' .. pointer:GetAbsoluteY() .. ') Local: ' .. pointer:GetX() .. ', ' .. pointer:GetY() )
		
	else
		pointer.onevent = function() end
		pointer:RefreshCallbacks()
		pointer:SetVisible(false)	
		label_frame:SetVisible(false)	
	end
end