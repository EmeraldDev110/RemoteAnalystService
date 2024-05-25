using System;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM
{
    public class MonthlyDisk
    {
        private readonly string _connectionString;

        public MonthlyDisk(string connectionStringTrend)
        {
            _connectionString = connectionStringTrend;
        }

        public bool CheckData(string systemSerial, DateTime date, string diskName)
        {
            string cmdText =
                "SELECT MD_SystemSerialNum FROM MonthlyDisk WHERE MD_SystemSerialNum = @SystemSerial AND MD_Date = @Date AND MD_DiskName = @DiskName";
            bool exits = false;
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@Date", date);
                command.Parameters.AddWithValue("@DiskName", diskName);

                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    exits = true;
                }
            }

            return exits;
        }

        public void InsertNewData(string systemSerial, DateTime date, string diskName)
        {
            string cmdText = @"INSERT INTO MonthlyDisk(MD_SystemSerialNum, MD_Date, MD_DiskName) VALUES 
                            (@SystemSerial, @Date, @DiskName)";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@Date", date);
                command.Parameters.AddWithValue("@DiskName", diskName);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateData(string systemSerial, string diskName, DateTime tempDate, double firstDayGB,
            double lastDayGB, double avgUsedGB, double deltaMB, double deltaPercent)
        {
            string cmdText = @"UPDATE MonthlyDisk SET MD_FirstDayUsedGB = @FirstDayUsedGB, 
                              MD_LastDayUsedGB = @LastDayUsedGB, MD_AvgUsedGB = @AvgUsedGB,
                              MD_DeltaMB = @DeltaMB, MD_DeltaPercent = @DeltaPercent WHERE
                              MD_SystemSerialNum = @SystemSerial AND MD_Date = @Date AND MD_DiskName = @DiskName";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@Date", tempDate);
                command.Parameters.AddWithValue("@DiskName", diskName);

                command.Parameters.AddWithValue("@FirstDayUsedGB", firstDayGB);
                command.Parameters.AddWithValue("@LastDayUsedGB", lastDayGB);
                command.Parameters.AddWithValue("@AvgUsedGB", avgUsedGB);
                command.Parameters.AddWithValue("@DeltaMB", deltaMB);
                command.Parameters.AddWithValue("@DeltaPercent", deltaPercent);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteData(DateTime oldDate) {
            string cmdText = "DELETE FROM MonthlyDisk WHERE MD_Date < @OldDate";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@OldDate", oldDate);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}