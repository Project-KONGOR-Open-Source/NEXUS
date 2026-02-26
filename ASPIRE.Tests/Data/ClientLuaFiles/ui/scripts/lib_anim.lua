
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind

lib_Anim = {}

ANIM_PERCENT_PER_SECOND = 50
ANIM_BAR_GROW_TIME = 5000
BAR_SHOW_DELAY = 1000
ANIM_SHORT_DELAY = 150
ANIM_LONG_DELAY = 300
ANIM_LABEL_LERP_DURATION = 750
ANIM_LABEL_LERP_DELAY = 20

local interface, interfaceName = object, object:GetName()

function lib_Anim.AnimateProgressBar(barName, barShowDelay, barCurrentPercent, barNewPercent, hoverValue1, hoverValue2, coverUpNew, secondDelay)
	
	if GetWidget('progbar_'..barName..'_parent') and (barCurrentPercent) and (barNewPercent) then
	
		local currentGrowTime = (barCurrentPercent * 100000 / ANIM_PERCENT_PER_SECOND)
		local newGrowTime = (barNewPercent * 100000 / ANIM_PERCENT_PER_SECOND) - currentGrowTime
		local carryOver
		
		if (barNewPercent > 1.0) then
			carryOver = barNewPercent - 1.0
			barNewPercent = 1.0
		end	
		
		GetWidget('progbar_'..barName..'_parent'):SetVisible(true)
		if (hoverValue1) and (hoverValue2) then
			GetWidget('progbar_'..barName..'_parent'):SetCallback('onmouseover', function() GetWidget('progbar_'..barName..'_hover'):FadeIn(150) end)
			GetWidget('progbar_'..barName..'_parent'):SetCallback('onmouseout', function() GetWidget('progbar_'..barName..'_hover'):FadeOut(150) end)
			GetWidget('progbar_'..barName..'_parent'):RefreshCallbacks()
			GetWidget('progbar_'..barName..'_hover_label'):SetText(floor(hoverValue1) .. ' / ' .. floor(hoverValue2))
		end		
		
		-- reset new bar
		GetWidget('progbar_'..barName..'_new_bg'):SetVisible(false)
		GetWidget('progbar_'..barName..'_new_bg'):SetWidth('0')
		GetWidget('progbar_'..barName..'_new_glow'):SetVisible(false)
		
		-- reset current (old) bar
		GetWidget('progbar_'..barName..'_cur_bg'):SetVisible(false)
		GetWidget('progbar_'..barName..'_cur_bg'):SetWidth('0')
		GetWidget('progbar_'..barName..'_cur_glow'):SetVisible(false)
		
		GetWidget('progbar_'..barName..'_parent'):Sleep(barShowDelay, function()
			
			-- current bar
			GetWidget('progbar_'..barName..'_cur_bg'):SetWidth(0)
			GetWidget('progbar_'..barName..'_cur_bg'):SetVisible(true)
			GetWidget('progbar_'..barName..'_cur_bg'):ScaleWidth(FtoP(barCurrentPercent), currentGrowTime)					
			
			GetWidget('progbar_'..barName..'_cur_fog'):SetWatch('HostTime')
			GetWidget('progbar_'..barName..'_cur_fog'):Sleep(currentGrowTime, function() GetWidget('progbar_'..barName..'_cur_fog'):SetWatch('') end)
			
			GetWidget('progbar_'..barName..'_cur_glow'):SetVisible(true)
			GetWidget('progbar_'..barName..'_cur_glow'):Sleep(currentGrowTime * 0.8, function() GetWidget('progbar_'..barName..'_cur_glow'):FadeOut(currentGrowTime * 0.6) end)
			--GetWidget('progbar_'..barName..'_cur_glow'):FadeOut(currentGrowTime * 1.8)
			
			-- scale new bar (hidden)
			GetWidget('progbar_'..barName..'_new_bg'):SetWidth(0)
			
			if (barNewPercent > barCurrentPercent) then
			
				GetWidget('progbar_'..barName..'_new_bg'):ScaleWidth(FtoP(barNewPercent), currentGrowTime + (currentGrowTime * 0.4) + newGrowTime + ANIM_SHORT_DELAY)
				
				GetWidget('progbar_'..barName..'_new_fog'):SetWatch('HostTime')
				GetWidget('progbar_'..barName..'_new_fog'):Sleep(currentGrowTime + (currentGrowTime * 0.4) + newGrowTime + ANIM_SHORT_DELAY, function() GetWidget('progbar_'..barName..'_new_fog'):SetWatch('') end)		
				
				GetWidget('progbar_'..barName..'_new_bg'):Sleep(currentGrowTime + (currentGrowTime * 0.4), function()
					
					-- show new bar					
					GetWidget('progbar_'..barName..'_new_bg'):FadeIn(ANIM_SHORT_DELAY/2)
										
					GetWidget('progbar_'..barName..'_new_glow'):SetVisible(true)
					GetWidget('progbar_'..barName..'_new_glow'):Sleep(newGrowTime * 0.8, function() GetWidget('progbar_'..barName..'_new_glow'):FadeOut((newGrowTime * 0.6) + ANIM_LONG_DELAY) end)
					--GetWidget('progbar_'..barName..'_new_glow'):FadeOut((newGrowTime * 1.8) + (ANIM_LONG_DELAY * 2))
					
					if (carryOver) and (carryOver > 0) then		
						GetWidget('progbar_'..barName..'_parent'):Sleep((ANIM_LONG_DELAY * 3) + newGrowTime + ANIM_SHORT_DELAY, function()
							lib_Anim.AnimateProgressBar(barName, ANIM_SHORT_DELAY, 0.0, carryOver)
						end)
					elseif (coverUpNew) then
						GetWidget('progbar_'..barName..'_parent'):Sleep((secondDelay) + newGrowTime + 2500, function()
							GetWidget('progbar_'..barName..'_cur_bg'):ScaleWidth(FtoP(barNewPercent), newGrowTime)					
							
							GetWidget('progbar_'..barName..'_cur_fog'):SetWatch('HostTime')
							GetWidget('progbar_'..barName..'_cur_fog'):Sleep(newGrowTime, function() GetWidget('progbar_'..barName..'_cur_fog'):SetWatch('') end)
							
							GetWidget('progbar_'..barName..'_cur_glow'):SetVisible(true)
							GetWidget('progbar_'..barName..'_cur_glow'):Sleep(newGrowTime * 0.8, function() GetWidget('progbar_'..barName..'_cur_glow'):FadeOut(newGrowTime * 0.6) end)			
						end)
					end
				end)
				
			end
		end)
	end
