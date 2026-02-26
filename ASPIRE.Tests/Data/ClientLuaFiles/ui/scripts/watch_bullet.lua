-- defines
local Bullet_Comment_Padding = 2
local Bullet_Comment_Margin = 10
local Bullet_Comment_Font = "dyn_24" 			-- comments display size
local Bullet_Comment_Max_Pending_Count = 100	-- comments more than this will be discarded
local Bullet_Comment_Max_Pending_Delay = 3000	-- comments delayed more than this will be discarded
local Bullet_Comment_Overtake_Ratio = 0.5		-- distance/time rate for a long comment to overtake a short one
local Bullet_Comment_Max_Frame_Delta = 1000

--line setting for officail game
Set('bullet_comment_line_count_official', '9', 'int', true)			-- override line count
Set('bullet_comment_line_start_official', '1', 'float', true)		-- margin between first line and top of window(in persent)
Set('bullet_comment_line_height_official','7', 'float', true)		-- line height
Set('bullet_comment_line_gap_official', '2','float', true)			-- line gap

--line setting for other
Set('bullet_comment_line_count', '9', 'int', true)	
Set('bullet_comment_line_start', '1', 'float', true)
Set('bullet_comment_line_height','7', 'float', true)
Set('bullet_comment_line_gap', '2','float', true)

Set('bullet_comment_jam_check_time', '5000', 'float', true)			-- count discarded comments within this time 
Set('bullet_comment_jam_check_count', '5', 'int', true)				-- discarded comments count threshold to enter jam state
Set('bullet_comment_jam_spawn_time', '5000', 'float', true)			-- time to spawn all pending comments when entered jam state

local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, format, tostring, tonumber, tsort, ceil, floor, sub, find, gfind = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort, _G.math.ceil, _G.math.floor, _G.string.sub, _G.string.find, _G.string.gfind
local interface, interfaceName = object, object:GetName()

WatchBullet = WatchBullet or {}
WatchBullet.matchId = 0;
WatchBullet.inited = false;
WatchBullet.pendingComments = {}
WatchBullet.pendingCommentsCount = 0
WatchBullet.freePanels = {}
WatchBullet.RegionWidth = 0
WatchBullet.NameSuffix = 0
WatchBullet.enabled = true
WatchBullet.chatOriginalMaxLength = -1
WatchBullet.frameDeltaSum = 0
WatchBullet.frameDeltaCount = 0
WatchBullet.lineCount = 9
WatchBullet.lineStart = 1
WatchBullet.lineHeight = 7
WatchBullet.lineGap = 2
WatchBullet.lines = {
	{lineNumber = 1, lifeTime = 12000, preferLength = 60},
	{lineNumber = 2, lifeTime = 12000, preferLength = 60},
	{lineNumber = 3, lifeTime = 12000, preferLength = 50},
	{lineNumber = 4, lifeTime = 12000, preferLength = 40},
	{lineNumber = 5, lifeTime = 12000, preferLength = 0},
	{lineNumber = 6, lifeTime = 12000, preferLength = 0},
	{lineNumber = 7, lifeTime = 12000, preferLength = 0},
	{lineNumber = 8, lifeTime = 12000, preferLength = 0},
	{lineNumber = 9, lifeTime = 12000, preferLength = 0},
}
WatchBullet.jams = {}
WatchBullet.jamsCount = 0
WatchBullet.jamming = 0
WatchBullet.jamTime = 0

local activeChatbox = nil

-- common
local function GetBulletPanel()
	local widget = interface:GetWidget('watch_bullets_bg')
	if widget == nil then 
		println("watch_bullet error: null BulletPanel")
	end
	return widget
end

local function GetChatBox()
	local game_name = 'game'
	local game_interface = UIManager.GetInterface(game_name)
	local game_chat = game_interface:GetWidget(game_name..'_chatInput')
	local spectator_name = 'game_spectator'
	local spectator_interface = UIManager.GetInterface(spectator_name)
	local spectator_chat = spectator_interface:GetWidget(spectator_name..'_chatInput')
	return game_chat, spectator_chat
end

