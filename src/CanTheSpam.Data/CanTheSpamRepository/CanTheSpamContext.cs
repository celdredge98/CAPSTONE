using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using CanTheSpam.Data.Repository.Interfaces;
using log4net;
using Microsoft.EntityFrameworkCore;

namespace CanTheSpam.Data.CanTheSpamRepository
{
   public class CanTheSpamContext : CanTheSpamContainer, IUnitOfWork
   {
      private static readonly ILog _logger = LogManager.GetLogger(typeof(CanTheSpamContext));
      
      public CanTheSpamContext(DbContextOptions<CanTheSpamContainer> options) : base(options)
      { }

      public CanTheSpamContext() : base()
      {
      }

      public override int SaveChanges()
      {
         IEnumerable<object> entities = from e in ChangeTracker.Entries()
                                        where e.State == EntityState.Added
                                              || e.State == EntityState.Modified
                                        select e.Entity;

         foreach (object entity in entities)
         {
            ValidationContext validationContext = new ValidationContext(entity);
            Validator.ValidateObject(entity, validationContext);
         }

         return base.SaveChanges();
      }

      /// <summary>
      /// Saves this instance.
      /// </summary>
      public void Save()
      {
         _logger.DebugFormat("Method {0} called", MethodBase.GetCurrentMethod().Name);

         try
         {
            SaveChanges();
         }
         catch (Exception ex)
         {
            _logger.Fatal(ex.Message, ex);

            throw;
         }
      }

      /// <summary>
      /// Reloads the specified entity.
      /// </summary>
      /// <typeparam name="TEntity">The type of the entity.</typeparam>
      /// <param name="entity">The entity.</param>
      public void Reload<TEntity>(TEntity entity) where TEntity : class
      {
         _logger.DebugFormat("Method {0} called", MethodBase.GetCurrentMethod().Name);

         try
         {
            Entry(entity).Reload();
         }
         catch (Exception ex)
         {
            _logger.Debug(ex);
         }
      }

      /// <summary>
      /// Clears the state.
      /// </summary>
      /// <typeparam name="TEntity">The type of the entity.</typeparam>
      /// <param name="entity">The entity.</param>
      public void ClearState<TEntity>(TEntity entity) where TEntity : class
      {
         _logger.DebugFormat("Method {0} called", MethodBase.GetCurrentMethod().Name);

         try
         {
            Entry(entity).CurrentValues.SetValues(Entry(entity).OriginalValues);
         }
         catch (Exception ex)
         {
            _logger.Debug(ex);
         }

         try
         {
            Entry(entity).Reload();
         }
         catch (Exception ex)
         {
            _logger.Debug(ex);
         }

         try
         {
            Entry(entity).State = EntityState.Unchanged;
         }
         catch (Exception ex)
         {
            _logger.Debug(ex);
         }
      }

      /// <summary>
      /// Sets the database context configuration automatic detect changes.
      /// </summary>
      /// <param name="setAutoDetect">if set to <c>true</c> [set automatic detect].</param>
      public void SetDbContextConfigurationAutoDetectChanges(bool setAutoDetect)
      {
         _logger.DebugFormat("Method {0} called", MethodBase.GetCurrentMethod().Name);

         ChangeTracker.AutoDetectChangesEnabled = setAutoDetect;
      }
   }
}