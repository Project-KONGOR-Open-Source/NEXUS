---------------------------------------------------------- 
--	Name: 		New Options Script	            		--				
--  Copyright 2015 Frostburn Studios					--
----------------------------------------------------------

-- local REFERRAL_URL = "!hon/whitelistfolder/referral_stats.php"

local OPTION_WRAP = false 				-- when scrolling the left menu with the wheel, should the options wrap end to end?

local SUBCATEGORY_SLIDE_TIME = 40 		-- how long to take to slide a sub category per slot it needs to slide (20 into the first, 40 into the second, etc.)
local SUBCATEGORY_EXTRA_SLIDE = "0.5h"	-- extra amount to slide over 0 to make it look like the sub category is sliding into the category text
local SUBCATEGORY_SPACING = "2.0h" 		-- the amount to slide each subcategory text

local SUBCATEGORY_ARROW_OFFSET = "4.6h"	-- the distance from the top of the options panel scroll area to the sub indiactor arrow

local SCROLL_AMOUNT = "2.0h"			-- how much to scroll in one tick of the mouse wheel
local SCROLL_BOTTOM_PADDING = "1.0h" 	-- amount beyond the size of the category to allow scrolling below the panel
local SCROLL_ARROW_AMOUNT = "0.25h"		-- how much to scroll when hovering the arrows, higher means faster

local debug_output = false

----------------------------------------------------------

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()
RegisterScript2('Options', '30')

HoN_Options = _G['HoN_Options'] or {}
HoN_Options.selectedCategory = nil
HoN_Options.selectedSubCategory = nil
HoN_Options.scrollPosition = 0
HoN_Options.maxPossibleScroll = 100
HoN_Options.canScroll = true
HoN_Options.categoryInfo = nil
HoN_Options.sliding = false
HoN_Options.scrollState = 0
HoN_Options.staffOffset = nil
HoN_Options.lastLoginStatus = false
HoN_Options.scrollMultiplyer = 1.0
HoN_Options.targetCategory = nil
HoN_Options.targetSubCategory = nil

-- pulled from game chat
HoN_Options.gameChatFilters = GetDBEntry('ingameChatOptions2', nil, true, false, true) or { 
		['muteAll']=0,
		['shortFormat']=true,
		['teamColor']=true,
		['splitChat']=false,
		['chatSounds']=true,
		['nickHighlight']=true,
		['heroIcons']=true,
		['heroNames']=false,
		['activeFilter']='2',
		['walkthroughState']=0,
		
		['1']={['0']=false, ['1']=true,  ['2']=true, 	['3']=true,  ['4']=true,  ['5']=true,  ['6']=true,  ['7']=true,  ['8']=true,  ['9']=true,  ['10']=false,  ['11']=true, ['12']=true,		['13']=true,  ['14']=true, ['15']=true,  ['16']=true, ['17']=true, ['31']=false, ['32']=true, ['33']=true,   ['666']=true},	-- All
		['2']={['0']=false, ['1']=false, ['2']=false,   ['3']=false, ['4']=true,  ['5']=true,  ['6']=false, ['7']=true,  ['8']=false, ['9']=false, ['10']=false,  ['11']=true, ['12']=false,	['13']=false, ['14']=true, ['15']=false, ['16']=true, ['17']=true, ['31']=false, ['32']=true, ['33']=false,  ['666']=true},	-- Team
		['3']={['0']=false, ['1']=false, ['2']=false, 	['3']=false, ['4']=false, ['5']=false, ['6']=false, ['7']=false, ['8']=false, ['9']=false, ['10']=false,  ['11']=true, ['12']=false,	['13']=false, ['14']=true, ['15']=true,  ['16']=true, ['17']=true, ['31']=false, ['32']=true, ['33']=true,   ['666']=true},	-- Whisper		

	}

local function LoadChatFiltersFromCvars()
	local map = {
		{
			lua = 'shortFormat',
			cvar = 'chat_filterShortFormat'
		},
		{
			lua = 'heroIcons',
			cvar = 'chat_filterHeroIcons'
		},
		{
			lua = 'chatSounds',
			cvar = 'chat_filterChatSounds'
		},
		{
			lua = 'heroNames',
			cvar = 'chat_filterHeroNames'
		},
	}
	local target = HoN_Options.gameChatFilters
	for i, v in ipairs(map) do
		local cvar = Cvar.GetCvar(v.cvar)
		if cvar == nil then
			SetSave(v.cvar, target[v.lua] or false, 'bool', true)
			cvar = Cvar.GetCvar(v.cvar)
		else
			target[v.lua] = cvar:GetBoolean()
		end

		if cvar == nil then
			Echo('^rLoadChatFiltersFromCvars() nil cvar named '..tostring(v.cvar))
		else
			Cvar.SetCloudSave(cvar, true)
		end
	end
end
LoadChatFiltersFromCvars()

function HoN_Options:SaveChatOptions()
	GetDBEntry('ingameChatOptions2', HoN_Options.gameChatFilters, true, false, false)
end

-- mod stuff --------------------------------
HoN_Options.currentModOffset = 0
HoN_Options.currentModSubOffset = 0
HoN_Options.modCategoryWidgets = {}

function HoN_Options:BumpModOffset()
	HoN_Options.currentModOffset = HoN_Options.currentModOffset + 1
	HoN_Options.currentModSubOffset = 0
end

function HoN_Options:BumpModSubOffset()
	HoN_Options.currentModSubOffset = HoN_Options.currentModSubOffset + 1
end

function HoN_Options:CreateModInsertionPoint(headerWidget)
	HoN_Options:BumpModSubOffset()
	headerWidget:Instantiate("mod_insertion_panel",
		'name', 'mod_insertion_panel_'..HoN_Options.currentModOffset..'_'..HoN_Options.currentModSubOffset
	)
	HoN_Options.modCategoryWidgets[headerWidget] = 'mod_insertion_panel_'..HoN_Options.currentModOffset..'_'..HoN_Options.currentModSubOffset
end

function HoN_Options:InsertToCurrentMod(widget)
	local wdgParent = GetWidget('mod_insertion_panel_'..HoN_Options.currentModOffset..'_'..HoN_Options.currentModSubOffset)
	if wdgParent then
		widget:SetParent(wdgParent)
	end
end

function HoN_Options:ExpandCollapseMod(headerWidget) 
	HoN_Options:ToggleWidgetVisibility(GetWidget(HoN_Options.modCategoryWidgets[headerWidget]))
end

-- end mod stuff ----------------------------

function HoN_Options:OnShow()
	interface:UICmd("OptionsOpen();")

	if (not HoN_Options.categoryInfo) then
		HoN_Options:BuildCategoryInfo()
	end


	if (HoN_Options.targetCategory and HoN_Options.targetSubCategory) then
		HoN_Options:SelectSubCategory(HoN_Options.targetCategory, HoN_Options.targetSubCategory)
		HoN_Options.targetCategory, HoN_Options.targetSubCategory = nil, nil
	elseif (not HoN_Options.selectedCategory) then
		HoN_Options:SelectSubCategory(1, 0)
	end
end

