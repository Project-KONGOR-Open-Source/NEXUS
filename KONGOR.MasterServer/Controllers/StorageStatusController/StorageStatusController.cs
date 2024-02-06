namespace KONGOR.MasterServer.Controllers.StorageStatusController;

[ApiController]
[Route("master/storage/status")]
[Consumes("application/x-www-form-urlencoded")]
public class StorageStatusController : ControllerBase
{
    [HttpPost(Name = "Storage Status")]
    public IActionResult StorageStatus([FromForm] Dictionary<string, string> formData)
    {
        // TODO: Implement Storage Status Controller

        return Ok(@"a:4:{s:7:""success"";b:1;s:4:""data"";N;s:18:""cloud_storage_info"";a:4:{s:10:""account_id"";s:6:""195592"";s:9:""use_cloud"";s:1:""0"";s:16:""cloud_autoupload"";s:1:""0"";s:16:""file_modify_time"";s:19:""2021-01-10 11:39:47"";}s:8:""messages"";s:0:"""";}");
    }
}
