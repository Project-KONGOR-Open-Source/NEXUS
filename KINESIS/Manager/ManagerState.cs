namespace KINESIS.Manager;

public class ManagerState
{
    public readonly int AccountId;
    public readonly string Login;
    public readonly string Location;
    public readonly string Name;
    public readonly string Version;
    public readonly string Address;
    public readonly short Port;
    public readonly bool ShuttingDown;

    public ManagerState(int accountId, string login, string location, string name, string version, string address, short port, bool shuttingDown)
    {
        AccountId = accountId;
        Login = login;
        Location = location;
        Name = name;
        Version = version;
        Address = address;
        Port = port;
        ShuttingDown = shuttingDown;
    }
}
