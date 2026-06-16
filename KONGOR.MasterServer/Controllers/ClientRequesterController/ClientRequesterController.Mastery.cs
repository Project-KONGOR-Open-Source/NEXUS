namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    /// <summary>
    ///     Retrieves account mastery information for the authenticated player.
    ///     Returns hero mastery experience data used by the client to display mastery progress.
    /// </summary>
    private async Task<IActionResult> GetAccountMastery()
    {
        string? cookie = Request.Form["cookie"];

        if (string.IsNullOrWhiteSpace(cookie))
            return Unauthorized(@"Missing Value For Form Parameter ""cookie""");

        string? accountName = await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
            return Unauthorized($@"No Session Found For Cookie ""{cookie}""");

        Account? account = await MerrickContext.Accounts
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound($@"Account With Name ""{accountName}"" Could Not Be Found");

        // The Mastery Row Is Created During Statistics Submission; A Transient All-Zero Row Is Used As A Fallback So That Reads Never Write To The Database
        Mastery mastery = await MerrickContext.Masteries.SingleOrDefaultAsync(record => record.AccountID == account.ID)
            ?? new Mastery { Account = account };

        // The Client Expects The Mastery Information As A Flat Comma-Separated List Of Hero Identifier And Experience Pairs (For Example "Hero_Accursed,1400,Hero_Adrenaline,0")
        string masteryInformation = string.Join(',', mastery.GetAllMasteriesInfo().Select(entry => $"{entry.Key},{entry.Value}"));

        Dictionary<string, object> response = new ()
        {
            ["error_code"] = 0,
            ["error_msg"] = string.Empty,
            ["account_id"] = account.ID,
            ["mastery_info"] = masteryInformation
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Claims the reward for a total mastery level tier, granting the configured product and currency and marking the tier as claimed.
    /// </summary>
    private async Task<IActionResult> TakeMasteryReward()
    {
        string? cookie = Request.Form["cookie"];

        if (string.IsNullOrWhiteSpace(cookie))
            return Unauthorized(@"Missing Value For Form Parameter ""cookie""");

        string? accountName = await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
            return Unauthorized($@"No Session Found For Cookie ""{cookie}""");

        if (int.TryParse(Request.Form["level"], out int level).Equals(false))
            return BadRequest(@"Invalid Value For Form Parameter ""level""");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound($@"Account With Name ""{accountName}"" Could Not Be Found");

        MasteryRewards? rewards = await MerrickContext.MasteryRewards.SingleOrDefaultAsync(record => record.AccountID == account.ID);

        if (rewards is null)
        {
            rewards = new MasteryRewards { Account = account };

            MerrickContext.MasteryRewards.Add(rewards);
        }

        if (rewards.HasObtained(level))
            return BadRequest($"Level {level} Reward Has Already Been Obtained");

        global::KONGOR.MasterServer.Configuration.Mastery.MasteryReward? reward = JSONConfiguration.MasteryRewardsConfiguration.MasteryRewards
            .SingleOrDefault(reward => reward.RequiredLevel == level);

        if (reward is null)
            return NotFound($"No Level {level} Reward Found");

        User user = account.User;

        if (string.IsNullOrWhiteSpace(reward.ProductName).Equals(false))
        {
            switch (reward.ProductName)
            {
                case "Mastery Boost":        MasteryConsumables.AddMasteryBoost(user, reward.ProductQuantity); break;
                case "Mastery Boost Bundle": MasteryConsumables.AddMasteryBoost(user, 10); break;
                case "Super Mastery Boost":  MasteryConsumables.AddSuperMasteryBoost(user, reward.ProductQuantity); break;

                default:
                {
                    if (reward.ProductCode is not null && user.OwnedStoreItems.Contains(reward.ProductCode).Equals(false))
                        user.OwnedStoreItems.Add(reward.ProductCode);

                    break;
                }
            }
        }

        user.GoldCoins     += reward.GoldCoins;
        user.SilverCoins   += reward.SilverCoins;
        user.PlinkoTickets += reward.PlinkoTickets;

        rewards.MarkObtained(level);

        await MerrickContext.SaveChangesAsync();

        return Ok(PhpSerialization.Serialize(new Dictionary<string, object> { ["error_code"] = 0, ["error_msg"] = string.Empty }));
    }

    /// <summary>
    ///     Applies an owned mastery boost to the hero played in the given match, consuming the boost and issuing a level reward if the hero crosses a mastery level.
    ///     A regular boost adds double the combined base and bonus match experience, while a super boost advances the hero to the start of the next mastery level.
    /// </summary>
    private async Task<IActionResult> BoostMatchMastery()
    {
        string? cookie = Request.Form["cookie"];

        if (string.IsNullOrWhiteSpace(cookie))
            return Unauthorized(@"Missing Value For Form Parameter ""cookie""");

        string? accountName = await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
            return Unauthorized($@"No Session Found For Cookie ""{cookie}""");

        if (int.TryParse(Request.Form["match_id"], out int matchID).Equals(false))
            return BadRequest(@"Invalid Value For Form Parameter ""match_id""");

        string? isSuperBoost = Request.Form["is_super_boost"];

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound($@"Account With Name ""{accountName}"" Could Not Be Found");

        MatchParticipantStatistics? participant = await MerrickContext.MatchParticipantStatistics
            .SingleOrDefaultAsync(statistics => statistics.AccountID == account.ID && statistics.MatchID == matchID);

        if (participant is null)
            return NotFound($@"Match Participant Statistics For Account ID {account.ID} And Match ID {matchID} Could Not Be Found");

        MatchStatistics? matchStatistics = await MerrickContext.MatchStatistics.SingleOrDefaultAsync(statistics => statistics.MatchID == matchID);

        if (matchStatistics is null)
            return NotFound($@"Match Statistics For Match ID {matchID} Could Not Be Found");

        MatchInformation? matchInformation = matchStatistics.MatchInformationSnapshot is not null
            ? JsonSerializer.Deserialize<MatchInformation>(matchStatistics.MatchInformationSnapshot) : null;

        AccountStatisticsType statisticsType = MatchCompletionRewardsHandler.ResolveAccountStatisticsType(matchInformation);

        Mastery? mastery = await MerrickContext.Masteries.SingleOrDefaultAsync(record => record.AccountID == account.ID);

        if (mastery is null)
        {
            mastery = new Mastery { Account = account };

            MerrickContext.Masteries.Add(mastery);
        }

        User user = account.User;

        string heroIdentifier = participant.HeroIdentifier;

        int currentExperience = mastery.GetHeroExperienceByHeroIdentifier(heroIdentifier);
        int previousLevel = Mastery.GetLevelFromExperience(currentExperience);

        if (isSuperBoost is "0")
        {
            if (MasteryConsumables.MasteryBoostsOwned(user) < 1)
                return BadRequest("Not Enough Mastery Boosts");

            mastery.SetHeroExperienceByHeroIdentifier(heroIdentifier, currentExperience + mastery.CalculateRegularMasteryBoostExperience(statisticsType, participant.HeroLevel));

            MasteryConsumables.RemoveMasteryBoost(user);
        }

        else if (isSuperBoost is "1")
        {
            if (MasteryConsumables.SuperMasteryBoostsOwned(user) < 1)
                return BadRequest("Not Enough Super Mastery Boosts");

            mastery.SetHeroExperienceByHeroIdentifier(heroIdentifier, Mastery.GetUpperLevelBoundaryFromExperience(currentExperience));

            MasteryConsumables.RemoveSuperMasteryBoost(user);
        }

        else
            return BadRequest(@"Invalid Value For Form Parameter ""is_super_boost""");

        int currentLevel = Mastery.GetLevelFromExperience(mastery.GetHeroExperienceByHeroIdentifier(heroIdentifier));

        // A Single Boost Can Never Award Enough Experience To Cross More Than One Mastery Level
        if (currentLevel == previousLevel + 1)
            MasteryConsumables.IssueHeroMasteryLevelReward(user, currentLevel, heroIdentifier, Logger);

        await MerrickContext.SaveChangesAsync();

        return Ok(PhpSerialization.Serialize(new Dictionary<string, object> { ["error_code"] = 0, ["error_msg"] = string.Empty }));
    }
}
