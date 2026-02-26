---------------------------------------------------------- 
--	Name: Caster Overlay Script	            			--				
--  Copyright 2015 Frostburn Studios					--
----------------------------------------------------------


local _G = getfenv(0)
HoN_Caster_Panel = _G[HoN_Caster_Panel] or {}
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort			
local interface = object
local interfaceName = object:GetName()
HoN_Caster_Panel.db = Database.New('HoN_Caster_Panel.ldb')
local db = HoN_Caster_Panel.db
RegisterScript2('Caster', '1')

local function LoadLocation()
	if (db.widgetTable) then
		for i,v in pairs(db.widgetTable) do
			local widget = interface:GetWidget(i)
			--print('^r Load Widget: ' .. tostring(i) .. ' \n')
			
			if string.find(i, 'logo') then				
				if (db.widgetTable[i].h) and (db.widgetTable[i].h ~= '_' ) then
					--print('^cdb.h: '.. db.widgetTable[i].h .. '\n')
					widget:SetHeight(db.widgetTable[i].h)
				end
				if (db.widgetTable[i].w) and (db.widgetTable[i].w ~= '_' ) then
					--print('^cdb.w: '.. db.widgetTable[i].w .. '\n')
					widget:SetWidth(db.widgetTable[i].w)					
				end
				if (db.widgetTable[i].x) and (db.widgetTable[i].x ~= '_' ) then
					--print('^cdb.x: '.. db.widgetTable[i].x.. '\n')
					widget:UICmd("SetAbsoluteX(" .. db.widgetTable[i].x .. ")")			
				end
				if (db.widgetTable[i].y) and (db.widgetTable[i].y ~= '_' ) then
					--print('^cdb.y: '.. db.widgetTable[i].y  .. '\n')
					widget:UICmd("SetAbsoluteY(" .. db.widgetTable[i].y .. ")")
				end
				if (db.widgetTable[i].i) and (db.widgetTable[i].i ~= '_' ) and (db.widgetTable[i].i ~= '' ) then
					--print('^cdb.i: '.. db.widgetTable[i].i  .. '\n')
					widget:SetTexture(db.widgetTable[i].i)			
				end	
				
				interface:GetWidget(i..'_input_h'):UICmd("SetInputLine('"..round(db.widgetTable[i].h).."')")
				interface:GetWidget(i..'_input_w'):UICmd("SetInputLine('"..round(db.widgetTable[i].w).."')")	
				interface:GetWidget('bang_bbp_dropdown'):UICmd("SetSelectedItemByValue('"..db.widgetTable[i].i.."')")		
				
			elseif string.find(i, 'label') then	
				local text = interface:GetWidget(i..'_text')
				
				-- Draggable frame
				if (db.widgetTable[i].h) and (db.widgetTable[i].h ~= '_' ) then
					--print('^cdb.h: '.. db.widgetTable[i].h .. '\n')
					widget:SetHeight(db.widgetTable[i].h)
					text:SetHeight(db.widgetTable[i].h)
				end
				if (db.widgetTable[i].w) and (db.widgetTable[i].w ~= '_' ) then
					--print('^cdb.w: '.. db.widgetTable[i].w .. '\n')
					widget:SetWidth(db.widgetTable[i].w)
					text:SetWidth(db.widgetTable[i].w)
				end
				
				if (db.widgetTable[i].x) and (db.widgetTable[i].x ~= '_' ) then
					--print('^cdb.x: '.. db.widgetTable[i].x.. '\n')
					widget:UICmd("SetAbsoluteX(" .. db.widgetTable[i].x .. ")")			
					text:UICmd("SetAbsoluteX(" .. db.widgetTable[i].x .. ")")	
				end
				if (db.widgetTable[i].y) and (db.widgetTable[i].y ~= '_' ) then
					--print('^cdb.y: '.. db.widgetTable[i].y  .. '\n')
					widget:UICmd("SetAbsoluteY(" .. db.widgetTable[i].y .. ")")
					text:UICmd("SetAbsoluteY(" .. db.widgetTable[i].y .. ")")
				end
				
				-- Label			
				if (db.widgetTable[i].t) and (db.widgetTable[i].t ~= '_' ) then
					--print('^cdb.t: '.. db.widgetTable[i].t  .. '\n')
					text:SetText(db.widgetTable[i].t)	
					if (string.len(db.widgetTable[i].t) >= 1) then
						text:SetNoClick(false)
						widget:SetNoClick(false)
					else
						text:SetNoClick(true)
						widget:SetNoClick(true)					
					end
				end
				if (db.widgetTable[i].f) and (db.widgetTable[i].f ~= '_' ) then
					--print('^c db.f: '.. db.widgetTable[i].f  .. '\n')
					if (round(db.widgetTable[i].f) >= 1) and (round(db.widgetTable[i].f) <= 24) then
						text:SetFont('dyn_'..round(db.widgetTable[i].f))		
					else
						text:SetFont('dyn_24')
					end
				end

				interface:GetWidget(i..'_input_t'):UICmd("SetInputLine('"..db.widgetTable[i].t.."')")
				if (db.widgetTable[i].f) and (db.widgetTable[i].f ~= '_' ) then					
					interface:GetWidget(i..'_input_f'):UICmd("SetInputLine('"..round(db.widgetTable[i].f).."')")	
				end				
				
			end
		end
	end
	--UITrigger.Trigger(UITrigger.GetTrigger('CasterOverlayDelayReport'))
