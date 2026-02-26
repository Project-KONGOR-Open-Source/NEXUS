-- ctf_splash stuff

local function setNumberWidgets(value, container, template)
	if value and type(value) == 'number' then
		template = template or 'largeNumberImage'
		value = tostring(value)
		local widgetList = container:GetChildren()
		local widgetCount = #widgetList

		local valueLen = string.len(value)

		if widgetCount < valueLen then
			for i=(widgetCount + 1), valueLen,1 do
				container:Instantiate(template)
			end
			widgetList = container:GetChildren()
			widgetCount = #widgetList
		end

		for i=1,widgetCount,1 do
			if i > valueLen then
				widgetList[i]:SetVisible(false)
			else
				widgetList[i]:GetChildren()[1]:SetTexture('/ui/elements/number_large_'..string.sub(value, i, i)..'.tga')
				widgetList[i]:SetVisible(true)
			end
		end

	end
end

--version with auto recalulation of the layout
local function setNumberWidgetsV2(value, container, template)
	if value and type(value) == 'number' then
		template = template or 'largeNumberImage'
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
				widgetList[i]:GetChildren()[1]:SetTexture('/ui/elements/number_large_'..string.sub(value, i, i)..'.tga')
				widgetList[i]:GetChildren()[1]:SetVisible(true)
			end
		end
	end
end

local function calcScore(kills, assists, captures, returns, carrierKills)
	return (
		kills +
		(assists * 0.5) +
		(captures * 15) +
		(returns * 5) +
		(carrierKills * 5)
	)
end

local function ctfEndSplashPopulatePlayer(side, index, playerInfo)
	local container	= GetWidget('ctfEndSplashSide'..side..'Player'..index)
	local flag		= GetWidget('ctfEndSplashSide'..side..'Player'..index..'Flag')
	if playerInfo and playerInfo.getExists() then

		GetWidget('ctfEndSplashSide'..side..'Player'..index):SetVisible(true)

		local MVPFrameColor = '#2a425c'

		if ((playerInfo.getIsMVP() and playerInfo.getTeam() == 1) or ((not playerInfo.getIsMVP()) and playerInfo.getTeam() == 2)) then
			MVPFrameColor = '#d79c34'
		end

		local mvpLabel = GetWidget('ctfEndSplashSide'..side..'Player'..index..'MVP')
		if mvpLabel then
			GetWidget('ctfEndSplashSide'..side..'Player'..index..'MVP'):SetVisible(playerInfo.getIsTeamMVP())
		end
		
		GetWidget('ctfEndSplashSide'..side..'Player'..index..'Name'):SetText(playerInfo.getPlayerName())
		GetWidget('ctfEndSplashSide'..side..'Player'..index):SetVisible(playerInfo.getExists())
		GetWidget('ctfEndSplashSide'..side..'Player'..index..'HeroIcon'):SetTexture(playerInfo.getHeroIcon())

		-- GetWidget('ctfEndSplashSide'..side..'Player'..index..'Frame'):SetColor(MVPFrameColor)
		-- GetWidget('ctfEndSplashSide'..side..'Player'..index..'Frame'):SetBorderColor(MVPFrameColor)

		GetWidget('ctfEndSplashSide'..side..'Player'..index..'Hero'):SetText(playerInfo.getHeroName())

		setNumberWidgetsV2(playerInfo.getCaptures(), GetWidget('ctfEndSplashSide'..side..'Player'..index..'Value'))

		local match_id		= matchStatsGetMatchID()
		if GetWidget('ctfEndSplashSide'..side..'Player'..index..'Award1Icon') then
				GetWidget('ctfEndSplashSide'..side..'Player'..index..'Award1Icon'):SetTexture('/ui/icons/awards/award_'..matchStatsGetAssignedAward(playerInfo.getPosition(), '1')..'.tga')
		end	
		if GetWidget('ctfEndSplashSide'..side..'Player'..index..'Award2Icon') then	
			GetWidget('ctfEndSplashSide'..side..'Player'..index..'Award2Icon'):SetTexture('/ui/icons/awards/award_'..matchStatsGetAssignedAward(playerInfo.getPosition(), '2')..'.tga')
		end

		GetWidget('ctfEndSplashSide'..side..'Player'..index..'Award1'):SetCallback('onmouseover', function(widget)
			-- CallEventParamsX('match_stats_simple_player_item_{team}_{index}', 0, 1);
			GetWidget('match_stats_simple_player_item_'..side..'_'..playerInfo.getIndex()):DoEventN(0, 1)
		end)
		GetWidget('ctfEndSplashSide'..side..'Player'..index..'Award1'):SetCallback('onmouseout', function(widget)
			-- CallEventParamsX('match_stats_simple_player_item_{team}_{index}', 1, 1);
			GetWidget('match_stats_simple_player_item_'..side..'_'..playerInfo.getIndex()):DoEventN(1, 1)
		end)
		GetWidget('ctfEndSplashSide'..side..'Player'..index..'Award2'):SetCallback('onmouseover', function(widget)
			-- CallEventParamsX('match_stats_simple_player_item_{team}_{index}', 0, 2);
			GetWidget('match_stats_simple_player_item_'..side..'_'..playerInfo.getIndex()):DoEventN(0, 2)
		end)
		GetWidget('ctfEndSplashSide'..side..'Player'..index..'Award2'):SetCallback('onmouseout', function(widget)
			-- CallEventParamsX('match_stats_simple_player_item_{team}_{index}', 1, 2);
			GetWidget('match_stats_simple_player_item_'..side..'_'..playerInfo.getIndex()):DoEventN(1, 2)
		end)

	else
		GetWidget('ctfEndSplashSide'..side..'Player'..index):SetVisible(false)
	end
