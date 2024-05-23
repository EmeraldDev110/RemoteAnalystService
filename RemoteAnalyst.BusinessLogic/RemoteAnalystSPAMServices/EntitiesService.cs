using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class EntitiesService
    {
        private readonly string _connectionString = "";

        public EntitiesService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int GetEntityIDFor(string entityName)
        {
            var entities = new EntityRepository(_connectionString);
            int retVal = entities.GetEntityID(entityName);
            return retVal;
        }
    
    }
}