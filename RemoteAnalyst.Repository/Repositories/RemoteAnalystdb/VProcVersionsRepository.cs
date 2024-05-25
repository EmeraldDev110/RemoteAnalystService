using MySqlConnector;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Repositories
{
    public class VProcVersionsRepository
    {
        public string GetVprocVersion(string vProcVersion)
        {
            string dataDictionary = "";
                using (ISession session = NHibernateHelper.OpenSession("VProcVersions"))
                {
                    Models.VProcVersions res = session
                        .CreateCriteria(typeof(Models.VProcVersions))
                        .Add(Restrictions.Eq("VPROCVersion", vProcVersion))
                        .UniqueResult<Models.VProcVersions>();
                    return res.VPROCVersion;
                }
            
            /*string cmdText = "SELECT VPROCVersion FROM VProcVersions " +
                             "WHERE VPROCVersion = @VPROCVersion";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@VPROCVersion", vProcVersion);
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                    dataDictionary = Convert.ToString(reader["VPROCVersion"]);
            }

            return dataDictionary;*/
        }
    }
}
