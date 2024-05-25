using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.UWSLoader.TableUpdate.Models;

namespace RemoteAnalyst.UWSLoader.TableUpdate {
    public class TableUpdater {

        private readonly string _ConnectionString;

        TableUpdater(string connectionString) {
            _ConnectionString = connectionString;
        }

        public void UpdateAllTables() {
            var databaseMappings = new DatabaseMappings(_ConnectionString);
            DataTable databaseConnectionStrings = databaseMappings.GetAllDatabaseConnection();

            foreach(DataRow databaseConnectionString in databaseConnectionStrings.Rows) {
                UpdateTablePrimaryKeys(databaseConnectionString["MySqlConnectionString"].ToString());
            }
        }

        private void UpdateTablePrimaryKeys(string connectionString) {

            
            TableUpdateInfo tableUpdateInfo = new TableUpdateInfo();
            tableUpdateInfo.TableName = "TransactionProfileTrends";
            tableUpdateInfo.RowUpdate.Add("`FromDateTime` datetime NOT NULL");
            tableUpdateInfo.RowUpdate.Add("`ToDateTime` datetime NOT NULL");
            tableUpdateInfo.PrimaryKeys = "(`ProfileId`, `FromDateTime`, `ToDateTime`)";

        }

        private void UpdateTableRows() {
            try {
                
            } catch (Exception ex) {
                // add a log
            }
        }

        private void AddTablePrimaryKey() {
            try {

            } catch (Exception ex) {
                // add a log
            }
        }
    }
}
