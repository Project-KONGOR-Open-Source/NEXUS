----------------------------------------------------------
--	Name: 		Web Panel Script	            		--
--  Copyright 2015 Frostburn Studios					--
----------------------------------------------------------

local COVER_TO_BROWSER_SLEEP = 250

------------------------------

local _G = getfenv(0)
local HoN_Web_Panel = _G['HoN_Web_Panel'] or {}
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort
local interface, interfaceName = object, object:GetName()
RegisterScript2('Web Panel', '31')
HoN_Web_Panel.currentPage = 0
HoN_Web_Panel.isPopup = false
HoN_Web_Panel.pageHistory = {}
HoN_Web_Panel.defaultPage  = '!hon/streams/'
HoN_Web_Panel.tutorialPage = '!hon/whitelistfolder/learnatorium/laning_video.php'
HoN_Web_Panel.createAGuide = '!hon/guides/select.php?cookie='..(interface:UICmd("GetCookie()"))

local function normalMini()
	local web_browser = interface:GetWidget('web_browser')
	local web_browser_panel = interface:GetWidget('web_browser_panel')
	local web_browser_min_insert = interface:GetWidget('web_browser_min_insert')
	local web_browser_min_btn = interface:GetWidget('web_browser_min_btn')
	local web_browser_max_btn = interface:GetWidget('web_browser_max_btn')
	-- local web_browser_home_btn = interface:GetWidget('web_browser_home_btn')
	local web_browser_ref_btn = interface:GetWidget('web_browser_ref_btn')
	local web_browser_back_btn = interface:GetWidget('web_browser_back_btn')
	local web_browser_for_btn = interface:GetWidget('web_browser_for_btn')
	if (HoN_Web_Panel.isPopup) or (tonumber(interface:UICmd([[GetScreenWidth()]])) <= 1024) or (tonumber(interface:UICmd([[GetScreenHeight()]])) <= 768) then
		web_browser_panel:SetWidth('770')
		web_browser_panel:SetHeight('478')
		web_browser_min_insert:SetWidth('770')
		web_browser_min_insert:SetHeight('478')
		web_browser:SetWidth('768')
		web_browser:SetHeight('448')
	elseif (tonumber(interface:UICmd([[GetScreenWidth()]])) >= 1602) and (tonumber(interface:UICmd([[GetScreenHeight()]])) >= 1230) then
		web_browser_panel:SetWidth('1602')
		web_browser_panel:SetHeight('1230')
		web_browser_min_insert:SetWidth('1602')
		web_browser_min_insert:SetHeight('1230')
		web_browser:SetWidth('1600')
		web_browser:SetHeight('1200')
	else
		web_browser_panel:SetWidth('1026')
		web_browser_panel:SetHeight('798')
		web_browser_min_insert:SetWidth('1026')
		web_browser_min_insert:SetHeight('798')
		web_browser:SetWidth('1024')
		web_browser:SetHeight('768')
	end
	web_browser_panel:SetX('0')
	web_browser_panel:SetY('3h')
	web_browser:SetParent(web_browser_min_insert)
	-- web_browser_home_btn:SetVisible(true)
	web_browser_ref_btn:SetVisible(true)
	web_browser_back_btn:SetVisible(true)
	web_browser_for_btn:SetVisible(true)
	web_browser_min_btn:SetVisible(false)
	web_browser_max_btn:SetVisible(true)
	-- GetWidget('web_browser_viewer_title'):SetText(Translate('stream_viewer'))
end

