---------------------------------------------------------- 
--	Name: 		Game Shop Script            			--				
--  Copyright 2015 Frostburn Studios					--
----------------------------------------------------------
local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface = object
local interfaceName = interface:GetName()
RegisterScript2('Shop', '30')
Shop = {}
Shop.items = {}
Shop.COMP_VAR_MIN, Shop.COMP_VAR_MAX = 0, 139
Shop.rCompVarHolders = {}
Shop.rCompVarHolders.cur = {}
Shop.rCompVarHolders.old = {}
Shop.recipe = {}
Shop.shopFlag_animSpeedMult = 1
Shop.guide_fadein = 80
Shop._shopFlag_useAnims = true
Shop._shopFlag_maxViewableItems = 24
Shop._shopFlag_maxViewable0 = 24
Shop._shopFlag_maxViewable1 = 36
Shop._shopControlDown = false
Shop.ItemV2Panels = {}
Shop.ItemV2SimplePanels = {}
Shop.ItemCur = {}
Shop.MaxShopItems = 0
Shop.shopItemRowYSimple = {}
Shop.shopItemRowY = {}

local function GetWidgetShop(widget, fromInterface, hideErrors)
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
			if (not hideErrors) then println('GetWidget failed to find ' .. tostring(widget) .. ' in interface ' .. tostring(fromInterface)) end
			return nil		
		end	
	else
		println('GetWidget called without a target')
		return nil
	end
end
local GetWidget = memoizeObject(GetWidgetShop)

----------------------------------------------------------
-- 						Animation						--
----------------------------------------------------------

local function CompIsNewItem(index)
	if (not Shop.rCompVarHolders.old[index]) or 
		(
			(Shop.rCompVarHolders.cur[index] and Shop.rCompVarHolders.old[index]) and
			(Shop.rCompVarHolders.cur[index].Entity ~= Shop.rCompVarHolders.old[index].Entity)
		)	
	then
		return true
	else
		return false
	end
end

local function storeAlphaUpdate(sourceWidget, param0) 
	sourceWidget:SetColor('1 1 1 ' .. (tonumber(param0) / 100))
end

local function StoreRecipeExpandedComponents(index)
	local index = tonumber(index)
	
	Shop.rCompVarHolders.old[index] = Shop.rCompVarHolders.old[index] or {}	
	Shop.rCompVarHolders.old[index].Valid 				= Shop.rCompVarHolders.cur[index].Valid
	Shop.rCompVarHolders.old[index].Name 				= Shop.rCompVarHolders.cur[index].Name
	Shop.rCompVarHolders.old[index].Desc 				= Shop.rCompVarHolders.cur[index].Desc
	Shop.rCompVarHolders.old[index].Texture			 	= Shop.rCompVarHolders.cur[index].Texture
	Shop.rCompVarHolders.old[index].Cost 				= Shop.rCompVarHolders.cur[index].Cost
	Shop.rCompVarHolders.old[index].IsRecipe 			= Shop.rCompVarHolders.cur[index].IsRecipe
	Shop.rCompVarHolders.old[index].OwnLocal 			= Shop.rCompVarHolders.cur[index].OwnLocal
	Shop.rCompVarHolders.old[index].ToStash 			= Shop.rCompVarHolders.cur[index].ToStash
	Shop.rCompVarHolders.old[index].Available 			= Shop.rCompVarHolders.cur[index].Available
	Shop.rCompVarHolders.old[index].OwnRemote 			= Shop.rCompVarHolders.cur[index].OwnRemote
	Shop.rCompVarHolders.old[index].Recommended 		= Shop.rCompVarHolders.cur[index].Recommended
	Shop.rCompVarHolders.old[index].Entity 				= Shop.rCompVarHolders.cur[index].Entity
	Shop.rCompVarHolders.old[index].RecipeOverlay 		= Shop.rCompVarHolders.cur[index].RecipeOverlay
	
	--if NotEmpty(Shop.rCompVarHolders.old[index].Entity) then println('^w'..index..' ^rOld: ' .. Shop.rCompVarHolders.old[index].Entity ) end
end

local function RecipeExpandedComponents(index, sourceWidget, param0, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16, param17, param18, param19, param20, param21, param22, param23, param24, param25, param26, param27, param28, param29)
	local index = tonumber(index)
	--println('^g RecipeExpandedComponents | Shop.rCompVarHolders.cur.' .. index)
	--[[
	if (Shop.rCompVarHolders.cur[index]) then
		StoreRecipeExpandedComponents(index)
	end
	--]]
	Shop.rCompVarHolders.cur[index] = Shop.rCompVarHolders.cur[index] or {}	
	Shop.rCompVarHolders.cur[index].Valid 				= AtoB(param0)
	Shop.rCompVarHolders.cur[index].Name 				= param1
	Shop.rCompVarHolders.cur[index].Desc 				= param2
	Shop.rCompVarHolders.cur[index].Texture			 	= param3
	Shop.rCompVarHolders.cur[index].Cost 				= param5
	Shop.rCompVarHolders.cur[index].IsRecipe 			= AtoB(param8)
	Shop.rCompVarHolders.cur[index].OwnLocal 			= AtoB(param11)
	Shop.rCompVarHolders.cur[index].ToStash 			= AtoB(param15)
	Shop.rCompVarHolders.cur[index].Available 			= AtoB(param25)
	Shop.rCompVarHolders.cur[index].OwnRemote 			= AtoB(param26)
	Shop.rCompVarHolders.cur[index].Recommended 		= AtoB(param27)
	Shop.rCompVarHolders.cur[index].Entity 				= param28
	Shop.rCompVarHolders.cur[index].RecipeOverlay 		= AtoB(param29)
	Shop.rCompVarHolders.cur[index].FoundMatch 			= Shop.rCompVarHolders.cur[index].FoundMatch or false
	
	--if NotEmpty(Shop.rCompVarHolders.cur[index].Entity) then println('^w'..index..'  ^yCur: ' .. Shop.rCompVarHolders.cur[index].Entity ) end
end

local function cleanRecommendedItemString(sourceWidget, param0, param1)
	if (NotEmpty(param1)) then
		local _overrideRecommendedItems = GetCvarString('_overrideRecommendedItems')
		if (NotEmpty(_overrideRecommendedItems)) then
			Set('_overrideRecommendedItems', _overrideRecommendedItems  .. ',' .. param1)
		else
			Set('_overrideRecommendedItems', _overrideRecommendedItems  .. '' .. param1)
		end
	end
end

local function RecipeItemRequest(sourceWidget, param0)
	--println('^g RecipeItemRequest')
	local _shopNewRecipeTarget = param0 or ''
	Set('_shopNewRecipeTarget', _shopNewRecipeTarget)
	Set('_recipeTransitionMode', '0')
	Trigger('shopComponentAnimStep0')
end

local function StoreRecipeItemData()
	--println('^r StoreRecipeItemData')
	if ( Shop.recipe.cur) then 
		Shop.recipe.old = Shop.recipe.old or {}	
		Shop.recipe.old.Texture			 	= Shop.recipe.cur.Texture
		Shop.recipe.old.Cost 				= Shop.recipe.cur.Cost
		Shop.recipe.old.IsRecipe 			= Shop.recipe.cur.IsRecipe
		Shop.recipe.old.OwnLocal 			= Shop.recipe.cur.OwnLocal
		Shop.recipe.old.ToStash 			= Shop.recipe.cur.ToStash
		Shop.recipe.old.Available 			= Shop.recipe.cur.Available
		Shop.recipe.old.OwnRemote 			= Shop.recipe.cur.OwnRemote
		Shop.recipe.old.Recommended 		= Shop.recipe.cur.Recommended
		Shop.recipe.old.Entity 				= Shop.recipe.cur.Entity
		Shop.recipe.old.RecipeOverlay 		= Shop.recipe.cur.RecipeOverlay
	end
end

local function shopComponentAnimStep0()
	--println('^r shopComponentAnimStep0')
	for index, animTable in pairs(Shop.rCompVarHolders.cur) do 		-- Store old component data 
		StoreRecipeExpandedComponents(index)
	end
	StoreRecipeItemData()	-- Store old active recipe data 
	for index, varTable in pairs(Shop.rCompVarHolders.cur) do
		varTable.FoundMatch = false
	end
	interface:UICmd("SetActiveRecipe(_shopNewRecipeTarget);")			-- Set active recipe/new data vars are set (RecipeSequence triggers after this completes) 	
end

local function shopComponentAnimStep1()
	Trigger('updateRecipeDisplay');
	local storeCompCurValid76 = false
	if (Shop.rCompVarHolders.cur) and (Shop.rCompVarHolders.cur[76]) and (Shop.rCompVarHolders.cur[76].Valid) then
		storeCompCurValid76 = true
	end
	groupfcall('activeRecipeGroup', function(index, widget, groupName) widget:DoEventN(5) end, 'game_shop_v3')
	if (storeCompCurValid76) then
		groupfcall('recipeComponentItemIcons2', function(index, widget, groupName) widget:DoEventN(1) end, 'game_shop_v3')
		groupfcall('boot_tree_items', function(index, widget, groupName) widget:DoEvent() end, 'game_shop_v3')
	else
		groupfcall('recipeComponentItemIcons', function(index, widget, groupName) widget:DoEventN(1) end, 'game_shop_v3')
		groupfcall('recipeBranchImages', function(index, widget, groupName) widget:DoEventN(0) end, 'game_shop_v3')		
	end
	Set('_recipeTransitionMode', '0')	
end

local function RecipeSequence()
	--println('^g RecipeSequence')
	Set('_shopNewRecipeTarget', GetActiveRecipe())
	GetWidget('purchase_components_popup'):DoEventN(1)		
	
	local setRecipeViewStyle = 0
	local storeCompCurValid76 = false
	if (Shop.rCompVarHolders.cur) and (Shop.rCompVarHolders.cur[76]) and (Shop.rCompVarHolders.cur[76].Valid) then
		setRecipeViewStyle = 1
		storeCompCurValid76 = true
	end
	
	Trigger('setRecipeViewStyle', setRecipeViewStyle)		
	groupfcall('activeRecipeGroup', function(index, widget, groupName) widget:DoEventN(4) end, 'game_shop_v3')
	
	if (storeCompCurValid76) then
		--println('^o RecipeSequence A 1')
		Set('_recipeTransitionMode', '0')
		if (GetCvarInt('_recipeTransitionMode') == 1) then		
			groupfcall('recipeComponentItemIcons2', function(index, widget, groupName) widget:DoEventN(0) end, 'game_shop_v3')		
		else
			groupfcall('recipeComponentItemIcons', function(index, widget, groupName) widget:DoEventN(4) end, 'game_shop_v3')
			groupfcall('recipeComponentItemIcons2', function(index, widget, groupName) widget:DoEventN(4) end, 'game_shop_v3')
		end
	else
		--println('^o RecipeSequence A 2')
		Trigger('componentReadTree')
		Trigger('componentApplyTreeAnim')
		GetWidget('component_tree_parent'):DoEvent()
		groupfcall('recipe_tree_labels', function(index, widget, groupName) widget:DoEvent() end, 'game_shop_v3')
		
		if (GetCvarInt('_recipeTransitionMode') == 1) then
			groupfcall('recipeComponentItemIcons', function(index, widget, groupName) widget:DoEventN(5) end, 'game_shop_v3')
			groupfcall('recipeComponentItemIcons', function(index, widget, groupName) widget:DoEventN(3) end, 'game_shop_v3')
			GetWidget('shopActiveRecipeButton_1'):DoEventN(2)
			GetWidget('shopActiveRecipeButton_2'):DoEventN(2)
			groupfcall('recipeComponentItemIcons', function(index, widget, groupName) widget:DoEventN(0) end, 'game_shop_v3')
		else	
			groupfcall('recipeComponentItemIcons', function(index, widget, groupName) widget:DoEventN(4) end, 'game_shop_v3')
			groupfcall('recipeComponentItemIcons2', function(index, widget, groupName) widget:DoEventN(4) end, 'game_shop_v3')			
		end
	end
	
	groupfcall('recipeBranchImages', function(index, widget, groupName) widget:DoEvent() end, 'game_shop_v3')
	GetWidget('shop_v4_listbox'):Sleep(200 * Shop.shopFlag_animSpeedMult, function() shopComponentAnimStep1() end)
