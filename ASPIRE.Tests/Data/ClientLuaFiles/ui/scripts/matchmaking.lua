----------------------------------------------------------
--	Name: 		Matchmaking Script	            		--
--  Copyright 2015 Frostburn Studios					--
----------------------------------------------------------

local DefaultEnemyBots = {
	"ChronosBot",
	"ArachnaBot",
	"DefilerBot",
	"HammerstormBot",
	"WitchSlayerBot"
}

local DefaultTeamBots = {
	"ForsakenArcherBot",
	"DSBot",
	"MagmusBot",
	"GlaciusBot"
}

local ALLOW_DUPE_BOTS = false
local ALLOW_DUPE_HERO = false 	-- if the above is true this doesn't matter, allows the same hero, but with different bot scripts
-----------------------------------------------------------

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()
HoN_Matchmaking = _G['HoN_Matchmaking'] or {}
local triggerHelper = GetWidget("TMM_TriggerHelper")
RegisterScript2('Matchmaking', '38')

HoN_Matchmaking.bots = nil -- this is set in the init so we can mark the bots used if needed
HoN_Matchmaking.teamHasBots = false
HoN_Matchmaking.botPickTable = nil
HoN_Matchmaking.repickingSlot = {}
HoN_Matchmaking.multibotPickOffset = {}
HoN_Matchmaking.botPickScrollOffsets = {['left'] = 0, ['right'] = 0}
HoN_Matchmaking.multibotPickScrollOffsets = {['left'] = 0, ['right'] = 0}

HoN_Matchmaking.buddyLists = nil
HoN_Matchmaking.userInfoTable = {}
HoN_Matchmaking.inviteAutoCompleteAddOffset = 1
HoN_Matchmaking.invitePersonAutocompleteTable = {}
HoN_Matchmaking.invitePersonScrollOffset = 0
HoN_Matchmaking.invitedPersons = nil
HoN_Matchmaking.selectedTab = ""
HoN_Matchmaking.teamMembers = {}
HoN_Matchmaking.teamOffset = 2

HoN_Matchmaking.lastHeroInfoLookUp = {}
HoN_Matchmaking.botsTable = nil
-- HoN_Matchmaking.simpleModes = nil
-- HoN_Matchmaking.simpleMaps = nil
HoN_Matchmaking.suppressDCDiag = false

HoN_Matchmaking.TabClickInteval = 700
HoN_Matchmaking.TabClickTime = 0

HoN_Matchmaking.popupShow = true
HoN_Matchmaking.popupTimer = 0

HoN_Matchmaking.validTeamMember = 0


-- Note this is copied from cpp "enum ETMMGameTypes"
local TMM_GAME_TYPE_NORMAL = 1
local TMM_GAME_TYPE_CASUAL = 2
local TMM_GAME_TYPE_MIDWARS = 3
local TMM_GAME_TYPE_RIFTWARS = 4
local TMM_GAME_TYPE_CUSTOM = 5
local TMM_GAME_TYPE_CAMPAIGN_NORMAL = 6
local TMM_GAME_TYPE_CAMPAIGN_CASUAL = 7

local TMM_GAME_INVALID_MEMBER_COUNT = 0

-- default matchmaking options
-- gamemodes and the such will be generated from info that is elsewhere
local matchmakingOptions = HoN_Database:ReadDBEntry('matchmakingOptions') or
{
	-- general stuff
	['tab']				= "coop",
	-- coop stuff
	['botDifficulty'] 	= GetCvarInt("g_botDifficulty"),
	['randomEnemyBots']	= "true",
	-- pvp stuff
	['gameType'] 		= "3",
	['map']				= "midwars",
	--['ranked']			= tostring(IsEligibleForRanked()), -- ran at init, because we have account info then
}
local function FlushMatchmakingOptions()
	HoN_Database:SetDBEntry('matchmakingOptions', matchmakingOptions)
end

--[[ -----

local function TriggerPrint(trigger, sourceWidget, ...)
	print("^rTrigger:^g "..trigger.."\n")
	for i = 1, select("#", ...) do
		local param = select(i, ...)
		print("	^rparam"..(i-1)..":^o "..type(param).." - "..tostring(param).."\n")
	end
end

--
local trigger = {
--~ 	"GamePhase",
--~ 	"HostErrorMessage",
--~ 	"InfosRefreshed",
--~ 	"MatchMakerStatus",
--	"MatchmakingGroupErrorUpdate",
--	"MMBuySingleTokenStatus",
--	"MMBuySingleTokenResults",
--	"MMBuyTokenStatus",
--~ 	"TMMAvailable",
--~ 		"TMMDisplay",
--~ 	"TMMDisplayPopularity",
--~ 	"TMMFoundMatch",
--~ 	"TMMFoundServer",
--~ 	"TMMJoinMatch",
--~ 	"TMMLeaveGroup",
--~ 	"TMMLeftQueue",
--~ 	"TMMNoMatchesFound",
--~ 	"TMMNoServersFound",
--~ 	"TMMOptionsAvailable",
--~ 	"TMMPlayerStatus0",
--~ 	"TMMPlayerStatus1",
--~ 	"TMMPlayerStatus2",
--~ 	"TMMPlayerStatus3",
--~ 	"TMMPlayerStatus4",
--~ 	"TMMReadyStatus",
--~ 		"TMMReset",
--~ 		"TMMTime",
--	"TMMUpdateTooltipTrigger",
--~ 	"UIUpdateRegion",
--~ 	"UpgradesRefreshed",
--	"WarnUnverifiedTrigger",
}

for _, v in pairs(trigger) do
	triggerHelper:RegisterWatch(v, function(...) TriggerPrint(v, ...) end)
end
--]] -----

function interface:HoNMatchmakingF(func, ...)
	if (HoN_Matchmaking[func]) then
		print(HoN_Matchmaking[func](HoN_Matchmaking, ...))
	else
		print('HoNMatchmakingF failed to find: ' .. tostring(func) .. '\n')
	end
end

-- constants
local RULESET_INFO = {
	-- Standard
	[0] = {
		ALLOWED_GAMETYPE = {
			[1] = true,
			[2] = true,
			[3] = true,
			[4] = true,
			[5] = true,
		},
		ALLOWED_GAMEMODE = {
			ap = true,
			sd = true,
			bd = false,
			bp = true,
			ar = true,
			br = true,
			lp = true,
			bb = true,
			gt = false,
			apg = true,
			bbg = true,
			km = true,
			apd = true,
			bbr = true,
			bdr = true,
			cp = true,
			fp = true,
			sp = true,
			ss = true,
			sm = true,
			mwb = true,
			hb = true,
			rb = true
		},
		MIN_TEAM_SIZE = 1,
		PANEL_STYLE = 0,
		ALLOW_TOURNAMENT = true,
		VERIFIED_ONLY = 2,
	},
	-- Tournament
	[1] = {
		ALLOWED_GAMETYPE = {
			[1] = true,
			[2] = true,
			[3] = false,
			[4] = false,
			[5] = false,
		},
		ALLOWED_GAMEMODE = {
			ap = true,
			sd = true,
			bd = true,
			bp = true,
			ar = true,
			br = true,
			lp = true,
			bb = false,
			gt = false,
			apg = true,
			bbg = true,
			km = true,
			apd = true,
			bbr = true,
			bdr = true,
			cp = true,
			fp = true,
			sp = true,
			ss = true,
			sm = true,
			mwb = true,
			hb = true,
			rb = true
		},
		MIN_TEAM_SIZE = 1,
		PANEL_STYLE = 1,
		ALLOW_TOURNAMENT = true,
		VERIFIED_ONLY = 1,
	},
	-- coop
	[2] = {
		ALLOWED_MAP = {
			caldavar = true,
			grimmscrossing = false,
			midwars = false,
			riftwars = false,
			prophets = false,
			thegrimmhunt = false,
			capturetheflag = false,
			devowars = false,
			team_deathmatch = false,
			random = false,
		},
		ALLOWED_GAMETYPE = {
			[1] = false,
			[2] = true,
			[3] = false,
			[4] = false,
			[5] = false,
		},
		ALLOWED_GAMEMODE = {
			ap = true,
			sd = false,
			bd = false,
			bp = false,
			ar = false,
			lp = false,
			bb = false,
			gt = false,
			apg = false,
			bbg = false,
			km = false,
			apd = false,
			bbr = false,
			bdr = false,
			cp = false,
			fp = false,
			sp = false,
			ss = false,
			sm = false,
			mwb = true,
			hb = true
		},
		MIN_TEAM_SIZE = 1,
		PANEL_STYLE = 0,
		ALLOW_TOURNAMENT = true,
		VERIFIED_ONLY = 2,
	},
}
local MAPS = {} -- "caldavar", "midwars", "riftwars", "grimmscrossing", "prophets", "thegrimmhunt", "capturetheflag", "devowars", "random", "team_deathmatch"
local MAP_INFO = {
	caldavar = {
		ALLOWED_GAMETYPE = {
			[1] = true,
			[2] = true,
			[3] = false,
			[4] = false,
			[5] = false,
		},
		MMR = true,
		SIZE = 5,
		-- ALLOWED_GAMEMODE = {
		-- 	ap = true,
		-- 	sd = true,
		-- 	bd = false,
		-- 	bp = true,
		-- 	ar = false,
		-- 	br = true,
		-- 	lp = true,
		-- 	bb = false,
		-- 	gt = false,
		-- 	apg = true,
		-- 	bbg = false,
		-- 	km = false,
		-- },
		ALLOW_RANKED = true,
		HIDE_DISABLED = true,
	},
	grimmscrossing = {
		ALLOWED_GAMETYPE = {
			[1] = false,
			[2] = true,
			[3] = false,
			[4] = false,
			[5] = false,
		},
		MMR = false,
		SIZE = 5,
		-- ALLOWED_GAMEMODE = {
		-- 	ap = true,
		-- 	sd = true,
		-- 	bd = false,
		-- 	bp = true,
		-- 	ar = true,
		-- 	br = false,
		-- 	lp = false,
		-- 	bb = false,
		-- 	gt = false,
		-- 	apg = true,
		-- 	bbg = false,
		-- 	km = false,
		-- },
		ALLOW_RANKED = false,
		HIDE_DISABLED = true,
	},
	midwars = {
		ALLOWED_GAMETYPE = {
			[1] = false,
			[2] = false,
			[3] = true,
			[4] = false,
			[5] = false,
		},
		-- ALLOWED_GAMEMODE = {
		-- 	ap = false,
		-- 	sd = false,
		-- 	bd = false,
		-- 	bp = false,
		-- 	ar = true,
		-- 	br = false,
		-- 	lp = false,
		-- 	bb = true,
		-- 	gt = false,
		-- 	apg = false,
		-- 	bbg = true,
		-- 	km = false,
		-- },
		GAMEMODE_INFO_OVERRIDE = {
			--[[
			ap = {
				MODE = "normal", -- "blindbanningpick",
				NAME = "mainlobby_label_blind_ban",
				DESC = "general_mode_blindbanningpick",
				ICON = "blindban",
				BASIC = true,
				PASS = false,
			},
			--]]
		},
		MMR = false,
		SIZE = 5,
		ALLOW_RANKED = false,
		HIDE_DISABLED = true,
	},
	riftwars = {
		ALLOWED_GAMETYPE = {
			[1] = false,
			[2] = false,
			[3] = false,
			[4] = true,
			[5] = false,
		},
		-- ALLOWED_GAMEMODE = {
		-- 	ap = false,
		-- 	sd = false,
		-- 	bd = false,
		-- 	bp = false,
		-- 	ar = false,
		-- 	br = false,
		-- 	lp = false,
		-- 	bb = false,
		-- 	gt = false,
		-- 	apg = false,
		-- 	bbg = false,
		-- 	km = true,
		-- },
		GAMEMODE_INFO_OVERRIDE = {
			--[[
			ap = {
				MODE = "normal", -- "blindbanningpick",
				NAME = "mainlobby_label_blind_ban",
				DESC = "general_mode_blindbanningpick",
				ICON = "blindban",
				BASIC = true,
				PASS = false,
			},
			--]]
		},
		MMR = false,
		SIZE = 5,
		ALLOW_RANKED = false,
		HIDE_DISABLED = true,
	},
	prophets = {
		ALLOWED_GAMETYPE = {
			[1] = false,
			[2] = false,
			[3] = false,
			[4] = false,
			[5] = true,
		},
		-- ALLOWED_GAMEMODE = {
		-- 	ap = false,
		-- 	sd = false,
		-- 	bd = false,
		-- 	bp = false,
		-- 	ar = false,
		-- 	br = false,
		-- 	lp = false,
		-- 	bb = false,
		-- 	gt = false,
		-- 	apg = false,
		-- 	bbg = false,
		-- 	km = true,
		-- },
		GAMEMODE_INFO_OVERRIDE = {
			--[[
			ap = {
				MODE = "normal", -- "blindbanningpick",
				NAME = "mainlobby_label_blind_ban",
				DESC = "general_mode_blindbanningpick",
				ICON = "blindban",
				BASIC = true,
				PASS = false,
			},
			--]]
		},
		MMR = false,
		SIZE = 5,
		ALLOW_RANKED = false,
		HIDE_DISABLED = true,
	},
	team_deathmatch = {
		ALLOWED_GAMETYPE = {
			[1] = false,
			[2] = false,
			[3] = false,
			[4] = false,
			[5] = true,
		},
		-- ALLOWED_GAMEMODE = {
		-- 	ap = false,
		-- 	sd = false,
		-- 	bd = false,
		-- 	bp = false,
		-- 	ar = false,
		-- 	br = false,
		-- 	lp = false,
		-- 	bb = false,
		-- 	gt = false,
		-- 	apg = false,
		-- 	bbg = false,
		-- 	km = true,
		-- },
		GAMEMODE_INFO_OVERRIDE = {
			--[[
			ap = {
				MODE = "normal", -- "blindbanningpick",
				NAME = "mainlobby_label_blind_ban",
				DESC = "general_mode_blindbanningpick",
				ICON = "blindban",
				BASIC = true,
				PASS = false,
			},
			--]]
		},
		MMR = false,
		SIZE = 5,
		ALLOW_RANKED = false,
		HIDE_DISABLED = true,
	},
	thegrimmhunt = {
		ALLOWED_GAMETYPE = {
			[1] = false,
			[2] = true,
			[3] = false,
			[4] = false,
			[5] = false,
		},
		-- ALLOWED_GAMEMODE = {
		-- 	ap = false,
		-- 	sd = false,
		-- 	bd = false,
		-- 	bp = false,
		-- 	ar = false,
		-- 	br = false,
		-- 	lp = false,
		-- 	bb = false,
		-- 	gt = false,
		-- 	apg = false,
		-- 	bbg = false,
		-- 	km = true,
		-- },
		GAMEMODE_INFO_OVERRIDE = {
			--[[
			ap = {
				MODE = "normal", -- "blindbanningpick",
				NAME = "mainlobby_label_blind_ban",
				DESC = "general_mode_blindbanningpick",
				ICON = "blindban",
				BASIC = true,
				PASS = false,
			},
			--]]
		},
		MMR = false,
		SIZE = 5,
		ALLOW_RANKED = false,
		HIDE_DISABLED = true,
	},
	capturetheflag = {
		ALLOWED_GAMETYPE = {
			[1] = false,
			[2] = true,
			[3] = false,
			[4] = false,
			[5] = false,
		},
		-- ALLOWED_GAMEMODE = {
		-- 	ap = false,
		-- 	sd = false,
		-- 	bd = false,
		-- 	bp = false,
		-- 	ar = false,
		-- 	br = false,
		-- 	lp = false,
		-- 	bb = false,
		-- 	gt = false,
		-- 	apg = false,
		-- 	bbg = false,
		-- 	km = true,
		-- },
		GAMEMODE_INFO_OVERRIDE = {
			--[[
			ap = {
				MODE = "normal", -- "blindbanningpick",
				NAME = "mainlobby_label_blind_ban",
				DESC = "general_mode_blindbanningpick",
				ICON = "blindban",
				BASIC = true,
				PASS = false,
			},
			--]]
		},
		MMR = false,
		SIZE = 5,
		ALLOW_RANKED = false,
		HIDE_DISABLED = true,
	},
	devowars = {
		ALLOWED_GAMETYPE = {
			[1] = false,
			[2] = true,
			[3] = false,
			[4] = false,
			[5] = false,
		},
		-- ALLOWED_GAMEMODE = {
		-- 	ap = false,
		-- 	sd = false,
		-- 	bd = false,
		-- 	bp = false,
		-- 	ar = false,
		-- 	br = false,
		-- 	lp = false,
		-- 	bb = false,
		-- 	gt = false,
		-- 	apg = false,
		-- 	bbg = false,
		-- 	km = true,
		-- },
		GAMEMODE_INFO_OVERRIDE = {
			--[[
			ap = {
				MODE = "normal", -- "blindbanningpick",
				NAME = "mainlobby_label_blind_ban",
				DESC = "general_mode_blindbanningpick",
				ICON = "blindban",
				BASIC = true,
				PASS = false,
			},
			--]]
		},
		MMR = false,
		SIZE = 5,
		ALLOW_RANKED = false,
		HIDE_DISABLED = true,
	},
	random = {
		ALLOWED_GAMETYPE = {
			[1] = true,
			[2] = true,
			[3] = true,
			[4] = true,
			[5] = true,
		},
		-- ALLOWED_GAMEMODE = {
		-- 	ap = false,
		-- 	sd = false,
		-- 	bd = false,
		-- 	bp = false,
		-- 	ar = false,
		-- 	br = false,
		-- 	lp = false,
		-- 	bb = false,
		-- 	gt = false,
		-- 	apg = false,
		-- 	bbg = false,
		-- 	km = true,
		-- },
		GAMEMODE_INFO_OVERRIDE = {
			--[[
			ap = {
				MODE = "normal", -- "blindbanningpick",
				NAME = "mainlobby_label_blind_ban",
				DESC = "general_mode_blindbanningpick",
				ICON = "blindban",
				BASIC = true,
				PASS = false,
			},
			--]]
		},
		MMR = false,
		SIZE = 5,
		ALLOW_RANKED = false,
		HIDE_DISABLED = true,
	},
	soccer = {
		ALLOWED_GAMETYPE = {
			[1] = false,
			[2] = true,
			[3] = false,
			[4] = false,
			[5] = false,
		},
		-- ALLOWED_GAMEMODE = {
		-- 	ap = false,
		-- 	sd = false,
		-- 	bd = false,
		-- 	bp = false,
		-- 	ar = false,
		-- 	br = false,
		-- 	lp = false,
		-- 	bb = false,
		-- 	gt = false,
		-- 	apg = false,
		-- 	bbg = false,
		-- 	km = true,
		-- },
		GAMEMODE_INFO_OVERRIDE = {
			--[[
			ap = {
				MODE = "normal", -- "blindbanningpick",
				NAME = "mainlobby_label_blind_ban",
				DESC = "general_mode_blindbanningpick",
				ICON = "blindban",
				BASIC = true,
				PASS = false,
			},
			--]]
		},
		MMR = false,
		SIZE = 5,
		ALLOW_RANKED = false,
		HIDE_DISABLED = true,
	},
	solomap = {
		ALLOWED_GAMETYPE = {
			[1] = false,
			[2] = false,
			[3] = false,
			[4] = false,
			[5] = true,
		},
		-- ALLOWED_GAMEMODE = {
		-- 	ap = false,
		-- 	sd = false,
		-- 	bd = false,
		-- 	bp = false,
		-- 	ar = false,
		-- 	br = false,
		-- 	lp = false,
		-- 	bb = false,
		-- 	gt = false,
		-- 	apg = false,
		-- 	bbg = false,
		-- 	km = true,
		-- },
		GAMEMODE_INFO_OVERRIDE = {
			--[[
			ap = {
				MODE = "normal", -- "blindbanningpick",
				NAME = "mainlobby_label_blind_ban",
				DESC = "general_mode_blindbanningpick",
				ICON = "blindban",
				BASIC = true,
				PASS = false,
			},
			--]]
		},
		MMR = false,
		SIZE = 1,
		ALLOW_RANKED = true,
		HIDE_DISABLED = true,
	},
	midwarsbeta = {
		ALLOWED_GAMETYPE = {
			[1] = false,
			[2] = false,
			[3] = false,
			[4] = false,
			[5] = true,
		},
		-- ALLOWED_GAMEMODE = {
		-- 	ap = false,
		-- 	sd = false,
		-- 	bd = false,
		-- 	bp = false,
		-- 	ar = false,
		-- 	br = false,
		-- 	lp = false,
		-- 	bb = false,
		-- 	gt = false,
		-- 	apg = false,
		-- 	bbg = false,
		-- 	km = true,
		-- },
		GAMEMODE_INFO_OVERRIDE = {
			--[[
			ap = {
				MODE = "normal", -- "blindbanningpick",
				NAME = "mainlobby_label_blind_ban",
				DESC = "general_mode_blindbanningpick",
				ICON = "blindban",
				BASIC = true,
				PASS = false,
			},
			--]]
		},
		MMR = false,
		SIZE = 5,
		ALLOW_RANKED = false,
		HIDE_DISABLED = true,
	},
}
local GAMETYPE_INFO = {
	[1] = {		-- Normal
		-- ALLOWED_GAMEMODE = {
		-- 	ap = true,
		-- 	sd = true,
		-- 	bp = true,
		-- 	ar = true,
		-- 	lp = true,
		-- 	bb = true,
		-- 	gt = false,
		-- 	apg = true,
		-- 	bbg = true,
		-- 	bd = false,
		-- 	km = false,
		-- },
		MMR = true,
	},
	[2] = {	-- Casual
		-- ALLOWED_GAMEMODE = {
		-- 	ap = true,
		-- 	sd = true,
		-- 	bp = true,
		-- 	ar = true,
		-- 	lp = true,
		-- 	bb = false,
		-- 	gt = false,
		-- 	apg = true,
		-- 	bbg = true,
		-- 	bd = false,
		-- 	km = false,
		-- },
		MMR = true,
	},
	[3] = {	-- Midwars
		-- ALLOWED_GAMEMODE = {
		-- 	ap = true,
		-- 	sd = true,
		-- 	bp = true,
		-- 	ar = true,
		-- 	lp = true,
		-- 	bb = true,
		-- 	gt = false,
		-- 	apg = true,
		-- 	bbg = true,
		-- 	bd = false,
		-- 	km = false,
		-- },
		MMR = false
	},
	[4] = {	-- Riftwars
		-- ALLOWED_GAMEMODE = {
		-- 	ap = false,
		-- 	sd = false,
		-- 	bp = false,
		-- 	ar = false,
		-- 	lp = false,
		-- 	bb = false,
		-- 	gt = false,
		-- 	apg = false,
		-- 	bbg = false,
		-- 	bd = false,
		-- 	km = true,
		-- },
		MMR = false
	},
	[5] = {	-- Custom
		-- ALLOWED_GAMEMODE = {
		-- 	ap = false,
		-- 	sd = false,
		-- 	bp = false,
		-- 	ar = false,
		-- 	lp = false,
		-- 	bb = false,
		-- 	gt = false,
		-- 	apg = false,
		-- 	bbg = false,
		-- 	bd = false,
		-- 	km = true,
		-- },
		MMR = false
	},
	[6] = {	-- Campaign Normal
		-- ALLOWED_GAMEMODE = {
		-- 	ap = false,
		-- 	sd = false,
		-- 	bp = false,
		-- 	ar = false,
		-- 	lp = false,
		-- 	bb = false,
		-- 	gt = false,
		-- 	apg = false,
		-- 	bbg = false,
		-- 	bd = false,
		-- 	km = true,
		-- },
		MMR = false
	},
	[7] = {	-- Campaign Casual
		-- ALLOWED_GAMEMODE = {
		-- 	ap = false,
		-- 	sd = false,
		-- 	bp = false,
		-- 	ar = false,
		-- 	lp = false,
		-- 	bb = false,
		-- 	gt = false,
		-- 	apg = false,
		-- 	bbg = false,
		-- 	bd = false,
		-- 	km = true,
		-- },
		MMR = false
	},
}
local GAMETYPES = {1, 2, 3, 4, 5, 6, 7} -- 1 = normal, 2 = causual, 3 = midwars, 4 = riftwars, 5 = custom, 6 = campaign normal, 7 = campaign_casual
local GAMETYPE_COLOR = {
	[1] = {background = ".2 1 .2 .9", label = "#32e724"},
	[2] = {background = ".5 .5 1 .8", label = "#74a2ff"},
	[3] = {background = ".7 .5 0.1 .8", label = "#74a2ff"},
	[4] = {background = ".7 .5 0.1 .8", label = "#74a2ff"},
	[5] = {background = ".7 .5 0.1 .8", label = "#74a2ff"},
}

