namespace MERRICK.Database.Handlers;

public static class SeedDataHandlers
{
    public static async Task SeedUsers(MerrickContext context, CancellationToken cancellationToken, ILogger logger)
    {
        if (await context.Users.AnyAsync(cancellationToken) || await context.Roles.NoneAsync(cancellationToken)) return;

        Role role = await context.Roles.SingleAsync(role => role.Name.Equals(UserRoles.Administrator), cancellationToken: cancellationToken);

        User user = new()
        {
            EmailAddress = "project.kongor@proton.me",
            Role = role,
            SRPPasswordSalt = "861c37ec6d049d92cc1c67d195b414f26b572a56358272af3e9c06fcd9bfa053",
            SRPPasswordHash = "fe6f16b0ecb80f6b2bc95d68420fd13afef0c895172a81819870660208ac221a",
            PBKDF2PasswordHash = "AQAAAAIAAYagAAAAEMUkpLAr01NjkKRPaXCyTa17nlOdPKJucn5QYur+wQBTDKCpgsAcREenK+pGJPBCRw==",
            GoldCoins = 5_555_555,
            SilverCoins = 555_555_555,
            PlinkoTickets = 5_555_555,
            TotalLevel = 666,
            TotalExperience = 222_11_666,

            // TODO: Add All Upgrades (Including Missing Ones From PK Version Control); Maybe Scrape The Resources Again

            OwnedStoreItems = [ "ai.custom_icon:1", "av.Flamboyant", "c.cat_courier", "cc.frostburnlogo", "cr.Punk Creep", "cs.frostburnlogo", "m.Super-Taunt", "sc.paragon_circle_upgrade", "t.Dumpster_Taunt", "te.Punk TP", "w.8bit_ward" ]
        };

        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public static async Task SeedClans(MerrickContext context, CancellationToken cancellationToken, ILogger logger)
    {
        if (await context.Clans.AnyAsync(cancellationToken)) return;

        IEnumerable<Clan> clans =
        [
            new Clan { Name = "KONGOR", Tag = "K" },
            new Clan { Name = "Project KONGOR", Tag = "PK" },
            new Clan { Name = "Project KONGOR Open-Source", Tag = "PKOS" },
            new Clan { Name = "Heroes Of Newerth", Tag = "HON" }
        ];

        await context.Clans.AddRangeAsync(clans, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public static async Task SeedAccounts(MerrickContext context, CancellationToken cancellationToken, ILogger logger)
    {
        if (await context.Accounts.AnyAsync(cancellationToken) || await context.Users.NoneAsync(cancellationToken) || await context.Clans.NoneAsync(cancellationToken)) return;

        User user = await context.Users.FirstAsync(cancellationToken);
        Clan clan = await context.Clans.FirstAsync(cancellationToken);

        Account systemAccount = new()
        {
            Name = "KONGOR",
            User = user,
            Type = AccountType.Staff,
            IsMain = true,
            Clan = null,
            ClanTier = ClanTier.None,
            TimestampJoinedClan = null,
            AscensionLevel = 666,
            AutoConnectChatChannels = [ "KONGOR", "TERMINAL" ],
            SelectedStoreItems = [ "ai.custom_icon:1", "av.Flamboyant", "c.cat_courier", "cc.frostburnlogo", "cr.Punk Creep", "cs.frostburnlogo", "m.Super-Taunt", "sc.paragon_circle_upgrade", "t.Dumpster_Taunt", "te.Punk TP", "w.8bit_ward" ]
        };

        await context.AddAsync(systemAccount, cancellationToken);

        string[] subAccountNames = [ /* [K] */ "ONGOR", "GOPO", "Xen0byte" ];

        foreach (string subAccountName in subAccountNames)
        {
            Account subAccount = new()
            {
                Name = subAccountName,
                User = user,
                Type = AccountType.Staff,
                IsMain = false,
                Clan = clan,
                ClanTier = subAccountName is /* [K] */ "ONGOR" ? ClanTier.Leader : ClanTier.Officer,
                TimestampJoinedClan = DateTime.UtcNow,
                AscensionLevel = 666,
                AutoConnectChatChannels = [ "KONGOR", "TERMINAL" ],
                SelectedStoreItems = [ "ai.custom_icon:1", "av.Flamboyant", "c.cat_courier", "cc.frostburnlogo", "cr.Punk Creep", "cs.frostburnlogo", "m.Super-Taunt", "sc.paragon_circle_upgrade", "t.Dumpster_Taunt", "te.Punk TP", "w.8bit_ward" ]
            };

            await context.AddAsync(subAccount, cancellationToken);
        }

        string[] hostAccountNames = [ "HOST" ];

        foreach (string hostAccountName in hostAccountNames)
        {
            Account hostAccount = new()
            {
                Name = hostAccountName,
                User = user,
                Type = AccountType.ServerHost,
                IsMain = false,
                Clan = null,
                ClanTier = ClanTier.None,
                TimestampJoinedClan = null,
                AscensionLevel = 666,
                AutoConnectChatChannels = [ "KONGOR", "TERMINAL" ],
                SelectedStoreItems = [ "ai.custom_icon:1", "av.Flamboyant", "c.cat_courier", "cc.frostburnlogo", "cr.Punk Creep", "cs.frostburnlogo", "m.Super-Taunt", "sc.paragon_circle_upgrade", "t.Dumpster_Taunt", "te.Punk TP", "w.8bit_ward" ]
            };

            await context.AddAsync(hostAccount, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public static async Task SeedFriendedPeers(MerrickContext context, CancellationToken cancellationToken, ILogger logger)
    {
        if (await context.Accounts.NoneAsync(cancellationToken) || await context.Clans.NoneAsync(cancellationToken)) return;

        Account systemAccount = await context.Accounts.FirstAsync(cancellationToken);
        Account hostAccount = await context.Accounts.SingleAsync(account => account.Type == AccountType.ServerHost, cancellationToken: cancellationToken);

        List<Account> subAccounts = await context.Accounts.Include(account => account.User).Include(account => account.Clan)
            .Where(account => account.IsMain.Equals(false)).ToListAsync(cancellationToken: cancellationToken);

        foreach (Account subAccount in subAccounts.Except([hostAccount]))
        {
            subAccount.FriendedPeers = subAccounts.Except([hostAccount, subAccount]).Union([systemAccount]).Select(account => new FriendedPeer
            {
                Identifier = account.ID,
                Name = account.Name,
                ClanTag = account.Clan?.Tag,
                FriendGroup = "ALIAS"
            }).ToList();
        }
    }

    public static async Task SeedHeroGuides(MerrickContext context, CancellationToken cancellationToken, ILogger logger)
    {
        if (await context.HeroGuides.AnyAsync(cancellationToken) || await context.Accounts.NoneAsync(cancellationToken)) return;

        Account author = await context.Accounts.FirstAsync(cancellationToken);

        DeserializationDTOs.GuideGetDTO[] guideDTOs = JsonSerializer.Deserialize<DeserializationDTOs.GuideGetDTO[]>
            (await File.ReadAllTextAsync(DataFiles.HeroGuides, cancellationToken), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

        // The Game Client Requires At Least One Featured And One Standard Guide Per Hero

        IEnumerable<HeroGuide> guides = guideDTOs.Select(guide => new HeroGuide()
        {
            Name = guide.Name,
            HeroName = guide.HeroName,
            HeroIdentifier = guide.HeroIdentifier,
            Intro = guide.Intro,
            Content = guide.Content,
            StartingItems = guide.StartingItems.ListToPipeSeparatedString(),
            EarlyGameItems = guide.EarlyGameItems.ListToPipeSeparatedString(),
            CoreItems = guide.CoreItems.ListToPipeSeparatedString(),
            LuxuryItems = guide.LuxuryItems.ListToPipeSeparatedString(),
            AbilityQueue = guide.AbilityQueue.ListToPipeSeparatedString(),
            Author = author,
            Rating = guide.Rating,
            UpVotes = guide.UpVotes,
            DownVotes = guide.DownVotes,
            Public = guide.Public,
            Featured = guide.Featured,
            TimestampCreated = DateTime.UtcNow
        });

        await context.HeroGuides.AddRangeAsync(guides, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}
