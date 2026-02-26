
local themeHelper = {}	-- store grab bag related theme variables on here (ones that need to exist between onOpen/onAppear/onFinish, etc)

local grabBagDefinition = {
	[1] = {
			-- model stuff
			bag 		= "/ui/fe2/grabbag/lucky_bundle/footlocker.mdf",
			effect 		= "/ui/fe2/grabbag/lucky_bundle/hover_effect.effect",
			animName 	= "open",
			animTime 	= 900,
			animEffect	= "/ui/fe2/grabbag/lucky_bundle/open.effect",
			hoverEffect = "/ui/fe2/grabbag/lucky_bundle/hover_effect.effect",
			shakeName	= "shake",
			shakeTimes	= {3500, 7500},
			camAngles 	= "-6 0 270",
			camPos 		= "-600 0 50",
			sunColor	= "0.6 0.6 0.6",

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
				Play2DSFXSound('/ui/fe2/plinko/sounds/chest_appear_'..(math.random(1,2))..'.ogg', 0.3)
			end,
	},

	[2] = {
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
			sunColor	= "1 1 1",

			-- functional stuff
			yClipperPos	= "-10.6h",

			-- animation / effects stuff (unused panels exist for the sole purpose of being used in here)
			onPopulateFunc	= function()	-- ran when the theme is populated, can be used to hide panels, etc in prep for stuff
				-- snowww!!!!
				local themeModelBack = GetWidget("grab_bag2_theme_modelpanel_back")
				themeModelBack:SetModel("/ui/effects/mainmenu/right.mdf")
				themeModelBack:SetEffect("/ui/effects/mainmenu/snow_main.effect")
				themeModelBack:SetVisible(1)
			end,
	},

	[3] = {
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
			sunColor	= "1 1 1",

			-- functional stuff
			yClipperPos	= "-10.6h",

			-- animation / effects stuff (unused panels exist for the sole purpose of being used in here)
			onPopulateFunc	= function()	-- ran when the theme is populated, can be used to hide panels, etc in prep for stuff
				local themeModelBack = GetWidget("grab_bag2_theme_modelpanel_back")
				themeModelBack:SetModel("/ui/effects/mainmenu/right.mdf")
				themeModelBack:SetEffect("/ui/effects/mainmenu/snow_main.effect")
				themeModelBack:SetVisible(1)
			end,
	},

	[4] = {
			-- model stuff
			bag 		= "/ui/fe2/grabbag/christmas/present/present.mdf",
			effect 		= "/ui/fe2/grabbag/christmas/present/present.effect",
			animName 	= "open",
			animTime 	= 900,
			animEffect	= "/ui/fe2/grabbag/christmas/present/present_open.effect",
			hoverEffect = "/ui/fe2/grabbag/hover_effect.effect",
			shakeName	= "shake",
			shakeTimes	= {3500, 7500},
			camAngles 	= "0 0 0",
			camPos 		= "0 -700 0",
			sunColor	= "1 1 1",

			-- functional stuff
			yClipperPos	= "-6.25h",

			onOpenFunc = function(chestNumber, chestSleeper, sharedSleeper)	-- ran when the chest is clicked, chestNumber is the number clicked,
				PlaySound("/ui/fe2/plinko/sounds/chest_appear_1.ogg")
				chestSleeper:Sleep(400, function()
					PlaySound("/ui/fe2/plinko/sounds/tier_3_chest_open.ogg")
				end)
			end,
	},

	[5] = {
			-- model stuff
			bag 		= "/ui/fe2/grabbag/christmas/present/present_02.mdf",
			effect 		= "/ui/fe2/grabbag/christmas/present/present.effect",
			animName 	= "open",
			animTime 	= 900,
			animEffect	= "/ui/fe2/grabbag/christmas/present/present_open.effect",
			hoverEffect = "/ui/fe2/grabbag/hover_effect.effect",
			shakeName	= "shake",
			shakeTimes	= {3500, 7500},
			camAngles 	= "0 0 0",
			camPos 		= "0 -700 0",
			sunColor	= "1 1 1",

			-- functional stuff
			yClipperPos	= "-6.70h",

			onOpenFunc = function(chestNumber, chestSleeper, sharedSleeper)	-- ran when the chest is clicked, chestNumber is the number clicked,
				PlaySound("/ui/fe2/plinko/sounds/chest_appear_1.ogg")
				chestSleeper:Sleep(400, function()
					PlaySound("/ui/fe2/plinko/sounds/tier_3_chest_open.ogg")
				end)
			end,
	},

	[6] = {
			-- model stuff
			bag 		= "/ui/fe2/grabbag/christmas/present/present_03.mdf",
			effect 		= "/ui/fe2/grabbag/christmas/present/present.effect",
			animName 	= "open",
			animTime 	= 900,
			animEffect	= "/ui/fe2/grabbag/christmas/present/present_open.effect",
			hoverEffect = "/ui/fe2/grabbag/hover_effect.effect",
			shakeName	= "shake",
			shakeTimes	= {3500, 7500},
			camAngles 	= "0 0 0",
			camPos 		= "0 -700 0",
			sunColor	= "1 1 1",

			-- functional stuff
			yClipperPos	= "-6.25h",

			onOpenFunc = function(chestNumber, chestSleeper, sharedSleeper)	-- ran when the chest is clicked, chestNumber is the number clicked,
				PlaySound("/ui/fe2/plinko/sounds/chest_appear_1.ogg")
				chestSleeper:Sleep(400, function()
					PlaySound("/ui/fe2/plinko/sounds/tier_3_chest_open.ogg")
				end)
			end,
	},

	[7] = {
			-- model stuff
			bag 		= "/ui/fe2/grabbag/discount_bundle/footlocker.mdf",
			effect 		= "",
			animName 	= "open",
			animTime 	= 900,
			animEffect	= "/ui/fe2/grabbag/lucky_bundle/open.effect",
			hoverEffect = "/ui/fe2/grabbag/discount_bundle/hover_effect.effect",
			shakeName	= "shake",
			shakeTimes	= {3500, 7500},
			camAngles 	= "-6 0 270",
			camPos 		= "-600 0 50",
			sunColor	= "0.6 0.6 0.6",

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
				Play2DSFXSound('/ui/fe2/plinko/sounds/chest_appear_'..(math.random(1,2))..'.ogg', 0.3)
			end,
	},

	[8] = {
			-- model stuff
			bag 		= "/ui/fe2/grabbag/halloween2017/pumpkin.mdf",
			effect 		= "",
			animName 	= "open",
			animTime 	= 1300,
			animEffect	= "/ui/fe2/grabbag/halloween2017/pumpkin_open.effect",
			hoverEffect = "/ui/fe2/grabbag/halloween/hover_effect.effect",
			shakeName	= "shake",
			shakeTimes	= {3500, 7500},
			camAngles 	= "0 0 0",
			camPos 		= "0 -350 0",
			sunColor	= "1 1 1",

			-- functional stuff
			yClipperPos	= "-10.6h",

			-- animation / effects stuff (unused panels exist for the sole purpose of being used in here)
			onPopulateFunc	= function()	-- ran when the theme is populated, can be used to hide panels, etc in prep for stuff
				local themeModelBack = GetWidget("grab_bag2_theme_modelpanel_back")
				themeModelBack:SetModel("/ui/effects/mainmenu/right.mdf")
				themeModelBack:SetEffect("/ui/effects/mainmenu/snow_main.effect")
				themeModelBack:SetVisible(1)
			end,
	},
}


