------------------------------------------
--			Objective Sets 				--
------------------------------------------

-- Designers, this is the file you are looking for :D

-- This file has multiple functions, each of these functions is tied to a trigger.
-- The intent is that you create a function for each map/tutorial you want (each unique sets of objectives, etc)
-- In the map, you set 'game_objectives' as an overlay interface (which will cause this file to load, make sure /ui/game_objectives.interface is in the resource list)
-- Then you fire the trigger that corresponds to your initialization function
-- The function should set everything up, start watching all the triggers you've given it and then you should be good to go!

-- An example for each type of thing you can create
local function ExampleInit()
	-- Start by turning the interface on, which makes it visible and resets everything
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(true)
	HoN_Objectives.EnableDialog(true)
	HoN_Objectives.EnableTips(true)


	-- function HoN_Objectives.RegisterObjective(
	-- startTrigger,	-- The trigger to watch to start this objective
	-- mainLabel,		-- The main text for the objective
	-- maxValue,		-- The value we are trying to reach for any value based objects (x / 100 creep kills, etc)
	-- valueTrigger,	-- The trigger to watch to get the current progress value
	-- tipTrigger,		-- The trigger to fire for clicking for help
	-- pingScriptMessage-- The script message to send to use the map ping help system
	-- icon,			-- Any icon for the objective
	-- completeTrigger	-- The trigger to watch for the objective being complete
	-- )
	HoN_Objectives.RegisterObjective(
		'objOneStartTrigger',
		'This string will be translated',
		nil,		-- use nil for anything unused
		nil,
		nil,
		nil,
		'/ui/icons/ability_abuse.tga',
		'objOneComplete'
	)

	HoN_Objectives.RegisterObjective(
		'objTwoStartTrigger',
		'This string too!',
		6,
		'objTwoValue',
		nil,
		nil,
		'/ui/icons/ability_abuse.tga',
		'objTwoComplete'
	)

	-- function HoN_Objectives.RegisterSound(
	-- 	playTrigger,	-- Trigger that causes the sound to play
	-- 	soundPath		-- Path to the sound
	-- )
	HoN_Objectives.RegisterSound(
		'sndTestOnePlay',
		'/shared/sounds/announcer/startgame.ogg'
	)

	-- function HoN_Objectives.RegisterWidgetHighlight(
	-- playTrigger,	-- Trigger to cause the highlight
	-- widgetName,		-- Widget to highlight
	-- widgetInterface,-- Interface that the widget you want to highlight is in
	-- time,			-- Length of time to highlight (ms)
	-- stopTrigger,	-- Trigger to stop the effect instantly
	-- specialEffect	-- Any special effect to play on the highlight widget
	-- )
	HoN_Objectives.RegisterWidgetHighlight(
		'wdgTestOneStart',
		'mini_map_left',
		'game',
		nil,	-- nil makes it stay forever
		'wdgTestOneStop',
		nil
	)

	-- function HoN_Objectives.RegisterTip(
	-- tipTrigger,		-- The trigger to add the tip
	-- tipLabel,		-- The label to show on the small tip
	-- tipIcon,		-- The icon for the tip
	-- clickTrigger,	-- The trigger that fires when the tip is clicked
	-- tipLife,		-- How long the tip lasts before it disappears
	-- removeTrigger	-- Trigger to remove the tip
	-- )
	HoN_Objectives.RegisterTip(
		'tipTestOneDisplay',
		'TIP 1',
		'/ui/icons/admin.tga',
		'helpTestOneDisplay',
		nil,
		'tipTestOneRemove'
	)
	HoN_Objectives.RegisterTip(
		'tipTestTwoDisplay',
		'TIP 2',
		'/ui/icons/allheroes.tga',
		'diagTestOneDisplay',
		nil,
		'tipTestTwoRemove'
	)
	HoN_Objectives.RegisterTip(
		'tipTestThreeDisplay',
		'TIP 3',
		'/ui/icons/allNighter.tga',
		'chainTestOneStart',
		nil,
		'tipTestThreeRemove'
	)

	-- function HoN_Objectives.RegisterDialog(
	-- dialogTrigger,	-- Trigger to show the dialog
	-- dialogTitle,	-- Title text (such as a name) for the dialog
	-- dialogText,		-- The text for the dialog
	-- dialogImage,	-- The image for the dialog
	-- dialogLife,		-- How long the dialog is visible
	-- hideTrigger		-- Trigger to hide the dialog
	-- )
	HoN_Objectives.RegisterDialog(
		'diagTestOneDisplay',
		'hUrO oF NeWUrtH',
		'WOOF WOOF. LANE. GANK. AHHHHHHHH?',
		'/ui/fe2/store/icons/account_icons/derp.tga',
		10000,
		'diagTestOneHide'
	)

	-- function HoN_Objectives.RegisterHelp(
	-- 	displayTrigger,		-- The trigger that causes the window to display
	-- 	title,				-- The title on the window
	-- 	image,				-- The image in the window
	-- 	body,				-- The main body text for the window, next to the image
	--  life,				-- Lifetime of the window before it disappears
	-- 	hideTrigger,		-- Trigger to hide the box
	-- )
	HoN_Objectives.RegisterHelp(
		'helpTestOneDisplay',
		'OMG HELP!',
		'/ui/fe2/store/icons/helping_hands.tga',
		"YES, IT'S HERE! HELP HAS FINALLY ARRIVED. IN THE FORM OF A GOD DAMN USELESS WINDOW. BASK IN THE GLORY OF MY SQUARE UGLYNESS!!!",
		nil,
		nil
	)

	HoN_Objectives.RegisterHelp(
		'helpTestTwoDisplay',
		'OMG HELP!',
		'',
		"YES, IT'S HERE! HELP HAS FINALLY ARRIVED. IN THE FORM OF A GOD DAMN USELESS WINDOW. BASK IN THE GLORY OF MY SQUARE UGLYNESS!!!",
		nil,
		nil
	)

	HoN_Objectives.RegisterHelp(
		'helpTestThreeDisplay',
		'OMG HELP!',
		'/ui/fe2/store/icons/helping_hands.tga',
		"YES, IT'S HERE! HELP HAS FINALLY ARRIVED. IN THE FORM OF A GOD DAMN USELESS WINDOW. BASK IN THE GLORY OF MY SQUARE UGLYNESS!!!",
		nil,
		nil
	)

	-- function HoN_Objectives.RegisterEventChain(
	-- 	startTrigger,	-- The trigger that starts the chain
	-- 	chainLinks,		-- Table that contains each link in the chain
	-- 	stopTrigger		-- Trigger that stops a running chain
	-- )
	HoN_Objectives.RegisterEventChain(
		'chainTestOneStart',
		{	[1] = {['time'] = 1000, ['triggers'] = {'wdgTestOneStart', 'sndTestOnePlay'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)
			[2] = {['time'] = nil, ['triggers'] = {'wdgTestOneStop', 'sndTestOnePlay'}},	-- next link, ran after the time for the first one is over, same as above
		},
		nil
	)
end
UITrigger.CreateTrigger('testObjectivesTrigger')
object:RegisterWatch('testObjectivesTrigger', ExampleInit)

-- The Laning Tutorial Triggers and objectives
local function Tutorial_Laning_Easy()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(true)
	HoN_Objectives.EnableDialog(true)
	HoN_Objectives.EnableTips(true)


	-- function HoN_Objectives.RegisterObjective(
	-- startTrigger,	-- The trigger to watch to start this objective
	-- mainLabel,		-- The main text for the objective
	-- maxValue,		-- The value we are trying to reach for any value based objects (x / 100 creep kills, etc)
	-- valueTrigger,	-- The trigger to watch to get the current progress value
	-- tipTrigger,		-- The trigger to fire for clicking for help
	-- pingScriptMessage-- The script message to send to use the map ping help system
	-- icon,			-- Any icon for the objective
	-- completeTrigger	-- The trigger to watch for the objective being complete
	-- )
	HoN_Objectives.RegisterObjective(
		'objOneStartTrigger',
		'tutorial_laning_objective_one_easy',
		nil,
		nil,
		nil,
		nil,
		'/ui/icons/ability_abuse.tga',
		'objOneComplete'
	)

	HoN_Objectives.RegisterObjective(
		'objTwoStartTrigger',
		'tutorial_laning_objective_two_easy',
		15,
		'objTwoValue',
		nil,
		nil,
		'/ui/icons/ability_abuse.tga',
		'objTwoComplete'
	)

	-- function HoN_Objectives.RegisterSound(
	-- 	playTrigger,	-- Trigger that causes the sound to play
	-- 	soundPath		-- Path to the sound
	-- )
	HoN_Objectives.RegisterSound(
		'sndOnePlay',
		'/shared/sounds/announcer/startgame.ogg'
	)

	-- function HoN_Objectives.RegisterWidgetHighlight(
	-- playTrigger,	-- Trigger to cause the highlight
	-- widgetName,		-- Widget to highlight
	-- widgetInterface,-- Interface that the widget you want to highlight is in
	-- time,			-- Length of time to highlight (ms)
	-- stopTrigger,	-- Trigger to stop the effect instantly
	-- specialEffect	-- Any special effect to play on the highlight widget
	-- )
	HoN_Objectives.RegisterWidgetHighlight(
		'wdgOneHighlight',
		'game_info_courier_button',
		'game',
		15000,	-- nil makes it stay forever
		'wdgOneHighlightStop',
		nil
	)

	HoN_Objectives.RegisterWidgetHighlight(
		'wdgTwoHighlight',
		'panel_abilities',
		'game',
		10000,	-- nil makes it stay forever
		'wdgTwoHighlightStop',
		nil
	)

	-- function HoN_Objectives.RegisterTip(
	-- tipTrigger,		-- The trigger to add the tip
	-- tipLabel,		-- The label to show on the small tip
	-- tipIcon,		-- The icon for the tip
	-- clickTrigger,	-- The trigger that fires when the tip is clicked
	-- tipLife,		-- How long the tip lasts before it disappears
	-- removeTrigger	-- Trigger to remove the tip
	-- )
	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'tutorial_laning_tip_one',
		'/ui/icons/admin.tga',
		'helpOneDisplay',
		nil,
		'tipOneRemove'
	)

	HoN_Objectives.RegisterTip(
		'tipTwoDisplay',
		'tutorial_laning_tip_two',
		'/ui/icons/admin.tga',
		'helpTwoDisplay',
		nil,
		'tipTwoRemove'
	)

	-- function HoN_Objectives.RegisterDialog(
	-- dialogTrigger,	-- Trigger to show the dialog
	-- dialogTitle,	-- Title text (such as a name) for the dialog
	-- dialogText,		-- The text for the dialog
	-- dialogImage,	-- The image for the dialog
	-- dialogLife,		-- How long the dialog is visible
	-- hideTrigger		-- Trigger to hide the dialog
	-- )
	HoN_Objectives.RegisterDialog(
		'diagOneDisplay',
		'tutorial_laning_dialog_one_title_easy',
		'tutorial_laning_dialog_one_easy',
		'/ui/fe2/store/icons/account_icons/derp.tga',
		15000,
		'diagOneHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagTwoDisplay',
		'tutorial_laning_dialog_two_title_easy',
		'tutorial_laning_dialog_two_easy',
		'/ui/fe2/store/icons/account_icons/derp.tga',
		20000,
		'diagTwoHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagThreeDisplay',
		'tutorial_laning_dialog_three_title_easy',
		'tutorial_laning_dialog_three_easy',
		'/ui/fe2/store/icons/account_icons/derp.tga',
		15000,
		'diagThreeHide'
	)

	-- function HoN_Objectives.RegisterHelp(
	-- 	displayTrigger,		-- The trigger that causes the window to display
	-- 	title,				-- The title on the window
	-- 	image,				-- The image in the window
	-- 	body1,				-- The main body text for the window, next to the image
	--  body2,				-- The 2nd block of body text, beneath the image
	--  life,				-- Lifetime of the window before it disappears
	-- 	hideTrigger,		-- Trigger to hide the box
	-- )
	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'tutorial_laning_help_one_title',
		'/ui/fe2/store/icons/helping_hands.tga',
		"tutorial_laning_help_one",
		nil,
		nil
	)

	HoN_Objectives.RegisterHelp(
		'helpTwoDisplay',
		'tutorial_laning_help_two_title',
		'/ui/fe2/store/icons/helping_hands.tga',
		"tutorial_laning_help_two",
		nil,
		nil
	)

	-- function HoN_Objectives.RegisterEventChain(
	-- 	startTrigger,	-- The trigger that starts the chain
	-- 	chainLinks,		-- Table that contains each link in the chain
	-- 	stopTrigger		-- Trigger that stops a running chain
	-- )
	HoN_Objectives.RegisterEventChain(
		'chainOneStart',
		{	[1] = {['time'] = 15000, ['triggers'] = {'diagOneDisplay', 'objOneStartTrigger', 'tipOneDisplay'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)
			[2] = {['time'] = nil, ['triggers'] = {'diagTwoDisplay', 'wdgOneHighlight'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)
		},
		nil
	)

	HoN_Objectives.RegisterEventChain(
		'chainTwoStart',
		{	[1] = {['time'] = 15000, ['triggers'] = {'wdgOneHighlightStop','wdgTwoHighlight', 'diagThreeDisplay', 'sndOnePlay', 'tipOneRemove', 'tipTwoDisplay', 'objTwoStartTrigger'}},
			[2] = {['time'] = nil, ['triggers'] = {'wdgTwoHighlightStop'}},
		},
		nil
	)
end
UITrigger.CreateTrigger('TutorialLaningObjectivesTrigger_Easy')
object:RegisterWatch('TutorialLaningObjectivesTrigger_Easy', Tutorial_Laning_Easy)

local function Tutorial_Laning_Medium()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(true)
	HoN_Objectives.EnableDialog(true)
	HoN_Objectives.EnableTips(true)


	-- function HoN_Objectives.RegisterObjective(
	-- startTrigger,	-- The trigger to watch to start this objective
	-- mainLabel,		-- The main text for the objective
	-- maxValue,		-- The value we are trying to reach for any value based objects (x / 100 creep kills, etc)
	-- valueTrigger,	-- The trigger to watch to get the current progress value
	-- tipTrigger,		-- The trigger to fire for clicking for help
	-- pingScriptMessage-- The script message to send to use the map ping help system
	-- icon,			-- Any icon for the objective
	-- completeTrigger	-- The trigger to watch for the objective being complete
	-- )
	HoN_Objectives.RegisterObjective(
		'objOneStartTrigger',
		'tutorial_laning_objective_one_medium',
		nil,
		nil,
		nil,
		nil,
		'/ui/icons/ability_abuse.tga',
		'objOneComplete'
	)

	HoN_Objectives.RegisterObjective(
		'objTwoStartTrigger',
		'tutorial_laning_objective_two_medium',
		15,
		'objTwoValue',
		nil,
		nil,
		'/ui/icons/ability_abuse.tga',
		'objTwoComplete'
	)

	HoN_Objectives.RegisterObjective(
		'objThreeStartTrigger',
		'tutorial_laning_objective_three_medium',
		10,
		'objThreeValue',
		nil,
		nil,
		'/ui/icons/ability_abuse.tga',
		'objThreeComplete'
	)

	-- function HoN_Objectives.RegisterSound(
	-- 	playTrigger,	-- Trigger that causes the sound to play
	-- 	soundPath		-- Path to the sound
	-- )
	HoN_Objectives.RegisterSound(
		'sndOnePlay',
		'/shared/sounds/announcer/startgame.ogg'
	)

	-- function HoN_Objectives.RegisterWidgetHighlight(
	-- playTrigger,	-- Trigger to cause the highlight
	-- widgetName,		-- Widget to highlight
	-- widgetInterface,-- Interface that the widget you want to highlight is in
	-- time,			-- Length of time to highlight (ms)
	-- stopTrigger,	-- Trigger to stop the effect instantly
	-- specialEffect	-- Any special effect to play on the highlight widget
	-- )
	HoN_Objectives.RegisterWidgetHighlight(
		'wdgOneHighlight',
		'game_info_courier_button',
		'game',
		15000,	-- nil makes it stay forever
		'wdgOneHighlightStop',
		nil
	)

	HoN_Objectives.RegisterWidgetHighlight(
		'wdgTwoHighlight',
		'panel_abilities',
		'game',
		10000,	-- nil makes it stay forever
		'wdgTwoHighlightStop',
		nil
	)

	-- function HoN_Objectives.RegisterTip(
	-- tipTrigger,		-- The trigger to add the tip
	-- tipLabel,		-- The label to show on the small tip
	-- tipIcon,		-- The icon for the tip
	-- clickTrigger,	-- The trigger that fires when the tip is clicked
	-- tipLife,		-- How long the tip lasts before it disappears
	-- removeTrigger	-- Trigger to remove the tip
	-- )
	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'tutorial_laning_tip_one',
		'/ui/icons/admin.tga',
		'helpOneDisplay',
		nil,
		'tipOneRemove'
	)

	HoN_Objectives.RegisterTip(
		'tipTwoDisplay',
		'tutorial_laning_tip_two',
		'/ui/icons/admin.tga',
		'helpTwoDisplay',
		nil,
		'tipTwoRemove'
	)

	HoN_Objectives.RegisterTip(
		'tipThreeDisplay',
		'tutorial_laning_tip_three',
		'/ui/icons/admin.tga',
		'helpThreeDisplay',
		nil,
		'tipThreeRemove'
	)

	-- function HoN_Objectives.RegisterDialog(
	-- dialogTrigger,	-- Trigger to show the dialog
	-- dialogTitle,	-- Title text (such as a name) for the dialog
	-- dialogText,		-- The text for the dialog
	-- dialogImage,	-- The image for the dialog
	-- dialogLife,		-- How long the dialog is visible
	-- hideTrigger		-- Trigger to hide the dialog
	-- )
	HoN_Objectives.RegisterDialog(
		'diagOneDisplay',
		'tutorial_laning_dialog_one_title_medium',
		'tutorial_laning_dialog_one_medium',
		'/ui/fe2/store/icons/account_icons/derp.tga',
		15000,
		'diagOneHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagTwoDisplay',
		'tutorial_laning_dialog_two_title_medium',
		'tutorial_laning_dialog_two_medium',
		'/ui/fe2/store/icons/account_icons/derp.tga',
		20000,
		'diagTwoHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagThreeDisplay',
		'tutorial_laning_dialog_three_title_medium',
		'tutorial_laning_dialog_three_medium',
		'/ui/fe2/store/icons/account_icons/derp.tga',
		15000,
		'diagThreeHide'
	)

	-- function HoN_Objectives.RegisterHelp(
	-- 	displayTrigger,		-- The trigger that causes the window to display
	-- 	title,				-- The title on the window
	-- 	image,				-- The image in the window
	-- 	body1,				-- The main body text for the window, next to the image
	--  body2,				-- The 2nd block of body text, beneath the image
	--  life,				-- Lifetime of the window before it disappears
	-- 	hideTrigger,		-- Trigger to hide the box
	-- )
	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'tutorial_laning_help_one_title',
		'/ui/fe2/store/icons/helping_hands.tga',
		"tutorial_laning_help_one",
		nil,
		nil
	)

	HoN_Objectives.RegisterHelp(
		'helpTwoDisplay',
		'tutorial_laning_help_two_title',
		'/ui/fe2/store/icons/helping_hands.tga',
		"tutorial_laning_help_two",
		nil,
		nil
	)

	HoN_Objectives.RegisterHelp(
		'helpThreeDisplay',
		'tutorial_laning_help_three_title',
		'/ui/fe2/store/icons/helping_hands.tga',
		"tutorial_laning_help_three",
		nil,
		nil
	)

	-- function HoN_Objectives.RegisterEventChain(
	-- 	startTrigger,	-- The trigger that starts the chain
	-- 	chainLinks,		-- Table that contains each link in the chain
	-- 	stopTrigger		-- Trigger that stops a running chain
	-- )
	HoN_Objectives.RegisterEventChain(
		'chainOneStart',
		{	[1] = {['time'] = 15000, ['triggers'] = {'diagOneDisplay', 'objOneStartTrigger', 'tipOneDisplay'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)			
			[2] = {['time'] = nil, ['triggers'] = {'diagTwoDisplay', 'wdgOneHighlight'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)			
		},
		nil
	)

	HoN_Objectives.RegisterEventChain(
		'chainTwoStart',
		{	[1] = {['time'] = 15000, ['triggers'] = {'wdgTwoHighlight', 'diagThreeDisplay', 'sndOnePlay', 'tipOneRemove', 'tipTwoDisplay', 'objTwoStartTrigger', 'objThreeStartTrigger'}},
			[2] = {['time'] = nil, ['triggers'] = {'wdgTwoHighlightStop'}},
		},
		nil
	)
end
UITrigger.CreateTrigger('TutorialLaningObjectivesTrigger_Medium')
object:RegisterWatch('TutorialLaningObjectivesTrigger_Medium', Tutorial_Laning_Medium)

local function Tutorial_Laning_Hard()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(true)
	HoN_Objectives.EnableDialog(true)
	HoN_Objectives.EnableTips(true)


	-- function HoN_Objectives.RegisterObjective(
	-- startTrigger,	-- The trigger to watch to start this objective
	-- mainLabel,		-- The main text for the objective
	-- maxValue,		-- The value we are trying to reach for any value based objects (x / 100 creep kills, etc)
	-- valueTrigger,	-- The trigger to watch to get the current progress value
	-- tipTrigger,		-- The trigger to fire for clicking for help
	-- pingScriptMessage-- The script message to send to use the map ping help system
	-- icon,			-- Any icon for the objective
	-- completeTrigger	-- The trigger to watch for the objective being complete
	-- )
	HoN_Objectives.RegisterObjective(
		'objOneStartTrigger',
		'tutorial_laning_objective_one_hard',
		nil,
		nil,
		nil,
		nil,
		'/ui/icons/ability_abuse.tga',
		'objOneComplete'
	)

	HoN_Objectives.RegisterObjective(
		'objTwoStartTrigger',
		'tutorial_laning_objective_two_hard',
		15,
		'objTwoValue',
		nil,
		nil,
		'/ui/icons/ability_abuse.tga',
		'objTwoComplete'
	)

	HoN_Objectives.RegisterObjective(
		'objThreeStartTrigger',
		'tutorial_laning_objective_three_hard',
		10,
		'objThreeValue',
		nil,
		nil,
		'/ui/icons/ability_abuse.tga',
		'objThreeComplete'
	)

	-- function HoN_Objectives.RegisterSound(
	-- 	playTrigger,	-- Trigger that causes the sound to play
	-- 	soundPath		-- Path to the sound
	-- )
	HoN_Objectives.RegisterSound(
		'sndOnePlay',
		'/shared/sounds/announcer/startgame.ogg'
	)

	-- function HoN_Objectives.RegisterWidgetHighlight(
	-- playTrigger,	-- Trigger to cause the highlight
	-- widgetName,		-- Widget to highlight
	-- widgetInterface,-- Interface that the widget you want to highlight is in
	-- time,			-- Length of time to highlight (ms)
	-- stopTrigger,	-- Trigger to stop the effect instantly
	-- specialEffect	-- Any special effect to play on the highlight widget
	-- )
	HoN_Objectives.RegisterWidgetHighlight(
		'wdgOneHighlight',
		'game_info_courier_button',
		'game',
		15000,	-- nil makes it stay forever
		'wdgOneHighlightStop',
		nil
	)

	HoN_Objectives.RegisterWidgetHighlight(
		'wdgTwoHighlight',
		'panel_abilities',
		'game',
		15000,	-- nil makes it stay forever
		'wdgTwoHighlightStop',
		nil
	)

	-- function HoN_Objectives.RegisterTip(
	-- tipTrigger,		-- The trigger to add the tip
	-- tipLabel,		-- The label to show on the small tip
	-- tipIcon,		-- The icon for the tip
	-- clickTrigger,	-- The trigger that fires when the tip is clicked
	-- tipLife,		-- How long the tip lasts before it disappears
	-- removeTrigger	-- Trigger to remove the tip
	-- )
	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'tutorial_laning_tip_one',
		'/ui/icons/admin.tga',
		'helpOneDisplay',
		nil,
		'tipOneRemove'
	)

	HoN_Objectives.RegisterTip(
		'tipTwoDisplay',
		'tutorial_laning_tip_two',
		'/ui/icons/admin.tga',
		'helpTwoDisplay',
		nil,
		'tipTwoRemove'
	)

	HoN_Objectives.RegisterTip(
		'tipThreeDisplay',
		'tutorial_laning_tip_three',
		'/ui/icons/admin.tga',
		'helpThreeDisplay',
		nil,
		'tipThreeRemove'
	)

	-- function HoN_Objectives.RegisterDialog(
	-- dialogTrigger,	-- Trigger to show the dialog
	-- dialogTitle,	-- Title text (such as a name) for the dialog
	-- dialogText,		-- The text for the dialog
	-- dialogImage,	-- The image for the dialog
	-- dialogLife,		-- How long the dialog is visible
	-- hideTrigger		-- Trigger to hide the dialog
	-- )
	HoN_Objectives.RegisterDialog(
		'diagOneDisplay',
		'tutorial_laning_dialog_one_title_hard',
		'tutorial_laning_dialog_one_hard',
		'/ui/fe2/store/icons/account_icons/derp.tga',
		15000,
		'diagOneHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagTwoDisplay',
		'tutorial_laning_dialog_two_title_hard',
		'tutorial_laning_dialog_two_hard',
		'/ui/fe2/store/icons/account_icons/derp.tga',
		20000,
		'diagTwoHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagThreeDisplay',
		'tutorial_laning_dialog_three_title_hard',
		'tutorial_laning_dialog_three_hard',
		'/ui/fe2/store/icons/account_icons/derp.tga',
		15000,
		'diagThreeHide'
	)

	-- function HoN_Objectives.RegisterHelp(
	-- 	displayTrigger,		-- The trigger that causes the window to display
	-- 	title,				-- The title on the window
	-- 	image,				-- The image in the window
	-- 	body1,				-- The main body text for the window, next to the image
	--  body2,				-- The 2nd block of body text, beneath the image
	--  life,				-- Lifetime of the window before it disappears
	-- 	hideTrigger,		-- Trigger to hide the box
	-- )
	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'tutorial_laning_help_one_title',
		'/ui/fe2/store/icons/helping_hands.tga',
		"tutorial_laning_help_one",
		nil,
		nil
	)

	HoN_Objectives.RegisterHelp(
		'helpTwoDisplay',
		'tutorial_laning_help_two_title',
		'/ui/fe2/store/icons/helping_hands.tga',
		"tutorial_laning_help_two",
		nil,
		nil
	)

	HoN_Objectives.RegisterHelp(
		'helpThreeDisplay',
		'tutorial_laning_help_three_title',
		'/ui/fe2/store/icons/helping_hands.tga',
		"tutorial_laning_help_three",
		nil,
		nil
	)

	-- function HoN_Objectives.RegisterEventChain(
	-- 	startTrigger,	-- The trigger that starts the chain
	-- 	chainLinks,		-- Table that contains each link in the chain
	-- 	stopTrigger		-- Trigger that stops a running chain
	-- )
	HoN_Objectives.RegisterEventChain(
		'chainOneStart',
		{	[1] = {['time'] = 15000, ['triggers'] = {'diagOneDisplay', 'objOneStartTrigger', 'tipOneDisplay'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)			
			[2] = {['time'] = nil, ['triggers'] = {'diagTwoDisplay', 'wdgOneHighlight'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)			
		},
		nil
	)

	HoN_Objectives.RegisterEventChain(
		'chainTwoStart',
		{	[1] = {['time'] = 15000, ['triggers'] = {'wdgTwoHighlight', 'diagThreeDisplay', 'sndOnePlay', 'tipOneRemove', 'tipTwoDisplay', 'objTwoStartTrigger', 'objThreeStartTrigger'}},
			[2] = {['time'] = nil, ['triggers'] = {'wdgTwoHighlightStop'}},
		},
		nil
	)
end
UITrigger.CreateTrigger('TutorialLaningObjectivesTrigger_Hard')
object:RegisterWatch('TutorialLaningObjectivesTrigger_Hard', Tutorial_Laning_Hard)

-- The Laning Tutorial Triggers and objectives
local function Tutorial_Jungle_Easy()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(true)
	HoN_Objectives.EnableDialog(true)
	HoN_Objectives.EnableTips(true)


	-- function HoN_Objectives.RegisterObjective(
	-- startTrigger,	-- The trigger to watch to start this objective
	-- mainLabel,		-- The main text for the objective
	-- maxValue,		-- The value we are trying to reach for any value based objects (x / 100 creep kills, etc)
	-- valueTrigger,	-- The trigger to watch to get the current progress value
	-- tipTrigger,		-- The trigger to fire for clicking for help
	-- pingScriptMessage-- The script message to send to use the map ping help system
	-- icon,			-- Any icon for the objective
	-- completeTrigger	-- The trigger to watch for the objective being complete
	-- )
	HoN_Objectives.RegisterObjective(
		'objOneStartTrigger',
		'tutorial_jungle_objective_one_easy',
		nil,
		nil,
		nil,
		nil,
		'/ui/icons/ability_abuse.tga',
		'objOneComplete'
	)

	HoN_Objectives.RegisterObjective(
		'objTwoStartTrigger',
		'tutorial_jungle_objective_two_easy',
		7,
		'objTwoValue',
		nil,
		nil,
		'/ui/icons/ability_abuse.tga',
		'objTwoComplete'
	)

	-- function HoN_Objectives.RegisterSound(
	-- 	playTrigger,	-- Trigger that causes the sound to play
	-- 	soundPath		-- Path to the sound
	-- )
	HoN_Objectives.RegisterSound(
		'sndOnePlay',
		'/shared/sounds/announcer/startgame.ogg'
	)

	-- function HoN_Objectives.RegisterWidgetHighlight(
	-- playTrigger,	-- Trigger to cause the highlight
	-- widgetName,		-- Widget to highlight
	-- widgetInterface,-- Interface that the widget you want to highlight is in
	-- time,			-- Length of time to highlight (ms)
	-- stopTrigger,	-- Trigger to stop the effect instantly
	-- specialEffect	-- Any special effect to play on the highlight widget
	-- )
	HoN_Objectives.RegisterWidgetHighlight(
		'wdgOneHighlight',
		'game_info_courier_button',
		'game',
		15000,	-- nil makes it stay forever
		'wdgOneHighlightStop',
		nil
	)

	HoN_Objectives.RegisterWidgetHighlight(
		'wdgTwoHighlight',
		'panel_abilities',
		'game',
		15000,	-- nil makes it stay forever
		'wdgTwoHighlightStop',
		nil
	)

	-- function HoN_Objectives.RegisterTip(
	-- tipTrigger,		-- The trigger to add the tip
	-- tipLabel,		-- The label to show on the small tip
	-- tipIcon,		-- The icon for the tip
	-- clickTrigger,	-- The trigger that fires when the tip is clicked
	-- tipLife,		-- How long the tip lasts before it disappears
	-- removeTrigger	-- Trigger to remove the tip
	-- )
	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'tutorial_jungle_tip_one',
		'/ui/icons/admin.tga',
		'helpOneDisplay',
		nil,
		'tipOneRemove'
	)

	HoN_Objectives.RegisterTip(
		'tipTwoDisplay',
		'tutorial_jungle_tip_two',
		'/ui/icons/admin.tga',
		'helpTwoDisplay',
		nil,
		'tipTwoRemove'
	)

	-- function HoN_Objectives.RegisterDialog(
	-- dialogTrigger,	-- Trigger to show the dialog
	-- dialogTitle,	-- Title text (such as a name) for the dialog
	-- dialogText,		-- The text for the dialog
	-- dialogImage,	-- The image for the dialog
	-- dialogLife,		-- How long the dialog is visible
	-- hideTrigger		-- Trigger to hide the dialog
	-- )
	HoN_Objectives.RegisterDialog(
		'diagOneDisplay',
		'tutorial_jungle_dialog_one_title_easy',
		'tutorial_jungle_dialog_one_easy',
		'/ui/fe2/store/icons/account_icons/derp.tga',
		15000,
		'diagOneHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagTwoDisplay',
		'tutorial_jungle_dialog_two_title_easy',
		'tutorial_jungle_dialog_two_easy',
		'/ui/fe2/store/icons/account_icons/derp.tga',
		20000,
		'diagTwoHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagThreeDisplay',
		'tutorial_jungle_dialog_three_title_easy',
		'tutorial_jungle_dialog_three_easy',
		'/ui/fe2/store/icons/account_icons/derp.tga',
		15000,
		'diagThreeHide'
	)

	-- function HoN_Objectives.RegisterHelp(
	-- 	displayTrigger,		-- The trigger that causes the window to display
	-- 	title,				-- The title on the window
	-- 	image,				-- The image in the window
	-- 	body1,				-- The main body text for the window, next to the image
	--  body2,				-- The 2nd block of body text, beneath the image
	--  life,				-- Lifetime of the window before it disappears
	-- 	hideTrigger,		-- Trigger to hide the box
	-- )
	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'tutorial_jungle_help_one_title',
		'/ui/fe2/store/icons/helping_hands.tga',
		"tutorial_jungle_help_one",
		nil,
		nil
	)

	HoN_Objectives.RegisterHelp(
		'helpTwoDisplay',
		'tutorial_jungle_help_two_title',
		'/ui/fe2/store/icons/helping_hands.tga',
		"tutorial_jungle_help_two",
		nil,
		nil
	)

	-- function HoN_Objectives.RegisterEventChain(
	-- 	startTrigger,	-- The trigger that starts the chain
	-- 	chainLinks,		-- Table that contains each link in the chain
	-- 	stopTrigger		-- Trigger that stops a running chain
	-- )
	HoN_Objectives.RegisterEventChain(
		'chainOneStart',
		{	[1] = {['time'] = 15000, ['triggers'] = {'diagOneDisplay', 'objOneStartTrigger', 'tipOneDisplay','wdgOneHighlight'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)			
			[2] = {['time'] = nil, ['triggers'] = {'diagOneHide','diagTwoDisplay'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)			
		},
		nil
	)

	HoN_Objectives.RegisterEventChain(
		'chainTwoStart',
		{	[1] = {['time'] = nil, ['triggers'] = {'diagTwoHide','wdgTwoHighlight', 'diagThreeDisplay', 'sndOnePlay', 'tipOneRemove', 'tipTwoDisplay', 'objTwoStartTrigger'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)
		},
		nil
	)
end
UITrigger.CreateTrigger('TutorialJungleObjectivesTrigger_Easy')
object:RegisterWatch('TutorialJungleObjectivesTrigger_Easy', Tutorial_Jungle_Easy)

local function Tutorial_Jungle_Medium()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(true)
	HoN_Objectives.EnableDialog(true)
	HoN_Objectives.EnableTips(true)


	-- function HoN_Objectives.RegisterObjective(
	-- startTrigger,	-- The trigger to watch to start this objective
	-- mainLabel,		-- The main text for the objective
	-- maxValue,		-- The value we are trying to reach for any value based objects (x / 100 creep kills, etc)
	-- valueTrigger,	-- The trigger to watch to get the current progress value
	-- tipTrigger,		-- The trigger to fire for clicking for help
	-- pingScriptMessage-- The script message to send to use the map ping help system
	-- icon,			-- Any icon for the objective
	-- completeTrigger	-- The trigger to watch for the objective being complete
	-- )
	HoN_Objectives.RegisterObjective(
		'objOneStartTrigger',
		'tutorial_jungle_objective_one_medium',
		nil,
		nil,
		nil,
		nil,
		'/ui/icons/ability_abuse.tga',
		'objOneComplete'
	)

	HoN_Objectives.RegisterObjective(
		'objTwoStartTrigger',
		'tutorial_jungle_objective_two_medium',
		7,
		'objTwoValue',
		nil,
		nil,
		'/ui/icons/ability_abuse.tga',
		'objTwoComplete'
	)

	HoN_Objectives.RegisterObjective(
		'objThreeStartTrigger',
		'tutorial_jungle_objective_three_medium',
		4,
		'objThreeValue',
		nil,
		nil,
		'/ui/icons/ability_abuse.tga',
		'objThreeComplete'
	)

	-- function HoN_Objectives.RegisterSound(
	-- 	playTrigger,	-- Trigger that causes the sound to play
	-- 	soundPath		-- Path to the sound
	-- )
	HoN_Objectives.RegisterSound(
		'sndOnePlay',
		'/shared/sounds/announcer/startgame.ogg'
	)

	-- function HoN_Objectives.RegisterWidgetHighlight(
	-- playTrigger,	-- Trigger to cause the highlight
	-- widgetName,		-- Widget to highlight
	-- widgetInterface,-- Interface that the widget you want to highlight is in
	-- time,			-- Length of time to highlight (ms)
	-- stopTrigger,	-- Trigger to stop the effect instantly
	-- specialEffect	-- Any special effect to play on the highlight widget
	-- )
	HoN_Objectives.RegisterWidgetHighlight(
		'wdgOneHighlight',
		'game_info_courier_button',
		'game',
		15000,	-- nil makes it stay forever
		'wdgOneHighlightStop',
		nil
	)

	HoN_Objectives.RegisterWidgetHighlight(
		'wdgTwoHighlight',
		'panel_abilities',
		'game',
		15000,	-- nil makes it stay forever
		'wdgTwoHighlightStop',
		nil
	)

	-- function HoN_Objectives.RegisterTip(
	-- tipTrigger,		-- The trigger to add the tip
	-- tipLabel,		-- The label to show on the small tip
	-- tipIcon,		-- The icon for the tip
	-- clickTrigger,	-- The trigger that fires when the tip is clicked
	-- tipLife,		-- How long the tip lasts before it disappears
	-- removeTrigger	-- Trigger to remove the tip
	-- )
	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'tutorial_jungle_tip_one',
		'/ui/icons/admin.tga',
		'helpOneDisplay',
		nil,
		'tipOneRemove'
	)

	HoN_Objectives.RegisterTip(
		'tipTwoDisplay',
		'tutorial_jungle_tip_two',
		'/ui/icons/admin.tga',
		'helpTwoDisplay',
		nil,
		'tipTwoRemove'
	)

	HoN_Objectives.RegisterTip(
		'tipThreeDisplay',
		'tutorial_jungle_tip_three',
		'/ui/icons/admin.tga',
		'helpThreeDisplay',
		nil,
		'tipThreeRemove'
	)

	-- function HoN_Objectives.RegisterDialog(
	-- dialogTrigger,	-- Trigger to show the dialog
	-- dialogTitle,	-- Title text (such as a name) for the dialog
	-- dialogText,		-- The text for the dialog
	-- dialogImage,	-- The image for the dialog
	-- dialogLife,		-- How long the dialog is visible
	-- hideTrigger		-- Trigger to hide the dialog
	-- )
	HoN_Objectives.RegisterDialog(
		'diagOneDisplay',
		'tutorial_jungle_dialog_one_title_medium',
		'tutorial_jungle_dialog_one_medium',
		'/ui/fe2/store/icons/account_icons/derp.tga',
		15000,
		'diagOneHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagTwoDisplay',
		'tutorial_jungle_dialog_two_title_medium',
		'tutorial_jungle_dialog_two_medium',
		'/ui/fe2/store/icons/account_icons/derp.tga',
		20000,
		'diagTwoHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagThreeDisplay',
		'tutorial_jungle_dialog_three_title_medium',
		'tutorial_jungle_dialog_three_medium',
		'/ui/fe2/store/icons/account_icons/derp.tga',
		15000,
		'diagThreeHide'
	)

	-- function HoN_Objectives.RegisterHelp(
	-- 	displayTrigger,		-- The trigger that causes the window to display
	-- 	title,				-- The title on the window
	-- 	image,				-- The image in the window
	-- 	body1,				-- The main body text for the window, next to the image
	--  body2,				-- The 2nd block of body text, beneath the image
	--  life,				-- Lifetime of the window before it disappears
	-- 	hideTrigger,		-- Trigger to hide the box
	-- )
	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'tutorial_jungle_help_one_title',
		'/ui/fe2/store/icons/helping_hands.tga',
		"tutorial_jungle_help_one",
		nil,
		nil
	)

	HoN_Objectives.RegisterHelp(
		'helpTwoDisplay',
		'tutorial_jungle_help_two_title',
		'/ui/fe2/store/icons/helping_hands.tga',
		"tutorial_jungle_help_two",
		nil,
		nil
	)

	HoN_Objectives.RegisterHelp(
		'helpThreeDisplay',
		'tutorial_jungle_help_three_title',
		'/ui/fe2/store/icons/helping_hands.tga',
		"tutorial_jungle_help_three",
		nil,
		nil
	)

	-- function HoN_Objectives.RegisterEventChain(
	-- 	startTrigger,	-- The trigger that starts the chain
	-- 	chainLinks,		-- Table that contains each link in the chain
	-- 	stopTrigger		-- Trigger that stops a running chain
	-- )
	HoN_Objectives.RegisterEventChain(
		'chainOneStart',
		{	[1] = {['time'] = 15000, ['triggers'] = {'diagOneDisplay', 'objOneStartTrigger', 'tipOneDisplay'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)			
			[2] = {['time'] = nil, ['triggers'] = {'diagTwoDisplay', 'wdgOneHighlight'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)			
		},
		nil
	)

	HoN_Objectives.RegisterEventChain(
		'chainTwoStart',
		{	[1] = {['time'] = nil, ['triggers'] = {'wdgTwoHighlight', 'diagThreeDisplay', 'sndOnePlay', 'tipOneRemove', 'tipTwoDisplay', 'objTwoStartTrigger', 'objThreeStartTrigger'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)
		},
		nil
	)
end
UITrigger.CreateTrigger('TutorialJungleObjectivesTrigger_Medium')
object:RegisterWatch('TutorialJungleObjectivesTrigger_Medium', Tutorial_Jungle_Medium)

local function Tutorial_Jungle_Hard()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(true)
	HoN_Objectives.EnableDialog(true)
	HoN_Objectives.EnableTips(true)


	-- function HoN_Objectives.RegisterObjective(
	-- startTrigger,	-- The trigger to watch to start this objective
	-- mainLabel,		-- The main text for the objective
	-- maxValue,		-- The value we are trying to reach for any value based objects (x / 100 creep kills, etc)
	-- valueTrigger,	-- The trigger to watch to get the current progress value
	-- tipTrigger,		-- The trigger to fire for clicking for help
	-- pingScriptMessage-- The script message to send to use the map ping help system
	-- icon,			-- Any icon for the objective
	-- completeTrigger	-- The trigger to watch for the objective being complete
	-- )
	HoN_Objectives.RegisterObjective(
		'objOneStartTrigger',
		'tutorial_jungle_objective_one_hard',
		nil,
		nil,
		nil,
		nil,
		'/ui/icons/ability_abuse.tga',
		'objOneComplete'
	)

	HoN_Objectives.RegisterObjective(
		'objTwoStartTrigger',
		'tutorial_jungle_objective_two_hard',
		7,
		'objTwoValue',
		nil,
		nil,
		'/ui/icons/ability_abuse.tga',
		'objTwoComplete'
	)

	HoN_Objectives.RegisterObjective(
		'objThreeStartTrigger',
		'tutorial_jungle_objective_three_hard',
		4,
		'objThreeValue',
		nil,
		nil,
		'/ui/icons/ability_abuse.tga',
		'objThreeComplete'
	)

	-- function HoN_Objectives.RegisterSound(
	-- 	playTrigger,	-- Trigger that causes the sound to play
	-- 	soundPath		-- Path to the sound
	-- )
	HoN_Objectives.RegisterSound(
		'sndOnePlay',
		'/shared/sounds/announcer/startgame.ogg'
	)

	-- function HoN_Objectives.RegisterWidgetHighlight(
	-- playTrigger,	-- Trigger to cause the highlight
	-- widgetName,		-- Widget to highlight
	-- widgetInterface,-- Interface that the widget you want to highlight is in
	-- time,			-- Length of time to highlight (ms)
	-- stopTrigger,	-- Trigger to stop the effect instantly
	-- specialEffect	-- Any special effect to play on the highlight widget
	-- )
	HoN_Objectives.RegisterWidgetHighlight(
		'wdgOneHighlight',
		'game_info_courier_button',
		'game',
		15000,	-- nil makes it stay forever
		'wdgOneHighlightStop',
		nil
	)

	HoN_Objectives.RegisterWidgetHighlight(
		'wdgTwoHighlight',
		'panel_abilities',
		'game',
		15000,	-- nil makes it stay forever
		'wdgTwoHighlightStop',
		nil
	)

	-- function HoN_Objectives.RegisterTip(
	-- tipTrigger,		-- The trigger to add the tip
	-- tipLabel,		-- The label to show on the small tip
	-- tipIcon,		-- The icon for the tip
	-- clickTrigger,	-- The trigger that fires when the tip is clicked
	-- tipLife,		-- How long the tip lasts before it disappears
	-- removeTrigger	-- Trigger to remove the tip
	-- )
	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'tutorial_jungle_tip_one',
		'/ui/icons/admin.tga',
		'helpOneDisplay',
		nil,
		'tipOneRemove'
	)

	HoN_Objectives.RegisterTip(
		'tipTwoDisplay',
		'tutorial_jungle_tip_two',
		'/ui/icons/admin.tga',
		'helpTwoDisplay',
		nil,
		'tipTwoRemove'
	)

	HoN_Objectives.RegisterTip(
		'tipThreeDisplay',
		'tutorial_jungle_tip_three',
		'/ui/icons/admin.tga',
		'helpThreeDisplay',
		nil,
		'tipThreeRemove'
	)

	-- function HoN_Objectives.RegisterDialog(
	-- dialogTrigger,	-- Trigger to show the dialog
	-- dialogTitle,	-- Title text (such as a name) for the dialog
	-- dialogText,		-- The text for the dialog
	-- dialogImage,	-- The image for the dialog
	-- dialogLife,		-- How long the dialog is visible
	-- hideTrigger		-- Trigger to hide the dialog
	-- )
	HoN_Objectives.RegisterDialog(
		'diagOneDisplay',
		'tutorial_jungle_dialog_one_title_hard',
		'tutorial_jungle_dialog_one_hard',
		'/ui/fe2/store/icons/account_icons/derp.tga',
		15000,
		'diagOneHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagTwoDisplay',
		'tutorial_jungle_dialog_two_title_hard',
		'tutorial_jungle_dialog_two_hard',
		'/ui/fe2/store/icons/account_icons/derp.tga',
		20000,
		'diagTwoHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagThreeDisplay',
		'tutorial_jungle_dialog_three_title_hard',
		'tutorial_jungle_dialog_three_hard',
		'/ui/fe2/store/icons/account_icons/derp.tga',
		15000,
		'diagThreeHide'
	)

	-- function HoN_Objectives.RegisterHelp(
	-- 	displayTrigger,		-- The trigger that causes the window to display
	-- 	title,				-- The title on the window
	-- 	image,				-- The image in the window
	-- 	body1,				-- The main body text for the window, next to the image
	--  body2,				-- The 2nd block of body text, beneath the image
	--  life,				-- Lifetime of the window before it disappears
	-- 	hideTrigger,		-- Trigger to hide the box
	-- )
	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'tutorial_jungle_help_one_title',
		'/ui/fe2/store/icons/helping_hands.tga',
		"tutorial_jungle_help_one",
		nil,
		nil
	)

	HoN_Objectives.RegisterHelp(
		'helpTwoDisplay',
		'tutorial_jungle_help_two_title',
		'/ui/fe2/store/icons/helping_hands.tga',
		"tutorial_jungle_help_two",
		nil,
		nil
	)

	HoN_Objectives.RegisterHelp(
		'helpThreeDisplay',
		'tutorial_jungle_help_three_title',
		'/ui/fe2/store/icons/helping_hands.tga',
		"tutorial_jungle_help_three",
		nil,
		nil
	)

	-- function HoN_Objectives.RegisterEventChain(
	-- 	startTrigger,	-- The trigger that starts the chain
	-- 	chainLinks,		-- Table that contains each link in the chain
	-- 	stopTrigger		-- Trigger that stops a running chain
	-- )
	HoN_Objectives.RegisterEventChain(
		'chainOneStart',
		{	[1] = {['time'] = 15000, ['triggers'] = {'diagOneDisplay', 'objOneStartTrigger', 'tipOneDisplay'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)			
			[2] = {['time'] = nil, ['triggers'] = {'diagTwoDisplay', 'wdgOneHighlight'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)			
		},
		nil
	)

	HoN_Objectives.RegisterEventChain(
		'chainTwoStart',
		{	[1] = {['time'] = nil, ['triggers'] = {'wdgTwoHighlight', 'diagThreeDisplay', 'sndOnePlay', 'tipOneRemove', 'tipTwoDisplay', 'objTwoStartTrigger', 'objThreeStartTrigger'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)
		},
		nil
	)
end
UITrigger.CreateTrigger('TutorialJungleObjectivesTrigger_Hard')
object:RegisterWatch('TutorialJungleObjectivesTrigger_Hard', Tutorial_Jungle_Hard)

-- Campaign Bridge Map
local function Camp_Bridge_Legion()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(false)
	HoN_Objectives.EnableDialog(true)
	HoN_Objectives.EnableTips(false)


	-- function HoN_Objectives.RegisterObjective(
	-- startTrigger,	-- The trigger to watch to start this objective
	-- mainLabel,		-- The main text for the objective
	-- maxValue,		-- The value we are trying to reach for any value based objects (x / 100 creep kills, etc)
	-- valueTrigger,	-- The trigger to watch to get the current progress value
	-- tipTrigger,		-- The trigger to fire for clicking for help
	-- pingScriptMessage-- The script message to send to use the map ping help system
	-- icon,			-- Any icon for the objective
	-- completeTrigger	-- The trigger to watch for the objective being complete
	-- )
	HoN_Objectives.RegisterObjective(
		'objOneStartTrigger',
		'camp_bridge_objective_one_legion',
		1750,
		'objOneValue',
		nil,
		nil,
		'/npcs/good_melee/icons/creep.tga',
		'objOneComplete'
	)
	HoN_Objectives.RegisterObjective(
		'objTwoStartTrigger',
		'camp_bridge_objective_two_legion',
		1750,
		'objTwoValue',
		nil,
		nil,
		'/npcs/good_melee/icons/creep.tga',
		'objTwoComplete'
	)

	-- function HoN_Objectives.RegisterSound(
	-- 	playTrigger,	-- Trigger that causes the sound to play
	-- 	soundPath		-- Path to the sound
	-- )
	HoN_Objectives.RegisterSound(
		'sndOnePlay',
		'/shared/sounds/announcer/startgame.ogg'
	)

	-- function HoN_Objectives.RegisterWidgetHighlight(
	-- playTrigger,	-- Trigger to cause the highlight
	-- widgetName,		-- Widget to highlight
	-- widgetInterface,-- Interface that the widget you want to highlight is in
	-- time,			-- Length of time to highlight (ms)
	-- stopTrigger,	-- Trigger to stop the effect instantly
	-- specialEffect	-- Any special effect to play on the highlight widget
	-- )


	-- function HoN_Objectives.RegisterTip(
	-- tipTrigger,		-- The trigger to add the tip
	-- tipLabel,		-- The label to show on the small tip
	-- tipIcon,		-- The icon for the tip
	-- clickTrigger,	-- The trigger that fires when the tip is clicked
	-- tipLife,		-- How long the tip lasts before it disappears
	-- removeTrigger	-- Trigger to remove the tip
	-- )


	-- function HoN_Objectives.RegisterDialog(
	-- dialogTrigger,	-- Trigger to show the dialog
	-- dialogTitle,	-- Title text (such as a name) for the dialog
	-- dialogText,		-- The text for the dialog
	-- dialogImage,	-- The image for the dialog
	-- dialogLife,		-- How long the dialog is visible
	-- hideTrigger		-- Trigger to hide the dialog
	-- )
	HoN_Objectives.RegisterDialog(
		'diagOneDisplay',
		'camp_bridge_dialog_one_title_legion',
		'camp_bridge_dialog_one_legion',
		'/npcs/good_melee/icons/creep.tga',
		8000,
		'diagOneHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagTwoDisplay',
		'camp_bridge_dialog_two_title_legion',
		'camp_bridge_dialog_two_legion',
		'/npcs/good_melee/icons/creep.tga',
		8000,
		'diagTwoHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagThreeDisplay',
		'camp_bridge_dialog_three_title_legion',
		'camp_bridge_dialog_three_legion',
		'/npcs/good_melee/icons/creep.tga',
		8000,
		'diagThreeHide'
	)

	-- function HoN_Objectives.RegisterHelp(
	-- 	displayTrigger,		-- The trigger that causes the window to display
	-- 	title,				-- The title on the window
	-- 	image,				-- The image in the window
	-- 	body1,				-- The main body text for the window, next to the image
	--  body2,				-- The 2nd block of body text, beneath the image
	--  life,				-- Lifetime of the window before it disappears
	-- 	hideTrigger,		-- Trigger to hide the box
	-- )

	-- function HoN_Objectives.RegisterEventChain(
	-- 	startTrigger,	-- The trigger that starts the chain
	-- 	chainLinks,		-- Table that contains each link in the chain
	-- 	stopTrigger		-- Trigger that stops a running chain
	-- )
	HoN_Objectives.RegisterEventChain(
		'chainOneStart',
		{	[1] = {['time'] = 9000, ['triggers'] = {'diagOneDisplay'}},
			[2] = {['time'] = 9000, ['triggers'] = {'diagTwoDisplay'}},
			[3] = {['time'] = nil, ['triggers'] = {'diagThreeDisplay'}},
		},
		nil
	)
end
UITrigger.CreateTrigger('Camp_BridgeTrigger_Legion')
object:RegisterWatch('Camp_BridgeTrigger_Legion', Camp_Bridge_Legion)

local function Camp_Bridge_Hellbourne()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(false)
	HoN_Objectives.EnableDialog(true)
	HoN_Objectives.EnableTips(false)


	-- function HoN_Objectives.RegisterObjective(
	-- startTrigger,	-- The trigger to watch to start this objective
	-- mainLabel,		-- The main text for the objective
	-- maxValue,		-- The value we are trying to reach for any value based objects (x / 100 creep kills, etc)
	-- valueTrigger,	-- The trigger to watch to get the current progress value
	-- tipTrigger,		-- The trigger to fire for clicking for help
	-- pingScriptMessage-- The script message to send to use the map ping help system
	-- icon,			-- Any icon for the objective
	-- completeTrigger	-- The trigger to watch for the objective being complete
	-- )
	HoN_Objectives.RegisterObjective(
		'objOneStartTrigger',
		'camp_bridge_objective_one_hellbourne',
		1750,
		'objOneValue',
		nil,
		nil,
		'/npcs/bad_melee/icons/creep.tga',
		'objOneComplete'
	)
	HoN_Objectives.RegisterObjective(
		'objTwoStartTrigger',
		'camp_bridge_objective_two_hellbourne',
		1750,
		'objTwoValue',
		nil,
		nil,
		'/npcs/good_melee/icons/creep.tga',
		'objTwoComplete'
	)

	-- function HoN_Objectives.RegisterSound(
	-- 	playTrigger,	-- Trigger that causes the sound to play
	-- 	soundPath		-- Path to the sound
	-- )
	HoN_Objectives.RegisterSound(
		'sndOnePlay',
		'/shared/sounds/announcer/startgame.ogg'
	)

	-- function HoN_Objectives.RegisterWidgetHighlight(
	-- playTrigger,	-- Trigger to cause the highlight
	-- widgetName,		-- Widget to highlight
	-- widgetInterface,-- Interface that the widget you want to highlight is in
	-- time,			-- Length of time to highlight (ms)
	-- stopTrigger,	-- Trigger to stop the effect instantly
	-- specialEffect	-- Any special effect to play on the highlight widget
	-- )


	-- function HoN_Objectives.RegisterTip(
	-- tipTrigger,		-- The trigger to add the tip
	-- tipLabel,		-- The label to show on the small tip
	-- tipIcon,		-- The icon for the tip
	-- clickTrigger,	-- The trigger that fires when the tip is clicked
	-- tipLife,		-- How long the tip lasts before it disappears
	-- removeTrigger	-- Trigger to remove the tip
	-- )


	-- function HoN_Objectives.RegisterDialog(
	-- dialogTrigger,	-- Trigger to show the dialog
	-- dialogTitle,	-- Title text (such as a name) for the dialog
	-- dialogText,		-- The text for the dialog
	-- dialogImage,	-- The image for the dialog
	-- dialogLife,		-- How long the dialog is visible
	-- hideTrigger		-- Trigger to hide the dialog
	-- )
	HoN_Objectives.RegisterDialog(
		'diagOneDisplay',
		'camp_bridge_dialog_one_title_hellbourne',
		'camp_bridge_dialog_one_hellbourne',
		'/npcs/bad_melee/icons/creep.tga',
		8000,
		'diagOneHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagTwoDisplay',
		'camp_bridge_dialog_two_title_hellbourne',
		'camp_bridge_dialog_two_hellbourne',
		'/npcs/bad_melee/icons/creep.tga',
		8000,
		'diagTwoHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagThreeDisplay',
		'camp_bridge_dialog_three_title_hellbourne',
		'camp_bridge_dialog_three_hellbourne',
		'/npcs/bad_melee/icons/creep.tga',
		8000,
		'diagThreeHide'
	)

	-- function HoN_Objectives.RegisterHelp(
	-- 	displayTrigger,		-- The trigger that causes the window to display
	-- 	title,				-- The title on the window
	-- 	image,				-- The image in the window
	-- 	body1,				-- The main body text for the window, next to the image
	--  body2,				-- The 2nd block of body text, beneath the image
	--  life,				-- Lifetime of the window before it disappears
	-- 	hideTrigger,		-- Trigger to hide the box
	-- )

	-- function HoN_Objectives.RegisterEventChain(
	-- 	startTrigger,	-- The trigger that starts the chain
	-- 	chainLinks,		-- Table that contains each link in the chain
	-- 	stopTrigger		-- Trigger that stops a running chain
	-- )
	HoN_Objectives.RegisterEventChain(
		'chainOneStart',
		{	[1] = {['time'] = 9000, ['triggers'] = {'diagOneDisplay'}},
			[2] = {['time'] = 9000, ['triggers'] = {'diagTwoDisplay'}},
			[3] = {['time'] = nil, ['triggers'] = {'diagThreeDisplay'}},
		},
		nil
	)
end
UITrigger.CreateTrigger('Camp_BridgeTrigger_Hellbourne')
object:RegisterWatch('Camp_BridgeTrigger_Hellbourne', Camp_Bridge_Hellbourne)

local function Prophets_Objectives()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(false)
	HoN_Objectives.EnableDialog(true)
	HoN_Objectives.EnableTips(false)


	-- function HoN_Objectives.RegisterObjective(
	-- startTrigger,	-- The trigger to watch to start this objective
	-- mainLabel,		-- The main text for the objective
	-- maxValue,		-- The value we are trying to reach for any value based objects (x / 100 creep kills, etc)
	-- valueTrigger,	-- The trigger to watch to get the current progress value
	-- tipTrigger,		-- The trigger to fire for clicking for help
	-- pingScriptMessage-- The script message to send to use the map ping help system
	-- icon,			-- Any icon for the objective
	-- completeTrigger	-- The trigger to watch for the objective being complete
	-- )


	-- function HoN_Objectives.RegisterSound(
	-- 	playTrigger,	-- Trigger that causes the sound to play
	-- 	soundPath		-- Path to the sound
	-- )


	-- function HoN_Objectives.RegisterWidgetHighlight(
	-- playTrigger,	-- Trigger to cause the highlight
	-- widgetName,		-- Widget to highlight
	-- widgetInterface,-- Interface that the widget you want to highlight is in
	-- time,			-- Length of time to highlight (ms)
	-- stopTrigger,	-- Trigger to stop the effect instantly
	-- specialEffect	-- Any special effect to play on the highlight widget
	-- )


	-- function HoN_Objectives.RegisterTip(
	-- tipTrigger,		-- The trigger to add the tip
	-- tipLabel,		-- The label to show on the small tip
	-- tipIcon,		-- The icon for the tip
	-- clickTrigger,	-- The trigger that fires when the tip is clicked
	-- tipLife,		-- How long the tip lasts before it disappears
	-- removeTrigger	-- Trigger to remove the tip
	-- )


	-- function HoN_Objectives.RegisterDialog(
	-- dialogTrigger,	-- Trigger to show the dialog
	-- dialogTitle,	-- Title text (such as a name) for the dialog
	-- dialogText,		-- The text for the dialog
	-- dialogImage,	-- The image for the dialog
	-- dialogLife,		-- How long the dialog is visible
	-- hideTrigger		-- Trigger to hide the dialog
	-- )
	HoN_Objectives.RegisterDialog(
		'diagOneDisplay',
		'prophets_dialog_title_one',
		'prophets_dialog_one',
		'/heroes/prophet/icon.tga',
		9000,
		'diagOneHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagTwoDisplay',
		'prophets_dialog_title_two',
		'prophets_dialog_two',
		'/heroes/prophet/icon.tga',
		9000,
		'diagTwoHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagThreeDisplay',
		'prophets_dialog_title_three',
		'prophets_dialog_three',
		'/heroes/prophet/icon.tga',
		9000,
		'diagThreeHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagFourDisplay',
		'prophets_dialog_title_four',
		'prophets_dialog_four',
		'/heroes/prophet/icon.tga',
		9000,
		'diagFourHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagFiveDisplay',
		'prophets_dialog_title_five',
		'prophets_dialog_five',
		'/heroes/prophet/icon.tga',
		6000,
		'diagFiveHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagSixDisplay',
		'prophets_dialog_title_six',
		'prophets_dialog_six',
		'/heroes/prophet/icon.tga',
		16000,
		'diagSixHide'
	)

	-- function HoN_Objectives.RegisterHelp(
	-- 	displayTrigger,		-- The trigger that causes the window to display
	-- 	title,				-- The title on the window
	-- 	image,				-- The image in the window
	-- 	body1,				-- The main body text for the window, next to the image
	--  body2,				-- The 2nd block of body text, beneath the image
	--  life,				-- Lifetime of the window before it disappears
	-- 	hideTrigger,		-- Trigger to hide the box
	-- )

	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'prophets_help_one_title',
		'/triggers/custom_maps/prophets/abilities/ability_tooltip/help_picture.tga',
		"prophets_help_one",
		nil,
		nil
	)
	-- function HoN_Objectives.RegisterEventChain(
	-- 	startTrigger,	-- The trigger that starts the chain
	-- 	chainLinks,		-- Table that contains each link in the chain
	-- 	stopTrigger		-- Trigger that stops a running chain
	-- )
	HoN_Objectives.RegisterEventChain(
		'chainOneStart',
		{	[1] = {['time'] = 10000, ['triggers'] = {'diagOneDisplay'}},
			[2] = {['time'] = 10000, ['triggers'] = {'diagTwoDisplay'}},
			[3] = {['time'] = nil, ['triggers'] = {'diagThreeDisplay'}},
		},
		nil
	)
end
UITrigger.CreateTrigger('Prophets_Objectives')
object:RegisterWatch('Prophets_Objectives', Prophets_Objectives)

local function Capture_The_Flag_Objectives()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(false)
	HoN_Objectives.EnableDialog(false)
	HoN_Objectives.EnableTips(true)

	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'ctf_tip_one',
		'/maps/capturetheflag/icon.tga',
		'helpOneDisplay',
		23000,
		'tipOneRemove',
		'map_ctf_tip_hide'
	)

	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'ctf_help_one_title',
		'/triggers/custom_maps/capturetheflag/help_picture.tga',
		'ctf_help_one',
		nil,
		nil,
		'map_ctf_tip_hide'
	)
end
UITrigger.CreateTrigger('Capture_The_Flag_Objectives')
object:RegisterWatch('Capture_The_Flag_Objectives', Capture_The_Flag_Objectives)

local function Caldavar_Map_Objectives()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(false)
	HoN_Objectives.EnableDialog(false)
	HoN_Objectives.EnableTips(true)

	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'caldavar_map_tip_one',
		'/maps/caldavar/icon.tga',
		'helpOneDisplay',
		20000,
		'tipOneRemove',
		'map_caldavar_tip_hide'
	)

	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'caldavar_map_help_one_title',
		'/shared/textures/minimap_caldavar.tga',
		'caldavar_map_help_one',
		nil,
		nil,
		'map_caldavar_tip_hide'
	)
