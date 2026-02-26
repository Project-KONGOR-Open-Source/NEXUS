QA = {}
QA.current = {}
QA.checklists = {}

-------------------------------------------------------------------------------
--						*** ONLY EDIT BELOW THIS LINE  ***				 	 --
-------------------------------------------------------------------------------

-- CURRENT_TEST_ITEM_LIST_VERSION: You should increment this every week when you add or remove items from the currently tested list
QA.CURRENT_TEST_ITEM_LIST_VERSION 	= 9

--[[ Heroes / Items
	 Insert objects in the format 
		{'Entity_Name', {'Additional checklist item 1', 'Additional checklist item 2', 'Additional checklist item 3'} },
	 You may use up to 100 additional checklist items per object, these are added at the end of the default list
	 You may use zero additional checklist items. You must use correct syntax.
--]]

QA.current.hero = {
	-- *** Edit BELOW this line ***
	--{'Hero_Gunblade', 			{'Make sure his gun has a blade on it.', 'Test that he can shoot stuff.', 'Test that he can blade faces.'} },
	--{'Hero_Blitz', 				{} },
	--{'Hero_Artillery', 			{} },
	--{'Hero_Aids', 				{} },
	
	-- *** Edit ABOVE this line ***
	--{'Hero_Bangerz', 			{'Make sure Bangerz currently has a Red Bull.', 'Make sure Bangerz has eaten today.', 'Make sure Bangerz goes to sleep this week.'} },
	nil
}

QA.current.item = {
	-- *** Edit BELOW this line ***	
	--{'Item_HealthPotion', 			{'Make sure health potion heals you.', 'Make sure taking damage removes health potion.'} },
	-- *** Edit ABOVE this line ***
	--{'Item_HealthPotion', 			{'Make sure health potion heals you.', 'Make sure taking damage removes health potion.'} },
	nil
}

--[[ Alt Avatars / Taunts / Announcers / Couriers / Icons / Symbols / Name Colors / Other store items
	 Insert objects in the format 
		{ {'Entity_Name', 'Display Name', 'Icon Path'}, {'Additional checklist item 1', 'Additional checklist item 2', 'Additional checklist item 3'} },
	 You may use up to 100 additional checklist items per object, these are added at the end of the default list
	 You may use zero additional checklist items. You must use correct syntax.
--]]

