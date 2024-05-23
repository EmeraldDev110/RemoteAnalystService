using System;
using System.Collections.Generic;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices {
    public class PathwayService {
        private readonly string ConnectionString;

        public PathwayService(string connectionStringTrend) {
            ConnectionString = connectionStringTrend;
        }

        public List<string> GetListOfPathwayTablesFor() {
            var pathway = new Pathway(ConnectionString);
            var tables = pathway.GetListOfPathwayTables();
            return tables;
        }

        public void DeleteDataFor(DateTime oldDate, string tableName) {
            var pathway = new Pathway(ConnectionString);
            pathway.DeleteData(oldDate, tableName);
        }
    }
}