using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices {
	public class UWSArchiveService : IArchiveService {
		private readonly string _connectionString = "";

		public UWSArchiveService(string connectionString) {
			_connectionString = connectionString;
		}

		public void InsertArchiveIDFor(DateTime startTime, DateTime stopTime, string archiveID, DateTime creationDate, int status) {
			var uwsArchive = new UWSArchive(_connectionString);
			uwsArchive.InsertArchiveID(startTime, stopTime, archiveID, creationDate, status);
		}
	}
}
