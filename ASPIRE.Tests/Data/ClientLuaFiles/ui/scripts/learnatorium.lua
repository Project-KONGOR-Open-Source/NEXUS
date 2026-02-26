------------------------
--	Learnatorium Script
--  Copyright 2015 Frostburn Studios
------------------------

RegisterScript2('Learnatorium', '30')

local VIDEO_CENTER_URL = "!hon/whitelistfolder/learnatorium/"

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()

local triggerHelper = GetWidget("Learnatorium_TriggerHelper")

-- variables --
HoN_Learner = {}		-- Learnatorium is too long/complex to type out all the time
HoN_Learner.heroInfo = {}
HoN_Learner.heroInfo.abilities = {}
HoN_Learner.selectedAlt = nil
HoN_Learner.defaultOffset = 0
HoN_Learner.heroAlts = {}
HoN_Learner.heroUsagesCached = false
HoN_Learner.heroUsages = nil
HoN_Learner.totalUsage = 0
HoN_Learner.lastUsageSort = nil       
HoN_Learner.modelRotation = 0
HoN_Learner.altAvatarOffset = 0
-- HoN_Learner.breadCrumbs = {}
HoN_Learner.compendiumUpdate = false
HoN_Learner.overrideOpen = false
HoN_Learner.forceHeroInfoUpdate = false
HoN_Learner.promptingTutorial = false
HoN_Learner.currentItemTab = ''
HoN_Learner.currentitemSelected = ''
HoN_Learner.itemEntityArray = {}

-- favorite stuff
HoN_Learner.favoriteHeroes = GetDBEntry("picker_favorite_heroes", nil, nil) or {}
HoN_Learner.favoriteButtons = {}

-- these all get saved to the database, this table here is filled with the defaults
HoN_Learner.saved = {
	["selectedTopTab"] 	= 4,
	["selectedLeftTab"]	= 1,
	["selectedHero"]	= "Hero_Andromeda"
}

local function BuildRatingsString(ratingTable)
	local str = ""

	for key, rating in pairs(ratingTable) do
		if (rating >= 3.0) then
			if (NotEmpty(str)) then
				str = str..", "
			end

			str = str..Translate("filter_category_"..key)
		end
	end

	return str
end

-- functionality --
function HoN_Learner:RestoreSelections()
	HoN_Learner.saved = GetDBEntry('hon_learnatorium_selections', nil, false, false, false) or HoN_Learner.saved
end

function HoN_Learner:SaveSelections()
	GetDBEntry('hon_learnatorium_selections', HoN_Learner.saved, true, false, false)
end

function HoN_Learner:OnHide()
	HoN_Learner:SaveSelections()
end

function HoN_Learner:OnShow()
	if (not HoN_Learner.overrideOpen) then
		HoN_Learner:RestoreSelections()
	end

	if (not HoN_Learner.heroUsagesCached) then
		HoN_Learner:GetUsageStats()
	end

	-- get the DB, encase it has updated
	HoN_Learner.favoriteHeroes = GetDBEntry("picker_favorite_heroes", nil, nil) or {}
	HoN_Learner:UpdateFavoriteButtonStates()

	Set("compendium_heroLastViewed", HoN_Learner.saved.selectedHero, "string")
	HoN_Learner.heroInfo.heroName = HoN_Learner.saved.selectedHero
	interface:UICmd("GetHeroInfo('"..HoN_Learner.saved.selectedHero.."');")

	if (not HoN_Learner.compendiumUpdate) then
		interface:UICmd("UpdateHeroCompendium()")
		HoN_Learner:PopulateHeroInfoPanel()
		HoN_Learner.compendiumUpdate = true
		HoN_Learner.forceHeroInfoUpdate = false
	elseif (HoN_Learner.forceHeroInfoUpdate) then
		HoN_Learner:PopulateHeroInfoPanel()
		HoN_Learner.forceHeroInfoUpdate = false
	end

	if (not HoN_Learner.overrideOpen) then
		HoN_Learner:SelectTabs(HoN_Learner.saved.selectedLeftTab, HoN_Learner.saved.selectedTopTab)
	end
	HoN_Learner.overrideOpen = false
end

function HoN_Learner:LoginRefresher(widget, _, _, isLoggedin)
	if (AtoB(isLoggedin) and HoN_Learner.compendiumUpdate) then
		HoN_Learner.compendiumUpdate = false
		if (GetWidget("compendium"):IsVisible()) then
			HoN_Learner:OnShow()
		end
	end
end
triggerHelper:RegisterWatch("LoginStatus", function(...) HoN_Learner:LoginRefresher(...) end)

function HoN_Learner:SetMasteryInfo(heroName)
	local masteryExp = GetMyMasteryExp(heroName)
	local masteryLevel = GetMasteryLevelByExp(masteryExp)
	local masteryType = GetMasterTypeByLevel(masteryLevel)
	local minExp, maxExp = GetMasteryExpByLevel(masteryLevel)
	local rate = (masteryExp-minExp) / (maxExp-minExp)

	GetWidget('learner_masteryinfo_level_id_hero_info'):SetText(tostring(masteryLevel))
	GetWidget('learner_masteryinfo_plate_id_hero_info'):SetTexture('/ui/fe2/mastery/learner_lbg_'..masteryType..'.tga')
	
	if not isNewUI() then
		GetWidget('learner_masteryinfo_level_id_hero_tooltip'):SetText(tostring(masteryLevel))
		GetWidget('learner_masteryinfo_plate_id_hero_tooltip'):SetTexture('/ui/fe2/mastery/learner_sbg_'..masteryType..'.tga')
	end

	if masteryLevel >= GetCvarInt('hero_mastery_maxlevel_new') then
		GetWidget('learner_masteryinfo_experience_id_hero_info'):SetText('')
		GetWidget('mastery_level_progressbar_bg_id_hero_info'):SetWidth('100%')
		GetWidget('mastery_level_progressbar_bg_id_hero_info'):SetTexture('/ui/fe2/mastery/m_gradient_maxlevel.tga')
		
		if not isNewUI() then
			GetWidget('learner_masteryinfo_experience_id_hero_tooltip'):SetText('')
			GetWidget('mastery_level_progressbar_bg_id_hero_tooltip'):SetWidth('100%')
			GetWidget('mastery_level_progressbar_bg_id_hero_tooltip'):SetTexture('/ui/fe2/mastery/m_gradient_maxlevel.tga')
		end
	else
		GetWidget('learner_masteryinfo_experience_id_hero_info'):SetText(tostring(masteryExp-minExp)..' / '..tostring(maxExp-minExp))	
		GetWidget('mastery_level_progressbar_bg_id_hero_info'):SetWidth(tostring(rate*100)..'%')
		GetWidget('mastery_level_progressbar_bg_id_hero_info'):SetTexture('/ui/fe2/mastery/m_gradient.tga')
		
		if not isNewUI() then
			GetWidget('learner_masteryinfo_experience_id_hero_tooltip'):SetText(tostring(masteryExp-minExp)..' / '..tostring(maxExp-minExp))
			GetWidget('mastery_level_progressbar_bg_id_hero_tooltip'):SetWidth(tostring(rate*100)..'%')
			GetWidget('mastery_level_progressbar_bg_id_hero_tooltip'):SetTexture('/ui/fe2/mastery/m_gradient.tga')
		end
	end
end

