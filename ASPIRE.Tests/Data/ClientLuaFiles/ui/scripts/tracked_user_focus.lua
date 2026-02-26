----------------------------------------------------------
--	Name: 		Tracked User Focus	            		--
--  Copyright 2015 Frostburn Studios					--
----------------------------------------------------------

trackedUserFocus = trackedUserFocus or {}

trackedUserFocus.currentFocus	= {}
trackedUserFocus.lastFocus		= {}
trackedUserFocus.clearFocus		= {}
trackedUserFocus.updateFocus	= {}
trackedUserFocus.updated		= false

trackedUserFocus.hoverWidget	= nil
trackedUserFocus.hoverDuration	= 0

function trackedUserFocus:evaluateHover()
end

function trackedUserFocus:getPrimaryFocus()
	-- local gamePhase = AtoN(GetWidget('trackedUserFocus'):UICmd('GetCurrentGamePhase()'))

	if self.currentFocus['walkthroughPromptVisible'] then
		return 'walkthroughPromptVisible'
	end

	if self.currentFocus['viewingModdedFiles'] then
		return 'viewingModdedFiles'
	end

	if self.currentFocus['viewingEula'] then
		return 'viewingEula'
	end

	if self.currentFocus['gameChatVisible'] or self.currentFocus['gameChatFocus'] then
		return 'gameChat'
	end

	if self.currentFocus['shopOpen'] then
		return 'shopOpen'
	end

	if self.currentFocus['inMMQueue'] then
		if self.currentFocus['matchmakingType'] == 'coop' then
			return 'matchmakingQueued_coop'
		elseif self.currentFocus['matchmakingType'] == 'pvp' then
			return 'matchmakingQueued_pvp'
		else
			return 'matchmakingQueued_season'
		end
	end

	if self.currentFocus['mainMenuPanel'] and self.currentFocus['mainMenuPanel'] == 'player_stats' then
		if self.currentFocus['playerStatsView'] then
			return 'playerStatsView'
		end
	end

	if self.currentFocus['mainMenuPanel'] and self.currentFocus['mainMenuPanel'] == 'match_stats' then
		if self.currentFocus['matchStatsView'] then
			return 'matchStatsView'..self.currentFocus['matchStatsView']
		end
		return 'matchStatsViewstats'
	end

	if self.currentFocus['mainMenuPanel'] and self.currentFocus['mainMenuPanel'] == 'player_ladder' then
		if self.currentFocus['playerLadder'] then
			return 'playerLadder'..self.currentFocus['playerLadder']
		end
	end

	if self.currentFocus['mainMenuPanel'] and self.currentFocus['mainMenuPanel'] == 'plinko' then
		if self.currentFocus['plinkoTicketRedemption'] then
			return 'plinkoTicketRedemption'
		end
	end

	if self.currentFocus['mainMenuPanel'] and self.currentFocus['mainMenuPanel'] == 'game_options' then
		if self.currentFocus['optionsCategory'..self.currentFocus['optionsCategory']..'sub'] then
			return 'game_options_category_'..self.currentFocus['optionsCategory']..'sub'..self.currentFocus['optionsCategory'..self.currentFocus['optionsCategory']..'sub']
		elseif self.currentFocus['optionsCategory'] then
			return 'game_options_category_'..self.currentFocus['optionsCategory']
		end
	end

	local store = GetCvarBool('cg_store2_') and 'store_container2' or 'store_container'
	if self.currentFocus['mainMenuPanel'] and self.currentFocus['mainMenuPanel'] == store then

		if self.currentFocus['buyCoinsOpen'] then
			return 'storeBuyCoinsOpen'
		end

		if self.currentFocus['redeemCodeOpen'] then
			return 'storeRedeemCodeOpen'
		end

		if self.currentFocus['storeSpecials'] then
			return 'storeSpecials'
		end

		if self.currentFocus['storeCategory'] and self.currentFocus['storeCurrentPage'] then
			return 'goblinStoreCategory'..self.currentFocus['storeCategory']..'page'..self.currentFocus['storeCurrentPage']
		end

		if self.currentFocus['vaultCategory'] and self.currentFocus['storeCurrentPage'] then
			return 'goblinStoreVaultCategory'..self.currentFocus['vaultCategory']..'page'..self.currentFocus['storeCurrentPage']
		end
	end

	if self.currentFocus['mainMenuPanel'] and self.currentFocus['mainMenuPanel'] == 'compendium' then

		if self.currentFocus['learnatoriumTopTab'] then
			if self.currentFocus['learnatoriumTopTab'] == 3 then
				if self.currentFocus['learnatoriumUsageListSort'] then
					return 'learnatoriumUsageListSort_'..self.currentFocus['learnatoriumUsageListSort']
				end
			end
			if self.currentFocus['learnatoriumTopTab'] == 4 then
				if self.currentFocus['learnatoriumHero'] then
					if self.currentFocus['learnatoriumHeroAlt'] then
						if self.currentFocus['ownedAvatar'] then
							return 'learnatoriumViewHero_'..self.currentFocus['learnatoriumHero']..'_'..self.currentFocus['learnatoriumHeroAlt']..'_owned'
						end
						return 'learnatoriumViewHero_'..self.currentFocus['learnatoriumHero']..'_'..self.currentFocus['learnatoriumHeroAlt']
					else
						return 'learnatoriumViewHero_'..self.currentFocus['learnatoriumHero']
					end

				end
			end
			return 'learnatorium_tab_'..self.currentFocus['learnatoriumTopTab']
		else
			return 'learnatorium_tab_1'
		end

		return 'learnatorium'
	end

	if self.currentFocus['mainMenuPanel'] then
		return self.currentFocus['mainMenuPanel']
	end

	if self.currentFocus['gamePhase'] and self.currentFocus['gamePhase'] > 0 then
		return 'gamePhase'..self.currentFocus['gamePhase']
	end

	return ''