local function FocusChatBox(focus)
	if activeChatbox == nil then return end
	if focus then
		GameChat.lastType = 'comment'
		activeChatbox:SetFocus(true)
	else
		GameChat.lastType = 'all'
		activeChatbox:SetFocus(false)
	end
end

local function ShowBulletPannel(show)
	local widget = GetBulletPanel()
	if widget == nil then return end
	widget:SetVisible(show)
end

local function RecycleCommentPanel(commentPanel)
	commentPanel:SetVisible(false)
	table.insert(WatchBullet.freePanels, commentPanel)
end

local function GetCommentSubPanel(commentPanel, sub)
	local subName = commentPanel:GetName()..sub
	local subWidget = commentPanel:GetChildWidget(subName)
	return subWidget
end

local function ShowCommentPanelOutline(commentPanel, show)
	local frame = GetCommentSubPanel(commentPanel, '_frame')
	frame:SetVisible(show)
end

-- count discard comments
local function UpdateJamCount(deltaSysTime)
	for i, comment in pairs(WatchBullet.jams) do
		comment.expired = comment.expired + deltaSysTime
	end
	local timeout = GetCvarNumber('bullet_comment_jam_check_time')
	while WatchBullet.jamsCount > 0 and WatchBullet.jams[1].expired > timeout do
		local discard = table.remove(WatchBullet.jams, 1)
		WatchBullet.jamsCount = WatchBullet.jamsCount - 1
	end
end

local function DiscardComment(commentPair)
	println("watch_bullet discarding "..commentPair.comment..', pending='..WatchBullet.pendingCommentsCount..', discard='..WatchBullet.jamsCount)
	commentPair.expired = 0
	table.insert(WatchBullet.jams, commentPair)
	WatchBullet.jamsCount = WatchBullet.jamsCount + 1
end

-- pending comment
local function PushPendingComment(comment, isLocalComment, pushToFront)
	WatchBullet.pendingCommentsCount = WatchBullet.pendingCommentsCount + 1
	if pushToFront then
		table.insert(WatchBullet.pendingComments, 1, {
			comment = comment,
			expired = 0,
			isLocalComment = isLocalComment
		})
	else
		table.insert(WatchBullet.pendingComments, {
			comment = comment,
			expired = 0,
			isLocalComment = isLocalComment
		})
	end
end

local function PopPendingComment()
	if WatchBullet.pendingCommentsCount == 0 then return nil end
	WatchBullet.pendingCommentsCount = WatchBullet.pendingCommentsCount - 1
	local comment = table.remove(WatchBullet.pendingComments, 1)
	return comment
end

local function PeekPendingComment()
	if WatchBullet.pendingCommentsCount > 0 then
		return WatchBullet.pendingComments[1]
	else 
		return nil
	end
end

local function UpdatePendingCommentTimes(deltaTime)
	for i, comment in pairs(WatchBullet.pendingComments) do
		comment.expired = comment.expired + deltaTime
	end
	if WatchBullet.jamming then return end			-- not discard comments on jam state
	while WatchBullet.pendingCommentsCount > 0 and
		WatchBullet.pendingComments[1].expired > Bullet_Comment_Max_Pending_Delay do
		local discard = PopPendingComment()
		DiscardComment(discard)
	end
	while WatchBullet.pendingCommentsCount > Bullet_Comment_Max_Pending_Count do
		local discard = PopPendingComment()
		DiscardComment(discard)
	end
end

-- init
local function GetOrCreateLine(widget, lineName, i)

	local lineHeight = WatchBullet.lineHeight
	local lineGap = WatchBullet.lineGap
	local y = (lineHeight + lineGap) * (i - 1) 
			+ WatchBullet.lineStart
	y = y..'%'
	local height = lineHeight..'%'

	local panel = interface:GetWidget(lineName)
	if panel == nil then
		widget:Instantiate("watch_bullets_line",
			"lineNumber", i,
			"y", y,
			'height', height
			)
		panel = interface:GetWidget(lineName)
	else
		panel:SetY(y)
		panel:SetHeight(height)
	end
	return panel
end

