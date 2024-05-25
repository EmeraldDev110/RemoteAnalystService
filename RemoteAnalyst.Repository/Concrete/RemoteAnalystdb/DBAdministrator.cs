using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb
{
    public class DBAdministrator
    {
        private readonly string _connectionString;

        public DBAdministrator(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable GetClientConnection(string systemSerial, string ipAddress)
        {
            var cmdText = @"SELECT t.PROCESSLIST_HOST,
                                    t.PROCESSLIST_DB
                            FROM performance_schema.threads t  
                                LEFT OUTER JOIN performance_schema.session_connect_attrs a 
                                ON t.processlist_id = a.processlist_id 
                                AND (a.attr_name IS NULL OR a.attr_name = 'program_name') 
                            WHERE t.TYPE <> 'BACKGROUND'
                            AND t.PROCESSLIST_HOST = @ipAddress
                            AND t.PROCESSLIST_DB LIKE @systemSerial";

            DataTable clientConnection = new DataTable("ClientConnection");
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@ipAddress", ipAddress);
                    command.Parameters.AddWithValue("@systemSerial", systemSerial);
                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(clientConnection);
                }
            }
            catch
            {

            }
            return clientConnection;
        }

    }
}
