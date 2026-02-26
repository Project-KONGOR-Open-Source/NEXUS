local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort
local interface, interfaceName = object, object:GetName()

Player_Stats_V2 = {}

function Player_Stats_V2:PHP_Unserialize(s)
	if s == nil then println('^rPHP_Unserialize: Input is nil') return nil end
	if type(s) ~= 'string' then println('^rPHP_Unserialize: Input is not string: '..type(s)) return nil end
	if s == '' then return nil end
	
	local pos = 1
	local len_s = #s
	-- println('^gPHP_Unserialize: Starting parse. Len='..len_s..' val='..string.sub(s, 1, 20)..'...')
	
	local function parse()
		if pos > len_s then return nil end
		local type_char = string.sub(s, pos, pos)
		pos = pos + 2 -- skip type and :
		
		if type_char == 'i' then
			local _, next_pos, val = string.find(s, '^([%-?%d]+);', pos)
			if not next_pos then 
				println('^rPHP_Unserialize: Failed to parse integer at pos '..pos)
				return nil 
			end
			pos = next_pos + 1
			return tonumber(val)
		elseif type_char == 's' then
			local _, next_pos, len = string.find(s, '^(%d+):', pos)
			if not next_pos then 
				println('^rPHP_Unserialize: Failed to parse string length at pos '..pos)
				return nil 
			end
			pos = next_pos + 2 -- skip "
			local str = string.sub(s, pos, pos + tonumber(len) - 1)
			pos = pos + tonumber(len) + 2 -- skip ";
			return str
		elseif type_char == 'a' then
			local _, next_pos, len = string.find(s, '^(%d+):{', pos)
			if not next_pos then 
				println('^rPHP_Unserialize: Failed to parse array length at pos '..pos)
				return nil 
			end
			pos = next_pos + 1
			local t = {}
			for i = 1, tonumber(len) do
				local key = parse()
				local val = parse()
				if key ~= nil then t[key] = val end
			end
			pos = pos + 1 -- skip }
			return t
		else
			println('^rPHP_Unserialize: Unknown type char "'..type_char..'" at pos '..pos)
			return nil
		end
	end
	
	return parse()
end

local MASTER_ALLHERO_COLUMN = 5
local MASTER_ALLHERO_ROW = 33
local MASTER_OVERALL_REWARDS_MAX = 40

local _currentNickName = nil
local _waitingMaskCount = 0
local _currentTab = nil

local _heroMastery = {}
local bHeroMasteryRetrieved = false

local _overviewRankTypeActions = {}
local _statsRankTypeActions = {}
local _matchRankTypeActions = {}

local _historicalSeasons = {}

_publicStats = {}
_normalStats = {}
_casualStats = {}
_seasonStatsNormal = {}
_seasonStatsCasual = {}
_seasonHistoryStatsNormal = {}
_seasonHistoryStatsCasual = {}

_mvpScore = 0
_mvpAwards = {}

_matchList = {}

_subAccounts = {}
local _subAccountSwitching = nil

local bPublicStatsRetrieved = false
local bNormalStatsRetrieved = false
local bCasualStatsRetrieved = false
local bNormalSeasonStatsRetrieved = false
local bCasualSeasonStatsRetrieved = false

local bAddFriendClicked = false

local function GetHeroIconPath(hero)
	local folder = hero
	if hero == 'forsakenarcher' then folder = 'forsaken_archer' 
	elseif hero == 'corrupteddisciple' then folder = 'corrupted_disciple' 
	elseif hero == 'sandwraith' then folder = 'sand_wraith' 
	elseif hero == 'witchslayer' then folder = 'witch_slayer' 
	elseif hero == 'dwarfmagi' then folder = 'dwarf_magi' 
	elseif hero == 'flintbeastwood' then folder = 'flint_beastwood' 
	elseif hero == 'doctorrepulsor' then folder = 'doctor_repulsor' 
	elseif hero == 'bombardier' then folder = 'bomb' 
	elseif hero == 'emeraldwarden' then folder = 'emerald_warden' 
	elseif hero == 'monkeyking' then folder = 'monkey_king' 
	elseif hero == 'masterofarms' then folder = 'master_of_arms' 
	elseif hero == 'sirbenzington' then folder = 'sir_benzington' 
	elseif hero == 'kingklout' then folder = 'king_klout' 
	end

	if Empty(folder) then return '/ui/common/ability_coverup.tga' end
	local iconPath = '/heroes/'..folder..'/icon.tga'

	if hero == 'pollywogpriest' then iconPath = '/heroes/pollywogpriest/icons/hero.tga' 
	elseif hero == 'electrician' then iconPath = '/heroes/electrician/icons/hero.tga'
	elseif hero == 'yogi' then iconPath = '/heroes/yogi/icons/hero.tga'
	elseif hero == 'cthulhuphant' then iconPath = '/heroes/cthulhuphant/alt/icon.tga'
	elseif hero == 'artillery' then iconPath = '/heroes/artillery/alt/icon.tga'
	elseif hero == 'geomancer' then iconPath = '/heroes/geomancer/hd_geomancer/icon.tga'
	elseif hero == 'sand_wraith' then iconPath = '/heroes/sand_wraith/hd_sandwraith2/icon.tga'
	elseif hero == 'tremble' then iconPath = '/heroes/tremble/hd_tremble/icon.tga'
	elseif hero == 'tarot' then iconPath = '/heroes/tarot/hd_tarot/icon.tga'
	elseif hero == 'bushwack' then iconPath = '/heroes/bushwack/bushwack_hd/icon.tga'
	end

	return iconPath
end


local function GetHeroMasterType(hero_name)
	local result = 'iron'
	
	if _heroMastery == nil or not bHeroMasteryRetrieved or Empty(hero_name) then 
		return result
	end

	for _, v in ipairs(_heroMastery) do
		if hero_name == v.name then
			result = v.type
			break
		end
	end
	
	return result
end

local function ShouldHideSeasonHistory()
	if not GetCvarBool('cl_GarenaEnable') and not IsMe(_currentNickName) then	-- hide other's season history for naeu
		return true
	end
	return false
end

local function ResetStatsTabUI()
	_publicStats = {}
	_normalStats = {}
	_casualStats = {}
	_seasonStatsNormal = {}
	_seasonStatsCasual = {}
	_seasonHistoryStatsNormal = {}
	_seasonHistoryStatsCasual = {}
	
	bPublicStatsRetrieved = false
	bNormalStatsRetrieved = false
	bCasualStatsRetrieved = false
	bNormalSeasonStatsRetrieved = false
	bCasualSeasonStatsRetrieved = false
end

local function ResetOverviewUI()
	local container = GetWidget('playerstats_overview')
	if container == nil then
		println('^rError: couldn\'t locate widget playerstats_overview!')
		return 
	end
	
	container:GetWidget('playerstats_playericon'):SetTexture('/ui/common/ability_coverup.tga')
 	container:GetWidget('playerstats_playername'):SetText('')
	container:GetWidget('playerstats_playerstanding'):SetVisible(0)
	container:GetWidget('playerstats_clanname'):SetText('')
	container:GetWidget('playerstats_clanrole'):SetTexture('$invis')
	container:GetWidget('playerstats_playerlevel'):SetText('')
	container:GetWidget('playerstats_chatsymbol'):SetTexture('$invis')
	container:GetWidget('playerstats_levelprogress'):SetWidth('0%')
	container:GetWidget('playerstats_levelexp'):SetText('')
	container:GetWidget('playerstats_matches'):SetText('-')
	container:GetWidget('playerstats_disconnects'):SetText('-')
	container:GetWidget('playerstats_leaves'):SetText('-')
	container:GetWidget('playerstats_createdate'):SetText('-')
	container:GetWidget('playerstats_lastmatchdate'):SetText('-')
	
	container:GetWidget('playerstats_overview_season_name'):SetText('-')
	container:GetWidget('playerstats_overview_meter_to_next_rank'):SetValue('0')
	container:GetWidget('playerstats_overview_current_rank_icon'):SetTexture('/ui/fe2/season/nolevel.tga')
	container:GetWidget('playerstats_overview_current_rank_icon'):SetRenderMode('grayscale')
	container:GetWidget('playerstats_overview_current_rank'):SetText(Translate('player_compaign_level_name_0'))
	container:GetWidget('playerstats_overview_highest_rank_icon'):SetTexture('/ui/fe2/season/nolevel.tga')
	container:GetWidget('playerstats_overview_highest_rank_icon'):SetRenderMode('grayscale')
	container:GetWidget('playerstats_overview_highest_rank'):SetText(Translate('player_compaign_level_name_0'))
	if GetCvarBool('cl_GarenaEnable') then
		container:GetWidget('playerstats_overview_summary_garena'):SetVisible(1)
		container:GetWidget('playerstats_overview_summary'):SetVisible(0)
		GetWidget('playerstats_overview_rankwinstreaks_garena'):SetText('-')
		container:GetWidget('playerstats_overview_rankwins_garena'):SetText('-')
		container:GetWidget('playerstats_overview_ranklosses_garena'):SetText('-')
		container:GetWidget('playerstats_overview_rankwinpercent_garena'):SetText('-')
	else
		container:GetWidget('playerstats_overview_summary_garena'):SetVisible(0)
		container:GetWidget('playerstats_overview_summary'):SetVisible(1)
		container:GetWidget('playerstats_overview_rankwins'):SetText('-')
		container:GetWidget('playerstats_overview_ranklosses'):SetText('-')
		container:GetWidget('playerstats_overview_rankwinpercent'):SetText('-')
	end
	
	-- favorite heroes
	for i = 1, 5 do
		container:GetWidget('playerstats_hero_fav_overview_'..i..'_icon'):SetTexture('/ui/common/ability_coverup.tga')
		container:GetWidget('playerstats_hero_fav_overview_'..i..'_percent'):SetText('0%')
		container:GetWidget('playerstats_hero_fav_overview_'..i..'_name'):SetText('---')
		container:GetWidget('playerstats_hero_fav_overview_'..i..'_border'):SetTexture('/ui/fe2/newui/res/homepage/border_iron.png')
	end
	
	GetWidget('playerstats_overview_mvpawards'):ClearChildren()
	
	container:GetWidget('playerstats_overview_mastery_score'):SetText('---')
	
	for i=1, 6 do
		container:GetWidget('playerstats_mastery_level_'..i):SetVisible(false)
	end
end

local function ResetMatchListUI()
	local container = GetWidget('playerstats_match')
	if container == nil then return end
	
	local root = container:GetWidget('playerstats_match_list_root')
	root:ClearChildren()

	local scrollWidget = container:GetWidget('playerstats_match_list_scroll')
	scrollWidget:SetVisible(0)
end

local function ResetSwitchAccountsListUI()
	local container = GetWidget('playerstats_switch_accounts_list')
	if container == nil then return end
	
	local root = container:GetWidget('playerstats_switch_accounts_list_root')
	root:ClearChildren()
	
	local scrollWidget = container:GetWidget('playerstats_switch_accounts_list_scroll')
	scrollWidget:SetVisible(0)
end

local function SubmitInitialForms()
	local value = GetCvarString('_playerstats_overview_type')
	if not NotEmpty(value) then
		if GetCvarBool('cl_GarenaEnable') then
			value = '1'
		else
			value = '0'
		end
	end
	
	local action = _overviewRankTypeActions[value]
	if action == nil then
		println('^r Error: player_stats overview have no available tabs !^*')
		return 
	end

	GetWidget('playerstats_overview_ranktype'):SetSelectedItemByValue(value, false)
	
	SubmitForm('PlayerStatsMastery', 'f', 'show_stats', 'nickname', StripClanTag(_currentNickName), 'cookie', Client.GetCookie(), 'table', 'mastery')
	Player_Stats_V2:ShowWaitingMask()
	SubmitForm(action.form, 'f', action.f, 'nickname', action.nickname, 'cookie', action.cookie, 'table', action.table)
	Player_Stats_V2:ShowWaitingMask()
	SubmitForm('PlayerStatsMVPAwards', 'f', 'get_player_award_summ', 'nickname', StripClanTag(_currentNickName), 'cookie', Client.GetCookie())
	Player_Stats_V2:ShowWaitingMask()
	SubmitForm('GetSeasons', 'f', 'get_seasons', 'nickname', StripClanTag(_currentNickName), 'cookie', Client.GetCookie())
	Player_Stats_V2:ShowWaitingMask()
end

local function SetTipMatchesPlayed(tip_colors, numbers)
	local widget = GetWidget('playerstats_tip_matches_played_content')
	if widget == nil  then return end
	
	local public = NotEmpty(numbers.public) and numbers.public or '0'
	local normal = NotEmpty(numbers.normal) and numbers.normal or '0'
	local casual = NotEmpty(numbers.casual) and numbers.casual or '0'
	local season_normal = NotEmpty(numbers.season_normal) and numbers.season_normal or '0'
	local season_casual = NotEmpty(numbers.season_casual) and numbers.season_casual or '0'
	local pre_season_normal = NotEmpty(numbers.pre_season_normal) and numbers.pre_season_normal or '0'
	local pre_season_casual = NotEmpty(numbers.pre_season_casual) and numbers.pre_season_casual or '0'
	local midwars = NotEmpty(numbers.midwars) and numbers.midwars or '0'

	local displayText = tip_colors.public..Translate('player_stats_public')..': '..public..'\n'..
						tip_colors.season_normal..Translate('player_stats_campaign_normal')..': '..season_normal..'\n'

	if GetCvarBool('cl_GarenaEnable') then
		displayText = displayText..tip_colors.season_casual..Translate('player_stats_campaign_casual')..': '..season_casual..'\n'
	end

	displayText = displayText..
						tip_colors.pre_season_normal..Translate('player_stats_pre_campaign_normal')..': '..pre_season_normal..'\n'

	if GetCvarBool('cl_GarenaEnable') then
		displayText = displayText..tip_colors.pre_season_casual..Translate('player_stats_pre_campaign_casual')..': '..pre_season_casual..'\n'
	end

	displayText = displayText..
						tip_colors.normal..Translate('player_stats_ranked_normal')..': ' ..normal..'\n'..
						tip_colors.casual..Translate('player_stats_ranked_casual')..': '..casual..'\n'..
						tip_colors.midwars..Translate('player_stats_midwars')..': '..midwars..'\n'

	widget:SetText(displayText)
end

local function SetTipDisconnects(tip_colors, numbers)
	local widget = GetWidget('playerstats_tip_disconnects_content')
	if widget == nil  then return end
	
	local public = NotEmpty(numbers.public) and numbers.public or '0'
	local normal = NotEmpty(numbers.normal) and numbers.normal or '0'
	local casual = NotEmpty(numbers.casual) and numbers.casual or '0'
	local season_normal = NotEmpty(numbers.season_normal) and numbers.season_normal or '0'
	local season_casual = NotEmpty(numbers.season_casual) and numbers.season_casual or '0'
	local pre_season_normal = NotEmpty(numbers.pre_season_normal) and numbers.pre_season_normal or '0'
	local pre_season_casual = NotEmpty(numbers.pre_season_casual) and numbers.pre_season_casual or '0'
	local midwars = NotEmpty(numbers.midwars) and numbers.midwars or '0'

	local displayText = tip_colors.public..Translate('player_stats_public')..': '..public..'\n'..
						tip_colors.season_normal..Translate('player_stats_campaign_normal')..': '..season_normal..'\n'

	if GetCvarBool('cl_GarenaEnable') then
		displayText = displayText..tip_colors.season_casual..Translate('player_stats_campaign_casual')..': '..season_casual..'\n'
	end

	displayText = displayText..
						tip_colors.pre_season_normal..Translate('player_stats_pre_campaign_normal')..': '..pre_season_normal..'\n'

	if GetCvarBool('cl_GarenaEnable') then
		displayText = displayText..tip_colors.pre_season_casual..Translate('player_stats_pre_campaign_casual')..': '..pre_season_casual..'\n'
	end

	displayText = displayText..
						tip_colors.normal..Translate('player_stats_ranked_normal')..': ' ..normal..'\n'..
						tip_colors.casual..Translate('player_stats_ranked_casual')..': '..casual..'\n'..
						tip_colors.midwars..Translate('player_stats_midwars')..': '..midwars..'\n'

	widget:SetText(displayText)
end

local function SwitchToSubAccount()
	if _subAccountSwitching and NotEmpty(_subAccountSwitching) then
		interface:UICmd("SwitchSubAccount('".._subAccountSwitching.."');")
		UIManager.GetInterface('main'):HoNMainF('SwitchedAccount')
		
		_subAccountSwitching = nil
	end
end

local function SetOverviewMasteryInfo(heroMastery)
	local container = GetWidget('playerstats_overview')
	if container == nil then return end
	
	local masteryScore = 0
	for _,v in ipairs(heroMastery) do
		local level = tonumber(v.level) or 0
		masteryScore = masteryScore + level
	end
	container:GetWidget('playerstats_overview_mastery_score'):SetText(tostring(masteryScore))
	
	for i=1, 6 do
		if heroMastery[i] ~= nil then
			container:GetWidget('playerstats_mastery_level_'..i):SetVisible(true)
			container:GetWidget('playerstats_mastery_level_'..i..'_icon'):SetTexture(heroMastery[i].heroIcon)
			container:GetWidget('playerstats_mastery_level_'..i..'_name'):SetText(heroMastery[i].heroName)
			container:GetWidget('playerstats_mastery_level_'..i..'_level'):SetText(Translate('newui_playerstats_level', 'level', heroMastery[i].level))
			container:GetWidget('playerstats_mastery_level_'..i..'_border'):SetTexture('/ui/fe2/newui/res/homepage/border_'..GetMasterTypeByLevel(tonumber(heroMastery[i].level))..'.png')
		else
			container:GetWidget('playerstats_mastery_level_'..i):SetVisible(false)
		end
	end
end

local function SetOverviewMVPAwardsInfo()
	local overview_mvpawards = GetWidget('playerstats_overview_mvpawards')
	if overview_mvpawards == nil then return end
	
	local sort_tbl = {}
	for key, value in pairs(_mvpAwards) do
		table.insert(sort_tbl, {key, value[1], value[2]})
		--println('sort_tbl.insert('..key..'|'..tostring(value[1])..'|'..tostring(value[2]))
	end
	
	table.sort(sort_tbl, function(a, b) 
				if (a[2] > b[2]) then
					return true
				elseif (a[2] == b[2]) then
					return (a[3] > b[3])
				else
					return false
				end
			end)
	--println(sort_tbl[1][1]..'|'..sort_tbl[2][1]..'|'..sort_tbl[3][1]..'|'..sort_tbl[4][1]..'|'..sort_tbl[5][1])
	
	--println('^gSetOverviewMVPAwardsInfo - Award Number '..tostring(#sort_tbl))
	
	local padding = 41
	local block = 139
	local offset = -472 + block / 2

	local icon_path = '/ui/fe2/newui/res/playerstats/mvpawards/awd_mvp_big.png'
	local score_icon_path = '/ui/fe2/newui/res/playerstats/mvpawards/awd_mvp_small.png'
	local rendermode = 'normal'
	--if _mvpScore <= 0 then
	--	rendermode = 'greyscale'
	--end
	overview_mvpawards:Instantiate('playerstats_mvpawards_template', "valign", "bottom",
						'x', tostring(offset + padding)..'i', 'y', '-5%',
						'width', tostring(block)..'i', 'height', '189i', 'awd_bg', "$invis",
						'awd_code', 'overview0', 'font', 'dyn_10', 'awd_name', 'award_mvp',
						'awd_icon', icon_path, 'awd_score_icon', score_icon_path, 'rendermode', rendermode,
						'score_icon_offset', '13%', 'score_icon_height', '140%', 'score_icon_width', '140@',
						'awd_score_sum', tostring(_mvpScore))
	offset = offset + padding + block

	local count = #sort_tbl > 4 and 4 or #sort_tbl
	for idx = 1, count do
		local code = sort_tbl[idx][1]
		--println('^r'..code)
		overview_mvpawards:Instantiate('playerstats_mvpawards_template', "valign", "bottom",
							'x', tostring(offset + padding)..'i', 'y', '-5%',
							'width', tostring(block)..'i', 'height', '189i', 'awd_bg', "$invis",
							'awd_code', 'overview'..tostring(idx), 'font', 'dyn_10', 'awd_name', 'award_'..code,
							'awd_icon', '/ui/fe2/newui/res/playerstats/mvpawards/awd_'..code..'_big.png',
							'awd_score_icon', '/ui/fe2/newui/res/playerstats/mvpawards/awd_'..code..'_small.png', 
							'score_icon_offset', '13%', 'score_icon_height', '140%', 'score_icon_width', '140@',
							'awd_score_sum', tostring(sort_tbl[idx][2]))
		offset = offset + padding + block
	end
end

local function SetOverviewSeasonInfo(seasonInfo, isNormal)
	if not seasonInfo then
		println('^r[ERROR] SetOverviewSeasonInfo called with nil seasonInfo!')
	end
	local container = GetWidget('playerstats_overview')
	if container == nil or seasonInfo == nil then
		println('^rError: invalid invoking SetOverviewSeasonInfo()!')
		return 
	end
	
	-- season name
	if NotEmpty(seasonInfo.season_id) then
		local season_name = ''
		local sid = tonumber(seasonInfo.season_id) or 0
		if isNormal then
			if sid >= 1000 then
				season_name = Translate('player_stats_preseason_name_normal')
			else
				season_name = Translate('player_stats_season_name_normal', 'season_id', seasonInfo.season_id)
			end
		else
			if sid >= 1000 then
				season_name = Translate('player_stats_preseason_name_casual')
			else
				season_name = Translate('player_stats_season_name_casual', 'season_id', seasonInfo.season_id)
			end
		end
		container:GetWidget('playerstats_overview_season_name'):SetText(season_name)
	end
	
	-- rank icons, the rank_level allocation has been changed after season 6
	local current_level = tonumber(seasonInfo.current_level) or 0
	if NotEmpty(seasonInfo.current_level) and current_level > 0 then
		--println('seasonInfo.level_percent = '..seasonInfo.level_percent)
		local percentlevel = tonumber(seasonInfo.level_percent) or 0
		percentlevel = percentlevel > 0 and (percentlevel / 100) or 0
		container:GetWidget('playerstats_overview_meter_to_next_rank'):SetValue(tostring(percentlevel))

		local sid = tonumber(seasonInfo.season_id) or 0
		local current_ranking = tonumber(seasonInfo.current_ranking) or 0

		if sid > 6 and sid < 1000 then
			container:GetWidget('playerstats_overview_current_rank_icon'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevelAfterS6(current_level))
			local safeLevel = (seasonInfo.current_level and seasonInfo.current_level ~= '') and seasonInfo.current_level or '0'
			
			if IsMaxRankLevelAfterS6(current_level) and NotEmpty(seasonInfo.current_ranking) and current_ranking > 0 then
				container:GetWidget('playerstats_overview_current_rank'):SetText(Translate('player_compaign_level_name_S7_'..current_level)..' '..seasonInfo.current_ranking)
			else
				container:GetWidget('playerstats_overview_current_rank'):SetText(Translate('player_compaign_level_name_S7_'..current_level))
			end
		else
			container:GetWidget('playerstats_overview_current_rank_icon'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevel(current_level))
			if IsMaxRankLevel(current_level) and NotEmpty(seasonInfo.current_ranking) and tonumber(seasonInfo.current_ranking) > 0 then
				container:GetWidget('playerstats_overview_current_rank'):SetText(Translate('player_compaign_level_name_'..current_level)..' '..seasonInfo.current_ranking)
			else
				container:GetWidget('playerstats_overview_current_rank'):SetText(Translate('player_compaign_level_name_'..current_level))
			end
		end
		container:GetWidget('playerstats_overview_current_rank_icon'):SetRenderMode('normal')
	else
		container:GetWidget('playerstats_overview_meter_to_next_rank'):SetValue('0')
		container:GetWidget('playerstats_overview_current_rank_icon'):SetTexture('/ui/fe2/season/nolevel.tga')
		container:GetWidget('playerstats_overview_current_rank_icon'):SetRenderMode('grayscale')
		container:GetWidget('playerstats_overview_current_rank'):SetText(Translate('player_compaign_level_name_0'))
	end
	
	local highest_level_current = tonumber(seasonInfo.highest_level_current) or 0
	if NotEmpty(seasonInfo.highest_level_current) and highest_level_current > 0 then
		local sid = tonumber(seasonInfo.season_id) or 0
		if sid > 6 and sid < 1000 then
			container:GetWidget('playerstats_overview_highest_rank_icon'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevelAfterS6(highest_level_current))
			container:GetWidget('playerstats_overview_highest_rank'):SetText(Translate('player_compaign_level_name_S7_'..highest_level_current))
		else
			container:GetWidget('playerstats_overview_highest_rank_icon'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevel(highest_level_current))
			container:GetWidget('playerstats_overview_highest_rank'):SetText(Translate('player_compaign_level_name_'..highest_level_current))
		end
		container:GetWidget('playerstats_overview_highest_rank_icon'):SetRenderMode('normal')
	else
		container:GetWidget('playerstats_overview_highest_rank_icon'):SetTexture('/ui/fe2/season/nolevel.tga')
		container:GetWidget('playerstats_overview_highest_rank_icon'):SetRenderMode('grayscale')
		container:GetWidget('playerstats_overview_highest_rank'):SetText(Translate('player_compaign_level_name_0'))
	end
	
	-- CoN reward
	local conrwd_percentage = 0
	--seasonInfo.con_reward = '0,1,3,2,11,0.66'
	if NotEmpty(seasonInfo.con_reward) then
		local _, _, _, currlevel, _, _, _, _, percentage = string.find(seasonInfo.con_reward, "(.+),(.+),(.+),(.+),(.+),(.+),(.+)")
		println('seasonInfo.con_reward = '..seasonInfo.con_reward)
		println('currlevel = '..currlevel)
		println('percentage = '..percentage)
		conrwd_percentage = tonumber(percentage)
		container:GetWidget('playerstats_overview_con_reward_icon'):SetTexture('/ui/fe2/newui/res/playerstats/conrewards/chest'..currlevel..'.png')
	else
		container:GetWidget('playerstats_overview_con_reward_icon'):SetTexture('/ui/fe2/newui/res/playerstats/conrewards/chest0.png')
	end
	
	if conrwd_percentage > 0 then
		container:GetWidget('playerstats_overview_con_reward_progress'):SetWidth(tostring(conrwd_percentage*100)..'%')
		container:GetWidget('playerstats_overview_con_reward_percent'):SetText(tostring(conrwd_percentage*100)..'%')
		container:GetWidget('playerstats_overview_con_reward_progress_container'):SetVisible(1)
	else
		container:GetWidget('playerstats_overview_con_reward_progress'):SetWidth('0%')
		container:GetWidget('playerstats_overview_con_reward_percent'):SetText('0%')
		container:GetWidget('playerstats_overview_con_reward_progress_container'):SetVisible(0)
	end
	println('STEP: 1')

	-- summary
	local wins = tonumber(seasonInfo.wins) or 0
	local losses = tonumber(seasonInfo.losses) or 0
	local percentage = 0
	if wins and losses then
		percentage = wins > 0 and math.floor(wins / (wins + losses) * 100) or 0
	end
	println('STEP: 2')
	if GetCvarBool('cl_GarenaEnable') then
		container:GetWidget('playerstats_overview_summary_garena'):SetVisible(1)
		container:GetWidget('playerstats_overview_summary'):SetVisible(0)
		local rankInfo = GetRankedPlayInfo(isNormal and 0 or 1)
		if (rankInfo and rankInfo.level >= 0) then
			GetWidget('playerstats_overview_rankwinstreaks_garena'):SetText(tostring(rankInfo.winstreaks))
		end
		container:GetWidget('playerstats_overview_rankwins_garena'):SetText(tostring(seasonInfo.wins))
		container:GetWidget('playerstats_overview_ranklosses_garena'):SetText(tostring(seasonInfo.losses))
		container:GetWidget('playerstats_overview_rankwinpercent_garena'):SetText(tostring(percentage)..'%')
	else
		container:GetWidget('playerstats_overview_summary_garena'):SetVisible(0)
		container:GetWidget('playerstats_overview_summary'):SetVisible(1)
		container:GetWidget('playerstats_overview_rankwins'):SetText(tostring(seasonInfo.wins))
		container:GetWidget('playerstats_overview_ranklosses'):SetText(tostring(seasonInfo.losses))
		container:GetWidget('playerstats_overview_rankwinpercent'):SetText(tostring(percentage)..'%')
	end
	println('STEP: 3')
	
	-- favorite heroes
	for i = 1, 5 do
		local index = tostring(i)
		println('STEP: 4.1 ' .. index)
		if NotEmpty(seasonInfo['favHero'..index]) then
			container:GetWidget('playerstats_hero_fav_overview_'..index..'_icon'):SetTexture(GetHeroIconPath(seasonInfo['favHero'..index]))
		else
			container:GetWidget('playerstats_hero_fav_overview_'..index..'_icon'):SetTexture('/ui/common/ability_coverup.tga')
		end
		println('STEP: 4.2 ' .. index)

		local time = tonumber(seasonInfo['favHero'..index..'Time']) or 0
		if time > 0 then 
			local percent = math.floor(time + 0.5)
			container:GetWidget('playerstats_hero_fav_overview_'..index..'_percent'):SetText(tostring(percent)..'%')
		else
			container:GetWidget('playerstats_hero_fav_overview_'..index..'_percent'):SetText('0%')
		end
		println('STEP: 4.3 ' .. index)

		if NotEmpty(seasonInfo['favHero'..index..'_2']) then 
			local displayname = GetEntityDisplayName(seasonInfo['favHero'..index..'_2']) or seasonInfo['favHero'..index..'_2']
			local masteryType = GetHeroMasterType(displayname) or 'iron'
			container:GetWidget('playerstats_hero_fav_overview_'..index..'_border'):SetTexture('/ui/fe2/newui/res/homepage/border_'..tostring(masteryType)..'.png')
			container:GetWidget('playerstats_hero_fav_overview_'..index..'_name'):SetText(displayname)
		else
			container:GetWidget('playerstats_hero_fav_overview_'..index..'_name'):SetText('---')
			container:GetWidget('playerstats_hero_fav_overview_'..index..'_border'):SetTexture('/ui/fe2/newui/res/homepage/border_iron.png')
		end
		println('STEP: 4.4 ' .. index)
	end
	println('STEP: 5 DONE')
end


local function SetMVPAwardsTabInfo()
	local container = GetWidget('playerstats_mvpawards')
	if container == nil then return end
	
	local codes = {'mann', 'mqk', 'lgks', 'msd', 'mkill', 'masst', 'ledth', 'mbdmg', --[['mwk',--]] 'mhdd', 'hcs'}
	local awd_icon_path = ''
	local awd_score_icon_path = ''
	
	local awd_render_mode = 'normal' --[[dummy value]]--
	if _mvpScore <= 0 then
		awd_render_mode = 'grayscale'
	end
	container:GetWidget('awd_icon_mvp'):SetRenderMode(awd_render_mode)
	container:GetWidget('awd_score_icon_mvp'):SetRenderMode(awd_render_mode)
	container:GetWidget('awd_score_sum_mvp'):SetText('x '..tostring(_mvpScore))
	
	for _, code in ipairs(codes) do
		--println('Set info for award '..code)
		if _mvpAwards[code][1] > 0 then
			awd_render_mode = 'normal' --[[dummy value]]--
		else
			awd_render_mode = 'grayscale'
		end
		container:GetWidget('awd_icon_'..code):SetRenderMode(awd_render_mode)
		container:GetWidget('awd_score_icon_'..code):SetRenderMode(awd_render_mode)
		container:GetWidget('awd_score_sum_'..code):SetText('x '..tostring(_mvpAwards[code][1]))
	end
end

local function SetPlayerStatsStatsFavHeroInfo(stats)
	local container = GetWidget('playerstats_stats')
	if container == nil or stats == nil then
		println('^rError: invalid invoking SetPlayerStatsStatsFavHeroInfo()!')
		return 
	end
	
	for i = 1, 5 do
		local index = tostring(i)
		if NotEmpty(stats['favHero'..index]) then
			container:GetWidget('playerstats_hero_fav_stats_'..index..'_icon'):SetTexture(GetHeroIconPath(stats['favHero'..index]))
		else
			container:GetWidget('playerstats_hero_fav_stats_'..index..'_icon'):SetTexture('/ui/common/ability_coverup.tga')
		end

		if NotEmpty(stats['favHero'..index..'Time']) and tonumber(stats['favHero'..index..'Time']) ~= nil then 
			local percent = math.floor(tonumber(stats['favHero'..index..'Time']) + 0.5)
			container:GetWidget('playerstats_hero_fav_stats_'..index..'_percent'):SetText(tostring(percent)..'%')
		else
			container:GetWidget('playerstats_hero_fav_stats_'..index..'_percent'):SetText('0%')
		end

		if NotEmpty(stats['favHero'..index..'_2']) then 
			local displayname = GetEntityDisplayName(stats['favHero'..index..'_2'])
			local masteryType = GetHeroMasterType(displayname)
			container:GetWidget('playerstats_hero_fav_stats_'..index..'_border'):SetTexture('/ui/fe2/newui/res/homepage/border_'..masteryType..'.png')
			container:GetWidget('playerstats_hero_fav_stats_'..index..'_name'):SetText(displayname)
		else
			container:GetWidget('playerstats_hero_fav_stats_'..index..'_name'):SetText('---')
			container:GetWidget('playerstats_hero_fav_stats_'..index..'_border'):SetTexture('/ui/fe2/newui/res/homepage/border_iron.png')
		end
	end
end

local function SetPlayerStatsStatsInfo(stats, b_public, b_streakstats, b_psr)
	local container = GetWidget('playerstats_stats')
	if container == nil or stats == nil then
		println('^rError: invalid invoking SetPlayerStatsNormalSeasonStatsInfo()!')
		return 
	end
	
	println('enter SetPlayerStatsStatsInfo, '..tostring(b_public)..'/'..tostring(b_streakstats))

	local wins = tonumber(stats.wins) or 0
	local losses = tonumber(stats.losses) or 0
	local winningpercentage = 0
	if wins and losses then
		winningpercentage = wins > 0 and math.floor(wins / (wins + losses) * 100) or 0
	end

	local ttldiscos = tonumber(stats.total_discos) or 0
	local ttlgames = tonumber(stats.total_games_played) or 0
	local leavepercentage = 0
	if ttldiscos and ttlgames then
		leavepercentage = (ttldiscos > 0 and ttlgames > 0) and (ttldiscos / ttlgames * 100) or 0
	end

	if b_public then
		container:GetWidget('playerstats_stats_public_summary'):SetVisible(1)
		container:GetWidget('playerstats_stats_season_summary'):SetVisible(0)
		
		-- summary
		local mmr = tonumber(stats.mmr) or 0
		if b_psr then
			container:GetWidget('stats_name_player_stats_psr'):SetText(Translate('player_stats_psr'))
			container:GetWidget('stats_data_player_stats_psr'):SetText(tostring(math.floor(mmr)))
		else
			container:GetWidget('stats_name_player_stats_psr'):SetText(Translate('player_stats_mmr'))
			container:GetWidget('stats_data_player_stats_psr'):SetText(tostring(math.floor(mmr)))
		end
		container:GetWidget('stats_data_player_stats_wins'):SetText(tonumber(stats.wins) or 0)
		container:GetWidget('stats_data_player_stats_losses'):SetText(tonumber(stats.losses) or 0)
		container:GetWidget('stats_data_player_stats_winning_percentage'):SetText(tostring(winningpercentage)..'%')
		container:GetWidget('stats_data_player_stats_matches'):SetText((tonumber(stats.total_games_played) or 0)..'('..(tonumber(stats.cur_games_played) or 0)..')')
		container:GetWidget('stats_data_player_stats_disconnects'):SetText((tonumber(stats.total_discos) or 0)..'('..(tonumber(stats.cur_discos) or 0)..')')
		container:GetWidget('stats_data_player_stats_leave_percent'):SetText(string.format('%.1f', leavepercentage)..'%');
	else
		container:GetWidget('playerstats_stats_public_summary'):SetVisible(0)
		container:GetWidget('playerstats_stats_season_summary'):SetVisible(1)

		-- rank icons, the rank_level allocation has been changed after season 6
		local current_level = tonumber(stats.current_level) or 0
		if NotEmpty(stats.current_level) and current_level > 0 then
			local percentlevel = tonumber(stats.level_percent) or 0
			percentlevel = percentlevel > 0 and (percentlevel / 100) or 0
			container:GetWidget('playerstats_stats_meter_to_next_rank'):SetValue(tostring(percentlevel))

			local sid = tonumber(stats.season_id) or 0
			local current_ranking = tonumber(stats.current_ranking) or 0

			if sid > 6 and sid < 1000 then
				container:GetWidget('playerstats_stats_current_rank_icon'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevelAfterS6(current_level))
				if IsMaxRankLevelAfterS6(current_level) and NotEmpty(stats.current_ranking) and current_ranking > 0 then
					container:GetWidget('playerstats_stats_current_rank'):SetText(Translate('player_compaign_level_name_S7_'..stats.current_level)..' '..stats.current_ranking)
				else
					container:GetWidget('playerstats_stats_current_rank'):SetText(Translate('player_compaign_level_name_S7_'..stats.current_level))
				end
			else
				container:GetWidget('playerstats_stats_current_rank_icon'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevel(current_level))
				if IsMaxRankLevel(current_level) and NotEmpty(stats.current_ranking) and current_ranking > 0 then
					container:GetWidget('playerstats_stats_current_rank'):SetText(Translate('player_compaign_level_name_'..stats.current_level)..' '..stats.current_ranking)
				else
					container:GetWidget('playerstats_stats_current_rank'):SetText(Translate('player_compaign_level_name_'..stats.current_level))
				end
			end
			container:GetWidget('playerstats_stats_current_rank_icon'):SetRenderMode('normal')
		else
			container:GetWidget('playerstats_stats_meter_to_next_rank'):SetValue('0')
			container:GetWidget('playerstats_stats_current_rank_icon'):SetTexture('/ui/fe2/season/nolevel.tga')
			container:GetWidget('playerstats_stats_current_rank_icon'):SetRenderMode('grayscale')
			container:GetWidget('playerstats_stats_current_rank'):SetText(Translate('player_compaign_level_name_0'))
		end
		
		local highest_level_current = tonumber(stats.highest_level_current) or 0
		if NotEmpty(stats.highest_level_current) and highest_level_current > 0 then
			local sid = tonumber(stats.season_id) or 0
			if sid > 6 and sid < 1000 then
				container:GetWidget('playerstats_stats_highest_rank_icon'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevelAfterS6(highest_level_current))
			else
				container:GetWidget('playerstats_stats_highest_rank_icon'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevel(highest_level_current))
			end
			container:GetWidget('playerstats_stats_highest_rank_icon'):SetRenderMode('normal')
		else
			container:GetWidget('playerstats_stats_highest_rank_icon'):SetTexture('/ui/fe2/season/nolevel.tga')
			container:GetWidget('playerstats_stats_highest_rank_icon'):SetRenderMode('grayscale')
		end
		
		-- summary
		container:GetWidget('stats_data_season_player_stats_wins'):SetText(tonumber(stats.wins) or 0)
		container:GetWidget('stats_data_season_player_stats_losses'):SetText(tonumber(stats.losses) or 0)
		container:GetWidget('stats_data_season_player_stats_winning_percentage'):SetText(tostring(winningpercentage)..'%')
		container:GetWidget('stats_data_season_player_stats_matches'):SetText((tonumber(stats.total_games_played) or 0)..'('..(tonumber(stats.cur_games_played) or 0)..')')
		container:GetWidget('stats_data_season_player_stats_disconnects'):SetText((tonumber(stats.total_discos) or 0)..'('..(tonumber(stats.cur_discos) or 0)..')')
		container:GetWidget('stats_data_season_player_stats_leave_percent'):SetText(string.format('%.1f', leavepercentage)..'%');
	end
	
	-- most played heroes
	SetPlayerStatsStatsFavHeroInfo(stats)
	
	-- lifetime statistics
	println('^y[DEBUG] SetStatisticsTabInfo: stats.herokills='..tostring(stats.herokills))
	println('^y[DEBUG] SetStatisticsTabInfo: stats.deaths='..tostring(stats.deaths))
	println('^y[DEBUG] SetStatisticsTabInfo: stats.heroassists='..tostring(stats.heroassists))
	println('^y[DEBUG] SetStatisticsTabInfo: _publicStats[acc_herokills]='..tostring(_publicStats['acc_herokills']))
	
	container:GetWidget('stats_data_player_stats_kills'):SetText(NotEmpty(stats.herokills) and stats.herokills or '-')
	container:GetWidget('stats_data_player_stats_deaths'):SetText(NotEmpty(stats.deaths) and stats.deaths or '-')
	container:GetWidget('stats_data_player_stats_assists'):SetText(NotEmpty(stats.heroassists) and stats.heroassists or '-')
	
	local kills = tonumber(stats.herokills) or 0
	local deaths = tonumber(stats.deaths) or 0
	local assists = tonumber(stats.heroassists) or 0
	local ratio = '0:0'
	if kills and deaths then
		if kills > 0 and deaths > 0 then
			ratio = string.format('%.2f', kills/deaths)..':1'
		elseif kills > 0 and deaths == 0 then
			ratio = stats.herokills..':0'
		elseif kills == 0 and deaths > 0 then
			ratio = '0:'..stats.deaths
		end
	end
	container:GetWidget('stats_data_player_stats_kd_ratio'):SetText(ratio)
	
	ratio = '0:0'
	if kills and assists and deaths then
		local kna = kills + assists
		if kna > 0 and deaths > 0 then
			ratio = string.format('%.2f', kna/deaths)..':1'
		elseif kna > 0 and deaths == 0 then
			ratio = tostring(kna)..':0'
		elseif kna == 0 and deaths > 0 then
			ratio = '0:'..stats.deaths
		end
	end
	container:GetWidget('stats_data_player_stats_kad_ratio'):SetText(ratio)
	
	if NotEmpty(stats.quest_stats) then
		local _, _, quest_completed, quest_reset, quest_total = string.find(stats.quest_stats, "(.+),(.+),(.+)")	
		container:GetWidget('stats_data_player_stats_total_quests'):SetText(tostring(quest_completed))
	else
		container:GetWidget('stats_data_player_stats_total_quests'):SetText('-')
	end
	
	-- average game statistics
	local strgamelen = '00:00'
	local gamelen = tonumber(stats.avgGameLength) or 0
	if gamelen > 0 then
		strgamelen = tostring(container:UICmd('FtoT('..tostring(gamelen * 1000)..', 1, 0, \'-\')'))
	end
	container:GetWidget('stats_data_player_avg_game_length'):SetText(strgamelen)
	
	container:GetWidget('stats_data_player_avg_kda'):SetText(NotEmpty(stats.k_d_a) and stats.k_d_a or '-')
	container:GetWidget('stats_data_player_avg_creep_kills'):SetText(NotEmpty(stats.avgCreepKills) and string.format('%.1f', (tonumber(stats.avgCreepKills) or 0)) or '-')
	container:GetWidget('stats_data_player_avg_creep_denies'):SetText(NotEmpty(stats.avgDenies) and string.format('%.1f', (tonumber(stats.avgDenies) or 0)) or '-')
	
	local exp = tonumber(stats.exp) or 0
	local gamesplayed = tonumber(stats.cur_games_played) or 0
	container:GetWidget('stats_data_player_stats_experience_earned'):SetText(gamesplayed > 0 and FtoA(exp / gamesplayed, 1, 3, ',') or '0.0')
	
	container:GetWidget('stats_data_player_stats_experience_per_minute'):SetText(NotEmpty(stats.avgXP_min) and string.format('%.1f', (tonumber(stats.avgXP_min) or 0)) or '-')
	container:GetWidget('stats_data_player_stats_apm'):SetText(NotEmpty(stats.avgActions_min) and string.format('%.1f', (tonumber(stats.avgActions_min) or 0)) or '-')
	container:GetWidget('stats_data_player_stats_neutral_creeps'):SetText(NotEmpty(stats.avgNeutralKills) and string.format('%.1f', (tonumber(stats.avgNeutralKills) or 0)) or '-')
	
	local gold = tonumber(stats.gold) or 0
	local secs = tonumber(stats.secs) or 0
	container:GetWidget('stats_data_player_stats_gold_per_minute'):SetText(secs > 0 and string.format('%.1f', gold/(secs/60)) or '0.0')
	
	container:GetWidget('stats_data_player_stats_wards'):SetText(NotEmpty(stats.avgWardsUsed) and string.format('%.1f', (tonumber(stats.avgWardsUsed) or 0)) or '-')
	
	-- smackdowns
	container:GetWidget('stats_data_player_stats_smackdowns'):SetText(NotEmpty(stats.smackdown) and stats.smackdown or '0')
	
	-- streak stats
	if b_streakstats then
		container:GetWidget('playerstats_nostreakstats'):SetVisible(0)
		container:GetWidget('stats_data_player_stats_humiliated'):SetText(NotEmpty(stats.humiliation) and stats.humiliation or '0')
		container:GetWidget('stats_data_player_stats_serialkiller'):SetText(NotEmpty(stats.ks3) and stats.ks3 or '0')
		container:GetWidget('stats_data_player_stats_ultimatewarrior'):SetText(NotEmpty(stats.ks4) and stats.ks4 or '0')
		container:GetWidget('stats_data_player_stats_legndary'):SetText(NotEmpty(stats.ks5) and stats.ks5 or '0')
		container:GetWidget('stats_data_player_stats_onslaught'):SetText(NotEmpty(stats.ks6) and stats.ks6 or '0')
		container:GetWidget('stats_data_player_stats_savagesick'):SetText(NotEmpty(stats.ks7) and stats.ks7 or '0')
		container:GetWidget('stats_data_player_stats_dominating'):SetText(NotEmpty(stats.ks8) and stats.ks8 or '0')
		container:GetWidget('stats_data_player_stats_champion'):SetText(NotEmpty(stats.ks9) and stats.ks9 or '0')
		container:GetWidget('stats_data_player_stats_bloodbath'):SetText(NotEmpty(stats.ks10) and stats.ks10 or '0')
		container:GetWidget('stats_data_player_stats_immortal'):SetText(NotEmpty(stats.ks15) and stats.ks15 or '0')
		container:GetWidget('stats_data_player_stats_doubletap'):SetText(NotEmpty(stats.doublekill) and stats.doublekill or '0')
		container:GetWidget('stats_data_player_stats_hattrick'):SetText(NotEmpty(stats.triplekill) and stats.triplekill or '0')
		container:GetWidget('stats_data_player_stats_quadkill'):SetText(NotEmpty(stats.quadkill) and stats.quadkill or '0')
		container:GetWidget('stats_data_player_stats_annihilated'):SetText(NotEmpty(stats.annihilation) and stats.annihilation or '0')
		container:GetWidget('stats_data_player_stats_bloodlust'):SetText(NotEmpty(stats.bloodlust) and stats.bloodlust or '0')
	else
		container:GetWidget('playerstats_nostreakstats'):SetVisible(1)
	end
end

local function SetPlayerStatsMatchList()
	local container = GetWidget('playerstats_match')
	if container == nil then return end
	
	println('enter SetPlayerStatsMatchList.')
	
	if #_matchList <= 0 then
		ResetMatchListUI()
		container:GetWidget('playerstats_match_no_history'):SetVisible(1)
		return
	end
		
	-- fill up the match list
	container:GetWidget('playerstats_match_no_history'):SetVisible(0)
	local root = container:GetWidget('playerstats_match_list_root')
	for i, match in pairs(_matchList) do
		root:Instantiate('playerstats_match_row_template', 'row', tostring(i), 'match_id', match['id'])
		
		local heroname = string.lower(string.sub(match['heroname'], 6, -1))
		root:GetWidget('playerstats_match_row_hero_icon_'..tostring(i)):SetTexture(GetHeroIconPath(heroname))
		
		local displayname = GetEntityDisplayName(match['heroname'])
		root:GetWidget('playerstats_match_row_hero_name_'..tostring(i)):SetText(displayname)
		
		if match['result'] == '1' then
			root:GetWidget('playerstats_match_row_result_'..tostring(i)):SetTexture('/ui/fe2/newui/res/playerstats/match_result_victory.png')
		else
			root:GetWidget('playerstats_match_row_result_'..tostring(i)):SetTexture('/ui/fe2/newui/res/playerstats/match_result_defeat.png')
		end
		
		root:GetWidget('playerstats_match_row_kda_'..tostring(i)):SetText('^g'..match['kills']..'^*/^r'..match['deaths']..'^*/^y'..match['assists'])
		
		local strgamelen = '00:00'
		local gamelen = tonumber(match['duration']) or 0
		if gamelen > 0 then
			strgamelen = tostring(root:UICmd('FtoT('..tostring(gamelen * 1000)..', 1, 0, \'-\')'))
		end
		root:GetWidget('playerstats_match_row_duration_'..tostring(i)):SetText(strgamelen)
		
		root:GetWidget('playerstats_match_row_map_'..tostring(i)):SetText(TranslateOrEmpty('map_'..match['mapname']))
		
		root:GetWidget('playerstats_match_row_time_'..tostring(i)):SetText(match['mdt'])
		
		root:GetWidget('playerstats_match_row_id_'..tostring(i)):SetText(match['id'])
		
	end
	
	local scrollWidget = container:GetWidget('playerstats_match_list_scroll')
	scrollWidget:SetVisible(1)
	local height = root:GetHeight()
	local height2 = container:GetWidget('playerstats_match_list_clip'):GetHeight()

	if height <= height2 then
		scrollWidget:SetMaxValue(1)
	else
		scrollWidget:SetMaxValue(height - height2 + 2)
	end
	scrollWidget:SetValue(1)
	Player_Stats_V2:UpdateMatchList()
end



local function SetStatsRankTypes()
	local container = GetWidget('playerstats_stats_ranktype')
	if container == nil then return end
	
	println('SetStatsRankTypes')
	
	container:ClearItems()
	_statsRankTypeActions = {}
	
	_statsRankTypeActions['0'] = {}
	_statsRankTypeActions['0'].form 	= 'PlayerStatsNormalSeason'
	_statsRankTypeActions['0'].f 		= 'show_stats'
	_statsRankTypeActions['0'].nickname = StripClanTag(_currentNickName)
	_statsRankTypeActions['0'].cookie 	= Client.GetCookie()
	_statsRankTypeActions['0'].table 	= 'campaign'
	container:AddTemplateListItem('newui_combobox_item_template', '0', 'label', IsPreseason() and Translate('player_stats_preseason_normal_tab') or Translate('player_stats_season_normal_tab'), 'color', '#85695e')
	
	if GetCvarBool('cl_GarenaEnable') then
		_statsRankTypeActions['1'] = {}
		_statsRankTypeActions['1'].form 	= 'PlayerStatsCasualSeason'
		_statsRankTypeActions['1'].f 		= 'show_stats'
		_statsRankTypeActions['1'].nickname	= StripClanTag(_currentNickName)
		_statsRankTypeActions['1'].cookie 	= Client.GetCookie()
		_statsRankTypeActions['1'].table 	= 'campaign_casual'
		container:AddTemplateListItem('newui_combobox_item_template', '1', 'label', IsPreseason() and Translate('player_stats_preseason_casual_tab') or Translate('player_stats_season_casual_tab'), 'color', '#85695e')
	end

	if not GetCvarBool('cl_GarenaEnable') then
		_statsRankTypeActions['2'] = {}
		_statsRankTypeActions['2'].form 	= 'PlayerStatsRanked'
		_statsRankTypeActions['2'].f 		= 'show_stats'
		_statsRankTypeActions['2'].nickname = StripClanTag(_currentNickName)
		_statsRankTypeActions['2'].cookie 	= Client.GetCookie()
		_statsRankTypeActions['2'].table 	= 'ranked'
		container:AddTemplateListItem('newui_combobox_item_template', '2', 'label', Translate('player_stats_ranked_tab'), 'color', '#85695e')
		
		_statsRankTypeActions['3'] = {}
		_statsRankTypeActions['3'].form 	= 'PlayerStatsCasual'
		_statsRankTypeActions['3'].f 		= 'show_stats'
		_statsRankTypeActions['3'].nickname = StripClanTag(_currentNickName)
		_statsRankTypeActions['3'].cookie 	= Client.GetCookie()
		_statsRankTypeActions['3'].table 	= 'casual'
		container:AddTemplateListItem('newui_combobox_item_template', '3', 'label', Translate('player_stats_casual_tab'), 'color', '#85695e')
	end
	
	_statsRankTypeActions['4'] = {}
	_statsRankTypeActions['4'].form 	= 'PlayerStatsPublic'
	_statsRankTypeActions['4'].f 		= 'show_stats'
	_statsRankTypeActions['4'].nickname = StripClanTag(_currentNickName)
	_statsRankTypeActions['4'].cookie 	= Client.GetCookie()
	_statsRankTypeActions['4'].table 	= 'player'
	container:AddTemplateListItem('newui_combobox_item_template', '4', 'label', Translate('player_stats_public_tab'), 'color', '#85695e')
	
	if ShouldHideSeasonHistory() then return end
	
	local offset = 4
	for i,v in ipairs(_historicalSeasons) do
		local season = explode(',', v)
		local season_id = season[1] 
		local is_casual = season[2]
		if season_id and is_casual then
			local season_name = ''
			local isCasual = AtoB(is_casual)
			if tonumber(season_id) == 0 then
				if isCasual then
					season_name = Translate('player_stats_season_legacy_casual')
				else
					season_name = Translate('player_stats_season_legacy_normal')
				end
			elseif tonumber(season_id) >= 1000 then
				if isCasual then
					season_name = Translate('player_stats_preseason_name_casual')
				else
					season_name = Translate('player_stats_preseason_name_normal')
				end
			else
				if isCasual then
					season_name = Translate('player_stats_season_name_casual', 'season_id', season_id)
				else
					season_name = Translate('player_stats_season_name_normal', 'season_id', season_id)
				end
			end
			if tonumber(season_id) == 0 or not isCasual or GetCvarBool('cl_GarenaEnable') then
				local value = tostring(i + offset)
				_statsRankTypeActions[value] = {}
				_statsRankTypeActions[value].form 		= isCasual and 'PlayerStatsHistoryCasual' or 'PlayerStatsHistoryNormal'
				_statsRankTypeActions[value].f 			= 'show_stats'
				_statsRankTypeActions[value].nickname 	= StripClanTag(_currentNickName)
				_statsRankTypeActions[value].cookie 	= Client.GetCookie()
				_statsRankTypeActions[value].table 		= 'campaign_history'
				_statsRankTypeActions[value].seasonid 	= season_id
				_statsRankTypeActions[value].iscasual 	= is_casual
				container:AddTemplateListItem('newui_combobox_item_template', value, 'label', season_name, 'color', '#85695e')
			end
		end
	end
end

----------------------------------- Initializations --------------------------------------

function Player_Stats_V2:Init()
	local widget = GetWidget('playerstats_helper')
	widget:RegisterWatch('PlayerStatsMasteryResult', function(_, ...) Player_Stats_V2:OnPlayerStatsMasteryResult(...) end)
	widget:RegisterWatch('ClaimMasteryReward', function(_, ...)
		-- Fetch refreshed master info to update UI correctly after a claim
		SubmitForm('PlayerStatsMasteryResult', 'f', 'show_simple_stats', 'nickname', _publicStats.nickname, 'cookie', GetCookie(), 'table', 'mastery')
	end)
	widget:RegisterWatch('PlayerStatsMasteryStatus', function(_, ...) Player_Stats_V2:OnFormStatusTrigger(...) end)
	
	widget:RegisterWatch('PlayerStatsNormalSeasonResult', function(_, ...) Player_Stats_V2:OnPlayerStatsNormalSeasonResult(...) end)
	widget:RegisterWatch('PlayerStatsNormalSeasonStatus', function(_, ...) Player_Stats_V2:OnFormStatusTrigger(...) end)
	
	widget:RegisterWatch('PlayerStatsCasualSeasonResult', function(_, ...) Player_Stats_V2:OnPlayerStatsCasualSeasonResult(...) end)
	widget:RegisterWatch('PlayerStatsCasualSeasonStatus', function(_, ...) Player_Stats_V2:OnFormStatusTrigger(...) end)
	
	widget:RegisterWatch('PlayerStatsPublicResult', function(_, ...) Player_Stats_V2:OnPlayerStatsPublicResult(...) end)
	widget:RegisterWatch('PlayerStatsPublicStatus', function(_, ...) Player_Stats_V2:OnFormStatusTrigger(...) end)
	
	widget:RegisterWatch('PlayerStatsRankedResult', function(_, ...) Player_Stats_V2:OnPlayerStatsNormalResult(...) end)
	widget:RegisterWatch('PlayerStatsRankedStatus', function(_, ...) Player_Stats_V2:OnFormStatusTrigger(...) end)
	
	widget:RegisterWatch('PlayerStatsCasualResult', function(_, ...) Player_Stats_V2:OnPlayerStatsCasualResult(...) end)
	widget:RegisterWatch('PlayerStatsCasualStatus', function(_, ...) Player_Stats_V2:OnFormStatusTrigger(...) end)

	widget:RegisterWatch('PlayerStatsHistoryNormalResult', function(_, ...) Player_Stats_V2:OnPlayerStatsHistoryNormalResult(...) end)
	widget:RegisterWatch('PlayerStatsHistoryNormalStatus', function(_, ...) Player_Stats_V2:OnFormStatusTrigger(...) end)
	
	widget:RegisterWatch('PlayerStatsHistoryCasualResult', function(_, ...) Player_Stats_V2:OnPlayerStatsHistoryCasualResult(...) end)
	widget:RegisterWatch('PlayerStatsHistoryCasualStatus', function(_, ...) Player_Stats_V2:OnFormStatusTrigger(...) end)
	
	widget:RegisterWatch('RankedMatchWinInfo', function(object, wins, losses, smr) Player_Stats_V2:OnRankedMatchWinInfo(wins, losses, smr) end)
	-- [Logan] Disabled StatsStatusStart: The C++ engine emits this indiscriminately for background fetches like get_account_all_hero_stats without providing a Finish hook, stranding the Wait Mask counter. Our Lua SubmitForms manually invoke ShowWaitingMask() making this completely obsolete.
	-- widget:RegisterWatch('StatsStatusStart', function() Player_Stats_V2:ShowWaitingMask('main_stats_retrieving_stats') end)
	
	widget:RegisterWatch('MasteryHeroInfo', function(_, ...) Player_Stats_V2:OnMasteryHeroInfo(...) end)
	widget:RegisterWatch('MasteryClearInfo', function(_, ...) Player_Stats_V2:OnMasteryClearInfo(...) end)
	widget:RegisterWatch('MasteryFinishInfo', function(_, ...) Player_Stats_V2:OnMasteryFinishInfo(...) end)
	widget:RegisterWatch('UpdateMasteryInfo', function(_, ...) Player_Stats_V2:OnUpdateMasteryInfo(...) end)
	widget:RegisterWatch('EntityDefinitionsLoaded', function(_, ...) Player_Stats_V2:OnEntityDefinitionsLoaded(...) end)

	widget:RegisterWatch('PlayerStatsMVPAwardsResult', function(_, ...) Player_Stats_V2:OnPlayerStatsMVPAwardsResult(...) end)
	widget:RegisterWatch('PlayerStatsMVPAwardsStatus', function(_, ...) Player_Stats_V2:OnFormStatusTrigger(...) end)
	
	widget:RegisterWatch('PlayerStatsMatchListResult', function(_, ...) Player_Stats_V2:OnPlayerStatsMatchListResult(...) end)
	widget:RegisterWatch('PlayerStatsMatchListStatus', function(_, ...) Player_Stats_V2:OnFormStatusTrigger(...) end)
	
	widget:RegisterWatch('GetSeasonsResult', function(_, ...) Player_Stats_V2:OnPlayerStatsGetSeasonsResult(...) end)
	widget:RegisterWatch('GetSeasonsStatus', function(_, ...) Player_Stats_V2:OnFormStatusTrigger(...) end)
	
	widget:RegisterWatch('ChatTotalFriends', function(_, ...) Player_Stats_V2:SetFunctionalButtons() end)
	
	widget:RegisterWatch('SubAccount', function(_, ...) Player_Stats_V2:OnSubAccount(...) end)
	widget:RegisterWatch('AccountInfo', function(_, ...) Player_Stats_V2:OnAccountInfo(...) end)
	
	widget:RegisterWatch('MatchInfoSummary', function(_, ...) Player_Stats_V2:OnMatchInfoSummary(...) end)
end

function Player_Stats_V2:InitOverviewRankTypes()
	local container = GetWidget('playerstats_overview_ranktype')
	if container == nil then return end
	
	println('InitOverviewRankTypes')
	
	container:ClearItems()
	_overviewRankTypeActions = {}
	
	_overviewRankTypeActions['0'] = {}
	_overviewRankTypeActions['0'].form 		= 'PlayerStatsNormalSeason'
	_overviewRankTypeActions['0'].f 		= 'show_stats'
	_overviewRankTypeActions['0'].nickname 	= StripClanTag(_currentNickName)
	_overviewRankTypeActions['0'].cookie 	= Client.GetCookie()
	_overviewRankTypeActions['0'].table 	= 'campaign'
	
	container:AddTemplateListItem('newui_combobox_item_template', '0', 'label', IsPreseason() and Translate('player_stats_preseason_normal_tab') or Translate('player_stats_season_normal_tab'), 'color', '#85695e')
	
	if GetCvarBool('cl_GarenaEnable') then
		_overviewRankTypeActions['1'] = {}
		_overviewRankTypeActions['1'].form 		= 'PlayerStatsCasualSeason'
		_overviewRankTypeActions['1'].f 		= 'show_stats'
		_overviewRankTypeActions['1'].nickname 	= StripClanTag(_currentNickName)
		_overviewRankTypeActions['1'].cookie 	= Client.GetCookie()
		_overviewRankTypeActions['1'].table 	= 'campaign_casual'
		
		container:AddTemplateListItem('newui_combobox_item_template', '1', 'label', IsPreseason() and Translate('player_stats_preseason_casual_tab') or Translate('player_stats_season_casual_tab'), 'color', '#85695e')
	end
end

function Player_Stats_V2:InitMatchRankTypes()
	local container = GetWidget('playerstats_match_ranktype')
	if container == nil then return end
	
	println('InitMatchRankTypes')
	
	container:ClearItems()
	_matchRankTypeActions = {}
	
	_matchRankTypeActions['0'] = {}
	_matchRankTypeActions['0'].form 	= 'PlayerStatsMatchList'
	_matchRankTypeActions['0'].f 		= 'match_history_overview'
	_matchRankTypeActions['0'].nickname = StripClanTag(_currentNickName)
	_matchRankTypeActions['0'].cookie 	= Client.GetCookie()
	_matchRankTypeActions['0'].table 	= 'campaign'
	_matchRankTypeActions['0'].num	 	= '100'
	_matchRankTypeActions['0'].curseason = '1'
	container:AddTemplateListItem('newui_combobox_item_template', '0', 'label', IsPreseason() and Translate('player_stats_preseason_normal_tab') or Translate('player_stats_season_normal_tab'), 'color', '#85695e')
	
	if GetCvarBool('cl_GarenaEnable') then
		_matchRankTypeActions['1'] = {}
		_matchRankTypeActions['1'].form 	= 'PlayerStatsMatchList'
		_matchRankTypeActions['1'].f 		= 'match_history_overview'
		_matchRankTypeActions['1'].nickname	= StripClanTag(_currentNickName)
		_matchRankTypeActions['1'].cookie 	= Client.GetCookie()
		_matchRankTypeActions['1'].table 	= 'campaign_casual'
		_matchRankTypeActions['1'].num	 	= '100'
		_matchRankTypeActions['1'].curseason = '1'
		container:AddTemplateListItem('newui_combobox_item_template', '1', 'label', IsPreseason() and Translate('player_stats_preseason_casual_tab') or Translate('player_stats_season_casual_tab'), 'color', '#85695e')

		_matchRankTypeActions['2'] = {}
		_matchRankTypeActions['2'].form 	= 'PlayerStatsMatchList'
		_matchRankTypeActions['2'].f 		= 'match_history_overview'
		_matchRankTypeActions['2'].nickname = StripClanTag(_currentNickName)
		_matchRankTypeActions['2'].cookie 	= Client.GetCookie()
		_matchRankTypeActions['2'].table 	= 'ranked'
		_matchRankTypeActions['2'].num	 	= '100'
		_matchRankTypeActions['2'].curseason = '0'
		container:AddTemplateListItem('newui_combobox_item_template', '2', 'label', Translate('player_stats_ranked_tab'), 'color', '#85695e')
		
		_matchRankTypeActions['3'] = {}
		_matchRankTypeActions['3'].form 	= 'PlayerStatsMatchList'
		_matchRankTypeActions['3'].f 		= 'match_history_overview'
		_matchRankTypeActions['3'].nickname = StripClanTag(_currentNickName)
		_matchRankTypeActions['3'].cookie 	= Client.GetCookie()
		_matchRankTypeActions['3'].table 	= 'casual'
		_matchRankTypeActions['3'].num	 	= '100'
		_matchRankTypeActions['3'].curseason = '0'
		container:AddTemplateListItem('newui_combobox_item_template', '3', 'label', Translate('player_stats_casual_tab'), 'color', '#85695e')
	end
	
	_matchRankTypeActions['4'] = {}
	_matchRankTypeActions['4'].form 	= 'PlayerStatsMatchList'
	_matchRankTypeActions['4'].f 		= 'match_history_overview'
	_matchRankTypeActions['4'].nickname = StripClanTag(_currentNickName)
	_matchRankTypeActions['4'].cookie 	= Client.GetCookie()
	_matchRankTypeActions['4'].table 	= 'player'
	_matchRankTypeActions['4'].num	 	= '100'
	_matchRankTypeActions['4'].curseason = '0'
	container:AddTemplateListItem('newui_combobox_item_template', '4', 'label', Translate('player_stats_public_tab'), 'color', '#85695e')
	
	_matchRankTypeActions['5'] = {}
	_matchRankTypeActions['5'].form 	= 'PlayerStatsMatchList'
	_matchRankTypeActions['5'].f 		= 'match_history_overview'
	_matchRankTypeActions['5'].nickname = StripClanTag(_currentNickName)
	_matchRankTypeActions['5'].cookie 	= Client.GetCookie()
	_matchRankTypeActions['5'].table 	= 'history'
	_matchRankTypeActions['5'].num	 	= '100'
	_matchRankTypeActions['5'].curseason = '0'
	container:AddTemplateListItem('newui_combobox_item_template', '5', 'label', Translate('player_stats_history_tab'), 'color', '#85695e')
	
	_matchRankTypeActions['6'] = {}
	_matchRankTypeActions['6'].form 	= 'PlayerStatsMatchList'
	_matchRankTypeActions['6'].f 		= 'match_history_overview'
	_matchRankTypeActions['6'].nickname = StripClanTag(_currentNickName)
	_matchRankTypeActions['6'].cookie 	= Client.GetCookie()
	_matchRankTypeActions['6'].table 	= 'other'
	_matchRankTypeActions['6'].num	 	= '100'
	_matchRankTypeActions['6'].curseason = '0'
	container:AddTemplateListItem('newui_combobox_item_template', '6', 'label', 'Midwars', 'color', '#85695e')

	_matchRankTypeActions['7'] = {}
	_matchRankTypeActions['7'].form 	= 'PlayerStatsMatchList'
	_matchRankTypeActions['7'].f 		= 'match_history_overview'
	_matchRankTypeActions['7'].nickname = StripClanTag(_currentNickName)
	_matchRankTypeActions['7'].cookie 	= Client.GetCookie()
	_matchRankTypeActions['7'].table 	= 'riftwars'
	_matchRankTypeActions['7'].num	 	= '100'
	_matchRankTypeActions['7'].curseason = '0'
	container:AddTemplateListItem('newui_combobox_item_template', '7', 'label', 'Riftwars', 'color', '#85695e')
end
		
function Player_Stats_V2:InitMVPAwardsUI()
	local tab_mvpawards = GetWidget('playerstats_mvpawards')
	if tab_mvpawards == nil then return end
	
	local padding = 2
	local block_wid = 16
	local pos_y = '25%'
	local width = '190i'
	local height = '258i'

	local uppper_codes = {'mvp', 'mann', 'mqk', 'lgks', 'msd', 'mkill'}
	local lower_codes = {'masst', 'ledth', 'mbdmg', --[['mwk',--]] 'mhdd', 'hcs'}

	for i, code in ipairs(uppper_codes) do
		tab_mvpawards:Instantiate('playerstats_mvpawards_template',
							'x', tostring(-50 + padding + (i-1)*block_wid + block_wid / 2)..'%',
							'y', '-'..pos_y, 'width', width, 'height', height,
							'awd_code', code, 'awd_name', 'award_'..code,
							'awd_icon', '/ui/fe2/newui/res/playerstats/mvpawards/awd_'..code..'_big.png',
							'awd_score_icon', '/ui/fe2/newui/res/playerstats/mvpawards/awd_'..code..'_small.png',
							'rendermode', 'grayscale',
							'awd_score_sum', '-')
	end

	for i, code in ipairs(lower_codes) do
		tab_mvpawards:Instantiate('playerstats_mvpawards_template',
							'x', tostring(-50 + padding + (i-1)*block_wid + block_wid / 2)..'%',
							'y', pos_y, 'width', width, 'height', height,
							'awd_code', code, 'awd_name', 'award_'..code,
							'awd_icon', '/ui/fe2/newui/res/playerstats/mvpawards/awd_'..code..'_big.png',
							'awd_score_icon', '/ui/fe2/newui/res/playerstats/mvpawards/awd_'..code..'_small.png',
							'rendermode', 'grayscale',
							'awd_score_sum', '-')
	end
end

------------------------------------------  Trigger Handler -----------------------------------------------

function Player_Stats_V2:OnFormStatusTrigger(status)
	status = tonumber(status)
	if status == 2 or status == 3 then 
		Player_Stats_V2:HideWaitingMask()
	end
end

function Player_Stats_V2:GetArg(argTable, index)
	-- Try to detect packed arguments (single table/userdata at index 1)
	local firstArg = argTable[1]
	local firstType = type(firstArg)
	
	-- K2 often passes CArrays as userdata or tables
	if (firstType == 'table' or firstType == 'userdata') and argTable[2] == nil then
		-- It's a packed array/table. The data is inside argTable[1].
		-- We assume the payload is now 1-based Dictionary/Table from C#.
		-- So arg[k] corresponds to packed[k].
		
		-- Use pcall to safely access userdata index in case it fails
		local success, val = pcall(function() return firstArg[index] end)
		if success then
			return val
		else
			println('^r[ERROR] GetArg failed to index userdata: '..tostring(val))
			return nil
		end
	else
		return argTable[index]
	end
end

--------------------------------------------------------------------------------
-- PHP Unserialize Helper
--------------------------------------------------------------------------------
local function php_unserialize(s)
	local index = 1
	local len = string.len(s)
	
	local function parse_value()
		local type = string.sub(s, index, index)
		index = index + 2 -- skip type and :
		
		if type == 'i' then -- integer
			local end_pos = string.find(s, ';', index)
			local val = tonumber(string.sub(s, index, end_pos - 1))
			index = end_pos + 1
			return val
		elseif type == 'd' then -- double
			local end_pos = string.find(s, ';', index)
			local val = tonumber(string.sub(s, index, end_pos - 1))
			index = end_pos + 1
			return val
		elseif type == 'b' then -- boolean
			local end_pos = string.find(s, ';', index)
			local val = (string.sub(s, index, end_pos - 1) == '1')
			index = end_pos + 1
			return val
		elseif type == 's' then -- string
			local end_pos = string.find(s, ':', index)
			local str_len = tonumber(string.sub(s, index, end_pos - 1))
			index = end_pos + 2 -- skip : and "
			local val = string.sub(s, index, index + str_len - 1)
			index = index + str_len + 2 -- skip content and ";
			return val
		elseif type == 'a' then -- array
			local end_pos = string.find(s, ':', index)
			local count = tonumber(string.sub(s, index, end_pos - 1))
			index = end_pos + 2 -- skip : and {
			
			local arr = {}
			for i = 1, count do
				local k = parse_value()
				local v = parse_value()
				arr[k] = v
			end
			index = index + 1 -- skip }
			return arr
		elseif type == 'N' then -- null
			return nil
		else
			-- Unknown type or error
			return nil
		end
	end
	
	return parse_value()
end

function Player_Stats_V2:PHP_Unserialize(s)
	if s == nil then 
		println('^r[PHP_Unserialize] Error: Input string is nil.')
		return nil 
	end
	
	if type(s) ~= 'string' then 
		println('^r[PHP_Unserialize] Error: Input is not a string. Type: ' .. type(s))
		return nil 
	end
	
	-- Unserialize
	local status, result = pcall(php_unserialize, s)
	if status then
		return result
	else
		println('^rPHP_Unserialize: Failed to unserialize data. Error: ' .. tostring(result))
		return nil
	end
end

function Player_Stats_V2:OnPlayerStatsMasteryResult(...)
	local arg = { ... }
	arg.n = select('#', ...)
	-- Reconstruct flat Key-Value unrolls if the C++ Engine stripped the userdata wrapper
	if type(arg[1]) == 'string' and arg.n and arg.n > 20 then
		-- Detect if this is a flattened dictionary: key1, val1, key2, val2...
		local isFlatDict = false
		for i = 1, math.min(arg.n, 10), 2 do
			if type(arg[i]) == 'string' and (arg[i] == 'account_id' or arg[i] == 'nickname' or arg[i] == 'error_code') then
				isFlatDict = true
				break
			end
		end

		if isFlatDict then
			local rebuilt = {}
			for i = 1, arg.n, 2 do
				local k = arg[i]
				local v = arg[i+1]
				if k ~= nil then
					rebuilt[k] = v
				end
			end
			arg = rebuilt
			println('^y[PlayerStats] Successfully rebuilt Mastery variadic flattened Dictionary!^*')
		end
	end

	-- Universal Base64 Wrapper for SimpleStatsHandler
	-- Logan (2026-02-12): Fix for Modern Server Response (Table vs Positional)
	if type(arg[1]) == 'table' then
		println('^gOnPlayerStatsMasteryResult: Detected Table! Converting...')
		local params = {}
		for k,v in pairs(arg[1]) do
			if type(k) == 'number' then
				params[k+1] = v
			else
				params[k] = v
			end
		end
		-- Ensure strict nil checks don't break logic
		setmetatable(params, {__index = function(t,k) return nil end})
		arg = params
	end


	
	-- Legacy/Previous check (optional, but good for safety if we revert)
	if type(arg[1]) == 'string' and type(arg[2]) == 'string' then
		local data = Player_Stats_V2:PHP_Unserialize(arg[2])
		if data then arg = data end
	elseif type(arg[1]) == 'string' and (type(arg[2]) == 'table' or type(arg[2]) == 'userdata') then
		arg = arg[2]
	end

	local function sortbylevel(a, b)
		local level_a = tonumber(a.level) or 0
		local level_b = tonumber(b.level) or 0
		return level_a > level_b
	end

	local nickname = arg['nickname'] or Player_Stats_V2:GetArg(arg, 1)
	local masteryInfo = arg['mastery_info'] or Player_Stats_V2:GetArg(arg, 11)
	local rewardsInfo = arg['mastery_rewards'] or Player_Stats_V2:GetArg(arg, 12)
	
	println('^y[DEBUG] OnPlayerStatsMasteryResult: Nick='..tostring(nickname))
	println('^y[DEBUG] OnPlayerStatsMasteryResult: Arg11 type='..type(masteryInfo)..' val='..tostring(masteryInfo))
	-- Conversion Helper for NLua Userdata (Dictionary)
	if type(masteryInfo) == 'userdata' then
		local mt = {}
		for k,v in pairs(masteryInfo) do
			-- NLua Dictionary keys might be 1-based integers or strings
			if type(v) == 'userdata' then
				local inner = {}
				for k2,v2 in pairs(v) do inner[k2] = v2 end
				mt[k] = inner
			else
				mt[k] = v
			end
		end
		masteryInfo = mt
		println('^y[DEBUG] OnPlayerStatsMasteryResult: Converted Userdata to Table. Count='..(#masteryInfo))
	end

	-- Fix 0-based indexing for masteryInfo (common with C# Dictionaries serialization)
	if masteryInfo and masteryInfo[0] ~= nil then
		local fixed = {}
		for k,v in pairs(masteryInfo) do
			if type(k) == 'number' then fixed[k+1] = v else fixed[k] = v end
		end
		masteryInfo = fixed
		println('^gOnPlayerStatsMasteryResult: Fixed 0-based masteryInfo.')
	end
	println('^y[DEBUG] OnPlayerStatsMasteryResult: Arg12 type='..type(rewardsInfo) .. ' val='..tostring(rewardsInfo))
	if type(masteryInfo) == 'table' then
		Echo('^y[DEBUG] masteryInfo table size: ' .. tostring(#masteryInfo))
		for k,v in pairs(masteryInfo) do
			if type(v) == 'table' then
				Echo('^y[DEBUG] masteryInfo['..tostring(k)..'] = ' .. tostring(v.heroname) .. ' | exp: ' .. tostring(v.exp))
			end
			break -- just print the first one
		end
	end

	local heroMastery = GetHeroMasteryUpgradeInfo(masteryInfo)
	Echo('^y[DEBUG] heroMastery returned table size: ' .. tostring(#heroMastery))
	if #heroMastery > 0 then
		for k, v in pairs(heroMastery[1]) do
			Echo('^y[DEBUG] heroMastery[1].' .. tostring(k) .. ' = ' .. tostring(v))
		end
	end
	local rewards = GetMasteryRewardsInfo(rewardsInfo)
	local levelRewards = GetHeroProficiency(masteryInfo)

	table.sort(heroMastery, sortbylevel)

	SetOverviewMasteryInfo(heroMastery)
	println('^y[DEBUG] Called SetOverviewMasteryInfo')
	
	_heroMastery = {}
	bHeroMasteryRetrieved = false

	for i,v in ipairs(heroMastery) do
		local hero = {}
		hero.name = v.heroName
		hero.icon = v.heroIcon
		hero.level = tonumber(v.level) or 0
		hero.type = GetMasterTypeByLevel(hero.level)
		hero.own = true
		
		table.insert(_heroMastery, hero)
	end
	
	bHeroMasteryRetrieved = true
	
	if bNormalSeasonStatsRetrieved then
		println('^gSetting overview season info from mastery result!^*')
		local statsInfo = {}
		if true then	
			statsInfo.account_id					= _seasonStatsNormal['account_id']           
			statsInfo.season_id 					= _seasonStatsNormal['season_id']
			statsInfo.mmr							= _seasonStatsNormal['smr']
			statsInfo.current_level 				= _seasonStatsNormal['current_level']
			statsInfo.level_exp 					= _seasonStatsNormal['level_exp']
			statsInfo.level_percent					= _seasonStatsNormal['level_percent']
			statsInfo.current_ranking				= _seasonStatsNormal['current_ranking']
			statsInfo.highest_level_current			= _seasonStatsNormal['highest_level_current']
			statsInfo.highest_ranking				= _seasonStatsNormal['highest_ranking']
			statsInfo.favHero1						= _seasonStatsNormal['favHero1']
			statsInfo.favHero2						= _seasonStatsNormal['favHero2']
			statsInfo.favHero3						= _seasonStatsNormal['favHero3']
			statsInfo.favHero4						= _seasonStatsNormal['favHero4']
			statsInfo.favHero5						= _seasonStatsNormal['favHero5']        
			statsInfo.favHero1Time					= _seasonStatsNormal['favHero1Time']
			statsInfo.favHero2Time					= _seasonStatsNormal['favHero2Time']
			statsInfo.favHero3Time					= _seasonStatsNormal['favHero3Time']
			statsInfo.favHero4Time					= _seasonStatsNormal['favHero4Time']
			statsInfo.favHero5Time					= _seasonStatsNormal['favHero5Time']
			statsInfo.favHero1_2					= _seasonStatsNormal['favHero1_2']
			statsInfo.favHero2_2					= _seasonStatsNormal['favHero2_2']
			statsInfo.favHero3_2					= _seasonStatsNormal['favHero3_2']
			statsInfo.favHero4_2					= _seasonStatsNormal['favHero4_2']
			statsInfo.favHero5_2					= _seasonStatsNormal['favHero5_2']
			statsInfo.total_games_played			= _seasonStatsNormal['total_games_played']
			statsInfo.cur_games_played     			= _seasonStatsNormal['curr_season_cam_games_played']
			statsInfo.total_discos                  = _seasonStatsNormal['total_discos']
			statsInfo.cur_discos           			= _seasonStatsNormal['curr_season_cam_discos']
			statsInfo.avgCreepKills                 = _seasonStatsNormal['avgCreepKills']
			statsInfo.avgDenies                     = _seasonStatsNormal['avgDenies']
			statsInfo.avgGameLength                 = _seasonStatsNormal['avgGameLength']
			statsInfo.avgXP_min                     = _seasonStatsNormal['avgXP_min']
			statsInfo.avgActions_min                = _seasonStatsNormal['avgActions_min']
			statsInfo.avgNeutralKills               = _seasonStatsNormal['avgNeutralKills']
			statsInfo.avgWardsUsed                  = _seasonStatsNormal['avgWardsUsed']
			statsInfo.k_d_a                         = _seasonStatsNormal['k_d_a']
			statsInfo.wins							= _seasonStatsNormal['cam_wins']
			statsInfo.losses						= _seasonStatsNormal['cam_losses']
			statsInfo.herokills                    	= _seasonStatsNormal['cam_herokills']
			statsInfo.deaths                       	= _seasonStatsNormal['cam_deaths']
			statsInfo.heroassists                  	= _seasonStatsNormal['cam_heroassists']
			statsInfo.exp                          	= _seasonStatsNormal['cam_exp']
			statsInfo.gold                         	= _seasonStatsNormal['cam_gold']
			statsInfo.secs                         	= _seasonStatsNormal['cam_secs']
			statsInfo.smackdown                    	= _seasonStatsNormal['cam_smackdown']
			statsInfo.humiliation                  	= _seasonStatsNormal['cam_humiliation']
			statsInfo.ks3                          	= _seasonStatsNormal['cam_ks3']
			statsInfo.ks4                          	= _seasonStatsNormal['cam_ks4']
			statsInfo.ks5                          	= _seasonStatsNormal['cam_ks5']
			statsInfo.ks6                          	= _seasonStatsNormal['cam_ks6']
			statsInfo.ks7                          	= _seasonStatsNormal['cam_ks7']
			statsInfo.ks8                          	= _seasonStatsNormal['cam_ks8']
			statsInfo.ks9                          	= _seasonStatsNormal['cam_ks9']
			statsInfo.ks10                         	= _seasonStatsNormal['cam_ks10']
			statsInfo.ks15                         	= _seasonStatsNormal['cam_ks15']
			statsInfo.doublekill                   	= _seasonStatsNormal['cam_doublekill']
			statsInfo.triplekill                   	= _seasonStatsNormal['cam_triplekill']
			statsInfo.quadkill                     	= _seasonStatsNormal['cam_quadkill']
			statsInfo.annihilation                 	= _seasonStatsNormal['cam_annihilation']
			statsInfo.bloodlust                    	= _seasonStatsNormal['cam_bloodlust']
			statsInfo.con_reward					= _seasonStatsNormal['con_reward']
		end
		
		SetOverviewSeasonInfo(statsInfo)
	end
	
	Player_Stats_V2:SetMasteryAllHeroes()
	
	Player_Stats_V2:SetMasteryRewardsInfo(heroMastery, rewards, levelRewards)
    
	--------------------------------------------------------------
    if IsMe(nickname) then 
		SetMyMasteryExp(masteryInfo)
		Player_Stats_V2:ShowWaitingMask()
	end
end

function Player_Stats_V2:OnPlayerStatsNormalSeasonResult(...) 
	-- Wrapper for Table/Map vs Positional Args
	local args = { ... } args.n = select('#', ...)
	if not args.n then args.n = #args end
    
    println('^y--- CLEAN HYBRID DUMP ---')
    println('TOTAL ARGS via select: ' .. tostring(select('#', ...)))
    println('TOTAL ARGS unpacked: ' .. tostring(args.n))
    if args.n > 0 then
        for i = 1, args.n do
            if tostring(args[i]) == '10862' or tostring(args[i]) == '493' or i > 120 and i < 130 then
                println('Lua args[' .. i .. '] = "' .. tostring(args[i]) .. '" (C++ Index ' .. (i-1) .. ')')
            end
        end
    end
    println('^y------------------')
    
	local data = nil

	if type(args[1]) == 'table' or type(args[1]) == 'userdata' then
		data = args[1]
	elseif type(args[1]) == 'string' and string.len(args[1]) > 5 then
		-- Base64 Support & PHP Serialization
		data = Player_Stats_V2:PHP_Unserialize(args[1])
	end

	-- Conversion Helper for NLua Userdata (Dictionary) - REMOVED (Use Safe pcall access instead)
	-- if type(data) == 'userdata' then ... end

	local function GetVal(idx, key)
		if data then
			if type(data) == 'userdata' then
				-- Safe access for C# Dictionaries via NLua (prevents KeyNotFoundException)
				if key then
					local status, res = pcall(function() return data[key] end)
					if status and res ~= nil then return res end
				end
				if idx then
					local status, res = pcall(function() return data[idx] end)
					if status and res ~= nil then return res end
					
					local status2, res2 = pcall(function() return data[tostring(idx)] end)
					if status2 and res2 ~= nil then return res2 end
				end
			else
				if key and data[key] then return data[key] end
				-- Modern Hybrid payload boxes keys as 0-based integers natively matching C++ expectations.
				-- When unwrapped as a single Table into Lua, we must adjust the 1-based Lua index back to 0-based.
				if idx then
					local phpIndex = idx - 1
					if data[phpIndex] then return data[phpIndex] end
					if data[tostring(phpIndex)] then return data[tostring(phpIndex)] end
				end
			end
		elseif type(args) == 'table' then
			if args[idx] then return args[idx] end
		end
		return nil
	end

	-- Modern Server Support: Prioritize named keys if present
	local modernNickname = GetVal(nil, 'nickname')
	if modernNickname and modernNickname ~= '' then
		println('^gOnPlayerStatsNormalSeasonResult: Using Named Keys from Server Response.')
		_seasonHistoryStatsNormal = {} -- Reset or ensure clean state
		_seasonHistoryStatsNormal['nickname'] 						= modernNickname
		_seasonHistoryStatsNormal['cam_wins'] 						= GetVal(nil, 'wins') or GetVal(nil, 'rnk_wins') or '0'
		_seasonHistoryStatsNormal['cam_losses'] 					= GetVal(nil, 'losses') or GetVal(nil, 'rnk_losses') or '0'
		_seasonHistoryStatsNormal['curr_season_cam_games_played'] 	= GetVal(nil, 'matches') or GetVal(nil, 'total_games_played') or GetVal(nil, 'rnk_games_played') or '0'
		_seasonHistoryStatsNormal['k_d_a'] 							= GetVal(nil, 'k_d_a') or '0.00'
		_seasonHistoryStatsNormal['rnk_avg_score'] 					= GetVal(nil, 'avg_score') or '0'
		_seasonHistoryStatsNormal['cam_herokills'] 					= GetVal(nil, 'herokills') or '0'
		_seasonHistoryStatsNormal['cam_heroassists'] 				= GetVal(nil, 'heroassists') or '0'
		_seasonHistoryStatsNormal['cam_deaths'] 					= GetVal(nil, 'deaths') or '0'
		_seasonHistoryStatsNormal['cam_gold'] 						= GetVal(nil, 'gold') or '0'
		_seasonHistoryStatsNormal['cam_exp'] 						= GetVal(nil, 'exp') or '0'
		_seasonHistoryStatsNormal['cam_secs'] 						= GetVal(nil, 'secs') or '0'
		
		-- Average Game Statistics mapped explicitly from ASPIRE C# Server
		_seasonHistoryStatsNormal['avgGameLength'] 					= GetVal(nil, 'avgGameLength') or '0'
		_seasonHistoryStatsNormal['avgXP_min'] 						= GetVal(nil, 'avgXP_min') or '0'
		_seasonHistoryStatsNormal['avgDenies'] 						= GetVal(nil, 'avgDenies') or '0'
		_seasonHistoryStatsNormal['avgCreepKills'] 					= GetVal(nil, 'avgCreepKills') or '0'
		_seasonHistoryStatsNormal['avgNeutralKills'] 				= GetVal(nil, 'avgNeutralKills') or '0'
		_seasonHistoryStatsNormal['avgActions_min'] 				= GetVal(nil, 'avgActions_min') or '0'
		_seasonHistoryStatsNormal['avgWardsUsed'] 					= GetVal(nil, 'avgWardsUsed') or '0'
		
		-- Smackdown Panel Statistics natively appended mapped from ShowSimpleStatsResponse
		_seasonHistoryStatsNormal['cam_humiliation'] 				= GetVal(nil, 'humiliation') or '0'
		_seasonHistoryStatsNormal['cam_smackdown'] 					= GetVal(nil, 'smackdown') or '0'
		_seasonHistoryStatsNormal['cam_nemesis'] 					= GetVal(nil, 'nemesis') or '0'
		_seasonHistoryStatsNormal['cam_retribution'] 				= GetVal(nil, 'retribution') or '0'
		_seasonHistoryStatsNormal['total_games_played']				= _seasonHistoryStatsNormal['curr_season_cam_games_played']

		-- Essential fields for Overview
		_seasonHistoryStatsNormal['account_id']                     = GetVal(nil, 'account_id') or '0'
		_seasonHistoryStatsNormal['season_id']                      = GetVal(nil, 'season_id') or '0'
		_seasonHistoryStatsNormal['current_level']                  = GetVal(nil, 'level') or GetVal(nil, 'current_level') or GetVal(nil, 'rnk_level') or '0'
		_seasonHistoryStatsNormal['level']                          = _seasonHistoryStatsNormal['current_level']
		_seasonHistoryStatsNormal['level_percent']                  = GetVal(nil, 'level_percent') or GetVal(nil, 'percent_next_level') or '0'
		_seasonHistoryStatsNormal['current_ranking']                = GetVal(nil, 'ranking') or GetVal(nil, 'rank') or '0'
		_seasonHistoryStatsNormal['highest_level_current']          = GetVal(nil, 'highest_level') or GetVal(nil, 'highest_level_current') or '0'
		_seasonHistoryStatsNormal['con_reward']                     = GetVal(nil, 'con_reward') or GetVal(nil, 'con_rewards') or ''

		-- Fav Heroes (Legacy Indices/Keys)
		_seasonHistoryStatsNormal['favHero1']                          = GetVal(55, 'favHero1') or '0'
		_seasonHistoryStatsNormal['favHero2']                          = GetVal(56, 'favHero2') or '0'
		_seasonHistoryStatsNormal['favHero3']                          = GetVal(57, 'favHero3') or '0'
		_seasonHistoryStatsNormal['favHero4']                          = GetVal(58, 'favHero4') or '0'
		_seasonHistoryStatsNormal['favHero5']                          = GetVal(59, 'favHero5') or '0'
		_seasonHistoryStatsNormal['favHero1Time']                      = GetVal(60, 'favHero1Time') or '0'
		_seasonHistoryStatsNormal['favHero2Time']                      = GetVal(61, 'favHero2Time') or '0'
		_seasonHistoryStatsNormal['favHero3Time']                      = GetVal(62, 'favHero3Time') or '0'
		_seasonHistoryStatsNormal['favHero4Time']                      = GetVal(63, 'favHero4Time') or '0'
		_seasonHistoryStatsNormal['favHero5Time']                      = GetVal(64, 'favHero5Time') or '0'
		_seasonHistoryStatsNormal['favHero1_2']                        = GetVal(49, 'favHero1_2') or '0'
		_seasonHistoryStatsNormal['favHero2_2']                        = GetVal(50, 'favHero2_2') or '0'
		_seasonHistoryStatsNormal['favHero3_2']                        = GetVal(51, 'favHero3_2') or '0'
		_seasonHistoryStatsNormal['favHero4_2']                        = GetVal(52, 'favHero4_2') or '0'
		_seasonHistoryStatsNormal['favHero5_2']                        = GetVal(53, 'favHero5_2') or '0'

		-- Fill remaining with defaults or legacy fallbacks if needed, but for now specific mappings cover the reported bug
		_seasonHistoryStatsNormal['create_date']                    = GetVal(nil, 'create_date') or ''
		_seasonHistoryStatsNormal['last_activity']                  = GetVal(nil, 'last_activity') or ''

		-- Also ensure _seasonStatsNormal is updated
		_seasonStatsNormal = _seasonHistoryStatsNormal
		bNormalSeasonStatsRetrieved = true

		-- Let the legacy block fill in the rest of the missing stats
	end
	
	if true then
		_seasonHistoryStatsNormal['nickname']                          = GetVal(1, 'nickname') or ''
		_seasonHistoryStatsNormal['create_date']                       = GetVal(76, 'create_date') or ''
		_seasonHistoryStatsNormal['last_activity']                     = GetVal(52, 'last_activity') or ''
		
		_seasonHistoryStatsNormal['account_id']                        = GetVal(5, 'account_id') or '0'
		_seasonHistoryStatsNormal['season_id']                         = GetVal(138, 'season_id') or '0'
		_seasonHistoryStatsNormal['cam_wins']                          = GetVal(7, 'cam_wins') or '0'
		_seasonHistoryStatsNormal['cam_losses']                        = GetVal(8, 'cam_losses') or '0'
		_seasonHistoryStatsNormal['cam_discos']                        = GetVal(12, 'cam_discos') or '0'
		_seasonHistoryStatsNormal['cam_concedes']                      = GetVal(9, 'cam_concedes') or '0'
		_seasonHistoryStatsNormal['cam_concedevotes']                  = GetVal(10, 'cam_concedevotes') or '0'
		_seasonHistoryStatsNormal['cam_buybacks']                      = GetVal(11, 'cam_buybacks') or '0'
		_seasonHistoryStatsNormal['cam_herokills']                     = GetVal(21, 'cam_herokills') or '0'
		_seasonHistoryStatsNormal['cam_heroassists']                   = GetVal(25, 'cam_heroassists') or '0'
		_seasonHistoryStatsNormal['cam_deaths']                        = GetVal(26, 'cam_deaths') or '0'
		_seasonHistoryStatsNormal['cam_gold']                          = GetVal(43, 'cam_gold') or '0'
		_seasonHistoryStatsNormal['cam_exp']                           = GetVal(45, 'cam_exp') or '0'
		_seasonHistoryStatsNormal['cam_secs']                          = GetVal(47, 'cam_secs') or '0'
		_seasonHistoryStatsNormal['cam_consumables']                   = GetVal(48, 'cam_consumables') or '0'
		_seasonHistoryStatsNormal['cam_wards']                         = GetVal(49, 'cam_wards') or '0'
		_seasonHistoryStatsNormal['cam_bloodlust']                     = GetVal(94, 'cam_bloodlust') or '0'
		_seasonHistoryStatsNormal['cam_doublekill']                    = GetVal(95, 'cam_doublekill') or '0'
		_seasonHistoryStatsNormal['cam_triplekill']                    = GetVal(96, 'cam_triplekill') or '0'
		_seasonHistoryStatsNormal['cam_quadkill']                      = GetVal(97, 'cam_quadkill') or '0'
		_seasonHistoryStatsNormal['cam_annihilation']                  = GetVal(98, 'cam_annihilation') or '0'
		_seasonHistoryStatsNormal['cam_ks3']                           = GetVal(99, 'cam_ks3') or '0'
		_seasonHistoryStatsNormal['cam_ks4']                           = GetVal(100, 'cam_ks4') or '0'
		_seasonHistoryStatsNormal['cam_ks5']                           = GetVal(101, 'cam_ks5') or '0'
		_seasonHistoryStatsNormal['cam_ks6']                           = GetVal(102, 'cam_ks6') or '0'
		_seasonHistoryStatsNormal['cam_ks7']                           = GetVal(103, 'cam_ks7') or '0'
		_seasonHistoryStatsNormal['cam_ks8']                           = GetVal(104, 'cam_ks8') or '0'
		_seasonHistoryStatsNormal['cam_ks9']                           = GetVal(105, 'cam_ks9') or '0'
		_seasonHistoryStatsNormal['cam_ks10']                          = GetVal(106, 'cam_ks10') or '0'
		_seasonHistoryStatsNormal['cam_ks15']                          = GetVal(107, 'cam_ks15') or '0'
		_seasonHistoryStatsNormal['cam_smackdown']                     = GetVal(108, 'cam_smackdown') or '0'
		_seasonHistoryStatsNormal['cam_humiliation']                   = GetVal(109, 'cam_humiliation') or '0'
		_seasonHistoryStatsNormal['cam_nemesis']                       = GetVal(110, 'cam_nemesis') or '0'
		_seasonHistoryStatsNormal['cam_retribution']                   = GetVal(111, 'cam_retribution') or '0'
		_seasonHistoryStatsNormal['cam_teamkillexp']                   = GetVal(31, 'cam_teamkillexp') or '0'
		_seasonHistoryStatsNormal['cam_teamkillgold']                  = GetVal(32, 'cam_teamkillgold') or '0'
		_seasonHistoryStatsNormal['cam_actions']                       = GetVal(46, 'cam_actions') or '0'
		_seasonHistoryStatsNormal['cam_amm_team_rating']               = GetVal(14, 'cam_amm_team_rating') or '0'
		_seasonHistoryStatsNormal['favHero1']                          = GetVal(55, 'favHero1') or '0'
		_seasonHistoryStatsNormal['favHero2']                          = GetVal(56, 'favHero2') or '0'
		_seasonHistoryStatsNormal['favHero3']                          = GetVal(57, 'favHero3') or '0'
		_seasonHistoryStatsNormal['favHero4']                          = GetVal(58, 'favHero4') or '0'
		_seasonHistoryStatsNormal['favHero5']                          = GetVal(59, 'favHero5') or '0'
		_seasonHistoryStatsNormal['favHero1Time']                      = GetVal(60, 'favHero1Time') or '0'
		_seasonHistoryStatsNormal['favHero2Time']                      = GetVal(61, 'favHero2Time') or '0'
		_seasonHistoryStatsNormal['favHero3Time']                      = GetVal(62, 'favHero3Time') or '0'
		_seasonHistoryStatsNormal['favHero4Time']                      = GetVal(63, 'favHero4Time') or '0'
		_seasonHistoryStatsNormal['favHero5Time']                      = GetVal(64, 'favHero5Time') or '0'
		_seasonHistoryStatsNormal['favHero1_2']                        = GetVal(77, 'favHero1_2') or '0'
		_seasonHistoryStatsNormal['favHero2_2']                        = GetVal(78, 'favHero2_2') or '0'
		_seasonHistoryStatsNormal['favHero3_2']                        = GetVal(79, 'favHero3_2') or '0'
		_seasonHistoryStatsNormal['favHero4_2']                        = GetVal(80, 'favHero4_2') or '0'
		_seasonHistoryStatsNormal['favHero5_2']                        = GetVal(81, 'favHero5_2') or '0'
		_seasonHistoryStatsNormal['avgCreepKills']                     = GetVal(55, 'avgCreepKills') or '0'
		_seasonHistoryStatsNormal['avgDenies']                         = GetVal(56, 'avgDenies') or '0'
		_seasonHistoryStatsNormal['avgXP_min']                         = GetVal(57, 'avgXP_min') or '0'
		_seasonHistoryStatsNormal['avgGold_min']                       = GetVal(58, 'avgGold_min') or '0'
		_seasonHistoryStatsNormal['avgActions_min']                    = GetVal(59, 'avgActions_min') or '0'
		_seasonHistoryStatsNormal['avgWardsUsed']                      = GetVal(60, 'avgWardsUsed') or '0'
		_seasonHistoryStatsNormal['k_d_a']                             = GetVal(61, 'k_d_a') or '0'
		_seasonHistoryStatsNormal['avgGameLength']                     = GetVal(62, 'avgGameLength') or '0'
		_seasonHistoryStatsNormal['avgNeutralKills']                   = GetVal(63, 'avgNeutralKills') or '0'
		_seasonHistoryStatsNormal['level_exp']                         = GetVal(64, 'level_exp') or '0'
		_seasonHistoryStatsNormal['level_percent']                     = GetVal(65, 'level_percent') or '0'
		_seasonHistoryStatsNormal['max_exp']                           = GetVal(66, 'max_exp') or '0'
		_seasonHistoryStatsNormal['min_exp']                           = GetVal(67, 'min_exp') or '0'
		_seasonHistoryStatsNormal['mid_games_played']                  = GetVal(68, 'mid_games_played') or '0'
		_seasonHistoryStatsNormal['mid_discos']                        = GetVal(69, 'mid_discos') or '0'
		_seasonHistoryStatsNormal['total_games_played']                = GetVal(70, 'total_games_played') or '0'
		_seasonHistoryStatsNormal['total_discos']                      = GetVal(71, 'total_discos') or '0'
		_seasonHistoryStatsNormal['event_id']                          = GetVal(72, 'event_id') or '0'
		_seasonHistoryStatsNormal['events']                            = GetVal(73, 'events') or '0'
		_seasonHistoryStatsNormal['uncs_discos']                       = GetVal(74, 'uncs_discos') or '0'
		_seasonHistoryStatsNormal['unrnk_discos']                      = GetVal(75, 'unrnk_discos') or '0'
		_seasonHistoryStatsNormal['uncs_games_played']                 = GetVal(76, 'uncs_games_played') or '0'
		_seasonHistoryStatsNormal['unrnk_games_played']                = GetVal(77, 'unrnk_games_played') or '0'
		_seasonHistoryStatsNormal['rift_games_played']                 = GetVal(78, 'rift_games_played') or '0'
		_seasonHistoryStatsNormal['rift_discos']                       = GetVal(79, 'rift_discos') or '0'
		_seasonHistoryStatsNormal['highest_level']                     = GetVal(80, 'highest_level') or '0'
		_seasonHistoryStatsNormal['highest_level_current']             = GetVal(81, 'highest_level_current') or '0'
	-- Legacy Cleanup Complete. GetVal logic handles data population.
	
	-- Map _seasonHistoryStatsNormal to local generalInfo or other UI structures if needed below.
	-- (Logic below uses _seasonStatsNormal, so we might need to alias it or update below)

	-- Sync tables for compatibility with rest of function if it uses _seasonStatsNormal
	_seasonStatsNormal = _seasonHistoryStatsNormal
	
	bNormalSeasonStatsRetrieved = true
		_seasonStatsNormal['cam_heroassists'] 				       = GetVal(10, 'cam_heroassists') or '0'
		_seasonHistoryStatsNormal['favHero5_2']                        = GetVal(54, 'favHero5_2') or '0'
		_seasonHistoryStatsNormal['favHero1id']                        = GetVal(97, 'favHero1id') or '0'
		_seasonHistoryStatsNormal['favHero2id']                        = GetVal(98, 'favHero2id') or '0'
		_seasonHistoryStatsNormal['favHero3id']                        = GetVal(99, 'favHero3id') or '0'
		_seasonHistoryStatsNormal['favHero4id']                        = GetVal(100, 'favHero4id') or '0'
		_seasonHistoryStatsNormal['favHero5id']                        = GetVal(101, 'favHero5id') or '0'
		_seasonHistoryStatsNormal['error']                             = GetVal(102, 'error') or '0'
		_seasonHistoryStatsNormal['cam_level']                         = GetVal(103, 'season_level') or '0'
		_seasonHistoryStatsNormal['selected_upgrades']                 = GetVal(104, 'selected_upgrades') or '0'
		_seasonHistoryStatsNormal['acc_games_played']                  = GetVal(90, 'acc_games_played') or '0'
		_seasonHistoryStatsNormal['total_games_played']                = GetVal(126, 'total_games_played') or _seasonHistoryStatsNormal['total_games_played'] or '0'
		_seasonHistoryStatsNormal['total_discos']                      = GetVal(127, 'total_discos') or _seasonHistoryStatsNormal['total_discos'] or '0'
		_seasonHistoryStatsNormal['rnk_games_played']                  = GetVal(106, 'rnk_games_played') or '0'
		_seasonHistoryStatsNormal['acc_discos']                        = GetVal(107, 'acc_discos') or '0'
		_seasonHistoryStatsNormal['rnk_discos']                        = GetVal(108, 'rnk_discos') or '0'
		_seasonHistoryStatsNormal['cam_bloodlust']                     = GetVal(18, 'cam_bloodlust') or '0'
		_seasonHistoryStatsNormal['cam_doublekill']                    = GetVal(19, 'cam_doublekill') or '0'
		_seasonHistoryStatsNormal['cam_triplekill']                    = GetVal(20, 'cam_triplekill') or '0'
		_seasonHistoryStatsNormal['cam_quadkill']                      = GetVal(21, 'cam_quadkill') or '0'
		_seasonHistoryStatsNormal['cam_annihilation']                  = GetVal(22, 'cam_annihilation') or '0'
		_seasonHistoryStatsNormal['cam_ks3']                           = GetVal(23, 'cam_ks3') or '0'
		_seasonHistoryStatsNormal['cam_ks4']                           = GetVal(24, 'cam_ks4') or '0'
		_seasonHistoryStatsNormal['cam_ks5']                           = GetVal(25, 'cam_ks5') or '0'
		_seasonHistoryStatsNormal['cam_ks6']                           = GetVal(26, 'cam_ks6') or '0'
		_seasonHistoryStatsNormal['cam_ks7']                           = GetVal(27, 'cam_ks7') or '0'
		_seasonHistoryStatsNormal['cam_ks8']                           = GetVal(28, 'cam_ks8') or '0'
		_seasonHistoryStatsNormal['cam_ks9']                           = GetVal(29, 'cam_ks9') or '0'
		_seasonHistoryStatsNormal['cam_ks10']                          = GetVal(30, 'cam_ks10') or '0'
		_seasonHistoryStatsNormal['cam_ks15']                          = GetVal(31, 'cam_ks15') or '0'
		_seasonHistoryStatsNormal['cam_smackdown']                     = GetVal(32, 'cam_smackdown') or '0'
		_seasonHistoryStatsNormal['cam_humiliation']                   = GetVal(33, 'cam_humiliation') or '0'
		_seasonHistoryStatsNormal['cam_nemesis']                       = GetVal(34, 'cam_nemesis') or '0'
		_seasonHistoryStatsNormal['cam_retribution']                   = GetVal(35, 'cam_retribution') or '0'
		_seasonHistoryStatsNormal['cam_teamkillexp']                   = GetVal(36, 'cam_teamkillexp') or '0'
		_seasonHistoryStatsNormal['cam_teamkillgold']                  = GetVal(37, 'cam_teamkillgold') or '0'
		_seasonHistoryStatsNormal['cam_actions']                       = GetVal(38, 'cam_actions') or '0'
		_seasonHistoryStatsNormal['cam_amm_team_rating']               = GetVal(39, 'cam_amm_team_rating') or '0'
		
		-- S2 Games duplicate legacy `favHero` bindings stripped (Previously overrode valid 55-64 bounds to invalid 40-54 fallback resulting in wiped stats)
		_seasonHistoryStatsNormal['avgCreepKills']                     = GetVal(73, 'avgCreepKills') or '0'
		_seasonHistoryStatsNormal['avgDenies']                         = GetVal(72, 'avgDenies') or '0'
		_seasonHistoryStatsNormal['avgXP_min']                         = GetVal(71, 'avgXP_min') or '0'
		_seasonHistoryStatsNormal['avgGold_min']                       = GetVal(58, 'avgGold_min') or '0'
		_seasonHistoryStatsNormal['avgActions_min']                    = GetVal(75, 'avgActions_min') or '0'
		_seasonHistoryStatsNormal['avgWardsUsed']                      = GetVal(76, 'avgWardsUsed') or '0'
		_seasonHistoryStatsNormal['k_d_a']                             = GetVal(69, 'k_d_a') or '0'
		_seasonHistoryStatsNormal['avgGameLength']                     = GetVal(70, 'avgGameLength') or '0'
		_seasonHistoryStatsNormal['avgNeutralKills']                   = GetVal(74, 'avgNeutralKills') or '0'
		_seasonHistoryStatsNormal['current_level']                     = GetVal(114, 'current_level') or '0'
		_seasonHistoryStatsNormal['level_exp']                         = GetVal(115, 'level_exp') or '0'
		_seasonHistoryStatsNormal['level_percent']                     = GetVal(121, 'level_percent') or '0'
		_seasonHistoryStatsNormal['max_exp']                           = GetVal(122, 'max_exp') or '0'
		_seasonHistoryStatsNormal['min_exp']                           = GetVal(123, 'min_exp') or '0'
		_seasonHistoryStatsNormal['mid_games_played']                  = GetVal(124, 'mid_games_played') or '0'
		_seasonHistoryStatsNormal['mid_discos']                        = GetVal(125, 'mid_discos') or '0'
		-- Removed duplicate total_games_played and total_discos as they are handled at 1741 and 1742
		_seasonHistoryStatsNormal['event_id']                          = GetVal(128, 'event_id') or '0'
		_seasonHistoryStatsNormal['events']                            = GetVal(129, 'events') or '0'
		_seasonHistoryStatsNormal['uncs_discos']                       = GetVal(130, 'uncs_discos') or '0'
		_seasonHistoryStatsNormal['unrnk_discos']                      = GetVal(131, 'unrnk_discos') or '0'
		_seasonHistoryStatsNormal['uncs_games_played']                 = GetVal(132, 'uncs_games_played') or '0'
		_seasonHistoryStatsNormal['unrnk_games_played']                = GetVal(133, 'unrnk_games_played') or '0'
		_seasonHistoryStatsNormal['rift_games_played']                 = GetVal(134, 'rift_games_played') or '0'
		_seasonHistoryStatsNormal['rift_discos']                       = GetVal(135, 'rift_discos') or '0'
		_seasonHistoryStatsNormal['highest_level']                     = GetVal(136, 'highest_level') or '0'
		_seasonHistoryStatsNormal['highest_level_current']             = GetVal(137, 'highest_level_current') or '0'
		_seasonHistoryStatsNormal['cam_level']                         = GetVal(138, 'cam_level') or '0'
		_seasonHistoryStatsNormal['selected_upgrades']                 = GetVal(89, 'selected_upgrades') or '0'
		_seasonHistoryStatsNormal['acc_games_played']                  = GetVal(90, 'acc_games_played') or '0'
		_seasonHistoryStatsNormal['rnk_games_played']                  = GetVal(140, 'rnk_games_played') or '0'
		_seasonHistoryStatsNormal['acc_discos']                        = GetVal(92, 'acc_discos') or '0'
		_seasonHistoryStatsNormal['rnk_discos']                        = GetVal(142, 'rnk_discos') or '0'
		_seasonHistoryStatsNormal['level']                             = GetVal(114, 'level') or '0'
		_seasonHistoryStatsNormal['standing']                          = GetVal(120, 'standing') or '0'
		_seasonHistoryStatsNormal['highest_ranking']                   = GetVal(148, 'highest_ranking') or '0'
		_seasonHistoryStatsNormal['con_reward']                        = GetVal(149, 'con_reward') or ''
	end
	
	-- Legacy Cleanup: Removed obsolete _seasonStatsNormal assignments.
	-- The function now uses _seasonHistoryStatsNormal populated via GetVal.
	
	-- Map _seasonHistoryStatsNormal to local generalInfo or other UI structures if needed below.
	-- (Logic below uses _seasonStatsNormal, so we might need to alias it or update below)

	-- Sync tables for compatibility with rest of function if it uses _seasonStatsNormal
	_seasonStatsNormal = _seasonHistoryStatsNormal
	
	bNormalSeasonStatsRetrieved = true
	
	local generalInfo = {}
	generalInfo.nickname 			= _seasonStatsNormal['nickname'] or ''
	generalInfo.clanname			= _seasonStatsNormal['name'] or ''
	generalInfo.accountid 			= tonumber(_seasonStatsNormal['account_id']) or -1
	generalInfo.lastmatchdate 		= _seasonStatsNormal['last_activity'] or ''
	generalInfo.createdate 			= _seasonStatsNormal['create_date'] or ''
	generalInfo.selected_upgrade	= _seasonStatsNormal['selected_upgrades'] or ''
	generalInfo.level 				= _seasonStatsNormal['level'] or ''
	generalInfo.level_exp 			= tonumber(_seasonStatsNormal['level_exp']) or 0
	generalInfo.matches 			= tonumber(_seasonStatsNormal['total_games_played']) or 0
	generalInfo.disconnects 		= tonumber(_seasonStatsNormal['total_discos']) or 0
	generalInfo.standing			= tonumber(_seasonStatsNormal['standing']) or 0

	Player_Stats_V2:SetGeneralInfo(generalInfo)

	local statsInfo = {}
	if true then	
		statsInfo.account_id					= _seasonStatsNormal['account_id']           
		statsInfo.season_id 					= _seasonStatsNormal['season_id']
		statsInfo.mmr							= _seasonStatsNormal['smr']
		statsInfo.current_level 				= _seasonStatsNormal['current_level']
		statsInfo.level_exp 					= _seasonStatsNormal['level_exp']
		statsInfo.level_percent					= _seasonStatsNormal['level_percent']
		statsInfo.max_exp						= _seasonStatsNormal['max_exp']
		statsInfo.min_exp						= _seasonStatsNormal['min_exp']
		statsInfo.current_ranking				= _seasonStatsNormal['current_ranking']
		statsInfo.highest_level_current			= _seasonStatsNormal['highest_level_current']
		statsInfo.highest_ranking				= _seasonStatsNormal['highest_ranking']
		statsInfo.favHero1						= _seasonStatsNormal['favHero1']
		statsInfo.favHero2						= _seasonStatsNormal['favHero2']
		statsInfo.favHero3						= _seasonStatsNormal['favHero3']
		statsInfo.favHero4						= _seasonStatsNormal['favHero4']
		statsInfo.favHero5						= _seasonStatsNormal['favHero5']        
		statsInfo.favHero1Time					= _seasonStatsNormal['favHero1Time']
		statsInfo.favHero2Time					= _seasonStatsNormal['favHero2Time']
		statsInfo.favHero3Time					= _seasonStatsNormal['favHero3Time']
		statsInfo.favHero4Time					= _seasonStatsNormal['favHero4Time']
		statsInfo.favHero5Time					= _seasonStatsNormal['favHero5Time']
		statsInfo.favHero1_2					= _seasonStatsNormal['favHero1_2']
		statsInfo.favHero2_2					= _seasonStatsNormal['favHero2_2']
		statsInfo.favHero3_2					= _seasonStatsNormal['favHero3_2']
		statsInfo.favHero4_2					= _seasonStatsNormal['favHero4_2']
		statsInfo.favHero5_2					= _seasonStatsNormal['favHero5_2']
		statsInfo.total_games_played			= _seasonStatsNormal['total_games_played']
		statsInfo.cur_games_played     			= _seasonStatsNormal['curr_season_cam_games_played']
		statsInfo.total_discos                  = _seasonStatsNormal['total_discos']
		statsInfo.cur_discos           			= _seasonStatsNormal['curr_season_cam_discos']
		statsInfo.avgCreepKills                 = _seasonStatsNormal['avgCreepKills']
		statsInfo.avgDenies                     = _seasonStatsNormal['avgDenies']
		statsInfo.avgGameLength                 = _seasonStatsNormal['avgGameLength']
		statsInfo.avgXP_min                     = _seasonStatsNormal['avgXP_min']
		statsInfo.avgActions_min                = _seasonStatsNormal['avgActions_min']
		statsInfo.avgNeutralKills               = _seasonStatsNormal['avgNeutralKills']
		statsInfo.avgWardsUsed                  = _seasonStatsNormal['avgWardsUsed']
		statsInfo.k_d_a                         = _seasonStatsNormal['k_d_a']
		statsInfo.wins							= _seasonStatsNormal['cam_wins']
		statsInfo.losses						= _seasonStatsNormal['cam_losses']
		statsInfo.herokills                    	= _seasonStatsNormal['cam_herokills']
		statsInfo.deaths                       	= _seasonStatsNormal['cam_deaths']
		statsInfo.heroassists                  	= _seasonStatsNormal['cam_heroassists']
		statsInfo.exp                          	= _seasonStatsNormal['cam_exp']
		statsInfo.gold                         	= _seasonStatsNormal['cam_gold']
		statsInfo.secs                         	= _seasonStatsNormal['cam_secs']
		statsInfo.smackdown                    	= _seasonStatsNormal['cam_smackdown']
		statsInfo.humiliation                  	= _seasonStatsNormal['cam_humiliation']
		statsInfo.ks3                          	= _seasonStatsNormal['cam_ks3']
		statsInfo.ks4                          	= _seasonStatsNormal['cam_ks4']
		statsInfo.ks5                          	= _seasonStatsNormal['cam_ks5']
		statsInfo.ks6                          	= _seasonStatsNormal['cam_ks6']
		statsInfo.ks7                          	= _seasonStatsNormal['cam_ks7']
		statsInfo.ks8                          	= _seasonStatsNormal['cam_ks8']
		statsInfo.ks9                          	= _seasonStatsNormal['cam_ks9']
		statsInfo.ks10                         	= _seasonStatsNormal['cam_ks10']
		statsInfo.ks15                         	= _seasonStatsNormal['cam_ks15']
		statsInfo.doublekill                   	= _seasonStatsNormal['cam_doublekill']
		statsInfo.triplekill                   	= _seasonStatsNormal['cam_triplekill']
		statsInfo.quadkill                     	= _seasonStatsNormal['cam_quadkill']
		statsInfo.annihilation                 	= _seasonStatsNormal['cam_annihilation']
		statsInfo.bloodlust                    	= _seasonStatsNormal['cam_bloodlust']
		statsInfo.con_reward					= _seasonStatsNormal['con_reward']
	end
	
	if _currentTab == 'overview' and bHeroMasteryRetrieved then
		println('^gSetting overview season info from normal season result!^*')
		SetOverviewSeasonInfo(statsInfo, true)
	
	elseif _currentTab == 'stats' then
		SetPlayerStatsStatsInfo(statsInfo, false, true, false)
		
		local tip_colors = {}
		local tip_numbers = {}
		tip_colors.public = '^w'
		tip_colors.normal = '^w'
		tip_colors.casual = '^w'
		tip_colors.season_normal = '^r'
		tip_colors.season_casual = '^w'
		tip_colors.pre_season_normal = '^w'
		tip_colors.pre_season_casual = '^w'
		tip_colors.midwars = '^w'
		
		tip_numbers.public = _seasonStatsNormal['acc_games_played']
		tip_numbers.normal = _seasonStatsNormal['rnk_games_played']
		tip_numbers.casual = _seasonStatsNormal['cs_games_played']
		tip_numbers.season_normal = _seasonStatsNormal['curr_season_cam_games_played']
		tip_numbers.season_casual = _seasonStatsNormal['curr_season_cam_cs_games_played']
		tip_numbers.pre_season_normal = _seasonStatsNormal['prev_seasons_cam_games_played']
		tip_numbers.pre_season_casual = _seasonStatsNormal['prev_seasons_cam_cs_games_played']
		tip_numbers.midwars = _seasonStatsNormal['mid_games_played']
		SetTipMatchesPlayed(tip_colors, tip_numbers)
		
		tip_numbers.public = _seasonStatsNormal['acc_discos']
		tip_numbers.normal = _seasonStatsNormal['rnk_discos']
		tip_numbers.casual = _seasonStatsNormal['cs_discos']
		tip_numbers.season_normal = _seasonStatsNormal['curr_season_cam_discos']
		tip_numbers.season_casual = _seasonStatsNormal['curr_season_cam_cs_discos']
		tip_numbers.pre_season_normal = _seasonStatsNormal['prev_seasons_cam_discos']
		tip_numbers.pre_season_casual = _seasonStatsNormal['prev_seasons_cam_cs_discos']
		tip_numbers.midwars = _seasonStatsNormal['mid_discos']
		SetTipDisconnects(tip_colors, tip_numbers)
	else
	end
end

function Player_Stats_V2:OnPlayerStatsCasualSeasonResult(...)
	local arg = { ... }
	arg.n = select('#', ...)
	local arg = arg or { ... }
	if not arg.n then arg.n = #arg end
	println('^c[PlayerStats] ^*Casual Season Result Received. Nickname: ' .. tostring(arg and arg[1] or 'nil'))
	-- Logan (2026-02-12): Fix for Modern Server Response (Table vs Positional)
	-- Conversion Helper - REMOVED (Use SafeGet)
	if type(arg[1]) == 'userdata' then arg = arg[1] end

	local function SafeGet(src, key)
		if type(src) == 'userdata' then
			local s, v = pcall(function() return src[key] end)
			if s then return v end
		elseif type(src) == 'table' then
			return src[key]
		end
		return nil
	end

	if type(arg[1]) == 'string' and arg[2] == nil then
		local data = Player_Stats_V2:PHP_Unserialize(arg[1])
		if data then 
			-- Fix 0-based indexing from C# Hashtable
			if data[0] ~= nil then
				local fixed = {}
				for k,v in pairs(data) do
					if type(k) == 'number' then fixed[k+1] = v else fixed[k] = v end
				end
				data = fixed
			end
			arg = data 
			println('^gOnPlayerStatsCasualSeasonResult: Unserialized Base64 payload.')
		end
	end
	-- Reconstruct flat Key-Value unrolls if the C++ Engine stripped the userdata wrapper
	if type(arg[1]) == 'string' and arg.n and arg.n > 20 then
		-- Detect if this is a flattened dictionary: key1, val1, key2, val2...
		local isFlatDict = false
		for i = 1, math.min(arg.n, 10), 2 do
			if type(arg[i]) == 'string' and (arg[i] == 'account_id' or arg[i] == 'nickname' or arg[i] == 'error_code') then
				isFlatDict = true
				break
			end
		end

		if isFlatDict then
			local rebuilt = {}
			for i = 1, arg.n, 2 do
				local k = arg[i]
				local v = arg[i+1]
				if k ~= nil then
					rebuilt[k] = v
				end
			end
			arg = rebuilt
			println('^y[PlayerStats] Successfully rebuilt Casual variadic flattened Dictionary!^*')
		end
	end

	if not bCasualSeasonStatsRetrieved then
		-- Modern Server Support
		local nickname = SafeGet(arg, 'nickname')
		if nickname then
			-- Create a new Lua table to avoid userdata issues downstream
			_seasonStatsCasual = {}
			_seasonStatsCasual['nickname'] = nickname
			
			-- MAPPING: Modern Keys -> Legacy Keys
			_seasonStatsCasual['cam_cs_wins'] 						= SafeGet(arg, 'wins') or SafeGet(arg, 'cs_wins') or '0'
			_seasonStatsCasual['cam_cs_losses'] 					= SafeGet(arg, 'losses') or SafeGet(arg, 'cs_losses') or '0'
			_seasonStatsCasual['curr_season_cam_cs_games_played'] 	= SafeGet(arg, 'matches') or SafeGet(arg, 'total_games_played') or SafeGet(arg, 'cs_games_played') or '0'
			_seasonStatsCasual['cam_cs_amm_team_rating'] 			= SafeGet(arg, 'omm_rating') or SafeGet(arg, 'rating') or '0'
			_seasonStatsCasual['k_d_a'] 							= SafeGet(arg, 'k_d_a') or '0.00'
			_seasonStatsCasual['cam_cs_avg_score'] 					= SafeGet(arg, 'avg_score') or '0'
			_seasonStatsCasual['cam_cs_herokills'] 					= SafeGet(arg, 'herokills') or '0'
			_seasonStatsCasual['cam_cs_heroassists'] 				= SafeGet(arg, 'heroassists') or '0'
			_seasonStatsCasual['cam_cs_deaths'] 					= SafeGet(arg, 'deaths') or '0'
			_seasonStatsCasual['cam_cs_gold'] 						= SafeGet(arg, 'gold') or '0'
			_seasonStatsCasual['cam_cs_exp'] 						= SafeGet(arg, 'exp') or '0'
			_seasonStatsCasual['cam_cs_secs'] 						= SafeGet(arg, 'secs') or '0'
			_seasonStatsCasual['cam_cs_smackdown'] 					= SafeGet(arg, 'smackdown') or '0'
			_seasonStatsCasual['cam_cs_humiliation'] 				= SafeGet(arg, 'humiliation') or '0'
			_seasonStatsCasual['cam_cs_nemesis'] 					= SafeGet(arg, 'nemesis') or '0'
			_seasonStatsCasual['cam_cs_retribution'] 				= SafeGet(arg, 'retribution') or '0'
			_seasonStatsCasual['total_games_played']				= _seasonStatsCasual['curr_season_cam_cs_games_played']

			_seasonStatsCasual['current_level'] 					= SafeGet(arg, 'level') or SafeGet(arg, 'current_level') or SafeGet(arg, 'cs_level') or '0'
			_seasonStatsCasual['level'] 							= _seasonStatsCasual['current_level']
			_seasonStatsCasual['highest_level_current'] 			= SafeGet(arg, 'highest_level') or SafeGet(arg, 'highest_level_current') or '0'
			_seasonStatsCasual['current_ranking'] 					= SafeGet(arg, 'ranking') or SafeGet(arg, 'rank') or '0'
			_seasonStatsCasual['con_reward'] 						= SafeGet(arg, 'con_reward') or SafeGet(arg, 'con_rewards') or ''

			-- Fav Heroes
			_seasonStatsCasual['favHero1']                          = SafeGet(arg, 'favHero1') or SafeGet(arg, '55') or '0'
			_seasonStatsCasual['favHero2']                          = SafeGet(arg, 'favHero2') or SafeGet(arg, '56') or '0'
			_seasonStatsCasual['favHero3']                          = SafeGet(arg, 'favHero3') or SafeGet(arg, '57') or '0'
			_seasonStatsCasual['favHero4']                          = SafeGet(arg, 'favHero4') or SafeGet(arg, '58') or '0'
			_seasonStatsCasual['favHero5']                          = SafeGet(arg, 'favHero5') or SafeGet(arg, '59') or '0'
			_seasonStatsCasual['favHero1Time']                      = SafeGet(arg, 'favHero1Time') or SafeGet(arg, '60') or '0'
			_seasonStatsCasual['favHero2Time']                      = SafeGet(arg, 'favHero2Time') or SafeGet(arg, '61') or '0'
			_seasonStatsCasual['favHero3Time']                      = SafeGet(arg, 'favHero3Time') or SafeGet(arg, '62') or '0'
			_seasonStatsCasual['favHero4Time']                      = SafeGet(arg, 'favHero4Time') or SafeGet(arg, '63') or '0'
			_seasonStatsCasual['favHero5Time']                      = SafeGet(arg, 'favHero5Time') or SafeGet(arg, '64') or '0'
			_seasonStatsCasual['favHero1_2']                        = SafeGet(arg, 'favHero1_2') or SafeGet(arg, '77') or '0'
			_seasonStatsCasual['favHero2_2']                        = SafeGet(arg, 'favHero2_2') or SafeGet(arg, '78') or '0'
			_seasonStatsCasual['favHero3_2']                        = SafeGet(arg, 'favHero3_2') or SafeGet(arg, '79') or '0'
			_seasonStatsCasual['favHero4_2']                        = SafeGet(arg, 'favHero4_2') or SafeGet(arg, '80') or '0'
			_seasonStatsCasual['favHero5_2']                        = SafeGet(arg, 'favHero5_2') or SafeGet(arg, '81') or '0'

			bCasualSeasonStatsRetrieved = true
		else
			_seasonStatsCasual['nickname']		        			= arg[1] or '0'
		_seasonStatsCasual['name']                              = arg[2] or '0'
		_seasonStatsCasual['rank']                              = arg[3] or '0'
		_seasonStatsCasual['cam_cs_level']                      = arg[4] or '0'
		_seasonStatsCasual['account_id']                        = arg[5] or '0'
		_seasonStatsCasual['curr_season_cam_cs_games_played']   = arg[6] or '0'
		_seasonStatsCasual['cam_cs_wins']                       = arg[7] or '0'
		_seasonStatsCasual['cam_cs_losses']                     = arg[8] or '0'
		_seasonStatsCasual['cam_cs_concedes']                   = arg[9] or '0'
		_seasonStatsCasual['cam_cs_concedevotes']               = arg[10] or '0'
		_seasonStatsCasual['cam_cs_buybacks']                   = arg[11] or '0'
		_seasonStatsCasual['curr_season_cam_cs_discos']         = arg[12] or '0'
		_seasonStatsCasual['cam_cs_kicked']                     = arg[13] or '0'
		_seasonStatsCasual['cam_cs_amm_team_rating']			= arg[14] or '0'
		_seasonStatsCasual['cam_cs_pub_count']                  = arg[15] or '0'
		_seasonStatsCasual['cam_cs_amm_solo_rating']            = arg[16] or '0'
		_seasonStatsCasual['cam_cs_amm_solo_count']             = arg[17] or '0'
		_seasonStatsCasual['cam_cs_amm_team_rating']            = arg[18] or '0'
		_seasonStatsCasual['cam_cs_amm_team_count']             = arg[19] or '0'
		_seasonStatsCasual['cam_cs_avg_score']                  = arg[20] or '0'
		_seasonStatsCasual['cam_cs_herokills']                  = arg[21] or '0'
		_seasonStatsCasual['cam_cs_herodmg']                    = arg[22] or '0'
		_seasonStatsCasual['cam_cs_heroexp']                    = arg[23] or '0'
		_seasonStatsCasual['cam_cs_herokillsgold']              = arg[24] or '0'
		_seasonStatsCasual['cam_cs_heroassists']                = arg[25] or '0'
		_seasonStatsCasual['cam_cs_deaths']                     = arg[26] or '0'
		_seasonStatsCasual['cam_cs_goldlost2death']             = arg[27] or '0'
		_seasonStatsCasual['cam_cs_secs_dead']                  = arg[28] or '0'
		_seasonStatsCasual['cam_cs_teamcreepkills']             = arg[29] or '0'
		_seasonStatsCasual['cam_cs_teamcreepdmg']               = arg[30] or '0'
		_seasonStatsCasual['cam_cs_teamcreepexp']               = arg[31] or '0'
		_seasonStatsCasual['cam_cs_teamcreepgold']              = arg[32] or '0'
		_seasonStatsCasual['cam_cs_neutralcreepkills']          = arg[33] or '0'
		_seasonStatsCasual['cam_cs_neutralcreepdmg']            = arg[34] or '0'
		_seasonStatsCasual['cam_cs_neutralcreepexp']            = arg[35] or '0'
		_seasonStatsCasual['cam_cs_neutralcreepgold']           = arg[36] or '0'
		_seasonStatsCasual['cam_cs_bdmg']                       = arg[37] or '0'
		_seasonStatsCasual['cam_cs_bdmgexp']                    = arg[38] or '0'
		_seasonStatsCasual['cam_cs_razed']                      = arg[39] or '0'
		_seasonStatsCasual['cam_cs_bgold']                      = arg[40] or '0'
		_seasonStatsCasual['cam_cs_denies']                     = arg[41] or '0'
		_seasonStatsCasual['cam_cs_exp_denied']                 = arg[42] or '0'
		_seasonStatsCasual['cam_cs_gold']                       = arg[43] or '0'
		_seasonStatsCasual['cam_cs_gold_spend']                 = arg[44] or '0'
		_seasonStatsCasual['cam_cs_exp']                        = arg[45] or '0'
		_seasonStatsCasual['cam_cs_actions']                    = arg[46] or '0'
		_seasonStatsCasual['cam_cs_secs']                       = arg[47] or '0'
		_seasonStatsCasual['cam_cs_consumables']                = arg[48] or '0'
		_seasonStatsCasual['cam_cs_wards']                      = arg[49] or '0'
		_seasonStatsCasual['cam_cs_em_played']                  = arg[50] or '0'
		_seasonStatsCasual['maxXP']                             = arg[51] or '0'
		_seasonStatsCasual['last_activity']                     = arg[52] or '0'
		_seasonStatsCasual['matchIds']                          = arg[53] or '0'
		_seasonStatsCasual['matchDates']                        = arg[54] or '0'
		_seasonStatsCasual['favHero1']                          = arg[55] or '0'
		_seasonStatsCasual['favHero2']                          = arg[56] or '0'
		_seasonStatsCasual['favHero3']                          = arg[57] or '0'
		_seasonStatsCasual['favHero4']                          = arg[58] or '0'
		_seasonStatsCasual['favHero5']                          = arg[59] or '0'
		_seasonStatsCasual['favHero1Time']                      = arg[60] or '0'
		_seasonStatsCasual['favHero2Time']                      = arg[61] or '0'
		_seasonStatsCasual['favHero3Time']                      = arg[62] or '0'
		_seasonStatsCasual['favHero4Time']                      = arg[63] or '0'
		_seasonStatsCasual['favHero5Time']                      = arg[64] or '0'
		_seasonStatsCasual['xp2nextLevel']                      = arg[65] or '0'
		_seasonStatsCasual['xpPercent']                         = arg[66] or '0'
		_seasonStatsCasual['percentEM']                         = arg[67] or '0'
		_seasonStatsCasual['k_d_a']                             = arg[68] or '0'
		_seasonStatsCasual['avgGameLength']                     = arg[69] or '0'
		_seasonStatsCasual['avgXP_min']                         = arg[70] or '0'
		_seasonStatsCasual['avgDenies']                         = arg[71] or '0'
		_seasonStatsCasual['avgCreepKills']                     = arg[72] or '0'
		_seasonStatsCasual['avgNeutralKills']                   = arg[73] or '0'
		_seasonStatsCasual['avgActions_min']                    = arg[74] or '0'
		_seasonStatsCasual['avgWardsUsed']                      = arg[75] or '0'
		_seasonStatsCasual['create_date']                       = arg[76] or '0'
		_seasonStatsCasual['favHero1_2']                        = arg[77] or '0'
		_seasonStatsCasual['favHero2_2']                        = arg[78] or '0'
		_seasonStatsCasual['favHero3_2']                        = arg[79] or '0'
		_seasonStatsCasual['favHero4_2']                        = arg[80] or '0'
		_seasonStatsCasual['favHero5_2']                        = arg[81] or '0'
		_seasonStatsCasual['favHero1id']                        = arg[82] or '0'
		_seasonStatsCasual['favHero2id']                        = arg[83] or '0'
		_seasonStatsCasual['favHero3id']                        = arg[84] or '0'
		_seasonStatsCasual['favHero4id']                        = arg[85] or '0'
		_seasonStatsCasual['favHero5id']                        = arg[86] or '0'
		_seasonStatsCasual['error']                             = arg[87] or '0'
		_seasonStatsCasual['cam_cs_level']                      = arg[88] or '0'
		_seasonStatsCasual['selected_upgrades']                 = arg[89] or '0'
		_seasonStatsCasual['acc_games_played']                  = arg[90] or '0'
		_seasonStatsCasual['cs_games_played']                   = arg[91] or '0'
		_seasonStatsCasual['acc_discos']                        = arg[92] or '0'
		_seasonStatsCasual['cs_discos']                         = arg[93] or '0'
		_seasonStatsCasual['cam_cs_bloodlust']                  = arg[94] or '0'
		_seasonStatsCasual['cam_cs_doublekill']                 = arg[95] or '0'
		_seasonStatsCasual['cam_cs_triplekill']                 = arg[96] or '0'
		_seasonStatsCasual['cam_cs_quadkill']                   = arg[97] or '0'
		_seasonStatsCasual['cam_cs_annihilation']               = arg[98] or '0'
		_seasonStatsCasual['cam_cs_ks3']                        = arg[99] or '0'
		_seasonStatsCasual['cam_cs_ks4']                        = arg[100] or '0'
		_seasonStatsCasual['cam_cs_ks5']                        = arg[101] or '0'
		_seasonStatsCasual['cam_cs_ks6']                        = arg[102] or '0'
		_seasonStatsCasual['cam_cs_ks7']                        = arg[103] or '0'
		_seasonStatsCasual['cam_cs_ks8']                        = arg[104] or '0'
		_seasonStatsCasual['cam_cs_ks9']                        = arg[105] or '0'
		_seasonStatsCasual['cam_cs_ks10']                       = arg[106] or '0'
		_seasonStatsCasual['cam_cs_ks15']                       = arg[107] or '0'
		_seasonStatsCasual['cam_cs_smackdown']                  = arg[108] or '0'
		_seasonStatsCasual['cam_cs_humiliation']                = arg[109] or '0'
		_seasonStatsCasual['cam_cs_nemesis']                    = arg[110] or '0'
		_seasonStatsCasual['cam_cs_retribution']                = arg[111] or '0'
		_seasonStatsCasual['total_level_exp']                   = arg[112] or '0'
		_seasonStatsCasual['cam_cs_time_earning_exp']           = arg[113] or '0'
		_seasonStatsCasual['level']                             = arg[114] or '0'
		_seasonStatsCasual['level_exp']                         = arg[115] or '0'
		_seasonStatsCasual['discos']                            = arg[116] or '0'
		_seasonStatsCasual['possible_discos']                   = arg[117] or '0'
		_seasonStatsCasual['games_played']                      = arg[118] or '0'
		_seasonStatsCasual['account_type']                      = arg[119] or '0'
		_seasonStatsCasual['standing']                          = arg[120] or '0'
		_seasonStatsCasual['level_percent']                     = arg[121] or '0'
		_seasonStatsCasual['max_exp']                           = arg[122] or '0'
		_seasonStatsCasual['min_exp']                           = arg[123] or '0'
		_seasonStatsCasual['mid_games_played']                  = arg[124] or '0'
		_seasonStatsCasual['mid_discos']                        = arg[125] or '0'
		_seasonStatsCasual['total_games_played']                = arg[126] or '0'
		_seasonStatsCasual['total_discos']                      = arg[127] or '0'
		_seasonStatsCasual['event_id']                          = arg[128] or '0'
		_seasonStatsCasual['events']                            = arg[129] or '0'
		_seasonStatsCasual['uncs_discos']                       = arg[130] or '0'
		_seasonStatsCasual['unrnk_discos']                      = arg[131] or '0'
		_seasonStatsCasual['uncs_games_played']                 = arg[132] or '0'
		_seasonStatsCasual['unrnk_games_played']                = arg[133] or '0'
		_seasonStatsCasual['rift_games_played']                 = arg[134] or '0'
		_seasonStatsCasual['rift_discos']                       = arg[135] or '0'
		_seasonStatsCasual['current_level']                     = arg[136] or '0'
		_seasonStatsCasual['highest_level_current']             = arg[137] or '0'
		_seasonStatsCasual['season_id']                         = arg[138] or '0'
		_seasonStatsCasual['current_ranking']                   = arg[139] or '0'
		_seasonStatsCasual['curr_season_cam_games_played']   	= arg[140] or '0'
		_seasonStatsCasual['rnk_games_played']                  = arg[141] or '0'
		_seasonStatsCasual['curr_season_cam_discos']            = arg[142] or '0'
		_seasonStatsCasual['rnk_discos']                        = arg[143] or '0'
		_seasonStatsCasual['prev_seasons_cam_games_played']     = arg[144] or '0'
		_seasonStatsCasual['prev_seasons_cam_cs_games_played']  = arg[145] or '0'
		_seasonStatsCasual['prev_seasons_cam_discos']           = arg[146] or '0'
		_seasonStatsCasual['prev_seasons_cam_cs_discos']        = arg[147] or '0'
		_seasonStatsCasual['highest_ranking']					= arg[148] or '0'
			_seasonStatsCasual['con_reward']						= arg[149] or ''

			bCasualSeasonStatsRetrieved = true
		end
	end
	
	local generalInfo = {}
	generalInfo.nickname 			= _seasonStatsCasual['nickname'] or ''
	generalInfo.clanname			= _seasonStatsCasual['name'] or ''	
	generalInfo.accountid 			= tonumber(_seasonStatsCasual['account_id']) or -1
	generalInfo.lastmatchdate 		= _seasonStatsCasual['last_activity'] or ''
	generalInfo.createdate 			= _seasonStatsCasual['create_date'] or ''
	generalInfo.selected_upgrade 	= _seasonStatsCasual['selected_upgrades'] or ''
	generalInfo.level 				= _seasonStatsCasual['level'] or ''
	generalInfo.level_exp 			= tonumber(_seasonStatsCasual['level_exp'])or 0
	generalInfo.matches 			= tonumber(_seasonStatsCasual['total_games_played'])or 0
	generalInfo.disconnects 		= tonumber(_seasonStatsCasual['total_discos'])or 0
	generalInfo.standing			= tonumber(_seasonStatsCasual['standing']) or 0

	Player_Stats_V2:SetGeneralInfo(generalInfo)

	local statsInfo = {}
	if true then
		statsInfo.account_id					= _seasonStatsCasual['account_id']           
		statsInfo.season_id 					= _seasonStatsCasual['season_id']
		statsInfo.mmr							= _seasonStatsCasual['cam_cs_amm_team_rating']
		statsInfo.current_level 				= _seasonStatsCasual['current_level']
		statsInfo.level_exp 					= _seasonStatsCasual['level_exp']
		statsInfo.level_percent					= _seasonStatsCasual['level_percent']
		statsInfo.current_ranking				= _seasonStatsCasual['current_ranking']
		statsInfo.highest_level_current			= _seasonStatsCasual['highest_level_current']
		statsInfo.highest_ranking				= _seasonStatsCasual['highest_ranking']
		statsInfo.favHero1						= _seasonStatsCasual['favHero1']
		statsInfo.favHero2						= _seasonStatsCasual['favHero2']
		statsInfo.favHero3						= _seasonStatsCasual['favHero3']
		statsInfo.favHero4						= _seasonStatsCasual['favHero4']
		statsInfo.favHero5						= _seasonStatsCasual['favHero5']        
		statsInfo.favHero1Time					= _seasonStatsCasual['favHero1Time']
		statsInfo.favHero2Time					= _seasonStatsCasual['favHero2Time']
		statsInfo.favHero3Time					= _seasonStatsCasual['favHero3Time']
		statsInfo.favHero4Time					= _seasonStatsCasual['favHero4Time']
		statsInfo.favHero5Time					= _seasonStatsCasual['favHero5Time']
		statsInfo.favHero1_2					= _seasonStatsCasual['favHero1_2']
		statsInfo.favHero2_2					= _seasonStatsCasual['favHero2_2']
		statsInfo.favHero3_2					= _seasonStatsCasual['favHero3_2']
		statsInfo.favHero4_2					= _seasonStatsCasual['favHero4_2']
		statsInfo.favHero5_2					= _seasonStatsCasual['favHero5_2']
		statsInfo.total_games_played			= _seasonStatsCasual['total_games_played']
		statsInfo.cur_games_played				= _seasonStatsCasual['curr_season_cam_cs_games_played']
		statsInfo.total_discos                  = _seasonStatsCasual['total_discos']
		statsInfo.cur_discos		    		= _seasonStatsCasual['curr_season_cam_cs_discos']
		statsInfo.avgCreepKills                 = _seasonStatsCasual['avgCreepKills']
		statsInfo.avgDenies                     = _seasonStatsCasual['avgDenies']
		statsInfo.avgGameLength                 = _seasonStatsCasual['avgGameLength']
		statsInfo.avgXP_min                     = _seasonStatsCasual['avgXP_min']
		statsInfo.avgActions_min                = _seasonStatsCasual['avgActions_min']
		statsInfo.avgNeutralKills               = _seasonStatsCasual['avgNeutralKills']
		statsInfo.avgWardsUsed                  = _seasonStatsCasual['avgWardsUsed']
		statsInfo.k_d_a                         = _seasonStatsCasual['k_d_a']
		statsInfo.wins							= _seasonStatsCasual['cam_cs_wins']
		statsInfo.losses						= _seasonStatsCasual['cam_cs_losses']
		statsInfo.herokills                    	= _seasonStatsCasual['cam_cs_herokills']
		statsInfo.deaths                       	= _seasonStatsCasual['cam_cs_deaths']
		statsInfo.heroassists                  	= _seasonStatsCasual['cam_cs_heroassists']
		statsInfo.exp                          	= _seasonStatsCasual['cam_cs_exp']
		statsInfo.gold                         	= _seasonStatsCasual['cam_cs_gold']
		statsInfo.secs                         	= _seasonStatsCasual['cam_cs_secs']
		statsInfo.smackdown                    	= _seasonStatsCasual['cam_cs_smackdown']
		statsInfo.humiliation                  	= _seasonStatsCasual['cam_cs_humiliation']
		statsInfo.ks3                          	= _seasonStatsCasual['cam_cs_ks3']
		statsInfo.ks4                          	= _seasonStatsCasual['cam_cs_ks4']
		statsInfo.ks5                          	= _seasonStatsCasual['cam_cs_ks5']
		statsInfo.ks6                          	= _seasonStatsCasual['cam_cs_ks6']
		statsInfo.ks7                          	= _seasonStatsCasual['cam_cs_ks7']
		statsInfo.ks8                          	= _seasonStatsCasual['cam_cs_ks8']
		statsInfo.ks9                          	= _seasonStatsCasual['cam_cs_ks9']
		statsInfo.ks10                         	= _seasonStatsCasual['cam_cs_ks10']
		statsInfo.ks15                         	= _seasonStatsCasual['cam_cs_ks15']
		statsInfo.doublekill                   	= _seasonStatsCasual['cam_cs_doublekill']
		statsInfo.triplekill                   	= _seasonStatsCasual['cam_cs_triplekill']
		statsInfo.quadkill                     	= _seasonStatsCasual['cam_cs_quadkill']
		statsInfo.annihilation                 	= _seasonStatsCasual['cam_cs_annihilation']
		statsInfo.bloodlust                    	= _seasonStatsCasual['cam_cs_bloodlust']
		statsInfo.con_reward					= _seasonStatsCasual['con_reward']
	end
	
	if _currentTab == 'overview' and bHeroMasteryRetrieved then
		println('^gSetting overview season info from casual season result!^*')
		SetOverviewSeasonInfo(statsInfo, false)
	elseif _currentTab == 'stats' then
		SetPlayerStatsStatsInfo(statsInfo, false, true, false)
	
		local tip_colors = {}
		local tip_numbers = {}
		tip_colors.public = '^w'
		tip_colors.normal = '^w'
		tip_colors.casual = '^w'
		tip_colors.season_normal = '^w'
		tip_colors.season_casual = '^r'
		tip_colors.pre_season_normal = '^w'
		tip_colors.pre_season_casual = '^w'
		tip_colors.midwars = '^w'
		
		tip_numbers.public = _seasonStatsCasual['acc_games_played']
		tip_numbers.normal = _seasonStatsCasual['rnk_games_played']
		tip_numbers.casual = _seasonStatsCasual['cs_games_played']
		tip_numbers.season_normal = _seasonStatsCasual['curr_season_cam_games_played']
		tip_numbers.season_casual = _seasonStatsCasual['curr_season_cam_cs_games_played']
		tip_numbers.pre_season_normal = _seasonStatsCasual['prev_seasons_cam_games_played']
		tip_numbers.pre_season_casual = _seasonStatsCasual['prev_seasons_cam_cs_games_played']
		tip_numbers.midwars = _seasonStatsCasual['mid_games_played']
		SetTipMatchesPlayed(tip_colors, tip_numbers)
		
		tip_numbers.public = _seasonStatsCasual['acc_discos']
		tip_numbers.normal = _seasonStatsCasual['rnk_discos']
		tip_numbers.casual = _seasonStatsCasual['cs_discos']
		tip_numbers.season_normal = _seasonStatsCasual['curr_season_cam_discos']
		tip_numbers.season_casual = _seasonStatsCasual['curr_season_cam_cs_discos']
		tip_numbers.pre_season_normal = _seasonStatsCasual['prev_seasons_cam_discos']
		tip_numbers.pre_season_casual = _seasonStatsCasual['prev_seasons_cam_cs_discos']
		tip_numbers.midwars = _seasonStatsCasual['mid_discos']
		SetTipDisconnects(tip_colors, tip_numbers)
	else
	end
end

function Player_Stats_V2:OnPlayerStatsPublicResult(...)
	local arg = { ... }
	arg.n = select('#', ...)
	-- Logan (2026-02-12): Fix for Modern Server Response (Table vs Positional)
	-- Conversion Helper - REMOVED (Use SafeGet)
	if type(arg[1]) == 'userdata' then arg = arg[1] end

	local function SafeGet(src, key)
		if type(src) == 'userdata' then
			local s, v = pcall(function() return src[key] end)
			if s then return v end
		elseif type(src) == 'table' then
			return src[key]
		end
		return nil
	end


	-- Reconstruct flat Key-Value unrolls if the C++ Engine stripped the userdata wrapper
	if type(arg[1]) == 'string' and arg.n and arg.n > 20 then
		-- Detect if this is a flattened dictionary: key1, val1, key2, val2...
		local isFlatDict = false
		for i = 1, math.min(arg.n, 10), 2 do
			if type(arg[i]) == 'string' and (arg[i] == 'account_id' or arg[i] == 'nickname' or arg[i] == 'error_code') then
				isFlatDict = true
				break
			end
		end

		if isFlatDict then
			local rebuilt = {}
			for i = 1, arg.n, 2 do
				local k = arg[i]
				local v = arg[i+1]
				if k ~= nil then
					rebuilt[k] = v
				end
			end
			arg = rebuilt
			println('^y[PlayerStats] Successfully rebuilt Public variadic flattened Dictionary!^*')
		end
	end

	if not bPublicStatsRetrieved then
		-- Modern Server Support (Hashtable Response)
		local nickname = SafeGet(arg, 'nickname')
		if nickname then
			println('^gOnPlayerStatsPublicResult: Using Named Keys from Server Response.')
			
			-- Create new table to avoid userdata issues
			_publicStats = {}
			for k,v in pairs(arg) do _publicStats[k] = v end
			_publicStats['nickname'] = nickname
			
			-- MAPPING: Modern Keys -> Legacy Keys (Required for UI)
			_publicStats['acc_wins'] 			= SafeGet(arg, 'wins') or '0'
			_publicStats['acc_losses'] 			= SafeGet(arg, 'losses') or '0'
			_publicStats['acc_games_played'] 	= SafeGet(arg, 'matches') or SafeGet(arg, 'total_games_played') or '0'
			_publicStats['acc_kd'] 				= SafeGet(arg, 'k_d_a') or '0.00'
			_publicStats['acc_avg_score'] 		= SafeGet(arg, 'avg_score') or '0'
			_publicStats['acc_herokills'] 		= SafeGet(arg, 'herokills') or '0'
			_publicStats['acc_heroassists'] 	= SafeGet(arg, 'heroassists') or '0'
			_publicStats['acc_deaths'] 			= SafeGet(arg, 'deaths') or '0'
			_publicStats['acc_gold'] 			= SafeGet(arg, 'gold') or '0'
			_publicStats['acc_exp'] 			= SafeGet(arg, 'exp') or '0'
			_publicStats['acc_secs'] 			= SafeGet(arg, 'secs') or '0'
			_publicStats['total_games_played']	= _publicStats['acc_games_played']
			
			-- Fav Heroes
			_publicStats['favHero1']                = SafeGet(arg, 'favHero1') or SafeGet(arg, '55') or '0'
			_publicStats['favHero2']                = SafeGet(arg, 'favHero2') or SafeGet(arg, '56') or '0'
			_publicStats['favHero3']                = SafeGet(arg, 'favHero3') or SafeGet(arg, '57') or '0'
			_publicStats['favHero4']                = SafeGet(arg, 'favHero4') or SafeGet(arg, '58') or '0'
			_publicStats['favHero5']                = SafeGet(arg, 'favHero5') or SafeGet(arg, '59') or '0'
			_publicStats['favHero1Time']            = SafeGet(arg, 'favHero1Time') or SafeGet(arg, '60') or '0'
			_publicStats['favHero2Time']            = SafeGet(arg, 'favHero2Time') or SafeGet(arg, '61') or '0'
			_publicStats['favHero3Time']            = SafeGet(arg, 'favHero3Time') or SafeGet(arg, '62') or '0'
			_publicStats['favHero4Time']            = SafeGet(arg, 'favHero4Time') or SafeGet(arg, '63') or '0'
			_publicStats['favHero5Time']            = SafeGet(arg, 'favHero5Time') or SafeGet(arg, '64') or '0'
			_publicStats['favHero1_2']              = SafeGet(arg, 'favHero1_2') or SafeGet(arg, '77') or '0'
			_publicStats['favHero2_2']              = SafeGet(arg, 'favHero2_2') or SafeGet(arg, '78') or '0'
			_publicStats['favHero3_2']              = SafeGet(arg, 'favHero3_2') or SafeGet(arg, '79') or '0'
			_publicStats['favHero4_2']              = SafeGet(arg, 'favHero4_2') or SafeGet(arg, '80') or '0'
			_publicStats['favHero5_2']              = SafeGet(arg, 'favHero5_2') or SafeGet(arg, '81') or '0'

			bPublicStatsRetrieved = true
		else
			_publicStats['nickname'] 							= arg[1] or '0'
		_publicStats['name']								= arg[2] or '0'
		_publicStats['rank']                				= arg[3] or '0'
		_publicStats['level']               				= arg[4] or '0'
		_publicStats['account_id']          				= arg[5] or '0'
		_publicStats['acc_games_played']    				= arg[6] or '0'
		_publicStats['acc_wins']            				= arg[7] or '0'
		_publicStats['acc_losses']          				= arg[8] or '0'
		_publicStats['acc_concedes']        				= arg[9] or '0'
		_publicStats['acc_concedevotes']    				= arg[10] or '0'
		_publicStats['acc_buybacks']						= arg[11] or '0'
		_publicStats['acc_discos']              			= arg[12] or '0'
		_publicStats['acc_kicked']              			= arg[13] or '0'
		_publicStats['acc_pub_skill']           			= arg[14] or '0'
		_publicStats['acc_pub_count']           			= arg[15] or '0'
		_publicStats['acc_amm_solo_rating']     			= arg[16] or '0'
		_publicStats['acc_amm_solo_count']      			= arg[17] or '0'
		_publicStats['acc_amm_team_rating']     			= arg[18] or '0'
		_publicStats['acc_amm_team_count']      			= arg[19] or '0'
		_publicStats['acc_avg_score']           			= arg[20] or '0'
		_publicStats['acc_herokills']           			= arg[21] or '0'
		_publicStats['acc_herodmg']             			= arg[22] or '0'
		_publicStats['acc_heroexp']             			= arg[23] or '0'
		_publicStats['acc_herokillsgold']       			= arg[24] or '0'
		_publicStats['acc_heroassists']         			= arg[25] or '0'
		_publicStats['acc_deaths']              			= arg[26] or '0'
		_publicStats['acc_goldlost2death']      			= arg[27] or '0'
		_publicStats['acc_secs_dead']           			= arg[28] or '0'
		_publicStats['acc_teamcreepkills']      			= arg[29] or '0'
		_publicStats['acc_teamcreepdmg']        			= arg[30] or '0'
		_publicStats['acc_teamcreepexp']        			= arg[31] or '0'
		_publicStats['acc_teamcreepgold']       			= arg[32] or '0'
		_publicStats['acc_neutralcreepkills']				= arg[33] or '0'
		_publicStats['acc_neutralcreepdmg']					= arg[34] or '0'
		_publicStats['acc_neutralcreepexp']					= arg[35] or '0'
		_publicStats['acc_neutralcreepgold']				= arg[36] or '0'
		_publicStats['acc_bdmg']							= arg[37] or '0'
		_publicStats['acc_bdmgexp']             			= arg[38] or '0'
		_publicStats['acc_razed']               			= arg[39] or '0'
		_publicStats['acc_bgold']               			= arg[40] or '0'
		_publicStats['acc_denies']              			= arg[41] or '0'
		_publicStats['acc_exp_denied']          			= arg[42] or '0'
		_publicStats['acc_gold']                			= arg[43] or '0'
		_publicStats['acc_gold_spend']          			= arg[44] or '0'
		_publicStats['acc_exp']                 			= arg[45] or '0'
		_publicStats['acc_actions']             			= arg[46] or '0'
		_publicStats['acc_secs']                			= arg[47] or '0'
		_publicStats['acc_consumables']         			= arg[48] or '0'
		_publicStats['acc_wards']               			= arg[49] or '0'
		_publicStats['acc_em_played']           			= arg[50] or '0'
		_publicStats['maxXP']                   			= arg[51] or '0'
		_publicStats['last_activity']           			= arg[52] or '0'
		_publicStats['matchIds']                			= arg[53] or '0'
		_publicStats['matchDates']              			= arg[54] or '0'
		_publicStats['favHero1']                			= arg[55] or '0'
		_publicStats['favHero2']                			= arg[56] or '0'
		_publicStats['favHero3']                			= arg[57] or '0'
		_publicStats['favHero4']                			= arg[58] or '0'
		_publicStats['favHero5']                			= arg[59] or '0'
		_publicStats['favHero1Time']            			= arg[60] or '0'
		_publicStats['favHero2Time']            			= arg[61] or '0'
		_publicStats['favHero3Time']            			= arg[62] or '0'
		_publicStats['favHero4Time']            			= arg[63] or '0'
		_publicStats['favHero5Time']            			= arg[64] or '0'
		_publicStats['xp2nextLevel']            			= arg[65] or '0'
		_publicStats['xpPercent']               			= arg[66] or '0'
		_publicStats['percentEM']               			= arg[67] or '0'
		_publicStats['k_d_a']                   			= arg[68] or '0'
		_publicStats['avgGameLength']           			= arg[69] or '0'
		_publicStats['avgXP_min']               			= arg[70] or '0'
		_publicStats['avgDenies']               			= arg[71] or '0'
		_publicStats['avgCreepKills']           			= arg[72] or '0'
		_publicStats['avgNeutralKills']         			= arg[73] or '0'
		_publicStats['avgActions_min']          			= arg[74] or '0'
		_publicStats['avgWardsUsed']            			= arg[75] or '0'
		_publicStats['create_date']             			= arg[76] or '0'
		_publicStats['favHero1_2']            				= arg[77] or '0'
		_publicStats['favHero2_2']            				= arg[78] or '0'
		_publicStats['favHero3_2']            				= arg[79] or '0'
		_publicStats['favHero4_2']            				= arg[80] or '0'
		_publicStats['favHero5_2']            				= arg[81] or '0'
		_publicStats['favHero1id']              			= arg[82] or '0'
		_publicStats['favHero2id']              			= arg[83] or '0'
		_publicStats['favHero3id']              			= arg[84] or '0'
		_publicStats['favHero4id']              			= arg[85] or '0'
		_publicStats['favHero5id']              			= arg[86] or '0'
		_publicStats['error']                   			= arg[87] or '0'
		_publicStats['acc_level']               			= arg[88] or '0'
		_publicStats['selected_upgrades']       			= arg[89] or '0'
		_publicStats['cs_games_played']         			= arg[90] or '0'
		_publicStats['rnk_games_played']        			= arg[91] or '0'
		_publicStats['cs_discos']               			= arg[92] or '0'
		_publicStats['rnk_discos']              			= arg[93] or '0'
		_publicStats['acc_bloodlust']           			= arg[94] or '0'
		_publicStats['acc_doublekill']          			= arg[95] or '0'
		_publicStats['acc_triplekill']						= arg[96] or '0'
		_publicStats['acc_quadkill']            			= arg[97] or '0'
		_publicStats['acc_annihilation']        			= arg[98] or '0'
		_publicStats['acc_ks3']                 			= arg[99] or '0'
		_publicStats['acc_ks4']                 			= arg[100] or '0'
		_publicStats['acc_ks5']								= arg[101] or '0'
		_publicStats['acc_ks6']                 			= arg[102] or '0'
		_publicStats['acc_ks7']                 			= arg[103] or '0'
		_publicStats['acc_ks8']                 			= arg[104] or '0'
		_publicStats['acc_ks9']                 			= arg[105] or '0'
		_publicStats['acc_ks10']                			= arg[106] or '0'
		_publicStats['acc_ks15']                			= arg[107] or '0'
		_publicStats['acc_smackdown']           			= arg[108] or '0'
		_publicStats['acc_humiliation']         			= arg[109] or '0'
		_publicStats['acc_nemesis']             			= arg[110] or '0'
		_publicStats['acc_retribution']         			= arg[111] or '0'
		_publicStats['total_level_exp']						= arg[112] or '0'
		_publicStats['acc_time_earning_exp']                = arg[113] or '0'
		_publicStats['level']                               = arg[114] or '0'
		_publicStats['level_exp']                           = arg[115] or '0'
		_publicStats['discos']                              = arg[116] or '0'
		_publicStats['possible_discos']                     = arg[117] or '0'
		_publicStats['games_played']                        = arg[118] or '0'
		_publicStats['account_type']                        = arg[119] or '0'
		_publicStats['standing']                            = arg[120] or '0'
		_publicStats['level_percent']                       = arg[121] or '0'
		_publicStats['max_exp']                             = arg[122] or '0'
		_publicStats['min_exp']                             = arg[123] or '0'
		_publicStats['mid_games_played']                    = arg[124] or '0'
		_publicStats['mid_discos']                          = arg[125] or '0'
		_publicStats['total_games_played']                  = arg[126] or '0'
		_publicStats['total_discos']                        = arg[127] or '0'
		_publicStats['event_id']                            = arg[128] or '0'
		_publicStats['events']                              = arg[129] or '0'
		_publicStats['uncs_discos']                         = arg[130] or '0'
		_publicStats['unrnk_discos']                        = arg[131] or '0'
		_publicStats['uncs_games_played']                   = arg[132] or '0'
		_publicStats['unrnk_games_played']                  = arg[133] or '0'
		_publicStats['rift_games_played']                   = arg[134] or '0'
		_publicStats['rift_discos']                         = arg[135] or '0'
		_publicStats['current_level']                       = arg[136] or '0'
		_publicStats['highest_level_current']               = arg[137] or '0'
		_publicStats['season_id']                           = arg[138] or '0'
		_publicStats['current_ranking']                     = arg[139] or '0'
		_publicStats['curr_season_cam_games_played']        = arg[140] or '0'
		_publicStats['curr_season_cam_cs_games_played']     = arg[141] or '0'
		_publicStats['curr_season_cam_discos']              = arg[142] or '0'
		_publicStats['curr_season_cam_cs_discos']           = arg[143] or '0'
		_publicStats['prev_seasons_cam_games_played']       = arg[144] or '0'
		_publicStats['prev_seasons_cam_cs_games_played']	= arg[145] or '0'
		_publicStats['prev_seasons_cam_discos']             = arg[146] or '0'
		_publicStats['prev_seasons_cam_cs_discos']          = arg[147] or '0'
			_publicStats['quest_stats']					        = arg[148] or ''

			bPublicStatsRetrieved = true
		end
	end
	
	local statsInfo = {}
	if true then
		statsInfo.account_id					= _publicStats['account_id']           
		statsInfo.season_id 					= nil
		statsInfo.mmr							= _publicStats['acc_pub_skill']
		statsInfo.current_level 				= nil
		statsInfo.level_exp 					= nil
		statsInfo.current_ranking				= nil
		statsInfo.highest_level_current			= nil
		statsInfo.favHero1						= _publicStats['favHero1']
		statsInfo.favHero2						= _publicStats['favHero2']
		statsInfo.favHero3						= _publicStats['favHero3']
		statsInfo.favHero4						= _publicStats['favHero4']
		statsInfo.favHero5						= _publicStats['favHero5']        
		statsInfo.favHero1Time					= _publicStats['favHero1Time']
		statsInfo.favHero2Time					= _publicStats['favHero2Time']
		statsInfo.favHero3Time					= _publicStats['favHero3Time']
		statsInfo.favHero4Time					= _publicStats['favHero4Time']
		statsInfo.favHero5Time					= _publicStats['favHero5Time']
		statsInfo.favHero1_2					= _publicStats['favHero1_2']
		statsInfo.favHero2_2					= _publicStats['favHero2_2']
		statsInfo.favHero3_2					= _publicStats['favHero3_2']
		statsInfo.favHero4_2					= _publicStats['favHero4_2']
		statsInfo.favHero5_2					= _publicStats['favHero5_2']
		statsInfo.total_games_played			= _publicStats['total_games_played']
		statsInfo.cur_games_played		     	= _publicStats['acc_games_played']
		statsInfo.total_discos                  = _publicStats['total_discos']
		statsInfo.cur_discos           			= _publicStats['acc_discos']
		statsInfo.avgCreepKills                 = _publicStats['avgCreepKills']
		statsInfo.avgDenies                     = _publicStats['avgDenies']
		statsInfo.avgGameLength                 = _publicStats['avgGameLength']
		statsInfo.avgXP_min                     = _publicStats['avgXP_min']
		statsInfo.avgActions_min                = _publicStats['avgActions_min']
		statsInfo.avgNeutralKills               = _publicStats['avgNeutralKills']
		statsInfo.avgWardsUsed                  = _publicStats['avgWardsUsed']
		statsInfo.k_d_a                         = _publicStats['k_d_a']
		statsInfo.wins							= _publicStats['acc_wins']
		statsInfo.losses						= _publicStats['acc_losses']
		statsInfo.herokills                    	= _publicStats['acc_herokills']
		statsInfo.deaths                       	= _publicStats['acc_deaths']
		statsInfo.heroassists                  	= _publicStats['acc_heroassists']
		statsInfo.exp                          	= _publicStats['acc_exp']
		statsInfo.gold                         	= _publicStats['acc_gold']
		statsInfo.secs                         	= _publicStats['acc_secs']
		statsInfo.smackdown                    	= _publicStats['acc_smackdown']
		statsInfo.humiliation                  	= _publicStats['acc_humiliation']
		statsInfo.ks3                          	= _publicStats['acc_ks3']
		statsInfo.ks4                          	= _publicStats['acc_ks4']
		statsInfo.ks5                          	= _publicStats['acc_ks5']
		statsInfo.ks6                          	= _publicStats['acc_ks6']
		statsInfo.ks7                          	= _publicStats['acc_ks7']
		statsInfo.ks8                          	= _publicStats['acc_ks8']
		statsInfo.ks9                          	= _publicStats['acc_ks9']
		statsInfo.ks10                         	= _publicStats['acc_ks10']
		statsInfo.ks15                         	= _publicStats['acc_ks15']
		statsInfo.doublekill                   	= _publicStats['acc_doublekill']
		statsInfo.triplekill                   	= _publicStats['acc_triplekill']
		statsInfo.quadkill                     	= _publicStats['acc_quadkill']
		statsInfo.annihilation                 	= _publicStats['acc_annihilation']
		statsInfo.bloodlust                    	= _publicStats['acc_bloodlust']
		statsInfo.quest_stats					= _publicStats['quest_stats']
	end

	if _currentTab == 'stats' then
		SetPlayerStatsStatsInfo(statsInfo, true, false, true)
	
		local tip_colors = {}
		local tip_numbers = {}
		tip_colors.public = '^r'
		tip_colors.normal = '^w'
		tip_colors.casual = '^w'
		tip_colors.season_normal = '^w'
		tip_colors.season_casual = '^w'
		tip_colors.pre_season_normal = '^w'
		tip_colors.pre_season_casual = '^w'
		tip_colors.midwars = '^w'
		
		tip_numbers.public = _publicStats['acc_games_played']
		tip_numbers.normal = _publicStats['rnk_games_played']
		tip_numbers.casual = _publicStats['cs_games_played']
		tip_numbers.season_normal = _publicStats['curr_season_cam_games_played']
		tip_numbers.season_casual = _publicStats['curr_season_cam_cs_games_played']
		tip_numbers.pre_season_normal = _publicStats['prev_seasons_cam_games_played']
		tip_numbers.pre_season_casual = _publicStats['prev_seasons_cam_cs_games_played']
		tip_numbers.midwars = _publicStats['mid_games_played']
		SetTipMatchesPlayed(tip_colors, tip_numbers)
		
		tip_numbers.public = _publicStats['acc_discos']
		tip_numbers.normal = _publicStats['rnk_discos']
		tip_numbers.casual = _publicStats['cs_discos']
		tip_numbers.season_normal = _publicStats['curr_season_cam_discos']
		tip_numbers.season_casual = _publicStats['curr_season_cam_cs_discos']
		tip_numbers.pre_season_normal = _publicStats['prev_seasons_cam_discos']
		tip_numbers.pre_season_casual = _publicStats['prev_seasons_cam_cs_discos']
		tip_numbers.midwars = _publicStats['mid_discos']
		SetTipDisconnects(tip_colors, tip_numbers)
	else
	end
end

function Player_Stats_V2:OnPlayerStatsNormalResult(...)
	local arg = { ... }
	arg.n = select('#', ...)
	-- Logan (2026-02-12): Fix for Modern Server Response (Table vs Positional)
	-- Conversion Helper - REMOVED (Use SafeGet)
	if type(arg[1]) == 'userdata' or type(arg[1]) == 'table' then arg = arg[1] end

	local function SafeGet(src, key)
		if type(src) == 'userdata' then
			local s, v = pcall(function() return src[key] end)
			if s then return v end
		elseif type(src) == 'table' then
			return src[key]
		end
		return nil
	end


	-- Reconstruct flat Key-Value unrolls if the C++ Engine stripped the userdata wrapper
	if type(arg[1]) == 'string' and arg.n and arg.n > 20 then
		-- Detect if this is a flattened dictionary: key1, val1, key2, val2...
		local isFlatDict = false
		for i = 1, math.min(arg.n, 10), 2 do
			if type(arg[i]) == 'string' and (arg[i] == 'account_id' or arg[i] == 'nickname' or arg[i] == 'error_code') then
				isFlatDict = true
				break
			end
		end

		if isFlatDict then
			local rebuilt = {}
			for i = 1, arg.n, 2 do
				local k = arg[i]
				local v = arg[i+1]
				if k ~= nil then
					rebuilt[k] = v
				end
			end
			arg = rebuilt
			println('^y[PlayerStats] Successfully rebuilt variadic flattened Dictionary!^*')
		end
	end

	if not bNormalStatsRetrieved then
		-- Modern Server Support (Hashtable Response)
		local nickname = SafeGet(arg, 'nickname')
		if nickname then
			println('^gOnPlayerStatsNormalResult: Using Named Keys from Server Response.')
			
			_normalStats = {}
			for k,v in pairs(arg) do _normalStats[k] = v end
			_normalStats['nickname'] = nickname
			_normalStats['name']     = nickname
			_normalStats['accountid'] = SafeGet(arg, 'account_id') or '0'
			_normalStats['standing']  = tonumber(SafeGet(arg, 'standing')) or 0
			_normalStats['selected_upgrade'] = SafeGet(arg, 'selected_upgrade') or ''
			_normalStats['level']     = SafeGet(arg, 'current_level') or '1'
			_normalStats['level_exp'] = SafeGet(arg, 'rnk_exp') or '0'
			_normalStats['matches']   = tonumber(SafeGet(arg, 'rnk_games_played')) or 0
			_normalStats['disconnects'] = tonumber(SafeGet(arg, 'rnk_discos')) or 0
			_normalStats['wins']      = SafeGet(arg, 'rnk_wins') or '0'
			_normalStats['losses']    = SafeGet(arg, 'rnk_losses') or '0'
			
			self:SetGeneralInfo(_normalStats)
			SetOverviewSeasonInfo(_normalStats, true)
			
			-- MAPPING: Modern Keys -> Legacy Keys
			_normalStats['rnk_wins'] 			= SafeGet(arg, 'wins') or SafeGet(arg, 'rnk_wins') or '0'
			_normalStats['rnk_losses'] 			= SafeGet(arg, 'losses') or SafeGet(arg, 'rnk_losses') or '0'
			_normalStats['rnk_games_played'] 	= SafeGet(arg, 'matches') or SafeGet(arg, 'total_games_played') or SafeGet(arg, 'rnk_games_played') or '0'
			_normalStats['k_d_a'] 				= SafeGet(arg, 'k_d_a') or '0.00'
			_normalStats['rnk_avg_score'] 		= SafeGet(arg, 'avg_score') or '0'
			_normalStats['rnk_herokills'] 		= SafeGet(arg, 'herokills') or '0'
			_normalStats['rnk_heroassists'] 	= SafeGet(arg, 'heroassists') or '0'
			_normalStats['rnk_deaths'] 			= SafeGet(arg, 'deaths') or '0'
			_normalStats['rnk_gold'] 			= SafeGet(arg, 'gold') or '0'
			_normalStats['rnk_exp'] 			= SafeGet(arg, 'exp') or '0'
			_normalStats['rnk_secs'] 			= SafeGet(arg, 'secs') or '0'
			_normalStats['total_games_played']	= _normalStats['rnk_games_played']

			_normalStats['rnk_level']           = SafeGet(arg, 'level') or SafeGet(arg, 'rnk_level') or '0'
			
			-- Modern Keys -> Legacy Keys (Missing Statistics Tab bindings for Averages and Streaks)
			_normalStats['avgGameLength']       = SafeGet(arg, 'avgGameLength') or '0'
			_normalStats['avgXP_min']           = SafeGet(arg, 'avgXP_min') or '0'
			_normalStats['avgDenies']           = SafeGet(arg, 'avgDenies') or '0'
			_normalStats['avgCreepKills']       = SafeGet(arg, 'avgCreepKills') or '0'
			_normalStats['avgNeutralKills']     = SafeGet(arg, 'avgNeutralKills') or '0'
			_normalStats['avgActions_min']      = SafeGet(arg, 'avgActions_min') or '0'
			_normalStats['avgWardsUsed']        = SafeGet(arg, 'avgWardsUsed') or '0'
			_normalStats['rnk_smackdown']       = SafeGet(arg, 'smackdown') or '0'
			_normalStats['rnk_humiliation']     = SafeGet(arg, 'humiliation') or '0'
			_normalStats['rnk_nemesis']         = SafeGet(arg, 'nemesis') or '0'
			_normalStats['rnk_retribution']     = SafeGet(arg, 'retribution') or '0'
			
			-- Fav Heroes
			_normalStats['favHero1']                          = SafeGet(arg, 'favHero1') or SafeGet(arg, '55') or '0'
			_normalStats['favHero2']                          = SafeGet(arg, 'favHero2') or SafeGet(arg, '56') or '0'
			_normalStats['favHero3']                          = SafeGet(arg, 'favHero3') or SafeGet(arg, '57') or '0'
			_normalStats['favHero4']                          = SafeGet(arg, 'favHero4') or SafeGet(arg, '58') or '0'
			_normalStats['favHero5']                          = SafeGet(arg, 'favHero5') or SafeGet(arg, '59') or '0'
			_normalStats['favHero1Time']                      = SafeGet(arg, 'favHero1Time') or SafeGet(arg, '60') or '0'
			_normalStats['favHero2Time']                      = SafeGet(arg, 'favHero2Time') or SafeGet(arg, '61') or '0'
			_normalStats['favHero3Time']                      = SafeGet(arg, 'favHero3Time') or SafeGet(arg, '62') or '0'
			_normalStats['favHero4Time']                      = SafeGet(arg, 'favHero4Time') or SafeGet(arg, '63') or '0'
			_normalStats['favHero5Time']                      = SafeGet(arg, 'favHero5Time') or SafeGet(arg, '64') or '0'
			_normalStats['favHero1_2']                        = SafeGet(arg, 'favHero1_2') or SafeGet(arg, '77') or '0'
			_normalStats['favHero2_2']                        = SafeGet(arg, 'favHero2_2') or SafeGet(arg, '78') or '0'
			_normalStats['favHero3_2']                        = SafeGet(arg, 'favHero3_2') or SafeGet(arg, '79') or '0'
			_normalStats['favHero4_2']                        = SafeGet(arg, 'favHero4_2') or SafeGet(arg, '80') or '0'
			_normalStats['favHero5_2']                        = SafeGet(arg, 'favHero5_2') or SafeGet(arg, '81') or '0'

			bNormalStatsRetrieved = true
		else
			_normalStats['nickname']		        		  = arg[1] or '0'
		_normalStats['name']                              = arg[2] or '0'
		_normalStats['rank']                              = arg[3] or '0'
		_normalStats['rnk_level']                         = arg[4] or '0'
		_normalStats['account_id']                        = arg[5] or '0'
		_normalStats['rnk_games_played']      			  = arg[6] or '0'
		_normalStats['rnk_wins']                          = arg[7] or '0'
		_normalStats['rnk_losses']                        = arg[8] or '0'
		_normalStats['rnk_concedes']                      = arg[9] or '0'
		_normalStats['rnk_concedevotes']                  = arg[10] or '0'
		_normalStats['rnk_buybacks']                      = arg[11] or '0'
		_normalStats['rnk_discos']            			  = arg[12] or '0'
		_normalStats['rnk_kicked']                        = arg[13] or '0'
		_normalStats['smr']                               = arg[14] or '0'
		_normalStats['rnk_pub_count']                     = arg[15] or '0'
		_normalStats['rnk_amm_solo_rating']               = arg[16] or '0'
		_normalStats['rnk_amm_solo_count']                = arg[17] or '0'
		_normalStats['rnk_amm_team_rating']               = arg[18] or '0'
		_normalStats['rnk_amm_team_count']                = arg[19] or '0'
		_normalStats['rnk_avg_score']                     = arg[20] or '0'
		_normalStats['rnk_herokills']                     = arg[21] or '0'
		_normalStats['rnk_herodmg']                       = arg[22] or '0'
		_normalStats['rnk_heroexp']                       = arg[23] or '0'
		_normalStats['rnk_herokillsgold']                 = arg[24] or '0'
		_normalStats['rnk_heroassists']                   = arg[25] or '0'
		_normalStats['rnk_deaths']                        = arg[26] or '0'
		_normalStats['rnk_goldlost2death']                = arg[27] or '0'
		_normalStats['rnk_secs_dead']                     = arg[28] or '0'
		_normalStats['rnk_teamcreepkills']                = arg[29] or '0'
		_normalStats['rnk_teamcreepdmg']                  = arg[30] or '0'
		_normalStats['rnk_teamcreepexp']                  = arg[31] or '0'
		_normalStats['rnk_teamcreepgold']                 = arg[32] or '0'
		_normalStats['rnk_neutralcreepkills']             = arg[33] or '0'
		_normalStats['rnk_neutralcreepdmg']               = arg[34] or '0'
		_normalStats['rnk_neutralcreepexp']               = arg[35] or '0'
		_normalStats['rnk_neutralcreepgold']              = arg[36] or '0'
		_normalStats['rnk_bdmg']                          = arg[37] or '0'
		_normalStats['rnk_bdmgexp']                       = arg[38] or '0'
		_normalStats['rnk_razed']                         = arg[39] or '0'
		_normalStats['rnk_bgold']                         = arg[40] or '0'
		_normalStats['rnk_denies']                        = arg[41] or '0'
		_normalStats['rnk_exp_denied']                    = arg[42] or '0'
		_normalStats['rnk_gold']                          = arg[43] or '0'
		_normalStats['rnk_gold_spend']                    = arg[44] or '0'
		_normalStats['rnk_exp']                           = arg[45] or '0'
		_normalStats['rnk_actions']                       = arg[46] or '0'
		_normalStats['rnk_secs']                          = arg[47] or '0'
		_normalStats['rnk_consumables']                   = arg[48] or '0'
		_normalStats['rnk_wards']                         = arg[49] or '0'
		_normalStats['rnk_em_played']                     = arg[50] or '0'
		_normalStats['maxXP']                             = arg[51] or '0'
		_normalStats['last_activity']                     = arg[52] or '0'
		_normalStats['matchIds']                          = arg[53] or '0'
		_normalStats['matchDates']                        = arg[54] or '0'
		_normalStats['favHero1']                          = arg[55] or '0'
		_normalStats['favHero2']                          = arg[56] or '0'
		_normalStats['favHero3']                          = arg[57] or '0'
		_normalStats['favHero4']                          = arg[58] or '0'
		_normalStats['favHero5']                          = arg[59] or '0'
		_normalStats['favHero1Time']                      = arg[60] or '0'
		_normalStats['favHero2Time']                      = arg[61] or '0'
		_normalStats['favHero3Time']                      = arg[62] or '0'
		_normalStats['favHero4Time']                      = arg[63] or '0'
		_normalStats['favHero5Time']                      = arg[64] or '0'
		_normalStats['xp2nextLevel']                      = arg[65] or '0'
		_normalStats['xpPercent']                         = arg[66] or '0'
		_normalStats['percentEM']                         = arg[67] or '0'
		_normalStats['k_d_a']                             = arg[68] or '0'
		_normalStats['avgGameLength']                     = arg[69] or '0'
		_normalStats['avgXP_min']                         = arg[70] or '0'
		_normalStats['avgDenies']                         = arg[71] or '0'
		_normalStats['avgCreepKills']                     = arg[72] or '0'
		_normalStats['avgNeutralKills']                   = arg[73] or '0'
		_normalStats['avgActions_min']                    = arg[74] or '0'
		_normalStats['avgWardsUsed']                      = arg[75] or '0'
		_normalStats['create_date']                       = arg[76] or '0'
		_normalStats['favHero1_2']                        = arg[77] or '0'
		_normalStats['favHero2_2']                        = arg[78] or '0'
		_normalStats['favHero3_2']                        = arg[79] or '0'
		_normalStats['favHero4_2']                        = arg[80] or '0'
		_normalStats['favHero5_2']                        = arg[81] or '0'
		_normalStats['favHero1id']                        = arg[82] or '0'
		_normalStats['favHero2id']                        = arg[83] or '0'
		_normalStats['favHero3id']                        = arg[84] or '0'
		_normalStats['favHero4id']                        = arg[85] or '0'
		_normalStats['favHero5id']                        = arg[86] or '0'
		_normalStats['error']                             = arg[87] or '0'
		_normalStats['rnk_level']                         = arg[88] or '0'
		_normalStats['selected_upgrades']                 = arg[89] or '0'
		_normalStats['acc_games_played']                  = arg[90] or '0'
		_normalStats['cs_games_played']                   = arg[91] or '0'
		_normalStats['acc_discos']                         = arg[92] or '0'
		_normalStats['cs_discos']                        = arg[93] or '0'
		_normalStats['rnk_bloodlust']                     = arg[94] or '0'
		_normalStats['rnk_doublekill']                    = arg[95] or '0'
		_normalStats['rnk_triplekill']                    = arg[96] or '0'
		_normalStats['rnk_quadkill']                      = arg[97] or '0'
		_normalStats['rnk_annihilation']                  = arg[98] or '0'
		_normalStats['rnk_ks3']                           = arg[99] or '0'
		_normalStats['rnk_ks4']                           = arg[100] or '0'
		_normalStats['rnk_ks5']                           = arg[101] or '0'
		_normalStats['rnk_ks6']                           = arg[102] or '0'
		_normalStats['rnk_ks7']                           = arg[103] or '0'
		_normalStats['rnk_ks8']                           = arg[104] or '0'
		_normalStats['rnk_ks9']                           = arg[105] or '0'
		_normalStats['rnk_ks10']                          = arg[106] or '0'
		_normalStats['rnk_ks15']                          = arg[107] or '0'
		_normalStats['rnk_smackdown']                     = arg[108] or '0'
		_normalStats['rnk_humiliation']                   = arg[109] or '0'
		_normalStats['rnk_nemesis']                       = arg[110] or '0'
		_normalStats['rnk_retribution']                   = arg[111] or '0'
		_normalStats['total_level_exp']                   = arg[112] or '0'
		_normalStats['rnk_time_earning_exp']              = arg[113] or '0'
		_normalStats['level']                             = arg[114] or '0'
		_normalStats['level_exp']                         = arg[115] or '0'
		_normalStats['discos']                            = arg[116] or '0'
		_normalStats['possible_discos']                   = arg[117] or '0'
		_normalStats['games_played']                      = arg[118] or '0'
		_normalStats['account_type']                      = arg[119] or '0'
		_normalStats['standing']                          = arg[120] or '0'
		_normalStats['level_percent']                     = arg[121] or '0'
		_normalStats['max_exp']                           = arg[122] or '0'
		_normalStats['min_exp']                           = arg[123] or '0'
		_normalStats['mid_games_played']                  = arg[124] or '0'
		_normalStats['mid_discos']                        = arg[125] or '0'
		_normalStats['total_games_played']                = arg[126] or '0'
		_normalStats['total_discos']                      = arg[127] or '0'
		_normalStats['event_id']                          = arg[128] or '0'
		_normalStats['events']                            = arg[129] or '0'
		_normalStats['uncs_discos']                       = arg[130] or '0'
		_normalStats['unrnk_discos']                      = arg[131] or '0'
		_normalStats['uncs_games_played']                 = arg[132] or '0'
		_normalStats['unrnk_games_played']                = arg[133] or '0'
		_normalStats['rift_games_played']                 = arg[134] or '0'
		_normalStats['rift_discos']                       = arg[135] or '0'
		_normalStats['current_level']                     = arg[136] or '0'
		_normalStats['highest_level_current']             = arg[137] or '0'
		_normalStats['season_id']                         = arg[138] or '0'
		_normalStats['current_ranking']                   = arg[139] or '0'
		_normalStats['curr_season_cam_games_played']      = arg[140] or '0'
		_normalStats['curr_season_cam_cs_games_played']   = arg[141] or '0'
		_normalStats['curr_season_cam_discos']            = arg[142] or '0'
		_normalStats['curr_season_cam_cs_discos']         = arg[143] or '0'
		_normalStats['prev_seasons_cam_games_played']     = arg[144] or '0'
		_normalStats['prev_seasons_cam_cs_games_played']  = arg[145] or '0'
		_normalStats['prev_seasons_cam_discos']           = arg[146] or '0'
		_normalStats['prev_seasons_cam_cs_discos']        = arg[147] or '0'
		_normalStats['highest_ranking']					  = arg[148] or '0'
			_normalStats['quest_stats']					      = arg[149] or ''

			bNormalStatsRetrieved = true
		end
	end
	
	local statsInfo = {}
	if true then
		statsInfo.account_id					= _normalStats['account_id']           
		statsInfo.season_id 					= nil
		statsInfo.mmr							= _normalStats['rnk_amm_team_rating']
		statsInfo.current_level 				= nil
		statsInfo.level_exp 					= nil
		statsInfo.current_ranking				= nil
		statsInfo.highest_level_current			= nil
		statsInfo.favHero1						= _normalStats['favHero1']
		statsInfo.favHero2						= _normalStats['favHero2']
		statsInfo.favHero3						= _normalStats['favHero3']
		statsInfo.favHero4						= _normalStats['favHero4']
		statsInfo.favHero5						= _normalStats['favHero5']        
		statsInfo.favHero1Time					= _normalStats['favHero1Time']
		statsInfo.favHero2Time					= _normalStats['favHero2Time']
		statsInfo.favHero3Time					= _normalStats['favHero3Time']
		statsInfo.favHero4Time					= _normalStats['favHero4Time']
		statsInfo.favHero5Time					= _normalStats['favHero5Time']
		statsInfo.favHero1_2					= _normalStats['favHero1_2']
		statsInfo.favHero2_2					= _normalStats['favHero2_2']
		statsInfo.favHero3_2					= _normalStats['favHero3_2']
		statsInfo.favHero4_2					= _normalStats['favHero4_2']
		statsInfo.favHero5_2					= _normalStats['favHero5_2']
		statsInfo.total_games_played			= _normalStats['total_games_played']
		statsInfo.cur_games_played		     	= _normalStats['rnk_games_played']
		statsInfo.total_discos                  = _normalStats['total_discos']
		statsInfo.cur_discos           			= _normalStats['rnk_discos']
		statsInfo.avgCreepKills                 = _normalStats['avgCreepKills']
		statsInfo.avgDenies                     = _normalStats['avgDenies']
		statsInfo.avgGameLength                 = _normalStats['avgGameLength']
		statsInfo.avgXP_min                     = _normalStats['avgXP_min']
		statsInfo.avgActions_min                = _normalStats['avgActions_min']
		statsInfo.avgNeutralKills               = _normalStats['avgNeutralKills']
		statsInfo.avgWardsUsed                  = _normalStats['avgWardsUsed']
		statsInfo.k_d_a                         = _normalStats['k_d_a']
		statsInfo.wins							= _normalStats['rnk_wins']
		statsInfo.losses						= _normalStats['rnk_losses']
		statsInfo.herokills                    	= _normalStats['rnk_herokills']
		statsInfo.deaths                       	= _normalStats['rnk_deaths']
		statsInfo.heroassists                  	= _normalStats['rnk_heroassists']
		statsInfo.exp                          	= _normalStats['rnk_exp']
		statsInfo.gold                         	= _normalStats['rnk_gold']
		statsInfo.secs                         	= _normalStats['rnk_secs']
		statsInfo.smackdown                    	= _normalStats['rnk_smackdown']
		statsInfo.humiliation                  	= _normalStats['rnk_humiliation']
		statsInfo.ks3                          	= _normalStats['rnk_ks3']
		statsInfo.ks4                          	= _normalStats['rnk_ks4']
		statsInfo.ks5                          	= _normalStats['rnk_ks5']
		statsInfo.ks6                          	= _normalStats['rnk_ks6']
		statsInfo.ks7                          	= _normalStats['rnk_ks7']
		statsInfo.ks8                          	= _normalStats['rnk_ks8']
		statsInfo.ks9                          	= _normalStats['rnk_ks9']
		statsInfo.ks10                         	= _normalStats['rnk_ks10']
		statsInfo.ks15                         	= _normalStats['rnk_ks15']
		statsInfo.doublekill                   	= _normalStats['rnk_doublekill']
		statsInfo.triplekill                   	= _normalStats['rnk_triplekill']
		statsInfo.quadkill                     	= _normalStats['rnk_quadkill']
		statsInfo.annihilation                 	= _normalStats['rnk_annihilation']
		statsInfo.bloodlust                    	= _normalStats['rnk_bloodlust']
		statsInfo.quest_stats					= _normalStats['quest_stats']
	end
	
	if _currentTab == 'stats' then
		SetPlayerStatsStatsInfo(statsInfo, true, true, false)
	
		local tip_colors = {}
		local tip_numbers = {}
		tip_colors.public = '^w'
		tip_colors.normal = '^r'
		tip_colors.casual = '^w'
		tip_colors.season_normal = '^w'
		tip_colors.season_casual = '^w'
		tip_colors.pre_season_normal = '^w'
		tip_colors.pre_season_casual = '^w'
		tip_colors.midwars = '^w'
		
		tip_numbers.public = _normalStats['acc_games_played']
		tip_numbers.normal = _normalStats['rnk_games_played']
		tip_numbers.casual = _normalStats['cs_games_played']
		tip_numbers.season_normal = _normalStats['curr_season_cam_games_played']
		tip_numbers.season_casual = _normalStats['curr_season_cam_cs_games_played']
		tip_numbers.pre_season_normal = _normalStats['prev_seasons_cam_games_played']
		tip_numbers.pre_season_casual = _normalStats['prev_seasons_cam_cs_games_played']
		tip_numbers.midwars = _normalStats['mid_games_played']
		SetTipMatchesPlayed(tip_colors, tip_numbers)
		
		tip_numbers.public = _normalStats['acc_discos']
		tip_numbers.normal = _normalStats['rnk_discos']
		tip_numbers.casual = _normalStats['cs_discos']
		tip_numbers.season_normal = _normalStats['curr_season_cam_discos']
		tip_numbers.season_casual = _normalStats['curr_season_cam_cs_discos']
		tip_numbers.pre_season_normal = _normalStats['prev_seasons_cam_discos']
		tip_numbers.pre_season_casual = _normalStats['prev_seasons_cam_cs_discos']
		tip_numbers.midwars = _normalStats['mid_discos']
		SetTipDisconnects(tip_colors, tip_numbers)
	else
	end
end

function Player_Stats_V2:OnPlayerStatsCasualResult(...)
	local arg = { ... }
	arg.n = select('#', ...)
	-- Conversion Helper - REMOVED (Use SafeGet)
	if type(arg[1]) == 'userdata' then arg = arg[1] end

	local function SafeGet(src, key)
		if type(src) == 'userdata' then
			local s, v = pcall(function() return src[key] end)
			if s then return v end
		elseif type(src) == 'table' then
			return src[key]
		end
		return nil
	end


	if not bCasualStatsRetrieved then
		-- Modern Server Support (Hashtable Response)
		local nickname = SafeGet(arg, 'nickname')
		if nickname then
			println('^gOnPlayerStatsCasualResult: Using Named Keys from Server Response.')
			
			_casualStats = {}
			for k,v in pairs(arg) do _casualStats[k] = v end
			_casualStats['nickname'] = nickname
			
			-- MAPPING: Modern Keys -> Legacy Keys
			_casualStats['cs_wins'] 			= SafeGet(arg, 'wins') or SafeGet(arg, 'cs_wins') or '0'
			_casualStats['cs_losses'] 			= SafeGet(arg, 'losses') or SafeGet(arg, 'cs_losses') or '0'
			_casualStats['cs_games_played'] 	= SafeGet(arg, 'matches') or SafeGet(arg, 'total_games_played') or SafeGet(arg, 'cs_games_played') or '0'
			_casualStats['k_d_a'] 				= SafeGet(arg, 'k_d_a') or '0.00'
			_casualStats['cs_avg_score'] 		= SafeGet(arg, 'avg_score') or '0'
			_casualStats['cs_herokills'] 		= SafeGet(arg, 'herokills') or '0'
			_casualStats['cs_heroassists'] 		= SafeGet(arg, 'heroassists') or '0'
			_casualStats['cs_deaths'] 			= SafeGet(arg, 'deaths') or '0'
			_casualStats['cs_gold'] 			= SafeGet(arg, 'gold') or '0'
			_casualStats['cs_exp'] 				= SafeGet(arg, 'exp') or '0'
			_casualStats['cs_secs'] 			= SafeGet(arg, 'secs') or '0'
			_casualStats['total_games_played']	= _casualStats['cs_games_played']
			
			_casualStats['cs_level']            = SafeGet(arg, 'level') or SafeGet(arg, 'cs_level') or '0'
			
			-- Modern Keys -> Legacy Keys (Missing Statistics Tab bindings for Averages and Streaks)
			_casualStats['avgGameLength']       = SafeGet(arg, 'avgGameLength') or '0'
			_casualStats['avgXP_min']           = SafeGet(arg, 'avgXP_min') or '0'
			_casualStats['avgDenies']           = SafeGet(arg, 'avgDenies') or '0'
			_casualStats['avgCreepKills']       = SafeGet(arg, 'avgCreepKills') or '0'
			_casualStats['avgNeutralKills']     = SafeGet(arg, 'avgNeutralKills') or '0'
			_casualStats['avgActions_min']      = SafeGet(arg, 'avgActions_min') or '0'
			_casualStats['avgWardsUsed']        = SafeGet(arg, 'avgWardsUsed') or '0'
			_casualStats['cs_smackdown']        = SafeGet(arg, 'smackdown') or '0'
			_casualStats['cs_humiliation']      = SafeGet(arg, 'humiliation') or '0'
			_casualStats['cs_nemesis']          = SafeGet(arg, 'nemesis') or '0'
			_casualStats['cs_retribution']      = SafeGet(arg, 'retribution') or '0'

			-- Fav Heroes
			_casualStats['favHero1']                          = SafeGet(arg, 'favHero1') or SafeGet(arg, '55') or '0'
			_casualStats['favHero2']                          = SafeGet(arg, 'favHero2') or SafeGet(arg, '56') or '0'
			_casualStats['favHero3']                          = SafeGet(arg, 'favHero3') or SafeGet(arg, '57') or '0'
			_casualStats['favHero4']                          = SafeGet(arg, 'favHero4') or SafeGet(arg, '58') or '0'
			_casualStats['favHero5']                          = SafeGet(arg, 'favHero5') or SafeGet(arg, '59') or '0'
			_casualStats['favHero1Time']                      = SafeGet(arg, 'favHero1Time') or SafeGet(arg, '60') or '0'
			_casualStats['favHero2Time']                      = SafeGet(arg, 'favHero2Time') or SafeGet(arg, '61') or '0'
			_casualStats['favHero3Time']                      = SafeGet(arg, 'favHero3Time') or SafeGet(arg, '62') or '0'
			_casualStats['favHero4Time']                      = SafeGet(arg, 'favHero4Time') or SafeGet(arg, '63') or '0'
			_casualStats['favHero5Time']                      = SafeGet(arg, 'favHero5Time') or SafeGet(arg, '64') or '0'
			_casualStats['favHero1_2']                        = SafeGet(arg, 'favHero1_2') or SafeGet(arg, '77') or '0'
			_casualStats['favHero2_2']                        = SafeGet(arg, 'favHero2_2') or SafeGet(arg, '78') or '0'
			_casualStats['favHero3_2']                        = SafeGet(arg, 'favHero3_2') or SafeGet(arg, '79') or '0'
			_casualStats['favHero4_2']                        = SafeGet(arg, 'favHero4_2') or SafeGet(arg, '80') or '0'
			_casualStats['favHero5_2']                        = SafeGet(arg, 'favHero5_2') or SafeGet(arg, '81') or '0'

			bCasualStatsRetrieved = true
		else
			_casualStats['nickname']		        		  = arg[1] or '0'
		_casualStats['name']                              = arg[2] or '0'
		_casualStats['rank']                              = arg[3] or '0'
		_casualStats['cs_level']                          = arg[4] or '0'
		_casualStats['account_id']                        = arg[5] or '0'
		_casualStats['cs_games_played']      			  = arg[6] or '0'
		_casualStats['cs_wins']                           = arg[7] or '0'
		_casualStats['cs_losses']                         = arg[8] or '0'
		_casualStats['cs_concedes']                       = arg[9] or '0'
		_casualStats['cs_concedevotes']                   = arg[10] or '0'
		_casualStats['cs_buybacks']                       = arg[11] or '0'
		_casualStats['cs_discos']            			  = arg[12] or '0'
		_casualStats['cs_kicked']                         = arg[13] or '0'
		_casualStats['cs_amm_team_rating']                = arg[14] or '0'
		_casualStats['cs_pub_count']                      = arg[15] or '0'
		_casualStats['cs_amm_solo_rating']                = arg[16] or '0'
		_casualStats['cs_amm_solo_count']                 = arg[17] or '0'
		_casualStats['cs_amm_team_rating']                = arg[18] or '0'
		_casualStats['cs_amm_team_count']                 = arg[19] or '0'
		_casualStats['cs_avg_score']                      = arg[20] or '0'
		_casualStats['cs_herokills']                      = arg[21] or '0'
		_casualStats['cs_herodmg']                        = arg[22] or '0'
		_casualStats['cs_heroexp']                        = arg[23] or '0'
		_casualStats['cs_herokillsgold']                  = arg[24] or '0'
		_casualStats['cs_heroassists']                    = arg[25] or '0'
		_casualStats['cs_deaths']                         = arg[26] or '0'
		_casualStats['cs_goldlost2death']                 = arg[27] or '0'
		_casualStats['cs_secs_dead']                      = arg[28] or '0'
		_casualStats['cs_teamcreepkills']                 = arg[29] or '0'
		_casualStats['cs_teamcreepdmg']                   = arg[30] or '0'
		_casualStats['cs_teamcreepexp']                   = arg[31] or '0'
		_casualStats['cs_teamcreepgold']                  = arg[32] or '0'
		_casualStats['cs_neutralcreepkills']              = arg[33] or '0'
		_casualStats['cs_neutralcreepdmg']                = arg[34] or '0'
		_casualStats['cs_neutralcreepexp']                = arg[35] or '0'
		_casualStats['cs_neutralcreepgold']               = arg[36] or '0'
		_casualStats['cs_bdmg']                           = arg[37] or '0'
		_casualStats['cs_bdmgexp']                        = arg[38] or '0'
		_casualStats['cs_razed']                          = arg[39] or '0'
		_casualStats['cs_bgold']                          = arg[40] or '0'
		_casualStats['cs_denies']                         = arg[41] or '0'
		_casualStats['cs_exp_denied']                     = arg[42] or '0'
		_casualStats['cs_gold']                           = arg[43] or '0'
		_casualStats['cs_gold_spend']                     = arg[44] or '0'
		_casualStats['cs_exp']                            = arg[45] or '0'
		_casualStats['cs_actions']                        = arg[46] or '0'
		_casualStats['cs_secs']                           = arg[47] or '0'
		_casualStats['cs_consumables']                    = arg[48] or '0'
		_casualStats['cs_wards']                          = arg[49] or '0'
		_casualStats['cs_em_played']                      = arg[50] or '0'
		_casualStats['maxXP']                             = arg[51] or '0'
		_casualStats['last_activity']                     = arg[52] or '0'
		_casualStats['matchIds']                          = arg[53] or '0'
		_casualStats['matchDates']                        = arg[54] or '0'
		_casualStats['favHero1']                          = arg[55] or '0'
		_casualStats['favHero2']                          = arg[56] or '0'
		_casualStats['favHero3']                          = arg[57] or '0'
		_casualStats['favHero4']                          = arg[58] or '0'
		_casualStats['favHero5']                          = arg[59] or '0'
		_casualStats['favHero1Time']                      = arg[60] or '0'
		_casualStats['favHero2Time']                      = arg[61] or '0'
		_casualStats['favHero3Time']                      = arg[62] or '0'
		_casualStats['favHero4Time']                      = arg[63] or '0'
		_casualStats['favHero5Time']                      = arg[64] or '0'
		_casualStats['xp2nextLevel']                      = arg[65] or '0'
		_casualStats['xpPercent']                         = arg[66] or '0'
		_casualStats['percentEM']                         = arg[67] or '0'
		_casualStats['k_d_a']                             = arg[68] or '0'
		_casualStats['avgGameLength']                     = arg[69] or '0'
		_casualStats['avgXP_min']                         = arg[70] or '0'
		_casualStats['avgDenies']                         = arg[71] or '0'
		_casualStats['avgCreepKills']                     = arg[72] or '0'
		_casualStats['avgNeutralKills']                   = arg[73] or '0'
		_casualStats['avgActions_min']                    = arg[74] or '0'
		_casualStats['avgWardsUsed']                      = arg[75] or '0'
		_casualStats['create_date']                       = arg[76] or '0'
		_casualStats['favHero1_2']                        = arg[77] or '0'
		_casualStats['favHero2_2']                        = arg[78] or '0'
		_casualStats['favHero3_2']                        = arg[79] or '0'
		_casualStats['favHero4_2']                        = arg[80] or '0'
		_casualStats['favHero5_2']                        = arg[81] or '0'
		_casualStats['favHero1id']                        = arg[82] or '0'
		_casualStats['favHero2id']                        = arg[83] or '0'
		_casualStats['favHero3id']                        = arg[84] or '0'
		_casualStats['favHero4id']                        = arg[85] or '0'
		_casualStats['favHero5id']                        = arg[86] or '0'
		_casualStats['error']                             = arg[87] or '0'
		_casualStats['cs_level']                          = arg[88] or '0'
		_casualStats['selected_upgrades']                 = arg[89] or '0'
		_casualStats['acc_games_played']                  = arg[90] or '0'
		_casualStats['rnk_games_played']                  = arg[91] or '0'
		_casualStats['acc_discos']                        = arg[92] or '0'
		_casualStats['rnk_discos']                        = arg[93] or '0'
		_casualStats['cs_bloodlust']                      = arg[94] or '0'
		_casualStats['cs_doublekill']                     = arg[95] or '0'
		_casualStats['cs_triplekill']                     = arg[96] or '0'
		_casualStats['cs_quadkill']                       = arg[97] or '0'
		_casualStats['cs_annihilation']                   = arg[98] or '0'
		_casualStats['cs_ks3']                            = arg[99] or '0'
		_casualStats['cs_ks4']                            = arg[100] or '0'
		_casualStats['cs_ks5']                            = arg[101] or '0'
		_casualStats['cs_ks6']                            = arg[102] or '0'
		_casualStats['cs_ks7']                            = arg[103] or '0'
		_casualStats['cs_ks8']                            = arg[104] or '0'
		_casualStats['cs_ks9']                            = arg[105] or '0'
		_casualStats['cs_ks10']                           = arg[106] or '0'
		_casualStats['cs_ks15']                           = arg[107] or '0'
		_casualStats['cs_smackdown']                      = arg[108] or '0'
		_casualStats['cs_humiliation']                    = arg[109] or '0'
		_casualStats['cs_nemesis']                        = arg[110] or '0'
		_casualStats['cs_retribution']                    = arg[111] or '0'
		_casualStats['total_level_exp']                   = arg[112] or '0'
		_casualStats['cs_time_earning_exp']               = arg[113] or '0'
		_casualStats['level']                             = arg[114] or '0'
		_casualStats['level_exp']                         = arg[115] or '0'
		_casualStats['discos']                            = arg[116] or '0'
		_casualStats['possible_discos']                   = arg[117] or '0'
		_casualStats['games_played']                      = arg[118] or '0'
		_casualStats['account_type']                      = arg[119] or '0'
		_casualStats['standing']                          = arg[120] or '0'
		_casualStats['level_percent']                     = arg[121] or '0'
		_casualStats['max_exp']                           = arg[122] or '0'
		_casualStats['min_exp']                           = arg[123] or '0'
		_casualStats['mid_games_played']                  = arg[124] or '0'
		_casualStats['mid_discos']                        = arg[125] or '0'
		_casualStats['total_games_played']                = arg[126] or '0'
		_casualStats['total_discos']                      = arg[127] or '0'
		_casualStats['event_id']                          = arg[128] or '0'
		_casualStats['events']                            = arg[129] or '0'
		_casualStats['uncs_discos']                       = arg[130] or '0'
		_casualStats['unrnk_discos']                      = arg[131] or '0'
		_casualStats['uncs_games_played']                 = arg[132] or '0'
		_casualStats['unrnk_games_played']                = arg[133] or '0'
		_casualStats['rift_games_played']                 = arg[134] or '0'
		_casualStats['rift_discos']                       = arg[135] or '0'
		_casualStats['current_level']                     = arg[136] or '0'
		_casualStats['highest_level_current']             = arg[137] or '0'
		_casualStats['season_id']                         = arg[138] or '0'
		_casualStats['current_ranking']                   = arg[139] or '0'
		_casualStats['curr_season_cam_games_played']      = arg[140] or '0'
		_casualStats['curr_season_cam_cs_games_played']   = arg[141] or '0'
		_casualStats['curr_season_cam_discos']            = arg[142] or '0'
		_casualStats['curr_season_cam_cs_discos']         = arg[143] or '0'
		_casualStats['prev_seasons_cam_games_played']     = arg[144] or '0'
		_casualStats['prev_seasons_cam_cs_games_played']  = arg[145] or '0'
		_casualStats['prev_seasons_cam_discos']           = arg[146] or '0'
		_casualStats['prev_seasons_cam_cs_discos']        = arg[147] or '0'
		_casualStats['highest_ranking']					  = arg[148] or '0'
			_casualStats['quest_stats']					      = arg[149] or ''

			bCasualStatsRetrieved = true
		end
	end
	
	local statsInfo = {}
	if true then
		statsInfo.account_id					= _casualStats['account_id']           
		statsInfo.season_id 					= nil
		statsInfo.mmr							= _casualStats['cs_amm_team_rating']
		statsInfo.current_level 				= nil
		statsInfo.level_exp 					= nil
		statsInfo.current_ranking				= nil
		statsInfo.highest_level_current			= nil
		statsInfo.favHero1						= _casualStats['favHero1']
		statsInfo.favHero2						= _casualStats['favHero2']
		statsInfo.favHero3						= _casualStats['favHero3']
		statsInfo.favHero4						= _casualStats['favHero4']
		statsInfo.favHero5						= _casualStats['favHero5']        
		statsInfo.favHero1Time					= _casualStats['favHero1Time']
		statsInfo.favHero2Time					= _casualStats['favHero2Time']
		statsInfo.favHero3Time					= _casualStats['favHero3Time']
		statsInfo.favHero4Time					= _casualStats['favHero4Time']
		statsInfo.favHero5Time					= _casualStats['favHero5Time']
		statsInfo.favHero1_2					= _casualStats['favHero1_2']
		statsInfo.favHero2_2					= _casualStats['favHero2_2']
		statsInfo.favHero3_2					= _casualStats['favHero3_2']
		statsInfo.favHero4_2					= _casualStats['favHero4_2']
		statsInfo.favHero5_2					= _casualStats['favHero5_2']
		statsInfo.total_games_played			= _casualStats['total_games_played']
		statsInfo.cur_games_played		     	= _casualStats['cs_games_played']
		statsInfo.total_discos                  = _casualStats['total_discos']
		statsInfo.cur_discos           			= _casualStats['cs_discos']
		statsInfo.avgCreepKills                 = _casualStats['avgCreepKills']
		statsInfo.avgDenies                     = _casualStats['avgDenies']
		statsInfo.avgGameLength                 = _casualStats['avgGameLength']
		statsInfo.avgXP_min                     = _casualStats['avgXP_min']
		statsInfo.avgActions_min                = _casualStats['avgActions_min']
		statsInfo.avgNeutralKills               = _casualStats['avgNeutralKills']
		statsInfo.avgWardsUsed                  = _casualStats['avgWardsUsed']
		statsInfo.k_d_a                         = _casualStats['k_d_a']
		statsInfo.wins							= _casualStats['cs_wins']
		statsInfo.losses						= _casualStats['cs_losses']
		statsInfo.herokills                    	= _casualStats['cs_herokills']
		statsInfo.deaths                       	= _casualStats['cs_deaths']
		statsInfo.heroassists                  	= _casualStats['cs_heroassists']
		statsInfo.exp                          	= _casualStats['cs_exp']
		statsInfo.gold                         	= _casualStats['cs_gold']
		statsInfo.secs                         	= _casualStats['cs_secs']
		statsInfo.smackdown                    	= _casualStats['cs_smackdown']
		statsInfo.humiliation                  	= _casualStats['cs_humiliation']
		statsInfo.ks3                          	= _casualStats['cs_ks3']
		statsInfo.ks4                          	= _casualStats['cs_ks4']
		statsInfo.ks5                          	= _casualStats['cs_ks5']
		statsInfo.ks6                          	= _casualStats['cs_ks6']
		statsInfo.ks7                          	= _casualStats['cs_ks7']
		statsInfo.ks8                          	= _casualStats['cs_ks8']
		statsInfo.ks9                          	= _casualStats['cs_ks9']
		statsInfo.ks10                         	= _casualStats['cs_ks10']
		statsInfo.ks15                         	= _casualStats['cs_ks15']
		statsInfo.doublekill                   	= _casualStats['cs_doublekill']
		statsInfo.triplekill                   	= _casualStats['cs_triplekill']
		statsInfo.quadkill                     	= _casualStats['cs_quadkill']
		statsInfo.annihilation                 	= _casualStats['cs_annihilation']
		statsInfo.bloodlust                    	= _casualStats['cs_bloodlust']
		statsInfo.quest_stats					= _casualStats['quest_stats']
	end
	
	if _currentTab == 'stats' then
		SetPlayerStatsStatsInfo(statsInfo, true, true, false)
	
		local tip_colors = {}
		local tip_numbers = {}
		tip_colors.public = '^w'
		tip_colors.normal = '^w'
		tip_colors.casual = '^r'
		tip_colors.season_normal = '^w'
		tip_colors.season_casual = '^w'
		tip_colors.pre_season_normal = '^w'
		tip_colors.pre_season_casual = '^w'
		tip_colors.midwars = '^w'
		
		tip_numbers.public = _casualStats['acc_games_played']
		tip_numbers.normal = _casualStats['rnk_games_played']
		tip_numbers.casual = _casualStats['cs_games_played']
		tip_numbers.season_normal = _casualStats['curr_season_cam_games_played']
		tip_numbers.season_casual = _casualStats['curr_season_cam_cs_games_played']
		tip_numbers.pre_season_normal = _casualStats['prev_seasons_cam_games_played']
		tip_numbers.pre_season_casual = _casualStats['prev_seasons_cam_cs_games_played']
		tip_numbers.midwars = _casualStats['mid_games_played']
		SetTipMatchesPlayed(tip_colors, tip_numbers)
		
		tip_numbers.public = _casualStats['acc_discos']
		tip_numbers.normal = _casualStats['rnk_discos']
		tip_numbers.casual = _casualStats['cs_discos']
		tip_numbers.season_normal = _casualStats['curr_season_cam_discos']
		tip_numbers.season_casual = _casualStats['curr_season_cam_cs_discos']
		tip_numbers.pre_season_normal = _casualStats['prev_seasons_cam_discos']
		tip_numbers.pre_season_casual = _casualStats['prev_seasons_cam_cs_discos']
		tip_numbers.midwars = _casualStats['mid_discos']
		SetTipDisconnects(tip_colors, tip_numbers)
	end
end

function Player_Stats_V2:OnPlayerStatsHistoryNormalResult(...)
	local arg = { ... }
	arg.n = select('#', ...)
	-- Conversion Helper - REMOVED (Use SafeGet)
	if type(arg[1]) == 'userdata' then arg = arg[1] end

	local function SafeGet(src, key)
		if type(src) == 'userdata' then
			local s, v = pcall(function() return src[key] end)
			if s then return v end
		elseif type(src) == 'table' then
			return src[key]
		end
		return nil
	end


		-- Modern Server Support (Hashtable Response)
		local nickname = SafeGet(arg, 'nickname')
		if nickname then
			println('^gOnPlayerStatsHistoryNormalResult: Using Named Keys from Server Response.')
			
			-- MAPPING: Modern Keys -> Legacy Keys
			_seasonHistoryStatsNormal['nickname'] 						= nickname
			_seasonHistoryStatsNormal['cam_wins'] 						= SafeGet(arg, 'wins') or '0'
			_seasonHistoryStatsNormal['cam_losses'] 					= SafeGet(arg, 'losses') or '0'
			_seasonHistoryStatsNormal['curr_season_cam_games_played'] 	= SafeGet(arg, 'matches') or SafeGet(arg, 'total_games_played') or '0'
			_seasonHistoryStatsNormal['k_d_a'] 							= SafeGet(arg, 'k_d_a') or '0.00'
			_seasonHistoryStatsNormal['rnk_avg_score'] 					= SafeGet(arg, 'avg_score') or '0'
			_seasonHistoryStatsNormal['cam_herokills'] 					= SafeGet(arg, 'herokills') or '0'
			_seasonHistoryStatsNormal['cam_heroassists'] 				= SafeGet(arg, 'heroassists') or '0'
			_seasonHistoryStatsNormal['cam_deaths'] 					= SafeGet(arg, 'deaths') or '0'
			_seasonHistoryStatsNormal['cam_gold'] 						= SafeGet(arg, 'gold') or '0'
			_seasonHistoryStatsNormal['cam_exp'] 						= SafeGet(arg, 'exp') or '0'
			_seasonHistoryStatsNormal['cam_secs'] 						= SafeGet(arg, 'secs') or '0'
			_seasonHistoryStatsNormal['total_games_played']				= _seasonHistoryStatsNormal['curr_season_cam_games_played']
			
			_seasonHistoryStatsNormal['season']                         = SafeGet(arg, 'season') or SafeGet(arg, 'season_id') or '0'

			_seasonHistoryStatsNormal['current_level'] 					= SafeGet(arg, 'level') or SafeGet(arg, 'current_level') or '0'
			_seasonHistoryStatsNormal['level'] 							= _seasonHistoryStatsNormal['current_level']
			_seasonHistoryStatsNormal['highest_level_current'] 			= SafeGet(arg, 'highest_level') or SafeGet(arg, 'highest_level_current') or '0'
			_seasonHistoryStatsNormal['current_ranking'] 				= SafeGet(arg, 'ranking') or SafeGet(arg, 'rank') or '0'

			-- Fav Heroes
			_seasonHistoryStatsNormal['favHero1']                          = SafeGet(arg, 'favHero1') or SafeGet(arg, '39') or '0'
			_seasonHistoryStatsNormal['favHero2']                          = SafeGet(arg, 'favHero2') or SafeGet(arg, '40') or '0'
			_seasonHistoryStatsNormal['favHero3']                          = SafeGet(arg, 'favHero3') or SafeGet(arg, '41') or '0'
			_seasonHistoryStatsNormal['favHero4']                          = SafeGet(arg, 'favHero4') or SafeGet(arg, '42') or '0'
			_seasonHistoryStatsNormal['favHero5']                          = SafeGet(arg, 'favHero5') or SafeGet(arg, '43') or '0'
			_seasonHistoryStatsNormal['favHero1Time']                      = SafeGet(arg, 'favHero1Time') or SafeGet(arg, '44') or '0'
			_seasonHistoryStatsNormal['favHero2Time']                      = SafeGet(arg, 'favHero2Time') or SafeGet(arg, '45') or '0'
			_seasonHistoryStatsNormal['favHero3Time']                      = SafeGet(arg, 'favHero3Time') or SafeGet(arg, '46') or '0'
			_seasonHistoryStatsNormal['favHero4Time']                      = SafeGet(arg, 'favHero4Time') or SafeGet(arg, '47') or '0'
			_seasonHistoryStatsNormal['favHero5Time']                      = SafeGet(arg, 'favHero5Time') or SafeGet(arg, '48') or '0'
			_seasonHistoryStatsNormal['favHero1_2']                        = SafeGet(arg, 'favHero1_2') or SafeGet(arg, '49') or '0'
			_seasonHistoryStatsNormal['favHero2_2']                        = SafeGet(arg, 'favHero2_2') or SafeGet(arg, '50') or '0'
			_seasonHistoryStatsNormal['favHero3_2']                        = SafeGet(arg, 'favHero3_2') or SafeGet(arg, '51') or '0'
			_seasonHistoryStatsNormal['favHero4_2']                        = SafeGet(arg, 'favHero4_2') or SafeGet(arg, '52') or '0'
			_seasonHistoryStatsNormal['favHero5_2']                        = SafeGet(arg, 'favHero5_2') or SafeGet(arg, '53') or '0'

			-- Add return if we want to skip legacy logic, but legacy logic writes to fields too?
			-- The legacy logic below writes to specific indexes.
			-- If we return here, we must ensure ALL fields are covered or defaults set.
		else
			_seasonHistoryStatsNormal['nickname']		        		   = arg[1] or '0'
		_seasonHistoryStatsNormal['name']                              = arg[2] or '0'
		_seasonHistoryStatsNormal['rank']                              = arg[3] or '0'
		_seasonHistoryStatsNormal['cam_level']                         = arg[4] or '0'
		_seasonHistoryStatsNormal['account_id']                        = arg[5] or '0'
		_seasonHistoryStatsNormal['curr_season_cam_games_played']	   = arg[6] or '0'
		_seasonHistoryStatsNormal['cam_wins']                          = arg[7] or '0'
		_seasonHistoryStatsNormal['cam_losses']                        = arg[8] or '0'
		_seasonHistoryStatsNormal['cam_concedes']                      = arg[9] or '0'
		_seasonHistoryStatsNormal['cam_concedevotes']                  = arg[10] or '0'
		_seasonHistoryStatsNormal['cam_buybacks']                      = arg[11] or '0'
		_seasonHistoryStatsNormal['curr_season_cam_discos']			   = arg[12] or '0'
		_seasonHistoryStatsNormal['cam_kicked']                        = arg[13] or '0'
		_seasonHistoryStatsNormal['smr']                               = arg[14] or '0'
		_seasonHistoryStatsNormal['cam_pub_count']                     = arg[15] or '0'
		_seasonHistoryStatsNormal['cam_amm_solo_rating']               = arg[16] or '0'
		_seasonHistoryStatsNormal['cam_amm_solo_count']                = arg[17] or '0'
		_seasonHistoryStatsNormal['cam_amm_team_rating']               = arg[18] or '0'
		_seasonHistoryStatsNormal['cam_amm_team_count']                = arg[19] or '0'
		_seasonHistoryStatsNormal['rnk_avg_score']                     = arg[20] or '0'
		_seasonHistoryStatsNormal['cam_herokills']                     = arg[21] or '0'
		_seasonHistoryStatsNormal['cam_herodmg']                       = arg[22] or '0'
		_seasonHistoryStatsNormal['cam_heroexp']                       = arg[23] or '0'
		_seasonHistoryStatsNormal['cam_herokillsgold']                 = arg[24] or '0'
		_seasonHistoryStatsNormal['cam_heroassists']                   = arg[25] or '0'
		_seasonHistoryStatsNormal['cam_deaths']                        = arg[26] or '0'
		_seasonHistoryStatsNormal['cam_goldlost2death']                = arg[27] or '0'
		_seasonHistoryStatsNormal['cam_secs_dead']                     = arg[28] or '0'
		_seasonHistoryStatsNormal['cam_teamcreepkills']                = arg[29] or '0'
		_seasonHistoryStatsNormal['cam_teamcreepdmg']                  = arg[30] or '0'
		_seasonHistoryStatsNormal['cam_teamcreepexp']                  = arg[31] or '0'
		_seasonHistoryStatsNormal['cam_teamcreepgold']                 = arg[32] or '0'
		_seasonHistoryStatsNormal['cam_neutralcreepkills']             = arg[33] or '0'
		_seasonHistoryStatsNormal['cam_neutralcreepdmg']               = arg[34] or '0'
		_seasonHistoryStatsNormal['cam_neutralcreepexp']               = arg[35] or '0'
		_seasonHistoryStatsNormal['cam_neutralcreepgold']              = arg[36] or '0'
		_seasonHistoryStatsNormal['cam_bdmg']                          = arg[37] or '0'
		_seasonHistoryStatsNormal['cam_bdmgexp']                       = arg[38] or '0'
		_seasonHistoryStatsNormal['cam_razed']                         = arg[39] or '0'
		_seasonHistoryStatsNormal['cam_bgold']                         = arg[40] or '0'
		_seasonHistoryStatsNormal['cam_denies']                        = arg[41] or '0'
		_seasonHistoryStatsNormal['cam_exp_denied']                    = arg[42] or '0'
		_seasonHistoryStatsNormal['cam_gold']                          = arg[43] or '0'
		_seasonHistoryStatsNormal['cam_gold_spend']                    = arg[44] or '0'
		_seasonHistoryStatsNormal['cam_exp']                           = arg[45] or '0'
		_seasonHistoryStatsNormal['cam_actions']                       = arg[46] or '0'
		_seasonHistoryStatsNormal['cam_secs']                          = arg[47] or '0'
		_seasonHistoryStatsNormal['cam_consumables']                   = arg[48] or '0'
		_seasonHistoryStatsNormal['cam_wards']                         = arg[49] or '0'
		_seasonHistoryStatsNormal['cam_em_played']                     = arg[50] or '0'
		_seasonHistoryStatsNormal['maxXP']                             = arg[51] or '0'
		_seasonHistoryStatsNormal['last_activity']                     = arg[52] or '0'
		_seasonHistoryStatsNormal['matchIds']                          = arg[53] or '0'
		_seasonHistoryStatsNormal['matchDates']                        = arg[54] or '0'
		_seasonHistoryStatsNormal['favHero1']                          = arg[55] or '0'
		_seasonHistoryStatsNormal['favHero2']                          = arg[56] or '0'
		_seasonHistoryStatsNormal['favHero3']                          = arg[57] or '0'
		_seasonHistoryStatsNormal['favHero4']                          = arg[58] or '0'
		_seasonHistoryStatsNormal['favHero5']                          = arg[59] or '0'
		_seasonHistoryStatsNormal['favHero1Time']                      = arg[60] or '0'
		_seasonHistoryStatsNormal['favHero2Time']                      = arg[61] or '0'
		_seasonHistoryStatsNormal['favHero3Time']                      = arg[62] or '0'
		_seasonHistoryStatsNormal['favHero4Time']                      = arg[63] or '0'
		_seasonHistoryStatsNormal['favHero5Time']                      = arg[64] or '0'
		_seasonHistoryStatsNormal['xp2nextLevel']                      = arg[65] or '0'
		_seasonHistoryStatsNormal['xpPercent']                         = arg[66] or '0'
		_seasonHistoryStatsNormal['percentEM']                         = arg[67] or '0'
		_seasonHistoryStatsNormal['k_d_a']                             = arg[68] or '0'
		_seasonHistoryStatsNormal['avgGameLength']                     = arg[69] or '0'
		_seasonHistoryStatsNormal['avgXP_min']                         = arg[70] or '0'
		_seasonHistoryStatsNormal['avgDenies']                         = arg[71] or '0'
		_seasonHistoryStatsNormal['avgCreepKills']                     = arg[72] or '0'
		_seasonHistoryStatsNormal['avgNeutralKills']                   = arg[73] or '0'
		_seasonHistoryStatsNormal['avgActions_min']                    = arg[74] or '0'
		_seasonHistoryStatsNormal['avgWardsUsed']                      = arg[75] or '0'
		_seasonHistoryStatsNormal['create_date']                       = arg[76] or '0'
		_seasonHistoryStatsNormal['favHero1_2']                        = arg[77] or '0'
		_seasonHistoryStatsNormal['favHero2_2']                        = arg[78] or '0'
		_seasonHistoryStatsNormal['favHero3_2']                        = arg[79] or '0'
		_seasonHistoryStatsNormal['favHero4_2']                        = arg[80] or '0'
		_seasonHistoryStatsNormal['favHero5_2']                        = arg[81] or '0'
		_seasonHistoryStatsNormal['favHero1id']                        = arg[82] or '0'
		_seasonHistoryStatsNormal['favHero2id']                        = arg[83] or '0'
		_seasonHistoryStatsNormal['favHero3id']                        = arg[84] or '0'
		_seasonHistoryStatsNormal['favHero4id']                        = arg[85] or '0'
		_seasonHistoryStatsNormal['favHero5id']                        = arg[86] or '0'
		_seasonHistoryStatsNormal['error']                             = arg[87] or '0'
		_seasonHistoryStatsNormal['cam_level']                         = arg[88] or '0'
		_seasonHistoryStatsNormal['selected_upgrades']                 = arg[89] or '0'
		_seasonHistoryStatsNormal['acc_games_played']                  = arg[90] or '0'
		_seasonHistoryStatsNormal['cs_games_played']                   = arg[91] or '0'
		_seasonHistoryStatsNormal['acc_discos']                        = arg[92] or '0'
		_seasonHistoryStatsNormal['cs_discos']                         = arg[93] or '0'
		_seasonHistoryStatsNormal['cam_bloodlust']                     = arg[94] or '0'
		_seasonHistoryStatsNormal['cam_doublekill']                    = arg[95] or '0'
		_seasonHistoryStatsNormal['cam_triplekill']                    = arg[96] or '0'
		_seasonHistoryStatsNormal['cam_quadkill']                      = arg[97] or '0'
		_seasonHistoryStatsNormal['cam_annihilation']                  = arg[98] or '0'
		_seasonHistoryStatsNormal['cam_ks3']                           = arg[99] or '0'
		_seasonHistoryStatsNormal['cam_ks4']                           = arg[100] or '0'
		_seasonHistoryStatsNormal['cam_ks5']                           = arg[101] or '0'
		_seasonHistoryStatsNormal['cam_ks6']                           = arg[102] or '0'
		_seasonHistoryStatsNormal['cam_ks7']                           = arg[103] or '0'
		_seasonHistoryStatsNormal['cam_ks8']                           = arg[104] or '0'
		_seasonHistoryStatsNormal['cam_ks9']                           = arg[105] or '0'
		_seasonHistoryStatsNormal['cam_ks10']                          = arg[106] or '0'
		_seasonHistoryStatsNormal['cam_ks15']                          = arg[107] or '0'
		_seasonHistoryStatsNormal['cam_smackdown']                     = arg[108] or '0'
		_seasonHistoryStatsNormal['cam_humiliation']                   = arg[109] or '0'
		_seasonHistoryStatsNormal['cam_nemesis']                       = arg[110] or '0'
		_seasonHistoryStatsNormal['cam_retribution']                   = arg[111] or '0'
		_seasonHistoryStatsNormal['total_level_exp']                   = arg[112] or '0'
		_seasonHistoryStatsNormal['cam_time_earning_exp']              = arg[113] or '0'
		_seasonHistoryStatsNormal['level']                             = arg[114] or '0'
		_seasonHistoryStatsNormal['level_exp']                         = arg[115] or '0'
		_seasonHistoryStatsNormal['discos']                            = arg[116] or '0'
		_seasonHistoryStatsNormal['possible_discos']                   = arg[117] or '0'
		_seasonHistoryStatsNormal['games_played']                      = arg[118] or '0'
		_seasonHistoryStatsNormal['account_type']                      = arg[119] or '0'
		_seasonHistoryStatsNormal['standing']                          = arg[120] or '0'
		_seasonHistoryStatsNormal['level_percent']                     = arg[121] or '0'
		_seasonHistoryStatsNormal['max_exp']                           = arg[122] or '0'
		_seasonHistoryStatsNormal['min_exp']                           = arg[123] or '0'
		_seasonHistoryStatsNormal['mid_games_played']                  = arg[124] or '0'
		_seasonHistoryStatsNormal['mid_discos']                        = arg[125] or '0'
		_seasonHistoryStatsNormal['total_games_played']                = arg[126] or '0'
		_seasonHistoryStatsNormal['total_discos']                      = arg[127] or '0'
		_seasonHistoryStatsNormal['event_id']                          = arg[128] or '0'
		_seasonHistoryStatsNormal['events']                            = arg[129] or '0'
		_seasonHistoryStatsNormal['uncs_discos']                       = arg[130] or '0'
		_seasonHistoryStatsNormal['unrnk_discos']                      = arg[131] or '0'
		_seasonHistoryStatsNormal['uncs_games_played']                 = arg[132] or '0'
		_seasonHistoryStatsNormal['unrnk_games_played']                = arg[133] or '0'
		_seasonHistoryStatsNormal['rift_games_played']                 = arg[134] or '0'
		_seasonHistoryStatsNormal['rift_discos']                       = arg[135] or '0'
		_seasonHistoryStatsNormal['highest_level']                     = arg[136] or '0'
		_seasonHistoryStatsNormal['highest_level_current']             = arg[137] or '0'
		_seasonHistoryStatsNormal['season']                    		   = arg[138] or '0'
		_seasonHistoryStatsNormal['current_ranking']                   = arg[139] or '0'
		_seasonHistoryStatsNormal['rnk_games_played']				   = arg[140] or '0'
		_seasonHistoryStatsNormal['curr_season_cam_cs_games_played']   = arg[141] or '0'
		_seasonHistoryStatsNormal['rnk_discos']                        = arg[142] or '0'
		_seasonHistoryStatsNormal['curr_season_cam_cs_discos']         = arg[143] or '0'
		_seasonHistoryStatsNormal['prev_seasons_cam_games_played']     = arg[144] or '0'
		_seasonHistoryStatsNormal['prev_seasons_cam_cs_games_played']  = arg[145] or '0'
		_seasonHistoryStatsNormal['prev_seasons_cam_discos']           = arg[146] or '0'
		_seasonHistoryStatsNormal['prev_seasons_cam_cs_discos']        = arg[147] or '0'
		_seasonHistoryStatsNormal['latest_season_cam_games_played']    = arg[148] or '0'
		_seasonHistoryStatsNormal['latest_season_cam_cs_games_played'] = arg[149] or '0'
		_seasonHistoryStatsNormal['latest_season_cam_discos']          = arg[150] or '0'
			_seasonHistoryStatsNormal['latest_season_cam_cs_discos']       = arg[151] or '0'
			_seasonHistoryStatsNormal['highest_ranking']				   = arg[152] or '0'
		end
	
	local statsInfo = {}
	if true then
		statsInfo.account_id					= _seasonHistoryStatsNormal['account_id']           
		statsInfo.season_id 					= _seasonHistoryStatsNormal['season']
		statsInfo.mmr							= _seasonHistoryStatsNormal['cam_amm_team_rating']
		statsInfo.current_level 				= _seasonHistoryStatsNormal['highest_level']
		statsInfo.level_exp 					= _seasonHistoryStatsNormal['level_exp']
		statsInfo.level_percent					= _seasonHistoryStatsNormal['level_percent']
		statsInfo.current_ranking				= _seasonHistoryStatsNormal['current_ranking']
		statsInfo.highest_level_current			= _seasonHistoryStatsNormal['highest_level_current']
		statsInfo.favHero1						= _seasonHistoryStatsNormal['favHero1']
		statsInfo.favHero2						= _seasonHistoryStatsNormal['favHero2']
		statsInfo.favHero3						= _seasonHistoryStatsNormal['favHero3']
		statsInfo.favHero4						= _seasonHistoryStatsNormal['favHero4']
		statsInfo.favHero5						= _seasonHistoryStatsNormal['favHero5']        
		statsInfo.favHero1Time					= _seasonHistoryStatsNormal['favHero1Time']
		statsInfo.favHero2Time					= _seasonHistoryStatsNormal['favHero2Time']
		statsInfo.favHero3Time					= _seasonHistoryStatsNormal['favHero3Time']
		statsInfo.favHero4Time					= _seasonHistoryStatsNormal['favHero4Time']
		statsInfo.favHero5Time					= _seasonHistoryStatsNormal['favHero5Time']
		statsInfo.favHero1_2					= _seasonHistoryStatsNormal['favHero1_2']
		statsInfo.favHero2_2					= _seasonHistoryStatsNormal['favHero2_2']
		statsInfo.favHero3_2					= _seasonHistoryStatsNormal['favHero3_2']
		statsInfo.favHero4_2					= _seasonHistoryStatsNormal['favHero4_2']
		statsInfo.favHero5_2					= _seasonHistoryStatsNormal['favHero5_2']
		statsInfo.total_games_played			= _seasonHistoryStatsNormal['total_games_played']
		statsInfo.cur_games_played     			= _seasonHistoryStatsNormal['curr_season_cam_games_played']
		statsInfo.total_discos                  = _seasonHistoryStatsNormal['total_discos']
		statsInfo.cur_discos           			= _seasonHistoryStatsNormal['curr_season_cam_discos']
		statsInfo.avgCreepKills                 = _seasonHistoryStatsNormal['avgCreepKills']
		statsInfo.avgDenies                     = _seasonHistoryStatsNormal['avgDenies']
		statsInfo.avgGameLength                 = _seasonHistoryStatsNormal['avgGameLength']
		statsInfo.avgXP_min                     = _seasonHistoryStatsNormal['avgXP_min']
		statsInfo.avgActions_min                = _seasonHistoryStatsNormal['avgActions_min']
		statsInfo.avgNeutralKills               = _seasonHistoryStatsNormal['avgNeutralKills']
		statsInfo.avgWardsUsed                  = _seasonHistoryStatsNormal['avgWardsUsed']
		statsInfo.k_d_a                         = _seasonHistoryStatsNormal['k_d_a']
		statsInfo.wins							= _seasonHistoryStatsNormal['cam_wins']
		statsInfo.losses						= _seasonHistoryStatsNormal['cam_losses']
		statsInfo.herokills                    	= _seasonHistoryStatsNormal['cam_herokills']
		statsInfo.deaths                       	= _seasonHistoryStatsNormal['cam_deaths']
		statsInfo.heroassists                  	= _seasonHistoryStatsNormal['cam_heroassists']
		statsInfo.exp                          	= _seasonHistoryStatsNormal['cam_exp']
		statsInfo.gold                         	= _seasonHistoryStatsNormal['cam_gold']
		statsInfo.secs                         	= _seasonHistoryStatsNormal['cam_secs']
		statsInfo.smackdown                    	= _seasonHistoryStatsNormal['cam_smackdown']
		statsInfo.humiliation                  	= _seasonHistoryStatsNormal['cam_humiliation']
		statsInfo.ks3                          	= _seasonHistoryStatsNormal['cam_ks3']
		statsInfo.ks4                          	= _seasonHistoryStatsNormal['cam_ks4']
		statsInfo.ks5                          	= _seasonHistoryStatsNormal['cam_ks5']
		statsInfo.ks6                          	= _seasonHistoryStatsNormal['cam_ks6']
		statsInfo.ks7                          	= _seasonHistoryStatsNormal['cam_ks7']
		statsInfo.ks8                          	= _seasonHistoryStatsNormal['cam_ks8']
		statsInfo.ks9                          	= _seasonHistoryStatsNormal['cam_ks9']
		statsInfo.ks10                         	= _seasonHistoryStatsNormal['cam_ks10']
		statsInfo.ks15                         	= _seasonHistoryStatsNormal['cam_ks15']
		statsInfo.doublekill                   	= _seasonHistoryStatsNormal['cam_doublekill']
		statsInfo.triplekill                   	= _seasonHistoryStatsNormal['cam_triplekill']
		statsInfo.quadkill                     	= _seasonHistoryStatsNormal['cam_quadkill']
		statsInfo.annihilation                 	= _seasonHistoryStatsNormal['cam_annihilation']
		statsInfo.bloodlust                    	= _seasonHistoryStatsNormal['cam_bloodlust']
	end
	
	if _currentTab == 'stats' then
		if tonumber(statsInfo.season_id) == 0 then
			SetPlayerStatsStatsInfo(statsInfo, true, true, false)
		else
			SetPlayerStatsStatsInfo(statsInfo, false, true, false)
		end
		
		local tip_colors = {}
		local tip_numbers = {}
		tip_colors.public = '^w'
		tip_colors.normal = '^w'
		tip_colors.casual = '^w'
		tip_colors.season_normal = '^r'
		tip_colors.season_casual = '^w'
		tip_colors.pre_season_normal = '^w'
		tip_colors.pre_season_casual = '^w'
		tip_colors.midwars = '^w'
		
		tip_numbers.public = _seasonHistoryStatsNormal['acc_games_played']
		tip_numbers.normal = _seasonHistoryStatsNormal['rnk_games_played']
		tip_numbers.casual = _seasonHistoryStatsNormal['cs_games_played']
		tip_numbers.season_normal = _seasonHistoryStatsNormal['latest_season_cam_games_played']
		tip_numbers.season_casual = _seasonHistoryStatsNormal['latest_season_cam_cs_games_played']
		tip_numbers.pre_season_normal = _seasonHistoryStatsNormal['prev_seasons_cam_games_played']
		tip_numbers.pre_season_casual = _seasonHistoryStatsNormal['prev_seasons_cam_cs_games_played']
		tip_numbers.midwars = _seasonHistoryStatsNormal['mid_games_played']
		SetTipMatchesPlayed(tip_colors, tip_numbers)
		
		tip_numbers.public = _seasonHistoryStatsNormal['acc_discos']
		tip_numbers.normal = _seasonHistoryStatsNormal['rnk_discos']
		tip_numbers.casual = _seasonHistoryStatsNormal['cs_discos']
		tip_numbers.season_normal = _seasonHistoryStatsNormal['latest_season_cam_discos']
		tip_numbers.season_casual = _seasonHistoryStatsNormal['latest_season_cam_cs_discos']
		tip_numbers.pre_season_normal = _seasonHistoryStatsNormal['prev_seasons_cam_discos']
		tip_numbers.pre_season_casual = _seasonHistoryStatsNormal['prev_seasons_cam_cs_discos']
		tip_numbers.midwars = _seasonHistoryStatsNormal['mid_discos']
		SetTipDisconnects(tip_colors, tip_numbers)
	else
	end
end

function Player_Stats_V2:OnPlayerStatsHistoryCasualResult(...)
	local arg = { ... }
	arg.n = select('#', ...)

		_seasonHistoryStatsCasual['nickname']		        		   = arg[1] or '0'
		_seasonHistoryStatsCasual['name']                              = arg[2] or '0'
		_seasonHistoryStatsCasual['rank']                              = arg[3] or '0'
		_seasonHistoryStatsCasual['cam_cs_level']                      = arg[4] or '0'
		_seasonHistoryStatsCasual['account_id']                        = arg[5] or '0'
		_seasonHistoryStatsCasual['curr_season_cam_cs_games_played']   = arg[6] or '0'
		_seasonHistoryStatsCasual['cam_cs_wins']                       = arg[7] or '0'
		_seasonHistoryStatsCasual['cam_cs_losses']                     = arg[8] or '0'
		_seasonHistoryStatsCasual['cam_cs_concedes']                   = arg[9] or '0'
		_seasonHistoryStatsCasual['cam_cs_concedevotes']               = arg[10] or '0'
		_seasonHistoryStatsCasual['cam_cs_buybacks']                   = arg[11] or '0'
		_seasonHistoryStatsCasual['curr_season_cam_cs_discos']	       = arg[12] or '0'
		_seasonHistoryStatsCasual['cam_cs_kicked']                     = arg[13] or '0'
		_seasonHistoryStatsCasual['cam_cs_amm_team_rating']			   = arg[14] or '0'
		_seasonHistoryStatsCasual['cam_cs_pub_count']                  = arg[15] or '0'
		_seasonHistoryStatsCasual['cam_cs_amm_solo_rating']            = arg[16] or '0'
		_seasonHistoryStatsCasual['cam_cs_amm_solo_count']             = arg[17] or '0'
		_seasonHistoryStatsCasual['cam_cs_amm_team_rating']            = arg[18] or '0'
		_seasonHistoryStatsCasual['cam_cs_amm_team_count']             = arg[19] or '0'
		_seasonHistoryStatsCasual['cam_cs_avg_score']                  = arg[20] or '0'
		_seasonHistoryStatsCasual['cam_cs_herokills']                  = arg[21] or '0'
		_seasonHistoryStatsCasual['cam_cs_herodmg']                    = arg[22] or '0'
		_seasonHistoryStatsCasual['cam_cs_heroexp']                    = arg[23] or '0'
		_seasonHistoryStatsCasual['cam_cs_herokillsgold']              = arg[24] or '0'
		_seasonHistoryStatsCasual['cam_cs_heroassists']                = arg[25] or '0'
		_seasonHistoryStatsCasual['cam_cs_deaths']                     = arg[26] or '0'
		_seasonHistoryStatsCasual['cam_cs_goldlost2death']             = arg[27] or '0'
		_seasonHistoryStatsCasual['cam_cs_secs_dead']                  = arg[28] or '0'
		_seasonHistoryStatsCasual['cam_cs_teamcreepkills']             = arg[29] or '0'
		_seasonHistoryStatsCasual['cam_cs_teamcreepdmg']               = arg[30] or '0'
		_seasonHistoryStatsCasual['cam_cs_teamcreepexp']               = arg[31] or '0'
		_seasonHistoryStatsCasual['cam_cs_teamcreepgold']              = arg[32] or '0'
		_seasonHistoryStatsCasual['cam_cs_neutralcreepkills']          = arg[33] or '0'
		_seasonHistoryStatsCasual['cam_cs_neutralcreepdmg']            = arg[34] or '0'
		_seasonHistoryStatsCasual['cam_cs_neutralcreepexp']            = arg[35] or '0'
		_seasonHistoryStatsCasual['cam_cs_neutralcreepgold']           = arg[36] or '0'
		_seasonHistoryStatsCasual['cam_cs_bdmg']                       = arg[37] or '0'
		_seasonHistoryStatsCasual['cam_cs_bdmgexp']                    = arg[38] or '0'
		_seasonHistoryStatsCasual['cam_cs_razed']                      = arg[39] or '0'
		_seasonHistoryStatsCasual['cam_cs_bgold']                      = arg[40] or '0'
	-- Wrapper for Table/Map vs Positional Args
	local args = { ... } args.n = select('#', ...)
	local data = nil

	-- Logan (2026-02-12): Fix for Modern Server Response (Table vs Positional)
	-- Conversion Helper - REMOVED (Use SafeGet)
	if type(args[1]) == 'userdata' then args = args[1] end

	local function SafeGet(src, key)
		if type(src) == 'userdata' then
			local s, v = pcall(function() return src[key] end)
			if s then return v end
		elseif type(src) == 'table' then
			return src[key]
		end
		return nil
	end


		-- Modern Server Support (Hashtable Response)
		local nickname = SafeGet(args, 'nickname')
		if nickname then
			println('^gOnPlayerStatsHistoryCasualResult: Using Named Keys from Server Response.')
			
			-- MAPPING: Modern Keys -> Legacy Keys
			_seasonHistoryStatsCasual['nickname'] 						= nickname
			_seasonHistoryStatsCasual['cam_cs_wins'] 					= SafeGet(args, 'wins') or '0'
			_seasonHistoryStatsCasual['cam_cs_losses'] 					= SafeGet(args, 'losses') or '0'
			_seasonHistoryStatsCasual['curr_season_cam_cs_games_played']= SafeGet(args, 'matches') or SafeGet(args, 'total_games_played') or '0'
			_seasonHistoryStatsCasual['k_d_a'] 							= SafeGet(args, 'k_d_a') or '0.00'
			_seasonHistoryStatsCasual['cam_cs_avg_score'] 				= SafeGet(args, 'avg_score') or '0'
			_seasonHistoryStatsCasual['cam_cs_herokills'] 				= SafeGet(args, 'herokills') or '0'
			_seasonHistoryStatsCasual['cam_cs_heroassists'] 			= SafeGet(args, 'heroassists') or '0'
			_seasonHistoryStatsCasual['cam_cs_deaths'] 					= SafeGet(args, 'deaths') or '0'
			_seasonHistoryStatsCasual['cam_cs_gold'] 					= SafeGet(args, 'gold') or '0'
			_seasonHistoryStatsCasual['cam_cs_exp'] 					= SafeGet(args, 'exp') or '0'
			_seasonHistoryStatsCasual['cam_cs_secs'] 					= SafeGet(args, 'secs') or '0'
			_seasonHistoryStatsCasual['total_games_played']				= _seasonHistoryStatsCasual['curr_season_cam_cs_games_played']

			_seasonHistoryStatsCasual['season']                         = SafeGet(args, 'season') or SafeGet(args, 'season_id') or '0'

			_seasonHistoryStatsCasual['highest_level'] 					= SafeGet(args, 'level') or SafeGet(args, 'current_level') or '0'
			_seasonHistoryStatsCasual['level'] 							= _seasonHistoryStatsCasual['highest_level']
			_seasonHistoryStatsCasual['highest_level_current'] 			= SafeGet(args, 'highest_level') or SafeGet(args, 'highest_level_current') or '0'
			_seasonHistoryStatsCasual['current_ranking'] 				= SafeGet(args, 'ranking') or SafeGet(args, 'rank') or '0'

			-- Fav Heroes
			_seasonHistoryStatsCasual['favHero1']                          = SafeGet(args, 'favHero1') or SafeGet(args, '55') or '0'
			_seasonHistoryStatsCasual['favHero2']                          = SafeGet(args, 'favHero2') or SafeGet(args, '56') or '0'
			_seasonHistoryStatsCasual['favHero3']                          = SafeGet(args, 'favHero3') or SafeGet(args, '57') or '0'
			_seasonHistoryStatsCasual['favHero4']                          = SafeGet(args, 'favHero4') or SafeGet(args, '58') or '0'
			_seasonHistoryStatsCasual['favHero5']                          = SafeGet(args, 'favHero5') or SafeGet(args, '59') or '0'
			_seasonHistoryStatsCasual['favHero1Time']                      = SafeGet(args, 'favHero1Time') or SafeGet(args, '60') or '0'
			_seasonHistoryStatsCasual['favHero2Time']                      = SafeGet(args, 'favHero2Time') or SafeGet(args, '61') or '0'
			_seasonHistoryStatsCasual['favHero3Time']                      = SafeGet(args, 'favHero3Time') or SafeGet(args, '62') or '0'
			_seasonHistoryStatsCasual['favHero4Time']                      = SafeGet(args, 'favHero4Time') or SafeGet(args, '63') or '0'
			_seasonHistoryStatsCasual['favHero5Time']                      = SafeGet(args, 'favHero5Time') or SafeGet(args, '64') or '0'
			_seasonHistoryStatsCasual['favHero1_2']                        = SafeGet(args, 'favHero1_2') or SafeGet(args, '77') or '0'
			_seasonHistoryStatsCasual['favHero2_2']                        = SafeGet(args, 'favHero2_2') or SafeGet(args, '78') or '0'
			_seasonHistoryStatsCasual['favHero3_2']                        = SafeGet(args, 'favHero3_2') or SafeGet(args, '79') or '0'
			_seasonHistoryStatsCasual['favHero4_2']                        = SafeGet(args, 'favHero4_2') or SafeGet(args, '80') or '0'
			_seasonHistoryStatsCasual['favHero5_2']                        = SafeGet(args, 'favHero5_2') or SafeGet(args, '81') or '0'

		else
			-- Legacy Fallback only if no nickname found
			if type(args[1]) == 'table' then
		data = args[1]
	end


	local function GetVal(idx, key)
		if data then
			if key and data[key] then return data[key] end
			if data[idx] then return data[idx] end
			if data[tostring(idx)] then return data[tostring(idx)] end
		elseif type(args) == 'table' then
			if args[idx] then return args[idx] end
		end
		return nil
	end


        -- NOTE: Previous assignments 31-40 were here but are now covered by robust mapping logic below or elsewhere
		_seasonHistoryStatsCasual['cam_cs_denies']                     = GetVal(41, 'cam_cs_denies') or '0'
		_seasonHistoryStatsCasual['cam_cs_exp_denied']                 = GetVal(42, 'cam_cs_exp_denied') or '0'
		_seasonHistoryStatsCasual['cam_cs_gold']                       = GetVal(43, 'cam_cs_gold') or '0'
		_seasonHistoryStatsCasual['cam_cs_gold_spend']                 = GetVal(44, 'cam_cs_gold_spend') or '0'
		_seasonHistoryStatsCasual['cam_cs_exp']                        = GetVal(45, 'cam_cs_exp') or '0'
		_seasonHistoryStatsCasual['cam_cs_actions']                    = GetVal(46, 'cam_cs_actions') or '0'
		_seasonHistoryStatsCasual['cam_cs_secs']                       = GetVal(47, 'cam_cs_secs') or '0'
		_seasonHistoryStatsCasual['cam_cs_consumables']                = GetVal(48, 'cam_cs_consumables') or '0'
		_seasonHistoryStatsCasual['cam_cs_wards']                      = GetVal(49, 'cam_cs_wards') or '0'
		_seasonHistoryStatsCasual['cam_cs_em_played']                  = GetVal(50, 'cam_cs_em_played') or '0'
		_seasonHistoryStatsCasual['maxXP']                             = GetVal(51, 'maxXP') or '0'
		_seasonHistoryStatsCasual['last_activity']                     = GetVal(52, 'last_activity') or '0'
		_seasonHistoryStatsCasual['matchIds']                          = GetVal(53, 'matchIds') or '0'
		_seasonHistoryStatsCasual['matchDates']                        = GetVal(54, 'matchDates') or '0'
		_seasonHistoryStatsCasual['favHero1']                          = GetVal(55, 'favHero1') or '0'
		_seasonHistoryStatsCasual['favHero2']                          = GetVal(56, 'favHero2') or '0'
		_seasonHistoryStatsCasual['favHero3']                          = GetVal(57, 'favHero3') or '0'
		_seasonHistoryStatsCasual['favHero4']                          = GetVal(58, 'favHero4') or '0'
		_seasonHistoryStatsCasual['favHero5']                          = GetVal(59, 'favHero5') or '0'
		_seasonHistoryStatsCasual['favHero1Time']                      = GetVal(60, 'favHero1Time') or '0'
		_seasonHistoryStatsCasual['favHero2Time']                      = GetVal(61, 'favHero2Time') or '0'
		_seasonHistoryStatsCasual['favHero3Time']                      = GetVal(62, 'favHero3Time') or '0'
		_seasonHistoryStatsCasual['favHero4Time']                      = GetVal(63, 'favHero4Time') or '0'
		_seasonHistoryStatsCasual['favHero5Time']                      = GetVal(64, 'favHero5Time') or '0'
		_seasonHistoryStatsCasual['xp2nextLevel']                      = GetVal(65, 'xp2nextLevel') or '0'
		_seasonHistoryStatsCasual['xpPercent']                         = GetVal(66, 'xpPercent') or '0'
		_seasonHistoryStatsCasual['percentEM']                         = GetVal(67, 'percentEM') or '0'
		_seasonHistoryStatsCasual['k_d_a']                             = GetVal(68, 'k_d_a') or '0'
		_seasonHistoryStatsCasual['avgGameLength']                     = GetVal(69, 'avgGameLength') or '0'
		_seasonHistoryStatsCasual['avgXP_min']                         = GetVal(70, 'avgXP_min') or '0'
		_seasonHistoryStatsCasual['avgDenies']                         = GetVal(71, 'avgDenies') or '0'
		_seasonHistoryStatsCasual['avgCreepKills']                     = GetVal(72, 'avgCreepKills') or '0'
		_seasonHistoryStatsCasual['avgNeutralKills']                   = GetVal(73, 'avgNeutralKills') or '0'
		_seasonHistoryStatsCasual['avgActions_min']                    = GetVal(74, 'avgActions_min') or '0'
		_seasonHistoryStatsCasual['avgWardsUsed']                      = GetVal(75, 'avgWardsUsed') or '0'
		_seasonHistoryStatsCasual['create_date']                       = GetVal(76, 'create_date') or '0'
		_seasonHistoryStatsCasual['favHero1_2']                        = GetVal(77, 'favHero1_2') or '0'
		_seasonHistoryStatsCasual['favHero2_2']                        = GetVal(78, 'favHero2_2') or '0'
		_seasonHistoryStatsCasual['favHero3_2']                        = GetVal(79, 'favHero3_2') or '0'
		_seasonHistoryStatsCasual['favHero4_2']                        = GetVal(80, 'favHero4_2') or '0'
		_seasonHistoryStatsCasual['favHero5_2']                        = GetVal(81, 'favHero5_2') or '0'
		_seasonHistoryStatsCasual['favHero1id']                        = GetVal(82, 'favHero1id') or '0'
		_seasonHistoryStatsCasual['favHero2id']                        = GetVal(83, 'favHero2id') or '0'
		_seasonHistoryStatsCasual['favHero3id']                        = GetVal(84, 'favHero3id') or '0'
		_seasonHistoryStatsCasual['favHero4id']                        = GetVal(85, 'favHero4id') or '0'
		_seasonHistoryStatsCasual['favHero5id']                        = GetVal(86, 'favHero5id') or '0'
		_seasonHistoryStatsCasual['error']                             = GetVal(87, 'error') or '0'
		_seasonHistoryStatsCasual['cam_cs_level']                      = GetVal(88, 'cam_cs_level') or '0'
		_seasonHistoryStatsCasual['selected_upgrades']                 = GetVal(89, 'selected_upgrades') or '0'
		_seasonHistoryStatsCasual['acc_games_played']                  = GetVal(90, 'acc_games_played') or '0'
		_seasonHistoryStatsCasual['cs_games_played']                   = GetVal(91, 'cs_games_played') or '0'
		_seasonHistoryStatsCasual['acc_discos']                        = GetVal(92, 'acc_discos') or '0'
		_seasonHistoryStatsCasual['cs_discos']                         = GetVal(93, 'cs_discos') or '0'
		_seasonHistoryStatsCasual['cam_cs_bloodlust']                  = GetVal(94, 'cam_cs_bloodlust') or '0'
		_seasonHistoryStatsCasual['cam_cs_doublekill']                 = GetVal(95, 'cam_cs_doublekill') or '0'
		_seasonHistoryStatsCasual['cam_cs_triplekill']                 = GetVal(96, 'cam_cs_triplekill') or '0'
		_seasonHistoryStatsCasual['cam_cs_quadkill']                   = GetVal(97, 'cam_cs_quadkill') or '0'
		_seasonHistoryStatsCasual['cam_cs_annihilation']               = GetVal(98, 'cam_cs_annihilation') or '0'
		_seasonHistoryStatsCasual['cam_cs_ks3']                        = GetVal(99, 'cam_cs_ks3') or '0'
		_seasonHistoryStatsCasual['cam_cs_ks4']                        = GetVal(100, 'cam_cs_ks4') or '0'
		_seasonHistoryStatsCasual['cam_cs_ks5']                        = GetVal(101, 'cam_cs_ks5') or '0'
		_seasonHistoryStatsCasual['cam_cs_ks6']                        = GetVal(102, 'cam_cs_ks6') or '0'
		_seasonHistoryStatsCasual['cam_cs_ks7']                        = GetVal(103, 'cam_cs_ks7') or '0'
		_seasonHistoryStatsCasual['cam_cs_ks8']                        = GetVal(104, 'cam_cs_ks8') or '0'
		_seasonHistoryStatsCasual['cam_cs_ks9']                        = GetVal(105, 'cam_cs_ks9') or '0'
		_seasonHistoryStatsCasual['cam_cs_ks10']                       = GetVal(106, 'cam_cs_ks10') or '0'
		_seasonHistoryStatsCasual['cam_cs_ks15']                       = GetVal(107, 'cam_cs_ks15') or '0'
		_seasonHistoryStatsCasual['cam_cs_smackdown']                  = GetVal(108, 'cam_cs_smackdown') or '0'
		_seasonHistoryStatsCasual['cam_cs_humiliation']                = GetVal(109, 'cam_cs_humiliation') or '0'
		_seasonHistoryStatsCasual['cam_cs_nemesis']                    = GetVal(110, 'cam_cs_nemesis') or '0'
		_seasonHistoryStatsCasual['cam_cs_retribution']                = GetVal(111, 'cam_cs_retribution') or '0'
		_seasonHistoryStatsCasual['total_level_exp']                   = GetVal(112, 'total_level_exp') or '0'
		_seasonHistoryStatsCasual['cam_cs_time_earning_exp']           = GetVal(113, 'cam_cs_time_earning_exp') or '0'
		_seasonHistoryStatsCasual['level']                             = GetVal(114, 'level') or '0'
		_seasonHistoryStatsCasual['level_exp']                         = GetVal(115, 'level_exp') or '0'
		_seasonHistoryStatsCasual['discos']                            = GetVal(116, 'discos') or '0'
		_seasonHistoryStatsCasual['possible_discos']                   = GetVal(117, 'possible_discos') or '0'
		_seasonHistoryStatsCasual['games_played']                      = GetVal(118, 'games_played') or '0'
		_seasonHistoryStatsCasual['account_type']                      = GetVal(119, 'account_type') or '0'
		_seasonHistoryStatsCasual['standing']                          = GetVal(120, 'standing') or '0'
		_seasonHistoryStatsCasual['level_percent']                     = GetVal(121, 'level_percent') or '0'
		_seasonHistoryStatsCasual['max_exp']                           = GetVal(122, 'max_exp') or '0'
		_seasonHistoryStatsCasual['min_exp']                           = GetVal(123, 'min_exp') or '0'
		_seasonHistoryStatsCasual['mid_games_played']                  = GetVal(124, 'mid_games_played') or '0'
		_seasonHistoryStatsCasual['mid_discos']                        = GetVal(125, 'mid_discos') or '0'
		_seasonHistoryStatsCasual['total_games_played']                = GetVal(126, 'total_games_played') or '0'
		_seasonHistoryStatsCasual['total_discos']                      = GetVal(127, 'total_discos') or '0'
		_seasonHistoryStatsCasual['event_id']                          = GetVal(128, 'event_id') or '0'
		_seasonHistoryStatsCasual['events']                            = GetVal(129, 'events') or '0'
		_seasonHistoryStatsCasual['uncs_discos']                       = GetVal(130, 'uncs_discos') or '0'
		_seasonHistoryStatsCasual['unrnk_discos']                      = GetVal(131, 'unrnk_discos') or '0'
		_seasonHistoryStatsCasual['uncs_games_played']                 = GetVal(132, 'uncs_games_played') or '0'
		_seasonHistoryStatsCasual['unrnk_games_played']                = GetVal(133, 'unrnk_games_played') or '0'
		_seasonHistoryStatsCasual['rift_games_played']                 = GetVal(134, 'rift_games_played') or '0'
		_seasonHistoryStatsCasual['rift_discos']                       = GetVal(135, 'rift_discos') or '0'
		_seasonHistoryStatsCasual['highest_level']                     = GetVal(136, 'highest_level') or '0'
		_seasonHistoryStatsCasual['highest_level_current']             = GetVal(137, 'highest_level_current') or '0'
		_seasonHistoryStatsCasual['season']                            = GetVal(138, 'season') or '0'
		_seasonHistoryStatsCasual['current_ranking']                   = GetVal(139, 'current_ranking') or '0'
		_seasonHistoryStatsCasual['curr_season_cam_games_played']      = GetVal(140, 'curr_season_cam_games_played') or '0'
		_seasonHistoryStatsCasual['rnk_games_played']			       = GetVal(141, 'rnk_games_played') or '0'
		_seasonHistoryStatsCasual['curr_season_cam_discos'] 	       = GetVal(142, 'curr_season_cam_discos') or '0'
		_seasonHistoryStatsCasual['rnk_discos']				           = GetVal(143, 'rnk_discos') or '0'
		_seasonHistoryStatsCasual['prev_seasons_cam_games_played']     = GetVal(144, 'prev_seasons_cam_games_played') or '0'
		_seasonHistoryStatsCasual['prev_seasons_cam_cs_games_played']  = GetVal(145, 'prev_seasons_cam_cs_games_played') or '0'
		_seasonHistoryStatsCasual['prev_seasons_cam_discos']           = GetVal(146, 'prev_seasons_cam_discos') or '0'
		_seasonHistoryStatsCasual['prev_seasons_cam_cs_discos']        = GetVal(147, 'prev_seasons_cam_cs_discos') or '0'
		_seasonHistoryStatsCasual['latest_season_cam_games_played']    = GetVal(148, 'latest_season_cam_games_played') or '0'
		_seasonHistoryStatsCasual['latest_season_cam_cs_games_played'] = GetVal(149, 'latest_season_cam_cs_games_played') or '0'
		_seasonHistoryStatsCasual['latest_season_cam_discos']          = GetVal(150, 'latest_season_cam_discos') or '0'
		_seasonHistoryStatsCasual['latest_season_cam_cs_discos']       = GetVal(151, 'latest_season_cam_cs_discos') or '0'
		_seasonHistoryStatsCasual['highest_ranking']				   = GetVal(152, 'highest_ranking') or '0'
		end

	
	local statsInfo = {}
	if true then
		statsInfo.account_id					= _seasonHistoryStatsCasual['account_id']           
		statsInfo.season_id 					= _seasonHistoryStatsCasual['season']
		statsInfo.mmr							= _seasonHistoryStatsCasual['cam_cs_amm_team_rating']
		statsInfo.current_level 				= _seasonHistoryStatsCasual['highest_level']
		statsInfo.level_exp 					= _seasonHistoryStatsCasual['level_exp']
		statsInfo.level_percent					= _seasonHistoryStatsCasual['level_percent']
		statsInfo.current_ranking				= _seasonHistoryStatsCasual['current_ranking']
		statsInfo.highest_level_current			= _seasonHistoryStatsCasual['highest_level_current']
		statsInfo.favHero1						= _seasonHistoryStatsCasual['favHero1']
		statsInfo.favHero2						= _seasonHistoryStatsCasual['favHero2']
		statsInfo.favHero3						= _seasonHistoryStatsCasual['favHero3']
		statsInfo.favHero4						= _seasonHistoryStatsCasual['favHero4']
		statsInfo.favHero5						= _seasonHistoryStatsCasual['favHero5']        
		statsInfo.favHero1Time					= _seasonHistoryStatsCasual['favHero1Time']
		statsInfo.favHero2Time					= _seasonHistoryStatsCasual['favHero2Time']
		statsInfo.favHero3Time					= _seasonHistoryStatsCasual['favHero3Time']
		statsInfo.favHero4Time					= _seasonHistoryStatsCasual['favHero4Time']
		statsInfo.favHero5Time					= _seasonHistoryStatsCasual['favHero5Time']
		statsInfo.favHero1_2					= _seasonHistoryStatsCasual['favHero1_2']
		statsInfo.favHero2_2					= _seasonHistoryStatsCasual['favHero2_2']
		statsInfo.favHero3_2					= _seasonHistoryStatsCasual['favHero3_2']
		statsInfo.favHero4_2					= _seasonHistoryStatsCasual['favHero4_2']
		statsInfo.favHero5_2					= _seasonHistoryStatsCasual['favHero5_2']
		statsInfo.total_games_played			= _seasonHistoryStatsCasual['total_games_played']
		statsInfo.cur_games_played     			= _seasonHistoryStatsCasual['curr_season_cam_cs_games_played']
		statsInfo.total_discos                  = _seasonHistoryStatsCasual['total_discos']
		statsInfo.cur_discos           			= _seasonHistoryStatsCasual['curr_season_cam_cs_discos']
		statsInfo.avgCreepKills                 = _seasonHistoryStatsCasual['avgCreepKills']
		statsInfo.avgDenies                     = _seasonHistoryStatsCasual['avgDenies']
		statsInfo.avgGameLength                 = _seasonHistoryStatsCasual['avgGameLength']
		statsInfo.avgXP_min                     = _seasonHistoryStatsCasual['avgXP_min']
		statsInfo.avgActions_min                = _seasonHistoryStatsCasual['avgActions_min']
		statsInfo.avgNeutralKills               = _seasonHistoryStatsCasual['avgNeutralKills']
		statsInfo.avgWardsUsed                  = _seasonHistoryStatsCasual['avgWardsUsed']
		statsInfo.k_d_a                         = _seasonHistoryStatsCasual['k_d_a']
		statsInfo.wins							= _seasonHistoryStatsCasual['cam_cs_wins']
		statsInfo.losses						= _seasonHistoryStatsCasual['cam_cs_losses']
		statsInfo.herokills                    	= _seasonHistoryStatsCasual['cam_cs_herokills']
		statsInfo.deaths                       	= _seasonHistoryStatsCasual['cam_cs_deaths']
		statsInfo.heroassists                  	= _seasonHistoryStatsCasual['cam_cs_heroassists']
		statsInfo.exp                          	= _seasonHistoryStatsCasual['cam_cs_exp']
		statsInfo.gold                         	= _seasonHistoryStatsCasual['cam_cs_gold']
		statsInfo.secs                         	= _seasonHistoryStatsCasual['cam_cs_secs']
		statsInfo.smackdown                    	= _seasonHistoryStatsCasual['cam_cs_smackdown']
		statsInfo.humiliation                  	= _seasonHistoryStatsCasual['cam_cs_humiliation']
		statsInfo.ks3                          	= _seasonHistoryStatsCasual['cam_cs_ks3']
		statsInfo.ks4                          	= _seasonHistoryStatsCasual['cam_cs_ks4']
		statsInfo.ks5                          	= _seasonHistoryStatsCasual['cam_cs_ks5']
		statsInfo.ks6                          	= _seasonHistoryStatsCasual['cam_cs_ks6']
		statsInfo.ks7                          	= _seasonHistoryStatsCasual['cam_cs_ks7']
		statsInfo.ks8                          	= _seasonHistoryStatsCasual['cam_cs_ks8']
		statsInfo.ks9                          	= _seasonHistoryStatsCasual['cam_cs_ks9']
		statsInfo.ks10                         	= _seasonHistoryStatsCasual['cam_cs_ks10']
		statsInfo.ks15                         	= _seasonHistoryStatsCasual['cam_cs_ks15']
		statsInfo.doublekill                   	= _seasonHistoryStatsCasual['cam_cs_doublekill']
		statsInfo.triplekill                   	= _seasonHistoryStatsCasual['cam_cs_triplekill']
		statsInfo.quadkill                     	= _seasonHistoryStatsCasual['cam_cs_quadkill']
		statsInfo.annihilation                 	= _seasonHistoryStatsCasual['cam_cs_annihilation']
		statsInfo.bloodlust                    	= _seasonHistoryStatsCasual['cam_cs_bloodlust']
	end
	
	if _currentTab == 'stats' then
		if tonumber(statsInfo.season_id) == 0 then
			SetPlayerStatsStatsInfo(statsInfo, true, true, false)
		else
			SetPlayerStatsStatsInfo(statsInfo, false, true, false)
		end

		local tip_colors = {}
		local tip_numbers = {}
		tip_colors.public = '^w'
		tip_colors.normal = '^w'
		tip_colors.casual = '^w'
		tip_colors.season_normal = '^w'
		tip_colors.season_casual = '^r'
		tip_colors.pre_season_normal = '^w'
		tip_colors.pre_season_casual = '^w'
		tip_colors.midwars = '^w'
		
		tip_numbers.public = _seasonHistoryStatsCasual['acc_games_played']
		tip_numbers.normal = _seasonHistoryStatsCasual['rnk_games_played']
		tip_numbers.casual = _seasonHistoryStatsCasual['cs_games_played']
		tip_numbers.season_normal = _seasonHistoryStatsCasual['latest_season_cam_games_played']
		tip_numbers.season_casual = _seasonHistoryStatsCasual['latest_season_cam_cs_games_played']
		tip_numbers.pre_season_normal = _seasonHistoryStatsCasual['prev_seasons_cam_games_played']
		tip_numbers.pre_season_casual = _seasonHistoryStatsCasual['prev_seasons_cam_cs_games_played']
		tip_numbers.midwars = _seasonHistoryStatsCasual['mid_games_played']
		SetTipMatchesPlayed(tip_colors, tip_numbers)
		
		tip_numbers.public = _seasonHistoryStatsCasual['acc_discos']
		tip_numbers.normal = _seasonHistoryStatsCasual['rnk_discos']
		tip_numbers.casual = _seasonHistoryStatsCasual['cs_discos']
		tip_numbers.season_normal = _seasonHistoryStatsCasual['latest_season_cam_discos']
		tip_numbers.season_casual = _seasonHistoryStatsCasual['latest_season_cam_cs_discos']
		tip_numbers.pre_season_normal = _seasonHistoryStatsCasual['prev_seasons_cam_discos']
		tip_numbers.pre_season_casual = _seasonHistoryStatsCasual['prev_seasons_cam_cs_discos']
		tip_numbers.midwars = _seasonHistoryStatsCasual['mid_discos']
		SetTipDisconnects(tip_colors, tip_numbers)
	else
	end
end



function Player_Stats_V2:OnPlayerStatsMVPAwardsResult(...)
	local arg = { ... }
	arg.n = select('#', ...)
	
	-- Base64 Handling / Legacy Wrapper Handling
	-- When using Base64, arg[1] is the base64 string.
	-- When using Legacy, arg[1] is the first vararg.
	-- BUT the wrapper might pass `arg` as a table in arg[2] if called via Dispatcher?
	-- Let's inspect `arg` from the function call.
	
	local args = { ... } args.n = select('#', ...)
	if not args.n then args.n = #args end
	local data = nil

	if type(args[1]) == 'table' or type(args[1]) == 'userdata' then
		data = args[1]
	end

	-- Reconstruct flat Key-Value unrolls if the C++ Engine stripped the userdata wrapper
	if type(args[1]) == 'string' and args.n and args.n >= 10 then
		-- Detect if this is a flattened dictionary: key1, val1, key2, val2...
		local isFlatDict = false
		for i = 1, math.min(args.n, 10), 2 do
			if type(args[i]) == 'string' and (args[i] == 'annihilation' or args[i] == 'quadkill' or args[i] == '1' or args[i] == '2') then
				isFlatDict = true
				break
			end
		end

		if isFlatDict then
			local rebuilt = {}
			for i = 1, args.n, 2 do
				local k = args[i]
				local v = args[i+1]
				if k ~= nil then
					rebuilt[k] = v
				end
			end
			data = rebuilt
			println('^y[PlayerStats] Successfully rebuilt MVPAwards variadic flattened Dictionary!^*')
		end
	end
	
	-- Fallback: If not Base64, assume it's the varargs table directly (Legacy behavior IF simple)
	-- BUT if we force Base64, we rely on `data` being populated.
	
	if data then
		for k, v in pairs(data) do
			Echo('^g[MVP RAW] Key: ' .. tostring(k) .. ' = ' .. tostring(v))
		end
	end
	
	-- Helper to fetch values safely from either table (Hashtable strings or Array integers)
	local function GetVal(idx, key)
		if data then
			if key and data[key] then return data[key] end
			if data[tostring(idx)] then return data[tostring(idx)] end
			if data[idx] then return data[idx] end
		elseif type(args) == 'table' then
			-- Legacy positional
			if args[idx] then return args[idx] end
		end
		return 0
	end

	-- Using Hybrid Indices (from OnPlayerStatsNormalSeasonResult mapping)
	_mvpAwards['mann']			= {tonumber(GetVal(1, 'annihilation')) or 0, 7}   -- Annihilation
	_mvpAwards['mqk']			= {tonumber(GetVal(2, 'quadkill')) or 0, 6}   -- Quad Kill
	_mvpAwards['lgks']			= {tonumber(GetVal(3, 'ks15')) or 0, 5}  -- Ks15
	_mvpAwards['msd']			= {tonumber(GetVal(4, 'smackdown')) or 0, 4}  -- Smackdown
	
	-- Mapped Guesses based on standard Award Slots
	_mvpAwards['mkill']			= {tonumber(GetVal(5, 'mkill')) or 0, 11}  -- Most Kills (Fixed mapping from mvp_score to mkill)
	_mvpAwards['masst']			= {tonumber(GetVal(6, 'most_assists')) or 0, 10}  -- Most Assists
	_mvpAwards['ledth']			= {tonumber(GetVal(7, 'least_deaths')) or 0, 8}   -- Least Deaths
	_mvpAwards['mbdmg']			= {tonumber(GetVal(8, 'most_building_damage')) or 0, 9}  -- Building Damage
	_mvpAwards['mhdd']			= {tonumber(GetVal(10, 'most_hero_damage')) or 0, 3}  -- Hero Damage
	_mvpAwards['hcs']			= {tonumber(GetVal(11, 'most_creeps')) or 0, 2}  -- Creep Kills

	-- MVP Score
	local mvpScore = tonumber(GetVal(5, 'mvp_score')) or 0
	if mvpScore > 0 then _mvpScore = mvpScore end
	
	println('^y[DEBUG] OnPlayerStatsMVPAwardsResult: Annihilation='..tostring(_mvpAwards['mann'][1])..' Score='..tostring(_mvpScore))

	SetOverviewMVPAwardsInfo()
	SetMVPAwardsTabInfo()
end

function Player_Stats_V2:OnPlayerStatsMatchListResult(...)
	local arg = { ... }
	arg.n = select('#', ...)
	local arg = { ... }
	arg.n = #arg
	
	_matchList = {}

	-- Handle Serialized Response (likely from MatchHistoryHandler)
	if type(arg[1]) == 'string' and arg[2] == nil then
		local data = Player_Stats_V2:PHP_Unserialize(arg[1])
		if data then 
			-- Fix 0-based indexing from C# List<string> serialization
			if data[0] ~= nil then
				local fixed = {}
				for k,v in pairs(data) do
					if type(k) == 'number' then fixed[k+1] = v else fixed[k] = v end
				end
				data = fixed
			end
			arg = data 
			if not arg.n then arg.n = #arg end
			println('^gOnPlayerStatsMatchListResult: Unserialized PHP payload.')
		end
	end

	-- K2 Vector/Table Handling:
	-- If arg[1] is a table, we are receiving a vector from the engine.
	-- Otherwise, we are receiving varargs (positional strings).
	local matchListSource = arg
	if type(arg[1]) == 'table' then
		matchListSource = arg[1]
	end

	-- Fix 0-based indexing for matchListSource (e.g. K2 Vector or C# List)
	if matchListSource[0] ~= nil then
		local fixed = {}
		for k,v in pairs(matchListSource) do
			if type(k) == 'number' then fixed[k+1] = v else fixed[k] = v end
		end
		matchListSource = fixed
		println('^gOnPlayerStatsMatchListResult: Fixed 0-based matchListSource.')
	end

	-- Recalculate n after fixing
	if not matchListSource.n then
		local count = 0
		for k,v in pairs(matchListSource) do
			if type(k) == 'number' and k > count then count = k end
		end
		if count == 0 then count = 0 for _ in pairs(matchListSource) do count = count + 1 end end
		matchListSource.n = count
	end

	--println('Matches returned: '..tostring(matchListSource.n))
	
	for i=1, matchListSource.n do
		if not Empty(matchListSource[i]) then
			local matchStr = matchListSource[i]
			Echo('^y[DEBUG] raw match str: ' .. tostring(matchStr))
			local _, _, id, result, team, kills, deaths, assists, heroid, duration, mapname, mdt, heroname = 
				string.find(matchStr, "(.+),(.+),(.+),(.+),(.+),(.+),(.+),(.+),(.+),(.+),(.+)")

			if id then 
				Echo('^g[DEBUG] Match Parsed: id='..id..' map='..tostring(mapname)..' hero='..tostring(heroname))
				local match = {}
				match['id'] 		= id
				match['result'] 	= result
				match['kills'] 		= kills
				match['deaths'] 	= deaths
				match['assists'] 	= assists
				match['heroid'] 	= heroid
				match['duration'] 	= duration
				match['mapname'] 	= mapname
				match['mdt'] 		= mdt
				match['heroname'] 	= heroname
				
				table.insert(_matchList, match)
			else 
				Echo('^r[DEBUG] Match Parse Failed: '..tostring(matchStr)) 
			end
		end
	end
	
	SetPlayerStatsMatchList()
end

function Player_Stats_V2:OnPlayerStatsGetSeasonsResult(...)
	local arg = { ... }
	arg.n = select('#', ...)
	println(arg[1])
	if NotEmpty(arg[1]) then 
		_historicalSeasons = explode('|', arg[1])
	else
		_historicalSeasons = {}
	end

	SetStatsRankTypes()
end



function Player_Stats_V2:OnMasteryHeroInfo(...) 
	local hero = {}
	hero.name = arg[2]
	hero.icon = arg[3]
	hero.level = GetMasteryLevelByExp(tonumber(arg[17]))
	hero.type = GetMasterTypeByLevel(hero.level)
	hero.own = CanAccessHeroProduct(arg[1])
	

	table.insert(_heroMastery, hero)
end

function Player_Stats_V2:OnMasteryClearInfo()
	println('^gMasteryClearInfo triggered!')
	_heroMastery = {}
end

function Player_Stats_V2:OnMasteryFinishInfo()
	--println('^gMasteryFinishInfo triggered!')
	Player_Stats_V2:SetMasteryAllHeroes()
	Player_Stats_V2:HideWaitingMask()
end

function Player_Stats_V2:OnUpdateMasteryInfo(errorCode)
	Player_Stats_V2:HideWaitingMask()
	
	println('^gUpdateMasteryInfo triggered! errorCode: '..errorCode)
	
	if tonumber(errorCode) == 0 then
		SubmitForm('PlayerStatsMastery', 'f', 'show_stats', 'nickname', StripClanTag(_currentNickName), 'cookie', Client.GetCookie(), 'table', 'mastery')
		Cmd('ClientRefreshUpgrades')
		Player_Stats_V2:ShowWaitingMask()
	else
		GetWidget('playerstats_claim_mastery_reward_fail_msg'):SetText(Translate('mastery_error_popup_getreward', 'value', ' '..errorCode))
		GetWidget('playerstats_claim_mastery_reward_fail_mask'):SetVisible(1)
	end
end

function Player_Stats_V2:OnEntityDefinitionsLoaded()
	if not GetWidget('playerstats'):IsVisible() and Empty(_currentNickName) then
		return
	end
	
	GetWidget('playerstats_loadingentity_mask'):SetVisible(false)
	GetWidget('playerstats'):Sleep(1,
		function () 
			SubmitInitialForms()
		end
	)
end

function Player_Stats_V2:OnMatchInfoSummary(...)
	Player_Stats_V2:HideWaitingMask()
	
	if not GetWidget('playerstats_match'):IsVisible() then
		return
	end
	
	if NotEmpty(arg[1]) then
		if arg[1] ~= 'main_stats_retrieving_match' then
			println('^Error: Retrieving match info failed! reason: '..(arg[1] ~= nil and arg[1] or 'unknown'))
		end
		return
	end
	
	if arg[2] and NotEmpty(arg[2]) and tonumber(arg[2]) > 0 then
		GetWidget('match_stats'):FadeIn(250)
	end
end

function Player_Stats_V2:OnSubAccount(...)
	if NotEmpty(tostring(arg[1])) then
		local num = #_subAccounts
		println('^gAdd subaccount '..arg[1]..' index'..tostring(num + 1))
		_subAccounts[num + 1] = StripClanTag(tostring(arg[1]))
		
		local name_color = '#CFC2A6'
		if IsMe(tostring(arg[1])) then
			name_color = '#FFFF00'
		end
		
		local root = GetWidget('playerstats_switch_accounts_list_root')
		root:Instantiate('playerstats_switch_accounts_list_item_template', 
			'row', tostring(num + 1), 
			'account_name', _subAccounts[num + 1],
			'name_color', name_color)
	
		local scrollWidget = GetWidget('playerstats_switch_accounts_list_scroll')
		scrollWidget:SetVisible(1)
		local height = root:GetHeight()
		local height2 = GetWidget('playerstats_switch_accounts_list_clip'):GetHeight()

		if height <= height2 then
			scrollWidget:SetMaxValue(1)
		else
			scrollWidget:SetMaxValue(height - height2 + 50)
		end
		scrollWidget:SetValue(1)
		Player_Stats_V2:UpdateSwitchAccountsList()
	else
		println('^gClear subaccount list.')
		ResetSwitchAccountsListUI()
		_subAccounts = {}
	end
end

function Player_Stats_V2:OnAccountInfo(...)
	println('^g'..arg[1]..'|'..arg[2]..'|'..arg[3])
	if NotEmpty(tostring(arg[2])) then
		local accountname = StripClanTag(tostring(arg[2]))
		for i,v in ipairs(_subAccounts) do
			if accountname == v then
				GetWidget('playerstats_switch_accounts_list_item_name_'..tostring(i)):SetColor('#FFFF00')
			else
				GetWidget('playerstats_switch_accounts_list_item_name_'..tostring(i)):SetColor('#CFC2A6')
			end
		end
	end
end

-----------------------------------------  Functions -----------------------------------------------

function Player_Stats_V2:ShowWaitingMask()
	_waitingMaskCount = _waitingMaskCount + 1
	GetWidget('playerstats_waiting_mask'):SetVisible(true)
end

function Player_Stats_V2:HideWaitingMask()
	_waitingMaskCount = _waitingMaskCount - 1
	if _waitingMaskCount <= 0 then
		GetWidget('playerstats_waiting_mask'):SetVisible(false)
		_waitingMaskCount = 0
	end
end

function Player_Stats_V2:SetFunctionalButtons()
	if _currentNickName == nil then return end
	
	if IsMe(_currentNickName) then
		if not GetCvarBool('cl_GarenaEnable') then
			GetWidget('playerstats_switch_accounts_button'):SetVisible(1)
		else
			GetWidget('playerstats_switch_accounts_button'):SetVisible(0)
		end
		GetWidget('playerstats_social_buttons'):SetVisible(0)
		GetWidget('playerstats_friendship'):SetEnabled(0)
		GetWidget('playerstats_invitegame'):SetEnabled(0)
		--GetWidget('playerstats_chat'):SetEnabled(0)
	else
		GetWidget('playerstats_switch_accounts_button'):SetVisible(0)
		GetWidget('playerstats_social_buttons'):SetVisible(1)
	
		local chatUserOnline = AtoB(interface:UICmd('ChatUserOnline(\''..StripClanTag(_currentNickName)..'\')'))
		local chatInGame = AtoB(interface:UICmd('ChatInGame()'))
		local chatUserInGame = AtoB(interface:UICmd('ChatUserInGame(\''..StripClanTag(_currentNickName)..'\')'))
		local isInQueue = AtoB(interface:UICmd('IsInQueue()'))
		GetWidget('playerstats_invitegame'):SetCallback('onmouseover', function(...) ShowWidget('playerstats_tip_invitegroup') end)
		GetWidget('playerstats_invitegame'):SetCallback('onmouseout', function(...) HideWidget('playerstats_tip_invitegroup') end)
		if not IsMe(_currentNickName) and not chatInGame and chatUserOnline and not chatUserInGame and not isInQueue then	
			GetWidget('playerstats_invitegame'):SetEnabled(1)
		else
			GetWidget('playerstats_invitegame'):SetEnabled(0)
		end
	
		--GetWidget('playerstats_chat'):SetEnabled(1)
		
		if IsBuddy(_currentNickName) then
			GetWidget('playerstats_friendship_up'):SetTexture('/ui/fe2/newui/res/playerstats/delete_friend_up.png')
			GetWidget('playerstats_friendship_over'):SetTexture('/ui/fe2/newui/res/playerstats/delete_friend_hover.png')
			GetWidget('playerstats_friendship_down'):SetTexture('/ui/fe2/newui/res/playerstats/delete_friend_up.png')
			GetWidget('playerstats_friendship'):SetCallback('onmouseover', function(...) ShowWidget('playerstats_tip_removefriend') end)
			GetWidget('playerstats_friendship'):SetCallback('onmouseout', function(...) HideWidget('playerstats_tip_removefriend') end)
			GetWidget('playerstats_friendship'):SetEnabled(1)
		else
			GetWidget('playerstats_friendship_up'):SetTexture('/ui/fe2/newui/res/playerstats/add_friend_up.png')
			GetWidget('playerstats_friendship_over'):SetTexture('/ui/fe2/newui/res/playerstats/add_friend_hover.png')
			GetWidget('playerstats_friendship_down'):SetTexture('/ui/fe2/newui/res/playerstats/add_friend_up.png')
			GetWidget('playerstats_friendship'):SetCallback('onmouseover', function(...) ShowWidget('playerstats_tip_addfriend') end)
			GetWidget('playerstats_friendship'):SetCallback('onmouseout', function(...) HideWidget('playerstats_tip_addfriend') end)
			if not bAddFriendClicked then
				GetWidget('playerstats_friendship'):SetEnabled(1)
			end
		end
	end
	
	GetWidget('playerstats_fetch_player_button'):SetCallback('onmouseover', function(...) ShowWidget('playerstats_tip_fetchplayer') end)
	GetWidget('playerstats_fetch_player_button'):SetCallback('onmouseout', function(...) HideWidget('playerstats_tip_fetchplayer') end)
end

function Player_Stats_V2:Show(nickName)
	_currentNickName = nickName or GetAccountName()
	--_currentNickName = 'snq001'

	if nickName ~= nil then 
		println('^gPlayer_Stats_V2:Show() called with name: '..nickName)
	else
		println('^gPlayer_Stats_V2:Show() called with name: nil')
	end

	Set('_playerstats_match_owner', StripClanTag(_currentNickName))

	if _waitingMaskCount ~= 0 then
		println('^r Error: Player_Stats_V2:Show() - _waitingMaskCount is not 0!!! ^*')
	end
	
	Player_Stats_V2:ResetUI()

	Player_Stats_V2:SetFunctionalButtons()
	
	if not GetCvarBool('_entityDefinitionsLoaded') then
		GetWidget('RegisterEntityDefinitionsHelper'):DoEvent()
		GetWidget('playerstats_loadingentity_mask'):SetVisible(true)
	else
		SubmitInitialForms()
	end
	
	Set('_mainmenu_currentpanel', 'playerstats') 
	if GetWidget('MainMenuPanelSwitcher') ~= nil then
		GetWidget('MainMenuPanelSwitcher'):DoEvent()
	end
	
	GetWidget('playerstats'):FadeIn(150)
end

function Player_Stats_V2:Hide()
	Set('_playerstats_match_owner', '')
	
	_waitingMaskCount = 0
	GetWidget('playerstats_waiting_mask'):SetVisible(0)
	GetWidget('playerstats_claim_mastery_reward_fail_mask'):SetVisible(0)
	
	if GetWidget('playerstats'):IsVisible() then
		Set('_mainmenu_currentpanel', '') 
		GetWidget('playerstats_fetch_player_container'):SetVisible(0)
		GetWidget('playerstats_switch_accounts_list'):FadeOut(150)
		GetWidget('playerstats'):FadeOut(150)
	end
end

function Player_Stats_V2:ResetUI()
	Player_Stats_V2:InitOverviewRankTypes()
	Player_Stats_V2:InitMatchRankTypes()
	
	Player_Stats_V2:OnClickTab('overview')

	GetWidget('playerstats_mastery_overallrewards'):SetVisible(false)
	GetWidget('playerstats_overview_ranktype'):SetSelectedItemByValue(0, false)
	GetWidget('playerstats_mastery_showoverall'):SetVisible(IsMe(_currentNickName))
	GetWidget('playerstats_mastery_new_reward_flag'):SetVisible(IsMe(_currentNickName))
	GetWidget('playerstats_stats_ranktype'):SetSelectedItemByValue(-1, false)

	_waitingMaskCount = 0
	GetWidget('playerstats_waiting_mask'):SetVisible(false)
	
	bAddFriendClicked = false
	
	ResetOverviewUI()
	
	ResetStatsTabUI()
	
	ResetMatchListUI()
	
	GetWidget('playerstats_switch_accounts_list'):SetVisible(false)
	GetWidget('playerstats_switch_account_confirm_mask'):SetVisible(false)
end

function Player_Stats_V2:SetGeneralInfo(info)
	if not info then
		println('^r[ERROR] SetGeneralInfo called with nil info table!')
	end
	local nickname = info.nickname
	local clanname = info.clanname
	local accountid = info.accountid
	local lastmatchdate = info.lastmatchdate
	local createdate = info.createdate
	local selected_upgrade = info.selected_upgrade
	local level = info.level
	local level_exp = info.level_exp
	local matches = info.matches
	local disconnects = info.disconnects
	local standing = info.standing

	--------------------------------------------------------------------------------------
	local widget = GetWidget('playerstats_playericon')
	widget:SetTexture('/ui/common/ability_coverup.tga')

	local playerIcon = GetAccountIconTexturePathFromUpgrades(selected_upgrade, accountid)
	if NotEmpty(playerIcon) then
		widget:SetTexture(playerIcon)
	else
		widget:SetAvatar('http://www.heroesofnewerth.com/getAvatar.php?id='..accountid)
	end

	--------------------------------------------------------------------------------------
 	widget = GetWidget('playerstats_playername')
	local nameColor = GetChatNameColorStringFromUpgrades(selected_upgrade)
	local nameColorFont = GetChatNameColorFontFromUpgrades(selected_upgrade)
	local font = NotEmpty(nameColorFont) and nameColorFont..'_16' or 'dyn_16'

	local playername = StripClanTag(nickname)
	widget:SetFont(font)
	widget:SetColor(NotEmpty(nameColor) and nameColor or '#efd2c0')
	widget:SetGlow(GetChatNameGlowFromUpgrades(selected_upgrade))
	widget:SetGlowColor(GetChatNameGlowColorStringFromUpgrades(selected_upgrade))
	widget:SetBackgroundGlow(GetChatNameBackgroundGlowFromUpgrades(selected_upgrade))
	widget:SetText(font == '8bit_16' and string.upper(nickname) or nickname)

	local standing_x = widget:GetAbsoluteX() + widget:GetWidth() + 5
	widget = GetWidget('playerstats_playerstanding')
	widget:SetAbsoluteX(standing_x)
	if not GetCvarBool('cl_GarenaEnable') then
		if standing >= GetCvarInt('accountStanding_verifiedThreshold') or standing == 0 then
			widget:SetTexture('/ui/icons/verified.tga')
			widget:SetCallback('onmouseover', function(...) ShowWidget('playerstats_tip_standing_verified') end)
			widget:SetCallback('onmouseout', function(...) HideWidget('playerstats_tip_standing_verified') end)
		else
			widget:SetTexture('/ui/icons/basic.tga')
			widget:SetCallback('onmouseover', function(...) ShowWidget('playerstats_tip_standing_basic') end)
			widget:SetCallback('onmouseout', function(...) HideWidget('playerstats_tip_standing_basic') end)
		end
		widget:SetVisible(1)
	else
		widget:SetVisible(0)
		widget:ClearCallback('onmouseover')
		widget:ClearCallback('onmouseout')
	end
	
	local isClanMember = AtoB(interface:UICmd('IsClanMember(\''..playername..'\')'))
	local isUserLeader = AtoB(interface:UICmd('ChatIsClanLeader(\''..playername..'\')'))
	local isUserOfficer = AtoB(interface:UICmd('ChatIsClanOfficer(\''..playername..'\')'))
	
	widget = GetWidget('playerstats_clanname')
	if NotEmpty(clanname) then
		widget:SetText(Translate('newui_playerstats_clanname')..clanname)
	else
		widget:SetText('')
	end
	
	local clanrole_x = widget:GetAbsoluteX() + widget:GetWidth() + 5
	widget = GetWidget('playerstats_clanrole')
	widget:SetAbsoluteX(clanrole_x)
	if isUserLeader then
		widget:SetTexture('/ui/fe2/newui/res/social/clanleader.png')
	elseif isUserOfficer then
		widget:SetTexture('/ui/fe2/newui/res/social/clanofficer.png')
	else
		widget:SetTexture('$invis')
	end

	--------------------------------------------------------------------------------------
	widget = GetWidget('playerstats_playerlevel')
	widget:SetText(Translate('newui_playerstats_level', 'level', level))

	--------------------------------------------------------------------------------------
	widget = GetWidget('playerstats_chatsymbol')
	local chatSymbol = GetChatSymbolTexturePathFromUpgrades(selected_upgrade)
	nameColor = GetChatNameColorTexturePathFromUpgrades(selected_upgrade)

	if NotEmpty(chatSymbol) then
		widget:SetTexture(chatSymbol)
	elseif NotEmpty(nameColor) then
		widget:SetTexture(nameColor)
	else
		widget:SetTexture('$invis')
	end
	--------------------------------------------------------------------------------------
	local levelExp = tonumber(interface:UICmd('GetAccountExperienceForLevel('..level..')')) or 0
	local nextLevelExp = tonumber(interface:UICmd('GetAccountExperienceForNextLevel(\''..level_exp..'\')')) or 0
	local nextLevelPercent = tonumber(interface:UICmd('GetAccountPercentNextLevel(\''..level_exp..'\')')) or 0
	GetWidget('playerstats_levelprogress'):SetWidth(tostring(nextLevelPercent*100)..'%')
	GetWidget('playerstats_levelexp'):SetText(tostring(level_exp - levelExp)..'/'..tostring(nextLevelExp - levelExp))

	--------------------------------------------------------------------------------------
	local leaves = 0
	if matches > 0 then
		leaves = disconnects / matches * 100
	end
	GetWidget('playerstats_matches'):SetText(tostring(matches))
	GetWidget('playerstats_disconnects'):SetText(tostring(disconnects))
	GetWidget('playerstats_leaves'):SetText(string.format('%.1f', leaves)..'%')

	--------------------------------------------------------------------------------------
	GetWidget('playerstats_createdate'):SetText(tostring(createdate))
	GetWidget('playerstats_lastmatchdate'):SetText(tostring(lastmatchdate))
end

function Player_Stats_V2:SetMasteryAllHeroes()
	local function sortbylevel(a, b)
		local level_a = tonumber(a.level) or 0
		local level_b = tonumber(b.level) or 0
		return level_a > level_b
	end

	table.sort(_heroMastery, sortbylevel)

	local masteryScore = 0
	local rowCount = math.ceil(#_heroMastery/MASTER_ALLHERO_COLUMN)
	for i=0, MASTER_ALLHERO_ROW-1 do
		GetWidget('playerstats_mastery_all_row_'..i):SetVisible(i <= rowCount)
		for j=0, MASTER_ALLHERO_COLUMN-1 do
			local index = i * MASTER_ALLHERO_COLUMN + j + 1
			local hero = _heroMastery[index]

			if hero ~= nil then
				GetWidget('playerstats_mastery_all_item_'..i..'_'..j):SetVisible(true)
				GetWidget('playerstats_mastery_all_item_'..i..'_'..j..'_name'):SetText(hero.name or '---')
				if hero.type == 'goldenred' then
					GetWidget('playerstats_mastery_all_item_'..i..'_'..j..'_name'):SetOutlineColor('#331038')
					GetWidget('playerstats_mastery_all_item_'..i..'_'..j..'_level'):SetOutlineColor('#331038')
				elseif hero.type == 'gold' then
					GetWidget('playerstats_mastery_all_item_'..i..'_'..j..'_name'):SetOutlineColor('#803705')
					GetWidget('playerstats_mastery_all_item_'..i..'_'..j..'_level'):SetOutlineColor('#803705')
				elseif hero.type == 'silver' then
					GetWidget('playerstats_mastery_all_item_'..i..'_'..j..'_name'):SetOutlineColor('#202121')
					GetWidget('playerstats_mastery_all_item_'..i..'_'..j..'_level'):SetOutlineColor('#202121')
				else
					GetWidget('playerstats_mastery_all_item_'..i..'_'..j..'_name'):SetOutlineColor('#241410')
					GetWidget('playerstats_mastery_all_item_'..i..'_'..j..'_level'):SetOutlineColor('#241410')
				end
				GetWidget('playerstats_mastery_all_item_'..i..'_'..j..'_level'):SetText(tostring(hero.level) or '0')
				GetWidget('playerstats_mastery_all_item_'..i..'_'..j..'_icon'):SetTexture(hero.icon or '$invis')

				if hero.own then
					GetWidget('playerstats_mastery_all_item_'..i..'_'..j..'_border'):SetTexture('/ui/fe2/newui/res/playerstats/border_'..hero.type..'.png')
					GetWidget('playerstats_mastery_all_item_'..i..'_'..j..'_icon'):SetRenderMode('normal')
				else
					GetWidget('playerstats_mastery_all_item_'..i..'_'..j..'_border'):SetTexture('/ui/fe2/newui/res/playerstats/border_unowned.png')
					GetWidget('playerstats_mastery_all_item_'..i..'_'..j..'_icon'):SetRenderMode('grayscale')
				end
				
				masteryScore = masteryScore + hero.level
			else
				GetWidget('playerstats_mastery_all_item_'..i..'_'..j):SetVisible(false)
			end
		end
	end

	GetWidget('playerstats_mastery_mastery_score'):SetText(tostring(masteryScore))
	GetWidget('playerstats_overview_mastery_score'):SetText(tostring(masteryScore))
	
	local scrollWidget = GetWidget('playerstats_mastery_scroll')
	local height = GetWidget('playerstats_mastery_allheroes_root'):GetHeight()
	local height2 = GetWidget('playerstats_mastery_allheroes_clip'):GetHeight()

	if height <= height2 then
		scrollWidget:SetMaxValue(1)
	else
		scrollWidget:SetMaxValue(height - height2)
	end
	scrollWidget:SetValue(1)
	Player_Stats_V2:UpdateAllHeroesMastery()
end

function Player_Stats_V2:ClearMasteryRewardsInfo(levelRewards)
	for i, _ in ipairs(levelRewards) do
		local widget = GetWidget('playerstats_mastery_level_reward_'..i..'_number')
		if (widget) then
			widget:SetText('-')
		end
	end
end

function Player_Stats_V2:SetMasteryRewardsInfo(heroMastery, rewards, levelRewards)
	for i,v in ipairs(levelRewards) do
		local widget = GetWidget('playerstats_mastery_level_reward_'..i..'_number')
		if (widget) then
			widget:SetText(tostring(v))
		end
	end

	local totalLevel = 0
	for _,v in ipairs(heroMastery) do
		if v ~= nil and v.level ~= nil then
			totalLevel = totalLevel + (tonumber(v.level) or 0)
		end
	end
	
	local scoreWidget = GetWidget('playerstats_mastery_mastery_score')
	if scoreWidget then scoreWidget:SetText(tostring(totalLevel)) end
	
	bHasUnclaimedRewards = false
	
	for i=1, MASTER_OVERALL_REWARDS_MAX do
		local reward = rewards[i]

		if reward ~= nil then
			GetWidget('playerstats_mastery_overall_reward_'..i):SetVisible(true)

			GetWidget('playerstats_mastery_overall_reward_'..i..'_icon'):SetTexture(reward.icon)
			GetWidget('playerstats_mastery_overall_reward_'..i..'_text'):SetText(reward.text)
			GetWidget('playerstats_mastery_overall_reward_'..i..'_level'):SetText(Translate('newui_playerstats_level2', 'level', tostring(reward.level)))
			GetWidget('playerstats_mastery_overall_reward_'..i..'_mask'):SetVisible(reward.taken)

			if reward.taken then
				GetWidget('playerstats_mastery_overall_reward_'..i..'_claim_icon'):SetTexture('/ui/fe2/newui/res/playerstats/reward_claimed.png')
				GetWidget('playerstats_mastery_overall_reward_'..i..'_bg'):SetVisible(false)
				GetWidget('playerstats_mastery_overall_reward_'..i..'_icon'):SetRenderMode('grayscale')
				GetWidget('playerstats_mastery_overall_reward_'..i..'_text'):SetColor('#ac9180')
				GetWidget('playerstats_mastery_overall_reward_'..i..'_level'):SetColor('#906b5b')
				GetWidget('playerstats_mastery_overall_reward_'..i..'_body'):ClearCallback('onclick')
			elseif totalLevel >= reward.level then
				GetWidget('playerstats_mastery_overall_reward_'..i..'_claim_icon'):SetTexture('/ui/fe2/newui/res/playerstats/reward_to_claim.png')
				GetWidget('playerstats_mastery_overall_reward_'..i..'_bg'):SetVisible(true)
				GetWidget('playerstats_mastery_overall_reward_'..i..'_icon'):SetRenderMode('normal')
				GetWidget('playerstats_mastery_overall_reward_'..i..'_text'):SetColor('#feae00')
				GetWidget('playerstats_mastery_overall_reward_'..i..'_level'):SetColor('#f0d3c1')
				GetWidget('playerstats_mastery_overall_reward_'..i..'_value'):SetText(tostring(reward.level))
				GetWidget('playerstats_mastery_overall_reward_'..i..'_body'):SetCallback('onclick', function(widget) Player_Stats_V2:OnClickClaimMasteryReward(tostring(i)) end)
				bHasUnclaimedRewards = true
			else
				--println('^yInfo: playerstats_mastery_overall_reward_'..i..' is neither taken nor available!!! reward.level = '..tostring(reward.level)..'^*')
				GetWidget('playerstats_mastery_overall_reward_'..i..'_claim_icon'):SetTexture('$invis')
				GetWidget('playerstats_mastery_overall_reward_'..i..'_bg'):SetVisible(true)
				GetWidget('playerstats_mastery_overall_reward_'..i..'_icon'):SetRenderMode('normal')
				GetWidget('playerstats_mastery_overall_reward_'..i..'_text'):SetColor('#ac9180')
				GetWidget('playerstats_mastery_overall_reward_'..i..'_level'):SetColor('#906b5b')
				GetWidget('playerstats_mastery_overall_reward_'..i..'_body'):ClearCallback('onclick')
			end
		else
			GetWidget('playerstats_mastery_overall_reward_'..i):SetVisible(false)
		end
	end

	if bHasUnclaimedRewards and IsMe(_currentNickName) then
		GetWidget('playerstats_mastery_new_reward_flag'):SetVisible(1)
	else
		GetWidget('playerstats_mastery_new_reward_flag'):SetVisible(0)
	end
	
	local scrollWidget = GetWidget('playerstats_mastery_overallrewards_scroll')
	local height = GetWidget('playerstats_mastery_overallrewards_root'):GetHeight()
	local height2 = GetWidget('playerstats_mastery_overallrewards_clip'):GetHeight()

	if height <= height2 then
		scrollWidget:SetMaxValue(1)
	else
		scrollWidget:SetMaxValue(height - height2)
	end
	scrollWidget:SetValue(1)
	Player_Stats_V2:UpdateMasteryOverallRewards()
end

function Player_Stats_V2:UpdateAllHeroesMastery()
	local scrollWidget = GetWidget('playerstats_mastery_scroll')
	local startIndex = tonumber(scrollWidget:GetValue())
	local maxValue = tonumber(scrollWidget:UICmd("GetScrollbarMaxValue()"))
	if startIndex < 1 then startIndex = 1 end
	if startIndex > maxValue then startIndex = maxValue end

	GetWidget('playerstats_mastery_allheroes_root'):SetY('-'..tostring(startIndex-1))
end

function Player_Stats_V2:UpdateMasteryOverallRewards()
	local scrollWidget = GetWidget('playerstats_mastery_overallrewards_scroll')
	local startIndex = tonumber(scrollWidget:GetValue())
	local maxValue = tonumber(scrollWidget:UICmd("GetScrollbarMaxValue()"))
	if startIndex < 1 then startIndex = 1 end
	if startIndex > maxValue then startIndex = maxValue end

	GetWidget('playerstats_mastery_overallrewards_root'):SetY('-'..tostring(startIndex-1))
end

function Player_Stats_V2:UpdateMatchList()
	local scrollWidget = GetWidget('playerstats_match_list_scroll')
	local startIndex = tonumber(scrollWidget:GetValue())
	local maxValue = tonumber(scrollWidget:UICmd("GetScrollbarMaxValue()"))
	if startIndex < 1 then startIndex = 1 end
	if startIndex > maxValue then startIndex = maxValue end

	GetWidget('playerstats_match_list_root'):SetY('-'..tostring(startIndex-1))
end

function Player_Stats_V2:UpdateSwitchAccountsList()
	local scrollWidget = GetWidget('playerstats_switch_accounts_list_scroll')
	local startIndex = tonumber(scrollWidget:GetValue())
	local maxValue = tonumber(scrollWidget:UICmd("GetScrollbarMaxValue()"))
	if startIndex < 1 then startIndex = 1 end
	if startIndex > maxValue then startIndex = maxValue end

	GetWidget('playerstats_switch_accounts_list_root'):SetY('-'..tostring(startIndex-1))
end

-----------------------------------------  UI Message Handler -----------------------------------------
function Player_Stats_V2:OnClickTab(type)
	println('^c[PlayerStats] ^*Switching Tab to: ' .. tostring(type))
	GetWidget('playerstats_overview'):SetVisible(type == 'overview')
	GetWidget('playerstats_mastery'):SetVisible(type == 'mastery')
	GetWidget('playerstats_mvpawards'):SetVisible(type == 'mvpawards')
	GetWidget('playerstats_stats'):SetVisible(type == 'stats')
	GetWidget('playerstats_match'):SetVisible(type == 'match')

	GetWidget('playerstats_tab_overview'):SetEnabled(type ~= 'overview')
	GetWidget('playerstats_tab_mastery'):SetEnabled(type ~= 'mastery')
	GetWidget('playerstats_tab_mvpawards'):SetEnabled(type ~= 'mvpawards')
	GetWidget('playerstats_tab_stats'):SetEnabled(type ~= 'stats')
	GetWidget('playerstats_tab_match'):SetEnabled(type ~= 'match')

	_currentTab = type
	
	if type == 'mastery' and IsMe(_currentNickName) then
		Cmd('GetMasteryHeroInfo')
		Player_Stats_V2:ShowWaitingMask()
	end
	
	if type == 'stats' then
		local valueSaved = GetCvarString('_playerstats_stats_type')
		if not NotEmpty(valueSaved) then
			if GetCvarBool('cl_GarenaEnable') then
				valueSaved = '1'
			else
				valueSaved = '0'
			end
		end
		local value = GetWidget('playerstats_stats_ranktype'):GetValue()
		if value ~= valueSaved then
			GetWidget('playerstats_stats_ranktype'):SetSelectedItemByValue(valueSaved, false)
			--have to be like this, SetSelectedItemByValue(event=true) will fire the selection event twice
			Player_Stats_V2:OnSelectStatsRankType()
		end
	end
	
	if type == 'match' then
		local valueSaved = GetCvarString('_playerstats_match_type')
		if not NotEmpty(valueSaved) then
			if GetCvarBool('cl_GarenaEnable') then
				valueSaved = '1'
			else
				valueSaved = '0'
			end
		end
		local value = GetWidget('playerstats_match_ranktype'):GetValue()
		if value ~= valueSaved then
			GetWidget('playerstats_match_ranktype'):SetSelectedItemByValue(valueSaved, false)
			--have to be like this, SetSelectedItemByValue(event=true) will fire the selection event twice
			Player_Stats_V2:OnSelectMatchRankType()
		end
	end
end

function Player_Stats_V2:OnSelectOverviewRankType()
	local value = GetWidget('playerstats_overview_ranktype'):GetValue()
	
	local action = _overviewRankTypeActions[value]
	if action == nil then return end
	
	if value == '0' then
		if not bNormalSeasonStatsRetrieved then
			SubmitForm(action.form, 'f', action.f, 'nickname', action.nickname, 'cookie', action.cookie, 'table', action.table)
			Player_Stats_V2:ShowWaitingMask()
		else
			Player_Stats_V2:OnPlayerStatsNormalSeasonResult()
		end
		SetSave('_playerstats_overview_type', value, 'string')
		return
	end
	
	if value == '1' then
		if not bCasualSeasonStatsRetrieved then
			SubmitForm(action.form, 'f', action.f, 'nickname', action.nickname, 'cookie', action.cookie, 'table', action.table)
			Player_Stats_V2:ShowWaitingMask()
		else
			Player_Stats_V2:OnPlayerStatsCasualSeasonResult()
		end
		SetSave('_playerstats_overview_type', value, 'string')
		return
	end
end

-- Logan (2026-02-12): Stub/Port of RewardStat_OnEntityDefinitionsLoaded from match_stats_v2.lua
-- This is required because rewardstats_loadingentity_mask is used in the Profile Page but match_stats_v2.lua is not loaded.
function RewardStat_OnEntityDefinitionsLoaded()
	println('^g[Profile] RewardStat_OnEntityDefinitionsLoaded Triggered (Stub/Port)')
	
	local mask = GetWidget('rewardstats_loadingentity_mask')
	if mask then
		mask:SetVisible(false)
	end

	-- Request Recent Games (Matches) for the Profile History
	-- Note: exact arguments might differ, but this matches match_stats_v2.lua logic
	SubmitForm('PlayerRecentGamesList', 'f', 'grab_last_matches_from_nick', 'nickname', StripClanTag(GetAccountName()), 'hosttime', HostTime())	
end

function Player_Stats_V2:OnSelectStatsRankType()
	local value = GetWidget('playerstats_stats_ranktype'):GetValue()
	
	local action = _statsRankTypeActions[value]
	if action == nil then return end
	
	if value == '0' then
		if not bNormalSeasonStatsRetrieved then
			SubmitForm(action.form, 'f', action.f, 'nickname', action.nickname, 'cookie', action.cookie, 'table', action.table)
			Player_Stats_V2:ShowWaitingMask()
		else
			Player_Stats_V2:OnPlayerStatsNormalSeasonResult()
		end
	end
	
	if value == '1' then
		if not bCasualSeasonStatsRetrieved then
			SubmitForm(action.form, 'f', action.f, 'nickname', action.nickname, 'cookie', action.cookie, 'table', action.table)
			Player_Stats_V2:ShowWaitingMask()
		else
			Player_Stats_V2:OnPlayerStatsCasualSeasonResult()
		end
	end
	
	if value == '2' then
		if not bNormalStatsRetrieved then
			SubmitForm(action.form, 'f', action.f, 'nickname', action.nickname, 'cookie', action.cookie, 'table', action.table)
			Player_Stats_V2:ShowWaitingMask()
		else
			Player_Stats_V2:OnPlayerStatsNormalResult()
		end
	end
	
	if value == '3' then
		if not bCasualStatsRetrieved then
			SubmitForm(action.form, 'f', action.f, 'nickname', action.nickname, 'cookie', action.cookie, 'table', action.table)
			Player_Stats_V2:ShowWaitingMask()
		else
			Player_Stats_V2:OnPlayerStatsCasualResult()
		end
	end
	
	if value == '4' then
		if not bPublicStatsRetrieved then
			SubmitForm(action.form, 'f', action.f, 'nickname', action.nickname, 'cookie', action.cookie, 'table', action.table)
			Player_Stats_V2:ShowWaitingMask()
		else
			Player_Stats_V2:OnPlayerStatsPublicResult()
		end
	end
	
	-- historical season stats
	if tonumber(value) > 4 then 
		SubmitForm(action.form, 'f', action.f, 'nickname', action.nickname, 'cookie', action.cookie, 'table', action.table, 'season_id', action.seasonid, 'is_casual', action.iscasual)
		Player_Stats_V2:ShowWaitingMask()
	end

	SetSave('_playerstats_stats_type', value, 'string')
end

function Player_Stats_V2:OnSelectMatchRankType()
	-- clear list items
	ResetMatchListUI()

	local value = GetWidget('playerstats_match_ranktype'):GetValue()
	local action = _matchRankTypeActions[value]
	if action == nil then return end

	SubmitForm(action.form, 'f', action.f, 'nickname', action.nickname, 'cookie', action.cookie, 'table', action.table, 'num', action.num, 'current_season', action.curseason)
	Player_Stats_V2:ShowWaitingMask()
	
	SetSave('_playerstats_match_type', value, 'string')
end

function Player_Stats_V2:OnClickClaimMasteryReward(index)
	local level = GetWidget('playerstats_mastery_overall_reward_'..index..'_value'):GetText()
	-- Fire our new C# ASP.NET Core endpoint instead of the dead C++ command
	SubmitForm('ClaimMasteryReward', 'f', 'claim_mastery_reward', 'level', level, 'nickname', _publicStats.nickname, 'cookie', Client.GetCookie())
	Player_Stats_V2:ShowWaitingMask()
end

function Player_Stats_V2:OnClaimMasteryRewardResult(sourceWidget, reward_status)
    Player_Stats_V2:HideWaitingMask()
    println('^g[PlayerStats] OnClaimMasteryRewardResult Triggered. Status: ' .. tostring(reward_status))
    
    if reward_status == 'OK' then
        -- Refresh the Mastery information explicitly from the server.
        Cmd('GetMasteryHeroInfo')
        Player_Stats_V2:ShowWaitingMask()
    else
        GetWidget('playerstats_claim_mastery_reward_fail_mask'):FadeIn(150)
        GetWidget('playerstats_claim_mastery_reward_fail_mask'):BringToFront()
    end
end

function Player_Stats_V2:OnClickMatchDetail(match_id)
	println('Match List Clicked, matchid = '..match_id)
	interface:UICmd("ClearEndGameStats();")
	RewardStat_GetMatchInfo(match_id)
	Player_Stats_V2:ShowWaitingMask()
end


function Player_Stats_V2:OnClickSwitchAccountButton()
	local widget = GetWidget('playerstats_switch_accounts_list')
	if widget:IsVisible() then
		widget:FadeOut(150)
	else
		widget:FadeIn(150)
		widget:BringToFront()
	end
end

function Player_Stats_V2:OnClickSwitchAccount(row)
	GetWidget('playerstats_switch_accounts_list'):FadeOut(250)
	
	local sub_account_name = _subAccounts[tonumber(row)]
	if Empty(sub_account_name) then return end
	
	println('^gSwitching to Account '..sub_account_name)
	_subAccountSwitching = sub_account_name
	
	if GetCvarBool('subswitch_dontask') then
		SwitchToSubAccount()
		Player_Stats_V2:Hide()
	else
		GetWidget('playerstats_switch_account_confirm_mask'):FadeIn(250)
		GetWidget('playerstats_switch_account_confirm_mask'):BringToFront()
	end
end

function Player_Stats_V2:OnClickedConfirmSwitchAccountCancel()
	_subAccountSwitching = nil
	GetWidget('playerstats_switch_account_confirm_mask'):FadeOut(250)
end

function Player_Stats_V2:OnClickedConfirmSwitchAccountOk()
	SwitchToSubAccount()
	GetWidget('playerstats_switch_account_confirm_mask'):FadeOut(250)
	Player_Stats_V2:Hide()
end

function Player_Stats_V2:OnClickModifyFriendship()
	if _currentNickName == nil then return end
	
	if not IsMe(_currentNickName) then
		if not IsBuddy(_currentNickName) then
			Common_V2:AddFriend(_currentNickName)
			bAddFriendClicked = true
		else
			Common_V2:RemoveFriend(_currentNickName)
			bAddFriendClicked = false
		end
		GetWidget('playerstats_friendship'):SetEnabled(0)
	end
end

function Player_Stats_V2:OnClickStartChatWith()
	if _currentNickName == nil then return end
	
	if not IsMe(_currentNickName) then	
		Communicator_V2:StartChatWithPlayer(_currentNickName, true)
		Player_Stats_V2:Hide()
	end	
end

function Player_Stats_V2:OnClickInviteToGroup()
	if _currentNickName == nil then return end
	
	local chatUserOnline = AtoB(interface:UICmd('ChatUserOnline(\''..StripClanTag(_currentNickName)..'\')'))
	local chatInGame = AtoB(interface:UICmd('ChatInGame()'))
	local chatUserInGame = AtoB(interface:UICmd('ChatUserInGame(\''..StripClanTag(_currentNickName)..'\')'))
	local isInQueue = AtoB(interface:UICmd('IsInQueue()'))
	
	if not IsMe(_currentNickName) and not chatInGame and chatUserOnline and not chatUserInGame and not isInQueue then	
		Teammaking_V2:InvitePlayer(_currentNickName)
		GetWidget('playerstats_invitegame'):SetEnabled(0)
	end	
end

function Player_Stats_V2:OpenTourStats()
	Player_Stats_V2:Hide()
	local playername = StripClanTag(_currentNickName)
	local url = HoN_Region:GetPlayerTourStats() .. '&nickname=' .. playername
	Echo('Player_Stats.OpenTourStats url: '..url)

	UIManager.GetInterface('webpanel'):HoNWebPanelF('ShowPlayerTourStats', GetWidget('web_browser_player_tour_stats_insert'), url)
	UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'player_tour_stats', nil, false)
end

function Player_Stats_V2:SearchPlayer()
	local txtbox = GetWidget('playerstats_fetch_player_container')
	if txtbox == nil then return end
	
	if txtbox:IsVisible() then
		local playername = GetWidget('playerstats_fetch_player'):GetValue()
		if NotEmpty(playername) then
			Set('_playerstats_playername_fetch', playername) 
			GetWidget('playerstats_fetch_player'):DoEventN(0)
		end
		txtbox:FadeOut(150)
	else
		txtbox:FadeIn(150)
		GetWidget('playerstats_fetch_player'):SetFocus(true)
	end
end



