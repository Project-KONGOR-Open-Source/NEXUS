---------------------------------------------------------- 
--	Name: 		AlienFX Script	            			--
--  Copyright 2015 Frostburn Studios					--
---------------------------------------------------------- 

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()
RegisterScript2('AlienFX', '30')

local lastHealthPercent = 1
local lastHostTime = 0
local animationDuration = 0
local animationStartTime = 0
local animationEndTime = 0
local animationR, animationG, animationB, animationA = 0, 0, 0, 1
local baseR, baseG, baseB, baseA = 0, 0, 0, 1
local baseDuration = 0
local alienFXState = 0
local activeAlive = true
local respawnTimer = 0
local lastR, lastG, lastB, lastA

local function SetAllLights(r, g, b, a)
	if (AlienFX.IsInitialized()) then
		if ((lastR ~= r) or (lastB ~= b) or (lastG ~= g) or (lastA ~= a)) then
			AlienFX.SetAllLights(r, g, b, a)
			lastR = r
			lastG = g
			lastB = b
			lastA = a
			--println('^c^:SetAllLights^*^; ^r'  .. r .. ' ^g ' .. g .. ' ^c ' .. b .. ' ^* ' .. a)
		end
	elseif GetWidget('alienfx_delete_me', 'game', true) then
		GetWidget('alienfx_delete_me', 'game'):SetBorderColor(r, g, b, 0.8)	
	end
end
GetWidget('alienfx_panel'):RegisterWatch('AlienFX', function(_, r, g, b, a) SetAllLights(r, g, b, a) end)

local function State0LightAnimation(iHostTime, r, g, b, a, duration)
	if ( (iHostTime) >= animationEndTime) and (duration > 0) then
		animationDuration = duration
		animationStartTime = iHostTime
		animationEndTime = iHostTime + duration
		animationR = r
		animationG = g
		animationB = b
		animationA = a		
		SetAllLights(r, g, b, a)
	else
		if (animationDuration > 0) then
			local animationCompletion = (iHostTime - animationStartTime) / animationDuration
			if (animationCompletion <= 1) then
				SetAllLights(((r * animationCompletion) + (animationR * (1 - animationCompletion) )) , ((g * animationCompletion) + (animationG * (1 - animationCompletion) )) , ((b * animationCompletion) + (animationB * (1 - animationCompletion) )) , ((a * animationCompletion) + (animationA * (1 - animationCompletion) )) )
			else
				SetAllLights(r, g, b, a)
				animationDuration = 0
			end
		else
			SetAllLights(r, g, b, a)
		end
	end
	
end

local function SetState0LightAnimation(r, g, b, a, durationA)
	baseR = r
	baseG = g
	baseB = b
	baseA = a
	baseDuration = durationA
end

local lastAnimationAttempt = 0
local function AnimationController(_, timeValue)
	timeValue = tonumber(timeValue)
	if (timeValue ) and (timeValue > lastAnimationAttempt + 20) then
		lastAnimationAttempt = timeValue
		--if (alienFXState == 0) then
			State0LightAnimation(tonumber(timeValue), baseR, baseG, baseB, baseA, baseDuration)
		--elseif (alienFXState == 1) then
			--State1LightAnimation(tonumber(timeValue), baseR, baseG, baseB, baseA, baseDuration)
		--end
	end
end

local function ActiveHealth(sourceWidget, health, maxHealth, healthPercent, healthShadow)
	local fHealthPercent = tonumber(healthPercent)
	local fHealthDelta = (fHealthPercent - lastHealthPercent)
	local iHostTime = HostTime()
	--println('^y ActiveHealth: ' .. healthPercent)
	if (iHostTime) and (alienFXState == 0) then
		--println('^g ActiveHealth: ' .. (iHostTime - lastHostTime))
		if	( (iHostTime - lastHostTime) > 20) then
			if (fHealthDelta < -0.01) and (animationDuration == 0) then	
				SetState0LightAnimation(1, 0.4, 0.4, 1, 400)
			else
				local r, g, b, a = Saturate(1 - (healthPercent - 0.50) / 0.50), ((healthPercent + (((healthPercent - 0.05) / 1.0)*0.2)) / 0.45), 0, 1
				SetState0LightAnimation(r, g, b, a, 0)
			end	
			lastHostTime = iHostTime
			lastHealthPercent = healthPercent
		end
	end
end

local function ActiveName()
	lastR = 0
	lastG = 0
	lastB = 0
	lastA = 0
	lastHealthPercent = 0
	GetWidget('alienfx_panel'):RegisterWatch('HostTime', AnimationController)
end

local function ActiveHealthRegen(sourceWidget, baseHealthRegen, healthRegen)
end

local function AnimatePlayerDeath()
	println("^y^:AnimatePlayerDeath^*^;")	
	SetState0LightAnimation(1, 0, 0, 1, 5000)
	GetWidget('alienfx_panel_game', 'game'):Sleep(10, function() SetState0LightAnimation(0, 0, 0, 1, 0) end )
end

