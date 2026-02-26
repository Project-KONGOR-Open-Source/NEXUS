-- Post-XML Store Lua

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind


-- "specials" is not a category
MSTORE_CATEGORY_1				= 1		-- can't remember what this is
MSTORE_CATEGORY_HERO_AVATARS	= 2
MSTORE_CATEGORY_ACCOUNT_ICONS	= 3
MSTORE_CATEGORY_SYMBOLS			= 4
MSTORE_CATEGORY_ANNOUNCERS		= 5
MSTORE_CATEGORY_MISC			= 6
MSTORE_CATEGORY_7				= 7			-- bundle (sale/good deal stuff) purchase?
MSTORE_CATEGORY_NAME_COLORS		= 16
MSTORE_CATEGORY_TAUNTS			= 27
MSTORE_CATEGORY_56				= 56		-- Unknown
MSTORE_CATEGORY_COURIERS		= 57
MSTORE_CATEGORY_EAP				= 58		-- EAP/Heroes
MSTORE_CATEGORY_FLAGS			= 59
MSTORE_CATEGORY_HERO_SYMBOLS	= 60
MSTORE_CATEGORY_MISC_SYMBOLS	= 61		-- Unused
MSTORE_CATEGORY_FEATURED_HEROES	= 68		-- Formally EAP
MSTORE_CATEGORY_HEROES			= 71
MSTORE_CATEGORY_CUSTOM_WARDS	= 74
MSTORE_CATEGORY_ENHANCEMENTS	= 75		-- 06102015
MSTROE_CATEGORY_CUSTOM_CREEP    = 76

MSTORE_RESPONSE_0						= 0
MSTORE_RESPONSE_1						= 1
MSTORE_RESPONSE_2						= 2
MSTORE_RESPONSE_3						= 3
MSTORE_RESPONSE_4						= 4
MSTORE_CURRENCY_PACKAGES_UPDATE			= 5
MSTORE_RESPONSE_VIEW_BUNDLE				= 6
MSTORE_RESPONSE_REFRESH_SELECTED_ITEMS	= 7

MSTORE_POPUP_0					= 0
MSTORE_POPUP_ERROR				= 1
MSTORE_POPUP_BUY_COIN_SUCCESS	= 2
MSTORE_POPUP_3					= 3
MSTORE_POPUP_4					= 4
MSTORE_POPUP_BUY_COIN_FRIEND	= 5
MSTORE_POPUP_6					= 6

MSTORE_ITEM_VIEW_BLANK					= 0
MSTORE_ITEM_VIEW_STORE_SHELF			= 1
MSTORE_ITEM_VIEW_STORE_BOOK				= 2
MSTORE_ITEM_VIEW_BOOK_ACC_ICONS			= 3
MSTORE_ITEM_VIEW_STORE_HEROES			= 4
MSTORE_ITEM_VIEW_STORE_HEROAVATARS		= 5
MSTORE_ITEM_VIEW_STORE_EAP				= 6
MSTORE_ITEM_VIEW_STORE_FEATURED			= 7
MSTORE_ITEM_VIEW_VAULT_SHELF			= 8
MSTORE_ITEM_VIEW_VAULT_BOOK				= 9

MSTORE_CATEGORIES_NONE		= 0
MSTORE_CATEGORIES_STORE		= 1
MSTORE_CATEGORIES_VAULT		= 2

MSTORE_NAVIGATION_NONE	= 0
MSTORE_NAVIGATION_STORE	= 1
MSTORE_NAVIGATION_VAULT	= 2

--[[
MSTORE_ITEM_VIEW_STORE_HERO_AVATARS		= 3

MSTORE_ITEM_VIEW_VAULT_SHELF			= 3
MSTORE_ITEM_VIEW_VAULT_BOOK				= 4
--]]

local tabs = {'specials', 'ea', 'featured', 'heroes', 'altavatars', 'accountvanity', 'gamevanity', 'others', 'bundles'}
local tabs_vault = {'vault_accountvanity', 'vault_gamevanity', 'vault_others'}

local currentStore2Tab = nil
local currentVaultTab = nil
local currentTabName = nil

function IsVault(tabName)
	local prefix = sub(tabName or '', 1, 6)
	return prefix == 'vault_'
end

