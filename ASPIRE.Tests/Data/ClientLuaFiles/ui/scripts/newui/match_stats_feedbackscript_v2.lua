----------------------------------------------------------
--	Name: 		Feedback UI Script			            --
--  Copyright 2018 Frostburn Studios					--
----------------------------------------------------------

--Current State Question
PBT_State = 0

--Answer Table
PBT_Answers = {}

--Debug, different numbers for debug level
PBT_Debug = 0

--Current form type
PBT_FormType = ''

function PBTFeedback_TestFunc1()

	PBT_Answers["kv_1"] = 'hey'
	PBT_Answers["kv_2"] = 'yesss'
	PBT_Answers["kv_3"] = 'This is a sentence OPmg OGMsogmsgs'
	PBT_Answers["kv_4"] = 'no'

end

function PBTFeedback_TestFunc2()

	local allString = '|'

	for i=1,#PBT_Answers do
		allString = tostring(allString .. PBT_Answers[i] .. '|')
	end
	
	Echo(allString)
	
end

function PBTFeedback_TestFunc3()

	table.remove(PBT_Answers)

end

function PBTFeedback_TestFunc4()

	local editFrame = GetWidget('rewardstats_report_feedback_editbox')
	
	Echo(editFrame:GetWrap())

end

--Retarded onframe debug thing so i can track a number in game this shud be deleted before submitting to sbt if ur reading this tell hyper 
function PBTFeedback_ScanState(self)
	self:SetText('state: [' .. tostring(PBT_State) .. ']')
end

function PBTFeedback_PanelShowHide(param)
	
	--Triggered when opened
	if param == 'onshowlua' then
		PBTFeedback_StartPoll()
	elseif param == 'onhidelua' then
		PBT_State = 0
		PBT_FormType = ''
		PBT_Answers = {}
	else
		--nil
	end
	
end

function PBTFeedback_ResetWidgets()
	local checkboxPanel = GetWidget('rewardstat_report_feedback_ptm_checkboxtemplate')
	local formPanel = GetWidget('rewardstat_report_feedback_ptm_formtemplate')
	
	--Hides both template panels
	checkboxPanel:SetVisible(0)
	formPanel:SetVisible(0)
	
	--Resetting next/end button
	GetWidget('rewardstats_feedback_next'):SetVisible(1)
	GetWidget('rewardstats_feedback_end'):SetVisible(0)
	GetWidget('rewardstats_feedback_next'):SetEnabled(0)
	GetWidget('rewardstats_feedback_opt1'):SetButtonState(0)
	GetWidget('rewardstats_feedback_opt2'):SetButtonState(0)
end

function PBTFeedback_StartPoll()

	--resets stuff
	PBTFeedback_ResetWidgets()
	
	--clear table
	PBT_Answers = {}

	--Sets state to first question and executes it
	PBT_State = 1
	PBTFeedback_QuestionExecute(1)

end

function PBTFeedback_DebugPrintFull()
	
	if PBT_Debug >= 1 then
		local allString = '|'

		for i=1,#PBT_Answers do
			allString = tostring(allString .. PBT_Answers[i] .. '|')
		end
		
		Echo(allString)
	end

end

function PBTFeedback_End()
	
	--grabbing matchid
	local mID = tostring(matchStats_match_id)
	
	--Adds the final answer to the table
	PBT_Answers["kv_" .. (PBT_State+1)] = tostring(PBTFeedback_CheckTemplateData())
	
	--prints out all answers in 1 string with | separators, debug only.
	PBTFeedback_DebugPrintFull()
	
	--adds matchid to table, and gets the processing visual on the screen
	PBT_Answers["match_id"] = mID
	GetWidget('rewardstat_operation_mask'):SetVisible(1)
	Match_Stats_MasteryInfo_masktimer = GetTime()
	
	--BOOM send feedback!
	SendRebornFeedback(PBT_Answers)
	
	--clears the table since it has being sent
	PBT_Answers = {}
	
	--ends the form
	GetWidget('rewardstat_report_feedback_ptm_panel'):FadeOut(300)
	
end

function PBTFeedback_CheckTemplateData()

	if GetWidget('rewardstat_report_feedback_ptm_checkboxtemplate'):IsVisible() then
		
		local chkBox1 = GetWidget('rewardstats_feedback_opt1')
		local chkBox2 = GetWidget('rewardstats_feedback_opt2')
		
		if chkBox1:GetButtonState() == 1 then
			return 'yes'
		elseif chkBox2:GetButtonState() == 1 then
			return 'no'
		else
			if PBT_Debug >= 1 then
				Echo('PBTFeedback Critical Error! [Function: PBTFeedback_CheckTemplateData() Unknown data!! MAYBE user was able to press next without selecting option?]')
			end
			return 'unknownData'
		end
		
	elseif GetWidget('rewardstat_report_feedback_ptm_formtemplate'):IsVisible() then
		
		local editFormBox = GetWidget('rewardstats_report_feedback_editbox')
		local formData = editFormBox:GetInputLine()
		
		return tostring(formData)	
	elseif GetWidget('rewardstat_report_feedback_ptm_starratingtemplate'):IsVisible() then
		
		local formData = GetCvarString('_rewardstat_star')
		
		return tostring(formData)
	else
		if PBT_Debug >= 1 then
			Echo('PBTFeedback Critical Error! [Function: PBTFeedback_CheckTemplateData() Unknown template!!]')
		end
	end

