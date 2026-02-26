--------------------------------------
--		Objective System 			--
--------------------------------------

-- This is not the file you are looking for, try 'objectives.lua'


--[[

				'disableCheckbox', disableCheckbox or 'false',
				'disableCvar', disableCvar or ''

--]]

-------------------------- Variables ------------------------
local interface = object

HoN_Objectives = {}			-- Globally accessable functionality, mainly for registering things

local functionTable = {}
local registered = {
	['objectives'] = {},
	['tips'] = {},
	['dialogs'] = {},
	['helps'] = {},
	['sounds'] = {},
	['highlights'] = {},
	['chains'] = {},
}

local idCounter = 0
local numHelps = 0

--------------------------------------------------------------

--------------------- Local function -------------------------
-- GetWidget override
local function GetWidgetGameObj(widget, fromInterface, hideErrors)
	--println('GetWidget ' .. tostring(widget) .. ' in interface ' .. tostring(fromInterface)) 
	if (widget) then
		local returnWidget		
		if (fromInterface) then
			returnWidget = UIManager.GetInterface(fromInterface):GetWidget(widget)		
		else
			returnWidget = interface:GetWidget(widget)
		end	
		if (returnWidget) then
			return returnWidget
		else
			if (not hideErrors) then println('GetWidget Game_Objectives failed to find ' .. tostring(widget) .. ' in interface ' .. tostring(fromInterface)) end
			return nil		
		end	
	else
		println('GetWidget called without a target')
		return nil
	end
end
local GetWidget = memoizeObject(GetWidgetGameObj)

-- Translate if not nil
local function TranslateNil(string)
	if (string) then
		return Translate(string)
	end

	return nil
end

-- Debug echo
local function EchoDB(string)
	if (GetCvarBool('ui_debugObjectives')) then
		Echo('^:Objectives: ^*'..string)
	end
end

-- Creates a trigger if the name is valid
local function CreateTrigger(triggerName)
	if (triggerName and NotEmpty(triggerName)) then
		UITrigger.CreateTrigger(triggerName)
	end
end

-- Deletes a trigger, if it's custom and valid
local function DeleteTrigger(triggerName)
	if (triggerName and NotEmpty(triggerName)) then
		local trigger = UITrigger.GetTrigger(triggerName)
		if (trigger and trigger:IsCustom()) then
			UITrigger.DeleteTrigger(triggerName)
		end
	end
end

-- Get and increment unique id
local function GetID()
	local ret = idCounter
	idCounter = idCounter + 1

	return ret
end

-- Register function to be ran on a trigger firing
local function registerFunctionOnTrigger(triggerName, funct)
	if (triggerName and NotEmpty(triggerName)) then
		if (not functionTable[triggerName]) then
			functionTable[triggerName] = {}
		end

		table.insert(functionTable[triggerName], funct)

		interface:RegisterWatch(triggerName, function(_, ...)
			for _,funct in ipairs(functionTable[triggerName]) do
				funct(...)
			end
		end)
	end
end


-----------------------------------------------------------------


