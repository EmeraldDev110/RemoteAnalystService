using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM {
    public class Exceptions {
        private readonly string _connectionString;

        public Exceptions(string connectionString) {
            _connectionString = connectionString;
        }

        public DataTable GetException(DateTime startTime, DateTime stopTime, string entity, string counter) {
            var exception = new DataTable();
            const string cmdText = @"SELECT FromTimestamp, Instance, DisplayRed FROM Exceptions
                                    WHERE(DisplayRed = 0 || DisplayRed = 1) AND
                                    (FromTimestamp >= @StartTime AND FromTimestamp <= @StopTime)
                                    AND Entity = @Entity AND Counter = @Counter";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@StopTime", stopTime);
                command.Parameters.AddWithValue("@Entity", entity);
                command.Parameters.AddWithValue("@Counter", counter);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(exception);
            }
            return exception;

        }
    }
}