local GAMEMODES = {} -- eh, nothing by default --{"ap", "bb", "sd", "bp", "ar"} -- the defaults, this gets overridden by the region
local GAMEMODE_INFO = {
	ap = {
		MODE = "normal",
		NAME = "mainlobby_label_normal",
		DESC = "general_mode_all_pick",
		ICON = "normal",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	bb = {
		MODE = "blindban",
		NAME = "mainlobby_label_blind_ban",
		DESC = "general_mode_blindbanningpick",
		ICON = "blindban",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = true,
		HIDE_DISABLED = true,
	},
	sd = {
		MODE = "singledraft",
		NAME = "mainlobby_label_single_draft",
		DESC = "general_mode_singledraft",
		ICON = "singledraft",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = true,
		HIDE_DISABLED = true,
	},
	bd = {
		MODE = "banningdraft",
		NAME = "mainlobby_label_banning_draft",
		DESC = "general_mode_banningdraft",
		ICON = "banningdraft",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = true,
		HIDE_DISABLED = true,
	},
	bp = {
		MODE = "banningpick",
		NAME = "mainlobby_label_banning_pick",
		DESC = "general_mode_banningpick",
		ICON = "banningpick",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	lp = {
		MODE = "lockpick",
		NAME = "mainlobby_label_lockpick",
		DESC = "general_mode_lockpick",
		ICON = "lockpick",
		BASIC = false,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
		MIN_TEAM_SIZE = 5,
		DEFAULT_OFF = true,
	},
	cm = {
		MODE = "captainsmode",
		NAME = "mainlobby_label_captainsmode",
		DESC = "general_mode_captainsmode",
		ICON = "captainsmode",
		BASIC = false,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
		MIN_TEAM_SIZE = 5,
		DEFAULT_OFF = true,
	},
	km = {
		MODE = "krosmode",
		NAME = "mainlobby_label_krosmode",
		DESC = "general_mode_krosmode",
		ICON = "krosmode",
		BASIC = false,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
		DEFAULT_OFF = false,
	},
	ar = {
		MODE = "allrandom",
		NAME = "mainlobby_label_all_random",
		DESC = "general_mode_allrandom",
		ICON = "forcerandom",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = true,
		HIDE_DISABLED = true,
	},
	br = {
		MODE = "allrandom",
		NAME = "mainlobby_label_balanced_random",
		DESC = "general_mode_balancedrandom",
		ICON = "forcerandom",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = true,
		HIDE_DISABLED = true,
	},
	rd = {
		MODE = "randomdraft",
		NAME = "mainlobby_label_random_draft",
		DESC = "general_mode_random_draft",
		ICON = "randomdraft",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = true,
		HIDE_DISABLED = true,
	},
	gt = {
		MODE = "normal",
		NAME = "mainlobby_label_gated",
		DESC = "general_mode_gated",
		ICON = "gated",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	apg = {
		MODE = "normal",
		NAME = "mainlobby_label_gated",
		DESC = "general_mode_gated",
		ICON = "gated",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	bbg = {
		MODE = "normal",
		NAME = "mainlobby_label_gated",
		DESC = "general_mode_gated",
		ICON = "gated",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	apd = {
		MODE = "normal",
		NAME = "mainlobby_label_dupe_hero",
		DESC = "general_mode_dupe_hero",
		ICON = "dupehero",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	bbr = {
		MODE = "blindban",
		NAME = "mainlobby_label_blind_ban_blitz",
		DESC = "general_mode_blindbanningpickblitz",
		ICON = "blitz",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = true,
		HIDE_DISABLED = true,
	},
	bdr = {
		MODE = "banningdraft",
		NAME = "mainlobby_label_banning_draft_blitz",
		DESC = "general_mode_banningdraftblitz",
		ICON = "blitz",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = true,
		HIDE_DISABLED = true,
	},
	cp = {
		MODE = "counterpick",
		NAME = "mainlobby_label_counter_pick",
		DESC = "general_mode_counterpick",
		ICON = "counterpick",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	fp = {
		MODE = "forcepick",
		NAME = "mainlobby_label_force_pick",
		DESC = "general_mode_force_pick",
		ICON = "forcepick",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	sp = {
		MODE = "soccerpick",
		NAME = "mainlobby_label_soccer_pick",
		DESC = "general_mode_soccer_pick",
		ICON = "soccerpick",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	ss = {
		MODE = "solosame",
		NAME = "mainlobby_label_solo_same",
		DESC = "general_mode_solo_same",
		ICON = "solosame",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	sm = {
		MODE = "solodiff",
		NAME = "mainlobby_label_solo_diff",
		DESC = "general_mode_solo_diff",
		ICON = "solodiff",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	hb = {
		MODE = "heroban",
		NAME = "mainlobby_label_heroban",
		DESC = "general_mode_herobanpick",
		ICON = "heroban",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = true,
		HIDE_DISABLED = true,
	},
	mwb = {
		MODE = "midwars_beta",
		NAME = "mainlobby_label_midwars_beta",
		DESC = "general_mode_midwars_beta",
		ICON = "midwars_beta",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
	rb = {
		MODE = "reborn",
		NAME = "mainlobby_label_reborn_playerhosted",
		DESC = "general_mode_reborn",
		ICON = "banningdraft",
		BASIC = true,
		--RANKED_ONLY = false,
		PASS = false,
		HIDE_DISABLED = true,
	},
}

local REGIONS_MAP = {
	korea = true,
	lat = true,
} -- "international", sea", "cis", "zh" (china)
local REGIONS = {}
local REGION_INFO = {}
local MIN_SELECTED_GAMEMODES = 1
local MIN_SELECTED_REGIONS = 1
local LOW_POP_THRESHOLD = 0.03
local MED_POP_THRESHOLD = 0.20
local HIGH_DISPARITY_THRESHOLD = 200
local MED_DISPARITY_THRESHOLD = 150

-- DB
local defaultRulesetMode

--
local availableGameType, availableMap, availableGameMode, availableRegion = "", "", "", ""

local verifiedOnly = nil
local rulesetMode = nil
local viewMode = nil
local selectedMinGroupSize = 1

local tournamentModeAllowed = nil
local mapAllowed = {}
local mapSelected = nil
local lastMapSelected = nil
local lastRuleset = nil
local lastVerifiedOnly = nil
local gameTypeAllowed = {}
local gameTypeSelected = nil
local gameModeAllowed = {}
local gameModeIndexToName = {}
local gameModeNameToIndex = {}
local gameModeCompatible = {}
local gameModeSelected = {}
local gameModeSelecetdString = ""
local gameModeSelectedCount = 0
local queueModeLoad = false
local regionAllowed = {}
local regionSelected = {}
local regionSelectedString = ""
local regionSelectedCount = 0
local savedModeSettings = {}

local gou_verifiedOnly = nil
local gou_map = nil
local gou_gameType = nil
local gou_gameModeString = ""
local gou_regionString = ""
local gou_botDifficulty = nil
local gou_randomBots = ""

local basicOptionsOnly = false
local verifiedOnlyGroup = false
local soloQueue = false
local soloQueueSendReady = false
local groupSize = 1
local groupLeaderID = nil

local mouseoverButton = nil
local forceModeButtonsRefresh = false
local buttonState = {}
local buttonStateShown = {}

local playerName = {}
local playerVerified = {}
local playerMMR = {}

local randomEnemyBots
local randomAllybots = false
local botDifficulty

-- -----
-- local helper functions
-- -----
local function GetPrefixFromProductType(type)
	if type == 'Chat Color' then 
		return 'cc' 
	elseif type == 'Chat Symbol' then
		return 'cs'
	elseif type == 'Account Icon' then 
		return 'ai'
	elseif type == 'Alt Avatar' then
		return 'aa' 
	end

	return ''
end

local function ReceiveCampaignSeasonRewards(_, highestleve, ...)
	GetWidget('campaign_reward_popup_icon'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevelAfterS6(tonumber(highestleve)))

	local levelStr = Translate('player_compaign_level_name_S7_'..highestleve)
	GetWidget('campaign_reward_popup_tips'):SetText(Translate('season_rewards_tips', 'level', levelStr))
	
	local medal = tonumber(highestleve)
	if medal > 0 and medal <= 5 then 
		GetWidget('campaign_reward_popup_effect'):StartEffect('/ui/fe2/season/effects/popup_back_1.effect', 0.5, 0.5, '1 1 1', 1)
	elseif medal > 5 and medal <= 10 then 
		GetWidget('campaign_reward_popup_effect'):StartEffect('/ui/fe2/season/effects/popup_back_2.effect', 0.5, 0.5, '1 1 1', 1)
	elseif medal > 10 and medal <= 13 then
		GetWidget('campaign_reward_popup_effect'):StartEffect('/ui/fe2/season/effects/popup_back_3.effect', 0.5, 0.5, '1 1 1', 1)
	elseif medal > 13 and medal <= 15 then 
		GetWidget('campaign_reward_popup_effect'):StartEffect('/ui/fe2/season/effects/popup_back_4.effect', 0.5, 0.5, '1 1 1', 1)
	elseif medal == 16 then
		GetWidget('campaign_reward_popup_effect'):StartEffect('/ui/fe2/season/effects/popup_back_5.effect', 0.5, 0.5, '1 1 1', 1)
	elseif medal >= 17 then
		GetWidget('campaign_reward_popup_effect'):StartEffect('/ui/fe2/season/effects/popup_back_6.effect', 0.5, 0.5, '1 1 1', 1)
	end

	GetWidget('rank_reward_popup_icon'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevelAfterS6(medal))
	GetWidget('rank_reward_popup_tips'):SetText(Translate('rankreward_popup_tips', 'level_name', Translate('player_compaign_level_name_S7_'..tostring(medal))))
	for i=1, 6 do
		local type = arg[(i-1)*5+1]
		local id = arg[(i-1)*5+2]
		local path = arg[(i-1)*5+3]
		local count = arg[(i-1)*5+4]
		local productType = arg[(i-1)*5+5]

		GetWidget('campaign_reward_popup_item'..i..'_text'):SetFont('dyn_12')
		if type ~= nil then
			if tonumber(type) == 1 then
				GetWidget('campaign_reward_popup_item'..i..'_icon'):SetTexture(path)

				local typeprefix = GetPrefixFromProductType(productType)
				local typeText = ''
				if NotEmpty(typeprefix) then
					GetWidget('campaign_reward_popup_item'..i..'_text'):SetFont('dyn_10')
					typeText = '^885['..Translate('general_product_type_'..typeprefix)..']^* \n'
				end

				local text = typeText..Translate('mstore_product'..id..'_name')
				if tonumber(count) > 1 then
					text = text..' ^279x '..count
				end
				GetWidget('campaign_reward_popup_item'..i..'_text'):SetText(text)
			elseif tonumber(type) == 2 then
				GetWidget('campaign_reward_popup_item'..i..'_icon'):SetTexture('/ui/fe2/season/gold.tga')
				local text = Translate('season_rewards_goldcoin')..' ^279x '..count
				GetWidget('campaign_reward_popup_item'..i..'_text'):SetText(text)
			elseif tonumber(type) == 3 then
				GetWidget('campaign_reward_popup_item'..i..'_icon'):SetTexture('/ui/fe2/season/plinko.tga')
				local text = Translate('season_rewards_plinkoticket')..' ^279x '..count
				GetWidget('campaign_reward_popup_item'..i..'_text'):SetText(text)
			end
		end
		GetWidget('campaign_reward_popup_item'..i):SetVisible(type~=nil)
	end

	HoN_Matchmaking.popupTimer = GetTime()
    HoN_Matchmaking.popupShow = true
    GetWidget('campaign_reward_popup'):SetVisible(true)
end
triggerHelper:RegisterWatch("ReceiveCampaignSeasonRewards", function(...) ReceiveCampaignSeasonRewards(...) end)

local function TMMSettingsDebug(source, forceDisplay, gameTypeSelectedOverride)
	if GetCvarBool('ui_debugTMM') or forceDisplay then
		println('^y TMMSettingsDebug = ' .. tostring(source) )
		println('gameTypeSelected = ' .. tostring(gameTypeSelected) )
		println('mapSelected = ' .. tostring(mapSelected) )
		println('gameModeSelectedString = ' .. tostring(gameModeSelectedString) )
		println('regionSelectedString = ' .. tostring(regionSelectedString) )
		--println('verifiedOnly = ' .. tostring(verifiedOnly) )
		println('ranked = ' .. tostring(verifiedOnly) )
		println('cc_TMMMatchFidelity = ' .. GetCvarString('cc_TMMMatchFidelity'))
		println('rulesetMode = ' .. tostring(rulesetMode) )
		println('viewMode = ' .. tostring(viewMode) )
		println('selectedMinGroupSize = ' .. tostring(selectedMinGroupSize) )
		if (gameTypeSelectedOverride) then
			println('^rgameTypeSelectedOverride = ' .. tostring(gameTypeSelectedOverride) )
		end
	end
end

local function MidwarsShennanigans(map, gameType)
	if (map == 'midwars') then
		return TMM_GAME_TYPE_MIDWARS
	elseif (map == 'riftwars') then
		return TMM_GAME_TYPE_RIFTWARS
	elseif (map ~= 'caldavar' and map ~= 'grimmscrossing' and map ~= 'thegrimmhunt' and map ~= 'capturetheflag' and map ~= 'devowars' and map ~='soccer') then
		return TMM_GAME_TYPE_CUSTOM
	else
		return gameType
	end
end

local function RankedShennanigans()
	if HoN_Matchmaking.selectedTab == 'season' or HoN_Matchmaking.selectedTab == 'pvp' then 
		return true
	else
		return false
	end
end

local function GameModeTrick(gameType, gameMode)
	if gameType ~= TMM_GAME_TYPE_CAMPAIGN_NORMAL and gameType ~= TMM_GAME_TYPE_CAMPAIGN_CASUAL then
		local modes = explode('|', gameMode)
		local newmodes = ''
		for _,v in ipairs(modes) do
			if v ~= 'rb' then
				if NotEmpty(newmodes) then
					newmodes = newmodes..'|'
				end
				newmodes = newmodes..v
			end
		end
		return newmodes
	end
	return gameMode
end

local function isVerifiedOnlyAllowed()
	return (rulesetMode == nil) or (RULESET_INFO[rulesetMode] == nil) or (RULESET_INFO[rulesetMode].VERIFIED_ONLY == nil) or (RULESET_INFO[rulesetMode].VERIFIED_ONLY == 1 or RULESET_INFO[rulesetMode].VERIFIED_ONLY == 2)
end

local function isUnVerifiedAllowed()
	return (rulesetMode == nil) or (RULESET_INFO[rulesetMode] == nil) or (RULESET_INFO[rulesetMode].VERIFIED_ONLY == nil) or (RULESET_INFO[rulesetMode].VERIFIED_ONLY == 0 or RULESET_INFO[rulesetMode].VERIFIED_ONLY == 2)
end

local function isMapAllowed(id)
	return mapAllowed[id]
	   and (not RULESET_INFO[rulesetMode].ALLOWED_MAP or RULESET_INFO[rulesetMode].ALLOWED_MAP[id])
end

local function IsTournamentAllowed(id)
	local id = id or 'lp'
	return tournamentModeAllowed
		and ((mapSelected == nil) or (not MAP_INFO[mapSelected].ALLOWED_GAMEMODE or MAP_INFO[mapSelected].ALLOWED_GAMEMODE[id]))
		and ((gameTypeSelected == nil) or (not GAMETYPE_INFO[gameTypeSelected].ALLOWED_GAMEMODE or GAMETYPE_INFO[gameTypeSelected].ALLOWED_GAMEMODE[id]))
		and ((rulesetMode == nil) or (RULESET_INFO[rulesetMode] == nil) or (RULESET_INFO[rulesetMode].ALLOW_TOURNAMENT))
end

local function IsGameTypeAllowed(id)
	return gameTypeAllowed[id]
	   and ((rulesetMode == nil) or (not RULESET_INFO[rulesetMode].ALLOWED_GAMETYPE or RULESET_INFO[rulesetMode].ALLOWED_GAMETYPE[id]))
	   and ((mapSelected == nil) or (not MAP_INFO[mapSelected].ALLOWED_GAMETYPE or MAP_INFO[mapSelected].ALLOWED_GAMETYPE[id]))
end

local function IsGameModeAllowed(id)

	local regionAllowsMode, isBetaRegion = true, false
	for tempRegion, isSelected in pairs(regionSelected) do
		if (isSelected) then
			if (REGION_INFO) and (REGION_INFO[tempRegion]) and (REGION_INFO[tempRegion].ALLOWED_GAMEMODE) and (not REGION_INFO[tempRegion].ALLOWED_GAMEMODE[id]) then
				if (REGION_INFO[tempRegion].BETA_REGION) then
					isBetaRegion = true
				end
				regionAllowsMode = false
				break
			end
		end
	end

	local gameModeExists = {}
	for _, gm in ipairs(GAMEMODES) do
		gameModeExists[gm] = true
	end

	local gameTypeSelectedOverride = MidwarsShennanigans(mapSelected or "", gameTypeSelected or "")
	local gmAllowedFunc = IsEnabledGameMode(gameTypeSelectedOverride or "", mapSelected or "", id, tostring(verifiedOnly) or "")

	return gameModeAllowed[id]
	   and gmAllowedFunc
	   and (gameModeExists[id] ~= nil)
	   and ((rulesetMode == nil) or (not RULESET_INFO[rulesetMode].ALLOWED_GAMEMODE or RULESET_INFO[rulesetMode].ALLOWED_GAMEMODE[id]))
	   and (regionAllowsMode)
	   and ((not IsInGroup() and (IsEligibleForRanked() or (not GAMEMODE_INFO[id].RANKED_ONLY))) or ((not basicOptionsOnly and (IsEligibleForRanked())) or ((not GAMEMODE_INFO[id].RANKED_ONLY))))
	   , (regionAllowsMode)
	   , isBetaRegion
	    --and ((not IsInGroup() and (UIIsVerified() or GAMEMODE_INFO[id].BASIC)) or ((not basicOptionsOnly and UIIsVerified()) or (GAMEMODE_INFO[id].BASIC)))
end

local function GetGameModeInfo(id)
	local infoOverride = NotEmpty(mapSelected) and MAP_INFO[mapSelected] and MAP_INFO[mapSelected].GAMEMODE_INFO_OVERRIDE
	return infoOverride and infoOverride[id] or GAMEMODE_INFO[id]
end

local function CountSelectedGameModes()
	gameModeSelectedCount = 0
	for gm, t in pairs(GAMEMODE_INFO) do
		if (IsGameModeAllowed(gm) and gameModeSelected[gm]) then
			gameModeSelectedCount = gameModeSelectedCount + 1
		end
	end
end

local function CountSelectedRegions()
	regionSelectedCount = 0
	for _, v in pairs(REGIONS) do
		if (regionAllowed[v] and regionSelected[v]) then
			regionSelectedCount = regionSelectedCount + 1
		end
	end
end

local function GetInvalidRegionPlayerNames(region)
	local returnString = ""
	local hasNames = false
	for i = 0, 4 do
		local name = playerName[i]
		if (name and not CanGroupPlayerAccessRegion(name, region)) then
			returnString = returnString..(hasNames and ", " or "").."^y"..name.."^*"
			hasNames = true
		end
	end
	return returnString
end

local function SaveBots()
	local enemyBotTable = {	HoN_Matchmaking:GetNameFromID(HoN_Matchmaking.bots[1]),
							HoN_Matchmaking:GetNameFromID(HoN_Matchmaking.bots[2]),
							HoN_Matchmaking:GetNameFromID(HoN_Matchmaking.bots[3]),
							HoN_Matchmaking:GetNameFromID(HoN_Matchmaking.bots[4]),
							HoN_Matchmaking:GetNameFromID(HoN_Matchmaking.bots[5])
						  }
	GetDBEntry('mm_enemy_bots', enemyBotTable, true, false, false)

	local teamBotTable = {}
	for i=2,5 do
		if (HoN_Matchmaking.teamMembers[i] and HoN_Matchmaking.teamMembers[i].isBot) then
			table.insert(teamBotTable, HoN_Matchmaking:GetNameFromID(HoN_Matchmaking.teamMembers[i].botID))
		end
	end

	GetDBEntry('mm_team_bots', teamBotTable, true, false, false)
end

function HoN_Matchmaking:OnHide()
	HoN_Matchmaking:DumpUserInfoTable()

	-- save the selected heros
	--SaveBots()
end

-- checks for the need of tokens when starting/readying, showing popups as needed; will call widgetName's event if check is passed
local function CheckTokensAndDoFunction(functionName)
	if (AccountStanding() == 3) then -- legacy account; unlimited tokens
		functionName()
		return
	end
	local requiresToken = false
	for _, v in pairs(GAMEMODES) do
		local info = GetGameModeInfo(v)
		if (gameModeSelected[v]) and (info) and (not (info.BASIC)) and (NeedsToken(info.MODE)) then
			requiresToken = true
		end
	end
	if (requiresToken) then
		--if (UIIsVerified()) then
			local hasToken = GameTokens() >= 0
			if (hasToken) then
				local warnTokensPanel = GetWidget("warn_tokens")
				local warnTokensButton = GetWidget("warn_button_1")

				local unRegFuncs = function (...)
					warnTokensPanel:UnregisterCallback("onhide")
					warnTokensPanel:RefreshCallbacks()
					warnTokensButton:UnregisterCallback("onclick")
					warnTokensButton:RefreshCallbacks()
				end

				local executeFunc = function (...)
					functionName()
					unRegFuncs()
				end

				warnTokensPanel:SetCallback("onhide", unRegFuncs)
				warnTokensPanel:RefreshCallbacks()
				warnTokensPanel:SetCallback("onclick", executeFunc)
				warnTokensPanel:RefreshCallbacks()

				Trigger("WarnTokensTrigger", '', "")
			else
				Trigger("NeedTokensTrigger", "", "CallEvent('mm_popup_buytoken', 0);")
			end
		-- else
		-- 	GetWidget("verified_benefits_modal"):UICmd("FadeIn(250);")
		-- end
	else
		functionName()
	end
end

local function SendGroupOptionsUpdate()
	if (IsInGroup() and not IsInQueue()) then
		local gameTypeSelectedOverride
		gameTypeSelectedOverride = MidwarsShennanigans(gou_map, gou_gameType)

		local rankedgameoverride = RankedShennanigans()
		
		interface:UICmd("SendTMMGroupOptionsUpdate("..gameTypeSelectedOverride..", '"..gou_map.."', '"..gou_gameModeString.."', '"..gou_regionString.."', "..tostring(rankedgameoverride)..", cc_TMMMatchFidelity, "..gou_botDifficulty..", "..tostring(gou_randomBots)..");")

		TMMSettingsDebug('SendGroupOptionsUpdate ', nil, gameTypeSelectedOverride)
	end
end

local function SendGroupOptionsUpdateFidelity()
	if (IsInGroup() and IsInQueue() and UIGetAccountID() == groupLeaderID) then
		--sending 99 as the GameType ensures we just update the fidelity value and nothing else
		interface:UICmd("SendTMMGroupOptionsUpdate(99, '"..gou_map.."', '"..gou_gameModeString.."', '"..gou_regionString.."', "..tostring(gou_verifiedOnly)..", cc_TMMMatchFidelity, "..gou_botDifficulty..", "..tostring(gou_randomBots)..");")
		TMMSettingsDebug('SendGroupOptionsUpdateFidelity')
	end
end

local function UpdateDefaultRuleset()
	if GetCvarBool('_loggedin') then
		--local regionMap, regionsList, regionInfo = HoN_Region:GetMatchmakingRegionInfo()
		--defaultRulesetMode = GetDBEntry('def_tmm_tab', 0, false, false, false) -- saveToDB, restoreDefault, setDefault

		if (HoN_Matchmaking.selectedTab and HoN_Matchmaking.selectedTab ~= "coop") then
			--if Client.IsNewAccount() then
			--	rulesetMode = 5
			--else
				rulesetMode = 0
			--end
		else
			rulesetMode = 2
		end

		HoN_Matchmaking:CheckSelection()
	else
		defaultRulesetMode = 0
		rulesetMode = 0
	end
end

-------

function HoN_Matchmaking:FillEnemyTeamWithbots()
	-- first attempt to fill the slots with prefered team bots, then ramdom pick the remaining
	local botOffset, slotOffset = 1, 1
	local savedTeamBots = GetDBEntry('mm_enemy_bots', nil, false, false, false) or {}

	while (botOffset <= 5 and slotOffset <= 5) do
		local bot = nil
		if (savedTeamBots[botOffset]) then
			bot = HoN_Matchmaking:GetBotFromName(savedTeamBots[botOffset])
		end
		if (not bot or bot.used) then
			 bot = HoN_Matchmaking:GetBotFromName(DefaultEnemyBots[botOffset])
		end

		if (HoN_Matchmaking.teamMembers[slotOffset]) then
			slotOffset = slotOffset + 1
		elseif (bot) and (bot.used) then
			botOffset = botOffset + 1
		elseif (bot) then
			HoN_Matchmaking.bots[slotOffset] = {}
			HoN_Matchmaking.bots[slotOffset] = bot.id
			SetTeamBot(slotOffset-1, bot.name)
			-- populate and set enemy bots, but only mark usage if random is not on
			if (not randomEnemyBots) then
				HoN_Matchmaking:MarkBotUsageByID(bot.id, true)
			end
			botOffset = botOffset + 1
			slotOffset = slotOffset + 1
		else -- this bot offset was no good, try the next
			botOffset = botOffset + 1
		end
	end

	-- fill empty slots with the first unused bots in sequence
	botOffset = 1
	for i=1, 5 do
		if (not HoN_Matchmaking.bots[i]) then -- empty slot
			while (HoN_Matchmaking:CheckBotUsageByOffsets(botOffset, 1)) do botOffset = botOffset + 1 end
			HoN_Matchmaking.bots[i] = HoN_Matchmaking.botPickTable[botOffset].bots[1].id
			SetTeamBot(i-1, HoN_Matchmaking.botPickTable[botOffset].bots[1].name)
			-- populate and set enemy bots, but only mark usage if random is not on
			if (not randomEnemyBots) then
				HoN_Matchmaking:MarkBotUsageByOffsets(botOffset, 1, true)
			end
			botOffset = botOffset + 1
		end
	end -- for the record, this should screw everything up if there are less bots than what is needed to fill the slots, lets not have that happen
end

local function TabAnimate(modename, show, dontAnimate)
	if modename == "coop" then
		if show then
			if (dontAnimate) then
				GetWidget("mm_botteam_builder"):SetX(0)
				GetWidget("mm_botteam_builder"):SetVisible(1)
			else
				GetWidget("mm_botteam_builder"):SlideX(0, 250)
				GetWidget("mm_botteam_builder"):FadeIn(250)
			end
		else
			if dontAnimate then
				GetWidget("mm_botteam_builder"):SetVisible(0)
				GetWidget("mm_botteam_builder"):SetX(-GetWidget("mm_botteam_builder"):GetWidth())
			else
				GetWidget("mm_botteam_builder"):FadeOut(250)
				GetWidget("mm_botteam_builder"):SlideX(-GetWidget("mm_botteam_builder"):GetWidth(), 250)
			end
		end
	elseif modename == "pvp" then
		if show then
			if (dontAnimate) then
				GetWidget("mm_pvp_options"):SetX(0)
				GetWidget("mm_pvp_options"):SetVisible(1)
			else
				GetWidget("mm_pvp_options"):SlideX(0, 250)
				GetWidget("mm_pvp_options"):FadeIn(250)
			end
		else
			if dontAnimate then
				GetWidget("mm_pvp_options"):SetVisible(0)
				GetWidget("mm_pvp_options"):SetX(-GetWidget("mm_pvp_options"):GetWidth())
			else
				GetWidget("mm_pvp_options"):FadeOut(250)
				GetWidget("mm_pvp_options"):SlideX(-GetWidget("mm_pvp_options"):GetWidth(), 250)
			end
		end
	elseif modename == "season" then
		if show then
			if (dontAnimate) then
				GetWidget("mm_season_options"):SetX(0)
				GetWidget("mm_season_options"):SetVisible(1)
			else
				GetWidget("mm_season_options"):SlideX(0, 250)
				GetWidget("mm_season_options"):FadeIn(250)
			end
		else
			if dontAnimate then
				GetWidget("mm_season_options"):SetVisible(0)
				GetWidget("mm_season_options"):SetX(-GetWidget("mm_season_options"):GetWidth())
			else
				GetWidget("mm_season_options"):FadeOut(250)
				GetWidget("mm_season_options"):SlideX(-GetWidget("mm_season_options"):GetWidth(), 250)
			end
		end
	end
end

function MatchmakingInitialize(RegWidget)
	TabAnimate("coop", false, true)
	TabAnimate("pvp", false, true)
	TabAnimate("season", false, true)

	if (RegisterBotDefinitions) and (not GetCvarBool('ui_bot_definitions_loaded')) then
		RegisterBotDefinitions()
		Set('ui_bot_definitions_loaded', 'true', 'bool')
	end

	-- random bots, do this first since it's needed for FillEnemyTeam
	if (randomEnemyBots == nil) then
		randomEnemyBots = AtoB(matchmakingOptions.randomEnemyBots)
	end

	-- build map and gamemode tables
	-- Iterate over the GAMEMODE_INFO table and take the keys
	GAMEMODES = {}
	for k,_ in pairs(GAMEMODE_INFO) do
		table.insert(GAMEMODES, k)
	end

	-- same as the above for maps
	MAPS = {}
	for k,_ in pairs(MAP_INFO) do
		table.insert(MAPS, k)
	end

	HoN_Matchmaking.botsTable = GetBotDefinitions()
	HoN_Matchmaking:CreateBotPickTable() -- I need to do this now so I can mark the default bots as used, not much way around it

	-- load the entity defs for the bots
	-- for i,t in ipairs(HoN_Matchmaking.botsTable) do
	-- 	if (t.sPrecachePath and t.sPrecachePath ~= "") then
	-- 		RegisterEntityFolder(t.sPrecachePath)
	-- 	end
	-- end

	-- instead of setting the bots to the first 5 entries in the bot pick table, we will set it to the first bot of the first 5 heros
	-- this makes it so we can't accidently select an invalid bot (one without a hero) and we can easily mark them used
	-- also, MarkBotUsageByID should be avoided if we can, mark by offsets is a million times better/faster
	HoN_Matchmaking.bots = {}
	HoN_Matchmaking:FillEnemyTeamWithbots()

	UpdateDefaultRuleset()

	-- bot difficulty
	if (botDifficulty == nil) then
		botDifficulty = tonumber(matchmakingOptions.botDifficulty)
	end

	-- tournament mode
	tournamentModeAllowed = false

	-- maps
	for _, v in ipairs(MAPS) do
		if (mapAllowed[v] == nil) then
			mapAllowed[v] = true
		end
	end

	if (mapSelected == nil) then
		mapSelected = tostring(matchmakingOptions.map)
	end

	if (matchmakingOptions.ranked == nil) then
		matchmakingOptions.ranked = tostring(IsEligibleForRanked())
	end

	if (verifiedOnly == nil) then
		if (IsEligibleForRanked() and mapSelected and MAP_INFO[mapSelected].ALLOW_RANKED) then
			verifiedOnly = AtoB(matchmakingOptions.ranked)
		else
			verifiedOnly = false
		end
	end

	-- all this simple stuff was removed
	-- simple modes
	-- Set("_TMM_SimpleMaps_saved", true, "bool", true)
	-- if (HoN_Matchmaking.simpleMaps == nil) then
	-- 	HoN_Matchmaking.simpleMaps = GetCvarBool("_TMM_SimpleMaps_saved")
	-- end

	-- if (HoN_Matchmaking.simpleMaps) then
	-- 	GetWidget("mm_map_advanced_options"):SetY("-0.3h")
	-- else
	-- 	GetWidget("mm_map_advanced_options"):SetY("13.3h")
	-- end

	-- Set("_TMM_SimpleModes_saved", true, "bool", true)
	-- if (HoN_Matchmaking.simpleModes == nil) then
	-- 	HoN_Matchmaking.simpleModes = GetCvarBool("_TMM_SimpleModes_saved")
	-- end

	-- if (HoN_Matchmaking.simpleModes) then
	-- 	GetWidget("mm_modes_advanced_options"):SetY("-0.3h")
	-- else
	-- 	GetWidget("mm_modes_advanced_options"):SetY("27.7h")
	-- end

	-- game types
	for _, v in ipairs(GAMETYPES) do
		if (gameTypeAllowed[v] == nil) then
			gameTypeAllowed[v] = true
		end
	end
	--if (AccountStanding() <= 1) then
	--	Set("_TMM_GameType_saved_v2", 2, "int", true)
	--else
	--	Set("_TMM_GameType_saved_v2", 1, "int", true)	-- default is now set above
	--end
	if (gameTypeSelected == nil) then
		gameTypeSelected = tonumber(matchmakingOptions.gameType)
	end

	-- game modes
	for _, v in ipairs(GAMEMODES) do
		if (gameModeAllowed[v] == nil) then
			gameModeAllowed[v] = true
			gameModeCompatible[v] = true
		end

		if (not matchmakingOptions["gameMode"..v]) then
			matchmakingOptions["gameMode"..v] = tostring(not ((GAMEMODE_INFO) and (GAMEMODE_INFO[v]) and (GAMEMODE_INFO[v].DEFAULT_OFF)))
		end

		if (gameModeSelected[v] == nil) then
			gameModeSelected[v] = AtoB(matchmakingOptions["gameMode"..v])
		end
	end
	FlushMatchmakingOptions()

	-- regions
	HoN_Matchmaking:InitRegions()

	Set("cc_TMMMatchFidelity", 0, "int")

	-- tab
	if (HoN_Matchmaking.selectedTab == "") then
		HoN_Matchmaking:SelectTab(tostring(matchmakingOptions.tab), true)
	end

--	GetWidget("matchmaking_downvotes_mode"):SetText(Translate('mm_mode_txt1', 'max_downvotes', #GAMEMODES - MIN_SELECTED_GAMEMODES))
--	GetWidget("matchmaking_downvotes_region"):SetText(Translate('tmm_region_text1', 'max_downvotes', #REGIONS - MIN_SELECTED_REGIONS))

	HoN_Matchmaking:CheckSelection()
	HoN_Matchmaking:UpdateButtons()

	matchmakingInitializeMapGuides()
end

function HoN_Matchmaking:InitRegions()
	for _, v in ipairs(REGIONS) do
		if (regionAllowed[v] == nil) then
			regionAllowed[v] = true
		end

		if (not matchmakingOptions["region"..v]) then
			matchmakingOptions["region"..v] = tostring(not ((REGION_INFO) and (REGION_INFO[v]) and (REGION_INFO[v].DEFAULT_OFF)))
		end

		if (regionSelected[v] == nil) then
			regionSelected[v] = AtoB(matchmakingOptions["region"..v])
		end
	end
	FlushMatchmakingOptions()
end

function HoN_Matchmaking:UpdateAllowedOptions()
	local inGroup = IsInGroup()
	local uiVerified = IsEligibleForRanked()

	-- maps
	for _, v in pairs(MAPS) do
		mapAllowed[v] = false
	end
	for _, v in ipairs(explode("|", availableMap)) do
		if (v and NotEmpty(v)) then
			v = string.lower(v)
			mapAllowed[v] = true
		end
	end

	-- game types
	for _, v in pairs(GAMETYPES) do
		gameTypeAllowed[v] = false
	end

	local vToNum
	for _, v in ipairs(explode("|", availableGameType)) do
		if (v and NotEmpty(v)) then
			vToNum = tonumber(v)
			if (vToNum) then
				gameTypeAllowed[vToNum] = true
			else
				println('^oMatchmaking Error - GameType not a number: ' .. tostring(v) )
			end
		end
	end

	-- game modes
	for _, v in pairs(GAMEMODES) do
		gameModeAllowed[v] = false
	end

	local gameModeExists = {}
	for _,gm in ipairs(GAMEMODES) do
		gameModeExists[gm] = true
	end

	gameModeNameToIndex = {}
	gameModeIndexToName = {}

	local index = 0

	for _, v in ipairs(explode("|", availableGameMode)) do
		if (v and NotEmpty(v)) then
			v = string.lower(v)
			local info = GetGameModeInfo(v)
			if ((gameModeExists[v]) and (info) and (CanAccessGameMode(info.MODE) and (gameModeCompatible[v] or not IsInGroup()))) then
				gameModeAllowed[v] = true
			end

			gameModeNameToIndex[v] = index;
			gameModeIndexToName[index] = v;

			index = index + 1;
		end
	end

	-- tournament mode
	-- if (uiVerified and not basicOptionsOnly and gameModeAllowed["lp"]) then  --TOURNAMENT_LP
	-- 	tournamentModeAllowed = true
	-- else
	-- 	tournamentModeAllowed = false
	-- end

	-- regions
	for _, v in pairs(REGIONS) do
		regionAllowed[v] = false
	end
	for _, v in ipairs(explode("|", availableRegion)) do
		if (v and NotEmpty(v)) then
			v = string.lower(v)
			if (CanPlayerAccessRegion(v) and (not inGroup or CanGroupAccessRegion(v))) then
				regionAllowed[v] = true
			end
		end
	end
end

function HoN_Matchmaking:ResetSelection()
	if (matchmakingOptions.tab ~= "coop") then
		if (IsEligibleForRanked() and mapSelected and MAP_INFO[mapSelected] and MAP_INFO[mapSelected].ALLOW_RANKED) then
			verifiedOnly = AtoB(matchmakingOptions.ranked)
		else
			verifiedOnly = false
		end
		mapSelected = matchmakingOptions.map
		gameTypeSelected = tonumber(matchmakingOptions.gameType)
		for _, v in ipairs(GAMEMODES) do
			gameModeSelected[v] = AtoB(matchmakingOptions["gameMode"..v])
		end
	else
		verifiedOnly = false
	end

	basicOptionsOnly = false

	for _, v in ipairs(REGIONS) do
		regionSelected[v] = AtoB(matchmakingOptions["region"..v])
	end

	HoN_Matchmaking:SelectTab(matchmakingOptions.tab, true)

	randomEnemyBots = AtoB(matchmakingOptions.randomEnemyBots)
	botDifficulty = tonumber(matchmakingOptions.botDifficulty)
	Set("g_botDifficulty", botDifficulty, "int")

	forceModeButtonsRefresh = true
	HoN_Matchmaking:CheckSelection()
end

function HoN_Matchmaking:CheckSelection()
	HoN_Matchmaking:UpdateAllowedOptions()
	if (basicOptionsOnly) then
		local basicInGroup = false
		for i = 0, 4 do
			if (NotEmpty(playerName[i]) and not playerVerified[i]) then
				basicInGroup = true
				break
			end
		end
		if (not basicInGroup) then
			basicOptionsOnly = false
			Trigger("TMMReset")
			HoN_Matchmaking:ScheduleGroupOptionsUpdate()
			return
		end
	end

	if (not IsInGroup() or UIGetAccountID() == groupLeaderID) then

		-- verified only
		if (((not isBetaRegion) and (mapSelected)) and ((rulesetMode ~= lastRuleset) or (((not lastMapSelected) or (lastMapSelected ~= mapSelected)) or queueModeLoad))) then
			if ((HoN_Matchmaking.selectedTab ~= "coop") and IsEligibleForRanked() and (not basicOptionsOnly) and mapSelected and MAP_INFO[mapSelected] and MAP_INFO[mapSelected].ALLOW_RANKED) then
				verifiedOnly = AtoB(matchmakingOptions.ranked)
			else
				verifiedOnly = false
			end
		end

		if HoN_Matchmaking.selectedTab == 'pvp' then verifiedOnly = true end
		-- if (not isVerifiedOnlyAllowed()) and (verifiedOnly) or basicOptionsOnly then
		-- 	verifiedOnly = false
		-- elseif (not isUnVerifiedAllowed()) and (not verifiedOnly) then
		-- 	verifiedOnly = true
		-- end

		-- maps
		if (not mapSelected) or (not isMapAllowed(mapSelected) or groupSize > MAP_INFO[mapSelected].SIZE) then
			mapSelected = nil
			forceModeButtonsRefresh = true
			for _, v in pairs(MAPS) do
				if (isMapAllowed(v)) then
					mapSelected = v
					break
				end
			end
		end

		-- game types
		if HoN_Matchmaking.selectedTab ~= 'season' then
			if (not IsGameTypeAllowed(gameTypeSelected)) then

				if (gameTypeSelected and (gameTypeSelected >= 3) and IsGameTypeAllowed(tonumber(matchmakingOptions.gameType))) then
					gameTypeSelected = tonumber(matchmakingOptions.gameType)
				else
					gameTypeSelected = nil
					for _, v in pairs(GAMETYPES) do
						if (IsGameTypeAllowed(v)) then
							gameTypeSelected = v
							break
						end
					end
				end
			elseif ((gameTypeSelected ~= tonumber(matchmakingOptions.gameTypeSelected)) and IsGameTypeAllowed(tonumber(matchmakingOptions.gameType))) then
				gameTypeSelected = tonumber(matchmakingOptions.gameType)
			end
		end

		local isBetaRegion = false
		for tempRegion, isSelected in pairs(regionSelected) do
			if (isSelected) then
				if (REGION_INFO) and (REGION_INFO[tempRegion]) and (REGION_INFO[tempRegion].BETA_REGION) then
					isBetaRegion = true
					queueModeLoad = true
					break
				end
			end
		end

		--println('isBetaRegion = ' .. tostring(isBetaRegion) )

		---[[ load remembered mode settings by map
		local savedModeSettings = {}
		if (((not isBetaRegion) and (mapSelected)) and ((rulesetMode ~= lastRuleset) or (((not lastMapSelected) or (lastMapSelected ~= mapSelected)) or ((lastVerifiedOnly == nil) or (lastVerifiedOnly ~= verifiedOnly)) or queueModeLoad))) then
			if (matchmakingOptions.savedModes and matchmakingOptions.savedModes[rulesetMode] and matchmakingOptions.savedModes[rulesetMode][mapSelected] and matchmakingOptions.savedModes[rulesetMode][mapSelected][tostring(verifiedOnly)]) then
				local matchmakingSaved = matchmakingOptions.savedModes[rulesetMode][mapSelected][tostring(verifiedOnly)]
				for v,i in pairs(matchmakingSaved) do
					if (i) then
						savedModeSettings[v] = true
					end
				end

				queueModeLoad = false
				for _, mode in pairs(GAMEMODES) do
					if (savedModeSettings[mode]) then
						gameModeSelected[mode] = true
					else
						gameModeSelected[mode] = false
					end
				end
			else
				queueModeLoad = false
				gameModeSelected['ap'] = true
				gameModeSelected['bb'] = true
				gameModeSelected['km'] = true
			end
		end
		--]]

		-- game modes
		if (mapSelected) then
			for _, v in pairs(GAMEMODES) do
				if (not IsGameModeAllowed(v) and gameModeSelected[v]) then
					gameModeSelected[v] = false
				end
			end
		end
		CountSelectedGameModes()
		---[[
		if (mapSelected) then
			for _, v in ipairs(GAMEMODES) do
				if (gameModeSelectedCount < MIN_SELECTED_GAMEMODES and IsGameModeAllowed(v) and not gameModeSelected[v]) then
					gameModeSelected[v] = true
					gameModeSelectedCount = gameModeSelectedCount + 1
				end
			end
		end
		--]]

		-- remember mode settings by map
		local loadModeSettings = nil
		if (((not isBetaRegion) and (mapSelected)) and ((rulesetMode ~= lastRuleset) or (not queueModeLoad))) then
			loadModeSettings = loadModeSettings or {}
			--println('Saving ' .. tostring(mapSelected) )

			for _, mode in pairs(GAMEMODES) do
				if (gameModeSelected[mode]) then
					loadModeSettings[mode] = true
				else
					loadModeSettings[mode] = false
				end
			end

			local matchmakingSaved = {}
			for mode, allowed in pairs(loadModeSettings) do
				matchmakingSaved[mode] = allowed
			end
			if (not matchmakingOptions.savedModes) then
				matchmakingOptions.savedModes = {}
			end
			if (not matchmakingOptions.savedModes[rulesetMode]) then
				matchmakingOptions.savedModes[rulesetMode] = {}
			end
			if (not matchmakingOptions.savedModes[rulesetMode][mapSelected]) then
				matchmakingOptions.savedModes[rulesetMode][mapSelected] = {}
			end
			matchmakingOptions.savedModes[rulesetMode][mapSelected][tostring(verifiedOnly)] = matchmakingSaved
			FlushMatchmakingOptions()

			--println('matchmaking_saved_modes_'..mapSelected..' = ' .. tostring(GetCvarString('matchmaking_saved_modes_'..mapSelected)) )

			--GetDBEntry('matchmaking_saved_modes', savedModeSettings, true, false, false)

		end

		lastMapSelected = mapSelected
		lastRuleset = rulesetMode
		lastVerifiedOnly = verifiedOnly

		-- regions
		for _, v in pairs(REGIONS) do
			if (not regionAllowed[v] and regionSelected[v]) then
				regionSelected[v] = false
			end
		end
		CountSelectedRegions()
		for _, v in pairs(REGIONS) do
			if (regionSelectedCount < MIN_SELECTED_REGIONS and regionAllowed[v] and not regionSelected[v]) then
				regionSelected[v] = true
				regionSelectedCount = regionSelectedCount + 1
			end
		end

		-- calculate minimum group size
		if (mapSelected) and (rulesetMode) then
			selectedMinGroupSize = 1
			selectedMinGroupSize = Max(selectedMinGroupSize, RULESET_INFO[rulesetMode].MIN_TEAM_SIZE)
			local info
			for _, v in ipairs(GAMEMODES) do
				info = GetGameModeInfo(v)
				if (info) and gameModeSelected[v] then
					selectedMinGroupSize = Max(selectedMinGroupSize, info.MIN_TEAM_SIZE)
				end
			end
		end

	end

	HoN_Matchmaking:UpdateButtons()
end

function HoN_Matchmaking:ScheduleGroupOptionsUpdate(fidelityOnly)
	-- buffer current values so they aren't overwritten by TMMDisplay before being sent
	gou_verifiedOnly, gou_map, gou_gameType, gou_gameModeString, gou_regionString, gou_botDifficulty, gou_randomBots = verifiedOnly, mapSelected, gameTypeSelected, gameModeSelectedString, regionSelectedString, botDifficulty, randomEnemyBots

	if (fidelityOnly) then
		triggerHelper:Sleep(500, SendGroupOptionsUpdateFidelity)
	else
		triggerHelper:Sleep(500, SendGroupOptionsUpdate)
	end
end

-- update XP/level/MMR value display
function HoN_Matchmaking:UpdatePlayerStats()
	local level = interface:UICmd("GetAccountLevel(GetExperience())")
	local progress = interface:UICmd("FtoP(GetAccountPercentNextLevel(GetExperience()))")
	local mmr = 0

	-- for i = 1,2,1 do
	-- 	GetWidget("tmm_infopanel_stats_level_"..i):SetText(level)
	-- 	GetWidget("tmm_infopanel_stats_progress_"..i):SetWidth(progress)
	-- 	GetWidget("tmm_infopanel_stats_mmr_text_"..i):SetText(mmr)
	-- 	GetWidget("tmm_infopanel_account_basic_level_"..i):SetText(Translate("mm_current_basic_verify2").." "..level)
	-- end

	HoN_Matchmaking:UpdateLeaverBlock()

	-- new popularity
	local maxPop, selectedPop = 0, 0
	local modePop, regionPop, totalPop = 0, 0, 1

	local gameType = tostring(MidwarsShennanigans(mapSelected, gameTypeSelected) or -1)

	-- game modes
	for gm, info in pairs(GAMEMODE_INFO) do
		if (info and IsGameModeAllowed(gm) and mapSelected) then
			local pop = GetPopularity("gamemode", gameType, mapSelected, gm, "-1", true)
			maxPop = maxPop + pop

			if (gameModeSelected[gm]) then
				selectedPop = selectedPop + pop
			end
		end
	end
	if (maxPop > 1) then
		modePop = selectedPop / maxPop
	else
		modePop = 1
	end
	totalPop = totalPop * modePop

	-- regions
	maxPop, selectedPop = 0, 0
	for _, v in ipairs(REGIONS) do
		if (REGION_INFO[v] and mapSelected) then
			local pop = GetPopularity("region", gameType, mapSelected, "-1", REGION_INFO[v].ID, true)
			maxPop = maxPop + pop

			if (regionSelected[v]) then
				selectedPop = selectedPop + pop
			end
		end
	end
	if (maxPop > 1) then
		regionPop = selectedPop / maxPop
	else
		regionPop = 1
	end
	totalPop = totalPop * regionPop
	-- old popularity
	-- HoN_Matchmaking.popularityTable = HoN_Matchmaking.popularityTable or {}
	-- HoN_Matchmaking.popularityTable.summary = {}
	-- HoN_Matchmaking.popularityTable.summary.TOTAL = 1

	-- if (HoN_Matchmaking.popularityTable.MODE) then
	-- 	 maxPop, selectedPop = 0, 0
	-- 	for _, mode in ipairs(GAMEMODES) do
	-- 		if (gameModeAllowed[mode]) and (HoN_Matchmaking.popularityTable.MODE[mode]) then
	-- 			maxPop = maxPop + HoN_Matchmaking.popularityTable.MODE[mode]
	-- 			if (gameModeSelected[mode]) then
	-- 				selectedPop = selectedPop + HoN_Matchmaking.popularityTable.MODE[mode]
	-- 			end
	-- 		end
	-- 	end
	-- 	if (maxPop > 1) then
	-- 		HoN_Matchmaking.popularityTable.summary.MODE = selectedPop / maxPop
	-- 	else
	-- 		HoN_Matchmaking.popularityTable.summary.MODE = 1
	-- 	end
	-- 	HoN_Matchmaking.popularityTable.summary.TOTAL = HoN_Matchmaking.popularityTable.summary.TOTAL * HoN_Matchmaking.popularityTable.summary.MODE
	-- end

	-- if (HoN_Matchmaking.popularityTable.REGION) then
	-- 	 maxPop, selectedPop = 0, 0
	-- 	for _, region in ipairs(REGIONS) do
	-- 		if (regionAllowed[region]) and (HoN_Matchmaking.popularityTable.REGION[region]) then
	-- 			maxPop = maxPop + HoN_Matchmaking.popularityTable.REGION[region]
	-- 			if (regionSelected[region]) then
	-- 				selectedPop = selectedPop + HoN_Matchmaking.popularityTable.REGION[region]
	-- 			end
	-- 		end
	-- 	end
	-- 	if (maxPop > 1) then
	-- 		HoN_Matchmaking.popularityTable.summary.REGION = selectedPop / maxPop
	-- 	else
	-- 		HoN_Matchmaking.popularityTable.summary.REGION = 1
	-- 	end
	-- 	HoN_Matchmaking.popularityTable.summary.TOTAL = HoN_Matchmaking.popularityTable.summary.TOTAL * HoN_Matchmaking.popularityTable.summary.REGION
	-- end

	-- if (HoN_Matchmaking.popularityTable.MAP) then
	-- 	 maxPop, selectedPop = 0, 0
	-- 	for _, map in ipairs(MAPS) do
	-- 		if (mapAllowed[map]) and (HoN_Matchmaking.popularityTable.MAP[map]) then
	-- 			maxPop = maxPop + HoN_Matchmaking.popularityTable.MAP[map]
	-- 			if (mapSelected == map) then
	-- 				selectedPop = selectedPop + HoN_Matchmaking.popularityTable.MAP[map]
	-- 			end
	-- 		end
	-- 	end
	-- 	if (maxPop > 1) then
	-- 		HoN_Matchmaking.popularityTable.summary.MAP = selectedPop / maxPop
	-- 	else
	-- 		HoN_Matchmaking.popularityTable.summary.MAP = 1
	-- 	end
	-- 	HoN_Matchmaking.popularityTable.summary.TOTAL = HoN_Matchmaking.popularityTable.summary.TOTAL * HoN_Matchmaking.popularityTable.summary.MAP
	-- end

	-- if (HoN_Matchmaking.popularityTable.TYPE) then
	-- 	 maxPop, selectedPop = 0, 0
	-- 	for _, gameType in ipairs(MAPS) do
	-- 		if (gameTypeAllowed[gameType]) and (HoN_Matchmaking.popularityTable.TYPE[gameType]) then
	-- 			maxPop = maxPop + HoN_Matchmaking.popularityTable.TYPE[gameType]
	-- 			if (gameTypeSelected == gameType) then
	-- 				selectedPop = selectedPop + HoN_Matchmaking.popularityTable.TYPE[gameType]
	-- 			end
	-- 		end
	-- 	end
	-- 	if (maxPop > 1) then
	-- 		HoN_Matchmaking.popularityTable.summary.TYPE = selectedPop / maxPop
	-- 	else
	-- 		HoN_Matchmaking.popularityTable.summary.TYPE = 1
	-- 	end
	-- 	HoN_Matchmaking.popularityTable.summary.TOTAL = HoN_Matchmaking.popularityTable.summary.TOTAL * HoN_Matchmaking.popularityTable.summary.TYPE
	-- end

	local lowestMMR, highestMMR, disparity = nil, nil, 0
	for index, mmr in pairs(playerMMR) do
		if (lowestMMR) then
			lowestMMR = Min(lowestMMR, mmr)
		else
			lowestMMR = mmr
		end
		if (highestMMR) then
			highestMMR = Max(highestMMR, mmr)
		else
			highestMMR = mmr
		end
	end
	if (highestMMR) and (lowestMMR) then
		disparity = highestMMR - lowestMMR
	end

	if (totalPop < LOW_POP_THRESHOLD) then
		GetWidget('matchmaking_queue_popularity_1'):SetVisible(1)
		GetWidget('matchmaking_queue_popularity_1b'):SetVisible(1)
		GetWidget('matchmaking_queue_popularity_2'):SetText('^o'..Translate('mm_queue_low'))
		GetWidget('matchmaking_queue_popularity_2b'):SetText('^o'..Translate('mm_queue_low'))
	elseif (totalPop < MED_POP_THRESHOLD) then
		GetWidget('matchmaking_queue_popularity_1'):SetVisible(1)
		GetWidget('matchmaking_queue_popularity_1b'):SetVisible(1)
		GetWidget('matchmaking_queue_popularity_2'):SetText('^o'..Translate('mm_queue_med'))
		GetWidget('matchmaking_queue_popularity_2b'):SetText('^o'..Translate('mm_queue_med'))
	else
		GetWidget('matchmaking_queue_popularity_1'):SetVisible(1)
		GetWidget('matchmaking_queue_popularity_1b'):SetVisible(1)
		GetWidget('matchmaking_queue_popularity_2'):SetText('^g'..Translate('mm_queue_high'))
		GetWidget('matchmaking_queue_popularity_2b'):SetText('^g'..Translate('mm_queue_high'))
	end

	local disparityTip = ''
	if (disparity >= HIGH_DISPARITY_THRESHOLD) then
		GetWidget('matchmaking_queue_popularity_3'):SetVisible(1)
		GetWidget('matchmaking_queue_popularity_3b'):SetVisible(1)
		GetWidget('matchmaking_queue_popularity_4'):SetVisible(1)
		GetWidget('matchmaking_queue_popularity_4b'):SetVisible(1)
		GetWidget('matchmaking_queue_popularity_4'):SetText('^r'..Translate('mm_queue_disparity_high'))
		GetWidget('matchmaking_queue_popularity_4b'):SetText('^r'..Translate('mm_queue_disparity_high'))
		disparityTip = '\n\n ' .. Translate('mm_queue_disparity_high_tip')
		GetWidget('matchmaking_queue_display'):SetHeight('29.6h')
	elseif (disparity >= MED_DISPARITY_THRESHOLD) then
		GetWidget('matchmaking_queue_popularity_3'):SetVisible(1)
		GetWidget('matchmaking_queue_popularity_3b'):SetVisible(1)
		GetWidget('matchmaking_queue_popularity_4'):SetVisible(1)
		GetWidget('matchmaking_queue_popularity_4b'):SetVisible(1)
		GetWidget('matchmaking_queue_popularity_4'):SetText('^o'..Translate('mm_queue_disparity_med'))
		GetWidget('matchmaking_queue_popularity_4b'):SetText('^o'..Translate('mm_queue_disparity_med'))
		disparityTip = '\n\n ' .. Translate('mm_queue_disparity_high_tip')
		GetWidget('matchmaking_queue_display'):SetHeight('29.6h')
	else
		GetWidget('matchmaking_queue_popularity_3'):SetVisible(0)
		GetWidget('matchmaking_queue_popularity_3b'):SetVisible(0)
		GetWidget('matchmaking_queue_popularity_4'):SetVisible(0)
		GetWidget('matchmaking_queue_popularity_4b'):SetVisible(0)
		GetWidget('matchmaking_queue_display'):SetHeight('26.2h')
	end

	GetWidget('matchmaking_hover_question_btn_1'):SetCallback('onmouseover', function()
		PlaySound('/shared/sounds/ui/ccpanel/button_over_02.wav')
		Trigger('genericMainFloatingTip', 'true', '35h', '', 'mm_queue_popularity', Translate('mm_popularity2', 'region', (floor(regionPop*100)..'%'), 'mode', (floor(modePop*100)..'%')) .. disparityTip, '', '', '3h', '-2h')
	end)
	GetWidget('matchmaking_hover_question_btn_1'):RefreshCallbacks()

	GetWidget('matchmaking_hover_question_btn_1b'):SetCallback('onmouseover', function()
		PlaySound('/shared/sounds/ui/ccpanel/button_over_02.wav')
		Trigger('genericMainFloatingTip', 'true', '35h', '', 'mm_queue_popularity', Translate('mm_popularity2', 'region', (floor(regionPop*100)..'%'), 'mode', (floor(modePop*100)..'%')) .. disparityTip, '', '', '3h', '-2h')
	end)
	GetWidget('matchmaking_hover_question_btn_1b'):RefreshCallbacks()
end

function HoN_Matchmaking:JoinGroup(player)
	soloQueue = false
	interface:UICmd("JoinTMMGroup('"..player.."');")
	if (not GetWidget("matchmaking"):IsVisible()) then
		Set("_mainmenu_currentpanel", "matchmaking", "string")
		GetWidget("MainMenuPanelSwitcher"):DoEvent()
	end
end

function HoN_Matchmaking:KickBots()
	for i=2,5 do
		if (HoN_Matchmaking.teamMembers[i] and HoN_Matchmaking.teamMembers[i].isBot) then
			SetTeamBot(i-1, "")
			HoN_Matchmaking:MarkBotUsageByID(HoN_Matchmaking.teamMembers[i].botID, false)
			HoN_Matchmaking.teamMembers[i] = nil
		end
	end
	HoN_Matchmaking.teamHasBots = false

	HoN_Matchmaking:PopulateAllBotPickLists()
	HoN_Matchmaking:UpdateButtons()
end

-- -----

function HoN_Matchmaking:ClickInvitePerson(slot)
	if (HoN_Social_Panel and HoN_Social_Panel.groupTable and HoN_Social_Panel.friendslist) then
		HoN_Matchmaking.buddyLists = {}
		for k,v in pairs(HoN_Social_Panel.groupTable) do
			if ((k ~= "inactive") and (k ~= "offline") and (k ~= "add_friend") and (k ~= "recent") and (k ~= "alert") and (k ~= "autocomplete")) then
				if (HoN_Social_Panel.friendslist[k]) then
					table.insert(HoN_Matchmaking.buddyLists, HoN_Social_Panel.friendslist[k])
				end
			end
		end
	end

	Trigger("ChatAutoCompleteClear") --clear and add in the friends and clan
	HoN_Matchmaking:PopulateInviteList(HoN_Matchmaking.invitePersonScrollOffset)
end


local function ShowSingleTabButton(modename)
	GetWidget("mm_category_tab_pvp"):SetButtonState(1)
	GetWidget("mm_category_tab_coop"):SetButtonState(1)
	GetWidget("mm_category_tab_season"):SetButtonState(1)
	GetWidget("mm_category_tab_pvp"):SetEnabled(true)
	GetWidget("mm_category_tab_coop"):SetEnabled(true)
	GetWidget("mm_category_tab_season"):SetEnabled(true)

	GetWidget("mm_category_tab_"..modename):SetButtonState(1)
	GetWidget("mm_category_tab_"..modename):SetEnabled(false)
end

function HoN_Matchmaking:SelectTab(modename, dontAnimate, dontSendMessage)
	if (IsInGroup() and groupLeaderID ~= UIGetAccountID() and HoN_Matchmaking.selectedTab ~= "") then
		modename = HoN_Matchmaking.selectedTab
	end

	if IsInGroup() and groupLeaderID ~= UIGetAccountID() then
		TabAnimate("coop", false, false)
		TabAnimate("pvp", false, false)
		TabAnimate("season", false, false)
	end
	GetWidget('season_not_eligible_mask'):SetVisible(false)	
	GetWidget('mm_start_btn_effect'):SetVisible(true)	
	GetWidget('mm_start_btn_seasonend'):SetVisible(false)

	if (HoN_Matchmaking.selectedTab ~= modename) then
		
		TabAnimate(HoN_Matchmaking.selectedTab, false, false)
		TabAnimate(modename, true, false)
		ShowSingleTabButton(modename)

		if (modename == "coop") then 	-- we are switching to coop
			if (not (IsInGroup() and groupLeaderID ~= UIGetAccountID())) then
				matchmakingOptions.pvpType = tostring(gameTypeSelected)
				matchmakingOptions.pvpMap = tostring(mapSelected)
				matchmakingOptions.pvpRanked = tostring(verifiedOnly)
				FlushMatchmakingOptions()

				verifiedOnly = false
			end

			HoN_Matchmaking.selectedTab = modename
			rulesetMode = 2
			if (IsInGroup() and groupLeaderID == UIGetAccountID() and (not dontSendMessage)) then
				GroupChangeType(3)
			end
		elseif (modename == "pvp") then	-- we are switching to pvp
			if (not (IsInGroup() and groupLeaderID ~= UIGetAccountID())) then
				mapSelected = matchmakingOptions.pvpMap
				gameTypeSelected = TMM_GAME_TYPE_NORMAL
				if GetCvarBool('cl_GarenaEnable') then
					gameTypeSelected = TMM_GAME_TYPE_CASUAL
				end
				matchmakingOptions.gameType = tostring(gameTypeSelected)
				
				if (not basicOptionsOnly) then
					if (matchmakingOptions.pvpRanked == nil) then
						verifiedOnly = IsEligibleForRanked()
					else
						verifiedOnly = AtoB(matchmakingOptions.pvpRanked)
					end

				else
					verifiedOnly = false
				end
			end


			HoN_Matchmaking.selectedTab = modename
			if (IsInGroup() and groupLeaderID == UIGetAccountID()) then
				if (not dontSendMessage) then
					GroupChangeType(2)
				end
				UpdateDefaultRuleset()
			elseif (not IsInGroup()) then
				UpdateDefaultRuleset()
			end
		elseif modename == "season" then 
			local normalRankInfo = GetRankedPlayInfo(0)
			if not normalRankInfo.eligible then
				GetWidget('season_not_eligible_mask'):SetVisible(true)	
			end

			if normalRankInfo.seasonend then
				GetWidget('mm_start_btn_effect'):SetVisible(false)
				GetWidget('mm_start_btn_seasonend'):SetVisible(true)
			end	

			if (not (IsInGroup() and groupLeaderID ~= UIGetAccountID())) then
				mapSelected = "caldavar"
				matchmakingOptions.map = mapSelected
				gameTypeSelected = TMM_GAME_TYPE_CAMPAIGN_NORMAL
				if GetCvarBool('cl_GarenaEnable') then
					gameTypeSelected = TMM_GAME_TYPE_CAMPAIGN_CASUAL
				end
				matchmakingOptions.gameType = tostring(gameTypeSelected)
				verifiedOnly = false
			end


			HoN_Matchmaking.selectedTab = modename
			if (IsInGroup() and groupLeaderID == UIGetAccountID()) then
				if (not dontSendMessage) then
					GroupChangeType(4)
				end
				UpdateDefaultRuleset()
			elseif (not IsInGroup()) then
				UpdateDefaultRuleset()
			end
			if GetWidget('matchmaking'):IsVisible() then 
				Cmd('GetCampaignSeasonRewards')
			end
		end
		if ((not IsInGroup()) or (groupLeaderID == UIGetAccountID())) then
			matchmakingOptions.tab = tostring(modename)
			FlushMatchmakingOptions()
		end
	elseif (modename and modename ~= "") then
		GetWidget("mm_category_tab_"..modename):SetButtonState(1)
		GetWidget("mm_category_tab_"..modename):SetEnabled(false)
	end

	HoN_Matchmaking:CheckSelection()
	HoN_Matchmaking:ScheduleGroupOptionsUpdate()
end

function HoN_Matchmaking:FindBotByName(name)
	for i,hero in pairs(HoN_Matchmaking.botPickTable) do
		for j,bot in pairs(hero.bots) do
			if (bot.name == name) then
				return bot.id
			end
		end
	end
end

function HoN_Matchmaking:GetBotFromName(heroName)
	for i,hero in pairs(HoN_Matchmaking.botPickTable) do
		for j,bot in ipairs(hero.bots) do
			if (bot.name == heroName) then
				return bot
			end
		end
	end
end

function HoN_Matchmaking:GetNameFromID(id)
	for i,hero in ipairs(HoN_Matchmaking.botPickTable) do
		for j,bot in ipairs(hero.bots) do
			if (bot.id == id) then
				return bot.name
			end
		end
	end
end

function HoN_Matchmaking:LeaveUnavailableBot()
	-- leave the group
	interface:UICmd("LeaveTMMGroup()")
	-- dialog box
	Trigger("TriggerDialogBox", "tmm_error", "", "general_close", "", "", "tmm_bot_error", "tmm_bot_error_body")
end

function HoN_Matchmaking:ChangedEnemyBot(_, slot, name)
	slot = tonumber(slot)

	-- nobody is ready after a bot change...
	for i=1, 5 do
		if (HoN_Matchmaking.teamMembers[i]) then
			HoN_Matchmaking.teamMembers[i].isReady = false
		end
	end
	HoN_Matchmaking:RefreshButtons()

	if (name ~= "") then
		local botID = HoN_Matchmaking:FindBotByName(name)
		if (botID) then
			HoN_Matchmaking:MarkBotUsageByID(HoN_Matchmaking.bots[slot+1], false)
			HoN_Matchmaking:MarkBotUsageByID(botID, true)
			HoN_Matchmaking.bots[slot+1] = botID
			HoN_Matchmaking:PopulateEnemyTeam()
		else
			--Echo("^rSomeone added a bot you don't seem to have locally!")
			HoN_Matchmaking:LeaveUnavailableBot()
		end
	end
end
triggerHelper:RegisterWatch("TMMEnemyBotChange", function(...) HoN_Matchmaking:ChangedEnemyBot(...) end)

function HoN_Matchmaking:ChangedTeamBot(_, slot, name)
	slot = tonumber(slot)

	-- nobody is ready after a bot change...
	for i=1, 5 do
		if (HoN_Matchmaking.teamMembers[i]) then
			HoN_Matchmaking.teamMembers[i].isReady = false
		end
	end

	if (name == "") then
		if (HoN_Matchmaking.teamMembers[slot+1] and HoN_Matchmaking.teamMembers[slot+1].isBot) then
			HoN_Matchmaking:MarkBotUsageByID(HoN_Matchmaking.teamMembers[slot+1].botID, false)
			HoN_Matchmaking.teamMembers[slot+1] = nil
		end
	elseif ((not HoN_Matchmaking.teamMembers[slot+1]) or HoN_Matchmaking.teamMembers[slot+1].isBot) then
		local botID = HoN_Matchmaking:FindBotByName(name)
		if (botID) then
			if (HoN_Matchmaking.teamMembers[slot+1] and HoN_Matchmaking.teamMembers[slot+1].isBot) then
				HoN_Matchmaking:MarkBotUsageByID(HoN_Matchmaking.teamMembers[slot+1].botID, false)
			end
			HoN_Matchmaking:MarkBotUsageByID(botID, true)
			HoN_Matchmaking.teamMembers[slot+1] = {}
			HoN_Matchmaking.teamMembers[slot+1].isBot = true
			HoN_Matchmaking.teamMembers[slot+1].botID = botID
		else
			--Echo("^rSomeone added a bot you don't seem to have locally!")
			HoN_Matchmaking:LeaveUnavailableBot()
		end
	end

	HoN_Matchmaking:RefreshButtons()
end
triggerHelper:RegisterWatch("TMMTeamBotChange", function(...) HoN_Matchmaking:ChangedTeamBot(...) end)

-- function HoN_Matchmaking:ClickMapAdvanced(simpleMode)
-- 	simpleMode = AtoB(simpleMode)
-- 	HoN_Matchmaking.simpleMaps = simpleMode
-- 	interface:UICmd("SetSave('_TMM_SimpleMaps_saved', '"..tostring(simpleMode).."')")

-- 	if (not UIIsVerified() and IsInGroup() and groupLeaderID ~= UIGetAccountID()) then
-- 		rulesetMode = 0
-- 	else
-- 		if (simpleMode or Client.IsNewAccount()) then
-- 			if (HoN_Matchmaking.simpleModes or Client.IsNewAccount()) then
-- 				rulesetMode = 5
-- 			else
-- 				rulesetMode = 3
-- 			end
-- 		elseif (HoN_Matchmaking.simpleModes) then
-- 			rulesetMode = 4
-- 		else
-- 			rulesetMode = 0
-- 		end

-- 		defaultRulesetMode = GetDBEntry('def_tmm_tab', rulesetMode, true, false, true)
-- 	end

-- 	HoN_Matchmaking:CheckSelection()
-- 	HoN_Matchmaking:ScheduleGroupOptionsUpdate()
-- end

-- function HoN_Matchmaking:ClickGameModeAdvanced(simpleMode)
-- 	simpleMode = AtoB(simpleMode)
-- 	HoN_Matchmaking.simpleModes = simpleMode
-- 	interface:UICmd("SetSave('_TMM_SimpleModes_saved', '"..tostring(simpleMode).."')")

-- 	if (not UIIsVerified() and IsInGroup() and groupLeaderID ~= UIGetAccountID()) then
-- 		rulesetMode = 0
-- 	else
-- 		if (simpleMode or Client.IsNewAccount()) then
-- 			if (HoN_Matchmaking.simpleMaps or Client.IsNewAccount()) then
-- 				rulesetMode = 5
-- 			else
-- 				rulesetMode = 4
-- 			end
-- 		elseif (HoN_Matchmaking.simpleMaps) then
-- 			rulesetMode = 3
-- 		else
-- 			rulesetMode = 0
-- 		end

-- 		defaultRulesetMode = GetDBEntry('def_tmm_tab', rulesetMode, true, false, true)
-- 	end

-- 	defaultRulesetMode = GetDBEntry('def_tmm_tab', rulesetMode, true, false, true)

-- 	HoN_Matchmaking:CheckSelection()
-- 	HoN_Matchmaking:ScheduleGroupOptionsUpdate()
-- end

local function UIUpdateRegion() --TODO?

	local regionMap, regionsList, regionInfo = HoN_Region:GetMatchmakingRegionInfo()

	if (regionMap) then
		HoN_Matchmaking:SelectTab(HoN_Matchmaking.selectedTab)

		-- remove old region data / widgets
		groupfcall('matchmaking_option_regions', function(index, widget, groupName) widget:UICmd([[DestroyWidget()]]) end)

		REGIONS = regionsList
		REGION_INFO = regionInfo

		HoN_Matchmaking:InitRegions()

		-- game modes
		for _, v in ipairs(GAMEMODES) do
			if (gameModeAllowed[v] == nil) then
				gameModeAllowed[v] = true
				gameModeCompatible[v] = true
			end

			if (not matchmakingOptions["gameMode"..v]) then
				matchmakingOptions["gameMode"..v] = tostring((not ((GAMEMODE_INFO) and (GAMEMODE_INFO[v]) and (GAMEMODE_INFO[v].DEFAULT_OFF))))
			end

			if (gameModeSelected[v] == nil) then
				gameModeSelected[v] = AtoB(matchmakingOptions["gameMode"..v])
			end
		end
		FlushMatchmakingOptions()

		if (REGIONS_MAP[regionMap]) then
			GetWidget("matchmaking_maps"):SetVisible(true)
			for k, _ in pairs(REGIONS_MAP) do
				if (k == regionMap) then
					GetWidget("matchmaking_map_"..k):SetVisible(true)
				else
					GetWidget("matchmaking_map_"..k):SetVisible(false)
				end
			end
		else
			GetWidget("matchmaking_maps"):SetVisible(false)
		end

		-- create new region widgets
		local widget = GetWidget("matchmaking_option_regions_panel")
		for _, v in ipairs(REGIONS) do
			if (not WidgetExists("matchmaking_option_region_"..v)) then
				widget:Instantiate('matchmaking_option_region',
					'regionid', v,
					'label', 'mm_'..v..'_region',
					'icon', regionInfo[v].FLAG
				)
			end
		end

		-- refresh buttons
		HoN_Matchmaking:UpdateButtons()

	end
end
triggerHelper:RegisterWatch("UIUpdateRegion", UIUpdateRegion)

function HoN_Matchmaking:UpdateLeaverBlock()
	if (IsLeaver()) then
		if (not GetWidget("matchmaking_blocker_leaver"):IsVisibleSelf()) and (not HoN_Region.regionTable[HoN_Region.activeRegion].tmmAllowLeavers) then
			PlaySound('/shared/sounds/announcer/denied.wav')
			GetWidget("matchmaking_blocker_leaver"):SetVisible(true)
			Trigger("TMMReset")
		end
	else
		GetWidget("matchmaking_blocker_leaver"):SetVisible(false)
	end
end

function HoN_Matchmaking:UpdateButtons()
	if (basicOptionsOnly) then
		local basicInGroup = false
		for i = 0, 4 do
			if (NotEmpty(playerName[i]) and not playerVerified[i]) then
				basicInGroup = true
				break
			end
		end
		if (not basicInGroup) then
			basicOptionsOnly = false
			Trigger("TMMReset")
			HoN_Matchmaking:ScheduleGroupOptionsUpdate()
			return
		end
	end

	-- matchmaking_option_map
	for _, v in ipairs(MAPS) do
		if (isMapAllowed(v)) then
			if (groupSize > MAP_INFO[v].SIZE) then
				buttonState["matchmaking_option_map_"..v] = "n/a_teamsize"
			elseif (mapSelected == v) then
				buttonState["matchmaking_option_map_"..v] = "yes"
			else
				buttonState["matchmaking_option_map_"..v] = "no"
			end
		else
			if (MAP_INFO[v].HIDE_DISABLED) then
				buttonState["matchmaking_option_map_"..v] = "hidden"
			else
				buttonState["matchmaking_option_map_"..v] = "n/a"
			end
		end
	end

	-- matchmaking_option_gametype
	for _, v in ipairs(GAMETYPES) do
		if (IsGameTypeAllowed(v)) then
			if (gameTypeSelected == v) then
				buttonState["matchmaking_option_gametype_"..v] = "yes"
			elseif (gameTypeSelected < 3) then
				buttonState["matchmaking_option_gametype_"..v] = "no"
			else
				buttonState["matchmaking_option_gametype_"..v] = "n/a"
			end
		else
			buttonState["matchmaking_option_gametype_"..v] = "n/a"
		end
	end

	-- matchmaking_option_gamemode
	local gameModeExists = {}
	for _,gm in ipairs(GAMEMODES) do
		gameModeExists[gm] = true
	end


	gameModeSelectedString = ""
	CountSelectedGameModes()
	local tempGameModeAllowed, tempGameModeAllowedByRegion, isBetaRegion
	for gm, info in pairs(GAMEMODE_INFO) do
		--println('gm = ' .. tostring(gm) .. ' | IsGameModeAllowed(gm) = ' .. tostring(IsGameModeAllowed(gm)) )
		tempGameModeAllowed, tempGameModeAllowedByRegion, isBetaRegion = IsGameModeAllowed(gm)
		if (tempGameModeAllowed) then
			if GetWidget('matchmaking_option_gamemode_'..gm, nil, true) then
				GetWidget('matchmaking_option_gamemode_'..gm, nil, true):SetVisible(true)
			end
			if (gameModeSelected[gm]) then
				if (NotEmpty(gameModeSelectedString)) then
					gameModeSelectedString = gameModeSelectedString.."|"
				end
				gameModeSelectedString = gameModeSelectedString..gm
				if (gameModeSelectedCount > MIN_SELECTED_GAMEMODES) then
					buttonState["matchmaking_option_gamemode_"..gm] = "yes"
				else
					buttonState["matchmaking_option_gamemode_"..gm] = "yes_disabled"
				end
			else
				buttonState["matchmaking_option_gamemode_"..gm] = "no"
			end
		else
			if GetWidget('matchmaking_option_gamemode_'..gm, nil, true) and ((info.HIDE_DISABLED) or (not gameModeExists[gm])) then
				GetWidget('matchmaking_option_gamemode_'..gm, nil, true):SetVisible(false)
			elseif GetWidget('matchmaking_option_gamemode_'..gm, nil, true) then
				GetWidget('matchmaking_option_gamemode_'..gm, nil, true):SetVisible(true)
			end

			if (not tempGameModeAllowedByRegion) and (isBetaRegion) then
				buttonState["matchmaking_option_gamemode_"..gm] = "n/a_betaregion"
			elseif (not tempGameModeAllowedByRegion) then
				buttonState["matchmaking_option_gamemode_"..gm] = "n/a"
			elseif (info) and (CanAccessGameMode(info.MODE)) then
				buttonState["matchmaking_option_gamemode_"..gm] = "n/a"
			elseif (info) and (info.PASS) then
				buttonState["matchmaking_option_gamemode_"..gm] = "n/a_needtokenpass"
			elseif (info) and NeedsToken(info.MODE) then
				buttonState["matchmaking_option_gamemode_"..gm] = "n/a_needtoken"
			elseif (groupSize) and ( ((RULESET_INFO[rulesetMode]) and (RULESET_INFO[rulesetMode].MIN_TEAM_SIZE) and (groupSize < RULESET_INFO[rulesetMode].MIN_TEAM_SIZE)) or ((info) and (info.MIN_TEAM_SIZE) and (groupSize < info.MIN_TEAM_SIZE)) ) then
				buttonState["matchmaking_option_gamemode_"..gm] = "n/a_groupsize"
			else
				buttonState["matchmaking_option_gamemode_"..gm] = "n/a"
			end
		end
	end


	-- if HoN_Matchmaking.selectedTab == "season" then
	-- 	gameModeSelectedString = "ap"
	-- end

	-- matchmaking_option_region
	regionSelectedString = ""
	CountSelectedRegions()
	for _, v in ipairs(REGIONS) do
		if (regionAllowed[v]) then
			if (regionSelected[v]) then
				if (NotEmpty(regionSelectedString)) then
					regionSelectedString = regionSelectedString.."|"
				end
				regionSelectedString = regionSelectedString..REGION_INFO[v].ID
				if (regionSelectedCount > MIN_SELECTED_REGIONS) then
					buttonState["matchmaking_option_region_"..v] = "yes"
				else
					buttonState["matchmaking_option_region_"..v] = "yes_disabled"
				end
				if (WidgetExists("matchmaking_map_region_"..v)) then
					GetWidget("matchmaking_map_region_"..v):SetVisible(true)
				end
			else
				buttonState["matchmaking_option_region_"..v] = "no"
				if (WidgetExists("matchmaking_map_region_"..v)) then
					GetWidget("matchmaking_map_region_"..v):SetVisible(false)
				end
			end
		else
			if (not CanPlayerAccessRegion(v)) then
				buttonState["matchmaking_option_region_"..v] = "n/a_localplayerregion"
			elseif (not CanGroupAccessRegion(v)) then
				buttonState["matchmaking_option_region_"..v] = "n/a_groupmateregion"
			else
				buttonState["matchmaking_option_region_"..v] = "n/a"
			end
			if (WidgetExists("matchmaking_map_region_"..v)) then
				GetWidget("matchmaking_map_region_"..v):SetVisible(false)
			end
		end
	end

	-- if (mapSelected and MAP_INFO[mapSelected].MMR) then
	-- 	for i = 1,2,1 do
	-- 		GetWidget("tmm_infopanel_stats_mmr_"..i):SetVisible(true)
	-- 		GetWidget("tmm_infopanel_stats_nommr_"..i):SetVisible(false)
	-- 	end
	-- else
	-- 	for i = 1,2,1 do
	-- 		GetWidget("tmm_infopanel_stats_mmr_"..i):SetVisible(false)
	-- 		GetWidget("tmm_infopanel_stats_nommr_"..i):SetVisible(true)
	-- 	end
	-- end

	-- calculate minimum group size
	if (mapSelected) and (rulesetMode) then
		selectedMinGroupSize = 1
		selectedMinGroupSize = Max(selectedMinGroupSize, RULESET_INFO[rulesetMode].MIN_TEAM_SIZE)
		local info
		for _, v in ipairs(GAMEMODES) do
			info = GetGameModeInfo(v)
			if (info) and gameModeSelected[v] then
				selectedMinGroupSize = Max(selectedMinGroupSize, info.MIN_TEAM_SIZE)
			end
		end
	end

	-- GetWidget("matchmaking_button_group_not_full"):SetVisible(groupSize < selectedMinGroupSize)
	-- GetWidget('matchmaking_button_play_solo'):SetEnabled(groupSize >= selectedMinGroupSize)

	-- region cover
	if HoN_Matchmaking.selectedTab == "coop" then
		-- search for human comrades, not, the toggle will need to be taken into account soon
		local haveFriends = false
		for i=2,5 do
			if (HoN_Matchmaking.teamMembers[i] and not HoN_Matchmaking.teamMembers[i].isBot) then
				haveFriends = true
				break
			end
		end

		if (haveFriends or not HoN_Matchmaking.teamHasBots) then
			GetWidget("mm_region_cover"):FadeOut(100)
			GetWidget("mm_start_practice_btn"):FadeOut(100)
			GetWidget("mm_start_btn"):FadeIn(100)
		else
			GetWidget("mm_region_cover"):FadeIn(100)
			GetWidget("mm_start_practice_btn"):FadeIn(100)
			GetWidget("mm_start_btn"):FadeOut(100)
		end
	elseif HoN_Matchmaking.selectedTab == "pvp" then
		GetWidget("mm_region_cover"):FadeOut(100)
		GetWidget("mm_start_practice_btn"):FadeOut(100)
		GetWidget("mm_start_btn"):FadeIn(100)
	else
		GetWidget("mm_region_cover"):FadeOut(100)
		GetWidget("mm_start_practice_btn"):FadeOut(100)
		GetWidget("mm_start_btn"):FadeIn(100)
	end

	-- random enemy bots cover + button
	HoN_Matchmaking:UpdateRandomEnemyBotsCover()
	-- difficulty slider
	HoN_Matchmaking:UpdateDifficultySlider()

	HoN_Matchmaking:UpdatePlayerStats()
	HoN_Matchmaking:RefreshButtons()
end

function HoN_Matchmaking:UpdateRandomEnemyBotsCover()
	if (randomEnemyBots) then
		GetWidget("mm_bot_advanced"):Sleep(50, function()
			GetWidget("mm_bot_advanced"):SetText(Translate("mm3_bot_custom"))
			GetWidget("mm_bot_advanced_arrow"):Rotate("-180", 150)
		end)
		GetWidget("mm_enemy_bot_cover"):SlideY("5.0h", 300)
	else
		GetWidget("mm_bot_advanced"):Sleep(50, function()
			GetWidget("mm_bot_advanced"):SetText(Translate("mm3_bot_random"))
		end)
		GetWidget("mm_enemy_bot_cover"):SlideY("-30.0h", 300)
		GetWidget("mm_bot_advanced_arrow"):Rotate("0", 150)
	end
end

function HoN_Matchmaking:UpdateDifficultySlider()
	if (botDifficulty) then
		--GetWidget("mm_bot_difficulty_image"):DoEventN(botDifficulty)
		GetWidget("mm_bot_difficulty_label"):DoEventN(botDifficulty)
		GetWidget("mm_bot_difficulty_slider"):SetValue(botDifficulty)

		-- we can put events on the slide slot, so we will do it here
		if (botDifficulty == 1) then
			GetWidget('mm_bot_difficulty_slider_slot'):SetColor('#00FF2F')
		elseif (botDifficulty == 2) then
			GetWidget('mm_bot_difficulty_slider_slot'):SetColor('#FFBF00')
		else
			GetWidget('mm_bot_difficulty_slider_slot'):SetColor('#FF0000')
		end
	end
end

function HoN_Matchmaking:Random_enemy_bots_click()
	randomEnemyBots = not randomEnemyBots
	matchmakingOptions.randomEnemyBots = tostring(randomEnemyBots)
	FlushMatchmakingOptions()

	-- toggle the usage on/off for enemy bots
	for i,id in pairs(HoN_Matchmaking.bots) do
		HoN_Matchmaking:MarkBotUsageByID(id, (not randomEnemyBots))
	end
	HoN_Matchmaking:PopulateAllBotPickLists()

	HoN_Matchmaking:ScheduleGroupOptionsUpdate()
	HoN_Matchmaking:UpdateButtons()
end

function HoN_Matchmaking:ChangeBotDifficulty(difficulty)
	difficulty = tonumber(difficulty)
	Set("g_botDifficulty", difficulty, "int")
	-- only update if leader or not in group, not much of a better way for me to do this
	if (not IsInGroup() or (IsInGroup() and groupLeaderID == UIGetAccountID())) then
		matchmakingOptions.botDifficulty = tostring(difficulty)
		FlushMatchmakingOptions()
	end
	SetBotDifficulty(difficulty)

	botDifficulty = difficulty

	HoN_Matchmaking:CheckSelection()
	HoN_Matchmaking:ScheduleGroupOptionsUpdate()
end

function HoN_Matchmaking:RefreshButtons()
	local mouseover, state, stateShown

	local mapsAvailable = 0
	if HoN_Matchmaking.selectedTab == "pvp" then
		-- matchmaking_option_map
		for _, v in ipairs(MAPS) do
			local button = "matchmaking_option_map_"..v
			local widget = button.."_1"

			state = buttonState[button]
			stateShown = buttonStateShown[widget]
			mouseover = ((mouseoverButton == button) and (state == "no"))

			if (state == "hidden") then
				GetWidget(widget):SetVisible(0)
			else
				GetWidget(widget):SetVisible(1)
				mapsAvailable = mapsAvailable + 1
			end

			if (WidgetExists(widget) and stateShown ~= (state..(mouseover and "_mo" or ""))) then
				if (state == "yes") then
					GetWidget(widget.."_backdrop"):SetColor("1 1 1 .8")
					GetWidget(widget.."_backdrop"):SetBorderColor("1 1 1 .8")
					GetWidget(widget.."_border"):SetBorderColor("#455166")
					GetWidget(widget.."_cornerarrow"):SetVisible(true)
					GetWidget(widget.."_beta"):SetColor("1 1 1 1")
					GetWidget(widget.."_beta"):UICmd([[SetRendermode('normal')]])
					GetWidget(widget.."_name"):SetColor("#ffaa00")
					GetWidget(widget.."_minimap"):UICmd([[SetRendermode('normal')]])
					GetWidget(widget.."_minimap_border"):SetBorderColor("#455166")
					GetWidget(widget.."_minimap_thumb"):SetVisible(true)
					if GetWidget(widget.."_teamsize_label", nil, true) then
						GetWidget(widget.."_teamsize_label"):SetColor("1 1 1 1")
						GetWidget(widget.."_teamsize_text"):SetColor("1 1 1 1")
						GetWidget(widget.."_icon"):SetColor("1 1 1 1")
						GetWidget(widget.."_icon"):UICmd([[SetRendermode('normal')]])
					end
					GetWidget(widget.."_disabled"):SetVisible(false)
				elseif (state == "no") then
					if (mouseover) then
						GetWidget(widget.."_backdrop"):SetColor("1 1 1 .8")
						GetWidget(widget.."_backdrop"):SetBorderColor("1 1 1 .8")
						GetWidget(widget.."_border"):SetBorderColor(".2 .2 .2 1")
						GetWidget(widget.."_cornerarrow"):SetVisible(false)
						GetWidget(widget.."_beta"):SetColor("1 1 1 .5")
						GetWidget(widget.."_beta"):UICmd([[SetRendermode('normal')]])
						GetWidget(widget.."_name"):SetColor("1 1 1 1")
						GetWidget(widget.."_minimap"):UICmd([[SetRendermode('normal')]])
						GetWidget(widget.."_minimap_border"):SetBorderColor("#455166")
						GetWidget(widget.."_minimap_thumb"):SetVisible(false)
						if GetWidget(widget.."_teamsize_label", nil, true) then
							GetWidget(widget.."_teamsize_label"):SetColor("1 1 1 1")
							GetWidget(widget.."_teamsize_text"):SetColor("1 1 1 1")
							GetWidget(widget.."_icon"):SetColor("1 1 1 1")
							GetWidget(widget.."_icon"):UICmd([[SetRendermode('normal')]])
						end
						GetWidget(widget.."_disabled"):SetVisible(false)
					else
						GetWidget(widget.."_backdrop"):SetColor("1 1 1 .3")
						GetWidget(widget.."_backdrop"):SetBorderColor("1 1 1 .3")
						GetWidget(widget.."_border"):SetBorderColor(".2 .2 .2 1")
						GetWidget(widget.."_cornerarrow"):SetVisible(false)
						GetWidget(widget.."_beta"):SetColor("1 1 1 .5")
						GetWidget(widget.."_beta"):UICmd([[SetRendermode('normal')]])
						GetWidget(widget.."_name"):SetColor(".5 .5 .5 1")
						GetWidget(widget.."_minimap"):UICmd([[SetRendermode('normal')]])
						GetWidget(widget.."_minimap_border"):SetBorderColor(".2 .2 .2 1")
						GetWidget(widget.."_minimap_thumb"):SetVisible(false)
						if GetWidget(widget.."_teamsize_label", nil, true) then
							GetWidget(widget.."_teamsize_label"):SetColor(".5 .5 .5 1")
							GetWidget(widget.."_teamsize_text"):SetColor(".5 .5 .5 1")
							GetWidget(widget.."_icon"):SetColor("1 1 1 .5")
							GetWidget(widget.."_icon"):UICmd([[SetRendermode('normal')]])
						end
						GetWidget(widget.."_disabled"):SetVisible(false)
					end
				else -- disabled
					GetWidget(widget.."_backdrop"):SetColor("1 1 1 .3")
					GetWidget(widget.."_backdrop"):SetBorderColor("1 1 1 .3")
					GetWidget(widget.."_border"):SetBorderColor(".2 .2 .2 1")
					GetWidget(widget.."_cornerarrow"):SetVisible(false)
					GetWidget(widget.."_beta"):SetColor("1 1 1 .5")
					GetWidget(widget.."_beta"):UICmd([[SetRendermode('grayscale')]])
					GetWidget(widget.."_name"):SetColor(".5 .5 .5 1")
					GetWidget(widget.."_minimap"):UICmd([[SetRendermode('grayscale')]])
					GetWidget(widget.."_minimap_border"):SetBorderColor(".2 .2 .2 1")
					GetWidget(widget.."_minimap_thumb"):SetVisible(false)
					if GetWidget(widget.."_teamsize_label", nil, true) then
						GetWidget(widget.."_teamsize_label"):SetColor(".5 .5 .5 1")
						GetWidget(widget.."_teamsize_text"):SetColor(".5 .5 .5 1")
						GetWidget(widget.."_icon"):SetColor("1 1 1 .5")
						GetWidget(widget.."_icon"):UICmd([[SetRendermode('grayscale')]])
					end
					GetWidget(widget.."_disabled"):SetVisible(true)
					if (state == "n/a_teamsize") then
						GetWidget(widget.."_disabled_text"):SetText(Translate("mm_map_locked"))
					else
						GetWidget(widget.."_disabled_text"):SetText("")
					end
				end
				buttonStateShown[widget] = (state..(mouseover and "_mo" or ""))
			end
		end

		-- matchmaking_option_gametype
		for _, v in ipairs(GAMETYPES) do
			local button = "matchmaking_option_gametype_"..v
			local widget = button.."_1"
			state = buttonState[button]
			stateShown = buttonStateShown[widget]
			mouseover = ((mouseoverButton == button) and (state == "no"))

			if (WidgetExists(widget) and stateShown ~= (state..(mouseover and "_mo" or ""))) then
				if (state == "yes") then
					GetWidget(widget.."_frame"):SetVisible(false)
					GetWidget(widget.."_frame_color"):SetColor(GAMETYPE_COLOR[v].background)
					GetWidget(widget.."_frame_color"):SetVisible(true)
					GetWidget(widget.."_thumb_background"):SetColor("1 1 1 1")
					GetWidget(widget.."_thumb_background"):SetBorderColor("1 1 1 1")
					GetWidget(widget.."_thumb_border"):SetColor("#32e724")
					GetWidget(widget.."_thumb_border"):SetBorderColor("#32e724")
					GetWidget(widget.."_thumb_icon"):SetColor(".8 .8 .8 1")
					GetWidget(widget.."_thumb_icon"):SetTexture("/ui/fe2/elements/vote_up.tga")
					GetWidget(widget.."_thumb_icon"):UICmd([[SetRendermode('normal')]])
					GetWidget(widget.."_label"):SetColor(GAMETYPE_COLOR[v].label)
					GetWidget(widget.."_disabled"):SetVisible(false)
				elseif (state == "no") then
					if (mouseover) then
						GetWidget(widget.."_frame"):SetVisible(true)
						GetWidget(widget.."_frame_color"):SetVisible(false)
						GetWidget(widget.."_thumb_background"):SetColor(".8 .8 .8 1")
						GetWidget(widget.."_thumb_background"):SetBorderColor(".8 .8 .8 1")
						GetWidget(widget.."_thumb_border"):SetColor(".5 .5 .5 1")
						GetWidget(widget.."_thumb_border"):SetBorderColor(".5 .5 .5 1")
						GetWidget(widget.."_thumb_icon"):SetColor("1 1 1 1")
						GetWidget(widget.."_thumb_icon"):SetTexture("/ui/fe2/elements/vote_down.tga")
						GetWidget(widget.."_thumb_icon"):UICmd([[SetRendermode('grayscale')]])
						GetWidget(widget.."_label"):SetColor(".6 .6 .6 1")
						GetWidget(widget.."_disabled"):SetVisible(false)
					else
						GetWidget(widget.."_frame"):SetVisible(true)
						GetWidget(widget.."_frame_color"):SetVisible(false)
						GetWidget(widget.."_thumb_background"):SetColor(".6 .6 .6 1")
						GetWidget(widget.."_thumb_background"):SetBorderColor(".6 .6 .6 1")
						GetWidget(widget.."_thumb_border"):SetColor(".3 .3 .3 1")
						GetWidget(widget.."_thumb_border"):SetBorderColor(".3 .3 .3 1")
						GetWidget(widget.."_thumb_icon"):SetColor(".8 .8 .8 1")
						GetWidget(widget.."_thumb_icon"):SetTexture("/ui/fe2/elements/vote_down.tga")
						GetWidget(widget.."_thumb_icon"):UICmd([[SetRendermode('grayscale')]])
						GetWidget(widget.."_label"):SetColor(".6 .6 .6 1")
						GetWidget(widget.."_disabled"):SetVisible(false)
					end
				else -- disabled
					GetWidget(widget.."_frame"):SetVisible(true)
					GetWidget(widget.."_frame_color"):SetVisible(false)
					GetWidget(widget.."_thumb_background"):SetColor(".6 .6 .6 1")
					GetWidget(widget.."_thumb_background"):SetBorderColor(".6 .6 .6 1")
					GetWidget(widget.."_thumb_border"):SetColor(".3 .3 .3 1")
					GetWidget(widget.."_thumb_border"):SetBorderColor(".3 .3 .3 1")
					GetWidget(widget.."_thumb_icon"):SetColor(".8 .8 .8 1")
					GetWidget(widget.."_thumb_icon"):SetTexture("/ui/fe2/elements/vote_down.tga")
					GetWidget(widget.."_thumb_icon"):UICmd([[SetRendermode('grayscale')]])
					GetWidget(widget.."_label"):SetColor(".6 .6 .6 1")
					GetWidget(widget.."_disabled"):SetVisible(true)
				end
			end
			buttonStateShown[widget] = (state..(mouseover and "_mo" or ""))
		end

		-- matchmaking_option_gamemode
		for gm, info in pairs(GAMEMODE_INFO) do
			local button = "matchmaking_option_gamemode_"..gm
			state = buttonState[button]
			stateShown = buttonStateShown[button]
			mouseover = ((mouseoverButton == button) and (state ~= "yes_disabled"))

			-- popularity
			if (info and IsGameModeAllowed(gm) and mapSelected) then
				HoN_Matchmaking:UpdatePopularityBar("matchmaking_option_gamemode_"..gm.."_popularity", GetPopularity("gamemode", tostring(MidwarsShennanigans(mapSelected, gameTypeSelected) or -1), mapSelected, gm, "-1", true))
			else
				HoN_Matchmaking:UpdatePopularityBar("matchmaking_option_gamemode_"..gm.."_popularity", 0)
			end

			if (WidgetExists(button) and (stateShown ~= (state..(mouseover and "_mo" or "")) or forceModeButtonsRefresh)) then
				if (info) then
					GetWidget(button.."_icon"):SetTexture("/ui/icons/"..info.ICON..".tga")
					GetWidget(button.."_text"):SetText(Translate(info.NAME))
				end
				if (state == "yes") then
					GetWidget(button.."_thumb_background"):SetColor(mouseover and "#1d6f0f" or "white")
					GetWidget(button.."_thumb_background"):SetBorderColor(mouseover and "#1d6f0f" or "white")
					GetWidget(button.."_thumb_border"):SetColor("#32e724")
					GetWidget(button.."_thumb_border"):SetBorderColor("#32e724")
					GetWidget(button.."_thumb_icon"):SetVisible(true)
					GetWidget(button.."_thumb_icon"):SetColor(".8 .8 .8 1")
					GetWidget(button.."_thumb_icon"):SetTexture("/ui/fe2/elements/vote_up.tga")
					GetWidget(button.."_icon"):UICmd([[SetRendermode('normal')]])
					GetWidget(button.."_text"):SetColor("#32e724")
					GetWidget(button.."_disabled_panel"):SetVisible(false)
					GetWidget(button.."_buytokens"):SetVisible(false)
				elseif (state == "yes_disabled") then
					GetWidget(button.."_thumb_background"):SetColor(".7 .7 .7 1")
					GetWidget(button.."_thumb_background"):SetBorderColor(".7 .7 .7 1")
					GetWidget(button.."_thumb_border"):SetColor("#0c3405")
					GetWidget(button.."_thumb_border"):SetBorderColor("#0c3405")
					GetWidget(button.."_thumb_icon"):SetVisible(true)
					GetWidget(button.."_thumb_icon"):SetColor(".3 .3 .3 1")
					GetWidget(button.."_thumb_icon"):SetTexture("/ui/fe2/elements/vote_up.tga")
					GetWidget(button.."_icon"):UICmd([[SetRendermode('normal')]])
					GetWidget(button.."_text"):SetColor("#1b700c")
					GetWidget(button.."_disabled_panel"):SetVisible(false)
					GetWidget(button.."_buytokens"):SetVisible(false)
				elseif (state == "no") then
					GetWidget(button.."_thumb_background"):SetColor(mouseover and "#8e2a2a" or "white")
					GetWidget(button.."_thumb_background"):SetBorderColor(mouseover and "#8e2a2a" or "white")
					GetWidget(button.."_thumb_border"):SetColor("red")
					GetWidget(button.."_thumb_border"):SetBorderColor("red")
					GetWidget(button.."_thumb_icon"):SetVisible(true)
					GetWidget(button.."_thumb_icon"):SetColor(".8 .8 .8 1")
					GetWidget(button.."_thumb_icon"):SetTexture("/ui/fe2/elements/vote_down.tga")
					GetWidget(button.."_icon"):UICmd([[SetRendermode('normal')]])
					GetWidget(button.."_text"):SetColor("red")
					GetWidget(button.."_disabled_panel"):SetVisible(false)
					GetWidget(button.."_buytokens"):SetVisible(false)
				else -- n/a
					GetWidget(button.."_thumb_background"):SetColor(".7 .7 .7 1")
					GetWidget(button.."_thumb_background"):SetBorderColor(".7 .7 .7 1")
					GetWidget(button.."_thumb_border"):SetColor("#0c3405")
					GetWidget(button.."_thumb_border"):SetBorderColor("#0c3405")
					GetWidget(button.."_thumb_icon"):SetVisible(false)
					GetWidget(button.."_icon"):UICmd([[SetRendermode('grayscale')]])
					GetWidget(button.."_text"):SetColor("#888888")
					if (state == "n/a_verified") then
						GetWidget(button.."_disabled_panel"):SetVisible(true)
						GetWidget(button.."_disabled_text"):SetText(Translate("general_mode_verified")) --TODO: new string?
						GetWidget(button.."_buytokens"):SetVisible(false)
					elseif (state == "n/a_basicplayer") then
						GetWidget(button.."_disabled_panel"):SetVisible(true)
						GetWidget(button.."_disabled_text"):SetText(Translate("mm_mode_disabled_basicplayer")) --TODO: new string?
						GetWidget(button.."_buytokens"):SetVisible(false)
					elseif (state == "n/a_needtokenpass") then
						GetWidget(button.."_disabled_panel"):SetVisible(not mouseover)
						GetWidget(button.."_disabled_text"):SetText(Translate("mm_option_disabled_needtokenpass"))
						GetWidget(button.."_buytokens"):SetVisible(mouseover)
					elseif (state == "n/a_needtoken") then
						GetWidget(button.."_disabled_panel"):SetVisible(not mouseover)
						GetWidget(button.."_disabled_text"):SetText(Translate("mm_option_disabled_needtoken"))
						GetWidget(button.."_buytokens"):SetVisible(mouseover)
					elseif (state == "n/a_betaregion") then
						GetWidget(button.."_disabled_panel"):SetVisible(true)
						GetWidget(button.."_disabled_text"):SetText(Translate("mm_option_disabled_betaregion"))
						GetWidget(button.."_buytokens"):SetVisible(false)
					elseif (state == "n/a_groupsize") then
						GetWidget(button.."_disabled_panel"):SetVisible(true)
						GetWidget(button.."_disabled_text"):SetText(Translate("mm_option_disabled_groupsize"))
						GetWidget(button.."_buytokens"):SetVisible(false)
					else
						GetWidget(button.."_disabled_panel"):SetVisible(true)
						GetWidget(button.."_disabled_text"):SetText(Translate("mm_option_disabled"))
						GetWidget(button.."_buytokens"):SetVisible(false)
					end
				end
				buttonStateShown[button] = (state..(mouseover and "_mo" or ""))
			end
		end
	elseif HoN_Matchmaking.selectedTab == "season" then
		local gameTypes = {"normal", "casual"}
		local gameTypesID = {TMM_GAME_TYPE_CAMPAIGN_NORMAL, TMM_GAME_TYPE_CAMPAIGN_CASUAL}

		for _i, gameType in ipairs(gameTypes) do
			GetWidget("season_gm_allpick_button_"..gameType.."_default"):SetVisible(0)
			GetWidget("season_gm_allpick_button_"..gameType.."_normal"):SetVisible(0)
			GetWidget("season_gm_allpick_button_"..gameType.."_casual"):SetVisible(0)
		end
		for _i, gameType in ipairs(gameTypes) do
			local shouldHighLight = mouseoverButton == "season_gm_allpick_button_"..gameType or gameTypesID[_i] == gameTypeSelected
			if shouldHighLight then
				GetWidget("season_gm_allpick_button_"..gameType.."_"..gameType):SetVisible(1)
			else
				GetWidget("season_gm_allpick_button_"..gameType.."_default"):SetVisible(1)
			end
		end

		-- thumbs 
		local thumb_normal_up = GetWidget("season_gm_allpick_button_normal_up_thumb")
		local thumb_normal_down = GetWidget("season_gm_allpick_button_normal_down_thumb")
		local thumb_casual_up = GetWidget("season_gm_allpick_button_casual_up_thumb")
		local thumbA_casual_down = GetWidget("season_gm_allpick_button_casual_down_thumb")
		thumb_normal_up:SetRenderMode('grayscale')
		thumb_normal_down:SetRenderMode('grayscale')
		thumb_casual_up:SetRenderMode('grayscale')
		thumbA_casual_down:SetRenderMode('grayscale')
		thumb_normal_up:SetVisible(0)
		thumb_normal_down:SetVisible(0)
		thumb_casual_up:SetVisible(0)
		thumbA_casual_down:SetVisible(0)

		local normalRankInfo = GetRankedPlayInfo(0)
		local casualRankInfo = GetRankedPlayInfo(1)

		local pickmodes_normal = explode('|', normalRankInfo.pickmode)
		if #pickmodes_normal == 1 and NotEmpty(pickmodes_normal[1]) then
			GetWidget("season_gm_allpick_button_normal_pickmode_icon"):SetTexture('/ui/icons/'..GAMEMODE_INFO[pickmodes_normal[1]].ICON..'.tga')

			GetWidget("season_gm_allpick_button_normal_pickmode_text"):SetText(Translate(GAMEMODE_INFO[pickmodes_normal[1]].NAME))
			GetWidget("season_gm_allpick_button_normal_pickmode_text_1"):SetText('')
			GetWidget("season_gm_allpick_button_normal_pickmode_text_2"):SetText('')
		elseif #pickmodes_normal >= 2 then
			GetWidget("season_gm_allpick_button_normal_pickmode_icon"):SetTexture('/ui/icons/'..GAMEMODE_INFO[pickmodes_normal[1]].ICON..'.tga')

			GetWidget("season_gm_allpick_button_normal_pickmode_text"):SetText('')
			GetWidget("season_gm_allpick_button_normal_pickmode_text_1"):SetText(Translate(GAMEMODE_INFO[pickmodes_normal[1]].NAME))
			GetWidget("season_gm_allpick_button_normal_pickmode_text_2"):SetText(Translate(GAMEMODE_INFO[pickmodes_normal[2]].NAME))
		else
			GetWidget("season_gm_allpick_button_normal_pickmode_icon"):SetTexture('$invis')
			GetWidget("season_gm_allpick_button_normal_pickmode_text"):SetText('')
			GetWidget("season_gm_allpick_button_normal_pickmode_text_1"):SetText('')
			GetWidget("season_gm_allpick_button_normal_pickmode_text_2"):SetText('')
		end

		local pickmodes_casual = explode('|', casualRankInfo.pickmode)
		if #pickmodes_casual == 1 and NotEmpty(pickmodes_casual[1]) then
			GetWidget("season_gm_allpick_button_casual_pickmode_icon"):SetTexture('/ui/icons/'..GAMEMODE_INFO[pickmodes_casual[1]].ICON..'.tga')

			GetWidget("season_gm_allpick_button_casual_pickmode_text"):SetText(Translate(GAMEMODE_INFO[pickmodes_casual[1]].NAME))
			GetWidget("season_gm_allpick_button_casual_pickmode_text_1"):SetText('')
			GetWidget("season_gm_allpick_button_casual_pickmode_text_2"):SetText('')
		elseif #pickmodes_casual >= 2 then
			GetWidget("season_gm_allpick_button_casual_pickmode_icon"):SetTexture('/ui/icons/'..GAMEMODE_INFO[pickmodes_casual[1]].ICON..'.tga')

			GetWidget("season_gm_allpick_button_casual_pickmode_text"):SetText('')
			GetWidget("season_gm_allpick_button_casual_pickmode_text_1"):SetText(Translate(GAMEMODE_INFO[pickmodes_casual[1]].NAME))
			GetWidget("season_gm_allpick_button_casual_pickmode_text_2"):SetText(Translate(GAMEMODE_INFO[pickmodes_casual[2]].NAME))
		else
			GetWidget("season_gm_allpick_button_casual_pickmode_icon"):SetTexture('$invis')
			GetWidget("season_gm_allpick_button_casual_pickmode_text"):SetText('')
			GetWidget("season_gm_allpick_button_casual_pickmode_text_1"):SetText('')
			GetWidget("season_gm_allpick_button_casual_pickmode_text_2"):SetText('')
		end

		if gameTypeSelected == TMM_GAME_TYPE_CAMPAIGN_NORMAL then
			gameModeSelectedString = 'rb'

			GetWidget("season_gm_allpick_button_normal_label"):SetColor("#32e724")
			GetWidget("season_gm_allpick_button_casual_label"):SetColor("#81817f")

			thumb_normal_up:SetVisible(1)
			thumb_normal_up:SetRenderMode('normal')
			thumbA_casual_down:SetVisible(1)

			HoN_Matchmaking:RefreshRankedPlayInfo(0)
		elseif gameTypeSelected == TMM_GAME_TYPE_CAMPAIGN_CASUAL then
			gameModeSelectedString = 'rb'

			GetWidget("season_gm_allpick_button_casual_label"):SetColor("#37b2d9")
			GetWidget("season_gm_allpick_button_normal_label"):SetColor("#81817f")

			thumb_normal_down:SetVisible(1)
			thumb_casual_up:SetVisible(1)
			thumb_casual_up:SetRenderMode('normal')

			HoN_Matchmaking:RefreshRankedPlayInfo(1)
		end
	end
	forceModeButtonsRefresh = false	-- don't know if this should be in the above if statement or not

	-- matchmaking_option_region
	for _, v in ipairs(REGIONS) do
		local button = "matchmaking_option_region_"..v
		state = buttonState[button]
		stateShown = buttonStateShown[button]
		mouseover = ((mouseoverButton == button) and (state ~= "yes_disabled"))

		-- popularity
		if (REGION_INFO[v] and mapSelected) then
			HoN_Matchmaking:UpdatePopularityBar("matchmaking_option_region_"..v.."_popularity", GetPopularity("region", tostring(MidwarsShennanigans(mapSelected, gameTypeSelected) or -1), mapSelected, "-1", REGION_INFO[v].ID, true))
		else
			HoN_Matchmaking:UpdatePopularityBar("matchmaking_option_region_"..v.."_popularity", 0)
		end

		if (WidgetExists(button) and stateShown ~= (state..(mouseover and "_mo" or ""))) then
			if (state == "yes") then
				GetWidget(button.."_thumb_background"):SetColor(mouseover and "#1d6f0f" or "white")
				GetWidget(button.."_thumb_background"):SetBorderColor(mouseover and "#1d6f0f" or "white")
				GetWidget(button.."_thumb_border"):SetColor("#32e724")
				GetWidget(button.."_thumb_border"):SetBorderColor("#32e724")
				GetWidget(button.."_thumb_icon"):SetVisible(true)
				GetWidget(button.."_thumb_icon"):SetColor(".8 .8 .8 1")
				GetWidget(button.."_thumb_icon"):SetTexture("/ui/fe2/elements/vote_up.tga")
				GetWidget(button.."_icon"):UICmd([[SetRendermode('normal')]])
				GetWidget(button.."_text"):SetColor("#32e724")
				GetWidget(button.."_disabled_panel"):SetVisible(false)
			elseif (state == "yes_disabled") then
				GetWidget(button.."_thumb_background"):SetColor(".7 .7 .7 1")
				GetWidget(button.."_thumb_background"):SetBorderColor(".7 .7 .7 1")
				GetWidget(button.."_thumb_border"):SetColor("#0c3405")
				GetWidget(button.."_thumb_border"):SetBorderColor("#0c3405")
				GetWidget(button.."_thumb_icon"):SetVisible(true)
				GetWidget(button.."_thumb_icon"):SetColor(".3 .3 .3 1")
				GetWidget(button.."_thumb_icon"):SetTexture("/ui/fe2/elements/vote_up.tga")
				GetWidget(button.."_icon"):UICmd([[SetRendermode('normal')]])
				GetWidget(button.."_text"):SetColor("#1b700c")
				GetWidget(button.."_disabled_panel"):SetVisible(false)
			elseif (state == "no") then
				GetWidget(button.."_thumb_background"):SetColor(mouseover and "#8e2a2a" or "white")
				GetWidget(button.."_thumb_background"):SetBorderColor(mouseover and "#8e2a2a" or "white")
				GetWidget(button.."_thumb_border"):SetColor("red")
				GetWidget(button.."_thumb_border"):SetBorderColor("red")
				GetWidget(button.."_thumb_icon"):SetVisible(true)
				GetWidget(button.."_thumb_icon"):SetColor(".8 .8 .8 1")
				GetWidget(button.."_thumb_icon"):SetTexture("/ui/fe2/elements/vote_down.tga")
				GetWidget(button.."_icon"):UICmd([[SetRendermode('normal')]])
				GetWidget(button.."_text"):SetColor("red")
				GetWidget(button.."_disabled_panel"):SetVisible(false)
			else -- n/a
				GetWidget(button.."_thumb_background"):SetColor(".7 .7 .7 1")
				GetWidget(button.."_thumb_background"):SetBorderColor(".7 .7 .7 1")
				GetWidget(button.."_thumb_border"):SetColor("#0c3405")
				GetWidget(button.."_thumb_border"):SetBorderColor("#0c3405")
				GetWidget(button.."_thumb_icon"):SetVisible(false)
				GetWidget(button.."_icon"):UICmd([[SetRendermode('grayscale')]])
				GetWidget(button.."_text"):SetColor("#888888")
				GetWidget(button.."_disabled_panel"):SetVisible(not mouseover)
				if (state == "n/a_localplayerregion") then
					GetWidget(button.."_disabled_text"):SetText(Translate("mm_option_disabled_localplayerregion"))
				elseif (state == "n/a_groupmateregion") then
					GetWidget(button.."_disabled_text"):SetText(Translate("mm_option_disabled_groupmateregion"))
				else
					GetWidget(button.."_disabled_text"):SetText(Translate("mm_option_disabled"))
				end
			end
			buttonStateShown[button] = (state..(mouseover and "_mo" or ""))
		end
	end

	-- update the main button
	-- queue button
	if (not IsInGroup()) then
		GetWidget("mm_selfleave"):SetVisible(0)

		if (gameModeSelected["lp"] or gameModeSelected["cm"]) then -- lock pick, need 5
			GetWidget("mm_queuebutton_1"):SetText(Translate("mm3_lockpicktoosmall"))
			GetWidget("mm_queuebutton_2"):SetText(Translate("mm3_lockpicktoosmall"))
			GetWidget("mm_queuebutton_3"):SetText(Translate("mm3_lockpicktoosmall"))
			GetWidget("mm_queuebutton_4"):SetText(Translate("mm3_lockpicktoosmall"))
		else
			GetWidget("mm_queuebutton_1"):SetText(Translate("mm3_enterqueuebutton"))
			GetWidget("mm_queuebutton_2"):SetText(Translate("mm3_enterqueuebutton"))
			GetWidget("mm_queuebutton_3"):SetText(Translate("mm3_enterqueuebutton"))
			GetWidget("mm_queuebutton_4"):SetText(Translate("mm3_enterqueuebutton"))
		end
	elseif (IsInGroup() and groupLeaderID == UIGetAccountID()) then
		GetWidget("mm_selfleave"):SetVisible(1)
		if (gameModeSelected["lp"] or gameModeSelected["cm"]) then -- lock pick, need 5
			local hasFive = true
			for i=2,5 do
				if (not HoN_Matchmaking.teamMembers[i] or HoN_Matchmaking.teamMembers[i].isBot) then
					hasFive = false
					break
				end
			end

			if (hasFive) then
				if (AtoB(interface:UICmd("GetTMMOtherPlayersReady()"))) then
					GetWidget("mm_queuebutton_1"):SetText(Translate("mm3_enterqueuebutton"))
					GetWidget("mm_queuebutton_2"):SetText(Translate("mm3_enterqueuebutton"))
					GetWidget("mm_queuebutton_3"):SetText(Translate("mm3_enterqueuebutton"))
					GetWidget("mm_queuebutton_4"):SetText(Translate("mm3_enterqueuebutton"))
				else
					GetWidget("mm_queuebutton_1"):SetText(Translate("mm3_notallreadybutton"))
					GetWidget("mm_queuebutton_2"):SetText(Translate("mm3_notallreadybutton"))
					GetWidget("mm_queuebutton_3"):SetText(Translate("mm3_notallreadybutton"))
					GetWidget("mm_queuebutton_4"):SetText(Translate("mm3_notallreadybutton"))
				end
			else
				GetWidget("mm_queuebutton_1"):SetText(Translate("mm3_lockpicktoosmall"))
				GetWidget("mm_queuebutton_2"):SetText(Translate("mm3_lockpicktoosmall"))
				GetWidget("mm_queuebutton_3"):SetText(Translate("mm3_lockpicktoosmall"))
				GetWidget("mm_queuebutton_4"):SetText(Translate("mm3_lockpicktoosmall"))
			end
		else
			if (AtoB(interface:UICmd("GetTMMOtherPlayersReady()"))) then
				GetWidget("mm_queuebutton_1"):SetText(Translate("mm3_enterqueuebutton"))
				GetWidget("mm_queuebutton_2"):SetText(Translate("mm3_enterqueuebutton"))
				GetWidget("mm_queuebutton_3"):SetText(Translate("mm3_enterqueuebutton"))
				GetWidget("mm_queuebutton_4"):SetText(Translate("mm3_enterqueuebutton"))
			else
				GetWidget("mm_queuebutton_1"):SetText(Translate("mm3_notallreadybutton"))
				GetWidget("mm_queuebutton_2"):SetText(Translate("mm3_notallreadybutton"))
				GetWidget("mm_queuebutton_3"):SetText(Translate("mm3_notallreadybutton"))
				GetWidget("mm_queuebutton_4"):SetText(Translate("mm3_notallreadybutton"))
			end
		end
	else
		GetWidget("mm_selfleave"):SetVisible(1)
		if (HoN_Matchmaking.teamMembers[1] and not HoN_Matchmaking.teamMembers[1].isReady) then
			GetWidget("mm_queuebutton_1"):SetText(Translate("mm3_readybutton"))
			GetWidget("mm_queuebutton_2"):SetText(Translate("mm3_readybutton"))
			GetWidget("mm_queuebutton_3"):SetText(Translate("mm3_readybutton"))
			GetWidget("mm_queuebutton_4"):SetText(Translate("mm3_readybutton"))
		else
			GetWidget("mm_queuebutton_1"):SetText(Translate("mm3_notreadybutton"))
			GetWidget("mm_queuebutton_2"):SetText(Translate("mm3_notreadybutton"))
			GetWidget("mm_queuebutton_3"):SetText(Translate("mm3_notreadybutton"))
			GetWidget("mm_queuebutton_4"):SetText(Translate("mm3_notreadybutton"))
		end
	end

	HoN_Matchmaking:PopulateTeam()

	TMMSettingsDebug('TMM Update Buttons')
end

function HoN_Matchmaking:LeaveGroup()
	interface:UICmd("LeaveTMMGroup();")
end

function HoN_Matchmaking:UpdatePopularityBar(baseName, value)
	for i = 1, value do
		if GetWidget(baseName.."_"..i, nil, true) then
			GetWidget(baseName.."_"..i, nil, true):SetVisible(true)
		end
	end
	for i = value + 1, 10 do
		if GetWidget(baseName.."_"..i, nil, true) then
			GetWidget(baseName.."_"..i, nil, true):SetVisible(false)
		end
	end
end

local function UpdateVerifiedStatus()
	UpdateDefaultRuleset()
	HoN_Matchmaking:UpdateButtons()
end
triggerHelper:RegisterWatch("LoginStatus", UpdateVerifiedStatus)
triggerHelper:RegisterWatch("UIIsVerified", UpdateVerifiedStatus)

local function UpgradesRefreshed()
	HoN_Matchmaking:UpdateButtons()
	HoN_Matchmaking:UpdatePlayerStats()
	HoN_Matchmaking:ScheduleGroupOptionsUpdate()
end
triggerHelper:RegisterWatch("UpgradesRefreshed", UpgradesRefreshed)

local function InfosRefreshed()
	HoN_Matchmaking:UpdatePlayerStats()
end
triggerHelper:RegisterWatch("InfosRefreshed", InfosRefreshed)

local function HostErrorMessage()
	if (not IsInGroup()) and (not IsInQueue()) then
		Trigger('TMMReset')
	end
end
triggerHelper:RegisterWatch("HostErrorMessage", HostErrorMessage)

local function GamePhase(sourceWidget, param0)
	param0 = AtoN(param0)
	if (GetWidget("matchmaking"):IsVisible() and param0 > 0) then
		Set("_mainmenu_currentpanel", "", "string")
		GetWidget("MainMenuPanelSwitcher"):DoEvent()
	end
end
triggerHelper:RegisterWatch("GamePhase", GamePhase)

local function MatchMakerStatus(sourceWidget, param0)
	param0 = tonumber(param0)
	if (param0 >= 6) then
		Set("_mainmenu_currentpanel", "", "string")
		GetWidget("MainMenuPanelSwitcher"):DoEvent()
	end
	if (param0 >= 5) then
		Set("_latch_status", false, "bool")
		GetWidget("toggle_latch"):DoEventN(0)
	end
end
triggerHelper:RegisterWatch("MatchMakerStatus", MatchMakerStatus)

local function TMMAvailable()
	UpdateVerifiedStatus()
	if (not IsTMMEnabled()) then
		if (GetWidget("matchmaking"):IsVisible()) then
			Set("_mainmenu_currentpanel", "", "string")
			GetWidget("MainMenuPanelSwitcher"):DoEvent()
			GetWidget("mm_popup_disabled"):DoEventN(0)
		end
		if (IsInGroup()) then
			interface:UICmd("LeaveTMMGroup()")
		end
	end
end
triggerHelper:RegisterWatch("TMMAvailable", TMMAvailable)

local function TMMReset()
	HoN_Matchmaking:ResetSelection()
	--HideWidget("matchmaking_team_builder")
	HideWidget("matchmaking_queue_display")
	HideWidget("matchmaking_queue_blocker")
	GetWidget("mm_tmm_cover"):DoEventN(0)

	GetWidget("mm_queuebutton_1"):SetText(Translate("mm3_enterqueuebutton"))
	GetWidget("mm_queuebutton_2"):SetText(Translate("mm3_enterqueuebutton"))
	GetWidget("mm_queuebutton_3"):SetText(Translate("mm3_enterqueuebutton"))
	GetWidget("mm_queuebutton_4"):SetText(Translate("mm3_enterqueuebutton"))

	for i=2,5 do
		GetWidget("mm_teamslot"..i.."_text"):SetText(Translate("mm3_coop_emptyslot"))
		GetWidget("mm_teamslot"..i.."_icon_dis"):SetTexture("/ui/fe2/store/icons/account_icons/default.tga")
		GetWidget("mm_teamslot"..i.."_icon_dis"):SetColor('1 1 1 0.7')
	end
	for i=1,5 do
		if (HoN_Matchmaking.teamMembers[i] and not HoN_Matchmaking.teamMembers[i].isBot) then
			HoN_Matchmaking.teamMembers[i] = nil
		end
	end

	if (HoN_Matchmaking.teamHasBots) then
		HoN_Matchmaking:FillTeamWithBots()
	else
		HoN_Matchmaking:KickBots()
	end

	HoN_Matchmaking:PopulateTeam()

	--HideWidget("matchmaking_friends_list")
	HideWidget("mm_popup_init")
	HideWidget("mm_loading_1")
	HideWidget('mapLoadGuide_mm_loading_1')
	HideWidget("mm_popup_joiningqueue")
	HideWidget("mm_popup_serverfound")
	HideWidget("mm_popup_foundmatch")
	HideWidget("mm_popup_joinmatch")
	HideWidget("mm_popup_buytoken")
end
triggerHelper:RegisterWatch("TMMReset", TMMReset)

local function TMMOptionsAvailable(sourceWidget, avGameType, avMap, avGameMode, avRegion)
	availableGameType, availableMap, availableGameMode, availableRegion = avGameType, avMap, avGameMode, avRegion

	availableGameMode = GameModeTrick(TMM_GAME_TYPE_NORMAL, availableGameMode)
	
	if (GetCvarBool("ui_debugTMM")) then
		println("^y^:TMMOptionsAvailable")
		println("\tMaps: "..availableMap)
		println("\tRegions: "..availableRegion)
		println("\tGameTypes: "..availableGameType)
		println("\tGameModes: "..availableGameMode)
	end
	if GetCvarBool('ui_enabledAllTMMModes') then
		availableGameMode = 'ap|sd|bp|bd|ar|lp|bb|gt|apg|bbg|apd|bbr|bdr|cp|fp|rb'
	end

	if GetCvarBool('ui_enabledAllTMMRegions') then
		availableRegion = '|usw|use|eu|au|sg|my|ph|th|id|vn|ru|kr|lat|br|dx|lt'
	end

	if (not IsInGroup() and not IsInQueue()) then
		Trigger('TMMReset')
	else
		HoN_Matchmaking:CheckSelection()
	end
end
triggerHelper:RegisterWatch("TMMOptionsAvailable", TMMOptionsAvailable)

local function TMMDisplay(sourceWidget, ...)
--[[ params
		-- for player slots 1 - 5
		0 = Slot Account ID
		1 = Slot Username
		2 = Slot number
		3 = Slot TMR
		4 = Player Loading TMM Status | Player Ready Status
		--
		25 = Update type
		26 = group size
		27 = average TMR
		28 = leader account id
		29 = game type
		30 = MapNames
		31 = GameModes
		32 = Regions
		33 = TSNULL
		34 = PlayerInvitationResponses
		35 = TeamSize
		36 = TSNULL
		37 = Verified
		38 = VerifiedOnly
		39 = BotDifficulty
		40 = RandomizeBots
		41 = GroupType
]]
--~ 	TriggerPrint("TMMDisplay", sourceWidget, ...)
	local serverGroupSize, _, serverGroupLeaderID, serverGameType, serverMap, serverGameModes, serverRegions = select(27, ...)
	local serverVerifiedOnly, serverBotDifficulty, serverRandomizeBots, serverGroupType = select(39, ...)
	serverGroupSize, serverGameType, serverVerifiedOnly = AtoN(serverGroupSize), AtoN(serverGameType), AtoB(serverVerifiedOnly)
	serverBotDifficulty, serverRandomizeBots = tonumber(serverBotDifficulty), AtoB(serverRandomizeBots)
	local accountID = UIGetAccountID()

	HideWidget("mm_popup_init")

	groupSize = serverGroupSize
	groupLeaderID = serverGroupLeaderID
	verifiedOnly = serverVerifiedOnly
	rulesetMode = defaultRulesetMode
	viewMode = defaultRulesetMode
	mapSelected = serverMap
	gameTypeSelected = serverGameType
	botDifficulty = serverBotDifficulty
	Set("g_botDifficulty", serverBotDifficulty, "int")
	randomEnemyBots = serverRandomizeBots
	local lockPickSelected, gatedSelected, standardSelected = false, false, false

	local gType = tonumber(serverGroupType)
	-- special thingy for teammates
	if (IsInGroup() and (groupLeaderID ~=  UIGetAccountID())) then
		HoN_Matchmaking.selectedTab = ""
	end

	if (HoN_Matchmaking.selectedTab ~= "coop" and gType == 3) then
		-- switch to Coop tab
		HoN_Matchmaking:SelectTab("coop", false, true)
	elseif (HoN_Matchmaking.selectedTab ~= "pvp" and HoN_Matchmaking.selectedTab ~='season' and gType == 2) then
		-- switch to pvp tab
		if HoN_Matchmaking.selectedTab ~= "season" and (gameTypeSelected == 6 or gameTypeSelected == 7) then
			HoN_Matchmaking:SelectTab("season", false, true)
		elseif HoN_Matchmaking.selectedTab ~= "pvp" and gameTypeSelected ~= 6 and gameTypeSelected ~= 7 then
			HoN_Matchmaking:SelectTab("pvp", false, true)
		end
	end

	for _, v in pairs(GAMEMODES) do
		gameModeSelected[v] = false
	end
	for _, v in ipairs(explode("|", serverGameModes)) do
		if (v and NotEmpty(v)) then
			v = string.lower(v)
			gameModeSelected[v] = true
			if (v == "lp") then --TOURNAMENT_LP
				lockPickSelected = true
			elseif ((v == "gt") or (v == "apg") or (v == "bbg")) then --GATED MODE
				gatedSelected = true
			else
				standardSelected = true
			end
		end
	end

	if (gatedSelected) and (not standardSelected) and (not lockPickSelected) then
		--rulesetMode = 2 -- Disabled to not overwrite custom
	elseif (lockPickSelected) then --(standardSelected) or
		rulesetMode = 0
		viewMode = 0
	end

	for _, v in pairs(REGIONS) do
		regionSelected[v] = false
	end
	for _, v in ipairs(explode("|", serverRegions)) do
		if (v and NotEmpty(v)) then
			v = string.lower(v)
			regionSelected[v] = true
		end
	end

	if (not IsInQueue() and not IsInGroup()) then
		soloQueue = false
	end
	if (soloQueue and soloQueueSendReady) then
		soloQueueSendReady = false
		local gameTypeSelectedOverride
		gameTypeSelectedOverride = MidwarsShennanigans(mapSelected, gameTypeSelected)
		interface:UICmd("SendTMMPlayerReadyStatus(1,"..gameTypeSelectedOverride..");")
	end

	HoN_Matchmaking:UpdateButtons()

	if (not GetWidget("mm_loading_progress"):IsVisible()) then
		local coverWidget = GetWidget("mm_tmm_cover")
		if (not IsInGroup()) then
			if (coverWidget:IsVisible()) then
				coverWidget:DoEventN(0)
			end
		else
			if (accountID ~= groupLeaderID) then
				if (not coverWidget:IsVisible()) then
					coverWidget:DoEventN(1)
				end
			elseif (coverWidget:IsVisible()) then
				coverWidget:DoEventN(0)
			end
		end
	end

	if groupSize > MAP_INFO[mapSelected].SIZE then 
		HoN_Matchmaking:LeaveGroup()
	end
	-- GetWidget("matchmaking_button_group_disband"):SetVisible(IsInGroup() and not IsInQueue() and accountID == groupLeaderID)
	-- GetWidget("matchmaking_button_group_leave"):SetVisible(IsInGroup() and not IsInQueue() and accountID ~= groupLeaderID)

	if (IsInGroup() or IsInQueue()) then
		--if (not GetWidget("matchmaking_team_builder"):IsVisible() and not soloQueue) then
			-- ShowWidget("matchmaking_team_builder")
			-- ShowWidget("matchmaking_friends_list")
		--end
	else
		-- HideWidget("matchmaking_team_builder")
		-- HideWidget("matchmaking_friends_list")
	end

	TMMSettingsDebug('TMMDisplay')
end
triggerHelper:RegisterWatch("TMMDisplay", TMMDisplay)

local function TMMPlayerStatus(sourceWidget, playerID, ...)
--[[ params
		0 = uiAccountID
		1 = sName
		2 = ySlot
		3 = nRating
		4 = yLoadingPercent
		5 = yReadyStatus
		6 = isLeader
		7 = isValidIndex
		8 = bVerified
		9 = bFriend
		10 = uiChatNameColorString
		11 = GetChatNameColorTexturePath(uiChatNameColorString)
		12 = bGameModeAccess
		13 = GetChatNameGlow(uiChatNameColor)
		14 = sGameModeAccess
		15 = Ingame
		16 = GetChatNameGlowColorString
		17 = GetChatNameGlowColorIngameString
		18 = Normal RankLevel
		19 = Casual RankLevel
		20 = Normal Ranking
		21 = Casual Ranking
		22 = bEligibleForCampaign
]]
	if (playerID == 0) then
		HoN_Matchmaking.teamOffset = 2
	end

	local playerAccountID, name, slot, mmr, _, isReady, isLeader, isValidSlot, isVerified, isFriend, nameColor, _, isModeCompat, nameGlow, modeAccess, inGame, nameGlowColorIngame, rankLevel_normal, rankLevel_casual, ranking_normal, ranking_casual, eligible_for_campaign = ...
	local accountID = UIGetAccountID()
	slot, mmr, isReady, isLeader, isValidSlot, isVerified, isFriend, isModeCompat, inGame, rankLevel_normal, rankLevel_casual, ranking_normal, ranking_casual, eligible_for_campaign = AtoN(slot), AtoN(mmr), AtoB(isReady), AtoB(isLeader), AtoB(isValidSlot), AtoB(isVerified), AtoB(isFriend), AtoB(isModeCompat), AtoB(inGame), AtoN(rankLevel_normal), AtoN(rankLevel_casual), AtoN(ranking_normal), AtoN(ranking_casual), AtoB(eligible_for_campaign)

	if tonumber(mmr) and (tonumber(mmr) > 0) and (tonumber(mmr) < 3000) then
		playerMMR[playerID] = mmr
	else
		playerMMR[playerID] = nil
	end
	playerName[playerID] = NotEmpty(playerAccountID) and name or nil
	playerVerified[playerID] = isVerified

	if (playerAccountID == accountID) then
		if (not isVerified and (not isModeCompat or verifiedOnly) or inGame) then
		--	GetWidget("matchmaking_button_self_ready"):SetEnabled(false)
		else
		--	GetWidget("matchmaking_button_self_ready"):SetEnabled(true)
		end
	end

	if (playerAccountID ~= "" and playerAccountID ~= accountID) then -- not ourself and a good slot
		-- if (HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset] and HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].isBot) then
		if (HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset] and HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].isBot) then
			HoN_Matchmaking:KickTeamMember(nil, HoN_Matchmaking.teamOffset) end
		-- 	-- move from the bottom of the list up, pushing bots down a slot
		-- 	HoN_Matchmaking:KickTeamMember(nil, 5) -- kick the bottom bot
		-- 	if (HoN_Matchmaking.teamOffset ~= 5) then
		-- 		SetTeamBot(HoN_Matchmaking.teamOffset-1, "") -- clear out the bot in the current slot
		-- 	end
		-- 	SetTeamBot(4, "") -- empty
		-- 	if (HoN_Matchmaking.teamOffset ~= 5) then
		-- 		for i=4, HoN_Matchmaking.teamOffset, -1 do
		-- 			if (HoN_Matchmaking.teamMembers[i]) then
		-- 				SetTeamBot(i, HoN_Matchmaking.botsTable[HoN_Matchmaking.teamMembers[i].botID].sName)
		-- 				HoN_Matchmaking.teamMembers[i+1] = HoN_Matchmaking.teamMembers[i]
		-- 			end
		-- 		end
		-- 	end
		-- end

		-- if HoN_Matchmaking.selectedTab == 'season' then
			-- mmr = nil
		-- end

		-- setup the player info
		HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset] = {}
		HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].name = name
		HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].accountID = tonumber(playerAccountID)
		HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].isLeader = isLeader
		HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].verified = isVerified
		HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].slotNumber = slot
		HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].isReady = isReady
		HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].isGood = not (not isModeCompat or ((not isVerified) and verifiedOnly) or inGame)
		HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].inGame = inGame
		HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].mmr = mmr
		HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].rankLevel_normal = rankLevel_normal
		HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].rankLevel_casual = rankLevel_casual
		HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].ranking_normal = ranking_normal
		HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].ranking_casual = ranking_casual
		

		if (verifiedOnly and not isVerified) then
			HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].status = 1
		elseif (not isModeCompat) then
			HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].status = 3
		elseif (inGame) then
			HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].status = 7
		elseif (isReady) then
			HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].status = 4
		else
			HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].status = 2
		end

		local chatIcon = nil
		if (GetChatClientInfo) then
			chatIcon = GetChatClientInfo(StripClanTag(HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].name), "getaccounticontexturepath")
		end

		if (chatIcon and (string.sub(chatIcon, 0, -2) ~= "")) then
			HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].accountIcon = string.sub(chatIcon, 0, -2)
		else
			HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].accountIcon = "/ui/fe2/store/icons/account_icons/default.tga"
		end

		HoN_Matchmaking.teamOffset = HoN_Matchmaking.teamOffset + 1

	elseif(playerAccountID == "") then 	-- empty slot
		if (HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset] and not HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset].isBot) then
			--filled and doesn't have a bot, if the next slot has a bot we need to shift bots up
			HoN_Matchmaking.teamMembers[HoN_Matchmaking.teamOffset] = nil
			-- if (HoN_Matchmaking.teamOffset ~= 5) then
			-- 	for i=HoN_Matchmaking.teamOffset,4 do
			-- 		if (HoN_Matchmaking.teamMembers[i+1]) then
			-- 			SetTeamBot(i-1, HoN_Matchmaking.botsTable[HoN_Matchmaking.teamMembers[i+1].botID].sName)
			-- 			HoN_Matchmaking.teamMembers[i] = HoN_Matchmaking.teamMembers[i+1]
			-- 			if (i == 4) then HoN_Matchmaking.teamMembers[5] = nil end
			-- 		else
			-- 			SetTeamBot(i-1, "")
			-- 			HoN_Matchmaking.teamMembers[i] = nil
			-- 		end
			-- 	end
		 -- 	else
		 -- 		HoN_Matchmaking.teamMembers[5] = nil
		 -- 	end

		 	-- fill any empty slots
		 	if (HoN_Matchmaking.teamHasBots) then
		 		HoN_Matchmaking:FillTeamWithBots()
		 	end
		end

		if (not isValidSlot) then
			GetWidget("mm_teamslot"..HoN_Matchmaking.teamOffset.."_text"):SetText(Translate("mm3_coop_lockedslot"))
			GetWidget("mm_teamslot"..HoN_Matchmaking.teamOffset.."_icon_dis"):SetTexture("/ui/elements/kick_disabled.tga")
			GetWidget("mm_teamslot"..HoN_Matchmaking.teamOffset.."_icon_dis"):SetColor('1 1 1 1')
		else
			GetWidget("mm_teamslot"..HoN_Matchmaking.teamOffset.."_text"):SetText(Translate("mm3_coop_emptyslot"))
			GetWidget("mm_teamslot"..HoN_Matchmaking.teamOffset.."_icon_dis"):SetTexture("/ui/fe2/store/icons/account_icons/default.tga")
			GetWidget("mm_teamslot"..HoN_Matchmaking.teamOffset.."_icon_dis"):SetColor('1 1 1 0.7')
		end
	else 	-- ourself
		HoN_Matchmaking.teamMembers[1] = {}
		HoN_Matchmaking.teamMembers[1].isReady = isReady
		HoN_Matchmaking.teamMembers[1].isGood = not (not isModeCompat or ((not isVerified) and verifiedOnly) or inGame)
		HoN_Matchmaking.teamMembers[1].inGame = inGame

		if (verifiedOnly and not isVerified) then
			HoN_Matchmaking.teamMembers[1].status = 1
		elseif (not isModeCompat) then
			HoN_Matchmaking.teamMembers[1].status = 3
		elseif (inGame) then
			HoN_Matchmaking.teamMembers[1].status = 7
		elseif (isReady) then
			HoN_Matchmaking.teamMembers[1].status = 4
		else
			HoN_Matchmaking.teamMembers[1].status = 2
		end
	end


	local index = 0

	if (playerAccountID ~= "") then
		for _, v in ipairs(explode("|", modeAccess)) do
			if (v and NotEmpty(v)) then
				local access = AtoB(v)
				local modeName = gameModeIndexToName[index]
				if (not access) then
					gameModeCompatible[v] = false
				end
				index = index + 1;
			end
		end
	end
	
	if (groupLeaderID == accountID and NotEmpty(playerAccountID) and playerAccountID ~= accountID and not eligible_for_campaign) then
		Trigger(
			"WarnNotEligibleForCampaignTrigger",
			name,
			"KickFromTMMGroup('"..playerID.."');"
		)

		rulesetMode = defaultRulesetMode
		viewMode = defaultRulesetMode
		tournamentModeAllowed = false
		HoN_Matchmaking:CheckSelection()
		HoN_Matchmaking:ScheduleGroupOptionsUpdate()
	end

	HoN_Matchmaking:UpdateButtons()