end

function trackedUserFocus:evaluateStorePosition()
	self:logFocus('storeCategory', GetCvarNumber('_microStore_Category'))
	self:logFocus('vaultCategory', nil)
end

function trackedUserFocus:evaluateVaultPosition()
	self:logFocus('vaultCategory', GetCvarNumber('_microStore_Category'))
	self:logFocus('storeCategory', nil)
end

function trackedUserFocus:incrementActionCount(key)
	if self.lastFocus[key] then
		self:logFocus(key, self.lastFocus[key] + 1)
	else
		self:logFocus(key, 1)
	end
end

function trackedUserFocus:openPrivateMessages()
	trackedUserFocus:logFocus('privateMessages', true)
end

function trackedUserFocus:closePrivateMessages()
	trackedUserFocus:logFocus('privateMessages', false)
end

function trackedUserFocus:openBuyCoins()
	trackedUserFocus:logFocus('buyCoinsOpen', true)
end

function trackedUserFocus:openRedeemCode()
	trackedUserFocus:logFocus('redeemCodeOpen', true)
end

function trackedUserFocus:closeBuyCoins()
	trackedUserFocus:logFocus('buyCoinsOpen', nil)
end

function trackedUserFocus:closeRedeemCode()
	trackedUserFocus:logFocus('redeemCodeOpen', nil)
end

function trackedUserFocus:openStoreSpecials()
	trackedUserFocus:logFocus('storeSpecials', true)
end

function trackedUserFocus:closeStoreSpecials()
	trackedUserFocus:logFocus('storeSpecials', nil)
end

function trackedUserFocus:logFocus(key, data)
	if data == nil then
		self.clearFocus[key] = true
	else
		self.updateFocus[key]	= data
	end

	self.updated			= true
end

function trackedUserFocus:evaluateKey(key)
	if self.lastFocus[key] ~= self.currentFocus[key] then
		self.lastFocus[key] = self.currentFocus[key]
		return true
	end

	return false
end

function trackedUserFocus:sendUpdate()
	print('should send new focus\n')
	printTable2(self.currentFocus)
end

function trackedUserFocus:initialize()
	if GetWidget('trackedUserFocus') then
		GetWidget('trackedUserFocus'):RegisterWatch('EndUpdate', function(widget, ...)
			if self.updated then
				local foundChange = false
				for k,v in pairs(self.updateFocus) do
					self.currentFocus[k] = v
					self.updateFocus[k] = nil
					if self:evaluateKey(k) then
						foundChange = true
					end
				end

				for k,v in pairs(self.clearFocus) do
					self.currentFocus[k] = nil
					self.clearFocus[k] = nil
					if self:evaluateKey(k) then
						foundChange = true
					end
				end

				if foundChange then
					self:sendUpdate()
					local primaryFocus = self:getPrimaryFocus()
					print('PRIMARY focus is '..primaryFocus..'\n')
				end

				self.updated = false
			end
		end)
	end
end

trackedUserFocus:initialize()