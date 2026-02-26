-----------------
-- News/MOTD Script
-- Copyright 2015 Frostburn Studios
-----------------

local EXTRA_BROWSER_SWAP_TIME = 225	-- extra sleep time to allow the browser to display the page after loading it
local MOTD_MAIN_WEB_ADDRESS = "http://naeu-icb2.s3.amazonaws.com/sbt/web/motd/index.html"	-- url built dynamically, this is deprecated

-----------------

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort			
local interface, interfaceName = object, object:GetName()
local triggerHelper = GetWidget("motd_trigger_helper")
HoN_News = {}
HoN_News.lastNewsHash = "0"
HoN_News.LeftPanelInfo = {}
HoN_News.LeftProductType = ""
HoN_News.LeftPanelButtons = {}
HoN_News.LeftPanelNumButtons = 0
HoN_News.LeftPanelButtonsOffset = 1
HoN_News.PanelAdTarget = {}
HoN_News.RightPanelInfo = {}
HoN_News.RightProductType = ""
HoN_News.MOTDUrl = MOTD_MAIN_WEB_ADDRESS.."?lang="..GetCvarString('host_language')	-- deprecated
HoN_News.PopupUrl = ""
HoN_News.WebPopupTargetX = 0
HoN_News.WebPopupTargetY = 0
HoN_News.BuyNowProduct = {}
HoN_News.loadingVisible = false
HoN_News.countdownOver = {}

RegisterScript2('News', '33')

local function getMOTDURL()

	local motdURL = HoN_Region.regionTable[HoN_Region.activeRegion].motdURL.retail

	if GetCvarBool('releaseStage_rc') then
		motdURL = HoN_Region.regionTable[HoN_Region.activeRegion].motdURL.rct
	elseif GetCvarBool('releaseStage_test') then
		motdURL = HoN_Region.regionTable[HoN_Region.activeRegion].motdURL.sbt
	end

	return motdURL.."?lang="..GetCvarString('host_language')
end

