using log4net.Appender;
using Microsoft.Extensions.Logging;

namespace CanTheSpam.Log4Net.Extensions
{
   public static class Log4NetExtensions
   {
      public static ILoggerFactory AddLog4Net(this ILoggerFactory factory, IAppender[] appenders)
      {
         factory.AddProvider(new Log4NetProvider(appenders));
         return factory;
      }
   }
}