local _G = getfenv(0)
HoN_Grabbag2 = _G['HoN_Grabbag2'] or {}
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()

local chestOpened = false

local function GetGrabBagDefinition(modelPath)
	for i,v in ipairs(grabBagDefinition) do
		if modelPath == v.bag then
			return v
		end
	end
	return grabBagDefinition[1]
end

local function ResetModelPanel(modelPanel)
	modelPanel:SetCameraPos(0, 6000, 75)
	modelPanel:SetCameraAngles(0, 0, 180)
	modelPanel:SetModelPos(0, 0, 0)
	modelPanel:SetModelAngles(0, 0, 0)
	modelPanel:SetModelScale(1)

	modelPanel:SetModel('/shared/models/invis.mdf')
	modelPanel:SetModel(0, '/shared/models/invis.mdf')
	modelPanel:SetModel(1, '/shared/models/invis.mdf')
	modelPanel:SetEffect('')
	modelPanel:SetEffect(0, '')
	modelPanel:SetEffect(1, '')
end

------ Grab Bag Stuff -----
local function PopulateGrabBag2Theme(themeName, chestCount)

	local grabBagDef = GetGrabBagDefinition(HoN_Store.BundleDetail.Model)

	-- set any of the models panels, etc. invisible encase a different theme used them, the pop function used next might reset them visible
	GetWidget("grab_bag2_theme_modelpanel_back"):SetVisible(0)
	GetWidget("grab_bag2_theme_modelpanel_fore"):SetVisible(0)

	if (grabBagDef.onPopulateFunc) then
		grabBagDef.onPopulateFunc()
	end

	-- Echo('PopulateGrabBag2Theme themeName:'..themeName..' chestCount: '..chestCount..' HoN_Store.BundleDetail.Model:'..HoN_Store.BundleDetail.Model)

	for i=1, chestCount do
		local model = GetWidget("grab_bag2_prize_chest_"..i.."_model")
		model:SetCameraPosString(grabBagDef.camPos)
		model:SetCameraAnglesString(grabBagDef.camAngles)
		model:SetSunColorString(grabBagDef.sunColor)

		model:SetModel(grabBagDef.bag)
		model:SetAnim("idle")
		model:SetMultiEffect(grabBagDef.effect, 0)
	end

	if NotEmpty(grabBagDef.overlayImage) then
		GetWidget("grab_bag2_theme_fore"):SetTexture(grabBagDef.overlayImage)
		GetWidget("grab_bag2_theme_fore"):SetVisible(1)
	else
		GetWidget("grab_bag2_theme_fore"):SetVisible(0)
	end
	if NotEmpty(grabBagDef.backgroundImage) then
		GetWidget("grab_bag2_theme_back"):SetTexture(grabBagDef.backgroundImage)
		GetWidget("grab_bag2_theme_back"):SetVisible(1)
	else
		GetWidget("grab_bag2_theme_back"):SetTexture("$invis")
		GetWidget("grab_bag2_theme_back"):SetVisible(0)
	end

	GetWidget('store2_grabbag_comfirm'):SetVisible(false)
