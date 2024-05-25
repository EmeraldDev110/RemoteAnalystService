using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using RemoteAnalyst.UWSLoader.Core.BaseClass;
using RemoteAnalyst.UWSLoader.Core.Repository;

namespace RemoteAnalyst.UWSLoader.Core.BusinessLogic {
    class CPUEntityTableService {
        private readonly string _connectionString = "";

        public CPUEntityTableService(string connectionString) {
            _connectionString = connectionString;
        }

        public int GetCPUEntityTableColumnCountFor(string systemSerial, string databaseName, string entityTableName) {
            var cpuEntityTable = new CPUEntityTable(_connectionString);
            int count = cpuEntityTable.GetCPUEntityTableColumnCount(systemSerial, databaseName, entityTableName);
            return count;
        }

        public List<MultiDays> GetCPUEntityTableIntervalListFor(string entityTableName) {
            var cpuEntityTable = new CPUEntityTable(_connectionString);
            var cpuIntervalList = new List<MultiDays>();
            DataTable intervalList = cpuEntityTable.GetCPUEntityTableIntervalList(entityTableName);
            foreach (DataRow dr in intervalList.Rows) {
                var startEnd = new MultiDays {
                    StartDate = Convert.ToDateTime(dr["FromTimestamp"]),
                    EndDate = Convert.ToDateTime(dr["ToTimestamp"]),
                };
                cpuIntervalList.Add(startEnd);
            }
            return cpuIntervalList;
        }
    }
}
