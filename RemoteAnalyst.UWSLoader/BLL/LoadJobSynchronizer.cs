using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RemoteAnalyst.UWSLoader.BLL {
	public static class LoadJobSynchronizer {
		public static bool SynchromizeLoadJob(WaitHandle[] loadJobThreads) {
			int retry = 20;
			int index = 0;
			while (!WaitHandle.WaitAll(loadJobThreads, 30000) && index < retry) {
				retry++;
			}
			if(index == retry) {
				return false;
			}
			else {
				return true;
			}
		}
	}
}
