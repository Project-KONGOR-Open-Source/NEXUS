local interface, interfaceName = object, object:GetName()

Game_Walkthrough = {}
Game_Walkthrough.Storage = {}
Game_Walkthrough.Storage.highlightID = 0
Game_Walkthrough.Storage.MiscTipsUsed = {}

Game_Walkthrough.Storage.MiscTipTracker = {}
Game_Walkthrough.Storage.MiscTipTracker.trackedTipIDs = {}
Game_Walkthrough.Storage.MiscTipTracker.maxStoredTips = 5

Game_Walkthrough.Storage.questsLoaded = false
Game_Walkthrough.Storage.lvlUpWaitDuration = 45000
Game_Walkthrough.Storage.goldTipTriggerAmount = 1500
Game_Walkthrough.Storage.tipFlashDuration = 5000
Game_Walkthrough.Storage.tipFlashCycleDuration = 1000
--------------------------------------------------------------
--------------------------------------------------------------
--					Utility									--
--------------------------------------------------------------
--------------------------------------------------------------
function Game_Walkthrough.WrapText(textToWrap, widthAllowed)

	local currentWidth = 0
	local builtString = ''
	local spaceWidth = GetStringWidth('dyn_12', ' ')
	
	local splitText = split(textToWrap, ' ')
	
	for key, word in pairs(splitText) do
		local wordWidth = GetStringWidth('dyn_12', word)
		if(currentWidth + spaceWidth + wordWidth <= widthAllowed) then
			builtString = builtString .. ' '
			currentWidth = currentWidth + wordWidth + spaceWidth
		else
			builtString = builtString .. '\n'
			currentWidth = wordWidth
		end
		builtString = builtString .. word
	end
	
	-- Remove the starting ' ' which always exists
	builtString = builtString:sub(2)
	return builtString
end

--------------------------------------------------------------
--------------------------------------------------------------
--				Target Widget / Highlighting				--
--------------------------------------------------------------
--------------------------------------------------------------
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

local function PointAtWidget(targetWidget, highlightID, doPointer, pointerLabel)
	local pointer		= GetWidget('walkthrough_widget_highlighter_pointer'.. highlightID, interfaceName)
	local label_frame 	= GetWidget('walkthrough_widget_highlighter_label_frame'.. highlightID, interfaceName)
	local label 		= GetWidget('walkthrough_widget_highlighter_label'.. highlightID, interfaceName)
	
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
		
		if x > (screenWidth / 2) then	
			pointer:SetX(x + targetWidth/2 - pointerWidth)	
			label_frame:SetX(x + (-1 * (labelWidth + (pointerWidth * 2.1))) )		
			
			if y > (screenHeight / 2) then
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
			
			if y > (screenHeight / 2) then
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
		
		--Echo('Pointer xy: ' .. pointer:GetAbsoluteX() .. ', ' .. pointer:GetAbsoluteY())
		
	else
		pointer.onevent = function() end
		pointer:RefreshCallbacks()
		pointer:SetVisible(false)	
		label_frame:SetVisible(false)	
	end
end

-- parameter hilightWidgetID is optional
-- returns the highlightID for later manipulation or destruction
function Game_Walkthrough.HighlightWidget(self, widgetName, showHighlight, doPointer, pointerLabel, highlightWidgetID, targetInterfaceName, effectFile)
	
	local highlightID = nil
	if(highlightWidgetID == nil or highlightWidgetID == '') then
		highlightID = Game_Walkthrough.Storage.highlightID
		Game_Walkthrough.Storage.highlightID = Game_Walkthrough.Storage.highlightID + 1
		GetWidget('walkthrough_highlight_root', interfaceName):Instantiate('walkthrough_widget_highlighter_template', 'index', highlightID, 'effectFile', (effectFile or '/shared/effects/gold_sparks_246x78.effect'))
		--Echo('^780Highlight widget passed no ID. Set id to "'..highlightID..'" and created widget.')
	else
		highlightID = highlightWidgetID
		--Echo('^780Highlight widget passed ID "'..highlightID..'"')
	end

	
	if(targetInterfaceName == nil) then
		targetInterfaceName = 'game'
	end
	
	return Game_Walkthrough.HighlightWidgetInternal(self, widgetName, showHighlight, doPointer, pointerLabel, highlightID, targetInterfaceName)
end

function Game_Walkthrough.HighlightWidgetInternal(self, targetWidgetName, showHighlight, doPointer, pointerLabel, highlightID, targetInterfaceName)
		
	local highlightWidgetContainer = GetWidget('walkthrough_widget_highlighter_container'.. highlightID, interfaceName)
	local highlightWidget = GetWidget('walkthrough_widget_highlighter'.. highlightID, interfaceName)
	
	if(highlightWidget == nil) then
		Echo('^960HighlightWidget is invalid with ID "'.. highlightID..'".')
		return highlightID
	end
	
	if(highlightWidgetContainer == nil) then
		Echo('^960Highlight widget "walkthrough_widget_highlighter_container'.. highlightID..'" does not exist.')
		return highlightID
	end
	
	if (showHighlight) then			
		if (targetWidgetName) and GetWidget(targetWidgetName, targetInterfaceName) then
			printdb('^1911 HighlightWidget W: ' .. tostring(self) .. ' | ' .. tostring(targetWidgetName) .. ' | ' .. tostring(showHighlight) .. ' | ' .. tostring(pointerLabel) .. ' | ' .. tostring(targetInterfaceName) )
			
			local targetWidget = GetWidget(targetWidgetName, targetInterfaceName)
			
			highlightWidget:SetHeight(targetWidget:GetHeight())
			highlightWidget:SetWidth(targetWidget:GetWidth())
			highlightWidget:SetX(targetWidget:GetAbsoluteX())
			highlightWidget:SetY(targetWidget:GetAbsoluteY())
			highlightWidget:SetVisible(true)	
			PointAtWidget(targetWidget, highlightID, doPointer, pointerLabel)
			
		else
			println('^rHighlightTarget widget not found: ' .. tostring(targetWidgetName) )
		end
	else
		println('^oClearing HighlightWidget W: ' .. tostring(self) .. ' | ' .. tostring(targetWidgetName) .. ' | ' .. tostring(showHighlight) .. ' | ' .. tostring(pointerLabel) )
		highlightWidget:SetVisible(false)
		PointAtWidget(targetWidget, highlightID, false, '')
		highlightWidgetContainer:Destroy()
		return nil
	end
	
	return highlightID
end

function Game_Walkthrough.SetWidgetAsStepClickTarget(self, targetWidget)

	if(self == nil) then
		Echo('^900Failed to set widget as click target because the self parameter was nil.')
		return
	end

	if(targetWidget == nil) then
		Echo('^900Failed to set widget as click target for the targetWidget passed in because it is nil. Associated quest: "' .. self:GetName() .. '". TargetWidget: "' ..  Game_Walkthrough.Storage[self:GetName()].widgetToHighlight .. '"' )
		return
	end
	-- Intercept click of the target, advance tutorial, then restore old function onclick.
	local targetCallback = targetWidget:GetCallback('onclick')
	Game_Walkthrough.Storage[self:GetName()].targetOnClick = targetCallback
	targetWidget:SetCallback('onclick',
		function() 					
			targetWidget:SetCallback('onclick',
				function()
					if (targetCallback) then targetCallback() end
				end)
			targetWidget:RefreshCallbacks()
			if (targetCallback) then targetCallback() end
			--Echo('^171Target widget "'.. targetWidget:GetName() ..'" Clicked')
			local walkthroughDetail = "Trigger('advance_walkthrough', '"..self:GetName().."');"
			interface:UICmd(walkthroughDetail)
			--Echo(walkthroughDetail)
		end
	)
	
	local targetCallback = targetWidget:GetCallback('onselect')
	Game_Walkthrough.Storage[self:GetName()].targetOnSelect = targetCallback
	targetWidget:SetCallback('onselect',
		function() 					
			targetWidget:SetCallback('onselect',
				function()
					if (targetCallback) then targetCallback() end
				end)
			targetWidget:RefreshCallbacks()
			if (targetCallback) then targetCallback() end
			local walkthroughDetail = "Trigger('advance_walkthrough', '"..self:GetName().."');"
			interface:UICmd(walkthroughDetail)
		end
	)
	
	targetWidget:RefreshCallbacks()			

