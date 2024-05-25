using System;
using System.Data;
using MySqlConnector;
using System.Threading;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb
{
    public class SampleInfo
    {
        private readonly string ConnectionString = "";

        public SampleInfo(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public void InsertNewEntry(int nsid, string systemName, string systemSerial, DateTime startTimeLCT,
            DateTime stopTimeLCT, long sampleInterval, int uwsID, string sysContent, int type)
        {
            string cmdText = "INSERT INTO SampleInfo (CustomerID, DatabaseID, NSID, SystemName, SystemSerial, " +
                             "SampleType, StartTimeLCT, StopTimeLCT, SampleInterval, UwsID, " +
                             "SysContent, ExpireUws, ExpireSample) VALUES (@CustomerID, @DatabaseID, @NSID, @SystemName, @SystemSerial, " +
                             "@SampleType, @StartTimeLCT, @StopTimeLCT, @SampleInterval, @UwsID, " +
                             "@SysContent, @ExpireUws, @ExpireSample)";

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@CustomerID", 0);
                    command.Parameters.AddWithValue("@DatabaseID", "");
                    command.Parameters.AddWithValue("@NSID", nsid);
                    command.Parameters.AddWithValue("@SystemName", systemName);
                    command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                    command.Parameters.AddWithValue("@SampleType", type);
                    command.Parameters.AddWithValue("@StartTimeLCT", startTimeLCT);
                    command.Parameters.AddWithValue("@StopTimeLCT", stopTimeLCT);
                    command.Parameters.AddWithValue("@SampleInterval", sampleInterval);
                    command.Parameters.AddWithValue("@UwsID", uwsID);
                    command.Parameters.AddWithValue("@SysContent", sysContent);
                    command.Parameters.AddWithValue("@ExpireUws", DateTime.Now.AddDays(20));
                    command.Parameters.AddWithValue("@ExpireSample", DateTime.Now.AddDays(20));

                    connection.Open();
                    command.ExecuteReader();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool CheckDuplicateData(string systemSerial, DateTime startTime, DateTime endTime, bool isSystem)
        {
            //Force all datetime to be in US format.
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            //sampleType: 3 = Pathway, 4 = System.
            int sampleType = 3;
            if (isSystem)
                sampleType = 4;

            bool result = false;

            string cmdText = "select * from SampleInfo where " +
                             "systemSerial = @SystemSerial " +
                             "AND starttimeLCT = @StartTime " +
                             "AND stoptimeLCT = @EndTime " +
                             "AND sampletype = @SampleType";


            try {
                var data = new DataTable();
                using (var connection = new MySqlConnection(ConnectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                    command.Parameters.AddWithValue("@StartTime", startTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@EndTime", endTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@SampleType", sampleType);
                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(data);
                    if (data.Rows.Count > 0) {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }

        public void UpdateExpireInfo(DateTime retentionDate, string newNsid)
        {
            string cmdText =
                @"UPDATE SampleInfo SET expireUws = @RetentionDate, expireSample = @RetentionDate WHERE NSID = @newNsid";
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@RetentionDate", retentionDate);
                    command.Parameters.AddWithValue("@newNsid", newNsid);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateStopTime(string stopTime, string newNsid)
        {
            string cmdText =
                "UPDATE SampleInfo SET stoptimeGMT = @StopTimeGMT, stoptimeLCT = @StopTimeLCT WHERE NSID = @NSID";
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@StopTimeGMT", stopTime);
                    command.Parameters.AddWithValue("@StopTimeLCT", stopTime);
                    command.Parameters.AddWithValue("@NSID", newNsid);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}