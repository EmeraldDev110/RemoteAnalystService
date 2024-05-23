using System;
using System.Collections.Generic;
using System.Data;
using RemoteAnalyst.BusinessLogic.UWSLoader.BaseClass;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices {
    public class CPUEntityTableService {
        private readonly string _connectionString = "";

        public CPUEntityTableService(string connectionString) {
            _connectionString = connectionString;
        }

        public List<MultiDays> GetCPUEntityTableIntervalListFor(string entityTableName, string mySqlConnectionString) {
            var cpuEntityTable = new CPUEntityTable(mySqlConnectionString);
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

        public int GetPageSizeBytesFor(string cpuTableName) {
            var cpuEntityTable = new CPUEntityTable(_connectionString);
            var pageSizeBytes = cpuEntityTable.GetPageSizeBytes(cpuTableName);

            return pageSizeBytes;
        }

        public int GetPageSizeBytesFor(List<string> cpuTableNames) {
            var cpuEntityTable = new CPUEntityTable(_connectionString);
            var pageSizeBytes = 0;

            foreach (var cpuTableName in cpuTableNames) {
                try {
                    pageSizeBytes = cpuEntityTable.GetPageSizeBytes(cpuTableName);
                }
                catch {
                }
            }

            return pageSizeBytes;
        }
    }
}