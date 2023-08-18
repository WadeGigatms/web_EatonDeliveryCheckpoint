using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Database.Dapper
{
    public class ConnectionRepositoryBase
    {
        protected IDbConnection _connection { private set; get; }
        public DatabaseConnectionName databaseConnectionName { private set; get; }
        public IDbConnection GetConnection() => _connection;

        public ConnectionRepositoryBase(DatabaseConnectionName connectionName, string connectionString)
        {
            databaseConnectionName = connectionName;
            switch (connectionName)
            {
                case DatabaseConnectionName.MsSql:
                    _connection = new SqlConnection(connectionString);
                    break;
            }
        }

        public void Dispose()
        {
            _connection?.Close();
        }
    }
}