local function compactMini(useCurrentParent)
	local web_browser = interface:GetWidget('web_browser')
	local web_browser_panel = interface:GetWidget('web_browser_panel')
	local web_browser_min_insert = interface:GetWidget('web_browser_min_insert')
	local web_browser_min_btn = interface:GetWidget('web_browser_min_btn')
	local web_browser_max_btn = interface:GetWidget('web_browser_max_btn')
	-- local web_browser_home_btn = interface:GetWidget('web_browser_home_btn')
	local web_browser_ref_btn = interface:GetWidget('web_browser_ref_btn')
	local web_browser_back_btn = interface:GetWidget('web_browser_back_btn')
	local web_browser_for_btn = interface:GetWidget('web_browser_for_btn')
	if (useCurrentParent) then
		web_browser:SetWidth('100%')
		web_browser:SetHeight('100%')
		--web_browser:SetWidth('768')
		--web_browser:SetHeight('448')
	else
		web_browser_panel:SetWidth('770')
		web_browser_panel:SetHeight('478')
		web_browser_min_insert:SetWidth('770')
		web_browser_min_insert:SetHeight('478')
		web_browser_panel:SetX('0')
		web_browser_panel:SetY('3h')
		web_browser:SetParent(web_browser_min_insert)
		web_browser:SetWidth('768')
		web_browser:SetHeight('448')
		-- web_browser_home_btn:SetVisible(false)
		web_browser_ref_btn:SetVisible(false)
		web_browser_back_btn:SetVisible(false)
		web_browser_for_btn:SetVisible(false)
		web_browser_min_btn:SetVisible(false)
		web_browser_max_btn:SetVisible(true)
		-- GetWidget('web_browser_viewer_title'):SetText('')
	end
end

local function normalMaxi()
	local web_browser = interface:GetWidget('web_browser')
	local web_browser_panel = interface:GetWidget('web_browser_panel')
	local web_browser_min_btn = interface:GetWidget('web_browser_min_btn')
	local web_browser_max_btn = interface:GetWidget('web_browser_max_btn')
	local web_browser_ref_btn = interface:GetWidget('web_browser_ref_btn')
	local web_browser_back_btn = interface:GetWidget('web_browser_back_btn')
	local web_browser_for_btn = interface:GetWidget('web_browser_for_btn')
	web_browser_panel:SetWidth('100%')
	web_browser_panel:SetHeight('100%')
	web_browser:SetWidth('100%')
	web_browser:SetHeight('100%')
	web_browser_panel:SetX('0')
	web_browser_panel:SetY('0')
	web_browser:SetParent(web_browser_panel)
	web_browser:BringToFront()
	web_browser_ref_btn:SetVisible(true)
	web_browser_back_btn:SetVisible(true)
	web_browser_for_btn:SetVisible(true)
	web_browser_max_btn:SetVisible(false)
	web_browser_min_btn:SetVisible(true)
	web_browser_min_btn:BringToFront()
end

local function LoadURL(url, isPopup, useCurrentParent)
	HoN_Web_Panel.isPopup = isPopup or false
	print('^r LoadURL: ' .. tostring(url) .. '\n' )
	interface:GetWidget('web_browser'):WebBrowserLoadURL(url)
	if (HoN_Web_Panel.isPopup) or (tonumber(interface:UICmd([[GetScreenWidth()]])) <= 1024) or (tonumber(interface:UICmd([[GetScreenHeight()]])) <= 768) then
		compactMini(useCurrentParent)
	end
end

local function FormStatusController(trigger, ...)
	--print('^r FormStatusController!: ' .. trigger .. ' | ' .. arg[2] .. ' \n' )
end

function HoN_Web_Panel:MoveWebPanel(newParent)
	local web_browser = interface:GetWidget('web_browser')
	web_browser:SetParent(newParent)
	web_browser:SetWidth(newParent:GetWidth())
	web_browser:SetHeight(newParent:GetHeight())
end

function HoN_Web_Panel:LoadURLWithThrob(url, parent, localURL, isTour)
	-- Echo('LoadURLWithThrob localURL: ' .. tostring(localURL))
	-- Echo('LoadURLWithThrob isTour: ' .. tostring(isTour))
	if (localURL == nil) then
		localURL = false
	end
	if (isTour == nil) then
		isTour = false
	end

	local cover = isTour and GetWidget("web_browser_cover_tour") or GetWidget("web_browser_cover")
	local webBrowser = localURL and GetWidget("web_browser_local") or GetWidget("web_browser")
	if (webBrowser == nil or cover == nil) then return end

	webBrowser:SetNoMouseRightClick(isTour)

	if (parent) then
		if (parent ~= webBrowser:GetParent()) then
			parent:SetVisible(true)
			webBrowser:SetParent(parent)
			webBrowser:SetWidth('100%')
			webBrowser:SetHeight('100%')
		end
	else
		parent = webBrowser:GetParent()
		if (not parent) then return end -- no parent, don't bother
	end

	local statusHelper = function(_, statusCode)
		statusCode = tonumber(statusCode)
		-- 1 = done, 0 = thinking, 2 = error
		if (statusCode == 1) then
			cover:UnregisterWatch("WebBrowserStatus")
			cover:Sleep(COVER_TO_BROWSER_SLEEP, function()
				if (url == HoN_Region:GetPlayerTourMatch()) then
					cover:SetVisible(0)
				elseif (url == HoN_Region:GetPlayerTour()) then
					-- cover:FadeOut(2000)
				elseif (url == HoN_Region:GetPlayerTourTreasure()) then
					-- cover:FadeOut(1000)
				elseif (url == HoN_Region:GetPlayerTourLevel()) then
				else
					cover:FadeOut(500)
				end
			end )
		end
	end

	if not localURL then
		cover:RegisterWatch("WebBrowserStatus", statusHelper)
		if (not isTour) then
			cover:SetParent(parent)
		end
		cover:SetVisible(1)
	end

	if (isTour) then
		cover:SetParent(UIManager.GetInterface('main'):GetWidget('web_browser_player_tour_throb_parent'))
	end

	webBrowser:WebBrowserLoadURL(url)
