using System.Diagnostics;
using JoinAGameTable.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace JoinAGameTable.Controllers
{
    /// <summary>
    /// This controller handle home related methods.
    /// </summary>
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}
