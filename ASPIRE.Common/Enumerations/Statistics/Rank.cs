namespace ASPIRE.Common.Enumerations.Statistics;

public enum Rank
{
    NO_MEDAL = 00,

    BRONZE_5 = 01, // 1250 MMR
    BRONZE_4 = 02, // 1275 MMR
    BRONZE_3 = 03, // 1300 MMR
    BRONZE_2 = 04, // 1330 MMR
    BRONZE_1 = 05, // 1360 MMR
    SILVER_5 = 06, // 1400 MMR
    SILVER_4 = 07, // 1435 MMR
    SILVER_3 = 08, // 1470 MMR
    SILVER_2 = 09, // 1505 MMR
    SILVER_1 = 10, // 1540 MMR
    GOLD_4 = 11, // 1575 MMR
    GOLD_3 = 12, // 1610 MMR
    GOLD_2 = 13, // 1645 MMR
    GOLD_1 = 14, // 1685 MMR
    DIAMOND_3 = 15, // 1725 MMR
    DIAMOND_2 = 16, // 1765 MMR
    DIAMOND_1 = 17, // 1805 MMR
    LEGENDARY_2 = 18, // 1850 MMR
    LEGENDARY_1 = 19, // 1900 MMR
    IMMORTAL = 20 // 1950 MMR
}

public static class RankExtensions
{
    public static Rank GetRank(double rating)
    {
        return rating switch
        {
            >= 1950 => Rank.IMMORTAL,
            >= 1900 => Rank.LEGENDARY_1,
            >= 1850 => Rank.LEGENDARY_2,
            >= 1805 => Rank.DIAMOND_1,
            >= 1765 => Rank.DIAMOND_2,
            >= 1725 => Rank.DIAMOND_3,
            >= 1685 => Rank.GOLD_1,
            >= 1645 => Rank.GOLD_2,
            >= 1610 => Rank.GOLD_3,
            >= 1575 => Rank.GOLD_4,
            >= 1540 => Rank.SILVER_1,
            >= 1505 => Rank.SILVER_2,
            >= 1470 => Rank.SILVER_3,
            >= 1435 => Rank.SILVER_4,
            >= 1400 => Rank.SILVER_5,
            >= 1360 => Rank.BRONZE_1,
            >= 1330 => Rank.BRONZE_2,
            >= 1300 => Rank.BRONZE_3,
            >= 1275 => Rank.BRONZE_4,
            >= 1250 => Rank.BRONZE_5,
            _ => Rank.NO_MEDAL
        };
    }
}