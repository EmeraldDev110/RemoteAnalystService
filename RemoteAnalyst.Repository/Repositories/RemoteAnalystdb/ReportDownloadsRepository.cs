using log4net;
using MySqlConnector;
using NHibernate.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Repositories
{
    public class ReportDownloadsRepository
    {
        public int InsertNewReport(string systemSerial, int typeId, int custId)
        {
            using (NHibernate.ISession session = NHibernateHelper.OpenSession("ReportDownloads"))
            {
                Models.ReportDownloads rq = new Models.ReportDownloads();
                rq.SystemSerial = systemSerial;
                rq.TypeID = typeId;
                rq.OrderBy = custId;
                rq.Status = 0;
                rq.RequestDate = DateTime.Now;
                var id = session.Save(rq);
                return Convert.ToInt32(id);
            }
            /*int reportDownloadId = 0;
            string cmdInsert = @"INSERT INTO ReportDownloads (SystemSerial, TypeID, OrderBy, Status, RequestDate) 
                                VALUES (@SystemSerial, @TypeID, @OrderBy, @Status, @RequestDate); SELECT LAST_INSERT_ID();";

            using (var connection = new MySqlConnection(ConnectionString))
            {
                var insertCmd = new MySqlCommand(cmdInsert, connection);
                insertCmd.CommandTimeout = 0;
                insertCmd.Parameters.AddWithValue("@SystemSerial", systemSerial);
                insertCmd.Parameters.AddWithValue("@TypeID", typeId);
                insertCmd.Parameters.AddWithValue("@OrderBy", custId);
                insertCmd.Parameters.AddWithValue("@Status", 0);
                insertCmd.Parameters.AddWithValue("@RequestDate", DateTime.Now);
                connection.Open();
                reportDownloadId = Convert.ToInt32(insertCmd.ExecuteScalar());
            }
            return reportDownloadId;*/
        }

        public int InsertNewReport(string systemSerial, DateTime startDateTime, DateTime stopDateTime, int typeId, int custId, ILog log)
        {
            using (NHibernate.ISession session = NHibernateHelper.OpenSession("ReportDownloads"))
            {
                Models.ReportDownloads rq = new Models.ReportDownloads();
                rq.SystemSerial = systemSerial;
                rq.TypeID = typeId;
                rq.OrderBy = custId;
                rq.Status = 0;
                rq.StartTime = startDateTime;
                rq.EndTime = stopDateTime;
                rq.RequestDate = DateTime.Now;
                var id = session.Save(rq);
                return Convert.ToInt32(id);
            }
            /*int reportDownloadId = 0;
            string cmdInsert = @"INSERT INTO ReportDownloads (SystemSerial, StartTime, EndTime, TypeID, OrderBy, Status, RequestDate) 
                                VALUES (@SystemSerial, @StartTime, @EndTime, @TypeID, @OrderBy, @Status, @RequestDate); SELECT LAST_INSERT_ID();";
            log.Info("Insert entry");
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    var insertCmd = new MySqlCommand(cmdInsert, connection);
                    insertCmd.CommandTimeout = 0;
                    insertCmd.Parameters.AddWithValue("@SystemSerial", systemSerial);
                    insertCmd.Parameters.AddWithValue("@StartTime", startDateTime);
                    insertCmd.Parameters.AddWithValue("@EndTime", stopDateTime);
                    insertCmd.Parameters.AddWithValue("@TypeID", typeId);
                    insertCmd.Parameters.AddWithValue("@OrderBy", custId);
                    insertCmd.Parameters.AddWithValue("@Status", 0);
                    insertCmd.Parameters.AddWithValue("@RequestDate", DateTime.Now);
                    connection.Open();
                    reportDownloadId = Convert.ToInt32(insertCmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error: {0}", ex);
            }
            return reportDownloadId;*/
        }

        public void UpdateFileLocation(string systemSerial, DateTime startTime, DateTime stopTime, int typeID,
            string fileLocation)
        {
            using (NHibernate.ISession session = NHibernateHelper.OpenSession("ReportDownloads"))
            {
                session.Query<Models.ReportDownloads>()
                    .Where(c => c.SystemSerial == systemSerial && c.StartTime == startTime && c.EndTime == stopTime && c.TypeID == typeID)
                    .UpdateBuilder()
                    .Set(c => c.GenerateDate, DateTime.Now)
                    .Set(c => c.FileLocation, fileLocation)
                    .Update();
            }
            /*string cmdText = @"UPDATE ReportDownloads SET GenerateDate = @GenerateDate, FileLocation = @FileLocation, Status = 1
                           WHERE SystemSerial = @SystemSerial AND StartTime = @StartTime AND EndTime = @EndTime AND TypeID = @TypeID";

            using (var connection = new MySqlConnection(ConnectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@EndTime", stopTime);
                command.Parameters.AddWithValue("@TypeID", typeID);
                command.Parameters.AddWithValue("@GenerateDate", DateTime.Now);
                command.Parameters.AddWithValue("@FileLocation", fileLocation);
                connection.Open();
                command.ExecuteNonQuery();
            }*/
        }

        public void FileFailed(int reportDownloadId)
        {
            using (NHibernate.ISession session = NHibernateHelper.OpenSession("ReportDownloads"))
            {
                session.Query<Models.ReportDownloads>()
                    .Where(c => c.ReportDownloadId == reportDownloadId)
                    .UpdateBuilder()
                    .Set(c => c.Status, 2)
                    .Update();
            }
            /*string cmdText = @"UPDATE ReportDownloads SET Status = 2 WHERE ReportDownloadId = @ReportDownloadId";

            using (var connection = new MySqlConnection(ConnectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@ReportDownloadId", reportDownloadId);
                connection.Open();
                command.ExecuteNonQuery();
            }*/
        }
        public void UpdateFileLocation(int reportDownloadId, string fileLocation)
        {
            using (NHibernate.ISession session = NHibernateHelper.OpenSession("ReportDownloads"))
            {
                session.Query<Models.ReportDownloads>()
                    .Where(c => c.ReportDownloadId == reportDownloadId)
                    .UpdateBuilder()
                    .Set(c => c.GenerateDate, DateTime.Now)
                    .Set(c => c.FileLocation, fileLocation)
                    .Set(c => c.Status, 1)
                    .Update();
            }
            /*string cmdText = @"UPDATE ReportDownloads SET GenerateDate = @GenerateDate, FileLocation = @FileLocation, Status = 1
                           WHERE ReportDownloadId = @ReportDownloadId";

            using (var connection = new MySqlConnection(ConnectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@ReportDownloadId", reportDownloadId);
                command.Parameters.AddWithValue("@GenerateDate", DateTime.Now);
                command.Parameters.AddWithValue("@FileLocation", fileLocation);
                connection.Open();
                command.ExecuteNonQuery();
            }*/
        }

        public void UpdateStatus(int reportDownloadId)
        {
            using (NHibernate.ISession session = NHibernateHelper.OpenSession("ReportDownloads"))
            {
                session.Query<Models.ReportDownloads>()
                    .Where(c => c.ReportDownloadId == reportDownloadId)
                    .UpdateBuilder()
                    .Set(c => c.GenerateDate, DateTime.Now)
                    .Set(c => c.Status, 1)
                    .Update();
            }
            /*string cmdText = @"UPDATE ReportDownloads SET GenerateDate = @GenerateDate, Status = 1
                           WHERE ReportDownloadId = @ReportDownloadId";

            using (var connection = new MySqlConnection(ConnectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@ReportDownloadId", reportDownloadId);
                command.Parameters.AddWithValue("@GenerateDate", DateTime.Now);
                connection.Open();
                command.ExecuteNonQuery();
            }*/
        }
    }
}