function HoN_Learner:PopulateHeroInfoPanel()
	-- really want to always have this done before calling this guy
	if (not HoN_Learner.heroInfo.isValid or HoN_Learner.heroInfo.heroName ~= HoN_Learner.saved.selectedHero) then
		HoN_Learner.heroInfo.heroName = HoN_Learner.saved.selectedHero
		interface:UICmd("GetHeroInfo('"..HoN_Learner.saved.selectedHero.."');")
	end

	-- breadcrumb base
	-- HoN_Learner.breadCrumbs[1] = HoN_Learner.heroInfo.displayName

	-- icon, name, difficulty, role
	GetWidget("learner_heroicon"):SetTexture(HoN_Learner.heroInfo.icon)
	GetWidget("learner_heroname"):SetText(HoN_Learner.heroInfo.displayName)
	HoN_Learner:PopulateDifficulty(HoN_Learner.heroInfo.difficulty)

	-- favorite button
	if (HoN_Learner.favoriteHeroes[HoN_Learner.saved.selectedHero]) then
		GetWidget("learner_hero_favorite_button"):SetButtonState(0)
	else
		GetWidget("learner_hero_favorite_button"):SetButtonState(1)
	end

	GetWidget("learner_hero_favorite_button"):SetCallback('onclick', function() HoN_Learner:ToggleFavorite(HoN_Learner.saved.selectedHero) end)

	--local categories = interface:UICmd("GetHeroCategories('"..HoN_Learner.heroInfo.heroName.."', 1)")
	local ratings = BuildRatingsString(HoN_Learner.heroInfo.ratings)
	if (NotEmpty(ratings)) then
		GetWidget("learner_role"):SetText(ratings)
	else
		GetWidget("learner_role"):SetText("-")
	end

	-- abilities
	for i=1, 4 do
		if (HoN_Learner.heroInfo.abilities[i]) then
			GetWidget("learner_hero_ability_"..i):SetTexture(HoN_Learner.heroInfo.abilities[i].iconPath)
		end
	end

	-- usage stats
	HoN_Learner:PopulateUsageStats()

	-- average match stats
	HoN_Learner:UpdateMatchAverage()

	-- alts
	HoN_Learner:SetupAltAvatars()
	HoN_Learner:PopulateAltAvatars()

	-- model (the default alt)
	HoN_Learner:PopulateModel()

	-- update the breadcrumbs
	-- HoN_Learner:PopulateBreadCrumbs()

	-- Set Mastery Info
	HoN_Learner:SetMasteryInfo(HoN_Learner.saved.selectedHero)	
end

-- function HoN_Learner:PopulateBreadCrumbs()
-- 	local lastExisting = nil

-- 	for i=1,3 do
-- 		if (HoN_Learner.breadCrumbs[i] and HoN_Learner.breadCrumbs[i] ~= "") then
-- 			GetWidget("learner_breadcrumb_"..i.."_text"):SetColor("#37b2d9")
-- 			if (i ~= 1) then
-- 				GetWidget("learner_breadcrumb_"..i.."_text"):SetText(" > "..HoN_Learner.breadCrumbs[i])
-- 			else
-- 				GetWidget("learner_breadcrumb_"..i.."_text"):SetText(HoN_Learner.breadCrumbs[i])
-- 			end
-- 			GetWidget("learner_breadcrumb_"..i):SetVisible(1)
-- 			lastExisting = i
-- 		else
-- 			GetWidget("learner_breadcrumb_"..i):SetVisible(0)
-- 		end
-- 	end

-- 	if (lastExisting) then
-- 		GetWidget("learner_breadcrumb_"..lastExisting.."_text"):SetColor("1 1 1 1")
-- 	end
-- end

-- this is a hackish thing that resembles breadcrumbs, it in no way really acts like them...
-- function HoN_Learner:ClickBreadCrumb(level)
-- 	level = tonumber(level)
-- 	if (level == 1) then
-- 		level = 2 end

-- 	if (level == 2) then -- this will always be the hero info, and honestly the only thing we need support for
-- 		for i=(level+1), 3 do 	-- clear out all higher levels
-- 			HoN_Learner.breadCrumbs[i] = nil
-- 		end

-- 		HoN_Learner:ClickTopTab(4) -- I'm a cheater
-- 	end
-- end

function HoN_Learner:ClickUsageGraph()
	HoN_Learner:ClickTopTab(5)
end

function HoN_Learner:ClickOpenGuides()
	HoN_Learner:ClickTopTab(6)
end

function HoN_Learner:ClickCreateGuide()
	--HoN_Learner:ClickTopTab(7) 		-- original new inplace guide
	UIManager.GetInterface('webpanel'):HoNWebPanelF('CreateAGuide')
end

function HoN_Learner:PopulateModel(hero, alt)
	if (not hero or hero == "") then
		hero = HoN_Learner.heroInfo.heroName
	end
	if (not alt or alt == "") then
		alt = HoN_Learner.heroAlts[HoN_Learner.selectedAlt].Name
	end

	local heroString = hero .. "." .. alt

	local modelPanel = GetWidget("learner_hero_model")
	modelPanel:UICmd("SetModelPos(GetHeroStorePosFromProduct('"..heroString.."'));")
	modelPanel:UICmd("SetModelAngles(GetHeroStoreAnglesFromProduct('"..heroString.."'));")
	modelPanel:UICmd("SetModelScale(GetHeroStoreScaleFromProduct('"..heroString.."'));")
	modelPanel:UICmd("SetModel(GetHeroPreviewModelPathFromProduct('"..heroString.."'));")
	if (hero ~= "Hero_Empath" and hero ~= "Hero_Gemini" and hero ~= "Hero_ShadowBlade" and hero ~= "Hero_Dampeer" and heroString ~= "Hero_Ra.Alt2") then
		modelPanel:UICmd("SetEffect(GetHeroStorePassiveEffectPathFromProduct('"..heroString.."'));")
	else
		modelPanel:UICmd("SetEffect(GetHeroPassiveEffectPathFromProduct('"..heroString.."'));")
	end
	modelPanel:UICmd("SetTeamColor('1 0 0 ');")
end

