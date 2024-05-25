using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
	public class NullCheck {
		public bool NullCheckForPathwayPramaterPvCollects(DateTime fromTimestamp, DateTime toTimestamp, string connectionStringSystem) {
			string cmdText = @"SELECT COUNT(*) AS TableCount FROM PvCollects 
								WHERE (FromTimestamp >= @FromTimestamp AND FromTimestamp < @ToTimestamp AND
										ToTimestamp > @FromTimestamp AND ToTimestamp <= @ToTimestamp)
								LIMIT 1;";
			int rowCount = 0;
			try {
				using (var connection = new MySqlConnection(connectionStringSystem)) {
					var command = new MySqlCommand(cmdText, connection);
					command.Parameters.AddWithValue("@FromTimeStamp", fromTimestamp.ToString("yyyy-MM-dd HH:mm:ss"));
					command.Parameters.AddWithValue("@ToTimeStamp", toTimestamp.ToString("yyyy-MM-dd HH:mm:ss"));
					connection.Open();
					var reader = command.ExecuteReader();
					if (reader.Read()) {
						rowCount = Convert.ToInt32(reader["TableCount"].ToString());
					}
				}
				if (rowCount > 0) {
					return true;
				}
				else {
					return false;
				}

			}
			catch(Exception ex) {
				return false;
			}
		}
		public bool NullCheckForPathwayPramaterPvPwyList(DateTime fromTimestamp, DateTime toTimestamp, string connectionStringSystem) {
			string cmdText = @"SELECT COUNT(*) AS TableCount FROM PvPwylist 
								WHERE (FromTimestamp >= @FromTimestamp AND FromTimestamp < @ToTimestamp AND
										ToTimestamp > @FromTimestamp AND ToTimestamp <= @ToTimestamp);";
			int rowCount = 0;
			try {
				using (var connection = new MySqlConnection(connectionStringSystem)) {
					var command = new MySqlCommand(cmdText, connection);
					command.Parameters.AddWithValue("@FromTimeStamp", fromTimestamp.ToString("yyyy-MM-dd HH:mm:ss"));
					command.Parameters.AddWithValue("@ToTimeStamp", toTimestamp.ToString("yyyy-MM-dd HH:mm:ss"));
					connection.Open();
					var reader = command.ExecuteReader();
					if (reader.Read()) {
						rowCount = Convert.ToInt32(reader["TableCount"].ToString());
					}
				}
				if (rowCount > 0) {
					return true;
				}
				else {
					return false;
				}

			}
			catch(Exception ex) {
				return false;
			}
		}
	}
}
