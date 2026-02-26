---------------------------------------------------------- 
--	Name: 		Radial Menu	            				--				
--  Copyright 2015 Frostburn Studios					--
----------------------------------------------------------
local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface = object
local interfaceName = interface:GetName()

------ Menus ------
-- BL = Bottom Left
-- TL = Top Left
-- TR = Top Right
-- BR = Bottom Right
-- C = Center
--
-- Number subCommands 1 - 3
-- Skipping a number, like using 1 and 3, but not 2 is valid and will work
-- Do not use subCommands on the Center item
--
-- Not setting a command value for a menu item will cause it to gracefully close
--
-- Descriptions and icons can be functions that return a string, encase they need to change based on context, if they are
-- a string, it will be directly used.
--
-- If param4 of the menu open trigger equals the 'disable' string as part of a menu item, it will cause it to render
-- grayscale and not be selectable. For example, this is used to disable the lane top/mid/bottom missing calls when you are in said lane.
-- 'disable' can be a function or a string, if it's a function it will compare against the return- for context sensativity
--
-- 'prerequisites' determines if a menu item should be included, if it's not defined it will be included no matter what
-- otherwise prerequisites must be a function that evaluate to true for the item to be included, for example including lanes in the missing call.
--
-- The minimap, currently, pulls TL, TR, BR to populate it's 3 items, and doesn't support sub items.

-- unused (for now?)
local allyHeroCommandMenu = {}

-- single commands, won't be showing menus
local enemyBuildingCommandMenu = {
								["C"] =		{	["icon"] = "/ui/common/radial_menu/dial_attackhere.tga",
												["desc"] = "radial_attack",
												["command"] = function() SendGamePing("building_attack") end
											}
								}

local allyBuildingCommandMenu = {
								["C"] =		{	["icon"] = "/ui/common/radial_menu/dial_defendstructure.tga",
												["desc"] = "radial_defend",
												["command"] = function() SendGamePing("building_defend") end
											}
								}
								
local ctfPickupEnemyFlag = {
								["C"] =		{	["icon"] = "/ui/common/radial_menu/dial_attackhere.tga",
												["desc"] = "radial_attack",
												["command"] = function() SendGamePing("ctf_pick_up_enemy_flag") end
											}
								}
								
local ctfDefendAllyFlag = {
								["C"] =		{	["icon"] = "/ui/common/radial_menu/dial_defendstructure.tga",
												["desc"] = "radial_defend",
												["command"] = function() SendGamePing("ctf_defend_ally_flag") end
											}
								}

-- full menus
local enemyHeroCommandMenu = {
								["BL"] =	{ 	["icon"] = "/ui/common/radial_menu/dial_cancel.tga",
												["desc"] = "general_cancel"
											},
								["TL"] =	{	["icon"] = "/ui/common/radial_menu/dial_getback.tga",
												["desc"] = "radial_getBack",
												["command"] = function() SendGamePing("hero_get_back") end
											},
								["TR"] =	{	["icon"] = "/ui/common/radial_menu/dial_heromissing.tga",
												["desc"] = "radial_heroMissing",
												["command"] = function() SendGamePing("hero_missing") end,

												["subCommands"] = 	{	[1] = 	{	["icon"] = "/ui/common/radial_menu/dial_gotop.tga",
																					["desc"] = "radial_missingTop",
																					["command"] = function() SendGamePing("hero_missing_going_top") end,
																					["disable"] = "top",
																					["prerequisites"] = function() return HoN_RadialMenu.activeLanes["top"] end
																				},
																		[2] = 	{	["icon"] = "/ui/common/radial_menu/dial_gomid.tga",
																					["desc"] = "radial_missingMid",
																					["command"] = function() SendGamePing("hero_missing_going_mid") end,
																					["disable"] = "middle",
																					["prerequisites"] = function() return HoN_RadialMenu.activeLanes["mid"] end
																				},
																		[3] = 	{	["icon"] = "/ui/common/radial_menu/dial_gobottom.tga",
																					["desc"] = "radial_missingBottom",
																					["command"] = function() SendGamePing("hero_missing_going_bot") end,
																					["disable"] = "bottom",
																					["prerequisites"] = function() return HoN_RadialMenu.activeLanes["bot"] end
																				}
																	}
											},
								["BR"] = 	{	["icon"] = "/ui/common/radial_menu/dial_gank.tga",
												["desc"] = "radial_gank",
												["command"] = function() SendGamePing("hero_coming_to_gank") end
											},
								["C"] =		{	["icon"] = "/ui/common/radial_menu/dial_attackhere.tga",
												["desc"] = "radial_attack",
												["command"] = function() SendGamePing("hero_attack") end
											}
	
							}

