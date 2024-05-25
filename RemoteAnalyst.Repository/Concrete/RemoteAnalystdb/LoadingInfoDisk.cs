using System;
using System.Data;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb
{
    public class LoadingInfoDisk
    {
        private readonly string _connectionString;

        public LoadingInfoDisk(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable GetUWSFileName(string systemSerial)
        {
            var fileNames = new DataTable();

            string cmdText = @"SELECT DiskUWSID, FileName, UploadedTime FROM LoadingInfoDisk
                               WHERE SystemSerial = @SystemSerial";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(fileNames);
            }

            return fileNames;
        }

        public void DeleteLoadingInfoDisk(int uwsID)
        {
            string cmdText = "DELETE FROM LoadingInfoDisk WHERE DiskUWSID = @DiskUWSID";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var commnad = new MySqlCommand(cmdText, connection);
                commnad.CommandTimeout = 0;
                commnad.Parameters.AddWithValue("@DiskUWSID", uwsID);
                connection.Open();
                commnad.ExecuteNonQuery();
            }
        }

        public void UpdateFailedToLoadDisk(string fileName)
        {
            string cmdText = "UPDATE LoadingInfoDisk SET Failed = '1' WHERE " +
                             "FileName = @FileName";
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.Add("@FileName", MySqlDbType.VarChar).Value = fileName;
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateLoadingInfoDisk(string fileName)
        {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string cmdText = "UPDATE LoadingInfoDisk SET LoadedTime = @LoadedTime WHERE FileName = @FileName";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@LoadedTime", DateTime.Now);
                command.Parameters.AddWithValue("@FileName", fileName);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Insert(string systemSerial, int customerID, string fileName, long fileSize)
        {
            string cmdText =
                "INSERT INTO LoadingInfoDisk (SystemSerial, CustomerID, FileName, FileSize, UploadedTime, Failed) " +
                "VALUES (@SystemSerial, @CustomerID, @FileName, @FileSize, @UploadTime, @Failed)";

            //Insert into the database.
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@CustomerID", customerID.ToString());
                command.Parameters.AddWithValue("@FileName", fileName);
                command.Parameters.AddWithValue("@FileSize", fileSize.ToString());
                command.Parameters.AddWithValue("@UploadTime", DateTime.Now);
                command.Parameters.AddWithValue("@Failed", "0");

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}