end

function Game_Walkthrough.UnsetWidgetAsStepClickTarget(self, targetWidget)

	if(self == nil) then
		Echo('^900Failed to set widget as click target because the self parameter was nil.')
		return
	end

	if(targetWidget == nil) then
		Echo('^900Failed to unset widget as click target for the targetWidget passed in because it is nil. Associated quest: "' .. self:GetName() .. '". TargetWidget: "' ..  Game_Walkthrough.Storage[self:GetName()].widgetToHighlight .. '"' )
		return
	end
	
	local targetCallback = targetWidget:GetCallback('onclick')
	Game_Walkthrough.Storage[self:GetName()].targetOnClick = targetCallback
	targetWidget:SetCallback('onclick', function() if (targetCallback) then targetCallback() end end )
	
	local targetCallback = targetWidget:GetCallback('onselect')
	Game_Walkthrough.Storage[self:GetName()].targetOnSelect = targetCallback
	targetWidget:SetCallback('onselect', function()	if (targetCallback) then targetCallback() end end )
	
	targetWidget:RefreshCallbacks()			


end

function Game_Walkthrough.HighlightShopItems1()
	
	Game_Walkthrough.Storage.shopHighlight1 = Game_Walkthrough.HighlightWidget(nil, 'guide_recommended_panel_1', true, false, '', nil, 'game_shop_v3', Translate('walkthrough_pregame_quest3_widgetHighlightEffect'))
	
end
interface:RegisterWatch('shop_storetour_highlightcomponent1', Game_Walkthrough.HighlightShopItems1)

function Game_Walkthrough.HighlightShopItems2()
	
	Game_Walkthrough.HighlightWidget(nil, 'guide_recommended_panel_1', false, false, '', Game_Walkthrough.Storage.shopHighlight1, 'game_shop_v3')
	Game_Walkthrough.Storage.shopHighlight2 = Game_Walkthrough.HighlightWidget(nil, 'menu_items_basic_containing_panel', true, false, '', nil, 'game_shop_v3', Translate('walkthrough_pregame_quest3_widgetHighlightEffect2'))
	
	GetWidget('walkthrough_highlight_root', interfaceName):Sleep(5000, function() Game_Walkthrough.HighlightWidget(nil, 'menu_items_basic_containing_panel', false, false, '', Game_Walkthrough.Storage.shopHighlight2, 'game_shop_v3') end)
	
end
interface:RegisterWatch('shop_storetour_highlightcomponent2', Game_Walkthrough.HighlightShopItems2)

--------------------------------------------------------------
--------------------------------------------------------------
--				Initializations and shutdowns				--
--------------------------------------------------------------
--------------------------------------------------------------
function Game_Walkthrough.InitWidget(sourceWidget, completionTrigger, widgetToHighlight)

	sourceWidget:RegisterWatch('advance_walkthrough', Game_Walkthrough.QuestEntryCompleted)
end

-- entries : string			A pipe ('|') deliminated list of quest information. Quest information is comma deliminated in
--							the format [UniqueQuestID],[NextStep],[QuestTitle],[CompletionTrigger],[WidgetToHighlight],[DescriptionEntry]
--							UniqueQuestID will be used as a widget name, so it must be a unique name among widgets.
--							NextStep should be an empty string if it is the last step in the walkthrough sequence
function Game_Walkthrough.PopulateQuestTableFromString(self, entries)

	local splitEntries = split(entries, '|')
	
	for key,entry in pairs(splitEntries) do
	
		local splitQuest = split(entry, ',')
		
		local stepID = splitQuest[1]
		local nextStep = splitQuest[2]
		local stepTitle = splitQuest[3]
		local completionTrigger = splitQuest[4]
		local widgetToHighlight = splitQuest[5]	
		local description = splitQuest[6]
		
		if(stepID == nil or nextStep == nil or stepTitle == nil or completionTrigger == nil or widgetToHighlight == nil) then
			Echo('Failed to parse walkthrough entry from string: "'.. entry ..'"')
			return
		end
		
		if(Game_Walkthrough.Storage[stepID] ~= nil) then
			Echo('^900stepID "'.. stepID ..'" has already been used. Aborting creation.')
			return
		end
		
		Game_Walkthrough.AddWalkthroughEntry(stepID, nextStep, stepTitle, completionTrigger, widgetToHighlight, description)
	end
	
	--Echo('Populating walkthrough table complete.')
end

function Game_Walkthrough.InitWalkthrough()
	Echo("^p^:Init Walkthrough")
	-- Instantiate the tutorial stuff in the game interface
	GetWidget("game_walkthrough_section", "game"):SetVisible(1)

	-- Init the stored misc tips container
	for i=1, Game_Walkthrough.Storage.MiscTipTracker.maxStoredTips, 1 do
		Game_Walkthrough.Storage.MiscTipTracker.trackedTipIDs[i] = {}
		Game_Walkthrough.Storage.MiscTipTracker.trackedTipIDs[i].tipID = -1
	end
	
	interface:RegisterWatch('ActiveInventoryStatus1', Game_Walkthrough.LvlUpActiveInventoryStatus)
	
	Game_Walkthrough.Storage.MiscTipsUsed['17'] = true
	Game_Walkthrough.Storage.MiscTipsUsed['18'] = true
end

function Game_Walkthrough.RevertWalkthrough()
	GetWidget("game_walkthrough_section", "game"):SetVisible(0)

	GetWidget('walkthrough_quest_widget', interfaceName):SetVisible(1)
	local dropdown = GetWidget('game_dropdown_options', 'game')
	if (dropdown) then
		dropdown:SetVisible(1)
	end
	local scoreboard = GetWidget('Nscores', 'game')
	if (scoreboard) then
		scoreboard:SetVisible(1)
	end
	GetWidget('walkthrough_end_tutorial_btn', interfaceName):SetVisible(1)
end

