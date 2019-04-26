using System;
using System.Linq;
using System.Threading.Tasks;
using CanTheSpam.Data.CanTheSpamRepository.Models;
using CanTheSpam.Data.Repository.Interfaces;

namespace CanTheSpam.Data.CanTheSpamRepository.Interfaces
{
   public interface IEmailListRepository : IRepository<EmailList, Guid>, IDisposable
   {
      new IQueryable<EmailList> GetAsQueryable();

      EmailList GetEntityByEmail(string email);
      Task<EmailList> GetEntityByEmailAsync(string email);

   }
}