-- Special Messages
local interface, interfaceName = object, object:GetName()

local PANEL_CLOSE_ANIMATION_TIME = 200

FacebookStream = FacebookStream or {}

FacebookStream.facebookConnected = false
FacebookStream.bInStreaming = false

local _streamMessages = {}
local _currentStreamMessage = ''
local _fbStreamEnabled = false

interface:RegisterWatch('WebFacebookStreamUI', function(_, open, clean)
	local bShow = AtoB(open)
	if bShow then
		FacebookStream:OpenPanel()
	else
		if AtoB(clean) then
			UIManager.GetInterface('webpanel'):HoNWebPanelF('ResetFacebookStream')
		end
		FacebookStream:ClosePanel()
	end
end)

interface:RegisterWatch('FacebookLoginStatus', function(_, status)
	FacebookStream.facebookConnected = AtoB(status)

	GetWidget('facebook_stream_status_connected'):SetVisible(FacebookStream.facebookConnected)
	GetWidget('facebook_stream_status_disconnected'):SetVisible(not FacebookStream.facebookConnected)

	if FacebookStream.facebookConnected and GetFbShareType() == 1 then --auto-close browser if streaming
		FacebookStream:CloseBrowser()
	end
end)

interface:RegisterWatch('FacebookStreamStatus', function(_, status)
	FacebookStream.bInStreaming = AtoB(status)

	if not FacebookStream.bInStreaming then
		GetWidget('facebook_stream_views'):SetText('-')
		GetWidget('facebook_stream_likes'):SetText('-')
	end
end)

interface:RegisterWatch('FacebookStreamRelogin', function()
	FacebookStream:StartStream()
end)

interface:RegisterWatch('FacebookStreamToggleMenu', function()
	FacebookStream:TogglePanel()
end)

interface:RegisterWatch('FacebookBrowserOpen', function()
	FacebookStream:OpenBrowser()
end)

interface:RegisterWatch('FacebookBrowserClose', function()
	FacebookStream:CloseBrowser()
end)

interface:RegisterWatch('FacebookSilentStream', function()
	FacebookStream:StartStream()
end)

interface:RegisterWatch('FacebookStreamMessage', function(_, message, msgType)
	msgType = tonumber(msgType) or 0
	local msgObj = {msg=message, msgType=msgType}

	local bShowing = GetWidget('facebook_stream_message'):IsVisible()
	if bShowing or #_streamMessages > 0 then
		if _currentStreamMessage == message then
			return
		end
		for i=1, #_streamMessages do
			if _streamMessages[i].msg == message then
				return
			end
		end
		table.insert(_streamMessages, msgObj)
	else
		FacebookStream:ShowMessage(msgObj)
	end
end)

interface:RegisterWatch('WebFacebookStreamViews', function(_, id, total_views, live_views)
	Echo('WebFacebookStreamViews live_views:' .. live_views .. ' total_views:' .. total_views)
	GetWidget('facebook_stream_views'):SetText(total_views)
end)

interface:RegisterWatch('WebFacebookStreamLikes', function(_, id, total_count, has_liked)
	Echo('WebFacebookStreamLikes total_count:' .. total_count .. ' has_liked:' .. has_liked)
	GetWidget('facebook_stream_likes'):SetText(total_count)
end)

function FacebookStream:PlayerLoginStatus(status, description, loggedin, passwordExpiration, statusChanged)
	--Echo('FacebookStream:PlayerLoginStatus status: '..status..' description: '..description..' loggedin: '..loggedin..' passwordExpiration: '..passwordExpiration..' statusChanged: '..statusChanged)

	if AtoB(loggedin) then
		_fbStreamEnabled = GetCvarBool('cg_EnableFbStream')
	end

	if AtoB(statusChanged) and not AtoB(loggedin) then
		FacebookStream:ClosePanel()

		FacebookStream.facebookConnected = false
		FacebookStream.bInStreaming = false

		StopFbStream()
		LogoutFb()
	end
end

function FacebookStream:StartStream()
	if FacebookStream.bInStreaming then
		return
	end

	if GetCvarBool('vid_fullscreen') and GetCvarBool('d3d_exclusive') then
		Trigger('FacebookStreamMessage', 'facebook_stream_exclusive_mode_not_support')
		return
	end

	if GetCvarString('host_vidDriver') ~= 'vid_d3d9' then
		Trigger('FacebookStreamMessage', 'facebook_stream_opengl_mode_not_support')
		return
	end

	--sharetype:1 stream; 2 scoreboard; 3 graffiti
	SetFbShareType(1)

	Set('facebook_stream_title', GetWidget('facebook_stream_settings_title'):GetInputLine())
	Set('facebook_stream_description', GetWidget('facebook_stream_settings_description'):GetInputLine())

	Echo('FacebookStream:StartStream facebookConnected: ' .. tostring(FacebookStream.facebookConnected))
	if FacebookStream.facebookConnected then
		CreateFbStream()
	-- elseif IsFbSDKLoaded() then
	-- 	FacebookStream:OpenBrowser(false)
	-- 	LoginFb()
	else
		if not GetWidget('facebook_stream'):IsVisible() then
			FacebookStream:OpenPanel()
		end
		FacebookStream:OpenBrowser()
	end
end

function FacebookStream:TogglePanel()
	local outerPanel = GetWidget('facebook_stream')
	if outerPanel:IsVisible() then
		FacebookStream:ClosePanel()
	else
		FacebookStream:OpenPanel()
	end
end

function FacebookStream:OpenPanel()
	GetWidget('facebook_stream'):SetVisible(true)
end

function FacebookStream:ClosePanel()
	GetWidget('facebook_stream'):SetVisible(false)
end

function FacebookStream:OpenBrowser(loadURL)
	loadURL = loadURL or true
	GetWidget('facebook_stream_browser'):SetVisible(true)
	if loadURL then
		UIManager.GetInterface('webpanel'):HoNWebPanelF('ShowFacebookStream', GetWidget('facebook_streams_web_browser_insert'), HoN_Region:GetFacebookStream())
	end
end

function FacebookStream:CloseBrowser()
	GetWidget('facebook_stream_browser'):SetVisible(false)
end

function FacebookStream:ShowMessage(msgObj)
	_currentStreamMessage = msgObj.msg

	GetWidget('facebook_stream_message_title'):SetText(Translate(msgObj.msgType == 0 and 'facebook_stream' or 'facebook_share'))
	GetWidget('facebook_stream_message_label'):SetText(Translate(msgObj.msg))
	GetWidget('facebook_stream_message'):DoEventN(0)
end

function FacebookStream:HideMessage()
	GetWidget('facebook_stream_message'):DoEventN(1)

	if #_streamMessages > 0 then
		GetWidget('facebook_stream_message_helper'):Sleep(200, function() FacebookStream:ShowMessage(table.remove(_streamMessages, 1)) end)
	end
end

function FacebookStream:CheckStatus()
	if not GetCvarBool('cg_EnableFbStream') then
		if _fbStreamEnabled then
			Trigger('FacebookStreamMessage', 'facebook_stream_functionality_closed')
		end

		FacebookStream:ClosePanel()

		FacebookStream.facebookConnected = false
		FacebookStream.bInStreaming = false

		StopFbStream()
		LogoutFb()
	end

	_fbStreamEnabled = GetCvarBool('cg_EnableFbStream')
end