function Game_Walkthrough.PopulateQuestTable()
	
	--Echo('^707Started populating quest table.')

	GetWidget('walkthrough_quest_widget', interfaceName):SetVisible(false)
	local dropdown = GetWidget('game_dropdown_options', 'game')
	if (dropdown) then
		dropdown:SetVisible(0)
	end
	local scoreboard = GetWidget('Nscores', 'game')
	if (scoreboard) then
		scoreboard:SetVisible(0)
	end
	GetWidget('walkthrough_end_tutorial_btn', interfaceName):SetVisible(false)
	
	Game_Walkthrough.Storage.PanelWidth = GetWidget('quest_widget_entries_container', interfaceName):GetWidth()
	
	Game_Walkthrough.AddWalkthroughEntry(
		Translate('walkthrough_pregame_quest1_id'),
		Translate('walkthrough_pregame_quest1_nextStep'),
		Translate('walkthrough_pregame_quest1_title'),
		Translate('walkthrough_pregame_quest1_completionTrigger'),
		Translate('walkthrough_pregame_quest1_widgetToHighlight'),
		Translate('walkthrough_pregame_quest1_highlightWidgetInterface'),
		Translate('walkthrough_pregame_quest1_description'),
		Translate('walkthrough_pregame_quest1_widgetToHighlightClickToAdvance'),
		Translate('walkthrough_pregame_quest1_completionsToAdvance'),
		Translate('walkthrough_pregame_quest1_locateQuestScriptValue'),
		Translate('walkthrough_pregame_quest1_onClickWidget'),
		Translate('walkthrough_pregame_quest1_soundToPlay'),
		Translate('walkthrough_pregame_quest1_durationOfSound'))
	Game_Walkthrough.AddWalkthroughEntry(
		Translate('walkthrough_pregame_quest2_id'),
		Translate('walkthrough_pregame_quest2_nextStep'),
		Translate('walkthrough_pregame_quest2_title'),
		Translate('walkthrough_pregame_quest2_completionTrigger'),
		Translate('walkthrough_pregame_quest2_widgetToHighlight'),
		Translate('walkthrough_pregame_quest2_highlightWidgetInterface'),
		Translate('walkthrough_pregame_quest2_description'),
		Translate('walkthrough_pregame_quest2_widgetToHighlightClickToAdvance'),
		Translate('walkthrough_pregame_quest2_completionsToAdvance'),
		Translate('walkthrough_pregame_quest2_locateQuestScriptValue'),
		Translate('walkthrough_pregame_quest2_onClickWidget'),
		Translate('walkthrough_pregame_quest2_soundToPlay'),
		Translate('walkthrough_pregame_quest2_durationOfSound'))
	Game_Walkthrough.AddWalkthroughEntry(
		Translate('walkthrough_pregame_quest3_id'),
		Translate('walkthrough_pregame_quest3_nextStep'),
		Translate('walkthrough_pregame_quest3_title'),
		Translate('walkthrough_pregame_quest3_completionTrigger'),
		Translate('walkthrough_pregame_quest3_widgetToHighlight'),
		Translate('walkthrough_pregame_quest3_highlightWidgetInterface'),
		Translate('walkthrough_pregame_quest3_description'),
		Translate('walkthrough_pregame_quest3_widgetToHighlightClickToAdvance'),
		Translate('walkthrough_pregame_quest3_completionsToAdvance'),
		Translate('walkthrough_pregame_quest3_locateQuestScriptValue'),
		Translate('walkthrough_pregame_quest3_onClickWidget'),
		Translate('walkthrough_pregame_quest3_soundToPlay'),
		Translate('walkthrough_pregame_quest3_durationOfSound'))
	Game_Walkthrough.AddWalkthroughEntry(
		Translate('walkthrough_pregame_quest4_id'),
		Translate('walkthrough_pregame_quest4_nextStep'),
		Translate('walkthrough_pregame_quest4_title'),
		Translate('walkthrough_pregame_quest4_completionTrigger'),
		Translate('walkthrough_pregame_quest4_widgetToHighlight'),
		Translate('walkthrough_pregame_quest4_highlightWidgetInterface'),
		Translate('walkthrough_pregame_quest4_description'),
		Translate('walkthrough_pregame_quest4_widgetToHighlightClickToAdvance'),
		Translate('walkthrough_pregame_quest4_completionsToAdvance'),
		Translate('walkthrough_pregame_quest4_locateQuestScriptValue'),
		Translate('walkthrough_pregame_quest4_onClickWidget'),
		Translate('walkthrough_pregame_quest4_soundToPlay'),
		Translate('walkthrough_pregame_quest4_durationOfSound'))
	Game_Walkthrough.AddWalkthroughEntry(
		Translate('walkthrough_pregame_quest5_id'),
		Translate('walkthrough_pregame_quest5_nextStep'),
		Translate('walkthrough_pregame_quest5_title'),
		Translate('walkthrough_pregame_quest5_completionTrigger'),
		Translate('walkthrough_pregame_quest5_widgetToHighlight'),
		Translate('walkthrough_pregame_quest5_highlightWidgetInterface'),
		Translate('walkthrough_pregame_quest5_description'),
		Translate('walkthrough_pregame_quest5_widgetToHighlightClickToAdvance'),
		Translate('walkthrough_pregame_quest5_completionsToAdvance'),
		Translate('walkthrough_pregame_quest5_locateQuestScriptValue'),
		Translate('walkthrough_pregame_quest5_onClickWidget'),
		Translate('walkthrough_pregame_quest5_soundToPlay'),
		Translate('walkthrough_pregame_quest5_durationOfSound'))
	Game_Walkthrough.AddWalkthroughEntry(
		Translate('walkthrough_pregame_sidequest1_id'),
		Translate('walkthrough_pregame_sidequest1_nextStep'),
		Translate('walkthrough_pregame_sidequest1_title'),
		Translate('walkthrough_pregame_sidequest1_completionTrigger'),
		Translate('walkthrough_pregame_sidequest1_widgetToHighlight'),
		Translate('walkthrough_pregame_sidequest1_highlightWidgetInterface'),
		Translate('walkthrough_pregame_sidequest1_description'),
		Translate('walkthrough_pregame_sidequest1_widgetToHighlightClickToAdvance'),
		Translate('walkthrough_pregame_sidequest1_completionsToAdvance'),
		Translate('walkthrough_pregame_sidequest1_locateQuestScriptValue'),
		Translate('walkthrough_pregame_sidequest1_onClickWidget'),
		Translate('walkthrough_pregame_sidequest1_soundToPlay'),
		Translate('walkthrough_pregame_sidequest1_durationOfSound'))
	Game_Walkthrough.AddWalkthroughEntry(
		Translate('walkthrough_game_quest1_id'),
		Translate('walkthrough_game_quest1_nextStep'),
		Translate('walkthrough_game_quest1_title'),
		Translate('walkthrough_game_quest1_completionTrigger'),
		Translate('walkthrough_game_quest1_widgetToHighlight'),
		Translate('walkthrough_game_quest1_highlightWidgetInterface'),
		Translate('walkthrough_game_quest1_description'),
		Translate('walkthrough_game_quest1_widgetToHighlightClickToAdvance'),
		Translate('walkthrough_game_quest1_completionsToAdvance'),
		Translate('walkthrough_game_quest1_locateQuestScriptValue'),
		Translate('walkthrough_game_quest1_onClickWidget'),
		Translate('walkthrough_game_quest1_soundToPlay'),
		Translate('walkthrough_game_quest1_durationOfSound'))
	Game_Walkthrough.AddWalkthroughEntry(
		Translate('walkthrough_game_quest2_id'),
		Translate('walkthrough_game_quest2_nextStep'),
		Translate('walkthrough_game_quest2_title'),
		Translate('walkthrough_game_quest2_completionTrigger'),
		Translate('walkthrough_game_quest2_widgetToHighlight'),
		Translate('walkthrough_game_quest2_highlightWidgetInterface'),
		Translate('walkthrough_game_quest2_description'),
		Translate('walkthrough_game_quest2_widgetToHighlightClickToAdvance'),
		Translate('walkthrough_game_quest2_completionsToAdvance'),
		Translate('walkthrough_game_quest2_locateQuestScriptValue'),
		Translate('walkthrough_game_quest2_onClickWidget'),
		Translate('walkthrough_game_quest2_soundToPlay'),
		Translate('walkthrough_game_quest2_durationOfSound'))
	Game_Walkthrough.AddWalkthroughEntry(
		Translate('walkthrough_game_sidequest1_id'),
		Translate('walkthrough_game_sidequest1_nextStep'),
		Translate('walkthrough_game_sidequest1_title'),
		Translate('walkthrough_game_sidequest1_completionTrigger'),
		Translate('walkthrough_game_sidequest1_widgetToHighlight'),
		Translate('walkthrough_game_sidequest1_highlightWidgetInterface'),
		Translate('walkthrough_game_sidequest1_description'),
		Translate('walkthrough_game_sidequest1_widgetToHighlightClickToAdvance'),
		Translate('walkthrough_game_sidequest1_completionsToAdvance'),
		Translate('walkthrough_game_sidequest1_locateQuestScriptValue'),
		Translate('walkthrough_game_sidequest1_onClickWidget'),
		Translate('walkthrough_game_sidequest1_soundToPlay'),
		Translate('walkthrough_game_sidequest1_durationOfSound'))
	Game_Walkthrough.AddWalkthroughEntry(
		Translate('walkthrough_game_sidequest2_id'),
		Translate('walkthrough_game_sidequest2_nextStep'),
		Translate('walkthrough_game_sidequest2_title'),
		Translate('walkthrough_game_sidequest2_completionTrigger'),
		Translate('walkthrough_game_sidequest2_widgetToHighlight'),
		Translate('walkthrough_game_sidequest2_highlightWidgetInterface'),
		Translate('walkthrough_game_sidequest2_description'),
		Translate('walkthrough_game_sidequest2_widgetToHighlightClickToAdvance'),
		Translate('walkthrough_game_sidequest2_completionsToAdvance'),
		Translate('walkthrough_game_sidequest2_locateQuestScriptValue'),
		Translate('walkthrough_game_sidequest2_onClickWidget'),
		Translate('walkthrough_game_sidequest2_soundToPlay'),
		Translate('walkthrough_game_sidequest2_durationOfSound'))
	Game_Walkthrough.AddWalkthroughEntry(
		Translate('walkthrough_game_quest3_id'),
		Translate('walkthrough_game_quest3_nextStep'),
		Translate('walkthrough_game_quest3_title'),
		Translate('walkthrough_game_quest3_completionTrigger'),
		Translate('walkthrough_game_quest3_widgetToHighlight'),
		Translate('walkthrough_game_quest3_highlightWidgetInterface'),
		Translate('walkthrough_game_quest3_description'),
		Translate('walkthrough_game_quest3_widgetToHighlightClickToAdvance'),
		Translate('walkthrough_game_quest3_completionsToAdvance'),
		Translate('walkthrough_game_quest3_locateQuestScriptValue'),
		Translate('walkthrough_game_quest3_onClickWidget'),
		Translate('walkthrough_game_quest3_soundToPlay'),
		Translate('walkthrough_game_quest3_durationOfSound'))
	Game_Walkthrough.AddWalkthroughEntry(
		Translate('walkthrough_game_quest4_id'),
		Translate('walkthrough_game_quest4_nextStep'),
		Translate('walkthrough_game_quest4_title'),
		Translate('walkthrough_game_quest4_completionTrigger'),
		Translate('walkthrough_game_quest4_widgetToHighlight'),
		Translate('walkthrough_game_quest4_highlightWidgetInterface'),
		Translate('walkthrough_game_quest4_description'),
		Translate('walkthrough_game_quest4_widgetToHighlightClickToAdvance'),
		Translate('walkthrough_game_quest4_completionsToAdvance'),
		Translate('walkthrough_game_quest4_locateQuestScriptValue'),
		Translate('walkthrough_game_quest4_onClickWidget'),
		Translate('walkthrough_game_quest4_soundToPlay'),
		Translate('walkthrough_game_quest4_durationOfSound'))
	Game_Walkthrough.AddWalkthroughEntry(
		Translate('walkthrough_game_quest5_id'),
		Translate('walkthrough_game_quest5_nextStep'),
		Translate('walkthrough_game_quest5_title'),
		Translate('walkthrough_game_quest5_completionTrigger'),
		Translate('walkthrough_game_quest5_widgetToHighlight'),
		Translate('walkthrough_game_quest5_highlightWidgetInterface'),
		Translate('walkthrough_game_quest5_description'),
		Translate('walkthrough_game_quest5_widgetToHighlightClickToAdvance'),
		Translate('walkthrough_game_quest5_completionsToAdvance'),
		Translate('walkthrough_game_quest5_locateQuestScriptValue'),
		Translate('walkthrough_game_quest5_onClickWidget'),
		Translate('walkthrough_game_quest5_soundToPlay'),
		Translate('walkthrough_game_quest5_durationOfSound'))
	Game_Walkthrough.AddWalkthroughEntry(
		Translate('walkthrough_game_quest6_id'),
		Translate('walkthrough_game_quest6_nextStep'),
		Translate('walkthrough_game_quest6_title'),
		Translate('walkthrough_game_quest6_completionTrigger'),
		Translate('walkthrough_game_quest6_widgetToHighlight'),
		Translate('walkthrough_game_quest6_highlightWidgetInterface'),
		Translate('walkthrough_game_quest6_description'),
		Translate('walkthrough_game_quest6_widgetToHighlightClickToAdvance'),
		Translate('walkthrough_game_quest6_completionsToAdvance'),
		Translate('walkthrough_game_quest6_locateQuestScriptValue'),
		Translate('walkthrough_game_quest6_onClickWidget'),
		Translate('walkthrough_game_quest6_soundToPlay'),
		Translate('walkthrough_game_quest6_durationOfSound'))
	Game_Walkthrough.AddWalkthroughEntry(
		Translate('walkthrough_game_quest7_id'),
		Translate('walkthrough_game_quest7_nextStep'),
		Translate('walkthrough_game_quest7_title'),
		Translate('walkthrough_game_quest7_completionTrigger'),
		Translate('walkthrough_game_quest7_widgetToHighlight'),
		Translate('walkthrough_game_quest7_highlightWidgetInterface'),
		Translate('walkthrough_game_quest7_description'),
		Translate('walkthrough_game_quest7_widgetToHighlightClickToAdvance'),
		Translate('walkthrough_game_quest7_completionsToAdvance'),
		Translate('walkthrough_game_quest7_locateQuestScriptValue'),
		Translate('walkthrough_game_quest7_onClickWidget'),
		Translate('walkthrough_game_quest7_soundToPlay'),
		Translate('walkthrough_game_quest7_durationOfSound'))
	
	Game_Walkthrough.Storage.questsLoaded = true
	--Echo('^707Finished populating quest table.')
