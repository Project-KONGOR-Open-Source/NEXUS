namespace KINESIS.Client;

public enum ChatClientStatus
{
    Disconnected,
    Connecting,
    WaitingForAuth,
    Connected,
    JoiningGame,
    InGame
}