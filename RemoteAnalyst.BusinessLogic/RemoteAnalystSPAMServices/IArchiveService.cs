using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices {
	public interface IArchiveService {
		void InsertArchiveIDFor(DateTime startTime, DateTime stopTime, string archiveID, DateTime creationDate, int status);
	}
}