local genericCommandMenu = 	{
								["BL"] =	{ 	["icon"] = "/ui/common/radial_menu/dial_cancel.tga",
												["desc"] = "general_cancel",
											},
								["TL"] =	{	["icon"] = "/ui/common/radial_menu/dial_getback.tga",
												["desc"] = "radial_getBack",
												["command"] = function() SendGamePing("ground_get_back") end,

												["subCommands"] = 	{	[1] = 	{	["icon"] = "/ui/common/radial_menu/dial_dangerhere.tga",
																					["desc"] = "radial_danger",
																					["command"] = function() SendGamePing("ground_danger_here") end
																				},
																		[2] = 	{	["icon"] = "/ui/common/radial_menu/dial_helphere.tga",
																					["desc"] = "radial_help",
																					["command"] = function() SendGamePing("ground_need_help_here") end
																				}
																	}
											},
								["TR"] =	{	["icon"] = "/ui/common/radial_menu/dial_heromissing.tga",
												["desc"] = "radial_heroMissing",
												["command"] = function() SendGamePing("ground_hero_missing") end,

												["subCommands"] = 	{	[1] = 	{	["icon"] = "/ui/common/radial_menu/dial_gotop.tga",
																					["desc"] = "radial_missingTop",
																					["command"] = function() SendGamePing("ground_hero_missing_going_top") end,
																					["disable"] = "top",
																					["prerequisites"] = function() return HoN_RadialMenu.activeLanes["top"] end
																				},
																		[2] = 	{	["icon"] = "/ui/common/radial_menu/dial_gomid.tga",
																					["desc"] = "radial_missingMid",
																					["command"] = function() SendGamePing("ground_hero_missing_going_mid") end,
																					["disable"] = "middle",
																					["prerequisites"] = function() return HoN_RadialMenu.activeLanes["mid"] end
																				},
																		[3] = 	{	["icon"] = "/ui/common/radial_menu/dial_gobottom.tga",
																					["desc"] = "radial_missingBottom",
																					["command"] = function() SendGamePing("ground_hero_missing_going_bot") end,
																					["disable"] = "bottom",
																					["prerequisites"] = function() return HoN_RadialMenu.activeLanes["bot"] end
																				}
																	}
											},
								["BR"] = 	{	["icon"] = "/ui/common/radial_menu/dial_attackhere.tga",
												["desc"] = "radial_fightHere",
												["command"] = function() SendGamePing("ground_fight_here") end,

												["subCommands"] = 	{	[1] = 	{	["icon"] = "/ui/common/radial_menu/dial_helphere.tga",
																					["desc"] = "radial_comingToHelp",
																					["command"] = function() SendGamePing("ground_coming_to_help") end
																				}
																	}
											},
								["C"] =		{	["icon"] = "/ui/common/radial_menu/dial_ping.tga",
												["desc"] = "general_ping",
												["command"] = function() SendGamePing("ground_ping") end
											}
	
							}

-- gets string values from parameters, whether they are a function or string
local function GetString(val)
	if (val) then
		local sType = type(val)

		if (sType == "string") then
			return val
		elseif (sType == "function") then
			return val()
		else
			return tostring(val)
		end
	end

	return ""