function HoN_News:ScrollLeftButtons(widget, dir)
	-- i know this is pretty bad ~~
	local buttonWidget = GetWidget("button_slider")
	local fadeOut, fadeIn, fadeOut2, fadeIn2 = nil, nil, nil, nil
	local setX = nil
	local offsetAdd = nil
	local slideMul = nil
	local slideTime = 100
	local buttonWidth = GetWidget("motd_left_button_0"):GetWidth()

	if (dir == "right") then
		-- if (HoN_News.LeftPanelNumButtons == 4 and HoN_News.LeftPanelButtonsOffset == 1) then -- we are sliding from edge to edge
		-- 	offsetAdd = -1
		-- 	fadeOut = "news_right_button_grade"
		-- 	fadeOut2 = "motd_left_scroller_right"
		-- 	fadeIn = "news_left_button_grade"
		-- 	fadeIn2 = "motd_left_scroller_left"
		-- 	setX = "-6.5h"
		-- 	slideAmount = "0.9h"
		-- 	slideMul = -1
		if (HoN_News.LeftPanelButtonsOffset == (5 - HoN_News.LeftPanelNumButtons)) then -- sliding to the right edge
			offsetAdd = -1
			fadeOut = "news_right_button_grade"
			fadeOut2 = "motd_left_scroller_right"
			setX = buttonWidth * -1.30
			slideMul = -0.52
		elseif(HoN_News.LeftPanelButtonsOffset == 1) then 	-- sliding away from the left edge
			offsetAdd = -1
			fadeIn = "news_left_button_grade"
			fadeIn2 = "motd_left_scroller_left"
			setX = buttonWidth * -1.81
			slideMul = -0.52
		elseif (HoN_News.LeftPanelButtonsOffset > (4 - HoN_News.LeftPanelNumButtons)) then -- normal slide
			offsetAdd = -1
			setX = buttonWidth * -1.81
			slideMul = -1
			slideTime = 135
		end
	elseif (dir == "left") then
		-- if (HoN_News.LeftPanelNumButtons == 4 and HoN_News.LeftPanelButtonsOffset == 0) then -- we are sliding from edge to edge
		-- 	offsetAdd = 1
		-- 	fadeOut = "news_left_button_grade"
		-- 	fadeOut2 = "motd_left_scroller_left"
		-- 	fadeIn = "news_right_button_grade"
		-- 	fadeIn2 = "motd_left_scroller_right"
		-- 	setX = "-11.0h"
		-- 	slideAmount = "0.9h"
		if (HoN_News.LeftPanelButtonsOffset == 0) then 	-- sliding to the left edge
			offsetAdd = 1
			slideMul = 0.43
			fadeOut = "news_left_button_grade"
			fadeOut2 = "motd_left_scroller_left"
			setX = buttonWidth * -2.45
		elseif (HoN_News.LeftPanelButtonsOffset == (4 - HoN_News.LeftPanelNumButtons)) then	-- sliding away from the right edge
			offsetAdd = 1
			slideMul = 0.52
			fadeIn = "news_right_button_grade"
			fadeIn2 = "motd_left_scroller_right"
			setX = buttonWidth * -1.81
		elseif (HoN_News.LeftPanelButtonsOffset < 1) then -- normal slide
			offsetAdd = 1
			slideMul = 1
			setX = buttonWidth * -1.81
			slideTime = 135
		end
	end

	if (setX and offsetAdd and slideMul) then
		buttonWidget:SlideX((buttonWidget:GetX()+(buttonWidth * slideMul)), slideTime)
		if (fadeIn) then GetWidget(fadeIn):FadeIn(100) end
		if (fadeIn2) then GetWidget(fadeIn2):FadeIn(100) end
		buttonWidget:Sleep(slideTime+5, function(...) 	HoN_News.LeftPanelButtonsOffset = HoN_News.LeftPanelButtonsOffset + offsetAdd
												if (fadeOut) then GetWidget(fadeOut):FadeOut(100) end
												if (fadeOut2) then GetWidget(fadeOut2):FadeOut(100) end
												buttonWidget:SetX(setX)
												for i=0, 6 do
													HoN_News:SetupLeftPanelButton(i, i-HoN_News.LeftPanelButtonsOffset)
												end
											end)
	end
end

function HoN_News:ClickLeftButton(widget, buttonNumber)
	if (not IsLoggedIn()) then return end
	-- 1 and 5 are disabled at the moment, remove the if to stop that
	if ((buttonNumber > 1  or HoN_News.LeftPanelButtonsOffset == (4 - HoN_News.LeftPanelNumButtons)) and (buttonNumber < 5 or HoN_News.LeftPanelButtonsOffset == 1)) then
		local buttonIndex = buttonNumber-HoN_News.LeftPanelButtonsOffset
		if (HoN_News.LeftPanelButtons[buttonIndex][3] and HoN_News.LeftPanelButtons[buttonIndex][3] ~= "") then	-- web url
			HoN_News:MOTDWebPopup(HoN_News.LeftPanelButtons[buttonIndex][3], 0)
			UIManager.GetInterface('main'):HoNGMainF('UserAction', 3, true, 'url')
		elseif (HoN_News.LeftPanelButtons[buttonIndex][4] and HoN_News.LeftPanelButtons[buttonIndex][4] ~= "")	then	-- store\herodex link
			if (HoN_News.LeftPanelButtons[buttonIndex][4] == "usage") then 			-- don't open any of these if they aren't logged in
				OpenLearnatorium(1, 5, HoN_News.LeftPanelInfo[2], false)
			elseif (HoN_News.LeftPanelButtons[buttonIndex][4] == "details") then
				OpenLearnatorium(1, 4, HoN_News.LeftPanelInfo[2], false)
			elseif (HoN_News.LeftPanelButtons[buttonIndex][4] == "guides") then
				OpenLearnatorium(1, 6, HoN_News.LeftPanelInfo[2], false)
			elseif (HoN_News.LeftPanelButtons[buttonIndex][4] == "referrals") then
				OpenOptions(7, 1, false)
			else 	-- store link
				Set('microStore_targetCategory', HoN_News.LeftPanelButtons[buttonIndex][4])
				local store = GetCvarBool('cg_store2_') and 'store_container2' or 'store_container'
				UIManager.GetInterface('main'):HoNGMainF('UserAction', 3, true, 'shop')
				UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', store, nil, false)
			end
		end
	end
