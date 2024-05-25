using System;
using System.Collections.Generic;
using MySqlConnector;
using RemoteAnalyst.Repository.MySQLConcrete;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM {
	public class UWSArchive {
		private readonly string _connectionString;

		public UWSArchive(string connectionString) {
			_connectionString = connectionString;
		}

		public void InsertArchiveID(DateTime startTime, DateTime stopTime, string ArchiveID, DateTime creationDate, int status) {
			string cmdText = @"INSERT INTO `UWSArchive`
									(`FromTimestamp`,
									`ToTimestamp`,
									`Status`,
									`ArchiveID`,
									`CreationDate`)
									VALUES
									(@StartTime,
									 @StopTime,
									 @Status,
									 @ArchiveID,
									 @CreationDate)";
			try {
				using (var connection = new MySqlConnection(_connectionString)) {
					var command = new MySqlCommand(cmdText + Infrastructure.Helper.CommandParameter, connection);
					command.Parameters.AddWithValue("@StartTime", startTime.ToString(Helper._mySQLTimeFormat));
					command.Parameters.AddWithValue("@StopTime", stopTime.ToString(Helper._mySQLTimeFormat));
					command.Parameters.AddWithValue("@Status", status);
					command.Parameters.AddWithValue("@ArchiveID", ArchiveID);
					command.Parameters.AddWithValue("@CreationDate", creationDate);
					connection.Open();
					command.ExecuteNonQuery();
				}
			}
			catch (Exception ex) {
				throw new Exception(ex.Message);
			}
		}
	}
}