local function UpdateCommentsShowing(force)
	local enabled = GetCvarBool('watch_bullet_showing')
	if force or WatchBullet.enabled ~= enabled then
		println('WatchBullet toggle watch_bullet_showing')
		WatchBullet.enabled = enabled
		ShowBulletPannel(enabled)
		return true
	end
	return false
end

function WatchBullet:Init()
	println('watch_bullet Init()')
	
	local widget = GetBulletPanel()
	if widget == nil then return end

	UpdateCommentsShowing(true);

	-- chat box
	local game_chat, spectator_chat = GetChatBox()
	WatchBullet.chatOriginalMaxLength = game_chat:GetMaxLength()
	local maxLength = GetCvarNumber('watch_bullet_comment_max_length')
	game_chat:SetMaxLength(maxLength)
	spectator_chat:SetMaxLength(maxLength)

	local activeInterface = UIManager.GetActiveInterface()
	activeChatbox = activeInterface:GetWidget(activeInterface:GetName()..'_chatInput')
	FocusChatBox(false)
	FocusChatBox(true)

	-- line settings
	local isOfficial = false
	if not WatchBullet.matchId then
		WatchBullet.matchId = WatchBulletGetMatchId()
	end
	if WatchBullet.matchId and Watch_System and Watch_System.GetSelectedMatch then
		local tmpTable = Watch_System.GetSelectedMatch(matchId)
		if tmpTable then
			isOfficial = not tmpTable.is_recommended
		else
			println('WatchBullet error: nil is_recommended for matchId '..matchId)
		end
	else 
		println('WatchBullet error: nil Watch_System or GetSelectedMatch')
	end

	local suffix = ''
	if isOfficial then
		suffix = '_official'
	end

	WatchBullet.lineStart = GetCvarNumber('bullet_comment_line_start'..suffix)
	WatchBullet.lineHeight = GetCvarNumber('bullet_comment_line_height'..suffix)
	WatchBullet.lineGap = GetCvarNumber('bullet_comment_line_gap'..suffix)
	WatchBullet.lineCount = GetCvarNumber('bullet_comment_line_count'..suffix);

	-- line creation
	local count = 0
	for i, line in pairs(WatchBullet.lines) do
		local lineName = 'watch_bullets_line_'..i
		line.panel = GetOrCreateLine(widget, lineName, line.lineNumber)
		line.commentCount = 0
		line.commentPanelPairs = {}
		line.pendingLength = 0
		line.pendingTime = 0
		count = count + 1
		if count == WatchBullet.lineCount then break end
	end
	if count < WatchBullet.lineCount then
		local model = WatchBullet.lines[count]	
		for i = count + 1, WatchBullet.lineCount, 1 do
			local lineName = 'watch_bullets_line_'..i
			local line = {
				lineNumber = i, 
				lifeTime = model.lifeTime, 
				preferLength = model.preferLength, 
				panel = GetOrCreateLine(widget, lineName, i),
				commentCount = 0,
				commentPanelPairs = {},
				pendingLength = 0,
				pendingTime = 0,
			}
			WatchBullet.lines[i] = line
		end
	end

	WatchBullet.RegionWidth = widget:GetWidth()
	WatchBullet.frameDeltaSum = 0
	WatchBullet.frameDeltaCount = 0
end

local function EnsureInted()
	if not WatchBullet.inited then
		WatchBullet.inited = true
		WatchBullet:Init()
	end
end

function WatchBullet:Start(matchId)
	println("watch_bullet Start() matchId="..matchId)
	local matchId = AtoN(matchId)
	WatchBullet.matchId = matchId
end

