namespace KINESIS.Client;

public class ChatChannelUser
{
    public readonly string DisplayedName;
    public readonly int AccountId;
    public readonly ChatClientStatus ChatClientStatus;
    public readonly ChatAdminLevel ChatAdminLevel;
    public readonly string ChatSymbol;
    public readonly string ChatNameColour;
    public readonly string AccountIcon;
    public readonly int AscensionLevel;

    public ChatChannelUser(string displayedName, int accountId, ChatClientStatus chatClientStatus, ChatAdminLevel chatAdminLevel, string chatSymbol, string chatNameColour, string accountIcon, int ascensionLevel)
    {
        DisplayedName = displayedName;
        AccountId = accountId;
        ChatClientStatus = chatClientStatus;
        ChatAdminLevel = chatAdminLevel;
        ChatSymbol = chatSymbol;
        ChatNameColour = chatNameColour;
        AccountIcon = accountIcon;
        AscensionLevel = ascensionLevel;
    }
}

public class ChatChannel
{
    // private static int _lastChannelId = 0;
    // private int _id;
    private readonly string _name;
    public string Name => _name;
    private readonly string _upperCaseName;
    public string UpperCaseName => _upperCaseName;
    private readonly string _topic;
    public string Topic => _topic;
    private readonly ChatChannelFlags _flags;
    public ChatChannelFlags Flags => _flags;
    private readonly List<ChatChannelUser> _users = new();
    // private bool _channelIsFull = false;



    public ChatChannel(string name, string upperCaseName, string topic, ChatChannelFlags flags)
    {
        _name = name;
        _upperCaseName = upperCaseName;
        _topic = topic;
        _flags = flags;
    }


    // Logic removed
    /*
    public void CollectAccountIds(HashSet<int> accountIds) { ... }
    public void SendMessage(int accountId, string message) { ... }
    */
}
