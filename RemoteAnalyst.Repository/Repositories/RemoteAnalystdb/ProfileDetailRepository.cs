using Google.Protobuf.WellKnownTypes;
using MySqlConnector;
using NHibernate;
using NHibernate.Criterion;
using Org.BouncyCastle.Math.EC.Rfc7748;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Repositories
{
    public class ProfileDetailRepository
    {
        public IDictionary<int, string> GetProfileDetail(int reportID)
        {
            using (ISession session = NHibernateHelper.OpenSession("ProfileDetail"))
            {
                Models.ProfileDetail c = null;
                var res = session
                    .CreateCriteria(typeof(Models.ProfileDetail))
                    .Add(Restrictions.Eq("RecordID", reportID))
                    .List<Models.ProfileDetail>();
                IDictionary<int, string> profile = res.ToDictionary(x => x.RecordID, x => x.ProfileEntity);
                return profile;
            }
            /*string cmdText = @"SELECT RecordID, ProfileEntity FROM ProfileDetail WHERE RecordID =  @RecordID";

            IDictionary<int, string> profileData = new Dictionary<int, string>();

            using (var connection = new MySqlConnection(ConnectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@RecordID", reportID);
                connection.Open();

                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (!profileData.ContainsKey(Convert.ToInt32(reader["RecordID"])))
                        profileData.Add(Convert.ToInt32(reader["RecordID"]), Convert.ToString(reader["ProfileEntity"]));
                }
            }

            return profileData;*/
        }
    }
}