end

function HoN_News:HoverLeftButton(widget, buttonNumber)
	GetWidget("motd_left_button_tip"):SetText(Translate(HoN_News.LeftPanelButtons[buttonNumber-HoN_News.LeftPanelButtonsOffset][2]))
end

function HoN_News:LoadMOTDUrl(url)
	if (GetWidget('motd_center_panel'):IsVisible()) then
		UIManager.GetInterface('webpanel'):HoNWebPanelF('LoadURLWithThrob', url, GetWidget("motd_center_panel"))
		return true
	else
		return false
	end
end

function HoN_News:RefreshMainWindow()
	if (GetWidget("motd_master"):IsVisible()) then
		UIManager.GetInterface('webpanel'):HoNWebPanelF('LoadURLWithThrob', getMOTDURL(), GetWidget("motd_center_panel"))
	end
end

function HoN_News:InitalizeWebPopup(scaleTime, slideTime, fadeOutTime)
	-- determine the window size we want (sizes taken from the web_panel.lua and tweaked)
	local sx, sy = GetScreenWidth(), GetScreenHeight()
	if ((sx <= 1024) or (sy <= 800)) then
		HoN_News.WebPopupTargetX = 770
		HoN_News.WebPopupTargetY = 448
	elseif ((sx >= 1602) and (sy >= 1230)) then
		HoN_News.WebPopupTargetX = 1602
		HoN_News.WebPopupTargetY = 1230
	else
		HoN_News.WebPopupTargetX = 1026
		HoN_News.WebPopupTargetY = 798
	end

	local webPopup = GetWidget("motd_web_popup")

	local onevent0 = function(...)
		webPopup:SetY("2.6h")
		webPopup:Scale(HoN_News.WebPopupTargetX, HoN_News.WebPopupTargetY, scaleTime)
		--webPopup:SlideY("2.6h", scaleTime+slideTime)
		webPopup:Sleep(scaleTime+slideTime, function(...) HoN_News:MOTDWebPopupOpenURL(webPopup) end)
	end
	webPopup:SetCallback("onevent0", onevent0)

	local CloseHelper = function(widget, status)
		if (tonumber(status) == 1) then
			webPopup:UnregisterWatch("WebBrowserStatus")

			webPopup:Sleep(EXTRA_BROWSER_SWAP_TIME, function(...)
				--webPopup:FadeOut(fadeOutTime)
				webPopup:Scale(5, 5, scaleTime)
				webPopup:SlideY("25%", scaleTime+slideTime)
				--webPopup:Sleep(fadeOutTime, function(...) webPopup:SetVisible(0) end)
				webPopup:Sleep(scaleTime+slideTime, function(...) webPopup:SetVisible(0) end)
			end)
		end
	end

	local onevent1 = function(...)
		if (HoN_News:LoadMOTDUrl(getMOTDURL())) then	-- move the web browser back, fade the popup
			webPopup:RegisterWatch("WebBrowserStatus", CloseHelper)
		else
			CloseHelper(nil, 1)
		end
	end
	webPopup:SetCallback("onevent1", onevent1)

	webPopup:RefreshCallbacks()

	-- frameless window
	local webPopupFL = GetWidget("motd_frameless_web_popup")
	local centerPanel = GetWidget("motd_center_panel")

	local onevent0 = function(...)
		webPopupFL:SetY("4.0h")
		webPopupFL:Scale(centerPanel:GetWidth()+1, centerPanel:GetHeight(), scaleTime)	-- +1 to fix flicker from loading. I have no idea why the scale is coming up
		--webPopupFL:SlideY("2.6h", scaleTime+slideTime)								-- 1 pixel short, it worked before it was resized
		webPopupFL:Sleep(scaleTime, function(...) HoN_News:MOTDWebPopupOpenURL(webPopupFL) end) --scaleTime+slideTime
	end
	webPopupFL:SetCallback("onevent0", onevent0)

	local CloseHelper = function(widget, status)
		if (tonumber(status) == 1) then
			webPopupFL:UnregisterWatch("WebBrowserStatus")

			webPopupFL:Sleep(EXTRA_BROWSER_SWAP_TIME, function(...)
				--webPopupFL:FadeOut(fadeOutTime)
				webPopupFL:Scale(5, 5, scaleTime)
				webPopupFL:SlideY("25%", scaleTime+slideTime)
				--webPopupFL:Sleep(fadeOutTime, function(...) webPopupFL:SetVisible(0) end)
				webPopupFL:Sleep(scaleTime+slideTime, function(...) webPopupFL:SetVisible(0) end)
			end)
		end
	end

	local onevent1 = function(...)
		if (HoN_News:LoadMOTDUrl(getMOTDURL())) then	-- move the web browser back, fade the popup
			webPopupFL:RegisterWatch("WebBrowserStatus", CloseHelper)
		else
			CloseHelper(nil, 1)
		end
	end
	webPopupFL:SetCallback("onevent1", onevent1)

	webPopupFL:RefreshCallbacks()