end

local function UpdateGameShop()
	--println('^g UpdateGameShop')
	GetWidget('game_shop_main_controller'):DoEventN(0)
end

function Shop:RegisterRecipeComponentItemIcon(sourceWidget, index, groupSuffix)
	--println('^g RegisterRecipeComponentItemIcon')
	local groupSuffix = groupSuffix or ''
	local index = tonumber(index)
	sourceWidget.onevent0 = function()
		--println('^g RegisterRecipeComponentItemIcon onevent0')
		if (Shop.rCompVarHolders.cur[index]) then
			if (not Shop.rCompVarHolders.cur[index].Valid) or ((not Shop.rCompVarHolders.cur[index].FoundMatch) and CompIsNewItem(index) ) then
				sourceWidget:FadeOut(200 * Shop.shopFlag_animSpeedMult)
			end
			if (Shop.rCompVarHolders.cur[index].Valid) and (not Shop.rCompVarHolders.cur[index].FoundMatch) then
				Trigger('recipeCompDisplay'..groupSuffix..index)
				sourceWidget:FadeIn(200 * Shop.shopFlag_animSpeedMult)	
			end
		end
	end
	
	sourceWidget.onevent1 = function()
		if (Shop.rCompVarHolders.cur[index]) then
			--println('^g RegisterRecipeComponentItemIcon onevent1')
			if (Shop.rCompVarHolders.cur[index].FoundMatch) and CompIsNewItem(index) then
				sourceWidget:SetVisible(false)
				--println('^o RegisterRecipeComponentItemIcon onevent1 A')
			end
			if (Shop.rCompVarHolders.cur[index].Valid) then
				Trigger('recipeCompDisplay'..groupSuffix..index)
				sourceWidget:FadeIn(200 * Shop.shopFlag_animSpeedMult)
				--println('^o RegisterRecipeComponentItemIcon onevent1 B')
			end
		end
	end	
	
	sourceWidget.onevent3 = function()	
		if (Shop.rCompVarHolders.cur[index]) then
			if ((Shop.rCompVarHolders.old[index]) and Shop.rCompVarHolders.old[index].Valid) and (not Shop.rCompVarHolders.cur[index].FoundMatch) then
				Trigger('componentPreMatchFound', index, sourceWidget:GetAbsoluteX(), sourceWidget:GetAbsoluteY(), sourceWidget:GetWidth(), sourceWidget:GetHeight() )				
			end
		end
	end		
	
	sourceWidget.onevent4 = function()		
		if (Shop.rCompVarHolders.cur[index]) then
			--println('^g RegisterRecipeComponentItemIcon onevent4')
			if (not Shop.rCompVarHolders.cur[index].Valid) or CompIsNewItem(index) then
				sourceWidget:FadeOut(200 * Shop.shopFlag_animSpeedMult)
				--println('^o RegisterRecipeComponentItemIcon onevent4 A')
			end
		end
	end		
	
	sourceWidget.onevent5 = function()
		--println('^g RegisterRecipeComponentItemIcon onevent5')
		if (Shop.rCompVarHolders.cur[index]) then
			if ((Shop.rCompVarHolders.old[index]) and Shop.rCompVarHolders.old[index].Valid) and (not Shop.rCompVarHolders.cur[index].FoundMatch) then
				local yloc
				local yIndex = Shop.rows._storeCompCurRowIndex[index]
				if (index > 71) then 
					yloc = Shop.buildup.GroupTargY[yIndex]
				else
					yloc = Shop.component.GroupTargY[yIndex]
				end
				Trigger(
					'recipeCheckComponentMatch',
					index,
					sourceWidget:GetAbsoluteX(),
					(
						sourceWidget:GetYFromString(yloc) +
						GetCvarInt('component_tree_baseY') +
						(
							( GetCvarInt('component_branch_height') - sourceWidget:GetHeight() ) / 2
						)
					),
					sourceWidget:GetWidth(),
					sourceWidget:GetHeight()
				)		
			end
		end
	end		
	
	sourceWidget.onevent6 = function()
		--println('^g RegisterRecipeComponentItemIcon onevent6')
		if (Shop.rCompVarHolders.cur[index]) then
			Shop.rCompVarHolders.cur[index].FoundMatch = false
		end
	end		
	
	local function componentPreMatchFound(sourceWidget, targetIndex, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10)
		local targetIndex = tonumber(targetIndex)
		if (Shop.rCompVarHolders.cur[index]) then
			if (Shop.rCompVarHolders.cur[index].Valid) and (index ~= targetIndex) then
				if (Shop.rCompVarHolders.old[targetIndex]) then
					if ( ((not Shop.rCompVarHolders.cur[index].RecipeOverlay) and (not Shop.rCompVarHolders.old[targetIndex].RecipeOverlay)) or  ((Shop.rCompVarHolders.cur[index].RecipeOverlay) and (Shop.rCompVarHolders.old[targetIndex].RecipeOverlay)) ) then					
						if ((Shop.rCompVarHolders.cur[index].Entity == Shop.rCompVarHolders.old[targetIndex].Entity)) then
							Shop.rCompVarHolders.cur[index].FoundMatch = true
							local yloc
							local yIndex = Shop.rows._storeCompCurRowIndex[index]
							if (index > 71) then 
								yloc = Shop.buildup.GroupTargY[yIndex]
							else
								yloc = Shop.component.GroupTargY[yIndex]
							end				
							Trigger(
								'instantiateRecipeTransition1',
								targetIndex,
								param1,
								param2,
								param3,
								param4,
								index,
								sourceWidget:GetAbsoluteX(),
								(
									sourceWidget:GetYFromString( (yloc) ) +
									GetCvarInt('component_tree_baseY') +
									(
										( GetCvarInt('component_branch_height') - sourceWidget:GetHeight() ) / 2
									)
								),
								sourceWidget:GetWidth(),
								sourceWidget:GetHeight()
							)				
						end
					end
				end
			end
		end
	end
	sourceWidget:RegisterWatch('componentPreMatchFound', componentPreMatchFound)
	
	local function recipeFindMatch(sourceWidget, param0, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10)
		if (Shop.rCompVarHolders.cur[index]) then
			if (not Shop.recipe.cur.FoundMatch) and (Shop.rCompVarHolders.cur[index].Valid) then
				if ((Shop.rCompVarHolders.old[index]) and (Shop.rCompVarHolders.cur[index].Entity == Shop.recipe.old.Entity)) then
					Shop.recipe.cur.FoundMatch = true
					local yloc
					local yIndex = Shop.rows._storeCompCurRowIndex[index]
					if (index > 71) then 
						yloc = Shop.buildup.GroupTargY[yIndex]
					else
						yloc = Shop.component.GroupTargY[yIndex]
					end		
					
					Trigger('InstantiateRecipeIconTrans2',
						'RecipeItem',
						'',
						param0,
						param1,
						param2,
						param3,
						'RecipeExpandedComponents',
						tostring(index),
						sourceWidget:GetAbsoluteX(),
						(
							sourceWidget:GetYFromString( (yloc) ) +
							GetCvarInt('component_tree_baseY') +
							(
								( GetCvarInt('component_branch_height') - sourceWidget:GetHeight() ) / 2
							)
						),
						sourceWidget:GetWidth(),
						sourceWidget:GetHeight()
					)									
				end
			end
		end
	end
	sourceWidget:RegisterWatch('recipeFindMatch'..groupSuffix, recipeFindMatch)	
	
	sourceWidget.onrightclick = function()
		if (Shop.rCompVarHolders.cur[index]) then
			if (Shop.rCompVarHolders.cur[index].Entity ~= interface:UICmd("GetActiveRecipe()")) then
				if (Shop.rCompVarHolders.cur[index].IsRecipe) and (not Shop.rCompVarHolders.cur[index].RecipeOverlay) then
					Set('_lastSelectedItemEntity', (Shop.rCompVarHolders.cur[index].Entity))
					Trigger(
						'shopBuyRemainingInfo',
						tostring(Shop.rCompVarHolders.cur[index].Name),
						tostring(Shop.rCompVarHolders.cur[index].Desc),
						tostring(Shop.rCompVarHolders.cur[index].Texture)							
					)
					GetWidget('purchase_components_popup', 'game_shop_v3'):DoEventN(0)
				else
					interface:UICmd("PurchaseByName('".. tostring(Shop.rCompVarHolders.cur[index].Entity).."')")
				end
			else
				interface:UICmd("PurchaseByName('".. tostring(Shop.rCompVarHolders.cur[index].Entity).."')")
			end
		end
	end		
	
	sourceWidget.onclick = function()
		if (Shop.rCompVarHolders.cur[index]) then
			Set('_shopNewRecipeTarget', (Shop.rCompVarHolders.cur[index].Entity))
			Set('_recipeTransitionMode', '1')
			Trigger('shopComponentAnimStep0')
		end
	end		
	
	sourceWidget:RefreshCallbacks()
end

function Shop:RegisterLeftTreeImage(sourceWidget)
	--println('^g RegisterLeftTreeImage')
	sourceWidget.onevent = function()
		if (Shop.component) and (Shop.component.GroupValid) then
			if ((not Shop.component.GroupValid[0]) and (not Shop.component.GroupValid[1]) and (not Shop.component.GroupValid[2]) and (not Shop.component.GroupValid[3])) or
				((Shop.component.GroupValid[0] ~= Shop.component.GroupValidOld[0]) or (Shop.component.GroupValid[1] ~= Shop.component.GroupValidOld[1]) or (Shop.component.GroupValid[2] ~= Shop.component.GroupValidOld[2]) or (Shop.component.GroupValid[3] ~= Shop.component.GroupValidOld[3]))
			then
				sourceWidget:SlideX('5h', 200 * Shop.shopFlag_animSpeedMult)
			else
				sourceWidget:FadeOut(200 * Shop.shopFlag_animSpeedMult)		
			end
		end	
	end
	sourceWidget.onevent0 = function()
		if (Shop.component) and (Shop.component.GroupValid) then
			if (Shop.component.GroupValid[0]) or (Shop.component.GroupValid[1]) or (Shop.component.GroupValid[2]) or (Shop.component.GroupValid[3]) then
				if (Shop.component.GroupValid[3]) then
					sourceWidget:SetTexture('/ui/common/new_shop_interface/tree_left_4.tga')
				elseif (Shop.component.GroupValid[2]) then
					sourceWidget:SetTexture('/ui/common/new_shop_interface/tree_left_3.tga')
				elseif (Shop.component.GroupValid[1]) then
					sourceWidget:SetTexture('/ui/common/new_shop_interface/tree_left_2.tga')
				else
					sourceWidget:SetTexture('/ui/common/new_shop_interface/tree_left_1.tga')					
				end
				sourceWidget:SlideX('0', 200 * Shop.shopFlag_animSpeedMult)
				sourceWidget:FadeIn(200 * Shop.shopFlag_animSpeedMult)	
			end
		end	
	end	
	sourceWidget:RefreshCallbacks()

	sourceWidget:RegisterWatch('storeAlphaUpdate', storeAlphaUpdate)
end

function Shop:RegisterRightTreeImage(sourceWidget)
	--println('^g RegisterRightTreeImage')
	sourceWidget.onevent = function()
		if (Shop.buildup) and (Shop.buildup.GroupValid) then
			if ((not Shop.buildup.GroupValid[0]) and (not Shop.buildup.GroupValid[1]) and (not Shop.buildup.GroupValid[2]) and (not Shop.buildup.GroupValid[3])) or
				((Shop.buildup.GroupValid[0] ~= Shop.buildup.GroupValidOld[0]) or (Shop.buildup.GroupValid[1] ~= Shop.buildup.GroupValidOld[1]) or (Shop.buildup.GroupValid[2] ~= Shop.buildup.GroupValidOld[2]) or (Shop.buildup.GroupValid[3] ~= Shop.buildup.GroupValidOld[3]))
			then
				sourceWidget:SlideX('-5h', 200 * Shop.shopFlag_animSpeedMult)
			else
				sourceWidget:FadeOut(200 * Shop.shopFlag_animSpeedMult)		
			end
		end	
	end
	sourceWidget.onevent0 = function()
		if (Shop.buildup) and (Shop.buildup.GroupValid) then
			if (Shop.buildup.GroupValid[0]) or (Shop.buildup.GroupValid[1]) or (Shop.buildup.GroupValid[2]) or (Shop.buildup.GroupValid[3]) then
				if (Shop.buildup.GroupValid[3]) then
					sourceWidget:SetTexture('/ui/common/new_shop_interface/tree_right_4.tga')
				elseif (Shop.buildup.GroupValid[2]) then
					sourceWidget:SetTexture('/ui/common/new_shop_interface/tree_right_3.tga')
				elseif (Shop.buildup.GroupValid[1]) then
					sourceWidget:SetTexture('/ui/common/new_shop_interface/tree_right_2.tga')
				else
					sourceWidget:SetTexture('/ui/common/new_shop_interface/tree_right_1.tga')					
				end
				sourceWidget:SlideX('0', 200 * Shop.shopFlag_animSpeedMult)
				sourceWidget:FadeIn(200 * Shop.shopFlag_animSpeedMult)	
			end
		end	
	end	
	sourceWidget:RefreshCallbacks()
	
	sourceWidget:RegisterWatch('storeAlphaUpdate', storeAlphaUpdate)
