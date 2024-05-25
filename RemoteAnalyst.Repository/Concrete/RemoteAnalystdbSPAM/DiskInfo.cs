using System;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM
{
    public class DiskInfo
    {
        private readonly string _connectionString;

        public DiskInfo(string connectionStringTrend)
        {
            _connectionString = connectionStringTrend;
        }

        public double GetAveragedUsedGB(string systemSerial, string diskName)
        {
            string cmdText = @"SELECT DI_CapacityGB FROM DiskInfo WHERE 
                                DI_SystemSerialNum = @SystemSerial AND DI_DiskName = @DiskName ORDER BY DI_DiskID DESC LIMIT 1";

            double capacityGB = 0;
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@DiskName", diskName);
                
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                        capacityGB = Convert.ToDouble(reader["DI_CapacityGB"]);
                }
            }

            return capacityGB;
        }

    }
}