end

function PBTFeedback_NextQ(back)
	
	--Increments global state cvar and executes next question, unless its back
	if (back) then
		PBT_State = tonumber(PBT_State - 1)
	else
		--Adds answer to the table
		PBT_State = tonumber(PBT_State + 1)
		PBT_Answers["kv_" .. PBT_State] = tostring(PBTFeedback_CheckTemplateData())
	end
	
	--Clears combobox options and locks out next again
	GetWidget('rewardstats_feedback_next'):SetEnabled(0)
	GetWidget('rewardstats_feedback_opt1'):SetButtonState(0)
	GetWidget('rewardstats_feedback_opt2'):SetButtonState(0)
	
	--resets starRating
	PBTFeedback_StarRating('reset')
	
	PBTFeedback_QuestionExecute(PBT_State)
	
	local numOfQ = tonumber(PBTFeedback_QuestionList(1, 'NUMQ'))
	
	if PBT_Debug >= 2 then
		Echo('numOfQ['..numOfQ..']'..'PBT_State['..PBT_State..']')
	end
	
	if tonumber(PBT_State) >= tonumber(numOfQ) then
		GetWidget('rewardstats_feedback_next'):SetVisible(0)
		GetWidget('rewardstats_feedback_end'):SetVisible(1)
	else
		GetWidget('rewardstats_feedback_next'):SetVisible(1)
		GetWidget('rewardstats_feedback_end'):SetVisible(0)
	end
	
	PBTFeedback_DebugPrintFull()
	
end