QA.current.avatar = {
	-- *** Edit BELOW this line ***
	{{'Hero_Chronos',		'Patience Chronos',		'/heroes/chronos/alt5/icon.tga'},		{'Do the visuals work when Chronos has Staff of the Master?'},		{'Does ability 4 function properly with Staff of the Master?'},		{'Does the icon when you stun someone with ability 3 display on the enemy hero?'},		{'Does ths icon correctly display when you slow the enemy hero with ability 1?'}},
	{{'Hero_Xalynx',		'Kindness Torturer',		'/heroes/xalynx/alt4/icon.tga'},		{'Do the visuals work when Chronos has Staff of the Master?'}, 				{'Does ability 4 function properly with Staff of the Master?'}},
	{{'Hero_Flux',		'Jupiter Flux',		'/heroes/flux/alt3/icon.tga'},		{'Does ths icon correctly display on the enemy when you slow the enemy hero with ability 1 in both forms?'}},
	{{'Hero_Tempest',		'Poseidon Tempest',		'/heroes/tempest/alt3/icon.tga'},		{}},
	{{'Hero_Artillery',		'Apollo Artillery',		'/heroes/artillery/alt4/icon.tga'},		{}},
	
	-- *** Edit ABOVE this line ***
	nil
}
QA.current.taunt = {
	-- *** Edit BELOW this line ***
	--{{'fortunecookie_taunt', 		'Fortune Cookie Taunt', 		'/ui/fe2/store/icons/taunt_fortune_cookie.tga'}, 			{'Does it say a different taunt each time?'} },
	-- *** Edit ABOVE this line ***
	nil
}
QA.current.announcer = {
	-- *** Edit BELOW this line ***
	
	-- *** Edit ABOVE this line ***
	nil
}
QA.current.courier = {
	-- *** Edit BELOW this line ***
	
	-- *** Edit ABOVE this line ***
	nil
}
QA.current.icon = {
	-- *** Edit BELOW this line ***
	--{{'namecolor_1', 		'Emerald Green Icon', 			'/ui/icons/emerald.tga'}, 				{'Make sure its green.'} },
	-- *** Edit ABOVE this line ***
	nil
}
QA.current.symbol = {
	-- *** Edit BELOW this line ***
	--{{'Chupacabra', 		'Hero Alt- Chupacabra', 			'/heroes/scar/alt3/icon.tga'}, 				{'Does this show up.'} },
	-- *** Edit ABOVE this line ***
	nil
}
QA.current.namecolor = {
	-- *** Edit BELOW this line ***
	--{{'namecolor_1', 		'Emerald Green Color', 			'/ui/icons/emerald.tga'}, 				{'Make sure its green.'} },
	-- *** Edit ABOVE this line ***
	nil
}
QA.current.store = {
	-- *** Edit BELOW this line ***
	
	-- *** Edit ABOVE this line ***
	nil
}
--[[ Other (Balance change, etc)
	 Insert objects in the format 
		{ {'Entity_Name', 'Display Name', 'Icon Path'}, {'Additional checklist item 1', 'Additional checklist item 2', 'Additional checklist item 3'} },
		OR
		{'Entity_Name', {'Additional checklist item 1', 'Additional checklist item 2', 'Additional checklist item 3'} },
	There is no default checklist for this category. You must add all checklist items per item.
--]]

-- In game items
QA.current.other = {
	-- *** Edit BELOW this line ***
	--{'Hero_Arachna', 																{'Make sure buffed web shot can still shot web.'} },
	--{{'General_Report', 	'Other Bug', 		'/heroes/pestilence/icon.tga'},		{'Generic bug report.'} },	
	-- *** Edit ABOVE this line ***
	--{'Hero_Arachna', 			{'Make sure buffed web shot can still shot web.'} },
	nil
}

-- Out of game items
QA.current.other2 = {
	-- *** Edit BELOW this line ***
	
	-- *** Edit ABOVE this line ***
	nil
}

-------------------------------------------------------------------------------
--						    	DEFAULT CHECKLISTS							 --
-- 		Modifying any of the follow will change the checklist 				 --
-- 					for ALL test objects of that type. 						 --
-------------------------------------------------------------------------------

--	DEFAULT_CHECKLIST_VERSION: You should increment this only if you alter the *INDEXES* of the default checklist (ie by adding or removing items to/from the anywhere but the end of the list). 
--	This will break/reset all existing report indexes. Be *SURE* about doing this.
QA.DEFAULT_CHECKLIST_VERSION 		= 3

-- Hero checklist. 
QA.checklists.hero = {
	[[Ability 1 of this hero has all of its effects, sounds and icons - including buff icons.]],
	[[Ability 1 of this hero is functioning correctly.]],
	[[Ability 2 of this hero has all of its effects, sounds and icons - including buff icons.]],
	[[Ability 2 of this hero is functioning correctly.]],
	[[Ability 3 of this hero has all of its effects, sounds and icons - including buff icons.]],
	[[Ability 3 of this hero is functioning correctly.]],
	[[Ability 4 of this hero has all of its effects, sounds and icons - including buff icons.]],
	[[Ability 4 of this hero is functioning correctly.]],	
	[[This hero has all of its sounds, and they are working correctly.]],
	[[This hero has all of its textures, and they are working correctly.]],
	[[This hero has all of its effects, and they are working correctly.]],
	[[This hero has all of its models, and they are working correctly.]],
	[[This hero has all of its icons, and they are working correctly.]],
	[[The avatar interacts correctly with barbed armor]],
	[[The avatar interacts correctly with void talisman]],
	[[The avatar interacts correctly with null stone]],
	[[The avatar interacts correctly with shrunken head]],	
	[[This hero has no other bugs.]]
	}
	
