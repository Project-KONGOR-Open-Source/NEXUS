----------------------------------------------------------
--	Name: 		Global Main Script	            		--
--  Copyright 2015 Frostburn Studios					--
--  ATTENTION MODDERS: DO NOT MODIFY THIS SCRIPT        --
----------------------------------------------------------

if LuaTriggerSystem then
TriggerManager = LuaTriggerSystem.InitManager("luaTriggerManager")

systemBarStatus = TriggerManager:CreateTrigger('systemBarStatus')
systemBarStatus.view						= 0
systemBarStatus.socialPanelView				= 0
-- systemBarStatus.friendsCount				= 0
-- systemBarStatus.notificationsCount		= 0
systemBarStatus.systemNotificationsCount	= 0
systemBarStatus.systemNotificationsBusy		= false
end
local _G = getfenv(0)
local HoN_GMain = _G['HoN_GMain'] or {}
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()
local delayedFunctionTable = {}
UI = UI or {}
UI.RequiresValidation = true
UI.Shennanigans = false
UI.Script = {}
UI.ScriptVersions = {				-- These *must* be present
	['Main_Interface'] 		= '33', -- main.interface
	['Global Main'] 		= '34'	-- global_main.lua
	}
UI.ScriptVersions2 = {				-- These are checked only if they are present
	['AlienFX'] 			= '30',	-- alienfx.lua
	['Database'] 			= '31',	-- database.lua
	-- ['Create_Game'] 		= '2',	-- create_game.lua
	['PlayerHosted'] 		= '32', -- player_hosted.lua
	['Game'] 				= '39',	-- game_new.lua
	['Game_Interface'] 		= '39', -- game.interface
	['HonTour'] 			= '5',	-- hontour.lua
	['Lobby'] 				= '33',	-- game_lobby.lua
	['Main'] 				= '33',	-- main.lua
	['Matchmaking'] 		= '38',	-- matchmaking.lua
	['Match_Stats'] 		= '33',	-- match_stats.lua
	['News'] 				= '32',	-- news.lua
	['RAP'] 				= '30',	-- report_a_player.lua
	['Region'] 				= '33',	-- regions.lua
	['SpecUI'] 				= '35', -- specui.lua
	['Shop'] 				= '30',	-- game_shop.lua
	['Shop_Interface'] 		= '30', -- game_shop_v3.interface
	['Store'] 				= '34',	-- store.lua
	['Social Panel'] 		= '30',	-- social_panel.lua
	['Communicator']		= '32', -- communicator.lua
	['Notifications']		= '32', -- notifications.lua
	['Social_IM']			= '31',	-- social_im.lua
	['Web Panel'] 			= '31',	-- web_panel.lua
	['Plinko'] 				= '5', 	-- plinko.lua
	['Plinko_Interface'] 	= '2',	-- plinko.package
	['Codex'] 				= '1', 	-- codex.lua
	['Codex_Interface'] 	= '1',	-- codex.package
}

UI.WidgetEditIndex = -1

function RegisterScript(scriptName, scriptVersion)
	UI = UI or {}
	UI.Script = UI.Script or {}
	UI.Script[scriptName] = scriptVersion
end
RegisterScript('Global Main', '34')

function println(stringVar)
	print(tostring(stringVar)..'\n')
end

function e(stringVar, stringVal)
	print('^rError: ' .. tostring(stringVar)..' | ' .. tostring(stringVal) .. '\n')
end

function SetActiveRegion(region)
	UI = UI or {}
	UI.ActiveRegion = region
end

function groupcall(groupName, uicall, fromInterface)
	local groupTable
	if (fromInterface) then
		local interfaceWidget = UIManager.GetInterface(fromInterface)
		if (interfaceWidget) then
			groupTable = interfaceWidget:GetGroup(groupName)
		end
	else
		groupTable = UIManager.GetActiveInterface():GetGroup(groupName)
	end
	if (groupTable) then
		for index, widget in ipairs(groupTable) do
			widget:UICmd(uicall)
		end
	else
		--println('^o groupcall could not find: ' .. tostring(groupName) .. ' in interface ' .. tostring(fromInterface)..' \n' )
	end
end

function groupfcall(groupName, functionArg, fromInterface)
	--println('^o groupfcall looking for: ' .. tostring(groupName) .. ' in interface ' .. tostring(fromInterface)..' \n')
	local groupTable
	if (fromInterface) then
		local interfaceWidget = UIManager.GetInterface(fromInterface)
		if (interfaceWidget) then
			groupTable = interfaceWidget:GetGroup(groupName)
		end
	else
		groupTable = interface:GetGroup(groupName)
	end
	if (groupTable) then
		for index, widget in ipairs(groupTable) do
			functionArg(index, widget, groupName)
		end
	else
		--println('^o groupfcall could not find: ' .. tostring(groupName) .. ' in interface ' .. tostring(fromInterface)..' \n')
	end
end

function ShowOnly(widgetName, fromInterface)
	local groupTable, widget
	if (fromInterface) then
		local interfaceWidget = UIManager.GetInterface(fromInterface)
		if (interfaceWidget) then
			widget = interfaceWidget:GetWidget(widgetName)
			if (widget and NotEmpty(widget:GetGroupName())) then
				groupTable = interfaceWidget:GetGroup(widget:GetGroupName())
			end
		end
	else
		widget = interface:GetWidget(widgetName)
		if (widget and NotEmpty(widget:GetGroupName())) then
			groupTable = interface:GetGroup(widget:GetGroupName())
		end
	end
	if (groupTable) then
		for i,w in ipairs(groupTable) do
			if (w ~= widget) then
				w:SetVisible(0)
			else
				w:SetVisible(1)
			end
		end
	end
end

function SetGroupVisibility(groupName, visibility, fromInterface)
	local groupTable
	if (fromInterface) then
		local interfaceWidget = UIManager.GetInterface(fromInterface)
		if (interfaceWidget) then
			groupTable = interfaceWidget:GetGroup(groupName)
		end
	else
		groupTable = interface:GetGroup(groupName)
	end
	if (groupTable) then
		for i,w in ipairs(groupTable) do
			w:SetVisible(visibility)
		end
	end
end

function round(num, idp)
	local num = tonumber(num)
	if (num) and type(num) == 'number' then
		return tonumber(format("%." .. (idp or 0) .. "f", num))
	else
		return nil
	end
end

function commaSeperate(n)
	local left,num,right = string.match(n,'^([^%d]*%d)(%d*)(.-)$')
	return left..(num:reverse():gsub('(%d%d%d)','%1,'):reverse())..right
end

