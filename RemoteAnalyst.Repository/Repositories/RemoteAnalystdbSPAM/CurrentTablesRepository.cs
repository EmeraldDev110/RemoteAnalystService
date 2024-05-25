using MySqlConnector;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Repositories
{
    public class CurrentTablesRepository
    {
        private readonly string ConnectionString;

        public CurrentTablesRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }
        public long GetInterval(string buildTableName)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(ConnectionString, "CurrentTables"))
            {
                Models.CurrentTables c = null;
                var res = session.QueryOver<Models.CurrentTables>(() => c)
                                        .Select(ct => ct.Interval)
                                        .Where(() => c.TableName == buildTableName).List<int>();
                return res != null? res[0] : 0;
            }
            //string connectionString = Config.ConnectionString;
            /*string cmdText = "SELECT CurrentTables.Interval FROM CurrentTables WHERE TableName = @TableName";
            long currentInterval = 0;

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@TableName", buildTableName);

                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        currentInterval = Convert.ToInt64(reader["Interval"]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return currentInterval;*/
        }

        public string GetLatestCPUTable()
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(ConnectionString, "CurrentTables"))
            {
                Models.CurrentTables c = null;
                var res = session.QueryOver<Models.CurrentTables>(() => c)
                                        .Select(ct => ct.TableName)
                                        .Where(() => c.EntityID == 1).OrderBy(ct => ct.DataDate).Asc.List<string>();
                return res != null ? res[0] : "";
            }
           /* var cmdText = @"SELECT TableName FROM CurrentTables WHERE EntityID = 1 ORDER BY DataDate DESC LIMIT 1";
            var cpuTableName = "";

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);

                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        cpuTableName = reader["TableName"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return cpuTableName;*/
        }

        public DateTime GetLatestCPUDate()
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(ConnectionString, "CurrentTables"))
            {
                Models.CurrentTables c = null;
                var res = session.QueryOver<Models.CurrentTables>(() => c)
                                        .Select(ct => ct.DataDate)
                                        .Where(() => c.EntityID == 1).OrderBy(ct => ct.DataDate).Asc.List<DateTime>();
                return res != null ? res[0] : new DateTime();
            }
            /*var cmdText = @"SELECT DataDate FROM CurrentTables WHERE EntityID = 1 ORDER BY DataDate DESC LIMIT 1";
            var latestDate = new DateTime();

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);

                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        latestDate = Convert.ToDateTime(reader["DataDate"]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return latestDate;*/
        }
    }
}
