using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace RemoteAnalyst.BusinessLogic.ModelView {
	public class RDSLoaderInfo {
		public string InstanceID { get; set; }

		public static string GetRDSLoaderInfo(string instanceID) {
			var info = new ReportGeneratorInfo {
				InstanceID = instanceID
			};
			return new JavaScriptSerializer().Serialize(info);
		}
	}
}
