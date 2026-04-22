namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Serialisation.PHP;

/// <summary>
///     Tests To Verify Correct Serialisation And Deserialisation Of Discriminated Union Properties To/From PHP Array Format
/// </summary>
public sealed class DiscriminatedUnionPHPSerialisationTests
{
    /// <summary>
    ///     Asserts That <see cref="SRPAuthenticationResponseStageTwo.ClanMembershipData"/> Serialises To <see cref="ClanMemberData"/>
    /// </summary>
    [Test]
    public async Task ClanMembershipData_Serialises_To_ClanMemberData()
    {
        ClanMemberData clanData = new ()
        {
            ClanID = "666",
            ID = "42",
            ClanName = "Project KONGOR",
            ClanTag = "PK",
            ClanOwnerAccountID = "1",
            JoinDate = "2026-01-17 14:30:00",
            Rank = "Officer",
            Message = "Welcome To The Project KONGOR Clan !",
            Logo = "ლ(ಠ益ಠლ) BUT AT WHAT COST ?",
            Title = "Project KONGOR Main Channel"
        };

        OneOf<ClanMemberData, ClanMemberDataError> clanMembershipData = clanData;

        string serialisedData = clanMembershipData.Match(data => PhpSerialization.Serialize(data), error => PhpSerialization.Serialize(error));

        const string expectedSerialisationOutput = @"a:13:{s:7:""clan_id"";s:3:""666"";s:4:""name"";s:14:""Project KONGOR"";s:3:""tag"";s:2:""PK"";s:7:""creator"";s:1:""1"";s:10:""account_id"";s:2:""42"";s:4:""rank"";s:7:""Officer"";s:7:""message"";s:36:""Welcome To The Project KONGOR Clan !"";s:9:""join_date"";s:19:""2026-01-17 14:30:00"";s:5:""title"";s:27:""Project KONGOR Main Channel"";s:6:""active"";s:1:""1"";s:4:""logo"";s:36:""ლ(ಠ益ಠლ) BUT AT WHAT COST ?"";s:8:""idleWarn"";s:1:""0"";s:11:""activeIndex"";s:1:""0"";}";

        await Assert.That(serialisedData).IsEqualTo(expectedSerialisationOutput);

        if (PhpSerialization.Deserialize(serialisedData) is not IDictionary deserialisedData)
        {
            Assert.Fail("Deserialised Data Is NULL");
        }

        else
        {
            using (Assert.Multiple())
            {
                await Assert.That(deserialisedData["clan_id"]).IsEqualTo("666");
                await Assert.That(deserialisedData["account_id"]).IsEqualTo("42");
                await Assert.That(deserialisedData["name"]).IsEqualTo("Project KONGOR");
                await Assert.That(deserialisedData["tag"]).IsEqualTo("PK");
                await Assert.That(deserialisedData["creator"]).IsEqualTo("1");
                await Assert.That(deserialisedData["join_date"]).IsEqualTo("2026-01-17 14:30:00");
                await Assert.That(deserialisedData["rank"]).IsEqualTo("Officer");
                await Assert.That(deserialisedData["message"]).IsEqualTo("Welcome To The Project KONGOR Clan !");
                await Assert.That(deserialisedData["logo"]).IsEqualTo("ლ(ಠ益ಠლ) BUT AT WHAT COST ?");
                await Assert.That(deserialisedData["title"]).IsEqualTo("Project KONGOR Main Channel");
            }
        }
    }

    /// <summary>
    ///     Asserts That <see cref="SRPAuthenticationResponseStageTwo.ClanMembershipData"/> Serialises To <see cref="ClanMemberDataError"/>
    /// </summary>
    [Test]
    public async Task ClanMembershipData_Serialises_To_ClanMemberDataError()
    {
        ClanMemberDataError errorData = new ();

        OneOf<ClanMemberData, ClanMemberDataError> clanMembershipData = errorData;

        string serialisedData = clanMembershipData.Match(data => PhpSerialization.Serialize(data), error => PhpSerialization.Serialize(error));

        const string expectedSerialisationOutput = @"a:1:{s:5:""error"";s:20:""No Clan Member Found"";}";

        await Assert.That(serialisedData).IsEqualTo(expectedSerialisationOutput);

        if (PhpSerialization.Deserialize(serialisedData) is not IDictionary deserialisedData)
        {
            Assert.Fail("Deserialised Data Is NULL");
        }

        else
        {
            await Assert.That(deserialisedData["error"]).IsEqualTo("No Clan Member Found");
        }
    }

