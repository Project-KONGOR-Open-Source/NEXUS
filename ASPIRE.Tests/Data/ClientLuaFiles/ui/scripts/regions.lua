----------------------------------------------------------
--	Name: 		Region Interface Script            		--
--  Copyright 2015 Frostburn Studios					--
----------------------------------------------------------

local _G = getfenv(0)
HoN_Region = _G['HoN_Region'] or {}
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()
RegisterScript2('Region', '33')
HoN_Region.formInfo = [[, 'account_id', GetAccountID(), 'cookie', GetCookie(), 'region', GetRegion()]]
HoN_Region.regionOverride = GetCvarString('ui_overrideRegion', true)
HoN_Region.activeRegion = nil

local function getLangCodes()
	if GetCvarBool('releaseStage_stable') then
		return {'en', 'br', 'es', 'ru', 'ua' }
	else
		return {'en', 'fr', 'de', 'br', 'es', 'ro', 'ru', 'vn', 'th', 'id', 'my', 'cn', 'ua', 'tr', 'ko'}
	end
end

HoN_Region.regionTable = {

		-- default International region
		['international'] = {
			['regionCondition'] = ( (Empty(GetRegion()) or GetRegion() == 'int' or GetRegion() == 'en') and (not GetCvarBool('cl_GarenaEnable')) ),	-- If these conditions are met, activate this region
			['langCodes'] = getLangCodes(),	-- Language codes, used to populate language selection and various labels
			['serverCodes'] = { 'US', 'USE', 'USW', 'EU', 'AU', 'BR', 'RU', 'TOUR' },																		-- Server area codes, used in public games
			['tmmRegions'] = { 'usw', 'use', 'eu', 'au', 'br', 'ru' },																									-- Active Matchmaking regions
			['tmmRegionInfo'] = {																													-- Matchmaking regions info
				['usw'] = {FLAG = '/ui/icons/flags/unitedstates.tga', ID = 'USW'},
				['use'] = {FLAG = '/ui/icons/flags/unitedstates.tga', ID = 'USE'},
				['eu'] = {FLAG = '/ui/icons/flags/eu.tga', ID = 'EU'},
				['au'] = {FLAG = '/ui/icons/flags/australia.tga', ID = 'AU', DEFAULT_OFF = true},
				['lat'] = {FLAG = '/ui/icons/flags/lat.tga', ID = 'LAT', DEFAULT_OFF = true},
				['br'] = {FLAG = '/ui/icons/flags/brazil.tga', ID = 'BR', DEFAULT_OFF = true},
				['ru'] = {FLAG = '/ui/icons/flags/russia.tga', ID = 'RU', DEFAULT_OFF = true},
			},
			['cvars'] = {
				{'ui_regionalPartner', 'string', 'Frostburn Studios'},											-- Regional partner name
				{'ui_staffIconPath', 'string', '/ui/icons/frostburn.tga'},										-- Staff Icon path
				{'ui_staffStringName', 'string', 'mainlobby_icon_key_chat_staff'},						-- Staff name string
				{'ui_launcherRequiredString', 'string', 'main_label_login_garena'},						-- String displayed if login system disabled for this region and hon is executed
				{'ui_canCreateGame', 'bool', UIIsVerified},												-- Condition for accounts to create a game
				{'ui_promoCornerLeft', 'bool', 'true'},													-- Left corner button (HoN TV)
				{'ui_promoCornerRight', 'bool', 'true'},												-- Right corner button (Mentoring)
				{'ui_promoSpotlight', 'bool', 'true'},													-- Popup spotlights in this region
				{'ui_promoMOTD', 'bool', 'true'},														-- Popup news in this region
				{'ui_disableAccountCreation', 'bool', 'false'},											-- Standard 'slide panel' account creation forms. includes subaccounts, upgrades, gifting, name changes, and stat resets
				{'ui_disableAccountForms', 'bool', 'true'},												-- Standard 'slide panel' account creation forms. includes subaccounts, upgrades, gifting, name changes, and stat resets
				{'cg_altInfoColorScheme', 'int', '2', true},											-- Health bar style default
				{'ui_useRegionalCurrencyName', 'string', 'general_usd'},								-- Unit of currency measurement
				{'ui_useRegionalCurrencyIcon', 'string', ''},											-- Icon for regional currency (eg garena shell)
				{'ui_buyCoinsBlurb', 'string', 'mstore_buycoins_form_blurb'},							-- Explaination of coin purchasing, different in regions using regional currency
				{'ui_buyRegCurBlurb', 'string', ''},													-- Where the player can purchase regional currency (eg garena website - mstore_buycoins_regcur_info_garena)
				{'ui_background', 'int', '2'},															-- Which background to use (0 == none, 1 == ledge, 2 == newest)
				{'ui_showSpecialEventLogo', 'bool', 'false', true},										-- Show the current special event logo (e.g. honniversary)
				{'ui_showMalikenMessage', 'bool', 'false', true},										-- Show the current message from maliken
				{'ui_malikenMessagePath', 'string', '/ui/fe2/elements/hon_message_scroll.tga'},				-- Maliken message texture path
				{'ui_malikenMessageString', 'string', 'maliken_says'},									-- Maliken message string
				{'ui_dev_button', 'bool', 'true'},														-- Does this region have dev options
				{'ui_enableAllHeroes', 'bool', 'true'},
				{'ui_canBuyCoinsForFriend', 'bool', 'false'},											-- Enabled friend option in buy coins form
				{'ui_showCodeRedemption', 'bool', 'true'},												-- Enabled promo code redemption
				{'ui_lobby_showOldAAPicker', 'bool', 'false'},
				{'ui_region_accountCreationType', 'int', '0', true},
				{'ui_support_url', 'string', 'http://www.heroesofnewerth.com/support/'},
				{'ui_hon_url', 'string', 'http://www.heroesofnewerth.com'},
				{'ui_rap_enabled', 'bool', 'true'},
				{'ui_allow_store_tutorial', 'bool', 'true'},
				{'ui_raf_enabled', 'bool', 'false'},
				{'ui_options_referral_url', 'string', '!hon/whitelistfolder/referral_new/referral.php'},
				{'ui_raf_region_string', 'string', 'int'},
				{'ui_stats_eventStats', 'bool', 'true'}, 												-- Should the tournament stats show in the player stats area
				{'ui_regionLogoPath', 'string', '/ui/common/logo_garena.tga'},							-- region logo
			},
			['forms'] = {
				-- ClientLoginResponse error account creation prompt and nickname check (the orange one). These aren't used in international.
				--['createNewAccount'] = [[]], --['checkNickname'] = [[]]
			},
			['loginSytem'] = true,			-- Use the client login system
			['regionalCurrency'] = false,	-- Use regional currency instead of USD (shells, treecash)
			['replaySytem'] = true,			-- Activate replays in the UI
			['guidesEnabled'] = true,		-- Enable/disable entire guides system
			['guideCreation'] = false,
			['guideDetails'] = true,		-- Author and popularity
			['displayCCU'] = true,			-- Display CCU on the system bar
			['displayCCUBreakdown'] = function() return IsStaff() end, -- Display CCU breakdown on the system bar
			['scheduledMatches'] = true,	-- Enable scheduled match system (e.g. HoN Tour)
			['fbsplash'] = false,			-- Use the secondary Frostburn splash screen
			['customAccountIcons'] = true,	-- Enable purchasing custom account icons
			['canBuyCoins'] = true,			-- Enable the buy coins buttons (Disable in test cycles etc)
			['ladder'] = true,				-- Activate Player ladder in the UI
			['emailInactive'] = true, 		-- Whether the Bring Back To HoN Dialog allows them to email or not
			['useGoldCoinButton'] = true,	-- If gold coins can be purchased in-game
			['useGoldCoinWebpage'] = false,	-- If the gold coins window is actually awesomium
			['goldCoinsMessage'] = "region_naeu_buy_coins",		-- the string to display if gold coins can't be purchased in-game.
			['goldCoinsWebpage'] = 'https://www.heroesofnewerth.com/store/gold/',	-- the web address for the gold coins page if one is being used, the cookie and account id is appended to the end
			['goldCoinButtonTemplates'] 		= {'paperform_radiobutton_5_state1', 'paperform_radiobutton_5_state2', 'paperform_radiobutton_5_state3', 'paperform_radiobutton_5_state4', 'paperform_radiobutton_5_state5'},	-- Templates used by the buy coins screen
			['goldCoinButtonTemplateWidths'] 	= {'9.7h', '9.7h', '9.7h', '9.7h', '9.7h'},	-- Widths used by the buy coins screen
			['goldCoinButtonTemplateTooltips']  = {'mstore_tip_package_1', 'mstore_tip_package_2', 'mstore_tip_package_3', 'mstore_tip_package_4', 'mstore_tip_package_5'},	-- Tooltips used by the buy coins screen
			['goldCoinButtonAction'] 	= {'41', '42', '43', '44', '45'},	-- Shopkeeper actions used by the buy coins screen
			['hasEAP'] = false,				-- EAP or Special Avatars in Early Access, mostly just string changes
			['hasHolidayNotification'] = true,
			['questSystem']		= false,
			['facebookStreamURL'] = '!ascension/html/stream.html',
			['playerTourURL'] = '!ascension/html/index.html',
			['playerTourStatsURL'] = '!ascension/?r=season/stats',
			['playerTourMatchURL'] = '!ascension/?r=season/matchList',
			['playerTourNewsURL'] = '!ascension/?r=news/index',
			['playerTourLevelURL'] = '!ascension/?r=master/index',
			['playerTourTreasureURL'] = '!ascension/html/treasure.html',
			['playerTourProgressURL'] = '!ascension/?r=master/crossfunding',
			['playerLadderURL'] = 'http://naeu-icb2.s3.amazonaws.com/web/Player-Ladder/index.html',
			['questLadderURL'] = 'http://naeu-icb2.s3.amazonaws.com/web/Quest-Ladder/index.html',
			['codexLevelLadderURL'] = '',
			-- ['strikerLadderURL'] = '!ascension/?r=site/soccerladder',
			['masteryLadderAllURL'] = '!ascension/?r=site/masteryallladder', 
			['masteryLadderFriendsURL'] = '!ascension/?r=site/masteryfriendsladder', 
			['hasDice'] = false,		-- Rift wars
			['hasTokens'] = false,
			['hasPasses'] = false,
			['tmmAllowLeavers'] = false,
			['motdURL'] = {			-- Prefix for motd
				sbt		= 'http://fun.sbt.naeu.heroesofnewerth.com/events/common/client/motd/index.php',
				rct		= 'http://fun.rct.naeu.heroesofnewerth.com/events/common/client/motd/index.php',
				retail	= 'http://fun.naeu.heroesofnewerth.com/events/common/client/motd/index.php',
			},
			['tos_paths'] = {
				en		= 'tos',
				es		= 'tos_es',
				br		= 'tos_br',
			},
			['globalMessaging'] = true,
			['refereeControlPage'] = '!ascension/?r=match/bindpage',
			['gcaControlPage'] = 'http://www.hon.in.th/events/common/client/gca/benefit.php',
		},

		-- SEA region (garena)
		['sea'] = {
			['regionCondition'] = ( (Empty(GetRegion()) or GetRegion() == 'sea') and (GetCvarBool('cl_GarenaEnable')) ),
			['langCodes'] = {'en', 'th', 'id', 'my', 'cn'},
			['serverCodes'] = { 'SG', 'MY', 'PH', 'TH', 'ID', 'SEA', 'CN'},
			['tmmRegions']  = { 'sg', 'my', 'ph', 'th', 'id', 'cn'},
			['tmmRegionInfo'] = {
				['sg'] = {FLAG = "/ui/icons/flags/singapore.tga", ID = 'SG'},
				['my'] = {FLAG = "/ui/icons/flags/malaysia.tga", ID = 'MY'},
				['ph'] = {FLAG = "/ui/icons/flags/philippines.tga", ID = 'PH'},
				['th'] = {FLAG = "/ui/icons/flags/thailand.tga", ID = 'TH'},
				['id'] = {FLAG = "/ui/icons/flags/indonesia.tga", ID = 'ID'},
				['cn'] = {FLAG = "/ui/icons/flags/china.tga", ID = 'CN'},

			},
			['cvars'] = {
				{'ui_regionalPartner', 'string', 'Garena'},
				{'ui_staffIconPath', 'string', '/ui/icons/garena.tga'},
				{'ui_staffStringName', 'string', 'mainlobby_icon_key_chat_garenastaff'},
				{'ui_launcherRequiredString', 'string', 'main_label_login_garena'},
				{'ui_canCreateGame', 'bool', 'true'},
				{'ui_promoCornerLeft', 'bool', 'true'},
				{'ui_promoCornerRight', 'bool', 'false'},
				{'ui_promoSpotlight', 'bool', 'true'},
				{'ui_promoMOTD', 'bool', 'true'},
				{'ui_disableAccountCreation', 'bool', 'true'},
				{'ui_disableAccountForms', 'bool', 'true'},
				{'cg_altInfoColorScheme', 'int', '2', true},
				{'ui_useRegionalCurrencyName', 'string', 'general_shells'},
				{'ui_useRegionalCurrencyIcon', 'string', '/ui/fe2/elements/garena_shell.tga'},
				{'ui_buyCoinsBlurb', 'string', 'mstore_buycoins_form_blurb_garena'},
				{'ui_buyRegCurBlurb', 'string', 'mstore_buycoins_regcur_info_garena_sea'},
				{'ui_background', 'int', '1'},
				{'ui_showSpecialEventLogo', 'bool', 'false', true},
				{'ui_showMalikenMessage', 'bool', 'false', true},
				{'ui_malikenMessagePath', 'string', '/ui/fe2/elements/hon_message_scroll.tga'},
				{'ui_malikenMessageString', 'string', 'maliken_says_sea'},
				{'ui_dev_button', 'bool', 'false'},
				{'ui_enableAllHeroes', 'bool', 'false'},
				{'ui_canBuyCoinsForFriend', 'bool', 'false'},
				{'ui_showCodeRedemption', 'bool', 'false'},
				{'ui_lobby_showOldAAPicker', 'bool', 'false'},
				{'ui_region_accountCreationType', 'int', '0'},
				{'ui_support_url', 'string', 'http://www.garena.sg/support/'},
				{'ui_hon_url', 'string', 'http://www.garena.sg'},
				{'ui_rap_enabled', 'bool', 'true'},
				{'ui_allow_store_tutorial', 'bool', 'false'},
				{'ui_raf_enabled', 'bool', 'false'},
				{'ui_options_referral_url', 'string', '!hon/whitelistfolder/referral_new/SEA_CIS_referral.php'},
				{'ui_raf_region_string', 'string', 'garena'},
				{'ui_stats_eventStats', 'bool', 'false'},
				{'ui_regionLogoPath', 'string', '/ui/common/logo_garena.tga'},							-- region logo
			},
			['forms'] = {
				['createNewAccount'] = [[SubmitForm('GarenaCreateAccount', 'f', 'garena_register', 'token', GetGarenaToken(), 'nickname', region_create_account_nickname, 'referrer', region_create_account_referrer);]],
				['checkNickname'] = [[SubmitForm('GarenaUserNameCheck', 'nickname', region_create_account_nickname);']]
			},
			['loginSytem'] = false,
			['regionalCurrency'] = true,
			['replaySytem'] = true,
			['guidesEnabled'] = true,		-- Enable/disable entire guides system
			['guideCreation'] = false,
			['guideDetails'] = false,		-- Author and popularity
			['displayCCU'] = true,
			['displayCCUBreakdown'] = function() return IsStaff() end,
			['scheduledMatches'] = false,
			['fbsplash'] = true,
			['customAccountIcons'] = false,
			['canBuyCoins'] = true,
			['ladder'] = true,
			['emailInactive'] = false,
			['useGoldCoinButton'] = true,
			['useGoldCoinWebpage'] = false,
			['goldCoinsMessage'] = "",
			['goldCoinsWebpage'] = '',
			['goldCoinButtonTemplates'] = {'paperform_radiobutton_4_state1', 'paperform_radiobutton_4_state2', 'paperform_radiobutton_4_state3', 'paperform_radiobutton_4_state4'},
			['goldCoinButtonTemplateWidths'] 	= {'9h', '9h', '13.25h', '18.6h', '0'},
			['goldCoinButtonTemplateTooltips']  = {'mstore_tip_package_worst', 'mstore_tip_package_middle', 'mstore_tip_package_middle', 'mstore_tip_package_best', ''},
			['goldCoinButtonAction'] 	= {'11', '11', '10', '9', '9'},
			['hasEAP'] = true,
			['hasHolidayNotification'] = false,
			['questSystem']		= false,
			['facebookStreamURL'] = '!ascension/html/stream.html',
			['playerTourURL'] = '!ascension/html/index.html',
			['playerTourStatsURL'] = '!ascension/?r=season/stats',
			['playerTourMatchURL'] = '!ascension/?r=season/matchList',
			['playerTourNewsURL'] = '!ascension/?r=news/index',
			['playerTourLevelURL'] = '!ascension/?r=master/index',
			['playerTourTreasureURL'] = '!ascension/html/treasure.html',
			['playerTourProgressURL'] = '!ascension/?r=master/crossfunding',
			['playerLadderURL'] = 'http://sea-icb2.s3.amazonaws.com/web/Player-Ladder/index.html',
			['questLadderURL'] = 'http://sea-icb2.s3.amazonaws.com/web/Quest-Ladder/index.html',
			['codexLevelLadderURL'] = '!ascension/?r=season/ladder',
			-- ['strikerLadderURL'] = '!ascension/?r=site/soccerladder',
			['masteryLadderAllURL'] = '!ascension/?r=site/masteryallladder', 
			['masteryLadderFriendsURL'] = '!ascension/?r=site/masteryfriendsladder', 
			['hasDice'] = false,		-- Rift wars
			['hasTokens'] = true,
			['hasPasses'] = true,
			['tmmAllowLeavers'] = false,
			['motdURL'] = {			-- Prefix for motd
				sbt		= 'http://fun.sbt.naeu.heroesofnewerth.com/events/common/client/motd/index.php',
				rct		= 'http://fun.rct.sea.heroesofnewerth.com/events/common/client/motd/index.php',
				retail	= 'http://www.hon.in.th/events/common/client/motd/index.php',
			},
			['tos_paths'] = {
				en		= 'tos',
				es		= 'tos_es',
				br		= 'tos_br',
			},
			['globalMessaging'] = true,
			['refereeControlPage'] = '!ascension/?r=match/bindpage',
			['gcaControlPage'] = 'http://www.hon.in.th/events/common/client/gca/benefit.php',
		},

		-- CIS region (garena)
		['cis'] = {
			['regionCondition'] = ( GetRegion() == 'ru' and (GetCvarBool('cl_GarenaEnable')) ),
			['langCodes'] = {'en', 'ru', 'ua' },
			['serverCodes'] = { 'RU' },
			['tmmRegions']  = { 'ru' },
			['tmmRegionInfo'] = {
				['ru'] = {FLAG = "/ui/icons/flags/russia.tga", ID = 'RU'},
			},
			['cvars'] = {
				{'ui_regionalPartner', 'string', 'Garena'},
				{'ui_staffIconPath', 'string', '/ui/icons/garena.tga'},
				{'ui_staffStringName', 'string', 'mainlobby_icon_key_chat_garenastaff'},
				{'ui_launcherRequiredString', 'string', 'main_label_login_garena'},
				{'ui_canCreateGame', 'bool', 'true'},
				{'ui_promoCornerLeft', 'bool', 'true'},
				{'ui_promoCornerRight', 'bool', 'false'},
				{'ui_promoSpotlight', 'bool', 'true'},
				{'ui_promoMOTD', 'bool', 'true'},
				{'ui_disableAccountCreation', 'bool', 'true'},
				{'ui_disableAccountForms', 'bool', 'true'},
				{'cg_altInfoColorScheme', 'int', '2', true},
				{'ui_useRegionalCurrencyName', 'string', 'general_shells'},
				{'ui_useRegionalCurrencyIcon', 'string', '/ui/fe2/elements/garena_shell.tga'},
				{'ui_buyCoinsBlurb', 'string', 'mstore_buycoins_form_blurb_garena'},
				{'ui_buyRegCurBlurb', 'string', 'mstore_buycoins_regcur_info_garena_cis'},
				{'ui_background', 'int', '1'},
				{'ui_showSpecialEventLogo', 'bool', 'false', true},
				{'ui_showMalikenMessage', 'bool', 'false', true},
				{'ui_malikenMessagePath', 'string', '/ui/fe2/elements/hon_message_scroll.tga'},
				{'ui_malikenMessageString', 'string', 'maliken_says_cis'},
				{'ui_dev_button', 'bool', 'false'},
				{'ui_enableAllHeroes', 'bool', 'false'},
				{'ui_canBuyCoinsForFriend', 'bool', 'false'},
				{'ui_showCodeRedemption', 'bool', 'false'},
				{'ui_lobby_showOldAAPicker', 'bool', 'false'},
				{'ui_region_accountCreationType', 'int', '0'},
				{'ui_support_url', 'string', 'http://support.garena.ru/'},
				{'ui_hon_url', 'string', 'http://garena.ru/'},
				{'ui_rap_enabled', 'bool', 'true'},
				{'ui_allow_store_tutorial', 'bool', 'false'},
				{'ui_raf_enabled', 'bool', 'false'},
				{'ui_options_referral_url', 'string', '!hon/whitelistfolder/referral_new/SEA_CIS_referral.php'},
				{'ui_raf_region_string', 'string', 'garena'},
				{'ui_stats_eventStats', 'bool', 'false'},
				{'ui_regionLogoPath', 'string', '/ui/common/logo_garena.tga'},							-- region logo
			},
			['forms'] = {
				['createNewAccount'] = [[SubmitForm('GarenaCreateAccount', 'f', 'garena_register', 'token', GetGarenaToken(), 'nickname', region_create_account_nickname, 'referrer', region_create_account_referrer);]],
				['checkNickname'] = [[SubmitForm('GarenaUserNameCheck', 'nickname', region_create_account_nickname);']]
			},
			['loginSytem'] = false,
			['regionalCurrency'] = true,
			['replaySytem'] = true,
			['guidesEnabled'] = true,		-- Enable/disable entire guides system
			['guideCreation'] = false,
			['guideDetails'] = false,		-- Author and popularity
			['displayCCU'] = true,
			['displayCCUBreakdown'] = function() return IsStaff() end,
			['scheduledMatches'] = false,
			['fbsplash'] = true,
			['customAccountIcons'] = false,
			['canBuyCoins'] = true,
			['ladder'] = true,
			['emailInactive'] = false,
			['useGoldCoinButton'] = true,
			['useGoldCoinWebpage'] = false,
			['goldCoinsMessage'] = "",
			['goldCoinsWebpage'] = '',
			['goldCoinButtonTemplates'] = {'paperform_radiobutton_state1', 'paperform_radiobutton_state2', 'paperform_radiobutton_state3'},
			['goldCoinButtonTemplateWidths'] 	= {'10.5h', '14.75h', '23.25h', '0', '0'},
			['goldCoinButtonTemplateTooltips']  = {'mstore_tip_package_worst', 'mstore_tip_package_middle', 'mstore_tip_package_best', '', ''},
			['goldCoinButtonAction'] 	= {'11', '10', '9', '9', '9'},
			['hasEAP'] = true,
			['hasHolidayNotification'] = false,
			['questSystem']		= false,
			['facebookStreamURL'] = '!ascension/html/stream.html',
			['playerTourURL'] = '!ascension/html/index.html',
			['playerTourStatsURL'] = '!ascension/?r=season/stats',
			['playerTourMatchURL'] = '!ascension/?r=season/matchList',
			['playerTourNewsURL'] = '!ascension/?r=news/index',
			['playerTourLevelURL'] = '!ascension/?r=master/index',
			['playerTourTreasureURL'] = '!ascension/html/treasure.html',
			['playerTourProgressURL'] = '!ascension/?r=master/crossfunding',
			['playerLadderURL'] = 'http://cis-icb.s3.amazonaws.com/web/Player-Ladder/index.html',
			['questLadderURL'] = 'http://cis-icb.s3.amazonaws.com/web/Quest-Ladder/index.html',
			['codexLevelLadderURL'] = '',
			-- ['strikerLadderURL'] = '!ascension/?r=site/soccerladder',
			['masteryLadderAllURL'] = '!ascension/?r=site/masteryallladder', 
			['masteryLadderFriendsURL'] = '!ascension/?r=site/masteryfriendsladder', 
			['hasDice'] = false,		-- Rift wars
			['hasTokens'] = true,
			['hasPasses'] = true,
			['tmmAllowLeavers'] = false,
			['motdURL'] = {			-- Prefix for motd
				sbt		= 'http://cis-icb.s3.amazonaws.com/rct/web/motd/index.html',
				rct		= 'http://cis-icb.s3.amazonaws.com/rct/web/motd/index.html',
				retail	= 'http://cis-icb.s3.amazonaws.com/web/motd/index.html',
			},
			['tos_paths'] = {
				en		= 'tos',
				es		= 'tos_es',
				br		= 'tos_br',
				ru		= 'tos_ru',
			},
			['globalMessaging'] = false,
			['refereeControlPage'] = '!ascension/?r=match/bindpage',
			['gcaControlPage'] = 'http://www.hon.in.th/events/common/client/gca/benefit.php',
		},

		-- Turkey region (garena, based off CIS's systems)
		['tr'] = {
			['regionCondition'] = ( GetRegion() == 'tr' and (GetCvarBool('cl_GarenaEnable')) ),
			['langCodes'] = { 'tr', 'en' },
			['serverCodes'] = { 'TR' },
			['tmmRegions']  = { 'tr' },
			['tmmRegionInfo'] = {
				['tr'] = {FLAG = "/ui/icons/flags/turkey.tga", ID = 'TR'},
			},
			['cvars'] = {
				{'ui_regionalPartner', 'string', 'Garena'},
				{'ui_staffIconPath', 'string', '/ui/icons/garena.tga'},
				{'ui_staffStringName', 'string', 'mainlobby_icon_key_chat_garenastaff'},
				{'ui_launcherRequiredString', 'string', 'main_label_login_garena'},
				{'ui_canCreateGame', 'bool', 'true'},
				{'ui_promoCornerLeft', 'bool', 'true'},
				{'ui_promoCornerRight', 'bool', 'false'},
				{'ui_promoSpotlight', 'bool', 'true'},
				{'ui_promoMOTD', 'bool', 'true'},
				{'ui_disableAccountCreation', 'bool', 'true'},
				{'ui_disableAccountForms', 'bool', 'true'},
				{'cg_altInfoColorScheme', 'int', '2', true},
				{'ui_useRegionalCurrencyName', 'string', 'general_shells'},
				{'ui_useRegionalCurrencyIcon', 'string', '/ui/fe2/elements/garena_shell.tga'},
				{'ui_buyCoinsBlurb', 'string', 'mstore_buycoins_form_blurb_garena'},
				{'ui_buyRegCurBlurb', 'string', 'mstore_buycoins_regcur_info_garena_cis'},
				{'ui_background', 'int', '1'},
				{'ui_showSpecialEventLogo', 'bool', 'false', true},
				{'ui_showMalikenMessage', 'bool', 'false', true},
				{'ui_malikenMessagePath', 'string', '/ui/fe2/elements/hon_message_scroll.tga'},
				{'ui_malikenMessageString', 'string', 'maliken_says_cis'},
				{'ui_dev_button', 'bool', 'false'},
				{'ui_enableAllHeroes', 'bool', 'false'},
				{'ui_canBuyCoinsForFriend', 'bool', 'false'},
				{'ui_showCodeRedemption', 'bool', 'false'},
				{'ui_lobby_showOldAAPicker', 'bool', 'false'},
				{'ui_region_accountCreationType', 'int', '0'},
				{'ui_support_url', 'string', 'http://support.garena.ru/'},
				{'ui_hon_url', 'string', 'http://garena.ru/'},
				{'ui_rap_enabled', 'bool', 'true'},
				{'ui_allow_store_tutorial', 'bool', 'false'},
				{'ui_raf_enabled', 'bool', 'false'},
				{'ui_options_referral_url', 'string', '!hon/whitelistfolder/referral_new/SEA_CIS_referral.php'},
				{'ui_raf_region_string', 'string', 'garena'},
				{'ui_stats_eventStats', 'bool', 'false'},
				{'ui_regionLogoPath', 'string', '/ui/common/logo_garena.tga'},							-- region logo
			},
			['forms'] = {
				['createNewAccount'] = [[SubmitForm('GarenaCreateAccount', 'f', 'garena_register', 'token', GetGarenaToken(), 'nickname', region_create_account_nickname, 'referrer', region_create_account_referrer);]],
				['checkNickname'] = [[SubmitForm('GarenaUserNameCheck', 'nickname', region_create_account_nickname);']]
			},
			['loginSytem'] = false,
			['regionalCurrency'] = true,
			['replaySytem'] = true,
			['guidesEnabled'] = true,		-- Enable/disable entire guides system
			['guideCreation'] = false,
			['guideDetails'] = false,		-- Author and popularity
			['displayCCU'] = false,
			['displayCCUBreakdown'] = function() return IsStaff() end,
			['scheduledMatches'] = false,
			['fbsplash'] = true,
			['customAccountIcons'] = false,
			['canBuyCoins'] = true,
			['ladder'] = true,
			['emailInactive'] = false,
			['useGoldCoinButton'] = false,
			['useGoldCoinWebpage'] = false,
			['goldCoinsMessage'] = "region_turkey_buy_coins",
			['goldCoinsWebpage'] = '',
			['goldCoinButtonTemplates'] = {'paperform_radiobutton_state1', 'paperform_radiobutton_state2', 'paperform_radiobutton_state3'},
			['goldCoinButtonTemplateWidths'] 	= {'10.5h', '14.75h', '23.25h', '0', '0'},
			['goldCoinButtonTemplateTooltips']  = {'mstore_tip_package_worst', 'mstore_tip_package_middle', 'mstore_tip_package_best', '', ''},
			['goldCoinButtonAction'] 	= {'11', '10', '9', '9', '9'},
			['hasEAP'] = true,
			['hasHolidayNotification'] = false,
			['questSystem']		= false,
			['facebookStreamURL'] = '!ascension/html/stream.html',
			['playerTourURL'] = '!ascension/html/index.html',
			['playerTourStatsURL'] = '!ascension/?r=season/stats',
			['playerTourMatchURL'] = '!ascension/?r=season/matchList',
			['playerTourNewsURL'] = '!ascension/?r=news/index',
			['playerTourLevelURL'] = '!ascension/?r=master/index',
			['playerTourTreasureURL'] = '!ascension/html/treasure.html',
			['playerTourProgressURL'] = '!ascension/?r=master/crossfunding',
			['playerLadderURL'] = 'http://cis-icb.s3.amazonaws.com/web/Player-Ladder/index.html',
			['questLadderURL'] = 'http://cis-icb.s3.amazonaws.com/web/Quest-Ladder/index.html',
			['codexLevelLadderURL'] = '',
			-- ['strikerLadderURL'] = '!ascension/?r=site/soccerladder',
			['masteryLadderAllURL'] = '!ascension/?r=site/masteryallladder', 
			['masteryLadderFriendsURL'] = '!ascension/?r=site/masteryfriendsladder', 
			['hasDice'] = false,		-- Rift wars
			['hasTokens'] = false,
			['hasPasses'] = false,
			['tmmAllowLeavers'] = false,
			['motdURL'] = {			-- Prefix for motd
				sbt		= 'http://cis-icb.s3.amazonaws.com/rct/web/motd/index.html',
				rct		= 'http://cis-icb.s3.amazonaws.com/rct/web/motd/index.html',
				retail	= 'http://cis-icb.s3.amazonaws.com/web/motd/index.html',
			},
			['tos_paths'] = {
				en		= 'tos',
				es		= 'tos_es',
				br		= 'tos_br',
			},
			['globalMessaging'] = false,
			['refereeControlPage'] = '!ascension/?r=match/bindpage',
			['gcaControlPage'] = 'http://www.hon.in.th/events/common/client/gca/benefit.php',
		},

		-- Korea region (UNUSED, should probably remove)
		['korea'] = {
			['regionCondition'] = ( GetRegion() == 'ko' and (not GetCvarBool('cl_GarenaEnable')) ),
			['langCodes'] = {'en', 'ko'},
			['serverCodes'] = { 'KR' },
			['tmmRegions']  = { 'kr' },
			['tmmRegionInfo'] = {
				['kr'] = {FLAG = "/ui/icons/flags/southkorea.tga", ID = 'KR'},
			},
			['cvars'] = {
				{'ui_regionalPartner', 'string', 'Ntreev'},
				{'ui_staffIconPath', 'string', '/ui/icons/gm_icon.tga'},
				{'ui_staffStringName', 'string', 'mainlobby_icon_key_chat_ntreevstaff'},
				{'ui_launcherRequiredString', 'string', 'main_label_login_ntreev'},
				{'ui_canCreateGame', 'bool', 'true'},
				{'ui_promoCornerLeft', 'bool', 'false'},
				{'ui_promoCornerRight', 'bool', 'false'},
				{'ui_promoSpotlight', 'bool', 'true'},
				{'ui_promoMOTD', 'bool', 'true'},
				{'ui_disableAccountCreation', 'bool', 'true'},
				{'ui_disableAccountForms', 'bool', 'true'},
				{'cg_altInfoColorScheme', 'int', '2', true},
				{'ui_useRegionalCurrencyName', 'string', 'general_treecash'},
				{'ui_useRegionalCurrencyIcon', 'string', '/ui/fe2/elements/treecash.tga'},
				{'ui_buyCoinsBlurb', 'string', 'mstore_buycoins_form_blurb_ntreev'},
				{'ui_buyRegCurBlurb', 'string', 'mstore_buycoins_regcur_info_ntreev'},
				{'ui_background', 'int', '1'},
				{'ui_showSpecialEventLogo', 'bool', 'false', true},
				{'ui_showMalikenMessage', 'bool', 'false', true},
				{'ui_malikenMessagePath', 'string', '/ui/fe2/elements/hon_message_scroll.tga'},
				{'ui_malikenMessageString', 'string', 'maliken_says'},
				{'ui_dev_button', 'bool', 'false'},
				{'ui_enableAllHeroes', 'bool', 'false'},
				{'ui_canBuyCoinsForFriend', 'bool', 'false'},
				{'ui_showCodeRedemption', 'bool', 'false'},
				{'ui_lobby_showOldAAPicker', 'bool', 'false'},
				{'ui_region_accountCreationType', 'int', '0'},
				{'ui_support_url', 'string', 'http://www.gametree.co.kr/Customer/FAQList.aspx?CateCd=1&GameCd=14'},
				{'ui_hon_url', 'string', 'http://www.gametree.co.kr'},
				{'ui_rap_enabled', 'bool', 'false'},
				{'ui_allow_store_tutorial', 'bool', 'false'},
				{'ui_raf_enabled', 'bool', 'false'},
				{'ui_options_referral_url', 'string', '!hon/whitelistfolder/referral_new/KO_referral.php'},
				{'ui_raf_region_string', 'string', 'ko'},
				{'ui_stats_eventStats', 'bool', 'false'},
				{'ui_regionLogoPath', 'string', '/ui/common/logo_korea.tga'},							-- region logo
			},
			['forms'] = {
				['createNewAccount'] = [[SubmitForm('APICreateAccount', 'token', GetRegion(), 'serialized', '1', 'nickname', region_create_account_nickname, 'referrer', region_create_account_referrer);]],
				['checkNickname'] = {[[SubmitForm('APIUserNameCheck', 'token', GetRegion(), 'serialized', '1', 'nickCheck', '1');]], 'APIUserNameCheck', '!api', '/account/', 'region_create_account_nickname'}
			},
			['loginSytem'] = false,
			['regionalCurrency'] = true,
			['replaySytem'] = false,
			['guidesEnabled'] = false,		-- Enable/disable entire guides system
			['guideCreation'] = false,
			['guideDetails'] = false,		-- Author and popularity
			['displayCCU'] = IsStaff(),
			['displayCCUBreakdown'] = function() return IsStaff() end,
			['scheduledMatches'] = false,
			['fbsplash'] = true,
			['koreaRatings'] = true,
			['canBuyCoins'] = (GetCvarBool('releaseStage_stable') or GetCvarBool('releaseStage_rc')),
			['customAccountIcons'] = false,
			['ladder'] = false,
			['emailInactive'] = false,
			['useGoldCoinButton'] = true,
			['useGoldCoinWebpage'] = false,
			['goldCoinsMessage'] = "",
			['goldCoinsWebpage'] = '',
			['goldCoinButtonTemplates'] = {'paperform_radiobutton_state1', 'paperform_radiobutton_state2', 'paperform_radiobutton_state3'},
			['goldCoinButtonTemplateWidths'] 	= {'10.5h', '14.75h', '23.25h', '0', '0'},
			['goldCoinButtonTemplateTooltips']  = {'mstore_tip_package_worst', 'mstore_tip_package_middle', 'mstore_tip_package_best', '', ''},
			['goldCoinButtonAction'] 	= {'11', '10', '9', '9', '9'},
			['hasEAP'] = true,
			['hasHolidayNotification'] = false,
			['questSystem']		= false,
			['facebookStreamURL'] = '!ascension/html/stream.html',
			['playerTourURL'] = '!ascension/html/index.html',
			['playerTourStatsURL'] = '!ascension/?r=season/stats',
			['playerTourMatchURL'] = '!ascension/?r=season/matchList',
			['playerTourNewsURL'] = '!ascension/?r=news/index',
			['playerTourLevelURL'] = '!ascension/?r=master/index',
			['playerTourTreasureURL'] = '!ascension/html/treasure.html',
			['playerTourProgressURL'] = '!ascension/?r=master/crossfunding',
			['playerLadderURL'] = 'http://naeu-icb2.s3.amazonaws.com/web/Player-Ladder/index.html',
			['questLadderURL'] = 'http://naeu-icb2.s3.amazonaws.com/web/Quest-Ladder/index.html',
			['codexLevelLadderURL'] = '',
			-- ['strikerLadderURL'] = '!ascension/?r=site/soccerladder',
			['masteryLadderAllURL'] = '!ascension/?r=site/masteryallladder', 
			['masteryLadderFriendsURL'] = '!ascension/?r=site/masteryfriendsladder', 
			['hasDice'] = false,		-- Rift wars
			['hasTokens'] = false,
			['hasPasses'] = false,
			['tmmAllowLeavers'] = false,
			['motdURL'] = {			-- Prefix for motd
				sbt		= 'http://naeu-icb2.s3.amazonaws.com/sbt/web/motd/index.html',
				rct		= 'http://naeu-icb2.s3.amazonaws.com/rct/web/motd/index.html',
				retail	= 'http://naeu-icb2.s3.amazonaws.com/web/motd/index.html',
			},
			['tos_paths'] = {
				en		= 'tos',
				es		= 'tos_es',
				br		= 'tos_br',
			},
			['globalMessaging'] = false,
			['refereeControlPage'] = '!ascension/?r=match/bindpage',
			['gcaControlPage'] = 'http://www.hon.in.th/events/common/client/gca/benefit.php',
		},

		-- LAT region (UNUSED, should probably remove)
		['lat'] = {
			['regionCondition'] = ( GetRegion() == 'lat' and (not GetCvarBool('cl_GarenaEnable')) ),
			['langCodes'] = {'en', 'br', 'es'},
			['serverCodes'] = { 'LAT', 'BR' },
			['tmmRegions']  = { 'lat', 'br' },
			['tmmRegionInfo'] = {
				['lat'] = {FLAG = "/ui/icons/flags/lat.tga", ID = 'LAT'},
				['br'] = {FLAG = "/ui/icons/flags/brazil.tga", ID = "BR"},
			},
			['cvars'] = {
				{'ui_regionalPartner', 'string', 'Axeso5'},
				{'ui_staffIconPath', 'string', '/ui/icons/frostburn.tga'},
				{'ui_staffStringName', 'string', 'mainlobby_icon_key_chat_axeso5staff'},
				{'ui_launcherRequiredString', 'string', 'main_label_login_axeso5'},
				{'ui_canCreateGame', 'bool', UIIsVerified},
				{'ui_promoCornerLeft', 'bool', 'true'},
				{'ui_promoCornerRight', 'bool', 'false'},
				{'ui_promoSpotlight', 'bool', 'true'},
				{'ui_promoMOTD', 'bool', 'true'},
				{'ui_disableAccountCreation', 'bool', 'false'},
				{'ui_disableAccountForms', 'bool', 'true'},
				{'cg_altInfoColorScheme', 'int', '2', true},
				{'ui_useRegionalCurrencyName', 'string', 'general_axsocash'},
				{'ui_useRegionalCurrencyIcon', 'string', '/ui/fe2/elements/axesocash.tga'},
				{'ui_buyCoinsBlurb', 'string', 'mstore_buycoins_form_blurb_axeso5'},
				{'ui_buyRegCurBlurb', 'string', 'mstore_buycoins_regcur_info_axeso5'},
				{'ui_background', 'int', '2'},
				{'ui_showSpecialEventLogo', 'bool', 'false', true},
				{'ui_showMalikenMessage', 'bool', 'false', true},
				{'ui_malikenMessagePath', 'string', '/ui/fe2/elements/hon_message_scroll.tga'},
				{'ui_malikenMessageString', 'string', 'maliken_says_lat'},
				{'ui_dev_button', 'bool', 'false'},
				{'ui_enableAllHeroes', 'bool', 'true'},
				{'ui_canBuyCoinsForFriend', 'bool', 'false'},
				{'ui_showCodeRedemption', 'bool', 'false'},
				{'ui_lobby_showOldAAPicker', 'bool', 'false'},
				{'ui_region_accountCreationType', 'int', '1'},
				{'ui_support_url', 'string', 'http://tkts.axeso5.com/'},
				{'ui_hon_url', 'string', 'http://tkts.axeso5.com/'},
				{'ui_rap_enabled', 'bool', 'true'},
				{'ui_allow_store_tutorial', 'bool', 'true'},
				{'ui_raf_enabled', 'bool', 'false'},
				{'ui_options_referral_url', 'string', '!hon/whitelistfolder/referral_new/LAT_referral.php'},
				{'ui_raf_region_string', 'string', 'lat'},
				{'ui_stats_eventStats', 'bool', 'false'},
				{'ui_regionLogoPath', 'string', '/ui/common/logo_garena.tga'},							-- region logo
			},
			['forms'] = {
				['createNewAccount'] = [[SubmitForm('LATCreateAccount', 'f', 'create_lat_account', 'token', GetRegion(), 'login', create_paid_account_nickname, 'password', MD5(create_paid_account_pass), 'email', create_paid_account_email, 'gender', _form_create_paid_account_gender, 'first_name', create_paid_account_firstname, 'last_name', create_paid_account_lastname, 'country_id', create_paid_account_countryid, 'referrer', create_paid_account_referrer);]],
				['checkNickname'] = {[[SubmitForm('APIUserNameCheck', 'token', GetRegion(), 'serialized', '1', 'nickCheck', '1');]], 'APIUserNameCheck', '!api', '/account/', 'region_create_account_nickname'}
			},
			['loginSytem'] = true,
			['regionalCurrency'] = true,
			['replaySytem'] = true,
			['guidesEnabled'] = true,		-- Enable/disable entire guides system
			['guideCreation'] = false,
			['guideDetails'] = false,		-- Author and popularity
			['displayCCU'] = true,
			['displayCCUBreakdown'] = function() return IsStaff() end,
			['scheduledMatches'] = false,
			['fbsplash'] = false,
			['koreaRatings'] = false,
			['canBuyCoins'] = true,
			['customAccountIcons'] = false,
			['ladder'] = false,
			['emailInactive'] = false,
			['useGoldCoinButton'] = true,
			['useGoldCoinWebpage'] = false,
			['goldCoinsMessage'] = "",
			['goldCoinsWebpage'] = '',
			['goldCoinButtonTemplates'] 		= {'paperform_radiobutton_5_state1_lat', 'paperform_radiobutton_5_state2_lat', 'paperform_radiobutton_5_state3_lat', 'paperform_radiobutton_5_state4_lat', ''},	-- Templates used by the buy coins screen
			['goldCoinButtonTemplateWidths'] 	= {'12.165h', '12.165h', '12.165h', '12.165h', ''},	-- Widths used by the buy coins screen
			['goldCoinButtonTemplateTooltips']  = {'mstore_tip_package_1', 'mstore_tip_package_3', 'mstore_tip_package_4', 'mstore_tip_package_5', ''},	-- Tooltips used by the buy coins screen
			['goldCoinButtonAction'] 	= {'41', '43', '44', '45', ''},	-- Shopkeeper actions used by the buy coins screen
			['hasEAP'] = true,
			['hasHolidayNotification'] = false,
			['questSystem']		= false,
			['facebookStreamURL'] = '!ascension/html/stream.html',
			['playerTourURL'] = '!ascension/html/index.html',
			['playerTourStatsURL'] = '!ascension/?r=season/stats',
			['playerTourMatchURL'] = '!ascension/?r=season/matchList',
			['playerTourNewsURL'] = '!ascension/?r=news/index',
			['playerTourLevelURL'] = '!ascension/?r=master/index',
			['playerTourTreasureURL'] = '!ascension/html/treasure.html',
			['playerTourProgressURL'] = '!ascension/?r=master/crossfunding',
			['playerLadderURL'] = 'http://naeu-icb2.s3.amazonaws.com/web/Player-Ladder/index.html',
			['questLadderURL'] = 'http://naeu-icb2.s3.amazonaws.com/web/Quest-Ladder/index.html',
			['codexLevelLadderURL'] = '',
			-- ['strikerLadderURL'] = '!ascension/?r=site/soccerladder',
			['masteryLadderAllURL'] = '!ascension/?r=site/masteryallladder', 
			['masteryLadderFriendsURL'] = '!ascension/?r=site/masteryfriendsladder', 
			['hasDice'] = false,		-- Rift wars
			['hasTokens'] = false,
			['hasPasses'] = false,
			['tmmAllowLeavers'] = false,
			['motdURL'] = {			-- Prefix for motd
				sbt		= 'http://naeu-icb2.s3.amazonaws.com/sbt/web/motd/index.html',
				rct		= 'http://naeu-icb2.s3.amazonaws.com/rct/web/motd/index.html',
				retail	= 'http://naeu-icb2.s3.amazonaws.com/web/motd/index.html',
			},
			['tos_paths'] = {
				en		= 'tos',
				es		= 'tos_es',
				br		= 'tos_br',
			},
			['globalMessaging'] = false,
			['refereeControlPage'] = '!ascension/?r=match/bindpage',
			['gcaControlPage'] = 'http://www.hon.in.th/events/common/client/gca/benefit.php',
		},

		-- China region (UNUSED, should probably remove)
		['zh'] = {
			['regionCondition'] = ( GetRegion() == 'zh'),
			['langCodes'] = {'cn'},
			['serverCodes'] = { 'DX', 'LT' },
			['tmmRegions']  = { 'dx', 'lt'},
			['tmmRegionInfo'] = {
				['dx'] = {FLAG = "/ui/icons/flags/china.tga", ID = 'DX'},
				['lt'] = {FLAG = "/ui/icons/flags/china.tga", ID = 'LT'},
			},
			['cvars'] = {
				{'ui_regionalPartner', 'string', 'Garena'},
				{'ui_staffIconPath', 'string', '/ui/icons/garena.tga'},
				{'ui_staffStringName', 'string', 'mainlobby_icon_key_chat_garenastaff'},
				{'ui_launcherRequiredString', 'string', 'main_label_login_garena'},
				{'ui_canCreateGame', 'bool', 'true'},
				{'ui_promoCornerLeft', 'bool', 'false'},
				{'ui_promoCornerRight', 'bool', 'false'},
				{'ui_promoSpotlight', 'bool', 'true'},
				{'ui_promoMOTD', 'bool', 'true'},
				{'ui_disableAccountCreation', 'bool', 'true'},
				{'ui_disableAccountForms', 'bool', 'true'},
				{'cg_altInfoColorScheme', 'int', '2', true},
				{'ui_useRegionalCurrencyName', 'string', 'general_shells'},
				{'ui_useRegionalCurrencyIcon', 'string', '/ui/fe2/elements/garena_shell.tga'},
				{'ui_buyCoinsBlurb', 'string', 'mstore_buycoins_form_blurb_garena'},
				{'ui_buyRegCurBlurb', 'string', 'mstore_buycoins_regcur_info_garena_zh'},
				{'ui_background', 'int', '0'},
				{'ui_showSpecialEventLogo', 'bool', 'false', true},
				{'ui_showMalikenMessage', 'bool', 'false', true},
				{'ui_malikenMessagePath', 'string', '/ui/fe2/elements/hon_message_scroll.tga'},
				{'ui_malikenMessageString', 'string', 'maliken_says_sea'},
				{'ui_dev_button', 'bool', 'false'},
				{'ui_enableAllHeroes', 'bool', 'false'},
				{'ui_canBuyCoinsForFriend', 'bool', 'false'},
				{'ui_showCodeRedemption', 'bool', 'false'},
				{'ui_lobby_showOldAAPicker', 'bool', 'false'},
				{'ui_region_accountCreationType', 'int', '0'},
				{'ui_support_url', 'string', 'http://www.garena.cn/support/'},
				{'ui_hon_url', 'string', 'http://www.garena.cn'},
				{'ui_rap_enabled', 'bool', 'false'},
				{'ui_allow_store_tutorial', 'bool', 'false'},
				{'ui_raf_enabled', 'bool', 'false'},
				{'ui_options_referral_url', 'string', '!hon/whitelistfolder/referral_new/ZH_referral.php'},
				{'ui_raf_region_string', 'string', 'garena'},
				{'ui_stats_eventStats', 'bool', 'false'},
				{'ui_regionLogoPath', 'string', '/ui/common/logo_garena.tga'},							-- region logo
			},
			['forms'] = {
				['createNewAccount'] = [[SubmitForm('GarenaCreateAccount', 'f', 'garena_register', 'token', GetGarenaToken(), 'nickname', region_create_account_nickname, 'referrer', region_create_account_referrer);]],
				['checkNickname'] = [[SubmitForm('GarenaUserNameCheck', 'nickname', region_create_account_nickname);']]
			},
			['loginSytem'] = false,
			['regionalCurrency'] = true,
			['replaySytem'] = false,
			['guidesEnabled'] = false,		-- Enable/disable entire guides system
			['guideCreation'] = false,
			['guideDetails'] = false,		-- Author and popularity
			['displayCCU'] = false,
			['displayCCUBreakdown'] = false,
			['scheduledMatches'] = false,
			['fbsplash'] = true,
			['customAccountIcons'] = false,
			['canBuyCoins'] = false,
			['ladder'] = false,
			['emailInactive'] = false,
			['useGoldCoinButton'] = true,
			['useGoldCoinWebpage'] = false,
			['goldCoinsMessage'] = "",
			['goldCoinsWebpage'] = '',
			['goldCoinButtonTemplates'] = {'paperform_radiobutton_state1', 'paperform_radiobutton_state2', 'paperform_radiobutton_state3'},
			['goldCoinButtonTemplateWidths'] 	= {'10.5h', '14.75h', '23.25h', '0', '0'},
			['goldCoinButtonTemplateTooltips']  = {'mstore_tip_package_worst', 'mstore_tip_package_middle', 'mstore_tip_package_best', '', ''},
			['goldCoinButtonAction'] 	= {'11', '10', '9', '9', '9'},
			['hasEAP'] = true,
			['hasHolidayNotification'] = false,
			['questSystem']		= false,
			['facebookStreamURL'] = '!ascension/html/stream.html',
			['playerTourURL'] = '!ascension/html/index.html',
			['playerTourStatsURL'] = '!ascension/?r=season/stats',
			['playerTourMatchURL'] = '!ascension/?r=season/matchList',
			['playerTourNewsURL'] = '!ascension/?r=news/index',
			['playerTourLevelURL'] = '!ascension/?r=master/index',
			['playerTourTreasureURL'] = '!ascension/html/treasure.html',
			['playerTourProgressURL'] = '!ascension/?r=master/crossfunding',
			['playerLadderURL'] = 'http://naeu-icb2.s3.amazonaws.com/web/Player-Ladder/index.html',
			['questLadderURL'] = 'http://naeu-icb2.s3.amazonaws.com/web/Quest-Ladder/index.html',
			['codexLevelLadderURL'] = '',
			-- ['strikerLadderURL'] = '!ascension/?r=site/soccerladder',
			['masteryLadderAllURL'] = '!ascension/?r=site/masteryallladder', 
			['masteryLadderFriendsURL'] = '!ascension/?r=site/masteryfriendsladder', 
			['hasDice'] = false,		-- Rift wars
			['hasTokens'] = false,
			['hasPasses'] = false,
			['tmmAllowLeavers'] = false,
			['motdURL'] = {			-- Prefix for motd
				sbt		= 'http://naeu-icb2.s3.amazonaws.com/sbt/web/motd/index.html',
				rct		= 'http://naeu-icb2.s3.amazonaws.com/rct/web/motd/index.html',
				retail	= 'http://naeu-icb2.s3.amazonaws.com/web/motd/index.html',
			},
			['tos_paths'] = {
				en		= 'tos',
				es		= 'tos_es',
				br		= 'tos_br',
			},
			['globalMessaging'] = false,
			['refereeControlPage'] = '!ascension/?r=match/bindpage',
			['gcaControlPage'] = 'http://www.hon.in.th/events/common/client/gca/benefit.php',
		},

	}


local function RegionalCurrencySystem(isEnabled)
	if (isEnabled) then
		Cvar.CreateCvar('ui_useRegionalCurrency', 'bool', 'true')
		GetWidget('store_form_buycoins_regional_currency_display'):SetVisible(true)
		GetWidget('store_form_buycoins_regcur_submit'):SetVisible(true)
		GetWidget('store_form_buycoins_regcur_shellinfo'):SetVisible(true)

		GetWidget('store_form_buycoins_normal_submit'):SetVisible(false)
		GetWidget('store_form_buycoins_paypal_label'):SetVisible(false)
		GetWidget('store_form_buycoins_cc_input'):SetVisible(false)
	else
		Cvar.CreateCvar('ui_useRegionalCurrency', 'bool', 'false')
		GetWidget('store_form_buycoins_regional_currency_display'):SetVisible(false)
		GetWidget('store_form_buycoins_regcur_submit'):SetVisible(false)
		GetWidget('store_form_buycoins_regcur_shellinfo'):SetVisible(false)

		GetWidget('store_form_buycoins_normal_submit'):SetVisible(true)
		GetWidget('store_form_buycoins_paypal_label'):SetVisible(true)
		GetWidget('store_form_buycoins_cc_input'):SetVisible(true)
	end

	GetWidget('store_form_buycoins_blurb_label'):SetText(Translate(GetCvarString('ui_buyCoinsBlurb')))
	GetWidget('store_form_buy_regcur_info'):SetText(Translate(GetCvarString('ui_buyRegCurBlurb')))
end

local function LoginSystem(isEnabled)
	if (isEnabled) then
		Cvar.CreateCvar('ui_useClientLoginSystem', 'bool', 'true')
		if not isNewUI() then
			GetWidget('sysbar_center_langpicker'):SetVisible(false)
			GetWidget('sysbar_center_account_info'):SetVisible(true)
		end
		GetWidget('prelogin_options_panel_login'):SetVisible(true)
		GetWidget('prelogin_options_panel_nologin'):SetVisible(false)
		GetWidget('main_logging_in_warn'):SetVisible(false)
		GetWidget('main_logging_in'):SetVisible(true)
		GetWidget('prelogin_loginbox_disabled_nologin'):FadeOut(50)
		GetWidget('prelogin_loginbox_disabled_login'):SetVisible(true)
	else
		Cvar.CreateCvar('ui_useClientLoginSystem', 'bool', 'false')
		if not isNewUI() then
			GetWidget('sysbar_center_langpicker'):SetVisible(true)
			GetWidget('sysbar_center_account_info'):SetVisible(true)
		end
		GetWidget('prelogin_options_panel_login'):SetVisible(false)
		GetWidget('prelogin_options_panel_nologin'):SetVisible(true)
		GetWidget('main_logging_in_warn'):SetVisible(true)
		GetWidget('main_status_label'):SetText(Translate(GetCvarString('ui_launcherRequiredString')))
		if GetCvarBool('ui_dev') and (GetCvarBool('releaseStage_dev') or GetCvarBool('releaseStage_test')) then
			GetWidget('main_login_launcher_skip'):SetVisible(true)
		end
		GetWidget('prelogin_loginbox_disabled_nologin'):SetVisible(true)
		GetWidget('prelogin_loginbox_disabled_login'):SetVisible(false)
	end
end

local function ReplaySytem(isEnabled)
		if not isNewUI() then
		GetWidget('match_replays_download_replay_btn'):SetVisible(isEnabled)
		GetWidget('sysbar_replays_button'):SetVisible(isEnabled)
	end
end

local function GuidesSystem(isEnabled, canCreate, showDetails)
	GetWidget('compendium_guides_cover'):SetVisible(not isEnabled)
	GetWidget('compendium_guide_vote_thumbs_up_button'):SetVisible(showDetails)
	GetWidget('compendium_guide_vote_thumbs_down_button'):SetVisible(showDetails)
	GetWidget('learnatorium_guidePopularity'):SetVisible(showDetails)
	GetWidget('learnatorium_guideCreation'):SetVisible(canCreate)
end

local function CanBuyCoins(isEnabled)
	local wdgBuyCoin = GetCvarBool('cg_store2_') and GetWidget('store2_store_common_elements_button_buy_coin') or GetWidget('mstore_front_buycoins_btn')
	if (wdgBuyCoin) then
		if (isEnabled) then
			--println('CanBuyCoins: Enabled')
			GetWidget('sysbar_coins_gold'):UICmd([[SetOnClick('CallEvent(\'store_open_purchase_helper\', 2);')]])
			wdgBuyCoin:UICmd([[SetWatch('MicroStoreProcessing')]])
			wdgBuyCoin:SetEnabled(true)
			GetWidget('mstore_purchase_coins_submit_btn'):UICmd([[SetWatch('MicroStoreProcessing')]])
			GetWidget('mstore_purchase_coins_submit_btn'):UICmd([[SetWatch(1, 'UpdatePurchaseCoinsForm')]])
			GetWidget('mstore_purchase_coins_submit_btn'):SetEnabled(true)
			GetWidget('mstore_buycoins_form_blocker'):SetVisible(false)
		else
			--println('CanBuyCoins: Disabled')
			GetWidget('sysbar_coins_gold'):UICmd([[SetOnClick('')]])
			wdgBuyCoin:UICmd([[SetWatch('')]])
			wdgBuyCoin:SetEnabled(false)
			GetWidget('mstore_purchase_coins_submit_btn'):UICmd([[SetWatch('')]])
			GetWidget('mstore_purchase_coins_submit_btn'):UICmd([[SetWatch(1, '')]])
			GetWidget('mstore_purchase_coins_submit_btn'):SetEnabled(false)
			GetWidget('mstore_buycoins_form_blocker'):SetVisible(true)
		end
	end
end

local function CustomAccountIcons(isEnabled)
	if (isEnabled) then
		Cvar.CreateCvar('ui_enableCustomAccIcons', 'bool', 'true')
	else
		Cvar.CreateCvar('ui_enableCustomAccIcons', 'bool', 'false')
	end
end

local function UpdateRegionCvars()
	if (HoN_Region.regionTable) and (HoN_Region.regionTable[HoN_Region.activeRegion]) and (HoN_Region.regionTable[HoN_Region.activeRegion].cvars) then
		for index, cvarTable in pairs((HoN_Region.regionTable[HoN_Region.activeRegion].cvars)) do
			if (type(cvarTable[3]) == 'function') then
				--Cvar.CreateCvar(cvarTable[1], cvarTable[2], tostring(cvarTable[3]()) )
				Set(cvarTable[1], tostring(cvarTable[3]()), cvarTable[2], cvarTable[4])
			else
				--Cvar.CreateCvar(cvarTable[1], cvarTable[2], tostring(cvarTable[3]))
				Set(cvarTable[1],  tostring(cvarTable[3]), cvarTable[2], cvarTable[4])
				-- ( name               value                  type          nooverwrite
			end
		end
	end
end

local function LoginStatus(self, accountStatus, statusDescription, isLoggedIn, pwordExpired, isLoggedInChanged, updaterStatus)
	if (HoN_Region.regionTable) and (HoN_Region.regionTable[HoN_Region.activeRegion]) then
		if AtoB(isLoggedIn) then
			UpdateRegionCvars()
			if not isNewUI() then
				--GetWidget('sysbar_onlineinfo'):SetVisible(HoN_Region.regionTable[HoN_Region.activeRegion].displayCCU)
				--GetWidget('sysbar_onlineinfo'):SetNoClick(not HoN_Region.regionTable[HoN_Region.activeRegion].displayCCUBreakdown)
			end
			if (HonTour) and (HonTour.Initialize) then
				HonTour.isMatchScheduled = HonTour.isMatchScheduled or false
				HonTour.Initialize(HoN_Region.regionTable[HoN_Region.activeRegion].scheduledMatches)
			end
		else
			if (HonTour) and (HonTour.Initialize) then
				HonTour.isMatchScheduled = false
			end
			if not isNewUI() then
				--GetWidget('sysbar_onlineinfo'):SetVisible(false)
			end
		end
		if (GetWidget('midbar_button_ladder', nil, true)) then
			if (HoN_Region.regionTable[HoN_Region.activeRegion].ladder) then
				GetWidget('midbar_button_ladder'):SetEnabled(true)
				GetWidget('playerLadderTabCodexLevelParent'):SetVisible(HoN_Region:GetCodexLevelLadder() ~= '')
			else
				GetWidget('midbar_button_ladder'):SetEnabled(false)
			end
		end
	end
end

local function ActivateRegionPostLoad(_, region)
	GetWidget('systembar'):RegisterWatch('LoginStatus', LoginStatus)
	LoginSystem(HoN_Region.regionTable[region].loginSytem)
	ReplaySytem(HoN_Region.regionTable[region].replaySytem)
	GuidesSystem(HoN_Region.regionTable[region].guidesEnabled, HoN_Region.regionTable[region].guideCreation, HoN_Region.regionTable[region].guideDetails)
	CanBuyCoins(HoN_Region.regionTable[region].canBuyCoins)
	CustomAccountIcons(HoN_Region.regionTable[region].customAccountIcons)
	RegionalCurrencySystem(HoN_Region.regionTable[region].regionalCurrency)
	Trigger('UIUpdateRegion')
	interface:HoNMainF('UICheckForErrors')
	if (GetWidget('midbar_button_ladder', nil, true)) then
		if (HoN_Region.regionTable[HoN_Region.activeRegion].ladder) then
			GetWidget('midbar_button_ladder'):SetEnabled(true)
		else
			GetWidget('midbar_button_ladder'):SetEnabled(false)
		end
	end
	
	if not isNewUI() then
		--GetWidget('sysbar_onlineinfo'):SetVisible(HoN_Region.regionTable[region].displayCCU)
		--GetWidget('sysbar_onlineinfo'):SetNoClick(not HoN_Region.regionTable[HoN_Region.activeRegion].displayCCUBreakdown)
	end
	Main.InitMain(HoN_Region.regionTable[region].loginSytem)
end

local function ActivateRegion(_, region)
	println('^cUI: Region is ' .. tostring(region) )
	if (HoN_Region.regionTable) and (HoN_Region.regionTable[region]) then
		if GetCvarBool('_loggedin') then
			interface:UICmd("LogOut()")
		end
		HoN_Region.activeRegion = region
		Cvar.CreateCvar('ui_useClientLoginSystem', 'bool', tostring(HoN_Region.regionTable[region].loginSytem))
		SetActiveRegion(region)
		UpdateRegionCvars()
		if (HoN_Region.regionTable[region].fbsplash) then
			interface:Sleep(1,
				function()
					GetWidget('main_login_logo_splash_image'):SetTexture(GetCvarString('ui_regionLogoPath'))
					if (HoN_Region.regionTable[region].koreaRatings) then
						GetWidget('main_login_logo_splash_image_2'):SetTexture('/ui/icons/korea_rating_1.tga')
						GetWidget('main_login_logo_splash_image_3'):SetTexture('/ui/icons/korea_rating_3.tga')
					end
					GetWidget('main_login_logo_splash'):SetVisible(1)
					interface:Sleep(2800,
						function()
							GetWidget('main_login_logo_splash'):FadeOut(50)
							ActivateRegionPostLoad(_, region)
						end
					)
				end
			)
		else
			interface:Sleep(1,
				function()
					GetWidget('main_login_logo_splash'):SetVisible(0)
					ActivateRegionPostLoad(_, region)
				end
			)
		end
	end
end
interface:RegisterWatch('SetActiveRegion', ActivateRegion)

local function UIFindRegion()
	if (HoN_Region.regionOverride) and NotEmpty(HoN_Region.regionOverride) then
		ActivateRegion(nil, HoN_Region.regionOverride)
	elseif (HoN_Region.regionTable) then
		local useDefault = true
		for region, regionTable in pairs(HoN_Region.regionTable) do
			if (regionTable) and (regionTable.regionCondition) then
				useDefault = false
				ActivateRegion(nil, region)
				break
			end
		end
		if (useDefault) then
			ActivateRegion(nil, 'international')
			interface:Sleep(1, function() interface:HoNMainF('UICriticalError', 'UIFindRegion Failed', 2) end)
		end
	else
		interface:Sleep(1, function() interface:HoNMainF('UICriticalError', 'Region Table Failed To Load', 1) end)
	end
	UIFindRegion = nil
end
UIFindRegion()

function HoN_Region:OpenGoldCoinsWebpage()
	-- append the cookie to the URL
	local url = HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinsWebpage
	url = url..'?ingame=true&cookie='..Client.GetCookie()..'&accountid='..Client.GetAccountID()

	UIManager.GetInterface('webpanel'):HoNWebPanelF('LoadURLWithThrob', url, GetWidget("storePopupBuyCoinsForm_web"))
end

function HoN_Region:PopulatePointPackageButtons(widget)
	Echo('HoN_Region:PopulatePointPackageButtons HoN_Region.activeRegion: '..tostring(HoN_Region.activeRegion))
	if (HoN_Region.regionTable) and (HoN_Region.regionTable[HoN_Region.activeRegion]) then
		if (HoN_Region.regionTable[HoN_Region.activeRegion].useGoldCoinButton) and (HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplates) and (widget) then
			GetWidget("storePopupBuyCoinsForm_cover"):SetVisible(0)

			if (HoN_Region.regionTable[HoN_Region.activeRegion].useGoldCoinWebpage) then
				GetWidget("storePopupBuyCoinsForm_main"):SetVisible(0)
				GetWidget("storePopupBuyCoinsForm_web"):SetVisible(1)
			else
				GetWidget("storePopupBuyCoinsForm_main"):SetVisible(1)
				GetWidget("storePopupBuyCoinsForm_web"):SetVisible(0)

				if (HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplates[1]) then
					widget:Instantiate('form_radiobutton',
						'group', 'store_buycoins_radiobuttons_group',
						'tip_id', tostring(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplateTooltips[1]),
						'updatetrigger', 'UpdatePurchaseCoinsForm',
						'cvar', '_microStore_buyCoins_package',
						'value', '1',
						'color', 'white',
						'displaytemplate', tostring(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplates[1]),
						'height', '100%',
						'width', tostring(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplateWidths[1]),
						'onclick', 'Trigger(\'ShopkeeperAction\', ' .. tostring(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonAction[1]) .. ');',
						'radiobutton_name', 'microStorePackageButton1'
					)
				end
				if (HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplates[2]) then
					widget:Instantiate('form_radiobutton',
						'group', 'store_buycoins_radiobuttons_group',
						'tip_id', tostring(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplateTooltips[2]),
						'updatetrigger', 'UpdatePurchaseCoinsForm',
						'cvar', '_microStore_buyCoins_package',
						'value', '2',
						'color', 'white',
						'displaytemplate', tostring(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplates[2]),
						'height', '100%',
						'width', tostring(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplateWidths[2]),
						'onclick', 'Trigger(\'ShopkeeperAction\', ' .. tostring(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonAction[2]) .. ');',
						'radiobutton_name', 'microStorePackageButton2'
					)
				end
				if (HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplates[3]) then
					widget:Instantiate('form_radiobutton',
						'group', 'store_buycoins_radiobuttons_group',
						'tip_id', tostring(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplateTooltips[3]),
						'updatetrigger', 'UpdatePurchaseCoinsForm',
						'cvar', '_microStore_buyCoins_package',
						'value', '3',
						'color', 'white',
						'displaytemplate', tostring(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplates[3]),
						'height', '100%',
						'width', tostring(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplateWidths[3]),
						'onclick', 'Trigger(\'ShopkeeperAction\', ' .. tostring(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonAction[3]) .. ');',
						'radiobutton_name', 'microStorePackageButton3'
					)
				end
				if (HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplates[4]) then
					widget:Instantiate('form_radiobutton',
						'group', 'store_buycoins_radiobuttons_group',
						'tip_id', tostring(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplateTooltips[4]),
						'updatetrigger', 'UpdatePurchaseCoinsForm',
						'cvar', '_microStore_buyCoins_package',
						'value', '4',
						'color', 'white',
						'displaytemplate', tostring(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplates[4]),
						'height', '100%',
						'width', tostring(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplateWidths[4]),
						'onclick', 'Trigger(\'ShopkeeperAction\', ' .. tostring(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonAction[4]) .. ');',
						'radiobutton_name', 'microStorePackageButton4'
					)
				end
				if (HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplates[5]) then
					widget:Instantiate('form_radiobutton',
						'group', 'store_buycoins_radiobuttons_group',
						'tip_id', tostring(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplateTooltips[5]),
						'updatetrigger', 'UpdatePurchaseCoinsForm',
						'cvar', '_microStore_buyCoins_package',
						'value', '5',
						'color', 'white',
						'displaytemplate', tostring(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplates[5]),
						'height', '100%',
						'width', tostring(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonTemplateWidths[5]),
						'onclick', 'Trigger(\'ShopkeeperAction\', ' .. tostring(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinButtonAction[5]) .. ');',
						'radiobutton_name', 'microStorePackageButton5'
					)
				end
				widget:Instantiate('form_radiobutton',
					'visible', '0',
					'group', 'store_buycoins_radiobuttons_group',
					'tip_id', 'mstore_tip_package_1',
					'updatetrigger', 'UpdatePurchaseCoinsForm',
					'cvar', '_microStore_buyCoins_package',
					'value', '0',
					'color', 'white',
					'displaytemplate', 'paperform_radiobutton_state1',
					'height', '0',
					'width', '0',
					'onclick', '',
					'radiobutton_name', 'microStorePackageButton0'
				)
			end
		elseif (not HoN_Region.regionTable[HoN_Region.activeRegion].useGoldCoinButton) then
			GetWidget("storePopupBuyCoinsForm_main"):SetVisible(0)
			GetWidget("storePopupBuyCoinsForm_web"):SetVisible(0)
			GetWidget("storePopupBuyCoinsForm_cover"):SetVisible(1)

			if (HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinsMessage) then
				GetWidget("storePopupBuyCoinsForm_cover_text"):SetText(Translate(HoN_Region.regionTable[HoN_Region.activeRegion].goldCoinsMessage))
			end
		end
	end
end

function HoN_Region:PopulatePublicGameStaff(widget)
	if (HoN_Region.regionTable) and (HoN_Region.regionTable[HoN_Region.activeRegion]) and (HoN_Region.regionTable[HoN_Region.activeRegion].cvars) and (widget) then
		groupcall('icon_staff_keys', 'DoEvent(7)')
		widget:Instantiate('icon_key_item', 'image', GetCvarString('ui_staffIconPath'), 'label', Translate(GetCvarString('ui_staffStringName')), 'group', 'icon_staff_keys')
	end
end

function HoN_Region:PopulatePublicGameRegion(widget)
	if (HoN_Region.regionTable) and (HoN_Region.regionTable[HoN_Region.activeRegion]) and (widget) then
		widget:ClearItems()
		widget:AddTemplateListItem('Ncombobox_item', '', 'label', 'main_lobby_filter_any')
		for index, serverCode in pairs(HoN_Region.regionTable[HoN_Region.activeRegion].serverCodes) do
			widget:AddTemplateListItem('Ncombobox_item', serverCode, 'label', 'mainlobby_label_custom_game_'..serverCode)
		end
	end
end

function HoN_Region:GetLadderRegionInfo()
	if (HoN_Region.activeRegion) then
		return HoN_Region.activeRegion, HoN_Region.regionTable[HoN_Region.activeRegion].ladder
	end
end

function HoN_Region:GetEmailInactiveRegionInfo()
	if (HoN_Region.activeRegion) then
		return HoN_Region.activeRegion, HoN_Region.regionTable[HoN_Region.activeRegion].emailInactive
	end
end

function HoN_Region:GetMatchmakingRegionInfo()
	if (HoN_Region.activeRegion) then
		return HoN_Region.activeRegion, HoN_Region.regionTable[HoN_Region.activeRegion].tmmRegions, HoN_Region.regionTable[HoN_Region.activeRegion].tmmRegionInfo
	end
end

function HoN_Region:GetHasEAP()
	if (HoN_Region.activeRegion) then
		return HoN_Region.regionTable[HoN_Region.activeRegion].hasEAP
	else
		-- default to having EAP, as most regions will have it
		return true
	end
end

function HoN_Region:GetFacebookStream()
	if (HoN_Region.activeRegion) then
		return HoN_Region.regionTable[HoN_Region.activeRegion].facebookStreamURL
	else
		return ""
	end
end

function HoN_Region:GetPlayerTour()
	if (HoN_Region.activeRegion) then
		return HoN_Region.regionTable[HoN_Region.activeRegion].playerTourURL
	else
		return ""
	end
end

function HoN_Region:GetPlayerTourStats()
	if (HoN_Region.activeRegion) then
		return HoN_Region.regionTable[HoN_Region.activeRegion].playerTourStatsURL
	else
		return ""
	end
end

function HoN_Region:GetPlayerTourLevel()
	if (HoN_Region.activeRegion) then
		return HoN_Region.regionTable[HoN_Region.activeRegion].playerTourLevelURL
	else
		return ""
	end
end

function HoN_Region:GetPlayerTourMatch()
	if (HoN_Region.activeRegion) then
		return HoN_Region.regionTable[HoN_Region.activeRegion].playerTourMatchURL
	else
		return ""
	end
end

function HoN_Region:GetPlayerTourNews()
	if (HoN_Region.activeRegion) then
		return HoN_Region.regionTable[HoN_Region.activeRegion].playerTourNewsURL
	else
		return ""
	end
end

function HoN_Region:GetPlayerTourProgress()
	if (HoN_Region.activeRegion) then
		return HoN_Region.regionTable[HoN_Region.activeRegion].playerTourProgressURL
	else
		return ""
	end
end

function HoN_Region:GetPlayerTourTreasure()
	if (HoN_Region.activeRegion) then
		return HoN_Region.regionTable[HoN_Region.activeRegion].playerTourTreasureURL
	else
		return ""
	end
end

function HoN_Region:GetPlayerLadder()
	if (HoN_Region.activeRegion) then
		return HoN_Region.regionTable[HoN_Region.activeRegion].playerLadderURL..'#?lang='..GetCvarString('host_language')
	else
		return ""
	end
end

function HoN_Region:GetGCAControlPage()
	if (HoN_Region.activeRegion) then
		return HoN_Region.regionTable[HoN_Region.activeRegion].gcaControlPage
	else
		return ""
	end
end

function HoN_Region:GetRefereeControlPage()
	if (HoN_Region.activeRegion) then
		return HoN_Region.regionTable[HoN_Region.activeRegion].refereeControlPage
	else
		return ""
	end
end

function HoN_Region:GetQuestLadder()
	if (HoN_Region.activeRegion) then
		return HoN_Region.regionTable[HoN_Region.activeRegion].questLadderURL..'#?lang='..GetCvarString('host_language')
	else
		return ""
	end
end

function HoN_Region:GetCodexLevelLadder()
	if (HoN_Region.activeRegion) then
		return HoN_Region.regionTable[HoN_Region.activeRegion].codexLevelLadderURL
	else
		return ""
	end
end

--[[
function HoN_Region:GetStrikerLadder()
	if(HoN_Region.activeRegion) then
		return HoN_Region.regionTable[HoN_Region.activeRegion].strikerLadderURL
	else
		return ""
	end
end
]]

function HoN_Region:GetMasteryAllLadder()
	if(HoN_Region.activeRegion) then
		return HoN_Region.regionTable[HoN_Region.activeRegion].masteryLadderAllURL
	else
		return ""
	end
end

function HoN_Region:GetMasteryFriendsLadder()
	if(HoN_Region.activeRegion) then
		return HoN_Region.regionTable[HoN_Region.activeRegion].masteryLadderFriendsURL
	else
		return ""
	end
end

function HoN_Region:PopulateCustomGameRegion(widget)
	if (HoN_Region.regionTable) and (HoN_Region.regionTable[HoN_Region.activeRegion]) and (widget) then
		widget:ClearItems()
		widget:AddTemplateListItem('Ncombobox_item', '', 'label', 'main_lobby_filter_any')
		for index, serverCode in pairs(HoN_Region.regionTable[HoN_Region.activeRegion].serverCodes) do
			widget:AddTemplateListItem('Ncombobox_item', serverCode, 'label', 'mainlobby_label_custom_game_'..serverCode)
		end
	end
end

function HoN_Region:PopulateLanguageSelector(widget)
	if (HoN_Region.regionTable) and (HoN_Region.regionTable[HoN_Region.activeRegion]) and (widget) then
		local prevLang = widget:UICmd("GetSelectedItemName()")
		if Empty(prevLang) then prevLang = GetCvarString('host_language') end
		widget:ClearItems()
		for index, langCode in pairs(HoN_Region.regionTable[HoN_Region.activeRegion].langCodes) do
			widget:AddTemplateListItem('sysbar_combobox_lang_item', langCode, 'code', langCode, 'texture', '/ui/fe2/elements/flag_'..langCode..'.tga', 'label', 'lang_'..langCode)
		end
		widget:Sleep(500, widget:UICmd("SetSelectedItemByValue('"..prevLang.."', false)"))
	end
end

function HoN_Region:PopulateRegionSelector(widget)
	if (HoN_Region.regionTable) and (widget) then
		widget:ClearItems()
		for index, regionCode in pairs(HoN_Region.regionTable) do
			widget:AddTemplateListItem('sysbar_combobox_lang_item', index, 'label', index)
		end
		widget:Sleep(500, widget:UICmd("SetSelectedItemByValue('"..HoN_Region.activeRegion.."', false)"))
	end
end

function HoN_Region:CreateAccount(widget)
	if (HoN_Region.regionTable) and (HoN_Region.regionTable[HoN_Region.activeRegion]) and (HoN_Region.regionTable[HoN_Region.activeRegion].forms) and (HoN_Region.regionTable[HoN_Region.activeRegion].forms.createNewAccount) then
		println('^g Submit Form')
		println(HoN_Region.regionTable[HoN_Region.activeRegion].forms.createNewAccount)
		interface:UICmd(HoN_Region.regionTable[HoN_Region.activeRegion].forms.createNewAccount)
	else
		println('^r Error: No form exists for this region')
	end
end

function HoN_Region:CheckNickname(widget)
	if (HoN_Region.regionTable) and (HoN_Region.regionTable[HoN_Region.activeRegion]) and (HoN_Region.regionTable[HoN_Region.activeRegion].forms) and (HoN_Region.regionTable[HoN_Region.activeRegion].forms.checkNickname) then
		if type(HoN_Region.regionTable[HoN_Region.activeRegion].forms.checkNickname) == 'string' then
			interface:UICmd(HoN_Region.regionTable[HoN_Region.activeRegion].forms.checkNickname)
		elseif type(HoN_Region.regionTable[HoN_Region.activeRegion].forms.checkNickname) == 'table'	then
			SetFormHost(HoN_Region.regionTable[HoN_Region.activeRegion].forms.checkNickname[2], HoN_Region.regionTable[HoN_Region.activeRegion].forms.checkNickname[3])
			if type(HoN_Region.regionTable[HoN_Region.activeRegion].forms.checkNickname[5]) == 'string' then
				SetFormTarget(HoN_Region.regionTable[HoN_Region.activeRegion].forms.checkNickname[2], (HoN_Region.regionTable[HoN_Region.activeRegion].forms.checkNickname[4] .. interface:UICmd(HoN_Region.regionTable[HoN_Region.activeRegion].forms.checkNickname[5]) ))
			elseif type(HoN_Region.regionTable[HoN_Region.activeRegion].forms.checkNickname[5]) == 'function' then
				SetFormTarget(HoN_Region.regionTable[HoN_Region.activeRegion].forms.checkNickname[2], (HoN_Region.regionTable[HoN_Region.activeRegion].forms.checkNickname[4] .. HoN_Region.regionTable[HoN_Region.activeRegion].forms.checkNickname[5]() ))
			end
			interface:UICmd(HoN_Region.regionTable[HoN_Region.activeRegion].forms.checkNickname[1])
		end
	end
end

local function ClientLoginResponse(sourceWidget, param0, param1)
	println('^o ClientLoginResponse: ' .. param0)
end
interface:RegisterWatch('ClientLoginResponse', ClientLoginResponse)
interface:RegisterWatch('GarenaClientLoginResponse', ClientLoginResponse)

function interface:HoNRegionF(func, ...)
	if (HoN_Region[func]) then
		print(HoN_Region[func](self, ...))
	else
		print('HoNRegionF failed to find: ' .. tostring(func) .. '\n')
	end
end



