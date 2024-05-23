using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.Repository.Infrastructure;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
	public class QNMDirectoriesService {
		private readonly string _connectionString = "";

		public QNMDirectoriesService(string connectionString) {
			_connectionString = connectionString;
		}

		public void InsertQNMDirectoryFor(int uwsID, string systemSerial, DateTime startTime, DateTime stopTime, string location) {
			var directories = new QNMDirectories(_connectionString);
			directories.InsertQNMDirectory(uwsID, systemSerial, startTime, stopTime, location);
		}

	}
}
