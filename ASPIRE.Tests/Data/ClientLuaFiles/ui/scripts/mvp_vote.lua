-- mvp vote stuff
local interface, interfaceName = object, object:GetName()

local container = interface:GetWidget('mvp_vote')

local candidates = {}

local cdd_voted = nil

local bVoteEnabled = false

local function GetAwardName(award_type)
	local award = tonumber(award_type)
	if award == 0 then
		return 'award_mann'			
	elseif award == 1 then          
		return 'award_mqk'          
	elseif award == 2 then          
		return 'award_lgks'         
	elseif award == 3 then          
		return 'award_msd'          
	elseif award == 4 then          
		return 'award_mkill'        
	elseif award == 5 then          
		return 'award_masst'        
	elseif award == 6 then          
		return 'award_ledth'
	elseif award == 7 then
		return 'award_mbdmg'
	elseif award == 8 then
		return 'award_mwk'
	elseif award == 9 then
		return 'award_mhdd' 
	elseif award == 10 then
		return 'award_hcs'
	else
		return ''
	end
end

local function MVPVote(cdd_index)
	println('^c[MVP] ^*MVPVote Called. Index: ' .. tostring(cdd_index))
	if candidates[cdd_index] then
		interface:UICmd("MVPVote("..candidates[cdd_index]['ClientNum']..");")
	end
end

local function MVPVoteCountDown(new_cd)
	local new_cd_num = tonumber(new_cd)
	if (new_cd_num == nil) or (new_cd_num < 0) then
		println('^rError === MVP Vote Invalid counting down value: '..new_cd..' ===')
		return 
	end
	
	local cd_total = GetCvarNumber('g_MVPVoteCountDownTimer')
	if new_cd_num > cd_total then 
		new_cd_num = cd_total 
	end
	
	local pbar_widget = container:GetWidget('simple_progbar_countdown')
	if (pbar_widget == nil) then return end
	
	pbar_widget:GetWidget('simple_progbar_body_countdown'):SetWidth(tostring(new_cd_num * 100 / cd_total)..'%')
end

local function ResetScreen()
	if (container == nil) then return end
	
	local body = container:GetWidget('mvp_vote_body')
    local showButtonBody = interface:GetWidget('game_top_clock')
	if (body == nil or showButtonBody == nil) then return end

	body:GetWidget('mvp_vote_close_btn'):SetVisible(1)
	body:GetWidget('mvp_vote_close_btn'):SetCallback('onclick', 
		function(self)
			PlaySound('/shared/sounds/ui/ccpanel/button_close_02.wav')
			container:SetVisible(0)
            showButtonBody:GetWidget('mvp_vote_show_button'):SetVisible(true)
		end)
	body:GetWidget('mvp_vote_close_btn'):SetCallback('onmouseover', 
		function(self)
			PlaySound('/shared/sounds/ui/ccpanel/button_over_01.wav')
		end)
    showButtonBody:GetWidget('mvp_vote_show_button'):SetCallback('onclick', 
		function(self)
            PlaySound('/shared/sounds/ui/ccpanel/button_close_02.wav')
            showButtonBody:GetWidget('mvp_vote_show_button'):SetVisible(false)
			container:FadeIn(150)
		end)
	showButtonBody:GetWidget('mvp_vote_show_button'):SetCallback('onmouseover', 
		function(self)
			PlaySound('/shared/sounds/ui/ccpanel/button_over_01.wav')
		end)
    
	for i, _ in ipairs(candidates) do
		local cdd_widget = body:GetWidget('candidate'..tostring(i))
		if (cdd_widget) then
			cdd_widget:SetVisible(false)
			cdd_widget:Destroy()
		end
	end

	bVoteEnabled = false
	
	MVPVoteCountDown('0')
end