end
UITrigger.CreateTrigger('Caldavar_Map_Objectives')
object:RegisterWatch('Caldavar_Map_Objectives', Caldavar_Map_Objectives)

local function Caldavar_Reborn_Map_Objectives()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(false)
	HoN_Objectives.EnableDialog(false)
	HoN_Objectives.EnableTips(true)

	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'caldavar_reborn_map_tip_one',
		'/maps/caldavar_reborn/icon.tga',
		'helpOneDisplay',
		20000,
		'tipOneRemove'
	)

	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'caldavar_reborn_map_help_one_title',
		'/maps/caldavar_reborn/minimap.tga',
		'caldavar_reborn_map_help_one',
		nil,
		nil
	)
end
UITrigger.CreateTrigger('Caldavar_Reborn_Map_Objectives')
object:RegisterWatch('Caldavar_Reborn_Map_Objectives', Caldavar_Reborn_Map_Objectives)

local function Darkwood_Vale_Objectives()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(false)
	HoN_Objectives.EnableDialog(false)
	HoN_Objectives.EnableTips(true)

	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'darkwood_vale_tip_one',
		'/maps/darkwoodvale/icon.tga',
		'helpOneDisplay',
		20000,
		'tipOneRemove',
		'map_darkwood_tip_hide'
	)

	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'darkwood_vale_help_one_title',
		'/shared/textures/minimap_darkwood.tga',
		'darkwood_vale_help_one',
		nil,
		nil,
		'map_darkwood_tip_hide'
	)