local function AnimatePlayerRespawn(respawnIn)
	println("^y^:AnimatePlayerRespawn^*^;")	
	SetState0LightAnimation(0, 0, 0, 1, respawnIn)
	GetWidget('alienfx_panel_game', 'game'):Sleep(10, function() SetState0LightAnimation(0, 1, 1, 1, 0) end )
end

local function HeroRespawn(sourceWidget, respawnIn)
	local respawnIn = tonumber(respawnIn)
	if (not activeAlive) and (alienFXState == 1) and (respawnIn > 0) and (respawnIn < 10000) then
		AnimatePlayerRespawn(respawnIn)
		GetWidget('alienfx_panel'):UnregisterWatch('HeroRespawn')
	end
end

local function ActiveStatus(sourceWidget, status)
	local status = AtoB(status)
	if (status) then
		alienFXState = 0
	else
		alienFXState = 1
		if (activeAlive) then
			AnimatePlayerDeath()
			GetWidget('alienfx_panel'):RegisterWatch('HeroRespawn', HeroRespawn)
		end
	end
	activeAlive = status
end

-- announcers, kills, deaths, stuns, silences, purchases, respawn powerup, health potions / heals, item use
local function EnableAlienFX()
	GetWidget('alienfx_panel'):RegisterWatch('ActiveStatus', ActiveStatus)
	GetWidget('alienfx_panel'):RegisterWatch('ActiveHealthRegen', ActiveHealthRegen)
	GetWidget('alienfx_panel'):RegisterWatch('ActiveHealth', ActiveHealth)
	GetWidget('alienfx_panel'):RegisterWatch('ActiveName', ActiveName)
end

local function DisableAlienFX()
	GetWidget('alienfx_panel'):UnregisterWatch('ActiveStatus')
	GetWidget('alienfx_panel'):UnregisterWatch('ActiveHealthRegen')
	GetWidget('alienfx_panel'):UnregisterWatch('ActiveHealth')
	GetWidget('alienfx_panel'):UnregisterWatch('ActiveName')
	GetWidget('alienfx_panel'):UnregisterWatch('HostTime')
end

local function CheckForAlienFX()
	Set('ui_EnableAlienFX', 'true', 'bool', true)
	if (AlienFX) and (AlienFX.IsInitialized()) and (GetCvarBool('ui_EnableAlienFX')) then
		println("^y^:AlienFX^*^; On " .. tostring(AlienFX.IsInitialized()) )
		EnableAlienFX()
	else
		println("^y^:AlienFX^*^; Off ")	
		DisableAlienFX()
	end
	interface:UICmd("SetSave('ui_EnableAlienFX')")
end

CheckForAlienFX()

--[[			
if AlienFX.GetNumberDevices then Echo("GetNumberDevices") end			
if AlienFX.GetDeviceDescription then Echo("GetDeviceDescription") end
if AlienFX.GetNumberLights then Echo("GetNumberLights") end			
if AlienFX.GetLightDescription then Echo("GetLightDescription") end
if AlienFX.GetLightLocation then Echo("GetLightLocation") end			
if AlienFX.GetLightColor then Echo("GetLightColor") end			
if AlienFX.SetLightColor then Echo("SetLightColor") end			
if AlienFX.SetLightColorByName then Echo("SetLightColorByName") end			
if AlienFX.ResetAllLights then Echo("ResetAllLights") end			
if AlienFX.SetAllLights then Echo("SetAllLights") end

--AlienFX.SetAllLights(0,0,1,1)

local numDevs = AlienFX.GetNumberDevices()
Echo("Number of Devices: "..numDevs)
for devIndex = 0, numDevs-1, 1 do
	local devDesc, devType = AlienFX.GetDeviceDescription(devIndex)
	local numLights = AlienFX.GetNumberLights(devIndex)
	Echo(string.format('%u, Type:%u  Lights:%u  "%s"', devIndex, devType, numLights, devDesc))
	
	for lightIndex = 0, numLights-1, 1 do
		local lightDesc = AlienFX.GetLightDescription(devIndex, lightIndex)
		local vLightLocation = AlienFX.GetLightLocation(devIndex, lightIndex)
		local vLightColor, brightness = AlienFX.GetLightColor(devIndex, lightIndex)
		Echo( string.format('  %u, Color:%s %g  Location:%s  "%s"', 
			lightIndex, tostring(vLightColor), brightness, tostring(vLightLocation), lightDesc) 
		)
		
		--AlienFX.SetLightColor(devIndex, lightIndex, 0.5, 0.25, 1, 1)
	end
end
--]]

--[[
local function SetLightColor(devIndex, lightIndx, r, g, b, a)
	GetWidget('alienfx_delete_me', 'game'):SetBorderColor(r, g, b, a)
	--println('^c^:SetLightColor^*^; ^r'  .. r .. ' ^g ' .. g .. ' ^c ' .. b .. ' ^* ' .. a)
	--AlienFX.SetLightColor(r, g, b, a)
end
--]]

--[[
println('^g iHostTime: ' .. iHostTime)
println('g animationEndTime: ' .. animationEndTime)
println('g r: ' .. r)
println('g g: ' .. g)
println('g b: ' .. b)
println('g a: ' .. a)
println('g duration: ' .. duration)
--]]