end
triggerHelper:RegisterWatch("TMMPlayerStatus0", function(sourceWidget, ...) TMMPlayerStatus(sourceWidget, 0, ...) end)
triggerHelper:RegisterWatch("TMMPlayerStatus1", function(sourceWidget, ...) TMMPlayerStatus(sourceWidget, 1, ...) end)
triggerHelper:RegisterWatch("TMMPlayerStatus2", function(sourceWidget, ...) TMMPlayerStatus(sourceWidget, 2, ...) end)
triggerHelper:RegisterWatch("TMMPlayerStatus3", function(sourceWidget, ...) TMMPlayerStatus(sourceWidget, 3, ...) end)
triggerHelper:RegisterWatch("TMMPlayerStatus4", function(sourceWidget, ...) TMMPlayerStatus(sourceWidget, 4, ...) end)

-- local function TMMReadyStatus(sourceWidget, isLeader, othersReady, allReady, selfReady, queued)
-- --[[ params
-- 	0 = isLeader
-- 	1 = m_bTMMOtherPlayersReady
-- 	2 = m_bTMMAllPlayersReady
-- 	3 = m_aGroupInfo[m_uiTMMSelfGroupIndex].yReadyStatus > 0
-- 	4 = m_uiTMMStartTime != INVALID_TIME
-- ]]
-- 	isLeader, othersReady, allReady, selfReady, queued = AtoB(isLeader), AtoB(othersReady), AtoB(allReady), AtoB(selfReady), AtoB(queued)
-- 	if (queued and not IsInQueue() and IsInGroup()) then
-- 		--GetWidget("mm_popup_joiningqueue"):DoEventN(0)
-- 	end

