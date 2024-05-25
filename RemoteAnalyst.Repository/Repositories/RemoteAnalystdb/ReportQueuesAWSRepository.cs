using MySqlConnector;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.SqlCommand;
using RemoteAnalyst.Repository.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Repositories
{
    public class ReportQueuesAWSRepository
    {
        public DataTable GetCurrentQueues(int typeID, string instanceID)
        {
            using (NHibernate.ISession session = NHibernateHelper.OpenSession("ReportQueuesAWS"))
            {
                Models.ReportQueuesAWS c = null;
                var res = session.QueryOver<Models.ReportQueuesAWS>(() => c)
                                        .Where(() => c.Loading == 0 && c.TypeID == typeID && c.InstanceID == instanceID).OrderBy(ct => ct.QueueID).Asc.List<object[]>();
                return CollectionHelper.ToDataTable(res);
            }
            /*string connectionString = _connectionString;
            string cmdText = "SELECT * FROM ReportQueuesAWS WHERE Loading = 0 AND TypeID = @TypeID AND InstanceID = @InstanceID ORDER BY QueueID LIMIT 1";
            var entityOrders = new DataTable();

            using (var connection = new MySqlConnection(connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@TypeID", typeID);
                command.Parameters.AddWithValue("@InstanceID", instanceID);

                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(entityOrders);
            }

            return entityOrders;*/
        }

        public void UpdateOrders(int queueID)
        {
            using (NHibernate.ISession session = NHibernateHelper.OpenSession("ReportQueuesAWS"))
            {
                Models.ReportQueuesAWS rq = session.Get<Models.ReportQueuesAWS>(queueID);
                if (rq != null)
                {
                    rq.Loading = 1;
                    session.Save(rq, queueID);
                }
            }
            /*    string connectionString = _connectionString;
            string cmdText = "UPDATE ReportQueuesAWS SET Loading = 1 WHERE QueueID = @QueueID";

            using (var connection = new MySqlConnection(connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@QueueID", queueID);
                connection.Open();

                command.ExecuteNonQuery();
            }*/
        }

        public int GetProcessingOrder(int typeID, string instanceID)
        {
            using (NHibernate.ISession session = NHibernateHelper.OpenSession("ReportQueuesAWS"))
            {
                Models.ReportQueuesAWS c = null;
                var res = session
                            .CreateCriteria(typeof(Models.ReportQueuesAWS))
                            .Add(Restrictions.Eq("Loading", 1))
                            .Add(Restrictions.Eq("TypeID", typeID))
                            .Add(Restrictions.Eq("InstanceID", instanceID))
                            .SetProjection(Projections.ProjectionList()
                                .Add(Projections.Alias(Projections.Count("QueueID"), "ProcessCount"))
                            )
                            .List<Models.ReportQueuesAWS>();
                return res[0].QueueID;
            }
            /*string connectionString = _connectionString;
            string cmdText = "SELECT COUNT(QueueID) AS ProcessCount FROM ReportQueuesAWS WHERE Loading = 1 AND TypeID = @TypeID AND InstanceID = @InstanceID";
            int processingEntity = 0;

            using (var connection = new MySqlConnection(connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@TypeID", typeID);
                command.Parameters.AddWithValue("@InstanceID", instanceID);
                connection.Open();

                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    processingEntity = Convert.ToInt32(reader["ProcessCount"]);
                }
            }

            return processingEntity;*/
        }

        public void InsertNewQueue(string message, int typeID, string instanceID)
        {
            using (NHibernate.ISession session = NHibernateHelper.OpenSession("ReportQueuesAWS"))
            {
                Models.ReportQueuesAWS rq = new Models.ReportQueuesAWS();
                rq.Message = message;
                rq.TypeID = typeID;
                rq.InstanceID = instanceID;
                rq.Loading = 0;
                rq.OrderDate = DateTime.Now;
                session.Save(rq);
            }
            /*string ntsConnectionString = _connectionString;
            string cmdText = @"INSERT INTO ReportQueuesAWS (Message ,TypeID ,Loading, OrderDate, InstanceID)
                           VALUES (@Message ,@TypeID ,@Loading, @OrderDate, @InstanceID)";

            using (var connection = new MySqlConnection(ntsConnectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@Message", message);
                command.Parameters.AddWithValue("@TypeID", typeID);
                command.Parameters.AddWithValue("@Loading", 0);
                command.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                command.Parameters.AddWithValue("@InstanceID", instanceID);
                connection.Open();
                command.ExecuteNonQuery();
            }*/
        }

        public int InsertNewQueueReturnQueueId(string message, int typeID, string instanceID)
        {
            using (NHibernate.ISession session = NHibernateHelper.OpenSession("ReportQueuesAWS"))
            {
                Models.ReportQueuesAWS rq = new Models.ReportQueuesAWS();
                rq.Message = message;
                rq.TypeID = typeID;
                rq.InstanceID = instanceID;
                rq.Loading = 0;
                rq.OrderDate = DateTime.Now;
                var id = session.Save(rq);
                return Convert.ToInt32(id);
            }
            /*string ntsConnectionString = _connectionString;
            string cmdText = @"INSERT INTO ReportQueuesAWS (Message ,TypeID ,Loading, OrderDate, InstanceID)
                           VALUES (@Message ,@TypeID ,@Loading, @OrderDate, @InstanceID);SELECT LAST_INSERT_ID()";
            int queueID;
            using (var connection = new MySqlConnection(ntsConnectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@Message", message);
                command.Parameters.AddWithValue("@TypeID", typeID);
                command.Parameters.AddWithValue("@Loading", 0);
                command.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                command.Parameters.AddWithValue("@InstanceID", instanceID);
                connection.Open();
                queueID = Convert.ToInt32(command.ExecuteScalar());
            }
            return queueID;*/
        }

        public void DeleteEntry(string instanceID)
        {
            using (NHibernate.ISession session = NHibernateHelper.OpenSession("ReportQueuesAWS"))
            {
                session.Query<Models.ReportQueuesAWS>()
                    .Where(c => c.InstanceID == instanceID)
                    .Delete();
                /*Models.ReportQueuesAWS rq = session.Get<Models.ReportQueuesAWS>(instanceID);
                if (rq != null)
                {
                    session.Delete(rq, instanceID);
                }*/
            }/*
            string cmdText = @"DELETE FROM ReportQueuesAWS WHERE InstanceID = @InstanceID";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@InstanceID", instanceID);
                connection.Open();
                command.ExecuteNonQuery();
            }*/
        }

        public int GetCurrentCount(int typeID, string instanceID)
        {
            using (NHibernate.ISession session = NHibernateHelper.OpenSession("ReportQueuesAWS"))
            {
                Models.ReportQueuesAWS c = null;
                var res = session
                            .CreateCriteria(typeof(Models.ReportQueuesAWS))
                            .Add(Restrictions.Eq("TypeID", typeID))
                            .Add(Restrictions.Eq("InstanceID", instanceID))
                            .SetProjection(Projections.ProjectionList()
                                .Add(Projections.Alias(Projections.Count("QueueID"), "ProcessCount"))
                            )
                            .List<Models.ReportQueuesAWS>();
                return res[0].QueueID;
            }
            /*string connectionString = _connectionString;
            string cmdText = "SELECT COUNT(QueueID) AS ProcessCount FROM ReportQueuesAWS WHERE TypeID = @TypeID AND InstanceID = @InstanceID";
            int processingEntity = 0;

            using (var connection = new MySqlConnection(connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@TypeID", typeID);
                command.Parameters.AddWithValue("@InstanceID", instanceID);
                connection.Open();

                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    processingEntity = Convert.ToInt32(reader["ProcessCount"]);
                }
            }

            return processingEntity;*/
        }

        public void RemoveQueue(int queueID)
        {
            using (NHibernate.ISession session = NHibernateHelper.OpenSession("ReportQueuesAWS"))
            {
                session.Query<Models.ReportQueuesAWS>()
                    .Where(c => c.QueueID == queueID)
                    .Delete();
            }
            /*string connectionString = _connectionString;
            string cmdText = "DELETE FROM ReportQueuesAWS WHERE QueueID = @QueueID";

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
