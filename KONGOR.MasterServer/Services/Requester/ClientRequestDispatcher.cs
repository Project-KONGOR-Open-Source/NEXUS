using Microsoft.AspNetCore.Mvc;

namespace KONGOR.MasterServer.Services.Requester;

public class ClientRequestDispatcher(IServiceProvider serviceProvider)
{
    public async Task<IActionResult> DispatchAsync(string functionName, HttpContext context)
    {
        if (serviceProvider is IKeyedServiceProvider keyedServiceProvider)
        {
            IClientRequestHandler? handler = keyedServiceProvider.GetKeyedService<IClientRequestHandler>(functionName);
            if (handler is not null)
            {
                return await handler.HandleRequestAsync(context);
            }
        }
        else
        {
            throw new InvalidOperationException("IServiceProvider does not support keyed services.");
        }

        return new BadRequestObjectResult($"Unsupported Client Requester Controller Form Parameter: f={functionName}");
    }
}