function ResetAllTabs()
	for i=1, #tabs do
		GetWidget('store2_' .. tabs[i], 'main'):SetVisible(false)
		GetWidget('store2_tab_' .. tabs[i], 'main'):SetVisible(true)
		GetWidget('store2_tab_selected_' .. tabs[i], 'main'):SetVisible(false)
	end
	for i=1, #tabs_vault do
		GetWidget('store2_' .. tabs_vault[i], 'main'):SetVisible(false)
		GetWidget('store2_tab_' .. tabs_vault[i], 'main'):SetVisible(true)
		GetWidget('store2_tab_selected_' .. tabs_vault[i], 'main'):SetVisible(false)
	end
end

function Store2Tab(tabName)
	println('^c[Store2] ^*Switching to tab: ' .. tostring(tabName))
	println('^c[Store2] ^*Switching to tab: ' .. tostring(tabName))

	PlaySound('/shared/sounds/ui/button_click_05.wav')

	if IsVault(tabName) then
		if currentStore2Tab then
			GetWidget('store2_tab_' .. currentStore2Tab):SetVisible(true)
			GetWidget('store2_tab_selected_' .. currentStore2Tab):SetVisible(false)
		end

		currentStore2Tab = tabName
	else
		if currentVaultTab then
			GetWidget('store2_tab_' .. currentVaultTab):SetVisible(true)
			GetWidget('store2_tab_selected_' .. currentVaultTab):SetVisible(false)
		end

		currentVaultTab = tabName
	end

	GetWidget('store2_tab_' .. tabName):SetVisible(false)
	GetWidget('store2_tab_selected_' .. tabName):SetVisible(true)

	RefreshStore2Tabs(tabName)
end

function RefreshStore2Tabs(tabName)

	currentTabName = tabName
	-- Hide all tab menu first
	Trigger('store2TabMenuHide', 'all')
	Trigger('BeforeRefreshStore2Tabs', '')

	if IsVault(tabName) then
		for i=1, #tabs_vault do
			local isCurrentTab = tabs_vault[i] == tabName
			GetWidget('store2_' .. tabs_vault[i], 'main'):SetVisible(isCurrentTab)
		end
	else
		for i=1, #tabs do
			local isCurrentTab = tabs[i] == tabName
			GetWidget('store2_' .. tabs[i], 'main'):SetVisible(isCurrentTab)
		end
	end

end

function GetStore2CurrentTab()
	return currentTabName
end

local storeItemViews = {}		-- Mostly for later.

storeItemViews[MSTORE_CATEGORY_HERO_AVATARS]		= {
	store		= {
		viewItems		= MSTORE_ITEM_VIEW_STORE_HEROAVATARS,
		viewCategories	= MSTORE_CATEGORIES_NONE,
		viewNavigation	= MSTORE_NAVIGATION_NONE,
	},
	vault		= {
		viewItems		= MSTORE_ITEM_VIEW_BLANK,
		viewCategories	= MSTORE_CATEGORIES_NONE,
		viewNavigation	= MSTORE_NAVIGATION_NONE,
	},
}

storeItemViews[MSTORE_CATEGORY_ACCOUNT_ICONS]		= {
	store		= {
		viewItems		= MSTORE_ITEM_VIEW_STORE_BOOK,
		viewCategories	= MSTORE_CATEGORIES_STORE,
		viewNavigation	= MSTORE_NAVIGATION_STORE,
	},
	vault		= {
		viewItems		= MSTORE_ITEM_VIEW_VAULT_BOOK,
		viewCategories	= MSTORE_CATEGORIES_VAULT,
		viewNavigation	= MSTORE_NAVIGATION_VAULT,
	},
}

storeItemViews[MSTORE_CATEGORY_SYMBOLS]		= {
	store		= {
		viewItems		= MSTORE_ITEM_VIEW_STORE_BOOK,
		viewCategories	= MSTORE_CATEGORIES_STORE,
		viewNavigation	= MSTORE_NAVIGATION_STORE,
	},
	vault		= {
		viewItems		= MSTORE_ITEM_VIEW_VAULT_BOOK,
		viewCategories	= MSTORE_CATEGORIES_VAULT,
		viewNavigation	= MSTORE_NAVIGATION_VAULT,
	},
}

storeItemViews[MSTORE_CATEGORY_ANNOUNCERS]		= {
	store		= {
		viewItems		= MSTORE_ITEM_VIEW_STORE_SHELF,
		viewCategories	= MSTORE_CATEGORIES_STORE,
		viewNavigation	= MSTORE_NAVIGATION_STORE,
	},
	vault		= {
		viewItems		= MSTORE_ITEM_VIEW_VAULT_SHELF,
		viewCategories	= MSTORE_CATEGORIES_VAULT,
		viewNavigation	= MSTORE_NAVIGATION_VAULT,
	},
}

