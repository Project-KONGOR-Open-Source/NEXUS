local _G = getfenv(0)
local ipairs, pairs, select, string, table, next, type, unpack, tinsert, tconcat, tremove, fmt, tostring, tonumber, tsort = _G.ipairs, _G.pairs, _G.select, _G.string, _G.table, _G.next, _G.type, _G.unpack, _G.table.insert, _G.table.concat, _G.table.remove, _G.string.format, _G.tostring, _G.tonumber, _G.table.sort
local interface, interfaceName = object, object:GetName()

Common_V2 = {}

local _gameOptionPutToFront = {}
table.insert(_gameOptionPutToFront, 'map_selection')
table.insert(_gameOptionPutToFront, 'hosted_game')
table.insert(_gameOptionPutToFront, 'lobby')

local _secondaryMenuId = 0

math.randomseed(GetTime())

--Restrict target to the top and right safe area
function Common_V2:RestrictTRSafeArea(wdgTarget, wdgRefName, fromInterface)
	if wdgTarget == nil then Echo('Common_V2:RestrictTRSafeArea') return end

	fromInterface = fromInterface or 'main'

	local wdgRef = NotEmpty(wdgRefName) and GetWidget(wdgRefName, fromInterface) or nil
	local x, xThreshold = wdgTarget:GetX(), GetScreenWidth() - wdgTarget:GetWidth()
	local alignRight = wdgTarget:GetAlign() == 'right'
	if x > xThreshold then
		wdgTarget:SetX(alignRight and -1 or (GetScreenWidth() - wdgTarget:GetWidth()))
	elseif x == xThreshold then
		local targetX = 0
		if alignRight then
			targetX = wdgRef ~= nil and -(GetScreenWidth() - (wdgRef:GetAbsoluteX() + wdgRef:GetWidth())) or -1
		else
			targetX = GetScreenWidth() - wdgTarget:GetWidth()
		end
		wdgTarget:SetX(targetX)
	end
	local destY = wdgRef:GetAbsoluteY() + wdgRef:GetHeight() + 10
	if not (wdgTarget:GetY() >= destY) then
		wdgTarget:SetY(destY)
	end
end

function Common_V2:UsingLargeUI()
	local screenW = GetScreenWidth()
	local screenH = GetScreenHeight()
	return (screenW / screenH) >= 1.6
end

function Common_V2:PopupMenu(menutable, width)
	if menutable == nil or #menutable == 0 then return end
	GetWidget('newui_popup_menu_menuroot'):ClearChildren()
	_secondaryMenuId = 0

	local maxWidth = 0
	if width == nil then
		for i,v in ipairs(menutable) do
			local menuStringWidth = GetWidget('newui_popup_menu_stringwidth'):GetStringWidth(Translate(v['content']))
			if menuStringWidth > maxWidth then
				maxWidth = menuStringWidth
			end
		end
		width = tostring(maxWidth + 20)
	end

	for i,v in ipairs(menutable) do
		local showline = (i ~= 1)
		if v['type'] == 2 then 
			GetWidget('newui_popup_menu_menuroot'):Instantiate('newui_popup_menu_item2_template',
																'width', width,
																'content', v['content'] or '',
																'id', tostring(_secondaryMenuId),
																'menu', v['menu'],
																'showline', tostring(showline))

			_secondaryMenuId = _secondaryMenuId + 1
		else
			GetWidget('newui_popup_menu_menuroot'):Instantiate('newui_popup_menu_item_template',
																'width', width,
																'content', v['content'] or '',
																'onclicklua', v['onclicklua'] or '',
																'showline', tostring(showline))
		end
	end

	local x = Input.GetCursorPosX()
	local y = Input.GetCursorPosY()
	local w = tonumber(GetWidget('newui_popup_menu'):GetWidth())
	local h = tonumber(GetWidget('newui_popup_menu'):GetHeight())
	local screenW = GetScreenWidth()
	local screenH = GetScreenHeight()
	if (x+w) > screenW then x = screenW - w end
	if (y+h) > screenH then y = screenH - h end

	GetWidget('newui_popup_menu'):SetX(tostring(x))
	GetWidget('newui_popup_menu'):SetY(tostring(y))
	GetWidget('newui_popup_menu'):SetVisible(true)
	GetWidget('newui_popup_menu'):SetExclusive()

	Trigger('NewUIPopupMenu')

	PlaySound('/shared/sounds/ui/revamp/region_click.wav')
