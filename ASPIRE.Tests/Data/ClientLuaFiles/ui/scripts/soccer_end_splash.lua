-- soccer_splash stuff

local function soccerSetNumberWidgets(value, container, template)
	if value and type(value) == 'number' then
		template = template or 'soccerLargeNumberImage'
		value = tostring(value)

		local widgetList = container:GetChildren()
		local widgetCount = #widgetList
		local valueLen = string.len(value)

		if widgetCount ~= valueLen then
			--clear all children
			for i=1, widgetCount, 1 do
				widgetList[i]:SetVisible(false)
				widgetList[i]:Destroy()
			end

			--fill in all numbers
			for i=1, valueLen, 1 do
				container:Instantiate(template, 'number', string.sub(value, i, i))
			end

			container:RecalculateSize()
		else
			for i=1,widgetCount,1 do
				widgetList[i]:GetChildren()[1]:SetTexture('/ui/elements/number_large_Soccer_'..string.sub(value, i, i)..'.tga')
				widgetList[i]:GetChildren()[1]:SetVisible(true)
			end
		end
	end
end

local function calcScore(paramGoals, paramSteals, paramAssists, paramKills)
	return (
		(paramGoals 	* 	10)	+
		(paramSteals 	* 	1) 	+
		(paramAssists	* 	5) 	+
		(paramKills 	* 	2)
	)
end

local function soccerEndSplashPopulatePlayer(side, index, playerInfo)
	local container	= GetWidget('soccerEndSplashSide'..side..'Player'..index)

	--debug
	--soccerSetNumberWidgets(2, GetWidget('soccerEndSplashSide'..side..'Player'..index..'Value'))
	--debug

	if playerInfo and playerInfo.getExists() then

		GetWidget('soccerEndSplashSide'..side..'Player'..index):SetVisible(true)

		local MVPFrameColor = '#2a425c'

		if ((playerInfo.getIsMVP() and playerInfo.getTeam() == 1) or ((not playerInfo.getIsMVP()) and playerInfo.getTeam() == 2)) then
			MVPFrameColor = '#d79c34'
		end

		local mvpLabel = GetWidget('soccerEndSplashSide'..side..'Player'..index..'MVP')
		if mvpLabel then
			GetWidget('soccerEndSplashSide'..side..'Player'..index..'MVP'):SetVisible(playerInfo.getIsTeamMVP())
		end

		GetWidget('soccerEndSplashSide'..side..'Player'..index..'Name'):SetText(playerInfo.getPlayerName())
		GetWidget('soccerEndSplashSide'..side..'Player'..index):SetVisible(playerInfo.getExists())
		GetWidget('soccerEndSplashSide'..side..'Player'..index..'HeroIcon'):SetTexture(playerInfo.getHeroIcon())

		-- GetWidget('soccerEndSplashSide'..side..'Player'..index..'Frame'):SetColor(MVPFrameColor)
		-- GetWidget('soccerEndSplashSide'..side..'Player'..index..'Frame'):SetBorderColor(MVPFrameColor)

		GetWidget('soccerEndSplashSide'..side..'Player'..index..'Hero'):SetText(playerInfo.getHeroName())

		soccerSetNumberWidgets(playerInfo.getGoals(), GetWidget('soccerEndSplashSide'..side..'Player'..index..'Value'))

		local match_id		= matchStatsGetMatchID()
		if GetWidget('soccerEndSplashSide'..side..'Player'..index..'Award1Icon') then
				GetWidget('soccerEndSplashSide'..side..'Player'..index..'Award1Icon'):SetTexture('/ui/icons/awards/award_'..matchStatsGetAssignedAward(playerInfo.getPosition(), '1')..'.tga')
		end
		if GetWidget('soccerEndSplashSide'..side..'Player'..index..'Award2Icon') then
			GetWidget('soccerEndSplashSide'..side..'Player'..index..'Award2Icon'):SetTexture('/ui/icons/awards/award_'..matchStatsGetAssignedAward(playerInfo.getPosition(), '2')..'.tga')
		end

		GetWidget('soccerEndSplashSide'..side..'Player'..index..'Award1'):SetCallback('onmouseover', function(widget)
			-- CallEventParamsX('match_stats_simple_player_item_{team}_{index}', 0, 1);
			GetWidget('match_stats_simple_player_item_'..side..'_'..playerInfo.getIndex()):DoEventN(0, 1)
		end)
		GetWidget('soccerEndSplashSide'..side..'Player'..index..'Award1'):SetCallback('onmouseout', function(widget)
			-- CallEventParamsX('match_stats_simple_player_item_{team}_{index}', 1, 1);
			GetWidget('match_stats_simple_player_item_'..side..'_'..playerInfo.getIndex()):DoEventN(1, 1)
		end)
		GetWidget('soccerEndSplashSide'..side..'Player'..index..'Award2'):SetCallback('onmouseover', function(widget)
			-- CallEventParamsX('match_stats_simple_player_item_{team}_{index}', 0, 2);
			GetWidget('match_stats_simple_player_item_'..side..'_'..playerInfo.getIndex()):DoEventN(0, 2)
		end)
		GetWidget('soccerEndSplashSide'..side..'Player'..index..'Award2'):SetCallback('onmouseout', function(widget)
			-- CallEventParamsX('match_stats_simple_player_item_{team}_{index}', 1, 2);
			GetWidget('match_stats_simple_player_item_'..side..'_'..playerInfo.getIndex()):DoEventN(1, 2)
		end)

	else
		GetWidget('soccerEndSplashSide'..side..'Player'..index):SetVisible(false)
	end
