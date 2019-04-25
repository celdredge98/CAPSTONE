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

      public IActionResult Index()
      {
         _logger.LogDebug($"{GetType().Name}.{nameof(Index)} method called...");

         return View();
      }

      public IActionResult Privacy()
      {
         _logger.LogDebug($"{GetType().Name}.{nameof(Privacy)} method called...");

         return View();
      }

      [HttpPost]
      public IActionResult ValidateEmail(UserEmailDetails userEmail)
      {
         _logger.LogDebug($"{GetType().Name}.{nameof(Privacy)} method called...");

         ViewData["Email"] = userEmail.Email;

         // Check for robots using captcha
         //    IF Not Robot continue on
         //    ELSE Robot Goto I hate robot's page --> Page redirect -- Log IP Address, Log in Robot submitted Email table
         // Save email in database
         // mark email with tentative approval pending validation
         // Send email to user using provided email address with "Magic" link to prove ownership
         // User can use the link from email to validate and then it redirects to "Thank you page"
         // or user can enter the code on this page (Validate) page and then goto "Thank you page"

         // https://CansTheSpam.com/ValidateEmail?e=user@example.com&v=37AB7F39-CC4D-40DC-B113-DA9452501B1D ([HttpGet])
         // https://CansTheSpam.com/ValidateEmail/?e=user@example.com&v=37AB7F39-CC4D-40DC-B113-DA9452501B1D ([HttpGet])

         return View();
      }

      [HttpGet]
      public IActionResult ValidateEmail([FromQuery] string e, [FromQuery] string v)
      {
         // IF email link used and is valid automatically redirect to "Thank You" page
         // ELSE stay on page and report error to use that it failed to validate and try again.

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
