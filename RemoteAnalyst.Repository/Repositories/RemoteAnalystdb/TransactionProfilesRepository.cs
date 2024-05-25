using MySqlConnector;
using NHibernate;
using NHibernate.Criterion;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NHibernate.Engine.Query.CallableParser;

namespace RemoteAnalyst.Repository.Repositories
{
    public class TransactionProfilesRepository
    {
        public bool GetCPUtoFile(int transactionProfileId)
        {
            using (ISession session = NHibernateHelper.OpenSession("TransactionProfiles"))
            {
                Models.TransactionProfiles res = session
                .CreateCriteria(typeof(Models.TransactionProfiles))
                    .Add(Restrictions.Eq("TransactionProfileId", transactionProfileId))
                    .UniqueResult<Models.TransactionProfiles>();
                return Convert.ToBoolean(res?.IsCpuToFile);
            }
            /*string cmdText = "SELECT IsCpuToFile FROM TransactionProfiles WHERE TransactionProfileId = @TransactionProfileId";
            bool isCpuToFile = true;

            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@TransactionProfileId", transactionProfileId);
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    if (reader["IsCpuToFile"] != DBNull.Value)
                    {
                        isCpuToFile = Convert.ToBoolean(reader["IsCpuToFile"]);
                    }
                }
                reader.Close();
            }
            return isCpuToFile;*/
        }

        public string GetProfileName(int transactionProfileId)
        {
            using (ISession session = NHibernateHelper.OpenSession("TransactionProfiles"))
            {
                Models.TransactionProfiles res = session
                .CreateCriteria(typeof(Models.TransactionProfiles))
                    .Add(Restrictions.Eq("TransactionProfileId", transactionProfileId))
                    .UniqueResult<Models.TransactionProfiles>();
                return res?.TransactionProfileName;
            }
            /*string cmdText = "SELECT TransactionProfileName FROM TransactionProfiles WHERE TransactionProfileId = @TransactionProfileId";
            var profileName = "";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@TransactionProfileId", transactionProfileId);
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    if (reader["TransactionProfileName"] != DBNull.Value)
                    {
                        profileName = Convert.ToString(reader["TransactionProfileName"]);
                    }
                }
            }
            return profileName;*/
        }
    }
}
