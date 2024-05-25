using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class SystemWeek {

        private readonly string _connectionString;

        public SystemWeek(string connectionString) {
            _connectionString = connectionString;
        }

        public DataTable GetSystemWeek(string systemSerial) {
            var systemWeek = new DataTable();
            //UserPreferencesService service = new UserPreferencesService();
            string cmdText = @"SELECT Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday,
                                H00, H01, H02, H03, H04, H05, H06, H07, H08, H09, H10, H11, H12, 
                                H13, H14, H15, H16, H17, H18, H19, H20, H21, H22, H23
                                FROM SystemWeek WHERE SystemSerial =  @SystemSerial";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(systemWeek);
            }

            return systemWeek;
        }
    }
}
