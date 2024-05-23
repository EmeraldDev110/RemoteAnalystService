using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
	public class PathwayDirectoryService {
		private readonly string _connectionString = "";

		public PathwayDirectoryService(string connectionString) {
			_connectionString = connectionString;
		}

		public bool CheckDuplicateTimeFor(string systemSerial, DateTime startTime, DateTime stopTime, string location) {
			var directories = new PathwayDirectories(_connectionString);
			bool retVal = directories.CheckDuplicateTime(systemSerial, startTime, stopTime, location);
			return retVal;
		}

		public void InsertpathwayDirectoryFor(int uwsID, string systemSerial, DateTime startTime, DateTime stopTime, string location) {
			var directories = new PathwayDirectories(_connectionString);
			directories.InsertPathwayDirectory(uwsID, systemSerial, startTime, stopTime, location);
		}

	}
}
