---------------------------------------------------------- 
--	Name: 		Grabbag Script	            			--				
--  Copyright 2015 Frostburn Studios					--
----------------------------------------------------------

local themeHelper = {}	-- store grab bag related theme variables on here (ones that need to exist between onOpen/onAppear/onFinish, etc)
local grabBagThemes = {
	['default'] = {
		possibleModels = {	-- a table of all the possible models that can be randomly selected
			{
				-- model stuff
				bag 		= "/ui/fe2/plinko/models/chests/silver/chest.mdf",
				effect 		= "/ui/fe2/plinko/models/chests/silver/silver_chest.effect",
				animName 	= "open",
				animTime 	= 900,
				animEffect	= "/ui/fe2/plinko/models/chests/silver/silver_chest.effect",
				hoverEffect = "/ui/fe2/grabbag/hover_effect.effect",
				shakeName	= "shake",
				shakeTimes	= {3500, 7500},
				camAngles 	= "0 0 0",
				camPos 		= "0 -600 0",

				-- functional stuff
				yClipperPos	= "-6.75h",

				-- animation / effects stuff (unused panels exist for the sole purpose of being used in here)
				onOpenFunc = function(chestNumber, chestSleeper, sharedSleeper)		-- ran when the chest is clicked, chestNumber is the number clicked,
					PlaySound("/ui/fe2/plinko/sounds/chest_appear_1.ogg")			-- chestSleeper is a panel for sleeping on, unique to the chest
					chestSleeper:Sleep(400, function()								-- sharedSleeper is a panel for sleeping on, shared by all the chests
						PlaySound("/ui/fe2/plinko/sounds/tier_3_chest_open.ogg")
					end)
				end,

				onAppearFunc = function(chestNumber, chestSleeper, sharedSleeper)	-- ran when the model appears out of the chest (after an animTime ms sleep- keep this in mind if sleeping on the sleepers)
				end,

				onFinishFunc = function(chestNumber, chestSleeper, sharedSleeper)	-- ran when the model is visible and done sliding (250 ms after the animTime sleep, keep this in mind if sleeping on the sleepers)
				end,

				onShakeFunc = function(chestNumber, chestSleeper, sharedSleeper)	-- ran when the model shakes, this will never interrupt a appear, open, or finish
					chestSleeper:UICmd("Play2DSFXSound('/ui/fe2/plinko/sounds/chest_appear_"..(math.random(1,2))..".ogg', 0.3)")
				end,
			},
		},

		-- theming stuff, these are always present/visible
		overlayImage	= "",
		backgroundImage	= "",

		getChestNumbers = function(numChests) 	-- returns the array of chest numbers to use (that indexes into possibleModels)
			return {							-- you can override this so that specific patterns are used, etc.
				math.random(1, numChests),
				math.random(1, numChests),
				math.random(1, numChests),
				math.random(1, numChests),
				math.random(1, numChests)
			}
		end,

		onChestsVisible	= function()	-- ran when all the chests are visible, after they have faded in
		end,

		onPopulateFunc	= function()	-- ran when the theme is populated, can be used to hide panels, etc in prep for stuff
		end,
	},

	['christmas'] = {
		-- model stuff
		possibleModels = {	-- a table of all the possible models that can be randomly selected
			{
				bag 		= "/ui/fe2/grabbag/christmas/present/present.mdf",
				effect 		= "/ui/fe2/grabbag/christmas/present/present.effect",
				animName 	= "open",
				animTime 	= 900,
				animEffect	= "/ui/fe2/grabbag/christmas/present/present_open.effect",
				hoverEffect = "/ui/fe2/grabbag/hover_effect.effect",
				shakeName	= "shake",
				shakeTimes	= {3500, 7500},
				camPos 		= "0 -700 0",

				-- functional stuff
				yClipperPos	= "-6.25h",

				onOpenFunc = function(chestNumber, chestSleeper, sharedSleeper)	-- ran when the chest is clicked, chestNumber is the number clicked,
					PlaySound("/ui/fe2/plinko/sounds/chest_appear_1.ogg")
					chestSleeper:Sleep(400, function()
						PlaySound("/ui/fe2/grabbag/christmas/grabbag_chests_"..math.random(1,3)..".ogg")
					end)
				end,
			},
			{
				bag 		= "/ui/fe2/grabbag/christmas/present/present_02.mdf",
				effect 		= "/ui/fe2/grabbag/christmas/present/present.effect",
				animName 	= "open",
				animTime 	= 900,
				animEffect	= "/ui/fe2/grabbag/christmas/present/present_open.effect",
				hoverEffect = "/ui/fe2/grabbag/hover_effect.effect",
				shakeName	= "shake",
				shakeTimes	= {3500, 7500},
				camPos 		= "0 -700 0",

				-- functional stuff
				yClipperPos	= "-6.70h",

				onOpenFunc = function(chestNumber, chestSleeper, sharedSleeper)	-- ran when the chest is clicked, chestNumber is the number clicked,
					PlaySound("/ui/fe2/plinko/sounds/chest_appear_1.ogg")
					chestSleeper:Sleep(400, function()
						PlaySound("/ui/fe2/grabbag/christmas/grabbag_chests_"..math.random(1,3)..".ogg")
					end)
				end,
			},
			{
				bag 		= "/ui/fe2/grabbag/christmas/present/present_03.mdf",
				effect 		= "/ui/fe2/grabbag/christmas/present/present.effect",
				animName 	= "open",
				animTime 	= 900,
				animEffect	= "/ui/fe2/grabbag/christmas/present/present_open.effect",
				hoverEffect = "/ui/fe2/grabbag/hover_effect.effect",
				shakeName	= "shake",
				shakeTimes	= {3500, 7500},
				camPos 		= "0 -700 0",

				-- functional stuff
				yClipperPos	= "-6.25h",

				onOpenFunc = function(chestNumber, chestSleeper, sharedSleeper)	-- ran when the chest is clicked, chestNumber is the number clicked,
					PlaySound("/ui/fe2/plinko/sounds/chest_appear_1.ogg")
					chestSleeper:Sleep(400, function()
						PlaySound("/ui/fe2/grabbag/christmas/grabbag_chests_"..math.random(1,3)..".ogg")
					end)
				end,
			},
			{
				bag 		= "/ui/fe2/grabbag/christmas/present/present_04.mdf",
				effect 		= "/ui/fe2/grabbag/christmas/present/present.effect",
				animName 	= "open",
				animTime 	= 900,
				animEffect	= "/ui/fe2/grabbag/christmas/present/present_open.effect",
				hoverEffect = "/ui/fe2/grabbag/hover_effect.effect",
				shakeName	= "shake",
				shakeTimes	= {3500, 7500},
				camPos 		= "0 -700 0",

				-- functional stuff
				yClipperPos	= "-7.40h",

				onOpenFunc = function(chestNumber, chestSleeper, sharedSleeper)	-- ran when the chest is clicked, chestNumber is the number clicked,
					PlaySound("/ui/fe2/plinko/sounds/chest_appear_1.ogg")
					chestSleeper:Sleep(400, function()
						PlaySound("/ui/fe2/grabbag/christmas/grabbag_chests_"..math.random(1,3)..".ogg")
					end)
				end,
			},
			{
				bag 		= "/ui/fe2/grabbag/christmas/present/present_05.mdf",
				effect 		= "/ui/fe2/grabbag/christmas/present/present.effect",
				animName 	= "open",
				animTime 	= 900,
				animEffect	= "/ui/fe2/grabbag/christmas/present/present_open.effect",
				hoverEffect = "/ui/fe2/grabbag/hover_effect.effect",
				shakeName	= "shake",
				shakeTimes	= {3500, 7500},
				camPos 		= "0 -700 0",

				-- functional stuff
				yClipperPos	= "-5.80h",

				onOpenFunc = function(chestNumber, chestSleeper, sharedSleeper)	-- ran when the chest is clicked, chestNumber is the number clicked,
					PlaySound("/ui/fe2/plinko/sounds/chest_appear_1.ogg")
					chestSleeper:Sleep(400, function()
						PlaySound("/ui/fe2/grabbag/christmas/grabbag_chests_"..math.random(1,3)..".ogg")
					end)
				end,
			},
		},

		onPopulateFunc	= function()	-- ran when the theme is populated, can be used to hide panels, etc in prep for stuff
			-- snowww!!!!
			local themeModelBack = GetWidget("grab_bag_theme_modelpanel_back")
			themeModelBack:SetModel("/ui/effects/mainmenu/right.mdf")
			themeModelBack:SetEffect("/ui/effects/mainmenu/snow_main.effect")
			themeModelBack:SetVisible(1)
		end,

		getChestNumbers = function(numChests) 	-- returns the array of chest numbers to use (that indexes into possibleModels)
			local retArray = {}
			for i=1,numChests do
				table.insert(retArray, i)
			end
			while #retArray < 5 do 	-- if there aren't 5 chests in the array, randomly add exsting chests until there are 5
				table.insert(retArray, math.random(1, numChests))
			end

			-- shuffle the retArray
			local n = #retArray
			while n >= 2 do
				local k = math.random(1, n)
				retArray[n], retArray[k] = retArray[k], retArray[n]
				n = n - 1
			end

			return retArray
		end,
	},

	['esports'] = {
		possibleModels = {	-- a table of all the possible models that can be randomly selected
			{
				-- model stuff
				bag 		= "/ui/fe2/grabbag/esports/footlocker.mdf",
				effect 		= "",
				animName 	= "open",
				animTime 	= 1300,
				animEffect	= "/ui/fe2/grabbag/esports/open.effect",
				hoverEffect = "/ui/fe2/grabbag/esports/hover_effect.effect",
				shakeName	= "shake",
				shakeTimes	= {3500, 7500},
				camAngles 	= "0 0 0",
				camPos 		= "0 -450 0",

				-- functional stuff
				yClipperPos	= "-10.6h",
			},
		},
	},

	['celebrate'] = {
		-- model stuff
		possibleModels = {	-- a table of all the possible models that can be randomly selected
			{
				bag 		= "/ui/fe2/grabbag/christmas/present/present.mdf",
				effect 		= "/ui/fe2/grabbag/christmas/present/present.effect",
				animName 	= "open",
				animTime 	= 900,
				animEffect	= "/ui/fe2/grabbag/christmas/present/present_open.effect",
				hoverEffect = "/ui/fe2/grabbag/hover_effect.effect",
				shakeName	= "shake",
				shakeTimes	= {3500, 7500},
				camPos 		= "0 -700 0",

				-- functional stuff
				yClipperPos	= "-6.25h",

				onOpenFunc = function(chestNumber, chestSleeper, sharedSleeper)	-- ran when the chest is clicked, chestNumber is the number clicked,
					PlaySound("/ui/fe2/plinko/sounds/chest_appear_1.ogg")
					chestSleeper:Sleep(400, function()
						PlaySound("/ui/fe2/plinko/sounds/tier_3_chest_open.ogg")
					end)
				end,
			},
			{
				bag 		= "/ui/fe2/grabbag/christmas/present/present_02.mdf",
				effect 		= "/ui/fe2/grabbag/christmas/present/present.effect",
				animName 	= "open",
				animTime 	= 900,
				animEffect	= "/ui/fe2/grabbag/christmas/present/present_open.effect",
				hoverEffect = "/ui/fe2/grabbag/hover_effect.effect",
				shakeName	= "shake",
				shakeTimes	= {3500, 7500},
				camPos 		= "0 -700 0",

				-- functional stuff
				yClipperPos	= "-6.70h",

				onOpenFunc = function(chestNumber, chestSleeper, sharedSleeper)	-- ran when the chest is clicked, chestNumber is the number clicked,
					PlaySound("/ui/fe2/plinko/sounds/chest_appear_1.ogg")
					chestSleeper:Sleep(400, function()
						PlaySound("/ui/fe2/plinko/sounds/tier_3_chest_open.ogg")
					end)
				end,
			},
			{
				bag 		= "/ui/fe2/grabbag/christmas/present/present_03.mdf",
				effect 		= "/ui/fe2/grabbag/christmas/present/present.effect",
				animName 	= "open",
				animTime 	= 900,
				animEffect	= "/ui/fe2/grabbag/christmas/present/present_open.effect",
				hoverEffect = "/ui/fe2/grabbag/hover_effect.effect",
				shakeName	= "shake",
				shakeTimes	= {3500, 7500},
				camPos 		= "0 -700 0",

				-- functional stuff
				yClipperPos	= "-6.25h",

				onOpenFunc = function(chestNumber, chestSleeper, sharedSleeper)	-- ran when the chest is clicked, chestNumber is the number clicked,
					PlaySound("/ui/fe2/plinko/sounds/chest_appear_1.ogg")
					chestSleeper:Sleep(400, function()
						PlaySound("/ui/fe2/plinko/sounds/tier_3_chest_open.ogg")
					end)
				end,
			},
			{
				bag 		= "/ui/fe2/grabbag/christmas/present/present_04.mdf",
				effect 		= "/ui/fe2/grabbag/christmas/present/present.effect",
				animName 	= "open",
				animTime 	= 900,
				animEffect	= "/ui/fe2/grabbag/christmas/present/present_open.effect",
				hoverEffect = "/ui/fe2/grabbag/hover_effect.effect",
				shakeName	= "shake",
				shakeTimes	= {3500, 7500},
				camPos 		= "0 -700 0",

				-- functional stuff
				yClipperPos	= "-7.40h",

				onOpenFunc = function(chestNumber, chestSleeper, sharedSleeper)	-- ran when the chest is clicked, chestNumber is the number clicked,
					PlaySound("/ui/fe2/plinko/sounds/chest_appear_1.ogg")
					chestSleeper:Sleep(400, function()
						PlaySound("/ui/fe2/plinko/sounds/tier_3_chest_open.ogg")
					end)
				end,
			},
			{
				bag 		= "/ui/fe2/grabbag/christmas/present/present_05.mdf",
				effect 		= "/ui/fe2/grabbag/christmas/present/present.effect",
				animName 	= "open",
				animTime 	= 900,
				animEffect	= "/ui/fe2/grabbag/christmas/present/present_open.effect",
				hoverEffect = "/ui/fe2/grabbag/hover_effect.effect",
				shakeName	= "shake",
				shakeTimes	= {3500, 7500},
				camPos 		= "0 -700 0",

				-- functional stuff
				yClipperPos	= "-5.80h",

				onOpenFunc = function(chestNumber, chestSleeper, sharedSleeper)	-- ran when the chest is clicked, chestNumber is the number clicked,
					PlaySound("/ui/fe2/plinko/sounds/chest_appear_1.ogg")
					chestSleeper:Sleep(400, function()
						PlaySound("/ui/fe2/plinko/sounds/tier_3_chest_open.ogg")
					end)
				end,
			},
		},

		getChestNumbers = function(numChests) 	-- returns the array of chest numbers to use (that indexes into possibleModels)
			local retArray = {}
			for i=1,numChests do
				table.insert(retArray, i)
			end
			while #retArray < 5 do 	-- if there aren't 5 chests in the array, randomly add exsting chests until there are 5
				table.insert(retArray, math.random(1, numChests))
			end

			-- shuffle the retArray
			local n = #retArray
			while n >= 2 do
				local k = math.random(1, n)
				retArray[n], retArray[k] = retArray[k], retArray[n]
				n = n - 1
			end

			return retArray
		end,
	},

	['halloween'] = {
		possibleModels = {	-- a table of all the possible models that can be randomly selected
			{
				-- model stuff
				bag 		= "/ui/fe2/grabbag/halloween/pumpkin.mdf",
				effect 		= "",
				animName 	= "open",
				animTime 	= 1300,
				animEffect	= "/ui/fe2/grabbag/halloween/pumpkin_open.effect",
				hoverEffect = "/ui/fe2/grabbag/halloween/hover_effect.effect",
				shakeName	= "shake",
				shakeTimes	= {3500, 7500},
				camAngles 	= "0 0 0",
				camPos 		= "0 -350 0",

				-- functional stuff
				yClipperPos	= "-10.6h",
			},
		},
	},
}