function PBTFeedback_QuestionList(param, req)

	--Questions here, copy paste a line and change question type to make a new question, gets the question string from stringtables (TODO)
	--templates: [checkbox] - [form500] - [form100] - [starRating]
	local pbtQuestionTable = {
		{QTYPE 	= 'starRating', 	QNAME = Translate('rewardstat_feedback_Q1')},
		{QTYPE 	= 'form500', 		QNAME = Translate('rewardstat_feedback_Q2')},
		{QTYPE 	= 'starRating', 	QNAME = Translate('rewardstat_feedback_Q3')},
		{QTYPE 	= 'checkbox', 		QNAME = Translate('rewardstat_feedback_Q4')},
	}
	
	if req == 'NUMQ' then
		return tonumber(#pbtQuestionTable)
	end
	
	if req == 'ptbdebug_print_question_strings' then
		Echo('[' .. pbtQuestionTable[param].QTYPE .. '] -- [' .. pbtQuestionTable[param].QNAME .. ']')
	end
	
	if req == 'QTYPE' then
		return tostring(pbtQuestionTable[param].QTYPE)
	end
	
	if req == 'QNAME' then
		return tostring(pbtQuestionTable[param].QNAME)
	end
	
	if PBT_Debug >= 1 then
		Echo('PBTFeedback Critical Error! [Function: PBTFeedback_QuestionList could not determine anything to return.]')
	end

end

function PBTFeedback_QuestionExecute(question, back)
	
	--Master Template Panels
	local checkboxPanel = GetWidget('rewardstat_report_feedback_ptm_checkboxtemplate')
	local starRatingPanel = GetWidget('rewardstat_report_feedback_ptm_starratingtemplate')
	local formPanel = GetWidget('rewardstat_report_feedback_ptm_formtemplate')
	local checkboxLabelQ = GetWidget('rewardstat_feedback_chkboxtemplatequestion')
	local starRatingLabelQ = GetWidget('rewardstat_feedback_starratingtemplatequestion')
	local formLabelQ = GetWidget('rewardstat_feedback_formtemplatequestion')
	local mainFrame = GetWidget('rewardstat_report_feedback_mainframe')
	local backBtn = GetWidget('rewardstats_feedback_back')
	local formFrame = GetWidget('rewardstats_report_feedback_panel')
	local editBox = GetWidget('rewardstats_report_feedback_editbox')
	local charLabel = GetWidget('rewardstat_charlimit_widgetlabel')
	
	--Sets type of question layout visible
	if PBTFeedback_QuestionList(question, 'QTYPE') == tostring('checkbox') then
		formPanel:SetVisible(0)
		checkboxPanel:SetVisible(1)
		starRatingPanel:SetVisible(0)
		
		--sets height of main panel
		mainFrame:SetHeight('27h')
		
		--change form type global for char limit or other things
		PBT_FormType = 'starRating'
		
		--Set strings/widget stuff
		checkboxLabelQ:SetText(PBTFeedback_QuestionList(question, 'QNAME'))
	elseif PBTFeedback_QuestionList(question, 'QTYPE') == tostring('starRating') then
		formPanel:SetVisible(0)
		checkboxPanel:SetVisible(0)
		starRatingPanel:SetVisible(1)
		
		--sets height of main panel
		mainFrame:SetHeight('27h')
		
		--change form type global for char limit or other things
		PBT_FormType = 'checkbox'
		
		--Set strings/widget stuff
		starRatingLabelQ:SetText(PBTFeedback_QuestionList(question, 'QNAME'))
	elseif PBTFeedback_QuestionList(question, 'QTYPE') == tostring('form500') then
		formPanel:SetVisible(1)
		checkboxPanel:SetVisible(0)
		starRatingPanel:SetVisible(0)
		
		--clears editbox
		editBox:SetInputLine('')
		
		--sets height of main panel
		mainFrame:SetHeight('47h')
		
		--sets height of edit frame
		formFrame:SetHeight('24h')
		
		--change form type global for char limit or other things
		PBT_FormType = 'form500'
		
		--character limit
		if GetCvarBool('cl_GarenaEnable') then
			editBox:SetMaxLength(1000)
			charLabel:SetText(Translate('rewardstat_charlimit') .. ' 1000')
		else
			editBox:SetMaxLength(500)
			charLabel:SetText(Translate('rewardstat_charlimit') .. '500')
		end
		
		--Set strings/widget stuff
		formLabelQ:SetText(PBTFeedback_QuestionList(question, 'QNAME'))
	elseif PBTFeedback_QuestionList(question, 'QTYPE') == tostring('form100') then
		formPanel:SetVisible(1)
		checkboxPanel:SetVisible(0)
		starRatingPanel:SetVisible(0)
		
		--clears editbox
		editBox:SetInputLine('')
		
		--sets height of frame for form
		mainFrame:SetHeight('27h')
		
		--sets height of edit frame
		formFrame:SetHeight('5.4h')
		
		--change form type global for char limit or other things
		PBT_FormType = 'form100'
		
		--character limit
		if GetCvarBool('cl_GarenaEnable') then
			editBox:SetMaxLength(200)
			charLabel:SetText(Translate('rewardstat_charlimit') .. ' 200')
		else
			editBox:SetMaxLength(100)
			charLabel:SetText(Translate('rewardstat_charlimit') .. ' 100')
		end
		
		--Set strings/widget stuff
		formLabelQ:SetText(PBTFeedback_QuestionList(question, 'QNAME'))
	else
		if PBT_Debug >= 1 then
			Echo('PBTFeedback Critical Error! could not detect question type.')
		end
	end
	
	if PBT_State > 1 then
		backBtn:SetEnabled(1)
	else
		backBtn:SetEnabled(0)
	end
	

end

function PBTFeedback_StarRating(param)

	if tostring(param) ~= 'reset' then
		--Resets stars to off instantly, this is never visually seen as the next frame the stars are turned on.
		for i=1,5 do
			GetWidget('rewardstat_ptm_star_' .. i):SetTexture('/ui/fe2/NewUI/Res/match_stats/star_feedback_off.png')
		end
		
		
		matchStats_starSelect = true
		matchStats_starRating = tonumber(param)
		
		--Sets the star rating (from 1 to 5 stars)
		for i=1,param do
			GetWidget('rewardstat_ptm_star_' .. i):SetTexture('/ui/fe2/NewUI/Res/match_stats/star_feedback_on.png')
		end
		
		--Sets cvar (spawned from the package file onload="")
		Set('_rewardstat_star', param)
		
		--enables next
		GetWidget('rewardstats_feedback_next'):SetEnabled(1)
		
	else
		for i=1,5 do
			--If resetting, rating is set to 0 stars, requiring a selection before submission
			GetWidget('rewardstat_ptm_star_' .. i):SetTexture('/ui/fe2/NewUI/Res/match_stats/star_feedback_off.png')
			matchStats_starSelect = false
		end
	end

end

--function RewardStat_SubmitPTMFeedback(param1, param2, param3, param4, param5, param6, param7)
--
--	local fmatchID = tonumber(param1)
--	local fbackstring = tostring(EscapeString(param2))
--	local starRating = tostring(param3)
--	local fTypeHeroes = tostring(param4)
--	local fTypeItems = tostring(param5)
--	local fTypeMaps = tostring(param6)
--	local fTypeOther = tostring(param7)
--	
--	SendRebornFeedback(fmatchID, EscapeString(fbackstring), starRating, fTypeHeroes, fTypeItems, fTypeMaps, fTypeOther)
--
--end



































