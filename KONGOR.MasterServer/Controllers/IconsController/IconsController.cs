using Microsoft.AspNetCore.Mvc;

namespace KONGOR.MasterServer.Controllers.IconsController;

[ApiController]
[Route("icons")]
public class IconsController : ControllerBase
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
        _logger.LogWarning("Icon Requested: {Path}", path);

        // TODO: Implement Dynamic Icon Generation Logic
        // The path likely contains slash-separated flags followed by comma-separated assets
        // Example: 0/8/12/1,av.Flamboyant,c.cat_courier,...

        // For now, check if we have a static file fallback or return 404
        // If we want to serve a placeholder, we could do it here.
        
        return NotFound();
    }
}