-- 	-- GetWidget("matchmaking_button_self_ready"):SetVisible(not isLeader and not selfReady)
-- 	-- GetWidget("matchmaking_button_self_unready"):SetVisible(not isLeader and selfReady)

-- 	if (othersReady) then
-- 		--GetWidget("matchmaking_button_self_unready_label"):SetText(Translate("mm_waiting_forhost"))
-- 	else
-- 		--GetWidget("matchmaking_button_self_unready_label"):SetText(Translate("mm_waiting_forgroup"))
-- 	end

-- 	-- GetWidget("matchmaking_button_host_waiting"):SetVisible(isLeader and not othersReady)
-- 	-- GetWidget("matchmaking_button_host_start"):SetVisible(isLeader and othersReady)
-- 	if (isLeader) then
-- 		-- GetWidget("matchmaking_button_group_not_full_label_1"):SetText(Translate('mm_arehost'))
-- 	else
-- 		-- GetWidget("matchmaking_button_group_not_full_label_1"):SetText(Translate('mm_waiting_forhost'))
-- 	end
-- end
-- triggerHelper:RegisterWatch("TMMReadyStatus", TMMReadyStatus)

--~ local function TMMFoundMatch()
--~ end
--~ triggerHelper:RegisterWatch("TMMFoundMatch", TMMFoundMatch)

