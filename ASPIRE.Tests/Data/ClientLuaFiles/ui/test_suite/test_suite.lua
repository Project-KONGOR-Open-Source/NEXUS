---- Test Suite
-- Functionality for the Test Suite
--
-- Copyright 2015 Frostburn Studios
-------------------
TestSuite = TestSuite or {}

TestSuite.entitiesLoaded = false
TestSuite.AuditLogText = {}
TestSuite.activeThread = nil

TestSuite.itemCache = nil
TestSuite.entityCache = nil
TestSuite.effectCache = nil

TestSuite.refreshList = {}
TestSuite.refreshListCount = 0
TestSuite.actionList = {}

local widgetMemoize = {}

-- Make sure the replace names cvar exists
if (not Cvar.GetCvar('ui_testSuiteReplaceItemNames')) then
	local replace = Cvar.CreateCvar('ui_testSuiteReplaceItemNames', 'bool', 'true')
	replace:SetSave(true)
end

if (not Cvar.GetCvar('ui_testSuiteReplaceEntityNames')) then
	local replace = Cvar.CreateCvar('ui_testSuiteReplaceEntityNames', 'bool', 'true')
	replace:SetSave(true)
end

-- Private functions
local function NotEmpty(str)
	return (str and (type(str) == 'string') and (str:len() ~= 0))
end

local function Empty(str)
	return (not NotEmpty(str))
end

local function tableDeepCopy(t)
	local newT = {}
	for k,v in pairs(t) do
		newT[k] = v
	end

	return newT
end