function HoN_Learner:PopulateAltAvatars(dontFadeSelection)
	local numAlts = #HoN_Learner.heroAlts
	if (numAlts <= 3) then
		GetWidget("hero_alt_scroll_up"):FadeOut(150)
		GetWidget("hero_alt_scroll_down"):FadeOut(150)
	else
		if (HoN_Learner.altAvatarOffset > 0) then GetWidget("hero_alt_scroll_up"):FadeIn(150) else GetWidget("hero_alt_scroll_up"):FadeOut(150) end
		if ((HoN_Learner.altAvatarOffset + 3) < numAlts) then GetWidget("hero_alt_scroll_down"):FadeIn(150) else GetWidget("hero_alt_scroll_down"):FadeOut(150) end
	end

	for i=1, 3 do
		if (HoN_Learner.heroAlts[i + HoN_Learner.altAvatarOffset]) then
			if (HoN_Learner.heroAlts[i + HoN_Learner.altAvatarOffset].AvatarDisplayName) then
				GetWidget("hero_alt_button_"..i.."_name"):SetText(HoN_Learner.heroAlts[i + HoN_Learner.altAvatarOffset].AvatarDisplayName)
			else
				GetWidget("hero_alt_button_"..i.."_name"):SetText(HoN_Learner.heroAlts[i + HoN_Learner.altAvatarOffset].DisplayName)
			end
			GetWidget("hero_alt_button_"..i.."_icon"):SetTexture(HoN_Learner.heroAlts[i + HoN_Learner.altAvatarOffset].Icon)

			if (HoN_Learner.selectedAlt == (i + HoN_Learner.altAvatarOffset)) then
				if (not dontFadeSelection) then
					GetWidget("hero_alt_button_"..i.."_selected"):FadeIn(150)
				else
					GetWidget("hero_alt_button_"..i.."_selected"):SetVisible(1)
				end
			else
				if (not dontFadeSelection) then
					GetWidget("hero_alt_button_"..i.."_selected"):FadeOut(150)
				else
					GetWidget("hero_alt_button_"..i.."_selected"):SetVisible(0)
				end
			end

			-- set the texts
			local heroName = HoN_Learner.heroAlts[i + HoN_Learner.altAvatarOffset].TypeName
			local altCode = HoN_Learner.heroAlts[i + HoN_Learner.altAvatarOffset].Name
			local displaySelect, unavailable = true, false

			if (altCode ~= "Base") then
				displaySelect = HoN_Learner.heroAlts[i + HoN_Learner.altAvatarOffset].Available
			else
				if (IsEarlyAccessHero(heroName)) then
					displaySelect = CanAccessEarlyAccessProduct(heroName)
				elseif (not CanAccessHeroProduct(heroName)) then
					if (not AtoB(interface:UICmd("IsHeroProductPurchasable('"..heroName.."')"))) then
						unavailable = true
					end
					displaySelect = false
				end
			end

			if (displaySelect) then
				isTrial = NotEmpty(HoN_Learner.heroAlts[i + HoN_Learner.altAvatarOffset].Trial)
				-- set as default
				GetWidget("hero_alt_button_"..i.."_default"):SetVisible(not isTrial)
				GetWidget("hero_alt_button_"..i.."_label"):SetVisible(isTrial)
				-- set checked or not
				if (HoN_Learner.defaultOffset == (i + HoN_Learner.altAvatarOffset)) then
					GetWidget("hero_alt_button_"..i.."_check"):SetButtonState(1)
				else
					GetWidget("hero_alt_button_"..i.."_check"):SetButtonState(0)
				end
			-- elseif (not AtoB(interface:UICmd("CanAccessAltAvatar('"..heroName.."."..altCode.."')"))) then  	-- ? Don't think I need this, seems to pretty much return if owned
			-- 	-- unavailable
			-- 	GetWidget("hero_alt_button_"..i.."_default"):SetVisible(0)
			-- 	GetWidget("hero_alt_button_"..i.."_label"):SetVisible(1)
			-- 	GetWidget("hero_alt_button_"..i.."_label_text"):SetText(Translate("general_unavailable"))

				GetWidget("hero_alt_button_"..i.."_trial"):SetVisible(isTrial)
				GetWidget("hero_alt_button_"..i.."_gcaicon"):SetVisible(false)

				if IsGCABenifitAltAvatar(heroName, altCode) then
					GetWidget("hero_alt_button_"..i.."_gcaicon"):SetVisible(true)
					GetWidget("hero_alt_button_"..i.."_default"):SetVisible(false)
					GetWidget("hero_alt_button_"..i.."_label"):SetVisible(false)
					GetWidget("hero_alt_button_"..i.."_trial"):SetVisible(false)
				end
			else
				-- go to store!!!!
				GetWidget("hero_alt_button_"..i.."_default"):SetVisible(0)
				GetWidget("hero_alt_button_"..i.."_label"):SetVisible(1)
				if (unavailable) then
					GetWidget("hero_alt_button_"..i.."_label_text"):SetText(Translate("general_unavailable"))
				else
					GetWidget("hero_alt_button_"..i.."_label_text"):SetText(Translate("learn_unowned"))
				end
				GetWidget("hero_alt_button_"..i.."_trial"):SetVisible(0)
				GetWidget("hero_alt_button_"..i.."_gcaicon"):SetVisible(0)
			end			

			GetWidget("hero_alt_button_"..i):SetVisible(1)
		else
			GetWidget("hero_alt_button_"..i):SetVisible(0)
		end
	end
end

function HoN_Learner:ClickAltGotoStore(index)
	local heroName = HoN_Learner.heroAlts[tonumber(index)].TypeName
	local altCode = HoN_Learner.heroAlts[tonumber(index)].Name

	if (IsEarlyAccessHero(heroName) and not CanAccessEarlyAccessProduct(heroName)) then
		OpenStoreToProduct(58, heroName, false) 		-- EAP Hero
	elseif (not CanAccessHeroProduct(heroName)) then
		if (AtoB(interface:UICmd("IsHeroProductPurchasable('"..heroName.."')"))) then
			OpenStoreToProduct(71, heroName, false) 		-- Regular Hero
		end
	else
		OpenStoreToProduct(2, heroName.."."..altCode, false) 	-- Alt
	end
end

function HoN_Learner:ClickAltAvatar(index)
	if (HoN_Learner.selectedAlt ~= (tonumber(index) + HoN_Learner.altAvatarOffset)) then
		HoN_Learner.selectedAlt = tonumber(index) + HoN_Learner.altAvatarOffset

		HoN_Learner:PopulateAltAvatars()
		HoN_Learner:PopulateModel()

		HoN_Learner.modelRotation = 0
		HoN_Learner:UpdateModelRotation()
	end
end