end
UITrigger.CreateTrigger('Darkwood_Vale_Objectives')
object:RegisterWatch('Darkwood_Vale_Objectives', Darkwood_Vale_Objectives)

local function Grimms_Crossing_Objectives()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(false)
	HoN_Objectives.EnableDialog(false)
	HoN_Objectives.EnableTips(true)

	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'grimms_crossing_tip_one',
		'/maps/grimmscrossing/icon.tga',
		'helpOneDisplay',
		20000,
		'tipOneRemove',
		'map_grimmcross_tip_hide'
	)

	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'grimms_crossing_help_one_title',
		'/shared/textures/minimap_grimms.tga',
		'grimms_crossing_help_one',
		nil,
		nil,
		'map_grimmcross_tip_hide'
	)
end
UITrigger.CreateTrigger('Grimms_Crossing_Objectives')
object:RegisterWatch('Grimms_Crossing_Objectives', Grimms_Crossing_Objectives)

local function Midwars_Objectives()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(false)
	HoN_Objectives.EnableDialog(false)
	HoN_Objectives.EnableTips(true)

	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'midwars_tip_one',
		'/maps/midwars/icon.tga',
		'helpOneDisplay',
		20000,
		'tipOneRemove',
		'map_midwars_tip_hide'
	)

	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'midwars_help_one_title',
		'/shared/textures/minimap_midwars.tga',
		'midwars_help_one',
		nil,
		nil,
		'map_midwars_tip_hide'
	)
