------------------------
--	HoN Plinko Script
-- Copyright 2015 Frostburn Studios
------------------------

local NUM_PAST_PATHS = 8 -- number of past paths to remember so they can't be reselected
local BULB_ANIMATION_CHANGE_TIME = 17500 --number of MS between the bulb animation changing to a random animation (it can reselect the same one)
local MS_PER_PATH_FRAME = math.floor(1000 / 57) -- how long to wait between each frame of animation

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()

RegisterScript2('Plinko', '5')

local HoN_Plinko = _G["HoN_Plinko"] or {}

HoN_Plinko.plinkoOpen = false

HoN_Plinko.pastPathsDB = nil
HoN_Plinko.path = nil
HoN_Plinko.puck_widget = nil
HoN_Plinko.puck_trail_widgets = {}
HoN_Plinko.bulbAnimationNumber = nil

HoN_Plinko.prizeTierWon = nil
HoN_Plinko.prizeProductWon = nil
HoN_Plinko.dropping = false

-- HoN_Plinko.dropCount = 1
HoN_Plinko.tickets = 0
 
HoN_Plinko.rewardsCount = {}
HoN_Plinko.rewardsItems = {}
HoN_Plinko.plinkoUpdateTier = {}

HoN_Plinko.dropDisableButtons = {}

HoN_Plinko.ticketPrizeRequested = nil
--HoN_Plinko.dropPackageRequested = nil

HoN_Plinko.dontPromptDrop = false

HoN_Plinko.bucketTiers = {}
HoN_Plinko.ticketPrizes = {}
--HoN_Plinko.dropPackages = {}
HoN_Plinko.dropTicketCost = 0
HoN_Plinko.dropGoldCost = 0
HoN_Plinko.currency = GetDBEntry("PlinkoCurrency") or "gold"
HoN_Plinko.mouseOverCurrency = ""

HoN_Plinko.theme = {}

-- time table for themeing the Plinko board based on dates
-- if there isn't an override given for a graphic/sound, it will use the default
-- if there is no special entry for the current time, it will use the default
-- the theme will start at 0:00 on startMonth/startDay
-- the theme will end at 0:00 on endMonth/endDay
-- if you want a theme to run all of (and only) 12/24, you would used 12, 24, 12, 25
local plinkoThemes = {
	['default'] = {
		-- times
		['startMonth']		= nil,
		['startDay'] 		= nil,
		['endMonth']		= nil,
		['endDay']			= nil,

		-- plinko art
		['puck']			= "/ui/fe2/plinko/puck.tga",
		['board']			= "/ui/fe2/plinko/board.tga",
		['plinkoBack']		= "/ui/fe2/plinko/background.tga",
		['plinkoDropBoard']	= "/ui/fe2/plinko/plinko_drop_board.tga",
		['plinkoDropBackers'] = true,
		['plinkoDropGold']	= "/ui/fe2/plinko/gold_icon_large.tga",
		['plinkoDropTicket']= "/ui/fe2/plinko/ticket_icon_large.tga",
		['plinkoThemeOver'] = "$invis",
		-- ticket redemption art
		['ticketCounter']	= "/ui/fe2/plinko/ticket_counter.tga",
		['ticketBack']		= "/ui/fe2/plinko/ticket_background.tga",
		['ticketRegister']	= "/ui/fe2/plinko/ticket_register.tga",
		['ticketSign']		= "/ui/fe2/plinko/ticket_sign.tga",
		['ticketShelve']	= "/ui/fe2/plinko/prize_shelve.tga",
		['ticketThemeOver'] = "$invis",
		-- board bulb art
		['bulb1']			= "/ui/fe2/plinko/bulb_1.tga",
		['bulb2']			= "/ui/fe2/plinko/bulb_2.tga",
		['bulb3']			= "/ui/fe2/plinko/bulb_3.tga",
		['bulb4']			= "/ui/fe2/plinko/bulb_4.tga",
		-- tier art
		['tier1']			= "/ui/fe2/plinko/tier_1.tga",
		['tier2']			= "/ui/fe2/plinko/tier_2.tga",
		['tier3']			= "/ui/fe2/plinko/tier_3.tga",
		['tier4']			= "/ui/fe2/plinko/tier_4.tga",
		['tier5']			= "/ui/fe2/plinko/tier_5_2.tga",
		['tier6']			= "/ui/fe2/plinko/tier_6.tga",
		-- other art
		['ticket']			= "/ui/fe2/plinko/ticket.tga",
		['back']			= "/ui/fe2/plinko/back.tga",
		['navSigns']		= "/ui/fe2/plinko/plinko_nav_signs.tga",
		['dropdown']		= "/ui/fe2/plinko/dropdown_sign.tga",
		['buttonInside']	= "/ui/fe2/plinko/button_inside.tga",
		['buttonOutside']	= "/ui/fe2/plinko/button_outside.tga",

		-- plinko sounds
		['sndDropAmbience']	= "/ui/fe2/plinko/sounds/amb_plinko_machine.ogg",
		['sndAmbience']		= "/ui/fe2/plinko/sounds/amb_background.ogg",
		['sndPlinkoTheme']	= "/ui/fe2/plinko/sounds/plinko_theme.ogg",
		['sndEnterPlinko']	= "/ui/fe2/plinko/sounds/enter_plinko.ogg",
		['sndDrop']			= "/ui/fe2/plinko/sounds/the_drop.ogg",
		['sndColBase']		= "/ui/fe2/plinko/sounds/col_",
		['sndPlinkoDone']	= "/ui/fe2/plinko/sounds/finished.ogg",
		-- ticket redemption sounds
		['sndEnterTicket']	= "/ui/fe2/plinko/sounds/enter_redeem.ogg",
		['sndTicketTheme']	= "/ui/fe2/plinko/sounds/amb_background.ogg",
		['sndTicketRedeem']	= "/ui/fe2/store/sounds/chaching.ogg",
		-- win sounds
		['sndCongratsWin']	= "/ui/fe2/store/goblin/voice/congratulations.ogg",
		['sndWinTickets']	= "/ui/fe2/plinko/sounds/ticket_fanfare.ogg",
		['sndManyTickets']	= "/ui/fe2/plinko/sounds/many_tickets.ogg",
		['sndChestApprBase']= "/ui/fe2/plinko/sounds/chest_appear_",
		['sndTier1ChestOpn']= "/ui/fe2/plinko/sounds/tier_1_chest_open.ogg",
		['sndTier2ChestOpn']= "/ui/fe2/plinko/sounds/tier_2_chest_open.ogg",
		['sndTier3ChestOpn']= "/ui/fe2/plinko/sounds/tier_3_chest_open.ogg",
	},
	['christmas'] = {
		-- times
		['startMonth']		= 12,
		['startDay'] 		= 13,
		['endMonth']		= 1,
		['endDay']			= 15,

		-- plinko art
		['puck']			= "/ui/fe2/plinko/christmas/puck.tga",
		['board']			= "/ui/fe2/plinko/christmas/board.tga",
		['plinkoBack']		= "/ui/fe2/plinko/christmas/background.tga",
		['plinkoDropBoard']	= "/ui/fe2/plinko/christmas/plinko_drop_board.tga",
		-- ticket redemption art
		['ticketCounter']	= "/ui/fe2/plinko/christmas/ticket_counter.tga",
		['ticketBack']		= "/ui/fe2/plinko/christmas/ticket_background.tga",
		['ticketSign']		= "/ui/fe2/plinko/christmas/ticket_sign.tga",
		['ticketShelve']	= "/ui/fe2/plinko/christmas/prize_shelve.tga",
		['ticketThemeOver'] = "/ui/fe2/plinko/christmas/ticket_redemption_overlay.tga",
		-- board bulb art
		['bulb1']			= "/ui/fe2/plinko/christmas/bulb_1.tga",
		['bulb3']			= "/ui/fe2/plinko/christmas/bulb_3.tga",
		-- other art
		['back']			= "/ui/fe2/plinko/christmas/back.tga",
		['navSigns']		= "/ui/fe2/plinko/christmas/plinko_nav_signs.tga",
	},
	--['paragon1'] = {
	--	-- times
	--	['startMonth']		= 1,
	--	['startDay'] 		= 15,
	--	['endMonth']		= 4,
	--	['endDay']			= 9,

	--	-- plinko art
	--	['puck']			= "/ui/fe2/plinko/paragon/puck.tga",
	--	['board']			= "/ui/fe2/plinko/paragon/board.tga",
	--	['plinkoBack']		= "/ui/fe2/plinko/paragon/background.tga",
	--	['plinkoDropTicket']= "/ui/fe2/plinko/paragon/ticket_icon_large.tga",
	--	-- other art
	--},
	-- ['water_festival'] = {
		-- -- times
		-- ['startMonth']		= 4,
		-- ['startDay'] 		= 9,
		-- ['endMonth']		= 4,
		-- ['endDay']			= 24,

		-- -- plinko art
		-- ['puck']			= "/ui/fe2/plinko/water_festival/puck.tga",
		-- ['board']			= "/ui/fe2/plinko/water_festival/board.tga",
		-- ['plinkoBack']		= "/ui/fe2/plinko/water_festival/background.tga",
		-- ['plinkoDropBoard']	= "/ui/fe2/plinko/water_festival/plinko_drop_board.tga",
		-- -- ticket redemption art
		-- ['ticketCounter']	= "/ui/fe2/plinko/water_festival/ticket_counter.tga",
		-- ['ticketBack']		= "/ui/fe2/plinko/water_festival/ticket_background.tga",
		-- ['ticketSign']		= "/ui/fe2/plinko/water_festival/ticket_sign.tga",
		-- ['ticketShelve']	= "/ui/fe2/plinko/water_festival/prize_shelve.tga",
		-- -- board bulb art
		-- ['bulb1']			= "/ui/fe2/plinko/water_festival/bulb_1.tga",
		-- ['bulb3']			= "/ui/fe2/plinko/water_festival/bulb_3.tga",
		-- -- other art
		-- ['back']			= "/ui/fe2/plinko/water_festival/back.tga",
		-- ['navSigns']		= "/ui/fe2/plinko/water_festival/plinko_nav_signs.tga",
	-- },
	['spring'] = {
		-- times
		['startMonth']		= 4,
		['startDay'] 		= 9,
		['endMonth']		= 5,
		['endDay']			= 5,
		-- plinko art
		['puck']			= "/ui/fe2/plinko/spring/puck.tga",
		['board']			= "/ui/fe2/plinko/spring/board.tga",
		['plinkoBack']		= "/ui/fe2/plinko/spring/background.tga",
		['plinkoDropBoard']	= "/ui/fe2/plinko/spring/plinko_drop_board.tga",
		-- ticket redemption art
		['ticketCounter']	= "/ui/fe2/plinko/spring/ticket_counter.tga",
		['ticketBack']		= "/ui/fe2/plinko/spring/ticket_background.tga",
		['ticketSign']		= "/ui/fe2/plinko/spring/ticket_sign.tga",
		['ticketShelve']	= "/ui/fe2/plinko/spring/prize_shelve.tga",
		['ticketThemeOver'] = "/ui/fe2/plinko/spring/ticket_redemption_overlay.tga",
		-- board bulb art
		['bulb1']			= "/ui/fe2/plinko/spring/bulb_1.tga",
		['bulb3']			= "/ui/fe2/plinko/spring/bulb_3.tga",
		-- other art
		['back']			= "/ui/fe2/plinko/spring/back.tga",
		['navSigns']		= "/ui/fe2/plinko/spring/plinko_nav_signs.tga",
	},
	--['paragon2'] = {
	--	-- times
	--	['startMonth']		= 7,
	--	['startDay'] 		= 28,
	--	['endMonth']		= 12,
	--	['endDay']			= 15,

	--	-- plinko art
	--	['puck']			= "/ui/fe2/plinko/paragon/puck.tga",
	--	['board']			= "/ui/fe2/plinko/paragon/board.tga",
	--	['plinkoBack']		= "/ui/fe2/plinko/paragon/background.tga",
	--	['plinkoDropTicket']= "/ui/fe2/plinko/paragon/ticket_icon_large.tga",
	--	-- other art
	--},
	['halloween'] = {
		-- times
		['startMonth']		= 10,
		['startDay'] 		= 20,
		['endMonth']		= 11,
		['endDay']			= 5,
		-- plinko art
		['puck']			= "/ui/fe2/plinko/holoween/puck.tga",
		['board']			= "/ui/fe2/plinko/holoween/board.tga",
		['plinkoBack']		= "/ui/fe2/plinko/holoween/background.tga",
		['plinkoDropBoard']	= "/ui/fe2/plinko/holoween/plinko_drop_board.tga",
		-- ticket redemption art
		['ticketCounter']	= "/ui/fe2/plinko/holoween/ticket_counter.tga",
		['ticketBack']		= "/ui/fe2/plinko/holoween/ticket_background.tga",
		['ticketSign']		= "/ui/fe2/plinko/holoween/ticket_sign.tga",
		['ticketShelve']	= "/ui/fe2/plinko/holoween/prize_shelve.tga",
		['ticketThemeOver'] = "/ui/fe2/plinko/holoween/ticket_redemption_overlay.tga",
		-- board bulb art
		['bulb1']			= "/ui/fe2/plinko/holoween/bulb_1.tga",
		['bulb3']			= "/ui/fe2/plinko/holoween/bulb_3.tga",
		-- other art
		['back']			= "/ui/fe2/plinko/holoween/back.tga",
		['navSigns']		= "/ui/fe2/plinko/holoween/plinko_nav_signs.tga",
	},
}

