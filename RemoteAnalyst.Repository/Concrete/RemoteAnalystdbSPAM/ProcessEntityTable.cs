using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM {
    public class ProcessEntityTable {
        private readonly string _connectionString;

        public ProcessEntityTable(string connectionString) {
            _connectionString = connectionString;
        }

        public DataTable GetAllProcessByBusy(List<string> processTableNames, DateTime startTime, DateTime stopTime) {
            var cmdText = new StringBuilder();
            cmdText.Append(" SELECT * FROM ( ");
            for (int i = 0; i < processTableNames.Count; i++) {
                cmdText.Append(@" SELECT 
                                FromTimestamp,
                                (MAX(CpuBusyTime / DeltaTime)) * 100  AS `Busy %`,
                                MAX(recvqtime/DeltaTime) AS ReceiveQueue,
                                SUM(AbortTrans) AbortTrans,
                                SUM(BeginTrans) BeginTrans
                                FROM " + processTableNames[i] + @"
                                FORCE INDEX ( `Timestamps` )
                                WHERE (FromTimestamp >= @FromTimestamp AND FromTimestamp <  @ToTimestamp AND 
                                       ToTimestamp   >  @FromTimestamp AND ToTimestamp   <= @ToTimestamp)   
                                GROUP BY FromTimestamp");

                if (i != processTableNames.Count - 1) {
                    cmdText.Append(" UNION ALL ");
                }
            }
            cmdText.Append(" ) AS A ORDER BY FromTimestamp ");

            var diskBusyTable = new DataTable();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText.ToString(), connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@FromTimestamp", startTime);
                command.Parameters.AddWithValue("@ToTimestamp", stopTime);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(diskBusyTable);
            }

            return diskBusyTable;
        }

        public int CheckIPUColumn(string entityTableName, string databaseName) {
            string cmdText = @"SELECT COUNT(*) AS ColumnCount FROM INFORMATION_SCHEMA.COLUMNS 
                                WHERE TABLE_NAME = @TableName AND TABLE_SCHEMA = @DatabaseName 
                                AND COLUMN_NAME = 'IPUNum'";
            int num = 0;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@TableName", entityTableName);
                command.Parameters.AddWithValue("@DatabaseName", databaseName);

                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read()) {
                    num = Convert.ToInt16(reader["ColumnCount"]);
                }
                reader.Close();
            }
            return num;
        }
        public DataTable GetTop20ProcessByBusyStatic(List<string> processTableNames, DateTime startTime, DateTime stopTime, int pageSizeBytes, long interval, bool isIPU) {
            var cmdText = new StringBuilder();
            cmdText.Append(" SELECT * FROM ( ");
            for (int i = 0; i < processTableNames.Count; i++) {
                cmdText.Append(@" ( SELECT 
                                ProcessName,
                                FromTimestamp,
                                (SUM(CpuBusyTime / DeltaTime)) * 100  AS `Busy %`,
                                CONCAT(Volume, '.', SubVol, '.', FileName) AS Program,
                                SUM(recvqtime/DeltaTime) AS ReceiveQueue, 
                                (SUM(PresPagesQtime/DeltaTime) * @PageSizeBytes) / (1024*1024) AS MemUsed,");
                if (isIPU)
                    cmdText.Append(@" CpuNum, IPUNum, PIN, Priority, AncestorProcessName, User, `Group`");
                else
                    cmdText.Append(@" CpuNum, PIN, Priority, AncestorProcessName, User, `Group`");

                cmdText.Append(@" FROM " + processTableNames[i] + @" 
                                FORCE INDEX (Timestamps)
                                WHERE (FromTimestamp >= @FromTimestamp AND FromTimestamp <  @ToTimestamp AND 
                                       ToTimestamp   >  @FromTimestamp AND ToTimestamp   <= @ToTimestamp)   
                                AND  DeltaTime > (( " + interval + @" * 1000000 ) * .90) AND Priority >= 90
                                AND (recvqtime / DeltaTime) > 1 ");
                if (isIPU)
                    cmdText.Append(@" GROUP BY ProcessName, FromTimestamp, Volume, SubVol, FileName, CpuNum, IPUNum, PIN, Priority, AncestorProcessName, User, `Group` ");
                else
                    cmdText.Append(@" GROUP BY ProcessName, FromTimestamp, Volume, SubVol, FileName, CpuNum, PIN, Priority, AncestorProcessName, User, `Group` ");

                cmdText.Append(" ORDER BY `Busy %` DESC LIMIT 20 ) ");
                if (i != processTableNames.Count - 1) {
                    cmdText.Append(" UNION ALL ");
                }
            }
            cmdText.Append(" ) AS A ORDER BY `Busy %` DESC LIMIT 20 ");

            var diskBusyTable = new DataTable();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText.ToString(), connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@FromTimestamp", startTime);
                command.Parameters.AddWithValue("@ToTimestamp", stopTime);
                command.Parameters.AddWithValue("@PageSizeBytes", pageSizeBytes);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(diskBusyTable);
            }

            return diskBusyTable;
        }

        public DataTable GetTop20ProcessByBusyDynamic(List<string> processTableNames, DateTime startTime, DateTime stopTime, int pageSizeBytes, long interval, bool isIPU) {
            var cmdText = new StringBuilder();
            cmdText.Append(" SELECT * FROM ( ");
            for (int i = 0; i < processTableNames.Count; i++) {
                cmdText.Append(@" ( SELECT 
                                ProcessName,
                                FromTimestamp,
                                ToTimestamp,
                                TIMEDIFF(ToTimestamp, FromTimestamp) AS TimeDif,
                                (SUM(CpuBusyTime / DeltaTime)) * 100  AS `Busy %`,
                                CONCAT(Volume, '.', SubVol, '.', FileName) AS Program,
                                SUM(recvqtime/DeltaTime) AS ReceiveQueue, 
                                (SUM(PresPagesQtime/DeltaTime) * @PageSizeBytes) / (1024*1024) AS MemUsed,");
                if (isIPU)
                    cmdText.Append(@" CpuNum, IPUNum, PIN, Priority, AncestorProcessName, User, `Group`");
                else
                    cmdText.Append(@" CpuNum, PIN, Priority, AncestorProcessName, User, `Group`");
                cmdText.Append(@" FROM " + processTableNames[i] + @"
                                FORCE INDEX ( `Timestamps` ) 
                                WHERE 
								(FromTimestamp >= @FromTimestamp AND FromTimestamp < @ToTimestamp AND
									ToTimestamp > @FromTimestamp AND ToTimestamp <= @ToTimestamp) 
								AND DeltaTime <= (( " + interval + @" * 1000000 ) * .90)");
                if (isIPU)
                    cmdText.Append(@" GROUP BY ProcessName, FromTimestamp, ToTimestamp, Volume, SubVol, FileName, CpuNum, IPUNum, PIN, Priority, AncestorProcessName, User, `Group` HAVING `Busy %` > 1 ");
                else
                    cmdText.Append(@" GROUP BY ProcessName, FromTimestamp, ToTimestamp, Volume, SubVol, FileName, CpuNum, PIN, Priority, AncestorProcessName, User, `Group` HAVING `Busy %` > 1");

                cmdText.Append(" ORDER BY `Busy %` DESC LIMIT 20 ) ");
                if (i != processTableNames.Count - 1) {
                    cmdText.Append(" UNION ALL ");
                }
            }
            cmdText.Append(" ) AS A WHERE TimeDif >= '00:00:05' ORDER BY `Busy %` DESC LIMIT 20 ");

            var diskBusyTable = new DataTable();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText.ToString(), connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@FromTimestamp", startTime);
                command.Parameters.AddWithValue("@ToTimestamp", stopTime);
                command.Parameters.AddWithValue("@PageSizeBytes", pageSizeBytes);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(diskBusyTable);
            }

            return diskBusyTable;
        }

        public DataTable GetTop20ProcessByQueueStatic(List<string> processTableNames, DateTime startTime, DateTime stopTime, int pageSizeBytes, long interval, bool isIPU) {
            var cmdText = new StringBuilder();
            cmdText.Append(" SELECT * FROM ( ");
            for (int i = 0; i < processTableNames.Count; i++) {
                cmdText.Append(@" ( SELECT 
                                ProcessName,
                                FromTimestamp,
                                (SUM(CpuBusyTime / DeltaTime)) * 100  AS `Busy %`,
                                CONCAT(Volume, '.', SubVol, '.', FileName) AS Program,
                                SUM(recvqtime/DeltaTime) AS ReceiveQueue, 
                                (SUM(PresPagesQtime/DeltaTime) * @PageSizeBytes) / (1024*1024) AS MemUsed, ");
                if(isIPU)
                    cmdText.Append(@" CpuNum, IPUNum,  PIN, Priority, AncestorProcessName, User, `Group` ");
                else
                    cmdText.Append(@" CpuNum,  PIN, Priority, AncestorProcessName, User, `Group` ");
                cmdText.Append(@" FROM " + processTableNames[i] + @" 
                                FORCE INDEX (Timestamps)
                                WHERE (FromTimestamp >= @FromTimestamp AND FromTimestamp <  @ToTimestamp AND 
                                       ToTimestamp   >  @FromTimestamp AND ToTimestamp   <= @ToTimestamp)
                                AND DeltaTime > (( " + interval + @" * 1000000 ) * .90) 
                                AND (recvqtime / DeltaTime) > 1 ");
                if (isIPU)
                    cmdText.Append(@" GROUP BY ProcessName, FromTimestamp, Volume, SubVol, FileName, CpuNum, IPUNum,  PIN, Priority, AncestorProcessName, User, `Group`");
                else
                    cmdText.Append(@" GROUP BY ProcessName, FromTimestamp, Volume, SubVol, FileName, CpuNum,  PIN, Priority, AncestorProcessName, User, `Group`");

                cmdText.Append(" ORDER BY ReceiveQueue DESC LIMIT 20 ) ");
                if (i != processTableNames.Count - 1) {
                    cmdText.Append(" UNION ALL ");
                }
            }
            cmdText.Append(" ) AS A ORDER BY ReceiveQueue DESC LIMIT 20 ");
            var diskBusyTable = new DataTable();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText.ToString(), connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@FromTimestamp", startTime);
                command.Parameters.AddWithValue("@ToTimestamp", stopTime);
                command.Parameters.AddWithValue("@PageSizeBytes", pageSizeBytes);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(diskBusyTable);
            }

            return diskBusyTable;
        }

        public DataTable GetTop20ProcessByQueueDynamic(List<string> processTableNames, DateTime startTime, DateTime stopTime, int pageSizeBytes, long interval, bool isIPU) {
            var cmdText = new StringBuilder();
            cmdText.Append(" SELECT * FROM ( ");
            for (int i = 0; i < processTableNames.Count; i++) {
                cmdText.Append(@" ( SELECT 
                                ProcessName,
                                FromTimestamp,
                                ToTimestamp,
                                TIMEDIFF(ToTimestamp, FromTimestamp) AS TimeDif,
                                (SUM(CpuBusyTime / DeltaTime)) * 100  AS `Busy %`,
                                CONCAT(Volume, '.', SubVol, '.', FileName) AS Program,
                                SUM(recvqtime/DeltaTime) AS ReceiveQueue, 
                                (SUM(PresPagesQtime/DeltaTime) * @PageSizeBytes) / (1024*1024) AS MemUsed, ");
                if (isIPU)
                    cmdText.Append(@" CpuNum, IPUNum,  PIN, Priority, AncestorProcessName, User, `Group`");
                else
                    cmdText.Append(@" CpuNum, PIN, Priority, AncestorProcessName, User, `Group`");

                cmdText.Append(@" FROM " + processTableNames[i] + @"
                                FORCE INDEX ( `Timestamps` ) 
                                WHERE (FromTimestamp >= @FromTimestamp AND FromTimestamp <  @ToTimestamp AND 
                                       ToTimestamp   >  @FromTimestamp AND ToTimestamp   <= @ToTimestamp)   
								AND DeltaTime <= (( " + interval + @" * 1000000 ) * .90)");
                if (isIPU)
                    cmdText.Append(@" GROUP BY ProcessName, FromTimestamp, ToTimestamp,Volume, SubVol, FileName, CpuNum, IPUNum,  PIN, Priority, AncestorProcessName, User, `Group` HAVING ReceiveQueue > 1 ");
                else
                    cmdText.Append(@" GROUP BY ProcessName, FromTimestamp, ToTimestamp,Volume, SubVol, FileName, CpuNum,  PIN, Priority, AncestorProcessName, User, `Group` HAVING ReceiveQueue > 1 ");

                cmdText.Append(" ORDER BY ReceiveQueue DESC LIMIT 20 ) ");
                if (i != processTableNames.Count - 1) {
                    cmdText.Append(" UNION ALL ");
                }
            }
            cmdText.Append(" ) AS A WHERE TimeDif >= '00:00:05' ORDER BY ReceiveQueue DESC LIMIT 20 ");

            var diskBusyTable = new DataTable();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText.ToString(), connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@FromTimestamp", startTime);
                command.Parameters.AddWithValue("@ToTimestamp", stopTime);
                command.Parameters.AddWithValue("@PageSizeBytes", pageSizeBytes);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(diskBusyTable);
            }

            return diskBusyTable;
        }

        public DataTable GetTop20ProcessByAbort(List<string> processTableNames, DateTime startTime, DateTime stopTime, int pageSizeBytes, bool isIPU) {
            var cmdText = new StringBuilder();
            cmdText.Append(" SELECT * FROM ( ");
            for (int i = 0; i < processTableNames.Count; i++) {
                cmdText.Append(@" ( SELECT 
                                ProcessName,
                                FromTimestamp,
                                ((SUM(AbortTrans / DeltaTime) * 1000000) / (SUM(BeginTrans / DeltaTime) * 1000000)) * 100 AS AbortTMF,
                                -- SUM(AbortTrans) * 100 / SUM(BeginTrans) AS AbortTMF,
                                SUM(AbortTrans / DeltaTime) * 1000000 AS `Abort / Sec`,
                                SUM(BeginTrans / DeltaTime) * 1000000 AS `Begin / Sec`,
                                (SUM(CpuBusyTime / DeltaTime)) * 100  AS `Busy %`,
                                CONCAT(Volume, '.', SubVol, '.', FileName) AS Program,
                                SUM(recvqtime/DeltaTime) AS ReceiveQueue, 
                                (SUM(PresPagesQtime/DeltaTime) * 16000) / (1024*1024) AS MemUsed,");
                if(isIPU)
                    cmdText.Append(@" CpuNum, IPUNum,  PIN, Priority, AncestorProcessName, User, `Group`");
                else
                    cmdText.Append(@" CpuNum,  PIN, Priority, AncestorProcessName, User, `Group`");

                cmdText.Append(@" FROM " + processTableNames[i] + @"
                                FORCE INDEX ( `Timestamps` ) 
                                WHERE AbortTrans > 0 AND 
									(FromTimestamp >= @FromTimestamp AND FromTimestamp <  @ToTimestamp AND 
                                       ToTimestamp   >  @FromTimestamp AND ToTimestamp   <= @ToTimestamp)   ");
                if (isIPU)
                    cmdText.Append(@" GROUP BY ProcessName, FromTimestamp, Volume, SubVol, FileName, CpuNum, IPUNum,  PIN, Priority, AncestorProcessName, User, `Group` ");
                else
                    cmdText.Append(@" GROUP BY ProcessName, FromTimestamp, Volume, SubVol, FileName, CpuNum,  PIN, Priority, AncestorProcessName, User, `Group` ");

                cmdText.Append(@" HAVING (`Abort / Sec` > 0.005 OR `Begin / Sec` > 0.005) AND `Abort / Sec` <= `Begin / Sec` ");

                cmdText.Append(" ORDER BY AbortTMF DESC LIMIT 20 ) ");
                if (i != processTableNames.Count - 1) {
                    cmdText.Append(" UNION ALL ");
                }
            }
            cmdText.Append(" ) AS A ORDER BY AbortTMF DESC LIMIT 20 ");

            var diskBusyTable = new DataTable();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText.ToString(), connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@FromTimestamp", startTime);
                command.Parameters.AddWithValue("@ToTimestamp", stopTime);
                command.Parameters.AddWithValue("@PageSizeBytes", pageSizeBytes);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(diskBusyTable);
            }

            return diskBusyTable;
        }
    }
}
