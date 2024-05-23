using System;
using System.Collections.Generic;
using System.Data;
using RemoteAnalyst.BusinessLogic.UWSLoader.BaseClass;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices {
    public class DISCEntityTableService {
        private readonly string ConnectionString = "";

        public DISCEntityTableService(string connectionString) {
            ConnectionString = connectionString;
        }

        public List<MultiDays> GetDISCEntityTableIntervalListFor(string entityTableName) {
            var cpuEntityTable = new DISCEntityTable(ConnectionString);
            var cpuIntervalList = new List<MultiDays>();
            bool exists = CheckTableNameFor(entityTableName);
            if (exists) {
                DataTable intervalList = cpuEntityTable.GetDISCEntityTableIntervalList(entityTableName);
                foreach (DataRow dr in intervalList.Rows) {
                    var startEnd = new MultiDays {
                        StartDate = Convert.ToDateTime(dr["FromTimestamp"]),
                        EndDate = Convert.ToDateTime(dr["ToTimestamp"]),
                    };
                    cpuIntervalList.Add(startEnd);
                }
            }
            return cpuIntervalList;
        }

        public bool CheckTableNameFor(string entityTableName) {
            var discEntityTable = new DISCEntityTable(ConnectionString);
            return discEntityTable.CheckTableName(entityTableName);
        }
    }
}