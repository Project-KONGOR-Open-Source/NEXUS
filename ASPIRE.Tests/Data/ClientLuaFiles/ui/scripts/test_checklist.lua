-------------------------------------------------------------------------------
--	Name: 		Test Checklist Script					          			--		
--  Copyright 2015 Frostburn Studios										--
-------------------------------------------------------------------------------
local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface = object
local interfaceName = interface:GetName()
QA = QA or {}
QA.currentChecklistPage = 1
QA.currentObjectType = ''
QA.currentEntityName = ''
QA.currentDisplayName = ''
QA.currentCompleteQs = 0
QA.currentTotalQs = 0	
local MAX_CHECKLIST_SIZE = 500
local SUBMIT_TO_WEB = true		--RMM
local CLEAR_FEEDBACK = true		--RMM

local function ShowWarning(warning, warnFunc)
	interface:GetWidget('qa_tier_0_popup'):SetVisible(true)
	interface:GetWidget('qa_tier_0_popup_label_1'):SetText(FormatStringNewline(Translate(warning)))
	interface:GetWidget('qa_error_ok'):SetCallback('onclick', function() if (warnFunc) then warnFunc() end end)
	interface:GetWidget('qa_error_ok'):RefreshCallbacks()
end

local function ClearTierButtons(index)
	--groupfcall('qa_tier_'..index..'_buttons', function(index, widget, groupName) widget:UICmd([[DestroyWidget()]]) end)
	Trigger('QAClearButtons', 'qa_tier_'..index..'_buttons')
end

local function SetTitleIcon(iconPath)
	interface:GetWidget('qa_title_image'):SetTexture(iconPath or '$invis')
end

local function SetTitleText(text1, text2)
	interface:GetWidget('qa_title_label_2'):SetText(Translate(text1 or ''))
	interface:GetWidget('qa_title_label_3'):SetText(Translate(text2 or ''))
end