end



local function PopulateGrabBag2Prizes(prizeTable)
	for i=1,5 do
		if (i > prizeTable.count) then
			GetWidget("grab_bag2_prize_"..i):SetVisible(0)
		else
			GetWidget("grab_bag2_prize_"..i.."_slider"):SetVisible(false)

			GetWidget("grab_bag2_prize_"..i):SetVisible(1)
			GetWidget("grab_bag2_prize_"..i.."_slider"):SetY("50%")

			local prize = prizeTable[i]

			local iconPanel = GetWidget("grab_bag2_prize_icon_"..i.."_icon")

			local modelPanel = GetWidget("grab_bag2_prize_model_"..i.."_model")
			ResetModelPanel(modelPanel)

			local itemID = prize.productID

			if prize.type == "EAP" then
				local t = string.match(prize.name, '(.+)%.eap')
				if t then
					prize.name = t
					prize.type = "Hero"
				end
			end

			local showModel = false

			-- setup the model
			if prize.type == "Alt Avatar" or prize.type == "Hero" then

				HoN_Store:SetModelPanel(modelPanel, prize.name, 'special')
				modelPanel:SetCameraPos(-28, 1100, 1500)
				modelPanel:SetCameraAngles(-12, 0, 180)

				showModel = true
			elseif prize.type == "Couriers" then

				local info = HoN_Store.CouriersTable[itemID]
				if info then
					local product = 'Pet_AutomatedCourier.'..info.product

					modelPanel:SetModel(GetHeroPreviewModelPathFromProduct(product))
					modelPanel:SetEffect(GetHeroStorePassiveEffectPathFromProduct(product))
					modelPanel:SetModelPos(GetHeroPreviewPosFromProduct(product))
					modelPanel:SetModelAngles(GetHeroPreviewAnglesFromProduct(product))
					modelPanel:SetModelScale(GetHeroPreviewScaleFromProduct(product))

					showModel = true
				end
			elseif prize.type == "Ward" then
				LoadEntityDefinition('/items/basic/flaming_eye/gadget.entity')
				LoadEntityDefinition('/items/basic/mana_eye/gadget.entity')

				local info = HoN_Store.WardsTable[itemID]
				if info then
					local flamingEyeProduct = 'Gadget_FlamingEye.'..info.product
					local manaEyeProduct = 'Gadget_Item_ManaEye.'..info.product

					modelPanel:SetModel(GetHeroPreviewModelPathFromProduct(flamingEyeProduct))
					modelPanel:SetEffect(GetHeroStorePassiveEffectPathFromProduct(flamingEyeProduct))
					modelPanel:SetModelAngles(GetHeroPreviewAnglesFromProduct(flamingEyeProduct))
					modelPanel:SetModelScale(GetHeroPreviewScaleFromProduct(flamingEyeProduct))
					modelPanel:SetModelPos(30, -50, -50)

					modelPanel:SetModel(0, GetHeroPreviewModelPathFromProduct(manaEyeProduct))
					modelPanel:SetEffect(0, GetHeroStorePassiveEffectPathFromProduct(manaEyeProduct))
					modelPanel:SetModelAngles(0, GetHeroPreviewAnglesFromProduct(manaEyeProduct))
					modelPanel:SetModelScale(0, GetHeroPreviewScaleFromProduct(manaEyeProduct))
					modelPanel:SetModelPos(0, -30, -50, -50)

					showModel = true
				end
			elseif prize.type == "Creep" then
				local info = HoN_Store.CreepsTable[itemID]
				if info then
					modelPanel:SetModel(info['bad_melee'].model)
					modelPanel:SetEffect(info['bad_melee'].effect)
					modelPanel:SetAnim('idle')
					modelPanel:SetModelScale(2.25)

					showModel = true
				end
			elseif prize.type == 'Taunt' then
				local info = HoN_Store.TauntsTable[itemID]
				if info then
					if info.effectPath ~= '' then
						modelPanel:SetModelScale(1)
						showModel = true
					end
				end
			elseif prize.type == 'Alt Announcement' then
				local info = HoN_Store.VoiceAnnouncersTable[itemID]
				if info then
					modelPanel:SetModelScale(0.25)
					showModel = true
				end
			end

			if not showModel then
				iconPanel:SetTexture(prize.path)
			end

			-- set the name
			local nameStr = Translate("mstore_product"..prize.productID.."_name")
			local nameLabel = GetWidget("grab_bag2_prize_"..i.."_name")
			nameLabel:SetFont(GetFontSizeForWidth(nameStr, nameLabel:GetWidth(), 14))
			nameLabel:SetText(nameStr)

			-- name color
			local nameColorLabel = GetWidget("grab_bag2_prize_icon_"..i.."_nameColor")
			local visible = false
			if prize.type == "Chat Color" then

				local name = ProductTypeToPrefix(prize.type)..prize.name

				local colorProps = GetChatNameColor(prize.name)
				nameColorLabel:SetColor(colorProps.color or 'red')
				nameColorLabel:SetGlow(colorProps.glow or false)
				nameColorLabel:SetBackgroundGlow(colorProps.backgroundGlow or false)
				nameColorLabel:SetGlowColor(colorProps.glowColor or 'red')
				visible = true
			end
			nameColorLabel:SetVisible(visible)

			-- set stuff visible
			GetWidget("grab_bag2_prize_model_"..i):SetVisible(showModel)
			GetWidget("grab_bag2_prize_icon_"..i):SetVisible(not showModel)
		end
	end
