
-- Graphic card descriptions(regex) for diffrent level
local DeviceLevelMap = 
{
	-- order dependent!
	[4] = {},
	[3] = 
		{
			-- Nvdia
			'.*GTX [89]%d%d.*',
			'.*GTX 7[4-9]%d.*',	
			'.*GTX 6[5-9]%d.*',
			'.*GTX 550%s-Ti.*',
			'.*GTX [245][6-9]%d[^M]*$',
			'.*GTX 1%d%d%d.*',						-- GTX 1000+
			'.*GTX 5[7-9]%d%s-M.*',					-- mobile cards
			'.*GT 755%s-M.*',						
			'.*GT 750%s-M%s+D5.*',	
			'.*GT 745%s-M%s+D5.*',	
			'.*GT 68%d%s-M.*',
			'.*GT 740[^M]*$',						-- specific cards
			'.*GT 730%s+384%s-[Ss][Pp].*',
			'.*9800%s-GX2.*',
			'.*Titan.*',							-- high end versions
			'.*TITAN.*',
			'.*Quadro.*',
			-- AMD
			'.*RX .*',
			'.*R9 .*',				
			'.*R7 [23][5-9]%d.*',			
			'.*Radeon HD 48[79]%d[^M]*$',			-- non mobile version
			'.*Radeon HD [56]7[79]%d[^M]*$',
			'.*Radeon HD [567][89]%d%d[^M]*$',		
			'.*Radeon HD 77[579]%d[^M]*$',		
			'.*Radeon HD [78][89]%d%d%s-M.*',		-- mobile versions
			'.*Radeon HD 69%d%d%s-M.*',	
			'.*Radeon Pro Duo.*',					-- high end versions
			'.*FirePro.*',
			-- Intel
			'.*Iris.+6[23]00.*',
			-- 
		},
	[2] = 
		{	
			-- Nvdia
			'.*GeForce %d%d%d.*',
			'.*GeForce G[%d%s].*',
			'.*GTS.*',							-- as prefix or suffix
			'.*GT.*',
			'.*GS.*',
			'.*%d%d%d%d%s-GTX.*',
			'.*8800 Ultra.*',			
			'.*9800 GX2.*',			
			-- AMD
			--'.*Radeon.*',	
			'.*R[57] M?[2-4]%d%d.*',			
			'.*Radeon HD [2-8]%d%d%d.*',		-- exclude those defined above
			'.*HD [1-8]%d%d%d%s-D.*',		
			'.*A[68] 7%d%d.*',		
			'.*A10 7%d%d.*',	
			-- Intel
			'.*Iris.+5%d%d%d.*',
			'.*HD Graphics [45]%d%d%d.*',
		},
	[1] = {},
}

local function SetCVar(cvarName, cvarValue, cvarType)

	local cvar = Cvar.GetCvar(cvarName)
	if (cvar) then
		Cvar.Set(cvar, tostring(cvarValue))
	elseif (cvarType) then
		cvar = Cvar.CreateCvar(cvarName, cvarType, tostring(cvarValue))
		Cvar.SetSave(cvar, tostring(cvarValue))
	else
		println('^o SetCVar: Unable to find cvar ' .. tostring(cvarName))
	end
end

-- options changed only by graphic level, which does not have a panel
function SetImplicitOptionsCVarsByLevel(level)
	if (level == 1) then -- low
		SetCVar('options_foliageRenderType', 0, 'int')
	elseif (level == 2) then -- med
		SetCVar('options_foliageRenderType', 0, 'int')
	elseif (level == 3) then -- high
		SetCVar('options_foliageRenderType', 1, 'int')
	elseif (level == 4) then -- ultra
		SetCVar('options_foliageRenderType', 1, 'int')
	elseif (level == 5) then -- custom
	end
end

function SetBoolOptionsCVarsByLevel(level)

	-- still need options_bpp, options_antialiasing, vid_textureFiltering
	-- these are all dependant on what their computer supports and will need to be selected from dropdowns
	-- if (level == 0) then	 -- super low
	-- 	SetCVar('options_postprocessing', false, 'bool')
	-- 	SetCVar('options_reflections', false, 'bool')
	-- 	SetCVar('options_refraction', false, 'bool')
	-- 	SetCVar('options_dynamiclights', false, 'bool')
	-- 	SetCVar('options_foliage', false, 'bool')
	-- 	SetCVar('options_rimlighting', false, 'bool')
	--  SetCVar('options_skybox', false, 'bool')-- 	
	--  SetCVar('options_modelQuality', 'low', 'string')
	-- 	SetCVar('options_textureSize', 2, 'int')
	-- 	SetCVar('options_shaderQuality', 2, 'int')
	-- 	SetCVar('options_shadowQuality', 4, 'int')
	-- 	SetCVar('options_waterQuality', 3, 'int')
	-- 	
	if (level == 1) then -- low
		SetCVar('options_postprocessing', false, 'bool')
		SetCVar('options_reflections', false, 'bool')
		SetCVar('options_refraction', false, 'bool')
		SetCVar('options_dynamiclights', false, 'bool')
		SetCVar('options_foliage', true, 'bool')
		SetCVar('options_rimlighting', false, 'bool')
		SetCVar('options_skybox', false, 'bool')

	elseif (level == 2) then -- med
		SetCVar('options_postprocessing', false, 'bool')
		SetCVar('options_reflections', false, 'bool')
		SetCVar('options_refraction', true, 'bool')
		SetCVar('options_dynamiclights', false, 'bool')
		SetCVar('options_foliage', true, 'bool')
		SetCVar('options_rimlighting', false, 'bool')
		SetCVar('options_skybox', false, 'bool')

	elseif (level == 3) then -- high
		SetCVar('options_postprocessing', true, 'bool')
		SetCVar('options_reflections', true, 'bool')
		SetCVar('options_refraction', true, 'bool')
		SetCVar('options_dynamiclights', true, 'bool')
		SetCVar('options_foliage', true, 'bool')
		SetCVar('options_rimlighting', false, 'bool')
		SetCVar('options_skybox', true, 'bool')

	elseif (level == 4) then -- ultra
		SetCVar('options_postprocessing', true, 'bool')
		SetCVar('options_reflections', true, 'bool')
		SetCVar('options_refraction', true, 'bool')
		SetCVar('options_dynamiclights', true, 'bool')
		SetCVar('options_foliage', true, 'bool')
		SetCVar('options_rimlighting', true, 'bool')
		SetCVar('options_skybox', true, 'bool')

	elseif (level == 5) then
	end