---------------------- Global Functions --------------------------
-- Register an objective
function HoN_Objectives.RegisterObjective(
	startTrigger,			-- The trigger to watch to start this objective
	mainLabel,				-- The main text for the objective
	maxValue,				-- The value we are trying to reach for any value based objects (x / 100 creep kills, etc)
	valueTrigger,			-- The trigger to watch to get the current progress value
	tipTrigger,				-- The trigger to fire for clicking for help
	pingScriptMessage,		-- The script message to send to use the map ping help system
	icon,					-- Any icon for the objective
	completeTrigger		-- The trigger to watch for the objective being complete

)

	EchoDB("Registering an objective with startTrigger '"..startTrigger.."'!")

	-- Create triggers needed for the objective
	CreateTrigger(startTrigger)
	CreateTrigger(valueTrigger)
	CreateTrigger(tipTrigger)
	CreateTrigger(completeTrigger)

	-- Build the objective
	local objTable = {}

	local mainLabel			= TranslateNil(mainLabel)

	local value 			= 0	-- The last value from the value trigger
	local active 			= false	-- If this is active
	local complete 			= false -- If the objective is complete
	local success 			= false -- If the objective was completed successfully
	local id 				= 'game_objective_quest_'..GetID()

	function objTable:Add()
		if (not active) then
			active = true

			GetWidget('game_objective_quests_hook'):Instantiate('game_objective_quest',
			'id', id,
			'text', mainLabel,
			'icon', icon,
			'hasValue', tostring((maxValue ~= nil) and NotEmpty(maxValue)),
			'value', (tostring(value) .. ' / ' .. tostring(maxValue))
			)
		end
	end

	function objTable:Update(newValue)
		if (active) then
			value = newValue
			GetWidget(id..'_value'):SetText(tostring(value) .. ' / ' .. tostring(maxValue))
		end
	end

	function objTable:Remove()
		if (active) then
			local widget = GetWidget(id, nil, true)
			if (widget) then
				widget:Destroy()
			end
		end
	end

	function objTable:Reset()
		self:Remove()
		active = false
		succeeded = false
		complete = false
		value = 0
	end

	function objTable:Complete(succeeded)
		if (active) then
			complete = true
			success = succeeded

			GetWidget(id):Sleep(2500, function(widget)
				widget:FadeOut(1500)
				widget:Sleep(1490, function(widget)
					self:Reset()
				end)
			end)

			GetWidget(id..'_success'):SetVisible(succeeded)
			GetWidget(id..'_failure'):SetVisible(not succeeded)
		end
	end

	function objTable:IsActive()
		return active
	end

	function objTable:SendScriptMessage()
		SendScriptMessage(pingScriptMessage, '1')
	end

	function objTable:FireTipTrigger()
		Trigger(tipTrigger)
	end

	table.insert(registered.objectives, objTable)

	-- Register watches as needed
	registerFunctionOnTrigger(startTrigger, function(...)
		objTable:Add()
	end)

	registerFunctionOnTrigger(valueTrigger, function(...)
		if (not arg[1]) then
			EchoDB("^rObjective valueTrigger '"..valueTrigger.."' fired without a value!")
		else
			objTable:Update(tonumber(arg[1]))
		end
	end)

	registerFunctionOnTrigger(completeTrigger, function(...)
		if (not arg[1]) then
			EchoDB("^rObjective completeTrigger '"..completeTrigger.."' fired without a boolean!")
		else
			objTable:Complete(AtoB(arg[1]))
		end
	end)
end

-- Register a sound
function HoN_Objectives.RegisterSound(
	playTrigger,	-- Trigger that causes the sound to play
	soundPath		-- Path to the sound
)
	EchoDB("Registering a sound with playTrigger '"..playTrigger.."'!")

	-- Create triggers needed for the sound
	CreateTrigger(playTrigger)

	local sndTable = {}

	local soundPath			= Translate(soundPath)	-- Translate so that we can do localized sounds
	local id 				= 'game_objective_sound_'..GetID()

	function sndTable:Play()
		PlaySound(soundPath)
	end

	table.insert(registered.sounds, sndTable)

	-- Register watches as needed
	registerFunctionOnTrigger(playTrigger, function(...)
		sndTable:Play()
	end)
end