end

function Game_Walkthrough.AddWalkthroughEntry(stepID, nextStep, stepTitle, completionTrigger, widgetToHighlight, highlightWidgetInterface, description, highlightWidgetClickToAdvance,
												numberOfCompletionsToAdvance, locateQuestScriptValue, onClickWidget, soundToPlay, durationOfSound)

	if(Game_Walkthrough.Storage[stepID] ~= nil) then
		Echo('^900StepID "'.. stepID ..'" is already loaded into the table.')
		return
	end
	
	--stepTitle = Game_Walkthrough.WrapText(stepTitle, Game_Walkthrough.Storage.PanelWidth - interface:GetWidthFromString('1h'))
	
	local splitWidgetsToHighlight
	
	Game_Walkthrough.Storage[stepID] = {}
	Game_Walkthrough.Storage[stepID].stepID = stepID
	Game_Walkthrough.Storage[stepID].nextStep = nextStep
	Game_Walkthrough.Storage[stepID].stepTitle = stepTitle
	Game_Walkthrough.Storage[stepID].completionTrigger = completionTrigger
	Game_Walkthrough.Storage[stepID].widgetToHighlight = widgetToHighlight
	if(highlightWidgetInterface ~= '') then Game_Walkthrough.Storage[stepID].highlightWidgetInterface = highlightWidgetInterface else Game_Walkthrough.Storage[stepID].highlightWidgetInterface = 'game' end
	Game_Walkthrough.Storage[stepID].description = description
	Game_Walkthrough.Storage[stepID].highlightWidgetClickToAdvance = (highlightWidgetClickToAdvance=='1' or highlightWidgetClickToAdvance:lower()=='true')
	Game_Walkthrough.Storage[stepID].completionsToAdvance = tonumber(numberOfCompletionsToAdvance) or 1
	Game_Walkthrough.Storage[stepID].locateQuestScriptValue = locateQuestScriptValue
	Game_Walkthrough.Storage[stepID].onClickWidget = onClickWidget
	Game_Walkthrough.Storage[stepID].soundToPlay = soundToPlay
	Game_Walkthrough.Storage[stepID].durationOfSound = durationOfSound
	Game_Walkthrough.Storage[stepID].currentCompletions = 0
	
	local displayStepTitle = stepTitle
	
	if(Game_Walkthrough.Storage[stepID].completionsToAdvance > 1) then
		displayStepTitle = displayStepTitle .. '\n\t\t\t\t' .. Game_Walkthrough.Storage[stepID].currentCompletions .. '/' ..Game_Walkthrough.Storage[stepID].completionsToAdvance
	end
	
	-- Make widgets for displaying the information
	local widgetInitFunction = 'Game_Walkthrough.InitWidget(self, \'' .. completionTrigger .. '\', \''..  widgetToHighlight .. '\');'
	GetWidget('quest_widget_entries_container', interfaceName):Instantiate('walkthrough_quest_entry_template', 'oninstantiatelua', widgetInitFunction, 'stepID', stepID, 'text', displayStepTitle, 'watchTarget', completionTrigger)
	
	-- Enable/disable buttons based on their data
	GetWidget(stepID..'_ping_map_button', interfaceName):SetEnabled(Game_Walkthrough.Storage[stepID].locateQuestScriptValue ~= '')
	GetWidget(stepID..'_more_info_button', interfaceName):SetEnabled(Game_Walkthrough.Storage[stepID].onClickWidget ~= '')
	
	--Echo('^815stepID "'.. stepID ..'" loaded into walkthrough table.')

