using DAWNBRINGER.WebPortal.UI.Components;
using ASPIRE.Common.ServiceDefaults;

namespace DAWNBRINGER.WebPortal.UI;

public class DAWNBRINGER
{
    // Entry Point For The Web Portal UI Application
    public static void Main(string[] args) 
    { 
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.AddServiceDefaults();
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(builder.Configuration["ApiConfiguration:ZorgathApiUrl"] ?? throw new InvalidOperationException("Zorgath Api Url Is Missing From Configuration")) 
        });

        builder.Services.AddScoped<global::DAWNBRINGER.WebPortal.UI.Services.AuthService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.MapDefaultEndpoints();

        app.Run();
    }
}
