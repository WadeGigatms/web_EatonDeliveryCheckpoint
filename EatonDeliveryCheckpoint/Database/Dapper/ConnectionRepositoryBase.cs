using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Database.Dapper
{
    public class ConnectionRepositoryBase
    {
        protected IDbConnection _connection { private set; get; }
        protected IDbTransaction _transaction { private set; get; }
        public DatabaseConnectionName DatabaseConnectionName { private set; get; }
        public String ConnectionString { private set; get; }

        public ConnectionRepositoryBase(DatabaseConnectionName connectionName, string connectionString)
        {
            DatabaseConnectionName = connectionName;
            ConnectionString = connectionString;
        }

        public IDbConnection InitConnection()
        {
            _connection = new SqlConnection(ConnectionString);
            return _connection;
        }

        public IDbTransaction BeginTransaction()
        {
            _transaction = _connection.BeginTransaction();
            return _transaction;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