end
UITrigger.CreateTrigger('Midwars_Objectives')
object:RegisterWatch('Midwars_Objectives', Midwars_Objectives)

local function Midwars_Reborn_Objectives()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(false)
	HoN_Objectives.EnableDialog(false)
	HoN_Objectives.EnableTips(true)

	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'midwars_reborn_tip_one',
		'/maps/midwars/icon.tga',
		'helpOneDisplay',
		20000,
		'tipOneRemove'
	)

	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'midwars_reborn_help_one_title',
		'/shared/textures/minimap_midwars_reborn.tga',
		'midwars_reborn_help_one',
		nil,
		nil
	)
end
UITrigger.CreateTrigger('Midwars_Reborn_Objectives')
object:RegisterWatch('Midwars_Reborn_Objectives', Midwars_Reborn_Objectives)

local function Midwars_Beta_Objectives()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(false)
	HoN_Objectives.EnableDialog(false)
	HoN_Objectives.EnableTips(true)

	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'midwars_beta_tip_one',
		'/ui/icons/midwars_beta.tga',
		'helpOneDisplay',
		20000,
		'tipOneRemove'
	)

	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'midwars_beta_help_one_title',
		'/shared/textures/minimap_midwars.tga',
		'midwars_beta_help_one',
		nil,
		nil
	)