end

function Game_Walkthrough.EndWalkthrough(source, isActuallyEndOfGame)
	
	if(AtoB(isActuallyEndOfGame)) then
		UIManager.GetInterface('main'):HoNGMainF('UserAction',25, true)
		if (Main.walkthroughState == 1) then
			Main.walkthroughState = 2
		end	
		GetWidget('game_walkthrough_tip_parent', 'game'):SetVisible(false)
		GetWidget('walkthrough_tip_storage_container', 'game'):SetVisible(false)
		local dropdown = GetWidget('game_dropdown_options', 'game')
		if (dropdown) then
			dropdown:SetVisible(true)
		end
		local scoreboard = GetWidget('Nscores', 'game')
		if (scoreboard) then
			scoreboard:SetVisible(true)
		end
	end
end
interface:RegisterWatch('EndGame', Game_Walkthrough.EndWalkthrough)

function Game_Walkthrough.EndWalkthroughGame()
	
	UIManager.GetInterface('main'):HoNGMainF('UserAction',24, true)
	GetWidget('walkthrough_finished_prompt', interfaceName):FadeIn(250)

	Game_Walkthrough.EndWalkthrough(nil, 'true')
	interface:UICmd("Cmd('EndMatch 2');")

end


--------------------------------------------------------------
--------------------------------------------------------------
--			Walkthrough traversal and interaction			--
--------------------------------------------------------------
--------------------------------------------------------------
function Game_Walkthrough.QuestEntryHovered(self)
	
	local step = Game_Walkthrough.Storage[self:GetName()]
end

function Game_Walkthrough.QuestEntryUnhovered(self)
	
	local step = Game_Walkthrough.Storage[self:GetName()]
end

function Game_Walkthrough.QuestEntryClicked(stepName)

	local step = Game_Walkthrough.Storage[stepName]
	Game_Walkthrough.Storage.pausedGameStep = step
	
	-- Let the game know the entry was clicked
	if(step.locateQuestScriptValue ~= '') then
		local scriptMsgString = "SendScriptMessage('"..step.locateQuestScriptValue.."', '1');"
		interface:UICmd(scriptMsgString)

		--Echo(scriptMsgString)
	end	
end

function Game_Walkthrough.QuestEntryDetailClicked(stepName)
	
	local step = Game_Walkthrough.Storage[stepName]
	
	Game_Walkthrough.ShowMoreInfoWidget(step.onClickWidget)
end

function Game_Walkthrough.PauseGame(widgetToShowOnPause)
	if(widgetToShowOnPause ~= nil) then
		Set('vid_postEffectPath', '/core/post/grayscale_noblur.posteffect')
		interface:UICmd("Cmd('ServerPause')")
		widgetToShowOnPause:FadeIn(250)
	else
		Echo('^rWalkthrough: Failed to show entry click target.')
	end
end

function Game_Walkthrough.ResumeGame(self)

	Set('vid_postEffectPath', '/core/post/bloom.posteffect')
	self:FadeOut(250)

	step = Game_Walkthrough.Storage.pausedGameStep
	Game_Walkthrough.Storage.pausedGameStep = nil
	
	interface:UICmd("Cmd('ServerUnpause')")
end

function Game_Walkthrough.ActivateStep(stepIDToActivate)
	
	local widgetToActivate = GetWidget(stepIDToActivate, interfaceName)
	local stepToActivate = Game_Walkthrough.Storage[stepIDToActivate]
	
	if(widgetToActivate == nil or stepToActivate == nil) then
		Echo('^900Step and/or widget from stepID "'.. stepIDToActivate ..'" were invalid and not activated.')
		return
	end
	
	if(stepToActivate.stepTitle ~= '') then
		widgetToActivate:SetVisible(true)
	end

	if(stepToActivate.description ~= '') then
		Game_Walkthrough.ShowTextPanel(stepToActivate.durationOfSound, true, string.gsub(stepToActivate.stepTitle, '\n', ''), stepToActivate.description, '', stepToActivate.soundToPlay, 'false')
	end
	
	if(stepToActivate.widgetToHighlight ~= nil and stepToActivate.widgetToHighlight ~= '') then
		stepToActivate.highlightID = Game_Walkthrough.HighlightWidget(widgetToActivate, stepToActivate.widgetToHighlight, true, true, "", stepToActivate.highlightID, stepToActivate.highlightWidgetInterface)
		--Echo('Highlight widget ' .. stepToActivate.widgetToHighlight)
	end
	
	if(stepToActivate.highlightWidgetClickToAdvance) then
		Game_Walkthrough.SetWidgetAsStepClickTarget(widgetToActivate, GetWidget(stepToActivate.widgetToHighlight, stepToActivate.highlightWidgetInterface))
	end
	
	stepToActivate.storedMouseOver = widgetToActivate:GetCallback('onmouseover')
	stepToActivate.storedMouseOut = widgetToActivate:GetCallback('onmouseout')
	stepToActivate.storedOnTrigger = widgetToActivate:GetCallback('ontrigger')

	widgetToActivate:SetCallback('onmouseover', 
		function()
			if(stepToActivate.storedMouseOver) then stepToActivate.storedMouseOver() end
			if(Game_Walkthrough.QuestEntryHovered) then Game_Walkthrough.QuestEntryHovered(widgetToActivate) end
		end
	)
	widgetToActivate:SetCallback('onmouseout', 
		function()
			if(stepToActivate.storedMouseOut) then stepToActivate.storedMouseOut() end
			if(Game_Walkthrough.QuestEntryUnhovered) then Game_Walkthrough.QuestEntryUnhovered(widgetToActivate) end
		end
	)
	
	widgetToActivate:SetCallback('ontrigger', 
		function()
			if(stepToActivate.storedOnTrigger) then stepToActivate.storedOnTrigger() end
			if(Game_Walkthrough.QuestEntryCompleted) then Game_Walkthrough.QuestEntryCompleted(widgetToActivate, widgetToActivate:GetName()) end
		end
	)
	widgetToActivate:RefreshCallbacks()
