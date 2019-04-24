using System;
using System.Data;
using log4net.Core;
using log4net.Layout;

namespace CanTheSpam.Log4Net.Parameters
{

   public class AdoNetAppenderParameter
   {
      #region Fields
      private DbType _dbType;
      private bool _inferType = true;
      #endregion

      #region Public Properties
      public string ParameterName { get; set; }

      public DbType DbType
      {
         get => _dbType;
         set
         {
            _dbType = value;
            _inferType = false;
         }
      }

      public byte Precision { get; set; }

      public byte Scale { get; set; }

      public int Size { get; set; }

      public IRawLayout Layout { get; set; }
      #endregion

      public AdoNetAppenderParameter() { }

      #region Public Methods
      public virtual void Prepare(IDbCommand command)
      {
         IDbDataParameter param = command.CreateParameter();

         param.ParameterName = ParameterName;

         if (!_inferType)
            param.DbType = _dbType;

         if (Precision != 0)
            param.Precision = Precision;

         if (Scale != 0)
            param.Scale = Scale;

         if (Size != 0)
            param.Size = Size;

         command.Parameters.Add(param);
      }

      public virtual void FormatValue(IDbCommand command, LoggingEvent loggingEvent)
      {
         IDbDataParameter param = (IDbDataParameter)command.Parameters[ParameterName];

         object formattedValue = Layout.Format(loggingEvent) ?? DBNull.Value;

         param.Value = formattedValue;
      }
      #endregion
   }
}
