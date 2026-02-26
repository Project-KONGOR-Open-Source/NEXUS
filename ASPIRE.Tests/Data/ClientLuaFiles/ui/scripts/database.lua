-------------------------------------------------------------------------------
--  Copyright 2015 Frostburn Studios					--
-------------------------------------------------------------------------------
local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()
RegisterScript2('Database', '31')

HoN_Database = HoN_Database or {}

function GetDBEntry(entry, value, saveToDB, restoreDefault, setDefault)	
	
	HoN_DB = HoN_DB or Database.New('HoNDB_v1.ldb')
	HoN_DB.current = HoN_DB.current or {}
	HoN_DB.default = HoN_DB.default or {}	
	
	if (entry) then	
		--println('^g DB GetEntry: ' .. tostring(entry) .. ' | ' .. ' value: ' .. tostring(value) .. ' | ' .. ' saveToDB: ' .. tostring(saveToDB) .. ' | ' .. ' restoreDefault: ' .. tostring(restoreDefault)  .. ' | ' .. ' setDefault: ' .. tostring(setDefault) )
		if (value ~= nil) then	
			if (HoN_DB.default[entry]) and (not setDefault) then				
				if (restoreDefault) then				
					HoN_DB.current[entry] = HoN_DB.default[entry]
					HoN_DB:Flush()
					--println('^y DB Restore default entry: ' .. tostring(entry)) 
					return HoN_DB.default[entry], false, true					
				elseif (saveToDB) then				
					HoN_DB.current[entry] = value
					HoN_DB:Flush()
					--println('^y DB Save to db entry: ' .. tostring(entry))
					return value, false, true					
				else
					--println('^y DB loading entry 1: ' .. tostring(entry))
					return HoN_DB.current[entry], false, false	
				end			
			else
				HoN_DB.default[entry] = value
				HoN_DB.current[entry] = value
				HoN_DB:Flush()
				--println('^y DB Set default entry: ' .. tostring(entry))
				return value, true, true
			end
		else
			--println('^y DB loading entry 2: ' .. tostring(entry))
			return HoN_DB.current[entry], false, false	
		end
	else
		println('^r DB GetEntry: No entry provided')
		return nil
	end		
end

local function ForceDBFlush()
	HoN_Database.database.flusher = not HoN_Database.database.flusher
	HoN_Database.database:Flush()
end

function HoN_Database:RemoveDBEntry(entryName)
	if (entryName) then
		HoN_Database.database.current[entryName] = nil
		HoN_Database.database.default[entryName] = nil
		ForceDBFlush()
	end
end

function HoN_Database:ReadDBEntry(entryName)
	if (entryName) then -- this can return nil for unsaved values
		return HoN_Database.database.current[entryName]
	end
end

function HoN_Database:SetDBEntry(entryName, value)
	if (value == nil) then return end

	if (not HoN_Database.database.default[entryName]) then
		HoN_Database.database.default[entryName] = value
	end
	HoN_Database.database.current[entryName] = value

	ForceDBFlush()
end

function HoN_Database:RestoreDBDefault(entryName)
	if (HoN_Database.database.default[entryName]) then
		HoN_Database.database.current[entryName] = HoN_Database.database.default[entryName]
	end	
end

function HoN_Database:SetDBDefault(entryName, value)
	if (value == nil) then return end

	HoN_Database.database.default[entryName] = value
	-- only set default, even if current isn't set, just for flexibility
	-- the next function can handle the case you want to set both, and you can
	-- have just a default set without a current without causing issues

	ForceDBFlush()
end

function HoN_Database:SetDBEntryAndDefault(entryName, value)
	SetDBDefault(entryName, value)
	SetDBEntry(entryName, value)
end

function HoN_Database:ReadDBDefault(entryName)
	if (entryName) then 	-- this can return nil for unsaved values
		return HoN_Database.database.default[entryName]
	end
end

-- init the database stuff
function HoN_Database:LoadDatabase(databaseName)
	if (HoN_Database.database) then
		ForceDBFlush()
		HoN_Database.previousDatabaseName = HoN_Database.currentDatabaseName
	end

	HoN_Database.database = Database.New(databaseName)
	HoN_Database.currentDatabaseName = databaseName
	HoN_Database.database.current = HoN_Database.database.current or {}
	HoN_Database.database.default = HoN_Database.database.default or {}
end

function HoN_Database:RestorePreviousDB()
	if (HoN_Database.previousDatabaseName and NotEmpty(HoN_Database.previousDatabaseName)) then
		HoN_Database:LoadDatabase(HoN_Database.previousDatabaseName)
	end
end

-- LoadDB on run
HoN_Database:LoadDatabase('HoNDB_v2.ldb')