storeItemViews[MSTORE_CATEGORY_MISC]		= {
	store		= {
		viewItems		= MSTORE_ITEM_VIEW_STORE_SHELF,
		viewCategories	= MSTORE_CATEGORIES_STORE,
		viewNavigation	= MSTORE_NAVIGATION_STORE,
	},
	vault		= {
		viewItems		= MSTORE_ITEM_VIEW_VAULT_SHELF,
		viewCategories	= MSTORE_CATEGORIES_VAULT,
		viewNavigation	= MSTORE_NAVIGATION_VAULT,
	},
}

storeItemViews[MSTORE_CATEGORY_7]		= {
	store		= {
		viewItems		= MSTORE_ITEM_VIEW_STORE_SHELF,
		viewCategories	= MSTORE_CATEGORIES_STORE,
		viewNavigation	= MSTORE_NAVIGATION_STORE,
	},
	vault		= {
		viewItems		= MSTORE_ITEM_VIEW_VAULT_SHELF,
		viewCategories	= MSTORE_CATEGORIES_VAULT,
		viewNavigation	= MSTORE_NAVIGATION_VAULT,
	},
}

storeItemViews[MSTORE_CATEGORY_NAME_COLORS]		= {
	store		= {
		viewItems		= MSTORE_ITEM_VIEW_STORE_SHELF,
		viewCategories	= MSTORE_CATEGORIES_STORE,
		viewNavigation	= MSTORE_NAVIGATION_STORE,
	},
	vault		= {
		viewItems		= MSTORE_ITEM_VIEW_VAULT_SHELF,
		viewCategories	= MSTORE_CATEGORIES_VAULT,
		viewNavigation	= MSTORE_NAVIGATION_VAULT,
	},
}

storeItemViews[MSTORE_CATEGORY_TAUNTS]		= {
	store		= {
		viewItems		= MSTORE_ITEM_VIEW_STORE_SHELF,
		viewCategories	= MSTORE_CATEGORIES_STORE,
		viewNavigation	= MSTORE_NAVIGATION_STORE,
	},
	vault		= {
		viewItems		= MSTORE_ITEM_VIEW_VAULT_SHELF,
		viewCategories	= MSTORE_CATEGORIES_VAULT,
		viewNavigation	= MSTORE_NAVIGATION_VAULT,
	},
}

storeItemViews[MSTORE_CATEGORY_56]		= {			-- Unknown!
	store		= {
		viewItems		= MSTORE_ITEM_VIEW_STORE_SHELF,
		viewCategories	= MSTORE_CATEGORIES_STORE,
		viewNavigation	= MSTORE_NAVIGATION_STORE,
	},
	vault		= {
		viewItems		= MSTORE_ITEM_VIEW_VAULT_SHELF,
		viewCategories	= MSTORE_CATEGORIES_VAULT,
		viewNavigation	= MSTORE_NAVIGATION_VAULT,
	},
}

storeItemViews[MSTORE_CATEGORY_COURIERS]		= {
	store		= {
		viewItems		= MSTORE_ITEM_VIEW_STORE_SHELF,
		viewCategories	= MSTORE_CATEGORIES_STORE,
		viewNavigation	= MSTORE_NAVIGATION_STORE,
	},
	vault		= {
		viewItems		= MSTORE_ITEM_VIEW_VAULT_SHELF,
		viewCategories	= MSTORE_CATEGORIES_VAULT,
		viewNavigation	= MSTORE_NAVIGATION_VAULT,
	},
}

storeItemViews[MSTORE_CATEGORY_EAP]		= {
	store		= {
		viewItems		= MSTORE_ITEM_VIEW_STORE_EAP,
		viewCategories	= MSTORE_CATEGORIES_NONE,
		viewNavigation	= MSTORE_NAVIGATION_NONE,
	},
	vault		= {
		viewItems		= MSTORE_ITEM_VIEW_BLANK,
		viewCategories	= MSTORE_CATEGORIES_NONE,
		viewNavigation	= MSTORE_NAVIGATION_NONE,
	},
}