end

function Game_Walkthrough.DeactivateStep(stepIDToDeactivate)
	local widgetToDeactivate = GetWidget(stepIDToDeactivate, interfaceName)
	
	if(widgetToDeactivate == nil) then
		Echo('^900Step and/or widget from stepID "'.. stepIDToDeactivate ..'" were invalid and not deactivated.')
		return
	end
	
	GetWidget(stepIDToDeactivate..'_checkmark', interfaceName):FadeIn(250)
	widgetToDeactivate:Sleep(8000, function(widgetToDeactivate) widgetToDeactivate:FadeOut(5000) end)
	GetWidget(stepIDToDeactivate..'_ping_map_button', interfaceName):SetEnabled(false)
	
	GetWidget(stepIDToDeactivate, interfaceName):SetNoClick(true)
	GetWidget(stepIDToDeactivate..'_ping_map_button', interfaceName):SetNoClick(true)
	GetWidget(stepIDToDeactivate..'_more_info_button', interfaceName):SetNoClick(true)
	GetWidget(stepIDToDeactivate..'_more_info_button', interfaceName):SetEnabled(false)
	
	local stepToDeactivate = Game_Walkthrough.Storage[stepIDToDeactivate]
	stepToDeactivate.highlightID = Game_Walkthrough.HighlightWidget(widgetToDeactivate, stepToDeactivate.widgetToHighlight, false, false, "", stepToDeactivate.highlightID, stepToDeactivate.highlightWidgetInterface)
	
	if(stepToDeactivate.highlightWidgetClickToAdvance) then
		Game_Walkthrough.UnsetWidgetAsStepClickTarget(widgetToDeactivate, GetWidget(stepToDeactivate.widgetToHighlight, stepToDeactivate.highlightWidgetInterface))
	end
	
	-- unregister for completion trigger?
	widgetToDeactivate:SetCallback('onmouseover', function() if(stepToDeactivate.storedMouseOver) then  stepToDeactivate.storedMouseOver(widgetToDeactivate) end end)
	widgetToDeactivate:SetCallback('onmouseout', function() if(stepToDeactivate.storedMouseOut) then stepToDeactivate.storedMouseOut(widgetToDeactivate) end end)
	widgetToDeactivate:SetCallback('ontrigger', function() if(stepToDeactivate.storedOnTrigger) then stepToDeactivate.storedOnTrigger(widgetToDeactivate) end end)
	widgetToDeactivate:RefreshCallbacks()
end

function Game_Walkthrough.QuestEntryCompleted(self, stepID)
	
	if(self == nil) then
		Echo('^900Self value passed with "'.. tostring(stepID) ..'" was nil')
		return
	end
	
	if(self:GetName() ~= stepID) then
		-- Only act upon steps that are ourself
		return
	end
	
	if(stepID == nil or stepID == '') then
		Echo('^900Failed to complete quest for "'.. self:GetName() ..'" due to invalid stepID "'.. (stepID or 'nil') ..'"')
		return
	end
	
	--Echo('^707Step: ' .. stepID .. ' Widget: ' .. self:GetName() .. ' Completions: ' .. Game_Walkthrough.Storage[stepID].currentCompletions .. ' Total: ' .. Game_Walkthrough.Storage[stepID].completionsToAdvance)
	
	Game_Walkthrough.Storage[stepID].currentCompletions = Game_Walkthrough.Storage[stepID].currentCompletions + 1
		
	if(Game_Walkthrough.Storage[stepID].completionsToAdvance > 1) then
		GetWidget(stepID .. 'Name', interfaceName):SetText(Game_Walkthrough.Storage[stepID].stepTitle .. '\n\t\t\t\t' .. Game_Walkthrough.Storage[stepID].currentCompletions .. '/' .. Game_Walkthrough.Storage[stepID].completionsToAdvance)
	end
	
	if(Game_Walkthrough.Storage[stepID].currentCompletions >= Game_Walkthrough.Storage[stepID].completionsToAdvance) then
		--Echo('^707Quest "'.. stepID .. '" completed.')
		
		if(Game_Walkthrough.Storage[stepID].completionsToAdvance > 1) then
			Game_Walkthrough.Storage[stepID].currentCompletions = 0
			GetWidget(stepID .. 'Name', interfaceName):SetText(Game_Walkthrough.Storage[stepID].stepTitle .. '\n\t\t\t\t' .. Game_Walkthrough.Storage[stepID].completionsToAdvance .. '/' .. Game_Walkthrough.Storage[stepID].completionsToAdvance)
		end
		
		Game_Walkthrough.AdvanceWalkthrough(stepID)	
	end
end

-- Shows the quest indicated by the startingStepID
function Game_Walkthrough.StartQuestline(self, startingStepID)

	--Echo('Starting walkthrough from step "'.. startingStepID ..'"')
	
	GetWidget('walkthrough_quest_widget', interfaceName):SetVisible(true)
	GetWidget('walkthrough_end_tutorial_btn', interfaceName):SetVisible(true)
	GetWidget('game_walkthrough_click_for_info_hint', interfaceName):SetVisible(true)
	
	if(Game_Walkthrough.Storage[startingStepID] == nil) then
		Echo('^900Failed to start walkthrough from stepID: "'.. startingStepID .. '". Step does not exist in walkthrough storage.')
		return
	end
	
	Game_Walkthrough.ActivateStep(startingStepID)
	
	Game_Walkthrough.ShowMiscTip(self, '16')

end

function Game_Walkthrough.AdvanceWalkthrough(currentStepID)
	
	if(currentStepID == nil) then
		Echo('^900Step value in AdvanceWalkthrough() was nil.')
		return
	end
	
	--Echo('^171Advancing walkthrough from step: ' .. currentStepID)
	
	if(Game_Walkthrough.Storage[currentStepID] == nil) then
		Echo('^900Failed to find current step from ID "' .. currentStepID .. '"')
		return
	end
	
	Game_Walkthrough.DeactivateStep(currentStepID)
	
	local currentStep = Game_Walkthrough.Storage[currentStepID]
	if(currentStep.nextStep == 'walkthrough_end') then
		--Echo('^171Finished quest chain. Last quest was "'.. currentStep.stepID ..'"')
		Game_Walkthrough.EndWalkthrough(nil, 'true')
		return
	end

	for key, nextStepID in pairs(split(currentStep.nextStep, '|')) do
	
		if(nextStepID ~= '' and Game_Walkthrough.Storage[nextStepID] == nil) then
			Echo('^900Failed to advance to next step "' .. currentStep.nextStep .. '"  from step "' .. currentStep.stepID .. '"')
			return
		end
		
		--Echo('^171Advanced walkthrough to step: ' .. nextStepID)	
		Game_Walkthrough.ActivateStep(nextStepID)
		
	end

