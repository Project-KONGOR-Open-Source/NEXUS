---------------------------------------------------------- 
--	Name: 		Game UI Manager		         			--				
--  Copyright 2014 Frostburn Studios					--
----------------------------------------------------------

local _G = getfenv(0)
local interface = object
local interfaceName = interface:GetName()

RegisterScript2('GameUIManager', '1')

------- Global Variables -------
GameUIManager = {}


------- Local Variables --------
-- List of stuff pending to be registered
local pending = {
	['features'] = {},
	['sections'] = {},
	['widgets'] = {},
}

-- All the registered features/sections
local features = {}

------- Local Functions --------
-- Functions to handle calling the conditional functions on features
local function GameUIUpdate(map, gameMode)
	printdb("^o^:The GameUIManager is updating the enabled features!")

	for name, featureTable in pairs(features) do
		if (featureTable.conditionFunc) then
			GameUIManager.SetFeatureEnabled(name, featureTable.conditionFunc(map, gameMode))
		end
	end
end
interface:RegisterWatch('GameInfo', function(_, ...) GameUIUpdate(...) end)

-- Creates a feature with the given name and information
local function CreateFeature(featureName, conditionFunc, onEnableFunc, onDisableFunc, dontMakeDormant)
	features[featureName] = {
		['context'] = 'gameui_'..featureName,
		['enabled'] = true,
		['cvar'] = 'gameui_'..featureName,
		['conditionFunc'] = conditionFunc,
		['onEnableFunc'] = onEnableFunc,
		['onDisableFunc'] = onDisableFunc,
		['dontMakeDormant'] = dontMakeDormant,
		['sections'] = {},
	}
end

-- Creates a section with the given name and widget
local function CreateSection(featureName, parent)
	if (not features[featureName]) then
		return false
	end

	features[featureName].sections[parent] = {}
end

