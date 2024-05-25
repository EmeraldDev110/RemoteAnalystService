
using MySqlConnector;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using RemoteAnalyst.Repository.Helpers;
using RemoteAnalyst.Repository.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RemoteAnalyst.Repository.Repositories
{
    public class DailyCPUDataRepository
    {
        private readonly string _connectionString;

        public DailyCPUDataRepository(string connectionStringTrend)
        {
            _connectionString = connectionStringTrend;
        }

        public bool CheckTableName(string databaseName)
        {
            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
                {
                    session.Query<DailyCPUData>().FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("doesn't exist")) return false;
                throw ex;
            }
            return true;
        }

        public void CreateDailyCPUDatas()
        {
            //TODO: Convert to NHibernate
            var cmdText = @"CREATE TABLE `DailyCPUDatas` (
                          `DateTime` DATETIME NOT NULL,
                          `CpuNumber` INT NOT NULL,
                          `CPUBusy` DOUBLE NULL,
                          `CPUQueue` DOUBLE NULL,
                          PRIMARY KEY (`DateTime`, `CpuNumber`));";

            using (var mySqlConnection = new MySqlConnection(_connectionString))
            {
                mySqlConnection.Open();
                var cmd = new MySqlCommand(cmdText, mySqlConnection);
                // cmd.prepare();
                cmd.ExecuteNonQuery();
            }
        }

        public bool CheckDailiesTopProcessesTableName(string databaseName)
        {
            string cmdText = @"SELECT COUNT(*) AS TableName
                            FROM information_schema.tables 
                            WHERE table_name = 'DailiesTopProcesses'
                            AND Table_Schema = @DatabaseName";
            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
                {
                    session.Query<DailiesTopProcess>().FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("doesn't exist")) return false;
                throw ex;
            }
            return true;
        }

        public void CreateDailiesTopProcessesTable()
        {
            //TODO: Convert to NHibernate
            var cmdText = @"CREATE TABLE `DailiesTopProcesses` (
                          `DataType` int(11) NOT NULL,
                          `FromTimestamp` datetime NOT NULL,
                          `CpuNum` int(11) NOT NULL,
                          `PIN` int(11) NOT NULL,
                          `IpuNum` bigint(20) NOT NULL,
                          `ProcessName` varchar(8) NOT NULL,
                          `Priority` int(11) NOT NULL,
                          `Busy` double DEFAULT NULL,
                          `Program` varchar(45) DEFAULT NULL,
                          `ReceiveQueue` double DEFAULT NULL,
                          `MemUsed` double DEFAULT NULL,
                          `AncestorProcessName` varchar(8) DEFAULT NULL,
                          `User` tinyint(3) unsigned DEFAULT NULL,
                          `Group` tinyint(3) unsigned DEFAULT NULL,
                          PRIMARY KEY (`DataType`,`FromTimestamp`,`CpuNum`,`PIN`,`IpuNum`,`ProcessName`,`Priority`)
                        )";

            using (var mySqlConnection = new MySqlConnection(_connectionString))
            {
                mySqlConnection.Open();
                var cmd = new MySqlCommand(cmdText, mySqlConnection);
                // cmd.prepare();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
