using Microsoft.AspNetCore.Mvc;

namespace KONGOR.MasterServer.Services.Requester;

public interface IClientRequestHandler
{
    Task<IActionResult> HandleRequestAsync(HttpContext context);
}
