using System.Data;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM
{
    public class StorageReport
    {
        private readonly string _connectionString;

        public StorageReport(string connectionStriong)
        {
            _connectionString = connectionStriong;
        }

        public bool CheckCapacities(int deliveryID)
        {
            string cmdText = @"SELECT SR_StorageID FROM DeliverySchedules
                            INNER JOIN StorageReport ON DS_TrendReportID = SR_ReportID
                            WHERE (SR_StorageID = '2' OR SR_StorageID = '3')
                            AND DS_DeliveryID = @DeliveryID";
            bool exists = false;

            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@DeliveryID", deliveryID);

                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    exists = true;
                }
            }
            return exists;
        }

        public DataTable GetSchduleData(int deliveryID)
        {
            string cmdText = @"SELECT SR_StorageID, SR_GroupDisk, SR_GroupDiskID, SR_CustomerID
                                FROM DeliverySchedules
                                INNER JOIN StorageReport ON DS_TrendReportID = SR_ReportID
                                WHERE DS_DeliveryID = @DeliveryID";

            var schdule = new DataTable();
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@DeliveryID", deliveryID);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(schdule);
            }

            return schdule;
        }
    }
}