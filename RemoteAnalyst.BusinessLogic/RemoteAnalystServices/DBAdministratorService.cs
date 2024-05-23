using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices
{
    public class DBAdministratorService
    {

        private readonly string _connectionString;

        public DBAdministratorService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool IsActive(string systemSerial, string ipAddress)
        {
            var dbAdmin = new DBAdministrator(_connectionString);
            DataTable clientConnection = dbAdmin.GetClientConnection(systemSerial, ipAddress);
            return (clientConnection.Rows.Count > 0);
        }
    }
}
