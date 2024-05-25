using System;
using System.Collections.Generic;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM
{
    public class RecepientList
    {
        private readonly string _connectionString = "";

        public RecepientList(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IList<string> GetEmailList(int deliveryID)
        {
            string cmdText = @"SELECT RL_EmailAddress FROM RecepientList WHERE RL_DeliveryID = @DeliveryID";

            IList<string> emailList = new List<string>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@DeliveryID", deliveryID);
                connection.Open();

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (!emailList.Contains(Convert.ToString(reader["RL_EmailAddress"])))
                        emailList.Add(Convert.ToString(reader["RL_EmailAddress"]));
                }
            }

            return emailList;
        }
    }
}