end
UITrigger.CreateTrigger('Midwars_Beta_Objectives')
object:RegisterWatch('Midwars_Beta_Objectives', Midwars_Beta_Objectives)

local function Riftwars_Objectives()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(false)
	HoN_Objectives.EnableDialog(false)
	HoN_Objectives.EnableTips(true)

	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'riftwars_tip_one',
		'/maps/riftwars/icon.tga',
		'helpOneDisplay',
		20000,
		'tipOneRemove',
		'map_riftwars_tip_hide'
	)

	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'riftwars_help_one_title',
		'/maps/riftwars/minimap.tga',
		'riftwars_help_one',
		nil,
		nil,
		'map_riftwars_tip_hide'
	)
end
UITrigger.CreateTrigger('Riftwars_Objectives')
object:RegisterWatch('Riftwars_Objectives', Riftwars_Objectives)

local function The_Grimm_Hunt_Objectives()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(false)
	HoN_Objectives.EnableDialog(false)
	HoN_Objectives.EnableTips(true)

	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'the_grimm_hunt_tip_one',
		'/maps/thegrimmhunt/icon.tga',
		'helpOneDisplay',
		20000,
		'tipOneRemove',
		'map_grimmhunt_tip_hide'
	)

	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'the_grimm_hunt_help_one_title',
		'/maps/thegrimmhunt/minimap.tga',
		'the_grimm_hunt_help_one',
		nil,
		nil,
		'map_grimmhunt_tip_hide'
	)
