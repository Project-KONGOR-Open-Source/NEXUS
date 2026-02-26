----------------------------------------------------------
--	Name: 		Reward/Match Stats Script	            --
--  Copyright 2018 Frostburn Studios					--
----------------------------------------------------------

Set('_stats_nav_to_match_stats', 'false', 'bool')

local _G = getfenv(0)
Match_Stats = {}
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()
local matchStats = {}
local numPlayers = 0

matchStats.tMatchInfoPlayer = {}
matchStats.tAwardsPlayer = {}
matchStats.tAssignedAwardsPlayer = {}
matchStats.tAssignedAwardsPlayer['1'] = {}
matchStats.tAssignedAwardsPlayer['2'] = {}
matchStats.scrollState = 0
matchStats.totalCards = 0
matchStats.mmrTable = {}
matchStats.mmrLevel = 0
matchStats.accountTotalSilver = 0
matchStats.winningTeam = 0
matchStats.newlyEarnedRewards = nil
matchStats.MINIMUM_UPCOMING = 3
matchStats.MAXIMUM_ITEM_CARDS = 150
matchStats_showPopupIfGated = 0
matchStats.isAutoLookup = false
matchStats.hasRewardData = false
matchStats.hasMasteryData = false
matchStats.hasRankData = false
matchStats.debugNewRewards = false
matchStats.isanyStats = false
matchStats.isBots = false
matchStats.playerTokensEarned = 0
matchStats.playerMatchesUntilTokens = 0
matchStats_match_id = -1
Match_Stats_V2 = {}
matchStats_extraInfo = 0
matchStats_maxleveladdon = 0
matchStats_matchExp = 0
matchStats_bonusExp = 0
matchStats_BoostExp = 0
matchStats_SuperBoostExp = 0
matchStats_pieGraph = 0
matchStats_ToBoostExp = 0
matchStats_boostNum = 0
matchStats_SuperBoostNum = 0
matchStats_boostCostGold = 0
matchStats_boostCostSilver = 0
matchStats_superBoostCost = 0
matchStats_eventBonus = 0
matchStats_maxleveladdonPercent = 0
matchStats_maxlevelcount = 0
matchStats_UILevel = 0
matchStats_RewardsDisabled = 0

matchStats_starSelect = false
matchStats_starRating = 0

matchStats.no_stats = 2

matchStats.repeatCheck = false

RewardStat_MVPFrameSize = 0

matchStats_ReplayURLStatus = 0
matchStats_ReplayUnav = 0
matchStats_Compat1 = 0
matchStats_Compat2 = 0

matchStats.pendingReplays = {}

matchStats.mvpID = 0
matchStats.mvp_awd_mann = 0
matchStats.mvp_awd_mqk = 0
matchStats.mvp_awd_lgks = 0
matchStats.mvp_awd_msd = 0
matchStats.mvp_awd_mkill = 0
matchStats.mvp_awd_masst = 0
matchStats.mvp_awd_ledth = 0
matchStats.mvp_awd_mbdmg = 0
matchStats.mvp_awd_mvp = 0
matchStats.mvp_awd_mhdd = 0
matchStats.mvp_awd_hcs = 0

matchStats.isSoccer = false
matchStats.isCTF = false

local _match_id_postponed_to_fetch = ''

-- soccer_splash stuff start
local soccerPlayerByIndex = {}

local soccerLastWinningTeam = -1

local function soccerSetupPlayers()
	for i=1,2,1 do
		soccerPlayerByIndex[i] = {}
		for j=0,4,1 do
			soccerPlayerByIndex[i][j] = {}
			soccerPlayerByIndex[i][j]["exists"]			= false
			soccerPlayerByIndex[i][j]["playerName"]		= ''
			soccerPlayerByIndex[i][j]["heroName"]		= ''
			soccerPlayerByIndex[i][j]["heroIcon"]		= ''
			soccerPlayerByIndex[i][j]["awardIndex"]		= { -1, -1 }
			soccerPlayerByIndex[i][j]["score"]			= 0
			soccerPlayerByIndex[i][j]["isMVP"]			= false
			soccerPlayerByIndex[i][j]["isTeamMVP"]		= false
			soccerPlayerByIndex[i][j]["position"]		= -1
			soccerPlayerByIndex[i][j]["goals"]			= 0 --gameplaystat0
			soccerPlayerByIndex[i][j]["goalAssists"]	= 0 --gameplaystat1
			soccerPlayerByIndex[i][j]["selfGoals"]		= 0 --gameplaystat2
			soccerPlayerByIndex[i][j]["steals"]			= 0 --gameplaystat3
			soccerPlayerByIndex[i][j]["timeWithBalls"]	= 0 --gameplaystat4
			soccerPlayerByIndex[i][j]["deaths"]			= 0 --gameplaystat5
			soccerPlayerByIndex[i][j]["kills"]			= 0 --gameplaystat6
			soccerPlayerByIndex[i][j]["runesGathered"] 	= 0 --gameplaystat7
		end
	end
end

soccerSetupPlayers()

local function calcScore(paramGoals, paramSteals, paramAssists, paramKills)
	return (
		(paramGoals 	* 	10)	+
		(paramSteals 	* 	1) 	+
		(paramAssists	* 	5) 	+
		(paramKills 	* 	2)
	)
end

local function matchStatsGetAssignedAward(position, awardID)	-- both strings
	return matchStats.tAssignedAwardsPlayer[awardID][position]
end

local function soccerEndSplashPopulatePlayer(side, index, playerInfo)
	
--	Echo('side = '..side..', index = '..index..', playerInfo.position = '..playerInfo.position)
--	if matchStatsGetAssignedAward(tostring(playerInfo.position), '1') ~= nil then
--		Echo('award1 = '..matchStatsGetAssignedAward(tostring(playerInfo.position), '1'))
--	end
--	if matchStatsGetAssignedAward(tostring(playerInfo.position), '2') ~= nil then
--		Echo('award2 = '..matchStatsGetAssignedAward(tostring(playerInfo.position), '2'))
--	end
	
	if playerInfo.exists and playerInfo.isTeamMVP then
		local postfix = (side == 1) and 'blue' or 'yellow'
		GetWidget('rewardstats_soccer_mvp_playername_'..postfix):SetText(playerInfo.playerName)
		GetWidget('rewardstats_soccer_mvp_goals_'..postfix):SetText(tostring(math.floor(tonumber(playerInfo.goals))))
		GetWidget('rewardstats_soccer_mvp_heroname_'..postfix):SetText(playerInfo.heroName)
		GetWidget('rewardstats_soccer_mvp_heroicon_'..postfix):SetTexture(playerInfo.heroIcon)
		GetWidget('rewardstats_soccer_mvp_award1_'..postfix):SetTexture('/ui/icons/awards/award_'..matchStatsGetAssignedAward(tostring(playerInfo.position), '1')..'.tga')
		GetWidget('rewardstats_soccer_mvp_award1_'..postfix):SetCallback('onmouseover', 
			function(widget)
				local title = Translate('match_awards_'..matchStatsGetAssignedAward(tostring(playerInfo.position), '1'))
				local desc = Translate('match_awards_'..matchStatsGetAssignedAward(tostring(playerInfo.position), '1')..'_desc')
				Common_V2:ShowGenericTip(true, title, desc, '300i')
			end)	
		GetWidget('rewardstats_soccer_mvp_award1_'..postfix):SetCallback('onmouseout', 
			function(widget)
				Common_V2:ShowGenericTip(false)
			end)	
		GetWidget('rewardstats_soccer_mvp_award2_'..postfix):SetTexture('/ui/icons/awards/award_'..matchStatsGetAssignedAward(tostring(playerInfo.position), '2')..'.tga')
		GetWidget('rewardstats_soccer_mvp_award2_'..postfix):SetCallback('onmouseover', 
			function(widget)
				local title = Translate('match_awards_'..matchStatsGetAssignedAward(tostring(playerInfo.position), '2'))
				local desc = Translate('match_awards_'..matchStatsGetAssignedAward(tostring(playerInfo.position), '2')..'_desc')
				Common_V2:ShowGenericTip(true, title, desc, '300i')
			end)	
		GetWidget('rewardstats_soccer_mvp_award2_'..postfix):SetCallback('onmouseout', 
			function(widget)
				Common_V2:ShowGenericTip(false)
			end)	
	else
		if playerInfo.exists then
			GetWidget('rewardstat_soccer_otherplayerinstance_'..side..'_'..index):SetVisible(1)
			GetWidget('rewardstat_soccer_matename_'..side..'_'..index):SetText(playerInfo.playerName)
			GetWidget('rewardstat_soccer_mategoals_'..side..'_'..index):SetText(tostring(math.floor(tonumber(playerInfo.goals))))
			GetWidget('rewardstat_soccer_matehero_'..side..'_'..index):SetText(playerInfo.heroName)
			GetWidget('rewardstat_soccer_mateheroicon_'..side..'_'..index):SetTexture(playerInfo.heroIcon)
			GetWidget('rewardstat_soccer_mateaward1_'..side..'_'..index):SetTexture('/ui/icons/awards/award_'..matchStatsGetAssignedAward(tostring(playerInfo.position), '1')..'.tga')
			GetWidget('rewardstat_soccer_mateaward1_'..side..'_'..index):SetCallback('onmouseover', 
				function(widget)
					local title = Translate('match_awards_'..matchStatsGetAssignedAward(tostring(playerInfo.position), '1'))
					local desc = Translate('match_awards_'..matchStatsGetAssignedAward(tostring(playerInfo.position), '1')..'_desc')
					Common_V2:ShowGenericTip(true, title, desc, '300i')
				end)	
			GetWidget('rewardstat_soccer_mateaward1_'..side..'_'..index):SetCallback('onmouseout', 
				function(widget)
					Common_V2:ShowGenericTip(false)
				end)	
			GetWidget('rewardstat_soccer_mateaward2_'..side..'_'..index):SetTexture('/ui/icons/awards/award_'..matchStatsGetAssignedAward(tostring(playerInfo.position), '2')..'.tga')
			GetWidget('rewardstat_soccer_mateaward2_'..side..'_'..index):SetCallback('onmouseover', 
				function(widget)
					local title = Translate('match_awards_'..matchStatsGetAssignedAward(tostring(playerInfo.position), '2'))
					local desc = Translate('match_awards_'..matchStatsGetAssignedAward(tostring(playerInfo.position), '2')..'_desc')
					Common_V2:ShowGenericTip(true, title, desc, '300i')
				end)	
			GetWidget('rewardstat_soccer_mateaward2_'..side..'_'..index):SetCallback('onmouseout', 
				function(widget)
					Common_V2:ShowGenericTip(false)
				end)	
		else
			GetWidget('rewardstat_soccer_otherplayerinstance_'..side..'_'..index):SetVisible(0)
		end
	end

end

local function OnSoccerMatchInfoPlayer(index, name, team, pos, heroName, heroIcon, goals, goalAssists, selfGoals, steals, timeWithBalls, deaths, kills, runesGathered)
	if NotEmpty(name) then
		soccerPlayerByIndex[team][index].exists		= true
		soccerPlayerByIndex[team][index].playerName	= name
		-- local heroEntity = arg[21]
		soccerPlayerByIndex[team][index].heroName	= heroName
		soccerPlayerByIndex[team][index].heroIcon	= heroIcon
		-- team		= tostring(arg[2])
		soccerPlayerByIndex[team][index].position	= pos

		soccerPlayerByIndex[team][index].goals			= tostring(goals)
		soccerPlayerByIndex[team][index].goalAssists	= tostring(goalAssists)
		soccerPlayerByIndex[team][index].selfGoals		= tostring(selfGoals)
		soccerPlayerByIndex[team][index].steals			= tostring(steals)
		soccerPlayerByIndex[team][index].timeWithBalls	= tostring(timeWithBalls)
		soccerPlayerByIndex[team][index].deaths			= tostring(deaths)
		soccerPlayerByIndex[team][index].kills			= tostring(kills)
		soccerPlayerByIndex[team][index].runesGathered	= tostring(runesGathered)

		soccerPlayerByIndex[team][index].score = calcScore(goals, steals, goalAssists, kills)
	end
end

local function OnSoccerMatchInfoSummary(mapname, winner)
	if (mapname == 'soccer') then
		soccerLastWinningTeam = tonumber(winner)
--		Echo('soccerLastWinningTeam = '..winner)
	else
		-- reset players info saved by requesting the previous match stats
		for i=1,2,1 do
			for j=0,4,1 do
				soccerPlayerByIndex[i][j] = {}
			end
		end
	end
end

local function OnEndUpdate(widget)
	
	if not matchStats.isSoccer then return end
	
--	Echo('EndUpdate received!')
	
	widget:UnregisterWatch('EndUpdate')
	local scoreList		= {}
	local scoresByTeam	= { {}, {} }

	for i=1,2,1 do
		for j=0,4,1 do
			soccerPlayerByIndex[i][j].isMVP = false
			soccerPlayerByIndex[i][j].isTeamMVP = false
			table.insert(scoreList, soccerPlayerByIndex[i][j])
			table.insert(scoresByTeam[i], soccerPlayerByIndex[i][j])
			Echo('player = '..soccerPlayerByIndex[i][j].playerName..', goals = '..soccerPlayerByIndex[i][j].goals)
		end
	end

	table.sort(scoreList, function(a,b)
		return a.score > b.score
	end)

	for i=1,2,1 do
		table.sort(scoresByTeam[i], function(a,b)
			return a.score > b.score
		end)
		scoresByTeam[i][1].isTeamMVP = true
	end

	scoreList[1].isMVP = true

	local goalsTotal = {}
	local selfGoalsTotal = {}
	for i=1,2,1 do
		goalsTotal[i] = 0
		selfGoalsTotal[i] = 0
		for j=0,4,1 do
			soccerEndSplashPopulatePlayer(i, j + 1, scoresByTeam[i][j + 1])
			goalsTotal[i] = goalsTotal[i] + scoresByTeam[i][j + 1].goals
			selfGoalsTotal[i] = selfGoalsTotal[i] + scoresByTeam[i][j + 1].selfGoals
		end
	end
	
--	Echo('teamgoal_blue = '..tostring(goalsTotal[1]+selfGoalsTotal[2]))
--	Echo('teamgoal_yellow = '..tostring(goalsTotal[2]+selfGoalsTotal[1]))
	
	GetWidget('rewardstats_soccer_teamgoal_blue'):SetText(tostring(goalsTotal[1]+selfGoalsTotal[2]))
	GetWidget('rewardstats_soccer_teamgoal_yellow'):SetText(tostring(goalsTotal[2]+selfGoalsTotal[1]))
end

-- soccer_splash stuff end

function RewardStat_OnEntityDefinitionsLoaded()
	Echo('^gLoading entity definitions finished! match_stats.isVisible = '..tostring(GetWidget('match_stats'):IsVisible())..'^*')

	GetWidget('rewardstats_loadingentity_mask'):SetVisible(false)

	SubmitForm('PlayerRecentGamesList', 'f', 'grab_last_matches_from_nick', 'nickname', StripClanTag(GetAccountName()), 'hosttime', HostTime())	

	if NotEmpty(_match_id_postponed_to_fetch) and GetWidget('match_stats'):IsVisible() then
		GetWidget('match_stats'):Sleep(1,
			function ()
				interface:UICmd("GetMatchInfo(".._match_id_postponed_to_fetch..")")
				Set('_stats_last_replay_id', match_id)
				GetWidget('endgame_stats_waiting_mask'):FadeIn(150)
				_match_id_postponed_to_fetch = ''
			end
		)
	end
end

interface:RegisterWatch('PlayerRecentGamesListResult', function(widget, ...)
	local matchList = Explode(',', arg[1])
	Echo('^g arg1 = '..arg[1]..'^*')
	Echo('^g matchList0 = '..matchList[1]..'^*')
	if NotEmpty(matchList[1]) then
		Set('_stats_latest_match_played', matchList[1])
	end
end)

--Empty Blanket Fix
local function EmptyCheck(param)
	if (NotEmpty(param)) then
		return param
	else
		return '0'
	end
end

local function GetClanTag(stringVar)
	if (string.find(stringVar, ']')) then
		local tag = string.sub(stringVar, 2, string.find(stringVar, ']') - 1)
		return tag
	end	
	return ''
end

local function GetPlayerName(stringVar)
	if (string.find(stringVar, ']')) then
		local name = string.sub(stringVar, string.find(stringVar, ']') + 1)
		return name
	end	
	return stringVar
end

function CutMatchID(stringVar)
	if (string.find(stringVar, '#')) then
		local name = string.sub(stringVar, string.find(stringVar, '#') + 1)
		return name
	end	

	return stringVar
end

function rewardstatsResetWidgets()
	--Placement Empty Icons
	GetWidget('rewardstat_placementsico_1'):SetTexture('/ui/fe2/NewUI/Res/match_stats/1th.png')
	GetWidget('rewardstat_placementsico_2'):SetTexture('/ui/fe2/NewUI/Res/match_stats/2th.png')
	GetWidget('rewardstat_placementsico_3'):SetTexture('/ui/fe2/NewUI/Res/match_stats/3th.png')
	GetWidget('rewardstat_placementsico_4'):SetTexture('/ui/fe2/NewUI/Res/match_stats/4th.png')
	GetWidget('rewardstat_placementsico_5'):SetTexture('/ui/fe2/NewUI/Res/match_stats/5th.png')
	GetWidget('rewardstat_placementsico_6'):SetTexture('/ui/fe2/NewUI/Res/match_stats/6th.png')
	
	--Placement Icon Colors
	GetWidget('rewardstat_placementsico_1'):SetColor('1 1 1')
	GetWidget('rewardstat_placementsico_2'):SetColor('1 1 1')
	GetWidget('rewardstat_placementsico_3'):SetColor('1 1 1')
	GetWidget('rewardstat_placementsico_4'):SetColor('1 1 1')
	GetWidget('rewardstat_placementsico_5'):SetColor('1 1 1')
	GetWidget('rewardstat_placementsico_6'):SetColor('1 1 1')
	
	-- Placement Pie Values
	GetWidget('rewardstat_placementspie_2'):SetColor('.2 .2 .2')
	GetWidget('rewardstat_placementspie_3'):SetColor('.2 .2 .2')
	GetWidget('rewardstat_placementspie_4'):SetColor('.2 .2 .2')
	GetWidget('rewardstat_placementspie_5'):SetColor('.2 .2 .2')
	GetWidget('rewardstat_placementspie_6'):SetColor('.2 .2 .2')
end

matchStats.milestonesTable = {
	{STEP 				= 50, EXPERIENCE_REWARD = 200, SILVER_REWARD = 10,},
	{STEP 				= 250, EXPERIENCE_REWARD = 100, SILVER_REWARD = 5,},
	{STEP 				= 250, EXPERIENCE_REWARD = 100, SILVER_REWARD = 5,},
	{STEP 				= 50, EXPERIENCE_REWARD = 100, SILVER_REWARD = 5,},
	{STEP 				= 10, EXPERIENCE_REWARD = 50, SILVER_REWARD = 1,},
}

local function MatchMilestones(_, matchMilestonesData)
	--printdb('^r^: MatchMilestones')
	--printdb('^r^: matchMilestonesData = ' .. tostring(matchMilestonesData) )

	if NotEmpty(matchMilestonesData) then
		matchStats.milestonesTable = {}
		local itemTable

		for index, itemString in pairs(explode('`', matchMilestonesData)) do
			itemTable = explode('|', itemString)
			tinsert(matchStats.milestonesTable, {
				EXPERIENCE_REWARD 	= itemTable[1],
				SILVER_REWARD 		= itemTable[2],
				STEP 				= itemTable[3],
			})
		end
		
		for index, rewardTable in pairs(matchStats.milestonesTable) do
			printdb('matchStats.milestonesTable: '..index)
			printTable(rewardTable)
		end
		
	end
end
interface:RegisterWatch('MatchMilestones', function(...) MatchMilestones(...) end)

function RewardStat_ExtraEarned(type)

	local function openExtraEarned()
		GetWidget('rewardstat_total_frame'):SetHeight('255%')
		GetWidget('rewardstat_total_extra_panel'):SetVisible(1)
		GetWidget('rewardstat_total_frame_darkbg_extra'):SetVisible(1)
		matchStats_extraInfo = 1
	end
	
	local function closeExtraEarned()
		GetWidget('rewardstat_total_frame'):SetHeight('100%')
		GetWidget('rewardstat_total_extra_panel'):SetVisible(0)
		GetWidget('rewardstat_total_frame_darkbg_extra'):SetVisible(0)
		matchStats_extraInfo = 0
	end
	
	if type == 'toggle' then
		if matchStats_extraInfo == 0 then
			openExtraEarned()
		else
			closeExtraEarned()
		end
	elseif type == 'close' then
		closeExtraEarned()
	elseif type == 'open' then
		openExtraEarned()
	else
		Echo('^rInvalid calls to RewardStat_ExtraEarned()!^*')
		return
	end
	
	if matchStats_extraInfo == 1 then
		GetWidget('match_stats_rewards_normal_expand_flash'):SetVFlip(1)
		GetWidget('match_stats_rewards_normal_expand_flash_over'):SetVFlip(1)
		GetWidget('match_stats_rewards_normal_expand_down'):SetVFlip(1)
		GetWidget('match_stats_rewards_normal_expand_flash'):SetColor('1 1 1 1')
	elseif matchStats_extraInfo == 0 then
		GetWidget('match_stats_rewards_normal_expand_flash'):SetVFlip(0)
		GetWidget('match_stats_rewards_normal_expand_flash_over'):SetVFlip(0)
		GetWidget('match_stats_rewards_normal_expand_down'):SetVFlip(0)
		local value = (GetTime() % 2500) / 2500
		local alpha = math.abs(math.sin(math.pi * 2 * value))
		GetWidget('match_stats_rewards_normal_expand_flash'):SetColor('1 1 1 '..tostring(alpha))
	end
	
