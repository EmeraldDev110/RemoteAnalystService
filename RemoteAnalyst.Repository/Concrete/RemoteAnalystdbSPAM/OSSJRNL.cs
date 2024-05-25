using System.Data;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM {
    public class OSSJRNL {
        private readonly string ConnectionString = "";

        public OSSJRNL(string connectionString) {
            ConnectionString = connectionString;
        }

        public void CreateOSSTable(string tableName) {
            string cmdText = @"CREATE TABLE `" + tableName + @"` (
                               `FileType` NVARCHAR(10) NOT NULL,
                                `FileName` NVARCHAR(50) NOT NULL,
                                `PathName` NVARCHAR(255) NOT NULL,
                                PRIMARY KEY (`FileType`,`FileName`, `PathName`)
                              )";

            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void CreateOSSIndex(string tableName)
        {
            string cmdText = @"ALTER TABLE `" + tableName + "` " +
                "ADD INDEX `OSSNames_FileName` (`FileName` ASC), " +
                "ADD INDEX `OSSNames_PathName` (`PathName` ASC);";

            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool CheckDuplicate(string tableName) {
            bool duplicate = true;
            string cmdText = @"SELECT PathName FROM " + tableName + " LIMIT 1";

            try {
                using (var connection = new MySqlConnection(ConnectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    connection.Open();
                    command.ExecuteReader();
                }
            }
            catch {
                duplicate = false;
            }
            return duplicate;
        }
    }
}