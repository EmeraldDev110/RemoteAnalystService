using Amazon;
using System.Configuration;
using System.Diagnostics;

namespace RemoteAnalyst.AWS {
    public static class Helper {
        public static RegionEndpoint GetRegionEndpoint() {
			string region = ConfigurationManager.AppSettings["RegionEndpoint"];
            RegionEndpoint endPoint = null;
            if (region.Equals("west2")) {
                endPoint = RegionEndpoint.USWest2;
            }
            else if (region.Equals("west")) // Default west region
            {
                endPoint = RegionEndpoint.USWest2;
            }
            else if (region.Equals("east2")) {
                endPoint = RegionEndpoint.USEast2;
            } else if (region.Equals("east1")) {
                endPoint = RegionEndpoint.USEast1;
            }
            return endPoint;
        }
    }
}