if GetCvarBool('cl_GarenaEnable') then
	plinkoThemes.default.tier5 = '/ui/fe2/plinko/tier_5_2.tga'
end

-- animation tables for the bulbs, overkill- but fun! :D
-- each animation is an index, each index has the 'stepTime' which is the time between moving from one set of visibilities to the next
-- then instead each animation is a series of indexes, which has the true/false visible state for each of the 8 bulbs
-- just add a new animation at the next free index and it can be randomly selected by 'StartRandomBulbAnimation'
local bulbAnimations =
{
	[1]={
			['stepTime'] = 350,
			['fadeTime'] = 150,
			[1] = {true, false, true, false, true, false, true, false},
			[2] = {false, true, false, true, false, true, false, true},
		},
	[2]={
			['stepTime'] = 500,
			['fadeTime'] = 150,
			[1] = {true, true, true, false, true, true, true, false},
			[2] = {false, true, true, true, false, true, true, true},
			[3] = {true, false, true, true, true, false, true, true},
			[4] = {true, true, false, true, true, true, false, true},
		},
	[3]={
			['stepTime'] = 200,
			['fadeTime'] = 150,
			[1] = {true, true, true, true, true, true, true, false},
			[2] = {false, true, true, true, true, true, true, true},
			[3] = {true, false, true, true, true, true, true, true},
			[4] = {true, true, false, true, true, true, true, true},
			[5] = {true, true, true, false, true, true, true, true},
			[6] = {true, true, true, true, false, true, true, true},
			[7] = {true, true, true, true, true, false, true, true},
			[8] = {true, true, true, true, true, true, false, true},
		},
}

-- how the tiers populate their effects on the board
local tierInfo =
{
	[1] = 	{	["effect"] = "/ui/fe2/plinko/effects/plinko_chest_diamond.effect",
			},
	[2] = 	{	["effect"] = "/ui/fe2/plinko/effects/plinko_chest_gold.effect",
			},
	[3] = 	{	["effect"] = "/ui/fe2/plinko/effects/plinko_chest_silver.effect",
			},
	[4] = 	{	["effect"] = "/ui/fe2/plinko/effects/plinko_chest_bronze.effect",
			},
	[5] = 	{},
	[6] = 	{}
}