end

function Shop:RegisterRecipeTreeLabelComponents(sourceWidget)
	--println('^g RegisterRecipeTreeLabelComponents')
	sourceWidget.onevent = function()
		if (Shop.component) and (Shop.component.GroupValid) then
			if (Shop.component.GroupValid[0]) or (Shop.component.GroupValid[1]) or (Shop.component.GroupValid[2]) or (Shop.component.GroupValid[3]) then
				if (not sourceWidget:IsVisible()) then
					sourceWidget:FadeIn(250)
				end
			else
				if (sourceWidget:IsVisible()) then
					sourceWidget:FadeOut(250)
				end			
			end
		end
	end
	sourceWidget:RefreshCallbacks()
end

function Shop:RegisterRecipeTreeLabelBuildup(sourceWidget)
	--println('^g RegisterRecipeTreeLabelBuildup')
	sourceWidget.onevent = function()
		if (Shop.buildup) and (Shop.buildup.GroupValid) then
			if (Shop.buildup.GroupValid[0]) or (Shop.buildup.GroupValid[1]) or (Shop.buildup.GroupValid[2]) or (Shop.buildup.GroupValid[3]) then
				if (not sourceWidget:IsVisible()) then
					sourceWidget:FadeIn(250)
				end
			else
				if (sourceWidget:IsVisible()) then
					sourceWidget:FadeOut(250)
				end			
			end
		end
	end
	sourceWidget:RefreshCallbacks()
end

function Shop:RegisterBootTreeItem(sourceWidget, itemIndex, slideX8, slideX7, slideX6, slideX5)
	--println('^g RegisterBootTreeItem')
	local itemIndex = tonumber(itemIndex)
	sourceWidget.onevent = function () 
		if (Shop.rCompVarHolders.cur) and (Shop.rCompVarHolders.cur[itemIndex]) and (Shop.rCompVarHolders.cur[itemIndex].Valid) then
			sourceWidget:FadeIn(200 * Shop.shopFlag_animSpeedMult)
			if (Shop.rCompVarHolders.cur[79].Valid) then
				sourceWidget:SlideX(slideX8, 200 * Shop.shopFlag_animSpeedMult)
			elseif (Shop.rCompVarHolders.cur[78].Valid) then
				sourceWidget:SlideX(slideX7, 200 * Shop.shopFlag_animSpeedMult)
			elseif (Shop.rCompVarHolders.cur[77].Valid) then
				sourceWidget:SlideX(slideX6, 200 * Shop.shopFlag_animSpeedMult)
			elseif (Shop.rCompVarHolders.cur[76].Valid) then
				sourceWidget:SlideX(slideX5, 200 * Shop.shopFlag_animSpeedMult)
			end
		else
			sourceWidget:FadeOut(200 * Shop.shopFlag_animSpeedMult)
		end
	end
	sourceWidget:RefreshCallbacks()
end

function Shop:RegisterBootTreeItemCost(sourceWidget, itemIndex)
	--println('^g RegisterBootTreeItemCost')
	local itemIndex = tonumber(itemIndex)
	local function updateRecipeDisplay()
		if (Shop.rCompVarHolders.cur[itemIndex]) then
			if CompIsNewItem(itemIndex) then
				sourceWidget:FadeOut(200 * Shop.shopFlag_animSpeedMult)
				sourceWidget:Sleep(200 * Shop.shopFlag_animSpeedMult, function() 
						if (Shop.rCompVarHolders.cur) and (Shop.rCompVarHolders.cur[itemIndex]) and (Shop.rCompVarHolders.cur[itemIndex].Valid) then
							sourceWidget:FadeIn(200 * Shop.shopFlag_animSpeedMult)
						end
					end)
			elseif (Shop.rCompVarHolders.cur) and (Shop.rCompVarHolders.cur[itemIndex]) and (Shop.rCompVarHolders.cur[itemIndex].Valid) then
				sourceWidget:FadeIn(200 * Shop.shopFlag_animSpeedMult)
			end
		end
	end
	sourceWidget:RegisterWatch('updateRecipeDisplay', updateRecipeDisplay)
end

function Shop:RegisterBootTreeItemLabel(sourceWidget, itemIndex)
	--println('^g RegisterBootTreeItemLabel')
	local itemIndex = tonumber(itemIndex)
	local function updateRecipeDisplay()
		if (Shop.rCompVarHolders.cur[itemIndex]) then
			if CompIsNewItem(itemIndex)	then
				sourceWidget:Sleep(200 * Shop.shopFlag_animSpeedMult, function() 
						if (Shop.rCompVarHolders.cur) and (Shop.rCompVarHolders.cur[itemIndex]) and (Shop.rCompVarHolders.cur[itemIndex].Valid) then
							sourceWidget:SetText(Shop.rCompVarHolders.cur[itemIndex].Cost)
						end
					end)
			elseif (Shop.rCompVarHolders.cur) and (Shop.rCompVarHolders.cur[itemIndex]) and (Shop.rCompVarHolders.cur[itemIndex].Valid) then
				sourceWidget:SetText(Shop.rCompVarHolders.cur[itemIndex].Cost)
			end
		end
	end
	sourceWidget:RegisterWatch('updateRecipeDisplay', updateRecipeDisplay)
end

function Shop:RegisterBootTreeItemName(sourceWidget, itemIndex)
	--println('^g RegisterBootTreeItemName')
	local itemIndex = tonumber(itemIndex)
	local function updateRecipeDisplay()
		if (Shop.rCompVarHolders.cur[itemIndex]) then
			if CompIsNewItem(itemIndex)	then
				sourceWidget:FadeOut(200 * Shop.shopFlag_animSpeedMult)
				sourceWidget:Sleep(200 * Shop.shopFlag_animSpeedMult, function() 
						if (Shop.rCompVarHolders.cur) and (Shop.rCompVarHolders.cur[itemIndex]) and (Shop.rCompVarHolders.cur[itemIndex].Valid) then
							sourceWidget:SetText(Shop.rCompVarHolders.cur[itemIndex].Name)
							sourceWidget:FadeIn(200 * Shop.shopFlag_animSpeedMult)
						end
					end)
			elseif (Shop.rCompVarHolders.cur) and (Shop.rCompVarHolders.cur[itemIndex]) and (Shop.rCompVarHolders.cur[itemIndex].Valid) then
				sourceWidget:SetText(Shop.rCompVarHolders.cur[itemIndex].Name)
				sourceWidget:FadeIn(200 * Shop.shopFlag_animSpeedMult)			
			end
		end
	end
	sourceWidget:RegisterWatch('updateRecipeDisplay', updateRecipeDisplay)
end

function Shop:RegisterTreeComponentImage(sourceWidget, rowIndex, slotIndex, destination, slideMe, sideIndex)
	local rowIndex, slotIndex = tonumber(rowIndex), tonumber(slotIndex)
	local function componentSlotVis(watchWidget, param0, param1, param2)
		local param0, param1 = tonumber(param0), tonumber(param1)
		if (param0 == sideIndex) and (param1 == rowIndex) and (Shop.rCompVarHolders.cur) and (Shop.rCompVarHolders.cur[slotIndex]) then
			--println('^g componentSlotVis | ' .. tonumber(param0) .. ' == ' .. sideIndex.. ' | ' .. tonumber(param1) .. ' == ' .. tonumber(rowIndex).. ' | Shop.rCompVarHolders.cur.'.. slotIndex.. ': ' .. tostring(Shop.rCompVarHolders.cur[slotIndex]))
			if AtoB(param2) and (Shop.rCompVarHolders.cur[slotIndex].Valid) then
				sourceWidget:FadeIn(200 * Shop.shopFlag_animSpeedMult)
				if (slideMe) then sourceWidget:SlideX('0', 200 * Shop.shopFlag_animSpeedMult) end
				--println('^o componentSlotVis A 1 | ' ..slotIndex)
			else
				sourceWidget:FadeOut(200 * Shop.shopFlag_animSpeedMult)
				if (slideMe) then sourceWidget:SlideX(destination, 200 * Shop.shopFlag_animSpeedMult) end
				--println('^o componentSlotVis A 2 | ' ..slotIndex)
			end
		end
	end
	sourceWidget:RegisterWatch('componentSlotVis', componentSlotVis)
end

function Shop:RegisterTreeComponentBase(sourceWidget, rowIndex, slotInit, slotSubA, slotSubB, slotSubC, slotSubD, itemY1, itemY2, itemY3, itemY4)
	--println('^g RegisterTreeComponentBase')
	local rowIndex, slotInit, slotSubA, slotSubB, slotSubC, slotSubD = tonumber(rowIndex), tonumber(slotInit), tonumber(slotSubA), tonumber(slotSubB), tonumber(slotSubC), tonumber(slotSubD)
	Shop.component = Shop.component or {}
	Shop.component.GroupValid = Shop.component.GroupValid or {}
	Shop.component.GroupValid[rowIndex] = false
	Shop.component.GroupValidOld = Shop.component.GroupValidOld or {}
	Shop.component.GroupValidOld[rowIndex] = false
	Shop.component.GroupTargY = Shop.component.GroupTargY or {}
	Shop.component.GroupTargY[rowIndex] = '0'
	
	Shop.rows = Shop.rows or {}
	Shop.rows._storeCompCurRowIndex = Shop.rows._storeCompCurRowIndex or {}
	Shop.rows._storeCompCurRowIndex[slotInit] = rowIndex
	Shop.rows._storeCompCurRowIndex[slotSubA] = rowIndex
	Shop.rows._storeCompCurRowIndex[slotSubB] = rowIndex
	Shop.rows._storeCompCurRowIndex[slotSubC] = rowIndex
	Shop.rows._storeCompCurRowIndex[slotSubD] = rowIndex
	
	local function componentReadTree()	
		--println('^g componentReadTree')
		Shop.component.GroupValidOld[rowIndex] = Shop.component.GroupValid[rowIndex]		
		if (Shop.rCompVarHolders.cur) and (
			(Shop.rCompVarHolders.cur[slotInit] and Shop.rCompVarHolders.cur[slotInit].Valid) or 
			(Shop.rCompVarHolders.cur[slotSubA] and Shop.rCompVarHolders.cur[slotSubA].Valid) or 
			(Shop.rCompVarHolders.cur[slotSubB] and Shop.rCompVarHolders.cur[slotSubB].Valid) or 
			(Shop.rCompVarHolders.cur[slotSubC] and Shop.rCompVarHolders.cur[slotSubC].Valid) or 
			(Shop.rCompVarHolders.cur[slotSubD] and Shop.rCompVarHolders.cur[slotSubD].Valid)) 
		then
			Shop.component.GroupValid[rowIndex] = true
			--println('^o componentReadTree A 1')
		else
			Shop.component.GroupValid[rowIndex] = false
			--println('^o componentReadTree A 2')
		end
	end
	sourceWidget:RegisterWatch('componentReadTree', componentReadTree)
	
	local function componentApplyTreeAnim()	
		--println('^g componentApplyTreeAnim')
		if (Shop.component) and (Shop.component.GroupValid) and (Shop.component.GroupValid[rowIndex]) then
			Trigger('componentSlotVis', '0', rowIndex, 'true')
			if ((Shop.component.GroupValid[3])) then
				Shop.component.GroupTargY[rowIndex] = itemY4
			elseif ((Shop.component.GroupValid[2])) then
				Shop.component.GroupTargY[rowIndex] = itemY3		
			elseif ((Shop.component.GroupValid[1])) then
				Shop.component.GroupTargY[rowIndex] = itemY2			
			else
				Shop.component.GroupTargY[rowIndex] = itemY1
			end
			--println('Shop.component.GroupTargY[rowIndex] |  ' .. rowIndex .. '   |   ' .. Shop.component.GroupTargY[rowIndex] )
			sourceWidget:SlideY(Shop.component.GroupTargY[rowIndex], 200 * Shop.shopFlag_animSpeedMult)
			--println('^o componentApplyTreeAnim A 1')
		else
			Trigger('componentSlotVis', '0', rowIndex, 'false')
			--println('^o componentApplyTreeAnim A 2')
		end
	end
	sourceWidget:RegisterWatch('componentApplyTreeAnim', componentApplyTreeAnim)		
	
	sourceWidget.onevent2 = function()
		if (Shop.component) and (Shop.component.GroupValid) and (Shop.component.GroupValid[rowIndex]) then
			sourceWidget:SetVisible(true)
		else
			sourceWidget:SetVisible(false)
		end
	end
	sourceWidget:RefreshCallbacks()
