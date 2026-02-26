-----------------
-- Replay Controller Script
-- Copyright 2015 Frostburn Studios
-----------------

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort		
local interface, interfaceName = object, object:GetName()

HoN_ReplayController = {}

HoN_ReplayController.indexedPlayers = {}
HoN_ReplayController.selectedPlayer = ""

HoN_ReplayController.indexedVoices = {}
HoN_ReplayController.selectedVoice = ""

HoN_ReplayController.FirstFrame = 0;

HoN_ReplayController.StreamingUIShowTime = 0

function HoN_ReplayController:GetWidget(widgetName)
	if (GetCvarBool(replay_v2)) then
		widgetName = widgetName..'2'
	end
		
	return GetWidget(widgetName, 'game_replay_control')
end

function HoN_ReplayController:AddPlayer(playerName)
	table.insert(HoN_ReplayController.indexedPlayers, playerName)
end

function HoN_ReplayController:ClearPlayers()
	HoN_ReplayController.indexedPlayers = {}
end

function HoN_ReplayController:AddVoice(playerName)
	table.insert(HoN_ReplayController.indexedVoices, playerName)
end

function HoN_ReplayController:ClearVoices()
	HoN_ReplayController.indexedVoices = {}
end

local function ChooseViewpointIndex(i)
	local isStream = GetCvarBool('replay_v2')
	local comboBoxName = (isStream and 'specui_operation_combobox') or 'specui_viewpoint_combobox'
	UIManager.GetInterface('game_replay_control'):GetWidget(comboBoxName):SetSelectedItemByIndex(i)
end

function HoN_ReplayController:SelectPlayerSpectate(playerName)
	for i,v in ipairs(HoN_ReplayController.indexedPlayers) do
		if (v == playerName) then
			ChooseViewpointIndex(i)
		end
	end
end
interface:RegisterWatch("SpecUISelectPOV", function(_, ...) HoN_ReplayController:SelectPlayerSpectate(...) end)

function FastForward(param)
	param = AtoB(param)
	if (param) then
		HoN_ReplayController.prevSpeed = tonumber(interface:UICmd('ReplayGetPlaybackSpeed()'))
		Cmd('ReplaySetPlaybackSpeed 3')
	elseif (HoN_ReplayController.prevSpeed and HoN_ReplayController.prevSpeed > 0) then
		Cmd('ReplaySetPlaybackSpeed '..HoN_ReplayController.prevSpeed)
	else
		Cmd('ReplaySetPlaybackSpeed 0')
	end
end
interface:RegisterWatch("ReplayFFToggle", function (_, ...) FastForward(...) end)

function JumpIn(param)
	param = AtoB(param)
	
	-- textbox will not eat 'up' event
	local chatBoxShowing = GetCvarBool('_game_chat_inputbox_focused')
	if chatBoxShowing then
		return
	end

	-- if the button was held down, and is now being released, go back to the spectator view
	if ((not param) and ((not HoN_ReplayController.povTime) or (HostTime() >= HoN_ReplayController.povTime))) then
		ChooseViewpointIndex(0)
	end

	-- handle button presses with a player selected
	if (NotEmpty(HoN_ReplayController.selectedPlayer) and (param)) then
		if (GetCvarInt('replay_client') ~= -1) then
			ChooseViewpointIndex(0)
		else
			HoN_ReplayController:SelectPlayerSpectate(HoN_ReplayController.selectedPlayer)
		end
	end
	-- the point at which the button becomes held down instead of pressed
	HoN_ReplayController.povTime = (HostTime() + 250)
end
interface:RegisterWatch("SpecUIJumpIn", function (_, ...) JumpIn(...) end)
interface:RegisterWatch("SelectedPlayerInfo0", function(_,name) HoN_ReplayController.selectedPlayer = name end)

function HoN_ReplayController:SetFrameFromTime(minutes, seconds)
	minutes, seconds = tonumber(minutes) or 0, tonumber(seconds) or 0
	local frame = (((minutes % 60) * 1200) + (seconds * 20))
	Cmd("ReplaySetFrame "..frame)
end

function HoN_ReplayController:ReplayControlMainUI2_OnFrame()
	local time = GetTime() - HoN_ReplayController.StreamingUIShowTime
	local alpha = 0
	if (time < 5000) then
		local pi = 3.14159265
		local rate = ((time % 1000) / 1000) * pi 
		alpha = math.abs(math.sin(rate))
	end
	GetWidget('replay_control_main2_alphabg', 'game_replay_control'):SetBorderColor('1 1 1 '..tostring(alpha))
end