function explode(d,p)
	if (d) and (p) and (type(p) == 'string') then
		  local t, ll
		  t={}
		  ll=0
		  if(#p == 1) then return {p} end
			while true do
			  l=find(p,d,ll,true)
			  if l~=nil then
				tinsert(t, sub(p,ll,l-1))
				ll=l+1
			  else
				tinsert(t, sub(p,ll))
				break
			  end
			end
		  return t
	else
		println('Explode error d: ' .. tostring(d) .. ' in ' .. tostring(p))
	end
end

function split(str, delim, maxNb)
    if str == nil then return nil end
    if find(str, delim) == nil then
        return { str }
    end
    if maxNb == nil or maxNb < 1 then
        maxNb = 0
    end
    local result = {}
    local pat = "(.-)" .. delim .. "()"
    local nb = 0
    local lastPos
    for part, pos in gfind(str, pat) do
        nb = nb + 1
        result[nb] = part
        lastPos = pos
        if nb == maxNb then break end
    end
    if nb ~= maxNb then
        result[nb + 1] = sub(str, lastPos)
    end
    return result
end

function pairsByKeys (t, f)
  local a = {}
  for n in pairs(t) do tinsert(a, n) end
  tsort(a, f)
  local i = 0      -- iterator variable
  local iter = function ()   -- iterator function
	i = i + 1
	if a[i] == nil then return nil
	else return a[i], t[a[i]]
	end
  end
  return iter
end

function table.copy(t)
  local u = { }
  for k, v in pairs(t) do u[k] = v end
  return setmetatable(u, getmetatable(t))
end

function printTable(printThatTable)
	if (type(printThatTable) == 'table') then
		for i,v in pairs(printThatTable) do
			print('i: '..tostring(i)..' | v: '.. tostring(v)..'\n')
		end
	else
		print('printTable: ' .. tostring(printThatTable) .. ' is not a table \n')
	end
end

function printTable2(toPrint, prefix, prefixIsTab)
	prefix = prefix or ''
	if (not toPrint) then
		print('printTable2: ' .. tostring(toPrint) .. ' is not a table\n')
		return
	end

	for k,v in pairs(toPrint) do
		if type(v) == 'table' then
			local usePrefix = '    '
			if prefixIsTab then
				usePrefix = '\t'
			end
			print(prefix..k..' = {\n')
			printTable2(v, usePrefix)
			print(prefix..'},\n')
		else
			local displayValue = tostring(v)
			if type(v) == 'string' then
				displayValue = "\""..tostring(v).."\""
			end
			if type(k) == 'string' then
				print(prefix.."['"..k.."'] = "..displayValue..',\n')
			else
				print(prefix..k .. ' = '..displayValue..'\n')

			end
		end
	end

end

function printTableDeep(printThatTable)
	if (not printThatTable) then
		Echo('printTableDeep: ' .. tostring(printThatTable) .. ' is not a table')
		return
	end

	local function printTableRecur(table, prefix)
		for key, value in pairs(table) do
			Echo(prefix.."k: "..tostring(key)..' | v: '..tostring(value))

			if (type(value) == "table") then
				printTableRecur(value, prefix.."  ")
			end
		end
	end

	printTableRecur(printThatTable, "")
end

function PrintList(...)
	for k,v in ipairs(arg) do
		Echo("k:"..k.." | v:"..tostring(v).." | type: "..type(v))
	end
end

function PrintListDeep(...)
	local function printTable(table, prefix)
		for k,v in pairs(table) do
			Echo(prefix.."k:"..k.." | v:"..tostring(v).." | type: "..type(v))
			if (type(v) == "table") then
				printTable(v, prefix.."\t")
			end
		end
	end

	for k,v in ipairs(arg) do
		Echo("k:"..k.." | v:"..tostring(v).." | type: "..type(v))
		if (type(v) == "table") then
			printTable(v, "\t")
		end
	end
end

function newTable()
	return {}
end

function EscapeString(incString)
	return string.gsub(incString, [[']], [[\']])
end

function EscapeString2(incString)
	return string.gsub(incString, [["]], [[\"]])
end

function delayFunction(duration, callFunc, inTable, a1, a2, a3)
	if (#delayedFunctionTable <= 0) then
		local function delayThread ()
			wait(duration)
			for index, value in ipairs(delayedFunctionTable) do
				print(value.inTable[value.callFunc](value.a1, value.a2, value.a3))
			end
			delayedFunctionTable = {}
		end
		delayThread = newthread(delayThread)
	end
	table.insert(delayedFunctionTable, {callFunc = callFunc, inTable = inTable, a1 = a1, a2 = a2, a3 = a3})
end

function memoize (f)
	if (f) then
		local mem = {} -- memoizing table
		setmetatable(mem, {__mode = "kv"}) -- make it weak
		return function (x) -- new version of f, with memoizing
			if (x) then
				local r = mem[x]
				if r == nil then -- no previous result?
					r = f(x) -- calls original function
					mem[x] = r -- store result for reuse
				end
				return r
			end
		end
	end
end

function memoize3 (f)
	if (f) then
		local mem = {} -- memoizing table
		setmetatable(mem, {__mode = "kv"}) -- make it weak
		return function (x,a1,a2) -- new version of f, with memoizing
			if (x) then
				local r = mem[x]
				if r == nil then -- no previous result?
					r = f(x,a1,a2) -- calls original function
					mem[x] = r -- store result for reuse
				end
				return r
			end
		end
	end
end

function memoize2 (f)
	if (f) then
		local mem = {} -- memoizing table
		setmetatable(mem, {
			__mode = "kv",  -- make it weak
			__index = function(t, k)
				local x = f(k) -- calls original function
				rawset(t, x) -- store result for reuse
				return x
			end
		})
		return function (x) -- new version of f, with memoizing
			return mem[x]
		end
	end
end

function memoizeR2 (f)
	if (f) then
		local mem = {} -- memoizing table
		setmetatable(mem, {__mode = "kv"}) -- make it weak
		return function (x) -- new version of f, with memoizing
			if (x) then
				local rT = mem[x]
				local r1, r2

				if (rT ~= nil) then
					r1, r2 = rT[1], rT[2]
				end
				if ((rT == nil) or (r1 == nil) or (r2 == nil)) then -- no previous result?
					r1, r2 = f(x) -- calls original function
					mem[x] = {r1, r2} -- store result for reuse
				end
				return r1, r2
			end
		end
	end
end

function memoizeObject (f)
	if (f) then
		local mem = {} -- memoizing table
		setmetatable(mem, {__mode = "kv"}) -- make it weak
		return function (x,a1,a2) -- new version of f, with memoizing
			if (x) then
				local r = mem[x]
				if r == nil or not r:IsValid() then -- no previous result?
					r = f(x,a1,a2) -- calls original function
					mem[x] = r -- store result for reuse
				end
				return r
			end
		end
	end
end

function Min(value1, value2)
	if tonumber(value1) and tonumber(value2) and (tonumber(value1) > tonumber(value2)) then
		return value2
	else
		return value1
	end
end

function Max(value1, value2)
	if tonumber(value1) and tonumber(value2) and (tonumber(value1) < tonumber(value2)) then
		return value2
	else
		return value1
	end
end

function FtoP(value1, idp)
	if (not idp) then
		return (tonumber(value1) * 100) .. '%'
	else
		return round((tonumber(value1) * 100), idp) .. '%'
	end
end

-- FtoT has variable args, which makes this ick
function FtoT(...)
	local str = ""
	for i,v in ipairs(arg) do
		if (tonumber(v)) then
			str = str .. v
		else
			str = str .. "'"..v.."'"
		end

		if (arg[i+1]) then
			str = str..", "
		end
	end

	if (NotEmpty(str)) then
		return interface:UICmd("FtoT("..str..")")
	else
		return ""
	end
end

function SelectAvatar(inputString)
	printdb('^r SelectAvatar: '..' | ' .. tostring(inputString))
	interface:UICmd("SelectAvatar('"..inputString.."')")
end

function SpawnHero(inputString)
	printdb('^r SpawnHero: '..' | ' .. tostring(inputString))
	interface:UICmd("SpawnHero('"..inputString.."')")
end

function GameTokens()
	return AtoN(interface:UICmd("GameTokens()"))
end

function CanAccessGameMode(mode)
	return AtoB(interface:UICmd("CanAccessGameMode('"..mode.."')"))
end

function CanPlayerAccessRegion(region)
	return AtoB(interface:UICmd("CanPlayerAccessRegion('"..string.upper(region).."')"))
end

function CanGroupAccessRegion(region)
	return AtoB(interface:UICmd("CanGroupAccessRegion('"..string.upper(region).."')"))
end

function CanGroupPlayerAccessRegion(player, region)
	return AtoB(interface:UICmd("CanGroupPlayerAccessRegion('"..player.."', '"..string.upper(region).."')"))
end

function ShouldHideTMMStats()
	return AtoB(interface:UICmd("ShouldHideTMMStats()"))
end

function UIGetAccountID()
	return interface:UICmd("GetAccountID()")
end

function GetEntityDisplayName(entityName)
	return interface:UICmd("GetEntityDisplayName('"..entityName.."')")
end

function GetRegion()
	return string.lower(interface:UICmd("GetRegion()"))
end

function UIIsVerified()
	return AtoB(interface:UICmd("UIIsVerified()"))
end

function IsLeaver()
	return AtoB(interface:UICmd("IsLeaver()"))
end

function GetShowStatsMatchID()
	return AtoN(interface:UICmd("GetShowStatsMatchID()"))
end

function WaitingToShowStats()
	return AtoB(interface:UICmd("WaitingToShowStats()"))
end

function GetExperience()
	return tonumber(interface:UICmd("GetExperience()"))
end

function GetGarenaToken()
	return interface:UICmd("GetGarenaToken()")
end

function GetViewHeroName()
	return interface:UICmd("GetViewHeroName()")
end

function UIGetCookie()
	return interface:UICmd("GetCookie()")
end

function UISilverCoins()
	return interface:UICmd("SilverCoins()")
end

function UIGoldCoins()
	return interface:UICmd("GoldCoins()")
end

function GetActiveRecipe()
	return interface:UICmd("GetActiveRecipe()")
end

function UIGetBuddyGroup(name)
	return interface:UICmd("GetBuddyGroup('"..name.."')")
end

function HostTime()
	return AtoN(interface:UICmd("HostTime"))
end

function IsInGroup()
	return AtoB(interface:UICmd("IsInGroup()"))
end

function IsLoggedIn()
	return AtoB(interface:UICmd("IsLoggedIn()"))
end

function IsLoggingIn()
	return AtoB(interface:UICmd("IsLoggingIn()"))
end

function IsInGame()
	return AtoB(interface:UICmd("IsInGame()"))
end

function WidgetExists(widgetName, inInterface)
	if (inInterface) then
		local interfaceWidget = UIManager.GetInterface(inInterface)
		if (interfaceWidget) then
			return AtoB(interfaceWidget:UICmd("WidgetExists('"..widgetName.."')"))
		else
			return false
		end
	else
		return AtoB(interface:UICmd("WidgetExists('"..widgetName.."')"))
	end
end

function WidgetPicture(widgetName, fromInterface, path)
	local widget = GetWidget(widgetName, fromInterface)
	if widget ~= nil then
		-- local visible = widget:IsVisible()
		-- if not visible then
		-- 	widget:SetVisible(true)
		-- end
		local x = widget:GetAbsoluteX()
		local y = widget:GetAbsoluteY()
		local width = widget:GetWidth()
		local height = widget:GetHeight()
		local screenW = GetScreenWidth()
		local screenH = GetScreenHeight()

		x = x >= 1 and x-1 or x
		y = y >= 1 and y-1 or y
		width = x + width + 1
		width = width <= screenW and width or screenW
		height = y + height + 1
		height = height <= screenH and height or screenH
		Echo('WidgetPicture x:'..x..' y:'..y..' width:'..width..' height:'..height)

		CaptureScreen(x, y, width, height, path)

		-- if not visible then
		-- 	widget:SetVisible(false)
		-- end
	else
		Echo('WidgetPicture invalid widget with name: '..widgetName..' interface: '..fromInterface)
	end
end

function CaptureScreen(left, top, right, bottom, path)
	Echo('CaptureScreen left:'..left..' top:'..top..' right:'..right..' bottom:'..bottom)
	if NotEmpty(left) and NotEmpty(top) and NotEmpty(right) and NotEmpty(bottom) then
		local param = left..' '..top..' '..right..' '..bottom
		if NotEmpty(path) then
			param = param .. ' ' .. path
		end
		Cmd('ScreenShot '..param)
	else
		Cmd('ScreenShot')
	end
end

function IsInQueue()
	return AtoB(interface:UICmd("IsInQueue()"))
end

function HasGamePass(inputString)
	return AtoB(interface:UICmd("HasGamePass('"..inputString.."')"))
end

function HasAccessPass(inputString)
	return AtoB(interface:UICmd("HasAccessPass('"..inputString.."')"))
end

function NeedsToken(inputString)
	return AtoB(interface:UICmd("NeedsToken('"..inputString.."')"))
end

function IsTMMEnabled()
	return AtoB(interface:UICmd("IsTMMEnabled()"))
end

function UIGamePhase()
	return AtoN(interface:UICmd("GamePhase()"))
end

function AccountStanding()
	return AtoN(interface:UICmd("AccountStanding()"))
end

function AreYouSureDialog(sourceWidget, doOnYes, doOnNo, linkedBtn, leftOffset)
	local generic_are_you_sure_dialog = interface:GetWidget('generic_are_you_sure_dialog')
	local generic_are_you_sure_label_2 = interface:GetWidget('generic_are_you_sure_label_2')
	local generic_are_you_sure_label_3 = interface:GetWidget('generic_are_you_sure_label_3')
	local generic_are_you_sure_yes = interface:GetWidget('generic_are_you_sure_yes')
	local generic_are_you_sure_no = interface:GetWidget('generic_are_you_sure_no')

	generic_are_you_sure_dialog:SetVisible(true)
	generic_are_you_sure_dialog:SetHeight(sourceWidget:GetHeight())
	if (linkedBtn) then
		generic_are_you_sure_dialog:SetWidth(sourceWidget:GetWidth() * 2.2)
	else
		generic_are_you_sure_dialog:SetWidth(sourceWidget:GetWidth())
	end
	if (leftOffset) then
		generic_are_you_sure_dialog:UICmd("SetAbsoluteX("..(sourceWidget:GetAbsoluteX() - (sourceWidget:GetWidth() * 1.1))..")")
	else
		generic_are_you_sure_dialog:UICmd("SetAbsoluteX("..(sourceWidget:GetAbsoluteX())..")")
	end
	generic_are_you_sure_dialog:UICmd("SetAbsoluteY("..sourceWidget:GetAbsoluteY()..")")

	generic_are_you_sure_yes:SetCallback('onevent', function() print(doOnYes()) generic_are_you_sure_dialog:FadeOut(50) end )
	generic_are_you_sure_no:SetCallback('onevent',  function() print(doOnNo()) generic_are_you_sure_dialog:FadeOut(50) end )
end

function ChatRefresh()
	interface:UICmd("Cmd('UICall * ChatRefresh()')")
end

function PlaySoundOld(soundPath, isCont)
	if (isCont) then
		interface:UICmd("PlaySound('"..soundPath.."', "..tostring(isCont)..")")
	else
		interface:UICmd("PlaySound('"..soundPath.."')")
	end
end

function PlayChatSound(soundPath, isCont)
	-- println('PlayChatSound: ' .. tostring(soundPath) )
	if (isCont) then
		interface:UICmd("PlayChatSound('"..soundPath.."', "..tostring(isCont)..")")
	else
		interface:UICmd("PlayChatSound('"..soundPath.."')")
	end
end

function PlayMusic(musicPath, isCont)
	if (isCont) then
		interface:UICmd("PlayMusic('"..musicPath.."', "..tostring(isCont)..")")
	else
		interface:UICmd("PlayMusic('"..musicPath.."')")
	end
end

function StopMusic(fadeOut)
	if (fadeOut == nil) then
		fadeOut = true
	end

	interface:UICmd("StopMusic("..tostring(fadeOut)..")")
end

function ChatAddBuddy(stringVar)
	interface:UICmd("ChatAddBuddy('"..stringVar.."')")
end

function ChatRemoveBuddy(stringVar)
	interface:UICmd("ChatRemoveBuddy('"..stringVar.."')")
end

function ChatInviteUserToClan(stringVar)
	interface:UICmd("ChatInviteUserToClan('"..stringVar.."')")
end

function ChatRemoveClanMember(stringVar)
	interface:UICmd("ChatRemoveClanMember('"..stringVar.."')")
end

function ChatUserIsInClan(stringVar)
	return AtoB(interface:UICmd("ChatUserIsInClan('"..stringVar.."')"))
end

function ChatIsClanOfficer(stringVar)
	return AtoB(interface:UICmd("ChatIsClanOfficer('"..stringVar.."')"))
end

function ChatIsClanLeader(stringVar)
	return AtoB(interface:UICmd("ChatIsClanLeader('"..stringVar.."')"))
end

function IsClanMember(stringVar)
	if NotEmpty(stringVar) then
		return AtoB(interface:UICmd("IsClanMember('"..stringVar.."')"))
	else
		return false
	end
end

function IsBuddy(stringVar)
	if NotEmpty(stringVar) then
		return AtoB(interface:UICmd("IsBuddy('"..stringVar.."')"))
	else
		return false
	end
end

function IsStaff()
	return AtoB(interface:UICmd("IsStaff()"))
end

function HasAllHeroes()
	return AtoB(interface:UICmd("HasAllHeroes()"))
end

function IsFollowing(stringVar)
	return AtoB(interface:UICmd("IsFollowing('"..stringVar.."')"))
end

function ChatUserOnline(stringVar)
	if NotEmpty(stringVar) then
		return AtoB(interface:UICmd("ChatUserOnline('"..stringVar.."')"))
	else
		return false
	end
end

function ChatHasServerInfo(stringVar)
	if NotEmpty(stringVar) then
		return AtoB(interface:UICmd("ChatHasServerInfo('"..stringVar.."')"))
	else
		return false
	end
end

function ChatInGame()
	return AtoB(interface:UICmd("ChatInGame()"))
end

function ChatUserInGame(stringVar)
	if NotEmpty(stringVar) then
		return AtoB(interface:UICmd("ChatUserInGame('"..stringVar.."')"))
	else
		return false
	end
end

function CanAccessHeroProduct(stringVar)
	return AtoB(interface:UICmd("CanAccessHeroProduct('"..stringVar.."')"))
end

function IsEarlyAccessHero(stringVar)
	return AtoB(interface:UICmd("IsEarlyAccessHero('"..stringVar.."')"))
end

function CanAccessEarlyAccessProduct(stringVar)
	return AtoB(interface:UICmd("CanAccessEarlyAccessProduct('"..stringVar.."')"))
end

function CanAccessAltAvatar(stringVar)
	return AtoB(interface:UICmd("CanAccessAltAvatar('"..stringVar.."')"))
end

function GetHeroPreviewPosFromProduct(stringVar)
	return (interface:UICmd("GetHeroPreviewPosFromProduct('"..stringVar.."')"))
end

function GetHeroPreviewAnglesFromProduct(stringVar)
	return (interface:UICmd("GetHeroPreviewAnglesFromProduct('"..stringVar.."')"))
end

function GetHeroPreviewScaleFromProduct(stringVar)
	return (interface:UICmd("GetHeroPreviewScaleFromProduct('"..stringVar.."')"))
end

function GetHeroPreviewModelPathFromProduct(stringVar)
	return (interface:UICmd("GetHeroPreviewModelPathFromProduct('"..stringVar.."')"))
end

function StripClanTag(stringVar)
	if (string.find(stringVar, ']')) then
		stringVar = string.sub(stringVar, string.find(stringVar, ']') + 1)
	end
	return stringVar
end

function IsAccountInactive(stringVar)
	if true then return false end
	if NotEmpty(stringVar) then
		local accountName = GetAccountName()
		if (string.find(stringVar, ']')) then
			stringVar = string.sub(stringVar, string.find(stringVar, ']') + 1)
		end
		if (string.find(accountName, ']')) then
			accountName = string.sub(accountName, string.find(accountName, ']') + 1)
		end
		return AtoB(interface:UICmd("IsAccountInactive('"..stringVar.."')"))
	else
		return false
	end
end

function IsMe(stringVar)
	if NotEmpty(stringVar) then
		local accountName = GetAccountName()
		if (string.find(stringVar, ']')) then
			stringVar = string.sub(stringVar, string.find(stringVar, ']') + 1)
		end
		if (string.find(accountName, ']')) then
			accountName = string.sub(accountName, string.find(accountName, ']') + 1)
		end
		return string.lower(stringVar) == string.lower(accountName)
	else
		return false
	end
end

function IsNotMe(stringVar)
	if NotEmpty(stringVar) then
		local accountName = GetAccountName()
		if (string.find(stringVar, ']')) then
			stringVar = string.sub(stringVar, string.find(stringVar, ']') + 1)
		end
		if (string.find(accountName, ']')) then
			accountName = string.sub(accountName, string.find(accountName, ']') + 1)
		end
		return string.lower(stringVar) ~= string.lower(accountName)
	else
		return true
	end
end

function Empty(stringVar)
	if (stringVar) then
		return (string.len(stringVar) <= 0)
	else
		return true
	end
end

function NotEmpty(stringVar)
	if (stringVar) then
		return (string.len(stringVar) >= 1)
	else
		return false
	end
end

function GetTimeUntilTimestamp(endTime)
	local promoCounterDays, promoCounterHours, promoCounterMinutes, promoCounterSeconds = '', '', '',''
	local timeDiff = interface:UICmd("GetTimeDifference( "..endTime..", GetLocalServerTime())")
	promoCounterDays = tostring(interface:UICmd("GetDayFromTime("..timeDiff..")"))
	promoCounterHours = tostring(interface:UICmd("GetHourFromTime("..timeDiff..")"))
	promoCounterMinutes = tostring(interface:UICmd("GetMinuteFromTime("..timeDiff..")"))
	promoCounterSeconds = tostring(interface:UICmd("GetSecondFromTime("..timeDiff..")"))
	return promoCounterDays, promoCounterHours, promoCounterMinutes, promoCounterSeconds
end

function RequestWebURL(url, channel, isPopup)
	--println('RequestWebURL ' .. tostring(url) )
	local web_browser_panel = interface:GetWidget('web_browser_panel')
	UIManager.GetInterface('main'):HoNMainF('MainMenuPanelToggle', 'web_browser_panel', nil, nil, nil, nil, 21)
	--web_browser_panel:SetVisible(true)
	UIManager.GetInterface('webpanel'):HoNWebPanelF('InputURL', url, isPopup)
	if (url) and (channel) and NotEmpty(channel) then
		interface:UICmd("ChatJoinStreamChannel('"..channel.."')")
		interface:GetWidget('web_browser_stream_title'):SetText(channel)
		Cvar.CreateCvar('webbrowser_last_channel', 'string', channel)
		--print('^g ' .. channel)
	else
		interface:GetWidget('web_browser_stream_title'):SetText('')
	end
end

function GetWidgetNoMem(widget, fromInterface, hideErrors)
	--println('GetWidget Global: ' .. tostring(widget) .. ' in interface ' .. tostring(fromInterface))
	if (widget) then
		local returnWidget
		if (fromInterface) then
			local interfaceWidget = UIManager.GetInterface(fromInterface)
			if interfaceWidget then
				returnWidget = interfaceWidget:GetWidget(widget)
			elseif (not hideErrors) then
				println('^o GetWidget could not find interface ' .. tostring(fromInterface))
			end
		else
			returnWidget = interface:GetWidget(widget)
		end
		if (returnWidget) then
			return returnWidget
		else
			if (not hideErrors) then println('GetWidget Global failed to find ' .. tostring(widget) .. ' in interface ' .. tostring(fromInterface)) end
			return nil
		end
	else
		println('GetWidget called without a target')
		return nil
	end
end

function GetWidget(widget, fromInterface, hideErrors)
	--println('GetWidget Global: ' .. tostring(widget) .. ' in interface ' .. tostring(fromInterface))
	if (widget) then
		local returnWidget
		if (fromInterface) then
			local interfaceWidget = UIManager.GetInterface(fromInterface)
			if interfaceWidget then
				returnWidget = interfaceWidget:GetWidget(widget)
			elseif (not hideErrors) then
				println('^o GetWidget could not find interface ' .. tostring(fromInterface))
			end
		else
			returnWidget = interface:GetWidget(widget)
		end
		if (returnWidget) then
			return returnWidget
		else
			if (not hideErrors) then println('GetWidget Global failed to find ' .. tostring(widget) .. ' in interface ' .. tostring(fromInterface)) end
			return nil
		end
	else
		println('GetWidget called without a target')
		return nil
	end
end
GetWidget = memoizeObject(GetWidget)

function ShowWidget(widgetName, fromInterface)
	if (fromInterface) then
		local interfaceWidget = UIManager.GetInterface(fromInterface)
		if (interfaceWidget) then
			local widget = interfaceWidget:GetWidget(widgetName)
			if (widget) then
				widget:SetVisible(true)
			else
				println('ShowWidget could not find: ' .. tostring(widgetName))
			end
		else
			println('ShowWidget could not find interface: ' .. tostring(fromInterface))
		end
	else
		local widget = interface:GetWidget(widgetName)
		if (widget) then
			widget:SetVisible(true)
		else
			println('ShowWidget could not find: ' .. tostring(widgetName))
		end
	end
end

function HideWidget(widgetName, fromInterface)
	if (fromInterface) then
		local interfaceWidget = UIManager.GetInterface(fromInterface)
		if (interfaceWidget) then
			local widget = interfaceWidget:GetWidget(widgetName)
			if (widget) then
				widget:SetVisible(false)
			else
				println('HideWidget could not find: ' .. tostring(widgetName))
			end
		else
			println('HideWidget could not find interface: ' .. tostring(fromInterface))
		end
	else
		local widget = interface:GetWidget(widgetName)
		if (widget) then
			widget:SetVisible(false)
		else
			println('HideWidget could not find: ' .. tostring(widgetName))
		end
	end
end

function ToggleWidget(widgetName, fromInterface)
	if (fromInterface) then
		local interfaceWidget = UIManager.GetInterface(fromInterface)
		if (interfaceWidget) then
			local widget = interfaceWidget:GetWidget(widgetName)
			if (widget) then
				widget:SetVisible(not widget:IsVisible())
			else
				println('ToggleWidget could not find: ' .. tostring(widgetName))
			end
		else
			println('ToggleWidget could not find interface: ' .. tostring(fromInterface))
		end
	else
		local widget = interface:GetWidget(widgetName)
		if (widget) then
			widget:SetVisible(not widget:IsVisible())
		else
			println('ToggleWidget could not find: ' .. tostring(widgetName))
		end
	end
end

function GetCvarBool(cvar, checkForNil)
	--println('GetCvarBool: ' .. tostring(cvar))
	if (cvar) then
		if (Cvar.GetCvar(cvar)) then
			return Cvar.GetCvar(cvar):GetBoolean()
		elseif (checkForNil) then
			return nil
		else
			return false
		end
	else
		println('GetCvarBool: ' .. tostring(cvar))
	end
end
GetCvar = GetCvarBool
GetCvarBoolMem = memoize(GetCvarBool)

function GetCvarNumber(cvar, checkForNil)
	if (cvar) then
		if (Cvar.GetCvar(cvar)) then
			return Cvar.GetCvar(cvar):GetNumber()
		elseif (checkForNil) then
			return nil
		else
			return 0
		end
	else
		println('GetCvarNumber: ' .. tostring(cvar))
	end
end
GetCvarNumberMem = memoize(GetCvarNumber)

function GetCvarInt(cvar, checkForNil)
	if (cvar) then
		if (Cvar.GetCvar(cvar)) then
			return Cvar.GetCvar(cvar):GetNumber()
		elseif (checkForNil) then
			return nil
		else
			return 0
		end
	else
		println('GetCvarInt: ' .. tostring(cvar))
	end
end
GetCvarIntMem = memoize(GetCvarInt)

function GetCvarString(cvar, checkForNil)
	if (cvar) then
		if (Cvar.GetCvar(cvar)) then
			return Cvar.GetCvar(cvar):GetString()
		elseif (checkForNil) then
			return nil
		else
			return ''
		end
	else
		println('GetCvarString: ' .. tostring(cvar))
	end
end
GetCvarStringMem = memoize(GetCvarString)

function Set(cvarName, cvarValue, cvarType, noOverwrite)
	local cvar = Cvar.GetCvar(cvarName)
	if (cvar) then
		if (not noOverwrite) then
			Cvar.Set(cvar, tostring(cvarValue))
		end
	elseif (cvarType) then
		Cvar.CreateCvar(cvarName, cvarType, tostring(cvarValue))
	else
		println('^o Set: Unable to find cvar ' .. tostring(cvarName))
	end
end

function SetSave(cvarName, cvarValue, cvarType, noOverwrite)
	local cvar = Cvar.GetCvar(cvarName)
	if (cvar) then
		if (not noOverwrite) then
			Cvar.Set(cvar, tostring(cvarValue))
			interface:UICmd([[SetSave(']] .. cvarName .. [[')]])
		end
	elseif (cvarType) then
		Cvar.CreateCvar(cvarName, cvarType, tostring(cvarValue))
		interface:UICmd([[SetSave(']] .. cvarName .. [[')]])
	else
		println('^o Set: Unable to find cvar ' .. tostring(cvarName))
	end
end

function printdb(stringVar)
	if GetCvarBoolMem('ui_dev') then print(tostring(stringVar)..'\n') end
end

function printvar(varName, varValue)
	print(tostring(varName).. ': ' .. tostring(varValue) .. ' | ' .. type(varValue) .. '\n')
end

function TranslateOrNil(inputString)
	--println('inputString = ' .. tostring(inputString) )
	--println('Translate(inputString) = ' .. tostring(Translate(inputString)) )

	if (inputString == Translate(inputString)) then
		--println('returning nil')
		return nil
	else
		--println('returning ' .. Translate(inputString))
		return Translate(inputString)
	end
end

function TranslateOrEmpty(inputString)
	local str = Translate(inputString)
	if (str == inputString) then
		return ""
	else
		return str
	end
end

function DoSubmitForm(...)
	SubmitForm(..., 'account_id', UIGetAccountID(), 'cookie', UIGetCookie(), 'region', GetRegion())
end

function SetReferrer(userName)
	if (userName) and NotEmpty(userName) then
		Trigger("TriggerDialogBoxWebRequest",
			Translate("referrer_header", "username", userName),
			"referrer_left_button",
			"general_cancel",
			"SubmitForm('SetAsReferrer', 'f', 'set_ingame_referrer', 'referrer', '"..userName.."', 'cookie', GetCookie());",
			"",
			"referrer_title",
			"referrer_desc",
			"SetAsReferrerStatus",
			"SetAsReferrerResult",
			"",
			"referrer_success_title",
			"referrer_seccess_body"
		)
	end
end

function HoN_GMain:UserAction(action, openAction, extraInfo)
	--println('UserAction - action: ' ..tostring(action) .. ' | openAction: ' ..tostring(openAction) .. ' | extraInfo: ' ..tostring(extraInfo) )
	--local validActions = {1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22}
	if (openAction) then
		if (GetCvarBoolMem('ui_sendUserActions')) then
			printdb('^gSent UserAction - action: ' ..tostring(action) .. ' | openAction: ' ..tostring(openAction) .. ' | extraInfo: ' ..tostring(extraInfo) )
			interface:UICmd("SendAction("..action..");")
		elseif (GetCvarBoolMem('client_enableActionTracking'))  then
			interface:UICmd("SendAction("..action..");")
		end
	end
end

function BoolToColoredString(cvarString)
	local cvar = GetCvarBool(cvarString)
	if cvar then
		return '^g'..tostring(cvar)
	else
		return '^r'..tostring(cvar)
	end
end

local function VidDriverRestartedAndReloaded()
	Trigger('TriggerDialogBox', 'sysbar_driver_restarted', '', 'options_button_ok', '', '', 'sysbar_driver_restarted_desc', '')
end
interface:RegisterWatch('VidDriverRestartedAndReloaded', VidDriverRestartedAndReloaded)

local function GLIncorrectDisplay(_, param)
	if (interface:IsVisible()) then
		if AtoB(param) then
			if (not GetCvarBool('ui_hideGLIncorrectWarning')) then
				Trigger('TriggerDialogBoxWithCombobox', 'gl_incorrect_display', '', 'options_button_ok', '', '', 'gl_incorrect_display_desc', '', 'game_menu_dont_show_again', 'ui_hideGLIncorrectWarning')
			end
		else
			-- problem has been solved, hide it
			GetWidget("generic_dialog_box_combo"):FadeOut(100)
		end
	end
end
interface:RegisterWatch('GLIncorrectDisplay', GLIncorrectDisplay)

local function ScriptVersion()
	print('\n\n')
	println('=== 			^oREGION FLAGS^w            ===')
	println('^wRegion:    ^y' .. tostring(UI.ActiveRegion) )
	println('^wGetRegion: ^y' .. GetRegion() )
	println('^wGarena:    ^y' .. BoolToColoredString('cl_GarenaEnable') )
	print('\n\n')
	println('=== 			^oDEV FLAGS^w               ===')
	println('^wUI:        ^y' .. BoolToColoredString('releaseStage_ui') )
	println('^wInternal:  ^y' .. BoolToColoredString('releaseStage_dev') )
	println('^wSBT:       ^y' .. BoolToColoredString('releaseStage_test') )
	println('^wRCT:       ^y' .. BoolToColoredString('releaseStage_rc') )
	println('^wLive:      ^y' .. BoolToColoredString('releaseStage_stable') )
	print('\n\n')
	println('=== 			^oScript Versions^w         ===')
	if (UI) and (UI.Script) then
		for	scriptName, scriptVersion in pairs(UI.Script) do
			println('^w'..scriptName..':  ^y'..scriptVersion)
		end
	end
	print('\n\n')
	println('=== 			^oDev Features^w            ===')
	local featureIndex = 0
	while GetCvarBool('ui_dev_feature_'..featureIndex, true) ~= nil do
		println('^wFeature ' .. featureIndex .. ':  ' .. BoolToColoredString('ui_dev_feature_'..featureIndex))
		featureIndex = featureIndex + 1
		if (featureIndex > 100) then
			break
		end
	end
end
interface:RegisterWatch('ScriptVersion', ScriptVersion)
Cmd([[Alias ScriptVersion "Trigger ScriptVersion"]])

local function VersionValid(versionIsValid, versionName, versionNumber)
	local sysbar_ui_version_valid_btn = interface:GetWidget('sysbar_ui_version_valid_btn')
	if (sysbar_ui_version_valid_btn) then
		if (versionIsValid) then
			--sysbar_ui_version_valid_btn:SetVisible(false)
			--print('^g^:UI Version Validation Successful!^*^;\n')
		else
			--sysbar_ui_version_valid_btn:SetVisible(true)
			ScriptVersion()
			--print('^r^:UI Version Validation Failed!^*^;\n Name: ' .. tostring(versionName) .. ' \n Version: ' .. tostring(versionNumber) .. ' \n')
			ValidateScriptVersions = function() end
			Trigger('TriggerDialogBox', 'sysbar_uivalid', 'options_button_ok', 'general_cancel', '', '', 'sysbar_uivalid_desc', '')
			if GetWidget('fe2_background', nil, true) then
				GetWidget('fe2_background'):SetTexture('/ui/fe2/mainmenu/bg4.tga')
				if GetWidget('legion_scene', nil, true) then
					GetWidget('legion_scene'):SetVisible(false)
					GetWidget('hellbourne_scene'):SetVisible(false)
				end
			end
		end
	end
end

function ValidateScriptVersions(softFail)
	if (UI) and (UI.Script) then
		local versionIsValid = true
		local versionName = ''
		local versionNumber = ''
		if (UI.ScriptVersions) then
			for scriptName, scriptVersion in pairs(UI.ScriptVersions) do
				if (scriptVersion) then
					if (UI.Script[scriptName]) and (tonumber(UI.Script[scriptName]) >= tonumber(scriptVersion)) then
						UI.ScriptVersions[scriptName] = nil
					else
						versionIsValid = false
						versionName = scriptName
						versionNumber = tostring(UI.Script[scriptName]) .. ' | ' .. tostring(scriptVersion)
						break
					end
				end
			end
		end
		if (softFail) and (UI.ScriptVersions2) then
			for scriptName, scriptVersion in pairs(UI.ScriptVersions2) do
				if (UI.Script[scriptName]) then
					if (scriptVersion) then
						if (tonumber(UI.Script[scriptName]) >= tonumber(scriptVersion)) then
							UI.ScriptVersions2[scriptName] = nil
						else
							versionIsValid = false
							versionName = scriptName
							versionNumber = tostring(UI.Script[scriptName]) .. ' | ' .. tostring(scriptVersion)
							break
						end
					end
				end
			end
		end
		VersionValid(versionIsValid, versionName, versionNumber)
	end
end

function RegisterScript2(scriptName, scriptVersion)
	RegisterScript(scriptName, scriptVersion)
	if GetCvarBool('_loggedin') and (interface:GetWidget('sysbar_ui_version_valid_btn')) then
		UI.RequiresValidation = true
		interface:GetWidget('lod_main_unload_delay'):RegisterWatch('GamePhase',
		function()
			if (UI.RequiresValidation) then
				UI.RequiresValidation = false
				ValidateScriptVersions(true)
			end
		end)
	end
end

function GetFrameTime()
	return AtoN(interface:UICmd("Frametime()"))
end

function UIGetLevel()
	return AtoN(interface:UICmd("GetLevel()"))
end

function ProductTypeToPrefix(type, codeOnly)
	local prefixTable = {
		['Chat Color']			= 'cc',
		['Chat Symbol']			= 'cs',
		['Account Icon']		= 'ai',
		['Alt Avatar']			= 'aa',
		['Alt Announcement']	= 'av',
		['Taunt']				= 't',
		['Couriers']			= 'c',
		['Hero']				= 'h',
		['EAP']					= 'eap',
		['Status']				= 's',
		['Ward']				= 'w',
		['Misc']				= 'm',
		['Enhancement']			= 'en',
	}

	local prefix = prefixTable[type]
	if (not prefix) then prefix = "" elseif (not codeOnly) then prefix = prefix.."." end

	return prefix
end

function GetFontSizeForWidth(string, width, max, min)
	width = tonumber(width)

	local fontSizes = {18, 16, 14, 12, 11, 10, 9, 8, 7, 6}

	local lastTested = nil
	for _,size in ipairs(fontSizes) do
		-- if there isn't a min/max or the size fits the min/max
		if (((not min) or (size >= min)) and ((not max) or (size <= max))) then
			lastTested = size
			if (GetStringWidth("dyn_"..size, string) <= width) then
				return "dyn_"..size
			end
		end
	end

	-- if we get here it means there wasn't a fitting size, we should just return the smallest (last) tested string
	-- this will fit the minimum set by the user
	if (lastTested) then
		return "dyn_"..lastTested
	end
end

function GetFontSizeForWidthHeight(string, width, height, max, min)
	width = tonumber(width)
	height = tonumber(height)

	local fontSizes = {18, 16, 14, 12, 11, 10, 9, 8, 7, 6}

	local lastTested = nil

	for _,size in ipairs(fontSizes) do
		-- if there isn't a min/max or the size fits the min/max
		if (((not min) or (size >= min)) and ((not max) or (size <= max))) then
			lastTested = size
			if (GetStringWrapHeight("dyn_"..size, string, width) <= height) then
				return "dyn_"..size
			end
		end
	end

	-- if we get here it means there wasn't a fitting size, we should just return the smallest (last) tested string
	-- this will fit the minimum set by the user
	if (lastTested) then
		return "dyn_"..lastTested
	end
end

function GetTrigger(triggerName, hideErrors)
	--println('GetTrigger Global: ' .. tostring(triggerName))
	if (triggerName) then
		local returnTrigger

		returnTrigger = UITrigger.GetTrigger(triggerName)

		if (returnTrigger) then
			return returnTrigger
		else
			if (not hideErrors) then println('GetTrigger Global failed to find ' .. tostring(triggerName)) end
			return nil
		end
	else
		println('GetTrigger called without a trigger name')
		return nil
	end
end
GetTrigger = memoize3(GetTrigger)

function toroman(number)
	local number = tonumber(number)

	local indexMap = {'I', 'V', 'X', 'L', 'C', 'D', 'M'}
	local valueMap = {
		['I'] = 1,
		['V'] = 5,
		['X'] = 10,
		['L'] = 50,
		['C'] = 100,
		['D'] = 500,
		['M'] = 1000,
	}
	local subtractionMap = {
		['V'] = 'I',
		['X'] = 'I',
		['L'] = 'X',
		['C'] = 'X',
		['D'] = 'C',
		['M'] = 'C',
	}

	-- Empty roman string, and start at the largest roman numeral to find the next value
	local romanStr = ""
	local romanIndex = #indexMap

	while (number > 0) do
		-- Find the highest roman numeral that isn't larger than the number
		while (valueMap[indexMap[romanIndex]] > number) do
			romanIndex = romanIndex - 1
		end

		local numeral = indexMap[romanIndex]	-- the string for the numeral
		local value = valueMap[numeral]			-- the value for the numeral

		-- See if we need to do a subtraction
		local continue = true

		if (indexMap[romanIndex + 1]) then
			local nextNumeral = indexMap[romanIndex + 1]
			local nextValue = valueMap[nextNumeral]
			local subtractNumeral = subtractionMap[nextNumeral]
			local subtractValue = valueMap[subtractNumeral]

			local combinedValue = nextValue - subtractValue

			if (combinedValue <= number) then
				number = number - combinedValue
				romanStr = romanStr..subtractNumeral..nextNumeral

				continue = false
			end
		end

		-- no subtraction, just stick on the next numeral
		if (continue) then
			romanStr = romanStr..numeral
			number = number - value
		end
	end

	return romanStr
end

function IsAlan()
	return (string.lower(StripClanTag(GetAccountName())) == 'idejder')
end

function IsFriday()
	return (GetCurrentDayOfWeek() == 5)
end

function IsLeapYear(year)
	if ((year % 4) == 0) then
		if ((not ((year % 100) == 0)) or ((year % 400) == 0)) then
			return true
		end
	end

	return false
end

function SecondsInMonth(month, isLeapYear)
	local timeInMonths = {2678400, nil, 2678400, 2592000, 2678400, 2592000, 2678400, 2678400, 2592000, 2678400, 2592000, 2678400}

	if (month ~= 2) then
		return timeInMonths[month]
	else
		if (isLeapYear) then
			return 2505600
		else
			return 2419200
		end
	end
end

function NumeralTimeToUnixTimestamp(seconds, minutes, hours, days, months, years)
	local timestamp = 0

	local currentYear = years -- remember this for month calculations with leap years
	years = years - 1
	months = months - 1
	days = days - 1

	while (years >= 1970) do
		if (IsLeapYear(years)) then
			timestamp = PreciseIntAdd(timestamp, 31622400)
		else
			timestamp = PreciseIntAdd(timestamp, 31536000)
		end

		years = years - 1
	end

	while (months > 0) do
		timestamp = PreciseIntAdd(timestamp, SecondsInMonth(months, IsLeapYear(currentYear)))

		months = months - 1
	end

	timestamp = PreciseIntAdd(timestamp, (days * 86400))
	timestamp = PreciseIntAdd(timestamp, (hours * 3600))
	timestamp = PreciseIntAdd(timestamp, (minutes * 60))
	timestamp = PreciseIntAdd(timestamp, seconds)

	return timestamp
end

function UnixTimestampToNumeralTime(timeStamp)
	local secondsRemaining = timeStamp

	local years = 1970
	local months = 1
	local days = 1
	local hours = 0
	local minutes = 0
	local seconds = 0

	-- year
	while ((IsLeapYear(years) and (secondsRemaining >= 31622400)) or
		  ((not IsLeapYear(years)) and (secondsRemaining >= 31536000))) do

		if (IsLeapYear(years)) then
			secondsRemaining = PreciseIntSubtract(secondsRemaining, 31622400);
		else
			secondsRemaining = PreciseIntSubtract(secondsRemaining, 31536000);
		end
		years = years + 1
	end

	-- months
	while (secondsRemaining >= SecondsInMonth(months, IsLeapYear(years))) do
		secondsRemaining = PreciseIntSubtract(secondsRemaining, SecondsInMonth(months, IsLeapYear(years)))

		months = months + 1
	end

	-- days
	days = days + math.floor(secondsRemaining / 86400)
	secondsRemaining = secondsRemaining % 86400

	-- hours
	hours = math.floor(secondsRemaining / 3600)
	secondsRemaining = secondsRemaining % 3600

	-- minutes
	minutes = math.floor(secondsRemaining / 60)
	secondsRemaining = secondsRemaining % 60

	-- seconds
	seconds = secondsRemaining

	return seconds, minutes, hours, days, months, years
end

function IsCurrentlyWithinNumericDateRange(startDate, endDate)
	if (not (startDate and endDate)) then return false end

	local S2TimeZoneFix = -18000
	local localSecond, localMinute, localHour, localDay, localMonth, localYear = UnixTimestampToNumeralTime(PreciseIntAdd(GetLocalServerTime(), S2TimeZoneFix))

	-- if the start or end date aren't completely filled out, fill it out with some info to help make checking if it's valid easier
	if (not startDate.second) then startDate.second = 0 end
	if (not endDate.second) then endDate.second = 0 end
	if (not startDate.minute) then startDate.minute = 0 end
	if (not endDate.minute) then endDate.minute = 0 end
	if (not startDate.hour) then startDate.hour = 0 end
	if (not endDate.hour) then endDate.hour = 0 end

	if (not startDate.day) then startDate.day = localDay end
	if (not endDate.day) then endDate.day = localDay end
	if (not startDate.month) then startDate.month = localMonth end
	if (not endDate.month) then endDate.month = localMonth end

	-- Determine if we are passed/before the end/start date, which will help us determine a year
	-- if one was not given. If one was given, then we just won't have to do these checks later
	local passedEndDate = false

	if (localMonth > endDate.month) then
		passedEndDate = true
	elseif (endDate.month == localMonth) then
		if (localDay > endDate.day) then
			passedEndDate = true
		elseif (endDate.day == localDay) then
			if (localHour > endDate.hour) then
				passedEndDate = true
			elseif (endDate.hour == localHour) then
				if (localMinute > endDate.minute) then
					passedEndDate = true
				elseif (endDate.minute == localMinute) then
					if (localSecond > endDate.second) then
						passedEndDate = true
					end
				end
			end
		end
	end

	local beforeStartDate = false
	if (localMonth < startDate.month) then
		beforeStartDate = true
	elseif (startDate.month == localMonth) then
		if (localDay < startDate.day) then
			beforeStartDate = true
		elseif (startDate.day == localDay) then
			if (localHour < startDate.hour) then
				beforeStartDate = true
			elseif (startDate.hour == localHour) then
				if (localMinute < startDate.minute) then
					beforeStartDate = true
				elseif (startDate.minute == localMinute) then
					if (localSecond < startDate.second) then
						beforeStartDate = true
					end
				end
			end
		end
	end

	if (not startDate.year) then
		if (endDate.month < startDate.month) then -- This span moves between two year values
			-- If we are passed the end date, then the start year is this year
			-- If we are before the end date, then the event is running and the start year is last year
			if (passedEndDate) then
				startDate.year = localYear
			else
				startDate.year = localYear - 1
			end
		else 	-- This span is contained in the same year, set the time to the local year
			startDate.year = localYear
		end
	end
	if (not endDate.year) then
		if (endDate.month < startDate.month) then -- This span moves between two year values
			-- If we are passed the end date, then the end year is next year
			-- If we are before the end date, then the end year is this year
			if (passedEndDate) then
				endDate.year = localYear + 1
			else
				endDate.year = localYear
			end
		else
			endDate.year = localYear
		end
	end

	if (localYear < startDate.year) or
	   (localYear > endDate.year) then
		return false
	end

	if (startDate.year == localYear) then
		if (beforeStartDate) then
			return false
		end
	end

	if (endDate.year == localYear) then
		if (passedEndDate) then
			return false
		end
	end

	return true
end

function GetCurrentDayOfWeek()
	local S2TimeZoneFix = -18000
	local currTime = PreciseIntAdd(GetLocalServerTime(), S2TimeZoneFix)

	local days = currTime / 86400
	local dayOfWeek = math.floor(days % 7)

	-- UTC starts on Thursday
	local dayNames = {"Thursday", "Friday", "Saturday", "Sunday", "Monday", "Tuesday", "Wednesday"}
	local dayValues = {4, 5, 6, 0, 1, 2, 3}


	return dayValues[dayOfWeek + 1], dayNames[dayOfWeek + 1]
end

function UpdateTimeDependantFunctionality()

	--[[
	local function AprilFools()

		local lulz = GetCvarInt('ui_selectPrank', true) or math.random(10) -- RMM
		println('Today is April Fools: ' .. lulz)

		UI.Shennanigans = false
		groupfcall('midbar_button_matchmaking_bg', 		function(index, widget, groupName) widget:SetTexture('/ui/fe2/frames/menu_playnow.tga') 	end)
		groupfcall('midbar_button_matchmaking_icons', 	function(index, widget, groupName) widget:SetTexture('/ui/fe2/mainmenu/icons/N_matchmaking.tga')  end)
		groupfcall('midbar_button_creategame_bg', 		function(index, widget, groupName) widget:SetTexture('/ui/fe2/frames/menu.tga') 	end)
		groupfcall('midbar_button_creategame_icons', 	function(index, widget, groupName) widget:SetTexture('/ui/fe2/mainmenu/icons/N_creategame.tga')  end)
		groupfcall('midbar_button_publicgames_bg', 		function(index, widget, groupName) widget:SetTexture('/ui/fe2/frames/menu.tga') 	end)
		groupfcall('midbar_button_publicgames_icons', 	function(index, widget, groupName) widget:SetTexture('/ui/fe2/mainmenu/icons/N_public.tga')  end)
		groupfcall('midbar_button_matchstats_bg', 		function(index, widget, groupName) widget:SetTexture('/ui/fe2/frames/menu.tga') 	end)
		groupfcall('midbar_button_matchstats_icons', 	function(index, widget, groupName) widget:SetTexture('/ui/fe2/mainmenu/icons/N_stats.tga')  end)
		groupfcall('midbar_button_compendium_bg', 		function(index, widget, groupName) widget:SetTexture('/ui/fe2/frames/menu.tga') 	end)
		groupfcall('midbar_button_compendium_icons', 	function(index, widget, groupName) widget:SetTexture('/ui/fe2/mainmenu/icons/N_heroes.tga')  end)

		local latch = GetWidget('latch', 'main')
		latch:SetHFlip(false)
		GetWidget('main_logo'):SetHFlip(false)
		--GetWidget('background_main_bg_image'):SetHFlip(false)

		if (lulz == 1) then -- HoN Logo lulz
			latch:SetCallback('onclick', function()
				latch:UICmd([=[Rotate(GetRotation()+Rand(-20,30), 250)); If(GetRotation() ge 360, DoEvent());]=])
			end)
			latch:SetCallback('onevent', function()
				latch:UICmd([=[SlideY('-50h', 6500); Rotate(1080, 6500); SleepWidget(6500, 'SlideY(\'-70h\', 1500); FadeOut(700);');]=])
			end)
			local function GamePhase(sourceWidget)
				sourceWidget:SetRotation(0)
				sourceWidget:SetY('-1.42h')
				if (UIGamePhase() == 0) then
					sourceWidget:SetVisible(true)
					sourceWidget:SetNoClick(false)
				else
					sourceWidget:SetNoClick(true)
				end
			end
			latch:RegisterWatch('GamePhase', GamePhase)
			latch:RefreshCallbacks()
			latch:SetNoClick(false)
		elseif (lulz == 2) then -- All the worlds a store
			groupfcall('midbar_button_matchmaking_bg', 		function(index, widget, groupName) widget:SetTexture('/ui/fe2/frames/menu_playnow.tga') 	end)
			groupfcall('midbar_button_matchmaking_icons', 	function(index, widget, groupName) widget:SetTexture('/ui/fe2/mainmenu/icons/N_store.tga')  end)
			groupfcall('midbar_button_creategame_bg', 		function(index, widget, groupName) widget:SetTexture('/ui/fe2/frames/menu_playnow.tga') 	end)
			groupfcall('midbar_button_creategame_icons', 	function(index, widget, groupName) widget:SetTexture('/ui/fe2/mainmenu/icons/N_store.tga')  end)
			groupfcall('midbar_button_publicgames_bg', 		function(index, widget, groupName) widget:SetTexture('/ui/fe2/frames/menu_playnow.tga') 	end)
			groupfcall('midbar_button_publicgames_icons', 	function(index, widget, groupName) widget:SetTexture('/ui/fe2/mainmenu/icons/N_store.tga')  end)
			groupfcall('midbar_button_matchstats_bg', 		function(index, widget, groupName) widget:SetTexture('/ui/fe2/frames/menu_playnow.tga') 	end)
			groupfcall('midbar_button_matchstats_icons', 	function(index, widget, groupName) widget:SetTexture('/ui/fe2/mainmenu/icons/N_store.tga')  end)
			groupfcall('midbar_button_compendium_bg', 		function(index, widget, groupName) widget:SetTexture('/ui/fe2/frames/menu_playnow.tga') 	end)
			groupfcall('midbar_button_compendium_icons', 	function(index, widget, groupName) widget:SetTexture('/ui/fe2/mainmenu/icons/N_store.tga')  end)
		elseif (lulz == 3) then -- forever alone :(
			groupfcall('sysbar_friends_labels', 		function(index, widget, groupName) widget:UICmd([=[SetWatch('')]=]) widget:SetText('0') 	end)
			groupfcall('sysbar_friends_icons', 			function(index, widget, groupName) widget:SetTexture('/ui/ui_temp/friends.tga') 	end)
		elseif (lulz == 4) then -- Tom is your friend
			local function TomIsYourFriend()
				if (not AtoB( GetWidget('cc_panel_friends_main_lobby_listbox'):UICmd([=[HasListItem('S2Tom')]=]) ) ) then
					GetWidget('cc_panel_friends_main_lobby_listbox'):AddTemplateListItemWithSort('cc_friend_online', 'S2Tom', '0S2Tom', 'username', 'S2Tom', 'tooltip', '', 'verified', 'true')
				end
			end
			GetWidget('cc_panel_friends_main_lobby_listbox'):RegisterWatch('ChatBuddyOnline', TomIsYourFriend)
		elseif (lulz == 5) then -- 	gold lulz
			local curGold = tonumber(interface:UICmd('GoldCoins'))
			local sysbar_gold_label = GetWidget('sysbar_gold_label')
			local sysbar_coins_gold = GetWidget('sysbar_coins_gold')
			local function gold()
				curGold = curGold + 5
				if (curGold <= 9005) then
					sysbar_gold_label:UICmd([=[SetText(FtoA(]=]..curGold..[=[, 0, 0, ',')); SetWidth( GetStringWidth('dyn_10', (FtoA(]=]..curGold..[=[, 0, 0, ',')) ) );]=])
					sysbar_coins_gold:UICmd([=[SetWidth( GetStringWidth('dyn_10', (FtoA(]=]..curGold..[=[, 0, 0, ',')) ) + GetWidthFromString('3.2h') );]=])
					sysbar_gold_label:Sleep(1, function() gold() end)
				else
					sysbar_gold_label:UICmd([=[SetText(':(')]=])
					sysbar_gold_label:Sleep(10000, function()
						sysbar_coins_gold:UICmd([=[SetWidth( GetStringWidth('dyn_10', (FtoA(GoldCoins, 0, 0, ',')) ) + GetWidthFromString('3.2h') );]=])
						sysbar_gold_label:UICmd([=[SetText(FtoA(GoldCoins, 0, 0, ',')); SetWidth( GetStringWidth('dyn_10', (FtoA(GoldCoins, 0, 0, ',')) ) );]=])
					end)
				end
			end
			gold()
		elseif (lulz == 6) then -- 	login
			local function rotate()
				local rotation = latch:GetRotation()
				latch:SetRotation(rotation + 1)
				if (rotation <= 720) then
					latch:Sleep(1, function()
						rotate()
					end)
				end
			end
			rotate()
		elseif (lulz == 7) then -- logo flip
			GetWidget('main_logo'):SetHFlip(true)
			--GetWidget('background_main_bg_image'):SetHFlip(true)
			latch:SetHFlip(true)
		elseif (lulz == 8) then -- 	Matchmaking is space invaders
			UI.Shennanigans = true
		elseif (lulz == 9) then -- 	Pong
			UI.Shennanigans = true
		elseif (lulz == 10) then -- Handprint

		end

	end
	--]]
	--[[
	local function Birfdey()
		local value, setDefault, setCurrent = GetDBEntry('lulz_5', 1, false, false, false)
		if (setDefault) then
			GetWidget('announce_tip_parent'):Sleep(5000, function()
				GetWidget('announce_tip_icon'):SetVisible(0)
				GetWidget('announce_tip_icon_label'):SetVisible(0)
				GetWidget('announce_tip_icon_desc'):SetVisible(0)
				GetWidget('announce_tip_parent'):SetVisible(1)

				GetWidget('announce_tip_parent'):SetHeight('1h')
				GetWidget('announce_tip_parent'):SetWidth('1h')
				GetWidget('announce_tip_parent'):SetX('-5h')
				GetWidget('announce_tip_parent'):SetY('5h')

				GetWidget('announce_tip_parent'):Scale('65h', '18h', 750)

				GetWidget('announce_tip_parent'):Sleep(750, function()
					GetWidget('announce_tip_icon'):SetTexture('/ui/fe2/store/icons/effects.tga')
					GetWidget('announce_tip_icon_label'):SetText('You have ^y1^* friend with a birthday today!')
					GetWidget('announce_tip_icon_desc'):SetText('[S2]Bangerz ^wis 26^*')
					GetWidget('announce_tip_icon'):FadeIn(150)
					GetWidget('announce_tip_icon_label'):FadeIn(150)
					GetWidget('announce_tip_icon_desc'):FadeIn(150)
				end)
			end)
		end
	end
	--]]


	local function HolidayStorePromoMessage()

		print('doing holiday whatever message ====================================\n')

		local holidaySaleNotice = GetDBEntry('holidaySaleNotice') or {}

		local hadHolidayNotification = holidaySaleNotice[Client.GetAccountID()] or false

		if HoN_Region.regionTable[HoN_Region.activeRegion].hasHolidayNotification and (not hadHolidayNotification) then
			GetWidget('mainLoginHolidayMessage'):FadeIn(250)
		end

		GetWidget('mainLoginHolidayMessageOK'):SetCallback('onclick', function(widget)
			holidaySaleNotice[Client.GetAccountID()] = true
			GetDBEntry('holidaySaleNotice', holidaySaleNotice, true)
			widget:GetWidget('mainLoginHolidayMessage'):FadeOut(250)
		end)

	end

	local function DisplaySnow()
		ResourceManager.LoadContext('snow')
		GetWidget('menu_effects_snow'):SetVisible(1)
	end

	--  Timestamp (of start), Extra days beforehand, Event function, Condition
	-- Start Month, Start Day, End Month, End Day, Event function, Conditions
	-- the event will start at 0:00 of the start date
	-- the event will end at 0:00 of the end date, so if you specify 12, 25 as the end day, it will stop as soon as it starts, not after the day is over
	---[[
	local eventTable = {
		{12, 1, 2, 1,  DisplaySnow, GetCvarBool('releaseStage_stable')										},		-- Snow
		{12, 1, 2, 1,  DisplaySnow, GetCvarBool('releaseStage_rc')											},		-- Snow
		{12, 1, 2, 1,  DisplaySnow, (GetCvarBool('releaseStage_test') or GetCvarBool('releaseStage_dev'))	},		-- Snow (SBT Flagged)
		--{1365029999, 3,  AprilFools, GetCvarBool('releaseStage_stable')										},		-- April Fools
		--{1365029999, 8,  AprilFools, GetCvarBool('releaseStage_rc')											},		-- April Fools (RCT Flagged)
		--{1365029999, 8,  AprilFools, (GetCvarBool('releaseStage_test') or GetCvarBool('releaseStage_dev'))	},		-- April Fools (SBT Flagged)
		--{1364774400, 0,  AprilFools, GetCvarBool('releaseStage_stable')										},		-- April Fools
		--{1364774400, 0,  AprilFools, GetCvarBool('releaseStage_rc')											},		-- April Fools (RCT Flagged)
		--{1364774400, 0,  AprilFools, (GetCvarBool('releaseStage_test') or GetCvarBool('releaseStage_dev'))	},		-- April Fools (SBT Flagged)
		--{1341964861, 0,  Birfdey, (GetCvarBool('releaseStage_test') or GetCvarBool('releaseStage_dev'))	},
		--{1341914461, 0,  Birfdey, (GetCvarBool('releaseStage_test') or GetCvarBool('releaseStage_dev'))	}	-- dev
		-- {12, 17, 1, 2, HolidayStorePromoMessage, true, 2014, 2015},
	}

	for eventIndex, eventInfoTable in pairs(eventTable) do
		if eventInfoTable[6] and IsCurrentlyWithinNumericDateRange({['month'] = eventInfoTable[1], ['day'] = eventInfoTable[2], ['year'] = eventInfoTable[7]}, {['month'] = eventInfoTable[3], ['day'] = eventInfoTable[4], ['year'] = eventInfoTable[8]}) then
			if (eventInfoTable[5]) then
				eventInfoTable[5]()
			end
		end
	end
	--]]
end

function TestTutorialSystem(tutorialTrigger)
	UIManager.LoadInterface("/ui/game_objectives.interface")
	UIManager.AddOverlayInterface('game_objectives')

	if (NotEmpty(tutorialTrigger)) then
		Trigger(tutorialTrigger)
	end
end

function interface:HoNGMainF(func, ...)
  print(HoN_GMain[func](self, ...))
end
function IsWebResource(resourcePath)
	return ((string.sub(resourcePath, 1, 5) == "http:") or (string.sub(resourcePath, 1, 6) == "https:"))
end

function setTextureCheckForWeb(widget, path)
	if widget and type(widget) == 'userdata' and widget:IsValid() then
		if path and type(path) == 'string' and string.len(path) > 0 then
			if IsWebResource(path) then
				widget:SetTextureURL(path)
			else
				widget:SetTexture(path)
			end
		end
	end
end

function tableCopy(sourceTable, recurse)
	if recurse == nil then recurse = true end
	local newTable = {}
	for k,v in pairs(sourceTable) do
		if recurse and type(v) == 'table' then
			newTable[k] = tableCopy(v)
		else
			newTable[k] = v
		end
	end
	return setmetatable(newTable, getmetatable(sourceTable))
end

function tableMerge(tSource, tTarget)
	for k,v in pairs(tSource) do
		if type(v) == 'table' then
			if tTarget[k] then
				if type(tTarget[k]) == 'table' then
					tableMerge(v, tTarget[k])
				else
					tTarget[k] = tableCopy(v)
				end
			else
				tTarget[k] = tableCopy(v)
			end
		else
			tTarget[k] = v
		end
	end
end

function getTriggerParams(triggerName, paramName)
	if triggerName and type(triggerName) == 'string' and string.len(triggerName) > 0 then
		for k,v in ipairs(LT.GetTriggers()) do
			if v:getName() == triggerName then
				local paramTable = v:getParams()
				if paramName and type(paramName) == 'string' and string.len(paramName) > 0 then
					if paramTable[paramName] ~= nil then
						print(tostring(paramTable[paramName])..'\n')
					else
						print('Param '..paramName..' not found in '..triggerName..'\n')
					end
				else
					printTable2(paramTable)
				end

			end
		end
	end
end

function getTriggers(searchString)
	local useSearch = (searchString and type(searchString) == 'string' and string.len(searchString) > 0)
	local triggerCount = 0
	for k,v in pairsByKeys(LT.GetTriggers()) do
		if useSearch then
			if string.find(string.lower(v:getName()), string.lower(searchString)) then
				print(v:getName()..'\n')
				triggerCount = triggerCount + 1
			end
		else
			print(v:getName()..'\n')
			triggerCount = triggerCount + 1
		end
	end

	print(triggerCount..' triggers found.\n')
end

function isNewUI()
	return GetCvarBool('cg_newMainInterface')
end

-- Extensive Logging Helpers
function SerializeTable(val, name, skipnewlines, depth)
    skipnewlines = skipnewlines or false
    depth = depth or 0
    local tmp = string.rep(" ", depth)
    if name then tmp = tmp .. name .. " = " end
    if type(val) == "table" then
        tmp = tmp .. "{" .. (not skipnewlines and "\n" or "")
        for k, v in pairs(val) do
            tmp =  tmp .. SerializeTable(v, k, skipnewlines, depth + 1) .. "," .. (not skipnewlines and "\n" or "")
        end
        tmp = tmp .. string.rep(" ", depth) .. "}"
    elseif type(val) == "number" then
        tmp = tmp .. tostring(val)
    elseif type(val) == "string" then
        tmp = tmp .. string.format("%q", val)
    elseif type(val) == "boolean" then
        tmp = tmp .. (val and "true" or "false")
    else
        tmp = tmp .. "\"[inserializeable datatype:" .. type(val) .. "]\""
    end
    return tmp
end

function SerializeArgs(...)
    local arg = {...}
    local str = ""
    for i, v in ipairs(arg) do
        str = str .. SerializeTable(v) .. ", "
    end
    return str
end