end

--------------------------------------
--------------------------------------
--			Misc functions			--
--------------------------------------
--------------------------------------

function Game_Walkthrough.PlayTranslatedSound(path)

	local language = GetCvarString('host_language')
	local localizedPath = string.gsub(path, 'LANGUAGE', language)
	
	if(localizedPath ~= '') then
		--Echo('^941Playing sound from path: "' .. localizedPath .. '"')
		interface:UICmd("PlaySoundDampen('"..localizedPath.."', '.5')")
	end
end

function Game_Walkthrough.ShowMiscTip(self, numberIDOfTip)
	
	-- Only show if this wasn't shown before
	if(Game_Walkthrough.Storage.MiscTipsUsed[numberIDOfTip] == true) then
		return
	end
	
	-- Show the text panel if there's a description to show
	if(Translate('walkthrough_tip'..numberIDOfTip..'_text') ~= '') then
		Game_Walkthrough.ShowTextPanel(tonumber(Translate('walkthrough_tip'..numberIDOfTip..'_duration')),
										Translate('walkthrough_tip'..numberIDOfTip..'_skippable') ~= '1',
										Translate('walkthrough_tip'..numberIDOfTip..'_title'),
										Translate('walkthrough_tip'..numberIDOfTip..'_text'),
										Translate('walkthrough_tip'..numberIDOfTip..'_icon'),
										Translate('walkthrough_tip'..numberIDOfTip..'_sound'),
										Translate('walkthrough_tip'..numberIDOfTip..'_duringPanning'))
	end
	-- Track the shown tip if the tip has a widget to show for more information
	if(Translate('walkthrough_tip'..numberIDOfTip..'_moreInfoWidget') ~= '') then
		Game_Walkthrough.AppendTrackedMiscTip(numberIDOfTip)
	end
	
	if(AtoB(Translate('walkthrough_tip'.. numberIDOfTip .. '_pauseOnShow'))) then
		--Echo('walkthrough_paused_grey_background in ' .. interfaceName)
		Game_Walkthrough.PauseGame(GetWidget('walkthrough_paused_grey_background', interfaceName))
		GetWidget('walkthrough_paused_grey_background', interfaceName):SetNoClick(true)
		GetWidget('walkthrough_paused_grey_background', interfaceName):Sleep(tonumber(Translate('walkthrough_tip'..numberIDOfTip..'_duration')), function()
			GetWidget('walkthrough_paused_grey_background', interfaceName):SetNoClick(false)
			Game_Walkthrough.ResumeGame(GetWidget('walkthrough_paused_grey_background', interfaceName))
		end)
	end
	
	if(numberIDOfTip ~= '17') then
		Game_Walkthrough.Storage.MiscTipsUsed[numberIDOfTip] = true
	end

end

function Game_Walkthrough.AppendTrackedMiscTip(tipID)

	-- Slide everything down by 1
	for i=Game_Walkthrough.Storage.MiscTipTracker.maxStoredTips, 1, -1 do
		Game_Walkthrough.Storage.MiscTipTracker.trackedTipIDs[i+1] = Game_Walkthrough.Storage.MiscTipTracker.trackedTipIDs[i] or {}
	end
	
	-- Remove the extra entry at the end
	Game_Walkthrough.Storage.MiscTipTracker.trackedTipIDs[Game_Walkthrough.Storage.MiscTipTracker.maxStoredTips+1] = nil
	
	-- Set the first entry to the one just appended
	Game_Walkthrough.Storage.MiscTipTracker.trackedTipIDs[1] = {}
	Game_Walkthrough.Storage.MiscTipTracker.trackedTipIDs[1].tipID = tipID

	-- Update the widgets shown
	Game_Walkthrough.UpdateTrackedMiscTips()

end

function Game_Walkthrough.UpdateTrackedMiscTips()

	local haventFoundFirstUsed = true
	for i=1, Game_Walkthrough.Storage.MiscTipTracker.maxStoredTips, 1 do
		-- Hide all the trackers
		GetWidget('walkthrough_tip_storage_entry_'..i, 'game'):SetVisible(false)
		GetWidget('walkthrough_tip_storage_entry_color_panel_'..i, 'game'):SetVisible(false)

		-- Show and populate trackers if there's a stored tip
		if(Game_Walkthrough.Storage.MiscTipTracker.trackedTipIDs[i].tipID ~= -1) then
			GetWidget('walkthrough_tip_storage_entry_label_'..i, 'game'):SetText('Tip: ' .. Translate('walkthrough_tip'..Game_Walkthrough.Storage.MiscTipTracker.trackedTipIDs[i].tipID..'_title'))
			if(haventFoundFirstUsed) then 
				GetWidget('walkthrough_tip_storage_entry_'..i, 'game'):FadeIn(500)
				haventFoundFirstUsed = false
				Game_Walkthrough.FlashTip(i)
			else
				GetWidget('walkthrough_tip_storage_entry_'..i, 'game'):SetVisible(true)
			end
			GetWidget('walkthrough_tip_storage_entry_icon_'..i, 'game'):SetTexture(Translate('walkthrough_tip_storage_entry_tip'.. Game_Walkthrough.Storage.MiscTipTracker.trackedTipIDs[i].tipID .. '_icon'))
		end
	end
end

function Game_Walkthrough.FlashTip(index)
	
	--Echo('^707FlashTip')
	if(Game_Walkthrough.Storage.MiscTipTracker.trackedTipIDs[index].tipID ~= -1) then
		GetWidget('walkthrough_tip_storage_entry_color_panel_'..index, 'game'):FadeIn(250)
		GetWidget('walkthrough_tip_storage_entry_color_panel_'..index, 'game'):Sleep(Game_Walkthrough.Storage.tipFlashCycleDuration,
			function() 
				GetWidget('walkthrough_tip_storage_entry_color_panel_'..index, 'game'):FadeOut(Game_Walkthrough.Storage.tipFlashCycleDuration)
				GetWidget('walkthrough_tip_storage_entry_color_panel_'..index, 'game'):Sleep(Game_Walkthrough.Storage.tipFlashCycleDuration,
					function() 
						GetWidget('walkthrough_tip_storage_entry_color_panel_'..index, 'game'):FadeIn(Game_Walkthrough.Storage.tipFlashCycleDuration)
						GetWidget('walkthrough_tip_storage_entry_color_panel_'..index, 'game'):Sleep(Game_Walkthrough.Storage.tipFlashCycleDuration,
							function() 
								GetWidget('walkthrough_tip_storage_entry_color_panel_'..index, 'game'):FadeOut(Game_Walkthrough.Storage.tipFlashCycleDuration)
							end)
					end)
			end)
	end
end

function Game_Walkthrough.MiscTipClicked(widgetID)
	
	if(Game_Walkthrough.Storage.MiscTipTracker.trackedTipIDs[widgetID] ~= nil) then
		local tipID = Game_Walkthrough.Storage.MiscTipTracker.trackedTipIDs[widgetID].tipID
		local targetWidgetName = Translate('walkthrough_tip'..tipID..'_moreInfoWidget')
		--Echo('Tip tracker clicked - TipID: ' .. tipID.. ' | Widget to show: ' .. targetWidgetName)
		Game_Walkthrough.ShowMoreInfoWidget(targetWidgetName)
	end
