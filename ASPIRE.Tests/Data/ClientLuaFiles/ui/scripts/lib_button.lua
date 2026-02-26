lib_button = {} -- lib_button or {}
lib_button.visualStateHandlers = lib_button.visualStateHandlers or {}

local interface = object

local ERROR_TYPE_WIDGET		= 1
local ERROR_TYPE_HANDLER	= 2
local ERROR_TYPE_OTHER		= 3

local buttonsRegistered	= 0

local errorTypePrefix = {}
errorTypePrefix[ERROR_TYPE_WIDGET]	= '^258Widget: ^w'
errorTypePrefix[ERROR_TYPE_HANDLER]	= '^964Handler: ^w'
errorTypePrefix[ERROR_TYPE_OTHER]		= '^657Other: ^w'

function lib_button.processError(isWarning, errorType, funcName, errorMessage)
	isWarning	= isWarning or false
	errorType	= errorType or ERROR_TYPE_OTHER

	local typePrefix			= errorTypePrefix[errorType]
	local severityPrefix		= '^932Error: '

	if isWarning then
		severityPrefix = '^960Warning: '
	end

	print('^wlib_button '..severityPrefix..typePrefix..errorMessage..'\n')
end

function lib_button.validateVisualStateHandler(handlerInfo)
	local funcName = 'validateVisualStateHandler'
	if handlerInfo.handlerName and type(handlerInfo.handlerName) == 'string' and string.len(handlerInfo.handlerName) > 0 then
		if lib_button.visualStateHandlers[handlerInfo.handlerName] then
			lib_button.processError(true, ERROR_TYPE_HANDLER, funcName, 'State handler named '..handlerInfo.handlerName..' already exists and will not be overwritten.')
			return false
		end

		if (not (handlerInfo.stateFunctions and type(handlerInfo.stateFunctions) == 'table')) then
			lib_button.processError(false, ERROR_TYPE_HANDLER, funcName, 'State handler named '..handlerInfo.handlerName..' does not have valid state information.')
			return false
		end

		local stateFunctionCount = 0
		for k,v in pairs(handlerInfo.stateFunctions) do
			stateFunctionCount = stateFunctionCount + 1
		end
		if stateFunctionCount < 2 then
			lib_button.processError(true, ERROR_TYPE_HANDLER, funcName, 'State handler named '..handlerInfo.handlerName..' only contains a single state!')
		end

		if handlerInfo.validateFunction ~= nil and type(handlerInfo.validateFunction) ~= 'function' then
			lib_button.processError(true, ERROR_TYPE_HANDLER, funcName, 'State handler named '..handlerInfo.handlerName..' contains invalid validateFunction, discarding.')
			handlerInfo.validateFunction = nil
		end

		if not (handlerInfo.requiredWidgets and type(handlerInfo.requiredWidgets) == 'table') then
			handlerInfo.requiredWidgets = {}
		end

		table.insert(handlerInfo.requiredWidgets, 'Button')

		return true
	else
		lib_button.processError(false, ERROR_TYPE_HANDLER, funcName, 'Empty or otherwise invalid state handler name.')
		return false
	end
end

function lib_button.validateWidget(buttonInfo, widgetName)
	local funcName = 'validateWidget'

	local buttonNameString = ''

	if buttonInfo.name then
		buttonNameString = ' (button name = '..buttonInfo.name..') '
	end

	local interfaceNameString = ''

	if buttonInfo.interfaceName then
		interfaceNameString = ' (interface = '..buttonInfo.interfaceName..') '
	end

	if not (widgetName and type(widgetName) == 'string' and string.len(widgetName) > 0) then
		lib_button.processError(true, ERROR_TYPE_WIDGET, funcName, 'Invalid widget name.')
		return false
	end

	if not buttonInfo.widgets[widgetName] then
		lib_button.processError(true, ERROR_TYPE_WIDGET, funcName, 'Nonexistant required widget named '..widgetName..'.'..buttonNameString..interfaceNameString)
		return false
	else
		if not (type(buttonInfo.widgets[widgetName]) == 'userdata' and buttonInfo.widgets[widgetName]:IsValid()) then
			lib_button.processError(true, ERROR_TYPE_WIDGET, funcName, 'Invalid widget named '..widgetName..'.'..buttonNameString..interfaceNameString)
			return false
		end
	end

	return true
