using global::MERRICK.DatabaseContext.Entities.Core;
using global::MERRICK.DatabaseContext.Enumerations;

namespace MERRICK.DatabaseContext.Extensions;

public static class AccountExtensions
{
    public static string GetNameWithClanTag(this Account account)
    {
        if (account is null) return "Unknown";
        return Equals(account.Clan, null) ? account.Name : $"[{account.Clan.Tag}]{account.Name}";
    }

    public static string GetClanTierName(this Account account)
    {
        if (account is null) return "None";
        return account.ClanTier switch
        {
            ClanTier.None => "None",
            ClanTier.Member => "Member",
            ClanTier.Officer => "Officer",
            ClanTier.Leader => "Leader",
            _ => throw new ArgumentOutOfRangeException(@$"Unsupported Clan Tier ""{account.ClanTier}""")
        };
    }

    public static string GetIcon(this Account account)
    {
        if (account is null) return "ai.Default Icon";
        return account.SelectedStoreItems?.SingleOrDefault(item => item.StartsWith("ai.", StringComparison.OrdinalIgnoreCase)) ?? "ai.Default Icon";
    }

    public static string GetIconNoPrefixCode(this Account account)
    {
        return account.GetIcon().Replace("ai.", string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    public static string GetChatSymbol(this Account account)
    {
        if (account is null) return string.Empty;
        return account.SelectedStoreItems?.SingleOrDefault(item => item.StartsWith("cs.", StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
    }

    public static string GetChatSymbolNoPrefixCode(this Account account)
    {
        return account.GetChatSymbol().Replace("cs.", string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    public static string GetNameColour(this Account account)
    {
        if (account is null) return "cc.white";
        return account.SelectedStoreItems?.SingleOrDefault(item => item.StartsWith("cc.", StringComparison.OrdinalIgnoreCase)) ?? "cc.white";
    }

    public static string GetNameColourNoPrefixCode(this Account account)
    {
        string code = account.GetNameColour().Replace("cc.", string.Empty, StringComparison.OrdinalIgnoreCase);
        // FAILSAFE: "frostburnlogo" is a symbol, not a color. Sending it as a color crashes the client.
        if (code.Equals("frostburnlogo", StringComparison.OrdinalIgnoreCase) || code.Length > 10)
        {
            return "white";
        }

        return code;
    }

    public static (string ClanTag, string AccountName) SeparateClanTagFromAccountName(string accountNameWithClanTag)
    {
        // If no '[' and ']' characters are found, then the account is not part of a clan and has no clan tag.
        if (accountNameWithClanTag.Contains('[') == false && accountNameWithClanTag.Contains(']') == false)
        {
            return (string.Empty, accountNameWithClanTag);
        }

        // If '[' is not the first character, then the account name contains the '[' and ']' characters, but the account is not part of a clan and has no clan tag.
        if (accountNameWithClanTag.StartsWith('[') == false)
        {
            return (string.Empty, accountNameWithClanTag);
        }

        // Remove the leading '[' character and split at the first occurrence of the ']' character. The resulting account name may contain the '[' and ']' characters.
        string[] segments = accountNameWithClanTag.TrimStart('[').Split(']', 2);

        return (segments.First(), segments.Last());
    }
}