local function SpawnTier2Buttons(objectType)
	
	local insertPoints = {}
	insertPoints[1]  = interface:GetWidget('qa_tier_2_insertion_point_1')
	insertPoints[2]  = interface:GetWidget('qa_tier_2_insertion_point_2')
	insertPoints[3]  = interface:GetWidget('qa_tier_2_insertion_point_3')
	
	local insertParents = {}
	insertParents[1]  = interface:GetWidget('qa_tier_2_insertion_parent_1')
	insertParents[2]  = interface:GetWidget('qa_tier_2_insertion_parent_2')
	insertParents[3]  = interface:GetWidget('qa_tier_2_insertion_parent_3')	
	for i=1,3 do
		insertParents[i]:SetVisible(false)
	end
	
	local index = 1
	local insertWidget, iconPath, displayName, totalQs, x
	local isStarted, isComplete, isSubmitted  = false, false, false
	local completeQs = ''
	
	for entityName, entityTable in pairs(QA.testobjects[objectType]) do
		local entityChecklistTable = entityTable[1]
		insertWidget = insertPoints[ceil(index/10)]
		insertParent = insertParents[ceil(index/10)]
		if (insertWidget) then
			insertParent:SetVisible(true)
			--println('Insert T2: ' .. entityName.. ' ('..#entityChecklistTable..')')
			
			if (QA_DQ.submitted) and (QA_DQ.submitted[objectType]) and (QA_DQ.submitted[objectType][entityName]) then
				isComplete = true
				isSubmitted = true
				isStarted = true
			elseif (QA_DQ.feedback) and (QA_DQ.feedback[objectType]) and (QA_DQ.feedback[objectType][entityName]) and (QA_DQ.feedback[objectType][entityName]) then
				local completeCount = 0
				for _,feedbackItemTable in pairs(QA_DQ.feedback[objectType][entityName]) do
					if (feedbackItemTable[1]) and (feedbackItemTable[1] ~= 0) then
						completeCount = completeCount + 1
					end
				end
				isStarted = true
				completeQs = completeCount
				isComplete = (completeCount >= #entityChecklistTable)
				isSubmitted = false
			else
				isStarted = false
				completeQs = 0
				isComplete = false
				isSubmitted = false
			end
			
			iconPath 	= interface:UICmd([[GetEntityIconPath(']]..entityName..[[')]])
			displayName = interface:UICmd([[GetEntityDisplayName(']]..entityName..[[')]])
			
			if Empty(iconPath) 		or entityTable[3]	then iconPath 	 = entityTable[3] or '$invis'   end
			if Empty(displayName) 	or entityTable[2]	then displayName = entityTable[2] or entityName end
			
			if ((index%10) == 1) then x = '2h' else x = '' end
			insertWidget:Instantiate('qa_type_btn',
				'icon', 		iconPath,
				'AbuseType', 	displayName,
				'group', 		'qa_tier_2_buttons',
				'x',			x,
				'height',		'12h',
				'width',		'12h',
				'font', 		'dyn_11',
				'size', 		'55',
				'size2', 		'55',
				'total',		#entityChecklistTable,
				'complete',		completeQs..'/',
				'isSubmitted',	tostring(isSubmitted),
				'isStarted',	tostring(isStarted),
				'isComplete',	tostring(isComplete),
				'enabled', 		tostring(not isComplete),
				'onclicklua', 	[[QA:ClickedTier2Button(self, self:GetName(), ]]..index..[[, ']]..iconPath..[[', ']]..displayName..[[', ]]..completeQs..[[, ]]..#entityChecklistTable..[[, ']]..entityName..[[', ']]..objectType..[[')]]
			)
			if ((index%10) == 0) then
				insertWidget:Instantiate('qa_type_btn_padder', 'group', 'qa_tier_2_buttons')
			end
			index = index + 1
		else
			break
		end	
	end
	if (index < 10) then
		insertWidget:Instantiate('qa_type_btn_padder', 'group', 'qa_tier_2_buttons')
	end		
end

function QA:ClickedBackButton(sourceWidget, sourceName, sourceIndex)
	interface:UICmd([[ShowOnly('qa_tier_1_]]..UIManager.GetActiveInterface():GetName()..[[')]])
	interface:GetWidget('qa_tier_0_back_btn'):SetVisible(false)
	SetTitleIcon('/ui/icons/item_destruction.tga')
	SetTitleText('Select A Category')	
end

local function CreateLocalFeedback(currentObjectType, currentEntityName, currentChecklistPage, checklistItemStatus, itemDescriptionText)
	QA_DQ = QA_DQ or Database.New('qachecklist.ldb')
	QA_DQ.feedback = QA_DQ.feedback or {}
	QA_DQ.feedback[currentObjectType] = QA_DQ.feedback[currentObjectType] or {}
	QA_DQ.feedback[currentObjectType][currentEntityName] = QA_DQ.feedback[currentObjectType][currentEntityName] or {}
	QA_DQ.feedback[currentObjectType][currentEntityName][currentChecklistPage] = {checklistItemStatus, itemDescriptionText}
	QA_DQ:Flush()
	if (checklistItemStatus ~= 0) then
		QA.currentCompleteQs = QA.currentCompleteQs + 1
	end
end

local function GetNextFreeChecklistPage(objectType, entityName)
	for pageIndex = QA.currentChecklistPage, MAX_CHECKLIST_SIZE, 1 do
		if (QA.testobjects[objectType]) and (QA.testobjects[objectType][entityName]) and (QA.testobjects[objectType][entityName][1]) and (QA.testobjects[objectType][entityName][1][pageIndex]) then
			if (not QA_DQ.feedback) or (not QA_DQ.feedback[objectType]) or (not QA_DQ.feedback[objectType][entityName]) or (not QA_DQ.feedback[objectType][entityName][pageIndex]) or (QA_DQ.feedback[objectType][entityName][pageIndex][1] == 0) then
				interface:GetWidget('qa_tier_3_label_1'):SetText(FormatStringNewline(QA.testobjects[objectType][entityName][1][pageIndex]))
				QA.currentChecklistPage = pageIndex
				break
			end
		else			
			interface:GetWidget('qa_tier_3_label_1'):SetText([[You're done!]])
			QA:ClickedBackButton(_, _, _, objectType, entityName)
			break
		end
	end
end

function QA:Tier3DescriptionChanged(sourceWidget, value)
	local inputLength = string.len(EscapeString(value))
	interface:GetWidget('qa_tier_3_label_2'):SetText(inputLength .. '/' .. '1000 max')
	if (inputLength > 4) then
		interface:GetWidget('qa_type_btn_tier_3_4'):SetEnabled(true)
	else
		interface:GetWidget('qa_type_btn_tier_3_4'):SetEnabled(false)
	end
end

function QA:ClickedTier3Button(sourceWidget, sourceName, sourceIndex)
	if 		(sourceIndex == 1) then	-- Works
		CreateLocalFeedback(QA.currentObjectType, QA.currentEntityName, QA.currentChecklistPage, 1, nil)
		QA.currentChecklistPage = QA.currentChecklistPage + 1
		GetNextFreeChecklistPage(QA.currentObjectType, QA.currentEntityName)		
	elseif 	(sourceIndex == 2) then	-- Broken
		interface:GetWidget('qa_tier_3_input_1'):SetVisible(true)
		interface:GetWidget('qa_tier_3_spacer_1'):SetVisible(true)
		interface:UICmd([[ShowOnly('qa_tier_3_buttons_2')]])
	elseif 	(sourceIndex == 3) then -- Skip
		CreateLocalFeedback(QA.currentObjectType, QA.currentEntityName, QA.currentChecklistPage, 0, nil)
		QA.currentChecklistPage = QA.currentChecklistPage + 1
		GetNextFreeChecklistPage(QA.currentObjectType, QA.currentEntityName)
	elseif 	(sourceIndex == 4) then -- Submit
		CreateLocalFeedback(QA.currentObjectType, QA.currentEntityName, QA.currentChecklistPage, 2, EscapeString(interface:GetWidget('qa_user_description'):GetValue()))
		interface:GetWidget('qa_tier_3_input_1'):SetVisible(false)
		interface:GetWidget('qa_tier_3_spacer_1'):SetVisible(false)
		interface:UICmd([[ShowOnly('qa_tier_3_buttons_1')]])	
		QA.currentChecklistPage = QA.currentChecklistPage + 1
		GetNextFreeChecklistPage(QA.currentObjectType, QA.currentEntityName)	
		interface:GetWidget('qa_user_description'):UICmd([[SetInputLine('')]])
	elseif 	(sourceIndex == 5) then	-- Cancel
		interface:GetWidget('qa_tier_3_input_1'):SetVisible(false)
		interface:GetWidget('qa_tier_3_spacer_1'):SetVisible(false)
		interface:UICmd([[ShowOnly('qa_tier_3_buttons_1')]])			
	end
	SetTitleIcon(QA.currentIconPath)
	SetTitleText(QA.currentDisplayName, QA.currentCompleteQs .. '/' .. QA.currentTotalQs)	
end

function QA:ClickedTier2Button(sourceWidget, sourceName, sourceIndex, iconPath, displayName, completeQs, numEntityChecklistTable, entityName, objectType)
	SetTitleIcon(iconPath)
	SetTitleText(displayName, completeQs .. '/' .. numEntityChecklistTable)
	interface:UICmd([[ShowOnly('qa_tier_3')]])
	interface:GetWidget('qa_tier_0_back_btn'):SetVisible(true)
	QA.currentChecklistPage = 1
	QA.currentIconPath = iconPath
	QA.currentEntityName = entityName
	QA.currentDisplayName = displayName
	QA.currentObjectType = objectType
	QA.currentCompleteQs = completeQs
	QA.currentTotalQs = numEntityChecklistTable	
	GetNextFreeChecklistPage(objectType, entityName)
end

local function QAFormSubmissionResult(_, success)
	if AtoB(success) then
		-- Transfer feedback to submitted
		if (CLEAR_FEEDBACK) then
			if (QA_DQ.feedback) then
				for entityType, entityTable in pairs(QA_DQ.feedback) do
					for entityName,entityTable in pairs(entityTable) do
						QA_DQ.submitted[entityType] = QA_DQ.submitted[entityType] or {}
						QA_DQ.submitted[entityType][entityName] = entityTable
					end
				end
				-- Clear feedback
				QA_DQ.feedback = {}
			end
		end
		QA:QAInGameChecklistDisplayed()
	else
		-- Error handler RMM
	end
end
interface:RegisterWatch('QAFormSubmissionResult', QAFormSubmissionResult)

local function QAFormSubmissionStatus(_, status)
	interface:GetWidget('qa_tier_0_throb'):SetVisible(status == '1')
end
interface:RegisterWatch('QAFormSubmissionStatus', QAFormSubmissionStatus)

local function SubmitQAData()	
	local exportClean, exportEmpty, exportIssues = '', '', ''
	local localIssues = ''
	local hasIssues = false
	local hasEmpty = false
	local status, desc
	local totalSubmissions = 0
	local entityTable
	
	for objectType, objectTable in pairs(QA.testobjects) do	
	
		if (QA_DQ.feedback) or (QA_DQ.submitted) then
		
			--println('^r objectType: ' .. tostring(objectType) )
			--println('^r objectTable: ' .. tostring(objectTable) )		
			
			for entityName, entityChecklistTable in pairs(objectTable) do
				
				hasEmpty = false
				hasIssues = false				
				
				-- Object already submitted
				if (QA_DQ.submitted) and (QA_DQ.submitted[objectType]) and  (QA_DQ.submitted[objectType][entityName]) then
					
					entityTable = QA_DQ.submitted[objectType][entityName]
					
				-- Object has feedback
				elseif (QA_DQ.feedback) and (QA_DQ.feedback[objectType]) and  (QA_DQ.feedback[objectType][entityName]) then
					
					entityTable = QA_DQ.feedback[objectType][entityName]
					
					--println('^g entityName: ' .. tostring(entityName) )
					--println('^g entityTable: ' .. tostring(entityTable) )
					
					--printTable(entityTable)

					for checklistItemID, checklistItemTable in ipairs(entityTable) do
						status = checklistItemTable[1]
						desc   = checklistItemTable[2] or ''
						--println(' objectType: ' .. tostring(objectType) .. ' | ' .. ' entityName: ' .. tostring(entityName) .. ' | ' .. ' checklistItemID: ' .. tostring(checklistItemID) .. ' | ' .. ' status: ' .. tostring(status) .. ' | ' .. ' desc: ' .. tostring(desc) )			
						
						localIssues = localIssues .. checklistItemID .. '|' .. status.. '|' .. desc .. '~'			
						
						--println('+^r localIssues: ' .. localIssues)
						
						if (checklistItemTable[1] == 0) then
							hasEmpty = true
						elseif (checklistItemTable[1] == 1) then										
						elseif (checklistItemTable[1] == 2) then
							hasIssues = true
						end	
						
					end
					
					if (hasIssues) then
						if Empty(exportIssues) then
							exportIssues 	= exportIssues 	.. ( objectType .. '_' .. entityName)  .. '~' .. localIssues
						else
							exportIssues 	= exportIssues 	.. ',' .. ( objectType .. '_' .. entityName)  .. '~' .. localIssues
						end
						--println('+^g exportIssues: ' .. exportIssues)
					elseif (hasEmpty) then
						if Empty(exportEmpty) then
							exportEmpty 	= exportEmpty 	.. ( objectType .. '_' .. entityName)
						else
							exportEmpty 	= exportEmpty 	.. ',' .. ( objectType .. '_' .. entityName)
						end
					else
						if Empty(exportClean) then
							exportClean 	= exportClean 	.. ( objectType .. '_' .. entityName)
						else
							exportClean 	= exportClean 	.. ',' .. ( objectType .. '_' .. entityName)
						end			
					end
					localIssues = ''
					
				
				else -- Object is empty
					if Empty(exportEmpty) then
						exportEmpty 	= exportEmpty 	.. ( objectType .. '_' .. entityName)
					else
						exportEmpty 	= exportEmpty 	.. ',' .. ( objectType .. '_' .. entityName)
					end				
				end
				
				totalSubmissions = totalSubmissions + 1
			end
		end	
		
	end
	
	println('cookie: '.. UIGetCookie())
	println('hosttime: '.. HostTime())
	println('clver: '.. QA_DQ.listversion)
	println('dlver: '.. QA_DQ.defversion)
	println('clean: '.. exportClean)
	println('empty: '.. exportEmpty)
	println('issues: '.. exportIssues)
	println('total: '.. totalSubmissions)		
	
	if (SUBMIT_TO_WEB) then	
		interface:GetWidget('qa_tier_1_submit_button_game'):SetVisible(false)
		interface:GetWidget('qa_tier_1_submit_button_main'):SetVisible(false)
		interface:GetWidget('qa_tier_0_throb'):SetVisible(true)	
		SubmitForm('QAChecklistSubmit', 
			'f', 'qa_submit',
			'cookie', UIGetCookie(), 
			'hosttime', HostTime(),
			'clver', QA_DQ.listversion,
			'dlver', QA_DQ.defversion,
			'clean', exportClean,
			'empty', exportEmpty,
			'issues', exportIssues,
			'total', totalSubmissions
		)	
	end

end

function QA:ClickedSubmitButton(sourceWidget, sourceName, sourceIndex)
	if (QA.skipWarn) then
		ShowWarning('qa_partial_warning', SubmitQAData)
	else
		SubmitQAData()
	end		
end

function QA:ClickedTier1Button(sourceWidget, sourceName, sourceIndex)
	interface:UICmd([[ShowOnly('qa_tier_2')]])
	interface:GetWidget('qa_tier_0_back_btn'):SetVisible(true)
	if 	(sourceIndex == 1) then
		ClearTierButtons(2)
		interface:GetWidget('game_qa_mini_panel_parent'):Sleep(1, function() SpawnTier2Buttons('hero') end)
		SetTitleIcon('/heroes/witch_slayer/icon.tga')
		SetTitleText('Choose A Hero')			
	elseif 	(sourceIndex == 2) then
		ClearTierButtons(2)
		interface:GetWidget('game_qa_mini_panel_parent'):Sleep(1, function() SpawnTier2Buttons('avatar') end)
		SetTitleIcon('/heroes/witch_slayer/alt/icon.tga')
		SetTitleText('Choose An Avatar')			
	elseif 	(sourceIndex == 3) then
		ClearTierButtons(2)
		interface:GetWidget('game_qa_mini_panel_parent'):Sleep(1, function() SpawnTier2Buttons('item') end)	
		SetTitleIcon('/items/basic/health_potion/icon.tga')
		SetTitleText('Choose An Item')			
	elseif 	(sourceIndex == 4) then
		ClearTierButtons(2)
		interface:GetWidget('game_qa_mini_panel_parent'):Sleep(1, function() SpawnTier2Buttons('taunt') end)	
		SetTitleIcon('/ui/fe2/store/icons/taunt_dump.tga')
		SetTitleText('Choose A Taunt')			
	elseif 	(sourceIndex == 5) then
		ClearTierButtons(2)
		interface:GetWidget('game_qa_mini_panel_parent'):Sleep(1, function() SpawnTier2Buttons('announcer') end)
		SetTitleIcon('/ui/fe2/store/icons/announcer_hardcore.tga')
		SetTitleText('Choose An Announcer')			
	elseif 	(sourceIndex == 6) then
		ClearTierButtons(2)
		interface:GetWidget('game_qa_mini_panel_parent'):Sleep(1, function() SpawnTier2Buttons('courier') end)		
		SetTitleIcon('/ui/icons/courier_spamming.tga')
		SetTitleText('Choose A Courier')
	elseif 	(sourceIndex == 7) then
		ClearTierButtons(2)
		interface:GetWidget('game_qa_mini_panel_parent'):Sleep(1, function() SpawnTier2Buttons('ui') end)		
		SetTitleIcon('/ui/fe2/store/icons/mulligan.tga')
		SetTitleText('Choose A UI Feature')	
	elseif 	(sourceIndex == 8) then
		ClearTierButtons(2)
		interface:GetWidget('game_qa_mini_panel_parent'):Sleep(1, function() SpawnTier2Buttons('other') end)		
		SetTitleIcon('/ui/fe2/store/icons/bundle.tga')
		SetTitleText('Choose A Category')		

	elseif 	(sourceIndex == 101) then
		ClearTierButtons(2)
		interface:GetWidget('game_qa_mini_panel_parent'):Sleep(1, function() SpawnTier2Buttons('icon') end)
		SetTitleIcon('/ui/fe2/store/icons/custom_icon.tga')
		SetTitleText('Choose An Icon')			
	elseif 	(sourceIndex == 102) then
		ClearTierButtons(2)
		interface:GetWidget('game_qa_mini_panel_parent'):Sleep(1, function() SpawnTier2Buttons('symbol') end)
		SetTitleIcon('/ui/fe2/store/icons/custom_icon.tga')
		SetTitleText('Choose A Symbol')	
	elseif 	(sourceIndex == 103) then
		ClearTierButtons(2)
		interface:GetWidget('game_qa_mini_panel_parent'):Sleep(1, function() SpawnTier2Buttons('namecolor') end)
		SetTitleIcon('/ui/icons/emerald.tga')
		SetTitleText('Choose A Name Color')	
	elseif 	(sourceIndex == 104) then
		interface:GetWidget('game_qa_mini_panel_parent'):Sleep(1, function() SpawnTier2Buttons('store') end)
		SetTitleIcon('/ui/fe2/mainmenu/icons/N_store.tga')
		SetTitleText('Choose An Item')	
	elseif 	(sourceIndex == 105) then
		interface:GetWidget('game_qa_mini_panel_parent'):Sleep(1, function() SpawnTier2Buttons('chatserver') end)
		SetTitleIcon('/ui/fe2/systembar/icons/ims.tga')
		SetTitleText('Choose An Item')	
	elseif 	(sourceIndex == 106) then
		interface:GetWidget('game_qa_mini_panel_parent'):Sleep(1, function() SpawnTier2Buttons('matchmaking') end)
		SetTitleIcon('/ui/fe2/mainmenu/icons/N_matchmaking.tga')
		SetTitleText('Choose An Item')		
	elseif 	(sourceIndex == 107) then
		ClearTierButtons(2)
		interface:GetWidget('game_qa_mini_panel_parent'):Sleep(1, function() SpawnTier2Buttons('ui2') end)
		SetTitleIcon('/ui/fe2/store/icons/mulligan.tga')
		SetTitleText('Choose A Feature')			
	elseif 	(sourceIndex == 108) then
		ClearTierButtons(2)
		interface:GetWidget('game_qa_mini_panel_parent'):Sleep(1, function() SpawnTier2Buttons('other2') end)
		SetTitleIcon('/ui/fe2/store/icons/bundle.tga')
		SetTitleText('Choose An Item')			
		
		
	end
end

local function EnableTier1Button(buttonWidget, entityType)
	local button = UIManager.GetInterface('game_qa'):GetWidget(buttonWidget)
	--local button_submitted = interface:GetWidget(buttonWidget..'_submitted_label')
	--local button_complete = interface:GetWidget(buttonWidget..'_complete_label')
	
	local isSubmitted = false
	local requiresSubmit = false
	local numObjects, numSubmittedObjects, numObjectsWithFeedback = 0, 0, 0
	local skipped = false
	
	for i,v in pairs(QA.testobjects[entityType]) do
		numObjects = numObjects + 1
	end
	
	if (QA_DQ.submitted) and (QA_DQ.submitted[entityType]) then
		for i,v in pairs(QA_DQ.submitted[entityType]) do
			numSubmittedObjects = numSubmittedObjects + 1
		end
		if (numSubmittedObjects >= numObjects) then
			isSubmitted = true
		else
			isSubmitted = false
		end
	end
	
	if (QA_DQ.feedback) and (QA_DQ.feedback[entityType]) then
		for i,v in pairs(QA_DQ.feedback[entityType]) do		
			skipped = false
			for clIndex,clTable in pairs(v) do
				if (clTable[1]) then
					if (clTable[1] == 0) then
						QA.skipWarn = true
						skipped = true
						break
					end
				end
			end	
			if (not skipped) then
				numObjectsWithFeedback = numObjectsWithFeedback + 1
			end
		end
		if (numObjectsWithFeedback > 0) then
			requiresSubmit = true
			interface:GetWidget('qa_tier_1_submit_button_game'):SetVisible(true)
			interface:GetWidget('qa_tier_1_submit_button_main'):SetVisible(true)
		else
			requiresSubmit = false
		end
	end	
	
	--button_submitted:SetVisible(false)
	--button_complete:SetVisible(false)
	button:SetEnabled(false)
	
	if (numObjects > 0) then
		if (numObjects > numObjectsWithFeedback) then
			if (isSubmitted) then			
				--button_submitted:SetVisible(true)
				return false
			else
				button:SetEnabled(true)
				return true
			end		
		else
			--button_complete:SetVisible(true)
			return false
		end
	else
		return false
	end
	
end

function QA:QAInGameChecklistLoad()	

	GetWidget('sysbar_qa_checklist_button'):Sleep(1, function()

		local enableQAHighlight = false
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_1', 'hero') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_2', 'avatar') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_3', 'item') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_4', 'taunt') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_5', 'announcer') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_6', 'courier') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_7', 'ui') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_8', 'other') or enableQAHighlight
		
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_101', 'icon') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_102', 'symbol') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_103', 'namecolor') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_104', 'store') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_105', 'chatserver') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_106', 'matchmaking') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_107', 'ui2') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_108', 'other2')	 or enableQAHighlight
		
		GetWidget('sysbar_qa_checklist_button'):SetVisible(enableQAHighlight or false)	
		
		if (enableQAHighlight) and GetWidget('main_widget_highlighter', nil, true) then
			GetWidget('sysbar_qa_checklist_button'):Sleep(8000, function()
				if (QA.firstload) then
					UIManager.GetInterface('main'):HoNMainF('HighlightWidget', nil, 'sysbar_qa_checklist_button', true, false, true, 'QA Checklist updated')
				end
			end)
		end
	end)