end

function lib_button.addRequiredWidget(buttonInfo, widgetName, nameOverride)
	buttonInfo.widgets[nameOverride or widgetName] = buttonInfo.self:GetWidget(buttonInfo.name..widgetName)
end

function lib_button.addVisualStateHandler(handlerName, requiredWidgets, stateFunctions, validateFunction)	--, extraHandlerInfo
	local handlerInfo = {
		handlerName				= handlerName,
		requiredWidgets			= requiredWidgets,
		stateFunctions			= stateFunctions,
		validateFunction		= validateFunction,
	}

	if lib_button.validateVisualStateHandler(handlerInfo) then
		lib_button.visualStateHandlers[handlerName] = handlerInfo
	end
end

function lib_button.getWidgetsFromName(buttonInfo)
	local funcName = 'getWidgetsFromName'
	if buttonInfo and type(buttonInfo) == 'table' then
		buttonInfo.widgets = buttonInfo.widgets or {}
		if buttonInfo.visualStateHandler and type(buttonInfo.visualStateHandler) == 'table' then
			local missingWidget = false

			if not buttonInfo.widgets.Button then
				lib_button.addRequiredWidget(buttonInfo, '', 'Button')
			end

			for k,widgetName in ipairs(buttonInfo.visualStateHandler.requiredWidgets) do
				if not buttonInfo.widgets[widgetName] and widgetName ~= 'Button' then
					lib_button.addRequiredWidget(buttonInfo, widgetName)
				end
				
				if not lib_button.validateWidget(buttonInfo, widgetName) then
					missingWidget = true
				end
			end

			return (not missingWidget)
		else
			lib_button.processError(false, ERROR_TYPE_HANDLER, funcName, 'No visual state handler for button named '..tostring(buttonInfo.name)..'.')
			return false
		end
	else
		lib_button.processError(false, ERROR_TYPE_OTHER, funcName, 'Empty button info.')
		return false
	end
end

function lib_button.attachStateHandler(buttonInfo, stateHandlerName)
	local funcName = 'attachStateHandler'
	if stateHandlerName and type(stateHandlerName) == 'string' and string.len(stateHandlerName) > 0 then
		if lib_button.visualStateHandlers[stateHandlerName] then
			buttonInfo.visualStateHandler = lib_button.visualStateHandlers[stateHandlerName]
		else
			lib_button.processError(false, ERROR_TYPE_HANDLER, funcName, 'No visual state handler named '..stateHandlerName..'.')
			return false
		end
	else
		lib_button.processError(false, ERROR_TYPE_HANDLER, funcName, 'Invalid visual state handler name.')
		return false
	end
	return true
end