local function TMMFoundServer()
	HideWidget("mm_popup_foundmatch")
	PlaySound("/shared/sounds/ui/menu/match_found.wav")
	if (GetWidget("matchmaking"):IsVisible()) then
		Set("_mainmenu_currentpanel", "")
		GetWidget("MainMenuPanelSwitcher"):DoEvent()
	end
	GetWidget("mm_popup_serverfound"):DoEventN(0)
end
triggerHelper:RegisterWatch("TMMFoundServer", TMMFoundServer)

local function TMMJoinMatch()
	GetWidget("mm_popup_serverfound"):SetVisible(false)
	GetWidget("mm_popup_joinmatch"):DoEventN(0)
end
triggerHelper:RegisterWatch("TMMJoinMatch", TMMJoinMatch)

local function TMMLeftQueue()
	GetWidget("matchmaking_queue_display"):SetVisible(false)
	GetWidget("matchmaking_queue_blocker"):SetVisible(false)
	if (not IsInGame()) then
		--StopMusic()
	end
end
triggerHelper:RegisterWatch("TMMLeftQueue", TMMLeftQueue)

local function TMMNoMatchesFound()
	GetWidget("mm_popup_nomatchesfound"):DoEventN(0)
	TMMLeftQueue()
end
triggerHelper:RegisterWatch("TMMNoMatchesFound", TMMNoMatchesFound)

local function TMMNoServersFound()
	GetWidget("mm_popup_noserversfound"):DoEventN(0)
	TMMLeftQueue()
end
triggerHelper:RegisterWatch("TMMNoServersFound", TMMNoServersFound)

local function TMMLeaveGroup(sourceWidget, reason, param1)
	-- suppress this diag when they are kicked from the group to make a local match
	if (HoN_Matchmaking.suppressDCDiag and reason == "disbanded") then
		HoN_Matchmaking.suppressDCDiag = false
		return
	end

	if (reason == "isleaver") then
		HoN_Matchmaking:UpdateLeaverBlock()
	elseif (reason == "disabled") then
		GetWidget("mm_popup_tmmdisabled"):DoEventN(0)
	elseif (reason == "invalidversion") then
		GetWidget("mm_popup_invalid_version"):DoEventN(0)
	elseif (reason == "groupfull") then
		GetWidget("mm_popup_group_full"):DoEventN(0)
	elseif (reason == "busy") then
		GetWidget("mm_popup_tmmbusy"):DoEventN(0)
	elseif (reason == "optionunavailable") then
		GetWidget("mm_popup_optionsunavailable"):DoEventN(0)
	elseif (reason == "disbanded" and not soloQueue) then
		GetWidget("mm_popup_disbanded"):DoEventN(0)
	elseif (reason == "kicked") then
		GetWidget("mm_popup_kicked"):DoEventN(0)
	elseif (reason == "disconnected" and AtoB(param1)) then
		GetWidget("mm_popup_disconnected"):DoEventN(0)
	elseif (reason == "groupqueued") then
		GetWidget("mm_popup_groupqueued"):DoEventN(0)
	elseif (reason == "banned") then
		GetWidget("mm_popup_banned"):DoEventN(0)
	elseif (reason == "unknown") then
		GetWidget("mm_popup_unknown"):DoEventN(0)
	elseif (reason == "foundmatch") then
		HideWidget("mm_popup_serverfound")
	elseif (reason == "servernotidle") then
		GetWidget("mm_popup_servernotidle"):DoEventN(0)
	elseif (reason == 'noteligible') then
		GetWidget("mm_popup_noteligible"):DoEventN(0)
	end

	if (reason == "optionunavailable") then
		interface:UICmd("RequestTMMPopularityUpdate()")
	else
		Trigger("TMMReset")
	end

	Set('_TMM_GroupTeamSize', 1, 'int')
	groupSize = 1
	Set('_TMM_tournamentSettings', 'false', 'string')
	rulesetMode = defaultRulesetMode
	viewMode = defaultRulesetMode
	HoN_Matchmaking:PopulateTeam()
end
triggerHelper:RegisterWatch("TMMLeaveGroup", TMMLeaveGroup)

local function AccountInfo(sourceWidget, ...)
--[[
 1 - 39 Games		3 Leaves
40 - 79 Games		4 Leaves
79 - 99 Games		5 Leaves
100+ Games			5% Leaves
]]
	if (GetAccountName() == "UnnamedNewbie") then return end

	local gamesPlayed = tonumber((select(5, ...))) or 0
	local disconnects = tonumber((select(6, ...))) or 0
	local leaverPercentage = (gamesPlayed > 0) and (disconnects / gamesPlayed) or 0
	local gamesToClearLeaver = 0
	if (disconnects <= 4) then -- leaver with <= 4 dcs indicates < 40 games
		gamesToClearLeaver = 40
	elseif (disconnects == 5) then -- leaver with 5 dcs indicates < 79 games
		gamesToClearLeaver = 79
	else -- disconnects > 5
		local leaverThreshold = tonumber(interface:UICmd("GetLeaverThreshold("..(gamesPlayed >= 100 and gamesPlayed or 100)..")")) + 0.001
		gamesToClearLeaver = disconnects / leaverThreshold
	end
	local gamesToClearLeaverLeft = math.ceil(gamesToClearLeaver - gamesPlayed)
	if (gamesToClearLeaverLeft < 0) then
		gamesToClearLeaverLeft = 0
	end
	GetWidget("matchmaking_leaver_label"):SetText(Translate("mm_isleaver_onopen_body", "leaverpercent", string.format("%.2f%%", leaverPercentage * 100), 'matchestoclear', gamesToClearLeaverLeft, 'gamesplayed', gamesPlayed) )
	HoN_Matchmaking:UpdateLeaverBlock()
	HoN_Matchmaking:PopulateSelfSlot()
end
triggerHelper:RegisterWatch("AccountInfo", AccountInfo)

-- -----
-- frame event handler
-- -----
function HoN_Matchmaking.OnCampaignRewardPopupFrame()
	if HoN_Matchmaking.popupShow then
	    local value = (GetTime() - HoN_Matchmaking.popupTimer) / 200
	    if (value > 1) then
	    	value = 1
	    end

	    GetWidget('campaign_reward_popup_levelup_anim'):SetWidth(tostring(96*value)..'h')
	    GetWidget('campaign_reward_popup_levelup_anim'):SetHeight(tostring(96*value)..'h')
	else
		local value = (GetTime() - HoN_Matchmaking.popupTimer) / 200 + 1.0
		GetWidget('campaign_reward_popup_levelup_anim'):SetWidth(tostring(96*value)..'h')
		GetWidget('campaign_reward_popup_levelup_anim'):SetHeight(tostring(96*value)..'h')

		if (value > 1.8) then
			GetWidget('campaign_reward_popup'):SetVisible(false)
			GetWidget('campaign_reward_popup_levelup_anim'):SetWidth('96h')
			GetWidget('campaign_reward_popup_levelup_anim'):SetHeight('96h')
		end
	end 
end

function HoN_Matchmaking:matchmaking_OnShow()
	--UpdateDisplayedTab()
	HoN_Matchmaking:UpdateLeaverBlock()

	if(Main.walkthroughPrompted == 0) and (not GetWidget("matchmaking_blocker_leaver"):IsVisible()) then
		Main.walkthroughPrompted = GetDBEntry('walkthroughPrompted', 1, true, false, true)
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'main_splash_new_player', nil, nil, nil, nil, 63)
	end

	UpdateVerifiedStatus()
	HoN_Matchmaking:PopulateEnemyTeam()
	HoN_Matchmaking:PopulateTeam()
	HoN_Matchmaking:PopulateBotPickList("left")
	HoN_Matchmaking:PopulateBotPickList("right")
end

local function GetSeasonButtonGameType(id)
	if id == 'normal' then return TMM_GAME_TYPE_CAMPAIGN_NORMAL end
	if id == 'casual' then return TMM_GAME_TYPE_CAMPAIGN_CASUAL end
end

function HoN_Matchmaking:season_gm_allpick_button_OnMouseOver(id)
	local button = "season_gm_allpick_button_"..id

	if mouseoverButton ~= button then
		mouseoverButton = button
		PlaySound("/shared/sounds/ui/ccpanel/button_over_01.wav")
	end

	HoN_Matchmaking:RefreshButtons()
end

function HoN_Matchmaking:season_gm_allpick_button_OnMouseOut(id)
	if (mouseoverButton == "season_gm_allpick_button_"..id) then
		mouseoverButton = nil
	end
	HoN_Matchmaking:RefreshButtons()
end

function HoN_Matchmaking:season_gm_allpick_button_OnClick(id)
	PlaySound('/shared/sounds/ui/button_click_06.wav')
	mapSelected = "caldavar"
	matchmakingOptions.map = tostring(mapSelected)

	local newGameType = GetSeasonButtonGameType(id)

	if newGameType ~= gameTypeSelected then
		gameTypeSelected = tonumber(newGameType)
		matchmakingOptions.gameType = tostring(gameTypeSelected)

		FlushMatchmakingOptions()
		HoN_Matchmaking:CheckSelection()
		HoN_Matchmaking:ScheduleGroupOptionsUpdate()
	end
end

function HoN_Matchmaking:matchmaking_option_map_OnMouseOver(id)
	local button = "matchmaking_option_map_"..id
	mouseoverButton = button
	if (buttonState[button] == "no") then
		PlaySound("/shared/sounds/ui/ccpanel/button_over_01.wav")
		HoN_Matchmaking:RefreshButtons()
	end
end

function HoN_Matchmaking:matchmaking_option_map_OnMouseOut(id)
	if (mouseoverButton == "matchmaking_option_map_"..id) then
		mouseoverButton = nil
	end
	HoN_Matchmaking:RefreshButtons()
end

function HoN_Matchmaking:matchmaking_option_map_OnClick(id, modifier)
	if (buttonState["matchmaking_option_map_"..id] == "no") then
		PlaySound('/shared/sounds/ui/button_click_06.wav')
		mapSelected = id
		matchmakingOptions.map = tostring(mapSelected)
		FlushMatchmakingOptions()

		forceModeButtonsRefresh = true
		HoN_Matchmaking:CheckSelection()
		HoN_Matchmaking:ScheduleGroupOptionsUpdate()

		-- 1v1 map should hide invite panel
		if mapSelected == 'solomap' then
			GetWidget('mm_invite_person'):FadeOut(200)
		end
	end
end

function HoN_Matchmaking:matchmaking_option_gametype_OnMouseOver(id)
	local button = "matchmaking_option_gametype_"..id
	mouseoverButton = button
	if (buttonState[button] == "no") then
		PlaySound("/shared/sounds/ui/ccpanel/button_over_01.wav")
		HoN_Matchmaking:RefreshButtons()
	end
end

function HoN_Matchmaking:matchmaking_option_gametype_OnMouseOut(id)
	if (mouseoverButton == "matchmaking_option_gametype_"..id) then
		mouseoverButton = nil
	end
	HoN_Matchmaking:RefreshButtons()
end

function HoN_Matchmaking:matchmaking_option_gametype_OnClick(id)
	if (buttonState["matchmaking_option_gametype_"..id] == "no") then
		PlaySound('/shared/sounds/ui/button_click_06.wav')
		gameTypeSelected = tonumber(id)
		matchmakingOptions.gameType = tostring(gameTypeSelected)
		FlushMatchmakingOptions()
		HoN_Matchmaking:CheckSelection()
		HoN_Matchmaking:ScheduleGroupOptionsUpdate()
	end
end

function HoN_Matchmaking:matchmaking_option_gamemode_OnMouseOver(id)
	local button = "matchmaking_option_gamemode_"..id
	local state = buttonState[button]
	local info = GetGameModeInfo(id)
	mouseoverButton = button

	local na_desc = ""
	if (state == "n/a") then
		na_desc = "mm_option_disabled_body"
	elseif (state == "n/a_betaregion") then
		na_desc = "mm_option_disabled_betaregion_body"
	elseif (state == "n/a_groupsize") then
		na_desc = "mm_option_disabled_groupsize_body"
	elseif (state == "n/a_verified") then
		local hasPass = HasGamePass(info.MODE)
		local hasToken = GameTokens() >= 0
		if (hasPass) then
			if (hasToken) then
				na_desc = "mm_option_disabled_tokenpass_body"
			else
				na_desc = "mm_option_disabled_havepass_body"
			end
		elseif (hasToken) then
			na_desc = "mm_option_disabled_havetoken_body"
		else
			na_desc = "mm_option_disabled_unverified_body"
		end
	elseif (state == "n/a_basicplayer") then
		na_desc = "mm_mode_disabled_basicplayer_desc" --TODO: new string?
	elseif (state == "n/a_needtokenpass") then
		na_desc = "mm_option_disabled_needtokenpass_body"
	elseif (state == "n/a_needtoken") then
		na_desc = "mm_option_disabled_needtoken_body"
	end
	Trigger(
		"TMMUpdateTooltipTrigger",
		info.DESC,
		info.DESC.."_desc",
		na_desc,
		"",
		"/ui/icons/"..info.ICON..".tga"
	)

	if (state == "yes" or state == "no" or state == "n/a_needtoken" or state == "n/a_needtokenpass") then
		PlaySound("/shared/sounds/ui/ccpanel/button_over_01.wav")
		HoN_Matchmaking:RefreshButtons()
	end
end

function HoN_Matchmaking:matchmaking_option_gamemode_OnMouseOut(id)
	if (mouseoverButton == "matchmaking_option_gamemode_"..id) then
		Trigger("TMMUpdateTooltipTrigger", "", "", "", "", "")
		mouseoverButton = nil
	end
	HoN_Matchmaking:RefreshButtons()
end

function HoN_Matchmaking:matchmaking_option_gamemode_OnClick(id)
	local state = buttonState["matchmaking_option_gamemode_"..id]
	if (state == "yes" or state == "no") then
		PlaySound('/shared/sounds/ui/button_click_06.wav')
		gameModeSelected[id] = not (state == "yes")
		matchmakingOptions["gameMode"..id] = tostring(gameModeSelected[id])
		FlushMatchmakingOptions()
		HoN_Matchmaking:CheckSelection()
		HoN_Matchmaking:ScheduleGroupOptionsUpdate()
	elseif (state == "n/a_needtoken" or state == "n/a_needtokenpass") then
		GetWidget("mm_popup_buytoken"):DoEventN(0)
	end
end

function HoN_Matchmaking:matchmaking_option_region_OnMouseOver(id)
	local button = "matchmaking_option_region_"..id
	local state = buttonState[button]
	mouseoverButton = button

	if (state == "yes" or state == "no") then
		PlaySound("/shared/sounds/ui/ccpanel/button_over_01.wav")
		HoN_Matchmaking:RefreshButtons()
	elseif (state == "n/a_localplayerregion") then
		Trigger(
			'TMMUpdateTooltipTrigger',
			'mm_option_disabled_localplayerregion', 'mm_option_disabled_localplayerregion_body', '', '', ''
		)
	elseif (state == "n/a_groupmateregion") then
		local invalidRegionPlayerNames = GetInvalidRegionPlayerNames(id)
		Trigger(
			'TMMUpdateTooltipTrigger',
			'mm_option_disabled_groupmateregion', 'mm_option_disabled_groupmateregion_body', invalidRegionPlayerNames, '', ''
		)
	end
end

function HoN_Matchmaking:matchmaking_option_region_OnMouseOut(id)
	if (mouseoverButton == "matchmaking_option_region_"..id) then
		Trigger("TMMUpdateTooltipTrigger", "", "", "", "", "");
		mouseoverButton = nil
	end
	HoN_Matchmaking:RefreshButtons()
end

function HoN_Matchmaking:matchmaking_option_region_OnClick(id)
	local state = buttonState["matchmaking_option_region_"..id]
	if (state == "yes" or state == "no") then
		PlaySound('/shared/sounds/ui/button_click_06.wav')
		regionSelected[id] = not (state == "yes")
		matchmakingOptions["region"..id] = tostring(regionSelected[id])
		FlushMatchmakingOptions()
		HoN_Matchmaking:CheckSelection()
		HoN_Matchmaking:ScheduleGroupOptionsUpdate()
	end
end

function HoN_Matchmaking:matchmaking_button_leave_queue_OnClick()
	HideWidget("matchmaking_queue_blocker")
	HideWidget("matchmaking_queue_display")
	--StopMusic()
	GetWidget("mm_tmm_cover"):DoEventN(0)
	HideWidget("mm_loading_1")
	HideWidget('mapLoadGuide_mm_loading_1')
	if (soloQueue or UIGetAccountID() ~= groupLeaderID) then
		interface:UICmd("LeaveTMMGroup()")
	else
		interface:UICmd("LeaveTMMQueue()")
	end
	Set('_TMM_GroupTeamSize', 1, "int")
	groupSize = 1
end

function HoN_Matchmaking:HoverEnemyTeamSlot(widget, slot)
	slot = tonumber(slot)

	if (not HoN_Matchmaking.lastHeroInfoLookUp.isValid or (HoN_Matchmaking.lastHeroInfoLookUp.hero ~= HoN_Matchmaking.botsTable[HoN_Matchmaking.bots[slot]].sHeroName)) then
		HoN_Matchmaking.lastHeroInfoLookUp.hero = HoN_Matchmaking.botsTable[HoN_Matchmaking.bots[slot]].sHeroName
		interface:UICmd("GetHeroInfo('"..HoN_Matchmaking.botsTable[HoN_Matchmaking.bots[slot]].sHeroName.."');")
	end
	GetWidget("hero_info"):SetVisible(1)
end

function HoN_Matchmaking:HoverTeamSlot(widget, slot)
	if (not IsInGroup()) then
		return end

	slot = tonumber(slot)
	local statusVal = nil
	local name = ""

	if (slot == 1) then -- self
		name = StripClanTag(GetAccountName())

		if (UIGetAccountID() == groupLeaderID) then
			statusVal = 6
		else
			statusVal = HoN_Matchmaking.teamMembers[1].status
		end
	else
		if (HoN_Matchmaking.teamMembers[slot]) then
			if (not HoN_Matchmaking.teamMembers[slot].isBot) then
				if (HoN_Matchmaking.teamMembers[slot].isLeader) then
					statusVal = 6
				else
					statusVal = HoN_Matchmaking.teamMembers[slot].status
				end

				name = HoN_Matchmaking.teamMembers[slot].name
			else
				statusVal = 5
				name = HoN_Matchmaking.botsTable[HoN_Matchmaking.teamMembers[slot].botID].sName
			end
		end
	end

	if (statusVal) then
		local statusStrings = {	"tmm_player_not_verified",
								"tmm_player_not_ready",
								"tmm_player_needs_tokens",
								"tmm_player_is_ready",
								"mm3_coop_playerisbot",
								"mm3_player_is_host",
								"mm3_player_in_game"
							  }
		Trigger("MatchmakingGroupErrorUpdate", name, Translate(statusStrings[statusVal]), statusVal)
	end
end

function HoN_Matchmaking:matchmaking_queue_fidelity_slider_OnShow()
	GetWidget("matchmaking_queue_fidelity_slider"):SetValue(GetCvarInt("cc_TMMMatchFidelity"));
	if ((soloQueue or UIGetAccountID() == groupLeaderID) and verifiedOnly) then
		GetWidget("matchmaking_queue_fidelity_slider"):SetEnabled(true)
	else
		GetWidget("matchmaking_queue_fidelity_slider"):SetEnabled(false)
	end
end

function HoN_Matchmaking:matchmaking_queue_fidelity_slider_OnChange()
	PlaySound("/shared/sounds/ui/button_slide_01.wav")
	HoN_Matchmaking:ScheduleGroupOptionsUpdate(true)
end

function HoN_Matchmaking:matchmaking_queue_fidelity_slider_OnMouseOver()
	PlaySound("/shared/sounds/ui/button_over_01.wav")
	if (not verifiedOnly) then
		Trigger("genericMainFloatingTip", "true", "25h", "", "mm_queue_fidelity1c", "mm_queue_fidelity_tt_desc", "", "", "3h", "-2h")
	elseif (soloQueue or UIGetAccountID() == groupLeaderID) then
		Trigger("genericMainFloatingTip", "true", "25h", "", "mm_queue_fidelity1", "mm_queue_fidelity_tt_desc", "", "", "3h", "-2h")
	else
		Trigger("genericMainFloatingTip", "true", "25h", "", "mm_queue_fidelity1b", "mm_queue_fidelity_tt_desc", "", "", "3h", "-2h")
	end
end

function HoN_Matchmaking:matchmaking_queue_fidelity_slider_OnMouseOut()
	Trigger("genericMainFloatingTip", "false", "", "", "", "", "", "", "", "")
end

--------------------------
local function HeroInfoHelper(...)
	if (arg[2]) then
		HoN_Matchmaking.lastHeroInfoLookUp.isValid = true
		HoN_Matchmaking.lastHeroInfoLookUp.icon = arg[3]
		HoN_Matchmaking.lastHeroInfoLookUp.name = arg[4]
		HoN_Matchmaking.lastHeroInfoLookUp.type = arg[6]
		HoN_Matchmaking.lastHeroInfoLookUp.attacktype = arg[25]
		HoN_Matchmaking.lastHeroInfoLookUp.abilities = {}
	else
		HoN_Matchmaking.lastHeroInfoLookUp.isValid = false
	end
end
triggerHelper:RegisterWatch("HeroSelectHeroInfo", HeroInfoHelper)

local function HeroAbilityHelper(abilityNum, ...)
	if (arg[2]) then
		HoN_Matchmaking.lastHeroInfoLookUp.abilities[abilityNum+1] = arg[3]
	end
end
for i=0, 3 do triggerHelper:RegisterWatch("HeroSelectHeroAbilityInfo"..i, function(...) HeroAbilityHelper(i, ...) end) end

function HoN_Matchmaking:PopulateTeamSlot(slotNumber)
	if (not IsInGroup() or (IsInGroup() and groupLeaderID == UIGetAccountID())) then
		GetWidget("mm_teamslot"..slotNumber.."_kick"):FadeIn(150)
		GetWidget("mm_botslot"..slotNumber.."_kick"):FadeIn(150)
		GetWidget("mm_botslot"..slotNumber.."_invitebot"):FadeIn(150)
	else
		GetWidget("mm_teamslot"..slotNumber.."_kick"):FadeOut(150)
		GetWidget("mm_botslot"..slotNumber.."_kick"):FadeOut(150)
		GetWidget("mm_botslot"..slotNumber.."_invitebot"):FadeOut(150)
	end

	local mmr = nil
	if (HoN_Matchmaking.selectedTab == "pvp" and MAP_INFO[mapSelected] and MAP_INFO[mapSelected].MMR and HoN_Matchmaking.teamMembers[slotNumber] and GAMETYPE_INFO[gameTypeSelected].MMR and verifiedOnly) then
		mmr = HoN_Matchmaking.teamMembers[slotNumber].mmr
	end

	-- only sea will show mmr
	-- if not GetCvarBool('cl_GarenaEnable') then
		-- mmr = nil
	-- end

	local locked = false
	if (mapSelected and MAP_INFO[mapSelected] and slotNumber > MAP_INFO[mapSelected].SIZE) then
		locked = true
	else
		GetWidget("mm_teamslot"..slotNumber.."_icon_dis"):SetTexture("/ui/fe2/store/icons/account_icons/default.tga")
		GetWidget("mm_teamslot"..slotNumber.."_text"):SetText(Translate("mm3_coop_emptyslot"))
		GetWidget("mm_teamslot"..slotNumber.."_icon_dis"):SetColor('1 1 1 0.7')
	end

	GetWidget('mm_teamslot'..slotNumber..'_ranklevel_root'):SetVisible(false)

	if (locked) then
		GetWidget("mm_teamslot"..slotNumber.."_text"):SetText(Translate("mm3_coop_lockedslot"))
		GetWidget("mm_teamslot"..slotNumber.."_icon_dis"):SetTexture("/ui/elements/kick_disabled.tga")
		GetWidget("mm_teamslot"..slotNumber.."_icon_dis"):SetColor('1 1 1 1')
		GetWidget("mm_teamslot"..slotNumber.."_invite"):FadeOut(150)
		GetWidget("mm_teamslot"..slotNumber.."_invitebot"):FadeOut(150)
		GetWidget("mm_team_slot"..slotNumber.."_empty"):SetVisible(1)
		GetWidget("mm_team_slot"..slotNumber.."_occupied"):SetVisible(0)
		GetWidget("mm_team_slot"..slotNumber.."_bot"):SetVisible(0)
	elseif (HoN_Matchmaking.teamMembers[slotNumber] and (not (HoN_Matchmaking.teamMembers[slotNumber].isBot and HoN_Matchmaking.selectedTab ~= "coop"))) then
		GetWidget("mm_team_slot"..slotNumber.."_empty"):SetVisible(0)

		-- fill out info
		if (not HoN_Matchmaking.teamMembers[slotNumber].isBot) then	-- human
			GetWidget("mm_team_slot"..slotNumber.."_bot"):SetVisible(0)
			HoN_Matchmaking.validTeamMember = HoN_Matchmaking.validTeamMember + 1
			local icon = GetWidget("mm_teamslot"..slotNumber.."_icon")
			local name = GetWidget("mm_teamslot"..slotNumber.."_name")
			local subIcon = GetWidget("mm_teamslot"..slotNumber.."_subicon")
			local subName = GetWidget("mm_teamslot"..slotNumber.."_namesub")
			local botver = GetWidget("mm_teamslot"..slotNumber.."_botver")
			local rankicon = GetWidget("mm_teamslot"..slotNumber.."_ranklevel_icon")
			local ranktext = GetWidget("mm_teamslot"..slotNumber.."_ranklevel_text")

			local rankLevel = 0
			local ranking = 0
			if gameTypeSelected == TMM_GAME_TYPE_CAMPAIGN_NORMAL then
				rankLevel = HoN_Matchmaking.teamMembers[slotNumber].rankLevel_normal
				ranking = HoN_Matchmaking.teamMembers[slotNumber].ranking_normal
			else
				rankLevel = HoN_Matchmaking.teamMembers[slotNumber].rankLevel_casual
				ranking = HoN_Matchmaking.teamMembers[slotNumber].ranking_casual
			end
			if (rankLevel) then
				if rankLevel > 0 then
					rankicon:SetTexture('/ui/fe2/season/icon_mini/'..GetRankIconNameRankLevelAfterS6(tonumber(rankLevel)))

					if (IsMaxRankLevel(rankLevel)) and ranking > 0 then
						ranktext:SetText(Translate('player_compaign_level_name_S7_'..tostring(rankLevel))..' '..tostring(ranking))
					else
						ranktext:SetText(Translate('player_compaign_level_name_S7_'..tostring(rankLevel)))
					end
				else
					rankicon:SetTexture('/ui/fe2/season/nolevel_mini.tga')
					ranktext:SetText(Translate('player_compaign_level_name_0'))
				end
			else
				rankicon:SetTexture('$invis')
				ranktext:SetText('')
			end
			

			if (HoN_Matchmaking.teamMembers[slotNumber].isLeader) then
				GetWidget("mm_slot"..slotNumber.."_frame1"):SetColor("0.05 0.15 0.65 1.0")
				GetWidget("mm_slot"..slotNumber.."_frame2"):SetColor("0.05 0.15 0.65 1.0")
				GetWidget("mm_slot"..slotNumber.."_frame1"):SetBorderColor("0.05 0.15 0.65 1.0")
				GetWidget("mm_slot"..slotNumber.."_frame2"):SetBorderColor("0.05 0.15 0.65 1.0")
				GetWidget("mm_slot"..slotNumber.."_cpanel"):SetColor("0.05 0.15 0.65 1.0")
				GetWidget("mm_slot"..slotNumber.."_background"):SetColor("invisible")
				if (mmr and HoN_Matchmaking.selectedTab == "pvp") then
					subName:SetText("MMR: "..tostring(mmr))
					botver:SetText(Translate("mm3_host"))
				elseif HoN_Matchmaking.selectedTab == 'season' then
					subName:SetText('')
					botver:SetText(Translate("mm3_host"))
					GetWidget('mm_teamslot'..slotNumber..'_ranklevel_root'):SetVisible(true)
				else
					subName:SetText(Translate("mm3_host"))
					botver:SetText(" ")
				end
			elseif (not HoN_Matchmaking.teamMembers[slotNumber].isGood) then
				GetWidget("mm_slot"..slotNumber.."_frame1"):SetColor("0.7 0.1 0.1 1.0")
				GetWidget("mm_slot"..slotNumber.."_frame2"):SetColor("0.7 0.1 0.1 1.0")
				GetWidget("mm_slot"..slotNumber.."_frame1"):SetBorderColor("0.7 0.1 0.1 1.0")
				GetWidget("mm_slot"..slotNumber.."_frame2"):SetBorderColor("0.7 0.1 0.1 1.0")
				GetWidget("mm_slot"..slotNumber.."_cpanel"):SetColor("0.7 0.1 0.1 1.0")
				GetWidget("mm_slot"..slotNumber.."_background"):SetColor("1 0 0 0.15")
				if (mmr and HoN_Matchmaking.selectedTab == "pvp") then
					subName:SetText("MMR: "..tostring(mmr))
					if (not HoN_Matchmaking.teamMembers[slotNumber].inGame) then
						botver:SetText(Translate("mm3_error"))
					else
						botver:SetText(Translate("mm3_ingame"))
					end
				elseif HoN_Matchmaking.selectedTab == 'season' then
					subName:SetText('')
					GetWidget('mm_teamslot'..slotNumber..'_ranklevel_root'):SetVisible(true)
					if (not HoN_Matchmaking.teamMembers[slotNumber].inGame) then
						botver:SetText(Translate("mm3_error"))
					else
						botver:SetText(Translate("mm3_ingame"))
					end
				else
					if (not HoN_Matchmaking.teamMembers[slotNumber].inGame) then
						subName:SetText(Translate("mm3_error"))
					else
						subName:SetText(Translate("mm3_ingame"))
					end
					botver:SetText(" ")
				end
			elseif (HoN_Matchmaking.teamMembers[slotNumber].isReady) then
				GetWidget("mm_slot"..slotNumber.."_frame1"):SetColor("0.2 0.55 0.1 1.0")
				GetWidget("mm_slot"..slotNumber.."_frame2"):SetColor("0.2 0.55 0.1 1.0")
				GetWidget("mm_slot"..slotNumber.."_frame1"):SetBorderColor("0.2 0.55 0.1 1.0")
				GetWidget("mm_slot"..slotNumber.."_frame2"):SetBorderColor("0.2 0.55 0.1 1.0")
				GetWidget("mm_slot"..slotNumber.."_cpanel"):SetColor("0.2 0.55 0.1 1.0")
				GetWidget("mm_slot"..slotNumber.."_background"):SetColor("invisible")
				if (mmr and HoN_Matchmaking.selectedTab == "pvp") then
					subName:SetText("MMR: "..tostring(mmr))
					botver:SetText(Translate("mm3_ready"))
				elseif HoN_Matchmaking.selectedTab == 'season' then
					subName:SetText('')
					GetWidget('mm_teamslot'..slotNumber..'_ranklevel_root'):SetVisible(true)
					botver:SetText(Translate("mm3_ready"))
				else
					subName:SetText(Translate("mm3_ready"))
					botver:SetText(" ")
				end
			else
				GetWidget("mm_slot"..slotNumber.."_frame1"):SetColor("#183042")
				GetWidget("mm_slot"..slotNumber.."_frame2"):SetColor("#183042")
				GetWidget("mm_slot"..slotNumber.."_frame1"):SetBorderColor("#183042")
				GetWidget("mm_slot"..slotNumber.."_frame2"):SetBorderColor("#183042")
				GetWidget("mm_slot"..slotNumber.."_cpanel"):SetColor("#183042")
				GetWidget("mm_slot"..slotNumber.."_background"):SetColor("invisible")
				if (mmr and HoN_Matchmaking.selectedTab == "pvp") then
					subName:SetText("MMR: "..tostring(mmr))
					botver:SetText(Translate("mm3_notready"))
				elseif HoN_Matchmaking.selectedTab == 'season' then
					subName:SetText('')
					GetWidget('mm_teamslot'..slotNumber..'_ranklevel_root'):SetVisible(true)
					botver:SetText(Translate("mm3_notready"))
				else
					subName:SetText(Translate("mm3_notready"))
					botver:SetText(" ")
				end
			end
			icon:SetTexture(HoN_Matchmaking.teamMembers[slotNumber].accountIcon)

			local tempName = HoN_Matchmaking.teamMembers[slotNumber].name

			if (string.len(tempName) > 9) then
				tempName = StripClanTag(tempName)
			end

			if (string.len(tempName) < 9) then
				name:SetFont('dyn_14')
			elseif (string.len(tempName) < 12) then
				name:SetFont('dyn_12')
			else
				name:SetFont('dyn_11')
			end

			name:SetText(tempName)

			-- if (HoN_Matchmaking.teamMembers[slotNumber].verified) then
			-- 	subIcon:SetTexture("/ui/elements/verified.tga")
			-- 	subIcon:SetVisible(1)
			-- else
			-- 	subIcon:FadeOut(150)
			-- end
			subIcon:SetVisible(0)

			GetWidget("mm_team_slot"..slotNumber.."_occupied"):SetVisible(1)
		else 		 		-- bot
			GetWidget("mm_team_slot"..slotNumber.."_occupied"):SetVisible(0)
			local icon = GetWidget("mm_botslot"..slotNumber.."_icon")
			local name = GetWidget("mm_botslot"..slotNumber.."_name")
			local subIcon = GetWidget("mm_botslot"..slotNumber.."_subicon")
			--local subName = GetWidget("mm_botslot"..slotNumber.."_namesub")
			local botver = GetWidget("mm_botslot"..slotNumber.."_botver")

			icon:SetTexture(interface:UICmd("GetHeroIconPathFromProduct('"..HoN_Matchmaking.botsTable[HoN_Matchmaking.teamMembers[slotNumber].botID].sHeroName.."')"))
			name:SetText(Translate("mstore_"..HoN_Matchmaking.botsTable[HoN_Matchmaking.teamMembers[slotNumber].botID].sHeroName.."_name"))

			subIcon:SetTexture("/ui/icons/botmatch.tga")
			subIcon:SetVisible(1)
			--subName:SetText(HoN_Matchmaking.botsTable[HoN_Matchmaking.teamMembers[slotNumber].botID].sName)
			botver:SetText("")   --HoN_Matchmaking.botsTable["v"..HoN_Matchmaking.teamMembers[slotNumber].botID].nTypeID)

			GetWidget("mm_team_slot"..slotNumber.."_bot"):SetVisible(1)
		end
	else
		GetWidget("mm_team_slot"..slotNumber.."_empty"):SetVisible(1)
		GetWidget("mm_team_slot"..slotNumber.."_occupied"):SetVisible(0)
		GetWidget("mm_team_slot"..slotNumber.."_bot"):SetVisible(0)

		-- if (slotNumber == 2 or (not GetWidget("mm_team_slot"..tostring(slotNumber-1).."_empty"):IsVisible())) then
		GetWidget("mm_teamslot"..slotNumber.."_invite"):FadeIn(150)
		if (HoN_Matchmaking.selectedTab == "coop" and ((not IsInGroup()) or groupLeaderID == UIGetAccountID())) then
			GetWidget("mm_teamslot"..slotNumber.."_invitebot"):FadeIn(150)
		else
			GetWidget("mm_teamslot"..slotNumber.."_invitebot"):FadeOut(150)
		end
		-- else
		-- 	GetWidget("mm_teamslot"..slotNumber.."_invite"):FadeOut(150)
		-- 	GetWidget("mm_teamslot"..slotNumber.."_invitebot"):FadeOut(150)
		-- end
	end