local function PuckFramePath()
	-- follow along the pre-generated path
	if (HoN_Plinko.pathCount < #HoN_Plinko.path) then
		for i=1,3 do
			local widget = HoN_Plinko.puck_trail_widgets[i]
			if (HoN_Plinko.pathCount > i) then
				if (HoN_Plinko.path[HoN_Plinko.pathCount-i].x) then
					widget:SetX(FtoP(HoN_Plinko.path[HoN_Plinko.pathCount-i].x))
				end
				if (HoN_Plinko.path[HoN_Plinko.pathCount-i].y) then
					widget:SetY(FtoP(HoN_Plinko.path[HoN_Plinko.pathCount-i].y))
				end
				if (HoN_Plinko.path[HoN_Plinko.pathCount-i].r) then
					widget:SetRotation(HoN_Plinko.path[HoN_Plinko.pathCount-i].r)
				end
				widget:SetVisible(1)
			else
				widget:SetVisible(0)
			end
		end

		if (HoN_Plinko.path[HoN_Plinko.pathCount].x) then
			HoN_Plinko.puck_widget:SetX(FtoP(HoN_Plinko.path[HoN_Plinko.pathCount].x))
		end
		if (HoN_Plinko.path[HoN_Plinko.pathCount].y) then
			HoN_Plinko.puck_widget:SetY(FtoP(HoN_Plinko.path[HoN_Plinko.pathCount].y))
		end
		-- HoN_Plinko.puck_widget:SlideX(FtoP(HoN_Plinko.path[HoN_Plinko.pathCount].x), MS_PER_PATH_FRAME)
		-- HoN_Plinko.puck_widget:SlideY(FtoP(HoN_Plinko.path[HoN_Plinko.pathCount].y), MS_PER_PATH_FRAME)
		if (HoN_Plinko.path[HoN_Plinko.pathCount].r) then
			HoN_Plinko.puck_widget:SetRotation(HoN_Plinko.path[HoN_Plinko.pathCount].r)
		end

		-- causes the fadeout on the sound to play
		if (HoN_Plinko.path[HoN_Plinko.pathCount].c and HoN_Plinko.path[HoN_Plinko.pathCount].y) then
			PlaySound(HoN_Plinko.theme.sndColBase..math.random(1,6)..'.ogg')
		end
		HoN_Plinko.pathCount = HoN_Plinko.pathCount + 1

		local framesLeft = (#HoN_Plinko.path) - HoN_Plinko.pathCount
		if (framesLeft == 10) then
			interface:UICmd("StopSound(4);")
		end

		HoN_Plinko.puck_widget:Sleep(MS_PER_PATH_FRAME, PuckFramePath)
	else
		--HoN_Plinko.puck_widget:ClearCallback('onframe')
		PlaySound(HoN_Plinko.theme.sndPlinkoDone)
		interface:UICmd("StopSound(4);")
		HoN_Plinko.puck_widget:SetVisible(0)
		for i=1,3 do
			HoN_Plinko.puck_trail_widgets[i]:SetVisible(0)
		end

		-- bit of a sleep before the presentation
		HoN_Plinko.puck_widget:Sleep(600, function() HoN_Plinko:PresentPrize() end)
	end
end

local function PlayPlinkoPath()
	PlaySound(HoN_Plinko.theme.sndDrop)
	-- start the background sound, 500 fadeout, 200 fadein
	interface:UICmd("Play2DSFXSound('"..HoN_Plinko.theme.sndDropAmbience.."', 1, 4, true, 200, 0, 500)")
	HoN_Plinko.pathCount = 1

	HoN_Plinko.puck_widget:SetVisible(1)
	PuckFramePath()
	-- HoN_Plinko.puck_widget:SetCallback("onframe", function()
	-- 	frameTimer = frameTimer + GetFrameTime()
	-- 	if (frameTimer >= msPerFrame) then
	-- 		frameTimer = frameTimer - msPerFrame
	-- 		PuckFramePath()
	-- 	end
	-- end)
end

local function StepBulbAnimation()
	if (HoN_Plinko.bulbAnimationNumber) then
		HoN_Plinko.bulbAnimationStep = HoN_Plinko.bulbAnimationStep + 1
		if (HoN_Plinko.bulbAnimationStep > #bulbAnimations[HoN_Plinko.bulbAnimationNumber]) then
			HoN_Plinko.bulbAnimationStep = 1
		end

		for i=1,8 do
			if (not bulbAnimations[HoN_Plinko.bulbAnimationNumber][HoN_Plinko.bulbAnimationStep][i]) then
				GetWidget("plinko_bulb_"..i):FadeOut(bulbAnimations[HoN_Plinko.bulbAnimationNumber].fadeTime)
			else
				GetWidget("plinko_bulb_"..i):FadeIn(bulbAnimations[HoN_Plinko.bulbAnimationNumber].fadeTime)
			end
		end

		-- two because 1 is sleeping for the animation change
		GetWidget("plinko_bulb_2"):Sleep(bulbAnimations[HoN_Plinko.bulbAnimationNumber].stepTime, StepBulbAnimation)
	end
end

local function StartRandomBulbAnimation()
	HoN_Plinko.bulbAnimationNumber = math.random(1, #bulbAnimations)
	HoN_Plinko.bulbAnimationStep = 1

	StepBulbAnimation()
end

local function BuildPlinkoTheme()
	local theme = table.copy(plinkoThemes.default) -- start with the default and override with changes from date appropriate ones

	local overrideTheme = GetCvarString('ui_overrideTheme', true)
	if (overrideTheme and plinkoThemes[overrideTheme]) then
		for k, v in pairs(plinkoThemes[overrideTheme]) do
			theme[k] = v
		end
		return theme
	end

	-- look for any themes with times that match the current date/time (year independant)
	for name, pTheme in pairs(plinkoThemes) do
		if (pTheme.startDay and pTheme.startMonth and pTheme.endDay and pTheme.endMonth) then
			if IsCurrentlyWithinNumericDateRange({['month'] = pTheme.startMonth, ['day'] = pTheme.startDay}, {['month'] = pTheme.endMonth, ['day'] = pTheme.endDay}) then
				for k, v in pairs(pTheme) do
					theme[k] = v
				end

				break
			end
		end
	end

	return theme
end

local function PopulatePlinkoTheme()
	if (not HoN_Plinko.theme) then return end

	-- populate all the panels and images etc with textures

	-- plinko
	GetWidget("plinko_background"):SetTexture(HoN_Plinko.theme.plinkoBack)
	GetWidget("plinko_board_main"):SetTexture(HoN_Plinko.theme.board)
	GetWidget("plinko_board_sub"):SetTexture(HoN_Plinko.theme.board)
	GetWidget("plinko_drop_board"):SetTexture(HoN_Plinko.theme.plinkoDropBoard)
	GetWidget('plinko_drop_type_gold_frame'):SetVisible(HoN_Plinko.theme.plinkoDropBackers)
	GetWidget('plinko_drop_type_tickets_frame'):SetVisible(HoN_Plinko.theme.plinkoDropBackers)
	GetWidget("plinko_drop_type_gold_icon"):SetTexture(HoN_Plinko.theme.plinkoDropGold)
	GetWidget("plinko_drop_type_tickets_icon"):SetTexture(HoN_Plinko.theme.plinkoDropTicket)
	GetWidget("plinko_nav_signs"):SetTexture(HoN_Plinko.theme.navSigns)

	GetWidget("plinko_puck"):SetTexture(HoN_Plinko.theme.puck)
	for i=1,3 do
		GetWidget("plinko_puck_trail_"..i):SetTexture(HoN_Plinko.theme.puck)
	end

	local bulbOrder1 = {1, 2, 3, 4}
	local bulbOrder2 = {8, 7, 6, 5}
	for i=1,4 do
		GetWidget("plinko_bulb_"..bulbOrder1[i]):SetTexture(HoN_Plinko.theme["bulb"..i])
		GetWidget("plinko_bulb_"..bulbOrder2[i]):SetTexture(HoN_Plinko.theme["bulb"..i])
	end

	GetWidget("plinko_prize_ticket_texture"):SetTexture(HoN_Plinko.theme.ticket)

	GetWidget("plinko_theme_overlay"):SetTexture(HoN_Plinko.theme.plinkoThemeOver)


	-- ticket redemption
	GetWidget("ticket_redemption_background"):SetTexture(HoN_Plinko.theme.ticketBack)
	GetWidget("ticket_redemption_counter"):SetTexture(HoN_Plinko.theme.ticketCounter)
	GetWidget("ticket_redemption_register"):SetTexture(HoN_Plinko.theme.ticketRegister)
	GetWidget("ticket_redemption_sign"):SetTexture(HoN_Plinko.theme.ticketSign)
	GetWidget("ticket_redemption_back"):SetTexture(HoN_Plinko.theme.back)
	for i=1,10 do
		GetWidget("prize_stand_"..i.."_stand_texture"):SetTexture(HoN_Plinko.theme.ticketShelve)
		GetWidget("prize_stand_"..i.."_cost_section_texture"):SetTexture(HoN_Plinko.theme.ticket)
	end
	GetWidget("ticket_theme_overlay"):SetTexture(HoN_Plinko.theme.ticketThemeOver)

	-- other
	groupfcall("plinko_button_outside", function(_, w, _) w:SetTexture(HoN_Plinko.theme.buttonOutside) end)
	groupfcall("plinko_button_inside", function(_, w, _) w:SetTexture(HoN_Plinko.theme.buttonInside) end)
	GetWidget("plinko_generic_dialog_panel"):SetTexture(HoN_Plinko.theme.dropdown)
end

function HoN_Plinko:UpdateCurrencySelection(dontUpdateTickets)
	local gold = HoN_Plinko.currency == "gold"

	-- backgrounds/selections
	if (gold) then
		GetWidget("plinko_drop_type_gold_icon"):SetRenderMode("normal")
		GetWidget("plinko_drop_type_gold_icon"):SetColor("1 1 1")
		GetWidget("plinko_drop_type_gold_frame"):SetColor("#996102")
		GetWidget("plinko_drop_type_gold_frame"):SetBorderColor("#996102")

		if (HoN_Plinko.mouseOverCurrency == "tickets") then
			GetWidget("plinko_drop_type_tickets_icon"):SetRenderMode("grayscale")
			GetWidget("plinko_drop_type_tickets_icon"):SetColor(".6 .6 .6")
			GetWidget("plinko_drop_type_tickets_frame"):SetColor("#802800")
			GetWidget("plinko_drop_type_tickets_frame"):SetBorderColor("#802800")
		else
			GetWidget("plinko_drop_type_tickets_icon"):SetRenderMode("grayscale")
			GetWidget("plinko_drop_type_tickets_icon"):SetColor(".3 .3 .3")
			GetWidget("plinko_drop_type_tickets_frame"):SetColor("#461b07")
			GetWidget("plinko_drop_type_tickets_frame"):SetBorderColor("#461b07")
		end
	else
		if (HoN_Plinko.mouseOverCurrency == "gold") then
			GetWidget("plinko_drop_type_gold_icon"):SetRenderMode("grayscale")
			GetWidget("plinko_drop_type_gold_icon"):SetColor(".6 .6 .6")
			GetWidget("plinko_drop_type_gold_frame"):SetColor("#802800")
			GetWidget("plinko_drop_type_gold_frame"):SetBorderColor("#802800")
		else
			GetWidget("plinko_drop_type_gold_icon"):SetRenderMode("grayscale")
			GetWidget("plinko_drop_type_gold_icon"):SetColor(".3 .3 .3")
			GetWidget("plinko_drop_type_gold_frame"):SetColor("#461b07")
			GetWidget("plinko_drop_type_gold_frame"):SetBorderColor("#461b07")
		end

		GetWidget("plinko_drop_type_tickets_icon"):SetRenderMode("normal")
		GetWidget("plinko_drop_type_tickets_icon"):SetColor("1 1 1")
		GetWidget("plinko_drop_type_tickets_frame"):SetColor("#996102")
		GetWidget("plinko_drop_type_tickets_frame"):SetBorderColor("#996102")
	end

	-- costs and colors
	GetWidget("plinko_gold_drop_cost"):SetText(HoN_Plinko.dropGoldCost)
	if ((not dontUpdateTickets) and (not HoN_Plinko.dropping)) then
		GetWidget("plinko_ticket_drop_cost"):SetText(HoN_Plinko.dropTicketCost)
		GetWidget("plinko_drop_ticket_count"):SetText(HoN_Plinko.tickets)
	end

	if (tonumber(UIGoldCoins()) < HoN_Plinko.dropGoldCost) then
		GetWidget("plinko_gold_drop_cost"):SetColor("red")
	else
		GetWidget("plinko_gold_drop_cost"):SetColor("white")
	end

	if ((not dontUpdateTickets) and (not HoN_Plinko.dropping)) then
		if (HoN_Plinko.tickets < HoN_Plinko.dropTicketCost) then
			GetWidget("plinko_ticket_drop_cost"):SetColor("red")
		else
			GetWidget("plinko_ticket_drop_cost"):SetColor("white")
		end
	end
end

local function UpdateProcessing(status)
	-- 0 = IDLE, 1 = SENDING, 2 = SUCCESS, 3 = ERROR
	if (status == 1) then
		if (not HoN_Plinko.processingVisible) then
			GetWidget("plinko_processing"):FadeIn(200)
			GetWidget("plinko_input_blocker"):SetVisible(1)
			HoN_Plinko.processingVisible = true
		end
	else
		if (HoN_Plinko.processingVisible) then
			GetWidget("plinko_processing"):FadeOut(200)
			GetWidget("plinko_input_blocker"):SetVisible(0)
			HoN_Plinko.processingVisible = nil
		end
	end
end

-- start up, load DBs, etc
function HoN_Plinko:InitializePlinko()
	if (not HoN_Plinko.pastPathsDB) then
		HoN_Plinko.pastPathsDB = GetDBEntry('PastPlinkoPaths') or {}
	end
	LoadPlinkoDB()
	if (not pathDatabase) then
		Echo("^r^:ERROR NO PATH DATABASE FOUND!")
	end

	-- setup and populate the themeing system
	HoN_Plinko.theme = BuildPlinkoTheme()
	PopulatePlinkoTheme()


	if (not HoN_Plinko.dropping) then -- we aren't resuming a drop
		HoN_Plinko.puck_widget = GetWidget("plinko_puck")
		for i=1,3 do
			HoN_Plinko.puck_trail_widgets[i] = GetWidget("plinko_puck_trail_"..i)
		end
	end

	HoN_Plinko.plinkoOpen = true

	GetWidget('plinko'):Sleep(400, function() 
		HoN_Plinko:FadePlinkoSection(true, 300)
		GetWidget('plinko_close_button'):FadeIn(300) 
	end)

	HoN_Plinko:StartBulbAnimation()
end

function HoN_Plinko:ShowPlinko()
	-- set the loading throbber visible
	--GetWidget("plinko_processing"):FadeIn(200)

	-- drops will be repopulated by the form results
	--GetWidget("plinko_drops"):SetText("-")
	--GetWidget("plinko_input_blocker"):SetVisible(1)

	-- submit the form
	if (not HoN_Plinko.dropping) then -- only submit the form if we aren't mid-drop
		SubmitForm('PlinkoMain', 'cookie', Client.GetCookie())
	else
		HoN_Plinko:PopulateBuckets()	-- repopulate the buckets to make effects work, since we won't be getting a response
	end

	-- the selected currency
	HoN_Plinko.currency = GetDBEntry("PlinkoCurrency") or "gold"

	-- update the graphics based on the selected currency
	HoN_Plinko:UpdateCurrencySelection()

	-- Play the sound!
	local gamePhase = AtoN(interface:UICmd('GetCurrentGamePhase()'))
	if gamePhase <= 3 then
		PlayMusic(HoN_Plinko.theme.sndPlinkoTheme, true)
	end
	PlaySound(HoN_Plinko.theme.sndEnterPlinko)
	interface:UICmd("Play2DSFXSound('"..HoN_Plinko.theme.sndAmbience.."', 1, 3, true)")

	-- reset this on show
	HoN_Plinko.dontPromptDrop = false
end

function HoN_Plinko:HidePlinko()
	-- for i=1, 6 do
	-- 	GetWidget("plinko_tier_info_"..i):SetVisible(0)
	-- end
end

function HoN_Plinko:SetCurrency(currency)
	if (HoN_Plinko.currency ~= currency) then
		HoN_Plinko.dontPromptDrop = false
	end

	HoN_Plinko.currency = currency

	GetDBEntry("PlinkoCurrency", HoN_Plinko.currency, true)

	HoN_Plinko:UpdateCurrencySelection()
end

function HoN_Plinko:MouseOverCurrency(currency)
	HoN_Plinko.mouseOverCurrency = currency

	HoN_Plinko:UpdateCurrencySelection()
end

function HoN_Plinko:MouseOutCurrency()
	HoN_Plinko.mouseOverCurrency = ""

	HoN_Plinko:UpdateCurrencySelection()
end

local function PlinkoMainResult(status_code, tiers, userTickets, goldCost, ticketCost, rewardsCount, lastUpdateTime)
	-- hide the processing thing
	--GetWidget("plinko_processing"):FadeOut(200)
	--GetWidget("plinko_input_blocker"):SetVisible(0)

	if (tonumber(status_code) == 1) then
		tiers = explode(",", tiers)
		for i,t in ipairs(tiers) do
			HoN_Plinko.bucketTiers[i] = tonumber(t)
		end

		lastUpdateTime = explode(',', lastUpdateTime or '')
		for i,t in ipairs(lastUpdateTime) do
			local localtime = GetDBEntry('PlinkoUpdateTier'..HoN_Plinko.bucketTiers[i])
			HoN_Plinko.plinkoUpdateTier[HoN_Plinko.bucketTiers[i]] = t or '0'

			local w = GetWidget('plinko_tier_info_'..i..'_new')
			if localtime == nil or tonumber(localtime) < tonumber(t) then
				if w ~= nil then w:SetVisible(true) end
			else
				if w ~= nil then w:SetVisible(false) end
			end
		end

		rewardsCount = explode(',', rewardsCount or '')
		for i,c in ipairs(rewardsCount) do
			HoN_Plinko.rewardsCount[i] = tonumber(c)

			if HoN_Plinko.rewardsCount[i] == 0 then
				local w = GetWidget('plinko_tier_info_'..i..'_new')
				if w ~= nil then w:SetVisible(false) end
			end
		end

		local showed = GetDBEntry('PlinkoTipShowed')
		GetWidget('plinko_lootlist_tip'):SetVisible(showed == nil)
		
		-- drop_packages = explode(",", drop_packages)
		-- for i=1,((#drop_packages)/4) do
		-- 	local baseIndex = (i - 1) * 4

		-- 	HoN_Plinko.dropPackages[i] = {
		-- 		['productID'] = drop_packages[baseIndex+1],
		-- 		['goldCost'] = drop_packages[baseIndex+2],
		-- 		['silverCost'] = drop_packages[baseIndex+3],
		-- 		['drops'] = drop_packages[baseIndex+4]
		-- 	}
		-- end

		HoN_Plinko.dropGoldCost = tonumber(goldCost)
		HoN_Plinko.dropTicketCost = tonumber(ticketCost)
		HoN_Plinko.tickets = tonumber(userTickets)

		HoN_Plinko:UpdateCurrencySelection()

		-- HoN_Plinko:PopulateDropPackages()
		HoN_Plinko:PopulateBuckets()
	else
		HoN_Plinko:GenericDialog(nil, nil, "plinko_request_net_error_title", "plinko_area_net_error", "general_retry", "general_leave",
			function()
				-- set the loading throbber visible
				--GetWidget("plinko_processing"):FadeIn(200)

				-- drops will be repopulated by the form results
				--GetWidget("plinko_drops"):SetText("-")
				--GetWidget("plinko_input_blocker"):SetVisible(1)

				-- submit the form
				SubmitForm('PlinkoMain', 'cookie', Client.GetCookie())
			end,
			function()
				-- leave plinko
				UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'plinko', nil, false)
			end
		)
	end
end
interface:RegisterWatch("PlinkoMainResult", function(_, ...) PlinkoMainResult(...) end)

local function PlinkoMainStatus(status)
	UpdateProcessing(tonumber(status))

	if (status == 3) then -- error
		-- hide the processing thing
		--GetWidget("plinko_processing"):FadeOut(200)
		--GetWidget("plinko_input_blocker"):SetVisible(0)

		HoN_Plinko:GenericDialog(nil, nil, "plinko_request_net_error_title", "plinko_area_net_error", "general_retry", "general_leave",
			function()
				-- set the loading throbber visible
				--GetWidget("plinko_processing"):FadeIn(200)

				-- drops will be repopulated by the form results
				--GetWidget("plinko_drops"):SetText("-")
				--GetWidget("plinko_input_blocker"):SetVisible(1)

				-- submit the form
				SubmitForm('PlinkoMain', 'cookie', Client.GetCookie())
			end,
			function()
				-- leave plinko
				UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'plinko', nil, false)
			end
		)
	end
end
interface:RegisterWatch("PlinkoMainStatus", function(_, ...) PlinkoMainStatus(...) end)

function HoN_Plinko:ShowTicketRedemption()
	-- refresh to accurately reflect anything won during plinko
	interface:UICmd("ChatRefreshUpgrades(); ClientRefreshUpgrades(); ServerRefreshUpgrades();")

	-- set the loading throbber visible
	--GetWidget("plinko_processing"):FadeIn(200)

	-- tickets will be repopulated by the form results
	GetWidget("plinko_ticket_count"):SetText("-")
	--GetWidget("plinko_input_blocker"):SetVisible(1)

	-- submit the form
	SubmitForm('TicketRedemptionMain', 'cookie', Client.GetCookie())

	-- play sounds!
	local gamePhase = AtoN(interface:UICmd('GetCurrentGamePhase()'))
	if gamePhase <= 3 then
		PlayMusic(HoN_Plinko.theme.sndTicketTheme, true)
	end
	PlaySound(HoN_Plinko.theme.sndEnterTicket)
	interface:UICmd("StopSound(3);")
	interface:UICmd("StopSound(4);")
end

function HoN_Plinko:HideTicketRedemption()
	for i=1,10 do
		GetWidget("prize_stand_"..i):SetVisible(0)
	end
end

local function TicketRedemptionMainResult(status_code, items, tickets)
	-- hide the processing thing
	--GetWidget("plinko_processing"):FadeOut(200)
	--GetWidget("plinko_input_blocker"):SetVisible(0)

	if (tonumber(status_code) == 51) then
		HoN_Plinko.tickets = tonumber(tickets)
		GetWidget("plinko_ticket_count"):SetText(HoN_Plinko.tickets)

		HoN_Plinko.ticketPrizes = {}
		items = explode(",", items)
		for i=1,((#items)/6) do
			local baseIndex = (i - 1) * 6

			local item={
				['id'] = tonumber(items[baseIndex+1]),
				['ticketCost'] = tonumber(items[baseIndex+2]),
				['productID'] = items[baseIndex+3],
				['name'] = items[baseIndex+4],
				['type'] = items[baseIndex+5],
				['path'] = items[baseIndex+6]
			}
			table.insert(HoN_Plinko.ticketPrizes, item)
		end

		HoN_Plinko:PopulateTicketPrizes()
	else
		HoN_Plinko:GenericDialog(nil, nil, "plinko_request_net_error_title", "plinko_area_net_error", "general_retry", "general_leave",
			function()
				-- set the loading throbber visible
				--GetWidget("plinko_processing"):FadeIn(200)

				-- tickets will be repopulated by the form results
				GetWidget("plinko_ticket_count"):SetText("-")
				--GetWidget("plinko_input_blocker"):SetVisible(1)

				-- submit the form
				SubmitForm('TicketRedemptionMain', 'cookie', Client.GetCookie())
			end,
			function()
				-- go back to plinko
				HoN_Plinko:FadeRedemptionSection(false, 250)
				HoN_Plinko:BlockPlinkoInput(350)
				GetWidget('ticket_redemption_section'):Sleep(150, function()
					HoN_Plinko:FadePlinkoSection(true, 250)
				end)
			end
		)
	end
end
interface:RegisterWatch("TicketRedemptionMainResult", function(_, ...) TicketRedemptionMainResult(...) end)

local function TicketRedemptionMainStatus(status)
	UpdateProcessing(tonumber(status))

	if (status == 3) then -- error
		-- hide the processing thing
		--GetWidget("plinko_processing"):FadeOut(200)
		--GetWidget("plinko_input_blocker"):SetVisible(0)

		HoN_Plinko:GenericDialog(nil, nil, "plinko_request_net_error_title", "plinko_area_net_error", "general_retry", "general_leave",
			function()
				-- set the loading throbber visible
				--GetWidget("plinko_processing"):FadeIn(200)

				-- tickets will be repopulated by the form results
				GetWidget("plinko_ticket_count"):SetText("-")
				--GetWidget("plinko_input_blocker"):SetVisible(1)

				-- submit the form
				SubmitForm('TicketRedemptionMain', 'cookie', Client.GetCookie())
			end,
			function()
				-- go back to plinko
				HoN_Plinko:FadeRedemptionSection(false, 250)
				HoN_Plinko:BlockPlinkoInput(350)
				GetWidget('ticket_redemption_section'):Sleep(150, function()
					HoN_Plinko:FadePlinkoSection(true, 250)
				end)
			end
		)
	end
end
interface:RegisterWatch("TicketRedemptionMainStatus", function(_, ...) TicketRedemptionMainStatus(...) end)

local function PlinkoDropResult(status_code, randomTier, productID, productName, productPath, productType, ticketAmount, tickets, userGold, productsExhausted)
	-- hide the processing thing
	--GetWidget("plinko_processing"):FadeOut(200)
	--GetWidget("plinko_input_blocker"):SetVisible(0)

	-- if they closed plinko while it was processing
	if (not HoN_Plinko.plinkoOpen) then
		return
	end

	if (tonumber(status_code) == 1) then -- success
		-- cause upgrades and such to refresh
		-- isntead of refreshing upgrades, manually trigger how much gold they have
		-- (uses new function now)
		-- only refresh upgrades when leaving the plinko area
		UpdateGold(userGold)
		--interface:UICmd("ChatRefreshUpgrades(); ClientRefreshUpgrades(); ServerRefreshUpgrades();")

		HoN_Plinko.prizeTierWon = tonumber(randomTier)
		HoN_Plinko.prizeProductWon = {
			["name"] = productName,
			["path"] = productPath,
			["productID"] = productID,
			["type"] = productType,
			["numTickets"] = tonumber(ticketAmount),
			["productsExhausted"] = AtoB(productsExhausted)
		}

		
		-- we are going to cheat this, this will show their tickets without any prize they may have won yet
		-- it will be updated after the prize is shown
		if (HoN_Plinko.requestedCurrency == "tickets") then
			HoN_Plinko.tickets = HoN_Plinko.tickets - HoN_Plinko.dropTicketCost
		end
		HoN_Plinko:UpdateCurrencySelection()

		-- this is actually how many tickets they have, the prize presentation calls HoN_Plinko:UpdateCurrencySelection() later
		HoN_Plinko.tickets = tonumber(tickets)

		local acceptableBuckets = {}
		for i,v in ipairs(HoN_Plinko.bucketTiers) do
			if (v == HoN_Plinko.prizeTierWon) then
				table.insert(acceptableBuckets, i)
			end
		end

		-- if there are no acceptable buckets for the tier they won, they are gonna get a random one-- sorry
		local bucket = math.random(1,6)
		if (#acceptableBuckets ~= 0) then
			bucket = acceptableBuckets[math.random(1, #acceptableBuckets)]
		end

		HoN_Plinko:PlinkoToGoal(bucket)

		GetDBEntry('PlinkoUpdateTier'..HoN_Plinko.prizeTierWon, HoN_Plinko.plinkoUpdateTier[HoN_Plinko.prizeTierWon], true)
		
	else -- error
		-- no retry on this, most error codes have a reason and a retry probably wouldn't fix it
		HoN_Plinko:GenericDialog(nil, nil, "plinko_request_net_error_title", "plinko_drop_request_status_"..status_code, "general_okay")
	end
end
interface:RegisterWatch("PlinkoDropResult", function(_, ...) PlinkoDropResult(...) end)

local function PlinkoDropStatus(status)
	UpdateProcessing(tonumber(status))

	if (status == 3) then -- error
		-- hide the processing thing
		--GetWidget("plinko_processing"):FadeOut(200)
		--GetWidget("plinko_input_blocker"):SetVisible(0)

		HoN_Plinko:GenericDialog(nil, nil, "plinko_request_net_error_title", "plinko_request_net_error", "general_retry", "general_okay",
			function()
				-- set the loading throbber visible
				--GetWidget("plinko_processing"):FadeIn(200)

				-- drops will be repopulated by the form results
				--GetWidget("plinko_input_blocker"):SetVisible(1)

				-- submit the form
				SubmitForm('PlinkoDrop', 'cookie', Client.GetCookie(), 'currency', HoN_Plinko.requestedCurrency)
			end
		)
	end
end
interface:RegisterWatch("PlinkoDropStatus", function(_, ...) PlinkoDropStatus(...) end)

function HoN_Plinko:StartBulbAnimation()
	for i=1,6 do
		if HoN_Plinko.bucketTiers[i] == HoN_Plinko.prizeTierWon then
			GetWidget('plinko_tier_info_'..i..'_new'):SetVisible(false)
		end
	end
	StartRandomBulbAnimation()
	GetWidget("plinko_bulb_1"):Sleep(BULB_ANIMATION_CHANGE_TIME, function() HoN_Plinko:StartBulbAnimation() end)
end

-- function HoN_Plinko:PopulateDropPackages()
-- 	for i=1,3 do
-- 		if (HoN_Plinko.dropPackages[i]) then
-- 			local costLabel = GetWidget("purchase_drops_"..i.."_cost")
-- 			costLabel:SetText(HoN_Plinko.dropPackages[i].goldCost)
-- 			if (tonumber(UIGoldCoins()) >= tonumber(HoN_Plinko.dropPackages[i].goldCost)) then
-- 				costLabel:SetColor("white")
-- 				GetWidget("purchase_drops_"..i.."_x"):SetColor("white")
-- 			else
-- 				costLabel:SetColor("red")
-- 				GetWidget("purchase_drops_"..i.."_x"):SetColor("red")
-- 			end

-- 			groupfcall("purchase_drops_"..i.."_amount_group", function(_,w,_) w:SetText(HoN_Plinko.dropPackages[i].drops) end)

-- 			GetWidget("purchase_drops_"..i):SetVisible(1)
-- 		else
-- 			GetWidget("purchase_drops_"..i):SetVisible(0)
-- 		end
-- 	end
-- end

-- local function BuyDropsResults(_, ...)
-- 	-- hide the processing thing
-- 	GetWidget("plinko_processing"):FadeOut(200)
-- 	GetWidget("plinko_input_blocker"):SetVisible(0)

-- 	GetWidget("plinko_section"):UnregisterWatch("MicroStoreStatus")
-- 	GetWidget("plinko_section"):UnregisterWatch("MicroStoreResults")

-- 	-- cause upgrades and such to refresh
-- 	if (tonumber(arg[16]) > 0) then -- error
-- 		HoN_Plinko:GenericDialog(nil, nil, "plinko_request_net_error_title", "mstore_error"..arg[16], "general_retry", "general_okay",
-- 			function()
-- 				if (HoN_Plinko.dropPackageRequested) then
-- 					-- set the loading throbber visible
-- 					GetWidget("plinko_processing"):FadeIn(200)

-- 					-- block input during processing
-- 					GetWidget("plinko_input_blocker"):SetVisible(1)

-- 					-- register watches
-- 					GetWidget("plinko_section"):RegisterWatch("MicroStoreStatus", HoN_Plinko.buyDropsStatusFunc)
-- 					GetWidget("plinko_section"):RegisterWatch("MicroStoreResults", HoN_Plinko.buyDropsResultsFunc)

-- 					-- web request
-- 					SubmitForm('MicroStore', 'account_id', Client.GetAccountID(), 'request_code', '4', 'cookie', Client.GetCookie(), 'product_id', HoN_Plinko.dropPackageRequested, 'hostTime', HostTime(), 'currency', 0, 'type');
-- 				else
-- 					HoN_Plinko:GenericDialog(nil, nil, "plinko_request_net_error_title", "plinko_request_retry_error", "general_okay")
-- 				end
-- 			end
-- 		)
-- 	elseif (tonumber(arg[2]) == 3) then -- success
-- 		-- refresh upgrades, might not be needed
-- 		interface:UICmd("ChatRefreshUpgrades(); ClientRefreshUpgrades(); ServerRefreshUpgrades();")

-- 		-- show a dialog to confirm success
-- 		HoN_Plinko:GenericDialog(nil, nil, "general_success", "plinko_drops_bought", "general_okay")
-- 	else -- This should never happen...
-- 		-- unknown error?
-- 		HoN_Plinko:GenericDialog(nil, nil, "plinko_request_net_error_title", "plinko_request_status_0", "general_okay")
-- 	end
-- end

-- local function BuyDropsStatus(status)
-- 	if (tonumber(status) == 3) then -- error
-- 		-- hide the processing thing
-- 		GetWidget("plinko_processing"):FadeOut(200)
-- 		GetWidget("plinko_input_blocker"):SetVisible(0)

-- 		GetWidget("plinko_section"):UnregisterWatch("MicroStoreStatus")
-- 		GetWidget("plinko_section"):UnregisterWatch("MicroStoreResults")

-- 		HoN_Plinko:GenericDialog(nil, nil, "plinko_request_net_error_title", "plinko_request_net_error", "general_retry", "general_okay",
-- 			function()
-- 				if (HoN_Plinko.dropPackageRequested) then
-- 					-- set the loading throbber visible
-- 					GetWidget("plinko_processing"):FadeIn(200)

-- 					-- block input during processing
-- 					GetWidget("plinko_input_blocker"):SetVisible(1)

-- 					-- register watches
-- 					GetWidget("plinko_section"):RegisterWatch("MicroStoreStatus", HoN_Plinko.buyDropsStatusFunc)
-- 					GetWidget("plinko_section"):RegisterWatch("MicroStoreResults", HoN_Plinko.buyDropsResultsFunc)

-- 					-- web request
-- 					SubmitForm('MicroStore', 'account_id', Client.GetAccountID(), 'request_code', '4', 'cookie', Client.GetCookie(), 'product_id', HoN_Plinko.dropPackageRequested, 'hostTime', HostTime(), 'currency', 0, 'type');
-- 				else
-- 					HoN_Plinko:GenericDialog(nil, nil, "plinko_request_net_error_title", "plinko_request_retry_error", "general_okay")
-- 				end
-- 			end
-- 		)
-- 	end
-- end

-- function HoN_Plinko:BuyDropPackage(id)
-- 	if (HoN_Plinko.dropPackages[id]) then
-- 		-- register watches for the results onto 'plinko_section', it won't cause any issues on there
-- 		-- we will remove them once we get a result or error from the status
-- 		if (tonumber(UIGoldCoins()) >= tonumber(HoN_Plinko.dropPackages[id].goldCost)) then
-- 			HoN_Plinko:GenericDialog(
-- 				"general_are_you_sure",
-- 				"/ui/fe2/plinko/puck.tga", Translate("plinko_drops_amount", "amount", HoN_Plinko.dropPackages[id].drops),
-- 				Translate("plinko_buy_drops_body", "drops", (HoN_Plinko.dropPackages[id].drops), "goldCost", HoN_Plinko.dropPackages[id].goldCost),
-- 				"general_confirm", "general_cancel",
-- 				function()
-- 					-- set the loading throbber visible
-- 					GetWidget("plinko_processing"):FadeIn(200)

-- 					-- block input during processing
-- 					GetWidget("plinko_input_blocker"):SetVisible(1)

-- 					-- save this for retrys
-- 					HoN_Plinko.dropPackageRequested = HoN_Plinko.dropPackages[id].productID
-- 					HoN_Plinko.buyDropsStatusFunc = BuyDropsStatus
-- 					HoN_Plinko.buyDropsResultsFunc = BuyDropsResults

-- 					-- get rid of the purchase box
-- 					GetWidget("plinko_token_purchase"):DoEventN(0)

-- 					-- register watches
-- 					GetWidget("plinko_section"):RegisterWatch("MicroStoreStatus", BuyDropsStatus)
-- 					GetWidget("plinko_section"):RegisterWatch("MicroStoreResults", BuyDropsResults)

-- 					-- web request
-- 					SubmitForm('MicroStore', 'account_id', Client.GetAccountID(), 'request_code', '4', 'cookie', Client.GetCookie(), 'product_id', HoN_Plinko.dropPackages[id].productID, 'category_id', 2, 'hostTime', HostTime(), 'currency', 0, 'type')
-- 				end
-- 			)
-- 		else
-- 			HoN_Plinko:GenericDialog(nil, nil, "plinko_buy_drops_gold", "plinko_buy_drops_gold_body", "general_okay")
-- 		end
-- 	end
-- end

-- shutdown, erase DBs, etc (to not waste memory, paths take a decent chunk)
function HoN_Plinko:ShutdownPlinko()
	-- interrupt any sleeps for pathing so things can't break
	if (not HoN_Plinko.dropping) then -- we aren't mid drop, stop everything
		if (HoN_Plinko.puck_widget) then
			HoN_Plinko.puck_widget:SetVisible(0)
			HoN_Plinko.puck_widget:Sleep(1, function() end)
			for i=1,3 do
				HoN_Plinko.puck_trail_widgets[i]:SetVisible(0)
			end
		end

		-- Hide the prize icons, they will be reshown once they have info to populate with
		for i=1,6 do
			GetWidget('plinko_tier_info_'..i):SetVisible(0)
		end

		-- hide any dialogs if they are visible
		GetWidget("plinko_generic_dialog"):DoEventN(0)
		--GetWidget("plinko_token_purchase"):DoEventN(0)

		-- dump the things we don't need anymore
		HoN_Plinko.plinkoOpen = false
		HoN_Plinko.path = nil
		HoN_Plinko.puck_widget = nil
		HoN_Plinko.puck_trail_widgets = {}

		-- flush the past paths DB
		if (HoN_Plinko.pastPathsDB) then
			GetDBEntry('PastPlinkoPaths', HoN_Plinko.pastPathsDB, true)
		end

		HoN_Plinko:FadePlinkoSection(false, 300)
		HoN_Plinko:FadeRedemptionSection(false, 300)
		GetWidget('plinko_close_button'):FadeOut(300)
	
		playBgMusic()

		interface:UICmd("StopSound(3);")
		interface:UICmd("StopSound(4);")

		HoN_Plinko.prizeTierWon = nil
		HoN_Plinko.prizeProductWon = nil
		HoN_Plinko:SetDropping(false)

		HoN_Plinko:StopBulbAnimation()
		pathDatabase = nil
	else -- stop nonessential things tso that we can continue the drop when we return
		-- hide any dialogs if they are visible
		GetWidget("plinko_generic_dialog"):DoEventN(0)

		HoN_Plinko.plinkoOpen = false

		-- flush the past paths DB
		if (HoN_Plinko.pastPathsDB) then
			GetDBEntry('PastPlinkoPaths', HoN_Plinko.pastPathsDB, true)
		end

		HoN_Plinko:FadePlinkoSection(false, 300)
		HoN_Plinko:FadeRedemptionSection(false, 300)
		GetWidget('plinko_close_button'):FadeOut(300)

	
		playBgMusic()

		interface:UICmd("StopSound(3);")
		interface:UICmd("StopSound(4);")

		HoN_Plinko:StopBulbAnimation()
		pathDatabase = nil
	end

	-- refresh for anything won during plinko
	if (IsLoggedIn()) then
		interface:UICmd("ChatRefreshUpgrades(); ClientRefreshUpgrades(); ServerRefreshUpgrades();")
	end
end

function HoN_Plinko:StopBulbAnimation()
	HoN_Plinko.bulbAnimationNumber = nil
	HoN_Plinko.bulbAnimationStep = 1

	-- interrupt the sleep
	GetWidget("plinko_bulb_2"):Sleep(1, function() end)
end

function HoN_Plinko:DropPlinko()
	if (not HoN_Plinko.dropping) then
		-- save this for resubmission and checks after the drop returns
		HoN_Plinko.requestedCurrency = HoN_Plinko.currency

		if ((HoN_Plinko.requestedCurrency == "gold") and (tonumber(UIGoldCoins()) < HoN_Plinko.dropGoldCost)) then
			HoN_Plinko:GenericDialog(nil, nil,
				"plinko_no_gold_header", "plinko_no_gold_body",
				"general_okay"
			)

			-- we aren't performing the drop
			return
		elseif ((HoN_Plinko.requestedCurrency == "tickets") and (HoN_Plinko.tickets < HoN_Plinko.dropTicketCost)) then
			HoN_Plinko:GenericDialog(nil, nil,
				"plinko_no_tickets_header", "plinko_no_tickets_body",
				"general_okay"
			)

			-- we aren't performing the drop
			return
		end

		-- check don't remind
		if (HoN_Plinko.dontPromptDrop) then
			SubmitForm('PlinkoDrop', 'cookie', Client.GetCookie(), 'currency', HoN_Plinko.requestedCurrency)
		else
			local headerText, bodyText, icon

			headerText = "plinko_drop_"..HoN_Plinko.requestedCurrency.."_header"

			if (HoN_Plinko.requestedCurrency == "gold") then
				bodyText = Translate("plinko_drop_gold_body", "cost", HoN_Plinko.dropGoldCost)
				--icon = "/ui/fe2/plinko/gold_icon_large.tga"
			elseif (HoN_Plinko.requestedCurrency == "tickets") then
				bodyText = Translate("plinko_drop_tickets_body", "cost", HoN_Plinko.dropTicketCost)
				--icon = "/ui/fe2/plinko/ticket_icon_large.tga"
			end

			if (headerText and bodyText) then
				HoN_Plinko:GenericDialog(nil, nil,
					headerText, bodyText,
					"general_okay", "general_cancel",
					function()
						-- set the loading throbber visible
						--GetWidget("plinko_processing"):FadeIn(200)

						-- drops will be repopulated by the form results
						--GetWidget("plinko_input_blocker"):SetVisible(1)

						-- submit the form
						SubmitForm('PlinkoDrop', 'cookie', Client.GetCookie(), 'currency', HoN_Plinko.requestedCurrency)
					end,
					nil,
					"plinko_dont_remind_session",
					HoN_Plinko.dontPromptDrop,
					function(val)
						HoN_Plinko.dontPromptDrop = val
					end
				)
			end
		end
	end
end

function HoN_Plinko:SetDropping(dropping)
	HoN_Plinko.dropping = dropping

	for w,_ in pairs(HoN_Plinko.dropDisableButtons) do
		w:DoEvent()
	end
end


function HoN_Plinko:PlinkoToGoal(goalID)
	if (not HoN_Plinko.pastPathsDB) then
		HoN_Plinko.pastPathsDB = GetDBEntry('PastPlinkoPaths') or {}
	end

	if ((not pathDatabase[goalID]) or (#(pathDatabase[goalID]) == 0)) then -- someone broke something
		Echo("^r^:No paths to the given goal! Not playing a path!")
		HoN_Plinko:PresentPrize()
		return
	end

	HoN_Plinko:SetDropping(true)

	-- create a table of the past paths with the paths used as keys
	local pastPathsUsed = {}
	if (HoN_Plinko.pastPathsDB[goalID]) then
		for _,v in ipairs(HoN_Plinko.pastPathsDB[goalID]) do
			pastPathsUsed[v] = true
		end
	end

	-- build a list of the paths we can use
	local numAvailablePaths = #(pathDatabase[goalID])
	local possiblePathNums = {}
	if (NUM_PAST_PATHS < numAvailablePaths) then
		for i=1,numAvailablePaths do
			if (not pastPathsUsed[i]) then
				table.insert(possiblePathNums, i)
			end
		end
	else
		for i=1,numAvailablePaths do
			table.insert(possiblePathNums, i)
		end
	end

	-- randomly select one of the available paths
	local randomPath = math.random(1, numAvailablePaths)
	if (#possiblePathNums > 0) then
		randomPath = possiblePathNums[math.random(1, #possiblePathNums)]
	end

	HoN_Plinko.path = pathDatabase[goalID][randomPath]

	-- remove me when testing is done, but the releaseStage test is for safety
	-- if (not GetCvarBool("releaseStage_stable")) then
	-- 	Echo("^o^:Plinko Path "..goalID.." "..randomPath)
	-- end

	-- store off the selected path so we can't 'randomly' select it again in the next X times
	if (not HoN_Plinko.pastPathsDB[goalID]) then HoN_Plinko.pastPathsDB[goalID] = {} end
	table.insert(HoN_Plinko.pastPathsDB[goalID], 1, randomPath)
	-- prune the past paths
	while (#HoN_Plinko.pastPathsDB[goalID] > NUM_PAST_PATHS) do
		table.remove(HoN_Plinko.pastPathsDB[goalID], #HoN_Plinko.pastPathsDB[goalID])
	end
	
	PlayPlinkoPath()
end

function HoN_Plinko:PopulateBuckets()
	for i=1, 6 do
		if (HoN_Plinko.bucketTiers[i] and tierInfo[HoN_Plinko.bucketTiers[i]]) then
			local tier = HoN_Plinko.bucketTiers[i]
			local tInfo = tierInfo[tier]

			GetWidget("plinko_tier_info_"..i.."_image"):SetTexture(HoN_Plinko.theme['tier'..tier])
			if (NotEmpty(tInfo.effect)) then
				GetWidget("plinko_tier_info_"..i.."_effect"):SetEffect(tInfo.effect)
				GetWidget("plinko_tier_info_"..i.."_effect"):SetVisible(1)
			else
				GetWidget("plinko_tier_info_"..i.."_effect"):SetVisible(0)
			end

			GetWidget("plinko_tier_info_"..i):SetVisible(1)
		else
			GetWidget("plinko_tier_info_"..i):SetVisible(0)
		end
	end
end

function HoN_Plinko:HoverBucket(bucketID)
	if (HoN_Plinko.bucketTiers[bucketID] and tierInfo[HoN_Plinko.bucketTiers[bucketID]]) then
		local tier = HoN_Plinko.bucketTiers[bucketID]

		-- fix this up once we have things (textures, text, web hookup, etc)
		-- if (tier == 5 and not GetCvarBool('cl_GarenaEnable')) then
			-- Trigger('genericMainFloatingTip', 'true', '23h', HoN_Plinko.theme['tier'..tier], 'plinko_prize_tier_'..tier..'_naeu', 'plinko_prize_tier_'..tier..'_tip_naeu', '', '', '3h', '-1h')
		-- else
			Trigger('genericMainFloatingTip', 'true', '23h', HoN_Plinko.theme['tier'..tier], 'plinko_prize_tier_'..tier, 'plinko_prize_tier_'..tier..'_tip', '', '', '3h', '-1h')
		-- end
	end
end

function HoN_Plinko:PlayMerricAnimation(animName)
	local merricPanel = nil

	if (GetWidget("ticket_redemption_section"):IsVisible()) then
		merricPanel = GetWidget("ticker_redemption_merric")
	else
		merricPanel = GetWidget("plinko_merric")
	end

	merricPanel:SetAnim(animName)
end

function HoN_Plinko:PopulateTicketPrizes()
	for i=1,10 do
		if (HoN_Plinko.ticketPrizes[i]) then
			GetWidget("prize_stand_"..i.."_prize_image"):SetTexture(HoN_Plinko.ticketPrizes[i].path)

			local costWidget = GetWidget("prize_stand_"..i.."_ticket_cost")
			costWidget:SetFont(GetFontSizeForWidth(tostring(HoN_Plinko.ticketPrizes[i].ticketCost), costWidget:GetWidth(), 14))
			costWidget:SetText(HoN_Plinko.ticketPrizes[i].ticketCost)

			-- set product type
			local typeText = ""
			if (HoN_Plinko.ticketPrizes[i].type == "Couriers") then
				typeText = Translate("general_courier")
			elseif (HoN_Plinko.ticketPrizes[i].type == "Bundle") then
				typeText = Translate("general_bundle")
			else
				typeText = Translate("general_product_type_"..ProductTypeToPrefix(HoN_Plinko.ticketPrizes[i].type, true))
			end
			GetWidget("prize_stand_"..i.."_type"):SetText(typeText)

			-- cost font color, gray = owned, red = too expensive, white otherwise
			if (not Client.IsProductOwned(ProductTypeToPrefix(HoN_Plinko.ticketPrizes[i].type)..HoN_Plinko.ticketPrizes[i].name)) then
				if (HoN_Plinko.ticketPrizes[i].ticketCost <= HoN_Plinko.tickets) then
					costWidget:SetColor("white")
				else
					costWidget:SetColor("red")
				end
				GetWidget("prize_stand_"..i.."_owned_section"):SetVisible(0)
				GetWidget("prize_stand_"..i.."_cost_section"):SetVisible(1)
			else
				costWidget:SetColor("gray")
				GetWidget("prize_stand_"..i.."_owned_section"):SetVisible(1)
				GetWidget("prize_stand_"..i.."_cost_section"):SetVisible(0)
			end

			GetWidget("prize_stand_"..i):FadeIn(100)
		else
			GetWidget("prize_stand_"..i):SetVisible(0)
		end
	end
end

function HoN_Plinko:RedeemPrize(prizeID)
	local pInfo = HoN_Plinko.ticketPrizes[prizeID]
	if (pInfo and (not Client.IsProductOwned(ProductTypeToPrefix(pInfo.type)..pInfo.name)) and (HoN_Plinko.tickets >= pInfo.ticketCost)) then
		PlaySound('/shared/sounds/ui/button_click_01.wav')
		HoN_Plinko:GenericDialog(
			"general_are_you_sure",
			pInfo.path, "mstore_product"..pInfo.productID.."_name",
			Translate("plinko_ticket_purchase", "ticketCost", pInfo.ticketCost, "ticketsLeft", HoN_Plinko.tickets - pInfo.ticketCost),
			"general_confirm", "general_cancel",
			function()
				-- set the loading throbber visible
				--GetWidget("plinko_processing"):FadeIn(200)

				-- block input during processing
				--GetWidget("plinko_input_blocker"):SetVisible(1)

				-- save this for retrys
				HoN_Plinko.ticketPrizeRequested = pInfo.productID

				-- web request
				SubmitForm("RedeemTickets", "cookie", Client.GetCookie(), "id", pInfo.productID)
			end
		)
	else
		PlaySound('/shared/sounds/ui/button_click_06.wav')
	end
end

local function RedeemTicketsResult(status_code, tickets, isGrabBag, grabBagTheme, grabBagIDs, grabBagTypes, grabBagPaths, grabBagNames)
	--GetWidget("plinko_processing"):FadeOut(200)
	--GetWidget("plinko_input_blocker"):SetVisible(0)

	if (tonumber(status_code) == 51) then -- success
		PlaySound(HoN_Plinko.theme.sndTicketRedeem)

		HoN_Plinko.tickets = tonumber(tickets)
		GetWidget("plinko_ticket_count"):SetText(HoN_Plinko.tickets)

		-- refresh upgrades
		interface:UICmd("ChatRefreshUpgrades(); ClientRefreshUpgrades(); ServerRefreshUpgrades();")

		-- refreshes ticket cost
		HoN_Plinko:PopulateTicketPrizes()

		-- again, this time update owned products after the refresh is hopefully done
		GetWidget("ticket_redemption_section"):Sleep(1500, function()
			HoN_Plinko:PopulateTicketPrizes()
		end)

		-- show grab bag
		if (AtoB(isGrabBag)) then
			HoN_Grabbag:GrabBagResults("plinko_grab_bag_container", grabBagTheme, grabBagIDs, grabBagNames, grabBagPaths, grabBagTypes)
		else
			-- show a dialog to confirm success
			HoN_Plinko:GenericDialog(nil, nil, "general_success", "plinko_tickets_redeemed", "general_okay")
		end
	else -- error
		-- no retry on this, most error codes have a reason and a retry probably wouldn't fix it
		HoN_Plinko:GenericDialog(nil, nil, "plinko_request_net_error_title", "mstore_error"..status_code, "general_okay")
	end
end
interface:RegisterWatch("RedeemTicketsResult", function(_, ...) RedeemTicketsResult(...) end)

local function RedeemTicketsStatus(status)
	UpdateProcessing(tonumber(status))

	if (status == 3) then -- error
		-- hide the processing thing
		--GetWidget("plinko_processing"):FadeOut(200)
		--GetWidget("plinko_input_blocker"):SetVisible(0)

		HoN_Plinko:GenericDialog(nil, nil, "plinko_request_net_error_title", "plinko_request_net_error", "general_retry", "general_okay",
			function()
				if (HoN_Plinko.ticketPrizeRequested) then
					-- set the loading throbber visible
					--GetWidget("plinko_processing"):FadeIn(200)

					-- block input during processing
					--GetWidget("plinko_input_blocker"):SetVisible(1)

					-- web request
					SubmitForm("RedeemTickets", "cookie", Client.GetCookie(), "id", HoN_Plinko.ticketPrizeRequested)
				else
					HoN_Plinko:GenericDialog(nil, nil, "plinko_request_net_error_title", "plinko_request_retry_error", "general_okay")
				end
			end
		)
	end
end
interface:RegisterWatch("RedeemTicketsStatus", function(_, ...) RedeemTicketsStatus(...) end)

function HoN_Plinko:HoverPrize(prizeID)
	if (HoN_Plinko.ticketPrizes[prizeID]) then
		local pInfo = HoN_Plinko.ticketPrizes[prizeID]
		local bodyText = ""

		local nameSplit = explode(".", pInfo.name)
		local fullProduct = ProductTypeToPrefix(pInfo.type)..pInfo.name

		if (pInfo.type == "Alt Avatar") then
			bodyText = Translate("plinko_ticket_tooltip_aa", "heroName", Translate("mstore_"..nameSplit[1].."_name"))
		elseif (pInfo.type == "EAP") then
			bodyText = Translate("plinko_ticket_tooltip_eap", "heroName", Translate("mstore_product"..pInfo.productID.."_name"))
		elseif (pInfo.type == "Couriers") then
			bodyText = Translate("plinko_ticket_tooltip_courier")
		elseif (pInfo.type == "Bundle") then
			bodyText = Translate("plinko_ticket_tooltip_bundle")
		else
			bodyText = Translate("plinko_ticket_tooltip_"..ProductTypeToPrefix(pInfo.type, true))
		end

		if (Client.IsProductOwned(fullProduct)) then
			bodyText = bodyText.."\n\n"..Translate("plinko_ticket_tooltip_owned")
		elseif (HoN_Plinko.tickets < pInfo.ticketCost) then
			bodyText = bodyText.."\n\n"..Translate("plinko_ticket_tooltip_cost")
		end


		Trigger('genericMainFloatingTip', 'true', '20h', pInfo.path, 'mstore_product'..pInfo.productID..'_name', bodyText, '', '', '3h', '-2h')
	end
end

function plinkoTestPrizeModel()
	HoN_Plinko.prizeProductWon = {
		type		= 'Alt Avatar',
		path		= '/heroes/legionnaire/alt/icon.tga',
		productID	= 842,
		name		= 'Hero_Legionnaire.Alt',
		-- numTick
	}

	HoN_Plinko.prizeTierWon = 1
	HoN_Plinko:PresentPrize()
end

function HoN_Plinko:PresentPrize()
	if (HoN_Plinko.prizeProductWon) then

		if (HoN_Plinko.prizeProductWon.type ~= "Ticket") then -- tickets have their own thing
			GetWidget("plinko_prize_info_tickets"):SetVisible(0)
			GetWidget("plinko_clipper"):SetVisible(1)

			local chestTable = {
				[1] = {
					["model"] = "/ui/fe2/plinko/models/chests/diamond/chest.mdf",
					["effect"] = "/ui/fe2/plinko/models/chests/diamond/diamond_chest.effect",
					['modelPos'] = "0 -1650 0",
					['openTime'] = 1750,

					["rewardSound"] = HoN_Plinko.theme.sndTier1ChestOpn,
					["rewardSoundDelay"] = 1275,

					['clipPanelY'] = "-2.25h",
				},
				[2] = {
					["model"] = "/ui/fe2/plinko/models/chests/gold/chest.mdf",
					["effect"] = "/ui/fe2/plinko/models/chests/gold/gold_chest.effect",
					['modelPos'] = "0 -1650 0",
					['openTime'] = 1750,

					["rewardSound"] = HoN_Plinko.theme.sndTier2ChestOpn,
					["rewardSoundDelay"] = 1275,

					['clipPanelY'] = "-2.15h",
				},
				[3] = {
					["model"] = "/ui/fe2/plinko/models/chests/silver/chest.mdf",
					["effect"] = "/ui/fe2/plinko/models/chests/silver/silver_chest.effect",
					['modelPos'] = "0 -1650 0",
					['openTime'] = 1200,

					["rewardSound"] = HoN_Plinko.theme.sndTier3ChestOpn,
					["rewardSoundDelay"] = 550,

					['clipPanelY'] = "-2.15h",
				},
				[4] = {
					["model"] = "/ui/fe2/plinko/models/chests/bronze/chest.mdf",
					["effect"] = "/ui/fe2/plinko/models/chests/bronze/bronze_chest.effect",
					['modelPos'] = "0 -1650 0",
					['openTime'] = 1200,

					["rewardSound"] = HoN_Plinko.theme.sndTier3ChestOpn,
					["rewardSoundDelay"] = 550,

					['clipPanelY'] = "-1.4h",
				},
				[5] = {
					["model"] = "/ui/fe2/plinko/models/chests/bronze/chest.mdf",
					["effect"] = "/ui/fe2/plinko/models/chests/bronze/bronze_chest.effect",
					['modelPos'] = "0 -1650 0",
					['openTime'] = 1200,

					["rewardSound"] = HoN_Plinko.theme.sndTier3ChestOpn,
					["rewardSoundDelay"] = 550,

					['clipPanelY'] = "-1.4h",
				},
				[6] = {
					["model"] = "/ui/fe2/plinko/models/chests/bronze/chest.mdf",
					["effect"] = "/ui/fe2/plinko/models/chests/bronze/bronze_chest.effect",
					['modelPos'] = "0 -1650 0",
					['openTime'] = 1200,

					["rewardSound"] = HoN_Plinko.theme.sndTier3ChestOpn,
					["rewardSoundDelay"] = 550,

					['clipPanelY'] = "-1.4h",
				},
			}

			if (chestTable[HoN_Plinko.prizeTierWon]) then
				-- get widget needed to be populated
				local master = GetWidget("plinko_winner_is_you")
				local chest = GetWidget("plinko_chest")

				-- determine if we are going to be showing an icon or a model
				local displayType, model
				if ((HoN_Plinko.prizeProductWon.type == "Alt Avatar") or (HoN_Plinko.prizeProductWon.type == "Couriers") or (HoN_Plinko.prizeProductWon.type == "Hero") or (HoN_Plinko.prizeProductWon.type == "EAP")) then
					displayType = "model"
					model = true
					GetWidget("plinko_prize_info_icon"):SetVisible(0) -- just set the other set invisible here
				else
					displayType = "icon"
					model = false
					GetWidget("plinko_prize_info_model"):SetVisible(0) -- just set the other set invisible here
				end

				local productSection = GetWidget("plinko_prize_info_"..displayType)
				local effect = GetWidget("plinko_prize_effect_"..displayType)
				local typeLabel = GetWidget("plinko_prize_type_"..displayType)
				local nameLabel = GetWidget("plinko_prize_name_"..displayType)

				-- info from the chest table
				local chestSetup = chestTable[HoN_Plinko.prizeTierWon]

				-- populate everything before starting the animation
				chest:SetModel(chestSetup.model)
				chest:UICmd("SetCameraPos('"..chestSetup.modelPos.."');")
				chest:SetAnim("idle")
				chest:SetEffect(chestSetup.effect or "")
				chest:SetVisible(1)
				-- hide the effect, but start it here so it has time to get started, but doesn't show
				effect:SetVisible(0)
				effect:SetEffect("/ui/fe2/plinko/effects/plinko_chest_gold.effect")

				productSection:SetY(productSection:GetHeight())
				productSection:SetVisible(1)

				GetWidget("plinko_prize_icon_"..displayType):SetTexture(HoN_Plinko.prizeProductWon.path)

				nameLabel:SetText(Translate("mstore_product"..HoN_Plinko.prizeProductWon.productID.."_name"))
				
				local courier = (HoN_Plinko.prizeProductWon.type == "Couriers")
				if (courier) then -- Courier's product type code is aa, so we have to translate them special
					typeLabel:SetText(Translate("general_courier"))
				elseif (HoN_Plinko.prizeProductWon.type == "Bundle") then
					typeLabel:SetText(Translate("general_bundle"))
				else
					typeLabel:SetText(Translate("general_product_type_"..ProductTypeToPrefix(HoN_Plinko.prizeProductWon.type, true)))
				end

				-- for chat colors, make the font the color of the chat color
				if (HoN_Plinko.prizeProductWon.type == "Chat Color") then
					nameLabel:SetColor(GetChatNameColorStringFromUpgrades(ProductTypeToPrefix(HoN_Plinko.prizeProductWon.type)..HoN_Plinko.prizeProductWon.name))
					-- glow is set when the product slides up, so it doesn't show above the clip
					nameLabel:SetGlow(false)
					nameLabel:SetGlowColor(GetChatNameGlowColorStringFromUpgrades(ProductTypeToPrefix(HoN_Plinko.prizeProductWon.type)..HoN_Plinko.prizeProductWon.name))
				else
					nameLabel:SetColor('white')
					nameLabel:SetGlow(false)
				end
			
				-- model only population stuff
				if (model) then
					-- load entity defs if we need to
					if (not GetCvar('_entityDefinitionsLoaded')) then
						if (courier) then --courier are weird with their entity defs
							interface:UICmd("LoadEntityDefinitionsFromFolder('/items/basic/ground_familiar/')")
						else
							interface:UICmd("LoadEntityDefinitionsFromFolder('"..string.sub(HoN_Plinko.prizeProductWon.path, 1, string.find(HoN_Plinko.prizeProductWon.path, "/", 9)).."')")
						end
					end

					-- setup the model
					local modelWidget = GetWidget("plinko_prize_model")
					if (not courier) then
						-- get the raw hero_name for special case effect paths
						local heroName = explode(".", HoN_Plinko.prizeProductWon.name)

						modelWidget:UICmd("SetCameraPos('30 10000 65');")
						modelWidget:UICmd("SetModelPos(GetHeroStorePosFromProduct('"..HoN_Plinko.prizeProductWon.name.."'));")
						modelWidget:UICmd("SetModelAngles(GetHeroStoreAnglesFromProduct('"..HoN_Plinko.prizeProductWon.name.."'));")
						modelWidget:UICmd("SetModelScale(GetHeroStoreScaleFromProduct('"..HoN_Plinko.prizeProductWon.name.."'));")
						modelWidget:UICmd("SetModel(GetHeroPreviewModelPathFromProduct('"..HoN_Plinko.prizeProductWon.name.."'));")
						if (heroName[1] ~= "Hero_Empath" and heroName[1] ~= "Hero_Gemini" and heroName[1] ~= "Hero_ShadowBlade" and heroName[1] ~= "Hero_Dampeer") then
							modelWidget:UICmd("SetEffectIndexed(GetHeroStorePassiveEffectPathFromProduct('"..HoN_Plinko.prizeProductWon.name.."'), 0);")
						else
							modelWidget:UICmd("SetEffectIndexed(GetHeroPassiveEffectPathFromProduct('"..HoN_Plinko.prizeProductWon.name.."'), 0);")
						end
						-- glow effect
						modelWidget:UICmd("SetEffectIndexed('/shared/effects/glow_gold_high.effect', 1);")
						modelWidget:UICmd("SetTeamColor('1 0 0');")
					else -- couriers don't have store stuff set
						modelWidget:UICmd("SetCameraPos('30 10000 30');")
						modelWidget:UICmd("SetModelPos(GetHeroPreviewPosFromProduct('"..HoN_Plinko.prizeProductWon.name.."'));")
						modelWidget:UICmd("SetModelAngles(GetHeroPreviewAnglesFromProduct('"..HoN_Plinko.prizeProductWon.name.."'));")
						modelWidget:UICmd("SetModelScale(GetHeroPreviewScaleFromProduct('"..HoN_Plinko.prizeProductWon.name.."'));")
						modelWidget:UICmd("SetModel(GetHeroPreviewModelPathFromProduct('"..HoN_Plinko.prizeProductWon.name.."'));")
						modelWidget:UICmd("SetEffectIndexed(GetHeroPassiveEffectPathFromProduct('"..HoN_Plinko.prizeProductWon.name.."'), 0);")
						-- glow effect
						modelWidget:UICmd("SetEffectIndexed('/shared/effects/glow_gold_high.effect', 1);")
					end
				end

				GetWidget("plinko_clipper"):SetY(chestSetup.clipPanelY)

				-- hide the normal close button
				GetWidget("plinko_prize_close_button"):SetVisible(0)

				-- do the animation
				master:FadeIn(250)
				master:Sleep(500, function()
					chest:SetAnim("open")
					-- play the chest appear sound
					PlaySound(HoN_Plinko.theme.sndChestApprBase..math.random(1,2)..'.ogg')
					chest:Sleep(chestSetup.rewardSoundDelay, function() PlaySound(chestSetup.rewardSound) end)

					master:Sleep(chestSetup.openTime, function()
						PlaySound(HoN_Plinko.theme.sndCongratsWin)
						-- set glow here so it doesn't overflow above the clip during animation
						if (HoN_Plinko.prizeProductWon.type == "Chat Color") then
							local upgrades = ProductTypeToPrefix(HoN_Plinko.prizeProductWon.type)..HoN_Plinko.prizeProductWon.name
							nameLabel:SetGlow(GetChatNameGlowFromUpgrades(upgrades))
							nameLabel:SetGlowColor(GetChatNameGlowColorStringFromUpgrades(upgrades))
							nameLabel:SetBackgroundGlow(GetChatNameBackgroundGlowFromUpgrades(upgrades))
						end
						-- enable the effect here so that it doesn't overflow above the clip
						effect:SetVisible(1)
						-- slide the info and stuff up
						productSection:SlideY("-7.5h", 500)

						master:Sleep(500, function()
							GetWidget("plinko_prize_close_button"):FadeIn(150)
							HoN_Plinko:UpdateCurrencySelection()
							HoN_Plinko:SetDropping(false)
						end)
					end)
				end)
			else
				HoN_Plinko:SetDropping(false)
			end
		else -- Ticket presentation
			GetWidget("plinko_prize_info_tickets"):SetVisible(1)
			GetWidget("plinko_clipper"):SetVisible(0)
			GetWidget("plinko_chest"):SetVisible(0)

			local master = GetWidget("plinko_winner_is_you")
			master:FadeIn(250)

			-- IT'S RAINING TICKETS!
			local ticketList = {}
			local rain_master = GetWidget("plinko_ticket_make_it_rain")
			for i=1,math.min(HoN_Plinko.prizeProductWon.numTickets, 150) do -- put the amount of tickets won in here
				local size = math.random(30, 60)
				size = (size / 10.0)

				ticketList[i] = {
					["x"] = math.random(0,100).."%",
					["y"] = math.random(-100, 0).."%",
					["height"] = size.."h",
					["sort"] = size,
					["rot"] = math.random(-20, 20),
					["fallTime"] = math.random(3500, 6000),
					["rotTime"] = math.random(650, 750),
					["color"] = (size/6).." "..(size/6).." "..(size/6)
				}
			end

			table.sort(ticketList, function(a, b)
				return a.sort < b.sort
			end)

			for index,ticket in ipairs(ticketList) do
				rain_master:Instantiate("ticket_rain",
					"droplet", index,
					"x", ticket.x,
					"y", ticket.y,
					"height", ticket.height,
					"rot", ticket.rot,
					"fallTime", ticket.fallTime,
					"rotTime", ticket.rotTime,
					"color", ticket.color,
					"ticketTexture", HoN_Plinko.theme.ticket
				)
			end

			local labelWidget = GetWidget("plinko_ticket_won_amount")
			local imageWidget = GetWidget("plinko_tickets_won_image")
			local effectWidget = GetWidget("plinko_prize_effect_tickets")
			local buttonWidget = GetWidget("plinko_prize_close_button")
			local exhaustedWidget = GetWidget("plinko_ticket_products_exhausted")

			labelWidget:SetText(Translate("plinko_won_tickets", "amount", HoN_Plinko.prizeProductWon.numTickets))
			labelWidget:SetVisible(0)
			exhaustedWidget:SetVisible(0)
			buttonWidget:SetVisible(0)
			effectWidget:SetEffect("")
			imageWidget:SetY("100%")

			-- play a ticket sound effect based on tier
			if (HoN_Plinko.prizeTierWon >= 6) then
				PlaySound(HoN_Plinko.theme.sndWinTickets)
			else
				PlaySound(HoN_Plinko.theme.sndManyTickets)
			end
			rain_master:Sleep(150, function()
				labelWidget:FadeIn(250)
				if (HoN_Plinko.prizeProductWon.productsExhausted) then
					exhaustedWidget:FadeIn(250)
				end
				imageWidget:SlideY("13h", 350)
				rain_master:Sleep(200, function()
					effectWidget:SetEffect("/ui/fe2/plinko/effects/plinko_chest_silver.effect")
					rain_master:Sleep(150, function()
						HoN_Plinko:SetDropping(false)
						HoN_Plinko:UpdateCurrencySelection()
						buttonWidget:FadeIn(100)
					end)
				end)
			end)
		end
	end
end

function HoN_Plinko:BlockPlinkoInput(timeToBlock)
	local blocker = GetWidget("plinko_input_blocker")
	blocker:SetVisible(1)
	blocker:Sleep(timeToBlock, function() blocker:SetVisible(0) end)
end

--- System for all the buttons that need to be disabled when dropping, and reenabled otherwise
-- just makes life easier, and automation easier. Also can't be clicked when dropping
function HoN_Plinko:CreateDropDisabledButton(widget, normalColor, overColor, disableColor, onclick, ...)
	if (not HoN_Plinko.dropDisableButtons[widget]) then
		HoN_Plinko.dropDisableButtons[widget] = false

		-- set callbacks
		widget:SetCallback('onmouseover', function()
			HoN_Plinko.dropDisableButtons[widget] = true
			if (not HoN_Plinko.dropping) then
				PlaySound('/shared/sounds/ui/ccpanel/button_over_01.wav')
				for _,w in ipairs(arg) do
					w:SetColor(overColor)
				end
			end
		end)

		widget:SetCallback('onmouseout', function()
			HoN_Plinko.dropDisableButtons[widget] = false
			if (not HoN_Plinko.dropping) then
				for _,w in ipairs(arg) do
					w:SetColor(normalColor)
				end
			end
		end)

		widget:SetCallback('onclick', function()
			if (not HoN_Plinko.dropping) then
				PlaySound('/shared/sounds/ui/button_click_01.wav')
				onclick()
			end
		end)

		widget:SetCallback('onevent', function()
			for _,w in ipairs(arg) do
				if (HoN_Plinko.dropping) then
					w:SetColor(disableColor)
				else
					if (not HoN_Plinko.dropDisableButtons[widget]) then
						w:SetColor(normalColor)
					else
						w:SetColor(overColor)
					end
				end
			end
		end)
	end
end

function HoN_Plinko:AddDropDisableButton(widget)
	if (not HoN_Plinko.dropDisableButtons[widget]) then
		HoN_Plinko.dropDisableButtons[widget] = true

		widget:SetCallback('onevent', function()
			widget:SetEnabled(not HoN_Plinko.dropping)
		end)
	end
end

function HoN_Plinko:GenericDialog(title, image, header, body, button1, button2, button1Func, button2Func, checkBoxTitle, checkBoxVal, checkBoxFunc)
	local titleWidget = GetWidget("plinko_generic_dialog_title")
	if (title) then
		titleWidget:SetText(Translate(title))
		titleWidget:SetVisible(1)
	else
		titleWidget:SetVisible(0)
	end

	local imageWidget, headerWidget, imageHeaderWidget = GetWidget("plinko_generic_dialog_image"), GetWidget("plinko_generic_dialog_header"), GetWidget("plinko_generic_dialog_header_image")
	if (image and NotEmpty(image)) then
		imageWidget:SetTexture(image)
		imageWidget:SetVisible(1)

		if (header) then
			headerWidget:SetVisible(0)
			imageHeaderWidget:SetVisible(1)
			imageHeaderWidget:SetText(Translate(header))
		else
			headerWidget:SetVisible(0)
			imageHeaderWidget:SetVisible(0)
		end
	else
		imageWidget:SetVisible(0)

		if (header) then
			imageHeaderWidget:SetVisible(0)
			headerWidget:SetVisible(1)
			headerWidget:SetText(Translate(header))
		else
			imageHeaderWidget:SetVisible(0)
			headerWidget:SetVisible(0)
		end
	end

	local bodyWidget = GetWidget("plinko_generic_dialog_body")
	if (body) then
		bodyWidget:SetText(Translate(body))
		bodyWidget:SetVisible(1)
	else
		bodyWidget:SetVisible(0)
	end

	local checkboxHelper = GetWidget("plinko_generic_dialog_checbox_helper")
	local checkboxLabel = GetWidget("plinko_generic_dialog_checbox_label")
	local checboxCheck = GetWidget("plinko_generic_dialog_checbox_checkmark")

	if (checkBoxTitle and NotEmpty(checkBoxTitle) and (checkBoxVal ~= nil)) then
		checkboxLabel:SetText(Translate(checkBoxTitle))
		checboxCheck:SetVisible(checkBoxVal)
		if (checkBoxVal) then
			checkboxLabel:SetColor('#ffaa00')
		else
			checkboxLabel:SetColor('white')
		end

		checkboxHelper:SetCallback("onmouseover", function()
			PlaySound('/shared/sounds/ui/ccpanel/button_over_01.wav')
			checkboxLabel:SetColor('#3abde7')
		end)

		checkboxHelper:SetCallback("onmouseout", function()
			if (checkBoxVal) then
				checkboxLabel:SetColor('#ffaa00')
			else
				checkboxLabel:SetColor("white")
			end
		end)

		checkboxHelper:SetCallback("onclick", function()
			PlaySound('/shared/sounds/ui/ccpanel/button_close_02.wav')
			checkBoxVal = (not checkBoxVal)
			if (checkBoxFunc) then
				checkBoxFunc(checkBoxVal)
			end

			checboxCheck:SetVisible(checkBoxVal)
		end)

		GetWidget("plinko_generic_dialog_checbox"):SetVisible(1)
		bodyWidget:SetHeight("4.0h")
	else
		GetWidget("plinko_generic_dialog_checbox"):SetVisible(0)
		bodyWidget:SetHeight("5.5h")
	end

	local leftButton, rightButton = GetWidget("plinko_generic_dialog_left_button"), GetWidget("plinko_generic_dialog_right_button")
	if (button1 and button2) then
		groupfcall("plinko_generic_dialog_left_button_labels", function(_,w,_) w:SetText(Translate(button1)) end)
		leftButton:SetAlign("left")
		leftButton:SetVisible(1)

		groupfcall("plinko_generic_dialog_right_button_labels", function(_,w,_) w:SetText(Translate(button2)) end)
		rightButton:SetAlign("right")
		rightButton:SetVisible(1)
	elseif (button1) then
		groupfcall("plinko_generic_dialog_left_button_labels", function(_,w,_) w:SetText(Translate(button1)) end)
		leftButton:SetAlign("center")
		leftButton:SetVisible(1)

		rightButton:SetVisible(0)
	elseif (button2) then
		groupfcall("plinko_generic_dialog_right_button_labels", function(_,w,_) w:SetText(Translate(button2)) end)
		rightButton:SetAlign("center")
		rightButton:SetVisible(1)

		leftButton:SetVisible(0)
	else
		rightButton:SetVisible(0)
		leftButton:SetVisible(0)
	end

	leftButton:SetCallback("onclick", function()
		if (button1Func) then button1Func() end

		PlaySound('/shared/sounds/ui/ccpanel/button_close_02.wav')
		GetWidget("plinko_generic_dialog"):DoEventN(0)

		leftButton:ClearCallback("onclick")
	end)

	rightButton:SetCallback("onclick", function()
		if (button2Func) then button2Func() end
		
		PlaySound('/shared/sounds/ui/ccpanel/button_close_02.wav')
		GetWidget("plinko_generic_dialog"):DoEventN(0)

		rightButton:ClearCallback("onclick")
	end)

	GetWidget("plinko_generic_dialog"):DoEventN(1)
end

function HoN_Plinko:FadeRedemptionSection(isFadeIn, time)

	local merric = GetWidget('ticker_redemption_merric')
	local section = GetWidget('ticket_redemption_section')
	if isFadeIn then
		merric:SetVisible(false)
		section:FadeIn(time)
		merric:Sleep(time + 50, function() merric:SetVisible(true) end)
	else
		merric:SetVisible(false)
		section:FadeOut(time)
	end
end

function HoN_Plinko:FadePlinkoSection(isFadeIn, time)

	local merric = GetWidget('plinko_merric')
	local plinko = GetWidget('plinko_section')
	if isFadeIn then
		merric:SetVisible(false)
		plinko:FadeIn(time)
		merric:Sleep(time + 50, function() merric:SetVisible(true) end)
	else
		merric:SetVisible(false)
		plinko:FadeOut(time)
	end
end

function interface:HoNPlinkoF(func, ...)
	if (HoN_Plinko[func]) then
		print(HoN_Plinko[func](HoN_Plinko, ...))
	else
		print('HoNPlinkoF failed to find: ' .. tostring(func) .. '\n')
	end
end

local LootListCurrentTier = 0
local LootListCurrentPage = 0
local LootListTotalPage = 1
local LootListResourceCount = 0

local PAGE_ITEM_COUNT = 4
local PAGE_INDEX = 9

local function OnLootListResult(tier, target_index, first_item_index, count, names, types, paths, productIds)

	GetWidget('plinko_lootlist_mask'):SetVisible(false)
	tier = tonumber(tier)
	HoN_Plinko.rewardsItems[tier] = HoN_Plinko.rewardsItems[tier] or {}

	local itemsTable = HoN_Plinko.rewardsItems[tier]

	local names = explode(',', names)
	local types = explode(',', types)
	local paths = explode(',', paths)
	local productIds = explode(',', productIds)

	for i=1, count do
		if names[i] == nil and paths[i] == nil and types[i] == nil and productIds[i] == nil then
			itemsTable[first_item_index+i-1] = nil 
		else
			itemsTable[first_item_index+i-1] = {}
			itemsTable[first_item_index+i-1].name = names[i] or ''
			itemsTable[first_item_index+i-1].type = types[i] or ''
			itemsTable[first_item_index+i-1].path = paths[i] or ''
			itemsTable[first_item_index+i-1].productId = productIds[i] or ''

			-- Echo('------------------------------')
			-- Echo(tostring(i))
			-- Echo('name:'..itemsTable[first_item_index+i-1].name)
			-- Echo('type:'..itemsTable[first_item_index+i-1].type)
			-- Echo('path:'..itemsTable[first_item_index+i-1].path)
			-- Echo('productId:'..itemsTable[first_item_index+i-1].productId)
		end
	end

	local page = math.floor((target_index-1) / PAGE_ITEM_COUNT) + 1
	if tier == LootListCurrentTier then 
		HoN_Plinko:SelectLootListPage(page)
	end
	GetWidget('plinko_lootlist_tip'):SetVisible(false)
	GetDBEntry('PlinkoTipShowed', 1, true)

	GetDBEntry('PlinkoUpdateTier'..tier, HoN_Plinko.plinkoUpdateTier[tier], true)
	for i=1,6 do
		if HoN_Plinko.bucketTiers[i] == tier then
			GetWidget('plinko_tier_info_'..i..'_new'):SetVisible(false)
		end
	end
end
interface:RegisterWatch("LootListResult", function(_, ...) OnLootListResult(...) end)

function HoN_Plinko:ShowLootList(bucket)
	DeleteResourceContext('plinko_lootlist', true)
	LootListResourceCount = 0

	if HoN_Plinko.rewardsCount[bucket] == nil or HoN_Plinko.rewardsCount[bucket] < 1 then return end

	LootListCurrentTier = HoN_Plinko.bucketTiers[bucket]
	LootListTotalPage = math.ceil(HoN_Plinko.rewardsCount[bucket] / PAGE_ITEM_COUNT)

	local tInfo = tierInfo[LootListCurrentTier]

	GetWidget('plinko_lootlist_chest_name'):SetText(Translate('plinko_prize_tier_'..LootListCurrentTier))
	GetWidget('plinko_lootlist_chest_desc'):SetText(Translate('plinko_prize_tier_'..LootListCurrentTier..'_tip'))
	GetWidget('plinko_lootlist_chest_icon'):SetTexture(HoN_Plinko.theme['tier'..LootListCurrentTier])
	if (NotEmpty(tInfo.effect)) then
		GetWidget('plinko_lootlist_chest_effect'):SetEffect(tInfo.effect)
		GetWidget('plinko_lootlist_chest_effect'):SetVisible(1)
	else
		GetWidget('plinko_lootlist_chest_effect'):SetVisible(0)
	end
	for i=1,PAGE_ITEM_COUNT do
		GetWidget('plinko_lootlist_item_'..i):SetVisible(false)
	end

	GetWidget('plinko_lootlist'):SetVisible(true)

	HoN_Plinko:SelectLootListPage(1)
end

function HoN_Plinko:RefreshLootListPageIndex()
	local function SetSelected(i, selected)
		if selected then
			GetWidget('plinko_lootlist_page_index_'..i..'_label1'):SetColor('#ffab01')
			GetWidget('plinko_lootlist_page_index_'..i..'_label2'):SetColor('#ffab01')
			GetWidget('plinko_lootlist_page_index_'..i..'_label3'):SetColor('#ffab01')
		else
			GetWidget('plinko_lootlist_page_index_'..i..'_label1'):SetColor('#827474')
			GetWidget('plinko_lootlist_page_index_'..i..'_label2'):SetColor('#c2b4b4')
			GetWidget('plinko_lootlist_page_index_'..i..'_label3'):SetColor('#c2b4b4')
		end
	end

	local function SetText(i, text)
		GetWidget('plinko_lootlist_page_index_'..i..'_label1'):SetText(text)
		GetWidget('plinko_lootlist_page_index_'..i..'_label2'):SetText(text)
		GetWidget('plinko_lootlist_page_index_'..i..'_label3'):SetText(text)

		local number = tonumber(text)
		local w = GetWidget('plinko_lootlist_page_index_'..i)

		if number == nil then
			w:SetWidth('30i')
		elseif number >= 100 then
			w:SetWidth('36i')
		elseif number >= 10 then
			w:SetWidth('30i')
		elseif number > 0 then
			w:SetWidth('24i')
		end
	end


	local total = LootListTotalPage
	local selected = LootListCurrentPage

	GetWidget('plinko_lootlist_lastpage'):SetVisible(total > 1 and selected > 1)
	GetWidget('plinko_lootlist_nextpage'):SetVisible(total > 1 and selected < total)

	if total <= PAGE_INDEX then
		for i=1,PAGE_INDEX do
			GetWidget('plinko_lootlist_page_index_'..i):SetVisible(i <= total)
			SetSelected(i, i == selected)
			SetText(i, tostring(i))
		end
		GetWidget('plinko_lootlist_page_index_1'):SetVisible(total > 1)
	else
		local leftpoints = selected >= 5
		local rightpoints = selected < (total - 3)
		local wallnum = {}
		if leftpoints and rightpoints then
			for i= 3, PAGE_INDEX-2 do
				wallnum[i] = selected - 5 + i
			end
		elseif leftpoints then
			for i= 3, PAGE_INDEX-2 do
				wallnum[i] = total - 9 + i
			end
		elseif rightpoints then
			for i= 3, PAGE_INDEX-2 do
				wallnum[i] = i
			end
		else
			return
		end

		for i=1,PAGE_INDEX do
			GetWidget('plinko_lootlist_page_index_'..i):SetVisible(true)

			if i == 1 then
				SetSelected(i, i == selected)
				SetText(i, tostring(i))
			elseif i == PAGE_INDEX then
				SetSelected(i, total == selected)
				SetText(i, tostring(total))
			elseif i == 2 then
				if leftpoints then
					SetSelected(i, false)
					SetText(i, '...')
				else
					SetSelected(i, i == selected)
					SetText(i, tostring(i))
				end
			elseif i == PAGE_INDEX-1 then
				if rightpoints then
					SetSelected(i, false)
					SetText(i, '...')
				else
					SetSelected(i, (total-1) == selected)
					SetText(i, tostring(total-1))
				end
			else
				SetSelected(i, wallnum[i] == selected)
				SetText(i, tostring(wallnum[i]))
			end
		end	
	end
end

function HoN_Plinko:SelectLootListPage(page)
	if page < 1 or page > LootListTotalPage then return end

	if LootListResourceCount >= 10 then 
		DeleteResourceContext('plinko_lootlist', true)
		LootListResourceCount = 0
	end

	local itemsTable = HoN_Plinko.rewardsItems[LootListCurrentTier] or {}
	local index = (page -1) * PAGE_ITEM_COUNT + 1

	LootListCurrentPage = page
	HoN_Plinko:RefreshLootListPageIndex()

	if itemsTable[index] == nil then
		SubmitForm('LootList', 'cookie', Client.GetCookie(), 'tier_id', tostring(LootListCurrentTier), 'target_index', tostring(index))
		GetWidget('plinko_lootlist_mask'):SetVisible(true)
	else		
		local items = {}
		for i=1,PAGE_ITEM_COUNT do
			table.insert(items, itemsTable[index+i-1])
		end
		HoN_Plinko:SetLootItems(items)
	end	
	LootListResourceCount = LootListResourceCount + 1
end

function HoN_Plinko:OnClickLootListPageIndex(i)
	local text = GetWidget('plinko_lootlist_page_index_'..i..'_label1'):GetText()
	local pagenum = tonumber(text)

	if pagenum ~= nil then
		if pagenum ~= LootListCurrentPage then 
			HoN_Plinko:SelectLootListPage(pagenum)
		end
	elseif i == 2 then
		text = GetWidget('plinko_lootlist_page_index_3_label1'):GetText()
		pagenum = tonumber(text)
		HoN_Plinko:SelectLootListPage(pagenum - 1)
	elseif i == PAGE_INDEX-1 then
		text = GetWidget('plinko_lootlist_page_index_7_label1'):GetText()
		pagenum = tonumber(text)
		HoN_Plinko:SelectLootListPage(pagenum + 1)
	end
end

function HoN_Plinko:OnClickLootListPageLastOrNext(last)
	if last then
		if LootListCurrentPage > 1 then HoN_Plinko:SelectLootListPage(LootListCurrentPage - 1) end
	else
		if LootListCurrentPage < LootListTotalPage then HoN_Plinko:SelectLootListPage(LootListCurrentPage + 1) end
	end
end

function HoN_Plinko:SetLootItems(items)
	local function SetAvatar(i, name, type, path, productId)
		if not GetCvar('_entityDefinitionsLoaded') then
			LoadEntityDefinitionsFromFolder(string.sub(path, 1, string.find(path, "/", 9)))
		end

		GetWidget('plinko_lootlist_item_'..i..'_avatar'):SetVisible(true)
		GetWidget('plinko_lootlist_item_'..i..'_other'):SetVisible(false)

		

		local w = GetWidget('plinko_lootlist_item_'..i..'_avatar_model')
		w:SetModel(GetHeroStoreModelPathFromProduct(name))
		w:SetModelAngles(GetHeroStoreAnglesFromProduct(name))
		w:SetModelScale(GetHeroStoreScaleFromProduct(name))
		w:SetModelPos(GetHeroStorePosFromProduct(name))
		w:SetAnim('idle')

		local heroName = explode('.', name)
		if (heroName[1] ~= "Hero_Empath" and heroName[1] ~= "Hero_Gemini" and heroName[1] ~= "Hero_ShadowBlade" and heroName[1] ~= "Hero_Dampeer") then
			w:UICmd("SetEffectIndexed(GetHeroStorePassiveEffectPathFromProduct('"..name.."'), 0);")
		else
			w:UICmd("SetEffectIndexed(GetHeroPassiveEffectPathFromProduct('"..name.."'), 0);")
		end


		w = GetWidget('plinko_lootlist_item_'..i..'_avatar_icon')
		if type == 'aa' then
			w:SetTexture(GetHeroIcon2PathFromProduct(name))
		else
			w:SetTexture(GetHeroIconPathFromProduct(name))
		end
		

		w = GetWidget('plinko_lootlist_item_'..i..'_avatar_name')
		w:SetText(Translate('mstore_product'..productId..'_name'))

		w = GetWidget('plinko_lootlist_item_'..i..'_avatar_type')
		w:SetText(Translate('general_product_type_'..type))

		w = GetWidget('plinko_lootlist_item_'..i..'_avatar_owned')
		w:SetVisible(Client.IsProductOwned(type..'.'..name))
	end

	local function SetOther(i, name, type, path, productId, isChatColor)
		GetWidget('plinko_lootlist_item_'..i..'_avatar'):SetVisible(false)
		GetWidget('plinko_lootlist_item_'..i..'_other'):SetVisible(true)

		local w = GetWidget('plinko_lootlist_item_'..i..'_other_icon')
		w:SetTexture(path)

		w = GetWidget('plinko_lootlist_item_'..i..'_other_type')
		w:SetVisible(NotEmpty(type))
		w:SetText(Translate('general_product_type_'..type))

		w = GetWidget('plinko_lootlist_item_'..i..'_other_name')
		w:SetText(Translate('mstore_product'..productId..'_name'))

		if isChatColor then
			w:SetColor(GetChatNameColorStringFromUpgrades('cc.'..name))
			w:SetGlow(true)
			w:SetBackgroundGlow(GetChatNameBackgroundGlowFromUpgrades('cc.'..name))
			w:SetGlowColor(GetChatNameGlowColorStringFromUpgrades('cc.'..name))
		else
			w:SetColor('white')
			w:SetGlow(false)
			w:SetBackgroundGlow(false)
		end

		w = GetWidget('plinko_lootlist_item_'..i..'_other_owned')
		w:SetVisible(Client.IsProductOwned(type..'.'..name))
	end

	for i=1,PAGE_ITEM_COUNT do
		GetWidget('plinko_lootlist_item_'..i):SetVisible(items[i] ~= nil)

		if items[i] ~= nil then 
			local type = items[i].type
			local name = items[i].name
			local path = items[i].path
			local productId = items[i].productId

			if type == 'aa' or type =='h' or type == 'eap' then 
				SetAvatar(i, name, type, path, productId)
			else
				SetOther(i, name, type, path, productId, type == 'cc')
			end
		end
	end
end