end

function RewardStats_OnMasteryFrame()
	println('^c[MatchStats] ^*Mastery Frame Triggered')
	if GetWidget('rewardstat_operation_mask'):IsVisible() then
		local timeout = GetCvarInt('cg_masteryTimeout')
		local elapse = GetTime() - Match_Stats_MasteryInfo_masktimer
		if elapse > timeout then
			GetWidget('rewardstat_operation_mask'):SetVisible(false)
		end
	end
end

function RewardStat_MasteryXPAnimation(heroIcon, mastery_exp_original, mastery_exp_match, mastery_maxlevel_addon, mastery_exp_boost, mastery_exp_to_boost, mastery_canboost, mastery_super_canboost, mastery_boostnum, mastery_matchid, mastery_boost_product_id, mastery_boost_gold_coin, mastery_boost_silver_coin, mastery_exp_super_boost, mastery_super_boostnum, mastery_exp_event)
	
	GetWidget('rewardstat_masteryheroicon'):SetTexture(heroIcon)
	
	local mastery_boostnum = tonumber(mastery_boostnum)
	local mastery_super_boostnum = tonumber(mastery_super_boostnum)
	
	matchStats_boostNum = tonumber(mastery_boostnum)
	matchStats_SuperBoostNum = tonumber(mastery_super_boostnum)
	
	matchStats_boostCostGold = tonumber(mastery_boost_gold_coin)
	matchStats_boostCostSilver = tonumber(mastery_boost_silver_coin)
	
	GetWidget('rewardstat_boostbutton_regular'):SetEnabled(0)
	GetWidget('rewardstat_boostbutton_super'):SetEnabled(0)
	
	newXPVal = mastery_exp_original + mastery_exp_match + mastery_exp_boost + mastery_maxlevel_addon + mastery_exp_super_boost + mastery_exp_event
	CurXPVal = mastery_exp_original
	local min, max = GetMasteryExpByLevel(GetMasteryLevelByExp(newXPVal))
	masteryLevelVar = GetMasteryExpByLevel(GetMasteryLevelByExp(CurXPVal))
	GetWidget('rewardstat_label_heromastery_nextrank'):SetVisible(1)
	
	local function LoopAnim()
		GetWidget('rewardstat_masterypiegraph'):Sleep(10, function() CurXPVal = CurXPVal+8 --Change this value to increase number increment, and sleep for how often to process increment.
			if CurXPVal <= newXPVal then
				local min, max = GetMasteryExpByLevel(GetMasteryLevelByExp(CurXPVal))
				local function masteryResult()
					if GetMasteryLevelByExp(CurXPVal) ~= GetMasteryLevelByExp(newXPVal) then
						return 1.0
					else
						return tonumber(mastery_exp_original+mastery_exp_match+mastery_exp_to_boost+mastery_maxlevel_addon+mastery_exp_super_boost - min) / tonumber(max - min)
					end
				end
				GetWidget('rewardstat_masterypiegraph_masteryboost'):SetValue(masteryResult())
				GetWidget('rewardstat_label_mastery_exp'):SetText(Translate('mastery_replay_main_exp') .. CurXPVal - min .. '/' .. max - min)			
				local valuePercent = tonumber(CurXPVal - min) / tonumber(max - min)		
				GetWidget('rewardstat_masterypiegraph'):SetValue(valuePercent)		
				
				local masteryboostPie = GetWidget('rewardstat_masterypiegraph_masteryboost') -- :SetColor('#41281c')
				local heroMasteryPie = GetWidget('rewardstat_masterypiegraph') -- :SetColor('#ac7258')
				
				if GetMasteryLevelByExp(CurXPVal) >= 15 then
					masteryboostPie:SetColor('#581856')
					heroMasteryPie:SetColor('#aa1fd2')
				elseif GetMasteryLevelByExp(CurXPVal) >= 10 then
					masteryboostPie:SetColor('#6a3d01')
					heroMasteryPie:SetColor('#d69200')
				elseif GetMasteryLevelByExp(CurXPVal) >= 3 then
					masteryboostPie:SetColor('#444747')
					heroMasteryPie:SetColor('#b7c9c9')
				else
					masteryboostPie:SetColor('#593726')
					heroMasteryPie:SetColor('#ac7258')
				end
				
				local function IsMaxMasteryLevel()
					if GetMasteryLevelByExp(CurXPVal) == 15 then
						return Translate('15')
					else
						return GetMasteryLevelByExp(CurXPVal)
					end
				end
				GetWidget('rewardstat_label_mastery_level'):SetText(IsMaxMasteryLevel())
				
				if GetMasteryLevelByExp(CurXPVal) == 15 then
					GetWidget('rewardstat_masterypiegraph'):SetValue(1.0)
					GetWidget('rewardstat_label_mastery_exp'):SetText(Translate('mastery_replay_main_exp') .. 3500 .. '/' .. 3500) --Preset numbers for max level of 15
					GetWidget('rewardstat_label_heromastery_nextrank'):SetVisible(0)
					return
				end
				
				local function rewardVar(mastLevel)
					if mastLevel == 15 then
						return 15
					else
						return mastLevel+1
					end
				
				end
				
				GetWidget('rewardstat_label_heromastery_nextrank'):SetTexture('/ui/fe2/mastery/reward_level_'..(rewardVar(IsMaxMasteryLevel()))..'.tga')
				GetWidget('rewardstat_nextlevelaward_panel'):SetVisible(1)
				
				matchStats_UILevel = rewardVar(IsMaxMasteryLevel())
				
				if masteryLevelVar ~= GetMasteryLevelByExp(CurXPVal) then
					masteryLevelVar = GetMasteryLevelByExp(CurXPVal)
					GetWidget('rewardstat_heromastery_info_levelup_effect'):SetEffect('/ui/fe2/NewUI/Res/match_stats/effects/player_lvl_award_up.effect')
					GetWidget('rewardstat_heromastery_visual_levelup_effect'):SetEffect('/ui/fe2/NewUI/Res/match_stats/effects/player_hero_up.effect')
				end
				
				LoopAnim()
			else

				if tostring(mastery_canboost) == 'true' then
					GetWidget('rewardstat_boostbutton_regular'):SetEnabled(1)
				else
					GetWidget('rewardstat_boostbutton_regular'):SetEnabled(0)
				end
				
				if tostring(mastery_super_canboost) == 'true' and mastery_super_boostnum >= 1 then
					GetWidget('rewardstat_boostbutton_super'):SetEnabled(1)
					--GetWidget('rewardstat_boostbutton_super'):SetVisible(1)
					--GetWidget('rewardstat_label_mastery_superboosts_labels'):SetVisible(1)
				else
					GetWidget('rewardstat_boostbutton_super'):SetEnabled(0)
					--GetWidget('rewardstat_boostbutton_super'):SetVisible(0)
					--GetWidget('rewardstat_label_mastery_superboosts_labels'):SetVisible(0)
				end
			
			end
		end)
		
	end
	
	LoopAnim()
	
end

function RewardStat_SuperBoostMouseOverUI(param)

	local boostMulti = tonumber(matchStats_BoostExp + matchStats_ToBoostExp * 9)
	
	if param == 1 then
		GetWidget('rewardstat_label_heromastery_boostexp'):SetText(boostMulti)
		matchStats_pieGraph = GetWidget('rewardstat_masterypiegraph_masteryboost'):GetValue()
		GetWidget('rewardstat_masterypiegraph_masteryboost'):SetValue(1.0)
	else
		GetWidget('rewardstat_label_heromastery_boostexp'):SetText(matchStats_BoostExp + matchStats_SuperBoostExp)
		GetWidget('rewardstat_masterypiegraph_masteryboost'):SetValue(matchStats_pieGraph)
	end
	
end

function RewardStat_RegularBoostMouseOverUI(param)
	
	if param == 1 then
		GetWidget('rewardstat_label_heromastery_boostexp'):SetText(matchStats_SuperBoostExp + matchStats_ToBoostExp)
	else
		GetWidget('rewardstat_label_heromastery_boostexp'):SetText(matchStats_BoostExp + matchStats_SuperBoostExp)
	end
	
end


function MVPAwardTest()
	
	GetWidget('rewardstat_frame_mvprewards'):SetVisible(1)
	GetWidget('rewardstat_frame_mvprewards'):GetChildren()
	local rewardWidgets = GetWidget('rewardstat_frame_mvprewards'):GetChildren()
	local rewardWNum = #rewardWidgets
	for i=1, rewardWNum, 1 do
		rewardWidgets[i]:SetVisible(false)
		rewardWidgets[i]:Destroy()
	end
	
	interface:Sleep(1, function()
		GetWidget('rewardstat_frame_mvprewards'):Instantiate('rewardstat_mvp_award', 'content', Translate('award_mann'), 'id', 'mvp_awd_mann', 'data',  '1', 'icon', '/ui/fe2/newui/Res/playerstats/mvpawards/awd_mann_big.png')
		GetWidget('rewardstat_frame_mvprewards'):Instantiate('rewardstat_mvp_award', 'content', Translate('award_mqk'), 'id', 'mvp_awd_mqk', 'data', '1', 'icon', '/ui/fe2/newui/Res/playerstats/mvpawards/awd_mqk_big.png')
		GetWidget('rewardstat_frame_mvprewards'):Instantiate('rewardstat_mvp_award', 'content', Translate('award_lgks'), 'id', 'mvp_awd_lgks', 'data', 'Ultimate Warrior', 'icon', '/ui/fe2/newui/Res/playerstats/mvpawards/awd_lgks_big.png')
	end)
end

function SetRewardLayout(layoutParam)
	
	--Echo('^glayoutParam ='..layoutParam..'^*')
	if layoutParam == 'masteryonly' then
		GetWidget('rewardstat_heromastery_visual_panel'):SetVisible(1)
		GetWidget('rewardstat_heromastery_info_panel'):SetVisible(1)
		GetWidget('rewardstat_rankprogression_visual_panel'):SetX('5.2h')
		GetWidget('rewardstat_rankprogression_info_panel'):SetX('5.2h')
		GetWidget('rewardstat_rankprogression_visual_panel'):SetVisible(0)
		GetWidget('rewardstat_rankprogression_info_panel'):SetVisible(0)
		GetWidget('rewardstat_heromastery_visual_panel'):SetX('22h')
		GetWidget('rewardstat_heromastery_info_panel'):SetX('22h')
		GetWidget('rewardstat_total_frame'):SetHeight('100%')
		GetWidget('rewardstat_border2'):SetVisible(1)
		GetWidget('rewardstat_match_gameday'):SetVisible(1)
		GetWidget('rewardstat_social_consec'):SetVisible(1)
		GetWidget('rewardstat_extraearnedpanel'):SetVisible(1)
		GetWidget('rewardstat_heromastery_visual_panel'):SetY('0.8h')
		RewardStat_ExtraEarned('close')
	elseif layoutParam == 'con' then
		GetWidget('rewardstat_rankprogression_visual_panel'):SetVisible(1)
		GetWidget('rewardstat_rankprogression_info_panel'):SetVisible(1)
		GetWidget('rewardstat_rankprogression_visual_panel'):SetX('5.2h')
		GetWidget('rewardstat_rankprogression_info_panel'):SetX('5.2h')
		GetWidget('rewardstat_heromastery_visual_panel'):SetVisible(1)
		GetWidget('rewardstat_heromastery_info_panel'):SetVisible(1)
		GetWidget('rewardstat_heromastery_visual_panel'):SetX('38.7h')
		GetWidget('rewardstat_heromastery_info_panel'):SetX('38.7h')
		GetWidget('rewardstat_total_frame'):SetHeight('100%')
		GetWidget('rewardstat_border2'):SetVisible(1)
		GetWidget('rewardstat_match_gameday'):SetVisible(1)
		GetWidget('rewardstat_social_consec'):SetVisible(1)
		GetWidget('rewardstat_extraearnedpanel'):SetVisible(1)
		GetWidget('rewardstat_heromastery_visual_panel'):SetY('0.8h')
		RewardStat_ExtraEarned('close')
	elseif layoutParam == 'rankonly' then
		GetWidget('rewardstat_rankprogression_visual_panel'):SetVisible(1)
		GetWidget('rewardstat_rankprogression_info_panel'):SetVisible(1)
		GetWidget('rewardstat_rankprogression_visual_panel'):SetX('21.2h')
		GetWidget('rewardstat_rankprogression_info_panel'):SetX('22.0h')
		GetWidget('rewardstat_heromastery_visual_panel'):SetVisible(0)
		GetWidget('rewardstat_heromastery_info_panel'):SetVisible(0)
		GetWidget('rewardstat_heromastery_visual_panel'):SetX('38.7h')
		GetWidget('rewardstat_heromastery_info_panel'):SetX('38.7h')
		GetWidget('rewardstat_total_frame'):SetHeight('100%')
		GetWidget('rewardstat_border2'):SetVisible(1)
		GetWidget('rewardstat_match_gameday'):SetVisible(1)
		GetWidget('rewardstat_social_consec'):SetVisible(1)
		GetWidget('rewardstat_extraearnedpanel'):SetVisible(1)
		GetWidget('rewardstat_heromastery_visual_panel'):SetY('0.8h')
		RewardStat_ExtraEarned('close')
	elseif layoutParam == 'nostats' then
		GetWidget('rewardstat_rankprogression_visual_panel'):SetVisible(0)
		GetWidget('rewardstat_rankprogression_info_panel'):SetVisible(0)
		GetWidget('rewardstat_rankprogression_visual_panel'):SetX('5.2h')
		GetWidget('rewardstat_rankprogression_info_panel'):SetX('5.2h')
		GetWidget('rewardstat_heromastery_visual_panel'):SetVisible(0)
		GetWidget('rewardstat_heromastery_info_panel'):SetVisible(0)
		GetWidget('rewardstat_total_frame'):SetHeight('430%')
		GetWidget('rewardstat_border2'):SetVisible(0)
		GetWidget('rewardstat_match_gameday'):SetVisible(0)
		GetWidget('rewardstat_social_consec'):SetVisible(0)
		GetWidget('rewardstat_extraearnedpanel'):SetVisible(0)
		GetWidget('rewardstat_heromastery_visual_panel'):SetY('0.8h')
		RewardStat_ExtraEarned('close')
	elseif layoutParam == 'onlymastery' then
		GetWidget('rewardstat_heromastery_visual_panel'):SetVisible(1)
		GetWidget('rewardstat_heromastery_info_panel'):SetVisible(1)
		GetWidget('rewardstat_rankprogression_visual_panel'):SetX('5.2h')
		GetWidget('rewardstat_rankprogression_info_panel'):SetX('5.2h')
		GetWidget('rewardstat_rankprogression_visual_panel'):SetVisible(0)
		GetWidget('rewardstat_rankprogression_info_panel'):SetVisible(0)
		GetWidget('rewardstat_heromastery_visual_panel'):SetX('22h')
		GetWidget('rewardstat_heromastery_info_panel'):SetX('22h')
		GetWidget('rewardstat_total_frame'):SetHeight('36%')
		GetWidget('rewardstat_border2'):SetVisible(0)
		GetWidget('rewardstat_match_gameday'):SetVisible(0)
		GetWidget('rewardstat_social_consec'):SetVisible(0)
		GetWidget('rewardstat_extraearnedpanel'):SetVisible(0)
		GetWidget('rewardstat_heromastery_visual_panel'):SetY('0.8h')
		RewardStat_ExtraEarned('close')
	elseif layoutParam == 'coinsonly' then
		GetWidget('rewardstat_rankprogression_visual_panel'):SetVisible(0)
		GetWidget('rewardstat_rankprogression_info_panel'):SetVisible(0)
		GetWidget('rewardstat_rankprogression_visual_panel'):SetX('5.2h')
		GetWidget('rewardstat_rankprogression_info_panel'):SetX('5.2h')
		GetWidget('rewardstat_heromastery_visual_panel'):SetVisible(0)
		GetWidget('rewardstat_heromastery_info_panel'):SetVisible(0)
		GetWidget('rewardstat_heromastery_visual_panel'):SetX('38.7h')
		GetWidget('rewardstat_heromastery_info_panel'):SetX('38.7h')
		GetWidget('rewardstat_total_frame'):SetHeight('36%')
		GetWidget('rewardstat_border2'):SetVisible(0)
		GetWidget('rewardstat_match_gameday'):SetVisible(0)
		GetWidget('rewardstat_social_consec'):SetVisible(0)
		GetWidget('rewardstat_extraearnedpanel'):SetVisible(0)
		GetWidget('rewardstat_heromastery_visual_panel'):SetY('0.8h')
		RewardStat_ExtraEarned('close')
	else
		--Nothing!
	end
	
end

function RewardStat_CalculateKillStreak(ks3, ks4, ks5, ks6, ks7, ks8, ks9, ks10, ks15)
	
	if tonumber(ks15) == 1 then
		return(Translate('player_stats_immortal'))
	elseif tonumber(ks10) == 1 then
		return(Translate('player_stats_bloodbath'))
	elseif tonumber(ks9) == 1 then
		return(Translate('player_stats_champion'))
	elseif tonumber(ks8) == 1 then
		return(Translate('player_stats_dominating'))
	elseif tonumber(ks7) == 1 then
		return(Translate('player_stats_savagesick'))
	elseif tonumber(ks6) == 1 then
		return(Translate('player_stats_onslaught'))
	elseif tonumber(ks5) == 1 then
		return(Translate('player_stats_legndary'))
	elseif tonumber(ks4) == 1 then
		return(Translate('player_stats_ultimatewarrior'))
	elseif tonumber(ks3) == 1 then
		return(Translate('player_stats_serialkiller'))
	else
		return('1-3 Kills')
	end
	
end

local function AnimateProgressBar(bar_widget, start, target, step, interval, period_callback, levelup_callback, anim_end_callback)
	if start > 100 then Echo('^rError: AnimateProgressBar() - Invalid start value!') return end
	
	if start >= target then
		--Echo('End of AnimateProgressBar : '..bar_widget)
		if anim_end_callback ~= nil then anim_end_callback() end
		return
	end
	
	if step > target - start then step = target - start end
	
	local progress_counting = start
	local progress_target = target
	
	GetWidget(bar_widget):Sleep(interval, function() 
		progress_counting = progress_counting + step

--		Echo('^gprogress_counting = '..tostring(progress_counting)..'^*')
--		Echo('^gprogress_target = '..tostring(progress_target)..'^*')
		
		if progress_counting <= progress_target then
			if progress_counting >= 100 then
				progress_counting = progress_counting - 100
				progress_target = progress_target - 100
				
				if levelup_callback ~= nil then levelup_callback(progress_counting) end
			end
			
			GetWidget(bar_widget):SetWidth(tonumber(progress_counting)..'%')
			
			if period_callback ~= nil then period_callback(progress_counting) end
			
			AnimateProgressBar(bar_widget, progress_counting, progress_target, step, interval, period_callback, levelup_callback, anim_end_callback)
		end
	end)
end

