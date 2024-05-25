using MySqlConnector;
using NHibernate.Criterion;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Repositories
{
    public class ReportQueuesRepository
    {
        public void InsertNewQueue(string fileName, int typeID)
        {
            using (NHibernate.ISession session = NHibernateHelper.OpenSession("ReportQueues"))
            {
                Models.ReportQueues rq = new Models.ReportQueues();
                rq.FileName = fileName;
                rq.TypeID = typeID;
                rq.Loading = 0;
                rq.OrderDate = DateTime.Now;
                session.Save(rq);
            }
            /*string ntsConnectionString = _connectionString;
            string cmdText = @"INSERT INTO ReportQueues (FileName ,TypeID ,Loading, OrderDate)
                           VALUES (@FileName ,@TypeID ,@Loading, @OrderDate)";

            using (var connection = new MySqlConnection(ntsConnectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@FileName", fileName);
                command.Parameters.AddWithValue("@TypeID", typeID);
                command.Parameters.AddWithValue("@Loading", 0);
                command.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                connection.Open();
                command.ExecuteNonQuery();
            }*/
        }

        public int GetProcessingOrder(int typeID)
        {
            using (NHibernate.ISession session = NHibernateHelper.OpenSession("ReportQueues"))
            {
                Models.ReportQueues c = null;
                var res = session
                            .CreateCriteria(typeof(Models.ReportQueues))
                            .Add(Restrictions.Eq("TypeID", typeID))
                            .Add(Restrictions.Eq("Loading", 1))
                            .SetProjection(Projections.ProjectionList()
                                .Add(Projections.Alias(Projections.Count("QueueID"), "ProcessCount"))
                            )
                            .List<int>();
                return res[0];
            }
            /*string connectionString = _connectionString;
            string cmdText = "SELECT COUNT(QueueID) AS ProcessCount FROM ReportQueues WHERE Loading = 1 AND TypeID = @TypeID";
            int processingEntity = 0;

            using (var connection = new MySqlConnection(connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@TypeID", typeID);
                connection.Open();

                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    processingEntity = Convert.ToInt32(reader["ProcessCount"]);
                }
            }

            return processingEntity;*/
        }

        public void UpdateOrders(int queueID)
        {
            using (NHibernate.ISession session = NHibernateHelper.OpenSession("ReportQueues"))
            {
                Models.ReportQueues rq = session.Get<Models.ReportQueues>(queueID);
                if (rq != null)
                {
                    rq.Loading = 1;
                    session.Save(rq, queueID);
                }
            }
            /*string connectionString = _connectionString;
            string cmdText = "UPDATE ReportQueues SET Loading = 1 WHERE QueueID = @QueueID";

            using (var connection = new MySqlConnection(connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@QueueID", queueID);
                connection.Open();

                command.ExecuteNonQuery();
            }*/
        }

        public void RemoveQueue(int queueID)
        {
            using (NHibernate.ISession session = NHibernateHelper.OpenSession("ReportQueues"))
            {
                session.Query<Models.ReportQueues>()
                    .Where(c => c.QueueID == queueID)
                    .Delete();
            }
            /*string connectionString = _connectionString;
            string cmdText = "DELETE FROM ReportQueues WHERE QueueID = @QueueID";

            using (var connection = new MySqlConnection(connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@QueueID", queueID);
                connection.Open();

                command.ExecuteNonQuery();
            }*/
        }
    }
}