end

function HoN_News:MOTDWebPopup(url, mode, overrideWindowName)
	--0 = small center windows
	--1 = small center window with browser buttons
	--2 = window without full browser buttons
	--3 = window with full browser buttons
	HoN_News.PopupUrl = url

	local needToSleep = false

	if (mode == 3) then
		GetWidget("motd_fullscreen_buttonset"):SetVisible(1)
		GetWidget("motd_web_popup_title"):SetText(Translate(overrideWindowName or "news_title"))
		GetWidget("motd_min_frameless"):SetVisible(0)
		GetWidget("motd_min"):SetVisible(1)
	elseif (mode == 2) then
		GetWidget("motd_fullscreen_buttonset"):SetVisible(0)
		GetWidget("motd_web_popup_title"):SetText(Translate("news_title"))
		GetWidget("motd_min_frameless"):SetVisible(0)
		GetWidget("motd_min"):SetVisible(1)
	elseif (mode == 1) then
		GetWidget("motdf_buttonset"):SetVisible(1)
		GetWidget("motd_fullscreen_buttonset"):SetVisible(1)
		GetWidget("motd_min_frameless"):SetVisible(1)
		GetWidget("motd_min"):SetVisible(0)
	else
		mode = 0
		GetWidget("motdf_buttonset"):SetVisible(0)
		GetWidget("motd_fullscreen_buttonset"):SetVisible(0)
		GetWidget("motd_min_frameless"):SetVisible(1)
		GetWidget("motd_min"):SetVisible(0)
	end

	local windowedPopup = GetWidget("motd_web_popup")
	local framelessPopup = GetWidget("motd_frameless_web_popup")
	local webPopup

	if (mode <= 1) then -- check if windowed visible, if it is close it
		if windowedPopup:IsVisible() then
			windowedPopup:DoEventN(1)
			needToSleep = true
		end
		webPopup = framelessPopup
	else
		if framelessPopup:IsVisible() then
			framelessPopup:DoEventN(1)
			needToSleep = true
		end
		webPopup = windowedPopup
	end

	local openFunctionHelperThing = function()

		local centerPanel = GetWidget("motd_center_panel")

		if (webPopup:IsVisible() and
			((mode ~= 0 and mode ~= 1 and ((webPopup:GetWidth() == HoN_News.WebPopupTargetX) and (webPopup:GetHeight() == HoN_News.WebPopupTargetY))) or
			((webPopup:GetWidth() == centerPanel:GetWidth()+1) and (webPopup:GetHeight() == centerPanel:GetHeight())))) then
			-- the browser is open and scaled to the right size
			-- if it's not the right size, it will open newly requested URL (the one that didn't trigger the change) when it's done
			GetWidget('web_browser'):UICmd("Call('web_browser', 'WebBrowserLoadURL(\\\'"..HoN_News.PopupUrl.."\\\')')")
		elseif (not webPopup:IsVisible()) then	-- the window is not visible, scale, slide, etc, and make it so
			webPopup:SetVisible(1)
			webPopup:DoEventN(0)
		end
	end

	if (not needToSleep) then
		openFunctionHelperThing()
	else
		GetWidget("motd_center_panel"):Sleep(EXTRA_BROWSER_SWAP_TIME+350, openFunctionHelperThing)
	end
