local interface = object

local popoutMessageQueue		= {}
local popoutMessageActive		= false
local popoutMessageTimePerChar	= 125
local popoutMessageTimeMinimum	= 10000
local popoutMessageSlideTime	= 250

local function spectatorPopoutMessageDisplay(message)
	if message then
		popoutMessageActive = true
		local container = interface:GetWidget('spectatorPopoutMessage')
		container:FadeIn(popoutMessageSlideTime)
		interface:GetWidget('spectatorPopoutMessageLabel'):SetText(message)
		container:SlideY(0, popoutMessageSlideTime)
		container:Sleep(math.max(popoutMessageTimeMinimum, (popoutMessageTimePerChar * string.len(message))), function()
			container:SlideY('100%', popoutMessageSlideTime)
			container:FadeOut(popoutMessageSlideTime, function()
				if #popoutMessageQueue > 0 then
					spectatorPopoutMessageDisplay(table.remove(popoutMessageQueue, 1))
				else
					popoutMessageActive = false
				end
			end)
		end)
	end
end

interface:GetWidget('spectatorPopoutMessage'):RegisterWatch('EventSpectatorToast', function(widget, message)
	if message and type(message) == 'string' and string.len(message) > 0 then
		if popoutMessageActive then
			table.insert(popoutMessageQueue, message)
		else
			spectatorPopoutMessageDisplay(message)
		end
	end
end)

interface:GetWidget('spectatorPopoutMessageContainer'):RegisterWatch('GamePhase', function(widget, gamePhase)
	if phase == 0 then
		popoutMessageQueue = {}
	end
end)

