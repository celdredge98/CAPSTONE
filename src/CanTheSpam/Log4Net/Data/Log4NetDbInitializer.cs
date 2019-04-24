namespace CanTheSpam.Log4Net.Data
{
   public static class Log4NetDbInitializer
   {
      public static void Initialize(Log4NetDbContext context)
      {
         context.Database.EnsureCreated();
      }
   }
}