using KONGOR.MasterServer.Logging;

using Microsoft.AspNetCore.Mvc;

namespace KONGOR.MasterServer.Controllers.IconsController;

[ApiController]
[Route("icons")]
public partial class IconsController : ControllerBase
{
    private readonly ILogger<IconsController> _logger;
    private readonly IWebHostEnvironment _environment;

    public IconsController(ILogger<IconsController> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    [HttpGet("{*path}")]
    public IActionResult GetIcon(string path)
    {
        // Sanitize path to prevent traversal (basic check)
        if (string.IsNullOrWhiteSpace(path) || path.Contains(".."))
        {
            return BadRequest();
        }

        string fullPath = Path.Combine(_environment.ContentRootPath, "Resources", "Icons", path);

        if (System.IO.File.Exists(fullPath))
        {
            return PhysicalFile(fullPath, "application/octet-stream");
        }

        // Log as debug to reduce noise if icons are missing commonly
        _logger.LogIconNotFound(path);
        return NotFound();
    }
}
