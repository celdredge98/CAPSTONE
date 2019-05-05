using System.Net;
using System.Threading.Tasks;
using CanTheSpam.Controllers.Web;
using CanTheSpam.Data.CanTheSpamRepository;
using CanTheSpam.Data.CanTheSpamRepository.Interfaces;
using CanTheSpam.Data.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CanTheSpam.Controllers.Api
{
   [Route("api/[controller]")]
   [ApiController]
   public class EmailController : ControllerBase
   {
      private readonly IUnitOfWork _unitOfWork;
      private readonly IEmailListRepository _emailListRepository;

      private readonly ILogger<EmailController> _logger;

      public EmailController(ILogger<EmailController> logger, IUnitOfWork unitOfWork)
      {
         _logger = logger;
         _unitOfWork = unitOfWork;
         _emailListRepository = new EmailListRepository(_unitOfWork);
      }

      [HttpGet]
      [Route("Validate")]
      public async Task<IActionResult> GetAsync([FromQuery]string email, [FromQuery]string apiKey)
      {
         if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(apiKey))
         {
            return await Task.Run(() => Ok(new {Email = email, ApiKey = apiKey, Status = "Exclude"}));
         }
         // look up API key in database
         // if not found
         //    return error (Access Denied)
         //else
         //    look up record in database
         //    if found
         //       return result
         //    else
         //       return Not found

         return await Task.Run(() => StatusCode((int) HttpStatusCode.BadRequest, new {Email = email, ApiKey = apiKey, Status = string.Empty}));
      }
   }
}
