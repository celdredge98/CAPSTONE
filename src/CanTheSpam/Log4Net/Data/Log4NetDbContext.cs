using CanTheSpam.Log4Net.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CanTheSpam.Log4Net.Data
{
   public class Log4NetDbContext : DbContext
   {
      public virtual DbSet<Log> Logs { get; set; }

      public Log4NetDbContext(DbContextOptions<Log4NetDbContext> options)
         : base(options)
      { }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
         modelBuilder.Entity<Log>().ToTable(nameof(Log));

         base.OnModelCreating(modelBuilder);
      }
   }
}