function WatchBullet:Stop()
	println("watch_bullet Stop()")

	if not WatchBullet.inited then return end;

	ShowBulletPannel(false)

	-- reset chat max length
	local game_chat, spectator_chat = GetChatBox()
	game_chat:SetMaxLength(WatchBullet.chatOriginalMaxLength)
	spectator_chat:SetMaxLength(WatchBullet.chatOriginalMaxLength)
	
	FocusChatBox(false)

	-- free mem
	for i, line in pairs(WatchBullet.lines) do
		if i > WatchBullet.lineCount then break end
		for j, panelPair in pairs(line.commentPanelPairs) do
			--RecycleCommentPanel(panelPair.panel)
			panelPair.panel:Destroy()
		end
		line.commentPanelPairs = {}
	end
	WatchBullet.pendingComments = {}
	WatchBullet.pendingCommentsCount = 0

	for i, panel in pairs(WatchBullet.freePanels) do
		panel:Destroy()
	end	
	WatchBullet.freePanels = {}

	local widget = GetBulletPanel()
	for i = 1, WatchBullet.lineCount do
		local lineName = 'watch_bullets_line_'..i
		local panel = GetOrCreateLine(widget, lineName, i)
		--panel:Destroy()
	end

	WatchBullet.matchId = 0
	WatchBullet.inited = false

	println("WatchBullet:Stop() end")
end

-- spawn
local function GetStringLengthWithPadding(comment)
	return GetStringWidth(Bullet_Comment_Font, comment)
				+ Bullet_Comment_Padding / 100 * WatchBullet.RegionWidth
end

local function CommentLifeTime(lineLifeTime, marginedLength)
	local overtakeTime = lineLifeTime * Bullet_Comment_Overtake_Ratio
	local overtakeLength = WatchBullet.RegionWidth * Bullet_Comment_Overtake_Ratio + marginedLength
	local speed = overtakeLength / overtakeTime
	local totalTime = (WatchBullet.RegionWidth + marginedLength) / speed
	return totalTime, speed
end

local function CanPostToLine(line, commentLength)
	local marginedLength = commentLength + Bullet_Comment_Margin
	local commentLifeTime = CommentLifeTime(line.lifeTime, marginedLength)
	local percent = WatchBullet.RegionWidth / (WatchBullet.RegionWidth + marginedLength)
	local hitTime = percent * commentLifeTime
	return hitTime > line.pendingTime
end

local function PostToLine(line, commentPair, commentLength)
	
	-- init comment panel	
	local commentPanel = table.remove(WatchBullet.freePanels)
	if commentPanel == nil then
		WatchBullet.NameSuffix = WatchBullet.NameSuffix + 1
		local name = "watch_bullets_commet"..WatchBullet.NameSuffix
		line.panel:Instantiate("watch_bullets_commet",
			'x', "100%",
			'name', name,
			'width', commentLength,
			'font', Bullet_Comment_Font,
			'content', commentPair.comment
			)
		commentPanel = interface:GetWidget(name)
	else
		commentPanel:SetX("100%")
		commentPanel:SetWidth(commentLength)
		commentPanel:SetParent(line.panel)
		commentPanel:SetVisible(true)
		local label = GetCommentSubPanel(commentPanel, '_label')
		label:SetText(commentPair.comment)
	end

	ShowCommentPanelOutline(commentPanel, commentPair.isLocalComment)

	-- label floating speed
	local marginedLength = commentLength + Bullet_Comment_Margin
	local commentLifeTime, speed = CommentLifeTime(line.lifeTime, marginedLength)

	-- update data
	line.commentCount = line.commentCount + 1
	line.pendingLength = (WatchBullet.RegionWidth + marginedLength) / WatchBullet.RegionWidth * 100
	line.pendingTime = commentLifeTime
	table.insert(line.commentPanelPairs,{
			panel = commentPanel,
			x = WatchBullet.RegionWidth,
			width = marginedLength,
			speed = speed
		})
end

function WatchBullet:Spawn(isLocalComment, comment) 
	local isLocalComment = AtoB(isLocalComment)
	PushPendingComment(comment, isLocalComment, false)
end

-- post
function WatchBullet:PostFail(reason)
	println('watch_bullet postFail '..reason)
	GameChat:AllChatMessages('666', nil, reason, nil, nil, "false")
end

function WatchBullet:Post(comment)
	PushPendingComment(comment, true, true)	
end