function RewardStat_PlayerEXPAnimation2(nickname, exp_before, exp_delta, lvl_before, rating_before, rating_delta)

	GetWidget('rewardstat_player_name'):SetText(nickname and nickname or '---')

	exp_before = tonumber(EmptyCheck(exp_before))
	lvl_before = tonumber(EmptyCheck(lvl_before))
	
	local exp_after = exp_before + tonumber(EmptyCheck(exp_delta))
	
	local rating_new = tonumber(EmptyCheck(rating_before)) + tonumber(EmptyCheck(rating_delta))
	
	--if new rating is less then 1250 it won't display the subtraction visually, on the db the player has not gone below 1250
	if rating_new < 1250 then
		rating_new = 1250
	end
	
	GetWidget('rewardstat_player_level'):Sleep(1, function() 
		if GetCvarBool('cl_GarenaEnable') and matchStats.showMMR == true then
			GetWidget('rewardstat_player_level'):SetText('LV. ' .. lvl_before .. '  ( ^rMMR: ^*' .. math.ceil(rating_new) .. ' )')
		else
			GetWidget('rewardstat_player_level'):SetText('LV. ' .. lvl_before)
		end
	end)
	
	local least_exp_lvl_before = tonumber(interface:UICmd([[GetAccountExperienceForLevel(]]..lvl_before..[[)]]))
	local least_exp_lvl_next = tonumber(interface:UICmd([[GetAccountExperienceForLevel(]]..(lvl_before+1)..[[)]]))
	
	if exp_before < least_exp_lvl_before or exp_after < least_exp_lvl_before or exp_before > exp_after then
		Echo('^rError: RewardStat_PlayerEXPAnimation2() - invalid parameters!^*')
		return
	end
	
	local exp_range_curr = least_exp_lvl_next - least_exp_lvl_before
	local exp_delta_curr = exp_before - least_exp_lvl_before
	local percent_before = exp_delta_curr / exp_range_curr * 100

	GetWidget('rewardstat_player_label_accountexp'):SetText(exp_delta_curr .. '/' .. exp_range_curr)
	GetWidget('rewardstat_player_frame_accountexp'):SetWidth(percent_before .. '%')
	
	local percent_after = 0
	local lvl_curr = lvl_before
	local exp_delta = 0
	repeat
		local exp_lvl_curr = tonumber(interface:UICmd([[GetAccountExperienceForLevel(]]..lvl_curr..[[)]]))
		local exp_lvl_next = tonumber(interface:UICmd([[GetAccountExperienceForLevel(]]..(lvl_curr+1)..[[)]]))
		local exp_range = exp_lvl_next - exp_lvl_curr
		exp_delta = exp_after - exp_lvl_curr
		if exp_delta >= exp_range then
			percent_after = percent_after + 100
		elseif exp_delta > 0 then
			percent_after = percent_after + exp_delta / exp_range * 100
		end
		lvl_curr = lvl_curr + 1
	until( exp_delta <= 0)

	-- Echo('^gpercent_before = '..percent_before..'%^*')
	-- Echo('^gpercent_after = '..percent_after..'%^*')
	
	-- start animation recursive calls
	lvl_curr = lvl_before
	
	local function AccountExpPeriodCallback(curPercentage)
		GetWidget('rewardstat_player_label_accountexp'):SetText(math.floor(curPercentage * exp_range_curr / 100) .. '/' .. exp_range_curr)
	end
	
	local function AccountExpLevelupCallback(curPercentage)
		GetWidget('rewardstat_experience_levelup_effect'):SetEffect('/ui/fe2/NewUI/Res/match_stats/effects/player_lv_up.effect')
		GetWidget('rewardstat_player_frame_accountexp'):SetWidth('0%')

		lvl_curr = lvl_curr + 1
		least_exp_lvl_before = tonumber(interface:UICmd([[GetAccountExperienceForLevel(]]..lvl_curr..[[)]]))
		least_exp_lvl_next = tonumber(interface:UICmd([[GetAccountExperienceForLevel(]]..(lvl_curr+1)..[[)]]))
		exp_range_curr = least_exp_lvl_next - least_exp_lvl_before

		if GetCvarBool('cl_GarenaEnable') and matchStats.showMMR == true then
			GetWidget('rewardstat_player_level'):SetText('LV. ' .. lvl_curr .. '  ( ^rMMR: ^*' .. math.floor(rating_new) .. ' )')
		else
			GetWidget('rewardstat_player_level'):SetText('LV. ' .. lvl_curr)
		end
	end

	AnimateProgressBar('rewardstat_player_frame_accountexp_extra', 
		percent_before, percent_after, 0.1, 14, 
		AccountExpPeriodCallback, AccountExpLevelupCallback, nil)
end

function RewardStat_PlayerEXPAnimation(experience_total_next_level, experience_total_current_level, perf_total_experience_new, account_level_old, nickname, experience_into_current_level, account_rating_old, account_rating_delta)

	Echo(experience_total_next_level .. '|' .. experience_total_current_level .. '|' .. perf_total_experience_new .. '|' .. account_level_old .. '|' .. nickname .. '|' .. experience_into_current_level .. '|' .. account_rating_old .. '|' .. account_rating_delta)
	
	--Debug Locals
	local nickname = nickname
	local account_level_old = account_level_old
	local xpData = tonumber(experience_total_current_level)
	local xpData_delta = tonumber(perf_total_experience_new)
	local total_xp_earned = tonumber(experience_total_current_level) + tonumber(perf_total_experience_new) + tonumber(experience_into_current_level)
	local xpMax = tonumber(experience_total_next_level)
	local xpPercentOriginal = (xpData / xpMax * 100)
	local xpPercentNew = total_xp_earned / xpMax * 100
	local nextLevelTotal = tonumber(interface:UICmd([[GetAccountExperienceForLevel(]]..(EmptyCheck(account_level_old) + 2)..[[)]]))
	
	GetWidget('rewardstat_player_name'):SetText(nickname)
	
	-- [Show MMR] --
	GetWidget('rewardstat_player_level'):Sleep(1, function() 
		if GetCvarBool('cl_GarenaEnable') then
			if matchStats.showMMR == true then
				GetWidget('rewardstat_player_level'):SetText('LV. ' .. account_level_old .. '  ( ^rMMR: ^*' .. math.ceil(account_rating_old+account_rating_delta) .. ' )')
			else
				GetWidget('rewardstat_player_level'):SetText('LV. ' .. account_level_old)
			end
		else
			GetWidget('rewardstat_player_level'):SetText('LV. ' .. account_level_old)
		end
	end)
	
	GetWidget('rewardstat_player_label_accountexp'):SetText(total_xp_earned .. '/' .. xpMax)
	GetWidget('rewardstat_player_frame_accountexp'):SetWidth(xpPercentOriginal .. '%')
	
	CurXPAccVal = xpPercentOriginal
	XpLevelup = 0
	
	local function LoopAccXPBarAnim()
		GetWidget('rewardstat_player_frame_accountexp_extra'):Sleep(14, function()
			CurXPAccVal = CurXPAccVal+.1
			if CurXPAccVal <= xpPercentNew then	
				if CurXPAccVal > 100 then
					if XpLevelup == 0 then
						GetWidget('rewardstat_player_frame_accountexp'):SetWidth('0%')
						if GetCvarBool('cl_GarenaEnable') then
							GetWidget('rewardstat_player_level'):SetText('LV. ' .. account_level_old+1 .. '  ( ^rMMR: ^*' .. tonumber(account_rating_old+account_rating_delta) .. ' )')
						else
							GetWidget('rewardstat_player_level'):SetText('LV. ' .. account_level_old+1)
						end
						XpLevelup = 1
					end
					GetWidget('rewardstat_player_frame_accountexp_extra'):SetWidth(CurXPAccVal - 100 .. '%')
				else
					GetWidget('rewardstat_player_frame_accountexp_extra'):SetWidth(CurXPAccVal .. '%')
				end
				if XpLevelup == 1 then
					GetWidget('rewardstat_experience_levelup_effect'):SetEffect('/ui/fe2/NewUI/Res/match_stats/effects/player_lv_up.effect')
					GetWidget('rewardstat_player_label_accountexp'):SetText(math.ceil(CurXPAccVal * xpMax / 100 - xpMax) .. '/' .. nextLevelTotal)
				else
					GetWidget('rewardstat_player_label_accountexp'):SetText(math.ceil(CurXPAccVal * xpMax / 100) .. '/' .. xpMax)
				end
				LoopAccXPBarAnim()
			else
				--Echo('End of LoopAccXPBarAnim()')
			end
		end)
		
	end
	
	LoopAccXPBarAnim()

end

function RewardStat_ResetPlayerMVPAndBG()

	for i=0,4 do
		GetWidget('rewardstat_detailedstats_gradient_' .. '1' .. '_' .. i):SetVisible(0)
		GetWidget('rewardstat_mvpbanner_' .. '1' .. '_' .. i):SetVisible(0)
	end

	for i=0,4 do
		GetWidget('rewardstat_detailedstats_gradient_' .. '2' .. '_' .. i):SetVisible(0)
		GetWidget('rewardstat_mvpbanner_' .. '2' .. '_' .. i):SetVisible(0)
	end

end

function RewardStat_PlayerName(pn_selected_upgrade, nickname)

	local nameColor = GetChatNameColorStringFromUpgrades(pn_selected_upgrade)
	local nameColorFont = GetChatNameColorFontFromUpgrades(pn_selected_upgrade)

	local playername = GetPlayerName(nickname)
	local clanname = GetClanTag(nickname)

	local widget = GetWidget('rewardstat_player_name')

	widget:SetFont(NotEmpty(nameColorFont) and nameColorFont..'_16' or 'dyn_16')
	widget:SetColor(NotEmpty(nameColor) and nameColor or '#efd2c0')
	widget:SetGlow(GetChatNameGlowFromUpgrades(pn_selected_upgrade))
	widget:SetGlowColor(GetChatNameGlowColorStringFromUpgrades(pn_selected_upgrade))
	widget:SetBackgroundGlow(GetChatNameBackgroundGlowFromUpgrades(pn_selected_upgrade))
	widget:SetText(playername)
end

function Test5()

	--GetWidget('rewardstat_frame_mvprewards'):RecalculateSize()
	GetWidget('rewardstat_frame_mvprewards'):GetChildren()
	local rewardWidgets = GetWidget('rewardstat_frame_mvprewards'):GetChildren()
	local rewardWNum = #rewardWidgets
	for i=1, rewardWNum, 1 do
		rewardWidgets[i]:SetVisible(false)
		rewardWidgets[i]:Destroy()
	end

end


function Match_Stats_V2:OnClickMainTab(type)
	if type == 'rewards' then
		if matchStats_RewardsDisabled ~= 1 then
			GetWidget('match_stats_tabbutton_rewards'):SetEnabled(false)
			GetWidget('match_stats_tabbutton_stats'):SetEnabled(true)
			GetWidget('rewardstat_rewardframe'):SetVisible(true)
			GetWidget('rewardstat_statframe'):SetVisible(false)
		end
	elseif type == 'stats' then
		GetWidget('match_stats_tabbutton_rewards'):SetEnabled(true)
		GetWidget('match_stats_tabbutton_stats'):SetEnabled(false)
		GetWidget('rewardstat_rewardframe'):SetVisible(false)
		GetWidget('rewardstat_statframe'):SetVisible(true)
		if 	GetWidget('endgame_stats_fetch_match_container'):IsVisible() then
			GetWidget('endgame_stats_matchid_label'):SetVisible(0)
			GetWidget('endgame_stats_matchid_label_cover'):SetVisible(0)
			GetWidget('endgame_stats_fetch_match'):SetFocus(1)
		end
	else
		GetWidget('match_stats_tabbutton_rewards'):SetEnabled(false)
		GetWidget('match_stats_tabbutton_stats'):SetEnabled(false)
		GetWidget('rewardstat_rewardframe'):SetVisible(false)
		GetWidget('rewardstat_statframe'):SetVisible(false)
	end
end

function Match_Stats_V2:SwitchDetailedSimple()
	if GetWidget('rewardstat_detailedstats_panel'):IsVisible() then
		GetWidget('rewardstat_detailedstats_panel'):SetVisible(false)
		GetWidget('rewardstat_simpledstats_panel'):SetVisible(true)
	else
		GetWidget('rewardstat_detailedstats_panel'):SetVisible(true)
		GetWidget('rewardstat_simpledstats_panel'):SetVisible(false)
	end
end

local function MatchInfoSummary(sourceWidget, update_type, match_id, match_date, match_time, name, time_played, ap, alt_pick, dm, em, ar, nl, nm, rd, shuf, sd,
	mname, winner, k2version, sUrl, iSize, _, bFileExists, sPath, compatVersion, map_name, bd, bp, ab, account_silver_coins,
	selected_upgrades, cas, no_stats, verified_only, gated, isBots, bm, matchUnixTime, cm, km, class, lp, br, bb, gamemode, hadLocalPlayer, 
	fp, sm, iLocalPlayerTeam, mwb, awd_mann, awd_mqk, awd_lgks, awd_msd, awd_mkill, awd_masst, awd_ledth, awd_mbdmg, awd_mvk, awd_mhdd, awd_hcs, mvp, ...)
	
	println('^c[MatchStats] ^*MatchInfoSummary Triggered. Result=' .. tostring(update_type) .. ' MatchID=' .. tostring(match_id))
	
	GetWidget('endgame_stats_waiting_mask'):FadeOut(150)

	if NotEmpty(update_type) then
		if update_type ~= 'main_stats_retrieving_match' and update_type ~= 'main_stats_retrieving_match' then
			Echo('^rError: match_stats - Retrieving match info failed! reason: '..(update_type ~= nil and update_type or 'unknown')..'^*')
			
			GetWidget('rewardstats_match_fetch_fail_mask'):FadeIn(250)
			
			GetWidget('endgame_stats_matchid_label'):SetText('')
		end
		return
	elseif (match_id) and NotEmpty(match_id) and (tonumber(match_id) > 0) then 
		GetWidget('endgame_stats_fetch_match'):EraseInputLine()

		Set('_stats_last_match_id', tostring(match_id), 'string')
		matchStats.selected_upgrade = selected_upgrades
		matchStats.mvpID = mvp
		matchStats.isSoccer = ((map_name) and (map_name == 'soccer'))
		matchStats.isCTF = ((map_name) and (map_name == 'capturetheflag'))
		matchStats_match_id = match_id
		matchStats.mvp_awd_mann = awd_mann
		matchStats.mvp_awd_mqk = awd_mqk
		matchStats.mvp_awd_lgks = awd_lgks
		matchStats.mvp_awd_msd = awd_msd
		matchStats.mvp_awd_mkill = awd_mkill
		matchStats.mvp_awd_masst = awd_masst
		matchStats.mvp_awd_ledth = awd_ledth
		matchStats.mvp_awd_mbdmg = awd_mbdmg
		matchStats.mvp_awd_mvp = awd_mvk
		matchStats.mvp_awd_mhdd = awd_mhdd
		matchStats.mvp_awd_hcs = awd_hcs
		matchStats.no_stats = no_stats
		matchStats.showMMR = false and GetCvarBool('cl_GarenaEnable')
		
		-- [Match ID] --
		GetWidget('endgame_stats_matchid_label'):SetText(match_id)
		
		-- [PTM Feedback Button] --
		local feedbackBtn = GetWidget('rewardstat_ptm_feedback_button')
		local feedbackFX = GetWidget('rewardstat_ptm_feedback_effect')
		if gated == tostring('1') then
			feedbackBtn:SetVisible(1)
			feedbackFX:SetVisible(1)
			if matchStats_showPopupIfGated == 1 then
				--Shows popup if its after match
				GetWidget('rewardstat_report_feedback_ptm_panel'):SetVisible(1)
			end
		else
			feedbackBtn:SetVisible(0)
			feedbackFX:SetVisible(0)
		end
		
		--Resets the if after match check
		matchStats_showPopupIfGated = 0
		
		-- [RAP After game Window] --
		if(AtoN(match_id) > 0) then
			Rap_Info.ShowRapPostgameWindow(time_played, match_id)
		end
		
		if tostring(UIGetAccountID()) == mvp then
			matchStats.mvp = '1'
		else
			matchStats.mvp = '0'
		end
		
		if matchStats.isSoccer then
			OnSoccerMatchInfoSummary(map_name, winner)
		end
	end
	
end
interface:RegisterWatch('MatchInfoSummary', MatchInfoSummary)

function RewardStat_GetMatchInfo(match_id)

	
	Match_Stats_V2:OnClickMainTab('stats')
	matchStats_UILevel = 0
	
	--Resets Victory/Defeat model graphic and player mvp awards
	Trigger('ModelReset')
	Trigger('ResetPlayerMVPMedals')
	RewardStat_DisableRewardsTabHandle(1)
	matchStats.repeatCheck = false
	RewardStat_ResetPlayerMVPAndBG()
	GetWidget('rewardstat_replaybutton'):SetVisible(1)
	GetWidget('rewardstat_replay_uploadinglabel'):SetVisible(0)
	GetWidget('rewardstat_nextlevelaward_panel'):SetVisible(0)
	
	GetWidget('rewardstats_match_fetch_fail_mask'):SetVisible(0)
	GetWidget('rewardstats_loadingentity_mask'):SetVisible(0)
	
	GetWidget('rewardstat_detailedstats_standard'):SetVisible(1)
	GetWidget('rewardstat_detailedstats_ctf'):SetVisible(0)
	GetWidget('rewardstat_detailedstats_soccer'):SetVisible(0)
	
	GetWidget('endgame_stats_fetch_match_container'):SetVisible(0)
	GetWidget('endgame_stats_matchid_label'):SetText('')
	GetWidget('endgame_stats_matchid_label'):SetVisible(1)
	GetWidget('endgame_stats_matchid_label_cover'):SetVisible(1)
	
	if not GetCvarBool('_entityDefinitionsLoaded') then
		Echo('^gLoading entity definitions...^*')
		_match_id_postponed_to_fetch = match_id
		GetWidget('match_stats'):Sleep(1, function()
				GetWidget('RegisterEntityDefinitionsHelper'):DoEvent()
				GetWidget('rewardstats_loadingentity_mask'):SetVisible(true)
			end)
	else
		interface:UICmd("GetMatchInfo("..match_id..")")
		Set('_stats_last_replay_id', match_id)
		GetWidget('endgame_stats_waiting_mask'):FadeIn(150)
	end
	
end

local function ChatShowPostGameStats()
	printdb('^g^: ChatShowPostGameStats 1')

	Set('ui_match_stats_waitingToShow', 'true', 'bool')
	Set('_stats_last_match_id', '', 'string')
	Set('_stats_last_replay_id', '', 'string')
	Set('_stats_last_match_id', GetShowStatsMatchID(), 'string')
	Set('_stats_latest_match_played', GetShowStatsMatchID(), 'string')

	printdb('Out of game ^g^: GetShowStatsMatchID() ' .. tostring(GetShowStatsMatchID()))
	
end

interface:RegisterWatch('ChatShowPostGameStats', ChatShowPostGameStats)

function RewardStat_SetPlayerBG(accID, triggerIndex, wNum, nickname, matchOwner)
	
	if  (NotEmpty(matchOwner) and nickname == matchOwner) or
		(not NotEmpty(matchOwner) and IsMe(nickname))
	then
		GetWidget('rewardstat_detailedstats_gradient_' .. wNum .. '_' .. triggerIndex):SetVisible(1)
		
		GetWidget('rewardstat_general_label_player_name_' .. wNum .. '_' .. triggerIndex):SetOutlineColor('#482e1b')
		
		GetWidget('rewardstat_general_label_player_lvl_' .. wNum .. '_' .. triggerIndex):SetColor('#ffdcc5')
		GetWidget('rewardstat_general_label_player_lvl_' .. wNum .. '_' .. triggerIndex):SetOutlineColor('#482e1b')
		GetWidget('rewardstat_general_label_player_kda_' .. wNum .. '_' .. triggerIndex):SetColor('#ffdcc5')
		GetWidget('rewardstat_general_label_player_kda_' .. wNum .. '_' .. triggerIndex):SetOutlineColor('#482e1b')
		GetWidget('rewardstat_general_label_player_ck_' .. wNum .. '_' .. triggerIndex):SetColor('#ffdcc5')
		GetWidget('rewardstat_general_label_player_ck_' .. wNum .. '_' .. triggerIndex):SetOutlineColor('#482e1b')
		GetWidget('rewardstat_general_label_player_assists_' .. wNum .. '_' .. triggerIndex):SetColor('#ffdcc5')
		GetWidget('rewardstat_general_label_player_assists_' .. wNum .. '_' .. triggerIndex):SetOutlineColor('#482e1b')
		GetWidget('rewardstat_general_label_player_exp_' .. wNum .. '_' .. triggerIndex):SetColor('#ffdcc5')
		GetWidget('rewardstat_general_label_player_exp_' .. wNum .. '_' .. triggerIndex):SetOutlineColor('#482e1b')
		GetWidget('rewardstat_general_label_player_gpm_' .. wNum .. '_' .. triggerIndex):SetColor('#ffdcc5')
		GetWidget('rewardstat_general_label_player_gpm_' .. wNum .. '_' .. triggerIndex):SetOutlineColor('#482e1b')
		GetWidget('rewardstat_general_label_player_apm_' .. wNum .. '_' .. triggerIndex):SetColor('#ffdcc5')
		GetWidget('rewardstat_general_label_player_apm_' .. wNum .. '_' .. triggerIndex):SetOutlineColor('#482e1b')
	else
		GetWidget('rewardstat_detailedstats_gradient_' .. wNum .. '_' .. triggerIndex):SetVisible(0)
		
		GetWidget('rewardstat_general_label_player_name_' .. wNum .. '_' .. triggerIndex):SetOutlineColor('#230000')
		
		GetWidget('rewardstat_general_label_player_lvl_' .. wNum .. '_' .. triggerIndex):SetColor('#d7b6a2')
		GetWidget('rewardstat_general_label_player_lvl_' .. wNum .. '_' .. triggerIndex):SetOutlineColor('#230000')
		GetWidget('rewardstat_general_label_player_kda_' .. wNum .. '_' .. triggerIndex):SetColor('#d7b6a2')
		GetWidget('rewardstat_general_label_player_kda_' .. wNum .. '_' .. triggerIndex):SetOutlineColor('#230000')
		GetWidget('rewardstat_general_label_player_ck_' .. wNum .. '_' .. triggerIndex):SetColor('#d7b6a2')
		GetWidget('rewardstat_general_label_player_ck_' .. wNum .. '_' .. triggerIndex):SetOutlineColor('#230000')	
		GetWidget('rewardstat_general_label_player_assists_' .. wNum .. '_' .. triggerIndex):SetColor('#d7b6a2')
		GetWidget('rewardstat_general_label_player_assists_' .. wNum .. '_' .. triggerIndex):SetOutlineColor('#230000')
		GetWidget('rewardstat_general_label_player_exp_' .. wNum .. '_' .. triggerIndex):SetColor('#d7b6a2')
		GetWidget('rewardstat_general_label_player_exp_' .. wNum .. '_' .. triggerIndex):SetOutlineColor('#230000')	
		GetWidget('rewardstat_general_label_player_gpm_' .. wNum .. '_' .. triggerIndex):SetColor('#d7b6a2')
		GetWidget('rewardstat_general_label_player_gpm_' .. wNum .. '_' .. triggerIndex):SetOutlineColor('#230000')	
		GetWidget('rewardstat_general_label_player_apm_' .. wNum .. '_' .. triggerIndex):SetColor('#d7b6a2')
		GetWidget('rewardstat_general_label_player_apm_' .. wNum .. '_' .. triggerIndex):SetOutlineColor('#230000')		
	end
	
