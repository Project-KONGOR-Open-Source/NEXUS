local interface = object

local lastUpdateFromLogin = false

local function questsRegisterQuest(object, index, prefix, showRefresh)
	prefix = prefix or ''
	if showRefresh == nil then
		showRefresh = true
	end
	local container		= GetWidget('questsQuest'..prefix..index)
	local button		= GetWidget('questsQuest'..prefix..index..'Button')
	local buttonRefresh	= GetWidget('questsQuest'..prefix..index..'ButtonRefresh')

	buttonRefresh:SetVisible(showRefresh)

	local questID			= -1
	local addTimestamp		= '--'
	local progressCurrent	= 0
	local progressMax		= 3

	local function setProgress(newProgressCur, newProgressMax)
		progressCurrent = newProgressCur or 0
		progressMax = newProgressMax or 0
		GetWidget('questsQuest'..prefix..index..'ProgressLabel'):SetText(progressCurrent .. '/' .. progressMax )
		GetWidget('questsQuest'..prefix..index..'Progress'):SetVisible(progressMax > 1)
	end

	buttonRefresh:SetCallback('onclick', function(widget)
		if questID >= 0 then
			PlaySound('/ui/fe2/quests/sounds/button_reroll_'..math.random(1, 2)..'.wav')
			questsPopulateResetPrompt(index, questID)
		end
	end)

	buttonRefresh:SetCallback('onmouseover', function(widget)
		Trigger('genericMainFloatingTip', 'true', '24.0h', '', 'quests_reset', 'quests_reset_tip', '', '', '22', '-22')
	end)

	buttonRefresh:SetCallback('onmouseout', function(widget)
		Trigger('genericMainFloatingTip', 'false', '', '', '', '', '', '', '', '')
	end)

	buttonRefresh:SetCallback('onmouseoverdisabled', function(widget)
		Trigger('genericMainFloatingTip', 'true', '24.0h', '', 'quests_reset_disabled', 'quests_reset_disabled_tip', '', '', '22', '-22')
	end)

	buttonRefresh:SetCallback('onmouseoutdisabled', function(widget)
		Trigger('genericMainFloatingTip', 'false', '', '', '', '', '', '', '', '')
	end)

	buttonRefresh:RegisterWatch('questsResetCountUpdate', function(widget, resetCount)
		widget:SetVisible(AtoN(resetCount) > 0)
	end)

	--[[
	button:SetCallback('onclick', function(widget)

	end)

	button:SetCallback('onmouseover', function(widget)
		
	end)

	button:SetCallback('onmouseout', function(widget)
		
	end)
	--]]

	button:RegisterWatch('questsBusy', function(widget, isBusy)
		widget:SetEnabled(not AtoB(isBusy))
	end)

	buttonRefresh:RegisterWatch('questsBusy', function(widget, isBusy)
		widget:SetEnabled(not AtoB(isBusy))
	end)


	local function getIconPath()
		return iconPath
	end

	local animateOutTime	= 1000		-- Assumed time to animate out (not representative of the amount of time it actually takes)
	local animateThread		= nil

	local function checkKillAnimateThread()
		if animateThread ~= nil then
			animateThread:Kill()
			animateThread = nil
		end
	end

	local function animateOut()
		checkKillAnimateThread()

		animateThread = newthread(function()
			local iconBody	= GetWidget('questsQuest'..prefix..index..'IconBody')
			local name		= GetWidget('questsQuest'..prefix..index..'Name')
			local desc		= GetWidget('questsQuest'..prefix..index..'Description')

			iconBody:Scale('15%', '15%', 1000)
			wait(750)
			iconBody:FadeOut(250)

			name:SlideY('-2h', 250)
			desc:SlideY('2h', 250)
			name:FadeOut(250)
			desc:FadeOut(250)

			GetWidget('questsQuest'..prefix..index..'Progress'):FadeOut(250)
			GetWidget('questsQuest'..prefix..index..'RefreshContainer'):FadeOut(250)

			animateThread = nil
		end)
	end

	local function animateIn()
		checkKillAnimateThread()

		animateThread = newthread(function()

			local iconBody	= GetWidget('questsQuest'..prefix..index..'IconBody')
			local name		= GetWidget('questsQuest'..prefix..index..'Name')
			local desc		= GetWidget('questsQuest'..prefix..index..'Description')
			local progress	= GetWidget('questsQuest'..prefix..index..'Progress')
			local refresh	= GetWidget('questsQuest'..prefix..index..'RefreshContainer')

			iconBody:SetVisible(false)
			name:SetVisible(false)
			desc:SetVisible(false)
			progress:SetVisible(false)
			refresh:SetVisible(false)

			iconBody:SetWidth('15%')
			iconBody:SetHeight('15%')
			iconBody:FadeIn(250)
			iconBody:Scale('110%', '110%', 250)

			name:SetY('-2h')
			name:FadeIn(250)
			name:SlideY(0, 250)


			desc:SetY('2h')
			desc:FadeIn(250)
			desc:SlideY(0, 250)

			desc:FadeIn(250)

			if progressMax > 1 then
				progress:FadeIn(250)
			end

			refresh:FadeIn(250)

			wait(250)
			iconBody:Scale('100%', '100%', 75)


			animateThread = nil
		end)

	end

	local resetThread = nil

	local function populate(newQuestID, achievementType, valueCurrent, valueGoal, questCode, heroEntity, productIconPath)

		setProgress(valueCurrent, valueGoal)

		local iconPath = ''

		if questCode == 'quest_unknown' then
			iconPath = '/ui/fe2/quests/quest_icons/quest_unknown.tga'
		elseif questCode == 'ability_coverup' then
			iconPath = '/ui/common/ability_coverup.tga'
		elseif questCode == 'hero_played' then
			if (heroEntity and string.len(heroEntity) > 0) then
				iconPath = GetEntityIconPath(heroEntity)
			else
				iconPath = '/ui/common/ability_coverup.tga'
			end
		else
			iconPath = '/ui/fe2/quests/quest_icons/'..questCode..'.tga'
		end

		local stringValue = valueGoal
		if achievementType == 'achievement_value_role_played' then
			stringValue = Translate('quests_'..questCode)
		elseif achievementType == 'achievement_value_attack_type_played' or achievementType == 'achievement_value_attribute_played' then
			stringValue = Translate('quests_attribute_'..questCode)
		elseif achievementType == 'achievement_value_hero_played' then
			if (heroEntity and string.len(heroEntity) > 0) then
				stringValue = GetEntityDisplayName(heroEntity)
			else
				stringValue = heroEntity or '---'
			end
		end

		GetWidget('questsQuest'..prefix..index..'Name'):SetText(Translate('quest_type_'..achievementType))
		GetWidget('questsQuest'..prefix..index..'Description'):SetText(Translate('quest_type_'..achievementType..'_body', 'value', stringValue))

		GetWidget('questsQuest'..prefix..index..'Icon'):SetTexture(iconPath)
		GetWidget('questsQuest'..prefix..index..'Icon'):SetUseAlphaMask((questCode == 'hero_played'))
	end

	local function update(doAnimation, newQuestID, questTrigger, valueCurrent, valueGoal, questCode, heroEntity, productIconPath, fromReset, newAddTimestamp)
		fromReset = fromReset or false
		questID = newQuestID

		doAnimation = doAnimation or false

		if doAnimation then
			if resetThread then
				resetThread:Kill()
				resetThread = nil
			end

			local args = { newQuestID, questTrigger, valueCurrent, valueGoal, questCode, heroEntity, productIconPath }

			resetThread = newthread(function()
				animateOut()
				PlaySound('/ui/fe2/quests/sounds/reroll.wav')
				if (not fromReset) then
					questsPopulateCompletePopup(index, animateOutTime)
				end
				wait(animateOutTime)
				populate(unpack(args))
				animateIn()
				resetThread = nil
			end)
		else
			populate(newQuestID, questTrigger, valueCurrent, valueGoal, questCode, heroEntity, productIconPath)
		end
	end

	local function getQuestID()
		return questID
	end

	return {
		setProgress		= setProgress,
		getIconPath		= getIconPath,
		animateIn		= animateIn,
		animateOut		= animateOut,
		update			= update,
		getQuestID		= getQuestID,
	}
end

local REWARD_TYPE_UNKNOWN	= 0
local REWARD_TYPE_PRODUCT	= 1
local REWARD_TYPE_SILVER	= 2
local REWARD_TYPE_GOLD		= 3

local rewardIconSilver		= '/ui/fe2/quests/textures/newui/silver_icon_large.tga'
local rewardIconGold		= '/ui/fe2/plinko/gold_icon_large.tga'
local rewardSpacing			= 0.55

