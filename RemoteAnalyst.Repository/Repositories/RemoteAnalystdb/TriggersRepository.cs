using MySqlConnector;
using NHibernate;
using NHibernate.Linq;
using RemoteAnalyst.Repository.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Repositories
{
    public class TriggersRepository
    {
        public void DeleteTriiger(int triggerId)
        {
            using (ISession session = NHibernateHelper.OpenSession("Triggers"))
            {
                session.Query<Models.Triggers>()
                    .Where(c => c.TriggerId == triggerId)
                    .Delete();
            }
            /*    string cmdText = @"DELETE FROM Triggers WHERE TriggerId = @TriggerId";
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@TriggerId", triggerId);
                connection.Open();
                command.ExecuteNonQuery();
            }*/
        }

        public DataTable GetTrigger(int triggerType)
        {
            using (ISession session = NHibernateHelper.OpenSession("Triggers"))
            {
                Models.Triggers c = null;
                var res = session.QueryOver<Models.Triggers>(() => c)
                                        .SelectList(l => l
                                            .Select(s => s.TriggerId)
                                            .Select(s => s.SystemSerial)
                                            .Select(s => s.FileType)
                                            .Select(s => s.FileLocation)
                                            .Select(s => s.UploadId)
                                            .Select(s => s.Message)
                                            .Select(s => s.CustomerId)
                                            .Select(s => s.InsertDate))
                                        .Where(() => c.TriggerType == triggerType).OrderBy(ct=>ct.InsertDate).Asc.List<object[]>();
                return CollectionHelper.ToDataTable(res);
            }
            /*return myDataSet;
            string cmdText = @"SELECT TriggerId, SystemSerial, FileType, FileLocation, UploadId, Message, CustomerId, InsertDate
                            FROM Triggers WHERE TriggerType = @TriggerType ORDER BY InsertDate LIMIT 1";
            var triggerView = new DataTable();
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@TriggerType", triggerType);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(triggerView);
            }

            return triggerView;*/
        }
    }
}
