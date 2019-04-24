using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Reflection;
using CanTheSpam.Log4Net.Parameters;
using log4net.Appender;
using log4net.Core;
using log4net.Util;

namespace CanTheSpam.Log4Net.Appenders
{
   public class AdoNetAppender : BufferingAppenderSkeleton
   {
      private static readonly Type DeclaringType = typeof(AdoNetAppender);

      #region Fields
      private IDbCommand _dbCommand;
      protected bool UsePreparedCommand;
      protected ArrayList Parameters;
      #endregion

      #region Properties
      public string ConnectionString { get; set; }

      public string AppSettingsKey { get; set; }

      public string ConnectionType { get; set; }

      public string CommandText { get; set; }

      public CommandType CommandType { get; set; } = CommandType.Text;

      public bool UseTransactions { get; set; } = true;

      public SecurityContext SecurityContext { get; set; }

      public bool ReconnectOnError { get; set; } = false;

      protected IDbConnection Connection { get; set; }
      #endregion

      public AdoNetAppender()
      {
         ConnectionType = "System.Data.OleDb.OleDbConnection, System.Data, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
         Parameters = new ArrayList();
      }

      #region Methods
      public override void ActivateOptions()
      {
         base.ActivateOptions();

         UsePreparedCommand = !string.IsNullOrEmpty(CommandText);

         if (SecurityContext == null)
         {
            SecurityContext = SecurityContextProvider.DefaultProvider.CreateSecurityContext(this);
         }

         InitializeDatabaseConnection();
         InitializeDatabaseCommand();
      }

      public void AddParameter(AdoNetAppenderParameter parameter)
      {
         Parameters.Add(parameter);
      }

      protected override void OnClose()
      {
         base.OnClose();
         DisposeCommand(false);
         DisposeConnection();
      }

      protected override void SendBuffer(LoggingEvent[] events)
      {
         if (ReconnectOnError && (Connection == null || Connection.State != ConnectionState.Open))
         {
            LogLog.Debug(DeclaringType, "Attempting to reconnect to database. Current Connection State: " + ((Connection == null) ? SystemInfo.NullText : Connection.State.ToString()));

            InitializeDatabaseConnection();
            InitializeDatabaseCommand();
         }

         if (Connection != null && Connection.State == ConnectionState.Open)
         {
            if (UseTransactions)
            {
               IDbTransaction dbTran = null;
               try
               {
                  dbTran = Connection.BeginTransaction();
                  SendBuffer(dbTran, events);
                  dbTran.Commit();
               }
               catch (Exception ex)
               {
                  if (dbTran != null)
                  {
                     try
                     {
                        dbTran.Rollback();
                     }
                     catch (Exception)
                     {
                        // ignored
                     }
                  }

                  ErrorHandler.Error("Exception while writing to database", ex);
               }
            }
            else
            {
               SendBuffer(null, events);
            }
         }
      }

      protected virtual void SendBuffer(IDbTransaction dbTran, LoggingEvent[] events)
      {
         if (UsePreparedCommand)
         {
            if (_dbCommand != null)
            {
               if (dbTran != null)
               {
                  _dbCommand.Transaction = dbTran;
               }

               foreach (LoggingEvent e in events)
               {
                  foreach (AdoNetAppenderParameter param in Parameters)
                  {
                     param.FormatValue(_dbCommand, e);
                  }

                  _dbCommand.ExecuteNonQuery();
               }
            }
         }
         else
         {
            using (IDbCommand dbCmd = Connection.CreateCommand())
            {
               if (dbTran != null)
               {
                  dbCmd.Transaction = dbTran;
               }

               foreach (LoggingEvent e in events)
               {
                  string logStatement = GetLogStatement(e);
                  LogLog.Debug(DeclaringType, "LogStatement [" + logStatement + "]");
                  dbCmd.CommandText = logStatement;
                  dbCmd.ExecuteNonQuery();
               }
            }
         }
      }

      protected virtual string GetLogStatement(LoggingEvent logEvent)
      {
         if (Layout == null)
         {
            ErrorHandler.Error("AdoNetAppender: No Layout specified.");
            return string.Empty;
         }
         else
         {
            StringWriter writer = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
            Layout.Format(writer, logEvent);
            return writer.ToString();
         }
      }

