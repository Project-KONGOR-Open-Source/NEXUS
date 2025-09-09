﻿namespace KONGOR.MasterServer.Models.ServerManagement;

public class MatchServerManager
{
    public required int HostAccountID { get; set; }

    public required string HostAccountName { get; set; }

    public required int ID { get; set; }

    public required string IPAddress { get; set; }

    public string Cookie { get; set; } = Guid.NewGuid().ToString();

    public DateTimeOffset TimestampRegistered { get; set; } = DateTimeOffset.UtcNow;
}
