
local _G = getfenv(0)
local interface = _G[interface] or object
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
WidgetEditor = {}

WidgetEditor.WidgetEditIndex = -1

function WidgetEditor:Init()
	Set('widget_editor_move_stride', 10, 'float')
end

function WidgetEditor.FillWidgetList(listWidget, ...)
	listWidget:ClearItems()
	local y = 2
	while y <= #arg do
		local interface = arg[y]
		local wname = arg[y + 1]
		if wname == '' then wname = 'Not_Named' end
		local wtype = arg[y + 2]
		y = y + 3

		local lbl = interface..' ^g'..wname..'^b '..wtype

		listWidget:AddTemplateListItem('dbg_widget_editor_list', y-1, 'label', lbl);
	end
end


local SizeTypeString = {
	[1] = '',
	[2] = '%',
	[3] = '@',
	[4] = 'w',
	[5] = 'h',
	[6] = 'i',
	[7] = 'a',
}
local function IsSizeIdentifier(char)
	for _, c in pairs(SizeTypeString) do
		if c == char then
			return true
		end
	end
	return false;
end

local function GetSizeTypeString(byte)
	local s = SizeTypeString[byte] 
	if s == nil then s = '' end
	return s
end

function WidgetEditor:ParseSizeString(sizeString)
	local lastChar = sub(sizeString, -1)

	if IsSizeIdentifier(lastChar) then
		return tonumber(sub(sizeString, 1, -2)), lastChar
	else
		return tonumber(sizeString), ''
	end
end

function WidgetEditor:SetWidgetSizeCvars(cvarName, _type, value)
	local _typeStr = GetSizeTypeString(_type)
	if _typeStr == 'i' then
		value = value * 10.8
	end

	Set(cvarName, value, 'float')
	Set(cvarName..'_typestring', _typeStr, 'string')
end

function WidgetEditor.SetCurrentEditWidgetIndex(idx)
	WidgetEditor.WidgetEditIndex = idx

	local w = GetEditingWidget(WidgetEditor.WidgetEditIndex)
	local name = w:GetName()

	GetWidget("widget_editor_label"):SetText("Editing: ^g"..name)

	local _type, x = w:GetBaseX();
	WidgetEditor:SetWidgetSizeCvars('widget_editor_x', _type, x)

	local _type, y = w:GetBaseY();
	WidgetEditor:SetWidgetSizeCvars('widget_editor_y', _type, y)

	local _type, width = w:GetBaseWidth();
	WidgetEditor:SetWidgetSizeCvars('widget_editor_width', _type, width)

	local _type, height = w:GetBaseHeight();
	WidgetEditor:SetWidgetSizeCvars('widget_editor_height', _type, height)

	local r, g, b, a = w:GetColor()
	WidgetEditor:SetWidgetSizeCvars('widget_editor_color_r', '', r)	
	WidgetEditor:SetWidgetSizeCvars('widget_editor_color_g', '', g)	
	WidgetEditor:SetWidgetSizeCvars('widget_editor_color_b', '', b)	
	WidgetEditor:SetWidgetSizeCvars('widget_editor_color_a', '', a)	

	local colorScale = split(GetCvarString('gui_colorscale1_1'), ' ')
	Echo(GetCvarString('gui_colorscale1_1'))
	Echo(tostring(colorScale[1]))
	Echo(tostring(colorScale[2]))
	Echo(tostring(colorScale[4]))
	WidgetEditor:SetWidgetSizeCvars('widget_editor_colorscale_r', '', colorScale[1])	
	WidgetEditor:SetWidgetSizeCvars('widget_editor_colorscale_g', '', colorScale[2])	
	WidgetEditor:SetWidgetSizeCvars('widget_editor_colorscale_b', '', colorScale[3])	
	WidgetEditor:SetWidgetSizeCvars('widget_editor_colorscale_a', '', colorScale[4])	
end


local function GetXString()
	return GetCvarString('widget_editor_x')..GetCvarString('widget_editor_x_typestring')
end

local function GetYString()
	return GetCvarString('widget_editor_y')..GetCvarString('widget_editor_y_typestring')
end

local function GetWidthString()
	return GetCvarString('widget_editor_width')..GetCvarString('widget_editor_width_typestring')
end

local function GetHeightString()
	return GetCvarString('widget_editor_height')..GetCvarString('widget_editor_height_typestring')
end

local function GetColorString()
	return GetCvarString('widget_editor_color_r')..' '..GetCvarString('widget_editor_color_g')..' '..GetCvarString('widget_editor_color_b')..' '..GetCvarString('widget_editor_color_a')
end

local function GetColorScaleString()
	return GetCvarString('widget_editor_colorscale_r')..' '..GetCvarString('widget_editor_colorscale_g')..' '..GetCvarString('widget_editor_colorscale_b')..' '..GetCvarString('widget_editor_colorscale_a')
end

function WidgetEditor:OnChange()
	local w = GetEditingWidget(WidgetEditor.WidgetEditIndex)
	if w ~= nil then
		w:SetX(GetXString())
		w:SetY(GetYString())
		w:SetWidth(GetWidthString())
		w:SetHeight(GetHeightString())
		w:SetColor(GetColorString())
	end

	Set('gui_colorscale1_1', GetColorScaleString(), 'string')
end

function WidgetEditor:CopyToClipboard(...)
	local w = GetEditingWidget(WidgetEditor.WidgetEditIndex)

	if w  == nil then return end

	local s = ''

	for i=1, #arg do
  		if i > 1 then s = s .. ' ' end

  		local prop = arg[i]
  		if prop == 'X' then
  			s = s .. "x='" .. GetXString() .. "'"
  		elseif prop == 'Y' then
  			s = s .. "y='" .. GetYString() .. "'"
  		elseif prop == 'Width' then
  			s = s .. "width='" .. GetWidthString() .. "'"
  		elseif prop == 'Height' then
  			s = s .. "height='" .. GetHeightString() .. "'"
  		elseif prop == 'Color' then
  			s = s .. "color='" .. GetColorString() .. "'"
  		end
  	end

  	CopyToClipboard(s)
end