local _G = getfenv(0)
HoN_Grabbag = _G['HoN_Grabbag'] or {}
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()

------ Grab Bag Stuff -----
local function PopulateGrabBagTheme(themeName)
	local theme = {}
	if (not grabBagThemes[themeName]) then
		themeName = "default"
		theme = grabBagThemes[themeName]
	else
		if (grabBagThemes[themeName]) then
			theme = table.copy(grabBagThemes[themeName])
		end

		-- copy non-existant parts to the theme table
		for k,v in pairs(grabBagThemes["default"]) do
			if (theme[k] == nil) then
				theme[k] = v
			end
		end

		-- copy non-existant chest elements to the theme table
		for k,v in pairs(grabBagThemes["default"].possibleModels[1]) do
			for i=1,#(theme.possibleModels) do
				if (theme.possibleModels[i][k] == nil) then
					theme.possibleModels[i][k] = v
				end
			end
		end
	end

	HoN_Grabbag.grabBagTheme = theme

	-- pick the random chests to use, just generate 5, whatever isn't used will just be ignored
	HoN_Grabbag.grabBagRandomChests = theme.getChestNumbers(#(theme.possibleModels))

	-- set any of the models panels, etc. invisible encase a different theme used them, the pop function used next might reset them visible
	GetWidget("grab_bag_theme_modelpanel_back"):SetVisible(0)
	GetWidget("grab_bag_theme_modelpanel_fore"):SetVisible(0)

	if (theme.onPopulateFunc) then
		theme.onPopulateFunc()
	end

	for i=1,5 do
		local model = GetWidget("grab_bag_prize_chest_"..i.."_model")
		model:UICmd("SetCameraPos('"..theme.possibleModels[HoN_Grabbag.grabBagRandomChests[i]].camPos.."');")
		model:UICmd("SetCameraAngles('"..theme.possibleModels[HoN_Grabbag.grabBagRandomChests[i]].camAngles.."');")

		model:SetModel(theme.possibleModels[HoN_Grabbag.grabBagRandomChests[i]].bag)
		model:SetAnim("idle")
		model:UICmd("SetEffectIndexed('"..theme.possibleModels[HoN_Grabbag.grabBagRandomChests[i]].effect.."', 0);")

		GetWidget("grab_bag_prize_"..i.."_clipper"):SetY(theme.possibleModels[HoN_Grabbag.grabBagRandomChests[i]].yClipperPos)
	end

	if NotEmpty(theme.overlayImage) then
		GetWidget("grab_bag_theme_fore"):SetTexture(theme.overlayImage)
		GetWidget("grab_bag_theme_fore"):SetVisible(1)
	else
		GetWidget("grab_bag_theme_fore"):SetVisible(0)
	end
	if NotEmpty(theme.backgroundImage) then
		GetWidget("grab_bag_theme_back"):SetTexture(theme.backgroundImage)
		GetWidget("grab_bag_theme_back"):SetVisible(1)
	else
		GetWidget("grab_bag_theme_back"):SetTexture("$invis")
		GetWidget("grab_bag_theme_back"):SetVisible(0)
	end
end

local function PopulateGrabBagPrizes(prizeTable)
	for i=1,5 do
		if (i > prizeTable.count) then
			GetWidget("grab_bag_prize_"..i.."_master"):SetVisible(0)
		else
			GetWidget("grab_bag_prize_"..i.."_master"):SetVisible(1)
			GetWidget("grab_bag_prize_"..i):SetVisible(0)
			GetWidget("grab_bag_prize_"..i.."_slider"):SetY("150%")

			local prize = prizeTable[i]

			-- panels for generic assignments
			local typeLabel, nameLabel, iconPanel
			prize.model = ((prize.type == "Alt Avatar") or (prize.type == "Couriers") or (prize.type == "Hero") or (prize.type == "EAP"))
			if (prize.model) then
				-- load stuff
				if (not GetCvarBool("_entityDefinitionsLoaded")) then
					if (prize.type == "Couriers") then
						interface:UICmd("LoadEntityDefinitionsFromFolder('/items/basic/ground_familiar/')")
					else
						interface:UICmd("LoadEntityDefinitionsFromFolder('"..string.sub(prize.path, 1, string.find(prize.path, "/", 9)).."')")
					end
				end

				-- get the widgets
				local modelPanel = GetWidget("grab_bag_prize_model_"..i.."_model")
				typeLabel = GetWidget("grab_bag_prize_model_"..i.."_type")
				nameLabel = GetWidget("grab_bag_prize_model_"..i.."_name")
				iconPanel = GetWidget("grab_bag_prize_model_"..i.."_icon")

				-- setup the model
				if (not (prize.type == "Couriers")) then
					-- get the raw hero_name for special case effect paths
					local heroName = explode(".", prize.name)

					modelPanel:UICmd("SetCameraPos('0 6000 75');")
					modelPanel:UICmd("SetModelPos(GetHeroStorePosFromProduct('"..prize.name.."'));")
					modelPanel:UICmd("SetModelAngles(GetHeroStoreAnglesFromProduct('"..prize.name.."'));")
					modelPanel:UICmd("SetModelScale(GetHeroStoreScaleFromProduct('"..prize.name.."'));")
					modelPanel:UICmd("SetModel(GetHeroPreviewModelPathFromProduct('"..prize.name.."'));")
					if (heroName[1] ~= "Hero_Empath" and heroName[1] ~= "Hero_Gemini" and heroName[1] ~= "Hero_ShadowBlade" and heroName[1] ~= "Hero_Dampeer") then
						modelPanel:UICmd("SetEffectIndexed(GetHeroStorePassiveEffectPathFromProduct('"..prize.name.."'), 0);")
					else
						modelPanel:UICmd("SetEffectIndexed(GetHeroPassiveEffectPathFromProduct('"..prize.name.."'), 0);")
					end
					-- glow effect
					modelPanel:UICmd("SetEffectIndexed('/shared/effects/glow_gold_high.effect', 1);")
					modelPanel:UICmd("SetTeamColor('1 0 0');")
				else -- couriers don't have store stuff set
					modelPanel:UICmd("SetCameraPos('0 6000 75');")
					modelPanel:UICmd("SetModelPos(GetHeroPreviewPosFromProduct('"..prize.name.."'));")
					modelPanel:UICmd("SetModelAngles(GetHeroPreviewAnglesFromProduct('"..prize.name.."'));")
					modelPanel:UICmd("SetModelScale(GetHeroPreviewScaleFromProduct('"..prize.name.."'));")
					modelPanel:UICmd("SetModel(GetHeroPreviewModelPathFromProduct('"..prize.name.."'));")
					modelPanel:UICmd("SetEffectIndexed(GetHeroPassiveEffectPathFromProduct('"..prize.name.."'), 0);")
					-- glow effect
					modelPanel:UICmd("SetEffectIndexed('/shared/effects/glow_gold_high.effect', 1);")
				end
			else 	-- icon
				-- get the widgets
				typeLabel = GetWidget("grab_bag_prize_icon_"..i.."_type")
				nameLabel = GetWidget("grab_bag_prize_icon_"..i.."_name")
				iconPanel = GetWidget("grab_bag_prize_icon_"..i.."_icon")
			end

			-- set the name
			local nameStr = Translate("mstore_product"..prize.productID.."_name")
			nameLabel:SetFont(GetFontSizeForWidth(nameStr, nameLabel:GetWidth(), 14))
			nameLabel:SetText(nameStr)
			if (prize.type == "Chat Color") then
				local upgrade = ProductTypeToPrefix(prize.type)..prize.name
				nameLabel:SetColor(GetChatNameColorStringFromUpgrades(upgrade))
				nameLabel:SetGlow(GetChatNameGlowFromUpgrades(upgrade))
				nameLabel:SetGlowColor(GetChatNameGlowColorStringFromUpgrades(upgrade))
				nameLabel:SetBackgroundGlow(GetChatNameBackgroundGlowFromUpgrades(upgrade))
			else
				nameLabel:SetColor("white")
				nameLabel:SetGlow(false)
			end

			-- set the icon
			iconPanel:SetTexture(prize.path)

			-- set the type label
			if (prize.type == "Couriers") then -- Courier's product type code is aa, so we have to translate them special
				typeLabel:SetText(Translate("general_courier"))
			elseif (prize.type == "Bundle") then
				typeLabel:SetText(Translate("general_bundle"))
			else
				typeLabel:SetText(Translate("general_product_type_"..ProductTypeToPrefix(prize.type, true)))
			end

			-- set stuff visible
			GetWidget("grab_bag_prize_model_"..i):SetVisible(prize.model)
			GetWidget("grab_bag_prize_icon_"..i):SetVisible(not prize.model)
		end
	end
end

local function PresentGrabBagPrizes(prizes)
	local sleeper1 = GetWidget("grab_bag_sleeper_1")
	local sleeper2 = GetWidget("grab_bag_sleeper_2")

	GetWidget("grabBag"):FadeIn(150)
	sleeper1:Sleep(150, function()

		-- present each prize
		local prize = 0
		local presentNextPrize = function(func)
			prize = prize + 1

			local shakeWidget = GetWidget("grab_bag_prize_chest_"..prize.."_shaker")
			local modelWidget = GetWidget("grab_bag_prize_chest_"..prize.."_model")
			local sleeper = GetWidget("grab_bag_prize_chest_"..prize.."_sleeper")
			local sharedSleeper = GetWidget("grab_bag_theme_sleeper")

			local theme = HoN_Grabbag.grabBagTheme.possibleModels[HoN_Grabbag.grabBagRandomChests[prize]]

			GetWidget("grab_bag_prize_"..prize):FadeIn(150)

			-- start the chests shaking
			if (theme.shakeName and NotEmpty(theme.shakeName)) then
				local chestNum = prize
				local chestShake = function(func)
					shakeWidget:Sleep(math.random(theme.shakeTimes[1], theme.shakeTimes[2]), function()
						modelWidget:SetAnim(theme.shakeName)
						if (theme.onShakeFunc) then
							theme.onShakeFunc(chestNum, sleeper, sharedSleeper)
						end

						func(func)
					end)
				end

				chestShake(chestShake)
			end

			if (prize < prizes.count) then
				sleeper2:Sleep(150, function() func(func) end)
			end
		end

		presentNextPrize(presentNextPrize)

		-- runs after all the prizes are displayed
		sleeper1:Sleep(150*prizes.count, function()
			if (HoN_Grabbag.grabBagTheme.onChestsVisible) then
				HoN_Grabbag.grabBagTheme.onChestsVisible()
			end
		end)
	end)

end

function HoN_Grabbag:GrabBagResults(parentName, theme, productIDs, productNames, productPaths, productTypes)
	local parent = GetWidget(parentName)
	if (not parent) then
		return
	end

	GetWidget("grabBag"):SetParent(parent)
	PopulateGrabBagTheme(theme)

	-- build the prizes table
	HoN_Grabbag.prizeTable = {}

	local productIDs = explode('|', productIDs)
	local productNames = explode('|', productNames)
	local productPaths = explode('|', productPaths)
	local productTypes = explode('|', productTypes)
	HoN_Grabbag.prizeTable.count = #productIDs

	for i=1, HoN_Grabbag.prizeTable.count do
		HoN_Grabbag.prizeTable[i] = {}
		HoN_Grabbag.prizeTable[i].productID = productIDs[i]
		HoN_Grabbag.prizeTable[i].name = productNames[i]
		HoN_Grabbag.prizeTable[i].path = productPaths[i]
		HoN_Grabbag.prizeTable[i].type = productTypes[i]
	end

	PopulateGrabBagPrizes(HoN_Grabbag.prizeTable)

	PresentGrabBagPrizes(HoN_Grabbag.prizeTable)
end

function HoN_Grabbag:HoverGrabBagChest(chestNumber)
	chestNumber = tonumber(chestNumber)

	if (HoN_Grabbag.prizeTable[chestNumber] and (not HoN_Grabbag.prizeTable[chestNumber].opened)) then
		GetWidget("grab_bag_prize_chest_"..chestNumber.."_model"):BringToFront()
		GetWidget("grab_bag_prize_chest_"..chestNumber.."_model"):UICmd("SetEffectIndexed('"..HoN_Grabbag.grabBagTheme.possibleModels[HoN_Grabbag.grabBagRandomChests[chestNumber]].hoverEffect.."', 1);")
	end
end

function HoN_Grabbag:UnhoverGrabBagChest(chestNumber)
	chestNumber = tonumber(chestNumber)

	if (HoN_Grabbag.prizeTable[chestNumber] and (not HoN_Grabbag.prizeTable[chestNumber].opened)) then
		GetWidget("grab_bag_prize_chest_"..chestNumber.."_model"):UICmd("SetEffectIndexed('', 1);")
	end
end

function HoN_Grabbag:ClickGrabBagChest(chestNumber)
	chestNumber = tonumber(chestNumber)
	-- stop the chest from shaking by interrupting the sleeper for it
	GetWidget("grab_bag_prize_chest_"..chestNumber.."_shaker"):Sleep(1, function() end)

	local sleeper = GetWidget("grab_bag_prize_chest_"..chestNumber.."_sleeper")
	local sharedSleeper = GetWidget("grab_bag_theme_sleeper")
	local theme = HoN_Grabbag.grabBagTheme.possibleModels[HoN_Grabbag.grabBagRandomChests[chestNumber]]

	if (HoN_Grabbag.prizeTable[chestNumber] and (not HoN_Grabbag.prizeTable[chestNumber].opened)) then
		HoN_Grabbag.prizeTable[chestNumber].opened = true

		if (theme.onOpenFunc) then
			theme.onOpenFunc(chestNumber, sleeper, sharedSleeper)
		end

		local chest = GetWidget("grab_bag_prize_chest_"..chestNumber.."_model")
		chest:SetAnim(theme.animName)
		chest:UICmd("SetEffectIndexed('"..theme.animEffect.."', 1);")	-- this will overwrite the hover effect
		chest:Sleep(theme.animTime, function()
			local slider = GetWidget("grab_bag_prize_"..chestNumber.."_slider")
			slider:SlideY(-slider:GetYFromString(theme.yClipperPos), 250)
			if (theme.onAppearFunc) then
				theme.onAppearFunc(chestNumber, sleeper, sharedSleeper)
			end

			chest:Sleep(250, function()
				if (theme.onFinishFunc) then
					theme.onFinishFunc(chestNumber, sleeper, sharedSleeper)
				end
			end)
		end)
	end
end

-- Grabbag accessor function
function interface:HoNGrabbagF(func, ...)
  print(HoN_Grabbag[func](self, ...))
end	