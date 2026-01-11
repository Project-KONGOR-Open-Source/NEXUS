namespace TRANSMUTANSTEIN.ChatServer.Domain.Clans;

public interface IPendingClanService
{
    void RemoveObsoledPendingClans();
    bool IsUserInPendingClans(Account account);
    bool IsClanInPendingClans(string name, string tag);
    void InsertPendingClan(PendingClan pendingClan);
    void RemovePendingClan(string pendingClanTagAndName);
    PendingClan? GetPendingClanForUser(Account account);

    // Invite Helpers
    void RemoveObsoledPendingClanInvites();
    bool IsUserInPendingClanInvites(string username);
    string? GetPendingClanInviteKeyForUser(Account account);
    void InsertPendingClanInvite(string key, PendingClanInvite invite);
    void RemovePendingClanInvite(string key);
    PendingClanInvite? GetPendingClanInvite(string key);
}

public class PendingClanService : IPendingClanService
{
    // Key: Invite Key? Legacy didn't specify key format clearly in helper but used Dictionary.
    // Legacy InsertPendingClanInvite just added to dictionary. 
    // We will assume Key is Username or something unique. 
    // Legacy GetPendingClanInviteKeyForUser iterates values.
    private readonly ConcurrentDictionary<string, PendingClanInvite> _pendingClanInvites = new();

    // Key: [tag]name (lowercase)
    private readonly ConcurrentDictionary<string, PendingClan> _pendingClans = new();

    public void RemoveObsoledPendingClans()
    {
        foreach (KeyValuePair<string, PendingClan> kvp in _pendingClans)
        {
            if (!kvp.Value.IsValid)
            {
                _pendingClans.TryRemove(kvp.Key, out _);
            }
        }
    }

    public bool IsUserInPendingClans(Account account)
    {
        foreach (KeyValuePair<string, PendingClan> kvp in _pendingClans)
        {
            if (kvp.Value.MembersAccountId.Contains(account.ID) || kvp.Value.CreatorAccountId == account.ID)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsClanInPendingClans(string name, string tag)
    {
        foreach (KeyValuePair<string, PendingClan> kvp in _pendingClans)
        {
            if (kvp.Value.ClanName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                kvp.Value.ClanTag.Equals(tag, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public void InsertPendingClan(PendingClan pendingClan)
    {
        string key = $"[{pendingClan.ClanTag.ToLower()}]{pendingClan.ClanName.ToLower()}";
        _pendingClans[key] = pendingClan;
    }

    public void RemovePendingClan(string pendingClanTagAndName)
    {
        _pendingClans.TryRemove(pendingClanTagAndName, out _);
    }

    public PendingClan? GetPendingClanForUser(Account account)
    {
        foreach (KeyValuePair<string, PendingClan> kvp in _pendingClans)
        {
            if (kvp.Value.MembersAccountId.Contains(account.ID))
            {
                return kvp.Value;
            }
        }

        return null;
    }

    public void RemoveObsoledPendingClanInvites()
    {
        foreach (KeyValuePair<string, PendingClanInvite> kvp in _pendingClanInvites)
        {
            if (!kvp.Value.IsValid)
            {
                _pendingClanInvites.TryRemove(kvp.Key, out _);
            }
        }
    }

    public bool IsUserInPendingClanInvites(string username)
    {
        return _pendingClanInvites.ContainsKey(username);
    }

    public string? GetPendingClanInviteKeyForUser(Account account)
    {
        foreach (KeyValuePair<string, PendingClanInvite> kvp in _pendingClanInvites)
        {
            if (kvp.Value.InvitedAccountId == account.ID)
            {
                return kvp.Key;
            }
        }

        return null;
    }

    public void InsertPendingClanInvite(string key, PendingClanInvite invite)
    {
        _pendingClanInvites[key] = invite;
    }

    public void RemovePendingClanInvite(string key)
    {
        _pendingClanInvites.TryRemove(key, out _);
    }

    public PendingClanInvite? GetPendingClanInvite(string key)
    {
        if (_pendingClanInvites.TryGetValue(key, out PendingClanInvite? invite))
        {
            return invite;
        }

        return null;
    }
}