end

local function PresentGrabBag2Prizes(prizes, count)
	local sleeper1 = GetWidget("grab_bag2_sleeper_1")
	local sleeper2 = GetWidget("grab_bag2_sleeper_2")

	GetWidget("grabBag2"):FadeIn(150)
	sleeper1:Sleep(150, function()

		-- present each prize
		local prize = 0
		local presentNextPrize = function(func)
			prize = prize + 1

			local shakeWidget = GetWidget("grab_bag2_prize_chest_"..prize.."_shaker")
			local modelWidget = GetWidget("grab_bag2_prize_chest_"..prize.."_model")
			local sleeper = GetWidget("grab_bag2_prize_chest_"..prize.."_sleeper")
			local sharedSleeper = GetWidget("grab_bag2_theme_sleeper")

			local grabBagDef = GetGrabBagDefinition(HoN_Store.BundleDetail.Model)

			GetWidget("grab_bag2_prize_chest_"..prize):FadeIn(150)

			-- start the chests shaking
			if (grabBagDef.shakeName and NotEmpty(grabBagDef.shakeName)) then
				local chestNum = prize
				local chestShake = function(func)
					shakeWidget:Sleep(math.random(grabBagDef.shakeTimes[1], grabBagDef.shakeTimes[2]), function()
						modelWidget:SetAnim(grabBagDef.shakeName)
						if (grabBagDef.onShakeFunc) then
							grabBagDef.onShakeFunc(chestNum, sleeper, sharedSleeper)
						end

						func(func)
					end)
				end

				chestShake(chestShake)
			end

			if (prize < count) then
				sleeper2:Sleep(150, function() func(func) end)
			end
		end

		presentNextPrize(presentNextPrize)

		-- runs after all the prizes are displayed
		-- sleeper1:Sleep(150, function()
		-- 	if (HoN_Grabbag2.grabBagTheme.onChestsVisible) then
		-- 		HoN_Grabbag2.grabBagTheme.onChestsVisible()
		-- 	end
		-- end)
	end)
