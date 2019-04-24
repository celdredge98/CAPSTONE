﻿using System;
using System.Reflection;
using log4net;
using log4net.Appender;
using log4net.Repository;
using Microsoft.Extensions.Logging;

namespace CanTheSpam.Log4Net
{
   internal class Log4NetLogger : ILogger
   {
      #region Fields
      private readonly string _name;
      private readonly ILog _log;
      private static ILoggerRepository _loggerRepository;
      #endregion

      public Log4NetLogger(string name, IAppender[] appenders)
      {
         _name = name;

         if (_loggerRepository != null)
            _log = LogManager.GetLogger(_loggerRepository.Name, name);

         if (_log == null)
         {
            _loggerRepository = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            _log = LogManager.GetLogger(_loggerRepository.Name, name);
            log4net.Config.BasicConfigurator.Configure(_loggerRepository, appenders);
         }
      }

      #region Public Methods
      public IDisposable BeginScope<TState>(TState state) => null;

      public bool IsEnabled(LogLevel logLevel)
      {
         switch (logLevel)
         {
            case LogLevel.Critical:
               return _log.IsFatalEnabled;
            case LogLevel.Debug:
            case LogLevel.Trace:
               return _log.IsDebugEnabled;
            case LogLevel.Error:
               return _log.IsErrorEnabled;
            case LogLevel.Information:
               return _log.IsInfoEnabled;
            case LogLevel.Warning:
               return _log.IsWarnEnabled;
            default:
               throw new ArgumentOutOfRangeException(nameof(logLevel));
         }
      }

      public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
      {
         if (!IsEnabled(logLevel))
         {
            return;
         }

         if (formatter == null)
         {
            throw new ArgumentNullException(nameof(formatter));
         }

         Log(logLevel, state, exception, formatter);
      }

      private void Log<TState>(LogLevel logLevel, TState state, Exception exception, Func<TState, Exception, string> formatter)
      {
         string message = null;

         if (null != formatter)
         {
            message = formatter(state, exception);
         }

         if (!string.IsNullOrEmpty(message) || exception != null)
         {
            switch (logLevel)
            {
               case LogLevel.Critical:
                  _log.Fatal(message, exception);
                  break;
               case LogLevel.Debug:
               case LogLevel.Trace:
                  _log.Debug(message, exception);
                  break;
               case LogLevel.Error:
                  _log.Error(message, exception);
                  break;
               case LogLevel.Information:
                  _log.Info(message, exception);
                  break;
               case LogLevel.Warning:
                  _log.Warn(message, exception);
                  break;
               default:
                  _log.Warn($"Encountered unknown log level {logLevel}, writing out as Info.");
                  _log.Info(message, exception);
                  break;
            }
         }
      }
      #endregion
   }
}