local function questsRegisterReward(object, index, isAvailableInit, isMetInit, isOpenedInit)
	local container		= GetWidget('questsReward'..index)
	local button		= GetWidget('questsReward'..index..'Button')

	local isAvailableLast	= isAvailableInit or false
	local isMetLast			= isMetInit or false
	local isOpenedLast		= isOpenedInit or false

	local rewardType		= REWARD_TYPE_SILVER
	local productType		= ''
	local rewardOpen		= false
	local lastBagID			= -1

	local function updateIcon()
		bagID = lastBagID or -1
		local bagTexture = '/ui/fe2/quests/quest_icons/quest_unknown'

		if bagID ~= -1 then
			local suffixBig	= ''
			if index == 14 then
				bagTexture = '/ui/fe2/quests/textures/newui/reward_chest_big'
			elseif rewardType == REWARD_TYPE_PRODUCT then
				bagTexture = '/ui/fe2/quests/textures/newui/reward_chest'..suffixBig
			elseif rewardType == REWARD_TYPE_SILVER or rewardType == REWARD_TYPE_GOLD then
				bagTexture = '/ui/fe2/quests/textures/newui/reward_bag'..suffixBig
			else
				bagTexture = '/ui/fe2/quests/textures/newui/reward_bag'
				print('bad reward type for index '..index..' - '..rewardType..'\n')
			end
		end

		GetWidget('questsReward'..index..'Icon'):SetTexture(bagTexture..'.tga')
		GetWidget('questsReward'..index..'IconOpen'):SetTexture(bagID ~= -1 and bagTexture..'_open.tga' or '$invis')

		if lastUpdateFromLogin then
			GetWidget('questsReward'..index..'Icon'):SetVisible(not rewardOpen)
			GetWidget('questsReward'..index..'IconOpen'):SetVisible(rewardOpen)
		else
			if rewardOpen then
				GetWidget('questsReward'..index..'Icon'):FadeOut(250)
				GetWidget('questsReward'..index..'IconOpen'):FadeIn(250)
			else
				GetWidget('questsReward'..index..'Icon'):FadeIn(250)
				GetWidget('questsReward'..index..'IconOpen'):FadeOut(250)
			end
		end


	end

	local function setOpened(isOpened)
		if isOpened ~= nil then
			isOpenedLast = isOpened
		end
		updateIcon()
	end

	updateIcon()

	button:SetCallback('onclick', function(widget)
		if lastBagID >= 0 and isMetLast == true and isOpenedLast == false then
			questsOpenReward(lastBagID, index)
		end
	end)

	button:SetCallback('onmouselup', function(widget) questsRewardsScrollMouseDragEnd() end)
	button:SetCallback('onmouseldown', function(widget) questsRewardsScrollMouseDragStart() end)

	button:SetCallback('onmouseover', function(widget)

		local prefix = ''
		if index == 14 then prefix = 'big' end

		if isOpenedLast then
			
			local bagInfo = questsGetBagInfoByIndex(index)
			if bagInfo then
				if bagInfo.productID then
					Trigger('genericMainFloatingTip', 'true', '24.0h', (bagInfo.icon or ''), 'quests_bag_opened_product', 'quests_bag_opened_product_tip', bagInfo.name, bagInfo.description, '22', '-22')
				elseif bagInfo.gold then
					Trigger('genericMainFloatingTip', 'true', '24.0h', '/ui/fe2/store/gold_coins.tga', 'quests_bag_opened_gold', Translate('quests_bag_opened_gold_tip', 'amount', bagInfo.gold), '', '', '22', '-22')
				elseif bagInfo.silver then
					Trigger('genericMainFloatingTip', 'true', '24.0h', '/ui/fe2/store/silver_coins.tga', 'quests_bag_opened_silver', Translate('quests_bag_opened_silver_tip', 'amount', bagInfo.silver), '', '', '22', '-22')
				end
			end
 		elseif isMetLast then
			Trigger('genericMainFloatingTip', 'true', '24.0h', '', 'quests_reward'..prefix..'_unopened', 'quests_reward'..prefix..'_unopened_tip', '', '', '22', '-22')
		elseif isAvailableLast then
			Trigger('genericMainFloatingTip', 'true', '24.0h', '', 'quests_reward'..prefix..'_unmet', 'quests_reward'..prefix..'_unmet_tip', '', '', '22', '-22')
		elseif questsPlayerBagQueueFull() then
			Trigger('genericMainFloatingTip', 'true', '24.0h', '', 'quests_reward'..prefix..'_unavailable', 'quests_reward'..prefix..'_unavailable_queuefull_tip', '', '', '22', '-22')
		else
			Trigger('genericMainFloatingTip', 'true', '24.0h', '', 'quests_reward'..prefix..'_unavailable', 'quests_reward'..prefix..'_unavailable_tip', '', '', '22', '-22')
		end
	end)

	button:SetCallback('onmouseout', function(widget)
		Trigger('genericMainFloatingTip', 'false', '', '', '', '', '', '', '', '')
	end)

	button:RegisterWatch('questsBusy', function(widget, isBusy)
		button:SetEnabled(not AtoB(isBusy))
	end)


	local progressLast = 0
	local pieAnimTime = 1000

	local function setProgress(progressPercent, doAnimation)
		doAnimation = doAnimation or false
		progressPercent = progressPercent or 0
		local startProgress = progressLast

		if doAnimation and (progressPercent > progressLast) then
			local progressOffset = (progressPercent - startProgress)
			local progressPie = GetWidget('questsReward'..index..'ProgressPie')
			local endTime = GetTime() + pieAnimTime
			local startTime = GetTime()
			progressPie:UnregisterWatch('HostTime')
			progressPie:RegisterWatch('HostTime', function(widget, hostTime)
				local animPercent = math.min(((AtoN(hostTime) - startTime) / pieAnimTime), 1)
				local showProgress = math.min((startProgress + (progressOffset * animPercent)), 1)

				GetWidget('questsReward'..index..'ProgressPie'):SetValue(showProgress)
				GetWidget('questsReward'..index..'ProgressLabel'):SetText(math.max(0, round(showProgress * 100, 0)) .. '%')

				if AtoN(hostTime) >= endTime then
					progressPie:UnregisterWatch('HostTime')
				end
			end)
		else
			GetWidget('questsReward'..index..'ProgressPie'):SetValue(progressPercent)
			GetWidget('questsReward'..index..'ProgressLabel'):SetText(round(progressPercent * 100, 0) .. '%')

		end

		progressLast = progressPercent

	end

	local function setPosition(newPosition, positionX)
		local oneH		= GetScreenHeight() * 0.01
		local newPos	= 0

		if positionX and type(positionX) == 'number' then
			newPos = positionX * oneH
		else
			newPos = (index - 1) * ((oneH * rewardSpacing) + container:GetWidth())
		end

		container:SetX((oneH * 3.35) + newPos)
		container:SetY(newPosition * oneH)

		return newPos + container:GetWidth() + (oneH * 6)
	end

	local function getPosition()
		return container:GetX() - (GetScreenHeight() * 0.05)
	end

	--[[

	local glow	= GetWidget('questsReward'..index..'Glow')

	local glowOn	= false

	glow:SetCallback('onshow', function(widget)
		GetWidget('questsReward'..index..'GlowBody'):SetVisible(false)
		GetWidget('questsReward'..index..'GlowBody'):FadeIn(250)
		widget:UnregisterWatch('HostTime')
		widget:RegisterWatch('HostTime', function(widget, hostTime)
			if (AtoN(hostTime) % 2000) < 1000 then
				if not glowOn then
					GetWidget('questsReward'..index..'GlowBody'):Scale('115@', '115%', 1000)
					glowOn = true
				end
			else
				if glowOn then
					GetWidget('questsReward'..index..'GlowBody'):Scale('100@', '100%', 1000)
					glowOn = false
				end
			end
		end)
	end)

	glow:SetCallback('onhide', function(widget)
		widget:UnregisterWatch('HostTime')
	end)

	--]]

	local function setStatus(isAvailable, isMet, isOpened, isCurrent, newBagID, newRewardType)

		if isCurrent then
			local bagInfo = questsGetBagInfoByIndex(index)
			if bagInfo then
				local progressCurrent	= bagInfo.progressCurrent or 0
				local progressGoal		= bagInfo.progressGoal or 1
				setProgress(progressCurrent / progressGoal, (isCurrent and (not lastUpdateFromLogin)))
			end
		else
			GetWidget('questsReward'..index..'ProgressPie'):SetVisible(false)
		end

		if newRewardType ~= nil then
			rewardType = newRewardType
		end

		isAvailable	= isAvailable or false
		isMet		= isMet or false
		isOpened	= isOpened or false
		isCurrent	= isCurrent or false

		if isAvailable then
			-- GetWidget('questsReward'..index..'Icon'):SetRenderMode('normal')
			GetWidget('questsReward'..index..'Unavailable'):SetVisible(false)

			if isCurrent then
				if lastUpdateFromLogin then
					GetWidget('questsReward'..index..'Current'):SetVisible(true)
				else
					GetWidget('questsReward'..index..'Current'):FadeIn(250)
				end
			else
				if lastUpdateFromLogin then
					GetWidget('questsReward'..index..'Current'):SetVisible(false)
				else
					GetWidget('questsReward'..index..'Current'):FadeOut(250)
				end
			end

			rewardOpen = isOpened

			local body = GetWidget('questsReward'..index..'Body')
			if isMet and not isOpened then
				-- body:SetWidth('115@')
				-- body:SetHeight('115%')
				if lastUpdateFromLogin then
					GetWidget('questsReward'..index..'Glow'):SetVisible(true)
				else
					GetWidget('questsReward'..index..'Glow'):FadeIn(250)
				end
			else
				-- body:SetWidth('100@')
				-- body:SetHeight('100%')
				if lastUpdateFromLogin then
					GetWidget('questsReward'..index..'Glow'):SetVisible(false)
				else
					GetWidget('questsReward'..index..'Glow'):FadeOut(250)
				end
			end

		else
			-- GetWidget('questsReward'..index..'Icon'):SetRenderMode('grayscale')
			GetWidget('questsReward'..index..'Current'):SetVisible(false)
			GetWidget('questsReward'..index..'Glow'):SetVisible(false)
			GetWidget('questsReward'..index..'Unavailable'):SetVisible(true)
		end

		if newBagID ~= nil then
			lastBagID = newBagID
		end
		
		updateIcon()

		isAvailableLast		= isAvailable
		isMetLast			= isMet
		isOpenedLast		= isOpened
		
	end

	return {
		setStatus	= setStatus,
		setPosition	= setPosition,
		getPosition	= getPosition,
		setOpened	= setOpened,
		setProgress	= setProgress
	}
end

-- =================================

local rewardContainers	= {
	goldChest			= {
		model				= "/ui/fe2/quests/models/gold_chest/model.mdf",
		effect				= "/ui/fe2/quests/models/gold_chest/body.effect",
		cameraPos			= "10 -1400 -190",
		timeBeforeOpen		= 2900,			-- Wait for the prize to appear
		timeBeforeReveal	= 150,			-- X ms into the open animation, show prize
		rewardSoundDelay	= 1275,
		rewardSound			= "/ui/fe2/plinko/sounds/tier_1_chest_open.wav",

		clipPanelY = "11h",
	},
	silverChest			= {
		model				= "/ui/fe2/quests/models/silver_chest/model.mdf",
		effect				= "/ui/fe2/quests/models/silver_chest/body.effect",
		cameraPos			= "0 -1050 -150",
		timeBeforeOpen		= 2500,
		timeBeforeReveal	= 150,
		rewardSoundDelay	= 1275,
		rewardSound			= "/ui/fe2/plinko/sounds/tier_1_chest_open.wav",
		
		clipPanelY = "11h",
	},
	pouch			= {
		model				= "/ui/fe2/quests/models/silver_pouch/model.mdf",
		effect				= "/ui/fe2/quests/models/silver_pouch/body.effect",
		cameraPos			= "0 -1050 -25",
		timeBeforeOpen		= 250,
		timeBeforeReveal	= 500,
		rewardSoundDelay	= 1275,
		rewardSound			= "/ui/fe2/plinko/sounds/tier_1_chest_open.wav",

		clipPanelY = "17h",
	},
	pouchGold		= {
		model				= "/ui/fe2/quests/models/gold_pouch/model.mdf",
		effect				= "/ui/fe2/quests/models/gold_pouch/body.effect",
		cameraPos			= "0 -1050 -25",
		timeBeforeOpen		= 250,
		timeBeforeReveal	= 500,
		rewardSoundDelay	= 1275,
		rewardSound			= "/ui/fe2/plinko/sounds/tier_1_chest_open.wav",

		clipPanelY = "17h",
	},

}