-- Register a widget hightlight
function HoN_Objectives.RegisterWidgetHighlight(
	playTrigger,	-- Trigger to cause the highlight
	widgetName,		-- Widget to highlight
	widgetInterface,-- Interface that the widget you want to highlight is in
	time,			-- Length of time to highlight (ms)
	stopTrigger,	-- Trigger to stop the effect instantly
	specialEffect	-- Any special effect to play on the highlight widget
)
	EchoDB("Registering a widget highlight with playTrigger '"..playTrigger.."'!")

	-- Create triggers needed for the highlight
	CreateTrigger(playTrigger)
	CreateTrigger(stopTrigger)

	local highlgtTable = {}

	local active 			= false	-- If this is active
	local id 				= 'game_objective_highlight_'..GetID()

	function highlgtTable:Highlight(highlightTime)
		if (not active) then
			active = true

			local highlightWidget = GetWidget(widgetName, widgetInterface or 'game', true)

			if (highlightWidget) then
				GetWidget('game_objective_helper'):Instantiate(
					'game_objective_highlight',
					'id', id,
					'effect', specialEffect or ''
				)

				local highlighter = GetWidget(id)
				if (highlighter) then
					highlighter:SetParent(highlightWidget)
					highlighter:BringToFront()
					highlighter:FadeIn(100)

					if (highlightTime) then
						highlighter:Sleep(highlightTime, function(widget)
							self:Stop()
						end)
					end
				end
			end
		end
	end

	function highlgtTable:Remove()
		if (active) then
			local widget = GetWidget(id, nil, true)
			if (widget) then
				widget:Destroy()
			end
		end
	end

	function highlgtTable:Reset()
		if (active) then
			self:Remove()
			active = false
		end
	end

	function highlgtTable:IsActive()
		return active
	end

	function highlgtTable:Stop()
		if (active) then
			local widget = GetWidget(id)

			widget:FadeOut(100)
			widget:Sleep(100, function(widget)
				self:Reset()
			end)
		end
	end

	table.insert(registered.highlights, highlgtTable)

	-- Register watches as needed
	registerFunctionOnTrigger(playTrigger, function(...)
		highlgtTable:Highlight(time)
	end)

	registerFunctionOnTrigger(stopTrigger, function(...)
		highlgtTable:Stop()
	end)
end

-- Registers a tip
function HoN_Objectives.RegisterTip(
	tipTrigger,				-- The trigger to add the tip
	tipLabel,				-- The label to show on the small tip
	tipIcon,				-- The icon for the tip
	clickTrigger,			-- The trigger that fires when the tip is clicked
	tipLife,				-- How long the tip lasts before it disappears
	removeTrigger,			-- Trigger to remove the tip
	skipRegistrationCvar	-- Skip registration if this cvar is true
)

	if skipRegistrationCvar and GetCvarBool(skipRegistrationCvar) then
		return
	end

	EchoDB("Registering a tip with tipTrigger '"..tipTrigger.."'!")

	-- Create triggers needed for the tip
	CreateTrigger(tipTrigger)
	CreateTrigger(clickTrigger)
	CreateTrigger(removeTrigger)

	local tipTable = {}

	local tipLabel			= TranslateNil(tipLabel)
	local active 			= false	-- If this is active
	local id 				= 'game_objective_tip_'..GetID()

	function tipTable:AddTip(lifetime)
		if (not active) then
			active = true

			GetWidget('game_objectives_tips'):Instantiate('game_objective_tip',
				'id', id,
				'tipIcon', tipIcon,
				'tipText', tipLabel
			)
			local tipWidget = GetWidget(id)

			self:Highlight(5000)

			tipWidget:SetCallback('onclick', function()
				self:OnClick()
			end)

			tipWidget:SetCallback('onmouseover', function()
				self:RemoveHighlight()
			end)

			if (lifetime) then
				tipWidget:Sleep(lifetime, function()
					self:Close()
				end)
			end

			tipWidget:RefreshCallbacks()
		else
			self:Highlight(2500)
		end
	end

	function tipTable:IsActive()
		return active
	end

	function tipTable:Highlight(time)
		if (active) then
			local highlightWidget = GetWidget(id..'_highlight')
			highlightWidget:SetVisible(1)

			highlightWidget:Sleep(time, function()
				self:RemoveHighlight()
			end)
		end
	end

	function tipTable:RemoveHighlight()
		if (active) then
			GetWidget(id..'_highlight'):SetVisible(0)
		end
	end

	function tipTable:OnClick()
		if (active) then
			if (clickTrigger and NotEmpty(clickTrigger)) then
				Trigger(clickTrigger)
			end

			self:Close()
		end
	end

	function tipTable:Reset()
		if (active) then
			self:Remove()

			active = false
		end
	end

	function tipTable:Remove()
		if (active) then
			local tipWidget = GetWidget(id, nil, true)
			if (tipWidget) then
				tipWidget:Destroy()
			end
		end
	end

	function tipTable:Close()
		local tipWidget = GetWidget(id)
		tipWidget:FadeOut(250)
		tipWidget:Sleep(250, function(widget)
			self:Reset()
		end)
	end

	table.insert(registered.tips, tipTable)

	-- Register watches as needed
	registerFunctionOnTrigger(tipTrigger, function(...)
		tipTable:AddTip(tipLife)
	end)

	registerFunctionOnTrigger(removeTrigger, function(...)
		tipTable:Close()
	end)
