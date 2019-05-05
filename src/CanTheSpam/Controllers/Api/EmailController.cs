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

      private readonly ILogger<HomeController> _logger;

      public EmailController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
      {
         _logger = logger;
         _unitOfWork = unitOfWork;
         _emailListRepository = new EmailListRepository(_unitOfWork);
      }

      // GET: api/Email/5
      [HttpGet("{email}", Name = "Email")]
      [Route("Validate")]
      public Task<IActionResult> GetAsync(string email)
      {
         return null;
      }
   }
}
