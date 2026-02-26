---------------------------------------------------------- 
--	Name: 		Cards Script	            			--				
--  Copyright 2015 Frostburn Studios					--
---------------------------------------------------------- 

local CARDS_MAX_INSTANTIATED = 14
local CARDS_MAX_DISPLAYED = 10
local CARDS_REDUCTION_PER_TIER = 0.90
local ANIM_TIME = 150

---------------------------------------------------------- 
--														--				
---------------------------------------------------------- 

lib_Card = {}
local c = {}
c.userViewingCard = 1
c.totalActiveCards = 0

local function SetCardOrder()	
	if (c.ActiveTemplate) then
		for i = #c.cards, (c.userViewingCard+1), -1 do
			if GetWidgetNoMem(c.ActiveTemplate..'_parent_'..i, nil, true) then
				GetWidgetNoMem(c.ActiveTemplate..'_parent_'..i):BringToFront()
			end
		end	
		for i = 1, c.userViewingCard, 1 do
			if GetWidgetNoMem(c.ActiveTemplate..'_parent_'..i, nil, true) then
				GetWidgetNoMem(c.ActiveTemplate..'_parent_'..i):BringToFront()
			end
		end
	end
end

local function ResizeAndAnimateCards()
	if (c.ActiveTemplate) then
		for index, cardTable in ipairs(c.cards) do
			if GetWidgetNoMem(c.ActiveTemplate..'_parent_'..index, nil, true) then
				if GetWidgetNoMem(c.ActiveTemplate..'_parent_'..index..'_bg', nil, true) then
					GetWidgetNoMem(c.ActiveTemplate..'_parent_'..index..'_bg', nil, true):SetColor(cardTable.color)
				end
				--GetWidgetNoMem(c.ActiveTemplate..'_parent_'..index, nil, true):SetColor(cardTable.color)
					
				GetWidgetNoMem(c.ActiveTemplate..'_parent_'..index, nil, true):ScaleWidth(cardTable.width, ANIM_TIME)	
				
				if GetWidgetNoMem(c.ActiveTemplate..'_model_'..index, nil, true) then
					--GetWidgetNoMem(c.ActiveTemplate..'_model_'..index, nil, true):ScaleHeight(cardTable.height, ANIM_TIME)	
					GetWidgetNoMem(c.ActiveTemplate..'_parent_'..index, nil, true):ScaleHeight(cardTable.height, ANIM_TIME)	
					if (cardTable.doEvent) then
						GetWidgetNoMem(c.ActiveTemplate..'_model_'..index, nil, true):DoEventN(cardTable.doEvent)
					end
				else
					GetWidgetNoMem(c.ActiveTemplate..'_parent_'..index, nil, true):ScaleHeight(cardTable.height, ANIM_TIME)					
				end
				
				GetWidgetNoMem(c.ActiveTemplate..'_parent_'..index, nil, true):SetX(cardTable.lastx)
					
				GetWidgetNoMem(c.ActiveTemplate..'_parent_'..index, nil, true):Sleep(ANIM_TIME, function()
					GetWidgetNoMem(c.ActiveTemplate..'_parent_'..index, nil, true):SlideX(cardTable.x, ANIM_TIME, true)	
					if (not cardTable.hide) then
						GetWidgetNoMem(c.ActiveTemplate..'_parent_'..index):FadeIn(250)
					end					
				end)	
				
				if (cardTable.hide) then
					GetWidgetNoMem(c.ActiveTemplate..'_parent_'..index):FadeOut(250)
				end					
			end
		end
	end	
end

