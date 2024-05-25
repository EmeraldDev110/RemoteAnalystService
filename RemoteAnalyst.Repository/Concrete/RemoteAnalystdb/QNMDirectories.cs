using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using RemoteAnalyst.Repository.Infrastructure;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
	public class QNMDirectories {
		private readonly string _connectionString;

		public QNMDirectories(string connectionString) {
			_connectionString = connectionString;
		}

		public void InsertQNMDirectory(int uwsID, string systemSerial, DateTime startTime, DateTime stopTime, string location) {
			string cmdText =
				"INSERT INTO QNMDirectories (UWSID, SystemSerial, StartTime, StopTime, QNMLocation, LoadedDate, Loading) " +
				"VALUES (@UWSID, @SystemSerial, @StartTime, @StopTime, @QNMLocation, @LoadedDate, @Loading)";

			//Get last date from DailySysUnrated.
			using (var connection = new MySqlConnection(_connectionString)) {
				var command = new MySqlCommand(cmdText, connection);
				command.Parameters.AddWithValue("@UWSID", uwsID);
				command.Parameters.AddWithValue("@SystemSerial", systemSerial);
				command.Parameters.AddWithValue("@StartTime", startTime);
				command.Parameters.AddWithValue("@StopTime", stopTime);
				command.Parameters.AddWithValue("@QNMLocation", location);
				command.Parameters.AddWithValue("@LoadedDate", DateTime.Now);
				command.Parameters.AddWithValue("@Loading", 0);
				connection.Open();
				command.ExecuteNonQuery();
			}
		}

	}
}
