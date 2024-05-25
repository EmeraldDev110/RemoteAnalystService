using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.UWSLoader.Core.ModelView;
using RemoteAnalyst.UWSLoader.Core.Repository;

namespace RemoteAnalyst.UWSLoader.Core.BusinessLogic {
    class EntitiesService {
        private readonly string _connectionString = "";

        public EntitiesService(string connectionString) {
            _connectionString = connectionString;
        }

        public int GetEntityIDFor(string entityName) {
            var entities = new Entities(_connectionString);
            int retVal = entities.GetEntityID(entityName);
            return retVal;
        }

        public string CreateEntityTable(string buildTableName, IList<ColumnInfoView> columnInfo, bool bolCreateIdentityColumn, bool website, string connectionStringDetail) {
            var entities = new Entities(connectionStringDetail);
            IList<string> columnNameList = new List<string>();
            IList<string> typeNameList = new List<string>();
            IList<int> typeValueList = new List<int>();

            foreach (var columnInfoView in columnInfo) {
                if (website && columnInfoView.Website.Equals(false)) continue;

                columnNameList.Add(columnInfoView.ColumnName);
                typeNameList.Add(columnInfoView.TypeName);
                typeValueList.Add(columnInfoView.TypeValue);
            }
            string retVal = entities.CreateEntityTable(buildTableName, columnNameList, typeNameList, typeValueList,
                bolCreateIdentityColumn);
            return retVal;
        }
    }
}