end
UITrigger.CreateTrigger('The_Grimm_Hunt_Objectives')
object:RegisterWatch('The_Grimm_Hunt_Objectives', The_Grimm_Hunt_Objectives)

local function Watch_Tower_Objectives()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(false)
	HoN_Objectives.EnableDialog(false)
	HoN_Objectives.EnableTips(true)

	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'watch_tower_tip_one',
		'/maps/watchtower/icon.tga',
		'helpOneDisplay',
		20000,
		'tipOneRemove',
		'map_watch_tip_hide'
	)

	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'watch_tower_help_one_title',
		'/shared/textures/watchtower_help.tga',
		'watch_tower_help_one',
		nil,
		nil,
		'map_watch_tip_hide'
	)
end
UITrigger.CreateTrigger('Watch_Tower_Objectives')
object:RegisterWatch('Watch_Tower_Objectives', Watch_Tower_Objectives)

local function Hero_Wars_Objectives()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(false)
	HoN_Objectives.EnableDialog(false)
	HoN_Objectives.EnableTips(true)

	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'hero_wars_tip_one',
		'/maps/devowars/icon.tga',
		'helpOneDisplay',
		20000,
		'tipOneRemove',
		'map_herowars_tip_hide'
	)

	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'hero_wars_help_one_title',
		'/maps/devowars/minimap_large.tga',
		'hero_wars_help_one',
		nil,
		nil,
		'map_herowars_tip_hide'
	)