end

function Common_V2:ShowGenericTip(show, title, desc, width)
	if show and (NotEmpty(title) or NotEmpty(desc)) then
		GetWidget('newui_generic_tip'):BringToFront()

		GetWidget('newui_generic_tip'):SetVisible(true)
		GetWidget('newui_generic_tip_line'):SetVisible(NotEmpty(title))
		GetWidget('newui_generic_tip_title_container'):SetVisible(NotEmpty(title))

		GetWidget('newui_generic_tip_title'):SetText(Translate(title))
		GetWidget('newui_generic_tip_desc'):SetText(Translate(desc))

		if width ~= nil then
			GetWidget('newui_generic_tip'):SetWidth(width)
		else
			GetWidget('newui_generic_tip'):SetWidth('300i')
		end
	else
		GetWidget('newui_generic_tip'):SetVisible(false)
	end
end

function Common_V2:ShowSimpleTip(show, text, width, textalign)
	if show and NotEmpty(text) then
		GetWidget('newui_simple_tip'):SetVisible(true)

		if width ~= nil then
			GetWidget('newui_simple_tip'):SetWidth(width)
		else
			GetWidget('newui_simple_tip'):SetWidth('300i')
		end

		GetWidget('newui_simple_tip_text'):SetText(Translate(text))
		GetWidget('newui_simple_tip_text_left'):SetText(Translate(text))
		GetWidget('newui_simple_tip_text'):SetVisible(textalign ~= 'left')
		GetWidget('newui_simple_tip_text_left'):SetVisible(textalign == 'left')
	else
		GetWidget('newui_simple_tip'):SetVisible(false)
	end
end

function Common_V2:SetCheckboxText(name, text)
	for i=1,8 do
		GetWidget(name..'_text'..i):SetText(text)
	end
end

function Common_V2:FollowPlayer(nickname)
	local name = StripClanTag(nickname)
	Loading_V2:SetHostedLoadingInfo('unknown')
	interface:UICmd('Follow(\''..name..'\');')
end

function Common_V2:UnFollowPlayer(nickname)
	local name = StripClanTag(nickname)
	interface:UICmd('UnFollow(\''..name..'\');')
end

function Common_V2:MonitorGame(nickname)
	nickname = StripClanTag(nickname)
	local wdgContainer = GetWidget('watch_user_main', 'main')
	if not wdgContainer:HasChildWidget('watch_user_box_'..nickname) and GetCvarInt('watch_user_activeWatches') < 3 then
		wdgContainer:Instantiate('watch_user_box', 'user', nickname)
	end
end

function Common_V2:SpectatePlayer(nickname, bMentor)
	interface:UICmd('ChatSpectatePlayer(\''..nickname..'\', \''..tostring(bMentor)..'\');')
end

function Common_V2:JoinGameAsStaff(nickname)
	interface:UICmd('ChatJoinGameAsStaff(\''..nickname..'\');')
end

function Common_V2:ChatJoinGame(nickname)
	local name = StripClanTag(nickname)
	Loading_V2:SetHostedLoadingInfo('unknown')
	interface:UICmd('ChatJoinGame(\''..name..'\');')

	Social_V2:CloseSocialPopups()
end

function Common_V2:AddFriend(nickname)
	interface:UICmd('ChatAddBuddy(\''..nickname..'\');')
end

function Common_V2:RemoveFriend(nickname)
	local name = StripClanTag(nickname)
	interface:UICmd('ChatRemoveBuddy(\''..name..'\');')
end

function Common_V2:GameInvite(nickname)
	local name = StripClanTag(nickname)
	interface:UICmd('GameInvite(\''..name..'\');')