      protected virtual IDbConnection CreateConnection(Type connectionType, string connectionString)
      {
         IDbConnection connection = (IDbConnection)Activator.CreateInstance(connectionType);
         connection.ConnectionString = connectionString;
         return connection;
      }

      protected virtual string ResolveConnectionString(out string connectionStringContext)
      {
         if (!string.IsNullOrEmpty(ConnectionString))
         {
            connectionStringContext = "ConnectionString";
            return ConnectionString;
         }

         if (!string.IsNullOrEmpty(AppSettingsKey))
         {
            connectionStringContext = "AppSettingsKey";
            string appSettingsConnectionString = SystemInfo.GetAppSetting(AppSettingsKey);
            if (string.IsNullOrEmpty(appSettingsConnectionString))
            {
               throw new LogException("Unable to find [" + AppSettingsKey + "] AppSettings key.");
            }
            return appSettingsConnectionString;
         }

         connectionStringContext = "Unable to resolve connection string from ConnectionString, ConnectionStrings, or AppSettings.";
         return string.Empty;
      }

      protected virtual Type ResolveConnectionType()
      {
         try
         {
            return SystemInfo.GetTypeFromString(Assembly.GetEntryAssembly(), ConnectionType, true, false);
         }
         catch (Exception ex)
         {
            ErrorHandler.Error("Failed to load connection type [" + ConnectionType + "]", ex);
            throw;
         }
      }
      #endregion

      #region Helper Methods
      private void InitializeDatabaseCommand()
      {
         if (Connection != null && UsePreparedCommand)
         {
            try
            {
               DisposeCommand(false);
               _dbCommand = Connection.CreateCommand();
               _dbCommand.CommandText = CommandText;
               _dbCommand.CommandType = CommandType;
            }
            catch (Exception e)
            {
               ErrorHandler.Error("Could not create database command [" + CommandText + "]", e);
               DisposeCommand(true);
            }

            if (_dbCommand != null)
            {
               try
               {
                  foreach (AdoNetAppenderParameter param in Parameters)
                  {
                     try
                     {
                        param.Prepare(_dbCommand);
                     }
                     catch (Exception e)
                     {
                        ErrorHandler.Error("Could not add database command parameter [" + param.ParameterName + "]", e);
                        throw;
                     }
                  }
               }
               catch
               {
                  DisposeCommand(true);
               }
            }

            if (_dbCommand != null)
            {
               try
               {
                  _dbCommand.Prepare();
               }
               catch (Exception e)
               {
                  ErrorHandler.Error("Could not prepare database command [" + CommandText + "]", e);

                  DisposeCommand(true);
               }
            }
         }
      }

      private void InitializeDatabaseConnection()
      {
         string connectionStringContext = "Unable to determine connection string context.";
         string resolvedConnectionString = string.Empty;

         try
         {
            DisposeCommand(true);
            DisposeConnection();

            resolvedConnectionString = ResolveConnectionString(out connectionStringContext);
            Connection = CreateConnection(ResolveConnectionType(), resolvedConnectionString);

            using (SecurityContext.Impersonate(this))
            {
               Connection.Open();
            }
         }
         catch (Exception e)
         {
            ErrorHandler.Error("Could not open database connection [" + resolvedConnectionString + "]. Connection string context [" + connectionStringContext + "].", e);
            Connection = null;
         }
      }

      private void DisposeCommand(bool ignoreException)
      {
         if (_dbCommand != null)
         {
            try
            {
               _dbCommand.Dispose();
            }
            catch (Exception ex)
            {
               if (!ignoreException)
               {
                  LogLog.Warn(DeclaringType, "Exception while disposing cached command object", ex);
               }
            }
            finally
            {
               _dbCommand = null;
            }
         }
      }

      private void DisposeConnection()
      {
         if (Connection != null)
         {
            try
            {
               Connection.Close();
            }
            catch (Exception ex)
            {
               LogLog.Warn(DeclaringType, "Exception while disposing cached connection object", ex);
            }
            finally
            {
               Connection = null;
            }
         }
      }
      #endregion
   }
}