end

-- Registers a dialog
function HoN_Objectives.RegisterDialog(
	dialogTrigger,	-- Trigger to show the dialog
	dialogTitle,	-- Title text (such as a name) for the dialog
	dialogText,		-- The text for the dialog
	dialogImage,	-- The image for the dialog
	dialogLife,		-- How long the dialog is visible
	hideTrigger		-- Trigger to hide the dialog
)
	EchoDB("Registering a dialog with dialogTrigger '"..dialogTrigger.."'!")

	-- Create triggers needed for the dialog
	CreateTrigger(dialogTrigger)
	CreateTrigger(hideTrigger)

	local diagTable = {}

	local dialogText	= TranslateNil(dialogText)
	local active 		= false	-- If this is active
	local id 			= 'game_objective_dialog_'..GetID()

	function diagTable:Show(lifetime)
		if (not active) then
			active = true

			local width = '100%'
			if (dialogImage and NotEmpty(dialogImage)) then
				width = '-100@'
			end

			GetWidget('game_objectives_dialog'):Instantiate('game_objective_dialog',
				'id', id,
				'hasImage', tostring((dialogImage ~= nil) and NotEmpty(dialogImage)),
				'image', tostring(dialogImage),
				'textAreaWidth', width,
				'name', dialogTitle,
				'text', dialogText
			)

			if (lifetime) then
				GetWidget(id):Sleep(lifetime, function()
					self:Hide()
				end)
			end
		end
	end

	function diagTable:Hide()
		if (active) then
			local widget = GetWidget(id)

			widget:FadeOut(250)
			widget:Sleep(240, function()
				self:Reset()
			end)
		end
	end

	function diagTable:Reset()
		if (active) then
			self:Remove()

			active = false
		end
	end

	function diagTable:Remove()
		if (active) then
			local widget = GetWidget(id, nil, true)
			if (widget) then
				widget:Destroy()
			end
		end
	end

	function diagTable:IsActive()
		return active
	end

	table.insert(registered.dialogs, diagTable)

	-- Register watches as needed
	registerFunctionOnTrigger(dialogTrigger, function(...)
		diagTable:Show(dialogLife)
	end)

	registerFunctionOnTrigger(hideTrigger, function(...)
		diagTable:Hide()
	end)
end

-- Registers an event chain
function HoN_Objectives.RegisterEventChain(
	startTrigger,	-- The trigger that starts the chain
	chainLinks,		-- Table that contains each link in the chain
	stopTrigger		-- Trigger that stops a running chain
)
	EchoDB("Registering a chain with startTrigger '"..startTrigger.."'!")

	-- Create triggers needed for the chain
	CreateTrigger(startTrigger)
	CreateTrigger(stopTrigger)

	local chainTable = {}

	local active 		= false	-- If this is active
	local id 			= 'game_objective_chainer_'..GetID()

	local chainLink 	= nil

	local function stepChain()
		if (chainLinks[chainLink]) then
			for _,trigger in pairs(chainLinks[chainLink].triggers) do
				if (trigger ~= startTrigger) then
					Trigger(trigger)
				else
					EchoDB('^r^:You are trying to chain an infinite loop! YOU FOOL!')
				end
			end

			if (chainLinks[chainLink].time) then
				GetWidget(id):Sleep(chainLinks[chainLink].time, function()
					chainLink = chainLink + 1
					stepChain()
				end)
			else
				chainTable:EndChain()
			end
		else
			chainTable:EndChain()
		end
	end

	function chainTable:StartChain()
		if (not active) then
			active = true

			GetWidget('game_objective_helper'):Instantiate(
				'game_objective_sleeper',
				'id', id
			)

			chainLink = 1

			stepChain()
		end
	end

	function chainTable:IsActive()
		return active
	end

	function chainTable:EndChain()
		if (active) then
			self:Reset()
		end
	end

	function chainTable:Reset()
		if (active) then
			self:Remove()

			active = false
			chainLink = nil
		end
	end

	function chainTable:Remove()
		if (active) then
			local widget = GetWidget(id, nil, true)
			if (widget) then
				widget:Destroy()
			end
		end
	end

	table.insert(registered.chains, chainTable)

	-- Register watches as needed
	registerFunctionOnTrigger(startTrigger, function(...)
		chainTable:StartChain()
	end)

	registerFunctionOnTrigger(stopTrigger, function(...)
		chainTable:EndChain()
	end)
