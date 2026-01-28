using ASPIRE.Common.ServiceDefaults;

using MERRICK.DatabaseContext.Persistence;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add Aspire Service Defaults
builder.AddServiceDefaults();

// Add MVC Controllers with Views
// Add MVC Controllers with Views
builder.Services.AddControllersWithViews();

// Add Output Caching
builder.Services.AddOutputCache();

// Add HttpClient for ZORGATH API
builder.Services.AddHttpClient("ZORGATH", client =>
{
    client.BaseAddress = new Uri("http://web-portal-api");
});

// Add The Database Context
builder.AddSqlServerDbContext<MerrickContext>("MERRICK", null, options =>
{
    // Enable Detailed Error Messages In Development Environment
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());

    // Enable Thread Safety Checks For Entity Framework
    options.EnableThreadSafetyChecks();
});

// Configure Authentication
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "Discord";
    })
    .AddCookie()
    .AddDiscord(options =>
    {
        options.ClientId = builder.Configuration["discord-client-id"] ?? "MISSING_CLIENT_ID";
        options.ClientSecret = builder.Configuration["discord-client-secret"] ?? "MISSING_CLIENT_SECRET";
        options.CallbackPath = new PathString("/signin-discord");

        // Map Discord User Information to Claims
        options.ClaimActions.MapJsonKey("urn:discord:avatar", "avatar");
        options.ClaimActions.MapJsonKey("urn:discord:discriminator", "discriminator");

        options.SaveTokens = true;
    });

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseOutputCache();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

app.Run();