namespace KINESIS.Client;

[Flags]
public enum ChatClientFlags
{
    None = 0,
    IsClanOfficer = 1 << 0,
    IsClanLeader = 1 << 1,
    IsStaff = 1 << 5,
    IsPremium = 1 << 6,
    IsVerified = 1 << 7
}