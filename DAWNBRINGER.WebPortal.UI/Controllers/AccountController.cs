using System.Security.Claims;

using ASPIRE.Common.DTOs;

using DAWNBRINGER.WebPortal.UI.Logging;
using DAWNBRINGER.WebPortal.UI.Models;

using MERRICK.DatabaseContext.Entities.Core;
using MERRICK.DatabaseContext.Persistence;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace DAWNBRINGER.WebPortal.UI.Controllers;

public partial class AccountController : Controller
{
    private readonly MerrickContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AccountController> _logger;
    private readonly IOutputCacheStore _outputCacheStore;
    private readonly IConfiguration _configuration;

    public AccountController(MerrickContext context, IHttpClientFactory httpClientFactory, ILogger<AccountController> logger, IOutputCacheStore outputCacheStore, IConfiguration configuration)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _outputCacheStore = outputCacheStore;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpGet]
    public IActionResult Signup()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        string? apiKey = _configuration["Operational:Email:ApiKey"];
        bool hasApiKey = !string.IsNullOrWhiteSpace(apiKey) && apiKey != "SENDGRID_API_KEY_PLACEHOLDER";
        ViewBag.HasApiKey = hasApiKey;

        return View();
    }

    [HttpPost]
    public IActionResult LoginDiscord()
    {
        string redirectUrl = Url.Action("DiscordHandshake", "Account")!;
        return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, "Discord");
    }

    [HttpPost]
    public IActionResult SignupDiscord()
    {
        string redirectUrl = Url.Action("DiscordHandshake", "Account")!;
        return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, "Discord");
    }

    [HttpGet]
    public async Task<IActionResult> DiscordHandshake()
    {
        // User should be authenticated via Cookie at this point (handled by Middleware)
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login");
        }

        string? email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            // Should not happen with Discord scope
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        // Check if User exists in DB and fetch Account for IGN
        User? user = await _context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.EmailAddress == email);

        if (user != null)
        {
            string ign = user.Accounts.FirstOrDefault()?.Name ?? email;

            // Refresh the authentication cookie with the IGN
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, ign), // Use IGN
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, email)
            };

            ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            // Login Complete
            return RedirectToAction("Index", "Home");
        }

        // New User -> Registration Flow
        return RedirectToAction("Register");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CompleteRegistration(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Register", model);
        }

        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login");
        }

        string? email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return RedirectToAction("Login");
        }

        // Check if IGN is taken
        if (await _context.Accounts.AnyAsync(a => a.Name == model.IGN))
        {
            ModelState.AddModelError("IGN", "This IGN is already taken.");
            return View("Register", model);
        }

        try
        {
            RegisterDirectUserDTO dto = new(email, model.IGN, model.Password);
            HttpClient client = _httpClientFactory.CreateClient("ZORGATH");
            HttpResponseMessage response = await client.PostAsJsonAsync("/Registration/Register", dto);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                _logger.LogApiRegistrationFailed((int) response.StatusCode, error);
                ModelState.AddModelError(string.Empty, $"Registration failed: {response.StatusCode} - {error}");
                return View("Register", model);
            }

            // Refresh the authentication cookie with the new IGN
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, model.IGN), // Use IGN as Name
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, email) // Or user ID if returned
            };

            ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogRegistrationException(ex);
            ModelState.AddModelError(string.Empty, $"An error occurred during registration: {ex.Message}");
            return View("Register", model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> LoginMock(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToAction("Login");
        }

        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.Name, email), new Claim(ClaimTypes.Email, email), new Claim("MockUser", "true")
        };

        ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> SignupMock(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToAction("Signup");
        }

        // Check if User exists to decide flow
        User? user = await _context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.EmailAddress == email);

        if (user != null)
        {
            // User exists, just log them in
            string ign = user.Accounts.FirstOrDefault()?.Name ?? email;

            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, ign),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, email),
                new Claim("MockUser", "true")
            };

            ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }
        else
        {
            // New User -> Redirect to Register to choose IGN/Password
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, email), // Temporary Name until IGN chosen
                new Claim(ClaimTypes.Email, email),
                new Claim("MockUser", "true")
            };

            ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Register");
        }
    }

    [HttpGet]
    public IActionResult RegisterDirect()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RegisterDirect(RegisterEmailViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        try
        {
            RegisterEmailAddressDTO dto = new(model.Email, model.Email);
            HttpClient client = _httpClientFactory.CreateClient("ZORGATH");
            HttpResponseMessage response = await client.PostAsJsonAsync("/Registration/InitiateRegistration", dto);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                _logger.LogApiRegistrationFailed((int) response.StatusCode, error);
                ModelState.AddModelError(string.Empty, $"Registration failed: {response.StatusCode} - {error}");
                return View(model);
            }

            ViewBag.Email = model.Email;
            return View("CheckEmail");
        }
        catch (Exception ex)
        {
            _logger.LogRegistrationException(ex);
            ModelState.AddModelError(string.Empty, $"An error occurred during registration: {ex.Message}");
            return View(model);
        }
    }

    // This handles the link from email: /Account/RegisterComplete?token=...
    // Or if route is mapped: /register/{token} -> We might need a route attribute or update Program.cs
    // The EmailService generates: BaseURL + "/register/" + token
    // If DAWNBRINGER is BaseURL, we need a route for "register/{token}" to map here.
    [HttpGet("register/{token}")]
    public IActionResult RegisterComplete(string token)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        RegisterCompleteViewModel model = new() { Token = token, IGN = string.Empty, Password = string.Empty, ConfirmPassword = string.Empty };
        return View("RegisterComplete", model);
    }

    [HttpPost("register/complete")]
    public async Task<IActionResult> RegisterComplete(RegisterCompleteViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        try
        {
            RegisterUserWithTokenDTO dto = new(model.Token, model.IGN, model.Password);
            HttpClient client = _httpClientFactory.CreateClient("ZORGATH");
            HttpResponseMessage response = await client.PostAsJsonAsync("/Registration/CompleteRegistration", dto);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                _logger.LogApiRegistrationFailed((int) response.StatusCode, error);
                ModelState.AddModelError(string.Empty, $"Registration failed: {response.StatusCode} - {error}");
                return View(model);
            }

            // Immediately sign in the new user
            // We need to decode the response to get Email/ID if possible, or trust the inputs?
            // The response for CompleteRegistration returns GetBasicUserDTO per code.

            GetBasicUserDTO? userDto = await response.Content.ReadFromJsonAsync<GetBasicUserDTO>();

            if (userDto is not null)
            {
                // Create Claims based on returned user
                List<Claim> claims = new()
                {
                    new Claim(ClaimTypes.Name, userDto.Accounts.FirstOrDefault()?.Name ?? model.IGN),
                    new Claim(ClaimTypes.Email, userDto.EmailAddress),
                    new Claim(ClaimTypes.NameIdentifier, userDto.EmailAddress)
                };

                ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));
            }

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogRegistrationException(ex);
            ModelState.AddModelError(string.Empty, $"An error occurred during registration: {ex.Message}");
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}