-- update
local function GetDeltaTime(deltaSysTime, deltaGameTime)
	
	local stopped = deltaGameTime == 0
	local jumpped = deltaGameTime < -Bullet_Comment_Max_Frame_Delta or deltaGameTime > Bullet_Comment_Max_Frame_Delta
	
	if WatchBullet.frameDeltaCount < 30 then
		WatchBullet.frameDeltaCount = WatchBullet.frameDeltaCount + 1
	else
		WatchBullet.frameDeltaSum = WatchBullet.frameDeltaSum - WatchBullet.frameDeltaSum / 30
	end
	WatchBullet.frameDeltaSum = WatchBullet.frameDeltaSum + deltaSysTime
	local deltaTime = WatchBullet.frameDeltaSum / WatchBullet.frameDeltaCount

	return deltaTime, stopped, jumpped 
end

local function TrySpawnComment(line, commentPair, commentLength, checkPreferLength, checkPendingLength, checkCollision)
	if commentPair == nil then 
		return commentPair, commentLength, false
	end

	-- check line length
	local checked = true
	if checkPreferLength then
		checked = line.pendingLength <= line.preferLength
	elseif checkPendingLength then
		checked = line.pendingLength < 100
	end

	-- check collistion
	if commentLength < 0 then
		commentLength = GetStringLengthWithPadding(commentPair.comment)
	end	
	if checked and checkCollision then
		checked = CanPostToLine(line, commentLength)
	end

	if checked then
		PopPendingComment()
		PostToLine(line, commentPair, commentLength)

		commentPair = PeekPendingComment()
		commentLength = -1
	end
	return commentPair, commentLength, checked
end

local nextSpawnLineIndex = 1
function WatchBullet:Update(deltaSysTime, deltaGameTime, live)
	EnsureInted()

	local showingChanged = UpdateCommentsShowing(false);

	local deltaSysTime = AtoN(deltaSysTime)
	local deltaGameTime = AtoN(deltaGameTime)
	local live = AtoB(live)

	-- update time
	local deltaTime, gameStopped, gameJumped = GetDeltaTime(deltaSysTime, deltaGameTime)
	if deltaTime == 0 then return end

	local spawnNewComments = (not gameStopped) or live
	spawnNewComments = spawnNewComments and WatchBullet.enabled

	local discardAllComments = gameJumped
	if discardAllComments then
		WatchBullet.pendingCommentsCount = 0
		WatchBullet.pendingComments = {}
	end

	local discardShowingComments = discardAllComments or (showingChanged and not WatchBullet.enabled)

	-- update lines
	local hasFreeLines = discardShowingComments
	local pendingComment = PeekPendingComment()
	local pendingCommentLength = -1

	for i, line in pairs(WatchBullet.lines) do
		if i > WatchBullet.lineCount then break end			-- test line count

		if line.commentCount > 0 then

			local maxPendingLength = 0
			if discardShowingComments then
				-- remove all comments
				for j = 1, line.commentCount do
					local panelPair = line.commentPanelPairs[j]
					RecycleCommentPanel(panelPair.panel)
				end
				line.commentPanelPairs = {}
				line.commentCount = 0
			else
				-- update comments' positions or remove it
				for j = line.commentCount, 1, -1 do
					panelPair = line.commentPanelPairs[j]
					local deltaX = panelPair.speed * deltaTime
					local newX = panelPair.x - deltaX

					if newX + panelPair.width < 0  then
						RecycleCommentPanel(panelPair.panel)
						table.remove(line.commentPanelPairs, j)
						line.commentCount = line.commentCount - 1
					else
						panelPair.x = newX
						panelPair.panel:SetX(newX)
						local pendingLength = newX + panelPair.width
						if pendingLength > maxPendingLength then
							maxPendingLength = pendingLength
						end
					end
				end
			end

			-- update line data
			if line.commentCount == 0 then
				line.pendingLength = 0
				line.pendingTime = 0
			else
				line.pendingLength = maxPendingLength / WatchBullet.RegionWidth * 100
				line.pendingTime = line.pendingTime - deltaTime
			end
		end
		
		-- spawn comment when line has prefered length
		if spawnNewComments then
			pendingComment, pendingCommentLength = TrySpawnComment(line, pendingComment, pendingCommentLength, true, true, true)
		end

		if line.commentCount == 0 then
			hasFreeLines = true
		end
	end

	-- spawn comments
	if spawnNewComments and pendingComment ~= nil then
		-- spawn when line has enough space
		for i, line in pairs(WatchBullet.lines) do
			if i > WatchBullet.lineCount or pendingComment == nil then break end
			pendingComment, pendingCommentLength = TrySpawnComment(line, pendingComment, pendingCommentLength, false, true, true)
		end
		-- spawn when comment posted by local player
		while pendingComment ~= nil and pendingComment.isLocalComment do
			local line = WatchBullet.lines[nextSpawnLineIndex]
			pendingComment, pendingCommentLength = TrySpawnComment(line, pendingComment, pendingCommentLength, false, false, false)
			nextSpawnLineIndex = (nextSpawnLineIndex % WatchBullet.lineCount)+ 1
		end
		-- spawn when in jam state
		if pendingComment ~= nil and WatchBullet.jamming then
			for i, line in pairs(WatchBullet.lines) do
				if i > WatchBullet.lineCount or pendingComment == nil then break end		
				pendingComment, pendingCommentLength = TrySpawnComment(line, pendingComment, pendingCommentLength, false, false, true)
			end
		end
		if pendingComment ~= nil and WatchBullet.jamming then
			local spawnTime = math.max(1, GetCvarNumber('bullet_comment_jam_spawn_time'))
			local rate = math.max(0, 1 - WatchBullet.jamTime / spawnTime)
			local maxDelay = rate * Bullet_Comment_Max_Pending_Delay
			while pendingComment ~= nil and pendingComment.expired >= maxDelay do
				local line = WatchBullet.lines[nextSpawnLineIndex]
				pendingComment, pendingCommentLength = TrySpawnComment(line, pendingComment, pendingCommentLength, false, false, false)
				nextSpawnLineIndex = (nextSpawnLineIndex % WatchBullet.lineCount)+ 1
			end
		end
	end

	-- update pending comments time
	if not discardAllComments then
		UpdatePendingCommentTimes(deltaTime)		
	end

	-- update jam state, using real time
	if spawnNewComments then
		UpdateJamCount(deltaSysTime)
		if WatchBullet.jamming then
			if hasFreeLines then
				WatchBullet.jamming = false
				WatchBullet.jamsCount = 0
				WatchBullet.jams = {}
				println("watch_bullet leave jam state")
			else
				WatchBullet.jamTime = WatchBullet.jamTime + deltaSysTime		
			end
		elseif WatchBullet.jamsCount > 0 then
			if WatchBullet.jamsCount > GetCvarNumber('bullet_comment_jam_check_count') then 			
				WatchBullet.jamming = true
				WatchBullet.jamTime = 0
				println("watch_bullet enter jam state, discard count = "..WatchBullet.jamsCount)
			end
		end
	end

