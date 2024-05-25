using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class CustomerOrders {
        private readonly string _connectionString;

        public CustomerOrders(string connectionString)
        {
            _connectionString = connectionString;
        }

		public int GetNtsOrderIdBySystemSerialAndFileName(string systemSerial, string fileName) {
			int ntsOrderId = -1;
			string cmdText = @"SELECT OrderId FROM CustomerOrders WHERE SystemSerial = @SystemSerial AND FileName = @FileName";
			try {
				using (var connection = new MySqlConnection(_connectionString)) {
					var command = new MySqlCommand(cmdText, connection);
					command.CommandTimeout = 0;
					command.Parameters.AddWithValue("@SystemSerial", systemSerial);
					command.Parameters.AddWithValue("@FileName", fileName);

					connection.Open();
					var reader = command.ExecuteReader();
					if (reader.Read()) {
						ntsOrderId = Convert.ToInt32(reader["OrderId"].ToString());
					}
				}
			}
			catch(Exception ex) {
				throw ex;
			}
			return ntsOrderId;
		}

    }
}