-- Finds which section/feature a widget belongs to (searchs up it's parents until it finds on in a section)
local function FindFeatureSectionForWidget(widget)
	if (not widget) then
		return nil, nil
	end

	-- create a table of masterWidgets as keys to make this faster
	local masterWidgets = {}
	for featureName, featureTable in pairs(features) do
		for masterWidget, section in pairs(featureTable.sections) do
			masterWidgets[masterWidget] = featureName
		end
	end

	-- cycle up through each parent until we either have no parent, or find a masterWidget
	local testWidget = widget
	while (testWidget and (not masterWidgets[testWidget])) do
		testWidget = testWidget:GetParent()
	end

	if (not testWidget) then
		return nil, nil
	else
		return masterWidgets[testWidget], testWidget
	end
end
FindSectionForWidget = memoizeR2(FindFeatureSectionForWidget) -- memoize this guy to make it much faster with multiple calls

-- Adds a widget to the given section, with the texture/model/effect to load on enabling, also sets the context
local function AddWidgetToSection(featureName, sectionWidget, widget, texture, model, effect)
	if ((not features[featureName]) or (not sectionWidget) or (not features[featureName].sections[sectionWidget]) or (not widget)) then
		return false
	end

	local widgetTable = features[featureName].sections[sectionWidget][widget] or {}

	if (NotEmpty(texture)) then
		widgetTable.texture = texture
	end

	if (NotEmpty(model)) then
		widgetTable.model = model
	end

	if (NotEmpty(effect)) then
		widgetTable.effect = effect
	end

	features[featureName].sections[sectionWidget][widget] = widgetTable

	-- set a resource context on the widget
	widget:SetResourceContext(features[featureName].context)
end

-- Processes the list of pending sections and widgets, created during the register functions
local function ProcessPending()
	printdb("^o^:The GameUIManager is processing pending informations...")
	-- create each feature, which holds sections
	for featureName, featureTable in pairs(pending.features) do
		CreateFeature(featureName, featureTable.conditionFunc, featureTable.onEnableFunc, featureTable.onDisableFunc, featureTable.dontMakeDormant)
	end

	-- process each section, which is tied to a feature and holds elements
	for widget, featureName in pairs(pending.sections) do
		CreateSection(featureName, widget)
	end
	pending.sections = {}

	-- Process each individual element
	for widget, pendingTable in pairs(pending.widgets) do
		local feature, sectionWidget = FindFeatureSectionForWidget(widget)
		if (feature and sectionWidget) then
			AddWidgetToSection(feature, sectionWidget, widget, pendingTable.texture, pendingTable.model, pendingTable.effect)
		end
	end
	pending.widgets = {}

	-- Disable each section we just added
	for featureName, _ in pairs(pending.features) do
		GameUIManager.DisableFeature(featureName)
	end
	pending.features = {}

	printdb("^o^:The GameUIManager is finished processing!")
end

-- Generic function to make sure the ProcessPending function will be called on frame end
local function RegisterForProcessing()
	interface:RegisterWatch('EndUpdate', function()
		ProcessPending()
		interface:UnregisterWatch('EndUpdate')
	end)
end

-- Takes a bool and a feature name to set something enabled/disabled
function GameUIManager.SetFeatureEnabled(featureName, enabled)
	if (not features[featureName]) then
		return false
	end
	
	if (enabled) then
		return GameUIManager.EnableFeature(featureName)
	else
		return GameUIManager.DisableFeature(featureName)
	end
end

-- Enables the feature with the given name
function GameUIManager.EnableFeature(featureName)
	if (not features[featureName]) then
		return false
	end
	if (features[featureName].enabled) then
		return true
	end

	printdb("^g^:Enabling Feature: "..featureName)

	local featureTable = features[featureName]

	featureTable.enabled = true
	Set(featureTable.cvar, true, 'bool')

	for masterWidget,sectionTable in pairs(featureTable.sections) do
		masterWidget:SetEnabled(true)
		masterWidget:SetVisible(true)

		-- set all the involved widgets to not dormant if we need to
		if (not featureTable.dontMakeDormant) then
			local makeNotDormant
			makeNotDormant = function(widget)
				widget:SetDormant(false)

				local children = widget:GetChildren()
				for _, child in ipairs(children) do
					makeNotDormant(child)
				end
			end
			makeNotDormant(masterWidget)
		end

		for widget, widgetTable in pairs(sectionTable) do
			if (widgetTable.texture) then
				widget:SetTexture(widgetTable.texture)
			end
			if (widgetTable.model) then
				widget:SetModel(widgetTable.model)
			end
			if (widgetTable.effect) then
				widget:SetEffect(widgetTable.effect)
			end
		end
	end

	if (featureTable.onEnableFunc) then
		featureTable.onEnableFunc()
	end

	return true
end

-- Disables the features with the given name
function GameUIManager.DisableFeature(featureName)
	if (not features[featureName]) then
		return false
	end
	if (not features[featureName].enabled) then
		return true
	end

	printdb("^r^:Disabling Feature: "..featureName)

	local featureTable = features[featureName]

	featureTable.enabled = false
	Set(featureTable.cvar, false, 'bool')

	for masterWidget,sectionTable in pairs(featureTable.sections) do
		masterWidget:SetEnabled(false)
		masterWidget:SetVisible(false)

		-- set all the involved widgets to dormant if we need to
		if (not featureTable.dontMakeDormant) then
			local makeDormant
			makeDormant = function(widget)
				widget:SetDormant(true)

				local children = widget:GetChildren()
				for _, child in ipairs(children) do
					makeDormant(child)
				end
			end
			makeDormant(masterWidget)
		end
	end

	DeleteResourceContext(featureTable.context)

	if (featureTable.onDisableFunc) then
		featureTable.onDisableFunc()
	end

	return true
end

-- Creates a feature as pending to be added
function GameUIManager.RegisterFeature(featureName, conditionFunc, onEnableFunc, onDisableFunc, dontMakeDormant)
	pending.features[featureName] =
	{
		['conditionFunc'] = conditionFunc,
		['onEnableFunc'] = onEnableFunc,
		['onDisableFunc'] = onDisableFunc,
		['dontMakeDormant'] = dontMakeDormant,
	}

	RegisterForProcessing()
end

-- Creates a section as pending to be added
function GameUIManager.RegisterSection(widget, featureName)
	pending.sections[widget] = featureName

	RegisterForProcessing()
end

-- Creates a feature and adds to given widget as a section immedietly
function GameUIManager.RegisterFeatureAndSection(widget, featureName, conditionFunc, onEnableFunc, onDisableFunc, dontMakeDormant)
	pending.features[featureName] =
	{
		['conditionFunc'] = conditionFunc,
		['onEnableFunc'] = onEnableFunc,
		['onDisableFunc'] = onDisableFunc,
		['dontMakeDormant'] = dontMakeDormant,
	}

	pending.sections[widget] = featureName

	RegisterForProcessing()
end

-- Adds a widget with a texture as pending to be added to a section
function GameUIManager.RegisterTexture(widget, texture)
	local pendingTable = pending.widgets[widget] or {}
	pendingTable.texture = texture

	pending.widgets[widget] = pendingTable

	RegisterForProcessing()
end

-- Adds a widget with a model as pending to be added to a section
function GameUIManager.RegisterModel(widget, model)
	local pendingTable = pending.widgets[widget] or {}
	pendingTable.model = model
	
	pending.widgets[widget] = pendingTable

	RegisterForProcessing()
end

-- Adds a widget with an effect as pending to be added to a section
function GameUIManager.RegisterEffect(widget, effect)
	local pendingTable = pending.widgets[widget] or {}
	pendingTable.effect = effect
	
	pending.widgets[widget] = pendingTable

	RegisterForProcessing()
end

-- Adds a widget with both a model and effect as pending to be added to a section
function GameUIManager.RegisterModelAndEffect(widget, model, effect)
	local pendingTable = pending.widgets[widget] or {}
	pendingTable.effect = effect
	pendingTable.model = model
	
	pending.widgets[widget] = pendingTable

	RegisterForProcessing()
end

-- Adds a widget, with no special info, to be added to a section (which will put the section resource context on it)
function GameUIManager.RegisterForContext(widget)
	local pendingTable = pending.widgets[widget] or {}

	pending.widgets[widget] = pendingTable

	RegisterForProcessing()
end

-- Can check if a feature is enabled/disabled
function GameUIManager.FeatureEnabled(featureName)
	if (not features[featureName]) then
		return false
	else
		return features[featureName].enabled
	end
end

-- Prints a list of all the registered features as well as basic info about them
function GameUIManager.PrintRegisteredFeatures()
	Echo("^oRegistered Features-----")
	for name, featureTable in pairs(features) do
		local elementCount = 0
		local sectionCount = 0
		for _,sectionTable in pairs(featureTable.sections) do
			sectionCount = sectionCount + 1
			for _,_ in pairs(sectionTable) do
				elementCount = elementCount + 1
			end
		end

		Echo("^yFeature: "..name.." enabled: "..tostring(featureTable.enabled).." numSections: "..sectionCount.." numElements: "..elementCount.." table: "..tostring(featureTable))
	end
end