local function MVPVoteBegin(widget, ...)
	println('^c[MVP] ^*MVPVoteBegin Triggered.')
	println('^c================== MVP Vote Begin ==================')
	
	ResetScreen()
	
	candidates = {}
	cdd_voted = nil
	
	local body = container:GetWidget('mvp_vote_body')
	if (body == nil) then return end
	
	local cdd_num = tonumber(arg['n']) - 1
	local padding = 7
	local width = 20
	local height = 58
	local scaleup_w = 20.5
	local scaleup_h = scaleup_w / width * height
	local pos_x = ''
	local pos_y = '1%'
	local labelcolor = '#384c42'

	local interval = 0
	local cvar_MVPVoteMaxNumCandidates = GetCvarNumber('g_MVPVoteMaxNumCandidates')
	
	bVoteEnabled = arg[cdd_num + 1]
	
	if cdd_num > cvar_MVPVoteMaxNumCandidates then
		println('Warning! === too many MVP Candidates! ===')
		cdd_num = cvar_MVPVoteMaxNumCandidates
	end
	
	if cdd_num == cvar_MVPVoteMaxNumCandidates then
		interval = (100 - padding * 2 - width * cdd_num) / (cdd_num - 1)
	elseif cdd_num < cvar_MVPVoteMaxNumCandidates then
		interval = (100 - padding * 2 - width * cdd_num) / (cdd_num + 1)
		padding = padding + interval
	end

	for i=1, cdd_num, 1 do
		local _, _, client_num, player_name, hero_name, avatar_name, award_type, award_score = string.find(arg[i], "(.+),(.+),(.+),(.+),(.+),(.+)")	
		candidates[i] = {}
		candidates[i]['ClientNum'] 	= client_num
		candidates[i]['PlayerName'] = player_name
		candidates[i]['HeroName'] 	= hero_name
		candidates[i]['AvatarName'] = avatar_name
		candidates[i]['AwardType'] 	= award_type
		candidates[i]['AwardScore'] = award_score
		
		pos_x = tostring(padding + width/2 + (i - 1) * (width + interval) - 50)..'%'
		body:Instantiate('mvp_vote_candidate', 	'id',				tostring(i), 
												'x',				pos_x,
												'y',				pos_y,
												'width',			tostring(width)..'%',
												'height',			tostring(height)..'%',
												'scaleup_w',		tostring(scaleup_w)..'%',
												'scaleup_h',		tostring(scaleup_h)..'%',
												'cdd_player',		player_name,
												'cdd_hero',			hero_name,
												'cdd_award_score',	award_score,
												'cdd_award_name',	GetAwardName(tonumber(award_type)),
												'award_texture',	'ui/mvpvoting/awards/award'..tostring(award_type)..'.png'
						)
		
		local vote_btn = interface:GetWidget('candidate'..tostring(i))
		if (vote_btn) then
			if bVoteEnabled == 'true' then
				vote_btn:SetCallback('onclick', function(self) 
													PlaySound('/shared/sounds/ui/revamp/mvp_vote.ogg')
													MVPVote(i)
													cdd_voted = i
													for index = 1, cdd_num, 1 do
														local btn = container:GetWidget('candidate'..tostring(index))
														if btn then 
															btn:ClearCallback('onclick') 
														end
														if cdd_voted ~= index then
															self:GetWidget('thumbup_img'..tostring(index)):SetTexture('/ui/mvpvoting/thumbup_disabled.png')
															container:GetWidget('mvp_selected_effect'..tostring(index)):SetVisible(false)
														else
															self:GetWidget('thumbup_img'..tostring(index)):SetTexture('/ui/mvpvoting/thumbup.png')

															local wdgEffect = container:GetWidget('mvp_selected_effect'..tostring(index))
															--wdgEffect:Sleep(150, function() wdgEffect:SetVisible(true) PlaySound('/shared/sounds/ui/revamp/mvp_light.ogg') end)
														end
													end
												end)
			else
				vote_btn:ClearCallback('onclick')
				vote_btn:ClearCallback('onmouseover')
				vote_btn:ClearCallback('onmouseout')
			end
			
			model = vote_btn:GetWidget('mvp_hero_avatar'..tostring(i))
			if model then
				model:SetModel(GetHeroStoreModelPathFromProduct(avatar_name))
				model:SetEffect(GetHeroStorePassiveEffectPathFromProduct(avatar_name))
				model:SetModelAngles(GetHeroStoreAnglesFromProduct(avatar_name))
				model:SetModelScale(GetHeroStoreScaleFromProduct(avatar_name))
				model:SetModelPos(GetHeroStorePosFromProduct(avatar_name))
				model:SetAnim('idle')
			end
		end
		
	end

	MVPVoteCountDown('0')
	
	if container then
		container:SetVisible(true)
	end
end

