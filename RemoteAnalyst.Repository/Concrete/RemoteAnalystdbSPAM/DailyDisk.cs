using System;
using System.Data;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM
{
    public class DailyDisk
    {
        private readonly string _connectionString;

        public DailyDisk(string connectionStringTrend)
        {
            _connectionString = connectionStringTrend;
        }

        public DataTable GetDiskNames()
        {
            string cmdText = @"SELECT DISTINCT DD_SystemSerialNum, DD_DiskName FROM DailyDisk
                                WHERE DD_SystemSerialNum != '000000' 
                                ORDER BY  DD_SystemSerialNum";

            var disks = new DataTable();
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(disks);
            }

            return disks;
        }

        public bool CheckData(DateTime startTime, DateTime stopTime) {
            string cmdText = @"SELECT DD_DiskName FROM DailyDisk
                                WHERE DD_Date >= @StartTime AND DD_Date < @StopTime
                                ORDER BY DD_Date DESC
                                LIMIT 1;";

            bool exits = false;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@StopTime", stopTime);

                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read()) {
                    exits = true;
                }
                reader.Close();
                connection.Close();
            }

            return exits;
        }
        public bool CheckData(string systemSerial, string diskName, int month, int year)
        {
            string cmdText = @"SELECT DD_Date FROM DailyDisk WHERE DD_SystemSerialNum = @SystemSerial
                            AND DD_DiskName = @DiskName AND MONTH(DD_Date) = @Month AND YEAR(DD_Date) = @Year LIMIT 1";

            bool exits = false;
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@DiskName", diskName);
                command.Parameters.AddWithValue("@Month", month);
                command.Parameters.AddWithValue("@Year", year);

                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    exits = true;
                }
                reader.Close();
                connection.Close();
            }

            return exits;
        }

        public double GetUsedGB(string systemSerial, string diskName, int month, int year, string order)
        {
            string cmdText = @"SELECT DD_UsedGB FROM DailyDisk WHERE MONTH(DD_Date)= @Month AND 
                              YEAR(DD_Date)= @Year AND DD_SystemSerialNum = @SystemSerial AND 
                              DD_DiskName = @DiskName ORDER BY DD_Date " + order + " LIMIT 1";

            double usedGB = 0;
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@DiskName", diskName);
                command.Parameters.AddWithValue("@Month", month);
                command.Parameters.AddWithValue("@Year", year);

                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                        usedGB = Convert.ToDouble(reader["DD_UsedGB"]);
                }
                reader.Close();
                connection.Close();
            }

            return usedGB;
        }

        public double GetAveragedUsedGB(string systemSerial, string diskName, int month, int year)
        {
            string cmdText = @"SELECT AVG(DD_UsedGB) AS AVGUSEDGB FROM DailyDisk WHERE MONTH(DD_Date)= @Month AND 
                                YEAR(DD_Date)= @Year AND DD_SystemSerialNum = @SystemSerial AND 
                                DD_DiskName = @DiskName";

            double avgUsedGB = 0;
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@DiskName", diskName);
                command.Parameters.AddWithValue("@Month", month);
                command.Parameters.AddWithValue("@Year", year);

                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                        avgUsedGB = Convert.ToDouble(reader["AVGUSEDGB"]);
                }
                reader.Close();
                connection.Close();
            }

            return avgUsedGB;
        }

        public void DeleteData(DateTime oldDate) {
            string cmdText = "DELETE FROM DailyDisk WHERE DD_Date < @OldDate";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@OldDate", oldDate);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public DataTable GetDiskName(DateTime displayDate) {
            string cmdText = @"SELECT DISTINCT DD_DiskName FROM DailyDisk
                                WHERE ORDER BY  DD_SystemSerialNum";

            var disks = new DataTable();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(disks);
            }

            return disks;
        }

        public DataTable GetDailyDiskInfo(string startDate) {
            string cmdText = @"SELECT DI_DiskName AS DiskName, DI_CapacityGB AS Capacity, DD_UsedGB Used, 
                            (DD_UsedGB / DI_CapacityGB) * 100 AS UsedPercent FROM DiskInfo AS D 
                            INNER JOIN DailyDisk AS DD ON D.DI_DiskName = DD.DD_DiskName
                            WHERE DATE(DD_Date) = @StartDate
                            AND (DD_UsedGB / DI_CapacityGB) * 100 >= 80
                            AND DI_DateReplaced IS NULL
                            ORDER BY UsedPercent DESC";

            var disks = new DataTable();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@StartDate", startDate);

                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(disks);
            }

            return disks;
        }

        public double GetUsedGB(string startDate, string diskName) {
            string cmdText = @"SELECT DD_UsedGB FROM DailyDisk WHERE DATE(DD_Date) = @StartDate AND 
                              DD_DiskName = @DiskName";

            double usedGB = 0;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@DiskName", diskName);
                command.Parameters.AddWithValue("@StartDate", startDate);

                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read()) {
                    if (!reader.IsDBNull(0))
                        usedGB = Convert.ToDouble(reader["DD_UsedGB"]);
                }
                reader.Close();
                connection.Close();
            }

            return usedGB;
        }
        public DataTable GetUsedGB(string systemSerial, string yesterday, string lastWeek, string lastMonth, string diskNames) {
            string cmdText = @"SELECT DD_DiskName AS DiskName, SUM(DD_UsedGB_Yesterday) AS Yesterday, SUM(DD_UsedGB_LastWeek) AS LastWeek, SUM(DD_UsedGB_Month) AS LastMonth
                                FROM (
	                                SELECT DD_DiskName, 
	                                  CASE DATE(DD_Date)
		                                when @Yesterday then DD_UsedGB else 0
		                                END as DD_UsedGB_Yesterday,
	                                  CASE DATE(DD_Date)
		                                when @LastWeek then DD_UsedGB else 0
		                                END as DD_UsedGB_LastWeek,
	                                  CASE DATE(DD_Date)
		                                when @LastMonth then DD_UsedGB else 0
		                                END as DD_UsedGB_Month		
	                                FROM DailyDisk WHERE 
	                                DD_SystemSerialNum = @SystemSerial
	                                AND
	                                (
	                                DATE(DD_Date) = @Yesterday OR DATE(DD_Date) = @LastWeek OR DATE(DD_Date) = @LastMonth
	                                ) AND 
	                                DD_DiskName IN (" + diskNames + @") 
                                ) as Combined_Disk
                                group by DD_DiskName";

            var disks = new DataTable();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@Yesterday", yesterday);
                command.Parameters.AddWithValue("@LastWeek", lastWeek);
                command.Parameters.AddWithValue("@LastMonth", lastMonth);

                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(disks);
            }

            return disks;
        }

        public DataTable GetFreeUsedGB(string startDate, string stopDate) {
            /*string cmdText = @"SELECT SUM(DI_CapacityGB) AS Free, SUM(DD_UsedGB) AS Used
                                FROM DiskInfo AS D 
                                INNER JOIN DailyDisk AS DD ON D.DI_DiskName = DD.DD_DiskName
                                WHERE DATE(DD_Date) = @StartDate";*/
            string cmdText = @"SELECT DATE(DD_Date) AS StorageDate, SUM(DI_CapacityGB) AS Free, SUM(DD_UsedGB) AS Used
                            FROM DiskInfo AS D 
                            INNER JOIN DailyDisk AS DD ON D.DI_DiskName = DD.DD_DiskName
                            WHERE DATE(DD_Date) <= @StartDate AND DATE(DD_Date) >= @StopDate
                            AND DI_DateReplaced IS NULL
                            GROUP BY StorageDate
                            ORDER BY StorageDate";
            var disks = new DataTable();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@StopDate", stopDate);

                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(disks);
            }

            return disks;
        }

        public DataTable GetDailyDiskInfo(DateTime startDate, DateTime stopDate) {
            string cmdText = @"SELECT DATE(DD_Date) AS FromTimestamp, DI_DiskName AS DeviceName, 
                            IFNULL((DD_UsedGB / DI_CapacityGB) * 100, 0) AS UsedPercent FROM DiskInfo AS D 
                            INNER JOIN DailyDisk AS DD ON D.DI_DiskName = DD.DD_DiskName
                            WHERE DATE(DD_Date) >= @StartDate AND DATE(DD_Date) <= @StopDate
                            AND DI_DateReplaced IS NULL
                            ORDER BY FromTimestamp, DeviceName";

            var disks = new DataTable();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText + "; set net_write_timeout=99999; set net_read_timeout=99999", connection);
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@StopDate", stopDate);

                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(disks);
            }

            return disks;
        }
    }
}