end

function HoN_Web_Panel:HideTourCover(page)
	-- Echo('HideTourCover page: ' .. page)

	local cover = GetWidget("web_browser_cover_tour")
	if (cover == nil) then return end

	if (page == 'index' or page == 'level' or page == 'treasure' or page == 'ticket') then
		cover:FadeOut(500)
	end
end

function HoN_Web_Panel:LoadURLWithThrobOpaque(url, parent)
	local cover = GetWidget("web_browser_cover")
	local webBrowser = GetWidget("web_browser_opaque")
	if (webBrowser == nil or cover == nil) then return end

	if (parent) then
		if (parent ~= webBrowser:GetParent()) then
			parent:SetVisible(true)
			webBrowser:SetParent(parent)
			webBrowser:SetWidth('100%')
			webBrowser:SetHeight('100%')
		end
	else
		parent = webBrowser:GetParent()
		if (not parent) then return end -- no parent, don't bother
	end

	local statusHelper = function(_, statusCode)
		statusCode = tonumber(statusCode)
		-- 1 = done, 0 = thinking, 2 = error
		if (statusCode == 1) then
			cover:UnregisterWatch("WebBrowserStatus")
			cover:Sleep(COVER_TO_BROWSER_SLEEP, function()
				cover:SetVisible(0)
			end )
		end
	end

	if not localURL then
		cover:RegisterWatch("WebBrowserStatus", statusHelper)
		cover:SetParent(parent)
		cover:SetVisible(1)
	end

	webBrowser:WebBrowserLoadURL(url)
end

function HoN_Web_Panel:RestoreWebPanel()
	normalMini()
end

function HoN_Web_Panel:ShowNewPlayerTutorial(widget)
	HoN_Web_Panel:LoadURLWithThrob(HoN_Web_Panel.tutorialPage .. '?lang=' .. GetCvarString('host_language'), widget)
end

function HoN_Web_Panel:ShowFacebookStream(widget, url)
	local webBrowser = GetWidget('web_browser_facebook_stream')
	local cover = GetWidget("web_browser_cover")
	if (webBrowser == nil) then return end

	webBrowser:SetParent(widget)
	webBrowser:SetWidth(widget:GetWidth())
	webBrowser:SetHeight(widget:GetHeight())

	local statusHelper = function(_, statusCode)
		statusCode = tonumber(statusCode)
		if (statusCode == 1) then
			cover:UnregisterWatch("WebBrowserStatus")
			cover:Sleep(COVER_TO_BROWSER_SLEEP, function()
				cover:SetVisible(false)
			end )
		end
	end

	cover:RegisterWatch("WebBrowserStatus", statusHelper)

	cover:SetParent(widget)
	cover:SetVisible(true)

	url = url..'?cookie='..Client.GetCookie()

	webBrowser:WebBrowserLoadURL(url)
end

function HoN_Web_Panel:ResetFacebookStream()
	local webBrowser = GetWidget('web_browser_facebook_stream')
	if webBrowser then
		webBrowser:WebBrowserLoadURL('')
	end
end

function HoN_Web_Panel:ShowMOTD(widget, url)
	HoN_Web_Panel:LoadURLWithThrob(url, widget)
