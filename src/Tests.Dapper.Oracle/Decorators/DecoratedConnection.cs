using System.Data;
using System.Data.Common;

namespace Tests.Dapper.Oracle
{
    internal class DecoratedConnection : DbConnection
    {
        private DbConnection Decorated;

        public DecoratedConnection(DbConnection decorated)
        {
            Decorated = decorated;
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return new DecoratedTransaction(Decorated.BeginTransaction(isolationLevel), this);
        }

        public override void ChangeDatabase(string databaseName)
        {
            Decorated.ChangeDatabase(databaseName);
        }

        public override void Close()
        {
            Decorated.Close();
        }

        public override void Open()
        {
            Decorated.Open();
        }

        public override string ConnectionString
        {
            get => Decorated.ConnectionString;
            set => Decorated.ConnectionString = value;
        }

        public override string Database => Decorated.Database;
        public override ConnectionState State => Decorated.State;
        public override string DataSource => Decorated.DataSource;
        public override string ServerVersion => Decorated.ServerVersion;


        protected override DbCommand CreateDbCommand()
        {
            return new DecoratedDbCommand(Decorated.CreateCommand());
        }
    }
}