end

-- make sure values exist and are valid (either functions or strings)
local function ValidString(val)
	local str = GetString(val)

	return NotEmpty(str)
end
-------------------

HoN_RadialMenu = {}
HoN_RadialMenu.currentHover = nil
HoN_RadialMenu.currentHoverSub = nil
HoN_RadialMenu.menuItems = nil
HoN_RadialMenu.subItemsDisplayed = ""
HoN_RadialMenu.menuOpen = false
HoN_RadialMenu.minimapMenu = false

HoN_RadialMenu.activeLanes = {}

function HoN_RadialMenu:ExecuteItem(itemID)
	if (HoN_RadialMenu.menuItems[itemID].command) then
		HoN_RadialMenu.menuItems[itemID].command()
	end
end

function HoN_RadialMenu:ClickItem()
	HoN_RadialMenu:ExecuteAndCloseMenu()	-- this will cause the currently hovered item to be executed
end

function HoN_RadialMenu:RightClickItem()
	-- set the selection to BL since it's always cancel
	HoN_RadialMenu:MenuClose()
end

function HoN_RadialMenu:MouseOverItem(itemID)
	if (not HoN_RadialMenu.minimapMenu) then
		GetWidget("radial_command_description", "game"):SetText(Translate(GetString(HoN_RadialMenu.menuItems[itemID].desc)))
	else
		GetWidget("radial_command_minimap_description", "game"):SetText(Translate(GetString(HoN_RadialMenu.menuItems[itemID].desc)))
		GetWidget("radial_minimap_tip", "game"):SetVisible(1)
	end
	HoN_RadialMenu.currentHover = itemID
	HoN_RadialMenu.currentHoverSub = nil

	-- hide last sub items
	if (not HoN_RadialMenu.minimapMenu) then -- sub icons only for non-minimap version
		if (HoN_RadialMenu.subItemsDisplayed and NotEmpty(HoN_RadialMenu.subItemsDisplayed)) then
			for i=1,3 do
				GetWidget("radial_button_sub_"..HoN_RadialMenu.subItemsDisplayed..i, "game"):FadeOut(75)
			end
		end

		-- check for and display subicons
		if (HoN_RadialMenu.menuItems[itemID].subCommands) then
			for i=1,3 do
				if (HoN_RadialMenu.menuItems[itemID].subCommands[i] and ValidString(HoN_RadialMenu.menuItems[itemID].subCommands[i].icon) and ((not HoN_RadialMenu.menuItems[itemID].subCommands[i].prerequisites) or HoN_RadialMenu.menuItems[itemID].subCommands[i].prerequisites())) then
					GetWidget("radial_button_sub_"..itemID..i, "game"):FadeIn(75)
				else
					GetWidget("radial_button_sub_"..itemID..i, "game"):SetVisible(0)
				end
			end
			HoN_RadialMenu.subItemsDisplayed = itemID
		else
			HoN_RadialMenu.subItemsDisplayed = ""
		end
	end
end

function HoN_RadialMenu:MouseOverSubItem(parent, id)
	id = tonumber(id)

	HoN_RadialMenu.currentHoverSub = id
	HoN_RadialMenu.currentHover = parent

	GetWidget("radial_command_description", "game"):SetText(Translate(GetString(HoN_RadialMenu.menuItems[parent].subCommands[id].desc)))
end

function HoN_RadialMenu:ExecuteSubItem(parent, id)
	id = tonumber(id)
	if (HoN_RadialMenu.menuItems[parent].subCommands[id].command) then
		HoN_RadialMenu.menuItems[parent].subCommands[id].command()
	end
end