end

local function CasterOverlayPosition (_, widget, x, y, w, h, t, i) 
	--print('^g Save Widget: ' .. widget .. ' \n')
	--print('^g Save x: ' .. x .. ' \n')
	--print('^g Save y: ' .. y .. ' \n')
	--print('^g Save w: ' .. w .. ' \n')
	--print('^g Save h: ' .. h .. ' \n')
	--print('^g Save t: ' .. t .. ' \n')
	--print('^g Save i: ' .. i .. ' \n')
	db.widgetTable = db.widgetTable or {}
	if (db.widgetTable) then		
		db.widgetTable[widget] = db.widgetTable[widget] or {}		
		if (db.widgetTable[widget]) then				
			db.widgetTable[widget].x = x
			db.widgetTable[widget].y = y
			db.widgetTable[widget].w = w
			db.widgetTable[widget].h = h
			db.widgetTable[widget].t = t
			db.widgetTable[widget].f = db.widgetTable[widget].f or '_'
			db.widgetTable[widget].i = i
			db:Flush()					
			if string.find(widget, 'logo') then
				interface:GetWidget(widget..'_input_h'):UICmd("SetInputLine('"..round(h).."')")
				interface:GetWidget(widget..'_input_w'):UICmd("SetInputLine('"..round(w).."')")	
				interface:GetWidget('bang_bbp_dropdown'):UICmd("SetSelectedItemByValue('"..i.."')")		
			elseif string.find(widget, 'label') then
				interface:GetWidget(widget..'_input_t'):UICmd("SetInputLine('"..t.."')")
				if (db.widgetTable[widget].f) and (db.widgetTable[widget].f ~= '_' ) then					
					interface:GetWidget(widget..'_input_f'):UICmd("SetInputLine('"..round(db.widgetTable[widget].f).."')")	
				end
			end
		else
			--print('^y No widget \n')
		end		
	else
		--HoN_Caster_Panel.ResetDefaults ()
		--print('^y No widgetTable \n')
	end	

end	

function HoN_Caster_Panel.ClearTable ()
	--print('^r Clear 1')
	db.widgetTable = {}
end

function HoN_Caster_Panel.ResetDefaults ()
	db.widgetTable = db.widgetTable or {}	
	--print('^r Reset 1')
	for widget,v in pairs(db.widgetTable) do
		if (db.widgetTable) then		
			db.widgetTable[widget] = db.widgetTable[widget] or {}		
			if (db.widgetTable[widget]) then				
				--print('^r Reset Widget: ' .. widget .. ' \n')
				if string.find(widget, 'logo') then
					db.widgetTable[widget].x = 500
					db.widgetTable[widget].y = 250
					db.widgetTable[widget].w = 450
					db.widgetTable[widget].h = 127
					db.widgetTable[widget].t = '_'
					db.widgetTable[widget].f = 20
					db.widgetTable[widget].i = ''
				elseif string.find(widget, 'label') then
					db.widgetTable[widget].x = 500
					db.widgetTable[widget].y = 250
					db.widgetTable[widget].w = 250
					db.widgetTable[widget].h = 127
					db.widgetTable[widget].t = ''
					db.widgetTable[widget].f = 20
					db.widgetTable[widget].i = '_'				
				end	
				db:Flush()	
			else
				--print('^y No widget \n')
			end		
		else
			--print('^y No widgetTable \n')
		end	
	end
	LoadLocation()
end

local function UpdateLabelFont(_, widget, size)
	if (widget) and (size) then
		db.widgetTable[widget].f = round(tonumber(size))	
		--print('^g Save f: ' .. db.widgetTable[widget].f .. ' \n')
	end
end

interface:RegisterWatch('CasterOverlayPosition', CasterOverlayPosition)	
interface:RegisterWatch('CasterOverlayLoad', LoadLocation)	
interface:RegisterWatch('CasterOverlayReportFont', UpdateLabelFont)	
interface:RegisterWatch('CasterOverlayReset', HoN_Caster_Panel.ResetDefaults)	
interface:RegisterWatch('CasterOverlayClear', HoN_Caster_Panel.ClearTable)	

function interface:HoN_Caster_Panel(func, ...)
  print(HoN_Caster_Panel[func](self, ...))
end	