local function SortCardTable()	
	local centerPos = (CARDS_MAX_DISPLAYED % 2) + 1
	local cardOffset = (centerPos - c.userViewingCard)
	local cardOffCenter = 0
	local flip = -1
	
	for index, cardTable in ipairs(c.cards) do
		cardOffCenter = ((centerPos - index) - cardOffset) 
		flip = -1
		if (cardOffCenter < 0) then
			flip = 1
			cardOffCenter = cardOffCenter * -1
		end
		
		if (cardOffCenter == 0) then
			cardTable.doEvent = 1
		else
			cardTable.doEvent = 0
		end
		
		-- cards appear to move -- in a circle
		if (cardOffCenter > (centerPos + 1)) then
			cardOffCenter = centerPos + 1
			if (c.Card.default.hideSideCards) then
				cardTable.hide = true
			else
				cardTable.hide = false
			end
		else
			cardTable.hide = false
		end
		
		local reducePerTier = c.Card.default.reducePerTier or CARDS_REDUCTION_PER_TIER
		
		local width = GetWidgetNoMem('main_card_spawn_target'):GetWidthFromString(c.Card.default.width)	
		cardTable.lastwidth	= cardTable.width
		cardTable.width		=		(width * (reducePerTier ^ cardOffCenter) )
		
		cardTable.lastx 	= 		cardTable.x
		cardTable.x			=		c.Card.default.x + ((width - (width * (reducePerTier ^ cardOffCenter) )) * flip * 1.3) + (c.Card.default.xMod * (width * (reducePerTier ^ (0.3 * cardOffCenter)) ) * flip * cardOffCenter)
		
		local height = GetWidgetNoMem('main_card_spawn_target'):GetHeightFromString(c.Card.default.height)	
		cardTable.lastheight	= cardTable.height
		cardTable.height		=		(height * (reducePerTier ^ cardOffCenter) )
		
		local color = cardOffCenter/5 --(reducePerTier ^ cardOffCenter)		
		cardTable.color		=	'0 0 0 ' .. color	
		
	end	
	GetWidgetNoMem('main_card_next_btn'):SetVisible((c.userViewingCard + 1) <= #c.cards)
	GetWidgetNoMem('main_card_prev_btn'):SetVisible((c.userViewingCard - 1) >= 1)
end

local function ChangeViewedCard(newTarget)
	c.userViewingCard = newTarget
	SortCardTable()
	ResizeAndAnimateCards()
	SetCardOrder()
end

local function ShowCardInterface()
	if (lib_Card) then
		lib_Card.contextActive = lib_Card.contextActive or {}
		lib_Card.contextActive.libcards = true
	end
	GetWidgetNoMem('main_card_parent'):SetVisible(true)
end

local function HideCardInterface()
	GetWidgetNoMem('main_card_parent'):SetVisible(false)
end

local function DestroyCards()
	HideCardInterface()
	c.cards = {}
	c.userViewingCard = 1
	c.totalActiveCards = 0	
	groupfcall('main_card_widgets', function(index, widget, groupName) widget:Destroy() end)
	if (lib_Card) and (lib_Card.contextActive) and (lib_Card.contextActive.libcards) then
		lib_Card.contextActive.libcards = false
		interface:UICmd("DeleteResourceContext('libcards')")
	end
end

-- create actual card widget
local function InstantiateCard(cardTable)
	if (c.totalActiveCards < CARDS_MAX_INSTANTIATED) then
		c.totalActiveCards = c.totalActiveCards + 1
		--println('^y InstantiateCard(cardTable)')
		--printTable(cardTable)
		if (GetWidgetNoMem(cardTable.TEMPLATE..'_parent_'..c.totalActiveCards, nil, true) == nil) then
			println('^g Instantiate ' .. cardTable.TEMPLATE .. ' # ' .. c.totalActiveCards)
			c.ActiveTemplate = cardTable.TEMPLATE
			if (cardTable.Name) and NotEmpty(cardTable.Name) then 
				cardTable.Name = '.' .. cardTable.Name
			end
			GetWidgetNoMem('main_card_spawn_target'):Instantiate(cardTable.TEMPLATE,
				'index'			,	c.totalActiveCards,
				'x'				,	cardTable.x or '',
				'y'				,	cardTable.y or '',
				'width'			,	cardTable.width or '',
				'height'		,	cardTable.height or '',
				'color'			,	cardTable.color or '',
				'header'		,	cardTable.HEADER or '',
				'subheader'		,	cardTable.SUBHEADER or '',
				'icon'			,	cardTable.ICON or '',
				'type'			,	cardTable.TYPE or '',
				'startdate'		,	cardTable.STARTDATE or '',
				'enddate'		,	cardTable.ENDDATE or '',
				'checkmark'		,	cardTable.CHECKMARK or '',
				'rewardinfo1'	,	cardTable.REWARDINFO1 or '',
				'rewardinfo2'	,	cardTable.REWARDINFO2 or '',
				'rewardinfo3'	,	cardTable.REWARDINFO3 or '',
				
				'model'			,	cardTable.Model or '',
				'heroEntity'	,	cardTable.TypeName or '',
				'altCode'		,	cardTable.Name or '',
				'scale'			,	cardTable.Scale or '',
				'pos'			,	cardTable.Pos or '',
				'angles'		,	cardTable.Angles or ''
			)
		end
	end
end

local function CreateCardTable(rewardsTable)
	DestroyCards()
	GetWidgetNoMem('main_card_spawn_target'):Sleep(1, function()
		c.cards = {}
		-- Card object creation
		c.Card = {}
		c.Card.default = {x=0, lastx=0, xMod=0, y=0, width='77.6h', height='24h', lastwidth='57.6h', lastheight='18h', valign='center', align='center', TEMPLATE='generic_card_template'}
		c.Card.mt = {}

		function c.Card.new(cardParameters)
			cardParameters = cardParameters or {}
			setmetatable(cardParameters, c.Card.mt)
			return cardParameters
		end

		function c.Card.mt.__index(table, key)
			return c.Card.default[key]
		end		
		
		-- turn rewards info into cards
		for index, rewardTable in ipairs(rewardsTable) do	
			c.Card.default = rewardTable.DEFAULTS or c.Card.default
			table.insert(c.cards, c.Card.new(rewardTable))
		end
		-- sort cards with offset
		SortCardTable()
		-- instantiate card ui object
		for index, cardTable in ipairs(c.cards) do
			InstantiateCard(cardTable)
		end
		
		GetWidgetNoMem('main_card_spawn_target'):Sleep(1, function()
			ResizeAndAnimateCards()
			SetCardOrder()		
		end)
		
		ShowCardInterface()
	end)
end

function lib_Card:RegisterCard()
	SetCardOrder()	
end

function lib_Card:NextViewedCard()
	if (c.userViewingCard + 1) <= #c.cards then
		ChangeViewedCard(c.userViewingCard + 1)
	end
end

function lib_Card:PrevViewedCard()
	if (c.userViewingCard - 1) >= 1 then
		ChangeViewedCard(c.userViewingCard - 1)
	end
end

---------------------------------------------------------- 
--														--				
---------------------------------------------------------- 

function lib_Card:DisplayCardception(rewards)
	if (rewards) and (#rewards > 0) then
		--println('lib_Card:DisplayCardception(rewards)')
		--printTable(rewards)
		CreateCardTable(rewards)
		ShowCardInterface()
	end
end

function lib_Card:HideCardception()
	DestroyCards()
end
