namespace KONGOR.MasterServer.Controllers.PatcherController;

[ApiController]
[Route("patcher/patcher.php")]
[Consumes("application/x-www-form-urlencoded")]
public class PatcherController : ControllerBase
{
    [HttpPost(Name = "Patcher")]
    public IActionResult Patcher()
    {
        // TODO: Implement Patcher Controller

        return Ok(@"a:5:{i:0;a:8:{s:7:""version"";s:6:""4.10.1"";s:2:""os"";s:3:""wac"";s:4:""arch"";s:6:""x86_64"";s:3:""url"";s:42:""http://cdn.naeu.patch.heroesofnewerth.com/"";s:4:""url2"";s:42:""http://cdn.naeu.patch.heroesofnewerth.com/"";s:14:""latest_version"";s:6:""4.10.1"";s:24:""latest_manifest_checksum"";s:40:""33b5151fca1704aff892cf76e41f3986634d38bb"";s:20:""latest_manifest_size"";s:7:""3628533"";}s:7:""version"";s:8:""4.10.1.0"";s:15:""current_version"";s:8:""4.10.1.0"";s:25:""current_manifest_checksum"";s:40:""33b5151fca1704aff892cf76e41f3986634d38bb"";s:21:""current_manifest_size"";s:7:""3628533"";}");
    }
}
