namespace KINESIS.Server;

public enum ServerStatus
{
    Sleeping,
    Idle,
    Loading,
    Active,
    Crashed,
    Killed,
    Unknown
}