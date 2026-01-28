namespace KONGOR.MasterServer.Models.RequestResponse.Store;

public class Product
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string IconPath { get; init; } // e.g. /heroes/accursed/icon.tga
    public required int CostGold { get; init; }
    public required int CostSilver { get; init; }
    public required string ProductCode { get; init; } // e.g. h.accursed
    public required string Type { get; init; } // Hero, Avatar, AccountIcon, etc.
}
