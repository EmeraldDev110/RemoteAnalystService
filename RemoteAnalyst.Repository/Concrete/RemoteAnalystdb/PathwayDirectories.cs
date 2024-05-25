using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
	public class PathwayDirectories {
		private readonly string _connectionString;

		public PathwayDirectories(string connectionString) {
			_connectionString = connectionString;
		}

		public bool CheckDuplicateTime(string systemSerial, DateTime startTime, DateTime stopTime, string location) {
			bool exists = false;
			string cmdText = @"SELECT SystemSerial FROM PathwayDirectories WHERE 
                                SystemSerial = @SystemSerial AND 
                                StartTime = @StartTime AND 
                                StopTime = @StopTime AND
                                PathwayLocation = @PathwayLocation";

			//Get last date from DailySysUnrated.
			using (var connection = new MySqlConnection(_connectionString)) {
				var command = new MySqlCommand(cmdText, connection);
				command.Parameters.AddWithValue("@SystemSerial", systemSerial);
				command.Parameters.AddWithValue("@StartTime", startTime);
				command.Parameters.AddWithValue("@StopTime", stopTime);
				command.Parameters.AddWithValue("@PathwayLocation", location);
				connection.Open();

				var reader = command.ExecuteReader();
				if (reader.Read()) {
					exists = true;
				}
			}
			return exists;
		}

		public void InsertPathwayDirectory(int uwsID, string systemSerial, DateTime startTime, DateTime stopTime, string location) {
			string cmdText =
				"INSERT INTO PathwayDirectories (UWSID, SystemSerial, StartTime, StopTime, PathwayLocation, LoadedDate, Loading) " +
				"VALUES (@UWSID, @SystemSerial, @StartTime, @StopTime, @PathwayLocation, @LoadedDate, @Loading)";

			//Get last date from DailySysUnrated.
			using (var connection = new MySqlConnection(_connectionString)) {
				var command = new MySqlCommand(cmdText, connection);
				command.Parameters.AddWithValue("@UWSID", uwsID);
				command.Parameters.AddWithValue("@SystemSerial", systemSerial);
				command.Parameters.AddWithValue("@StartTime", startTime);
				command.Parameters.AddWithValue("@StopTime", stopTime);
				command.Parameters.AddWithValue("@PathwayLocation", location);
				command.Parameters.AddWithValue("@LoadedDate", DateTime.Now);
				command.Parameters.AddWithValue("@Loading", 0);
				connection.Open();
				command.ExecuteNonQuery();
			}
		}
	}
}
