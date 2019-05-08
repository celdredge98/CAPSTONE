using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using CanTheSpam.Data.CanTheSpamRepository;
using CanTheSpam.Data.CanTheSpamRepository.Interfaces;
using CanTheSpam.Data.CanTheSpamRepository.Models;
using CanTheSpam.Data.Repository.Interfaces;
using CanTheSpam.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
      private readonly IHostingEnvironment _hostingEnvironment;
      private readonly ILogger<HomeController> _logger;

      public HomeController(ILogger<HomeController> logger,
         IHostingEnvironment hostingEnvironment,
         IRecaptchaService recaptcha,
         IConfiguration config,
         IUnitOfWork unitOfWork)
      {
         _logger = logger;
         _hostingEnvironment = hostingEnvironment;
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
         ViewData["ErrorMessage"] = string.Empty;

         RecaptchaResponse recaptcha = await _recaptcha.Validate(Request);

         if (!recaptcha.success)
         {
            ViewData["ErrorMessage"] = $"There was an error validating recatpcha. Please try again!";
            ModelState.AddModelError("", "There was an error validating recatpcha. Please try again!");

            return View();
         }
         else
         {
            try
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

                  if (!await SendEmailMessage(this.HttpContext, emailListItem.Id, emailListItem.Email))
                  {
                     ViewData["ErrorMessage"] = "Unable to send confirmation email";
                     return View();
                  }

                  this.HttpContext.Session.Set("Email", Encoding.UTF8.GetBytes(userEmail.Email));

                  return Redirect($"/Home/ValidateEmail/?e={userEmail?.Email}");
               }
            }
            catch (DbUpdateException edu)
            {
               _logger.LogError(edu, edu.Message);
               ViewData["ErrorMessage"] = "This email has already been submitted.";
            }
            catch (SqlException esql)
            {
               _logger.LogError(esql, esql.Message);
               if (esql.Message.ToLower().Contains("cannot insert duplicate key"))
               {
                  ViewData["ErrorMessage"] = "This email has already been submitted.";
               }
               else
               {
                  ViewData["ErrorMessage"] = "There was a critical failure saving to the database.";
               }
            }
         }
         
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

      // From Page Form Post
      [HttpPost]
      public async Task<IActionResult> ValidateEmail(ValidateUserEmail userEmail)
      {
         _logger.LogDebug($"{GetType().Name}.{nameof(ValidateEmail)}-Post method called...");

         if (!string.IsNullOrEmpty(userEmail?.Email))
         {
            EmailList emailItem = await _emailListRepository.GetEntityByEmailAsync(userEmail.Email);
            if (emailItem != null && emailItem.Id.ToString().Equals(userEmail.EmailCode, StringComparison.InvariantCultureIgnoreCase))
            {
               emailItem.IsValidated = true;
               _emailListRepository.Update(emailItem);
               _unitOfWork.Save();

               this.HttpContext.Session.Set("Email", Encoding.UTF8.GetBytes(emailItem.Email));

               return Redirect($"/Home/ThankYou");
            }
         }

         return View();
      }

      // From Email URL Link
      [HttpGet]
      public IActionResult ValidateEmail([FromQuery] string e, [FromQuery] string v)
      {
         _logger.LogDebug($"{GetType().Name}.{nameof(ValidateEmail)}-Get method called...");

         if (!string.IsNullOrEmpty(e) && !string.IsNullOrEmpty(v))
         {
            EmailList emailItem = _emailListRepository.GetEntityByEmail(e);

            if (emailItem != null &&
                emailItem.Email.Equals(e, StringComparison.InvariantCultureIgnoreCase) &&
                emailItem.Id.ToString().Equals(v, StringComparison.InvariantCultureIgnoreCase))
            {
               emailItem.IsValidated = true;
               _emailListRepository.Update(emailItem);
               _unitOfWork.Save();

               this.HttpContext.Session.Set("Email", Encoding.UTF8.GetBytes(emailItem.Email));

               return Redirect($"/Home/ThankYou");
            }
         }
         else if (!string.IsNullOrEmpty(e))
         {
            if (!string.IsNullOrEmpty(e))
            {
               EmailList emailItem = _emailListRepository.GetEntityByEmail(e);
               this.HttpContext.Session.Set("Email", Encoding.UTF8.GetBytes(emailItem.Email));
            }
         }

         return View();
      }

      [HttpGet]
      public async Task<IActionResult> ResendEmail([FromQuery] string e)
      {
         if (await SendEmailMessage(this.HttpContext, Guid.Empty, e))
         {
            return Json(new { Status = "Success", Email = e });
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

      private async Task<bool> SendEmailMessage(Microsoft.AspNetCore.Http.HttpContext httpContext, Guid id, string userEmail)
      {
         SendGridClient sendGridClient = new SendGridClient(_config["AppSettings:SendGridKey"]);

         EmailAddress from = new EmailAddress("support@cansthespam.com");
         EmailAddress to = new EmailAddress(HttpUtility.UrlDecode(userEmail));

         string emailTemplate = System.IO.File.ReadAllText($"{_hostingEnvironment.WebRootPath}\\SendGridTemplate.html", Encoding.UTF8);
         emailTemplate = emailTemplate.Replace("{{userEmail}}", userEmail);

         emailTemplate = emailTemplate.Replace("{{appUrl}}",
            $"{(httpContext.Request.IsHttps ? "https://" : "http://")}{httpContext.Request.Host}/Home/ValidateEmail/?e={userEmail}&v={id}");

         emailTemplate = emailTemplate.Replace("{{userCode}}", id.ToString());

         SendGridMessage msg = MailHelper.CreateSingleEmail(from, to, "CansTheSpam.com - Confirm your email.", emailTemplate, emailTemplate);
         Response response = await sendGridClient.SendEmailAsync(msg);

         switch (response.StatusCode)
         {
            case System.Net.HttpStatusCode.Accepted:
               break;
            default:
               _logger.LogError($"Sendgrid Response: {response.StatusCode}");
               return false;
         }

         return true;
      }
   }
}
