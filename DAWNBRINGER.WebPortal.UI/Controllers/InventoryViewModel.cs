namespace DAWNBRINGER.WebPortal.UI.Controllers;

public class InventoryViewModel
{
    public string AccountName { get; set; } = string.Empty;
    public List<string> OwnedItems { get; set; } = new();
    public Dictionary<string, string> CommonItems { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}