end

-- Registers a full screen help window
function HoN_Objectives.RegisterHelp(
	displayTrigger,		-- The trigger that causes the window to display
	title,				-- The title on the window
	image,				-- The image in the window
	body,				-- The main body text for the window, next to the image
	life,				-- Lifetime of the window before it disappears
	hideTrigger,			-- Trigger to hide the box
	disableCvar
)
	EchoDB("Registering a help window with displayTrigger '"..displayTrigger.."'!")

	-- Create triggers needed for the help
	CreateTrigger(displayTrigger)
	CreateTrigger(hideTrigger)

	local helpTable = {}
	local title 			= TranslateNil(title)
	local body 			= TranslateNil(body)
	local active 			= false -- If this is active
	local id 				= 'game_objective_help_'..GetID()

	function helpTable:Show(lifetime)
		if (not active) then
			active = true

			numHelps = numHelps + 1

			-- Bring up the cover
			GetWidget('game_objectives_help_cover'):SetVisible(1)
			-- Grayscale and pause
			Set('vid_postEffectPath', '/core/post/grayscale_noblur.posteffect')
			Cmd('ServerPause')

			local textWidth = '-15@'
			if (image and NotEmpty(image)) then
				textWidth = '-100@'
			end

			GetWidget('game_objectives_help'):Instantiate('game_objectives_help_window',
				'id', id,
				'textWidth', textWidth,
				'title', title or '',
				'body', body or '',
				'hasImage', tostring((image ~= nil) and NotEmpty(image)),
				'image', image or '',
				'disable_cvar', disableCvar or ''
			)

			local bodyText = GetWidget(id..'_text')
			local scrollbar = GetWidget(id..'_scrollbar')

			local sizeDiff = bodyText:GetHeight() - bodyText:GetParent():GetHeight()

			if (sizeDiff <= 0) then
				scrollbar:SetVisible(0)
			else
				scrollbar:SetMaxValue(sizeDiff)
				scrollbar:SetValue(0)
				scrollbar:SetVisible(1)
			end

			if (lifetime) then
				GetWidget(id):Sleep(lifetime, function()
					self:Hide()
				end)
			end

			GetWidget(id..'_close'):SetCallback('onclick', function()
				self:Hide()
			end)
		end
	end

	function helpTable:Hide()
		if (active) then
			local widget = GetWidget(id)
			widget:FadeOut(150)

			widget:Sleep(140, function()
				self:Reset()

				numHelps = numHelps - 1

				if (numHelps <= 0) then
					GetWidget('game_objectives_help_cover'):SetVisible(0)
					Set('vid_postEffectPath', '/core/post/bloom.posteffect')
					Cmd('ServerUnpause')
				end
			end)
		end
	end

	function helpTable:IsActive()
		return active
	end

	function helpTable:Reset()
		if (active) then
			self:Remove()

			active = false
		end
	end

	function helpTable:Remove()
		if (active) then
			local widget = GetWidget(id, nil, true)
			if (widget) then
				widget:Destroy()
			end
		end
	end

	table.insert(registered.helps, helpTable)

	-- Register watches as needed
	registerFunctionOnTrigger(displayTrigger, function(...)
		helpTable:Show(life)
	end)

	registerFunctionOnTrigger(stopTrigger, function(...)
		helpTable:Hide()
	end)