local REWARD_DISPLAY_TYPE_MODEL			= 0
local REWARD_DISPLAY_TYPE_ICON			= 1
local REWARD_DISPLAY_TYPE_TICKET		= 2
local REWARD_DISPLAY_TYPE_SILVER		= 3
local REWARD_DISPLAY_TYPE_GOLD			= 4

-- =================================

local function questsRegister(object)
	local container		= GetWidget('quests')
	local quests		= {}
	local questsCount	= 2
	local rewards		= {}
	local rewardsCount	= 14
	local lastOpenedBagIndex = -1

	local lastResetsAvailable = 1

	function questsGetLastResetsAvailable()
		return lastResetsAvailable
	end

	for i=1,questsCount,1 do
		table.insert(quests, questsRegisterQuest(object, i))
	end

	for i=1,rewardsCount,1 do
		table.insert(rewards, questsRegisterReward(object, i))
	end

	function questsGetQuest(questID)
		return quests[questID]
	end

	function questsGetReward(rewardID)
		return rewards[rewardID]
	end

	function questsSetRewardOpenable(rewardID)
		local reward = questsGetReward(rewardID)
		if reward then
			reward.setStatus(true, true, false, false, nil, nil)
		end
	end

	function questsSetAllRewardsRandom()
		local reward = nil

		local isAvailable
		local isMet
		local isOpened
		local isCurrent

		for i=1,14,1 do
			reward = questsGetReward(i)
			if reward then
				isAvailable		= false
				isMet			= false
				isOpened		= false
				isCurrent		= false
				newBagID		= false
				newRewardType	= false

				if math.random(0,1) == 1 then isAvailable = true end
				if math.random(0,1) == 1 and isAvailable then isMet = true end
				if math.random(0,1) == 1 and isMet then isOpened = true end
				if math.random(0,1) == 1 and isMet and (not isOpened) then isCurrent = true end

				reward.setStatus(isAvailable, isMet, isOpened, isCurrent, nil, nil)
				if isCurrent then
					reward.setProgress((math.random(1, 99) / 100), false)
				end
				
			end
		end
	end
	GetWidget('questsBusyCover'):RegisterWatch('questsBusy', function(widget, isBusy)
		widget:SetVisible(AtoB(isBusy))
	end)

	function questsSetRewardCurrent(rewardID)
		local reward = questsGetReward(rewardID)
		if reward then
			reward.setStatus(true, false, false, true, nil, nil)
			reward.setProgress((math.random(1, 99) / 100), false)
		end
	end

	local rewardPositions = {
		-3,		-- 1
		4.25,	-- 2
		-4,		-- 3
		3,		-- 4
		-2,		-- 5
		-7,		-- 6
		2.5,	-- 7
		-6,		-- 8
		-2,		-- 9
		3,		-- 10
		-3,		-- 11
		1,		-- 12
		-5,		-- 13
		0,		-- 14
	}

	local lastRewardPos = 0

	local rewardsScrollArea		= GetWidget('questsRewardsScrollArea')
	local rewardsScrollBody		= GetWidget('questsRewardsScrollBody')
	local rewardsScrollBodyBack	= GetWidget('questsRewardsScrollBodyBack')
	local rewardsScrollPanel	= GetWidget('questsRewardsScrollPanel')

	local rewardsScrollAmount		= GetScreenHeight() * 0.01 * 6
	local rewardsScrollAmountHeld	= GetScreenHeight() * 0.01 * 1
	local rewardsScrollMax			= rewardsScrollBody:GetWidth() - rewardsScrollArea:GetWidth()
	local rewardsScrollPos			= 0
	local rewardsScrollTargPos		= 0
	local rewardsScrollTime			= 250
	local rewardsScrollActive		= false

	local rewardsScrollDownPos	= 0

	local function repositionRewards()
		local newBodySize = 0
		for i=1,rewardsCount,1 do
			newBodySize = rewards[i].setPosition(rewardPositions[i])
		end
		
		rewardsScrollBody:SetWidth(newBodySize)
		rewardsScrollMax = rewardsScrollBody:GetWidth() - rewardsScrollArea:GetWidth()
		rewardsScrollBodyBack:SetWidth(rewardsScrollBody:GetWidth())
	end

	local nextEnabled = true
	local prevEnabled = true

	function rewardsUpdateScrollButtons()		-- can't disable due to needing proper onmouselup
		-- local nextTexture	= "/ui/fe2/quests/textures/newui/scroll_button_up.tga"
		-- local prevTexture	= "/ui/fe2/quests/textures/newui/scroll_button_up.tga"

		nextEnabled = true
		prevEnabled = true

		if rewardsScrollTargPos <= 0 then
			-- prevTexture = "/ui/fe2/quests/textures/newui/scroll_button_disabled.tga"
			-- GetWidget('questsRewardsButtonScrollPrev'):SetEnabled(false)
			prevEnabled = false
		else
			-- GetWidget('questsRewardsButtonScrollPrev'):SetEnabled(true)
		end

		if rewardsScrollTargPos >= rewardsScrollMax then
			-- nextTexture = "/ui/fe2/quests/textures/newui/scroll_button_disabled.tga"
			nextEnabled = false
			-- GetWidget('questsRewardsButtonScrollNext'):SetEnabled(false)
		else
			-- GetWidget('questsRewardsButtonScrollNext'):SetEnabled(true)
		end

		--[[
		for k,v in ipairs(object:GetGroup('questsRewardsButtonScrollNextImages')) do
			v:SetTexture(nextTexture)
		end

		for k,v in ipairs(object:GetGroup('questsRewardsButtonScrollPrevImages')) do
			v:SetTexture(prevTexture)
		end
		--]]


		local textureDisabled		= '/ui/fe2/quests/textures/newui/scroll_button_disabled.tga'
		local textureDown			= '/ui/fe2/quests/textures/newui/scroll_button_down.tga'
		local textureUp				= '/ui/fe2/quests/textures/newui/scroll_button_up.tga'

		if nextEnabled then
			GetWidget('questsRewardsButtonScrollNextImageup'):SetTexture(textureUp)
			GetWidget('questsRewardsButtonScrollNextImageover'):SetTexture(textureUp)
			GetWidget('questsRewardsButtonScrollNextImagedown'):SetTexture(textureDown)

			GetWidget('questsRewardsButtonScrollNextImageup'):SetColor('#DDDDDD')

			GetWidget('questsRewardsButtonScrollNextBodydown'):SetX(1)
			GetWidget('questsRewardsButtonScrollNextBodydown'):SetY(1)

		else
			GetWidget('questsRewardsButtonScrollNextImageup'):SetTexture(textureDisabled)
			GetWidget('questsRewardsButtonScrollNextImageover'):SetTexture(textureDisabled)
			GetWidget('questsRewardsButtonScrollNextImagedown'):SetTexture(textureDisabled)

			GetWidget('questsRewardsButtonScrollNextImageup'):SetColor(1,1,1)

			GetWidget('questsRewardsButtonScrollNextBodydown'):SetX(0)
			GetWidget('questsRewardsButtonScrollNextBodydown'):SetY(0)
		end

		if prevEnabled then
			GetWidget('questsRewardsButtonScrollPrevImageup'):SetTexture(textureUp)
			GetWidget('questsRewardsButtonScrollPrevImageover'):SetTexture(textureUp)
			GetWidget('questsRewardsButtonScrollPrevImagedown'):SetTexture(textureDown)

			GetWidget('questsRewardsButtonScrollPrevImageup'):SetColor('#DDDDDD')

			GetWidget('questsRewardsButtonScrollPrevBodydown'):SetX(1)
			GetWidget('questsRewardsButtonScrollPrevBodydown'):SetY(1)
		else
			GetWidget('questsRewardsButtonScrollPrevImageup'):SetTexture(textureDisabled)
			GetWidget('questsRewardsButtonScrollPrevImageover'):SetTexture(textureDisabled)
			GetWidget('questsRewardsButtonScrollPrevImagedown'):SetTexture(textureDisabled)

			GetWidget('questsRewardsButtonScrollPrevImageup'):SetColor(1,1,1)

			GetWidget('questsRewardsButtonScrollPrevBodydown'):SetX(0)
			GetWidget('questsRewardsButtonScrollPrevBodydown'):SetY(0)
		end

		-- GetWidget('questsRewardsButtonScrollNextImageup'):SetTexture(nextTexture)
		-- GetWidget('questsRewardsButtonScrollPrevImageup'):SetTexture(prevTexture)

	end


	local scrollGearTL		= GetWidget('questsMapGearTL')
	local scrollGearTR		= GetWidget('questsMapGearTR')
	local scrollGearBL		= GetWidget('questsMapGearBL')
	local scrollGearBR		= GetWidget('questsMapGearBR')
	local scrollGearSpeed	= 100

	local function rewardsScrollSetPos(newPos)
		rewardsScrollPos = newPos

		rewardsScrollBody:SetX(rewardsScrollPos * -1)
		rewardsScrollBodyBack:SetX(rewardsScrollPos * 0.9)

		local rotationTop	= ((rewardsScrollPos % scrollGearSpeed) / scrollGearSpeed) * -360
		local rotationBot	= ((rewardsScrollPos + 5 % scrollGearSpeed) / scrollGearSpeed) * 360

		scrollGearTL:SetRotation(rotationTop)
		scrollGearTR:SetRotation(rotationTop)
		scrollGearBL:SetRotation(rotationBot)
		scrollGearBR:SetRotation(rotationBot)
	end

	function questsRewardsScrollMouseDragEnd()
		rewardsScrollArea:UnregisterWatch('HostTime')
	end

	local rewardsDragHandle = GetWidget('questsRewardsDragHandle')

	function questsRewardsScrollMouseDragStart()
		questsRewardsScrollDragEnd()
		questsRewardsScrollMouseDragEnd()
		local mouseInitialX	= Input.GetCursorPosX()
		local initialPos	= rewardsScrollPos

		rewardsScrollArea:RegisterWatch('HostTime', function(widget, currentTime)

			local cursorX	= Input.GetCursorPosX()
			local cursorY	= Input.GetCursorPosY()
			local handleX	= rewardsDragHandle:GetAbsoluteX()
			local handleY	= rewardsDragHandle:GetAbsoluteY()

			local newPos = initialPos + (mouseInitialX - cursorX)

			newPos = math.max(0,  math.min( newPos, rewardsScrollMax ) )

			rewardsScrollTargPos = newPos
			rewardsScrollSetPos(newPos)
			rewardsUpdateScrollButtons()


			if (
				cursorX < handleX or
				cursorY < handleY or
				cursorX > (handleX + rewardsDragHandle:GetWidth()) or
				cursorY > (handleY + rewardsDragHandle:GetHeight())
			) then
				questsRewardsScrollMouseDragEnd()
			end

		end)
	end

	rewardsDragHandle:SetCallback('onmouselup', questsRewardsScrollMouseDragEnd)
	rewardsDragHandle:SetCallback('onmouseldown', questsRewardsScrollMouseDragStart)
	rewardsDragHandle:SetCallback('onhide', questsRewardsScrollMouseDragEnd)

	function questsRewardsScrollDragEnd()
		rewardsScrollArea:UnregisterWatch('HostTime')
		rewardsScrollActive = false
		interface:UICmd("StopSound(10)")
	end

	local beltLoopSoundLast = 0
	local beltLoopSoundThrottle = 2421

	local function rewardsScrollDragStart()
		rewardsUpdateScrollButtons()
		if rewardsScrollPos ~= rewardsScrollTargPos then
			if not rewardsScrollActive then
				questsRewardsScrollDragEnd()
				rewardsScrollActive = true
				PlaySound('/ui/fe2/quests/sounds/belt_quick_loop.wav', 1, 10)
				beltLoopSoundLast = GetTime() + 805 - beltLoopSoundThrottle
				rewardsScrollArea:RegisterWatch('HostTime', function(widget, currentTime)
					local hostTime = AtoN(currentTime)

					if math.floor(rewardsScrollPos) == math.floor(rewardsScrollTargPos) then
						questsRewardsScrollDragEnd()
						rewardsScrollSetPos(rewardsScrollTargPos)
					else
						local maxDistance		= rewardsScrollTargPos - rewardsScrollPos
						local actualDistance	= maxDistance * (0.025 * (GetFrameTime() / 16))

						if hostTime > (beltLoopSoundLast + beltLoopSoundThrottle) then
							PlaySound('/ui/fe2/quests/sounds/belt_quick_loop.wav', 1, 10)

							beltLoopSoundLast = hostTime
						end

						if math.abs(actualDistance) < 1 then


							if actualDistance > 0 then
								actualDistance = math.ceil(actualDistance * (GetFrameTime() / 16))
							else
								actualDistance = math.floor(actualDistance * (GetFrameTime() / 16))
							end
						end
						rewardsScrollSetPos(rewardsScrollPos + actualDistance)

					end
				end)
			end
		end
	end

	local function rewardsScrollUpdatePos()
		rewardsScrollBody:SlideX(rewardsScrollPos * -1, rewardsScrollTime)
	end

	local function rewardsScrollNext()
		rewardsScrollTargPos = math.max((rewardsScrollTargPos - rewardsScrollAmount), 0)
		rewardsScrollDragStart()
	end

	local function rewardsScrollPrev()
		rewardsScrollTargPos = math.min((rewardsScrollTargPos + rewardsScrollAmount), rewardsScrollMax)
		rewardsScrollDragStart()
	end

	rewardsScrollPanel:SetCallback('onmousewheeldown', function(widget)
		rewardsScrollPrev()
	end)

	rewardsScrollPanel:SetCallback('onmousewheelup', function(widget)
		rewardsScrollNext()
	end)

	function questsScrollToPosition(index, doAnimation)
		if doAnimation == nil then doAnimation = true end

		if index and type(index) == 'number' and index >= 1 and index <= rewardsCount then
			rewardsScrollTargPos = math.min(
				math.max(
					rewards[index].getPosition(), 0
				), rewardsScrollMax
			)
			if doAnimation then
				rewardsScrollDragStart()
			else
				rewardsScrollSetPos(rewardsScrollTargPos)
			end
		end
	end

	GetWidget('questsRewardsButtonScrollPrev'):SetCallback('onmouseldown', function(widget)

		if prevEnabled then
			PlaySound('/ui/fe2/quests/sounds/belt_button_%.wav')
		end

		
		rewardsScrollTargPos = rewardsScrollPos
		widget:UnregisterWatch('HostTime')

		local widgetX		= widget:GetAbsoluteX()
		local widgetY		= widget:GetAbsoluteY()
		local widgetWidth	= widget:GetWidth()
		local widgetHeight	= widget:GetHeight()

		widget:RegisterWatch('HostTime', function(widget, currentTime)
			rewardsScrollTargPos = math.max((rewardsScrollTargPos - (rewardsScrollAmountHeld * (GetFrameTime() / 16))), 0)
			rewardsScrollDragStart()

			local cursorX	= Input.GetCursorPosX()
			local cursorY	= Input.GetCursorPosY()

			if (
				cursorX < widgetX or
				cursorY < widgetY or
				cursorX > (widgetX + widgetWidth) or
				cursorY > (widgetY + widgetHeight)
			) then
				widget:UnregisterWatch('HostTime')
			end
		end)
	end)

	GetWidget('questsRewardsButtonScrollPrev'):SetCallback('onmouselup', function(widget)
		widget:UnregisterWatch('HostTime')
		questsRewardsScrollDragEnd()
	end)

	GetWidget('questsRewardsButtonScrollNext'):SetCallback('onmouseldown', function(widget)
		if nextEnabled then
			PlaySound('/ui/fe2/quests/sounds/belt_button_%.wav')
		end
		
		rewardsScrollTargPos = rewardsScrollPos
		widget:UnregisterWatch('HostTime')

		local widgetX		= widget:GetAbsoluteX()
		local widgetY		= widget:GetAbsoluteY()
		local widgetWidth	= widget:GetWidth()
		local widgetHeight	= widget:GetHeight()

		widget:RegisterWatch('HostTime', function(widget, currentTime)
			rewardsScrollTargPos = math.min((rewardsScrollTargPos + (rewardsScrollAmountHeld * (GetFrameTime() / 16))), rewardsScrollMax)
			rewardsScrollDragStart()

			local cursorX	= Input.GetCursorPosX()
			local cursorY	= Input.GetCursorPosY()

			if (
				cursorX < widgetX or
				cursorY < widgetY or
				cursorX > (widgetX + widgetWidth) or
				cursorY > (widgetY + widgetHeight)
			) then
				widget:UnregisterWatch('HostTime')
			end

		end)
	end)

	GetWidget('questsRewardsButtonScrollNext'):SetCallback('onmouselup', function(widget)
		widget:UnregisterWatch('HostTime')
		questsRewardsScrollDragEnd()
	end)

	local rewardCurrent = nil

	repositionRewards()

	local lastStatusOpenReward			= false
	local lastStatusGetCurrentStatus	= false
	local lastStatusResetQuest			= false
	local lastStatusUpdateQuest			= false
	local lastGamePhase					= 0

	local function processQuestStatus()
		local isBusy	= (
			lastStatusOpenReward or
			lastStatusGetCurrentStatus or
			lastStatusResetQuest or
			lastGamePhase > 1
		)

		GetWidget('questsRewardsButtonScrollPrev'):SetEnabled(not isBusy)
		GetWidget('questsRewardsButtonScrollNext'):SetEnabled(not isBusy)

		rewardsScrollPanel:SetVisible(not isBusy)

		local busyUITrigger = UITrigger.GetTrigger('questsBusy')
		if busyUITrigger then
			busyUITrigger:Trigger(tostring(isBusy))
		end
	end

	container:RegisterWatch('GamePhase', function(widget, newPhase)
		local gamePhase = AtoN(newPhase)
		if lastGamePhase == 0 and gamePhase > 0 then
			UIManager.GetInterface('main'):HoNMainF('SelectChat')
		end
		lastGamePhase = gamePhase
		processQuestStatus()
	end)

	container:RegisterWatch('questsOpenRewardStatus', function(widget, status)
		local requestStatus = AtoN(status)
		lastStatusOpenReward = (requestStatus == 1)

		if requestStatus == 3 then
			questsErrorMessage(tostring(Translate('quest_error_open_reward_request_fail')))
		end

		processQuestStatus()
	end)

	--[[
	container:RegisterWatch('questsGetCurrentStatus', function(widget, status)
		lastStatusGetCurrentStatus = (AtoN(status) == 1)
		processQuestStatus()
	end)
	--]]

	local questResetQueued = -1

	function questsResetQuest(questID, slotIndex)
		questResetQueued = slotIndex
		ResetQuest(questID)
	end

	local codeByQuestTrigger = {
		-- achievement_value_building_damage		= '',
		-- achievement_value_kongors_slain			= '',
		achievement_value_games_won					= 'games_won',
		achievement_value_rift_wars_won				= 'rift_wars_won',
		achievement_value_mid_wars_won				= 'mid_wars_won',
		achievement_value_caldavar_won				= 'caldavar_won',
		achievement_value_grimms_won				= 'grimms_won',
		achievement_value_prophet_kills				= 'prophet_kills',
		achievement_value_rapid_fire_won			= 'rapid_fire_won',
		achievement_value_grimm_hunt_won			= 'grimm_hunt_won',
		achievement_value_ctf_won					= 'capture_the_flag_won',
		achievement_value_duplicate_won				= 'duplicate_won',
		achievement_value_devo_wars_won				= 'devo_wars_won',
		achievement_value_random_won				= 'random_won',
		-- achievement_value_hero_played			= '',
		-- achievement_value_role_played			= '',
		-- achievement_value_attribute_played		= '',
		-- achievement_value_attack_type_played		= '',
	}

	local roleList = {	-- This order must remain the same
		'solo',
		'jungle',
		'carry',
		'support',
		'initiator',
		'ganker',
		'pusher'
	}

	local function getUIQuestCode(questTrigger, roleSolo, roleJungle, roleCarry, roleSupport, roleInitiator, roleGanker, rolePusher, isRanged, isLegion, attribute)
		if questTrigger == 'achievement_value_hero_played' then
			return 'hero_played'
		elseif questTrigger == 'achievement_value_role_played' then
			local roleCount = 0
			local roles		= {
				roleSolo,
				roleJungle,
				roleCarry,
				roleSupport,
				roleInitiator,
				roleGanker,
				rolePusher
			}

			local rolesValid = {}
			for i=1,#roleList,1 do
				if roles[i] then
					table.insert(rolesValid, roleList[i])
					roleCount = roleCount + 1
				end

				if roleCount >= 2 then
					break
				end
			end

			if roleCount == 1 then
				return 'role_'..rolesValid[i]
			elseif roleCount == 2 then
				return 'role_' .. rolesValid[1] .. '_' .. rolesValid[2]
			else
				return 'quest_unknown'
			end
		elseif questTrigger == 'achievement_value_attribute_played' then
			local prefix = 'hellbourne'
			if isLegion then
				prefix = 'legion'
			end

			return prefix .. '_' .. attribute
		elseif questTrigger == 'achievement_value_attack_type_played' then
			local prefix = 'hellbourne'
			if isLegion then
				prefix = 'legion'
			end

			local attackType = 'melee'
			if isRanged then
				attackType = 'ranged'
			end

			return prefix .. '_' .. attackType

		elseif codeByQuestTrigger[questTrigger] then
			return codeByQuestTrigger[questTrigger]
		end

		return 'quest_unknown'
	end

	local questResetCountLeft = 1

	local questsUpdateCache			= {}		-- by Trigger
	local playerQuestQueueIDNew		= {}		-- by Trigger
	local playerQuestQueueIDLast	= {}		-- by Slot
	local playerQuestTriggerIndex	= {}		-- Trigger per slot
	local playerQuestSlotByTrigger	= {}		-- Slot per slot

	local function populateQuestSlot(questInfo, slotIndex)
		attributes = {}
		attributes[0] = 'strength'
		attributes[1] = 'agility'
		attributes[2] = 'intelligence'
		local heroAttribute = attributes[questInfo.attributeCode] or ''

		local questEntry = quests[slotIndex]
		local fromReset = (questResetQueued == slotIndex)
		local animateReset = ((fromReset or (not lastUpdateFromLogin)) and questInfo.questID ~= questEntry.getQuestID())

		if fromReset then
			questResetQueued = -1
		end

		local heroEntity = ''
		if questInfo.heroProduct and string.len(questInfo.heroProduct) > 0 then
			heroEntity	= string.sub(questInfo.heroProduct, 0, -6)
		end

		if questInfo.productIconPath and string.len(questInfo.productIconPath) > 0 then
			if (not GetCvarBool("_entityDefinitionsLoaded")) then
				local heroPath		= string.sub(questInfo.productIconPath, 1, string.find(questInfo.productIconPath, "/", 9))
				interface:UICmd("LoadEntityDefinitionsFromFolder('"..heroPath.."')")
			end
		end

		local roleCheck	= {
			solo		= 'roleSolo',
			jungle		= 'roleJungle',
			carry		= 'roleCarry',
			support		= 'roleSupport',
			initiator	= 'roleInitiator',
			ganker		= 'roleGanker',
			pusher		= 'rolePusher'
		}

		rolesValid		= {}
		lastQuestIDs	= {}

		local questHeroRoles = Explode(',', questInfo.heroRoleString)

		for k,v in ipairs(questHeroRoles) do
			if v and roleCheck[v] then
				rolesValid[roleCheck[v]] = true
			end
		end

		for k,v in ipairs({
			'roleSolo',
			'roleJungle',
			'roleCarry',
			'roleSupport',
			'roleInitiator',
			'roleGanker',
			'rolePusher'
			}) do
			rolesValid[v] = rolesValid[v] or false
		end

		local questCode = getUIQuestCode(questInfo.questTrigger, rolesValid.roleSolo, rolesValid.roleJungle, rolesValid.roleCarry, rolesValid.roleSupport, rolesValid.roleInitiator, rolesValid.roleGanker, rolesValid.rolePusher, (questInfo.attackType ~= 'melee'), ((questInfo.teamID or 1) == 1), heroAttribute)

		questEntry.update(animateReset, questInfo.questID, questInfo.questTrigger, questInfo.valueCurrent, questInfo.valueGoal, questCode, heroEntity, questInfo.productIconPath, fromReset, questInfo.addTimestamp)
	end

	local function populateSlotsFromQuestInfo()	-- This accounts for quests moving as quests are reset, etc.
		local displaySlotUpdated		= {}		-- by Slot
		local questTriggerAssigned		= {}		-- by Trigger
		for i=1,2,1 do
			displaySlotUpdated[i]	= false
			questTriggerAssigned[i]	= false
		end

		for i=1,2,1 do		-- per trigger
			for j=1,2,1 do	-- per slot
				if (not questTriggerAssigned[i]) and playerQuestQueueIDNew[i] == playerQuestQueueIDLast[j] and not displaySlotUpdated[j] then
					displaySlotUpdated[j] = true
					questTriggerAssigned[i] = true
					playerQuestTriggerIndex[j] = i
					playerQuestSlotByTrigger[i] = j
				end
			end
		end

		for i=1,2,1 do
			for j=1,2,1 do
				if (not questTriggerAssigned[i]) and not displaySlotUpdated[j] then
					displaySlotUpdated[j] = true
					questTriggerAssigned[i] = true
					playerQuestTriggerIndex[j] = i
					playerQuestSlotByTrigger[i] = j
				end
			end
		end

		for i=1,2,1 do
			playerQuestQueueIDLast[i] = playerQuestQueueIDNew[playerQuestTriggerIndex[i]]
		end
	end


	if HoN_Region.regionTable[HoN_Region.activeRegion].questSystem then
		container:RegisterWatch('PlayerQuestIDs', function(widget, id1, id2)
			if id1 == nil then id1 = '-1' end
			if id2 == nil then id2 = '-1' end

			playerQuestQueueIDNew[1] = AtoN(id1)
			playerQuestQueueIDNew[2] = AtoN(id2)

			populateSlotsFromQuestInfo()	-- actually only the ID assignment
		end)
	end

	if HoN_Region.regionTable[HoN_Region.activeRegion].questSystem then

		for i=0,1,1 do
			container:RegisterWatch('QuestsUpdate'..i, function(widget, questID, questName, questType, questTrigger, valueCurrent, valueGoal, bagValue, heroProduct, heroRoleString, attributeCode, attackType, teamID, productIconPath)
				questsUpdateCache[(i + 1)] = {
					questID			= AtoN(questID),
					questName		= questName,
					questType		= questType,
					questTrigger	= questTrigger,
					valueCurrent	= AtoN(valueCurrent),
					valueGoal		= AtoN(valueGoal),
					bagValue		= bagValue,
					heroProduct		= heroProduct,
					heroRoleString	= heroRoleString,
					attributeCode	= AtoN(attributeCode),
					attackType		= attackType,
					teamID			= AtoN(teamID),
					productIconPath	= productIconPath
				}

				if playerQuestSlotByTrigger[i + 1] then
					populateQuestSlot(questsUpdateCache[i + 1], playerQuestSlotByTrigger[i + 1])
				else
					print('could not find display slot for QuestsUpdate'..i..'\n')
				end
			end)
		end

	end

	GetWidget('questsResetPromptCancel'):SetCallback('onclick', function(widget)
		GetWidget('questsResetPrompt'):FadeOut(250)
	end)

	local function getQuestWidgets(index, prefix)
		prefix = prefix or ''
		return {
			icon				= GetWidget('questsQuest'..prefix..index..'Icon'),
			name				= GetWidget('questsQuest'..prefix..index..'Name'),
			description			= GetWidget('questsQuest'..prefix..index..'Description'),
			progress			= GetWidget('questsQuest'..prefix..index..'ProgressLabel'),
			progressContainer	= GetWidget('questsQuest'..prefix..index..'Progress')
		}
	end

	function questCopyWidgetProperties(questFrom, questTo)
		questTo.icon:SetTexture(questFrom.icon:GetTexture())
		questTo.icon:SetUseAlphaMask(questFrom.icon:UsingAlphaMask())

		questTo.name:SetText(questFrom.name:GetText())
		questTo.description:SetText(questFrom.description:GetText())
		questTo.progress:SetText(questFrom.progress:GetText())

		questTo.progressContainer:SetVisible(questFrom.progressContainer:IsVisibleSelf())
	end

	GetWidget('questsCompletedPopup1OK'):SetCallback('onclick', function(widget)
		GetWidget('questsCompletedPopup1'):FadeOut(250)
	end)

	GetWidget('questsCompletedPopup2OK'):SetCallback('onclick', function(widget)
		GetWidget('questsCompletedPopup2'):FadeOut(250)
	end)

	function questsPopulateCompletePopup(completedIndex, waitTime)
		local questFrom	= getQuestWidgets(completedIndex)
		local questTo	= getQuestWidgets(completedIndex, 'Completed')

		questCopyWidgetProperties(questFrom, questTo)

		GetWidget('questsCompletedPopup'..completedIndex):Sleep(waitTime, function(widget)
			widget:FadeIn(250)

			interface:UICmd("StopSound(12)")
			PlaySound('/ui/fe2/quests/sounds/quest_complete_jingle_'..math.random(1,3)..'.wav', 1, 12)
		end)
	end

	function questsPopulateResetPrompt(toResetIndex, questID)
		local questFrom	= getQuestWidgets(toResetIndex)
		local questTo	= getQuestWidgets(1, 'Reset')

		questCopyWidgetProperties(questFrom, questTo)

		GetWidget('questsResetPromptOK'):SetCallback('onclick', function(widget)
			GetWidget('questsResetPrompt'):FadeOut(250)
			if HoN_Region.regionTable[HoN_Region.activeRegion].questSystem then
				questsResetQuest(questID, toResetIndex)	
			end
			
		end)

		GetWidget('questsResetPrompt'):FadeIn(250)
	end

	GetWidget('questsErrorMessageOK'):SetCallback('onclick', function(widget)
		widget:GetWidget('questsErrorMessage'):FadeOut(250)
	end)

	function questsErrorMessage(errorString)
		if errorString == nil or string.len(errorString) <= 0 then
			errorString = Translate('general_error')
		end

		GetWidget('questsErrorMessageBody'):SetText(errorString)
		GetWidget('questsErrorMessage'):FadeIn(250)
	end

	local bagInfoByID	= {}

	local function initializeBagByID(bagID, bagType, debugSource, progress, goal)
		if not bagID then
			if debugSource and string.len(debugSource) > 0 then
				print('bad bag source is '..debugSource..'\n')
			end
		end
		bagInfoByID[bagID] = bagInfoByID[bagID] or {}
		bagInfoByID[bagID].bagID = bagID
		bagInfoByID[bagID].bagType = bagType
		bagInfoByID[bagID].progressCurrent = progress or 0
		bagInfoByID[bagID].progressGoal = goal or 0
	end

	function questsGetBagInfoByID(bagID)
		return bagInfoByID[bagID] or false
	end

	local BAG_TYPE_QUEUE			= 0
	local BAG_TYPE_PLAYER_QUEUE		= 1
	local BAG_TYPE_COMPLETED		= 2

	local tabBarAnimTime = 1000

	local tabBarLastProgress = 0

	function questsSetTabBarProgress(progressCurrent, progressGoal, doAnimation)
		doAnimation = doAnimation or false
		progressCurrent	= progressCurrent or 0
		progressGoal	= progressGoal or 1
		local progressBar = GetWidget('questsProgresBarTab')


		if doAnimation and ((progressCurrent / progressGoal) > tabBarLastProgress) then
			progressBar:ScaleWidth(ToPercent(progressCurrent / progressGoal), tabBarAnimTime)
			PlaySound('/ui/fe2/quests/sounds/progress_loop.wav', 1, 13)
			local endTime = GetTime() + tabBarAnimTime
			progressBar:UnregisterWatch('HostTime')
			progressBar:RegisterWatch('HostTime', function(widget, hostTime)
				progressBar:SetUScale((progressBar:GetHeight() * 20) .. 'p')
				if AtoN(hostTime) >= endTime then
					progressBar:UnregisterWatch('HostTime')
					PlaySound('/ui/fe2/quests/sounds/progress_end.wav', 1, 13)
				end
			end)
		else
			progressBar:SetWidth(ToPercent(progressCurrent / progressGoal))
			progressBar:SetUScale((progressBar:GetHeight() * 20) .. 'p')
		end
		tabBarLastProgress = (progressCurrent / progressGoal)

	end

	local function updateBagStatus(bagInfo, rewardIndex)
		--print("Bag Type: ")
		--print(bagInfo.bagType)
		--print(" Goal: ")
		--print(tostring(bagInfo.progressGoal))
		--print(" First: ")
		--print(tostring(bagInfo.isFirst))
		--print("\n")
		rewards[rewardIndex].setStatus(
			(bagInfo.bagType >= BAG_TYPE_PLAYER_QUEUE),
			((bagInfo.bagType >= BAG_TYPE_PLAYER_QUEUE) and bagInfo.progressCurrent >= bagInfo.progressGoal and bagInfo.progressGoal ~= 0),
			(bagInfo.bagType == BAG_TYPE_COMPLETED),
			bagInfo.isFirst,	-- is current?
			bagInfo.bagID,
			bagInfo.rewardType
		)
	end

	local rewardAnimThread = nil

	function questsPopulateRewardPopup(rewardType, rewardValue, rewardPath, rewardProductID, productType, isAltReward)	-- RMM local
		isAltReward		= isAltReward or false
		rewardType		= rewardType or REWARD_DISPLAY_TYPE_ICON
		rewardPath		= rewardPath or '/heroes/legionnaire/icon.tga'	-- products only
		rewardProductID	= rewardProductID or 119
		rewardValue		= rewardValue or 0								-- silver/gold amount or product code
		productType		= productType or 'Alt Avatar'

		if rewardAnimThread ~= nil then
			rewardAnimThread:Kill()
			rewardAnimThread = nil
		end

		if rewardType == REWARD_DISPLAY_TYPE_MODEL then
			PlaySound('/ui/fe2/quests/sounds/large_reward_jingle.tga')
		elseif rewardType == REWARD_DISPLAY_TYPE_ICON then
			PlaySound('/ui/fe2/quests/sounds/medium_reward_jingle.tga')
		else
			PlaySound('/ui/fe2/quests/sounds/small_reward_jingle.tga')
		end

		local widgetSuffix = 'Icon'
		if rewardType == REWARD_DISPLAY_TYPE_MODEL then
			widgetSuffix = 'Model'
			GetWidget('questsPrizePopupInfoIcon'):SetVisible(false)
		else
			GetWidget('questsPrizePopupInfoModel'):SetVisible(false)
		end

		local container	= GetWidget('questsPrizePopup')
		local chest		= GetWidget('questsPrizePopupChest')
		
		local prizeInfo = GetWidget('questsPrizePopupInfo'..widgetSuffix)
		local effect	= GetWidget('questsPrizePopupInfo'..widgetSuffix..'Effect')
		local typeLabel	= GetWidget('questsPrizePopupInfo'..widgetSuffix..'Type')
		local nameLabel	= GetWidget('questsPrizePopupInfo'..widgetSuffix..'Name')

		if rewardType == REWARD_DISPLAY_TYPE_TICKET or rewardType == REWARD_DISPLAY_TYPE_SILVER or rewardType == REWARD_DISPLAY_TYPE_GOLD then
			typeLabel:SetText(productType)
			nameLabel:SetText(rewardValue)
		else
			typeLabel:SetText(Translate('general_product_type_'..ProductTypeToPrefix(productType, true)))
		end

		local chestModelInfo = nil

		if rewardType == REWARD_DISPLAY_TYPE_MODEL then
			chestModelInfo = rewardContainers.goldChest
		elseif rewardType == REWARD_DISPLAY_TYPE_ICON then
			chestModelInfo = rewardContainers.silverChest
		elseif rewardType == REWARD_DISPLAY_TYPE_GOLD then
			chestModelInfo = rewardContainers.pouchGold
		else
			chestModelInfo = rewardContainers.pouch
		end

		chest:SetModel(chestModelInfo.model)

		-- effect:SetVisible(false)
		-- effect:SetEffect("")
		prizeInfo:SetY(prizeInfo:GetHeight())
		prizeInfo:SetVisible(1)

		GetWidget('questsPrizePopupInfo'..widgetSuffix..'Icon'):SetTexture(rewardPath)

		local courier = (productType == 'Couriers')

		if rewardType == REWARD_DISPLAY_TYPE_MODEL or rewardType == REWARD_DISPLAY_TYPE_ICON then
			nameLabel:SetText(Translate('mstore_product'..rewardProductID..'_name'))
		end

		-- for chat colors, make the font the color of the chat color
		if (productType == "Chat Color") then
			nameLabel:SetColor(GetChatNameColorStringFromUpgrades(rewardValue))
			nameLabel:SetGlowColor(GetChatNameGlowColorStringFromUpgrades(rewardValue))
			-- glow is set when the product slides up, so it doesn't show above the clip
			nameLabel:SetGlow(false)
		else
			nameLabel:SetColor('white')
			nameLabel:SetGlow(false)
		end


		if rewardType == REWARD_DISPLAY_TYPE_MODEL then

			if (not GetCvar('_entityDefinitionsLoaded')) then
				if (courier) then --courier are weird with their entity defs
					interface:UICmd("LoadEntityDefinitionsFromFolder('/items/basic/ground_familiar/')")
				else
					interface:UICmd("LoadEntityDefinitionsFromFolder('"..string.sub(rewardPath, 1, string.find(rewardPath, "/", 9)).."')")
				end
			end

			local prizeModel = GetWidget('questsPrizePopupInfo'..widgetSuffix..'Model')


			if (not courier) then
				-- get the raw hero_name for special case effect paths
				local heroName = explode(".", rewardValue)

				prizeModel:UICmd("SetCameraPos('30 10000 65');")
				prizeModel:UICmd("SetModelPos(GetHeroStorePosFromProduct('"..rewardValue.."'));")
				prizeModel:UICmd("SetModelAngles(GetHeroStoreAnglesFromProduct('"..rewardValue.."'));")
				prizeModel:UICmd("SetModelScale(GetHeroStoreScaleFromProduct('"..rewardValue.."'));")
				prizeModel:UICmd("SetModel(GetHeroPreviewModelPathFromProduct('"..rewardValue.."'));")
				if (heroName[1] ~= "Hero_Empath" and heroName[1] ~= "Hero_Gemini" and heroName[1] ~= "Hero_ShadowBlade" and heroName[1] ~= "Hero_Dampeer") then
					prizeModel:UICmd("SetEffectIndexed(GetHeroStorePassiveEffectPathFromProduct('"..rewardValue.."'), 0);")
				else
					prizeModel:UICmd("SetEffectIndexed(GetHeroPassiveEffectPathFromProduct('"..rewardValue.."'), 0);")
				end
				-- glow effect
				prizeModel:UICmd("SetEffectIndexed('/shared/effects/glow_gold_high.effect', 1);")
				prizeModel:UICmd("SetTeamColor('1 0 0');")
			else -- couriers don't have store stuff set
				typeLabel:SetText(Translate("general_courier"))
				prizeModel:UICmd("SetCameraPos('30 10000 30');")
				prizeModel:UICmd("SetModelPos(GetHeroPreviewPosFromProduct('"..rewardValue.."'));")
				prizeModel:UICmd("SetModelAngles(GetHeroPreviewAnglesFromProduct('"..rewardValue.."'));")
				prizeModel:UICmd("SetModelScale(GetHeroPreviewScaleFromProduct('"..rewardValue.."'));")
				prizeModel:UICmd("SetModel(GetHeroPreviewModelPathFromProduct('"..rewardValue.."'));")
				prizeModel:UICmd("SetEffectIndexed(GetHeroPassiveEffectPathFromProduct('"..rewardValue.."'), 0);")
				-- glow effect
				prizeModel:UICmd("SetEffectIndexed('/shared/effects/glow_gold_high.effect', 1);")
			end

		end

		GetWidget('questsPrizePopupClip'):SetY(chestModelInfo.clipPanelY)

		-- ======

		GetWidget('questsPrizePopupAltRewardNotice'):SetVisible(false)

		GetWidget('questsPrizePopup'):FadeIn(250)
		GetWidget('questsPrizePopupCloseX'):SetVisible(false)
		GetWidget('questsPrizePopupClose'):SetVisible(false)

		function questsPrizePopupClose()
			GetWidget('questsPrizePopup'):FadeOut(150, function()
				interface:UICmd("DeleteResourceContext('questsPrizePopup')")
			end)
		end

		-- ======

		rewardAnimThread = newthread(function()
			wait(GetFrameTime())
			chest:UICmd("SetCameraPos('"..chestModelInfo.cameraPos.."')")
			chest:SetAnim('idle')
			chest:SetEffect('')
			chest:SetEffect(chestModelInfo.effect or '')
			chest:SetVisible(true)
			wait(chestModelInfo.timeBeforeOpen)

			if rewardType ~= REWARD_DISPLAY_TYPE_SILVER and rewardType ~= REWARD_DISPLAY_TYPE_GOLD then
				chest:SetAnim('open')
			end

			wait(chestModelInfo.timeBeforeReveal)

			if (productType == "Chat Color") then
				nameLabel:SetGlow(GetChatNameGlowFromUpgrades(rewardValue))
				nameLabel:SetBackgroundGlow(GetChatNameBackgroundGlowFromUpgrades(rewardValue))
			end

			prizeInfo:SlideY("-7.5h", 750)

			wait(750)

			if isAltReward then
				object:GetWidget('questsPrizePopupAltRewardNotice'):FadeIn(250)
			end

			wait(1500)
			-- effect:SetEffect("/ui/fe2/plinko/effects/plinko_chest_gold.effect")
			-- effect:SetVisible(true)

			GetWidget('questsPrizePopupCloseX'):FadeIn(250)
			GetWidget('questsPrizePopupClose'):FadeIn(250)

			if lastOpenedBagIndex > 0 then
				updateBagStatus(questsGetBagInfoByIndex(lastOpenedBagIndex), lastOpenedBagIndex)
			end

			lastOpenedBagIndex = -1

			rewardAnimThread = nil
		end)

	end

	function questComingSoon()
		questsErrorMessage(tostring('Coming Soon!'))
	end

	function questTestError()
		questsErrorMessage(tostring('Big error, very scary'))
	end

	function questsTestSilverReward()
		questsPopulateRewardPopup(REWARD_DISPLAY_TYPE_SILVER, math.random(150, 350), rewardIconSilver, nil, Translate('sysbar_silver_tip_title'), true)
	end

	function questsTestGoldReward()
		questsPopulateRewardPopup(REWARD_DISPLAY_TYPE_GOLD, math.random(150, 350), rewardIconGold, nil, Translate('sysbar_gold_tip_title'))
	end

	function questsTestProductReward()
		questsPopulateRewardPopup(REWARD_DISPLAY_TYPE_ICON, nil, '/ui/fe2/store/icons/account_icons/i_heart_beer.tga', 219, 'Account Icon')
	end

	function questsTestProductRewardBig()
		questsPopulateRewardPopup(REWARD_DISPLAY_TYPE_MODEL, 'Hero_Legionnaire.Alt', '/heroes/legionnaire/alt/icon.tga', 842, 'Alt Avatar')
	end

	function questsTestProductRewardNameColor()
		questsPopulateRewardPopup(REWARD_DISPLAY_TYPE_ICON, 'cc.diamond', '/ui/icons/diamond.tga', 185, 'Chat Color')
	end

	function questsTestProductRewardNameColorGlow()
		questsPopulateRewardPopup(REWARD_DISPLAY_TYPE_ICON, 'cc.glowinghalloween', '/ui/icons/glowing_halloween.tga', 2608, 'Chat Color')
	end

	function questsTestProductRewardTaunt()
		questsPopulateRewardPopup(REWARD_DISPLAY_TYPE_ICON, 't.Chiprel_Taunt', '/ui/fe2/store/icons/taunt_chiprel.tga', 2501, 'Taunt')
	end

	function questsTestProductRewardAnnouncer()
		questsPopulateRewardPopup(REWARD_DISPLAY_TYPE_ICON, 'av.Ninja Announcer Pack', '/ui/fe2/store/icons/announcer_ninja.tga', 2339, 'Alt Announcement')
	end

	function questsTestProductRewardSymbol()
		questsPopulateRewardPopup(REWARD_DISPLAY_TYPE_ICON, 'cs.12th Day of Christmas', '/ui/fe2/store/icons/12th_day_christmas.tga', 2166, 'Chat Symbol')
	end

	function questsTestProductRewardAccountIcon()
		questsPopulateRewardPopup(REWARD_DISPLAY_TYPE_ICON, 'ai.Call to Arms Bronze', '/ui/fe2/store/icons/account_icons/call_to_arms_bronze.tga', 2504, 'Account Icon')
	end

	function questsTestProductRewardCourier()
		questsPopulateRewardPopup(REWARD_DISPLAY_TYPE_MODEL, 'Pet_GroundFamiliar.C_Kongor', '/shared/automated_courier/couriers/kongor/icon.tga', 1083, 'Couriers')
	end

	local function updateBagCompleteInfo(bagTable, newInfo, hadProduct)
		hadProduct = hadProduct or false	-- rmm if you already had the product, flag that in the bag info
		local productInfo = newInfo.PRODUCT_ID or newInfo.HERO
		if productInfo then
			bagTable.rewardType		= REWARD_TYPE_PRODUCT
			bagTable.productCode		= productInfo.type
			if productInfo.productid then
				bagTable.productID	= productInfo.productid
				bagTable.name			= Translate('mstore_product'..productInfo.productid..'_name')
				local productDesc = Translate('mstore_product'..productInfo.productid..'_desc')
				if productDesc == Translate('mstore_product'..productInfo.productid..'_desc') then
					productDesc = ''
				end
				bagTable.description	= productDesc
			end
			bagTable.icon			= productInfo.localcontent
		elseif newInfo.POINTS then
			bagTable.rewardType	= REWARD_TYPE_GOLD
			bagTable.gold		= newInfo.POINTS
		elseif newInfo.MMPOINTS then
			bagTable.rewardType	= REWARD_TYPE_SILVER
			bagTable.silver		= newInfo.MMPOINTS
		else
			bagTable.rewardType = REWARD_TYPE_UNKNOWN
		end
		bagTable.bagType = BAG_TYPE_COMPLETED
	end
	
	SetQuestUpdateCallback(function(questTable, updateSource)
		if not HoN_Region.regionTable[HoN_Region.activeRegion].questSystem then return end

		lastUpdateFromLogin = (updateSource == 'login')

		--for key, value in pairs(questTable) do
		--	print(key .. tostring(value) .. "\n")
		--end

		local firstPlayerBag = nil

		local tempBagID = nil

		if questTable.success and questTable.success == false then
			if questTable.error and string.len(questTable.error) > 0 then
				questsErrorMessage(tostring(Translate('quest_error_'..questTable.error)))
			end
		end

		if questTable.bag_queue then
			for k,v in pairs(questTable.bag_queue) do
				tempBagID = tonumber(v.bagid)
				initializeBagByID(tempBagID, BAG_TYPE_QUEUE, 'bag_queue', 0, v.achievement)
				bagInfoByID[tempBagID].progressGoal = tonumber(v.achievement) or 999
				if v.info then
					if v.info.PRODUCT_ID then
						bagInfoByID[tempBagID].rewardType	= REWARD_TYPE_PRODUCT
					elseif v.info.POINTS then
						bagInfoByID[tempBagID].rewardType = REWARD_TYPE_GOLD
					elseif v.info.MMPOINTS then
						bagInfoByID[tempBagID].rewardType = REWARD_TYPE_SILVER
					else
						bagInfoByID[tempBagID].rewardType = REWARD_TYPE_UNKNOWN
					end
				end
			end
		end
		
		
		--local function tprint (tbl, indent)
		--if not indent then indent = 0 end
		--  for k, v in pairs(tbl) do
		--	formatting = string.rep("  ", indent) .. k .. ": "
		--	if type(v) == "table" then
		--	  print(formatting .. "\n")
		--	  tprint(v, indent+1)
		--	else
		--	  print(formatting .. v .. "\n")
		--	end
		--  end
		--end

		if questTable.player_bag_queue then
			for j,l in pairs(questTable.player_bag_queue) do
				for k,v in pairs(l) do
					--tprint(v)
					tempBagID = tonumber(v.bag.bagid)
					initializeBagByID(tempBagID, BAG_TYPE_PLAYER_QUEUE, 'player_bag_queue', v.achievementstatus, bagInfoByID[tempBagID].progressGoal)
					bagInfoByID[tempBagID].progressCurrent = tonumber(v.bag.achievementstatus) or 0
				end
			end
		end

		if questTable.player_bag_complete then
			for k,v in pairs(questTable.player_bag_complete) do
				tempBagID = tonumber(v.bagid)
				initializeBagByID(tempBagID, BAG_TYPE_COMPLETED, 'player_bag_complete')
				if v.info then
					updateBagCompleteInfo(bagInfoByID[tempBagID], v.info)
				end
			end
		end

		local bagList = {}

		for k,v in pairs(bagInfoByID) do
			table.insert(bagList, v)
		end

		table.sort(bagList, function(a, b)
			return a.bagID < b.bagID
		end)

		local foundFirstBag = false

		local scrollToBag = nil

		for k,v in ipairs(bagList) do
			if (v.bagType ~= BAG_TYPE_COMPLETED) and v.progressCurrent < v.progressGoal and (not foundFirstBag) then
				v.isFirst = true
				foundFirstBag = true
				firstPlayerBag = v
				--questsSetTabBarProgress(v.progressCurrent, v.progressGoal, (not lastUpdateFromLogin))
				if (not scrollToBag) then
					scrollToBag = k
				end
			elseif (v.bagType ~= BAG_TYPE_COMPLETED) and (not scrollToBag) then
				v.isFirst = false
				scrollToBag = k
			else
				v.isFirst = false
			end
		end

		if updateSource ~= "reset" then
			questsScrollToPosition(scrollToBag, (not lastUpdateFromLogin))
		end

		local bagIDByIndex = {}

		function questsGetBagInfoByIndex(bagIndex)
			if bagIDByIndex[bagIndex] then
				return bagInfoByID[bagIDByIndex[bagIndex]]
			end
		end

		for i=1,rewardsCount,1 do
			if bagList[i] then
				bagIDByIndex[i] = bagList[i].bagID
				updateBagStatus(bagList[i], i)
			else 
				rewards[i].setStatus(nil, nil, nil, nil, -1)
			end
		end

		lastResetsAvailable = AtoN(questTable.quest_resets_available or '0')

		Trigger('questsResetCountUpdate', lastResetsAvailable or 0)

	end)

	function questsPlayerBagQueueFull()
		local queueBags = 0
		for k,v in pairs(bagInfoByID) do
			if v.bagType == 1 then
				queueBags = queueBags + 1
			end
		end
		return queueBags >= 5	-- rmm return queue status
	end

	function questResetsAvailable()
		return (questResetCountLeft) >= 1
	end

	GetWidget('questsLadderButton'):SetCallback('onclick', function(widget)
	
		--reenable functions below to bring back the ladder
	
		--Player_Stats.ClickedQuests(true)
		--trackedUserFocus:incrementActionCount('questsLadderButtonClicked')
		
		questComingSoon()
		
	end)

	GetWidget('questsLadderButton'):SetCallback('onmouseover', function(widget)
		Trigger('genericMainFloatingTip', 'true', '24.0h', '', 'quests_open_ladder', 'quests_open_ladder_tip', '', '', '22', '-22')
	end)

	GetWidget('questsLadderButton'):SetCallback('onmouseout', function(widget)
		Trigger('genericMainFloatingTip', 'false', '', '', '', '', '', '', '', '')
	end)

	container:RegisterWatch('QuestResetStatus', function(widget, requestStatus, wasSuccessful, errorString)
		if requestStatus == 'Error' then
			questsErrorMessage(tostring(Translate('quest_error_reset_quest_request_fail')))
			questResetQueued = -1
		elseif requestStatus == 'Sending' then
			lastStatusResetQuest = true
			processQuestStatus()
			return
		end

		if AtoB(wasSuccessful) == false then
			if errorString and string.len(errorString) > 0 then
				questsErrorMessage(tostring(Translate('quest_error_'..errorString)))
			end
		else
			lastResetsAvailable = lastResetsAvailable - 1
			Trigger('questsResetCountUpdate', lastResetsAvailable)
		end

		lastStatusResetQuest = false
		processQuestStatus()
	end)


	container:RegisterWatch('QuestsUpdateStatus', function(widget, requestStatus, wasSuccessful, errorString)
		if requestStatus == 'Error' then
			questsErrorMessage(tostring(Translate('quest_error_update_quest_request_fail')))
		elseif requestStatus == 'Sending' then
			lastStatusUpdateQuest = true
			processQuestStatus()
			return
		end

		if AtoB(wasSuccessful) == false then
			if errorString and string.len(errorString) > 0 then
				questsErrorMessage(tostring(Translate('quest_error_'..errorString)))
			end
		end

		lastStatusUpdateQuest = false
		processQuestStatus()
	end)


	local openRewardForm = object:GetForm('questsOpenReward')
	openRewardForm:SetResponseCallback(function(updatedForm)
		local responseTable = updatedForm:GetLastResponse()
		lastUpdateFromLogin = false
		if responseTable.success then
			-- Update upgrades and such so the player can access their new stuff
			interface:UICmd("ChatRefreshUpgrades(); ClientRefreshUpgrades(); ServerRefreshUpgrades();")
			
			local rewardType = nil
			if responseTable.info.MMPOINTS then
				rewardType = REWARD_DISPLAY_TYPE_SILVER
			elseif responseTable.info.POINTS then
				rewardType = REWARD_DISPLAY_TYPE_GOLD
			elseif responseTable.info.PRODUCT_ID then
				if ((responseTable.info.PRODUCT_ID.type == "Alt Avatar") or (responseTable.info.PRODUCT_ID.type == "Couriers") or (responseTable.info.PRODUCT_ID.type == "Hero") or (responseTable.info.PRODUCT_ID.type == "EAP")) then
					rewardType = REWARD_DISPLAY_TYPE_MODEL
				else
					rewardType = REWARD_DISPLAY_TYPE_ICON
				end
			end

			local isAltReward = false

			if responseTable.info.MMPOINTS and bagInfoByID[tonumber(responseTable.bag.bagid)].rewardType ~= REWARD_TYPE_SILVER then
				isAltReward = true
			end

			updateBagCompleteInfo(bagInfoByID[tonumber(responseTable.bag.bagid)], responseTable.info)

			if rewardType == REWARD_DISPLAY_TYPE_SILVER then
				questsPopulateRewardPopup(rewardType, AtoN(responseTable.info.MMPOINTS or 0), rewardIconSilver, nil, Translate('sysbar_silver_tip_title'), isAltReward)
			elseif rewardType == REWARD_DISPLAY_TYPE_GOLD then
				questsPopulateRewardPopup(rewardType, AtoN(responseTable.info.POINTS or 0), rewardIconGold, nil, Translate('sysbar_gold_tip_title'))
			else
				if responseTable.info.PRODUCT_ID then
					questsPopulateRewardPopup(rewardType, responseTable.info.PRODUCT_ID.name, responseTable.info.PRODUCT_ID.localcontent, responseTable.info.PRODUCT_ID.productid, responseTable.info.PRODUCT_ID.type)
				else
					print('Non-silver/gold reward with no product information.\n')
				end
			end
		else
			questsErrorMessage(tostring(Translate('quest_error_'..responseTable.error)))
		end
	end)

	--[[

		ARRAY:
		  mmpoints, INT: 0
		  points, INT: 0
		  product_ids, INT: 0
		  success, BOOL: 1
		  bag, ARRAY:
		    bagid, STRING: 1
		    active, STRING: 1
		    baggroupid, STRING: 2
		    priority, STRING: NORMAL
		    achievement, STRING: 100
		    reward, STRING: 25
		    rewardtype, STRING: MMPOINTS
		    addtimestamp, STRING: 2014-12-04 21:23:16
		    modifytimestamp, STRING: 2014-12-04 21:23:16
		  info, ARRAY:
		    MMPOINTS, STRING: 25
		bag = {
		    ['bagid'] = "1",
		    ['active'] = "1",
		    ['rewardtype'] = "MMPOINTS",
		    ['addtimestamp'] = "2014-12-04 21:23:16",
		    ['achievement'] = "100",
		    ['modifytimestamp'] = "2014-12-04 21:23:16",
		    ['priority'] = "NORMAL",
		    ['baggroupid'] = "2",
		    ['reward'] = "25",
		},
		['success'] = true,
		['product_ids'] = 0,
		['points'] = 0,
		info = {
		    ['MMPOINTS'] = "25",

			OR PRODUCT INFO ==============

			PRODUCT_ID = {
			['localcontent'] = "/ui/icons/flags/turkey.tga",
			['productid'] = "75",
			['name'] = "turkey",
			['type'] = "Chat Symbol",
			},



		},
		['mmpoints'] = 0,
	
	--]]

	function questsOpenReward(bagID, bagIndex)
		if HoN_Region.regionTable[HoN_Region.activeRegion].questSystem then
			if bagID and type(bagID) == 'number' and bagID >= 0 then
				if bagIndex and type(bagIndex) == 'number' and bagIndex >= 1 and bagIndex <= rewardsCount then
					lastOpenedBagIndex = bagIndex
				else
					lastOpenedBagIndex = -1
				end
				SubmitForm('questsOpenReward', 'cookie', Client.GetCookie(), 'bag_id', bagID)
			end
		end
	end

	--[[
	function questsUpdateTabUnreadMessages(messageCount)
		messageCount = messageCount or 0

		if messageCount > 0 then
			GetWidget('chat_tab_label'):SetText(Translate('communicator_tab_chat')..' (^w'..messageCount..'^*)')
		else
			GetWidget('chat_tab_label'):SetText(Translate('communicator_tab_chat'))
		end
	end
	--]]

	local statForms = {
		{ 'PlayerStats', 		'PlayerStatsResult' },
		{ 'PlayerStatsRanked', 	'PlayerStatsResultRanked' },
		{ 'PlayerStatsCasual', 	'PlayerStatsResultCasual' },
		{ 'PlayerStatsEvent', 	'PlayerStatsResultEvent' }
	}

	for k,v in ipairs(statForms) do
		local statForm = object:GetForm(v[1])
		if statForm then
			statForm:SetResponseCallback(function(playerStatsForm)
				local responseTable = playerStatsForm:GetLastResponse()
				if responseTable.quest_stats then
					Trigger(v[2]..'Quests', responseTable.quest_stats.quests_complete or 0)
				end
			end)
		end
	end

	rewardsUpdateScrollButtons()

	if HoN_Region.regionTable[HoN_Region.activeRegion].questSystem then
		GetWidget('communicatorTabsHasQuests'):SetVisible(true)
		GetWidget('communicatorTabsNoQuests'):SetVisible(false)
		GetWidget('playerLadderTabQuestParent'):SetVisible(true)
	else
		GetWidget('communicatorTabsHasQuests'):SetVisible(false)
		GetWidget('communicatorTabsNoQuests'):SetVisible(true)
		GetWidget('playerLadderTabQuestParent'):SetVisible(false)
	end