function HoN_Options:CatTableUpdate(isLoggedIn)
	isLoggedIn = AtoB(isLoggedIn)
	if (HoN_Options.lastLoginStatus ~= isLoggedIn) then
		HoN_Options.lastLoginStatus = isLoggedIn

		if (GetWidget("game_options"):IsVisible()) then
			if (HoN_Options.staffOffset and HoN_Options.selectedCategory and HoN_Options.selectedCategory == HoN_Options.staffOffset) then
				HoN_Options:SelectSubCategory(HoN_Options.categoryInfo.numCategories-1, 0)
			end

			HoN_Options.categoryInfo = nil
			HoN_Options:BuildCategoryInfo()	-- update the categories
		else
			HoN_Options.categoryInfo = nil
		end
	end
end

function HoN_Options:SetStaffOffset(offset)
	HoN_Options.staffOffset = tonumber(offset)
end

function HoN_Options:BuildCategoryInfo()
	HoN_Options.categoryInfo = {}

	-- get the number of categories, and the height of each subcatehory list
	HoN_Options.categoryInfo.numCategories = 1
	while (WidgetExists("options_cat"..HoN_Options.categoryInfo.numCategories) and GetWidget("options_cat"..HoN_Options.categoryInfo.numCategories):IsVisible()) do
		HoN_Options.categoryInfo[HoN_Options.categoryInfo.numCategories] = {}
		HoN_Options.categoryInfo[HoN_Options.categoryInfo.numCategories].height = 0
		if (not WidgetExists("options_main_category"..HoN_Options.categoryInfo.numCategories)) then
			if (debug_output) then Echo("^rWARNING: The left options menu has a 'options_cat"..HoN_Options.categoryInfo.numCategories.."' but a corresponding 'options_main_category"..HoN_Options.categoryInfo.numCategories.."' in the main panel was not found!") end
		end
		if (not WidgetExists("options_submenu"..HoN_Options.categoryInfo.numCategories)) then
			if (debug_output) then Echo("^rWARNING: The left options menu has a 'options_cat"..HoN_Options.categoryInfo.numCategories.."' but a corresponding 'options_submenu"..HoN_Options.categoryInfo.numCategories.."' was not found!") end
		end
		HoN_Options.categoryInfo.numCategories = HoN_Options.categoryInfo.numCategories + 1
	end
	HoN_Options.categoryInfo.numCategories = HoN_Options.categoryInfo.numCategories - 1 -- get rid of the extra that failed

	-- get the number of subcategories in each, and the position of said sub
	for i=1, HoN_Options.categoryInfo.numCategories do
		HoN_Options.categoryInfo[i].numSubs = 1
		while (WidgetExists("options_sub"..i..HoN_Options.categoryInfo[i].numSubs)) do
			if (WidgetExists("options_main_sub"..i..HoN_Options.categoryInfo[i].numSubs)) then
				HoN_Options.categoryInfo[i][HoN_Options.categoryInfo[i].numSubs] = GetWidget("options_main_sub"..i..HoN_Options.categoryInfo[i].numSubs):GetY()
				HoN_Options.categoryInfo[i].height = HoN_Options.categoryInfo[i].height + interface:GetHeightFromString(SUBCATEGORY_SPACING)
			else
				if (debug_output) then Echo("^rWARNING: The left options menu has a 'options_sub"..i..HoN_Options.categoryInfo[i].numSubs.."' for 'options_cat"..i.."' but a corresponding 'options_main_sub"..i..HoN_Options.categoryInfo[i].numSubs.."' was not found!") end
			end
			HoN_Options.categoryInfo[i].numSubs = HoN_Options.categoryInfo[i].numSubs + 1
		end
		HoN_Options.categoryInfo[i].numSubs = HoN_Options.categoryInfo[i].numSubs - 1 -- get rid of the extra that failedss
	end
	
	if (not GetCvarBool('ui_raf_enabled')) then
		GetWidget('options_cat7_parent'):SetVisible(0)
	end
	
	if (debug_output) then
		Echo("^gNum categories found: "..HoN_Options.categoryInfo.numCategories)
		Echo("^gSubs and positions in each:")
		for i=1, HoN_Options.categoryInfo.numCategories do
			Echo("\tCategory: "..i)
			for j=1, HoN_Options.categoryInfo[i].numSubs do
				Echo("\t\tSub: "..j.."-- Pos: "..(HoN_Options.categoryInfo[i][j] or "^rUnable to retrieve"))
			end
		end
	end
end

