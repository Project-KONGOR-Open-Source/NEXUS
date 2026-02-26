----------------------------------------------------------
--	Name: 		Resource Manager Script	       			--
--  Copyright 2015 Frostburn Studios					--
----------------------------------------------------------

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()

ResourceManager = {}

------ Private Variables
local contextActive = {}
local textureResources = {}
local modelResources = {}
local effectResources = {}

------- Private Functions
local function printdb(str)
	if (GetCvarBool('ui_debugResourceManager')) then
		Echo(str)
	end
end

local function LoadTexturesFromContext(context)
	if (textureResources[context]) then
		local contextTable = textureResources[context]

		for widget, texture in pairs(contextTable) do
			widget:SetTexture(texture)
		end
	end
end

local function LoadModelsFromContext(context)
	if (modelResources[context]) then
		local contextTable = modelResources[context]

		for widget, model in pairs(contextTable) do
			widget:SetModel(model)
		end
	end
end

local function LoadEffectsFromContext(context)
	if (effectResources[context]) then
		local contextTable = effectResources[context]

		for widget, effect in pairs(contextTable) do
			widget:SetEffect(effect)
		end
	end
end

------- Public Functions
function ResourceManager.RegisterTexture(texturePath, context, widget)
	if (not (texturePath and context and widget)) then
		return
	end

	-- create the context in the context list/active if it doesn't exist
	contextActive[context] = contextActive[context] or false
	textureResources[context] = textureResources[context] or {}

	textureResources[context][widget] = texturePath
end

function ResourceManager.RegisterModel(modelPath, context, widget)
	if (not (modelPath and context and widget)) then
		return
	end

	-- create the context in the context list/active if it doesn't exist
	contextActive[context] = contextActive[context] or false
	modelResources[context] = modelResources[context] or {}

	modelResources[context][widget] = modelPath
end

function ResourceManager.RegisterEffect(effectPath, context, widget)
	if (not (effectPath and context and widget)) then
		return
	end

	-- create the context in the context list/active if it doesn't exist
	contextActive[context] = contextActive[context] or false
	effectResources[context] = effectResources[context] or {}

	effectResources[context][widget] = effectPath
end

function ResourceManager.LoadContext(context)
	printdb("^cLoading context ^y'ui:"..context.."'")

	contextActive[context] = true

	LoadTexturesFromContext(context)
	LoadModelsFromContext(context)
	LoadEffectsFromContext(context)
end

function ResourceManager.UnloadContext(context, instant)
	if ((not context) or (contextActive[context] == nil)) then
		return
	end

	printdb("^cRequested to unload context ^y'ui:"..context.."'")

	if (instant) then
		printdb("^cUnloading context ^y'ui:"..context.."'")

		contextActive[context] = false
		DeleteResourceContext(context)
	else
		contextActive[context] = false

		sleepThread = newthread(function()
			wait(5000)

			-- Only delete it if the context hasn't been reactivated since we started waiting
			if (not contextActive[context]) then
				printdb("^cUnloading context ^y'ui:"..context.."'")

				DeleteResourceContext(context)
			else
				printdb("^cNot unloading context ^y'ui:"..context.."' ^ccontext is active")
			end
		end)
	end
end

function ResourceManager.IsContextActive(context)
	if (not context) then
		return false
	end

	return contextActive[context]
end

function ResourceManager.PrecacheAll()
	printdb("^:^rResourceManager.PrecacheAll- Precaching everything!")

	for context,_ in pairs(contextActive) do
		ResourceManager.LoadContext(context)
	end
end

function ResourceManager.GetActiveContexts()
	local activeTable = {}

	for context,active in pairs(contextActive) do
		if (active) then
			table.insert(activeTable, context)
		end
	end

	return activeTable
end
