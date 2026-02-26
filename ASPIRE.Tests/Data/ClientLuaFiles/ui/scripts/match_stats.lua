----------------------------------------------------------
--	Name: 		Match Stats Script	            		--
--  Copyright 2015 Frostburn Studios					--
----------------------------------------------------------

--[[ RMM To Do
	- animation timer TL -> BR
	- levelup effect on level number
	- question mark tooltips, use milestone data
]]--

Set('_stats_nav_to_match_stats', 'false', 'bool')

local ANIM_XP_PER_SECOND = 500
local ANIM_BAR_GROW_TIME = 1000
local MS_ANIM_DELAY = 100

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
matchStats.accountTotalSilver = 0
matchStats.winningTeam = 0
matchStats.newlyEarnedRewards = nil
matchStats.MINIMUM_UPCOMING = 3
matchStats.MAXIMUM_ITEM_CARDS = 150
matchStats.isAutoLookup = false
matchStats.hasRewardData = false
matchStats.hasMasteryData = false
matchStats.hasRankData = false
matchStats.debugNewRewards = false
matchStats.isBots = false
matchStats.playerTokensEarned = 0
matchStats.playerMatchesUntilTokens = 0
matchStats.match_id = -1

Match_Stats.MasteryInfo = {}
Match_Stats.OtherPlayerMatch = false

Match_Stats.RankInfo = {}

function matchStatsPrint()
	printTable2(matchStats)
end

function matchStatsGetAssignedAward(position, awardID)	-- both strings
	return matchStats.tAssignedAwardsPlayer[awardID][position]
end



function matchStatsGetMatchID()
	return matchStats.match_id
end

matchStats.pendingReplays = {}

RegisterScript2('Match_Stats', '33')
local tSpecialAwards

function matchStatsResetSpecialAwards()
	tSpecialAwards = {1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20}
end

matchStats.templateOverride = {
	[1] = {
		[1] = 'ms_rewards_reward_page_template',		-- 10 games
		[2] = 'ms_rewards_reward_page_template_s_3',	-- 10 social games
		[3] = 'ms_rewards_reward_page_template_s_2',	-- 25 games
	},
}

--[[
local function ShowSimpleStats()
	GetWidget('match_stats_simple_view'):SetVisible(true)
	GetWidget('match_stats_detailed_view'):SetVisible(false)
	GetWidget('matchstats_new_rewardsscreen'):SetVisible(false)
end
--]]

local function ShowDetailedStats()
	-- GetWidget('match_stats_simple_view'):SetVisible(false)
	-- GetWidget('match_stats_detailed_view'):SetVisible(true)
	GetWidget('matchstats_new_rewardsscreen'):SetVisible(false)
	GetWidget('match_stats_detailed_view'):SetVisible(true)

    -- Echo('ShowDetailedStats:'..tostring(matchStats.postGameSimple)..'/'..tostring(matchStats.isFOC)..'/'..tostring(matchStats.isCTF)..'/'..tostring(matchStats.isSoccer))
	if (matchStats.postGameSimple) then
		matchStats.postGameSimple = false
		HoN_Database:SetDBEntry('show_detailed_matchstats', true)
		if matchStats.isCTF then
			GetWidget('ctfEndSplash'):FadeIn(150)
			GetWidget('soccerEndSplash'):SetVisible(0)
			GetWidget('match_stats_simple_view'):SetVisible(0)
		elseif matchStats.isSoccer then
			GetWidget('soccerEndSplash'):FadeIn(150)
			GetWidget('ctfEndSplash'):SetVisible(0)
			GetWidget('match_stats_simple_view'):SetVisible(0)
		elseif matchStats.isFOC then
			GetWidget('match_stats_simple_view'):SetVisible(0)
			GetWidget('soccerEndSplash'):SetVisible(0)
			GetWidget('ctfEndSplash'):SetVisible(0)
		else
			GetWidget('match_stats_simple_view'):SetVisible(1)
			GetWidget('soccerEndSplash'):SetVisible(0)
			GetWidget('ctfEndSplash'):SetVisible(0)
		end
	else
		if ((not HoN_Database:ReadDBEntry('show_detailed_matchstats')) and
			(not matchStats.isCTF) and
			(not matchStats.isSoccer)) then
			GetWidget('match_stats_simple_view'):SetVisible(1)
			GetWidget('soccerEndSplash'):SetVisible(0)
			GetWidget('ctfEndSplash'):SetVisible(0)
		else
			GetWidget('match_stats_simple_view'):SetVisible(0)
			GetWidget('soccerEndSplash'):SetVisible(0)
			GetWidget('ctfEndSplash'):SetVisible(0)
		end
	end
end

local function ShowRewards()
	--GetWidget('match_stats_simple_view'):SetVisible(false)
	GetWidget('match_stats_detailed_view'):SetVisible(true)
	GetWidget('matchstats_new_rewardsscreen'):SetVisible(true)
end

local function ChatShowPostGameStats()
	printdb('^g^: ChatShowPostGameStats 1')

	Set('ui_match_stats_waitingToShow', 'true', 'bool')
	Set('_stats_last_match_id', '', 'string')
	Set('_stats_last_replay_id', '', 'string')
	Set('_stats_last_match_id', GetShowStatsMatchID(), 'string')
	Set('_stats_latest_match_played', GetShowStatsMatchID(), 'string')

	printdb('Out of game ^g^: GetShowStatsMatchID() ' .. tostring(GetShowStatsMatchID()))

	if GetWidget('match_stats'):IsVisible() then
		printdb('^g^: ChatShowPostGameStats 2')
		Set('ui_match_replays_blockSimpleStats', 'false', 'bool')
		GetWidget('RegisterEntityDefinitionsHelper'):DoEvent()
	else
		printdb('^g^: ChatShowPostGameStats 3')
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'match_stats', nil, true)
	end
end
interface:RegisterWatch('ChatShowPostGameStats', ChatShowPostGameStats)

local function EntityDefinitionsLoaded()
	printdb('^g^: EntityDefinitionsLoaded 1 GetShowStatsMatchID() = ' .. tostring(GetShowStatsMatchID()))

	if GetWidget('match_stats'):IsVisible() and (Main.walkthroughState ~= 2) then

		local isLookingUpMatch = false

		printdb('^g^: EntityDefinitionsLoaded 2')
		printdb('^g^: WaitingToShowStats()  '  			..  tostring( WaitingToShowStats() ) )
		printdb('^g^: ui_match_stats_waitingToShow  '  	..  tostring( GetCvarBool('ui_match_stats_waitingToShow')) )
		printdb('^g^: GetShowStatsMatchID()  '  		..  GetShowStatsMatchID() )
		printdb('^g^: _stats_last_match_id  '  			.. GetCvarString('_stats_last_match_id') )
		printdb('^g^: _matchstats_recentgames_lastSuccessfulLookup  '  			.. GetCvarString('_matchstats_recentgames_lastSuccessfulLookup') )
		printdb('^g^: _matchstats_recentgames_lastLookup  '  			.. GetCvarString('_matchstats_recentgames_lastLookup') )

		Match_Stats.MasteryInfo.needPopup = false
		Match_Stats.RankInfo.needPopup = false
		if (GetCvarBool('ui_local_bot_game') and ((GetShowStatsMatchID() == 4294967295) or (GetCvarString('_stats_last_match_id') == '4294967295'))) then
			printdb('^g^: EntityDefinitionsLoaded 2 LocalBot')
			GetWidget("localbotgame"):FadeIn(150)
			Set('ui_local_bot_game', false, 'bool')
		elseif ((WaitingToShowStats() or GetCvarBool('ui_match_stats_waitingToShow')) and (GetShowStatsMatchID() ~= 4294967295)) and (tonumber(GetShowStatsMatchID()) > 0) then
			printdb('^g^: EntityDefinitionsLoaded 2 A')
			Match_Stats.MasteryInfo.needPopup = true
			Match_Stats.RankInfo.needPopup = true
			matchStats.postGameSimple = true
			GetWidget('match_stats'):UICmd([=[GetMatchInfo(]=]..GetShowStatsMatchID()..[=[)]=])
			Set('_stats_last_match_id', tostring(GetShowStatsMatchID()), 'string')
			isLookingUpMatch = true
			matchStats.isAutoLookup = true
			--HoN_Codex.autopopup = true
			interface:UICmd("ClientRefreshUpgrades()")
		elseif NotEmpty(GetCvarString('_stats_last_match_id')) and (GetCvarString('_stats_last_match_id') ~= '4294967295') and (GetCvarInt('_stats_last_match_id') > 0) then
			printdb('^g^: EntityDefinitionsLoaded 2 B')
			Set('ui_match_replays_blockSimpleStats', 'true', 'bool')
			GetWidget('match_stats'):UICmd([=[GetMatchInfo(_stats_last_match_id)]=])
			isLookingUpMatch = true
		else
			-- no recent match, covered by dropdown below
			printdb('^g^: EntityDefinitionsLoaded 2 C')
		end

		Set('ui_match_stats_waitingToShow', 'false', 'bool')
		GetWidget('match_stats'):UICmd([=[ClearWaitingToShowStats()]=])

		-- This gets recent matches, if there is no currently selected match, it also looks up the first recent match
		local searchName = GetCvarString('_player_stats_searchname')
		if not Match_Stats.OtherPlayerMatch or not NotEmpty(searchName) then
			searchName = GetAccountName()
			Set('_player_stats_searchname', searchName, 'string')
		end

		local navToMatchStats = GetCvarBool('_stats_nav_to_match_stats')
		if not matchStats.isAutoLookup and not navToMatchStats then
			Set('_stats_last_match_id', '', 'string')
		end

		if navToMatchStats then
			Set('_stats_nav_to_match_stats', false)
		end

		SubmitForm('PlayerRecentGamesList', 'f', 'grab_last_matches_from_nick', 'nickname', StripClanTag(searchName), 'hosttime', HostTime())

		--GetWidget('matchreplays_selectedplayer_dropdown'):DoEvent()

		GetWidget('match_stats_detailed_tab_rewards'):SetEnabled(false)

		GetWidget('match_stats'):Sleep(1, function()
			if GetWidget('match_stats_simple_view'):IsVisible() and (not isLookingUpMatch) and Empty(GetCvarString('_stats_last_match_id')) then
				Set('ui_match_replays_blockSimpleStats', 'true', 'bool')
				Trigger('MatchEntriesFinished2')
			end
		end)

	else
		printdb('^g^: EntityDefinitionsLoaded 3 failed - match_stats not visible')
	end
	GetWidget('main_stats_blocker'):FadeOut(50)
end
local function EntityDefinitionsLoaded2()
	GetWidget('match_stats'):Sleep(1, function() EntityDefinitionsLoaded() end)
end

function Match_Stats.OpenedMatchStats()
	printdb('^g^: Match_Stats.OpenedMatchStats()')
	if (Main.walkthroughState ~= 2) then
		matchStats.isCTF = false
		matchStats.isSoccer = false
		matchStats.isFOC = false
		ShowDetailedStats()
		lib_Card:HideCardception()
		GetWidget('match_stats'):RegisterWatch('EntityDefinitionsLoaded', EntityDefinitionsLoaded2)
		Set('ui_match_replays_blockSimpleStats', 'false', 'bool')
		--GetWidget('progstats_full_container'):DoEventN(1)
		GetWidget('main_stats_blocker'):Sleep(1, function()
			GetWidget('RegisterEntityDefinitionsHelper'):DoEvent()
		end)
		GetWidget('main_stats_blocker'):FadeIn(50)
	end
end

function Match_Stats.ClosedMatchStats()
	-- if GetCvarBool('matchstats_use_simple_view') then
	-- 	printdb(' ^c CloseMatchStats A ')
	-- 	Set('_stats_last_match_id', '', 'string')
	-- 	Set('matchstats_use_simple_view', 'false', 'bool')
	-- 	Set('ui_match_replays_blockSimpleStats', 'false', 'bool')
	-- else
	-- 	printdb(' ^c CloseMatchStats B ')
	-- end
end

function Match_Stats.ClickedCloseSimple()
	Set('_stats_last_match_id', '', 'string')
	Match_Stats.ClosedMatchStats()
	Match_Stats.OpenedMatchStats()
end

function Match_Stats.ExpandAward(playerPosition, awardPosition)
	--printdb('Match_Stats.ExpandAward: ' .. tostring(playerPosition) .. ' | ' .. tostring(awardPosition) )
	GetWidget('match_stats_player_award_max_image'):SetTexture('/ui/icons/awards/award_' .. matchStats.tAssignedAwardsPlayer[tostring(awardPosition)][tostring(playerPosition)] .. '.tga')
	GetWidget('match_stats_player_award_max_label'):SetText( Translate('match_awards_' .. matchStats.tAssignedAwardsPlayer[tostring(awardPosition)][tostring(playerPosition)]) )
	GetWidget('match_stats_player_award_max_desc'):SetText( Translate('match_awards_' .. matchStats.tAssignedAwardsPlayer[tostring(awardPosition)][tostring(playerPosition)] .. '_desc') )
end

local function AwardSpecial(position, seed)
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

local function ResetSleeps()
	GetWidget('matchstats_rewards_player_rating'):Sleep(1, function() end)
	GetWidget('ms_rewards_'..'reward_wins'..'_prog_bar'):Sleep(1, function() end)
	GetWidget('progbar_'..'reward_wins'..'_label_left'):Sleep(1, function() end)
	GetWidget('ms_rewards_'..'reward_kills'..'_prog_bar'):Sleep(1, function() end)
	GetWidget('progbar_'..'reward_kills'..'_label_left'):Sleep(1, function() end)
	GetWidget('ms_rewards_'..'reward_assists'..'_prog_bar'):Sleep(1, function() end)
	GetWidget('progbar_'..'reward_assists'..'_label_left'):Sleep(1, function() end)
	GetWidget('ms_rewards_'..'reward_wards'..'_prog_bar'):Sleep(1, function() end)
	GetWidget('progbar_'..'reward_wards'..'_label_left'):Sleep(1, function() end)
	GetWidget('ms_rewards_'..'reward_smackdowns'..'_prog_bar'):Sleep(1, function() end)
	GetWidget('progbar_'..'reward_smackdowns'..'_label_left'):Sleep(1, function() end)
	GetWidget('ms_rewards_'..'match_finished'..'_breakdown_item'):Sleep(1, function() end)
	GetWidget('ms_rewards_'..'first_match'..'_breakdown_item'):Sleep(1, function() end)
	GetWidget('ms_rewards_'..'social_bonus'..'_breakdown_item'):Sleep(1, function() end)
	GetWidget('ms_rewards_'..'consecutive_bonus'..'_breakdown_item'):Sleep(1, function() end)
	GetWidget('ms_rewards_'..'gca'..'_breakdown_item'):Sleep(1, function() end)
	GetWidget('ms_rewards_'..'bloodlust'..'_breakdown_item'):Sleep(1, function() end)
	GetWidget('ms_rewards_'..'immortal'..'_breakdown_item'):Sleep(1, function() end)
	GetWidget('ms_rewards_'..'annihilation'..'_breakdown_item'):Sleep(1, function() end)
end

local function UpdateRewardTabs()
	local rewardCount = 0
	local startX = 5
	local width = 13.5

	if matchStats.hasRankData then
		GetWidget('mastery_reward_tabs_rank'):SetX(tostring(startX)..'h')
		GetWidget('mastery_reward_tabs_rank'):SetVisible(true)
		rewardCount = rewardCount + 1
	else
		GetWidget('mastery_reward_tabs_rank'):SetVisible(false)
	end

	if matchStats.hasMasteryData then
		GetWidget('mastery_reward_tabs_mastery'):SetX(tostring(startX + rewardCount * width)..'h')
		GetWidget('mastery_reward_tabs_mastery'):SetVisible(true)
		rewardCount = rewardCount + 1
	else
		GetWidget('mastery_reward_tabs_mastery'):SetVisible(false)
	end

	if matchStats.hasRewardData then
		GetWidget('mastery_reward_tabs_rewards'):SetX(tostring(startX + rewardCount * width)..'h')
		GetWidget('mastery_reward_tabs_rewards'):SetVisible(true)
		rewardCount = rewardCount + 1
	else
		GetWidget('mastery_reward_tabs_rewards'):SetVisible(false)
	end

	GetWidget('mastery_reward_tabs'):SetVisible(rewardCount > 1)
end

local function MatchInfoSummary(sourceWidget,
	update_type, match_id, match_date, match_time, name, time_played, ap, alt_pick, dm, em, ar, nl, nm, rd, shuf, sd,
	mname, winner, k2version, sUrl, iSize, _, bFileExists, sPath, compatVersion, map_name, bd, bp, ab, account_silver_coins,
	selected_upgrades, cas, no_stats, verified_only, gated, isBots, bm, matchUnixTime, cm, km, class, lp, br, bb, gamemode, hadLocalPlayer)

	-- class values
	-- 1 = public
	-- 2 = matchmaking (normal + casual)
	-- 3 = scheduled match (tournament / hontour)
	-- 4 = unscheduled match (unused)
	-- 5 = matchmaking midwars
	-- 6 = matchmaking bot + coop
	-- 7 = unranked matchmaking (normal + casual)
	-- 8 = matchmaking riftwars
	printdb('^y^: update_type: ' .. update_type)
	printdb('^y^: map_name: ' .. map_name)
	printdb('^y^: match_id: ' .. match_id)
	printdb('^y^: isBots: ' .. tostring(isBots))
	printdb('^y^: bm: ' .. tostring(bm))

	local isBots = isBots or 'false'
	local bm = bm or 'false'

	GetWidget('match_stats_simple_silver_value_haslocalplayer_parent'):SetVisible(AtoB(hadLocalPlayer))
	print('had local player is '..tostring(hadLocalPlayer)..'\n')

	GetWidget('match_stats_reward_tab_stats'):SetEnabled(1)

	if (update_type == 'main_stats_retrieving_matches') or (update_type == 'main_stats_retrieving_match') then	-- main_stats_retrieving_matches main_stats_retrieving_match main_stats_failed_retrieval main_stats_does_not_exist
		matchStats.tMatchInfoPlayer = {}
		matchStats.tAwardsPlayer = {}
		matchStats.rewardsTable	= {}
		matchStats.playerStatus = {}
		matchStats.newlyEarnedRewards = {}
		matchStats.hasRewardData = false
		matchStats.hasMasteryData = false
		matchStats.hasRankData = false
		matchStats.popupNewRewards = false
		matchStats.isBots = false
		matchStats.isMidwars = false
		matchStats.isCTF = false
		matchStats.isSoccer = false
		matchStats.isFOC = false
		matchStats.isKrosMode = false
		matchStats.useAwardsScreen = false
		matchStats.match_id = -1
		lib_Card:HideCardception()
		-- GetWidget('match_stats_simple_silver_value_parent'):SetVisible(0)
		-- GetWidget('match_stats_simple_games_played'):SetVisible(0)
		-- GetWidget('match_stats_riftwars_games_played'):SetVisible(0)
		-- GetWidget('match_stats_bot_games_played'):SetVisible(0)
		GetWidget("match_rewards_bot_games_played"):SetVisible(0)
		GetWidget("match_rewards_mid_games_played"):SetVisible(0)
		GetWidget("match_rewards_rift_games_played"):SetVisible(0)
		GetWidget("match_rewards_norm_games_played"):SetVisible(0)
		GetWidget('matchstats_new_rewardsscreen'):SetVisible(false)
		GetWidget('matchstats_new_rewardsscreen'):ClearCallback('onshow')
		GetWidget('matchstats_new_rewardsscreen'):RefreshCallbacks()
		GetWidget('match_stats_detailed_tab_rewards'):SetEnabled(false)
		ResetSleeps()
		numPlayers = 0
		printdb('^o^: MatchInfoSummary - Clearing')
		--GetWidget('progstats_full_container'):DoEventN(1)
	elseif (match_id) and NotEmpty(match_id) and (tonumber(match_id) > 0) then -- Empty(update_type) and
		printdb('^o^: MatchInfoSummary - Has Data')

		Set('_stats_last_match_id', tostring(match_id), 'string')
		matchStats.accountSelectedIconUpgrade = selected_upgrades
		matchStats.accountTotalSilver = tonumber(account_silver_coins) or 0
		matchStats.winningTeam = tonumber(winner)
		matchStats.match_id = match_id
		matchStats.isBots = AtoB(isBots) or AtoB(bm)
		matchStats.isMidwars = ((map_name) and (map_name == 'midwars'))
		matchStats.isKrosMode = (km and AtoB(km))
		matchStats.isCTF = ((map_name) and (map_name == 'capturetheflag'))
		matchStats.isSoccer = ((map_name) and (map_name == 'soccer'))
		matchStats.isFOC = ((map_name) and (map_name == 'caldavar'))
		matchStats.useAwardsScreen = (matchStats.isMidwars or matchStats.isKrosMode)
		matchStats.isUnranked = (tonumber(class) == 7)
		matchStats.isCampaign = (tonumber(class) == 10 or tonumber(class) == 11)
		matchStats.showMMR = (tonumber(class) == 2) and GetCvarBool('cl_GarenaEnable')

		GetWidget("match_rewards_bot_games_played"):SetVisible(matchStats.isBots)
		GetWidget("match_rewards_mid_games_played"):SetVisible(matchStats.isMidwars)
		GetWidget("match_rewards_rift_games_played"):SetVisible(matchStats.isKrosMode)
		GetWidget("match_rewards_norm_games_played"):SetVisible(not (matchStats.isMidwars or matchStats.isBots or matchStats.isKrosMode))

		-- if (matchStats.isBots) then
		-- 	GetWidget('ms_header_bar_limited_bar'):SetVisible(0)
		-- 	GetWidget('ms_header_bar_riftwars_bar'):SetVisible(0)
		-- 	GetWidget('ms_header_bar_bots_bar'):SetVisible(1)
		-- 	GetWidget('match_stats_simple_games_played'):SetVisible(0)
		-- 	GetWidget('match_stats_bot_games_played'):SetVisible(1)
		-- 	GetWidget('match_stats_riftwars_games_played'):SetVisible(0)
		-- elseif (matchStats.useAwardsScreen) then
		-- 	GetWidget('ms_header_bar_limited_bar'):SetVisible(matchStats.isMidwars)
		-- 	GetWidget('ms_header_bar_riftwars_bar'):SetVisible(matchStats.isKrosMode)
		-- 	GetWidget('ms_header_bar_bots_bar'):SetVisible(0)
		-- 	GetWidget('match_stats_simple_games_played'):SetVisible(matchStats.isMidwars)
		-- 	GetWidget('match_stats_riftwars_games_played'):SetVisible(matchStats.isKrosMode)
		-- 	GetWidget('match_stats_bot_games_played'):SetVisible(0)

		if (matchStats.isKrosMode) then
			if (matchStats.playerTokensEarned > 0) then
				GetWidget('match_rewards_mid_games_played'):SetWidth('-7h')
				GetWidget('match_stats_simple_token_value_parent'):SetVisible(1)
				GetWidget('match_stats_simple_token_until_value_parent'):SetVisible(0)
			elseif (matchStats.playerMatchesUntilTokens > 0) then
				GetWidget('match_rewards_mid_games_played'):SetWidth('-19h')
				GetWidget('match_stats_simple_token_value_parent'):SetVisible(0)
				GetWidget('match_stats_simple_token_until_value_parent'):SetVisible(1)
			else
				GetWidget('match_rewards_mid_games_played'):SetWidth('100%')
				GetWidget('match_stats_simple_token_value_parent'):SetVisible(0)
				GetWidget('match_stats_simple_token_until_value_parent'):SetVisible(0)
			end
		else
			GetWidget('match_rewards_mid_games_played'):SetWidth('100%')
			GetWidget('match_stats_simple_token_value_parent'):SetVisible(0)
			GetWidget('match_stats_simple_token_until_value_parent'):SetVisible(0)
		end
		-- else
		-- 	GetWidget('ms_header_bar_limited_bar'):SetVisible(0)
		-- 	GetWidget('ms_header_bar_riftwars_bar'):SetVisible(0)
		-- 	GetWidget('ms_header_bar_bots_bar'):SetVisible(0)
		-- 	GetWidget('match_stats_simple_games_played'):SetVisible(0)
		-- 	GetWidget('match_stats_bot_games_played'):SetVisible(0)
		-- 	GetWidget('match_stats_riftwars_games_played'):SetVisible(0)
		-- end

		-- if (matchStats.useAwardsScreen or matchStats.isBots) and (not GetCvarBool('ui_match_replays_blockSimpleStats')) then --(CreateGame.mapRestrictions[map_name]) and (CreateGame.mapRestrictions[map_name].simple_stats_display) and (not GetCvarBool('ui_match_replays_blockSimpleStats')) then
		-- 	-- Show simple stats
		-- 	printdb('^o^: MatchInfoSummary - Show Simple Stats')
		-- 	Set('matchstats_use_simple_view', 'true', 'bool')
		-- 	GetWidget('match_stats_simple_midwars_bg'):SetVisible(numPlayers < 10)
		-- 	GetWidget('match_stats_detailed_tab_rewards'):SetEnabled(false)
		-- 	ShowSimpleStats()
		GetWidget('match_glimmer_drop_popup'):SetVisible(GetCvarBool('cg_masteryuidebug') or (matchStats.isAutoLookup and matchStats.popupDiamond))
		if ((matchStats.isAutoLookup) or (matchStats.debugNewRewards)) and (matchStats.hasRewardData or matchStats.hasMasteryData or matchStats.hasRankData) then
			-- Show rewards tab
			printdb('^o^: MatchInfoSummary - Show Rewards')
			matchStats.isAutoLookup = false
			matchStats.popupNewRewards = true
			GetWidget('match_stats_detailed_tab_rewards'):SetEnabled(true)
			local needPopup = Match_Stats.MasteryInfo.needPopup
			GetWidget('mastery_reward'):SetVisible(matchStats.hasMasteryData and not matchStats.hasRankData)
			Match_Stats.MasteryInfo.needPopup = needPopup
			GetWidget('rank_reward'):SetVisible(matchStats.hasRankData)
			UpdateRewardTabs()
			ShowRewards()
			RequestRankedPlayInfo()
		elseif (matchStats.hasRewardData or matchStats.hasMasteryData or matchStats.hasRankData) then
			-- Enable rewards tab
			matchStats.isAutoLookup = false
			printdb('^o^: MatchInfoSummary - Show Detailed (Rewards Enabled)')
			GetWidget('match_stats_detailed_tab_rewards'):SetEnabled(true)
			-- Show full stats
			Set('matchstats_use_simple_view', 'false', 'bool')
			local needPopup = Match_Stats.MasteryInfo.needPopup
			GetWidget('mastery_reward'):SetVisible(matchStats.hasMasteryData and not matchStats.hasRankData)
			Match_Stats.MasteryInfo.needPopup = needPopup
			GetWidget('rank_reward'):SetVisible(matchStats.hasRankData)
			UpdateRewardTabs()
			ShowDetailedStats()
		else
			matchStats.isAutoLookup = false
			-- Disable rewards tab
			printdb('^o^: MatchInfoSummary - Show Detailed (Rewards Disabled)')
			GetWidget('match_stats_detailed_tab_rewards'):SetEnabled(false)
			printdb('false')
			-- Show full stats
			Set('matchstats_use_simple_view', 'false', 'bool')
			ShowDetailedStats()
		end

		groupfcall('match_stats_player_awards', function(index, widget, groupName) widget:SetText('?') end)

		tSpecialAwards = {1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20}

		for playerIndex = 0,9,1 do
			local awardTable = matchStats.tAwardsPlayer[tostring(playerIndex)]
			local widgetIndex = playerIndex --- 1

			--printdb('^g^: playerIndex: ' .. playerIndex)
			--printTable(awardTable)

			if GetWidget('match_stats_player_award_1_'..widgetIndex) then
				if (awardTable) and (awardTable[1]) then
					--printdb('Player: ' .. playerIndex .. ' | Awarded: ^g' .. tostring(awardTable[1]) .. ' at widget ' .. tostring(widgetIndex) )
					GetWidget('match_stats_player_award_1_'..widgetIndex):SetTexture('/ui/icons/awards/award_'..awardTable[1] ..'.tga')
					matchStats.tAssignedAwardsPlayer['1'][tostring(playerIndex)] = awardTable[1]
				else
					local award = AwardSpecial(playerIndex, tonumber(string.sub(match_id, string.len(match_id),  string.len(match_id))) )
					--printdb('Player: ' .. playerIndex .. ' | Awarded: ^y' .. tostring(award) .. ' at widget ' .. tostring(widgetIndex))
					GetWidget('match_stats_player_award_1_'..widgetIndex):SetTexture('/ui/icons/awards/award_' .. 'special_' ..award ..'.tga')
					matchStats.tAssignedAwardsPlayer['1'][tostring(playerIndex)] = 'special_'..award
				end
			end
			if GetWidget('match_stats_player_award_2_'..widgetIndex) then
				if (awardTable) and (awardTable[2])  then
					--printdb('Player: ' .. playerIndex .. ' | Awarded: ^g' .. tostring(awardTable[2]) .. ' at widget ' .. tostring(widgetIndex))
					GetWidget('match_stats_player_award_2_'..widgetIndex):SetTexture('/ui/icons/awards/award_'..awardTable[2] ..'.tga')
					matchStats.tAssignedAwardsPlayer['2'][tostring(playerIndex)] = awardTable[2]
				else
					local award = AwardSpecial(playerIndex, tonumber(string.sub(match_id, string.len(match_id),  string.len(match_id))) + 1)
					--printdb('Player: ' .. playerIndex .. ' | Awarded: ^y' .. tostring(award) .. ' at widget ' .. tostring(widgetIndex))
					GetWidget('match_stats_player_award_2_'..widgetIndex):SetTexture('/ui/icons/awards/award_' .. 'special_' ..award ..'.tga')
					matchStats.tAssignedAwardsPlayer['2'][tostring(playerIndex)] = 'special_'..award
				end
			end
		end

		if(AtoN(match_id) > 0) then
			Rap_Info.ShowRapPostgameWindow(time_played, match_id)
		end

	else
		printdb('^r^: end game stats doesnt know what to do - reverting to default! update_type: ' .. tostring(update_type) .. ' | match_id: ' .. tostring(match_id) )
		Set('matchstats_use_simple_view', 'false', 'bool')
		--GetWidget('match_stats_simple_view'):SetVisible(0)
		--GetWidget('match_stats_detailed_view'):SetVisible(1)
		--GetWidget('endstats_coverup_img'):SetVisible(1)
	end