end
UITrigger.CreateTrigger('Hero_Wars_Objectives')
object:RegisterWatch('Hero_Wars_Objectives', Hero_Wars_Objectives)

local function Soccer_Objectives()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(false)
	HoN_Objectives.EnableDialog(false)
	HoN_Objectives.EnableTips(true)

	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'soccer_tip_one',
		'/maps/soccer/icon.tga',
		'helpOneDisplay',
		20000,
		'tipOneRemove'
	)

	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'soccer_help_one_title',
		'/maps/soccer/minimap.tga',
		'soccer_help_one',
		nil,
		nil
	)
end
UITrigger.CreateTrigger('Soccer_Objectives')
object:RegisterWatch('Soccer_Objectives', Soccer_Objectives)

local function Team_Deathmatch_Objectives()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(false)
	HoN_Objectives.EnableDialog(false)
	HoN_Objectives.EnableTips(true)

	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'team_deathmatch_tip_one',
		'/maps/team_deathmatch/icon.tga',
		'helpOneDisplay',
		20000,
		'tipOneRemove'
	)

	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'team_deathmatch_help_one_title',
		'/maps/team_deathmatch/minimap_large.tga',
		'team_deathmatch_help_one',
		nil,
		nil
	)
end
UITrigger.CreateTrigger('Team_Deathmatch_Objectives')
object:RegisterWatch('Team_Deathmatch_Objectives', Team_Deathmatch_Objectives)

