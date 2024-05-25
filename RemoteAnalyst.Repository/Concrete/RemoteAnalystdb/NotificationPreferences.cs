using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class NotificationPreferences {
        private readonly string _connectionString = "";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"> Connection string that point to RemoteAnalystdb</param>
        public NotificationPreferences(string connectionString) {
            _connectionString = connectionString;
        }

        public DataTable CheckIsEveryLoad(string systemSerial, int customerID) {
            var notification = new DataTable();

            string cmdText = @"SELECT IsEveryLoad, IsEveryHour, IsDaily, IsEmailCritical, IsEmailMajor 
                                FROM NotificationPreferences
                                WHERE SystemSerial = @SystemSerial AND CustomerID = @CustomerID";

            //Get last date from DailySysUnrated.
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@CustomerID", customerID);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(notification);
            }
            return notification;
        }

        public bool CheckIsDailyLoad(string systemSerial, int customerID) {
            bool exists = false;

            string cmdText = @"SELECT IsDaily 
                                FROM NotificationPreferences
                                WHERE SystemSerial = @SystemSerial AND CustomerID = @CustomerID";

            //Get last date from DailySysUnrated.
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@CustomerID", customerID);
                connection.Open();

                var reader = command.ExecuteReader();
                if (reader.Read())
                    exists = true;
            }
            return exists;
        }

        public DataTable GetEveryHourSystems() {
            var notification = new DataTable();

            string cmdText = @"SELECT N.NotificationID, SystemSerial, C.CustomerID, EveryHour, StartHour, email
                                FROM NotificationPreferences AS N
                                INNER JOIN NotificationPreferenceHours AS H ON N.NotificationID = H.NotificationID
                                INNER JOIN CusAnalyst AS C ON C.CustomerID = N.CustomerID
                                WHERE IsEveryHour = 1";

            //Get last date from DailySysUnrated.
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(notification);
            }
            return notification;
        }

        public DataTable GetEveryDailySystems() {
            var notification = new DataTable();

            string cmdText = @"SELECT N.NotificationID, SystemSerial, C.CustomerID, SendHour, 
                            IsPreviousDay, IsLastHour, LastHour, Email
                            FROM NotificationPreferences AS N
                            INNER JOIN NotificationPreferenceDailies AS D ON N.NotificationID = D.NotificationID
                            INNER JOIN CusAnalyst AS C ON C.CustomerID = N.CustomerID
                            WHERE IsDaily = 1";

            //Get last date from DailySysUnrated.
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(notification);
            }
            return notification;
        }

        public DataTable GetEveryWeeklySystems() {
            var notification = new DataTable();

            string cmdText = @"SELECT N.NotificationID, SystemSerial, C.CustomerID, SendWeek, Email, 
                            WeekSendHour, LastWeek
                            FROM NotificationPreferences AS N
                            INNER JOIN NotificationPreferenceWeeklies AS W ON N.NotificationID = W.NotificationID
                            INNER JOIN CusAnalyst AS C ON C.CustomerID = N.CustomerID
                            WHERE IsWeekly = 1";

            //Get last date from DailySysUnrated.
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(notification);
            }
            return notification;
        }
    }
}