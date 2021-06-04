using System.Data;
using System.Data.Common;

namespace Tests.Dapper.Oracle
{
    internal class DecoratedDbCommand : DbCommand
    {
        public DbCommand Decorated { get; }

        public DecoratedDbCommand(DbCommand decorated)
        {
            Decorated = decorated;
        }
        
        public override void Cancel()
        {
            Decorated.Cancel();
        }

        public override int ExecuteNonQuery()
        {
            return Decorated.ExecuteNonQuery();
        }

        public override object ExecuteScalar()
        {
            return Decorated.ExecuteNonQuery();
        }

        public override void Prepare()
        {
            Decorated.Prepare();
        }

        public override string CommandText
        {
            get => Decorated.CommandText;
            set => Decorated.CommandText = value;
        }

        public override int CommandTimeout
        {
            get => Decorated.CommandTimeout;
            set => Decorated.CommandTimeout = value;
        }

        public override CommandType CommandType
        {
            get => Decorated.CommandType;
            set => Decorated.CommandType = value;
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get => Decorated.UpdatedRowSource;
            set => Decorated.UpdatedRowSource = value;
        }

        protected override DbConnection DbConnection
        {
            get => Decorated.Connection;
            set => Decorated.Connection = value;
        }

        protected override DbParameterCollection DbParameterCollection => Decorated.Parameters;

        protected override DbTransaction DbTransaction
        {
            get => Decorated.Transaction;
            set => Decorated.Transaction = value;
        }

        public override bool DesignTimeVisible
        {
            get => Decorated.DesignTimeVisible;
            set => Decorated.DesignTimeVisible = value;
        }

        protected override DbParameter CreateDbParameter()
        {
            return Decorated.CreateParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return Decorated.ExecuteReader(behavior);
        }
    }
}