end

function Shop:CompareCurrentItemWithStash(sourceWidget, targetIndex)
	local targetIndex = tonumber(targetIndex)
	if (Shop.rCompVarHolders.cur) and (Shop.rCompVarHolders.cur[targetIndex]) then
		sourceWidget:SetVisible(Shop.rCompVarHolders.cur[targetIndex].ToStash)
	end
end

function Shop:RegisterTreeComponentBuildup(sourceWidget, rowIndex, slotInit, slotSubA, slotSubB, slotSubC, slotSubD, itemY1, itemY2, itemY3, itemY4)
	--println('^g RegisterTreeComponentBuildup')
	local rowIndex, slotInit, slotSubA, slotSubB, slotSubC, slotSubD = tonumber(rowIndex), tonumber(slotInit), tonumber(slotSubA), tonumber(slotSubB), tonumber(slotSubC), tonumber(slotSubD)
	Shop.buildup = Shop.buildup or {}
	Shop.buildup.GroupValid = Shop.buildup.GroupValid or {}
	Shop.buildup.GroupValid[rowIndex] = false
	Shop.buildup.GroupValidOld = Shop.buildup.GroupValidOld or {}
	Shop.buildup.GroupValidOld[rowIndex] = false
	Shop.buildup.GroupTargY = Shop.buildup.GroupTargY or {}
	Shop.buildup.GroupTargY[rowIndex] = '0'
	
	Shop.rows = Shop.rows or {}
	Shop.rows._storeCompCurRowIndex = Shop.rows._storeCompCurRowIndex or {}
	Shop.rows._storeCompCurRowIndex[slotInit] = rowIndex
	Shop.rows._storeCompCurRowIndex[slotSubA] = rowIndex
	Shop.rows._storeCompCurRowIndex[slotSubB] = rowIndex
	Shop.rows._storeCompCurRowIndex[slotSubC] = rowIndex
	Shop.rows._storeCompCurRowIndex[slotSubD] = rowIndex
	
	local function componentReadTree()	
		Shop.buildup.GroupValidOld[rowIndex] = Shop.buildup.GroupValid[rowIndex]		
		if (Shop.rCompVarHolders.cur) and ((Shop.rCompVarHolders.cur[slotInit] and Shop.rCompVarHolders.cur[slotInit].Valid) or (Shop.rCompVarHolders.cur[slotSubA] and Shop.rCompVarHolders.cur[slotSubA].Valid) or (Shop.rCompVarHolders.cur[slotSubB] and Shop.rCompVarHolders.cur[slotSubB].Valid) or (Shop.rCompVarHolders.cur[slotSubC] and Shop.rCompVarHolders.cur[slotSubC].Valid) or (Shop.rCompVarHolders.cur[slotSubD] and Shop.rCompVarHolders.cur[slotSubD].Valid)) then
			Shop.buildup.GroupValid[rowIndex] = true
		else
			Shop.buildup.GroupValid[rowIndex] = false
		end
	end
	sourceWidget:RegisterWatch('componentReadTree', componentReadTree)
	
	local function componentApplyTreeAnim()	
		if (Shop.buildup) and (Shop.buildup.GroupValid) and (Shop.buildup.GroupValid[rowIndex]) then
			Trigger('componentSlotVis', '1', rowIndex, 'true')
			if ((Shop.buildup.GroupValid[3])) then
				Shop.buildup.GroupTargY[rowIndex] = itemY4
			elseif ((Shop.buildup.GroupValid[2])) then
				Shop.buildup.GroupTargY[rowIndex] = itemY3		
			elseif ((Shop.buildup.GroupValid[1])) then
				Shop.buildup.GroupTargY[rowIndex] = itemY2			
			else
				Shop.buildup.GroupTargY[rowIndex] = itemY1
			end
			sourceWidget:SlideY(Shop.buildup.GroupTargY[rowIndex], 200 * Shop.shopFlag_animSpeedMult)
		else
			Trigger('componentSlotVis', '1', rowIndex, 'false')
		end
	end
	sourceWidget:RegisterWatch('componentApplyTreeAnim', componentApplyTreeAnim)		
	
	sourceWidget.onevent2 = function()
		if (Shop.buildup) and (Shop.buildup.GroupValid) and (Shop.buildup.GroupValid[rowIndex]) then
			sourceWidget:SetVisible(true)
		else
			sourceWidget:SetVisible(false)
		end
	end
	sourceWidget:RefreshCallbacks()
end

local function InitializeShopItemList(sourceWidget)	
	--println('^g InitializeShopItemList')

	local shop_v4_itemList = GetWidget('shop_v4_itemList')
	shop_v4_itemList.onevent0 = function()
		shop_v4_itemList:FadeIn(200 * Shop.shopFlag_animSpeedMult)
	end		
	shop_v4_itemList.onevent1 = function()
		if (shop_v4_itemList:IsVisibleSelf()) then
			shop_v4_itemList:FadeOut(200 * Shop.shopFlag_animSpeedMult)
		end
	end	
	shop_v4_itemList.onevent2 = function()
		shop_v4_itemList:SetVisible(true)
	end	
	shop_v4_itemList.onevent3 = function()
		if (shop_v4_itemList:IsVisibleSelf()) then
			shop_v4_itemList:SetVisible(false)
		end
	end
	shop_v4_itemList.onhotkey = function()
		PlaySound('/shared/sounds/ui/button_close_01.wav')
		Set('_lastShopSearch', '')
		Set('_activeShopFilters', '')
		Trigger('SetShopDisplay', '')
	end	
	shop_v4_itemList:RefreshCallbacks()
end

local function InitializeShopNoItemsFound(sourceWidget)	
	--println('^g InitializeShopNoItemsFound')
	local shop_popup_noitemsfound = GetWidget('shop_popup_noitemsfound')
	shop_popup_noitemsfound.onevent0 = function()
		if (not shop_popup_noitemsfound:IsVisibleSelf()) then
			shop_popup_noitemsfound:FadeIn(200 * Shop.shopFlag_animSpeedMult)
		end
	end		
	shop_popup_noitemsfound.onevent1 = function()
		if (shop_popup_noitemsfound:IsVisibleSelf()) then
			shop_popup_noitemsfound:FadeOut(200 * Shop.shopFlag_animSpeedMult)
		end
	end	
	shop_popup_noitemsfound.onevent2 = function()
		if (not shop_popup_noitemsfound:IsVisibleSelf()) then
			shop_popup_noitemsfound:SetVisible(true)
		end
	end	
	shop_popup_noitemsfound.onevent3 = function()
		if (shop_popup_noitemsfound:IsVisibleSelf()) then
			shop_popup_noitemsfound:SetVisible(false)
		end
	end
	
	local function storeAlphaUpdate2(sourceWidget, param0)
		shop_popup_noitemsfound:SetColor('0 0 0 ' .. (0.25 + (tonumber(param0) * 0.005)))
	end
	shop_popup_noitemsfound:RegisterWatch('storeAlphaUpdate', storeAlphaUpdate2)
	shop_popup_noitemsfound:RefreshCallbacks()
end

local function InitializeShopAnimations(sourceWidget)
	--println('^g InitializeShopAnimations')
	interface:RegisterWatch('RecipeSequence', RecipeSequence)
	GetWidget('shop_v4_listbox').onevent5 = function()
		Set('_overrideRecommendedItems', 'Item_HomecomingStone')
		interface:UICmd("ExplodeTrigger('cleanRecommendedItemString', guide_recommendedString, '|');")
		interface:UICmd("OverrideRecommendedItems(_overrideRecommendedItems);")
		end
	GetWidget('shop_v4_listbox'):RefreshCallbacks()
	interface:RegisterWatch('cleanRecommendedItemString', cleanRecommendedItemString)
	interface:RegisterWatch('RecipeItemRequest', RecipeItemRequest)
	interface:RegisterWatch('shopComponentAnimStep0', shopComponentAnimStep0)
	GetWidget('game_shop_main_controller'):RegisterWatch('RecipeSequence', UpdateGameShop)
	GetWidget('game_shop_main_controller'):RegisterWatch('RecipeItemType', UpdateGameShop)
	for i=Shop.COMP_VAR_MIN,Shop.COMP_VAR_MAX ,1 do
		interface:RegisterWatch('RecipeExpandedComponents'..i, function(...) RecipeExpandedComponents(i, ...) end)
	end
	InitializeShopItemList(sourceWidget)
	InitializeShopNoItemsFound(sourceWidget)	
end

----------------------------------------------------------
-- 				Shop Recipe Item Button					--
----------------------------------------------------------

local function InitializeShopRecipeItem(sourceWidget) 

	local function PopulateRecipeItemData()
		--println('^o PopulateRecipeItemData')
		Shop.recipe.cur = Shop.recipe.cur or {}	
		Shop.recipe.cur.Title 				= ''
		Shop.recipe.cur.Desc 				= ''
		Shop.recipe.cur.Cost 				= ''
		Shop.recipe.cur.FoundMatch 			= false
		
		Shop.recipe.cur.Texture			 	= ''
		Shop.recipe.cur.IsRecipe 			= false
		Shop.recipe.cur.OwnLocal 			= false
		Shop.recipe.cur.ToStash 			= false
		Shop.recipe.cur.Available 			= false
		Shop.recipe.cur.OwnRemote 			= false
		Shop.recipe.cur.Recommended 		= false
		Shop.recipe.cur.Entity 				= ''
		Shop.recipe.cur.RecipeOverlay 		= false
		
		Shop.recipe.old = Shop.recipe.old or {}	
		Shop.recipe.old.Texture			 	= ''
		Shop.recipe.old.IsRecipe 			= false
		Shop.recipe.old.OwnLocal 			= false
		Shop.recipe.old.ToStash 			= false
		Shop.recipe.old.Available 			= false
		Shop.recipe.old.OwnRemote 			= false
		Shop.recipe.old.Recommended 		= false
		Shop.recipe.old.Entity 				= ''
		Shop.recipe.old.RecipeOverlay 		= false		
	end	
	 
	local function RecipeItem(sourceWidget, param0, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16, param17, param18, param19, param20, param21, param22, param23, param24, param25, param26, param27, param28, param29)
		--println('^g RecipeItem | Shop.rCompVarHolders.cur.')
		if (Shop.recipe.cur) then
			StoreRecipeItemData()
		end
		Shop.recipe.cur = Shop.recipe.cur or {}	
		Shop.recipe.cur.Valid 				= AtoB(param0)
		Shop.recipe.cur.Title 				= param1
		Shop.recipe.cur.Desc 				= param2
		Shop.recipe.cur.Texture			 	= param3
		Shop.recipe.cur.Cost 				= param5
		Shop.recipe.cur.IsRecipe 			= AtoB(param8)
		Shop.recipe.cur.OwnLocal 			= AtoB(param11)
		Shop.recipe.cur.ToStash 			= AtoB(param15)
		Shop.recipe.cur.Available 			= AtoB(param25)
		Shop.recipe.cur.OwnRemote 			= AtoB(param26)
		Shop.recipe.cur.Recommended 		= AtoB(param27)
		Shop.recipe.cur.Entity 				= param28
		Shop.recipe.cur.RecipeOverlay 		= AtoB(param29)
		Shop.recipe.cur.FoundMatch 			= Shop.recipe.cur.FoundMatch or false
	end	
	
	PopulateRecipeItemData()
	interface:RegisterWatch('RecipeItem', RecipeItem)