storeItemViews[MSTORE_CATEGORY_FLAGS]		= {
	store		= {
		viewItems		= MSTORE_ITEM_VIEW_STORE_BOOK,
		viewCategories	= MSTORE_CATEGORIES_STORE,
		viewNavigation	= MSTORE_NAVIGATION_STORE,
	},
	vault		= {
		viewItems		= MSTORE_ITEM_VIEW_VAULT_BOOK,
		viewCategories	= MSTORE_CATEGORIES_VAULT,
		viewNavigation	= MSTORE_NAVIGATION_VAULT,
	},
}

storeItemViews[MSTORE_CATEGORY_HERO_SYMBOLS]		= {
	store		= {
		viewItems		= MSTORE_ITEM_VIEW_STORE_BOOK,
		viewCategories	= MSTORE_CATEGORIES_STORE,
		viewNavigation	= MSTORE_NAVIGATION_STORE,
	},
	vault		= {
		viewItems		= MSTORE_ITEM_VIEW_VAULT_BOOK,
		viewCategories	= MSTORE_CATEGORIES_VAULT,
		viewNavigation	= MSTORE_NAVIGATION_VAULT,
	},
}

storeItemViews[MSTORE_CATEGORY_MISC_SYMBOLS]		= {
	store		= {
		viewItems		= MSTORE_ITEM_VIEW_STORE_BOOK,
		viewCategories	= MSTORE_CATEGORIES_STORE,
		viewNavigation	= MSTORE_NAVIGATION_STORE,
	},
	vault		= {
		viewItems		= MSTORE_ITEM_VIEW_VAULT_BOOK,
		viewCategories	= MSTORE_CATEGORIES_VAULT,
		viewNavigation	= MSTORE_NAVIGATION_VAULT,
	},
}

storeItemViews[MSTORE_CATEGORY_FEATURED_HEROES]		= {
	store		= {
		viewItems		= MSTORE_ITEM_VIEW_STORE_FEATURED,
		viewCategories	= MSTORE_CATEGORIES_NONE,
		viewNavigation	= MSTORE_NAVIGATION_NONE,
	},
	vault		= {
		viewItems		= MSTORE_ITEM_VIEW_BLANK,
		viewCategories	= MSTORE_CATEGORIES_NONE,
		viewNavigation	= MSTORE_NAVIGATION_NONE,
	},
}

storeItemViews[MSTORE_CATEGORY_FEATURED_HEROES]		= {
	store		= {
		viewItems		= MSTORE_ITEM_VIEW_STORE_HEROES,
		viewCategories	= MSTORE_CATEGORIES_NONE,
		viewNavigation	= MSTORE_NAVIGATION_NONE,
	},
	vault		= {
		viewItems		= MSTORE_ITEM_VIEW_BLANK,
		viewCategories	= MSTORE_CATEGORIES_NONE,
		viewNavigation	= MSTORE_NAVIGATION_NONE,
	},
}

storeItemViews[MSTORE_CATEGORY_CUSTOM_WARDS]		= {
	store		= {
		viewItems		= MSTORE_ITEM_VIEW_STORE_SHELF,
		viewCategories	= MSTORE_CATEGORIES_STORE,
		viewNavigation	= MSTORE_NAVIGATION_STORE,
	},
	vault		= {
		viewItems		= MSTORE_ITEM_VIEW_VAULT_SHELF,
		viewCategories	= MSTORE_CATEGORIES_VAULT,
		viewNavigation	= MSTORE_NAVIGATION_VAULT,
	},
}

storeItemViews[MSTORE_CATEGORY_ENHANCEMENTS]		= {
	store		= {
		viewItems		= MSTORE_ITEM_VIEW_STORE_SHELF,
		viewCategories	= MSTORE_CATEGORIES_STORE,
		viewNavigation	= MSTORE_NAVIGATION_STORE,
	},
	vault		= {
		viewItems		= MSTORE_ITEM_VIEW_VAULT_SHELF,
		viewCategories	= MSTORE_CATEGORIES_VAULT,
		viewNavigation	= MSTORE_NAVIGATION_VAULT,
	},
}

storeItemViews[MSTROE_CATEGORY_CUSTOM_CREEP]		= {
	store		= {
		viewItems		= MSTORE_ITEM_VIEW_STORE_SHELF,
		viewCategories	= MSTORE_CATEGORIES_STORE,
		viewNavigation	= MSTORE_NAVIGATION_STORE,
	},
	vault		= {
		viewItems		= MSTORE_ITEM_VIEW_VAULT_SHELF,
		viewCategories	= MSTORE_CATEGORIES_VAULT,
		viewNavigation	= MSTORE_NAVIGATION_VAULT,
	},
}

