using System.Collections.Generic;
using CanTheSpam.Data.Repository;
using CanTheSpam.Data.Repository.Interfaces;


namespace CanTheSpam.Data.CanTheSpamRepository
{
   public class CanTheSpamBaseRepository : RepositoryBase<CanTheSpamContext>
   {
      public CanTheSpamBaseRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
      {
      }
   }
}
