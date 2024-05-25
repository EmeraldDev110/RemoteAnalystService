using System;
using System.Data;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM
{
    public class PvCollects
    {
        private readonly string _connectionString;

        public PvCollects(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable GetInterval(DateTime fromTimestamp, DateTime toTimestamp)
        {
            string cmdText = "SELECT IntervalNn, IntervalHOrM FROM PvCollects " +
                " WHERE FromTimestamp >= @FromTimestamp AND FromTimestamp < @ToTimestamp AND " +
                " ToTimestamp > @FromTimestamp AND ToTimestamp <= @ToTimestamp LIMIT 1";
            DataTable table =  new DataTable();
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@FromTimestamp", fromTimestamp);
                command.Parameters.AddWithValue("@ToTimestamp", toTimestamp);
                connection.Open();
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(table);
            }
            return table;
        }
    }
}