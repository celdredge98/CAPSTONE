using CanTheSpam.Log4Net.Parameters;
using log4net.Appender;
using log4net.Layout;

namespace CanTheSpam.Log4Net.Appenders
{
   public static class Configuration
   {
      public static IAppender CreateConsoleAppender()
      {
         ConsoleAppender appender = new ConsoleAppender() { Name = "ConsoleAppender" };
         PatternLayout layout = new PatternLayout() { ConversionPattern = "%date [%thread] %-5level %logger - %message%newline" };
         layout.ActivateOptions();
         appender.Layout = layout;
         appender.ActivateOptions();
         return appender;
      }

      public static IAppender CreateTraceAppender()
      {
         TraceAppender appender = new TraceAppender() { Name = "TraceAppender" };
         PatternLayout layout = new PatternLayout() { ConversionPattern = "%date [%thread] %-5level %logger - %message%newline" };
         layout.ActivateOptions();
         appender.Layout = layout;
         appender.ActivateOptions();
         return appender;
      }

      public static IAppender CreateRollingFileAppender(string logFilePath)
      {
         RollingFileAppender appender = new RollingFileAppender()
         {
            Name = "RollingFileAppender",
            File = logFilePath,
            AppendToFile = true,
            ImmediateFlush = true,
            DatePattern = ".yyyyMMdd-HHmm",
            MaxSizeRollBackups = 10,
            MaximumFileSize = "10MB",
            CountDirection = 1,
            RollingStyle = RollingFileAppender.RollingMode.Size,
            StaticLogFileName = true,
         };

         PatternLayout layout = new PatternLayout()
         {
            Header = "[Header] ",
            Footer = "[Footer] ",
            ConversionPattern = "%date [%thread] %-5level %logger - %message%newline"
         };

         layout.ActivateOptions();
         appender.Layout = layout;
         appender.ActivateOptions();
         return appender;
      }

      public static IAppender CreateAdoNetAppender(string connectionString, string tableName)
      {
         AdoNetAppender appender = new AdoNetAppender()
         {
            Name = "AdoNetAppender",
            BufferSize = 1,
            ConnectionType = "System.Data.SqlClient.SqlConnection, System.Data, Version = 1.0.3300.0, Culture = neutral, PublicKeyToken = b77a5c561934e089",
            ConnectionString = connectionString,
            CommandText = $"INSERT INTO {tableName} ([Date],[Thread],[Level],[Logger],[Message],[Exception]) VALUES (@log_date, @thread, @log_level, @logger, @message, @exception)"
         };

         AddDateTimeParameterToAppender(appender, "@log_date");
         AddStringParameterToAppender(appender, "@thread", 255, "%thread");
         AddStringParameterToAppender(appender, "@log_level", 50, "%level");
         AddStringParameterToAppender(appender, "@logger", 255, "%logger");
         AddStringParameterToAppender(appender, "@message", -1, "%message");
         AddErrorParameterToAppender(appender, "@exception", -1);

         appender.ActivateOptions();
         return appender;
      }

      #region Helper Methods
      private static void AddStringParameterToAppender(this AdoNetAppender appender, string paramName, int size, string conversionPattern)
      {
         AdoNetAppenderParameter param = new AdoNetAppenderParameter
         {
            ParameterName = paramName,
            DbType = System.Data.DbType.String,
            Size = size,
            Layout = new Layout2RawLayoutAdapter(new PatternLayout(conversionPattern))
         };
         appender.AddParameter(param);
      }

      private static void AddDateTimeParameterToAppender(this AdoNetAppender appender, string paramName)
      {
         AdoNetAppenderParameter param = new AdoNetAppenderParameter
         {
            ParameterName = paramName,
            DbType = System.Data.DbType.DateTime,
            Layout = new RawUtcTimeStampLayout()
         };
         appender.AddParameter(param);
      }

      private static void AddErrorParameterToAppender(this AdoNetAppender appender, string paramName, int size)
      {
         AdoNetAppenderParameter param = new AdoNetAppenderParameter
         {
            ParameterName = paramName,
            DbType = System.Data.DbType.String,
            Size = size,
            Layout = new Layout2RawLayoutAdapter(new ExceptionLayout())
         };
         appender.AddParameter(param);
      }
      #endregion
   }
}