local function MVPVoteResult(widget, ...)
	println('^c================== MVP Vote Result ==================')
	println('HasMVP: '..arg[1])
	
	local cdd_num = table.getn(candidates)

	for i=1, cdd_num, 1 do
		local btn = container:GetWidget('candidate'..tostring(i))
		if btn then 
			btn:ClearCallback('onclick') 
			btn:ClearCallback('onmouseover') 
			btn:ClearCallback('onmouseout') 
			btn:GetWidget('cdd_bg'..tostring(i)):SetTexture('/ui/mvpvoting/bg02.png')
		end
	end

	if arg[1] == 'true' then
		println('MVP is '..arg[3])
		for i = 1, cdd_num, 1 do
			if (arg[2] == candidates[i]['ClientNum']) then
				container:GetWidget('cdd_bg'..tostring(i)):SetTexture('/ui/mvpvoting/bg03.png')
				container:GetWidget('mvp_effect'..tostring(i)):SetVisible(true)
				PlaySound('/shared/sounds/ui/revamp/mvp_elect.ogg')
				break
			end
		end
	else
	end
end

local function MVPVoteStatus(widget, ...)
	local cdd_num = table.getn(candidates)
	if arg['n'] <= cdd_num then return end
	
	--println('MVPStatus: '..tostring(arg['n'])..' | '..tostring(cdd_num))
	for i=1, cdd_num, 1 do
		local _, _, client_num, votes_owned = string.find(arg[i], "(.+),(.+)")
		if candidates[i]['ClientNum'] == client_num then
			candidates[i]['VotesOwned'] = votes_owned
		else
			candidates[i]['VotesOwned'] = '-1'
		end
		
		--println(tostring(i)..' | '..arg[i])
		--println(client_num..' | '..votes_owned..' | '..candidates[i]['ClientNum']..' | '..candidates[i]['VotesOwned'])
		local label_votes_owned = container:GetWidget('thumbup_owned'..tostring(i))
		if label_votes_owned then
			label_votes_owned:SetText(candidates[i]['VotesOwned'])
		end
	end
	
	MVPVoteCountDown(arg[cdd_num + 1])
	
	--println('Potential MVP is '..arg[cdd_num + 2])
end

local function HideContainer()	
	ResetScreen()
	container:FadeOut(150)
end

local function OnGamePhase(self, gamePhase)
	--Echo('^gmvp_vote:OnGamePhase, phase = ^*'..gamePhase)
	if (tonumber(gamePhase) == 5 or tonumber(gamePhase) == 6) and container then
		--Echo('^gIn game now, hiding mvp_vote!^*')
		ResetScreen()
		container:SetVisible(0)
	end
end

local function MVPVoteRegister()
	if container then
		container:RegisterWatch('MVPVoteBegin', MVPVoteBegin)
		container:RegisterWatch('MVPVoteStatus', MVPVoteStatus)
		container:RegisterWatch('MVPVoteResult', MVPVoteResult)
		container:RegisterWatch('MVPVoteEnd', HideContainer)
		container:RegisterWatch('GamePhase', OnGamePhase)
	end
end

MVPVoteRegister()

--[[
function Debug_MVPVoteSimMVPLaureate(slot)
	local ctnr = interface:GetWidget('mvp_vote')
	for i=1, 4, 1 do
		ctnr:GetWidget('candidate'..tostring(i)):ClearCallback('onclick')
		ctnr:GetWidget('candidate'..tostring(i)):ClearCallback('onmouseover')
		ctnr:GetWidget('candidate'..tostring(i)):ClearCallback('onmouseout')
		ctnr:GetWidget('thumbup_img'..tostring(i)):SetTexture('/ui/mvpvoting/thumbup_disabled.png')
		ctnr:GetWidget('cdd_bg'..tostring(i)):SetTexture('/ui/mvpvoting/bg02.png')
	end

	ctnr:GetWidget('cdd_bg'..tostring(slot)):SetTexture('/ui/mvpvoting/bg03.png')
	ctnr:GetWidget('mvp_effect'..tostring(slot)):SetVisible(1)
	ctnr:GetWidget('mvp_selected_effect'..tostring(slot)):SetVisible(1)
	ctnr:GetWidget('thumbup_img'..tostring(slot)):SetTexture('/ui/mvpvoting/thumbup.png')
end
--]]