end

function lib_Anim.AnimateIconPulse(imageName)
	if GetWidget('progiconpulse_'..imageName) then
	
		GetWidget('progiconpulse_'..imageName):SetVisible(true)
		GetWidget('progiconpulse_'..imageName..'_icon'):ScaleWidth('400%', 250)
		GetWidget('progiconpulse_'..imageName..'_icon'):ScaleHeight('400%', 250)
		
		GetWidget('progiconpulse_'..imageName):Sleep(280, function()
			GetWidget('progiconpulse_'..imageName..'_icon'):ScaleWidth('100%', 100)
			GetWidget('progiconpulse_'..imageName..'_icon'):ScaleHeight('100%', 100)
		end)
	end
end

function lib_Anim.RewardStat_Pulse(img, matchResult, interval)
	if GetWidget('rewardstat_placementsico_'..img) then
		
		local mResult = matchResult
		
		local function FuncMatchResultTexture(param)
			local winLoss = tonumber(param)
			
			if winLoss == 1 then
				return '/ui/fe2/NewUI/Res/match_stats/placements/place_victory.png'
			elseif winLoss == 0 then
				return '/ui/fe2/NewUI/Res/match_stats/placements/place_loss.png'
			else
				Echo('lib_anim error: result texture had a false return? should never happen and is problematic (saftey check) notify hyperxewl.')
			end
		end

		local function FuncMatchResultColor(param)
			local winLoss = tonumber(param)
			
			if winLoss == 1 then
				return '#00e2ff'
			elseif winLoss == 0 then
				return '#00e2ff'
			else
				Echo('lib_anim error: result color had a false return?')
			end
		end
		
		GetWidget('rewardstat_placementsico_'..img):SetTexture(FuncMatchResultTexture(mResult))
		GetWidget('rewardstat_placementsico_'..img):SetColor(FuncMatchResultColor(mResult))
		
		if interval > 0 then
			GetWidget('rewardstat_placementsico_'..img):BringToFront()
			GetWidget('rewardstat_placementsico_'..img):ScaleWidth('128i', 250)
			GetWidget('rewardstat_placementsico_'..img):ScaleHeight('128i', 250)
		
			GetWidget('rewardstat_placementsico_'..img):Sleep(interval, function()
				GetWidget('rewardstat_placementsico_'..img):ScaleWidth('64i', 100)
				GetWidget('rewardstat_placementsico_'..img):ScaleHeight('64i', 100)
			end)
		else
		end
	end
end

function lib_Anim.RewardStat_PieGraphAnim(widget, stop)

	if stop == 'stop' then
		for i=2,6 do
			timer = 1
			interface:Sleep(1, function() GetWidget('rewardstat_placementspie_' .. widget):SetValue(0) end)	
		end
	elseif stop == 'instant' then
		timer = 1
		GetWidget('rewardstat_placementspie_' .. widget):SetValue(1)
	else
	
		timer = 0
		function PieTimer(param)
			
			timer = (timer+0.022)
			GetWidget('rewardstat_placementspie_' .. widget):SetValue(timer)
			if timer < 1 then
				interface:Sleep(3, function() PieTimer() end)
			else
				timer = 0
			end
		end
		
		PieTimer()
	end
	
