namespace DAWNBRINGER.WebPortal.UI;

public class DAWNBRINGER
{
    public static void Main(string[] arguments)
    {
        // Create The Application Builder
        WebApplicationBuilder builder = WebApplication.CreateBuilder(arguments);

        // Add Aspire Service Defaults
        builder.AddServiceDefaults();

        // Add Razor Components With Interactive Server Rendering
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Add MudBlazor Component Library Services
        builder.Services.AddMudServices(configuration =>
        {
            configuration.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
            configuration.SnackbarConfiguration.PreventDuplicates = true;
            configuration.SnackbarConfiguration.ShowCloseIcon = true;
            configuration.SnackbarConfiguration.VisibleStateDuration = 5000;
            configuration.SnackbarConfiguration.HideTransitionDuration = 300;
            configuration.SnackbarConfiguration.ShowTransitionDuration = 300;
            configuration.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
        });

        // Configure Named HTTP Client For The Web Portal API
        builder.Services.AddHttpClient(PortalAuthenticationService.HTTPClientName, client =>
        {
            // The Base Address Is Resolved By Aspire Service Discovery At Runtime
            client.BaseAddress = new Uri("https+http://web-portal-api");
        });

        // Register Authentication State Provider And Authentication Service
        builder.Services.AddScoped<PortalAuthenticationService>();
        builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<PortalAuthenticationService>());

        // Add Cascading Authentication State For The Entire Application
        builder.Services.AddCascadingAuthenticationState();

        // Build The Application
        WebApplication application = builder.Build();

        // Configure The HTTP Request Pipeline
        if (application.Environment.IsDevelopment().Equals(false))
        {
            application.UseExceptionHandler("/error");
            application.UseHsts();
        }

        application.UseHttpsRedirection();
        application.UseAntiforgery();
        application.MapStaticAssets();

        // Map Razor Components With Interactive Server Render Mode
        application.MapRazorComponents<Components.App>()
            .AddInteractiveServerRenderMode();

        // Map Default Aspire Endpoints
        application.MapDefaultEndpoints();

        // Start The Application
        application.Run();
    }
}
