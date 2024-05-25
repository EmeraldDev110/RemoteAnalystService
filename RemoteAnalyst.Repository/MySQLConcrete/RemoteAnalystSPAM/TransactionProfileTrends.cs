using System;
using MySqlConnector;

namespace RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystSPAM {
    public class TransactionProfileTrends {
        private readonly string _connectionString;
        private const string _mySQLTimeFormat = "yyyy-MM-dd HH:mm:ss";

        public TransactionProfileTrends(string connectionString) {
            _connectionString = connectionString;
        }

        public bool CheckTransactionProfileTrends(string dbName) {
            string cmdText = @"SELECT COUNT(*) AS TableName
                            FROM information_schema.tables 
                            WHERE table_name = @TableName
                            AND Table_Schema = @DatabaseName";

            bool tableExists = false;
            using (var mySqlConnection = new MySqlConnection(_connectionString)) {
                mySqlConnection.Open();
                var cmd = new MySqlCommand(cmdText, mySqlConnection);
                // cmd.prepare();
                cmd.Parameters.AddWithValue("@DatabaseName", dbName);
                cmd.Parameters.AddWithValue("@TableName", "TransactionProfileTrends");
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read()) {
                    if (!Convert.ToInt16(reader["TableName"]).Equals(0))
                        tableExists = true;
                }
            }

            return tableExists;
        }

        public void CreateTransactionProfileTrends() {
            string cmdText = @"CREATE TABLE `TransactionProfileTrends` (
                              `ProfileId` INT NOT NULL,
                              `FromDateTime` DATETIME NULL,
                              `ToDateTime` DATETIME NULL,
                              `TPS` FLOAT NULL);";

            using (var mySqlConnection = new MySqlConnection(_connectionString)) {
                mySqlConnection.Open();
                var cmd = new MySqlCommand(cmdText, mySqlConnection);
                // cmd.prepare();
                cmd.ExecuteNonQuery();
            }
        }

        public void InsertNewData(int profileId, DateTime fromDateTime, DateTime toDateTime, double tps) {
            string cmdText = @"INSERT INTO `TransactionProfileTrends`
                            (`ProfileId`, `FromDateTime`, `ToDateTime`, `TPS`)
                            VALUES (@ProfileId, @FromDateTime, @ToDateTime, @TPS);";
            using (var mySqlConnection = new MySqlConnection(_connectionString)) {
                mySqlConnection.Open();
                var cmd = new MySqlCommand(cmdText, mySqlConnection);
                // cmd.prepare();
                cmd.Parameters.AddWithValue("@ProfileId", profileId);
                cmd.Parameters.AddWithValue("@FromDateTime", fromDateTime.ToString(_mySQLTimeFormat));
                cmd.Parameters.AddWithValue("@ToDateTime", toDateTime.ToString(_mySQLTimeFormat));
                cmd.Parameters.AddWithValue("@TPS", tps);
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateTrendData(int profileId, DateTime fromDateTime, DateTime toDateTime, double tps) {
            string cmdText = @"UPDATE `TransactionProfileTrends` SET `TPS` = @TPS 
                            WHERE ProfileId = @ProfileId AND FromDateTime = @FromDateTime AND ToDateTime = @ToDateTime";
            using (var mySqlConnection = new MySqlConnection(_connectionString)) {
                mySqlConnection.Open();
                var cmd = new MySqlCommand(cmdText, mySqlConnection);
                // cmd.prepare();
                cmd.Parameters.AddWithValue("@ProfileId", profileId);
                cmd.Parameters.AddWithValue("@FromDateTime", fromDateTime.ToString(_mySQLTimeFormat));
                cmd.Parameters.AddWithValue("@ToDateTime", toDateTime.ToString(_mySQLTimeFormat));
                cmd.Parameters.AddWithValue("@TPS", tps);
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
            }
        }

        public bool CheckDuplicatedTrend(int profileId, DateTime fromDateTime, DateTime toDateTime) {
            string cmdText = @"SELECT ProfileId FROM `TransactionProfileTrends`
                            WHERE ProfileId = @ProfileId AND FromDateTime = @FromDateTime AND ToDateTime = @ToDateTime";
            bool exists = false;
            using (var mySqlConnection = new MySqlConnection(_connectionString)) {
                mySqlConnection.Open();
                var cmd = new MySqlCommand(cmdText, mySqlConnection);
                // cmd.prepare();
                cmd.Parameters.AddWithValue("@ProfileId", profileId);
                cmd.Parameters.AddWithValue("@FromDateTime", fromDateTime.ToString(_mySQLTimeFormat));
                cmd.Parameters.AddWithValue("@ToDateTime", toDateTime.ToString(_mySQLTimeFormat));
                var reader = cmd.ExecuteReader();

                if (reader.Read()) {
                    exists = true;
                }
            }

            return exists;
        }
    }
}
