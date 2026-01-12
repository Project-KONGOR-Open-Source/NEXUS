using global::MERRICK.DatabaseContext.Entities.Core;

namespace MERRICK.DatabaseContext.Extensions;

public static class ClanExtensions
{
    public static string GetChatChannelName(this Clan clan)
    {
        return $"Clan {clan.Name}";
    }
}