end

local function ctfEndSplashRegisterPlayerIndex(index, team, sortPlayers)
	local exists		= false
	local captures		= 0
	local playerName	= ''
	local heroName		= ''
	local heroIcon		= ''
	local awardIndex	= { -1, -1 }

	local score			= 0
	local isMVP			= false
	local isTeamMVP		= false
	local position		= -1

	local function resetInfo()
		exists 		= false
		playerName	= ''
		captures	= 0
		heroName	= ''
		heroIcon	= ''
		awardIndex	= { -1, -1 }
		score		= 0
		isMVP		= false
		isTeamMVP	= false
		-- team		= -1
		position	= -1
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

	local function getCaptures()
		return captures
	end

	local container	= GetWidget('ctfEndSplashSide'..team..'Player'..(index + 1))

	container:RegisterWatch('MatchInfoPlayer'..index, function(widget, ...)


		if AtoN(arg[2]) == team then

			resetInfo()
			
			local name = arg[1]

			if arg[28] == 'capturetheflag' and name and type(name) == 'string' and string.len(name) > 0 then
				exists		= true
				playerName	= name
				clanTag		= arg[58]
				score		= calcScore(
					AtoN(arg[5]),		-- Kills
					AtoN(arg[19]),	-- Assists
					AtoN(arg[111]),	-- Captures
					AtoN(arg[112]),	-- Returns
					AtoN(arg[113])	-- Carrier kills
				)
				-- local heroEntity = arg[21]
				heroName	= arg[20]
				heroIcon	= arg[11]
				-- team		= AtoN(arg[2])
				captures	= AtoN(arg[111])
				position	= arg[3]
			end

			captures = AtoN(arg[111])

			sortPlayers()
		end

	end)

	return {
		resetInfo		= resetInfo,
		getExists		= getExists,
		getCaptures		= getCaptures,
		getName			= getName,
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

	}
end

local function ctfEndSplashRegister()

	local container		= GetWidget('ctfEndSplash')
	local closeButton	= GetWidget('ctfEndSplashClose')

	closeButton:SetCallback('onclick', function(widget)
		container:FadeOut(250)
	end)

	local playerByIndex = { {}, {} }
	local indexToSlot	= {}

	local lastWinningTeam = -1

	container:RegisterWatch('MatchInfoSummary', function(widget, ...)
		if (arg[26] == 'capturetheflag') then
			lastWinningTeam = AtoN(arg[18])
		else
			for i=1,2,1 do
				for j=0,4,1 do
					playerByIndex[i][j].resetInfo()
				end
			end
		end
	end)

	local function sortPlayers()
		GetWidget('ctfEndSplash'):RegisterWatch('EndUpdate', function(widget)
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

				GetWidget('ctfSplashSide'..i..'ValueEffect'):SetVisible(lastWinningTeam == i)
				GetWidget('ctfSplashSide'..i..'WinnerModel'):SetVisible(lastWinningTeam == i)
				GetWidget('ctfSplashSide'..i..'WinnerGlow'):SetVisible(lastWinningTeam == i)
				GetWidget('ctfSplashSide'..i..'MVPModel'):SetVisible(false)
			end

			scoreList[1].setIsMVP(true)

			GetWidget('ctfSplashSide'..scoreList[1].getTeam()..'MVPModel'):SetVisible(true)

			local capturesTotal
			for i=1,2,1 do
				capturesTotal = 0
				for j=0,4,1 do
					ctfEndSplashPopulatePlayer(i, j + 1, scoresByTeam[i][j + 1])
					capturesTotal = capturesTotal + scoresByTeam[i][j + 1].getCaptures()
				end
				setNumberWidgetsV2(capturesTotal, GetWidget('ctfSplashSide'..i..'ValueTeam'))
			end
		end)

	end
	
	for i=1,2,1 do
		for j=0,4,1 do
			playerByIndex[i][j] = ctfEndSplashRegisterPlayerIndex(j, i, sortPlayers)
		end
	end
end

ctfEndSplashRegister()