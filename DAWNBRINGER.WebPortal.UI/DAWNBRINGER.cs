using DAWNBRINGER.WebPortal.UI.Components;

namespace DAWNBRINGER.WebPortal.UI;

public class DAWNBRINGER
{
    // Entry Point For The Web Portal UI Application
    public static void Main(string[] args) 
    { 
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Register HttpClient to talk to ZORGATH API
        // In development, we might use the Aspire service discovery name or localhost if not using Aspire for this part yet.
        // The implementation plan says to listen on 55510.
        // For outgoing requests to ZORGATH:
        // Based on ZORGATH launch settings, it is on https://localhost:55556
        
        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri("https://localhost:55556") 
        });

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

        app.Run();
    }
}
