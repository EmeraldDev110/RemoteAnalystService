using System;
using System.Collections.Generic;
using MySqlConnector;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.Core.Repository {
    class Entities {
        private readonly string ConnectionString = "";

        public Entities(string connectionString) {
            ConnectionString = connectionString;
        }

        public int GetEntityID(string entityName) {
            //string connectionString = Config.ConnectionString;
            string cmdText = "SELECT EntityID FROM Entities WHERE EntityName = @EntityName";
            int entityID = 0;

            try {
                using (var connection = new MySqlConnection(ConnectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@EntityName", entityName);
                    connection.Open();

                    var reader = command.ExecuteReader();

                    if (reader.Read()) {
                        entityID = Convert.ToInt32(reader["EntityID"]);
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }

            return entityID;
        }

        public string CreateEntityTable(string buildTableName, IList<string> columnNameList, IList<string> typeNameList,
            IList<int> typeValueList, bool bolCreateIdentityColumn) {
            var buildColumns = new StringBuilder();
            string cmdText = string.Empty;
            int listLength = columnNameList.Count;

            //Build Column.
            if (bolCreateIdentityColumn) {
                buildColumns.Append("[EntityCounterID] [int] IDENTITY (1, 1) NOT NULL ,");
            }
            for (int i = 0; i < listLength; i++) {
                if (typeNameList[i].ToUpper().Contains("TINYINT")) {
                    buildColumns.Append(columnNameList[i] + " ");
                    buildColumns.Append(typeNameList[i].Trim() + ",");
                }
                else if (typeNameList[i].ToUpper().Contains("NVARCHAR")) {
                    buildColumns.Append(columnNameList[i] + " ");
                    buildColumns.Append(typeNameList[i].Trim() + "(" + typeValueList[i] + "),");
                }
                else {
                    buildColumns.Append(columnNameList[i] + " ");
                    buildColumns.Append(typeNameList[i].Trim() + ",");
                }
            }

            //At the end, take out the last ',' from the string builder.
            buildColumns.Remove(buildColumns.Length - 1, 1);

            //string connectionString = Config.ConnectionString;
            if (bolCreateIdentityColumn) {
                cmdText = "IF NOT EXISTS " +
                          "(SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[" + buildTableName + "]') " +
                          "AND OBJECTPROPERTY(id, N'IsUserTable') = 1)" +
                          "CREATE TABLE [" + buildTableName + "] " +
                          "(" + buildColumns + ") " +
                          " " +
                          "IF NOT EXISTS " +
                          "(SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[" + buildTableName + "]') " +
                          "AND OBJECTPROPERTY(id, N'IsUserTable') = 1)" +
                          "ALTER TABLE [" + buildTableName + "] ADD " +
                          "CONSTRAINT [PK_" + buildTableName + "] PRIMARY KEY CLUSTERED " +
                          "([EntityCounterID]) ";
            }
            else {
                cmdText = "IF NOT EXISTS " +
                          "(SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[" + buildTableName + "]') " +
                          "AND OBJECTPROPERTY(id, N'IsUserTable') = 1)" +
                          "CREATE TABLE [" + buildTableName + "] " +
                          "(" + buildColumns + ") ";
            }

            try {
                using (var connection = new MySqlConnection(ConnectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    connection.Open();

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }

            //Create index.
            if (buildTableName.Contains("PROCESS")) {
                #region Code
                string processCmdText = @"CREATE NONCLUSTERED INDEX [TOP_Processes]
                                            ON [dbo].[" + buildTableName + @"] ([CpuNum],[FromTimestamp])
                                            INCLUDE ([ToTimestamp],[DeltaTime],[CpuBusyTime],[PresPagesQTime],[Pin],
                                            [Priority],[Group],[User],[ProcessName],[Volume],[SubVol],[FileName])
                                            WITH FILLFACTOR = 90";
                try {
                    using (var connection = new MySqlConnection(ConnectionString)) {
                        var command = new MySqlCommand(processCmdText, connection);
                        connection.Open();
                        command.CommandTimeout = 0;
                        command.ExecuteNonQuery();
                    }
                }
                catch {
                }

                processCmdText = @"CREATE NONCLUSTERED INDEX [GetProcessInfo]
                                    ON [dbo].[" + buildTableName + @"] ([CpuNum],[Pin],[ProcessName],[FromTimestamp])
                                    INCLUDE ([DeltaTime],[CpuBusyTime],[Dispatches],[PageFaults],[PresPagesQTime],
                                    [MessagesSent],[MessagesReceived],[Priority],[Volume],[SubVol],[FileName],
                                    [AncestorCpu],[AncestorPin],[AncestorProcessName]) WITH FILLFACTOR = 90";
                try {
                    using (var connection = new MySqlConnection(ConnectionString)) {
                        var command = new MySqlCommand(processCmdText, connection);
                        connection.Open();
                        command.CommandTimeout = 0;
                        command.ExecuteNonQuery();
                    }
                }
                catch {
                }

                processCmdText = @"CREATE NONCLUSTERED INDEX [GetProcessTrend]
                                        ON [dbo].[" + buildTableName + @"] ([CpuNum],[Pin],[ProcessName])
                                        INCLUDE ([FromTimestamp],[DeltaTime],[CpuBusyTime],[PresPagesQTime]) WITH FILLFACTOR = 90";
                try {
                    using (var connection = new MySqlConnection(ConnectionString)) {
                        var command = new MySqlCommand(processCmdText, connection);
                        connection.Open();
                        command.CommandTimeout = 0;
                        command.ExecuteNonQuery();
                    }
                }
                catch {
                }

                
                processCmdText = "CREATE  INDEX [IDX_Application_UserGroup] ON " +
                                      "[dbo].[" + buildTableName + "]([Group], [User], " +
                                      "[FromTimestamp], [ToTimestamp]) WITH FILLFACTOR = 90";
                try {
                    using (var connection = new MySqlConnection(ConnectionString)) {
                        var command = new MySqlCommand(processCmdText, connection);
                        connection.Open();
                        command.CommandTimeout = 0;

                        command.ExecuteNonQuery();
                    }
                }
                catch {
                }
                #endregion
            }

            if (buildTableName.Contains("FILE")) {
                #region Codex
                string fileCmdText = "CREATE  INDEX [IX_" + buildTableName + "_2] ON " +
                                      "[dbo].[" + buildTableName + "]([OpenerCpu], [OpenerPin], " +
                                      "[OpenerProcessName], [OpenerVolume], [OpenerSubVol], " +
                                      "[OpenerFileName], [OpenerDeviceName], [OpenerOsspid], " +
                                      "[OpenerPathID], [OpenerCrvsn]) WITH FILLFACTOR = 90";
                try {
                    using (var connection = new MySqlConnection(ConnectionString)) {
                        var command = new MySqlCommand(fileCmdText, connection);
                        connection.Open();
                        command.CommandTimeout = 0;

                        command.ExecuteNonQuery();
                    }
                }
                catch {
                }

                fileCmdText = @"CREATE NONCLUSTERED INDEX [GetFileInfo]
                                      ON [dbo].[" + buildTableName +
                                      @"] ([OpenerCpu],[OpenerPin],[OpenerProcessName],[FromTimestamp])
                                      INCLUDE ([DeltaTime],[Reads],[Writes],[UpdatesOrReplies],[DeletesOrWriteReads],[Volume],[SubVol],
                                      [FileName],[OpenerVolume],[OpenerSubVol],[OpenerFileName]) WITH FILLFACTOR = 90";
                try {
                    using (var connection = new MySqlConnection(ConnectionString)) {
                        var command = new MySqlCommand(fileCmdText, connection);
                        connection.Open();
                        command.CommandTimeout = 0;

                        command.ExecuteNonQuery();
                    }
                }
                catch {
                }
                fileCmdText = @"CREATE NONCLUSTERED INDEX [DataBrowser_FILE]
                                        ON [dbo].[" + buildTableName +
                                      @"] ([Volume],[SubVol],[FileName], [FromTimestamp])
                                        INCLUDE ([OpenerCpu],[DeltaTime],[Reads],[Writes],[UpdatesOrReplies],
                                        [DeletesOrWriteReads],[RecordsUsed],[RecordsAccessed],[LockWaits],
                                        [OpenerProcessName],[OpenerVolume],[OpenerSubVol],[OpenerFileName]) WITH FILLFACTOR = 90";
                try {
                    using (var connection = new MySqlConnection(ConnectionString)) {
                        var command = new MySqlCommand(fileCmdText, connection);
                        connection.Open();
                        command.CommandTimeout = 0;

                        command.ExecuteNonQuery();
                    }
                }
                catch {
                }

                fileCmdText = @"CREATE NONCLUSTERED INDEX [DPA_IREPORT21]
                                        ON [dbo].[" + buildTableName +
                                      @"] ([DeviceType])
                                        INCLUDE ([FileBusyTime],[Volume],[OpenerVolume],[OpenerSubVol],[OpenerFileName]) WITH FILLFACTOR = 90";
                try {
                    using (var connection = new MySqlConnection(ConnectionString)) {
                        var command = new MySqlCommand(fileCmdText, connection);
                        connection.Open();
                        command.CommandTimeout = 0;

                        command.ExecuteNonQuery();
                    }
                }
                catch {
                }

                fileCmdText = @"CREATE NONCLUSTERED INDEX [FILE_Scanner]
                                        ON [dbo].[" + buildTableName +
                                      @"] ([FromTimestamp], [Volume],[SubVol],[FileName]) WITH FILLFACTOR = 90";
                try {
                    using (var connection = new MySqlConnection(ConnectionString)) {
                        var command = new MySqlCommand(fileCmdText, connection);
                        connection.Open();
                        command.CommandTimeout = 0;

                        command.ExecuteNonQuery();
                    }
                }
                catch {
                }

                fileCmdText = "CREATE  INDEX [IDX_Application_Opener] ON " +
                                      "[dbo].[" + buildTableName + "]([OpenerCpu], [OpenerPin], " +
                                      "[OpenerProcessName], [OpenerVolume], [OpenerSubVol], " +
                                      "[OpenerFileName], [FromTimestamp], [ToTimestamp]) WITH FILLFACTOR = 90";
                try {
                    using (var connection = new MySqlConnection(ConnectionString)) {
                        var command = new MySqlCommand(fileCmdText, connection);
                        connection.Open();
                        command.CommandTimeout = 0;

                        command.ExecuteNonQuery();
                    }
                }
                catch {
                }
                #endregion
            }

            if (buildTableName.Contains("DISCOPE")) {
                #region Code

                /*string discopeCmdText1 = "CREATE  INDEX [IX_" + buildTableName + "_1] ON " +
                                         "[dbo].[" + buildTableName + "]([Volume], [SubVol], " +
                                         "[FileName]) ";
                try {
                    using (SqlConnection connection = new MySqlConnection(ConnectionString)) {
                        SqlCommand command = new MySqlCommand(discopeCmdText1, connection);
                        connection.Open();
                        command.CommandTimeout = 0;

                        command.ExecuteNonQuery();
                    }

                }
                catch {
                }
                string discopeCmdText2 = "CREATE  INDEX [IX_" + buildTableName + "_2] ON " +
                                         "[dbo].[" + buildTableName + "]([SystemName]) ";
                try {
                    using (SqlConnection connection = new MySqlConnection(ConnectionString)) {
                        SqlCommand command = new MySqlCommand(discopeCmdText2, connection);
                        connection.Open();
                        command.CommandTimeout = 0;

                        command.ExecuteNonQuery();
                    }

                }
                catch {
                }
                string discopeCmdText3 = "CREATE  INDEX [IX_" + buildTableName + "_3] ON " +
                                         "[dbo].[" + buildTableName + "]([DeviceName]) ";
                try {
                    using (SqlConnection connection = new MySqlConnection(ConnectionString)) {
                        SqlCommand command = new MySqlCommand(discopeCmdText3, connection);
                        connection.Open();
                        command.CommandTimeout = 0;

                        command.ExecuteNonQuery();
                    }

                }
                catch {
                }
                string discopeCmdText4 = "CREATE  INDEX [IX_" + buildTableName + "_4] ON " +
                                         "[dbo].[" + buildTableName + "]([Volume]) ";
                try {
                    using (SqlConnection connection = new MySqlConnection(ConnectionString)) {
                        SqlCommand command = new MySqlCommand(discopeCmdText4, connection);
                        connection.Open();
                        command.CommandTimeout = 0;

                        command.ExecuteNonQuery();
                    }

                }
                catch {
                }
                string discopeCmdText5 = "CREATE  INDEX [IX_" + buildTableName + "_5] ON " +
                                         "[dbo].[" + buildTableName + "]([OpenerCpu], [OpenerPin]) ";
                try {
                    using (SqlConnection connection = new MySqlConnection(ConnectionString)) {
                        SqlCommand command = new MySqlCommand(discopeCmdText5, connection);
                        connection.Open();
                        command.CommandTimeout = 0;

                        command.ExecuteNonQuery();
                    }
                }
                catch {
                }*/

                #endregion
            }

            if (buildTableName.Contains("DISC")) {
                #region Code

                string discCmdText1 = @"CREATE NONCLUSTERED INDEX [GetApplicationProcesses-Helper]
                                        ON [dbo].[" + buildTableName + @"]  ([DeviceName])  WITH FILLFACTOR = 90";
                try {
                    using (var connection = new MySqlConnection(ConnectionString)) {
                        var command = new MySqlCommand(discCmdText1, connection);
                        connection.Open();
                        command.CommandTimeout = 0;
                        command.ExecuteNonQuery();
                    }
                }
                catch {
                }
                string discCmdText2 = @"CREATE NONCLUSTERED INDEX [GetDP2Processes_Helper]
                                        ON [dbo].[" + buildTableName + @"] ([CpuNum],[FromTimestamp])
                                        INCLUDE ([DeltaTime],[RequestQTime],[Reads],[Writes],[Hits1],
                                        [Misses1],[Hits2],[Misses2],[Hits3],[Misses3],[Hits4],[Misses4],[DeviceName]) WITH FILLFACTOR = 90";
                try {
                    using (var connection = new MySqlConnection(ConnectionString)) {
                        var command = new MySqlCommand(discCmdText2, connection);
                        connection.Open();
                        command.CommandTimeout = 0;
                        command.ExecuteNonQuery();
                    }
                }
                catch {
                }
                string discCmdText3 = @"CREATE NONCLUSTERED INDEX [DISCInfo]
                                        ON [dbo].[074485_DISC_2013_7_17] ([CpuNum],[FromTimestamp])
                                        INCLUDE ([DeltaTime],[Requests],[Reads],[Writes]) WITH FILLFACTOR = 90";
                try {
                    using (var connection = new MySqlConnection(ConnectionString)) {
                        var command = new MySqlCommand(discCmdText3, connection);
                        connection.Open();
                        command.CommandTimeout = 0;
                        command.ExecuteNonQuery();
                    }
                }
                catch {
                }

                #endregion
            }

            return buildTableName;
        }
    }
}
