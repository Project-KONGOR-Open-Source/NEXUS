local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface = object
local interfaceName = interface:GetName()
QA = QA or {}
QA_DQ = Database.New('qachecklist.ldb')
QA.testobjects = {} 
-- Game
QA.testobjects.hero = {}
QA.testobjects.item = {}
QA.testobjects.avatar = {}
QA.testobjects.taunt = {}
QA.testobjects.announcer = {}
QA.testobjects.courier = {}
QA.testobjects.ui = {}
QA.testobjects.other = {}
-- Main
QA.testobjects.icon = {}
QA.testobjects.symbol = {}
QA.testobjects.namecolor = {}
QA.testobjects.store = {}
QA.testobjects.chatserver = {}
QA.testobjects.matchmaking = {}
QA.testobjects.ui2 = {}
QA.testobjects.other2 = {}
QA.firstload = false
local QA_CHECKLIST_VALID = true

local function CheckListVersion()
	if (not QA.DEFAULT_CHECKLIST_VERSION) or (not QA.CURRENT_TEST_ITEM_LIST_VERSION) then
		-- qa_current.lua is missing
		QA_CHECKLIST_VALID = false
		QA.firstload = false
		if (HoN_Main) then
			HoN_Main:UICriticalError('QA qa_current.lua is missing ('..tostring(QA_DQ.listversion)..'-db / '..tostring(QA.CURRENT_TEST_ITEM_LIST_VERSION)..'-cur)', 60)
		else
			e('QA.DEFAULT_CHECKLIST_VERSION', QA.DEFAULT_CHECKLIST_VERSION)
			e('QA.CURRENT_TEST_ITEM_LIST_VERSION', QA.CURRENT_TEST_ITEM_LIST_VERSION)
		end		
	elseif (not QA_DQ.listversion) or (not QA_DQ.defversion) then
		-- Create version
		QA.firstload = true
		QA_DQ.listversion = QA.CURRENT_TEST_ITEM_LIST_VERSION
		QA_DQ.defversion = QA.DEFAULT_CHECKLIST_VERSION
	elseif (QA_DQ.listversion == QA.CURRENT_TEST_ITEM_LIST_VERSION) and (QA_DQ.defversion == QA.DEFAULT_CHECKLIST_VERSION) then
		-- Do nothing
		QA.firstload = false
	elseif (QA_DQ.listversion < QA.CURRENT_TEST_ITEM_LIST_VERSION) or (QA_DQ.defversion < QA.DEFAULT_CHECKLIST_VERSION) then
		-- Reset db and update version
		QA.firstload = true
		QA_DQ.feedback = {}
		QA_DQ.submitted = {}
		QA_DQ:Flush()		
		QA_DQ.listversion = QA.CURRENT_TEST_ITEM_LIST_VERSION
		QA_DQ.defversion = QA.DEFAULT_CHECKLIST_VERSION
	else
		-- You have gone back in time
		QA_CHECKLIST_VALID = false
		if (HoN_Main) then
			HoN_Main:UICriticalError('QA Checklist Version Error ('..tostring(QA_DQ.listversion)..'-db / '..tostring(QA.CURRENT_TEST_ITEM_LIST_VERSION)..'-cur)', 60)
		else
			e('QA Checklist Version Error ('..tostring(QA_DQ.listversion)..'-db / '..tostring(QA.CURRENT_TEST_ITEM_LIST_VERSION)..'-cur)', 61)
		end
	end
end
CheckListVersion()

-- Checklist Item Object
local function NewChecklistItem(itemType, itemChecklistTable, iconOverride, nameOverride)
	local checklist = {}
	if (QA.checklists[itemType]) then
		for _, listItem in pairs(QA.checklists[itemType]) do
			tinsert(checklist, listItem)
		end
	end
	for _, listItem in pairs(itemChecklistTable) do
		tinsert(checklist, listItem)
	end	
    return checklist
end

if (QA.current) and (QA_CHECKLIST_VALID) then
	QA_DQ:Flush()
	for objectType, objectTypeTable in pairs(QA.current) do
		for object, objectTable in pairs(objectTypeTable) do
			if (objectTable[1]) then
				if (type(objectTable[1]) == 'string') then
					QA.testobjects[objectType][objectTable[1]] = {NewChecklistItem(objectType, objectTable[2])}
				else
					if (objectTable[1][1]) and (QA.testobjects[objectType]) then
						QA.testobjects[objectType][objectTable[1][1]] = {NewChecklistItem(objectType, objectTable[2]), objectTable[1][2], objectTable[1][3]}
					end
				end
			end
		end
	end
else
	if (HoN_Main) then
		HoN_Main:UICriticalError('qa_current.lua is malformed. Check your syntax!', QA_CHECKLIST_VALID) 
	else
		e('qa_current.lua is malformed. Check your syntax!', QA_CHECKLIST_VALID)
	end
end

QA.current = nil
QA.checklists = nil