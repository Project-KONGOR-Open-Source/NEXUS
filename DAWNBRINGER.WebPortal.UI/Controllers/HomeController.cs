using Microsoft.AspNetCore.Mvc;

namespace DAWNBRINGER.WebPortal.UI.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}