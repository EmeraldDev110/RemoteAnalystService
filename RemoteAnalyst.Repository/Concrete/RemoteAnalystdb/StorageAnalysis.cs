using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySqlConnector;
using RemoteAnalyst.Repository.Infrastructure;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb
{
    public class StorageAnalysis
    {
        private readonly string _connectionString;

        public StorageAnalysis(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable GetTrendSize()
        {
            string cmdText =
                "SELECT " +
                    "SUBSTRING(s.schema_name, 16) SystemSerial, " +
                    "IFNULL(ROUND((SUM(data_length)+SUM(index_length)) / 1000 / 1000), 0) TotalSize " +
                "FROM " +
                    "INFORMATION_SCHEMA.SCHEMATA s,  " +
                    "INFORMATION_SCHEMA.TABLES t " +
                "WHERE " +
                    "s.schema_name = t.table_schema " +
                    "AND " +
                    "s.schema_name LIKE \"RemoteAnalyst%\" " +
                    "AND " +
                    "t.table_name LIKE \"Trend%\" " +
                "GROUP BY " +
                    "s.schema_name " +
                "ORDER BY " +
                    "s.schema_name  " +
                "DESC;";

            var table = new DataTable();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    connection.Open();

                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(table);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return table;
        }

        public DataTable GetDBSize()
        {
            string cmdText =
                "SELECT  " +
                    "s.schema_name, " +
                    "SUBSTRING(s.schema_name, 16) SystemSerial, " +
                    "IFNULL(ROUND((SUM(data_length)+SUM(index_length)) / 1000 / 1000), 0) TotalSize " +
                "FROM " +
                    "INFORMATION_SCHEMA.SCHEMATA s, " +
                    "INFORMATION_SCHEMA.TABLES t " +
                "WHERE " +
                    "s.schema_name = t.table_schema " +
                    "AND " +
                    "s.schema_name LIKE \"RemoteAnalyst%\" " +
                "GROUP BY " +
                    "s.schema_name " +
                "ORDER BY " +
                    "s.schema_name  " +
                "DESC;";

            var table = new DataTable();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    connection.Open();

                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(table);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return table;
        }

        public void Insert(string systemSerial, int activeSize, int trendSize, float s3Size)
        {
            string cmdText =
                "INSERT INTO StorageAnalyses(" +
                    "SystemSerial, " +
                    "ActiveSizeInMB, " +
                    "TrendSizeInMB, " +
                    "S3SizeInMB, " +
                    "Glacier, " +
                    "GeneratedDate, " +
                    "GeneratedTime) " + 
                "VALUES (" +
                    "\"" + systemSerial + "\", " +
                    activeSize + ", " +
                    trendSize + ", " +
                    s3Size + ", " +
                    "0, " +
                    "CAST(CONVERT_TZ(NOW(), \"GMT\", \"America/Los_Angeles\") AS DATE), " +
                    "CAST(CONVERT_TZ(NOW(), \"GMT\", \"America/Los_Angeles\") AS TIME));";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        public DataTable GetAllRdsRealName()
        {
            string cmdText =
                @"SELECT RdsRealName FROM MonitorRDS";

            var table = new DataTable();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    connection.Open();

                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(table);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return table;
        }

		public DataTable GetTop10StorageUsageBy(string date) {
			string cmdText =
				$@"SELECT x.SystemSerial, SystemName, CompanyName, ActiveSizeInMB, TrendSizeInMB, S3SizeInMB, GeneratedDate
				FROM StorageAnalyses x
				LEFT JOIN System_Tbl y
					ON y.SystemSerial = x.SystemSerial
				LEFT JOIN Company_Tbl z
					ON z.CompanyID = y.CompanyID
				WHERE GeneratedDate = '{date}'
				ORDER BY(ActiveSizeInMB +TrendSizeInMB + S3SizeInMB) DESC
				LIMIT 10;";

			var table = new DataTable();
			try {
				using (var connection = new MySqlConnection(_connectionString)) {
					var command = new MySqlCommand(cmdText, connection);
					command.CommandTimeout = 0;
					connection.Open();

					var adapter = new MySqlDataAdapter(command);
					adapter.Fill(table);
				}
			}
			catch (Exception ex) {
				throw new Exception(ex.Message);
			}

			return table;
		}

		// added an extra '\' at SystemName to make sure the back slash is excaped in MySQL
		public DataTable GetStoragesBy(string systemName, string fromDate, string toDate) {
			string cmdText =
				$@"SELECT 
					SystemName, 
					(ActiveSizeInMB + TrendSizeInMB + S3SizeInMB) StorageUsageInMB, 
					GeneratedDate
				FROM StorageAnalyses x
				LEFT JOIN System_Tbl y
					ON y.SystemSerial = x.SystemSerial
				WHERE 
					GeneratedDate >= '{fromDate}' 
					AND GeneratedDate <= '{toDate}'
					AND SystemName = '\{systemName}'
				ORDER BY GeneratedDate ASC;";

			var table = new DataTable();
			try {
				using (var connection = new MySqlConnection(_connectionString)) {
					var command = new MySqlCommand(cmdText, connection);
					command.CommandTimeout = 0;
					connection.Open();

					var adapter = new MySqlDataAdapter(command);
					adapter.Fill(table);
				}
			}
			catch (Exception ex) {
				throw new Exception(ex.Message);
			}

			return table;
		}

		public DataTable GetSystemNamesInTopForPeriod(int top, string fromDate, string toDate) {
			string cmdText =
				$@"SELECT SystemName 
					FROM ( 
						SELECT 
							SystemSerial, 
							ActiveSizeInMB, 
							GeneratedDate, 
							@rn := IF(@prev = GeneratedDate, @rn + 1, 1) AS rn, 
							@prev := GeneratedDate 
						FROM StorageAnalyses 
						JOIN (SELECT @prev := NULL, @rn := 0) AS vars 
						ORDER BY GeneratedDate, ActiveSizeInMB DESC 
					) as t 
					LEFT JOIN System_Tbl y ON y.SystemSerial = t.SystemSerial 
					WHERE rn <= 10 
					AND GeneratedDate >= '{fromDate}' AND GeneratedDate <= '{toDate}' 
					GROUP BY SystemName;";

			var table = new DataTable();
			try {
				using (var connection = new MySqlConnection("allowuservariables=True;" + _connectionString)) {
					var command = new MySqlCommand(cmdText, connection);
					command.CommandTimeout = 0;
					connection.Open();

					var adapter = new MySqlDataAdapter(command);
					adapter.Fill(table);
				}
			}
			catch (Exception ex) {
				throw new Exception(ex.Message);
			}

			return table;
		}
	}
}
