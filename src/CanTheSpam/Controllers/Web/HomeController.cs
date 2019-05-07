using System;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using CanTheSpam.Data.CanTheSpamRepository;
using CanTheSpam.Data.CanTheSpamRepository.Interfaces;
using CanTheSpam.Data.CanTheSpamRepository.Models;
using CanTheSpam.Data.Repository.Interfaces;
using CanTheSpam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using reCAPTCHA.AspNetCore;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CanTheSpam.Controllers.Web
{
   public class HomeController : Controller
   {
      private readonly IUnitOfWork _unitOfWork;
      private readonly IEmailListRepository _emailListRepository;
      private readonly IRecaptchaService _recaptcha;
      private readonly IConfiguration _config;

      private readonly ILogger<HomeController> _logger;

      public HomeController(ILogger<HomeController> logger,
         IRecaptchaService recaptcha,
         IConfiguration config,
         IUnitOfWork unitOfWork)
      {
         _logger = logger;
         _recaptcha = recaptcha;
         _config = config;
         _unitOfWork = unitOfWork;
         _emailListRepository = new EmailListRepository(_unitOfWork);
      }

      [HttpGet]
      public IActionResult Index()
      {
         _logger.LogDebug($"{GetType().Name}.{nameof(Index)} method called...");

         return View();
      }

      [HttpPost]
      public async Task<IActionResult> Index(UserEmailDetails userEmail)
      {
         _logger.LogDebug($"{GetType().Name}.{nameof(Index)} method called...");

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
               EmailList emailListItem = new EmailList()
               {
                  Id = Guid.NewGuid(),
                  Email = userEmail.Email,
                  IsValidated = false,
                  DateCreated = DateTime.UtcNow
               };

               _emailListRepository.Add(emailListItem);
               _unitOfWork.Save();

               bool status = await SendEmailMessage(emailListItem.Id, emailListItem.Email);

               ViewData["Email"] = $"{userEmail.Email}";
            }
         }

         return Redirect($"/Home/ValidateEmail");
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
      public async Task<IActionResult> ValidateEmail(ValidateUserEmail userEmail)
      {
         _logger.LogDebug($"{GetType().Name}.{nameof(ValidateEmail)}-Post method called...");

         if (!string.IsNullOrEmpty(userEmail?.Email))
         {
            EmailList emailItem = await _emailListRepository.GetEntityByEmailAsync(userEmail.Email);
            if (emailItem != null /*&& emailItem.Id.ToString().Equals(userEmail.Email, StringComparison.InvariantCultureIgnoreCase)*/)
            {
               emailItem.IsValidated = true;
               _emailListRepository.Update(emailItem);
               _unitOfWork.Save();
            }
         }

         //truncate table dbo.EmailList


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

         return View();
      }

      [HttpGet]
      public async Task<IActionResult> ResendEmail([FromQuery] string e)
      {
         if (await SendEmailMessage(Guid.Empty, e))
         {
            return Json(new { Status = "Success", Email = e});
         }

         return Json(new { Status = "Failure", Email = e });
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

      private async Task<bool> SendEmailMessage(Guid id, string userEmail)
      {
         SendGridClient sendGridClient = new SendGridClient(_config["AppSettings:SendGridKey"]);

         EmailAddress from = new EmailAddress("support@cansthespam.com");
         EmailAddress to = new EmailAddress(HttpUtility.UrlDecode(userEmail));

         SendGridMessage msg = MailHelper.CreateSingleEmail(from, to, $"Test Email Message", $"email body content{id} with email of: {userEmail}", $"email body content{userEmail}  with email of: {userEmail}");
         Response response = await sendGridClient.SendEmailAsync(msg);

         switch (response.StatusCode)
         {
            case System.Net.HttpStatusCode.Accepted:
               // Process Accepted complete the method and return
               break;
            default:
               _logger.LogError($"Sendgrid Response: {response.StatusCode}");
               return false;
         }

         return true;
      }
   }
}
