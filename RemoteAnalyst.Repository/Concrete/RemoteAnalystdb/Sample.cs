using System;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb
{
    public class Sample
    {
        private readonly string ConnectionString = "";

        public Sample(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public int GetMaxNSID()
        {
            int nsid = 0;
            string cmdText = "SELECT (MAX(NSID) + 1) AS MaxNSID FROM Sample";
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    connection.Open();

                    var reader = command.ExecuteReader();

                    if (reader.Read() && !reader.IsDBNull(0))
                    {
                        nsid = Convert.ToInt32(reader["MaxNSID"]);
                    }
                    else if (reader.IsDBNull(0))
                    {
                        nsid = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return nsid;
        }

        public void InsertNewEntryPathway(string systemName, int nsid, string UWSPath)
        {
            string cmdText = "INSERT INTO Sample (SampleNode, NSID, DataClass, DataClassName, UWSFile) " +
                             "VALUES (@SampleNode, @NSID, @DataClass, @DataClassName, @UWSFile)";

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@SampleNode", systemName);
                    command.Parameters.AddWithValue("@NSID", nsid);
                    command.Parameters.AddWithValue("@DataClass", 3);
                    command.Parameters.AddWithValue("@DataClassName", "Pv");
                    command.Parameters.AddWithValue("@UWSFile", UWSPath);

                    connection.Open();
                    command.ExecuteReader();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}