local function explode(d,p)
	if (d) and (p) and (type(p) == 'string') then
		  local t, ll
		  t={}
		  ll=0
		  if(#p == 1) then return {p} end
			while true do
			  l=string.find(p,d,ll,true) 
			  if l~=nil then 
				table.insert(t, string.sub(p,ll,l-1))
				ll=l+1 
			  else
				table.insert(t, string.sub(p,ll))
				break 
			  end
			end
		  return t
	else
		Echo('Explode error d: ' .. tostring(d) .. ' in ' .. tostring(p))
	end
end

local function IsLeapYear(year)
	if ((year % 4) == 0) then
		if ((not ((year % 100) == 0)) or ((year % 400) == 0)) then
			return true
		end
	end

	return false
end

local function SecondsInMonth(month, isLeapYear)
	local timeInMonths = {2678400, nil, 2678400, 2592000, 2678400, 2592000, 2678400, 2678400, 2592000, 2678400, 2592000, 2678400}

	if (month ~= 2) then
		return timeInMonths[month]
	else
		if (isLeapYear) then
			return 2505600
		else
			return 2419200
		end
	end
end

local function SetActiveThread(thread)
	TestSuite.activeThread = thread
	return thread
end

local function ReplaceItemNames()
	return Cvar.GetCvar('ui_testSuiteReplaceItemNames'):GetBoolean()
end

local function ReplaceEntityNames()
	return Cvar.GetCvar('ui_testSuiteReplaceEntityNames'):GetBoolean()
end

local function GetUnitDisplayName(entity, appendBaseHero)
	local names = nil
	if (not pcall(function() names = Testing.GetNamesFromDefinition(entity) end)) then
		return ''
	end


	if (not string.find(entity, '.', 0, true)) then
		return names.displayName
	else
		local info = nil
		if (pcall(function() info = Testing.GetProductInfo(entity) end)) then
			local display = info.cName

			if (appendBaseHero) then
				display = display..' ('..names.displayName..')'
			end

			return display
		else
			return names.displayName..' ('..entity:sub(entity:find('.', 0, true) + 1)..')'
		end
	end

	return ''
end

local function TranslateOrEmpty(str, nilOnEmpty)
	local s = Translate(str)
	if (s == str) then
		if (nilOnEmpty) then
			return nil
		else
			return ''
		end
	end

	return s
end

local function BuildEffectCache()
	local effects = Testing.GetFiles('/', '*.effect', true)
	if (table.getn(effects) == 0) then
		return
	end

	TestSuite.effectCache = {}

	for _, effect in ipairs(effects) do
		table.insert(TestSuite.effectCache, effect.filePath)
	end
end

local function BuildItemCache()
	local items = Testing.GetItems()
	if (table.getn(items) == 0) then
		return
	end

	TestSuite.itemCache = {}

	for _, item in ipairs(items) do
		local icon = GetEntityIconPath(item)
		if (Empty(icon)) then
			icon = '$invis'
		end

		table.insert(TestSuite.itemCache, {
			['name'] = item,
			['displayName'] = GetEntityDisplayName(item),
			['icon'] = icon,
		})
	end
end

local function BuildEntityCache()
	local units = Testing.GetUnits()
	if (table.getn(units) == 0) then
		return
	end

	TestSuite.entityCache = {}

	for _, entity in ipairs(units) do
		local icons = nil
		if (pcall(function() icons = Testing.GetIconsFromDefinition(entity) end)) then
			table.insert(TestSuite.entityCache, {
				['name'] = entity,
				['displayName'] = GetUnitDisplayName(entity, true),
				['icon'] = icons.icon2[1] or icons.icon[1] or '$invis',
			})
		end
	end
end

local function printTableDeep(printThatTable)
	if (not printThatTable) then
		Echo('printTableDeep: ' .. tostring(printThatTable) .. ' is not a table')
		return
	end

	local function printTableRecur(table, prefix)
		for key, value in pairs(table) do
			Echo(prefix.."k: "..tostring(key)..' | v: '..tostring(value))

			if (type(value) == "table") then
				printTableRecur(value, prefix.."  ")
			end
		end
	end

	printTableRecur(printThatTable, "")
end

local function SetAutoRefreshList(enabled)
	if (enabled) then
		TestSuite.GetWidget('test_suite_server_advanced_auto_refresh'):SetCallback('onframe', function()
			local rebuildList = false
			for eID,_ in pairs(TestSuite.refreshList) do
				if (Testing.GameEntityExists(eID)) then
					if (Testing.EntityIsHero(eID)) then
						Testing.RespawnHero(eID)
					end

					Testing.Refresh(eID)
					Testing.RefreshLifetime(eID)
				else
					TestSuite.refreshList[eID] = nil
					rebuildList = true
					TestSuite.refreshListCount = TestSuite.refreshListCount - 1
					if (TestSuite.refreshListCount == 0) then
						SetAutoRefreshList(false)
					end
				end
			end

			if (rebuildList) then
				TestSuite.PopulateRefreshList()
			end
		end)
	else
		TestSuite.GetWidget('test_suite_server_advanced_auto_refresh'):ClearCallback('onframe')
	end
end

local function BuildEntityRelationGraph(baseEntity, avatar)
	local recursiveProtection = {}

	local function buildRelations(thisEntity, avatar, parentTable)
		local theseEntities = Testing.GetEntityPrecacheList(thisEntity, avatar)
		local thisEntityChildren = {}

		for _,entity in ipairs(theseEntities) do
			if (entity.name ~= thisEntity) then 	-- Ignore our current entitiy in the precache list
				-- Remove the entity from the parent table, it belongs under thisEntity
				if (parentTable) then
					parentTable[entity.name] = nil
				end

				-- Add the entity to the list of thisEntity's children
				thisEntityChildren[entity.name] = entity
			end
		end

		-- if we have a parent, add this entitiy to the parent with it's children
		if (parentTable) then
			parentTable[thisEntity] = thisEntityChildren
		end

		-- Process each child
		local removeTable = {}	-- Table of recursive entities that should be removed (they would normally be removed by precache guards)

		for entity,entityInfo in pairs(thisEntityChildren) do
			if (not recursiveProtection[entity]) then
				recursiveProtection[entity] = true
				if (NotEmpty(entityInfo.avatarKey)) then
					buildRelations(entity, entityInfo.avatarKey, thisEntityChildren)
				elseif (NotEmpty(entityInfo.modifier)) then
					buildRelations(entity, entityInfo.modifier, thisEntityChildren)
				else
					buildRelations(entity, nil, thisEntityChildren)
				end
				recursiveProtection[entity] = false
			else
				-- Not iterating because of recur protection, but at least clear the entity info out
				table.insert(removeTable, entity)
			end
		end

		for _,entity in ipairs(removeTable) do
			thisEntityChildren[entity] = nil
		end

		return thisEntityChildren
	end

	return buildRelations(baseEntity, avatar, nil)
end

function BuildEntityResourceRelationGraph(baseEntity, avatar)
	-- The entity should not already be loaded in any context before calling this function, if it is
	-- we will not be able to pick up all the resources that an entity uses due to how we load resources
	-- and how we have to build the relation graph

	-- shared resources are those also loaded by an entity's child entities, this is because it could still
	-- be used by that parent resource (a good example is ability icons with states and such)
	-- unique resources are those that are not loaded by an entity's child, this means that that entity
	-- if the final instance of that resource in the entity chain (either indicating that that entity is
	-- what causes the resource to load, or that it's the last entity in the chain that needs the resource
	-- (since an resource isn't uniquely used by that resource if the parent actually does make use of it))

	local recursiveProtection = {}

	local function buildRelations(thisEntity, avatar, parentTable)
		local theseEntities = Testing.GetEntityPrecacheList(thisEntity, avatar)
		local theseResources = Testing.PrecacheAvatarAndGetResources(thisEntity, avatar, 'entity_resource_graph')
		Testing.UnloadContext('entity_resource_graph')

		local thisEntityChildren = {}
		local thisEntityResources = {}

		for _,entity in ipairs(theseEntities) do
			if (entity.name ~= thisEntity) then 	-- Ignore our current entitiy in the precache list
				-- Remove the entity from the parent table, it belongs under thisEntity
				if (parentTable) then
					parentTable[entity.name] = nil
				end

				-- Add the entity to the list of thisEntity's children
				thisEntityChildren[entity.name] = entity
			end
		end

		for _,resource in ipairs(theseResources) do
			-- Remove the resource from the parent's unique table, it is also under thisEntity
			if (parentTable) then
				parentTable.uniqueResources[resource] = nil
				parentTable.sharedResources[resource] = true
			end

			-- Add the entity to the list of thisEntity's children
			thisEntityResources[resource] = true
		end

		thisEntityChildren.uniqueResources = thisEntityResources
		thisEntityChildren.sharedResources = {}

		-- if we have a parent, add this entitiy to the parent with it's children
		if (parentTable) then
			parentTable[thisEntity] = thisEntityChildren
		end

		-- Process each child
		local removeTable = {}	-- Table of recursive entities that should be removed (they would normally be removed by precache guards)

		for entity,entityInfo in pairs(thisEntityChildren) do
			if ((entity ~= 'uniqueResources') and (entity ~= 'sharedResources')) then 	-- ignore the resource tables, they aren't entities
				if (not recursiveProtection[entity]) then
					recursiveProtection[entity] = true
					if (NotEmpty(entityInfo.avatarKey)) then
						buildRelations(entity, entityInfo.avatarKey, thisEntityChildren)
					elseif (NotEmpty(entityInfo.modifier)) then
						buildRelations(entity, entityInfo.modifier, thisEntityChildren)
					else
						buildRelations(entity, nil, thisEntityChildren)
					end
					recursiveProtection[entity] = false
				else
					-- Not iterating because of recur protection, but at least clear the entity info out
					table.insert(removeTable, entity)
				end
			end
		end

		for _,entity in ipairs(removeTable) do
			thisEntityChildren[entity] = nil
		end

		return thisEntityChildren
	end

	local relation = { [baseEntity] = buildRelations(baseEntity, avatar, nil) }

	return relation
end

local function PrintResourceAndEntityChains(resourcePath, entityResourceRelationGraph, prefix)
	-- entityResourceRelationGraph is a table generated by BuildEntityResourceRelationGraph,
	-- we pass it in instead of generating it here so so that it can be reused between prints
	-- as generating the graph isn't exactly cheap

	local completeChains = {}
	local chains = Testing.GetResourceParents(resourcePath)

	for index,chain in ipairs(chains) do
		-- Find entity chains that use the resource and follow them, then at the end of them print the resource chain
		-- Search the resource relation for the uppermost level of the chain
		local function buildEntityChainsToResource(resource, relationTable, chainTable)
			for entity,childTables in pairs(relationTable) do
				if ((entity ~= 'uniqueResources') and (entity ~= 'sharedResources')) then
					if (childTables.uniqueResources[resource]) then 	-- End of the line
						chainTable[entity] = {}
						local currentLink = chainTable[entity]
						for _,res in ipairs(chain) do
							currentLink[res] = {}
							currentLink = currentLink[res]
						end

						--return chainTable
					elseif (childTables.sharedResources[resource]) then 	-- If the above isn't true, this has to be true, otherwise the entity doesn't use the given resource
						chainTable[entity] = {}
						buildEntityChainsToResource(resource, childTables, chainTable[entity])
						--return chainTable
					end
				end
			end

			return chainTable
		end

		table.insert(completeChains, buildEntityChainsToResource(chain[1], entityResourceRelationGraph, {}))
	end

	for _,chainStart in ipairs(completeChains) do
		local function printKeysAsChain(table, indent)
			for k,v in pairs(table) do
				local line = ''
				if (prefix ~= nil) then
					line = tostring(prefix)
				end

				for i=1,indent-1 do
					line = line..'^:^p|^*^; '
				end
				line = line..'^:^p+^*^; '

				TestSuite.AuditLog(line.."^y"..k)
				printKeysAsChain(v, indent + 1)
			end
		end

		printKeysAsChain(chainStart, 1)
	end

	TestSuite.AuditLog('')
end

local function PrintResourceChains(resourcePath, prefix)
	local chains = Testing.GetResourceParents(resourcePath)

	for _,chain in ipairs(chains) do
		for level,res in ipairs(chain) do
			local line = ''
			if (prefix ~= nil) then
				line = tostring(prefix)
			end

			for i=1,indent-1 do
				line = line..'^:^p|^*^; '
			end
			line = line..'^:^p+^*^; '

			TestSuite.AuditLog(line.."^y"..res)
		end
	end

	TestSuite.AuditLog('')
end

-- Public Functions
function TestSuite.GetWidget(name, suppressErrors, interfaceName)
	if (not name) then
		if (not suppressErrors) then
			Echo("TestSuite.GetWidget was not passed a valid name")
		end
		return nil
	end

	if (widgetMemoize[name] and widgetMemoize[name]:IsValid()) then
		return widgetMemoize[name]
	end

	-- I kinda don't like this but at this point- meh
	local validInterfaces = {'test_suite_client', 'test_suite_server', 'test_suite_cinema'}
	local widget = nil

	if (interfaceName) then
		local interface = UIManager.GetInterface(interfaceName)
		if (interface) then
			widget = interface:GetWidget(name)
		end
	else
		for _,interfaceName in ipairs(validInterfaces) do
			local interface = UIManager.GetInterface(interfaceName)
			if (interface) then
				widget = interface:GetWidget(name)
				if (widget ~= nil) then
					break
				end
			end
		end
	end

	widgetMemoize[name] = widget

	if ((not widget) and (not suppressErrors)) then
		Echo('TestSuite.GetWidget cannot find widget '..name)
	end

	return widget
end

function TestSuite.CheckboxEnabled(checkboxName)
	return (TestSuite.GetWidget(checkboxName):GetButtonState() == 1)
end

function TestSuite.ResetTestSuite()
	local cvarResets = {
		"g_disableFogOfWar", "g_camDistanceMin", "g_camDistanceMax"
	}

	for _,cvarName in ipairs(cvarResets) do
		local cvar = Cvar.GetCvar(cvarName)
		if (cvar) then cvar:Reset() end
	end

	TestSuite.SetAutoRefresh(false)
	TestSuite.ClearRefreshList()

	TestSuite.GetWidget('test_suite_server_basic'):SetVisible(0)
	TestSuite.GetWidget('test_suite_server_advanced'):SetVisible(0)
	TestSuite.GetWidget('test_suite_basic_items'):SetVisible(0)
	TestSuite.GetWidget('test_suite_basic_spawner'):SetVisible(0)
end

function TestSuite.RefreshAllKeybinds()
	local validInterfaces = {'test_suite_client', 'test_suite_server', 'test_suite_cinema'}

	for _,interface in ipairs(validInterfaces) do
		local widget = UIManager.GetInterface(interface)
		if (widget) then
			local group = widget:GetGroup('test_suite_keybinds')
			if (group) then
				for _,keybindWidget in ipairs(group) do
					keybindWidget:DoEventN(9)
				end
			end
		end
	end
end

function TestSuite.Unbind(table, action, param)
	local bindButton = GetKeybindButton(table, action, param, 0)

	while (bindButton ~= 'None') do
		Unbind(table, bindButton)
		bindButton = GetKeybindButton(table, action, param, 0)
	end

	TestSuite.RefreshAllKeybinds()
end

function TestSuite.FinalizeUnbind()
	TestSuite.Unbind(TestSuite.currentBindTable, TestSuite.currentBindAction, TestSuite.currentBindParam)
	TestSuite.GetWidget('test_suite_keybind_prompt', true, TestSuite.currentBindWidget:GetInterface():GetName()):SetVisible(0)

	TestSuite.RefreshAllKeybinds()
end

function TestSuite.FinalizeBind(button)
	TestSuite.Unbind(TestSuite.currentBindTable, TestSuite.currentBindAction, TestSuite.currentBindParam)
	BindImpulse(TestSuite.currentBindTable, button, TestSuite.currentBindAction, TestSuite.currentBindParam)
	TestSuite.GetWidget('test_suite_keybind_prompt', true, TestSuite.currentBindWidget:GetInterface():GetName()):SetVisible(0)

	TestSuite.RefreshAllKeybinds()
end

function TestSuite.BeginBind(bindTable, action, param, bindWidget)
	TestSuite.currentBindTable = bindTable
	TestSuite.currentBindAction = action
	TestSuite.currentBindParam = param
	TestSuite.currentBindWidget = bindWidget

	TestSuite.GetWidget('test_suite_keybind_prompt', true, bindWidget:GetInterface():GetName()):SetVisible(1)
end

function TestSuite.ClientAccessible()
	local overrideCvar = Cvar.GetCvar('ui_forceEnableTesting')
	if ((overrideCvar ~= nil) and (overrideCvar:GetBoolean())) then
		return true
	end

	if (not Cvar.GetCvar('releaseStage_stable'):GetBoolean()) then
		return true
	end

	if (Client.IsStaff()) then
		return true
	end

	return false
end

function TestSuite.CinemaAccessible()
	if (ViewingReplay()) then 	-- Never show in replays, no questions asked
		return (not TestSuite.ServerAccessible())
	end

	return false
end

function TestSuite.ServerAccessible()
	if (ViewingReplay()) then 	-- Never show in replays, no questions asked
		return false
	end

	if (not Testing.IsServerLoaded) then
		return false
	end

	local gameInfo = Testing.GetGameInfo()
	local overrideCvar = Cvar.GetCvar('ui_forceEnableTesting')
	if ((overrideCvar ~= nil) and (overrideCvar:GetBoolean())) then
		if (Testing.IsPracticeGame() or Testing.IsLocalGame()) then
			return true
		end
	end

	if ((Testing.IsPracticeGame() or Testing.IsLocalGame()) and ((gameInfo.gameMode ~= 'botmatch') or Cvar.GetCvar('releaseStage_test'):GetBoolean())) then
		return true
	end

	return false
end

function TestSuite.ServerAdvancedAccessible()
	if (ViewingReplay()) then 	-- Never show in replays, no questions asked
		return false
	end

	if (not Testing.IsServerLoaded) then
		return false
	end

	local overrideCvar = Cvar.GetCvar('ui_forceEnableTesting')
	if ((overrideCvar ~= nil) and (overrideCvar:GetBoolean())) then
		return TestSuite.ServerAccessible()
	end

	if (not Cvar.GetCvar('releaseStage_stable'):GetBoolean()) then
		return TestSuite.ServerAccessible()
	end

	if (Client.IsStaff()) then
		return TestSuite.ServerAccessible()
	end

	return false
end

function TestSuite.PopulateEffectList(searchString)
	if (TestSuite.effectCache == nil) then
		BuildEffectCache()
	end

	local listBox = TestSuite.GetWidget('server_effect_list')

	listBox:Clear()
	if (TestSuite.effectCache == nil) then 		-- cache didn't build
		listBox:AddTemplateListItem('test_suite_listitem_simple', '', 'font', 'dyn_10', 'content', Translate('test_server_no_effects'))
		return
	end

	local matchList = {}
	if (Empty(searchString)) then
		matchList = TestSuite.effectCache
	else
		local searchString = string.lower(searchString)
		for _,v in ipairs(TestSuite.effectCache) do
			if (string.find(string.lower(v), searchString, 0, true)) then
				table.insert(matchList, v)
			end
		end
	end

	-- sort the list
	table.sort(matchList, function(a,b) return string.lower(a) < string.lower(b) end)

	for _,v in ipairs(matchList) do
		listBox:AddTemplateListItem('test_suite_listitem_simple_oversized', v, 'font', 'dyn_10', 'content', v)
	end
end

function TestSuite.ToggleCinemaMode()
	if (GetGameInterface() ~= 'test_suite_cinema') then
		UIManager.LoadInterface('/ui/test_suite/test_suite_cinema.interface')
		TestSuite.previousInterface = GetGameInterface()
		SetGameInterface('test_suite_cinema')
	else
		SetGameInterface(TestSuite.previousInterface)
	end
end

function TestSuite.ExitCinemaMode()
	if (GetGameInterface() == 'test_suite_cinema') then
		SetGameInterface(TestSuite.previousInterface)
	end
end

function TestSuite.UpdateModifierDropdown()
	local selectedEntity = Testing.GetSelectedEntity()
	if (selectedEntity == nil) then 	-- Nothing selected
		return
	end

	local comboBox = TestSuite.GetWidget('test_server_modifier')
	local modifiers = Testing.GetAllAvailableEntityModifiers(selectedEntity)

	local selectedItem = comboBox:GetValue()
	comboBox:ClearItems()
	for i,modifier in ipairs(modifiers) do
		comboBox:AddTemplateListItem('test_suite_listitem_simple', modifier.name, 'font', 'dyn_10', 'content', modifier.name)
	end
	comboBox:SetSelectedItemByValue(selectedItem, false)
end

function TestSuite.UpdateModifierList()
	local listBox = TestSuite.GetWidget('test_server_modifiers_list')

	local selectedEntity = Testing.GetSelectedEntity()
	if (selectedEntity == nil) then 	-- Nothing selected
		listBox:ClearItems()
		return
	end

	local modifiers = Testing.GetEntityModifiers(selectedEntity)
	local listItems = listBox:GetListItems()

	-- Remove old items
	for i,value in ipairs(listItems) do
		local found = false
		for k,modifier in ipairs(modifiers) do
			if (modifier.enabled and (modifier.name == value)) then
				found = true
			end
		end

		if (not found) then
			listBox:EraseListItemByValue(value)
		end
	end

	-- Add new items
	for i,modifier in ipairs(modifiers) do
		if (modifier.enabled and (not listBox:HasListItem(modifier.name))) then
			local label = '['
			if (modifier.global) then
				label = label..'^gG^*,'
			else
				label = label..'G,'
			end
			if (modifier.active) then
				label = label..'^gA^*,'
			else
				label = label..'A,'
			end
			if (modifier.persistent) then
				label = label..'^gP^*] '
			else
				label = label..'P] '
			end
			label = label..modifier.name

			listBox:AddTemplateListItem('test_suite_listitem_simple', modifier.name, 'font', 'dyn_10', 'content', label)
		end
	end
end

function TestSuite.UpdateGlobalModifierList()
	local listBox = TestSuite.GetWidget('test_server_global_modifiers_list')
	local modifiers = Testing.GetGlobalModifiers()
	local listItems = listBox:GetListItems()

	-- Remove old items
	for i,value in ipairs(listItems) do
		local found = false
		for k,modifier in ipairs(modifiers) do
			if (modifier.name == value) then
				found = true
			end
		end

		if (not found) then
			listBox:EraseListItemByValue(value)
		end
	end

	-- Add new items
	for i,modifier in ipairs(modifiers) do
		if (not listBox:HasListItem(modifier.name)) then
			listBox:AddTemplateListItem('test_suite_listitem_simple', modifier.name, 'font', 'dyn_10', 'content', modifier.name)
		end
	end
end

function TestSuite.UpdateStateList()
	local listBox = TestSuite.GetWidget('test_server_state_list')
	local level = TestSuite.GetWidget('test_server_state_level')
	local liferemaining = TestSuite.GetWidget('test_server_state_liferemaining')
	local charges = TestSuite.GetWidget('test_server_state_charges')
	local hidden = TestSuite.GetWidget('test_server_state_hidden')

	local selectedEntity = Testing.GetSelectedEntity()
	if (selectedEntity == nil) then 	-- Nothing selected
		listBox:ClearItems()

		level:SetText(Translate('test_server_state_level', 'level', ''))
		liferemaining:SetText(Translate('test_server_state_liferemaining', 'time', ''))
		charges:SetText(Translate('test_server_state_charges', 'charges', ''))
		hidden:SetText(Translate('test_server_state_hidden', 'hidden', ''))
		return
	end

	local states = Testing.GetStates(selectedEntity)
	local listItems = listBox:GetListItems()
	local selectedItem = listBox:GetValue()
	local selectedState = nil

	-- Remove old items
	for i,value in ipairs(listItems) do
		local found = false
		for k,state in ipairs(states) do
			if (state.exists and (state.name == value)) then
				found = true
			end
		end

		if (not found) then
			listBox:EraseListItemByValue(value)
		end
	end

	-- Add new items
	for i,state in ipairs(states) do
		if (state.exists) then
			if (not listBox:HasListItem(state.name)) then
				listBox:AddTemplateListItem('test_suite_listitem_entity', state.name, 'font', 'dyn_10', 'content', state.name, 'icon', state.iconPath)
			end

			if (state.name == selectedItem) then
				selectedState = state
			end
		end
	end

	-- Populate state info
	if (selectedState) then
		local gameInfo = Testing.GetGameInfo()

		level:SetText(Translate('test_server_state_level', 'level', selectedState.level))
		if (selectedState.expireTime == 4294967296) then
			liferemaining:SetText(Translate('test_server_state_liferemaining', 'time', '---'))
		else
			liferemaining:SetText(Translate('test_server_state_liferemaining', 'time', selectedState.expireTime - gameInfo.gameTime))
		end
		charges:SetText(Translate('test_server_state_charges', 'charges', selectedState.charges))
		hidden:SetText(Translate('test_server_state_hidden', 'hidden', tostring(selectedState.hidden)))
	else
		level:SetText(Translate('test_server_state_level', 'level', ''))
		liferemaining:SetText(Translate('test_server_state_liferemaining', 'time', ''))
		charges:SetText(Translate('test_server_state_charges', 'charges', ''))
		hidden:SetText(Translate('test_server_state_hidden', 'hidden', ''))
	end
end

function TestSuite.TrackStates(enabled)
	if (enabled) then
		TestSuite.GetWidget('test_suite_server_advanced_track_states'):SetCallback('onframe', function()
			TestSuite.UpdateStateList()
		end)
	else
		TestSuite.GetWidget('test_suite_server_advanced_track_states'):ClearCallback('onframe')
	end
end


function TestSuite.TrackModifiers(enabled)
	if (enabled) then
		TestSuite.GetWidget('test_suite_server_advanced_track_modifiers'):SetCallback('onframe', function()
			TestSuite.UpdateModifierList()
		end)
	else
		TestSuite.GetWidget('test_suite_server_advanced_track_modifiers'):ClearCallback('onframe')
	end
end

function TestSuite.TrackPossibleModifiers(enabled)
	if (enabled) then
		TestSuite.GetWidget('test_suite_server_advanced_track_possible_modifiers'):SetCallback('onframe', function()
			TestSuite.UpdateModifierDropdown()
		end)
	else
		TestSuite.GetWidget('test_suite_server_advanced_track_possible_modifiers'):ClearCallback('onframe')
	end
end

function TestSuite.TrackGlobalModifiers(enabled)
	if (enabled) then
		TestSuite.GetWidget('test_suite_server_advanced_track_global_modifiers'):SetCallback('onframe', function()
			TestSuite.UpdateGlobalModifierList()
		end)
	else
		TestSuite.GetWidget('test_suite_server_advanced_track_global_modifiers'):ClearCallback('onframe')
	end
end

function TestSuite.PopulateActionList()
	local listBox = TestSuite.GetWidget('test_server_entity_action_list')
	listBox:ClearItems()

	for _,entity in pairs(TestSuite.actionList) do
		local typeName = entity.avatarKey
		if (Empty(typeName)) then
			typeName = entity.typeName
		end

		listBox:AddTemplateListItem('test_suite_listitem_entity', entity.entityID, 'font', 'dyn_10', 'content', '['..entity.entityID..'] '..typeName..' ('..entity.displayName..')', 'icon', entity.icon)
	end
end

function TestSuite.ClearActionList()
	TestSuite.actionList = {}

	TestSuite.PopulateActionList()
end

function TestSuite.AddActionListEntities(entityList)
	for _,eID in ipairs(entityList) do
		if (not TestSuite.actionList[eID]) then
			local infoTable = Testing.GetEntityInfo(eID)
			local defName = infoTable.avatarKey
			if (Empty(defName)) then
				defName = infoTable.typeName
			end

			local icons = Testing.GetIconsFromDefinition(defName)
			local names = Testing.GetNamesFromDefinition(defName)
			
			TestSuite.actionList[eID] = {
				['entityID'] = eID,
				['displayName'] = names.displayName,
				['avatarKey'] = infoTable.avatarKey,
				['typeName'] = infoTable.typeName,
				['icon'] = icons.icon2[1] or icons.icon[1] or '$invis'
			}
		end
	end

	TestSuite.PopulateActionList()
end

function TestSuite.RemoveActionListEntities(entities)
	if (entities == nil) then
		return
	end

	if (type(entities) == 'number') then
		entities = { entities }
	end

	for _,v in ipairs(entities) do
		if (TestSuite.actionList[v]) then
			TestSuite.actionList[v] = nil
		end
	end

	TestSuite.PopulateActionList()
end

function TestSuite.PopulateRefreshList()
	local listBox = TestSuite.GetWidget('test_server_auto_refresh_list')
	listBox:ClearItems()

	for _,entity in pairs(TestSuite.refreshList) do
		local typeName = entity.avatarKey
		if (Empty(typeName)) then
			typeName = entity.typeName
		end

		listBox:AddTemplateListItemWithSort('test_suite_listitem_entity', entity.entityID, entity.entityID, 'font', 'dyn_10', 'content', '['..entity.entityID..'] '..typeName..' ('..entity.displayName..')', 'icon', entity.icon)
	end
end

function TestSuite.ClearRefreshList()
	TestSuite.refreshList = {}
	TestSuite.refreshListCount = 0
	SetAutoRefreshList(false)

	TestSuite.PopulateRefreshList()
end

function TestSuite.AddRefreshListEntities(entityList)
	for _,eID in ipairs(entityList) do
		if (not TestSuite.refreshList[eID]) then
			local infoTable = Testing.GetEntityInfo(eID)
			local defName = infoTable.avatarKey
			if (Empty(defName)) then
				defName = infoTable.typeName
			end

			local icons = Testing.GetIconsFromDefinition(defName)
			local names = Testing.GetNamesFromDefinition(defName)
			
			TestSuite.refreshList[eID] = {
				['entityID'] = eID,
				['displayName'] = names.displayName,
				['avatarKey'] = infoTable.avatarKey,
				['typeName'] = infoTable.typeName,
				['icon'] = icons.icon2[1] or icons.icon[1] or '$invis'
			}

			TestSuite.refreshListCount = TestSuite.refreshListCount + 1
		end
	end

	if (TestSuite.refreshListCount > 0) then
		SetAutoRefreshList(true)
	end

	TestSuite.PopulateRefreshList()
end

function TestSuite.RemoveRefreshListEntities(entities)
	if (entities == nil) then
		return
	end

	if (type(entities) == 'number') then
		entities = { entities }
	end

	for _,v in ipairs(entities) do
		if (TestSuite.refreshList[v]) then
			TestSuite.refreshList[v] = nil
			TestSuite.refreshListCount = TestSuite.refreshListCount - 1
		end
	end

	if (TestSuite.refreshListCount == 0) then
		SetAutoRefreshList(false)
	end

	TestSuite.PopulateRefreshList()
end

function TestSuite.ForceEffectCacheRebuild()
	BuildEffectCache()
end

function TestSuite.ForceEntityCacheRebuild()
	BuildEntityCache()
end

function TestSuite.ForceItemCacheRebuild()
	BuildItemCache()
end

function TestSuite.BasicSpawnerSpawn(posX, posY)
	local team = 0
	if (TestSuite.CheckboxEnabled('test_server_spawn_team1')) then
		team = 1
	elseif (TestSuite.CheckboxEnabled('test_server_spawn_team2')) then
		team = 2
	elseif (TestSuite.CheckboxEnabled('test_server_spawn_team_passive')) then
		team = -1
	elseif (TestSuite.CheckboxEnabled('test_server_spawn_team_neutral')) then
		team = -2
	end

	local owner = -1
	if (not TestSuite.CheckboxEnabled('test_server_spawner_unowned')) then
		if (TestSuite.CheckboxEnabled('test_server_spawner_self_own')) then
			owner = GetLocalClientNumber()
		else
			local targetTeam = Testing.GetLocalPlayerTeamID()
			if (TestSuite.CheckboxEnabled('test_server_spawner_enemy_own')) then
				if (targetTeam == 1) then
					targetTeam = 2
				else
					targetTeam = 1
				end
			end

			-- Find a fake player (on the player's team so they can share control) to assign the unit to
			local teamPlayers = Testing.GetTeamPlayers(targetTeam)
			for _,player in ipairs(teamPlayers) do
				if (player.isConnectionless and (not player.isBot)) then
					owner = player.clientNumber
					break
				end
			end

			if (owner == -1) then 	-- No team memeber found, add one (and if that fails keep an invalid owner number)
				owner = Testing.AddFakePlayer(targetTeam) or -1
			end
		end
	end

	-- Do a precache as that is client side and the server side precaching done in SpawnUnit doesn't cache everything the client needs
	local unitName = TestSuite.GetWidget('client_server_basic_spawner_list'):GetValue()

	if (not TestSuite.CheckboxEnabled('test_server_spawner_alts')) then
		Testing.Precache(unitName)
		local unit = Testing.SpawnUnit(unitName, owner, team, posX, posY)

		if (TestSuite.CheckboxEnabled('test_server_spawner_hold')) then
			Testing.OrderHold(unit)
		end

		if (owner ~= -1) then
			if (not Testing.PlayerHasValidHero(owner)) then
				if (Testing.EntityIsHero(unit)) then
					Testing.SetPlayerHero(owner, unit)
				end
			end
		end
	else
		local baseEntity = unitName
		if (string.find(unitName, '.', 1, true)) then
			baseEntity = string.sub(unitName, 0, string.find(unitName, '.', 1, true) - 1)
		end

		local spawnList = { baseEntity }
		local alts = Testing.GetDefinitionAltAvatars(baseEntity)
		for _,alt in ipairs(alts) do
			if (string.find(alt.fullAvatarName, baseEntity)) then
				table.insert(spawnList, alt.fullAvatarName)
			end
		end

		local perRow = math.floor(math.sqrt(#spawnList))
		local row, col = 0, 0
		local spawnSummary = ''

		for _,spawnUnit in ipairs(spawnList) do
			Testing.Precache(spawnUnit)
			local unit = Testing.SpawnUnit(spawnUnit, owner, team, posX, posY)

			if (not ((row == 0) and (col == 0))) then
				spawnSummary = spawnSummary..'\n'
			end
			spawnSummary = spawnSummary..row..','..col..': '..spawnUnit

			col = col + 1
			if (col > perRow) then
				posY = posY - 125
				posX = posX - (125 * perRow)
				col = 0
				row = row + 1
			else
				posX = posX + 125
			end

			if (TestSuite.CheckboxEnabled('test_server_spawner_hold')) then
				Testing.OrderHold(unit)
			end

			if (owner ~= -1) then
				if (not Testing.PlayerHasValidHero(owner)) then
					if (Testing.EntityIsHero(unit)) then
						Testing.SetPlayerHero(owner, unit)
					end
				end
			end
		end

		Echo(spawnSummary)
	end
end

function TestSuite.PopulateSpawnerList(searchString)
	if (TestSuite.entityCache == nil) then
		BuildEntityCache()
	end

	local listBox = TestSuite.GetWidget('client_server_basic_spawner_list')

	listBox:Clear()
	if (TestSuite.entityCache == nil) then -- cache didn't build
		listBox:AddTemplateListItem('test_suite_listitem_simple', '-1', 'font', 'dyn_10', 'content', Translate('test_server_item_list_empty'))
		return
	end

	local matchList = {}
	if (Empty(searchString)) then
		matchList = TestSuite.entityCache
	else
		local searchString = string.lower(searchString)
		for _,v in ipairs(TestSuite.entityCache) do
			if (string.find(string.lower(v.name), searchString, 0, true)) then
				table.insert(matchList, v)
			elseif (string.find(string.lower(v.displayName), searchString, 0, true)) then
				table.insert(matchList, v)
			end
		end
	end

	-- sort the list
	table.sort(matchList, function(a,b) return string.lower(a.name) < string.lower(b.name) end)

	for _,v in ipairs(matchList) do
		local name = v.name
		if (ReplaceEntityNames() and NotEmpty(v.displayName)) then
			name = v.displayName
		end

		listBox:AddTemplateListItem('test_suite_listitem_entity', v.name, 'font', 'dyn_12', 'content', name, 'icon', v.icon)
	end
end

function TestSuite.PopulateBasicItemList(searchString)
	if (TestSuite.itemCache == nil) then
		BuildItemCache()
	end

	local listBox = TestSuite.GetWidget('client_server_basic_item_list')

	listBox:Clear()
	if (TestSuite.itemCache == nil) then 		-- cache didn't build
		listBox:AddTemplateListItem('test_suite_listitem_simple', '-1', 'font', 'dyn_10', 'content', Translate('test_server_item_list_empty'))
		return
	end

	local matchList = {}
	if (Empty(searchString)) then
		matchList = TestSuite.itemCache
	else
		local searchString = string.lower(searchString)
		for _,v in ipairs(TestSuite.itemCache) do
			if (string.find(string.lower(v.name), searchString, 0, true)) then
				table.insert(matchList, v)
			elseif (string.find(string.lower(v.displayName), searchString, 0, true)) then
				table.insert(matchList, v)
			end
		end
	end

	-- sort the list
	table.sort(matchList, function(a,b) return string.lower(a.name) < string.lower(b.name) end)

	for _,v in ipairs(matchList) do
		local name = v.name
		if (ReplaceItemNames() and NotEmpty(v.displayName)) then
			name = v.displayName
		end

		listBox:AddTemplateListItem('test_suite_listitem_entity', v.name, 'font', 'dyn_12', 'content', name, 'icon', v.icon)
	end
end

function TestSuite.AppendDateTime(name, extension)
	local dateString = string.gsub(Cvar.GetCvar('host_date'):GetString(), "/", "_")
	local timeString = string.gsub(Cvar.GetCvar('host_time'):GetString(), ":", "_")

	return name..'_'..dateString..'_'..timeString..(extension or '')
end

function TestSuite.SetAutoRefresh(enabled)
	if (enabled) then
		TestSuite.GetWidget('test_suite_server_basic_auto_refresh'):SetCallback('onframe', function()
			TestSuite.DoSelectedUnits(function(unit) Testing.Refresh(unit) end)
		end)
	else
		TestSuite.GetWidget('test_suite_server_basic_auto_refresh'):ClearCallback('onframe')
	end
end

function TestSuite.DoSelectedUnitsAdvanced(unitFunction)
	if (unitFunction == nil) then
		return
	end

	-- State 0 = first unit, 1 = all units
	local state = TestSuite.GetWidget('test_server_adv_all_units'):GetButtonState()

	if (state == 0) then
		unitFunction(Testing.GetSelectedEntity())
	else
		local executedOn = {}
		local units = Testing.GetSelectedEntities()

		for k,v in ipairs(units) do
			if (not executedOn[v]) then
				pcall(function() unitFunction(v) end)
				executedOn[v] = true
			end
		end

		for k,v in pairs(TestSuite.actionList) do
			if (not executedOn[k]) then
				pcall(function() unitFunction(k) end)
				executedOn[k] = true
			end
		end
	end
end

function TestSuite.DoSelectedUnits(unitFunction)
	if (unitFunction == nil) then
		return
	end

	-- State 0 = first unit, 1 = all units
	local state = TestSuite.GetWidget('test_server_all_units'):GetButtonState()

	if (state == 0) then
		unitFunction(Testing.GetSelectedEntity())
	else
		local units = Testing.GetSelectedEntities()

		for k,v in ipairs(units) do
			pcall(function() unitFunction(v) end)
		end
	end
end

function TestSuite.PopulateProductList(filterString)
	local productList = Testing.GetProductList()

	local listBox = TestSuite.GetWidget('client_test_product_list')

	listBox:Clear()
	if (productList.count == 0) then -- empty
		listBox:AddTemplateListItem('test_suite_listitem_simple', '-1', 'font', 'dyn_8', 'content', Translate('test_client_product_list_empty'))
		return
	end

	local matchList = {}
	if (Empty(filterString)) then
		matchList = productList
	else
		local filterString = string.lower(filterString)
		for _,v in ipairs(productList) do
			if (string.find(string.lower(v.name), filterString, 0, true)) then
				table.insert(matchList, v)
			elseif (string.find(string.lower(v.cName), filterString, 0, true)) then
				table.insert(matchList, v)
			elseif (string.find(v.productID, filterString, 0, true)) then
				table.insert(matchList, v)
			end
		end
	end

	-- sort the list
	table.sort(matchList, function(a,b) return a.productID > b.productID end)

	for _,v in ipairs(matchList) do
		listBox:AddTemplateListItem('test_suite_listitem_simple', v.productID, 'font', 'dyn_8', 'content', v.cName..' ('..v.name..') - '..v.productID)
	end
end

function TestSuite.SelectProductList(productID)
	local productID = tonumber(productID)

	local product = nil
	if (productID and (productID ~= -1)) then
		product = Testing.GetProductInfo(productID)
	end

	if (not product) then
		TestSuite.GetWidget('test_client_product_list_name'):EraseInputLine()
		TestSuite.GetWidget('test_client_product_list_cname'):EraseInputLine()
		TestSuite.GetWidget('test_client_product_list_path'):EraseInputLine()
		TestSuite.GetWidget('test_client_product_list_type'):EraseInputLine()
		TestSuite.GetWidget('test_client_product_list_type_string'):EraseInputLine()
		TestSuite.GetWidget('test_client_product_list_type_desc'):EraseInputLine()
		TestSuite.GetWidget('test_client_product_list_id'):EraseInputLine()
		TestSuite.GetWidget('test_client_product_list_purchasable'):EraseInputLine()
		TestSuite.GetWidget('test_client_product_list_premium'):EraseInputLine()
		TestSuite.GetWidget('test_client_product_list_cost'):EraseInputLine()
		TestSuite.GetWidget('test_client_product_list_premium_cost'):EraseInputLine()
		TestSuite.GetWidget('test_client_product_list_dynamic'):EraseInputLine()
	else
		TestSuite.GetWidget('test_client_product_list_name'):SetInputLine(product.name)
		TestSuite.GetWidget('test_client_product_list_cname'):SetInputLine(product.cName)
		TestSuite.GetWidget('test_client_product_list_path'):SetInputLine(product.path)
		TestSuite.GetWidget('test_client_product_list_type'):SetInputLine(product.type)
		TestSuite.GetWidget('test_client_product_list_type_string'):SetInputLine(product.typeString)
		TestSuite.GetWidget('test_client_product_list_type_desc'):SetInputLine(Translate('test_client_type_'..product.typeString))
		TestSuite.GetWidget('test_client_product_list_id'):SetInputLine(product.productID)
		TestSuite.GetWidget('test_client_product_list_purchasable'):SetInputLine(tostring(product.purchaseable))
		TestSuite.GetWidget('test_client_product_list_premium'):SetInputLine(tostring(product.premium))
		TestSuite.GetWidget('test_client_product_list_cost'):SetInputLine(product.cost)
		TestSuite.GetWidget('test_client_product_list_premium_cost'):SetInputLine(product.premiumCost)
		TestSuite.GetWidget('test_client_product_list_dynamic'):SetInputLine(tostring(product.dynamic))
	end
end

function TestSuite.UnixTimestampToNumeralTime(timeStamp)
	local secondsRemaining = timeStamp

	local years = 1970
	local months = 1
	local days = 1
	local hours = 0
	local minutes = 0
	local seconds = 0

	-- year
	while ((IsLeapYear(years) and (secondsRemaining >= 31622400)) or
		  ((not IsLeapYear(years)) and (secondsRemaining >= 31536000))) do

		if (IsLeapYear(years)) then
			secondsRemaining = PreciseIntSubtract(secondsRemaining, 31622400);
		else
			secondsRemaining = PreciseIntSubtract(secondsRemaining, 31536000);
		end
		years = years + 1
	end

	-- months
	while (secondsRemaining >= SecondsInMonth(months, IsLeapYear(years))) do
		secondsRemaining = PreciseIntSubtract(secondsRemaining, SecondsInMonth(months, IsLeapYear(years)))

		months = months + 1
	end

	-- days
	days = days + math.floor(secondsRemaining / 86400)
	secondsRemaining = secondsRemaining % 86400

	-- hours
	hours = math.floor(secondsRemaining / 3600)
	secondsRemaining = secondsRemaining % 3600

	-- minutes
	minutes = math.floor(secondsRemaining / 60)
	secondsRemaining = secondsRemaining % 60

	-- seconds
	seconds = secondsRemaining

	return seconds, minutes, hours, days, months, years
end

function TestSuite.OpenStoreCategory(category)
	local store = GetCvarBool('cg_store2_') and 'store_container2' or 'store_container'
	if (UIManager.GetInterface('main'):GetWidget(store):IsVisible()) then
		Cvar.CreateCvar('_microStore_Category', 'int', category)
		Cvar.CreateCvar('_microStore_RequestCode', 'int', 1)
		UIManager.GetInterface('main'):GetWidget('MicroStoreAction'):DoEventN(0)
	else
		Cvar.CreateCvar('microStore_targetCategory', 'int', category)
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', store, nil, forceOpen)
	end
end

function TestSuite.FakePlinko(tier, productName, productID, productIcon, productType, tickets)
	local tier = tonumber(tier)
	if (not tier) then
		tier = math.random(1, 6)
	end

	local tickets = tonumber(tickets)
	if (not tickets) then
		tickets = math.random(25, 1250)
	end

	-- Open plinko
	local sleepTime = 0
	local plinko = UIManager.GetInterface('main'):GetWidget('plinko')
	local ticketRedemption = UIManager.GetInterface('main'):GetWidget('ticket_redemption_section')
	local plinkoSection = UIManager.GetInterface('main'):GetWidget('plinko_section')

	if (not plinko:IsVisible()) then 	-- Need to open plinko
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'plinko', nil, false)
		sleepTime = 1000
	elseif (ticketRedemption:IsVisible()) then	-- Need to close the ticket redemption
		ticketRedemption:SetVisible(false)
		plinkoSection:SetVisible(true)
	end

	ticketRedemption:Sleep(sleepTime, function()
		if (NotEmpty(productName)) then 	-- We are faking a drop for a product
			Trigger('PlinkoDropResult', '1', tier, productID, productName, productIcon, productType, 0, 5000, UITrigger.GetTrigger('GoldCoins'):GetLastValue(), '0')
		else
			Trigger('PlinkoDropResult', '1', tier, '', '', '', 'Ticket', tickets, 5000, UITrigger.GetTrigger('GoldCoins'):GetLastValue(), '0')
		end
	end)
end

function TestSuite.FakeGrabBag(theme, productNames, productIDs, productIcons, productTypes)
	if (Empty(productNames)) then
		return
	end

	local sleep = 0
	local store = GetCvarBool('cg_store2_') and 'store_container2' or 'store_container'
	if (not UIManager.GetInterface('main'):GetWidget(store):IsVisible()) then
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', store, nil, forceOpen)
		sleep = 1000
	end

	UIManager.GetInterface('test_suite_client'):Sleep(sleep, function()
		HoN_Grabbag:GrabBagResults('store_grab_bag_container', theme, productIDs, productNames, productIcons, productTypes)
	end)
end

function TestSuite.LoadEntities(noMessages)
	if (not TestSuite.entitiesLoaded) then
		if (noMessages) then
			LoadEntityDefinitionsFromFolder('/')
			TestSuite.entitiesLoaded = true
		else
			TestSuite.AuditLog('test_client_msg_loading_defs')

			newthread(function()
				wait(1)
				LoadEntityDefinitionsFromFolder('/')
				TestSuite.AuditLog('test_client_msg_loading_defs_complete')
				TestSuite.entitiesLoaded = true
			end)
		end
	elseif (not noMessages) then
		TestSuite.AuditLog('test_client_msg_defs_already_loaded')
	end
end

function TestSuite.ClearAuditLog()
	TestSuite.GetWidget('test_client_audit_log'):ClearBufferText()
	TestSuite.AuditLogText = {}
end

function TestSuite.AuditLog(message, ...)
	if ((table.getn(TestSuite.AuditLogText) == 0) and Empty(message)) then
		return
	end

	local translatedMessage = Translate(message, ...)

	table.insert(TestSuite.AuditLogText, translatedMessage)

	if (NotEmpty(translatedMessage)) then
		translatedMessage = ' ^:-+^; '..translatedMessage
	end

	TestSuite.GetWidget('test_client_audit_log'):AddBufferText(translatedMessage)
	--Echo(translatedMessage)
end

function TestSuite.SaveAuditLog()
	if (table.getn(TestSuite.AuditLogText) == 0) then
		TestSuite.AuditLog('test_client_error_log_empty')
		return
	end

	if (not io) then
		TestSuite.AuditLog('test_client_save_no_io')
		return
	end

	local dateString = string.gsub(Cvar.GetCvar('host_date'):GetString(), "/", "_")
	local timeString = string.gsub(Cvar.GetCvar('host_time'):GetString(), ":", "_")
	local filePath = Testing.GetSystemPath('~/auditlog'..'_'..dateString..'_'..timeString..'.txt')

	local file = io.open(filePath, 'w')
	if (not file) then
		TestSuite.AuditLog('test_client_error_file_io')
		return
	end

	for _,line in ipairs(TestSuite.AuditLogText) do
		-- Strip color codes, only supports ^x format
		local color = line:find('%^')
		while (color) do
			local newLine = line:sub(0, color-1)
			newLine = newLine .. line:sub(color+2)
			line = newLine
			color = line:find('%^')
		end

		file:write(line..'\n')
	end

	file:flush()
	file:close()

	TestSuite.AuditLog('test_client_msg_saved', 'filePath', filePath)
end

function TestSuite.UpdateClientProgress(thinking, title, progress, total)
	if (thinking) then
		TestSuite.GetWidget('test_suite_client_progress_title'):SetText(Translate("test_client_progress", "title", title))
		if ((type(total) == 'number') and (total ~= 0)) then
			TestSuite.GetWidget('test_suite_client_progress_bar'):SetWidth(((progress / total) * 100.0)..'%')
			TestSuite.GetWidget('test_suite_client_progress_percent'):SetText(string.format("%.2f%%", ((progress / total) * 100.0)))
		else
			TestSuite.GetWidget('test_suite_client_progress_bar'):SetWidth('0%')
			TestSuite.GetWidget('test_suite_client_progress_percent'):SetText('0%')
		end
		TestSuite.GetWidget('test_suite_client_progress_count'):SetText(progress..' / '..total)
		TestSuite.GetWidget('test_suite_client_progress_cover'):SetVisible(true)
	else
		TestSuite.GetWidget('test_suite_client_progress_cover'):SetVisible(false)
	end
end

function TestSuite.CancelClientOperation()
	if (TestSuite.activeThread and TestSuite.activeThread:IsValid()) then
		TestSuite.activeThread:Kill()

		TestSuite.AuditLog("^yThe current operation has been canceled...")

		TestSuite.UpdateClientProgress(false)
	end
end

-- Runs all audits
function TestSuite.AuditAll()

	local function waitOnThread(t)
		while (t:IsValid()) do
			wait(1)
		end
	end

	newthread(function()
		waitOnThread(TestSuite.AuditProductList())
		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_audit_all"), 1, 10)
		TestSuite.AuditLog("")
		waitOnThread(TestSuite.AuditEntityExistance())
		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_audit_all"), 2, 10)
		TestSuite.AuditLog("")
		-- waitOnThread(TestSuite.AuditEntityUsage())
		-- TestSuite.UpdateClientProgress(true, Translate("test_client_progress_audit_all"), 3, 11)
		-- TestSuite.AuditLog("")
		waitOnThread(TestSuite.AuditHerosAndAlts())
		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_audit_all"), 3, 10)
		TestSuite.AuditLog("")
		waitOnThread(TestSuite.AuditCouriers())
		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_audit_all"), 4, 10)
		TestSuite.AuditLog("")
		waitOnThread(TestSuite.AuditAccountIcons())
		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_audit_all"), 5, 10)
		TestSuite.AuditLog("")
		waitOnThread(TestSuite.AuditNameColors())
		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_audit_all"), 6, 10)
		TestSuite.AuditLog("")
		waitOnThread(TestSuite.AuditChatSymbols())
		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_audit_all"), 7, 10)
		TestSuite.AuditLog("")
		waitOnThread(TestSuite.AuditAnnouncers())
		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_audit_all"), 8, 10)
		TestSuite.AuditLog("")
		waitOnThread(TestSuite.AuditTaunts())
		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_audit_all"), 9, 10)
		TestSuite.AuditLog("")
		waitOnThread(TestSuite.AuditWards())
	end)
end

-- Audits that all entities that exist are referenced somewhere
function TestSuite.AuditEntityUsage()
	return SetActiveThread(newthread(function()
	TestSuite.UpdateClientProgress(true, Translate("test_client_progress_usage"), 0, "?")
	wait(1)

	TestSuite.LoadEntities(true)

	local tblHeroes = Testing.GetEntities()

	local unitCount = table.getn(tblHeroes)

	local entityInfo = {}
	local parsedEntities = {}

	TestSuite.AuditLog("test_client_msg_audit_usage")

	-- We need to parse raw XML for this
	local function parseXMLForErrors(entityPath, entityName, var)
		local tblXML = nil
		local success, err = pcall(function() tblXML = Testing.ParseXML(entityPath) end)

		if (not success) then
			return
		end

		entityInfo[entityName].type = tblXML[1].NODE_NAME

		-- search for references to any other entities
		-- this is the table of properties to search on the root (hero/pet/etc) node for when looking for related entities
		local relatedRootPropertyNames = {
			'inventory0', 'inventory1', 'inventory2',
			'inventory3', 'inventory4', 'inventory5',
			'inventory6', 'inventory7', 'inventory8',
			'inventory9', 'inventory10', 'inventory11',
			'inventory12', 'attackprojectile', 'sharedinventory0',
			'sharedinventory1', 'sharedinventory2', 'projectile'
		}

		-- this is the table of ['node'] = {'property'} to search for when looking for related entities
		local relatedNodeAndPropertyNames = {
			['applystate'] = {'name'},
			['spawnunit'] = {'name'},
			['spawnprojectile'] = {'name', 'bindstate'},
			['spawnaffector'] = {'name'},
			['spawnlinearaffector'] = {'name'},
			['recommendeditem'] = {'name'},
			['aura'] = {'state', 'gadget'},
			['item'] = {'name'},
			['order'] = {'ordername'},
			['setattackprojectile'] = {'name'},
		}

		local foundEntities = {}

		local function parseNode(node)
			for _,property in pairs(relatedRootPropertyNames) do
				local value = node[property]
				if ((value ~= nil) and (value:len() ~= 0)) then
					local levels = explode(',', value)

					for _,level in ipairs(levels) do
						if (NotEmpty(level)) then
							foundEntities[level] = true
						end
					end
				end
			end

			for nodeName, properties in pairs(relatedNodeAndPropertyNames) do
				if (node.NODE_NAME == nodeName) then
					for _,property in ipairs(properties) do
						if (node[property] and (node[property]:len() ~= 0)) then
							local levels = explode(',', node[property])

							for _,level in ipairs(levels) do
								if (NotEmpty(level)) then
									foundEntities[level] = true
								end
							end
						end
					end
				end
			end

			for _, childNode in ipairs(node) do
				parseNode(childNode)
			end
		end

		parseNode(tblXML)

		for entity,_ in pairs(foundEntities) do
			if (Testing.EntityExists(entity)) then
				if (not entityInfo[entity]) then
					entityInfo[entity] = {['refCount'] = 1}
				else
					entityInfo[entity].refCount = entityInfo[entity].refCount + 1
				end

				if (not parsedEntities[entity]) then
					parsedEntities[entity] = true
					parseXMLForErrors(Testing.GetEntityFilePath(entity), entity)
				end
			end
		end
	end

	for k,hero in ipairs(tblHeroes) do
		if (hero:find('.', 0, true) == nil)	then -- ignore alt avatars, these get parsed as a file, not entity
			local path = Testing.GetEntityFilePath(hero)

			if (not entityInfo[hero]) then
				entityInfo[hero] = {['refCount'] = 0}
			end

			if (not parsedEntities[hero]) then
				parsedEntities[hero] = true
				parseXMLForErrors(path, hero, true)
			end

			TestSuite.UpdateClientProgress(true, Translate("test_client_progress_usage"), k, unitCount)
			wait(1)
		end
	end

	-- Find everything without a reference now
	local parsedCount = 0
	local errorCount = 0
	for entityName, info in pairs(entityInfo) do
		parsedCount = parsedCount + 1

		if (info.refCount == 0) then
			local typeIgnoreList = {
				['hero'] = true,
				['game'] = true,
				['gamelogic'] = true,
				['shop'] = true,
				['building'] = true,
				['neutral'] = true,
				['creep'] = true,
				['critter'] = true,
			}

			if (not typeIgnoreList[info.type]) then
				errorCount = errorCount + 1
				TestSuite.AuditLog("test_client_error_entity_unused", "entityType", info.type, "entityName", entityName, "entityPath", Testing.GetEntityFilePath(entityName))
			end
		end
	end

	TestSuite.AuditLog("test_client_msg_audit_usage_done", "passed", (parsedCount - errorCount), "total", parsedCount)
	if (errorCount == 0) then
		TestSuite.AuditLog("test_client_msg_all_passed")
	end

	TestSuite.UpdateClientProgress(false)
	end))
end

-- Audits that all entities referenced by heroes actually exist
function TestSuite.AuditEntityExistance()
	return SetActiveThread(newthread(function()
	TestSuite.UpdateClientProgress(true, Translate("test_client_progress_exists"), 0, "?")
	wait(1)

	TestSuite.LoadEntities(true)

	local tblHeroes = Testing.GetEntities()

	local unitCount = table.getn(tblHeroes)

	local parsedEntities = {}
	local parsedCount = 0
	local errorCount = 0

	TestSuite.AuditLog("test_client_msg_audit_existance")

	-- We need to parse raw XML for this
	local function parseXMLForErrors(entityPath, entityName)
		parsedCount = parsedCount + 1

		local tblXML = nil
		local success, err = pcall(function() tblXML = Testing.ParseXML(entityPath) end)

		if (not success) then
			TestSuite.AuditLog("test_client_error_xml_parse", "filePath", entityPath)
			errorCount = errorCount + 1
			return
		end

		-- search for references to any other entities
		-- this is the table of properties to search on the root (hero/pet/etc) node for when looking for related entities
		local relatedRootPropertyNames = {
			'inventory0', 'inventory1', 'inventory2',
			'inventory3', 'inventory4', 'inventory5',
			'inventory6', 'inventory7', 'inventory8',
			'inventory9', 'inventory10', 'inventory11',
			'inventory12', 'attackprojectile', 'sharedinventory0',
			'sharedinventory1', 'sharedinventory2', 'projectile'
		}

		-- this is the table of ['node'] = {'property'} to search for when looking for related entities
		local relatedNodeAndPropertyNames = {
			['applystate'] = {'name'},
			['spawnunit'] = {'name'},
			['spawnprojectile'] = {'name', 'bindstate'},
			['spawnaffector'] = {'name'},
			['spawnlinearaffector'] = {'name'},
			['recommendeditem'] = {'name'},
			['aura'] = {'state', 'gadget'},
			['item'] = {'name'},
			['order'] = {'ordername'},
			['setattackprojectile'] = {'name'},
		}

		local foundEntities = {}

		local function parseNode(node)
			for _,property in pairs(relatedRootPropertyNames) do
				local value = node[property]
				if ((value ~= nil) and (value:len() ~= 0)) then
					local levels = explode(',', value)

					for _,level in ipairs(levels) do
						if (NotEmpty(level)) then
							foundEntities[level] = true
						end
					end
				end
			end

			for nodeName, properties in pairs(relatedNodeAndPropertyNames) do
				if (node.NODE_NAME == nodeName) then
					for _,property in ipairs(properties) do
						if (node[property] and (node[property]:len() ~= 0)) then
							local levels = explode(',', node[property])

							for _,level in ipairs(levels) do
								if (NotEmpty(level)) then
									foundEntities[level] = true
								end
							end
						end
					end
				end
			end

			for _, childNode in ipairs(node) do
				parseNode(childNode)
			end
		end

		parseNode(tblXML)

		for entity,_ in pairs(foundEntities) do
			if (not Testing.EntityExists(entity)) then
				parsedEntities[entity] = true
				TestSuite.AuditLog("test_client_error_entity_not_found", "entityPath", entityPath, "otherEntity", entity)
				errorCount = errorCount + 1
			else
				if (not parsedEntities[entity]) then
					parsedEntities[entity] = true
					parseXMLForErrors(Testing.GetEntityFilePath(entity), entity)
				end
			end
		end
	end

	for k,hero in ipairs(tblHeroes) do
		if (hero:find('.', 0, true) == nil)	then -- ignore alt avatars, these get parsed as a file, not entity
			local path = Testing.GetEntityFilePath(hero)
			if (not parsedEntities[hero]) then
				parsedEntities[hero] = true
				parseXMLForErrors(path, hero)
			end

			TestSuite.UpdateClientProgress(true, Translate("test_client_progress_exists"), k, unitCount)
			wait(1)
		end
	end

	TestSuite.AuditLog("test_client_msg_audit_exist_done", "passed", (parsedCount - errorCount), "total", parsedCount)
	if (errorCount == 0) then
		TestSuite.AuditLog("test_client_msg_all_passed")
	end

	TestSuite.UpdateClientProgress(false)
	end))
end


-- Audits all textures/models/sounds/etc precached by each hero/alt avatar
function TestSuite.AuditEntitiyResources(unitType)
	return SetActiveThread(newthread(function()
	TestSuite.UpdateClientProgress(true, Translate("test_client_progress_res"), 0, "?")
	wait(1)

	TestSuite.LoadEntities(true)

	local tblUnits = Testing.GetUnits(unitType)
	local entityCount = table.getn(tblUnits)

	TestSuite.AuditLog("test_client_msg_audit_res", "count", entityCount)

	local errors = 0
	for k,entity in ipairs(tblUnits) do
		if (not TestSuite.AuditResource(entity, true, true)) then
			errors = errors + 1
		end
		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_res"), k, entityCount)
		wait(1)
	end

	TestSuite.AuditLog("test_client_msg_audit_res_done", "passed", (entityCount - errors), "total", entityCount)
	if (errors == 0) then
		TestSuite.AuditLog("test_client_msg_all_passed")
	end

	TestSuite.UpdateClientProgress(false)
	end))
end

-- Like the above function but for a single resource
function TestSuite.AuditResource(entityName, unload, massTestMode)
	local function body()
	if (not massTestMode) then
		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_res"), 0, "?")
		wait(1)
	end
	if (not Testing.EntityExists(entityName)) then
		TestSuite.AuditLog("test_client_error_entity_doesnt_exist", "entityName", entityName)
		TestSuite.UpdateClientProgress(false)
		return false
	end

	local context = nil
	if (unload) then
		context = 'AuditResource'
		Testing.UnloadContext(context)
	end

	-- This is called before we load the entity since the entity must be unloaded in order to
	-- properly build the entity/resource relation graph
	local entityResourceRelationGraph = nil
	if (TestSuite.CheckboxEnabled('audit_resources_display_chains')) then
		local baseEntity = entityName
		local avatar = nil
		if (string.find(entityName, '.', 1, true)) then
			avatar = entityName
			baseEntity = string.sub(entityName, 0, string.find(entityName, '.', 1, true) - 1)
		end

		entityResourceRelationGraph = BuildEntityResourceRelationGraph(baseEntity, avatar)
	end

	local tblResources = Testing.PrecacheAndGetResources(entityName, context)
	local resourceCount = table.getn(tblResources)
	if (not massTestMode) then
		TestSuite.AuditLog("test_client_msg_audit_entity_res", "resCount", resourceCount, "entity", entityName)
	end

	-- Layed out as keys so it's easier to lookup
	local knownSystemTextures = {
		['$white'] = true,
		['$black'] = true,
		['$green'] = true,
		['$invis'] = true,
		['$glow'] = true,
		['$checker'] = true,
		['$yellow_checker'] = true,
		['$red_checker'] = true,
		['$smooth_checker'] = true,
		['$dull_checker'] = true,
		['$red_smooth_checker'] = true,
		['$yellow_smooth_checker'] = true,
		['$blue_smooth_checker'] = true,
		['$green_smooth_checker'] = true,
		['$invis'] = true,
		['$flat'] = true,
		['$flat_dull'] = true,
		['$flat_matte'] = true,
		['$tile_norm'] = true,
		['$pyrmid_norm'] = true,
		['$noise'] = true,
		['$mono_noise'] = true,
		['$spectrum'] = true,
		['$alphagrad'] = true,
		['$size_hint'] = true,
		['$fogofwar'] = true,
		['$fogofwar0'] = true,
		['$fogofwar1'] = true,
		['$fogofwar_rt'] = true,
		['$fogofwar0_dt'] = true,
		['$fogofwar1_dt'] = true,
		['$velocity'] = true,
		['$postbuffer0'] = true,
		['$postbuffer1'] = true,
		['$reflection'] = true,
		['$scene'] = true,
		['$shadowmap'] = true,
		['$shadowmap_color'] = true,
		['$waterdistortion'] = true,
		['$waterflowmap'] = true,
		['$modelpanelpostbuffer0'] = true,
		['$modelpanelpostbuffer1'] = true,
		['$modelpanelscene'] = true,

		['!velocity'] = true,
		['!post_color'] = true,
		['!foliage_d'] = true,
		['!foliage_v'] = true,
		['!terrain_d'] = true,
		['!terrain_n'] = true,
		['!terrain_d2'] = true,
		['!terrain_n2'] = true,
		['!skin'] = true,
		['!sky'] = true,
		['!minimap_texture'] = true,
	}

	local errors = 0
	for k,resourcePath in ipairs(tblResources) do
		-- Special case for resources with a '%', if these didn't fail to load then there will be numbered versions later in the resource table
		-- However if they did fail then it means the files don't exist at all
		if (resourcePath:find('%', 0, true) ~= nil) then
			if (not Testing.ResourceSuccessfullyLoaded(resourcePath)) then
				TestSuite.AuditLog("test_client_error_sample_res_not_found", "entityName", entityName, "resource", resourcePath)
				if (TestSuite.CheckboxEnabled('audit_resources_display_chains')) then
					PrintResourceAndEntityChains(resourcePath, entityResourceRelationGraph)
				end
				errors = errors + 1
			end
		else
			if ((resourcePath:sub(0, 1) == '$') or (resourcePath:sub(0, 1) == '!')) then
				if (not knownSystemTextures[resourcePath]) then
					TestSuite.AuditLog("test_client_error_sys_res_not_found", "entityName", entityName, "resource", resourcePath)
					if (TestSuite.CheckboxEnabled('audit_resources_display_chains')) then
						PrintResourceAndEntityChains(resourcePath, entityResourceRelationGraph)
					end
					errors = errors + 1
				end
			elseif (not Testing.FileExists(resourcePath)) then
				-- Special cases
				local resType = Testing.GetResourceType(resourcePath)
				if (resType.typeName == '{sample}') then
					-- Try the compression format file type (NOTE: This assumes the file type is 3 letters)
					local newResourcePath = resourcePath:sub(0, -4)..(Cvar.GetCvar('sound_compressedFormat'):GetString())
					if (not Testing.FileExists(newResourcePath)) then
						TestSuite.AuditLog("test_client_error_sample_res_not_found", "entityName", entityName, "resource", resourcePath)
						if (TestSuite.CheckboxEnabled('audit_resources_display_chains')) then
							PrintResourceAndEntityChains(resourcePath, entityResourceRelationGraph)
						end
						errors = errors + 1
					end
				elseif (resType.typeName == '{texture}') then
					-- Volume maps are flipbooks, only bother checking for 0000.tga, that's the only texture needed for the texture to be 'valid'
					local textureType = Testing.GetTextureType(resourcePath)
					if (textureType == 1) then	-- NOTE: Like we sounds, this is currently assuming the file type is 3 letters
						local newResourcePath = resourcePath:sub(0, -5)..'0000'..resourcePath:sub(-4)
						if (not Testing.FileExists(newResourcePath)) then
							TestSuite.AuditLog("test_client_error_shadow_res_not_found", "entityName", entityName, "resource", resourcePath, "subResource", newResourcePath)
							if (TestSuite.CheckboxEnabled('audit_resources_display_chains')) then
								PrintResourceAndEntityChains(resourcePath, entityResourceRelationGraph)
							end
							errors = errors + 1
						end
					elseif (textureType == 2) then	-- Cube map
						local broken = false		-- NOTE: More assumed file type
						local cubeMapFiles = {"_posx", "_negx", "_posy", "_negy", "_posz", "_negz"}
						for _,file in ipairs(cubeMapFiles) do
							local newResourcePath = resourcePath:sub(0, -5)..file..resourcePath:sub(-4)
							if (not Testing.FileExists(newResourcePath)) then
								TestSuite.AuditLog("test_client_error_cube_res_not_found", "entityName", entityName, "resource", resourcePath, "subResource", newResourcePath)
								broken = true
							end
						end
						if (broken) then
							if (TestSuite.CheckboxEnabled('audit_resources_display_chains')) then
								PrintResourceAndEntityChains(resourcePath, entityResourceRelationGraph)
							end
							errors = errors + 1
						end
					else
						TestSuite.AuditLog("test_client_error_res_not_found", "entityName", entityName, "resource", resourcePath)
						if (TestSuite.CheckboxEnabled('audit_resources_display_chains')) then
							PrintResourceAndEntityChains(resourcePath, entityResourceRelationGraph)
						end
						errors = errors + 1
					end
				else
					TestSuite.AuditLog("test_client_error_res_not_found", "entityName", entityName, "resource", resourcePath)
					if (TestSuite.CheckboxEnabled('audit_resources_display_chains')) then
						PrintResourceAndEntityChains(resourcePath, entityResourceRelationGraph)
					end
					errors = errors + 1
				end
			elseif (not Testing.ResourceSuccessfullyLoaded(resourcePath)) then	-- The file exists, but it failed to load
				TestSuite.AuditLog("test_client_error_res_failed", "entityName", entityName, "resource", resourcePath)
				if (TestSuite.CheckboxEnabled('audit_resources_display_chains')) then
					PrintResourceAndEntityChains(resourcePath, entityResourceRelationGraph)
				end
				errors = errors + 1
			end
		end

		if (not massTestMode) then
			TestSuite.UpdateClientProgress(true, Translate("test_client_progress_res"), k, resourceCount)
			wait(1)
		end
	end

	if (resourceCount > 0) then
		if (not massTestMode) then
			TestSuite.AuditLog("test_client_msg_audit_entity_res_done", "passed", (resourceCount - errors), "total", resourceCount)
			if (errors == 0) then
				TestSuite.AuditLog("test_client_msg_all_passed")
			end
		end
	else
		TestSuite.AuditLog("test_client_msg_audit_entity_res_none")
	end

	if (unload) then
		Testing.UnloadContext('AuditResource')
	end

	if (not massTestMode) then
		TestSuite.UpdateClientProgress(false)
	end

	return (errors == 0)
	end

	if (not massTestMode) then
		return SetActiveThread(newthread(body))
	else
		return body()
	end
end

-- Audits that all the products in the product list from web have valid matching client-side info
-- NOTE: It would be awesome to test file paths, but that info isn't in the product list
function TestSuite.AuditProductList()
	return SetActiveThread(newthread(function()
	TestSuite.UpdateClientProgress(true, Translate("test_client_progress_list"), 0, "?")
	wait(1)

	local tblProductList = Testing.GetProductList()
	TestSuite.AuditLog("test_client_msg_audit_product_list", "products", tblProductList.count)

	if (tblProductList.count == 0) then
		TestSuite.AuditLog("test_client_error_empty_product_list")
		TestSuite.UpdateClientProgress(false)

		return
	end

	-- load entities for checking alt avatars
	TestSuite.LoadEntities(true)

	-- Setup lists of all the local data we need to check against
	local function convertToNameTable(inputTable)
		local nameTable = {}
		for _,product in ipairs(inputTable) do
			nameTable[product.name] = true
		end

		return nameTable
	end

	local tblAccountIcons = convertToNameTable(Testing.GetAccountIcons())
	local tblNameColors = convertToNameTable(Testing.GetChatNameColors())
	local tblChatSymbols = convertToNameTable(Testing.GetChatSymbols())
	local tblAnnouncers = convertToNameTable(Testing.GetAnnouncers())
	local tblTaunts = convertToNameTable(Testing.GetTaunts())
	local tblWards = convertToNameTable(Testing.GetWards())
	local tblCouriers = convertToNameTable(Testing.GetCouriers())

	local errors = 0
	for k,product in ipairs(tblProductList) do
		if (product.typeString == 'cs')	then		-- Chat symbol
			-- NOTE: This is not currently in the product list, there is nothing to really test here
			if (not tblChatSymbols[product.name]) then
				TestSuite.AuditLog("test_client_error_cs_doesnt_exist", "name", product.name)
				errors = errors + 1
			end
		elseif (product.typeString == 'cc') then	-- Name color
			-- NOTE: This is not currently in the product list, there is nothing to really test here
			if (not tblNameColors[product.name]) then
				TestSuite.AuditLog("test_client_error_cc_doesnt_exist", "name", product.name)
				errors = errors + 1
			end
		elseif (product.typeString == 'ai') then	-- Account Icon
			-- NOTE: This is not currently in the product list, there is nothing to really test here
			if (not tblAccountIcons[product.name]) then
				TestSuite.AuditLog("test_client_error_ai_doesnt_exist", "name", product.name)
				errors = errors + 1
			end
		elseif (product.typeString == 'av') then	-- Announcer
			if (not tblAnnouncers[product.name]) then
				TestSuite.AuditLog("test_client_error_av_doesnt_exist", "name", product.name)
				errors = errors + 1
			end
		elseif (product.typeString == 't') then	-- Taunt
			if (not tblTaunts[product.name]) then
				TestSuite.AuditLog("test_client_error_t_doesnt_exist", "name", product.name)
				errors = errors + 1
			end
		elseif (product.typeString == 'w') then	-- Ward
			if (not tblWards[product.name]) then
				TestSuite.AuditLog("test_client_error_w_doesnt_exist", "name", product.name)
				errors = errors + 1
			end
		elseif (product.typeString == 'aa') then	-- Alt Avatar
			if (not Testing.EntityExists(product.name)) then
				TestSuite.AuditLog("test_client_error_aa_doesnt_exist", "name", product.name)
				errors = errors + 1
			end
		elseif (product.typeString == 'h') then	-- Hero
			local heroName = string.sub(product.name, 0, -6)

			if ((heroName ~= 'AllHeroes') and (not Testing.EntityExists(heroName))) then
				TestSuite.AuditLog("test_client_error_h_doesnt_exist", "name", product.name)
				errors = errors + 1
			end
		elseif (product.typeString == 'eap') then	-- EAP
			local name = product.name
			if (string.find(name, '.eap', -4)) then
				name = string.sub(product.name, 0, -5)
			end

			if (not Testing.EntityExists(name)) then
				TestSuite.AuditLog("test_client_error_eap_doesnt_exist", "name", product.name)
				errors = errors + 1
			end
		elseif (product.typeString == 'c') then	-- Courier
			if (not tblCouriers[product.name]) then
				TestSuite.AuditLog("test_client_error_c_doesnt_exist", "name", product.name)
				errors = errors + 1
			end
		elseif (product.typeString == 'misc') then	-- Misc
			-- Do Nothing, these always pass
		elseif (product.typeString == 'en') then	-- Enhancement
			-- Do nothing, these always pass
		end

		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_list"), k, tblProductList.count)
		wait(1)
	end

	TestSuite.AuditLog("test_client_msg_audit_products_done", "passed", (tblProductList.count - errors), "total", tblProductList.count)
	if (errors == 0) then
		TestSuite.AuditLog("test_client_msg_all_passed")
	end
	TestSuite.UpdateClientProgress(false)

	end))
end

-- Audits that all heros and alt avatars in hero entities have existing models, icons, etc. NOTE: This doens't compare against the product list or base.upgrades
function TestSuite.AuditHerosAndAlts()
	return SetActiveThread(newthread(function()
	TestSuite.UpdateClientProgress(true, Translate("test_client_progress_heroes"), 0, "?")
	wait(1)

	TestSuite.LoadEntities(true)
	local tblHeroes = Testing.GetUnits(1)

	local entityCount = table.getn(tblHeroes)
	TestSuite.AuditLog("test_client_msg_audit_heroes_alts", "count", entityCount)

	local errors = 0

	for k,entityName in ipairs(tblHeroes) do
		local broken = false

		local modelPaths = Testing.GetModelsFromDefinition(entityName)
		local iconPaths = Testing.GetIconsFromDefinition(entityName)
		local effectPaths = Testing.GetEffectsFromDefinition(entityName)

		for _,icon in ipairs(iconPaths.icon) do
			if (not Testing.FileExists(icon)) then
				TestSuite.AuditLog("test_client_error_entity_no_icon", "name", entityName, "icon", icon)
				broken = true
			end
		end

		if (Testing.DefinitionIsAltAvatar(entityName)) then
			for _,icon in ipairs(iconPaths.icon2) do
				if (not Testing.FileExists(icon)) then
					TestSuite.AuditLog("test_client_error_entity_no_icon2", "name", entityName, "icon", icon)
					broken = true
				end
			end
		end

		for _,icon in ipairs(iconPaths.portrait) do
			if (not Testing.FileExists(icon)) then
				TestSuite.AuditLog("test_client_error_entity_no_portrait", "name", entityName, "icon", icon)
				broken = true
			end
		end

		if (not Testing.FileExists(modelPaths.previewModel)) then
			TestSuite.AuditLog("test_client_error_entity_no_preview", "name", entityName, "model", modelPaths.previewModel)
			broken = true
		end

		if (not Testing.FileExists(modelPaths.storeModel)) then
			TestSuite.AuditLog("test_client_error_entity_no_store", "name", entityName, "model", modelPaths.storeModel)
			broken = true
		end

		for _,model in ipairs(modelPaths.model) do
			if (not Testing.FileExists(model)) then
				TestSuite.AuditLog("test_client_error_entity_no_model", "name", entityName, "model", model)
				broken = true
			end
		end

		for _,effect in ipairs(effectPaths.passiveEffect) do
			if (not Testing.FileExists(effect)) then
				TestSuite.AuditLog("test_client_error_entity_no_passive", "name", entityName, "effect", effect)
				broken = true
			end
		end

		if (NotEmpty(effectPaths.previewPassiveEffect) and (not Testing.FileExists(effectPaths.previewPassiveEffect))) then
			TestSuite.AuditLog("test_client_error_entity_no_preview_effect", "name", entityName, "effect", effectPaths.previewPassiveEffect)
			broken = true
		end

		if (NotEmpty(effectPaths.storePassiveEffect) and (not Testing.FileExists(effectPaths.storePassiveEffect))) then
			TestSuite.AuditLog("test_client_error_entity_no_store_effect", "name", entityName, "effect", effectPaths.storePassiveEffect)
			broken = true
		end

		if (broken) then
			errors = errors + 1
		end

		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_heroes"), k, entityCount)
		wait(1)
	end

	TestSuite.AuditLog("test_client_msg_audit_res_done", "passed", (entityCount - errors), "total", entityCount)
	if (errors == 0) then
		TestSuite.AuditLog("test_client_msg_all_passed")
	end

	TestSuite.UpdateClientProgress(false)
	end))
end

-- Audits that all account icons in the base.upgrades has valid files
function TestSuite.AuditAccountIcons()
	return SetActiveThread(newthread(function()
	TestSuite.UpdateClientProgress(true, Translate("test_client_progress_ai"), 0, "?")
	wait(1)

	local tblAccountIcons = Testing.GetAccountIcons()
	TestSuite.AuditLog("test_client_msg_audit_ai", "count", tblAccountIcons.count)

	local errors = 0
	for k, icon in ipairs(tblAccountIcons) do
		if (not Testing.FileExists(icon.texturePath)) then
			TestSuite.AuditLog("test_client_error_ai_missing_file", "name", icon.name, "file", icon.texturePath)
			errors = errors + 1
		end

		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_ai"), k, tblAccountIcons.count)
		wait(1)
	end

	TestSuite.AuditLog("test_client_msg_audit_ai_done", "passed", (tblAccountIcons.count - errors), "total", tblAccountIcons.count)
	if (errors == 0) then
		TestSuite.AuditLog("test_client_msg_all_passed")
	end

	TestSuite.UpdateClientProgress(false)
	end))
end

-- Audits that all name colors in the base.upgrades has valid files/options
function TestSuite.AuditNameColors()
	return SetActiveThread(newthread(function()
	TestSuite.UpdateClientProgress(true, Translate("test_client_progress_cc"), 0, "?")
	wait(1)

	local tblNameColors = Testing.GetChatNameColors()
	TestSuite.AuditLog("test_client_msg_audit_cc", "count", tblNameColors.count)

	local errors = 0
	for k, nameColor in ipairs(tblNameColors) do
		local broken = false

		if (not Testing.FileExists(nameColor.texturePath)) then
			TestSuite.AuditLog("test_client_error_cc_missing_file", "name", nameColor.name, "file", nameColor.texturePath)
			broken = true
		end

		if (Empty(nameColor.color)) then
			TestSuite.AuditLog("test_client_error_cc_no_color", "name", nameColor.name)
			broken = true
		end

		if (Empty(nameColor.ingameColor)) then
			TestSuite.AuditLog("test_client_error_cc_no_ingame_color", "name", nameColor.name)
			broken = true
		end

		if (Empty(nameColor.glowColor)) then
			TestSuite.AuditLog("test_client_error_cc_no_glow_color", "name", nameColor.name)
			broken = true
		end

		if (Empty(nameColor.ingameGlowColor)) then
			TestSuite.AuditLog("test_client_error_cc_no_ingame_glow_color", "name", nameColor.name)
			broken = true
		end

		if (nameColor.sortIndex == 0) then
			TestSuite.AuditLog("test_client_error_cc_no_sort_index", "name", nameColor.name)
			broken = true
		end

		if (broken) then
			errors = errors + 1
		end

		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_cc"), k, tblNameColors.count)
		wait(1)
	end

	TestSuite.AuditLog("test_client_msg_audit_cc_done", "passed", (tblNameColors.count - errors), "total", tblNameColors.count)
	if (errors == 0) then
		TestSuite.AuditLog("test_client_msg_all_passed")
	end

	TestSuite.UpdateClientProgress(false)
	end))
end

-- Audits that all chat symbols in the base.upgrades has valid files
function TestSuite.AuditChatSymbols()
	return SetActiveThread(newthread(function()
	TestSuite.UpdateClientProgress(true, Translate("test_client_progress_cs"), 0, "?")
	wait(1)

	local tblSymbols = Testing.GetChatSymbols()
	TestSuite.AuditLog("test_client_msg_audit_cs", "count", tblSymbols.count)

	local errors = 0
	for k, symbol in ipairs(tblSymbols) do
		if (not Testing.FileExists(symbol.texturePath)) then
			TestSuite.AuditLog("test_client_error_cs_missing_file", "name", symbol.name, "file", symbol.texturePath)
			errors = errors + 1
		end

		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_cs"), k, tblSymbols.count)
		wait(1)
	end

	TestSuite.AuditLog("test_client_msg_audit_cs_done", "passed", (tblSymbols.count - errors), "total", tblSymbols.count)
	if (errors == 0) then
		TestSuite.AuditLog("test_client_msg_all_passed")
	end

	TestSuite.UpdateClientProgress(false)
	end))
end

-- Audits that all taunts in the base.upgrades has matching avatars in the taunt ability
function TestSuite.AuditTaunts()
	return SetActiveThread(newthread(function()
	TestSuite.UpdateClientProgress(true, Translate("test_client_progress_taunts"), 0, "?")
	wait(1)

	-- Get the entity info
	LoadEntityDefinition("/heroes/ability_taunt.entity")
	local tblTauntAlts = Testing.GetDefinitionAltAvatars('Ability_Taunt')
	-- build a table with the alts as the keys for easy searching
	local tauntAvatarKeys = {}
	for _,avatar in ipairs(tblTauntAlts) do
		tauntAvatarKeys[avatar.avatarName] = true
	end

	local tblTaunts = Testing.GetTaunts()
	TestSuite.AuditLog("test_client_msg_audit_t", "count", tblTaunts.count)

	local errors = 0
	for k, taunt in ipairs(tblTaunts) do
		if (not tauntAvatarKeys[taunt.modifier]) then
			TestSuite.AuditLog("test_client_error_t_no_alt", "name", taunt.name, "alt", taunt.modifier)
			errors = errors + 1
		end

		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_taunts"), k, tblTaunts.count)
		wait(1)
	end

	TestSuite.AuditLog("test_client_msg_audit_t_done", "passed", (tblTaunts.count - errors), "total", tblTaunts.count)
	if (errors == 0) then
		TestSuite.AuditLog("test_client_msg_all_passed")
	end

	TestSuite.UpdateClientProgress(false)
	end))
end

-- Audits that all wards in the base.upgrades has matching avatars in the two ward gadgets as well as valid model/effect paths
function TestSuite.AuditWards()
	return SetActiveThread(newthread(function()
	TestSuite.UpdateClientProgress(true, Translate("test_client_progress_wards"), 0, "?")
	wait(1)

	-- Get the entity info
	LoadEntityDefinitionsFromFolder("/items/basic/flaming_eye/")
	LoadEntityDefinitionsFromFolder("/items/basic/mana_eye/")
	local tblFlamingEyeAlts = Testing.GetDefinitionAltAvatars('Gadget_FlamingEye')
	local tblManaEyeAlts = Testing.GetDefinitionAltAvatars('Gadget_Item_ManaEye')
	-- build a table with the alts as the keys for easy searching
	local wardFlamingEyeAvatarKeys = {}
	for key,avatar in ipairs(tblFlamingEyeAlts) do
		wardFlamingEyeAvatarKeys[avatar.avatarName] = key
	end
	local wardManaEyeAvatarKeys = {}
	for key,avatar in ipairs(tblManaEyeAlts) do
		wardManaEyeAvatarKeys[avatar.avatarName] = key
	end

	local tblWards = Testing.GetWards()
	TestSuite.AuditLog("test_client_msg_audit_w", "count", tblWards.count)

	local errors = 0
	for k, ward in ipairs(tblWards) do
		local broken = false
		local flamingEyeKey = wardFlamingEyeAvatarKeys[ward.modifier]
		if (not flamingEyeKey) then
			TestSuite.AuditLog("test_client_error_w_flaming_alt", "name", ward.name, "alt", ward.modifier)
			broken = true
		end

		if (not broken) then
			local modelPaths = Testing.GetModelsFromDefinition(tblFlamingEyeAlts[flamingEyeKey].fullAvatarName)
			local effectPaths = Testing.GetEffectsFromDefinition(tblFlamingEyeAlts[flamingEyeKey].fullAvatarName)

			if (not Testing.FileExists(modelPaths.previewModel)) then
				TestSuite.AuditLog("test_client_error_w_flaming_preview", "name", ward.name, "model", modelPaths.previewModel)
				broken = true
			end

			for _,model in ipairs(modelPaths.model) do
				if (not Testing.FileExists(model)) then
					TestSuite.AuditLog("test_client_error_w_flaming_model", "name", ward.name, "model", model)
					broken = true
				end
			end

			for _,effect in ipairs(effectPaths.passiveEffect) do
				if (not Testing.FileExists(effect)) then
					TestSuite.AuditLog("test_client_error_w_flaming_effect", "name", ward.name, "effect", effect)
					broken = true
				end
			end
		end

		local broken2 = false	-- Use another variable simply so we can still check effects and such on it if it exists
		local manaEyeKey = wardManaEyeAvatarKeys[ward.modifier]
		if (not manaEyeKey) then
			TestSuite.AuditLog("test_client_error_w_mana_alt", "name", ward.name, "alt", ward.modifier)
			broken2 = true
		end

		if (not broken2) then
			local modelPaths = Testing.GetModelsFromDefinition(tblManaEyeAlts[manaEyeKey].fullAvatarName)
			local effectPaths = Testing.GetEffectsFromDefinition(tblManaEyeAlts[manaEyeKey].fullAvatarName)

			if (not Testing.FileExists(modelPaths.previewModel)) then
				TestSuite.AuditLog("test_client_error_w_mana_preview", "name", ward.name, "model", modelPaths.previewModel)
				broken2 = true
			end

			for _,model in ipairs(modelPaths.model) do
				if (not Testing.FileExists(model)) then
					TestSuite.AuditLog("test_client_error_w_mana_model", "name", ward.name, "model", model)
					broken2 = true
				end
			end

			for _,effect in ipairs(effectPaths.passiveEffect) do
				if (not Testing.FileExists(effect)) then
					TestSuite.AuditLog("test_client_error_w_mana_effect", "name", ward.name, "effect", effect)
					broken2 = true
				end
			end
		end

		if (broken or broken2) then
			errors = errors + 1
		end

		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_wards"), k, tblWards.count)
		wait(1)
	end

	TestSuite.AuditLog("test_client_msg_audit_w_done", "passed", (tblWards.count - errors), "total", tblWards.count)
	if (errors == 0) then
		TestSuite.AuditLog("test_client_msg_all_passed")
	end

	TestSuite.UpdateClientProgress(false)
	end))
end

-- Audits that all couriers in the base.upgrades has matching avatars in the courier base entitiy as well as valid icon/model/effect paths
function TestSuite.AuditCouriers()
	return SetActiveThread(newthread(function()
	TestSuite.UpdateClientProgress(true, Translate("test_client_progress_couriers"), 0, "?")
	wait(1)

	-- Get the entity info
	LoadEntityDefinitionsFromFolder("/shared/automated_courier/pet_courier/")
	local tblCourierAlts = Testing.GetDefinitionAltAvatars('Pet_AutomatedCourier')
	-- build a table with the alts as the keys for easy searching
	local courierAvatarKeys = {}
	for key,avatar in ipairs(tblCourierAlts) do
		courierAvatarKeys[avatar.avatarName] = key
	end

	local tblCouriers = Testing.GetCouriers()
	TestSuite.AuditLog("test_client_msg_audit_c", "count", tblCouriers.count)

	local errors = 0
	for k, courier in ipairs(tblCouriers) do
		local broken = false
		local index = courierAvatarKeys[courier.modifier]
		if (not index) then
			TestSuite.AuditLog("test_client_error_c_no_alt", "name", courier.name, "alt", courier.modifier)
			broken = true
		end

		-- Check paths
		if (not broken) then
			local modelPaths = Testing.GetModelsFromDefinition(tblCourierAlts[index].fullAvatarName)
			local iconPaths = Testing.GetIconsFromDefinition(tblCourierAlts[index].fullAvatarName)
			local effectPaths = Testing.GetEffectsFromDefinition(tblCourierAlts[index].fullAvatarName)

			for _,icon in ipairs(iconPaths.icon) do
				if (not Testing.FileExists(icon)) then
					TestSuite.AuditLog("test_client_error_c_mising_icon", "name", courier.name, "icon", icon)
					broken = true
				end
			end

			if (not Testing.FileExists(modelPaths.previewModel)) then
				TestSuite.AuditLog("test_client_error_c_mising_preview", "name", courier.name, "model", modelPaths.previewModel)
				broken = true
			end

			for _,model in ipairs(modelPaths.model) do
				if (not Testing.FileExists(model)) then
					TestSuite.AuditLog("test_client_error_c_mising_model", "name", courier.name, "model", model)
					broken = true
				end
			end

			for _,effect in ipairs(effectPaths.passiveEffect) do
				if (not Testing.FileExists(effect)) then
					TestSuite.AuditLog("test_client_error_c_mising_effect", "name", courier.name, "effect", effect)
					broken = true
				end
			end
		end

		if (broken) then
			errors = errors + 1
		end

		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_couriers"), k, tblCouriers.count)
		wait(1)
	end

	TestSuite.AuditLog("test_client_msg_audit_c_done", "passed", (tblCouriers.count - errors), "total", tblCouriers.count)
	if (errors == 0) then
		TestSuite.AuditLog("test_client_msg_all_passed")
	end

	TestSuite.UpdateClientProgress(false)
	end))
end

-- Audits that all announcers in the base.upgrades has all voice lines and models expected
function TestSuite.AuditAnnouncers()
	return SetActiveThread(newthread(function()
	TestSuite.UpdateClientProgress(true, Translate("test_client_progress_announcers"), 0, "?")
	wait(1)

	local tblAnnouncers = Testing.GetAnnouncers()
	TestSuite.AuditLog("test_client_msg_audit_av", "count", tblAnnouncers.count)

	local errors = 0
	for k, announcer in ipairs(tblAnnouncers) do
		-- Taken from the test_client_sounds stringtable
		local soundPaths = {
			--"/shared/sounds/announcer/"..announcer.voiceSet.."/prepare_for_battle.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/first_blood.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/3_kills.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/4_kills.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/5_kills.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/6_kills.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/7_kills.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/8_kills.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/9_kills.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/10_kills.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/immortal.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/double_kill.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/triple_kill.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/quad_kill.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/annihilation.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/massacre.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/genocide.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/denied.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/startgame.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/hellbourne_wins.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/legion_wins.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/victory.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/defeat.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/rival.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/payback.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/smackdown.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/humiliation.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/rage_quit.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/hellbourne_destroy_legion_tower.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/legion_destroy_hellbourne_tower.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/hellbourne_barracks_destroyed.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/legion_barracks_destroyed.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/structure_under_attack.ogg",
			--"/shared/sounds/announcer/"..announcer.voiceSet.."/ally_hero_under_attack.ogg",
			--"/shared/sounds/announcer/"..announcer.voiceSet.."/our_hero_under_attack.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/get_it_on.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/kongor_slain.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/zorgath_slain.ogg",
			"/shared/sounds/announcer/"..announcer.voiceSet.."/transmutantstein_slain.ogg",
		}

		-- Taken from game_new.lua
		local modelPaths = {
			"/ui/common/models/"..announcer.arcadeText.."/denied.mdf",
			"/ui/common/models/"..announcer.arcadeText.."/bloodlust.mdf",
			"/ui/common/models/"..announcer.arcadeText.."/hattrick.mdf",
			"/ui/common/models/"..announcer.arcadeText.."/quadkill.mdf",
			"/ui/common/models/"..announcer.arcadeText.."/annihilation.mdf",
			"/ui/common/models/"..announcer.arcadeText.."/bloodbath.mdf",
			"/ui/common/models/"..announcer.arcadeText.."/immortal.mdf",
			"/ui/common/models/"..announcer.arcadeText.."/genocide.mdf",
			"/ui/common/models/"..announcer.arcadeText.."/smackdown.mdf",
			"/ui/common/models/"..announcer.arcadeText.."/humiliation.mdf",
			"/ui/common/models/"..announcer.arcadeText.."/nemesis.mdf",
			"/ui/common/models/"..announcer.arcadeText.."/payback.mdf",
			"/ui/common/models/"..announcer.arcadeText.."/ragequit.mdf",
			"/ui/common/models/"..announcer.arcadeText.."/victory.mdf",
			"/ui/common/models/"..announcer.arcadeText.."/defeat.mdf",
			"/ui/common/models/"..announcer.arcadeText.."/bloodlust.effect",
		}

		local broken = false
		for _,path in ipairs(soundPaths) do
			if (not Testing.FileExists(path)) then
				TestSuite.AuditLog("test_client_error_av_missing_file", "name", announcer.name, "path", path)
				broken = true
			end
		end

		for _,path in ipairs(modelPaths) do
			if (not Testing.FileExists(path)) then
				TestSuite.AuditLog("test_client_error_av_missing_file", "name", announcer.name, "path", path)
				broken = true
			end
		end

		if (broken) then
			errors = errors + 1
		end

		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_announcers"), k, tblAnnouncers.count)
		wait(1)
	end

	TestSuite.AuditLog("test_client_msg_audit_av_done", "passed", (tblAnnouncers.count - errors), "total", tblAnnouncers.count)
	if (errors == 0) then
		TestSuite.AuditLog("test_client_msg_all_passed")
	end

	TestSuite.UpdateClientProgress(false)
	end))
end

function TestSuite.AuditUsage(entityName)
	if (not Testing.EntityExists(entityName)) then
		TestSuite.AuditLog("test_client_error_entity_doesnt_exist", "entityName", entityName)
		return false
	end

	Testing.UnloadContext('AuditUsage')

	local resTable = Testing.PrecacheAndGetResources(entityName, 'AuditUsage')

	local textureUsage = {}
	local sampleUsage = {}
	local modelUsage = {}
	local clipUsage = {}

	if (TestSuite.CheckboxEnabled('audit_usage_show_resources')) then
		for _,res in ipairs(resTable) do
			local resType = Testing.GetResourceType(res)
			if (resType.type == 5) then
				local resTable = {['path'] = res, ['usage'] = nil}
				pcall(function() resTable.usage = Testing.GetTextureUsage(res) end)
				table.insert(textureUsage, resTable)
			elseif (resType.type == 12) then
				local resTable = {['path'] = res, ['usage'] = nil}
				pcall(function() resTable.usage = Testing.GetSampleUsage(res) end)
				table.insert(sampleUsage, resTable)
			elseif (resType.type == 8) then
				local resTable = {['path'] = res, ['usage'] = nil}
				pcall(function() resTable.usage = Testing.GetResourceUsage(res) end)
				table.insert(modelUsage, resTable)
			elseif (resType.type == 9) then
				local resTable = {['path'] = res, ['usage'] = nil}
				pcall(function() resTable.usage = Testing.GetResourceUsage(res) end)
				table.insert(clipUsage, resTable)
			end
		end
	end

	local sortFunc = function(a, b)
		if ((a.usage == nil) and (b.usage == nil)) then
			return a.path < b.path
		elseif (a.usage == nil) then
			return true
		elseif (b.usage == nil) then
			return false
		end

		if (a.usage.memoryUsed == b.usage.memoryUsed) then
			return string.lower(a.path) < string.lower(b.path)
		end

		return a.usage.memoryUsed < b.usage.memoryUsed
	end

	local sortSimpleFunc = function(a, b)
		if ((a.usage == nil) and (b.usage == nil)) then
			return a.path < b.path
		elseif (a.usage == nil) then
			return true
		elseif (b.usage == nil) then
			return false
		end

		if (a.usage == b.usage) then
			return string.lower(a.path) < string.lower(b.path)
		end

		return a.usage < b.usage
	end

	table.sort(textureUsage, sortFunc)
	table.sort(sampleUsage, sortFunc)
	table.sort(modelUsage, sortSimpleFunc)
	table.sort(clipUsage, sortSimpleFunc)

	local memoryReport = Testing.GetContextMemoryReport("testing:curgame_AuditUsage")
	local usage = Testing.GetContextMemoryUsage("testing:curgame_AuditUsage")

	TestSuite.AuditLog("test_client_msg_measure_usage", "name", entityName)

	if (TestSuite.CheckboxEnabled('audit_usage_show_resources')) then
		TestSuite.AuditLog("test_client_msg_measure_usage_total", "memory", Testing.GetByteString(usage))
	end

	if (TestSuite.CheckboxEnabled('audit_usage_show_resources')) then
		TestSuite.AuditLog("")
		TestSuite.AuditLog("test_client_msg_usage_textures")
		for _,res in ipairs(textureUsage) do
			if (not Testing.ResourceSuccessfullyLoaded(res.path)) then
				local usage = 0
				if (res.usage ~= nil) then usage = res.usage.memoryUsed end
				TestSuite.AuditLog("test_client_msg_measure_usage_res_failed", "path", res.path, "memory", Testing.GetByteString(usage))
			elseif (res.usage == nil) then
				TestSuite.AuditLog("test_client_msg_measure_usage_res", "path", res.path, "memory", Testing.GetByteString(0))
			else
				if (res.usage.volume) then
					TestSuite.AuditLog("test_client_msg_usage_texture_depth", "path", res.path, "memory", Testing.GetByteString(res.usage.memoryUsed), "width", res.usage.width, "height", res.usage.height, "depth", res.usage.depth)
				elseif (res.usage.cube) then
					TestSuite.AuditLog("test_client_msg_usage_texture_depth", "path", res.path, "memory", Testing.GetByteString(res.usage.memoryUsed), "width", res.usage.width, "height", res.usage.height, "depth", "6")
				else
					TestSuite.AuditLog("test_client_msg_usage_texture", "path", res.path, "memory", Testing.GetByteString(res.usage.memoryUsed), "width", res.usage.width, "height", res.usage.height)
				end
			end
		end
	end
	TestSuite.AuditLog("test_client_msg_measure_usage_texture", "memory", Testing.GetByteString(memoryReport['{texture}'].memoryUsed), "count", memoryReport['{texture}'].numResources)
	
	if (TestSuite.CheckboxEnabled('audit_usage_show_resources')) then
		TestSuite.AuditLog("")
		TestSuite.AuditLog("test_client_msg_usage_samples")
		for _,res in ipairs(sampleUsage) do
			if (not Testing.ResourceSuccessfullyLoaded(res.path)) then
				local usage = 0
				if (res.usage ~= nil) then usage = res.usage.memoryUsed end
				TestSuite.AuditLog("test_client_msg_measure_usage_res_failed", "path", res.path, "memory", Testing.GetByteString(usage))
			elseif (res.usage == nil) then
				TestSuite.AuditLog("test_client_msg_measure_usage_res", "path", res.path, "memory", Testing.GetByteString(0))
			else
				TestSuite.AuditLog("test_client_msg_usage_sample", "path", res.path, "memory", Testing.GetByteString(res.usage.memoryUsed), "bits", res.usage.bits, "rate", res.usage.rate, "length", string.format("%.2f", (res.usage.length / 1000)))
			end
		end
	end
	TestSuite.AuditLog("test_client_msg_measure_usage_sample", "memory", Testing.GetByteString(memoryReport['{sample}'].memoryUsed), "count", memoryReport['{sample}'].numResources)

	if (TestSuite.CheckboxEnabled('audit_usage_show_resources')) then
		TestSuite.AuditLog("")
		TestSuite.AuditLog("test_client_msg_usage_models")
		for _,res in ipairs(modelUsage) do
			local usage = 0
			if (res.usage ~= nil) then usage = res.usage end

			if ((not Testing.ResourceSuccessfullyLoaded(res.path)) or (not Testing.FileExists(res.path))) then
				TestSuite.AuditLog("test_client_msg_measure_usage_res_failed", "path", res.path, "memory", Testing.GetByteString(usage))
			else
				TestSuite.AuditLog("test_client_msg_measure_usage_res", "path", res.path, "memory", Testing.GetByteString(usage))
			end
		end
	end
	TestSuite.AuditLog("test_client_msg_measure_usage_model", "memory", Testing.GetByteString(memoryReport['{model}'].memoryUsed), "count", memoryReport['{model}'].numResources)

	if (TestSuite.CheckboxEnabled('audit_usage_show_resources')) then
		TestSuite.AuditLog("")
		TestSuite.AuditLog("test_client_msg_usage_clips")
		for _,res in ipairs(clipUsage) do
			local usage = 0
			if (res.usage ~= nil) then usage = res.usage end

			if ((not Testing.ResourceSuccessfullyLoaded(res.path)) or (not Testing.FileExists(res.path))) then
				TestSuite.AuditLog("test_client_msg_measure_usage_res_failed", "path", res.path, "memory", Testing.GetByteString(usage))
			else
				TestSuite.AuditLog("test_client_msg_measure_usage_res", "path", res.path, "memory", Testing.GetByteString(usage))
			end
		end
	end
	TestSuite.AuditLog("test_client_msg_measure_usage_clip", "memory", Testing.GetByteString(memoryReport['{clip}'].memoryUsed), "count", memoryReport['{clip}'].numResources)

	if (not TestSuite.CheckboxEnabled('audit_usage_show_resources')) then
		TestSuite.AuditLog("test_client_msg_measure_usage_total", "memory", Testing.GetByteString(usage))
	end

	Testing.UnloadContext('AuditUsage')
end

function TestSuite.GenerateUnitUsageReport(unitType) 
	return SetActiveThread(newthread(function()
	TestSuite.UpdateClientProgress(true, Translate("test_client_progress_usage"), 0, "?")
	wait(1)

	TestSuite.LoadEntities(true)

	local unitTable = Testing.GetUnits(unitType)	-- 1 heroes, 2 pets, 3 creeps, 4 gadgets, 5 building, 6 powerup, 7 neutral, 8 critter, 9 tool, 10 state
	local csvTable = {}

	local num = table.getn(unitTable)

	Testing.UnloadContext("usage")
	for k,v in ipairs(unitTable) do
		csvTable[v] = {}

		Testing.Precache(v, "usage")
		csvTable[v].contextReport = Testing.GetContextMemoryReport("testing:curgame_usage")
		csvTable[v].contextUsage = Testing.GetContextMemoryUsage("testing:curgame_usage")

		Testing.UnloadContext("usage")

		TestSuite.UpdateClientProgress(true, Translate("test_client_progress_usage"), k, num)
		wait(1)
	end

	local reportFileNames = {
		[0] = 'complete_usageReport',
		[1] = 'heroes_usageReport',
		[2] = 'pets_usageReport',
		[3] = 'creeps_usageReport',
		[4] = 'gadgets_usageReport',
		[5] = 'building_usageReport',
		[6] = 'powerup_usageReport',
		[7] = 'neutral_usageReport',
		[8] = 'critter_usageReport',
		[9] = 'tool_usageReport',
		[10] = 'state_usageReport',
	}

	local dateString = string.gsub(Cvar.GetCvar('host_date'):GetString(), "/", "_")
	local timeString = string.gsub(Cvar.GetCvar('host_time'):GetString(), ":", "_")
	local filePath = Testing.GetSystemPath('~/'..reportFileNames[unitType or 0]..'_'..dateString..'_'..timeString..'.csv')

	local file = nil
	local outputFunction = nil
	if (not io) then	-- We won't have the io library one some platforms
		outputFunction = print
	else
		file = io.open(filePath, 'w')
		if (not file) then
			outputFunction = print
		else
			outputFunction = function(str)
				file:write(str)
			end
		end
	end

	outputFunction('"Entity","Path","Total Context Usage","Total Context Usage String","Texture Resource Count","Texture Context Usage","Texture Context Usage String","Sample Resource Count","Sample Context Usage","Sample Context Usage String","Model Resource Count","Model Context Usage","Model Context Usage String","Clip Resource Count","Clip Context Usage","Clip Context Usage String"\n')
	for entity, info in pairs(csvTable) do
		-- Entity
		outputFunction('"'..entity..'",')
		-- Path
		local altSplit = entity:find('.', 0, true)
		if (altSplit) then
			outputFunction('"'..Testing.GetEntityFilePath(entity:sub(0, altSplit - 1))..'",')
		else
			outputFunction('"'..Testing.GetEntityFilePath(entity)..'",')
		end
		-- Total Usage
		outputFunction('"'..info.contextUsage..'",')
		-- Total Usage String
		outputFunction('"'..Testing.GetByteString(info.contextUsage)..'",')

		-- No texture usage case
		if (not info.contextReport['{texture}']) then
			outputFunction('"0","0","0 bytes",')
		else
			-- Texture Resource Count
			outputFunction('"'..info.contextReport['{texture}'].numResources..'",')
			-- Texture Resource Usage
			outputFunction('"'..info.contextReport['{texture}'].memoryUsed..'",')
			-- Texture Resource Usage String
			outputFunction('"'..Testing.GetByteString(info.contextReport['{texture}'].memoryUsed)..'",')
		end

		-- No sample usage case
		if (not info.contextReport['{sample}']) then
			outputFunction('"0","0","0 bytes",')
		else
			-- Sample Resource Count
			outputFunction('"'..info.contextReport['{sample}'].numResources..'",')
			-- Sample Resource Usage
			outputFunction('"'..info.contextReport['{sample}'].memoryUsed..'",')
			-- Sample Resource Usage String
			outputFunction('"'..Testing.GetByteString(info.contextReport['{sample}'].memoryUsed)..'",')
		end

		-- No model usage case
		if (not info.contextReport['{model}']) then
			outputFunction('"0","0","0 bytes",')
		else
			-- Model Resource Count
			outputFunction('"'..info.contextReport['{model}'].numResources..'",')
			-- Model Resource Usage
			outputFunction('"'..info.contextReport['{model}'].memoryUsed..'",')
			-- Model Resource Usage String
			outputFunction('"'..Testing.GetByteString(info.contextReport['{model}'].memoryUsed)..'",')
		end

		-- No clip usage case
		if (not info.contextReport['{clip}']) then
			outputFunction('"0","0","0 bytes"\n')
		else
			-- Clip Resource Count
			outputFunction('"'..info.contextReport['{clip}'].numResources..'",')
			-- Clip Resource Usage
			outputFunction('"'..info.contextReport['{clip}'].memoryUsed..'",')
			-- Clip Resource Usage String
			outputFunction('"'..Testing.GetByteString(info.contextReport['{clip}'].memoryUsed)..'"\n')
		end
	end

	if (file) then
		file:flush()
		file:close()

		TestSuite.AuditLog("test_client_msg_usage_report_saved", "path", filePath)
	else
		TestSuite.AuditLog("test_client_msg_usage_report_console")
		Cmd('FlushLogs')
	end

	TestSuite.UpdateClientProgress(false)
	end))
end

function TestSuite.MountWorld(world)
	local gamePhase = tonumber(UITrigger.GetTrigger('GamePhase'):GetLastValue())
	if (gamePhase ~= 0) then
		TestSuite.AuditLog('test_client_error_mount_world_ingame')
		TestSuite.GetWidget('test_client_world'):SetSelectedItemByValue(Testing.GetMountedWorld(), false)
		return
	end

	if (world == 'NONE') then
		Testing.UnmountWorld()
		TestSuite.AuditLog('test_client_msg_unmount_world')
	else
		Testing.MountWorld(world)
		TestSuite.AuditLog('test_client_msg_mount_world', 'world', world)

		-- Not sure if this is needed, but it will ensure that any new definitions inside the map are loaded
		if (TestSuite.entitiesLoaded) then
			TestSuite.AuditLog('test_client_msg_mount_world_loading_defs')

			newthread(function()
				wait(1)
				LoadEntityDefinitionsFromFolder('/')
				TestSuite.AuditLog('test_client_msg_loading_defs_complete')
			end)
		end
	end
end