end

questsRegister(object)



-- This is redundant and only exists due to the position of main.lua.

local questsLastDisabled			= false
local questLadderLastDisabled		= false
local questsLastDisabledReason		= false
local questLadderLastDisabledReason	= false

local QUEST_DISABLED_REASON_GENERAL		= 0
local QUEST_DISABLED_REASON_ISENABLED	= 1
local QUEST_DISABLED_REASON_TECHNICAL	= 2
local QUEST_DISABLED_REASON_SEASON		= 3

GetWidget('player_stats_quests_tab'):RegisterWatch('QuestsDisabled', function(widget, questsDisabled, questsDisabledReason, questLadderDisabled, questLadderDisabledReason)
	questsLastDisabled = AtoB(questsDisabled)
	questLadderLastDisabled = AtoB(questLadderDisabled)

	questsLastDisabledReason = AtoN(questsDisabledReason)
	questLadderLastDisabledReason = AtoN(questLadderDisabledReason)


	GetWidget('questsLadderButton'):SetEnabled(not questLadderLastDisabled)
end)

GetWidget('quest_tab'):SetCallback('onmouseover', function(widget)
	Trigger('genericMainFloatingTip', 'true', '24.0h', '', 'communicator_tab_quests', 'communicator_tab_quests_tip', '', '', '22', '-22')
end)

