namespace KONGOR.MasterServer.Controllers.ClientRequester;

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

        // Error Code 1 Matches The Original API's "Internal System Error" Code
        if (account is null)
            return MasteryErrorResponse(1, $@"Account With Name ""{accountName}"" Could Not Be Found");

        // The Mastery Row Is Created During Statistics Submission; A Transient All-Zero Row Is Used As A Fallback So That Reads Never Write To The Database
        Mastery mastery = await MerrickContext.Masteries.SingleOrDefaultAsync(record => record.AccountID == account.ID)
            ?? new Mastery { Account = account };

        // The Client Expects The Mastery Information As A Flat Comma-Separated List Of Hero Identifier And Experience Pairs (For Example "Hero_Accursed,1400,Hero_Adrenaline,0"), Covering Every Hero
        string masteryInformation = string.Join(',', Heroes.AllHeroIdentifiers().Select(identifier => $"{identifier},{mastery.GetHeroExperienceByHeroIdentifier(identifier)}"));

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

        // Error Code 2 Matches The Original API's "Input Parameter Error" Code
        if (int.TryParse(Request.Form["level"], out int level).Equals(false))
            return MasteryErrorResponse(2, @"Invalid Value For Form Parameter ""level""");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        // Error Code 1 Matches The Original API's "Internal System Error" Code
        if (account is null)
            return MasteryErrorResponse(1, $@"Account With Name ""{accountName}"" Could Not Be Found");

        global::KONGOR.MasterServer.Configuration.Mastery.MasteryReward? reward = JSONConfiguration.MasteryRewardsConfiguration.MasteryRewards
            .SingleOrDefault(reward => reward.RequiredLevel == level);

        // Error Code 5 Matches The Original API's "Mastery Reward Invalid" Code
        if (reward is null)
            return MasteryErrorResponse(5, $"No Level {level} Reward Found");

        MasteryRewards? rewards = await MerrickContext.MasteryRewards.SingleOrDefaultAsync(record => record.AccountID == account.ID);

        if (rewards is null)
        {
            rewards = new MasteryRewards { Account = account };

            MerrickContext.MasteryRewards.Add(rewards);
        }

        // Error Code 3 Matches The Original API's "Reward Already Taken" Code
        if (rewards.HasObtained(level))
            return MasteryErrorResponse(3, $"Level {level} Reward Has Already Been Obtained");

        Mastery? mastery = await MerrickContext.Masteries.SingleOrDefaultAsync(record => record.AccountID == account.ID);

        // Error Code 4 Matches The Original API's "Reward Does Not Exist" Code, Which It Returned For Reward Tiers The Account Had Not Unlocked Yet
        if ((mastery?.TotalMasteryLevel() ?? 0) < level)
            return MasteryErrorResponse(4, $"Level {level} Reward Requires A Total Mastery Level Of {level}");

        User user = account.User;

        if (string.IsNullOrWhiteSpace(reward.ProductName).Equals(false))
        {
            switch (reward.ProductName)
            {
                case "Mastery Boost":        MasteryConsumables.AddMasteryBoost(user, reward.ProductQuantity); break;
                case "Mastery Boost Bundle": MasteryConsumables.AddMasteryBoost(user, 10); break;
                case "Super Mastery Boost":  MasteryConsumables.AddSuperMasteryBoost(user, reward.ProductQuantity); break;

                // TODO: Implement Sub-Accounts
                // The "Sub-Account" Reward Is Granted Here As An Owned Store Item, But Owning It Has No Effect Until Sub-Accounts Are Implemented
                default:
                {
                    if (reward.ProductCode is not null && user.OwnedStoreItems.Contains(reward.ProductCode).Equals(false))
                        user.OwnedStoreItems.Add(reward.ProductCode);

                    // Owning More Alternative Avatars Can Leave A Hero's Mastery Coupon Without Any Avatar To Apply It To, In Which Case It Is Exchanged For The All-Avatar Mastery Coupon
                    MasteryConsumables.ResolveUnusableCoupons(user);

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
    ///     Applies an owned mastery boost to the hero played in the given match, consuming the boost, recording it on the match, and issuing a level reward if the hero crosses a mastery level.
    ///     A regular boost adds double the match and bonus experience of the match, while a super boost advances the hero to the start of the next mastery level.
    /// </summary>
    private async Task<IActionResult> BoostMatchMastery()
    {
        string? cookie = Request.Form["cookie"];

        if (string.IsNullOrWhiteSpace(cookie))
            return Unauthorized(@"Missing Value For Form Parameter ""cookie""");

        string? accountName = await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
            return Unauthorized($@"No Session Found For Cookie ""{cookie}""");

        // Error Code 2 Matches The Original API's "Input Parameter Error" Code
        if (int.TryParse(Request.Form["match_id"], out int matchID).Equals(false))
            return MasteryErrorResponse(2, @"Invalid Value For Form Parameter ""match_id""");

        string? isSuperBoost = Request.Form["is_super_boost"];

        // Error Code 2 Matches The Original API's "Input Parameter Error" Code
        if (isSuperBoost is not ("0" or "1"))
            return MasteryErrorResponse(2, @"Invalid Value For Form Parameter ""is_super_boost""");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        // Error Code 1 Matches The Original API's "Internal System Error" Code
        if (account is null)
            return MasteryErrorResponse(1, $@"Account With Name ""{accountName}"" Could Not Be Found");

        MatchParticipantStatistics? participant = await MerrickContext.MatchParticipantStatistics
            .SingleOrDefaultAsync(statistics => statistics.AccountID == account.ID && statistics.MatchID == matchID);

        MasteryProgression? progression = participant?.MasteryProgression;

        // Error Code 6 Matches The Original API's "Match Mastery Data Does Not Exist" Code, Because The Original API Recorded No Mastery Data For Matches That Award No Mastery Experience
        if (participant is null || progression is null)
            return MasteryErrorResponse(6, $"No Mastery Data Exists For Match ID {matchID}");

        Mastery? mastery = await MerrickContext.Masteries.SingleOrDefaultAsync(record => record.AccountID == account.ID);

        if (mastery is null)
        {
            mastery = new Mastery { Account = account };

            MerrickContext.Masteries.Add(mastery);
        }

        MasteryBoostRejection? rejection = await MasteryBoostEligibility.GetRejection(MerrickContext, mastery, participant);

        if (rejection is not null)
            return MasteryErrorResponse(rejection.ErrorCode, rejection.ErrorMessage);

        User user = account.User;

        string heroIdentifier = participant.HeroIdentifier;

        int currentExperience = mastery.GetHeroExperienceByHeroIdentifier(heroIdentifier);
        int previousLevel = Mastery.GetLevelFromExperience(currentExperience);

        if (isSuperBoost is "0")
        {
            // Error Code 3 Matches The Original API's "Not Enough Mastery Boosts" Code
            if (MasteryConsumables.MasteryBoostsOwned(user) < 1)
                return MasteryErrorResponse(3, "Not Enough Mastery Boosts");

            progression.BoostExperience = Mastery.CalculateRegularMasteryBoostExperience(progression.MatchExperience, progression.BonusExperience);

            mastery.SetHeroExperienceByHeroIdentifier(heroIdentifier, currentExperience + progression.BoostExperience);

            MasteryConsumables.RemoveMasteryBoost(user);
        }

        else
        {
            // Error Code 3 Matches The Original API's "Not Enough Mastery Boosts" Code
            if (MasteryConsumables.SuperMasteryBoostsOwned(user) < 1)
                return MasteryErrorResponse(3, "Not Enough Super Mastery Boosts");

            progression.SuperBoostExperience = Mastery.GetUpperLevelBoundaryFromExperience(currentExperience) - currentExperience;

            mastery.SetHeroExperienceByHeroIdentifier(heroIdentifier, currentExperience + progression.SuperBoostExperience);

            MasteryConsumables.RemoveSuperMasteryBoost(user);
        }

        int currentLevel = mastery.GetHeroLevelByHeroIdentifier(heroIdentifier);

        // A Single Boost Can Never Award Enough Experience To Cross More Than One Mastery Level
        if (currentLevel == previousLevel + 1)
            MasteryConsumables.IssueHeroMasteryLevelReward(user, currentLevel, heroIdentifier, Logger);

        await MerrickContext.SaveChangesAsync();

        return Ok(PhpSerialization.Serialize(new Dictionary<string, object> { ["error_code"] = 0, ["error_msg"] = string.Empty }));
    }

    /// <summary>
    ///     Creates a mastery error response with the given error code, matching the error contract of the original client API.
    ///     The client parses the response body as PHP-serialised data and treats a missing "error_code" key as a success, so mastery errors must be returned as PHP-serialised payloads rather than as plain HTTP errors.
    /// </summary>
    private IActionResult MasteryErrorResponse(int errorCode, string errorMessage)
        => Ok(PhpSerialization.Serialize(new Dictionary<string, object> { ["error_code"] = errorCode, ["error_msg"] = errorMessage }));
}