end

function HoN_Web_Panel:ShowPlayerTourStats(widget, url)
	HoN_Web_Panel:LoadURLWithThrob(url, widget)
end

function HoN_Web_Panel:ShowLadder(widget)
	local url = ''
	if GetCvarBool('cl_GarenaEnable') then
		url = '!ascension/index.php?r=site/rankladder&ranktype=normal&hongameclientcookie='..Client.GetCookie()..'|sea|'..GetCvarString('host_language')
	else
		url = '!ascension/index.php?r=site/rankladder&ranktype=normal&hongameclientcookie='..Client.GetCookie()..'|naeu|'..GetCvarString('host_language')
	end
	HoN_Web_Panel:LoadURLWithThrob(GetCvarString('ui_url_playerLadder', true) or url, widget)
end

function HoN_Web_Panel:ShowLadder2(widget)
	local url = ''
	if GetCvarBool('cl_GarenaEnable') then
		url = '!ascension/index.php?r=site/rankladder&ranktype=casual&hongameclientcookie='..Client.GetCookie()..'|sea|'..GetCvarString('host_language')
	end
	HoN_Web_Panel:LoadURLWithThrob(GetCvarString('ui_url_playerLadder2', true) or url, widget)
end

function HoN_Web_Panel:ShowRefereeWebPage(widget)
	widget:UICmd("WebBrowserSetMatchIDAsCookie()")
	HoN_Web_Panel:LoadURLWithThrobOpaque(HoN_Region:GetRefereeControlPage(), widget)
end

function HoN_Web_Panel:ShowQuestLadder(widget)
	HoN_Web_Panel:LoadURLWithThrob(GetCvarString('ui_url_playerQuestLadder', true) or HoN_Region:GetQuestLadder(), widget)
end

function HoN_Web_Panel:ShowCodexLevelLadder(widget)
	HoN_Web_Panel:LoadURLWithThrob(GetCvarString('ui_url_playerCodexLevelLadder', true) or HoN_Region:GetCodexLevelLadder(), widget)
end

-- function HoN_Web_Panel:ShowStrikerLadder(widget)
-- 	HoN_Web_Panel:LoadURLWithThrob(GetCvarString('ui_url_playerStrikerLadder', true) or HoN_Region:GetStrikerLadder(), widget)
-- end

function HoN_Web_Panel:ShowMasteryAllLadder(widget)
	HoN_Web_Panel:LoadURLWithThrob(GetCvarString('ui_url_playerMasteryAllLadder', true) or HoN_Region:GetMasteryAllLadder(), widget)
end

function HoN_Web_Panel:ShowMasteryFriendsLadder(widget)
	HoN_Web_Panel:LoadURLWithThrob(GetCvarString('ui_url_playerMasteryFriendsLadder', true) or HoN_Region:GetMasteryFriendsLadder(), widget)
end

function HoN_Web_Panel:ShowGCAWebPage()
	local webBrowser = GetWidget('web_browser_gca')
	local cover = GetWidget("web_browser_cover")
	if (webBrowser == nil) then return end

	local grandpa = UIManager.GetInterface('main'):GetWidget('popup_gca_wnd')
	local parent = UIManager.GetInterface('main'):GetWidget('popup_gca_webcontrol')

	grandpa:SetVisible(true)

	webBrowser:SetParent(parent)
	webBrowser:SetWidth(parent:GetWidth())
	webBrowser:SetHeight(parent:GetHeight())

	if not isNewUI() then
		local width = 11.56 * math.floor((GetScreenHeight() / 1080 * 100))
		UIManager.GetInterface('main'):GetWidget('gca_header_bar'):SetWidth(width)
	end

	url = HoN_Region:GetGCAControlPage()

	local statusHelper = function(_, statusCode)
		statusCode = tonumber(statusCode)
		-- 1 = done, 0 = thinking, 2 = error
		if (statusCode == 1) then
			cover:UnregisterWatch("WebBrowserStatus")
			cover:Sleep(COVER_TO_BROWSER_SLEEP, function()
				cover:SetVisible(0)
			end )
		end
	end

	cover:RegisterWatch("WebBrowserStatus", statusHelper)

	cover:SetParent(parent)
	cover:SetVisible(1)

	webBrowser:UICmd("WebBrowserSetCafeInfoAsCookie()")
	webBrowser:WebBrowserLoadURL(url)