end

-- for store2
function HoN_Grabbag2:GrabBagResults(parentName, theme, productIDs, productNames, productPaths, productTypes)

	-- Echo('HoN_Grabbag2:GrabBagResults parentName:'..parentName..' theme:'..theme..' productIDs:'..productIDs..' productNames:'..productNames..' productPaths:'..productPaths)

	local parent = GetWidget(parentName)
	if (not parent) then
		return
	end

	PopulateGrabBag2Theme(theme, 1)

	-- build the prizes table
	HoN_Grabbag2.prizeTable = {}

	local productIDs = explode('|', productIDs)
	local productNames = explode('|', productNames)
	local productPaths = explode('|', productPaths)
	local productTypes = explode('|', productTypes)
	HoN_Grabbag2.prizeTable.count = #productIDs

	for i=1, HoN_Grabbag2.prizeTable.count do
		HoN_Grabbag2.prizeTable[i] = {}
		HoN_Grabbag2.prizeTable[i].productID = productIDs[i]
		HoN_Grabbag2.prizeTable[i].name = productNames[i]
		HoN_Grabbag2.prizeTable[i].path = productPaths[i]
		HoN_Grabbag2.prizeTable[i].type = productTypes[i]
	end

	PopulateGrabBag2Prizes(HoN_Grabbag2.prizeTable)
	PresentGrabBag2Prizes(HoN_Grabbag2.prizeTable, 1)

	GetWidget('grabBag2'):BringToFront()

	chestOpened = false
end

function HoN_Grabbag2:HoverGrabBagChest(chestNumber)
	local grabBagDef = GetGrabBagDefinition(HoN_Store.BundleDetail.Model)
	chestNumber = tonumber(chestNumber)

	if (HoN_Grabbag2.prizeTable[chestNumber] and (not HoN_Grabbag2.prizeTable[chestNumber].opened)) then
		local modelWdg = GetWidget("grab_bag2_prize_chest_"..chestNumber.."_model")
		modelWdg:BringToFront()
		modelWdg:SetMultiEffect(grabBagDef.hoverEffect, 1)
	end
end

function HoN_Grabbag2:UnhoverGrabBagChest(chestNumber)
	local grabBagDef = GetGrabBagDefinition(HoN_Store.BundleDetail.Model)
	chestNumber = tonumber(chestNumber)

	if (HoN_Grabbag2.prizeTable[chestNumber] and (not HoN_Grabbag2.prizeTable[chestNumber].opened)) then
		local modelWdg = GetWidget("grab_bag2_prize_chest_"..chestNumber.."_model")
		modelWdg:SetMultiEffect(grabBagDef.effect, 0)
	end
end

local PrizeXSlides =
{
	[1] = {'0'},
	[2] = {'-25h', '25h'},
	[3] = {'-35h', '0', '35h'},
	[4] = {'-45h', '-15h', '15h', '45h'},
	[5] = {'-50h', '-25h', '0', '25h', '50h'},
}