end

function WatchBullet:OnPanelHide()
	WatchBullet.pendingCommentsCount = 0
	WatchBullet.pendingComments = {}
	for i, line in pairs(WatchBullet.lines) do
		if line and line.commentCount ~= nil then
			for j = 1, tonumber(line.commentCount) do
				local panelPair = line.commentPanelPairs[j]
				RecycleCommentPanel(panelPair.panel)
			end
		end
		line.commentPanelPairs = {}
		line.commentCount = 0
		line.pendingLength = 0
		line.pendingTime = 0
	end
end

-- register
interface:RegisterWatch('WatchBulletStart', WatchBullet.Start)
interface:RegisterWatch('WatchBulletStop', WatchBullet.Stop)
interface:RegisterWatch('WatchBulletUpdate', WatchBullet.Update)
interface:RegisterWatch('WatchBulletSpawn', WatchBullet.Spawn)
interface:RegisterWatch('WatchBulletPostFail', WatchBullet.PostFail)
interface:RegisterWatch('WatchBulletPost', WatchBullet.Post)


-- test
function AjustBulletOvertakeRatio(str)
	local newValue = AtoN(str)
	println("set from "..Bullet_Comment_Overtake_Ratio.." to "..newValue)
	Bullet_Comment_Overtake_Ratio = newValue
end

function AdjustBulletLifeTime(str)
	local newValue = AtoN(str)
	println("set from "..WatchBullet.lines[1].lifeTime.." to "..newValue)
	for i,v in pairs(WatchBullet.lines) do
		v.lifeTime = newValue
	end
end