end

local function soccerEndSplashRegisterPlayerIndex(index, team, sortPlayers)
	local exists		= false
	local playerName	= ''
	local heroName		= ''
	local heroIcon		= ''
	local awardIndex	= { -1, -1 }

	local score			= 0
	local isMVP			= false
	local isTeamMVP		= false
	local position		= -1

	local goals			= 0 --gameplaystat0
	local goalAssists	= 0 --gameplaystat1
	local selfGoals		= 0 --gameplaystat2
	local steals		= 0 --gameplaystat3
	local timeWithBalls	= 0 --gameplaystat4
	local deaths		= 0 --gameplaystat5
	local kills			= 0 --gameplaystat6
	local runesGathered = 0 --gameplaystat7

	local function resetInfo()
		exists 		= false
		playerName	= ''
		heroName	= ''
		heroIcon	= ''
		awardIndex	= { -1, -1 }
		score		= 0
		isMVP		= false
		isTeamMVP	= false
		-- team		= -1
		position	= -1

		goals			= 0
		goalAssists		= 0
		selfGoals		= 0
		steals			= 0
		timeWithBalls	= 0
		deaths			= 0
		kills			= 0
		runesGathered 	= 0
	end

	local function getPosition()
		return position
	end

	local function getIndex()
		return index
	end

	local function setAwardIndex(awardIndex, value)
		awardIndex[awardIndex] = value
	end

	local function getAwardIndex(awardIndex)
		return awardIndex[awardIndex]
	end

	local function getScore()
		return score
	end

	local function setIsTeamMVP(isTrue)
		isTeamMVP = isTrue
	end

	local function getIsTeamMVP()
		return isTeamMVP
	end

	local function setIsMVP(isTrue)
		isMVP = isTrue
	end

	local function getIsMVP()
		return isMVP
	end

	local function getExists()
		return exists
	end

	local function getTeam()
		return team
	end

	local function getHeroIcon()
		return heroIcon
	end

	local function getPlayerName()
		return playerName
	end

	local function getHeroName()
		return heroName
	end

	local function getGoals()
		return goals
	end

	local function getGoalAssists()
		return goalAssists
	end

	local function getSelfGoals()
		return selfGoals
	end

	local function getSteals()
		return steals
	end

	local function getTimeWithBalls()
		return timeWithBalls
	end

	local function getDeaths()
		return deaths
	end

	local function getKills()
		return kills
	end

	local function getRunesGathered()
		return runesGathered
	end

	local container	= GetWidget('soccerEndSplashSide'..team..'Player'..(index + 1))

	container:RegisterWatch('MatchInfoPlayer'..index, function(widget, ...)

		if AtoN(arg[2]) == team then

			resetInfo()

			local name = arg[1]

			if arg[28] == 'soccer' and name and type(name) == 'string' and string.len(name) > 0 then
				exists		= true
				playerName	= name
				-- local heroEntity = arg[21]
				heroName	= arg[20]
				heroIcon	= arg[11]
				-- team		= AtoN(arg[2])
				position	= arg[3]

				goals			= AtoN(arg[109])
				goalAssists		= AtoN(arg[110])
				selfGoals		= AtoN(arg[111])
				steals			= AtoN(arg[112])
				timeWithBalls	= AtoN(arg[113])
				deaths			= AtoN(arg[114])
				kills			= AtoN(arg[115])
				runesGathered	= AtoN(arg[116])

				score = calcScore(goals, steals, goalAssists, kills)
			end

			sortPlayers()
		end
	end)

	return {
		resetInfo		= resetInfo,
		getExists		= getExists,
		getAwardIndex	= getAwardIndex,
		setAwardIndex	= setAwardIndex,
		getTeam			= getTeam,
		getScore		= getScore,
		setIsMVP		= setIsMVP,
		getIsMVP		= getIsMVP,
		setIsTeamMVP	= setIsTeamMVP,
		getIsTeamMVP	= getIsTeamMVP,
		getHeroName		= getHeroName,
		getPlayerName	= getPlayerName,
		getHeroIcon		= getHeroIcon,
		getPosition		= getPosition,
		getIndex		= getIndex,
		getGoals		= getGoals,
		getGoalAssists	= getGoalAssists,
		getSelfGoals	= getSelfGoals,
		getSteals		= getSteals,
		getTimeWithBalls= getTimeWithBalls,
		getDeaths		= getDeaths,
		getKills		= getKills,
		getRunesGathered= getRunesGathered,
	}
