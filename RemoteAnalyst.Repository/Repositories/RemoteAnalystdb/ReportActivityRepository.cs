using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Repositories
{
    public class ReportActivityRepository
    {
        public void InsertNewEntry(string email, string systemSerial, string reportType, string reportName,
            DateTime from, DateTime to)
        {
            
            string cmdText = "";

            if (reportType != "Storage")
            {
                using (NHibernate.ISession session = NHibernateHelper.OpenSession("ReportActivity"))
                {
                    Models.ReportActivity rq = new Models.ReportActivity();
                    rq.Date = DateTime.Now;
                    rq.Email = email;
                    rq.SystemSerial = systemSerial;
                    rq.ReportType = reportType;
                    rq.ReportName = reportName;
                    rq.PeriodFrom = from;
                    rq.PeriodTo = to;
                    var id = session.Save(rq);
                }
                /*cmdText = "INSERT INTO ReportActivity (Date, Email, SystemSerial, " +
                          "ReportType, ReportName, PeriodFrom, PeriodTo) VALUES " +
                          "(@Date, @Email, @SystemSerial, @ReportType, " +
                          "@ReportName, @PeriodFrom, @PeriodTo)";*/
            }
            else
            {
                using (NHibernate.ISession session = NHibernateHelper.OpenSession("ReportActivity"))
                {
                    Models.ReportActivity rq = new Models.ReportActivity();
                    rq.Date = DateTime.Now;
                    rq.Email = email;
                    rq.SystemSerial = systemSerial;
                    rq.ReportType = reportType;
                    rq.ReportName = reportName;
                    var id = session.Save(rq);
                }
                /*cmdText = "INSERT INTO ReportActivity (Date, Email, SystemSerial, " +
                          "ReportType, ReportName) VALUES " +
                          "(@Date, @Email, @SystemSerial, @ReportType, " +
                          "@ReportName)";*/
            }

            /*using (var connection = new MySqlConnection(ConnectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@Date", DateTime.Now);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@ReportType", reportType);
                command.Parameters.AddWithValue("@ReportName", reportName);
                if (reportType != "Storage")
                {
                    command.Parameters.AddWithValue("@PeriodFrom", from);
                    command.Parameters.AddWithValue("@PeriodTo", to);
                }
                connection.Open();
                command.ExecuteNonQuery();
            }*/
        }
    }
}