    /// <summary>
    ///     Asserts That <see cref="DiscriminatedUnionDictionary.OwnedStoreItemsData"/> Serialises To <see cref="StoreItemData"/>
    /// </summary>
    [Test]
    public async Task OwnedStoreItemsData_Serialises_To_StoreItemData()
    {
        DiscriminatedUnionDictionary storeItemData = new ()
        {
            OwnedStoreItemsData = new Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>>
            {
                {
                    "ai.custom_icon:12345", new StoreItemData
                    {
                        Data = "custom_data_value",
                        AvailableFrom = "1704067200",
                        AvailableUntil = "1735689600",
                        Used = 5,
                        Score = "150"
                    }
                },
                {
                    "aa.hero_avatar", new StoreItemData
                    {
                        Data = "avatar_data",
                        AvailableFrom = "1672531200",
                        AvailableUntil = "2209017600",
                        Used = 0,
                        Score = "0"
                    }
                }
            }
        };

        string serialisedData = PhpSerialization.Serialize(storeItemData);

        const string expectedSerialisationOutput = @"a:1:{s:22:""owned_store_items_data"";a:2:{s:20:""ai.custom_icon:12345"";a:5:{s:4:""data"";s:17:""custom_data_value"";s:10:""start_time"";s:10:""1704067200"";s:8:""end_time"";s:10:""1735689600"";s:4:""used"";i:5;s:5:""score"";s:3:""150"";}s:14:""aa.hero_avatar"";a:5:{s:4:""data"";s:11:""avatar_data"";s:10:""start_time"";s:10:""1672531200"";s:8:""end_time"";s:10:""2209017600"";s:4:""used"";i:0;s:5:""score"";s:1:""0"";}}}";

        await Assert.That(serialisedData).IsEqualTo(expectedSerialisationOutput);

        if (PhpSerialization.Deserialize(serialisedData) is not IDictionary deserialisedData)
        {
            Assert.Fail("Deserialised Data Is NULL");
        }

        else
        {
            if (deserialisedData["owned_store_items_data"] is not IDictionary ownedItemsDictionary)
            {
                Assert.Fail("Owned Store Items Data Is NULL");
            }

            else
            {
                using (Assert.Multiple())
                {
                    await Assert.That(ownedItemsDictionary.Contains("ai.custom_icon:12345")).IsTrue();
                    await Assert.That(ownedItemsDictionary.Contains("aa.hero_avatar")).IsTrue();
                }

                if (ownedItemsDictionary["ai.custom_icon:12345"] is not IDictionary one)
                {
                    Assert.Fail("Item 1 Is NULL");
                }

                else
                {
                    using (Assert.Multiple())
                    {
                        await Assert.That(one["data"]).IsEqualTo("custom_data_value");
                        await Assert.That(one["start_time"]).IsEqualTo("1704067200");
                        await Assert.That(one["end_time"]).IsEqualTo("1735689600");
                        await Assert.That(one["used"]).IsEqualTo(5);
                        await Assert.That(one["score"]).IsEqualTo("150");
                    }
                }

                if (ownedItemsDictionary["aa.hero_avatar"] is not IDictionary two)
                {
                    Assert.Fail("Item 2 Is NULL");
                }

                else
                {
                    using (Assert.Multiple())
                    {
                        await Assert.That(two["data"]).IsEqualTo("avatar_data");
                        await Assert.That(two["start_time"]).IsEqualTo("1672531200");
                        await Assert.That(two["end_time"]).IsEqualTo("2209017600");
                        await Assert.That(two["used"]).IsEqualTo(0);
                        await Assert.That(two["score"]).IsEqualTo("0");
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Asserts That <see cref="DiscriminatedUnionDictionary.OwnedStoreItemsData"/> Serialises To <see cref="StoreItemDiscountCoupon"/>
    /// </summary>
    [Test]
    public async Task OwnedStoreItemsData_Serialises_To_StoreItemDiscountCoupon()
    {
        DiscriminatedUnionDictionary storeItemDiscountCoupon = new ()
        {
            OwnedStoreItemsData = new Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>>
            {
                {
                    "cp.discount_coupon:99999", new StoreItemDiscountCoupon
                    {
                        Id = 99999,
                        Name = "discount_coupon",
                        Hero = "Hero_Test",
                        ApplicableProducts = "aa.Hero_Test.Alt,aa.Hero_Test.Alt2",
                        ApplicableProductsList = ["aa.Hero_Test.Alt", "aa.Hero_Test.Alt2"],
                        DiscountExpirationDate = "31 December 3000"
                    }
                }
            }
        };

        string serialisedData = PhpSerialization.Serialize(storeItemDiscountCoupon);

        const string expectedSerialisationOutput = @"a:1:{s:22:""owned_store_items_data"";a:1:{s:24:""cp.discount_coupon:99999"";a:6:{s:10:""product_id"";i:99999;s:9:""coupon_id"";i:99999;s:15:""coupon_products"";s:34:""aa.Hero_Test.Alt,aa.Hero_Test.Alt2"";s:8:""discount"";d:0.75;s:12:""mmp_discount"";d:0.75;s:8:""end_time"";s:16:""31 December 3000"";}}}";

        await Assert.That(serialisedData).IsEqualTo(expectedSerialisationOutput);

        if (PhpSerialization.Deserialize(serialisedData) is not IDictionary deserialisedData)
        {
            Assert.Fail("Deserialised Data Is NULL");
        }

        else
        {
            if (deserialisedData["owned_store_items_data"] is not IDictionary ownedItemsDictionary)
            {
                Assert.Fail("Owned Store Items Data Is NULL");
            }

            else
            {
                await Assert.That(ownedItemsDictionary.Contains("cp.discount_coupon:99999")).IsTrue();

                if (ownedItemsDictionary["cp.discount_coupon:99999"] is not IDictionary coupon)
                {
                    Assert.Fail("Coupon Is NULL");
                }

                else
                {
                    using (Assert.Multiple())
                    {
                        await Assert.That(coupon["product_id"]).IsEqualTo(99999);
                        await Assert.That(coupon["coupon_id"]).IsEqualTo(99999);
                        await Assert.That(coupon["coupon_products"]).IsEqualTo("aa.Hero_Test.Alt,aa.Hero_Test.Alt2");
                        await Assert.That(coupon["discount"]).IsEqualTo(0.75);
                        await Assert.That(coupon["mmp_discount"]).IsEqualTo(0.75);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Asserts That <see cref="DiscriminatedUnionDictionary.OwnedStoreItemsData"/> Serialises To Mixed Types
    /// </summary>
    [Test]
    public async Task OwnedStoreItemsData_Serialises_To_MixedTypes()
    {
        DiscriminatedUnionDictionary @object = new ()
        {
            OwnedStoreItemsData = new Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>>
            {
                {
                    "ai.icon:1", new StoreItemData
                    {
                        Data = "icon_data",
                        AvailableFrom = "1000000000",
                        AvailableUntil = "2000000000",
                        Used = 10,
                        Score = "500"
                    }
                },
                {
                    "cp.coupon:2", new StoreItemDiscountCoupon
                    {
                        Id = 2,
                        Name = "coupon",
                        Hero = "Hero_Mixed",
                        ApplicableProducts = "aa.Hero_Mixed.Alt",
                        ApplicableProductsList = ["aa.Hero_Mixed.Alt"],
                        DiscountExpirationDate = "31 December 3000"
                    }
                }
            }
        };

        string serialisedData = PhpSerialization.Serialize(@object);

        const string expectedSerialisationOutput = @"a:1:{s:22:""owned_store_items_data"";a:2:{s:9:""ai.icon:1"";a:5:{s:4:""data"";s:9:""icon_data"";s:10:""start_time"";s:10:""1000000000"";s:8:""end_time"";s:10:""2000000000"";s:4:""used"";i:10;s:5:""score"";s:3:""500"";}s:11:""cp.coupon:2"";a:6:{s:10:""product_id"";i:2;s:9:""coupon_id"";i:2;s:15:""coupon_products"";s:17:""aa.Hero_Mixed.Alt"";s:8:""discount"";d:0.75;s:12:""mmp_discount"";d:0.75;s:8:""end_time"";s:16:""31 December 3000"";}}}";

        await Assert.That(serialisedData).IsEqualTo(expectedSerialisationOutput);

        if (PhpSerialization.Deserialize(serialisedData) is not IDictionary deserialisedData)
        {
            Assert.Fail("Deserialised Data Is NULL");
        }

        else
        {
            if (deserialisedData["owned_store_items_data"] is not IDictionary ownedItemsDictionary)
            {
                Assert.Fail("Owned Store Items Data Is NULL");
            }

            else
            {
                using (Assert.Multiple())
                {
                    await Assert.That(ownedItemsDictionary.Contains("ai.icon:1")).IsTrue();
                    await Assert.That(ownedItemsDictionary.Contains("cp.coupon:2")).IsTrue();
                }

                if (ownedItemsDictionary["ai.icon:1"] is not IDictionary item)
                {
                    Assert.Fail("Item Is NULL");
                }

                else
                {
                    using (Assert.Multiple())
                    {
                        await Assert.That(item["data"]).IsEqualTo("icon_data");
                        await Assert.That(item["start_time"]).IsEqualTo("1000000000");
                        await Assert.That(item["end_time"]).IsEqualTo("2000000000");
                        await Assert.That(item["used"]).IsEqualTo(10);
                        await Assert.That(item["score"]).IsEqualTo("500");
                    }
                }

                if (ownedItemsDictionary["cp.coupon:2"] is not IDictionary coupon)
                {
                    Assert.Fail("Coupon Is NULL");
                }

                else
                {
                    using (Assert.Multiple())
                    {
                        await Assert.That(coupon["product_id"]).IsEqualTo(2);
                        await Assert.That(coupon["coupon_id"]).IsEqualTo(2);
                        await Assert.That(coupon["coupon_products"]).IsEqualTo("aa.Hero_Mixed.Alt");
                        await Assert.That(coupon["discount"]).IsEqualTo(0.75);
                        await Assert.That(coupon["mmp_discount"]).IsEqualTo(0.75);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Asserts That <see cref="NestedDiscriminatedUnionDictionary.MatchPlayerStatistics"/> Serialises To <see cref="ExtendedNativePropertyCollection"/> And <see cref="NativePropertyCollection"/>
    /// </summary>
    [Test]
    public async Task MatchPlayerStatistics_Serialises_To_ExtendedNativePropertyCollection_And_NativePropertyCollection()
    {
        NestedDiscriminatedUnionDictionary @object = new ()
        {
            MatchPlayerStatistics = new Dictionary<int, Dictionary<int, OneOf<ExtendedNativePropertyCollection, NativePropertyCollection>>>
            {
                {
                    100001, new Dictionary<int, OneOf<ExtendedNativePropertyCollection, NativePropertyCollection>>
                    {
                        {
                            1, new ExtendedNativePropertyCollection
                            {
                                AccountID = 1,
                                Kills = 5,
                                Deaths = 15,
                                Assists = 10,
                                MatchPerformanceQuickMatchExperience = "35",
                                MatchPerformanceQuickMatchGoldCoins = "40",
                                MatchPerformanceConsecutiveMatchExperience = "45",
                                MatchPerformanceConsecutiveMatchGoldCoins = "50",
                            }
                        },
                        {
                            2, new NativePropertyCollection
                            {
                                AccountID = 2,
                                Kills = 15,
                                Deaths = 10,
                                Assists = 5
                            }
                        },
                        {
                            3, new NativePropertyCollection
                            {
                                AccountID = 3,
                                Kills = 10,
                                Deaths = 5,
                                Assists = 15
                            }
                        }
                    }
                }
            }
        };

        string serialisedData = PhpSerialization.Serialize(@object);

        const string expectedSerialisationOutput = @"a:1:{s:18:""match_player_stats"";a:1:{i:100001;a:3:{i:1;a:8:{s:14:""perf_quick_exp"";s:2:""35"";s:13:""perf_quick_gc"";s:2:""40"";s:15:""perf_consec_exp"";s:2:""45"";s:14:""perf_consec_gc"";s:2:""50"";s:10:""account_id"";i:1;s:5:""kills"";i:5;s:6:""deaths"";i:15;s:7:""assists"";i:10;}i:2;a:4:{s:10:""account_id"";i:2;s:5:""kills"";i:15;s:6:""deaths"";i:10;s:7:""assists"";i:5;}i:3;a:4:{s:10:""account_id"";i:3;s:5:""kills"";i:10;s:6:""deaths"";i:5;s:7:""assists"";i:15;}}}}";

        await Assert.That(serialisedData).IsEqualTo(expectedSerialisationOutput);

        if (PhpSerialization.Deserialize(serialisedData) is not IDictionary deserialisedData)
        {
            Assert.Fail("Deserialised Data Is NULL");
        }

        else
        {
            using (Assert.Multiple())
            {
                await Assert.That(deserialisedData.Contains("match_player_stats")).IsTrue();

                if (deserialisedData["match_player_stats"] is not IList objectList)
                {
                    Assert.Fail("Object List Is NULL");
                }

                else
                {
                    if (objectList[0] is not IList playerStatisticsList)
                    {
                        Assert.Fail("Player Statistics List Is NULL");
                    }

                    else
                    {
                        if (playerStatisticsList[0] is not IDictionary playerOne)
                        {
                            Assert.Fail("Player One Is NULL");
                        }

                        else
                        {
                            using (Assert.Multiple())
                            {
                                await Assert.That(playerOne["account_id"]).IsEqualTo(1);
                                await Assert.That(playerOne["kills"]).IsEqualTo(5);
                                await Assert.That(playerOne["deaths"]).IsEqualTo(15);
                                await Assert.That(playerOne["assists"]).IsEqualTo(10);
                                await Assert.That(playerOne["perf_quick_exp"]).IsEqualTo("35");
                                await Assert.That(playerOne["perf_quick_gc"]).IsEqualTo("40");
                                await Assert.That(playerOne["perf_consec_exp"]).IsEqualTo("45");
                                await Assert.That(playerOne["perf_consec_gc"]).IsEqualTo("50");
                            }
                        }

                        if (playerStatisticsList[1] is not IDictionary playerTwo)
                        {
                            Assert.Fail("Player Two Is NULL");
                        }

                        else
                        {
                            using (Assert.Multiple())
                            {
                                await Assert.That(playerTwo["account_id"]).IsEqualTo(2);
                                await Assert.That(playerTwo["kills"]).IsEqualTo(15);
                                await Assert.That(playerTwo["deaths"]).IsEqualTo(10);
                                await Assert.That(playerTwo["assists"]).IsEqualTo(5);
                            }
                        }

                        if (playerStatisticsList[2] is not IDictionary playerThree)
                        {
                            Assert.Fail("Player Three Is NULL");
                        }

                        else
                        {
                            using (Assert.Multiple())
                            {
                                await Assert.That(playerThree["account_id"]).IsEqualTo(3);
                                await Assert.That(playerThree["kills"]).IsEqualTo(10);
                                await Assert.That(playerThree["deaths"]).IsEqualTo(5);
                                await Assert.That(playerThree["assists"]).IsEqualTo(15);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Asserts That <see cref="DiscriminatedUnionPropertyCollection.ClanMembershipData"/> And <see cref="DiscriminatedUnionPropertyCollection.OwnedStoreItemsData"/> Serialise Correctly
    /// </summary>
    [Test]
    public async Task ClanMembershipData_And_OwnedStoreItemsData_Serialise_Correctly()
    {
        DiscriminatedUnionPropertyCollection @object = new ()
        {
            ClanMembershipData = new ClanMemberData
            {
                ClanID = "99999",
                ID = "88888",
                ClanName = "Test Clan",
                ClanTag = "TEST",
                ClanOwnerAccountID = "77777",
                JoinDate = "2026-01-16 15:00:00",
                Rank = "Member",
                Message = "Test Message",
                Logo = "Test Logo",
                Title = "Test Channel Title"
            },
            OwnedStoreItemsData = new Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>>
            {
                {
                    "test.item:1", new StoreItemData
                    {
                        Data = "test_data",
                        AvailableFrom = "1700000000",
                        AvailableUntil = "1800000000",
                        Used = 3,
                        Score = "75"
                    }
                }
            }
        };

        string serialisedData = PhpSerialization.Serialize(@object);

        const string expectedSerialisationOutput = @"a:2:{s:16:""clan_member_info"";a:13:{s:7:""clan_id"";s:5:""99999"";s:4:""name"";s:9:""Test Clan"";s:3:""tag"";s:4:""TEST"";s:7:""creator"";s:5:""77777"";s:10:""account_id"";s:5:""88888"";s:4:""rank"";s:6:""Member"";s:7:""message"";s:12:""Test Message"";s:9:""join_date"";s:19:""2026-01-16 15:00:00"";s:5:""title"";s:18:""Test Channel Title"";s:6:""active"";s:1:""1"";s:4:""logo"";s:9:""Test Logo"";s:8:""idleWarn"";s:1:""0"";s:11:""activeIndex"";s:1:""0"";}s:16:""my_upgrades_info"";a:1:{s:11:""test.item:1"";a:5:{s:4:""data"";s:9:""test_data"";s:10:""start_time"";s:10:""1700000000"";s:8:""end_time"";s:10:""1800000000"";s:4:""used"";i:3;s:5:""score"";s:2:""75"";}}}";

        await Assert.That(serialisedData).IsEqualTo(expectedSerialisationOutput);

        if (PhpSerialization.Deserialize(serialisedData) is not IDictionary deserialisedData)
        {
            Assert.Fail("Deserialised Data Is NULL");
        }

        else
        {
            if (deserialisedData["clan_member_info"] is not IDictionary clanData)
            {
                Assert.Fail("Clan Member Info Is NULL");
            }

            else
            {
                using (Assert.Multiple())
                {
                    await Assert.That(clanData["clan_id"]).IsEqualTo("99999");
                    await Assert.That(clanData["account_id"]).IsEqualTo("88888");
                    await Assert.That(clanData["name"]).IsEqualTo("Test Clan");
                    await Assert.That(clanData["tag"]).IsEqualTo("TEST");
                    await Assert.That(clanData["creator"]).IsEqualTo("77777");
                    await Assert.That(clanData["join_date"]).IsEqualTo("2026-01-16 15:00:00");
                    await Assert.That(clanData["rank"]).IsEqualTo("Member");
                    await Assert.That(clanData["message"]).IsEqualTo("Test Message");
                    await Assert.That(clanData["logo"]).IsEqualTo("Test Logo");
                    await Assert.That(clanData["title"]).IsEqualTo("Test Channel Title");
                }
            }

            if (deserialisedData["my_upgrades_info"] is not IDictionary upgradesData)
            {
                Assert.Fail("My Upgrades Info Is NULL");
            }

            else
            {
                await Assert.That(upgradesData.Contains("test.item:1")).IsTrue();

                if (upgradesData["test.item:1"] is not IDictionary item)
                {
                    Assert.Fail("Item Is NULL");
                }

                else
                {
                    using (Assert.Multiple())
                    {
                        await Assert.That(item["data"]).IsEqualTo("test_data");
                        await Assert.That(item["start_time"]).IsEqualTo("1700000000");
                        await Assert.That(item["end_time"]).IsEqualTo("1800000000");
                        await Assert.That(item["used"]).IsEqualTo(3);
                        await Assert.That(item["score"]).IsEqualTo("75");
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Asserts that an empty discriminated union dictionary serialises to an empty PHP array.
    /// </summary>
    [Test]
    public async Task EmptyDiscriminatedUnionDictionary_Serialises_To_EmptyPHPArray()
    {
        DiscriminatedUnionDictionary @object = new ()
        {
            OwnedStoreItemsData = new Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>>()
        };

        string serialisedData = PhpSerialization.Serialize(@object);

        const string expectedSerialisationOutput = @"a:1:{s:22:""owned_store_items_data"";a:0:{}}";

        await Assert.That(serialisedData).IsEqualTo(expectedSerialisationOutput);

        if (PhpSerialization.Deserialize(serialisedData) is not IDictionary deserialisedData)
        {
            Assert.Fail("Deserialised Data Is NULL");
        }

        else
        {
            // PhpSerializerNET Deserialises An Empty PHP Array (a:0:{}) As An Empty List Rather Than A Dictionary
            if (deserialisedData["owned_store_items_data"] is not IList ownedItemsList)
            {
                Assert.Fail("Owned Store Items Data Is NULL");
            }

            else
            {
                await Assert.That(ownedItemsList.Count).IsEqualTo(0);
            }
        }
    }

    /// <summary>
    ///     Asserts that a single-entry discriminated union dictionary with each union variant serialises correctly.
    /// </summary>
    [Test]
    public async Task SingleEntryDiscriminatedUnionDictionary_Serialises_Correctly_For_Each_Variant()
    {
        DiscriminatedUnionDictionary firstVariant = new ()
        {
            OwnedStoreItemsData = new Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>>
            {
                {
                    "ai.icon:1", new StoreItemData
                    {
                        Data = "icon_data",
                        AvailableFrom = "0",
                        AvailableUntil = "0",
                        Used = 0,
                        Score = "0"
                    }
                }
            }
        };

        DiscriminatedUnionDictionary secondVariant = new ()
        {
            OwnedStoreItemsData = new Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>>
            {
                {
                    "cp.coupon:1", new StoreItemDiscountCoupon
                    {
                        Id = 1,
                        Name = "coupon",
                        Hero = "Hero_Test",
                        ApplicableProducts = "aa.Hero_Test.Alt",
                        ApplicableProductsList = ["aa.Hero_Test.Alt"],
                        DiscountExpirationDate = "31 December 3000"
                    }
                }
            }
        };

        string firstSerialised = PhpSerialization.Serialize(firstVariant);
        string secondSerialised = PhpSerialization.Serialize(secondVariant);

        // First Variant Should Contain StoreItemData Properties
        if (PhpSerialization.Deserialize(firstSerialised) is not IDictionary firstDeserialisedData)
        {
            Assert.Fail("First Deserialised Data Is NULL");
        }

        else
        {
            if (firstDeserialisedData["owned_store_items_data"] is not IDictionary firstItems)
            {
                Assert.Fail("First Owned Store Items Data Is NULL");
            }

            else
            {
                if (firstItems["ai.icon:1"] is not IDictionary firstItem)
                {
                    Assert.Fail("First Item Is NULL");
                }

                else
                {
                    using (Assert.Multiple())
                    {
                        await Assert.That(firstItem.Contains("data")).IsTrue();
                        await Assert.That(firstItem.Contains("start_time")).IsTrue();
                        await Assert.That(firstItem.Contains("end_time")).IsTrue();
                        await Assert.That(firstItem.Contains("used")).IsTrue();
                        await Assert.That(firstItem.Contains("score")).IsTrue();
                    }
                }
            }
        }

        // Second Variant Should Contain StoreItemDiscountCoupon Properties
        if (PhpSerialization.Deserialize(secondSerialised) is not IDictionary secondDeserialisedData)
        {
            Assert.Fail("Second Deserialised Data Is NULL");
        }

        else
        {
            if (secondDeserialisedData["owned_store_items_data"] is not IDictionary secondItems)
            {
                Assert.Fail("Second Owned Store Items Data Is NULL");
            }

            else
            {
                if (secondItems["cp.coupon:1"] is not IDictionary secondItem)
                {
                    Assert.Fail("Second Item Is NULL");
                }

                else
                {
                    using (Assert.Multiple())
                    {
                        await Assert.That(secondItem.Contains("product_id")).IsTrue();
                        await Assert.That(secondItem.Contains("coupon_id")).IsTrue();
                        await Assert.That(secondItem.Contains("coupon_products")).IsTrue();
                        await Assert.That(secondItem.Contains("discount")).IsTrue();
                        await Assert.That(secondItem.Contains("mmp_discount")).IsTrue();
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Asserts that nested discriminated union dictionaries with a single player produce correct output.
    /// </summary>
    [Test]
    public async Task NestedDiscriminatedUnionDictionary_With_SinglePlayer_Serialises_Correctly()
    {
        NestedDiscriminatedUnionDictionary @object = new ()
        {
            MatchPlayerStatistics = new Dictionary<int, Dictionary<int, OneOf<ExtendedNativePropertyCollection, NativePropertyCollection>>>
            {
                {
                    999, new Dictionary<int, OneOf<ExtendedNativePropertyCollection, NativePropertyCollection>>
                    {
                        {
                            42, new ExtendedNativePropertyCollection
                            {
                                AccountID = 42,
                                Kills = 20,
                                Deaths = 3,
                                Assists = 12,
                                MatchPerformanceQuickMatchExperience = "100",
                                MatchPerformanceQuickMatchGoldCoins = "50",
                                MatchPerformanceConsecutiveMatchExperience = "25",
                                MatchPerformanceConsecutiveMatchGoldCoins = "15"
                            }
                        }
                    }
                }
            }
        };

        string serialisedData = PhpSerialization.Serialize(@object);

        if (PhpSerialization.Deserialize(serialisedData) is not IDictionary deserialisedData)
        {
            Assert.Fail("Deserialised Data Is NULL");
        }

        else
        {
            if (deserialisedData["match_player_stats"] is not IList matchList)
            {
                Assert.Fail("Match List Is NULL");
            }

            else
            {
                if (matchList[0] is not IList playerList)
                {
                    Assert.Fail("Player List Is NULL");
                }

                else
                {
                    if (playerList[0] is not IDictionary player)
                    {
                        Assert.Fail("Player Is NULL");
                    }

                    else
                    {
                        using (Assert.Multiple())
                        {
                            await Assert.That(player["account_id"]).IsEqualTo(42);
                            await Assert.That(player["kills"]).IsEqualTo(20);
                            await Assert.That(player["deaths"]).IsEqualTo(3);
                            await Assert.That(player["assists"]).IsEqualTo(12);
                            await Assert.That(player["perf_quick_exp"]).IsEqualTo("100");
                            await Assert.That(player["perf_quick_gc"]).IsEqualTo("50");
                            await Assert.That(player["perf_consec_exp"]).IsEqualTo("25");
                            await Assert.That(player["perf_consec_gc"]).IsEqualTo("15");
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Asserts that an empty nested discriminated union dictionary serialises to an empty PHP array.
    /// </summary>
    [Test]
    public async Task EmptyNestedDiscriminatedUnionDictionary_Serialises_To_EmptyPHPArray()
    {
        NestedDiscriminatedUnionDictionary @object = new ()
        {
            MatchPlayerStatistics = new Dictionary<int, Dictionary<int, OneOf<ExtendedNativePropertyCollection, NativePropertyCollection>>>()
        };

        string serialisedData = PhpSerialization.Serialize(@object);

        const string expectedSerialisationOutput = @"a:1:{s:18:""match_player_stats"";a:0:{}}";

        await Assert.That(serialisedData).IsEqualTo(expectedSerialisationOutput);
    }

    /// <summary>
    ///     Asserts that both union variants serialise with the correct property set when they share a base class.
    ///     The extended variant should contain all base properties plus its own additional properties.
    /// </summary>
    [Test]
    public async Task InheritedDiscriminatedUnion_Serialises_BaseAndDerivedProperties_Correctly()
    {
        NestedDiscriminatedUnionDictionary @object = new ()
        {
            MatchPlayerStatistics = new Dictionary<int, Dictionary<int, OneOf<ExtendedNativePropertyCollection, NativePropertyCollection>>>
            {
                {
                    1, new Dictionary<int, OneOf<ExtendedNativePropertyCollection, NativePropertyCollection>>
                    {
                        { 10, new NativePropertyCollection { AccountID = 10, Kills = 1, Deaths = 2, Assists = 3 } },
                        {
                            20, new ExtendedNativePropertyCollection
                            {
                                AccountID = 20, Kills = 4, Deaths = 5, Assists = 6,
                                MatchPerformanceQuickMatchExperience = "10",
                                MatchPerformanceQuickMatchGoldCoins = "20",
                                MatchPerformanceConsecutiveMatchExperience = "30",
                                MatchPerformanceConsecutiveMatchGoldCoins = "40"
                            }
                        }
                    }
                }
            }
        };

        string serialisedData = PhpSerialization.Serialize(@object);

        if (PhpSerialization.Deserialize(serialisedData) is not IDictionary deserialisedData)
        {
            Assert.Fail("Deserialised Data Is NULL");
        }

        else
        {
            if (deserialisedData["match_player_stats"] is not IList matchList)
            {
                Assert.Fail("Match List Is NULL");
            }

            else
            {
                if (matchList[0] is not IDictionary playerDictionary)
                    {
                        Assert.Fail("Player Dictionary Is NULL");
                    }

                    else
                    {
                        // Base Variant Should Have Exactly 4 Properties
                        // Non-Sequential Integer Keys (10, 20) Deserialise As IDictionary Entries
                        if (playerDictionary[10] is not IDictionary basePlayer)
                        {
                            Assert.Fail("Base Player Is NULL");
                        }

                        else
                        {
                            using (Assert.Multiple())
                            {
                                await Assert.That(basePlayer.Count).IsEqualTo(4);
                                await Assert.That(basePlayer["account_id"]).IsEqualTo(10);
                                await Assert.That(basePlayer.Contains("perf_quick_exp")).IsFalse();
                            }
                        }

                        // Extended Variant Should Have 8 Properties (4 Base + 4 Extended)
                        if (playerDictionary[20] is not IDictionary extendedPlayer)
                        {
                            Assert.Fail("Extended Player Is NULL");
                        }

                        else
                        {
                            using (Assert.Multiple())
                            {
                                await Assert.That(extendedPlayer.Count).IsEqualTo(8);
                                await Assert.That(extendedPlayer["account_id"]).IsEqualTo(20);
                                await Assert.That(extendedPlayer["perf_quick_exp"]).IsEqualTo("10");
                                await Assert.That(extendedPlayer["perf_consec_gc"]).IsEqualTo("40");
                            }
                        }
                    }
            }
        }
    }
}

file class DiscriminatedUnionDictionary
{
    [PHPProperty("owned_store_items_data", isDiscriminatedUnion: true)]
    public required Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> OwnedStoreItemsData { get; set; }
}

file class NestedDiscriminatedUnionDictionary
{
    [PHPProperty("match_player_stats", isDiscriminatedUnion: true)]
    public required Dictionary<int, Dictionary<int, OneOf<ExtendedNativePropertyCollection, NativePropertyCollection>>> MatchPlayerStatistics { get; set; }
}

file class NativePropertyCollection
{
    [PHPProperty("account_id")]
    public required int AccountID { get; set; }

    [PHPProperty("kills")]
    public required int Kills { get; set; }

    [PHPProperty("deaths")]
    public required int Deaths { get; set; }

    [PHPProperty("assists")]
    public required int Assists { get; set; }
}

file class ExtendedNativePropertyCollection : NativePropertyCollection
{
    [PHPProperty("perf_quick_exp")]
    public required string MatchPerformanceQuickMatchExperience { get; init; }

    [PHPProperty("perf_quick_gc")]
    public required string MatchPerformanceQuickMatchGoldCoins { get; init; }

    [PHPProperty("perf_consec_exp")]
    public required string MatchPerformanceConsecutiveMatchExperience { get; init; }

    [PHPProperty("perf_consec_gc")]
    public required string MatchPerformanceConsecutiveMatchGoldCoins { get; init; }
}

file class DiscriminatedUnionPropertyCollection
{
    [PHPProperty("clan_member_info", isDiscriminatedUnion: true)]
    public required OneOf<ClanMemberData, ClanMemberDataError> ClanMembershipData { get; set; }

    [PHPProperty("my_upgrades_info", isDiscriminatedUnion: true)]
    public required Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> OwnedStoreItemsData { get; set; }
}