end

function QA:QAInGameChecklistDisplayed()	
	if (not interface:GetWidget('qa_tier_1_'..UIManager.GetActiveInterface():GetName()):IsVisible()) then
		interface:UICmd([[ShowOnly('qa_tier_1_]]..UIManager.GetActiveInterface():GetName()..[[')]])
	else
		SetTitleIcon('/ui/icons/item_destruction.tga')
		SetTitleText('Select A Category')
		interface:GetWidget('qa_tier_1_submit_button_game'):SetVisible(false)
		interface:GetWidget('qa_tier_1_submit_button_main'):SetVisible(false)
		QA.skipWarn = false
		local enableQAHighlight = false
		
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_1', 'hero') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_2', 'avatar') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_3', 'item') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_4', 'taunt') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_5', 'announcer') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_6', 'courier') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_7', 'ui') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_8', 'other') or enableQAHighlight
		
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_101', 'icon') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_102', 'symbol') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_103', 'namecolor') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_104', 'store') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_105', 'chatserver') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_106', 'matchmaking') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_107', 'ui2') or enableQAHighlight
		enableQAHighlight = EnableTier1Button('qa_type_btn_tier_1_108', 'other2')	 or enableQAHighlight
		
		GetWidget('sysbar_qa_checklist_button'):SetVisible(enableQAHighlight or false)
		
		if (QA.firstload) then
			QA.firstload = false
			ShowWarning('qa_firstload')
		end
	end
	interface:UICmd("Trigger('DoWalkthrough', '', '')")
end

interface:RegisterWatch('GamePhase', function() QA:QAInGameChecklistDisplayed() end)

function QA:ResetDB(sourceWidget, sourceName, sourceIndex)	
	println('^yQA: Resetting DB')
	QA_DQ.defversion = QA.DEFAULT_CHECKLIST_VERSION
	QA_DQ.listversion = QA.CURRENT_TEST_ITEM_LIST_VERSION
	QA_DQ.feedback = {}
	QA_DQ.submitted = {}
	QA_DQ:Flush()
	QA:QAInGameChecklistDisplayed()	
end