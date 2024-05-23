using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices {
    public class TransactionProfileServices {
        private readonly string _connectionString;

        public TransactionProfileServices(string connectionString) {
            _connectionString = connectionString;
        }

        public List<TransactionProfileInfo> GetTransactionProfileInfoFor(string systemSerial) {
            var transactionProfiles = new TransactionProfiles(_connectionString);
            var profileDataTable = transactionProfiles.GetTransactionProfileInfo(systemSerial);

            return (from DataRow dataRow in profileDataTable.Rows
                select new TransactionProfileInfo {
                    TransactionProfileID = Convert.ToInt32(dataRow["TransactionProfileID"]), 
                    TransactionFile = Convert.ToString(dataRow["TransactionFile"]),
                    OpenerType = Convert.ToInt16(dataRow["OpenerType"]),
                    OpenerName = Convert.ToString(dataRow["OpenerName"]), 
                    TransactionCounter = Convert.ToInt16(dataRow["TransactionCounter"]), 
                    IOTransactionRatio = Convert.ToDouble(dataRow["IOTransactionRatio"]),
                    IsCpuToFile = dataRow.IsNull("IsCpuToFile") || Convert.ToBoolean(dataRow["IsCpuToFile"])
                }).ToList();
        }

        public TransactionProfileInfo GetTransactionProfileInfoFor(string systemSerial, int profileId) {
            var transactionProfiles = new TransactionProfiles(_connectionString);
            var profileDataTable = transactionProfiles.GetTransactionProfileInfo(systemSerial, profileId);

            var profileInfo = new TransactionProfileInfo();

            if (profileDataTable.Rows.Count > 0) {
                profileInfo = new TransactionProfileInfo {
                    TransactionProfileID = Convert.ToInt32(profileDataTable.Rows[0]["TransactionProfileID"]),
                    TransactionFile = Convert.ToString(profileDataTable.Rows[0]["TransactionFile"]),
                    OpenerType = Convert.ToInt16(profileDataTable.Rows[0]["OpenerType"]),
                    OpenerName = Convert.ToString(profileDataTable.Rows[0]["OpenerName"]),
                    TransactionCounter = Convert.ToInt16(profileDataTable.Rows[0]["TransactionCounter"]),
                    IOTransactionRatio = Convert.ToDouble(profileDataTable.Rows[0]["IOTransactionRatio"]),
                    IsCpuToFile = profileDataTable.Rows[0].IsNull("IsCpuToFile") || Convert.ToBoolean(profileDataTable.Rows[0]["IsCpuToFile"])
                };
            }

            return profileInfo;
        }

    }
}
