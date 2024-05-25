using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM {
	public class TrendCPUNormalized {
		public void CreateTrendCPUNormalizedTable(string connectionString, string tableName) {
			string cmdText = @" CREATE TABLE `TrendCPUNormalized` ( " +
								"  `CpuNum` int(11) NULL, " +
								"  `DeltaTime` double NULL, " +
								"  `CpuBusyTime` double NULL, " +
								"  `FromTimestamp` datetime NULL, " +
								"  `ToTimestamp` datetime NULL, " +
								"  `Ipus` int(11) NULL " +
								" );";
			using (var connection = new MySqlConnection(connectionString)) {
				var command = new MySqlCommand(cmdText, connection);
				var adapter = new MySqlDataAdapter(command);
				connection.Open();
				command.ExecuteNonQuery();
				connection.Close();
			}

		}

		public bool CheckDuplicateDataFromNorTable(string cpuTableName, DateTime startTime, DateTime stopTime, string connectionString) {
			string cmdText = @"SELECT CpuNum FROM `" + cpuTableName + @"` 
                                WHERE FromTimestamp >= @FromTimestamp AND FromTimestamp < @ToTimestamp
                                AND ToTimestamp > @FromTimestamp AND ToTimestamp <= @ToTimestamp LIMIT 1";
			bool exists = false;
			using (var connection = new MySqlConnection(connectionString)) {
				var command = new MySqlCommand(cmdText, connection);
				command.Parameters.AddWithValue("@FromTimestamp", startTime);
				command.Parameters.AddWithValue("@ToTimestamp", stopTime);
				command.CommandTimeout = 0;

				connection.Open();
				var reader = command.ExecuteReader();
				if (reader.Read()) {
					exists = true;
				}
				reader.Close();
			}
			return exists;
		}

		public DataTable GetCPUBaseData(string cpuTableName, DateTime startTime, DateTime stopTime, DateTime columnStartTime, DateTime columnStopTime, string connectionString) {
			var baseData = new DataTable();
			try {
				string cmdText = @"SELECT CpuNum, SUM(DeltaTime) AS DeltaTime, SUM(CpuBusyTime) AS CpuBusyTime,
                                  STR_TO_DATE('" + columnStartTime.ToString("yyyy-MM-dd HH:mm:ss") + "', '%Y-%m-%d %H:%i:%s') AS `FromTimestamp`, " +
								 "STR_TO_DATE('" + columnStopTime.ToString("yyyy-MM-dd HH:mm:ss") + "', '%Y-%m-%d %H:%i:%s') AS `ToTimestamp`,  " +
								 "Ipus FROM " + cpuTableName +
								   @" WHERE FromTimestamp >= @FromTimestamp AND FromTimestamp < @ToTimestamp
                                   AND ToTimestamp > @FromTimestamp AND ToTimestamp <= @ToTimestamp
                                   GROUP BY CpuNum, Ipus";
				using (var connection = new MySqlConnection(connectionString)) {
					var command = new MySqlCommand(cmdText, connection);
					command.Parameters.AddWithValue("@FromTimestamp", startTime);
					command.Parameters.AddWithValue("@ToTimestamp", stopTime);
					command.CommandTimeout = 0;
					var adapter = new MySqlDataAdapter(command);
					adapter.Fill(baseData);
				}
			}
			catch (Exception) {
			}
			return baseData;
		}


	}
}