QA.checklists.item = {	

	}
	
-- Alt avatar checklist. 
QA.checklists.avatar = {
	[[Ability 1 of this avatar has all of its effects.]],
	[[Ability 1 of this avatar has all of its sounds.]],
	[[Ability 1 of this avatar is functioning correctly.]],
	[[Ability 1 of this avatar animation is smooth and in sync.]],
	[[Ability 1 of this avatar simple and detailed tooltips are correct.]],
	[[Ability 2 of this avatar has all of its effects.]],
	[[Ability 2 of this avatar has all of its sounds.]],
	[[Ability 2 of this avatar is functioning correctly.]],
	[[Ability 2 of this avatar animation is smooth and in sync.]],
	[[Ability 2 of this avatar simple and detailed tooltips are correct.]],
	[[Ability 3 of this avatar has all of its effects.]],
	[[Ability 3 of this avatar has all of its sounds.]],
	[[Ability 3 of this avatar is functioning correctly.]],
	[[Ability 3 of this avatar animation is smooth and in sync.]],
	[[Ability 3 of this avatar simple and detailed tooltips are correct.]],
	[[Ability 4 of this avatar has all of its effects.]],
	[[Ability 4 of this avatar has all of its sounds.]],
	[[Ability 4 of this avatar is functioning correctly.]],
	[[Ability 4 of this avatar animation is smooth and in sync.]],
	[[Ability 4 of this avatar simple and detailed tooltips are correct.]],
	[[This avatar has all of his animations, and they are working correctly.]],
	[[This avatar has all of its sounds, and they are working correctly.]],
	[[This avatar has all of its textures, and they are working correctly.]],
	[[This avatar has all of its effects, and they are working correctly.]],
	[[This avatar has all of its models, and they are working correctly.]],
	[[This avatar has its 3d portrait, and working correct.]],
	[[This avatar has all of its ability icons, and they are working correctly.]],
	[[This avatar has not effected any of the other alt avatars.]],
	[[The avatar interacts correctly with Barbed Armor]],
	[[The avatar interacts correctly with Void Talisman]],
	[[The avatar interacts correctly with Null Stone]],
	[[The avatar interacts correctly with Assassin's Shroud]],
	[[The avatar interacts correctly with Shrunken Head]],
	[[The avatar interacts correctly with Bound Eye]],
	[[The avatar interacts correctly with Geometers Bane]],
	[[The avatar interacts correctly with all of the Runes]],
	[[This avatar has no other bugs.]]
	}
	
QA.checklists.taunt = {	
	[[This taunt is not missing any textures.]],
	[[This taunt is not missing any sounds.]],
	[[This taunt has its icon in the store when it was purchased.]]
	}
	
QA.checklists.announcer = {	

	}

QA.checklists.courier = {	

	}
QA.checklists.store = {	

	}
QA.checklists.matchmaking = {	 			
	--[[Do all of the option buttons seem to work correctly?]]--
	--[[Are the correct options disabled/enabled on basic, verified and legacy accounts?]]--
	--[[Are you placed in the correct game mode, game type, and map?]]--
	--[[Do the competitive mode restrictions and notifications work correctly?]]--
}

QA.checklists.chatserver = {	

}	
	
QA.checklists.ui = {}
QA.checklists.other = {}	
QA.checklists.ui2 = {}
QA.checklists.other2 = {}	
	
-------------------------------------------------------------------------------
--						*** ONLY EDIT ABOVE THIS LINE ***					 --
-------------------------------------------------------------------------------

QA.current.ui = {
	nil
}

QA.current.ui2 = {
	nil
}
QA.current.chatserver = {
	nil
}
QA.current.matchmaking = {
	nil
	--{{'UI_Feature_8', 	'Matchmaking Window', 		'/ui/fe2/mainmenu/icons/N_matchmaking.tga'},		{'Any other bugs?'} },
}