end

local function SetNonBoolOptionsCVarsByLevel(level)

	local textFilterModes = GetTextureFilteringModes()
	local numTextFilter = table.getn(textFilterModes)
		
	local aaSettings = GetAntiAliasingModes()
	local numAASettings = table.getn(aaSettings)

	if (level == 1) then -- low

		SetCVar('options_modelQuality', 'med', 'string')
		SetCVar('options_textureSize', 1, 'int')
		SetCVar('options_shaderQuality', 2, 'int')
		SetCVar('options_shadowQuality', 4, 'int')
		SetCVar('options_waterQuality', 3, 'int')

		local textFilterIndex = 2
		local antialisingIndex = 1
		SetCVar('options_textureFiltering', textFilterModes[textFilterIndex], 'int')
		SetCVar('options_antialiasing', aaSettings[antialisingIndex], 'string')

	elseif (level == 2) then -- med

		SetCVar('options_modelQuality', 'med', 'string')
		SetCVar('options_textureSize', 1, 'int')
		SetCVar('options_shaderQuality', 1, 'int')
		SetCVar('options_shadowQuality', 2, 'int')
		SetCVar('options_waterQuality', 2, 'int')

		-- mid range filtering
		local textFilterIndex = 3
		local antialisingIndex = 1
		SetCVar('options_textureFiltering', textFilterModes[textFilterIndex], 'int')
		SetCVar('options_antialiasing', aaSettings[antialisingIndex], 'string')

	elseif (level == 3) then -- high

		SetCVar('options_modelQuality', 'high', 'string')
		SetCVar('options_textureSize', 0, 'int')
		SetCVar('options_shaderQuality', 0, 'int')
		SetCVar('options_shadowQuality', 1, 'int')
		SetCVar('options_waterQuality', 1, 'int')

		-- x8 or highest filtering
		local textFilterIndex = math.min(numTextFilter, 7)
		-- 4x or highest AA
		local antialisingIndex = math.min(numAASettings, 3)
		SetCVar('options_textureFiltering', textFilterModes[textFilterIndex], 'int')
		SetCVar('options_antialiasing', aaSettings[antialisingIndex], 'string')

	elseif (level == 4) then -- ultra

		SetCVar('options_modelQuality', 'high', 'string')
		SetCVar('options_textureSize', 0, 'int')
		SetCVar('options_shaderQuality', 0, 'int')
		SetCVar('options_shadowQuality', 1, 'int')
		SetCVar('options_waterQuality', 0, 'int')

		-- select the highest filtering
		local textFilterIndex = numTextFilter
		-- 8x or highest AA
		local antialisingIndex = math.min(numAASettings, 4)

		SetCVar('options_textureFiltering', textFilterModes[textFilterIndex], 'int')
		SetCVar('options_antialiasing', aaSettings[antialisingIndex], 'string')

	elseif (level == 5) then
	end
end

local function GetGraphicsLevel(deviceDescription)
	for level = 4, 1, -1 do
		local patterns = DeviceLevelMap[level]
		for index, pattern in pairs(patterns) do
			if string.match(deviceDescription, pattern) then
				return level
			end
		end
	end
	return nil
end

-- function to auto SetCVar graphics level by graphic card description
function AutoSetGraphicsLevel(deviceDescription)
	Echo('AutoSetGraphicsLevel: '..deviceDescription)

	local level = GetGraphicsLevel(deviceDescription)	

	if level ~= nil then
		Echo('AutoSetGraphicsLevel: setting to '..tostring(level))

		SetCVar('ui_options_gfxslider', level, 'int')
		SetImplicitOptionsCVarsByLevel(level)
		SetBoolOptionsCVarsByLevel(level)
		SetNonBoolOptionsCVarsByLevel(level)
		
		return level
	else
		return 0
	end
end

