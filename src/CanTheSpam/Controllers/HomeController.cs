using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CanTheSpam.Models;
using System.Threading.Tasks;

namespace CanTheSpam.Controllers
{
   public class HomeController : Controller
   {
      private readonly ILogger<HomeController> _logger;

      public HomeController(ILogger<HomeController> logger)
      {
         _logger = logger;
      }

      public async Task<IActionResult> Index()
      {
         _logger.LogDebug($"{GetType().Name}.{nameof(Index)} method called...");

         return View();
      }

      public IActionResult Privacy()
      {
         _logger.LogDebug($"{GetType().Name}.{nameof(Privacy)} method called...");

         return View();
      }

      [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
      public IActionResult Error()
      {
         _logger.LogDebug($"{GetType().Name}.{nameof(Error)} method called...");

         return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
      }
   }
}