local function Onev1_Map_Objectives()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(false)
	HoN_Objectives.EnableDialog(false)
	HoN_Objectives.EnableTips(true)

	HoN_Objectives.RegisterTip(
		'tipOneDisplay',
		'Onev1_map_tip_one',
		'/maps/caldavar/icon.tga',
		'helpOneDisplay',
		20000,
		'tipOneRemove'
	)

	HoN_Objectives.RegisterHelp(
		'helpOneDisplay',
		'Onev1_map_help_one_title',
		'/shared/textures/minimap_caldavar.tga',
		'Onev1_map_help_one',
		nil,
		nil
	)
end
UITrigger.CreateTrigger('Onev1_Map_Objectives')
object:RegisterWatch('Onev1_Map_Objectives', Onev1_Map_Objectives)

local function Reaper_Mode_Objectives()
	-- Start by resetting the objectives stuff, this will clear any previously running thing out
	HoN_Objectives.TurnOn()

	-- Start by enabling quests, tips, and dialogs if the interface is going to be using them
	-- This will position things in the interface as well as making them visible
	HoN_Objectives.EnableQuests(false)
	HoN_Objectives.EnableDialog(true)
	HoN_Objectives.EnableTips(false)


	-- function HoN_Objectives.RegisterObjective(
	-- startTrigger,	-- The trigger to watch to start this objective
	-- mainLabel,		-- The main text for the objective
	-- maxValue,		-- The value we are trying to reach for any value based objects (x / 100 creep kills, etc)
	-- valueTrigger,	-- The trigger to watch to get the current progress value
	-- tipTrigger,		-- The trigger to fire for clicking for help
	-- pingScriptMessage-- The script message to send to use the map ping help system
	-- icon,			-- Any icon for the objective
	-- completeTrigger	-- The trigger to watch for the objective being complete
	-- )
	HoN_Objectives.RegisterObjective(
		'objOneStartTrigger',
		'challenge_platformer_objective_one',
		nil,
		nil,
		nil,
		nil,
		'/ui/icons/ability_abuse.tga',
		'objOneComplete'
	)

	-- function HoN_Objectives.RegisterSound(
	-- 	playTrigger,	-- Trigger that causes the sound to play
	-- 	soundPath		-- Path to the sound
	-- )
	HoN_Objectives.RegisterSound(
		'sndOnePlay',
		'/shared/sounds/announcer/startgame.ogg'
	)

	-- function HoN_Objectives.RegisterWidgetHighlight(
	-- playTrigger,	-- Trigger to cause the highlight
	-- widgetName,		-- Widget to highlight
	-- widgetInterface,-- Interface that the widget you want to highlight is in
	-- time,			-- Length of time to highlight (ms)
	-- stopTrigger,	-- Trigger to stop the effect instantly
	-- specialEffect	-- Any special effect to play on the highlight widget
	-- )
	HoN_Objectives.RegisterWidgetHighlight(
		'wdgOneHighlight',
		'game_info_courier_button',
		'game',
		15000,	-- nil makes it stay forever
		'wdgOneHighlightStop',
		nil
	)

	HoN_Objectives.RegisterWidgetHighlight(
		'wdgTwoHighlight',
		'panel_abilities',
		'game',
		10000,	-- nil makes it stay forever
		'wdgTwoHighlightStop',
		nil
	)

	-- function HoN_Objectives.RegisterDialog(
	-- dialogTrigger,	-- Trigger to show the dialog
	-- dialogTitle,	-- Title text (such as a name) for the dialog
	-- dialogText,		-- The text for the dialog
	-- dialogImage,	-- The image for the dialog
	-- dialogLife,		-- How long the dialog is visible
	-- hideTrigger		-- Trigger to hide the dialog
	-- )
	HoN_Objectives.RegisterDialog(
		'diagOneDisplay',
		'reaper_mode_dialog_title_one',
		'reaper_mode_dialog_one',
		'/heroes/ellonia/ability_03/icon.tga',
		10000,
		'diagOneHide'
	)

	HoN_Objectives.RegisterDialog(
		'diagTwoDisplay',
		'reaper_mode_dialog_title_two',
		'reaper_mode_dialog_two',
		'/heroes/nomad/alt4/icon.tga',
		5000,
		'diagTwoHide'
	)
	
	HoN_Objectives.RegisterDialog(
		'diagThreeDisplay',
		'reaper_mode_dialog_title_three',
		'reaper_mode_dialog_three',
		'/heroes/nomad/alt4/icon.tga',
		5000,
		'diagThreeHide'
	)
	
	HoN_Objectives.RegisterDialog(
		'diagFourDisplay',
		'reaper_mode_dialog_title_four',
		'reaper_mode_dialog_four',
		'/heroes/nomad/alt6/icon.tga',
		5000,
		'diagFourHide'
	)
	
	HoN_Objectives.RegisterDialog(
		'diagFiveDisplay',
		'reaper_mode_dialog_title_five',
		'reaper_mode_dialog_five',
		'/heroes/nomad/alt6/icon.tga',
		5000,
		'diagFiveHide'
	)
	
	HoN_Objectives.RegisterDialog(
		'diagSixDisplay',
		'reaper_mode_dialog_title_six',
		'reaper_mode_dialog_six',
		'/heroes/nomad/alt6/icon.tga',
		5000,
		'diagSixHide'
	)
	
	HoN_Objectives.RegisterDialog(
		'diagSevenDisplay',
		'reaper_mode_dialog_title_seven',
		'reaper_mode_dialog_seven',
		'/heroes/nomad/alt6/icon.tga',
		5000,
		'diagSevenHide'
	)

	-- function HoN_Objectives.RegisterEventChain(
	-- 	startTrigger,	-- The trigger that starts the chain
	-- 	chainLinks,		-- Table that contains each link in the chain
	-- 	stopTrigger		-- Trigger that stops a running chain
	-- )
	HoN_Objectives.RegisterEventChain(
		'chainOneStart',
		{	[1] = {['time'] = 10000, ['triggers'] = {'diagOneDisplay'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)
		},
		nil
	)
	HoN_Objectives.RegisterEventChain(
		'chainTwoStart',
		{	[1] = {['time'] = 5000, ['triggers'] = {'diagTwoDisplay'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)
		},
		nil
	)
	HoN_Objectives.RegisterEventChain(
		'chainThreeStart',
		{	[1] = {['time'] = 5000, ['triggers'] = {'diagThreeDisplay'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)
		},
		nil
	)
	HoN_Objectives.RegisterEventChain(
		'chainFourStart',
		{	[1] = {['time'] = 5000, ['triggers'] = {'diagFourDisplay'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)
		},
		nil
	)
	HoN_Objectives.RegisterEventChain(
		'chainFiveStart',
		{	[1] = {['time'] = 5000, ['triggers'] = {'diagFiveDisplay'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)
		},
		nil
	)
	HoN_Objectives.RegisterEventChain(
		'chainSixStart',
		{	[1] = {['time'] = 5000, ['triggers'] = {'diagSixDisplay'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)
		},
		nil
	)
	HoN_Objectives.RegisterEventChain(
		'chainSevenStart',
		{	[1] = {['time'] = 5000, ['triggers'] = {'diagSevenDisplay'}},	-- time for link to take, triggers are what triggers it fires (to start other elements)
		},
		nil
	)
end
UITrigger.CreateTrigger('Reaper_Mode_Objectives')
object:RegisterWatch('Reaper_Mode_Objectives', Reaper_Mode_Objectives)