function HoN_Options:SelectCategory(categoryIndex)
	categoryIndex = tonumber(categoryIndex)
	if (categoryIndex < 0 or categoryIndex > HoN_Options.categoryInfo.numCategories) then
		return
	end

	if (HoN_Options.selectedCategory == categoryIndex) then return
	elseif (HoN_Options.sliding) then return end

	-- leave me here plz
	local oldCategory = HoN_Options.selectedCategory
	HoN_Options.selectedCategory = categoryIndex

	-- get the height of the catefory to set the max scroll
	----------------- old max scroll, based on height of all widgets -----------------
	-- HoN_Options.maxPossibleScroll = GetWidget("options_main_category"..categoryIndex):GetHeight() - GetWidget("options_main_holder"):GetHeight()
	-- HoN_Options.maxPossibleScroll = HoN_Options.maxPossibleScroll + interface:GetHeightFromString(SCROLL_BOTTOM_PADDING)

	-- if (HoN_Options.maxPossibleScroll < 0) then
	-- 	HoN_Options.canScroll = false
	-- else
	-- 	HoN_Options.canScroll = true
	-- end
	-----------------------------------------------------------------------------------
	-- new, based on position of last sub header OR the bottom of that sub section (if it's taller than the screen)
	HoN_Options:CalculateScroll(categoryIndex)
	-----------------------------------------------------------------------------------

	-- slide the main menu (right) into view and the old one out
	if (oldCategory) then
		GetWidget("options_main_category"..oldCategory):SetVisible(0)
	end

	GetWidget("options_main_category"..categoryIndex):SetY(0) 		-- reset any scrolling
	GetWidget("options_main_category"..categoryIndex):SetVisible(1)

	-- collpase the old category and expand the new
	HoN_Options.sliding = true
	local sleep = HoN_Options:CollapseCategory(oldCategory)
	local sleep2 = HoN_Options:ExpandCategory(categoryIndex)
	if sleep2 > sleep then sleep = sleep2 end
	GetWidget("options_main_holder"):Sleep(sleep, function(...)
		HoN_Options.sliding = false
	end)

	HoN_Options:HighlightCategory(categoryIndex, oldCategory)
	HoN_Options.scrollPosition = 0
	GetWidget("options_scrollbar"):SetValue(0)
end

function HoN_Options:SelectSubCategory(categoryIndex, subIndex)
	categoryIndex, subIndex = tonumber(categoryIndex), tonumber(subIndex)

	HoN_Options:HighlightSubCategory(categoryIndex, subIndex)

	if (categoryIndex ~= HoN_Options.selectedCategory) then
		if (HoN_Options.sliding) then return end
		HoN_Options:SelectCategory(categoryIndex)
	end

	-- set the y to that of the category
	if (HoN_Options.canScroll) then
		local scrollDest = 0
		if (subIndex > 0) then
			scrollDest = -HoN_Options.categoryInfo[categoryIndex][subIndex]
			scrollDest = scrollDest + interface:GetHeightFromString(SUBCATEGORY_ARROW_OFFSET)
		end

		if (scrollDest >= 0) then
			scrollDest = 0
			GetWidget("options_top_grad"):FadeOut(25)
			GetWidget("options_scrollbox_scroller_up"):FadeOut(150)
			GetWidget("options_bottom_grad"):FadeIn(25)
			GetWidget("options_scrollbox_scroller_down"):FadeIn(150)
		elseif (scrollDest <= -HoN_Options.maxPossibleScroll) then
			scrollDest = -HoN_Options.maxPossibleScroll
			GetWidget("options_top_grad"):FadeIn(25)
			GetWidget("options_scrollbox_scroller_up"):FadeIn(150)
			GetWidget("options_bottom_grad"):FadeOut(25)
			GetWidget("options_scrollbox_scroller_down"):FadeOut(150)
		else
			GetWidget("options_top_grad"):FadeIn(25)
			GetWidget("options_scrollbox_scroller_up"):FadeIn(150)
			GetWidget("options_bottom_grad"):FadeIn(25)
			GetWidget("options_scrollbox_scroller_down"):FadeIn(150)
		end

		GetWidget("options_main_category"..categoryIndex):SetY(scrollDest)
		HoN_Options.scrollPosition = scrollDest
		GetWidget("options_scrollbar"):SetValue(-scrollDest)
	end

	if (subIndex == 0) then subIndex = 1 end
	HoN_Options.selectedSubCategory = subIndex
end

function HoN_Options:HighlightSubCategory(categoryIndex, subCategory)
	if (HoN_Options.selectedCategory and HoN_Options.selectedSubCategory) then
		GetWidget("options_sub"..HoN_Options.selectedCategory..HoN_Options.selectedSubCategory.."_lbl"):SetColor("#37b2d9")
	end

	if (subCategory == 0) then subCategory = 1 end
	HoN_Options.selectedSubCategory = subCategory
	GetWidget("options_sub"..categoryIndex..subCategory.."_lbl"):SetColor("1 1 1 1")
end

function HoN_Options:HighlightCategory(categoryIndex, oldCategory)
	if (oldCategory) then
		GetWidget("options_cat"..oldCategory.."_lbl"):SetColor(".7 .7 .7")
	end

	GetWidget("options_cat"..categoryIndex.."_lbl"):SetColor("1 1 1")
end

function HoN_Options:ScrollMain(direction, amount)
	if (not HoN_Options.canScroll) then return end

	if (not amount) then amount = interface:GetHeightFromString(SCROLL_AMOUNT) end

	-- determine destinations
	direction = tonumber(direction)
	local scrollDest = nil

	if (direction == 1) then -- up
		scrollDest = HoN_Options.scrollPosition + amount
	else 					 -- down
		scrollDest = HoN_Options.scrollPosition - amount
	end

	-- clamp, change the time to match the change in destination as well, so speed is always the same
	if (scrollDest) then
		if (scrollDest >= 0) then
			scrollDest = 0
			GetWidget("options_top_grad"):FadeOut(25)
			GetWidget("options_scrollbox_scroller_up"):FadeOut(150)
			GetWidget("options_bottom_grad"):FadeIn(25)
			GetWidget("options_scrollbox_scroller_down"):FadeIn(150)
		elseif (scrollDest <= -HoN_Options.maxPossibleScroll) then
			scrollDest = -HoN_Options.maxPossibleScroll
			GetWidget("options_top_grad"):FadeIn(25)
			GetWidget("options_scrollbox_scroller_up"):FadeIn(150)
			GetWidget("options_bottom_grad"):FadeOut(25)
			GetWidget("options_scrollbox_scroller_down"):FadeOut(150)
		elseif (HoN_Options.scrollPosition == 0) then -- scrolling from top
			GetWidget("options_top_grad"):FadeIn(25)
			GetWidget("options_scrollbox_scroller_up"):FadeIn(150)
		elseif (HoN_Options.scrollPosition == -HoN_Options.maxPossibleScroll) then --scrolling from bottom
			GetWidget("options_bottom_grad"):FadeIn(25)
			GetWidget("options_scrollbox_scroller_down"):FadeIn(150)
		end

		-- scroll
		GetWidget("options_main_category"..HoN_Options.selectedCategory):SetY(scrollDest)
		HoN_Options.scrollPosition = scrollDest
		if (not dontSetScrollbar) then
			GetWidget("options_scrollbar"):SetValue(-scrollDest)
		end

		local selectedOffset = nil
		-- check if we need to highlight a new sub
		for i=1,HoN_Options.categoryInfo[HoN_Options.selectedCategory].numSubs do
			if ((-(HoN_Options.scrollPosition - interface:GetHeightFromString(SUBCATEGORY_ARROW_OFFSET)) >= HoN_Options.categoryInfo[HoN_Options.selectedCategory][i])) then
				selectedOffset = i
			else
				break
			end
		end

		if (selectedOffset and selectedOffset ~= HoN_Options.selectedSubCategory) then
			HoN_Options:HighlightSubCategory(HoN_Options.selectedCategory, selectedOffset)
		end
	end
end

function HoN_Options:ScrollSub(direction)
	direction = tonumber(direction)
	local main, sub = nil, nil

	if (direction == 1) then -- up
		sub = HoN_Options.selectedSubCategory - 1
		if (sub <= 0) then
			main = HoN_Options.selectedCategory - 1
			if (main <= 0) then
				if (OPTION_WRAP) then
					main = HoN_Options.categoryInfo.numCategories
				else
					return
				end
			end

			sub = HoN_Options.categoryInfo[main or HoN_Options.selectedCategory].numSubs
		end
	else 					 -- down
		sub = HoN_Options.selectedSubCategory + 1
		if (sub > HoN_Options.categoryInfo[HoN_Options.selectedCategory].numSubs) then
			main = HoN_Options.selectedCategory + 1
			if (main > HoN_Options.categoryInfo.numCategories) then
				if (OPTION_WRAP) then
					main = 1
				else
					return
				end
			end

			sub = 1
		end
	end

	if (sub and main) then
		if (not HoN_Options.sliding) then
			HoN_Options:SelectSubCategory(main, sub)
		else
			return
		end
	elseif (sub) then
		HoN_Options:SelectSubCategory(HoN_Options.selectedCategory, sub)
	end
end

function HoN_Options:ExpandCategory(categoryIndex)
	categoryIndex = tonumber(categoryIndex)
	if (not categoryIndex) then return SUBCATEGORY_SLIDE_TIME+15 end

	local optionsWidget = GetWidget("options_submenu"..categoryIndex)
	if (optionsWidget) then
		local menuHeight = HoN_Options.categoryInfo[categoryIndex].height
		local expandTime = HoN_Options.categoryInfo[categoryIndex].numSubs * SUBCATEGORY_SLIDE_TIME

		-- expand the panel holding the sub options
		GetWidget("options_submenu"..categoryIndex):ScaleHeight(menuHeight, expandTime)
		
		-- slide the sub categories down
		for i=1, HoN_Options.categoryInfo[categoryIndex].numSubs do
			local subWidget = GetWidget("options_sub"..categoryIndex..i)
			subWidget:SetY(-interface:GetHeightFromString(SUBCATEGORY_EXTRA_SLIDE))
			subWidget:SetVisible(1)
			subWidget:SlideY((interface:GetHeightFromString("2.0h") * (i-1)), SUBCATEGORY_SLIDE_TIME * i)
		end
		GetWidget("options_submenu"..categoryIndex):SetVisible(1)

		return expandTime+15
	end
end

function HoN_Options:CollapseCategory(categoryIndex)
	categoryIndex = tonumber(categoryIndex)
	if (not categoryIndex) then return SUBCATEGORY_SLIDE_TIME+15 end

	local optionsWidget = GetWidget("options_submenu"..categoryIndex)
	if (optionsWidget) then
		local menuHeight = HoN_Options.categoryInfo[categoryIndex].height
		local collapseTime = SUBCATEGORY_SLIDE_TIME * HoN_Options.categoryInfo[categoryIndex].numSubs

		-- collapse the panel holding the sub options
		GetWidget("options_submenu"..categoryIndex):ScaleHeight(0, collapseTime)

		-- slide the sub categories up
		for i=1, HoN_Options.categoryInfo[categoryIndex].numSubs do
			local subWidget = GetWidget("options_sub"..categoryIndex..i)
			subWidget:SlideY(-interface:GetHeightFromString("2.5h"), SUBCATEGORY_SLIDE_TIME * i)
			subWidget:Sleep((SUBCATEGORY_SLIDE_TIME * i) - (SUBCATEGORY_SLIDE_TIME), function()
				subWidget:SetVisible(0)
			end)
		end
		
		GetWidget("options_cat"..categoryIndex):Sleep(collapseTime, function()
			GetWidget("options_submenu"..categoryIndex):SetVisible(0)
		end)

		return collapseTime+15
	end
end

function HoN_Options:HoverScroller()
	if (HoN_Options.canScroll) then
		if (HoN_Options.scrollPosition < 0) then
			GetWidget("options_scrollbox_scroller_up"):FadeIn(150)
		end
		if (HoN_Options.scrollPosition > -HoN_Options.maxPossibleScroll) then
			GetWidget("options_scrollbox_scroller_down"):FadeIn(150)
		end
	end
end

function HoN_Options:DoScroll()	
	if (HoN_Options.scrollState == 1) then
		HoN_Options:ScrollMain(0, interface:GetHeightFromString(SCROLL_ARROW_AMOUNT) * HoN_Options.scrollMultiplyer)
		if (HoN_Options.scrollPosition > -HoN_Options.maxPossibleScroll) then
			GetWidget('options_main_scroller'):Sleep(1, function() HoN_Options:DoScroll() end)
		end
	elseif (HoN_Options.scrollState == -1) then
		HoN_Options:ScrollMain(1, interface:GetHeightFromString(SCROLL_ARROW_AMOUNT) * HoN_Options.scrollMultiplyer)
		if (HoN_Options.scrollPosition < 0) then
			GetWidget('options_main_scroller'):Sleep(1, function() HoN_Options:DoScroll() end)
		end
	end
end

function HoN_Options:StartScrollDown()
	HoN_Options:HoverScroller()
	HoN_Options.scrollState = 1
	HoN_Options:DoScroll()	
end

function HoN_Options:StartScrollUp()
	HoN_Options:HoverScroller()
	HoN_Options.scrollState = -1
	HoN_Options:DoScroll()	
end

function HoN_Options:MultiplyScrollSpeed(amount)
	amount = tonumber(amount)
	if (HoN_Options.scrollMultiplyer == amount) then
		HoN_Options.scrollMultiplyer = 1.0
	else
		HoN_Options.scrollMultiplyer = amount
	end
end

function HoN_Options:StopScroll()
	HoN_Options.scrollState = 0
	HoN_Options.scrollMultiplyer = 1.0
end

function HoN_Options:ReferralPage()
	UIManager.GetInterface('webpanel'):HoNWebPanelF('LoadURLWithThrob', GetCvarString('ui_options_referral_url') .. '?lang=' .. GetCvarString('host_language') .. '&cookie=' .. (interface:UICmd("GetCookie()")), GetWidget("options_referral_browser"))
end

function HoN_Options:CalculateScroll(categoryIndex)
	if (not categoryIndex) then categoryIndex = HoN_Options.selectedCategory end

	HoN_Options.maxPossibleScroll = HoN_Options.categoryInfo[categoryIndex][HoN_Options.categoryInfo[categoryIndex].numSubs] - interface:GetHeightFromString(SUBCATEGORY_ARROW_OFFSET)
	local distanceToBottomFromLastHeader = GetWidget("options_main_category"..categoryIndex):GetHeight() - HoN_Options.maxPossibleScroll
	if (distanceToBottomFromLastHeader > GetWidget("options_main_holder"):GetHeight()) then -- distance is larger than a screen's height
		HoN_Options.maxPossibleScroll = HoN_Options.maxPossibleScroll + (distanceToBottomFromLastHeader - GetWidget("options_main_holder"):GetHeight())
		HoN_Options.maxPossibleScroll = HoN_Options.maxPossibleScroll + interface:GetHeightFromString(SCROLL_BOTTOM_PADDING)
	end

	if (HoN_Options.maxPossibleScroll <= 0) then
		HoN_Options.canScroll = false
		GetWidget("options_top_grad"):FadeOut(25)
		GetWidget("options_scrollbox_scroller_up"):FadeOut(150)
		GetWidget("options_bottom_grad"):FadeOut(25)
		GetWidget("options_scrollbox_scroller_down"):FadeOut(150)
		GetWidget("options_scrollbar"):SetMaxValue(0)
	else
		HoN_Options.canScroll = true
		GetWidget("options_scrollbar"):SetMaxValue(HoN_Options.maxPossibleScroll)
	end

	if (HoN_Options.scrollPosition < -HoN_Options.maxPossibleScroll) then
		HoN_Options.scrollPosition = -HoN_Options.maxPossibleScroll
		GetWidget("options_scrollbar"):SetValue(-HoN_Options.maxPossibleScroll)
		GetWidget("options_main_category"..HoN_Options.selectedCategory):SetY(HoN_Options.scrollPosition)
		-- we will be at the bottom, hide the scrollers and stuff
		GetWidget("options_bottom_grad"):FadeOut(25)
		GetWidget("options_scrollbox_scroller_down"):FadeOut(150)
	elseif (HoN_Options.scrollPosition ~= -HoN_Options.maxPossibleScroll) then -- not at bottom
		GetWidget("options_bottom_grad"):FadeIn(25)
		GetWidget("options_scrollbox_scroller_down"):FadeIn(150)
	end
end

function HoN_Options:UpdateScrollAmounts()
	HoN_Options:CalculateScroll(HoN_Options.selectedCategory)
end

function HoN_Options:HoverCategory(over, cat, sub)
	cat, sub = tonumber(cat), tonumber(sub)

	if (over) then -- on mouse over
		if (cat ~= HoN_Options.selectedCategory) then
			GetWidget("options_cat"..cat.."_lbl"):SetColor(".9 .9 .9 1")
		end
		if (sub ~= 0 and sub ~= HoN_Options.selectedSubCategory) then
			GetWidget("options_sub"..cat..sub.."_lbl"):SetColor("#9ED6E8")
		end
	else  		   -- on mouse out
		if (cat ~= HoN_Options.selectedCategory) then
			GetWidget("options_cat"..cat.."_lbl"):SetColor(".8 .8 .8 1")
		end
		if (sub ~= 0 and sub ~= HoN_Options.selectedSubCategory) then
			GetWidget("options_sub"..cat..sub.."_lbl"):SetColor("#37b2d9")
		end
	end
end

function HoN_Options:ExpandCollapseFAQ(faqID)
	HoN_Options:ToggleWidgetVisibility(GetWidget("faq_q"..faqID))
end

function HoN_Options:ToggleWidgetVisibility(widget)
	local isVisible = not widget:IsVisible()
	widget:SetVisible(isVisible)

	local setY = nil
	if (isVisible) then -- check if we need to move down to fit the collapsed item
		local currentYBottom = GetWidget("options_main_holder"):GetAbsoluteY() + GetWidget("options_main_holder"):GetHeight()
		local widgetBottom = widget:GetAbsoluteY() + widget:GetHeight() + interface:GetHeightFromString("2.5h")
		-- 2.5 on the widget bottom so it won't appear in the bottom gradient

		if (widgetBottom > currentYBottom) then
			setY = HoN_Options.scrollPosition - (widgetBottom - currentYBottom)
		end
	end

	HoN_Options:UpdateScrollAmounts()

	-- set the y if we need to and do any hiding, showing, updating if we need to
	if (setY) then
		if (setY >= 0) then
			setY = 0
			GetWidget("options_top_grad"):FadeOut(25)
			GetWidget("options_scrollbox_scroller_up"):FadeOut(150)
			GetWidget("options_bottom_grad"):FadeIn(25)
			GetWidget("options_scrollbox_scroller_down"):FadeIn(150)
		elseif (setY <= -HoN_Options.maxPossibleScroll) then
			setY = -HoN_Options.maxPossibleScroll
			GetWidget("options_top_grad"):FadeIn(25)
			GetWidget("options_scrollbox_scroller_up"):FadeIn(150)
			GetWidget("options_bottom_grad"):FadeOut(25)
			GetWidget("options_scrollbox_scroller_down"):FadeOut(150)
		else
			GetWidget("options_top_grad"):FadeIn(25)
			GetWidget("options_scrollbox_scroller_up"):FadeIn(150)
			GetWidget("options_bottom_grad"):FadeIn(25)
			GetWidget("options_scrollbox_scroller_down"):FadeIn(150)
		end

		-- scroll
		GetWidget("options_main_category"..HoN_Options.selectedCategory):SetY(setY)
		HoN_Options.scrollPosition = setY
		GetWidget("options_scrollbar"):SetValue(-setY)

		local selectedOffset = nil
		-- check if we need to highlight a new sub
		for i=1,HoN_Options.categoryInfo[HoN_Options.selectedCategory].numSubs do
			if ((-(HoN_Options.scrollPosition - interface:GetHeightFromString(SUBCATEGORY_ARROW_OFFSET)) >= HoN_Options.categoryInfo[HoN_Options.selectedCategory][i])) then
				selectedOffset = i
			else
				break
			end
		end

		if (selectedOffset and selectedOffset ~= HoN_Options.selectedSubCategory) then
			HoN_Options:HighlightSubCategory(HoN_Options.selectedCategory, selectedOffset)
		end
	end
end

----- Graduated Slider functions ------

function HoN_Options.InitializeGraphicsOptions()

	local level = GetCvarInt('ui_options_gfxslider')

	-- foliage render type
	if level == 1 or level == 2 then
		Set('options_foliageRenderType', 0)
		Set('vid_foliageAlphaTestRef', 90)
		Set('vid_foliageAlphaTestRef2', 90)
		Set('vid_foliageRenderType', 0)
	elseif level == 3 or level == 4 then
		Set('options_foliageRenderType', 1)
		Set('vid_foliageAlphaTestRef', 178)
		Set('vid_foliageAlphaTestRef2', 178)
		Set('vid_foliageRenderType', 2)
	elseif level == 5 then
		-- custom
	end
end

function HoN_Options:NoSlideFunction(value)
	value = tonumber(value)
	Echo("^rGraduated Slider not hooked up to a proper function (slidefunction=). ^gSlider Value: "..value)
end

function HoN_Options:GraphicsSlideFunction(value)
	value = tonumber(value)

	SetImplicitOptionsCVarsByLevel(value)

	SetBoolOptionsCVarsByLevel(value)
		
	-- setup button+glow
	local enabled = (value ~= 5 and value ~= GetCvarInt('ui_options_gfxslider'))
	GetWidget("options_graphics_apply_glow"):SetVisible(enabled)
	GetWidget("option_gfx_slider_apply"):SetEnabled(enabled)
	GetWidget("option_gfx_slider_apply"):SetNoClick(not enabled)

	local textureFiltering = GetWidget("otpwdg_options_textureFiltering")
	local numTextFilter = textureFiltering:GetNumListItems()

	local AADropdown = GetWidget("otpwdg_options_antialiasing")
	local numAASettings = AADropdown:GetNumListItems()


	-- still need options_bpp, options_antialiasing, vid_textureFiltering
	-- these are all dependant on what their computer supports and will need to be selected from dropdowns
	-- if (value == 0) then	 -- super low
	-- 	
	if (value == 1) then -- low
		-- dropdowns
		GetWidget("otpwdg_options_modelQuality"):UICmd("SetSelectedItemByIndex(1, false)")
		GetWidget("otpwdg_options_textureSize"):UICmd("SetSelectedItemByIndex(1, false)")
		GetWidget("otpwdg_options_shaderQuality"):UICmd("SetSelectedItemByIndex(0, false)")
		GetWidget("otpwdg_options_shadowQuality"):UICmd("SetSelectedItemByIndex(0, false)")
		GetWidget("otpwdg_options_waterQuality"):UICmd("SetSelectedItemByIndex(0, false)")

		textureFiltering:UICmd("SetSelectedItemByIndex(1, false)")
		AADropdown:UICmd("SetSelectedItemByIndex(0, false)")
	elseif (value == 2) then -- med
		-- drop downs
		GetWidget("otpwdg_options_modelQuality"):UICmd("SetSelectedItemByIndex(1, false)")
		GetWidget("otpwdg_options_textureSize"):UICmd("SetSelectedItemByIndex(1, false)")
		GetWidget("otpwdg_options_shaderQuality"):UICmd("SetSelectedItemByIndex(1, false)")
		GetWidget("otpwdg_options_shadowQuality"):UICmd("SetSelectedItemByIndex(2, false)")
		GetWidget("otpwdg_options_waterQuality"):UICmd("SetSelectedItemByIndex(1, false)")

		-- mid range filtering
		textureFiltering:UICmd("SetSelectedItemByIndex(2, false)")
		AADropdown:UICmd("SetSelectedItemByIndex(0, false)")
	elseif (value == 3) then -- high
		-- drop downs
		GetWidget("otpwdg_options_modelQuality"):UICmd("SetSelectedItemByIndex(2, false)")
		GetWidget("otpwdg_options_textureSize"):UICmd("SetSelectedItemByIndex(2, false)")
		GetWidget("otpwdg_options_shaderQuality"):UICmd("SetSelectedItemByIndex(2, false)")
		GetWidget("otpwdg_options_shadowQuality"):UICmd("SetSelectedItemByIndex(3, false)")
		GetWidget("otpwdg_options_waterQuality"):UICmd("SetSelectedItemByIndex(2, false)")

		-- x8 or highest filtering
		if (numTextFilter >= 7) then
			textureFiltering:UICmd("SetSelectedItemByIndex(6, false)")
		else
			textureFiltering:UICmd("SetSelectedItemByIndex("..(numTextFilter-1)..", false)")
		end

		-- 4x or highest AA
		if (numAASettings >= 3) then
			AADropdown:UICmd("SetSelectedItemByIndex(2, false)")
		else
			AADropdown:UICmd("SetSelectedItemByIndex("..(numAASettings-1)..", false)")
		end
	elseif (value == 4) then -- ultra
		-- drop downs
		GetWidget("otpwdg_options_modelQuality"):UICmd("SetSelectedItemByIndex(2, false)")
		GetWidget("otpwdg_options_textureSize"):UICmd("SetSelectedItemByIndex(2, false)")
		GetWidget("otpwdg_options_shaderQuality"):UICmd("SetSelectedItemByIndex(2, false)")
		GetWidget("otpwdg_options_shadowQuality"):UICmd("SetSelectedItemByIndex(3, false)")
		GetWidget("otpwdg_options_waterQuality"):UICmd("SetSelectedItemByIndex(3, false)")

		-- select the highest filtering
		textureFiltering:UICmd("SetSelectedItemByIndex("..(numTextFilter-1)..", false)")
		-- 8x or highest filtering
		if (numAASettings >= 4) then
			AADropdown:UICmd("SetSelectedItemByIndex(3, false)")
		else
			AADropdown:UICmd("SetSelectedItemByIndex("..(numAASettings-1)..", false)")
		end
	elseif (value == 5) then
		-- removed since clicking an option after picking a slider position would reset all other options
		--interface:UICmd("OptionsCancel();") -- this will result in the last saved options getting set (not sure if we want this)
	end

	HoN_Options:UpdatePreview(value)
end

function HoN_Options:PopulateModels()
	HoN_Options:UpdatePreview(GetCvarInt("ui_options_gfxslider", true) or 5)
end

function HoN_Options:UpdatePreview(sliderPos)
	GetWidget("ui_options_preview_high"):SetVisible(sliderPos >= 3)
	GetWidget("ui_options_preview_med"):SetVisible(sliderPos == 2)
	GetWidget("ui_options_preview_low"):SetVisible(sliderPos == 1)
end

function HoN_Options:SlideScroller(position)
	position = -tonumber(position)
	local diffToScroll = round(HoN_Options.scrollPosition - position)

	if (diffToScroll < 0) then
		HoN_Options:ScrollMain(1, math.abs(diffToScroll))
	elseif (diffToScroll > 0) then
		HoN_Options:ScrollMain(-1, math.abs(diffToScroll))
	end
end

---------------------------------------

function interface:HoNOptionsF(func, ...)
	if (HoN_Options[func]) then
		print(HoN_Options[func](HoN_Options, ...))
	else
		print('HoNOptionsF failed to find: ' .. tostring(func) .. '\n')
	end
end	

function HoN_Options:UpdateLUAOptions()
	groupfcall('options_options_lua',  function (_, widget, _) widget:DoEventN(7) widget:DoEventN(9) end)
	groupfcall('options_search_results',  function (_, widget, _) widget:DoEventN(7) widget:DoEventN(9) end)
end

--------------- Search WIP

HoN_Options.optionsInfoTable = nil

function HoN_Options.DumpOptionsTable()
	if (debug_output) then
		println("Register options table dumped.")
	end

	HoN_Options.optionsInfoTable = nil
end

function HoN_Options.RegisterOptions()
	if (not HoN_Options.optionsInfoTable) then
		if (debug_output) then
			println("Registering Options...")
		end
		HoN_Options.optionsInfoTable = {}
		groupfcall('options_options', function (_, widget, _) widget:DoEventN(8) end)
		groupfcall('options_options_lua', function (_, widget, _) widget:DoEventN(8) end)
	end
end

function HoN_Options:SearchButton()
	HoN_Options:SelectSubCategory(9, 1)
	-- focus the input box
	GetWidget("options_search_input"):SetFocus(true)
end
function HoN_Options.RegisterOption(option_template, category, subcategory, cvar, title, sub_text1, sub_text2, sub_text3, tip_image, tip_name, tip_text1, tip_text2, tip_text3, gradPos1, gradPos2, gradPos3, gradPos4, gradPos5, gradName1, gradName2, gradName3, gradName4, gradName5, slidefunction, maxvalue, data, onchange, onchangelua, onselect, populate, maxlistheight, pair, inverted, enforce, enforce2, enforce3, enforce4, precision, numerictype, percent, round_var, title_font, command, table, action, param, impulse, onclick, usemodifiers, cloudcvar)

	if (debug_output) then
		println('Register ' .. tostring(option_template) .. ' | ' .. tostring(title) .. ' | ' .. tostring(category) .. ' | ' .. tostring(subcategory) .. ' | ' .. tostring(cvar) .. ' | ' .. tostring(slidefunction) .. " | " )
	end
	
	HoN_Options.optionsInfoTable[cvar] = {	
		option_template = option_template,
		category = category,
		subcategory = subcategory,
		cvar = cvar,
		title = title,
		sub_text1 = sub_text1,
		sub_text2 = sub_text2,
		sub_text3 = sub_text3,
		tip_image = tip_image,
		tip_name = tip_name,
		tip_text1 = tip_text1,
		tip_text2 = tip_text2,
		tip_text3 = tip_text3,
		gradPos1 = gradPos1,
		gradPos2 = gradPos2,
		gradPos3 = gradPos3,
		gradPos4 = gradPos4,
		gradPos5 = gradPos5,
		gradName1 = gradName1,
		gradName2 = gradName2,
		gradName3 = gradName3,
		gradName4 = gradName4,
		gradName5 = gradName5,
		maxvalue = maxvalue,
		data = data,
		onchange = onchange,
		onchangelua = onchangelua,
		onselect = onselect,
		populate = populate,
		maxlistheight = maxlistheight,
		slidefunction = slidefunction,
		inverted = inverted,
		pair = pair,
		enforce = enforce,
		enforce2 = enforce2,
		enforce3 = enforce3,
		enforce4 = enforce4,
		precision = precision,
		numerictype = numerictype,
		percent = percent,
		round_var = round_var,
		title_font = title_font,
		command = command,
		table = table,
		action = action,
		param = param,
		impulse = impulse,
		onclick = onclick,
		usemodifiers = usemodifiers,
		cloudcvar = cloudcvar or ''
	}
	
end

local function DisplaySearchResults(matchingOptionTable) 
	
	local EscapeString = EscapeString
	local resultCount = 0
	
	local function sortFunc(a, b)
		return a.title < b.title
	end

	table.sort(matchingOptionTable, sortFunc)

	for _, optionTable in pairs(matchingOptionTable) do
	
		resultCount = resultCount + 1
		if (resultCount > 30) then break end

		GetWidget('options_search_insertion_point'):Instantiate(optionTable.option_template,
			'nameid', '_search',
			'width', '75%',
			'group', 'options_search_results',
			'category', optionTable.category,
			'subcategory', optionTable.subcategory,
			'cvar', optionTable.cvar,
			'title', EscapeString(optionTable.title),
			'sub_text1', EscapeString(optionTable.sub_text1),
			'sub_text2', EscapeString(optionTable.sub_text2),
			'sub_text3', EscapeString(optionTable.sub_text3),
			'tip_image', EscapeString(optionTable.tip_image),
			'tip_name', EscapeString(optionTable.tip_name),
			'tip_text1', EscapeString(optionTable.tip_text1),
			'tip_text2', EscapeString(optionTable.tip_text2),
			'tip_text3', EscapeString(optionTable.tip_text3),
			'gradmark1_pos', optionTable.gradPos1,
			'gradmark2_pos', optionTable.gradPos2,
			'gradmark3_pos', optionTable.gradPos3,
			'gradmark4_pos', optionTable.gradPos4,
			'gradmark5_pos', optionTable.gradPos5,
			'gradmark1_name', EscapeString(optionTable.gradName1),
			'gradmark2_name', EscapeString(optionTable.gradName2),
			'gradmark3_name', EscapeString(optionTable.gradName3),
			'gradmark4_name', EscapeString(optionTable.gradName4),
			'gradmark5_name', EscapeString(optionTable.gradName5),
			'maxvalue', optionTable.maxvalue,
			'data', optionTable.data,
			'onchange', optionTable.onchange,
			'onchangelua', optionTable.onchangelua,
			'onselect', optionTable.onselect,
			'populate', optionTable.populate,
			'maxlistheight', optionTable.maxlistheight,
			'slidefunction', optionTable.slidefunction,
			'inverted', optionTable.inverted,
			'pair', optionTable.pair,
			'enforce', optionTable.enforce,
			'enforce2', optionTable.enforce2,
			'enforce3', optionTable.enforce3,
			'enforce4', optionTable.enforce4,
			'precision', optionTable.precision,
			'numerictype', optionTable.numerictype,
			'percent', optionTable.percent,
			'round_var', optionTable.round_var,
			'title_font', optionTable.title_font,
			'command', optionTable.command,
			'table', optionTable.table,
			'action', optionTable.action,
			'param', optionTable.param,
			'impulse', optionTable.impulse,
			'onclick', optionTable.onclick,
			'usemodifiers', optionTable.usemodifiers,
			'cloudcvar', optionTable.cloudcvar,
			'register', 'false'
		)

	end

	groupfcall('options_search_results', function (_, widget, _) widget:DoEventN(9) end)

	GetWidget('options_search_cover_throb'):SetVisible(0)	
		
	if (#matchingOptionTable > 0) then
		GetWidget('options_search_insertion_point_cover'):SetVisible(0)
		GetWidget('options_search_cover_label'):SetVisible(0)
	else
		GetWidget('options_search_insertion_point_cover'):SetVisible(1)
		GetWidget('options_search_cover_label'):SetVisible(1)
		GetWidget('options_search_cover_label'):SetText(Translate("options_search_not_found"))
	end

	-- scroll amount (extra amount to pad for the top since we need the height of the results)
	if ((GetWidget("options_search_insertion_point"):GetHeight() + interface:GetHeightFromString("9.0h")) > GetWidget("options_main_holder"):GetHeight()) then -- distance is larger than a screen's height
		HoN_Options.maxPossibleScroll = (GetWidget("options_search_insertion_point"):GetHeight() + interface:GetHeightFromString("9.0h")) - GetWidget("options_main_holder"):GetHeight()
		HoN_Options.maxPossibleScroll = HoN_Options.maxPossibleScroll + interface:GetHeightFromString(SCROLL_BOTTOM_PADDING)
		GetWidget("options_scrollbar"):SetMaxValue(HoN_Options.maxPossibleScroll)

		HoN_Options.canScroll = true
		GetWidget("options_top_grad"):FadeOut(25)
		GetWidget("options_scrollbox_scroller_up"):FadeOut(150)
		GetWidget("options_bottom_grad"):FadeIn(25)
		GetWidget("options_scrollbox_scroller_down"):FadeIn(150)
	else
		GetWidget("options_scrollbar"):SetMaxValue(0)
		HoN_Options.canScroll = false
		GetWidget("options_top_grad"):FadeOut(25)
		GetWidget("options_scrollbox_scroller_up"):FadeOut(150)
		GetWidget("options_bottom_grad"):FadeOut(25)
		GetWidget("options_scrollbox_scroller_down"):FadeOut(150)
	end
end

local function SearchOptions(searchString)
	
	-- println('searchString = ' .. tostring(searchString) )
	if (debug_output) then
		Echo("Searching for "..searchString)
	end
	
	groupfcall('options_search_results', function (_, widget, _) widget:Destroy() end)
	local matchingOptionTable = {}

	for k, optionTable in pairs(HoN_Options.optionsInfoTable) do
		-- search the title, the sub texts, the tips, and cvar name for matches
		-- instead of the old way which was against everything and against them untranslated
		if (
			find(string.lower(Translate(optionTable.title)), string.lower(searchString), nil, true) or
			find(string.lower(Translate(optionTable.sub_text1)), string.lower(searchString), nil, true) or
			find(string.lower(Translate(optionTable.sub_text2)), string.lower(searchString), nil, true) or
			find(string.lower(Translate(optionTable.sub_text3)), string.lower(searchString), nil, true) or
			find(string.lower(Translate(optionTable.tip_name)), string.lower(searchString), nil, true) or
			find(string.lower(Translate(optionTable.tip_text1)), string.lower(searchString), nil, true) or
			find(string.lower(Translate(optionTable.tip_text2)), string.lower(searchString), nil, true) or
			find(string.lower(Translate(optionTable.tip_text3)), string.lower(searchString), nil, true) or
			find(string.lower(optionTable.cvar), string.lower(searchString), nil, true) or
			find(string.lower(optionTable.cloudcvar), string.lower(searchString), nil, true)
		) then
			local available = true
			local action = optionTable.action
			if not GetCvarBool('cg_EnableFbStream') and (action == 'ToggleFacebookStreamMenu' or action == 'StartFacebookStream' or action == 'StopFacebookStream') then
				available = false
			end

			if available then
				tinsert(matchingOptionTable, optionTable)

				if (debug_output) then
					Echo("Match found in search "..optionTable.title.." Key: "..k.." Translated: "..Translate(optionTable.title))
				end
			end
		else
			if (debug_output) then
				Echo("Match found ^rNOT^* found in search "..optionTable.title.." Key: "..k.." Translated: "..Translate(optionTable.title))
			end
		end
	end

	GetWidget('options_search_insertion_point'):Sleep(1, function()
		DisplaySearchResults(matchingOptionTable)
	end)
end

function HoN_Options.SearchOptionsInput(searchString)
	groupfcall('options_search_results', function (_, widget, _) widget:Destroy() end)
	GetWidget('options_search_insertion_point_cover'):SetVisible(1)

	if (searchString) and NotEmpty(searchString) then
		if (string.len(searchString) >= 3) then
			HoN_Options.canScroll = false
			GetWidget("options_top_grad"):FadeOut(25)
			GetWidget("options_scrollbox_scroller_up"):FadeOut(150)
			GetWidget("options_bottom_grad"):FadeOut(25)
			GetWidget("options_scrollbox_scroller_down"):FadeOut(150)

			GetWidget("options_main_category"..HoN_Options.selectedCategory):SetY(0)
			HoN_Options.scrollPosition = 0
			GetWidget("options_scrollbar"):SetValue(0)

			GetWidget('options_search_cover_throb'):SetVisible(1)
			GetWidget('options_search_cover_label'):SetVisible(0)
			GetWidget('options_search_input'):Sleep(550, function()
				SearchOptions(searchString)
			end)
		else
			GetWidget('options_search_cover_throb'):SetVisible(0)
			GetWidget('options_search_cover_label'):SetText(Translate("options_search_too_short"))
			GetWidget('options_search_cover_label'):SetVisible(1)

			HoN_Options.canScroll = false
			GetWidget("options_top_grad"):FadeOut(25)
			GetWidget("options_scrollbox_scroller_up"):FadeOut(150)
			GetWidget("options_bottom_grad"):FadeOut(25)
			GetWidget("options_scrollbox_scroller_down"):FadeOut(150)
			-- interrupt an already running sleep to do a search
			GetWidget('options_search_input'):Sleep(1, function() end)

			GetWidget("options_main_category"..HoN_Options.selectedCategory):SetY(0)
			HoN_Options.scrollPosition = 0
			GetWidget("options_scrollbar"):SetValue(0)
		end
	else
		GetWidget('options_search_cover_throb'):SetVisible(0)
		GetWidget('options_search_cover_label'):SetText(Translate("options_search_terms"))
		GetWidget('options_search_cover_label'):SetVisible(1)
		-- interrupt an already running sleep to do a search
		GetWidget('options_search_input'):Sleep(1, function() end)

		if (HoN_Options.selectedCategory == 8) then -- update me if the search moves
			HoN_Options.canScroll = false
			GetWidget("options_top_grad"):FadeOut(25)
			GetWidget("options_scrollbox_scroller_up"):FadeOut(150)
			GetWidget("options_bottom_grad"):FadeOut(25)
			GetWidget("options_scrollbox_scroller_down"):FadeOut(150)

			GetWidget("options_main_category"..HoN_Options.selectedCategory):SetY(0)
			HoN_Options.scrollPosition = 0
			GetWidget("options_scrollbar"):SetValue(0)
		end
	end
end

---------------------- end search

--- Helper for rebinding the overlay stuff
function HoN_Options.UpdateOverlayBinds()
	local function unbind(table, funct, index)
		local oldBtn = interface:UICmd("GetKeybindButton('"..table.."', '"..funct.."', '', "..index..")")
		if (oldBtn ~= 'None') then
			Cmd("Unbind "..table.." "..oldBtn)
		end
	end

	-- 1 first, otherwise 1 becomes 0, technically calling it with 0 twice would work
	unbind("spectator", "ToggleOverlayMenuSpectator", 1)
	unbind("spectator", "ToggleOverlayMenuSpectator", 0)
	unbind("ui", "ToggleOverlayMenuUI", 1)
	unbind("ui", "ToggleOverlayMenuUI", 0)

	local newBtn1 = self:UICmd("GetKeybindButton('game', 'ToggleOverlayMenuGame', '', 0)")
	local newBtn2 = self:UICmd("GetKeybindButton('game', 'ToggleOverlayMenuGame', '', 1)")

	Cmd("BindImpulse spectator "..newBtn1.." ToggleOverlayMenuSpectator");
	Cmd("BindImpulse spectator "..newBtn2.." ToggleOverlayMenuSpectator");
	Cmd("BindImpulse ui "..newBtn1.." ToggleOverlayMenuUI");
	Cmd("BindImpulse ui "..newBtn2.." ToggleOverlayMenuUI");
end

-- function to open the options from elsewhere
function OpenOptions(category, subCategory, forceOpen)
	if (not GetWidget("game_options"):IsVisible()) then
		HoN_Options.targetCategory, HoN_Options.targetSubCategory = tonumber(category), tonumber(subCategory)

		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'game_options', nil, forceOpen)
	else
		HoN_Options:SelectSubCategory(tonumber(category), tonumber(subCategory))
	end
end

function HoN_Options.SetPrimaryKeybindLabelName(self ,table, action, param, num)

	local keyname = GetKeybindButton(table, action, param, num)

	if(#keyname < 4) then
		self:SetFont('dyn_18')
	else
		self:SetFont('dyn_12')
	end

	self:SetText(keyname)
end

function HoN_Options.SetAllSmartCasting()

	SetSave('cg_smartcastingAbility_1', true, 'bool')
	SetSave('cg_smartcastingAbility_2', true, 'bool')
	SetSave('cg_smartcastingAbility_3', true, 'bool')
	SetSave('cg_smartcastingAbility_4', true, 'bool')
	SetSave('cg_smartcastingAbility_Taunt', true, 'bool')
	SetSave('cg_smartcastingOrderAttack', true, 'bool')

	SetSave('cg_smartcastingBackpack_1', true, 'bool')
	SetSave('cg_smartcastingBackpack_2', true, 'bool')
	SetSave('cg_smartcastingBackpack_3', true, 'bool')
	SetSave('cg_smartcastingBackpack_4', true, 'bool')
	SetSave('cg_smartcastingBackpack_5', true, 'bool')
	SetSave('cg_smartcastingBackpack_6', true, 'bool')

	SetSave('cg_smartcastingExtraAbility_1', true, 'bool')
	SetSave('cg_smartcastingExtraAbility_2', true, 'bool')
	SetSave('cg_smartcastingExtraAbility_3', true, 'bool')
	SetSave('cg_smartcastingExtraAbility_4', true, 'bool')
	SetSave('cg_smartcastingExtraAbility_5', true, 'bool')
	SetSave('cg_smartcastingExtraAbility_6', true, 'bool')
	
	SetSave('cg_smartcastingBackpack_Ward', true, 'bool')
end


function HoN_Options.SetAllNormalCasting()

	SetSave('cg_smartcastingAbility_1', false, 'bool')
	SetSave('cg_smartcastingAbility_2', false, 'bool')
	SetSave('cg_smartcastingAbility_3', false, 'bool')
	SetSave('cg_smartcastingAbility_4', false, 'bool')
	SetSave('cg_smartcastingAbility_Taunt', false, 'bool')
	SetSave('cg_smartcastingOrderAttack', false, 'bool')

	SetSave('cg_smartcastingBackpack_1', false, 'bool')
	SetSave('cg_smartcastingBackpack_2', false, 'bool')
	SetSave('cg_smartcastingBackpack_3', false, 'bool')
	SetSave('cg_smartcastingBackpack_4', false, 'bool')
	SetSave('cg_smartcastingBackpack_5', false, 'bool')
	SetSave('cg_smartcastingBackpack_6', false, 'bool')

	SetSave('cg_smartcastingExtraAbility_1', false, 'bool')
	SetSave('cg_smartcastingExtraAbility_2', false, 'bool')
	SetSave('cg_smartcastingExtraAbility_3', false, 'bool')
	SetSave('cg_smartcastingExtraAbility_4', false, 'bool')
	SetSave('cg_smartcastingExtraAbility_5', false, 'bool')
	SetSave('cg_smartcastingExtraAbility_6', false, 'bool')

	SetSave('cg_smartcastingBackpack_Ward', false, 'bool')
end

function HoN_Options.SetMinimapPortraitAndColor()
	local type = GetCvarInt('g_minimapPortraitAndColorType')
	if type == 1 then
		Set('g_heroMinimapPortrait', 'false')
	elseif type == 2 then
		Set('g_heroMinimapPortrait', 'true')
		Set('g_forceMinimapPlayerColor', 'false')
	elseif type == 3 then
		Set('g_heroMinimapPortrait', 'true')
		Set('g_forceMinimapPlayerColor', 'true')
	else
		Set('g_heroMinimapPortrait', 'true')
		Set('g_forceMinimapPlayerColor', 'false')
	end
end

function RetroRevert()
	if (GetCvarString('cg_defaultInterface')) == '/ui/game_retro.interface' then
		if (GetCvarBool('cl_GarenaEnable')) then
			Set('cg_defaultInterface', "/ui/game_garena_other.interface")
		else
			Set('cg_defaultInterface', "/ui/game_apex.interface")
		end
	end
end