GetWidget('quest_tab'):SetCallback('onmouseout', function(widget)
	Trigger('genericMainFloatingTip', 'false', '', '', '', '', '', '', '', '')
end)

GetWidget('quest_tab'):SetCallback('onmouseoverdisabled', function(widget)
	if questsLastDisabledReason == QUEST_DISABLED_REASON_TECHNICAL then
		Trigger('genericMainFloatingTip', 'true', '24.0h', '', 'quests_disabled', 'quests_disabled_technical_body', '', '', '22', '-22')
	elseif questsLastDisabledReason == QUEST_DISABLED_REASON_SEASON then
		Trigger('genericMainFloatingTip', 'true', '24.0h', '', 'quests_disabled', 'quests_disabled_ended_body', '', '', '22', '-22')
	else
		Trigger('genericMainFloatingTip', 'true', '24.0h', '', 'quests_disabled', 'quests_disabled_body', '', '', '22', '-22')
	end
end)

GetWidget('quest_tab'):SetCallback('onmouseoutdisabled', function(widget)
	Trigger('genericMainFloatingTip', 'false', '', '', '', '', '', '', '', '')
end)


GetWidget('questsLadderButton'):SetCallback('onmouseoverdisabled', function(widget)
	if questLadderLastDisabledReason == QUEST_DISABLED_REASON_TECHNICAL then
		Trigger('genericMainFloatingTip', 'true', '24.0h', '', 'quests_disabled', 'quests_ladder_disabled_technical_body', '', '', '22', '-22')
	elseif questLadderLastDisabledReason == QUEST_DISABLED_REASON_SEASON then
		Trigger('genericMainFloatingTip', 'true', '24.0h', '', 'quests_disabled', 'quests_ladder_disabled_ended_body', '', '', '22', '-22')
	else
		Trigger('genericMainFloatingTip', 'true', '24.0h', '', 'quests_disabled', 'quests_ladder_disabled_body', '', '', '22', '-22')
	end
end)