end

----------------------------------------------------------
-- 				Shop v4 Item Trigger					--
----------------------------------------------------------

local function InitializeShopItemTriggers(sourceWidget) 
	
	local function PopulateShopItemData(index)
		--println('^o Shop:PopulateShopItemData ' .. index)
		Shop.ItemCur[index] = {}	
		Shop.ItemCur[index].Valid 				= false
		Shop.ItemCur[index].Name 				= ''
		Shop.ItemCur[index].Desc 				= ''
		Shop.ItemCur[index].Texture			 	= ''
		Shop.ItemCur[index].Cost 				= ''
		Shop.ItemCur[index].IsRecipe 			= false
		Shop.ItemCur[index].OwnLocal 			= false
		Shop.ItemCur[index].ToStash 			= false
		Shop.ItemCur[index].IsNew 				= false
		Shop.ItemCur[index].Available 			= false
		Shop.ItemCur[index].OwnRemote 			= false
		Shop.ItemCur[index].Recommended 		= false
		Shop.ItemCur[index].Entity 				= ''
		Shop.ItemCur[index].RecipeOverlay 		= false
		Shop.ItemCur[index].SlotWatch = index
	end
	
	local function ShopItemParam(index, sourceWidget, param0, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16, param17, param18, param19, param20, param21, param22, param23, param24, param25, param26, param27, param28, param29)
		--println('^o Shop:ShopItemParam ' .. index)
		Shop.ItemCur[index] = Shop.ItemCur[index] or {}	
		Shop.ItemCur[index].Valid 				= AtoB(param0)	
	end
	
	local function ShopItem(index, sourceWidget, param0, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16, param17, param18, param19, param20, param21, param22, param23, param24, param25, param26, param27, param28, param29)	-- offset by 1
		--println('^o Shop:ShopItem ' .. index)
		Shop.ItemCur[index] = Shop.ItemCur[index] or {}	
		Shop.ItemCur[index].Name 				= param1
		Shop.ItemCur[index].Desc 				= param2
		Shop.ItemCur[index].Texture			 	= param3
		Shop.ItemCur[index].Cost 				= param4
		Shop.ItemCur[index].IsRecipe 			= AtoB(param8)
		Shop.ItemCur[index].OwnLocal 			= AtoB(param11)
		Shop.ItemCur[index].ToStash 			= AtoB(param15)
		Shop.ItemCur[index].IsNew 				= AtoB(param23)
		Shop.ItemCur[index].Available 			= AtoB(param25)
		Shop.ItemCur[index].OwnRemote 			= AtoB(param26)
		Shop.ItemCur[index].Recommended 		= AtoB(param27)
		Shop.ItemCur[index].Entity 				= param28
		Shop.ItemCur[index].RecipeOverlay 		= AtoB(param29)
	end
	
	local function MaxShopItems(_, maxItems)
		--println('^c Shop:MaxShopItems ' .. maxItems)
		local maxItems = tonumber(maxItems) or 0
		if maxItems > Shop.MaxShopItems then
			for i = 1,maxItems,1 do
				if (not Shop.ItemCur[i]) then
					PopulateShopItemData(i-1)
				end
				interface:RegisterWatch('ShopItem'..(i-1), 		function(...) ShopItem((i-1), ...) 		end)
				interface:RegisterWatch('ShopItemParam'..(i-1), function(...) ShopItemParam((i-1), ...) end)
			end
			Shop.MaxShopItems = maxItems
		end
	end
	interface:RegisterWatch('MaxShopItems', MaxShopItems)
end

----------------------------------------------------------
-- 						Guide							--
----------------------------------------------------------

function Shop:RegisterGuidePanel(sourceWidget, panel, id, watch, watch1)
	sourceWidget.onevent = function ()
		sourceWidget:SetVisible(false)
		sourceWidget:FadeIn(Shop.guide_fadein * tonumber(id) * Shop.shopFlag_animSpeedMult)
	end
	local function GuideRecommendedClear(sourceWidget)
		sourceWidget:SetVisible(false)
		sourceWidget.ontrigger3 = function () end
		sourceWidget:RefreshCallbacks()
	end
	sourceWidget:RegisterWatch('GuideRecommendedClear', GuideRecommendedClear)
	local function PopulateItemInfo(sourceWidget, index, itemEntity)
		if (id == index) and (string.len(itemEntity) >= 5) and NotEmpty(GetEntityIconPath(itemEntity)) then
			sourceWidget.ontrigger3 = function()
				sourceWidget:DoEvent()
				sourceWidget:UICmd("TriggerShopItem('"..watch1.."', '"..itemEntity.."', 0)")
			end	
			sourceWidget:RefreshCallbacks()
		end	
	end
	sourceWidget:RegisterWatch(watch, PopulateItemInfo)
	sourceWidget:RefreshCallbacks()
end

local function GuideIsScrollItem(panel, id)
	--println('guide_recommendeditem_isscroll: ' .. tostring(GetCvarBool('guide_recommendeditem_isscroll'..panel..id)))
	return GetCvarBool('guide_recommendeditem_isscroll'..panel..id)
end

local function IsShopControlDown()
	--println('IsShopControlDown: ' .. tostring(Shop._shopControlDown))
	return Shop._shopControlDown
end

local function ShopControlDown(sourceWidget, isControlDown)
	Shop._shopControlDown = AtoB(isControlDown)
end
interface:RegisterWatch('ShopControlDown', ShopControlDown)