end

function HoN_Matchmaking:PopulateTeam()
	HoN_Matchmaking.validTeamMember = 1
	for i=2, 5 do
		HoN_Matchmaking:PopulateTeamSlot(i)
	end
	HoN_Matchmaking:PopulateSelfSlot()

	local seasonEnd = GetWidget('mm_start_btn_seasonend'):IsVisible()
	if HoN_Matchmaking.selectedTab == "season" and HoN_Matchmaking.validTeamMember == TMM_GAME_INVALID_MEMBER_COUNT and not seasonEnd then
		GetWidget('season_invalid_teammembers_number_mask'):SetVisible(true)
	else
		GetWidget('season_invalid_teammembers_number_mask'):SetVisible(false)
	end
end

function HoN_Matchmaking:RepickBot(self, slotNumber, side)
	HoN_Matchmaking.repickingSlot[side] = tonumber(slotNumber)
end

local function QueueSolo()
	soloQueue = true
	soloQueueSendReady = true
	if (gameTypeSelected) and (mapSelected) and (gameModeSelectedString) and (regionSelectedString) and (verifiedOnly ~= nil) then
		local gameTypeSelectedOverride
		gameTypeSelectedOverride = MidwarsShennanigans(mapSelected, gameTypeSelected)
		local gameModeSelectedOverride = GameModeTrick(gameTypeSelectedOverride, gameModeSelectedString)
		local rankedgameoverride = RankedShennanigans()

		interface:UICmd("CreateTMMGroup(1, "..gameTypeSelectedOverride..", '"..mapSelected.."', '"..gameModeSelectedOverride.."', '"..regionSelectedString.."', "..tostring(rankedgameoverride)..", cc_TMMMatchFidelity, "..botDifficulty..", "..tostring(randomEnemyBots)..");")

		TMMSettingsDebug('CreateTMMGroup Solo', nil, gameTypeSelectedOverride)
	else
		println('^oMatchmaking Error - CreateTMMGroup arguments are missing')
		TMMSettingsDebug('CreateTMMGroup Solo', true)
	end
	playerMMR = {}
	GetWidget('matchmaking_queue_popularity_3'):SetVisible(0)
	GetWidget('matchmaking_queue_popularity_4'):SetVisible(0)

	GetWidget('matchmaking_queue_popularity_3b'):SetVisible(0)
	GetWidget('matchmaking_queue_popularity_4b'):SetVisible(0)
	GetWidget('matchmaking_queue_display'):SetHeight('26.2h')
	ShowWidget("mm_popup_init")
end

local function BotQueueSolo()
	soloQueue = true
	soloQueueSendReady = true
	if (gameTypeSelected) and (mapSelected) and (gameModeSelectedString) and (regionSelectedString) and (verifiedOnly ~= nil) then
		local gameTypeSelectedOverride
		gameTypeSelectedOverride = MidwarsShennanigans(mapSelected, gameTypeSelected)
		local gameModeSelectedOverride = GameModeTrick(gameTypeSelectedOverride, gameModeSelectedString)
		local regWidget = GetWidget("mm_start_btn") -- register on the start button cause why not?
		-- the group does not exist immediately, when it does add the bots and ready up
		local tmmQueueHelper = function ()
			regWidget:UnregisterWatch("TMMDisplay")

			-- add all the enemy bots
			SetEnemyBot(0, HoN_Matchmaking.botsTable[HoN_Matchmaking.bots[1]].sName)
			SetEnemyBot(1, HoN_Matchmaking.botsTable[HoN_Matchmaking.bots[2]].sName)
			SetEnemyBot(2, HoN_Matchmaking.botsTable[HoN_Matchmaking.bots[3]].sName)
			SetEnemyBot(3, HoN_Matchmaking.botsTable[HoN_Matchmaking.bots[4]].sName)
			SetEnemyBot(4, HoN_Matchmaking.botsTable[HoN_Matchmaking.bots[5]].sName)

			interface:UICmd("SendTMMPlayerReadyStatus(1,"..gameTypeSelectedOverride..");")
		end
		regWidget:RegisterWatch("TMMDisplay", tmmQueueHelper)

		interface:UICmd("CreateTMMGroup(3, "..gameTypeSelectedOverride..", '"..mapSelected.."', '"..gameModeSelectedOverride.."', '"..regionSelectedString.."', false, cc_TMMMatchFidelity, "..botDifficulty..", "..tostring(randomEnemyBots)..");")
		TMMSettingsDebug('CreateTMMGroup Solo', nil, gameTypeSelectedOverride)
	else
		println('^oMatchmaking Error - CreateTMMGroup arguments are missing')
		TMMSettingsDebug('CreateTMMGroup Solo', true)
	end
	playerMMR = {}
	GetWidget('matchmaking_queue_popularity_3'):SetVisible(0)
	GetWidget('matchmaking_queue_popularity_4'):SetVisible(0)
	GetWidget('matchmaking_queue_popularity_3b'):SetVisible(0)
	GetWidget('matchmaking_queue_popularity_4b'):SetVisible(0)
	GetWidget('matchmaking_queue_display'):SetHeight('26.2h')
	ShowWidget("mm_popup_init")
end

local function ReadyTeam()
	local gameTypeSelectedOverride
	gameTypeSelectedOverride = MidwarsShennanigans(mapSelected, gameTypeSelected)
	interface:UICmd("SendTMMPlayerReadyStatus(1,"..gameTypeSelectedOverride..");")
end

function HoN_Matchmaking:EnterQueue()
	-- TMM Group handling stufffffffff~~~
	if (IsInGroup() and groupLeaderID ~= UIGetAccountID()) then
		-- we just need to ready up, none of the other stuff
		if (not HoN_Matchmaking.teamMembers[1].isReady) then
			CheckTokensAndDoFunction(ReadyTeam)
		else
			local gameTypeSelectedOverride
			gameTypeSelectedOverride = MidwarsShennanigans(mapSelected, gameTypeSelected)
			interface:UICmd("SendTMMPlayerReadyStatus(0,"..gameTypeSelectedOverride..");")
		end
	else
		local haveFriends = false
		if (IsInGroup()) then
			-- make sure we have players in the group, if we don't then we should disband the TMM and just roll solo
			for i=2,5 do
				if (HoN_Matchmaking.teamMembers[i] and not HoN_Matchmaking.teamMembers[i].isBot) then
					haveFriends = true
					break
				end
			end
			-- if (not haveFriends) then -- :(
			-- 	-- kick from their group, they can do solo queue
			-- 	HoN_Matchmaking.suppressDCDiag = true
			-- 	interface:UICmd("LeaveTMMGroup()")
			-- 	Set('_TMM_GroupTeamSize', 1, "int") -- dunno if these are needed, better safe than sorry
			-- 	groupSize = 1
			-- end
		end

		-- Removed the 'GetTMMOtherPlayersReady's below so that we can send a sound to all group members that
		-- aren't ready
		if (HoN_Matchmaking.selectedTab == "pvp") then
			if (IsInGroup() and groupLeaderID == UIGetAccountID()) then
				if (gameModeSelected["lp"] or gameModeSelected["cm"]) then
					local hasFive = true
					for i=2,5 do
						if (not HoN_Matchmaking.teamMembers[i] or HoN_Matchmaking.teamMembers[i].isBot) then
							hasFive = false
							break
						end
					end

					if (hasFive) then-- and AtoB(interface:UICmd("GetTMMOtherPlayersReady()"))) then
						PlaySound("/shared/sounds/ui/ccpanel/button_click_02.wav")
						CheckTokensAndDoFunction(ReadyTeam)
					end
				else--if (AtoB(interface:UICmd("GetTMMOtherPlayersReady()"))) then
					-- we are already in a TMM group and good to go
					PlaySound("/shared/sounds/ui/ccpanel/button_click_02.wav")
					CheckTokensAndDoFunction(ReadyTeam)
				end
			elseif (not (gameModeSelected["lp"] or gameModeSelected["cm"])) then
				-- create a solo tmm group
				Set('_TMM_GroupTeamSize', 1, 'int')
				groupSize = 1
				Set('_TMM_tournamentSettings', 'false', 'string')
				rulesetMode = defaultRulesetMode
				viewMode = defaultRulesetMode
				PlaySound("/shared/sounds/ui/ccpanel/button_click_02.wav")
				CheckTokensAndDoFunction(QueueSolo)
			end
		elseif HoN_Matchmaking.selectedTab == "season" then
			if (IsInGroup() and groupLeaderID == UIGetAccountID()) then
				-- we are already in a TMM group and good to go
				PlaySound("/shared/sounds/ui/ccpanel/button_click_02.wav")
				CheckTokensAndDoFunction(ReadyTeam)
			else
				-- create a solo tmm group
				Set('_TMM_GroupTeamSize', 1, 'int')
				groupSize = 1
				Set('_TMM_tournamentSettings', 'false', 'string')
				rulesetMode = defaultRulesetMode
				viewMode = defaultRulesetMode
				PlaySound("/shared/sounds/ui/ccpanel/button_click_02.wav")
				CheckTokensAndDoFunction(QueueSolo)
			end
		else
			if (IsInGroup() and groupLeaderID == UIGetAccountID() and (haveFriends or not HoN_Matchmaking.teamHasBots)) then
				--if (AtoB(interface:UICmd("GetTMMOtherPlayersReady()"))) then
					-- we are already in a TMM group and good to go
					PlaySound("/shared/sounds/ui/ccpanel/button_click_02.wav")
					CheckTokensAndDoFunction(ReadyTeam)
				--end
			elseif (not HoN_Matchmaking.teamHasBots) then -- queue solo
				-- queuing solo for bot game
				Set('_TMM_GroupTeamSize', 1, 'int')
				groupSize = 1
				Set('_TMM_tournamentSettings', 'false', 'string')
				rulesetMode = defaultRulesetMode
				viewMode = defaultRulesetMode
				PlaySound("/shared/sounds/ui/ccpanel/button_click_02.wav")
				BotQueueSolo() -- don't need to check tokens
			else -- start local
				if (IsInGroup()) then
					HoN_Matchmaking.suppressDCDiag = true
					interface:UICmd("LeaveTMMGroup()")
				end
				HoN_Matchmaking:StartLocalBotGame()
				-- hide the matchmaking panel
				if (GetWidget("matchmaking"):IsVisible()) then
					Set("_mainmenu_currentpanel", "", "string")
					GetWidget("MainMenuPanelSwitcher"):DoEvent()
				end
			end
		end
	end
end

function HoN_Matchmaking:StartLocalBotGame()
	Set("ui_local_bot_game", true, "bool")

	local nBotDifficulty = botDifficulty

	-- build bots string
	local bots1 = ""
	local bots2 = ""
	if (not randomAllybots) then
		for i=2,5 do
			bots1 = bots1 .. HoN_Matchmaking.botsTable[HoN_Matchmaking.teamMembers[i].botID].sName
			if i ~= 5 then
				bots1 = bots1 .. "|"
			end
		end
	end

	if (not randomEnemyBots) then
		for i=1,5 do
			bots2 = bots2 .. HoN_Matchmaking.botsTable[HoN_Matchmaking.bots[i]].sName
			if i ~= 5 then
				bots2 = bots2 .. "|"
			end
		end
	end

	-- start the game!!!!!
	local sStartGame = "StartGame practice LocalBotsGame mode:botmatch map:caldavar casual:true"

	if (math.random(2) == 1) then
		if randomEnemyBots and randomAllybots then
			sStartGame = sStartGame.." randombots:4|5"
		elseif randomEnemyBots then
			sStartGame = sStartGame.." randombots:0|5".." bots1:"..bots1
		elseif randomAllybots then
			sStartGame = sStartGame.." randombots:4|0".." bots2:"..bots2
		else
			sStartGame = sStartGame.." bots1:"..bots1.." bots2:"..bots2
		end
	else
		if randomEnemyBots and randomAllybots then
			sStartGame = sStartGame.." randombots:5|4"
		elseif randomEnemyBots then
			sStartGame = sStartGame.." randombots:5|0".." bots2:"..bots1
		elseif randomAllybots then
			sStartGame = sStartGame.." randombots:0|4".." bots1:"..bots2
		else
			sStartGame = sStartGame.." bots1:"..bots1.." bots2:"..bots2
		end
	end
	--Echo("^:^r"..sStartGame)
	Cmd(sStartGame)
end
triggerHelper:RegisterWatch("TMMStartLocalBotMatch", function() HoN_Matchmaking:StartLocalBotGame() end)

function HoN_Matchmaking:SlideInvite(slider, value)
	HoN_Matchmaking.invitePersonScrollOffset = tonumber(value)
	HoN_Matchmaking:PopulateInviteList(HoN_Matchmaking.invitePersonScrollOffset)
end

function HoN_Matchmaking:PopulateInviteList(scrollOffset)
	local anyVisible = false
	for i=1, 7 do
		if (HoN_Matchmaking.invitePersonAutocompleteTable[i+scrollOffset]) then
			anyVisible = true

			local name = HoN_Matchmaking.invitePersonAutocompleteTable[i+scrollOffset]
			GetWidget("mm_autocomplete_plate_"..i):SetVisible(1)
			GetWidget("mm_autocomplete_plate_"..i.."_name"):SetText(name)

			-- invited indicator
			if (HoN_Matchmaking.invitedPersons and HoN_Matchmaking.invitedPersons[StripClanTag(name)]) then
				GetWidget("mm_autocomplete_plate_"..i.."_invited"):SetVisible(1)
			else
				GetWidget("mm_autocomplete_plate_"..i.."_invited"):SetVisible(0)
			end

			inGroup = nil
			local strippedName = StripClanTag(name)
			for i,g in ipairs(HoN_Matchmaking.buddyLists) do
				if (g[strippedName]) then
					inGroup = i
					break
				end
			end

			if (inGroup) then 	-- steal the info for the social panel
				-- account icon
				if (HoN_Matchmaking.buddyLists[inGroup][strippedName].accountIcon and HoN_Matchmaking.buddyLists[inGroup][strippedName].accountIcon ~= "") then
					GetWidget("mm_autocomplete_plate_"..i.."_icon"):SetTexture(HoN_Matchmaking.buddyLists[inGroup][strippedName].accountIcon)
				else
					GetWidget("mm_autocomplete_plate_"..i.."_icon"):SetTexture("ui/fe2/store/icons/account_icons/default.tga")
				end
				-- name color
				if (HoN_Matchmaking.buddyLists[inGroup][strippedName].status and HoN_Matchmaking.buddyLists[inGroup][strippedName].status == "ingame") then
					if (not SHOW_CUSTOM_COLORS) then -- social panel option
						GetWidget("mm_autocomplete_plate_"..i.."_name"):SetColor("#f3c113")
						GetWidget("mm_autocomplete_plate_"..i.."_name"):SetGlow(false)
					elseif (HoN_Matchmaking.buddyLists[inGroup][strippedName].chatColorIngameString and HoN_Matchmaking.buddyLists[inGroup][strippedName].chatColorIngameString ~= "") then
						GetWidget("mm_autocomplete_plate_"..i.."_name"):SetColor(HoN_Matchmaking.buddyLists[inGroup][strippedName].chatColorIngameString)
						GetWidget("mm_autocomplete_plate_"..i.."_name"):SetGlow(AtoB(HoN_Matchmaking.buddyLists[inGroup][strippedName].nameGlow))

						if HoN_Matchmaking.buddyLists[inGroup][strippedName].nameGlowColorIngame and HoN_Matchmaking.buddyLists[inGroup][strippedName].nameGlowColorIngame ~= "" then
							GetWidget("mm_autocomplete_plate_"..i.."_name"):SetGlowColor(HoN_Matchmaking.buddyLists[inGroup][strippedName].nameGlowColorIngame)
						end
					else
						GetWidget("mm_autocomplete_plate_"..i.."_name"):SetColor("#999999")
						GetWidget("mm_autocomplete_plate_"..i.."_name"):SetGlow(false)
					end
				else
					if (not SHOW_CUSTOM_COLORS) then -- social panel option
						GetWidget("mm_autocomplete_plate_"..i.."_name"):SetColor("#39c6ff")
						GetWidget("mm_autocomplete_plate_"..i.."_name"):SetGlow(false)
					elseif (HoN_Matchmaking.buddyLists[inGroup][strippedName].chatColorOnlineString and HoN_Matchmaking.buddyLists[inGroup][strippedName].chatColorOnlineString ~= "") then
						GetWidget("mm_autocomplete_plate_"..i.."_name"):SetColor(HoN_Matchmaking.buddyLists[inGroup][strippedName].chatColorOnlineString)
						GetWidget("mm_autocomplete_plate_"..i.."_name"):SetGlow(AtoB(HoN_Matchmaking.buddyLists[inGroup][strippedName].nameGlow))
						
						if HoN_Matchmaking.buddyLists[inGroup][strippedName].nameGlowColor and HoN_Matchmaking.buddyLists[inGroup][strippedName].nameGlowColor ~= "" then
							GetWidget("mm_autocomplete_plate_"..i.."_name"):SetGlowColor(HoN_Matchmaking.buddyLists[inGroup][strippedName].nameGlowColor)
						end

					else
						GetWidget("mm_autocomplete_plate_"..i.."_name"):SetColor("white")
						GetWidget("mm_autocomplete_plate_"..i.."_name"):SetGlow(false)
					end
				end
			else 											-- try to get info
				if (not HoN_Matchmaking.userInfoTable[name]) then 	-- attempt to fill out the info
					local userInfo = GetChatClientInfo(name, "getaccounticontexturepath|chatnamecolorstring|chatnamecoloringamestring|matchid|chatnameglow|chatnameglowcolorstring|chatnameglowcoloringamestring")
					if (userInfo and userInfo ~= "") then
						infoTable = Explode("|", userInfo)
						HoN_Matchmaking.userInfoTable[name] = {}
						HoN_Matchmaking.userInfoTable[name].accountIcon = infoTable[1]
						HoN_Matchmaking.userInfoTable[name].chatColorOnlineString = infoTable[2]
						HoN_Matchmaking.userInfoTable[name].chatColorIngameString = infoTable[3]
						if (infoTable[4] == "4294967295") then
							HoN_Matchmaking.userInfoTable[name].status = "online"
						else
							HoN_Matchmaking.userInfoTable[name].status = "ingame"
						end
						if (infoTable[5] and NotEmpty(infoTable[5])) then
							HoN_Matchmaking.userInfoTable[name].nameGlow = infoTable[5]
						else
							HoN_Matchmaking.userInfoTable[name].nameGlow = false
						end

						if (infoTable[6] and NotEmpty(infoTable[6])) then
							HoN_Matchmaking.userInfoTable[name].nameGlowColor = infoTable[6]
						else
							HoN_Matchmaking.userInfoTable[name].nameGlowColor = ""
						end

						if (infoTable[7] and NotEmpty(infoTable[7])) then
							HoN_Matchmaking.userInfoTable[name].nameGlowColorIngame = infoTable[7]
						else
							HoN_Matchmaking.userInfoTable[name].nameGlowColorIngame = ""
						end
					end
				else
					-- if the info is cached, update the ingame status
					local matchID = GetChatClientInfo(name, "matchid")
					if (matchID ~= "") then
						if (string.sub(matchID, 0, -2) == "4294967295") then
							HoN_Matchmaking.userInfoTable[name].status = "online"
						else
							HoN_Matchmaking.userInfoTable[name].status = "ingame"
						end
					end
				end

				if (HoN_Matchmaking.userInfoTable[name]) then -- use info
					-- account icon
					if (HoN_Matchmaking.userInfoTable[name].accountIcon and HoN_Matchmaking.userInfoTable[name].accountIcon ~= "") then
						GetWidget("mm_autocomplete_plate_"..i.."_icon"):SetTexture(HoN_Matchmaking.userInfoTable[name].accountIcon)
					else
						GetWidget("mm_autocomplete_plate_"..i.."_icon"):SetTexture("ui/fe2/store/icons/account_icons/default.tga")
					end
					-- name color
					if (HoN_Matchmaking.userInfoTable[name].status and HoN_Matchmaking.userInfoTable[name].status == "ingame") then
						if (not SHOW_CUSTOM_COLORS) then
							GetWidget("mm_autocomplete_plate_"..i.."_name"):SetColor("#f3c113")
							GetWidget("mm_autocomplete_plate_"..i.."_name"):SetGlow(false)
						elseif (HoN_Matchmaking.userInfoTable[name].chatColorIngameString and HoN_Matchmaking.userInfoTable[name].chatColorIngameString ~= "") then
							GetWidget("mm_autocomplete_plate_"..i.."_name"):SetColor(HoN_Matchmaking.userInfoTable[name].chatColorIngameString)
							GetWidget("mm_autocomplete_plate_"..i.."_name"):SetGlow(AtoB(HoN_Matchmaking.userInfoTable[name].nameGlow))

							if HoN_Matchmaking.buddyLists[inGroup][strippedName].nameGlowColorIngame and HoN_Matchmaking.buddyLists[inGroup][strippedName].nameGlowColorIngame ~= "" then
								GetWidget("mm_autocomplete_plate_"..i.."_name"):SetGlowColor(HoN_Matchmaking.userInfoTable[name].nameGlowColorIngame)
							end
						else
							GetWidget("mm_autocomplete_plate_"..i.."_name"):SetColor("#999999")
							GetWidget("mm_autocomplete_plate_"..i.."_name"):SetGlow(false)
						end
					else
						if (not SHOW_CUSTOM_COLORS) then
							GetWidget("mm_autocomplete_plate_"..i.."_name"):SetColor("#39c6ff")
							GetWidget("mm_autocomplete_plate_"..i.."_name"):SetGlow(false)
						elseif (HoN_Matchmaking.userInfoTable[name].chatColorOnlineString and HoN_Matchmaking.userInfoTable[name].chatColorOnlineString ~= "") then
							GetWidget("mm_autocomplete_plate_"..i.."_name"):SetColor(HoN_Matchmaking.userInfoTable[name].chatColorOnlineString)
							GetWidget("mm_autocomplete_plate_"..i.."_name"):SetGlow(AtoB(HoN_Matchmaking.userInfoTable[name].nameGlow))

							if HoN_Matchmaking.buddyLists[inGroup][strippedName].nameGlowColor and HoN_Matchmaking.buddyLists[inGroup][strippedName].nameGlowColor ~= "" then
								GetWidget("mm_autocomplete_plate_"..i.."_name"):SetGlowColor(HoN_Matchmaking.userInfoTable[name].nameGlowColor)
							end
						else
							GetWidget("mm_autocomplete_plate_"..i.."_name"):SetColor("white")
							GetWidget("mm_autocomplete_plate_"..i.."_name"):SetGlow(false)
						end
					end
				else 										  -- use default
					GetWidget("mm_autocomplete_plate_"..i.."_icon"):SetTexture("ui/fe2/store/icons/account_icons/default.tga")
					GetWidget("mm_autocomplete_plate_"..i.."_name"):SetColor("white")
				end
			end
		else
			GetWidget("mm_autocomplete_plate_"..i):SetVisible(0)
		end
	end

	GetWidget("mm_noauto_cover"):SetVisible(not anyVisible)
end

function HoN_Matchmaking:ChatAutoCompleteAdd(widget, name)
	if (not GetWidget("mm_invite_person"):IsVisible()) then
		return end
	-- make sure what is being auto completed is possibly coming from the MM text box
	-- thus auto completes from elsewhere won't screw up the mm autocomplete so bad
	-- it will still get cleared though ~~
	local inputBoxVal = StripClanTag(GetWidget("mm_autocomplete_textbox"):GetValue())

	if (string.lower(string.sub(name, 1, string.len(inputBoxVal))) == string.lower(inputBoxVal) and StripClanTag(name) ~= StripClanTag(GetAccountName())) then -- weeeeee
		-- search the list and make sure it's not a duplicate
		local nameExists = false
		for i,n in ipairs(HoN_Matchmaking.invitePersonAutocompleteTable) do
			if (string.lower(StripClanTag(n)) == string.lower(StripClanTag(name))) then
				nameExists = true
				break
			end
		end

		for i,n in ipairs(HoN_Matchmaking.teamMembers) do
			if (n.name) and (string.lower(StripClanTag(n.name)) == string.lower(StripClanTag(name))) then
				nameExists = true
				break
			end
		end

		if (not nameExists) then
			table.insert(HoN_Matchmaking.invitePersonAutocompleteTable, name)
			HoN_Matchmaking.inviteAutoCompleteAddOffset = HoN_Matchmaking.inviteAutoCompleteAddOffset + 1
			HoN_Matchmaking:PopulateInviteList(HoN_Matchmaking.invitePersonScrollOffset)

			local scrollNeeded = 0
			if (HoN_Matchmaking.invitePersonAutocompleteTable) then
				scrollNeeded = #HoN_Matchmaking.invitePersonAutocompleteTable - 6
			end
			local scrollBar = GetWidget("mm_invite_scrollbar")
			if (scrollNeeded > 0) then
				scrollBar:SetMaxValue(scrollNeeded)
				scrollBar:SetValue(0)
			else
				scrollBar:SetMaxValue(0)
				scrollBar:SetValue(0)
			end
		end
	end
end
triggerHelper:RegisterWatch("ChatAutoCompleteAdd", function(...) HoN_Matchmaking:ChatAutoCompleteAdd(...) end)

