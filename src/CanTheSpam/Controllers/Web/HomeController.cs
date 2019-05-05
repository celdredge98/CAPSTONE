using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CanTheSpam.Data.CanTheSpamRepository;
using CanTheSpam.Data.CanTheSpamRepository.Interfaces;
using CanTheSpam.Data.CanTheSpamRepository.Models;
using CanTheSpam.Data.Repository.Interfaces;
using CanTheSpam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using reCAPTCHA.AspNetCore;

namespace CanTheSpam.Controllers.Web
{
   public class HomeController : Controller
   {
      private readonly IUnitOfWork _unitOfWork;
      private readonly IEmailListRepository _emailListRepository;
      private readonly IRecaptchaService _recaptcha;

      private readonly ILogger<HomeController> _logger;

      public HomeController(ILogger<HomeController> logger,
         IRecaptchaService recaptcha,
         IUnitOfWork unitOfWork)
      {
         _logger = logger;
         _recaptcha = recaptcha;
         _unitOfWork = unitOfWork;
         _emailListRepository = new EmailListRepository(_unitOfWork);
      }

      public IActionResult Index()
      {
         _logger.LogDebug($"{GetType().Name}.{nameof(Index)} method called...");

         return View();
      }

      public IActionResult Contact()
      {
         _logger.LogDebug($"{GetType().Name}.{nameof(Contact)} method called...");

         return View();
      }


      public IActionResult Privacy()
      {
         _logger.LogDebug($"{GetType().Name}.{nameof(Privacy)} method called...");

         return View();
      }

      public IActionResult TermsOfUse()
      {
         _logger.LogDebug($"{GetType().Name}.{nameof(TermsOfUse)} method called...");

         return View();
      }

      public IActionResult Faq()
      {
         _logger.LogDebug($"{GetType().Name}.{nameof(Faq)} method called...");

         return View();
      }

      [HttpPost]
      public async Task<IActionResult> ValidateEmail(UserEmailDetails userEmail)
      {
         _logger.LogDebug($"{GetType().Name}.{nameof(ValidateEmail)}-Post method called...");

         RecaptchaResponse recaptcha = await _recaptcha.Validate(Request);

         if (!recaptcha.success)
         {
            ViewData["Email"] = $"{userEmail.Email} - Success: {recaptcha.success}";
            ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");
         }
         else
         {
            if (!string.IsNullOrEmpty(userEmail?.Email))
            {
               // if userEmail.Email = "RandomValue@example.com" not in the database "emailItem" == null
               EmailList emailItem = _emailListRepository.GetEntityByEmail(userEmail.Email);
               if (emailItem != null)
               {
                  ViewData["Email"] = $"{emailItem.Email} - Success: {recaptcha.success}";
               }
            }
         }
         // Check for robots using captcha
         //    IF Not Robot continue on
         //    ELSE Robot Goto I hate robot's page --> Page redirect -- Log IP Address, Log in Robot submitted Email table
         // Save email in database
         // mark email with tentative approval pending validation
         // Send email to user using provided email address with "Magic" link to prove ownership
         // User can use the link from email to validate and then it redirects to "Thank you page"
         // or user can enter the code on this page (Validate) page and then goto "Thank you page"

         return View();
      }

      [HttpGet]
      public IActionResult ValidateEmail([FromQuery] string e, [FromQuery] string v)
      {
         _logger.LogDebug($"{GetType().Name}.{nameof(ValidateEmail)}-Get method called...");

         // get control handler

         if (!string.IsNullOrEmpty(e))
         {
            // https://CansTheSpam.com/ValidateEmail?e=user@example.com&v=37AB7F39-CC4D-40DC-B113-DA9452501B1D ([HttpGet])
            // https://CansTheSpam.com/ValidateEmail/?e=user@example.com&v=37AB7F39-CC4D-40DC-B113-DA9452501B1D ([HttpGet])

            EmailList emailItem = _emailListRepository.GetEntityByEmail(e);
            ViewData["Email"] = emailItem.Email;
         }
         // IF email link used and is valid automatically redirect to "Thank You" page
         // ELSE stay on page and report error to use that it failed to validate and try again.

         return View();
      }

      [HttpPost]
      public async Task<IActionResult> EmailCapture(UserEmailDetails userEmail)
      {
         _logger.LogDebug($"{GetType().Name}.{nameof(EmailCapture)} method called...");

         RecaptchaResponse recaptcha = await _recaptcha.Validate(Request);

         if (!recaptcha.success)
         {
            ViewData["Email"] = $"{userEmail.Email} - Success: {recaptcha.success}";
            ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");
         }
         else
         {
            if (!string.IsNullOrEmpty(userEmail?.Email))
            {
               if (userEmail != null)
               {
                  if (!string.IsNullOrEmpty(userEmail?.Email))
                  {
                     EmailList emailListItem = new EmailList()
                     {
                        Id = Guid.NewGuid(),
                        Email = userEmail.Email,
                        IsValidated = false,
                        DateCreated = DateTime.UtcNow
                     };

                     _emailListRepository.Add(emailListItem);
                     _unitOfWork.Save();

                  }

                  ViewData["Email"] = $"{userEmail.Email} - Success: {recaptcha.success}";
               }
            }
         }

         return View();
      }

      [HttpGet]
      public IActionResult ThankYou([FromQuery] string e)
      {
         _logger.LogDebug($"{GetType().Name}.{nameof(ThankYou)} method called...");

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
