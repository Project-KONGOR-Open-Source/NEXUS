using Microsoft.JSInterop;

namespace DAWNBRINGER.WebPortal.UI.Services;

public class AuthService
{
    private readonly IJSRuntime _js;

    public AuthService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task SaveSessionAsync(string token, int userId)
    {
        await _js.InvokeVoidAsync("auth.save", "authToken", token);
        await _js.InvokeVoidAsync("auth.save", "authUserId", userId.ToString());
    }

    public async Task<(string? Token, int? UserId)> LoadSessionAsync()
    {
        var token = await _js.InvokeAsync<string?>("auth.load", "authToken");
        var userIdStr = await _js.InvokeAsync<string?>("auth.load", "authUserId");

        int? userId = null;
        if (int.TryParse(userIdStr, out var id))
        {
            userId = id;
        }

        return (token, userId);
    }

    public async Task ClearSessionAsync()
    {
        await _js.InvokeVoidAsync("auth.remove", "authToken");
        await _js.InvokeVoidAsync("auth.remove", "authUserId");
    }
}
