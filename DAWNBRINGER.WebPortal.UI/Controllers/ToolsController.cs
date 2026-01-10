using MERRICK.DatabaseContext.Entities.Core;
using MERRICK.DatabaseContext.Persistence;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DAWNBRINGER.WebPortal.UI.Controllers;

[Route("tools")]
public class ToolsController : Controller
{
    private readonly MerrickContext _context;
    private readonly ILogger<ToolsController> _logger;

    public ToolsController(MerrickContext context, ILogger<ToolsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("inventory")]
    public async Task<IActionResult> Inventory(string? accountName)
    {
        InventoryViewModel viewModel = new();

        // Curated List of Popular Items
        viewModel.CommonItems = new Dictionary<string, string>
        {
            // Avatars
            { "Avatar_Legionnaire.Plogger", "Legionnaire: Plogger" },
            { "Avatar_Devourer.Oni", "Devourer: Oni" },
            { "Avatar_Predator.Xenomorph", "Predator: Xenomorph" },
            { "Avatar_FlintBeastwood.Rambo", "Flint: Rambo" },
            { "Avatar_Swiftblade.Ronin", "Swiftblade: Ronin" },
            { "Avatar_WitchSlayer.Pimp", "Witch Slayer: Pimp" },
            { "Avatar_Accursed.Arthas", "Accursed: Arthas" },

            // Announcers
            { "Announcer_Flamboyant", "Announcer: Flamboyant" },
            { "Announcer_English", "Announcer: English" },
            { "Announcer_Badass", "Announcer: Badass" },
            { "Announcer_Thai", "Announcer: Thai" },

            // Taunts
            { "Taunt_Kongor", "Taunt: Kongor" },
            { "Taunt_GDrop", "Taunt: G-Drop" },
            { "Taunt_Standard", "Taunt: Standard" },

            // Account Icons
            { "AccountIcon_HoN", "Icon: HoN Logo" },
            { "AccountIcon_Legion", "Icon: Legion" },
            { "AccountIcon_Hellbourne", "Icon: Hellbourne" },

            // Name Colors
            { "NameColor_Gold", "Name Color: Gold" },
            { "NameColor_Diamond", "Name Color: Diamond" }
        };

        if (string.IsNullOrWhiteSpace(accountName))
        {
            return View(viewModel);
        }

        Account? account = await _context.Accounts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Name == accountName);

        if (account == null)
        {
            viewModel.ErrorMessage = $"Account '{accountName}' not found.";
            return View(viewModel);
        }

        viewModel.AccountName = account.Name;
        viewModel.OwnedItems = account.User.OwnedStoreItems;
        viewModel.Message = "Account loaded.";

        return View(viewModel);
    }

    [HttpPost("inventory/add")]
    public async Task<IActionResult> AddItem(string accountName, string productCode)
    {
        Account? account = await _context.Accounts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Name == accountName);

        if (account == null)
        {
            return RedirectToAction("Inventory", new { accountName, error = "Account not found" });
        }

        if (!account.User.OwnedStoreItems.Contains(productCode))
        {
            account.User.OwnedStoreItems.Add(productCode);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Inventory", new { accountName });
    }

    [HttpPost("inventory/remove")]
    public async Task<IActionResult> RemoveItem(string accountName, string productCode)
    {
        Account? account = await _context.Accounts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Name == accountName);

        if (account == null)
        {
            return RedirectToAction("Inventory", new { accountName, error = "Account not found" });
        }

        if (account.User.OwnedStoreItems.Contains(productCode))
        {
            account.User.OwnedStoreItems.Remove(productCode);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Inventory", new { accountName });
    }

    [HttpPost("inventory/reset")]
    public async Task<IActionResult> ResetInventory(string accountName)
    {
        Account? account = await _context.Accounts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Name == accountName);

        if (account == null)
        {
            return RedirectToAction("Inventory", new { accountName, error = "Account not found" });
        }

        account.User.OwnedStoreItems.Clear();
        await _context.SaveChangesAsync();

        return RedirectToAction("Inventory", new { accountName });
    }
}

public class InventoryViewModel
{
    public string AccountName { get; set; } = string.Empty;
    public List<string> OwnedItems { get; set; } = new();
    public Dictionary<string, string> CommonItems { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}