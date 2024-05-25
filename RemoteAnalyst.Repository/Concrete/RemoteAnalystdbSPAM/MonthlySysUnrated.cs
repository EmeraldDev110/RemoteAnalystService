using System;
using System.Data;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM
{
    public class MonthlySysUnrated
    {
        private readonly string _connectionString;

        public MonthlySysUnrated(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool CheckData(string systemSerial, DateTime date, int attributeId, string obj)
        {
            string cmdText = @"SELECT SystemSerialNum FROM MonthlySysUnrated WHERE SystemSerialNum = @SystemSerial AND 
                                DataMonth = @Date AND AttributeId = @AttributeId AND Object = @Object";
            bool exits = false;
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@Date", date);
                command.Parameters.AddWithValue("@AttributeId", attributeId);
                command.Parameters.AddWithValue("@Object", obj);

                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    exits = true;
                }
            }

            return exits;
        }

        public void InsertNewData(string systemSerial, DateTime date, int attributeId, string obj)
        {
            string cmdText = @"INSERT INTO MonthlySysUnrated(SystemSerialNum, AttributeId, DataMonth, Object) VALUES
                            (@SystemSerial, @AttributeId, @DataMonth,  @Object)";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@DataMonth", date);
                command.Parameters.AddWithValue("@AttributeId", attributeId);
                command.Parameters.AddWithValue("@Object", obj);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public DataTable GetHourlyData(string systemSerial, int attributeId, string obj, DateTime date)
        {
            string cmdText = @"SELECT `Hour0` ,`Hour1` ,`Hour2`  ,`Hour3` ,`Hour4` ,`Hour5` ,`Hour6`
                            ,`Hour7` ,`Hour8` ,`Hour9` ,`Hour10` ,`Hour11` ,`Hour12` ,`Hour13` ,`Hour14`
                            ,`Hour15` ,`Hour16` ,`Hour17` ,`Hour18` ,`Hour19` ,`Hour20` ,`Hour21` ,`Hour22` ,`Hour23` 
                            FROM MonthlySysUnrated WHERE SystemSerialNum = @SystemSerial AND DataMonth = @Date 
                            AND AttributeId = @AttributeId AND Object = @Object";

            var systemData = new DataTable();

            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@Date", date);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@AttributeId", attributeId);
                command.Parameters.AddWithValue("@Object", obj);

                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(systemData);
            }

            return systemData;
        }

        public void UpdateData(double sumval, double avgValue, int peakhour, int numhours, string systemSerial,
            int attributeId, string obj, DateTime date, string hourValue)
        {
            string cmdText = @"UPDATE MonthlySysUnrated SET SumVal = @SumVal, AvgVal = @AvgVal, 
                            PeakHour = @PeakHour, NumHours = @NumHours, " + hourValue + @"
                            WHERE SystemSerialNum = @SystemSerial AND DataMonth = @Date 
                            AND AttributeId = @AttributeId AND Object = @Object";
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SumVal", sumval);
                command.Parameters.AddWithValue("@AvgVal", avgValue);
                command.Parameters.AddWithValue("@PeakHour", peakhour);
                command.Parameters.AddWithValue("@NumHours", numhours);
                command.Parameters.AddWithValue("@Date", date);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@AttributeId", attributeId);
                command.Parameters.AddWithValue("@Object", obj);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}