function lib_button.register(stateHandlerName, baseButtonInfo, fromNameless)
	fromNameless = fromNameless or false

	if fromNameless then
		buttonsRegistered = buttonsRegistered + 1
	end

	local funcName = 'register'
	local buttonInfo = {}

	local hasStateHandler = false
	if not buttonInfo.visualStateHandler then
		hasStateHandler = lib_button.attachStateHandler(buttonInfo, stateHandlerName)
	end

	if not hasStateHandler then
		return false
	end

	if baseButtonInfo and type(baseButtonInfo) == 'table' then
		for k,v in pairs(baseButtonInfo) do
			buttonInfo[k] = v
		end
	end

	if not (buttonInfo.visualStateHandler.validateFunction(buttonInfo)) then
		lib_button.processError(true, ERROR_TYPE_HANDLER, funcName, 'Custom validator for '..stateHandlerName..' returned false.  Skipping registration.')
		return false
	end

	buttonInfo.widgets.Button:SetCallback('onbutton', function(widget, currentState, oldState)
		if currentState ~= oldState and (currentState ~= 'over' or (oldState ~= 'downright' or widget:GetCallback('onrightclick'))) then
			if buttonInfo.visualStateHandler.stateFunctions[currentState] and type(buttonInfo.visualStateHandler.stateFunctions[currentState]) == 'function' then
				buttonInfo.visualStateHandler.stateFunctions[currentState](buttonInfo, oldState)
			end
			if hasExtraStateHandlers and extraStateHandlers[currentState] and type(extraStateHandlers[currentState]) == 'function' then
				extraStateHandlers[currentState](widget, buttonInfo, oldState)
			end
		end
	end)
	
	local disabledFunc		= buttonInfo.visualStateHandler.stateFunctions.disabled
	
	if (not buttonInfo.widgets.Button:IsEnabled()) and disabledFunc and type(disabledFunc) == 'function' then
		disabledFunc(buttonInfo)	-- if disabled upon initialization
	end

	return buttonInfo
end

function lib_button.initializeTemplate(buttonName, widget, stateHandlerName, baseButtonInfo)
	local funcName = 'initializeTemplate'
	if not (buttonName and type(buttonName) == 'string' and string.len(buttonName) > 0) then
		lib_button.processError(false, ERROR_TYPE_OTHER, funcName, 'Empty button info.')
		return false
	end

	local buttonInfo = {
		name		= buttonName,

		widgets		= {
			Button		= widget:GetWidget(buttonName)
		},
		interfaceName = widget:GetInterface():GetName(),
		self		= widget,
	}

	if baseButtonInfo and type(baseButtonInfo) == 'table' then
		for k,v in pairs(baseButtonInfo) do
			buttonInfo[k] = v
		end
	end

	if not lib_button.attachStateHandler(buttonInfo, stateHandlerName) then
		return false
	end

	if lib_button.getWidgetsFromName(buttonInfo) then
		return lib_button.register(stateHandlerName, buttonInfo)	-- including widgets
	else
		return false	-- Error would occur from validateWidget
	end
end

local initializeNamelessQueue = {}


-- This is effectively a hack to compensate for the enormous amount of unnamed button widgets
function lib_button.initializeNamelessWidget(buttonName, widget, widgetName, stateHandlerName, baseButtonInfo)
	if string.len(buttonName) > 0 then
		return
	end

	local hasStateHandler = false

	if not initializeNamelessQueue[buttonsRegistered] then
		initializeNamelessQueue[buttonsRegistered] = {}
	end

	initializeNamelessQueue.interfaceName = widget:GetInterface():GetName()
	initializeNamelessQueue.self = widget

	if not initializeNamelessQueue[buttonsRegistered].visualStateHandler then
		hasStateHandler = lib_button.attachStateHandler(initializeNamelessQueue[buttonsRegistered], stateHandlerName)
	else
		hasStateHandler = true
	end

	if hasStateHandler then
		initializeNamelessQueue[buttonsRegistered].widgets = initializeNamelessQueue[buttonsRegistered].widgets or {}
		initializeNamelessQueue[buttonsRegistered].widgets[widgetName] = widget
		for k,v in ipairs(initializeNamelessQueue[buttonsRegistered].visualStateHandler.requiredWidgets) do
			if initializeNamelessQueue[buttonsRegistered].widgets[v] == nil then
				return
			end
		end

		if baseButtonInfo and type(baseButtonInfo) == 'table' then
			for k,v in pairs(baseButtonInfo) do
				initializeNamelessQueue[buttonsRegistered][k] = v
			end
		end
		lib_button.register(stateHandlerName, initializeNamelessQueue[buttonsRegistered], true)
		-- initializeNamelessQueue[buttonsRegistered] = nil
	end
end