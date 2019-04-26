using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CanTheSpam.Data.CanTheSpamRepository.Interfaces;
using CanTheSpam.Data.CanTheSpamRepository.Models;
using CanTheSpam.Data.Repository.Interfaces;
using log4net;
using Microsoft.EntityFrameworkCore;

namespace CanTheSpam.Data.CanTheSpamRepository
{
   public class EmailListRepository : CanTheSpamBaseRepository, IEmailListRepository
   {
      private static readonly ILog _logger = LogManager.GetLogger(typeof(EmailListRepository));

      public EmailListRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
      {
      }

      public EmailList GetEntityById(Guid key)
      {
         _logger.DebugFormat("Method {0} called", MethodBase.GetCurrentMethod().Name);

         return _context.EmailList.FirstOrDefault(e => e.Id == key);
      }

      public IList<EmailList> GetAll()
      {
         _logger.DebugFormat("Method {0} called", MethodBase.GetCurrentMethod().Name);

         return _context.EmailList.ToList();
      }

      public async Task<IList<EmailList>> GetAllAsync()
      {
         _logger.DebugFormat("Method {0} called", MethodBase.GetCurrentMethod().Name);

         return await _context.EmailList.ToListAsync();
      }

      public IQueryable<EmailList> Query(Expression<Func<EmailList, bool>> filter)
      {
         _logger.DebugFormat("Method {0} called", MethodBase.GetCurrentMethod().Name);

         return _context.EmailList.Where(filter);
      }

      IQueryable<EmailList> IEmailListRepository.GetAsQueryable()
      {
         _logger.DebugFormat("Method {0} called", MethodBase.GetCurrentMethod().Name);

         return _context.EmailList.AsQueryable();
      }

      public EmailList GetEntityByEmail(string email)
      {
         _logger.DebugFormat("Method {0} called", MethodBase.GetCurrentMethod().Name);

         return _context.EmailList
            .FirstOrDefault(m =>
               string.Compare(m.Email, email, StringComparison.CurrentCultureIgnoreCase) == 0);
      }

      public async Task<EmailList> GetEntityByEmailAsync(string email)
      {
         _logger.DebugFormat("Method {0} called", MethodBase.GetCurrentMethod().Name);

         return await _context.EmailList
            .FirstOrDefaultAsync(m =>
               string.Compare(m.Email, email, StringComparison.CurrentCultureIgnoreCase) == 0);
      }

      IQueryable<EmailList> ILookupRepository<EmailList, Guid>.GetAsQueryable()
      {
         _logger.DebugFormat("Method {0} called", MethodBase.GetCurrentMethod().Name);

         throw new NotImplementedException();
      }

      public void Add(EmailList entity)
      {
         _logger.DebugFormat("Method {0} called", MethodBase.GetCurrentMethod().Name);

         _context.EmailList.Add(entity);
      }

      public void Delete(EmailList entity)
      {
         _logger.DebugFormat("Method {0} called", MethodBase.GetCurrentMethod().Name);

         _context.EmailList.Remove(entity);
      }

      public void Update(EmailList entity)
      {
         _logger.DebugFormat("Method {0} called", MethodBase.GetCurrentMethod().Name);

         _context.EmailList.Update(entity);
      }
   }
}