end
interface:RegisterWatch('MatchInfoSummary', MatchInfoSummary)

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

local function SleepThenFadeIn(widgetName, sleepDuration, fadeDuration)
	GetWidget(widgetName):Sleep(sleepDuration, FadeIn(widgetName, fadeDuration))
end

local function HideAndRemoveExistingSleep(widgetName)
	GetWidget(widgetName):SetVisible(false)
	GetWidget(widgetName):Sleep(1, RemoveExistingSleep)
end

local function HideProgressStatsUI()	-- event 1

	ResetSleeps()

	---[[ Top Left Player Info
	GetWidget('ms_rewards_levelup_breakdown_item_sc_icon'):SetVisible(false)
	GetWidget('matchstats_rewards_player_avatar'):SetTexture('$invis')
	GetWidget('matchstats_rewards_player_name'):SetText('')
	GetWidget('matchstats_rewards_player_rating'):SetText('')
	GetWidget('matchstats_rewards_player_rating_add'):SetText('')
	GetWidget('matchstats_rewards_player_lvl'):SetText('')
	GetWidget('matchstats_rewards_player_total_silver'):SetText('')
	GetWidget('matchstats_rewards_player_total_silver_icon'):SetVisible(false)
	GetWidget('ms_rewards_levelup_label'):SetVisible(false)
	GetWidget('ms_rewards_levelup_breakdown_item_sc_label'):SetVisible(false)

	-- Top Left Total Earned
	GetWidget('matchstats_rewards_total_earned'):SetVisible(0)
	GetWidget('matchstats_rewards_total_earned_xp'):SetVisible(0)
	GetWidget('progiconpulse_total_earned_xp_icon'):SetVisible(0)
	GetWidget('matchstats_rewards_total_earned_silver'):SetVisible(0)
	GetWidget('matchstats_rewards_total_earned_silver_icon'):SetVisible(0)
	GetWidget('matchstats_rewards_player_rewards_earned'):SetVisible(0)
	--GetWidget('matchstats_rewards_player_detailed_info'):SetVisible(0)

	if GetWidget('ms_rewards_'..'reward_wins'..'_prog_bar', nil, true) then
		GetWidget('ms_rewards_'..'reward_wins'..'_prog_bar'):SetVisible(0)
		GetWidget('ms_rewards_'..'reward_kills'..'_prog_bar'):SetVisible(0)
		GetWidget('ms_rewards_'..'reward_assists'..'_prog_bar'):SetVisible(0)
		GetWidget('ms_rewards_'..'reward_wards'..'_prog_bar'):SetVisible(0)
		GetWidget('ms_rewards_'..'reward_smackdowns'..'_prog_bar'):SetVisible(0)

		GetWidget('ms_rewards_'..'match_finished'..'_breakdown_item'):SetVisible(0)
		GetWidget('ms_rewards_'..'first_match'..'_breakdown_item'):SetVisible(0)
		GetWidget('ms_rewards_'..'social_bonus'..'_breakdown_item'):SetVisible(0)
		GetWidget('ms_rewards_'..'consecutive_bonus'..'_breakdown_item'):SetVisible(0)
		GetWidget('ms_rewards_'..'gca'..'_breakdown_item'):SetVisible(0)
		GetWidget('ms_rewards_'..'bloodlust'..'_breakdown_item'):SetVisible(0)
		GetWidget('ms_rewards_'..'immortal'..'_breakdown_item'):SetVisible(0)
		GetWidget('ms_rewards_'..'annihilation'..'_breakdown_item'):SetVisible(0)
	end
	GetWidget('matchstats_rewards_player_avatar'):SetTexture('$invis')
end

local function InstantiateUpcomingRewards(upcomingRewards, upcomingMilestones, newlyEarnedRewards)
	if (#upcomingRewards > 0) or (#upcomingMilestones > 0) then

		if (#newlyEarnedRewards > 0) then
			GetWidget('ms_rewards_scrollbox'):Instantiate('ms_rewards_reward_header_template',
				'title', 'match_stat_rewards_upcoming_rewards',
				'extralabel', 'match_stat_rewards_new_rewards',
				'value',  #newlyEarnedRewards
			)
		else
			GetWidget('ms_rewards_scrollbox'):Instantiate('ms_rewards_reward_header_template',
				'title', 'match_stat_rewards_upcoming_rewards',
				'extralabel', '',
				'value',  ''
			)
		end

		for index, rewardTable in ipairs(upcomingRewards) do
			--printTable(rewardTable)
			matchStats.totalCards = matchStats.totalCards + 1
			if (matchStats.totalCards > matchStats.MAXIMUM_ITEM_CARDS) then break end
			GetWidget('ms_rewards_scrollbox'):Instantiate(rewardTable.TEMPLATE,
				'index', 		'u_'..matchStats.totalCards,
				'header', 		rewardTable.HEADER,
				'subheader', 	rewardTable.SUBHEADER,
				'icon',			rewardTable.ICON,
				'type', 		rewardTable.TYPE,
				'startdate', 	rewardTable.STARTDATE,
				'enddate', 		rewardTable.ENDDATE,
				'checkmark', 	rewardTable.CHECKMARK,
				'rewardinfo1', 	rewardTable.REWARDINFO1,
				'rewardinfo2', 	rewardTable.REWARDINFO2,
				'rewardinfo3', 	rewardTable.REWARDINFO3
			)
			GetWidget('ms_rewards_scrollbox'):Instantiate('ms_rewards_reward_spacer_template')
		end

		local function AddRandomMilestone(randomOffset)
			randomOffset = randomOffset + math.random(2)
			rewardTable = upcomingMilestones[randomOffset]
			if (rewardTable) then
				matchStats.totalCards = matchStats.totalCards + 1
				GetWidget('ms_rewards_scrollbox'):Instantiate(rewardTable.TEMPLATE,
					'index', 		'm_'..matchStats.totalCards,
					'header', 		rewardTable.HEADER,
					'subheader', 	rewardTable.SUBHEADER,
					'icon',			rewardTable.ICON,
					'type', 		rewardTable.TYPE,
					'startdate', 	rewardTable.STARTDATE,
					'enddate', 		rewardTable.ENDDATE,
					'checkmark', 	rewardTable.CHECKMARK,
					'rewardinfo1', 	rewardTable.REWARDINFO1,
					'rewardinfo2', 	rewardTable.REWARDINFO2,
					'rewardinfo3', 	rewardTable.REWARDINFO3
				)
				GetWidget('ms_rewards_scrollbox'):Instantiate('ms_rewards_reward_spacer_template')
			end
			return randomOffset
		end

		local randomOffset, remainingSpaces = 0, matchStats.MINIMUM_UPCOMING - #upcomingRewards

		for i = 1, remainingSpaces, 1 do
			if (not (matchStats.totalCards > matchStats.MAXIMUM_ITEM_CARDS)) then
				randomOffset = AddRandomMilestone(randomOffset)
			end
		end

		GetWidget('ms_rewards_scrollbox'):Instantiate('ms_rewards_reward_line_template')
	end
end

local function InstantiateHistoryRewards(rewardHistory)
	if (#rewardHistory > 0) then
		GetWidget('ms_rewards_scrollbox'):Instantiate('ms_rewards_reward_header_template',
			'title', 'match_stat_rewards_reward_history',
			'extralabel', 'match_stat_rewards_rewards_earned', -- match_stat_rewards_rewards_earned
			'value', #rewardHistory
		)
		for index, rewardTable in ipairs(rewardHistory) do
			--printTable(rewardTable)
			matchStats.totalCards = matchStats.totalCards + 1
			if (matchStats.totalCards > matchStats.MAXIMUM_ITEM_CARDS) then break end
			GetWidget('ms_rewards_scrollbox'):Instantiate(rewardTable.TEMPLATE,
				'index', 		'h_'..matchStats.totalCards,
				'header', 		rewardTable.HEADER,
				'subheader', 	rewardTable.SUBHEADER,
				'icon',			rewardTable.ICON,
				'type', 		rewardTable.TYPE,
				'startdate', 	rewardTable.STARTDATE,
				'enddate', 		rewardTable.ENDDATE,
				'checkmark', 	rewardTable.CHECKMARK,
				'rewardinfo1', 	rewardTable.REWARDINFO1,
				'rewardinfo2', 	rewardTable.REWARDINFO2,
				'rewardinfo3', 	rewardTable.REWARDINFO3
			)
			GetWidget('ms_rewards_scrollbox'):Instantiate('ms_rewards_reward_spacer_template')
		end
		GetWidget('ms_rewards_scrollbox'):Instantiate('ms_rewards_reward_line_template')
	end
end

local function InstantiateEarnedRewards(newlyEarnedRewards)
	printdb('InstantiateEarnedRewards. #newlyEarnedRewards = ' .. tostring(#newlyEarnedRewards) )
	printdb('InstantiateEarnedRewards. matchStats.popupNewRewards = ' .. tostring(matchStats.popupNewRewards) )
	if (#newlyEarnedRewards > 0) and (matchStats.popupNewRewards) then
		lib_Card:DisplayCardception(newlyEarnedRewards)
	end
end

local function InstantiateRewards(newlyEarnedRewards, rewardHistory, upcomingRewards, upcomingMilestones)
	matchStats.totalCards = 0
	groupfcall('ms_reward_temporary_widgets', function(index, widget, groupName) widget:Destroy() end)

	printdb('newlyEarnedRewards')
	for i,v in pairs(newlyEarnedRewards) do
		printdb('newlyEarnedRewards: ' .. tostring(v.HEADER) .. ' | ' .. tostring(v.COMPLETION) )
	end

	printdb('rewardHistory')
	for i,v in pairs(rewardHistory) do
		printdb('rewardHistory: ' .. tostring(v.HEADER) .. ' | ' .. tostring(v.COMPLETION) )
	end

	printdb('upcomingRewards')
	for i,v in pairs(upcomingRewards) do
		printdb('upcomingRewards: ' .. tostring(v.HEADER) .. ' | ' .. tostring(v.COMPLETION) )
	end

	printdb('upcomingMilestones')
	for i,v in pairs(upcomingMilestones) do
		printdb('upcomingMilestones: ' .. tostring(v.HEADER) .. ' | ' .. tostring(v.COMPLETION) )
	end

	--[=[
	GetWidget('ms_rewards_scrollbox'):Sleep(1, function()
		InstantiateUpcomingRewards(upcomingRewards, upcomingMilestones, newlyEarnedRewards)
		InstantiateHistoryRewards(rewardHistory)
		InstantiateEarnedRewards(newlyEarnedRewards)

		GetWidget('ms_rewards_scrollbox'):Sleep(1, function()
			GetWidget('ms_rewards_scrollbox'):UICmd([[SetClipAreaToChild()]])
		end)
	end)
	--]=]
end

function Match_Stats.OnMasteryFrame()
	if GetWidget('mastery_operation_mask'):IsVisible() then
		local timeout = GetCvarInt('cg_masteryTimeout')
		local elapse = GetTime() - Match_Stats.MasteryInfo.masktimer
		if elapse > timeout then
			GetWidget('mastery_operation_mask'):SetVisible(false)
		end
	end
end

local function SetMasteryRewardIcon(type)
	GetWidget('mastery_playerinfo_textbg'):SetTexture('/ui/fe2/mastery/player_info_bg_'..type..'.tga')
	GetWidget('mastery_playerinfo_iconbg'):SetTexture('/ui/fe2/mastery/hero_bg_'..type..'.tga')
	GetWidget('mastery_progress_bg'):SetTexture('/ui/fe2/mastery/pb_bg_'..type..'.tga')

    local color = '#5a646d'
    if (type == 'silver') then
    	color = '#dad8d1'
    elseif (type == 'gold') then
    	color = '#ff9c00'
    elseif (type == 'goldenred') then
    	color = '#9660b5'
    end
	GetWidget('mastery_reward'):SetColor(color)

	-- boost dont have 'goldenred' set
	if type == 'goldenred' then
		type = 'gold'
	end

	GetWidget('mastery_boost_btn_up'):SetTexture('/ui/fe2/mastery/boost_'..type..'_up.tga')
	GetWidget('mastery_boost_btn_over'):SetTexture('/ui/fe2/mastery/boost_'..type..'_over.tga')
	GetWidget('mastery_boost_btn_down'):SetTexture('/ui/fe2/mastery/boost_'..type..'_down.tga')

end

function Match_Stats.OnMasteryBoost(errorcode)
	Echo('Match_Stats.OnMasteryBoostSuccess')

	local wasSuperBoost = Match_Stats.MasteryInfo.boostType == 'super'
	Match_Stats.MasteryInfo.boostType = ''

	if tonumber(errorcode) > 0 then
		return
	end

	local boostExp = Match_Stats.MasteryInfo.toBoostExp
	if wasSuperBoost then boostExp = 9 * boostExp end
	Match_Stats.MasteryInfo.boostExp = Match_Stats.MasteryInfo.boostExp + boostExp

	GetWidget('mastery_boost_experience'):SetColor('white')
	GetWidget('mastery_boost_experience'):SetOutline(1)
	GetWidget('mastery_total_experience'):SetText(tostring(Match_Stats.MasteryInfo.matchExp + Match_Stats.MasteryInfo.bonusExp + Match_Stats.MasteryInfo.boostExp + Match_Stats.MasteryInfo.maxleveladdon + Match_Stats.MasteryInfo.eventBonus))
	
	if wasSuperBoost then
		Match_Stats.MasteryInfo.canSuperBoost = false
		GetWidget('mastery_super_boost_btn'):SetEnabled(false)
		GetWidget('mastery_super_boost_disable_tip'):SetVisible(true)
		GetWidget('mastery_super_boost_number'):SetText(Translate('mastery_replay_main_masteryboost_text3'))

		if Match_Stats.MasteryInfo.superBoostNum > 0 then Match_Stats.MasteryInfo.superBoostNum = Match_Stats.MasteryInfo.superBoostNum - 1 end

		if Match_Stats.MasteryInfo.superBoostNum > 0 then
			GetWidget('mastery_super_boost_btn_text'):SetText(Translate('mastery_replay_main_masterysuperboost')..' ('..Match_Stats.MasteryInfo.superBoostNum..')')
		else
			GetWidget('mastery_super_boost_btn_text'):SetText(Translate('mastery_replay_main_masterysuperboost'))
		end
	else
		Match_Stats.MasteryInfo.canBoost = false
		GetWidget('mastery_boost_btn'):SetEnabled(false)
		GetWidget('mastery_boost_disable_tip'):SetVisible(true)
		GetWidget('mastery_boost_number'):SetText(Translate('mastery_replay_main_masteryboost_text3'))

		if Match_Stats.MasteryInfo.boostNum > 0 then Match_Stats.MasteryInfo.boostNum = Match_Stats.MasteryInfo.boostNum - 1 end

		if Match_Stats.MasteryInfo.boostNum > 0 then
			GetWidget('mastery_boost_btn_text'):SetText(Translate('mastery_replay_main_masteryboost')..' ('..Match_Stats.MasteryInfo.boostNum..')')
		else
			GetWidget('mastery_boost_btn_text'):SetText(Translate('mastery_replay_main_masteryboost'))
		end
	end

	local bothBoostUsed = Match_Stats.MasteryInfo.boostExp == Match_Stats.MasteryInfo.toBoostExp * 10
	Match_Stats.MasteryInfo.boostUsed = Match_Stats.MasteryInfo.boostExp == Match_Stats.MasteryInfo.toBoostExp or bothBoostUsed
	Match_Stats.MasteryInfo.superBoostUsed = Match_Stats.MasteryInfo.boostExp == Match_Stats.MasteryInfo.toBoostExp * 9 or bothBoostUsed

	Match_Stats.UpdateBoostExpUI()

	Match_Stats.MasteryInfo.step = 2
end

function Match_Stats.OnMasteryPopupFrame()
	if Match_Stats.MasteryInfo.popupShow then
	    local value = (GetTime() - Match_Stats.MasteryInfo.popupTimer) / 200
	    if (value > 1) then
	    	value = 1
	    end

	    GetWidget('mastery_pop_levelup_anim'):SetWidth(tostring(96*value)..'h')
		GetWidget('mastery_pop_levelup_anim'):SetHeight(tostring(96*value)..'h')
	else
		local value = (GetTime() - Match_Stats.MasteryInfo.popupTimer) / 200 + 1.0
		GetWidget('mastery_pop_levelup_anim'):SetWidth(tostring(96*value)..'h')
		GetWidget('mastery_pop_levelup_anim'):SetHeight(tostring(96*value)..'h')

		if (value > 1.8) then
			GetWidget('mastery_pop_levelup'):SetVisible(false)
			GetWidget('mastery_pop_levelup_anim'):SetWidth('96h')
			GetWidget('mastery_pop_levelup_anim'):SetHeight('96h')
		end
	end
end

function Match_Stats.OnRankRewardPopupFrame()
	if Match_Stats.RankInfo.popupShow then
	    local value = (GetTime() - Match_Stats.RankInfo.popupTimer) / 200
	    if (value > 1) then
	    	value = 1
	    end

	    GetWidget('rank_reward_popup_levelup_anim'):SetWidth(tostring(96*value)..'h')
	    GetWidget('rank_reward_popup_levelup_anim'):SetHeight(tostring(96*value)..'h')
	else
		local value = (GetTime() - Match_Stats.RankInfo.popupTimer) / 200 + 1.0
		GetWidget('rank_reward_popup_levelup_anim'):SetWidth(tostring(96*value)..'h')
		GetWidget('rank_reward_popup_levelup_anim'):SetHeight(tostring(96*value)..'h')

		if (value > 1.8) then
			GetWidget('rank_reward_popup'):SetVisible(false)
			GetWidget('rank_reward_popup_levelup_anim'):SetWidth('96h')
			GetWidget('rank_reward_popup_levelup_anim'):SetHeight('96h')
		end
	end
end

local function SetMasteryPop(level)
	local type = GetMasterTypeByLevel(level)

	if level > 0 then
		GetWidget('mastery_pop_heroicon'):SetTexture('/ui/fe2/mastery/reward_level_'..level..'.tga')
	end
	GetWidget('mastery_pop_nickname'):SetText(Translate('mastery_replay_pop_nickname', 'nickname', Match_Stats.MasteryInfo.nickname))
	GetWidget('mastery_pop_heroname1'):SetText(Translate('mastery_replay_pop_heroname1', 'level', tostring(level), 'heroname', Match_Stats.MasteryInfo.heroName))
	GetWidget('mastery_pop_tips'):SetText(Translate('mastery_replay_pop_tiplevel'..level))
	local color1 = 'black'
	local color2 = 'black'
	if (type == 'silver') then
		color1 = '#2c2012'
		color2 = '#170f02'
	elseif (type == 'gold') then
		color1 = '#271e0c'
		color2 = '#120c00'
	elseif (type == 'goldenred') then
		color1 = '#271e0c'		-- @@ 2 todo
		color2 = '#120c00'
	end

	GetWidget('mastery_pop_levelup_bgcolor1'):SetColor(color1)
	GetWidget('mastery_pop_levelup_bgcolor1'):SetColor(color2)
	GetWidget('mastery_pop_heroname2'):SetText(Translate('mastery_replay_pop_heroname2', 'heroname', Match_Stats.MasteryInfo.heroName, 'reward', Translate('mastery_levelreward_'..level)))
	GetWidget('mastery_pop_bg'):SetTexture('/ui/fe2/mastery/pop_'..type..'.tga')
	GetWidget('mastery_pop_levelup_backeffect_iron'):SetVisible(type=='iron')
	GetWidget('mastery_pop_levelup_backeffect_silver'):SetVisible(type=='silver')
	GetWidget('mastery_pop_levelup_backeffect_gold'):SetVisible(type=='gold')
	GetWidget('mastery_pop_levelup_backeffect_goldenred'):SetVisible(type=='goldenred')

	GetWidget('mastery_pop_levelup_fronteffect'):SetEffect('/ui/fe2/mastery/models/highlight_'..type..'.effect')

	if (string.find(matchStats.accountSelectedIconUpgrade, 'custom_icon')) then
		GetWidget('mastery_pop_avatar'):UICmd([[SetTextureURL(
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
									SubString(']]..matchStats.accountSelectedIconUpgrade..[[',
										SearchString(']]..matchStats.accountSelectedIconUpgrade..[[', ':', 0) + 1,
										( StringLength(']]..matchStats.accountSelectedIconUpgrade..[[') - (SearchString(']]..matchStats.accountSelectedIconUpgrade..[[', ':', 0) - 1) )
									) #
									'.cai'
								)
							]])
	elseif NotEmpty(matchStats.accountSelectedIconUpgrade) then
		GetWidget('mastery_pop_avatar'):SetTexture( GetAccountIconTexturePathFromUpgrades(matchStats.accountSelectedIconUpgrade) )
	else
		GetWidget('mastery_pop_avatar'):SetTexture('/ui/common/ability_coverup.tga')
	end
end

local function GetMasteryMaxLevel(versionString)
	local limitedVersion = '4.1.0'
	if GetCvarBool('releaseStage_test') then
		limitedVersion = '0.27.184'
	elseif GetCvarBool('releaseStage_rc') then
		limitedVersion = '0.26.206'
	end
	local function VersionLess(a, b)
		local as = split(a, '%.')
		local bs = split(b, '%.')
		for i = 1, 3 do
			if tonumber(as[i]) > tonumber(bs[i]) then
				return false
			elseif tonumber(as[i]) < tonumber(bs[i]) then
				return true
			end
		end
		return false
	end
	local maxLevel
	if VersionLess(versionString, limitedVersion) then
		maxLevel = 10
	else
		maxLevel = GetCvarInt('hero_mastery_maxlevel_new')
	end
	return maxLevel
end

local function UpdateMasteryToBoostBar()
	local experience = Match_Stats.MasteryInfo.displayExp
	if experience == nil then return end
	local masteryLevel = GetMasteryLevelByExp(experience)
    local min, max = GetMasteryExpByLevel(masteryLevel)
    
	local baseExp = Match_Stats.MasteryInfo.originalExp + Match_Stats.MasteryInfo.matchExp + Match_Stats.MasteryInfo.bonusExp + Match_Stats.MasteryInfo.maxleveladdon + Match_Stats.MasteryInfo.eventBonus
    local startExp = baseExp
    local boostExp = Match_Stats.MasteryInfo.toBoostExp			-- default: boost exp
    if Match_Stats.MasteryInfo.boostExp >= 9 * boostExp then	-- super boost used
    	boostExp = 10 * boostExp
    elseif Match_Stats.MasteryInfo.hoverOnSuperBoost then		-- hover on super boost
    	boostExp = 9 * boostExp
    end

    local afterBoostExp = baseExp + boostExp

    if afterBoostExp > startExp then
	    local boostEnd = (startExp - min) / (max - min)
	    if (boostEnd > 1.0) then boostEnd = 1 end
	    if (boostEnd < 0) then boostEnd = 0 end
	    GetWidget('mastery_progress_boostvalue'):SetX(tostring(boostEnd * 100)..'%')

		local boostEnd2 = (afterBoostExp - min) / (max - min)
	    if (boostEnd2 > 1.0) then boostEnd2 = 1 end

	    local width = boostEnd2 - boostEnd
	    if (width < 0) then width = 0 end
	    GetWidget('mastery_progress_boostvalue'):SetWidth(tostring(width * 100)..'%')
    else
    	GetWidget('mastery_progress_boostvalue'):SetWidth(0)
    end
end

local function UpdateMasteryPanel(experience)
	local masteryLevel = GetMasteryLevelByExp(experience)
    GetWidget('mastery_level'):SetText(tostring(masteryLevel))

    if (Match_Stats.MasteryInfo.originalLevel ~= nil) and (Match_Stats.MasteryInfo.originalLevel ~= masteryLevel) and Match_Stats.MasteryInfo.needPopup then
    	SetMasteryPop(masteryLevel)
    	GetWidget('mastery_pop_levelup'):SetVisible(1)
    	Match_Stats.MasteryInfo.popupTimer = GetTime()
    	Match_Stats.MasteryInfo.popupShow = true
    	GetWidget('mastery_pop_levelup_anim'):FadeIn(200)
    	PlaySound('shared/sounds/achievement.ogg');
    	Cmd('ClientRefreshUpgrades')
    end
    Match_Stats.MasteryInfo.originalLevel = masteryLevel

    local curType = GetMasterTypeByLevel(masteryLevel)
	SetMasteryRewardIcon(curType)

	local reachedMaxLevel = masteryLevel >= Match_Stats.MasteryInfo.maxlevel
	local reachedLimitedMaxLevel = reachedMaxLevel and (Match_Stats.MasteryInfo.maxlevel < GetCvarInt('hero_mastery_maxlevel_new'))

	if reachedLimitedMaxLevel then
    	GetWidget('mastery_progress_value'):SetColor('#b809ea')
    	GetWidget('mastery_progress_reward_bg'):SetVisible(true)
    	GetWidget('mastery_progress_boostvalue'):SetVisible(false)
    	experience = GetMasteryExpByLevel(Match_Stats.MasteryInfo.maxlevel)
    elseif reachedMaxLevel then
    	GetWidget('mastery_xp'):SetText('')
    	GetWidget('mastery_progress_reward_name'):SetText('')
    	GetWidget('mastery_progress_reward_bg'):SetVisible(false)
    	GetWidget('mastery_progress_value'):SetColor('#e8b0fd')
    	GetWidget('mastery_progress_value_mask'):SetTexture('/ui/fe2/mastery/progressbar2_mask.tga')
    	GetWidget('mastery_progress_boostvalue'):SetVisible(false)
    	GetWidget('mastery_progress_value'):SetWidth('100%')

    	if Match_Stats.MasteryInfo.boostUsed then
			GetWidget('mastery_boost_number'):SetText(Translate('mastery_replay_main_masteryboost_text3'))
		else
			GetWidget('mastery_boost_number'):SetText(Translate('mastery_replay_main_masteryboost_text4'))
		end
    	GetWidget('mastery_boost_btn'):SetEnabled(false)
    	GetWidget('mastery_boost_disable_tip'):SetVisible(true)

		if Match_Stats.MasteryInfo.superBoostUsed then
			GetWidget('mastery_super_boost_number'):SetText(Translate('mastery_replay_main_masteryboost_text3'))
		else
			GetWidget('mastery_super_boost_number'):SetText(Translate('mastery_replay_main_masteryboost_text4'))
		end
		GetWidget('mastery_super_boost_btn'):SetEnabled(false)
		GetWidget('mastery_super_boost_disable_tip'):SetVisible(true)

		return reachedMaxLevel 
    else
    	GetWidget('mastery_progress_value'):SetColor('#b809ea')
    	GetWidget('mastery_progress_reward_bg'):SetVisible(true)
    end

    local rewardtext = 'mastery_levelreward_'..(masteryLevel+1)
    if rewardtext ~= Translate(rewardtext) then
   		GetWidget('mastery_progress_reward'):SetTexture('/ui/fe2/mastery/reward_level_'..(masteryLevel+1)..'.tga')
		GetWidget('mastery_progress_reward_name'):SetText(Translate(rewardtext))
	else
		GetWidget('mastery_progress_reward'):SetTexture('$invis')
		GetWidget('mastery_progress_reward_name'):SetText('')
	end

    local min, max = GetMasteryExpByLevel(masteryLevel)
    local value = (experience - min) / (max - min)
	GetWidget('mastery_progress_value'):SetWidth(tostring(value * 100)..'%')

	UpdateMasteryToBoostBar()
	if reachedLimitedMaxLevel then
    	GetWidget('mastery_progress_boostvalue'):SetWidth(0)
	end

    --current exp--
    GetWidget('mastery_xp'):SetText(Translate('mastery_replay_main_exp')..' '..tostring(experience - min)..'/'..tostring(max - min))

    return reachedMaxLevel, experience
end

function Match_Stats.UpdateMastery()
	GetWidget('mastery_progress_boostvalue'):SetVisible(Match_Stats.MasteryInfo.step > 1)

    local speedofstep1 = 1
    if (Match_Stats.MasteryInfo.matchExp + Match_Stats.MasteryInfo.bonusExp + Match_Stats.MasteryInfo.maxleveladdon + Match_Stats.MasteryInfo.eventBonus) >= 1000 then
    	speedofstep1 = 6
    elseif (Match_Stats.MasteryInfo.matchExp + Match_Stats.MasteryInfo.bonusExp + Match_Stats.MasteryInfo.maxleveladdon + Match_Stats.MasteryInfo.eventBonus) >= 500 then
    	speedofstep1 = 3
    elseif (Match_Stats.MasteryInfo.matchExp + Match_Stats.MasteryInfo.bonusExp + Match_Stats.MasteryInfo.maxleveladdon + Match_Stats.MasteryInfo.eventBonus) >= 200 then
    	speedofstep1 = 2
    end

    local speedofstep3 = 1
    if Match_Stats.MasteryInfo.boostExp >= 1000 then
    	speedofstep3 = 6
    elseif Match_Stats.MasteryInfo.boostExp >= 500 then
    	speedofstep3 = 3
    elseif Match_Stats.MasteryInfo.boostExp >= 200 then
    	speedofstep3 = 2
    end

	if (Match_Stats.MasteryInfo.step == 0) then
		local duration = GetTime() - Match_Stats.MasteryInfo.timer
		local maxLevelReached, exp = UpdateMasteryPanel(Match_Stats.MasteryInfo.displayExp)
		if maxLevelReached then
			Match_Stats.MasteryInfo.step = 4
			Match_Stats.MasteryInfo.displayExp = exp
		elseif duration > 1000 then
			Match_Stats.MasteryInfo.step = 1
		end
	elseif (Match_Stats.MasteryInfo.step == 1) then
		Match_Stats.MasteryInfo.displayExp = Match_Stats.MasteryInfo.displayExp + speedofstep1

		if Match_Stats.MasteryInfo.displayExp >= (Match_Stats.MasteryInfo.originalExp + Match_Stats.MasteryInfo.matchExp + Match_Stats.MasteryInfo.bonusExp + Match_Stats.MasteryInfo.maxleveladdon + Match_Stats.MasteryInfo.eventBonus) then
			Match_Stats.MasteryInfo.displayExp = Match_Stats.MasteryInfo.originalExp + Match_Stats.MasteryInfo.matchExp + Match_Stats.MasteryInfo.bonusExp + Match_Stats.MasteryInfo.maxleveladdon + Match_Stats.MasteryInfo.eventBonus
			Match_Stats.MasteryInfo.step = 2
			Match_Stats.MasteryInfo.timer = GetTime()
		end
		local maxLevelReached, exp = UpdateMasteryPanel(Match_Stats.MasteryInfo.displayExp)
		if maxLevelReached then
			Match_Stats.MasteryInfo.step = 4
			Match_Stats.MasteryInfo.displayExp = exp
		end
	elseif (Match_Stats.MasteryInfo.step == 2) then
		local duration = GetTime() - Match_Stats.MasteryInfo.timer
		if duration > 1000 and Match_Stats.MasteryInfo.boostExp >= Match_Stats.MasteryInfo.toBoostExp then
			Match_Stats.MasteryInfo.step = 3
		end
	elseif (Match_Stats.MasteryInfo.step == 3) then
		Match_Stats.MasteryInfo.displayExp = Match_Stats.MasteryInfo.displayExp + speedofstep3

		if Match_Stats.MasteryInfo.displayExp >= (Match_Stats.MasteryInfo.originalExp + Match_Stats.MasteryInfo.matchExp + Match_Stats.MasteryInfo.bonusExp + Match_Stats.MasteryInfo.boostExp + Match_Stats.MasteryInfo.maxleveladdon + Match_Stats.MasteryInfo.eventBonus) then
			Match_Stats.MasteryInfo.displayExp = Match_Stats.MasteryInfo.originalExp + Match_Stats.MasteryInfo.matchExp + Match_Stats.MasteryInfo.bonusExp + Match_Stats.MasteryInfo.boostExp + Match_Stats.MasteryInfo.maxleveladdon + Match_Stats.MasteryInfo.eventBonus
			Match_Stats.MasteryInfo.step = 4
		end
		local maxLevelReached, exp = UpdateMasteryPanel(Match_Stats.MasteryInfo.displayExp)
		if maxLevelReached then
			Match_Stats.MasteryInfo.step = 4
			Match_Stats.MasteryInfo.displayExp = exp
		end
	end
end

local function RefreshRankInfoPanel(mmr, medal, ranking)

	GetWidget('rank_reward_curret_icon'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevelAfterS6(medal))

	local minMMR, maxMMR = GetMMRByRankLevelAfterS6(medal)
	local currentMMR = mmr
	local debugStr = 'min:'..minMMR..', current:'..currentMMR..', max:'..maxMMR

	if GetCvarBool('cg_campaigndebug') then
		GetWidget('rank_reward_debug'):SetText(debugStr)
	else
		GetWidget('rank_reward_debug'):SetText('')
	end

	if IsMaxRankLevel(medal) then
		GetWidget('rank_reward_nextlevel_root'):SetVisible(false)

		if ranking and ranking > 0 then
			GetWidget('rank_reward_curret_text'):SetText(Translate('player_compaign_level_name_S7_'..tostring(medal))..' '..tostring(ranking))
		else
			GetWidget('rank_reward_curret_text'):SetText(Translate('player_compaign_level_name_S7_'..tostring(medal)))
		end

		if currentMMR > maxMMR then
			GetWidget('rank_reward_progressbar'):SetWidth('100%')
			GetWidget('rank_reward_progressbar'):SetColor('#d8daef')
		else
			if currentMMR < minMMR then currentMMR = minMMR end
			if currentMMR > maxMMR then currentMMR = maxMMR end

			local rate = (currentMMR - minMMR) / (maxMMR - minMMR)
			GetWidget('rank_reward_progressbar'):SetWidth(tostring(rate*100)..'%')
			GetWidget('rank_reward_progressbar'):SetColor('#7e35c2')
		end
	else
		GetWidget('rank_reward_curret_text'):SetText(Translate('player_compaign_level_name_S7_'..tostring(medal)))
		GetWidget('rank_reward_nextlevel_root'):SetVisible(true)
		GetWidget('rank_reward_next_text'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevelAfterS6(medal+1))
		GetWidget('rank_reward_nextlevel_tips'):SetCallback('onmouseover', function()
			Trigger('genericMainFloatingTip', 'true', '10h', '', '', 'player_compaign_level_name_S7_'..tostring(medal + 1), '', '', '3h', '-2h')
		end)

		if currentMMR < minMMR then currentMMR = minMMR end
		if currentMMR > maxMMR then currentMMR = maxMMR end

		local rate = (currentMMR - minMMR) / (maxMMR - minMMR)
		GetWidget('rank_reward_progressbar'):SetWidth(tostring(rate*100)..'%')
		GetWidget('rank_reward_progressbar'):SetColor('#7e35c2')
	end

	GetWidget('rank_reward_curret_effect'):SetVisible(Match_Stats.RankInfo.displayLevelUp)
end

local function SetRankRewardPopup(medal, isPlacement)
	if medal > 0 and medal <= 5 then
		GetWidget('rank_reward_popup_effect'):StartEffect('/ui/fe2/season/effects/popup_back_1.effect', 0.5, 0.5, '1 1 1', 1)
	elseif medal > 5 and medal <= 10 then
		GetWidget('rank_reward_popup_effect'):StartEffect('/ui/fe2/season/effects/popup_back_2.effect', 0.5, 0.5, '1 1 1', 1)
	elseif medal > 10 and medal <= 13 then
		GetWidget('rank_reward_popup_effect'):StartEffect('/ui/fe2/season/effects/popup_back_3.effect', 0.5, 0.5, '1 1 1', 1)
	elseif medal > 13 and medal <= 15 then
		GetWidget('rank_reward_popup_effect'):StartEffect('/ui/fe2/season/effects/popup_back_4.effect', 0.5, 0.5, '1 1 1', 1)
	elseif medal == 16 then
		GetWidget('rank_reward_popup_effect'):StartEffect('/ui/fe2/season/effects/popup_back_5.effect', 0.5, 0.5, '1 1 1', 1)
	elseif medal >= 17 then
		GetWidget('rank_reward_popup_effect'):StartEffect('/ui/fe2/season/effects/popup_back_6.effect', 0.5, 0.5, '1 1 1', 1)
	end

	GetWidget('rank_reward_popup_icon'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevelAfterS6(medal))
	GetWidget('rank_reward_popup_tips'):SetText(Translate('rankreward_popup_tips', 'level_name', Translate('player_compaign_level_name_S7_'..tostring(medal))))

	isPlacement = isPlacement or false
	GetWidget('rank_reward_popup_title_placement'):SetVisible(isPlacement)
	GetWidget('rank_reward_popup_close_placement'):SetVisible(isPlacement)
	GetWidget('rank_reward_popup_title_campaign'):SetVisible(not isPlacement)
	GetWidget('rank_reward_popup_close_campaign'):SetVisible(not isPlacement)
end

function Match_Stats.UpdateRankInfoPanel()
	local speedofstep1 = 0.1
	if (Match_Stats.RankInfo.step == 0) then
		local duration = GetTime() - Match_Stats.RankInfo.timer
		RefreshRankInfoPanel(Match_Stats.RankInfo.displayMMR, Match_Stats.RankInfo.displayMedal, Match_Stats.RankInfo.ranking)
		if duration > 1000 then
			Match_Stats.RankInfo.step = 1
		end
	elseif (Match_Stats.RankInfo.step == 1) then
		-- Win (mmr increase)
		if (Match_Stats.RankInfo.mmr_after > Match_Stats.RankInfo.mmr_before) then

			if IsMaxRankLevel(Match_Stats.RankInfo.displayMedal) then
				Match_Stats.RankInfo.displayMMR = Match_Stats.RankInfo.mmr_after
				Match_Stats.RankInfo.step = 2
				Match_Stats.RankInfo.timer = GetTime()
				return
			end

			Match_Stats.RankInfo.displayMMR = Match_Stats.RankInfo.displayMMR + speedofstep1
			local minMMR, maxMMR = GetMMRByRankLevelAfterS6(Match_Stats.RankInfo.displayMedal)

			if Match_Stats.RankInfo.displayMMR > maxMMR and Match_Stats.RankInfo.medal_after > Match_Stats.RankInfo.displayMedal then
				Match_Stats.RankInfo.displayMedal = Match_Stats.RankInfo.displayMedal + 1

				if Match_Stats.RankInfo.needPopup then
					SetRankRewardPopup(Match_Stats.RankInfo.displayMedal)
					GetWidget('rank_reward_popup'):SetVisible(1)
    				Match_Stats.RankInfo.popupTimer = GetTime()
    				Match_Stats.RankInfo.popupShow = true
    				GetWidget('rank_reward_popup_levelup_anim'):FadeIn(200)
    				PlaySound('shared/sounds/rankup_display.ogg')
    			end
    			Match_Stats.RankInfo.displayLevelUp = true
			end

			if Match_Stats.RankInfo.displayMMR >= Match_Stats.RankInfo.mmr_after then
				Match_Stats.RankInfo.displayMMR = Match_Stats.RankInfo.mmr_after
				Match_Stats.RankInfo.step = 2
				Match_Stats.RankInfo.timer = GetTime()
			end
		-- Loss (mmr decrease)
		else
			Match_Stats.RankInfo.displayMMR = Match_Stats.RankInfo.displayMMR - speedofstep1
			local minMMR, maxMMR = GetMMRByRankLevelAfterS6(Match_Stats.RankInfo.displayMedal)

			if Match_Stats.RankInfo.displayMMR < minMMR and Match_Stats.RankInfo.medal_after < Match_Stats.RankInfo.displayMedal then
				Match_Stats.RankInfo.displayMedal = Match_Stats.RankInfo.displayMedal - 1
			end

			if Match_Stats.RankInfo.displayMMR <= Match_Stats.RankInfo.mmr_after then
				Match_Stats.RankInfo.displayMMR = Match_Stats.RankInfo.mmr_after
				Match_Stats.RankInfo.step = 2
				Match_Stats.RankInfo.timer = GetTime()
			end
		end

		RefreshRankInfoPanel(Match_Stats.RankInfo.displayMMR, Match_Stats.RankInfo.displayMedal, Match_Stats.RankInfo.ranking)
	end
end

local function RefreshPlacementMatchesPanel(width)
	for i=1,6 do
		if i < Match_Stats.RankInfo.placement_matches then
			GetWidget('rank_reward_placement_matches_item'..i..'_line1'):SetWidth('100%')
			GetWidget('rank_reward_placement_matches_item'..i..'_number'):SetVisible(false)
			GetWidget('rank_reward_placement_matches_item'..i..'_tick'):SetVisible(true)

			if Empty(Match_Stats.RankInfo.placement_detail[i]) then
				GetWidget('rank_reward_placement_matches_item'..i..'_line1_color'):SetColor('#ffc600')
				GetWidget('rank_reward_placement_matches_item'..i..'_line2'):SetColor('#ffc600')
				GetWidget('rank_reward_placement_matches_item'..i..'_tick'):SetTexture('/ui/fe2/season/dag.tga')
			elseif Match_Stats.RankInfo.placement_detail[i] == '0' then
				GetWidget('rank_reward_placement_matches_item'..i..'_line1_color'):SetColor('#c50500')
				GetWidget('rank_reward_placement_matches_item'..i..'_line2'):SetColor('#c50500')
				GetWidget('rank_reward_placement_matches_item'..i..'_tick'):SetTexture('/ui/fe2/season/fail.tga')
			else
				GetWidget('rank_reward_placement_matches_item'..i..'_line1_color'):SetColor('#9be113')
				GetWidget('rank_reward_placement_matches_item'..i..'_line2'):SetColor('#9be113')
				GetWidget('rank_reward_placement_matches_item'..i..'_tick'):SetTexture('/ui/fe2/season/win.tga')
			end

		elseif i > Match_Stats.RankInfo.placement_matches then
			GetWidget('rank_reward_placement_matches_item'..i..'_line1'):SetWidth('0%')
			GetWidget('rank_reward_placement_matches_item'..i..'_line2'):SetColor('#666f7c')
			GetWidget('rank_reward_placement_matches_item'..i..'_number'):SetVisible(true)
			GetWidget('rank_reward_placement_matches_item'..i..'_tick'):SetVisible(false)
		end
		GetWidget('rank_reward_placement_matches_item'..i..'_tick'):SetY('-38i')
	end

	local index = Match_Stats.RankInfo.placement_matches
	if index < 1 or index > 6 then
		return
	end

	if Empty(Match_Stats.RankInfo.placement_detail[index]) then
		GetWidget('rank_reward_placement_matches_item'..index..'_line1_color'):SetColor('#ffc600')
		GetWidget('rank_reward_placement_matches_item'..index..'_line2'):SetColor('#ffc600')
		GetWidget('rank_reward_placement_matches_item'..index..'_tick'):SetTexture('/ui/fe2/season/dag.tga')
	elseif Match_Stats.RankInfo.placement_detail[index] == '0' then
		GetWidget('rank_reward_placement_matches_item'..index..'_line1_color'):SetColor('#c50500')
		GetWidget('rank_reward_placement_matches_item'..index..'_line2'):SetColor('#c50500')
		GetWidget('rank_reward_placement_matches_item'..index..'_tick'):SetTexture('/ui/fe2/season/fail.tga')
	else
		GetWidget('rank_reward_placement_matches_item'..index..'_line1_color'):SetColor('#9be113')
		GetWidget('rank_reward_placement_matches_item'..index..'_line2'):SetColor('#9be113')
		GetWidget('rank_reward_placement_matches_item'..index..'_tick'):SetTexture('/ui/fe2/season/win.tga')
	end

	if width >= 100 then
		GetWidget('rank_reward_placement_matches_item'..index..'_line1'):SetWidth('100%')

		local source = 0
		local target = 38
		local time = GetTime() - Match_Stats.RankInfo.timer
		local current = time / 300 * (target - source)
		if current > target then current = target end
		GetWidget('rank_reward_placement_matches_item'..index..'_tick'):SetY('-'..tostring(current)..'i')
	else
		GetWidget('rank_reward_placement_matches_item'..index..'_line1'):SetWidth(tostring(width)..'%')
		GetWidget('rank_reward_placement_matches_item'..index..'_line2'):SetColor('#666f7c')
		GetWidget('rank_reward_placement_matches_item'..index..'_number'):SetVisible(true)
		GetWidget('rank_reward_placement_matches_item'..index..'_tick'):SetVisible(false)
	end
end

function Match_Stats.UpdatePlacementMatchesPanel()
	if (Match_Stats.RankInfo.step == 0) then
		local duration = GetTime() - Match_Stats.RankInfo.timer
		RefreshPlacementMatchesPanel(Match_Stats.RankInfo.displayPlacement)
		if duration > 1000 then
			Match_Stats.RankInfo.step = 1
		end
	elseif (Match_Stats.RankInfo.step == 1) then
		Match_Stats.RankInfo.displayPlacement = Match_Stats.RankInfo.displayPlacement + 2
		if Match_Stats.RankInfo.displayPlacement >= 100 then
			Match_Stats.RankInfo.displayPlacement = 100
			Match_Stats.RankInfo.step = 2
			Match_Stats.RankInfo.timer = GetTime()

			GetWidget('rank_reward_placement_matches_item'..Match_Stats.RankInfo.placement_matches..'_number'):FadeOut(300)
			GetWidget('rank_reward_placement_matches_item'..Match_Stats.RankInfo.placement_matches..'_tick'):FadeIn(300)
		else
			RefreshPlacementMatchesPanel(Match_Stats.RankInfo.displayPlacement)
		end
	elseif (Match_Stats.RankInfo.step == 2) then
		local duration = GetTime() - Match_Stats.RankInfo.timer
		RefreshPlacementMatchesPanel(Match_Stats.RankInfo.displayPlacement)
		if duration > 500 then
			Match_Stats.RankInfo.step = 3
			if Match_Stats.RankInfo.placement_matches >= 6 and Match_Stats.RankInfo.medal_after > 0 then
				Match_Stats.RankInfo.showRankPanel = true
				GetWidget('rank_reward_hasrank'):SetVisible(Match_Stats.RankInfo.showRankPanel)
				GetWidget('rank_reward_hasnorank'):SetVisible(not Match_Stats.RankInfo.showRankPanel)

				if Match_Stats.RankInfo.needPopup then
					SetRankRewardPopup(Match_Stats.RankInfo.medal_after, true)
					GetWidget('rank_reward_popup'):SetVisible(1)
					Match_Stats.RankInfo.popupTimer = GetTime()
					Match_Stats.RankInfo.popupShow = true
					GetWidget('rank_reward_popup_levelup_anim'):FadeIn(200)
					PlaySound('shared/sounds/rankup_display.ogg')
				end

				Match_Stats.RankInfo.step = 0
				Match_Stats.RankInfo.timer = GetTime()
				Match_Stats.RankInfo.displayMMR = Match_Stats.RankInfo.mmr_before
				Match_Stats.RankInfo.displayMedal = Match_Stats.RankInfo.medal_before
				Match_Stats.RankInfo.displayPlacement = 0
				Match_Stats.RankInfo.displayLevelUp = true
			end
		end
	end
end

function Match_Stats.UpdateBoostExpUI(bHoverIn, bSuperBoost)

	local function UpdateUI(number, highlight)
		local w = GetWidget('mastery_boost_experience')
		w:SetText('+'..tostring(number))

	    if highlight then
	    	w:SetColor('white')
	    	w:SetOutline(true)
	    else
	    	w:SetColor('#666666')
	    	w:SetOutline(false)
	    end
	end

	Match_Stats.MasteryInfo.hoverOnSuperBoost = false
	local number, highlight
	if bHoverIn then
		if bSuperBoost then
			number = Match_Stats.MasteryInfo.toBoostExp * 9
			highlight = Match_Stats.MasteryInfo.superBoostUsed
			Match_Stats.MasteryInfo.hoverOnSuperBoost = true
		else
			number = Match_Stats.MasteryInfo.toBoostExp
			highlight = Match_Stats.MasteryInfo.boostUsed
		end
	else
		number = math.max(Match_Stats.MasteryInfo.boostExp, Match_Stats.MasteryInfo.toBoostExp)
		highlight = Match_Stats.MasteryInfo.boostUsed or Match_Stats.MasteryInfo.superBoostUsed
	end

	UpdateUI(number, highlight)

	if Match_Stats.MasteryInfo.wasHoverOnSuperBoost ~= Match_Stats.MasteryInfo.hoverOnSuperBoost then
		Match_Stats.MasteryInfo.wasHoverOnSuperBoost = Match_Stats.MasteryInfo.hoverOnSuperBoost
		UpdateMasteryToBoostBar()
	end
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


	--arg[1] -- ext_stats_0_coin (arg[1] == parameter 139 of Trigger 'MatchInfoPlayer')
	--arg[2] -- ext_stats_0_exp
	--arg[3] -- ext_stats_1_coin
	--arg[4] -- ext_stats_1_exp
	--arg[5] -- ext_stats_2_coin
	--arg[6] -- ext_stats_2_exp
	--arg[7] -- ext_stats_3_coin
	--arg[8] -- ext_stats_3_exp
	--arg[9] -- ext_stats_4_coin
	--arg[10] -- ext_stats_4_coin
	--arg[11] -- ext_stats_5_exp
	--arg[12] -- ext_stats_5_coin
	--arg[13] -- time_played
	--arg[14] -- mmr_before
	--arg[15] -- mmr_after
	--arg[16] -- medal_before
	--arg[17] -- medal_after
	--arg[18] -- is_casual
	--arg[19] -- ranking
	--arg[20] -- placement_matches
	--arg[21] -- account_id
	--arg[22] -- placement_detail
	--arg[23] -- versionString
	--arg[24] -- mastery_super_canboost 
	--arg[25] -- mastery_super_boostnum
	--arg[26] -- mastery_exp_super_boost
	--arg[27] -- match_id
	--arg[28] -- mastery_exp_event
	-- class is the same as MatchInfoSummary class
	-- winningTeam is also from MatchInfoSummary

	
	local isCTF = (map_name == 'capturetheflag')
	local isSoccer = (map_name == 'soccer')

	--[[
		gameplaystat0		-- unused
		gameplaystat1		-- unused
		gameplaystat2		-- captures
		gameplaystat3		-- returns
		gameplaystat4		-- carrier kills
		gameplaystat5		-- distance with flag
		gameplaystat6		-- damage to carriers
	--]]

	if NotEmpty(position) then

		if (NotEmpty(arg[21]) and tostring(UIGetAccountID()) == arg[21]) or IsMe(nickname) then --IsMe for tutorial, account id for matchstats
			-- if team == winningTeam then HoN_Codex:PostGamePopup(arg[27]) end
			--HoN_Codex.autopopup = false
			
			local perf_victory_gc, perf_first_gc, perf_social_bonus_gc, perf_consec_gc, perf_annihilation_gc, perf_bloodlust_gc, perf_ks15_gc, perf_wins_gc, perf_herokills_gc, perf_heroassists_gc, perf_wards_gc, perf_smackdown_gc, perf_level_gc, mid_games_played, limited_stats_coins, levelup_coins, legacy_bonus_gc, perf_bots_gc, bot_games_played, perf_multiplier_mmpoints = tonumber(perf_victory_gc) or 0, tonumber(perf_first_gc) or 0, tonumber(perf_social_bonus_gc) or 0, tonumber(perf_consec_gc) or 0, tonumber(perf_annihilation_gc) or 0, tonumber(perf_bloodlust_gc) or 0, tonumber(perf_ks15_gc) or 0, tonumber(perf_wins_gc) or 0, tonumber(perf_herokills_gc) or 0, tonumber(perf_heroassists_gc) or 0, tonumber(perf_wards_gc) or 0, tonumber(perf_smackdown_gc) or 0, tonumber(perf_level_gc) or 0, tonumber(mid_games_played) or 0, tonumber(limited_stats_coins) or 0, tonumber(levelup_coins) or 0, tonumber(legacy_bonus_gc) or 0, tonumber(perf_bots_gc) or 0, tonumber(bot_games_played) or 0, tonumber(perf_multiplier_mmpoints) or 0
			local perf_total_silver_new = ceil(legacy_bonus_gc + levelup_coins + perf_bloodlust_gc + perf_annihilation_gc + perf_ks15_gc + limited_stats_coins + perf_victory_gc + perf_first_gc + perf_social_bonus_gc + perf_consec_gc + perf_wins_gc + perf_herokills_gc + perf_heroassists_gc + perf_wards_gc + perf_smackdown_gc + perf_level_gc + perf_bots_gc + perf_multiplier_mmpoints + tonumber(arg[1]))

			local perf_victory_exp, perf_first_exp, perf_social_bonus_exp, perf_consec_exp, perf_annihilation_exp, perf_bloodlust_exp, perf_ks15_exp, perf_wins_exp, perf_herokills_exp, perf_heroassists_exp, perf_wards_exp, perf_smackdown_exp, perf_level_exp	= tonumber(perf_victory_exp) or 0, tonumber(perf_first_exp) or 0, tonumber(perf_social_bonus_exp) or 0, tonumber(perf_consec_exp) or 0, tonumber(perf_annihilation_exp) or 0, tonumber(perf_bloodlust_exp) or 0, tonumber(perf_ks15_exp) or 0, tonumber(perf_wins_exp) or 0, tonumber(perf_herokills_exp) or 0, tonumber(perf_heroassists_exp) or 0, tonumber(perf_wards_exp) or 0, tonumber(perf_smackdown_exp) or 0, tonumber(perf_level_exp) or 0
			local perf_total_experience_new = ceil(perf_bloodlust_exp + perf_annihilation_exp + perf_ks15_exp  + perf_victory_exp + perf_first_exp + perf_social_bonus_exp + perf_consec_exp + perf_wins_exp + perf_herokills_exp + perf_heroassists_exp + perf_wards_exp + perf_smackdown_exp + perf_level_exp + tonumber(arg[2]))

			local perf_tokens_earned = ceil(tokens_won)
			local perf_matches_until_tokens = ceil(matches_until_tokens)
			matchStats.playerTokensEarned = perf_tokens_earned
			matchStats.playerMatchesUntilTokens = perf_matches_until_tokens

			GetWidget("match_rewards_bot_games_played"):SetText(Translate('endstats_simple_view_bots_played', 'value', ceil(bot_games_played)))
			GetWidget("match_rewards_mid_games_played"):SetText(Translate('endstats_simple_view_midwars_played', 'value', ceil(mid_games_played)))
			GetWidget("match_rewards_rift_games_played"):SetText(Translate('endstats_simple_view_riftwars_played', 'value', ceil(riftwars_games_played)))
			GetWidget("match_rewards_norm_games_played"):SetText(Translate('endstats_simple_view_games_played', 'value', ceil(stats_games_played)))

			GetWidget('matchStatsPlayerIcon'):SetTexture(heroIcon)
			GetWidget('matchStatsHeroName'):SetText(heroName)

			GetWidget('matchStatsAwardsLVL'):SetText(hero_level)
			GetWidget('matchStatsAwardsKDA'):SetText(Translate('game_end_stats_kda', 'kills', herokills, 'deaths', deaths, 'assists', heroassists))
			GetWidget('matchStatsAwardsCK'):SetText(ceil(teamcreepkills))
			GetWidget('matchStatsAwardsCD'):SetText(ceil(denies))
			GetWidget('matchStatsAwardsXPM'):SetText(FtoA(experience, 1))
			GetWidget('matchStatsAwardsGPM'):SetText(FtoA(gold, 1))
			GetWidget('matchStatsAwardsAPM'):SetText(FtoA(actions, 0))

			local inventorySlots = { inventory1, inventory2, inventory3, inventory4, inventory5, inventory6 }

			for i=1,6,1 do
				if inventorySlots[i] and string.len(inventorySlots[i]) > 0 then
					GetWidget('matchStatsPlayerInfoItem'..i):SetVisible(true)
					GetWidget('matchStatsPlayerInfoItem'..i):SetTexture(GetEntityIconPath(inventorySlots[i]))
					GetWidget('matchStatsPlayerInfoItem'..i):SetCallback('onmouseover', function(widget)
						print("TriggerItemTooltip('MSPlayerInventoryTip', "..inventorySlots[i].."', 0)\n")
						widget:UICmd("TriggerItemTooltip('MSPlayerInventoryTip', '"..inventorySlots[i].."', 0)")
						Trigger('MSPlayerInventoryTipIcon', GetEntityIconPath(inventorySlots[i]))

						GetWidget('endgameItemTooltip'):SetVisible(true)
					end)
				else
					GetWidget('matchStatsPlayerInfoItem'..i):SetVisible(false)
				end
			end

			if GetCvarBool('cg_masteryuidebug') then
				local params = explode(',', GetCvarString('cg_masteryMatchReward'))
				mastery_exp_original = params[1]
				mastery_exp_match = params[2]
				mastery_exp_bonus = params[3]
				mastery_exp_boost = params[4]
				mastery_exp_to_boost = params[5]
				mastery_canboost = params[6]
				mastery_boostnum = params[7]
				mastery_boost_product_id = '3609'
				mastery_boost_gold_coin = '10'
				mastery_boost_silver_coin = '10'
				mastery_maxlevel_herocount = '10'
				mastery_maxlevel_addon = '120'
				Match_Stats.MasteryInfo.needPopup = true
				season_diamond = '120'
				cur_diamond = '10023'
				netcafe_coin = '12'
				arg[1] = '34'
			end

			if GetCvarBool('cg_campaigndebug') then
				local debugStr = GetCvarString('cg_campaigndebug_string')
				local param = {}
				if NotEmpty(debugStr) then
					param = explode(',', debugStr)
				end

				arg[14] = param[1] or '1940' -- mmr_before
				arg[15] = param[2] or '1960' -- mmr_after
				arg[16] = param[3] or '16' -- medal_before
				arg[17] = param[4] or '17' -- medal_after
				arg[18] = 'false' -- is_casual
				arg[19] = param[5] or '9' -- ranking
				arg[20] = param[6] or '3' -- placement_matches
				arg[22] = param[7] or '001' --placement_detail
				Match_Stats.RankInfo.needPopup = true
			end

			Match_Stats.netcafe_coin = netcafe_coin
			Match_Stats.cyber_bonuscoin = arg[1]
			local season_diamond_num = tonumber(season_diamond)
			GetWidget('match_stats_simple_diamond_value_parent'):SetVisible(season_diamond_num > 0)
			GetWidget('match_stats_simple_diamond_value'):SetText('+'..season_diamond)

			GetWidget('match_stats_simple_silver_value'):SetText(perf_total_silver_new)
			if (perf_total_silver_new > 0) then
				GetWidget('match_stats_simple_silver_value_parent'):SetVisible(1)
			else
				GetWidget('match_stats_simple_silver_value_parent'):SetVisible(0)
			end

			local cur_diamond_num = tonumber(cur_diamond)
			local chest_price1_num = tonumber(chest_price1)
			local chest_price2_num = tonumber(chest_price2)
			local chest_price3_num = tonumber(chest_price3)

			matchStats.popupDiamond = season_diamond_num > 0

			GetWidget('match_glimmer_drop_popup_got'):SetText('+'..season_diamond)
			GetWidget('match_glimmer_drop_popup_own'):SetText(cur_diamond)

			GetWidget('match_glimmer_drop_popup_chestnumber_chest1'):SetText(tostring(math.floor(cur_diamond_num / chest_price1_num)))
			GetWidget('match_glimmer_drop_popup_chestnumber_chest2'):SetText(tostring(math.floor(cur_diamond_num / chest_price2_num)))
			GetWidget('match_glimmer_drop_popup_chestnumber_chest3'):SetText(tostring(math.floor(cur_diamond_num / chest_price3_num)))

			if cur_diamond_num >= chest_price1_num then
				GetWidget('match_glimmer_drop_popup_desc'):SetText(Translate('match_glimmer_drop_popup_desc2'))
				GetWidget('match_glimmer_drop_popup_chestnumber'):SetVisible(true)
			else
				GetWidget('match_glimmer_drop_popup_desc'):SetText(Translate('match_glimmer_drop_popup_desc'))
				GetWidget('match_glimmer_drop_popup_chestnumber'):SetVisible(false)
			end

			GetWidget('match_stats_simple_token_value'):SetText(perf_tokens_earned)
			if (perf_matches_until_tokens ~= 1) then
				GetWidget('match_stats_simple_token_until_value'):SetText(Translate("game_end_stats_matches_until_tokens", "value", perf_matches_until_tokens))
			else
				GetWidget('match_stats_simple_token_until_value'):SetText(Translate("game_end_stats_match_until_token", "value", perf_matches_until_tokens))
			end

			if (legacy_bonus_gc > 0) then
				GetWidget('matchstats_rewards_total_earned_legacy'):SetText('+'..floor(legacy_bonus_gc))
				GetWidget('matchstats_reward_bonus_legacy'):SetVisible(1)
			else
				GetWidget('matchstats_reward_bonus_legacy'):SetVisible(0)
			end

			if (perf_multiplier_mmpoints > 0) then
				GetWidget('matchstats_rewards_total_earned_multi'):SetText('+'..floor(perf_multiplier_mmpoints))
				GetWidget('matchstats_reward_bonus_multi'):SetVisible(1)
			else
				GetWidget('matchstats_reward_bonus_multi'):SetVisible(0)
			end

			GetWidget('matchstats_new_rewardsscreen'):ClearCallback('onshow')


			if (tonumber(mastery_exp_match) > 0) then
				Match_Stats.MasteryInfo.originalExp = tonumber(mastery_exp_original)
				Match_Stats.MasteryInfo.matchExp = tonumber(mastery_exp_match)
				Match_Stats.MasteryInfo.bonusExp = tonumber(mastery_exp_bonus)
				Match_Stats.MasteryInfo.boostExp = tonumber(mastery_exp_boost) + tonumber(arg[26])
				Match_Stats.MasteryInfo.toBoostExp = tonumber(mastery_exp_to_boost)
				Match_Stats.MasteryInfo.canBoost = AtoB(mastery_canboost)
				Match_Stats.MasteryInfo.boostNum = tonumber(mastery_boostnum)
				Match_Stats.MasteryInfo.matchId = tonumber(mastery_matchid)
				Match_Stats.MasteryInfo.productId = tonumber(mastery_boost_product_id)
				Match_Stats.MasteryInfo.boostPriceGold = tonumber(mastery_boost_gold_coin)
				Match_Stats.MasteryInfo.boostPriceSilver = tonumber(mastery_boost_silver_coin)
				Match_Stats.MasteryInfo.maxleveladdon = tonumber(mastery_maxlevel_addon)
				Match_Stats.MasteryInfo.maxlevelcount = tonumber(mastery_maxlevel_herocount)
				Match_Stats.MasteryInfo.maxlevel = GetMasteryMaxLevel(arg[23])

				Match_Stats.MasteryInfo.canSuperBoost = AtoB(arg[24])
				Match_Stats.MasteryInfo.superBoostNum = tonumber(arg[25])
				Match_Stats.MasteryInfo.boostType = ''
				Match_Stats.MasteryInfo.eventBonus = tonumber(arg[28])

				local bothBoostUsed = Match_Stats.MasteryInfo.boostExp == Match_Stats.MasteryInfo.toBoostExp * 10
				Match_Stats.MasteryInfo.boostUsed = Match_Stats.MasteryInfo.boostExp == Match_Stats.MasteryInfo.toBoostExp or bothBoostUsed
				Match_Stats.MasteryInfo.superBoostUsed = Match_Stats.MasteryInfo.boostExp == Match_Stats.MasteryInfo.toBoostExp * 9 or bothBoostUsed

				Match_Stats.MasteryInfo.heroName = heroName
				Match_Stats.MasteryInfo.nickname = nickname

				matchStats.hasMasteryData = true
				--matchStats.hasRewardData = true
				Match_Stats.MasteryInfo.maxleveladdonPercent = math.floor(Match_Stats.MasteryInfo.maxleveladdon / Match_Stats.MasteryInfo.matchExp * 100 + 0.5)

				-- disallow using of boosts for old matches
				local limitedLevel = Match_Stats.MasteryInfo.maxlevel < GetCvarInt('hero_mastery_maxlevel_new')
				if limitedLevel then
					Match_Stats.MasteryInfo.canBoost = false
					Match_Stats.MasteryInfo.canSuperBoost = false
				end

                local levelOld = GetMasteryLevelByExp(Match_Stats.MasteryInfo.originalExp)
                local levelNew = GetMasteryLevelByExp(Match_Stats.MasteryInfo.originalExp + Match_Stats.MasteryInfo.matchExp + Match_Stats.MasteryInfo.bonusExp)
				Match_Stats.MasteryInfo.needPopup = Match_Stats.MasteryInfo.needPopup or (Match_Stats.MasteryInfo.canBoost and (levelOld == levelNew))

				-- old match
				local gametime = tonumber(arg[13])
				local expPerMin = GetCvarInt('cg_newMasteryExpPerMin') or 0
				if gametime > 0 and expPerMin > 0 and (Match_Stats.MasteryInfo.matchExp / gametime) < expPerMin then
					Match_Stats.MasteryInfo.matchExp = Match_Stats.MasteryInfo.matchExp + Match_Stats.MasteryInfo.bonusExp
					Match_Stats.MasteryInfo.bonusExp = 0

					if levelOld >= 3 then
						Match_Stats.MasteryInfo.matchExp = 0
						Match_Stats.MasteryInfo.toBoostExp = 0
						Match_Stats.MasteryInfo.boostExp = 0
						Match_Stats.MasteryInfo.canBoost = false
					end
				end

				local showSuperBoost = Match_Stats.MasteryInfo.superBoostNum > 0

				-- level check
				if levelOld >= 2 then
					Match_Stats.MasteryInfo.canSuperBoost = false
					showSuperBoost = false
				end

				if Match_Stats.MasteryInfo.superBoostNum < 1 then
					Match_Stats.MasteryInfo.canSuperBoost = false
				end
				
				if showSuperBoost then
					GetWidget('mastery_boost_panel'):SetX('-15h')
					GetWidget('mastery_boost_panel'):SetWidth('14h')
					GetWidget('mastery_boost_panel'):SetY(0)
					GetWidget('mastery_boost_btn_text'):SetFont('dyn_10')
					GetWidget('mastery_super_boost_panel'):SetVisible(true)
				else
					GetWidget('mastery_boost_panel'):SetX('-7h')
					GetWidget('mastery_boost_panel'):SetWidth('16.52h')
					GetWidget('mastery_boost_panel'):SetY('1h')
					GetWidget('mastery_boost_btn_text'):SetFont('dyn_12')
					GetWidget('mastery_super_boost_panel'):SetVisible(false)
				end

				if (Match_Stats.MasteryInfo.canBoost) then
					GetWidget('mastery_boost_btn'):SetEnabled(true)
					GetWidget('mastery_boost_disable_tip'):SetVisible(false)
					if Match_Stats.MasteryInfo.boostNum > 0 then
						GetWidget('mastery_boost_number'):SetText(Translate('mastery_replay_main_masteryboost_text1', 'value',mastery_boostnum ))
					else
						GetWidget('mastery_boost_number'):SetText(Translate('mastery_replay_main_masteryboost_text2'))
					end
				else
					GetWidget('mastery_boost_btn'):SetEnabled(false)
					GetWidget('mastery_boost_disable_tip'):SetVisible(true)
					if Match_Stats.MasteryInfo.boostUsed then
						GetWidget('mastery_boost_number'):SetText(Translate('mastery_replay_main_masteryboost_text3'))
					else
						GetWidget('mastery_boost_number'):SetText(Translate('mastery_replay_main_masteryboost_text4'))
					end
				end

				if Match_Stats.MasteryInfo.canSuperBoost then
					GetWidget('mastery_super_boost_btn'):SetEnabled(true)
					GetWidget('mastery_super_boost_disable_tip'):SetVisible(false)
					if Match_Stats.MasteryInfo.superBoostNum > 0 then
						GetWidget('mastery_super_boost_number'):SetText(Translate('mastery_replay_main_masteryboost_text1', 'value',Match_Stats.MasteryInfo.superBoostNum ))
					else
						GetWidget('mastery_super_boost_number'):SetText(Translate('mastery_replay_main_masteryboost_text4'))
					end
				else
					GetWidget('mastery_super_boost_btn'):SetEnabled(false)
					GetWidget('mastery_super_boost_disable_tip'):SetVisible(true)
					if Match_Stats.MasteryInfo.superBoostUsed then
						GetWidget('mastery_super_boost_number'):SetText(Translate('mastery_replay_main_masteryboost_text3'))
					else
						GetWidget('mastery_super_boost_number'):SetText(Translate('mastery_replay_main_masteryboost_text4'))
					end
				end

				if Match_Stats.MasteryInfo.boostNum > 0 then
					GetWidget('mastery_boost_btn_text'):SetText(Translate('mastery_replay_main_masteryboost')..' ('..Match_Stats.MasteryInfo.boostNum..')')
				else
					GetWidget('mastery_boost_btn_text'):SetText(Translate('mastery_replay_main_masteryboost'))
				end

				if Match_Stats.MasteryInfo.superBoostNum > 0 then
					GetWidget('mastery_super_boost_btn_text'):SetText(Translate('mastery_replay_main_masterysuperboost')..' ('..Match_Stats.MasteryInfo.superBoostNum..')')
				else
					GetWidget('mastery_super_boost_btn_text'):SetText(Translate('mastery_replay_main_masterysuperboost'))
				end

				GetWidget('mastery_playerinfo_heroname'):SetText(heroName)
				GetWidget('mastery_playerinfo_nickname'):SetText(nickname)
				GetWidget('mastery_playerinfo_heroicon'):SetTexture(heroIcon)

				GetWidget('mastery_match_experience_winicon'):SetVisible(team == winningTeam)
				GetWidget('mastery_match_experience'):SetText(tostring(Match_Stats.MasteryInfo.matchExp))
				GetWidget('mastery_total_experience'):SetText(tostring(Match_Stats.MasteryInfo.matchExp + Match_Stats.MasteryInfo.bonusExp + Match_Stats.MasteryInfo.boostExp + Match_Stats.MasteryInfo.maxleveladdon + Match_Stats.MasteryInfo.eventBonus))

				Match_Stats.UpdateBoostExpUI()

				local totalbonums = '0'
				local alreadyHasBonus = false
				if Match_Stats.MasteryInfo.maxleveladdon > 0 then
					totalbonums = tostring(Match_Stats.MasteryInfo.maxleveladdon)
					alreadyHasBonus = true
				end

				if Match_Stats.MasteryInfo.bonusExp > 0 or Match_Stats.MasteryInfo.eventBonus > 0 then
					if alreadyHasBonus then
						totalbonums = totalbonums..'^852'..' + '..tostring(Match_Stats.MasteryInfo.bonusExp + Match_Stats.MasteryInfo.eventBonus)..'^*'
					else
						totalbonums = '^852'..tostring(Match_Stats.MasteryInfo.bonusExp + Match_Stats.MasteryInfo.eventBonus)..'^*'
					end
					alreadyHasBonus = true
				end

				GetWidget('mastery_bonus_experience'):SetText(totalbonums)
				GetWidget('mastery_match_bonus_tipbutton'):SetVisible(alreadyHasBonus)

                Cmd('ClientRefreshUpgrades')
                SubmitForm('PlayerStatsMastery', 'f', 'show_stats', 'nickname', StripClanTag(GetAccountName()), 'cookie', Client.GetCookie(), 'table', 'mastery')
			end

			-- Rank Panel
			if tonumber(arg[17]) > 0 or tonumber(arg[20]) > 0 then
				matchStats.hasRankData = true
				Match_Stats.RankInfo.mmr_before = tonumber(arg[14])
				Match_Stats.RankInfo.mmr_after = tonumber(arg[15])
				Match_Stats.RankInfo.medal_before = tonumber(arg[16])
				Match_Stats.RankInfo.medal_after = tonumber(arg[17])
				Match_Stats.RankInfo.is_casual = AtoB(arg[18])
				Match_Stats.RankInfo.ranking = tonumber(arg[19])
				Match_Stats.RankInfo.placement_matches = tonumber(arg[20])

				Match_Stats.RankInfo.showRankPanel_origial = tonumber(arg[16]) > 0

				Match_Stats.RankInfo.placement_detail = {}
				for i=1, string.len(arg[22]) do
					table.insert(Match_Stats.RankInfo.placement_detail, string.sub(arg[22], i, i))
				end

				if tonumber(arg[16]) <= 0 then
					Match_Stats.RankInfo.mmr_before = Match_Stats.RankInfo.mmr_after
					Match_Stats.RankInfo.medal_before = Match_Stats.RankInfo.medal_after
				end
			end

			if (perf_consec_played) and NotEmpty(perf_consec_played) then

				printdb('^g MatchInfoPlayer hasRewardData')

				matchStats.hasRewardData = true

				GetWidget('matchstats_new_rewardsscreen'):SetCallback('onshow', function()

					printdb('^g matchstats_new_rewardsscreen onshow')

					local newlyEarnedRewards, rewardHistory, upcomingRewards, upcomingMilestones, tempUpcomingMilestones = {},{},{},{},{}

					local template

					if (not tonumber(current_time)) or (tonumber(current_time) <= 1) then
						current_time = tonumber(interface:UICmd("GetLocalServerTime()"))
					end

					-- printdb('^y current_time = '..current_time)

					-- season diamond
					local season_diamond_num = tonumber(season_diamond)
					GetWidget('mastery_reward_tabs_rewards_diamond'):SetVisible(season_diamond_num > 0)
					GetWidget('matchstats_rewards_total_earned_diamonds'):SetVisible(season_diamond_num > 0)
					GetWidget('matchstats_rewards_total_earned_diamonds_number'):SetText('+'..season_diamond)

					-- process reward data
					for eventIndex, eventTable in pairs(matchStats.rewardsTable) do
						printdb('^g eventIndex = ' .. tostring(eventIndex) )
						for itemIndex, itemTable in pairs(eventTable) do
							printdb('^y itemIndex = ' .. tostring(itemIndex) )

							printdb('itemTable.TIME_AWARDED = ' .. tostring(itemTable.TIME_AWARDED) )
							printdb('itemTable.START_TIME = ' .. tostring(itemTable.START_TIME) )
							printdb('itemTable.END_TIME = ' .. tostring(itemTable.END_TIME) )
							printdb('itemTable.PRODUCT_ID = ' .. tostring(itemTable.PRODUCT_ID) )

							if (matchStats.templateOverride) and (matchStats.templateOverride[(eventIndex)]) and (matchStats.templateOverride[(eventIndex)][(itemIndex)]) then
								template = matchStats.templateOverride[(eventIndex)][(itemIndex)]
							else
								template = 'ms_rewards_reward_page_template'
							end

							printdb('^y template = ' .. template)

							matchStats.playerStatus[eventIndex] = matchStats.playerStatus[eventIndex] or {}
							matchStats.playerStatus[eventIndex].GAMES_WON = matchStats.playerStatus[eventIndex].GAMES_WON or 0
							matchStats.playerStatus[eventIndex].SOCIAL_GAMES = matchStats.playerStatus[eventIndex].SOCIAL_GAMES or 0
							matchStats.playerStatus[eventIndex].GAMES_PLAYED = matchStats.playerStatus[eventIndex].GAMES_PLAYED or 0
							matchStats.playerStatus[eventIndex].GATED_GAMES = matchStats.playerStatus[eventIndex].GATED_GAMES or 0
							matchStats.playerStatus[eventIndex].BOT_GAMES = matchStats.playerStatus[eventIndex].BOT_GAMES or 0
							matchStats.playerStatus[eventIndex].RIFTWARS_GAMES = matchStats.playerStatus[eventIndex].RIFTWARS_GAMES or 0
							matchStats.playerStatus[eventIndex].MIDWARS_GAMES = matchStats.playerStatus[eventIndex].MIDWARS_GAMES or 0

							if (matchStats.playerStatus) and (matchStats.playerStatus[eventIndex]) then

								if (( (tonumber(itemTable.MATCH_ID)) and (tonumber(matchStats.match_id) > 0) and (tonumber(matchStats.match_id) == tonumber(itemTable.MATCH_ID)) ) ) or
									( tonumber(itemTable.TIME_AWARDED) and tonumber(current_time) and ((tonumber(current_time) - tonumber(itemTable.TIME_AWARDED)) <= 120) and ((tonumber(current_time) - tonumber(itemTable.TIME_AWARDED)) > -30) ) then

									-- new reward (do the popup)

									printdb('New from this match = ' .. tostring(( (tonumber(itemTable.MATCH_ID)) and (tonumber(matchStats.match_id) > 0) and (tonumber(matchStats.match_id) == tonumber(itemTable.MATCH_ID)) )) .. ' | ' .. tostring(eventIndex) .. ' | ' .. tostring(itemIndex))
									printdb('New from timestamp = ' .. tostring(( tonumber(itemTable.TIME_AWARDED) and tonumber(current_time) and ((tonumber(current_time) - tonumber(itemTable.TIME_AWARDED)) <= 120) )) )

									local headerPrefix, subHeaderPrefix, headerSuffix, headerGameType = '', '', '', ''
									local gameRequirement, gameCount = 0, 0
									local winPercent, socialPercent, playedPercent, gatedPercent, botPercent, completion = 1, 1, 1, 1, 1, 0

									if  (itemTable.WINS_REQUIRED) and (itemTable.WINS_REQUIRED > 0) then
										headerPrefix = Translate('match_stat_rewards_win_games')
										subHeaderPrefix = Translate('match_stat_rewards_won_games')
										gameRequirement = gameRequirement + itemTable.WINS_REQUIRED
										gameCount = gameCount + matchStats.playerStatus[eventIndex].GAMES_WON
										winPercent = matchStats.playerStatus[eventIndex].GAMES_WON / itemTable.WINS_REQUIRED
									else
										headerPrefix = Translate('match_stat_rewards_play_games')
										subHeaderPrefix = Translate('match_stat_rewards_played_games')
									end
									if  (itemTable.SOCIAL_GAMES_REQUIRED) and (itemTable.SOCIAL_GAMES_REQUIRED > 0) then
										headerSuffix = Translate('match_stat_rewards_friends_games')
										gameRequirement = gameRequirement + itemTable.SOCIAL_GAMES_REQUIRED
										gameCount = gameCount + matchStats.playerStatus[eventIndex].SOCIAL_GAMES
										socialPercent =  matchStats.playerStatus[eventIndex].SOCIAL_GAMES / itemTable.SOCIAL_GAMES_REQUIRED
									else
										headerSuffix = ''
									end
									if  (itemTable.GAMES_REQUIRED) and (itemTable.GAMES_REQUIRED > 0) then
										gameRequirement = gameRequirement + itemTable.GAMES_REQUIRED
										gameCount = gameCount + matchStats.playerStatus[eventIndex].GAMES_PLAYED
										playedPercent  = matchStats.playerStatus[eventIndex].GAMES_PLAYED / itemTable.GAMES_REQUIRED
									end
									if  (itemTable.GATED_GAMES_REQUIRED) and (itemTable.GATED_GAMES_REQUIRED > 0) then
										gameRequirement = gameRequirement + itemTable.GATED_GAMES_REQUIRED
										gameCount = gameCount + matchStats.playerStatus[eventIndex].GATED_GAMES
										gatedPercent = matchStats.playerStatus[eventIndex].GATED_GAMES / itemTable.GATED_GAMES_REQUIRED
										headerGameType = ' ' .. Translate('match_stat_rewards_gated_games')
									elseif  (itemTable.BOTS_GAMES_REQUIRED) and (itemTable.BOTS_GAMES_REQUIRED > 0) then
										gameRequirement = gameRequirement + itemTable.BOTS_GAMES_REQUIRED
										gameCount = gameCount + matchStats.playerStatus[eventIndex].BOT_GAMES
										botPercent = matchStats.playerStatus[eventIndex].BOT_GAMES / itemTable.BOTS_GAMES_REQUIRED
										headerGameType = ' ' .. Translate('match_stat_rewards_bots_games')
									elseif  (itemTable.RIFTWARS_GAMES_REQUIRED) and (itemTable.RIFTWARS_GAMES_REQUIRED > 0) then
										gameRequirement = gameRequirement + itemTable.RIFTWARS_GAMES_REQUIRED
										gameCount = gameCount + matchStats.playerStatus[eventIndex].RIFTWARS_GAMES
										botPercent = matchStats.playerStatus[eventIndex].RIFTWARS_GAMES / itemTable.RIFTWARS_GAMES_REQUIRED
										headerGameType = ' ' .. Translate('match_stat_rewards_riftwars_games')
									elseif  (itemTable.MIDWARS_REQUIRED) and (itemTable.MIDWARS_REQUIRED > 0) then
										gameRequirement = gameRequirement + itemTable.MIDWARS_REQUIRED
										gameCount = gameCount + matchStats.playerStatus[eventIndex].MIDWARS_GAMES
										botPercent = matchStats.playerStatus[eventIndex].MIDWARS_GAMES / itemTable.MIDWARS_REQUIRED
										headerGameType = ' ' .. Translate('match_stat_rewards_midwars_games')
									else
										headerGameType = ''
									end

									completion = (winPercent + socialPercent + playedPercent + gatedPercent + botPercent) / 5

									local startDate = ''
									if (itemTable.START_TIME) and NotEmpty(itemTable.START_TIME) then
										local startCalcDate = FormatDateTime(itemTable.START_TIME, '%B %d, %Y', true)
										startDate = Translate('match_stat_rewards_began', 'value', startCalcDate)
									end

									local endDate = ''
									if (itemTable.TIME_AWARDED) and NotEmpty(itemTable.TIME_AWARDED) and (tonumber(itemTable.TIME_AWARDED)) and (tonumber(itemTable.TIME_AWARDED) > 0)  then
										local awardedDate = FormatDateTime(itemTable.TIME_AWARDED, '%B %d, %Y', true)
										endDate = Translate('match_stat_rewards_awarded', 'value', awardedDate)
									end

									printdb('newlyEarnedRewards adding ' .. Translate('match_stat_rewards_header', 'play', headerPrefix, 'value', (gameRequirement), 'withfriends', headerSuffix, 'gametype', headerGameType) )
									printdb('^r rewardHistory adding ' .. Translate('match_stat_rewards_header', 'play', headerPrefix, 'value', (gameRequirement), 'withfriends', headerSuffix) )

									if ((itemTable.PRODUCT_ID) and NotEmpty(itemTable.PRODUCT_ID) and (tonumber(itemTable.PRODUCT_ID) > 0)) then

										table.insert(newlyEarnedRewards, {
											HEADER 			= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_header') 		or Translate('match_stat_rewards_header', 'play', headerPrefix, 'value', (gameRequirement), 'withfriends', headerSuffix, 'gametype', headerGameType),
											SUBHEADER 		= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_subheader') 		or Translate('match_stat_rewards_sub_header', 'played', subHeaderPrefix, 'target', (gameRequirement), 'withfriends', string.lower(headerSuffix), 'value', (Min(gameCount, gameRequirement)), 'gametype', headerGameType),
											ICON 			= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_icon') 			or itemTable.TEXTURE,
											TYPE 			= 'earned',
											STARTDATE 		= startDate,
											ENDDATE 		= endDate,
											CHECKMARK 		= 'true',
											COMPLETION 		= FtoP(completion),
											REWARDINFO1 	= string.upper(Translate('match_stat_rewards_has_item')),
											REWARDINFO2 	= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_reward') 		or string.upper(TranslateOrNil('mstore_product'..itemTable.PRODUCT_ID..'_name') or ''),
											REWARDINFO3 	= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_reward_desc') 	or TranslateOrNil('mstore_product'..itemTable.PRODUCT_ID..'_desc') or '',
											TEMPLATE 		= 'ms_rewards_reward_page_card_template',
											TIME_AWARDED	= itemTable.TIME_AWARDED,
										})

										table.insert(rewardHistory, {
											HEADER 			= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_header') 		or Translate('match_stat_rewards_header', 'play', headerPrefix, 'value', (gameRequirement), 'withfriends', headerSuffix, 'gametype', headerGameType),
											SUBHEADER 		= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_subheader') 		or Translate('match_stat_rewards_sub_header', 'played', subHeaderPrefix, 'target', (gameRequirement), 'withfriends', string.lower(headerSuffix), 'value', (gameRequirement), 'gametype', headerGameType),
											ICON 			= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_icon') 			or itemTable.TEXTURE,
											TYPE 			= 'normal',
											STARTDATE 		= startDate,
											ENDDATE 		= endDate,
											CHECKMARK 		= 'true',
											COMPLETION 		= FtoP(completion),
											REWARDINFO1 	= string.upper(Translate('match_stat_rewards_has_item')),
											REWARDINFO2 	= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_reward') 		or string.upper(TranslateOrNil('mstore_product'..itemTable.PRODUCT_ID..'_name') or ''),
											REWARDINFO3 	= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_reward_desc') 	or TranslateOrNil('mstore_product'..itemTable.PRODUCT_ID..'_desc') or '',
											TEMPLATE 		= template,
											TIME_AWARDED	= itemTable.TIME_AWARDED,
										})

									elseif ((itemTable.GOLD_COINS) and (itemTable.GOLD_COINS > 0)) or ((itemTable.SILVER_COINS) and (itemTable.SILVER_COINS > 0)) then

										local rewardInfo1, rewardInfo2 = nil, nil
										local templateIcon = '/ui/fe2/store/silver_coins.tga'

										if (itemTable.GOLD_COINS) and (itemTable.GOLD_COINS > 0) then
										    templateIcon = '/ui/fe2/store/gold_coins.tga'
											rewardInfo1 = Translate('match_stat_rewards_text_gold_prize', 'value', itemTable.GOLD_COINS)
											rewardInfo1 = '+'..rewardInfo1
										end
										if (itemTable.SILVER_COINS) and (itemTable.SILVER_COINS > 0) then
											rewardInfo2 = Translate('match_stat_rewards_text_silver_prize', 'value', itemTable.SILVER_COINS)
											rewardInfo2 = '+'..rewardInfo2
										end
										if (not rewardInfo1) and (rewardInfo2) then
											rewardInfo1 = rewardInfo2
											rewardInfo2 = nil
										end

										table.insert(newlyEarnedRewards, {
											HEADER 			= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_header') 		or Translate('match_stat_rewards_header', 'play', headerPrefix, 'value', (gameRequirement), 'withfriends', headerSuffix, 'gametype', headerGameType),
											SUBHEADER 		= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_subheader') 		or Translate('match_stat_rewards_sub_header', 'played', subHeaderPrefix, 'target', (gameRequirement), 'withfriends', string.lower(headerSuffix), 'value', (Min(gameCount, gameRequirement)), 'gametype', headerGameType),
											ICON 			= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_icon') 			or templateIcon,
											TYPE 			= 'earned',
											STARTDATE 		= startDate,
											ENDDATE 		= endDate,
											CHECKMARK 		= 'true',
											COMPLETION 		= FtoP(completion),
											REWARDINFO1 	= string.upper(Translate('match_stat_rewards_earn')),
											REWARDINFO2 	= rewardInfo1 or '',
											REWARDINFO3 	= rewardInfo2 or '',
											TEMPLATE 		= 'ms_rewards_reward_page_card_template',
											TIME_AWARDED	= itemTable.TIME_AWARDED,
										})

										table.insert(rewardHistory, {
											HEADER 			= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_header') 		or Translate('match_stat_rewards_header', 'play', headerPrefix, 'value', (gameRequirement), 'withfriends', headerSuffix, 'gametype', headerGameType),
											SUBHEADER 		= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_subheader') 		or Translate('match_stat_rewards_sub_header', 'played', subHeaderPrefix, 'target', (gameRequirement), 'withfriends', string.lower(headerSuffix), 'value', (gameRequirement), 'gametype', headerGameType),
											ICON 			= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_icon') 			or templateIcon,
											TYPE 			= 'normal',
											STARTDATE 		= startDate,
											ENDDATE 		= endDate,
											CHECKMARK 		= 'true',
											COMPLETION 		= FtoP(completion),
											REWARDINFO1 	= string.upper(Translate('match_stat_rewards_earn')),
											REWARDINFO2 	= rewardInfo1 or '',
											REWARDINFO3 	= rewardInfo2 or '',
											TEMPLATE 		= template,
											TIME_AWARDED	= itemTable.TIME_AWARDED,
										})
									else
										printdb('^r'..Translate('match_stat_rewards_header', 'play', headerPrefix, 'value', (gameRequirement), 'withfriends', headerSuffix, 'gametype', headerGameType)..'not added to newlyEarnedRewards / rewardHistory!')
										printdb('^p^:itemTable: ')
										if (GetCvarBool('ui_dev')) then
											printTable(itemTable)
										end
									end

								elseif (itemTable.TIME_AWARDED) and NotEmpty(itemTable.TIME_AWARDED) then
									-- history reward (sort by timestamp)

									printdb('history reward ' .. tostring(eventIndex) .. ' | ' .. tostring(itemIndex))

									local headerPrefix, subHeaderPrefix, headerSuffix, headerGameType = '', '', '', ''
									local gameRequirement, gameCount = 0, 0
										local winPercent, socialPercent, playedPercent, gatedPercent, botPercent, completion = 1, 1, 1, 1, 1, 0

									if (itemTable.WINS_REQUIRED) and (itemTable.WINS_REQUIRED > 0) then
										headerPrefix = Translate('match_stat_rewards_win_games')
										subHeaderPrefix = Translate('match_stat_rewards_won_games')
										gameRequirement = gameRequirement + itemTable.WINS_REQUIRED
										gameCount = gameCount + matchStats.playerStatus[eventIndex].GAMES_WON
										winPercent = matchStats.playerStatus[eventIndex].GAMES_WON / itemTable.WINS_REQUIRED
									else
										headerPrefix = Translate('match_stat_rewards_play_games')
										subHeaderPrefix = Translate('match_stat_rewards_played_games')
									end
									if (itemTable.SOCIAL_GAMES_REQUIRED) and (itemTable.SOCIAL_GAMES_REQUIRED > 0) then
										headerSuffix = Translate('match_stat_rewards_friends_games')
										gameRequirement = gameRequirement + itemTable.SOCIAL_GAMES_REQUIRED
										gameCount = gameCount + matchStats.playerStatus[eventIndex].SOCIAL_GAMES
										socialPercent =  matchStats.playerStatus[eventIndex].SOCIAL_GAMES / itemTable.SOCIAL_GAMES_REQUIRED
									else
										headerSuffix = ''
									end
									if  (itemTable.GATED_GAMES_REQUIRED) and (itemTable.GATED_GAMES_REQUIRED > 0) then
										gameRequirement = gameRequirement + itemTable.GATED_GAMES_REQUIRED
										gameCount = gameCount + matchStats.playerStatus[eventIndex].GATED_GAMES
										gatedPercent = matchStats.playerStatus[eventIndex].GATED_GAMES / itemTable.GATED_GAMES_REQUIRED
										headerGameType = ' ' .. Translate('match_stat_rewards_gated_games')
									elseif  (itemTable.BOTS_GAMES_REQUIRED) and (itemTable.BOTS_GAMES_REQUIRED > 0) then
										gameRequirement = gameRequirement + itemTable.BOTS_GAMES_REQUIRED
										gameCount = gameCount + matchStats.playerStatus[eventIndex].BOT_GAMES
										botPercent = matchStats.playerStatus[eventIndex].BOT_GAMES / itemTable.BOTS_GAMES_REQUIRED
										headerGameType = ' ' .. Translate('match_stat_rewards_bots_games')
									elseif  (itemTable.RIFTWARS_GAMES_REQUIRED) and (itemTable.RIFTWARS_GAMES_REQUIRED > 0) then
										gameRequirement = gameRequirement + itemTable.RIFTWARS_GAMES_REQUIRED
										gameCount = gameCount + matchStats.playerStatus[eventIndex].RIFTWARS_GAMES
										botPercent = matchStats.playerStatus[eventIndex].RIFTWARS_GAMES / itemTable.RIFTWARS_GAMES_REQUIRED
										headerGameType = ' ' .. Translate('match_stat_rewards_riftwars_games')
									elseif  (itemTable.MIDWARS_REQUIRED) and (itemTable.MIDWARS_REQUIRED > 0) then
										gameRequirement = gameRequirement + itemTable.MIDWARS_REQUIRED
										gameCount = gameCount + matchStats.playerStatus[eventIndex].MIDWARS_GAMES
										botPercent = matchStats.playerStatus[eventIndex].MIDWARS_GAMES / itemTable.MIDWARS_REQUIRED
										headerGameType = ' ' .. Translate('match_stat_rewards_midwars_games')
									else
										headerGameType = ''
									end

									completion = (winPercent + socialPercent + playedPercent + gatedPercent + botPercent) / 5

									local startDate = ''
									if NotEmpty(itemTable.START_TIME) and (tonumber(itemTable.START_TIME)) and (tonumber(itemTable.START_TIME) > 0) then
										local startCalcDate = FormatDateTime(itemTable.START_TIME, '%B %d, %Y', true)
										startDate = Translate('match_stat_rewards_began', 'value', startCalcDate)
									end

									local endDate = ''
									if NotEmpty(itemTable.TIME_AWARDED) and (tonumber(itemTable.TIME_AWARDED)) and (tonumber(itemTable.TIME_AWARDED) > 0) then
										local awardedDate = FormatDateTime(itemTable.TIME_AWARDED, '%B %d, %Y', true)
										endDate = Translate('match_stat_rewards_awarded', 'value', awardedDate)
									end

									printdb('rewardHistory adding ' .. Translate('match_stat_rewards_header', 'play', headerPrefix, 'value', (gameRequirement), 'withfriends', headerSuffix, 'gametype', headerGameType) )

									if ((itemTable.PRODUCT_ID) and NotEmpty(itemTable.PRODUCT_ID) and (tonumber(itemTable.PRODUCT_ID) > 0)) then

										table.insert(rewardHistory, {
											HEADER 			= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_header') 		or Translate('match_stat_rewards_header', 'play', headerPrefix, 'value', (gameRequirement), 'withfriends', headerSuffix, 'gametype', headerGameType),
											SUBHEADER 		= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_subheader') 		or Translate('match_stat_rewards_sub_header', 'played', subHeaderPrefix, 'target', (gameRequirement), 'withfriends', string.lower(headerSuffix), 'value', (gameRequirement), 'gametype', headerGameType),
											ICON 			= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_icon') 			or itemTable.TEXTURE,
											TYPE 			= 'normal',
											STARTDATE 		= startDate,
											ENDDATE 		= endDate,
											CHECKMARK 		= 'true',
											COMPLETION 		= FtoP(completion),
											REWARDINFO1 	= string.upper(Translate('match_stat_rewards_has_item')),
											REWARDINFO2 	= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_reward') 		or string.upper(TranslateOrNil('mstore_product'..itemTable.PRODUCT_ID..'_name') or ''),
											REWARDINFO3 	= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_reward_desc') 	or TranslateOrNil('mstore_product'..itemTable.PRODUCT_ID..'_desc') or '',
											TEMPLATE 		= template,
											TIME_AWARDED	= tonumber(itemTable.TIME_AWARDED) or 0,
										})

									elseif ((itemTable.GOLD_COINS) and (itemTable.GOLD_COINS > 0)) or ((itemTable.SILVER_COINS) and (itemTable.SILVER_COINS > 0)) then

										local rewardInfo1, rewardInfo2 = nil, nil
										local templateIcon = '/ui/fe2/store/silver_coins.tga'

										if (itemTable.GOLD_COINS) and (itemTable.GOLD_COINS > 0) then
										    templateIcon = '/ui/fe2/store/gold_coins.tga'
											rewardInfo1 = Translate('match_stat_rewards_text_gold_prize', 'value', itemTable.GOLD_COINS)
											rewardInfo1 = '+'..rewardInfo1
										end
										if (itemTable.SILVER_COINS) and (itemTable.SILVER_COINS > 0) then
											rewardInfo2 = Translate('match_stat_rewards_text_silver_prize', 'value', itemTable.SILVER_COINS)
											rewardInfo2 = '+'..rewardInfo2
										end
										if (not rewardInfo1) and (rewardInfo2) then
											rewardInfo1 = rewardInfo2
											rewardInfo2 = nil
										end

										table.insert(rewardHistory, {
											HEADER 			= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_header') 		or Translate('match_stat_rewards_header', 'play', headerPrefix, 'value', (gameRequirement), 'withfriends', headerSuffix, 'gametype', headerGameType),
											SUBHEADER 		= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_subheader') 		or Translate('match_stat_rewards_sub_header', 'played', subHeaderPrefix, 'target', (gameRequirement), 'withfriends', string.lower(headerSuffix), 'value', (gameRequirement), 'gametype', headerGameType),
											ICON 			= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_icon') 			or templateIcon,
											TYPE 			= 'normal',
											STARTDATE 		= startDate,
											ENDDATE 		= endDate,
											CHECKMARK 		= 'true',
											COMPLETION 		= FtoP(completion),
											REWARDINFO1 	= string.upper(Translate('match_stat_rewards_earn')),
											REWARDINFO2 	= rewardInfo1 or '',
											REWARDINFO3 	= rewardInfo2 or '',
											TEMPLATE 		= template,
											TIME_AWARDED	= tonumber(itemTable.TIME_AWARDED) or 0,
										})
									else
										printdb('^r'..Translate('match_stat_rewards_header', 'play', headerPrefix, 'value', (gameRequirement), 'withfriends', headerSuffix, 'gametype', headerGameType)..'not added to rewardHistory!')
										printdb('^p^:itemTable: ')
										if (GetCvarBool('ui_dev')) then
											printTable(itemTable)
										end
									end

								elseif ((tonumber(current_time) == nil) or (tonumber(current_time) <= 1)) or
									( ( (itemTable.END_TIME == nil) or (Empty(itemTable.END_TIME)) or ((itemTable.END_TIME) and (tonumber(current_time)) and (tonumber(current_time) <= itemTable.END_TIME)) ) and
									( (itemTable.START_TIME == nil) or (Empty(itemTable.START_TIME)) or ((itemTable.START_TIME) and (tonumber(current_time)) and (tonumber(current_time) >= itemTable.START_TIME)) ) ) then

									-- upcoming reward (from web)
									printdb('upcoming reward (from web rewards list) ' .. tostring(eventIndex) .. ' | ' .. tostring(itemIndex))

									local headerPrefix, subHeaderPrefix, headerSuffix, headerGameType = '', '', '', ''
									local gameRequirement, gameCount = 0, 0
										local winPercent, socialPercent, playedPercent, gatedPercent, botPercent, completion = 1, 1, 1, 1, 1, 0

									if (itemTable.WINS_REQUIRED) and (itemTable.WINS_REQUIRED > 0) then
										headerPrefix = Translate('match_stat_rewards_win_games')
										subHeaderPrefix = Translate('match_stat_rewards_won_games')
										gameRequirement = gameRequirement + itemTable.WINS_REQUIRED
										gameCount = gameCount + matchStats.playerStatus[eventIndex].GAMES_WON
										winPercent = matchStats.playerStatus[eventIndex].GAMES_WON / itemTable.WINS_REQUIRED
									else
										headerPrefix = Translate('match_stat_rewards_play_games')
										subHeaderPrefix = Translate('match_stat_rewards_played_games')
									end
									if (itemTable.SOCIAL_GAMES_REQUIRED) and (itemTable.SOCIAL_GAMES_REQUIRED > 0) then
										headerSuffix = Translate('match_stat_rewards_friends_games')
										gameRequirement = gameRequirement + itemTable.SOCIAL_GAMES_REQUIRED
										gameCount = gameCount + matchStats.playerStatus[eventIndex].SOCIAL_GAMES
										socialPercent =  matchStats.playerStatus[eventIndex].SOCIAL_GAMES / itemTable.SOCIAL_GAMES_REQUIRED
									else
										headerSuffix = ''
									end
									if (itemTable.GAMES_REQUIRED) and (itemTable.GAMES_REQUIRED > 0) then
										gameRequirement = gameRequirement + itemTable.GAMES_REQUIRED
										gameCount = gameCount + matchStats.playerStatus[eventIndex].GAMES_PLAYED
										playedPercent  = matchStats.playerStatus[eventIndex].GAMES_PLAYED / itemTable.GAMES_REQUIRED
									end
									if  (itemTable.GATED_GAMES_REQUIRED) and (itemTable.GATED_GAMES_REQUIRED > 0) then
										gameRequirement = gameRequirement + itemTable.GATED_GAMES_REQUIRED
										gameCount = gameCount + matchStats.playerStatus[eventIndex].GATED_GAMES
										gatedPercent = matchStats.playerStatus[eventIndex].GATED_GAMES / itemTable.GATED_GAMES_REQUIRED
										headerGameType = ' ' .. Translate('match_stat_rewards_gated_games')
									elseif  (itemTable.BOTS_GAMES_REQUIRED) and (itemTable.BOTS_GAMES_REQUIRED > 0) then
										gameRequirement = gameRequirement + itemTable.BOTS_GAMES_REQUIRED
										gameCount = gameCount + matchStats.playerStatus[eventIndex].BOT_GAMES
										botPercent = matchStats.playerStatus[eventIndex].BOT_GAMES / itemTable.BOTS_GAMES_REQUIRED
										headerGameType = ' ' .. Translate('match_stat_rewards_bots_games')
									elseif  (itemTable.RIFTWARS_GAMES_REQUIRED) and (itemTable.RIFTWARS_GAMES_REQUIRED > 0) then
										gameRequirement = gameRequirement + itemTable.RIFTWARS_GAMES_REQUIRED
										gameCount = gameCount + matchStats.playerStatus[eventIndex].RIFTWARS_GAMES
										botPercent = matchStats.playerStatus[eventIndex].RIFTWARS_GAMES / itemTable.RIFTWARS_GAMES_REQUIRED
										headerGameType = ' ' .. Translate('match_stat_rewards_riftwars_games')
									elseif  (itemTable.MIDWARS_REQUIRED) and (itemTable.MIDWARS_REQUIRED > 0) then
										gameRequirement = gameRequirement + itemTable.MIDWARS_REQUIRED
										gameCount = gameCount + matchStats.playerStatus[eventIndex].MIDWARS_GAMES
										botPercent = matchStats.playerStatus[eventIndex].MIDWARS_GAMES / itemTable.MIDWARS_REQUIRED
										headerGameType = ' ' .. Translate('match_stat_rewards_midwars_games')
									else
										headerGameType = ''
									end

									completion = (winPercent + socialPercent + playedPercent + gatedPercent + botPercent) / 5

									local startDate = ''
									if NotEmpty(itemTable.START_TIME) and (tonumber(itemTable.START_TIME)) and (tonumber(itemTable.START_TIME) > 0)  then
										local startCalcDate = FormatDateTime(itemTable.START_TIME, '%B %d, %Y', true)
										startDate = Translate('match_stat_rewards_began', 'value', startCalcDate)
									end

									local endDate = ''
									if NotEmpty(itemTable.END_TIME) and (tonumber(itemTable.START_TIME)) and (tonumber(itemTable.START_TIME) > 0) then
										local awardedDate = FormatDateTime(itemTable.END_TIME, '%B %d, %Y', true)
										endDate = Translate('match_stat_rewards_ends', 'value', awardedDate)
									end

									printdb('upcomingRewards adding ' .. Translate('match_stat_rewards_header', 'play', headerPrefix, 'value', (gameRequirement), 'withfriends', headerSuffix, 'gametype', headerGameType) )

									-- figure out the product prefix
									local prefix = ProductTypeToPrefix(itemTable.TYPE)

									if ((itemTable.PRODUCT_ID) and NotEmpty(itemTable.PRODUCT_ID) and (tonumber(itemTable.PRODUCT_ID) > 0)) then
										-- not owned, show the product
										if (not Client.IsProductOwned(prefix..itemTable.NAME)) then
											table.insert(upcomingRewards, {
												HEADER 			= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_header') 		or Translate('match_stat_rewards_header', 'play', headerPrefix, 'value', (gameRequirement), 'withfriends', headerSuffix, 'gametype', headerGameType),
												SUBHEADER 		= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_subheader') 		or Translate('match_stat_rewards_sub_header', 'played', subHeaderPrefix, 'target', (gameRequirement), 'withfriends', string.lower(headerSuffix), 'value', (Min(gameCount, gameRequirement)), 'gametype', headerGameType),
												ICON 			= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_icon') 			or itemTable.TEXTURE,
												TYPE 			= 'earning',
												STARTDATE 		= startDate,
												ENDDATE 		= endDate,
												CHECKMARK 		= 'false',
												COMPLETION 		= FtoP(completion),
												REWARDINFO1 	= string.upper(Translate('match_stat_rewards_earn_item')),
												REWARDINFO2 	= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_reward') 		or string.upper(TranslateOrNil('mstore_product'..itemTable.PRODUCT_ID..'_name') or ''),
												REWARDINFO3 	= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_reward_desc') 	or TranslateOrNil('mstore_product'..itemTable.PRODUCT_ID..'_desc') or '',
												TEMPLATE 		= template,
												TIME_AWARDED	= itemTable.TIME_AWARDED,
											})
										else -- owned, show the alt gold/silver
											local rewardInfo1, rewardInfo2 = nil, nil
											local goldReward, silverReward = 0, 0

											if (itemTable.R_GOLD_COINS and (tonumber(itemTable.R_GOLD_COINS) > 0)) then
												goldReward = goldReward + tonumber(itemTable.R_GOLD_COINS)
											end
											if (itemTable.GOLD_COINS and (tonumber(itemTable.GOLD_COINS) > 0)) then
												goldReward = goldReward + tonumber(itemTable.GOLD_COINS)
											end

											if (itemTable.R_SILVER_COINS and (tonumber(itemTable.R_SILVER_COINS) > 0)) then
												silverReward = silverReward + tonumber(itemTable.R_SILVER_COINS)
											end
											if (itemTable.SILVER_COINS and (tonumber(itemTable.SILVER_COINS) > 0)) then
												silverReward = silverReward + tonumber(itemTable.SILVER_COINS)
											end

											if (goldReward > 0) then
												rewardInfo1 = Translate('match_stat_rewards_text_gold_prize', 'value', goldReward)
												rewardInfo1 = '+'..rewardInfo1
											end
											if (silverReward > 0) then
												rewardInfo2 = Translate('match_stat_rewards_text_silver_prize', 'value', silverReward)
												rewardInfo2 = '+'..rewardInfo2
											end
											if (not rewardInfo1) and (rewardInfo2) then
												rewardInfo1 = rewardInfo2
												rewardInfo2 = nil
											end

											local templateIcon = "/ui/fe2/store/silver_coins.tga"
											if (goldReward > 0) then
												templateIcon = "/ui/fe2/store/gold_coins.tga"
											end

											table.insert(upcomingRewards, {
												HEADER 			= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_header') 		or Translate('match_stat_rewards_header', 'play', headerPrefix, 'value', (gameRequirement), 'withfriends', headerSuffix, 'gametype', headerGameType),
												SUBHEADER 		= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_subheader') 		or Translate('match_stat_rewards_sub_header', 'played', subHeaderPrefix, 'target', (gameRequirement), 'withfriends', string.lower(headerSuffix), 'value', (Min(gameCount, gameRequirement)), 'gametype', headerGameType),
												ICON 			= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_icon') 			or templateIcon,
												TYPE 			= 'earning',
												STARTDATE 		= startDate,
												ENDDATE 		= endDate,
												CHECKMARK 		= 'false',
												COMPLETION 		= FtoP(completion),
												REWARDINFO1 	= string.upper(Translate('match_stat_rewards_earn')),
												REWARDINFO2 	= rewardInfo1 or '',
												REWARDINFO3 	= rewardInfo2 or '',
												TEMPLATE 		= template,
												TIME_AWARDED	= itemTable.TIME_AWARDED,
											})
										end
									elseif ((itemTable.GOLD_COINS) and (itemTable.GOLD_COINS > 0)) or ((itemTable.SILVER_COINS) and (itemTable.SILVER_COINS > 0)) then
										local rewardInfo1, rewardInfo2 = nil, nil
										local templateIcon = '/ui/fe2/store/silver_coins.tga'

										if (itemTable.GOLD_COINS) and (itemTable.GOLD_COINS > 0) then
										    templateIcon = "/ui/fe2/store/gold_coins.tga"
											rewardInfo1 = Translate('match_stat_rewards_text_gold_prize', 'value', itemTable.GOLD_COINS)
											rewardInfo1 = '+'..rewardInfo1
										end
										if (itemTable.SILVER_COINS) and (itemTable.SILVER_COINS > 0) then
											rewardInfo2 = Translate('match_stat_rewards_text_silver_prize', 'value', itemTable.SILVER_COINS)
											rewardInfo2 = '+'..rewardInfo2
										end
										if (not rewardInfo1) and (rewardInfo2) then
											rewardInfo1 = rewardInfo2
											rewardInfo2 = nil
										end

										table.insert(upcomingRewards, {
											HEADER 			= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_header') 		or Translate('match_stat_rewards_header', 'play', headerPrefix, 'value', (gameRequirement), 'withfriends', headerSuffix, 'gametype', headerGameType),
											SUBHEADER 		= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_subheader') 		or Translate('match_stat_rewards_sub_header', 'played', subHeaderPrefix, 'target', (gameRequirement), 'withfriends', string.lower(headerSuffix), 'value', (Min(gameCount, gameRequirement)), 'gametype', headerGameType),
											ICON 			= TranslateOrNil('ms_match_reward_special_'..eventIndex..'_'..itemIndex..'_icon') 			or templateIcon,
											TYPE 			= 'earning',
											STARTDATE 		= startDate,
											ENDDATE 		= endDate,
											CHECKMARK 		= 'false',
											COMPLETION 		= FtoP(completion),
											REWARDINFO1 	= string.upper(Translate('match_stat_rewards_earn')),
											REWARDINFO2 	= rewardInfo1 or '',
											REWARDINFO3 	= rewardInfo2 or '',
											TEMPLATE 		= template,
											TIME_AWARDED	= itemTable.TIME_AWARDED,
										})
									else
										printdb('^r'..Translate('match_stat_rewards_header', 'play', headerPrefix, 'value', (gameRequirement), 'withfriends', headerSuffix, 'gametype', headerGameType)..'not added to upcomingRewards!')
										printdb('^p^:itemTable: ')
										if (GetCvarBool('ui_dev')) then
											printTable(itemTable)
										end
									end

								end
							else
								printdb('^o matchStats.playerStatus[eventIndex] does not exist - cannot display reward data')
							end
						end
					end

					-- sort history rewards by timestamp
					table.sort(rewardHistory, function(a,b) return tonumber(a.TIME_AWARDED) > tonumber(b.TIME_AWARDED) end )

					--printdb('^r^: Process player performance data')

					local experience_total_current_level 	= interface:UICmd([[GetAccountExperienceForLevel(]]..account_level_old..[[)]])
					local experience_total_next_level 	= interface:UICmd([[GetAccountExperienceForLevel(]]..(account_level_old + 1)..[[)]])
					local experience_into_current_level = account_exp - experience_total_current_level
					local experience_req_current_level = experience_total_next_level - experience_total_current_level
					local oldExperiencePercent = experience_into_current_level / experience_req_current_level
					local newExperiencePercent = account_exp_delta / experience_req_current_level

					-- Game A Day
					local consecutiveBonusDay = floor((Max(perf_first_gc, 4) - 4) / 2)
					local nextConsecutiveBonus = floor((Min((consecutiveBonusDay + 1), 5) * 2) + 4)

					-- Streak
					local consecutiveStreakBonusGame = floor(Max(perf_consec_gc, 1))
					local nextConsecutiveStreakBonus = floor(Min(consecutiveStreakBonusGame + 1, 6))

					local socialBonusLevel
					if (perf_social_bonus_gc >= 10) then
						socialBonusLevel = 2
					elseif (perf_social_bonus_gc >= 4) then
						socialBonusLevel = 1
					else
						socialBonusLevel = 0
					end

					-- progstats_full_container event 1
					HideProgressStatsUI()

					-- progstats_full_container event 0
					local function UpdateMatchBreakdownItem(startDelay, itemName, silverEarned, xpEarned)

						GetWidget('ms_rewards_'..itemName..'_breakdown_item'):Sleep(startDelay, function()
							GetWidget('ms_rewards_'..itemName..'_breakdown_item'):FadeIn(250)

							GetWidget('ms_rewards_'..itemName..'_breakdown_item_xp_label'):SetVisible(false)
							GetWidget('progiconpulse_'..itemName..'_breakdown_item_xp_icon'):SetVisible(false)

							GetWidget('ms_rewards_'..itemName..'_breakdown_item_sc_label'):SetVisible(false)
							GetWidget('ms_rewards_'..itemName..'_breakdown_item_sc_icon'):SetVisible(false)

							if (xpEarned) and (xpEarned > 0) then
								lib_Anim.AnimateNumericLabel('ms_rewards_'..itemName..'_breakdown_item_xp_label', 0, 0, xpEarned, false, '+')
								lib_Anim.AnimateIconPulse(itemName..'_breakdown_item_xp_icon')
							end

							if (silverEarned) and (silverEarned > 0) then
								lib_Anim.AnimateNumericLabel('ms_rewards_'..itemName..'_breakdown_item_sc_label', 0, 0, silverEarned, false, '+')
								GetWidget('ms_rewards_'..itemName..'_breakdown_item_sc_icon'):UICmd([[FadeIn(50); StartAnim();]])
							end
						end)

						return startDelay + MS_ANIM_DELAY
					end

					local function UpdateBonusMatchBreakdownItem(startDelay, silverEarnedGCA, silverEarnedCyber)
						GetWidget('ms_rewards_gca_breakdown_item'):Sleep(startDelay, function()
							GetWidget('ms_rewards_gca_breakdown_item'):FadeIn(250)

							GetWidget('ms_rewards_gca_breakdown_item_xp_label'):SetVisible(false)
							GetWidget('progiconpulse_gca_breakdown_item_xp_icon'):SetVisible(false)

							GetWidget('ms_rewards_gca_breakdown_item_sc_label'):SetVisible(false)
							GetWidget('ms_rewards_gca_breakdown_item_sc_icon'):SetVisible(false)

							if (silverEarnedCyber > 0) then
								GetWidget('ms_rewards_gca_breakdown_item_coinroot1'):SetX('-13h')
							else
								GetWidget('ms_rewards_gca_breakdown_item_coinroot1'):SetX('0')
							end

							GetWidget('ms_rewards_gca_breakdown_item_coinroot1'):SetVisible(silverEarnedGCA > 0)
							GetWidget('ms_rewards_gca_breakdown_item_coinroot2'):SetVisible(silverEarnedCyber > 0)


							if (silverEarnedGCA) and (silverEarnedGCA > 0) then
								lib_Anim.AnimateNumericLabel('ms_rewards_gca_breakdown_item_sc_label', 0, 0, silverEarnedGCA, false, '+')
								GetWidget('ms_rewards_gca_breakdown_item_sc_icon'):UICmd([[FadeIn(50); StartAnim();]])
							end

							if (silverEarnedCyber) and (silverEarnedCyber > 0) then
								lib_Anim.AnimateNumericLabel('ms_rewards_gca_breakdown_item_sc_label2', 0, 0, silverEarnedCyber, false, '+')
								GetWidget('ms_rewards_gca_breakdown_item_sc_icon2'):UICmd([[FadeIn(50); StartAnim();]])
							end
						end)

						return startDelay + MS_ANIM_DELAY
					end

					local function UpdatePlayerBreakdownItem(startDelay, itemName, oldValue, newValue, step, silverEarned, xpEarned, silverReward, experienceReward)
						oldValue, newValue, step, silverEarned, xpEarned, silverReward, experienceReward = tonumber(oldValue), tonumber(newValue), tonumber(step), tonumber(silverEarned), tonumber(xpEarned), tonumber(silverReward), tonumber(experienceReward)

						local oldRemainder 		=  	(oldValue) % step
						local remainder 		= 	(newValue + oldValue) % step
						local nextRewardIn 		= 	step - remainder

						GetWidget('ms_rewards_'..itemName..'_prog_bar'):Sleep(startDelay, function()
							GetWidget('ms_rewards_'..itemName..'_prog_bar'):FadeIn(150)

							lib_Anim.AnimateProgressBar(itemName, 1, oldRemainder / step, (oldRemainder + newValue) / step, (oldRemainder + newValue), step, true, startDelay)

							GetWidget('progbar_'..itemName..'_label_left'):SetText(oldValue)
							GetWidget('progbar_'..itemName..'_label_right'):SetX('88%')
							GetWidget('progbar_'..itemName..'_label_right'):SlideX('88%', 1)
							if (newValue > 0) then

								lib_Anim.AnimateNumericLabel('progbar_'..itemName..'_label_right', 0, 0, tonumber(newValue), false, '+')

								GetWidget('progbar_'..itemName..'_label_left'):Sleep(2500, function()
									GetWidget('progbar_'..itemName..'_label_right'):SlideX('0.4h', 750)
									GetWidget('progbar_'..itemName..'_label_right'):FadeOut(750)
									lib_Anim.AnimateNumericLabel('progbar_'..itemName..'_label_left', 0, tonumber(oldValue), tonumber(oldValue + newValue), false)
								end)
							else
								GetWidget('progbar_'..itemName..'_label_right'):SetText('')
							end

							GetWidget('ms_rewards_'..itemName..'_breakdown_item_xp_label'):SetVisible(false)
							GetWidget('progiconpulse_'..itemName..'_breakdown_item_xp_icon'):SetVisible(false)

							GetWidget('ms_rewards_'..itemName..'_breakdown_item_sc_label'):SetVisible(false)
							GetWidget('ms_rewards_'..itemName..'_breakdown_item_sc_icon'):SetVisible(false)

							if ((xpEarned) and (xpEarned > 0)) or ((silverEarned) and (silverEarned > 0)) then
								GetWidget('ms_rewards_'..itemName..'_prog_bar_goal'):SetText('')
								if (xpEarned) and (xpEarned > 0) then
									lib_Anim.AnimateNumericLabel('ms_rewards_'..itemName..'_breakdown_item_xp_label', 0, 0, xpEarned, false, '+')
									lib_Anim.AnimateIconPulse(itemName..'_breakdown_item_xp_icon')
								end
								if (silverEarned) and (silverEarned > 0) then
									lib_Anim.AnimateNumericLabel('ms_rewards_'..itemName..'_breakdown_item_sc_label', 0, 0, silverEarned, false, '+')
									GetWidget('ms_rewards_'..itemName..'_breakdown_item_sc_icon'):UICmd([[FadeIn(50); StartAnim();]])
								end
							else
								GetWidget('ms_rewards_'..itemName..'_prog_bar_goal'):SetText(Translate('match_stat_rewards_title_label_reward_away', 'value', nextRewardIn))
							end

						end)

						if (((oldRemainder + newValue) / step) < 1.0) and ((((oldRemainder + newValue) / step) > 0.85) or true) then

							local rewardInfo1, rewardInfo2 = '', ''
							if (silverReward) and (silverReward > 0) then
								rewardInfo1 = Translate('match_stat_rewards_text_silver_prize', 'value', silverReward)
							end
							if (experienceReward) and (experienceReward > 0) then
								rewardInfo2 = Translate('match_stat_rewards_text_exp_prize', 'value', experienceReward)
							end
							table.insert(tempUpcomingMilestones, {
								HEADER 			= Translate('match_stat_rewards_header_'..itemName, 'value', (step)),
								SUBHEADER 		= Translate('match_stat_rewards_subheader_'..itemName, 'value', (oldRemainder + newValue), 'target', step),
								ICON 			= '/ui/fe2/store/silver_coins.tga',
								TYPE 			= 'earning',
								STARTDATE 		= '',
								ENDDATE 		= '',
								CHECKMARK 		= 'false',
								COMPLETION 		= ((oldRemainder + newValue) / step),
								REWARDINFO1 	= string.upper(Translate('match_stat_rewards_earn')),
								REWARDINFO2 	= '+'..rewardInfo1,
								REWARDINFO3 	= '+'..rewardInfo2,
								TEMPLATE 		= 'ms_rewards_reward_page_template',
								TIME_AWARDED	= '',
							})
						end

						return startDelay + MS_ANIM_DELAY
					end

					-- MILESTONES (Player Breakdown)
					-- wins
					local startDelay = 0
					startDelay = UpdatePlayerBreakdownItem(startDelay, 'reward_wins', perf_wins, perf_wins_delta, matchStats.milestonesTable[1].STEP, perf_wins_gc, perf_wins_exp, matchStats.milestonesTable[1].SILVER_REWARD, matchStats.milestonesTable[1].EXPERIENCE_REWARD)

					-- kills
					startDelay = UpdatePlayerBreakdownItem(startDelay, 'reward_kills', perf_herokills, perf_herokills_delta, matchStats.milestonesTable[2].STEP, perf_herokills_gc, perf_herokills_exp, matchStats.milestonesTable[2].SILVER_REWARD, matchStats.milestonesTable[2].EXPERIENCE_REWARD)

					-- assists
					startDelay = UpdatePlayerBreakdownItem(startDelay, 'reward_assists', perf_heroassists, perf_heroassists_delta, matchStats.milestonesTable[3].STEP, perf_heroassists_gc, perf_heroassists_exp, matchStats.milestonesTable[3].SILVER_REWARD, matchStats.milestonesTable[3].EXPERIENCE_REWARD)

					-- wards
					startDelay = UpdatePlayerBreakdownItem(startDelay, 'reward_wards', perf_wards, perf_wards_delta, matchStats.milestonesTable[4].STEP, perf_wards_gc, perf_wards_exp, matchStats.milestonesTable[4].SILVER_REWARD, matchStats.milestonesTable[4].EXPERIENCE_REWARD)

					-- smackdowns
					startDelay = UpdatePlayerBreakdownItem(startDelay, 'reward_smackdowns', perf_smackdown, perf_smackdown_delta, matchStats.milestonesTable[5].STEP, perf_smackdown_gc, perf_smackdown_exp, matchStats.milestonesTable[5].SILVER_REWARD, matchStats.milestonesTable[5].EXPERIENCE_REWARD)

					-- MATCH BREAKDOWN
					-- match finished
					startDelay = UpdateMatchBreakdownItem(startDelay, 'match_finished', (perf_victory_gc + limited_stats_coins + perf_bots_gc), perf_victory_exp)
					if (matchStats.winningTeam == tonumber(team)) then
						GetWidget('ms_rewards_match_finished_breakdown_item_title'):SetText(Translate('ms_match_breakdown_item_match_victory'))
					else
						GetWidget('ms_rewards_match_finished_breakdown_item_title'):SetText(Translate('ms_match_breakdown_item_match_finished'))
					end

					-- Game A Day (First)
					startDelay = UpdateMatchBreakdownItem(startDelay, 'first_match', perf_first_gc, perf_first_exp)
					GetWidget('ms_rewards_first_match_breakdown_item_title'):SetText(Translate('ms_match_breakdown_item_first_match', 'value', consecutiveBonusDay)) -- ms_match_breakdown_item_first_match_value
					if (perf_first_gc) and (perf_first_gc > 0) then
						GetWidget('ms_rewards_first_match_breakdown_item_x1_label'):SetText(Translate('match_stat_rewards_play_tomorrow', 'xp', 20, 'silver', nextConsecutiveBonus))
					else
						GetWidget('ms_rewards_first_match_breakdown_item_x1_label'):SetText(Translate('match_stat_rewards_play_tomorrow', 'xp', 20, 'silver', '6-14'))
					end

					-- Add game of the day to upcoming milestones as high priority
					if (perf_first_gc) and (perf_first_gc > 0) then
						table.insert(tempUpcomingMilestones, {
							HEADER 			= Translate('match_stat_rewards_header_first_match'),
							SUBHEADER 		= Translate('match_stat_rewards_subheader_first_match'),
							ICON 			= '/ui/fe2/store/silver_coins.tga',
							TYPE 			= 'earning',
							STARTDATE 		= '',
							ENDDATE 		= '',
							CHECKMARK 		= 'false',
							COMPLETION 		= 0.80,
							REWARDINFO1 	= string.upper(Translate('match_stat_rewards_bonus')),
							REWARDINFO2 	= Translate('match_stat_rewards_text_silver_prize', 'value', '+'..nextConsecutiveBonus),
							REWARDINFO3 	= Translate('match_stat_rewards_text_exp_prize', 'value', '+20'),
							TEMPLATE 		= 'ms_rewards_reward_page_template',
							TIME_AWARDED	= '',
						})
					end

					-- Social bonus
					startDelay = UpdateMatchBreakdownItem(startDelay, 'social_bonus', perf_social_bonus_gc, perf_social_bonus_exp)
					GetWidget('ms_rewards_social_bonus_breakdown_item_title'):SetText(Translate('ms_match_breakdown_item_social_bonus', 'value', socialBonusLevel))	--ms_match_breakdown_item_social_bonus_value

					-- Add social bonus to upcoming milestones as medium priority
					table.insert(tempUpcomingMilestones, {
						HEADER 			= Translate('match_stat_rewards_header_social_bonus'),
						SUBHEADER 		= Translate('match_stat_rewards_subheader_social_bonus'),
						ICON 			= '/ui/fe2/store/silver_coins.tga',
						TYPE 			= 'earning',
						STARTDATE 		= '',
						ENDDATE 		= '',
						CHECKMARK 		= 'false',
						COMPLETION 		= 0.90,
						REWARDINFO1 	= string.upper(Translate('match_stat_rewards_bonus')),
						REWARDINFO2 	= Translate('match_stat_rewards_play_social_2'),
						REWARDINFO3 	= Translate('match_stat_rewards_play_social_1'),
						TEMPLATE 		= 'ms_rewards_reward_page_template',
						TIME_AWARDED	= '',
					})

					-- Streak (Consecutive bonus)
					startDelay = UpdateMatchBreakdownItem(startDelay, 'consecutive_bonus', perf_consec_gc, perf_consec_exp)
					GetWidget('ms_rewards_consecutive_bonus_breakdown_item_title'):SetText(Translate('ms_match_breakdown_item_consecutive_bonus', 'value', (consecutiveStreakBonusGame-1))) -- ms_match_breakdown_item_consecutive_bonus_value
					if (matchStats.popupNewRewards) then
						GetWidget('ms_rewards_consecutive_bonus_breakdown_item_x1_label'):SetText(Translate('match_stat_rewards_play_consec', 'silver', nextConsecutiveStreakBonus))
					else
						GetWidget('ms_rewards_consecutive_bonus_breakdown_item_x1_label'):SetText(Translate('match_stat_rewards_play_consec', 'silver', nextConsecutiveStreakBonus))
					end

					-- Add consecutive streak to upcoming milestones as low priority
					if (matchStats.popupNewRewards) then
						table.insert(tempUpcomingMilestones, {
							HEADER 			= Translate('match_stat_rewards_header_consecutive_bonus'),
							SUBHEADER 		= Translate('match_stat_rewards_subheader_consecutive_bonus'),
							ICON 			= '/ui/fe2/store/silver_coins.tga',
							TYPE 			= 'earning',
							STARTDATE 		= '',
							ENDDATE 		= '',
							CHECKMARK 		= 'false',
							COMPLETION 		= 0.50,
							REWARDINFO1 	= string.upper(Translate('match_stat_rewards_bonus')),
							REWARDINFO2 	= Translate('match_stat_rewards_play_consec', 'silver', nextConsecutiveStreakBonus),
							REWARDINFO3 	= '',
							TEMPLATE 		= 'ms_rewards_reward_page_template',
							TIME_AWARDED	= '',
						})
					end

					-- gca
					local netcafe_coin_num = tonumber(Match_Stats.netcafe_coin)
					local cyber_bonuscoin_num = tonumber(Match_Stats.cyber_bonuscoin)
					GetWidget('matchstats_breakdown_match_items_root'):SetY('0')
					if (netcafe_coin_num > 0 or cyber_bonuscoin_num > 0) then
						GetWidget('ms_rewards_gca_breakdown_item'):SetVisible(true)
						GetWidget('matchstats_breakdown_match_scroll_mask'):SetVisible(true)
						GetWidget('matchstats_breakdown_match_scroll'):SetVisible(true)
						startDelay = UpdateBonusMatchBreakdownItem(startDelay, netcafe_coin_num, cyber_bonuscoin_num)
					else
						GetWidget('ms_rewards_gca_breakdown_item'):SetVisible(false)
						GetWidget('matchstats_breakdown_match_scroll_mask'):SetVisible(false)
						GetWidget('matchstats_breakdown_match_scroll'):SetVisible(false)
					end

					-- bloodlust
					startDelay = UpdateMatchBreakdownItem(startDelay, 'bloodlust', perf_bloodlust_gc, perf_bloodlust_exp)

					-- immortal
					startDelay = UpdateMatchBreakdownItem(startDelay, 'immortal', perf_ks15_gc, perf_ks15_exp)

					-- annihilation
					startDelay = UpdateMatchBreakdownItem(startDelay, 'annihilation', perf_annihilation_gc, perf_annihilation_exp)

					-- sort milestone rewards by highest completion
					local remainingKeyTable = {}
					local keyTag = 0
					for _ , tempConTable in pairs(tempUpcomingMilestones) do
						keyTag = keyTag + 0.001
						remainingKeyTable[tempConTable.COMPLETION + keyTag] = tempConTable
					end
					-- add prioritised list of UI side milestone upcoming rewards	(social, consecutive, daily, wards, kills, smackdowns, assists, wins)
					for _, milestoneItemTable in pairsByKeys(remainingKeyTable, function(a,b) return tonumber(a) > tonumber(b) end ) do
						table.insert(upcomingMilestones, milestoneItemTable)
					end
					remainingKeyTable = nil
					tempUpcomingMilestones = nil

					-- build rewards
					matchStats.newlyEarnedRewards = newlyEarnedRewards
					InstantiateRewards(newlyEarnedRewards, rewardHistory, upcomingRewards, upcomingMilestones)

					--- sleep allows matchinfosummary data to be stored before continuing
					GetWidget('matchstats_new_rewardsscreen'):Sleep(1, function()

						-- Top Left Player Info
						if (string.find(matchStats.accountSelectedIconUpgrade, 'custom_icon')) then
							--local _, iconIndex, iconIndex2 = string.gsub(matchStats.accountSelectedIconUpgrade, '%:(%d+)', '')
							GetWidget('matchstats_rewards_player_avatar'):UICmd([[SetTextureURL(
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
									SubString(']]..matchStats.accountSelectedIconUpgrade..[[',
										SearchString(']]..matchStats.accountSelectedIconUpgrade..[[', ':', 0) + 1,
										( StringLength(']]..matchStats.accountSelectedIconUpgrade..[[') - (SearchString(']]..matchStats.accountSelectedIconUpgrade..[[', ':', 0) - 1) )
									) #
									'.cai'
								)
							]])
						elseif NotEmpty(matchStats.accountSelectedIconUpgrade) then
							GetWidget('matchstats_rewards_player_avatar'):SetTexture( GetAccountIconTexturePathFromUpgrades(matchStats.accountSelectedIconUpgrade) )
						else
							GetWidget('matchstats_rewards_player_avatar'):SetTexture('/ui/common/ability_coverup.tga')
						end

						GetWidget('matchstats_rewards_player_name'):SetText(nickname)

						if matchStats.showMMR then
							GetWidget('matchstats_rewards_player_rating'):SetText(account_rating_old)
							GetWidget('matchstats_rewards_player_rating_add'):SetX('9.0h')
							GetWidget('matchstats_rewards_player_rating_add'):SlideX('9.0h', 1)
							if (tonumber(account_rating_delta) < 0) then
								GetWidget('matchstats_rewards_player_rating_add'):SetColor('1.0 0.3 0.0')
								lib_Anim.AnimateNumericLabel('matchstats_rewards_player_rating_add', 0, 0, tonumber(account_rating_delta), true, '')
							elseif (tonumber(account_rating_delta) > 0) then
								GetWidget('matchstats_rewards_player_rating_add'):SetColor('0.0 1.0 0.0')
								lib_Anim.AnimateNumericLabel('matchstats_rewards_player_rating_add', 0, 0, tonumber(account_rating_delta), true, '+')
							end

							GetWidget('matchstats_rewards_player_rating'):Sleep(2500, function()
								GetWidget('matchstats_rewards_player_rating_add'):SlideX('0.4h', 500)
								GetWidget('matchstats_rewards_player_rating_add'):FadeOut(500)
								lib_Anim.AnimateNumericLabel('matchstats_rewards_player_rating', 0, tonumber(account_rating_old), tonumber(account_rating_old + account_rating_delta), true)
							end)
						else
							GetWidget('matchstats_rewards_player_rating'):SetText("")
							GetWidget('matchstats_rewards_player_rating_add'):SetText("")
						end

						-- Update the total experience if we've just leveled
						if(tonumber(account_exp) + tonumber(account_exp_delta) > tonumber(experience_total_next_level)) then
							experience_total_next_level = interface:UICmd([[GetAccountExperienceForLevel(]]..(account_level_old + 2)..[[)]])
						end

						lib_Anim.AnimateProgressBar('reward_xp', 1, oldExperiencePercent, (oldExperiencePercent + newExperiencePercent), account_exp + account_exp_delta, experience_total_next_level, false, 0)

						GetWidget('matchstats_rewards_player_lvl'):SetText(account_level_old)

						-- levelup
						if (levelup_coins > 0) then
							GetWidget('matchstats_rewards_player_lvl'):Sleep(3200, function()
								GetWidget('matchstats_rewards_player_lvl'):SetText(floor(account_level_old + 1))
								GetWidget('ms_rewards_levelup_label'):FadeIn(150)
								GetWidget('ms_rewards_levelup_breakdown_item_sc_icon'):SetVisible(false)
								GetWidget('ms_rewards_levelup_breakdown_item_sc_icon'):UICmd([[FadeIn(50); StartAnim();]])
								lib_Anim.AnimateNumericLabel('ms_rewards_levelup_breakdown_item_sc_label', 0, 0, (levelup_coins), false, '+')
							end)
						else
							GetWidget('ms_rewards_levelup_label'):SetVisible(false)
							GetWidget('ms_rewards_levelup_breakdown_item_sc_icon'):SetVisible(false)
							GetWidget('ms_rewards_levelup_breakdown_item_sc_label'):SetVisible(false)
						end

						lib_Anim.AnimateNumericLabel('matchstats_rewards_player_total_silver', 1, (matchStats.accountTotalSilver - perf_total_silver_new), matchStats.accountTotalSilver, false)
						GetWidget('matchstats_rewards_player_total_silver_icon'):UICmd([[FadeIn(50); StartAnim();]])

						-- Top Left Total Earned
						GetWidget('matchstats_rewards_total_earned'):FadeIn(250)

						lib_Anim.AnimateNumericLabel('matchstats_rewards_total_earned_xp', 0, 0, perf_total_experience_new, false, '+')
						lib_Anim.AnimateIconPulse('total_earned_xp_icon')

						lib_Anim.AnimateNumericLabel('matchstats_rewards_total_earned_silver', 0, 0, perf_total_silver_new, false, '+')
						GetWidget('matchstats_rewards_total_earned_silver_icon'):UICmd([[FadeIn(50); StartAnim();]])

						if (#matchStats.newlyEarnedRewards > 0) or (#newlyEarnedRewards > 0) then
							GetWidget('matchstats_rewards_player_rewards_earned'):FadeIn(250)
						else
							GetWidget('matchstats_rewards_player_rewards_earned'):FadeOut(250)
						end
					end)
				end)
			end
			GetWidget('matchstats_new_rewardsscreen'):RefreshCallbacks()
		end

		numPlayers = numPlayers + 1
		matchStats.tAwardsPlayer = {}
		matchStats.tAwardsGroup = {}

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

		--printdb('^y^: MatchInfoPlayer: ' .. position)
		--printTableDeep(matchStats.tAwardsPlayer)
	end
end
for i=0,4,1 do
	interface:RegisterWatch('MatchInfoPlayer'..i, function(...) MatchInfoPlayer(i, ...) end)
end

--[[
function Match_Stats.ClickedSimpleStats()
	ShowSimpleStats()
end
--]]

function Match_Stats.ClickedDetailedStats()
	ShowDetailedStats()
end

function Match_Stats.ClickedRewards()
	ShowRewards()
end

local function DoRewardHistoryScroll()
	if (matchStats.scrollState == 1) then
		GetWidget('ms_rewards_scrollbox_vscroll'):UICmd([[SetScrollbarValue(GetScrollbarValue()+1)]])
		GetWidget('ms_rewards_scrollbox'):Sleep(1, function() DoRewardHistoryScroll() end)
	elseif (matchStats.scrollState == -1) then
		GetWidget('ms_rewards_scrollbox_vscroll'):UICmd([[SetScrollbarValue(GetScrollbarValue()-1)]])
		GetWidget('ms_rewards_scrollbox'):Sleep(1, function() DoRewardHistoryScroll() end)
	end
end

function Match_Stats.StartRewardHistoryScrollDown()
	GetWidget('ms_rewards_scrollbox'):UICmd([[SetVScrollbarStep('1')]])
	matchStats.scrollState = 1
	DoRewardHistoryScroll()
end

function Match_Stats.StartRewardHistoryScrollUp()
	GetWidget('ms_rewards_scrollbox'):UICmd([[SetVScrollbarStep('1')]])
	matchStats.scrollState = -1
	DoRewardHistoryScroll()
end

function Match_Stats.StopRewardHistoryScroll()
	matchStats.scrollState = 0
end

function Match_Stats.InstantiateEarnedRewards()
	if (#matchStats.newlyEarnedRewards > 0) then
		lib_Card:DisplayCardception(matchStats.newlyEarnedRewards)
	end
end

function Match_Stats.InitRewardsScroller(sourceWidget)
	sourceWidget:SetCallback('onslide', function()
		scrollValue = tonumber(GetWidget('ms_rewards_scrollbox_vscroll'):UICmd([[GetScrollbarValue()]])) or 0
		scrollMax = tonumber(GetWidget('ms_rewards_scrollbox_vscroll'):UICmd([[GetScrollbarMaxValue()]])) or 500
		scrollMin = tonumber(GetWidget('ms_rewards_scrollbox_vscroll'):UICmd([[GetScrollbarMinValue()]])) or 1
		if (scrollValue > scrollMin) then
			Set('ms_rewards_canScrollUp', 'true', 'bool')
			--GetWidget('ms_rewards_scrollbox_scroller_up'):FadeIn(150)
		else
			Set('ms_rewards_canScrollUp', 'false', 'bool')
			--GetWidget('ms_rewards_scrollbox_scroller_up'):FadeOut(150)
		end
		if (scrollValue < scrollMax) then
			Set('ms_rewards_canScrollDown', 'true', 'bool')
			--GetWidget('ms_rewards_scrollbox_scroller_down'):FadeIn(150)
		else
			Set('ms_rewards_canScrollDown', 'false', 'bool')
			--GetWidget('ms_rewards_scrollbox_scroller_down'):FadeOut(150)
		end
	end)
	sourceWidget:RefreshCallbacks()
end

--[[
milestones, ARRAY:
	wins, ARRAY:
	  exp, STRING: 200
	  gc, STRING: 10
	  step, STRING: 50
	herokills, ARRAY:
	  exp, STRING: 100
	  gc, STRING: 5
	  step, STRING: 250
	heroassists, ARRAY:
	  exp, STRING: 100
	  gc, STRING: 5
	  step, STRING: 250
	wards, ARRAY:
	  exp, STRING: 100
	  gc, STRING: 5
	  step, STRING: 50
	smackdown, ARRAY:
	  exp, STRING: 50
	  gc, STRING: 1
	  step, STRING: 10
--]]
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
		--[[
		for index, rewardTable in pairs(matchStats.milestonesTable) do
			printdb('matchStats.milestonesTable: '..index)
			printTable(rewardTable)
		end
		--]]
	end
end
interface:RegisterWatch('MatchMilestones', function(...) MatchMilestones(...) end)

--[[
	rewards, ARRAY:
	  items, ARRAY:
		1, ARRAY:
		  0, ARRAY:
			item_id, STRING: 1
			event_id, STRING: 1
			games_required, STRING: 3
			wins_required, STRING: 0
			social_games_required, STRING: 0
			active, STRING: 1
			start_time, NULL
			end_time, NULL
			product_id, STRING: 639
			charge_id, NULL
			duration_id, NULL
			points, NULL
			mmpoints, NULL
			name, STRING: Hero_Tundra.Hero
			type, STRING: Hero
			local_content, STRING: /heroes/tundra/icon.tga
			unix_start_time
			unix_end_time
--]]
local function MatchRewardsItems(_, matchRewardsItemsData)	-- fires first
	printdb('^r^: MatchRewardsItems')
	printdb('^r^: matchRewardsItemsData = ' .. tostring(matchRewardsItemsData) )

	if NotEmpty(matchRewardsItemsData) then
		local itemTable
		matchStats.rewardsTable	= matchStats.rewardsTable or {}

		for _, dataString in pairs(explode('`', matchRewardsItemsData)) do
			for _, itemString in pairs(explode('|', dataString)) do
				itemTable = explode(',', itemString)

				printdb('^g items itemTable[2] = ' .. tostring(itemTable[2]) )
				if (GetCvarBool('ui_dev')) then
					printTable(itemTable)
				end

				local eventID = tonumber(itemTable[2])
				local itemID = tonumber(itemTable[1])

				if (eventID) and (itemID) then

					matchStats.rewardsTable[eventID] 					=		matchStats.rewardsTable[eventID] or  {}
					matchStats.rewardsTable[eventID][itemID] 			=		matchStats.rewardsTable[eventID][itemID] or  {}

					matchStats.rewardsTable[eventID][itemID].ITEM_ID 				= itemID
					matchStats.rewardsTable[eventID][itemID].EVENT_ID 				= eventID
					matchStats.rewardsTable[eventID][itemID].GAMES_REQUIRED 		= tonumber(itemTable[3])
					matchStats.rewardsTable[eventID][itemID].WINS_REQUIRED 			= tonumber(itemTable[4])
					matchStats.rewardsTable[eventID][itemID].SOCIAL_GAMES_REQUIRED 	= tonumber(itemTable[5])
					matchStats.rewardsTable[eventID][itemID].BOTS_GAMES_REQUIRED 	= tonumber(itemTable[6]) or 0
					matchStats.rewardsTable[eventID][itemID].RIFTWARS_GAMES_REQUIRED= tonumber(itemTable[7]) or 0
					matchStats.rewardsTable[eventID][itemID].MIDWARS_REQUIRED		= tonumber(itemTable[8]) or 0
					matchStats.rewardsTable[eventID][itemID].ACTIVE 				= itemTable[9]
					matchStats.rewardsTable[eventID][itemID].START_TIME 			= tonumber(itemTable[10])
					matchStats.rewardsTable[eventID][itemID].END_TIME 				= tonumber(itemTable[11])
					matchStats.rewardsTable[eventID][itemID].PRODUCT_ID 			= itemTable[12]
					matchStats.rewardsTable[eventID][itemID].CHARGE_ID 				= itemTable[13]
					matchStats.rewardsTable[eventID][itemID].DURATION_ID 			= itemTable[14]
					matchStats.rewardsTable[eventID][itemID].GOLD_COINS 			= (tonumber(itemTable[15]) or 0)
					matchStats.rewardsTable[eventID][itemID].SILVER_COINS 			= (tonumber(itemTable[16]) or 0)
					matchStats.rewardsTable[eventID][itemID].NAME 					= itemTable[17]
					matchStats.rewardsTable[eventID][itemID].TYPE 					= itemTable[18]
					matchStats.rewardsTable[eventID][itemID].TEXTURE 				= itemTable[19]
					-- Start time
					-- End Time
					matchStats.rewardsTable[eventID][itemID].GATED_GAMES_REQUIRED 	= tonumber(itemTable[22]) or 0
					matchStats.rewardsTable[eventID][itemID].R_GOLD_COINS 			= tonumber(itemTable[23]) or 0
					matchStats.rewardsTable[eventID][itemID].R_SILVER_COINS 		= tonumber(itemTable[24]) or 0

					if (GetCvarBool('ui_dev')) then
						printTable(matchStats.rewardsTable[eventID][itemID])
					end
				end

			end
		end
		---[[
		for index, rewardTable in pairs(matchStats.rewardsTable) do
			printdb('matchStats.rewardsTable: '..index)
			if (GetCvarBool('ui_dev')) then
				printTable(rewardTable)
			end
		end
		--]]
	end
end
interface:RegisterWatch('MatchRewardsItems', function(...) MatchRewardsItems(...) end)

--[[
	player_rewards, ARRAY:
	  3633166, ARRAY:
		1, ARRAY:
		  reward_id, STRING: 4
		  super_id, STRING: 3633166
		  event_id, STRING: 1
		  match_id, STRING: 8294729
		  item_id, STRING: 1
		  time_given, STRING: 2012-06-20 15:36:33
		  unix_time_given, STRING 0983098092384092384
--]]
local function MatchRewardsPlayerRewards(_, matchRewardsPlayerHistoryData)	-- fires third
	printdb('^r^: MatchRewardsPlayerRewards')
	printdb('^r^: matchRewardsPlayerHistoryData = ' .. tostring(matchRewardsPlayerHistoryData) )

	if NotEmpty(matchRewardsPlayerHistoryData) then
		local itemTable
		matchStats.rewardsTable	= matchStats.rewardsTable or {}

		for _, dataString in pairs(explode('`', matchRewardsPlayerHistoryData)) do
			for index, itemString in pairs(explode('|', dataString)) do
				itemTable = explode(',', itemString)

				printdb('player_rewards itemTable[3] = ' .. tostring(itemTable[3]) )

				local eventID = tonumber(itemTable[3])
				local itemID = tonumber(itemTable[5])

				if (eventID) and (itemID) then

					matchStats.rewardsTable[eventID] 						=		matchStats.rewardsTable[eventID] or  {}
					matchStats.rewardsTable[eventID][itemID] 				=		matchStats.rewardsTable[eventID][itemID] or  {}

					matchStats.rewardsTable[eventID][itemID].REWARD_ID 				= itemTable[1]
					matchStats.rewardsTable[eventID][itemID].SUPER_ID 				= itemTable[2]
					matchStats.rewardsTable[eventID][itemID].EVENT_ID 				= eventID
					matchStats.rewardsTable[eventID][itemID].MATCH_ID 				= itemTable[4]
					matchStats.rewardsTable[eventID][itemID].ITEM_ID 				= itemID
					matchStats.rewardsTable[eventID][itemID].TIME_AWARDED 			= itemTable[6]

					printTable(matchStats.rewardsTable[eventID][itemID])

				end

			end
		end
		---[[
		if (matchStats.historyRewardsTable) then
			for index, rewardTable in pairs(matchStats.historyRewardsTable) do
				printdb('matchStats.historyRewardsTable: '..index)
				printTable(rewardTable)
			end
		end
		--]]
	end
end
interface:RegisterWatch('MatchRewardsPlayerRewards', function(...) MatchRewardsPlayerRewards(...) end)

--[[
	player_status, ARRAY:
	  3633166, ARRAY:
		1, ARRAY:
		  super_id, STRING: 3633166
		  event_id, STRING: 1
		  games_played, STRING: 71
		  wins, STRING: 0
		  social_games, STRING: 3
		  last_game_time, STRING: 2012-06-21 12:55:11
		  unix_last_game_time, STRING 0983098092384092384
--]]
local function MatchRewardsPlayerStatus(_, matchRewardsPlayerStatusData)	-- fires second
	printdb('^r^: MatchRewardsPlayerStatus')
	printdb('matchRewardsPlayerStatusData: ' .. tostring(matchRewardsPlayerStatusData) )

	if NotEmpty(matchRewardsPlayerStatusData) then
		matchStats.playerStatus =  matchStats.playerStatus or {}
		local itemTable
		for index, itemString in pairs(explode('`', matchRewardsPlayerStatusData)) do
			itemTable = explode('|', itemString)

			if (GetCvarBool('ui_dev')) then
				printTable(itemTable)
			end

			local eventID = tonumber(itemTable[2])

			if (eventID) then

				matchStats.playerStatus[eventID] 					=	matchStats.playerStatus[eventID] or  {}
				matchStats.playerStatus[eventID].SUPER_ACCOUNT_ID 	=	itemTable[1]
				matchStats.playerStatus[eventID].EVENT_ID 			=	eventID
				matchStats.playerStatus[eventID].GAMES_PLAYED		=	tonumber(itemTable[3])
				matchStats.playerStatus[eventID].GAMES_WON 			=	tonumber(itemTable[4])
				matchStats.playerStatus[eventID].SOCIAL_GAMES		=	tonumber(itemTable[5])
				-- last game
				matchStats.playerStatus[eventID].LAST_GAME_TIME		=	itemTable[7]
				matchStats.playerStatus[eventID].GATED_GAMES		=	tonumber(itemTable[8]) or 0
				matchStats.playerStatus[eventID].BOT_GAMES			=	tonumber(itemTable[9]) or 0
				matchStats.playerStatus[eventID].RIFTWARS_GAMES		=	tonumber(itemTable[10]) or 0
				matchStats.playerStatus[eventID].MIDWARS_GAMES		=	tonumber(itemTable[11]) or 0
				-- alt gold
				-- alt silver
				if (GetCvarBool('ui_dev')) then
					printTable(matchStats.playerStatus[eventID])
				end
			end
		end
	end
end
interface:RegisterWatch('MatchRewardsPlayerStatus', function(...) MatchRewardsPlayerStatus(...) end)

local function MatchReferralData(_, matchReferralData)
	printdb('^r^: matchReferralData = ' .. tostring(matchReferralData) )
	matchStats.referralStatus = {}
	matchStats.referralOffset = nil

	if (matchReferralData) and NotEmpty(matchReferralData) and GetCvarBool('ui_raf_enabled') then

		local function ShowReferralSelection(referrersTable)

			matchStats.referralStatus = referrersTable

			groupfcall('ms_slot_parents', function(_, widget, _) widget:SetVisible(0) end)

			GetWidget('ms_referral_1_submit_btn'):SetEnabled(0)

			-- printTable(referrersTable)

			for i, userName in pairs(referrersTable) do
				-- println('i = ' .. tostring(i))
				-- println('userName = ' .. tostring(userName))
				if (userName) and NotEmpty(userName) then
					GetWidget('ms_slot' .. i .. '_parent'):SetVisible(1)
					GetWidget('ms_slot' .. i .. '_name'):SetText(userName)
					GetWidget('ms_slot' .. i .. '_namesub'):SetText('')
					-- clear out any selected colors from previous popups
					GetWidget("ms_slot"..i.."_frame1"):SetColor("#183042")
					GetWidget("ms_slot"..i.."_frame2"):SetColor("#183042")
					GetWidget("ms_slot"..i.."_frame1"):SetBorderColor("#183042")
					GetWidget("ms_slot"..i.."_frame2"):SetBorderColor("#183042")
					GetWidget("ms_slot"..i.."_cpanel"):SetColor("#183042")

					-- try to get the account icon
					local chatIcon = nil
					if (GetChatClientInfo) then
						chatIcon = GetChatClientInfo(StripClanTag(userName), "getaccounticontexturepath")
					end

					if (chatIcon and (string.sub(chatIcon, 0, -2) ~= "")) then
						GetWidget('ms_slot' .. i .. '_icon'):SetTexture(string.sub(chatIcon, 0, -2))
					else
						GetWidget('ms_slot' .. i .. '_icon'):SetTexture("/ui/fe2/store/icons/account_icons/default.tga")
					end
				else
					GetWidget('ms_slot' .. i .. '_parent'):SetVisible(0)
				end
			end

			-- interface:UICmd([[Set('_mainmenu_currentpanel', 'main_splash_referral_popup_1'); CallEvent('MainMenuPanelSwitcher');]])
			ResourceManager.LoadContext('referral_popup_1')
			GetWidget('main_splash_referral_popup_1'):FadeIn(150)
		end

		-- local itemTable
		-- for index, itemString in pairs(explode('|', matchReferralData)) do
			-- itemTable = explode('`', itemString)

			-- printTable(itemTable)

			-- for i, userName in pairs(itemTable) do
				-- if IsMe(userName) then
					--
					-- ShowReferralSelection(itemTable)
					-- break
				-- end
			-- end

		-- end

		if CanBeReferred and CanBeReferred() then
			ShowReferralSelection(explode('|', matchReferralData))
		else
			GetWidget('match_replays_tutorial_sleeper2'):Sleep(1000, function()
				if CanBeReferred and CanBeReferred() then
					ShowReferralSelection(explode('|', matchReferralData))
				end
			end)
		end

	end
end
interface:RegisterWatch('MatchReferralData', function(...) MatchReferralData(...) end)

function Match_Stats.ClickedMatchReferral(sourceWidget, slot)

	if (matchStats.referralStatus) and (matchStats.referralStatus[slot]) then
		if (matchStats.referralOffset) then -- set the color on the old referral
			GetWidget("ms_slot"..matchStats.referralOffset.."_frame1"):SetColor("#183042")
			GetWidget("ms_slot"..matchStats.referralOffset.."_frame2"):SetColor("#183042")
			GetWidget("ms_slot"..matchStats.referralOffset.."_frame1"):SetBorderColor("#183042")
			GetWidget("ms_slot"..matchStats.referralOffset.."_frame2"):SetBorderColor("#183042")
			GetWidget("ms_slot"..matchStats.referralOffset.."_cpanel"):SetColor("#183042")
		end
		-- set the color on the new
		GetWidget("ms_slot"..slot.."_frame1"):SetColor("#3F8BC4")
		GetWidget("ms_slot"..slot.."_frame2"):SetColor("#3F8BC4")
		GetWidget("ms_slot"..slot.."_frame1"):SetBorderColor("#3F8BC4")
		GetWidget("ms_slot"..slot.."_frame2"):SetBorderColor("#3F8BC4")
		GetWidget("ms_slot"..slot.."_cpanel"):SetColor("#3F8BC4")

		matchStats.referralOffset = slot

		GetWidget('ms_referral_1_submit_btn'):SetEnabled(1)
	end

end

function Match_Stats.ClickedMatchReferralSubmit()
	if (matchStats.referralOffset) then
		interface:UICmd("SubmitForm('SetAsReferrer', 'f', 'set_ingame_referrer', 'referrer', '"..matchStats.referralStatus[matchStats.referralOffset].."', 'cookie', GetCookie());")

		Trigger("TriggerDialogBoxWebRequest",
			Translate("referrer_header", "username", matchStats.referralStatus[matchStats.referralOffset]),
			"referrer_left_button",
			"general_cancel",
			"SubmitForm('SetAsReferrer', 'f', 'set_ingame_referrer', 'referrer', '"..matchStats.referralStatus[matchStats.referralOffset].."', 'cookie', GetCookie(), 'match_id', _stats_last_match_id);",
			"",
			"referrer_title",
			"referrer_desc",
			"SetAsReferrerStatus",
			"SetAsReferrerResult",
			"",
			"referrer_success_title",
			"referrer_seccess_body"
		)
		GetWidget("main_splash_referral_popup_1"):FadeOut(150)
	end
end

function Match_Stats.Tutorial()

	printdb('^o^: Match_Stats.Tutorial() 1')

	matchStats.tMatchInfoPlayer = {}
	matchStats.tAwardsPlayer = {}
	matchStats.rewardsTable	= {}
	matchStats.playerStatus = {}
	matchStats.newlyEarnedRewards = {}
	matchStats.hasRewardData = false
	matchStats.hasMasteryData = false
	matchStats.hasRankData = false
	matchStats.popupNewRewards = false
	matchStats.isBots = false
	matchStats.isMidwars = false
	matchStats.isKrosMode = false
	matchStats.useAwardsScreen = false
	matchStats.match_id = -1
	lib_Card:HideCardception()
	GetWidget('loadentityprogress'):SetVisible(0)
	--GetWidget('match_stats_simple_silver_value_parent'):SetVisible(0)
	-- GetWidget('match_stats_simple_games_played'):SetVisible(0)
	-- GetWidget('match_stats_riftwars_games_played'):SetVisible(0)
	-- GetWidget('match_stats_bot_games_played'):SetVisible(0)
	GetWidget("match_rewards_bot_games_played"):SetVisible(0)
	GetWidget("match_rewards_mid_games_played"):SetVisible(0)
	GetWidget("match_rewards_rift_games_played"):SetVisible(0)
	GetWidget("match_rewards_norm_games_played"):SetVisible(0)
	GetWidget('matchstats_new_rewardsscreen'):SetVisible(0)
	GetWidget('matchstats_new_rewardsscreen'):ClearCallback('onshow')
	GetWidget('matchstats_new_rewardsscreen'):RefreshCallbacks()
	GetWidget('match_stats_detailed_tab_rewards'):SetEnabled(false)
	ResetSleeps()
	numPlayers = 0

	matchStats.debugNewRewards = true
	matchStats.hasRewardData = true
	matchStats.hasMasteryData = true
	matchStats.hasRankData = true

	GetWidget('nomatchhistory'):SetVisible(0)

	GetWidget('match_replays_tutorial_sleeper'):Sleep(1, function()

		printdb('^o^: Match_Stats.Tutorial() 2')

		-- MatchInfoSummary(sourceWidget,
			-- 'main_stats_tutorial', '1', 'Tutorial', 'Tutorial', 'Tutorial', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0',
			-- 'Tutorial', '1', '1', '1', '1', '1', '1', '1', '1', 'Tutorial', '0', '0', '0', '500',
			-- '0', '0', '0', '0', '0', '1', '1')

		printdb('^o^: MatchInfoSummary - Has Data')

		Set('_stats_last_match_id', tostring(1), 'string')
		matchStats.accountSelectedIconUpgrade = ''
		matchStats.accountTotalSilver = tonumber(UISilverCoins())
		matchStats.winningTeam = tonumber(1)
		matchStats.match_id = 1
		matchStats.isBots = true
		matchStats.isMidwars = false
		matchStats.isKrosMode = false
		matchStats.useAwardsScreen = false
		GetWidget('match_stats_reward_tab_stats'):SetEnabled(0)

		GetWidget('match_replays_tutorial_sleeper'):Sleep(1, function()

		printdb('^o^: Match_Stats.Tutorial() 3')

			MatchInfoPlayer(1, sourceWidget,
				GetAccountName(), '1', '1', '25', '1', '1', '1', '1', '1', '1', '/heroes/pyromancer/icon.tga',
				'', '', '', '', '', '', '0', '0', 'Pyromancer', 'Hero_Pyromancer',
				'0', '0', '0', '0', '0', '0', '0', '0', '0', '0',
				'0', '0', '0', '0', '0', '0', '0', '0', '0', '0',
				'0', '0', '0', '0', '0', '0', '0', '0', '0', '0',
				'0', '0', '1', '0', '0', '0', '', '0', '0', '0',
				'0', '0', '0', '0', '0', '0', '0', '0', '0',
				'0', '0', '0', '0', '0', '0', '0', '0', '0', '0',
				'0', '0', '0', '0', '0', '0', '0', '0', '0', '0',
				'0', '0', '0', '0', '0', '0', '0', '0',
				'0', '0', '0', '500', '1')

			GetWidget('match_replays_tutorial_sleeper'):Sleep(1, function()
				GetWidget('match_replays_tutorial_sleeper'):Sleep(1, function()
					-- Show rewards tab
					printdb('^o^: MatchInfoSummary - Show Rewards')
					matchStats.isAutoLookup = false
					matchStats.popupNewRewards = true
					GetWidget('match_stats_detailed_tab_rewards'):SetEnabled(true)
					ShowRewards()

					Main.walkthroughState = 3
					CheckForWalkthrough()

				end)
			end)

			matchStats.debugNewRewards = false
			matchStats.hasRewardData = false
			matchStats.hasMasteryData = false
			matchStats.hasRankData = false

		end)
	end)

end

function Match_Stats:RequestUpload(matchID)
	if (not matchStats.pendingReplays[matchID]) then
		RequestSMUpload(matchID, 'honreplay')
	end
end

function Match_Stats:HoverReplayStatus()
	local matchID = GetCvarString('_stats_last_replay_id')
	local tipTitle = "game_end_stats_request_btn"
	local tipBody = "game_end_stats_request_tooltip"

	if (matchStats.pendingReplays[matchID]) then
		tipTitle = "match_stats_ondemand_"..matchStats.pendingReplays[matchID].updateType
		tipBody = "match_stats_ondemand_"..matchStats.pendingReplays[matchID].updateType.."_tip"
	end

	Trigger('genericMainFloatingTip', 'true', '30h', '', tipTitle, tipBody, '', '', '3h', '-2h')
end

function Match_Stats:UpdateOnDemandLabel()
	local matchID = GetCvarString('_stats_last_replay_id')
	local str = Translate('game_end_stats_request_btn')
	local button = GetWidget('game_end_stats_request_btn')

	if (matchStats.pendingReplays[matchID]) then
		local state = matchStats.pendingReplays[matchID].updateType
		if ((state == 0) or (state == 1) or (state == 2) or (state == 5) or (state == 6)) then
			str = Translate('match_stats_ondemand_'..state)
		end
	end

	groupfcall('game_end_request_group', function(_,w,_) w:SetText(str) end)
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
		Match_Stats:UpdateOnDemandLabel()

		if (GetWidget('match_stats'):IsVisible() and ((updateType == 7) or (updateType == 3))) then
			interface:UICmd("Set('ui_match_replays_blockSimpleStats', false); ClearEndGameStats(); GetMatchInfo('"..matchID.."');")
		end
	end

	-- only save updates of these types, otherwise we want the ability to rerequest the file
	if (not ((updateType == 1) or (updateType == 5) or (updateType == 6))) then
		matchStats.pendingReplays[matchID] = nil
	end
end
interface:RegisterWatch('UploadReplayStatus', ProcessPendingDownload)