end

function Common_V2:GetUserInfo(nickname)
	local name = StripClanTag(nickname)
	interface:UICmd('GetUserInfo(\''..name..'\');')
end

function Common_V2:ChatIgnore(nickname)
	local name = StripClanTag(nickname)
	interface:UICmd('ChatIgnore(\''..name..'\');')
end

function Common_V2:ChatUnignore(nickname)
	local name = StripClanTag(nickname)
	interface:UICmd('ChatUnignore(\''..name..'\');')
end

function Common_V2:ChatAddBanlist(nickname)
	local name = StripClanTag(nickname)
	interface:UICmd('ChatAddBanlist(\''..name..'\', \'None\');')
end

function Common_V2:ChatRemoveBanlist(nickname)
	local name = StripClanTag(nickname)
	interface:UICmd('ChatRemoveBanlist(\''..name..'\');')
end

function Common_V2:ChatChannelPromote(nickname, channelID)
	local name = StripClanTag(nickname)
	interface:UICmd('ChatChannelPromote(\''..name..'\', \''..channelID..'\');')
end

function Common_V2:ChatChannelDemote(nickname, channelID)
	local name = StripClanTag(nickname)
	interface:UICmd('ChatChannelDemote(\''..name..'\', \''..channelID..'\', 5);')
end

function Common_V2:ChatPromoteClanMember(nickname)
	local name = StripClanTag(nickname)
	interface:UICmd('ChatPromoteClanMember(\''..name..'\');')
end

function Common_V2:ChatDemoteClanMember(nickname)
	local name = StripClanTag(nickname)
	interface:UICmd('ChatDemoteClanMember(\''..name..'\');')
end

function Common_V2:ChatInviteUserToClan(nickname)
	local name = StripClanTag(nickname)
	interface:UICmd('ChatInviteUserToClan(\''..name..'\');')
end

function Common_V2:ChatRemoveClanMember(nickname)
	local name = StripClanTag(nickname)
	interface:UICmd('ChatRemoveClanMember(\''..name..'\');')
end

function Common_V2:SetAutoConnect(chatName, isRemove)
	if isRemove then
		interface:UICmd("ChatRemoveChannel('"..chatName.."');")
	else
		interface:UICmd("ChatSaveChannel('"..chatName.."');")
	end
end

function Common_V2:SetLongText(widget, text, text2)
	text2 = text2 or ''
	local labelWidth = widget:GetWidth()
	local stringWidth = widget:GetStringWidth(text..text2)

	local needTip = false

	local fitText = text..text2
	if labelWidth < stringWidth then
		for i= string.len(text) - 2, 1, -1 do
			fitText = string.sub(text, 1, i)..'...'..text2
			stringWidth = widget:GetStringWidth(fitText)

			if stringWidth <= labelWidth then
				break
			end
		end

		needTip = true
	end

	widget:SetText(fitText)
	return needTip
end

function Common_V2:SetLongTextLabel(name, label1, label2)
	label2 = label2 or ''
	local labelWidget = GetWidget(name)
	if labelWidget then
		local needTip = Common_V2:SetLongText(labelWidget, label1, label2)
		local tipLabelRoot = GetWidget(name..'_tip_root')
		local tipLabelWidget = GetWidget(name..'_tip_value')

		if tipLabelRoot then
			tipLabelRoot:SetVisible(needTip)
		end

		if tipLabelWidget then
			tipLabelWidget:SetText(label1)
		end
	end
end

function Common_V2:GetPlayerColor(index)
	if index == 0 then
		return '#5473cb'
	elseif index == 1 then
		return '#63c8b1'
	elseif index == 2 then
		return '#844cac'
	elseif index == 3 then
		return '#ccc954'
	elseif index == 4 then
		return '#d0985c'
	elseif index == 5 then
		return '#ce8ab3'
	elseif index == 6 then
		return '#a1a1a2'
	elseif index == 7 then
		return '#a3c4dd'
	elseif index == 8 then
		return '#4a7465'
	else
		return '#8f6b51'
	end
end