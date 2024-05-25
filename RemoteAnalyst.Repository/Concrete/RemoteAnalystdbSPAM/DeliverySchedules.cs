using System.Data;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM
{
    public class DeliverySchedules
    {
        private readonly string _connectionString;

        public DeliverySchedules(string connectionStriong)
        {
            _connectionString = connectionStriong;
        }

        public DataTable GetSchdules(int typeID)
        {
            string cmdText = @"SELECT DS_DeliveryID, DS_TrendReportID, DS_SystemSerial, 
                             FR_FrequencyName, DS_SendDay, DS_SendMonth
                             FROM DeliverySchedules, Frequencies WHERE DS_FrequencyID=FR_FrequencyID
                             AND DS_ReportTypeID = @ReportTypeID ORDER BY DS_SystemSerial";

            var schdule = new DataTable();
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@ReportTypeID", typeID);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(schdule);
            }

            return schdule;
        }

        public DataTable GetSchduleData(int deliveryID)
        {
            string cmdText =
                @"SELECT DS_TrendReportID, DS_SystemSerial, RT_ReportType, DS_PeriodType, DS_PeriodCount, DS_Title 
                            FROM DeliverySchedules, ReportTypes WHERE DS_DeliveryID=@DeliveryID AND RT_ReportTypeID=DS_ReportTypeID";

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

        public DataTable GetQTSchdule()
        {
            string cmdText = @"SELECT DS_DeliveryID, DS_TrendReportID, DS_SystemSerial,
                        FR_FrequencyName, DS_SendDay, DS_SendMonth, ReportName,
                        DS_StartTime, DS_StopTime, DS_ProcessDate, DS_Title,
                        QLenAlert, FOpenAlert, FLockWaitAlert, MinProcBusy,
                        ExSourceCPU, ExDestCPU, ExProgName, DS_IsWeekdays, DS_FrequencyWeekday, 
                        DS_FrequencyMonthCount, DS_IsReportDataLast, DS_ReportDataWeekday
                        FROM DeliverySchedules
                        INNER JOIN Frequencies ON DS_FrequencyID = FR_FrequencyID
                        INNER JOIN QTGroups ON DS_TrendReportID = GroupID
                        WHERE DS_ReportTypeID = '7'";

            var schdule = new DataTable();
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(schdule);
            }

            return schdule;
        }

        public DataTable GetDPASchdule()
        {
            string cmdText = @"SELECT DS_DeliveryID, DS_TrendReportID, DS_SystemSerial,
                        FR_FrequencyName, DS_SendDay, DS_SendMonth, GroupName,
                        DS_StartTime, DS_StopTime, DS_ProcessDate, DS_Title, DS_IsWeekdays, 
                        DS_FrequencyWeekday, DS_FrequencyMonthCount, DS_IsReportDataLast, DS_ReportDataWeekday
                        FROM DeliverySchedules
                        INNER JOIN Frequencies ON DS_FrequencyID = FR_FrequencyID
                        INNER JOIN iReportGroups ON DS_TrendReportID = GroupID
                        WHERE DS_ReportTypeID = '6'";

            var schdule = new DataTable();
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(schdule);
            }

            return schdule;
        }

        public DataTable GetTPSSchdule() {
            string cmdText = @"SELECT DS_DeliveryID, DS_TrendReportID, DS_SystemSerial,
                            FR_FrequencyName, DS_SendDay, DS_SendMonth, TransactionProfileName, TransactionProfileId,
                            DS_StartTime, DS_StopTime, DS_ProcessDate, DS_Title, DS_IsWeekdays, 
                            DS_FrequencyWeekday, DS_FrequencyMonthCount, DS_IsReportDataLast, DS_ReportDataWeekday
                            FROM DeliverySchedules
                            INNER JOIN Frequencies ON DS_FrequencyID = FR_FrequencyID
                            INNER JOIN TransactionProfiles ON DS_TrendReportID = TransactionProfileId
                            WHERE DS_ReportTypeID = '10'";

            var schdule = new DataTable();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(schdule);
            }

            return schdule;
        }
    }
}