function HoN_Grabbag2:ClickGrabBagChest(chestNumber)

	if chestOpened then return end
	chestOpened = true

	local function OpenChest(chestNumber, onFinishCallback)
		chestNumber = tonumber(chestNumber)

		local shaker = GetWidget("grab_bag2_prize_chest_"..chestNumber.."_shaker", nil, true)
		if shaker then shaker:Sleep(1, function() end) end

		local sharedSleeper = GetWidget("grab_bag2_theme_sleeper")
		local grabBagDef = GetGrabBagDefinition(HoN_Store.BundleDetail.Model)

		if (HoN_Grabbag2.prizeTable[chestNumber] and (not HoN_Grabbag2.prizeTable[chestNumber].opened)) then
			HoN_Grabbag2.prizeTable[chestNumber].opened = true

			local chest = GetWidget("grab_bag2_prize_chest_"..chestNumber.."_model", nil, true)
			local sleeper = GetWidget("grab_bag2_prize_chest_"..chestNumber.."_sleeper", nil, true)

			if chest and grabBagDef.onOpenFunc then
				grabBagDef.onOpenFunc(chestNumber, sleeper, sharedSleeper)
			end

			if chest then
				chest:SetAnim(grabBagDef.animName)
				chest:SetMultiEffect(grabBagDef.animEffect, 1)	-- this will overwrite the hover effect
			end

			sharedSleeper:Sleep(grabBagDef.animTime, function()
				local slider = GetWidget("grab_bag2_prize_"..chestNumber.."_slider")
				slider:SlideY('15h', 250)

				local centerXDiff = PrizeXSlides[HoN_Grabbag2.prizeTable.count][chestNumber]
				Echo('^p centerXDiff='..centerXDiff)
				slider:SlideX(centerXDiff, 250)
				slider:FadeIn(250)

				if chest and grabBagDef.onAppearFunc then
					grabBagDef.onAppearFunc(chestNumber, sleeper, sharedSleeper)
				end

				sharedSleeper:Sleep(250, function()

					local recCall = true
					local prize = HoN_Grabbag2.prizeTable[chestNumber]
					if prize.type == 'Taunt' or prize.type == 'Alt Announcement' then
						local itemID = prize.productID

						local modelPanel = GetWidget("grab_bag2_prize_model_"..chestNumber.."_model")
						local modelParent = GetWidget("grab_bag2_prize_model_"..chestNumber)
						local iconPanel = GetWidget("grab_bag2_prize_icon_"..chestNumber.."_icon")
						local iconParent = GetWidget("grab_bag2_prize_icon_"..chestNumber)

						if prize.type == 'Taunt' then
							local info = HoN_Store.TauntsTable[itemID]
							if info then
								if info.effectPath ~= '' then
									modelPanel:SetEffect(info.effectPath)
									modelPanel:SetAnim('idle')
						
									iconPanel:RegisterWatch("EffectFinished",
										function(self, modelPanelName)
											if modelPanelName == modelPanel:GetName() then
												modelParent:SetVisible(false)
												iconPanel:UnregisterWatch("EffectFinished")
												iconPanel:SetTexture(prize.path)
												iconParent:SetVisible(true)

												onFinishCallback(chestNumber, onFinishCallback)
											end
										end)
									recCall = false
								end
							end
						elseif prize.type == 'Alt Announcement' then
							local info = HoN_Store.VoiceAnnouncersTable[itemID]
							if info then
								PlaySound('/shared/sounds/announcer/'..info['voicePack_code']..'/first_blood.wav', 1.0, 1)
								modelPanel:SetModel('/ui/common/models/'..info['voicePack_models']..'/bloodlust.mdf', 1.0, 1)
								modelPanel:SetAnim('idle')
								modelPanel:SetEffect('/ui/common/models/'..info['voicePack_models']..'/bloodlust.effect')
								modelPanel:Sleep(3200, function(self)
									modelParent:SetVisible(false)
									iconPanel:SetTexture(prize.path)
									iconParent:FadeIn(250)

									onFinishCallback(chestNumber, onFinishCallback)
								end)

								recCall = false
							end
						end
					end

					if chest and grabBagDef.onFinishFunc then
						grabBagDef.onFinishFunc(chestNumber, sleeper, sharedSleeper)
					end
					if recCall then
						onFinishCallback(chestNumber, onFinishCallback)
					end
				end)
			end)
		end
	end

	local function OpenChestOneByOne(chestNumber, OpenChestOneByOneFunc)
		local function OnOpenChestFinish(chestNumber, func)
			if chestNumber < HoN_Grabbag2.prizeTable.count then
				local nextChestNumber = chestNumber + 1
				OpenChestOneByOneFunc(nextChestNumber, OpenChestOneByOneFunc)
			else
				GetWidget('store2_grabbag_comfirm'):FadeIn(200)
			end
		end
		OpenChest(chestNumber, OnOpenChestFinish)
	end

	local chestSlideTime = 200
	local chest = GetWidget('grab_bag2_prize_chest_'..chestNumber)
	chest:SlideY('', chestSlideTime)

	local sharedSleeper = GetWidget("grab_bag2_theme_sleeper")
	sharedSleeper:Sleep(chestSlideTime, function()
		local count = HoN_Grabbag2.prizeTable.count
		if count < 1 then return end

		local chestNumber = 1
		OpenChestOneByOne(chestNumber, OpenChestOneByOne)
		end)
end


-- Grabbag accessor function
function interface:HoNGrabbag2F(func, ...)
  print(HoN_Grabbag2[func](self, ...))
end