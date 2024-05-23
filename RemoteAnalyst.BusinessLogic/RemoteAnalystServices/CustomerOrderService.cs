using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class CustomerOrderService {
        private readonly string _connectionString;

        public CustomerOrderService(string connectionString) {
            _connectionString = connectionString;
        }
       
		public int GetUploadIDBySystemSeirlaAndFileName(string systemSerial, string fileName) {
			var customerOrders = new CustomerOrders(_connectionString);
			return customerOrders.GetNtsOrderIdBySystemSerialAndFileName(systemSerial, fileName);
		}

    }
}