end

-- Sets the quest dialog visible and positions it
function HoN_Objectives.EnableQuests(enabled)
	if (enabled) then
		EchoDB('^gEnabling Quests!')
		-- Position the quests panel from here, use conditions if needed to make it look nice in different interfaces
		local quests = GetWidget('game_objectives_quests')

		-- Default, original game interface positioning
		quests:SetY('10h')
		quests:SetX('-2')
		quests:SetWidth('25h')
		quests:SetAlign('right')

		quests:SetVisible(1)
	else
		EchoDB('^rDisabling Quests!')
		GetWidget('game_objectives_quests'):SetVisible(0)
	end
end

-- Sets the dialog area visible and positions it
function HoN_Objectives.EnableDialog(enabled)
	if (enabled) then
		EchoDB('^gEnabling Dialog!')

		local dialog = GetWidget('game_objectives_dialog')

		-- Default, original game interface positioning
		dialog:SetWidth('45%')
		dialog:SetHeight('17%')
		dialog:SetY('-20%')
		dialog:SetVAlign('bottom')
		dialog:SetAlign('center')

		dialog:SetVisible(1)
	else
		EchoDB('^rDisabling Dialog!')
		GetWidget('game_objectives_dialog'):SetVisible(0)
	end
end

-- Sets the tip area visible and positions it
function HoN_Objectives.EnableTips(enabled)
	if (enabled) then
		EchoDB('^gEnabling Tips!')

		local tip = GetWidget('game_objectives_tips')

		-- Default, original game interface positioning
		tip:SetWidth('12%')
		tip:SetHeight('20%')
		tip:SetY('-15%')
		tip:SetX('-2')
		tip:SetVAlign('bottom')
		tip:SetAlign('right')

		tip:SetVisible(1)
	else
		EchoDB('^rDisabling Tips!')
		GetWidget('game_objectives_tips'):SetVisible(0)
	end
end

-- For clearing out past registered stuff
function HoN_Objectives.Reset()
	EchoDB('^yResetting the objectives interface!')
	-- Hide all the things
	HoN_Objectives.EnableQuests(false)
	HoN_Objectives.EnableDialog(false)
	HoN_Objectives.EnableTips(false)

	-- Reset anything done by pausing
	GetWidget('game_objectives_help_cover'):SetVisible(0)
	Set('vid_postEffectPath', '/core/post/bloom.posteffect')
	Cmd('ServerUnpause')

	-- iterate over the registered table and try and call any existing remove functions
	for category, registers in pairs(registered) do
		for _, obj in ipairs(registers) do
			if (obj.Remove) then
				obj:Remove()
			end
		end
	end

	-- Clear out the registered table
	local registered = {
		['objectives'] = {},
		['tips'] = {},
		['dialogs'] = {},
		['helps'] = {},
		['sounds'] = {},
		['highlights'] = {},
		['chains'] = {},
	}

	-- Remove any old watches
	for trigger,func in pairs(functionTable) do
		interface:UnregisterWatch(trigger)
	end

	functionTable = {}	-- Remove any previously registered functions

	idCounter = 0		-- Reset the id counter
	numHelps = 0		-- Reset the number of help windows active
end

function HoN_Objectives.TurnOff()
	local iface = UIManager.GetInterface('game_objectives')

	if (iface and iface:IsVisible()) then
		EchoDB('^rTurning the objectives interface off!')

		UIManager.GetInterface('game_objectives'):SetVisible(0)
		HoN_Objectives.Reset()
	end
end

function HoN_Objectives.TurnOn()
	EchoDB('^gTurning the objectives interface on!')
	HoN_Objectives.Reset()

	UIManager.GetInterface('game_objectives'):SetVisible(1)
end
------------------------------------------------------------------