local interface = object

-- MSTORE_VIEW_SHELF		= 0
-- MSTORE_VIEW_BOOK			= 1
-- MSTORE_VIEW_AVATARS		= 2
-- MSTORE_VIEW_

local function explodeVar(delim1, input, varPrefix, varType)
	varType = varType or 'string'

	if input and type(input) == 'string' and varPrefix and type(varPrefix) == 'string' and delim1 and type(delim1) == 'string' and string.len(delim1) > 0 then
		for k,v in ipairs(Explode(delim1, input)) do
			Set(varPrefix..(k), v, varType)
		end
	end
end

local function hideEventIfVisible(widgetName)
	local widget = interface:GetWidget(widgetName)

	if widget and widget:IsVisible() then
		widget:DoEventN(1)
	end
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

local function refreshAllUpgrades()
	interface:UICmd("ChatRefreshUpgrades()")
	interface:UICmd("ClientRefreshUpgrades()")
	if AtoN(interface:UICmd("GamePhase")) >= 1 then
		interface:UICmd("ServerRefreshUpgrades()")
	end
end

local useCategoryID		= 2

GetWidget('microStoreResultHandler'):RegisterWatch('MicroStoreResults', function(widget,
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
	vaultCategory78,			-- param81
	vaultCategory79,			-- param82
	santa_event_expiration		-- param83
)
	println('^c[Store2] ^*MicroStoreResults Received.')
	println('^y[Store2] ^*Response: '..tostring(responseCode)..' Error: '..tostring(errorCode)..' Popup: '..tostring(popupCode))
	println('^y[Store2] ^*CategoryID: '..tostring(categoryID)..' ProductIDs: '..tostring(productIDs))
	println('^y[Store2] ^*TotalPoints: '..tostring(totalPoints)..' TotalMMPoints: '..tostring(totalMMPoints))

	Echo('microStoreResultHandler responseCode: '..responseCode..' errorCode: '..errorCode..' popupCode: '..popupCode)
	Set('_microStoreResponseCode',	responseCode)
	local responseCode	= AtoN(responseCode)
	local categoryID	= AtoN(categoryID)
	Set('_microStorePopupCode',		popupCode)
	local popupCode = AtoN(popupCode)

	if string.len(santas) > 0 then
		Set('_microStoreGiftsRemaining', santas)
	end

	Set('_microStoreGiftsEventExpiration', santa_event_expiration)

	local requestHostTimeMatch	= (GetCvarString('_lastRequestHostTime') == requestHostTime)

	UIManager.GetInterface('main'):HoNStoreF('MicroStoreResults', productEligibility, productIDs, productEnhancements, productEnhancementIDs)

	Set('microStoreIDList', '')

	if AtoN(errorCode) == 0 then

		if requestHostTimeMatch then

			if GetCvarBool('cg_store2_') and responseCode == MSTORE_RESPONSE_1 and categoryID == 3 then
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
					productNames = HoN_Store:BringIthItemFirst(productNames, customIconIndex)
					productWebContent = HoN_Store:BringIthItemFirst(productWebContent, customIconIndex)
					productDescription = HoN_Store:BringIthItemFirst(productDescription, customIconIndex)
					chargesRemaining = HoN_Store:BringIthItemFirst(chargesRemaining, customIconIndex)
					specialBundles = HoN_Store:BringIthItemFirst(specialBundles, customIconIndex)
					specialDisplay = HoN_Store:BringIthItemFirst(specialDisplay, customIconIndex)
					productPremium = HoN_Store:BringIthItemFirst(productPremium, customIconIndex)
					productCharges = HoN_Store:BringIthItemFirst(productCharges, customIconIndex)
					productDurations = HoN_Store:BringIthItemFirst(productDurations, customIconIndex)
					productQuantity = HoN_Store:BringIthItemFirst(productQuantity, customIconIndex)
				end
			end

			if timestamp ~= nil then
				Set('_microStore_lastTimestamp', timestamp)
				interface:UICmd("SetLocalServerTimeOffset("..timestamp..")")
			end

			Set('_microStoreFirstVisit', "false")
			Set('_microStore_TotalCoins', interface:UICmd("GoldCoins"))
			Set('_microStore_TotalSilverCoins', interface:UICmd("SilverCoins"))

			GetWidget("sysbar_gold_label"):SetText(FtoA(totalPoints, 0, 0, ','))
			GetWidget("sysbar_silver_label"):SetText(FtoA(totalMMPoints, 0, 0, ','))

			if GetCvarBool('ui_useRegionalCurrency') then
				if responseCode == MSTORE_CURRENCY_PACKAGES_UPDATE then
					Set('_microStore_regionalBalance', regional_currency)
				else
					Set('_microStore_regionalBalance', 0)
				end
			end

			if responseCode == MSTORE_RESPONSE_1 or responseCode == MSTORE_RESPONSE_2 or responseCode ~= MSTORE_RESPONSE_REFRESH_SELECTED_ITEMS then
				Set('_microStore_currentPage', currentPage)
			end

			Trigger('MicroStoreUpdateCoins')
			Set('_microStore_nextAction', responseCode)

			if responseCode == MSTORE_RESPONSE_1 or responseCode == MSTORE_RESPONSE_2 then
				for k,v in ipairs(interface:GetGroup('storeItemDataContainers')) do
					v:DoEventN(0)
				end

				Set('_microStore_Category', categoryID)
				useCategoryID = categoryID
				Set('_microStore_TotalPages', totalPages)
				Set('microStoreTimes', productTimes)

				explodeVar('|', productIDs, 'microStoreID', 'int')
				explodeVar('|', productNames, 'microStoreName')
				explodeVar('|', productWebContent, 'microStoreWebContent')
				explodeVar('|', productLocalContent, 'microStoreLocalContent')
				explodeVar('|', productDescription, 'microStoreDescription')
				explodeVar('|', productCodes, 'microStoreCode')
				explodeVar('|', chargesRemaining, 'microStoreChargeRemaining')
				explodeVar('|', specialBundles, 'microStoreSubBundles')
				explodeVar('|', specialDisplay, 'microStoreSpecialDisplay', 'int')

				if not GetCvarBool('cg_store2_') then
						interface:GetWidget('storeCoinDisplayChest'):Sleep(250, function()
						interface:GetWidget('storeCoinDisplayChest'):DoEventN(0)
					end)
				end
			else
				useCategoryID = GetCvarNumber('_microStore_Category')
			end

			if responseCode == MSTORE_RESPONSE_1 then
				explodeVar('|', purchasable, 'microStorePurchasable', 'bool')
				explodeVar('|', productAlreadyOwned, 'microStoreAlreadyOwned', 'bool')
				explodeVar('|', productIsBundle, 'microStoreBundle', 'bool')
				explodeVar('|', productPrices, 'microStorePrice', 'int')
				explodeVar('|', premium_mmp_cost, 'microStoreSilverPrice', 'int')
				explodeVar('|', productPremium, 'microStorePremium', 'bool')
				explodeVar('|', productCharges, 'microStoreCharges')
				explodeVar('|', productDurations, 'microStoreDurations')
				explodeVar('|', productQuantity, 'microStoreQuantity', 'int')
				Set('_microStore_iconsUnlocked', 1)		-- accountIconsUnlocked - Unlock ALL the icons
				Set('_microStore_accIconUnlockCost', unlockAccountIconsCost)
				Set('_microStore_accIconUnlockSilverCost', unlockAccountIconsCostMMP)
				Set('_microStore_vaultHighlight', vaultHighlight)
				Set('_microStore_custAccIconUnlocked', customAccountIcon)
				Set('_microStore_custAccIconCost', customAccountIconCost)
				Set('customAccountIconCostMMP', customAccountIconCostMMP)
			end

			Set('_microStore_tauntUnlocked', tauntUnlocked)
			Set('_microStore_tauntUnlockCost', tauntUnlockCost)
			Set('_microStore_tauntUnlockSilverCost', tauntUnlockCostMMP)

			if responseCode == MSTORE_RESPONSE_2 then
				Set('microStore_vaultData3', vaultCategory3)
				Set('microStore_vaultData4', prependCategoryData(vaultCategory4, '999999002`cs`/ui/legion/ability_coverup.tga`Chat Symbol'))
				Set('microStore_vaultData5', prependCategoryData(vaultCategory5, '999999003`av`/ui/legion/ability_coverup.tga`Alt Announcement'))
				Set('microStore_vaultData6', prependCategoryData(vaultCategory6, '999999005`misc`/ui/legion/ability_coverup.tga`Miscellaneous', (
					(
						AtoN(interface:UICmd("AccountStanding")) ~= 3 and
						(
							HoN_Region.regionTable[HoN_Region.activeRegion].hasTokens or HoN_Region.regionTable[HoN_Region.activeRegion].hasPasses
						)
					)
				)))
				Set('microStore_vaultData16', vaultCategory16)
				Set('microStore_vaultData27', vaultCategory27)
				Set('microStore_vaultData72', vaultCategory72)
				Set('microStore_vaultData57', prependCategoryData(vaultCategory57, '999999007`c`/ui/legion/ability_coverup.tga`Couriers'))
				Set('microStore_vaultData74', prependCategoryData(vaultCategory74, '999999006`w`/ui/legion/ability_coverup.tga`Custom Ward'))
				Set('microStore_vaultData75', vaultCategory75)
				Set('microStore_vaultData76', prependCategoryData(vaultCategory76, '999999008`cr`/ui/legion/ability_coverup.tga`Custom Creep'))
			end

			if responseCode == MSTORE_CURRENCY_PACKAGES_UPDATE then
				explodeVar('|', packagePrices, 'microStorePackagePrice', 'float')
				explodeVar('|', packagePoints, 'microStorePackageCoins', 'int')
				explodeVar('|', packageIDs, 'microStorePackageID', 'int')
				explodeVar('|', packageSpecial, 'microStorePackageSpecial', 'bool')
				explodeVar('|', packageTextures, 'microStorePackageTextures')
				Set('microStoreCurrencyToCoin', packageCurrencyToCoins)
			end

			Set('_microStore_lastSelectedUpgrades', selectedUpgrades)
			if responseCode == MSTORE_RESPONSE_REFRESH_SELECTED_ITEMS then
				if storeItemViews[useCategoryID].vault.viewItems == MSTORE_ITEM_VIEW_VAULT_BOOK then
					for k,v in ipairs(interface:GetGroup('vaultViewBookItems')) do
						v:DoEventN(4)
					end
				end

				if storeItemViews[useCategoryID].vault.viewItems == MSTORE_ITEM_VIEW_VAULT_BOOK then
					Trigger('refreshVaultViewBookCfg')
					GetWidget('MicroStoreUpgCheck'):DoEventN(0)
				elseif storeItemViews[useCategoryID].vault.viewItems == MSTORE_ITEM_VIEW_VAULT_SHELF then
					Trigger('refreshVaultViewShelfCfg')
					GetWidget('MicroStoreUpgCheck'):DoEventN(0)
				end
			end

			if responseCode == MSTORE_RESPONSE_VIEW_BUNDLE then
				Set('_microStore_bundleContents', bundleContents)
				Trigger('MicroStoreBundlePreview')
				interface:GetWidget('store_popup_bundle_preview'):DoEventN(0)
			end

			if popupCode == MSTORE_POPUP_BUY_COIN_SUCCESS or popupCode == MSTORE_POPUP_BUY_COIN_FRIEND then
				Trigger('EventAccountLevelup', 1, 'true')

				interface:GetWidget('store_popup_buy_coins'):DoEventN(1)
				interface:GetWidget('buy_coins_forfriend'):SetSelectedItemByValue(1)
				interface:GetWidget('buy_coins_ccfirstname'):EraseInputLine()
				interface:GetWidget('buy_coins_cclastname'):EraseInputLine()
				interface:GetWidget('buy_coins_cczip'):EraseInputLine()
				interface:GetWidget('buy_coins_ccnum'):EraseInputLine()
				interface:GetWidget('buy_coins_cccvv'):EraseInputLine()
				interface:GetWidget('buy_coins_ccmonth'):SetSelectedItemByValue(-1)
				interface:GetWidget('buy_coins_ccyear'):SetSelectedItemByValue(-1)
				Set('_microStore_buyCoins_package', -1)
				for k,v in ipairs(interface:GetGroup('_microStore_buyCoins_package_radio_group')) do
					v:SetButtonState(0)
				end
			end

			if popupCode == MSTORE_POPUP_BUY_COIN_SUCCESS then
				refreshAllUpgrades()
				interface:GetWidget('store_popup_buy_coin_success'):DoEventN(0)
			end

			if popupCode == MSTORE_POPUP_6 and not GetCvarBool('cg_store2_') then
				Trigger('MicroStoreSetProcTrig', 'StorePromoStatus')
				interface:UICmd("SubmitWebPanel('StorePromo', 'account_id', "..Client.GetAccountID()..", 'cookie', '"..Client.GetCookie().."', 'dev', '"..GetCvarString('_mstore_frontpage_dev').."')")
			end

			local storePromo = interface:GetWidget('store_promo')

			if storePromo and storePromo:IsVisible() and popupCode ~= MSTORE_POPUP_6 and (responseCode == MSTORE_RESPONSE_1 or responseCode == MSTORE_RESPONSE_2) then
				storePromo:DoEventN(1)
			end

			if popupCode == MSTORE_POPUP_BUY_COIN_FRIEND then
				interface:GetWidget('store_popup_buy_coin_friend_success'):DoEventN(0)
			end

			if popupCode == MSTORE_POPUP_3 then
				hideEventIfVisible('store_popup_confirm_purchase_gift')
				hideEventIfVisible('store_popup_confirm_purchase_choose')
				hideEventIfVisible('store_popup_unlock_icons')
				hideEventIfVisible('store_popup_unlock_taunt')

				if AtoB(grabBag) then
					widget:DoEventN(7, grabBagTheme, grabBagIDs, grabBagTypes, grabBagLocalPaths, grabBagProductNames)
					Trigger('ShopkeeperAction', 20)
				else
					interface:GetWidget('store_popup_buy_item_success'):DoEventN(0)
					refreshAllUpgrades()

					if useCategoryID == MSTORE_CATEGORY_7 then
						Trigger('ShopkeeperAction', 20)
					else
						Trigger('ShopkeeperAction', 19)
					end
				end
			end

			if popupCode == MSTORE_RESPONSE_4 or GetCvarBool('ui_showStoreWalkthrough') then
				Set('ui_showStoreWalkthrough', false)
				SetSave('ui_showStoreWalkthrough', false)
				Set('_microStoreFirstVisit', true)

				if GetCvarNumber('ui_isInWalkthrough') == 0 then
					interface:GetWidget('store_popup_firstvisit1'):DoEventN(0)
				end
			end

			if responseCode == MSTORE_RESPONSE_1 and (useCategoryID == MSTORE_CATEGORY_HERO_AVATARS or useCategoryID == MSTORE_CATEGORY_HEROES) then
				Set('msCodes', productCodes)
				Set('msGoldPrice', productPrices)
				Set('msSilverPrice', premium_mmp_cost)
				Set('msPremium', productPremium)
				Set('msOwned', productAlreadyOwned)
				Set('msBundle', productIsBundle)
				Set('msPurchasable', purchasable)
			else
				Set('msCodes', '')
				Set('msGoldPrice', '')
				Set('msSilverPrice', '')
				Set('msPremium', '')
				Set('msOwned', '')
				Set('msBundle', '')
				Set('msPurchasable', '')
			end

			if responseCode == MSTORE_RESPONSE_1 then
				if useCategoryID == MSTORE_CATEGORY_HEROES then
					Set('_microStoreHeroCategoryData', '')
					Set('microStoreIDList', productIDs)
				else
					Set('microStore_heroSelectTarget', '')
				end

				if useCategoryID == MSTORE_CATEGORY_HERO_AVATARS then
					widget:DoEventN(5, productIDs, productCodes, productPrices, premium_mmp_cost, productPremium, productAlreadyOwned, productIsBundle, purchasable)
				end

				if useCategoryID == MSTORE_CATEGORY_FEATURED_HEROES then
					widget:DoEventN(6, productNames, productIDs, productLocalContent, productAlreadyOwned, bundleNames, bundleCosts, bundleLocalPaths, bundleIncludedProducts, bundleAlreadyOwned, bundleIDs)
				end

				Trigger('MicroStoreUpdatePaging')

			end

			-- ALWAYS AT THE BOTTOM:
			if popupCode <= 0 then
				GetWidget('microStoreProcessResponse'):DoEventN(0)	-- THIS SHOULD ALWAYS BE THE LAST THING DONE
			end

		end	-- end request hosttime time match

		Set('_mstore_lastVerified', interface:UICmd("UIIsVerified"))

	end	-- end error 0

	if popupCode == MSTORE_POPUP_ERROR then
		interface:GetWidget('store_popup_error'):DoEventN(0)
		Trigger('MicroStoreErrorID', errorCode)
	end

end)