function HoN_Learner:ScrollAltAvatars(dir)
	if (dir == "up" and HoN_Learner.altAvatarOffset > 0) then
		HoN_Learner.altAvatarOffset = HoN_Learner.altAvatarOffset - 1
	elseif (dir == "down" and (HoN_Learner.altAvatarOffset + 3) < #HoN_Learner.heroAlts) then
		HoN_Learner.altAvatarOffset = HoN_Learner.altAvatarOffset + 1
	else
		return
	end

	HoN_Learner:PopulateAltAvatars(true)
end

function HoN_Learner:SetupAltAvatars(heroName)
	if (not heroName or heroName == "") then
		heroName = HoN_Learner.heroInfo.heroName
	end
	HoN_Learner.altAvatarOffset = 0
	HoN_Learner.heroAlts = GetAltAvatars(heroName)
	local default = GetDBEntry('def_av_'..heroName, nil, false, false, false)
	--local default = GetDBEntry('newui_default_avatar_'..Client.GetAccountID()..'_'..heroName, nil, false, false, false)
	if (not default or default == "") then
		default = "Base"
		HoN_Learner:SetDefaultHeroAvatar(heroName, default)
	end

	HoN_Learner.selectedAlt = HoN_Learner:GetAltOffsetFromCode(default)

	if (HoN_Learner.selectedAlt == -1) then
		HoN_Learner.selectedAlt = 1
		HoN_Learner.defaultOffset = 0
	else
		HoN_Learner.defaultOffset = HoN_Learner.selectedAlt
	end
end

function HoN_Learner:GetAltOffsetFromCode(code)
	if (not code or code == "") then
		code = "Base"
	end

	for i,t in ipairs(HoN_Learner.heroAlts) do
		if (t.Name == code) then
			return i
		end
	end

	return -1
end

function HoN_Learner:PopulateDifficulty(difficulty)
	local halfPoint = false
	if (difficulty ~= math.ceil(difficulty)) then
		halfPoint = true
		difficulty = difficulty - 0.5
	end

	for i=1, 5 do
		if (i <= difficulty) then
			GetWidget("compendium_hero_difficulty_"..i):SetWidth("100@")
			GetWidget("compendium_hero_difficulty_"..i):SetVisible(1)
		elseif ((i-1) == difficulty and halfPoint) then
			GetWidget("compendium_hero_difficulty_"..i):SetWidth("50@")
			GetWidget("compendium_hero_difficulty_"..i):SetVisible(1)
		else
			GetWidget("compendium_hero_difficulty_"..i):SetVisible(0)
		end
	end
end

function HoN_Learner:PopulateUsageStats()
	-- usage stats
	local heroUsage = HoN_Learner:GetUsageStatsFromHeroName(HoN_Learner.heroInfo.heroName)
	if (heroUsage) then
		GetWidget("learner_loading_usage"):SetVisible(0)
		GetWidget("learner_used"):SetText(FtoA(heroUsage[5], 0, 0 ,","))
		GetWidget("learner_used_percent"):SetText(heroUsage[2].."%")
		GetWidget("learner_wins"):SetText(FtoA(heroUsage[6], 0, 0 ,","))
		GetWidget("learner_wins_percent"):SetText(heroUsage[3].."%")
		GetWidget("learner_loss"):SetText(FtoA(heroUsage[7], 0, 0 ,","))
		GetWidget("learner_loss_percent"):SetText(heroUsage[4].."%")
	else
		-- display the a throb, we will fill it out in the results
		GetWidget("learner_loading_usage"):SetVisible(1)
		GetWidget("learner_used"):SetText("0")
		GetWidget("learner_used_percent"):SetText("0".."%")
		GetWidget("learner_wins"):SetText("0")
		GetWidget("learner_wins_percent"):SetText("0".."%")
		GetWidget("learner_loss"):SetText("0")
		GetWidget("learner_loss_percent"):SetText("0".."%")
	end
end

function HoN_Learner:PlayVoiceSample(heroName, altCode)
	if (not heroName or heroName == "") then
		heroName = HoN_Learner.heroInfo.heroName
	end
	if (not altCode or altCode == "") then
		altCode = HoN_Learner.heroAlts[HoN_Learner.selectedAlt].Name
	end

	interface:UICmd("PlayHeroPreviewSoundFromProduct('"..heroName.."."..altCode.."');")
end

function HoN_Learner:ClickDefaultAvatar(offset)
	HoN_Learner:ClickAltAvatar(offset) -- select the alt as well :3
	
	offset = tonumber(offset) + HoN_Learner.altAvatarOffset
	HoN_Learner.defaultOffset = offset 

	HoN_Learner:SetDefaultHeroAvatar(HoN_Learner.heroAlts[offset].TypeName, HoN_Learner.heroAlts[offset].Name)
	HoN_Learner:PopulateAltAvatars()
end

function HoN_Learner:SetDefaultHeroAvatar(heroEntity, avatarCode)
	if (avatarCode == "") then avatarCode = "Base" end

	if (avatarCode) and (heroEntity) and NotEmpty(heroEntity) then
		GetDBEntry('def_av_'..heroEntity, avatarCode, true, false, true)
		--GetDBEntry('newui_default_avatar_'..Client.GetAccountID()..'_'..heroEntity, avatarCode, true, false, true)
		SetDefaultAvatar(heroEntity, avatarCode)	
	end
end

local function HeroInfoHelper(widget, ...)
	if (AtoB(arg[1])) then
		HoN_Learner.heroInfo.isValid = true
		HoN_Learner.heroInfo.icon 			= arg[2]
		HoN_Learner.heroInfo.displayName 	= arg[3]
		HoN_Learner.heroInfo.desc			= arg[4]
		HoN_Learner.heroInfo.attribute 		= arg[5]
		HoN_Learner.heroInfo.armor 			= arg[18]
		HoN_Learner.heroInfo.attackTypeIcon = "/ui/fe2/lobby/"..arg[24]..".tga"
		HoN_Learner.heroInfo.role 			= arg[25]

		HoN_Learner.heroInfo.ratings = {
			['solo'] = tonumber(arg[26]),
			['jungle'] = tonumber(arg[27]),
			['carry'] = tonumber(arg[28]),
			['support'] = tonumber(arg[29]),
			['initiator'] = tonumber(arg[30]),
			['ganker'] = tonumber(arg[31]),
			['ranged'] = tonumber(arg[32]),
			['melee'] = tonumber(arg[33]),
			['pusher'] = tonumber(arg[34]),
		}

		if (HoN_Learner.heroInfo.heroName) then
			HoN_Learner.heroInfo.difficulty = tonumber(interface:UICmd("GetHeroDifficulty('"..HoN_Learner.heroInfo.heroName.."')"))
		else
			HoN_Learner.heroInfo.difficulty = 0
		end
	else
		HoN_Learner.heroInfo.isValid = false
	end
end
triggerHelper:RegisterWatch("HeroSelectHeroInfo", HeroInfoHelper)

local function HeroAbilityHelper(abilityNum, widget, ...)
	offset = abilityNum + 1
	if (arg[1]) then
		HoN_Learner.heroInfo.abilities[offset] = {}
		HoN_Learner.heroInfo.abilities[offset].isValid 		= true
		HoN_Learner.heroInfo.abilities[offset].iconPath 	= arg[2]
	else
		HoN_Learner.heroInfo.abilities[offset].isValid = false
	end
end
for i=0, 3 do triggerHelper:RegisterWatch("HeroSelectHeroAbilityInfo"..i, function(...) HeroAbilityHelper(i, ...) end) end

function HoN_Learner:GetUsageStats(evenIfCached)
	if ((evenIfCached and AtoB(evenIfCached)) or not HoN_Learner.heroUsagesCached) then
		interface:UICmd("SubmitForm('HeroUsageListForm', 'f', 'get_hero_usage_list', 'cookie', GetCookie(), 'sort', 'use')")
	end
end

function HoN_Learner:RecieveUsageStats(widget, errorStr, successStr, dataStr, totalUsage)
	if ((not errorStr or errorStr == "") and (successStr or successStr ~= "") and (dataStr or dataStr ~= "")) then
		HoN_Learner.heroUsagesCached = true
		HoN_Learner.heroUsages = {} -- clear out the table if it already had info in it

		local perHeroStrings = explode("`", dataStr)
		for _,v in pairs(perHeroStrings) do
			local heroTempSortTable = explode("|", v)
			heroTempSortTable.winPercent	= tonumber(heroTempSortTable[6]) / tonumber(heroTempSortTable[5]) * 100
			heroTempSortTable.lossPercent	= tonumber(heroTempSortTable[7]) / tonumber(heroTempSortTable[5]) * 100
			table.insert(HoN_Learner.heroUsages, heroTempSortTable)
		end

		HoN_Learner.totalUsage = totalUsage
		HoN_Learner.lastUsageSort = "use"	-- the form is requested with this sort, so it's the order we get them in initally

		-- populate the usage for the selected hero
		HoN_Learner:PopulateUsageStats()
		-- populate the usage list
		HoN_Learner:PopulateUsageList()
	else
		-- error
		GetWidget("learner_usage_loading_wait"):SetVisible(0)
		GetWidget("learner_usage_loading_error"):SetVisible(1)
		GetWidget("learner_usage_loading_throb"):SetVisible(0)
	end
end
triggerHelper:RegisterWatch("HeroUsageListFormResult", function (...) HoN_Learner:RecieveUsageStats(...) end)

function HoN_Learner:HandleUsageStatsStatus(widget, statusCode)
	if (tonumber(statusCode) == 3) then -- error
		GetWidget("learner_usage_loading_wait"):SetVisible(0)
		GetWidget("learner_usage_loading_error"):SetVisible(1)
		GetWidget("learner_usage_loading_throb"):SetVisible(0)
	else
		GetWidget("learner_usage_loading_wait"):SetVisible(1)
		GetWidget("learner_usage_loading_error"):SetVisible(0)
		GetWidget("learner_usage_loading_throb"):SetVisible(1)
	end
end
triggerHelper:RegisterWatch("HeroUsageListFormStatus", function (...) HoN_Learner:HandleUsageStatsStatus(...) end)

---- Hero usage table layout
-- 1- Hero_Name
-- 2- Used %
-- 3- Win %
-- 4- Loss %
-- 5- Used #
-- 6- Win #
-- 7- Loss #
----
function HoN_Learner:SortHeroUsage(sortType)
	if (HoN_Learner.lastUsageSort and HoN_Learner.lastUsageSort == sortType) then
		return  	-- no need to resort
	end
	-- "use", "win", "loss"
	local sortFunc = nil

	if (sortType == "win") then
		sortFunc = function(a, b)
			return tonumber(a.winPercent) > tonumber(b.winPercent)
		end
	elseif (sortType == "loss") then
		sortFunc = function(a, b)
			return tonumber(a.lossPercent) > tonumber(b.lossPercent)
		end
	elseif (sortType == "use") then
		sortFunc = function(a, b)
			return tonumber(a[5]) > tonumber(b[5])
		end
	end
	-- yes, the above can be condensed to a single variable for the offset
	-- however, with it like this we can add more complex sorts easily
	--			(alphabetic sorting being the only thing I can think of...)

	if (sortFunc) then
		table.sort(HoN_Learner.heroUsages, sortFunc)
		HoN_Learner.lastUsageSort = sortType

		-- update the usage boxes
		HoN_Learner:PopulateUsageList()
	end
end

function HoN_Learner:PopulateUsageList()
	if (not HoN_Learner.heroUsages) then
		return end

	local scrollbox = GetWidget("compendium_usage_list_scrolbox")
	local rowWidget = nil

	-- clear out any old list
	groupcall("compendium_hero_usage_list_boxes", "DoEvent(3)")

	scrollbox:Sleep(1, function(...) 	-- sleep so the above group call can finish
		-- add the top row template
		scrollbox:Instantiate(
			"compendium_usage_list_top_row_template", 
			"total", tostring(HoN_Learner.totalUsage),
			"value", tostring(HoN_Learner.totalUsage)
		);
		GetWidget("herodex_hero_usage_list_dropdown"):DoEvent()
		GetWidget("herodex_header_usage_list"):SetText(Translate("compendium_usage_list_header", "value", FtoA(HoN_Learner.totalUsage, 0, 0 ,",")))

		-- add all the new widgets
		for i=0, (#HoN_Learner.heroUsages - 1) do
			if (not WidgetExists("compendium_usage_list_row_"..tostring(math.floor(((i+0.1) / 6) + 1)))) then
				scrollbox:Instantiate("compendium_usage_list_row_template", "id", tostring(math.floor(((i+0.1) / 6) + 1)))
				rowWidget = GetWidget("compendium_usage_list_row_"..tostring(math.floor(((i+0.1) / 6) + 1)))
			end

			rowWidget:Instantiate(
				"compendium_usage_hero_box",
				"index", i+1,
				"hero", HoN_Learner.heroUsages[i+1][1],
				"use", HoN_Learner.heroUsages[i+1][2],
				"win", round(HoN_Learner.heroUsages[i+1].winPercent),
				"lose", round(HoN_Learner.heroUsages[i+1].lossPercent),
				"total_use", HoN_Learner.heroUsages[i+1][5],
				"total_win", HoN_Learner.heroUsages[i+1][6],
				"total_lose", HoN_Learner.heroUsages[i+1][7],
				"row", tostring(math.floor(((i+0.1) / 6) + 1))
			)

			scrollbox:UICmd("SetClipAreaToChild();")
		end
	end )
end

function HoN_Learner:SetUsageSortBox(box)
	box:UICmd("SetSelectedItemByValue('"..HoN_Learner.lastUsageSort.."');")
end

function HoN_Learner:GetUsageStatsFromHeroName(heroName)
	if (not HoN_Learner.heroUsages) then return end

	for i, t in pairs(HoN_Learner.heroUsages) do
		if (t[1] == heroName) then
			return t
		end
	end

	return nil
end

function HoN_Learner:RotateButtonDown(direction)
	local modelPanel = GetWidget("learner_hero_model") -- less lua table widgets if we just reg it on the model panel
	local addAmount = nil

	if (direction == "left") then
		addAmount = 0.21 			-- degrees per ms
	elseif (direction == "right") then
		addAmount = -0.21
	end

	if (addAmount) then
		modelPanel.onframe = function()
			HoN_Learner.modelRotation = HoN_Learner.modelRotation + (GetFrameTime() * addAmount)
			HoN_Learner:UpdateModelRotation()
			if (HoN_Learner.modelRotation > 360) then
				HoN_Learner.modelRotation = HoN_Learner.modelRotation - 360
			elseif (HoN_Learner.modelRotation < 0) then
				HoN_Learner.modelRotation = HoN_Learner.modelRotation + 360
			end
			HoN_Learner:UpdateModelRotation()
		end

		modelPanel:RefreshCallbacks()
	end
end

function HoN_Learner:RotateButtonUp()
	local modelPanel = GetWidget("learner_hero_model")
	modelPanel.onframe = nil
	modelPanel:RefreshCallbacks()
end

function HoN_Learner:UpdateModelRotation(rotOverride)
	local rot
	if (rotOverride) then
		rot = rotOverride
	else
		rot = HoN_Learner.modelRotation
	end

	GetWidget("learner_hero_model"):UICmd("RotateCameraAroundModel("..rot..")")
end

function HoN_Learner:UpdateMatchAverage()
	Set("compendium_heroLastViewed", HoN_Learner.heroInfo.heroName, "string")
	GetWidget("hero_stats_submit_compendium"):DoEvent()
end

function HoN_Learner:ClickSideTab(tabNum)
	tabNum = tonumber(tabNum)
	if (HoN_Learner.saved.selectedLeftTab ~= tonumber(tabNum)) then
		local topTab = nil
		if (tabNum ~= 1 and HoN_Learner.saved.selectedTopTab > 4) then
			topTab = 4
		end

		HoN_Learner:SelectTabs(tonumber(tabNum), topTab)
	end
end

function HoN_Learner:ClickTopTab(tabNum)
	tabNum = tonumber(tabNum)
	if (HoN_Learner.saved.selectedTopTab ~= tabNum) then
		-- create a guide goes back to guides, only special case
		if (tabNum == 4 and HoN_Learner.saved.selectedTopTab == 7) then tabNum = 6 end

		HoN_Learner:SelectTabs(nil, tabNum)
	end
end

function HoN_Learner:SelectTabs(leftTab, topTab)
	-- get or save the left tab
	if (not leftTab) then
		leftTab = HoN_Learner.saved.selectedLeftTab
	elseif (leftTab ~= 3) then			-- CHANGE THIS ELSE TAB WHEN THE WALKTHROUGH PANEL IS DONE!!!!
		HoN_Learner.saved.selectedLeftTab = leftTab
	end

	-- get or save the top tab
	if (not topTab) then
		topTab = HoN_Learner.saved.selectedTopTab
	else
		HoN_Learner.saved.selectedTopTab = topTab
	end

	-- set all the panels to invis
	if (leftTab ~= 3) then 			-- REMOVE ME TOOOOOOO
		GetWidget("learner_heroinfo"):FadeOut(100)
		GetWidget("learner_guides"):FadeOut(100)
		GetWidget("learner_usageGraph"):FadeOut(100)
		GetWidget("learner_createAGuide"):FadeOut(100)
		GetWidget("learner_simpleList"):FadeOut(100)
		GetWidget("learner_detailedList"):FadeOut(100)
		GetWidget("learner_masteryList"):FadeOut(100)
		GetWidget("learner_usageList"):FadeOut(100)
		GetWidget("learner_videoCenter"):FadeOut(100)
		GetWidget("learner_items"):FadeOut(100)
		GetWidget("learner_walkthrough"):FadeOut(100)
		GetWidget("learner_top_tabs"):FadeOut(100)
		GetWidget("top_sub_tab_arrow_4"):SetVisible(0)
		GetWidget("top_sub_tab_arrow2_4"):SetVisible(0)
 	end

	if (leftTab == 1) then -- heroes...
		GetWidget("learner_left_hero"):SetButtonState(0)
		GetWidget("learner_left_items"):SetButtonState(1)
		-- GetWidget("learner_left_video"):SetButtonState(1)
		GetWidget("learner_left_walkthrough"):SetButtonState(1)
		GetWidget("learner_top_tabs"):FadeIn(100)

		for i=1,4 do
			GetWidget("top_sub_tab_current"..i):SetVisible(i==topTab)
		end
		GetWidget("top_sub_tab_current8"):SetVisible(8==topTab)

		if (topTab > 4 and topTab ~= 8) then 	-- graph and history tab
			GetWidget("top_sub_tab_current4"):SetVisible(1)
		end

		if (topTab == 1) then -- simple hero list
			GetWidget("learner_simpleList"):FadeIn(100)
		elseif (topTab == 2) then -- detailed hero list
			GetWidget("learner_detailedList"):FadeIn(100)
		elseif (topTab == 3) then -- hero usage list
			GetWidget("learner_usageList"):FadeIn(100)
		elseif (topTab == 4) then -- hero info
			-- HoN_Learner.breadCrumbs[2] = Translate("learn_tabs_hero_info") 	-- add the bread crumb
			-- HoN_Learner.breadCrumbs[3] = nil
			-- HoN_Learner:PopulateBreadCrumbs()
			--HoN_Learner:PopulateHeroInfoPanel()
			GetWidget("learner_heroinfo"):FadeIn(100)
		elseif (topTab == 5) then -- usage graph 					--- Not sure about these 2 guys
			GetWidget("submit_herousagegraphform"):DoEvent()
			GetWidget("top_sub_tab_arrow_4"):SetVisible(1)
			GetWidget("top_sub_tab_arrow2_4"):SetVisible(1)
			-- HoN_Learner.breadCrumbs[3] = Translate("compendium_usage")
			-- HoN_Learner:PopulateBreadCrumbs()

			GetWidget("learner_usageGraph"):FadeIn(100)
		elseif (topTab == 6) then -- guides
			Set("compendium_guide_newRound", 1)
			--Set("compendium_usage_useCache", 0);
			Trigger("compendium_guideHeroName", HoN_Learner.heroInfo.heroName);
			GetWidget("top_sub_tab_arrow_4"):SetVisible(1)
			GetWidget("top_sub_tab_arrow2_4"):SetVisible(1)
			-- HoN_Learner.breadCrumbs[3] = Translate("compendium_guides")
			-- HoN_Learner:PopulateBreadCrumbs()

			GetWidget("learner_guides"):FadeIn(100)
		elseif (topTab == 7) then -- create a guide
			GetWidget("top_sub_tab_arrow_4"):SetVisible(1)
			GetWidget("top_sub_tab_arrow2_4"):SetVisible(1)
			-- HoN_Learner.breadCrumbs[3] = Translate("tooltip_compendium_createguide")
			-- HoN_Learner:PopulateBreadCrumbs()

			GetWidget("learner_createAGuide"):FadeIn(100)

		elseif (topTab == 8) then 
			GetWidget("learner_masteryList"):FadeIn(100)
			Cmd('GetMasteryHeroInfo')
		end
	elseif (leftTab == 2) then -- video center
		GetWidget("learner_left_hero"):SetButtonState(1)
		-- GetWidget("learner_left_video"):SetButtonState(0)
		GetWidget("learner_left_walkthrough"):SetButtonState(1)

		GetWidget("learner_videoCenter"):FadeIn(100)
	elseif (leftTab == 3) then -- walkthrough
		-- original panel
		GetWidget("learner_left_hero"):SetButtonState(1)
		-- GetWidget("learner_left_video"):SetButtonState(1)
		GetWidget("learner_left_walkthrough"):SetButtonState(0)

		-- GetWidget("learner_walkthrough"):FadeIn(100)

		-- simple button
		HoN_Learner:DoTutorialClicked()
	elseif (leftTab == 4) then -- items
		GetWidget("learner_left_hero"):SetButtonState(1)
		GetWidget("learner_left_items"):SetButtonState(0)
		GetWidget("learner_items"):FadeIn(100)
	end
end

function HoN_Learner:DoTutorialClicked()
	HoN_Learner.promptingTutorial = true

	local doTutorial = function ()
		ResourceManager.LoadContext('splash1') 
		Main.walkthroughPrompted = GetDBEntry('walkthroughPrompted', 1, true, false, true)
		GetWidget('main_splash_new_player'):FadeIn(150)
	end

	if ((not IsInGroup()) and (not IsInQueue()) and (not IsInGame())) then
		doTutorial()
	else
		triggerHelper:SetCallback('onevent1', function() interface:UICmd("LeaveTMMQueue(); LeaveTMMGroup();") doTutorial() end)
		triggerHelper:SetCallback('onevent2', function() HoN_Learner:TutorialPromptEnd() end)
		triggerHelper:RefreshCallbacks()

		Trigger('TriggerDialogBox',
			'main_leavegroup',
			'general_back', 'general_continue',
			'Call(\'Learnatorium_TriggerHelper\', \'DoEvent(2);\');', 'Call(\'Learnatorium_TriggerHelper\', \'DoEvent(1);\');',
			'', 'main_leavegroup_body')
	end
end

function HoN_Learner:TutorialPromptEnd()
	if not HoN_Learner.promptingTutorial then return else HoN_Learner.promptingTutorial = false end
	
	if (HoN_Learner.saved.selectedLeftTab ~= 1) then
		GetWidget("learner_left_hero"):SetButtonState(1)
	else
		GetWidget("learner_left_hero"):SetButtonState(0)
	end

	if (HoN_Learner.saved.selectedLeftTab ~= 2) then
		-- GetWidget("learner_left_video"):SetButtonState(1)
	else
		-- refreash the web browser
		GetWidget("learner_videocenter_browser"):DoEvent()
		-- GetWidget("learner_left_video"):SetButtonState(0)
	end

	GetWidget("learner_left_walkthrough"):SetButtonState(1)
end

function HoN_Learner:RefreshLearningCenter()
	--local webParent = GetWidget("web_browser"):GetParent()
	--local parent = GetWidget("learner_videocenter_browser")

	--if (webParent ~= parent) then
		UIManager.GetInterface('webpanel'):HoNWebPanelF('LoadURLWithThrob', VIDEO_CENTER_URL .. '?lang=' .. GetCvarString('host_language'), GetWidget("learner_videocenter_browser"))
		--GetWidget('web_browser'):UICmd("WebBrowserLoadURL('"..VIDEO_CENTER_URL.."')")
	--end
end

function HoN_Learner:RefreshLearningItems()

	Echo('refreshlearningitems()')

end

function HoN_Learner:ItemDetails(param1)
	
	
	--reset components
	for i=1,6 do
		GetWidget('compendium_detailed_component_' .. i):SetTexture('$invis')
		GetWidget('compendium_detailed_component_' .. i):SetCallback('onmouseover', function(widget) end)
		GetWidget('compendium_detailed_component_' .. i):SetCallback('onmouseout', function(widget) end)
		GetWidget('compendium_detailed_component_' .. i):SetWidth('100%')
		GetWidget('compendium_detailed_component_' .. i):SetHeight('100%')
		GetWidget('compendium_detailed_component_' .. i .. '_recipe'):SetVisible(0)
		GetWidget('compendium_detailed_component_' .. i .. '_frame'):SetVisible(0)
		GetWidget('compendium_detailed_componentlabel'):SetVisible(0)
	end
	
	--reset detailed
	if param1 == tonumber(0) then
		GetWidget('compendium_detailed_itemname'):SetText('')
		GetWidget('compendium_detailed_itemicon'):SetTexture('$invis')
		GetWidget('compendium_detailed_itemicon_frame'):SetVisible(0)
		GetWidget('compendium_detailed_componentlabel'):SetVisible(0)
		return
	end
	
	--plays sound (wont reach this far if its a reset)
	PlaySound('/shared/sounds/ui/button_click_03.wav');
	
	--sets detailed icon and sets its frame visible
	GetWidget('compendium_detailed_itemicon'):SetTexture(GetEntityIconPath(HoN_Learner.itemEntityArray[param1]))
	GetWidget('compendium_detailed_itemicon_frame'):SetVisible(1)
	
	--sets detailed name but makes it smaller if its too long
	if string.len(GetEntityDisplayName(HoN_Learner.itemEntityArray[param1])) > 20 then
		GetWidget('compendium_detailed_itemname'):SetFont('dyn_12')
	else
		GetWidget('compendium_detailed_itemname'):SetFont('dyn_14')
	end
	GetWidget('compendium_detailed_itemname'):SetText(GetEntityDisplayName(HoN_Learner.itemEntityArray[param1]))
	
	--rolls components into detailed
	compArray = GetShopItems(HoN_Learner.currentItemTab)
	for i=1, #compArray do
		local itemName = tostring(itemArray[i]["itemName"])
		if itemName == tostring(HoN_Learner.itemEntityArray[param1]) then
			
			--checks to see if it has components to avoid lua errors
			local componentNum = compArray[i]["components"]					
			if (componentNum) then
				
				--if golds above 0, then the component has a recipie item with it too
				local componentGold = tostring(itemArray[i]["itemCostOnly"])
				if tonumber(componentGold) > 0 then
					local rWig = (#componentNum + 1)
					--GetWidget('compendium_detailed_component_' .. rWig):SetTexture('/ui/elements/shop/scroll_overlay.tga')
					
					Trigger('compendium_detailed_component_' .. rWig .. '_trigger', 1)
					
					--sets mouseover functions for component tooltips
					GetWidget('compendium_detailed_component_' .. rWig .. '_recipe'):SetCallback('onmouseover', function(widget)	
						widget:UICmd("TriggerItemTooltip('MSPlayerInventoryTip', '".. HoN_Learner.itemEntityArray[i] .."', 1)")
						Trigger('MSPlayerInventoryTipIcon', '/ui/elements/shop/scroll_overlay.tga')
						GetWidget('endgameItemTooltip'):SetVisible(true)
						
					end)
					
					--sets mouseout to hide them
					GetWidget('compendium_detailed_component_'..rWig .. '_recipe'):SetCallback('onmouseout', function(widget)
						GetWidget('endgameItemTooltip'):SetVisible(false)
					end)
					
				end
				
				for i=1, #compArray[param1]["components"] do
					
					GetWidget('compendium_detailed_component_' .. i):SetTexture(GetEntityIconPath(compArray[param1]["components"][i]))
					
					--sets mouseover functions for component tooltips
					GetWidget('compendium_detailed_component_' .. i):SetCallback('onmouseover', function(widget)	
						widget:UICmd("TriggerItemTooltip('MSPlayerInventoryTip', '"..compArray[param1]["components"][i].."', 0)")
						Trigger('MSPlayerInventoryTipIcon', GetEntityIconPath(compArray[param1]["components"][i]))
						GetWidget('endgameItemTooltip'):SetVisible(true)
						
					end)
					
					--sets mouseout to hide them
					GetWidget('compendium_detailed_component_'..i):SetCallback('onmouseout', function(widget)
						GetWidget('endgameItemTooltip'):SetVisible(false)
					end)
					
					GetWidget('compendium_detailed_componentlabel'):SetVisible(1)
					GetWidget('compendium_detailed_component_' .. i .. '_frame'):SetVisible(1)
					
				end
			end
		end
	end
	
end

function HoN_Learner:SelectItem(self)

	--HoN_Learner.itemEntityArray

	local wFrame = tostring(self:GetName() .. 'frame')
	local wGlow = tostring(self:GetName() .. 'glow')
	
	if HoN_Learner.currentitemSelected == wFrame then
		GetWidget(wGlow):SetVisible(0)
		HoN_Learner.currentitemSelected = ''
		HoN_Learner:ItemDetails(0)
		return
	end
	
	local iTabs = GetWidget('compendium_item_listbox'):GetListItems()
	for i=1, #iTabs do
		local checkFrame = tostring(GetWidget('compendium_item_instance' .. i):GetName() .. 'frame')
		local checkGlow = tostring(GetWidget('compendium_item_instance' .. i):GetName() .. 'glow')
		if wFrame == checkFrame then
			GetWidget(wGlow):SetVisible(1)
			HoN_Learner.currentitemSelected = tostring(wFrame)
			HoN_Learner:ItemDetails(i)
		else
			GetWidget(checkGlow):SetVisible(0)
		end
	end
	
end

function HoN_Learner:ItemPopulate(catType)
	
	--resets item selected
	HoN_Learner.currentitemSelected = ''
	
	--resets detail section
	HoN_Learner:ItemDetails(0)
	
	--checks to see if current tab is same as one pressed, if so, function is canceled
	if tostring(HoN_Learner.currentItemTab) ~= tostring(catType) then
		HoN_Learner.currentItemTab = (tostring(catType))
	else
		return
	end
	
	--loops over all category instances and runs category selection graphic
	local catTabs = GetWidget('compendium_tabinstance_collection'):GetChildren()
	for i=1, #catTabs do
		local tabInst = tostring(catTabs[i]:GetName())
		if tostring('compendium_item_tabframe_' .. catType) == tostring(tabInst) then
			--changes color if rest of categorys are recipies instead of components
			if tonumber(i) > 6 then
				--blue outline
				GetWidget(tabInst .. 'frame'):SetBorderColor('#43a5d1')
			else
				--green outline
				GetWidget(tabInst .. 'frame'):SetBorderColor('#45b16b')
			end
		else
			GetWidget(tabInst .. 'frame'):SetBorderColor('1 1 1 .05')
		end
	end
	
	
	local function ItemPopulateFunction()
	
		--Set current shop
		itemArray = GetShopItems(catType)
		
		--reset global entity item array
		HoN_Learner.itemEntityArray = {}
		
		--SOUND
		PlaySound('/shared/sounds/ui/button_click_01.wav');
		
		local function PopulateItems()
			for i=1, #itemArray do
				--terminates PopulateItems() once its reached last item
				if i > #itemArray then
					return
				end
				
				--gets the item name/icon
				local itemName = tostring(GetEntityDisplayName(itemArray[i]["itemName"]))
				local itemEntityName = tostring(itemArray[i]["itemName"])
				local itemIcon = tostring(GetEntityIconPath(itemArray[i]["itemName"]))
				local itemGold = tostring(itemArray[i]["itemCost"])

				
				--Gets first component of the first item, for however many items where in the shop (so this is wrong)
				--Echo(itemArray[1]["components"][1])
				
				--Example table lmao
				--nICOSMasterPlanToConquertheWorld = {'Item_WhisperingHelm', 'Item_KawaiiSucks', 'Item_UnityIsbEtterThanUnreal4'}
				
				
				--Fills slots with items
				GetWidget('compendium_item_listbox'):AddTemplateListItem('compendium_item_instance', 1, 
				'id', i, 
				'itemname', itemName, 
				'texture', itemIcon, 
				'gold', itemGold
				)
				
				--adds to global entity item array
				table.insert(HoN_Learner.itemEntityArray, itemEntityName)
				
				
				
				--sets mouseover functions for inventory tooltips
				GetWidget('compendium_item_instance' .. i):SetCallback('onmouseover', function(widget)	
					widget:UICmd("TriggerItemTooltip('MSPlayerInventoryTip', '"..itemEntityName.."', 0)")
					Trigger('MSPlayerInventoryTipIcon', itemIcon)
					GetWidget('endgameItemTooltip'):SetVisible(true)
					local fw = GetWidget(tostring(widget:GetName() .. 'texture'))
					fw:SetBorderColor('.48 .48 .48 1')
					PlaySound('/shared/sounds/ui/button_over_02.wav')
				end)
				
				--sets mouseout to hide them
				GetWidget('compendium_item_instance'..i):SetCallback('onmouseout', function(widget)
					GetWidget('endgameItemTooltip'):SetVisible(false)
					local fw = GetWidget(tostring(widget:GetName() .. 'texture'))
					fw:SetBorderColor('.28 .28 .28 .62')
				end)
				
				--sets onclick to show additional item info
				--GetWidget('compendium_item_instance' .. i):SetCallback('onclick', function(widget)
				--	local childArray = GetWidget('compendium_item_listbox'):GetChildren()
				--	Echo(#childArray)
				--end)
				
			end
		end
		
		PopulateItems()
		
	end
	
	--clear current item slots
	--HoN_Learner:PurgeItemRows()
	Trigger('LearnaItemsClearList')
	
	GetWidget('compendium_item_rowspanel'):Sleep(1, function() ItemPopulateFunction() end)
	
	
end

function HoN_Learner:InventoryPopup()

	local x = Input.GetCursorPosX()
	local y = Input.GetCursorPosY()
	local w = tonumber(GetWidget('endgameItemTooltip'):GetWidth())
	local h = tonumber(GetWidget('endgameItemTooltip'):GetHeight())
	local screenW = GetScreenWidth()
	local screenH = GetScreenHeight()
	if (x+w) > screenW then x = screenW - w end
	if (y+h) > screenH then y = screenH - h end
	GetWidget('endgameItemTooltip'):SetX(tostring(x))
	GetWidget('endgameItemTooltip'):SetY(tostring(y))
	GetWidget('endgameItemTooltip'):SetExclusive()

end

function HoN_Learner:RefreshCreateGuide()
	local webParent = GetWidget("web_browser"):GetParent()
	local parent = GetWidget("learner_createAGuide_browser")

	if (webParent ~= parent) then
		UIManager.GetInterface('webpanel'):HoNWebPanelF('LoadURLWithThrob', '!hon/guides/select.php?cookie='..(interface:UICmd("GetCookie()")), parent)
	end
end

function HoN_Learner:ClickHero(heroName)
	if (HoN_Learner.saved.selectedHero ~= heroName) then
		HoN_Learner.saved.selectedHero = heroName
		HoN_Learner.heroInfo.heroName = heroName
		Set("compendium_heroLastViewed", heroName, "string")
		interface:UICmd("GetHeroInfo('"..HoN_Learner.saved.selectedHero.."');")
		
		HoN_Learner:PopulateHeroInfoPanel()

		HoN_Learner.modelRotation = 0
		HoN_Learner:UpdateModelRotation()
	end
	HoN_Learner:SelectTabs(1, 4)
end

function HoN_Learner:HeroRefresh(resultParam)
	-- 3 means something was purchased, we don't know what
	if (resultParam == 3) then
		HoN_Learner.forceHeroInfoUpdate = true
 	end
end
triggerHelper:RegisterWatch("MicroStoreResults", function(...) HoN_Learner:HeroRefresh(tonumber(arg[3])) end)

function OpenLearnatorium(leftTab, topTab, hero, forceOpen)
	if (leftTab and topTab) then
		HoN_Learner.overrideOpen = true
		local hero = string.gsub(hero, '.Hero', '')
		if (hero and hero ~= "") then
			HoN_Learner.saved.selectedHero = hero
			HoN_Learner.forceHeroInfoUpdate = true
		end

		HoN_Learner:SelectTabs(leftTab, topTab)
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'compendium', nil, forceOpen)
	end
end
triggerHelper:RegisterWatch("matchstats_hero_helper", function(_, heroName) OpenLearnatorium(1, 4, heroName, false) end)

------- Favorites Stuff -------
function HoN_Learner:RegisterFavoriteButton(button, heroName)
	HoN_Learner.favoriteButtons[heroName] = button

	if (HoN_Learner.favoriteHeroes[heroName]) then
		button:SetButtonState(0)
	else
		button:SetButtonState(1)
	end
end

function HoN_Learner:ToggleFavorite(heroName)
	local value = not HoN_Learner.favoriteHeroes[heroName]

	-- delete stuff if it's false (less stuff to cycle through)
	if (value == false) then
		HoN_Learner.favoriteHeroes[heroName] = nil
	else
		HoN_Learner.favoriteHeroes[heroName] = value
	end

	-- save the db
	GetDBEntry("picker_favorite_heroes", HoN_Learner.favoriteHeroes, true)

	-- update the game lobby if we are in hero pick
	if ((UIGamePhase() >= 2) and Game_Lobby) then
		Game_Lobby:UpdateFavoriteButtonStates()
		Game_Lobby:UpdateFilteredHeroes()
		Game_Lobby:SetFilterFavorites()
	end

	HoN_Learner:UpdateFavoriteButtonStates()
end

function HoN_Learner:UpdateFavoriteButtonStates()
	-- Update the list buttons
	for hero, button in pairs(HoN_Learner.favoriteButtons) do
		if (HoN_Learner.favoriteHeroes[hero]) then
			button:SetButtonState(0)
		else
			button:SetButtonState(1)
		end
	end

	-- update the info button
	if (HoN_Learner.favoriteHeroes[HoN_Learner.saved.selectedHero]) then
		GetWidget("learner_hero_favorite_button"):SetButtonState(0)
	else
		GetWidget("learner_hero_favorite_button"):SetButtonState(1)
	end
end

function HoN_Learner:ShowHeroFavorite(heroEntity)
	if (NotEmpty(heroEntity)) then
		HoN_Learner.favoriteButtons[heroEntity]:SetVisible(HoN_Learner.favoriteHeroes[heroEntity] or false)
	end
end

function HoN_Learner:MouseOutFavorite(heroEntity)
	if (NotEmpty(heroEntity)) then
		if (not HoN_Learner.favoriteHeroes[heroEntity]) then
			GetWidget("learner_favorite_"..heroEntity):FadeOut(150)
		end
	end
end

function HoN_Learner:MouseOverTrial(index)
	local trialEnd = HoN_Learner.heroAlts[tonumber(index) + HoN_Learner.altAvatarOffset].Trial
	if (NotEmpty(trialEnd)) then
		Trigger("trial_avatar_info", trialEnd)
		GetWidget("trial_avatar_tip"):SetVisible(1)
	else
		GetWidget("trial_avatar_tip"):SetVisible(0)
	end
end
-------------------------------

function interface:HoNLearnerF(func, ...)
	if (HoN_Learner[func]) then
		print(HoN_Learner[func](HoN_Learner, ...))
	else
		print('HoNLearnerF failed to find: ' .. tostring(func) .. '\n')
	end
end

interface:RegisterWatch('MasteryUpdateInfo', function(_, ...) HoN_Learner:SetMasteryInfo(HoN_Learner.saved.selectedHero) Cmd('GetMasteryHeroInfo') end)