using System;
using RemoteAnalyst.Repository.Models;
using NHibernate;

namespace RemoteAnalyst.Repository.Repositories
{
    public class EntityRepository
    {
        private string _connectionString = "";

        public EntityRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int GetEntityID(string entityName)
        {
            int entityID = 0;

            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "Entity"))
                {
                    int res = session.QueryOver<Entity>()
                        .Select(x => x.EntityID)
                        .Where(x => x.EntityName == entityName)
                        .SingleOrDefault<int>();
                    entityID = res;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return entityID;
        }
    }
}
