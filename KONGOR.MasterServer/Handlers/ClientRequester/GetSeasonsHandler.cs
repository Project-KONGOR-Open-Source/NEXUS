using KONGOR.MasterServer.Services; // For PhpSerialization
using KONGOR.MasterServer.Services.Requester;
using Microsoft.AspNetCore.Mvc;

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public class GetSeasonsHandler : IClientRequestHandler
{
    public Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        // Legacy K2 expects a pipe-delimited string of season IDs in arg[1].
        // Lua: _historicalSeasons = explode('|', arg[1])
        
        // Generate a list of seasons (e.g. 1 to 20)
        // In a real implementation, this might come from a database.
        string seasons = string.Join("|", Enumerable.Range(1, 20));

        // Return as a PHP serialized array where index 0 is the seasons string.
        // This maps to arg[1] in Lua (0-based C# -> 1-based Lua correction is usually handled, 
        // but here we are using standard PHP array with integer key 0).
        // Logan (2025-02-24): Restored to a flat list so the S2 Engine vararg unwrapper treats it as a contiguous vector array natively.
        List<string> response = new List<string> { seasons };

        return Task.FromResult<IActionResult>(new ContentResult { Content = PhpSerialization.Serialize(response), ContentType = "text/html" });
    }
}
