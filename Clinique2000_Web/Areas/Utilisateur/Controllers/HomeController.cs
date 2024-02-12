using Clinique2000_Models.Models;
using Clinique2000_Services.Services;
using Clinique2000_Services.Services.IServices;
using Clinique2000_Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Clinique2000_Web.Areas.Utilisateur.Controllers
{
    [Area("Utilisateur")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ContexteService _contexteService;
        private readonly IUserService _userService;

        public HomeController(ILogger<HomeController> logger, ContexteService contexteService, IUserService userService)
        {
            _logger = logger;
            _contexteService = contexteService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


        public IActionResult Register()
        {
            return View(new RegisterDTO());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}