end

function HoN_News:MOTDWebPopupOpenURL(webWidget)
	if (GetWidget("motd_center_panel"):IsVisible()) then
		GetWidget('web_browser'):UICmd("Call('web_browser', 'WebBrowserLoadURL(\\\'"..HoN_News.PopupUrl.."\\\')')")

		local OpenHelper = function(widget, status)
			if (tonumber(status) == 1) then
				webWidget:UnregisterWatch("WebBrowserStatus")
				webWidget:Sleep(EXTRA_BROWSER_SWAP_TIME, function (...) webWidget:DoEventN(2) end)
			end
		end

		webWidget:RegisterWatch("WebBrowserStatus", OpenHelper)
	else
		webWidget:DoEventN(2)
		GetWidget('web_browser'):UICmd("Call('web_browser', 'WebBrowserLoadURL(\\\'"..HoN_News.PopupUrl.."\\\')')")
	end
end

triggerHelper:RegisterWatch("RequestMessageOfTheDay", function()
	if UIGamePhase() == 0 then
		HoN_News:InitalizeWebPopup(250, 100, 250)

		Set('_lastNewsRefresh', GetTime())

		HoN_News:LoadMOTDUrl(getMOTDURL())

		GetWidget("motd_master"):SetVisible(1)
		GetWidget("motd_background"):SetVisible(1)

		if (not GetCvarBool('news_auto_displayed')) or GetCvarBool('_motd_dev') then
			Cvar.CreateCvar('news_auto_displayed', 'bool', 'true')
			HoN_News:ReopenNews(true)
		end
	end
end)

function HoN_News:ReopenNews(dontSleep)
	if (UIGamePhase() == 0 and not HoN_News.loadingVisible and IsLoggedIn()) then
		--if (not GetWidget("news"):IsVisible()) then
			if (dontSleep) then
				UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'news', nil, nil, nil, true, 2)
			else
				GetWidget("news"):DoEventN(4)
			end
		--end
	elseif (GetWidget("news"):IsVisible()) then
		UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'news', nil, nil, true, nil, 6)
	end
	-- the event sleeps the news panel a bit before opening it so it doesn't open a bit early
end

-- triggers from web
function HoN_News:WebBuyProduct(widget, ...)
	-- name (hero.alt), display Name, id, purchaseable, cost, premium, premiumcost
	if (AtoB(arg[4])) then
		local premiumCost = arg[7]
		if (not AtoB(arg[6])) then
			premiumCost = 9002
		end
		Trigger('Confirm_Purchase_Other', "mstore_product"..arg[3].."_name", "mstore_buyitem_confirm", arg[5], arg[7], "DERPY", arg[3])
	end
end
triggerHelper:RegisterWatch("webbuyproduct", function(...) HoN_News:WebBuyProduct(...) end)

function HoN_News:WebPlayVideo(widget, address, windowType)
	HoN_News:MOTDWebPopup(address, tonumber(windowType) or 1)
end
triggerHelper:RegisterWatch("webplayvideo", function(...) HoN_News:WebPlayVideo(...) end)

function HoN_News:loadingVisibility(_, loading)
	HoN_News.loadingVisible = AtoB(loading)

	if (HoN_News.loadingVisible) then
		if (GetWidget("news"):IsVisible()) then
			UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'news', nil, nil, true, nil, 6)
		end
	end
end
triggerHelper:RegisterWatch('LoadingVisible', function(...) HoN_News:loadingVisibility(...) end)

function interface:HoNNewsF(func, ...)
	print(HoN_News[func](self, ...))
end	