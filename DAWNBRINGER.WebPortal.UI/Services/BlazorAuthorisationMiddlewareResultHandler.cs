namespace DAWNBRINGER.WebPortal.UI.Services;

/// <summary>
///     Custom authorisation middleware result handler for Blazor Server applications.
///     Prevents the HTTP pipeline from issuing authentication challenges (which require a configured scheme) and instead passes all requests through to Blazor, where <see cref="AuthorizeRouteView"/> handles unauthorised access at the component level.
/// </summary>
public class BlazorAuthorisationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    /// <summary>
    ///     Passes all requests through to the next middleware without issuing authentication challenges.
    ///     Blazor's <see cref="AuthorizeRouteView"/> is responsible for displaying the appropriate UI when a user is not authorised.
    /// </summary>
    public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        return next(context);
    }
}
