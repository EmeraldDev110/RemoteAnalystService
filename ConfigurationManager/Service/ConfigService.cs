using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager.Service {
	class ConfigService {
		public string EditConnectionString(string databaseDomainName, string databasePort, string databaseUserName, string databasePassword) {
			var connectionString = "SERVER=" + databaseDomainName + ";PORT=" + databasePort + ";DATABASE=pmc;UID=" + databaseUserName + ";PASSWORD=" + databasePassword + ";";
			return connectionString;
		}
	}
}