GetWidget('player_stats_quests_tab'):SetCallback('onmouseover', function(widget)
	PlaySound('/shared/sounds/ui/ccpanel/button_over_02.wav');
end)

GetWidget('questsLadderButton'):SetCallback('onmouseoutdisabled', function(widget)
	Trigger('genericMainFloatingTip', 'false', '', '', '', '', '', '', '', '')
end)


GetWidget('player_stats_quests_tab'):SetCallback('onmouseoverdisabled', function(widget)
	if questLadderLastDisabledReason == QUEST_DISABLED_REASON_TECHNICAL then
		Trigger('genericMainFloatingTip', 'true', '24.0h', '', 'quests_disabled', 'quests_disabled_technical_body', '', '', '22', '-22')
	elseif questLadderLastDisabledReason == QUEST_DISABLED_REASON_SEASON then
		Trigger('genericMainFloatingTip', 'true', '24.0h', '', 'quests_disabled', 'quests_disabled_ended_body', '', '', '22', '-22')
	else
		Trigger('genericMainFloatingTip', 'true', '24.0h', '', 'quests_disabled', 'quests_disabled_body', '', '', '22', '-22')
	end
end)

GetWidget('player_stats_quests_tab'):SetCallback('onmouseoutdisabled', function(widget)
	Trigger('genericMainFloatingTip', 'false', '', '', '', '', '', '', '', '')
end)