function HoN_RadialMenu:MouseOutItem()
	if (not HoN_RadialMenu.minimapMenu) then
		GetWidget("radial_command_description", "game"):SetText("")
	else
		GetWidget("radial_minimap_tip", "game"):SetVisible(0)
		GetWidget("radial_command_minimap_description", "game"):SetText(" ") -- space instead of empty so it doesn't break the fitx/growing
	end
	HoN_RadialMenu.currentHover = ""
	HoN_RadialMenu.currentHoverSub = nil
end

-- 0 = ground
-- 1 = Ally building
-- 2 = enemy building
-- 3 = ally unit (unused)
-- 4 = enemy unit
function HoN_RadialMenu:MenuOpen(menuType, mouseX, mouseY, minimapPing, compareString, lanes)
	-- if you are poking around looking to enable this on other maps, they won't work correctly. We don't have
	-- them painted so we don't know what 'areas' we are pinging. Also, we need support for left/right lanes and
	-- such. It will get done in the future though, fear not.

	-- if a menu is already open, close it (important for menus with only one item that just run the command, or empty menus)
	if (HoN_RadialMenu.menuOpen) then
		HoN_RadialMenu:MenuClose()
	end

	-- update lanes
	HoN_RadialMenu:UpdateLanes(tonumber(lanes))

	minimapPing = AtoB(minimapPing)
	-- if (AtoB(minimapPing)) then
	-- 	SendGamePing("ground_ping")
	-- 	return
	-- end
	-- moved down to the numItems check to run the center command for now

	local menus = {	[0] = genericCommandMenu,
					[1] = allyBuildingCommandMenu,
					[2] = enemyBuildingCommandMenu,
					[3] = allyHeroCommandMenu,
					[4] = enemyHeroCommandMenu,
					[5] = ctfPickupEnemyFlag,
					[6] = ctfDefendAllyFlag
				  }

	if (menus[tonumber(menuType)]) then
		HoN_RadialMenu.menuItems = menus[tonumber(menuType)]

		local numItems = 0
		for k,v in pairs(HoN_RadialMenu.menuItems) do
			numItems = numItems + 1
		end

		-- empty table, we know it's a thing but there's nothing to do
		if (numItems == 0) then
			return
		-- comment this block here out to show menus with only one item instead of automatically running the command
		---[[
		elseif (numItems == 1) then -- one item, just do the command (if it's center)
			if (HoN_RadialMenu.menuItems["C"] and HoN_RadialMenu.menuItems["C"].command) then
				HoN_RadialMenu.menuItems["C"].command()
			end

			return
		end
		--]]
	else
		printdb("^rRadial Menu Open #"..menuType.." | Unknown menu index")
		return
	end

	if (not minimapPing) then -- normal
		HoN_RadialMenu.minimapMenu = false
		HoN_RadialMenu:PopulateMenu(compareString)

		-- position and set visible
		local masterWidget = GetWidget("radial_menu_master", "game")

		local mouseX, mouseY = tonumber(mouseX), tonumber(mouseY)
		local menuWidth, menuHeight = masterWidget:GetWidth(), masterWidget:GetHeight()
		local screenWidth, screenHeight = GetScreenWidth(), GetScreenHeight()
		local posX, posY = (mouseX - (menuWidth / 2)), (mouseY - (menuHeight / 2))

		-- clamp the position to stay completely on screen
		posX = math.max(posX, 0) -- left side
		posX = math.min(posX, screenWidth - menuWidth) -- right side
		posY = math.max(posY, 0) -- top
		posY = math.min(posY, screenHeight - menuHeight) -- bottom

		masterWidget:SetX(posX)
		masterWidget:SetY(posY)
		masterWidget:BringToFront()
		masterWidget:FadeIn(75)
	else -- minimap
		HoN_RadialMenu.minimapMenu = true
		HoN_RadialMenu:PopulateMinimap(compareString)

		local masterWidget = GetWidget("radial_minimap", "game")
		if (GetCvarBool('ui_minimap_rightside')) then
			masterWidget:SetAlign('right')
		else
			masterWidget:SetAlign('left')
		end

		masterWidget:FadeIn(75)
	end

	HoN_RadialMenu.menuOpen = true
end
interface:RegisterWatch("UIPingOpen", function(_, ...) HoN_RadialMenu:MenuOpen(...) end)

function HoN_RadialMenu:ExecuteAndCloseMenu()
	-- don't trigger commands when close is fired if the menu was never open
	if (not HoN_RadialMenu.menuOpen) then
		return
	end

	if (HoN_RadialMenu.currentHover and NotEmpty(HoN_RadialMenu.currentHover)) then
		if (HoN_RadialMenu.currentHoverSub) then
			HoN_RadialMenu:ExecuteSubItem(HoN_RadialMenu.currentHover, HoN_RadialMenu.currentHoverSub)
		else
			HoN_RadialMenu:ExecuteItem(HoN_RadialMenu.currentHover)
		end
	else
		HoN_RadialMenu:ExecuteItem("C") -- execute the center/default
	end

	HoN_RadialMenu:MenuClose()
end
interface:RegisterWatch("UIPingClose", function(_, sentPing) if (not AtoB(sentPing)) then HoN_RadialMenu:ExecuteAndCloseMenu() else HoN_RadialMenu:MenuClose() end end)

function HoN_RadialMenu:MenuClose()
	-- don't trigger commands when close is fired if the menu was never open
	if (not HoN_RadialMenu.menuOpen) then
		return
	end

	if (not HoN_RadialMenu.minimapMenu) then
		if (HoN_RadialMenu.currentHover and NotEmpty(HoN_RadialMenu.currentHover)) then
			for i=1,3 do
				GetWidget("radial_button_sub_"..HoN_RadialMenu.currentHover..i, "game"):SetVisible(0)
			end
			HoN_RadialMenu.currentHover = ""
			HoN_RadialMenu.currentHoverSub = nil
		end
		-- hide last sub items
		if (HoN_RadialMenu.subItemsDisplayed and NotEmpty(HoN_RadialMenu.subItemsDisplayed)) then
			for i=1,3 do
				GetWidget("radial_button_sub_"..HoN_RadialMenu.subItemsDisplayed..i, "game"):FadeOut(75)
			end
			HoN_RadialMenu.subItemsDisplayed = ""
		end

		GetWidget("radial_menu_master", "game"):FadeOut(75)
	else
		GetWidget("radial_minimap", "game"):FadeOut(75)
	end

	HoN_RadialMenu.menuOpen = false
end

function HoN_RadialMenu:PopulateMenu(compareString)
	local iconNames = {"BL", "TL", "TR", "BR", "C"}

	for _,v in ipairs(iconNames) do
		if (HoN_RadialMenu.menuItems[v] and ValidString(HoN_RadialMenu.menuItems[v].icon) and ((not HoN_RadialMenu.menuItems[v].prerequisites) or HoN_RadialMenu.menuItems[v].prerequisites())) then
			GetWidget("radial_button_"..v, "game"):SetVisible(1)
			GetWidget("radial_button_"..v.."_icon", "game"):SetTexture(GetString(HoN_RadialMenu.menuItems[v].icon))
			if (ValidString(HoN_RadialMenu.menuItems[v].disable) and (GetString(HoN_RadialMenu.menuItems[v].disable) == compareString)) then
				GetWidget("radial_button_"..v.."_icon", "game"):SetRenderMode('grayscale')
				GetWidget("radial_button_"..v "game"):SetNoClick(true)
			else
				GetWidget("radial_button_"..v.."_icon", "game"):SetRenderMode('normal')
				GetWidget("radial_button_"..v, "game"):SetNoClick(false)
			end

			-- sub icons
			if (HoN_RadialMenu.menuItems[v].subCommands) then
				for i=1,3 do
					if (HoN_RadialMenu.menuItems[v].subCommands[i] and ValidString(HoN_RadialMenu.menuItems[v].subCommands[i].icon) and ((not HoN_RadialMenu.menuItems[v].subCommands[i].prerequisites) or HoN_RadialMenu.menuItems[v].subCommands[i].prerequisites())) then
						GetWidget("radial_button_sub_"..v..i.."_icon", "game"):SetTexture(GetString(HoN_RadialMenu.menuItems[v].subCommands[i].icon))
						if (ValidString(HoN_RadialMenu.menuItems[v].subCommands[i].disable) and (GetString(HoN_RadialMenu.menuItems[v].subCommands[i].disable) == compareString)) then
							GetWidget("radial_button_sub_"..v..i.."_icon", "game"):SetRenderMode('grayscale')
							GetWidget("radial_button_sub_"..v..i, "game"):SetNoClick(true)
						else
							GetWidget("radial_button_sub_"..v..i.."_icon", "game"):SetRenderMode('normal')
							GetWidget("radial_button_sub_"..v..i, "game"):SetNoClick(false)
						end
					end
				end
			end
		else
			GetWidget("radial_button_"..v, "game"):SetVisible(0)
		end
	end
end

function HoN_RadialMenu:PopulateMinimap(compareString)
	local iconNames = {"TL", "TR", "BR"}

	local iconsVisible = {}
	for i,v in ipairs(iconNames) do
		if (HoN_RadialMenu.menuItems[v] and ValidString(HoN_RadialMenu.menuItems[v].icon) and ((not HoN_RadialMenu.menuItems[v].prerequisites) or HoN_RadialMenu.menuItems[v].prerequisites())) then
			GetWidget("radial_button_"..v.."_minimap", "game"):SetVisible(1)
			GetWidget("radial_button_"..v.."_icon_minimap", "game"):SetTexture(GetString(HoN_RadialMenu.menuItems[v].icon))
			if (ValidString(HoN_RadialMenu.menuItems[v].disable) and (GetString(HoN_RadialMenu.menuItems[v].disable) == compareString)) then
				GetWidget("radial_button_"..v.."_icon_minimap", "game"):SetRenderMode('grayscale')
				GetWidget("radial_button_"..v.."_minimap", "game"):SetNoClick(true)
			else
				GetWidget("radial_button_"..v.."_icon_minimap", "game"):SetRenderMode('normal')
				GetWidget("radial_button_"..v.."_minimap", "game"):SetNoClick(false)
			end

			iconsVisible[i] = true
		else
			GetWidget("radial_button_"..v.."_minimap", "game"):SetVisible(0)
			iconsVisible[i] = false
		end
	end

	-- set dividers visible
	local lastVisible = nil
	for i,v in ipairs(iconsVisible) do
		-- if we aren't at the last icon (it has no divider after it), set the divider invisible
		-- yes, it may be set visible later, but we don't know that for sure now
		if (i ~= #iconsVisible) then
			GetWidget("radial_minimap_divider"..i, "game"):SetVisible(0)
		end

		-- icon is visible
		if (v) then
			-- there has been an icon visible in the past, set it's divider visible
			if (lastVisible) then
				GetWidget("radial_minimap_divider"..lastVisible, "game"):SetVisible(1)
			end
			-- the new last visible icon is now our current one
			lastVisible = i
		end
	end
end

-- This is so bad :'(
function HoN_RadialMenu:UpdateLanes(numLanes)
	-- set the lanes that should be visible
	HoN_RadialMenu.activeLanes = {}

	if (numLanes == 3) then
		HoN_RadialMenu.activeLanes["top"] = true
		HoN_RadialMenu.activeLanes["mid"] = true
		HoN_RadialMenu.activeLanes["bot"] = true
	elseif (numLanes == 2) then
		HoN_RadialMenu.activeLanes["top"] = true
		HoN_RadialMenu.activeLanes["bot"] = true
	end
	-- midwars has none active, other maps are :\
end

function interface:RadialF(func, ...)
  print(HoN_RadialMenu[func](self, ...))
end