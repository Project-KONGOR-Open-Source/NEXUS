-- System notifications test suite

if systemNotifications then

	function systemNotificationTestGift()
		systemNotifications:newMessage({
			subject		= 'sysmessage_product_gift',
			icon		= '/ui/icons/gift_red.tga',
			expiration	= 1446768333,
			crc			= 'aaaaaaaaaaa',
			meta		= {
				productType		= 'aa',
				gifterNick		= 'FBMerc',
				subtitle	= 'sysmessage_product_gift_body',
				productId	= '1282'
			},
			isTest		= true
		}, nil, true)
	end

	function systemNotificationTestProductRemoved()
		systemNotifications:newMessage({
			subject		= 'sysmessage_product_removed',
			icon		= '/ui/icons/alert_green.tga',
			expiration	= 1446768331,
			crc			= 'aaaaaaaaaab',
			meta		= {
				productType		= 'aa',
				productId	= '1282'

			},
			isTest		= true
		}, nil, true)
	end

	function systemNotificationTestProductAdded()
		systemNotifications:newMessage({
			subject		= 'sysmessage_product_system',
			icon		= '/ui/icons/gift_blue.tga',
			expiration	= 1446768331,
			crc			= 'aaaaaaaaaac',
			meta		= {
				productType		= 'aa',
				subtitle	= 'sysmessage_product_system_body',
				productId	= '1282'
			},
			isTest		= true
		}, nil, true)
	end

	function systemNotificationTestGiftSilver()
		systemNotifications:newMessage({
			subject		= 'sysmessage_silver_friend',
			icon		= '/ui/icons/silver_coin_stack.tga',
			expiration	= 1446768333,
			crc			= 'aaaaaaaaaad',
			meta		= {
				body	= 'sysmessage_silver_friend_body',
				gifterNick	= 'merctest56',
				amount	= tostring(math.random(25, 250))
			},
			isTest		= true
		}, nil, true)
	end

	function systemNotificationTestSilver()
		systemNotifications:newMessage({
			subject		= 'sysmessage_silver_system_add',
			icon		= '/ui/icons/silver_coin_stack.tga',
			expiration	= 1446768333,
			crc			= 'aaaaaaaaaae',
			meta		= {
				body	= 'sysmessage_silver_system_add_body',
				amount	= tostring(math.random(25, 250))
			},
			isTest		= true
		}, nil, true)
	end

	function systemNotificationTestSilverRemove()
		systemNotifications:newMessage({
			subject		= 'sysmessage_silver_system_remove',
			icon		= '/ui/icons/alert_green.tga',
			expiration	= 1446768333,
			crc			= 'aaaaaaaaaaf',
			meta		= {
				body	= 'sysmessage_silver_system_remove_body',
				amount	= tostring(math.random(25, 250))
			},
			isTest		= true
		}, nil, true)
	end

	function systemNotificationTestGiftGold()
		systemNotifications:newMessage({
			subject		= 'sysmessage_gold_friend',
			icon		= '/ui/icons/gold_coin_stack.tga',
			expiration	= 1446768333,
			crc			= 'aaaaaaaaaag',
			meta		= {
				body	= 'sysmessage_gold_friend_body',
				amount	= tostring(math.random(25, 250)),
				gifterNick	= 'merctest101'
			},
			isTest		= true
		}, nil, true)
	end

	function systemNotificationTestGold()
		systemNotifications:newMessage({
			subject		= 'sysmessage_gold_system_add',
			icon		= '/ui/icons/gold_coin_stack.tga',
			expiration	= 1446768333,
			crc			= 'aaaaaaaaaah',
			meta		= {
				body	= 'sysmessage_gold_system_add_body',
				amount	= tostring(math.random(25, 250))
			},
			isTest		= true
		}, nil, true)
	end

	function systemNotificationTestGoldRemove()
		systemNotifications:newMessage({
			subject		= 'sysmessage_gold_system_remove',
			icon		= '/ui/icons/alert_green.tga',
			expiration	= 1446768333,
			crc			= 'aaaaaaaaaai',
			meta		= {
				body	= 'sysmessage_gold_system_remove_body',
				amount	= tostring(math.random(25, 250))
			},
			isTest		= true
		}, nil, true)
	end

	function systemNotificationTestGiftTickets()
		systemNotifications:newMessage({
			subject		= 'sysmessage_tickets_friend',
			icon		= '/ui/icons/tickets_stack.tga',
			expiration	= 1446768333,
			crc			= 'aaaaaaaaaaj',
			meta		= {
				body	= 'sysmessage_tickets_friend_body',
				gifterNick	= 'merctest102',
				amount	= tostring(math.random(25, 250))
			},
			isTest		= true
		}, nil, true)
	end

	function systemNotificationTestTickets()
		systemNotifications:newMessage({
			subject		= 'sysmessage_tickets_system_add',
			icon		= '/ui/icons/tickets_stack.tga',
			expiration	= 1446768333,
			crc			= 'aaaaaaaaaak',
			meta		= {
				body	= 'sysmessage_tickets_system_add_body',
				amount	= tostring(math.random(25, 250))
			},
			isTest		= true
		}, nil, true)
	end

	function systemNotificationTestTicketsRemove()
		systemNotifications:newMessage({
			subject		= 'sysmessage_tickets_system_remove',
			icon		= '/ui/icons/alert_green.tga',
			expiration	= 1446768333,
			crc			= 'aaaaaaaaaal',
			meta		= {
				body	= 'sysmessage_tickets_system_remove_body',
				amount	= tostring(math.random(25, 250))
			},
			isTest		= true
		}, nil, true)
	end

	function systemNotificationTestEvent()
		systemNotifications:newMessage({
			subject		= 'Mercenary Legionnaire Event',
			icon		= '/heroes/legionnaire/icon.tga',
			expiration	= 1446768333,
			crc			= 'aaaaaaaaaam',
			meta		= {
				body		= "Hello [PLAYERNAME], we've announced the Mercenary Legionnaire release event.  Check it out November 20th.  Good luck if you don't speak English cause this won't be translated.",
				image		= 'http://1.bp.blogspot.com/-pCYJKyFLBtg/Vk83JW6W8bI/AAAAAAAABDU/3MnY6OoTS_0/s1600/legion-MOTD-2.jpg',
				imageWidth	= '43.5h',
				imageHeight = '30h',
				footer		= "sysmessage_footer_website"
			},
			isTest		= true
		}, nil, true)
	end

	function systemNotificationTestDowntime()
		systemNotifications:newMessage({
			subject		= 'sysmessage_downtime',
			expiration	= 1446768000,
			-- ['local']	= true,		-- we'll always be translating these
			crc			= 'aaaaaaaaaan',
			meta		= {
				body	= "sysmessage_downtime_body",
				footer	= "sysmessage_footer_website"
			},
			icon		= '/ui/icons/alert_yellow.tga',
			isTest		= true
		}, nil, true)
	end

	local spamThread = nil

	function systemNotificationsTestSpam(doWait, waitMaxAdd)
		if doWait == nil then doWait = true end
		local waitMaxAdd = waitMaxAdd or 0

		if spamThread ~= nil then
			spamThread:Kill()
			spamThread = nil
		end

		spamThread = newthread(function()
			spamList = {
				systemNotificationTestGift,
				systemNotificationTestGiftSilver,
				systemNotificationTestSilver,
				systemNotificationTestSilverRemove,
				systemNotificationTestGiftGold,
				systemNotificationTestGold,
				systemNotificationTestGoldRemove,
				systemNotificationTestGiftTickets,
				systemNotificationTestTickets,
				systemNotificationTestTicketsRemove,
				systemNotificationTestEvent,
				systemNotificationTestProductRemoved,
				systemNotificationTestProductAdded,
				systemNotificationTestDowntime
			}

			for k,v in ipairs(spamList) do
				v()
				if doWait then
					wait(math.random(2500, 6000 + waitMaxAdd))
				end
			end


			spamThread = nil
		end)

	end

	function systemNotificationsClearCache()
		GetDBEntry('systemNotificationsCache', {}, true, false, false)
	end

end