function Shop:RegisterGuideAbilityLevel(sourceWidget, id)
	local function GuideInfoFormResultExplodeAbilities(_, index, itemEntity)
		if (id == index) then
			sourceWidget:UICmd([[
				SetOnMouseOver('
					TriggerLevelupTooltips();
					if(GetInventorySlotFromEntityName(\']]..itemEntity..[[\') ge 0, 
						ShowWidget(\'ability_levelup_tooltip_simple\'#GetInventorySlotFromEntityName(\']]..itemEntity..[[\'))
					);
				');]])
			sourceWidget:UICmd([[
				SetOnMouseOut('
					if(GetInventorySlotFromEntityName(\']]..itemEntity..[[\') ge 0, 
						HideWidget(\'ability_levelup_tooltip_simple\'#GetInventorySlotFromEntityName(\']]..itemEntity..[[\'))
					)
				;');]])
		end
	end
	sourceWidget:RegisterWatch('GuideInfoFormResultExplodeAbilities', GuideInfoFormResultExplodeAbilities)
end	
	
function Shop:RegisterGuideAbilityIcon(sourceWidget, id)
	local function GuideInfoFormResultExplodeAbilities(_, index, itemEntity)
		if (id == index) then
			if NotEmpty(itemEntity) then
				sourceWidget:SetTexture(GetEntityIconPath(itemEntity))
			else
				sourceWidget:SetTexture('$invis')
			end
		end
	end
	sourceWidget:RegisterWatch('GuideInfoFormResultExplodeAbilities', GuideInfoFormResultExplodeAbilities)
end		

function Shop:AltClickShopItem(itemName)
	if itemName == nil or itemName == '' then
		Echo('^rerror: itemName: '..tostring(itemName))
		return
	end
	local typeId = HoN.GetItemTypeID(itemName)
	SendGamePing('alt_shop_item', typeId)
end

function Shop:RegisterGuideButton(sourceWidget, panel, id, watch, watch1)

	local function GuideRecommendedClear(sourceWidget)
		sourceWidget.onmouseover = function () end
		sourceWidget.onmouseout = function () end
		sourceWidget.onclick = function () end
		sourceWidget.ontrigger1 = function () end
		sourceWidget.ontrigger2 = function () end
		sourceWidget:RefreshCallbacks()
	end
	sourceWidget:RegisterWatch('GuideRecommendedClear', GuideRecommendedClear)

	local function PopulateItemInfo(sourceWidget, index, itemEntity)
		if (id == index) and (string.len(itemEntity) >= 6) and NotEmpty(GetEntityIconPath(itemEntity)) then
			sourceWidget.onclick = function()
				if Input.IsAltDown() then
					Shop:AltClickShopItem(itemEntity)
					return
				end
				if (GetActiveRecipe() ~= itemEntity) then
					interface:UICmd("SetActiveRecipe('"..itemEntity.."')")
				else
					interface:UICmd("SetActiveRecipe('')")
				end
				GetWidget('shop_v4_listbox'):SetVisible(false)
			end	
			
			sourceWidget.onmouseover = function()
				interface:UICmd("TriggerItemTooltip('shopGlobalItemTooltipUpdate','"..itemEntity.."', false)")
				Trigger('shopGlobalItemTooltipIcon', GetEntityIconPath(itemEntity))
				Trigger('GuideRecommendedBorder', panel, id)
				GetWidget('shopGlobalItemTooltip'):SetVisible(true)
			end
			
			sourceWidget.onmouseout = function()
				Trigger('GuideRecommendedBorder', '', '')
				GetWidget('shopGlobalItemTooltip'):SetVisible(false)
			end			
			
			sourceWidget.ontrigger1 = function(_, param0, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16, param17, param18, param19, param20, param21, param22, param23, param24, param25, param26, param27, param28, param29, param30, param31, param32, param33, param34, param35, param36, param37, param38, param39)
				local components = tonumber(param39)
				if (components > 0) then
					sourceWidget.onrightclick = function()
						if (not IsShopControlDown()) or (not GuideIsScrollItem(panel, id)) then
							Set('_lastSelectedItemEntity', itemEntity)
							interface:UICmd("Trigger('shopBuyRemainingInfo', '"..EscapeString(param0).."', '"..EscapeString(param4).."', '"..GetEntityIconPath(itemEntity).."')'")
							GetWidget('purchase_components_popup'):DoEventN(0)
							GetWidget('shop_v4_listbox'):SetVisible(false)
						else
							interface:UICmd("Purchase2('"..itemEntity.."')")
							GetWidget('shop_v4_listbox'):SetVisible(false)
						end
					end
					sourceWidget:RefreshCallbacks()
				else
					sourceWidget.onrightclick = function()
						interface:UICmd("PurchaseByName('"..itemEntity.."')")
						GetWidget('shop_v4_listbox'):SetVisible(false)
					end
					sourceWidget:RefreshCallbacks()
				end
			end
			
			sourceWidget.ontrigger2 = function(_, param0, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16, param17, param18, param19, param20, param21, param22, param23, param24, param25, param26, param27, param28, param29, param30, param31, param32, param33, param34, param35, param36, param37, param38, param39)			
				if (param28 == itemEntity) then
					if AtoB(param8) and (not AtoB(param17)) and (not AtoB(param18)) then
						Set('guide_recommendeditem_isscroll'..panel..id, true)
					else
						Set('guide_recommendeditem_isscroll'..panel..id, false)
					end				
					sourceWidget.onmouseover = function()					
						interface:UICmd("TriggerItemTooltip('shopGlobalItemTooltipUpdate','"..itemEntity.."', false)")
						Trigger('shopGlobalItemTooltipIcon', GetEntityIconPath(itemEntity))
						Trigger('GuideRecommendedBorder', panel, id)
						GetWidget('shopGlobalItemTooltip'):SetVisible(true)						
						if (sourceWidget:IsVisible()) and (not GetWidget('item_cursor'):IsVisibleSelf()) and (not GetCvarBool('quickitem_dummy_draggingItem')) then
							Trigger('QuickItemDummyUpdate', param28, GetEntityIconPath(param28), sourceWidget:GetAbsoluteX(), sourceWidget:GetAbsoluteY(), '4.5h', '4.5h', panel..id)						
						end									
					end
					sourceWidget:RefreshCallbacks()
				end
			end			
			sourceWidget:RefreshCallbacks()
		end	
	end
	sourceWidget:RegisterWatch(watch, PopulateItemInfo)	
	
	sourceWidget:RefreshCallbacks()
end

function Shop:RegisterGuideIconBG(sourceWidget, panel, id, watch, watch1)
	local function PopulateItemInfo(sourceWidget, index, itemEntity)
		if (id == index) and (string.len(itemEntity) >= 6) and NotEmpty(GetEntityIconPath(itemEntity)) then
			sourceWidget.ontrigger = function(_, param0, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16, param17, param18, param19, param20, param21, param22, param23, param24, param25, param26, param27, param28, param29, param30, param31, param32, param33, param34, param35, param36, param37, param38, param39)
				if (param28 == itemEntity) then
					if AtoB(param8) and (not AtoB(param17)) and (not AtoB(param18)) then
						sourceWidget:SetVisible(true)
					else
						sourceWidget:SetVisible(false)
					end	
				end
			end	
			sourceWidget:RefreshCallbacks()
		end	
	end
	sourceWidget:RegisterWatch(watch, PopulateItemInfo)
end

function Shop:RegisterGuideIcon(sourceWidget, panel, id, watch, watch1)
	local function PopulateItemInfo(sourceWidget, index, itemEntity)
		if (id == index) and (string.len(itemEntity) >= 6) and NotEmpty(GetEntityIconPath(itemEntity)) then
			sourceWidget.ontrigger2 = function(_, param0, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16, param17, param18, param19, param20, param21, param22, param23, param24, param25, param26, param27, param28, param29, param30, param31, param32, param33, param34, param35, param36, param37, param38, param39)
				if (param28 == itemEntity) then
					sourceWidget:SetTexture(param3)
					if (AtoB(param6) or (AtoB(param8) and AtoB(param12)) ) then
						sourceWidget:UICmd("SetRenderMode('normal')")
					else
						sourceWidget:UICmd("SetRenderMode('grayscale')")
					end
				end
			end	
			sourceWidget:RefreshCallbacks()
		end	
	end
	sourceWidget:RegisterWatch(watch, PopulateItemInfo)
end

function Shop:RegisterGuideIconState(sourceWidget, panel, id, watch, watch1)
	local function PopulateItemInfo(sourceWidget, index, itemEntity)
		if (id == index) and (string.len(itemEntity) >= 6) and NotEmpty(GetEntityIconPath(itemEntity)) then
			sourceWidget:SetTexture(GetEntityIconPath(itemEntity))
		end	
	end
	sourceWidget:RegisterWatch(watch, PopulateItemInfo)
end

----------------------------------------------------------
-- 					ItemV2 WidgetState					--
----------------------------------------------------------
--[[
local function updateShopItem(sourceWidget, index, overState, itemDisplayType, overColor, baseColor, recOverColor, recBaseColor, twoColors) 
	println('updateShopItem: ' .. index)
	local _itemSlotWatch = GetCvarInt('_itemSlotWatch'..index)
	local _storeItemCurIsRecipe = GetCvarBool('_storeItemCurIsRecipe'.._itemSlotWatch) -- RMM deprecated
	local shopAlphaValue = GetCvarInt('shopAlphaValue')
	
	if (isRecipe) then
		if (twoColors) then
			sourceWidget:SetBorderColor(recOverColor .. (0.25 + (shopAlphaValue * 0.0075)))
			sourceWidget:SetColor(recBaseColor .. (0.25 + (shopAlphaValue * 0.0075)))
		else
			if AtoB(overState) then
				sourceWidget:SetColor(recOverColor .. (0.1 + (shopAlphaValue * 0.006)))
				sourceWidget:SetBorderColor(recOverColor .. (0.1 + (shopAlphaValue * 0.006)))		
			else
				sourceWidget:SetColor(recBaseColor.. (0.1 + (shopAlphaValue * 0.006)))
				sourceWidget:SetBorderColor(recBaseColor .. (0.1 + (shopAlphaValue * 0.006)))		
			end
		end		
	else	
		if (twoColors) then
			sourceWidget:SetBorderColor(overColor .. (0.25 + (shopAlphaValue * 0.0075)))
			sourceWidget:SetColor(baseColor .. (0.25 + (shopAlphaValue * 0.0075)))
		else
			if AtoB(overState) then
				sourceWidget:SetColor(overColor .. (0.1 + (shopAlphaValue * 0.006)))
				sourceWidget:SetBorderColor(overColor .. (0.1 + (shopAlphaValue * 0.006)))		
			else
				sourceWidget:SetColor(baseColor.. (0.1 + (shopAlphaValue * 0.006)))
				sourceWidget:SetBorderColor(baseColor .. (0.1 + (shopAlphaValue * 0.006)))		
			end
		end	
	end
end

function Shop:RegisterItemStateBG(sourceWidget, index, overState, itemDisplayType, overColor, baseColor, recOverColor, recBaseColor, twoColors)
	sourceWidget:RegisterWatch('updateShopItem'..index, function(...) 		updateShopItem(sourceWidget, index, overState, itemDisplayType, overColor, baseColor, recOverColor, recBaseColor, twoColors) end)
end
--]]

----------------------------------------------------------
-- 						ItemV2							--
----------------------------------------------------------

local function ItemV2PanelOnEvent5(index, targetWidget) --## Fade out - if switching item sets or if no search/filter/category is chosen ##
	if (targetWidget:IsVisibleSelf()) then
		targetWidget:FadeOut(30 * Shop.shopFlag_animSpeedMult)
	end
end

local function ItemV2SimplePanelOnEvent5(index, targetWidget) 
	if (targetWidget:IsVisibleSelf()) then
		targetWidget:FadeOut(30 * Shop.shopFlag_animSpeedMult)
	end
end

local function ItemV2PanelOnEvent3(index, targetWidget) --## Instant visibility ##
	if ((Shop.ItemCur[index]) and (Shop.ItemCur[index].Valid)) and (tonumber(index) >= GetCvarInt('_shopScrollMinID')) and (tonumber(index) <= GetCvarInt('_shopScrollMaxID')) then
		local targetRow = (ceil((((index - GetCvarInt('_shopScrollMinID')) + 1) / 4) - 1))
		if (Shop.shopItemRowY[targetRow]) then
			targetWidget:SetY(Shop.shopItemRowY[targetRow])
		end	
		Trigger('updateShopItem'..index)
		groupfcall('shop_v4_item2_group_'..index, function(index, widget, groupName) widget:DoEventN(3) end)
		groupfcall('shop_v4_item2_group2_'..index, function(index, widget, groupName) widget:DoEventN(3) end)
		if (not targetWidget:IsVisibleSelf()) then
			targetWidget:SetVisible(true)
		end
	elseif (targetWidget:IsVisibleSelf()) then
		targetWidget:SetVisible(false)
	end
	-- Triggers caldavar tutorial shop catagory checker
	Trigger('tut_shopitemrefresh')
end

local function ItemV2SimplePanelOnEvent3(index, targetWidget) 
	if ((Shop.ItemCur[index]) and (Shop.ItemCur[index].Valid)) and (tonumber(index) >= GetCvarInt('_shopScrollMinID')) and (tonumber(index) <= GetCvarInt('_shopScrollMaxID')) then
		local targetRow = (ceil((((index - GetCvarInt('_shopScrollMinID')) + 1) / 6) - 1))
		if (Shop.shopItemRowYSimple[targetRow]) then
			targetWidget:SetY(Shop.shopItemRowYSimple[targetRow])
		end
		Trigger('updateShopItemSimple'..index)
		groupfcall('shop_v4_item2_simple_group_'..index, function(index, widget, groupName) widget:DoEventN(3) end)
		groupfcall('shop_v4_item2_simple_group2_'..index, function(index, widget, groupName) widget:DoEventN(3) end)
		if (not targetWidget:IsVisibleSelf()) then
			targetWidget:SetVisible(true)
		end
	elseif (targetWidget:IsVisibleSelf()) then
		targetWidget:SetVisible(false)
	end
end

local function ItemV2PanelOnEvent4(index, targetWidget) --## Active to active switch (if the minimum item in the scroll range is visible) ##
	println('ItemV2PanelOnEvent4')
	ItemV2PanelOnEvent5(index, targetWidget) 
	targetWidget:Sleep(30 * Shop.shopFlag_animSpeedMult, function() 
		if ((Shop.ItemCur[index]) and (Shop.ItemCur[index].Valid)) and (tonumber(index) >= GetCvarInt('_shopScrollMinID')) and (tonumber(index) <= GetCvarInt('_shopScrollMaxID')) then
			targetWidget:Sleep(3 * tonumber(index) * Shop.shopFlag_animSpeedMult,
				function()
					local targetRow = (ceil((((index - GetCvarInt('_shopScrollMinID')) + 1) / 4) - 1))
					if (Shop.shopItemRowY[targetRow]) then
						targetWidget:SetY(Shop.shopItemRowY[targetRow])
					end					
					Trigger('updateShopItem'..index)
					groupfcall('shop_v4_item2_group_'..index, function(index, widget, groupName) widget:DoEventN(3) end)
					groupfcall('shop_v4_item2_group2_'..index, function(index, widget, groupName) widget:DoEventN(3) end)
					targetWidget:FadeIn(50 *  Shop.shopFlag_animSpeedMult)
				end
			)
		end	
	end)
end

local function ItemV2SimplePanelOnEvent4(index, targetWidget) 
	ItemV2SimplePanelOnEvent5(index, targetWidget) 
	targetWidget:Sleep(30 * Shop.shopFlag_animSpeedMult, function() 
		if ((Shop.ItemCur[index]) and (Shop.ItemCur[index].Valid)) and (tonumber(index) >= GetCvarInt('_shopScrollMinID')) and (tonumber(index) <= GetCvarInt('_shopScrollMaxID')) then
			targetWidget:Sleep(3 * tonumber(index) * Shop.shopFlag_animSpeedMult,
				function()
					local targetRow = (ceil((((index - GetCvarInt('_shopScrollMinID')) + 1) / 6) - 1))
					if (Shop.shopItemRowYSimple[targetRow]) then
						targetWidget:SetY(Shop.shopItemRowYSimple[targetRow])
					end
					Trigger('updateShopItemSimple'..index)
					groupfcall('shop_v4_item2_simple_group_'..index, function(index, widget, groupName) widget:DoEventN(3) end)
					groupfcall('shop_v4_item2_simple_group2_'..index, function(index, widget, groupName) widget:DoEventN(3) end)
					targetWidget:FadeIn(50 *  Shop.shopFlag_animSpeedMult)
				end
			)
		end	
	end)
end

local function UpdateShopItems(targetFunction, isSimple)
	if (isSimple) then
		for index, targetWidget in pairs(Shop.ItemV2SimplePanels) do
			targetFunction(index, targetWidget)
		end	
	else
		for index, targetWidget in pairs(Shop.ItemV2Panels) do
			targetFunction(index, targetWidget)
		end
	end
end

function Shop:RegisterItemV2SimplePanel(sourceWidget, index, reportRowY)
	--println('D RegisterItemV2SimplePanel ' .. index)
	-- onload
	sourceWidget:SetX( ( 16.3 * (index % 6) ) .. 'h' )	
	sourceWidget:SetY( ( 8 * floor(index / 6) ) .. 'h' )	
	if (reportRowY) and NotEmpty(reportRowY) then
		Shop.shopItemRowYSimple[tonumber(reportRowY)] = sourceWidget:GetY()
	end	
	-- insert into widget data table
	Shop.ItemV2SimplePanels[tonumber(index)] = sourceWidget
	
	local function storeAlphaUpdate(sourceWidget) 
		if (sourceWidget:IsVisible()) then
			groupfcall('shop_v4_item2_simple_group_'..index, function(index, widget, groupName) widget:DoEventN(3) end)
			groupfcall('shop_v4_item2_simple_group2_'..index, function(index, widget, groupName) widget:DoEventN(3) end)
		end
	end
	sourceWidget:RegisterWatch('storeAlphaUpdate', storeAlphaUpdate)
end

function Shop:RegisterItemV2Panel(sourceWidget, index, reportRowY)
	--println('C RegisterItemV2Panel ' .. index)
	-- onload
	sourceWidget:SetX( ( 24.5 * (index % 4) ) .. 'h' )	
	sourceWidget:SetY( ( 7.9 * floor(index / 4) ) .. 'h' )
	if (reportRowY) and NotEmpty(reportRowY) then
		Shop.shopItemRowY[tonumber(reportRowY)] = sourceWidget:GetY()		
	end	
	-- insert into widget data table
	Shop.ItemV2Panels[tonumber(index)] = sourceWidget
	
	local function storeAlphaUpdate(sourceWidget) 
		if (sourceWidget:IsVisible()) then
			groupfcall('shop_v4_item2_group_'..index, function(index, widget, groupName) widget:DoEventN(3) end)
			groupfcall('shop_v4_item2_group2_'..index, function(index, widget, groupName) widget:DoEventN(3) end)
		end
	end
	sourceWidget:RegisterWatch('storeAlphaUpdate', storeAlphaUpdate)
end

local function ShopItem(sourceWidget, index, reportRowY, itemDisplayType, height, width, _, param0, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16, param17, param18, param19, param20, param21, param22, param23, param24, param25, param26, param27, param28, param29, param30, param31, param32, param33, param34, param35, param36, param37, param38, param39) 
	--println('ShopItem: ' .. index .. ' | ' .. param27 .. ' | ' .. param28 .. ' | ' .. param29)
	sourceWidget:SetCallback('onclick', function()
		if Input.IsAltDown() then
			Shop:AltClickShopItem(param28)
			return
		end
		sourceWidget:DoEventN(1)
		if (GetActiveRecipe() ~= param28) then
			Set('_recipeTransitionMode', '0')
			Set('_shopNewRecipeTarget', param28)
			Trigger('shopComponentAnimStep0', 0)
		else
			sourceWidget:UICmd("SetActiveRecipe('')")
		end
	end)

	sourceWidget:SetCallback('onrightclick', function()
		sourceWidget:DoEventN(1)
		if (AtoB(param8) and (not AtoB(param17)) and (not AtoB(param18))) or (not AtoB(param8)) then
			if NotEmpty(GetCvarString('_lastActiveShop')) and Empty(GetCvarString('_activeShopFilters')) and Empty(GetCvarString('_lastShopSearch')) then
				sourceWidget:UICmd("Purchase('"..index.."')'")
			else
				sourceWidget:UICmd("PurchaseByName('"..param28.."')'")
			end
		else
			Set('_lastSelectedItemEntity', param28)
			interface:UICmd("Trigger('shopBuyRemainingInfo', '"..EscapeString(param1).."', '"..EscapeString(param2).."', '"..param3.."')'")
			GetWidget('purchase_components_popup'):DoEventN(0)
		end
	end)

	sourceWidget:SetCallback('onmouseover', function()
		if (tonumber(itemDisplayType) == 0) then
			groupfcall('shop_v4_item2_group_'..index, function(index, widget, groupName) widget:DoEventN(1) end, 'game_shop_v3')
		elseif (tonumber(itemDisplayType) == 1) then
			groupfcall('shop_v4_item2_simple_group_'..index, function(index, widget, groupName) widget:DoEventN(1) end, 'game_shop_v3')
		end
		interface:UICmd("TriggerItemTooltip('shopGlobalItemTooltipUpdate', '"..param28.."', "..param29..");")
		if NotEmpty(param28) then
			Trigger('shopGlobalItemTooltipIcon', GetEntityIconPath(param28))
		end
		if (sourceWidget:IsVisible()) and (not GetWidget('item_cursor'):IsVisibleSelf()) and (not GetCvarBool('quickitem_dummy_draggingItem')) then
			Trigger('QuickItemDummyUpdate', param28, GetEntityIconPath(param28), sourceWidget:GetAbsoluteX(), sourceWidget:GetAbsoluteY(), height, width, index)						
		end				
		sourceWidget:DoEventN(0)
	end)

	sourceWidget:RefreshCallbacks()
end

function Shop:RegisterItemV2Button(sourceWidget, index, reportRowY, itemDisplayType, height, width)
	local function ShopItemDisplayType(_, displayType)
		if (tonumber(displayType) == tonumber(itemDisplayType)) then
			sourceWidget:RegisterWatch('ShopItem'..index, function(...) ShopItem(sourceWidget, index, reportRowY, itemDisplayType, height, width, ...) end)
		else
			sourceWidget:UnregisterWatch('ShopItem'..index)
		end
	end
	sourceWidget:RegisterWatch('ShopItemDisplayType', ShopItemDisplayType)	
end

----------------------------------------------------------
-- 					Gen Widgets							--
----------------------------------------------------------

function Shop:RegisterCtrlHoldWidget(sourceWidget, panel, id)
	local function ShopControlDown(sourceWidget, param0)
		sourceWidget:SetVisible(AtoB(param0))
	end
	sourceWidget:RegisterWatch('ShopControlDown', ShopControlDown)
end

function Shop:RegisterQuickItemBorder(sourceWidget, panel, id)
	local function QuickItemBorder(_, param0, param1)
		if (param0 == panel..id) and (tonumber(param1) == -2) then
			sourceWidget:SetVisible(1)
		else
			sourceWidget:SetVisible(0)
		end
	end
	sourceWidget:RegisterWatch('QuickItemBorder', QuickItemBorder)
end

function Shop:RegisterGuideRecommendedBorder(sourceWidget, panel, id)
	local function GuideRecommendedBorder(sourceWidget, param0, param1)
		if (param0 == panel) and (param1 == id) then
			sourceWidget:SetVisible(1)
		else
			sourceWidget:SetVisible(0)
		end
	end
	sourceWidget:RegisterWatch('GuideRecommendedBorder', GuideRecommendedBorder)
	
	local function shopGlobalItemTooltipUpdate(sourceWidget, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, param39)
		if (tonumber(param39) > 0) then
			sourceWidget:SetBorderColor('#00acff')
		else
			sourceWidget:SetBorderColor('0 1 0 1')
		end
	end
	sourceWidget:RegisterWatch('shopGlobalItemTooltipUpdate', shopGlobalItemTooltipUpdate)	
end

function Shop:RegisterQuickSlotItemBorder(sourceWidget, panel, id)
	local function QuickItemBorder(sourceWidget, param0, param1)
		if (tonumber(param1) == -2) then
			sourceWidget:SetVisible(1)
			sourceWidget:SetBorderColor('1 1 0 1')
		elseif (param0 == panel) and (tonumber(param1) == tonumber(id)) then
			sourceWidget:SetVisible(1)
		else
			sourceWidget:SetVisible(0)
		end
	end
	sourceWidget:RegisterWatch('QuickItemBorder', QuickItemBorder)
	
	local function shopGlobalItemTooltipUpdate(sourceWidget, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, param39)
		if (tonumber(param39) > 0) then
			sourceWidget:SetBorderColor('#00acff')
		else
			sourceWidget:SetBorderColor('0 1 0 1')
		end
	end
	sourceWidget:RegisterWatch('shopGlobalItemTooltipUpdate', shopGlobalItemTooltipUpdate)	
end


----------------------------------------------------------
-- 						Init							--
----------------------------------------------------------

local function InitializeShopController(sourceWidget)
	--println('^o InitializeShopController 4')

	local controller = GetWidget('game_shop_main_controller')
	
	local function CurrentShopItems(sourceWidget, param0)
		Set('_lastCurrentShopItems', param0)
	end
	controller:RegisterWatch('CurrentShopItems', CurrentShopItems)
	
	local function ShopActive(sourceWidget, param0)
		--println('^o ShopActive 4')

		local shopActive = AtoB(param0)

		--trackedUserFocus:logFocus('shopOpen', shopActive)

		if not shopActive then
			Set('_shopFlag_guideOpen', 'false')
		end
		Set('_shopFlag_visible', param0)
		controller:DoEventN(0)
	end
	controller:RegisterWatch('ShopActive', ShopActive)	
	
	local function ShopActiveRequest(sourceWidget, requestString)
		local requestString = requestString or ''
		interface:UICmd("SetActiveRecipe('');")
		Set('_activeShopFilters', '')
		Set('_lastShopSearch', '')
		local gameOptions = GetCurrentGameOptions()
		if string.find(gameOptions, 'gated') then
			if requestString == 'Reborn_Shop_Outpost' then
				Trigger('SetShopDisplay', '', requestString)
				Cmd('SetActiveShop ' .. requestString)
			elseif requestString == 'Reborn_Shop_Observatory' then
				Trigger('SetShopDisplay', '', requestString)
				Cmd('SetActiveShop ' .. requestString)
			else
				Trigger('SetShopDisplay', 'Reborn_' .. requestString)
				Cmd('SetActiveShop ' .. 'Reborn_' .. requestString)
			end
		else
			Trigger('SetShopDisplay', requestString)
			--Cmd('SetActiveShop' .. requestString)
		end
	end
	controller:RegisterWatch('ShopActiveRequest', ShopActiveRequest)
	
	-- ## Menu item update based on search/filters/shops  ##
	local function ShopUpdateSequence(sourceWidget, param0)
		--println('^o ShopUpdateSequence 4')
		GetWidget('purchase_components_popup'):DoEventN(1)		
		if (GetCvarInt('_shopFlag_itemStyle') == 1) then
			UpdateShopItems(ItemV2PanelOnEvent5, false)
			if true or (GetCvarBool('_storeInstantTransition') or (not Shop._shopFlag_useAnims)) then -- RMM Force instant load
				UpdateShopItems(ItemV2SimplePanelOnEvent3, true)
			else
				UpdateShopItems(ItemV2SimplePanelOnEvent4, true)
			end
		else
			UpdateShopItems(ItemV2SimplePanelOnEvent5, true)
			if true or (GetCvarBool('_storeInstantTransition') or (not Shop._shopFlag_useAnims)) then -- RMM Force instant load
				UpdateShopItems(ItemV2PanelOnEvent3, false)
			else
				UpdateShopItems(ItemV2PanelOnEvent4, false)
			end			
		end
		controller:DoEventN(0)
	end
	controller:RegisterWatch('ShopUpdateSequence', ShopUpdateSequence)		
	
	local function SetShopDisplay(sourceWidget, param0, param1)
		--println('^o SetShopDisplay 4')
		Trigger('ShopItemDisplayType', GetCvarInt('_shopFlag_itemStyle'))
		------------------
		
		--Use this if checking Game Mode:
		
		-- local gameModeName = GetCurrentGameModeName()
		-- if gameModeName == 'gated' then	
			-- Set('_lastActiveShop', param1)
		-- else
			-- Set('_lastActiveShop', param0)
		-- end
		
		-- Use this if checking for a Game Option:
		
		local gameOptions = GetCurrentGameOptions()
		local reborn = string.find(gameOptions, 'gated')
		if reborn then
			Set('_lastActiveShop', param1)
		else
			Set('_lastActiveShop', param0)
		end
		
		-- If using neither, use this by itself:
		
		-- Set('_lastActiveShop', param0)
		
		------------------
		Set('_storeInstantTransition', 'false')
		local _lastShopSearch = GetCvarString('_lastShopSearch')
		local _activeShopFilters = GetCvarString('_activeShopFilters')
		local _lastActiveShop = GetCvarString('_lastActiveShop')

		if GetMap() == 'devowars' then
			_lastActiveShop = 'Shop_Devo'
		end
		
		if NotEmpty(_lastShopSearch) then
			interface:UICmd("SetShopFilter('')")
			interface:UICmd("SetActiveShop('')")
			interface:UICmd("SetShopSearch('".._lastShopSearch.."')")
			sourceWidget:DoEventN(1)
			groupfcall('menu_item_group', function(index, widget, groupName) widget:DoEventN(3) end, 'game_shop_v3')
		elseif NotEmpty(_activeShopFilters) then
			interface:UICmd("SetShopSearch('')")
			interface:UICmd("SetActiveShop('')")
			interface:UICmd("SetShopFilter('".._activeShopFilters.."')")
			sourceWidget:DoEventN(1)
		elseif NotEmpty(_lastActiveShop) then
			interface:UICmd("SetShopSearch('')")
			interface:UICmd("SetShopFilter('')")
			interface:UICmd("SetActiveShop('".._lastActiveShop.."')")
			sourceWidget:DoEventN(1)
		else
			interface:UICmd("SetShopSearch('')")
			interface:UICmd("SetShopFilter('')")
			interface:UICmd("SetActiveShop('')")
			Set('_shopNewRecipeTarget', '')
			Set('_recipeTransitionMode', '0')
			Trigger('shopComponentAnimStep0')
			sourceWidget:DoEventN(0)
		end
		
		Set('_useShopScroll', 'false')	
		GetWidget('shopItemScrollbar'):UICmd("SetValue(6)")
	end
	controller:RegisterWatch('SetShopDisplay', SetShopDisplay)	
	
	controller.onevent0 = function() 
		--println('^o controller.onevent0 4')
		local _lastShopSearch = GetCvarString('_lastShopSearch')
		local _activeShopFilters = GetCvarString('_activeShopFilters')
		local _lastActiveShop = GetCvarString('_lastActiveShop')		
		local _shopVisShowEventID, _shopVisHideEventID, _shopFlag_showItems, _shopFlag_guideOpen, _shopFlag_recipeVisible, _shopFlag_showScroll, _lastCurrentShopItems, _shopFlag_sortType = 0, 1, false, false, false, false, 0, 0
		_lastCurrentShopItems = GetCvarInt('_lastCurrentShopItems')

		GetWidget('simpleViewCheckbox'):DoEventN(0)
		Set('_shopFlag_recipeVisible', NotEmpty(GetActiveRecipe()))
		_shopFlag_recipeVisible = GetCvarBool('_shopFlag_recipeVisible')
		
		if Empty(_activeShopFilters) then
			if  NotEmpty(_lastShopSearch) then
				groupfcall('menu_item_group', function(index, widget, groupName) widget:DoEventN(3) end, 'game_shop_v3')
			else
				groupfcall('menu_item_group', function(index, widget, groupName) widget:DoEvent() end, 'game_shop_v3')
			end
		end
		
		if Shop._shopFlag_useAnims and GetCvarBool('_shopFlag_visible') then
			_shopVisShowEventID = 0
			_shopVisHideEventID = 1
		else
			_shopVisShowEventID = 2
			_shopVisHideEventID = 3	
		end
		
		local gameOptions = GetCurrentGameOptions()
		local reborn = string.find(gameOptions, 'gated')
		
		if NotEmpty(_lastActiveShop) or NotEmpty(_activeShopFilters) or NotEmpty(_lastShopSearch) then
			_shopFlag_showItems = true
			Set('_shopFlag_guideOpen', 'false')
		else
			_shopFlag_showItems = false
			if GetCvarBool('_shopFlag_guideOnEmpty') then
				Set('_shopFlag_guideOpen', 'true')
			end
		end	
		_shopFlag_guideOpen = GetCvarBool('_shopFlag_guideOpen')

		if reborn then
			Set('guide_usingLocal', 'true')
			GetWidget('guidelist_combobox'):SetVisible(false)
			GetWidget('guide_remote_operation'):SetVisible(false)
		else
			Set('guide_usingLocal', 'false')
			GetWidget('guidelist_combobox'):SetVisible(true)
			GetWidget('guide_remote_operation'):SetVisible(true)
		end

		if GetMap() == 'devowars' then
			_shopFlag_guideOpen = false
			_shopFlag_showItems = true
		end
		
		if GetCvarInt('_shopFlag_itemStyle') == 1 then
			Shop._shopFlag_maxViewableItems = Shop._shopFlag_maxViewable1
		else
			Shop._shopFlag_maxViewableItems = Shop._shopFlag_maxViewable0
		end
		
		if (_lastCurrentShopItems > Shop._shopFlag_maxViewableItems) and (_shopFlag_showItems) then
			Set('_shopFlag_showScroll', 'true')
		else
			Set('_shopFlag_showScroll', 'false')
		end	
		_shopFlag_showScroll = GetCvarBool('_shopFlag_showScroll')
		
		-- ## Take action based on all flags ##
		
		local function storeVisiblityEvent(targetWidget, firstCondition, secondCondition) 
			if (targetWidget) then
				if (firstCondition) or (secondCondition) then
					targetWidget:DoEventN(_shopVisShowEventID)
				else
					targetWidget:DoEventN(_shopVisHideEventID)
				end
			else
				println('^r storeVisiblityEvent: Failed to find ' .. tostring(targetWidget))
			end
		end
		
		storeVisiblityEvent(GetWidget('header_store'), _shopFlag_showItems)
		storeVisiblityEvent(GetWidget('shop_v4_itemList'), _shopFlag_showItems)
		storeVisiblityEvent(GetWidget('shopSimpleViewCheckbox'), _shopFlag_showItems)
		
		if (_shopFlag_showItems) or (_shopFlag_guideOpen) then
			GetWidget('newStore'):DoEventN(4)
		else
			GetWidget('newStore'):DoEventN(5)
		end		
		
		storeVisiblityEvent(GetWidget('shop_right'), _shopFlag_showItems, _shopFlag_guideOpen)
		local viewHeroName = interface:UICmd("GetViewHeroName()")
		storeVisiblityEvent(GetWidget('hero_guide'), (_shopFlag_guideOpen and NotEmpty(viewHeroName) and (not _shopFlag_showItems) ) )
		
		if ((_shopFlag_guideOpen) and (not _shopFlag_showItems) and NotEmpty(viewHeroName) ) then
			GetWidget('hero_guide_button'):SetButtonState(1)
		else
			GetWidget('hero_guide_button'):SetButtonState(0)
		end
		
		storeVisiblityEvent(GetWidget('header_guide'), (_shopFlag_guideOpen and NotEmpty(viewHeroName) ))
		
		if (_shopFlag_guideOpen) then
			GetWidget('heroGuideErrorDisplay'):DoEventN(0)
		else
			GetWidget('heroGuideErrorDisplay'):DoEventN(1)
		end
		
		storeVisiblityEvent(GetWidget('guide_lower_area'), ((not _shopFlag_recipeVisible) and _shopFlag_guideOpen))
		storeVisiblityEvent(GetWidget('shopRecipeTree'), _shopFlag_recipeVisible)
		
		storeVisiblityEvent(GetWidget('shop_scroll_backer'), _shopFlag_showScroll)
		storeVisiblityEvent(GetWidget('shopItemScrollbar'), _shopFlag_showScroll)

		if (_shopFlag_showScroll) then
			GetWidget('shopScrollCatcher'):DoEventN(0)
			GetWidget('shopItemScrollbar'):DoEventN(6)
		else
			GetWidget('shopScrollCatcher'):DoEventN(1)
		end		
		
		storeVisiblityEvent(GetWidget('shop_popup_noitemsfound'), (_lastCurrentShopItems == 0))
		
		_shopFlag_sortType = GetCvarInt('_shopFlag_sortType')
		storeVisiblityEvent(GetWidget('catlist_name'), (_shopFlag_sortType == 0))
		storeVisiblityEvent(GetWidget('catlist_filter'), (_shopFlag_sortType == 1))
		
		if Shop._shopFlag_useAnims then
			_shopVisShowEventID = 0
			_shopVisHideEventID = 1
		else
			_shopVisShowEventID = 2
			_shopVisHideEventID = 3	
		end		
		
		storeVisiblityEvent(GetWidget('newStore'), (GetCvarBool('_shopFlag_visible')))
		Set('_useShopScroll', 'true')
		
	end

	controller.onevent1 = function() 
		--println('^o controller.onevent1 4')
		--## Anything that changes what items could be displayed in the shop while actually displaying items should trigger these ##			
		Set('_shopScrollMinID', '0')
		if GetCvarInt('_shopFlag_itemStyle') == 1 then
			Set('_shopScrollMaxID', (Shop._shopFlag_maxViewable1 - 1))
		else
			Set('_shopScrollMaxID', (Shop._shopFlag_maxViewable0 - 1))
		end
		interface:UICmd("SetShopItemRange(_shopScrollMinID, _shopScrollMaxID)")
	end

	controller:RefreshCallbacks()	
end

function Shop:InitializeShop(sourceWidget)
	if GetCvarBool('cg_useNewShop2') then
		--println('^g Shop:InitializeShop')
		InitializeShopRecipeItem(sourceWidget)
		InitializeShopItemTriggers(sourceWidget)
		InitializeShopController(sourceWidget)
		InitializeShopAnimations(sourceWidget)

		if HoN_Region.regionTable[HoN_Region.activeRegion].guideDetails then
			GetWidget('guide_vote_thumbs_up_button'):SetVisible(true)
			GetWidget('guide_vote_thumbs_down_button'):SetVisible(true)
			GetWidget('game_guides_popularity'):SetVisible(true)
		else
			GetWidget('guide_vote_thumbs_up_button'):SetVisible(false)
			GetWidget('guide_vote_thumbs_down_button'):SetVisible(false)
			GetWidget('game_guides_popularity'):SetVisible(false)
		end
	end
	
	--InitializeShopRecipeItem = nil
	--InitializeShopItemTriggers = nil
	--InitializeShopController = nil
	--InitializeShopAnimations = nil
	
	--self.InitializeShop = nil
end

-- Call from UI
function interface:ShopF(func, ...)
	if (Shop[func]) then
		print(Shop[func](self, ...))
	else
		print('ShopF failed to find: ' .. tostring(func) .. '\n')
	end	
end	

-- Tutorial Shop Catagory Enable Handler
function Shop:CatagoryEnable(self, param, shopname, filterName)
	local mapName = (param)
	local shopCatList = {
		'Shop_Relics',
		'Shop_Recommended',
		'Shop_Supplies',
		'Shop_Accessories',
		'Shop_Weapons',
		'Shop_Secret',
		'Shop_Recipes1',
		'Shop_Recipes2',
		'Shop_Recipes3',
		'Shop_Recipes4',
		'Shop_Recipes5',
		'Shop_Outpost'
	}
	
	if mapName ~= 'caldavar_tutorial' then
		self:SetEnabled(1)
		for i=1, #shopCatList do
			GetWidget('menu_button_highlight_' .. shopCatList[i]):SetVisible(0)
		end
	end
	
	if mapName == 'caldavar_tutorial' then
		for i=1, #shopCatList do
			if (GetCvarInt('tut_shopcat_' .. shopCatList[i]) == 1) then
				GetWidget('menu_button_' .. shopCatList[i]):SetEnabled(0)
				GetWidget('menu_button_highlight_' .. shopCatList[i]):SetVisible(0)
			elseif (GetCvarInt('tut_shopcat_' .. shopCatList[i]) == 2) then
				GetWidget('menu_button_' .. shopCatList[i]):SetEnabled(1)
				GetWidget('menu_button_highlight_' .. shopCatList[i]):SetVisible(1)
			else
				GetWidget('menu_button_' .. shopCatList[i]):SetEnabled(1)
				GetWidget('menu_button_highlight_' .. shopCatList[i]):SetVisible(0)
			end
		end
	end
	
	if mapName == 'devowars' then
		self:SetEnabled(0)
	end

	--self:SetEnabled(param0 ~= 'devowars' or {devoButton=false})

end

function Shop:AdjustBgWidth(widgetName)
	if widgetName == 'shop_v4_itemList' then
		GetWidget('shop_body_background'):SetWidth('142.5h')
		GetWidget('shop_body_background'):SetX('-0.3h')
	elseif widgetName == 'hero_guide' then
		GetWidget('shop_body_background'):SetWidth('107.8h')
		GetWidget('shop_body_background'):SetX('0')
	end
end

function Shop:OnHeroDefName(_, heroName)
	local gameOptions = GetCurrentGameOptions()
	local reborn = string.find(gameOptions, 'gated')
	if reborn then
		local text = GetHeroLocalGuide(heroName)
		if Empty(text) then return end

		local guideInfo = explode('`', text)
		
		GetWidget('guide_content'):ClearBufferText()
		GetWidget('guide_content'):AddBufferText(guideInfo[12])

		interface:UICmd("ExplodeTrigger('GuideInfoFormResultExplodeItemsStart', EscapeString('"..guideInfo[7].."'), '|');")
		interface:UICmd("ExplodeTrigger('GuideInfoFormResultExplodeItemsLaning', EscapeString('"..guideInfo[8].."'), '|');")
		interface:UICmd("ExplodeTrigger('GuideInfoFormResultExplodeItemsCore', EscapeString('"..guideInfo[9].."'), '|');")
		interface:UICmd("ExplodeTrigger('GuideInfoFormResultExplodeItemsLux', EscapeString('"..guideInfo[10].."'), '|');")
		interface:UICmd("ExplodeTrigger('GuideInfoFormResultExplodeAbilities', EscapeString('"..guideInfo[11].."'), '|');")

		Set('guide_autolevel_abilities', guideInfo[11])
	end
end

interface:RegisterWatch('HeroDefName', function(...) Shop:OnHeroDefName(...) end)