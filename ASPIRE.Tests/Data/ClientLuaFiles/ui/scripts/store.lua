----------------------------------------------------------
--	Name: 		Store Script	            			--
--  Copyright 2015 Frostburn Studios					--
----------------------------------------------------------

local collectorsEditionSetImages = {
	"/ui/fe2/store/avatars_display/ico_7collection.tga",
	"/ui/fe2/store/avatars_display/ico_riseofra.tga",
	"/ui/fe2/store/avatars_display/ico_blacklegion.tga",
	"/ui/fe2/store/avatars_display/ico_neutrals.tga",
	"/ui/fe2/store/avatars_display/ico_rift.tga",
	"/ui/fe2/store/avatars_display/ico_hunters.tga",
	"/ui/fe2/store/avatars_display/ico_horsemen.tga",
	"/ui/fe2/store/avatars_display/ico_gods.tga",
	"/ui/fe2/store/avatars_display/ico_convention.tga",
	"/ui/fe2/store/avatars_display/ico_community.tga",
	"/ui/fe2/store/avatars_display/ico_anniversary.tga",
	"/ui/fe2/store/avatars_display/ico_marsh.tga",
	"/ui/fe2/store/avatars_display/ico_clockworks.tga",
	"/ui/fe2/store/avatars_display/ico_debut.tga",
	"/ui/fe2/store/avatars_display/ico_shangla.tga",
	"/ui/fe2/store/avatars_display/ico_fairytale.tga",
	"/ui/fe2/store/avatars_display/ico_evilgods.tga",
	"/ui/fe2/store/avatars_display/ico_revenge_neutrals.tga",
	"/ui/fe2/store/avatars_display/ico_legion_bots.tga",
	"/ui/fe2/store/avatars_display/ico_hellbourne_bots.tga",
	"/ui/fe2/store/avatars_display/ico_blinddisciple.tga",
	"/ui/fe2/store/avatars_display/ico_candy_world.tga",
	"/ui/fe2/store/avatars_display/ico_xmas_party.tga",
	"/ui/fe2/store/avatars_display/ico_heavenly_virtues.tga",
	"/ui/fe2/store/avatars_display/ico_scorched.tga",
	"/ui/fe2/store/avatars_display/ico_guardians.tga",
	"/ui/fe2/store/avatars_display/ico_bloodtide.tga",
	"/ui/fe2/store/avatars_display/ico_shroud.tga",
	"/ui/fe2/store/avatars_display/ico_ursa.tga",
	"/ui/fe2/store/avatars_display/ico_zodiac.tga",
	"/ui/fe2/store/avatars_display/ico_item.tga",
	"/ui/fe2/store/avatars_display/ico_halloween.tga",
	"/ui/fe2/store/avatars_display/ico_judge.tga",
	"/ui/fe2/store/avatars_display/ico_deadeye.tga",
	"/ui/fe2/store/avatars_display/ico_arms.tga",
	"/ui/fe2/store/avatars_display/ico_wareffort.tga",
	"/ui/fe2/store/avatars_display/ico_blackwal.tga",
	"/ui/fe2/store/avatars_display/ico_8bit.tga",
	"/ui/fe2/store/avatars_display/ico_paragon.tga",
	"/ui/fe2/store/avatars_display/ico_upgrade.tga",
	"/ui/fe2/store/avatars_display/ico_hotshot.tga",
	"/ui/fe2/store/avatars_display/ico_scar.tga",
	"/ui/fe2/store/avatars_display/ico_siam.tga",
	"/ui/fe2/store/avatars_display/ico_steamworks.tga",
	"/ui/fe2/store/avatars_display/ico_regional.tga",
	"/ui/fe2/store/avatars_display/ico_rifthunter.tga",
	"/ui/fe2/store/avatars_display/ico_songkran.tga",
	"/ui/fe2/store/avatars_display/ico_ascension.tga",
	"/ui/fe2/store/avatars_display/ico_apex.tga",
	"/ui/fe2/store/avatars_display/ico_soccer.tga",
	"/ui/fe2/store/avatars_display/ico_knockout.tga",
	"/ui/fe2/store/avatars_display/ico_sanguo.tga",
	"/ui/fe2/store/avatars_display/ico_cyber.tga",
	"/ui/fe2/store/avatars_display/ico_heavyarmor.tga",
	"/ui/fe2/store/avatars_display/ico_spring_festival.tga",
	"/ui/fe2/store/avatars_display/ico_runandgun.tga",
	"/ui/fe2/store/avatars_display/ico_thaighost.tga",
	"/ui/fe2/store/avatars_display/ico_songkran_2017.tga",
	"/ui/fe2/store/avatars_display/ico_imaginary_friends.tga",
    "/ui/fe2/store/avatars_display/ico_plushies_misfits.tga",
	"/ui/fe2/store/avatars_display/ico_graffiti.tga",
	"/ui/fe2/store/avatars_display/ico_yokai.tga",
	"/ui/fe2/store/avatars_display/ico_magic_girls.tga",
	"/ui/fe2/store/avatars_display/ico_magic_girls.tga",
	"/ui/fe2/store/avatars_display/beauty_and_the_beast.tga",
	"/ui/fe2/store/avatars_display/ico_8bit2.tga",
	"/ui/fe2/store/avatars_display/ico_lock_and_key.tga",
	"/ui/fe2/store/avatars_display/ico_flex.tga",
	"/ui/fe2/store/avatars_display/ico_songkran_2018.tga",
	"/ui/fe2/store/avatars_display/ico_4symbols.tga",
	"/ui/fe2/store/avatars_display/ico_evil_masters.tga",
	"/ui/fe2/store/avatars_display/ico_soccer2018.tga",
	"/ui/fe2/store/avatars_display/ico_aot.tga",
	"/ui/fe2/store/avatars_display/ico_poker.tga",
}

-----------------------------

local _G = getfenv(0)
Store = {}
HoN_Store = _G['HoN_Store'] or {}
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()
RegisterScript2('Store', '34')
HoN_Store.selectedEAPBundle = 2
HoN_Store.puchaseUnavailableConditions = {}

-- hero list stuff
HoN_Store.heroDisplayAltList = nil
HoN_Store.heroAltListScroll = 0
HoN_Store.heroAltListSelected = 1

-- alt avatar stuff --
HoN_Store.numAltAvatars = 0
HoN_Store.altAvatarScroll = 0
HoN_Store.altAvatarsDisplayData = nil
HoN_Store.altWebInfo = {}
HoN_Store.allAvatarsInfo = nil
HoN_Store.altListData = {}
HoN_Store.altDefsLoaded = {}
HoN_Store.altLoadList = {}
HoN_Store.altSelectedID = nil
HoN_Store.altHoveredIndex = nil
HoN_Store.altSearchFilter = ""
HoN_Store.altFilter = nil
HoN_Store.altSort = nil
HoN_Store.altScrollDestination = 0
HoN_Store.altListScrolling = false
HoN_Store.altListNeedsReset = true
HoN_Store.needLoading = true

HoN_Store.isAmbientRGBLocked = true
HoN_Store.isSunColorRGBLocked = true

HoN_Store.multiEfxCount = 1
------------------------

-- featured stuff --
HoN_Store.featuredTable = {}
HoN_Store.selectedFeaturedBundle = 0
--------------------

HoN_Store.InitializeProductShowCallback = nil

-- vanity announcer stuff --
HoN_Store.VoiceAnnouncersTable = {
	['164'] =
	{
		['voicePack_code'] = 'flamboyant',
		['voicePack_models'] = 'unicorn'
	},
	['235'] =
	{
		['voicePack_code'] = 'female',
		['voicePack_models'] = 'arcade_text'
	},
	['410'] =
	{
		['voicePack_code'] = 'ballsofsteel',
		['voicePack_models'] = 'balls'
	},
	['664'] =
	{
		['voicePack_code'] = 'english',
		['voicePack_models'] = 'english'
	},
	['890'] =
	{
		['voicePack_code'] = 'breakycpk',
		['voicePack_models'] = 'arcade_text'
	},
	['1059'] =
	{
		['voicePack_code'] = 'pimp',
		['voicePack_models'] = 'pimp'
	},
	['999999'] =
	{
		['voicePack_code'] = 'meme',
		['voicePack_models'] = 'meme'
	},
	['1311'] =
	{
		['voicePack_code'] = 'seductive',
		['voicePack_models'] = 'seductive'
	},
	['1522'] =
	{
		['voicePack_code'] = 'thai',
		['voicePack_models'] = 'thai'
	},
	['1523'] =
	{
		['voicePack_code'] = 'thaienglish',
		['voicePack_models'] = 'thaienglish'
	},
	['1999'] =
	{
		['voicePack_code'] = 'pirate',
		['voicePack_models'] = 'pirate'
	},
	['2031'] =
	{
		['voicePack_code'] = 'bamf',
		['voicePack_models'] = 'bamf'
	},
	['2108'] =
	{
		['voicePack_code'] = 'bamf_censored',
		['voicePack_models'] = 'bamf'
	},
	['2316'] =
	{
		['voicePack_code'] = 'na_khom',
		['voicePack_models'] = 'na_khom'
	},
	['2339'] =
	{
		['voicePack_code'] = 'ninja',
		['voicePack_models'] = 'ninja'
	},
	['2627'] =
	{
		['voicePack_code'] = 'ursa',
		['voicePack_models'] = 'ursa'
	},
	['2852'] =
	{
		['voicePack_code'] = 'merrick',
		['voicePack_models'] = 'merrick'
	},
	['2905'] =
	{
		['voicePack_code'] = '8-bit',
		['voicePack_models'] = '8-bit'
	},
	['2934'] =
	{
		['voicePack_code'] = 'sea',
		['voicePack_models'] = 'sea'
	},
	['3014'] =
	{
		['voicePack_code'] = 'surfer',
		['voicePack_models'] = 'surfer'
	},
	['3021'] =
	{
		['voicePack_code'] = 'paragon',
		['voicePack_models'] = 'paragon'
	},
	['3153'] =
	{
		['voicePack_code'] = 'dark_master',
		['voicePack_models'] = 'dark_master'
	},
	['3318'] =
	{
		['voicePack_code'] = 'siam',
		['voicePack_models'] = 'siam'
	},
	['3939'] =
	{
		['voicePack_code'] = 'ascension',
		['voicePack_models'] = 'ascension'
	},
	['4037'] =
	{
		['voicePack_code'] = 'soccer',
		['voicePack_models'] = 'soccer'
	},
	['9999'] =
	{
		['voicePack_code'] = 'soccer_mode',
		['voicePack_models'] = 'soccer_mode'
	},
	['4186'] =
	{
		['voicePack_code'] = 'haunted_house',
		['voicePack_models'] = 'haunted_house'
	},
	['4450'] =
	{
		['voicePack_code'] = 'mspudding',
		['voicePack_models'] = 'mspudding'
	},
	['4437'] =
	{
		['voicePack_code'] = 'miku',
		['voicePack_models'] = 'miku'
	},
	['4849'] =
	{
		['voicePack_code'] = 'graffiti',
		['voicePack_models'] = 'graffiti'
	},
	['4911'] =
	{
		['voicePack_code'] = 'esan',
		['voicePack_models'] = 'esan'
	},
	['5457'] =
	{
		['voicePack_code'] = '2018worldcup',
		['voicePack_models'] = '2018worldcup'
	},
	['999999003'] =
	{
		['voicePack_code'] = '',
		['voicePack_models'] = 'arcade_text'
	}
}
--------------------

-- creepes stuff --
HoN_Store.CreepsTable =
{
	['999999008'] = {
		['bad_melee'] = {
			['model'] = '/npcs/bad_melee/model.mdf',
			['effect'] = '',
			['scale']= '1'
		},
		['bad_range'] = {
			['model'] = '/npcs/bad_range/model.mdf',
			['effect'] = '',
			['scale']= '1'
		},
		['bad_siege'] = {
			['model'] = '/npcs/bad_siege/model.mdf',
			['effect'] = '',
			['scale']= '0.55'
		},
		['bad_super_melee'] = {
			['model'] = '/npcs/bad_super_creep/model.mdf',
			['effect'] = '',
			['scale']= '0.8'
		},
		['bad_super_range'] = {
			['model'] = '/npcs/bad_super_range/model.mdf',
			['effect'] = '',
			['scale']= '0.7'
		},
		['good_melee'] = {
			['model'] = '/npcs/good_melee/model.mdf',
			['effect'] = '',
			['scale']= '1'
		},
		['good_range'] = {
			['model'] = '/npcs/good_range/model.mdf',
			['effect'] = '',
			['scale']= '0.95'
		},
		['good_siege'] = {
			['model'] = '/npcs/good_siege/model.mdf',
			['effect'] = '',
			['scale']= '0.7'
		},
		['good_super_melee'] = {
			['model'] = '/npcs/good_super_creep/model.mdf',
			['effect'] = '',
			['scale']= '0.9'
		},
		['good_super_range'] = {
			['model'] = '/npcs/good_range_super_creep/model.mdf',
			['effect'] = '',
			['scale']= '0.9'
		}
	},
	['3928'] = {
		['bad_melee'] = {
			['model'] = '/npcs/bad_melee/model.mdf',
			['effect'] = '/shared/effects/masks/mask_17.effect',
			['scale']= '1'
		},
		['bad_range'] = {
			['model'] = '/npcs/bad_range/model.mdf',
			['effect'] = '/shared/effects/masks/mask_18.effect',
			['scale']= '1'
		},
		['bad_siege'] = {
			['model'] = '/npcs/bad_siege/model.mdf',
			['effect'] = '/shared/effects/masks/mask_19.effect',
			['scale']= '0.55'
		},
		['bad_super_melee'] = {
			['model'] = '/npcs/bad_super_creep/model.mdf',
			['effect'] = '/shared/effects/masks/mask_17_l.effect',
			['scale']= '0.8'
		},
		['bad_super_range'] = {
			['model'] = '/npcs/bad_super_range/model.mdf',
			['effect'] = '/shared/effects/masks/mask_18_l.effect',
			['scale']= '0.7'
		},
		['good_melee'] = {
			['model'] = '/npcs/good_melee/model.mdf',
			['effect'] = '/shared/effects/masks/mask_20.effect',
			['scale']= '1'
		},
		['good_range'] = {
			['model'] = '/npcs/good_range/model.mdf',
			['effect'] = '/shared/effects/masks/mask_21.effect',
			['scale']= '0.95'
		},
		['good_siege'] = {
			['model'] = '/npcs/good_siege/model.mdf',
			['effect'] = '/shared/effects/masks/mask_22.effect',
			['scale']= '0.7'
		},
		['good_super_melee'] = {
			['model'] = '/npcs/good_super_creep/model.mdf',
			['effect'] = '/shared/effects/masks/mask_20_l.effect',
			['scale']= '0.9'
		},
		['good_super_range'] = {
			['model'] = '/npcs/good_range_super_creep/model.mdf',
			['effect'] = '/shared/effects/masks/mask_21_l.effect',
			['scale']= '0.9'
		}
	},
	['4853'] = {
		['bad_melee'] = {
			['model'] = '/npcs/bad_melee/model.mdf',
			['effect'] = '/npcs/bad_melee/effects/graffiti.effect',
			['scale']= '1'
		},
		['bad_range'] = {
			['model'] = '/npcs/bad_range/model.mdf',
			['effect'] = '/npcs/bad_range/effects/graffiti.effect',
			['scale']= '1'
		},
		['bad_siege'] = {
			['model'] = '/npcs/bad_siege/model.mdf',
			['effect'] = '/npcs/bad_siege/effects/graffiti.effect',
			['scale']= '0.55'
		},
		['bad_super_melee'] = {
			['model'] = '/npcs/bad_super_creep/model.mdf',
			['effect'] = '/npcs/bad_super_creep/effects/graffiti.effect',
			['scale']= '0.8'
		},
		['bad_super_range'] = {
			['model'] = '/npcs/bad_super_range/model.mdf',
			['effect'] = '/npcs/bad_super_range/effects/graffiti.effect',
			['scale']= '0.7'
		},
		['good_melee'] = {
			['model'] = '/npcs/good_melee/model.mdf',
			['effect'] = '/npcs/good_melee/effects/graffiti.effect',
			['scale']= '1'
		},
		['good_range'] = {
			['model'] = '/npcs/good_range/model.mdf',
			['effect'] = '/npcs/good_range/effects/graffiti.effect',
			['scale']= '0.95'
		},
		['good_siege'] = {
			['model'] = '/npcs/good_siege/model.mdf',
			['effect'] = '/npcs/good_siege/effects/graffiti.effect',
			['scale']= '0.7'
		},
		['good_super_melee'] = {
			['model'] = '/npcs/good_super_creep/model.mdf',
			['effect'] = '/npcs/good_super_creep/effects/graffiti.effect',
			['scale']= '0.9'
		},
		['good_super_range'] = {
			['model'] = '/npcs/good_range_super_creep/model.mdf',
			['effect'] = '/npcs/good_range_super_creep/effects/graffiti.effect',
			['scale']= '0.9'
		}
	}
}
--------------------

HoN_Store.TauntsTable =
{
	["91"] = { ["effectPath"] = "" },
	["522"] = { ["effectPath"] = "/shared/effects/taunts/cryingbaby/death_preview.effect" },
	["482"] = { ["effectPath"] = "/shared/effects/taunts/toobad/death_preview.effect" },
	["472"] = { ["effectPath"] = "/shared/effects/taunts/gib/death_preview.effect" },
	["679"] = { ["effectPath"] = "/shared/effects/taunts/hellbourne/death_preview.effect" },
	["909"] = { ["effectPath"] = "/shared/effects/taunts/fist/death_preview.effect" },
	["926"] = { ["effectPath"] = "/shared/effects/taunts/dumpster/death_preview.effect" },
	["960"] = { ["effectPath"] = "/shared/effects/taunts/gs/death_preview.effect" },
	["1002"] = { ["effectPath"] = "/shared/effects/taunts/chiprel/death_preview.effect" },
	["1082"] = { ["effectPath"] = "/shared/effects/taunts/kongor/death_preview.effect" },
	["1148"] = { ["effectPath"] = "/shared/effects/taunts/rip/death_preview.effect" },
	["1193"] = { ["effectPath"] = "/shared/effects/taunts/honeybadger/death_preview.effect" },
	["1217"] = { ["effectPath"] = "/shared/effects/taunts/acme/death_preview.effect" },
	["1359"] = { ["effectPath"] = "/shared/effects/taunts/fatlady/death_preview.effect" },
	["1418"] = { ["effectPath"] = "/shared/effects/taunts/blacksmith_taunt/death_preview.effect" },
	["1485"] = { ["effectPath"] = "/shared/effects/taunts/celebration/death_preview.effect" },
	["2000"] = { ["effectPath"] = "/shared/effects/taunts/unicorn/death_preview.effect" },
	["1498"] = { ["effectPath"] = "/shared/effects/taunts/fortunecookie/death_preview.effect" },
	["2573"] = { ["effectPath"] = "/shared/effects/taunts/ursa/death_preview.effect" },
	["2680"] = { ["effectPath"] = "/shared/effects/taunts/snowman/death_preview.effect" },
	["2001"] = { ["effectPath"] = "/shared/effects/taunts/treasure_chest/death_preview.effect" },
	["2885"] = { ["effectPath"] = "/shared/effects/taunts/rest_in_p/death_preview.effect" },
	["2902"] = { ["effectPath"] = "/shared/effects/taunts/gamer_rage/death_preview.effect" },
	["2947"] = { ["effectPath"] = "/shared/effects/taunts/555/death_preview.effect" },
	["3020"] = { ["effectPath"] = "/shared/effects/taunts/paragon/death_preview.effect" },
	["3100"] = { ["effectPath"] = "/shared/effects/taunts/wolf_time/death_preview.effect" },
	["3264"] = { ["effectPath"] = "/shared/effects/taunts/slap_the_troll/death_preview.effect" },
	["3290"] = { ["effectPath"] = "/shared/effects/taunts/siam/death_preview.effect" },
	["3407"] = { ["effectPath"] = "/shared/effects/taunts/random/death_preview.effect" },
	["3471"] = { ["effectPath"] = "/shared/effects/taunts/rekt/death_preview.effect" },
	["3930"] = { ["effectPath"] = "/shared/effects/taunts/ascension01/death_preview.effect" },
	["3931"] = { ["effectPath"] = "/shared/effects/taunts/ascension02/death_preview.effect" },
	["3932"] = { ["effectPath"] = "/shared/effects/taunts/goblin/death_preview.effect" },
	["4071"] = { ["effectPath"] = "/shared/effects/taunts/killerball/death_preview.effect" },
	["4432"] = { ["effectPath"] = "/shared/effects/taunts/bad_present/death_preview.effect" },
	["4850"] = { ["effectPath"] = "/shared/effects/taunts/graffiti/death_preview.effect" },
	["4886"] = { ["effectPath"] = "/shared/effects/taunts/miku/death_preview.effect" },
	["5293"] = { ["effectPath"] = "/shared/effects/taunts/songkran_bucket/death_preview.effect" },
	["5377"] = { ["effectPath"] = "/shared/effects/taunts/forsaken/death_preview.effect" },
	["5534"] = { ["effectPath"] = "/shared/effects/taunts/poker/death_preview.effect" },
}

HoN_Store.CouriersTable =
{
	["462"] = { ["product"] = "C_Penguin" },
	["485"] = { ["product"] = "C_Chicken" },
	["463"] = { ["product"] = "C_Whelp" },
	["486"] = { ["product"] = "C_Goblin" },
	["487"] = { ["product"] = "C_Pig" },
	["488"] = { ["product"] = "C_Bat" },
	["489"] = { ["product"] = "C_Robot" },
	["709"] = { ["product"] = "C_Cat" },
	["951"] = { ["product"] = "C_Dino" },
	["958"] = { ["product"] = "C_Bernard" },
	["959"] = { ["product"] = "C_Panda" },
	["1083"] = { ["product"] = "C_Kongor" },
	["1191"] = { ["product"] = "C_HoneyBadger" },
	["1192"] = { ["product"] = "C_Garena" },
	["1199"] = { ["product"] = "C_Hamster" },
	["1406"] = { ["product"] = "C_Caterpillar" },
	["1437"] = { ["product"] = "C_Turkey" },
	["2361"] = { ["product"] = "C_Izbushka" },
	["2428"] = { ["product"] = "C_Cheburashka" },
	["2572"] = { ["product"] = "C_Ursa" },
	["2615"] = { ["product"] = "C_Pumpkin" },
	["2766"] = { ["product"] = "C_Sheep" },
	["2814"] = { ["product"] = "C_Postman" },
	["2866"] = { ["product"] = "C_8Bit" },
	["2910"] = { ["product"] = "C_WarEffort" },
	["3019"] = { ["product"] = "C_Paragon" },
	["3104"] = { ["product"] = "C_MountainDew" },
	["3288"] = { ["product"] = "C_Fairy" },
	["3510"] = { ["product"] = "C_Dragon_Stage_1" },
	["3511"] = { ["product"] = "C_Dragon_Stage_2" },
	["3512"] = { ["product"] = "C_Dragon_Stage_3" },
	["3513"] = { ["product"] = "C_Dragon_Stage_4" },
	["3514"] = { ["product"] = "C_Dragon_Stage_5" },
	["3515"] = { ["product"] = "C_Dragon_Stage_6" },
	["3936"] = { ["product"] = "C_Ascension_Hyperdrive" },
	["3937"] = { ["product"] = "C_Ascension_Gizmo" },
	["3938"] = { ["product"] = "C_Ascension_Drone" },
	["4068"] = { ["product"] = "C_Streaker" },
	["4109"] = { ["product"] = "C_Gryphon" },
	["4116"] = { ["product"] = "C_Demon" },
	["4431"] = { ["product"] = "C_Reindeer" },
	["4847"] = { ["product"] = "C_Graffiti" },
	['999999007'] = { ["product"] = ""}
}

HoN_Store.WardsTable =
{
	["2665"] = { ["product"] = "W_Aluna" },
	["2666"] = { ["product"] = "W_Empath" },
	["2669"] = { ["product"] = "W_Glacius" },
	["2670"] = { ["product"] = "W_Pearl" },
	["2671"] = { ["product"] = "W_Rhapsody" },
	["2673"] = { ["product"] = "W_Ursa" },
	["2672"] = { ["product"] = "W_Torturer" },
	["2687"] = { ["product"] = "W_Christmas" },
	["2688"] = { ["product"] = "W_Menorah" },
	["2689"] = { ["product"] = "W_Calamity" },
	["2690"] = { ["product"] = "W_Wynd" },
	["2691"] = { ["product"] = "W_Cassie" },
	["2786"] = { ["product"] = "W_Tempest" },
	["2787"] = { ["product"] = "W_Keeper" },
	["2788"] = { ["product"] = "W_Parasite" },
	["2789"] = { ["product"] = "W_Ophelia" },
	["2790"] = { ["product"] = "W_Solstice" },
	["2791"] = { ["product"] = "W_Warbeast" },
	["2792"] = { ["product"] = "W_Cthulhuphant" },
	["2793"] = { ["product"] = "W_Deadlift" },
	["2800"] = { ["product"] = "W_Shamrock" },
	["2816"] = { ["product"] = "W_Gsl" },
	["2843"] = { ["product"] = "W_DDog" },
	["2844"] = { ["product"] = "W_Sync" },
	["2849"] = { ["product"] = "W_8bit" },
	["2853"] = { ["product"] = "W_Honiversary_5" },
	["2859"] = { ["product"] = "W_Magebane" },
	["2860"] = { ["product"] = "W_Silhouette" },
	["2861"] = { ["product"] = "W_Dark_Lady" },
	["2862"] = { ["product"] = "W_Moonqueen" },
	["2863"] = { ["product"] = "W_Swiftblade" },
	["2864"] = { ["product"] = "W_Forsaken_Archer" },
	["2916"] = { ["product"] = "W_Benzington" },
	["2917"] = { ["product"] = "W_Pharaoh" },
	["2918"] = { ["product"] = "W_Pebbles" },
	["2919"] = { ["product"] = "W_Hammerstorm" },
	["2920"] = { ["product"] = "W_Fayde" },
	["2921"] = { ["product"] = "W_Magmus" },
	["2909"] = { ["product"] = "W_WarEffort" },
	["2997"] = { ["product"] = "W_Independence" },
	["2998"] = { ["product"] = "W_Pelita" },
	["3018"] = { ["product"] = "W_Paragon" },
	["3048"] = { ["product"] = "W_Deadwood" },
	["3049"] = { ["product"] = "W_Devourer" },
	["3050"] = { ["product"] = "W_Kraken" },
	["3051"] = { ["product"] = "W_Gauntlet" },
	["3052"] = { ["product"] = "W_Nomad" },
	["3053"] = { ["product"] = "W_Ravenor" },
	["3055"] = { ["product"] = "W_Beach_ball" },
	["3147"] = { ["product"] = "W_Bubbles" },
	["3148"] = { ["product"] = "W_Chronos" },
	["3149"] = { ["product"] = "W_Wildsoul" },
	["3150"] = { ["product"] = "W_Plague_Rider" },
	["3151"] = { ["product"] = "W_Rally" },
	["3152"] = { ["product"] = "W_Wretched_Hag" },
	["3204"] = { ["product"] = "W_Devo_Paku" },
	["3205"] = { ["product"] = "W_Devo_Pirate" },
	["3206"] = { ["product"] = "W_Bedsheet_Devo" },
	["3207"] = { ["product"] = "W_Jinchan_Devo" },
	["3217"] = { ["product"] = "W_Devo_Rift" },
	["3218"] = { ["product"] = "W_Devo_Gluttony" },
	["3219"] = { ["product"] = "W_Halloween" },
	["3245"] = { ["product"] = "W_Hotshot_Heroes" },
	["3272"] = { ["product"] = "W_Siam" },
	["3289"] = { ["product"] = "W_Thanksgiving" },
	["3313"] = { ["product"] = "W_Christmas_2015" },
	["3314"] = { ["product"] = "W_New_Year_2016" },
	["3466"] = { ["product"] = "W_Chinese_Ny_2016" },
	["3465"] = { ["product"] = "W_Easter" },
	["3495"] = { ["product"] = "W_Valentines_2016" },
	["3593"] = { ["product"] = "W_Easter" },
	["3933"] = { ["product"] = "W_Lookout" },
	["3934"] = { ["product"] = "W_Vizmo" },
	["3935"] = { ["product"] = "W_Sentry" },
	["4069"] = { ["product"] = "W_Floodlight" },
	["4070"] = { ["product"] = "W_Penaltycard" },
	["4429"] = { ["product"] = "W_Bigeye" },
	["4430"] = { ["product"] = "W_Snowman" },
	["4848"] = { ["product"] = "W_Graffiti" },
	["4885"] = { ["product"] = "W_Miku" },
	['999999006'] = { ["product"] = "" }
}
---------------------------------------------------------------
HoN_Store.TPEffectsTable =
{
	['999999010'] = {
						{
							effect1 = '/items/basic/homecoming_stone/effects/goldenred/source.effect',
							effect2 = '/items/basic/homecoming_stone/effects/goldenred/target_affector_3000.effect',
							btnnormal = '/ui/fe2/store/icons/mastery1.png',
							btnover ='/ui/fe2/store/icons/mastery1_over.png',
							onclicklua = function () HoN_Store:OnClickSwitchTPEffect(1) end
						},
						{
							effect1 = '/items/basic/homecoming_stone/effects/gold/source.effect',
							effect2 = '/items/basic/homecoming_stone/effects/gold/target_affector_3000.effect',
							btnnormal = '/ui/fe2/store/icons/mastery2.png',
							btnover ='/ui/fe2/store/icons/mastery2_over.png',
							onclicklua = function () HoN_Store:OnClickSwitchTPEffect(2) end
						},
						{
							effect1 = '/items/basic/homecoming_stone/effects/silver/source.effect',
							effect2 = '/items/basic/homecoming_stone/effects/silver/target_affector_3000.effect',
							btnnormal = '/ui/fe2/store/icons/mastery3.png',
							btnover ='/ui/fe2/store/icons/mastery3_over.png',
							onclicklua = function () HoN_Store:OnClickSwitchTPEffect(3) end
						},
						{
							effect1 = '/items/basic/homecoming_stone/effects/source.effect',
							effect2 = '/items/basic/homecoming_stone/effects/target_affector_3000.effect',
							btnnormal = '/ui/legion/ability_coverup.tga',
							btnover ='/ui/legion/ability_coverup.tga',
							onclicklua = function () HoN_Store:OnClickSwitchTPEffect(4) end
						}
					},
	['4884'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/miku/source.effect',
							effect2 = '/items/basic/homecoming_stone/effects/miku/target_affector_3000.effect'
						}
					},
	['5152'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/8bit2/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/8bit2/target_affector_preview.effect'
						}
					},
	['5275'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/con6reward/1/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/con6reward/1/target_affector_preview.effect'
						}
					},

	['5276'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/con6reward/2/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/con6reward/2/target_affector_preview.effect'
						}
					},
	['5277'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/con6reward/3/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/con6reward/3/target_affector_preview.effect'
						}
					},
	['5278'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/con6reward/4/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/con6reward/4/target_affector_preview.effect'
						}
					},
	['5279'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/con6reward/5/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/con6reward/5/target_affector_preview.effect'
						}
					},
	['5292'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/songkran/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/songkran/target_affector_preview.effect'
						}
					},		
	['5458'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/worldcup2018/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/worldcup2018/target_affector_preview.effect'
						}
					},
	['5459'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/con7reward/1/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/con7reward/1/target_affector_preview.effect'
						}
					},
	['5460'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/con7reward/2/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/con7reward/2/target_affector_preview.effect'
						}
					},
	['5461'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/con7reward/3/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/con7reward/3/target_affector_preview.effect'
						}
					},
	['5462'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/con7reward/4/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/con7reward/4/target_affector_preview.effect'
						}
					},
	['5463'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/con7reward/5/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/con7reward/5/target_affector_preview.effect'
						}
					},	
	['5490'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/punk/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/punk/target_affector_preview.effect'
						}
					},	
	['5592'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/con8reward/1/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/con8reward/1/target_affector_preview.effect'
						}
					},	
	['5593'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/con8reward/2/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/con8reward/2/target_affector_preview.effect'
						}
					},	
	['5594'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/con8reward/3/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/con8reward/3/target_affector_preview.effect'
						}
					},	
	['5595'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/con8reward/4/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/con8reward/4/target_affector_preview.effect'
						}
					},	
	['5596'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/con8reward/5/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/con8reward/5/target_affector_preview.effect'
						}
					},	
	['5652'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/con9reward/1/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/con9reward/1/target_affector_preview.effect'
						}
					},	
	['5653'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/con9reward/2/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/con9reward/2/target_affector_preview.effect'
						}
					},	
	['5654'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/con9reward/3/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/con9reward/3/target_affector_preview.effect'
						}
					},	
	['5655'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/con9reward/4/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/con9reward/4/target_affector_preview.effect'
						}
					},	
	['5656'] =		{
						{
							effect1 = '/items/basic/homecoming_stone/effects/con9reward/5/source_preview.effect',
							effect2 = '/items/basic/homecoming_stone/effects/con9reward/5/target_affector_preview.effect'
						}
					},					
}
-------------------------------------------------------------------------------
HoN_Store.TauntBadgesTable =
{
	['999999009'] = {
						{
							effect = '/shared/effects/taunting_goldenred.effect',
							btnnormal = '/ui/fe2/store/icons/tauntbadge1.png',
							btnover ='/ui/fe2/store/icons/tauntbadge1_over.png',
							onclicklua = function () HoN_Store:OnClickSwitchTauntBadge(1) end
						},
						{
							effect = '/shared/effects/taunting_gold.effect',
							btnnormal = '/ui/fe2/store/icons/tauntbadge2.png',
							btnover ='/ui/fe2/store/icons/tauntbadge2_over.png',
							onclicklua = function () HoN_Store:OnClickSwitchTauntBadge(2) end
						},
						{
							effect = '/shared/effects/taunting_silver.effect',
							btnnormal = '/ui/fe2/store/icons/tauntbadge3.png',
							btnover ='/ui/fe2/store/icons/tauntbadge3_over.png',
							onclicklua = function () HoN_Store:OnClickSwitchTauntBadge(3) end
						}
					},
}
-- card(coupon & trial) info
HoN_Store.GCardsTable = {}
HoN_Store.CardLastSelect = -1

HoN_Store.bIn8BitMode = false

local Avatar_Preview_MultiEffects_CurrentIndex = 0
local Avatar_Preview_MultiModels_CurrentIndex = 0
local Avatar_Preview_SetEffect_On = false

MODEL_PANEL_EFFECT_CHANNEL_MAX = 5
MODEL_PANEL_EFFECT_DISPLAY_MAX = 9
HoN_Store.EditorPanel_CurrentModelIndex = 0
HoN_Store.vanitySpecialDailyGold = -1
HoN_Store.vanitySpecialDailySilver = -1

function HoN_Store:SetSpecialDailyPrice()
	if HoN_Store.vanitySpecialDailyGold > 0 then 
		Set('microStoreCDPriceGold', HoN_Store.vanitySpecialDailyGold)
	end

	if HoN_Store.vanitySpecialDailySilver > 0 then 
		Set('microStoreCDPriceSilver', HoN_Store.vanitySpecialDailySilver)
	end
end

local function TestEcho(str)
	if not GetCvarBool('releaseStage_stable') then
		Echo(str)
	end
end


function HoN_Store:MicroStoreResults(productEligibility, productIDs, productEnhancements, productEnhancementIDs)
	TestEcho('HoN_Store:MicroStoreResults productEligibility: '..productEligibility)

	HoN_Store.heroTable = {}


	if (productEligibility) and NotEmpty(productEligibility) then
		HoN_Store.productEligibility = {}
		local productEligibilityTable = explode('|', productEligibility)
		for index, eligibilityString in pairs(productEligibilityTable) do
			local eligibilityTable = explode('~', eligibilityString)
			local productID = eligibilityTable[1]
			HoN_Store.productEligibility[productID]					 = {}
			HoN_Store.productEligibility[productID].productID		 = eligibilityTable[1]
			HoN_Store.productEligibility[productID].eligbleID		 = eligibilityTable[2]
			HoN_Store.productEligibility[productID].eligible		 = eligibilityTable[3]
			HoN_Store.productEligibility[productID].goldCost		 = eligibilityTable[4]
			HoN_Store.productEligibility[productID].silverCost		 = eligibilityTable[5]
			HoN_Store.productEligibility[productID].requiredProducts = eligibilityTable[6]
		end
	else
		HoN_Store.productEligibility = HoN_Store.productEligibility or {}
	end

	if productIDs and NotEmpty(productIDs) then
		storeProductIDs = explode('|', productIDs)

		if productEnhancements and NotEmpty(productEnhancements) then
			HoN_Store.productEnhancements = {}
			local enhancementTable = explode('|', productEnhancements)
			for index, productCodes in ipairs(enhancementTable) do
				if NotEmpty(productCodes) then
					HoN_Store.productEnhancements[tonumber(storeProductIDs[index])] = explode('~', productCodes)
				end
			end
		else
			HoN_Store.productEnhancements = HoN_Store.productEnhancements or {}
		end

		if productEnhancementIDs and NotEmpty(productEnhancementIDs) then
			HoN_Store.productEnhancementIDs = {}
			local enhancementIDTable = explode('|', productEnhancementIDs)

			for index, productIDList in ipairs(enhancementIDTable) do
				if NotEmpty(productIDList) then
					HoN_Store.productEnhancementIDs[tonumber(storeProductIDs[index])] = explode('~', productIDList)
				end
			end
		else
			HoN_Store.productEnhancementIDs = HoN_Store.productEnhancementIDs or {}
		end
	end

end

function HoN_Store:StoreClosed()
	HoN_Store.altListNeedsReset = true
	GetWidget('StorePromo'):SetVisible(false)
end

function HoN_Store:InputBoxUpdateAvatar(input)
	if (input) then
		HoN_Store.altSearchFilter = input
		HoN_Store:CreateAltList(input, HoN_Store.altFilter)
		HoN_Store:UpdateAltScroller()
		HoN_Store:DisplayAvatarList()
	end
end

function HoN_Store:UpdateAltFilter(filterVal)
	HoN_Store.altFilter = tonumber(filterVal)
	HoN_Store:CreateAltList(HoN_Store.altSearchFilter, HoN_Store.altFilter)
	HoN_Store:UpdateAltScroller()
	HoN_Store:DisplayAvatarList()
end

function HoN_Store:UpdateAltSort(sortType)
	HoN_Store.altSort = tonumber(sortType)
	HoN_Store:SortAltList(HoN_Store.altSort)
	HoN_Store:DisplayAvatarList()
end

function HoN_Store:SortAltList(sortType)
	-- 0 = Hero Name
	-- 1 = Price Ascending
	-- 2 = Price Descending
	-- 3 = Avatar Name
	-- 4 = Release Date
	local sortFunc = nil

	if (sortType == 0) then
		sortFunc = function(a,b)
			if (Translate("mstore_"..HoN_Store.altWebInfo[a].heroName.."_name") == Translate("mstore_"..HoN_Store.altWebInfo[b].heroName.."_name")) then
				return (string.lower(Translate("mstore_product"..a.."_name")) < string.lower(Translate("mstore_product"..b.."_name")))
			else
				return (string.lower(Translate("mstore_"..HoN_Store.altWebInfo[a].heroName.."_name")) < string.lower(Translate("mstore_"..HoN_Store.altWebInfo[b].heroName.."_name")))
			end
		end
	elseif (sortType == 1) then
		sortFunc = function(a,b)
			if (HoN_Store.altWebInfo[a].goldCost == HoN_Store.altWebInfo[b].goldCost) then
				return (string.lower(Translate("mstore_"..HoN_Store.altWebInfo[a].heroName.."_name")) < string.lower(Translate("mstore_"..HoN_Store.altWebInfo[b].heroName.."_name")))
			else
				return (HoN_Store.altWebInfo[a].goldCost < HoN_Store.altWebInfo[b].goldCost)
			end
		end
	elseif (sortType == 2) then
		sortFunc = function(a,b)
			if (HoN_Store.altWebInfo[a].goldCost == HoN_Store.altWebInfo[b].goldCost) then
				return (string.lower(Translate("mstore_"..HoN_Store.altWebInfo[a].heroName.."_name")) < string.lower(Translate("mstore_"..HoN_Store.altWebInfo[b].heroName.."_name")))
			else
				return (HoN_Store.altWebInfo[a].goldCost > HoN_Store.altWebInfo[b].goldCost)
			end
		end
	elseif (sortType == 3) then
		sortFunc = function(a,b)
			return (string.lower(Translate("mstore_product"..a.."_name")) < string.lower(Translate("mstore_product"..b.."_name")))
		end
	elseif (sortType == 4) then
		sortFunc = function(a,b)
			if (HoN_Store.allAvatarsInfo[a].heroEntryID == HoN_Store.allAvatarsInfo[b].heroEntryID) then
				if (a == b) then
					return (string.lower(Translate("mstore_"..HoN_Store.altWebInfo[a].heroName.."_name")) < string.lower(Translate("mstore_"..HoN_Store.altWebInfo[b].heroName.."_name")))
				else
					return (a > b)
				end
			else
				return (HoN_Store.allAvatarsInfo[a].heroEntryID > HoN_Store.allAvatarsInfo[b].heroEntryID)
			end
		end
	end

	if (sortFunc) then
		table.sort(HoN_Store.altListData, sortFunc)
	end
end

function HoN_Store:ResetAltList()
	HoN_Store.altSort = 4
	HoN_Store.altAvatarScroll = 0
	HoN_Store.altListScrolling = false
	HoN_Store.altFilter = 0

	local wdg = GetWidget("store_alt_avatar_scroller")
	if wdg ~= nil then wdg:SetValue(0) end
end

local function AltScrollStep()
	if (HoN_Store.altAvatarScroll ~= HoN_Store.altScrollDestination) then
		local listMaster = GetWidget("heroAvatarListContainer")
		local diff = (HoN_Store.altScrollDestination - HoN_Store.altAvatarScroll)
		-- exponential decay to get good step times
		local stepTime = (150.0 * math.pow((1 - .17), (math.abs(diff) - 1)))
		local step = 0
		local scrollDest = 0
		if (diff > 0) then
			scrollDest = -interface:GetHeightFromString("11.7h")
			step = 1
		elseif (diff < 0) then
			scrollDest = 0
			step = -1
		end

		-- I don't like this, but it works for now
		if (stepTime < 1.0) then
			if (step < 0) then
				step = math.max(step * math.floor(1.0 / stepTime), -5)
			else
				step = math.min(step * math.floor(1.0 / stepTime), 5)
			end
		end

		listMaster:SlideY(scrollDest, stepTime)
		listMaster:Sleep(stepTime, function()
			HoN_Store.altAvatarScroll = HoN_Store.altAvatarScroll + step
			listMaster:SetY(-interface:GetHeightFromString("5.85h"))
			HoN_Store:DisplayAvatarList()
			AltScrollStep()
		end)
	else
		HoN_Store.altListScrolling = false
	end
end

function HoN_Store:SlideAlts(slide)
	slide = round(tonumber(slide))

	if (slide ~= HoN_Store.altAvatarScroll and slide ~= HoN_Store.altScrollDestination) then
		HoN_Store.altScrollDestination = slide
		if (not HoN_Store.altListScrolling) then
			HoN_Store.altListScrolling = true
			AltScrollStep()
		end
	end
end

function HoN_Store:UpdateAltScroller()
	local scroller = GetWidget("store_alt_avatar_scroller")
	if scroller ~= nil then
		local value = tonumber(scroller:GetValue())
		local max = #HoN_Store.altListData - 6
		if (max < 0) then
			max = 0
		end

		if (value > max) then
			HoN_Store.altAvatarScroll = max
			scroller:UICmd("SetValue("..max..");")
		end

		scroller:UICmd("SetMaxValue("..max..");")
	end
end

function HoN_Store:GetAltIndex(identifier)
	if (not identifier or identifier == "") then
		return nil
	end
	local id = tonumber(identifier)

	for i,v in ipairs(HoN_Store.altListData) do
		if (id and v == id) then
			return i
		elseif ((HoN_Store.altWebInfo[v].heroName.."."..HoN_Store.altWebInfo[v].altCode) == identifier) then
			return i
		end
	end

	return nil
end

function HoN_Store:SelectAltIndex(index)
	index = tonumber(index)
	if (index) then
		if (index == 1) then
			HoN_Store.altAvatarScroll = 0
			HoN_Store:ClickAltAvatar(1, true)
		elseif ((#HoN_Store.altListData - 6) <= 0) then
			HoN_Store.altAvatarScroll = 0
			HoN_Store:ClickAltAvatar(index, true)
		elseif ((index - 1) > (#HoN_Store.altListData - 6)) then
			HoN_Store.altAvatarScroll = (#HoN_Store.altListData - 6)
			HoN_Store:ClickAltAvatar(index - (#HoN_Store.altListData - 6), true)
		else
			HoN_Store.altAvatarScroll = (index - 2)
			HoN_Store:ClickAltAvatar(2, true)
		end

		local wdg = GetWidget("store_alt_avatar_scroller")
		if wdg ~= nil then wdg:SetValue(HoN_Store.altAvatarScroll) end
	end
end

function HoN_Store:CreateAltList(filterString, filterType)
	-- filter types
	-- 0 = all
	-- 1 = Owned Heroes
	-- 2 = Unowned Heroes
	-- 3 = Owned Avatars
	-- 4 = Unowned Avatars

	HoN_Store.altListData = {}
	if ((filterString == "") and (filterType == 0)) then
		HoN_Store.altListData = HoN_Store.altAvatarsDisplayData
	else
		-- only do a string compare once on these, we don't need to do it every alt
		local searchGold = string.find(string.lower(Translate("mstore_goldcollection")), string.lower(filterString), 1, true)
		local searchLimited = string.find(string.lower(Translate("mstore_limitededition")), string.lower(filterString), 1, true)
		local searchHoliday = string.find(string.lower(Translate("mstore_holidayedition")), string.lower(filterString), 1, true)
		local searchUlt = string.find(string.lower(Translate("mstore_ultimateedition")), string.lower(filterString), 1, true)
		local searchEnhancement = false	-- need to fill this in
		--[[
			string.find(string.lower(Translate("mstore_enhancement")), string.lower(filterString), 1, true)
		--]]

		local ceSetSearchTable = {}
		-- we are gonna use the image table, we don't have another way of knowing how many CE sets there are
		for i=1, #collectorsEditionSetImages do
			ceSetSearchTable[i] = string.find(string.lower(Translate("mstore_collectorsEditionSet"..i)), string.lower(filterString), 1, true)
		end

		if (HoN_Store.altAvatarsDisplayData) then
			for i,v in ipairs(HoN_Store.altAvatarsDisplayData) do
				local heroName = HoN_Store.altWebInfo[v].heroName
				local owned = HoN_Store.altWebInfo[v].owned
				local altAvatar = HoN_Store.allAvatarsInfo[v]

				if ((filterType == 0) or
					((filterType == 1) and AtoB(interface:UICmd("CanAccessHeroProduct('"..heroName.."')"))) or
					((filterType == 2) and (not AtoB(interface:UICmd("CanAccessHeroProduct('"..heroName.."')")))) or
					((filterType == 3) and owned) or
					((filterType == 4) and (not owned))) then

					-- you can search by name, or stuff like gold edition, etc.
					if ((filterString == "") or
						string.find(string.lower(Translate("mstore_"..heroName.."_name")), string.lower(filterString), 1, true) or
						string.find(string.lower(Translate("mstore_product"..v.."_name")), string.lower(filterString), 1, true) or
						(altAvatar.goldCollection and searchGold) or
						(altAvatar.limitedEdition and searchLimited) or
						(altAvatar.holidayEdition and searchHoliday) or
						(altAvatar.ultimate and searchUlt) or
						(altAvatar.hasEnhancement and searchEnhancement) or
						(tonumber(altAvatar.collectorsSet) and ceSetSearchTable[tonumber(altAvatar.collectorsSet)])
					) then
						table.insert(HoN_Store.altListData, v)
					end
				end
			end
		end
	end

	HoN_Store:SortAltList(HoN_Store.altSort)
end

function HoN_Store:DisplayAvatarList()
	-- set the gradients visible, fix the fade stuff later
	if (HoN_Store.altAvatarScroll == 0) then
		GetWidget("shop_alt_top_grad"):FadeOut(100)
	else
		GetWidget("shop_alt_top_grad"):FadeIn(100)
	end

	if (HoN_Store.altAvatarScroll == (#HoN_Store.altListData - 6)) then
		GetWidget("shop_alt_bottom_grad"):FadeOut(100)
	else
		GetWidget("shop_alt_bottom_grad"):FadeIn(100)
	end

	-- populate the slots
	for i=0,7 do
		local offset = i+HoN_Store.altAvatarScroll

		if (offset <= 0) or (offset > #HoN_Store.altListData) then
			GetWidget("heroAvatarEntry_"..i):SetVisible(0)
		else
			-- alt data
			local altAvatar = HoN_Store.allAvatarsInfo[HoN_Store.altListData[offset]]
			local tempWidg = nil

			if (GetCvarBool('ui_altAvatarListDebug')) then
				TestEcho('^gPopulating Alt:')
				TestEcho('\t^yProductName: '..HoN_Store.altWebInfo[altAvatar.productID].heroName..'.'..HoN_Store.altWebInfo[altAvatar.productID].altCode)
				TestEcho('\t^yProductID: '..altAvatar.productID)
			end
			local err = false

			-- load defs if needed
			local loadingCover = GetWidget("shop_alt_loading_"..i)
			if (not GetCvarBool("_entityDefinitionsLoaded") and not HoN_Store.altDefsLoaded[altAvatar.definitionFolder]) then
				groupfcall("slot_premium_trim_"..i, function(_, widget, _) widget:SetVisible(0) end)
				groupfcall("slot_normal_trim_"..i, function(_, widget, _) widget:SetVisible(0) end)
				loadingCover:SetVisible(1)
				loadingCover:Sleep(1, function()
					loadingCover:SetVisible(0)
					HoN_Store.altDefsLoaded[altAvatar.definitionFolder] = true
					interface:UICmd("LoadEntityDefinitionsFromFolder('"..altAvatar.definitionFolder.."');")
					HoN_Store:DisplayAvatarList()
					-- the above will cause not yet loaded entities to resleep again and wait again
					-- this is good however as it limits the number of entities that load at once, which reduces/eliminates the hitching
				end)
			else
				loadingCover:SetVisible(0)
				loadingCover:Sleep(1, function() end) -- interrupt a sleep already running to load something

				-- update the hover if needed
				if (HoN_Store.altHoveredIndex and HoN_Store.altHoveredIndex == i) then
					interface:UICmd("GetHeroInfo('"..HoN_Store.altWebInfo[altAvatar.productID].heroName.."');")
				end

				-- update selected stuff
				if (HoN_Store.altSelectedID and HoN_Store.altSelectedID == altAvatar.productID) then
					GetWidget("avatarShadow_"..i):DoEventN(0)
					groupfcall("avatarSelectedPieces"..i, function(_, widget, _) widget:DoEventN(0) end)
					if (HoN_Store.altWebInfo[altAvatar.productID].isPremium) then
						if (not HoN_Store.altListScrolling) then
							GetWidget("slot_prem_selected_"..i):DoEventN(0)
						else
							GetWidget("slot_prem_selected_"..i):SetVisible(1)
						end
					else
						if (not HoN_Store.altListScrolling) then
							GetWidget("slot_norm_selected_"..i):DoEventN(0)
						else
							GetWidget("slot_norm_selected_"..i):SetVisible(1)
						end
					end
				else
					GetWidget("avatarShadow_"..i):DoEventN(1)
					groupfcall("avatarSelectedPieces"..i, function(_, widget, _) widget:DoEventN(1) end)
					if (HoN_Store.altWebInfo[altAvatar.productID].isPremium) then
						if (not HoN_Store.altListScrolling) then
							GetWidget("slot_prem_selected_"..i):DoEventN(1)
						else
							GetWidget("slot_prem_selected_"..i):SetVisible(0)
						end
					else
						if (not HoN_Store.altListScrolling) then
							GetWidget("slot_norm_selected_"..i):DoEventN(1)
						else
							GetWidget("slot_norm_selected_"..i):SetVisible(0)
						end
					end
				end

				-- vars for cost and stuff since eligibility can mess it up
				local goldCost = HoN_Store.altWebInfo[altAvatar.productID].goldCost
				local silverCost = HoN_Store.altWebInfo[altAvatar.productID].silverCost
				local purchasable = HoN_Store.altWebInfo[altAvatar.productID].purchasable

				-- Eligibilty Stuff
				local eligIndex = tostring(altAvatar.productID)
				if (HoN_Store.productEligibility) and (HoN_Store.productEligibility[eligIndex]) then
					local requiredProductTooltip = ""

					if (HoN_Store.productEligibility[eligIndex].eligible) then
						if AtoB(HoN_Store.productEligibility[eligIndex].eligible) then
							if NotEmpty(HoN_Store.productEligibility[eligIndex].goldCost) then
								goldCost = tonumber(HoN_Store.productEligibility[eligIndex].goldCost)
							else
								goldCost = 0
							end
							if NotEmpty(HoN_Store.productEligibility[eligIndex].silverCost) then
								silverCost = tonumber(HoN_Store.productEligibility[eligIndex].silverCost)
							else
								silverCost = 0
							end
						end

						if (HoN_Store.productEligibility[eligIndex].eligbleID) then
							local requireTable = nil

							if (HoN_Store.productEligibility[eligIndex].requiredProducts) then
								requireTable = explode(';', HoN_Store.productEligibility[eligIndex].requiredProducts)
							else
								requireTable = explode('|', Translate('mstore_elig_requirements_' .. HoN_Store.productEligibility[eligIndex].productID))
							end

							if (requireTable) then
								requiredProductTooltip = Translate("mstore_elig_required")
								for index, requiredProductID in pairs(requireTable) do
									local translateReqProductID = Translate('mstore_product'..requiredProductID..'_name')
									if (translateReqProductID ~= 'mstore_product'..requiredProductID..'_name') then
										requiredProductTooltip = requiredProductTooltip .. '\n' .. translateReqProductID
									end
								end
							end
						end
					end

					local eligDescStr = Translate('mstore_elig_package_'..HoN_Store.productEligibility[eligIndex].productID)
					if (eligDescStr == ('mstore_elig_package_'..HoN_Store.productEligibility[eligIndex].productID)) then
						eligDescStr = ""
					end

					tempWidg = GetWidget("avatarElibPriceHover_"..i)
					tempWidg:SetCallback('onmouseover', function()
						Trigger('genericMainFloatingTip', 'true', '', '', 'mstore_elig', 'mstore_elig_tip', eligDescStr, requiredProductTooltip, '', '')
						end
					)
					tempWidg:RefreshCallbacks()
					GetWidget("avatarEligPrice_"..i):SetVisible(not AtoB(HoN_Store.productEligibility[eligIndex].eligible))
					GetWidget("avatarPurchaseGlow_"..i):SetVisible(AtoB(HoN_Store.productEligibility[eligIndex].eligible))
					purchasable = AtoB(HoN_Store.productEligibility[eligIndex].eligible)
					-- icky fix, but it makes sense. If you aren't eligible, it's not purchasable.
					HoN_Store.altWebInfo[altAvatar.productID].purchasable = purchasable
				else
					tempWidg = GetWidget("avatarElibPriceHover_"..i)
					tempWidg:ClearCallback('onmouseover')
					tempWidg:RefreshCallbacks()
					GetWidget("avatarEligPrice_"..i):SetVisible(false)
					GetWidget("avatarPurchaseGlow_"..i):SetVisible(false)
				end

				local showGold		= true
				local showSilver	= true

				if silverCost == 9002 then
					showSilver = false
				elseif goldCost == 9006 then
					showGold = false
				end

				GetWidget("avatarSilver_"..i):SetVisible(showSilver)
				if showSilver then
					GetWidget("avatarSilverLabel_"..i):SetText(silverCost)
				end

				GetWidget("avatarGold_"..i):SetVisible(showGold)
				if showGold then
					GetWidget("avatarGoldLabel_"..i):SetText(goldCost)
				end

				-- gold cost
				-- if (goldCost == 9001) then
				-- 	GetWidget("avatarGold_"..i):SetVisible(0)
				-- else
				--	GetWidget("avatarGold_"..i):SetVisible(1)

				-- end
				-- while it's correct to hide a 9001 gold cost, the old code didn't do this so I won't either.

				-- EAP cost
				local var = GetCvarInt('microStoreSpecialDisplay'..altAvatar.heroEntryID, true)
				if (not var) then
					GetWidget("avatarEAPPrice_"..i):SetVisible(goldCost == 9001)
				else
					GetWidget("avatarEAPPrice_"..i):SetVisible(var == 1)
				end

				GetWidget("avatarPlinkoPrice_"..i):SetVisible(goldCost == 9003)
				GetWidget("avatarEsportsPrice_"..i):SetVisible(goldCost == 9004)
				GetWidget("avatarQuestsPrice_"..i):SetVisible(goldCost == 9005)
				GetWidget("avatarCodexPrice_"..i):SetVisible(goldCost == 9007)

				GetWidget("heroAvatarPrice_"..i):SetVisible(true)

				local trialInfo = GetTrialInfo(HoN_Store.altWebInfo[altAvatar.productID].heroName, HoN_Store.altWebInfo[altAvatar.productID].altCode)
				if NotEmpty(trialInfo) and (not HoN_Store.altWebInfo[altAvatar.productID].owned) then
					GetWidget("avatarTrial_"..i):SetVisible(true)
					if (HoN_Store.altWebInfo[altAvatar.productID].purchasable) then
						GetWidget("heroAvatarCards_"..i):SetVisible(true)
					else
						GetWidget("heroAvatarCards_"..i):SetVisible(false)
					end
				else
					GetWidget("avatarTrial_"..i):SetVisible(false)

					local cardsTable = GetCardsInfo(HoN_Store.altWebInfo[altAvatar.productID].heroName, HoN_Store.altWebInfo[altAvatar.productID].altCode)
					if (cardsTable) and (#cardsTable >= 1) and (not HoN_Store.altWebInfo[altAvatar.productID].owned) and HoN_Store.altWebInfo[altAvatar.productID].purchasable then
						GetWidget("heroAvatarCards_"..i):SetVisible(true)
					else
						GetWidget("heroAvatarCards_"..i):SetVisible(false)
					end
				end

				-- backer
				local backer = "avatar_backer_regular"
				if (HoN_Store.altWebInfo[altAvatar.productID].isPremium) then
					backer = "avatar_backer_premium"
				end

				if (HoN_Store.productEligibility) and (HoN_Store.productEligibility[eligIndex]) and (HoN_Store.productEligibility[eligIndex].eligible) then
					backer = "avatar_backer_special"
				end
				tempWidg = GetWidget("avatarBacker_"..i)
				tempWidg:SetTexture("/ui/fe2/store/avatars_display/"..backer..".tga")
				if (HoN_Store.altWebInfo[altAvatar.productID].purchasable) then
					tempWidg:SetRenderMode('normal')
				else
					tempWidg:SetRenderMode('grayscale')
				end

				-- alt icon (now with 100% error handling)
				local avatarIcon = GetWidget("avatarAvatarIcon_"..i)
				local success, errorMessage =	pcall(
					function()
						local path = interface:UICmd("GetHeroIcon2PathFromProduct('"..HoN_Store.altWebInfo[altAvatar.productID].heroName.."."..HoN_Store.altWebInfo[altAvatar.productID].altCode.."')")
						GetWidget("avatarAvatarIcon_"..i):SetTexture(path)
						assert(NotEmpty(path), "An empty path was returned for the avatar icon.")
					end
				)

				-- handle failure
				if (not success) then
					if (not GetCvarBool('ui_altAvatarListDebug')) then
						avatarIcon:SetTexture("$checker")	-- checker board if not debug
					else 	-- debug and warning for all!
						avatarIcon:SetTexture("/ui/icons/awards/award_deaths_l.tga")

						-- flash the icon
						local flash = function(flashIn, func)
							if (flashIn) then
								avatarIcon:FadeIn(200)
							else
								avatarIcon:FadeOut(200)
							end

							avatarIcon:Sleep(200, function() func(not flashIn, func) end)
						end

						flash(false, flash)

						-- debug output
						TestEcho("\n^r^:Error populating the alt icon!")
						TestEcho("^r^:Error Message: ^*"..errorMessage)
						TestEcho("^r^:Name used to try and get the icon: ^y"..HoN_Store.altWebInfo[altAvatar.productID].heroName.."."..HoN_Store.altWebInfo[altAvatar.productID].altCode)
						err = true
					end
				elseif (GetCvarBool('ui_altAvatarListDebug')) then -- success with debug, remove any sleeps from failure
					avatarIcon:Sleep(0, function() end)
					avatarIcon:SetVisible(1)
				end

				-- hero icon
				local heroIcon = GetWidget("avatarHeroIcon_"..i)
				local success, errorMessage =	pcall(
					function()
						heroIcon:SetTexture(GetEntityIconPath(HoN_Store.altWebInfo[altAvatar.productID].heroName))
					end
				)

				-- handle failure
				if (not success) then
					if (not GetCvarBool('ui_altAvatarListDebug')) then
						heroIcon:SetTexture("$checker")	-- checker board if not debug
					else 	-- debug and warning for all!
						heroIcon:SetTexture("/ui/icons/awards/award_deaths_l.tga")

						-- flash the icon
						local flash = function(flashIn, func)
							if (flashIn) then
								heroIcon:FadeIn(200)
							else
								heroIcon:FadeOut(200)
							end

							heroIcon:Sleep(200, function() func(not flashIn, func) end)
						end

						flash(false, flash)

						-- debug output
						TestEcho("\n^r^:Error populating the alt icon!")
						TestEcho("^r^:Error Message: ^*"..errorMessage)
						TestEcho("^r^:Name used to try and get the icon: ^y"..HoN_Store.altWebInfo[altAvatar.productID].heroName.."."..HoN_Store.altWebInfo[altAvatar.productID].altCode)
						err = true
					end
				elseif (GetCvarBool('ui_altAvatarListDebug')) then -- success with debug, remove any sleeps from failure
					heroIcon:Sleep(0, function() end)
					heroIcon:SetVisible(1)
				end

				-- dump info if there was an error
				if (GetCvarBool('ui_altAvatarListDebug') and err) then
					TestEcho("\n^r^:Dumping web and store_avatars info due to an error!")
					TestEcho("^y^:store_avatars")
					printTable(altAvatar)
					TestEcho("\n^y^:web")
					printTable(HoN_Store.altWebInfo[altAvatar.productID])
				end

				-- product name
				tempWidg = GetWidget("avatarProductName_"..i)
				if (HoN_Store.altWebInfo[altAvatar.productID].isPremium) then
					tempWidg:SetColor("#FFCC00")
				else
					tempWidg:SetColor("yellow")
				end
				local transText = Translate("mstore_product"..altAvatar.productID.."_name")
				tempWidg:SetFont(GetFontSizeForWidth(transText, tempWidg:GetWidth(), 14))
				tempWidg:SetText(transText)

				-- hero name
				tempWidg = GetWidget("avatarHeroName_"..i)
				if (HoN_Store.altWebInfo[altAvatar.productID].isPremium) then
					tempWidg:SetColor("#ffe680")
				else
					tempWidg:SetColor("#ffe9ad")
				end
				tempWidg:SetText(Translate("mstore_"..HoN_Store.altWebInfo[altAvatar.productID].heroName.."_name"))

				-- special icon
				if (altAvatar.limitedEdition or altAvatar.goldCollection or altAvatar.holidayEdition) then
					HoN_Store:SetSpecialIcon(GetWidget("avatarSpecialIconImage_"..i), altAvatar.limitedEdition, altAvatar.goldCollection, altAvatar.holidayEdition)
					GetWidget("avatarSpecialIcon_"..i):SetVisible(1)
				else
					GetWidget("avatarSpecialIcon_"..i):SetVisible(0)
				end

				-- CESet icon
				if (altAvatar.collectorsSet > 0) then
					HoN_Store:SetCEIcon(GetWidget("avatarCEIconImage_"..i), altAvatar.collectorsSet)
					GetWidget("avatarCEIcon_"..i):SetVisible(1)
				else
					GetWidget("avatarCEIcon_"..i):SetVisible(0)
				end

				-- ultimate icon
				GetWidget("avatarUltimate_"..i):SetVisible(altAvatar.ultimate)

				-- enhancement icon
				GetWidget("avatarEnhancement_"..i):SetVisible(altAvatar.hasEnhancement)

				-- owned
				GetWidget("avatarOwned_"..i):SetVisible(HoN_Store.altWebInfo[altAvatar.productID].owned)

				-- trim/frame stuff
				groupfcall("slot_premium_trim_"..i, function(_, widget, _) widget:SetVisible(HoN_Store.altWebInfo[altAvatar.productID].isPremium) end)
				groupfcall("slot_normal_trim_"..i, function(_, widget, _) widget:SetVisible(not HoN_Store.altWebInfo[altAvatar.productID].isPremium) end)
			end

			GetWidget("heroAvatarEntry_"..i):SetVisible(1)
		end
	end
end

function HoN_Store:RightClickAlt(index)
	HoN_Store:ClickAltAvatar(index)
	index = tonumber(index) + HoN_Store.altAvatarScroll

	if (HoN_Store.altListData[index]) then
		local avatar = HoN_Store.allAvatarsInfo[HoN_Store.altListData[index]]
		if ((not HoN_Store.altWebInfo[avatar.productID].owned) and (HoN_Store.altWebInfo[avatar.productID].purchasable)) then
			if (AtoB(interface:UICmd("CanAccessHeroProduct('"..HoN_Store.altWebInfo[avatar.productID].heroName.."')"))) then
				local goldCost = HoN_Store.altWebInfo[avatar.productID].goldCost
				local silverCost = HoN_Store.altWebInfo[avatar.productID].silverCost

				local eligIndex = tostring(avatar.productID)
				if (HoN_Store.productEligibility) and (HoN_Store.productEligibility[eligIndex]) then
					if (AtoB(HoN_Store.productEligibility[eligIndex].eligible)) then
						goldCost = HoN_Store.productEligibility[eligIndex].goldCost
						silverCost = HoN_Store.productEligibility[eligIndex].silverCost
					end
				end

				PlaySound('/shared/sounds/ui/ccpanel/button_click_02.wav')
				Set('_microStore_SelectedItem', 999)
				Set('microStoreID999', avatar.productID)
				Set('_microStore_SelectedID', avatar.productID)
				Set('microStoreLocalContent999', interface:UICmd("GetHeroIcon2PathFromProduct('"..HoN_Store.altWebInfo[avatar.productID].heroName.."."..HoN_Store.altWebInfo[avatar.productID].altCode.."')"))
				Set('microStorePrice999', goldCost)
				Set('microStorePremium999', false)
				Set('microStoreSilverPrice999', silverCost)
				Trigger('MicroStoreUpdateSelection')
				GetWidget("store_popup_confirm_purchase_choose"):DoEventN(0)
			else
				Set('microStore_heroSelectTarget', HoN_Store.altWebInfo[avatar.productID].heroName)
				GetWidget("store_popup_buyavatar_needhero"):DoEventN(0)
			end
		end
	end
end

function HoN_Store:ClickAltAvatar(index, allowReselect)
	index = tonumber(index) + HoN_Store.altAvatarScroll

	if ((HoN_Store.altListData[index]) and ((HoN_Store.altSelectedID ~= HoN_Store.altListData[index]) or allowReselect)) then
		PlaySound("/shared/sounds/ui/ccpanel/button_click_02.wav")
		local avatar = HoN_Store.allAvatarsInfo[HoN_Store.altListData[index]]
		HoN_Store.altSelectedID = avatar.productID

		local goldCost = HoN_Store.altWebInfo[avatar.productID].goldCost
		local silverCost = HoN_Store.altWebInfo[avatar.productID].silverCost

		local eligIndex = tostring(avatar.productID)
		if (HoN_Store.productEligibility) and (HoN_Store.productEligibility[eligIndex]) then
			if (AtoB(HoN_Store.productEligibility[eligIndex].eligible)) then
				goldCost = HoN_Store.productEligibility[eligIndex].goldCost
				silverCost = HoN_Store.productEligibility[eligIndex].silverCost
			end
		end

		local trialInfo = GetTrialInfo(HoN_Store.altWebInfo[avatar.productID].heroName, HoN_Store.altWebInfo[avatar.productID].altCode)
		local cardsTable = GetCardsInfo(HoN_Store.altWebInfo[avatar.productID].heroName, HoN_Store.altWebInfo[avatar.productID].altCode)

		if (HoN_Store.altWebInfo[avatar.productID].owned) or (not HoN_Store.altWebInfo[avatar.productID].purchasable) then
			GetWidget('storeAvatar_cards_panel'):SetVisible(false)
		else
			if (cardsTable and #cardsTable > 0) then
				GetWidget('storeAvatar_cards_panel'):SetVisible(true)
			else
				GetWidget('storeAvatar_cards_panel'):SetVisible(false)
			end
		end

		Trigger("populateAvatarDisplay",
			index,
			avatar.productID,
			HoN_Store.altWebInfo[avatar.productID].heroName,
			HoN_Store.altWebInfo[avatar.productID].altCode,
			goldCost,
			silverCost,
			tostring(HoN_Store.altWebInfo[avatar.productID].isPremium),
			tostring(HoN_Store.altWebInfo[avatar.productID].owned),
			avatar.definitionFolder,
			tostring(avatar.hasModel),
			tostring(avatar.hasTexture),
			tostring(avatar.hasEffects),
			tostring(avatar.hasSounds),
			tostring(avatar.hasAnims),
			tostring(HoN_Store.altWebInfo[avatar.productID].purchasable),
			tostring(avatar.limitedEdition),
			tostring(avatar.goldCollection),
			tostring(avatar.holidayEdition),
			avatar.collectorsSet,
			tostring(avatar.ultimate),
			tostring(avatar.hasEnhancement),
			trialInfo
			)


		HoN_Store:DisplayAvatarList()
	end
end

function HoN_Store:HoverAltSlot(index)
	HoN_Store.altHoveredIndex = tonumber(index)
	if (not HoN_Store.altListScrolling) then
		groupfcall('avatarHoverPieces'..index, function(_, widget, _) widget:DoEventN(0) end)
	end

	index = HoN_Store.altHoveredIndex + HoN_Store.altAvatarScroll
	interface:UICmd("GetHeroInfo('"..HoN_Store.altWebInfo[HoN_Store.altListData[index]].heroName.."');")
end

function HoN_Store:LeaveAltHover(index)
	HoN_Store.altHoveredIndex = nil
	groupfcall('avatarHoverPieces'..index, function(_, widget, _) widget:DoEventN(1) end)
end

function HoN_Store:ProgressiveAvatarLoad()
	local loaded = false
	if (not GetCvarBool("_entityDefinitionsLoaded")) then
		while ((not loaded) and (#HoN_Store.altLoadList > 0)) do
			local loadFolder = HoN_Store.altLoadList[1]
			if (HoN_Store.altDefsLoaded[loadFolder]) then
				table.remove(HoN_Store.altLoadList, 1)
			else
				HoN_Store.altDefsLoaded[loadFolder] = true
				interface:UICmd("LoadEntityDefinitionsFromFolder('"..loadFolder.."');")
				table.remove(HoN_Store.altLoadList, 1)
				loaded = true
			end
		end
	end

	if (loaded) then
		GetWidget("altAvatarSleepLoader"):Sleep(25, function() HoN_Store:ProgressiveAvatarLoad() end)
	else
		HoN_Store.altIconLoadPos = 1
		HoN_Store:ProgressiveAltIconLoad()
	end
end

function HoN_Store:ProgressiveAltIconLoad()
	local loaded = false
	while ((not loaded) and HoN_Store.altIconLoadPos and (HoN_Store.altIconLoadPos < HoN_Store.numAltAvatars)) do
		local widget = GetWidget("altAvatarSleepLoader")
		local info = HoN_Store.altWebInfo[HoN_Store.altAvatarsDisplayData[HoN_Store.altIconLoadPos]]

		widget:SetTexture(interface:UICmd("GetHeroIcon2PathFromProduct('"..info.heroName.."."..info.altCode.."')"))
		widget:SetTexture(GetEntityIconPath(info.heroName))

		HoN_Store.altIconLoadPos = HoN_Store.altIconLoadPos + 1
		loaded = true
	end

	if (loaded) then
		GetWidget("altAvatarSleepLoader"):Sleep(25, function() HoN_Store:ProgressiveAltIconLoad() end)
	else
		HoN_Store.altIconLoadPos = nil
	end
end

-- This comes from MicroStore results
-- when you click the alt button, it makes a web request, if the web request is for alts
-- we get this info back and this function will be ran (event5lua on the panel that watches results)
function HoN_Store:PopulateAltInfo(webIDs, webNames, webGold, webSilver, webPremium, webOwned, webBundle, webPurchasable)
	-- build the web info table
	HoN_Store.altWebInfo = {}
	local IDs = explode('|', webIDs)
	local names = explode('|', webNames)
	local gold = explode('|', webGold)
	local silver = explode('|', webSilver)
	local premium = explode('|', webPremium)
	local owned = explode('|', webOwned)
	local bundle = explode('|', webBundle)
	local purchasable = explode('|', webPurchasable)

	local productIDsFromWeb = {}

	for i,v in ipairs(IDs) do
		if (not AtoB(bundle[i])) then
			v = tonumber(v)
			local name = string.gsub(names[i], "aa.Hero_", "Hero_") -- knock off that aa. prefix
			local nameTable = explode('.', name)
			HoN_Store.altWebInfo[v] = {}
			HoN_Store.altWebInfo[v].heroName 		= nameTable[1]
			HoN_Store.altWebInfo[v].altCode 		= nameTable[2]
			HoN_Store.altWebInfo[v].goldCost 		= tonumber(gold[i])
			HoN_Store.altWebInfo[v].silverCost 		= tonumber(silver[i])
			HoN_Store.altWebInfo[v].isPremium 		= AtoB(premium[i])
			HoN_Store.altWebInfo[v].owned 			= AtoB(owned[i])
			HoN_Store.altWebInfo[v].purchasable 	= AtoB(purchasable[i])

			productIDsFromWeb[v] = true
		end
	end

	-- gets all alt info from the panels
	if (not HoN_Store.allAvatarsInfo) then
		HoN_Store.allAvatarsInfo = {}
		groupfcall("storeAltAvatarInfoPanels", function(_, widget, _) widget:DoEvent() end)
	end

	-- build the list of product IDs we are going to display
	if (not HoN_Store.altAvatarsDisplayData) then
		HoN_Store.altAvatarsDisplayData = {}
		HoN_Store:BuildDisplayList()
	else
		-- check for alts we need to remove, and prune existing alts from the web id list
		local offset = 1
		while (offset <= #HoN_Store.altAvatarsDisplayData) do
			if (not HoN_Store.altWebInfo[HoN_Store.altAvatarsDisplayData[offset]]) then
				table.remove(HoN_Store.altAvatarsDisplayData, offset)
			else
				productIDsFromWeb[HoN_Store.altAvatarsDisplayData[offset]] = nil
				offset = offset + 1
			end
		end

		-- now cycle through everything left in productIDsFromWeb, these are all newly activated alts
		-- if we have panel info for them, add them to the display list
		for pID, _ in pairs(productIDsFromWeb) do
			if (HoN_Store.allAvatarsInfo[pID]) then
				table.insert(HoN_Store.altAvatarsDisplayData, pID)
			end
		end
	end
	-- update num alts
	HoN_Store.numAltAvatars = #HoN_Store.altAvatarsDisplayData

	-- redo the alt list from scratch each time we get web info
	-- HoN_Store.altAvatarsDisplayData = {}
	-- groupfcall("storeAltAvatarInfoPanels", function(_, widget, _) widget:DoEvent() end)

	-- populate/update the list as needed
	if (HoN_Store.altListNeedsReset) then
		HoN_Store:ResetAltList()
		HoN_Store:CreateAltList(HoN_Store.altSearchFilter, HoN_Store.altFilter)
		HoN_Store:UpdateAltScroller()

		-- update the drop downs
		-- makes the values in these refresh
		GetWidget("avatar_filter_list"):SetSelectedItemByIndex(HoN_Store.altFilter)
		GetWidget("avatar_filter_allhero_list"):SetSelectedItemByIndex(HoN_Store.altFilter)
		GetWidget("avatar_sort_list"):SetSelectedItemByIndex(HoN_Store.altSort)

		local altString = GetCvarString("microStore_avatarSelectTarget")
		local selectIndex = nil
		if (altString and altString ~= "") then
			selectIndex = HoN_Store:GetAltIndex(altString)
		end
		if (not selectIndex) then selectIndex = 1 end

		-- clear out the target so it doesn't get set next time
		Set("microStore_avatarSelectTarget", "")

		-- check that the entity is loaded for what we are selecting, if not we need to load it otherwise
		-- the model area won't be populated correctly
		if ((not GetCvarBool("_entityDefinitionsLoaded")) and (not HoN_Store.altDefsLoaded[HoN_Store.allAvatarsInfo[HoN_Store.altListData[selectIndex]].definitionFolder])) then
			interface:UICmd("LoadEntityDefinitionsFromFolder('"..HoN_Store.allAvatarsInfo[HoN_Store.altListData[selectIndex]].definitionFolder.."');")
		end

		HoN_Store:SelectAltIndex(selectIndex)
		HoN_Store:DisplayAvatarList()

		-- only load once
		if (HoN_Store.needLoading) then
			HoN_Store:ProgressiveAvatarLoad()
			HoN_Store.needLoading = false
		end

		HoN_Store.altListNeedsReset = false
	else
		HoN_Store:CreateAltList(HoN_Store.altSearchFilter, HoN_Store.altFilter)

		HoN_Store:UpdateAltScroller()

		-- reselect the alt to update the purchase button
		if (HoN_Store.altSelectedID) then
			local index = HoN_Store:GetAltIndex(tostring(HoN_Store.altSelectedID))

			if (index) then
				HoN_Store:SelectAltIndex(index)
			else
				HoN_Store:ClickAltAvatar(2, true)
			end
		end

		HoN_Store:DisplayAvatarList()
	end
end

-- from 'altAvatarPreviewPanel' event, called above in groupcall
-- 1 = ID
-- 2 = Hero Definition Folder
-- 3 = Has Model?
-- 4 = Has Texture?
-- 4 = Has Sounds?
-- 6 = Has Animations?
-- 7 = Gold Collection?
-- 8 = Limited Edition?
-- 9 = Holiday Edition?
-- 10= Collectors Edition Set #
-- 11= Hero Entry ID (for release date sort)
-- 12= Ultimate Avatar?
function HoN_Store:RegisterAltAvatar(...)
	local altInfo = {}
	altInfo.productID 		 = arg[1] -- save this, then just index into the webinfo table,
	altInfo.definitionFolder = arg[2] -- thus we never need to rebuild this table (only the web one)
	altInfo.hasModel		 = arg[3]
	altInfo.hasTexture		 = arg[4]
	altInfo.hasEffects 		 = arg[5]
	altInfo.hasSounds		 = arg[6]
	altInfo.hasAnims		 = arg[7]
	altInfo.goldCollection 	 = arg[8]
	altInfo.limitedEdition 	 = arg[9]
	altInfo.holidayEdition 	 = arg[10]
	altInfo.collectorsSet 	 = arg[11]
	altInfo.heroEntryID 	 = arg[12]
	altInfo.ultimate 		 = arg[13]
	altInfo.hasEnhancement	 = arg[14]
	altInfo.hasNewAnimations = arg[15]

	HoN_Store.allAvatarsInfo[arg[1]] = altInfo
end

function HoN_Store:BuildDisplayList()
	-- if we have both panel and web info for the alt
	if (HoN_Store.allAvatarsInfo and HoN_Store.altWebInfo) then
		for id, t in pairs(HoN_Store.allAvatarsInfo) do
			if (HoN_Store.altWebInfo[id]) then
				table.insert(HoN_Store.altAvatarsDisplayData, id)

				-- list of entities to load
				if ((not GetCvarBool("_entityDefinitionsLoaded")) and (HoN_Store.altDefsLoaded[HoN_Store.allAvatarsInfo[id].definitionFolder] == nil)) then
					HoN_Store.altDefsLoaded[HoN_Store.allAvatarsInfo[id].definitionFolder] = false
					table.insert(HoN_Store.altLoadList, HoN_Store.allAvatarsInfo[id].definitionFolder)
				end
			end
		end
	end
end

function HoN_Store:InputBoxUpdateHero(input)
	if (input) then
		local length = string.len(input)
		if (length == 0 ) then
			--HoN_Store.searchString = nil
			groupfcall('heroHeroEntries', function(_, widget, _) widget:SetVisible(1) widget:DoEventN(7) end)
		else
			--HoN_Store.searchString = input
			groupfcall('heroHeroEntries', function(_, widget, _) widget:SetVisible(0) end)

			local foundCount = 0
			for productID, avatarTable in pairs(HoN_Store.heroTable) do
				if (string.find(string.lower(Translate('mstore_product' .. productID .. '_name')), string.lower(input)) or string.find(string.lower(avatarTable.hero), string.lower(input)) or string.find(string.lower(GetEntityDisplayName(avatarTable.hero)), string.lower(input))) then
					if (GetWidget('heroListHeroEntry_' .. productID, nil, true)) then
						GetWidget('heroListHeroEntry_' .. productID):SetVisible(true)
						GetWidget('heroListHeroEntry_' .. productID):SetY(foundCount * GetWidget('heroListHeroEntry_' .. productID):GetHeight())
						foundCount = foundCount + 1
					end
				end
			end

			Trigger('heroListTargViewIndex', 0)

		end
	end
end

function HoN_Store:InstantiateHeroListEntry(param0, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16, param17, param18, param19, param20)

	local heroDisplayListContainer = GetWidget('heroDisplayListContainer')
	local mstoreHeroSortMode = GetCvarInt('mstoreHeroSortMode', true)
	local instantiateHeroIndex = GetCvarInt('instantiateHeroIndex', true)

	--println('instantiateHeroIndex: ' .. instantiateHeroIndex .. ' | floor(tonumber(param0) - 1):' .. floor(tonumber(param0) - 1) .. ' | param1:' .. param1 .. ' | param2:' .. param2  .. ' | param3:' .. param3)

	HoN_Store.latestProducts = HoN_Store.latestProducts or {}
	HoN_Store.latestProducts.hero = HoN_Store.latestProducts.hero or 0

	if NotEmpty(GetCvarString('microStore_heroSelectTarget')) and (tonumber(param1) >= 0) and (GetCvarString('microStore_heroSelectTarget') == param2) then
		Set('heroListDefaultIndex', tostring(instantiateHeroIndex), 'int')
		Set('microStore_heroSelectTarget', 'found', 'string')
	elseif (GetCvarString('microStore_heroSelectTarget') ~= 'found') then
		if tonumber(param1) >= HoN_Store.latestProducts.hero then
			HoN_Store.latestProducts.hero = tonumber(param1)
			Set('heroListDefaultIndex', tostring(instantiateHeroIndex), 'int')
		end
	end

	if (instantiateHeroIndex) then

		local backer = 'hero_backer_regular'
		if AtoB(param5) then
			backer = 'hero_backer_premium'
		end

		HoN_Store.heroTable[tonumber(param1)]	=	{hero = param2}

		local trialInfo = GetTrialInfo(param2, '')
		local index = floor(tonumber(param0) - 1)

		heroDisplayListContainer:Instantiate(
			'heroListHeroEntry',
			'product_id', param1,
			'hero', param2,
			'cost', param3,
			'silver_cost', param4,
			'premium', param5,
			'index', index,
			'owned', param6,
			'backer', backer,
			'sourceItemIndex', param8,
			'trial', trialInfo
		)

		heroDisplayListContainer:SetHeight( (interface:GetHeightFromString('5.85h') * 10) * (tonumber(param0) - 1) )
		Cvar.CreateCvar('instantiateHeroIndex', 'int', instantiateHeroIndex + 1)

		if NotEmpty(trialInfo) and (param6 == "0") then
			GetWidget('heroTrial_'..index):SetVisible(true)
			GetWidget('heroListHeroCards_'..index):SetVisible(true)
		else
			GetWidget('heroTrial_'..index):SetVisible(false)

			local cardsTable = GetCardsInfo(param2, '')
			if (cardsTable) and (#cardsTable > 0) and (param6 == "0") then
				GetWidget('heroListHeroCards_'..index):SetVisible(true)
			else
				GetWidget('heroListHeroCards_'..index):SetVisible(false)
			end
		end
	end
end
interface:RegisterWatch('instantiateHeroListEntry', function(...) HoN_Store.InstantiateHeroListEntry(...) end)

local function UpdateEAPHeroModels(selectedBundle)
	if (selectedBundle == 3) then
		-- product eliigibility
		local goldCost = tonumber(Store.eapBundleGoldCost3)
		local silverCost = tonumber(Store.eapBundleSilverCost3)

		local eligIndex = tostring(Store.eapBundleProductID3)
		if (HoN_Store.productEligibility) and (HoN_Store.productEligibility[eligIndex]) then
			if (AtoB(HoN_Store.productEligibility[eligIndex].eligible)) then
				goldCost = HoN_Store.productEligibility[eligIndex].goldCost
				silverCost = HoN_Store.productEligibility[eligIndex].silverCost
			end
		end

		GetWidget('mstore_eap_selected_bundle_label'):SetText(Translate('mstore_eap_bundle_3'))
		GetWidget('store_eap_left_hero_model'):DoEventN(1)
		GetWidget('store_eap_center_hero_model'):DoEventN(1)
		GetWidget('store_eap_right_hero_model'):DoEventN(1)
		Set('ui_eapSelectedBundleID', Store.eapBundleProductID3, 'string')
		GetWidget('mstore_eap_purchase_cost_label'):SetText(goldCost)
		Set('ui_eapSelectedBundleGold', goldCost, 'string')
		Set('ui_eapSelectedBundleSilver', silverCost, 'string')
		GetWidget('ms_eap_left_star_1'):SetVisible(1)
		GetWidget('ms_eap_right_star_1'):SetVisible(1)
		GetWidget('ms_eap_left_star_2'):SetVisible(0)
		GetWidget('ms_eap_right_star_2'):SetVisible(0)
	elseif (selectedBundle == 2) then
		-- product eliigibility
		local goldCost = tonumber(Store.eapBundleGoldCost2)
		local silverCost = tonumber(Store.eapBundleSilverCost2)

		local eligIndex = tostring(Store.eapBundleProductID2)
		if (HoN_Store.productEligibility) and (HoN_Store.productEligibility[eligIndex]) then
			if (AtoB(HoN_Store.productEligibility[eligIndex].eligible)) then
				goldCost = HoN_Store.productEligibility[eligIndex].goldCost
				silverCost = HoN_Store.productEligibility[eligIndex].silverCost
			end
		end

		GetWidget('mstore_eap_selected_bundle_label'):SetText(Translate('mstore_eap_bundle_2'))
		GetWidget('store_eap_left_hero_model'):DoEventN(1)
		GetWidget('store_eap_center_hero_model'):DoEventN(1)
		GetWidget('store_eap_right_hero_model'):DoEventN(0)
		GetWidget('ms_eap_left_star_1'):SetVisible(1)
		GetWidget('ms_eap_right_star_1'):SetVisible(0)
		GetWidget('ms_eap_left_star_2'):SetVisible(0)
		GetWidget('ms_eap_right_star_2'):SetVisible(1)
		Set('ui_eapSelectedBundleID', Store.eapBundleProductID2, 'string')
		GetWidget('mstore_eap_purchase_cost_label'):SetText(goldCost)
		Set('ui_eapSelectedBundleGold', goldCost, 'string')
		Set('ui_eapSelectedBundleSilver', silverCost, 'string')
	elseif (selectedBundle == 1) then
		-- product eliigibility
		local goldCost = tonumber(Store.eapBundleGoldCost1)
		local silverCost = tonumber(Store.eapBundleSilverCost2)

		local eligIndex = tostring(Store.eapBundleProductID1)
		if (HoN_Store.productEligibility) and (HoN_Store.productEligibility[eligIndex]) then
			if (AtoB(HoN_Store.productEligibility[eligIndex].eligible)) then
				goldCost = HoN_Store.productEligibility[eligIndex].goldCost
				silverCost = HoN_Store.productEligibility[eligIndex].silverCost
			end
		end

		GetWidget('mstore_eap_selected_bundle_label'):SetText(Translate('mstore_eap_bundle_1'))
		GetWidget('store_eap_left_hero_model'):DoEventN(0)
		GetWidget('store_eap_center_hero_model'):DoEventN(1)
		GetWidget('store_eap_right_hero_model'):DoEventN(0)
		GetWidget('ms_eap_left_star_1'):SetVisible(0)
		GetWidget('ms_eap_right_star_1'):SetVisible(0)
		GetWidget('ms_eap_left_star_2'):SetVisible(1)
		GetWidget('ms_eap_right_star_2'):SetVisible(1)
		Set('ui_eapSelectedBundleID', Store.eapBundleProductID1, 'string')
		GetWidget('mstore_eap_purchase_cost_label'):SetText(goldCost)
		Set('ui_eapSelectedBundleGold', goldCost, 'string')
		Set('ui_eapSelectedBundleSilver', silverCost, 'string')
	end
end

local function ResetEAPScreenStatus()
	HoN_Store.selectedEAPBundle = 3
	UpdateEAPHeroModels(HoN_Store.selectedEAPBundle)
	GetWidget('store_eap_header_left'):SetVisible(1)
	GetWidget('store_eap_header_right'):SetVisible(1)
	GetWidget('store_eap_star_left'):SetVisible(1)
	GetWidget('store_eap_star_right'):SetVisible(1)
	GetWidget('store_eap_footer_center'):SetVisible(1)
end

local function populateEAPItem(_, index, heroProductID, entityName, timestamp, goldCost, silverCost, alt1, alt2, alt1productid, alt1name, alt1goldcost, alt1silvercost, alt2productid, alt2name, alt2goldcost, alt2silvercost)
	GetWidget('store_eap_header_left_label_1'):SetText(alt1name)
	GetWidget('store_eap_header_right_label_1'):SetText(alt2name)

	Store.eapBundleGoldCost1 = goldCost
	Store.eapBundleGoldCost2 = alt1goldcost
	Store.eapBundleGoldCost3 = alt2goldcost
	Store.eapBundleSilverCost1 = silverCost
	Store.eapBundleSilverCost2 = alt1silvercost
	Store.eapBundleSilverCost3 = alt2silvercost
	Store.eapBundleProductID1 = heroProductID
	Store.eapBundleProductID2 = alt1productid
	Store.eapBundleProductID3 = alt2productid
	Store.eapEntityName1 = entityName
	Store.eapEntityName2 = entityName.."."..alt1
	Store.eapEntityName3 = entityName.."."..alt2

	-- product eliigibility
	local goldCost = tonumber(goldCost)
	local silverCost = tonumber(silverCost)

	local eligIndex = tostring(heroProductID)
	if (HoN_Store.productEligibility) and (HoN_Store.productEligibility[eligIndex]) then
		if (AtoB(HoN_Store.productEligibility[eligIndex].eligible)) then
			goldCost = HoN_Store.productEligibility[eligIndex].goldCost
			silverCost = HoN_Store.productEligibility[eligIndex].silverCost
		end
	end

	Set('ui_eapSelectedBundleID', heroProductID, 'string')
	Set('ui_eapSelectedBundleGold', goldCost, 'string')
	Set('ui_eapSelectedBundleSilver', silverCost, 'string')

	ResetEAPScreenStatus()

	if (entityName) and NotEmpty(entityName) then
		local iconPath = GetEntityIconPath(entityName)
		if (iconPath) then
			PlaySound(string.sub(iconPath, 1, -15) .. '/sounds/voice/hero_select.wav')
		end
	end
end
interface:RegisterWatch('populateEAPItem', populateEAPItem)

function Store.HoverBundle(sourceWidget, index)
	UpdateEAPHeroModels(index)
end

function Store.RestoreHoverBundle(sourceWidget, index)
	UpdateEAPHeroModels(HoN_Store.selectedEAPBundle)
end

function Store.SelectBundle(sourceWidget, index)
	HoN_Store.selectedEAPBundle = tonumber(index)
	UpdateEAPHeroModels(HoN_Store.selectedEAPBundle)
	--GetWidget('ms_eap_above_purchase_label'):SetText(Translate('mstore_eap_bundle_'..index))
end

function interface:HoNStoreF(func, ...)
  print(HoNStoreF[func](self, ...))
end

function Store.ShowPromo(sourceWidget)
	if (sourceWidget) then
		sourceWidget:SetVisible(1)
	end
	if GetWidget('promo_mainarea', nil, true) then
		GetWidget('promo_mainarea'):DoEventN(0)
	end
	if GetWidget('modelpanel_av1', nil, true) then
		GetWidget('modelpanel_av1'):DoEventN(0)
	end
	if GetWidget('modelpanel_av2', nil, true) then
		GetWidget('modelpanel_av2'):DoEventN(0)
	end
	if GetWidget('modelpanel_av3', nil, true) then
		GetWidget('modelpanel_av3'):DoEventN(0)
	end
	if GetWidget('modelpanel_av4', nil, true) then
		GetWidget('modelpanel_av4'):DoEventN(0)
	end
	if GetWidget('modelpanel_av5', nil, true) then
		GetWidget('modelpanel_av5'):DoEventN(0)
	end
	if GetWidget('promo_modelpanel1', nil, true) then
		GetWidget('promo_modelpanel1'):DoEventN(0)
	end
	if GetWidget('promo_modelpanel1_cover', nil, true) then
		GetWidget('promo_modelpanel1_cover'):DoEventN(0)
	end
	if GetWidget('promo_modelpanel2', nil, true) then
		GetWidget('promo_modelpanel2'):DoEventN(0)
	end
	if GetWidget('promo_modelpanel2_cover', nil, true) then
		GetWidget('promo_modelpanel2_cover'):DoEventN(0)
	end
	if GetWidget('promo_botbar', nil, true) then
		GetWidget('promo_botbar'):DoEventN(0)
	end
	if GetWidget('promo_countdown_controller', nil, true) then
		GetWidget('promo_countdown_controller'):DoEventN(0)
	end
	if GetWidget('promo_description1', nil, true) then
		GetWidget('promo_description1'):DoEventN(0)
	end
end

function Store.HidePromo(sourceWidget)
	if (sourceWidget) then
		sourceWidget:Sleep(750, function() sourceWidget:SetVisible(0) end)
	end
	if GetWidget('promo_mainarea', nil, true) then
		GetWidget('promo_mainarea'):DoEventN(1)
	end
	if GetWidget('modelpanel_av1', nil, true) then
		GetWidget('modelpanel_av1'):DoEventN(1)
	end
	if GetWidget('modelpanel_av2', nil, true) then
		GetWidget('modelpanel_av2'):DoEventN(1)
	end
	if GetWidget('modelpanel_av3', nil, true) then
		GetWidget('modelpanel_av3'):DoEventN(1)
	end
	if GetWidget('modelpanel_av4', nil, true) then
		GetWidget('modelpanel_av4'):DoEventN(1)
	end
	if GetWidget('modelpanel_av5', nil, true) then
		GetWidget('modelpanel_av5'):DoEventN(1)
	end
	if GetWidget('promo_modelpanel1', nil, true) then
		GetWidget('promo_modelpanel1'):DoEventN(1)
	end
	if GetWidget('promo_modelpanel1_cover', nil, true) then
		GetWidget('promo_modelpanel1_cover'):DoEventN(1)
	end
	if GetWidget('promo_modelpanel2', nil, true) then
		GetWidget('promo_modelpanel2'):DoEventN(1)
	end
	if GetWidget('promo_modelpanel2_cover', nil, true) then
		GetWidget('promo_modelpanel2_cover'):DoEventN(1)
	end
	if GetWidget('promo_botbar', nil, true) then
		GetWidget('promo_botbar'):DoEventN(1)
	end
	if GetWidget('promo_countdown_controller', nil, true) then
		GetWidget('promo_countdown_controller'):DoEventN(1)
	end
	if GetWidget('promo_description1', nil, true) then
		GetWidget('promo_description1'):DoEventN(1)
	end
end

function Store.ShowCountdown(sourceWidget, productID)
	if GetCvarInt('promo_currentQuanity', true) then
		if GetWidget('store_promo_unit_remain_label', nil, true) then
			GetWidget('store_promo_unit_remain_label'):SetText(FtoA(GetCvarInt('promo_currentQuanity'), 0, 2, ','))
		end
	end
	if (productID) and tonumber(productID) and (tonumber(productID) > 0) and (tonumber(productID) < 100000) then
		interface:UICmd([[SubmitForm('StorePromoUnitQuantity', 'product_id', ]] .. productID ..[[, 'request_code', '9', 'cookie', GetCookie(), 'account_id', GetAccountID(), 'hosttime', HostTime);]])
	end
end

local function StorePromoUnitQuantityResult(_, param0, param1, param2, param3, param4, quantityRemaining)
	if (quantityRemaining) and tonumber(quantityRemaining) then
		if (tonumber(quantityRemaining) == 0) then
			if GetWidget('store_promo_unit_remain_label', nil, true) then
				GetWidget('store_promo_unit_remain_label'):SetText('SOLD OUT!')
			end
		elseif (tonumber(quantityRemaining) >= 0) and (tonumber(quantityRemaining) <= 1000000) then
			if GetWidget('store_promo_unit_remain_label', nil, true) then
				GetWidget('store_promo_unit_remain_label'):SetText(FtoA(tonumber(quantityRemaining), 0, 2, ','))
			end
		end
	end
end
interface:RegisterWatch('StorePromoUnitQuantityResult', StorePromoUnitQuantityResult)

function Store.ShowRedeemCodeWindow()
	GetWidget('store_redeem_text_1'):SetVisible(1)
	GetWidget('store_redeem_text_2'):SetVisible(0)
	GetWidget('store_redeem_text_3'):SetVisible(0)
end

function Store.ShowRedeemCodeWindowSuccess()
	GetWidget('store_redeem_text_1'):SetVisible(0)
	GetWidget('store_redeem_text_2'):SetVisible(1)
	GetWidget('store_redeem_text_3'):SetVisible(0)
	GetWidget('store_popup_redeem_code'):DoEventN(0)
end

function Store.ShowRedeemCodeWindowFailure()
	GetWidget('store_redeem_text_1'):SetVisible(0)
	GetWidget('store_redeem_text_2'):SetVisible(0)
	GetWidget('store_redeem_text_3'):SetVisible(1)
	GetWidget('store_popup_redeem_code'):DoEventN(0)
end

function Store.SubmitRedeemCodeForm(redeemCodeString)
	if (redeemCodeString) and NotEmpty(redeemCodeString) then
		groupfcall('ms_reward_temporary_widgets', function(index, widget, groupName) widget:Destroy() end)
		lib_Card:HideCardception()
		interface:UICmd([[SubmitForm('StoreRedeemPromoCode', 'code', ']] .. EscapeString(redeemCodeString) ..[[', 'request_code', '10', 'cookie', GetCookie(), 'account_id', GetAccountID(), 'hosttime', HostTime);]])
		GetWidget('redeem_code_friendnick_textbox'):UICmd([[SetInputLine('')]])
	end
end

local function InstantiateEarnedRewards(newlyEarnedRewards)
	if (#newlyEarnedRewards > 0) then
		lib_Card:DisplayCardception(newlyEarnedRewards)
	end
end

function Store.StoreRedeemPromoCodeResult(responseCode, popupCode, errorCode, redeemed)
	local greatSuccess = false
	local newlyEarnedRewards

	groupfcall('ms_reward_temporary_widgets', function(index, widget, groupName) widget:Destroy() end)
	lib_Card:HideCardception()

	if (responseCode == '0') and (popupCode == '6') then

		local redeemedTable = explode(',', redeemed)

		if (redeemedTable) then
			newlyEarnedRewards = {}

			if (redeemedTable[1]) and NotEmpty(redeemedTable[1]) and (tonumber(redeemedTable[1])) and (tonumber(redeemedTable[1]) > 0) then	-- gold
				greatSuccess = true

				table.insert(newlyEarnedRewards, {
					HEADER 			= Translate('mstore_reedeem_code_success_1'),
					SUBHEADER 		= '',
					ICON 			= '/ui/fe2/store/gold_coins.tga',
					TYPE 			= 'earned',
					STARTDATE 		= '',
					ENDDATE 		= '',
					CHECKMARK 		= 'true',
					COMPLETION 		= '100%',
					REWARDINFO1 	= string.upper(Translate('match_stat_rewards_has_item')),
					REWARDINFO2 	= Translate('match_stat_rewards_text_gold_prize', 'value', redeemedTable[1]),
					REWARDINFO3 	= '',
					TEMPLATE 		= 'ms_rewards_reward_page_card_template',
					TIME_AWARDED	= '',
				})

			end

			if (redeemedTable[2]) and NotEmpty(redeemedTable[2]) and (tonumber(redeemedTable[2])) and (tonumber(redeemedTable[2]) > 0) then	-- silver
				greatSuccess = true

				table.insert(newlyEarnedRewards, {
					HEADER 			= Translate('mstore_reedeem_code_success_1'),
					SUBHEADER 		= '',
					ICON 			= '/ui/fe2/store/silver_coins.tga',
					TYPE 			= 'earned',
					STARTDATE 		= '',
					ENDDATE 		= '',
					CHECKMARK 		= 'true',
					COMPLETION 		= '100%',
					REWARDINFO1 	= string.upper(Translate('match_stat_rewards_has_item')),
					REWARDINFO2 	= Translate('match_stat_rewards_text_silver_prize', 'value', redeemedTable[2]),
					REWARDINFO3 	= '',
					TEMPLATE 		= 'ms_rewards_reward_page_card_template',
					TIME_AWARDED	= '',
				})

			end

			if (redeemedTable[3]) and NotEmpty(redeemedTable[3]) then	-- product ID

				local productInfo = explode('~', redeemedTable[3])

				if  (tonumber(productInfo[1])) and (tonumber(productInfo[1]) > 0) then

					greatSuccess = true

					table.insert(newlyEarnedRewards, {
						HEADER 			= Translate('mstore_reedeem_code_success_1'),
						SUBHEADER 		= '',
						ICON 			= productInfo[2] or '$invis',
						TYPE 			= 'earned',
						STARTDATE 		= '',
						ENDDATE 		= '',
						CHECKMARK 		= 'true',
						COMPLETION 		= '100%',
						REWARDINFO1 	= string.upper(Translate('match_stat_rewards_has_item')),
						REWARDINFO2 	= string.upper(TranslateOrNil('mstore_product'..productInfo[1]..'_name') or ''),
						REWARDINFO3 	= TranslateOrNil('mstore_product'..productInfo[1]..'_desc') or '',
						TEMPLATE 		= 'ms_rewards_reward_page_card_template',
						TIME_AWARDED	= '',
					})

				end

			end

		end

	else
		println('^r StoreRedeemPromoCodeResult Error: popupCode = ' .. tostring(popupCode) .. ' | responseCode = ' .. tostring(responseCode) )
	end

	if (greatSuccess) and (newlyEarnedRewards) and (#newlyEarnedRewards > 0) then
		Store.ShowRedeemCodeWindowSuccess()
		InstantiateEarnedRewards(newlyEarnedRewards)

	elseif (errorCode) and NotEmpty(errorCode) then
		Store.ShowRedeemCodeWindowFailure()
		GetWidget('store_redeem_text_3_1'):SetText(Translate('mstore_error' .. errorCode))
	else
		Store.ShowRedeemCodeWindowFailure()
		GetWidget('store_redeem_text_3_1'):SetText(Translate('mstore_reedeem_code_error_2'))
	end

end
interface:RegisterWatch('StoreRedeemPromoCodeResult', function(_, ...) Store.StoreRedeemPromoCodeResult(...) end)

function HoN_Store:SetCEIcon(image, collectorsEditionSet)
	if ((collectorsEditionSet > 0) and collectorsEditionSetImages[collectorsEditionSet]) then
		image:SetTexture(collectorsEditionSetImages[collectorsEditionSet])
		return true
	end
	return false
end

function HoN_Store:SetSpecialIcon(image, limitedEdition, goldCollection, holidayEdition)
	local imagePath = nil

	if (limitedEdition) then
		imagePath = "/ui/fe2/store/avatars_display/ico_limitededition.tga"
	elseif (goldCollection) then
		imagePath = "/ui/fe2/store/avatars_display/ico_goldcollection.tga"
	elseif (holidayEdition) then
		imagePath = "/ui/fe2/store/avatars_display/ico_holidayedition.tga"
	end

	if (imagePath) then
		image:SetTexture(imagePath)
	end
end

function HoN_Store:HoverCE(slot)
	local altAvatar = HoN_Store.allAvatarsInfo[HoN_Store.altListData[slot+HoN_Store.altAvatarScroll]]

	if (altAvatar.collectorsSet > 0) then
		local title, body = nil, nil

		title = "mstore_collectorsEditionSet"..tostring(altAvatar.collectorsSet)

            -- add owned/purchasable
            if ((HoN_Store.altWebInfo[altAvatar.productID].owned) and title) then title = title.."_owned"
            elseif ((not HoN_Store.altWebInfo[altAvatar.productID].purchasable) and title) then title = title.."_expired" end
            if (title) then body = title.."_tip" end

            if (title and body) then
                Trigger('genericMainFloatingTip', 'true', '23h', '', title, body, '', '', '', '')
            end
        end
end

function HoN_Store:HoverSpecial(slot)
	local altAvatar = HoN_Store.allAvatarsInfo[HoN_Store.altListData[slot+HoN_Store.altAvatarScroll]]
	local title, body = nil, nil

	-- get the base
	if (altAvatar.limitedEdition) then
		title = "mstore_limitededition"
	elseif (altAvatar.goldCollection) then
		title = "mstore_goldcollection"
	elseif (altAvatar.holidayEdition) then
		title = "mstore_holidayedition"
	end

	-- add owned/purchasable
	if ((HoN_Store.altWebInfo[altAvatar.productID].owned) and title) then title = title.."_owned"
	elseif ((not HoN_Store.altWebInfo[altAvatar.productID].purchasable) and title) then title = title.."_expired" end
	if (title) then body = title.."_tip" end

	if (title and body) then
		Trigger('genericMainFloatingTip', 'true', '23h', '', title, body, '', '', '', '')
	end
end

function HoN_Store:HoverTrial(slot)
	local altAvatar = HoN_Store.allAvatarsInfo[HoN_Store.altListData[slot+HoN_Store.altAvatarScroll]]
	local trialInfo = GetTrialInfo(HoN_Store.altWebInfo[altAvatar.productID].heroName, HoN_Store.altWebInfo[altAvatar.productID].altCode)
	if (NotEmpty(trialInfo)) then
		Trigger('trial_store_info', trialInfo)
		GetWidget('store_trial_tip'):SetVisible(true)
	end
end

local function GetUnavailableTooltipLabels()
	local title, body1, body2, body3 = nil, nil, "", ""

	if (HoN_Store.productEligibility) and (HoN_Store.productEligibility[HoN_Store.puchaseUnavailableConditions[5]]) then
		title = "mstore_elig"
		body1 = "mstore_elig_tip"
		body3 = ""

		if (HoN_Store.productEligibility[HoN_Store.puchaseUnavailableConditions[5]].eligible) then
			if (HoN_Store.productEligibility[HoN_Store.puchaseUnavailableConditions[5]].eligbleID) then
				local requireTable = nil

				if (HoN_Store.productEligibility[HoN_Store.puchaseUnavailableConditions[5]].requiredProducts) then
					requireTable = explode(';', HoN_Store.productEligibility[HoN_Store.puchaseUnavailableConditions[5]].requiredProducts)
				else
					requireTable = explode('|', Translate('mstore_elig_requirements_' .. HoN_Store.productEligibility[HoN_Store.puchaseUnavailableConditions[5]].productID))
				end

				if (requireTable) then
					body3 = Translate("mstore_elig_required")
					for index, requiredProductID in pairs(requireTable) do
						local translateReqProductID = Translate('mstore_product'..requiredProductID..'_name')
						if (translateReqProductID ~= 'mstore_product'..requiredProductID..'_name') then
							body3 = body3 .. '\n' .. translateReqProductID
						end
					end
				end
			end
		end

		body2 = Translate('mstore_elig_package_'..HoN_Store.productEligibility[HoN_Store.puchaseUnavailableConditions[5]].productID)
		if (body2 == ('mstore_elig_package_'..HoN_Store.productEligibility[HoN_Store.puchaseUnavailableConditions[5]].productID)) then
			body2 = ""
		end
	else
		if (HoN_Store.puchaseUnavailableConditions[1]) then -- LE
			title = "mstore_limitededition"
		elseif (HoN_Store.puchaseUnavailableConditions[2]) then -- GC
			title = "mstore_goldcollection"
		elseif (HoN_Store.puchaseUnavailableConditions[3]) then -- HE
			title = "mstore_holidayedition"
		elseif (HoN_Store.puchaseUnavailableConditions[4] > 0) then --CE
			title = "mstore_collectorsEditionSet"..tostring(HoN_Store.puchaseUnavailableConditions[4])
		else
			title = "general_unavailable"
			body1 = true -- reuse some variables
		end

		if (not body1 and title) then
			title = title.."_expired"
			body1 = title.."_tip"
		else
			body1 = "mstore_unavailable_tip"
		end
	end
	return title, body1, body2, body3
end

function HoN_Store:HoverUnavailableButton()
	local title, body1, body2, body3 = GetUnavailableTooltipLabels()

	if (title and body1) then
		Trigger('genericMainFloatingTip', 'true', '23h', '', title, body1, body2, body3, '', '')
	end
end

function HoN_Store:SetUnavailableConditions(limitedEdition, goldCollection, holidayEdition, collectorsEditionSet, productID)
	HoN_Store.puchaseUnavailableConditions = {AtoB(limitedEdition), AtoB(goldCollection), AtoB(holidayEdition), tonumber(collectorsEditionSet), tostring(productID)}
end

-- Stuff for hero list alt scroller, because avatars
function HoN_Store:RegisterAltForList(heroId, hero, altCode, altId, fullProductName)
	--storeHeroListAvatarIcon
	if (AtoB(interface:UICmd("IsAvatarProductEnabled('"..fullProductName.."')"))) then
		-- new list, create it and add the default avatar as [1]
		if (not HoN_Store.heroDisplayAltList) then
			HoN_Store.heroDisplayAltList = {}

			local info = {}
			info.productID = heroId
			info.iconPath = GetEntityIconPath(hero)
			info.hero = hero
			info.product = ".Hero"

			table.insert(HoN_Store.heroDisplayAltList, info)
		end

		local info = {}
		info.productID = altId
		info.iconPath = interface:UICmd("GetHeroIcon2PathFromProduct('"..hero.."."..altCode.."')")
		info.hero = hero
		info.product = altCode

		-- insert at 2 so that we get a list with the default at 1 and then a list newest to oldest
		table.insert(HoN_Store.heroDisplayAltList, 2, info)

		HoN_Store:UpdateHeroAltListDisplay()
	end
end

function HoN_Store:DumpHeroAlts()
	HoN_Store.heroDisplayAltList = nil
	HoN_Store.heroAltListScroll = 0
	HoN_Store.heroAltListSelected = 1
end

function HoN_Store:UpdateHeroAltListDisplay()
	-- fill 6 slots without needing a scroller, if we need one then we only fill 5 to make room for arrows
	local slotsToFill = 6
	-- display the scrolling arrows
	if (#HoN_Store.heroDisplayAltList > 6) then
		GetWidget("hero_alt_list_left_scroller"):SetVisible(1)
		GetWidget("hero_alt_list_right_scroller"):SetVisible(1)
		slotsToFill = 5
		GetWidget("hero_list_alt_6"):SetVisible(0)

		-- enable/disable
		if (HoN_Store.heroAltListScroll == 0) then -- can't scroll left
			GetWidget("hero_alt_list_left_scroller_button"):SetEnabled(0)
			GetWidget("hero_alt_list_right_scroller_button"):SetEnabled(1)
		elseif (HoN_Store.heroAltListScroll == ((#HoN_Store.heroDisplayAltList) - 5)) then
			GetWidget("hero_alt_list_left_scroller_button"):SetEnabled(1)
			GetWidget("hero_alt_list_right_scroller_button"):SetEnabled(0)
		else
			GetWidget("hero_alt_list_left_scroller_button"):SetEnabled(1)
			GetWidget("hero_alt_list_right_scroller_button"):SetEnabled(1)
		end
	else
		GetWidget("hero_alt_list_left_scroller"):SetVisible(0)
		GetWidget("hero_alt_list_right_scroller"):SetVisible(0)
	end

	-- populate the base
	if (HoN_Store.heroDisplayAltList[1]) then
		GetWidget("hero_list_alt_1"):SetVisible(1)
		groupfcall("hero_list_alt_1_icon", function(_, w, _) w:SetTexture(HoN_Store.heroDisplayAltList[1].iconPath) end)
		groupfcall("hero_list_alt_1_selected", function(_, w, _) w:SetVisible(HoN_Store.heroAltListSelected == 1) end)
	else
		GetWidget("hero_list_alt_1"):SetVisible(0)
	end

	for i=2,slotsToFill do
		local offset = i+HoN_Store.heroAltListScroll

		if (HoN_Store.heroDisplayAltList[offset]) then
			GetWidget("hero_list_alt_"..i):SetVisible(1)
			groupfcall("hero_list_alt_"..i.."_icon", function(_, w, _) w:SetTexture(HoN_Store.heroDisplayAltList[offset].iconPath) end)
			groupfcall("hero_list_alt_"..i.."_selected", function(_, w, _) w:SetVisible(HoN_Store.heroAltListSelected == offset) end)
		else
			GetWidget("hero_list_alt_"..i):SetVisible(0)
		end
	end

end

function HoN_Store:SelectAltFromHeroList(id)
	id = tonumber(id)
	if (id ~= 1) then id = id + HoN_Store.heroAltListScroll end

	if (HoN_Store.heroDisplayAltList[id]) then
		HoN_Store.heroAltListSelected = id
		GetWidget("storeHeroListDisplayModel"):DoEventN(3, HoN_Store.heroDisplayAltList[id].hero, HoN_Store.heroDisplayAltList[id].product, 'z')
		HoN_Store:UpdateHeroAltListDisplay()
	end
end

function HoN_Store:ScrollHeroAltList(amount)
	amount = tonumber(amount)

	HoN_Store.heroAltListScroll = HoN_Store.heroAltListScroll + amount
	if (HoN_Store.heroAltListScroll < 0) then
		HoN_Store.heroAltListScroll = 0
	elseif (HoN_Store.heroAltListScroll > ((#HoN_Store.heroDisplayAltList) - 5)) then
		HoN_Store.heroAltListScroll = (#HoN_Store.heroDisplayAltList) - 5
	end

	HoN_Store:UpdateHeroAltListDisplay()
end

function OpenStoreCategory(categoryID, inVault)
	Set('_microStore_currentPage', 1)
	if GetCvarNumber('_microStore_Category') ~= categoryID then
		Set('_microStore_Category', categoryID)
	end
	if inVault then
		Set('_microStore_nextAction', 2)
	else
		Set('_microStore_nextAction', 1)
	end

	TestEcho('OpenStoreCategory categoryID: '..categoryID..' inVault: '..tostring(inVault))

	GetWidget('microStoreProcessResponse'):DoEventN(0)
	Trigger('MicroStoreSetProcTrig', 'MicroStoreVaultThrottle')
	Trigger('MicroStoreVaultThrottle', 1)
	Trigger('MicroStoreVaultThrottle', 0)
end

function OpenStoreToCategory(targetCategory, forceOpen)
	local store = GetCvarBool('cg_store2_') and 'store_container2' or 'store_container'
	if GetWidget(store):IsVisible() then
		OpenStoreCategory(tonumber(targetCategory))
	else
		Set("microStore_targetCategory", targetCategory)
		Set('store2_initCategory', targetCategory, 'int')
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', store, nil, forceOpen)	
	end
end

--- global function to open the store to an alt avatar or hero
function OpenStoreToProduct(targetCategory, targetProduct, forceOpen) -- 71 hero, 58 EAP, 2 alt

	local numTarget = tonumber(targetCategory)
	if (numTarget and (numTarget == 71 or numTarget == 58)) then
		if (numTarget == 71 and IsEarlyAccessHero(targetProduct)) then
			targetCategory = 58
		elseif (numTarget == 58 and not IsEarlyAccessHero(targetProduct)) then
			targetCategory = 71
		end

		Set("microStore_heroSelectTarget", targetProduct)
	else
		Set("microStore_avatarSelectTarget", targetProduct)
	end

	Set("microStore_targetCategory", targetCategory)

	local store = GetCvarBool('cg_store2_') and 'store_container2' or 'store_container'
	UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', store, nil, forceOpen)

	if GetCvarBool('cg_store2_') then
		local catId = targetCategory
		local productName = targetProduct
		if catId == 2 then
			local t = string.match(productName, '^(.+)%.Base$')
			if t then
				catId = 71
				productName = t
			end
		end
		HoN_Store.OpenStoreTargetPageInfo = {
			catId = catId, 
			productName = productName
		}
	end
end

function OpenStoreToBuyCoins(forceOpen) -- 71 hero, 58 EAP, 1 alt
	local store = GetCvarBool('cg_store2_') and 'store_container2' or 'store_container'
	if GetWidget(store):IsVisible() then
		Set('_microStore_RequestCode', 5)
		GetWidget('MicroStoreAction'):DoEventN(0)
	else
		Set('microStore_targetRequest', 5)
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', store, nil, forceOpen)
	end
end

function OpenStoreToSpecials()
	local store = GetCvarBool('cg_store2_') and 'store_container2' or 'store_container'
	if GetWidget(store):IsVisible() then
		Set('_microStore_RequestCode', 0)
		Set('_microStore_Category', 1)
		GetWidget('MicroStoreAction'):DoEventN(0)
	else
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', store, nil, true)
	end
end

-- global function to open the store with a search
function OpenStoreToAltAndSearch(searchTerm, forceOpen, visCheck)
	local store = GetCvarBool('cg_store2_') and 'store_container2' or 'store_container'
	if visCheck and GetWidget(store):IsVisible() then

		if GetWidget('storeViewAvatarsFull'):IsVisible() then
			GetWidget('store_heroavatar_search_button'):UICmd("GroupCall('store_heroavatar_sort_btns', 'SetVisible(0)');SetVisible(0);ShowWidget('store_heroavatar_search_bar');")
			GetWidget("store_heroavatar_textbox_input"):SetInputLine(searchTerm)
		else
			GetWidget('storeViewAvatarsFull'):SetCallback('onevent3', function(self)
				GetWidget('store_heroavatar_search_button'):UICmd("GroupCall('store_heroavatar_sort_btns', 'SetVisible(0)');SetVisible(0);ShowWidget('store_heroavatar_search_bar');")
				GetWidget("store_heroavatar_textbox_input"):SetInputLine(searchTerm)
				self:ClearCallback('onevent3')
				self:RefreshCallbacks()
			end)

			OpenStoreCategory(2)
		end
	else
		GetWidget('storeViewAvatarsFull'):SetCallback('onevent3', function(self)
			GetWidget('store_heroavatar_search_button'):UICmd("GroupCall('store_heroavatar_sort_btns', 'SetVisible(0)');SetVisible(0);ShowWidget('store_heroavatar_search_bar');")
			GetWidget("store_heroavatar_textbox_input"):SetInputLine(searchTerm)
			self:ClearCallback('onevent3')
			self:RefreshCallbacks()
		end)
		GetWidget('storeViewAvatarsFull'):RefreshCallbacks()

		Set("microStore_targetCategory", 1)
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', store, nil, forceOpen, nil)
	end
end

function OpenStore2ToAltAndSearch(searchTerm)
	Set('store2_initSearchTerm', searchTerm, 'string')
	OpenStoreToCategory(2, true)
end

function HoN_Store:PurchaseEAP()
	-- standard stuff
	-- product eliigibility
	local goldCost = tonumber(GetCvarInt("ui_eapSelectedBundleGold"))
	local silverCost = tonumber(GetCvarInt("ui_eapSelectedBundleSilver"))

	local eligIndex = tostring(GetCvarInt('ui_eapSelectedBundleID'))
	if (HoN_Store.productEligibility) and (HoN_Store.productEligibility[eligIndex]) then
		if (AtoB(HoN_Store.productEligibility[eligIndex].eligible)) then
			goldCost = HoN_Store.productEligibility[eligIndex].goldCost
			silverCost = HoN_Store.productEligibility[eligIndex].silverCost
		end
	end

	Set("_microStoreSelectedHeroItem", Store.eapEntityName1, "string")
	PlaySound("/shared/sounds/ui/ccpanel/button_click_02.wav")
	Set("_microStore_SelectedItem", 999, "int")
	Set("microStoreID999", GetCvarInt('ui_eapSelectedBundleID'), "int")
	Set("_microStore_SelectedID", GetCvarInt('microStoreID999'), "int")
	Set("microStoreLocalContent999", GetEntityIconPath(Store.eapEntityName1))
	Set("microStorePremium999", true, "bool")
	Set("microStorePrice999", tostring(goldCost), "int")
	Set("microStoreSilverPrice999", tostring(silverCost), "int")
	Trigger("MicroStoreUpdateSelection")
	GetWidget('store_popup_confirm_purchase_choose'):DoEventN(0)
end

----------- featured section stuff
function HoN_Store:ParseFeaturedSection(productNames, productIDs, productLocalPaths, productOwned, bundleNames, bundleCosts, bundlePaths, bundleProducts, bundlesOwned, bundleIDs)
	HoN_Store.featuredTable = {}

	local productCount = 0
	local bundleCount = 0

	-- explode out and fill out the products infos
	local pNames = explode("|", productNames)
	local pIds = explode("|", productIDs)
	local pPaths = explode("|", productLocalPaths)
	local pOwned = explode("|", productOwned)

	HoN_Store.featuredTable.products = {}

	for i,v in ipairs(pNames) do
		HoN_Store.featuredTable.products[i] = {}
		HoN_Store.featuredTable.products[i].name = v
		HoN_Store.featuredTable.products[i].id = tonumber(pIds[i])
		HoN_Store.featuredTable.products[i].path = pPaths[i]
		HoN_Store.featuredTable.products[i].owned = AtoB(pOwned[i])
		HoN_Store.featuredTable.products[i].isHero = string.find(v, ".eap", -4, true)
		productCount = productCount + 1
	end

	-- explode out and fill out the bundles infos
	local bNames = explode("|", bundleNames)
	local bCosts = explode("|", bundleCosts)
	local bPaths = explode("|", bundlePaths)
	local bIncludes = explode("|", bundleProducts)
	local bOwned = explode("|", bundlesOwned)
	local bIDs = explode("|", bundleIDs)
	HoN_Store.featuredTable.bundles = {}

	for i,v in ipairs(bNames) do
		HoN_Store.featuredTable.bundles[i] = {}
		HoN_Store.featuredTable.bundles[i].name = v
		HoN_Store.featuredTable.bundles[i].cost = tonumber(bCosts[i])
		HoN_Store.featuredTable.bundles[i].path = bPaths[i]
		HoN_Store.featuredTable.bundles[i].owned = AtoB(bOwned[i])
		HoN_Store.featuredTable.bundles[i].id = tonumber(bIDs[i])

		-- includes
		local includes = explode("~", bIncludes[i])
		HoN_Store.featuredTable.bundles[i].includes = includes

		-- override for owned, if they own all the products, we are going to mark it owned
		if (not AtoB(bOwned[i])) then
			local unOwnedProduct = false

			for j=1,productCount do
				if ((not AtoB(pOwned[j])) and includes and includes[j] and AtoB(includes[j])) then
					unOwnedProduct = true
					break
				end
			end

			if (not unOwnedProduct) then
				HoN_Store.featuredTable.bundles[i].owned = true
			end
		end

		bundleCount = bundleCount + 1
	end

	-- sort bundles by cost, or name if they are equal
	local sortFunc = function(a,b) if (a.cost == b.cost) then return string.lower(Translate(a.name)) < string.lower(Translate(b.name)) else return a.cost < b.cost end end
	table.sort(HoN_Store.featuredTable.bundles, sortFunc)

	-- fill out the counts
	HoN_Store.featuredTable.productCount = productCount
	HoN_Store.featuredTable.bundleCount = bundleCount

	-- pick the most expensive (complete) bundle to start
	HoN_Store:SelectFeaturedBundle(bundleCount)

	-- table built, populate stuff
	HoN_Store:PopulateFeaturedSection()
end

function HoN_Store:PopulateFeaturedSection()
	-- blech, not an acceptable count on things, quit out
	if ((not HoN_Store.featuredTable) or (not HoN_Store.featuredTable.productCount) or (HoN_Store.featuredTable.productCount < 2) or (HoN_Store.featuredTable.productCount > 5)) then
		-- hide everything but the error
		GetWidget("store_featured_footer_center"):SetVisible(0)
		for i=1,5 do GetWidget("featured_model_panel_"..i):SetVisible(0) end
		GetWidget("mstore_featured_error"):SetVisible(1)

		return
	else
		GetWidget("mstore_featured_error"):SetVisible(0)
		GetWidget("store_featured_footer_center"):SetVisible(1)
	end

	-- position tables, positions for the model panels and such given the number of products to display
	-- index into table is the number of panels, next index is that panel's offset (1-5)
	local positionTable = {
		[2] = { 		-- 2 products
			[1] = {		-- left side, 2 products
				['x'] 			= "10%",
				['y'] 			= "0",
				['width'] 		= "55%",
				['height'] 		= "100%",
				['align'] 		= "left",
				['valign'] 		= "top",
				['camAngles'] 	= "0 0 180",
				['modelPos']	= "0 -1250 30",
				['gearEffect'] 	= "/ui/fe2/store/eap/gears/gear_side.effect",
				['depthLevel']	= 1
			},
			[2] = {		-- right side, 2 products
				['x'] 			= "-10%",
				['y'] 			= "0",
				['width'] 		= "55%",
				['height'] 		= "100%",
				['align'] 		= "right",
				['valign'] 		= "top",
				['camAngles'] 	= "0 0 180",
				['modelPos']	= "0 -1250 30",
				['gearEffect'] 	= "/ui/fe2/store/eap/gears/gear_side.effect",
				['depthLevel']	= 1
			}
		},
		[3] = {			-- 3 products
			[1] = {		-- left side, 3 products
				['x'] 			= "1%",
				['y'] 			= "0",
				['width'] 		= "55%",
				['height'] 		= "100%",
				['align'] 		= "left",
				['valign'] 		= "top",
				['camAngles'] 	= "0 0 180",
				['modelPos']	= "0 -1500 20",
				['gearEffect'] 	= "/ui/fe2/store/eap/gears/gear_side.effect",
				['depthLevel']	= 1
			},
			[2] = {		-- center, 3 products
				['x'] 			= "0",
				['y'] 			= "0",
				['width'] 		= "55%",
				['height'] 		= "100%",
				['align'] 		= "center",
				['valign'] 		= "top",
				['camAngles'] 	= "0 0 180",
				['modelPos']	= "0 -1500 30",
				['gearEffect'] 	= "/ui/fe2/store/eap/gears/gear_side.effect",
				['depthLevel']	= 2
			},
			[3] = {		-- right side, 3 products
				['x'] 			= "-1%",
				['y'] 			= "0",
				['width'] 		= "55%",
				['height'] 		= "100%",
				['align'] 		= "right",
				['valign'] 		= "top",
				['camAngles'] 	= "0 0 180",
				['modelPos']	= "0 -1500 20",
				['gearEffect'] 	= "/ui/fe2/store/eap/gears/gear_side.effect",
				['depthLevel']	= 1
			}
		},
		[4] = {			-- 4 products
			[1] = {		-- left most, 4 products
				['x'] 			= "1%",
				['y'] 			= "0",
				['width'] 		= "45%",
				['height'] 		= "100%",
				['align'] 		= "left",
				['valign'] 		= "top",
				['camAngles'] 	= "0 0 180",
				['modelPos']	= "0 -1000 20",
				['gearEffect'] 	= "/ui/fe2/store/eap/gears/gear_side.effect",
				['depthLevel']	= 1
			},
			[2] = {		-- left center, 4 products
				['x'] 			= "18.5%",
				['y'] 			= "0",
				['width'] 		= "45%",
				['height'] 		= "100%",
				['align'] 		= "left",
				['valign'] 		= "top",
				['camAngles'] 	= "0 0 180",
				['modelPos']	= "0 -1000 35",
				['gearEffect'] 	= "/ui/fe2/store/eap/gears/gear_side.effect",
				['depthLevel']	= 2
			},
			[3] = {		-- right center, 4 products
				['x'] 			= "-18.5%",
				['y'] 			= "0",
				['width'] 		= "45%",
				['height'] 		= "100%",
				['align'] 		= "right",
				['valign'] 		= "top",
				['camAngles'] 	= "0 0 180",
				['modelPos']	= "0 -1000 35",
				['gearEffect'] 	= "/ui/fe2/store/eap/gears/gear_side.effect",
				['depthLevel']	= 2
			},
			[4] = {		-- right most, 4 products
				['x'] 			= "-1%",
				['y'] 			= "0",
				['width'] 		= "45%",
				['height'] 		= "100%",
				['align'] 		= "right",
				['valign'] 		= "top",
				['camAngles'] 	= "0 0 180",
				['modelPos']	= "0 -1000 20",
				['gearEffect'] 	= "/ui/fe2/store/eap/gears/gear_side.effect",
				['depthLevel']	= 1
			}
		},
		[5] = {			-- 5 products
			[1] = {		-- left most, 5 products
				['x'] 			= "-1%",
				['y'] 			= "0",
				['width'] 		= "45%",
				['height'] 		= "100%",
				['align'] 		= "left",
				['valign'] 		= "top",
				['camAngles'] 	= "0 0 180",
				['modelPos']	= "0 -1250 5",
				['gearEffect'] 	= "/ui/fe2/store/eap/gears/gear_side.effect",
				['depthLevel']	= 1
			},
			[2] = {		-- left center, 5 products
				['x'] 			= "13%",
				['y'] 			= "0",
				['width'] 		= "45%",
				['height'] 		= "100%",
				['align'] 		= "left",
				['valign'] 		= "top",
				['camAngles'] 	= "0 0 180",
				['modelPos']	= "0 -1250 15",
				['gearEffect'] 	= "/ui/fe2/store/eap/gears/gear_side.effect",
				['depthLevel']	= 2
			},
			[3] = {		-- center, 5 products
				['x'] 			= "0",
				['y'] 			= "0",
				['width'] 		= "45%",
				['height'] 		= "100%",
				['align'] 		= "center",
				['valign'] 		= "top",
				['camAngles'] 	= "0 0 180",
				['modelPos']	= "0 -1250 25",
				['gearEffect'] 	= "/ui/fe2/store/eap/gears/gear_side.effect",
				['depthLevel']	= 3
			},
			[4] = {		-- right center, 5 products
				['x'] 			= "-13%",
				['y'] 			= "0",
				['width'] 		= "45%",
				['height'] 		= "100%",
				['align'] 		= "right",
				['valign'] 		= "top",
				['camAngles'] 	= "0 0 180",
				['modelPos']	= "0 -1250 15",
				['gearEffect'] 	= "/ui/fe2/store/eap/gears/gear_side.effect",
				['depthLevel']	= 2
			},
			[5] = {		-- right most, 5 products
				['x'] 			= "1%",
				['y'] 			= "0",
				['width'] 		= "45%",
				['height'] 		= "100%",
				['align'] 		= "right",
				['valign'] 		= "top",
				['camAngles'] 	= "0 0 180",
				['modelPos']	= "0 -1250 5",
				['gearEffect'] 	= "/ui/fe2/store/eap/gears/gear_side.effect",
				['depthLevel']	= 1
			}
		}
	}

	-- populate and position products
	local defsLoaded = {} -- prevent double/triple/etc loading of the same entity defs

	for i=1, 5 do
		if (i > HoN_Store.featuredTable.productCount) then
			-- set invisible
			GetWidget("featured_model_panel_"..i):SetVisible(0)
		else
			-- infos
			local positionStuffs = positionTable[HoN_Store.featuredTable.productCount][i]
			local avatarInfo = HoN_Store.featuredTable.products[i]
			local modelPanel = GetWidget("featured_model_panel_"..i.."_model")
			local masterPanel = GetWidget("featured_model_panel_"..i)

			-- load the def if need be
			if (not GetCvar('_entityDefinitionsLoaded')) then
				-- come up with the entity folder path form the given path\
				local def = string.sub(avatarInfo.path, 1, string.find(avatarInfo.path, "/", 9))
				if (not defsLoaded[def]) then
					interface:UICmd("LoadEntityDefinitionsFromFolder('"..def.."')")
					defsLoaded[def] = true
				end
			end

			-- set the model, angles, etc
			local heroName = explode(".", avatarInfo.name)
			--modelPanel:UICmd("SetModelPos(GetHeroPreviewPosFromProduct('"..avatarInfo.name.."'));")
			modelPanel:UICmd("SetModelPos('"..positionStuffs.modelPos.."');")
			--modelPanel:UICmd("SetModelAngles(GetHeroPreviewAnglesFromProduct('"..avatarInfo.name.."'));")
			--modelPanel:UICmd("SetModelScale(GetHeroPreviewScaleFromProduct('"..avatarInfo.name.."'));")
			--modelPanel:UICmd("SetModel(GetHeroPreviewModelPathFromProduct('"..avatarInfo.name.."'));")
			modelPanel:UICmd("SetModelScale(GetHeroStoreScaleFromProduct('"..avatarInfo.name.."'));")
			modelPanel:UICmd("SetModel(GetHeroStoreModelPathFromProduct('"..avatarInfo.name.."'));")
			modelPanel:UICmd("SetEffect('"..positionStuffs.gearEffect.."');")
			modelPanel:UICmd("SetEffectIndexed('/shared/effects/glow.effect', 0);")
			if (heroName[1] ~= "Hero_Empath" and heroName[1] ~= "Hero_Gemini" and heroName[1] ~= "Hero_ShadowBlade" and heroName[1] ~= "Hero_Dampeer") then
				modelPanel:UICmd("SetEffectIndexed(GetHeroStorePassiveEffectPathFromProduct('"..avatarInfo.name.."'), 1);")
			else
				modelPanel:UICmd("SetEffectIndexed(GetHeroPassiveEffectPathFromProduct('"..avatarInfo.name.."'), 1);")
			end
			modelPanel:UICmd("SetTeamColor('1 0 0');")

			-- set the positions, etc
			masterPanel:SetX(positionStuffs.x)
			masterPanel:SetY(positionStuffs.y)
			masterPanel:SetWidth(positionStuffs.width)
			masterPanel:SetHeight(positionStuffs.height)
			masterPanel:SetAlign(positionStuffs.align)
			masterPanel:SetVAlign(positionStuffs.valign)
			modelPanel:UICmd("SetCameraAngles('"..positionStuffs.camAngles.."');")
			--modelPanel:UICmd("SetCameraPos('"..positionStuffs.camPos.."');")

			-- setup the top section
			if (not avatarInfo.isHero) then -- avatar, just icon and name
				GetWidget("featured_model_panel_"..i.."_alt_section"):SetVisible(1)
				GetWidget("featured_model_panel_"..i.."_owned_section"):SetY("5.0h")
				GetWidget("featured_model_panel_"..i.."_hero_section"):SetVisible(0)
				GetWidget("featured_model_panel_"..i.."_icon"):SetTexture(interface:UICmd("GetHeroIcon2PathFromProduct('"..avatarInfo.name.."')"))
				GetWidget("featured_model_panel_"..i.."_name"):SetText(Translate('mstore_product' .. avatarInfo.id .. '_name'))
			else
				GetWidget("featured_model_panel_"..i.."_alt_section"):SetVisible(0)
				GetWidget("featured_model_panel_"..i.."_owned_section"):SetY("6.25h")
				GetWidget("featured_model_panel_"..i.."_hero_section"):SetVisible(1)
				GetWidget("featured_model_panel_"..i.."_hero_icon"):SetTexture(interface:UICmd("GetHeroIconPathFromProduct('"..avatarInfo.name.."')"))

				local nameTable = explode(".", avatarInfo.name)
				-- font size and text for hero name
				GetWidget("featured_model_panel_"..i.."_hero_name"):SetFont(GetFontSizeForWidth(Translate('mstore_' .. nameTable[1] .. '_name'), GetWidget("featured_model_panel_"..i.."_hero_name"):GetWidth(), 16))
				GetWidget("featured_model_panel_"..i.."_hero_name"):SetText(Translate('mstore_' .. nameTable[1] .. '_name'))

				-- font size and text for category
				local categoryText = interface:UICmd("GetHeroCategories('"..nameTable[1].."', 1)")
				GetWidget("featured_model_panel_"..i.."_hero_categories"):SetFont(GetFontSizeForWidth(categoryText, GetWidget("featured_model_panel_"..i.."_hero_categories"):GetWidth(), 8))
				GetWidget("featured_model_panel_"..i.."_hero_categories"):SetText(categoryText)

				-- difficulty
				local difficulty = tonumber(interface:UICmd("GetHeroDifficulty('"..nameTable[1].."')"))
				for j=1, 5 do
					if (j <= difficulty) then
						GetWidget("featured_model_panel_"..i.."_hero_difficulty_"..j):SetWidth("100%")
					elseif ((difficulty - j) == -0.5) then
						GetWidget("featured_model_panel_"..i.."_hero_difficulty_"..j):SetWidth("50%")
					else
						GetWidget("featured_model_panel_"..i.."_hero_difficulty_"..j):SetWidth("0")
					end
				end
			end

			-- set the owned stuff
			if (avatarInfo.owned) then
				GetWidget("featured_model_panel_"..i.."_owned_label"):SetColor("#feaa00")
			else
				GetWidget("featured_model_panel_"..i.."_owned_label"):SetColor(".4 .4 .4 1")
			end
			GetWidget("featured_model_panel_"..i.."_owned_checkbox"):SetVisible(avatarInfo.owned)
			GetWidget("featured_model_panel_"..i.."_owned_highlight"):SetVisible(avatarInfo.owned)

			-- finally, show it
			masterPanel:SetVisible(1)
		end
	end

	-- set the depths on the model panels
	-- the lower the number for depth level, the lower it is
	local depthSort = 1
	while (depthSort > 0) do
		local depthFound = false
		for i=1,HoN_Store.featuredTable.productCount do
			if (positionTable[HoN_Store.featuredTable.productCount][i].depthLevel == depthSort) then
				GetWidget("featured_model_panel_"..i):BringToFront()
				depthFound = true
			end
		end

		if (not depthFound) then
			depthSort = 0
		else
			depthSort = depthSort + 1
		end
	end

	HoN_Store:SetFeaturedHightlights(HoN_Store.selectedFeaturedBundle)

	-- populate bundles
	for i=1,10 do
		if (i > HoN_Store.featuredTable.bundleCount) then
			GetWidget("mstore_featured_bundle"..i):SetVisible(0)
		else
			GetWidget("mstore_featured_bundle"..i.."_name"):SetText(Translate(HoN_Store.featuredTable.bundles[i].name))
			GetWidget("mstore_featured_bundle"..i):SetVisible(1)
		end
	end
end

function HoN_Store:SetFeaturedHightlights(id)
	id = tonumber(id)

	for i=1,HoN_Store.featuredTable.productCount do
		if (HoN_Store.featuredTable.bundles[id].includes and AtoB(HoN_Store.featuredTable.bundles[id].includes[i])) then
			-- name plate
			GetWidget("featured_model_panel_"..i.."_highlight"):SetVisible(1)
			GetWidget("featured_model_panel_"..i.."_hero_highlight"):SetVisible(1)
			-- model
			GetWidget("featured_model_panel_"..i.."_model"):DoEventN(1)
		else
			-- name plate
			GetWidget("featured_model_panel_"..i.."_highlight"):SetVisible(0)
			GetWidget("featured_model_panel_"..i.."_hero_highlight"):SetVisible(0)
			-- model
			GetWidget("featured_model_panel_"..i.."_model"):DoEventN(0)
		end
	end
end

function HoN_Store:SelectFeaturedBundle(id)
	id = tonumber(id)

	if ((id <= 0) or (id > HoN_Store.featuredTable.bundleCount)) then
		return
	end

	-- selected text
	GetWidget("mstore_featured_selectedBundle"):SetText(Translate(HoN_Store.featuredTable.bundles[id].name))
	HoN_Store.selectedFeaturedBundle = id

	-- product eliigibility
	local goldCost = tonumber(HoN_Store.featuredTable.bundles[id].cost)

	local eligIndex = tostring(HoN_Store.featuredTable.bundles[id].id)
	if (HoN_Store.productEligibility) and (HoN_Store.productEligibility[eligIndex]) then
		if (AtoB(HoN_Store.productEligibility[eligIndex].eligible)) then
			goldCost = HoN_Store.productEligibility[eligIndex].goldCost
		end
	end

	-- gold cost
	GetWidget("mstore_featured_purchase_cost_label"):SetText(goldCost)

	-- purchased/purchase
	GetWidget("mstore_featured_purchase_owned"):SetVisible(HoN_Store.featuredTable.bundles[id].owned)
	GetWidget("mstore_featured_purchase_purchase"):SetVisible(not HoN_Store.featuredTable.bundles[id].owned)

	GetWidget("mstore_featured_bundleselect_dropup"):SetVisible(0)

	HoN_Store:SetFeaturedHightlights(id)
end

function HoN_Store:HoverFeaturedBundle(id)
	id = tonumber(id)

	if ((id <= 0) or (id > HoN_Store.featuredTable.bundleCount)) then
		return
	end

	-- product eliigibility
	local goldCost = tonumber(HoN_Store.featuredTable.bundles[id].cost)

	local eligIndex = tostring(HoN_Store.featuredTable.bundles[id].id)
	if (HoN_Store.productEligibility) and (HoN_Store.productEligibility[eligIndex]) then
		if (AtoB(HoN_Store.productEligibility[eligIndex].eligible)) then
			goldCost = HoN_Store.productEligibility[eligIndex].goldCost
		end
	end

	GetWidget("mstore_featured_purchase_cost_label"):SetText(goldCost)
	GetWidget("mstore_featured_selectedBundle"):SetText(Translate(HoN_Store.featuredTable.bundles[id].name))

	-- purchased/purchase
	GetWidget("mstore_featured_purchase_owned"):SetVisible(HoN_Store.featuredTable.bundles[id].owned)
	GetWidget("mstore_featured_purchase_purchase"):SetVisible(not HoN_Store.featuredTable.bundles[id].owned)

	HoN_Store:SetFeaturedHightlights(id)
end

function HoN_Store:HoverFeatured(id)
	id = tonumber(id)

	if ((id <= 0) or (id > HoN_Store.featuredTable.productCount)) then
		return
	end

	-- trigger hero info to show the hero the avatar belongs to over chat on hover
	local nameTable = explode(".", HoN_Store.featuredTable.products[id].name)
	interface:UICmd("GetHeroInfo('"..nameTable[1].."');")
	GetWidget("hero_info"):SetVisible(1)
end

function HoN_Store:FeaturedVoicePreview(id)
	id = tonumber(id)

	if ((id <= 0) or (id > HoN_Store.featuredTable.productCount) or (not HoN_Store.featuredTable.products[id].isHero)) then
		return
	end

	-- trigger hero info to show the hero the avatar belongs to over chat on hover
	local nameTable = explode(".", HoN_Store.featuredTable.products[id].name)
	interface:UICmd("PlayHeroPreviewSoundFromProduct('"..nameTable[1].."');")
end

function HoN_Store:RestoreBundle()
	GetWidget("mstore_featured_purchase_cost_label"):SetText(HoN_Store.featuredTable.bundles[HoN_Store.selectedFeaturedBundle].cost)
	GetWidget("mstore_featured_selectedBundle"):SetText(Translate(HoN_Store.featuredTable.bundles[HoN_Store.selectedFeaturedBundle].name))

	-- purchased/purchase
	GetWidget("mstore_featured_purchase_owned"):SetVisible(HoN_Store.featuredTable.bundles[HoN_Store.selectedFeaturedBundle].owned)
	GetWidget("mstore_featured_purchase_purchase"):SetVisible(not HoN_Store.featuredTable.bundles[HoN_Store.selectedFeaturedBundle].owned)

	HoN_Store:SetFeaturedHightlights(HoN_Store.selectedFeaturedBundle)
end

function HoN_Store:CheckPurchaseFeatured()
	-- check if they own some of the products in the bundle, prompt them if they do
	local ownedProducts = false
	for i=1,HoN_Store.featuredTable.productCount do
		if (AtoB(HoN_Store.featuredTable.bundles[HoN_Store.selectedFeaturedBundle].includes[i]) and HoN_Store.featuredTable.products[i].owned) then
			ownedProducts = true
			break
		end
	end

	-- continue? dialog
	if (ownedProducts) then
		Trigger("TriggerDialogBox",
			"mstore_featured_owned_header",
			"general_continue",
			"general_cancel",
			"CallEvent('mstore_featured_purchase_purchase');",
			"",
			"mstore_featured_owned_header",
			Translate("mstore_featured_owned_body", "cost", HoN_Store.featuredTable.bundles[HoN_Store.selectedFeaturedBundle].cost)
		)
	else
		HoN_Store:PurchaseFeatured()
	end
end

function HoN_Store:PurchaseFeatured()
	-- override for dialog, easiest way to do this :\
	-- product eliigibility
	local goldCost = tonumber(HoN_Store.featuredTable.bundles[HoN_Store.selectedFeaturedBundle].cost)

	local eligIndex = HoN_Store.featuredTable.bundles[HoN_Store.selectedFeaturedBundle].id
	if (HoN_Store.productEligibility) and (HoN_Store.productEligibility[eligIndex]) then
		if (AtoB(HoN_Store.productEligibility[eligIndex].eligible)) then
			goldCost = HoN_Store.productEligibility[eligIndex].goldCost
		end
	end

	Set('_microStore_overrideConfirmName', true, 'bool')
	Set("_microStoreSelectedHeroItem", Translate(HoN_Store.featuredTable.bundles[HoN_Store.selectedFeaturedBundle].name), "string")
	PlaySound("/shared/sounds/ui/ccpanel/button_click_02.wav")
	Set("_microStore_SelectedItem", 999, "int")
	Set("microStoreID999", HoN_Store.featuredTable.bundles[HoN_Store.selectedFeaturedBundle].id, "int")
	Set("_microStore_SelectedID", GetCvarInt('microStoreID999'), "int")
	Set("microStoreLocalContent999", HoN_Store.featuredTable.bundles[HoN_Store.selectedFeaturedBundle].path, "string")
	Set("microStorePremium999", true, "bool")
	Set("microStorePrice999", goldCost, "int")
	Set("microStoreSilverPrice999", 0, "int")
	Trigger("MicroStoreUpdateSelection")
	Set('_microStore_overrideConfirmName', false, 'bool')

	GetWidget('store_popup_confirm_purchase_choose'):DoEventN(0)
end

function HoN_Store:SetupFeaturedSection()
	-- stuff for the background
	local widget = GetWidget("storeFeatured")
	local angleXCurrPercent, angleYCurrPercent = 0, 0

	local backgroundFrameFunction = function()
		local angleXPercent = (math.max(widget:GetX(), math.min((widget:GetX() + widget:GetWidth()), Input.GetCursorPosX())) - widget:GetX()) / widget:GetWidth()
		local angleYPercent = (math.max(widget:GetY(), math.min((widget:GetY() + widget:GetHeight()), Input.GetCursorPosY())) - widget:GetY()) / widget:GetHeight()

		angleXCurrPercent = angleXCurrPercent + ((angleXPercent - angleXCurrPercent) * (GetFrameTime() / 500))
		angleYCurrPercent = angleYCurrPercent + ((angleYPercent - angleYCurrPercent) * (GetFrameTime() / 500))

		local targXAngle = ((angleXCurrPercent * -16.0) + 8.0)
		local targYAngle = ((angleYCurrPercent * -6.0) + 8.0)

		GetWidget("featured_model_center_background"):SetModelAngles(targYAngle, 0, targXAngle)
	end

	--widget:RegisterWatch("HostTime", frameFunction)
	widget:SetCallback("onframe", backgroundFrameFunction)
	widget:RefreshCallbacks()

	-- stuff for model rotation
	groupfcall('featured_model_panels', function(_, w, _) w:UICmd('SetAnimSpeed(0);') end)
	GetWidget("featured_model_center_background"):UICmd('SetAnimSpeed(0);')

	widget = GetWidget("storeFeaturedRotation")
	local rotationPos, gearPos = 0, 0
	HoN_Store.featuredModelSpinSpeed = 0.1
	HoN_Store.featuredRotationDir = 1

	local modelFrameFunction = function()
		rotationPos = (rotationPos + (HoN_Store.featuredRotationDir * (GetFrameTime() * HoN_Store.featuredModelSpinSpeed / 2000)))
		gearPos = (gearPos + (HoN_Store.featuredRotationDir * (GetFrameTime() * HoN_Store.featuredModelSpinSpeed / 4000)))

		if (rotationPos > 1) then
			rotationPos = rotationPos - 1
		elseif (rotationPos < 0) then
			rotationPos = rotationPos + 1
		end

		if (gearPos > 1) then
			gearPos = gearPos - 1
		elseif (gearPos < 0) then
			gearPos = gearPos + 1
		end

		groupfcall("featured_model_panels", function(_, w, _) w:SetModelAngles(0, 0, (rotationPos * -360)) end)
		GetWidget("featured_model_center_background"):UICmd("SetAnimOffsetTime("..gearPos..");")
	end

	--widget:RegisterWatch("HostTime", frameFunction)
	widget:SetCallback("onframe", modelFrameFunction)
	widget:RefreshCallbacks()
end

function HoN_Store:CloseFeaturedSection()
	-- background
	--GetWidget("storeFeatured"):UnregisterWatch("HostTime")
	GetWidget("storeFeatured"):ClearCallback("onframe")
	GetWidget("storeFeatured"):RefreshCallbacks()

	-- models
	HoN_Store.featuredModelSpinSpeed = 0.1
	--GetWidget("storeFeaturedRotation"):UnregisterWatch("HostTime")
	GetWidget("storeFeaturedRotation"):ClearCallback("onframe")
	GetWidget("storeFeaturedRotation"):RefreshCallbacks()
end

function HoN_Store:FeaturedLeftRotation()
	HoN_Store.featuredRotationDir = -1
	HoN_Store.featuredModelSpinSpeed = 1.0
end

function HoN_Store:FeaturedRightRotation()
	HoN_Store.featuredRotationDir = 1
	HoN_Store.featuredModelSpinSpeed = 1.0
end

function HoN_Store:FeaturedNormalRotation()
	HoN_Store.featuredModelSpinSpeed = 0.1
end
----------- end of featured section stuff

-- Coin package stuff
local function roundToFive(num)
	local singles = num % 10
	local tens = round((math.floor(num / 10) * 10))

	if (singles < 2.5) then
		singles = 0
	elseif (singles < 7.5) then
		singles = 5
	else
		singles = 0
		tens = tens + 10
	end

	return tens + singles
end

function HoN_Store:UpdateCoinPackageLabels()
	-- automating the bonus %, includes X free stuff so it's always right
	local coins = {GetCvarInt('microStorePackageCoins1'), GetCvarInt('microStorePackageCoins2'), GetCvarInt('microStorePackageCoins3'), GetCvarInt('microStorePackageCoins4'), GetCvarInt('microStorePackageCoins5')}
	local costs = {GetCvarInt('microStorePackagePrice1'), GetCvarInt('microStorePackagePrice2'), GetCvarInt('microStorePackagePrice3'), GetCvarInt('microStorePackagePrice4'), GetCvarInt('microStorePackagePrice5')}

	local baseGoldPerCost = GetCvarNumber('microStoreCurrencyToCoin')
	-- this will be -1 if it wasn't sent, or 0 if done wrong, in that case just hide things
	if (baseGoldPerCost > 0) then
		local bonuses = {coins[1] - (baseGoldPerCost * costs[1]), coins[2] - (baseGoldPerCost * costs[2]), coins[3] - (baseGoldPerCost * costs[3]), coins[4] - (baseGoldPerCost * costs[4]), coins[5] - (baseGoldPerCost * costs[5])}
		local bonusPercents = {roundToFive((bonuses[1] / (coins[1] - bonuses[1])) * 100), roundToFive((bonuses[2] / (coins[2] - bonuses[2])) * 100), roundToFive((bonuses[3] / (coins[3] - bonuses[3])) * 100), roundToFive((bonuses[4] / (coins[4] - bonuses[4])) * 100), roundToFive((bonuses[5] / (coins[5] - bonuses[5])) * 100)}

		for i=1,5 do
			groupfcall("coinBundleBonusCoinsVisible"..i, function(_, w, _) w:SetVisible(math.ceil(bonuses[i]) > 0) end)
			groupfcall("coinBundleBonusCoins"..i, function(_, w, _) if (math.ceil(bonuses[i]) > 0) then w:SetText(math.ceil(bonuses[i])) else w:SetText("") end end)
			groupfcall("coinBundleBonusPercent"..i, function(_, w, _) if (math.ceil(bonusPercents[i]) > 0) then w:SetText(Translate("mstore_bonus_val", "amount", math.ceil(bonusPercents[i]))) else w:SetText("") end end)
			groupfcall("coinBundleBonusString"..i, function(_, w, _) if (math.ceil(bonuses[i]) > 0) then w:SetText("^w(^*"..(math.ceil(baseGoldPerCost * costs[i])).." ^w+^g "..math.ceil(bonuses[i]).."^w)") else w:SetText("") end end)
		end
	else
		for i=1,5 do
			groupfcall("coinBundleBonusCoinsVisible"..i, function(_, w, _) w:SetVisible(false) end)
			groupfcall("coinBundleBonusPercent"..i, function(_, w, _) w:SetText("") end)
		end
	end
end
-- end of coin package stuff

-- background themeing stuff --
function HoN_Store:SetStoreBackground()
	local themeTable = {
		['default'] = {
			background = "/ui/fe2/store/background.tga",
		},
		['christmas'] = {
			startDate = {['month'] = 12, ['day'] = 13},
			endDate = {['month'] = 1, ['day'] = 15},
			background = "/ui/fe2/plinko/christmas/background.tga",
		},
		-- ['paragon1'] = {
			-- startDate = {['month'] = 1, ['day'] = 15},
			-- endDate = {['month'] = 4, ['day'] = 9},
			-- background = "/ui/fe2/plinko/paragon/background.tga",
		-- },
		['water_festival'] = {
			startDate = {['month'] = 4, ['day'] = 9},
			endDate = {['month'] = 4, ['day'] = 24},
			background = "/ui/fe2/plinko/water_festival/background.tga",
		},
		['spring'] = {
			startDate = {['month'] = 4, ['day'] = 24},
			endDate = {['month'] = 5, ['day'] = 5},
			background = "/ui/fe2/plinko/spring/background.tga",
		},
		-- ['paragon2'] = {
			-- startDate = {['month'] = 7, ['day'] = 28},
			-- endDate = {['month'] = 12, ['day'] = 15},
			-- background = "/ui/fe2/plinko/paragon/background.tga",
		-- },
		['halloween'] = {
			startDate = {['month'] = 10, ['day'] = 20},
			endDate = {['month'] = 11, ['day'] = 5},
			background = "/ui/fe2/plinko/holoween/background.tga",
		},
	}

	local backgroundWidget = GetWidget("store_background_image")

	local overrideTheme = GetCvarString('ui_overrideTheme', true)
	if (overrideTheme and themeTable[overrideTheme]) then
		backgroundWidget:SetTexture(themeTable[overrideTheme].background)
		return
	end

	for k,v in pairs(themeTable) do
		if (v.endDate and v.startDate) then
			if (IsCurrentlyWithinNumericDateRange(v.startDate, v.endDate)) then
				backgroundWidget:SetTexture(v.background)
				return
			end
		end
	end

	backgroundWidget:SetTexture(themeTable['default'].background)
	return
end
-------------------------------

-- Shelved item eligiblity --
function HoN_Store:SetupShelvedPrice(shelveId, productId, goldPrice, silverPrice)
	--TODO:for store2?
	local wdg = GetCvarBool('cg_store2_') and nil or GetWidget("storeViewShelfBtnIneligible"..shelveId)
	if wdg ~= nil then
		if (HoN_Store.productEligibility[productId]) then
			-- set the prices here reguardless of things, since we have to show a cost, we will show what it will be
			-- when you are eligibile. No reason to show the ineligibile one.
			goldPrice = HoN_Store.productEligibility[productId].goldCost
			silverPrice = HoN_Store.productEligibility[productId].silverCost

			wdg:SetVisible(not AtoB(HoN_Store.productEligibility[productId].eligible))
		else
			wdg:SetVisible(false)
		end
	end

	-- override the price so prompts work
	Set('microStorePrice'..shelveId, goldPrice)
	Set('microStoreSilverPrice'..shelveId, silverPrice)
	-- set the temps used to populate the shelve
	Set('microStoreTempCDGold', goldPrice)
	Set('microStoreTempCDSilver', silverPrice)
end

-- Book view item eligibility
function HoN_Store:SetupBookPrice(shelveId, productId, goldPrice, silverPrice, widgetPrefix)
	if widgetPrefix and type(widgetPrefix) == 'string' and string.len(widgetPrefix) > 0 then
		if (HoN_Store.productEligibility[productId]) then
			-- set the prices here reguardless of things, since we have to show a cost, we will show what it will be
			-- when you are eligibile. No reason to show the ineligibile one.
			goldPrice = HoN_Store.productEligibility[productId].goldCost
			silverPrice = HoN_Store.productEligibility[productId].silverCost

			if (AtoB(HoN_Store.productEligibility[productId].eligible)) then
				GetWidget(widgetPrefix.."BtnIneligible"..shelveId):SetVisible(0)
				GetWidget(widgetPrefix.."PurchaseGlow"..shelveId):SetVisible(1)
				GetWidget(widgetPrefix.."BtnBuy"..shelveId.."EligContainer"):SetVisible(1)
			else
				GetWidget(widgetPrefix.."BtnIneligible"..shelveId):SetVisible(1)
				GetWidget(widgetPrefix.."PurchaseGlow"..shelveId):SetVisible(0)
				GetWidget(widgetPrefix.."BtnBuy"..shelveId.."EligContainer"):SetVisible(0)
			end

		else
			GetWidget(widgetPrefix.."BtnIneligible"..shelveId):SetVisible(0)
			GetWidget(widgetPrefix.."PurchaseGlow"..shelveId):SetVisible(0)
			GetWidget(widgetPrefix.."BtnBuy"..shelveId.."EligContainer"):SetVisible(1)
		end
	end

	-- override the price so prompts work
	Set('microStorePrice'..shelveId, goldPrice)
	Set('microStoreSilverPrice'..shelveId, silverPrice)
	-- set the temps used to populate the shelve
	Set('microStoreTempCDGold', goldPrice)
	Set('microStoreTempCDSilver', silverPrice)
end

function HoN_Store:HoverShelvedInelidgible(productId)
	title = "mstore_elig"
	body1 = "mstore_elig_tip"
	body3 = ""

	if (HoN_Store.productEligibility[productId].eligible) then
		if (HoN_Store.productEligibility[productId].eligbleID) then
			local requireTable = nil

			if (HoN_Store.productEligibility[productId].requiredProducts) then
				requireTable = explode(';', HoN_Store.productEligibility[productId].requiredProducts)
			else
				requireTable = explode('|', Translate('mstore_elig_requirements_' .. HoN_Store.productEligibility[productId].productID))
			end

			if (requireTable) then
				body3 = Translate("mstore_elig_required")
				for index, requiredProductID in pairs(requireTable) do
					local translateReqProductID = Translate('mstore_product'..requiredProductID..'_name')
					if (translateReqProductID ~= 'mstore_product'..requiredProductID..'_name') then
						body3 = body3 .. '\n' .. translateReqProductID
					end
				end
			end
		end
	end

	body2 = Translate('mstore_elig_package_'..HoN_Store.productEligibility[productId].productID)
	if (body2 == ('mstore_elig_package_'..HoN_Store.productEligibility[productId].productID)) then
		body2 = ""
	end

	if (title and body1) then
		Trigger('genericMainFloatingTip', 'true', '23h', '', title, body1, body2, body3, '', '')
	end
end

function HoN_Store:AvatarEntryOnMouseOver(id)
	HoN_Store:HoverAltSlot(id)
	PlaySound('/shared/sounds/ui/ccpanel/button_over_01.wav')
	GetWidget('hero_info', 'main'):SetVisible(true)

	id = tonumber(id) + HoN_Store.altAvatarScroll
	local avatar = HoN_Store.allAvatarsInfo[HoN_Store.altListData[id]]
	local trialInfo = GetTrialInfo(HoN_Store.altWebInfo[avatar.productID].heroName, HoN_Store.altWebInfo[avatar.productID].altCode)
	if (NotEmpty(trialInfo)) then
		Trigger('trial_store_info', trialInfo)
		GetWidget('store_trial_tip'):SetVisible(true)
	end
end


function HoN_Store:AddCardsListItems(product_id)

	Set('microStoreDiscountInfo', '')

	HoN_Store.GCardsTable = {}
	HoN_Store.CardLastSelect = -1

	local listControl = GetWidget('CardListControl_store')
	local listbox =	GetWidget('easy_CardListbox_store')

	local cardsTable = nil

	if (HoN_Store.heroTable[product_id]) then
		local heroName = HoN_Store.heroTable[product_id].hero
		cardsTable = GetCardsInfo(heroName, '')
	else
		if (HoN_Store.altWebInfo[product_id]) then
			local heroName = HoN_Store.altWebInfo[product_id].heroName
			local avatarName = HoN_Store.altWebInfo[product_id].altCode
			cardsTable = GetCardsInfo(heroName, avatarName)
		end
	end

	local cardlistPanel = GetWidget('CardListPanel_store')

	if (cardsTable == nil or #cardsTable <= 0) then
		cardlistPanel:SetVisible(false)
		return
	end

	for index, cardTable in pairs(cardsTable) do
		HoN_Store.GCardsTable[index - 1] = cardTable

		local cardName = nil
		if (cardTable.Coupon_id == "ext") then
			cardName = Translate('mstore_trial_name')
		else
			cardName = Translate('mstore_product'..tostring(cardTable.Product_id)..'_name')
		end

		listbox:AddTemplateListItem('Card_ListItem', '',
									 'name', cardName,
									 'discount', tostring(cardTable.Discount).."% off",
									 'textcolor', '#925a22',
									 'framecolor0', '#683f18',
									 'framecolor1', '#b9986c',
									 'framecolor2', '#e4d8bf')
	end

	local label = GetWidget('easy_CardDesc_store')
	label:SetText(Translate('mstore_nocards_desc'))
	label:SetColor("red")
	cardlistPanel:SetVisible(true)
end

function HoN_Store:OnCardListSelect(index)
	local label = GetWidget('easy_CardDesc_store')

	if (index ~= HoN_Store.CardLastSelect) then
		HoN_Store.CardLastSelect = index

		if (HoN_Store.GCardsTable and HoN_Store.GCardsTable[index]) then
			local descStr = Translate("mstore_cards_desc").." "..HoN_Store.GCardsTable[index].EndTime

			label:SetVisible(true)
			label:SetText(descStr)
			label:SetColor("#4f2e0b")

			Set('microStoreDiscountInfo', HoN_Store.GCardsTable[index].Coupon_id)

			local goldPrice = GetCvarInt('microStoreCDPriceGold', true)
			local silverPrice = GetCvarInt('microStoreCDPriceSilver', true)
			local goldOwn = GetCvarInt('_microStore_TotalCoins', true)
			local silverOwn = GetCvarInt('_microStore_TotalSilverCoins', true)
			goldPrice = math.floor(goldPrice * (100 - HoN_Store.GCardsTable[index].Discount) * 0.01)
			silverPrice = math.floor(silverPrice * (100 - HoN_Store.GCardsTable[index].Discount) * 0.01)
			GetWidget('storeConfirmPurchaseGoldCost'):SetText(FtoA(goldPrice, 0, 0 ,","))
			GetWidget('storeConfirmPurchaseSilverCost'):SetText(FtoA(silverPrice, 0, 0 ,","))
			-- confirm purchase panel
			GetWidget('storeConfirmPurchaseGoldCost2'):SetText(FtoA(goldPrice, 0, 0 ,","))
			-- confirm purchase give gift panel
			GetWidget('storeConfirmPurchaseGoldCost3'):SetText(FtoA(goldPrice, 0, 0 ,","))
			GetWidget('storeConfirmPurchaseGoldCost4'):SetText(FtoA(goldPrice, 0, 0 ,","))

			if (goldPrice <= goldOwn) then
				GetWidget('storeConfirmPurchaseButtonBtnGold'):SetVisible(true)
				GetWidget('storeConfirmPurchaseButtonNeedGold2'):SetVisible(false)
			end
			if (silverPrice <= silverOwn) then
				GetWidget('storeConfirmPurchaseButtonBtnSilver'):SetVisible(true)
				GetWidget('storeConfirmPurchaseButtonNeedSilver'):SetVisible(false)
			end
		else
			label:SetVisible(false)
			Set('microStoreDiscountInfo', '')
		end
	else
		local listbox =	GetWidget('easy_CardListbox_store')
		listbox:SetSelectedItemByIndex(-1)
		HoN_Store.CardLastSelect = -1
		local goldPrice = GetCvarInt('microStoreCDPriceGold', true)
		local silverPrice = GetCvarInt('microStoreCDPriceSilver', true)
		local goldOwn = GetCvarInt('_microStore_TotalCoins', true)
		local silverOwn = GetCvarInt('_microStore_TotalSilverCoins', true)
		GetWidget('storeConfirmPurchaseGoldCost'):SetText(FtoA(goldPrice, 0, 0 ,","))
		GetWidget('storeConfirmPurchaseSilverCost'):SetText(FtoA(silverPrice, 0, 0 ,","))
		-- confirm purchase panel
		GetWidget('storeConfirmPurchaseGoldCost2'):SetText(FtoA(goldPrice, 0, 0 ,","))
		-- confirm purchase give gift panel
		GetWidget('storeConfirmPurchaseGoldCost3'):SetText(FtoA(goldPrice, 0, 0 ,","))
		GetWidget('storeConfirmPurchaseGoldCost4'):SetText(FtoA(goldPrice, 0, 0 ,","))

		Set('microStoreDiscountInfo', '')
		label:SetText(Translate('mstore_nocards_desc'))
		label:SetColor("red")
		if (goldPrice > goldOwn) then
			GetWidget('storeConfirmPurchaseButtonBtnGold'):SetVisible(false)
			GetWidget('storeConfirmPurchaseButtonNeedGold2'):SetVisible(true)
		end
		if (silverPrice > silverOwn) then
			GetWidget('storeConfirmPurchaseButtonBtnSilver'):SetVisible(false)
			GetWidget('storeConfirmPurchaseButtonNeedSilver'):SetVisible(true)
		end
	end
end

function HoN_Store:OnSelectHero(product_id, owned)

	if (owned == 0 and product_id > 0) then
		local cardsTable = GetCardsInfo(HoN_Store.heroTable[product_id].hero, '')
		if (cardsTable and #cardsTable > 0) then
			GetWidget('storeHero_cards_panel'):SetVisible(true)
		else
			GetWidget('storeHero_cards_panel'):SetVisible(false)
		end
	else
		GetWidget('storeHero_cards_panel'):SetVisible(false)
	end
end

-- Store accessor function
function interface:HoNStoreF(func, ...)
  print(HoN_Store[func](self, ...))
end


local function IsVault(name)
	local prefix = sub(name, 1, 6)
	return prefix == 'vault_'
end

HoN_Store.Store2TrashCanWidget = nil
HoN_Store.Store2TrashCan = {}
local function Store2TrashCanAddSingle(trash)
	HoN_Store.Store2TrashCanWidget = HoN_Store.Store2TrashCanWidget or GetWidget("store2_TrashCan")
	local frameTrash = HoN_Store.Store2TrashCan[HoN_Store.FrameCounter]
	if frameTrash == nil then
		HoN_Store.Store2TrashCan[HoN_Store.FrameCounter] = {}
		frameTrash = HoN_Store.Store2TrashCan[HoN_Store.FrameCounter]
	end

	local count = #frameTrash
	frameTrash[count+1] = trash

	trash:SetEnabled(0)
	trash:SetNoClick(1)
	trash:SetVisible(0)
	trash:SetWidth(0)
	trash:SetHeight(0)

	trash:SetParent(HoN_Store.Store2TrashCanWidget)
end

local function Store2TrashCanAddChildren(trashParent)
	HoN_Store.trashParent = trashParent
	local children = trashParent:GetChildren()
	for idx, child in ipairs(children) do
		Store2TrashCanAddSingle(child)
	end
end

local function Store2TrashCanFrame(currentFrame)
	for k, v in ipairs(HoN_Store.Store2TrashCan) do
		if k + 3 < currentFrame then
			for w in HoN_Store.Store2TrashCan[k] do
				w:Destroy()
			end
		end
		HoN_Store.Store2TrashCan[k] = nil
	end
end


Store2NavBackInProgress = false
Store2NavBackLastTime	= 0

HoN_Store.Store2CurrentPage = ""
HoN_Store.Store2CurrentPage_Shop = ""
HoN_Store.Store2CurrentPage_Vault = ""

function HoN_Store:Store2SetCurrentPage(page)
	if page == '' then
		HoN_Store.Store2CurrentPage = ''
		return
	end

	HoN_Store.Store2CurrentPage = page
	if IsVault(page) then
		HoN_Store.Store2CurrentPage_Vault = page
	else
		HoN_Store.Store2CurrentPage_Shop = page
	end
end

function HoN_Store:Store2GetCurrentPage(shopOrVault)
	if shopOrVault == nil then
		return HoN_Store.Store2CurrentPage
	elseif shopOrVault == 'shop' then
		return HoN_Store.Store2CurrentPage_Shop
	else
		return HoN_Store.Store2CurrentPage_Vault
	end
end

function HoN_Store:Store2ClearLastPage(shopOrVault)
	if shopOrVault == nil then
		HoN_Store.Store2CurrentPage = nil
	elseif shopOrVault == 'shop' then
		HoN_Store.Store2CurrentPage_Shop = nil
	else
		HoN_Store.Store2CurrentPage_Vault = nil
	end
end

HoN_Store.ShowPageFuncs =
{
	["tauntbadges"] = function(strForceRefresh) 		Trigger('store2VanityShow', 'gamevanity', 'tauntbadges', strForceRefresh) end,
	["tpeffects"] = function(strForceRefresh) 			Trigger('store2VanityShow', 'gamevanity', 'tpeffects', strForceRefresh) end,
	["selectioncircles"] = function(strForceRefresh) 	Trigger('store2VanityShow', 'gamevanity', 'selectioncircles', strForceRefresh) end,
	["taunts"] = function(strForceRefresh) 				Trigger('store2VanityShow', 'gamevanity', 'taunts', strForceRefresh) end,
	["announcers"] = function(strForceRefresh) 			Trigger('store2VanityShow', 'gamevanity', 'announcers', strForceRefresh) end,
	["couriers"] = function(strForceRefresh) 			Trigger('store2VanityShow', 'gamevanity', 'couriers', strForceRefresh) end,
	["wards"] = function(strForceRefresh) 				Trigger('store2VanityShow', 'gamevanity', 'wards', strForceRefresh) end,
	["upgrades"] = function(strForceRefresh) 			Trigger('store2VanityShow', 'gamevanity', 'upgrades', strForceRefresh) end,
	["accounticons"] = function(strForceRefresh) 		Trigger('store2VanityShow', 'accountvanity', 'accounticons', strForceRefresh) end,
	["namecolors"] = function(strForceRefresh) 			Trigger('store2VanityShow', 'accountvanity', 'namecolors', strForceRefresh) end,
	["symbols"] = function(strForceRefresh) 			Trigger('store2VanityShow', 'accountvanity', 'symbols', strForceRefresh) end,
	["others"] = function(strForceRefresh)				Store2Tab('others') Trigger('store2VanityShow', 'others', 'others', strForceRefresh) end,
	["vault_tauntbadges"] = function(strForceRefresh) 	Trigger('store2VanityShow', 'vault_gamevanity', 'vault_tauntbadges', strForceRefresh) end,
	["vault_tpeffects"] = function(strForceRefresh) 	Trigger('store2VanityShow', 'vault_gamevanity', 'vault_tpeffects', strForceRefresh) end,
	["vault_selectioncircles"] = function(strForceRefresh) 	Trigger('store2VanityShow', 'vault_gamevanity', 'vault_selectioncircles', strForceRefresh) end,
	["vault_announcers"] = function(strForceRefresh) 	Trigger('store2VanityShow', 'vault_gamevanity', 'vault_announcers', strForceRefresh) Trigger('ShopkeeperIdleRoutine', 5, 180000) end,
	["vault_couriers"] = function(strForceRefresh)		Trigger('store2VanityShow', 'vault_gamevanity', 'vault_couriers', strForceRefresh) Trigger('ShopkeeperIdleRoutine', 5, 180000) end,
	["vault_taunts"] = function(strForceRefresh) 		Trigger('store2VanityShow', 'vault_gamevanity', 'vault_taunts', strForceRefresh) Trigger('ShopkeeperIdleRoutine', 5, 180000) end,
	["vault_wards"] = function(strForceRefresh) 		Trigger('store2VanityShow', 'vault_gamevanity', 'vault_wards', strForceRefresh) Trigger('ShopkeeperIdleRoutine', 5, 180000) end,
	["vault_upgrades"] = function(strForceRefresh) 		Trigger('store2VanityShow', 'vault_gamevanity', 'vault_upgrades', strForceRefresh) Trigger('ShopkeeperIdleRoutine', 5, 180000) end,
	["vault_creeps"] = function(strForceRefresh) 		Trigger('store2VanityShow', 'vault_gamevanity', 'vault_creeps', strForceRefresh) Trigger('ShopkeeperIdleRoutine', 5, 180000) end,
	["vault_others"] = function(strForceRefresh) 		Store2Tab('vault_others') Trigger('store2VanityShow', 'vault_others', 'vault_others', strForceRefresh) Trigger('ShopkeeperIdleRoutine', 5, 180000) end,
	["vault_accounticons"] = function(strForceRefresh) 	Trigger('store2VanityShow', 'vault_accountvanity', 'vault_accounticons', strForceRefresh) Trigger('ShopkeeperIdleRoutine', 5, 180000) end,
	["vault_namecolors"] = function(strForceRefresh) 	Trigger('store2VanityShow', 'vault_accountvanity', 'vault_namecolors', strForceRefresh) Trigger('ShopkeeperIdleRoutine', 5, 180000) end,
	["vault_symbols"] = function(strForceRefresh) 		Trigger('store2VanityShow', 'vault_accountvanity', 'vault_symbols', strForceRefresh)  Trigger('ShopkeeperIdleRoutine', 5, 180000) end,
}

function HoN_Store:Store2ShowPage(page, forceRefresh)
	if page == '' or page == nil then return end
	if forceRefresh == nil then forceRefresh = false end

	local showOrRefreshPage = true
	local currentPageName = HoN_Store:Store2GetCurrentPage()

	if currentPageName == page and not Store2NavBackInProgress then
		local pageInfo = HoN_Store.paging[currentPageName]
		if pageInfo ~= nil then
			local currentPage = pageInfo.currentPage
			if currentPage == 1 then
				showOrRefreshPage = false
				if currentPageName == 'symbols' then
					symbolsComboboxIndex = GetWidget('store2_combobox_symbols_types'):GetSelectedItemIndex()
					if symbolsComboboxIndex ~= 0 then
						showOrRefreshPage = true
					end
				end
				pageInfo = HoN_Store.ProductPageInfo[page]
				if pageInfo.fetchAllPages and HoN_Store:IsVanityConditionsChanged(page) then
					HoN_Store:ResetVanityConditions(page)
					showOrRefreshPage = true
				end
			end
		else
			showOrRefreshPage = false
		end
	end

	if showOrRefreshPage or forceRefresh then
		local func = HoN_Store.ShowPageFuncs[page]
		if func then
			func(tostring(forceRefresh))
		end
	end
end

local Store2PendingJobs =
{
	['first'] = 0,
	['last'] = -1,
}

local VaultItemJumpInfo =
{
	["productId"] = nil,
	["category"] = nil,
}

function HoN_Store:JumpToVaultPage(page)
	if HoN_Store:IsVaultOpened() then
		HoN_Store:Store2ShowPage(page)
	else
		VaultItemJumpInfo['productId'] = ''
		VaultItemJumpInfo['category'] = page
		HoN_Store:Store2OpenVault(false)
		HoN_Store:Store2Loading(1, "JumpToVaultPage")
	end
end

function Store2PendingJobsIsEmpty()
	return Store2PendingJobs['last'] < Store2PendingJobs['first']
end

function HoN_Store:Store2PendingJobsClear()
	if Store2PendingJobsIsEmpty() then return end

	for i = Store2PendingJobs.first, Store2PendingJobs.last do
		Store2PendingJobs[i] = nil
	end

	Store2PendingJobs.first = 0
	Store2PendingJobs.last = -1
end

function HoN_Store:Store2PendingJobsPush(func)

	local index = Store2PendingJobs['last'] + 1
	Store2PendingJobs[index] = func
	Store2PendingJobs.last = index
end

function HoN_Store:Store2PendingJobsPop()
	if Store2PendingJobsIsEmpty() then return nil end

	local first = Store2PendingJobs['first']
	local func = Store2PendingJobs[first]
	Store2PendingJobs[first] = nil
	Store2PendingJobs['first'] = Store2PendingJobs['first'] + 1;

	return func
end

function HoN_Store:Store2PendingJobsPeek()
	if Store2PendingJobsIsEmpty() then return nil end

	local first = Store2PendingJobs['first']
	local func = Store2PendingJobs[first]

	return func
end

HoN_Store.FrameCounter = 0
function HoN_Store:Store2Frame()
	HoN_Store:Store2LoadingCheckTimeout()

	if GetCvarBool('store2ShowLoadingTime') then
		local label = GetWidget('store2_loadingTimeDisplay')
		label:SetVisible(1)
		label:SetText("LoadingTime: "..HoN_Store:Store2GetLoadingStatistics())
	end

	-- One job in a single frme for now.
	local func = HoN_Store:Store2PendingJobsPeek()
	if func then
		if func() then
			HoN_Store:Store2PendingJobsPop()
		end
	end

	Store2TrashCanFrame()

	HoN_Store.FrameCounter = HoN_Store.FrameCounter + 1
end

HoN_Store.IsInVaultOrShop = nil

function HoN_Store:Store2OpenVault(navToLast)
	if HoN_Store.IsInVaultOrShop == 'vault' then return end
	if navToLast == nil then navToLast = true end
	if not navToLast then HoN_Store:Store2ClearLastPage('vault') end

	Trigger('store2VanityShow', '', '')

	GetWidget('store2_store'):SetVisible(0)
	GetWidget('store2_store_bg'):SetVisible(0)
	GetWidget('store2_vault'):SetVisible(1)
	GetWidget('store2_vault_bg'):SetVisible(1)
	GetWidget('store2_store_common_elements_button_open_shop'):SetVisible(1)
	GetWidget('store2_store_common_elements_button_open_vault'):SetVisible(0)
	GetWidget('store2_specials_booth_gift_button_vanity'):SetVisible(false)

	GetWidget('shopkeepr_shop'):SetVisible(false)
	GetWidget('shopkeepr_vault'):SetVisible(true)
	
	Trigger('store2TabMenuHide', 'all')

	HoN_Store:Store2FormGetVault()

	HoN_Store.IsInVaultOrShop = 'vault'
end

function HoN_Store:Store2OpenShop(navToLast)
	if HoN_Store.IsInVaultOrShop == 'shop' then return end
	if navToLast == nil then navToLast = true end
	if not navToLast then HoN_Store:Store2ClearLastPage('shop') end

	Trigger('store2VanityShow', '', '')

	GetWidget('store2_vault'):SetVisible(0)
	GetWidget('store2_vault_bg'):SetVisible(0)
	GetWidget('store2_store'):SetVisible(1)
	GetWidget('store2_store_bg'):SetVisible(1)
	GetWidget('store2_store_common_elements_button_open_vault'):SetVisible(1)
	GetWidget('store2_store_common_elements_button_open_shop'):SetVisible(0)

	GetWidget('shopkeepr_shop'):SetVisible(true)
	GetWidget('shopkeepr_vault'):SetVisible(false)

	Trigger('store2TabMenuHide', 'all')

	local lastShopPage = HoN_Store:Store2GetCurrentPage('shop')
	HoN_Store:Store2ShowPage(lastShopPage)

	HoN_Store.IsInVaultOrShop = 'shop'
end

function HoN_Store:IsVaultOpened()
	return GetWidget('store2_vault'):IsVisible()
end

function HoN_Store:OnClickShopkeeper()
	local isInSpecialsPage = false
	local isInVaunt = HoN_Store:IsVaultOpened()
	local tmp = Store2NavBackInProgress

	if isInVaunt then
		HoN_Store:Store2OpenShop(false)
		Store2NavBackInProgress = true
	else
		isInSpecialsPage = HoN_Store:Store2GetCurrentPage() == 'store2_specials'
	end

	if isInSpecialsPage then
		HoN_Store:SwitchToBundleListPage(1)
	else
		Store2Tab('specials')
	end

	if isInVaunt then
		Store2NavBackInProgress = tmp
	end
end

local STORE2_RESPONSE_CODE_BASICITEMLIST = 1
local STORE2_RESPONSE_CODE_VAULTITEMLIST = 2
local STORE2_RESPONSE_CODE_BUNDLECONTENTS = 6
local STORE2_RESPONSE_CODE_UPDATE_SELECTED = 7

local STORE2_POPUPCODE_NONE = -1
local STORE2_POPUPCODE_ERROR = 1
local STORE2_POPUPCODE_BUYCOIN = 2
local STORE2_POPUPCODE_BUYITEM = 3
local STORE2_POPUPCODE_FIRSTVISIT = 4
local STORE2_POPUPCODE_COINFORFRIENDS = 5
local STORE2_POPUPCODE_VAULTAVATAR =6

local ActiveDebugPanelID = 1

function HoN_Store:Store2PagingStateClear(page)
	if page ~= nil then
		local info = HoN_Store.paging[page]
		if info ~= nil then
			info.currentPage = 1
		end

		local info2 = HoN_Store.ProductPageInfo[page]
		if info2 ~= nil then
			info2.currentPage = 1
		end
		return
	end

	for k, v in pairs(HoN_Store.paging) do
		if v ~= nil and type(v) == 'table' and v.currentPage ~= nil then
			v.currentPage = 1
		end
	end

	for k, v in pairs(HoN_Store.ProductPageInfo) do
		if v ~= nil and type(v) == 'table' and v.currentPage ~= nil then
			v.currentPage = 1
		end
	end
end

-- paging
HoN_Store.paging = {
	hero = {
		countPerPage = 10,
		currentPage = 1,
		handler = function(...) HoN_Store:ShowHeroesPage(...) end,
	},
	heroAvatars = {
		countPerPage = 8,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:ShowHeroAvatarPanels(page, 0, true) end,
	},
	avatars = {
		countPerPage = 10,
		currentPage = 1,
		handler = function(...) HoN_Store:ShowAvatarsPage(...) end,
	},
	accounticons = {
		countPerPage = 24,
		currentPage = 1,
		handler = function(page) HoN_Store:Store2FormGetProducts('accounticons', page, true) end,
	},
	namecolors = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2FormGetProducts('namecolors', page, true) end,
	},
	symbols = {
		countPerPage = 24,
		currentPage = 1,
		handler = function(page) HoN_Store:Store2FormGetProducts('symbols', page, true) end,
	},
	taunts = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2FormGetProducts('taunts', page, true) end,
	},
	tauntbadges = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2FormGetProducts('tauntbadges', page, true) end,
	},
	announcers = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2FormGetProducts('announcers', page, true) end,
	},
	couriers = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2FormGetProducts('couriers', page, true) end,
	},
	wards = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2FormGetProducts('wards', page, true) end,
	},
	tpeffects = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2FormGetProducts('tpeffects', page, true) end,
	},
	selectioncircles = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2FormGetProducts('selectioncircles', page, true) end,
	},
	upgrades = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2FormGetProducts('upgrades', page, true) end,
	},
	others = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2FormGetProducts('others', page, true) end,
	},
	vault_accounticons = {
		countPerPage = 24,
		currentPage = 1,
		handler = function(page) HoN_Store:Store2InitializeVaultUI('vault_accounticons', page, true) end,
	},
	vault_namecolors = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2InitializeVaultUI('vault_namecolors', page, true) end,
	},
	vault_symbols = {
		countPerPage = 24,
		currentPage = 1,
		handler = function(page) HoN_Store:Store2InitializeVaultUI('vault_symbols', page, true) end,
	},
	vault_taunts = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2InitializeVaultUI('vault_taunts', page, true) end,
	},
	vault_tauntbadges = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2InitializeVaultUI('vault_tauntbadges', page, true) end,
	},
	vault_announcers = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2InitializeVaultUI('vault_announcers', page, true) end,
	},
	vault_couriers = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2InitializeVaultUI('vault_couriers', page, true) end,
	},
	vault_wards = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2InitializeVaultUI('vault_wards', page, true) end,
	},
	vault_tpeffects = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2InitializeVaultUI('vault_tpeffects', page, true) end,
	},
	vault_selectioncircles = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2InitializeVaultUI('vault_selectioncircles', page, true) end,
	},
	vault_upgrades = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2InitializeVaultUI('vault_upgrades', page, true) end,
	},
	vault_creeps = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2InitializeVaultUI('vault_creeps', page, true) end,
	},
	vault_others = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = true,
		handler = function(page) HoN_Store:Store2InitializeVaultUI('vault_others', page, true) end,
	},
	bundles = {
		countPerPage = 12,
		currentPage = 1,
		noIndexes = false,
		handler = function(page) HoN_Store:Store2FormGetProducts('bundles', page, true) end,
	},
	bundleDetail = {
		countPerPage = 8,
		currentPage = 1,
		noIndexes = false,
		handler = function(page) HoN_Store:ShowBundleDetailPage(page, 0, true) end,
	},
	StartIndex =
		function(page, cat)
			local config = HoN_Store.paging[cat]
			if not config then return -1 end
			return (page - 1) * config.countPerPage  + 1
		end,
	LastIndex =
		function(page, cat)
			local config = HoN_Store.paging[cat]
			if not config then return -1 end
			return page * config.countPerPage
		end,
	MaxPage =
		function(cat, count)
			local config = HoN_Store.paging[cat]
			local maxPage = (count - 1) / config.countPerPage  + ((config.countPerPage > 1 and 1) or 0)
			return math.floor(maxPage)
		end
}

HoN_Store.comboItems = {}

-- test only
SetSave('store2_request_interval', '0', 'int', true)
SetSave('store2_enable_edit', false, 'bool', true)
Set('store2_test_avatar_icons', false, 'bool', true)

HoN_Store.widgetToHeroAvatarStrMap = {}

-- heroes list
HoN_Store.editingHeroPair = {}

HoN_Store.ProductHeroes =
{
	Count 				= 0,
	SortedIndexes 		= {},
	FiltedIndexes 		= {},

	PreFiltedIndexes 	= {},

	FilterString 		= '',

	Ids 				= {},
	Codes 				= {},

	Purchasable 		= {},

	Names 				= {},
	GoldPrice 			= {},
	SilverPrice 		= {},
	Owned 				= {},
	Difficulty 			= {},
	PrimAttr 			= {},
	AttackType 			= {},

	SoloRating 			= {},
	JungleRating 		= {},
	CarryRating 		= {},
	SupportRating 		= {},
	InitiatorRating 	= {},
	GankerRating 		= {},
	PusherRating 		= {},
	RangedRating 		= {},
	MeleeRating 		= {},

	Categories 			= {},
	PageEverShown		= false,
}

-- avatars
HoN_Store.ProductAvatars =
{
	Count 				= 0,
	SortedIndexes 		= {},
	FiltedIndexes 		= {},

	PreFiltedIndexes 	= {},

	FilterString 		= '',

	Ids 				= {},
	Codes 				= {},
	Purchasable 		= {},
	GoldPrice 			= {},
	SilverPrice 		= {},
	Owned 				= {},

	PageEverShown		= false,

	Eligibility			= {},
	Enhancements		= {},
}

HoN_Store.HeroDetailPage =
{
	DefaultAvatarIndex 		= 0,

	IsShowing				= false,
	FromAvatarList			= false,

	HeroIndexToInit 		= 0,
	AvatarIndexToInit 		= 0,
	HeroProductIdToInit 	= '0',
	AvatarProductIdToInit 	= '0',
	HeroCodeToInit			= nil,
	AvatarCodeToInit		= nil,

	CurrentHeroName			= nil,
	CurrentHeroIndex 		= -1,
	CurrentAvatarIndex 		= -1,
	CurrentHeroAvatars 		= {},
	CurrentHeroAvatarStr	= nil,
	CurrentHeroFiltedIndex 	= 0,

	CurrentHeroGoldCost 	= nil,
	CurrentHeroSilverCost 	= nil,
}

HoN_Store.BackToListCallback = nil

-- edit
local EditData = {
}

-- common
local function GetProductDispayNameByID(id, defaultStr)
	if not id then return '' end
	local t = Translate('mstore_product'..id..'_name')
	if defaultStr and t == 'mstore_product'..id..'_name' then
		return defaultStr
	end
	return t
end

local function GetProductDisplayDescByID(id, defaultStr)
	if not id then return '' end
	local t = Translate('mstore_product'..id..'_desc')
	if defaultStr and t == 'mstore_product'..id..'_desc' then
		return defaultStr
	end
	return t
end

local function IsFree(iGoldCost, iSilverCost)
	return (iGoldCost == 0 or iGoldCost == 9006) and (iSilverCost == 0 or iSilverCost == 9002)
end

local function ShowGoldSiver(goldCost, silverCost)
	local goldCost = tonumber(goldCost)
	local silverCost = tonumber(silverCost)
	local showGold = goldCost ~= nil and goldCost > 0 and (goldCost < 9001 or goldCost > 9010)
	local showSilver = silverCost ~= nil and silverCost ~= 9002 and silverCost ~= 0
	return showGold, showSilver
end

-- eligibilty stuff
local function ParseEligiblityStrings(data, eligLists, productID)

	local eligIndex = tostring(productID)
	local eligEntry	= eligLists[eligIndex]

	if eligEntry == nil then return false, nil end

	data.requiredProductTooltip = ""
	if eligEntry.eligbleID then
		local requireTable = nil

		if (eligEntry.requiredProducts) then
			requireTable = explode(';', eligEntry.requiredProducts)
		else
			requireTable = explode('|', Translate('mstore_elig_requirements_' .. productID))
		end

		if requireTable then
			data.requiredProductTooltip = Translate("mstore_elig_required")
			for index, requiredProductID in pairs(requireTable) do
				local translateReqProductID = Translate('mstore_product'..requiredProductID..'_name')
				if (translateReqProductID ~= 'mstore_product'..requiredProductID..'_name') then
					data.requiredProductTooltip = data.requiredProductTooltip .. '\n' .. translateReqProductID
				end
			end
		end
	end

	local eligDescKey = 'mstore_elig_package_'..productID
	data.eligDescStr = Translate(eligDescKey)
	if data.eligDescStr == eligDescKey then
		data.eligDescStr = ""
	end

	local eligible = AtoB(eligEntry.eligible)
	data.showEligPrice = not eligible

	return eligible, eligEntry
end

-- copied form HoN_Store:DisplayAvatarList()
local function ParseSpecialPriceValue(goldCost, silverCost, purchasable, productID, heroEntry, eligLists, isAltAvatar)

	-- 900x case for avatars
	local function ParseAvatarSpecialPrices(data, heroEntry)

		local isSpecialPrice = data.goldCost >= 9001 and data.goldCost <= 9010

		if isSpecialPrice then
			data.showEAPPrice 		= data.goldCost == 9001
			data.showPlinkoPrice 	= data.goldCost == 9003
			data.showEsportsPrice 	= data.goldCost == 9004
			data.showQuestsPrice 	= data.goldCost == 9005
			data.showCodexPrice 	= data.goldCost == 9007
			data.showLuckyDrawPrice	= data.goldCost == 9008
			data.showPunkPrice 		= data.goldCost == 9009
			data.showEventPrice		= data.goldCost == 9010
		end

		local var = GetCvarInt('microStoreSpecialDisplay'..heroEntry.heroEntryID, true)
		if var then
			data.showEAPPrice = var == 1
		end

		data.showGoldPrice = data.showGoldPrice and (not isSpecialPrice or data.goldCost == 9006)

		if isSpecialPrice then
			if data.showEligPrice then
				data.priceStrKey = 'mstore_elig_only'
			elseif data.showEAPPrice then
				data.priceStrKey = 'mstore_eap_only'
			elseif data.showPlinkoPrice then
				data.priceStrKey = 'mstore_plinko_only'
			elseif data.showEsportsPrice then
				data.priceStrKey = 'mstore_esports_only'
			elseif data.showQuestsPrice then
				data.priceStrKey = 'mstore_quests_only'
			elseif data.showCodexPrice then
				data.priceStrKey = 'mstore_codex_only'
			elseif data.showLuckyDrawPrice then
				data.priceStrKey = 'mstore_luckydraw_only'
			elseif data.showPunkPrice then
				data.priceStrKey = 'mstore_pon_only'
			elseif data.showEventPrice then
				data.priceStrKey = 'mstore_event_only'
			end
		end

		data.showSilverPrice = data.showGoldPrice
	end

	local data = {
		showEligPrice 		= false,
		showGoldPrice		= true,
		showSilverPrice		= true,
		showEAPPrice		= false,
		showPlinkoPrice		= false,
		showEsportsPrice	= false,
		showQuestsPrice		= false,
		showCodexPrice		= false,
		showLuckyDrawPrice	= false,
		showPunkPrice		= false,

		goldCost			= tonumber(goldCost),
		silverCost			= tonumber(silverCost),
		purchasable			= purchasable,
		priceStrKey 		= nil,
	}

	if eligLists then
		local eligible, eligEntry = ParseEligiblityStrings(data, eligLists, productID)
		if eligible then				-- satisfy Eligiblity requirements
			if eligEntry.requiredProducts and string.len(eligEntry.requiredProducts) > 0 then -- if requeired list is empty, regard as requirements not met
				data.purchasable = true
			end
		elseif eligEntry ~= nil then
			if eligEntry.goldCost == 0 and eligEntry.silverCost == 0 then
				data.purchasable = false	-- special case: show unlock me if Eligiblity is 0/0 and Eligiblity requirements not met
			elseif data.purchasable and isAltAvatar then
				data.showEligPrice = false	-- not show 'unlock me' for purchasable avatars even if Eligiblity requirements not met
			end
		end
	end

	if heroEntry ~= nil then
		ParseAvatarSpecialPrices(data, heroEntry)	-- after ParseEligiblityStrings()
	end

	if data.showGoldPrice then
		data.showGoldPrice	 = data.goldCost and data.goldCost ~= 9006 and data.goldCost ~= 0
		data.showSilverPrice = data.silverCost and data.silverCost ~= 9002 and data.silverCost ~= 0
		if not data.showGoldPrice and not data.showSilverPrice then
			data.priceStrKey = 'mstore_free'
		end
	else
		data.showSilverPrice = false
	end

	return data
end

-- sort/filter heroes list
local function GetHeroTable(str)
	return HoN_Store.ProductHeroes[str]
end

local function CompNumber(v1, v2, asc)
	if asc then return v1 < v2 end
	return v2 < v1
end

local function CompString(v1, v2, asc)
	if not v1 then return false end
	if not v2 then return true end
	v1 = string.lower(v1)
	v2 = string.lower(v2)
	if asc then return v1 < v2 end
	return v2 < v1
end

local function CompFunc(getValueFunc, asc, number, comp2)
	if number then
		return function(i1, i2)
			local v1 = getValueFunc(i1)
			local v2 = getValueFunc(i2)
			if v1 == v2 and comp2 ~= nil then return comp2(i1, i2) end
			return CompNumber(v1, v2, asc)
		end
	else
		return function(i1, i2)
			local v1 = getValueFunc(i1)
			local v2 = getValueFunc(i2)
			if v1 == v2 and comp2 ~= nil then return comp2(i1, i2) end
			return CompString(v1, v2, asc)
		end
	end
end

local function CompFunc2(tbFunc, tbName, asc, number, comp2)
	local getValueFunc = function(i)
			local tb = tbFunc(tbName)
			tb = tb[i]
			if number then
				tb = tonumber(tb) or -2
			end
			return tb
		 end
	return CompFunc(getValueFunc, asc, number, comp2)
end

local function CompPrice(tbFunc, tbName, asc, comp2)
	local getValueFunc = function(i)
			local tb = tbFunc(tbName)
			tb = tonumber(tb[i]) or -2
			if tb == 9006 then
				tb = -1
			end
			return tb
		 end
	return CompFunc(getValueFunc, asc, true, comp2)
end


local function FilterFunc(tbFunc, tbName, test, isPattern)
	if isPattern then
		return function(idx)
			local tb = tbFunc(tbName)
			return tb[idx]:match(test)
		end
	else
		return function(idx)
			local tb = tbFunc(tbName)
			return tb[idx] == test
		end
	end
end

local function FilterRateFunc(tbFunc, tbName, min)
	return function(idx)
		local tb = tbFunc(tbName)
		return tb[idx] >= min
	end
end

local HeroRatingThreshold = 3.0
HoN_Store.comboItems.hero = {
	sortOrder = {
		{ label = 'store2_release_date_desc', 		comp = CompFunc2(GetHeroTable, 'Ids', false, true) },
		{ label = 'store2_release_date_asc', 		comp = CompFunc2(GetHeroTable, 'Ids', true, true) },
		{ label = 'store2_hero_name_desc', 			comp = CompFunc2(GetHeroTable, 'Names', false) },
		{ label = 'store2_hero_name_asc', 			comp = CompFunc2(GetHeroTable, 'Names', true) },
		{ label = 'store2_price_desc', 				comp = CompPrice(GetHeroTable, 'GoldPrice', false) },
		{ label = 'store2_price_asc', 				comp = CompPrice(GetHeroTable, 'GoldPrice', true) },
		{ label = 'store2_hero_difficulty_desc',	comp = CompFunc2(GetHeroTable, 'Difficulty', false, true) },
		{ label = 'store2_hero_difficulty_asc',		comp = CompFunc2(GetHeroTable, 'Difficulty', true, true) },
	},
	primAttr = {
		{ label = 'store2_hero_attr_primary' },
		{ label = 'store2_hero_attr_strength',		filter = FilterFunc(GetHeroTable, 'PrimAttr', 'S') },
		{ label = 'store2_hero_attr_agility',		filter = FilterFunc(GetHeroTable, 'PrimAttr', 'A') },
		{ label = 'store2_hero_attr_intelligence',	filter = FilterFunc(GetHeroTable, 'PrimAttr', 'I') },
	},
	attackType = {
		{ label = 'store2_hero_attack_type'},
		{ label = 'store2_hero_attack_melee',		filter = FilterFunc(GetHeroTable, 'AttackType', 'M') },
		{ label = 'store2_hero_attack_ranged',		filter = FilterFunc(GetHeroTable, 'AttackType', 'R') },
	},
	role = {
		{ label = 'store2_hero_role' },
		{ label = 'filter_category_solo', 			filter = FilterRateFunc(GetHeroTable, 'SoloRating', 	HeroRatingThreshold)  },
		{ label = 'filter_category_jungle', 		filter = FilterRateFunc(GetHeroTable, 'JungleRating', 	HeroRatingThreshold)  },
		{ label = 'filter_category_carry', 			filter = FilterRateFunc(GetHeroTable, 'CarryRating', 	HeroRatingThreshold)  },
		{ label = 'filter_category_support', 		filter = FilterRateFunc(GetHeroTable, 'SupportRating', 	HeroRatingThreshold)  },
		{ label = 'filter_category_initiator', 		filter = FilterRateFunc(GetHeroTable, 'InitiatorRating',HeroRatingThreshold)  },
		{ label = 'filter_category_ganker', 		filter = FilterRateFunc(GetHeroTable, 'GankerRating', 	HeroRatingThreshold)  },
		{ label = 'filter_category_pusher', 		filter = FilterRateFunc(GetHeroTable, 'PusherRating', 	HeroRatingThreshold)  },
	}
}

-- sort/filter avatars list
local function GetAvatarTable(str)
	return HoN_Store.ProductAvatars[str]
end

local function GetHeroNameByAvatarIndex(i)
	local heroAvatarStr = HoN_Store.ProductAvatars.Codes[i]
	local heroName 		= split(heroAvatarStr, '%.')[2]
	return heroName
end

local function GetDisplayName(name)
	local key = "mstore_"..name.."_name"
	local t = Translate(key)
	if t == key then return name end
	return t
end

local function GetHeroDisplayNameByAvatarIndex(i)
	local heroName 		= GetHeroNameByAvatarIndex(i)
	if heroName == nil then
		TestEcho('^rnil heroName for '..tostring(i))
		return ''
	end
	return GetDisplayName(heroName)
end

local function GetAvatarName(i)
	local productID = HoN_Store.ProductAvatars.Ids[i]
	return GetProductDispayNameByID(productID)
end

local function GetAvatarEntryId(i)
	local productID = HoN_Store.ProductAvatars.Ids[i]
	if not HoN_Store.allAvatarsInfo[tonumber(productID)] then
		return -1
	end
	local heroEntryID = HoN_Store.allAvatarsInfo[tonumber(productID)].heroEntryID
	return heroEntryID
end

local function FilterAvatarHeroOwned(owned)
	return function(idx)
		local heroName = GetHeroNameByAvatarIndex(idx)
		local heroOwned = AtoB(interface:UICmd("CanAccessHeroProduct('"..heroName.."')"))
		if owned then return heroOwned end
		return not heroOwned
	end
end

local compareAvatarNameAsc = CompFunc(GetAvatarName, true)

local compareAvatarHeroNameAsc = function(i1, i2)
	local codes = HoN_Store.ProductAvatars.Codes
	local heroName1 =  split(codes[i1], '%.')[2]
	local heroName2 =  split(codes[i2], '%.')[2]
	if heroName1 == heroName2 then
		return compareAvatarNameAsc(i1, i2)
	else
		local name1 = string.lower(Translate("mstore_"..heroName1.."_name"))
		local name2 = string.lower(Translate("mstore_"..heroName2.."_name"))
		return name1 < name2
	end
end

local compareAvatarNameLengthDesc = function(i1, i2)
	local s1 = GetAvatarName(i1)
	local s2 = GetAvatarName(i2)
	if string.len(s1) ~= string.len(s2) then
		return string.len(s1) > string.len(s2)
	else
		return s1 > s2
	end
end

HoN_Store.comboItems.avatars = {
	sortOrder = {
		{ label = 'store2_release_date_desc', 		comp = CompFunc(GetAvatarEntryId, false, true, compareAvatarNameAsc) },
		{ label = 'store2_hero_name_asc', 			comp = compareAvatarHeroNameAsc },
		{ label = 'store2_price_desc', 				comp = CompPrice(GetAvatarTable, 'GoldPrice', false, compareAvatarHeroNameAsc) },
		{ label = 'store2_price_asc', 				comp = CompPrice(GetAvatarTable, 'GoldPrice', true, compareAvatarHeroNameAsc) },
		{ label = 'store2_avatar_name_asc', 		comp = compareAvatarNameAsc },
	},
	owned = {
		{ label = 'mstore_avatar_show_allavatars'},
		{ label = 'mstore_avatar_show_ownedavatars',	filter = FilterFunc(GetAvatarTable, 'Owned', '1') },
		{ label = 'mstore_avatar_show_unownedavatars',	filter = FilterFunc(GetAvatarTable, 'Owned', '0') },
	},
	ownedHeroes = {
		{ label = 'mstore_avatar_show_allavatars'},
		{ label = 'mstore_avatar_show_ownedheroes',		filter = FilterAvatarHeroOwned(true) },
		{ label = 'mstore_avatar_show_unownedheroes',	filter = FilterAvatarHeroOwned(false) },
		{ label = 'mstore_avatar_show_ownedavatars',	filter = FilterFunc(GetAvatarTable, 'Owned', '1') },
		{ label = 'mstore_avatar_show_unownedavatars',	filter = FilterFunc(GetAvatarTable, 'Owned', '0') },
	},
}

local testMiddleFrameNameLength = false
if testMiddleFrameNameLength then
	table.insert(HoN_Store.comboItems.avatars.sortOrder,
		{ label = 'test_length', comp = compareAvatarNameLengthDesc})
end

local overrideAvatarTypes = nil
local function SetModelPanelByHeroAvatar(w, heroName, altCode, avatarType)

	for i=0, MODEL_PANEL_EFFECT_CHANNEL_MAX do
		w:SetMultiEffect('', i)
	end
	
	if not heroName or not altCode then
		w:SetModel('')
		w:SetEffect('')
		return
	end

	if overrideAvatarTypes ~= nil and overrideAvatarTypes ~= '' then
		avatarType = overrideAvatarTypes
	end
	local data = GetHeroPreviewDataFromDB(heroName, altCode, avatarType)

	w:SetModel(data.modelPath)

	if data.usingEffect then
		w:SetEffect(data.effectPath)
	else
		w:SetEffect('')
	end

	if HoN_Store:EditEnabled() then
		EditData[w] = {
			usingEffect = data.usingEffect,
			effectPath = data.effectPath,
		}
	end

	local scale = data.modelScale
	local pos = data.modelPosition
	local angle = data.modelAngle
	local ambient = data.ambient
	local sunHeight = data.sunHeight
	local sunAngle = data.sunAngle
	local sunColor = data.sunColor
	local pedestalScale = data.pedestalScale

	w:SetModelScale(scale)
	w:SetModelPos(pos.x, pos.y, pos.z)
	w:SetModelAngles(angle.x, angle.y, angle.z)

	w:SetAmbientColor(ambient.x, ambient.y, ambient.z)
	w:SetSunPosition(sunHeight, sunAngle)
	w:SetSunColor(sunColor.x, sunColor.y, sunColor.z)

	w:SetModelScale(0, pedestalScale)

	if avatarType == 'large' then
		local tag = data.tag or ''
		local norotate = string.match(' '..tag..' ', '[%s]norotate[%s]')
		if norotate then norotate = true else norotate = false end
		w:SetNoClick(norotate)
	end
	GetWidget('store2_detail_heroAvatars_right_icons_hasnewanims'):SetVisible(w:GetValidAnimCount() > 0)
end

local function MatchesAvatarCode(m)
	if m == nil then return false end
	return string.match(m, '^aa%.')
end

local function ParseHeroAvatar(m)
	if not m or m =='' then return nil, nil end
	local arr = split(m, '%.')

	local hero = arr[1]
	local avatar = arr[2]

	if avatar == 'Hero' then
		avatar = ''
	elseif not avatar then
		return nil, nil
	else
		avatar = hero..'.'..avatar
	end

	return hero, avatar
end

local function GetEditingHeroPair()
	return 	HoN_Store.editingHeroPair.widget,
			HoN_Store.editingHeroPair.heroAvatar,
			HoN_Store.editingHeroPair.type
end

local function SetEditingHeroPair(w, str, t)
	HoN_Store.editingHeroPair.widget = w
	HoN_Store.editingHeroPair.heroAvatar = str
	HoN_Store.editingHeroPair.type = t
end

function HoN_Store:SetModelPanel(w, m, t)
	local hero, avatar = ParseHeroAvatar(m)
	SetModelPanelByHeroAvatar(w, hero, avatar, t)
end

local function UpdateEditorInfo()
	-- Not for retail
end

function HoN_Store:EditEnabled()
	-- Not for retail
	return false
end

function HoN_Store:HideDebugPanel()
	-- Not for retail
end
function HoN_Store:EditEnableEffect(b)
	-- Not for retail
end

function HoN_Store:SetPanelSettings()
	-- Not for retail
end

function HoN_Store:SavePanelSettings()
	-- Not for retail
end

function HoN_Store:ToggleAmbientRgb()
	-- Not for retail
end

function HoN_Store:ToggleSunColorRgb()
	-- Not for retail
end

function HoN_Store:ModelPanelEditorCopyData(fromType, toCat)
	-- Not for retail
end

function HoN_Store:ModelPanelEditorMoveModel(dir, step)
	-- Not for retail
end

function HoN_Store:ModelPanelEditorGetEffectSettingType()
	if not GetWidget('model_panel_editor_newfeature'):IsVisible() then
		return 'normal'
	end

	local state = GetWidget('modelpanel_editor_effect_set'):GetButtonState()
	if state == 1 then return 'set' end

	for i=1,MODEL_PANEL_EFFECT_DISPLAY_MAX do
		state = GetWidget('modelpanel_editor_effect_'..i):GetButtonState()
		if state == 1 then return tostring(i) end
	end
	
	return 'normal'
end

function HoN_Store:ModelPanelEditorSetEffectSettingType(type)
	GetWidget('modelpanel_editor_effect_set'):SetButtonState(0)
	GetWidget('modelpanel_editor_effect_normal'):SetButtonState(0)
	for i=1,MODEL_PANEL_EFFECT_DISPLAY_MAX do
		GetWidget('modelpanel_editor_effect_'..i):SetButtonState(0)
	end

	GetWidget('modelpanel_editor_effect_'..type):SetButtonState(1)
end

function HoN_Store:ModelPanelEditorDeleteEffect(type, id)
	local w, m, t = GetEditingHeroPair()
	if type == 'set' then
		w:SetMultiEffect('', MODEL_PANEL_EFFECT_CHANNEL_MAX)
		UpdateEditorInfo()
	else
		local widget = GetWidget('model_panel_editor_effect_'..type..'_'..id)
		local text = widget:GetText()
		if Empty(text) then return end

		local typeid = tonumber(type)
		if EditData[w] == nil or EditData[w].multieffects == nil or EditData[w].multieffects[typeid] == nil then return end
		for i,v in ipairs(EditData[w].multieffects[typeid]) do
			if v == text then
				table.remove(EditData[w].multieffects[typeid], i)
				break
			end
		end
		HoN_Store:SetPanelSettings()
		HoN_Store:ModelPanelUpdateMultiEffects()
	end
end

function HoN_Store:ModelPanelAddMultiEffectPath(type, path)
	local w, m, t = GetEditingHeroPair()
	if EditData[w] == nil or EditData[w].multieffects == nil then return end

	local typeid = tonumber(type)

	EditData[w].multieffects[typeid] = EditData[w].multieffects[typeid] or {}
	for i,v in ipairs(EditData[w].multieffects[typeid]) do
		if v == path then return end
	end
	table.insert(EditData[w].multieffects[typeid], path)
end

function HoN_Store:ModelPanelUpdateMultiEffects()
	local w, m, t = GetEditingHeroPair()

	if EditData[w] == nil then return end

	EditData[w].multieffects = {}
	local hero, avatar = ParseHeroAvatar(m)
	if HoN_Store.EditorPanel_CurrentModelIndex > 0 then
		avatar = avatar..'_'..HoN_Store.EditorPanel_CurrentModelIndex
	end
	local data = GetHeroPreviewDataFromDB(hero, avatar, 'large')

	for i,v in ipairs(data.multieffects) do
		local effects = explode('|', v) or {}
		EditData[w].multieffects[i] = effects
	end

	local validnum = 0

	for i=1,MODEL_PANEL_EFFECT_DISPLAY_MAX do
		for j=1,MODEL_PANEL_EFFECT_CHANNEL_MAX do
			local effectPath = ''
			if EditData[w].multieffects[i] ~= nil and EditData[w].multieffects[i][j] ~= nil then
				effectPath = EditData[w].multieffects[i][j]
			end
			GetWidget('model_panel_editor_effect_'..i..'_'..j):SetText(effectPath)

			if NotEmpty(effectPath) then validnum = i end
		end
	end

	for i=1,MODEL_PANEL_EFFECT_DISPLAY_MAX do
		if i <= (validnum + 1) then 
			GetWidget('modelpanel_editor_effect_'..i..'_mask'):SetVisible(false)
		else
			GetWidget('modelpanel_editor_effect_'..i..'_mask'):SetVisible(true)
		end
	end

	for i=0,MODEL_PANEL_EFFECT_CHANNEL_MAX-1 do
		w:SetMultiEffect('', i)
	end

	if EditData[w].usingEffect then
		w:SetEffect(EditData[w].effectPath)
	else
		w:SetEffect('')
	end

	local effecttype = HoN_Store:ModelPanelEditorGetEffectSettingType()
	if effecttype ~= 'normal' and effecttype ~= 'set' then
		local typeid = tonumber(effecttype)
		local effects = EditData[w].multieffects[typeid]
		w:SetEffect('')

		if effects ~= nil then
			for i,v in ipairs(effects) do
				w:SetMultiEffect(v, i)
			end
		end
	end
	
	for i=1,9 do
		GetWidget('model_panel_editor_effect_group_file_path_'..i):SetVisible(false)
	end
	GetWidget('model_panel_editor_effect_set_path'):SetVisible(false)

	if effecttype == 'set' then
		GetWidget('model_panel_editor_effect_set_path'):SetVisible(true)
	elseif effecttype ~= 'normal' then
		GetWidget('model_panel_editor_effect_group_file_path_'..effecttype):SetVisible(true)
	end

	local effectnum = #(EditData[w].multieffects)
	for i=1,9 do
		GetWidget('modelpanel_editor_effect_'..i):SetVisible(i <= (effectnum + 1))
	end
end

function HoN_Store:ModelPanelEditorGetCurrentSubModelNum()
	local w, m, t = GetEditingHeroPair()
	local hero, avatar = ParseHeroAvatar(m)

	local modelcount = 0
	for i=1,6 do	
		local tempavatar = avatar..'_'..i
		local data = GetHeroPreviewDataFromDB(hero, tempavatar, 'large')
		if data.newFeatureModel then
			modelcount = modelcount + 1
		end
	end
	return modelcount
end

function HoN_Store:ModelPanelEditorUpdateMultiModelInfo()
	local modelcount = HoN_Store:ModelPanelEditorGetCurrentSubModelNum()
	
	for i=0,6 do
		GetWidget('modelpanel_editor_multimodels_select_'..i):SetVisible(i <= modelcount) 
		if i == HoN_Store.EditorPanel_CurrentModelIndex then
			GetWidget('modelpanel_editor_multimodels_select_'..i):SetButtonState(1)
		else
			GetWidget('modelpanel_editor_multimodels_select_'..i):SetButtonState(0)
		end
	end

	GetWidget('addMultiModel'):SetEnabled(modelcount < 6)
	GetWidget('removeMultiModel'):SetEnabled(modelcount > 0)
end

function HoN_Store:ModelPanelEditorSelectMultiModel(index)	

	
	if HoN_Store.EditorPanel_CurrentModelIndex ~= index then
		HoN_Store:SetPanelSettings()
	end

	local w, s, t = GetEditingHeroPair()
	local hero, avatar = ParseHeroAvatar(s)
	if (index > 0) then
		avatar = avatar..'_'..index
	end

	local data = GetHeroPreviewDataFromDB(hero, avatar, 'large')

	for i=0, MODEL_PANEL_EFFECT_CHANNEL_MAX do
		w:SetMultiEffect('', i)
	end

	w:SetModel(data.modelPath)

	if data.usingEffect then
		w:SetEffect(data.effectPath)
	else
		w:SetEffect('')
	end

	EditData[w].effectPath = data.effectPath

	local scale = data.modelScale
	local pos = data.modelPosition
	local angle = data.modelAngle
	local ambient = data.ambient
	local sunHeight = data.sunHeight		
	local sunAngle = data.sunAngle
	local sunColor = data.sunColor
	local pedestalScale = data.pedestalScale

	w:SetModelScale(scale)
	w:SetModelPos(pos.x, pos.y, pos.z)
	w:SetModelAngles(angle.x, angle.y, angle.z)

	w:SetAmbientColor(ambient.x, ambient.y, ambient.z)
	w:SetSunPosition(sunHeight, sunAngle)
	w:SetSunColor(sunColor.x, sunColor.y, sunColor.z)

	w:SetModelScale(0, pedestalScale)
	w:SetMultiEffect(data.setEffectPath, MODEL_PANEL_EFFECT_CHANNEL_MAX)

	HoN_Store.EditorPanel_CurrentModelIndex = index
	HoN_Store:ModelPanelEditorSetEffectSettingType('normal')
	HoN_Store:ModelPanelUpdateMultiEffects()
	UpdateEditorInfo()
	
end

function HoN_Store:ModelPanelEditorAddMultiModel()
	local currentNum = HoN_Store:ModelPanelEditorGetCurrentSubModelNum()
	if currentNum >= 6 then return end
	HoN_Store:ModelPanelEditorSelectMultiModel(currentNum+1)	

	HoN_Store:SetPanelSettings()
	HoN_Store:ModelPanelEditorUpdateMultiModelInfo()
end


function HoN_Store:ModelPanelEditorRemoveMultiModel()
	local currentNum = HoN_Store:ModelPanelEditorGetCurrentSubModelNum()
	if currentNum <= 0 then return end

	if HoN_Store.EditorPanel_CurrentModelIndex == currentNum then
		HoN_Store:ModelPanelEditorSelectMultiModel(currentNum-1)

	end

	local w, m, avatarType = GetEditingHeroPair()
	local hero, avatar = ParseHeroAvatar(m)

	if hero and avatar then

		RemoveHeroPreviewData(hero, avatar, avatarType, currentNum)

	end
	HoN_Store:ModelPanelEditorUpdateMultiModelInfo()
end

function HoN_Store:refreshMultiEfxCount()
	
	GetWidget('addMultiEfx'):SetEnabled(HoN_Store.multiEfxCount~=9)
	GetWidget('removeMultiEfx'):SetEnabled(HoN_Store.multiEfxCount~=1)
end
function HoN_Store:ModelPanelEditorOnChange()
	-- Not for retail
end

local GameResourceDir = nil
local function EnsureGameResourceDirInited()
	if not GameResourceDir then
		local obj = io.popen('cd')
		GameResourceDir= obj:read('*all'):sub(1,-2)..'\\game'
		obj:close()
	end
end
function HoN_Store:OpenHeroFolder(path)
	-- Not for retail
end

function FileDropNotify(self, ext, path)
	-- Not for retail
end
interface:RegisterWatch('FileDropNotifyTrigger', FileDropNotify)

function HoN_Store:ToggleHeroesPanelEditor()
	-- Not for retail
end


local TitleDescColorSetting = {
	vault = {
		name 		= '#b8d7ca',
		nameOutline = '#0b1a1a',
		desc 		= '#778c83',
		descOutline = '#0b1a1a',
	},
	store = {
		name 		= '#d7b6a2',
		nameOutline = '#241410',
		desc 		= '#a4806f',
		descOutline = '#241410',
	},
}

local function SetNameDescColors(title, ...)
	local t = HoN_Store:IsVaultOpened() and TitleDescColorSetting.vault or TitleDescColorSetting.store
	title:SetColor(t.name)
	title:SetOutlineColor(t.nameOutline)
	for i, v in ipairs(arg) do
		v:SetColor(t.desc)
		v:SetOutlineColor(t.descOutline)
	end
end

function HoN_Store:ShowToolTip(title, desc, dotranslate)
	local parent = GetWidget('store2_tooltip_floater')
	if dotranslate then
		title = Translate(title)
		desc = Translate(desc)
	end
	local t = parent:GetWidget('store2_tooltip_floater_title')
	t:SetText(title)
	local d = parent:GetWidget('store2_tooltip_floater_desc')
	d:SetText(desc)
	SetNameDescColors(t, d)

	local frame = parent:GetChildren()[1]
	if HoN_Store:IsVaultOpened() then
		frame:SetRenderMode('colorscale1')
		frame:SetColor('#44DDEE')
	else
		frame:SetRenderMode('normal')
		frame:SetColor('#ffffff')
	end	

	parent:SetVisible(true)
	parent:BringToFront()
end

function HoN_Store:ShowToolTip2(title, desc, desc2, desc3)
	local parent = GetWidget('store2_tooltip2_floater')

	local height = 15

	local d2 = parent:GetWidget('store2_tooltip2_floater_desc2')
	local show = desc2 ~= nil and desc2 ~= ''
	if show then
		d2:SetText(Translate(desc2))
		height = height + d2:GetHeight() + 2
	end
	d2:SetVisible(show)
	
	local w = parent:GetWidget('store2_tooltip2_floater_desc2_line')
	w:SetVisible(show)

	local d3 = parent:GetWidget('store2_tooltip2_floater_desc3')
	show = desc3 ~= nil and desc3 ~= ''
	if show then
		d3:SetText(Translate(desc3))
		height = height + d3:GetHeight() + 2
	end
	d3:SetVisible(show)

	w = parent:GetWidget('store2_tooltip2_floater_desc3_line')
	w:SetVisible(show)

	title = Translate(title)
	local t = parent:GetWidget('store2_tooltip2_floater_title')
	t:SetText(title)
	height = height + t:GetHeight()

	desc = Translate(desc)
	local d = parent:GetWidget('store2_tooltip2_floater_desc')
	d:SetText(desc)
	height = height + d:GetHeight()

	SetNameDescColors(t, d, d2, d3)

	local frame = parent:GetChildren()[1]
	if HoN_Store:IsVaultOpened() then
		frame:SetRenderMode('colorscale1')
		frame:SetColor('#44DDEE')
	else
		frame:SetRenderMode('normal')
		frame:SetColor('#ffffff')
	end	

	parent:SetVisible(true)
	parent:SetHeight(height)
	parent:BringToFront()
end

function HoN_Store:ShowEligibilityToolTipInNeed(productId, eligLists)

	local data = {}
	ParseEligiblityStrings(data, eligLists, productId)

	if data.showEligPrice then
		HoN_Store:ShowToolTip2('mstore_elig', 'mstore_elig_tip', data.eligDescStr, data.requiredProductTooltip)
		return true
	else
		return false
	end
end

function HoN_Store:HideToolTip()
	GetWidget('store2_tooltip_floater'):SetVisible(false)
	GetWidget('store2_tooltip2_floater'):SetVisible(false)
end

-- page should start from 1
local indexPrevButtonId = 'prev'
local indexNextButtonId = 'next'

local function IsEligibilityLocked(productId, eligTable)
	eligTable = eligTable or HoN_Store.productEligibility
	return eligTable[productId] and not AtoB(eligTable[productId].eligible) or false
end

local function IsNeedTauntLockedForIdCode(productId, productCode)
	local _microStore_tauntUnlocked = GetCvarBool('_microStore_tauntUnlocked')
	return string.sub(productCode, 0, 2) == 't.' and not _microStore_tauntUnlocked and productId ~= '91'
end

local function IsNeedTauntLocked(info, idx)
	local productId = info.productIDs[idx]
	local productCode = info.productCodes[idx]
	return IsNeedTauntLockedForIdCode(productId, productCode)
end

-- in old store, there's a magnifier to show bundle content, but in new store this is not implemented
local function IsBundle(info, idx)
	return info.bundle[idx] == '1'
end

local function IsOwned(info, idx)
	return info.owned[idx] == '1'
end

local function IsPurchasable(info, idx)
	return info.purchasable[idx] == '1'
end

local function AppendIntArray(a, first, last)
	for i = first, last do
		table.insert(a, i)
	end
end

local function CreateIntArray(first, last)
	local a = {}
	AppendIntArray(a, first, last)
	return a
end

local function GetIndexButtonLabel(labelName)
	local label = UIManager.GetInterface('main'):GetWidget(labelName)
	if not label then
		Echo('error: nil index button label for name '..labelName)
	end
	return label
end

local function OverridePricesCvars(id, gold, silver)
	Set('microStorePrice'..id, gold)
	Set('microStoreSilverPrice'..id, silver)
	Set('microStoreTempCDGold', gold)
	Set('microStoreTempCDSilver', silver)
end

local function ShowPage(cat, newPage)

	local tb = HoN_Store.paging[cat]

	local currentPage = tb.currentPage
	local handler = tb.handler

	TestEcho('===================================  currentPage='..currentPage
		..', newPage='..tostring(newPage)
		..' ===================================')

	if handler then
		handler(newPage)
	end
end

function HoN_Store:GetIndexes(currentPage, maxPage, maxBtCnt, showBothSp)

	local sp = '...'

	if maxPage <= maxBtCnt then
		return CreateIntArray(1, maxPage), -1
	end

	if showBothSp == nil then
		showBothSp = maxBtCnt > 6
	end

	local headIndexCount = 1	-- index count before first ...
	local tailIndexCount = 1	-- index count after last ...
	local middleIndexCount = maxBtCnt - 1 - tailIndexCount
	if showBothSp then
		middleIndexCount = middleIndexCount - 1 - headIndexCount
	end
	local middleLeftIndexCount = math.floor(middleIndexCount / 2)
	local middleRightIndexCount = middleIndexCount - middleLeftIndexCount - 1
	local rightIndexCount = middleRightIndexCount + 1 + tailIndexCount
	local leftIndexCount = maxBtCnt - 1 - rightIndexCount

	local leftPageCount = currentPage - 1
	local rightPageCount = maxPage - currentPage

	local showLeftSp = showBothSp and leftPageCount > leftIndexCount
	local showRightSp = rightPageCount > rightIndexCount

	local a = {}

	if showLeftSp then
		AppendIntArray(a, 1, headIndexCount)
		table.insert(a, sp)
	end
	if showRightSp then
		local leftStart = currentPage - leftIndexCount + #a
		local startIndex = math.max(1, leftStart)
		local endIndex = startIndex + middleIndexCount - 1
		if showBothSp and not showLeftSp then
			endIndex = endIndex + headIndexCount + 1
		end
		AppendIntArray(a, startIndex, endIndex)
		table.insert(a, sp)
		AppendIntArray(a, maxPage - tailIndexCount + 1, maxPage)
	else
		local btCnt = middleIndexCount + 1 + tailIndexCount
		local startIndex = maxPage - btCnt + 1
		AppendIntArray(a, startIndex, maxPage)
	end

	local spJumpPages = -1
	if showLeftSp and showRightSp then
		spJumpPages = middleIndexCount
	end

	return a, spJumpPages, middleIndexCount
end

function UpdatePaging(cat, currentPage, itemCount, maxBtCnt)

	local sp = '...'

	local function GetIndexButtonName(cat, index)
		return 'store2_page_index_'..cat..'_'..index
	end

	local function GetArrowButtonName(cat, isLeft)
		return 'store2_paging_arrow_'..cat..'_'..((isLeft and indexPrevButtonId) or indexNextButtonId)
	end

	local function GetPaggingPanel(cat)
		return GetWidget('store2_paging_'..cat)
	end

	local function ReCreatePaggingButtons(rootPanel, cat, indexCount)
		rootPanel:ClearChildren()
		local arrowTemplateName = 'store2_paging_arrow'
		local indexTemplateName = 'store2_paging_index'

		local tb = HoN_Store.paging[cat]

		rootPanel:Instantiate(arrowTemplateName, 'cat', cat, 'id', indexPrevButtonId)
		for i = 1, indexCount do
			rootPanel:Instantiate(indexTemplateName, 'cat', cat, 'id', i)
		end
		rootPanel:Instantiate(arrowTemplateName, 'cat', cat, 'id', indexNextButtonId, 'hflip', 'true')
	end

	local function SetIndexButtonStateLabels(cat, idx, indexes, currentPage, spJumpPages, middleIndexCount)

		local str	= indexes[idx]
		local isSp 	= str == sp
		local current = str == currentPage
		local enabled = not current --and not isSp

		local indexWidth = '20i'
		local indexNumber = tonumber(str)
		if indexNumber then
			if indexNumber > 99
				then indexWidth = '36i'
			elseif indexNumber > 9 then
				indexWidth = '28i'
			else
				indexWidth = '28i'
			end
		end

		local buttonName = 'store2_page_index_'..cat..'_'..idx
		local button = GetWidget(buttonName)
		button:SetEnabled(enabled)
		button:SetWidth(indexWidth)

		local states = {'up','over','down','disabled'}
		button:SetLabel(str)

		if not enabled then
			local refState = (current and 'over') or 'up'
			local labelName = buttonName..'_'..refState
			local label = GetIndexButtonLabel(labelName)
			local x, y, z, w = label:GetColor()
			labelName = buttonName..'_disabled'
			label = GetIndexButtonLabel(labelName)
			label:SetColor(x, y, z, w)
		end

		if enabled then
			button:SetCallback('onclick', function(...)
					if isSp then
						local left = idx < (#indexes / 2)
						local newPage
						if left then
							if spJumpPages < 0 then
								newPage = indexes[idx + 1]
								newPage = newPage - math.floor(middleIndexCount / 2) - 1
							else
								newPage = currentPage - spJumpPages
							end
							local leftPage = indexes[idx - 1]
							if newPage <= leftPage then
								newPage = leftPage + 1
							end
						else
							if spJumpPages < 0 then
								newPage = indexes[idx - 1]
								newPage = newPage + math.floor(middleIndexCount / 2) + 1
							else
								newPage = currentPage + spJumpPages
							end
							local rightPage = indexes[idx + 1]
							if newPage >= rightPage then
								newPage = rightPage - 1
							end
						end
						ShowPage(cat, newPage)
					else
						HoN_Store:OnClickPagingButton(button, cat)
					end
				end)
		end
	end

	local rootPanel = GetPaggingPanel(cat)
	local config = HoN_Store.paging[cat]

	if not rootPanel then Echo('^rUpdatePaging failed to find root panel for cat: '..cat) return end
	if not config then Echo('^rUpdatePaging failed to find config for cat: '..cat) return end

	local maxPage = HoN_Store.paging.MaxPage(cat, itemCount)

	if maxPage <= 1 then
		rootPanel:SetVisible(false)
	else
		rootPanel:SetVisible(true)

		if not config.noIndexes and maxBtCnt and maxBtCnt > 0 then
			local indexes, spJumpPages, middleIndexCount
			indexes, spJumpPages, middleIndexCount = HoN_Store:GetIndexes(currentPage, maxPage, maxBtCnt)

			local indexCount = #indexes
			local currentButtons = rootPanel:GetChildren()

			if #currentButtons ~= indexCount + 2 then
				ReCreatePaggingButtons(rootPanel, cat, indexCount)
				currentButtons = rootPanel:GetChildren()
			end

			for i, v in ipairs(indexes) do
				SetIndexButtonStateLabels(cat, i, indexes, currentPage, spJumpPages, middleIndexCount)
			end

			local width = 0
			for i, v in ipairs(currentButtons) do
				width = width + v:GetWidth()
			end
			rootPanel:SetWidth(width)
		end

		if not config.noArrows then
			local leftArrow = rootPanel:GetWidget(GetArrowButtonName(cat, true))
			local leftArrowEnabled = currentPage > 1
			leftArrow:SetEnabled(leftArrowEnabled)

			local rightArrow = rootPanel:GetWidget(GetArrowButtonName(cat, false))
			local rightArrowEnabled = currentPage < maxPage
			rightArrow:SetEnabled(rightArrowEnabled)
		end
	end
	config.currentPage = currentPage

	HoN_Store:HideDebugPanel()

	PlaySound('/ui/fe2/store/sounds/shelf_item_fadein.wav')
end

function HoN_Store:OnClickPagingButton(bt, cat)

	if not Store2PendingJobsIsEmpty() then return end

	local function CalcNewPage(currentPage, btId)
		if btId == indexPrevButtonId then
			currentPage = math.max(1, currentPage - 1)
		elseif btId == indexNextButtonId then
			currentPage = currentPage + 1
		else
			currentPage = tonumber(btId)
		end
		return currentPage
	end

	local function GetButtonId(bt)
		local name = bt:GetName()
		if name:match('.+_'..indexPrevButtonId) then return indexPrevButtonId end
		if name:match('.+_'..indexNextButtonId) then return indexNextButtonId end
		local labelName = name..'_up'
		local w = GetIndexButtonLabel(labelName)
		if w then
			return w:GetText()
		end
		return nil
	end

	local btId = GetButtonId(bt)
	if not btId then
		Echo("error: nil buttonId")
		return
	end

	local tb = HoN_Store.paging[cat]
	if not tb then
		Echo('error: nil info for cat '..cat)
		return
	end

	local currentPage = tb.currentPage
	local newPage = CalcNewPage(currentPage, btId)

	ShowPage(cat, newPage)
end

function HoN_Store:OnClickPagingDotButton(cat, page)

	if not Store2PendingJobsIsEmpty() then return end

	local tb = HoN_Store.paging[cat]
	if not tb then
		Echo('error: nil info for cat '..cat)
		return
	end

	local handler = tb.handler
	if handler then
		handler(page)
	end
end

function HoN_Store:InitializePaging2Dots(cat, currentPage, totalPages)
	local parent = GetWidget('store2_paging_dot_panel'..cat, nil, true)
	if parent == nil then return end

	Store2TrashCanAddChildren(parent)
	for page = 1,totalPages do
		local templateName = currentPage == page and 'store2_paging_dot_yellow' or 'store2_paging_dot_black'
		parent:Instantiate(templateName, 'cat', cat, 'page', page)
	end
end

-- Nav History
local Store2NavHistoryMaxCount = 10
local Store2NavHistory =
{
	['first'] = 0,
	['last'] = -1,
}

function NavHistoryIsEmpty()
	return Store2NavHistory['last'] < Store2NavHistory['first']
end

function HoN_Store:NavHsitoryClear()
	GetWidget("store2_top_backbutton_shop"):SetEnabled(0)
	GetWidget("store2_top_backbutton_vault"):SetEnabled(0)

	if NavHistoryIsEmpty() then return end

	for i = Store2NavHistory.first, Store2NavHistory.last do
		Store2NavHistory[i] = nil
	end

	Store2NavHistory.first = 0
	Store2NavHistory.last = -1
end

function HoN_Store:NavHistoryPush(data)

	local index = Store2NavHistory['last'] + 1
	Store2NavHistory[index] = data
	Store2NavHistory.last = index

	if Store2NavHistory.last - Store2NavHistory.first + 1 > Store2NavHistoryMaxCount then
		Store2NavHistory[Store2NavHistory.first] = nil
		Store2NavHistory.first = Store2NavHistory.first + 1
	end

	TestEcho("^yNavPush: "..data.name)

	GetWidget("store2_top_backbutton_shop"):SetEnabled(1)
	GetWidget("store2_top_backbutton_vault"):SetEnabled(1)
end

function HoN_Store:NavHistoryPop()
	if NavHistoryIsEmpty() then return nil end

	local last = Store2NavHistory['last']
	local data = Store2NavHistory[last]
	Store2NavHistory[last] = nil
	Store2NavHistory['last'] = Store2NavHistory['last'] - 1;

	if NavHistoryIsEmpty() then
		GetWidget("store2_top_backbutton_shop"):SetEnabled(0)
		GetWidget("store2_top_backbutton_vault"):SetEnabled(0)
	end

	TestEcho("^yNavPop: "..data.name)

	return data
end

function HoN_Store:NavHistoryPeek()
	if NavHistoryIsEmpty() then return nil end

	local last = Store2NavHistory['last']
	local data = Store2NavHistory[last]

	return data
end

function HoN_Store:NavigateBack()
	local data = HoN_Store:NavHistoryPop()
	if data == nil then return end

	Store2NavBackInProgress = true
	data.func(data.func)
	Store2NavBackInProgress = false
	Store2NavBackLastTime	= tostring(GetTime())
end

function HoN_Store:NavHistoryPush2(name, func, force)
	if not force and Store2NavBackInProgress then return end
	local data = {
		name = name,
		func = func,
	}
	HoN_Store:NavHistoryPush(data)
end

Store2ModelContextManager = _G['Store2ModelContextManager'] or
{
	contextPrefix			= 'model_',
	generationPrefix		= '_g',
	pagePrefix				= '_p',
	maxPageCount			= 20,
	preferedPageCount		= 10,
	currentPageCount		= 0,
	RecyclePages			= {},
	currentContexts			= {'model'},
}

if GetCvarString('host_os') ~= "windows" then
	Store2ModelContextManager.maxPageCount = 1
	Store2ModelContextManager.preferedPageCount = 1
end

Store2ModelContextManager.Categories = Store2ModelContextManager.Categories or
{
	heroList = {
		contextName			= 'heroList',
		minPageCount 		= 1,
		preferedPageCount 	= 5,
		generation			= 0,
		currentPages		= {},
		currentPage			= 0,
	},
	avatarList = {
		contextName			= 'avatarList',
		minPageCount 		= 1,
		preferedPageCount 	= 5,
		generation			= 0,
		currentPages		= {},
		currentPage			= 0,
	},
	heroAvatars = {
		contextName			= 'heroAvatars',
		minPageCount 		= 1,
		preferedPageCount 	= 3,
		generation			= 0,
		currentPages		= {},
		currentPage			= 0,
	},
	bundleDetail = {
		contextName			= 'bundleDetail',
		minPageCount 		= 1,
		preferedPageCount 	= 2,
		generation			= 0,
		currentPages		= {},
		currentPage			= 0,
	},
	detailLargeModel = {
		contextName			= 'detailLargeModel',
		minPageCount 		= 1,
		preferedPageCount 	= 1,
		generation			= 0,
		currentPages		= {},
		currentPage			= 0,
	},
}

Store2ModelContextManager.GetCatContextName = function(catTable, page)
	local sm = Store2ModelContextManager
	local contextName = sm.contextPrefix..catTable.contextName
					  ..sm.generationPrefix..catTable.generation
					  ..sm.pagePrefix..page
	return contextName
end

Store2ModelContextManager.GetContextIndex = function(name)
	local sm = Store2ModelContextManager
	for i, v in ipairs(sm.currentContexts) do
		if v == name then
			return i
		end
	end
end

Store2ModelContextManager.FreePendingContexts = function()
	local sm = Store2ModelContextManager

	local function RemoveContext(contextName)
		local index = sm.GetContextIndex(contextName)
		table.remove(sm.currentContexts, index)
		sm.currentPageCount = sm.currentPageCount - 1
	end

	local function FreeContexts(contextsToRemove)
		local count = #contextsToRemove
		for i, v in ipairs(contextsToRemove) do
			local unregisterOrphans = i == count
			DeleteResourceContext(v, unregisterOrphans)
			RemoveContext(v)
		end
	end

	FreeContexts(sm.RecyclePages)
	sm.RecyclePages = {}
end

Store2ModelContextManager.RecycleCatFirstPage = function(catTable, listToInsert)
	local sm = Store2ModelContextManager
	local pages = catTable.currentPages
	local t = table.remove(pages, 1)
	local contextName = sm.GetCatContextName(catTable, t.page)
	table.insert(listToInsert, contextName)
end

Store2ModelContextManager.RecyclePagesInCase = function(currentCat)
	local sm = Store2ModelContextManager

	if sm.currentPageCount <= sm.maxPageCount then return end

	local function PrepareRecycleList(list, count, currentCat)
		-- see if there are already enough recycling pages
		local countRemaining = count - #list
		if countRemaining <= 0 then return end

		-- check categories' preferedPageCount
		for i, v in pairs(sm.Categories) do
			if i ~= currentCat then
				while #v.currentPages > v.preferedPageCount do
					sm.RecycleCatFirstPage(v, list)
					countRemaining = countRemaining - 1
				end
			end
		end
		if countRemaining <= 0 then return end

		local currentCatTable = sm.Categories[currentCat]
		local currentCatPages = currentCatTable.currentPages
		while #currentCatPages > currentCatTable.preferedPageCount do
			sm.RecycleCatFirstPage(currentCatTable, list)
			countRemaining = countRemaining - 1
			if countRemaining == 0 then break end
		end
		if countRemaining <= 0 then return end

		-- check categories' minPageCount
		local catWithFreePages = {}
		for i, v in pairs(sm.Categories) do
			local freePageCount = #v.currentPages - v.minPageCount
			if i ~= currentCat and freePageCount > 0 then
				table.insert(catWithFreePages, {cat = v, count = freePageCount})
			end
		end

		local function GetEntryWithMaxFreePage(catWithFreePages)
			local entry = nil
			local entryIdnex = nil
			local maxCount = 0
			for i, v in pairs(catWithFreePages) do
				if v.count > maxCount then
					maxCount = v.count
					entry = v
					entryIdnex = i
				end
			end
			return entry, entryIdnex
		end

		while countRemaining > 0 and #catWithFreePages > 0 do
			local entry, entryIdnex = GetEntryWithMaxFreePage(catWithFreePages)
			local catTable = entry.cat
			sm.RecycleCatFirstPage(catTable, list)
			countRemaining = countRemaining - 1
			entry.count = entry.count - 1
			if entry.count == 0 then
				table.remove(catWithFreePages, entryIdnex)
			end
		end

		while countRemaining > 0 and #currentCatPages > currentCatTable.minPageCount do
			sm.RecycleCatFirstPage(currentCatTable, list)
			countRemaining = countRemaining - 1
		end

		if countRemaining > 0 then
			Echo('^r RecyclePagesInCase() error: countRemaining='..countRemaining)
		end
	end

	local countToRemove = sm.currentPageCount - sm.preferedPageCount
	PrepareRecycleList(sm.RecyclePages, countToRemove, currentCat)

	sm.FreePendingContexts()
end

Store2ModelContextManager.PrepareNewGeneration = function(cat)
	local sm = Store2ModelContextManager
	local catTable = sm.Categories[cat]

	for i, v in ipairs(catTable.currentPages) do
		local pageName = sm.GetCatContextName(catTable, v.page)
		table.insert(sm.RecyclePages, pageName)
	end
	catTable.currentPages = {}

	catTable.generation = catTable.generation + 1
end

Store2ModelContextManager.PrepareNewPage = function(cat, page)
	local sm = Store2ModelContextManager
	local catTable = Store2ModelContextManager.Categories[cat]
	local pages = catTable.currentPages

	local function GetPageIndex(pages, page)
		local index = -1
		for i, v in ipairs(pages) do
			if v.page == page then
				index=  i
				break
			end
		end
		return index
	end

	local function InsertNewPage(pages, page)
		table.insert(pages, { page = page})
	end

	local function MoveToEnd(pages, index)
		local t = table.remove(pages, index)
		table.insert(pages, t)
	end

	local function InsertNewContext(newContextName)
		table.insert(sm.currentContexts, newContextName)
		sm.currentPageCount = sm.currentPageCount + 1
	end

	local index = GetPageIndex(pages, page)
	if index <= 0 then
		local newContextName = sm.GetCatContextName(catTable, page)
		InsertNewPage(pages, page)
		InsertNewContext(newContextName)
	else
		MoveToEnd(pages, index)
	end

	catTable.currentPage = page
end

Store2ModelContextManager.SetModelContext = function(cat, modelPanel)
	local sm = Store2ModelContextManager
	local catTable = sm.Categories[cat]
	local contextName = sm.GetCatContextName(catTable, catTable.currentPage)
	modelPanel:SetResourceContext(contextName)
end

Store2ModelContextManager.ShrinkToMinPage = function(cat, release)
	local sm = Store2ModelContextManager
	local catTable = sm.Categories[cat]

	while #catTable.currentPages > catTable.minPageCount do
		sm.RecycleCatFirstPage(catTable, sm.RecyclePages)
	end

	if release then
		sm.FreePendingContexts()
	end
end

Store2ModelContextManager.ShrinkAllToMinPage = function()
	local sm = Store2ModelContextManager
	for i, v in pairs(sm.Categories) do
		sm.ShrinkToMinPage(v.contextName, false)
	end
	sm.FreePendingContexts()
end

Store2ModelContextManager.ReleaseAll = function()
	local sm = Store2ModelContextManager
	for i, v in pairs(sm.Categories) do
		local pages = v.currentPages
		for j, p in pairs(pages) do
			local contextName = sm.GetCatContextName(v, p.page)
			table.insert(sm.RecyclePages, contextName)
		end
		v.currentPage	= 0
		v.currentPages 	= {}
	end
	sm.FreePendingContexts()
end

local store_options = 
{
	['_rimlighting'] = true,
	['_shaderQuality'] = 0,
	['_shadowQuality'] = 1,
	['_waterQuality'] = 0,
	['_display_skybox'] = true,
	['_foliage'] = true,
	['_dynamiclights'] = true,
	['_reflections'] = true,
	['_refraction'] = true,
	['_postprocessing'] = true,
	['_fakeSpecColor'] = "0.6 0.6 0.6"
}


local function Store2OptionsApplyWithoutShaderReload()
	local vid_shader_Precache = GetCvarBool('vid_shader_Precache')
	if GetCvarBool('host_store2buildshader') == false then
		Set('vid_shader_Precache', false)
	end
	interface:UICmd('OptionsApply()')

	if GetCvarBool('host_store2buildshader') == false then
		Set('vid_shader_Precache', vid_shader_Precache)
	end
end

function HoN_Store:SetupStoreVidOptions()
	TestEcho('^:^r ....... SetupStoreVidOptions .......')

	HoN_Store.bIn8BitMode = GetCvarBool('vid_render3Das2D')
	if HoN_Store.bIn8BitMode then
		Exec('non8bit')
	end

	if GetCvarBool('host_store2unifyshader') == false then
		return
	end

	interface:UICmd('OptionsOpen()')

	-- do force to use good shaders
	store_options._rimlighting = GetCvarBool('options_rimlighting')
	store_options._shaderQuality = GetCvarInt('options_shaderQuality')
	store_options._shadowQuality = GetCvarInt('options_shadowQuality')
	store_options._waterQuality = GetCvarInt('options_waterQuality')
	store_options._display_skybox = GetCvarBool('options_skybox')
	store_options._foliage = GetCvarBool('options_foliage')
	store_options._dynamiclights = GetCvarBool('options_dynamiclights')
	store_options._reflections = GetCvarBool('options_reflections')
	store_options._refraction = GetCvarBool('options_refraction')
	store_options._postprocessing = GetCvarBool('options_postprocessing')
	store_options._fakeSpecColor = GetCvarString('scene_fakeSpecColor')

	vid_shader_Precache = GetCvarBool('vid_shader_Precache')

	Set('options_rimlighting', true)
	Set('options_shaderQuality', 0)
	Set('options_shadowQuality', 1)
	Set('options_waterQuality', 0)
	Set('options_skybox', true)
	Set('options_foliage', true)
	Set('options_dynamiclights', true)
	Set('options_reflections', true)
	Set('options_refraction', true)
	Set('options_postprocessing', true)
	Set('scene_fakeSpecColor', "0.6 0.6 0.6")

	Store2OptionsApplyWithoutShaderReload()

	Trigger('store2OptionsApply')
end

function HoN_Store:RestoreVidOptions()
	TestEcho('^:^r ....... RestoreVidOptions .......')

	if HoN_Store.bIn8BitMode then
		Exec('8bit')
	end

	if GetCvarBool('host_store2unifyshader') == false then
		return
	end

	interface:UICmd('OptionsOpen()')

	Set('options_rimlighting', store_options._rimlighting)
	Set('options_shaderQuality', store_options._shaderQuality)
	Set('options_shadowQuality', store_options._shadowQuality)
	Set('options_waterQuality', store_options._waterQuality)
	Set('options_skybox', store_options._display_skybox)
	Set('options_foliage', store_options._foliage)
	Set('options_dynamiclights', store_options._dynamiclights)
	Set('options_reflections', store_options._reflections)
	Set('options_refraction', store_options._refraction)
	Set('options_postprocessing', store_options._postprocessing)
	Set('scene_fakeSpecColor', store_options._fakeSpecColor)

	Store2OptionsApplyWithoutShaderReload()

	GetWidget('store2_vidOptionsHelper'):Sleep(100, function()
		Set('temp_gfxslider_int', GetCvarInt('ui_options_gfxslider'))
	end)

	Trigger('store2OptionsApply')
end

local vaultEverShown = false

function HoN_Store:CloseStoreCommand()
	Set('_mainmenu_currentpanel', 'store_container2')
	GetWidget('MainMenuPanelSwitcher'):DoEvent()
	Trigger('store2TabMenuHide', 'all')
	Trigger('Store2ShopClosing')
	HoN_Store:Store2DailySpecialClear()
	HoN_Store:Store2PagingStateClear()

	SetSFXMute(HoN_Store.sfxSoundMuted)
end

function HoN_Store:OnOpenStore()
	HoN_Store:Store2Loading(1, "OpenStore")
	Trigger('Store2ShopOpening')
	HoN_Store:SetupStoreVidOptions()
	HoN_Store:Store2OpenShop()
	HoN_Store:NavHsitoryClear()
	local hasInitCategory = HoN_Store:OnEnterInitRequest()

	local openSpecials = true
	if hasInitCategory then 
		openSpecials = false
	elseif HoN_Store.OpenStoreTargetPageInfo ~= nil then
		openSpecials = false
		local info = HoN_Store.OpenStoreTargetPageInfo
		HoN_Store.OpenStoreTargetPageInfo = nil

		if info.catId == 58 then		-- EAP		heroName
			HoN_Store:SwitchToEaPage()
		elseif info.catId == 71 then	-- Hero		heroName
			HoN_Store.HeroDetailPage.HeroCodeToInit = info.productName..'.Hero'
			HoN_Store:NavToHeroAvatarDetailPage(false, true, function()
				if HoN_Store:FreeHeros() then
					HoN_Store:SwitchToAvatarListPage(true)
				else
					HoN_Store:SwitchToHeroListPage(true)
				end
			end)
		elseif info.catId == 2 then		-- Alt Avatar 	heroName.altCode
			HoN_Store.HeroDetailPage.AvatarCodeToInit = 'aa.'..info.productName
			HoN_Store:NavToHeroAvatarDetailPage(true, true, function()
				HoN_Store:SwitchToAvatarListPage(true)
			end)
		else
			TestEcho('^rOpenStoreTargetPageInfo unimplemented catId: '..tostring(info.catId))
			openSpecials = true
		end
	end

	if openSpecials then
		GetWidget("store_container2"):Sleep(700, function()
			Store2Tab('specials')
		end)
	end

	LoadEntityDefinition('/shared/automated_courier/pet_courier/courier.entity')

	Store2NavBackInProgress = false

	HoN_Store.sfxSoundMuted = GetSFXMuted()
	SetSFXMute(false)
end

function HoN_Store:HideAllSpecialBoothUI()
	GetWidget('store2_specials_booth_1'):SetVisible(0)
	GetWidget('store2_specials_booth_2'):SetVisible(0)
	GetWidget('store2_specials_booth_3'):SetVisible(0)
	GetWidget('store2_specials_booth_4'):SetVisible(0)
	GetWidget('store2_specials_booth_5'):SetVisible(0)
	GetWidget('store2_specials_booth_vanity'):SetVisible(0)
end

function HoN_Store:OnCloseStore()
	HoN_Store:Store2Loading(1, "CloseStore")
	HoN_Store:RestoreVidOptions()

	Store2ModelContextManager.ReleaseAll()
	HoN_Store:Store2Loading(0, "CloseStoreDone")

	HoN_Store.ProductHeroes.PageEverShown = false
	HoN_Store.ProductAvatars.PageEverShown = false

	groupcall('storePopupWindows', 'DoEvent(2);');

	HoN_Store:Store2DailySpecialClear()
	HoN_Store:Store2PagingStateClear()

	HoN_Store:ClearEapData()
	HoN_Store:ClearFeaturedData()
	HoN_Store:ClearHeroesData()
	HoN_Store:ClearAvatarData()

	vaultEverShown = false

	HoN_Store.Store2VaultData = {}
	Trigger('store2VanityShow', '', '')
	
	GetWidget('store2_detail_heroAvatars'):SetVisible(false)
end

local function SetRequestStatusWatch(triggerName)
	TestEcho('^g trigger name: '..triggerName)
	Trigger('MicroStoreSetProcTrig', triggerName)
end

-- hero, avatar common
function HoN_Store:SubmitRequestForm(catId, displayAll, showNonPurchasable, page)

	local _microStore_Category = catId
	local _microStore_RequestCode = 1
	local _lastRequestHostTime = GetTime()
	local _microStoreRequestAllItems = displayAll and 'true' or 'false'
	local _microStoreShowNonPurchasable = showNonPurchasable and 'true' or 'false'
	local _microStore_currentPage = page or 1

	Set('_lastRequestHostTime', _lastRequestHostTime)
	SetRequestStatusWatch('MicroStoreStatus')

	SubmitForm('MicroStore', 'account_id', Client.GetAccountID(), 'category_id', _microStore_Category,
				'request_code', _microStore_RequestCode, 'page', _microStore_currentPage, 'cookie', Client.GetCookie(),
				'hostTime', _lastRequestHostTime, 'displayAll', _microStoreRequestAllItems, 'notPurchasable', _microStoreShowNonPurchasable)
end

function HoN_Store:GetHeroesTab()
	local allHeroes = HasAllHeroes()
	if allHeroes and HoN_Region and (not HoN_Region:GetHasEAP()) then
		return 'featured'
	elseif allHeroes then
		return 'ea'
	else
		return 'heroes'
	end
end

local function Store2TabHeroes()
	Store2Tab(HoN_Store:GetHeroesTab())
end

HoN_Store.RequestParam =
{
	eap = {
		cat					= 'eap',
		catId 				= MSTORE_CATEGORY_EAP or 58,
		displayAll 			= false,
		showNonPurchasable	= false,
		nextRequestTimes 	= 0,
	},
	featured = {
		cat					= 'featured',
		catId 				= MSTORE_CATEGORY_FEATURED or 68,
		displayAll 			= false,
		showNonPurchasable	= false,
		nextRequestTimes 	= 0,
	},
	heroes = {
		cat					= 'heroes',
		catId 				= MSTORE_CATEGORY_HEROES or 71,
		displayAll 			= true,
		showNonPurchasable	= false,
		nextRequestTimes 	= 0,
	},
	avatars = {
		cat					= 'avatars',
		catId 				= MSTORE_CATEGORY_HERO_AVATARS or 2,
		displayAll 			= true,
		showNonPurchasable	= true,
		nextRequestTimes 	= 0,
	},
	buyBundle = {
		cat					= 'buyBundle',
		catId 				= 7,
	},
}

function HoN_Store:FreeHeros()
	return not HoN_Region:GetHasEAP()
end

function HoN_Store:HasGca()
	return true
end

HoN_Store.RequestResultHandlers = {
	eap = {},
	featured = {},
	heroes = {},
	avatars = {},
	buyBundle = {},
}

function HoN_Store:RequestStoreDataInNeed(cat, callback, page, forceRequst)

	local sendRequest = true
	if cat == 'heroes' and HoN_Store:FreeHeros() then
		sendRequest = false
	end

	local alwaysRequest = true
	forceRequst = forceRequst or alwaysRequest

	local catTable	= HoN_Store.RequestParam[cat]
	local nextTime 	= catTable.nextRequestTimes
	local time 		= GetTime()		-- update every frame

	if not forceRequst and nextTime and time < nextTime then
		sendRequest = false
	end

	if sendRequest then
		page = page or 1
		local handlerRegistry = HoN_Store.RequestResultHandlers[cat]
		handlerRegistry.time = time
		handlerRegistry.callback = callback
		HoN_Store:SubmitRequestForm(catTable.catId, catTable.displayAll, catTable.showNonPurchasable, page)
	else
		TestEcho("[warn] RequestStoreDataInNeed() lazy no submit for cat="..cat)
		callback(true, false)
	end
end

function HoN_Store:RequestHeroAvatarData(cat, callback)
	HoN_Store:RequestStoreDataInNeed(cat, callback, 1, false)
end

local function HasHeroesData()
	return HoN_Store:FreeHeros() or HoN_Store.ProductHeroes.Count > 0
end

local function HasAvatarsData()
	return HoN_Store.ProductAvatars.Count > 0
end

function HoN_Store:RequestHeroAvatarDataOnNil(cat, callback)
	local hasData = false

	if cat == 'avatars' then
		hasData = HasAvatarsData()
	elseif cat == 'heroes' then
		hasData = HasHeroesData()
	end

	if hasData then
		callback(true, false)
	else
		HoN_Store:RequestHeroAvatarData(cat, callback)
	end
end

function HoN_Store:EnsureHeroAndAvatarData(callback)

	local sendBothFormAtSameTime = false

	local tmp = {}
	tmp.CreateWaitFunc = function(cat)
		return function(success, newData, ...)
			local heroHandlerRegistry 	= HoN_Store.RequestResultHandlers.heroes
			local avatarHandlerRegistry = HoN_Store.RequestResultHandlers.avatars

			if not success then
				if sendBothFormAtSameTime then
					heroHandlerRegistry.time 		= 0
					heroHandlerRegistry.callback 	= nil
					avatarHandlerRegistry.time 		= 0
					avatarHandlerRegistry.callback 	= nil
				end
				TestEcho('^r EnsureHeroAndAvatarData() wait func recieve error')
				callback(false)
				return
			end

			if cat == 'heroes' then
				HoN_Store:InitializeHeroesList(...)
			elseif cat == 'avatars' then
				HoN_Store:InitializeAvatarList(...)
			end

			local hasHeroes  = HasHeroesData()
			local hasAvatars = HasAvatarsData()

			if hasHeroes and hasAvatars then
				callback(true)
			elseif sendBothFormAtSameTime then
				if not hasHeroes and heroHandlerRegistry.callback == nil then
					Echo('^r EnsureHeroAndAvatarData() nil hero data and callback')
				end
				if not hasAvatars and avatarHandlerRegistry.callback == nil then
					Echo('^r EnsureHeroAndAvatarData() nil avatar data and callback')
				end
			else
				local w = GetWidget('store2_'..HoN_Store:GetHeroesTab())
				w:Sleep(1, function()
					if cat == 'heroes' then
						HoN_Store:RequestHeroAvatarData('avatars', tmp.AvatarDataCallback)
					elseif cat == 'avatars' then
						HoN_Store:RequestHeroAvatarData('heroes', tmp.HeroDataCallback)
					end
				end)
			end
		end
	end
	tmp.HeroDataCallback = tmp.CreateWaitFunc('heroes')
	tmp.AvatarDataCallback = tmp.CreateWaitFunc('avatars')

	local hasHeroes  = HasHeroesData()
	local hasAvatars = HasAvatarsData()

	HoN_Store:Store2Loading(1, 'EnsureHeroAndAvatarData')
	if hasHeroes and hasAvatars then
		callback()
	elseif hasHeroes then
		HoN_Store:RequestHeroAvatarData('avatars', tmp.AvatarDataCallback)
	elseif hasAvatars then
		HoN_Store:RequestHeroAvatarData('heroes', tmp.HeroDataCallback)
	else
		if sendBothFormAtSameTime then
			HoN_Store:RequestHeroAvatarData('heroes', tmp.HeroDataCallback)
			HoN_Store:RequestHeroAvatarData('avatars', tmp.AvatarDataCallback)
		else
			HoN_Store:RequestHeroAvatarData('heroes', tmp.HeroDataCallback)
		end
	end
end

function HoN_Store:GetIndex(list, value, debug)
	if not list then return -1 end
	for i, v in ipairs(list) do
		if v == value then
			return i
		end
	end
	if debug then
		TestEcho('^r GetIndex() fail to find '..tostring(value)..' in list sized '..#list
				..', type='..type(value)..'/'..type(list[1]))
	end
	return -1
end

local function CheckCodeType(code, typeStr)
	local data = {
		isavatar	= string.match(code, '^aa%.') 			and true or false,	-- Alt Avatar
		iscolor 	= string.match(code, '^cc%.') 			and true or false,	-- Chat Color
		isicon 		= string.match(code, '^ai%.') 			and true or false,	-- Account Icon
		isannouncer	= string.match(code, '^av%.') 			and true or false,	-- Alt Announcement
		issymbol 	= string.match(code, '^cs%.') 			and true or false,	-- Chat Symbol
		istaunt 	= string.match(code, '^t%.') 			and true or false,	-- Taunt
		iscouriers	= string.match(code, '^c%.') 			and true or false,	-- Couriers
		isward 		= string.match(code, '^w%.') 			and true or false,	-- Ward
		isupgrade	= string.match(code, '^en%.') 			and true or false,	-- Enhancement
		iscreep		= string.match(code, '^cr%.') 			and true or false,	-- Creep
		ishero		= string.match(code, '^[^%.]+%.Hero$')
				   or string.match(code, '^[^%.]+%.eap$')
				   or string.match(code, '^h%.')			and true or false,	-- Hero
		istpeffect	= string.match(code, '^te%.') 			and true or false,	-- TP Effect
	}
	if typeStr == 'EAP' then
		data.iseapbundle = true
	elseif typeStr == 'Bundle' then
		data.isbundle = true
		if code and string.match(code, '%.AltBundle$') then
			data.isfeaturedbundle = true
		end
	elseif typeStr == 'Misc' then
		data.isothers = true
	else
		data.isothers  = true
		for i, v in pairs(data) do
			if v then
				data.isothers = false
				break
			end
		end		
	end

	return data
end

local function GetProductCatName(type)
	if type.iscolor then
		return 'namecolors'
	elseif type.iscouriers then
		return 'couriers'
	elseif type.iscreep then
		return 'creeps'
	elseif type.isward then
		return 'wards'
	elseif type.isavatar then
		return HoN_Store.RequestParam.avatars.catId
	elseif type.issymbol then
		return 'symbols'
	elseif type.istaunt then
		return 'taunts'
	elseif type.isannouncer then
		return 'announcers'
	elseif type.isothers then
		return 'others'
	elseif type.isicon then
		return 'accounticons'
	elseif type.ishero then
		return HoN_Store.RequestParam.heroes.catId
	elseif type.isupgrade then
		return 'upgrades'
	else
		return ''
	end
end

local DailySpecialJson = nil

function HoN_Store:HandleDailySpecialData(popupCode, prodcutIDs, productOwned)
	if HoN_Store:Store2GetCurrentPage() == 'store2_specials' and popupCode == '3' then
		-- easiest way, request again
		HoN_Store:Store2DailySpecialFormGet(true)

		if not GetCvarBool('releaseStage_stable') then
			local Ids					= split(tostring(prodcutIDs) 	or '', '|')
			local Owned 				= split(tostring(productOwned) 	or '', '|')
			local productId = GetCvarInt('_microStore_SelectedID')
			local productIndex = HoN_Store:GetIndex(Ids, tostring(productId), true)
			if Owned[productIndex] == '1' then
				for idx = 1,5 do
					local single = DailySpecialJson[idx]
					if single.product_id == tostring(productId) then
						TestEcho('update product idx: '..tostring(idx)..'  id: '..tostring(productId))
						break
					end
				end
			end
		end
	end
end

function GetDailySpecialEntry(prodcutID)
	if DailySpecialJson == nil then Echo('^r Error: nil DailySpecialJson') return end
	for i, v in pairs(DailySpecialJson) do
		if tostring(v.product_id) == prodcutID then
			return v
		end
	end
	return nil
end

function HoN_Store:HandleStoreDataForHeroAvatars(responseCode, popupCode, categoryID, errorCode, timestamp, grabBag, ...)

	if responseCode ~= tostring(STORE2_RESPONSE_CODE_BASICITEMLIST) then return end		-- hero/avatar are return in list

	local cats		= HoN_Store.RequestParam
	local catId 	= tonumber(categoryID)
	local catTable 	= nil

	for i, v in pairs(cats) do
		if v and type(v) == 'table' and v.catId == catId then
			catTable = v
			break
		end
	end

	if catTable == nil then return end

	local callback = nil

	local handlerRegistry = HoN_Store.RequestResultHandlers[catTable.cat]
	if handlerRegistry and handlerRegistry.time == tonumber(timestamp) then
		callback = handlerRegistry.callback

		handlerRegistry.callback = nil
		handlerRegistry.time = 0
	elseif tonumber(popupCode) == STORE2_POPUPCODE_BUYITEM and grabBag and AtoB(grabBag) then
		handlerRegistry = HoN_Store.RequestResultHandlers.buyBundle
		callback = handlerRegistry.default
		TestEcho('^g grabbag callback is '..tostring(callback))
	else
		callback = handlerRegistry.default
		TestEcho('^g default callback for '..tostring(catTable.cat)..' is '..tostring(callback))
	end

	if not callback then
		TestEcho('^r nil callback for cat='..tostring(catTable.cat))
	else
		local validResult = tonumber(errorCode) == 0
		callback(validResult, true, ...)

		if validResult then
			catTable.nextRequestTimes = GetTime() + GetCvarNumber('store2_request_interval')
		end
	end
end

local function SetupGoldSilverPriceLabel(parent, prefix, visible, center, cost, startX, middleX)
	local panel = parent:GetWidget(prefix..'_panel')
	if panel ~= nil then
		panel:SetVisible(visible)
		parent:GetWidget(prefix):SetText(cost or '')
		if visible then
			if center then
				startX = middleX
			end
			panel:SetX(startX..'%')
		end
	end
end

local function SetupHeroAvatarFramePrice(parent, prefix, data, forAltAvatar)

	-- price stuff
	local function GetHeroAvatarFramePriceStr(data, forAltAvatar)

		if forAltAvatar and not data.isAltAvatar then
			return data.displayName, false, false
		end

		local result = ParseSpecialPriceValue(
			data.goldCost, data.silverCost, data.purchasable,
			data.productID, data.heroEntry, data.eligLists, data.isAltAvatar)

		data.purchasable = result.purchasable	-- may be changed by Eligibility stuff

		if forAltAvatar and not data.isAltAvatar then
			result.showGoldPrice = false
			result.showSilverPrice = false
		end

		return result.priceStrKey, result.showGoldPrice, result.showSilverPrice, result
	end

	local priceStrKey, showGold, showSilver, priceInfo = GetHeroAvatarFramePriceStr(data, forAltAvatar)
	local spPriceLabel = parent:GetWidget(prefix..'price')
	local labelParent = spPriceLabel:GetParent()
	if priceStrKey == nil then
		spPriceLabel:SetVisible(false)
		labelParent:SetNoClick(true)
	else
		spPriceLabel:SetVisible(true)
		spPriceLabel:SetText(Translate(priceStrKey))
		if priceInfo and priceInfo.showEligPrice then
			labelParent:SetNoClick(false)
			labelParent:SetCallback('onmouseover', function()
					parent:DoEventN(2)
					HoN_Store:ShowToolTip2('mstore_elig', 'mstore_elig_tip', priceInfo.eligDescStr, priceInfo.requiredProductTooltip)
				end)
			labelParent:SetCallback('onmouseout', function()
					parent:DoEventN(3)
					HoN_Store:HideToolTip()
				end)
			labelParent:SetCallback('onclick', function(...)
					parent:DoEventN(1)
				end)
			labelParent:RefreshCallbacks()
		else
			labelParent:SetNoClick(true)
		end
	end

	local middleX = 35
	if data.avatarType == 'small' then
		middleX = 25
	end
	SetupGoldSilverPriceLabel(parent, prefix..'gold', showGold, not showSilver, data.goldCost, 0, middleX)
	SetupGoldSilverPriceLabel(parent, prefix..'silver', showSilver, not showGold, data.silverCost, 50, middleX)
end

local function SetupDifficultyPanel(parent, prefix, difficulty)
	local texPath="/ui/fe2/elements/store2/"
	local difficulty = tonumber(difficulty or '0')
	local difficultyTexs= {'DI2.png','DI.png','DI_h.png'}
	for i=1, 5 do
		local texIndex 	= 1
		if difficulty >= i then
			texIndex 	= 2
		elseif difficulty + 0.5 >= i then
			texIndex 	= 3
		end
		local w = parent:GetWidget(prefix..'_difficulty_diamond_'..i)
		if w==nil then Echo('^rnil for '..prefix..'_difficulty_diamond_'..i) return end
		w:SetTexture(texPath..difficultyTexs[texIndex])
	end
end

function HoN_Store:TimedCheckCursorInSidePanel(panel, insideFunc, outsideFunc)
	panel:Sleep(200, function()
		if panel:IsVisible() then
			local cursorX 	= Input:GetCursorPosX()
			local cursorY 	= Input:GetCursorPosY()
			local panelX  	= panel:GetAbsoluteX()
			local panelY  	= panel:GetAbsoluteY()
			local width	  	= panel:GetWidth()
			local height  	= panel:GetHeight()
			local inside =
				cursorX >= panelX and
				cursorX <= panelX + width and
				cursorY >= panelY and
				cursorY <= panelY + height
			if inside then
				if insideFunc == HoN_Store.TimedCheckCursorInSidePanel then
					HoN_Store:TimedCheckCursorInSidePanel(panel, insideFunc, outsideFunc)
				else
					insideFunc(panel, insideFunc, outsideFunc)
				end
			else
				outsideFunc(panel, insideFunc, outsideFunc)
			end
		end
	end)
end

function HoN_Store:ShowGcaImageInNeed(parent, widgetName, code)
	if not HoN_Store:HasGca() then return end
	local w = parent and parent:GetChildWidget(widgetName) or GetWidget(widgetName)
	if w ~= nil and code and code ~= '' then
		w:SetVisible(IsGCABenifitUpgrades(code)) 
	end
end

local function SetupHeroAvatarFramePanel(parent, prefix, data, forAltAvatar, contextCategory)
	-- icon stuff
	local function SetupAvatarIcons(parent, prefix, data, showTrialInfo)
		prefix = prefix..'icons_'
		local anyIconVisible = false

		if data.heroEntry then
			local altAvatar = data.heroEntry

			-- special icon
			if (altAvatar.limitedEdition or altAvatar.goldCollection or altAvatar.holidayEdition) then
				HoN_Store:SetSpecialIcon(parent:GetWidget(prefix..'special'), altAvatar.limitedEdition, altAvatar.goldCollection, altAvatar.holidayEdition)
				parent:GetWidget(prefix..'special_panel'):SetVisible(1)
				anyIconVisible = true
			else
				parent:GetWidget(prefix..'special_panel'):SetVisible(0)
			end

			-- CESet icon
			if (altAvatar.collectorsSet > 0) then
				if  HoN_Store:SetCEIcon(parent:GetWidget(prefix..'ce'), altAvatar.collectorsSet) then
					parent:GetWidget(prefix..'ce_panel'):SetVisible(1)
					anyIconVisible = true
				end
			else
				parent:GetWidget(prefix..'ce_panel'):SetVisible(0)
			end

			-- ultimate icon
			parent:GetWidget(prefix..'ultimate_panel'):SetVisible(altAvatar.ultimate)
			anyIconVisible = anyIconVisible or altAvatar.ultimate

			-- enhancement icon
			parent:GetWidget(prefix..'enhancement_panel'):SetVisible(altAvatar.hasEnhancement)
			anyIconVisible = anyIconVisible or altAvatar.hasEnhancement

			-- trial
			parent:GetWidget(prefix..'trial_panel'):SetVisible(showTrialInfo)
			anyIconVisible = anyIconVisible or showTrialInfo
		end

		parent:GetWidget(prefix..'panel'):SetVisible(anyIconVisible)
	end

	-- difficulty diamonds
	local function SetupHeroDifficulty(parent, prefix, data)
		if data.difficulty == nil then return end
		SetupDifficultyPanel(parent, prefix, data.difficulty)
		parent:GetWidget(prefix..'difficulty_panel'):SetVisible(data.difficulty ~= nil)
	end

	-- model
	local hero, avatar = ParseHeroAvatar(data.heroAvatarStr)
	local modelPanel = parent:GetWidget(prefix..'model')
	if contextCategory then
		Store2ModelContextManager.SetModelContext(contextCategory, modelPanel)
	end
	SetModelPanelByHeroAvatar(modelPanel, hero, avatar, data.avatarType)
	HoN_Store.widgetToHeroAvatarStrMap[modelPanel] = data.heroAvatarStr
	modelPanel:SetVisible(true)

	-- name
	if data.displayName ~= nil and data.avatarType ~= 'small' then
		local text 	= data.displayName
		local label = parent:GetWidget(prefix..'name')
		local labelWidth = label:GetWidth()

		if label:GetStringWidth(text) <= labelWidth then
			label:GetParent():SetNoClick(true)
		else
			local i = -1
			local shortText = sub(text, 1, i) ..'...'
			while label:GetStringWidth(shortText) > labelWidth do
				i = i - 1
				shortText = sub(text, 1, i).."..."
			end

			local dotPanel = parent:GetWidget(prefix..'name_hover')
			dotPanel:SetText(data.displayName)
			label:GetParent():SetNoClick(false)

			text = shortText
		end

		label:SetText(text)
	end

	-- heroname for avatar
	local heroNameLabel = parent:GetWidget(prefix..'heroname')
	if heroNameLabel ~= nil then
		heroNameLabel:SetVisible(data.heroName ~= nil)
		if data.heroName ~= nil then
			heroNameLabel:SetText(data.heroName)
		end
	end

	-- owned
	parent:GetWidget(prefix..'owned'):SetVisible(data.owned or false)

	-- price
	SetupHeroAvatarFramePrice(parent, prefix, data, forAltAvatar)

	-- difficulty
	SetupHeroDifficulty(parent, prefix, data)

	-- trial, coupon
	local showTrialInfo = false
	local showCoupon = false

	if data.isAltAvatar then
		local ar = split(avatar, '%.')
		avatarCode = ar[2]

		local trialInfo = hero and GetTrialInfo(hero, avatarCode) or nil
		if NotEmpty(trialInfo) and not data.owned then
			showTrialInfo = true
			if data.purchasable then
				showCoupon = true
			else
				showCoupon = false
			end
		else
			showTrialInfo = false

			local cardsTable = hero and GetCardsInfo(hero, avatarCode) or nil
			if (cardsTable) and (#cardsTable >= 1) and not data.owned and data.purchasable then
				showCoupon = true
			else
				showCoupon = false
			end
		end
	end

	parent:GetWidget(prefix..'coupon'):SetVisible(showCoupon)

	-- icons
	SetupAvatarIcons(parent, prefix, data, showTrialInfo)

	-- gca
	HoN_Store:ShowGcaImageInNeed(parent, prefix..'gca', data.productCode)
end

local function EnsureAvatarInfoInited()
	-- gets all alt info from the panels
	if not HoN_Store.allAvatarsInfo then
		HoN_Store.allAvatarsInfo = {}
		groupfcall("storeAltAvatarInfoPanels", function(_, widget, _) widget:DoEvent() end)
	end
end

function HoN_Store:ShowBackToListButton(visible)
	local w = GetWidget('store2_store_common_elements_button_back_to_list')
	w:SetVisible(visible)
end

local function GetHeroIndexByHeroName(heroName)
	heroName = heroName..'.Hero'
	local codes = HoN_Store.ProductHeroes.Codes
	for i, v in ipairs(codes) do
		if heroName == v then return i end
	end
	return -1
end

local function TranslateCategoryString(str)
	local parts = split(str, ' ')
	local result = ''
	for i, v in ipairs(parts) do
		v = Translate('HeroCategory_'..v)
		if not string.match(v, 'HeroCategory_') then
			if i == 1 then
				result = v
			else
				result = result..'  '..v
			end
		end
	end
	return result
end

local function GetCategoryStringFromRating(data)
	local ratingMap = {
		solorating		= 'solo',
		junglerating	= 'jungle',
		carryrating		= 'carry',
		supportrating	= 'support',
		initiatorrating	= 'initiator',
		gankerrating	= 'ganker',
		pusherrating	= 'pusher',
		rangedrating	= 'ranged',
		meleerating		= 'melee',
	}
	local result = ''
	for k, v in pairs(ratingMap) do
		if data[k] >= HeroRatingThreshold then
			local s = Translate('filter_category_'..v)
			if result == '' then
				result = s
			else
				result = result..' '..s
			end
		end
	end
	return result..' '
end

local function SetupHeroPropertyPanel(prefix, data)

	local texPath="/ui/fe2/elements/store2/"

	local parent = GetWidget(prefix..'_info')

	parent:GetWidget(prefix..'_name'):SetText(data.displayName)
	parent:GetWidget(prefix..'_portrait'):SetTexture(data.iconPath)

	local attrTexs 		= {'primary_none.png','primary_strength.png','primary_agility.png','primary_intelligence.png'}
	local attrIndex 	= 1
	if string.match(data.primAttr, '^S') 	 then attrIndex = 2
	elseif string.match(data.primAttr, '^A') then attrIndex = 3
	elseif string.match(data.primAttr, '^I') then attrIndex = 4
	end
	parent:GetWidget(prefix..'_primAttr'):SetTexture(texPath..attrTexs[attrIndex])

	local attackTexs 	= {'melee.png','ranged.png'}
	local attackIndex 	= 1
	if string.match(data.attackType, '^R') then attackIndex = 2 end
	parent:GetWidget(prefix..'_attack'):SetTexture(texPath..attackTexs[attackIndex])

	SetupDifficultyPanel(parent, prefix, data.difficulty)

	local categoryStr 	= GetCategoryStringFromRating(data)
	parent:GetWidget(prefix..'_category'):SetText(categoryStr)

	-- abilities
	for i = 1, 4 do
		local ability 	= data.abilities[i]
		parent:GetWidget(prefix..'_ability_icon_'..i):SetTexture(ability.iconPath)
	end

	parent:SetVisible(true)
end

local function SetupHeroPropertyHoverPanel(heroInfo)
	local parent = GetWidget('store2_hero_property_hover');

	-- numbers
	local numberPrefix = 'store2_hero_hover_info_number_'
	local atkRangeStr = Translate('store2_hero_select_label_attack_range', 'range', round(heroInfo.attackRange))
	parent:GetWidget(numberPrefix..'atkRange'):SetText(atkRangeStr)

	local atkSpeed = FtoA(1000 / tonumber(heroInfo.attackCooldown), 2)
	local atkSpeed = Translate('store2_hero_select_label_attack_speed', 'speed', atkSpeed)
	parent:GetWidget(numberPrefix..'atkSpeed'):SetText(atkSpeed)

	local atkDamageStr = round(heroInfo.attackDamageMin) .. ' - ' .. round(heroInfo.attackDamagemax)
	atkDamageStr = Translate('store2_hero_select_label_damage', 'damage', atkDamageStr)
	parent:GetWidget(numberPrefix..'damage'):SetText(atkDamageStr)

	local moveSpeedStr = Translate('store2_hero_select_label_mvspeed', 'mvspeed', round(heroInfo.moveSpeed))
	parent:GetWidget(numberPrefix..'moveSpeed'):SetText(moveSpeedStr)

	local armorStr = Translate('store2_hero_select_label_armor', 'armor', FtoA(heroInfo.armor, 2))
	parent:GetWidget(numberPrefix..'armor'):SetText(armorStr)

	local magicArmorStr = Translate('store2_hero_select_label_magicarmor', 'armor', FtoA(heroInfo.magicArmor, 2))
	parent:GetWidget(numberPrefix..'magicArmor'):SetText(magicArmorStr)

	parent:GetWidget('store2_hero_hover_info_text_desc'):SetText(heroInfo.role or '')

	--attributes
	local prefix = 'store2_hero_hover_info_attr_'
	parent:GetWidget(prefix..'strength'):SetText(heroInfo.strength)
	parent:GetWidget(prefix..'agility'):SetText(heroInfo.agility)
	parent:GetWidget(prefix..'intelligence'):SetText(heroInfo.intelligence)
	local prefix2 = 'store2_hero_hover_info_attrPerLevel_'
	parent:GetWidget(prefix2..'strength'):SetText('(+'..heroInfo.strengthPerLevel..')')
	parent:GetWidget(prefix2..'agility'):SetText('(+'..heroInfo.agilityPerLevel..')')
	parent:GetWidget(prefix2..'intelligence'):SetText('(+'..heroInfo.intelligencePerLevel..')')

	local primAttr = heroInfo.primAttr
	if primAttr == 'S' then
		primAttr = 'strength'
	elseif primAttr == 'A' then
		primAttr = 'agility'
	elseif primAttr == 'I' then
		primAttr = 'intelligence'
	end
	local attrs = {'strength', 'agility', 'intelligence'}
	for i, v in ipairs(attrs) do
		local attr = attrs[i]
		if attr == primAttr then
			parent:GetWidget(prefix..attr):SetColor('orange')
			parent:GetWidget(prefix2..attr):SetColor('orange')
		else
			parent:GetWidget(prefix..attr):SetColor('#fff7d2')
			parent:GetWidget(prefix2..attr):SetColor('#fff7d2')
		end
	end
end

local function SetupHeroAbilityHoverPanel(ability)
	local prefix = 'store2_hero_skill_hover'
	local parent = GetWidget(prefix)

	prefix = prefix..'_'
	--parent:GetWidget(prefix..'icon'):SetTexture(ability.iconPath)
	parent:GetWidget(prefix..'name'):SetText(ability.name)

	-- mana cost
	local manaWidget = parent:GetWidget(prefix..'mana')
	if NotEmpty(ability.manaCost) then
		manaWidget:SetText(Translate('heroinfo_mana_cost') .. ability.manaCost)
		manaWidget:SetVisible(1)
	else
		manaWidget:ClearText()
		manaWidget:SetVisible(0)
	end

	-- range
	local rangeWidget = parent:GetWidget(prefix..'range')
	local rangeStr = ''
	local range = ''
	local showRange = true
	if NotEmpty(ability.range) then
		range = GetRangeString(ability.range, true)
		rangeStr = Translate('heroinfo_range')
	elseif NotEmpty(ability.targetRadius) then
		range = GetRangeString(ability.targetRadius, false)
		rangeStr = Translate('heroinfo_radius')
	elseif NotEmpty(ability.auraRange) then
		range = GetRangeString(ability.auraRange, true)
		rangeStr = Translate('heroinfo_aura')..' '..Translate('heroinfo_radius')
	else
		showRange = false
	end
	if showRange then
		rangeWidget:SetText(rangeStr..range)
	else
		rangeWidget:ClearText()
	end

	-- cooldown\passive
	local cooldownWidget = parent:GetWidget(prefix..'cd')
	cooldownWidget:SetText(Translate('heroinfo_cooldown', 'time', ability.cd))
	cooldownWidget:SetVisible(NotEmpty(ability.cd) and not AtoB(ability.passitive))

	-- desc
	local descnWidget = parent:GetWidget(prefix..'desc')
	local descStr = BuildMultiLevelTextForString(ability.description, ability.maxLevel, ability.param1, ability.param2)
	descnWidget:SetText(descStr)
end

function HoN_Store:ShowHeroPropertyHoverPanel(show)
	local w = GetWidget('store2_hero_property_hover')
	w:SetVisible(show)
end

local showHeroAbilityTooltip = false
function HoN_Store:ShowHeroAbilityHoverPanel(show, id)
	if show then
		local heroInfo = HoN_Store.CurrentHeroInfo.HeroInfo
		if heroInfo then
			SetupHeroAbilityHoverPanel(heroInfo.abilities[tonumber(id)])
		else
			Echo('^rHoN_Store:ShowHeroAbilityHoverPanel invalid hero info with id'..id)
		end
	end
	local w = GetWidget('store2_hero_skill_hover')
	local wc =  GetWidget('store2_hero_skill_hover_content')
	showHeroAbilityTooltip = show
	if show then
		w:SetVisible(true)
		wc:FadeIn(200)
	else
		wc:FadeOut(200)
		w:Sleep(200, function()
			if not showHeroAbilityTooltip then
				w:SetVisible(false)
			end
		end)
	end
end

function HoN_Store:OnClickHeroVoiceButton()
	local heroName = HoN_Store.CurrentHeroInfo.HeroName
	PlayHeroPreviewSoundFromProduct(heroName..'.Hero')
end

local function GetHeroIndexByName(name)
	local code = name..'.Hero'
	local codes = HoN_Store.ProductHeroes.Codes
	for i, v in ipairs(codes) do
		if v == code then
			return i
		end
	end
	TestEcho('^r GetHeroIndexByName() got nil, name='..name..', code='..code)
end

-- warn: assert DailySpecialJson valid, eligPrices will be cleared
function HoN_Store:AdjustProductPrices(typeName, ids, golds, silvers, productEligibility, checkEligible, updatePriceCvars)

	local function ParseProductEligiblity(productEligibility)

		local map = {}
		local priceMap = {}
		local count = 0

		local function ParseGoldSilverStr(str)
			if str == '' or str == nil then return 0 end
			return tonumber(str)
		end

		if productEligibility ~= nil and productEligibility ~= '' then
			local productEligibilityTable = explode('|', productEligibility)
			for index, eligibilityString in pairs(productEligibilityTable) do
				local eligibilityTable 	= explode('~', eligibilityString)

				local productID			= eligibilityTable[1]
				local eligible			= eligibilityTable[3]
				local t	=
				{
					productID		 	= productID,
					eligbleID		 	= eligibilityTable[2],
					eligible		 	= eligible,
					requiredProducts 	= eligibilityTable[6],
					goldCost		 	= ParseGoldSilverStr(eligibilityTable[4]),
					silverCost		 	= ParseGoldSilverStr(eligibilityTable[5]),
				}
				map[productID] 	= t

				count 					= count + 1
			end
		end

		return map, map, count
	end

	local eligibility, eligPrices, eligCount
	eligibility, eligPrices, eligCount = ParseProductEligiblity(productEligibility)

	local specialPrices, specialsCount
	specialPrices, specialsCount = HoN_Store:GetDailySpecialPrices(typeName)

	if eligCount == 0 and specialsCount == 0 then return eligibility end

	local function HasData(d)
		return d ~= nil and d ~= ''
	end

	local function AdjustPrice(i, id, gold, silver, t, updatePriceCvars)
		TestEcho('^p Adjust '..(t or 'Product')..' Price: i='..i..', id='..id
			..', old='..tostring(golds[i])..'/'..tostring(silvers and silvers[i] or 'nil')
			..', new='..tostring(gold)..'/'..tostring(silver)
			)
		if HasData(gold) then
			golds[i] = gold
		elseif updatePriceCvars then
			gold = golds[i]
		end
		if silvers then
			if HasData(silver) then
				silvers[i] = silver
			elseif updatePriceCvars then
				silver = silvers[i]
			end
		end
		if updatePriceCvars then
			OverridePricesCvars(i, gold, silver)
		end
	end

	for i, id in ipairs(ids) do

		local specialItem = specialPrices[id]

		local validSpecialItem = specialItem ~= nil and specialItem.gold ~= nil and specialItem.gold ~= ''

		if validSpecialItem then

			local gold = specialItem.initial_gold
			if gold ~= '' and gold > 9000 and gold < 9010 then
				gold = nil
			end

			local silver = specialItem.silver

			AdjustPrice(i, id, gold, silver, 'Sepcial', updatePriceCvars)
		else
			local eligItem 	= eligPrices[id]
			local overwriteEligPrice = eligItem ~= nil

			if checkEligible and overwriteEligPrice then
				overwriteEligPrice = AtoB(eligItem.eligible) 
					and eligItem.requiredProducts and string.len(eligItem.requiredProducts) > 0	-- if requeired list is empty, no discount
			end

			if overwriteEligPrice then
				AdjustPrice(i, id, eligItem.goldCost, eligItem.silverCost, 'Elig', updatePriceCvars)
			end
		end
	end

	return eligibility
end

function HoN_Store:AdjustProductPurchableForElig(ids, productPerchasable, productEligibility)
	if not productPerchasable or not productEligibility or #productPerchasable == 0 then
		Echo('^r error: productPerchasable='..tostring(productPerchasable)..', productEligibility='..tostring(productEligibility))
		return
	end
	for i, id in ipairs(ids) do
		if productPerchasable[i] == '0' then
			local t = productEligibility[id]
			if t then
				productPerchasable[i] = t.eligible
			end
		end
	end
end

function HoN_Store:SetupProductEnhancements(ids, productEnhancements, productEnhancementIDs)
	local map = {
		map = {},
		idmap = {},
	}
	if productEnhancements == nil or productEnhancements == '' then return map end
	if productEnhancementIDs == nil or productEnhancementIDs == '' then return map end

	local enhancementTable = explode('|', productEnhancements)
	for index, productCodes in ipairs(enhancementTable) do
		if NotEmpty(productCodes) then
			map.map[tonumber(ids[index])] = explode('~', productCodes)
		end
	end

	local enhancementIDTable = explode('|', productEnhancementIDs)
	for index, productIDList in ipairs(enhancementIDTable) do
		if NotEmpty(productIDList) then
			map.idmap[tonumber(ids[index])] = explode('~', productIDList)
		end
	end

	return map
end

local function GetComboBoxIndex(cat, id)
	return GetCvarNumber('store2_combobox_'..cat..'_'..id..'_value')
end

-- new heroes - ea
HoN_Store.Eap =
{
	Count 			= 0,
	SelectIndex		= 3,	-- immortal
	Rotation		= 0,
	ModelType		= 'special',

	Ids 			= {},
	Codes 			= {},

	Purchasable 	= {},
	Owned 			= {},
	GoldPrice 		= {},
	SilverPrice 	= {},

	Icons 			= {},
	Times			= {},

	BundleNames		= {},

	eapRotationInitialX = nil,
	ProductIdToInit = nil,
}

HoN_Store.CurrentHeroInfo =
{
	HeroInfo		= nil,
	HeroName		= nil,
}

function HoN_Store:SwitchToEaPage(forceUpdate)
	if HoN_Store:Store2GetCurrentPage() == 'store2_hero_ea' and not forceUpdate then return end

	HoN_Store:Store2Loading(1, 'SwitchToEaPage')
	interface:GetWidget('store2_heroes_list'):SetVisible(false)

	HoN_Store:RequestHeroAvatarData('eap', function(success, newData, ...)
			if not success then
				Echo('^r request eap failed')
			else
				if newData then
					HoN_Store:InitEapData(...)
					HoN_Store:InitEapPanel()
				end
				Store2TabHeroes()
				HoN_Store:ShowEapPage()
			end
			HoN_Store:Store2LoadingEndAsync('SwitchToEaPageDone')
		end)
end

function HoN_Store:InitEapData(...)
	local argOffset				= 2
	local eapList				= HoN_Store.Eap

	eapList.Ids 				= split(tostring(arg[argOffset + 2]) 	or '', '|')
	eapList.Codes 				= split(tostring(arg[argOffset + 20]) 	or '', '|')

	eapList.Purchasable 		= split(tostring(arg[argOffset + 50]) 	or '', '|')
	eapList.Owned 				= split(tostring(arg[argOffset + 5]) 	or '', '|')
	eapList.GoldPrice 			= split(tostring(arg[argOffset + 4]) 	or '', '|')
	eapList.SilverPrice 		= split(tostring(arg[argOffset + 35]) 	or '', '|')

	eapList.Icons 				= split(tostring(arg[argOffset + 12]) 	or '', '|')
	eapList.Times				= split(tostring(arg[argOffset + 47]) 	or '', '|')

	eapList.BundleNames			= split(tostring(arg[argOffset + 3]) 	or '', '|')

	local productEligibility	= arg[argOffset + 59]
	HoN_Store:AdjustProductPrices('EAP', eapList.Ids, eapList.GoldPrice, eapList.SilverPrice, productEligibility, true)

	eapList.Count				= #eapList.Ids

	EnsureAvatarInfoInited()

	if eapList.ProductIdToInit ~= nil then

		for i, v in ipairs(eapList.Ids) do
			if v == eapList.ProductIdToInit then
				HoN_Store.Eap.SelectIndex = i
				break
			end
		end

		eapList.ProductIdToInit = nil
	end
end

function HoN_Store:SetupEapSpecialsBoothPanel(id, data, showTime)
	local parent		= interface:GetWidget('store2_specials_booth_'..id)
	local modelPanel	= parent:GetWidget('store2_hero_ea_model_panel_'..id)
	local nameLabel 	= parent:GetWidget('st2_sp_booth_name_label_'..id)

	local timeBack 		= parent:GetWidget('store2_hero_ea_booth_time_'..id)
	local timeLabel 	= parent:GetWidget('store2_hero_ea_booth_time_label_'..id)

	parent:SetVisible(false)

	-- model
	local hero, avatar 	= ParseHeroAvatar(data.code)
	SetModelPanelByHeroAvatar(modelPanel, hero, avatar, HoN_Store.Eap.ModelType)

	-- name
	local displayName	= GetProductDispayNameByID(data.productID)
	HoN_Store:SetLabelText(nameLabel, 'heroname', id, displayName, true, true)

	-- hide price
	HoN_Store:BoothClearPrices(id)

	-- time
	if showTime and data.startTime > 0 then
		local timeStr = FormatDateTime(data.endTime, '%d/%m/%Y', true)

		HoN_Store:SetLabelText(timeLabel, 'info', id, timeStr)
		timeBack:SetVisible(true)
	else
		timeBack:SetVisible(false)
	end

	-- owned
	parent:GetWidget('st2_sp_booth_name_own_'..id):SetVisible(data.owned)

	-- hide info
	local infoLabel = parent:GetWidget('st2_sp_booth_info_label_'..id)
	local infoback  = parent:GetWidget('st2_sp_booth_info_back_'..id)
	if data.info then
		infoback:SetVisible(true)
		infoLabel:SetVisible(true)
		infoLabel:SetText(Translate(data.info))
	else
		infoback:SetVisible(false)
		infoLabel:SetVisible(false)
	end

	parent:SetVisible(true)
end

function HoN_Store:InitEapPanel()

	local eapList = HoN_Store.Eap
	if eapList.Count == 0 then return end

	local heroName = split(eapList.Codes[1], '%.')[1]

	local heroInfo 	= GetHeroInfo(heroName)
	heroInfo.displayName = Translate('store2_new_hero', 'name', heroInfo.displayName)

	local tab = HoN_Store:GetHeroesTab()
	local hasHeroes = tab == 'heroes'
	GetWidget('store2_hero_ea_header_heroes'):SetVisible(hasHeroes)

	SetupHeroPropertyPanel('store2_hero_ea_header', heroInfo)
	SetupHeroPropertyHoverPanel(heroInfo)

	HoN_Store.CurrentHeroInfo.HeroInfo = heroInfo
	HoN_Store.CurrentHeroInfo.HeroName = heroName

	local infos = {nil, 'mstore_eap_left_aa_1', 'mstore_eap_right_aa_1'}
	if eapList.Count >= 3 then
		for i = 1, 3 do
			local data =
			{
				index		= i,
				productID	= eapList.Ids[i],
				code		= eapList.Codes[i],
				info		= infos[i]
			}
			local ownedIndex = 1
			if i == 2 then ownedIndex = 3 end
			if i == 3 then ownedIndex = 2 end
			data.owned		= eapList.Owned[ownedIndex] == '1'

			data.code = string.gsub(data.code, '.eap$', '.Hero')

			local times 	= split(eapList.Times[i], ',')
			data.startTime	= tonumber(times[1])
			data.endTime	= tonumber(times[2])

			data.isAltAvatar= not string.match(data.code, '.Hero$')
			if data.isAltAvatar then
				data.heroEntry = HoN_Store.allAvatarsInfo[tonumber(data.productID)]
			end

			 HoN_Store:SetupEapSpecialsBoothPanel('ea'..i, data, not data.isAltAvatar)
		end
	end
end

function HoN_Store:ShowEapPage(visible)
	visible = visible or true
	interface:GetWidget('store2_hero_ea'):SetVisible(visible)
	interface:GetWidget('store2_heroes_list'):SetVisible(not visible)
	HoN_Store:SelectEapBundle(HoN_Store.Eap.SelectIndex)
end

local function GrayScaleEapBundle(parent, id, grayScale)
	local mode = grayScale and 'grayscale' or 'normal'
	parent:GetWidget('st2_sp_booth_jz_'..id):SetRenderMode(mode)
	parent:GetWidget('st2_sp_booth_name_bck_'..id):SetRenderMode(mode)
	parent:GetWidget('st2_sp_booth_name_own_'..id):SetRenderMode(mode)
	parent:GetWidget('st2_sp_booth_info_back_'..id):SetRenderMode(mode)

	parent:GetWidget('store2_hero_ea_booth_time_'..id):SetRenderMode(mode)

	local color = grayScale and '#ffffff' or '#d7b6a2'
	local outlineColor = grayScale and '#525252' or '#280b0b'

	local label = parent:GetWidget('st2_sp_booth_name_label_'..id)
	label:SetColor(color)
	label:SetOutlineColor(outlineColor)

	label = parent:GetWidget('st2_sp_booth_info_label_'..id)
	label:SetColor(color)
	label:SetOutlineColor(outlineColor)

	label = parent:GetWidget('store2_hero_ea_booth_time_label_'..id)
	label:SetColor(color)
	label:SetOutlineColor(outlineColor)

	label = parent:GetWidget('store2_hero_ea_booth_timepre_label_'..id)
	label:SetColor(color)
	label:SetOutlineColor(outlineColor)
end

local function HighlightEapBundle(id)
	local leftSelected = false
	local rightSelected = false
	local index	 = 0

	if id == 3 then			-- immortal
		index = 3
		leftSelected 	= true
		rightSelected 	= true
	elseif id == 2 then		-- legendary
		index = 2
		leftSelected 	= true
	elseif id == 1 then		-- heroonly
		index = 1
	end

	local ownedList = HoN_Store.Eap.Owned
	local bundleOwned = ownedList[index] == '1'

	local parent = GetWidget('store2_hero_ea')

	-- model
	local leftModel = parent:GetWidget('store2_hero_ea_model_panel_ea3')
	local rightModel = parent:GetWidget('store2_hero_ea_model_panel_ea2')
	local middleModel = parent:GetWidget('store2_hero_ea_model_panel_ea1')

	local sunR, sunG, sunB = middleModel:GetSunColor()
	local ambR, ambG, ambB = middleModel:GetAmbientColor()
	local blackR, blackG, blackB = 0, 0, 0
	local blackAmbR, blackAmbG, blackAmbB = 0.35, 0.35, 0.35

	if leftSelected then
		leftModel:SetSunColor(sunR, sunG, sunB)
		leftModel:SetAmbientColor(ambR, ambG, ambB)
	else
		leftModel:SetSunColor(blackR, blackG, blackB)
		leftModel:SetAmbientColor(blackAmbR, blackAmbG, blackAmbB)
	end
	GrayScaleEapBundle(parent, 'ea3', not leftSelected)

	if rightSelected then
		rightModel:SetSunColor(sunR, sunG, sunB)
		rightModel:SetAmbientColor(ambR, ambG, ambB)
	else
		rightModel:SetSunColor(blackR, blackG, blackB)
		rightModel:SetAmbientColor(blackAmbR, blackAmbG, blackAmbB)
	end
	GrayScaleEapBundle(parent, 'ea2', not rightSelected)

	-- start info
	parent:GetWidget('ms_eap_left_star_1_ea3'):SetVisible(leftSelected)
	parent:GetWidget('ms_eap_left_star_2_ea3'):SetVisible(not leftSelected)
	parent:GetWidget('ms_eap_right_star_1_ea2'):SetVisible(rightSelected)
	parent:GetWidget('ms_eap_right_star_2_ea2'):SetVisible(not rightSelected)

	-- price
	local eapList 	= HoN_Store.Eap
	local goldCost 	= tonumber(eapList.GoldPrice[index])
	local silverCost= tonumber(eapList.SilverPrice[index])

	local showGold, showSilver = ShowGoldSiver(goldCost, silverCost)
	parent:GetWidget('store2_hero_ea_gold'):SetText(showGold and goldCost or '')

	-- icon
	local iconPrefix = 'store2_hero_ea_bundle_'
	local iconNames = {iconPrefix..'1_image', iconPrefix..'2_image', iconPrefix..'3_image'}
	for i, v in ipairs(iconNames) do
		if i == id then
			parent:GetWidget(v):SetTexture('/ui/fe2/elements/store2/bundle2.png')
		else
			parent:GetWidget(v):SetTexture('/ui/fe2/elements/store2/bundle1.png')
		end
	end

	--button
	local buyButton = parent:GetWidget('store2_specials_booth_buy_button_ea')
	local ownedButton = parent:GetWidget('store2_specials_booth_owned_button_ea')
	buyButton:SetVisible(not bundleOwned)
	ownedButton:SetVisible(bundleOwned)

	if not bundleOwned then
		local key = IsFree(goldCost, silverCost) and 'mstore_free' or 'store2_purchase'
		buyButton:SetLabel(Translate(key))
	end

	--purchase parma
	local heroCode = eapList.Codes[1]
	local heroName = GetHeroDisplayNameFromDB(string.match(heroCode, '(.+)%.eap'))
	Set("_microStoreSelectedHeroItem", heroName)
	Set("_microStore_SelectedItem", 999)
	local productID = eapList.Ids[index]
	Set("microStoreID999", productID)
	Set("_microStore_SelectedID", GetCvarInt('microStoreID999'))
	local iconPath = eapList.Icons[1]
	Set("microStoreLocalContent999", iconPath)
	Set("microStorePremium999", true)
	Set("microStorePrice999", goldCost)
	Set("microStoreSilverPrice999", silverCost)
end

function HoN_Store:SelectEapBundle(id)
	HighlightEapBundle(id)
	HoN_Store.Eap.SelectIndex = id
end

local function CalcRotationDegree(panel)
	local currentX 	= Input.GetCursorPosX()
	local delta 	= currentX - HoN_Store.Eap.eapRotationInitialX
	local deltaRate = delta / math.max(1, panel:GetWidth())

	local deltaDegree = deltaRate * 360
	return deltaDegree
end

function HoN_Store:StartEapRotation()
	HoN_Store.Eap.eapRotationInitialX	= Input.GetCursorPosX()
end

function HoN_Store:StopEapRotation(parent)
	if not HoN_Store.Eap.eapRotationInitialX then return end
	HoN_Store.Eap.Rotation = CalcRotationDegree(parent) + HoN_Store.Eap.Rotation
	HoN_Store.Eap.eapRotationInitialX = nil
end

function HoN_Store:UpdateEapModelRotation(parent, cat, count)
	if not HoN_Store.Eap.eapRotationInitialX then return end
	local rotX, rotY = 0, 0
	local rotZ = CalcRotationDegree(parent) + HoN_Store.Eap.Rotation
	local prefix = 'store2_hero_ea_model_panel_'
	for i = 1, count do
		local w = parent:GetWidget(prefix..cat..i)
		w:SetModelAngles(rotX, rotY, rotZ)
	end
end

function HoN_Store:OnHideEapPage()
	HoN_Store:NavHistoryPush2('eap',function()
		if HoN_Store:IsVaultOpened() then
			HoN_Store:Store2OpenShop(false)
		end
		Store2TabHeroes()
		HoN_Store:ShowEapPage(true)
	end)
	HoN_Store:ShowHeroAbilityHoverPanel(false)

	HoN_Store.Eap.SelectIndex = 3 -- reset default
end

HoN_Store.RequestResultHandlers.eap.default = function(success, newData, ...)
	if not success then
		Echo('^r eap.default failed')
	else
		Echo('^r eap.default success')
		HoN_Store:InitEapData(...)
		HoN_Store:InitEapPanel()
		HoN_Store:ShowEapPage()
	end
end

function HoN_Store:ClearEapData()
	local eapList				= HoN_Store.Eap

	eapList.Ids 				= {}
	eapList.Codes 				= {}

	eapList.Purchasable 		= {}
	eapList.Owned 				= {}
	eapList.GoldPrice 			= {}
	eapList.SilverPrice 		= {}

	eapList.Icons 				= {}
	eapList.Times				= {}

	eapList.BundleNames			= {}

	eapList.Count				= 0
end

-- featured
HoN_Store.Featured = {
	AvatarCount	= 2,	-- assertion
	BundleCount	= 3,	-- assertion
	SelectIndex	= 1,
	Rotation	= 0,
	ProductIdToInit = nil,
}

function HoN_Store:SwitchToFeaturedPage(forceUpdate)
	if HoN_Store:Store2GetCurrentPage() == 'store2_featured_ea' and not forceUpdate then return end

	HoN_Store:ResetHeroListFilters()

	HoN_Store:Store2Loading(1, 'SwitchToFeaturedPage')
	HoN_Store:RequestHeroAvatarData('featured', function(success, newData, ...)
			if not success then
				Echo('^r request featured failed')
			else
				if newData then
					HoN_Store:InitFeaturedData(...)
					HoN_Store:InitFeaturedPanel()
				end
				Store2Tab('featured')
				HoN_Store:ShowFeaturedPage()
			end
			HoN_Store:Store2LoadingEndAsync('SwitchToFeaturedPageDone')
		end)
end

function HoN_Store:InitFeaturedData(...)
	local argOffset			= 2
	local list				= HoN_Store.Featured

	list.ProductIds			= split(tostring(arg[argOffset + 2]) 	or '', '|')
	list.ProductCodes 		= split(tostring(arg[argOffset + 3]) 	or '', '|')
	list.ProductOwned 		= split(tostring(arg[argOffset + 5]) 	or '', '|')

	list.BundleSilverCosts	= split(tostring(arg[argOffset + 35]) 	or '', '|')

	list.BundleNames		= split(tostring(arg[argOffset + 61]) 	or '', '|')
	list.BundleCosts		= split(tostring(arg[argOffset + 62]) 	or '', '|')
	list.bundleLocalPaths	= split(tostring(arg[argOffset + 63])	or '', '|')
	list.BundleIncludes		= split(tostring(arg[argOffset + 64]) 	or '', '|')
	list.BundleOwned		= split(tostring(arg[argOffset + 65]) 	or '', '|')
	list.BundleIds			= split(tostring(arg[argOffset + 66]) 	or '', '|')

	local productEligibility= arg[argOffset + 59]
	HoN_Store:AdjustProductPrices('Bundle', 
		list.BundleIds, list.BundleCosts, nil, productEligibility, true)

	-- update owned
	local bundleCount		= #list.BundleOwned
	for i = 1, bundleCount do
		if list.BundleOwned[i] == '0' then
			local allProductOwned = true
			local bundleIncludes = split(list.BundleIncludes[i], '~')
			for j, productInclude in ipairs(bundleIncludes) do
				if productInclude == '1' and list.ProductOwned[j] == '0' then
					allProductOwned = false
					break
				end
			end
			if allProductOwned then
				list.BundleOwned[i] = '1'
			end
		end
	end

	EnsureAvatarInfoInited()

	if list.ProductIdToInit ~= nil then
		for i, v in ipairs(list.BundleIds) do
			if v == list.ProductIdToInit then
				HoN_Store.Featured.SelectIndex = i
				break
			end
		end
		list.ProductIdToInit = nil
	end
end

function HoN_Store:InitFeaturedPanel()

	local list = HoN_Store.Featured
	if list.AvatarCount == 0 then return end

	local heroName = split(list.ProductCodes[1], '%.')[1]	-- same hero asserted

	local heroInfo 	= GetHeroInfo(heroName)
	heroInfo.displayName = Translate('store2_new_hero', 'name', heroInfo.displayName)

	SetupHeroPropertyPanel('store2_featured_ea_header', heroInfo)
	SetupHeroPropertyHoverPanel(heroInfo)

	HoN_Store.CurrentHeroInfo.HeroInfo = heroInfo
	HoN_Store.CurrentHeroInfo.HeroName = heroName

	for i = 1, list.AvatarCount do
		local data =
		{
			index		= i,
			productID	= list.ProductIds[i],
			code		= list.ProductCodes[i],
			owned		= list.ProductOwned[i] == '1',
			info		= 'mstore_noeap_alt_avatar'
		}

		data.isAltAvatar= true
		if data.isAltAvatar then
			data.heroEntry = HoN_Store.allAvatarsInfo[tonumber(data.productID)]
		end

		 HoN_Store:SetupEapSpecialsBoothPanel('fe'..i, data, not data.isAltAvatar)
	end

	local parent = GetWidget('store2_featured')
	local labelPrefix = 'store2_hero_ea_bundle_fe'
	for i = 1, 3 do
		local bundleLabel = parent:GetWidget(labelPrefix..i..'_label')
		local text = GetProductDispayNameByID(list.BundleIds[i])
		bundleLabel:SetText(text)
	end
end

function HoN_Store:ShowFeaturedPage(visible)
	visible = visible or true
	interface:GetWidget('store2_featured'):SetVisible(visible)
	HoN_Store:OnSelectFeaturedBundle(HoN_Store.Featured.SelectIndex)
end

local function HighlightFeaturedBundle(id)

	local list = HoN_Store.Featured
	local parent = GetWidget('store2_featured')

	local bundleOwned = list.BundleOwned[id] == '1'

	-- model
	local includes = split(list.BundleIncludes[id], '~')
	for i = 1, list.AvatarCount do
		local included = includes[i] == '1'
		local model = parent:GetWidget('store2_hero_ea_model_panel_fe'..i)
		if included then
			model:SetSunColor(0.9, 0.9, 0.9)
			model:SetAmbientColor(0.6, 0.7, 0.7)
		else
			model:SetSunColor(0, 0, 0)
			model:SetAmbientColor(0.35, 0.35, 0.35)
		end
		GrayScaleEapBundle(parent, 'fe'..i, not included)

		if included then
			local productOwned = list.ProductOwned[i] == '1'
			if not productOwned then bundleOwned = false end
		end
	end

	-- icon
	local iconPrefix = 'store2_hero_ea_bundle_fe'
	local iconNames = {iconPrefix..'1_image', iconPrefix..'2_image', iconPrefix..'3_image'}
	for i, v in ipairs(iconNames) do
		if i == id then
			parent:GetWidget(v):SetTexture('/ui/fe2/elements/store2/bundle2.png')
		else
			parent:GetWidget(v):SetTexture('/ui/fe2/elements/store2/bundle1.png')
		end
	end

	-- price
	local goldCost = list.BundleCosts[id]
	local silverCost = list.BundleSilverCosts[id]

	local showGold, showSilver = ShowGoldSiver(goldCost, silverCost)
	local free = (not showGold) and (not showSilver)

	local goldText = showGold and goldCost or (free and '0' or '-')
	parent:GetWidget('store2_featured_ea_gold'):SetText(goldText)

	--button
	local buyButton = parent:GetWidget('store2_specials_booth_buy_button_fe')
	local ownedButton = parent:GetWidget('store2_specials_booth_owned_button_fe')
	buyButton:SetVisible(not bundleOwned)
	ownedButton:SetVisible(bundleOwned)

	if not bundleOwned then
		local key = free and 'mstore_free' or 'store2_purchase'
		buyButton:SetLabel(Translate(key))
	end

	--purchase param
	local bundleName = list.BundleNames[id]
	Set("_microStoreSelectedHeroItem", Translate(bundleName))
	Set("_microStore_SelectedItem", 999)
	Set("microStoreID999", list.BundleIds[id])
	Set("_microStore_SelectedID", GetCvarInt('microStoreID999'))
	local icon = list.bundleLocalPaths[id]
	Set("microStoreLocalContent999", icon)
	Set("microStorePremium999", true)
	Set("microStorePrice999", goldCost)
	Set("microStoreSilverPrice999", 0)
end

function HoN_Store:OnSelectFeaturedBundle(id)
	HighlightFeaturedBundle(id)
	HoN_Store.Featured.SelectIndex = id
end

function HoN_Store:UpdateFeaturedModelRotation(frameTime)
	local anglePerSec = 30
	local deltaAngle = anglePerSec / 1000 * frameTime
	local angle = HoN_Store.Featured.Rotation  - deltaAngle
	if angle < 0 then
		angle = angle + 360
	end

	local parent = GetWidget('store2_featured')
	local leftModel = parent:GetWidget('store2_hero_ea_model_panel_fe1')
	local rightModel = parent:GetWidget('store2_hero_ea_model_panel_fe2')

	local angleX = 0
	local angleY = 0
	leftModel:SetModelAngles(angleX, angleY, angle)
	rightModel:SetModelAngles(angleX, angleY, angle)

	HoN_Store.Featured.Rotation = angle
end

function HoN_Store:OnHideFeaturedPage()
	HoN_Store:NavHistoryPush2('featured',function()
		if HoN_Store:IsVaultOpened() then
			HoN_Store:Store2OpenShop(false)
		end
		Store2Tab('featured')
		HoN_Store:ShowFeaturedPage(true)
	end)
	HoN_Store:ShowHeroAbilityHoverPanel(false)

	HoN_Store.Featured.SelectIndex = 1	-- reset default
end

HoN_Store.RequestResultHandlers.featured.default = function(success, newData, ...)
	if not success then
		Echo('^r featured.default failed')
	else
		Echo('^r featured.default success')
		HoN_Store:InitFeaturedData(...)
		HoN_Store:InitFeaturedPanel()
		HoN_Store:ShowFeaturedPage()
	end
end

function HoN_Store:ClearFeaturedData()

	local list				= HoN_Store.Featured

	list.ProductIds			= {}
	list.ProductCodes 		= {}
	list.ProductOwned 		= {}

	list.BundleSilverCosts	= {}

	list.BundleNames		= {}
	list.BundleCosts		= {}
	list.bundleLocalPaths	= {}
	list.BundleIncludes		= {}
	list.BundleOwned		= {}
	list.BundleIds			= {}
end

-- heroes list
local function SortHeroes()
	local sortOrderIndex = GetComboBoxIndex('hero', 'sortOrder')
	local sortOrder = HoN_Store.comboItems.hero.sortOrder[sortOrderIndex]
	table.sort(HoN_Store.ProductHeroes.SortedIndexes, sortOrder.comp)
end

local function ParseSearchStr(searchStr)
	if searchStr == nil then return '' end
	searchStr = searchStr:match("^%s*(.-)%s*$")
	searchStr = searchStr:lower()
	return searchStr
end

local function FilterHeroes()

	local lists 	= HoN_Store.ProductHeroes
	local src 		= lists.SortedIndexes
	local result 	= {}

	--local searchOwned = GetCvarBool('store2_radio_button_hero_owned_isOn')
	local searchPrim 		= GetComboBoxIndex('hero', 'primAttr')
	local searchAttackType 	= GetComboBoxIndex('hero', 'attackType')
	local searchRole 		= GetComboBoxIndex('hero', 'role')
	local searchStr 		= GetWidget('store2_searchbox_hero_textbox'):GetValue()
	searchStr 		= ParseSearchStr(searchStr)

	local ids 		= lists.Ids
	local owned 	= lists.Owned
	local filterPrim 	= HoN_Store.comboItems.hero.primAttr[searchPrim].filter
	local filterAttack 	= HoN_Store.comboItems.hero.attackType[searchAttackType].filter
	local filterRole 	= HoN_Store.comboItems.hero.role[searchRole].filter

	for i, idx in ipairs(src) do
		repeat
			--if not searchOwned and owned[idx] == '1' then break end
			if searchPrim > 1 and not filterPrim(idx) then break end
			if searchAttackType > 1 and not filterAttack(idx) then break end
			if searchRole > 1 and not filterRole(idx) then break end
			table.insert(result, idx)
		until true
	end

	lists.PreFiltedIndexes = result

	if searchStr ~= '' then
		src = result
		result = {}
		for i, idx in ipairs(src) do
			local name = GetProductDispayNameByID(ids[idx])
			if name and name:lower():find(searchStr, 1, true) then
				table.insert(result, idx)
			end
		end
	end

	lists.FilterString 	= searchStr
	lists.FiltedIndexes = result

	Store2ModelContextManager.PrepareNewGeneration('heroList')
end

function HoN_Store:ResetHeroListFilters()
	Set('store2_combobox_hero_sortOrder_value', 1)
	Set('store2_combobox_hero_primAttr_value', 1)
	Set('store2_combobox_hero_attackType_value', 1)
	Set('store2_combobox_hero_role_value', 1)
	GetWidget('store2_searchbox_hero_textbox'):UICmd('EraseInputLine')
	HoN_Store.paging.hero.currentPage = 1
end

function HoN_Store:SwitchToHeroListPage(lazy)
	local resetPage = true
	if resetPage then
		HoN_Store.paging.hero.currentPage = 1
	end

	HoN_Store:Store2Loading(1, 'SwitchToHeroListPage')
	local func = lazy and HoN_Store.RequestHeroAvatarDataOnNil or HoN_Store.RequestHeroAvatarData

	func(HoN_Store, 'heroes', function(success, newData, ...)
			if not success then
				Echo('^r request heroes failed')
			else
				HoN_Store:ResetHeroListFilters()
				if newData then
					HoN_Store:InitializeHeroesList(...)
				else
					SortHeroes()
					FilterHeroes()
				end
				HoN_Store:ShowHeroesPage(HoN_Store.paging.hero.currentPage)
				Store2TabHeroes()
			end

			HoN_Store:Store2LoadingEndAsync('SwitchToHeroListPageDone')
		end)
end

HoN_Store.RequestResultHandlers.heroes.default = function(success, newData, ...)
	if not success then
		Echo('^r heroes.default failed')
	else
		HoN_Store:InitializeHeroesList(...)
		HoN_Store:OnHeroAvatarListDataUpdated(HoN_Store.RequestParam.heroes.cat)
	end
end

function HoN_Store:ShowHeroesPage(page)

	local function InitHeroesPanel(indexes, page)

		HoN_Store:Store2Loading(1, "Paging")

		indexes = indexes or {}
		local heroLists 	= HoN_Store.ProductHeroes
		local productIds	= heroLists.Ids or {}
		local productNames 	= heroLists.Names or {}
		local productGolds 	= heroLists.GoldPrice or {}
		local productSilvers= heroLists.SilverPrice or {}
		local productOwned 	= heroLists.Owned or {}
		local productCodes 	= heroLists.Codes or {}
		local productDifficulties	= heroLists.Difficulty or {}
		local prductPurchasable		= heroLists.Purchasable or {}

		page = page or 1

		local productCount 	= #indexes
		local countPerPage 	= HoN_Store.paging.hero.countPerPage
		local maxPage 		= (productCount - 1) / countPerPage  + 1

		maxPage = math.floor(maxPage)
		page = math.min(page, maxPage)
		page = math.max(page, 1)

		local startIndex 	= HoN_Store.paging.StartIndex(page, 'hero')
		local lastIndex 	= HoN_Store.paging.LastIndex(page, 'hero')
		local endIndex 		= math.min(productCount, lastIndex)

		local data = {
			avatarType 		= 'middle',
			isAltAvatar 	= false,
		}

		local parent = GetWidget('store2_heroes_list')

		-- context issue
		local contextCategory = 'heroList'
		local sm = Store2ModelContextManager
		sm.PrepareNewPage(contextCategory, page)

		HoN_Store:Store2PendingJobsClear()

		-- set hero panels
		for i = startIndex, lastIndex do
			local widgetIndex = i - startIndex + 1
			local prefix = 'store2_avatars_middle_frame_hero_'..widgetIndex..'_'
			local modelCover = parent:GetWidget(prefix..'blank_cover')
			modelCover:SetVisible(1)
			HoN_Store:Store2PendingJobsPush(
				function()
					local heroIndex = indexes[i] or -1
					local panel = parent:GetWidget(prefix..'panel')
					local heroAvatarStr = productCodes[heroIndex]

					local visible = i <= endIndex
					panel:SetVisible(visible)

					if visible then
						data.productID		= productIds[heroIndex]
						data.heroAvatarStr 	= heroAvatarStr
						data.displayName 	= productNames[heroIndex]
						data.goldCost 		= productGolds[heroIndex]
						data.silverCost 	= productSilvers[heroIndex]
						data.owned 			= productOwned[heroIndex] == '1'
						data.difficulty		= productDifficulties[heroIndex]
						data.purchasable	= prductPurchasable[heroIndex] == '1'
						data.productCode	= productCodes[heroIndex]
						data.eligLists 		= heroLists.Eligibility

						SetupHeroAvatarFramePanel(panel, prefix, data, false, contextCategory)
					else
						HoN_Store.widgetToHeroAvatarStrMap[panel:GetWidget(prefix..'model')] = nil
					end

					modelCover:FadeOut(GetCvarInt('store2ModelCoverFadeTime'))
					return true
				end
			)
		end

		HoN_Store:Store2PendingJobsPush(
			function()
				sm.RecyclePagesInCase(contextCategory)
				return true
			end
		)

		HoN_Store:Store2LoadingEndAsync('PagingDone')

		-- set page index
		UpdatePaging('hero', page, productCount, 9)
	end

	interface:GetWidget('store2_hero_ea'):SetVisible(false)
	interface:GetWidget('store2_heroes_list'):SetVisible(true)
	InitHeroesPanel(HoN_Store.ProductHeroes.FiltedIndexes, page)

	HoN_Store.ProductHeroes.PageEverShown = true
end

function HoN_Store:InitializeHeroesList(...)

	local heroLists = HoN_Store.ProductHeroes

	local argOffset				= 2
	heroLists.Ids 				= split(tostring(arg[argOffset + 2]) or '', '|')
	heroLists.Names 			= split(tostring(arg[argOffset + 3]) or '', '|')
	heroLists.GoldPrice 		= split(tostring(arg[argOffset + 4]) or '', '|')
	heroLists.Owned 			= split(tostring(arg[argOffset + 5]) or '', '|')
	heroLists.Codes 			= split(tostring(arg[argOffset + 20]) or '', '|')
	heroLists.SilverPrice 		= split(tostring(arg[argOffset + 35]) or '', '|')
	heroLists.Purchasable 		= split(tostring(arg[argOffset + 50]) or '', '|')

	heroLists.Durations 		= split(tostring(arg[argOffset + 52]) or '', '|')

	local productEligibility	= arg[argOffset + 59]
	heroLists.Eligibility		= HoN_Store:AdjustProductPrices('Hero', 
		heroLists.Ids, heroLists.GoldPrice, heroLists.SilverPrice, productEligibility, true)

	local datas 				= GetHeroAttributes(heroLists.Codes)

	heroLists.Names				= datas.displayName

	heroLists.Difficulty 		= datas.difficulty
	heroLists.PrimAttr			= datas.primattribute
	heroLists.AttackType 		= datas.attacktype

	heroLists.SoloRating  		= datas.solorating
	heroLists.JungleRating  	= datas.junglerating
	heroLists.CarryRating  		= datas.carryrating
	heroLists.SupportRating  	= datas.supportrating
	heroLists.InitiatorRating 	= datas.initiatorrating
	heroLists.GankerRating  	= datas.gankerrating
	heroLists.PusherRating  	= datas.pusherrating
	heroLists.RangedRating  	= datas.rangedrating
	heroLists.MeleeRating  		= datas.meleerating
	heroLists.Categories		= datas.category

	heroLists.Count = #heroLists.Ids
	if #heroLists.SortedIndexes ~= heroLists.Count then
		heroLists.SortedIndexes = CreateIntArray(1, heroLists.Count)
	end

	EnsureAvatarInfoInited()

	SortHeroes()
	FilterHeroes()

	return true
end

function HoN_Store:EditHeroFrame(w, i, dataType)
	local heroAvatarStr = HoN_Store.widgetToHeroAvatarStrMap[w]
	if not heroAvatarStr then
		Echo('EditHeroFrame() error: nil heroAvatarStr for widget '..w:GetName())
		return
	end

	if not GetCvarBool('store2_enable_edit') then
		Echo('[warn] !store2_enable_edit')
	else
		dataType = dataType or 'middle'
		SetEditingHeroPair(w, heroAvatarStr, dataType)

		if dataType == 'large' then
			HoN_Store.EditorPanel_CurrentModelIndex = 0
			HoN_Store:ModelPanelEditorSelectMultiModel(0)
		else
			UpdateEditorInfo()
		end
	end
end

local function ShowHeroDetailPage(rawIndex)
	HoN_Store:Store2Loading(1, 'ShowHeroDetailPage')
	HoN_Store.HeroDetailPage.HeroIndexToInit = rawIndex

	HoN_Store:RequestHeroAvatarDataOnNil('avatars', function(success, newData, ...)
			if not success then
				Echo('^r request avatars failed')
			else
				if newData then
					HoN_Store:InitializeAvatarList(...)
				end
				HoN_Store:InitHeroAvatarPanel(1)
			end
			HoN_Store:SwitchToHeroAvatarPanel(false)
		end)
end

function HoN_Store:OnClickHeroFrame(w, i, dataType)

	dataType = dataType or 'middle'

	local page = HoN_Store.paging.hero.currentPage
	local filtedIndex = (page - 1) * HoN_Store.paging.hero.countPerPage + i
	local rawIndex = HoN_Store.ProductHeroes.FiltedIndexes[filtedIndex]

	if dataType == 'middle' then
		ShowHeroDetailPage(rawIndex)
		HoN_Store.HeroAvatarBackToListCallback = HoN_Store:CreateDefaultHeroAvatarBackToListCallback(false)
	end
end

local searchingHeroStr	= ''
local searchingHeroList = {}

local function UpdateSearchList(str, lists, searchingList)
	lists.FilterString = ParseSearchStr(str)

	if str ~= '' then
		local result = {}
		for i, v in ipairs(searchingList) do
			table.insert(result, v.index)
		end
		lists.FiltedIndexes = result
	else
		lists.FiltedIndexes = lists.PreFiltedIndexes
	end
end

local function SwitchToHeroAvatarPanelFromSearchingHero(filtedIndex)

	local lists = HoN_Store.ProductHeroes
	local rawIndex = lists.FiltedIndexes[filtedIndex]
	ShowHeroDetailPage(rawIndex)

	GetWidget('store2_searchbox_hero_listPanel'):SetVisible(false)

	HoN_Store.HeroAvatarBackToListCallback = function()
			GetWidget('store2_detail_heroAvatars'):SetVisible(false)
			GetWidget('store2_'..HoN_Store:GetHeroesTab()):SetVisible(true)
			HoN_Store:ShowHeroesPage(1)
		end
end

function HoN_Store:OnSearchHero(str)

	UpdateSearchList(str, HoN_Store.ProductHeroes, searchingHeroList)

	local lists = HoN_Store.ProductHeroes
	if #lists.FiltedIndexes == 1 then
		SwitchToHeroAvatarPanelFromSearchingHero(1)
	else
		HoN_Store:ShowHeroesPage(1)
	end

	GetWidget('store2_searchbox_hero_listPanel'):SetVisible(false)
end

local function SearchingHeroAvatar(indexes, ids, maxCount, matchFuc)

	local result 	= {}
	local count		= 0

	for i, idx in ipairs(indexes) do
		local id   = ids[idx]
		local name = GetProductDispayNameByID(id)
		if matchFuc(idx, id, name) then
			if count == maxCount then
				table.insert(result, {id=0})
				break
			else
				table.insert(result, {id=id, name=name, index=idx})
				count = count + 1
			end
		end
	end

	return result
end

function HoN_Store:OnSearchingHero(w, searchStr)

	local products 	= HoN_Store.ProductHeroes

	local matchedIds = {}
	searchingHeroStr = ParseSearchStr(searchStr)
	local displayCount = 5

	if searchingHeroStr ~= '' then
		local src 		= products.PreFiltedIndexes
		local ids 		= products.Ids

		matchedIds = SearchingHeroAvatar(src, ids, -1,
			function(idx, id, name)
				return name and name:lower():find(searchingHeroStr, 1, true)
			end)
	end

	local prefix	= 'store2_searchbox_hero_'
	local panel		= GetWidget(prefix..'listPanel')
	local list		= GetWidget(prefix..'list')
	local count		= #matchedIds

	local function AddListItem(list, data, i, isLast)
		local template = 'store2_heroes_list_search_item'
		local prefix   = 'store2_heroes_list_search_item_'..i
		list:Instantiate(template,
			'i',		i,
			'y',		((i - 1) * 50)..'i',
			'prefix',	prefix,
			'texture', 	data.icon,
			'label',	data.name,
			'showline',	isLast and 'false' or 'true')

		SetupDifficultyPanel(list, prefix, data.difficulty)
	end

	local function AddHasMoreItem(list, count)
		local template = 'store2_heroes_list_search_item_end'
		local text 	= ''
		local height='10i'
		if count >= displayCount then
			text = Translate('store2_search_result_count', 'count', count)
			height = '32i'
		end
		list:Instantiate(template, 'text', text, 'cat', 'hero', 'height', height)
	end

	if count == 0 then
		panel:SetVisible(false)
	else
		local difficulty	= products.Difficulty
		local codes			= products.Codes

		local lastIdnex = math.min(count, displayCount)
		local hasMore = count > displayCount

		list:ClearChildren()
		for i, v in ipairs(matchedIds) do
			v.difficulty 	= difficulty[v.index]

			local code	= codes[v.index] or ''
			local ar = split(code, '%.')
			local heroName = ar[1] or ''
			v.icon 		 	= GetHeroIconFromDB(heroName)

			AddListItem(list, v, i, i == lastIdnex and not hasMore)
			if i == displayCount then break end
		end
		AddHasMoreItem(list, count)
		list:RecalculateSize()
		panel:SetVisible(true)
	end

	searchingHeroList = matchedIds
end

function HoN_Store:OnClickSearchingHeroItem(i)
	UpdateSearchList(searchingHeroStr, HoN_Store.ProductHeroes, searchingHeroList)
	SwitchToHeroAvatarPanelFromSearchingHero(i)
end

function HoN_Store:OnHeroConditionChange(id)
	if HoN_Store.ProductHeroes.Count == 0 then return end
	if id == 'sortOrder' then
		SortHeroes()
	end
	FilterHeroes()
	HoN_Store:ShowHeroesPage(HoN_Store.paging.hero.currentPage)
end

local function SaveHeroListFilterState()
	return{
	 	order	= GetComboBoxIndex('hero', 'sortOrder'),
	 	attr	= GetComboBoxIndex('hero', 'primAttr'),
	 	attack	= GetComboBoxIndex('hero', 'attackType'),
	 	role	= GetComboBoxIndex('hero', 'role'),
	 	search	= HoN_Store.ProductHeroes.FilterString,
	}
end

local function RestoreHeroListFilterState(data)
	local changed = false

	if GetComboBoxIndex('hero', 'sortOrder') ~= data.order then
		Set('store2_combobox_hero_sortOrder_value', 	data.order)
		changed = true
	end
	if GetComboBoxIndex('hero', 'primAttr') ~= data.attr then
		Set('store2_combobox_hero_primAttr_value', 		data.attr)
		changed = true
	end
	if GetComboBoxIndex('hero', 'attackType') ~= data.attack then
		Set('store2_combobox_hero_attackType_value', 	data.attack)
		changed = true
	end
	if GetComboBoxIndex('hero', 'role') ~= data.role then
		Set('store2_combobox_hero_role_value', 			data.role)
		changed = true
	end
	if HoN_Store.ProductHeroes.FilterString ~= data.search then
		HoN_Store.ProductHeroes.FilterString = data.search
		changed = true
	end

	GetWidget('store2_searchbox_hero_textbox'):SetInputLine(data.search)
	return changed
end

function HoN_Store:OnHideHeroListPage()
	local data	= SaveHeroListFilterState()
	local page	= HoN_Store.paging.hero.currentPage

	HoN_Store:NavHistoryPush2('hero list',function()
		if HoN_Store:IsVaultOpened() then
			HoN_Store:Store2OpenShop(false)
		end
		HoN_Store:HideDebugPanel()
		Store2TabHeroes()

		if RestoreHeroListFilterState(data) then
			SortHeroes()
			FilterHeroes()
		end

		HoN_Store:ShowHeroesPage(page)
	end)
end

function HoN_Store:ClearHeroesData()
	local heroLists 			= HoN_Store.ProductHeroes

	heroLists.Ids 				= {}
	heroLists.Names 			= {}
	heroLists.GoldPrice 		= {}
	heroLists.Owned 			= {}
	heroLists.Codes 			= {}
	heroLists.SilverPrice 		= {}
	heroLists.Purchasable 		= {}

	heroLists.Durations 		= {}

	heroLists.Eligibility		= {}

	heroLists.Names				= {}

	heroLists.Difficulty 		= {}
	heroLists.PrimAttr			= {}
	heroLists.AttackType 		= {}

	heroLists.SoloRating  		= {}
	heroLists.JungleRating  	= {}
	heroLists.CarryRating  		= {}
	heroLists.SupportRating  	= {}
	heroLists.InitiatorRating 	= {}
	heroLists.GankerRating  	= {}
	heroLists.PusherRating  	= {}
	heroLists.RangedRating  	= {}
	heroLists.MeleeRating  		= {}
	heroLists.Categories		= {}

	heroLists.Count 			= 0
end

-- hero avatars
local function SetupHeroAvatarRightPanel(srcModelPanel, avatarIndex, heroAvatarStr, overrideGoldCost, overrideSilverCost, purchasable)
	
	local avatarLists 		= HoN_Store.ProductAvatars
	local details 			= HoN_Store.HeroDetailPage

	local srcParentPanel 	= srcModelPanel:GetParent()
	local srcPrefix 		= srcModelPanel:GetName()
	srcPrefix 				= string.sub(srcPrefix, 1, string.len(srcPrefix) - 5)
	local srcSpPriceLabel 	= srcParentPanel:GetWidget(srcPrefix..'price')

	local parent 			= GetWidget('store2_detail_heroAvatars_right')
	local prefix			= 'store2_detail_heroAvatars_right_'

	local isAltAvatar		= avatarIndex ~= HoN_Store.HeroDetailPage.DefaultAvatarIndex
	local showPriceBuy		= isAltAvatar or not HoN_Store:FreeHeros()

	-- name
	local displayName 		= ''
	if isAltAvatar then
		displayName 		= GetAvatarName(avatarIndex)
	else
		local heroName 		= split(heroAvatarStr, '%.')[1]
		displayName 		= Translate("mstore_"..heroName.."_name")
	end

	parent:GetWidget(prefix..'name'):SetText(displayName)

	local heroLists 		= HoN_Store.ProductHeroes
	local heroIndex 		= details.CurrentHeroIndex

	-- icons
	local iconsPrefix		= prefix..'icons'
	local iconsPanel 		= parent:GetWidget(iconsPrefix)
	local productID 		= 0
	if isAltAvatar then
		iconsPanel:GetParent():SetHeight('70i')
		productID 			= avatarLists.Ids[avatarIndex]

		local heroEntry 	= HoN_Store.allAvatarsInfo[tonumber(productID)]

		local hasnewsounds	= heroEntry and heroEntry.hasSounds or false
		local hasnewanims	= heroEntry and heroEntry.hasNewAnimations or false

		iconsPanel:GetWidget(iconsPrefix..'_hasnewsounds'):SetVisible(hasnewsounds)

		iconsPanel:RecalculateSize()
	else
		iconsPanel:GetParent():SetHeight('70i')
		productID 			= HoN_Store:FreeHeros() and 0 or heroLists.Ids[heroIndex]
	end
	iconsPanel:GetParent():DoEventN(1, 'false')
	iconsPanel:SetVisible(isAltAvatar)

	-- price
	local goldPanel			= parent:GetWidget(prefix..'gold_panel')
	local silverPanel		= parent:GetWidget(prefix..'silver_panel')

	local showGold			= false
	local showSilver		= false
	local goldCost 			= ''
	local silverCost 		= ''
	local buyButtonText		= ''
	local priceFree			= false

	if overrideGoldCost or overrideSilverCost then
		showGold			= overrideGoldCost ~= nil
		showSilver			= overrideSilverCost ~= nil
		goldCost			= overrideGoldCost
		silverCost			= overrideSilverCost
	elseif srcSpPriceLabel:IsVisible() then
		buyButtonText 		= srcSpPriceLabel:GetText()
		local freeText 		= Translate('mstore_free')
		priceFree			= buyButtonText == freeText

		if isAltAvatar then
			goldCost		= avatarLists.GoldPrice[avatarIndex] or 0
			silverCost		= avatarLists.SilverPrice[avatarIndex] or 0
		elseif not HoN_Store:FreeHeros() then
			goldCost		= heroLists.GoldPrice[heroIndex] or 0
			silverCost		= heroLists.SilverPrice[heroIndex] or 0
		end
	else
		showGold 			= srcParentPanel:GetWidget(srcPrefix..'gold_panel'):IsVisibleSelf()
		showSilver 			= srcParentPanel:GetWidget(srcPrefix..'silver_panel'):IsVisibleSelf()
		goldCost			= srcParentPanel:GetWidget(srcPrefix..'gold'):GetText()
		silverCost			= srcParentPanel:GetWidget(srcPrefix..'silver'):GetText()
	end

	showGold 	= showGold and showPriceBuy
	showSilver 	= showSilver and showPriceBuy

	local function HasData(d)
		return d ~= nil and d ~= ''
	end

	local priceId = "heroAvatars"
	HoN_Store:BoothClearPrices(priceId)
	if showGold or showSilver then
		local itemID = tostring(productID)
		local dailySpecialInfo = GetDailySpecialEntry(itemID)
		if dailySpecialInfo then
			if showGold and
				HasData(dailySpecialInfo.gold_coins) and
				HasData(dailySpecialInfo.current_gold_coins) and
				dailySpecialInfo.gold_coins ~= dailySpecialInfo.current_gold_coins then

				HoN_Store:BoothSetPrice(priceId, 1, 'gold', 1, true, dailySpecialInfo.gold_coins)
				goldCost = dailySpecialInfo.current_gold_coins

			end
			if showSilver and
				HasData(dailySpecialInfo.silver_coins) and
				HasData(dailySpecialInfo.current_silver_coins) and
				dailySpecialInfo.silver_coins ~= dailySpecialInfo.current_silver_coins then

				HoN_Store:BoothSetPrice(priceId, 2, 'silver', 1, true, dailySpecialInfo.silver_coins)
				silverCost = dailySpecialInfo.current_silver_coins

			end
		end

		if showGold then
			HoN_Store:BoothSetPrice(priceId, 3, 'gold', 1, false, goldCost)
		end
		if showSilver then
			HoN_Store:BoothSetPrice(priceId, 4, 'silver', 1, false, silverCost)
		end
	end

	-- button
	local buyButton, giftButton	= GetWidget(prefix..'buy'), GetWidget(prefix..'gift')
	GetWidget(prefix..'buy_locked'):SetVisible(0)

	local owned			= srcParentPanel:GetWidget(srcPrefix..'owned'):IsVisibleSelf()
	local showBuyButton = true
	local buyButtonTextKey = nil

	local canGift = GetCvarInt('_microStoreGiftsRemaining') > 0 and showPriceBuy and purchasable --and owned
	if giftButton then
		giftButton:SetVisible(canGift)
	end

	if not showPriceBuy then
		showBuyButton 	= false
	elseif owned then
		buyButton:SetEnabled(0)
		buyButtonTextKey = 'mstore_purchased'
	elseif (not isAltAvatar) and HasAllHeroes() then
		GetWidget(prefix..'buy_locked'):SetVisible(1)
		GetWidget(prefix..'buy_locked_image'):SetVisible(0)
		buyButtonText = Translate('mstore_inherited')
		GetWidget(prefix..'buy_locked_label'):SetText(buyButtonText)
		HoN_Store:BoothClearPrices(priceId)
		buyButtonText = ''
		showBuyButton = false
	elseif buyButtonText == '' then
		buyButton:SetEnabled(purchasable)
		buyButtonTextKey = purchasable and 'store2_purchase' or 'general_unavailable'
	elseif priceFree 		then
		buyButton:SetEnabled(purchasable)
		if not purchasable then
			buyButtonTextKey = 'general_unavailable'
		end
	else
		GetWidget(prefix..'buy_locked'):SetVisible(1)
		GetWidget(prefix..'buy_locked_image'):SetVisible(1)
		GetWidget(prefix..'buy_locked_label'):SetText(buyButtonText)
		buyButtonText = ''
		showBuyButton = false
	end

	if buyButtonTextKey ~= nil then
		buyButtonText = Translate(buyButtonTextKey)
	end
	if buyButtonText ~= '' then
		buyButton:SetLabel(buyButtonText)
	end
	buyButton:SetVisible(showBuyButton)

	local ar		= split(heroAvatarStr, '%.')
	local heroName 	= ar[1]
	local altCode 	= ar[2]

	-- coupon
	local showCoupon = false
	if isAltAvatar and not owned and purchasable then
		local cardsTable= GetCardsInfo(heroName, altCode)
		showCoupon = cardsTable and #cardsTable > 0
	end
	GetWidget('store2_detail_heroAvatars_right_coupon'):SetVisible(showCoupon)

	local function SetAvatarPurchaseParam(productId, localContent, gold, silver, catId, displayName, durations, owned)
		Set('_microStore_SelectedItem', 999)
		Set('microStoreID999', productId)
		Set('_microStore_SelectedID', GetCvarInt('microStoreID999'))
		Set('microStoreLocalContent999', localContent)
		Set('microStorePrice999', gold)
		Set('microStorePremium999', false)
		Set('microStoreSilverPrice999', silver)
		Set('_microStoreSelectedHeroItem', displayName, 'string')
		Set('_microStore_Category', catId)

		Set('microStoreCharges999', '-1~-1~-1~-1~-1~-1')
		Set('microStoreDurations999', durations or '-1~-1~-1~-1~-1~-1')
		Set('microStoreChargeRemaining999', '-1~-1~-1')

		Set('_microStore_SelectedItemOwned', GetCvarInt('_microStoreGiftsRemaining') > 0 and owned)
	end

	local cat = nil
	local productIcon = ''
	if isAltAvatar then
		cat = 'avatars'
		productIcon = GetAvatarIconFromDB(heroName, heroAvatarStr)
	elseif not HoN_Store:FreeHeros() then
		cat = 'heroes'
		productIcon = GetHeroIconFromDB(heroName)
	end

	if cat then
		local catId 	= HoN_Store.RequestParam[cat].catId

		local durations	= nil
		if not isAltAvatar then
			durations	= heroLists.Durations[heroIndex]
		end

		SetAvatarPurchaseParam(productID, productIcon, goldCost, silverCost, catId, displayName, durations, owned)
	end
end

local function SelectHeroAvatar(w, i)

	if w == nil then return end

	local heroAvatarStr = HoN_Store.widgetToHeroAvatarStrMap[w]
	if not heroAvatarStr then
		Echo('OnClickHeroAvatarFrame() error: nil heroAvatarStr for widget '..w:GetName())
		return
	end

	local details = HoN_Store.HeroDetailPage

	local page = HoN_Store.paging.heroAvatars.currentPage
	local index = (page - 1) * HoN_Store.paging.heroAvatars.countPerPage + i

	if details.CurrentHeroAvatarStr == heroAvatarStr and index == details.CurrentAvatarIndex then
		return
	end

	local function InitHeroAvtarLargeModel(avatarIndex, heroAvatarStr)
		local heroFrame = GetWidget('store2_detail_heroAvatars_large_model')
		local hero, avatar = ParseHeroAvatar(heroAvatarStr)

		local contextCategory = 'detailLargeModel'
		Store2ModelContextManager.SetModelContext(contextCategory, heroFrame)
		SetModelPanelByHeroAvatar(heroFrame, hero, avatar, 'large')
		HoN_Store.widgetToHeroAvatarStrMap[heroFrame] = heroAvatarStr
		heroFrame:SetVisible(true)
	end

	local function HighlightenHeroAvatarFrame(w)
		local highlightEffect = GetWidget('store2_detail_highlight_heroAvatars')
		if w == nil then
			highlightEffect:SetVisible(false)
		else
			highlightEffect:SetParent(w:GetParent())
			highlightEffect:SetVisible(true)
			highlightEffect:SetAnim('')
			highlightEffect:SetAnim('idle')
			highlightEffect:SetEffect('/ui/fe2/elements/store2/selectedeffect/selected_efx.effect')
		end
	end

	local avatarIndex = details.CurrentHeroAvatars[index]

	if avatarIndex == nil then
		Echo('^rerror: SelectHeroAvatar() i='..tostring(i)..', page='..tostring(page)
			..', index='..tostring(index)..', count='..#details.CurrentHeroAvatars)
		return
	end

	local defaultAvatarGoldCost = nil
	local defaultAvatarSilverCost = nil
	local isDefaultAvatar = index == 1
	if isDefaultAvatar and not HoN_Store:FreeHeros() then
		defaultAvatarGoldCost 	= details.CurrentHeroGoldCost
		defaultAvatarSilverCost = details.CurrentHeroSilverCost
	end

	local avatarLists = HoN_Store.ProductAvatars
	local heroesLists = HoN_Store.ProductHeroes

	-- purchasable
	local purchasable = nil
	local eligPurchable = true

	if isDefaultAvatar then
		if HoN_Store:FreeHeros() then
			purchasable = false
			eligPurchable = false
		else
			local heroIndex = HoN_Store.HeroDetailPage.CurrentHeroIndex
			purchasable = heroesLists.Purchasable[heroIndex] == '1'
		end
	else
		purchasable = avatarLists.Purchasable[avatarIndex] == '1'
	end

	if eligPurchable then
		local lists = isDefaultAvatar and heroesLists or avatarLists
		local index	= isDefaultAvatar and heroIndex or avatarIndex

		local productId = lists.Ids[index]
		if not productId then
			Echo('^r error, nil productId for index '..tostring(index))
		else
			local elig = lists.Eligibility[productId]
			if elig then
				Echo('^r productId='..productId..', eligible='..elig.eligible)
				-- if Eligiblity requirements meets, then product is purchasable regardless of the 'purchable' field
				if AtoB(elig.eligible) then
					if elig.requiredProducts and string.len(elig.requiredProducts) > 0 then -- if requeired list is empty, regard as requirements not met
						purchasable = true
					end
				else
					-- if Eligiblity requirements not meets, and Eligiblity price is 0/0, then product is not purchasable regardless of the 'purchable' field
					if elig.goldCost == 0 and elig.silverCost == 0 then
						purchasable = false
					end
				end
			end
		end
	end

	InitHeroAvtarLargeModel(avatarIndex, heroAvatarStr)
	SetupHeroAvatarRightPanel(w, avatarIndex, heroAvatarStr, defaultAvatarGoldCost, defaultAvatarSilverCost, purchasable)

	HighlightenHeroAvatarFrame(w)

	HoN_Store.HeroDetailPage.CurrentAvatarIndex = index
	HoN_Store.HeroDetailPage.CurrentHeroAvatarStr = heroAvatarStr
	Avatar_Preview_MultiEffects_CurrentIndex = 0 
	Avatar_Preview_MultiModels_CurrentIndex = 0
	Avatar_Preview_SetEffect_On = false

	local function  updatenewfeaturepanel()
		local hero, avatar = ParseHeroAvatar(HoN_Store.HeroDetailPage.CurrentHeroAvatarStr)
		local data = GetHeroPreviewDataFromDB(hero, avatar..'_1', 'large')
		GetWidget('store2_detail_heroAvatars_right_icons_hasnewmodels'):SetVisible(data.newFeatureModel)
		
		data = GetHeroPreviewDataFromDB(hero, avatar, 'large')

		GetWidget('store2_detail_heroAvatars_right_icons_hasneweffects'):SetVisible(#data.multieffects > 0)
		GetWidget('store2_detail_heroAvatars_right_icons_hasnewseteffect'):SetVisible(NotEmpty(data.setEffectPath))	
	end

	updatenewfeaturepanel()
end

local function SetupAltAvatarPanel(parent, prefix, index, avatarType, contextCategory)

	local avatarLists 		= HoN_Store.ProductAvatars
	local avatarGolds 		= avatarLists.GoldPrice
	local avatarSilvers 	= avatarLists.SilverPrice
	local avatarOwned 		= avatarLists.Owned
	local avatarPurchasable = avatarLists.Purchasable
	local avatarIds 		= avatarLists.Ids
	local avatarEligibility = avatarLists.Eligibility

	local detailPage		= HoN_Store.HeroDetailPage
	local altAvatar 		= index ~= detailPage.DefaultAvatarIndex

	-- avatarType
	local data = {
		avatarType 			= avatarType,
		isAltAvatar 		= altAvatar,
	}

	-- name, productID
	if altAvatar then
		data.productID 		= avatarIds[index]
		data.displayName 	= GetProductDispayNameByID(data.productID)
	else
		data.productID 		= 0
		data.displayName 	= Translate('store2_altavatars_default_name')
	end

	-- code
	data.heroAvatarStr = nil
	if altAvatar then
		data.productCode	= avatarLists.Codes[index]
	 	data.heroAvatarStr 	= string.sub(data.productCode, 4)		-- aa.Hero_XXX.AltX
	else
		data.productCode	= detailPage.CurrentHeroName..'.Hero'
		data.heroAvatarStr 	= data.productCode
	end

	-- owned
	data.owned = false
	if altAvatar then
	 	data.owned 			= avatarOwned[index] == '1'
	 	data.purchasable 	= avatarPurchasable[index] == '1'
	elseif HoN_Store:FreeHeros() then
		data.owned 			= true
	else
		local heroLists		= HoN_Store.ProductHeroes
		local heroIndex 	= detailPage.CurrentHeroIndex
		data.owned 			= heroLists.Owned[heroIndex] == '1'
	 	data.purchasable 	= heroLists.Purchasable[index] == '1'
	end

	-- price
	data.goldCost = (altAvatar and tonumber(avatarGolds[index])) or ''
	data.silverCost = (altAvatar and tonumber(avatarSilvers[index])) or ''

	-- entryId
	if altAvatar then
		data.heroEntry = HoN_Store.allAvatarsInfo[tonumber(data.productID)]
		data.eligLists = avatarEligibility
	end

	if altAvatar and avatarType == 'middle' then
		data.heroName = GetHeroDisplayNameByAvatarIndex(index)
	end

	SetupHeroAvatarFramePanel(parent, prefix, data, true, contextCategory)
end

local function SetupDetailFrames(cat, page, itemCount, itemIndex, initer)
	local startIndex 	= HoN_Store.paging.StartIndex(page, cat)
	local lastIndex 	= HoN_Store.paging.LastIndex(page, cat)
	local endIndex 		= math.min(itemCount, lastIndex)

	local widgetCount 	= HoN_Store.paging[cat].countPerPage
	local parent = GetWidget('store2_detail_'..cat..'_avatars')

	-- context issue
	local sm = Store2ModelContextManager
	sm.PrepareNewPage(cat, page)

	local itemFrame = nil
	for i = 1, widgetCount do
		local prefix = 'store2_alt_small_frame_'..cat..'_'..i..'_'
		local panel = parent:GetWidget(prefix..'panel')

		local j = startIndex + i - 1
		local visible = j <= endIndex
		panel:SetVisible(false)

		if visible then
			HoN_Store:Store2PendingJobsPush(
				function()
					initer(panel, prefix, j)
					panel:SetVisible(true)
					PlaySound('/ui/fe2/store/sounds/shelf_item_fadein.wav')
					return true
				end
			)
			if i == itemIndex then
				itemFrame = panel:GetWidget(prefix..'model')
			end

		else
			HoN_Store.widgetToHeroAvatarStrMap[panel:GetWidget(prefix..'model')] = nil
		end
	end

	UpdatePaging(cat, page, itemCount)

	local maxPage = HoN_Store.paging.MaxPage(cat, itemCount)
	HoN_Store:InitializePaging2Dots(cat, page, maxPage)

	return itemFrame
end

function HoN_Store:ShowHeroAvatarPanels(page, avatarIndex, fromPagging)

	local detailPage	= HoN_Store.HeroDetailPage
	local heroAvatars 	= detailPage.CurrentHeroAvatars
	local avatarCount 	= #heroAvatars

	local countPerPage = HoN_Store.paging.heroAvatars.countPerPage

	local itemIndex 	= 0
	if fromPagging then
		itemIndex		= detailPage.CurrentAvatarIndex - (page - 1) * countPerPage
	else
		page 			= (avatarIndex - 1) / countPerPage + 1
		page			= math.floor(page)
		itemIndex		= avatarIndex - (page - 1) * countPerPage
	end

	local avatarLists 	= HoN_Store.ProductAvatars
	local indexes 		= heroAvatars

	local cat = 'heroAvatars'
	local itemFrame = SetupDetailFrames(cat, page, avatarCount, itemIndex, function(panel, prefix, i)
			local index = indexes[i]
			SetupAltAvatarPanel(panel, prefix, index, 'small', cat)
		end)

	if not fromPagging then
		local largeModel = GetWidget('store2_detail_heroAvatars_large_model')
		largeModel:SetModel('')
		largeModel:SetEffect('')
	end

	if fromPagging then
		local SelectedItemPage 	= (detailPage.CurrentAvatarIndex - 1) / countPerPage + 1
		SelectedItemPage		= math.floor(SelectedItemPage)
		local isSelectedItemPage = page == SelectedItemPage
		local highlightEffect 	= GetWidget('store2_detail_highlight_heroAvatars')
		highlightEffect:SetVisible(isSelectedItemPage)
	else
		HoN_Store:Store2PendingJobsPush(
			function()
				SelectHeroAvatar(itemFrame, itemIndex)
				return true
			end
		)
	end
	HoN_Store:Store2PendingJobsPush(
		function()
			Store2ModelContextManager.RecyclePagesInCase(cat)
			return true
		end
	)

	HoN_Store:Store2LoadingEndAsync('ShowHeroAvatarPanelsDone')
end

function HoN_Store:InitHeroAvatarPanel(heroAvatarIndex)

	local heroLists		= HoN_Store.ProductHeroes
	local avatarLists 	= HoN_Store.ProductAvatars
	local detailPage	= HoN_Store.HeroDetailPage

	local heroIndex 	= detailPage.HeroIndexToInit
	local avatarIndex	= detailPage.AvatarIndexToInit
	
	local function Clear(detailPage)
		detailPage.HeroIndexToInit 		= 0
		detailPage.AvatarIndexToInit	= 0
		detailPage.HeroProductIdToInit 	= '0'
		detailPage.AvatarProductIdToInit = '0'
		detailPage.CurrentHeroAvatarStr	= nil
		detailPage.HeroCodeToInit		= nil
		detailPage.AvatarCodeToInit		= nil
	end

	if detailPage.HeroProductIdToInit ~= '0' then
		heroIndex = HoN_Store:GetIndex(heroLists.Ids, detailPage.HeroProductIdToInit, true)
		if heroIndex == nil then
			Echo('^rnil heroIndex for HeroProductIdToInit='..detailPage.HeroProductIdToInit..', name='..GetProductDispayNameByID(id))
		end
	elseif detailPage.AvatarProductIdToInit ~= '0' then
		avatarIndex = HoN_Store:GetIndex(avatarLists.Ids, detailPage.AvatarProductIdToInit, true)
	elseif detailPage.HeroCodeToInit ~= nil then
		if not HoN_Store:FreeHeros() then
			heroIndex = HoN_Store:GetIndex(heroLists.Codes, detailPage.HeroCodeToInit, true)
			if heroIndex == nil then
				Echo('^rnil heroIndex for HeroCodeToInit='..detailPage.HeroCodeToInit)
			end
		end
	elseif detailPage.AvatarCodeToInit ~= nil then
		avatarIndex = HoN_Store:GetIndex(avatarLists.Codes, detailPage.AvatarCodeToInit, true)
	end

	local heroName		= nil
	local avatarName	= nil

	if avatarIndex > 0 then
		heroName 		= GetHeroNameByAvatarIndex(avatarIndex)
		avatarName 		= avatarLists.Codes[avatarIndex]
	elseif heroIndex > 0 then
		local heroCode 	= heroLists.Codes[heroIndex]
		heroName 		= split(heroCode, '%.')[1]
	elseif detailPage.HeroCodeToInit ~= nil and HoN_Store:FreeHeros() then		
		heroName 		= split(detailPage.HeroCodeToInit, '%.')[1]
	elseif detailPage.AvatarCodeToInit ~= nil then
		heroName = split(detailPage.AvatarCodeToInit, '%.')[2]
	elseif detailPage.HeroCodeToInit ~= nil then
		heroName = split(detailPage.HeroCodeToInit, '%.')[1]
	else
		Echo('^r avatarIndex='..tostring(avatarIndex)..', heroIndex='..tostring(heroIndex))
		Clear(detailPage)
		return
	end

	detailPage.CurrentHeroFiltedIndex = 0
	if heroIndex > 0 and #heroLists.FiltedIndexes > 1 then
		for i, v in ipairs(heroLists.FiltedIndexes) do
			if v == heroIndex then
				detailPage.CurrentHeroFiltedIndex = i
			end
		end
	end

	-- clear
	Clear(detailPage)

	if not HoN_Store:FreeHeros() then
		if not heroIndex or heroIndex == 0 then
			heroIndex = GetHeroIndexByName(heroName)
		end
		detailPage.CurrentHeroGoldCost = heroLists.GoldPrice[heroIndex]
		detailPage.CurrentHeroSilverCost = heroLists.SilverPrice[heroIndex]
	end

	detailPage.CurrentHeroName		= heroName
	detailPage.CurrentHeroIndex 	= heroIndex

	local function InitHeroAvatarPanels(heroName, avatarName, heroAvatarIndex, avatarIndexInList)

		local heroAvatars = { detailPage.DefaultAvatarIndex }
		local avatarCount = 1
		local avatarIndex = 0

		local avatarCodes = avatarLists.Codes
		local pattern = '^aa.'..heroName..'%.'

		for i = 1, avatarLists.Count do
			local avatarCode = avatarCodes[i]
			if string.match(avatarCode, pattern) then

				local insertPos = avatarCount + 1
				if avatarCount > 1 then
					-- sort by release time(productID)
					local avatarID = avatarLists.Ids[i]
					for j = 2, avatarCount do
						local currentID = avatarLists.Ids[heroAvatars[j]]
						if tonumber(currentID) < tonumber(avatarID) then
							insertPos = j
							break
						end
					end
				end
				table.insert(heroAvatars, insertPos, i)

				if insertPos <= avatarIndex then
					avatarIndex = avatarIndex + 1
				elseif avatarIndexInList and i == avatarIndexInList then
					avatarIndex = insertPos
				elseif avatarIndex == 0 and avatarName and avatarCode == avatarName then
					avatarIndex = insertPos
				end

				avatarCount = avatarCount + 1
			end
		end

		if avatarIndex == 0 or heroAvatarIndex ~= nil then
			avatarIndex = heroAvatarIndex or 1
		end

		Store2ModelContextManager.PrepareNewGeneration('heroAvatars')
		Store2ModelContextManager.PrepareNewGeneration('detailLargeModel')
		Store2ModelContextManager.PrepareNewPage('detailLargeModel', 1)

		detailPage.CurrentHeroAvatars = heroAvatars
		HoN_Store:ShowHeroAvatarPanels(1, avatarIndex)
	end

	InitHeroAvatarPanels(heroName, avatarName, heroAvatarIndex, avatarIndex)

	local heroInfo = GetHeroInfo(heroName)

	if heroInfo == nil then
		Echo('^r nil heroInfo for '..tostring(heroName)
			..', heroAvatarIndex='..tostring(heroAvatarIndex)
			..', avatarName='..tostring(avatarName))
		return
	end

	SetupHeroPropertyPanel('store2_detail_heroAvatars', heroInfo)
	SetupHeroPropertyHoverPanel(heroInfo)

	HoN_Store.CurrentHeroInfo.HeroInfo = heroInfo
	HoN_Store.CurrentHeroInfo.HeroName = heroName
end

function HoN_Store:SwitchToHeroAvatarPanel(fromAvatarList, fromSpecials)
	if fromSpecials then
		GetWidget('store2_specials'):SetVisible(false)
	end

	local w = GetWidget('store2_altavatars')
	if w:IsVisible() then
		w:SetVisible(false)
	end

	w = GetWidget('store2_'..HoN_Store:GetHeroesTab())
	if w:IsVisible() then
		w:SetVisible(false)
	end

	GetWidget('store2_detail_heroAvatars'):SetVisible(true)

	local arrowsPanel = GetWidget('store2_detail_heroAvatars_arrows')
	if fromAvatarList or fromSpecials then
		arrowsPanel:SetVisible(false)
	else
		local filterdHeroCount = #HoN_Store.ProductHeroes.FiltedIndexes
		arrowsPanel:SetVisible(filterdHeroCount > 1)
	end

	HoN_Store.HeroDetailPage.FromAvatarList = fromAvatarList
end

function HoN_Store:OnClickHeroAvatarFrame(w, i)
	SelectHeroAvatar(w, i)
	StopSound(1)
end

function HoN_Store:OnHideHeroAvatarsPage()
	local detailList 		= HoN_Store.HeroDetailPage
	local tFromAvatarList 	= detailList.FromAvatarList

	local tSelectedAvatarIndex 	= detailList.CurrentAvatarIndex
	local tHeroIndexToInit		= 0
	local tAvatarIndexToInit	= 0

	if #detailList.CurrentHeroAvatars > 1 then
		tAvatarIndexToInit = detailList.CurrentHeroAvatars[2]
	elseif not HoN_Store:FreeHeros() then
		tHeroIndexToInit = detailList.CurrentHeroIndex
	else
		Echo('^rdetailList.CurrentAvatarIndex='..tostring(detailList.CurrentAvatarIndex)
			..', detailList.CurrentHeroIndex='..tostring(detailList.CurrentHeroIndex))
	end

	local data	= SaveHeroListFilterState()
	local page 	= HoN_Store.paging.heroAvatars.currentPage

	HoN_Store:NavHistoryPush2('hero avatars',function()
		if HoN_Store:IsVaultOpened() then
			HoN_Store:Store2OpenShop(false)
		end
		HoN_Store:HideDebugPanel()
		local tabName = tFromAvatarList and 'altavatars' or HoN_Store:GetHeroesTab()
		Store2Tab(tabName)

		if RestoreHeroListFilterState(data) then
			SortHeroes()
			FilterHeroes()
		end

		detailList.HeroIndexToInit = tHeroIndexToInit
		detailList.AvatarIndexToInit = tAvatarIndexToInit
		HoN_Store:InitHeroAvatarPanel(tSelectedAvatarIndex)
		HoN_Store:SwitchToHeroAvatarPanel(tFromAvatarList)

		if HoN_Store.paging.heroAvatars.currentPage ~= page then
			HoN_Store:Store2Loading(1, 'ShowHeroAvatarPanels')
			HoN_Store:Store2PendingJobsPush(
				function()
					HoN_Store:ShowHeroAvatarPanels(page, 0, true)
					HoN_Store:Store2LoadingEndAsync('ShowHeroAvatarPanelsDone')
					return true
				end
			)
		end

		HoN_Store.HeroAvatarBackToListCallback = HoN_Store:CreateDefaultHeroAvatarBackToListCallback(tFromAvatarList)
	end)

	HoN_Store:ShowHeroAbilityHoverPanel(false)
	StopSound(1)
end

function HoN_Store:OnPlayAvatarSound()
	PlayHeroPreviewSoundFromProduct(HoN_Store.HeroDetailPage.CurrentHeroAvatarStr)
end

function HoN_Store:OnShowNewAnims()
	GetWidget('store2_detail_heroAvatars_large_model'):PlayNextAnim()
end

function HoN_Store:OnShowNewSetEffect()
	local hero, avatar = ParseHeroAvatar(HoN_Store.HeroDetailPage.CurrentHeroAvatarStr)
	if Avatar_Preview_MultiModels_CurrentIndex > 0 then
		avatar = avatar..'_'..tostring(Avatar_Preview_MultiModels_CurrentIndex)
	end

	local data = GetHeroPreviewDataFromDB(hero, avatar, 'large')

	if NotEmpty(data.setEffectPath) then
		if Avatar_Preview_SetEffect_On then
			GetWidget('store2_detail_heroAvatars_large_model'):SetMultiEffect('', MODEL_PANEL_EFFECT_CHANNEL_MAX)
		else
			GetWidget('store2_detail_heroAvatars_large_model'):SetMultiEffect(data.setEffectPath, MODEL_PANEL_EFFECT_CHANNEL_MAX)
		end

		Avatar_Preview_SetEffect_On = not Avatar_Preview_SetEffect_On
	end
end

function HoN_Store:OnShowNewEffects()
	-- Echo('HoN_Store.HeroDetailPage.CurrentAvatarIndex = '..tostring(HoN_Store.HeroDetailPage.CurrentAvatarIndex))
	-- Echo('HoN_Store.HeroDetailPage.CurrentHeroAvatarStr = '..HoN_Store.HeroDetailPage.CurrentHeroAvatarStr)

	local hero, avatar = ParseHeroAvatar(HoN_Store.HeroDetailPage.CurrentHeroAvatarStr)
	if Avatar_Preview_MultiModels_CurrentIndex > 0 then
		avatar = avatar..'_'..tostring(Avatar_Preview_MultiModels_CurrentIndex)
	end
	local data = GetHeroPreviewDataFromDB(hero, avatar, 'large')

	local multiEffectsCount = #data.multieffects
	if multiEffectsCount > 0 then
		for i=0, MODEL_PANEL_EFFECT_CHANNEL_MAX-1 do
			GetWidget('store2_detail_heroAvatars_large_model'):SetMultiEffect('', i)
		end

		if Avatar_Preview_MultiEffects_CurrentIndex + 1 > multiEffectsCount then
			--GetWidget('store2_detail_heroAvatars_large_model'):SetEffect(data.effectPath)
			Avatar_Preview_MultiEffects_CurrentIndex = 1
		else
			Avatar_Preview_MultiEffects_CurrentIndex = Avatar_Preview_MultiEffects_CurrentIndex + 1
			
		end
		local effects = explode('|', data.multieffects[Avatar_Preview_MultiEffects_CurrentIndex]) or {}
		GetWidget('store2_detail_heroAvatars_large_model'):SetEffect('')
		for i, v in ipairs(effects) do
			if i > MODEL_PANEL_EFFECT_CHANNEL_MAX then
				break
			end
			GetWidget('store2_detail_heroAvatars_large_model'):SetMultiEffect(v, i-1)
		end

	end
end

function HoN_Store:OnShowNewModels()
	local hero, avatar = ParseHeroAvatar(HoN_Store.HeroDetailPage.CurrentHeroAvatarStr)

	local data = GetHeroPreviewDataFromDB(hero, avatar..'_'..(Avatar_Preview_MultiModels_CurrentIndex+1), 'large')

	local w = GetWidget('store2_detail_heroAvatars_large_model')

	if not data.newFeatureModel and Avatar_Preview_MultiModels_CurrentIndex == 0 then
		return
	end

	for i=0, MODEL_PANEL_EFFECT_CHANNEL_MAX do
		w:SetMultiEffect('', i)
	end

	Avatar_Preview_MultiEffects_CurrentIndex = 0
	Avatar_Preview_MultiModels_CurrentIndex = Avatar_Preview_MultiModels_CurrentIndex + 1
	Avatar_Preview_SetEffect_On = false

	if not data.newFeatureModel then
		Avatar_Preview_MultiModels_CurrentIndex = 0
		data = GetHeroPreviewDataFromDB(hero, avatar, 'large')
	end

	GetWidget('store2_detail_heroAvatars_right_icons_hasneweffects'):SetVisible(#data.multieffects > 0)
	GetWidget('store2_detail_heroAvatars_right_icons_hasnewseteffect'):SetVisible(NotEmpty(data.setEffectPath))	

	w:SetModel(data.modelPath)

	if data.usingEffect then
		w:SetEffect(data.effectPath)
	else
		w:SetEffect('')
	end

	local scale = data.modelScale
	local pos = data.modelPosition
	local angle = data.modelAngle
	local ambient = data.ambient
	local sunHeight = data.sunHeight		
	local sunAngle = data.sunAngle
	local sunColor = data.sunColor
	local pedestalScale = data.pedestalScale

	w:SetModelScale(scale)
	w:SetModelPos(pos.x, pos.y, pos.z)
	w:SetModelAngles(angle.x, angle.y, angle.z)

	w:SetAmbientColor(ambient.x, ambient.y, ambient.z)
	w:SetSunPosition(sunHeight, sunAngle)
	w:SetSunColor(sunColor.x, sunColor.y, sunColor.z)

	w:SetModelScale(0, pedestalScale)

	GetWidget('store2_detail_heroAvatars_right_icons_hasnewanims'):SetVisible(w:GetValidAnimCount() > 0)
end

function HoN_Store:OnHeroAvatarViewSwitchHero(toNextPage)
	local filterdHeroCount	= #HoN_Store.ProductHeroes.FiltedIndexes

	local detailList 		= HoN_Store.HeroDetailPage
	local heroIndexToInit 	= detailList.CurrentHeroFiltedIndex
	local AvatarIndexToInit	= 1

	if toNextPage then
		heroIndexToInit = heroIndexToInit + 1
		if heroIndexToInit > filterdHeroCount then
			heroIndexToInit = 1
		end
	else
		heroIndexToInit = heroIndexToInit - 1
		if heroIndexToInit < 1 then
			heroIndexToInit = filterdHeroCount
		end
	end

	heroIndexToInit = HoN_Store.ProductHeroes.FiltedIndexes[heroIndexToInit]

	detailList.HeroIndexToInit = heroIndexToInit

	HoN_Store:Store2Loading(1, 'OnHeroAvatarViewSwitchHero')
	HoN_Store:InitHeroAvatarPanel(AvatarIndexToInit)
end

function HoN_Store:OnHeroAvatarListDataUpdated(changedCat)
	local details = HoN_Store.HeroDetailPage
	local heroAvatarPageShowing = details.IsShowing

	if heroAvatarPageShowing then

		local isAltAvatar = details.CurrentAvatarIndex ~= 1
		if isAltAvatar then
			local rawIndex = details.CurrentHeroAvatars[details.CurrentAvatarIndex]
			HoN_Store:ShowAvatarDetailPage(rawIndex)
		else
			ShowHeroDetailPage(details.CurrentHeroIndex)
		end

		-- update list view(back to list button)
		local updateHeroListPage = (not details.FromAvatarList) and changedCat == HoN_Store.RequestParam.heroes.cat
		local updateAvatarListPage = details.FromAvatarList and changedCat == HoN_Store.RequestParam.avatars.cat
		HoN_Store.HeroAvatarBackToListCallback = function()
				if updateHeroListPage then
					HoN_Store:ShowHeroesPage(HoN_Store.paging.hero.currentPage)
					Store2TabHeroes()
				elseif updateAvatarListPage then
					HoN_Store:ShowAvatarsPage(HoN_Store.paging.avatars.currentPage)
					Store2Tab('altavatars')
				end
			end
	end
end

local function ShowUnavailableTooltip(heroEntry)

	HoN_Store.puchaseUnavailableConditions =
	{
		heroEntry.limitedEdition,
		heroEntry.goldCollection,
		heroEntry.holidayEdition,
		heroEntry.collectorsSet,
		heroEntry.productID
	}

	local title, body1, body2, body3 = GetUnavailableTooltipLabels()

	if title and body1 then
		HoN_Store:ShowToolTip2(title, body1, body2, body3)
	end
end

function HoN_Store:OnHoverAvatarBuyButton(hover)

	if not hover then HoN_Store:HideToolTip() return end

	local button = GetWidget('store2_detail_heroAvatars_right_buy')
	if button:IsVisible() and button:IsEnabled() then return end

	local details = HoN_Store.HeroDetailPage
	local index = details.CurrentAvatarIndex
	local isDefaultAvatar = index == 1

	if isDefaultAvatar then

	else
		local avatarIndex = details.CurrentHeroAvatars[index]

		-- owned
		local avatarLists = HoN_Store.ProductAvatars
		if avatarLists.Owned[avatarIndex] == '1' then
			return
		end

		local productID	= avatarLists.Ids[avatarIndex]

		-- unlock me
		local eligLists = HoN_Store.ProductAvatars.Eligibility
		if HoN_Store:ShowEligibilityToolTipInNeed(productID, eligLists) then
			return
		end

		-- set/edition
		local heroEntry = HoN_Store.allAvatarsInfo[tonumber(productID)]

		if heroEntry then
			ShowUnavailableTooltip(heroEntry)
		else
			Echo('^rnil heroEntry for='..tostring(productID)
				..', code='..tostring(avatarLists.Codes[avatarIndex]))
		end
	end
end

function HoN_Store:NavToHeroAvatarDetailPage(isAvatar, noNavHistory, backToListFunc)
	HoN_Store:EnsureHeroAndAvatarData(function()
			if noNavHistory then
				Store2NavBackInProgress = true
			end

			local isAvatarTab = isAvatar or HoN_Store:FreeHeros()
			local tabName = isAvatarTab and 'altavatars' or HoN_Store:GetHeroesTab()
			Store2Tab(tabName)

			local selectIndex = nil
			if not isAvatar then selectIndex = 1 end
			HoN_Store:InitHeroAvatarPanel(selectIndex)

			if not noNavHistory then
				Store2NavBackInProgress = true
			end
			HoN_Store:SwitchToHeroAvatarPanel(isAvatar, true)
			Store2NavBackInProgress = false

			HoN_Store.HeroAvatarBackToListCallback = backToListFunc or function()
					local peek = HoN_Store:NavHistoryPeek()
					if peek and peek.name == 'DailySpecials' then
						HoN_Store:NavigateBack()
					elseif isAvatarTab then
						HoN_Store:SwitchToAvatarListPage(true)
					else
						HoN_Store:SwitchToHeroListPage(true)
					end
				end
		end)
end

function HoN_Store:OnHeroAvatarBackToList()
	if HoN_Store.HeroAvatarBackToListCallback then
		HoN_Store.HeroAvatarBackToListCallback()
	end
end

function HoN_Store:CreateDefaultHeroAvatarBackToListCallback(toAvatarList)
	return function()
		GetWidget('store2_detail_heroAvatars'):SetVisible(false)
		if toAvatarList then
			if HoN_Store.ProductAvatars.PageEverShown then
				GetWidget('store2_avatars_list'):SetVisible(true)
				Store2Tab('altavatars')
			else
				HoN_Store:SwitchToAvatarListPage(true)
			end
		else
			if HoN_Store.ProductHeroes.PageEverShown then
				GetWidget('store2_hero_ea'):SetVisible(false)
				GetWidget('store2_heroes_list'):SetVisible(true)
				Store2TabHeroes()
			else
				HoN_Store:SwitchToHeroListPage(true)
			end
		end
	end
end

-- avatars list
local function SortAvatars()
	local sortOrderIndex = GetComboBoxIndex('avatars', 'sortOrder')
	local sortOrder = HoN_Store.comboItems.avatars.sortOrder[sortOrderIndex]
	table.sort(HoN_Store.ProductAvatars.SortedIndexes, sortOrder.comp)
end

local function GetAvatarOwnedCvarName()
	local hasAllHeroes = AtoB(interface:UICmd("HasAllHeroes"))
	local id = hasAllHeroes and 'owned' or 'ownedHeroes'
	return 'store2_combobox_avatars_'..id..'_value', id
end

local function GetAvatarSetSearchOption(searchStr)
	local result = {}
	local ceSetSearchTable 	= {}
	if searchStr ~= '' then
		result.searchGold 		= string.find(string.lower(Translate("mstore_goldcollection")), searchStr, 1, true)
		result.searchLimited 	= string.find(string.lower(Translate("mstore_limitededition")), searchStr, 1, true)
		result.searchHoliday 	= string.find(string.lower(Translate("mstore_holidayedition")),	searchStr, 1, true)
		result.searchUlt 		= string.find(string.lower(Translate("mstore_ultimateedition")),searchStr, 1, true)
		-- we are gonna use the image table, we don't have another way of knowing how many CE sets there are
		for i=1, #collectorsEditionSetImages do
			ceSetSearchTable[i] = string.find(string.lower(Translate("mstore_collectorsEditionSet"..i)), searchStr, 1, true)
		end
	end
	result.ceSetSearchTable = ceSetSearchTable
	return result
end

local function GetAvatarIconByIndex(idx)

	local ar = split(HoN_Store.ProductAvatars.Codes[idx], '%.')
	local heroName = ar[2] or ''
	local avatarName = ar[3] or ''

	local image = GetAvatarIconFromDB(heroName, heroName..'.'..avatarName)
	return image
end

local function MatchAvatarSearchStr(searchStr, option, idx)

	if searchStr ~= '' then
		local avatarName = GetAvatarName(idx)
		if avatarName:lower():find(searchStr, 1, true) then
			return true
		end

		local heroName = GetHeroDisplayNameByAvatarIndex(idx)
		if heroName:lower():find(searchStr, 1, true) then
			return true
		end
	end

	local lists = HoN_Store.ProductAvatars
	local productID = lists.Ids[idx]
	local heroEntry = HoN_Store.allAvatarsInfo[tonumber(productID)]
	if heroEntry then
		if (option.searchGold and heroEntry.goldCollection) or
			(option.searchLimited and heroEntry.limitedEdition) or
			(option.searchHoliday and heroEntry.holidayEdition) or
			(option.searchUlt and heroEntry.ultimate) or
			(tonumber(heroEntry.collectorsSet) and option.ceSetSearchTable[tonumber(heroEntry.collectorsSet)])
			then
			return true
		end

		if option.searchEnhancement and heroEntry.hasEnhancement then
			return true
		end

		if option.searchTiral then
			if lists.Owned[idx] == '0' then
				local code = lists.Codes[idx]
				local arr = split(code, '%.')

				local hero, alt = arr[2], arr[3]
				if hero and alt then
					local trialInfo = GetTrialInfo(hero, alt)
					if NotEmpty(trialInfo) then
						return true
					end
				end
			end
		end
	end
	-- @@ testonly
	if not GetCvarBool('releaseStage_stable') then
		local ids = lists.Ids
		local index = string.match(searchStr, 'index=(%d+)')
		if index ~= nil then
			index = tonumber(index)
			if index > 0 and index <= #ids then
				return index == idx
			end
		end
		local id = string.match(searchStr, 'id=(%d+)')
		if id ~= nil then
			return productID == id
		end
		if searchStr == 'icon=' then
			local image = GetAvatarIconByIndex(idx)
			if not Testing.FileExists(image) then
				Echo('^pidx='..idx..', icon not exist: '..image)
				return true
			end
		end
		if searchStr == 'trial' and heroEntry then
			if lists.Owned[idx] == '0' then
				local code = lists.Codes[idx]
				local arr = split(code, '%.')

				local hero, alt = arr[2], arr[3]
				if hero and alt then
					local trialInfo = GetTrialInfo(hero, alt)
					if NotEmpty(trialInfo) then
						return true
					end
				end
			end
		end
	end
	return false
end

local function FilterAvatars()
	local lists = HoN_Store.ProductAvatars
	local owned = lists.Owned
	local names = lists.Names
	local ids 	= lists.Ids

	local owndedCvar, ownedType = GetAvatarOwnedCvarName()
	local filterOwnedOption = GetCvarNumber(owndedCvar)
	local filterOwned = HoN_Store.comboItems.avatars[ownedType][filterOwnedOption].filter

	local searchStr = GetWidget('store2_searchbox_avatars_textbox'):GetValue()
	searchStr = ParseSearchStr(searchStr)

	local overrideSearchOpt = lists.OverrideSearchOpt
	local searchOpt = overrideSearchOpt or GetAvatarSetSearchOption(searchStr)
	lists.OverrideSearchOpt = nil

	-- filter with owned
	local src = lists.SortedIndexes
	local result = {}
	for i, idx in ipairs(src) do
		if not filterOwned or filterOwned(idx) then
			table.insert(result, idx)
		end
	end
	lists.PreFiltedIndexes = result

	-- filter with search str
	if searchStr ~= '' or overrideSearchOpt ~= nil then
		src = result
		result = {}
		for i, idx in ipairs(src) do
			if MatchAvatarSearchStr(searchStr, searchOpt, idx) then
				table.insert(result, idx)
			end
		end
	end

	lists.FilterString	= searchStr
	lists.FiltedIndexes = result

	Store2ModelContextManager.PrepareNewGeneration('avatarList')
end

function HoN_Store:ResetAvatarListFilters()

	Set('store2_combobox_avatars_sortOrder_value', 1)
	Set(GetAvatarOwnedCvarName(), 1)

	GetWidget('store2_searchbox_avatars_textbox'):UICmd('EraseInputLine')

	HoN_Store.paging.avatars.currentPage = 1
end

local function AvatarListFiltersChanged()
	if HoN_Store.paging.avatars.currentPage ~= 1 then return true end
	if GetWidget('store2_searchbox_avatars_textbox'):GetValue() ~= '' then return true end
	if GetCvarNumber(GetAvatarOwnedCvarName()) ~= 1 then return true end
	if GetComboBoxIndex('avatars', 'sortOrder') ~= 1 then return true end
	return false
end

function HoN_Store:SwitchToAvatarListPage(lazy)

	local lists = HoN_Store.ProductAvatars
	local filtersChanged = AvatarListFiltersChanged()
	if GetStore2CurrentTab() == 'altavatars'
		and not HoN_Store.HeroDetailPage.IsShowing
		and not filtersChanged 
		and lists.FiltedIndexes ~= nil and #lists.FiltedIndexes == #lists.Ids then
		return
	end

	HoN_Store:Store2Loading(1, 'SwitchToAvatarListPage')
	local func = lazy and HoN_Store.RequestHeroAvatarDataOnNil or HoN_Store.RequestHeroAvatarData

	func(HoN_Store, 'avatars', function(success, newData, ...)
			if not success then
				Echo('^r request avatars failed')
				return
			end
			HoN_Store:ResetAvatarListFilters()
			if newData then
				HoN_Store:InitializeAvatarList(...)
			else
				SortAvatars()
				FilterAvatars()
			end
			HoN_Store:ShowAvatarsPage(HoN_Store.paging.avatars.currentPage)
			Store2Tab('altavatars')
		end)
end

HoN_Store.RequestResultHandlers.avatars.default = function(success, newData, ...)
	if not success then
		Echo('^r avatars.default failed')
	else
		HoN_Store:InitializeAvatarList(...)
		HoN_Store:OnHeroAvatarListDataUpdated(HoN_Store.RequestParam.avatars.cat)
	end
end

function HoN_Store:ShowAvatarsPage(page)

	local function InitAvatarsPanel(indexes, page)
		HoN_Store:Store2Loading(1, "Paging")

		local productCount = #indexes
		local countPerPage = HoN_Store.paging.avatars.countPerPage
		local maxPage = (productCount - 1) / countPerPage  + 1
		maxPage = math.floor(maxPage)

		page = math.min(page, maxPage)
		page = math.max(page, 1)

		local startIndex = HoN_Store.paging.StartIndex(page, 'avatars')
		local lastIndex = HoN_Store.paging.LastIndex(page, 'avatars')
		local endIndex = math.min(productCount, lastIndex)

		local widgetCount = 10
		local parent = GetWidget('store2_avatars_list')

		-- context issue
		local contextCategory = 'avatarList'
		local sm = Store2ModelContextManager
		sm.PrepareNewPage(contextCategory, page)

		HoN_Store:Store2PendingJobsClear()
		for i = 1, widgetCount do
			local prefix = 'store2_avatars_middle_frame_avatar_'..i..'_'
			local modelCover = parent:GetWidget(prefix..'blank_cover')
			modelCover:SetVisible(1)
			HoN_Store:Store2PendingJobsPush(
				function()
					local panel = parent:GetWidget(prefix..'panel')
					local j = startIndex + i - 1
					local visible = j <= endIndex
					panel:SetVisible(visible)

					local testIconImage = GetWidget(prefix..'test_icon')
					testIconImage:SetVisible(false)
					if visible then
						local index = indexes[j]
						SetupAltAvatarPanel(panel, prefix, index, 'middle', contextCategory)

						if not GetCvarBool('releaseStage_stable') and GetCvarBool('store2_test_avatar_icons') then
							local lists = HoN_Store.ProductAvatars
							local image = GetAvatarIconByIndex(index)
							Echo('^pindex='..index
								..', id='..tostring(lists.Ids[index])
								..', code='..tostring(lists.Codes[index])
								..', image='..tostring(image)
								)
							testIconImage:SetTexture(image or '')
							testIconImage:SetVisible(true)
						end
					else
						HoN_Store.widgetToHeroAvatarStrMap[panel:GetWidget(prefix..'model')] = nil
					end

					modelCover:FadeOut(GetCvarInt('store2ModelCoverFadeTime'))
					return true
				end
			)
		end

		HoN_Store:Store2PendingJobsPush(
			function()
				sm.RecyclePagesInCase(contextCategory)
				return true
			end
		)

		HoN_Store:Store2LoadingEndAsync('PagingDone')

		-- set page index
		UpdatePaging('avatars', page, productCount, 11)
	end

	interface:GetWidget('store2_avatars_list'):SetVisible(true)
	InitAvatarsPanel(HoN_Store.ProductAvatars.FiltedIndexes, page)
	HoN_Store.ProductAvatars.PageEverShown = true
end

function HoN_Store:InitializeAvatarList(...)

	local avatarLists = HoN_Store.ProductAvatars

	local argOffset				= 2
	avatarLists.Ids				= split(tostring(arg[argOffset + 2]) or '', '|')
	avatarLists.GoldPrice 		= split(tostring(arg[argOffset + 4]) or '', '|')
	avatarLists.Owned 			= split(tostring(arg[argOffset + 5]) or '', '|')
	avatarLists.Codes 			= split(tostring(arg[argOffset + 20]) or '', '|')
	avatarLists.SilverPrice 	= split(tostring(arg[argOffset + 35]) or '', '|')
	avatarLists.Purchasable 	= split(tostring(arg[argOffset + 50]) or '', '|')

	local productEnhancements	= arg[argOffset + 76]
	local productEnhancementIDs	= arg[argOffset + 77]
	avatarLists.Enhancements	= HoN_Store:SetupProductEnhancements(avatarLists.Ids, productEnhancements, productEnhancementIDs)

	local isBundle				= split(tostring(arg[argOffset + 6]) or '', '|')

	local count = #avatarLists.Codes
	local j = 1
	for i = 1, count do
		local code = avatarLists.Codes[j]
		if isBundle[i] == '1' or not MatchesAvatarCode(code) then
			table.remove(avatarLists.Ids, j)
			table.remove(avatarLists.GoldPrice, j)
			table.remove(avatarLists.Owned, j)
			table.remove(avatarLists.Codes, j)
			table.remove(avatarLists.SilverPrice, j)
			table.remove(avatarLists.Purchasable, j)
		else
			j = j + 1
		end
	end

	-- Eligibility and Special discount
	local productEligibility	= arg[argOffset + 59]
	avatarLists.Eligibility 	= HoN_Store:AdjustProductPrices('Alt Avatar',
		avatarLists.Ids, avatarLists.GoldPrice, avatarLists.SilverPrice, productEligibility, true)

	avatarLists.Count = j - 1
	if #avatarLists.SortedIndexes ~= avatarLists.Count then
		avatarLists.SortedIndexes = CreateIntArray(1, avatarLists.Count)
	end

	EnsureAvatarInfoInited()

	SortAvatars()
	FilterAvatars()

	avatarLists.InitListTime = GetTime()

	return true
end

function HoN_Store:ShowAvatarDetailPage(rawIndex)
	HoN_Store:Store2Loading(1, 'ShowAvatarDetailPage')
	HoN_Store.HeroDetailPage.AvatarIndexToInit = rawIndex

	HoN_Store:RequestHeroAvatarDataOnNil('heroes', function(success, newData, ...)
			if not success then
				Echo('^r request heroes failed')
				return
			end
			if newData then
				HoN_Store:InitializeHeroesList(...)
			end
			HoN_Store:InitHeroAvatarPanel()
			HoN_Store:SwitchToHeroAvatarPanel(true)
		end)
end

function HoN_Store:OnClickAvatarFrame(w, i, dataType)

	dataType = dataType or 'middle'

	local page 			= HoN_Store.paging.avatars.currentPage
	local filtedIndex 	= (page - 1) * HoN_Store.paging.avatars.countPerPage + i
	local rawIndex 		= HoN_Store.ProductAvatars.FiltedIndexes[filtedIndex]

	if dataType == 'middle' then
		HoN_Store:ShowAvatarDetailPage(rawIndex)
		HoN_Store.HeroAvatarBackToListCallback = HoN_Store:CreateDefaultHeroAvatarBackToListCallback(true)
	end
end

local searchingAvatarStr	= ''
local searchingAvatarList 	= {}

local function SwitchToHeroAvatarPanelFromSearchingAvatar(filtedIndex)

	local lists = HoN_Store.ProductAvatars
	local rawIndex = lists.FiltedIndexes[filtedIndex]
	HoN_Store:ShowAvatarDetailPage(rawIndex)

	GetWidget('store2_searchbox_avatars_listPanel'):SetVisible(false)

	HoN_Store.HeroAvatarBackToListCallback = function()
			GetWidget('store2_detail_heroAvatars'):SetVisible(false)
			GetWidget('store2_altavatars'):SetVisible(true)
			HoN_Store:ShowAvatarsPage(1)
		end
end

function HoN_Store:OnSearchAvatar(str)

	UpdateSearchList(str, HoN_Store.ProductAvatars, searchingAvatarList)

	local lists = HoN_Store.ProductAvatars
	if #lists.FiltedIndexes == 1 then
		SwitchToHeroAvatarPanelFromSearchingAvatar(1)
	else
		HoN_Store:ShowAvatarsPage(1)
	end

	GetWidget('store2_searchbox_avatars_listPanel'):SetVisible(false)
end

function HoN_Store:OnSearchingAvatar(w, searchStr)

	local products 	= HoN_Store.ProductAvatars

	local matchedIds = {}
	local displayCount = 5

	searchingAvatarStr = ParseSearchStr(searchStr)
	if searchingAvatarStr ~= '' then
		local src 		= products.PreFiltedIndexes
		local ids 		= products.Ids

		local searchOpt = GetAvatarSetSearchOption(searchingAvatarStr)
		matchedIds = SearchingHeroAvatar(src, ids, -1,
			function(idx, id, name)
				return MatchAvatarSearchStr(searchingAvatarStr, searchOpt, idx)
			end)
	end

	local prefix	= 'store2_searchbox_avatars_'
	local panel		= GetWidget(prefix..'listPanel')
	local list		= GetWidget(prefix..'list')
	local count		= #matchedIds

	local function AddListItem(list, data, i, isLast)
		local template = 'store2_avatars_list_search_item'
		local prefix   = 'store2_avatars_list_search_item'..i
		list:Instantiate(template,
			'i',		i,
			'y',		((i - 1) * 50)..'i',
			'prefix',	prefix,
			'texture', 	data.icon,
			'texture2', data.icon2,
			'label',	data.name,
			'label2',	data.name2,
			'showline',	isLast and 'false' or 'true')
	end

	local function AddHasMoreItem(list, count)
		local template = 'store2_heroes_list_search_item_end'
		local text 	= ''
		local height='10i'
		if count >= displayCount then
			text = Translate('store2_search_result_count', 'count', count)
			height = '32i'
		end
		list:Instantiate(template, 'text', text, 'cat', 'avatars', 'height', height)
	end

	if count == 0 then
		panel:SetVisible(false)
	else
		local codes = products.Codes

		local lastIdnex = math.min(count, displayCount)
		local hasMore = count > displayCount

		list:ClearChildren()
		for i, v in ipairs(matchedIds) do

			local ar = split(HoN_Store.ProductAvatars.Codes[v.index], '%.')
			local heroName = ar[2] or ''
			local avatarName = ar[3] or ''
			v.icon 	= GetAvatarIconFromDB(heroName, heroName..'.'..avatarName)
			v.icon2 = GetHeroIconFromDB(heroName)
			v.name2 = GetDisplayName(heroName)

			AddListItem(list, v, i, i == lastIdnex and not hasMore)

			if i == displayCount then break end
		end
		AddHasMoreItem(list, count)
		list:RecalculateSize()
		panel:SetVisible(true)
	end

	searchingAvatarList = matchedIds
end

function HoN_Store:OnClickSearchingAvatarItem(i)
	UpdateSearchList(searchingAvatarStr, HoN_Store.ProductAvatars, searchingAvatarList)
	SwitchToHeroAvatarPanelFromSearchingAvatar(i)
end

function HoN_Store:OnAvatarConditionChange(id)

	local avatarLists = HoN_Store.ProductAvatars
	
	if not HoN_Store.allAvatarsInfo or avatarLists.Count == 0 then return end
	if avatarLists.InitListTime == GetTime() then return end

	if id == 'sortOrder' then
		SortAvatars()
	end
	FilterAvatars(id)
	HoN_Store:ShowAvatarsPage(HoN_Store.paging.avatars.currentPage)
end

function HoN_Store:GetHeroEntryForAvatarCategoryIcon(cat, id)
	if cat == 'avatar' then cat = 'avatars' end

	local pageInfo = HoN_Store.paging[cat]
	local index = pageInfo.countPerPage * (pageInfo.currentPage - 1) + tonumber(id)

	local avatarsLists 	= HoN_Store.ProductAvatars
	local avatarIndex, productID
	if cat == 'avatars' then
		avatarIndex 	= avatarsLists.FiltedIndexes[index]
	elseif cat == 'heroAvatars' then
		avatarIndex = HoN_Store.HeroDetailPage.CurrentHeroAvatars[index]
	elseif cat == HoN_Store.BundleDetail.Category then
		local infoList 	= HoN_Store.ProductPageInfo[cat]
		productID = infoList.productIDs[index]
	else
		Echo('^r cat='..tostring(cat))
		return nil
	end

	if avatarIndex then
		productID 	= avatarsLists.Ids[avatarIndex]
	end

	if not productID then
		Echo('^r nil productID, cat='..tostring(cat))
		return nil
	end

	local heroEntry = HoN_Store.allAvatarsInfo[tonumber(productID)]
	if heroEntry == nil then 
		Echo('^r nil heroEntry for productID='..tostring(productID))
		return 
	end

	if productID ~= tostring(heroEntry.productID) then
		Echo('^r error productID='..productID..', heroEntry.productID='..heroEntry.productID)
	end

	return heroEntry, avatarIndex, productID
end

function HoN_Store:OnHoverAvatarCategoryIcon(cat, id, imgType)

	local name = nil
	local desc = nil
	local translate = true

	if imgType == 'enhancement' then		-- altAvatar.hasEnhancement
		name = 'mstore_enhancement'
		desc = 'mstore_enhancement_tip'
	elseif imgType == 'ultimate' then		-- altAvatar.ultimate
		name = 'mstore_ultimateedition'
		desc = 'mstore_ultimateedition_tip'
	end

	if name == nil then

		local avatarsLists 	= HoN_Store.ProductAvatars

		local heroEntry, avatarIndex, productID = HoN_Store:GetHeroEntryForAvatarCategoryIcon(cat, id)
		if not heroEntry then
			return
		end

		local addOwnedAndPurchasable = false
		if imgType == 'trial' then
			local code = avatarsLists.Codes[avatarIndex]
			local ar = split(code, '%.')
			local hero, altCode = ar[2], ar[3]
			local trialInfo = GetTrialInfo(hero, altCode)
			if (NotEmpty(trialInfo)) then
				name = Translate('learn_trial')
				desc = Translate('compendium_trial_infohead') .. trialInfo .. Translate('compendium_trial_infotail')
				translate = false
			end
		elseif imgType == 'ce' then				-- altAvatar.collectorsSet
			if (heroEntry.collectorsSet > 0) then
				name = "mstore_collectorsEditionSet"..tostring(heroEntry.collectorsSet)
				local enhancementMap = HoN_Store.ProductAvatars.Enhancements
				local map = enhancementMap.map
				local idmap = enhancementMap.idmap
				local mapKey = tonumber(productID)
				if heroEntry.collectorsSet == 39 and idmap[mapKey] then
					local productString = ''
		        	for t, tproductID in ipairs(idmap[mapKey]) do
		                if string.len(productString) > 0 then
		                    productString = productString .. '\n'
		                end

		                local productPrefix = ''
		                if map and map[mapKey] and map[mapKey][t] then
			                if Client.IsProductOwned('en.' .. map[mapKey][t]) then
			                	productPrefix = '^g'
			                end
			            else
			            	Echo('^rerror: map='..tostring(map)..', mapKey='..tostring(mapKey)..', t='..tostring(t))
			            end
		                productString = productString .. productPrefix .. Translate('mstore_product'..tproductID..'_name') .. '^*'
		    		end
		            name = 'mstore_collectorsEditionSet39'
		            desc = 'mstore_collectorsEditionSet39_tip'
		            HoN_Store:ShowToolTip2(name, desc, 'mstore_collectorsEditionSet39_desc', productString)

	                return
				else
					addOwnedAndPurchasable = true
				end
			end
		elseif imgType == 'special' then

			if heroEntry.limitedEdition then
				name = "mstore_limitededition"
			elseif heroEntry.goldCollection then
				name = "mstore_goldcollection"
			elseif heroEntry.holidayEdition then
				name = "mstore_holidayedition"
			end

			addOwnedAndPurchasable = true
		end

		if addOwnedAndPurchasable and name then
			if avatarsLists.Owned[avatarIndex] == '1' then name = name.."_owned"
			elseif avatarsLists.Purchasable[avatarIndex] == '0' then name = name.."_expired" end

			desc = name.."_tip"
			HoN_Store:ShowToolTip2(name, desc, '', '')
			return
		end
	end
	if name ~= nil then
		HoN_Store:ShowToolTip(name, desc or '', translate)
	end
end

function HoN_Store:OnClickAvatarCategoryIcon(cat, id, imgType)

	local searchOpt = {ceSetSearchTable = {}}
	if imgType == 'enhancement' then		-- altAvatar.hasEnhancement
		searchOpt.searchEnhancement = true
	elseif imgType == 'ultimate' then		-- altAvatar.ultimate
		searchOpt.searchUlt = true
	elseif imgType == 'trial' then
		searchOpt.searchTiral = true
	else
		
		local heroEntry = HoN_Store:GetHeroEntryForAvatarCategoryIcon(cat, id)
		if not heroEntry then
			return
		end

		if imgType == 'ce' then				-- altAvatar.collectorsSet
			if (heroEntry.collectorsSet > 0) then
				searchOpt.ceSetSearchTable = {[heroEntry.collectorsSet] = true}
			end
		elseif imgType == 'special' then

			if heroEntry.limitedEdition then
				searchOpt.searchLimited = true

			elseif heroEntry.goldCollection then
				searchOpt.searchGold = true

			elseif heroEntry.holidayEdition then
				searchOpt.searchHoliday = true

			end
		end
	end

	local avatarLists = HoN_Store.ProductAvatars
	avatarLists.OverrideSearchOpt = searchOpt
	avatarLists.InitListTime = GetTime()	-- to avoid reset list on first showing up
	if HoN_Store:Store2GetCurrentPage() ~= 'store2_avatar_list' then
		HoN_Store:SwitchToAvatarListPage(true)
	else
		GetWidget('store2_searchbox_avatars_textbox'):SetInputLine('')
		FilterAvatars()
		HoN_Store:ShowAvatarsPage(1)
	end
end

function HoN_Store:OnHoverOutAvatarCategoryIcon()
	HoN_Store:HideToolTip()
end

function HoN_Store:OnHideAvatarListPage()
	local order	= GetComboBoxIndex('avatars', 'sortOrder')
	local owned	= GetCvarNumber(GetAvatarOwnedCvarName())
	local search= HoN_Store.ProductAvatars.FilterString
	local page 	= HoN_Store.paging.avatars.currentPage
	HoN_Store:NavHistoryPush2('avatar list',function()
		if HoN_Store:IsVaultOpened() then
			HoN_Store:Store2OpenShop(false)
		end
		HoN_Store:HideDebugPanel()
		Store2Tab('altavatars')

		Set('store2_combobox_avatars_sortOrder_value', order)
		Set(GetAvatarOwnedCvarName(), owned)
		GetWidget('store2_searchbox_avatars_textbox'):SetInputLine(search)
		GetWidget('store2_searchbox_avatars_listPanel'):SetVisible(false)

		SortAvatars()
		FilterAvatars()

		HoN_Store:ShowAvatarsPage(page)
	end)
end

function HoN_Store:ClearAvatarData()
	local avatarLists = HoN_Store.ProductAvatars

	avatarLists.Ids				= {}
	avatarLists.GoldPrice 		= {}
	avatarLists.Owned 			= {}
	avatarLists.Codes 			= {}
	avatarLists.SilverPrice 	= {}
	avatarLists.Purchasable 	= {}

	avatarLists.Enhancements	= {}

	avatarLists.Eligibility 	= {}

	avatarLists.Count 			= 0
end

-- bottom bar
function HoN_Store:InitBottomBarText(parent, font)
	local text = Translate('store2_footer_elements_texts')
	local id = 0
	local totalLen = 0
	for a,b in string.gmatch(text,'([%a%p%s]-){([^{}]+)}') do
	  if a and a ~= '' then
	  	parent:Instantiate('shopkeeper_label_normal', 'text', a, 'id', id, 'font', font)
	  	local len = GetStringWidth(font, a)
	  	parent:GetWidget('shopkeeper_label_'..id):SetWidth(len)
	  	totalLen = totalLen + len
	  	id = id + 1
	  end
	  if b and b ~= '' then
	  	parent:Instantiate('shopkeeper_label_highlight','text',b, 'id', id, 'font', font)
	  	local len = GetStringWidth(font, b)
	  	parent:GetWidget('shopkeeper_label_'..id):SetWidth(len)
	  	totalLen = totalLen + len
	  	id = id + 1
	  end
	end
	local c = string.match(text,'[%a%p%s]+}([%a%p%s]+)$')
	if c and c ~= '' then
	  	parent:Instantiate('shopkeeper_label_normal', 'text', c, 'id', id, 'font', font)
	  	local len = GetStringWidth(font, c)
	  	parent:GetWidget('shopkeeper_label_'..id):SetWidth(len)
	  	totalLen = totalLen + len
	  	id = id + 1
	end
	parent:SetWidth(totalLen)
end

-- bundle detail
local productTypeIcons = {
	['Bundle']				= '/ui/fe2/elements/store2/bundle/type_other.png',
	['Chat Color']			= '/ui/fe2/elements/store2/bundle/type_color.png',
	['Chat Symbol']			= '/ui/fe2/elements/store2/bundle/type_symbol.png',
	['Account Icon']		= '/ui/fe2/elements/store2/bundle/type_icon.png',
	['Alt Avatar']			= '/ui/fe2/elements/store2/bundle/type_avatar.png',
	['Alt Announcement']	= '/ui/fe2/elements/store2/bundle/type_announcment.png',
	['Ability']				= '/ui/fe2/elements/store2/bundle/type_other.png',
	['Taunt']				= '/ui/fe2/elements/store2/bundle/type_taunt.png',
	['Misc']				= '/ui/fe2/elements/store2/bundle/type_other.png',
	['Couriers']			= '/ui/fe2/elements/store2/bundle/type_couriers.png',
	['Hero']				= '/ui/fe2/elements/store2/bundle/type_hero.png',
	['EAP']					= '/ui/fe2/elements/store2/bundle/type_other.png',
	['Status']				= '/ui/fe2/elements/store2/bundle/type_other.png',
	['Multiplier']			= '/ui/fe2/elements/store2/bundle/type_other.png',
	['Ward']				= '/ui/fe2/elements/store2/bundle/type_ward.png',
	['Enhancement']			= '/ui/fe2/elements/store2/bundle/type_other.png',
	['Extention']			= '/ui/fe2/elements/store2/bundle/type_other.png',
	['Coupon']				= '/ui/fe2/elements/store2/bundle/type_other.png',
	['Mastery']				= '/ui/fe2/elements/store2/bundle/type_other.png',
	['Creep']				= '/ui/fe2/elements/store2/bundle/type_creep.png',
	['Building']			= '/ui/fe2/elements/store2/bundle/type_building.png',
}

HoN_Store.BundleDetail =
{
	Category		= 'bundleDetail',
	BoothID			= 'vanity',

	fromSpecials	= false,

	BundlePage		= 0,
	BundleRow		= 0,
	BundleCol		= 0,
	BundleIndex		= 0,

	BundleId 		= 0,
	BundleCost		= 0,
	BundleSilverCost= 0,
	BundleOwned		= false,
	BundleEligiblity= nil,
	Theme			= '',
	Icon			= '',
	Model 			='',

	IsLuckyBundle	= false,
	LuckyItemCount	= 0,
	LuckyChanceCount= 0,

	IsTypeList		= false,

	IsDiscountBundle= false,
	DiscountSaves	= 0,
	DiscountPercent	= 0,

	SelectIndex 	= -1,
	SelectIndexPage = 0,
	TotalCost		= 0,

	BundleRowToInit = nil,
	BundleColToInit = nil,
	PageToInit		= nil,
	IndexToInit		= nil,

	IsInitializing	= false,
	IsShowing		= false,

	CallingShowBundleDetailPage = false,
}

-- @@ todo diffrent chest model 
function HoN_Store:GetBundleChestModelForIconPath(path, theme, bundleType, model)
	-- TestEcho('HoN_Store:GetBundleChestModelForIconPath path:'..path..' theme: '..theme..' bundleType:'..bundleType..' model:'..model)
	if not theme and not bundleType and path then 
		theme, bundleType = string.match(path, '[^/]+/bundle_([^_]+)_(%a)%.png')
	end
	if theme and bundleType then
		local result = {}
		local isGrabbag = bundleType == 'l' or bundleType == 'lucky'
		if theme then
			if isGrabbag then
				result.folder 	= 'lucky_bundle'
				result.ambientR	= 0.72
				result.ambientG	= 0.54
				result.ambientB	= 0.73
				result.sunR		= 0.55
				result.sunG		= 0.61
				result.sunB		= 0.53
			else
				result.folder 	= 'discount_bundle'
				result.ambientR	= 0.87
				result.ambientG	= 0.78
				result.ambientB	= 0.89
				result.sunR		= 0.71
				result.sunG		= 0.96
				result.sunB		= 0.96
			end
		end

		result.model 	= model and model or '/ui/fe2/grabbag/'..result.folder..'/footlocker.mdf'
		result.effect 	= '/ui/fe2/grabbag/'..result.folder..'/hover_effect.effect'
		return result
	else
		return nil
	end
end

function HoN_Store:SwitchToBundleListPage(page, fromSpecials, forceRequest)
	local cat = 'bundles'

	local page = page or HoN_Store.ProductPageInfo[cat].currentPage

	local request = false
	if forceRequest then 
		request = true
	elseif GetStore2CurrentTab() ~= cat then
		request = true
	elseif GetWidget('store2_detail_bundleDetail'):IsVisible() and HoN_Store.BundleDetail.fromSpecials then
		request = true
	elseif fromSpecials then
		request = true
	else
		request = page ~= HoN_Store.ProductPageInfo[cat].currentPage
	end

	GetWidget('store2_detail_bundleDetail'):SetVisible(false)
	GetWidget('store2_bundle_list'):SetVisible(not fromSpecials)
	Store2Tab(cat)

	if request then
		HoN_Store:ClearVanityPage(cat)
		HoN_Store:Store2FormGetProducts(cat, page)
	end
end

local function RequestBundleContent(bundleId)
	SetRequestStatusWatch('MicroStoreBundleContentStatus')
	SubmitForm('StoreBundleContent',
		'account_id', Client.GetAccountID(),
		'f', 'get_bundle_contents',
		'bundle_id', bundleId,
		'cookie', Client.GetCookie())
end

local function FindBundleIndexForId(bundleId)

	local pageInfo 		= HoN_Store.ProductPageInfo.bundles

	if pageInfo.productIDs then 
		local bundleIndex	= 1

		for row = 1, pageInfo.rowsPerPage do
			for col = 1, pageInfo.elementPerRow do
				local productID = pageInfo.productIDs[bundleIndex]
				if bundleId == productID then
					return bundleIndex, row, col
				end
				bundleIndex = bundleIndex + 1
			end
		end
	end
	return nil 
end

interface:RegisterWatch('MicroStoreBundleContentResult', function(self, responseString)
	TestEcho('^rMicroStoreBundleContentResult:'..tostring(responseString))

	if not lib_json.RoughCheckJsonString(responseString) then
		Echo('^rInvalid MicroStoreBundleContentResult!!!')
		HoN_Store:Store2LoadingEndAsync('SwitchToBundleDetailPageDone')
		return
	end

	local json = lib_json.decode(responseString)
	local info				= json.info

	local detail 			= HoN_Store.BundleDetail
	detail.IsInitializing	= true
	detail.IsLuckyBundle	= info.grab_bag
	detail.LuckyItemCount	= info.prize_quantity or 0
	detail.LuckyChanceCount	= info.max_purchasable or 0
	detail.BundleCost 		= tonumber(info.gold_coins or 0)
	detail.BundleSilverCost = tonumber(info.silver_coins or 0)
	detail.BundleOwned		= info.is_owned or false
	detail.Theme			= info.ui_theme or 'default'
	detail.Model 			= info.model or ''

	if info.bundle_id ~= detail.BundleId then
		TestEcho('^rbundleId mismatch, req='..detail.BundleId..'/'..type(detail.BundleId)..', res='..info.bundle_id..'/'..type(info.bundle_id))
		detail.BundleId = info.bundle_id
	else
		-- update price based on bundle list
		local bundleIndex = FindBundleIndexForId(detail.BundleId)
		if bundleIndex then
			local bundleList = HoN_Store.ProductPageInfo['bundles']
			detail.BundleCost = bundleList.productPrices[bundleIndex]
			detail.BundleSilverCost = bundleList.premiumMmpCost[bundleIndex]
		else
			local specialPrices, count = HoN_Store:GetDailySpecialPrices('Bundle')
			if count > 0 then
				local specialItem = specialPrices[detail.BundleId]
				local validSpecialItem = specialItem ~= nil and specialItem.gold ~= nil and specialItem.gold ~= ''
				if validSpecialItem then
					detail.BundleCost = specialItem.gold
					detail.BundleSilverCost = specialItem.silver
				end
			end
		end
	end

	local infoList 			= HoN_Store.ProductPageInfo[detail.Category]

	infoList.productIDs   	= {}
	infoList.productCodes 	= {}
	infoList.productPrices 	= {}
	infoList.premiumMmpCost = {}
	infoList.owned 			= {}
	infoList.images 		= {}
	detail.TotalCost		= 0

	if json.list ~= nil then
		for i, v in ipairs(json.list) do
			table.insert(infoList.productIDs,   v.product_id)
			table.insert(infoList.productCodes,	v.item_code)

			local price = v.gold_coins or 0
			table.insert(infoList.productPrices,price)
			table.insert(infoList.images, 		v.local_content)

			table.insert(infoList.owned, 		v.is_owned and '1' or '0')
			table.insert(infoList.premiumMmpCost, v.silver_coins)

			detail.TotalCost = detail.TotalCost + tonumber(price)
		end
		detail.IsTypeList = false

		if detail.fromSpecials then
			if detail.BundleCost and detail.TotalCost then
				detail.DiscountPercent = round((detail.TotalCost - detail.BundleCost) / detail.TotalCost * 100)
			end
		end
	elseif json.type_list ~= nil then
		for i, v in ipairs(json.type_list) do
			table.insert(infoList.productIDs,   '0')
			table.insert(infoList.productCodes,	v)

			table.insert(infoList.productPrices,-1)
			table.insert(infoList.images, 		productTypeIcons[v] or '')

			table.insert(infoList.owned, 		'0')
			table.insert(infoList.premiumMmpCost, -1)
		end
		detail.IsTypeList = true
	else
		Echo('^rnil list/type list for bundle contents')
	end

	detail.Count = #infoList.productIDs

	detail.IsDiscountBundle = (not detail.IsLuckyBundle) and (detail.DiscountPercent > 0)

	if detail.IsDiscountBundle then
		if detail.IsTypeList then
			detail.DiscountSaves	= 0
		else
			detail.DiscountSaves 	= math.max(0, detail.TotalCost - detail.BundleCost)
		end
	end

	-- disable nav history pushing
	local tmp = Store2NavBackInProgress
	if detail.fromSpecials then
		Store2NavBackInProgress = true
	end

	-- ui
	EnsureAvatarInfoInited()
	HoN_Store:InitBundleDetailPage(detail.BundleId)

	local page 	= detail.PageToInit or 1
	local index = detail.IndexToInit or 0

	HoN_Store:ShowBundleDetailPage(page, index)

	if detail.fromSpecials then
		Store2NavBackInProgress = tmp
	end

	detail.PageToInit	= nil
	detail.IndexToInit	= nil

	detail.IsInitializing	= false

	HoN_Store:Store2LoadingEndAsync('SwitchToBundleDetailPageDone')
end)

function HoN_Store:SwitchToBundleDetailPage(row, col, fromSpecials)

	HoN_Store:Store2Loading(1, 'SwitchToBundleDetailPage')
	local pageInfo 		= HoN_Store.ProductPageInfo.bundles
	local bundleIndex 	= pageInfo.elementPerRow * (row - 1) + col
	local bundleId 		= pageInfo.productIDs[bundleIndex]

	local detail 		= HoN_Store.BundleDetail
	detail.BundlePage 	= pageInfo.currentPage
	detail.BundleRow 	= row
	detail.BundleCol 	= col
	detail.BundleIndex 	= bundleIndex
	detail.BundleId 	= bundleId
	detail.BundleEligiblity= pageInfo.Eligibility
	detail.Icon 		= pageInfo.images[bundleIndex] or ''
	detail.fromSpecials	= fromSpecials or false

	detail.charges		= pageInfo.productCharges[bundleIndex]
	detail.durations	= pageInfo.productDurations[bundleIndex]
	detail.chargesRemaining = pageInfo.chargesRemaining[bundleIndex]

	local sTypeDiscount = pageInfo.grabBag[bundleIndex]
	sTypeDiscount = split(sTypeDiscount, '~')
	detail.DiscountPercent = tonumber(sTypeDiscount[2]) or 0

	--purchase param
	Set('_microStore_SelectedID', GetCvarString('microStoreID'..bundleIndex))
	Set('_microStore_SelectedItem', bundleIndex)
	Set('_microStoreSelectedHeroItem', '')

	RequestBundleContent(bundleId)
end

function HoN_Store:SwitchToBundleDetailPageFromSpecials(productID, page, icon, fromNavPop)
	if not productID then return end

	HoN_Store.InitializeProductShowCallback = function()

			local fromSpecials 	= not fromNavPop

			local bundleIndex, row, col = FindBundleIndexForId(productID)
			if bundleIndex and row and col then
				HoN_Store:SwitchToBundleDetailPage(row, col, fromSpecials)
			else
				-- case when bundle not in the page, may leads to problems
				local detail 		= HoN_Store.BundleDetail
				detail.BundlePage 	= page
				detail.BundleRow 	= -1
				detail.BundleCol 	= -1
				detail.BundleIndex 	= -1
				detail.BundleId 	= productID
				detail.BundleEligiblity= nil
				detail.Icon 		= icon
				detail.fromSpecials	= true

				detail.charges		= nil
				detail.durations	= nil
				detail.chargesRemaining = nil

				detail.DiscountPercent = -1

				RequestBundleContent(productID)
			end
			
			return {noEndLoading = true}
		end

	HoN_Store:SwitchToBundleListPage(page, true)
end

function HoN_Store:OnBundlesBackToList()
	local detail 		= HoN_Store.BundleDetail
	if detail.fromSpecials then
		local peek = HoN_Store:NavHistoryPeek()
		if peek and peek.name == 'DailySpecials' then
			HoN_Store:NavigateBack()
		else
			HoN_Store:SwitchToBundleListPage()
		end
	else
		HoN_Store:SwitchToBundleListPage()
	end
end

function HoN_Store:SetupBundleDetailBoothPanel(itemIndex)

	TestEcho('^r SetupBundleDetailBoothPanel '..', itemIndex='..tostring(itemIndex))

	local detail 	= HoN_Store.BundleDetail
	local cat		= detail.Category
	local infoList 	= HoN_Store.ProductPageInfo[cat]

	local itemPerRow = HoN_Store.ProductPageInfo[cat].elementPerRow
	local row = math.floor((itemIndex - 1) / itemPerRow) + 1
	local col = itemIndex - itemPerRow * (row - 1)

	local info = {}

	local largeModel = GetWidget('store2_bundle_detail_large_model')
	largeModel:SetVisible(false)

	if itemIndex == 0 then
		info.ShowChest 	= true

		local bundleType
		if detail.IsLuckyBundle then
			bundleType = 'lucky'
			info.name	= Translate('store2_lucky_bundle_title')
			info.desc	= Translate('store2_lucky_bundle_desc', 'chance', detail.LuckyChanceCount, 'count', detail.LuckyItemCount)
		elseif detail.isDiscountBundle then
			bundleType = 'discount'
			info.name	= Translate('store2_discount_bundle_title')
			info.desc	= Translate('store2_discount_bundle_desc')
		else
			bundleType = 'discount'
			info.name	= Translate('store2_normal_bundle_title')
			info.desc	= Translate('store2_normal_bundle_desc')
		end

		info.chestModelSetting = HoN_Store:GetBundleChestModelForIconPath(detail.Icon, detail.Theme, bundleType, detail.Model)
	else
		local code		= infoList.productCodes[itemIndex]
		info			= CheckCodeType(code)

		info.index  	= itemIndex
		info.productID	= infoList.productIDs[itemIndex]

		if info.ishero or info.isavatar then

			local heroAvatarStr = code
			if info.isavatar then
				heroAvatarStr = string.sub(heroAvatarStr, 4)
			end

			local hero, avatar = ParseHeroAvatar(heroAvatarStr)

			local contextCategory = 'detailLargeModel'
			Store2ModelContextManager.SetModelContext(contextCategory, largeModel)

			SetModelPanelByHeroAvatar(largeModel, hero, avatar, 'large')
			HoN_Store.widgetToHeroAvatarStrMap[largeModel] = heroAvatarStr

			largeModel:SetVisible(true)
		end
		info.ShowChest 	= false
		info.istype 	= detail.IsTypeList

		if info.istype then
			info.name		= infoList.productCodes[itemIndex] or ''
			info.desc		= ''
		else
			info.name		= GetProductDispayNameByID(info.productID, '')
			info.desc		= GetProductDisplayDescByID(info.productID, '')
		end
	end

	if detail.IsLuckyBundle then
		info.luckyCount	= detail.LuckyChanceCount
	elseif detail.IsDiscountBundle then
		info.priceLabel = Translate('store2_discount_bundle_save_gold', 'gold', detail.DiscountSaves)
		info.discount	= detail.DiscountPercent
	else
		info.priceLabel = ''
		info.discount	= 0
	end

	info.price 				= detail.BundleCost
	info.silverprice 		= detail.BundleSilverCost
	info.row 				= row
	info.col 				= col
	info.isLuckyBundle		= detail.IsLuckyBundle
	info.isDiscountBundle	= detail.IsDiscountBundle
	info.owned				= detail.BundleOwned

	HoN_Store:InitializeVanityBoothUI(true, cat, info)
end

function HoN_Store:SelectBundleItem(w, i)

	local detail 	= HoN_Store.BundleDetail
	local cat		= detail.Category
	local infoList 	= HoN_Store.ProductPageInfo[cat]

	local function HighlightFrame(w, cat)
		local highlightEffect = GetWidget('store2_detail_highlight_'..cat)
		if w == nil then
			highlightEffect:SetVisible(false)
		else
			highlightEffect:SetParent(w:GetParent())
			highlightEffect:SetVisible(true)
			highlightEffect:SetAnim('')
			highlightEffect:SetAnim('idle')
			highlightEffect:SetEffect('/ui/fe2/elements/store2/selectedeffect/selected_efx.effect')
		end
	end

	local itemIndex = 0
	if i ~= 0 then
		local pageInfo = HoN_Store.paging[cat]
		local offset = (pageInfo.currentPage - 1) * pageInfo.countPerPage
		itemIndex 	 = offset + i
	end
	HoN_Store:SetupBundleDetailBoothPanel(itemIndex)

	HighlightFrame(w, cat)

	detail.SelectPage 	= HoN_Store.paging.bundleDetail.currentPage
	detail.SelectIndex 	= i
end

function HoN_Store:InitBundleDetailPage(bundleId)
	local detail 	= HoN_Store.BundleDetail
	local infoList 	= HoN_Store.ProductPageInfo['bundles']

	local function SetupPortraitInfo(id, icon)

		local prefix = 'store2_detail_'..HoN_Store.BundleDetail.Category..'_'
		local parent = GetWidget(prefix..'info')

		parent:SetVisible(true)

		local name		= id and GetProductDispayNameByID(id) or ''
		local desc		= id and GetProductDisplayDescByID(id) or ''

		if string.match(name, '^mstore_product') then
			name = infoList.productCodes[i]
		end
		if string.match(desc, '^mstore_product') then
			desc = ''
		end

		parent:GetWidget(prefix..'portrait'):SetTexture(icon or '')
		parent:GetWidget(prefix..'name'):SetText(name or '')
		parent:GetWidget(prefix..'desc'):SetText(desc or '')

		-- band
		local bandPrefix = 'store2_bundle_portrait_band_'
		parent:GetWidget(bandPrefix..'lucky'):SetVisible(detail.IsLuckyBundle)
		parent:GetWidget(bandPrefix..'discount'):SetVisible(detail.IsDiscountBundle)
	end

	SetupPortraitInfo(detail.BundleId, detail.Icon)

	local labelText = 'store2_goods'
	if detail.IsLuckyBundle then
		labelText = 'store2_grabbag_content'
	end
	GetWidget('store2_detail_bundleDetail_avatars_label'):SetText(Translate(labelText))

	Store2ModelContextManager.PrepareNewGeneration('bundleDetail')
	Store2ModelContextManager.PrepareNewGeneration('detailLargeModel')
	Store2ModelContextManager.PrepareNewPage('detailLargeModel', 1)
end

function HoN_Store:ShowBundleDetailPage(page, itemIndex, fromPagging)

	local detail = HoN_Store.BundleDetail

	if detail.CallingShowBundleDetailPage then return end
	detail.CallingShowBundleDetailPage = true

	local cat = detail.Category

	local function ClearBundleImageFrame(parent, prefix)
		local aps = {
			'icon_panel', 'icons_panel',
			'model',
			'owned', 'coupon', 'icons_trial_panel'
		}
		for i, v in ipairs(aps) do
			parent:GetWidget(prefix..v):SetVisible(false)
		end
	end

	local function SetupBundleImageFrame(parent, prefix, data)
		local iconPanel = parent:GetWidget(prefix..'icon_panel')
		iconPanel:SetVisible(true)

		iconPanel:GetWidget(prefix..'icon'):SetTexture(data.icon)

		parent:GetWidget(prefix..'owned'):SetVisible(data.owned)

		if detail.IsTypeList then
			SetupGoldSilverPriceLabel(parent, prefix..'gold', false, false, 0, 0, 0)
			SetupGoldSilverPriceLabel(parent, prefix..'silver', false, false, 0, 0, 0)
			local spPriceLabel = parent:GetWidget(prefix..'price')
			spPriceLabel:SetVisible(false)
		else
			SetupHeroAvatarFramePrice(parent, prefix, data, false)
		end

		-- gca
		HoN_Store:ShowGcaImageInNeed(parent, prefix..'gca', data.productCode)
	end

	local function SetupBundleDetailFrame(parent, prefix, index)
		local detail 	= HoN_Store.BundleDetail
		local infoList 	= HoN_Store.ProductPageInfo[detail.Category]

		local data = {
			productID			= infoList.productIDs[index],
			code				= infoList.productCodes[index],
			icon				= infoList.images[index],
			goldCost			= infoList.productPrices[index],
			owned				= infoList.owned[index] == '1',
			silverCost			= infoList.premiumMmpCost[index],
		}
		data.displayName 		= GetProductDispayNameByID(data.productID)
		data.productCode		= data.code

		local isHero = false
		local isAvatar = false
		if not detail.IsTypeList and string.match(data.code, '^aa%.') then
			if string.match(data.code, '%.Hero$') or string.match(data.code, '%.eap$') then
				isHero = true
			end
			isAvatar = not isHero
			data.code = string.sub(data.code, 4)
		end

		ClearBundleImageFrame(parent, prefix)

		data.avatarType 	= 'small'
		if isHero or isAvatar then
			data.isAltAvatar 	= isAvatar
			data.heroAvatarStr	= data.code
			if isAvatar then
				data.heroEntry = HoN_Store.allAvatarsInfo[tonumber(data.productID)]
			end

			SetupHeroAvatarFramePanel(parent, prefix, data, true, cat)
		else
			SetupBundleImageFrame(parent, prefix, data)
		end
	end

	local function SetupBundleDetailFrames(page, itemIndex, fromPagging)
		local itemCount = detail.Count

		local itemFrame = SetupDetailFrames(cat, page, itemCount, itemIndex, function(panel, prefix, index)
			SetupBundleDetailFrame(panel, prefix, index)
		end)

		local selectNothing = itemIndex == 0
		if selectNothing then
			itemFrame = nil
		end

		if fromPagging then
			local highlightEffect = GetWidget('store2_detail_highlight_bundleDetail')
			local isSelectedItemPage = page == detail.SelectPage
			local frameSelected =  detail.SelectIndex > 0
			highlightEffect:SetVisible(isSelectedItemPage and frameSelected)
		else
			HoN_Store:Store2PendingJobsPush(
				function()
					HoN_Store:SelectBundleItem(itemFrame, itemIndex)
					return true
				end
			)
		end
		HoN_Store:Store2PendingJobsPush(
			function()
				Store2ModelContextManager.RecyclePagesInCase(cat)
				return true
			end
		)
	end

	if fromPagging then
		itemIndex = detail.SelectIndex
	end

	SetupBundleDetailFrames(page, itemIndex, fromPagging)

	GetWidget('store2_bundle_list'):SetVisible(false)
	GetWidget('store2_detail_bundleDetail'):SetVisible(true)

	local currentPageName = HoN_Store:Store2GetCurrentPage()
	if currentPageName ~= 'bundle_detail' and currentPageName ~= 'bundles' then
		Store2Tab('bundles')
	end

	detail.CallingShowBundleDetailPage = false
end

function HoN_Store:OnClickBundleDetailFrame(w, i)
	if HoN_Store.BundleDetail.IsTypeList then return end

	local detail 	= HoN_Store.BundleDetail
	if detail.SelectPage == HoN_Store.paging.bundleDetail.currentPage and
		detail.SelectIndex  == i  then
		return
	end

	HoN_Store:SelectBundleItem(w, i, true)
end

function HoN_Store:RestoreBundleDetailPage()

	local detail 			= HoN_Store.BundleDetail
	HoN_Store:SwitchToBundleDetailPage(detail.BundleRowToInit, detail.BundleColToInit)
	detail.BundleRowToInit 	= nil
	detail.BundleColToInit 	= nil
end

function HoN_Store:OnShowBundleDetailPage()

	local detail = HoN_Store.BundleDetail

	if detail.CallingShowBundleDetailPage then return end

	local index = detail.SelectIndex
	HoN_Store:SetupBundleDetailBoothPanel(index)
end

function HoN_Store:OnHideBundleDetailPage()
	local boothID = HoN_Store.BundleDetail.BoothID
	GetWidget('store2_specials_booth_'..boothID):SetVisible(false)
	GetWidget('store2_specials_booth_model_'..boothID):SetVisible(true)

	local detail 			= HoN_Store.BundleDetail

	local bundlePage, bundleRow, BundleCol, pageToInit, indexToInit, eligiblity
	local bundleId, bundleIcon

	local fromSpecials	= detail.fromSpecials
	if fromSpecials then
		bundleId 	= detail.BundleId
		bundleIcon 	= detail.Icon
	else
		bundleRow	= detail.BundleRow
		BundleCol	= detail.BundleCol
	end
	pageToInit	= detail.SelectPage
	indexToInit	= detail.SelectIndex
	eligiblity 	= detail.BundleEligiblity
	bundlePage 	= detail.BundlePage

	HoN_Store:NavHistoryPush2('bundle detail',function()
		if HoN_Store:IsVaultOpened() then
			HoN_Store:Store2OpenShop(false)
		end

		GetWidget('store2_bundle_list'):SetVisible(false)
		Store2Tab('bundles')

		detail.PageToInit		= pageToInit
		detail.IndexToInit		= indexToInit
		if fromSpecials then
			HoN_Store:SwitchToBundleDetailPageFromSpecials(bundleId, bundlePage, bundleIcon, true)
		else
			detail.BundleRowToInit	= bundleRow
			detail.BundleColToInit	= BundleCol

			local pageInfo 	= HoN_Store.ProductPageInfo.bundles
			if bundlePage ~= pageInfo.currentPage then
				HoN_Store:SwitchToBundleListPage(bundlePage)
				GetWidget('store2_bundle_list'):SetVisible(false)
				HoN_Store.InitializeProductShowCallback = function()
					HoN_Store:RestoreBundleDetailPage()
					detail.BundleEligiblity = eligiblity
					return {noEndLoading = true}
				end
			else
				HoN_Store:RestoreBundleDetailPage()
				detail.BundleEligiblity = eligiblity
			end
		end
	end)
end

function HoN_Store:OnHideBundleListPage()
	local detail = HoN_Store.BundleDetail
	if detail.BundleRowToInit ~= nil then
		return
	end
	local page = HoN_Store.paging.bundles.currentPage
	HoN_Store:NavHistoryPush2('bundle list',function()
		--HoN_Store.paging.bundles.currentPage = page
		--HoN_Store.ProductPageInfo[cat].currentPage = page
		if HoN_Store:IsVaultOpened() then
			HoN_Store:Store2OpenShop(false)
		end
		--Store2Tab('bundles')
		HoN_Store:SwitchToBundleListPage(page)
	end)
	detail.SelectIndex 	= -1
end

local function RefreshUpgrades()
	interface:UICmd('ChatRefreshUpgrades(); ClientRefreshUpgrades();')
	if (UIGamePhase() >= 1) then
		interface:UICmd('ServerRefreshUpgrades()')
	end
end

function HoN_Store:UpdateBundleItems()

	local detail 			= HoN_Store.BundleDetail
	if detail.IsInitializing then return end
	if not detail.IsShowing	then return end

	local infoList 			= HoN_Store.ProductPageInfo[detail.Category]
	local codes				= infoList.productCodes
	local owns				= infoList.owned
	local count				= #codes

	local boothID 			= detail.BoothID
	local allOwned 			= false

	local function UpdateData()
		local allOwned = true
		for i = 1, count do
			local code 	= codes[i]
			local own	= Client.IsProductOwned(code)
			owns[i] = own and '1' or '0'
			if not own then
				allOwned = false
			end
		end
		return allOwned and count > 0
	end

	local function UpdateFrames()
		local cat 			= detail.Category
		local parent 		= GetWidget('store2_detail_'..cat..'_avatars')

		local pageInfo		= HoN_Store.paging[cat]
		local page			= pageInfo.currentPage
		local startIndex 	= HoN_Store.paging.StartIndex(page, cat)
		local widgetCount 	= pageInfo.countPerPage

		for i = 1, widgetCount do
			local prefix = 'store2_alt_small_frame_'..cat..'_'..i..'_'
			local panel = parent:GetWidget(prefix..'panel')
			if panel:IsVisibleSelf() then
				local index = startIndex + i - 1

				local owned = owns[index] == '1'
				panel:GetWidget(prefix..'owned'):SetVisible(owned)
			end
		end
	end

	if not detail.IsTypeList then
		allOwned = UpdateData()
		UpdateFrames()
	end
	if detail.IsLuckyBundle then
		local remainCount = detail.LuckyChanceCount or 0
		
		local newDesc	= Translate('store2_lucky_bundle_desc', 'chance', remainCount, 'count', detail.LuckyItemCount)
		local descriptionWidget = GetWidget('st2_sp_booth_description2_label_'..boothID)
		descriptionWidget:SetText(newDesc)

		GetWidget('store2_specials_booth_bundle_lucky_count_'..boothID):SetText(remainCount)
		if remainCount == 0 then
			allOwned = true
		end
	end

	if allOwned then
		GetWidget('store2_specials_booth_buy_button_'..boothID):SetVisible(false)
		GetWidget('store2_specials_booth_owned_button_vanity_label'):SetText(Translate('mstore_purchased'))
		GetWidget('store2_specials_booth_owned_button_vanity'):SetVisible(true)
	end
end

HoN_Store.RequestResultHandlers.buyBundle.default = function(success, newData, ...)

	local function HandleGrabbagData(...)

		if arg == nil then
			Echo('^rHandleGrabbagData() error: nil arg')
			return
		end

		local argOffset			= 2

		local popupCode			= tonumber(arg[argOffset + 1])
		if popupCode ~= STORE2_POPUPCODE_BUYITEM then
			return
		end

		local grabbagField 		= arg[argOffset + 68]
		local isGrabbag 		= grabbagField ~= nil and grabbagField ~= '' and AtoB(grabbagField)

		if isGrabbag then
			local theme			= arg[argOffset + 69]
			local productIDs 	= arg[argOffset + 70]
			local productTypes 	= arg[argOffset + 71]
			local productPaths 	= arg[argOffset + 72]
			local productNames 	= arg[argOffset + 73]

			UIManager.GetInterface('main'):HoNGrabbag2F('GrabBagResults', 'store_grab_bag_container',
				theme, productIDs, productNames, productPaths, productTypes)

			if HoN_Store.BundleDetail.IsShowing then
				HoN_Store.BundleDetail.LuckyChanceCount = HoN_Store.BundleDetail.LuckyChanceCount - 1
			end
		end
		RefreshUpgrades()
	end

	if success then
		HandleGrabbagData(...)
	end
end

-- tests
--[[
function FakeGrabbagResult(i, ...)
	local lists = HoN_Store.ProductAvatars

	i = tonumber(i) or 1
	local themes 		= {'default', 'christmas', 'esports', 'celebrate', 'halloween'}

	local ts = {
		{
			t 	= 'Alt Avatar',
			id	= '2353',	
			path= '/heroes/sir_benzington/alt8/icon.tga',
			code= 'Hero_Bubbles.Alt5',
		},
		{
			t 	= 'Alt Avatar',
			id	= '2353',	
			path= '/heroes/sir_benzington/alt8/icon.tga',
			code= 'Hero_Pyromancer.Alt4',
		},
		{
			t 	= 'Alt Avatar',
			id	= '2353',	
			path= '/heroes/sir_benzington/alt8/icon.tga',
			code= 'Hero_Riptide.Alt2',
		},
		{
			t 	= 'Creep',
			id	= '3928',	
			path= '/ui/legion/ability_coverup.tga',
			code= 'cr.Custom Creep',
		},
		{
			t 	= 'Taunt',
			id	= '91',	
			path= '/ui/legion/icons/taunt.tga',
			code= 't.Standard',
		},
		{
			t 	= 'Alt Announcement',
			id	= '4186',	
			path= '/ui/fe2/store/icons/announcer_haunted_house.tga',
			code= 'av.Haunted House Announcer',
		},
		{
			t 	= 'Misc',
			id	= '4150',	
			path= '/ui/fe2/store/icons/bundle.tga',
			code= 'Backpack 5',
		},
		{
			t 	= 'Hero',
			id	= '21',	
			path= '/heroes/moira/icon.tga',
			code= 'Hero_Moira.Hero',
		},
		{
			t 	= 'Alt Avatar',
			id	= '2353',	
			path= '/heroes/sir_benzington/alt8/icon.tga',
			code= 'Hero_Hellbringer.Alt',
		},
		{
			t 	= 'Account Icon',
			id	= '228',	
			path= 'ui/fe2/store/icons/unlock.tga',
			code= 'ai.Icon Unlock',
		},
		{
			t 	= 'Chat Color',
			id	= '174',	
			path= 'ui/icons/gm.tga',
			code= 'cc.gmshield',
		},
		{
			t 	= 'Chat Symbol',
			id	= '3563',	
			path= '/heroes/tarot/alt7/icon.tga',
			code= 'cs.Rift Hunter Tarot',
		},
		{
			t 	= 'Couriers',
			id	= '3019',	
			path= '/shared/automated_courier/couriers/paragon_fly_icon.tga',
			code= 'c.paragon_courier',
		},
		{
			t 	= 'Ward',
			id	= '4069',	
			path= '/ui/fe2/store/icons/ward_floodlightward.tga',
			code= 'w.floodlight_ward',
		},
		{
			t 	= 'Enhancement',
			id	= '3254',	
			path= '/ui/fe2/store/icons/upgrade_icons/paragon_revenant_stat.tga',
			code= 'en.revenant_stat_track',
		},
		{
			t 	= 'Bundle',
			id	= '4457',	
			path= '/ui/fe2/elements/store2/bundle_lucky.tga',
			code= '2016 Holiday Grab Bag 2',
		},
	}

	local theme			= themes[i]

	local offset = 1
	local p1 = tonumber(arg[offset]) or 1
	local t = ts[p1]
	local productIDs 	= t.id
	local productNames 	= t.code
	local productPaths 	= t.path
	local productTypes 	= t.t

	for i = offset + 1, math.min(#arg, offset + 5 - 1) do
		local j = arg[i]
		if j > #ts then j = 1 end
		local t = ts[j]
		productIDs 	 = productIDs..'|'.. t.id
		productNames = productNames..'|'.. t.code
		productPaths = productPaths..'|'.. t.path
		productTypes = productTypes..'|'.. t.t
	end
	
	UIManager.GetInterface('main'):HoNGrabbag2F('GrabBagResults', 'store_grab_bag_container',
		theme, productIDs, productNames, productPaths, productTypes)
end
]]--

-- vanity data
HoN_Store.ProductPageInfo =
{
	["ActiveVanity"] = nil,
	["ActiveCategory"] = nil,
	["accounticons"] =
	{
		["categoryID"] = '3',
		["vanityType"] = 'accountvanity',
		["elementPerRow"] = 8,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
		["typeStr"]		= 'Account Icon',
		["fetchAllPages"] = true,
		["FilterString"] = '',
	},
	["namecolors"] =
	{
		["categoryID"] = '16',
		["vanityType"] = 'accountvanity',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
		["typeStr"]		= 'Chat Color',
	},
	["symbols"] =
	{
		["categoryID"] = '4',
		["vanityType"] = 'accountvanity',
		["elementPerRow"] = 8,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
		["typeStr"]		= 'Chat Symbol',
		["fetchAllPages"] = true,
		["FilterString"] = '',
	},
	["taunts"] =
	{
		["categoryID"] = '27',
		["vanityType"] = 'gamevanity',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
		["typeStr"]		= 'Taunt',
	},
	["tauntbadges"] =
	{
		["categoryID"] = '77',
		["vanityType"] = 'gamevanity',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
		["typeStr"]		= 'Taunt Badge',
	},
	["announcers"] =
	{
		["categoryID"] = '5',
		["vanityType"] = 'gamevanity',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
		["typeStr"]		= 'Alt Announcement',
	},
	["couriers"] =
	{
		["categoryID"] = '57',
		["vanityType"] = 'gamevanity',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
		["typeStr"]		= 'Couriers',
	},
	["wards"] =
	{
		["categoryID"] = '74',
		["vanityType"] = 'gamevanity',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
		["typeStr"]		= 'Ward',
	},
	["tpeffects"] =
	{
		["categoryID"] = '78',
		["vanityType"] = 'gamevanity',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
		["typeStr"]		= 'TP Effect',
	},
	["selectioncircles"] =
	{
		["categoryID"] = '79',
		["vanityType"] = 'gamevanity',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
		["typeStr"]		= 'Selection Circle',
	},
	["upgrades"] =
	{
		["categoryID"] = '75',
		["vanityType"] = 'gamevanity',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
		["typeStr"]		= 'Enhancement',
	},
	["others"] =
	{
		["categoryID"] = '6',
		["vanityType"] = 'others',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
	},
	["vault_accounticons"] =
	{
		["categoryID"] = '3',
		["vanityType"] = 'vault_accountvanity',
		["elementPerRow"] = 8,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
	},
	["vault_namecolors"] =
	{
		["categoryID"] = '16',
		["vanityType"] = 'vault_accountvanity',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
	},
	["vault_symbols"] =
	{
		["categoryID"] = '4',
		["vanityType"] = 'vault_accountvanity',
		["elementPerRow"] = 8,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
	},
	["vault_taunts"] =
	{
		["categoryID"] = '27',
		["vanityType"] = 'vault_taunts',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
	},
	["vault_tauntbadges"] =
	{
		["categoryID"] = '77',
		["vanityType"] = 'vault_tauntbadges',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
	},
	["vault_announcers"] =
	{
		["categoryID"] = '5',
		["vanityType"] = 'vault_announcers',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
	},
	["vault_couriers"] =
	{
		["categoryID"] = '57',
		["vanityType"] = 'vault_couriers',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
	},
	["vault_wards"] =
	{
		["categoryID"] = '74',
		["vanityType"] = 'vault_wards',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
	},
	["vault_tpeffects"] =
	{
		["categoryID"] = '78',
		["vanityType"] = 'vault_tpeffects',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
	},
	["vault_selectioncircles"] =
	{
		["categoryID"] = '79',
		["vanityType"] = 'vault_selectioncircles',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
	},
	["vault_upgrades"] =
	{
		["categoryID"] = '75',
		["vanityType"] = 'vault_upgrades',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
	},
	["vault_creeps"] =
	{
		["categoryID"] = '76',
		["vanityType"] = 'vault_creeps',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
	},
	["vault_others"] =
	{
		["categoryID"] = '6',
		["vanityType"] = 'vault_others',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 3,
		["currentPage"] = 1,
	},
	["bundles"] =
	{
		["categoryID"] = '7',
		["vanityType"] = 'bundles',
		["elementPerRow"] = 3,
		["rowsPerPage"] = 4,
		["currentPage"] = 1,
		["typeStr"]		= 'Bundle',
	},
	["bundleDetail"] =
	{
		["vanityType"] = 'bundleDetail',
		["elementPerRow"] = 4,
		["rowsPerPage"] = 2,
		["currentPage"] = 1,
	},
}

-- symbols/account-icon sort/filter
local function GetVanityLists(cat, listName)
	return HoN_Store.ProductPageInfo[cat][listName]
end

local function CompVanity(cat, listName, asc, number, price, comp2)
	local getValueFunc = function(i)
			local tb = GetVanityLists(cat, listName)
			tb = tb[i]
			if number then
				tb = tonumber(tb) or -2
				if price then
					if tb == 9006 then tb = 0 end	-- 9006 not valid gold price
					if tb == 0 then	
						-- free < unlock me < (sc > 0)
						local lists = HoN_Store.ProductPageInfo[cat]
						local productID = lists.productIDs[i]
						local showUnlockLabel = IsEligibilityLocked(productID, lists.Eligibility)
						if showUnlockLabel then
							return -1
						end
						local silver = tonumber(lists.premiumMmpCost[i]) or 0
						if silver == 9002 then silver = 0 end
						return silver > 0 and 0 or -2
					end
				end
			end
			return tb
		 end
	return CompFunc(getValueFunc, asc, number, comp2)
end

local function CompVanityDisplayName(cat, asc, comp2)
	local getValueFunc = function(i)
			local pageInfo = HoN_Store.ProductPageInfo[cat]
			local productID = pageInfo.productIDs[i]
			return GetProductDispayNameByID(productID)
		 end
	return CompFunc(getValueFunc, asc, false, comp2)
end

local function CompVanityIdFunc(cat)
	local pageInfo = HoN_Store.ProductPageInfo[cat]
	return function(i1, i2)
		local v1 = tonumber(pageInfo.productIDs[i1])
		local v2 = tonumber(pageInfo.productIDs[i2])
		return v2 < v1
	end
end


HoN_Store.SharedSortedIndexes = {}
HoN_Store.CompareSymbolsIdFunc = CompVanityIdFunc('symbols')
HoN_Store.CompareAccounticonsIdFunc = CompVanityIdFunc('accounticons')

HoN_Store.comboItems.symbols = {
	types = {
		{ label = 'general_all', 			catId = 4 },
		{ label = 'mstore_category59', 		catId = 59 },
		{ label = 'mstore_category60', 		catId = 60 },
		},
	order = {
		{ label = 'store2_release_date_desc', 		comp = CompVanity('symbols', 'productIDs', false, true, false, HoN_Store.CompareSymbolsIdFunc ) },
		{ label = 'store2_product_name_asc', 		comp = CompVanityDisplayName('symbols', true, HoN_Store.CompareSymbolsIdFunc ) },
		{ label = 'store2_price_desc', 				comp = CompVanity('symbols', 'productPrices', false, true, true, HoN_Store.CompareSymbolsIdFunc ) },
		{ label = 'store2_price_asc', 				comp = CompVanity('symbols', 'productPrices', true, true, true, HoN_Store.CompareSymbolsIdFunc ) },
	},
}

HoN_Store.comboItems.accounticons = {
	order = {
		{ label = 'store2_release_date_desc', 		comp = CompVanity('accounticons', 'productIDs', false, true, false, HoN_Store.CompareAccounticonsIdFunc ) },
		{ label = 'store2_product_name_asc', 		comp = CompVanityDisplayName('accounticons', true, HoN_Store.CompareAccounticonsIdFunc ) },
		{ label = 'store2_price_desc', 				comp = CompVanity('accounticons', 'productPrices', false, true, true, HoN_Store.CompareAccounticonsIdFunc ) },
		{ label = 'store2_price_asc', 				comp = CompVanity('accounticons', 'productPrices', true, true, true, HoN_Store.CompareAccounticonsIdFunc ) },
	},
}

HoN_Store.IgnoreVanityConditionChange = false
function HoN_Store:OnVanityConditionChange(cat, changedType, searchStr, calledByInit)

	if HoN_Store.IgnoreVanityConditionChange then return end

	local pageInfo = HoN_Store.ProductPageInfo[cat]
	if pageInfo == nil or not pageInfo.fetchAllPages then
		Echo('^rOnVanityConditionChange invalid cat='..tostring(cat))
		return
	end

	if changedType == 'order' or changedType == 'all' then

		local sortOrderIndex = GetComboBoxIndex(cat, 'order')
		local sortOrder = HoN_Store.comboItems[cat].order[sortOrderIndex]
		table.sort(HoN_Store.SharedSortedIndexes, sortOrder.comp)

		-- pin custom icon to the first
		if cat == 'accounticons' and pageInfo.productIDs and pageInfo.productIDs[1] == '464' then
			local indexes = HoN_Store.SharedSortedIndexes

			local indexFound = false
			for i = #indexes, 1, -1 do
				if indexFound then
					indexes[i + 1] = indexes[i]
				else
					indexFound = indexes[i] == 1
				end
			end
			indexes[1] = 1
		end
	end

	if searchStr == nil then
		searchStr = pageInfo.FilterString
	end
	searchStr = ParseSearchStr(searchStr)
	local src = HoN_Store.SharedSortedIndexes
	local result

	if searchStr ~= '' then
		result = {}
		for i, idx in ipairs(src) do
			local productID = pageInfo.productIDs[idx]
			local displayName = GetProductDispayNameByID(productID)
			if displayName:lower():find(searchStr, 1, true) then
				table.insert(result, idx)
			end
		end
	else
		searchStr = ''
		result = src
	end

	pageInfo.FilterString = searchStr
	HoN_Store.SharedFiltedIndexes = result

	-- switch to first page
	if not calledByInit then
		pageInfo.currentPage = 1
	end
	
	HoN_Store:ClearVanityPage(cat)
	HoN_Store:InitializeProductShow(cat, pageInfo, pageInfo.currentPage)
end

function HoN_Store:ResetVanityConditions(cat)
	HoN_Store.IgnoreVanityConditionChange = true

	Set('store2_combobox_'..cat..'_order_value', 1)
	GetWidget('store2_searchbox_'..cat..'_textbox'):EraseInputLine()
	local pageInfo = HoN_Store.ProductPageInfo[cat]
	pageInfo.FilterString = ''

	HoN_Store.IgnoreVanityConditionChange = false
end

function HoN_Store:SaveVanityConditions(cat, saveTo)
	saveTo.order = GetCvarInt('store2_combobox_'..cat..'_order_value')
	local pageInfo = HoN_Store.ProductPageInfo[cat]
	saveTo.search = pageInfo.FilterString
end

function HoN_Store:RestoreVanityConditions(cat, savedValues)
	HoN_Store.IgnoreVanityConditionChange = true
	Set('store2_combobox_'..cat..'_order_value', savedValues.order)
	GetWidget('store2_searchbox_'..cat..'_textbox'):SetInputLine(savedValues.search)

	local pageInfo = HoN_Store.ProductPageInfo[cat]
	pageInfo.FilterString = savedValues.search

	HoN_Store.IgnoreVanityConditionChange = false
end

function HoN_Store:IsVanityConditionsChanged(cat)
	if GetCvarInt('store2_combobox_'..cat..'_order_value') ~=  1 then return true end
	local pageInfo = HoN_Store.ProductPageInfo[cat]
	return pageInfo.FilterString ~= ''
end

-- symbols type(all/flag/hero)
local resetingSymbolsType = false
function HoN_Store:ResetSymbolsType()
	resetingSymbolsType = true

	local combobox = GetWidget('store2_combobox_symbols_types')
	combobox:SetSelectedItemByIndex(0)

	resetingSymbolsType = false
end

function HoN_Store:OnSelectSymbolsType()
	local cat = 'symbols'
	local index = GetComboBoxIndex(cat, 'types')
	local t = HoN_Store.comboItems.symbols.types[index]

	local catTable = HoN_Store.ProductPageInfo.symbols
	catTable.categoryID = tostring(t.catId)

	if resetingSymbolsType then return end

	HoN_Store:Store2FormGetProducts(cat, 1)
end

-- scale
function HoN_Store:SetWidgetScale(widget, s)
	local w = widget:GetWidth()
	local h = widget:GetHeight()
	local widgetName = widget:GetName()

	widget:SetWidth(w * s)
	widget:SetHeight(h * s)
end

local boothLabelFonts = {
	['default'] = 'dyn_12',
	['buynow'] = {
		['1'] = 'dyn_16',
		['2'] = 'dyn_14',
		['3'] = 'dyn_14',
		['4'] = 'dyn_12',
		['5'] = 'dyn_12',
		['vanity'] = 'dyn_16',
		['others'] = 'dyn_16',
		['ea']	= 'dyn_16',
		['fe']	= 'dyn_16',
	},
	['discount'] = {
		['1'] = 'dyn_14',
		['2'] = 'dyn_12',
		['3'] = 'dyn_12',
		['4'] = 'dyn_10',
		['5'] = 'dyn_10',
		['vanity'] = 'dyn_14',
	},
	['heroname'] = {
		['1'] = 'dyn_16',
		['2'] = 'dyn_12',
		['3'] = 'dyn_12',
		['4'] = 'dyn_10',
		['5'] = 'dyn_10',
		['vanity'] = 'dyn_16',
		['ea1']	= 'dyn_16',
		['ea2']	= 'dyn_12',
		['ea3']	= 'dyn_12',
	},
	['info'] = {
		['1'] = 'dyn_11',
		['2'] = 'dyn_10',
		['3'] = 'dyn_10',
		['4'] = 'dyn_8',
		['5'] = 'dyn_8',
		['vanity'] = 'dyn_12',
		['ea1']	= 'dyn_12',
		['ea2']	= 'dyn_10',
		['ea3']	= 'dyn_10',
	},
	['band'] = {
		['1'] = 'dyn_16',
		['2'] = 'dyn_14',
		['3'] = 'dyn_14',
		['4'] = 'dyn_12',
		['5'] = 'dyn_12',
		['vanity'] = 'dyn_16',
		['ea1']	= 'dyn_12',
		['ea2']	= 'dyn_10',
		['ea3']	= 'dyn_10',
	},
	['price'] = {
		['1'] = 'dyn_11',
		['2'] = 'dyn_11',
		['3'] = 'dyn_11',
		['4'] = 'dyn_11',
		['5'] = 'dyn_11',
		['vanity'] = 'dyn_12',
		['ea1']	= 'dyn_12',
		['ea2']	= 'dyn_12',
		['ea3']	= 'dyn_12',
	},
	['choose'] = {
		['1'] = 'dyn_16',
		['2'] = 'dyn_14',
		['3'] = 'dyn_14',
		['4'] = 'dyn_12',
		['5'] = 'dyn_12',
		['vanity'] = 'dyn_16',
		['ea']	= 'dyn_16',
		['fe']	= 'dyn_16',
	},
	['namecolors'] = {
		['1'] = 'dyn_14',
		['2'] = 'dyn_12',
		['3'] = 'dyn_12',
		['4'] = 'dyn_12',
		['5'] = 'dyn_12',
		['vanity'] = 'dyn_14',
	}
}

local function HasBooth(cat)
	return cat ~= 'accounticons' and cat ~= 'symbols' and cat ~= 'vault_accounticons' and cat ~= 'vault_symbols' and cat ~= 'bundles'
end

function HoN_Store:GetBoothLabelFonts(cat, id)
	return boothLabelFonts[cat][id] or boothLabelFonts.default
end

function HoN_Store:SetLabelText(widget, widgetName, id, txt, noTranslate, shortenOnNeed, fitString)
	local font = boothLabelFonts[widgetName][id] or boothLabelFonts.default
	widget:SetFont(font)
	if not noTranslate then
		txt = Translate(txt)
	end
	if shortenOnNeed then
		local maxWidth = widget:GetWidth()
		if widget:GetStringWidth(txt) > maxWidth then
			local i = -1
			local shortText = sub(txt, 1, i) ..'...'
			while widget:GetStringWidth(shortText) > maxWidth do
				i = i - 1
				shortText = sub(txt, 1, i).."..."
			end
			txt = shortText
		end
	end
	if fitString then
		widget:SetWidth(GetStringWidth(font, txt) + 10)
	end
	widget:SetText(txt)
end

function HoN_Store:BoothClearPrices(id)
	local parent = GetWidget('store2_specials_booth_price_parent_'..id)
	parent:ClearChildren()
end

function HoN_Store:BoothSetPrice(id, subid, type, scale, linethrough, money)
	-- type: gold or silver
	if type == "gold" and money == 9006 then return end
	if type == "silver" and money == 9002 then return end

	local parent = GetWidget('store2_specials_booth_price_parent_'..id)
	parent:SetVisible(true)
	parent:Instantiate('store2_specials_booth_price', 'id', id, 'subid', subid, 'type', type, 'scale', scale, 'linethrough', tostring(linethrough), 'money', tostring(money))
end


function HoN_Store:Store2VanitySetVisible(self, myVanityType, nextVanityType, nextCategory)
	if nextCategory == '' then
		self:SetVisible(0)
	elseif nextVanityType == myVanityType then
		self:SetVisible(1)
		Store2Tab(nextVanityType)
	end
end

LastVisibleVanityCategoryWidget = nil
function HoN_Store:Store2CategorySetVisible(self, myVanityType, myCategoryType, nextVanityType, nextCategoryType, forceRefresh)
	forceRefresh = AtoB(forceRefresh or '0')

	-- it is trying to load another vanity, do nothing
	if myVanityType ~= nextVanityType then return end
	-- trying to activate the currently activated page and the current page is at page 1? do nothing
	if not forceRefresh
		and nextVanityType == HoN_Store.ProductPageInfo.ActiveVanity
		and nextCategoryType == HoN_Store.ProductPageInfo.ActiveCategory
		and HoN_Store.paging[nextVanityType] ~= nil and HoN_Store.paging[nextVanityType].currentPage == 1 then
		return
	end

	-- Reset to the 1st page, always except naving back
	if not forceRefresh and not Store2NavBackInProgress then
		HoN_Store:Store2PagingStateClear(nextCategoryType)
	end

	-- trying to load me
	if myCategoryType == nextCategoryType then
		HoN_Store:ClearVanityPage(myCategoryType)
		HoN_Store.ProductPageInfo.ActiveCategory = myCategoryType
		HoN_Store.ProductPageInfo.ActiveVanity = myVanityType

		if LastVisibleVanityCategoryWidget ~= nil then
			local tback = Store2NavBackInProgress
			if LastVisibleVanityCategoryWidget == self then
				Store2NavBackInProgress = true
			end
			LastVisibleVanityCategoryWidget:SetVisible(false)
			Store2NavBackInProgress = tback
		end
		LastVisibleVanityCategoryWidget = self

		-- force refresh? set invisible first!
		if forceRefresh and self:IsVisible() == true then
			self:SetVisible(false)
		end
		self:SetVisible(true)
	end
end


local VanityBoothInfo =
{
	["Pedestal"] =
	{
		["offset"] = {
			['default'] = {0, 0, -50},
			['taunts'] = {0, 0, -2000}, -- Hide
			['vault_taunts'] = {0, 0, -2000}, -- Hide
			['vault_creeps'] = {0, 0, 0},
			['creeps'] = {0, 0, 0},
		},
		["scale"] = 0.5,
	},
	["Couriers"] =
	{
		["scale"] = 0.5,
	},
	["FlamingEye"] =
	{
		["offset"] = {25, 0, -50},
		["scale"] = 0.4,
	},
	["ManaEye"] =
	{
		["offset"] = {-25, 0, -50},
		["scale"] = 0.4,
	},
}

-- key: vaultcategory (vault_accounticons, etc)
-- value: a string block returned from the master server
HoN_Store.Store2VaultData = {}

local function Store2GetMaxElementPerPage(category)
	return HoN_Store.ProductPageInfo[category]["elementPerRow"] * HoN_Store.ProductPageInfo[category]["rowsPerPage"]
end

local function Store2VanityGetIndex(cat, row, col)

	local currentPage, elementsPerPage, indexArray
	local catInfo = HoN_Store.ProductPageInfo[cat]
	local elementPerRow = catInfo['elementPerRow']

	if IsVault(cat) then
		currentPage = HoN_Store.paging[cat]['currentPage']
		elementsPerPage = Store2GetMaxElementPerPage(cat)
	else
		elementsPerPage = elementPerRow * catInfo.rowsPerPage
		if catInfo.fetchAllPages then
			currentPage = catInfo.currentPage
		end
		if HoN_Store.ProductPageInfo[cat].fetchAllPages then
			indexArray = HoN_Store.SharedFiltedIndexes
		end
	end

	local index = elementPerRow * (row-1) + col
	if currentPage then
		index = index + (currentPage - 1) * elementsPerPage
	end
	if indexArray then
		index = indexArray[index]	-- warn: check overflow
	end

	return index
end

-- current in-booth product info
local VanityBoothSelectInfo =
{
	["cat"] 		= '',
	["productID"] 	= nil,
	["Code"] 		= nil,
	["Owned"] 		= false,
	["Purchasable"] = false,
	["Page"] 		= 0,
	["Effect"] 		= nil,
	["Eligibility"]	= {},

	["localContent"]= '',
	["gold"]		= '',
	["silver"]		= '',

	["boothData"]	= {},
}

function TestPrintBoothSelectInfo()
	Echo('^g VanityBoothSelectInfo=')
	printTableDeep(VanityBoothSelectInfo)
end

-- select item should be in current page
local function SetVanityBoothSelectedInfo(cat, row, col, data)
	VanityBoothSelectInfo["cat"] = cat

	local productID 	= nil
	local Code 			= nil
	local Owned 		= false
	local Purchasable	= false
	local Eligibility 	= {}

	local charges		= nil
	local durations		= nil
	local chargesRemaining	= nil

	local localContent	= ''
	local gold			= '-1'
	local silver		= '-1'

	local Page 			= 0

	if row and col then
		local idx = Store2VanityGetIndex(cat, row, col)
		if IsVault(cat) then
			local singleVault = HoN_Store.Store2VaultData[cat][idx]
			if singleVault then
				productID 	= singleVault['itemid']
				Code 		= singleVault['code']
				Owned 		= true
			end
		else
			local catInfo = HoN_Store.ProductPageInfo[cat]
			productID 	= catInfo['productIDs'][idx]
			Code 		= catInfo['productCodes'][idx]
			Owned 		= IsOwned(catInfo, idx)
			Purchasable	= IsPurchasable(catInfo, idx)

			local eligibilityTable 	= HoN_Store.productEligibility[productID]
			if eligibilityTable ~= nil then
				t = {
					productID		= productID,
					eligbleID		= eligibilityTable.eligbleID,
					eligible		= eligibilityTable.eligible,
					requiredProducts= eligibilityTable.requiredProducts,
				}
				Eligibility[productID] 	= t
			end

			charges			= catInfo['productCharges'][idx]
			durations		= catInfo['productDurations'][idx]
			chargesRemaining= catInfo['chargesRemaining'][idx]

			localContent= catInfo['images'][idx]
			gold		= catInfo['productPrices'][idx]
			silver		= catInfo['premiumMmpCost'][idx]
		end
		Page 			= HoN_Store.paging[cat].currentPage
	elseif data then
		if data.productID then productID = data.productID end
		if data.Page then Page = data.Page end
	end

	VanityBoothSelectInfo["productID"] 	= productID
	VanityBoothSelectInfo["Code"] 		= Code
	VanityBoothSelectInfo["Owned"] 		= Owned
	VanityBoothSelectInfo["Purchasable"]= Purchasable
	VanityBoothSelectInfo["Eligibility"]= Eligibility

	VanityBoothSelectInfo["charges"]		= charges
	VanityBoothSelectInfo["durations"]		= durations
	VanityBoothSelectInfo["chargesRemaining"] = chargesRemaining

	VanityBoothSelectInfo["localContent"] 	= localContent
	VanityBoothSelectInfo["gold"] 			= gold
	VanityBoothSelectInfo["silver"] 		= silver

	VanityBoothSelectInfo["boothData"] 		= data

	if IsVault(cat) then
		VanityBoothSelectInfo["Vault_Page"] = Page
	else
		VanityBoothSelectInfo["Page"] 		= Page
	end
end

-- warn: not vault data
local function GetStoreVanityItemInfo(cat, row, col)
	local productId, productCode, owned, purchasable, eligibility
	if row and col then
		local idx 	= Store2VanityGetIndex(cat, row, col)
		local pageInfo = HoN_Store.ProductPageInfo[cat]
		productId 	= pageInfo.productIDs[idx]
		productCode = pageInfo.productCodes[idx]
		owned 		= IsOwned(pageInfo, idx)
		purchasable = IsPurchasable(pageInfo, idx)
		eligibility = pageInfo.Eligibility	--HoN_Store.productEligibility
	else
		productId	= VanityBoothSelectInfo["productID"]
		productCode = VanityBoothSelectInfo["Code"]
		owned		= VanityBoothSelectInfo["Owned"]
		purchasable	= VanityBoothSelectInfo["Purchasable"]
		eligibility = VanityBoothSelectInfo["Eligibility"]
	end
	if productId == nil then
		Echo('^rnil productId, cat='..tostring(cat)..', row='..tostring(row)..', col='..tostring(col))
	end
	return productId, productCode, owned, purchasable, eligibility
end

local function CopyTable(src)
	if type(src) ~= 'table' then return src end
	local dst = {}
	for i, v in pairs(src) do
		dst[i] = CopyTable(v)
	end
	return dst
end

local function OnVanityBoothItemBecomeOwned(category)
	TestEcho('^r OnVanityBoothItemBecomeOwned category:'..category)
	VanityBoothSelectInfo["Owned"] = true
	GetWidget('store2_specials_booth_buy_button_vanity'):SetVisible(false)
	GetWidget('store2_specials_booth_owned_button_vanity'):SetVisible(true)
	GetWidget('store2_specials_booth_owned_button_vanity_label'):SetText(Translate('mstore_purchased'))

	local canGift = GetCvarInt('_microStoreGiftsRemaining') > 0 and category ~= 'others' and category ~= 'bundleDetail'
	Set('_microStore_SelectedItemOwned', canGift)
	-- GetWidget('store2_specials_booth_gift_button_vanity'):SetVisible(canGift)
end

function HoN_Store:RefreshVanityBooth()
	local code = VanityBoothSelectInfo["Code"]
	local cat = VanityBoothSelectInfo["cat"]
	if code ~= nil and code ~= '' and not IsVault(cat) then
		local owned = Client.IsProductOwned(code)
		if owned then
			OnVanityBoothItemBecomeOwned(cat)
		end
	end
end

function HoN_Store:ClearVanityPage(cat)

	local elementsPerPage = Store2GetMaxElementPerPage(cat)
	for idx=1, elementsPerPage do
		local elementPerRow = HoN_Store.ProductPageInfo[cat]['elementPerRow']
		local w = nil
		local row = math.floor((idx-1) / elementPerRow) + 1;
		local col = (idx-1) % elementPerRow + 1
		local widgetPrefix = 'store2_vanity_frame_'..cat..'_'..row..'_'..col

		GetWidget(widgetPrefix..'_content'):SetVisible(0)
		GetWidget(widgetPrefix..'_empty'):SetVisible(1)

		local effect  = GetWidget(widgetPrefix..'_effect', nil, true)
		if effect ~= nil then
			effect:SetVisible(0)
		end
	end
end

local function DataNotExist(data)
	return data == '-1' or data == nil
end

local refreshCurrentVanityPageReqTime = 0
function HoN_Store:Store2FormGetProducts(cat, page, fromPagging)

	HoN_Store:ClearVanityPage(cat)

	local catTable = HoN_Store.ProductPageInfo[cat]

	page = page or catTable.currentPage or 1
	catTable.currentPage = page

	-- if all pages' data have been fetched, not rquest on switching page
	if catTable.fetchAllPages and fromPagging then
		HoN_Store:InitializeProductShow(cat, catTable, catTable.currentPage)
		return
	end

	HoN_Store:Store2Loading(1, "Store2FormGetProducts")

	local _microStore_Category = catTable["categoryID"]
	local _microStore_RequestCode = 1
	local _lastRequestHostTime = GetTime()
	local _microStoreRequestAllItems = catTable.fetchAllPages and 'true' or 'false'
	local _microStoreShowNonPurchasable = 'true'

	if not fromPagging and not Store2NavBackInProgress then
		refreshCurrentVanityPageReqTime = GetTime()
	end

	HoN_Store.RequestParam.lastRequestHostTime = _lastRequestHostTime
	Set('_lastRequestHostTime', _lastRequestHostTime)
	SetRequestStatusWatch('MicroStoreStatus')

	SubmitForm('MicroStore', 'account_id', Client.GetAccountID(), 'category_id', _microStore_Category,
				'request_code', _microStore_RequestCode, 'page', page, 'cookie', Client.GetCookie(),
				'hostTime', _lastRequestHostTime, 'displayAll', _microStoreRequestAllItems, 'notPurchasable', _microStoreShowNonPurchasable)
end

function HoN_Store:Store2FormGetVault()
	HoN_Store:Store2Loading(1, "Store2FormGetVault")

	local _microStore_RequestCode = 2
	local _lastRequestHostTime = GetTime()
	local _microStoreRequestAllItems = 'false'
	local _microStoreShowNonPurchasable = 'false'

	HoN_Store.RequestParam.lastRequestHostTime = _lastRequestHostTime
	Set('_lastRequestHostTime', _lastRequestHostTime)
	SetRequestStatusWatch('MicroStoreStatus')

	SubmitForm('MicroStore', 'account_id', Client.GetAccountID(), 'category_id', 0,
				'request_code', _microStore_RequestCode, 'page', 0, 'cookie', Client.GetCookie(),
				'hostTime', _lastRequestHostTime, 'displayAll', _microStoreRequestAllItems, 'notPurchasable', _microStoreShowNonPurchasable)
end

local function VanityFrameSetLimitedText(panel, cat, row, col, text)
	local prefix = 'store2_vanity_frame_'..cat..'_'..row..'_'..col
	local expandedLabel = GetWidget(prefix..'_name_expand')
	local dotPanel = GetWidget(prefix..'_dot_panel', nil, true)
	local imageC = GetWidget(prefix..'_imagec', nil, true)
	local label = panel:GetChildren()[1]
	if label == nil then label = panel end

	local function LabelOnMouseOverWithDot()
		dotPanel:SetVisible(true)
		HoN_Store:TimedCheckCursorInSidePanel(dotPanel, HoN_Store.TimedCheckCursorInSidePanel,
			function(panel) panel:SetVisible(false) end)
	end

	if dotPanel ~= nil then dotPanel:SetVisible(0) end

	local labelWidth = label:GetWidth()
	if label:GetStringWidth(text) <= labelWidth or dotPanel == nil then
		label:SetText(text)
		panel:SetNoClick(true)
		return
	end

	local i = -1
	local shortText = sub(text, 1, i) ..'...'
	while label:GetStringWidth(shortText) > labelWidth do
		i = i - 1
		shortText = sub(text, 1, i).."..."
	end
	label:SetText(shortText)
	panel:SetCallback('onmouseover', LabelOnMouseOverWithDot)
	panel:SetNoClick(false)

	local expandedWidth = expandedLabel:GetStringWidth(text)
	imageC:SetWidth(expandedWidth)
	expandedLabel:SetWidth(expandedWidth)
	dotPanel:SetWidth(expandedWidth + 2*dotPanel:GetWidthFromString('8i'))
	expandedLabel:SetText(text)
end

function HoN_Store:VanityItemOnClickAction(cat, row, col)
	local cat = cat or VanityBoothSelectInfo["cat"]
	if row == nil or col == nil then
		Echo('^rVanityItemOnClickAction() nil row or col, cat='..tostring(cat))
		return
	end

	if IsVault(cat) then
		HoN_Store:VanityItemOnClick(HasBooth(cat), cat, row, col)
	else
		local pageInfo = HoN_Store.ProductPageInfo[cat]
		local idx = Store2VanityGetIndex(cat, row, col)
		local productId = pageInfo.productIDs[idx]

		Set('_microStore_SelectedItemOwned', false)

		if HasBooth(cat) then
			HoN_Store:VanityItemOnClick(true, cat, row, col)
		else
			if (IsOwned(pageInfo, idx) and IsPurchasable(pageInfo, idx)) or IsEligibilityLocked(productId) or not IsPurchasable(pageInfo, idx) then
				if GetCvarInt('_microStoreGiftsRemaining') > 0 and IsOwned(pageInfo, idx) and IsPurchasable(pageInfo, idx) then
					Set('_microStore_SelectedItemOwned', true)
					HoN_Store:VanityItemOnClick(false, cat, row, col)
				end
				return
			elseif IsNeedTauntLocked(pageInfo, idx) then
				Set('microStoreID999', 91)
				Set('_microStore_SelectedID', GetCvarInt('microStoreID999'))
				Set('_microStore_SelectedItem', 999)
				Set('microStoreLocalContent999', '/ui/legion/icons/taunt.tga')
				Set('microStorePrice999', GetCvarInt('_microStore_tauntUnlockCost'))
				Set('microStoreSilverPrice999', GetCvarInt('_microStore_tauntUnlockSilverCost'))
				Set('microStorePremium999', false);
				HoN_Store:OnPurchase()
			else
				HoN_Store:VanityItemOnClick(false, cat, row, col)
			end
		end
	end
end

local function JumpToVaultItem(productId, category)
	VaultItemJumpInfo['productId'] = productId
	VaultItemJumpInfo['category'] = 'vault_'..category
	HoN_Store:Store2OpenVault(false)
	HoN_Store:Store2Loading(1, "JumpToVanityItem")
end

local function GetVaultVanityItemCode(cat, row, col)
	local code = nil
	if cat ~= nil and row ~= nil and col ~= nil then
		local idx = Store2VanityGetIndex(cat, row, col)
		local singleVault = HoN_Store.Store2VaultData[cat][idx]
		code = singleVault['code']
	else
		code = VanityBoothSelectInfo["Code"]
	end
	if code == nil then
		Echo('^rGetVaultVanityItemCode() nil code, cat='..tostring(cat)..', row='..tostring(row)..', col='..tostring(col))
	end
	return code
end

local function GetVaultVanityItemProductID(cat, row, col)
	local productID = nil
	if cat ~= nil and row ~= nil and col ~= nil then
		local idx = Store2VanityGetIndex(cat, row, col)
		local singleVault = HoN_Store.Store2VaultData[cat][idx]
		productID = singleVault['itemid']
	else
		productID = VanityBoothSelectInfo["itemid"]
	end
	if productID == nil then
		Echo('^rGetVaultVanityItemProductID() nil product id, cat='..tostring(cat)..', row='..tostring(row)..', col='..tostring(col))
	end
	return productID
end

function HoN_Store:VanityItemOnRightClickAction(cat, row, col)
	local cat = cat or VanityBoothSelectInfo["cat"]

	if cat == 'bundleDetail' or IsVault(cat) then return end

	local productId, productCode, owned
	productId, productCode, owned = GetStoreVanityItemInfo(cat, row, col)
	if owned then
		JumpToVaultItem(productId, cat)
	end
end

function HoN_Store:VanityItemOnMouseOverAction(cat, row, col)
	local cat = cat or VanityBoothSelectInfo["cat"]
	if row == nil or col == nil then
		Echo('^rVanityItemOnMouseOverAction() nil row or col, cat='..tostring(cat))
		return
	end

	if cat == 'bundleDetail' then return end

	local widgetPrefix = 'store2_vanity_frame_'..cat..'_'..row..'_'..col

	if IsVault(cat) then
		GetWidget(widgetPrefix..'_frame_over'):FadeIn(200)
	else
		local pageInfo = HoN_Store.ProductPageInfo[cat]
		local idx = Store2VanityGetIndex(cat, row, col)
		local productId = pageInfo.productIDs[idx]

		if HasBooth(cat) then
			GetWidget(widgetPrefix..'_frame_over'):FadeIn(200)
		else
			if IsOwned(pageInfo, idx) and IsPurchasable(pageInfo, idx) then
				return
			else
				GetWidget(widgetPrefix..'_frame_over'):FadeIn(200)
			end
		end
	end
end

function HoN_Store:VanityItemOnMouseOutAction(cat, row, col)
	local cat = cat or VanityBoothSelectInfo["cat"]
	if row == nil or col == nil then
		Echo('^VanityItemOnMouseOutAction() nil row or col, cat='..tostring(cat))
		return
	end

	if cat == 'bundleDetail' then return end

	local widgetPrefix = 'store2_vanity_frame_'..cat..'_'..row..'_'..col

	local w = GetWidget(widgetPrefix..'_frame_over', nil, true)
	if w ~= nil then w:FadeOut(200) end
	HoN_Store:HideToolTip()
	GetWidget('store2_empty_custom_slot_tip'):SetVisible(false)
end

function HoN_Store:VanityItemShowToolTip(cat, row, col)
	local cat = cat or VanityBoothSelectInfo["cat"]
	if IsVault(cat) then return end

	local productId, productCode, owned, purchasable, eligibility

	if cat == 'bundleDetail' then
		local bundleDetail = HoN_Store.BundleDetail
		owned = bundleDetail.BundleOwned
		productId = bundleDetail.BundleId
		eligibility = bundleDetail.BundleEligiblity
	else
		productId, productCode, owned, purchasable, eligibility = GetStoreVanityItemInfo(cat, row, col)
	end

	if owned then
		if cat ~= 'bundles' and cat ~= 'bundleDetail' then 
			HoN_Store:ShowToolTip('mstore_purchased', 'mstore_purchased_desc', true)
		else
			HoN_Store:ShowToolTip('mstore_purchased', 'mstore_purchasedb_desc', true)
		end
		return
	end
	if cat ~= 'bundleDetail' and IsNeedTauntLockedForIdCode(productId, productCode) then
		HoN_Store:ShowToolTip('mstore_taunt_slot_locked', 'mstore_unlock_taunt_body', true)
		return
	end
	if eligibility and eligibility[tostring(productId)] ~= nil then
		HoN_Store:ShowEligibilityToolTipInNeed(productId, eligibility)
	end
end

function HoN_Store:VanityBoothButtomOnClickAction(cat, row, col)
	local cat = cat or VanityBoothSelectInfo["cat"]

	if cat == 'bundleDetail' then return end

	local tauntLocked = false
	if row and col then
		local idx = Store2VanityGetIndex(cat, row, col)
		local pageInfo 	= HoN_Store.ProductPageInfo[cat]
		tauntLocked 	= IsNeedTauntLocked(pageInfo, idx)
	else
		local productId = VanityBoothSelectInfo['productID']
		local productCode = VanityBoothSelectInfo['Code']
		tauntLocked 	= IsNeedTauntLockedForIdCode(productId, productCode)
	end

	if HasBooth(cat) and tauntLocked then
		Set('microStoreID999', 91)
		Set('_microStore_SelectedID', GetCvarInt('microStoreID999'))
		Set('_microStore_SelectedItem', 999)
		Set('microStoreLocalContent999', '/ui/legion/icons/taunt.tga')
		Set('microStorePrice999', GetCvarInt('_microStore_tauntUnlockCost'))
		Set('microStoreSilverPrice999', GetCvarInt('_microStore_tauntUnlockSilverCost'))
		Set('microStorePremium999', false);
		HoN_Store:OnPurchase()
	end
end

function HoN_Store:VaultItemShowToolTip(cat, row, col)
	if cat ~= 'vault_accounticons' or not IsVault(cat) then return end
	local idx = Store2VanityGetIndex(cat, row, col)
	local productId = HoN_Store.Store2VaultData[cat][idx]['itemid']
	if productId == '464' then
		GetWidget('store2_empty_custom_slot_tip'):SetVisible(true)
	end
end

HoN_Store.CatId2Name =
{
	['3'] = 'accounticons',
	['4'] = 'symbols',
	['16'] = 'namecolors',
	['27'] = 'taunts',
	['5'] = 'announcers',
	['57'] = 'couriers',
	['74'] = 'wards',
	['75'] = 'upgrades',
	['76'] = 'creeps',
	['6'] = 'others'
}

function HoN_Store:OpenVaultAfterPurchase()
	local cat = VaultItemJumpInfo["category"]
	if cat == nil then
		--  special page
		local cateId = GetCvarInt('_microStore_Category')
		cat = HoN_Store.CatId2Name[tostring(cateId)]
	end
	local productId = GetCvarInt('_microStore_SelectedID')

	JumpToVaultItem(productId, cat)

	GetWidget('store_popup_buy_item_success'):DoEventN(1);
end

function HoN_Store:BringIthItemFirst(text, index)
	local bindex = 0
	local eindex = 0
	local i = 0
	local count = 0
	while true do
		i = string.find(text, '|', i + 1)
		if i == nil then
			break
		else
			count = count + 1
			if count == index - 1 then
				bindex = i
			elseif count == index then
				eindex = i
			end
		end
	end
	if bindex == 0 or eindex == 0 then
		return text
	end
	local target = string.sub(text, bindex + 1, eindex)
	local result = string.gsub(text, '^'..string.sub(text, 1, eindex), target..string.sub(text, 1, bindex))
	return result
end

local function ChangeChildrenStytle(wdgParent, bVault, bOwned)
	local children = wdgParent:GetChildren()
	if #children <= 0 then return end

	local color, outlineColor = bVault and '#778c83' or '#d7b6a2', bVault and '#0b1a1a' or '#241410'
	local disabledColor, disabledOutlineColor = bOwned and color or '#4c3226', bOwned and outlineColor or 'black'

	-- Echo('ChangeChildrenStytle color: '..color..' outlineColor: '..outlineColor..' disabledColor: '..disabledColor..' disabledOutlineColor: '..disabledOutlineColor)

	for _, child in ipairs(children) do
		local sType = child:GetType()
		-- Echo('ChangeChildrenStytle sType: '..sType)
		if sType == 'image' then
			child:SetColor('1 1 1 .2')
		elseif sType == 'label' then
			if string.find(child:GetParent():GetName(), 'paperForm_textbox_') then
				child:SetColor(disabledColor)
				child:SetOutlineColor(disabledOutlineColor)
			else
				child:SetColor(color)
				child:SetOutlineColor(outlineColor)
			end
		elseif sType == 'textbox' or sType == 'textbuffer' then
			child:SetEnabled(bOwned)
		elseif sType == 'button' then
			-- Echo('ChangeChildrenStytle bOwned: '..tostring(bOwned))
			if bOwned then
				ChangeChildrenStytle(child, bVault, bOwned)
			end
			child:SetEnabled(bOwned)
		else
			ChangeChildrenStytle(child, bVault, bOwned)
		end
	end
end

function HoN_Store:InitializeProductShow(cat, pageInfo, currentPage)

	local gold = pageInfo.productPrices
	local silver = pageInfo.premiumMmpCost

	local elementPerRow = pageInfo["elementPerRow"]
	local elementsPerPage = elementPerRow * pageInfo["rowsPerPage"]
	local pageIndexOffset = pageInfo.fetchAllPages and (currentPage * elementsPerPage - elementsPerPage) or 0

	local indexArray
	if pageInfo.fetchAllPages then
		indexArray = HoN_Store.SharedFiltedIndexes
	end

	for it = 1, elementsPerPage do
		if pageInfo.totalPages < 1 or pageInfo.totalPages < currentPage then break end 
		
		local idx = it + pageIndexOffset

		if indexArray then
			if idx > #indexArray then
				break
			end
			idx = indexArray[idx]
		end

		if DataNotExist(pageInfo.images[idx]) then
			break
		end

		local w = nil
		local row = math.floor((it - 1) / elementPerRow) + 1;
		local col = (it - 1) % elementPerRow + 1
		local widgetPrefix = 'store2_vanity_frame_'..cat..'_'..row..'_'..col

		GetWidget(widgetPrefix..'_content'):SetVisible(1)
		GetWidget(widgetPrefix..'_empty'):SetVisible(0)

		local image = pageInfo.images[idx]

		GetWidget(widgetPrefix..'_img'):SetTexture(image)

		w = GetWidget(widgetPrefix..'_owned', nil, true)
		if w ~= nil then w:SetVisible(pageInfo.owned[idx] == '1') end

		local name = ''
		local desc = ''
		local productID = pageInfo.productIDs[idx]
		if not DataNotExist(productID) then
			name = GetProductDispayNameByID(productID)
			desc = GetProductDisplayDescByID(productID)
		end

		w = GetWidget(widgetPrefix..'_name')
		if isBundleList then
			w:SetFont('dyn_12')
			w:SetText(name)
		else
			VanityFrameSetLimitedText(w, cat, row, col, name)
		end

		w = GetWidget(widgetPrefix..'_desc', nil, true)
		if w ~= nil then w:SetText(desc) end

		local hideGoldSilverPrice = cat == 'bundles' and pageInfo.owned[idx] == '1'

		if DataNotExist(gold[idx]) or hideGoldSilverPrice then
			GetWidget(widgetPrefix..'_price'):SetVisible(0)
			w = GetWidget(widgetPrefix..'_price_lable')
			if w ~= nil then
				w:SetVisible(false)
			end
		else
			GetWidget(widgetPrefix..'_price'):SetVisible(1)

			w = GetWidget(widgetPrefix..'_gold')

			local goldCost = gold[idx]
			local silverCost = silver[idx]

			local purchasable = IsPurchasable(pageInfo, idx)
			local showGold	= false
			local showSilver= false
			local showFreeLabel = false
			local showUnlockLabel = IsEligibilityLocked(productID)
			local showNeedTauntLabel = IsNeedTauntLocked(pageInfo, idx)
			local locked = showUnlockLabel or showNeedTauntLabel

			-- if item is not eligible, hide prices and freelabel
			if not locked then
				showGold, showSilver = ShowGoldSiver(goldCost, silverCost)
				if not showGold and not showSilver then
					showFreeLabel = true
				end
			end

			local middleX 	= 0
			local parent	= w:GetParent()

			local showLabel = false
			w = GetWidget(widgetPrefix..'_price_lable')
			if w ~= nil then
				if showNeedTauntLabel then
					w:SetText(Translate('mstore_needunlocktaunt'))
					w:SetVisible(true)
					showLabel = true
				elseif showUnlockLabel then
					w:SetText(Translate('mstore_elig_only'))
					w:SetVisible(true)
					showLabel = true
				elseif not purchasable then
					w:SetText(Translate('general_unavailable'))
					w:SetVisible(true)
					showLabel = true
				elseif showFreeLabel then
					w:SetText(Translate('mstore_free'))
					w:SetVisible(true)
					showLabel = true
				else
					w:SetVisible(false)
				end
			end

			if showLabel then
				showGold = false
				showSilver = false
			end

			SetupGoldSilverPriceLabel(parent, widgetPrefix..'_gold', showGold, not showSilver, goldCost, -25, middleX)
			SetupGoldSilverPriceLabel(parent, widgetPrefix..'_silver', showSilver, not showGold, silverCost, 25, middleX)

			-- gca
			HoN_Store:ShowGcaImageInNeed(parent, widgetPrefix..'_gca', pageInfo.productCodes[idx])
		end

		local isBundleList = cat =='bundles'
		if isBundleList then
			local sTypeDiscount = split(pageInfo.grabBag[idx] or '', '~')
			local isGrabbag = sTypeDiscount[1] == '1'
			local isDiscount = not isGrabbag and tonumber(sTypeDiscount[2] or '0') > 0
			GetWidget(widgetPrefix..'_discount_mark'):SetVisible(isDiscount)
			GetWidget(widgetPrefix..'_lucky_mark'):SetVisible(isGrabbag)
		end
	end

	local count, pages
	if pageInfo.fetchAllPages then
		local list = indexArray or pageInfo.productIDs
		count = #list
		pages = 0	-- no dots
	else
		pages = pageInfo.totalPages
		count = pageInfo.totalPages * elementsPerPage
	end

	UpdatePaging(cat, currentPage, count, 11)
	HoN_Store:InitializePaging2Dots(cat, currentPage, pages)
end

function HoN_Store:Store2InitializeProductShow(cat, self,
	responseCode,				-- param0
	popupCode,					-- param1
	productIDs,					-- param2
	productNames,				-- param3
	productPrices,				-- param4
	productAlreadyOwned,		-- param5
	productIsBundle,			-- param6
	productQuantity,			-- param7
	totalPages,					-- param8
	totalPoints,				-- param9
	productWebContent,			-- param10
	productDescription,			-- param11
	productLocalContent,		-- param12
	categoryID,					-- param13
	currentPage,				-- param14
	errorCode,					-- param15
	packagePrices,				-- param16
	packagePoints,				-- param17
	packageIDs,					-- param18
	packageSpecial,				-- param19
	productCodes,				-- param20
	vaultCategory2,				-- param21
	vaultCategory3,				-- param22
	vaultCategory4,				-- param23
	vaultCategory5,				-- param24
	vaultCategory6,				-- param25
	bundleContents,				-- param26
	selectedUpgrades,			-- param27
	vaultCategory16,			-- param28
	requestHostTime,			-- param29
	accountIconsUnlocked,		-- param30
	unlockAccountIconsCost,		-- param31
	vaultCategory27,			-- param32
	productPremium,				-- param33
	totalMMPoints,				-- param34
	premium_mmp_cost,			-- param35
	regional_currency,			-- param36
	promoCode,					-- param37
	vaultCategory56,			-- param38
	vaultCategory57,			-- param39
	vaultHighlight,				-- param40
	tauntUnlocked,				-- param41
	tauntUnlockCost,			-- param42
	customAccountIcon,			-- param43
	customAccountIconCost,		-- param44
	customAccountIconCostMMP,	-- param45
	timestamp,					-- param46
	productTimes,				-- param47
	vaultCategory72,			-- param48
	is_newly_verified,			-- param49
	purchasable,				-- param50
	productCharges,				-- param51
	productDurations,			-- param52
	chargesRemaining,			-- param53
	unlockAccountIconsCostMMP,	-- param54
	specialBundles,				-- param55
	tauntUnlockCostMMP,			-- param56
	specialDisplay,				-- param57
	santas,						-- param58
	productEligibility,			-- param59
	packageTextures,			-- param60
	bundleNames,				-- param61
	bundleCosts,				-- param62
	bundleLocalPaths,			-- param63
	bundleIncludedProducts,		-- param64
	bundleAlreadyOwned,			-- param65
	bundleIDs,					-- param66
	packageCurrencyToCoins,		-- param67
	grabBag,					-- param68
	grabBagTheme,				-- param69
	grabBagIDs,					-- param70
	grabBagTypes,				-- param71
	grabBagLocalPaths,			-- param72
	grabBagProductNames,		-- param73
	vaultCategory74,			-- param74
	vaultCategory75,			-- param75
	productEnhancements,		-- param76
	productEnhancementIDs,		-- param77
	product_id,                 -- param78
	vaultCategory76  			-- param79
)
	if not GetCvarBool('cg_store2_') then return end
	if AtoN(responseCode) ~= STORE2_RESPONSE_CODE_BASICITEMLIST then return end
	if categoryID ~= HoN_Store.ProductPageInfo[cat]["categoryID"] then return end
	if HoN_Store:Store2GetCurrentPage() == 'store2_specials' then return end

	HoN_Store:Store2SetCurrentPage(cat)

	local lists = HoN_Store.ProductPageInfo[cat]
	local currentPage = lists.currentPage

	local updatePageInfo = true
	local buyOthersResult = false

	-- special case for buy 'others' good result
	if AtoN(popupCode) == STORE2_POPUPCODE_BUYITEM and
		cat == 'others' and VanityBoothSelectInfo["cat"] == 'others' then
		buyOthersResult = true
		updatePageInfo = tostring(currentPage) == tostring(VanityBoothSelectInfo["Page"])
	end

	local count = 0
	if updatePageInfo then

		if cat == 'accounticons' then
			local customIconId = '464'
			local customIconIndex = HoN_Store:GetIndex(split(productIDs, '|'), customIconId)
			if customIconIndex ~= -1 then
				productIDs = HoN_Store:BringIthItemFirst(productIDs, customIconIndex)
				productCodes = HoN_Store:BringIthItemFirst(productCodes, customIconIndex)
				productPrices = HoN_Store:BringIthItemFirst(productPrices, customIconIndex)
				premium_mmp_cost = HoN_Store:BringIthItemFirst(premium_mmp_cost, customIconIndex)
				productAlreadyOwned = HoN_Store:BringIthItemFirst(productAlreadyOwned, customIconIndex)
				purchasable = HoN_Store:BringIthItemFirst(purchasable, customIconIndex)
				productLocalContent = HoN_Store:BringIthItemFirst(productLocalContent, customIconIndex)
				productIsBundle = HoN_Store:BringIthItemFirst(productIsBundle, customIconIndex)
				productCharges = HoN_Store:BringIthItemFirst(productCharges, customIconIndex)
				productDurations = HoN_Store:BringIthItemFirst(productDurations, customIconIndex)
				chargesRemaining = HoN_Store:BringIthItemFirst(chargesRemaining, customIconIndex)
			end
		end

		lists.productIDs 	= split(productIDs, '|')
		lists.productCodes 	= split(productCodes, '|')
		lists.productPrices = split(productPrices, '|')
		lists.premiumMmpCost= split(premium_mmp_cost, '|')
		lists.owned 		= split(productAlreadyOwned, '|')
		lists.purchasable 	= split(purchasable, '|')
		lists.images 		= split(productLocalContent, '|')
		lists.bundle 		= split(productIsBundle, '|')
		lists.productCharges	= split(productCharges, '|')
		lists.productDurations	= split(productDurations, '|')
		lists.chargesRemaining	= split(chargesRemaining, '|')
		lists.totalPages	= tonumber(totalPages)

		if lists.typeStr then
			lists.Eligibility = HoN_Store:AdjustProductPrices(lists.typeStr,
					lists.productIDs, lists.productPrices, lists.premiumMmpCost, productEligibility, true, true)
		end

		local isBundleList = cat =='bundles'
		if isBundleList then
			lists.grabBag = split(vaultCategory74, '|')
		end
		
		count = #lists.productIDs
		if lists.fetchAllPages then
			if #HoN_Store.SharedSortedIndexes ~= count then
				HoN_Store.SharedSortedIndexes = CreateIntArray(1, count)
			end
			HoN_Store:OnVanityConditionChange(cat, 'all', lists.FilterString, true)	-- will call InitializeProductShow()
		else
			HoN_Store:InitializeProductShow(cat, lists, currentPage)
		end
	end

	local endLoading = true
	if HoN_Store.InitializeProductShowCallback ~= nil then
		local t = HoN_Store.InitializeProductShowCallback()
		HoN_Store.InitializeProductShowCallback = nil
		if t and t.noEndLoading then endLoading = false end
	elseif HasBooth(cat) then
		local catPageChanged = cat ~= VanityBoothSelectInfo["cat"]
		local refreshPage = tostring(refreshCurrentVanityPageReqTime) == requestHostTime
		if catPageChanged or refreshPage then
			if count > 0 and AtoN(popupCode) == STORE2_POPUPCODE_NONE then
				HoN_Store:VanityItemOnClick(true, cat, 1, 1)
			end
		else
			-- buy item result
			if AtoN(popupCode) == STORE2_POPUPCODE_BUYITEM then
				local code = VanityBoothSelectInfo["Code"]
				if code ~= nil and code ~= '' then
					if buyOthersResult then
						local itemID = VanityBoothSelectInfo["productID"]
						itemID = tonumber(itemID)
						local shouldShowAsForm = itemID == 161 or itemID == 162 or itemID == 163 or itemID == 492
						if shouldShowAsForm then
							-- copy form BoothInitializeOthersDetail()
							local owned = true
							GetWidget('store2OthersBuyContainer'):SetVisible(not owned)
							GetWidget('store2OthersNeedCoinsBtn'):SetVisible(not owned)
							GetWidget('store2OthersBtnChangeNickname'):SetVisible(owned and itemID == 161)
							if not GetCvarBool('cl_GarenaEnable') then
								GetWidget('store2OthersBtnCreateSubaccount'):SetVisible(owned and itemID == 162)
							end
							GetWidget('store2OthersBtnResetStats'):SetVisible(owned and itemID == 163)

							local wdgPanel
							if itemID == 161 then wdgPanel = GetWidget('storePopupNameChangeForm_others')
							elseif itemID == 162 then wdgPanel = GetWidget('storePopupSubAccountForm_others')
							elseif itemID == 163 then wdgPanel = GetWidget('storePopupResetStatsForm_others')
							end
							if wdgPanel then
								ChangeChildrenStytle(wdgPanel, HoN_Store:IsVaultOpened(), owned)
							end
						elseif itemID == 655 then
							-- tokens can be bought multi times
						else
							OnVanityBoothItemBecomeOwned(cat)
						end
					else
						RefreshUpgrades()
					end
				end
			end

			if VanityBoothSelectInfo["Effect"] ~= nil then
				local samePage = currentPage == VanityBoothSelectInfo["Page"]
				VanityBoothSelectInfo["Effect"]:SetVisible(samePage)
			end
		end
	end

	if endLoading then
		HoN_Store:Store2LoadingEndAsync("GotProductShowDone")
	end
end

local function PrintMicroStoreResuts(self,
	responseCode,				-- param0
	popupCode,					-- param1
	productIDs,					-- param2
	productNames,				-- param3
	productPrices,				-- param4
	productAlreadyOwned,		-- param5
	productIsBundle,			-- param6
	productQuantity,			-- param7
	totalPages,					-- param8
	totalPoints,				-- param9
	productWebContent,			-- param10
	productDescription,			-- param11
	productLocalContent,		-- param12
	categoryID,					-- param13
	currentPage,				-- param14
	errorCode,					-- param15
	packagePrices,				-- param16
	packagePoints,				-- param17
	packageIDs,					-- param18
	packageSpecial,				-- param19
	productCodes,				-- param20
	vaultCategory2,				-- param21
	vaultCategory3,				-- param22
	vaultCategory4,				-- param23
	vaultCategory5,				-- param24
	vaultCategory6,				-- param25
	bundleContents,				-- param26
	selectedUpgrades,			-- param27
	vaultCategory16,			-- param28
	requestHostTime,			-- param29
	accountIconsUnlocked,		-- param30
	unlockAccountIconsCost,		-- param31
	vaultCategory27,			-- param32
	productPremium,				-- param33
	totalMMPoints,				-- param34
	premium_mmp_cost,			-- param35
	regional_currency,			-- param36
	promoCode,					-- param37
	vaultCategory56,			-- param38
	vaultCategory57,			-- param39
	vaultHighlight,				-- param40
	tauntUnlocked,				-- param41
	tauntUnlockCost,			-- param42
	customAccountIcon,			-- param43
	customAccountIconCost,		-- param44
	customAccountIconCostMMP,	-- param45
	timestamp,					-- param46
	productTimes,				-- param47
	vaultCategory72,			-- param48
	is_newly_verified,			-- param49
	purchasable,				-- param50
	productCharges,				-- param51
	productDurations,			-- param52
	chargesRemaining,			-- param53
	unlockAccountIconsCostMMP,	-- param54
	specialBundles,				-- param55
	tauntUnlockCostMMP,			-- param56
	specialDisplay,				-- param57
	santas,						-- param58
	productEligibility,			-- param59
	packageTextures,			-- param60
	bundleNames,				-- param61
	bundleCosts,				-- param62
	bundleLocalPaths,			-- param63
	bundleIncludedProducts,		-- param64
	bundleAlreadyOwned,			-- param65
	bundleIDs,					-- param66
	packageCurrencyToCoins,		-- param67
	grabBag,					-- param68
	grabBagTheme,				-- param69
	grabBagIDs,					-- param70
	grabBagTypes,				-- param71
	grabBagLocalPaths,			-- param72
	grabBagProductNames,		-- param73
	vaultCategory74,			-- param74
	vaultCategory75,			-- param75
	productEnhancements,		-- param76
	productEnhancementIDs,		-- param77
	product_id,                 -- param78
	vaultCategory76  			-- param79
)

	if not GetCvarBool('cg_MonitorStoreResult') then return end
	Echo("^g0: ^y" .."responseCode")
	Echo(responseCode)
	Echo("^g1: ^y" .."popupCode")
	Echo(popupCode)
	Echo("^g2: ^y" .."productIDs")
	Echo(productIDs)
	Echo("^g3: ^y" .."productNames")
	Echo(productNames)
	Echo("^g4: ^y" .."productPrices")
	Echo(productPrices)
	Echo("^g5: ^y" .."productAlreadyOwned")
	Echo(productAlreadyOwned)
	Echo("^g6: ^y" .."productIsBundle")
	Echo(productIsBundle)
	Echo("^g7: ^y" .."productQuantity")
	Echo(productQuantity)
	Echo("^g8: ^y" .."totalPages")
	Echo(totalPages)
	Echo("^g9: ^y" .."totalPoints")
	Echo(totalPoints)
	Echo("^g10: ^y" .."productWebContent")
	Echo(productWebContent)
	Echo("^g11: ^y" .."productDescription")
	Echo(productDescription)
	Echo("^g12: ^y" .."productLocalContent")
	Echo(productLocalContent)
	Echo("^g13: ^y" .."categoryID")
	Echo(categoryID)
	Echo("^g14: ^y" .."currentPage")
	Echo(currentPage)
	Echo("^g15: ^y" .."errorCode")
	Echo(errorCode)
	Echo("^g16: ^y" .."packagePrices")
	Echo(packagePrices)
	Echo("^g17: ^y" .."packagePoints")
	Echo(packagePoints)
	Echo("^g18: ^y" .."packageIDs")
	Echo(packageIDs)
	Echo("^g19: ^y" .."packageSpecial")
	Echo(packageSpecial)
	Echo("^g20: ^y" .."productCodes")
	Echo(productCodes)
	Echo("^g21: ^y" .."vaultCategory2")
	Echo(vaultCategory2)
	Echo("^g22: ^y" .."vaultCategory3")
	Echo(vaultCategory3)
	Echo("^g23: ^y" .."vaultCategory4")
	Echo(vaultCategory4)
	Echo("^g24: ^y" .."vaultCategory5")
	Echo(vaultCategory5)
	Echo("^g25: ^y" .."vaultCategory6")
	Echo(vaultCategory6)
	Echo("^g26: ^y" .."bundleContents")
	Echo(bundleContents)
	Echo("^g27: ^y" .."selectedUpgrades")
	Echo(selectedUpgrades)
	Echo("^g28: ^y" .."vaultCategory16")
	Echo(vaultCategory16)
	Echo("^g29: ^y" .."requestHostTime")
	Echo(requestHostTime)
	Echo("^g30: ^y" .."accountIconsUnlocked")
	Echo(accountIconsUnlocked)
	Echo("^g31: ^y" .."unlockAccountIconsCost")
	Echo(unlockAccountIconsCost)
	Echo("^g32: ^y" .."vaultCategory27")
	Echo(vaultCategory27)
	Echo("^g33: ^y" .."productPremium")
	Echo(productPremium)
	Echo("^g34: ^y" .."totalMMPoints")
	Echo(totalMMPoints)
	Echo("^g35: ^y" .."premium_mmp_cost")
	Echo(premium_mmp_cost)
	Echo("^g36: ^y" .."regional_currency")
	Echo(regional_currency)
	Echo("^g37: ^y" .."promoCode")
	Echo(promoCode)
	Echo("^g38: ^y" .."vaultCategory56")
	Echo(vaultCategory56)
	Echo("^g39: ^y" .."vaultCategory57")
	Echo(vaultCategory57)
	Echo("^g40: ^y" .."vaultHighlight")
	Echo(vaultHighlight)
	Echo("^g41: ^y" .."tauntUnlocked")
	Echo(tauntUnlocked)
	Echo("^g42: ^y" .."tauntUnlockCost")
	Echo(tauntUnlockCost)
	Echo("^g43: ^y" .."customAccountIcon")
	Echo(customAccountIcon)
	Echo("^g44: ^y" .."customAccountIconCost")
	Echo(customAccountIconCost)
	Echo("^g45: ^y" .."customAccountIconCostMMP")
	Echo(customAccountIconCostMMP)
	Echo("^g46: ^y" .."timestamp")
	Echo(timestamp)
	Echo("^g47: ^y" .."productTimes")
	Echo(productTimes)
	Echo("^g48: ^y" .."vaultCategory72")
	Echo(vaultCategory72)
	Echo("^g49: ^y" .."is_newly_verified")
	Echo(is_newly_verified)
	Echo("^g50: ^y" .."purchasable")
	Echo(purchasable)
	Echo("^g51: ^y" .."productCharges")
	Echo(productCharges)
	Echo("^g52: ^y" .."productDurations")
	Echo(productDurations)
	Echo("^g53: ^y" .."chargesRemaining")
	Echo(chargesRemaining)
	Echo("^g54: ^y" .."unlockAccountIconsCostMMP")
	Echo(unlockAccountIconsCostMMP)
	Echo("^g55: ^y" .."specialBundles")
	Echo(specialBundles)
	Echo("^g56: ^y" .."tauntUnlockCostMMP")
	Echo(tauntUnlockCostMMP)
	Echo("^g57: ^y" .."specialDisplay")
	Echo(specialDisplay)
	Echo("^g58: ^y" .."santas")
	Echo(santas)
	Echo("^g59: ^y" .."productEligibility")
	Echo(productEligibility)
	Echo("^g60: ^y" .."packageTextures")
	Echo(packageTextures)
	Echo("^g61: ^y" .."bundleNames")
	Echo(bundleNames)
	Echo("^g62: ^y" .."bundleCosts")
	Echo(bundleCosts)
	Echo("^g63: ^y" .."bundleLocalPaths")
	Echo(bundleLocalPaths)
	Echo("^g64: ^y" .."bundleIncludedProducts")
	Echo(bundleIncludedProducts)
	Echo("^g65: ^y" .."bundleAlreadyOwned")
	Echo(bundleAlreadyOwned)
	Echo("^g66: ^y" .."bundleIDs")
	Echo(bundleIDs)
	Echo("^g67: ^y" .."packageCurrencyToCoins")
	Echo(packageCurrencyToCoins)
	Echo("^g68: ^y" .."grabBag")
	Echo(grabBag)
	Echo("^g69: ^y" .."grabBagTheme")
	Echo(grabBagTheme)
	Echo("^g70: ^y" .."grabBagIDs")
	Echo(grabBagIDs)
	Echo("^g71: ^y" .."grabBagTypes")
	Echo(grabBagTypes)
	Echo("^g72: ^y" .."grabBagLocalPaths")
	Echo(grabBagLocalPaths)
	Echo("^g73: ^y" .."grabBagProductNames")
	Echo(grabBagProductNames)
	Echo("^g74: ^y" .."vaultCategory74")
	Echo(vaultCategory74)
	Echo("^g75: ^y" .."vaultCategory75")
	Echo(vaultCategory75)
	Echo("^g76: ^y" .."productEnhancements")
	Echo(productEnhancements)
	Echo("^g77: ^y" .."productEnhancementIDs")
	Echo(productEnhancementIDs)
	Echo("^g78: ^y" .."product_id")
	Echo(product_id)
	Echo("^g79: ^y" .."vaultCategory76")
	Echo(vaultCategory76)

end

function HoN_Store:GetVaultDataCount(cat)
	local data = HoN_Store.Store2VaultData[cat]
	if data == nil then return 0 end
	return #data
end

local function prependCategoryData(categoryData, toPrepend, prependCondition)
	if categoryData and type(categoryData) == 'string' then

		if prependCondition ~= nil and ( (type(prependCondition) == 'function' and (not prependCondition())) or (type(prependCondition) == 'boolean' and prependCondition == false) ) then
			toPrepend = ''
		else
			if string.len(categoryData) > 0 then
				toPrepend = toPrepend .. '|'
			end
		end

		return toPrepend .. categoryData
	end
end

local function Store2ReceiveVaultData(self,
	responseCode,				-- param0
	popupCode,					-- param1
	productIDs,					-- param2
	productNames,				-- param3
	productPrices,				-- param4
	productAlreadyOwned,		-- param5
	productIsBundle,			-- param6
	productQuantity,			-- param7
	totalPages,					-- param8
	totalPoints,				-- param9
	productWebContent,			-- param10
	productDescription,			-- param11
	productLocalContent,		-- param12
	categoryID,					-- param13
	currentPage,				-- param14
	errorCode,					-- param15
	packagePrices,				-- param16
	packagePoints,				-- param17
	packageIDs,					-- param18
	packageSpecial,				-- param19
	productCodes,				-- param20
	vaultCategory2,				-- param21
	vaultCategory3,				-- param22
	vaultCategory4,				-- param23
	vaultCategory5,				-- param24
	vaultCategory6,				-- param25
	bundleContents,				-- param26
	selectedUpgrades,			-- param27
	vaultCategory16,			-- param28
	requestHostTime,			-- param29
	accountIconsUnlocked,		-- param30
	unlockAccountIconsCost,		-- param31
	vaultCategory27,			-- param32
	productPremium,				-- param33
	totalMMPoints,				-- param34
	premium_mmp_cost,			-- param35
	regional_currency,			-- param36
	promoCode,					-- param37
	vaultCategory56,			-- param38
	vaultCategory57,			-- param39
	vaultHighlight,				-- param40
	tauntUnlocked,				-- param41
	tauntUnlockCost,			-- param42
	customAccountIcon,			-- param43
	customAccountIconCost,		-- param44
	customAccountIconCostMMP,	-- param45
	timestamp,					-- param46
	productTimes,				-- param47
	vaultCategory72,			-- param48
	is_newly_verified,			-- param49
	purchasable,				-- param50
	productCharges,				-- param51
	productDurations,			-- param52
	chargesRemaining,			-- param53
	unlockAccountIconsCostMMP,	-- param54
	specialBundles,				-- param55
	tauntUnlockCostMMP,			-- param56
	specialDisplay,				-- param57
	santas,						-- param58
	productEligibility,			-- param59
	packageTextures,			-- param60
	bundleNames,				-- param61
	bundleCosts,				-- param62
	bundleLocalPaths,			-- param63
	bundleIncludedProducts,		-- param64
	bundleAlreadyOwned,			-- param65
	bundleIDs,					-- param66
	packageCurrencyToCoins,		-- param67
	grabBag,					-- param68
	grabBagTheme,				-- param69
	grabBagIDs,					-- param70
	grabBagTypes,				-- param71
	grabBagLocalPaths,			-- param72
	grabBagProductNames,		-- param73
	vaultCategory74,			-- param74
	vaultCategory75,			-- param75
	productEnhancements,		-- param76
	productEnhancementIDs,		-- param77
	product_id,                 -- param78
	vaultCategory76,  			-- param79
	vaultCategory77,			-- param80
	vaultCategory78,            -- param81
	vaultCategory79  			-- param82
)

	local function ParseVaultData(data)
		local parsed = {}

		if Empty(data) then
			return parsed
		end

		local invalidCounter = 0
		for index, singleVaultString in ipairs(split(data, '|')) do
			local values = split(singleVaultString, '`')
			local singleVault = {}
			singleVault['itemid'] = values[1]
			singleVault['code'] = values[2]
			singleVault['localContent'] = values[3]
			singleVault['category'] = values[4]
			singleVault['chargesRemaining'] = values[5]
			singleVault['duration'] = values[6]
			singleVault['startTime'] = values[7]
			singleVault['endTime'] = values[8]

			if tonumber(singleVault['itemid']) ~= nil  then
				parsed[index - invalidCounter] = singleVault
			elseif singleVault['localContent'] == 'web' then
				parsed[index - invalidCounter] = singleVault
			else
				Echo("^rInvalidVaultData: "..singleVaultString)
				invalidCounter = invalidCounter + 1
			end
		end

		return parsed
	end

	local function Store2ReceiveVaultData_VaultList()
		vaultCategory4 = prependCategoryData(vaultCategory4, '999999002`cs`/ui/legion/ability_coverup.tga`Chat Symbol')
		vaultCategory5 = prependCategoryData(vaultCategory5, '999999003`av`/ui/legion/ability_coverup.tga`Alt Announcement')
		vaultCategory6 = prependCategoryData(vaultCategory6, '999999005`misc`/ui/fe2/store/icons/hero_token.tga`Miscellaneous',
							AtoN(interface:UICmd("AccountStanding")) ~= 3
							and (HoN_Region.regionTable[HoN_Region.activeRegion].hasTokens or HoN_Region.regionTable[HoN_Region.activeRegion].hasPasses))
		vaultCategory57 = prependCategoryData(vaultCategory57, '999999007`c`/shared/automated_courier/icon.tga`Couriers')
		vaultCategory74 = prependCategoryData(vaultCategory74, '999999006`w`/ui/legion/ability_coverup.tga`Custom Ward')
		vaultCategory76 = prependCategoryData(vaultCategory76, '999999008`cr`/ui/legion/ability_coverup.tga`Custom Creep')
		vaultCategory77 = prependCategoryData(vaultCategory77, '999999009`tb`/ui/legion/ability_coverup.tga`Custom TauntBadge')
		vaultCategory78 = prependCategoryData(vaultCategory78, '999999010`te`/ui/legion/ability_coverup.tga`Custom TP Effect')
		vaultCategory79 = prependCategoryData(vaultCategory79, '999999011`sc`/ui/legion/ability_coverup.tga`Custom Selection Circle')

		HoN_Store.Store2VaultData['vault_accounticons'] = ParseVaultData(vaultCategory3)
		HoN_Store.Store2VaultData['vault_namecolors'] = ParseVaultData(vaultCategory16)
		HoN_Store.Store2VaultData['vault_symbols'] = ParseVaultData(vaultCategory4)
		HoN_Store.Store2VaultData['vault_announcers'] = ParseVaultData(vaultCategory5)
		HoN_Store.Store2VaultData['vault_couriers'] = ParseVaultData(vaultCategory57)
		HoN_Store.Store2VaultData['vault_taunts'] = ParseVaultData(vaultCategory27)
		HoN_Store.Store2VaultData['vault_tauntbadges'] = ParseVaultData(vaultCategory77)
		HoN_Store.Store2VaultData['vault_tpeffects'] = ParseVaultData(vaultCategory78)
		HoN_Store.Store2VaultData['vault_selectioncircles'] = ParseVaultData(vaultCategory79)
		HoN_Store.Store2VaultData['vault_wards'] = ParseVaultData(vaultCategory74)
		HoN_Store.Store2VaultData['vault_upgrades'] = ParseVaultData(vaultCategory75)
		HoN_Store.Store2VaultData['vault_creeps'] = ParseVaultData(vaultCategory76)
		HoN_Store.Store2VaultData['vault_others'] = ParseVaultData(vaultCategory6)
		HoN_Store.Store2VaultData['selected_vault_items'] = split(selectedUpgrades, '|')

		local lastVaultPage
		if VaultItemJumpInfo['productId'] == nil or VaultItemJumpInfo['category'] == nil then
			if not vaultEverShown then
				vaultEverShown = true
				lastVaultPage = 'vault_accounticons'
			else
				lastVaultPage	= HoN_Store:Store2GetCurrentPage('vault')
			end
		else
			lastVaultPage	= VaultItemJumpInfo['category']
			vaultEverShown = true
		end

		local t = Store2NavBackInProgress
		local boothInfo = nil
		local asyncNavBackResult = Store2NavBackLastTime == requestHostTime
		if asyncNavBackResult then
			Store2NavBackInProgress = true
			boothInfo = CopyTable(VanityBoothSelectInfo)
			lastVaultPage = VanityBoothSelectInfo.cat
		end

		HoN_Store:Store2ShowPage(lastVaultPage)

		if asyncNavBackResult then	

			VanityBoothSelectInfo = boothInfo
			local cat = VanityBoothSelectInfo.cat
			local currentPage = VanityBoothSelectInfo.currentPage

			VanityBoothSelectInfo.currentPage = nil

			HoN_Store:Store2InitializeVaultUI(cat, currentPage, false)

			if HasBooth(cat) then
				HoN_Store:InitializeVanityBoothUI(true, 'NavBackToVanity', VanityBoothSelectInfo)

				local countPerPage = HoN_Store.paging[cat].countPerPage
				local productID = tostring(VanityBoothSelectInfo.productID)

				local row, col = 1, -1, -1
				local vaultData = HoN_Store.Store2VaultData[cat]
				for i, v in ipairs(vaultData) do
					if v.itemid == productID then
						local page = math.floor((i - 1) / countPerPage) + 1
						index = i - (page - 1) * countPerPage
						local countPerRow = HoN_Store.ProductPageInfo[cat].elementPerRow
						row = math.floor((index - 1) / countPerRow) + 1
						col = index - (row - 1) * countPerRow
						break
					end
				end
				
				if row > 0 and col > 0 then
					local w = GetWidget("store2_vanity_frame_"..cat.."_"..row.."_"..col.."_effect")
					VanityBoothSelectInfo["Vault_Effect"] = w
				end

				if tostring(VanityBoothSelectInfo.Vault_Page) == tostring(currentPage) then
					VanityBoothSelectInfo["Vault_Effect"] :SetVisible(true)
				end
			end
			Store2NavBackInProgress = t
		end

		HoN_Store:Store2LoadingEndAsync("GotVaultData")

		RefreshUpgrades()
	end

	local function Store2ReceiveVaultData_UpdateSelected()
		HoN_Store.Store2VaultData['selected_vault_items'] = split(selectedUpgrades, '|')
		local vaultName = GetStore2CurrentTab()

		if IsVault(vaultName) then
			local currentPage = HoN_Store:Store2GetCurrentPage()
			if HoN_Store.paging[currentPage] == nil then
				Echo('^rError: Store2ReceiveVaultData_UpdateSelected Failed to find page info: '..currentPage)
				return
			end

			local page = HoN_Store.paging[currentPage].currentPage
			HoN_Store:Store2InitializeVaultUI(currentPage, page, true)
		end
	end

	if not GetCvarBool('cg_store2_') then return end
	if AtoN(responseCode) == STORE2_RESPONSE_CODE_VAULTITEMLIST then
		Store2ReceiveVaultData_VaultList()
	elseif AtoN(responseCode) == STORE2_RESPONSE_CODE_UPDATE_SELECTED then
		Store2ReceiveVaultData_UpdateSelected()
	end
end

interface:RegisterWatch('MicroStoreResults', function(...) PrintMicroStoreResuts(...) Store2ReceiveVaultData(...) end)

function ToggleOthersDetailStyle(widget)
	local isVault = HoN_Store:IsVaultOpened()
	widget:SetColor(isVault and '#778c83' or '#d7b6a2')
	widget:SetOutlineColor(isVault and '#0b1a1a' or '#241410')
end

local function UpdateVaultItemPage()
	if VaultItemJumpInfo['productId'] ~= nil and VaultItemJumpInfo['productId'] ~= '' and VaultItemJumpInfo['category'] ~= nil then
		local productId = VaultItemJumpInfo['productId']
		local category = VaultItemJumpInfo['category']
		local vault = HoN_Store.Store2VaultData[category]
		local elementPerPage = Store2GetMaxElementPerPage(category)
		local elementPerRow = HoN_Store.ProductPageInfo[category]["elementPerRow"]
		local dataIdxOffset = 0
		for idx, singleVault in ipairs(vault) do
			if tostring(singleVault['itemid']) == tostring(productId) then
				dataIdxOffset = idx
				break
			end
		end
		Echo('dataIdxOffset:'..tostring(dataIdxOffset))
		if dataIdxOffset == 0 then
			return
		end
		local page = math.floor((dataIdxOffset - 1) / elementPerPage) + 1
		local pageIdxOffset = (dataIdxOffset - 1) % elementPerPage
		local row = math.floor(pageIdxOffset / elementPerRow) + 1
		local col = pageIdxOffset % elementPerRow + 1
		HoN_Store.ProductPageInfo[category].currentPage = page

		Echo('^g offset: '..tostring(pageIdxOffset)..' row: '..tostring(row)..' col: '..tostring(col)..' page: '..tostring(page))

		GetWidget('store_container2'):Sleep(1, function(self)
				if HasBooth(category) then
					HoN_Store:VanityItemOnClick(true, category, row, col)
				end
				VaultItemJumpInfo['productId'] = nil
				VaultItemJumpInfo['category'] = nil
		end)
	else
		VaultItemJumpInfo['productId'] = nil
		VaultItemJumpInfo['category'] = nil
	end
end

local function UpdateVaultSelectedItem(cat)
	local function IsVaultItemSelected(itemID)
		for key, value in pairs(HoN_Store.Store2VaultData['selected_vault_items']) do
			if itemID == value then return true end
		end
		return false
	end

	local vault = HoN_Store.Store2VaultData[cat]
	local hasSelected = false
	for idx, singleVault in ipairs(vault) do
		if IsVaultItemSelected(singleVault['itemid']) then
			singleVault['isSelected'] = true
			hasSelected = true
		else
			singleVault['isSelected'] = false
		end
	end

	-- Selected means it is chosen to use in game.
	if not hasSelected then
		if vault[1] ~= nil then
			local idInt = tonumber(vault[1]['itemid'])
			-- 999999005 is in "others" that is not selectable
			if idInt >= 999999002 and idInt <= 999999011 and idInt ~= 999999005 then
				vault[1]['isSelected'] = true
			end
		end
	end
end

function HoN_Store:Store2InitializeVaultUI(cat, page, fromPagging)

	HoN_Store:ClearVanityPage(cat)

	local vault = HoN_Store.Store2VaultData[cat]

	if vault == nil then
		Echo("^rDebug: fatal error: vault is nil @HoN_Store:Store2InitializeVaultUI "..tostring(cat))
	end

	HoN_Store:Store2SetCurrentPage(cat)

	UpdateVaultItemPage()

	UpdateVaultSelectedItem(cat)
	HoN_Store.ProductPageInfo[cat].currentPage = page or HoN_Store.ProductPageInfo[cat].currentPage or 1

	local dataIdxOffset = 0
	if HoN_Store.ProductPageInfo[cat].currentPage ~= nil then
		local elementsPerPage = Store2GetMaxElementPerPage(cat)
		dataIdxOffset = elementsPerPage * (HoN_Store.ProductPageInfo[cat].currentPage - 1)
	end

	local elementPerPage = Store2GetMaxElementPerPage(cat)

	for idx=1, elementPerPage do
		local w = nil
		local elementPerRow = HoN_Store.ProductPageInfo[cat]["elementPerRow"]
		local row = math.floor((idx-1) / elementPerRow) + 1;
		local col = (idx-1) % elementPerRow + 1
		local widgetPrefix = 'store2_vanity_frame_'..cat..'_'..row..'_'..col

		local dataIndex = idx + dataIdxOffset
		local single = vault[dataIndex]

		if not single or DataNotExist(single['localContent']) then
			break
		end

		GetWidget(widgetPrefix..'_empty'):SetVisible(0)
		GetWidget(widgetPrefix..'_content'):SetVisible(1)

		w = GetWidget(widgetPrefix..'_img')
		if single['localContent'] == 'web' then
			local clientID = Client.GetAccountID()
			local url = GetICBURL()..'/'..'icons'..'/'..
							math.floor(clientID / 1000000)..'/'..
							math.floor((clientID - (math.floor(clientID / 1000000) * 1000000)) / 1000)..
							'/'..clientID..'/'..split(single['code'], ':')[2]..'.cai'
			w:SetTextureURL(url)
		else
			w:SetTexture(single['localContent'])
		end

		w = GetWidget(widgetPrefix..'_name')

		local string name = ''
		if single and single['localContent'] == 'web' then
			name = Translate('mstore_product_custom_icon_name')
		elseif single and not DataNotExist(single['itemid']) then
			name = GetProductDispayNameByID(single['itemid'])
		end
		VanityFrameSetLimitedText(w, cat, row, col, name)

		w = GetWidget(widgetPrefix..'_selected')
		if single['itemid'] == '464' then
			w:SetText(Translate('mstore_icon_await_upload'))
			w:SetVisible(true)
		else
			w:SetText(Translate('store2_vault_selected'))
			w:SetVisible(single['isSelected'])
		end

		w = GetWidget(widgetPrefix..'_price')
		w:SetVisible(0)

		-- gca
		HoN_Store:ShowGcaImageInNeed(nil, widgetPrefix..'_gca', single['code'])
	end

	local productCount = #vault
	local currentPage = HoN_Store.ProductPageInfo[cat].currentPage

	local totalPages = productCount / elementPerPage;
	if  productCount % elementPerPage ~= 0  then totalPages = totalPages + 1 end

	UpdatePaging(cat, currentPage, productCount, 11)
	HoN_Store:InitializePaging2Dots(cat, currentPage, totalPages)

	if HasBooth(cat) then
		if fromPagging then
			if VanityBoothSelectInfo["Vault_Effect"] ~= nil then
				local samePage = currentPage == VanityBoothSelectInfo["Vault_Page"]
				VanityBoothSelectInfo["Vault_Effect"]:SetVisible(samePage)
			end
		elseif not Store2NavBackInProgress and productCount > (currentPage - 1) * elementPerPage then
			HoN_Store:VanityItemOnClick(true, cat, 1, 1)
		end
	end
end

local function ChooseVaultItemToUse(cat, row, col)
	if GetVaultVanityItemProductID(cat, row, col) == '464' then
		return
	end

	local code = GetVaultVanityItemCode(cat, row, col)
	code=string.gsub(code,"'","\\'")

	local cmd
	if code == 'ai' or code == 'cs' or code == 'av' or code == 'cc' or code == 'w' or code == 'c' or code == 'cr' or code=='tb' or code=='te' or code=='sc' then
		cmd = "ClearUpgrade('"..code.."')"
	else
		cmd = "SelectUpgrade('"..code.."')"
	end
	interface:UICmd(cmd)
end

function HoN_Store:VanityItemOnClick(hasBooth, category, row, col)

	--Echo('HoN_Store:VanityItemOnClick category:'..category..', row/col='..tostring(row)..'/'..tostring(col))
	local function SetPurchaseParam(cat, row, col)
		if not IsVault(cat) then
			local idx = Store2VanityGetIndex(cat, row, col)
			Set('_microStore_SelectedID', GetCvarString('microStoreID'..idx))
			Set('_microStore_SelectedItem', idx)
			Set('_microStoreSelectedHeroItem', '')

			VaultItemJumpInfo['category'] = category
		end
	end

	-- purchase param
	SetPurchaseParam(category, row, col)

	if hasBooth then

		-- if anyone is selected before, recover it
		if IsVault(category) then
			if VanityBoothSelectInfo["Vault_Effect"] ~= nil then
				VanityBoothSelectInfo["Vault_Effect"]:SetVisible(0)
				VanityBoothSelectInfo["Vault_Effect"] = nil
			end
		else
			if VanityBoothSelectInfo["Effect"] ~= nil then
				VanityBoothSelectInfo["Effect"]:SetVisible(0)
				VanityBoothSelectInfo["Effect"] = nil
			end
		end

		-- select the right one now
		if row ~= nil and col ~= nil then
			local w = GetWidget("store2_vanity_frame_"..category.."_"..row.."_"..col.."_effect")
			w:SetVisible(1)
			if IsVault(category) then
				VanityBoothSelectInfo["Vault_Effect"] = w
			else
				VanityBoothSelectInfo["Effect"] = w
			end
		end

		Trigger('CreepsTypeSelected')
		HoN_Store:InitializeVanityBoothUI(true, category, {["row"] = row, ["col"] = col})
	else
		if IsVault(category) then
			ChooseVaultItemToUse(category, row, col)
		else
			-- buy this vanity item
			HoN_Store:OnPurchaseVanity(category, row, col)
		end
	end
end

function HoN_Store:AnnouncersTypeOnClick(sound, model)

	local productID = VanityBoothSelectInfo["productID"]

	if productID == nil then
		Echo('^rAnnouncersTypeOnClick() nil productID')
		return
	elseif HoN_Store.VoiceAnnouncersTable[productID]== nil then
		Echo('^rAnnouncersTypeOnClick() nil table entry, productID='..tostring(productID))
		return
	else
		PlaySound('/shared/sounds/announcer/'..HoN_Store.VoiceAnnouncersTable[productID]['voicePack_code']..'/'..sound..'.wav', 1.0, 1)
		local modelPanel = GetWidget('store2_specials_booth_announcers_model_content_vanity')
		modelPanel:SetModel('/ui/common/models/'..HoN_Store.VoiceAnnouncersTable[productID]['voicePack_models']..'/'..model..'.mdf', 1.0, 1)
		modelPanel:SetVisible(true)
		modelPanel:SetAnim('idle')
		modelPanel:SetEffect('/ui/common/models/'..HoN_Store.VoiceAnnouncersTable[productID]['voicePack_models']..'/bloodlust.effect')
		modelPanel:Sleep(3200, function(self)
			self:SetVisible(false)
		end);
	end
end

function HoN_Store:CreepsTypeOnClick(sound, model)

	local productID = VanityBoothSelectInfo["productID"]

	Trigger('CreepsTypeSelected', sound, model)
	local modelPanel = GetWidget('store2_specials_booth_model_vanity')
	modelPanel:SetModelAngles(0, 0, 0)
	modelPanel:SetModel(HoN_Store.CreepsTable[productID][model]['model'], 1.0, 1)
	modelPanel:SetVisible(true)
	modelPanel:SetAnim('idle')
	modelPanel:SetEffect(HoN_Store.CreepsTable[productID][model]['effect'])
	modelPanel:SetModelScale(HoN_Store.CreepsTable[productID][model]['scale'])
end

HoN_Store.currentTauntBadgeID = 0
HoN_Store.currentTauntBadgeImage = ''
HoN_Store.tauntBadgeLoopIndex = 0
function HoN_Store:OnClickSwitchTauntBadge(index)
	local modelPanel = GetWidget('store2_specials_booth_model_vanity')
	local info = HoN_Store.TauntBadgesTable[HoN_Store.currentTauntBadgeID]
	if info == nil or info[index] == nil then return end

	HoN_Store.tauntBadgeLoopIndex = index
	local loopId = HoN_Store.currentTauntBadgeID
	local iconPanel = GetWidget('st2_sp_booth_icon_panel_vanity')
	if info[index].effect ~= nil then
		iconPanel:FadeOut(100)
		modelPanel:SetModel('/heroes/legionnaire/alt9/model.mdf')
		modelPanel:SetModelScale(0.2)
		modelPanel:SetModelPos(0, 0, -50)
		modelPanel:SetAnim('taunt_1')
		modelPanel:SetEffect(info[index].effect)


		modelPanel:Sleep(5000, function()
				if  index == HoN_Store.tauntBadgeLoopIndex and loopId == HoN_Store.currentTauntBadgeID then
					HoN_Store:OnClickSwitchTauntBadge(index)
				end
		end)
	 else
		GetWidget('st2_sp_booth_icon_vanity'):SetTexture(HoN_Store.currentTauntBadgeImage)
		iconPanel:FadeIn(300)
		GetWidget('st2_sp_booth_icon_play_vanity'):SetVisible(0)

		modelPanel:SetModel('')
		modelPanel:SetEffect('')
	end
end

HoN_Store.currentTPEffectID = 0
function HoN_Store:OnClickSwitchTPEffect(index)
	local modelPanel = GetWidget('store2_specials_booth_model_vanity')
	local info = HoN_Store.TPEffectsTable[HoN_Store.currentTPEffectID]
	if info == nil or info[index] == nil then return end

	local effect1 = info[index].effect1 or ''
	local effect2 = info[index].effect2 or ''

	modelPanel:SetEffect(effect1)
	modelPanel:SetMultiEffect(effect2, 1)
end

function HoN_Store:InitializeVanityBoothUI(show, cat, info)
	local id = 'vanity'
	if info and info.id then id = info.id end

	TestEcho('HoN_Store:InitializeVanityBoothUI id: '..id..' category: '..tostring(cat))
	TestEcho('^InitializeVanityBoothUI info=')
	if not GetCvarBool('releaseStage_stable') then
		printTableDeep(info)
	end

	Set('_microStore_SelectedItemOwned', false)

	local boothWidget = GetWidget('store2_specials_booth_' .. id)
	if not boothWidget then Echo("^r Cannot find booth widget for: "..tostring(id)) return end
	boothWidget:SetVisible(show)

	local function HasData(d)
		return d ~= nil and d ~= '' and d ~= 0
	end

	local function BoothInitializeDetailPanel(cat)
		TestEcho('BoothInitializeDetailPanel cat: '..cat)

		local showToggleDetailBtn = cat == 'couriers' or cat == 'wards'
		Set('store2_showModelDetailArrow', tostring(showToggleDetailBtn))
		Set('store2_isBundleModelDetail', tostring(false))

		if IsVault(cat) then
			-- scrollbar
			GetWidget('store2_specials_booth_announcers_preview_listbox_store_vanity'):SetVisible(false)
			GetWidget('store2_specials_booth_announcers_preview_listbox_vault_vanity'):SetVisible(true)
			GetWidget('store2_specials_booth_creeps_preview_listbox_store_vanity'):SetVisible(false)
			GetWidget('store2_specials_booth_creeps_preview_listbox_vault_vanity'):SetVisible(true)
		else
			-- scrollbar
			GetWidget('store2_specials_booth_announcers_preview_listbox_store_vanity'):SetVisible(true)
			GetWidget('store2_specials_booth_announcers_preview_listbox_vault_vanity'):SetVisible(false)
			GetWidget('store2_specials_booth_creeps_preview_listbox_store_vanity'):SetVisible(true)
			GetWidget('store2_specials_booth_creeps_preview_listbox_vault_vanity'):SetVisible(false)
		end

		-- preview
		if cat == 'creeps' or cat == 'vault_creeps' then
			GetWidget('store2_specials_booth_announcers_preview_list_vanity'):SetVisible(false)
			GetWidget('store2_specials_booth_creeps_preview_list_vanity'):SetVisible(true)
			GetWidget('st2_sp_booth_preview_list_panel'):SetVisible(true)
			GetWidget('st2_sp_booth_preview_list_panel'):SetHeight('150')
		elseif cat == 'announcers' or cat == 'vault_announcers' then
			GetWidget('store2_specials_booth_announcers_preview_list_vanity'):SetVisible(true)
			GetWidget('store2_specials_booth_creeps_preview_list_vanity'):SetVisible(false)
			GetWidget('st2_sp_booth_preview_list_panel'):SetVisible(true)
			GetWidget('st2_sp_booth_preview_list_panel'):SetHeight('150')
		else
			GetWidget('st2_sp_booth_preview_list_panel'):SetHeight('1')
			GetWidget('st2_sp_booth_preview_list_panel'):SetVisible(false)
		end
	end

	local function BoothInitializeIcons(modelPanel, imagePath, id)
		GetWidget('st2_sp_booth_icon_'..id):SetTexture(imagePath)
		GetWidget('st2_sp_booth_icon_panel_'..id):FadeIn(300)

		modelPanel:SetRotate(false)
	end

	local function BoothInitializeNameColor(modelPanel, productCode, id)
		if productCode == nil then Echo("^BoothInitializeNameColor got nil productCode") return end

		local name = split(productCode, '%.')[2]
		if name == nil then Echo("Failed to initialize name color for booth: "..tostring(productCode)) return end

		local label = GetWidget('st2_sp_booth_icon_namecolor_'..id)
		label:SetVisible(1)

		local colorProps = GetChatNameColor(name)
		local b8Bit = NotEmpty(colorProps.font) and colorProps.font == '8bit'
		local font = b8Bit and (cat == 'dailySpecial' and '8bit_11' or '8bit_14') or 'dyn_12'
		label:SetFont(font)
		label:SetColor(colorProps.color or 'red')
		label:SetGlow(colorProps.glow or false)
		label:SetGlowColor(colorProps.glowColor or 'red')
		label:SetBackgroundGlow(colorProps.backgroundGlow or false)

		label:SetText(b8Bit and 'NAME COLOR' or 'Name Color')

		modelPanel:SetRotate(false)
	end

	local function BoothInitializeTaunts(modelPanel, id, itemID, imagePath, isVault)
		itemID = tostring(itemID)
		local info = HoN_Store.TauntsTable[itemID]
		if not info then Echo("^rFatal error: Failed to find taunt info for item: "..tostring(itemID)) return end

		local modelPanelTaunt = GetWidget('store2_'..id..'_booth_taunt_modelpanel')
		local iconPanel = GetWidget('st2_sp_booth_icon_panel_'..id)
		if info.effectPath ~= '' then
			iconPanel:FadeOut(100)
			modelPanelTaunt:SetVisible(1)
			modelPanelTaunt:SetEffect(info.effectPath)

			GetWidget('st2_sp_booth_icon_play_vanity'):SetVisible(1)
			GetWidget('st2_sp_booth_icon_play_over_vanity'):SetCallback('onclick', function(...) BoothInitializeTaunts(modelPanel, id, itemID, imagePath, isVault) end)

			GetWidget('st2_sp_booth_icon_play_over_vanity'):RegisterWatch("EffectFinished",
				function(self, modelPanelName)
					if modelPanelName == modelPanelTaunt:GetName() then
						GetWidget('st2_sp_booth_icon_'..id):SetTexture(imagePath)
						iconPanel:FadeIn(300)
					end
				end)
		else
			GetWidget('st2_sp_booth_icon_'..id):SetTexture(imagePath)
			iconPanel:FadeIn(300)
			GetWidget('st2_sp_booth_icon_play_vanity'):SetVisible(0)

			modelPanelTaunt:SetVisible(1)
			modelPanelTaunt:SetEffect('')
		end

		local model = "/ui/fe2/elements/store2/pedestal/model.mdf"
		local effect = "/ui/fe2/elements/store2/pedestal/seat_light.effect"

		if isVault == true then
			model = "/ui/fe2/elements/store2/pedestal/model_vault.mdf"
		end

		modelPanelTaunt:SetModel(0, model)
		modelPanelTaunt:SetEffect(0, effect)

		modelPanel:SetRotate(false)
	end

	local function BoothInitializeCouriers(modelPanel, itemID)
		itemID = tostring(itemID)
		local info = HoN_Store.CouriersTable[itemID]
		if info == nil then Echo("Failed to initialize couriers model panel for booth: "..tostring(itemID)) return end

		if modelPanel == nil then modelPanel = GetWidget('store2_specials_booth_misc_model_'..tostring(id)) end
		local product = 'Pet_AutomatedCourier.'..info.product

		modelPanel:SetModel(1, GetHeroPreviewModelPathFromProduct(product))
		modelPanel:SetEffect(1, GetHeroStorePassiveEffectPathFromProduct(product))
		modelPanel:SetModelPos(1, GetHeroPreviewPosFromProduct(product))
		modelPanel:SetModelAngles(1, GetHeroPreviewAnglesFromProduct(product))
		modelPanel:SetModelScale(1, GetHeroPreviewScaleFromProduct(product) * VanityBoothInfo.Couriers.scale)

		modelPanel:SetRotate(true)
		modelPanel:SetRotate(0, true)
		modelPanel:SetRotate(1, true)
	end

	local function BoothInitializeWards(modelPanel, itemID)
		LoadEntityDefinition('/items/basic/flaming_eye/gadget.entity')
		LoadEntityDefinition('/items/basic/mana_eye/gadget.entity')

		local info = HoN_Store.WardsTable[itemID]
		if info == nil then Echo("Failed to initialize wards model panel for booth: "..tostring(itemID)) return end

		local flamingEyeProduct = 'Gadget_FlamingEye.'..info.product
		local manaEyeProduct = 'Gadget_Item_ManaEye.'..info.product

		modelPanel:SetModel(1, GetHeroPreviewModelPathFromProduct(flamingEyeProduct))
		modelPanel:SetEffect(1, GetHeroStorePassiveEffectPathFromProduct(flamingEyeProduct))
		modelPanel:SetModelAngles(1, GetHeroPreviewAnglesFromProduct(flamingEyeProduct))
		modelPanel:SetModelScale(1, GetHeroPreviewScaleFromProduct(flamingEyeProduct) * VanityBoothInfo.FlamingEye.scale)
		local offset = VanityBoothInfo.FlamingEye.offset
		modelPanel:SetModelPos(1, offset[1], offset[2], offset[3])

		modelPanel:SetModel(2, GetHeroPreviewModelPathFromProduct(manaEyeProduct))
		modelPanel:SetEffect(2, GetHeroStorePassiveEffectPathFromProduct(manaEyeProduct))
		modelPanel:SetModelAngles(2, GetHeroPreviewAnglesFromProduct(manaEyeProduct))
		modelPanel:SetModelScale(2, GetHeroPreviewScaleFromProduct(manaEyeProduct) * VanityBoothInfo.ManaEye.scale)
		local offset = VanityBoothInfo.ManaEye.offset
		modelPanel:SetModelPos(2, offset[1], offset[2], offset[3])

		modelPanel:SetRotate(0, false)
		modelPanel:SetRotate(1, true)
		modelPanel:SetRotate(2, true)
	end

	local function BoothInitializePedestalModels(cat, modelPanel)
		local model = "/ui/fe2/elements/store2/pedestal/model.mdf"
		local effect = "/ui/fe2/elements/store2/pedestal/seat_light.effect"

		if IsVault(cat) then
			model = "/ui/fe2/elements/store2/pedestal/model_vault.mdf"
		end
		modelPanel:SetModel(0, model)
		modelPanel:SetEffect(0, effect)
		modelPanel:SetModelScale(0, VanityBoothInfo.Pedestal.scale)

		local vPos = VanityBoothInfo.Pedestal.offset[cat] or VanityBoothInfo.Pedestal.offset['default']
		modelPanel:SetModelPos(0, vPos[1], vPos[2], vPos[3])

		modelPanel:SetModel(1, "/shared/models/invis.mdf")
		modelPanel:SetModel(2, "/shared/models/invis.mdf")

		modelPanel:SetRotate(true)
		modelPanel:SetRotate(0, true)

		modelPanel:SetCameraPos(0, 2200, 228)
		modelPanel:SetCameraAngles(-6, 0, 180)
	end

	local function BoothInitializeAvatarModels(modelPanel, code, productType, previewType)
		if not productType then productType = CheckCodeType(code) end

		local heroAvatarStr = code
		local hero = ''
		local avatar = ''

		if productType.isavatar then
			heroAvatarStr = string.sub(heroAvatarStr, 4)
			hero, avatar = ParseHeroAvatar(heroAvatarStr)
		else
			hero = string.sub(heroAvatarStr, 3, -6)
		end

		previewType = previewType or 'special'
		SetModelPanelByHeroAvatar(modelPanel, hero, avatar, previewType)

		modelPanel:SetVisible(1)
	end

	local function BoothInitializeAnnouncers(modelPanel)
		HoN_Store:AnnouncersTypeOnClick('first_blood' ,'bloodlust')
		modelPanel:SetRotate(false)
	end

	local function BoothInitializeCreeps(modelPanel)
		HoN_Store:CreepsTypeOnClick('type', 'bad_melee')

		modelPanel:SetModelScale(1)
		modelPanel:SetModelPos(0, 0, 0)
		modelPanel:SetCameraPos(0, 1970, 550)
		modelPanel:SetCameraAngles(-15, 0, 180)
	end

	local function  BoothInitializeTauntBadges(modelPanel, itemID, image)
		local info = HoN_Store.TauntBadgesTable[itemID]
		if info == nil or #info == 0 then return end

		GetWidget('store2_vanity_switchbtn_root_vanity'):SetVisible(#info > 1)

		HoN_Store.currentTauntBadgeID = itemID
		HoN_Store.currentTauntBadgeImage = image
		HoN_Store:OnClickSwitchTauntBadge(1)

		if #info > 1 then
			for i=1,4 do
				local item = info[i]
				GetWidget('store2_vanity_switchbtn_vanity_'..i):SetVisible(item ~= nil)

				if item ~= nil then
					local icon1 = item.btnnormal or ''
					local icon2 = item.btnover or ''

					GetWidget('store2_vanity_switchbtn_vanity_'..i..'_normal'):SetTexture(icon1)
					GetWidget('store2_vanity_switchbtn_vanity_'..i..'_over'):SetTexture(icon2)
					GetWidget('store2_vanity_switchbtn_vanity_'..i..'_down'):SetTexture(icon2)

					GetWidget('store2_vanity_switchbtn_vanity_'..i):ClearCallback('onclick')

					if item.onclicklua ~= nil then 
						GetWidget('store2_vanity_switchbtn_vanity_'..i):SetCallback('onclick', item.onclicklua)
					end
				end
			end
		end
	end

	local function BoothInitializeTPEffects(modelPanel, itemID)
		local info = HoN_Store.TPEffectsTable[itemID]
		if info == nil or #info == 0 then return end

		modelPanel:SetModel('/shared/models/invis.mdf')
		modelPanel:SetModelScale(0.25)
		modelPanel:SetModelPos(0, 0, -50)
		modelPanel:SetAnim('idle')

		GetWidget('store2_vanity_switchbtn_root_vanity'):SetVisible(#info > 1)

		HoN_Store.currentTPEffectID = itemID
		HoN_Store:OnClickSwitchTPEffect(1)

		if #info > 1 then 
			for i=1,4 do
				local item = info[i]
				GetWidget('store2_vanity_switchbtn_vanity_'..i):SetVisible(item ~= nil)

				if item ~= nil then
					local icon1 = item.btnnormal or ''
					local icon2 = item.btnover or ''

					GetWidget('store2_vanity_switchbtn_vanity_'..i..'_normal'):SetTexture(icon1)
					GetWidget('store2_vanity_switchbtn_vanity_'..i..'_over'):SetTexture(icon2)
					GetWidget('store2_vanity_switchbtn_vanity_'..i..'_down'):SetTexture(icon2)

					GetWidget('store2_vanity_switchbtn_vanity_'..i):ClearCallback('onclick')

					if item.onclicklua ~= nil then 
						GetWidget('store2_vanity_switchbtn_vanity_'..i):SetCallback('onclick', item.onclicklua)
					end
				end
			end
		end
	end

	local function  BoothInitializeSelectionCircle(modelPanel, itemID, image)
		GetWidget('st2_sp_booth_icon_vanity'):SetTexture(image)
		GetWidget('st2_sp_booth_icon_panel_vanity'):FadeIn(300)
		GetWidget('st2_sp_booth_icon_play_vanity'):SetVisible(0)
	end

	local function ClearVanityBooth(modelPanel, id)
		-- no name
		local w = GetWidget('st2_sp_booth_name_label_'..id, nil, true)
		if w ~= nil then w:SetText('') end

		-- no description
		w = GetWidget('st2_sp_booth_description_label_'..id, nil, true)
		if w ~= nil then w:SetText('') end
		w = GetWidget('st2_sp_booth_description2_label_'..id, nil, true)
		if w ~= nil then w:SetText('') end
		w = GetWidget('st2_sp_booth_label_'..id, nil, true)
		if w ~= nil then w:SetText('') end

		-- no icon
		GetWidget('st2_sp_booth_icon_panel_'..id):SetVisible(0)
		GetWidget('st2_sp_booth_icon_namecolor_'..id):SetText('Name Color')
		GetWidget('st2_sp_booth_icon_namecolor_'..id):SetVisible(false)
		w = GetWidget('st2_sp_booth_icon_play_'..id, nil, true)
		if w ~= nil then w:SetVisible(false) end
		w = GetWidget('st2_sp_booth_icon_play_over_'..id, nil, true)
		if w ~= nil then w:SetVisible(false) end

		-- no price
		GetWidget('store2_specials_booth_price_parent_'..id):SetVisible(false)

		-- no buy button
		GetWidget('store2_specials_booth_buy_button_'..id):SetVisible(false)

		-- no detail panel
		w = GetWidget('st2_sp_booth_'..id..'_detail_panel', nil, true)
		if w ~= nil then w:SetVisible(false) end

		-- no models and effects
		modelPanel:SetModel("")
		modelPanel:SetEffect("")
		w = GetWidget('store2_specials_booth_announcers_model_content_vanity')
		w:SetModel("")
		w:SetEffect("")

		w = GetWidget('store2_'..id..'_booth_taunt_modelpanel', nil, true)
		if w ~= nil then w:SetVisible(false) end

		-- no bundle panels
		w = GetWidget('store2_specials_booth_bundle_panel_'..id, nil, true)
		if w ~= nil then w:SetVisible(false) end
		w = GetWidget('store2_specials_booth_bundle_chest_panel_'..id, nil, true)
		if w ~= nil then w:SetVisible(false) end
		w = GetWidget('store2_specials_booth_bundle_chest_panel_'..id, nil, true)
		if w ~= nil then w:SetVisible(false) end

		-- no buy/choose/owned button
		w = GetWidget('store2_specials_booth_owned_button_'..id, nil, true)
		if w ~= nil then w:SetVisible(false) end
		w = GetWidget('store2_specials_booth_buy_button_'..id, nil, true)
		if w ~= nil then w:SetVisible(false) end
		w = GetWidget('store2_specials_booth_choose_button_'..id, nil, true)
		if w ~= nil then w:SetVisible(false) end
	end

	local function SetupNameDesc(isVault, itemID, sName, sDesc)

		local cs = isVault and TitleDescColorSetting.vault or TitleDescColorSetting.store

		-- name
		local displayName = GetProductDispayNameByID(itemID)
		local nameWidget = GetWidget(sName or 'st2_sp_booth_name_label_vanity')
		nameWidget:SetText(displayName)
		nameWidget:SetColor(cs.name)
		nameWidget:SetOutlineColor(cs.nameOutline)

		-- description
		local desc = GetProductDisplayDescByID(itemID)
		local descriptionWidget = GetWidget(sDesc or 'st2_sp_booth_description_label_vanity')
		descriptionWidget:SetText(desc)
		descriptionWidget:SetColor(cs.desc)
		descriptionWidget:SetOutlineColor(cs.descOutline)
	end

	local function BoothInitializeVanityCommon(cat, data, clear)
		BoothInitializeDetailPanel(cat)

		local modelPanel = GetWidget('store2_specials_booth_model_vanity')
		HoN_Store.vanitySpecialDailyGold = -1
		HoN_Store.vanitySpecialDailySilver = -1

		-- booth is shown but the content should be hidden
		if clear then
			ClearVanityBooth(modelPanel, id)
			BoothInitializePedestalModels(cat, modelPanel)
		else
			local itemID = data.itemID
			SetupNameDesc(data.isVault, itemID)

			local showChooseButton

			if data.isVault then
				showChooseButton = cat ~= 'vault_upgrades'
			else
				-- buy/owned button
				GetWidget('store2_specials_booth_buy_button_vanity'):SetVisible(not data.buyButtonDisabled)
				GetWidget('store2_specials_booth_owned_button_vanity'):SetVisible(data.buyButtonDisabled)
				if data.buyButtonDisabled then
					GetWidget('store2_specials_booth_owned_button_vanity_label'):SetText(Translate(data.disableButtonText))
				end

				local canGift = GetCvarInt('_microStoreGiftsRemaining') > 0 and data.purchasable
				if canGift and data.owned then
					Set('_microStore_SelectedItemOwned', true)
				end
				GetWidget('store2_specials_booth_gift_button_vanity'):SetVisible(canGift and not IsEligibilityLocked(data.itemID))

				-- prices
				HoN_Store:BoothClearPrices(id)

				local priceGold = data.gold
				local priceGoldDailySpecial = data.gold
				local priceSilver = data.silver
				local priceSilverDailySpecial = data.silver

				local dailySpecialInfo = GetDailySpecialEntry(itemID)
				if dailySpecialInfo then
					if HasData(dailySpecialInfo.gold_coins) and HasData(dailySpecialInfo.current_gold_coins) then 
						priceGold = dailySpecialInfo.gold_coins
						priceGoldDailySpecial = dailySpecialInfo.current_gold_coins
					end

					if HasData(dailySpecialInfo.silver_coins) and HasData(dailySpecialInfo.current_silver_coins) then
						priceSilver = dailySpecialInfo.silver_coins
						priceSilverDailySpecial = dailySpecialInfo.current_silver_coins
					end
				end

				if data.showGold and priceGold <= priceGoldDailySpecial then 
					HoN_Store:BoothSetPrice(id, 3, 'gold', 1, false, priceGold)
				elseif data.showGold then
					HoN_Store:BoothSetPrice(id, 1, 'gold', 1, true, priceGold)
					HoN_Store:BoothSetPrice(id, 3, 'gold', 1, false, priceGoldDailySpecial)

					HoN_Store.vanitySpecialDailyGold = priceGoldDailySpecial
				end

				if data.showSilver and priceSilver <= priceSilverDailySpecial then
					HoN_Store:BoothSetPrice(id, 4, 'silver', 1, false, priceSilver) 
				elseif data.showSilver then 
					HoN_Store:BoothSetPrice(id, 2, 'silver', 1, true, priceSilver)
					HoN_Store:BoothSetPrice(id, 4, 'silver', 1, false, priceSilverDailySpecial)

					HoN_Store.vanitySpecialDailySilver = priceSilverDailySpecial
				end

				if data.showLabel then
					local text = Translate('mstore_free')
					HoN_Store:BoothSetPrice(id, 5, 'lable', 1, false, text)
				end
			end

			-- pedestal models
			BoothInitializePedestalModels(cat, modelPanel)
			modelPanel:SetEffect('')
			modelPanel:SetMultiEffect('', 1)

			-- namecolor label
			local nameColorLabel = GetWidget('st2_sp_booth_icon_namecolor_vanity')
			nameColorLabel:SetText('Name Color')
			nameColorLabel:SetVisible(0)

			if cat == 'vault_namecolors' or cat == 'vault_upgrades' or cat == 'vault_others' or
				cat == 'namecolors' or cat == 'upgrades' or cat == 'others' then
				BoothInitializeIcons(modelPanel, data.image, id)

				if tostring(itemID) == '999999005' then
					local descriptionWidget = GetWidget('st2_sp_booth_description_label_vanity')
					descriptionWidget:SetText(Translate('mstore_vault_tip_tokens_txt'))

					nameColorLabel:SetVisible(1)
					nameColorLabel:SetColor(TitleDescColorSetting.vault.name)
					nameColorLabel:SetGlow(false)
					nameColorLabel:SetText(Translate('store2_owned_count', 'count', HoN_Store.GameTokens or '0'))

					showChooseButton = false
				end
			end

			if cat == 'vault_namecolors' or cat == 'namecolors' then
				BoothInitializeNameColor(modelPanel, data.code, id)
			end

			if cat == "vault_taunts" or cat == "taunts" then
				BoothInitializeTaunts(modelPanel, id, itemID, data.image, data.isVault)
			end

			if cat == "vault_couriers" or cat == "couriers" then
				BoothInitializeCouriers(modelPanel, itemID)
			end

			if cat == "vault_wards" or cat == "wards" then
				BoothInitializeWards(modelPanel, itemID)
			end

			if cat == "vault_announcers" or cat == "announcers" then
				BoothInitializeAnnouncers(modelPanel)
			end

			if cat == "vault_creeps" then
				BoothInitializeCreeps(modelPanel)
			end

			if cat == 'vault_tauntbadges' or cat == 'tauntbadges' then
				BoothInitializeTauntBadges(modelPanel, itemID, data.image)
			end

			if cat == 'vault_tpeffects' or cat == 'tpeffects' then
				BoothInitializeTPEffects(modelPanel, itemID)
			end

			if cat == 'vault_selectioncircles' or cat == 'selectioncircles' then
				BoothInitializeSelectionCircle(modelPanel, itemID, data.image)
			end
			-- choose button
			if data.isVault then
				GetWidget('store2_specials_booth_choose_button_vanity'):SetVisible(showChooseButton)
			end
			-- detail panel
			GetWidget('st2_sp_booth_vanity_detail_panel'):SetVisible(true)

			local container = GetWidget('st2_sp_booth_vanity_info_container')
			local height = container:GetHeight() + container:GetYFromString('16i')
			container:GetParent():SetHeight(height)
			container:DoEventN(1)

		end
	end

	local function BoothInitializeOthersDetail(cat, data)
		if data == nil then return end

		local function FormatPrice(gold, silver)
			local sGold, sSilver = gold, silver
			if gold == 0 and silver == 0 then
				sGold = Translate('mstore_free')
				sSilver = Translate('mstore_free')
			end
			return sGold, sSilver
		end

		local itemID = tonumber(data.itemID)
		local owned = data.owned or false
		SetupNameDesc(data.isVault, itemID, 'store2OthersDetailName', 'store2OthersDetailDescription')

		if cat == HoN_Store.BundleDetail.Category then
			GetWidget('store2OthersBuyContainer'):SetVisible(false)
			GetWidget('store2OthersNeedCoinsBtn'):SetVisible(false)
			GetWidget('store2OthersBtnChangeNickname'):SetVisible(false)
			if not GetCvarBool('cl_GarenaEnable') then
				GetWidget('store2OthersBtnCreateSubaccount'):SetVisible(false)
			end
			GetWidget('store2OthersBtnResetStats'):SetVisible(false)
		else
			if not data.isVault then
				-- prices
				HoN_Store:BoothClearPrices('others')
				local showGold, showSilver = ShowGoldSiver(data.gold, data.silver)
				if showGold or showSilver then
					if showGold then
						HoN_Store:BoothSetPrice('others', 1, 'gold', 1, false, data.gold)
					end
					if showSilver then
						HoN_Store:BoothSetPrice('others', 2, 'silver', 1, false, data.silver)
					end
				else
					HoN_Store:BoothSetPrice('others', 3, 'gold', 1, false, Translate('mstore_free'))
				end
			end

			local bCoinEnough 	= data.bCoinEnough
			local purchasable	= data.purchasable

			if not owned then
				if purchasable then
					GetWidget('store2OthersBuyContainer'):SetVisible(bCoinEnough)
					GetWidget('store2OthersNeedCoinsBtn'):SetVisible(not bCoinEnough)
				else
					GetWidget('store2OthersBuyContainer'):SetVisible(true)
					GetWidget('store2OthersNeedCoinsBtn'):SetVisible(false)
				end
				GetWidget('store2_specials_booth_buy_button_others'):SetVisible(purchasable)
				GetWidget('store2_specials_booth_owned_button_others'):SetVisible(not purchasable)
			else
				GetWidget('store2OthersBuyContainer'):SetVisible(false)
				GetWidget('store2OthersNeedCoinsBtn'):SetVisible(false)
			end

			GetWidget('store2OthersBtnChangeNickname'):SetVisible(owned and itemID == 161)
			if not GetCvarBool('cl_GarenaEnable') then
				GetWidget('store2OthersBtnCreateSubaccount'):SetVisible(owned and itemID == 162)
			end
			GetWidget('store2OthersBtnResetStats'):SetVisible(owned and itemID == 163)
		end

		--panels
		local wdgParent1, wdgParent2, wdgPanel = GetWidget('store2OthersFormContainer'), GetWidget('store2OthersForms'), GetWidget('storePopupNameChangeForm_others')

		wdgPanel:SetVisible(itemID == 161)
		wdgPanel:SetParent(itemID == 161 and wdgParent1 or wdgParent2)
		if itemID == 161 then
			ChangeChildrenStytle(wdgPanel, HoN_Store:IsVaultOpened(), owned)
		end

		if not GetCvarBool('cl_GarenaEnable') then
			wdgPanel = GetWidget('storePopupSubAccountForm_others')
			wdgPanel:SetVisible(itemID == 162)
			wdgPanel:SetParent(itemID == 162 and wdgParent1 or wdgParent2)
			if itemID == 162 then
				ChangeChildrenStytle(wdgPanel, HoN_Store:IsVaultOpened(), owned)
			end
		end

		wdgPanel = GetWidget('storePopupResetStatsForm_others')
		wdgPanel:SetVisible(itemID == 163)
		wdgPanel:SetParent(itemID == 163 and wdgParent1 or wdgParent2)
		if itemID == 163 then
			ChangeChildrenStytle(wdgPanel, HoN_Store:IsVaultOpened(), owned)
		end

		GetWidget('store2_specials_booth_vanity'):SetVisible(0)
		GetWidget('store2OthersDetailPanel'):SetVisible(1)
	end

	local function ShouldShowAsForm(itemID)

		if not itemID then
			if not info then return false end

			local idx = Store2VanityGetIndex(cat, info.row, info.col)
			itemID = nil
			if IsVault(cat) then
				local singleVault = HoN_Store.Store2VaultData[cat][idx]
				if singleVault ~= nil then
					itemID = singleVault['itemid']
				end
			else
				local catInfo = HoN_Store.ProductPageInfo[cat]
				itemID = catInfo['productIDs'][idx]
			end
		end
		itemID = tonumber(itemID)

		return itemID == 161 or itemID == 162 or itemID == 163 or itemID == 492
	end

	local function GetBundle_ModelInfo(modelPath, bDaily)
		bDaily = bDaily or false

		if modelPath == '/ui/fe2/grabbag/lucky_bundle/footlocker.mdf' or
			modelPath == '/ui/fe2/grabbag/discount_bundle/footlocker.mdf' then
			if bDaily then
				return 55, 0.35, nil
			else
				return 55, nil, nil
			end
		elseif modelPath == '/ui/fe2/grabbag/halloween/pumpkin.mdf' then
			if bDaily then
				return -20, 1, nil
			else
				return -20, 1.5, '/ui/fe2/grabbag/halloween/hover_effect.effect'
			end
		elseif modelPath == '/ui/fe2/grabbag/halloween2017/pumpkin.mdf' then
			if bDaily then
				return -20, 0.6, nil
			else
				return -20, 0.85, '/ui/fe2/grabbag/halloween/hover_effect.effect'
			end
		elseif modelPath == '/ui/fe2/grabbag/esports/footlocker.mdf' then
			if bDaily then
				return -30, 0.45, nil
			else
				return -30, 0.7, '/ui/fe2/grabbag/esports/hover_effect.effect'
			end
		elseif modelPath == '/ui/fe2/grabbag/christmas/present/present.mdf' or
			modelPath == '/ui/fe2/grabbag/christmas/present/present_02.mdf' or
			modelPath == '/ui/fe2/grabbag/christmas/present/present_03.mdf' then
			if bDaily then
				return -30, 0.35, nil
			else
				return -30, nil, nil
			end
		else
			return nil, nil, nil
		end
	end

	local function Show_Vanity()
		local row, col = info and info.row or nil, info and info.col or nil

		TestEcho('Show_Vanity category: '..tostring(cat)..', row='..tostring(row)..', col='..tostring(col))

		local data = {}
		local clear = cat == nil or row == nil
		if not clear then
			local idx = Store2VanityGetIndex(cat, row, col)

			local itemID
			if IsVault(cat) then
				data.isVault = true

				local singleVault = HoN_Store.Store2VaultData[cat][idx]

				if singleVault ~= nil then

					vaultCode = singleVault['code']
					VanityBoothInfo["selectVaultCode"] = vaultCode

					itemID = singleVault['itemid']

					data.image 	= singleVault.localContent
					data.code	= singleVault.code
				end
			else
				data.isVault = false

				local catInfo = HoN_Store.ProductPageInfo[cat]
				if not catInfo then Echo("^r Invalid vanity category info: "..tostring(cat)) return end

				itemID = catInfo['productIDs'][idx]

				data.purchasable = IsPurchasable(catInfo, idx)
				data.owned = IsOwned(catInfo, idx)

				data.gold = catInfo['productPrices'][idx]
				data.silver = catInfo['premiumMmpCost'][idx]

				data.showGold	= false
				data.showSilver = false
				data.showLabel  = false
				if data.purchasable then
					data.showGold, data.showSilver = ShowGoldSiver(data.gold, data.silver)
					if not data.showGold and not data.showSilver then
						data.showLabel 	= true
					end
				end

				data.buyButtonDisabled = (data.owned and data.purchasable) or IsEligibilityLocked(itemID) or IsNeedTauntLocked(catInfo, idx) or not data.purchasable
				if data.buyButtonDisabled then
					if data.owned and data.purchasable then
						data.disableButtonText = 'mstore_purchased'
					elseif IsEligibilityLocked(itemID) then
						data.disableButtonText = 'mstore_elig_only'
					elseif IsNeedTauntLocked(catInfo, idx) then
						data.disableButtonText = 'mstore_needunlocktaunt'
					elseif not data.purchasable then
						data.disableButtonText = 'general_unavailable'
					end
				end

				data.image = catInfo['images'][idx]
				if not data.image then
					data.image = ''
					Echo('^rShow_Vanity invalid icon, cat='..tostring(cat)..', idx='..tostring(idx))
				end
				data.code  = catInfo['productCodes'][idx]
			end

			data.itemID = itemID
			if itemID == nil then
				Echo('^rnil itemID, info=')
				printTableDeep(info)
			end
		end

		SetVanityBoothSelectedInfo(cat, row, col, data)
		BoothInitializeVanityCommon(cat, data, clear)
	end

	local function Show_NavBackToVanity()

		local itemID = info.productID
		if itemID == nil or itemID == '' then
			return
		end

		local cat = info.cat
		if cat == 'others' or cat == 'vault_others' then
			if ShouldShowAsForm(itemID) then
				BoothInitializeOthersDetail(cat, info.boothData)
				return
			end
		end
		BoothInitializeVanityCommon(cat, info.boothData, false)
	end

	local function Show_DailySpecial()

		local modelPanel = GetWidget('store2_specials_booth_model_'..id)
		ClearVanityBooth(modelPanel, id)
		modelPanel:SetCameraPos(-28,2945,1900)
		modelPanel:SetModelAngles(0,0,0)
		modelPanel:SetModelPos(0,0,0)
		modelPanel:SetModelScale(1)

		-- Name
		local name = info.item_cname
		local nameLabel = GetWidget('st2_sp_booth_name_label_'..id)
		HoN_Store:SetLabelText(nameLabel, 'heroname', id, name, true, true)

		-- LIMITED EDITION or ...
		GetWidget('st2_sp_booth_label_left_'..id):SetVisible(0)
		GetWidget('st2_sp_booth_label_right_'..id):SetVisible(0)

		if HasData(info.tag_title) then
			GetWidget('st2_sp_booth_label_'..id):SetText(info.tag_title)
			if id == 1 or id == 2 or id == 4 then
				GetWidget('st2_sp_booth_label_'..id):SetColor('#f8ffff')
				GetWidget('st2_sp_booth_label_'..id):SetOutlineColor('#04818b')
				GetWidget('st2_sp_booth_label_left_'..id):SetVisible(1)
			else
				GetWidget('st2_sp_booth_label_'..id):SetColor('white')
				GetWidget('st2_sp_booth_label_'..id):SetOutlineColor('#83132b')
				GetWidget('st2_sp_booth_label_right_'..id):SetVisible(1)
			end
		end

		-- Time limit
		local timelimiteLabel = GetWidget('st2_sp_booth_timelimit_label_'..id)
		if HasData(info.tag_date_limit) then
			GetWidget('st2_sp_booth_timelimit_back_'..id):SetVisible(1)
			timelimiteLabel:SetVisible(1)
			HoN_Store:SetLabelText(timelimiteLabel, 'info', id, info.tag_date_limit)
		else
			GetWidget('st2_sp_booth_timelimit_back_'..id):SetVisible(0)
			timelimiteLabel:SetVisible(0)
		end

		-- Price
		HoN_Store:BoothClearPrices(tostring(id))

		local isFree = false
		local isSpecialPrice = (type(info.plinko_only) == 'number' and info.plinko_only > 0)

		if not isSpecialPrice and not info.is_owned then
			local idStr = tostring(id)
			if HasData(info.gold_coins) and HasData(info.current_gold_coins) and info.gold_coins ~= info.current_gold_coins then
				HoN_Store:BoothSetPrice(idStr, 1, 'gold', 1, true, info.gold_coins)
			end

			if HasData(info.silver_coins) and HasData(info.current_silver_coins) and info.silver_coins ~= info.current_silver_coins then
				HoN_Store:BoothSetPrice(idStr, 2, 'silver', 1, true, info.silver_coins)
			end

			if HasData(info.current_gold_coins) then
				HoN_Store:BoothSetPrice(idStr, 3, 'gold', 1, false, info.current_gold_coins)
			end
			if HasData(info.current_silver_coins) then
				HoN_Store:BoothSetPrice(idStr, 4, 'silver', 1, false, info.current_silver_coins)
			end

			isFree = IsFree(info.current_gold_coins, info.current_silver_coins)
		end

		-- Is Owned
		GetWidget('st2_sp_booth_name_own_'..id):SetVisible(info.is_owned)

		-- Buy/Purchased/PlinkoOnly Button
		local buyButton = GetWidget('store2_specials_booth_buy_button_'..id)
		buyButton:SetVisible(1)
		buyButton:SetEnabled((not info.is_owned) and (info.purchasable or isSpecialPrice))
		if info.is_owned then
			buyButton:SetLabel(Translate('mstore_purchased'))
		elseif isSpecialPrice then
			if info.plinko_only == 1 then
				buyButton:SetLabel(Translate('store2_now_in_plinko'))
				buyButton:SetCallback('onclick', function() HoN_Store:Store2DailySpecialClear() UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'plinko', nil, false) end)
			elseif info.plinko_only == 2 then
				buyButton:SetLabel(Translate('mstore_luckydraw_only'))
				buyButton:SetCallback('onclick', function() if isNewUI() then HoNEvent:ToggleEventPanel() else specialMessages:ToggleSpecialMessages() end end)
			elseif info.plinko_only == 3 then
				buyButton:SetLabel(Translate('store2_now_in_pon'))
				buyButton:SetCallback('onclick', function() HoN_Codex:Open() end)
			elseif info.plinko_only == 4 then 
				buyButton:SetLabel(Translate('mstore_event_only'))
				buyButton:SetCallback('onclick', function() if isNewUI() then HoNEvent:ToggleEventPanel() else specialMessages:ToggleSpecialMessages() end end)
			end
		elseif not info.purchasable then
			buyButton:SetLabel(Translate('general_unavailable'))
		else
			if isFree then
				buyButton:SetLabel(Translate('mstore_free'))
			else	
				buyButton:SetLabel(Translate('store2_purchase'))
			end
			if info.productType ~= nil and info.productType.iseapbundle == true then
				buyButton:SetCallback('onclick', function(...) HoN_Store:DailySpecialOnClicked(id) end)
			else
				buyButton:SetCallback('onclick', function(...) HoN_Store:OnPurchaseSpecials(id) end)
			end
		end

		-- Discount
		local discount = info.discount_off
		if info.is_owned then discount = 0 end
		Trigger('BoothDiscount'..id, discount or 0)

		local productType = info.productType
		local code = info.item_code

		local showIconFrame = false
		if productType.isavatar then
			TestEcho('isavatar')
			BoothInitializeAvatarModels(modelPanel, code, productType, 'special')
		elseif productType.iscolor then
			TestEcho('iscolor')
			BoothInitializeIcons(modelPanel, info.local_content, id)
			showIconFrame = true
			BoothInitializeNameColor(modelPanel, code, id)
		elseif productType.isicon then
			TestEcho('isicon')
			BoothInitializeIcons(modelPanel, info.local_content, id)
			showIconFrame = true
		elseif productType.isannouncer then
			TestEcho('isannouncer')
			BoothInitializeIcons(modelPanel, info.local_content, id)
			showIconFrame = true
		elseif productType.issymbol then
			TestEcho('issymbol')
			BoothInitializeIcons(modelPanel, info.local_content, id)
			showIconFrame = true
		elseif productType.istaunt then
			TestEcho('istaunt')
			BoothInitializeIcons(modelPanel, info.local_content, id)
			showIconFrame = true
		elseif productType.iscouriers then
			TestEcho('iscouriers')
			BoothInitializeCouriers(nil, info.product_id)
		elseif productType.isward then
			TestEcho('isward')
			BoothInitializeIcons(modelPanel, info.local_content, id)
			showIconFrame = true
		elseif productType.isupgrade then
			TestEcho('isupgrade')
			BoothInitializeIcons(modelPanel, info.local_content, id)
			showIconFrame = true
		elseif productType.iscreep then
			TestEcho('iscreep')
			Echo('^rDaily Special: not supported: '..code)
		elseif productType.ishero then
			TestEcho('ishero')
			BoothInitializeAvatarModels(modelPanel, code, productType, 'special')
		elseif info.local_content then
			local setting = HoN_Store:GetBundleChestModelForIconPath(info.local_content)
			if NotEmpty(info.model) then
				local bundleAngle, bundleScale = GetBundle_ModelInfo(info.model, true)
				local angle, scale = bundleAngle and bundleAngle or 90, bundleScale and bundleScale or 0.35

				modelPanel:SetModel(info.model)
				modelPanel:SetModelAngles(0,0,angle)
				modelPanel:SetModelScale(scale)
				modelPanel:SetCameraPos(0,4950,1100)
			elseif setting then
				modelPanel:SetModel(setting.model)
				modelPanel:SetModelAngles(0,0,90)
				modelPanel:SetModelScale(0.35)
				modelPanel:SetCameraPos(0,4950,1100)
			else
				BoothInitializeIcons(modelPanel, info.local_content, id)
				showIconFrame = true
			end
		end

		-- special icon frame bg texture for Miku annoucer
		if showIconFrame then
			local bgTex, bgHoverTex
			if info.product_id == '4437' then
				bgTex = "/ui/fe2/elements/store2/vanityboothframe_miku.png"
				bgHoverTex = "/ui/fe2/elements/store2/vanityboothframe_over_miku.png"
			else
				bgTex = "/ui/fe2/elements/store2/vanityboothframe.png"
				bgHoverTex = "/ui/fe2/elements/store2/vanityboothframe_over.png"
			end
			GetWidget('st2_sp_booth_icon_bg_'..id):SetTexture(bgTex)
			GetWidget('st2_sp_booth_icon_bghover_'..id):SetTexture(bgHoverTex)
		end
	end

	local function Show_BundleDetail()
		local detail 	= HoN_Store.BundleDetail
		local catInfo 	= HoN_Store.ProductPageInfo[detail.Category]
		local boothID 	= detail.BoothID
		local productID = info.productID

		local showToggleDetailBtn = info.isward or info.iscouriers or info.ishero or info.isavatar or false
		Set('store2_showModelDetailArrow', tostring(showToggleDetailBtn))
		Set('store2_isBundleModelDetail', tostring(true))

		SetVanityBoothSelectedInfo(cat, nil, nil, {productID = productID})

		local realCat = GetProductCatName(info)

		local modelPanel = GetWidget('store2_specials_booth_model_'..boothID)
		ClearVanityBooth(modelPanel, boothID)
		BoothInitializePedestalModels(realCat, modelPanel)
		modelPanel:SetVisible(true)

		BoothInitializeDetailPanel(realCat)

		-- name
		local nameWidget = GetWidget('st2_sp_booth_name_label_'..boothID)
		nameWidget:SetText(info.name)
		nameWidget:SetColor('#d7b6a2')
		nameWidget:SetOutlineColor('#241410')

		-- description
		local descriptionWidget = GetWidget('st2_sp_booth_description2_label_'..boothID)
		descriptionWidget:SetText(info.desc)
		descriptionWidget:SetVisible(true)
		descriptionWidget:SetColor('#d7b6a2')
		descriptionWidget:SetOutlineColor('#241410')

		local bundleId = HoN_Store.BundleDetail.BundleId
		-- buttons
		GetWidget('store2_specials_booth_choose_button_'..boothID):SetVisible(false)
		if info.owned then
			GetWidget('store2_specials_booth_buy_button_'..boothID):SetVisible(false)
			GetWidget('store2_specials_booth_owned_button_vanity_label'):SetText(Translate('mstore_purchased'))
			GetWidget('store2_specials_booth_owned_button_vanity'):SetVisible(true)
		elseif IsEligibilityLocked(bundleId, HoN_Store.BundleDetail.BundleEligiblity) then
			GetWidget('store2_specials_booth_buy_button_'..boothID):SetVisible(false)
			GetWidget('store2_specials_booth_owned_button_vanity_label'):SetText(Translate('mstore_elig_only'))
			GetWidget('store2_specials_booth_owned_button_vanity'):SetVisible(true)
		elseif info.isLuckyBundle and info.luckyCount <= 0 then
			GetWidget('store2_specials_booth_buy_button_'..boothID):SetVisible(false)
			GetWidget('store2_specials_booth_owned_button_vanity'):SetVisible(true)
		else
			GetWidget('store2_specials_booth_buy_button_'..boothID):SetVisible(true)
			GetWidget('store2_specials_booth_owned_button_vanity'):SetVisible(false)
		end

		-- detail
		local idx 		= info.index
		local itemID 	= info.productID
		local showInfoPanel = true

		GetWidget('store2_specials_booth_bundle_chest_panel_'..boothID):SetVisible(info.ShowChest)
		if info.ShowChest then
			modelPanel:SetVisible(false)
			local setting = info.chestModelSetting
			local chest = GetWidget('store2_specials_booth_bundle_chest_model_'..boothID)
			chest:SetModelAngles("0 0 65")
			chest:SetModel(0, setting.model)
			chest:SetModelScale(0, 0.5)
			chest:SetEffect(0, setting.effect)
			chest:SetModelAngles(0, "0 0 0")
			chest:SetAmbientColor(setting.ambientR, setting.ambientG, setting.ambientB)
			chest:SetSunColor(setting.sunR, setting.sunG, setting.sunB)

			local angle, scale, effect = GetBundle_ModelInfo(setting.model)
			if angle ~= nil then
				chest:SetModelAngles(0, '0 0 '..angle)
			end

			if scale ~= nil then 
				chest:SetModelScale(0, scale)
			end

			if NotEmpty(effect) then 
				chest:SetEffect(0, effect)
			end
		
			--chest:SetModelScale(0, 1)

			local chestShake = function(chest, func, firstTime)
				local theme = {
					shakeName	= "shake",
					shakeTimes	= {3500, 7500},
					onShakeFunc = function(chest)
						chest:UICmd("Play2DSFXSound('/ui/fe2/plinko/sounds/chest_appear_"..(math.random(1,2))..".ogg', 0.3)")
					end,
				}
				local time = firstTime and 1 or math.random(theme.shakeTimes[1], theme.shakeTimes[2])
				chest:Sleep(time, function()
					if not chest:IsVisible() then return end
					chest:SetAnim(0, theme.shakeName)
					if (theme.onShakeFunc) then
						theme.onShakeFunc(chest)
					end

					func(chest, func)
				end)
			end
			chestShake(chest, chestShake, true)
		elseif ShouldShowAsForm(itemID) then
			BoothInitializeOthersDetail(cat, {
					itemID = itemID,
					isVault = false,
				})
			modelPanel:SetVisible(false)
			showInfoPanel = false
		elseif info.iscolor or
			info.isupgrade or
			info.isothers or
			info.isicon or
			info.issymbol or
			info.istype then

			BoothInitializeIcons(modelPanel, catInfo['images'][idx], boothID)
			if info.iscolor then
				BoothInitializeNameColor(modelPanel, catInfo['productCodes'][idx], boothID)
			end
		elseif info.ishero or info.isavatar then
			modelPanel:SetVisible(false)
		elseif info.istaunt or
			info.isward or
			info.isannouncer or
			info.iscouriers or
			info.iscreep then

			if info.istaunt then
				BoothInitializeTaunts(modelPanel, boothID, itemID, catInfo['images'][idx], false)
			elseif info.isward then
				BoothInitializeWards(modelPanel, itemID)
			elseif info.isannouncer then
				BoothInitializeAnnouncers(modelPanel, itemID)
			elseif info.iscouriers then
				BoothInitializeCouriers(modelPanel, itemID)
			elseif info.iscreep then
				BoothInitializeCreeps(modelPanel)
			end
		else --others
			BoothInitializeIcons(modelPanel, catInfo['images'][idx], boothID)
		end

		GetWidget('store2_specials_booth_bundle_panel_'..boothID):SetVisible(true)

		-- price
		local width = 0
		local showGold, showSilver = ShowGoldSiver(info.price, info.silverprice)

		if info.price then
			local w = GetWidget('store2_specials_booth_bundle_gold_'..boothID)
			if showGold then
				w:SetVisible(true)
				w:SetText(info.price)
				width = width + w:GetWidth()
			else
				w:SetText('')
				w:SetWidth(0)
				w:SetVisible(false)
			end
			GetWidget('store2_specials_booth_bundle_gold_icon_'..boothID):SetVisible(showGold)
		end

		local w = GetWidget('store2_specials_booth_bundle_saves_'..boothID)
		if info.priceLabel and info.priceLabel ~= '' then
			w:SetText(info.priceLabel)
			w:SetVisible(true)
			width = width + w:GetWidth()
		else
			w:SetVisible(false)
		end
	
		local silverIcon = GetWidget('store2_specials_booth_bundle_silver_icon_'..boothID)
		local silerLabel = GetWidget('store2_specials_booth_bundle_silver_'..boothID)
		local silverCost = tonumber(info.silverprice)
		if showSilver then
			silerLabel:SetText(silverCost)
			width = width + silverIcon:GetWidth()
			width = width + silerLabel:GetWidth()
		end
		silverIcon:SetVisible(showSilver)
		silerLabel:SetVisible(showSilver)

		if not showGold and not showSilver then
			w = GetWidget('store2_specials_booth_bundle_gold_'..boothID)
			w:SetVisible(true)
			w:SetText(Translate('mstore_free'))
			width = width + w:GetWidth()
		end

		w = GetWidget('store2_specials_booth_bundle_price_'..boothID)


		-- lucky/discount
		GetWidget('store2_specials_booth_bundle_lucky_'..boothID):SetVisible(info.isLuckyBundle)
		GetWidget('store2_specials_booth_bundle_onsale_'..boothID):SetVisible(info.isDiscountBundle)
		if info.isLuckyBundle and info.luckyCount then
			GetWidget('store2_specials_booth_bundle_lucky_count_'..boothID):SetText(info.luckyCount)
		end
		if info.isDiscountBundle and info.discount then
			GetWidget('store2_specials_booth_bundle_onsale_percent_'..boothID):SetText(info.discount)
		end

		boothWidget:SetVisible(true)

		-- info panel
		GetWidget('st2_sp_booth_'..boothID..'_info_panel'):SetVisible(showInfoPanel)
		GetWidget('st2_sp_booth_vanity_detail_panel'):SetVisible(true)
		if showInfoPanel then
			local container = GetWidget('st2_sp_booth_vanity_info_container')
			local height = container:GetHeight() + container:GetYFromString('16i')
			container:GetParent():SetHeight(height)
			container:DoEventN(1)
		end
	end

	local function Show_OthersDetail()
		if info == nil then return end

		local row, col = info.row, info.col

		local catInfo = HoN_Store.ProductPageInfo[cat]
		if not catInfo then Echo("^r Invalid vanity category info: "..tostring(cat)) return end

		local idx = Store2VanityGetIndex(cat, row, col)

		local itemID = nil
		local data = {}
		data.owned = true
		data.bCoinEnough = true

		if IsVault(cat) then
			data.isVault = true
			local singleVault = HoN_Store.Store2VaultData[cat][idx]
			if singleVault ~= nil then
				itemID = tonumber(singleVault['itemid'])
			end
		else
			data.isVault = false
			itemID = tonumber(catInfo['productIDs'][idx])

			-- prices
			data.gold	= tonumber(catInfo['productPrices'][idx])
			data.silver = tonumber(catInfo['premiumMmpCost'][idx])
			data.purchasable = IsPurchasable(catInfo, idx)

			data.owned 	= AtoB(catInfo['owned'][idx])
			data.bCoinEnough = GetCvarInt('_microStore_TotalCoins') >= data.gold or (GetCvarInt('_microStore_TotalSilverCoins') >= data.silver and data.silver >= 1)
		end

		data.itemID = itemID
		BoothInitializeOthersDetail(cat, data)

		SetVanityBoothSelectedInfo(cat, row, col, data)
	end

	TestEcho('InitializeVanityBoothUI show: '..tostring(show)..' category: '..tostring(cat)..' HoN_Store.BundleDetail.Category:'..HoN_Store.BundleDetail.Category)
	GetWidget('store2OthersDetailPanel'):SetVisible(0)

	if show then
		local vanityBoothWidget = GetWidget('store2_specials_booth_vanity')
		HoN_Store.currentTauntBadgeID = 0
		GetWidget('store2_vanity_switchbtn_root_vanity'):SetVisible(false)

		if cat == "dailySpecial" then
			vanityBoothWidget:SetVisible(0)
			Show_DailySpecial()
		elseif cat == 'others' or cat == 'vault_others' then
			if ShouldShowAsForm() then
				vanityBoothWidget:SetVisible(0)
				Show_OthersDetail()
			else
				vanityBoothWidget:SetVisible(1)
				Show_Vanity()
			end
		elseif cat ==  HoN_Store.BundleDetail.Category then
			vanityBoothWidget:SetVisible(1)
			Show_BundleDetail()
		elseif cat == 'NavBackToVanity' then
			vanityBoothWidget:SetVisible(1)
			Show_NavBackToVanity()
		else
			vanityBoothWidget:SetVisible(1)
			Show_Vanity()
		end
	end
end

function HoN_Store:Store2VanityChoose()
	ChooseVaultItemToUse()
end

function HoN_Store:AddNavHistoryVanity(category)

	local isVault = IsVault(category)

	local catInfo
	if not isVault then
		catInfo = HoN_Store.ProductPageInfo[category]
	end

	if Store2NavBackInProgress then 
		if catInfo and catInfo.fetchAllPages then
			HoN_Store:ResetVanityConditions(category)
		end
		return 
	end

	local lastData = HoN_Store:NavHistoryPeek()
	if lastData ~= nil and lastData.name == category then return end

	local symbolsCombobox = nil
	local symbolsComboboxIndex = nil
	if category == 'symbols' then
		symbolsCombobox = GetWidget('store2_combobox_symbols_types')
		symbolsComboboxIndex = symbolsCombobox:GetSelectedItemIndex()
	end

	local vanityFilters = {}

	local page
	if isVault then
		page = HoN_Store.paging[category].currentPage
	else
		page = catInfo.currentPage
		if catInfo.fetchAllPages then
			HoN_Store:SaveVanityConditions(category, vanityFilters)
			HoN_Store:ResetVanityConditions(category)
		end
	end

	local boothInfo = nil
	if HasBooth(category) then
		boothInfo = CopyTable(VanityBoothSelectInfo)
		boothInfo.CurrentPageName = HoN_Store:Store2GetCurrentPage()
		SetVanityBoothSelectedInfo(category, nil, nil)
		if isVault then
			boothInfo.currentPage = page
		end
	end

	local function NavigateToVanity()

		local hasBooth = boothInfo ~= nil
		if isVault and boothInfo == nil then
			boothInfo = {
				cat = category,
				currentPage = page,
			}
		end

		local vaultDataRequested = false
		if isVault then
			vaultDataRequested = HoN_Store.IsInVaultOrShop ~= 'vault'
			HoN_Store:Store2OpenVault(false)
		else
			HoN_Store:Store2OpenShop(false)
		end

		if category == 'symbols' then
			symbolsCombobox:SetSelectedItemByIndex(symbolsComboboxIndex)
		end

		HoN_Store.ProductPageInfo[category].currentPage = page

		if hasBooth then
			HoN_Store:Store2SetCurrentPage(boothInfo.CurrentPageName)
		end

		if not isVault then
			HoN_Store:Store2ShowPage(category)
		elseif not vaultDataRequested then
			HoN_Store:Store2FormGetVault()
		end

		if catInfo and catInfo.fetchAllPages then
			HoN_Store:RestoreVanityConditions(category, vanityFilters)
		end

		if hasBooth then
			VanityBoothSelectInfo = boothInfo
			if not isVault then
				HoN_Store:InitializeVanityBoothUI(true, 'NavBackToVanity', boothInfo)
				RefreshUpgrades()
			end
		end
		VanityBoothSelectInfo = boothInfo or {}
	end

	local data = {}
	data.name = category
	data.func = NavigateToVanity
	HoN_Store:NavHistoryPush(data)
end

function HoN_Store:Store2DailySpecialClear()
	DailySpecialJson = nil
end

function HoN_Store:Store2DailySpecialFormGet(force)
	local forceRequest = force or false
	if DailySpecialJson ~= nil and not forceRequest then return end
	SetRequestStatusWatch('MicroStoreDailySpecialStatus')

	HoN_Store:Store2Loading(1, "DailySpecialFormGet")
	local currentInterfaceName = UIManager.GetActiveInterface():GetName()
	if currentInterfaceName ~= 'main' then
		local w = GetWidget('store2_dailySpecialRequestHelper')
		w:Sleep(500, function()
			SubmitForm('StoreDailySpecial', 'account_id', Client.GetAccountID(), 'f', 'get_daily_special', 'cookie', Client.GetCookie())
		end)
	else
		SubmitForm('StoreDailySpecial', 'account_id', Client.GetAccountID(), 'f', 'get_daily_special', 'cookie', Client.GetCookie())
	end
end

function HoN_Store:GetDailySpecialPrices(typeStr)
	local map = {}
	local count = 0
	if DailySpecialJson ~= nil then
		for i, v in ipairs(DailySpecialJson) do
			if v.item_type == typeStr then
				local id = tostring(v.product_id)
				map[id] = {
					id		= id,
					gold 	= tonumber(v.current_gold_coins) or '',
					silver 	= tonumber(v.current_silver_coins) or '',
					discount_off = tonumber(v.discount_off) or '',
					initial_gold = tonumber(v.gold_coins) or '',
				}
				count = count + 1
			end
		end
	end
	return map, count
end

function TestPrintDailyJson(i)
	if DailySpecialJson == nil then Echo('^pnil') return end

	local t = DailySpecialJson[i]
	printTableDeep(t)
end

function HoN_Store.InitializeDailySpecialPage(self, responseString)
	function compareDailySpecialProduct(new, old)
		if type(new) ~= type(old) then return false end
		for k, v in pairs(new) do
			if k == 'productType' then
				for kk, vv in pairs(new[k]) do
					if vv ~= old[k][kk] then
						return false
					end
				end
			elseif new[k] ~= old[k] then
				return false
			end
		end
		return true
	end

	TestEcho('^rDailySpecialResponse:'..tostring(responseString))

	if not lib_json.RoughCheckJsonString(responseString) then
		Echo('^rInvalid DailySpecialResponse!!!')
		HoN_Store:Store2LoadingEndAsync('InitializeDailySpecialPageDone')
		return
	end

	local json = lib_json.decode(responseString)
	local lastestData = json.list

	--printTableDeep(lastestData)

	for idx = 1,5 do
		local single = lastestData[idx]
		single.id = single.panel_index

		single.productType = CheckCodeType(single.item_code, single.item_type)

		local productType = single.productType

		-- eap bundle code do not match avatar code, so just show hero model
		if productType.iseapbundle then
			local tmp = string.match(single.item_code, '(.+)%.([^%.]+)$')
			if tmp then
				single.item_code = tmp..'.eap'
			end
		elseif productType.isfeaturedbundle then
			-- featured bundle depends on the icon path to determine product code
			local iconPath = single.local_content or ''
			local dir = string.match(iconPath, '(.+)/([^/]+)$')
			if dir then
				local code = GetHeroAvatarCodeForPath(dir)
				if code and code ~= '' then
					productType.isbundle = false
					local isHero = string.match(code, '%.Hero$')
					if isHero then 
						productType.ishero = true
					else 
						productType.isavatar = true 
						code = 'aa.'..code
					end
					single.item_code = code
				end
			end
		end

		if not DailySpecialJson or not compareDailySpecialProduct(single, DailySpecialJson[idx]) then
			DailySpecialJson = DailySpecialJson or {}
			DailySpecialJson[idx] = single
			TestEcho('special item : '..tostring(idx)..' updated.')
			HoN_Store:InitializeVanityBoothUI(true, 'dailySpecial', single)
		end

		local productType = single.productType
		if productType.isavatar or productType.ishero then
			local code = single.item_code			
			if productType.isavatar then
				code = string.sub(code, 4)
			end

			local w = GetWidget('store2_specials_booth_model_'..idx)
			HoN_Store.widgetToHeroAvatarStrMap[w] = code
		end
	end

	Trigger('ShopkeeperIdleRoutine', 5, 180000)
	HoN_Store:Store2LoadingEndAsync("InitializeDailySpecialPageDone");
end

interface:RegisterWatch('MicroStoreDailySpecialResult', function(...) HoN_Store.InitializeDailySpecialPage(...) end)

function HoN_Store:ShowDailySpecialHoverTip(idx)
	if DailySpecialJson == nil then Echo('^r Error: nil DailySpecialJson') return end
	idx = tonumber(idx)
	local info = DailySpecialJson[idx]
	local name = info.item_cname
	local desc = info.item_type
	local productType = info.productType
	if productType.isavatar then
		local heroName = 'Hero_'..info.hero_name
		heroName = GetDisplayName(heroName)
		desc =  heroName.. ' ' .. desc
	end
	HoN_Store:ShowToolTip(name, desc)
end

function HoN_Store:HighlightDailySpecialBooth(boothPanel, id, highlight)
	if DailySpecialJson == nil then Echo('^r Error: nil DailySpecialJson') return end
	local modelPanel 		= boothPanel:GetWidget('store2_specials_booth_model_'..id)
	local highLightImage	= boothPanel:GetWidget('st2_sp_booth_jz_highlight_'..id)

	local iconPanel			= boothPanel:GetWidget('st2_sp_booth_icon_panel_'..id)
	local iconVisible		= iconPanel:IsVisibleSelf()

	local effectIndex 		= 1
	local effectPath 		= '/ui/fe2/elements/store2/selectedeffect/selected_light.effect'
	local fadeTime 			= 100

	if highlight then
		modelPanel:SetEffect(effectIndex, effectPath)
		highLightImage:FadeIn(fadeTime)
		if iconVisible then
			iconPanel:GetWidget('st2_sp_booth_icon_over_'..id):FadeIn(fadeTime)
		end

		if HoN_Store.MouseEverOverSpecials then
			idx = tonumber(id)
			local info = DailySpecialJson[idx]
			local code = info.item_code
			local productType = info.productType
			if productType.ishero or productType.iseapbundle then
				if string.match(code, '^h%.') then
					code = string.sub(code, 3)
				elseif string.match(code, '^aa%.') then
					code = string.sub(code, 4)
				end
				local t = string.match(code, '^([^%.]+)%.eap$')
				if t then
					code = t..'.Hero'
				elseif not string.match(code, '^[^%.]+%.Hero$') then
					code = code..'.Hero'
				end
			elseif productType.isavatar then
				code = string.sub(code, 4)
			else
				code = nil
			end
			if code then
				PlayHeroPreviewSoundFromProduct(code)
			end
		end
	else
		HoN_Store.MouseEverOverSpecials = true
		modelPanel:SetEffect(effectIndex, '')
		highLightImage:FadeOut(fadeTime)
		if iconVisible then
			iconPanel:GetWidget('st2_sp_booth_icon_over_'..id):FadeOut(fadeTime)
		end

		StopSound(1)
	end
end

HoN_Store.IsNavingJumpToVanity = false
function HoN_Store:Store2DailySpecialOnHide()
	if Store2NavBackInProgress and not HoN_Store.IsNavingJumpToVanity then return end

	local function NavigateToDailySpecials()
		HoN_Store:Store2OpenShop(false)
		Store2Tab('specials')
	end

	local data = {}
	data.name = 'DailySpecials'
	data.func = NavigateToDailySpecials
	HoN_Store:NavHistoryPush(data)
end

local function NavJumpToVanity(category, page, productID, icon)
	if not page or page < 1 then page = 1 end

	if category == 'eap' then
 		HoN_Store.Eap.ProductIdToInit = tostring(productID)
 		HoN_Store:SwitchToEaPage()
 	elseif category == 'featured' then
 		HoN_Store.Featured.ProductIdToInit = tostring(productID)
 		HoN_Store:SwitchToFeaturedPage()
 	else
		local vanity = HoN_Store.ProductPageInfo[category].vanityType
		HoN_Store.ProductPageInfo[category].currentPage = page

		local t = Store2NavBackInProgress
		Store2NavBackInProgress = true
		HoN_Store.IsNavingJumpToVanity  = true

		if category == 'bundles' then
			--HoN_Store:SwitchToBundleListPage(page)
			HoN_Store:SwitchToBundleDetailPageFromSpecials(productID, page, icon)
		else
			Trigger('store2VanityShow', vanity, category)
		end

		HoN_Store.IsNavingJumpToVanity  = false
		Store2NavBackInProgress = t

		local function ProductShowChooseProduct()

			local row, col = 1, 1
			local pageInfo = HoN_Store.ProductPageInfo[category]
			local elementPerRow = pageInfo['elementPerRow']
			local elementsPerPage = elementPerRow * pageInfo.rowsPerPage

			for it = 1, elementsPerPage do
				local idx = it
				if pageInfo.productIDs[idx] == productID then
					row = math.floor((it - 1) / elementPerRow) + 1
					col = (it - 1) % elementPerRow + 1
					break
				end
			end

			HoN_Store:VanityItemOnClick(true, category, row, col)
		end

		if category ~= 'bundles' and HasBooth(category) then
			HoN_Store.InitializeProductShowCallback = ProductShowChooseProduct
		end
	end
end

local function NavJumpToHeroDetail(productID, isAvatar)

	-- for edit
	if HoN_Store:EditEnabled() then
		for idx = 1,5 do
			local single = DailySpecialJson[idx]
			if single.product_id == tostring(productID) then
				local w = GetWidget('store2_specials_booth_model_'..idx)
				if HoN_Store.widgetToHeroAvatarStrMap[w] == nil then
					Echo('^r HoN_Store.widgetToHeroAvatarStrMap[w] == nil')
					return
				end
				HoN_Store:EditHeroFrame(w, nil, 'special')
				return
			end
		end
	end

	productID = tostring(productID)
	if isAvatar then
		HoN_Store.HeroDetailPage.AvatarProductIdToInit = productID
	else
		HoN_Store.HeroDetailPage.HeroProductIdToInit = productID
	end

	HoN_Store:NavToHeroAvatarDetailPage(isAvatar)
end

function HoN_Store:DailySpecialOnClicked(id)
	if DailySpecialJson == nil then Echo('^r Error: nil DailySpecialJson') return end
	local single = DailySpecialJson[id]
	local productType = single.productType
	local itemPage = single.item_page or 1

	if productType.iseapbundle then
		NavJumpToVanity("eap", itemPage, single.product_id)	
	elseif productType.isfeaturedbundle then
		NavJumpToVanity("featured", itemPage, single.product_id)
	elseif productType.isavatar then
		NavJumpToHeroDetail(single.product_id, true)
	elseif productType.iscolor then
		NavJumpToVanity("namecolors", itemPage, single.product_id)
	elseif productType.isicon then
		NavJumpToVanity("accounticons", itemPage, single.product_id)
	elseif productType.isannouncer then
		NavJumpToVanity("announcers", itemPage, single.product_id)
	elseif productType.issymbol then
		NavJumpToVanity("symbols", itemPage, single.product_id)
	elseif productType.istaunt then
		NavJumpToVanity("taunts", itemPage, single.product_id)
	elseif productType.iscouriers then
		NavJumpToVanity("couriers", itemPage, single.product_id)
	elseif productType.isward then
		NavJumpToVanity("wards", itemPage, single.product_id)
	elseif productType.isupgrade then
		NavJumpToVanity("upgrades", itemPage, single.product_id)
	elseif productType.isothers then
		NavJumpToVanity("others", itemPage, single.product_id)
	elseif productType.isbundle then
		NavJumpToVanity("bundles", itemPage, single.product_id, single.local_content)
	elseif productType.iscreep then
		Echo('Navigate to creeps? Not supported!')
	elseif productType.ishero then
		NavJumpToHeroDetail(single.product_id, false)
	elseif productType.istpeffect then
		NavJumpToVanity("tpeffects", itemPage, single.product_id)
	else
		Echo('^rHoN_Store:DailySpecialOnClicked Error, unknown product type!')
	end
end

function HoN_Store:SwitchToHeroAvatarPanelFromBuyAvatarPopup()

	local currentTab = HoN_Store:Store2GetCurrentPage()

	if currentTab == 'store2_hero_detail' then
		HoN_Store:ShowHeroAvatarPanels(1, 1)
	elseif currentTab == 'store2_specials' then
		local productID = GetCvarInt('_microStore_SelectedID')
		HoN_Store.HeroDetailPage.AvatarProductIdToInit = tostring(productID)

		HoN_Store:NavToHeroAvatarDetailPage(false)
	else
		Echo('^rSwitchToHeroAvatarPanelFromBuyAvatarPopup() unimplemented for tab '..currentTab)
	end
end

local TabMenuFade = {
	["accountvanity"] = {["shown"] = false, ["counter"] = 0, ["frame"] = 0},
	["gamevanity"] = {["shown"] = false, ["counter"] = 0, ["frame"] = 0},
	["vault_accountvanity"] = {["shown"] = false, ["counter"] = 0, ["frame"] = 0},
	["vault_gamevanity"] = {["shown"] = false, ["counter"] = 0, ["frame"] = 0},
}

function HoN_Store:TabMenuFadeIncrease(name, tag)
	TabMenuFade[name].counter = TabMenuFade[name].counter + 1
end

function HoN_Store:TabMenuFadeDecrease(name, tag)
	TabMenuFade[name].counter = TabMenuFade[name].counter - 1
end

function HoN_Store:TabMenuFadeCheck(name)
	if not TabMenuFade[name].shown then
		if TabMenuFade[name].counter > 0 then
			TabMenuFade[name].shown = true
			Trigger('store2TabMenuShow', name)
		end
	else
		if TabMenuFade[name].counter <= 0 then
			TabMenuFade[name].frame = TabMenuFade[name].frame + 1
			if TabMenuFade[name].frame > 3 then
				TabMenuFade[name].shown = false
				TabMenuFade[name].counter = 0
				TabMenuFade[name].frame = 0
				Trigger('store2TabMenuHide', name)
			end
		else
			TabMenuFade[name].frame = 0
		end
	end
end

function HoN_Store:Gift()
	Trigger('MicroStoreUpdateSelection')
	GetWidget('store_popup_confirm_purchase_gift'):DoEventN(0)
end

function HoN_Store:OnPurchase()
	Trigger('MicroStoreUpdateSelection')
	GetWidget('store_popup_confirm_purchase_choose'):DoEventN(0)
end

function HoN_Store:OnPurchaseVanity(cat, row, col)
	local cat = cat or VanityBoothSelectInfo["cat"]

	local function SetPurchaseParam(productId, localContent, gold, silver, charges, durations, chargesRemaining)
		Set('_microStore_SelectedItem', 999)
		Set('microStoreID999', productId)
		Set('_microStore_SelectedID', GetCvarInt('microStoreID999'))
		Set('microStoreLocalContent999', localContent)
		Set('microStorePrice999', gold)
		Set('microStoreSilverPrice999', silver)
		Set('_microStoreSelectedHeroItem', '', 'string')
		--Set('microStorePremium999', false)
		--Set('_microStore_Category', catId)
		
		Set('microStoreCharges999', charges or '-1~-1~-1~-1~-1~-1')
		Set('microStoreDurations999', durations or '-1~-1~-1~-1~-1~-1')
		Set('microStoreChargeRemaining999', chargesRemaining or '-1~-1~-1')
	end

	if HasBooth(cat) then
		if cat == 'bundleDetail' then
			local detail 		= HoN_Store.BundleDetail
			SetPurchaseParam(
				detail.BundleId,
				detail.Icon,
				detail.BundleCost,
				detail.BundleSilverCost,
				detail.charges,
				detail.durations,
				detail.chargesRemaining
				)
		elseif cat == 'others' then
			local page = VanityBoothSelectInfo["Page"]
			Set('_microStore_currentPage', page)
		else
			SetPurchaseParam(
				VanityBoothSelectInfo["productID"],
				VanityBoothSelectInfo["localContent"],
				VanityBoothSelectInfo["gold"],
				VanityBoothSelectInfo["silver"],
				VanityBoothSelectInfo["charges"],
				VanityBoothSelectInfo["durations"],
				VanityBoothSelectInfo["chargesRemaining"]
				)		
		end
	end
	HoN_Store:OnPurchase()
end

function HoN_Store:OnPurchaseAvatar()

	local buyAvatar = GetCvarNumber('_microStore_Category') == HoN_Store.RequestParam.avatars.catId
	if buyAvatar then
		local heroAvatarStr = nil

		local currentTab = HoN_Store:Store2GetCurrentPage()

		if currentTab == 'store2_hero_detail' then
			heroAvatarStr = HoN_Store.HeroDetailPage.CurrentHeroAvatarStr
		elseif currentTab == 'store2_specials' then
			local productID = GetCvarInt('_microStore_SelectedID')
			productID = tostring(productID)
			if DailySpecialJson == nil then Echo('^r Error: nil DailySpecialJson') return end
			for i, v in ipairs(DailySpecialJson) do
				if tostring(v.product_id) == productID then
					heroAvatarStr = v.item_name
					break
				end
			end
		else
			Echo('^rOnPurchase() unimplemented for tab '..currentTab)
			return
		end

		if heroAvatarStr == nil then
			Echo('^rOnPurchase() nil heroAvatarStr '..currentTab)
			return
		end

		local hasAccess = CanAccessHeroProduct(heroAvatarStr)

		if not hasAccess then
			GetWidget('store_popup_buyavatar_needhero'):DoEventN(0)
			return
		end
	end

	HoN_Store:OnPurchase()
end

function HoN_Store:OnPurchaseEapBundle()
	HoN_Store:OnPurchase()
end

function HoN_Store:OnPurchaseFeatureBundle()

	-- check if they own some of the products in the bundle, prompt them if they do
	local featured 		 = HoN_Store.Featured
	local selectedBundle = featured.SelectIndex
	local bundleIncludes = split(featured.BundleIncludes[selectedBundle], '~')

	local ownedProducts = false
	for i = 1, featured.AvatarCount do
		local included = bundleIncludes[i] == '1'
		if included and featured.ProductOwned[i] == '1' then
			ownedProducts = true
			break
		end
	end

	-- continue? dialog
	if ownedProducts then
		local cost = featured.BundleCosts[selectedBundle]
		local body = Translate("mstore_featured_owned_body", "cost", cost)
		GetWidget('store_popup_buyfeatured_ownedproduct_body'):SetText(body)
		GetWidget('store_popup_buyfeatured_ownedproduct'):DoEventN(0)

	else
		Set('_microStore_overrideConfirmName', true)
		HoN_Store:OnPurchase()
		Set('_microStore_overrideConfirmName', false)
	end
end

function HoN_Store:OnPurchaseSpecials(id)

	local function GetProductCateId(type)
		if type.iscolor then
			return HoN_Store.ProductPageInfo['namecolors']['categoryID']
		elseif type.iscouriers then
			return HoN_Store.ProductPageInfo['couriers']['categoryID']
		elseif type.iscreep then
			-- no creeps for purchase in shop actually
			return HoN_Store.ProductPageInfo['vault_creeps']['categoryID']
		elseif type.isward then
			return HoN_Store.ProductPageInfo['wards']['categoryID']
		elseif type.isavatar then
			return HoN_Store.RequestParam.avatars.catId
		elseif type.issymbol then
			return HoN_Store.ProductPageInfo['symbols']['categoryID']
		elseif type.istaunt then
			return HoN_Store.ProductPageInfo['taunts']['categoryID']
		elseif type.isannouncer then
			return HoN_Store.ProductPageInfo['announcers']['categoryID']
		elseif type.isothers then
			return HoN_Store.ProductPageInfo['others']['categoryID']
		elseif type.isicon then
			return HoN_Store.ProductPageInfo['accounticons']['categoryID']
		elseif type.ishero then
			return HoN_Store.RequestParam.heroes.catId
		elseif type.isupgrade then
			return HoN_Store.ProductPageInfo['upgrades']['categoryID']
		elseif type.istpeffect then
			return HoN_Store.ProductPageInfo['tpeffects']['categoryID']
		else
			Echo('^rGetProductCateId Error, unknown product type!')
			return -1
		end
	end

	if DailySpecialJson == nil then Echo('^r Error: nil DailySpecialJson') return end
	local single = DailySpecialJson[id]

	local productID = single.product_id
	local name = single.item_name
	local gold = single.current_gold_coins
	local silver = single.current_silver_coins
	local icon = single.local_content or GetHeroIcon2PathFromProduct(name)
	local page = single.item_page
	local hero = single.hero_name

	local displayName 	= ''
	if single.productType.isfeaturedbundle and single.productType.ishero then
		displayName = single.item_cname or ''
	else
		displayName = Translate("mstore_Hero_"..hero.."_name")
	end

	Echo('^y OnPurchaseSpecials')

	Set('_microStore_SelectedItem', 999)
	Set('microStoreID999', productID)
	Set('_microStore_SelectedID', GetCvarInt('microStoreID999'))
	Set('microStoreName999', '');
	Set('microStoreLocalContent999', icon)
	Set('microStorePrice999', gold)
	Set('microStoreSilverPrice999', silver)
	Set('microStorePremium999', true)
	Set('_microStore_currentPage', page)
	Set("_microStoreSelectedHeroItem", displayName)

	local productType = single.productType
	local catId = GetProductCateId(productType)

	Set('_microStore_Category', catId)
	Set('_microStore_SelectedItemOwned', false)

	if productType.isavatar then
		HoN_Store:OnPurchaseAvatar()
	else
		HoN_Store:OnPurchase()
	end
end

function HoN_Store:OnEnterInitRequest()
	-- this request is required to get initial informations such as santos
	local _microStore_Category = 2
	local _microStore_RequestCode = 0
	local _lastRequestHostTime = GetTime()
	local _microStoreRequestAllItems = 'false'
	local _microStoreShowNonPurchasable = 'false'
	local _microStore_currentPage = 0

	HoN_Store.RequestParam.lastRequestHostTime = _lastRequestHostTime
	Set('_lastRequestHostTime', _lastRequestHostTime)

	local currentInterfaceName = UIManager.GetActiveInterface():GetName()
	if currentInterfaceName ~= 'main' then
		local w = GetWidget('store2_firstEnterRequestHelper')
		w:Sleep(500, function()
		SubmitForm('MicroStore', 'account_id', Client.GetAccountID(), 'category_id', 0,
			'request_code', _microStore_RequestCode, 'page', _microStore_currentPage, 'cookie', Client.GetCookie(),
			'hostTime', _lastRequestHostTime, 'displayAll', _microStoreRequestAllItems, 'notPurchasable', _microStoreShowNonPurchasable)
		end)
	else
		local targetCategory = GetCvarInt('store2_initCategory')
		if targetCategory == 27 then
			HoN_Store:Store2ShowPage('taunts', true)
		elseif targetCategory == 77 then
			HoN_Store:Store2ShowPage('tauntbadges', true)
		elseif targetCategory == 5 then
			HoN_Store:Store2ShowPage('announcers', true)
		elseif targetCategory == 57 then
			HoN_Store:Store2ShowPage('couriers', true)
		elseif targetCategory == 74 then
			HoN_Store:Store2ShowPage('wards', true)
		elseif targetCategory == 78 then
			HoN_Store:Store2ShowPage('tpeffects', true)
		elseif targetCategory == 79 then
			HoN_Store:Store2ShowPage('selectioncircles', true)
		elseif targetCategory == 75 then
			HoN_Store:Store2ShowPage('upgrades', true)
		elseif targetCategory == 6 then
			HoN_Store:Store2ShowPage('others', true)
		elseif targetCategory == 7 then
			HoN_Store:SwitchToBundleListPage(1, false, true)
		elseif targetCategory == 3 then
			HoN_Store:Store2ShowPage('accounticons', true)
		elseif targetCategory == 16 then
			HoN_Store:Store2ShowPage('namecolors', true)
		elseif targetCategory == 4 then
			HoN_Store:Store2ShowPage('symbols', true)
		elseif targetCategory == 2 then
			HoN_Store:SwitchToAvatarListPage()
		elseif targetCategory == 68 then
			HoN_Store:SwitchToFeaturedPage(true)
		elseif targetCategory == 58 then
			HoN_Store:SwitchToEaPage(true)
		else
			SubmitForm('MicroStore', 'account_id', Client.GetAccountID(), 'category_id', 0,
			'request_code', _microStore_RequestCode, 'page', _microStore_currentPage, 'cookie', Client.GetCookie(),
			'hostTime', _lastRequestHostTime, 'displayAll', _microStoreRequestAllItems, 'notPurchasable', _microStoreShowNonPurchasable)
			return false
		end
		Set('store2_initCategory', 0, 'int')
		return true
	end
end

function HoN_Store:ArrangeChildrenCloseToCenter(parent)
	local count = 0
	local w = parent:GetChildren()
	for k, v in ipairs(w) do
		if v:IsVisible() then
			count = count + 1
		end
	end
	if count == 4 then
		parent:SetWidth('530i')
	elseif count == 3 then
		parent:SetWidth('400i')
	elseif count == 2 then
		parent:SetWidth('300i')
	end
	parent:SetAlign('center')
	local width = tostring(100 / count)..'%'
	for k, v in ipairs(w) do
		v:SetWidth(width)
	end
end

function HoN_Store:Store2BuildShaderStart()
	Echo("^g Store2BuildShaderStart")

	local w = GetWidget('store2ShaderBuilder', 'main')

	local function SetGraphics(level)
		UIManager.GetInterface('main'):HoNOptionsF('GraphicsSlideFunction', level)
	end

	local function CloseShop()
		Set('_mainmenu_currentpanel', 'store_container2') GetWidget('MainMenuPanelSwitcher'):DoEvent()
	end

	local function AvatarPages()
		Echo(tostring(GetWidget('store2_paging_arrow_avatars_next')))
		HoN_Store:OnClickPagingButton(GetWidget('store2_paging_arrow_avatars_next'), 'avatars', 'next')
		local maxPage = HoN_Store.paging.MaxPage('avatars', HoN_Store.ProductAvatars.Count)
		if HoN_Store.paging.avatars.currentPage < maxPage then
			return 'stay'
		end
	end

	local function OpenAltAvatars()
		HoN_Store.paging.avatars.currentPage = 1
		HoN_Store:SwitchToAvatarListPage()
	end

	local function HeroPages()
		HoN_Store:OnClickPagingButton(GetWidget('store2_paging_arrow_hero_next', 'main'), 'hero', 'next')
		local maxPage = HoN_Store.paging.MaxPage('hero', HoN_Store.ProductHeroes.Count)
		if HoN_Store.paging.hero.currentPage < maxPage then
			return 'stay'
		end
	end

	local function OpenHeroes()
		HoN_Store.paging.hero.currentPage = 1
		HoN_Store:SwitchToHeroListPage()
	end

	local function OpenEAOrFeatured()
		if HoN_Store:FreeHeros() then
			HoN_Store:SwitchToFeaturedPage()
		else
			HoN_Store:SwitchToEaPage()
		end
	end

	local function OpenShop()
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'store_container2', nil, false)
	end

	local function OpenOptions()
		w:UICmd("Set('_mainmenu_currentpanel', 'game_options'); CallEvent('MainMenuPanelSwitcher');")
	end

	local graphics = {1, 2, 3, 4}
	local currentGraphics = 1
	local function NextGrphics()
		currentGraphics = currentGraphics + 1
		return currentGraphics <= #graphics
	end

	local currentAvatarIndexForHeroDefault= 0
	local viewedHerosForHeroDefault = {}
	local function HeroesDetail()

		local loadingDoneTriggerFiring = false
		while currentAvatarIndexForHeroDefault < HoN_Store.ProductAvatars.Count do
			currentAvatarIndexForHeroDefault = currentAvatarIndexForHeroDefault + 1

			local code = HoN_Store.ProductAvatars.Codes[currentAvatarIndexForHeroDefault]
			local heroName = string.match(code,'aa%.([^%.]+).')
			if viewedHerosForHeroDefault[heroName] == nil then
				viewedHerosForHeroDefault[heroName] = 1
				HoN_Store.HeroDetailPage.AvatarIndexToInit = currentAvatarIndexForHeroDefault
				HoN_Store:InitHeroAvatarPanel(1)
				HoN_Store:SwitchToHeroAvatarPanel(true)
				loadingDoneTriggerFiring = true
				break
			end
		end

		if currentAvatarIndexForHeroDefault < HoN_Store.ProductAvatars.Count then
			return 'stay'
		end

		currentAvatarIndexForHeroDefault = 0
		viewedHerosForHeroDefault = {}

		if not loadingDoneTriggerFiring then
			return 'skipTrigger'
		end
	end

	local currentDetailedAvatar = 1
	local function AvatarsDetail()
		HoN_Store:ShowAvatarDetailPage(currentDetailedAvatar)
		currentDetailedAvatar = currentDetailedAvatar + 1
		if currentDetailedAvatar <= HoN_Store.ProductAvatars.Count then
			return 'stay'
		end
	end

	local shopAccessed = false
	local Steps = { {"OpenOptions", 15000, function() OpenOptions() end, nil},
					{"SetGraphics", 5000, function() SetGraphics(graphics[currentGraphics]) end, nil},
					{"OpenShop", 10000,
						function()
							currentDetailedHero = 1
							currentDetailedAvatar = 1
							OpenShop()
						end
					, 'store2LoadingDone'},
					{"SkipShop", 3000, 
						function() 
							if shopAccessed then
								return "CloseShop"
							end
						end,
						nil
					},
					{"MarkShopAccessed", 1000, function() shopAccessed = true end, nil},
					{"OpenVault", 2000, function() HoN_Store:Store2OpenVault() end, 'store2LoadingDone'},
					{"BackToShop", 2000, function() HoN_Store:Store2OpenShop() end, 'store2LoadingDone'},
					{"OpenEAOrFeatured", 3000,
						function()
							OpenEAOrFeatured()
							if HoN_Store:FreeHeros() then
								return 'OpenAltAvatars'
							end
						end
					, 'store2LoadingDone'},
					{"OpenHeroes", 2000, function() OpenHeroes() end, 'store2LoadingDone'},
					{"HeroPages", 2000, function() HeroPages() end, 'store2LoadingDone'},
					{"HeroPages2", 300, function() return HeroPages() end, 'store2LoadingDone'},
					{"OpenAltAvatars", 2000, function() OpenAltAvatars() end, 'store2LoadingDone'},
					{"AvatarPages", 2000, function() AvatarPages() end, 'store2LoadingDone'},
					{"AvatarPages2", 300, function() return AvatarPages() end, 'store2LoadingDone'},
					{"HeroDetail", 2000, function() HeroesDetail() end, 'store2LoadingDone'},
					{"HeroDetail2", 300, function() return HeroesDetail() end, 'store2LoadingDone'},
					{"AvatarsDetail", 3000, function() AvatarsDetail() end, 'store2LoadingDone'},
					{"AvatarsDetail2", 300, function() return AvatarsDetail() end, 'store2LoadingDone'},
					{"CloseShop", 1000, function() CloseShop() end, 'store2OptionsApply'},
					{"NextGrphics", 2000,
						function()
							local valid = NextGrphics()
							if valid then return 'OpenOptions' end
						end
					, nil},
					{"Cleanup", 3000,
						function()
							currentGraphics = 1
							currentAvatarType = 1
							overrideAvatarTypes = nil
						end
					, nil},
					{"Quit", 3000, function() quit() end, nil},
				  }

	local function GetIndexByName(name)
		for i = 1,#Steps do
			if Steps[i][1] == name then
				return i
			end
		end
	end

	local function SingleStep(index)
		step = Steps[index]
		if step == nil then return end
		local name, time, func, trigger = step[1], step[2], step[3], step[4]
		Echo("^g Store2BuildShaderStep: "..name)
		w:Sleep(time, function()
			local ret, ret2 = func()
			if ret == 'skipTrigger' then
				trigger = nil
				ret = ret2
			end
			if ret == 'stay' then
				nextIndex = index
			elseif ret ~= nil then
				nextIndex = GetIndexByName(ret)
			else
				nextIndex = index + 1
			end

			if trigger ~= nil then
				w:RegisterWatch(trigger,
					function()
						w:UnregisterWatch(trigger) SingleStep(nextIndex)
					end
				)
			else
				SingleStep(nextIndex)
			end
		end)
	end

	SingleStep(1)
end

function HoN_Store:Store2AddCardsListItems(product_id)

	Set('microStoreDiscountInfo', '')

	HoN_Store.GCardsTable = {}
	HoN_Store.CardLastSelect = -1

	local listControl = GetWidget('Store2_CardListControl_store')
	local listbox =	GetWidget('Store2_easy_CardListbox_store')

	local cardsTable = nil

	local heroIndex = HoN_Store:GetIndex(HoN_Store.ProductHeroes.Ids, tostring(product_id), false)
	if heroIndex ~= -1 then
		local heroName = HoN_Store.ProductHeroes.Names[heroIndex]
		cardsTable = GetCardsInfo(heroName, '')
	else
		local avatarIndex = HoN_Store:GetIndex(HoN_Store.ProductAvatars.Ids, tostring(product_id), false)
		if avatarIndex ~= -1 then
			local code = HoN_Store.ProductAvatars.Codes[avatarIndex]
			local nameTable = explode('.',string.gsub(code, 'aa.Hero_', 'Hero_'))
			local heroName = nameTable[1]
			local avatarName = nameTable[2]
			cardsTable = GetCardsInfo(heroName, avatarName)
		end
	end

	local cardlistPanel = GetWidget('CardListPanel_store')

	if (cardsTable == nil or #cardsTable <= 0) then
		cardlistPanel:SetVisible(false)
		return
	end
	
	for index, cardTable in pairs(cardsTable) do
		HoN_Store.GCardsTable[index - 1] = cardTable

		local cardName = nil
		if (cardTable.Coupon_id == "ext") then
			cardName = Translate('mstore_trial_name')
		else
			cardName = Translate('mstore_product'..tostring(cardTable.Product_id)..'_name')
		end

		listbox:AddTemplateListItem('Store2_Card_ListItem', '',
			'name', cardName,
			'discount', tostring(cardTable.Discount).."% off",
			'textcolor', '#925a22',
			'framecolor0', '#000000',
			'framecolor1', '#000000',
			'framecolor2', '#533f38')
	end

	local label = GetWidget('Store2_easy_CardDesc_store')
	label:SetText(Translate('mstore_nocards_desc'))
	label:SetColor("red")
	cardlistPanel:SetVisible(true)
end

function HoN_Store:Store2OnCardListSelect(index)
	local label = GetWidget('Store2_easy_CardDesc_store')

	if (index ~= HoN_Store.CardLastSelect) then
		HoN_Store.CardLastSelect = index

		if (HoN_Store.GCardsTable and HoN_Store.GCardsTable[index]) then
			local descStr = Translate("mstore_cards_desc").." "..HoN_Store.GCardsTable[index].EndTime

			label:SetVisible(true)
			label:SetText(descStr)
			label:SetColor("#4f2e0b")

			Set('microStoreDiscountInfo', HoN_Store.GCardsTable[index].Coupon_id)

			local goldPrice = GetCvarInt('microStoreCDPriceGold', true)
			local silverPrice = GetCvarInt('microStoreCDPriceSilver', true)
			local goldOwn = GetCvarInt('_microStore_TotalCoins', true)
			local silverOwn = GetCvarInt('_microStore_TotalSilverCoins', true)
			goldPrice = math.floor(goldPrice * (100 - HoN_Store.GCardsTable[index].Discount) * 0.01)
			silverPrice = math.floor(silverPrice * (100 - HoN_Store.GCardsTable[index].Discount) * 0.01)
			GetWidget('storeConfirmPurchaseGoldCost'):SetText(FtoA(goldPrice, 0, 0 ,","))
			GetWidget('storeConfirmPurchaseSilverCost'):SetText(FtoA(silverPrice, 0, 0 ,","))
			-- confirm purchase panel
			GetWidget('storeConfirmPurchaseGoldCost2'):SetText(FtoA(goldPrice, 0, 0 ,","))
			-- confirm purchase give gift panel
			GetWidget('storeConfirmPurchaseGoldCost3'):SetText(FtoA(goldPrice, 0, 0 ,","))
			GetWidget('storeConfirmPurchaseGoldCost4'):SetText(FtoA(goldPrice, 0, 0 ,","))

			if (goldPrice <= goldOwn) then
				GetWidget('storeConfirmPurchaseButtonBtnGold'):SetVisible(true)
				GetWidget('storeConfirmPurchaseButtonNeedGold2'):SetVisible(false)

				GetWidget('storeConfirmGiftButtonBtnGold'):SetVisible(true)
				GetWidget('storeConfirmPurchaseButtonNeedGold'):SetVisible(false)

				GetWidget('storeConfirmGiftButtonBtnGold2'):SetVisible(true)
				GetWidget('storeConfirmPurchaseButtonBtnGold2'):SetVisible(true)
			end
			if (silverPrice <= silverOwn) then
				GetWidget('storeConfirmPurchaseButtonBtnSilver'):SetVisible(true)
				GetWidget('storeConfirmPurchaseButtonNeedSilver'):SetVisible(false)
			end
		else
			label:SetVisible(false)
			Set('microStoreDiscountInfo', '')
		end
	else
		local listbox =	GetWidget('Store2_easy_CardListbox_store')
		listbox:SetSelectedItemByIndex(-1)
		HoN_Store.CardLastSelect = -1
		local goldPrice = GetCvarInt('microStoreCDPriceGold', true)
		local silverPrice = GetCvarInt('microStoreCDPriceSilver', true)
		local goldOwn = GetCvarInt('_microStore_TotalCoins', true)
		local silverOwn = GetCvarInt('_microStore_TotalSilverCoins', true)
		GetWidget('storeConfirmPurchaseGoldCost'):SetText(FtoA(goldPrice, 0, 0 ,","))
		GetWidget('storeConfirmPurchaseSilverCost'):SetText(FtoA(silverPrice, 0, 0 ,","))
		-- confirm purchase panel
		GetWidget('storeConfirmPurchaseGoldCost2'):SetText(FtoA(goldPrice, 0, 0 ,","))
		-- confirm purchase give gift panel
		GetWidget('storeConfirmPurchaseGoldCost3'):SetText(FtoA(goldPrice, 0, 0 ,","))
		GetWidget('storeConfirmPurchaseGoldCost4'):SetText(FtoA(goldPrice, 0, 0 ,","))

		Set('microStoreDiscountInfo', '')
		label:SetText(Translate('mstore_nocards_desc'))
		label:SetColor("red")
		if (goldPrice > goldOwn) then
			GetWidget('storeConfirmPurchaseButtonBtnGold'):SetVisible(false)
			GetWidget('storeConfirmPurchaseButtonNeedGold2'):SetVisible(true)

			GetWidget('storeConfirmGiftButtonBtnGold'):SetVisible(false)
			GetWidget('storeConfirmPurchaseButtonNeedGold'):SetVisible(true)
		end
		if (silverPrice > silverOwn) then
			GetWidget('storeConfirmPurchaseButtonBtnSilver'):SetVisible(false)
			GetWidget('storeConfirmPurchaseButtonNeedSilver'):SetVisible(true)
		end
	end
end

local Store2LoadingTime = {
	["Start"] = 0,
	["Done"] = 0,
	["Complete"] = 0,
}

local Store2LoadingCount = 0
function HoN_Store:Store2Loading(show, info)
	Echo("^gLoading .......... "..tostring(show).." "..tostring(info))
	Trigger('store2Loading', show)
end

function HoN_Store:Store2LoadingEndAsync(info)
	HoN_Store:Store2PendingJobsPush(
		function()
			if GetTime() - Store2LoadingTime.Start < GetCvarInt('store2LoadingMinMS') then
				if Store2LoadingTime.Done <= Store2LoadingTime.Start then
					Store2LoadingTime.Done = GetTime()
				end
				return false
			else
				HoN_Store:Store2Loading(0, info)
				return true
			end
		end
	)
end

function HoN_Store:Store2ProcessLoading(self, param)
	if param == -1 or param == '-1' then
		Store2LoadingCount = Store2LoadingCount - 1
	elseif param == 1 or param == '1' then
		Store2LoadingCount = Store2LoadingCount + 1
	elseif param == 0 or param == '0' then
		Store2LoadingCount = 0
	end

	if Store2LoadingCount < 0 then
		Store2LoadingCount = 0
	end

	if Store2LoadingCount == 0 then
	 	self:FadeOut(100)
	 	Store2LoadingTime.Complete = GetTime()
	 	if Store2LoadingTime.Done <= Store2LoadingTime.Start
	 		then Store2LoadingTime.Done = GetTime()
	 	end
	 	Trigger('store2LoadingDone')
	else
		Store2LoadingTime.Start = GetTime()
	 	self:FadeIn(100)
	end
end

function HoN_Store:Store2LoadingCheckTimeout()
	if Store2LoadingCount > 0 then
		if GetTime() - Store2LoadingTime.Start > 30000 then
			HoN_Store:Store2Loading(0, 'TIMEOUT')
		end
	end
end

function HoN_Store:Store2GetLoadingStatistics()

	 -- The loading is actually done
	local doneTime = Store2LoadingTime.Done - Store2LoadingTime.Start

	-- The loading screen is hidden. (Note we have a minimal loading time)
	local completeTime = Store2LoadingTime.Complete - Store2LoadingTime.Start

	if doneTime < 0 then
		doneTime = GetTime() - Store2LoadingTime.Start
	end

	if completeTime < 0 then
		completeTime = GetTime() - Store2LoadingTime.Start
	end

	return tostring(doneTime) .. ' / ' .. tostring(completeTime)
end

function HoN_Store:Store2ModelPanelHideContent(self)
	self:SetModel("/shared/models/invis.mdf")
	self:SetModel(0, "/shared/models/invis.mdf")
	self:SetModel(1, "/shared/models/invis.mdf")
	self:SetModel(2, "/shared/models/invis.mdf")
	self:SetEffect('')
	self:SetEffect(0, '')
	self:SetEffect(1, '')
	self:SetEffect(2, '')
end

function HoN_Store:Store2OnItemUsed(itemName)

	local function UsedOtherItems(productID)
		local strProductID = tostring(productID)
		-- before update the page, we need to update the vault data first.
		local j = 1
		local newData = {}
		for i=1,HoN_Store:GetVaultDataCount('vault_others') do
			local single = HoN_Store.Store2VaultData['vault_others'][i]
			if single.itemid ~= strProductID then
				newData[j] = single
				j = j + 1
			end
		end
		HoN_Store.Store2VaultData['vault_others'] = newData

		if HoN_Store.IsInVaultOrShop == 'vault' then
			HoN_Store:Store2ShowPage('vault_others', true)
		else
			HoN_Store:Store2ShowPage('others', true)
		end
	end

	local funcs = {
		["resetstats"] = function() UsedOtherItems(163) end,
		["namechange"] = function() UsedOtherItems(161) end,
		["subaccount"] = function() UsedOtherItems(162) end,
	}

	funcs[itemName]()
end

local function ResetResetStatsWidgetStats()
	GetWidget('store_resetstats_checkbox_pubstats'):SetButtonState(0)
	GetWidget('store_resetstats_checkbox_psr'):SetButtonState(0)
	GetWidget('store_resetstats_checkbox_smr'):SetButtonState(0)
	GetWidget('store_resetstats_checkbox_normalmm_stats'):SetButtonState(0)
	GetWidget('store_resetstats_checkbox_casualmm_stats'):SetButtonState(0)
	GetWidget('store_resetstats_checkbox_casual_mmr'):SetButtonState(0)
	GetWidget('store_resetstats_checkbox_midwars_stats'):SetButtonState(0)
	GetWidget('store_resetstats_confirmpass'):EraseInputLine()
	Set('_microStore_resetStats_psr', 0)
	Set('_microStore_resetStats_smr', 0)
	Set('_microStore_resetStats_normalmm_stats', 0)
	Set('_microStore_resetStats_casualmm_stats', 0)
	Set('_microStore_resetStats_casual_mmr', 0)
	Set('_microStore_resetStats_midwars_mmr', 0)
	Set('_microStore_resetStats_midwars_stats', 0)
	Set('_store_resetstats_confirm_pass', '')
	Set('_microStore_resetStats_pubstats', 0)
	Set('_microStore_resetStats_con_normal_stats', 0)

	if GetCvarBool('cl_GarenaEnable') then
		Set('_microStore_resetStats_con_casual_stats', 0)
	end
end

function HoN_Store:ProcessResetStatsForm(resultStr)
	HoN_Store:Store2Loading(0, "GotResetStatsResult")

	if resultStr == nil then
		Echo("^rProcessResetStatsForm Got nil result")
		return
	end

	local json = lib_json.decode(resultStr)
	if json == nil then
		Echo("^rProcessResetStatsForm Failed to parse json: "..resultStr)
		return
	end

	local txt = 'store2_resetstats_error_internal_error'
	local widget = GetWidget("storePopupResetStatsSuccessLabel")
	if json.success == true then
		txt = 'store2_resetstats_success'
		HoN_Store:Store2OnItemUsed('resetstats')

	else
		if json.errors == 'no options provided' then
			txt = 'store2_resetstats_error_no_options_provided'
		elseif json.errors == 'not enough stats reset' then
			txt = 'store2_resetstats_error_need_token'
		elseif json.errors == 'wrong password' then
			txt = 'store2_resetstats_error_wrong_password'
		elseif json.errors == 'internal error' then
			txt = 'store2_resetstats_error_internal_error'
		end

	end

	widget:SetText(Translate(txt))
	GetWidget('store_popup_resetstats_success'):DoEventN(0)
end


interface:RegisterWatch('EventUITrigger', function(self, cmd, p1)
	if cmd == 'screenEffect' then
		local effectPath = p1
		local w = GetWidget("store2_hero_detail_screen_effect")
		w:SetVisible(1)
		w:SetEffect(effectPath)
	end
end)