end

function RewardStat_PlayerMVPMedals(accID, triggerIndex, wNum, isDebug)

	if isDebug == 1 then
		matchStats.mvp_awd_mann = 	512
		matchStats.mvp_awd_mqk = 	0
		matchStats.mvp_awd_lgks = 	0
		matchStats.mvp_awd_msd = 	0
		matchStats.mvp_awd_mkill = 	0
		matchStats.mvp_awd_masst = 	0
		matchStats.mvp_awd_ledth = 	0
		matchStats.mvp_awd_mbdmg = 	0
		matchStats.mvp_awd_mvp = 	0
		matchStats.mvp_awd_mhdd = 	0
		matchStats.mvp_awd_hcs = 	0
	end

	local mvpWidget = 'rewardstat_player_mvprewards_' .. wNum .. '_' .. triggerIndex
	
	local function FrameMedalAmount(widget)
		local frame = GetWidget(widget):GetChildren()
		if (#frame % 2 == 0) then
			return 0
		else
			return 1
		end
	end
	
	local function MedalDisplay(param, param2)
		if FrameMedalAmount(mvpWidget .. '_top') == 0 and FrameMedalAmount(mvpWidget .. '_bottom') == 0 then
			GetWidget(mvpWidget .. '_top'):Instantiate('rewardstat_player_mvp_award', 'icon', '/ui/fe2/newui/Res/playerstats/mvpawards/' .. param, 'widget', param2)
		elseif FrameMedalAmount(mvpWidget .. '_bottom') == 0 then
			GetWidget(mvpWidget .. '_bottom'):Instantiate('rewardstat_player_mvp_award', 'icon', '/ui/fe2/newui/Res/playerstats/mvpawards/' .. param, 'widget', param2)
		elseif FrameMedalAmount(mvpWidget .. '_top') == 1 and FrameMedalAmount(mvpWidget .. '_bottom') == 1 then
			GetWidget(mvpWidget .. '_top'):Instantiate('rewardstat_player_mvp_award', 'icon', '/ui/fe2/newui/Res/playerstats/mvpawards/' .. param, 'widget', param2)
		elseif FrameMedalAmount(mvpWidget .. '_top') == 0 and FrameMedalAmount(mvpWidget .. '_bottom') == 1 then
			GetWidget(mvpWidget .. '_bottom'):Instantiate('rewardstat_player_mvp_award', 'icon', '/ui/fe2/newui/Res/playerstats/mvpawards/' .. param, 'widget', param2)
		else
			--Nothing
		end
	end
	
	if accID == matchStats.mvp_awd_mann then	
		MedalDisplay('awd_mann_small.png', 'award_mann')
	end
	if accID == matchStats.mvp_awd_mqk then
		MedalDisplay('awd_mqk_small.png', 'award_mqk')
	end
	if accID == matchStats.mvp_awd_lgks then
		MedalDisplay('awd_lgks_small.png', 'award_lgks')
	end
	if accID == matchStats.mvp_awd_msd then
		MedalDisplay('awd_msd_small.png', 'award_msd')
	end
	if accID == matchStats.mvp_awd_mkill then
		MedalDisplay('awd_mkill_small.png', 'award_mkill')
	end
	if accID == matchStats.mvp_awd_masst then
		MedalDisplay('awd_masst_small.png', 'award_masst')
	end
	if accID == matchStats.mvp_awd_ledth then
		MedalDisplay('awd_ledth_small.png', 'award_ledth')
	end
	if accID == matchStats.mvp_awd_mbdmg then
		MedalDisplay('awd_mbdmg_small.png', 'award_mbdmg')
	end
	if accID == matchStats.mvp_awd_mvp then
		MedalDisplay('awd_mvp_small.png', 'award_mvp')
	end
	if accID == matchStats.mvp_awd_mhdd then
		MedalDisplay('awd_mhdd_small.png', 'award_mhdd')
	end
	if accID == matchStats.mvp_awd_hcs then
		MedalDisplay('awd_hcs_small.png', 'award_hcs')
	end
	
	
	
end

function RewardStat_ResetMatchStatsMVP()

	--Resets player match stats awards
	local pRewardWidgets = GetWidget(self:GetName()):GetChildren()
	local pRewardWNum = #pRewardWidgets
	for i=1, pRewardWNum, 1 do
		pRewardWidgets[i]:SetVisible(false)
		pRewardWidgets[i]:Destroy()
	end

end

function TestBranch(param)

	Echo('testbranch: ' .. param)

end

function RewardStat_DisableRewardsTabHandle(param)

	if param == 1 then
		for i=1,3 do
			GetWidget('match_stats_tabbutton_rewards_img' .. i):SetColor('.5 .5 .5')
		end	
		matchStats_RewardsDisabled = 1
	else
		for i=1,3 do
			GetWidget('match_stats_tabbutton_rewards_img' .. i):SetColor('1 1 1')
		end
		matchStats_RewardsDisabled = 0	
	end

end

function MatchStatsLayoutHandle(perf_victory_exp, perf_victory_gc, mastery_exp_match, mastery_maxlevel_addon, mastery_exp_bonus, arg28, mastery_maxlevel_herocount, arg17, arg20, mastery_exp_to_boost, mastery_exp_boost, mastery_exp_super_boost)

	if (tonumber(mastery_exp_match) > 0) then
		matchStats.isanyStats = true
		matchStats_maxleveladdon = tonumber(mastery_maxlevel_addon)
		matchStats_matchExp = tonumber(mastery_exp_match)
		matchStats_bonusExp = tonumber(mastery_exp_bonus)
		matchStats_ToBoostExp = tonumber(mastery_exp_to_boost)
		matchStats_BoostExp = tonumber(mastery_exp_boost)
		matchStats_SuperBoostExp = tonumber(mastery_exp_super_boost)
		matchStats_eventBonus = tonumber(arg28)
		matchStats_maxleveladdonPercent = math.floor(matchStats_maxleveladdon / matchStats_matchExp * 100 + 0.5)
		matchStats_maxlevelcount = tonumber(mastery_maxlevel_herocount)
		if tonumber(arg17) > 0 or tonumber(arg20) > 0 then
			matchStats.isanyStats = true
			SetRewardLayout('con')
		else
			matchStats.isanyStats = true
			SetRewardLayout('masteryonly')
		end
	elseif tonumber(arg17) > 0 or tonumber(arg20) > 0 then
		SetRewardLayout('rankonly')
		matchStats.isanyStats = true
	elseif tonumber(perf_victory_exp) >= 0 or tonumber(perf_victory_exp) >= 0 then
		matchStats.isanyStats = true
		SetRewardLayout('coinsonly')
	else
		matchStats.isanyStats = false
		Match_Stats_V2:OnClickMainTab('stats')
		RewardStat_DisableRewardsTabHandle(1)
		SetRewardLayout('nostats')		
	end

end

function AccChatIconMVPBannerPlayerNameLegacyBonusMVPAwards(legacy_bonus_gc, annihilation, quadkill, ks3, ks4, ks5, ks6, ks7, ks8, ks9, ks10, ks15, perf_smackdown_delta, herokills, heroassists, deaths, herodmg, teamcreepkills, bdmg, perf_multiplier_mmpoints)

	GetWidget('rewardstat_centerframe'):Sleep(1, function()
		--Account Icon
		if (string.find(matchStats.selected_upgrade, 'custom_icon')) then
			GetWidget('rewardstat_player_icon_1'):UICmd([[SetTextureURL(
					GetICBURL()
					# '/' #
					'icons'
					# '/' #
					Floor(GetAccountID() / 1000000) #
					'/' #
					Floor((GetAccountID() - (Floor(GetAccountID() / 1000000) * 1000000)) / 1000) #
					'/' #
					GetAccountID() #
					'/' #
					SubString(']]..matchStats.selected_upgrade..[[',
						SearchString(']]..matchStats.selected_upgrade..[[', ':', 0) + 1,
						( StringLength(']]..matchStats.selected_upgrade..[[') - (SearchString(']]..matchStats.selected_upgrade..[[', ':', 0) - 1) )
					) #
					'.cai'
				)
			]])
		elseif NotEmpty(matchStats.selected_upgrade) then
			GetWidget('rewardstat_player_icon_1'):SetTexture( GetAccountIconTexturePathFromUpgrades(matchStats.selected_upgrade) )
		else
			GetWidget('rewardstat_player_icon_1'):SetTexture('/ui/common/ability_coverup.tga')
		end
		
		--Chat Symbol
		local chatSymbol = GetChatSymbolTexturePathFromUpgrades(matchStats.selected_upgrade)
		local chatColor = GetChatNameColorTexturePathFromUpgrades(matchStats.selected_upgrade)
		
		if NotEmpty(chatSymbol) then
			GetWidget('rewardstat_player_icon_2'):SetTexture(chatSymbol)
		elseif NotEmpty(chatColor) then
			GetWidget('rewardstat_player_icon_2'):SetTexture(chatColor)
		else
			GetWidget('rewardstat_player_icon_2'):SetTexture('$invis')
		end
		
		--Player Name
		local pn_selected_upgrade = matchStats.selected_upgrade
		-- [Commented out FOR **NOW** ] RewardStat_PlayerName(pn_selected_upgrade, nickname)
		
		-- [MVP Banner] --
		if matchStats.mvp == '1' then
			GetWidget('rewardstat_image_modelpanel_mvp'):SetTexture('/ui/fe2/NewUI/Res/playerstats/mvpawards/awd_mvp_big.png')
		else
			GetWidget('rewardstat_image_modelpanel_mvp'):SetTexture('$invis') 
		end
		
		-- [Legacy Bonus Award] --
		if legacy_bonus_gc > 0 then
			GetWidget('rewardstat_legacy_parentpanel'):SetVisible(1)
			GetWidget('rewardstat_label_legacy_gold'):SetText('+' .. legacy_bonus_gc)
		else
			GetWidget('rewardstat_legacy_parentpanel'):SetVisible(0)
			GetWidget('rewardstat_label_legacy_gold'):SetText('')
		end
		
		-- [URSA Bonus Award] --
		if perf_multiplier_mmpoints > 0 then
			GetWidget('rewardstat_ursa_parentpanel'):SetVisible(1)
			GetWidget('rewardstat_label_ursa_gold'):SetText('+' .. perf_multiplier_mmpoints)
		else
			GetWidget('rewardstat_ursa_parentpanel'):SetVisible(0)
			GetWidget('rewardstat_label_ursa_gold'):SetText('')
		end
			
		
		-- [MVP Awards] --
		
		-- This checks if you won any mvp medals at all, if not the frame is hidden from the user
		local anyMvp = {matchStats.mvp_awd_mann, matchStats.mvp_awd_mqk, matchStats.mvp_awd_lgks, matchStats.mvp_awd_msd, matchStats.mvp_awd_mkill, matchStats.mvp_awd_masst, matchStats.mvp_awd_ledth, matchStats.mvp_awd_mbdmg, matchStats.mvp_awd_mvp, matchStats.mvp_awd_mhdd, matchStats.mvp_awd_hcs}
		GetWidget('rewardstat_frame_mvprewards'):SetVisible(0)
		for i=1,11 do
			if anyMvp[i] == tostring(UIGetAccountID()) then
				RewardStat_MVPFrameSize = RewardStat_MVPFrameSize+1
				GetWidget('rewardstat_mvp_panel'):SetVisible(1)
				if RewardStat_MVPFrameSize == 0 then
					--Nothing
				elseif RewardStat_MVPFrameSize == 1 then
					GetWidget('rewardstat_mvp_panel'):SetWidth('12h')
				elseif RewardStat_MVPFrameSize == 2 then
					GetWidget('rewardstat_mvp_panel'):SetWidth('21h')
				elseif RewardStat_MVPFrameSize == 3 then
					GetWidget('rewardstat_mvp_panel'):SetWidth('30h')
				elseif RewardStat_MVPFrameSize == 4 then
					GetWidget('rewardstat_mvp_panel'):SetWidth('39h')
				end
				
			end
		end

		--This is a draft / rough test, above ( outside this 1ms sleep ) the instantiated award widgets are destroyed, and this currently checks for
		--each 'awd' string I saw, and makes a draft award, the title sets to the string with !'s around them (eg: !mvp_awd_lgks!) and the
		--value is just set to the account id ( which will always be your account id obviously or the award isn't created at all )
		--if your account id equals the reward, will make this set data later after I simplfy this atrocity below.
		--Tested with match id: 17485
		
		if tostring(UIGetAccountID()) == matchStats.mvp_awd_mann then
			GetWidget('rewardstat_mvp_listbox'):AddTemplateListItem('rewardstat_mvp_award', 1, 'content', Translate('award_mann'), 'id', 'mvp_awd_mann', 'data',  tonumber(annihilation), 'icon', '/ui/fe2/newui/Res/playerstats/mvpawards/awd_mann_big.png')
		end
		if tostring(UIGetAccountID()) == matchStats.mvp_awd_mqk then
			GetWidget('rewardstat_mvp_listbox'):AddTemplateListItem('rewardstat_mvp_award', 1, 'content', Translate('award_mqk'), 'id', 'mvp_awd_mqk', 'data', tonumber(quadkill), 'icon', '/ui/fe2/newui/Res/playerstats/mvpawards/awd_mqk_big.png')
		end
		if tostring(UIGetAccountID()) == matchStats.mvp_awd_lgks then
			GetWidget('rewardstat_mvp_listbox'):AddTemplateListItem('rewardstat_mvp_award', 1, 'content', Translate('award_lgks'), 'id', 'mvp_awd_lgks', 'data', RewardStat_CalculateKillStreak(ks3, ks4, ks5, ks6, ks7, ks8, ks9, ks10, ks15), 'icon', '/ui/fe2/newui/Res/playerstats/mvpawards/awd_lgks_big.png')
		end
		if tostring(UIGetAccountID()) == matchStats.mvp_awd_msd then
			GetWidget('rewardstat_mvp_listbox'):AddTemplateListItem('rewardstat_mvp_award', 1, 'content', Translate('award_msd'), 'id', 'mvp_awd_msd', 'data', perf_smackdown_delta, 'icon', '/ui/fe2/newui/Res/playerstats/mvpawards/awd_msd_big.png')
		end
		if tostring(UIGetAccountID()) == matchStats.mvp_awd_mkill then
			GetWidget('rewardstat_mvp_listbox'):AddTemplateListItem('rewardstat_mvp_award', 1, 'content', Translate('award_mkill'), 'id', 'mvp_awd_mkill', 'data', herokills, 'icon', '/ui/fe2/newui/Res/playerstats/mvpawards/awd_mkill_big.png')
		end
		if tostring(UIGetAccountID()) == matchStats.mvp_awd_masst then
			GetWidget('rewardstat_mvp_listbox'):AddTemplateListItem('rewardstat_mvp_award', 1, 'content', Translate('award_masst'), 'id', 'mvp_awd_masst', 'data', heroassists, 'icon', '/ui/fe2/newui/Res/playerstats/mvpawards/awd_masst_big.png')
		end
		if tostring(UIGetAccountID()) == matchStats.mvp_awd_ledth then
			GetWidget('rewardstat_mvp_listbox'):AddTemplateListItem('rewardstat_mvp_award', 1, 'content', Translate('award_ledth'), 'id', 'mvp_awd_ledth', 'data', deaths, 'icon', '/ui/fe2/newui/Res/playerstats/mvpawards/awd_ledth_big.png')
		end
		if tostring(UIGetAccountID()) == matchStats.mvp_awd_mbdmg then
			GetWidget('rewardstat_mvp_listbox'):AddTemplateListItem('rewardstat_mvp_award', 1, 'content', Translate('award_mbdmg'), 'id', 'mvp_awd_mbdmg', 'data', tonumber(bdmg), 'icon', '/ui/fe2/newui/Res/playerstats/mvpawards/awd_mbdmg_big.png')
		end
		if tostring(UIGetAccountID()) == matchStats.mvp_awd_mvp then
			GetWidget('rewardstat_mvp_listbox'):AddTemplateListItem('rewardstat_mvp_award', 1, 'content', Translate('award_mwk'), 'id', 'mvp_awd_mvk', 'data', '(need wardkill data)', 'icon', '/ui/fe2/newui/Res/playerstats/mvpawards/awd_mwk_big.png')
		end
		if tostring(UIGetAccountID()) == matchStats.mvp_awd_mhdd then
			GetWidget('rewardstat_mvp_listbox'):AddTemplateListItem('rewardstat_mvp_award', 1, 'content', Translate('award_mhdd'), 'id', 'mvp_awd_mhdd', 'data', tonumber(herodmg), 'icon', '/ui/fe2/newui/Res/playerstats/mvpawards/awd_mhdd_big.png')
		end
		if tostring(UIGetAccountID()) == matchStats.mvp_awd_hcs then
			GetWidget('rewardstat_mvp_listbox'):AddTemplateListItem('rewardstat_mvp_award', 1, 'content', Translate('award_hcs'), 'id', 'mvp_awd_hcs', 'data', teamcreepkills, 'icon', '/ui/fe2/newui/Res/playerstats/mvpawards/awd_hcs_big.png')
		end
		
	end)

end

function FuncUpdatePlayerBreakdownItem(perf_wins_gc, perf_herokills_gc, perf_heroassists_gc, perf_wards_gc, perf_smackdowns_gc,
		perf_wins_exp,  perf_herokills_exp, perf_heroassists_exp, perf_wards_exp, perf_smackdowns_exp,
		perf_wins_delta, perf_herokills_delta, perf_heroassists_delta, perf_wards_delta, perf_smackdowns_delta, 
		perf_wins, perf_herokills, perf_heroassists, perf_wards, perf_smackdowns)

--[[
	local testcase = 3
	if testcase == 1 then
		-- test case normal
		perf_wins_gc		= 0
		perf_herokills_gc	= 0
		perf_heroassists_gc = 0
		perf_wards_gc		= 0
		perf_smackdowns_gc	= 0
		perf_wins_delta			= '1'
		perf_herokills_delta	= '20'
		perf_heroassists_delta	= '12'
		perf_wards_delta		= '5'
		perf_smackdowns_delta	= '1' 
		perf_wins			= 20
		perf_herokills		= 121
		perf_heroassists	= 105
		perf_wards			= 41
		perf_smackdowns		= 5
	elseif testcase == 2 then
		-- test case progress full
		perf_wins_gc		= 10
		perf_herokills_gc	= 5
		perf_heroassists_gc = 5
		perf_wards_gc		= 5
		perf_smackdowns_gc	= 1
		perf_wins_delta			= '1'
		perf_herokills_delta	= '50'
		perf_heroassists_delta	= '65'
		perf_wards_delta		= '9'
		perf_smackdowns_delta	= '5' 
		perf_wins			= 49
		perf_herokills		= 200
		perf_heroassists	= 185
		perf_wards			= 41
		perf_smackdowns		= 5
	elseif testcase == 3 then
		-- test case progress overflow
		perf_wins_gc		= 10
		perf_herokills_gc	= 5
		perf_heroassists_gc = 5
		perf_wards_gc		= 5
		perf_smackdowns_gc	= 1
		perf_wins_delta			= '21'
		perf_herokills_delta	= '150'
		perf_heroassists_delta	= '265'
		perf_wards_delta		= '29'
		perf_smackdowns_delta	= '9' 
		perf_wins			= 40
		perf_herokills		= 200
		perf_heroassists	= 185
		perf_wards			= 41
		perf_smackdowns		= 25
	end
--]]

	local perf_data = {}
	perf_data[1] = {}
	perf_data[1].name = 'wins'
	perf_data[1].gc = perf_wins_gc
	perf_data[1].exp = perf_wins_exp
	perf_data[1].delta = perf_wins_delta
	perf_data[1].old = perf_wins
	perf_data[2] = {}
	perf_data[2].name = 'kills'
	perf_data[2].gc = perf_herokills_gc
	perf_data[2].exp = perf_herokills_exp
	perf_data[2].delta = perf_herokills_delta
	perf_data[2].old = perf_herokills
	perf_data[3] = {}
	perf_data[3].name = 'assists'
	perf_data[3].gc = perf_heroassists_gc
	perf_data[3].exp = perf_heroassists_exp
	perf_data[3].delta = perf_heroassists_delta
	perf_data[3].old = perf_heroassists
	perf_data[4] = {}
	perf_data[4].name = 'wards'
	perf_data[4].gc = perf_wards_gc
	perf_data[4].exp = perf_wards_exp
	perf_data[4].delta = perf_wards_delta
	perf_data[4].old = perf_wards
	perf_data[5] = {}
	perf_data[5].name = 'smackdowns'
	perf_data[5].gc = perf_smackdowns_gc
	perf_data[5].exp = perf_smackdowns_exp
	perf_data[5].delta = perf_smackdowns_delta
	perf_data[5].old = perf_smackdowns
	
	local function UpdatePlayerBreakdownItem(isanyStats)
		
		if isanyStats == true then
			if NotEmpty(perf_wins_delta) then
				
				for i, data in ipairs(perf_data) do
				
					local remainder = matchStats.milestonesTable[i].STEP
					local newScore = data.old + data.delta
					local curScore = newScore > remainder and newScore % remainder or newScore
					-- current values
					GetWidget('rewardstat_extra_'..data.name):SetText(curScore..'/'..remainder)
					-- progress bars
					GetWidget('rewardstat_'..data.name..'_framepercent'):SetWidth(100 * tonumber(curScore) / remainder .. '%')
					-- total values
					GetWidget('rewardstat_extra_'..data.name..'_total'):SetText(tostring(newScore))
					if newScore > remainder then
						GetWidget('rewardstat_'..data.name..'_framepercent_body'):SetY('2i')
						GetWidget('rewardstat_extra_'..data.name..'_total'):SetVisible(1)
					else
						GetWidget('rewardstat_'..data.name..'_framepercent_body'):SetY('-2i')
						GetWidget('rewardstat_extra_'..data.name..'_total'):SetVisible(0)
					end
					-- coins
					if data.gc > 0 then
						GetWidget('rewardstat_extra_'..data.name..'_gold'):SetVisible(1)
						GetWidget('rewardstat_extra_'..data.name..'_gold_icon'):SetVisible(1)
						GetWidget('rewardstat_extra_'..data.name..'_gold'):SetText('+'..tostring(data.gc))
					else
						GetWidget('rewardstat_extra_'..data.name..'_gold'):SetVisible(0)
						GetWidget('rewardstat_extra_'..data.name..'_gold_icon'):SetVisible(0)
						GetWidget('rewardstat_extra_'..data.name..'_gold'):SetText('-')
					end
					
				end
			
			else
				SetRewardLayout('onlymastery')
			end
			
		end
		
	end
	
	local isanyStats = matchStats.isanyStats
	UpdatePlayerBreakdownItem(isanyStats)

end

function EmptyAward(perf_bloodlust_exp, perf_bloodlust_gc, perf_ks15_exp, perf_ks15_gc, perf_annihilation_exp, perf_annihilation_gc)

	function HideEmptyAward(param, widgeticon)
		if param == 0 then
			GetWidget(widgeticon):SetVisible(0)
			return ''
		else
			GetWidget(widgeticon):SetVisible(1)
			return ('+' .. param)
		end
	end

	GetWidget('rewardstat_label_bloodlust_xp'):SetText(HideEmptyAward(perf_bloodlust_exp, 'rewardstat_icon_bloodlust_xp'))
	GetWidget('rewardstat_label_bloodlust_gold'):SetText(HideEmptyAward(perf_bloodlust_gc, 'rewardstat_icon_bloodlust_gold'))
	GetWidget('rewardstat_label_immortal_xp'):SetText(HideEmptyAward(perf_ks15_exp, 'rewardstat_icon_immortal_xp'))
	GetWidget('rewardstat_label_immortal_gold'):SetText(HideEmptyAward(perf_ks15_gc, 'rewardstat_icon_immortal_gold'))
	GetWidget('rewardstat_label_annihilation_gold_xp'):SetText(HideEmptyAward(perf_annihilation_exp, 'rewardstat_icon_annihilation_xp'))
	GetWidget('rewardstat_label_annihilation_gold'):SetText(HideEmptyAward(perf_annihilation_gc, 'rewardstat_icon_annihilation_gold'))

end

function HeroMatchInventory(inventory1, inventory2, inventory3, inventory4, inventory5, inventory6)

	local inventorySlots = { inventory1, inventory2, inventory3, inventory4, inventory5, inventory6 }
	for i=1,6,1 do
		if inventorySlots[i] and string.len(inventorySlots[i]) > 0 then
			GetWidget('rewardstat_player_inventory'..i):SetVisible(true)
			GetWidget('rewardstat_player_inventory'..i):SetTexture(GetEntityIconPath(inventorySlots[i]))
			GetWidget('rewardstat_player_inventory'..i):SetCallback('onmouseover', function(widget)
				--print("TriggerItemTooltip('MSPlayerInventoryTip', "..inventorySlots[i].."', 0)\n")
				widget:UICmd("TriggerItemTooltip('MSPlayerInventoryTip', '"..inventorySlots[i].."', 0)")
				Trigger('MSPlayerInventoryTipIcon', GetEntityIconPath(inventorySlots[i]))

				GetWidget('endgameItemTooltip'):SetVisible(true)
			end)
			GetWidget('rewardstat_player_inventory'..i):SetCallback('onmouseout', function(widget)
				GetWidget('endgameItemTooltip'):SetVisible(false)
			end)
		else
			GetWidget('rewardstat_player_inventory'..i):SetTexture('/ui/common/empty_pack_garena.tga')
		end
	end

end

function MVPAwardReset()

	RewardStat_MVPFrameSize = 0
	
	GetWidget('rewardstat_mvp_panel'):SetWidth('12h')
	GetWidget('rewardstat_mvp_panel'):SetVisible(0)
	
	Trigger('RewardStat_ClearList')

	--GetWidget('rewardstat_frame_mvprewards'):GetChildren()
	--local rewardWidgets = GetWidget('rewardstat_frame_mvprewards'):GetChildren()
	--local rewardWNum = #rewardWidgets
	--for i=1, rewardWNum, 1 do
	--	rewardWidgets[i]:SetVisible(false)
	--	rewardWidgets[i]:Destroy()
	--end

end

function LevelKDAGPM(hero_level, herokills, deaths, heroassists, gold)
	GetWidget('rewardstat_player_herolevel'):SetText('LV: ^888' .. hero_level)
	GetWidget('rewardstat_player_herokda'):SetText('KDA: ' .. Translate('game_end_stats_kda', 'kills', herokills, 'deaths', deaths, 'assists', heroassists))
	GetWidget('rewardstat_player_herogpm'):SetText('GPM: ^y' .. FtoA(gold, 1))			
end

function TotalEarnedGameADaySocialBonusConsecutiveBonusMatchVictory(perf_total_experience_new, perf_total_silver_new, perf_victory_exp, perf_victory_gc, perf_first_exp, perf_first_gc, perf_social_bonus_gc, perf_consec_gc, team, winningTeam)

	lib_Anim.AnimateNumericLabel('rewardstat_label_total_xp', 0, 0, perf_total_experience_new, false, '+')
	lib_Anim.AnimateNumericLabel('rewardstat_label_total_gold', 0, 0, perf_total_silver_new, false, '+')

	lib_Anim.AnimateNumericLabel('rewardstat_label_fin_xp', 0, 0, perf_victory_exp, false, '+')
	lib_Anim.AnimateNumericLabel('rewardstat_label_fin_gold', 0, 0, perf_victory_gc, false, '+')			

	lib_Anim.AnimateNumericLabel('rewardstat_label_gameaday_xp', 0, 0, perf_first_exp, false, '+')
	lib_Anim.AnimateNumericLabel('rewardstat_label_gameaday_gold', 0, 0, perf_first_gc, false, '+')

	lib_Anim.AnimateNumericLabel('rewardstat_label_social_gold', 0, 0, perf_social_bonus_gc, false, '+')
	lib_Anim.AnimateNumericLabel('rewardstat_label_conesc_gold', 0, 0, perf_consec_gc, false, '+')
	
	if tonumber(winningTeam) == tonumber(team) then
		GetWidget('rewardstat_label_fin_xp_desc'):SetText(Translate('ms_match_breakdown_item_match_victory') .. ':')
	else
		GetWidget('rewardstat_label_fin_xp_desc'):SetText(Translate('ms_match_breakdown_item_match_finished') .. ':')
	end
	
end

function RankProgressionAndHeroMasteryChart(perf_wins_delta, mastery_exp_original, mastery_exp_match, mastery_maxlevel_addon, mastery_exp_boost, mastery_exp_to_boost, mastery_exp_super_boost)

	GetWidget('rewardstat_label_heromastery_totalexp'):SetText(mastery_exp_original + mastery_exp_match + mastery_maxlevel_addon + mastery_exp_boost + mastery_exp_super_boost)
	GetWidget('rewardstat_label_heromastery_matchesexp'):SetText(mastery_exp_match)
	GetWidget('rewardstat_label_heromastery_bonusexp'):SetText(mastery_maxlevel_addon)
	if tonumber(mastery_exp_boost) > 0 then
		GetWidget('rewardstat_label_heromastery_boostexp'):SetColor('#d6b7a4')
	else
		GetWidget('rewardstat_label_heromastery_boostexp'):SetColor('#463830')
	end
	GetWidget('rewardstat_label_heromastery_boostexp'):SetText(mastery_exp_boost + mastery_exp_super_boost)

end

function RewardStat_GameTime(self, param5)

	local str = '0:00:00' 
	local p5 = tonumber(param5)
	
	if NotEmpty(p5) then
		if (p5 and (p5 > 0)) then
			str = FtoT(param5, 2)
		end
	end
	
	self:SetText(str)

end

function HeroModelPanel(heroEntity, avatar)

	local modelTable = GetHeroPreviewDataFromDB(heroEntity, NotEmpty(avatar) and avatar or heroEntity, 'large')
	local ambient = modelTable.ambient
	local sunColor = modelTable.sunColor
	local sunAngle = modelTable.sunAngle
	local sunHeight = modelTable.sunHeight
	
	local modelPanel = GetWidget('rewardstat_modelpanel_widget')
	modelPanel:SetModel(modelTable.modelPath)
	modelPanel:SetEffect(modelTable.effectPath)
	modelPanel:SetModelScale(modelTable.modelScale * 3.6)
	modelPanel:SetAmbientColor(ambient.x, ambient.y, ambient.z)
	modelPanel:SetSunColor(sunColor.x, sunColor.y, sunColor.z)
	modelPanel:SetSunPosition(sunHeight, sunAngle)
	
	
	
end

function RewardStat_HideShowXPBarAndLevel(param)

	if (NotEmpty(param)) then
		GetWidget('rewardstat_player_level'):SetVisible(1)
		GetWidget('rewardstat_player_panel_accountexp'):SetVisible(1)
	else
		GetWidget('rewardstat_player_level'):SetVisible(0)
		GetWidget('rewardstat_player_panel_accountexp'):SetVisible(0)	
	end

end

function WinningTeamModel(winningTeam, team, nickname, matchOwner)

	if  (NotEmpty(matchOwner) and nickname == matchOwner) or
		(not NotEmpty(matchOwner) and IsMe(nickname))
	then
		if winningTeam == team then
			Trigger('ModelWin')
		else
			Trigger('ModelLoose')
		end
	end

end

function AwardSpecial(position, seed)

	if (seed) and (position) and (tSpecialAwards) and (#tSpecialAwards > 0) then
		local awardPosition = (((seed) * position) % #tSpecialAwards) + 1
		--printdb('searching for award at ' .. awardPosition .. '. Table has ' .. #tSpecialAwards)
		local award = tSpecialAwards[awardPosition]
		
		if (award) then
			tremove(tSpecialAwards, awardPosition)
			--printdb('SPECIAL Player: ' .. position .. ' | Awarded: ' .. tostring(award) .. ' at award position ' .. awardPosition .. ' using seed: ' .. seed )
			return award
		else
			--printdb('^r UI: Special awards table has no entry at ' .. awardPosition .. ' using seed: ' .. seed .. ' for player ' .. position )
			return 0
		end
	else
		--printdb('^r UI: Special awards failed ' .. tostring(seed) .. ' | ' .. tostring(position) .. ' | '  .. tostring(tSpecialAwards) .. ' | '  .. tostring(#tSpecialAwards > 0) )
		return 0
	end
	
end

function RankingWidgetsReset()

	--Sets up widget displays for ranking	
	GetWidget('rewardstat_placement_widgets'):SetVisible(0)
	GetWidget('rewardstat_rankplacement_circle'):SetTexture('/ui/fe2/NewUI/Res/match_stats/rings/rank_bg.png')
	GetWidget('rewardstat_rankplacement_circle'):SetWidth('256i')
	GetWidget('rewardstat_rankplacement_circle'):SetHeight('256i')
	GetWidget('rewardstat_rankplacement_circle'):SetY('0')
	GetWidget('rewardstat_rankplacement_circle'):SetVisible(1)
	GetWidget('rewardstat_panel_rankprogression'):SetVisible(1)
	GetWidget('rewardstat_label_ranklabel'):SetVisible(1)
	GetWidget('rewardstat_bar_rankprogression'):SetVisible(1)
	GetWidget('rewardstat_placements_bg'):SetVisible(1)

end

function PlacementWidgetsReset()
	GetWidget('rewardstat_rankplacement_circle'):SetVisible(1)
	GetWidget('rewardstat_placement_widgets'):SetVisible(1)
	GetWidget('rewardstat_placements_bg'):SetTexture('/ui/fe2/season/nolevel.tga')
	GetWidget('rewardstat_label_ranklabel'):SetVisible(0)
	GetWidget('rewardstat_label_ranklabel'):SetText('')
	GetWidget('rewardstat_panel_rankprogression'):SetVisible(0)
	
	GetWidget('rewardstat_rankplacement_circle'):SetTexture('/ui/fe2/NewUI/Res/match_stats/placements/placement_bg.png')
	GetWidget('rewardstat_rankplacement_circle'):SetWidth('238i')
	GetWidget('rewardstat_rankplacement_circle'):SetHeight('238i')
	GetWidget('rewardstat_rankplacement_circle'):SetY('7i')
	GetWidget('rewardstat_placements_bg'):SetVisible(0)
	
	--Resets Color & Images
	for i=1,6 do
		GetWidget('rewardstat_placementsico_' .. i):SetVisible(1)
		GetWidget('rewardstat_placementspie_' .. i):SetVisible(1)
		GetWidget('rewardstat_placementsico_' .. i):SetTexture('/ui/fe2/NewUI/Res/match_stats/placements/' .. i .. 'th.png')
		GetWidget('rewardstat_placementsico_' .. i):SetColor('1 1 1 1')
		GetWidget('rewardstat_placementsico_'..i):ScaleWidth('64i', 0)
		GetWidget('rewardstat_placementsico_'..i):ScaleHeight('64i', 0)
	end
	
	for i=2,6 do
		GetWidget('rewardstat_placementspie_' .. i):SetValue(0)
		lib_Anim.RewardStat_PieGraphAnim(i, 'stop')
	end
	
end

function RewardStatsEmulator_RankCalc(mmr_before, mmr_after, medal_before, medal_after, placement_wins_data, placement_matches, season_id)
	
	-- [Initial Data] --
	local mmr_before = tonumber(mmr_before)
	local mmr_after = tonumber(mmr_after)
	local medal_before = tonumber(medal_before)
	local medal_after = tonumber(medal_after)
	
	
	local minMMR = 0
	local maxMMR = 0
	if tonumber(season_id) > 6 and tonumber(season_id) < 1000 then
		minMMR, maxMMR = GetMMRByRankLevelAfterS6(medal_before)
	else
		minMMR, maxMMR = GetMMRByRankLevel(medal_before)
	end
	
	local currentMMR = mmr_before
	local rate = (currentMMR - minMMR) / (maxMMR - minMMR)

	local function RankLoopCalculate(condition)
		
		--Runs only once when the animation loop is called, sets base lua cvars
		if condition == 'start' then
			Trigger('RankUpStop')
			medalcalc = medal_before
			mmrcalc = mmr_before
			percentcalc = rate
		else
			--player is ranking up
			if mmr_before < mmr_after then
				if currentMMR >= mmr_after then
					return
				else
					if tonumber(season_id) > 6 and tonumber(season_id) < 1000 then
						if IsMaxRankLevelAfterS6(medalcalc) then return end
					else
						if IsMaxRankLevel(medalcalc) then return end
					end
				end
				mmrcalc = mmrcalc+0.1
				mmrcalc = mmrcalc > mmr_after and mmr_after or mmrcalc
			--player is deranking
			elseif mmr_before > mmr_after then
				if currentMMR <= mmr_after then
					Echo('Debug: Finished ranking down.')
					return
				end
				mmrcalc = mmrcalc-0.1
				mmrcalc = mmrcalc < mmr_after and mmr_after or mmrcalc
			else
				--mmr before and after is identical, ends function instantly as a safeguard, this situation should not occur
				return
			end
		end
		
		local minMMR = 0
		local maxMMR = 0
		if tonumber(season_id) > 6 and tonumber(season_id) < 1000 then
			minMMR, maxMMR = GetMMRByRankLevelAfterS6(medalcalc)
		else
			minMMR, maxMMR = GetMMRByRankLevel(medalcalc)
		end
		
		currentMMR = mmrcalc
		
		--mmr is now below minimum, so the player levels down and sets the new minimum/maximum mmr for next level.
		if currentMMR < minMMR then
			currentMMR = minMMR
			medalcalc = medalcalc-1
			if tonumber(season_id) > 6 and tonumber(season_id) < 1000 then
				_, mmrcalc = GetMMRByRankLevelAfterS6(medalcalc)
			else
				_, mmrcalc = GetMMRByRankLevel(medalcalc)
			end
		end
		
		--mmr is at max level, so the player levels up and sets the new minimum/maximum mmr for next level.
		if currentMMR > maxMMR then
			currentMMR = maxMMR
			medalcalc = medalcalc+1
			if tonumber(season_id) > 6 and tonumber(season_id) < 1000 then
				mmrcalc, _ = GetMMRByRankLevelAfterS6(medalcalc)
			else
				mmrcalc, _ = GetMMRByRankLevel(medalcalc)
			end
			Trigger('RankUpVisual', '/ui/fe2/newui/res/match_stats/effects/player_season_grade_up.effect')
		end
		
		--algo for current rank progress %
		percentcalc = (currentMMR - minMMR) / (maxMMR - minMMR)	
		interface:Sleep(8, function() RewardStatsEmulator_RankEcho(medalcalc, percentcalc, season_id) RankLoopCalculate() end)
	end
	
	local function PlacementLoopCalculate(condition)
		
		if condition == 'start' then
		
			--Widgets reset for placement setup
			PlacementWidgetsReset()
			
			--Table setup for placement handle
			matchPlacements = {}		
			for i=1, string.len(placement_wins_data) do
				table.insert(matchPlacements, string.sub(placement_wins_data, i, i))
			end
			
			local placement_matches = tonumber(placement_matches)
			
			if placement_matches == 1 then
				GetWidget('rewardstat_placementsico_' .. '1'):Sleep(400, function() lib_Anim.RewardStat_Pulse('1', matchPlacements[1], 280) end)
			elseif placement_matches == 2 then
				lib_Anim.RewardStat_Pulse('1', matchPlacements[1], 0) 
				lib_Anim.RewardStat_PieGraphAnim('2')
				GetWidget('rewardstat_placementsico_' .. '2'):Sleep(400, function() lib_Anim.RewardStat_Pulse('2', matchPlacements[2], 280) end)
			elseif placement_matches == 3 then
				lib_Anim.RewardStat_Pulse('1', matchPlacements[1], 0) 
				lib_Anim.RewardStat_Pulse('2', matchPlacements[2], 0) 
				lib_Anim.RewardStat_PieGraphAnim('2', 'instant')
				lib_Anim.RewardStat_PieGraphAnim('3')
				GetWidget('rewardstat_placementsico_' .. '3'):Sleep(400, function() lib_Anim.RewardStat_Pulse('3', matchPlacements[3], 280) end)
			elseif placement_matches == 4 then
				lib_Anim.RewardStat_Pulse('1', matchPlacements[1], 0) 
				lib_Anim.RewardStat_Pulse('2', matchPlacements[2], 0) 
				lib_Anim.RewardStat_Pulse('3', matchPlacements[3], 0) 
				lib_Anim.RewardStat_PieGraphAnim('2', 'instant')
				lib_Anim.RewardStat_PieGraphAnim('3', 'instant')
				lib_Anim.RewardStat_PieGraphAnim('4')
				GetWidget('rewardstat_placementsico_' .. '4'):Sleep(400, function() lib_Anim.RewardStat_Pulse('4', matchPlacements[4], 280) end)
			elseif placement_matches == 5 then
				lib_Anim.RewardStat_Pulse('1', matchPlacements[1], 0) 
				lib_Anim.RewardStat_Pulse('2', matchPlacements[2], 0) 
				lib_Anim.RewardStat_Pulse('3', matchPlacements[3], 0) 
				lib_Anim.RewardStat_Pulse('4', matchPlacements[4], 0) 
				lib_Anim.RewardStat_PieGraphAnim('2', 'instant')
				lib_Anim.RewardStat_PieGraphAnim('3', 'instant')
				lib_Anim.RewardStat_PieGraphAnim('4', 'instant')
				lib_Anim.RewardStat_PieGraphAnim('5')
				GetWidget('rewardstat_placementsico_' .. '5'):Sleep(400, function() lib_Anim.RewardStat_Pulse('5', matchPlacements[5], 280) end)
			elseif placement_matches == 6 then
				lib_Anim.RewardStat_Pulse('1', matchPlacements[1], 0) 
				lib_Anim.RewardStat_Pulse('2', matchPlacements[2], 0) 
				lib_Anim.RewardStat_Pulse('3', matchPlacements[3], 0) 
				lib_Anim.RewardStat_Pulse('4', matchPlacements[4], 0) 
				lib_Anim.RewardStat_Pulse('5', matchPlacements[5], 0) 
				lib_Anim.RewardStat_PieGraphAnim('2', 'instant')
				lib_Anim.RewardStat_PieGraphAnim('3', 'instant')
				lib_Anim.RewardStat_PieGraphAnim('4', 'instant')
				lib_Anim.RewardStat_PieGraphAnim('5', 'instant')
				lib_Anim.RewardStat_PieGraphAnim('6')
				GetWidget('rewardstat_placementsico_' .. '6'):Sleep(400, function() lib_Anim.RewardStat_Pulse('6', matchPlacements[6], 280) end)
				GetWidget('rewardstat_panel_rankprogression'):Sleep(1500, function()
				
				--all 6 placement matches complete, widgets fade out
				GetWidget('rewardstat_placement_widgets'):FadeOut(500)
				GetWidget('rewardstat_rankplacement_circle'):FadeOut(500)
				GetWidget('rewardstat_panel_rankprogression'):Sleep(800, function()
					
					--placement frame transforms to rank frame
					GetWidget('rewardstat_rankplacement_circle'):SetTexture('/ui/fe2/NewUI/Res/match_stats/rings/rank_bg.png')
					GetWidget('rewardstat_rankplacement_circle'):SetWidth('256i')
					GetWidget('rewardstat_rankplacement_circle'):SetHeight('256i')
					GetWidget('rewardstat_rankplacement_circle'):SetY('0')
					GetWidget('rewardstat_reward_parent'):Sleep(100, function()
							GetWidget('rewardstat_rankplacement_circle'):FadeIn(300)
							GetWidget('rewardstat_panel_rankprogression'):SetVisible(1)
							GetWidget('rewardstat_label_ranklabel'):FadeIn(300)
							GetWidget('rewardstat_bar_rankprogression'):FadeIn(300)
							if tonumber(season_id) > 6 and tonumber(season_id) < 1000 then
								GetWidget('rewardstat_label_ranklabel'):SetText(Translate('player_compaign_level_name_S7_'..tostring(medal_after)))
							else
								GetWidget('rewardstat_label_ranklabel'):SetText(Translate('player_compaign_level_name_'..tostring(medal_after)))
							end
							GetWidget('rewardstat_placements_bg'):FadeIn(300)
							
							--calculate and set the current percentage instantly to achieved level
							local minMMR, maxMMR = GetMMRByRankLevel(medal_after)
							local currentMMR = mmr_after
							local rate = (currentMMR - minMMR) / (maxMMR - minMMR)
							
							GetWidget('rewardstat_bar_rankprogression'):SetValue(rate)

							if medal_after == 0 then
								GetWidget('rewardstat_placements_bg'):SetTexture('/ui/fe2/season/nolevel.tga')
							else
								if tonumber(season_id) > 6 and tonumber(season_id) < 1000 then
									GetWidget('rewardstat_placements_bg'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevelAfterS6(tonumber(medal_after)))
								else
									GetWidget('rewardstat_placements_bg'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevel(tonumber(medal_after)))
								end
							end
							
						end)
				
					end)

				end)
			else
				--Echo('stringlen is 0 for placements data')
			end
			
		end
		
		
		
	end
	
	--Just placed all 6 matches
	local placement_matches = tonumber(placement_matches)
	if placement_matches == 6 then
		PlacementLoopCalculate('start')
	--Show rank up! player is not in placements
	elseif placement_matches > 6 then
		RankLoopCalculate('start')
	--Player is in placements
	elseif placement_matches < 6 then
		PlacementLoopCalculate('start')
	end
	
end

function RewardStatsEmulator_RankEcho(medal, percent, season_id)
	
	RankingWidgetsReset()
	
	--Rank piegraph progression and rank label animation
	GetWidget('rewardstat_bar_rankprogression'):SetValue(percent)
	if tonumber(season_id) > 6 and tonumber(season_id) < 1000 then
		GetWidget('rewardstat_placements_bg'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevelAfterS6(tonumber(medal)))
		if IsMaxRankLevelAfterS6((medal)) then
			GetWidget('rewardstat_label_ranklabel'):SetText(Translate('player_compaign_level_name_S7_'..tostring(20)))
			GetWidget('rewardstat_placements_bg'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevelAfterS6(tonumber(medal)))
		else
			GetWidget('rewardstat_label_ranklabel'):SetText(Translate('player_compaign_level_name_S7_'..tostring(medal)))
			GetWidget('rewardstat_placements_bg'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevelAfterS6(tonumber(medal)))
		end
	else
		GetWidget('rewardstat_placements_bg'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevel(tonumber(medal)))
		if IsMaxRankLevel((medal)) then
			GetWidget('rewardstat_label_ranklabel'):SetText(Translate('player_compaign_level_name_'..tostring(17)))
			GetWidget('rewardstat_placements_bg'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevel(tonumber(medal)))
		else
			GetWidget('rewardstat_label_ranklabel'):SetText(Translate('player_compaign_level_name_'..tostring(medal)))
			GetWidget('rewardstat_placements_bg'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevel(tonumber(medal)))
		end
	end
end

function RewardStat_CTFTitleAndMVP(mvp, team, position, heroName, heroIcon, totalscore, captures)
	
	local captures = tonumber(captures)
	
	if tonumber(team) == 1 then
		GetWidget('rewardstat_ctf_playerMVP_blue'):SetText(mvp)
		GetWidget('rewardstat_ctf_playerMVP_heroname_blue'):SetText(heroName)
		GetWidget('rewardstat_ctf_playerMVP_heroicon_blue'):SetTexture(heroIcon)
		GetWidget('rewardstats_ctf_bluetotalflags'):SetText(totalscore)
		GetWidget('rewardstats_ctf_bluemvpflags'):SetText(captures)
		GetWidget('ctfEndSplashSide1Player1Award1Icon'):SetTexture('/ui/icons/awards/award_'..matchStatsGetAssignedAward(position, '1')..'.tga')
		GetWidget('ctfEndSplashSide1Player1Award2Icon'):SetTexture('/ui/icons/awards/award_'..matchStatsGetAssignedAward(position, '2')..'.tga')
	elseif tonumber(team) == 2 then
		GetWidget('rewardstat_ctf_playerMVP_yellow'):SetText(mvp)
		GetWidget('rewardstat_ctf_playerMVP_heroname_yellow'):SetText(heroName)
		GetWidget('rewardstat_ctf_playerMVP_heroicon_yellow'):SetTexture(heroIcon)
		GetWidget('rewardstats_ctf_yellowtotalflags'):SetText(totalscore)
		GetWidget('rewardstats_ctf_yellowmvpflags'):SetText(captures)
		GetWidget('ctfEndSplashSide2Player1Award1Icon'):SetTexture('/ui/icons/awards/award_'..matchStatsGetAssignedAward(position, '1')..'.tga')
		GetWidget('ctfEndSplashSide2Player1Award2Icon'):SetTexture('/ui/icons/awards/award_'..matchStatsGetAssignedAward(position, '2')..'.tga')
	end
	
end

function RewardStat_CTFCalculation(nickname, arg5, arg19, gameplaystat2, gameplaystat4, gameplaystat3, team, position, heroName, heroIcon)
	
	--CTF Locals
	local ctf_kills = arg5	-- Kills
	local ctf_assists = arg19	-- Assists
	local ctf_captures = gameplaystat2	-- Captures
	local ctf_returns = gameplaystat4	-- Returns
	local ctf_carrierkills = gameplaystat3	-- Carrier kills
	
	local function calcScore(nickname, ctf_kills, ctf_assists, ctf_captures, ctf_returns, ctf_carrierkills, team, heroName, heroIcon)		
		
		local totalScore = (
			ctf_kills +
			(ctf_assists * 0.5) +
			(ctf_captures * 15) +
			(ctf_returns * 5) +
			(ctf_carrierkills * 5)
		)
		
		if tonumber(team) == 1 then
			table.insert(match_StatsScoreT1, {id = nickname, score = totalScore, heroN = heroName, heroImg = heroIcon, captures = ctf_captures, pos = position})
		elseif tonumber(team) == 2 then
			table.insert(match_StatsScoreT2, {id = nickname, score = totalScore, heroN = heroName, heroImg = heroIcon, captures = ctf_captures, pos = position})
		end
		
	end
	
	calcScore(				 
				nickname, 
				ctf_kills,		-- Kills
				ctf_assists,	-- Assists
				ctf_captures,	-- Captures
				ctf_returns,	-- Returns
				ctf_carrierkills, 	-- Carrier kills
				team, 
				heroName, 
				heroIcon
			)
	
	
end

function RewardStat_SoccerTitleAndMVP(mvp, team, heroName, heroIcon, totalscore, goals)
	
	local goals = tonumber(goals)
	
	if tonumber(team) == 1 then
		GetWidget('rewardstat_soccer_playerMVP_blue'):SetText(mvp)
		GetWidget('rewardstat_soccer_playerMVP_heroname_blue'):SetText(heroName)
		GetWidget('rewardstat_soccer_playerMVP_heroicon_blue'):SetTexture(heroIcon)
		GetWidget('rewardstats_soccer_bluetotalgoals'):SetText(totalscore)
		GetWidget('rewardstats_soccer_bluemvpgoals'):SetText(goals)
	elseif tonumber(team) == 2 then
		GetWidget('rewardstat_soccer_playerMVP_yellow'):SetText(mvp)
		GetWidget('rewardstat_soccer_playerMVP_heroname_yellow'):SetText(heroName)
		GetWidget('rewardstat_soccer_playerMVP_heroicon_yellow'):SetTexture(heroIcon)
		GetWidget('rewardstats_soccer_yellowtotalgoals'):SetText(totalscore)
		GetWidget('rewardstats_soccer_yellowmvpgoals'):SetText(goals)
	end
	
end

function RewardStat_SoccerCalculation(nickname, gameplaystat0, gameplaystat3, gameplaystat1, gameplaystat6, team, heroName, heroIcon)
	
	--Soccer Locals
	local paramGoals = gameplaystat0
	local paramSteals = gameplaystat3
	local paramAssists = gameplaystat1
	local paramKills = gameplaystat6
	
	local function calcScore(nickname, paramGoals, paramSteals, paramAssists, paramKills, team, heroName, heroIcon)		
		
		local totalScore = (
		(paramGoals 	* 	10)	+
		(paramSteals 	* 	1) 	+
		(paramAssists	* 	5) 	+
		(paramKills 	* 	2)
		)
		
		if tonumber(team) == 1 then
			table.insert(match_StatsScoreT1, {id = nickname, score = totalScore, heroN = heroName, heroImg = heroIcon, goals = paramGoals})
		elseif tonumber(team) == 2 then
			table.insert(match_StatsScoreT2, {id = nickname, score = totalScore, heroN = heroName, heroImg = heroIcon, goals = paramGoals})
		end
		
	end
	
	calcScore(				 
				nickname, 
				paramGoals, 
				paramSteals, 
				paramAssists, 
				paramKills, 
				team, 
				heroName, 
				heroIcon
			)
	
	
end

function RewardStat_SubmitPTMFeedback(param1, param2, param3, param4, param5, param6, param7)

	local fmatchID = tonumber(param1)
	local fbackstring = tostring(EscapeString(param2))
	local starRating = tostring(param3)
	local fTypeHeroes = tostring(param4)
	local fTypeItems = tostring(param5)
	local fTypeMaps = tostring(param6)
	local fTypeOther = tostring(param7)
	
	SendRebornFeedback(fmatchID, EscapeString(fbackstring), starRating, fTypeHeroes, fTypeItems, fTypeMaps, fTypeOther)

end

local function MatchInfoPlayer(triggerIndex, sourceWidget,
	nickname, team, position, hero_level, herokills, teamcreepkills, experience, gold, actions, denies, heroIcon,
	inventory1, inventory2, inventory3, inventory4, inventory5, inventory6, deaths, heroassists, heroName, heroEntity,
	account_rating_old, account_rating_delta, perf_victory_exp, perf_victory_gc, perf_first_exp, perf_first_gc, map_name, perf_social_bonus_gc, perf_consec_played, perf_consec_exp,
	perf_consec_gc, perf_annihilation_exp, perf_annihilation_gc, perf_bloodlust_exp, perf_bloodlust_gc, perf_ks15_exp, perf_ks15_gc, perf_wins, perf_wins_delta, perf_wins_gc,
	perf_herokills, perf_herokills_delta, perf_herokills_gc, perf_heroassists, perf_heroassists_delta, perf_heroassists_gc, perf_wards, perf_wards_delta, perf_wards_gc, perf_smackdown,
	perf_smackdown_delta, perf_smackdown_gc, account_level_old, account_exp, account_exp_delta, levelup_coins, tag, stats_games_played, avg_k_d, avg_a_d,
	avgGoldPerMin, avgExpPerMin, k_d, a_d, goldPerMin, expPerMin, consumables, gold_spent, herodmg,
	bdmg, razed, time_earning_exp, secs_dead, wards, bloodlust, doublekill, triplekill, quadkill, annihilation,
	ks3, ks4, ks5, ks6, ks7, ks8, ks9, ks10, ks15, smackdown,
	humiliation, nemesis, retribution, neutralcreepkills, goldlost2death, heroexp, mid_games_played, mid_discos, limited_stats_coins, current_time, legacy_bonus_gc, perf_bots_gc,
	bot_games_played, riftwars_games_played, tokens_won, matches_until_tokens, perf_multiplier_mmpoints, perf_multiplier_exp, gameplaystat0, gameplaystat1, gameplaystat2, gameplaystat3, gameplaystat4, gameplaystat5, gameplaystat6, gameplaystat7, gameplaystat8, gameplaystat9, class, winningTeam,
	mastery_exp_original, mastery_exp_match, mastery_exp_bonus, mastery_exp_boost, mastery_exp_to_boost, mastery_canboost, mastery_boostnum, mastery_matchid, mastery_boost_product_id, mastery_boost_gold_coin, mastery_boost_silver_coin,
	season_diamond, cur_diamond, chest_price1, chest_price2, chest_price3, netcafe_coin, mastery_maxlevel_herocount, mastery_maxlevel_addon, ...)
	
	if team == '0' then
		return 
	end
	
	local isCTF = (map_name == 'capturetheflag')
	local isSoccer = (map_name == 'soccer')
	
	if matchStats.repeatCheck == false then
		--Echo('repeatcheck: ' .. arg[21] .. '|' .. nickname)
		Match_Stats_V2:OnClickMainTab('stats')
		RewardStat_DisableRewardsTabHandle(1)
		matchStats.repeatCheck = true
		
		--Resets
		match_StatsScoreT1 = {}
		match_StatsScoreT2 = {}
		match_StatsTotalScoreT1 = 0
		match_StatsTotalScoreT2 = 0
		for i=2,5 do
			GetWidget('rewardstat_ctf_otherplayerinstance_1_' .. i):SetVisible(0)
			GetWidget('rewardstat_ctf_otherplayerinstance_2_' .. i):SetVisible(0)
			GetWidget('rewardstat_soccer_otherplayerinstance_1_' .. i):SetVisible(0)
			GetWidget('rewardstat_soccer_otherplayerinstance_2_' .. i):SetVisible(0)
		end
	end
	
	--Handle for displaying CTF Rewards (instead of detailed stats, simple stats will still work.)
	if (isCTF) then
		GetWidget('rewardstat_detailedstats_standard'):SetVisible(0)
		GetWidget('rewardstat_detailedstats_ctf'):SetVisible(1)
		GetWidget('rewardstat_detailedstats_soccer'):SetVisible(0)
		if tonumber(team) == 1 then
			RewardStat_CTFCalculation(nickname, arg[5], arg[19], gameplaystat2, gameplaystat4, gameplaystat3, team, position, heroName, heroIcon)
			match_StatsTotalScoreT1 = match_StatsTotalScoreT1+gameplaystat2
			GetWidget('rewardstats_ctf_blueteam_panel'):Sleep(1, function()
				totalplayers = #match_StatsScoreT1
				table.sort(match_StatsScoreT1, function(a, b) return a.score > b.score end)	
				for i=2,#match_StatsScoreT1 do
					GetWidget('rewardstat_ctf_otherplayerinstance_1_' .. i):SetVisible(1)
					GetWidget('rewardstat_ctf_otherplayerinstance_name_1_' .. i):SetText(match_StatsScoreT1[i].id)
					GetWidget('rewardstat_ctf_otherplayerinstance_flags_1_' .. i):SetText(tonumber(match_StatsScoreT1[i].captures))
					GetWidget('rewardstat_ctf_otherplayerinstance_hero_1_' .. i):SetText(match_StatsScoreT1[i].heroN)
					GetWidget('rewardstat_ctf_otherplayerinstance_heroico_1_' .. i):SetTexture(match_StatsScoreT1[i].heroImg)
					GetWidget('rewardstat_ctf_playeraward_1_'..i..'_'..team):SetTexture('/ui/icons/awards/award_'..matchStatsGetAssignedAward(match_StatsScoreT1[i].pos, '1')..'.tga')
					GetWidget('rewardstat_ctf_playeraward_2_'..i..'_'..team):SetTexture('/ui/icons/awards/award_'..matchStatsGetAssignedAward(match_StatsScoreT1[i].pos, '2')..'.tga')
				end
				RewardStat_CTFTitleAndMVP(
						match_StatsScoreT1[1].id, 
						team, 
						match_StatsScoreT1[1].pos,
						match_StatsScoreT1[1].heroN, 
						match_StatsScoreT1[1].heroImg, 
						match_StatsTotalScoreT1, 
						match_StatsScoreT1[1].captures
					)
			end)
		end
		if tonumber(team) == 2 then
			RewardStat_CTFCalculation(nickname, arg[5], arg[19], gameplaystat2, gameplaystat4, gameplaystat3, team, position, heroName, heroIcon)
			match_StatsTotalScoreT2 = match_StatsTotalScoreT2+gameplaystat2
			GetWidget('rewardstats_ctf_yellowteam_panel'):Sleep(1, function()
				totalplayers = #match_StatsScoreT2
				table.sort(match_StatsScoreT2, function(a, b) return a.score > b.score end)	
				for i=2,#match_StatsScoreT2 do
					GetWidget('rewardstat_ctf_otherplayerinstance_2_' .. i):SetVisible(1)
					GetWidget('rewardstat_ctf_otherplayerinstance_name_2_' .. i):SetText(match_StatsScoreT2[i].id)
					GetWidget('rewardstat_ctf_otherplayerinstance_flags_2_' .. i):SetText(tonumber(match_StatsScoreT2[i].captures))
					GetWidget('rewardstat_ctf_otherplayerinstance_hero_2_' .. i):SetText(match_StatsScoreT2[i].heroN)
					GetWidget('rewardstat_ctf_otherplayerinstance_heroico_2_' .. i):SetTexture(match_StatsScoreT2[i].heroImg)
					GetWidget('rewardstat_ctf_playeraward_1_'..i..'_'..team):SetTexture('/ui/icons/awards/award_'..matchStatsGetAssignedAward(match_StatsScoreT2[i].pos, '1')..'.tga')
					GetWidget('rewardstat_ctf_playeraward_2_'..i..'_'..team):SetTexture('/ui/icons/awards/award_'..matchStatsGetAssignedAward(match_StatsScoreT2[i].pos, '2')..'.tga')
				end
				RewardStat_CTFTitleAndMVP(
						match_StatsScoreT2[1].id, 
						team, 
						match_StatsScoreT2[1].pos,
						match_StatsScoreT2[1].heroN, 
						match_StatsScoreT2[1].heroImg, 
						match_StatsTotalScoreT2, 
						match_StatsScoreT2[1].captures
					)
			end)
		end
	elseif (isSoccer) then
		GetWidget('rewardstat_detailedstats_standard'):SetVisible(0)
		GetWidget('rewardstat_detailedstats_ctf'):SetVisible(0)
		GetWidget('rewardstat_detailedstats_soccer'):SetVisible(1)
		OnSoccerMatchInfoPlayer(tonumber(triggerIndex), nickname, tonumber(team), position, heroName, heroIcon, gameplaystat0, gameplaystat1, gameplaystat2, 
			gameplaystat3, gameplaystat4, gameplaystat5, gameplaystat6, gameplaystat7, gameplaystat8, gameplaystat9)
	else
		GetWidget('rewardstat_detailedstats_standard'):SetVisible(1)
		GetWidget('rewardstat_detailedstats_ctf'):SetVisible(0)
		GetWidget('rewardstat_detailedstats_soccer'):SetVisible(0)
	end
	
	--Reward Functions
	if NotEmpty(position) then
	
		--Set Winning Team Label
		matchStats.winningTeam = winningTeam
	
		Echo('Prepare to show graffiti ~~~~' .. team..','..winningTeam)
		if (NotEmpty(arg[21]) and tostring(UIGetAccountID()) == arg[21]) or IsMe(nickname) then
			GetWidget('match_stats'):Sleep(500, function()
				if team == winningTeam then Echo('Call HoN_Codex:PostGamePopup') HoN_Codex:PostGamePopup(arg[27]) end
				HoN_Codex.autopopup = false
			end)
		end

		--if (NotEmpty(arg[21]) and tostring(UIGetAccountID()) == arg[21]) or IsMe(nickname) or rewardstats_emulator == 1 then
		local matchOwner = GetCvarString('_playerstats_match_owner')
		if (NotEmpty(matchOwner) and IsMe(matchOwner) and IsMe(nickname)) or 
			(not NotEmpty(matchOwner) and IsMe(nickname))
			or rewardstats_emulator == 1 
		then
			-- Echo('^gmatchOwner = '..matchOwner..'^*')
			-- Echo('^gIsMe = '..tostring(IsMe(matchOwner))..'^*')
			-- Echo('^gemulator = '..tostring(rewardstats_emulator)..'^*')
			-- Echo('^gmatchStats.repeatCheck = '..tostring(matchStats.repeatCheck)..'^*')
		
			RewardStat_DisableRewardsTabHandle(0)
		
			Echo('^gmatchStats_RewardsDisabled = '..tostring(matchStats_RewardsDisabled)..'^*')
			
			if not isSoccer and not isCTF then
				Match_Stats_V2:OnClickMainTab('rewards')
			end
			
			local 	perf_victory_gc, perf_first_gc, perf_social_bonus_gc, perf_consec_gc, perf_annihilation_gc, 
					perf_bloodlust_gc, perf_ks15_gc, perf_wins_gc, perf_herokills_gc, perf_heroassists_gc, 
					perf_wards_gc, perf_smackdown_gc, perf_level_gc, mid_games_played, limited_stats_coins, 
					levelup_coins, legacy_bonus_gc, perf_bots_gc, bot_games_played, perf_multiplier_mmpoints 
						= 	tonumber(perf_victory_gc) or 0, tonumber(perf_first_gc) or 0, tonumber(perf_social_bonus_gc) or 0,
							tonumber(perf_consec_gc) or 0, tonumber(perf_annihilation_gc) or 0, tonumber(perf_bloodlust_gc) or 0, 
							tonumber(perf_ks15_gc) or 0, tonumber(perf_wins_gc) or 0, tonumber(perf_herokills_gc) or 0, 
							tonumber(perf_heroassists_gc) or 0, tonumber(perf_wards_gc) or 0, tonumber(perf_smackdown_gc) or 0,
							tonumber(perf_level_gc) or 0, tonumber(mid_games_played) or 0, tonumber(limited_stats_coins) or 0,
							tonumber(levelup_coins) or 0, tonumber(legacy_bonus_gc) or 0, tonumber(perf_bots_gc) or 0,
							tonumber(bot_games_played) or 0, tonumber(perf_multiplier_mmpoints) or 0
			local 	perf_total_silver_new = ceil(legacy_bonus_gc + levelup_coins + perf_bloodlust_gc + perf_annihilation_gc +
											perf_ks15_gc + limited_stats_coins + perf_victory_gc + perf_first_gc + perf_social_bonus_gc +
											perf_consec_gc + perf_wins_gc + perf_herokills_gc + perf_heroassists_gc + perf_wards_gc +
											perf_smackdown_gc + perf_level_gc + perf_bots_gc + perf_multiplier_mmpoints + tonumber(arg[1]))
			local 	perf_victory_exp, perf_first_exp, perf_social_bonus_exp, perf_consec_exp, perf_annihilation_exp, perf_bloodlust_exp,
					perf_ks15_exp, perf_wins_exp, perf_herokills_exp, perf_heroassists_exp, perf_wards_exp, perf_smackdown_exp, perf_level_exp
						= tonumber(perf_victory_exp) or 0, tonumber(perf_first_exp) or 0, tonumber(perf_social_bonus_exp) or 0,
						tonumber(perf_consec_exp) or 0, tonumber(perf_annihilation_exp) or 0, tonumber(perf_bloodlust_exp) or 0,
						tonumber(perf_ks15_exp) or 0, tonumber(perf_wins_exp) or 0, tonumber(perf_herokills_exp) or 0,
						tonumber(perf_heroassists_exp) or 0, tonumber(perf_wards_exp) or 0, tonumber(perf_smackdown_exp) or 0,
						tonumber(perf_level_exp) or 0
			local perf_total_experience_new = ceil(perf_bloodlust_exp + perf_annihilation_exp + perf_ks15_exp  + perf_victory_exp +
												perf_first_exp + perf_social_bonus_exp + perf_consec_exp + perf_wins_exp +
												perf_herokills_exp + perf_heroassists_exp + perf_wards_exp + perf_smackdown_exp
												+ perf_level_exp + tonumber(arg[2]))
			local perf_tokens_earned = ceil(tokens_won)
			local perf_matches_until_tokens = ceil(matches_until_tokens)
			
			local masteryLevel = GetMasteryLevelByExp(mastery_exp_match)
			local ranking = arg[19]
			local medal_after = arg[17]
			
			local mastery_super_canboost = arg[24]
			local mastery_super_boostnum = arg[25]
			local mastery_exp_super_boost = arg[26]
			local mastery_exp_event = arg[28]
			
			local placement_wins_data = arg[22]
			
			local placement_matches = arg[20]
			
			Echo('ranking: ' .. arg[19])
			
			matchStats.playerTokensEarned = perf_tokens_earned
			matchStats.playerMatchesUntilTokens = perf_matches_until_tokens
			
			-- [Resets] --
			matchStats_extraInfo = 0
			matchStats.isanyStats = 0
			matchStats_maxleveladdon = 0
			matchStats_matchExp = 0
			matchStats_bonusExp = 0
			matchStats_eventBonus = 0
			matchStats_maxleveladdonPercent = 0
			matchStats_maxlevelcount = 0
			matchStats_ToBoostExp = 0
			matchStats_pieGraph = 0
			matchStats_BoostExp = 0
			matchStats_SuperBoostExp = 0
			matchStats_boostNum = 0
			matchStats_SuperBoostNum = 0
			matchStats_boostCostGold = 0
			matchStats_boostCostSilver = 0
			matchStats_superBoostCost = 0
			
			-- [If no account XP is Recorded, hides the bar and level, else shows it] --
			RewardStat_HideShowXPBarAndLevel(account_level_old)
			
			-- [Checks if mastery match, and if so checks if ranking match as well] --		
			MatchStatsLayoutHandle(perf_victory_exp, perf_victory_gc, mastery_exp_match, mastery_maxlevel_addon, mastery_exp_bonus,
				arg[28], mastery_maxlevel_herocount, arg[17], arg[20], mastery_exp_to_boost, mastery_exp_boost, mastery_exp_super_boost)
			
			-- [Mastery Boosts] --
			if tonumber(arg[25]) ~= 0 then
				GetWidget('rewardstat_label_mastery_superboosts'):SetText('x^o' .. arg[25])
				GetWidget('rewardstat_regularboostpanel'):SetY('0h')
				GetWidget('rewardstat_superboostpanel'):SetVisible(1)
			else
				GetWidget('rewardstat_regularboostpanel'):SetY('-25%')
				GetWidget('rewardstat_superboostpanel'):SetVisible(0)
			end			
			
			if tonumber(mastery_exp_boost) > 0 then
				local text = Translate('mastery_replay_main_masteryboost_text3')
				GetWidget('rewardstat_label_mastery_boosts_desc'):SetText(string.upper(text))
				GetWidget('rewardstat_label_mastery_boosts'):SetVisible(0)
				for i=1,3 do
					GetWidget('rewardstat_buyboost_regular_' .. i):SetVisible(0)
				end				
			elseif tonumber(mastery_canboost) == 1 then
				local text = Translate('mastery_replay_main_masteryboost_text4')
				GetWidget('rewardstat_label_mastery_boosts_desc'):SetText(string.upper(text))
				GetWidget('rewardstat_label_mastery_boosts'):SetVisible(0)
				for i=1,3 do
					GetWidget('rewardstat_buyboost_regular_' .. i):SetVisible(0)
				end				
			else
				if tonumber(mastery_boostnum) ~= 0 then
					local text = Translate('mastery_replay_main_masteryboost')
					GetWidget('rewardstat_label_mastery_boosts_desc'):SetText(string.upper(text))
					GetWidget('rewardstat_label_mastery_boosts'):SetText('x^o' .. mastery_boostnum)
					GetWidget('rewardstat_label_mastery_boosts'):SetVisible(1)
					for i=1,3 do
						GetWidget('rewardstat_buyboost_regular_' .. i):SetVisible(0)
					end					
				else
					local text1 = Translate('mastery_replay_main_masteryboost_text2')
					local text2 = Translate('mastery_replay_main_boosttip_title')
					GetWidget('rewardstat_label_mastery_boosts_desc'):SetText(string.upper(text1 .. ' ' .. text2))
					GetWidget('rewardstat_label_mastery_boosts'):SetVisible(0)
					for i=1,3 do
						GetWidget('rewardstat_buyboost_regular_' .. i):SetVisible(1)
					end
				end
			end
			
			RewardStat_PlayerEXPAnimation2(nickname, account_exp, account_exp_delta, account_level_old, account_rating_old, account_rating_delta)
			-- [Player Name] - [Account Level] - [Account Icon] -- [Account Extra Icon] -- [Account Experience]
			--RewardStat_PlayerEXPAnimation(experience_total_next_level, experience_total_current_level, perf_total_experience_new, account_level_old, nickname, experience_into_current_level, account_rating_old, account_rating_delta)

			-- [Resets for MVP Awards] --
			MVPAwardReset()
			
			-- [ *SLEEP DATA* Account Icon, Chat Symbol, MVP Banner, Player Name, Legacy Bonus, MVP Awards]
			AccChatIconMVPBannerPlayerNameLegacyBonusMVPAwards(legacy_bonus_gc, annihilation, quadkill, ks3, ks4, ks5, ks6, ks7, ks8, ks9, ks10, ks15, perf_smackdown_delta, herokills, heroassists, deaths, herodmg, teamcreepkills, bdmg, perf_multiplier_mmpoints)
			
			-- [Hero Match Inventory] --
			HeroMatchInventory(inventory1, inventory2, inventory3, inventory4, inventory5, inventory6)
				
			-- [Hide Empty Award] --
			EmptyAward(perf_bloodlust_exp, perf_bloodlust_gc, perf_ks15_exp, perf_ks15_gc, perf_annihilation_exp, perf_annihilation_gc)
			
			-- [Update Player Breakdown] --
			FuncUpdatePlayerBreakdownItem(perf_wins_gc, perf_herokills_gc, perf_heroassists_gc, perf_wards_gc, perf_smackdown_gc, 
				perf_wins_exp, perf_herokills_exp, perf_heroassists_exp, perf_wards_exp, perf_smackdown_exp, 
				perf_wins_delta, perf_herokills_delta, perf_heroassists_delta, perf_wards_delta, perf_smackdown_delta, 
				perf_wins, perf_herokills, perf_heroassists, perf_wards, perf_smackdown)
			
			-- [Level/KDA/GPM] --
			LevelKDAGPM(hero_level, herokills, deaths, heroassists, gold)
			
			-- [Total Earned, Game A Day, Social Bonus, Consecutive Bonus, Match Finished/Victory] --
			TotalEarnedGameADaySocialBonusConsecutiveBonusMatchVictory(perf_total_experience_new, perf_total_silver_new, perf_victory_exp, perf_victory_gc, perf_first_exp, perf_first_gc, perf_social_bonus_gc, perf_consec_gc, team, winningTeam)			

			RewardStatsEmulator_RankCalc(arg[14], arg[15], arg[16], arg[17], placement_wins_data, placement_matches, arg[30])
			
			-- [Rank Progression / Hero Mastery Chart] --
			RankProgressionAndHeroMasteryChart(perf_wins_delta, mastery_exp_original, mastery_exp_match, mastery_maxlevel_addon, mastery_exp_boost, mastery_exp_to_boost, mastery_exp_super_boost)
			
			-- [Mastery XP Animation] --
			RewardStat_MasteryXPAnimation(heroIcon, mastery_exp_original, mastery_exp_match, mastery_maxlevel_addon, mastery_exp_boost, mastery_exp_to_boost, mastery_canboost, mastery_super_canboost, mastery_boostnum, mastery_matchid, mastery_boost_product_id, mastery_boost_gold_coin, mastery_boost_silver_coin, mastery_exp_super_boost, mastery_super_boostnum, mastery_exp_event)
			
			-- [Hero Model Panel] --
			HeroModelPanel(heroEntity, arg[29])
			
			--End (NotEmpty)
		end
		
		--Player Awards
		local function CalcAward(awardName, awardVar, position, showMin, showMax, nonZero, awardGroup)
		
			if (awardName) and (awardVar) and (showMin or showMax) then
				local awardVar = tonumber(awardVar) or 0

				if (awardGroup) then
					matchStats.tAwardsGroup[awardGroup] = matchStats.tAwardsGroup[awardGroup] or {}
				end

				matchStats.tMatchInfoPlayer[awardName] = matchStats.tMatchInfoPlayer[awardName] or {}

				if (not nonZero) or (awardVar and (awardVar > 0)) then

					if (matchStats.tAwardsGroup[awardGroup]) then
						local canGetAward = true
						for i, v in ipairs(matchStats.tAwardsGroup[awardGroup]) do
							if (v == position) then
								canGetAward = false
								break
							end
						end
						if (canGetAward) then
							matchStats.tMatchInfoPlayer[awardName][awardVar] = position
							tinsert(matchStats.tAwardsGroup[awardGroup], position)
						end
					else
						matchStats.tMatchInfoPlayer[awardName][awardVar] = position
					end

				end

				--printdb('^y awardName: ' .. awardName .. ' | awardVar: ' .. awardVar )

				local tPositions = {}
				for val, pos in pairsByKeys(matchStats.tMatchInfoPlayer[awardName]) do
					tinsert(tPositions, pos)
				end

				--printdb('^g awardName: ' .. awardName)
				--Echo('^g awardName: ' .. awardName)
				--Echo('^r MatchInfoPlayer:')
				--printTable(matchStats.tMatchInfoPlayer[awardName])
				--Echo('^r tPositions:')
				--printTable(tPositions)

				if (#tPositions > 0) then
					if (showMin) and (#tPositions > 1) then
						matchStats.tAwardsPlayer[tPositions[1]] = matchStats.tAwardsPlayer[tPositions[1]] or {}
						tinsert(matchStats.tAwardsPlayer[tPositions[1]],  awardName..'_l')
					end
					if (showMax) then
						matchStats.tAwardsPlayer[tPositions[#tPositions]] = matchStats.tAwardsPlayer[tPositions[#tPositions]] or {}
						tinsert(matchStats.tAwardsPlayer[tPositions[#tPositions]], awardName..'_m')
					end
					tPositions = nil
				else
					--printdb('^r awardName: ' .. awardName)
					--printTable(matchStats.tMatchInfoPlayer[awardName])
				end
				
				
				
			end
		end
		
		numPlayers = numPlayers + 1
		matchStats.tAwardsPlayer = {}
		matchStats.tAwardsGroup = {}
		
		local level = hero_level
		
		if isSoccer then
		-- specail match stats calculations for soccer
			CalcAward('soccergoals', 			gameplaystat0, 			position, 	false, 	true,	true)
			CalcAward('soccergoalassists', 		gameplaystat1, 			position, 	false, 	true,	true)
			CalcAward('soccerselfgoals', 		gameplaystat2, 			position, 	false, 	true,	true)
			CalcAward('soccersteals', 			gameplaystat3, 			position, 	false, 	true,	true)
			CalcAward('soccertimewithballs',	gameplaystat4, 			position, 	true, 	true,	false)
			CalcAward('soccerbeknockedouts', 	gameplaystat5, 			position, 	false, 	true,	true)
			CalcAward('soccerknockouts',		gameplaystat6, 			position, 	false, 	true,	true)
			CalcAward('soccerrunesgathered', 	gameplaystat7, 			position, 	false, 	true,	true)
		end			
		if isCTF then
			--[[
				gameplaystat0		-- unused
				gameplaystat1		-- unused
				gameplaystat2		-- captures
				gameplaystat3		-- returns
				gameplaystat4		-- carrier kills
				gameplaystat5		-- distance with flag
				gameplaystat6		-- damage to carriers
			--]]
			-- local function CalcAward(awardName, awardVar, position, showMin, showMax, nonZero, awardGroup)

			CalcAward('ctfcaptures', 		gameplaystat2, 			position, 	false, 	true,	false)
			CalcAward('ctfdistance', 		gameplaystat5, 			position, 	false, 	true,	false)

			CalcAward('ctfcarrierdamage', 		gameplaystat6, 			position, 	true, 	true,	true)
			CalcAward('ctfreturns', 		gameplaystat3, 			position, 	false, 	true,	false)
			CalcAward('ctfdeaths', 			deaths, 				position, 	false, 	true,	false)
		end

		CalcAward('ks15', 			ks15, 				position, 	false, 	true, 	true, 	'ks')
		CalcAward('ks10', 			ks10, 				position, 	false, 	true, 	true, 	'ks')

		CalcAward('herokills', 		herokills, 			position, 	false, 	true,	true)
		if (not isCTF) or (not isSoccer) then
			CalcAward('deaths', 		deaths, 			position, 	true, 	false,	false)
		end
		CalcAward('heroassists', 	heroassists, 		position, 	true, 	true,	false)

		CalcAward('annihilation', 	annihilation, 		position, 	false, 	true, 	true,	'multi')
		CalcAward('quadkill', 		quadkill, 			position, 	false, 	true, 	true,	'multi')
		CalcAward('triplekill', 	triplekill, 		position, 	false, 	true, 	true,	'multi')
		CalcAward('doublekill', 	doublekill, 		position, 	false, 	true, 	true,	'multi')

		CalcAward('consumables', 	consumables, 		position, 	true, 	true,	true)
		CalcAward('level', 			level, 				position, 	false, 	true,	true)
		CalcAward('gold', 			gold, 				position, 	false, 	true,	true, 	'gold')
		CalcAward('gold_spent', 	gold_spent, 		position, 	true, 	true,	true, 	'gold')

		CalcAward('herodmg', 		herodmg, 			position, 	true, 	true,	false)
		CalcAward('bdmg', 			bdmg, 				position, 	true, 	true,	false)

		CalcAward('bloodlust', 		bloodlust, 			position, 	false, 	true,	true)
		CalcAward('humiliation', 	humiliation, 		position, 	false, 	true, 	true)
		CalcAward('smackdown', 		smackdown, 			position, 	false, 	true, 	true)
		CalcAward('nemesis', 		nemesis, 			position, 	false, 	true, 	true)
		CalcAward('retribution', 	retribution, 		position, 	false, 	true, 	true)

		CalcAward('actions', 		actions, 			position, 	true, 	true,	true)

		CalcAward('razed', 			razed, 				position, 	true, 	true,	false)
		CalcAward('teamcreepkills', teamcreepkills, 	position, 	true, 	true, 	false)
		CalcAward('denies', 		denies, 			position, 	true, 	true, 	false)
		CalcAward('neutcreepkills', neutralcreepkills, 	position, 	true, 	true, 	false)

		CalcAward('wards', 			wards, 				position, 	false, 	true, 	true)

		CalcAward('ks5', 			ks5, 				position, 	false, 	true, 	true, 	'ks')
		CalcAward('ks3', 			ks3, 				position, 	false, 	true, 	true, 	'ks')

		CalcAward('secs_dead', 		secs_dead, 			position, 	true, 	false,	false)
		CalcAward('avg_xpm', 		experience, 		position, 	false, 	true, 	true)	-- this is account experience
		
		tSpecialAwards = {1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20}
		
		--GetWidget('rewardstat_detailedstats_ctf'):Sleep(1, function()
		
		for playerIndex = 0,9,1 do
			local awardTable = matchStats.tAwardsPlayer[tostring(playerIndex)]
			local widgetIndex = playerIndex --- 1
			
			local function IncreaseNum(widgetIndex)
				if widgetIndex > 4 then
					return '_2'
				else
					return '_1'
				end
			end
			
			local function DecreaseNum(widgetIndex)
				if widgetIndex >= 5 then
					return (widgetIndex - 5)
				else
					return widgetIndex
				end
			end
			local match_id = matchStats_match_id
			
			if GetWidget('rewardstats_player_award_1_'..DecreaseNum(widgetIndex) .. IncreaseNum(widgetIndex)) then
				if (awardTable) and (awardTable[1]) then
					--Echo('Player: ' .. playerIndex .. ' | Awarded: ^g' .. tostring(awardTable[1]) .. ' at widget ' .. tostring(widgetIndex) )
					GetWidget('rewardstats_player_award_1_'..DecreaseNum(widgetIndex) .. IncreaseNum(widgetIndex)):SetTexture('/ui/icons/awards/award_'..awardTable[1] ..'.tga')
					GetWidget('rewardstats_player_award_1_'..DecreaseNum(widgetIndex) .. IncreaseNum(widgetIndex)):SetCallback('onmouseover', function(self)
						local title = Translate('match_awards_' .. awardTable[1])
						local desc = Translate('match_awards_' .. awardTable[1] .. '_desc')
						Common_V2:ShowGenericTip(true, title, desc, '300i')
					end)
					matchStats.tAssignedAwardsPlayer['1'][tostring(playerIndex)] = awardTable[1]
				else
					local award = AwardSpecial(playerIndex, tonumber(string.sub(match_id, string.len(match_id),  string.len(match_id))) )
					--Echo('Player: ' .. playerIndex .. ' | Awarded: ^y' .. tostring(award) .. ' at widget ' .. tostring(widgetIndex))
					GetWidget('rewardstats_player_award_1_'..DecreaseNum(widgetIndex) .. IncreaseNum(widgetIndex)):SetTexture('/ui/icons/awards/award_' .. 'special_' ..award ..'.tga')
					GetWidget('rewardstats_player_award_1_'..DecreaseNum(widgetIndex) .. IncreaseNum(widgetIndex)):SetCallback('onmouseover', function(self)
						local title = Translate('match_awards_special_' .. award)
						local desc = Translate('match_awards_special_' .. award .. '_desc')
						Common_V2:ShowGenericTip(true, title, desc, '300i')
					end)
					matchStats.tAssignedAwardsPlayer['1'][tostring(playerIndex)] = 'special_'..award
				end
			end
			if GetWidget('rewardstats_player_award_2_'..DecreaseNum(widgetIndex) .. IncreaseNum(widgetIndex)) then
				if (awardTable) and (awardTable[2])  then
					--printdb('Player: ' .. playerIndex .. ' | Awarded: ^g' .. tostring(awardTable[2]) .. ' at widget ' .. tostring(widgetIndex))
					GetWidget('rewardstats_player_award_2_'..DecreaseNum(widgetIndex) .. IncreaseNum(widgetIndex)):SetTexture('/ui/icons/awards/award_'..awardTable[2] ..'.tga')						
					GetWidget('rewardstats_player_award_2_'..DecreaseNum(widgetIndex) .. IncreaseNum(widgetIndex)):SetCallback('onmouseover', function(self)
						local title = Translate('match_awards_' .. awardTable[2])
						local desc = Translate('match_awards_' .. awardTable[2] .. '_desc')
						Common_V2:ShowGenericTip(true, title, desc, '300i')
					end)
					matchStats.tAssignedAwardsPlayer['2'][tostring(playerIndex)] = awardTable[2]
				else
					local award = AwardSpecial(playerIndex, tonumber(string.sub(match_id, string.len(match_id),  string.len(match_id))) + 1)
					--printdb('Player: ' .. playerIndex .. ' | Awarded: ^y' .. tostring(award) .. ' at widget ' .. tostring(widgetIndex))
					GetWidget('rewardstats_player_award_2_'..DecreaseNum(widgetIndex) .. IncreaseNum(widgetIndex)):SetTexture('/ui/icons/awards/award_' .. 'special_' ..award ..'.tga')
					GetWidget('rewardstats_player_award_2_'..DecreaseNum(widgetIndex) .. IncreaseNum(widgetIndex)):SetCallback('onmouseover', function(self)
						local title = Translate('match_awards_special_' .. award)
						local desc = Translate('match_awards_special_' .. award .. '_desc')
						Common_V2:ShowGenericTip(true, title, desc, '300i')
					end)
					matchStats.tAssignedAwardsPlayer['2'][tostring(playerIndex)] = 'special_'..award
				end
			end
		end
		
		-- [Detailed Stats MVP] -- (since I use index 0 to 4, and then team 2 indext 0 to 4 again for team positions, I made this system, makes text glow for now)
		if tonumber(position) >= 5 then
			WinningTeamModel(winningTeam, team, nickname, matchOwner)
			GetWidget('rewardstat_general_label_player_name_2_' .. triggerIndex):Sleep(1, 
				function() 
					RewardStat_PlayerMVPMedals(arg[21], triggerIndex, '2', 0)
					RewardStat_SetPlayerBG(arg[21], triggerIndex, '2', nickname, matchOwner)
					if arg[21] == matchStats.mvpID then
						GetWidget('rewardstat_mvpbanner_2_' .. triggerIndex):SetVisible(1)
					else
						GetWidget('rewardstat_mvpbanner_2_' .. triggerIndex):SetVisible(0)
					end
				end)
		else
			WinningTeamModel(winningTeam, team, nickname, matchOwner)
			GetWidget('rewardstat_general_label_player_name_1_' .. triggerIndex):Sleep(1, 
				function()
					RewardStat_PlayerMVPMedals(arg[21], triggerIndex, '1', 0)
					RewardStat_SetPlayerBG(arg[21], triggerIndex, '1', nickname, matchOwner)
					if arg[21] == matchStats.mvpID then
						GetWidget('rewardstat_mvpbanner_1_' .. triggerIndex):SetVisible(1)
					else
						GetWidget('rewardstat_mvpbanner_1_' .. triggerIndex):SetVisible(0)
					end
				end)
		end
		
		--End MatchInfoPlayer
	end
	
end

function RewardStat_OnOpen()
	GetWidget('match_stats'):RegisterWatch('EndUpdate', OnEndUpdate)
end

function RewardStat_OnClose()
	Set('ui_match_stats_waitingToShow', 'false', 'bool')
	GetWidget('match_stats'):UICmd([=[ClearWaitingToShowStats()]=])
	Set('_match_stats_parent_panel', '')
end

local function OnMatchCoNRewards(...)
	local old_lvl			= arg[1] or '0'
	if old_lvl == '-1' then return end
	
	local curr_lvl 			= arg[2] or '0'
	local next_lvl 			= arg[3] or '0'
	local games_req 		= arg[4] or '0'
	local rank_req 			= arg[5] or '0'
	local percent_before 	= arg[6] or '0'
	local percent_after 	= arg[7] or '0'
	
	Echo('^gCoN Rewards Info: '..old_lvl..','..curr_lvl..','..next_lvl..','..games_req..','..rank_req..','..percent_before..','..percent_after..'^*')
	
	if true then
--		test data for con reward not available
--		old_lvl			= '-2'
--		curr_lvl 		= ''
--		next_lvl 		= ''
--		games_req 		= ''
--		rank_req 		= ''
--		percent_before 	= ''
--		percent_after 	= ''

--		test data for the very beginning with game required
--		old_lvl			= '0'
--		curr_lvl 		= '0'
--		next_lvl 		= '1'
--		games_req 		= '1'
--		rank_req 		= '0'
--		percent_before 	= '0'
--		percent_after 	= '0'

--		test data for the very beginning with rank required
--		old_lvl			= '0'
--		curr_lvl 		= '0'
--		next_lvl 		= '1'
--		games_req 		= '100'
--		rank_req 		= '1'
--		percent_before 	= '0'
--		percent_after 	= '0'

--		test data for percentage change in the same level		
--		old_lvl			= '1'
--		curr_lvl 		= '1'
--		next_lvl 		= '2'
--		games_req 		= '3'
--		rank_req 		= '0'
--		percent_before 	= '0.2'
--		percent_after 	= '0.6'

--		test data for percentage change and 1 level up		
--		old_lvl			= '2'
--		curr_lvl 		= '3'
--		next_lvl 		= '3'
--		games_req 		= '50'
--		rank_req 		= '0'
--		percent_before 	= '0.2'
--		percent_after 	= '1.3'
	
--		test data for percentage change and multiple levels up		
--		old_lvl			= '2'
--		curr_lvl 		= '4'
--		next_lvl 		= '3'
--		games_req 		= '100'
--		rank_req 		= '0'
--		percent_before 	= '0.2'
--		percent_after 	= '2.3'
	
--		test data for rank required
--		old_lvl			= '4'
--		curr_lvl 		= '5'
--		next_lvl 		= '6'
--		games_req 		= '0'
--		rank_req 		= '1'
--		percent_before 	= '0.2'
--		percent_after 	= '1.0'
	
--		test data for level up to the top reward
--		old_lvl			= '5'
--		curr_lvl 		= '6'
--		next_lvl 		= '0'
--		games_req 		= '0'
--		rank_req 		= '0'
--		percent_before 	= '0.5'
--		percent_after 	= '1.0'
	
--		test data for already reached the top reward
--		old_lvl			= '6'
--		curr_lvl 		= '6'
--		next_lvl 		= '0'
--		games_req 		= '0'
--		rank_req 		= '0'
--		percent_before 	= '1.0'
--		percent_after 	= '1.0'
	end
	
	local container = GetWidget('rewardstat_rankprogression_info_panel')
	if container == nil then return end
	
	-- con reward info is available or not
	if old_lvl == '-2' then
		container:GetWidget('rewardstat_rankprogression_noinfo'):SetVisible(1)
		container:GetWidget('rewardstat_rankprogression_info_expbar_body'):SetVisible(0)
		return
	else
		container:GetWidget('rewardstat_rankprogression_noinfo'):SetVisible(0)
		container:GetWidget('rewardstat_rankprogression_info_expbar_body'):SetVisible(1)
	end
	
	-- chest icon
	old_lvl  = tonumber(old_lvl)
	curr_lvl = tonumber(curr_lvl)
	next_lvl = tonumber(next_lvl)
	container:GetWidget('rewardstat_rankprogression_info_chest'):SetTexture('/ui/fe2/newui/res/playerstats/conrewards/chest'..tostring(old_lvl)..'.png')
	
	-- progression
	percent_before = math.ceil(tonumber(percent_before) * 100)
	percent_after = math.ceil(tonumber(percent_after) * 100)
	
	if (percent_before < 0 and percent_after < 0 and curr_lvl == 0) then
		container:GetWidget('rewardstat_rankprogression_info_expbar_before'):SetWidth('0%')
		container:GetWidget('rewardstat_rankprogression_info_expbar_after'):SetWidth('0%')
		container:GetWidget('rewardstat_rankprogression_info_expvalue'):SetText('0%')
		container:GetWidget('rewardstat_rankprogression_info_expbar_body'):SetVisible(0)
		games_req = tonumber(games_req) > 0 and games_req or 'N/A'
		container:GetWidget('rewardstat_label_conrewards_desc'):SetText(Translate('mastery_replay_rewardtab_con_desc_needmoregame', 'games', games_req))
		container:GetWidget('rewardstat_label_conrewards_desc'):SetVisible(1)
	else
		container:GetWidget('rewardstat_rankprogression_info_expbar_before'):SetWidth(tostring(percent_before)..'%')
		container:GetWidget('rewardstat_rankprogression_info_expbar_after'):SetWidth(tostring(percent_before)..'%')
		container:GetWidget('rewardstat_rankprogression_info_expvalue'):SetText(tostring(percent_before)..'%')
		container:GetWidget('rewardstat_rankprogression_info_expbar_body'):SetVisible(1)
		container:GetWidget('rewardstat_label_conrewards_desc'):SetVisible(0)
		
		local function CoNRewardExpBarPeriodCallback(curPercentage)
			container:GetWidget('rewardstat_rankprogression_info_expvalue'):SetText(tostring(math.ceil(curPercentage))..'%')
		end
		
		local function CoNRewardExpBarLevelupCallback(curPercentage)
			container:GetWidget('rewardstat_rankprogression_info_expbar_before'):SetWidth('0%')
			container:GetWidget('rewardstat_rankprogression_levelup_effect'):SetEffect('/ui/fe2/NewUI/Res/match_stats/effects/player_rewards.effect')
			if old_lvl < curr_lvl then
				old_lvl = old_lvl + 1
				old_lvl = old_lvl < curr_lvl and old_lvl or curr_lvl
			
				container:GetWidget('rewardstat_rankprogression_info_chest'):SetTexture('/ui/fe2/newui/res/playerstats/conrewards/chest'..tostring(old_lvl)..'.png')
			else
				container:GetWidget('rewardstat_rankprogression_info_chest'):SetTexture('/ui/fe2/newui/res/playerstats/conrewards/chest'..tostring(curr_lvl)..'.png')
			end
		end
		
		local function CoNRewardExpBarEndCallback()
			if next_lvl == 0 then
				container:GetWidget('rewardstat_label_conrewards_desc'):SetText(Translate('mastery_replay_rewardtab_con_desc_topreward'))
				container:GetWidget('rewardstat_rankprogression_info_expbar_before'):SetWidth('100%')
				container:GetWidget('rewardstat_rankprogression_info_expbar_after'):SetWidth('100%')
				container:GetWidget('rewardstat_rankprogression_info_expvalue'):SetText('100%')
			elseif tonumber(rank_req) > 0 then
				container:GetWidget('rewardstat_label_conrewards_desc'):SetText(Translate('mastery_replay_rewardtab_con_desc_needrankup'))
			else
				games_req = tonumber(games_req) > 0 and games_req or 'N/A'
				container:GetWidget('rewardstat_label_conrewards_desc'):SetText(Translate('mastery_replay_rewardtab_con_desc_needmoregame', 'games', games_req))
			end
			container:GetWidget('rewardstat_label_conrewards_desc'):SetVisible(1)
		end
		
		AnimateProgressBar('rewardstat_rankprogression_info_expbar_after',
			percent_before, percent_after, 1, 50, 
			CoNRewardExpBarPeriodCallback, CoNRewardExpBarLevelupCallback, CoNRewardExpBarEndCallback)
	end
	--Echo('End of OnMatchCoNRewards!')
end

interface:RegisterWatch('MatchCoNRewards', function(_, ...) OnMatchCoNRewards(...) end)

------------------------------------------------------------------------------------------------------------------------

function ODRE()
	if OnDemandReplaysEnabled() then
		return 1
	else
		return 0
	end
end

function RPU(param)
	local replayParam = param
	if tonumber(replayParam) <= 0 then
		return 1
	else
		return 0
	end
end

function RewardStat_ReplayLabel()

	if tonumber(matchStats_ReplayURLStatus) ~= 1 then
		if ODRE() == 1 then
			if NotEmpty(matchStats_ReplayUnav) then
				if tonumber(matchStats_ReplayURLStatus) == 4 then
					if matchStats_Compat1 == 'true' and matchStats_Compat2 == 'false' then
						return 'game_end_stats_compat_btn'
					else
						return 'game_end_stats_watch_replay_btn'
					end
				else
					if tonumber(matchStats_ReplayUnav) <= 0 then
						return 'game_end_stats_refresh_btn'
					else
						if tonumber(matchStats_ReplayURLStatus) == 2 then
							return 'game_end_stats_download_btn'
						else
							return 'game_end_stats_request_btn'
						end
					end
				end
			else
				return 'game_end_stats_refresh_btn'
			end
		else
			return 'game_end_stats_download_unav_btn'
		end
	elseif tonumber(matchStats_ReplayURLStatus) ~= 4 then
		return 'game_end_stats_refresh_btn'
	else
		return 'game_end_stats_download_unav_btn'
	end

end

function RewardStats_ReplayButton()
	
	if RewardStat_ReplayLabel() == 'game_end_stats_refresh_btn' then
		RewardStat_GetMatchInfo(GetCvarString('_stats_last_match_id'))
		Trigger('ModelReset')
	else
		if tonumber(matchStats_ReplayURLStatus) == 4 and ODRE() == 1 and NotEmpty(matchStats_ReplayUnav) then
			if matchStats_Compat1 == 'true' and matchStats_Compat2 == 'false' then
				local compatUrl = GetCvarString('_stats_last_replay_version')		
				interface:UICmd("DownloadCompat('"..compatUrl.."');") 
			else
				local replayPath = GetCvarString('_stats_last_replay_path')
				interface:UICmd("StartReplay('"..replayPath.."');")
				GetWidget('match_stats'):SetVisible(0)
			end
		else
			if tonumber(matchStats_ReplayURLStatus) ~= 1 and ODRE() == 1 and NotEmpty(matchStats_ReplayUnav) and tonumber(matchStats_ReplayURLStatus) ~= 2 then
				RewardStats_RequestUpload(GetCvarString('_stats_last_replay_id'))
			elseif tonumber(matchStats_ReplayURLStatus) ~= 1 and ODRE() == 1 and NotEmpty(matchStats_ReplayUnav) and tonumber(matchStats_ReplayURLStatus) == 2 then				
				local downloadUrl = GetCvarString('_stats_last_replay_url')		
				interface:UICmd("DownloadReplay('"..downloadUrl.."');")	
				GetWidget('main_stats_download_box'):SetVisible(1)
				GetWidget('download_progress_bar'):SetVisible(1)
				--GetWidget('cancel_download_btn'):SetVisible(1)
			else
				--Match was not avaliable
			end
		end
	end

end

function RewardStat_ReplayHandle()
	
	--Echo('matchStats_ReplayURLStatus: [' .. matchStats_ReplayURLStatus .. ']')
	--Echo('OnDemandReplaysEnabled(): [' .. ODRE() .. ']')
	--Echo('matchStats_ReplayUnav: [' .. matchStats_ReplayUnav .. ']')
	--Echo('matchStats_Compat1: [' .. matchStats_Compat1 .. ']')
	--Echo('matchStats_Compat2: [' .. matchStats_Compat2 .. ']')
	
	for i=1,3 do
		local widgetLabel = GetWidget('rewardstat_replaybutton_' .. i)
		local widgetImg = GetWidget('rewardstat_replaybutton_image_' .. i)
		local str = RewardStat_ReplayLabel()		
		widgetLabel:SetText(Translate(str))
		
		if str == 'game_end_stats_download_unav_btn' then
			widgetImg:SetRenderMode('grayscale')
			widgetLabel:SetColor('gray')
		else
			widgetImg:SetRenderMode('normal')
			widgetLabel:SetColor('#ba9280')
		end
		
	end

end

function RewardStats_RequestUpload(matchID)
	
	if (not matchStats.pendingReplays[matchID]) then
		RequestSMUpload(matchID, 'honreplay')
	end
	
end

function RewardStats_UpdateOnDemandLabel()
	local matchID = GetCvarString('_stats_last_replay_id')
	local str = Translate('game_end_stats_request_btn')

	if (matchStats.pendingReplays[matchID]) then
		local state = matchStats.pendingReplays[matchID].updateType
		if ((state == 0) or (state == 1) or (state == 2) or (state == 5) or (state == 6)) then
			str = Translate('match_stats_ondemand_'..state)
		end
	end

	GetWidget('rewardstat_replaybutton'):SetVisible(0)
	GetWidget('rewardstat_replay_uploadinglabel'):SetVisible(1)
	GetWidget('rewardstat_replay_uploadinglabel'):SetText(str)
	
	-- groupfcall('game_end_request_group', function(_,w,_) w:SetText(str) end)
	
end


local function ProcessPendingDownload(_, matchID, updateType, downloadLink)
	-- UpdateTypes
	-- -1 None
	--  0 General Failure
	--  1 Does Not Exist
	--  2 Invalid Host
	--  3 Already Uploaded
	--  4 Already Queued
	--  5 Queued
	--  6 Uploading
	--  7 Upload Complete
	updateType = tonumber(updateType)

	matchStats.pendingReplays[matchID] = {
		['updateType'] = tonumber(updateType),
		['downloadLink'] = downloadLink,
	}

	if (matchID == GetCvarString('_stats_last_replay_id')) then
		RewardStats_UpdateOnDemandLabel()

		if (GetWidget('match_stats'):IsVisible() and ((updateType == 7) or (updateType == 3))) then
			interface:UICmd("ClearEndGameStats();")
			RewardStat_GetMatchInfo(matchID)
			GetWidget('rewardstat_replay_uploadinglabel'):SetVisible(0)
			GetWidget('rewardstat_replaybutton'):SetVisible(1)
		end
	end

	-- only save updates of these types, otherwise we want the ability to rerequest the file
	if (not ((updateType == 1) or (updateType == 5) or (updateType == 6))) then
		matchStats.pendingReplays[matchID] = nil
	end
end

interface:RegisterWatch('UploadReplayStatus', ProcessPendingDownload)


local playerstats = {}
playerstats[1] = {}
playerstats[2] = {}
local playerstatslocations = {}
local havelocations = false
local sortDirection = true
local lastSortedStat = ""
--playerstats[team][playerid]
--playerid is 1 to 5

for i in pairs({1,2}) do
	for j=1,5 do
		local index = (i-1)*5+j-1
		interface:RegisterWatch('MatchInfoPlayer'.. tostring(index), function(_, ...)
			MatchInfoPlayer(index, self, ...)
			local arg={...}
			if arg[2] ~= nil and tonumber(arg[2]) ~= 0 then
				--Keys from match_stats_v2.package template rewardstat_detailedstats_team_instance
				playerstats[tonumber(arg[2])][j] = {
					rank=tonumber(arg[155]),
					lvl=tonumber(arg[4]),
					kda=tonumber(arg[5]),
					deaths=tonumber(arg[6]),
					defense=tonumber(arg[10]),
					ep=tonumber(arg[7]),
					gold=tonumber(arg[8]),
					a=tonumber(arg[9]),
					ward=tonumber(arg[75]),
					index=j-1
				}
				if havelocations then
					for k=0,4 do
						widget = interface:GetWidget("rewardstat_detailedstats_player" .. k .. "_" .. arg[2])
						widget:SetY(tostring(playerstatslocations[k+1]))
					end
				end
			end
		end)
	end
end

function Match_Stats_V2:SortStats(team, stat)
	if stat == lastSortedStat then
		sortDirection = not sortDirection
	else
		sortDirection = true
	end
	lastSortedStat = stat
	local copy = {}
	--local copy = {unpack(playerstats[team])}
	for i, player in pairs(playerstats[team]) do
		if player == nil or player[stat] == nil then
			--continue
		else
			copy[i] = {}
			for key,value in pairs(player) do
				copy[i][key] = value
			end
			--copy[i] = {unpack(player)}
		end
	end
	if not havelocations then
		for i=0,4 do
			playerstatslocations[i+1] = interface:GetWidget("rewardstat_detailedstats_player" .. i .. "_" .. team):GetY()
		end
		havelocations = true
	end
	table.sort(copy, function(a, b) if sortDirection then return a[stat] < b[stat] else return a[stat] > b[stat] end end)

	for i, player in pairs(copy) do
		widget = interface:GetWidget("rewardstat_detailedstats_player" .. player.index .. "_" .. team)
		widget:SetY(tostring(playerstatslocations[i]))
	end
end


--End