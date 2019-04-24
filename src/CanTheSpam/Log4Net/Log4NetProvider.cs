using System.Collections.Concurrent;
using log4net.Appender;
using Microsoft.Extensions.Logging;

namespace CanTheSpam.Log4Net
{
   public class Log4NetProvider : ILoggerProvider
   {
      #region Fields
      private readonly IAppender[] _appenders;
      private readonly ConcurrentDictionary<string, Log4NetLogger> _loggers = new ConcurrentDictionary<string, Log4NetLogger>();
      #endregion

      public Log4NetProvider(IAppender[] appenders)
      {
         _appenders = appenders;
      }

      #region Public Methods
      public ILogger CreateLogger(string categoryName)
      {
         return _loggers.GetOrAdd(categoryName, CreateLoggerImplementation);
      }
      #endregion

      #region Helper Methods
      private Log4NetLogger CreateLoggerImplementation(string name)
      {
         return new Log4NetLogger(name, _appenders);
      }
      #endregion

      public void Dispose()
      {
         _loggers.Clear();
      }
   }
}