end

local function soccerEndSplashRegister()

	local container		= GetWidget('soccerEndSplash')
	local closeButton	= GetWidget('soccerEndSplashClose')

	closeButton:SetCallback('onclick', function(widget)
		container:FadeOut(250)
	end)

	local playerByIndex = { {}, {} }
	local indexToSlot	= {}

	local lastWinningTeam = -1

	container:RegisterWatch('MatchInfoSummary', function(widget, ...)
		if (arg[26] == 'soccer') then
			lastWinningTeam = AtoN(arg[18])
		else
			-- reset players info saved by requesting the previous match stats
			for i=1,2,1 do
				for j=0,4,1 do
					playerByIndex[i][j].resetInfo()
				end
			end
		end
	end)

	local function sortPlayers()
		GetWidget('soccerEndSplash'):RegisterWatch('EndUpdate', function(widget)
			widget:UnregisterWatch('EndUpdate')
			local scoreList		= {}
			local scoresByTeam	= { {}, {} }

			for i=1,2,1 do
				for j=0,4,1 do
					playerByIndex[i][j].setIsMVP(false)
					playerByIndex[i][j].setIsTeamMVP(false)
					table.insert(scoreList, playerByIndex[i][j])
					table.insert(scoresByTeam[i], playerByIndex[i][j])
				end
			end

			table.sort(scoreList, function(a,b)
				return a.getScore() > b.getScore()
			end)

			for i=1,2,1 do
				table.sort(scoresByTeam[i], function(a,b)
					return a.getScore() > b.getScore()
				end)
				scoresByTeam[i][1].setIsTeamMVP(true)

				GetWidget('soccerSplashSide'..i..'ValueEffect'):SetVisible(lastWinningTeam == i)
				GetWidget('soccerSplashSide'..i..'WinnerModel'):SetVisible(lastWinningTeam == i)
				GetWidget('soccerSplashSide'..i..'WinnerGlow'):SetVisible(lastWinningTeam == i)
				GetWidget('soccerSplashSide'..i..'MVPModel'):SetVisible(false)
			end

			scoreList[1].setIsMVP(true)

			local wdgModel = GetWidget('soccerSplashSide'..lastWinningTeam..'MVPModel')
			if wdgModel ~= nil then
				wdgModel:SetVisible(true)
			end

			local goalsTotal = {}
			local selfGoalsTotal = {}
			for i=1,2,1 do
				goalsTotal[i] = 0
				selfGoalsTotal[i] = 0
				for j=0,4,1 do
					soccerEndSplashPopulatePlayer(i, j + 1, scoresByTeam[i][j + 1])
					goalsTotal[i] = goalsTotal[i] + scoresByTeam[i][j + 1].getGoals()
					selfGoalsTotal[i] = selfGoalsTotal[i] + scoresByTeam[i][j + 1].getSelfGoals()
				end
			end
			soccerSetNumberWidgets(goalsTotal[1]+selfGoalsTotal[2], GetWidget('soccerSplashSide1ValueTeam'))
			soccerSetNumberWidgets(goalsTotal[2]+selfGoalsTotal[1], GetWidget('soccerSplashSide2ValueTeam'))
		end)
	end

	for i=1,2,1 do
		for j=0,4,1 do
			playerByIndex[i][j] = soccerEndSplashRegisterPlayerIndex(j, i, sortPlayers)
		end
	end
end

soccerEndSplashRegister()