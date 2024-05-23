using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
	public class NullCheckService {
		public bool NullCheckForPathwayPramaterPvCollects(DateTime fromTimestamp, DateTime toTimestamp, string connectionStringSystem) {
			var nullChecker = new NullCheck();
			return nullChecker.NullCheckForPathwayPramaterPvCollects(fromTimestamp, toTimestamp, connectionStringSystem);
		}
		public bool NullCheckForPathwayPramaterPvPwyList(DateTime fromTimestamp, DateTime toTimestamp, string connectionStringSystem) {
			var nullChecker = new NullCheck();
			return nullChecker.NullCheckForPathwayPramaterPvPwyList(fromTimestamp, toTimestamp, connectionStringSystem);
		}
	}
}