function HoN_Matchmaking:ChatAutoCompleteClear(widget)
	if (not GetWidget("mm_invite_person"):IsVisible()) then
		return end

	HoN_Matchmaking.invitePersonAutocompleteTable = {}
	HoN_Matchmaking.inviteAutoCompleteAddOffset = 1
	HoN_Matchmaking.invitePersonScrollOffset = 0
	GetWidget("mm_invite_scrollbar"):SetMaxValue(0)

	-- build a table of team names to so we don't have to cycle against every team mate's name per buddy list name
	local teamMates = {}
	for k,n in ipairs(HoN_Matchmaking.teamMembers) do
		if (n.name) then
			teamMates[StripClanTag(n.name)] = true
		end
	end

	-- add all the valid friends/clans/etc
	local findString = GetWidget("mm_autocomplete_textbox"):GetValue()
	local listedPlayers = {}
	if (HoN_Matchmaking.buddyLists) then
		for i,t in ipairs(HoN_Matchmaking.buddyLists) do
			for j,p in pairs(t) do
				if ((j ~= StripClanTag(GetAccountName())) and (p.status ~= 'offline') and (inputBoxVal == "" or (string.find(string.lower((p.tag or '')..j), string.lower(findString), 1, true)))) then
					if (not listedPlayers[j] and not teamMates[j]) then
						table.insert(HoN_Matchmaking.invitePersonAutocompleteTable, (p.tag or '')..j)
						listedPlayers[j] = true
					end
				end
			end
		end
	end

	if (#HoN_Matchmaking.invitePersonAutocompleteTable > 1) then 		-- sort the users in the list
		local sortFunc = function(a, b) return (string.lower(StripClanTag(a)) < string.lower(StripClanTag(b))) end
		table.sort(HoN_Matchmaking.invitePersonAutocompleteTable, sortFunc)

		local scrollNeeded = #HoN_Matchmaking.invitePersonAutocompleteTable - 6
		local scrollBar = GetWidget("mm_invite_scrollbar")
		if (scrollNeeded > 0) then
			scrollBar:SetMaxValue(scrollNeeded)
			scrollBar:SetValue(0)
		else
			scrollBar:SetValue(0)
			scrollBar:SetMaxValue(0)
		end
	end

	HoN_Matchmaking:PopulateInviteList(HoN_Matchmaking.invitePersonScrollOffset)
end
triggerHelper:RegisterWatch("ChatAutoCompleteClear", function(...) HoN_Matchmaking:ChatAutoCompleteClear(...) end)

function HoN_Matchmaking:InvitePlayerName(widget, name)
	if (NotEmpty(name) and (string.lower(StripClanTag(name)) ~= string.lower(StripClanTag(GetAccountName())))) then
		if (not IsInGroup()) then 	-- create a TMM group and whatnot
			playerMMR = {}
			PlaySound("/shared/sounds/ui/ccpanel/button_click_02.wav")
			soloQueue = false
			local groupType = 0
			if (gameTypeSelected) and (mapSelected) and (gameModeSelectedString) and (regionSelectedString) and (verifiedOnly ~= nil) then
				local gameTypeSelectedOverride
				gameTypeSelectedOverride = MidwarsShennanigans(mapSelected, gameTypeSelected)
				if (HoN_Matchmaking.selectedTab == "coop") then
					groupType = 3
				elseif (HoN_Matchmaking.selectedTab == "season") then
					groupType = 4
				else
					groupType = 2
				end

				local gameModeSelectedOverride = GameModeTrick(gameTypeSelectedOverride, gameModeSelectedString)
				local rankedgameoverride = RankedShennanigans() 

				-- the group does not exist immediately, TMMDisplay will be triggered when it does though
				-- this makes it so we will invite the person as soon as the group exists, otherwise the
				-- invite just doesn't get sent.
				-- We also will get the group leader ID here~~
				local tmmInviteHelper = function (...)
					widget:UnregisterWatch("TMMDisplay")
					groupLeaderID = arg[30]

					-- invite all the bots! (we won't have any players at this point)
					if (HoN_Matchmaking.teamHasBots) then
						SetTeamBot(1, HoN_Matchmaking.botsTable[HoN_Matchmaking.teamMembers[2].botID].sName)
						SetTeamBot(2, HoN_Matchmaking.botsTable[HoN_Matchmaking.teamMembers[3].botID].sName)
						SetTeamBot(3, HoN_Matchmaking.botsTable[HoN_Matchmaking.teamMembers[4].botID].sName)
						SetTeamBot(4, HoN_Matchmaking.botsTable[HoN_Matchmaking.teamMembers[5].botID].sName)
					end

					SetEnemyBot(0, HoN_Matchmaking.botsTable[HoN_Matchmaking.bots[1]].sName)
					SetEnemyBot(1, HoN_Matchmaking.botsTable[HoN_Matchmaking.bots[2]].sName)
					SetEnemyBot(2, HoN_Matchmaking.botsTable[HoN_Matchmaking.bots[3]].sName)
					SetEnemyBot(3, HoN_Matchmaking.botsTable[HoN_Matchmaking.bots[4]].sName)
					SetEnemyBot(4, HoN_Matchmaking.botsTable[HoN_Matchmaking.bots[5]].sName)

					interface:UICmd("InviteToTMMGroup('"..StripClanTag(name).."');")
					if (HoN_Matchmaking.invitedPersons) then
						HoN_Matchmaking.invitedPersons[StripClanTag(name)] = true
					end

					-- disabled as groupsize will be 1. GetWidget("mm_invite_person"):FadeOut(100)
					Trigger("ChatAutoCompleteClear") --clear and add in the friends and clan
					HoN_Matchmaking:PopulateInviteList(HoN_Matchmaking.invitePersonScrollOffset)
				end
				widget:RegisterWatch("TMMDisplay", tmmInviteHelper)

				interface:UICmd("CreateTMMGroup("..groupType..", "..gameTypeSelectedOverride..", '"..mapSelected.."', '"..gameModeSelectedOverride.."', '"..regionSelectedString.."', "..tostring(rankedgameoverride)..", cc_TMMMatchFidelity, "..botDifficulty..", "..tostring(randomEnemyBots)..");")
				TMMSettingsDebug('CreateTMMGroup Group', nil, gameTypeSelectedOverride)
			else
				println('^oMatchmaking Error - CreateTMMGroup arguments are missing')
				TMMSettingsDebug('CreateTMMGroup Group', true)
			end
		else
			interface:UICmd("InviteToTMMGroup('"..StripClanTag(name).."');")
			if (HoN_Matchmaking.invitedPersons) then
				HoN_Matchmaking.invitedPersons[StripClanTag(name)] = true
			end
			if (groupSize > 3) then
				GetWidget("mm_invite_person"):FadeOut(100)
			else
				-- trigger populate to make the star appear
				Trigger("ChatAutoCompleteClear") --clear and add in the friends and clan
				HoN_Matchmaking:PopulateInviteList(HoN_Matchmaking.invitePersonScrollOffset)
			end
		end
	end
end

function HoN_Matchmaking:InvitePlayer(widget)
	HoN_Matchmaking:InvitePlayerName(widget, widget:GetValue())
	widget:UICmd("EraseInputLine();")
end

function HoN_Matchmaking:PopulateSelfSlot()
	if (not IsLoggedIn()) then return end
	if (GetAccountName() == "UnnamedNewbie") then return end

	local tempName = GetAccountName()

	if (string.len(tempName) > 9) then
		tempName = StripClanTag(tempName)
	end

	if (string.len(tempName) < 9) then
		GetWidget("mm_player_name"):SetFont('dyn_14')
	elseif (string.len(tempName) < 12) then
		GetWidget("mm_player_name"):SetFont('dyn_12')
	else
		GetWidget("mm_player_name"):SetFont('dyn_11')
	end

	GetWidget("mm_player_name"):SetText(tempName)

	local mmr = nil
	if (HoN_Matchmaking.selectedTab == "pvp" and MAP_INFO[mapSelected] and MAP_INFO[mapSelected].MMR and verifiedOnly) then
		if (gameTypeSelected == 1) then
			mmr = interface:UICmd("ClampRank(Floor(GetNormalMMR()), 0)")
		else
			mmr = interface:UICmd("ClampRank(Floor(GetCasualMMR()), 0)")
		end
	end

	-- if not GetCvarBool('cl_GarenaEnable') then
		-- mmr = nil
	-- end

	if (GetChatClientInfo) then
		local iconTable = Explode( "|", GetChatClientInfo(StripClanTag(GetAccountName()), "getaccounticontexturepath"))
		if (iconTable[1]) then GetWidget("mm_player_icon"):SetTexture(iconTable[1]) end
	end

	GetWidget('mm_player_ranklevel_root'):SetVisible(false)
	-- fill out info
	if (IsInGroup()) then
		if (groupLeaderID == UIGetAccountID()) then
			GetWidget("mm_topslot_frame1"):SetColor("0.05 0.15 0.65 1.0")
			GetWidget("mm_topslot_frame2"):SetColor("0.05 0.15 0.65 1.0")
			GetWidget("mm_topslot_frame1"):SetBorderColor("0.05 0.15 0.65 1.0")
			GetWidget("mm_topslot_frame2"):SetBorderColor("0.05 0.15 0.65 1.0")
			GetWidget("mm_topslot_cpanel"):SetColor("0.05 0.15 0.65 1.0")
			GetWidget("mm_topslot_background"):SetColor("invisible")
			if (HoN_Matchmaking.selectedTab == "pvp" and mmr) then
				GetWidget("mm_player_mmr"):SetText("MMR: "..tostring(mmr))
				GetWidget("mm_player_title"):SetText(Translate("mm3_host"))
			elseif HoN_Matchmaking.selectedTab == 'season' then
				GetWidget("mm_player_mmr"):SetText('')
				GetWidget("mm_player_title"):SetText(Translate("mm3_host"))
				GetWidget('mm_player_ranklevel_root'):SetVisible(true)
			else
				GetWidget("mm_player_mmr"):SetText(Translate("mm3_host"))
				GetWidget("mm_player_title"):SetText(" ")
			end
		elseif (HoN_Matchmaking.teamMembers[1] and not HoN_Matchmaking.teamMembers[1].isGood) then
			GetWidget("mm_topslot_frame1"):SetColor("0.7 0.1 0.1 1.0")
			GetWidget("mm_topslot_frame2"):SetColor("0.7 0.1 0.1 1.0")
			GetWidget("mm_topslot_frame1"):SetBorderColor("0.7 0.1 0.1 1.0")
			GetWidget("mm_topslot_frame2"):SetBorderColor("0.7 0.1 0.1 1.0")
			GetWidget("mm_topslot_cpanel"):SetColor("0.7 0.1 0.1 1.0")
			GetWidget("mm_topslot_background"):SetColor("1 0 0 0.15")
			if (HoN_Matchmaking.selectedTab == "pvp" and mmr) then
				GetWidget("mm_player_mmr"):SetText("MMR: "..tostring(mmr))
				if (not HoN_Matchmaking.teamMembers[1].inGame) then
					GetWidget("mm_player_title"):SetText(Translate("mm3_error"))
				else
					GetWidget("mm_player_title"):SetText(Translate("mm3_ingame"))
				end
			elseif HoN_Matchmaking.selectedTab == 'season' then
				GetWidget("mm_player_mmr"):SetText('')
				GetWidget('mm_player_ranklevel_root'):SetVisible(true)
				if (not HoN_Matchmaking.teamMembers[1].inGame) then
					GetWidget("mm_player_title"):SetText(Translate("mm3_error"))
				else
					GetWidget("mm_player_title"):SetText(Translate("mm3_ingame"))
				end
			else
				if (not HoN_Matchmaking.teamMembers[1].inGame) then
					GetWidget("mm_player_mmr"):SetText(Translate("mm3_error"))
				else
					GetWidget("mm_player_mmr"):SetText(Translate("mm3_ingame"))
				end
				GetWidget("mm_player_title"):SetText(" ")
			end
		elseif (HoN_Matchmaking.teamMembers[1] and HoN_Matchmaking.teamMembers[1].isReady) then
			GetWidget("mm_topslot_frame1"):SetColor("0.2 0.55 0.1 1.0")
			GetWidget("mm_topslot_frame2"):SetColor("0.2 0.55 0.1 1.0")
			GetWidget("mm_topslot_frame1"):SetBorderColor("0.2 0.55 0.1 1.0")
			GetWidget("mm_topslot_frame2"):SetBorderColor("0.2 0.55 0.1 1.0")
			GetWidget("mm_topslot_cpanel"):SetColor("0.2 0.55 0.1 1.0")
			GetWidget("mm_topslot_background"):SetColor("invisible")
			if (HoN_Matchmaking.selectedTab == "pvp" and mmr) then
				GetWidget("mm_player_mmr"):SetText("MMR: "..tostring(mmr))
				GetWidget("mm_player_title"):SetText(Translate("mm3_ready"))
			elseif HoN_Matchmaking.selectedTab == 'season' then
				GetWidget("mm_player_mmr"):SetText('')
				GetWidget("mm_player_title"):SetText(Translate("mm3_ready"))
				GetWidget('mm_player_ranklevel_root'):SetVisible(true)
			else
				GetWidget("mm_player_mmr"):SetText(Translate("mm3_ready"))
				GetWidget("mm_player_title"):SetText(" ")
			end
		else
			GetWidget("mm_topslot_frame1"):SetColor("#183042")
			GetWidget("mm_topslot_frame2"):SetColor("#183042")
			GetWidget("mm_topslot_frame1"):SetBorderColor("#183042")
			GetWidget("mm_topslot_frame2"):SetBorderColor("#183042")
			GetWidget("mm_topslot_cpanel"):SetColor("#183042")
			GetWidget("mm_topslot_background"):SetColor("invisible")
			if (HoN_Matchmaking.selectedTab == "pvp" and mmr) then
				GetWidget("mm_player_mmr"):SetText("MMR: "..tostring(mmr))
				GetWidget("mm_player_title"):SetText(Translate("mm3_notready"))
			elseif HoN_Matchmaking.selectedTab == 'season' then
				GetWidget("mm_player_mmr"):SetText('')
				GetWidget("mm_player_title"):SetText(Translate("mm3_notready"))
				GetWidget('mm_player_ranklevel_root'):SetVisible(true)
			else
				GetWidget("mm_player_mmr"):SetText(Translate("mm3_notready"))
				GetWidget("mm_player_title"):SetText(" ")
			end
		end
	else
		GetWidget("mm_topslot_frame1"):SetColor("#183042")
		GetWidget("mm_topslot_frame2"):SetColor("#183042")
		GetWidget("mm_topslot_frame1"):SetBorderColor("#183042")
		GetWidget("mm_topslot_frame2"):SetBorderColor("#183042")
		GetWidget("mm_topslot_cpanel"):SetColor("#183042")
		GetWidget("mm_topslot_background"):SetColor("invisible")
		if (HoN_Matchmaking.selectedTab == "season") then
			GetWidget("mm_player_mmr"):SetText('')
			GetWidget("mm_player_title"):SetText(" ")
			GetWidget('mm_player_ranklevel_root'):SetVisible(true)
		elseif (HoN_Matchmaking.selectedTab == "pvp") then
			if (mmr) then
				GetWidget("mm_player_mmr"):SetText("MMR: "..tostring(mmr))
			else
				GetWidget("mm_player_mmr"):SetText("")
			end
			GetWidget("mm_player_title"):SetText(" ")
		else
			GetWidget("mm_player_mmr"):SetText(Translate('mm3_bots_mode'))
			GetWidget("mm_player_title"):SetText(" ")
		end
	end

	--GetWidget("mm_player_verified"):SetVisible(UIIsVerified())
	--GetWidget("mm_player_mmr"):SetText("") 	-- MMR, empty for now
	GetWidget("mm_player_level"):SetText(interface:UICmd("GetAccountLevel(GetExperience())"))
	GetWidget("tmm_infopanel_stats_progress_1"):SetWidth((tonumber(interface:UICmd("GetAccountPercentNextLevel(GetExperience())"))*100).."%")
end

function HoN_Matchmaking:GetHeroPickIndexfromName(heroName)
	for i, bot in ipairs(HoN_Matchmaking.botPickTable) do
		if (bot.hero == heroName) then
			return i
		end
	end

	return nil
end

function HoN_Matchmaking:CreateBotPickTable(_botTable)
	if (_botTable) then 	-- a table was passed, overwrite the table we are using for bot info
		HoN_Matchmaking.botsTable = _botTable
	end

	HoN_Matchmaking.botPickTable = {}

	local heroAlreadyExists = {}
	local nextOffset = 1
	for i, bot in ipairs(HoN_Matchmaking.botsTable) do
		if (bot.sHeroName and bot.sHeroName ~= "") then 	-- don't pick up bots without a hero (DefaultTeamBotBrain)
			if (not heroAlreadyExists[bot.sHeroName]) then
				heroAlreadyExists[bot.sHeroName] = true

				local botInfoTable ={	["name"] = bot.sName,
										["desc"] = bot.sDescription,
										["version"] = "",
										["id"] = i,
										["used"] = false
									}

				HoN_Matchmaking.botPickTable[nextOffset] = {}
				HoN_Matchmaking.botPickTable[nextOffset].hero = bot.sHeroName
				HoN_Matchmaking.botPickTable[nextOffset].numBots = 1
				HoN_Matchmaking.botPickTable[nextOffset].bots = {}
				table.insert(HoN_Matchmaking.botPickTable[nextOffset].bots, botInfoTable)

				nextOffset = nextOffset + 1
			else
				-- another bot of an already existing her
				local offset = HoN_Matchmaking:GetHeroPickIndexfromName(bot.sHeroName)
				local botInfoTable ={	["name"] = bot.sName,
										["desc"] = bot.sDescription,
										["version"] = "",
										["id"] = i,
										["used"] = false
									}

				table.insert(HoN_Matchmaking.botPickTable[offset].bots, botInfoTable)
				HoN_Matchmaking.botPickTable[offset].numBots = HoN_Matchmaking.botPickTable[offset].numBots + 1
			end
		end
	end

	--sort the table by heroname
	local sortFunc = function(a,b) return (a.hero < b.hero) end
	table.sort(HoN_Matchmaking.botPickTable, sortFunc)
end

function HoN_Matchmaking:PopulateBotPickEntry(side, botPlateID, botPickIndex)
	local plateWidget = GetWidget("mm_botpickplate_"..side.."_"..botPlateID)
	if (botPickIndex and HoN_Matchmaking.botPickTable[botPickIndex]) then
		GetWidget("mm_botpickplate_icon_"..side.."_"..botPlateID):SetTexture(interface:UICmd("GetHeroIconPathFromProduct('"..HoN_Matchmaking.botPickTable[botPickIndex].hero.."')"))
		GetWidget("mm_botpickplate_index_"..side.."_"..botPlateID):SetText(tostring(botPickIndex))
		plateWidget:SetVisible(1)
	else
		plateWidget:SetVisible(0)
	end
end

function HoN_Matchmaking:PopulateRandomBotSlot(side, botPlateID)
	GetWidget("mm_botpickplate_icon_"..side.."_"..botPlateID):SetTexture("/ui/elements/question_mark.tga")
	GetWidget("mm_botpickplate_index_"..side.."_"..botPlateID):SetText("random")
	GetWidget("mm_botpickplate_"..side.."_"..botPlateID):SetVisible(1)
end

function HoN_Matchmaking:PopulateBotPickList(side, scrollOffset)
	if (not HoN_Matchmaking.botPickTable) then
		HoN_Matchmaking:CreateBotPickTable()
	end

	if (not scrollOffset) then
		scrollOffset = HoN_Matchmaking.botPickScrollOffsets[side]
	end

	-- eat up slots above used by bots
	local botOffset = 0
	if (scrollOffset ~= 0) then
		for i=1,(scrollOffset * 5) do
			if (botOffset ~= 0) then
				while (HoN_Matchmaking:CheckAllBotsUsed(botOffset)) do botOffset = botOffset + 1 end
			end
			botOffset = botOffset + 1
		end
	end

	for i=1, 15 do
		if (botOffset == 0) then -- offset 1 is always a random slot
			HoN_Matchmaking:PopulateRandomBotSlot(side, i)
			botOffset = botOffset + 1
		else
			while (HoN_Matchmaking:CheckAllBotsUsed(botOffset)) do botOffset = botOffset + 1 end

			HoN_Matchmaking:PopulateBotPickEntry(side, i, botOffset)
			botOffset = botOffset + 1
		end
	end

	HoN_Matchmaking:UpdateBotScrollbar(side)
end

function HoN_Matchmaking:PopulateEnemyslot(slotNumber)
	slotNumber = tonumber(slotNumber)
	if (slotNumber >= 1 and slotNumber <= 5) then
		if (not HoN_Matchmaking.lastHeroInfoLookUp.isValid or (HoN_Matchmaking.botsTable[HoN_Matchmaking.bots[slotNumber]].sHeroName ~= HoN_Matchmaking.lastHeroInfoLookUp.name)) then
			-- need to get request the hero info
			interface:UICmd("GetHeroInfo('"..HoN_Matchmaking.botsTable[HoN_Matchmaking.bots[slotNumber]].sHeroName.."');")
		end

		local slotWidget = GetWidget("mm_enemy_team_slot_"..slotNumber)

		GetWidget("mm_enemyteam_"..slotNumber.."_icon"):SetTexture(HoN_Matchmaking.lastHeroInfoLookUp.icon)
		GetWidget("mm_enemyteam_"..slotNumber.."_name"):SetText(HoN_Matchmaking.lastHeroInfoLookUp.name)
		GetWidget("mm_enemyteam_"..slotNumber.."_type"):SetTexture("/ui/fe2/lobby/"..HoN_Matchmaking.lastHeroInfoLookUp.type..".tga")
		GetWidget("mm_enemyteam_"..slotNumber.."_attacktype"):SetTexture("/ui/fe2/lobby/"..HoN_Matchmaking.lastHeroInfoLookUp.attacktype..".tga")

		for i=1, 4 do
			GetWidget("mm_enemyteam_"..slotNumber.."_ability"..i):SetTexture(HoN_Matchmaking.lastHeroInfoLookUp.abilities[i])
		end
	end
end

function HoN_Matchmaking:PopulateEnemyTeam()
	for i=1,5 do
		HoN_Matchmaking:PopulateEnemyslot(i)
	end
end

function HoN_Matchmaking:PopulateMultibotEntry(pickTableIndex, botIndex, slotNumber, side)
	if (HoN_Matchmaking.botPickTable[pickTableIndex].bots[botIndex]) then
		GetWidget("mm_multibotplate_"..side.."_"..slotNumber.."_icon"):SetTexture(interface:UICmd("GetHeroIconPathFromProduct('"..HoN_Matchmaking.botPickTable[pickTableIndex].hero.."')"))
		GetWidget("mm_multibotplate_"..side.."_"..slotNumber.."_name"):SetText(Translate(HoN_Matchmaking.botPickTable[pickTableIndex].bots[botIndex].name))
		GetWidget("mm_multibotplate_"..side.."_"..slotNumber.."_version"):SetText(HoN_Matchmaking.botPickTable[pickTableIndex].bots[botIndex].version)
		GetWidget("mm_multibotplate_"..side.."_"..slotNumber.."_desc"):SetText(Translate(HoN_Matchmaking.botPickTable[pickTableIndex].bots[botIndex].desc))
		GetWidget("mm_multibotpickplate_index_"..side.."_"..slotNumber):SetText(tostring(botIndex))
		GetWidget("mm_multibotplate_"..side.."_"..slotNumber):SetVisible(1)
	else
		GetWidget("mm_multibotplate_"..side.."_"..slotNumber):SetVisible(0)
	end
end

function HoN_Matchmaking:UpdateMultibotPicker(side)
	HoN_Matchmaking:MultibotPicker(HoN_Matchmaking.multibotPickOffset[side], side)
end

function HoN_Matchmaking:MultibotPicker(pickTableOffset, side)
	if (pickTableOffset) then
		if (not HoN_Matchmaking.lastHeroInfoLookUp.isValid or (HoN_Matchmaking.lastHeroInfoLookUp.hero ~= HoN_Matchmaking.botPickTable[pickTableOffset].hero)) then
			HoN_Matchmaking.lastHeroInfoLookUp.hero = HoN_Matchmaking.botPickTable[pickTableOffset].hero
			interface:UICmd("GetHeroInfo('"..HoN_Matchmaking.botPickTable[pickTableOffset].hero.."');")
		end
		GetWidget("hero_info"):SetVisible(1)

		HoN_Matchmaking.multibotPickOffset[side] = pickTableOffset

		local multibotOffset = 1 + HoN_Matchmaking.multibotPickScrollOffsets[side]
		for i=1, 4 do
			while (HoN_Matchmaking:CheckBotUsageByOffsets(pickTableOffset, multibotOffset)) do multibotOffset = multibotOffset + 1 end
			HoN_Matchmaking:PopulateMultibotEntry(pickTableOffset, multibotOffset, i, side)
			multibotOffset = multibotOffset + 1
		end

		GetWidget("mm_"..side.."_mp_hero"):SetText(Translate("mstore_"..HoN_Matchmaking.botPickTable[pickTableOffset].hero.."_name"))
		GetWidget("mm_repick_multibot_"..side):FadeIn(100)
	end
end

function CheckAndSwapEnemyBots(oldID, newID)
	if (ALLOW_DUPE_BOTS) then
		return
	end

	local heroCollision, botCollision = false, false
	local collisionID = nil

	local newHeroIndex, newBotIndex = HoN_Matchmaking:GetBotPickTableOffsetFromID(newID)
	for i,id in pairs(HoN_Matchmaking.bots) do
		heroIndex, subBotIndex = HoN_Matchmaking:GetBotPickTableOffsetFromID(id)

		if (heroIndex == newHeroIndex) then
			heroCollision = true
			collisionID = i
			if (subBotIndex == newBotIndex) then
				botCollision = true
				collisionID = i
				break
			end
		end
	end

	if (heroIndex and subBotIndex and collisionID) then
		-- swap in the bot being replaced
		if (botCollision or ((not ALLOW_DUPE_HERO) and heroCollision)) then
			HoN_Matchmaking.bots[collisionID] = oldID
			HoN_Matchmaking:PopulateEnemyslot(collisionID)
		end
	end
end

function HoN_Matchmaking:SelectMultiBot(self, botListPos, side)
	if not (HoN_Matchmaking.multibotPickOffset[side]) then return end
	local multibotOffset = tonumber(GetWidget("mm_multibotpickplate_index_"..side.."_"..botListPos):GetValue())
	local oldBot = nil

	-- mark the old bot unused
	if (side == "right") then
		oldBot = HoN_Matchmaking.bots[HoN_Matchmaking.repickingSlot[side]]
		HoN_Matchmaking:MarkBotUsageByID(HoN_Matchmaking.bots[HoN_Matchmaking.repickingSlot[side]], false)
	else
		oldBot = HoN_Matchmaking.teamMembers[HoN_Matchmaking.repickingSlot[side]].botID
		HoN_Matchmaking:MarkBotUsageByID(HoN_Matchmaking.teamMembers[HoN_Matchmaking.repickingSlot[side]].botID, false)
	end
	-- mark the new bot used
	HoN_Matchmaking:MarkBotUsageByOffsets(HoN_Matchmaking.multibotPickOffset[side], multibotOffset, true)

	if (side == "right") then
		GetWidget("mm_repick_enemy_bot"):SetVisible(0)
		HoN_Matchmaking.bots[HoN_Matchmaking.repickingSlot[side]] = HoN_Matchmaking.botPickTable[HoN_Matchmaking.multibotPickOffset[side]].bots[multibotOffset].id
		HoN_Matchmaking:PopulateEnemyslot(HoN_Matchmaking.repickingSlot[side])
		SetEnemyBot(HoN_Matchmaking.repickingSlot[side]-1, HoN_Matchmaking.botPickTable[HoN_Matchmaking.multibotPickOffset[side]].bots[multibotOffset].name)
	else
		GetWidget("mm_invite_bot"):SetVisible(0)
		HoN_Matchmaking.teamMembers[HoN_Matchmaking.repickingSlot[side]] = {}
		HoN_Matchmaking.teamMembers[HoN_Matchmaking.repickingSlot[side]].isBot = true
		HoN_Matchmaking.teamMembers[HoN_Matchmaking.repickingSlot[side]].botID = HoN_Matchmaking.botPickTable[HoN_Matchmaking.multibotPickOffset[side]].bots[multibotOffset].id
		HoN_Matchmaking:PopulateTeam()
		SetTeamBot(HoN_Matchmaking.repickingSlot[side]-1, HoN_Matchmaking.botPickTable[HoN_Matchmaking.multibotPickOffset[side]].bots[multibotOffset].name)
		if (randomEnemyBots and oldBot) then
			CheckAndSwapEnemyBots(oldBot, HoN_Matchmaking.botPickTable[HoN_Matchmaking.multibotPickOffset[side]].bots[multibotOffset].id)
		end
	end

	SaveBots()
	GetWidget("mm_repick_multibot_"..side):FadeOut(150)
	if (HoN_Matchmaking:CheckAllBotsUsed(HoN_Matchmaking.multibotPickOffset[side])) then -- all bots used, we need to repopulate as they shouldn't be listed anymore
		-- update the available bots, need to update both sides
		self:Sleep(125, function(...)
			HoN_Matchmaking:PopulateAllBotPickLists()
		end)
	end
	HoN_Matchmaking.repickingSlot[side] = nil
	HoN_Matchmaking.multibotPickOffset[side] = nil
end

function HoN_Matchmaking:PopulateAllBotPickLists()
	HoN_Matchmaking:PopulateBotPickList("left")
	HoN_Matchmaking:PopulateBotPickList("right")
end

function HoN_Matchmaking:SelectBot(self, botPlateID, side)
	if (not HoN_Matchmaking.repickingSlot[side]) then return end
	local botOffset = GetWidget("mm_botpickplate_index_"..side.."_"..botPlateID):GetValue()
	local oldBot = nil

	if (botOffset == "random") then
		-- make a table of all the available bots
		local botsAvailable = {}
		for i, t in pairs(HoN_Matchmaking.botPickTable) do
			local heroUsed = false
			if ((not ALLOW_DUPE_BOTS) and (not ALLOW_DUPE_HERO)) then
				for j=1, #t.bots do
					if HoN_Matchmaking:CheckBotUsageByOffsets(i, j) then
						heroUsed = true
					end
				end
			end

			if (not heroUsed) then
				for j=1, #t.bots do
					if not HoN_Matchmaking:CheckBotUsageByOffsets(i, j) then
						table.insert(botsAvailable, {i, j})
					end
				end
			end
		end

		-- mark the old bot unused
		if (side == "right") then
			oldBot = HoN_Matchmaking.bots[HoN_Matchmaking.repickingSlot[side]]
			HoN_Matchmaking:MarkBotUsageByID(HoN_Matchmaking.bots[HoN_Matchmaking.repickingSlot[side]], false)
		else
			oldBot = HoN_Matchmaking.teamMembers[HoN_Matchmaking.repickingSlot[side]].botID
			HoN_Matchmaking:MarkBotUsageByID(HoN_Matchmaking.teamMembers[HoN_Matchmaking.repickingSlot[side]].botID, false)
		end

		local randomBot = math.random(1, #botsAvailable)
		if (side == "right") then
			GetWidget("mm_repick_enemy_bot"):FadeOut(150)
			HoN_Matchmaking.bots[HoN_Matchmaking.repickingSlot[side]] = HoN_Matchmaking.botPickTable[botsAvailable[randomBot][1]].bots[botsAvailable[randomBot][2]].id
			HoN_Matchmaking:PopulateEnemyslot(HoN_Matchmaking.repickingSlot[side])
			SetEnemyBot(HoN_Matchmaking.repickingSlot[side]-1, HoN_Matchmaking.botPickTable[botsAvailable[randomBot][1]].bots[botsAvailable[randomBot][2]].name)
		else
			GetWidget("mm_invite_bot"):FadeOut(150)
			HoN_Matchmaking.teamMembers[HoN_Matchmaking.repickingSlot[side]] = {}
			HoN_Matchmaking.teamMembers[HoN_Matchmaking.repickingSlot[side]].isBot = true
			HoN_Matchmaking.teamMembers[HoN_Matchmaking.repickingSlot[side]].botID = HoN_Matchmaking.botPickTable[botsAvailable[randomBot][1]].bots[botsAvailable[randomBot][2]].id
			HoN_Matchmaking:PopulateTeam()
			SetTeamBot(HoN_Matchmaking.repickingSlot[side]-1, HoN_Matchmaking.botPickTable[botsAvailable[randomBot][1]].bots[botsAvailable[randomBot][2]].name)
			if (randomEnemyBots and oldBot) then
				CheckAndSwapEnemyBots(oldBot, HoN_Matchmaking.botPickTable[botsAvailable[randomBot][1]].bots[botsAvailable[randomBot][2]].id)
			end
		end
		-- mark bot used
		HoN_Matchmaking:MarkBotUsageByOffsets(botsAvailable[randomBot][1], botsAvailable[randomBot][2], true)
	else
		botOffset = tonumber(botOffset)

		if (HoN_Matchmaking.botPickTable[botOffset].numBots > 1) then -- multibot picker
			HoN_Matchmaking.multibotPickScrollOffsets[side] = 0
			HoN_Matchmaking:UpdateMultiBotScrollbar(side, botOffset)
			HoN_Matchmaking:MultibotPicker(botOffset, side)
			return
		else
			-- mark the old bot unused
			if (side == "right") then
				oldBot = HoN_Matchmaking.bots[HoN_Matchmaking.repickingSlot[side]]
				HoN_Matchmaking:MarkBotUsageByID(HoN_Matchmaking.bots[HoN_Matchmaking.repickingSlot[side]], false)
			else
				oldBot = HoN_Matchmaking.teamMembers[HoN_Matchmaking.repickingSlot[side]].botID
				HoN_Matchmaking:MarkBotUsageByID(HoN_Matchmaking.teamMembers[HoN_Matchmaking.repickingSlot[side]].botID, false)
			end
			-- mark the new bot used
			HoN_Matchmaking:MarkBotUsageByOffsets(botOffset, 1, true)

			if (side == "right") then
				GetWidget("mm_repick_enemy_bot"):FadeOut(150)
				HoN_Matchmaking.bots[HoN_Matchmaking.repickingSlot[side]] = HoN_Matchmaking.botPickTable[botOffset].bots[1].id
				HoN_Matchmaking:PopulateEnemyslot(HoN_Matchmaking.repickingSlot[side])
				SetEnemyBot(HoN_Matchmaking.repickingSlot[side]-1, HoN_Matchmaking.botPickTable[botOffset].bots[1].name)
			else
				GetWidget("mm_invite_bot"):FadeOut(150)
				HoN_Matchmaking.teamMembers[HoN_Matchmaking.repickingSlot[side]] = {}
				HoN_Matchmaking.teamMembers[HoN_Matchmaking.repickingSlot[side]].isBot = true
				HoN_Matchmaking.teamMembers[HoN_Matchmaking.repickingSlot[side]].botID = HoN_Matchmaking.botPickTable[botOffset].bots[1].id
				HoN_Matchmaking:PopulateTeam()
				SetTeamBot(HoN_Matchmaking.repickingSlot[side]-1, HoN_Matchmaking.botPickTable[botOffset].bots[1].name)
				if (randomEnemyBots and oldBot) then
					CheckAndSwapEnemyBots(oldBot, HoN_Matchmaking.botPickTable[botOffset].bots[1].id)
				end
			end
		end
	end
	SaveBots()
	HoN_Matchmaking.repickingSlot[side] = nil
	-- update the available bots, need to update both sides
	self:Sleep(125, function(...)
		HoN_Matchmaking:PopulateAllBotPickLists()
	end)
end

function HoN_Matchmaking:FillTeamWithBots()
	-- first attempt to fill the slots with prefered team bots, then ramdom pick the remaining
	local botOffset, slotOffset = 1, 2
	local savedTeamBots = GetDBEntry('mm_team_bots', nil, false, false, false) or {}

	while (botOffset <= 4 and slotOffset <= 5) do
		local bot = nil
		if (savedTeamBots[botOffset]) then
			bot = HoN_Matchmaking:GetBotFromName(savedTeamBots[botOffset])
		end
		if (not bot or bot.used) then
			 bot = HoN_Matchmaking:GetBotFromName(DefaultTeamBots[botOffset])
		end

		if (HoN_Matchmaking.teamMembers[slotOffset]) then
			slotOffset = slotOffset + 1
		elseif (bot.used) then
			botOffset = botOffset + 1
		else
			HoN_Matchmaking.teamMembers[slotOffset] = {}
			HoN_Matchmaking.teamMembers[slotOffset].isBot = true
			HoN_Matchmaking.teamMembers[slotOffset].botID = bot.id
			SetTeamBot(slotOffset-1, bot.name)
			HoN_Matchmaking:MarkBotUsageByID(bot.id, true)
			botOffset = botOffset + 1
			slotOffset = slotOffset + 1
		end
	end

	-- fill empty slots with the first unused bots in sequence
	botOffset = 1
	for i=2, 5 do
		if (not HoN_Matchmaking.teamMembers[i]) then -- empty slot
			while (HoN_Matchmaking:CheckBotUsageByOffsets(botOffset, 1)) do botOffset = botOffset + 1 end
			HoN_Matchmaking.teamMembers[i] = {}
			HoN_Matchmaking.teamMembers[i].isBot = true
			HoN_Matchmaking.teamMembers[i].botID = HoN_Matchmaking.botPickTable[botOffset].bots[1].id
			SetTeamBot(i-1, HoN_Matchmaking.botPickTable[botOffset].bots[1].name)
			HoN_Matchmaking:MarkBotUsageByOffsets(botOffset, 1, true)
			botOffset = botOffset + 1
		end
	end -- for the record, this should screw everything up if there are less bots than what is needed to fill the slots, lets not have that happen
	HoN_Matchmaking.teamHasBots = true
	HoN_Matchmaking:UpdateButtons()
	HoN_Matchmaking:PopulateAllBotPickLists() -- update the pick lists to remove the selected bots
end

------------------- These functions are for checking usage, these will all check the allow dups variable
------------------- ALL CHECKS should be ran through these, as then you don't need to worry about checking
------------------- the ALLOW_DUPE variable on your own elsewhere (basically, if allow dupe bots, don't worry about anything)
-- Note: ID is the offset into the botsTable (not the pickTable, but the pickTable bots have an ID member)
function HoN_Matchmaking:MarkBotUsageByID(id, used)
	if (ALLOW_DUPE_BOTS) then return end

	for i=1, #HoN_Matchmaking.botPickTable do
		for j=1, #HoN_Matchmaking.botPickTable[i].bots do
			if (HoN_Matchmaking.botPickTable[i].bots[j].id == id) then
				HoN_Matchmaking.botPickTable[i].bots[j].used = used
				break
			end
		end
	end
end

function HoN_Matchmaking:CheckBotUsageByID(id)
	if (ALLOW_DUPE_BOTS) then return false end

	for i=1, #HoN_Matchmaking.botPickTable do
		for j=1, #HoN_Matchmaking.botPickTable[i].bots do
			if (HoN_Matchmaking.botPickTable[i].bots[j].id == id) then
				return HoN_Matchmaking.botPickTable[i].bots[j].used
			end
		end
	end
end

function HoN_Matchmaking:MarkBotUsageByOffsets(tablePos, botNum, used)
	if (ALLOW_DUPE_BOTS) then return end
	if (not HoN_Matchmaking.botPickTable[tablePos] or not HoN_Matchmaking.botPickTable[tablePos].bots[botNum]) then return end

	HoN_Matchmaking.botPickTable[tablePos].bots[botNum].used = used
end

function HoN_Matchmaking:CheckBotUsageByOffsets(tablePos, botNum)
	if (ALLOW_DUPE_BOTS) then return false end
	if (not HoN_Matchmaking.botPickTable[tablePos] or not HoN_Matchmaking.botPickTable[tablePos].bots[botNum]) then return false end
	return HoN_Matchmaking.botPickTable[tablePos].bots[botNum].used
end

function HoN_Matchmaking:CheckAllBotsUsed(tablePos)
	if (ALLOW_DUPE_BOTS) then return false end
	if (not HoN_Matchmaking.botPickTable[tablePos]) then return false end

	local allUsed
	if (ALLOW_DUPE_HERO) then
		allUsed = true
		for i=1, #HoN_Matchmaking.botPickTable[tablePos].bots do
			if (not HoN_Matchmaking.botPickTable[tablePos].bots[i].used) then
				allUsed = false
				break
			end
		end
 	else
		allUsed = false
		for i=1, #HoN_Matchmaking.botPickTable[tablePos].bots do
			if (HoN_Matchmaking.botPickTable[tablePos].bots[i].used) then
				allUsed = true
				break
			end
		end
	end

	return allUsed
end
----------------------------- End of usage functions

function HoN_Matchmaking:GetBotPickTableOffsetFromID(id)
	for i=1, #HoN_Matchmaking.botPickTable do
		for j=1, #HoN_Matchmaking.botPickTable[i].bots do
			if (HoN_Matchmaking.botPickTable[i].bots[j].id == id) then
				return i, j
			end
		end
	end
end

function HoN_Matchmaking:HoverBot(self, botPlateID, side)
	-- fill out hero info for panel over chat, it will get displayed by the uiscript
	local botOffset = GetWidget("mm_botpickplate_index_"..side.."_"..botPlateID):GetValue()

	if (botOffset == "random") then
		-- hide the hero info
		GetWidget("hero_info"):SetVisible(0)
		-- display the hero name as the title
		GetWidget("mm_"..side.."_heroname"):SetText(Translate("mm3_random_bot"))
		GetWidget("mm_"..side.."_generic"):FadeOut(150)
		GetWidget("mm_"..side.."_title"):FadeOut(150)

		local nameWidget = GetWidget("mm_"..side.."_botname")
		local versionWidget = GetWidget("mm_"..side.."_version")
		local descWidget = GetWidget("mm_"..side.."_desc")

		nameWidget:SetText("???")
		versionWidget:SetText("")
		descWidget:SetText(Translate("mm3_random_desc"))

		GetWidget("mm_"..side.."_heroname"):SetVisible(0)
		nameWidget:SetVisible(0)
		versionWidget:SetVisible(0)
		descWidget:SetVisible(0)
		GetWidget("mm_"..side.."_heroname"):FadeIn(150)
		nameWidget:FadeIn(150)
		versionWidget:FadeIn(150)
		descWidget:FadeIn(150)
	else
		botOffset = tonumber(botOffset)

		if (not HoN_Matchmaking.lastHeroInfoLookUp.isValid or (HoN_Matchmaking.lastHeroInfoLookUp.hero ~= HoN_Matchmaking.botPickTable[botOffset].hero)) then
			HoN_Matchmaking.lastHeroInfoLookUp.hero = HoN_Matchmaking.botPickTable[botOffset].hero
			interface:UICmd("GetHeroInfo('"..HoN_Matchmaking.botPickTable[botOffset].hero.."');")
		end

		-- display the hero name as the title
		GetWidget("mm_"..side.."_heroname"):SetText(Translate("mstore_"..HoN_Matchmaking.botPickTable[botOffset].hero.."_name"))
		GetWidget("mm_"..side.."_generic"):FadeOut(150)
		GetWidget("mm_"..side.."_title"):FadeOut(150)

		-- show the bot info on hover
		if (HoN_Matchmaking.botPickTable[botOffset].numBots == 1) then 	-- display the info in place
			local nameWidget = GetWidget("mm_"..side.."_botname")
			local versionWidget = GetWidget("mm_"..side.."_version")
			local descWidget = GetWidget("mm_"..side.."_desc")

			nameWidget:SetText(Translate(HoN_Matchmaking.botPickTable[botOffset].bots[1].name))
			versionWidget:SetText(HoN_Matchmaking.botPickTable[botOffset].bots[1].version)
			descWidget:SetText(Translate(HoN_Matchmaking.botPickTable[botOffset].bots[1].desc))

			GetWidget("mm_"..side.."_heroname"):SetVisible(0)
			nameWidget:SetVisible(0)
			versionWidget:SetVisible(0)
			descWidget:SetVisible(0)
			GetWidget("mm_"..side.."_heroname"):FadeIn(150)
			nameWidget:FadeIn(150)
			versionWidget:FadeIn(150)
			descWidget:FadeIn(150)
		else 	-- there is more than one bot
			GetWidget("mm_"..side.."_version"):FadeOut(100)

			local nameWidget = GetWidget("mm_"..side.."_botname")
			local descWidget = GetWidget("mm_"..side.."_desc")

			nameWidget:SetText(Translate("mm3_coop_multiplebots", "count", HoN_Matchmaking.botPickTable[botOffset].numBots))
			descWidget:SetText(Translate("mm3_coop_multiplebots_desc"))

			GetWidget("mm_"..side.."_heroname"):SetVisible(0)
			nameWidget:SetVisible(0)
			descWidget:SetVisible(0)
			GetWidget("mm_"..side.."_heroname"):FadeIn(150)
			nameWidget:FadeIn(150)
			descWidget:FadeIn(150)
		end
	end
end

function HoN_Matchmaking:LeaveBotHover(side)
	GetWidget("mm_"..side.."_title"):FadeIn(150)
	GetWidget("mm_"..side.."_generic"):FadeIn(150)

	GetWidget("mm_"..side.."_heroname"):FadeOut(150)
	GetWidget("mm_"..side.."_botname"):FadeOut(150)
	GetWidget("mm_"..side.."_version"):FadeOut(150)
	GetWidget("mm_"..side.."_desc"):FadeOut(150)
end

function HoN_Matchmaking:KickTeamMember(self, slotNumber)
	slotNumber = tonumber(slotNumber)
	if (HoN_Matchmaking.teamMembers[slotNumber]) then
		if (not HoN_Matchmaking.teamMembers[slotNumber].isBot) then
			interface:UICmd("KickFromTMMGroup("..HoN_Matchmaking.teamMembers[slotNumber].slotNumber..");")
		else 	-- kicking a bot
			HoN_Matchmaking:MarkBotUsageByID(HoN_Matchmaking.teamMembers[slotNumber].botID, false)
			SetTeamBot(slotNumber-1, "")
			-- if (slotNumber ~= 5) then
			-- 	for i=slotNumber,4 do
			-- 		if (HoN_Matchmaking.teamMembers[i+1]) then
			-- 			HoN_Matchmaking.teamMembers[i] = HoN_Matchmaking.teamMembers[i+1]
			-- 			if (i == 4) then HoN_Matchmaking.teamMembers[5] = nil end
			-- 		else
			-- 			HoN_Matchmaking.teamMembers[i] = nil
			-- 		end
			-- 	end
		 -- 	else
		 -- 		HoN_Matchmaking.teamMembers[5] = nil
		 -- 	end
		 	HoN_Matchmaking:PopulateAllBotPickLists()
			HoN_Matchmaking:PopulateTeam()
		end
	end
end

function HoN_Matchmaking:RightClickPlayer(slotNumber)
	slotNumber = tonumber(slotNumber)
	if (HoN_Matchmaking.teamMembers[slotNumber] and not HoN_Matchmaking.teamMembers[slotNumber].isBot) then
		Set("ui_lastSelectedUser", HoN_Matchmaking.teamMembers[slotNumber].name)
		GetWidget("tmm_grouplist_rtclick"):DoEvent()
	end
end

function HoN_Matchmaking:ClickAutocomplete(self, plateID)
	plateID = tonumber(plateID)
	if (HoN_Matchmaking.invitePersonAutocompleteTable[plateID+HoN_Matchmaking.invitePersonScrollOffset]) then
		GetWidget("mm_autocomplete_textbox"):Sleep(235, function()
			GetWidget("mm_autocomplete_textbox"):UICmd("SetInputLine('"..HoN_Matchmaking.invitePersonAutocompleteTable[plateID+HoN_Matchmaking.invitePersonScrollOffset].."');")
		end)
	end
end

function HoN_Matchmaking:DoubleClickAutoComplete(self, plateID)
	plateID = tonumber(plateID)
	if (plateID) then
		local name = GetWidget("mm_autocomplete_plate_"..plateID.."_name"):GetValue()
		-- interrupt the existing sleep to fill the input box (from the clip) and instead invite the person and clear the input
		GetWidget("mm_autocomplete_textbox"):Sleep(1, function()
			HoN_Matchmaking:InvitePlayerName(self, name)
			GetWidget("mm_autocomplete_textbox"):EraseInputLine()
		end)
	end
end

function HoN_Matchmaking:StartSavingInvites()
	-- just gonna do a nil check to if we should save
	HoN_Matchmaking.invitedPersons = {}
end

function HoN_Matchmaking:DumpInvitedList()
	HoN_Matchmaking.invitedPersons = nil
end

function HoN_Matchmaking:DumpUserInfoTable()
	HoN_Matchmaking.userInfoTable = {}
end

function HoN_Matchmaking:BotListSlide(slider, value, side)
	value = tonumber(value)
	HoN_Matchmaking.botPickScrollOffsets[side] = value
	HoN_Matchmaking:PopulateAllBotPickLists()
end

function HoN_Matchmaking:UpdateBotScrollbar(side)
	local botCount = 1 -- one to account for random slot
	if (not ALLOW_DUPE_BOTS) then
		-- count number of bots to be displayed
		for i,hero in ipairs(HoN_Matchmaking.botPickTable) do
			local botUsed = false
			for j,bot in ipairs(hero.bots) do
				if (bot.used) then
					botUsed = true
					break
				end
			end

			if (not botUsed) then
				botCount = botCount + 1
			end
		end
	else
		botCount = #HoN_Matchmaking.botPickTable
	end

	rows = math.ceil(botCount / 5)
	local max = rows - 3
	if (max < 0) then max = 0 end
	local scrollbar = GetWidget("mm_"..side.."_bot_scrollbar")
	if (tonumber(scrollbar:GetValue()) > max) then
		scrollbar:SetValue(max) end
	scrollbar:SetMaxValue(max)
end

function HoN_Matchmaking:MultiBotListSlide(slider, value, side)
	value = tonumber(value)
	HoN_Matchmaking.multibotPickScrollOffsets[side] = value
	HoN_Matchmaking:UpdateMultibotPicker(side)
end

function HoN_Matchmaking:UpdateMultiBotScrollbar(side, botOffset)
	local numBots = HoN_Matchmaking.botPickTable[botOffset].numBots

	local max = numBots - 4
	if (max < 0) then max = 0 end
	local scrollbar = GetWidget("mm_"..side.."_multibot_scrollbar")
	if (tonumber(scrollbar:GetValue()) > max) then
		scrollbar:SetValue(max) end
	scrollbar:SetMaxValue(max)
end

function HoN_Matchmaking:RefreshRankedPlayInfo(type)
	local rankInfo = GetRankedPlayInfo(type)

	if GetCvarBool('cg_campaigndebug') then
		local param = explode(',', GetCvarString('cg_campaigndebug_string'))
		rankInfo.mmr = param[1] and tonumber(param[1]) or 1951
		rankInfo.level = param[2] and tonumber(param[2]) or 17	
		rankInfo.ranking = param[3] and tonumber(param[3]) or 9	
	end

	if (rankInfo.level > 0) then
		
		GetWidget('mm_player_ranklevel_icon'):SetTexture('/ui/fe2/season/icon_mini/'..GetRankIconNameRankLevelAfterS6(tonumber(rankInfo.level)))
		GetWidget('mm_season_options_rankicon'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevelAfterS6(tonumber(rankInfo.level)))

		
		GetWidget('mm_season_options_wins'):SetText(Translate('mm3_season_wins')..' ^999'..tostring(rankInfo.wins)..'^*')
		GetWidget('mm_season_options_losses'):SetText(Translate('mm3_season_losses')..' ^999'..tostring(rankInfo.losses)..'^*')
		if GetCvarBool('cl_GarenaEnable') then
			GetWidget('mm_season_options_winstreaks'):SetText(Translate('mm3_season_win_streaks')..' ^999'..tostring(rankInfo.winstreaks)..'^*')
		end

		local minMMR, maxMMR = GetMMRByRankLevelAfterS6(rankInfo.level)
		local currentMMR = rankInfo.mmr
		local debugStr = 'min:'..minMMR..', current:'..currentMMR..', max:'..maxMMR

		if GetCvarBool('cg_campaigndebug') then
			GetWidget('mm_season_options_debug'):SetText(debugStr)
		else
			GetWidget('mm_season_options_debug'):SetText('')
		end
		

		if IsMaxRankLevel(rankInfo.level) then
			GetWidget('mm_season_options_nextrank_root'):SetVisible(false)

			if rankInfo.ranking > 0 then 
				GetWidget('mm_season_options_rankname'):SetText(Translate('player_compaign_level_name_S7_'..tostring(rankInfo.level))..' '..tostring(rankInfo.ranking))
				GetWidget('mm_player_ranklevel_text'):SetText(Translate('player_compaign_level_name_S7_'..tostring(rankInfo.level))..' '..tostring(rankInfo.ranking))
			else
				GetWidget('mm_season_options_rankname'):SetText(Translate('player_compaign_level_name_S7_'..tostring(rankInfo.level)))
				GetWidget('mm_player_ranklevel_text'):SetText(Translate('player_compaign_level_name_S7_'..tostring(rankInfo.level)))
			end
				
			if currentMMR > maxMMR then
				GetWidget('mm_season_options_progressbar'):SetWidth('100%')
				GetWidget('mm_season_options_progressbar'):SetColor('#d8daef')
			else
				if currentMMR < minMMR then currentMMR = minMMR end
				if currentMMR > maxMMR then currentMMR = maxMMR end

				local rate = (currentMMR - minMMR) / (maxMMR - minMMR)
				GetWidget('mm_season_options_progressbar'):SetWidth(tostring(rate*100)..'%')
				GetWidget('mm_season_options_progressbar'):SetColor('#7e35c2')
			end
		else
			GetWidget('mm_season_options_rankname'):SetText(Translate('player_compaign_level_name_S7_'..tostring(rankInfo.level)))
			GetWidget('mm_player_ranklevel_text'):SetText(Translate('player_compaign_level_name_S7_'..tostring(rankInfo.level)))
			GetWidget('mm_season_options_nextrank_root'):SetVisible(true)
			GetWidget('mm_season_options_nextrank_icon'):SetTexture('/ui/fe2/season/icon_l/'..GetRankIconNameRankLevelAfterS6(rankInfo.level+1))
			GetWidget('mm_season_options_nextrank_tips'):SetCallback('onmouseover', function()
				Trigger('genericMainFloatingTip', 'true', '10h', '', '', 'player_compaign_level_name_S7_'..tostring(rankInfo.level+1), '', '', '3h', '-2h')
			end)

			if currentMMR < minMMR then currentMMR = minMMR end
			if currentMMR > maxMMR then currentMMR = maxMMR end

			local rate = (currentMMR - minMMR) / (maxMMR - minMMR)
			GetWidget('mm_season_options_progressbar'):SetWidth(tostring(rate*100)..'%')
			GetWidget('mm_season_options_progressbar'):SetColor('#7e35c2')
		end
	else
		GetWidget('mm_player_ranklevel_icon'):SetTexture('/ui/fe2/season/nolevel_mini.tga')
		GetWidget('mm_player_ranklevel_text'):SetText(Translate('player_compaign_level_name_0'))

		local matchnumber = rankInfo.placementMatches
		local placement_detail = {} 
		if NotEmpty(rankInfo.placementDetail) then
			for i=1, string.len(rankInfo.placementDetail) do
				table.insert(placement_detail, string.sub(rankInfo.placementDetail, i, i))
			end
		end

		for i=1,6 do
			if i <= matchnumber then
				GetWidget('placement_match_'..tostring(i)..'_icon'):SetVisible(true)
				GetWidget('placement_match_'..tostring(i)..'_text'):SetVisible(false)

				if Empty(placement_detail[i]) then
					GetWidget('placement_match_'..tostring(i)..'_line'):SetColor('#ffc600')
					GetWidget('placement_match_'..tostring(i)..'_icon'):SetTexture('/ui/fe2/season/dag.tga')
				elseif placement_detail[i] == '0' then
					GetWidget('placement_match_'..tostring(i)..'_line'):SetColor('#c50500')
					GetWidget('placement_match_'..tostring(i)..'_icon'):SetTexture('/ui/fe2/season/fail.tga')
				else
					GetWidget('placement_match_'..tostring(i)..'_line'):SetColor('#9be113')
					GetWidget('placement_match_'..tostring(i)..'_icon'):SetTexture('/ui/fe2/season/win.tga')
				end
			else
				GetWidget('placement_match_'..tostring(i)..'_line'):SetColor('#666f7c')
				GetWidget('placement_match_'..tostring(i)..'_icon'):SetVisible(false)
				GetWidget('placement_match_'..tostring(i)..'_text'):SetVisible(true)
			end
		end
	end

	GetWidget('mm_season_options_hasrank'):SetVisible(rankInfo.level > 0)
	GetWidget('mm_season_options_hasnorank'):SetVisible(rankInfo.level <= 0)
	GetWidget('season_not_eligible_mask'):SetVisible(HoN_Matchmaking.selectedTab == 'season' and not rankInfo.eligible)	
	GetWidget('mm_start_btn_effect'):SetVisible(HoN_Matchmaking.selectedTab ~= 'season' or not rankInfo.seasonend)
	GetWidget('mm_start_btn_seasonend'):SetVisible(HoN_Matchmaking.selectedTab == 'season' and rankInfo.seasonend)
end

-- ready up response function 
function HoN_Matchmaking:ReadyUpPoke()
	PlaySound('/shared/sounds/ui/tutorial/popup.ogg')

	-- only flash the button if they aren't ready
	if (groupLeaderID ~= UIGetAccountID()) then
		if (not HoN_Matchmaking.teamMembers[1].isReady) then
			local hl = GetWidget('mm_start_btn')
			-- mismatched sleeps so it doesn't completely disappear
			hl:FadeOut(100)
			hl:Sleep(75, function()
				hl:FadeIn(100)
				hl:Sleep(100, function()
					hl:FadeOut(100)
					hl:Sleep(75, function()
						hl:FadeIn(100)
					end)
				end)
			end)
		end
	end
end
triggerHelper:RegisterWatch("TMMRequestReadyUp", function() HoN_Matchmaking:ReadyUpPoke() end)

function HoN_Matchmaking:FailedAcceptGroupMM(names)
	-- explode the names!
	local pNames = explode(",", names)
	local self = false
	local others = false

	if (pNames) then
		for i,n in ipairs(pNames) do
			if (n == StripClanTag(GetAccountName())) then
				self = true
			else
				others = true
			end

			if (self and others) then
				break
			end
		end
	end

	local string = ""
	if (self and others) then
		string = Translate("mm_group_timeout_self_others").." "
		local first = true
		for i,n in ipairs(pNames) do
			if (n ~= StripClanTag(GetAccountName())) then
				if (first) then
					string = string .. n
					first = false
				else
					string = string .. ", " .. n
				end
			end
		end
		string = string .. "."
	elseif (others) then
		string = Translate("mm_group_timeout_others").." "
		local first = true
		for i,n in ipairs(pNames) do
			if (n ~= StripClanTag(GetAccountName())) then
				if (first) then
					string = string .. n
					first = false
				else
					string = string .. ", " .. n
				end
			end
		end
		string = string .. "."
	elseif (self) then
		string = "mm_group_timeout_self"
	else
		string = "mm_group_timeout_unknown"
	end

	-- dialog box
	Trigger("TriggerDialogBox", "mm_group_timeout", "", "general_ok", "", "", "mm_group_timeout", string)
end

triggerHelper:RegisterWatch("TMMMatchFailedToAccept", function(_, names) HoN_Matchmaking:FailedAcceptGroupMM(names) end)

local mapLoadGuides = {
	devowars			= {
		map			= {
			texture		= '/ui/elements/devowars/loading/map.tga',
			width		= '42h',
			height		= '42h',
			x			= '6.25h',
			y			= '1h',
		},
		info		= {
			y		= '5.25h',
			padding	= '1.9h',
			content	= {
				{ 'mapLoadGuideInfo', 'label', 'map_loading_devowars_tip01', 'width', '37h', 'fitypadding', '0' },
				{ 'mapLoadGuideInfos_devowars_abilities' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_devowars_tip02', 'width', '37h', 'fitypadding', '0' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_devowars_tip03', 'width', '37h', 'fitypadding', '0' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_devowars_tip04', 'width', '37h', 'fitypadding', '0' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_devowars_tip05', 'width', '37h', 'fitypadding', '0' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_devowars_tip06', 'width', '37h', 'fitypadding', '0' },
			},
		},
		legend		= {
			{ '/ui/elements/devowars/loading/icon_shop.tga', 'map_loading_devowars_shop' },
			{ '/ui/elements/devowars/loading/icon_grapple.tga', 'map_loading_devowars_ledge' },
			{ '/ui/elements/devowars/loading/icon_collision.tga', 'map_loading_devowars_midcollision' },
			{ '/ui/elements/devowars/loading/icon_runespawn.tga', 'map_loading_devowars_runespawn' },
		},
		markers		= {
			{ '24.8%',		'64.3%',	'/ui/elements/devowars/loading/icon_shop.tga' },
			{ '69.1%',		'29%',		'/ui/elements/devowars/loading/icon_shop.tga' },
			{ '23.3%',		'43.9%',	'/ui/elements/devowars/loading/icon_grapple.tga' },
			{ '37.4%',		'32.4%',	'/ui/elements/devowars/loading/icon_grapple.tga' },
			{ '65.9%',		'68.5%',	'/ui/elements/devowars/loading/icon_grapple.tga' },
			{ '79.9%',		'56.3%',	'/ui/elements/devowars/loading/icon_grapple.tga' },
			{ '48%',		'47.7%',	'/ui/elements/devowars/loading/icon_collision.tga' },
			{ '29.2%',		'37.9%',	'/ui/elements/devowars/loading/icon_runespawn.tga' },
			{ '73.2%',		'62.2%',	'/ui/elements/devowars/loading/icon_runespawn.tga' },
		},
	},
	capturetheflag		= {
		map			= {
			texture		= '/ui/elements/ctf/loading/map.tga',
		},
		info		= {
			content	= {
				{ 'mapLoadGuideInfo', 'label', 'map_loading_capturetheflag_tip01' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_capturetheflag_tip02', 'width', '34h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_capturetheflag_tip03', 'width', '33h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_capturetheflag_tip05', 'width', '31h' },
				{ 'mapLoadGuideInfos_capturetheflag_flagdrop' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_capturetheflag_tip04', 'width', '25h' },
			},
		},
		legend		= {
			{ '/ui/elements/ctf/loading/icon_flag_gold.tga', 'map_loading_capturetheflag_flaggold' },
			{ '/ui/elements/ctf/loading/icon_flag_blue.tga', 'map_loading_capturetheflag_flagblue' },
			{ '/ui/elements/ctf/loading/icon_well.tga', 'map_loading_capturetheflag_well' },
			{ '/ui/elements/ctf/loading/icon_tower.tga', 'map_loading_capturetheflag_tower' },
			{ '/ui/elements/ctf/loading/icon_respawn.tga', 'map_loading_capturetheflag_respawn' },
		},
		markers		= {
			{ '13.5%',		'55%',		'/ui/elements/ctf/loading/icon_well.tga' },
			{ '18.7%',		'66%',		'/ui/elements/ctf/loading/icon_respawn.tga' },
			{ '35.5%',		'66.9%',	'/ui/elements/ctf/loading/icon_tower.tga' },
			{ '34.6%',		'73.3%',	'/ui/elements/ctf/loading/icon_flag_blue.tga' },
			{ '66.2%',		'21.5%',	'/ui/elements/ctf/loading/icon_flag_gold.tga' },
			{ '66%',		'28.5%',	'/ui/elements/ctf/loading/icon_tower.tga' },
			{ '80.3%',		'26.3%',	'/ui/elements/ctf/loading/icon_respawn.tga' },
			{ '86.8%',		'35.5%',	'/ui/elements/ctf/loading/icon_well.tga' },
		},
	},
	team_deathmatch		= {
		map			= {
			texture		= '/ui/elements/team_deathmatch/loading/map.tga',
			width		= '40h',
			height		= '40h',
			x			= '7.1h',
			y			= '1.85h',
		},
		info		= {
			y		= '6.5h',
			content	= {
				{ 'mapLoadGuideInfo', 'label', 'map_loading_team_deathmatch_tip01', 'width', '42h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_team_deathmatch_tip02', 'width', '41h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_team_deathmatch_tip03', 'width', '40h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_team_deathmatch_tip04', 'width', '39h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_team_deathmatch_tip05', 'width', '38h' },
			},
		},
		legend		= {
			{ '/ui/elements/caldavar/loading/icon_base.tga', 'map_loading_team_deathmatch_base' },
		},
		markers		= {
			{ '15.7%',		'38.8%',	'/ui/elements/caldavar/loading/icon_base.tga' },
			{ '84.8%',		'52.0%',	'/ui/elements/caldavar/loading/icon_base.tga' },
		},
	},
	grimmscrossing    = {
		map			= {
			texture			= '/ui/elements/grimmscrossing/loading/map.tga',
		},
		info		= {
			content	= {
				{ 'mapLoadGuideInfo', 'label', 'map_loading_grimmscrossing_tip01', 'width', '37h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_grimmscrossing_tip02', 'width', '34h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_grimmscrossing_tip03', 'width', '32h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_grimmscrossing_tip04', 'width', '29h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_grimmscrossing_tip05', 'width', '30h' },
			},
	
		},
		legend		= {
			{ '/ui/elements/grimmscrossing/loading/icon_well.tga', 'map_loading_grimmscrossing_fountain' },
			{ '/ui/elements/grimmscrossing/loading/icon_spawner.tga', 'map_loading_grimmscrossing_teleporter' },
			{ '/ui/elements/grimmscrossing/loading/icon_rune.tga', 'map_loading_grimmscrossing_rune' },
			{ '/ui/elements/grimmscrossing/loading/icon_tower.tga', 'map_loading_grimmscrossing_tower' },
			{ '/ui/elements/grimmscrossing/loading/icon_house.tga', 'map_loading_grimmscrossing_base' },
		},
		markers		= {
			{ '17.1%',	'69.8%',	'/ui/elements/grimmscrossing/loading/icon_well.tga' },
			{ '80.3%',	'20.6%',	'/ui/elements/grimmscrossing/loading/icon_well.tga' },
			{ '27.1%',	'63.1%',	'/ui/elements/grimmscrossing/loading/icon_house.tga' },
			{ '74.5%',	'25%',		'/ui/elements/grimmscrossing/loading/icon_house.tga' },
			{ '44.6%',	'47.1%',	'/ui/elements/grimmscrossing/loading/icon_spawner.tga' },
			{ '57.9%',	'37.2%',	'/ui/elements/grimmscrossing/loading/icon_spawner.tga' },
			{ '21.6%',	'41.9%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '19.5%',	'56.8%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '29.7%',	'58.1%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '32.7%',	'69.1%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '54.5%',	'66.4%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '47.3%',	'21.4%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '64.2%',	'18.8%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '71.3%',	'26.3%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '85.7%',	'29.1%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '82.1%',	'46.3%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '51%',	'42.2%',	'/ui/elements/grimmscrossing/loading/icon_rune.tga' },
		},
	},
	caldavar	= {
		map			= {
			texture		= '/ui/elements/caldavar/loading/map.tga',
			width		= '40h',
			height		= '40h',
			x			= '7.5h',
			y			= '1.5h',
		},
		info		= {
			y		= '6.5h',
			content	= {
				{ 'mapLoadGuideInfo', 'label', 'map_loading_caldavar_tip01', 'width', '42h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_caldavar_tip02', 'width', '41h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_caldavar_tip03', 'width', '40h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_caldavar_tip04', 'width', '39h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_caldavar_tip05', 'width', '38h' },
			},
		},
		legend		= {
			{ '/ui/elements/caldavar/loading/icon_base.tga',	'map_loading_caldavar_base' },
			{ '/ui/elements/caldavar/loading/icon_boss.tga',	'map_loading_caldavar_boss' },
			{ '/ui/elements/caldavar/loading/icon_rune.tga',	'map_loading_caldavar_rune' },
			{ '/ui/elements/caldavar/loading/icon_tower.tga',	'map_loading_caldavar_tower' },
			{ '/ui/elements/caldavar/loading/icon_well.tga',	'map_loading_caldavar_well' },
		},
		markers		= {
			{ '12.2%',		'83.3%',	'/ui/elements/caldavar/loading/icon_well.tga' },
			{ '78.0%',		'18.1%',	'/ui/elements/caldavar/loading/icon_well.tga' },
			{ '19.0%',		'77.8%',	'/ui/elements/caldavar/loading/icon_base.tga' },
			{ '73.0%',		'23.6%',	'/ui/elements/caldavar/loading/icon_base.tga' },
			{ '14.6%',		'62.8%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
			{ '26.4%',		'68.3%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
			{ '30.4%',		'83.1%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
			{ '18.1%',		'47.9%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
			{ '30.2%',		'60.0%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
			{ '47.5%',		'81.1%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
			{ '19.9%',		'34.0%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
			{ '40.3%',		'50.1%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
			{ '80.9%',		'81.8%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
			{ '29.8%',		'15.8%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
			{ '54.5%',		'40.3%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
			{ '84.4%',		'52.4%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
			{ '52.0%',		'15.9%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
			{ '64.0%',		'34.4%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
			{ '81.8%',		'29.9%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
			{ '64.4%',		'16.5%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
			{ '69.8%',		'26.2%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
			{ '81.8%',		'29.9%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
			{ '83.0%',		'42.0%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
			{ '66.5%',		'56.3%',	'/ui/elements/caldavar/loading/icon_boss.tga' },
			{ '40.7%',		'36.8%',	'/ui/elements/caldavar/loading/icon_rune.tga' },
			{ '60.0%',		'52.0%',	'/ui/elements/caldavar/loading/icon_rune.tga' },
		},
	},
	riftwars	= {
		map			= {
			texture			= '/ui/elements/riftwars/loading/map.tga',
			x				= '2.75h',
			y				= '-3.25h',
		},
		info		= {
			y		= '5.25h',
			content	= {
				{ 'mapLoadGuideInfo', 'label', 'map_loading_riftwars_tip01', 'width', '37h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_riftwars_tip02', 'width', '34h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_riftwars_tip03', 'width', '32h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_riftwars_tip04', 'width', '29h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_riftwars_tip05', 'width', '30h' },
			},
	
		},
		legend		= {
			{ '/ui/elements/grimmscrossing/loading/icon_well.tga',		'map_loading_grimmscrossing_fountain' },
			{ '/ui/elements/grimmscrossing/loading/icon_spawner.tga',	'map_loading_grimmscrossing_teleporter' },
			{ '/ui/elements/grimmscrossing/loading/icon_rune.tga',		'map_loading_grimmscrossing_rune' },
			{ '/ui/elements/grimmscrossing/loading/icon_tower.tga',		'map_loading_grimmscrossing_tower' },
			{ '/ui/elements/grimmscrossing/loading/icon_house.tga',		'map_loading_grimmscrossing_base' },
		},
		markers		= {
			{ '15.0%',	'74.4%',	'/ui/elements/grimmscrossing/loading/icon_well.tga' },
			{ '79.0%',	'24.0%',	'/ui/elements/grimmscrossing/loading/icon_well.tga' },
			{ '22.7%',	'65.6%',	'/ui/elements/grimmscrossing/loading/icon_house.tga' },
			{ '74.3%',	'27.6%',	'/ui/elements/grimmscrossing/loading/icon_house.tga' },
			{ '43.6%',	'48.6%',	'/ui/elements/grimmscrossing/loading/icon_spawner.tga' },
			{ '58.0%',	'37.8%',	'/ui/elements/grimmscrossing/loading/icon_spawner.tga' },
			{ '19.6%',	'44.0%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '16.2%',	'57.9%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '27.5%',	'62.5%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '33.8%',	'73.0%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '56.3%',	'70.0%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '58.8%',	'25.3%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '63.0%',	'23.8%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '71.3%',	'29.6%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '83.8%',	'33.1%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '84.5%',	'44.7%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '50.6%',	'44.0%',	'/ui/elements/grimmscrossing/loading/icon_rune.tga' },
		},
	},
	midwars	= {
		map			= {
			texture			= '/ui/elements/midwars/loading/map.tga',
			width			= '43h',
			height			= '43h',
			x				= '6h',
			y				= '0',
		},
		info		= {
			y		= '9h',
			content	= {
				{ 'mapLoadGuideInfo', 'label', 'map_loading_midwars_tip01', 'width', '33h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_midwars_tip02', 'width', '34h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_midwars_tip03', 'width', '35h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_midwars_tip04', 'width', '36h' },
			},
	
		},
		legend		= {
			{ '/ui/elements/grimmscrossing/loading/icon_house.tga',		'map_loading_caldavar_base' },
			{ '/ui/elements/caldavar/loading/icon_boss.tga',			'map_loading_caldavar_boss' },
			{ '/ui/elements/grimmscrossing/loading/icon_rune.tga',		'map_loading_caldavar_rune' },
			{ '/ui/elements/grimmscrossing/loading/icon_tower.tga',		'map_loading_caldavar_tower' },
			{ '/ui/elements/grimmscrossing/loading/icon_well.tga',		'map_loading_caldavar_well' },
			{ '/ui/elements/grimmscrossing/loading/icon_spawner.tga',	'map_loading_grimmscrossing_teleporter' },
		},
		markers		= {
			{ '28.9%',	'66.8%',	'/ui/elements/grimmscrossing/loading/icon_house.tga' },
			{ '64.0%',	'23.8%',	'/ui/elements/grimmscrossing/loading/icon_house.tga' },
			{ '21.4%',	'55.5%',	'/ui/elements/grimmscrossing/loading/icon_spawner.tga' },
			{ '25.7%',	'50.8%',	'/ui/elements/grimmscrossing/loading/icon_spawner.tga' },
			{ '46.6%',	'27.5%',	'/ui/elements/grimmscrossing/loading/icon_spawner.tga' },
			{ '50.1%',	'22.5%',	'/ui/elements/grimmscrossing/loading/icon_spawner.tga' },
			{ '72.4%',	'54.1%',	'/ui/elements/caldavar/loading/icon_boss.tga' },
			{ '17.1%',	'53.9%',	'/ui/elements/grimmscrossing/loading/icon_well.tga' },
			{ '47.5%',	'20.0%',	'/ui/elements/grimmscrossing/loading/icon_well.tga' },
			{ '43.3%',	'43.9%',	'/ui/elements/grimmscrossing/loading/icon_rune.tga' },
			{ '30.9%',	'62.8%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '41.2%',	'51.1%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '51.3%',	'36.7%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '61.1%',	'27.1%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
		},
	},
	soccer		= {
		map			= {
			texture		= '/ui/elements/soccer/loading/map.tga',
		},
		info		= {
			content	= {
				{ 'mapLoadGuideInfo', 'label', 'map_loading_soccer_tip01' },
				{ 'mapLoadGuideInfos_soccer_abilities' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_soccer_tip02' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_soccer_tip03', 'width', '36h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_soccer_tip04', 'width', '34h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_soccer_tip05', 'width', '36h' },
			},
		},
		legend		= {
			{ '/ui/elements/soccer/loading/icon_start.tga', 'map_loading_soccer_start' },
			{ '/ui/elements/soccer/loading/icon_goals.tga', 'map_loading_soccer_goals' },
			{ '/ui/elements/soccer/loading/icon_rune.tga', 'map_loading_soccer_rune' },
			{ '/ui/icons/invis.tga', '' },
		},
		markers		= {
			{ '51%',		'46%',		'/ui/elements/soccer/loading/icon_start.tga' },
			{ '27%',		'62%',		'/ui/elements/soccer/loading/icon_goals.tga' },
			{ '71%',		'31%',		'/ui/elements/soccer/loading/icon_goals.tga' },
			{ '32%',		'49%',		'/ui/elements/soccer/loading/icon_rune.tga' },
			{ '43%',		'63%',		'/ui/elements/soccer/loading/icon_rune.tga' },
			{ '69%',		'43%',		'/ui/elements/soccer/loading/icon_rune.tga' },
			{ '57.5%',		'31%',		'/ui/elements/soccer/loading/icon_rune.tga' },
		},
	},
	solomap	= {
		map			= {
			texture		= '/ui/elements/caldavar/loading/map.tga',
			width		= '40h',
			height		= '40h',
			x			= '7.5h',
			y			= '1.5h',
		},
		info		= {
			y		= '6.5h',
			content	= {
				{ 'mapLoadGuideInfo', 'label', 'map_loading_solomap_tip01', 'width', '42h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_solomap_tip02', 'width', '41h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_solomap_tip03', 'width', '40h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_solomap_tip04', 'width', '39h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_solomap_tip05', 'width', '38h' },
			},
		},
		legend		= {
			{ '/ui/elements/caldavar/loading/icon_tower.tga',	'map_loading_caldavar_tower' },
			{ '/ui/elements/caldavar/loading/icon_well.tga',	'map_loading_caldavar_well' },
		},
		markers		= {
			{ '12.2%',		'83.3%',	'/ui/elements/caldavar/loading/icon_well.tga' },
			{ '78.0%',		'18.1%',	'/ui/elements/caldavar/loading/icon_well.tga' },
			{ '40.3%',		'50.1%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
			{ '54.5%',		'40.3%',	'/ui/elements/caldavar/loading/icon_tower.tga' },
		},
	},
	midwarsbeta	= {
		map			= {
			texture			= '/ui/elements/midwars/loading/map.tga',
			width			= '43h',
			height			= '43h',
			x				= '6h',
			y				= '0',
		},
		info		= {
			y		= '9h',
			content	= {
				{ 'mapLoadGuideInfo', 'label', 'map_loading_midwars_beta_tip01', 'width', '33h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_midwars_beta_tip02', 'width', '34h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_midwars_beta_tip03', 'width', '35h' },
				{ 'mapLoadGuideInfo', 'label', 'map_loading_midwars_beta_tip04', 'width', '36h' },
			},
	
		},
		legend		= {
			{ '/ui/elements/grimmscrossing/loading/icon_house.tga',		'map_loading_caldavar_base' },
			{ '/ui/elements/caldavar/loading/icon_boss.tga',			'map_loading_caldavar_boss' },
			{ '/ui/elements/grimmscrossing/loading/icon_rune.tga',		'map_loading_caldavar_rune' },
			{ '/ui/elements/grimmscrossing/loading/icon_tower.tga',		'map_loading_caldavar_tower' },
			{ '/ui/elements/grimmscrossing/loading/icon_well.tga',		'map_loading_caldavar_well' },
			{ '/ui/elements/grimmscrossing/loading/icon_spawner.tga',	'map_loading_grimmscrossing_teleporter' },
		},
		markers		= {
			{ '28.9%',	'66.8%',	'/ui/elements/grimmscrossing/loading/icon_house.tga' },
			{ '64.0%',	'23.8%',	'/ui/elements/grimmscrossing/loading/icon_house.tga' },
			{ '21.4%',	'55.5%',	'/ui/elements/grimmscrossing/loading/icon_spawner.tga' },
			{ '25.7%',	'50.8%',	'/ui/elements/grimmscrossing/loading/icon_spawner.tga' },
			{ '46.6%',	'27.5%',	'/ui/elements/grimmscrossing/loading/icon_spawner.tga' },
			{ '50.1%',	'22.5%',	'/ui/elements/grimmscrossing/loading/icon_spawner.tga' },
			{ '72.4%',	'54.1%',	'/ui/elements/caldavar/loading/icon_boss.tga' },
			{ '17.1%',	'53.9%',	'/ui/elements/grimmscrossing/loading/icon_well.tga' },
			{ '47.5%',	'20.0%',	'/ui/elements/grimmscrossing/loading/icon_well.tga' },
			{ '43.3%',	'43.9%',	'/ui/elements/grimmscrossing/loading/icon_rune.tga' },
			{ '30.9%',	'62.8%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '41.2%',	'51.1%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '51.3%',	'36.7%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
			{ '61.1%',	'27.1%',	'/ui/elements/grimmscrossing/loading/icon_tower.tga' },
		},
	},
}

function matchmakingInitializeMapGuides()
	-- mm_start_btn
	-- matchmaking_queue_display

	local function isValidGuideMap(mapName)
		return ( mapLoadGuides[mapName] ~= nil )
	end

	local selectedMap = ''
	local lastAllReady		= false
	local lastValidStart	= false	-- This isn't cleared described

	function mapLoadGuideVis(checkMap, force)
		local isInQueue = AtoB(GetWidget('matchmaking_queue_display'):UICmd("IsInQueue"))

		checkMap = checkMap or selectedMap
		force = force or false

		if isInQueue or force then
			if isValidGuideMap(checkMap) then
				GetWidget('matchmaking_queue_display'):SetX('40h')
				GetWidget('mapLoadGuide'):SetVisible(true)
				-- GetWidget('mapLoadGuideMarkers'):ClearChildren()
				GetWidget('matchmaking_queue_display'):SetVisible(false)
				return true
			else
				GetWidget('matchmaking_queue_display'):SetX(0)
				GetWidget('mapLoadGuide'):SetVisible(false)
				GetWidget('matchmaking_queue_display'):SetVisible(true)
			end
		else
			GetWidget('matchmaking_queue_display'):SetX(0)
			
			GetWidget('matchmaking_queue_display'):SetVisible(false)

			if (force or (lastAllReady and (not lastValidStart))) and isValidGuideMap(checkMap) then
				GetWidget('mapLoadGuide'):SetVisible(true)
				return true
			else
				GetWidget('mapLoadGuide'):SetVisible(false)
			end
		end

		return false
	end

	GetWidget('mapLoadGuide'):RegisterWatch('TMMReset', mapLoadGuideVis)
	-- [HONG-2126] make loading screen display both FAQ and matchmaking queue.
	-- GetWidget('mapLoadGuide'):RegisterWatch('TMMTime', mapLoadGuideVis)
	GetWidget('mapLoadGuide'):RegisterWatch('TMMLeftQueue', mapLoadGuideVis)

	local function assetLoadingVis(useMap, forceGuide)
		useMap = useMap or selectedMap
		forceGuide = forceGuide or false

		if false or (lastAllReady and (not lastValidStart)) then
			if isValidGuideMap(useMap) then
				GetWidget('mm_loading_1', 'main'):SetVisible(false)
				GetWidget('mapLoadGuide_mm_loading_1'):SetVisible(true)
			else
				GetWidget('mm_loading_1', 'main'):SetVisible(true)
				GetWidget('mapLoadGuide_mm_loading_1'):SetVisible(false)
			end
		else
			GetWidget('mm_loading_1', 'main'):SetVisible(false)
			GetWidget('mapLoadGuide_mm_loading_1'):SetVisible(false)
		end
	end

	function mapLoadGuideDisplay(useMap, force)
		force = force or false
		useMap = useMap or selectedMap

		mapLoadGuideVis(useMap, force)
		assetLoadingVis(useMap, force)

		local guideInfos		= GetWidget('mapLoadGuideInfos')
		local showInfos			= false

		local guideLegend		= GetWidget('mapLoadGuideLegend')
		local showLegend		= false

		local guideMarkers		= GetWidget('mapLoadGuideMarkers')
		local showMarkers		= false

		local guideMap			= GetWidget('mapLoadGuideMap')
		local showMap			= false

		if mapLoadGuides[useMap] then
			local currentGuide	= mapLoadGuides[useMap]

			guideMap:SetTexture(currentGuide.map.texture)
			guideMap:SetX(currentGuide.map.x or '0')
			guideMap:SetY(currentGuide.map.y or '-2h')
			guideMap:SetWidth(currentGuide.map.width or '48h')
			guideMap:SetHeight(currentGuide.map.height or '48h')
			showMap = true


			if currentGuide.info then
				showInfos	= true
				guideInfos:ClearChildren()
				guideInfos:SetX(currentGuide.info.x or '3.25h')
				guideInfos:SetY(currentGuide.info.y or '6.5h')
				-- guideInfos:SetPadding(currentGuide.info.padding or '1.85h')

				for k,v in ipairs(currentGuide.info.content) do
					guideInfos:Instantiate(unpack(v))
				end
			end

			if currentGuide.legend then
				showLegend = true
				guideLegend:ClearChildren()
				
				for k,v in ipairs(currentGuide.legend) do
					guideLegend:Instantiate('mapLoadGuideLegendMarker', 'icon', v[1], 'label', v[2])
				end
			end

			if currentGuide.markers then
				showMarkers = true
				guideMarkers:ClearChildren()

				for k,v in ipairs(currentGuide.markers) do
					guideMarkers:Instantiate('mapLoadGuideMarker', 'x', v[1], 'y', v[2], 'icon', v[3])
				end
			end
		end


		guideInfos:SetVisible(showInfos)
		guideLegend:SetVisible(showLegend)
		guideMarkers:SetVisible(showMarkers)
		guideMap:SetVisible(showMap)

		GetWidget('mapLoadGuideHeaderGlow'):SetText(Translate('map_'..useMap))
		GetWidget('mapLoadGuideHeaderText'):SetText(Translate('map_'..useMap))

	end

	GetWidget('matchmaking_queue_display'):RegisterWatch('TMMDisplay', function(widget, ...)
		selectedMap = arg[31]

		mapLoadGuideDisplay(selectedMap, nil)

	end)

	GetWidget('mapLoadGuide'):RegisterWatch('TMMReadyStatus', function(widget, isGroupLeader, otherPlayersReady, allPlayersReady, selfIsReady, validStartTime)
		lastAllReady = AtoB(allPlayersReady)
		lastValidStart = AtoB(validStartTime)
		mapLoadGuideVis()
		assetLoadingVis()
	end)

	local assetLoadingPlayers = {}


	local function populateAssetLoadingPlayer(index, exists, loadPercent, playerName)
		if exists then
			GetWidget('mapLoadGuideLoadProgress'..index..'Bar'):SetWidth(loadPercent .. '%')
			GetWidget('mapLoadGuideLoadProgress'..index..'Name'):SetText(playerName)
			GetWidget('mapLoadGuideLoadProgress'..index):SetVisible(true)
		else
			GetWidget('mapLoadGuideLoadProgress'..index):SetVisible(false)
		end
	end

	local function processAssetLoadingPlayers()
		local alliedPlayerIndex = 0

		for i=0,4,1 do
			if assetLoadingPlayers[i].accountID == Client.GetAccountID() then
				populateAssetLoadingPlayer(
					0,
					assetLoadingPlayers[i].exists,
					assetLoadingPlayers[i].loadPercent,
					assetLoadingPlayers[i].playerName
				)
			else
				alliedPlayerIndex = alliedPlayerIndex + 1

				if alliedPlayerIndex <= 4 then

					populateAssetLoadingPlayer(
						alliedPlayerIndex,
						assetLoadingPlayers[i].exists,
						assetLoadingPlayers[i].loadPercent,
						assetLoadingPlayers[i].playerName
					)
				end
			end
		end
	end

	for i=0,4,1 do
		assetLoadingPlayers[i] = {
			exists		= false,
			accountID	= -1,
			playerName	= ''
		}

		GetWidget('mapLoadGuide'):RegisterWatch('TMMPlayerStatus'..i, function(widget, ...)
			assetLoadingPlayers[i].exists			= (string.len(arg[1]) > 0)
			if assetLoadingPlayers[i].exists then
				assetLoadingPlayers[i].accountID	= tonumber(arg[1])
			else
				assetLoadingPlayers[i].accountID	= -1
			end

			assetLoadingPlayers[i].playerName	= arg[2]
			assetLoadingPlayers[i].loadPercent	= arg[5]		-- 0-100, not just 0-1
			processAssetLoadingPlayers()
		end)

	end

end