using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteAnalyst.Repository.Models;

namespace RemoteAnalyst.Repository.Repositories
{
    public class DataTablesRepository
    {
        public bool CheckDatabase(string connectionString, string table)
        {
            string cmdText = "SELECT * FROM ZmsBladeDataDictionary LIMIT 1";
            bool exists = false;

            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(connectionString, table))
                {
                    var res = session
                        .CreateCriteria(typeof(Models.System))
                        .List<Models.System>();
                    if (res != null)
                    {
                        exists = true;
                    }
                }
            }
            catch (Exception ex)
            {
                exists = false;
            }

            return exists;
        }
    }
}
