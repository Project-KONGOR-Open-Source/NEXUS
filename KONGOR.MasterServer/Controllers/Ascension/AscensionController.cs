namespace KONGOR.MasterServer.Controllers.Ascension;

[ApiController]
[Route(TextConstant.EmptyString)]
public class AscensionController : ControllerBase
{
    [HttpGet("/", Name = "Ascension Root")]
    [HttpGet("index.php", Name = "Ascension Index")]
    public IActionResult GetAscension()
    {
        return Ok(@"{ ""error_code"": 100, ""data"": { ""is_season_match"": true } }");
    }
}