end

function HoN_Web_Panel:ShowSpecialMessagesPage(widget, url)
	local webBrowser = GetWidget('web_browser_special_message')
	local cover = GetWidget("web_browser_cover")
	if (webBrowser == nil) then return end

	webBrowser:SetParent(widget)
	webBrowser:SetWidth(widget:GetWidth())
	webBrowser:SetHeight(widget:GetHeight())

	local statusHelper = function(_, statusCode)
		statusCode = tonumber(statusCode)
		if (statusCode == 1) then
			cover:UnregisterWatch("WebBrowserStatus")
			cover:Sleep(COVER_TO_BROWSER_SLEEP, function()
				cover:SetVisible(0)
			end )
		end
	end

	cover:RegisterWatch("WebBrowserStatus", statusHelper)

	cover:SetParent(widget)
	cover:SetVisible(1)

	url = url..'?cookie='..Client.GetCookie()

	webBrowser:WebBrowserLoadURL(url)
end

function HoN_Web_Panel:CloseSpecialMessagesPage(widget)
	local webBrowser = GetWidget('web_browser_special_message')
	local cover = GetWidget("web_browser_cover")
	if cover == nil or webBrowser == nil then return end
	cover:SetParent(widget)
	cover:SetVisible(1)
	webBrowser:WebBrowserLoadURL('')
end

function HoN_Web_Panel:InputButton(input)
	local web_browser = interface:GetWidget('web_browser')
	interface:GetWidget('web_browser_stream_title'):SetText('')
	if (input == 1) then
		web_browser:UICmd("WebBrowserBack()")
	elseif (input == 2) then
		web_browser:UICmd("WebBrowserForward()")
	elseif (input == 3) then
		web_browser:UICmd("WebBrowserReload()")
	elseif (input == 4) then
		HoN_Web_Panel.isPopup = false
		normalMini()
		LoadURL(HoN_Web_Panel.defaultPage)
	elseif (input == 5) and (not HoN_Web_Panel.isPopup) then
		normalMini()
	elseif (input == 5) and (HoN_Web_Panel.isPopup) then
		compactMini()
	elseif (input == 6) then
		normalMaxi()
	end
end

function HoN_Web_Panel:InputURL(input, isPopup)

	local _, _, sizeInt = string.find(input, 'size=(.)')

	if (sizeInt) then
		if (sizeInt == '1')  then
			isPopup = true
		elseif (sizeInt == '2')  then
			isPopup = false
		else
			isPopup = isPopup or false
		end
		printdb('^y InputURL: ' .. tostring(input) .. '\n' .. '^y Size override = ' .. sizeInt .. '\n' .. ' isPopup = ' .. tostring(isPopup) )
	end

	HoN_Web_Panel.currentPage = HoN_Web_Panel.currentPage + 1
	HoN_Web_Panel.pageHistory[HoN_Web_Panel.currentPage] = input
	HoN_Web_Panel.isPopup = false
	if (not isPopup) then
		normalMini()
	end
	LoadURL(input, isPopup)
end

function HoN_Web_Panel:WebPanelClose()
	-- if  (HoN_Web_Panel.isPopup) and (GetCvarBool('ui_promoMOTD')) then
	-- 	 UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'news', nil, nil, nil, true, 1)
	-- end
	UIManager.GetInterface('main'):HoNNewsF('ReopenNews')
end

function HoN_Web_Panel:CreateAGuide()
	UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'compendium', nil, nil, nil, nil, 81)
	RequestWebURL('!hon/guides/select.php?cookie='..(interface:UICmd("GetCookie()")), nil, false)
end

function HoN_Web_Panel:UnloadPage(sleepAmount)
	sleepAmount = tonumber(sleepAmount)
	local hideFunc = function()
		if (not GetWidget("web_browser"):IsVisible()) then
			GetWidget('web_browser'):WebBrowserLoadURL('')
		end
	end

	if (not sleepAmount) then
		sleepAmount = 1
	end

	GetWidget('web_panel_sleeper'):Sleep(sleepAmount, hideFunc)
end

function interface:HoNWebPanelF(func, ...)
  print(HoN_Web_Panel[func](self, ...))
end