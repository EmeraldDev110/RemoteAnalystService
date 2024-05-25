using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using RemoteAnalyst.UWSLoader.Core.BaseClass;
using RemoteAnalyst.UWSLoader.Core.Repository;

namespace RemoteAnalyst.UWSLoader.Core.BusinessLogic {
    class DISCEntityTableService {
        private readonly string _connectionString = "";

        public DISCEntityTableService(string connectionString) {
            _connectionString = connectionString;
        }

        public List<string> GetDeviceNamesFor(string entityTableName) {
            var discEntityTable = new DISCEntityTable(_connectionString);
            List<string> deviceNames = discEntityTable.GetDeviceNames(entityTableName);
            return deviceNames;
        }

        public List<MultiDays> GetDISCEntityTableIntervalListFor(string entityTableName) {
            var cpuEntityTable = new DISCEntityTable(_connectionString);
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
            var discEntityTable = new DISCEntityTable(_connectionString);
            return discEntityTable.CheckTableName(entityTableName);
        }
    }
}