end

function Game_Walkthrough.MiscTipWaitUntil(sourceWidget)
	
	Game_Walkthrough.Storage.MiscTipsUsed['17'] = false
	Game_Walkthrough.Storage.MiscTipsUsed['18'] = false
	
end
interface:RegisterWatch(Translate('walkthrough_tip18_triggerToEnable'), Game_Walkthrough.MiscTipWaitUntil)

function Game_Walkthrough.ShowMoreInfoWidget(targetWidgetName)
	
	if(targetWidgetName ~= '') then
		
		local widgetToShow = GetWidget(targetWidgetName, interfaceName)
		Game_Walkthrough.PauseGame(widgetToShow)
	end
end

function Game_Walkthrough.InitMoreInfoWidget(self, context, texture)

	--Echo('^707Registering ' .. texture .. ' to context ' .. context)
	ResourceManager.RegisterTexture(texture, context, self)
end

function Game_Walkthrough.LoadTextureForMoreInfoWidget(self, context, texturePath)
	
	ResourceManager.LoadContext(context)
	self:SetTexture(texturePath)
	
end

function Game_Walkthrough.UnloadTextureForMoreInfoWidget(self, context)
	
	self:SetTexture('')
	ResourceManager.UnloadContext(context, true)
	
end

function Game_Walkthrough.ShowTextPanel(duration, skippable, label, description, iconTexture, sound, duringPanning)
	
	if(duringPanning == 'true') then
		-- Set the parent to invisible so the onshow triggers when visible is set to true
		GetWidget('game_walkthrough_tip_parent2', 'game_tutorial1'):SetVisible(false)
		
		Set('game_walkthrough_tip_duration', duration)
		GetWidget('game_walkthrough_tip_parent2', 'game_tutorial1'):SetNoClick(skippable)
		
		GetWidget('game_walkthrough_tip_icon_label2', 'game_tutorial1'):SetText(label)
		GetWidget('game_walkthrough_tip_icon_desc2', 'game_tutorial1'):SetText(description)
		
		if(iconTexture ~= '') then
			GetWidget('game_walkthrough_tip_icon2', 'game_tutorial1'):SetTexture(iconTexture)
		else
			GetWidget('game_walkthrough_tip_icon2', 'game_tutorial1'):SetTexture(Translate('walkthrough_tip_default_icon'))
		end
		GetWidget('game_walkthrough_tip_icon2', 'game_tutorial1'):SetVisible(true)
		GetWidget('game_walkthrough_tip_parent2', 'game_tutorial1'):SetVisible(true)
		GetWidget('game_walkthrough_tip_icon_label2', 'game_tutorial1'):SetVisible(true)
		GetWidget('game_walkthrough_tip_icon_desc2', 'game_tutorial1'):SetVisible(true)
	else
		-- Set the parent to invisible so the onshow triggers when visible is set to true
		GetWidget('game_walkthrough_tip_parent', 'game'):SetVisible(false)
		
		Set('game_walkthrough_tip_duration', duration)
		GetWidget('game_walkthrough_tip_parent', 'game'):SetNoClick(skippable)
		
		GetWidget('game_walkthrough_tip_icon_label', 'game'):SetText(label)
		GetWidget('game_walkthrough_tip_icon_desc', 'game'):SetText(description)
		
		if(iconTexture ~= '') then
			GetWidget('game_walkthrough_tip_icon', 'game'):SetTexture(iconTexture)
		else
			GetWidget('game_walkthrough_tip_icon', 'game'):SetTexture(Translate('walkthrough_tip_default_icon'))
		end
		GetWidget('game_walkthrough_tip_icon', 'game'):SetVisible(true)
		GetWidget('game_walkthrough_tip_parent', 'game'):SetVisible(true)
		GetWidget('game_walkthrough_tip_icon_label', 'game'):SetVisible(true)
		GetWidget('game_walkthrough_tip_icon_desc', 'game'):SetVisible(true)
	end
	
	Game_Walkthrough.PlayTranslatedSound(sound)
end

function Game_Walkthrough.ShopActive(self, value)
	
	if(value == 'true') then
		interface:UICmd('SendScriptMessage(\'walkthrough_shop_opened\', \'1\');')
	else
		interface:UICmd('SendScriptMessage(\'walkthrough_shop_closed\', \'1\');')
	end
end

function Game_Walkthrough.StartLvlUpTimer()
	
	--Echo('^707Timer started')
	-- Register a function for the game timer
	interface:RegisterWatch('HostTime', Game_Walkthrough.LvlUpTimerTick)
	-- reset the game timer
	Game_Walkthrough.Storage.lvlUpTargetTime = 0
end

function Game_Walkthrough.LvlUpTimerTick(sourceWidget, newTime)

	-- First time through, store the target time
	if(Game_Walkthrough.Storage.lvlUpTargetTime == 0) then
		Game_Walkthrough.Storage.lvlUpTargetTime = AtoN(newTime) + Game_Walkthrough.Storage.lvlUpWaitDuration 
	end
	Game_Walkthrough.Storage.lvlUpTargetTime = Game_Walkthrough.Storage.lvlUpTargetTime + 1
	--Echo('^707Timer Ticking ' .. Game_Walkthrough.Storage.lvlUpTargetTime .. ' | ' .. tostring(newTime) .. ' | ' .. tostring(Game_Walkthrough.Storage.lvlUpTargetTime - AtoN(newTime)))
	
	-- if time is greater than the amount we specify
	if(AtoN(newTime) > Game_Walkthrough.Storage.lvlUpTargetTime) then
	
		-- launch a tip
		--Echo('^707Blast off!')
		Game_Walkthrough.ShowMiscTip(nil, '17') -- hardcoded tip number for the 'level your skills already, man!'
	
		-- unregister from the timer function
		Game_Walkthrough.StopLvlUpTimer()
		
	end
end

function Game_Walkthrough.StopLvlUpTimer()
	
	--Echo('^707Timer stopped')
	interface:UnregisterWatch('HostTime', Game_Walkthrough.LvlUpTimerTick)
	Game_Walkthrough.Storage.lvlUpTargetTime = 0
end

function Game_Walkthrough.LvlUpActiveInventoryStatus(sourceWidget, canActivate, isActive, isDisabled, needMana, inUse, currentLevel, canLevelUp, maxLevel, isActiveSlot, canShare, isBorrowed, team, recentPurchase, index, iSlot)
	if (AtoB(canLevelUp)) then
		Game_Walkthrough.StartLvlUpTimer()
	else
		Game_Walkthrough.StopLvlUpTimer()
	end
end

function Game_Walkthrough.GoldUpdate(sourceWidget, goldAmount)
	
	if(tonumber(goldAmount) > Game_Walkthrough.Storage.goldTipTriggerAmount) then
		Game_Walkthrough.ShowMiscTip(nil, '18')
	end
end

interface:RegisterWatch('activate_quest', Game_Walkthrough.StartQuestline)
interface:RegisterWatch('walkthrough_misc_quest', Game_Walkthrough.ShowMiscTip)
interface:RegisterWatch('ShopActive', Game_Walkthrough.ShopActive)
interface:RegisterWatch('PlayerGold', Game_Walkthrough.GoldUpdate)