end

function lib_Anim.AnimateNumericLabel(labelName, startDelay, startValue, endValue, showDecimal, prefix)
	local prefix = prefix or ''
	if (startValue) and (endValue) and (endValue > startValue) and GetWidget(labelName) then
		local lerpDelta = endValue - startValue
		local lerpStep = lerpDelta / (ANIM_LABEL_LERP_DURATION / ANIM_LABEL_LERP_DELAY)
		local currentValue = startValue
		local limit = 0	
		
		GetWidget(labelName):FadeIn(ANIM_SHORT_DELAY)
		GetWidget(labelName):SetText(prefix..startValue)
		
		local function LoopUpdateLabel()
			limit = limit + 1
			if (currentValue < endValue) and (limit < 500) then
				currentValue = currentValue + lerpStep
				if (showDecimal) then
					GetWidget(labelName):SetText(prefix..format("%.2f", currentValue))
				else
					GetWidget(labelName):SetText(prefix..math.floor(currentValue))
				end
				GetWidget(labelName):Sleep(ANIM_LABEL_LERP_DELAY, function() LoopUpdateLabel() end)
			else
				if (showDecimal) then
					GetWidget(labelName):SetText(prefix..format("%.2f", endValue))
				else
					GetWidget(labelName):SetText(prefix..math.floor(endValue))
				end
			end
		end		
		
		GetWidget(labelName):Sleep(ANIM_SHORT_DELAY, function()	
			LoopUpdateLabel()
		end)
	else
		GetWidget(labelName):FadeIn(ANIM_SHORT_DELAY)
		if (showDecimal) then
			GetWidget(labelName):SetText(prefix..format("%.2f", endValue or 0))
		else
			GetWidget(labelName):SetText(prefix..math.floor(endValue or 0))
		end		
	end
end

lib_Anim.ANIMATION_QUEUE = {
	{6000, {
			[[lib_Anim.AnimateIconPulse('account_xp')]],
			[[lib_Anim.AnimateNumericLabel('account_xp', 500, 0, 84 )]],
			[[lib_Anim.AnimateProgressBar('xp', 500, 0.7, 0.85 )]]
		}
	},
	{6000, {
			[[lib_Anim.AnimateIconPulse('account_xp')]],
			[[lib_Anim.AnimateNumericLabel('account_xp', 500, 0, 176 )]],
			[[lib_Anim.AnimateProgressBar('xp', 500, 0.2, 0.85 )]]
		}
	},
	{6000, {
			[[lib_Anim.AnimateIconPulse('account_xp')]],
			[[lib_Anim.AnimateNumericLabel('account_xp', 500, 45, 451 )]],
			[[lib_Anim.AnimateProgressBar('xp', 500, 0.8, 1.25 )]]
		}
	},	
	{6000, {
			[[lib_Anim.AnimateIconPulse('account_xp')]],
			[[lib_Anim.AnimateNumericLabel('account_xp', 500, 0, 21 )]],
			[[lib_Anim.AnimateProgressBar('xp', 500, 0.4, 0.45 )]]
		}
	},	
}

function lib_Anim.ProcessAnimationQueue(animationQueueTable)
	animationQueueTable = animationQueueTable or lib_Anim.ANIMATION_QUEUE
	local limit = 0
	local function LoopProcessAnimationQueue()
		limit = limit + 1
		
		--println('LoopProcessAnimationQueue = ' .. limit)
		
		printTable(animationQueueTable)
		
		if (#animationQueueTable > 0) then
			animationTableTable = animationQueueTable[1]
			
			local delayBeforeNext = animationTableTable[1]
			local animationTable  = animationTableTable[2]
			
			--println('delayBeforeNext = ' .. delayBeforeNext)
			--printTable(animationTable)
			
			for animIndex, animationFunction in ipairs(animationTable) do
				--println('animationFunction = ' .. tostring(animationFunction) )
				loadstring(animationFunction)()
			end

			table.remove(animationQueueTable, 1)
		
			if (#animationQueueTable > 0) and (limit < 50) then
				GetWidget('dev_blank_backer'):Sleep(delayBeforeNext, function()
					LoopProcessAnimationQueue()
				end)
			end
		end	
	end
	LoopProcessAnimationQueue()
end

function lib_Anim.AddToAnimationqueue(additionalTable)
	table.insert(lib_Anim.ANIMATION_QUEUE, additionalTable)
end

local function Bang5(_, current, new)
	lib_Anim.ProcessAnimationQueue()
	--AnimateIconPulse('account_xp')
	--AnimateNumericLabel('account_xp', 500, 0, 84 )
	--AnimateProgressBar('xp', 500, 0.7, 0.85 )
end
object:RegisterWatch('Bang5', Bang5)


